using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Cymatic Water Tuning Mini-Game — Echohaven Fountain (Moon 1 vertical slice).
    ///
    /// Design sources (do NOT "fix" without re-reading):
    ///   - docs/02_AETHER_ENERGY_SYSTEM.md §6.2 — "Cymatic Garden Puzzle":
    ///     standing-wave pattern matching on a sacred-geometry grid; water flows
    ///     through channels; correct placement → ripples form geometric shapes;
    ///     perfect match → ionized golden mist; miss → muddy water + golems.
    ///   - docs/15_MVP_BUILD_SPEC.md §9 — Frequency slider, target band, 432 Hz,
    ///     accuracy tiers (Perfect ≥95 / Great ≥80 / Good ≥60 / Fail), RS multiplier.
    ///   - docs/15_MVP_BUILD_SPEC.md §6 Fountain entry — "When active, water carries
    ///     harmonic sound — the first hint of 432 Hz".
    ///   - Sprint 11 L1 (origin agent/audit/stub-sweep 1fb03541) — flagged 7 empty
    ///     method bodies on this class. This file replaces every stub with a real
    ///     implementation that drives a particle-based cymatic water visual,
    ///     reads Input System input, fires GameEvents.RaiseBuildingRestored on
    ///     success, and persists Gold-tier permanent visuals via LoadSaveData.
    ///
    /// Wired into InteractableBuilding.TryStartBuildingMiniGame (case "fountain")
    /// by Sprint 8 L4. EnsureMiniGameComponent&lt;T&gt; finds-or-adds this MonoBehaviour
    /// onto the "MiniGameHost" GameObject in the scene.
    /// </summary>
    public class CymaticWaterTuningMiniGame : MonoBehaviour
    {
        public static CymaticWaterTuningMiniGame Instance { get; private set; }

        // ─── Config ──────────────────────────────────────────────────────
        [Header("Frequency Tuning")]
        [Tooltip("Target frequency the player must match (Hz). 432 Hz per docs/15 §9.")]
        [SerializeField] private float targetFrequency = 432f;
        [Tooltip("Half-width of the success band in Hz (±). 8 Hz per docs/15 ±8% at slow node.")]
        [SerializeField] private float tolerance = 8f;
        [Tooltip("Slider min frequency.")]
        [SerializeField] private float minFrequency = 332f;
        [Tooltip("Slider max frequency.")]
        [SerializeField] private float maxFrequency = 532f;

        [Header("Run Settings")]
        public float timeLimit = 45f;
        [Tooltip("0=easy, 1=medium, 2=hard, 3=expert.")]
        public int difficulty = 1;
        [Tooltip("Base RS reward at 1.0 accuracy; final = base * multiplier * accuracy.")]
        [SerializeField] private float baseRSReward = 12f;
        [Tooltip("Building id used when firing GameEvents.RaiseBuildingRestored.")]
        [SerializeField] private string buildingId = "fountain";

        [Header("Cymatic Visual")]
        [Tooltip("Min particle emission rate (off-frequency).")]
        [SerializeField] private float minEmissionRate = 4f;
        [Tooltip("Max particle emission rate (perfect-frequency).")]
        [SerializeField] private float maxEmissionRate = 120f;
        [Tooltip("Color when far off frequency (mud).")]
        [SerializeField] private Color colorOff = new Color(0.45f, 0.35f, 0.20f, 0.85f);
        [Tooltip("Color when at target (gold).")]
        [SerializeField] private Color colorOn  = new Color(0.95f, 0.78f, 0.20f, 1f);

        // ─── Public events ──────────────────────────────────────────────
        public event Action<float> OnFrequencyChanged;      // current Hz
        public event Action<float> OnAccuracyChanged;       // 0..1
        public event Action<float> OnTuningComplete;        // final accuracy
        public event Action OnTuningFailed;

        public bool IsActive => _active;
        public float CurrentAccuracy => _bestAccuracy;
        public float CurrentFrequency => _currentFreq;
        public string CurrentTier => TuningMiniGame.GetAccuracyTier(_bestAccuracy);

        // ─── State ──────────────────────────────────────────────────────
        float _bestAccuracy = 0f;
        float _runningAccuracy = 0f;
        int _completions = 0;
        bool _goldTierForFountain;
        bool _permanentEffectsActive;
        bool _active;
        float _timeRemaining;
        float _currentFreq;
        string _lastReportedTier = "Fail";

        // ─── UI ─────────────────────────────────────────────────────────
        static Canvas _sharedCanvas;
        static Sprite _whiteSprite;
        GameObject _panel;
        Slider _slider;
        Text _statusText;
        Text _currentFreqText;
        Image _meter;

        // ─── Cymatic visual (runtime particle system) ──────────────────
        ParticleSystem _cymaticPS;            // session ripples
        ParticleSystem _permanentCymaticPS;   // post-restoration permanent ripples
        Transform _fountainTransform;         // resolved on first need

        // ─── Crystal pulse ──────────────────────────────────────────────
        readonly List<Renderer> _fountainCrystals = new();
        float _lastPulseTime = -10f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _currentFreq = targetFrequency; // before any drift on start, sane default
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ═════════════════════════════════════════════════════════════════
        // Public API — InteractableBuilding.TryStartBuildingMiniGame call site
        // ═════════════════════════════════════════════════════════════════
        public void StartMiniGame(float customTime = -1f)
        {
            if (_active)
            {
                Debug.LogWarning("[CymaticWater] StartMiniGame called while already active — ignoring.");
                return;
            }

            _timeRemaining = customTime > 0f ? customTime : timeLimit;
            _runningAccuracy = 0f;
            _lastReportedTier = "Fail";

            // Start the slider somewhere away from the target so the player has work to do.
            // Magnitude scales with difficulty (0=±30Hz, 3=±90Hz).
            float offsetMag = Mathf.Lerp(30f, 90f, Mathf.Clamp01(difficulty / 3f));
            float sign = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
            _currentFreq = Mathf.Clamp(
                targetFrequency + sign * offsetMag + UnityEngine.Random.Range(-10f, 10f),
                minFrequency, maxFrequency);

            EnsureUIBuilt();
            if (_panel != null) _panel.SetActive(true);
            if (_slider != null)
            {
                _slider.minValue = minFrequency;
                _slider.maxValue = maxFrequency;
                _slider.value = _currentFreq;
            }
            if (_statusText != null) _statusText.text = $"Tune the water to {targetFrequency:F0} Hz";

            EnsureCymaticPS();           // session-scoped visuals

            GameStateManager.Instance?.TransitionTo(GameState.Tuning);
            _active = true;
            Debug.Log($"[CymaticWater] Started — target={targetFrequency:F0}Hz tol=±{tolerance:F1} time={_timeRemaining:F1}s diff={difficulty}");
        }

        public void EndMiniGame(bool success)
        {
            _active = false;
            if (success) _completions++;
            if (_cymaticPS != null) _cymaticPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (_panel != null) _panel.SetActive(false);
            GameStateManager.Instance?.ReturnToPrevious();
        }

        /// <summary>Event hook — kept for backward compat with subscribers that
        /// listen on an "OnTuningInput" surface. Updates current Hz directly.</summary>
        public void OnTuningInput(float freq, float amp)
        {
            // amp is ignored in this variant; frequency drives both audio + visuals.
            _currentFreq = Mathf.Clamp(freq, minFrequency, maxFrequency);
            if (_slider != null) _slider.value = _currentFreq;
            OnFrequencyChanged?.Invoke(_currentFreq);
            UpdateCymaticPattern();
        }

        public float GetCurrentAccuracy() => _bestAccuracy;

        public void ForceFullCymaticVisualReapply() { EnsurePermanentCymaticVisuals(); }

        // ═════════════════════════════════════════════════════════════════
        // Save data
        // ═════════════════════════════════════════════════════════════════
        [System.Serializable]
        public class CymaticSaveData
        {
            public float bestCymaticAccuracy;
            public int cymaticCompletions;
            public bool goldTierUnlockedForFountain;
            public bool permanentEffectsActive;
        }

        public CymaticSaveData GetSaveData()
        {
            return new CymaticSaveData
            {
                bestCymaticAccuracy = _bestAccuracy,
                cymaticCompletions = _completions,
                goldTierUnlockedForFountain = _goldTierForFountain,
                permanentEffectsActive = _permanentEffectsActive
            };
        }

        public void LoadSaveData(CymaticSaveData data)
        {
            if (data == null) return;
            _bestAccuracy = data.bestCymaticAccuracy;
            _completions = data.cymaticCompletions;
            _goldTierForFountain = data.goldTierUnlockedForFountain;
            _permanentEffectsActive = data.permanentEffectsActive;
            EnsurePermanentCymaticVisuals();
        }

        // ═════════════════════════════════════════════════════════════════
        // Update loop
        // ═════════════════════════════════════════════════════════════════
        void Update()
        {
            if (!_active) return;

            _timeRemaining -= Time.unscaledDeltaTime;
            if (_timeRemaining <= 0f)
            {
                FinishCymatic();
                return;
            }

            HandleInput();
            UpdateAccuracy();
            UpdateCymaticPattern();
        }

        // ═════════════════════════════════════════════════════════════════
        // HandleInput — Logitech F310 / Keyboard / Mouse wheel via Input System
        // ═════════════════════════════════════════════════════════════════
        void HandleInput()
        {
            float nudge = 0f;
            const float nudgeRate = 80f; // Hz/sec when held

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) nudge -= nudgeRate * Time.unscaledDeltaTime;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) nudge += nudgeRate * Time.unscaledDeltaTime;

                // Snap-to-target on Space (debug / accessibility help)
                if (kb.spaceKey.wasPressedThisFrame)
                {
                    _currentFreq = targetFrequency;
                }
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                // Left stick analog
                float sx = pad.leftStick.ReadValue().x;
                if (Mathf.Abs(sx) > 0.15f) nudge += sx * nudgeRate * Time.unscaledDeltaTime;

                // D-Pad ←/→ per F310 mapping in CLAUDE.md (Frequency adjust)
                if (pad.dpad.left.isPressed)  nudge -= nudgeRate * Time.unscaledDeltaTime;
                if (pad.dpad.right.isPressed) nudge += nudgeRate * Time.unscaledDeltaTime;
            }

            if (Mathf.Abs(nudge) > 0.0001f)
            {
                _currentFreq = Mathf.Clamp(_currentFreq + nudge, minFrequency, maxFrequency);
                if (_slider != null) _slider.value = _currentFreq;
                OnFrequencyChanged?.Invoke(_currentFreq);
                GameEvents.FireTuningProgress(Mathf.Abs(_currentFreq - targetFrequency));
            }

            // Cancel — Start button or Escape
            if ((pad != null && pad.startButton.wasPressedThisFrame) ||
                (kb  != null && kb.escapeKey.wasPressedThisFrame))
            {
                Debug.Log("[CymaticWater] Player cancelled — failing run.");
                FailCymatic("Cancelled by player");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // UpdateAccuracy — compute & track best-of-run accuracy
        // ═════════════════════════════════════════════════════════════════
        void UpdateAccuracy()
        {
            float distance = Mathf.Abs(_currentFreq - targetFrequency);
            // Accuracy is 1.0 within tolerance band, 0.0 at ±100 Hz off, linear between.
            float withinBand = distance <= tolerance ? 1f
                                                    : Mathf.Clamp01(1f - (distance - tolerance) / 100f);
            _runningAccuracy = withinBand;
            if (withinBand > _bestAccuracy)
            {
                _bestAccuracy = withinBand;
                OnAccuracyChanged?.Invoke(_bestAccuracy);

                string tier = TuningMiniGame.GetAccuracyTier(_bestAccuracy);
                if (tier != _lastReportedTier)
                {
                    _lastReportedTier = tier;
                    // Pulse the fountain crystals each time we cross a tier boundary upward.
                    PulseFountainCrystals(_bestAccuracy);
                    Debug.Log($"[CymaticWater] Tier upgrade: {tier} ({_bestAccuracy:P0})");
                }
            }

            // Live HUD feedback colour on the meter bar
            if (_meter != null)
                _meter.color = Color.Lerp(Color.red, new Color(0.95f, 0.78f, 0.20f), withinBand);

            if (_currentFreqText != null)
                _currentFreqText.text = $"{_currentFreq:F1} Hz  ({withinBand:P0})";

            // Auto-complete when we hold inside the band for a moment.
            // Use a tiny dwell so a fast brush-through doesn't trigger a fluke success.
            if (withinBand >= 1f)
            {
                _dwellInsideBand += Time.unscaledDeltaTime;
                if (_dwellInsideBand >= 0.6f)
                {
                    FinishCymatic();
                }
            }
            else
            {
                _dwellInsideBand = 0f;
            }
        }
        float _dwellInsideBand = 0f;

        // ═════════════════════════════════════════════════════════════════
        // UpdateCymaticPattern — drive particle emission + tint from freq
        // ═════════════════════════════════════════════════════════════════
        void UpdateCymaticPattern()
        {
            if (_cymaticPS == null) return;

            float distance = Mathf.Abs(_currentFreq - targetFrequency);
            float band = distance <= tolerance ? 1f
                                              : Mathf.Clamp01(1f - (distance - tolerance) / 100f);

            // Emission scales with proximity to target
            var emission = _cymaticPS.emission;
            emission.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, band);

            // Tint
            var main = _cymaticPS.main;
            main.startColor = Color.Lerp(colorOff, colorOn, band);

            // Shape ring radius pulses with frequency (visual "cymatic ring" widening as
            // the frequency rises through the audible band — gives the player a clear
            // proprioceptive cue).
            var shape = _cymaticPS.shape;
            shape.radius = Mathf.Lerp(0.6f, 2.2f, Mathf.InverseLerp(minFrequency, maxFrequency, _currentFreq));

            // Audible tone follows the slider so the player hears what they tune.
            // Quantize to 4 Hz so we don't spam PlayTone every frame.
            int quant = Mathf.RoundToInt(_currentFreq / 4f) * 4;
            if (quant != _lastToneHz)
            {
                _lastToneHz = quant;
                try
                {
                    AudioManager.Instance?.PlayTone(quant, 0.25f, 0.15f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CymaticWater] PlayTone({quant}) threw {e.GetType().Name}: {e.Message}");
                }

                // Haptic: on/off frequency feedback
                try
                {
                    if (band >= 0.95f)      HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
                    else if (band <= 0.2f)  HapticFeedbackManager.Instance?.PlayTuningOffFrequency();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CymaticWater] Haptic call threw {e.GetType().Name}: {e.Message}");
                }
            }
        }
        int _lastToneHz = -1;

        // ═════════════════════════════════════════════════════════════════
        // EnsurePermanentCymaticVisuals — post-restoration permanent ripples
        // ═════════════════════════════════════════════════════════════════
        public void EnsurePermanentCymaticVisuals()
        {
            if (!_permanentEffectsActive) return;
            if (_permanentCymaticPS != null) return; // already spawned

            var fountain = ResolveFountainTransform();
            if (fountain == null)
            {
                Debug.LogWarning("[CymaticWater] EnsurePermanentCymaticVisuals: no fountain transform " +
                                 "found in scene (looked for Building_fountain / Echohaven_HarmonicFountain / " +
                                 "HarmonicFountain / Fountain). Permanent visuals deferred.");
                return;
            }

            var go = new GameObject("Cymatic_Permanent_Ripples");
            go.transform.SetParent(fountain, false);
            go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // flat on water

            _permanentCymaticPS = go.AddComponent<ParticleSystem>();
            ConfigureCymaticPS(_permanentCymaticPS,
                emission: maxEmissionRate * 0.6f,
                startSize: 0.4f,
                lifetime: 3f,
                tint: colorOn,
                shapeRadius: 1.4f);
            _permanentCymaticPS.Play();

            Debug.Log($"[CymaticWater] Permanent cymatic ripples instantiated under '{fountain.name}'.");
        }

        // ═════════════════════════════════════════════════════════════════
        // PulseFountainCrystals — flash crystal renderers on tier upgrade
        // ═════════════════════════════════════════════════════════════════
        void PulseFountainCrystals(float strength)
        {
            // Throttle — at most 1 pulse per 0.4s.
            if (Time.unscaledTime - _lastPulseTime < 0.4f) return;
            _lastPulseTime = Time.unscaledTime;

            CacheFountainCrystals();

            if (_fountainCrystals.Count == 0)
            {
                // No crystal mesh present — still emit a one-shot burst from the cymatic PS
                // so the player gets feedback. NOT a silent fail.
                if (_cymaticPS != null)
                {
                    var burstParams = new ParticleSystem.EmitParams
                    {
                        startColor = Color.Lerp(colorOff, colorOn, strength),
                        startSize  = 0.8f,
                        startLifetime = 1.2f
                    };
                    _cymaticPS.Emit(burstParams, Mathf.RoundToInt(20f * strength + 5f));
                }
                Debug.Log($"[CymaticWater] PulseFountainCrystals: no crystal renderers; emitted burst instead. strength={strength:F2}");
                return;
            }

            foreach (var r in _fountainCrystals)
            {
                if (r == null) continue;
                // Use a property block so we don't leak material instances.
                var block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                Color emission = Color.Lerp(Color.black, colorOn, strength) * Mathf.Lerp(0.6f, 3.0f, strength);
                block.SetColor("_EmissionColor", emission);
                block.SetColor("_BaseColor", Color.Lerp(new Color(0.4f, 0.5f, 0.7f), colorOn, strength * 0.5f));
                r.SetPropertyBlock(block);
            }

            // VFX hook (visible from across zone)
            try
            {
                var fountain = ResolveFountainTransform();
                if (fountain != null)
                {
                    ServiceLocator.VFX?.PlayResonancePulse(fountain.position + Vector3.up * 0.5f, 2.5f * strength);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CymaticWater] VFX pulse threw {e.GetType().Name}: {e.Message}");
            }
        }

        void CacheFountainCrystals()
        {
            if (_fountainCrystals.Count > 0) return;
            var fountain = ResolveFountainTransform();
            if (fountain == null) return;

            foreach (var r in fountain.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                string n = r.gameObject.name.ToLowerInvariant();
                if (n.Contains("crystal") || n.Contains("gem") || n.Contains("shard"))
                {
                    _fountainCrystals.Add(r);
                }
            }
            if (_fountainCrystals.Count == 0)
            {
                Debug.Log($"[CymaticWater] CacheFountainCrystals: 0 crystal-named renderers under '{fountain.name}'.");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // FinishCymatic — success path (Building Restored)
        // ═════════════════════════════════════════════════════════════════
        void FinishCymatic()
        {
            if (!_active) return;
            _active = false;

            string tier = TuningMiniGame.GetAccuracyTier(_bestAccuracy);
            Debug.Log($"[CymaticWater] Finished — accuracy={_bestAccuracy:P0} tier={tier}");

            if (_bestAccuracy < 0.60f)
            {
                FailCymatic($"Accuracy below threshold ({_bestAccuracy:P0})");
                return;
            }

            _completions++;
            if (_bestAccuracy >= 0.95f)
            {
                _goldTierForFountain = true;
                _permanentEffectsActive = true;
                EnsurePermanentCymaticVisuals();
            }

            // RS multiplier per docs/15 §9
            float multiplier = _bestAccuracy >= 0.95f ? 1.618f
                            : _bestAccuracy >= 0.80f ? 1.3f
                            :                          1.0f;
            float rsReward = baseRSReward * _bestAccuracy * multiplier;

            // Banner / RS / building event ─ each in its own try so a downstream
            // failure can't swallow the rest of the success path.
            try
            {
                ServiceLocator.HUD?.ShowBanner("TUNED!", $"{tier} — Fountain resonates ({rsReward:F0} RS)", 3f);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CymaticWater] ShowBanner threw {e.GetType().Name}: {e.Message}");
            }

            try
            {
                AetherFieldManager.Instance?.AddResonanceScore(rsReward);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CymaticWater] AddResonanceScore threw {e.GetType().Name}: {e.Message}");
            }

            try
            {
                ServiceLocator.GameLoop?.OnMiniGameCompleted(rsReward, "CymaticWater");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CymaticWater] GameLoop.OnMiniGameCompleted threw {e.GetType().Name}: {e.Message}");
            }

            try
            {
                HapticFeedbackManager.Instance?.PlayPerfectTune();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CymaticWater] PlayPerfectTune threw {e.GetType().Name}: {e.Message}");
            }

            // Fire BuildingRestored — confirmed publisher at
            // Assets/_Project/Scripts/Core/GameEvents.cs:465 (RaiseBuildingRestored),
            // which fires the typed event AND the legacy string event (line 471).
            var fountain = ResolveFountainTransform();
            try
            {
                GameEvents.RaiseBuildingRestored(new BuildingRestoredEventArgs
                {
                    buildingId    = buildingId,
                    rsReward      = Mathf.RoundToInt(rsReward),
                    position      = fountain != null ? fountain.position : transform.position,
                    tuningAccuracy = _bestAccuracy,
                    Building      = fountain != null ? fountain.gameObject : null
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[CymaticWater] RaiseBuildingRestored threw {e.GetType().Name}: {e.Message}");
            }

            OnTuningComplete?.Invoke(_bestAccuracy);

            if (_cymaticPS != null)
            {
                _cymaticPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (_panel != null) _panel.SetActive(false);

            GameStateManager.Instance?.ReturnToPrevious();

            try
            {
                Tartaria.Save.SaveManager.Instance?.MarkDirty();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CymaticWater] SaveManager.MarkDirty threw {e.GetType().Name}: {e.Message}");
            }
        }

        void FailCymatic(string reason)
        {
            _active = false;
            Debug.Log($"[CymaticWater] FAILED — {reason}");

            try { ServiceLocator.HUD?.ShowBanner("FAILED", "Water muddied — try again", 3f); }
            catch (Exception e) { Debug.LogError($"[CymaticWater] ShowBanner(FAIL) threw {e.GetType().Name}: {e.Message}"); }

            OnTuningFailed?.Invoke();

            if (_cymaticPS != null)
            {
                _cymaticPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (_panel != null) _panel.SetActive(false);

            GameStateManager.Instance?.ReturnToPrevious();
        }

        // ═════════════════════════════════════════════════════════════════
        // Visual scaffolding helpers
        // ═════════════════════════════════════════════════════════════════
        Transform ResolveFountainTransform()
        {
            if (_fountainTransform != null) return _fountainTransform;

            string[] candidates =
            {
                "Building_fountain",
                "Echohaven_HarmonicFountain",
                "HarmonicFountain",
                "HarmonicFountain_Placeholder",
                "Fountain"
            };
            foreach (var n in candidates)
            {
                var go = GameObject.Find(n);
                if (go != null) { _fountainTransform = go.transform; return _fountainTransform; }
            }
            // Fallback: nearest object tagged appropriately (won't throw if tag missing).
            Debug.LogWarning("[CymaticWater] ResolveFountainTransform: none of " +
                             $"[{string.Join(", ", candidates)}] found in scene.");
            return null;
        }

        void EnsureCymaticPS()
        {
            if (_cymaticPS != null) return;

            var fountain = ResolveFountainTransform();
            var parent = fountain != null ? fountain : transform;

            var go = new GameObject("Cymatic_Session_Ripples");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // flat ring on water plane

            _cymaticPS = go.AddComponent<ParticleSystem>();
            ConfigureCymaticPS(_cymaticPS,
                emission: minEmissionRate,
                startSize: 0.25f,
                lifetime: 2.2f,
                tint: colorOff,
                shapeRadius: 0.8f);
            _cymaticPS.Play();
        }

        static void ConfigureCymaticPS(ParticleSystem ps,
                                       float emission,
                                       float startSize,
                                       float lifetime,
                                       Color tint,
                                       float shapeRadius)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = 0.2f;
            main.startSize = startSize;
            main.startColor = tint;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;

            var em = ps.emission;
            em.rateOverTime = emission;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = shapeRadius;
            shape.arc = 360f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(tint, 0f), new GradientColorKey(tint, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            var curve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(1f, 2.5f));
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // URP-safe renderer fallback: use built-in Default-Particle so it shows up
            // even before the project's URP particle material is wired.
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.sharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    mat.color = tint;
                    renderer.sharedMaterial = mat; // URP-safe
                }
            }
        }

        // ─── UI ─────────────────────────────────────────────────────────
        static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            return _whiteSprite;
        }

        void EnsureUIBuilt()
        {
            if (_panel != null) return;

            if (_sharedCanvas == null)
            {
                var canvasGO = new GameObject("TuningCanvas_Cymatic");
                _sharedCanvas = canvasGO.AddComponent<Canvas>();
                _sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _sharedCanvas.sortingOrder = 32000;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            _panel = new GameObject("CymaticPanel");
            _panel.transform.SetParent(_sharedCanvas.transform, false);
            var rt = _panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.15f);
            rt.anchorMax = new Vector2(0.5f, 0.15f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900f, 240f);
            rt.anchoredPosition = Vector2.zero;
            var bg = _panel.AddComponent<Image>();
            bg.sprite = GetWhiteSprite();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.88f);

            // Status text
            _statusText = AddText(_panel.transform, "Status", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                   new Vector2(800f, 40f), new Vector2(0f, -8f),
                                   28, new Color(0.85f, 0.65f, 0.10f), $"Tune the water to {targetFrequency:F0} Hz");

            // Current freq + accuracy
            _currentFreqText = AddText(_panel.transform, "CurrentFreq", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
                                       new Vector2(800f, 60f), new Vector2(0f, 0f),
                                       40, Color.white, "—");
            _currentFreqText.fontStyle = FontStyle.Bold;

            // Meter
            var meterGO = new GameObject("Meter");
            meterGO.transform.SetParent(_panel.transform, false);
            var meterRT = meterGO.AddComponent<RectTransform>();
            meterRT.anchorMin = new Vector2(0.05f, 0.30f);
            meterRT.anchorMax = new Vector2(0.95f, 0.30f);
            meterRT.pivot = new Vector2(0.5f, 0.5f);
            meterRT.sizeDelta = new Vector2(0f, 12f);
            meterRT.anchoredPosition = Vector2.zero;
            _meter = meterGO.AddComponent<Image>();
            _meter.sprite = GetWhiteSprite();
            _meter.color = Color.red;

            // Slider (passive — driven by HandleInput; clicking is OK too)
            var sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(_panel.transform, false);
            var sliderRT = sliderGO.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.05f, 0.05f);
            sliderRT.anchorMax = new Vector2(0.95f, 0.22f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.sizeDelta = Vector2.zero;
            sliderRT.anchoredPosition = Vector2.zero;
            _slider = sliderGO.AddComponent<Slider>();
            _slider.minValue = minFrequency;
            _slider.maxValue = maxFrequency;
            _slider.value = _currentFreq;
            BuildSliderVisuals(sliderGO, _slider);
            _slider.onValueChanged.AddListener(v => _currentFreq = v);

            // Target band marker — golden tick at targetFrequency position
            var tickGO = new GameObject("TargetTick");
            tickGO.transform.SetParent(sliderGO.transform, false);
            var tickRT = tickGO.AddComponent<RectTransform>();
            float tFrac = Mathf.InverseLerp(minFrequency, maxFrequency, targetFrequency);
            tickRT.anchorMin = new Vector2(tFrac, 0f);
            tickRT.anchorMax = new Vector2(tFrac, 1f);
            tickRT.pivot = new Vector2(0.5f, 0.5f);
            tickRT.sizeDelta = new Vector2(4f, 0f);
            tickRT.anchoredPosition = Vector2.zero;
            var tickImg = tickGO.AddComponent<Image>();
            tickImg.sprite = GetWhiteSprite();
            tickImg.color = new Color(0.95f, 0.78f, 0.20f, 1f);

            Debug.Log("[CymaticWater] Auto-built UI: Canvas + Panel + Slider + Texts + Meter + Target Tick");
        }

        static Text AddText(Transform parent, string name,
                            Vector2 anchorMin, Vector2 anchorMax,
                            Vector2 sizeDelta, Vector2 anchoredPos,
                            int fontSize, Color color, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, anchorMax.y);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = text;
            return t;
        }

        static void BuildSliderVisuals(GameObject sliderRoot, Slider slider)
        {
            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(sliderRoot.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = GetWhiteSprite();
            bgImg.color = new Color(0.10f, 0.18f, 0.25f, 1f);

            // Fill area
            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(sliderRoot.transform, false);
            var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRT.sizeDelta = Vector2.zero;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = GetWhiteSprite();
            fillImg.color = new Color(0.30f, 0.55f, 0.85f, 1f);
            slider.fillRect = fillRT;
            slider.targetGraphic = fillImg;
        }

        // ═════════════════════════════════════════════════════════════════
        // Config record — preserved so callers that constructed CymaticConfig
        // before this refactor still compile.
        // ═════════════════════════════════════════════════════════════════
        [Serializable]
        public class CymaticConfig
        {
            public float timeLimit = 45f;
            public int difficulty = 1;
            public int patternType = -1;
            public static CymaticConfig Default()  => new CymaticConfig { difficulty = 1, timeLimit = 45f };
            public static CymaticConfig Easy()     => new CymaticConfig { difficulty = 0, timeLimit = 60f };
            public static CymaticConfig Advanced() => new CymaticConfig { difficulty = 2, timeLimit = 30f };
        }
    }
}
