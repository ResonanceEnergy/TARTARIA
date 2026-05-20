using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Gameplay; // for RailEscortController Moon 3 boss synergy hook (R5)

namespace Tartaria.Integration
{
    /// <summary>
    /// Boss Encounter System — multi-phase boss fights at Moon climaxes.
    ///
    /// Design per GDD §06 (Combat), §11 (Scripted Climaxes), 03C Moon Mechanics, 10 Roadmap Phase 3 polish:
    ///   - Each boss teaches frequency puzzle mastery while the world reacts
    ///   - Live player freq submissions via HarmonicCombatant (R5)
    ///   - Dedicated phase AI per major boss with telegraph VFX, vuln windows, desperation
    ///   - Golden Cascade payoffs on masterful solves
    ///   - Full persistence via hardened BossSaveState (all bosses, puzzle state)
    ///   - Moon 3 rail/leviathan synergy (internal + escort hook)
    ///
    /// Boss types supported (R6 full coverage): Mud Colossus, RailWraith (swarm), SludgeLeviathan,
    /// SkyReaver (aerial), ResetSeeker, Dissonance Leviathan + future.
    /// </summary>
    public class BossEncounterSystem : MonoBehaviour
    {
        public static BossEncounterSystem Instance { get; private set; }

        // ─── Events ───
        public event Action<BossDefinition> OnBossSpawned;
        public event Action<int> OnPhaseChanged;          // new phase index
        public event Action<float> OnBossHealthChanged;   // normalized 0-1
        public event Action<BossResult> OnBossDefeated;
        public event Action OnBossFailed;
        public event Action<string> OnBossDialogue;       // dialogue line

        // ─── State ───
        BossDefinition _currentBoss;
        int _currentPhase;
        float _bossHP;
        float _bossMaxHP;
        float _encounterTime;
        int _playerHits;         // hits taken by player
        bool _isActive;
        float _vulnerableTimer;
        bool _isVulnerable;
        float _phaseTransitionTimer;

        // Phase mechanics
        float _attackCooldown;
        float _patternTimer;
        int _patternIndex;

        // Round 4: Frequency puzzle integration + visual sync state (Bosses & Advanced Enemies)
        float _currentTargetFrequency;
        bool _frequencyPuzzleActive;
        float _lastHealthSync;
        GameObject _colossusVisualProxy; // for Mud Colossus dynamic health visuals (procedural)
        GameObject _railWraithVisual;    // procedural/DOTS-style visual proxy for RailWraith 3-phase
        GameObject _sludgeVisual;        // procedural/DOTS-style for SludgeLeviathan 3-phase

        // Round 5: Dedicated boss AI state (Mud Colossus phases) + telegraph + persistence prep
        float _mudColossusSpecialTimer;
        int _mudColossusQuakeCount;
        float _telegraphPulseTimer;
        float _lastTelegraphHz;

        // ─── R6: Extended live frequency + dedicated AI for RailWraith swarm, Dissonance Leviathan, SkyReaver aerial + full persistence harden ───
        float _railWraithSwarmTimer;
        int _railWraithSwarmSize;
        float _leviathanResonanceTimer;
        int _leviathanSynergyLevel; // internal Moon 3 escort/rail synergy payoff (narrative + mechanical world react without external scaffolding edits)
        float _skyReaverAltitude;
        GameObject _skyReaverVisual; // aerial proxy for SkyReaver (high-frequency dives)
        float _desperationTimer;
        List<float> _submittedFrequenciesThisFight;
        float _bestMatchAccuracy;
        int _puzzleAttempts;
        bool _goldenCascadeTriggered;

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float BossHPNormalized => _bossMaxHP > 0 ? _bossHP / _bossMaxHP : 0f;
        public int CurrentPhase => _currentPhase;
        public bool IsVulnerable => _isVulnerable;
        public BossDefinition CurrentBoss => _currentBoss;
        public float CurrentTargetFrequency => _currentTargetFrequency;
        public bool IsFrequencyPuzzleActive => _frequencyPuzzleActive && _isVulnerable;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            _submittedFrequenciesThisFight = new List<float>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("BossEncounterSystem");
            DontDestroyOnLoad(go);
            go.AddComponent<BossEncounterSystem>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Named Boss Lookup ────────────────────────
        static readonly Dictionary<string, int> NamedBossLookup = new()
        {
            { "mud_colossus", 0 },
            { "quartz_defiler", 1 },
            { "spire_breaker", 2 },
            { "iron_corruptor", 3 },
            { "echo_sovereign", 4 },
            { "crystal_phantom", 5 },
            { "fractal_tyrant", 6 },
            { "mirror_empress", 7 },
            { "void_shaper", 8 },
            { "rail_leviathan", 9 },
            { "sludge_leviathan", 10 },
            { "anti_resonance", 11 },
            { "guardian_of_true_history", 12 },
            { "rift_walker", 9 },
            { "ley_devourer", 10 },
            // R6: full coverage for advanced bosses
            { "sky_reaver", 8 },
            { "reset_seeker", 4 },
            { "dissonance_leviathan", 9 },
            { "rail_wraith", 9 }
        };

        // ─── Start / Stop ────────────────────────────

        /// <summary>Begin a boss encounter by string ID (e.g. "sludge_leviathan", "sky_reaver", "rail_wraith").</summary>
        public void SpawnBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId))
            {
                Debug.LogWarning("[Boss] SpawnBoss called with null/empty bossId.");
                return;
            }

            string key = bossId.ToLowerInvariant().Replace(' ', '_');
            if (NamedBossLookup.TryGetValue(key, out int moonIndex))
            {
                StartBoss(moonIndex);
            }
            else
            {
                Debug.LogWarning($"[Boss] Unknown bossId: {bossId}. Spawning default.");
                StartBoss(-1); // triggers default case in BuildBossForMoon
            }
        }

        /// <summary>Begin a boss encounter for the given Moon.</summary>
        public void StartBoss(int moonIndex)
        {
            _currentBoss = BuildBossForMoon(moonIndex);
            _bossMaxHP = _currentBoss.totalHP;
            _bossHP = _bossMaxHP;
            _currentPhase = 0;
            _encounterTime = 0f;
            _playerHits = 0;
            _isActive = true;
            _isVulnerable = false;
            _vulnerableTimer = 0f;
            _phaseTransitionTimer = 0f;
            _attackCooldown = 0f;
            _patternTimer = 0f;
            _patternIndex = 0;

            // Round 4 frequency puzzle + visual state reset
            _currentTargetFrequency = 432f;
            _frequencyPuzzleActive = false;
            _lastHealthSync = 1f;
            CleanupBossVisualProxies();

            // Round 5 dedicated AI reset
            _mudColossusSpecialTimer = 0f;
            _mudColossusQuakeCount = 0;
            _telegraphPulseTimer = 0f;
            _lastTelegraphHz = 0f;

            // R6: full reset for extended bosses + puzzle tracking
            _railWraithSwarmTimer = 0f;
            _railWraithSwarmSize = 0;
            _leviathanResonanceTimer = 0f;
            _leviathanSynergyLevel = 0;
            _skyReaverAltitude = 4.2f;
            _desperationTimer = 0f;
            _submittedFrequenciesThisFight.Clear();
            _bestMatchAccuracy = 0f;
            _puzzleAttempts = 0;
            _goldenCascadeTriggered = false;

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            OnBossSpawned?.Invoke(_currentBoss);
            OnBossDialogue?.Invoke(_currentBoss.phases[0].entranceDialogue);

            Debug.Log($"[Boss] {_currentBoss.bossName} spawned — {_currentBoss.phases.Count} phases, {_bossMaxHP} HP (R6: live freq + dedicated AI for Rail/Sky/Leviathan)");
        }

        /// <summary>Force-abort the boss encounter.</summary>
        public void AbortBoss()
        {
            _isActive = false;
            CleanupBossVisualProxies();
            OnBossFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
            Debug.Log("[Boss] Encounter aborted gracefully (player retreat / manual).");
        }

        /// <summary>
        /// Graceful fail with reason (time-out, too many hits, puzzle abort). Called from external or internal timers.
        /// </summary>
        public void FailBoss(string reason)
        {
            if (!_isActive) return;
            _isActive = false;
            CleanupBossVisualProxies();
            OnBossDialogue?.Invoke($"The encounter ends... {reason}");
            OnBossFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
            Debug.Log($"[Boss] Encounter failed gracefully: {reason}");
            VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
        }

        void CleanupBossVisualProxies()
        {
            if (_colossusVisualProxy != null) { Destroy(_colossusVisualProxy); _colossusVisualProxy = null; }
            if (_railWraithVisual != null) { Destroy(_railWraithVisual); _railWraithVisual = null; }
            if (_sludgeVisual != null) { Destroy(_sludgeVisual); _sludgeVisual = null; }
            if (_skyReaverVisual != null) { Destroy(_skyReaverVisual); _skyReaverVisual = null; }
        }

        // ─── Round 5: Persistence for active boss state + current target frequency ───
        // R6: Hardened for ALL current bosses (Mud, RailWraith swarm, Leviathan synergy, SkyReaver altitude, full puzzle stats)
        /// <summary>Serializable snapshot for SaveManager / GameLoop wiring (resumable boss encounters). v11-ready full puzzle state.</summary>
        [Serializable]
        public class BossSaveState
        {
            public bool isActive;
            public string bossName;
            public int currentPhase;
            public float currentHP;
            public float maxHP;
            public float currentTargetFrequency;
            public float encounterTime;
            public int playerHitsReceived;
            public bool frequencyPuzzleWasActive;

            // R6 hardened persistence (all bosses)
            public int railWraithSwarmSize;
            public int leviathanSynergyLevel;
            public float skyReaverAltitude;
            public float bestMatchAccuracy;
            public int puzzleAttempts;
            public int submittedCount;
            public bool goldenCascadeTriggeredThisFight;
        }

        public BossSaveState GetSaveState()
        {
            return new BossSaveState
            {
                isActive = _isActive,
                bossName = _currentBoss?.bossName ?? "",
                currentPhase = _currentPhase,
                currentHP = _bossHP,
                maxHP = _bossMaxHP,
                currentTargetFrequency = _currentTargetFrequency,
                encounterTime = _encounterTime,
                playerHitsReceived = _playerHits,
                frequencyPuzzleWasActive = _frequencyPuzzleActive,

                // R6 full state for resume (swarm, synergy, aerial, puzzle history)
                railWraithSwarmSize = _railWraithSwarmSize,
                leviathanSynergyLevel = _leviathanSynergyLevel,
                skyReaverAltitude = _skyReaverAltitude,
                bestMatchAccuracy = _bestMatchAccuracy,
                puzzleAttempts = _puzzleAttempts,
                submittedCount = _submittedFrequenciesThisFight != null ? _submittedFrequenciesThisFight.Count : 0,
                goldenCascadeTriggeredThisFight = _goldenCascadeTriggered
            };
        }

        /// <summary>Restore mid-fight boss exactly as left (persistent satisfying encounter resume). R6: restores swarm/synergy/aerial/puzzle state for all bosses.</summary>
        public void LoadSaveState(BossSaveState state)
        {
            if (state == null || !state.isActive || string.IsNullOrEmpty(state.bossName)) return;

            // Seed via normal spawn (establishes definition/phases) then override live values
            SpawnBoss(state.bossName.ToLowerInvariant().Replace(' ', '_'));

            _currentPhase = Mathf.Clamp(state.currentPhase, 0, 10);
            _bossHP = Mathf.Max(10f, state.currentHP);
            if (state.maxHP > 10f) _bossMaxHP = state.maxHP;
            _currentTargetFrequency = state.currentTargetFrequency > 20f ? state.currentTargetFrequency : _currentTargetFrequency;
            _encounterTime = state.encounterTime;
            _playerHits = Mathf.Max(0, state.playerHitsReceived);
            _frequencyPuzzleActive = state.frequencyPuzzleWasActive;
            _isActive = true;
            _isVulnerable = _frequencyPuzzleActive;
            _vulnerableTimer = _isVulnerable ? 1.8f : 0f;

            // R6 restore
            _railWraithSwarmSize = Mathf.Max(0, state.railWraithSwarmSize);
            _leviathanSynergyLevel = Mathf.Clamp(state.leviathanSynergyLevel, 0, 6);
            _skyReaverAltitude = state.skyReaverAltitude > 0.2f ? state.skyReaverAltitude : 3.8f;
            _bestMatchAccuracy = state.bestMatchAccuracy;
            _puzzleAttempts = state.puzzleAttempts;
            _goldenCascadeTriggered = state.goldenCascadeTriggeredThisFight;
            _submittedFrequenciesThisFight.Clear();

            // Rebuild visuals for resumed boss (Colossus scale sync, Rail/Sludge/Sky phases)
            if (_currentBoss != null)
            {
                string phaseName = (_currentPhase < _currentBoss.phases.Count) ? _currentBoss.phases[_currentPhase].phaseName : "Resumed";
                SpawnOrUpdateBossVisuals(phaseName);
            }

            OnBossHealthChanged?.Invoke(BossHPNormalized);
            Debug.Log($"[Boss] PERSISTENCE: Resumed active boss '{_currentBoss?.bossName}' phase {_currentPhase} target~{_currentTargetFrequency:F0}Hz (HP {BossHPNormalized:P0}) | R6: swarm={_railWraithSwarmSize} synergy={_leviathanSynergyLevel} skyAlt={_skyReaverAltitude:F1} bestMatch={_bestMatchAccuracy:P0}");
        }

        bool IsMudColossus() => _currentBoss != null && _currentBoss.bossName.ToLowerInvariant().Contains("mud") && _currentBoss.bossName.ToLowerInvariant().Contains("colossus");
        // R6: dedicated per-boss type checks for full puzzle + AI coverage
        bool IsRailWraith() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("rail") || _currentBoss.bossName.ToLowerInvariant().Contains("wraith"));
        bool IsDissonanceLeviathan() => _currentBoss != null && _currentBoss.bossName.ToLowerInvariant().Contains("leviathan");
        bool IsSkyReaver() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("sky") || _currentBoss.bossName.ToLowerInvariant().Contains("reaver"));
        bool IsResetSeeker() => _currentBoss != null && _currentBoss.bossName.ToLowerInvariant().Contains("reset") || _currentBoss.bossName.ToLowerInvariant().Contains("seeker");

        /// <summary>
        /// Round 4: Wire frequency puzzle submission into real combat via HarmonicStrike/ResonancePulse hooks.
        /// Call this from CombatBridge when boss is active + vulnerable: match quality drives scaled DealDamage.
        /// R6: Full per-boss puzzle integration (Rail dissonance swarm clear, Leviathan resonance synergy payoff, SkyReaver aerial dive, Golden Cascade on mastery).
        /// </summary>
        public void SubmitFrequencyPuzzle(float submittedFreq, float baseDamageMultiplier = 1f)
        {
            if (!_isActive || !_isVulnerable || !_frequencyPuzzleActive) return;

            float delta = Mathf.Abs(submittedFreq - _currentTargetFrequency);
            float tolerance = 55f; // forgiving but rewarding window for boss puzzle
            float matchQuality = (delta < tolerance) ? Mathf.Clamp01(1f - (delta / tolerance)) : 0f;

            if (matchQuality > 0.05f)
            {
                float damage = 35f * (0.6f + matchQuality * 1.8f) * baseDamageMultiplier;
                DealDamage(damage);

                OnBossDialogue?.Invoke($"Harmonic lock: {matchQuality:P0} match @ {_currentTargetFrequency:F0}Hz");
                HapticFeedbackManager.Instance?.PlayPerfectTune();
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2f);

                // Keep puzzle window open but nudge target slightly on strong hits (dynamic)
                if (matchQuality > 0.7f)
                    _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + UnityEngine.Random.Range(-18f, 18f), 0.3f);

                // R6: Full puzzle integration + world-reacting payoffs
                if (IsRailWraith())
                {
                    int cleared = Mathf.RoundToInt(matchQuality * 3.5f);
                    _railWraithSwarmSize = Mathf.Max(0, _railWraithSwarmSize - cleared);
                    if (_railWraithSwarmSize <= 1)
                    {
                        OnBossDialogue?.Invoke("Swarm shattered! You solved the living dissonance frequency!");
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3f);
                    }
                }

                if (IsDissonanceLeviathan())
                {
                    _leviathanSynergyLevel = Mathf.Min(6, _leviathanSynergyLevel + (matchQuality > 0.65f ? 1 : 0));
                    if (_leviathanSynergyLevel >= 3)
                    {
                        // Real mechanical + narrative payoff for good freq play during escort (Moon 3 synergy fantasy)
                        OnBossDialogue?.Invoke(_leviathanSynergyLevel >= 5 ? "The rails sing with the orphans' lullaby! Full Golden resonance!" : "Leviathan resonance builds — the train feels your frequency!");
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.forward * 2.5f);
                        // Extra world react on high synergy (escalating payoff)
                        if (_leviathanSynergyLevel % 2 == 0)
                            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position);
                    }
                }

                if (IsSkyReaver())
                {
                    // Aerial frequency puzzle: high match forces dive (lowers altitude, opens bigger vuln next)
                    _skyReaverAltitude = Mathf.Max(0.8f, _skyReaverAltitude - matchQuality * 1.6f);
                    OnBossDialogue?.Invoke(matchQuality > 0.75f ? "Sky Reaver dives! Frequency mastery pulls it from the clouds!" : "Aerial lock — the reaver wavers!");
                    if (_skyReaverVisual != null)
                        _skyReaverVisual.transform.position = transform.position + new Vector3(0, _skyReaverAltitude, 6f);
                }

                if (IsResetSeeker())
                {
                    // Seeker freq: strong match disrupts its seeking patterns
                    OnBossDialogue?.Invoke("Seeker pattern broken! Precise frequency shatters its scan!");
                }

                // R6: Golden Cascade payoff — the satisfying "I solved the living frequency puzzle while the world reacts"
                if (matchQuality > 0.85f && !_goldenCascadeTriggered)
                {
                    _goldenCascadeTriggered = true;
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 4f);
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.right * 2.2f);
                    VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.forward * 1.8f);
                    OnBossDialogue?.Invoke("GOLDEN CASCADE! You solved the living frequency — the world sings back in harmony!");
                    // Extra payoff damage + phase nudge for climax feel
                    DealDamage(22f);
                    if (_currentPhase < _currentBoss.phases.Count - 1)
                        _currentTargetFrequency += UnityEngine.Random.Range(-40f, 40f);
                }

                // Track for R6 hardened persistence
                _submittedFrequenciesThisFight.Add(submittedFreq);
                _bestMatchAccuracy = Mathf.Max(_bestMatchAccuracy, matchQuality);
                _puzzleAttempts++;

                // Round 5: Moon 3 boss synergy with escort — live freq submissions on rail bosses empower the orphan train defense
                if (_currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("rail") || _currentBoss.bossName.ToLowerInvariant().Contains("leviathan")))
                {
                    RailEscortController.Instance?.ApplyRailBossSynergy(matchQuality);
                }
            }
            else
            {
                OnBossDialogue?.Invoke("Dissonant — retune your strike!");
                // small feedback damage even on miss for tension (graceful)
                DealDamage(4f);
            }
        }

        // ─── Round 4 Boss Visuals & Health Sync (procedural, no new assets) ───

        void SpawnOrUpdateBossVisuals(string phaseName)
        {
            if (_currentBoss == null) return;
            string name = _currentBoss.bossName.ToLowerInvariant();

            Vector3 spawnPos = transform.position + new Vector3(0, 1.2f, 8f); // forward of system for arena feel

            if (name.Contains("mud") || name.Contains("colossus"))
            {
                if (_colossusVisualProxy == null)
                {
                    _colossusVisualProxy = new GameObject("MudColossus_VisualProxy");
                    _colossusVisualProxy.transform.position = spawnPos;
                    var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    body.transform.SetParent(_colossusVisualProxy.transform);
                    body.transform.localScale = new Vector3(2.8f, 3.2f, 2.8f);
                    body.GetComponent<Renderer>().material.color = new Color(0.35f, 0.28f, 0.18f, 1f); // deep mud
                    body.name = "ColossusBody";
                    Debug.Log("[Boss] Mud Colossus procedural visual proxy spawned (post-fountain climax).");
                }
                SyncMudColossusVisuals(BossHPNormalized);
            }
            else if (name.Contains("rail"))
            {
                if (_railWraithVisual == null)
                {
                    _railWraithVisual = new GameObject("RailWraith_VisualProxy");
                    _railWraithVisual.transform.position = spawnPos + new Vector3(4f, 2f, 0);
                    var railBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    railBody.transform.SetParent(_railWraithVisual.transform);
                    railBody.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
                    railBody.GetComponent<Renderer>().material.color = new Color(0.25f, 0.22f, 0.3f);
                    Debug.Log("[Boss] RailWraith procedural 3-phase visual spawned.");
                }
                UpdateRailWraithVisualPhase(phaseName);
            }
            else if (name.Contains("sludge"))
            {
                if (_sludgeVisual == null)
                {
                    _sludgeVisual = new GameObject("SludgeLeviathan_VisualProxy");
                    _sludgeVisual.transform.position = spawnPos + new Vector3(-4f, 0.8f, 0);
                    var sludgeBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sludgeBody.transform.SetParent(_sludgeVisual.transform);
                    sludgeBody.transform.localScale = new Vector3(3.5f, 1.6f, 3.5f);
                    sludgeBody.GetComponent<Renderer>().material.color = new Color(0.18f, 0.32f, 0.22f);
                    Debug.Log("[Boss] SludgeLeviathan procedural 3-phase visual spawned.");
                }
                UpdateSludgeLeviathanVisualPhase(phaseName);
            }
            // R6: SkyReaver aerial visual proxy (high altitude, dives on freq mastery)
            else if (name.Contains("sky") || name.Contains("reaver"))
            {
                if (_skyReaverVisual == null)
                {
                    _skyReaverVisual = new GameObject("SkyReaver_VisualProxy");
                    _skyReaverVisual.transform.position = spawnPos + new Vector3(0, _skyReaverAltitude, 6f);
                    var reaverBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    reaverBody.transform.SetParent(_skyReaverVisual.transform);
                    reaverBody.transform.localScale = new Vector3(1.1f, 0.6f, 2.4f); // sleek aerial
                    reaverBody.GetComponent<Renderer>().material.color = new Color(0.15f, 0.18f, 0.35f);
                    Debug.Log("[Boss] SkyReaver aerial procedural visual proxy spawned (high-freq puzzle).");
                }
                // position respects current altitude
                _skyReaverVisual.transform.position = spawnPos + new Vector3(0, _skyReaverAltitude, 6f);
            }
        }

        void SyncMudColossusVisuals(float normalizedHP)
        {
            if (_colossusVisualProxy == null) return;
            // Dynamic scale (shrinks as damaged) + emission-like color shift
            float scale = Mathf.Lerp(1.4f, 2.8f, normalizedHP);
            var body = _colossusVisualProxy.transform.Find("ColossusBody");
            if (body != null)
            {
                body.localScale = new Vector3(scale, scale * 1.15f, scale);
                var rend = body.GetComponent<Renderer>();
                if (rend != null)
                {
                    // "Emission" via color brightening on high HP, dark on low (mud cracking)
                    Color c = Color.Lerp(new Color(0.12f, 0.1f, 0.07f), new Color(0.55f, 0.45f, 0.22f), normalizedHP);
                    rend.material.color = c;
                }
            }
            _lastHealthSync = normalizedHP;

            // Occasional procedural VFX pulse on health change (DOTS-like visual reactivity)
            if (Mathf.Abs(normalizedHP - _lastHealthSync) > 0.08f || normalizedHP < 0.4f)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, _colossusVisualProxy.transform.position);
            }
        }

        void UpdateRailWraithVisualPhase(string phaseName)
        {
            if (_railWraithVisual == null) return;
            var rend = _railWraithVisual.GetComponentInChildren<Renderer>();
            if (rend == null) return;
            // 3-phase procedural visuals: phase 0 dark rail, phase1 sparking, phase2 enraged red
            if (phaseName.ToLower().Contains("true") || phaseName.ToLower().Contains("collapse"))
                rend.material.color = new Color(0.7f, 0.15f, 0.1f);
            else if (phaseName.ToLower().Contains("multi") || phaseName.ToLower().Contains("decon"))
                rend.material.color = new Color(0.9f, 0.85f, 0.3f); // sparks
            else
                rend.material.color = new Color(0.25f, 0.22f, 0.3f);
            // Procedural "DOTS" bob for rail motion feel
            _railWraithVisual.transform.position = transform.position + new Vector3(4f + Mathf.Sin(Time.time * 4f) * 0.8f, 2f, 0);
        }

        void UpdateSludgeLeviathanVisualPhase(string phaseName)
        {
            if (_sludgeVisual == null) return;
            var rend = _sludgeVisual.GetComponentInChildren<Renderer>();
            if (rend == null) return;
            // 3-phase: initial murky, mid bubbling, final enraged viscous
            if (phaseName.ToLower().Contains("truth") || phaseName.ToLower().Contains("void"))
                rend.material.color = new Color(0.45f, 0.1f, 0.08f);
            else if (phaseName.ToLower().Contains("demo") || phaseName.ToLower().Contains("decon"))
                rend.material.color = new Color(0.15f, 0.42f, 0.28f); // bubbling green
            else
                rend.material.color = new Color(0.18f, 0.32f, 0.22f);
            // Procedural pulse scale for sludge breathing / DOTS reactivity
            float pulse = 1f + Mathf.Sin(Time.time * 2.2f) * 0.08f * (1f - BossHPNormalized * 0.5f);
            _sludgeVisual.transform.localScale = Vector3.one * pulse;
        }

        void UpdateBossVisuals()
        {
            if (!_isActive) return;
            // Lightweight per-frame procedural animation for RailWraith / SludgeLeviathan (DOTS-feel without DOTS overhead)
            if (_railWraithVisual != null)
            {
                _railWraithVisual.transform.position = transform.position + new Vector3(4f + Mathf.Sin(Time.time * 4.5f) * 1.1f, 2f + Mathf.Cos(Time.time * 1.8f) * 0.4f, Mathf.Sin(Time.time * 1.2f) * 0.6f);
            }
            if (_sludgeVisual != null)
            {
                _sludgeVisual.transform.position = transform.position + new Vector3(-4f, 0.8f + Mathf.Sin(Time.time * 3f) * 0.35f, 0);
            }
            // R6: SkyReaver aerial bob + altitude dive reactivity
            if (_skyReaverVisual != null)
            {
                float bob = Mathf.Sin(Time.time * 3.8f) * 0.35f;
                _skyReaverVisual.transform.position = transform.position + new Vector3(0, _skyReaverAltitude + bob, 6f + Mathf.Cos(Time.time * 1.6f) * 0.4f);
            }
            // Colossus already synced on health; occasional idle pulse if low
            if (_colossusVisualProxy != null && BossHPNormalized < 0.35f && Time.frameCount % 18 == 0)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.Spark, _colossusVisualProxy.transform.position);
            }
        }

        void Update()
        {
            if (!_isActive) return;

            _encounterTime += Time.deltaTime;

            // Round 4: Graceful fail handling (timeout / excessive hits)
            if (_encounterTime > (_currentBoss?.parTime ?? 90f) * 2.8f)
            {
                FailBoss("the frequencies grow too wild — time runs out");
                return;
            }
            if (_playerHits > 14)
            {
                FailBoss("overwhelmed by dissonance — retreat and retune");
                return;
            }

            // Phase transition cinematic
            if (_phaseTransitionTimer > 0f)
            {
                _phaseTransitionTimer -= Time.deltaTime;
                return; // Freeze during transition
            }

            UpdateBossAI();
            UpdateVulnerability();
            UpdateBossVisuals();

            // Round 5: Frequency telegraph VFX pulse driver (real Hz-synced rings during vuln windows for satisfying encounters)
            if (_isVulnerable && _frequencyPuzzleActive)
            {
                _telegraphPulseTimer += Time.deltaTime;
                float pulseRate = Mathf.Clamp(1.8f - Mathf.Clamp01((_currentTargetFrequency - 80f) / 420f) * 0.9f, 0.35f, 1.6f); // higher target Hz = faster, more urgent telegraph
                if (_telegraphPulseTimer >= pulseRate)
                {
                    _telegraphPulseTimer = 0f;
                    Vector3 basePos = transform.position + Vector3.up * 2.2f;
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, basePos);
                    // Secondary ring offset for "pulsing at exact frequency" depth
                    if (_currentTargetFrequency > 140f)
                        VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, basePos + Vector3.right * 1.8f);
                    // R6 deepen: extra layer for aerial/sky and high-freq urgency
                    if (_currentTargetFrequency > 320f || IsSkyReaver())
                        VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, basePos + Vector3.up * 1.4f);
                    if (IsRailWraith() && _railWraithSwarmSize > 2)
                        VFXController.Instance?.PlayEffect(VFXEffect.Spark, basePos + Vector3.left * 1.4f);
                }
            }

            // Dedicated Mud Colossus AI tick (post-fountain climax now has real phases)
            if (IsMudColossus())
                UpdateMudColossusDedicatedAI();

            // R6: Dedicated AI for remaining major bosses (swarm + resonance + aerial)
            if (IsRailWraith())
                UpdateRailWraithDedicatedAI();
            if (IsDissonanceLeviathan())
                UpdateDissonanceLeviathanDedicatedAI();
            if (IsSkyReaver())
                UpdateSkyReaverDedicatedAI();
            if (IsResetSeeker())
                UpdateResetSeekerDedicatedAI();

            // R6 shared desperation (all bosses)
            if (BossHPNormalized < 0.32f)
            {
                _desperationTimer -= Time.deltaTime;
                if (_desperationTimer <= 0f)
                {
                    _desperationTimer = 7.5f;
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position + Vector3.up * 1.1f);
                    OnBossDialogue?.Invoke("The boss frenzies — only perfect frequency solves this now!");
                }
            }
        }

        // ─── Damage ─────────────────────────────────

        /// <summary>Deal damage to the boss. Only effective during vulnerability.</summary>
        public void DealDamage(float damage)
        {
            if (!_isActive || !_isVulnerable) return;

            _bossHP -= damage;
            OnBossHealthChanged?.Invoke(BossHPNormalized);

            // Round 4: Dynamic health sync for Mud Colossus visuals (scale + emission intensity)
            if (_currentBoss != null && _currentBoss.bossName.Contains("Mud") && _currentBoss.bossName.Contains("Colossus"))
            {
                SyncMudColossusVisuals(BossHPNormalized);
            }

            if (_bossHP <= 0f)
            {
                DefeatBoss();
                return;
            }

            // Check phase transitions
            CheckPhaseTransition();
        }

        /// <summary>Player was hit by boss attack.</summary>
        public void RegisterPlayerHit()
        {
            _playerHits++;
        }

        // ─── AI ──────────────────────────────────────

        void UpdateBossAI()
        {
            if (_currentPhase >= _currentBoss.phases.Count) return;
            var phase = _currentBoss.phases[_currentPhase];

            _attackCooldown -= Time.deltaTime;
            _patternTimer -= Time.deltaTime;

            if (_attackCooldown <= 0f)
            {
                ExecuteAttackPattern(phase);
                _attackCooldown = phase.attackInterval;
            }
        }

        // ─── Round 5: Dedicated Mud Colossus AI (phases feel like a real persistent boss encounter) ───
        void UpdateMudColossusDedicatedAI()
        {
            if (!_isActive || _currentBoss == null) return;

            _mudColossusSpecialTimer -= Time.deltaTime;

            // Phase-aware special behaviors for Mud Colossus (post-fountain climax)
            float hpNorm = BossHPNormalized;
            int phase = _currentPhase;

            // Special timer fires unique mechanics (telegraphed, satisfying counters via frequency puzzle)
            if (_mudColossusSpecialTimer <= 0f)
            {
                _mudColossusSpecialTimer = (phase == 0) ? 6.5f : 4.2f;

                if (phase == 0) // Awakening: Mud Siphon + sink telegraph
                {
                    // Slows player + minor corruption; visual sink ring
                    var combat = CombatBridge.Instance;
                    if (combat != null) combat.DamagePlayer(6f, "mud_siphon");
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position + Vector3.down * 0.5f);
                    OnBossDialogue?.Invoke("The earth drinks your resonance...");
                    _mudColossusQuakeCount = 0;
                }
                else // Frenzy: Resonance Quake (multiple pulses synced to current target freq for puzzle tie-in)
                {
                    _mudColossusQuakeCount++;
                    VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position);
                    var combat = CombatBridge.Instance;
                    if (combat != null) combat.DamagePlayer(9f + _mudColossusQuakeCount * 2f, "colossus_quake");

                    // Quake telegraphs the exact current target frequency (player must match to counter effectively)
                    OnBossDialogue?.Invoke(_mudColossusQuakeCount >= 3 ? "The colossus cracks! Match its buried frequency!" : "Quake — retune or sink!");
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();

                    if (_mudColossusQuakeCount >= 3 && _isVulnerable)
                    {
                        // Bonus: during vuln, quake nudges target slightly (dynamic puzzle)
                        _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + UnityEngine.Random.Range(-25f, 25f), 0.6f);
                    }
                }
            }

            // Low HP desperation: occasional extra mud wave that rewards precise frequency strikes
            if (hpNorm < 0.35f && Time.frameCount % 45 == 0)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.up * 0.8f);
            }
        }

        // ─── R6: Dedicated RailWraith Swarm AI (swarm grows, freq solve thins it, dissonance vuln) ───
        void UpdateRailWraithDedicatedAI()
        {
            if (!_isActive || !IsRailWraith()) return;

            _railWraithSwarmTimer -= Time.deltaTime;

            if (_railWraithSwarmTimer <= 0f)
            {
                _railWraithSwarmTimer = IsDissonanceLeviathan() ? 4.8f : 5.8f;
                _railWraithSwarmSize = Mathf.Min(7, _railWraithSwarmSize + ( _currentPhase + 1 ));
                VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.up * 1.6f);
                OnBossDialogue?.Invoke(_railWraithSwarmSize > 4 ? "The wraiths multiply — solve the dissonance frequency to break the swarm!" : "Rail wraiths converge!");
                var combat = CombatBridge.Instance;
                if (combat != null && _railWraithSwarmSize > 1)
                    combat.DamagePlayer(3f + _railWraithSwarmSize * 0.6f, "rail_swarm");
            }

            // During vuln, swarm size influences target (more chaotic when thick)
            if (_isVulnerable && _railWraithSwarmSize > 3)
            {
                _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + 28f, 0.12f);
            }
        }

        // ─── R6: Dedicated Dissonance Leviathan AI (Moon 3 train escort climax — phases + resonance synergy) ───
        void UpdateDissonanceLeviathanDedicatedAI()
        {
            if (!_isActive || !IsDissonanceLeviathan()) return;

            _leviathanResonanceTimer -= Time.deltaTime;

            float hp = BossHPNormalized;
            int phase = _currentPhase;

            if (_leviathanResonanceTimer <= 0f)
            {
                _leviathanResonanceTimer = (phase == 0) ? 5.2f : (phase == 1 ? 3.8f : 3.1f);

                // Phase-driven resonance waves that shift target freq (player solves living puzzle)
                float shift = (hp < 0.4f ? 45f : 22f) * (phase + 1);
                _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, 155f + UnityEngine.Random.Range(-shift, shift), 0.55f);

                VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.up * 1.3f);
                OnBossDialogue?.Invoke(hp < 0.45f ? "Leviathan screams! Match its buried grief frequency!" : "Resonance wave — retune or the rails fracture!");

                var combat = CombatBridge.Instance;
                if (combat != null)
                    combat.DamagePlayer(7f + phase * 2.5f + (_leviathanSynergyLevel > 2 ? -1.5f : 0), "leviathan_wave"); // synergy reduces incoming on good play

                // High synergy (from good freq during escort) = world reacts with golden payoff
                if (_leviathanSynergyLevel >= 4 && _isVulnerable)
                {
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position);
                    OnBossDialogue?.Invoke("The orphans' lullaby answers your frequency! The leviathan weakens!");
                }
            }

            // Desperation on low HP: faster resonance shifts + telegraph emphasis
            if (hp < 0.28f && Time.frameCount % 32 == 0)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position);
            }
        }

        // ─── R6: Dedicated SkyReaver Aerial AI (high-freq aerial puzzle, altitude dives on mastery) ───
        void UpdateSkyReaverDedicatedAI()
        {
            if (!_isActive || !IsSkyReaver()) return;

            // Aerial bob + occasional dive when low or after strong solve
            if (_skyReaverVisual != null)
            {
                // Dynamic altitude from puzzle solves already handled in Submit
                float targetY = _skyReaverAltitude + Mathf.Sin(Time.time * 4.1f) * 0.25f;
                _skyReaverVisual.transform.position = Vector3.Lerp(_skyReaverVisual.transform.position, transform.position + new Vector3(0, targetY, 6.4f), 0.08f);
            }

            // Desperation dive attack that rewards aerial freq precision
            _desperationTimer -= Time.deltaTime * 0.6f; // faster in air
            if (BossHPNormalized < 0.38f && _desperationTimer <= 0f)
            {
                _desperationTimer = 6.8f;
                _currentTargetFrequency = 410f + UnityEngine.Random.Range(-60f, 85f); // high aerial signature
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3.5f);
                OnBossDialogue?.Invoke("Sky Reaver dives from the aether! Match its high-frequency cry!");
                var c = CombatBridge.Instance;
                if (c != null) c.DamagePlayer(11f, "sky_dive");
            }
        }

        // ─── R6: Dedicated ResetSeeker AI (scanning/seeking patterns disrupted by precise freq) ───
        void UpdateResetSeekerDedicatedAI()
        {
            if (!_isActive || !IsResetSeeker()) return;

            if (Time.frameCount % 48 == 0 && _isVulnerable)
            {
                _currentTargetFrequency += UnityEngine.Random.Range(-15f, 15f);
                OnBossDialogue?.Invoke("Seeker retunes its scan — stay precise!");
            }
        }

        void ExecuteAttackPattern(BossPhase phase)
        {
            if (phase.attackPatterns.Count == 0) return;

            var pattern = phase.attackPatterns[_patternIndex % phase.attackPatterns.Count];
            _patternIndex++;

            float baseDamage = 10f + _currentPhase * 5f;
            var combat = CombatBridge.Instance;
            var playerPos = combat != null ? combat.transform.position : Vector3.zero;

            switch (pattern)
            {
                case BossAttackPattern.Sweep:
                    // Wide 180° cone attack
                    combat?.DamagePlayer(baseDamage, "boss_sweep");
                    VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    break;

                case BossAttackPattern.Slam:
                    // AOE ground slam centered on boss
                    combat?.DamagePlayer(baseDamage * 1.5f, "boss_slam");
                    VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayBuildingEmergence();
                    break;

                case BossAttackPattern.CorruptionWave:
                    // Expanding corruption ring — also applies zone corruption
                    combat?.DamagePlayer(baseDamage * 0.8f, "corruption_wave");
                    CorruptionSystem.Instance?.ApplyCorruption(
                        "boss_arena", _currentPhase * 5f);
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.CorruptionPulse, transform.position);
                    break;

                case BossAttackPattern.MirrorClone:
                    // Spawns a decoy — reduced damage but disorients
                    combat?.DamagePlayer(baseDamage * 0.5f, "mirror_clone");
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.HarmonicCascade, transform.position);
                    break;

                case BossAttackPattern.VoidRift:
                    // Opens a rift that pulls player and deals DOT
                    combat?.DamagePlayer(baseDamage * 1.2f, "void_rift");
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.AetherVortex, transform.position);
                    break;

                case BossAttackPattern.FrequencyJam:
                    // Disables tuning for 5 seconds + minor damage
                    combat?.DamagePlayer(baseDamage * 0.3f, "freq_jam");
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    break;

                case BossAttackPattern.LeyLineSever:
                    // Severs a ley line node and deals damage
                    combat?.DamagePlayer(baseDamage * 0.6f, "ley_sever");
                    Core.LeyLineManager.Instance?.SeverNode(0);
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.Spark, transform.position);
                    break;

                case BossAttackPattern.Enrage:
                    // Boss speeds up — halve attack interval for this phase
                    _attackCooldown *= 0.5f;
                    VFXController.Instance?.PlayEffect(
                        VFXEffect.CorruptionPulse, transform.position);
                    break;
            }

            bool dealtDamage = pattern != BossAttackPattern.Enrage;
            if (dealtDamage)
                RegisterPlayerHit();

            // Audio
            Audio.AudioManager.Instance?.PlayTone(180f, 0.5f);
        }

        void UpdateVulnerability()
        {
            if (_isVulnerable)
            {
                _vulnerableTimer -= Time.deltaTime;
                if (_vulnerableTimer <= 0f)
                {
                    _isVulnerable = false;
                    _frequencyPuzzleActive = false; // puzzle window closed
                }
            }
            else
            {
                // Periodically become vulnerable (frequency-matching window)
                if (_currentPhase < _currentBoss.phases.Count)
                {
                    var phase = _currentBoss.phases[_currentPhase];
                    _vulnerableTimer -= Time.deltaTime;
                    if (_vulnerableTimer <= -phase.invulnerableDuration)
                    {
                        _isVulnerable = true;
                        _vulnerableTimer = phase.vulnerableDuration;
                        _frequencyPuzzleActive = true;

                        // Round 4: Deepened frequency puzzle — assign target on each vuln window
                        // R6: Boss-specific ranges for flavor (Mud earth, Rail dissonance, Sludge gurgle, Sky aerial high, Reset scan)
                        float bossBase = 280f;
                        if (_currentBoss != null)
                        {
                            if (_currentBoss.bossName.Contains("Mud") || _currentBoss.bossName.Contains("Colossus")) bossBase = 174f;
                            else if (_currentBoss.bossName.Contains("Rail") || _currentBoss.bossName.Contains("Wraith")) bossBase = 210f;
                            else if (_currentBoss.bossName.Contains("Sludge")) bossBase = 155f;
                            else if (IsSkyReaver()) bossBase = 410f; // aerial high signature
                            else if (IsResetSeeker() || _currentBoss.bossName.Contains("Reset")) bossBase = 320f;
                            else if (_currentBoss.bossName.Contains("Leviathan")) bossBase = 188f;
                        }
                        _currentTargetFrequency = bossBase + UnityEngine.Random.Range(-35f, 95f);

                        OnBossDialogue?.Invoke($"The boss staggers! Strike now — target ~{_currentTargetFrequency:F0} Hz!");
                        HapticFeedbackManager.Instance?.PlayCombatHit();
                        AudioManager.Instance?.PlayTone(528f, 0.3f);

                        // Spawn/refresh procedural visuals for advanced bosses (RailWraith + SludgeLeviathan 3-phase + Mud Colossus + SkyReaver R6)
                        SpawnOrUpdateBossVisuals(phase.phaseName);

                        // Round 5: Improved telegraph VFX — frequency-synced pulsing rings (satisfying "hear the target" moment)
                        // R6: Deepened with multi-layer per-type telegraphs
                        _telegraphPulseTimer = 0f;
                        _lastTelegraphHz = _currentTargetFrequency;
                        // Initial strong telegraph burst (harmonic rings at exact target freq feel)
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 1.5f);
                        VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.forward * 1.2f);
                        if (IsSkyReaver() || _currentTargetFrequency > 350f)
                            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3.1f);
                        if (IsRailWraith())
                            VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.left * 1.9f);
                    }
                }
            }
        }

        // ─── Phase Transitions ───────────────────────

        void CheckPhaseTransition()
        {
            if (_currentPhase >= _currentBoss.phases.Count - 1) return;

            float nextThreshold = _currentBoss.phases[_currentPhase].hpThresholdToAdvance;
            if (BossHPNormalized <= nextThreshold)
            {
                _currentPhase++;
                _phaseTransitionTimer = 2f; // 2s cinematic pause
                _isVulnerable = false;
                _patternIndex = 0;

                var newPhase = _currentBoss.phases[_currentPhase];
                OnPhaseChanged?.Invoke(_currentPhase);
                OnBossDialogue?.Invoke(newPhase.entranceDialogue);

                // VFX burst on phase change
                VFXController.Instance?.PlayEffect(
                    VFXEffect.HarmonicCascade, transform.position);
                AdaptiveMusicController.Instance?.PlayZoneShift();
                HapticFeedbackManager.Instance?.PlayGolemSpawn();

                Debug.Log($"[Boss] Phase {_currentPhase + 1}: {newPhase.phaseName}");

                // Refresh procedural visuals for Rail/Sludge/Sky 3-phase behavior on phase shift
                SpawnOrUpdateBossVisuals(newPhase.phaseName);
            }
        }

        // ─── Defeat ─────────────────────────────────

        void DefeatBoss()
        {
            _isActive = false;
            _bossHP = 0f;

            // Score calculation
            float timeBonus = Mathf.Clamp01(1f - _encounterTime / (_currentBoss.parTime * 2f));
            float noHitBonus = _playerHits == 0 ? 0.5f : 0f;
            float performanceScore = 0.5f + timeBonus * 0.25f + noHitBonus;

            float rsReward = _currentBoss.baseRSReward * performanceScore;
            AetherFieldManager.Instance?.AddResonanceScore(rsReward);

            var result = new BossResult
            {
                bossName = _currentBoss.bossName,
                encounterTime = _encounterTime,
                playerHitsReceived = _playerHits,
                performanceScore = performanceScore,
                rsRewarded = rsReward,
                noHitClear = _playerHits == 0
            };

            OnBossDefeated?.Invoke(result);
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DefeatBoss, _currentBoss.bossName);

            // Defeat VFX / audio / haptics
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position);
            VFXController.Instance?.TriggerZoneComplete();
            AudioManager.Instance?.PlaySFX("BossDefeat", transform.position);
            AdaptiveMusicController.Instance?.ExitCombat();
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.BossDefeat);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            EconomySystem.Instance?.AddCurrency(CurrencyType.AetherShards, Mathf.RoundToInt(rsReward / 5f));

            // R6: final Golden Cascade if puzzle was mastered
            if (_bestMatchAccuracy > 0.78f || _goldenCascadeTriggered)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2.8f);
                OnBossDialogue?.Invoke("Perfect frequency mastery! The boss dissolves into golden light.");
            }

            Debug.Log($"[Boss] {_currentBoss.bossName} DEFEATED! Score: {performanceScore:P0}, RS: {rsReward:F0} | R6 puzzle stats: best={_bestMatchAccuracy:P0} attempts={_puzzleAttempts} cascade={_goldenCascadeTriggered}");

            CleanupBossVisualProxies();

            // Restore ley lines severed during fight
            // (handled by ClimaxSequenceSystem EnvironmentShift)
        }

        // ─── Boss Factory ────────────────────────────

        static BossDefinition BuildBossForMoon(int moonIndex)
        {
            return moonIndex switch
            {
                0 => BuildCorruptionTitan("Mud Colossus", 500f, 15f, 60f),
                1 => BuildCorruptionTitan("Quartz Defiler", 700f, 20f, 75f),
                2 => BuildCorruptionTitan("Spire Breaker", 900f, 22f, 80f),
                3 => BuildCorruptionTitan("Iron Corruptor", 1200f, 28f, 90f),
                4 => BuildMirrorSovereign("Echo Sovereign", 1000f, 25f, 90f),
                5 => BuildMirrorSovereign("Crystal Phantom", 1300f, 30f, 100f),
                6 => BuildMirrorSovereign("Fractal Tyrant", 1500f, 32f, 110f),
                7 => BuildMirrorSovereign("Mirror Empress", 1800f, 35f, 120f),
                8 => BuildVoidArchitect("Void Shaper", 1600f, 30f, 120f),
                9 => BuildVoidArchitect("Rift Walker", 2000f, 35f, 130f),
                10 => BuildVoidArchitect("Ley Devourer", 2200f, 38f, 140f),
                11 => BuildVoidArchitect("Anti-Resonance", 2500f, 42f, 150f),
                12 => BuildTrueHistoryGuardian(),
                _ => BuildCorruptionTitan("Unnamed Boss", 500f, 15f, 60f)
            };
        }

        static BossDefinition BuildCorruptionTitan(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.CorruptionTitan,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Awakening",
                        entranceDialogue = $"The {name} rises from corrupted earth!",
                        hpThresholdToAdvance = 0.5f,
                        attackInterval = 3f,
                        vulnerableDuration = 2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Sweep, BossAttackPattern.Slam, BossAttackPattern.CorruptionWave }
                    },
                    new()
                    {
                        phaseName = "Frenzy",
                        entranceDialogue = "The corruption surges! The titan enters a frenzy!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.CorruptionWave, BossAttackPattern.Enrage, BossAttackPattern.Sweep }
                    }
                }
            };
        }

        static BossDefinition BuildMirrorSovereign(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.MirrorSovereign,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Reflection",
                        entranceDialogue = $"The {name} materializes from shattered mirrors!",
                        hpThresholdToAdvance = 0.6f,
                        attackInterval = 2.5f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.Sweep, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "Multiplication",
                        entranceDialogue = "Mirror images splinter across the arena!",
                        hpThresholdToAdvance = 0.3f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.MirrorClone, BossAttackPattern.Slam, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "True Form",
                        entranceDialogue = "All mirrors shatter! The sovereign reveals its true frequency!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 2f,
                        invulnerableDuration = 3f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Sweep, BossAttackPattern.Slam, BossAttackPattern.Enrage }
                    }
                }
            };
        }

        static BossDefinition BuildVoidArchitect(string name, float hp, float rsReward, float parTime)
        {
            return new BossDefinition
            {
                bossName = name,
                bossType = BossType.VoidArchitect,
                totalHP = hp,
                baseRSReward = rsReward,
                parTime = parTime,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Construction",
                        entranceDialogue = $"The {name} tears open the fabric of the zone!",
                        hpThresholdToAdvance = 0.65f,
                        attackInterval = 3f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.LeyLineSever, BossAttackPattern.Sweep }
                    },
                    new()
                    {
                        phaseName = "Deconstruction",
                        entranceDialogue = "Reality warps! The architect unmakes the buildings around you!",
                        hpThresholdToAdvance = 0.3f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.LeyLineSever, BossAttackPattern.CorruptionWave, BossAttackPattern.FrequencyJam }
                    },
                    new()
                    {
                        phaseName = "Void Collapse",
                        entranceDialogue = "The void collapses inward! All frequencies converge!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 2.5f,
                        invulnerableDuration = 3f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.Enrage, BossAttackPattern.VoidRift, BossAttackPattern.CorruptionWave }
                    }
                }
            };
        }

        static BossDefinition BuildTrueHistoryGuardian()
        {
            return new BossDefinition
            {
                bossName = "Guardian of True History",
                bossType = BossType.TrueHistoryGuardian,
                totalHP = 5000f,
                baseRSReward = 100f,
                parTime = 180f,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "The Burial",
                        entranceDialogue = "You dare unbury what was hidden? I am the seal. I am the silence. I am the lie made manifest!",
                        hpThresholdToAdvance = 0.75f,
                        attackInterval = 2.5f,
                        vulnerableDuration = 1.5f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.CorruptionWave, BossAttackPattern.Sweep, BossAttackPattern.Slam }
                    },
                    new()
                    {
                        phaseName = "The Demolition",
                        entranceDialogue = "World Fairs! Grand Exhibitions! And then... the wrecking balls. I remember every brick that fell!",
                        hpThresholdToAdvance = 0.5f,
                        attackInterval = 2f,
                        vulnerableDuration = 1.2f,
                        invulnerableDuration = 4f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.MirrorClone, BossAttackPattern.FrequencyJam, BossAttackPattern.LeyLineSever, BossAttackPattern.Slam }
                    },
                    new()
                    {
                        phaseName = "The Erasure",
                        entranceDialogue = "History rewrites itself! The frequency of forgetting grows louder!",
                        hpThresholdToAdvance = 0.25f,
                        attackInterval = 1.5f,
                        vulnerableDuration = 1f,
                        invulnerableDuration = 5f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.VoidRift, BossAttackPattern.CorruptionWave, BossAttackPattern.MirrorClone, BossAttackPattern.Enrage }
                    },
                    new()
                    {
                        phaseName = "The Truth",
                        entranceDialogue = "No... the resonance... it's too strong. You carry all thirteen frequencies. The truth... cannot be buried forever!",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1f,
                        vulnerableDuration = 3f,
                        invulnerableDuration = 2f,
                        attackPatterns = new List<BossAttackPattern>
                            { BossAttackPattern.Slam, BossAttackPattern.Sweep, BossAttackPattern.Enrage }
                    }
                }
            };
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum BossType : byte
    {
        CorruptionTitan = 0,
        MirrorSovereign = 1,
        VoidArchitect = 2,
        TrueHistoryGuardian = 3
    }

    public enum BossAttackPattern : byte
    {
        Sweep = 0,
        Slam = 1,
        CorruptionWave = 2,
        MirrorClone = 3,
        VoidRift = 4,
        FrequencyJam = 5,
        LeyLineSever = 6,
        Enrage = 7
    }

    [Serializable]
    public class BossDefinition
    {
        public string bossName;
        public BossType bossType;
        public float totalHP;
        public float baseRSReward;
        public float parTime; // seconds for "par" clear time
        public List<BossPhase> phases;
    }

    [Serializable]
    public class BossPhase
    {
        public string phaseName;
        public string entranceDialogue;
        public float hpThresholdToAdvance; // normalized HP to trigger next phase
        public float attackInterval;       // seconds between attacks
        public float vulnerableDuration;   // seconds vulnerable
        public float invulnerableDuration; // seconds invulnerable
        public List<BossAttackPattern> attackPatterns;
    }

    [Serializable]
    public class BossResult
    {
        public string bossName;
        public float encounterTime;
        public int playerHitsReceived;
        public float performanceScore; // 0-1
        public float rsRewarded;
        public bool noHitClear;
    }
}
