using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    // ─── Enums & Data ────────────────────────────────

    public enum AnastasiaMode { Silent, ReactiveWhisper, Conversational, Invisible }

    public enum SolidificationPhase { NotTriggered, Glow, Shift, Solid, Return, Complete }

    public enum AnastasiaLineCategory
    {
        LoreWhisper,
        MemoryFragment,
        CompanionReaction,
        BuildingCommentary,
        StoryBeat,
        PersonalReflection,
        EasterEgg,
        TheFinalLine
    }

    [Serializable]
    public struct AnastasiaLine
    {
        public int id;                        // 0–111, maps to bitmask bit
        public int moon;                      // 1–13, 0 = DotT
        public AnastasiaLineCategory category;
        public string triggerContext;          // proximity tag or event key
        [TextArea(2, 4)]
        public string text;
    }

    // ─── ScriptableObject for line database ──────────

    [CreateAssetMenu(fileName = "AnastasiaDialogue", menuName = "Tartaria/Anastasia Dialogue Database")]
    public class AnastasiaDialogueDatabase : ScriptableObject
    {
        public AnastasiaLine[] lines = Array.Empty<AnastasiaLine>();
    }

    // ─── Main Controller ─────────────────────────────

    /// <summary>
    /// Princess Anastasia — Archive Echo companion controller.
    ///
    /// Implements the four-mode state machine (Silent / Reactive Whisper /
    /// Conversational / Invisible), 128-bit dialogue bitmask, silence-first
    /// delivery rules, golden mote tracking, and Day-Out-of-Time
    /// solidification sequence.
    ///
    /// Design doc: docs/18_PRINCESS_ANASTASIA.md
    /// </summary>
    [DisallowMultipleComponent]
    public class AnastasiaController : MonoBehaviour
    {
        public static AnastasiaController Instance { get; private set; }

        // ── Inspector ────────────────────────────────
        [Header("Dialogue")]
        [SerializeField] AnastasiaDialogueDatabase dialogueDatabase;
        [SerializeField] float whisperCooldown = 45f;
        [SerializeField] int sessionLineCap = 8;
        [SerializeField] int moonLineCap = 12;

        [Header("Follow Distances")]
        [SerializeField] float silentDistance = 6.5f;    // 5–8 m
        [SerializeField] float whisperDistance = 4f;     // 3–5 m
        [SerializeField] float conversationDistance = 2.5f; // 2–3 m
        [SerializeField] float followSpeed = 2f;
        [SerializeField] float hoverHeight = 0.03f;      // 2–3 cm above ground

        [Header("Mode Timings")]
        [SerializeField] float conversationIdleThreshold = 30f;
        [SerializeField] float whisperExitDelay = 30f;
        [SerializeField] float manifestDuration = 4f;     // 3–5 s

        [Header("Opacity")]
        [SerializeField] float silentOpacity = 0.30f;
        [SerializeField] float whisperOpacity = 0.50f;
        [SerializeField] float conversationalOpacity = 0.70f;
        [SerializeField] float opacityLerpSpeed = 2f;

        [Header("Golden Motes")]
        [SerializeField] int totalMotes = 13;

        // ── Runtime State ────────────────────────────
        AnastasiaMode _currentMode = AnastasiaMode.Invisible;
        AnastasiaMode _previousMode = AnastasiaMode.Silent;
        float _targetOpacity;
        float _currentOpacity;
        bool _hasManifested;
        bool _zoneFirstAppearanceDone;

        // Dialogue bitmask (128-bit as two ulongs)
        ulong _bitmaskLow;   // bits 0–63
        ulong _bitmaskHigh;  // bits 64–127

        float _lastLinetime = -999f;
        int _sessionLineCount;
        int _moonLineCount;
        int _currentMoon = 1;

        // Follow
        Transform _playerTransform;
        float _playerIdleTimer;
        float _whisperExitTimer;

        // Proximity
        Collider[] _proximityBuffer = new Collider[8];

        // Golden Motes (13-bit packed into ushort)
        ushort _motesCollected;

        // Solidification
        SolidificationPhase _solidPhase = SolidificationPhase.NotTriggered;
        bool _postSolidificationWarmGlow;

        // Line index for fast lookup
        readonly Dictionary<string, List<AnastasiaLine>> _linesByContext = new();
        readonly List<AnastasiaLine> _linesByMoon = new();

        // Priority weights (higher = higher priority)
        static readonly Dictionary<AnastasiaLineCategory, int> CategoryPriority = new()
        {
            { AnastasiaLineCategory.StoryBeat, 6 },
            { AnastasiaLineCategory.MemoryFragment, 5 },
            { AnastasiaLineCategory.LoreWhisper, 4 },
            { AnastasiaLineCategory.CompanionReaction, 3 },
            { AnastasiaLineCategory.BuildingCommentary, 2 },
            { AnastasiaLineCategory.PersonalReflection, 1 },
            { AnastasiaLineCategory.EasterEgg, 7 },
            { AnastasiaLineCategory.TheFinalLine, 8 },
        };

        // ── Events ───────────────────────────────────
        public event Action<AnastasiaMode, AnastasiaMode> OnModeChanged;
        public event Action<AnastasiaLine> OnLineDelivered;
        public event Action<int> OnMoteCollected;
        public event Action<SolidificationPhase> OnSolidificationPhaseChanged;

        // ── Lifecycle ────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildLineIndex();
        }

        void Start()
        {
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
            GameStateManager.Instance.OnStateChanged += OnGameStateChanged;

            // Start invisible — manifest after first dome restoration
            SetMode(AnastasiaMode.Invisible);
        }

        void OnDestroy()
        {
            GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        void Update()
        {
            if (!_hasManifested) return;
            if (_solidPhase == SolidificationPhase.Glow ||
                _solidPhase == SolidificationPhase.Shift ||
                _solidPhase == SolidificationPhase.Solid ||
                _solidPhase == SolidificationPhase.Return)
                return; // Solidification coroutine owns positioning

            UpdateModeTransitions();
            UpdateFollow();
            UpdateOpacity();
        }

        // ─── Mode State Machine ─────────────────────

        void UpdateModeTransitions()
        {
            // Priority 1: Combat → Invisible
            if (GameStateManager.Instance.CurrentState == GameState.Combat)
            {
                if (_currentMode != AnastasiaMode.Invisible)
                    SetMode(AnastasiaMode.Invisible);
                return;
            }

            // If returning from Invisible (combat ended)
            if (_currentMode == AnastasiaMode.Invisible &&
                GameStateManager.Instance.CurrentState != GameState.Combat)
            {
                StartCoroutine(RemanifestCoroutine());
                return;
            }

            // Priority 2: Conversational requires safety + low player velocity
            if (_currentMode == AnastasiaMode.Silent || _currentMode == AnastasiaMode.ReactiveWhisper)
            {
                bool playerIdle = IsPlayerIdle();
                if (playerIdle)
                {
                    _playerIdleTimer += Time.deltaTime;
                    if (_playerIdleTimer >= conversationIdleThreshold &&
                        GameStateManager.Instance.CurrentState == GameState.Exploration)
                    {
                        SetMode(AnastasiaMode.Conversational);
                        _playerIdleTimer = 0f;
                        return;
                    }
                }
                else
                {
                    _playerIdleTimer = 0f;
                }
            }

            // Exit Conversational if player sprints or combat starts
            if (_currentMode == AnastasiaMode.Conversational)
            {
                if (!IsPlayerIdle() ||
                    GameStateManager.Instance.CurrentState != GameState.Exploration)
                {
                    SetMode(AnastasiaMode.Silent);
                    return;
                }
            }

            // Priority 3: Reactive Whisper on lore proximity
            if (_currentMode == AnastasiaMode.Silent && IsNearLoreTrigger())
            {
                if (Time.time - _lastLinetime >= whisperCooldown)
                {
                    SetMode(AnastasiaMode.ReactiveWhisper);
                    _whisperExitTimer = whisperExitDelay;
                    return;
                }
            }

            // Whisper exit timer
            if (_currentMode == AnastasiaMode.ReactiveWhisper)
            {
                _whisperExitTimer -= Time.deltaTime;
                if (_whisperExitTimer <= 0f)
                {
                    SetMode(AnastasiaMode.Silent);
                }
            }
        }

        void SetMode(AnastasiaMode newMode)
        {
            if (newMode == _currentMode) return;

            _previousMode = _currentMode;
            _currentMode = newMode;

            _targetOpacity = newMode switch
            {
                AnastasiaMode.Silent => silentOpacity,
                AnastasiaMode.ReactiveWhisper => whisperOpacity,
                AnastasiaMode.Conversational => conversationalOpacity,
                AnastasiaMode.Invisible => 0f,
                _ => 0f
            };

            OnModeChanged?.Invoke(_previousMode, newMode);
            Debug.Log($"[Anastasia] Mode: {_previousMode} -> {newMode}");
        }

        // ─── Follow Behaviour ────────────────────────

        void UpdateFollow()
        {
            if (_playerTransform == null) return;
            if (_currentMode == AnastasiaMode.Invisible) return;

            float targetDist = _currentMode switch
            {
                AnastasiaMode.Silent => silentDistance,
                AnastasiaMode.ReactiveWhisper => whisperDistance,
                AnastasiaMode.Conversational => conversationDistance,
                _ => silentDistance
            };

            Vector3 playerPos = _playerTransform.position;
            Vector3 toPlayer = playerPos - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (dist > targetDist + 0.5f)
            {
                Vector3 targetPos = playerPos - toPlayer.normalized * targetDist;
                targetPos.y = playerPos.y + hoverHeight;
                transform.position = Vector3.Lerp(transform.position, targetPos,
                    followSpeed * Time.deltaTime);
            }
            else
            {
                // Maintain hover height
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, playerPos.y + hoverHeight, 2f * Time.deltaTime);
                transform.position = pos;
            }

            // Face toward player (gentle rotation)
            if (toPlayer.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 2f * Time.deltaTime);
            }
        }

        // ─── Opacity ─────────────────────────────────

        void UpdateOpacity()
        {
            _currentOpacity = Mathf.Lerp(_currentOpacity, _targetOpacity,
                opacityLerpSpeed * Time.deltaTime);

            // Apply to renderers (particle system alpha, mesh alpha, etc.)
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.material.HasProperty("_BaseColor"))
                {
                    Color c = r.material.GetColor("_BaseColor");
                    c.a = _currentOpacity;
                    r.material.SetColor("_BaseColor", c);
                }
                else if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.GetColor("_Color");
                    c.a = _currentOpacity;
                    r.material.SetColor("_Color", c);
                }
            }
        }

        // ─── Proximity Detection ─────────────────────

        bool IsNearLoreTrigger()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, whisperDistance + 2f, _proximityBuffer,
                LayerMask.GetMask("Interactable", "Lore"));
            return count > 0;
        }

        bool IsPlayerIdle()
        {
            if (_playerTransform == null) return false;
            // Check velocity via Rigidbody if available, otherwise position delta
            var rb = _playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
                return rb.linearVelocity.magnitude < 1.5f;
            return true; // Conservative: assume idle if no rigidbody
        }

        // ─── Manifestation ───────────────────────────

        /// <summary>
        /// Called by GameLoopController when the first dome is restored.
        /// Anastasia does NOT appear during restoration — only afterward.
        /// </summary>
        public void TriggerFirstManifestation()
        {
            if (_hasManifested) return;
            StartCoroutine(FirstManifestCoroutine());
        }

        IEnumerator FirstManifestCoroutine()
        {
            // Wait a beat after restoration settles
            yield return new WaitForSeconds(2f);

            _hasManifested = true;

            // Position 10 meters from player
            if (_playerTransform != null)
            {
                Vector3 spawnPos = _playerTransform.position +
                    _playerTransform.forward * 10f;
                spawnPos.y += hoverHeight;
                transform.position = spawnPos;
            }

            // Coalesce particles over manifestDuration
            _currentOpacity = 0f;
            SetMode(AnastasiaMode.Silent);

            float elapsed = 0f;
            while (elapsed < manifestDuration)
            {
                elapsed += Time.deltaTime;
                _currentOpacity = Mathf.Lerp(0f, silentOpacity, elapsed / manifestDuration);
                yield return null;
            }

            // She looks at the dome, not the player — then turns briefly
            yield return new WaitForSeconds(2f);

            // Drift backward if player approaches (Silent mode default)
            _zoneFirstAppearanceDone = true;
            Debug.Log("[Anastasia] First manifestation complete — Silent mode active.");
        }

        IEnumerator RemanifestCoroutine()
        {
            // Return after combat: 3s particle coalesce
            SetMode(AnastasiaMode.Silent);
            _currentOpacity = 0f;
            float elapsed = 0f;
            const float remanifestTime = 3f;
            while (elapsed < remanifestTime)
            {
                elapsed += Time.deltaTime;
                _currentOpacity = Mathf.Lerp(0f, silentOpacity, elapsed / remanifestTime);
                yield return null;
            }
        }

        // ─── Dialogue System ─────────────────────────

        void BuildLineIndex()
        {
            _linesByContext.Clear();
            _linesByMoon.Clear();

            if (dialogueDatabase == null) return;

            foreach (var line in dialogueDatabase.lines)
            {
                // Index by context tag
                if (!string.IsNullOrEmpty(line.triggerContext))
                {
                    if (!_linesByContext.TryGetValue(line.triggerContext, out var list))
                    {
                        list = new List<AnastasiaLine>();
                        _linesByContext[line.triggerContext] = list;
                    }
                    list.Add(line);
                }
                _linesByMoon.Add(line);
            }
        }

        /// <summary>
        /// Attempt to deliver a line for the given context trigger.
        /// Respects cooldown, session cap, moon cap, bitmask, and priority.
        /// </summary>
        public bool TryDeliverLine(string context)
        {
            if (!_hasManifested) return false;
            if (_currentMode == AnastasiaMode.Invisible) return false;
            if (Time.time - _lastLinetime < whisperCooldown) return false;
            if (_sessionLineCount >= sessionLineCap) return false;
            if (_moonLineCount >= moonLineCap) return false;

            if (!_linesByContext.TryGetValue(context, out var candidates))
                return false;

            // Filter: correct moon, not yet delivered
            AnastasiaLine? best = null;
            int bestPriority = -1;

            foreach (var line in candidates)
            {
                if (line.moon != _currentMoon && line.moon != 0) continue;
                if (IsLineDelivered(line.id)) continue;

                int prio = CategoryPriority.GetValueOrDefault(line.category, 0);
                if (prio > bestPriority)
                {
                    bestPriority = prio;
                    best = line;
                }
            }

            if (best == null) return false;

            DeliverLine(best.Value);
            return true;
        }

        /// <summary>
        /// Deliver a line by its exact ID (for scripted story triggers).
        /// Bypasses context matching but still respects bitmask.
        /// </summary>
        public bool TryDeliverLineById(int lineId)
        {
            if (!_hasManifested) return false;
            if (IsLineDelivered(lineId)) return false;

            if (dialogueDatabase == null) return false;
            foreach (var line in dialogueDatabase.lines)
            {
                if (line.id == lineId)
                {
                    DeliverLine(line);
                    return true;
                }
            }
            return false;
        }

        void DeliverLine(AnastasiaLine line)
        {
            MarkLineDelivered(line.id);
            _lastLinetime = Time.time;
            _sessionLineCount++;
            _moonLineCount++;

            // Show via UI with golden text styling
            UIManager.Instance?.ShowDialogue("Anastasia", line.text);

            // Auto-close after duration proportional to text length
            float displayTime = Mathf.Max(4f, line.text.Length * 0.06f);
            CancelInvoke(nameof(HideAnastasiaDialogue));
            Invoke(nameof(HideAnastasiaDialogue), displayTime);

            // Haptic pulse for whisper
            if (_currentMode == AnastasiaMode.ReactiveWhisper ||
                _currentMode == AnastasiaMode.Conversational)
            {
                HapticFeedbackManager.Instance?.PlayDiscovery(); // gentle pulse
            }

            OnLineDelivered?.Invoke(line);
            Debug.Log($"[Anastasia] Line {line.id}: {line.text}");
        }

        void HideAnastasiaDialogue()
        {
            UIManager.Instance?.HideDialogue();
        }

        // ─── 128-bit Bitmask ─────────────────────────

        bool IsLineDelivered(int id)
        {
            if (id < 0 || id > 127) return true;
            if (id < 64)
                return (_bitmaskLow & (1UL << id)) != 0;
            return (_bitmaskHigh & (1UL << (id - 64))) != 0;
        }

        void MarkLineDelivered(int id)
        {
            if (id < 0 || id > 127) return;
            if (id < 64)
                _bitmaskLow |= (1UL << id);
            else
                _bitmaskHigh |= (1UL << (id - 64));

            SaveManager.Instance?.MarkDirty();
        }

        public int GetDeliveredLineCount()
        {
            return CountBits(_bitmaskLow) + CountBits(_bitmaskHigh);
        }

        static int CountBits(ulong v)
        {
            int count = 0;
            while (v != 0) { count++; v &= v - 1; }
            return count;
        }

        // ─── Golden Motes ────────────────────────────

        /// <summary>
        /// Called when the player collects a golden mote in the specified zone.
        /// Shifts to Reactive Whisper and delivers the mote's Easter Egg line.
        /// </summary>
        public void CollectMote(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= totalMotes) return;
            if (IsMoteCollected(zoneIndex)) return;

            _motesCollected |= (ushort)(1 << zoneIndex);

            // Briefly shift to Reactive Whisper for the mote line
            if (_currentMode == AnastasiaMode.Silent)
                SetMode(AnastasiaMode.ReactiveWhisper);

            TryDeliverLine($"mote_{zoneIndex}");

            OnMoteCollected?.Invoke(zoneIndex);
            SaveManager.Instance?.MarkDirty();

            Debug.Log($"[Anastasia] Golden Mote collected: zone {zoneIndex} ({GetMotesCollectedCount()}/{totalMotes})");
        }

        public bool IsMoteCollected(int zoneIndex)
        {
            return (zoneIndex >= 0 && zoneIndex < totalMotes) &&
                   (_motesCollected & (1 << zoneIndex)) != 0;
        }

        public int GetMotesCollectedCount()
        {
            int count = 0;
            ushort v = _motesCollected;
            while (v != 0) { count++; v &= (ushort)(v - 1); }
            return count;
        }

        public bool AllMotesCollected => GetMotesCollectedCount() >= totalMotes;

        // ─── Solidification (Day Out of Time) ────────

        /// <summary>
        /// Trigger the 10-second solidification sequence.
        /// Called by the Day Out of Time event controller at harmonic peak.
        /// </summary>
        public void TriggerSolidification()
        {
            if (_solidPhase != SolidificationPhase.NotTriggered) return;
            StartCoroutine(SolidificationCoroutine());
        }

        IEnumerator SolidificationCoroutine()
        {
            // Phase 1 — The Glow (0:00–3:00)
            SetSolidPhase(SolidificationPhase.Glow);
            float elapsed = 0f;
            while (elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                // Particles contract, gold deepens
                _currentOpacity = Mathf.Lerp(conversationalOpacity, 0.9f, elapsed / 3f);
                yield return null;
            }

            // Phase 2 — The Shift (3:00–5:00)
            SetSolidPhase(SolidificationPhase.Shift);
            elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                _currentOpacity = Mathf.Lerp(0.9f, 1f, elapsed / 2f);
                yield return null;
            }

            // Phase 3 — The Moment (10 real seconds of solid form)
            SetSolidPhase(SolidificationPhase.Solid);

            // Ground the feet — remove hover
            Vector3 pos = transform.position;
            pos.y -= hoverHeight;
            transform.position = pos;

            // Play the footstep — the most important sound in the game
            // AudioSource.PlayClipAtPoint(solidFootstep, transform.position);

            // Deliver The Final Line
            TryDeliverLine("solidification_final");

            yield return new WaitForSeconds(10f);

            // Phase 4 — The Return (5 seconds)
            SetSolidPhase(SolidificationPhase.Return);
            elapsed = 0f;
            while (elapsed < 5f)
            {
                elapsed += Time.deltaTime;
                _currentOpacity = Mathf.Lerp(1f, silentOpacity, elapsed / 5f);
                yield return null;
            }

            // She is an echo again — but warmer
            _postSolidificationWarmGlow = true;
            SetSolidPhase(SolidificationPhase.Complete);
            SetMode(AnastasiaMode.Silent);

            // Post-solidification whisper
            TryDeliverLine("post_solidification");

            Debug.Log("[Anastasia] Solidification complete. Warm glow active.");
        }

        void SetSolidPhase(SolidificationPhase phase)
        {
            _solidPhase = phase;
            OnSolidificationPhaseChanged?.Invoke(phase);
            Debug.Log($"[Anastasia] Solidification phase: {phase}");
        }

        // ─── GameState Integration ───────────────────

        void OnGameStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Combat)
                SetMode(AnastasiaMode.Invisible);
        }

        // ─── Moon Progression ────────────────────────

        public void SetCurrentMoon(int moon)
        {
            if (moon == _currentMoon) return;
            _currentMoon = Mathf.Clamp(moon, 1, 13);
            _moonLineCount = 0;
            Debug.Log($"[Anastasia] Moon set to {_currentMoon}");
        }

        public void ResetSessionLineCount()
        {
            _sessionLineCount = 0;
        }

        // ─── Save / Load ────────────────────────────

        [Serializable]
        public struct AnastasiaSaveData
        {
            public ulong bitmaskLow;
            public ulong bitmaskHigh;
            public ushort motesCollected;
            public int currentMoon;
            public bool hasManifested;
            public bool postSolidWarmGlow;
            public int solidPhase;
        }

        public AnastasiaSaveData GetSaveData()
        {
            return new AnastasiaSaveData
            {
                bitmaskLow = _bitmaskLow,
                bitmaskHigh = _bitmaskHigh,
                motesCollected = _motesCollected,
                currentMoon = _currentMoon,
                hasManifested = _hasManifested,
                postSolidWarmGlow = _postSolidificationWarmGlow,
                solidPhase = (int)_solidPhase
            };
        }

        public void RestoreFromSave(AnastasiaSaveData data)
        {
            _bitmaskLow = data.bitmaskLow;
            _bitmaskHigh = data.bitmaskHigh;
            _motesCollected = data.motesCollected;
            _currentMoon = data.currentMoon;
            _hasManifested = data.hasManifested;
            _postSolidificationWarmGlow = data.postSolidWarmGlow;
            _solidPhase = (SolidificationPhase)data.solidPhase;

            if (_hasManifested && _solidPhase == SolidificationPhase.NotTriggered)
                SetMode(AnastasiaMode.Silent);
        }

        // ─── Public Queries ──────────────────────────

        public AnastasiaMode CurrentMode => _currentMode;
        public float CurrentOpacity => _currentOpacity;
        public bool HasManifested => _hasManifested;
        public bool IsPostSolidificationWarm => _postSolidificationWarmGlow;
        public SolidificationPhase CurrentSolidPhase => _solidPhase;
        public int CurrentMoon => _currentMoon;

        /// <summary>
        /// Bitmask as hex string for debug/codex display.
        /// </summary>
        public string GetBitmaskHex()
        {
            return $"0x{_bitmaskHigh:X16}{_bitmaskLow:X16}";
        }
    }
}
