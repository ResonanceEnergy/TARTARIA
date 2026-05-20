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
    /// Design per GDD §06 (Combat), §11 (Scripted Climaxes), 03C Moon Mechanics (RailWraith + Dissonance Leviathan Moon 3 escort/lullaby), 10 Roadmap Phase 3 polish + combat:
    ///   - Each boss teaches frequency puzzle mastery while the world reacts ("world sings back")
    ///   - Live player freq submissions via HarmonicCombatant (R5/R6)
    ///   - Dedicated phase AI per major boss with telegraph VFX, vuln windows, desperation
    ///   - Golden Cascade payoffs on masterful solves + cross-boss ley reactions
    ///   - Full persistence via hardened BossSaveState (R6 + R7 expanded: phaseHistory, streaks, cascades, worldHarmony)
    ///   - Moon 3 rail/leviathan synergy (internal + escort hook) + production depth
    ///
    /// R7: Significantly deepened dedicated AIs (RailWraith complex tiered swarm growth+clearing, Leviathan dynamic lullaby phase scaling+protection, SkyReaver altitude mastery+dive patterns, ResetSeeker disruption),
    ///     + new advanced FrequencyWraith variant (own shifting mirror freq puzzle + dedicated AI),
    ///     cross-boss ley-line "world sings back" system (global harmony eases nearby solves),
    ///     expanded v11+ persistence roundtrip with visual/AI continuity,
    ///     production telegraph/VFX/desparation type-specific urgency layers + richer cascades,
    ///     clean proxy upgrade hooks for KayKit/DOTS (no behavior change).
    /// Boss types: Mud Colossus, RailWraith, Dissonance/Sludge Leviathan, SkyReaver, ResetSeeker, FrequencyWraith (new R7), all Void/Mirror variants.
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

        // ─── R7: Deepened dedicated AIs, new FrequencyWraith variant, cross-boss ley reactions, expanded persistence, production polish, proxy hooks ───
        int _railWraithSwarmTier;        // 0-3: tiered growth/clearing/dmg behaviors (R7 complex swarm)
        float _leviathanLullabyStreak;   // consecutive good solves drive dynamic lullaby synergy scaling + escort protection
        int _skyReaverDiveCount;         // dive mastery tracking for altitude + high-freq patterns
        int _resetDisruptionCount;       // ResetSeeker scan/jam disruption count
        List<int> _phaseHistory;         // R7 full phase history for persistence continuity
        int _synergyStreakCount;         // R7 synergy streaks (levi + cross)
        int _cascadeCount;               // R7 cascade counts for payoff tracking + persistence
        float _worldLeyHarmonyBonus;     // per-fight contribution to global cross-boss
        GameObject _frequencyWraithVisual; // R7 new advanced enemy variant proxy (shifting mirror freq puzzle)

        // Cross-boss / ley-line "world sings back" (R7): good solves on one boss temporarily ease/empower others in session
        static float s_worldSingsBackHarmony = 0f;

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float BossHPNormalized => _bossMaxHP > 0 ? _bossHP / _bossMaxHP : 0f;
        public int CurrentPhase => _currentPhase;
        public bool IsVulnerable => _isVulnerable;
        public BossDefinition CurrentBoss => _currentBoss;
        public float CurrentTargetFrequency => _currentTargetFrequency;
        public bool IsFrequencyPuzzleActive => _frequencyPuzzleActive && _isVulnerable;
        public float WorldSingsBackHarmony => s_worldSingsBackHarmony; // R7 exposed for debug/bridge

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            _submittedFrequenciesThisFight = new List<float>();
            _phaseHistory = new List<int>();
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
            { "rail_wraith", 9 },
            // R7: new advanced enemy variant + enhanced
            { "frequency_wraith", 6 },
            { "enhanced_sludge_leviathan", 10 }
        };

        // ─── Start / Stop ────────────────────────────

        /// <summary>Begin a boss encounter by string ID (e.g. "sludge_leviathan", "sky_reaver", "rail_wraith", "frequency_wraith").</summary>
        public void SpawnBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId))
            {
                Debug.LogWarning("[Boss] SpawnBoss called with null/empty bossId.");
                return;
            }

            string key = bossId.ToLowerInvariant().Replace(' ', '_');
            if (key == "frequency_wraith" || key == "enhanced_sludge_leviathan")
            {
                // R7 special path for new variant (still uses existing moonIndex for definition, but custom build)
                StartBossForR7Variant(key);
                return;
            }
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

        void StartBossForR7Variant(string key)
        {
            // R7: dedicated builder for new advanced variant while reusing robust phase factory
            _currentBoss = (key == "frequency_wraith") ? BuildFrequencyWraithVariant() : BuildVoidArchitect("Enhanced Sludge Leviathan", 1850f, 36f, 125f);
            // proceed with standard init (reuses all R6/R7 state reset)
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

            _currentTargetFrequency = 432f;
            _frequencyPuzzleActive = false;
            _lastHealthSync = 1f;
            CleanupBossVisualProxies();

            // Round 5 + R6 + R7 resets
            _mudColossusSpecialTimer = 0f; _mudColossusQuakeCount = 0; _telegraphPulseTimer = 0f; _lastTelegraphHz = 0f;
            _railWraithSwarmTimer = 0f; _railWraithSwarmSize = 0; _railWraithSwarmTier = 0;
            _leviathanResonanceTimer = 0f; _leviathanSynergyLevel = 0; _leviathanLullabyStreak = 0f;
            _skyReaverAltitude = 4.2f; _skyReaverDiveCount = 0;
            _desperationTimer = 0f; _resetDisruptionCount = 0;
            _submittedFrequenciesThisFight.Clear(); _bestMatchAccuracy = 0f; _puzzleAttempts = 0; _goldenCascadeTriggered = false;
            _phaseHistory.Clear(); _synergyStreakCount = 0; _cascadeCount = 0; _worldLeyHarmonyBonus = 0f;

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            OnBossSpawned?.Invoke(_currentBoss);
            OnBossDialogue?.Invoke(_currentBoss.phases[0].entranceDialogue);

            Debug.Log($"[Boss] R7 {_currentBoss.bossName} spawned — variant path (freq puzzle + dedicated AI)");
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

            // R7 resets (deepened + new variant + cross + persistence)
            _railWraithSwarmTier = 0;
            _leviathanLullabyStreak = 0f;
            _skyReaverDiveCount = 0;
            _resetDisruptionCount = 0;
            if (_phaseHistory == null) _phaseHistory = new List<int>();
            _phaseHistory.Clear();
            _synergyStreakCount = 0;
            _cascadeCount = 0;
            _worldLeyHarmonyBonus = 0f;

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            OnBossSpawned?.Invoke(_currentBoss);
            OnBossDialogue?.Invoke(_currentBoss.phases[0].entranceDialogue);

            Debug.Log($"[Boss] {_currentBoss.bossName} spawned — {_currentBoss.phases.Count} phases, {_bossMaxHP} HP (R7: deepened AIs + FrequencyWraith + ley harmony)");
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
            if (_frequencyWraithVisual != null) { Destroy(_frequencyWraithVisual); _frequencyWraithVisual = null; }
        }

        // ─── Round 5: Persistence for active boss state + current target frequency ───
        // R6: Hardened for ALL current bosses (Mud, RailWraith swarm, Leviathan synergy, SkyReaver altitude, full puzzle stats)
        // R7: Expanded with phaseHistory, lullabyStreak, dive/disrupt counts, cascade, worldLeyHarmony + full re-apply continuity
        /// <summary>Serializable snapshot for SaveManager / GameLoop wiring (resumable boss encounters). v11+ R7 full state.</summary>
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

            // R7: expanded roundtrip (phase history, streaks, cascades, cross-boss harmony)
            public List<int> phaseHistory;
            public float lullabySynergyStreak;
            public int skyDiveCount;
            public int disruptionCount;
            public int cascadeCount;
            public float worldLeyHarmonyBonus;
            public int synergyStreakCount;
            public int railWraithSwarmTier;
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
                goldenCascadeTriggeredThisFight = _goldenCascadeTriggered,

                // R7 expanded
                phaseHistory = _phaseHistory != null ? new List<int>(_phaseHistory) : new List<int>(),
                lullabySynergyStreak = _leviathanLullabyStreak,
                skyDiveCount = _skyReaverDiveCount,
                disruptionCount = _resetDisruptionCount,
                cascadeCount = _cascadeCount,
                worldLeyHarmonyBonus = _worldLeyHarmonyBonus,
                synergyStreakCount = _synergyStreakCount,
                railWraithSwarmTier = _railWraithSwarmTier
            };
        }

        /// <summary>Restore mid-fight boss exactly as left (persistent satisfying encounter resume). R6: restores swarm/synergy/aerial/puzzle state for all bosses. R7: full history + streaks + harmony + visual/AI continuity.</summary>
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

            // R7 restore expanded
            _railWraithSwarmTier = Mathf.Clamp(state.railWraithSwarmTier, 0, 3);
            _leviathanLullabyStreak = Mathf.Max(0f, state.lullabySynergyStreak);
            _skyReaverDiveCount = Mathf.Max(0, state.skyDiveCount);
            _resetDisruptionCount = Mathf.Max(0, state.disruptionCount);
            _cascadeCount = Mathf.Max(0, state.cascadeCount);
            _worldLeyHarmonyBonus = Mathf.Max(0f, state.worldLeyHarmonyBonus);
            _synergyStreakCount = Mathf.Max(0, state.synergyStreakCount);
            _phaseHistory = state.phaseHistory != null ? new List<int>(state.phaseHistory) : new List<int>();
            if (_phaseHistory.Count > 0) _currentPhase = _phaseHistory[_phaseHistory.Count - 1];

            s_worldSingsBackHarmony = Mathf.Max(s_worldSingsBackHarmony, _worldLeyHarmonyBonus);

            // Rebuild visuals for resumed boss (Colossus scale sync, Rail/Sludge/Sky/Frequency phases + R7 continuity)
            if (_currentBoss != null)
            {
                string phaseName = (_currentPhase < _currentBoss.phases.Count) ? _currentBoss.phases[_currentPhase].phaseName : "Resumed";
                SpawnOrUpdateBossVisuals(phaseName);
                ReapplyR7PersistenceVisualsAndAI(); // R7: visual bob, tier visuals, harmony ease, AI state continuity
            }

            OnBossHealthChanged?.Invoke(BossHPNormalized);
            Debug.Log($"[Boss] PERSISTENCE R7: Resumed '{_currentBoss?.bossName}' phase {_currentPhase} target~{_currentTargetFrequency:F0}Hz | swarmT{_railWraithSwarmTier} synergyStreak={_leviathanLullabyStreak:F1} dives={_skyReaverDiveCount} cascades={_cascadeCount} worldHarmony={s_worldSingsBackHarmony:F2}");
        }

        /// <summary>R7: Re-apply visual proxies, AI state (tier, streaks, altitude, harmony easing) and phase continuity after load for seamless resume.</summary>
        void ReapplyR7PersistenceVisualsAndAI()
        {
            if (_currentBoss == null) return;

            // Re-position / update existing proxies with restored values
            if (IsRailWraith() && _railWraithVisual != null)
            {
                UpdateRailWraithVisualPhase(_currentBoss.phases[Mathf.Clamp(_currentPhase,0,_currentBoss.phases.Count-1)].phaseName);
            }
            if (IsSkyReaver() && _skyReaverVisual != null)
            {
                _skyReaverVisual.transform.position = transform.position + new Vector3(0, _skyReaverAltitude, 6f);
            }
            if (IsFrequencyWraith() && _frequencyWraithVisual != null)
            {
                _frequencyWraithVisual.transform.position = transform.position + new Vector3(2f, 1.5f, 0);
            }

            // Apply world harmony easing to current target (cross-boss payoff persists)
            if (s_worldSingsBackHarmony > 0.15f)
            {
                float ease = Mathf.Clamp01(s_worldSingsBackHarmony * 0.25f);
                _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + UnityEngine.Random.Range(-12f, 12f) * (1f - ease), 0.4f);
            }

            // Re-spawn type visuals if missing after load
            SpawnOrUpdateBossVisuals("ResumedR7");
        }

        bool IsMudColossus() => _currentBoss != null && _currentBoss.bossName.ToLowerInvariant().Contains("mud") && _currentBoss.bossName.ToLowerInvariant().Contains("colossus");
        // R6: dedicated per-boss type checks for full puzzle + AI coverage
        bool IsRailWraith() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("rail") || _currentBoss.bossName.ToLowerInvariant().Contains("wraith"));
        bool IsDissonanceLeviathan() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("leviathan") || _currentBoss.bossName.ToLowerInvariant().Contains("sludge"));
        bool IsSkyReaver() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("sky") || _currentBoss.bossName.ToLowerInvariant().Contains("reaver"));
        bool IsResetSeeker() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("reset") || _currentBoss.bossName.ToLowerInvariant().Contains("seeker"));
        // R7 new advanced variant
        bool IsFrequencyWraith() => _currentBoss != null && (_currentBoss.bossName.ToLowerInvariant().Contains("frequency") || _currentBoss.bossName.ToLowerInvariant().Contains("wraith_variant"));

        /// <summary>
        /// Round 4: Wire frequency puzzle submission into real combat via HarmonicStrike/ResonancePulse hooks.
        /// Call this from CombatBridge when boss is active + vulnerable: match quality drives scaled DealDamage.
        /// R6: Full per-boss puzzle integration (Rail dissonance swarm clear, Leviathan resonance synergy payoff, SkyReaver aerial dive, Golden Cascade on mastery).
        /// R7: Deepened clearing (tiered), lullaby streaks + protection, dive mastery, disruption counters, FrequencyWraith mirror, cross-boss harmony broadcast, richer cascades + nudge.
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
                // R7: deepened + new variant + cross harmony
                if (IsRailWraith())
                {
                    int tierBonus = _railWraithSwarmTier;
                    int cleared = Mathf.RoundToInt(matchQuality * 4.2f + tierBonus * 0.6f);
                    _railWraithSwarmSize = Mathf.Max(0, _railWraithSwarmSize - cleared);
                    if (cleared > 0 && _railWraithSwarmSize <= 1)
                    {
                        OnBossDialogue?.Invoke("Swarm shattered! You solved the living dissonance frequency!");
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3f);
                        if (matchQuality > 0.65f) { _synergyStreakCount++; s_worldSingsBackHarmony = Mathf.Min(1.8f, s_worldSingsBackHarmony + 0.09f); }
                    }
                    // R7 tiered freq-clearing behavior
                    if (_railWraithSwarmSize > 4 && matchQuality > 0.55f)
                    {
                        VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.left * 2f);
                        OnBossDialogue?.Invoke("Harmony echoes thin the swarm — the rails answer!");
                    }
                }

                if (IsDissonanceLeviathan())
                {
                    _leviathanSynergyLevel = Mathf.Min(6, _leviathanSynergyLevel + (matchQuality > 0.65f ? 1 : 0));
                    _leviathanLullabyStreak = (matchQuality > 0.62f) ? _leviathanLullabyStreak + 1.1f : Mathf.Max(0f, _leviathanLullabyStreak - 0.6f);
                    _synergyStreakCount = Mathf.Max(_synergyStreakCount, Mathf.RoundToInt(_leviathanLullabyStreak));
                    if (_leviathanSynergyLevel >= 3)
                    {
                        // Real mechanical + narrative payoff for good freq play during escort (Moon 3 synergy fantasy)
                        OnBossDialogue?.Invoke(_leviathanSynergyLevel >= 5 || _leviathanLullabyStreak > 3.5f ? "The rails sing with the orphans' lullaby! Full Golden resonance!" : "Leviathan resonance builds — the train feels your frequency!");
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.forward * 2.5f);
                        // Extra world react on high synergy (escalating payoff) + R7 dynamic protection
                        if (_leviathanSynergyLevel % 2 == 0 || _leviathanLullabyStreak > 2f)
                            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position);
                    }
                }

                if (IsSkyReaver())
                {
                    // Aerial frequency puzzle: high match forces dive (lowers altitude, opens bigger vuln next)
                    float preAlt = _skyReaverAltitude;
                    _skyReaverAltitude = Mathf.Max(0.6f, _skyReaverAltitude - matchQuality * 1.75f);
                    _skyReaverDiveCount++;
                    OnBossDialogue?.Invoke(matchQuality > 0.75f ? "Sky Reaver dives! Frequency mastery pulls it from the clouds!" : "Aerial lock — the reaver wavers!");
                    if (_skyReaverVisual != null)
                        _skyReaverVisual.transform.position = transform.position + new Vector3(0, _skyReaverAltitude, 6f);
                    if (preAlt > 3.2f && _skyReaverAltitude < 2.0f && matchQuality > 0.7f)
                        s_worldSingsBackHarmony = Mathf.Min(1.8f, s_worldSingsBackHarmony + 0.07f);
                }

                if (IsResetSeeker())
                {
                    // Seeker freq: strong match disrupts its seeking patterns + R7 reduction in disruption count
                    _resetDisruptionCount = Mathf.Max(0, _resetDisruptionCount - (matchQuality > 0.7f ? 2 : 1));
                    OnBossDialogue?.Invoke("Seeker pattern broken! Precise frequency shatters its scan!");
                }

                // R7: New FrequencyWraith variant dedicated puzzle behavior (mirror last solve for tension)
                if (IsFrequencyWraith())
                {
                    if (_submittedFrequenciesThisFight.Count > 0)
                    {
                        float mirror = _submittedFrequenciesThisFight[_submittedFrequenciesThisFight.Count - 1];
                        _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, mirror + UnityEngine.Random.Range(-14f, 14f), 0.55f);
                    }
                    OnBossDialogue?.Invoke("Frequency wraith mirrors your resonance — break the living echo!");
                    if (matchQuality > 0.78f) s_worldSingsBackHarmony = Mathf.Min(1.8f, s_worldSingsBackHarmony + 0.11f);
                }

                // R6: Golden Cascade payoff — the satisfying "I solved the living frequency puzzle while the world reacts"
                // R7: richer + world reactivity + cross-boss broadcast + nudge + streak/cascade tracking
                if (matchQuality > 0.85f && !_goldenCascadeTriggered)
                {
                    _goldenCascadeTriggered = true;
                    _cascadeCount++;
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 4f);
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.right * 2.2f);
                    VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.forward * 1.8f);
                    OnBossDialogue?.Invoke("GOLDEN CASCADE! You solved the living frequency — the world sings back in harmony!");
                    // Extra payoff damage + phase nudge for climax feel
                    DealDamage(22f);
                    if (_currentPhase < _currentBoss.phases.Count - 1)
                        _currentTargetFrequency += UnityEngine.Random.Range(-40f, 40f);

                    // R7: cross-boss ley reaction + freq bridge nudge for tangible "world sings"
                    s_worldSingsBackHarmony = Mathf.Min(2.2f, s_worldSingsBackHarmony + 0.28f);
                    _worldLeyHarmonyBonus = s_worldSingsBackHarmony;
                    CombatBridge.Instance?.NudgePlayerFrequencyTowardBossSolution(_currentTargetFrequency, 0.38f);
                    CombatBridge.Instance?.ApplyLeylineCrossBossResonance(0.18f); // freq bridge only helper
                }

                // Track for R6 hardened persistence + R7 counts
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
                PrepareProxyForKayKitUpgrade(_colossusVisualProxy, "mud_colossus");
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
                PrepareProxyForKayKitUpgrade(_railWraithVisual, "rail_wraith");
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
                PrepareProxyForKayKitUpgrade(_sludgeVisual, "sludge_leviathan");
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
                PrepareProxyForKayKitUpgrade(_skyReaverVisual, "sky_reaver");
            }
            // R7: New advanced FrequencyWraith variant proxy (shifting crystalline wraith, mirror freq visual)
            else if (IsFrequencyWraith() || name.Contains("frequency"))
            {
                if (_frequencyWraithVisual == null)
                {
                    _frequencyWraithVisual = new GameObject("FrequencyWraith_VisualProxy_R7");
                    _frequencyWraithVisual.transform.position = spawnPos + new Vector3(2f, 1.5f, 0);
                    var wraithBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wraithBody.transform.SetParent(_frequencyWraithVisual.transform);
                    wraithBody.transform.localScale = new Vector3(1.4f, 2.1f, 0.5f);
                    wraithBody.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.85f); // shifting violet
                    Debug.Log("[Boss] R7 FrequencyWraith advanced variant procedural proxy spawned (mirror freq puzzle).");
                }
                _frequencyWraithVisual.transform.position = spawnPos + new Vector3(2f + Mathf.Sin(Time.time * 5f) * 0.6f, 1.5f, 0);
                PrepareProxyForKayKitUpgrade(_frequencyWraithVisual, "frequency_wraith");
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
            // R7: FrequencyWraith shifting crystalline bob + freq-reactive scale (mirror puzzle visual)
            if (_frequencyWraithVisual != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 6.2f) * 0.12f;
                _frequencyWraithVisual.transform.localScale = Vector3.one * pulse;
                _frequencyWraithVisual.transform.position = transform.position + new Vector3(2f + Mathf.Sin(Time.time * 5.1f) * 0.7f, 1.6f + Mathf.Cos(Time.time * 2.9f) * 0.3f, 0);
                // Color shift toward current target for puzzle feedback
                var rend = _frequencyWraithVisual.GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = Color.Lerp(new Color(0.6f, 0.4f, 0.85f), new Color(0.95f, 0.85f, 0.4f), Mathf.Clamp01((_currentTargetFrequency - 200f) / 280f));
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

                // R7: urgency scaling by swarm tier / lullaby / altitude / world harmony (type-specific production polish)
                float urgencyMod = 1f + (_railWraithSwarmTier * 0.18f) + (_railWraithSwarmSize * 0.06f);
                if (IsDissonanceLeviathan() && _leviathanLullabyStreak > 2f) urgencyMod -= 0.35f; // lullaby eases telegraph
                if (IsSkyReaver()) urgencyMod += (4.5f - _skyReaverAltitude) * 0.09f;
                if (s_worldSingsBackHarmony > 0.4f) urgencyMod *= (1f - Mathf.Clamp01(s_worldSingsBackHarmony * 0.22f));
                pulseRate = Mathf.Clamp(pulseRate / Mathf.Clamp(urgencyMod, 0.6f, 2.4f), 0.22f, 2.1f);

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

                    // R7: more type-specific layers + world harmony golden reactivity
                    if (IsFrequencyWraith())
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, basePos + Vector3.left * 2.1f);
                    if (s_worldSingsBackHarmony > 0.55f)
                        VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, basePos + Vector3.forward * 2.6f); // "ley sings back"
                    if (IsDissonanceLeviathan() && _leviathanSynergyLevel >= 4)
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, basePos + Vector3.down * 0.9f);
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
            // R7: new advanced variant dedicated AI
            if (IsFrequencyWraith())
                UpdateFrequencyWraithDedicatedAI();

            // R6 shared desperation (all bosses)
            // R7: type-specific urgency + extra layers
            if (BossHPNormalized < 0.32f)
            {
                _desperationTimer -= Time.deltaTime;
                if (_desperationTimer <= 0f)
                {
                    _desperationTimer = 7.2f;
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position + Vector3.up * 1.1f);
                    string desperationLine = "The boss frenzies — only perfect frequency solves this now!";
                    if (IsRailWraith() && _railWraithSwarmTier >= 2) desperationLine = "The swarm frenzies in dissonance — clear it with precision!";
                    else if (IsDissonanceLeviathan() && _leviathanLullabyStreak > 1.5f) desperationLine = "Leviathan's grief peaks — your lullaby is the only shield!";
                    else if (IsSkyReaver()) desperationLine = "Sky Reaver screams from the aether — high-frequency mastery or fall!";
                    else if (IsFrequencyWraith()) desperationLine = "The wraith fractures the mirror — retune the echo!";
                    OnBossDialogue?.Invoke(desperationLine);
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
        // R7: Significantly deepened — tiered complex growth, accel if uncleared, harmony echo clearing, world harmony broadcast
        void UpdateRailWraithDedicatedAI()
        {
            if (!_isActive || !IsRailWraith()) return;

            _railWraithSwarmTimer -= Time.deltaTime;

            if (_railWraithSwarmTimer <= 0f)
            {
                _railWraithSwarmTimer = IsDissonanceLeviathan() ? 4.6f : 5.4f;
                int growth = (int)(1.6f + _currentPhase * 0.9f + _railWraithSwarmTier * 0.45f);
                if (_railWraithSwarmSize > 4) growth += 1; // accel when thick (R7 complex pattern)
                _railWraithSwarmSize = Mathf.Min(9, _railWraithSwarmSize + growth);
                _railWraithSwarmTier = Mathf.Clamp(_railWraithSwarmSize / 3, 0, 3);

                VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.up * 1.6f);
                OnBossDialogue?.Invoke(_railWraithSwarmSize > 5 ? $"The wraiths multiply (tier {_railWraithSwarmTier}) — solve the dissonance frequency to break the swarm!" : "Rail wraiths converge!");
                var combat = CombatBridge.Instance;
                if (combat != null && _railWraithSwarmSize > 1)
                    combat.DamagePlayer(3f + _railWraithSwarmSize * 0.7f + _railWraithSwarmTier * 1.1f, "rail_swarm");
            }

            // During vuln, swarm size influences target (more chaotic when thick) — R7 tier scaling
            if (_isVulnerable && _railWraithSwarmSize > 3)
            {
                float chaos = 26f + _railWraithSwarmTier * 11f;
                _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, _currentTargetFrequency + chaos, 0.13f);
            }
        }

        // ─── R7: Dedicated FrequencyWraith (new advanced enemy) — shifting mirror freq puzzle + disruption behaviors
        void UpdateFrequencyWraithDedicatedAI()
        {
            if (!_isActive || !IsFrequencyWraith()) return;

            // R7: Mirror/shift puzzle dynamic (harder when player submits)
            if (Time.frameCount % 27 == 0 && _isVulnerable)
            {
                if (_submittedFrequenciesThisFight.Count > 0)
                {
                    float last = _submittedFrequenciesThisFight[_submittedFrequenciesThisFight.Count - 1];
                    _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, last + UnityEngine.Random.Range(-22f, 22f), 0.7f);
                }
                else
                {
                    _currentTargetFrequency += UnityEngine.Random.Range(-18f, 18f);
                }
                OnBossDialogue?.Invoke("Wraith fractures the mirror frequency — stay one step ahead!");
                _resetDisruptionCount++;
            }

            // Occasional mirror clone attack feel (internal)
            if (Time.frameCount % 41 == 0 && _isVulnerable && _bestMatchAccuracy > 0.4f)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.right * 1.8f);
            }
        }

        // ─── R6: Dedicated Dissonance Leviathan AI (Moon 3 train escort climax — phases + resonance synergy) ───
        // R7: Significantly richer — dynamic lullaby streak scaling, phase transition synergy, escort protection, world harmony
        void UpdateDissonanceLeviathanDedicatedAI()
        {
            if (!_isActive || !IsDissonanceLeviathan()) return;

            _leviathanResonanceTimer -= Time.deltaTime;

            float hp = BossHPNormalized;
            int phase = _currentPhase;

            if (_leviathanResonanceTimer <= 0f)
            {
                _leviathanResonanceTimer = (phase == 0) ? 5.0f : (phase == 1 ? 3.6f : 2.9f);

                // Phase-driven resonance waves that shift target freq (player solves living puzzle)
                float shift = (hp < 0.4f ? 45f : 22f) * (phase + 1);
                _currentTargetFrequency = Mathf.Lerp(_currentTargetFrequency, 155f + UnityEngine.Random.Range(-shift, shift), 0.55f);

                VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.up * 1.3f);
                OnBossDialogue?.Invoke(hp < 0.45f ? "Leviathan screams! Match its buried grief frequency!" : "Resonance wave — retune or the rails fracture!");

                var combat = CombatBridge.Instance;
                float protection = (_leviathanSynergyLevel * 1.8f) + (_leviathanLullabyStreak * 0.9f); // R7 dynamic lullaby protection scaling
                if (combat != null)
                    combat.DamagePlayer(7f + phase * 2.5f - protection, "leviathan_wave"); // synergy reduces incoming on good play

                // High synergy (from good freq during escort) = world reacts with golden payoff + R7 streak escalation
                if ((_leviathanSynergyLevel >= 4 || _leviathanLullabyStreak > 2.2f) && _isVulnerable)
                {
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position);
                    OnBossDialogue?.Invoke("The orphans' lullaby answers your frequency! The leviathan weakens!");
                    s_worldSingsBackHarmony = Mathf.Min(2.0f, s_worldSingsBackHarmony + 0.06f);
                }
            }

            // Desperation on low HP: faster resonance shifts + telegraph emphasis + R7 lullaby payoff
            if (hp < 0.28f && Time.frameCount % 29 == 0)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position);
                if (_leviathanLullabyStreak > 3f)
                    VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2f);
            }
        }

        // ─── R6: Dedicated SkyReaver Aerial AI (high-freq aerial puzzle, altitude dives on mastery) ───
        // R7: Altitude + dive mastery — multi-pattern dives, high-freq desperation mastery rewards
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

            // Desperation dive attack that rewards aerial freq precision — R7 richer patterns
            _desperationTimer -= Time.deltaTime * 0.55f; // faster in air
            if (BossHPNormalized < 0.38f && _desperationTimer <= 0f)
            {
                _desperationTimer = 6.4f;
                _currentTargetFrequency = 410f + UnityEngine.Random.Range(-55f, 90f); // high aerial signature
                _skyReaverDiveCount++;
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3.5f);
                OnBossDialogue?.Invoke(_skyReaverDiveCount > 2 ? "Sky Reaver performs a mastery dive! Match the aether cry!" : "Sky Reaver dives from the aether! Match its high-frequency cry!");
                var c = CombatBridge.Instance;
                if (c != null) c.DamagePlayer(11f + (_skyReaverAltitude < 1.8f ? 4f : 0), "sky_dive");
                if (_skyReaverAltitude < 1.5f && _skyReaverDiveCount % 2 == 0) s_worldSingsBackHarmony += 0.05f;
            }
        }

        // ─── R6: Dedicated ResetSeeker AI (scanning/seeking patterns disrupted by precise freq) ───
        // R7: Richer disruption — freq lock/jam patterns, retune penalties, scan escalation
        void UpdateResetSeekerDedicatedAI()
        {
            if (!_isActive || !IsResetSeeker()) return;

            if (Time.frameCount % 44 == 0 && _isVulnerable)
            {
                _currentTargetFrequency += UnityEngine.Random.Range(-19f, 19f);
                _resetDisruptionCount++;
                OnBossDialogue?.Invoke(_resetDisruptionCount > 4 ? "Seeker retunes its scan — stay precise or lose the lock!" : "Seeker retunes its scan — stay precise!");
            }

            // R7: occasional jam that punishes low accuracy (internal state)
            if (Time.frameCount % 67 == 0 && _isVulnerable && _bestMatchAccuracy < 0.45f)
            {
                _puzzleAttempts += 1;
                VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position + Vector3.right * 1.2f);
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
                        // R7: cross-boss harmony eases target window + type-specific base + new wraith
                        float bossBase = 280f;
                        if (_currentBoss != null)
                        {
                            if (_currentBoss.bossName.Contains("Mud") || _currentBoss.bossName.Contains("Colossus")) bossBase = 174f;
                            else if (_currentBoss.bossName.Contains("Rail") || _currentBoss.bossName.Contains("Wraith")) bossBase = 210f;
                            else if (_currentBoss.bossName.Contains("Sludge")) bossBase = 155f;
                            else if (IsSkyReaver()) bossBase = 410f; // aerial high signature
                            else if (IsResetSeeker() || _currentBoss.bossName.Contains("Reset")) bossBase = 320f;
                            else if (_currentBoss.bossName.Contains("Leviathan")) bossBase = 188f;
                            else if (IsFrequencyWraith()) bossBase = 295f; // R7 shifting mirror base
                        }
                        float harmonyEase = Mathf.Clamp01(s_worldSingsBackHarmony * 0.32f);
                        float range = 95f * (1f - harmonyEase * 0.45f); // R7 world sings eases the puzzle when prior mastery high
                        _currentTargetFrequency = bossBase + UnityEngine.Random.Range(-35f * (1f - harmonyEase * 0.6f), range);

                        OnBossDialogue?.Invoke($"The boss staggers! Strike now — target ~{_currentTargetFrequency:F0} Hz!" + (harmonyEase > 0.2f ? " (Ley lines resonate — the world sings back)" : ""));
                        HapticFeedbackManager.Instance?.PlayCombatHit();
                        AudioManager.Instance?.PlayTone(528f, 0.3f);

                        // Spawn/refresh procedural visuals for advanced bosses (RailWraith + SludgeLeviathan 3-phase + Mud Colossus + SkyReaver + R7 FrequencyWraith)
                        SpawnOrUpdateBossVisuals(phase.phaseName);

                        // Round 5: Improved telegraph VFX — frequency-synced pulsing rings (satisfying "hear the target" moment)
                        // R7: Deepened with multi-layer per-type telegraphs + urgency + harmony golden
                        _telegraphPulseTimer = 0f;
                        _lastTelegraphHz = _currentTargetFrequency;
                        // Initial strong telegraph burst (harmonic rings at exact target freq feel)
                        VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 1.5f);
                        VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.forward * 1.2f);
                        if (IsSkyReaver() || _currentTargetFrequency > 350f)
                            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 3.1f);
                        if (IsRailWraith())
                            VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position + Vector3.left * 1.9f);
                        if (IsFrequencyWraith())
                            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.right * 2.3f);
                        if (s_worldSingsBackHarmony > 0.3f)
                            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.up * 2.4f);
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
                _phaseHistory.Add(_currentPhase); // R7 persistence history
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

                // Refresh procedural visuals for Rail/Sludge/Sky/Frequency 3-phase behavior on phase shift
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
            // R7: richer mastery celebration + leave world harmony for cross-boss payoff on next encounter
            if (_bestMatchAccuracy > 0.78f || _goldenCascadeTriggered)
            {
                VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2.8f);
                VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position + Vector3.forward * 1.9f);
                OnBossDialogue?.Invoke("Perfect frequency mastery! The boss dissolves into golden light. The world remembers.");
                s_worldSingsBackHarmony = Mathf.Min(2.4f, s_worldSingsBackHarmony + 0.35f);
                _worldLeyHarmonyBonus = s_worldSingsBackHarmony;
            }

            Debug.Log($"[Boss] {_currentBoss.bossName} DEFEATED! Score: {performanceScore:P0}, RS: {rsReward:F0} | R7 puzzle: best={_bestMatchAccuracy:P0} attempts={_puzzleAttempts} cascades={_cascadeCount} worldHarmony={s_worldSingsBackHarmony:F2}");

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

        // R7: Dedicated builder for new FrequencyWraith advanced variant (shifting mirror puzzle boss)
        static BossDefinition BuildFrequencyWraithVariant()
        {
            return new BossDefinition
            {
                bossName = "Frequency Wraith",
                bossType = BossType.MirrorSovereign,
                totalHP = 1350f,
                baseRSReward = 32f,
                parTime = 95f,
                phases = new List<BossPhase>
                {
                    new()
                    {
                        phaseName = "Echo Mirror",
                        entranceDialogue = "The Frequency Wraith materializes — it wears your resonance like a second skin!",
                        hpThresholdToAdvance = 0.58f,
                        attackInterval = 2.4f,
                        vulnerableDuration = 2.1f,
                        invulnerableDuration = 4.2f,
                        attackPatterns = new List<BossAttackPattern> { BossAttackPattern.MirrorClone, BossAttackPattern.FrequencyJam, BossAttackPattern.Sweep }
                    },
                    new()
                    {
                        phaseName = "Fractured Song",
                        entranceDialogue = "The mirror shatters! The wraith now sings your past solves against you.",
                        hpThresholdToAdvance = 0.22f,
                        attackInterval = 1.9f,
                        vulnerableDuration = 1.6f,
                        invulnerableDuration = 4.8f,
                        attackPatterns = new List<BossAttackPattern> { BossAttackPattern.MirrorClone, BossAttackPattern.VoidRift, BossAttackPattern.FrequencyJam, BossAttackPattern.Enrage }
                    },
                    new()
                    {
                        phaseName = "True Echo",
                        entranceDialogue = "One final mirror. Solve yourself to free the frequency.",
                        hpThresholdToAdvance = 0f,
                        attackInterval = 1.4f,
                        vulnerableDuration = 2.4f,
                        invulnerableDuration = 3.2f,
                        attackPatterns = new List<BossAttackPattern> { BossAttackPattern.Slam, BossAttackPattern.MirrorClone, BossAttackPattern.FrequencyJam }
                    }
                }
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

        // ─── R7: Clean proxy upgrade preparation hook (zero logic change, ready for KayKit/DOTS)
        void PrepareProxyForKayKitUpgrade(GameObject proxy, string bossKey)
        {
            if (proxy == null) return;
            proxy.name = $"{bossKey}_VisualProxy_R7Ready";

            // R7 production hook: future visual upgrade path without touching AI/telegraph/persistence.
            // Example intended usage (commented, never changes current primitive behavior):
            //   var kayKitPrefab = Resources.Load<GameObject>($"KayKit/AdvancedEnemies/{bossKey}");
            //   if (kayKitPrefab != null) {
            //       var upgraded = Instantiate(kayKitPrefab, proxy.transform.position, proxy.transform.rotation, proxy.transform);
            //       upgraded.transform.localScale = proxy.transform.localScale;
            //       // destroy or disable primitive children, transfer any runtime state (color/scale hooks)
            //   }
            // Or for DOTS: convert to entity with custom mesh renderer while preserving all boss state references.

            // Marker component or tag could be added here for editor tooling / upgrade pipeline.
            // Current runtime, VFX, persistence, and dedicated AI remain 100% unchanged.
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
