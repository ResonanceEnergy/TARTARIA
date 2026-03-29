using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Boss Encounter System — multi-phase boss fights at Moon climaxes.
    ///
    /// Design per GDD §06 (Combat), §11 (Scripted Climaxes):
    ///   - Each Moon ends with a boss encounter before the climax sequence
    ///   - Bosses have multiple phases with unique mechanics
    ///   - Phase transitions at HP thresholds with cinematic beats
    ///   - Vulnerability windows tied to frequency-matching mechanics
    ///   - RS rewards scale with performance (no-hit bonus, time bonus)
    ///
    /// Boss types:
    ///   - CorruptionTitan (Moon 1-4): brute force + corruption AOE
    ///   - MirrorSovereign (Moon 5-8): reflection/clone mechanics
    ///   - VoidArchitect (Moon 9-12): reality-warping, ley line disruption
    ///   - TrueHistoryGuardian (Moon 13): all mechanics combined
    ///
    /// Performance budget: 2ms (within Combat 2ms budget, takes over from normal combat)
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

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float BossHPNormalized => _bossMaxHP > 0 ? _bossHP / _bossMaxHP : 0f;
        public int CurrentPhase => _currentPhase;
        public bool IsVulnerable => _isVulnerable;
        public BossDefinition CurrentBoss => _currentBoss;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
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
            { "ley_devourer", 10 }
        };

        // ─── Start / Stop ────────────────────────────

        /// <summary>Begin a boss encounter by string ID (e.g. "sludge_leviathan").</summary>
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

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            OnBossSpawned?.Invoke(_currentBoss);
            OnBossDialogue?.Invoke(_currentBoss.phases[0].entranceDialogue);

            Debug.Log($"[Boss] {_currentBoss.bossName} spawned — {_currentBoss.phases.Count} phases, {_bossMaxHP} HP");
        }

        /// <summary>Force-abort the boss encounter.</summary>
        public void AbortBoss()
        {
            _isActive = false;
            OnBossFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
        }

        void Update()
        {
            if (!_isActive) return;

            _encounterTime += Time.deltaTime;

            // Phase transition cinematic
            if (_phaseTransitionTimer > 0f)
            {
                _phaseTransitionTimer -= Time.deltaTime;
                return; // Freeze during transition
            }

            UpdateBossAI();
            UpdateVulnerability();
        }

        // ─── Damage ─────────────────────────────────

        /// <summary>Deal damage to the boss. Only effective during vulnerability.</summary>
        public void DealDamage(float damage)
        {
            if (!_isActive || !_isVulnerable) return;

            _bossHP -= damage;
            OnBossHealthChanged?.Invoke(BossHPNormalized);

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
                    VFXController.Instance?.PlayEffect(VFXController.ParticleEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    break;

                case BossAttackPattern.Slam:
                    // AOE ground slam centered on boss
                    combat?.DamagePlayer(baseDamage * 1.5f, "boss_slam");
                    VFXController.Instance?.PlayEffect(VFXController.ParticleEffect.Spark, transform.position);
                    HapticFeedbackManager.Instance?.PlayBuildingEmergence();
                    break;

                case BossAttackPattern.CorruptionWave:
                    // Expanding corruption ring — also applies zone corruption
                    combat?.DamagePlayer(baseDamage * 0.8f, "corruption_wave");
                    CorruptionSystem.Instance?.ApplyCorruption(
                        "boss_arena", _currentPhase * 5f);
                    VFXController.Instance?.PlayEffect(
                        VFXController.ParticleEffect.CorruptionPulse, transform.position);
                    break;

                case BossAttackPattern.MirrorClone:
                    // Spawns a decoy — reduced damage but disorients
                    combat?.DamagePlayer(baseDamage * 0.5f, "mirror_clone");
                    VFXController.Instance?.PlayEffect(
                        VFXController.ParticleEffect.HarmonicCascade, transform.position);
                    break;

                case BossAttackPattern.VoidRift:
                    // Opens a rift that pulls player and deals DOT
                    combat?.DamagePlayer(baseDamage * 1.2f, "void_rift");
                    VFXController.Instance?.PlayEffect(
                        VFXController.ParticleEffect.AetherVortex, transform.position);
                    break;

                case BossAttackPattern.FrequencyJam:
                    // Disables tuning for 5 seconds + minor damage
                    combat?.DamagePlayer(baseDamage * 0.3f, "freq_jam");
                    break;

                case BossAttackPattern.LeyLineSever:
                    // Severs a ley line node and deals damage
                    combat?.DamagePlayer(baseDamage * 0.6f, "ley_sever");
                    Core.LeyLineManager.Instance?.SeverNode(0);
                    VFXController.Instance?.PlayEffect(
                        VFXController.ParticleEffect.Spark, transform.position);
                    break;

                case BossAttackPattern.Enrage:
                    // Boss speeds up — halve attack interval for this phase
                    _attackCooldown *= 0.5f;
                    VFXController.Instance?.PlayEffect(
                        VFXController.ParticleEffect.CorruptionPulse, transform.position);
                    break;
            }

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
                        OnBossDialogue?.Invoke("The boss staggers! Strike now!");
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
                    VFXController.ParticleEffect.HarmonicCascade, transform.position);

                Debug.Log($"[Boss] Phase {_currentPhase + 1}: {newPhase.phaseName}");
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
            Debug.Log($"[Boss] {_currentBoss.bossName} DEFEATED! Score: {performanceScore:P0}, RS: {rsReward:F0}");

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
