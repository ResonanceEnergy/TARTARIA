using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Combat Wave Manager — orchestrates multi-wave enemy encounters
    /// triggered by RS thresholds, zone entry, or quest events.
    ///
    /// Wave Composition per GDD §06 + Moon 2 Crystal Caverns expansion:
    ///   - Mud Golems: melee brawlers, stun after 3 Resonance Pulses
    ///   - Fractal Wraiths / Mirror Wraiths: Moon 2 base
    ///   - NEW Moon 2 exclusive: CrystalShardling, VeinCrawler, ResonanceDisruptor, WindveilPhantom, GravityPillar
    ///     These make the Crystalline Caverns dangerous through direct use of crystals/veins/wind/gravity/narrow corridors.
    ///     Distinct from Echohaven open arenas and Moon 3 linear rail combat.
    /// </summary>
    public class CombatWaveManager : MonoBehaviour
    {
        public static CombatWaveManager Instance { get; private set; }

        public event Action<int> OnWaveStarted;       // waveIndex
        public event Action<int> OnWaveCleared;        // waveIndex
        public event Action OnAllWavesCleared;

        [Header("Settings")]
        [SerializeField] float spawnRadius = 15f;
        [SerializeField] float timeBetweenWaves = 5f;
        [SerializeField] float spawnStagger = 0.5f;

        readonly List<WaveDefinition> _waves = new();
        int _currentWaveIndex = -1;
        int _enemiesRemaining;
        bool _encounterActive;
        Coroutine _spawnCoroutine;
        Vector3 _encounterCenter;

        public bool IsEncounterActive => _encounterActive;
        public int CurrentWaveIndex => _currentWaveIndex;
        public int EnemiesRemaining => _enemiesRemaining;
        public int TotalWaves => _waves.Count;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (Instance == this) Instance = null;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Start a multi-wave encounter at a world position.
        /// </summary>
        public void StartEncounter(WaveEncounterDef encounter, Vector3 center)
        {
            if (_encounterActive) return;
            if (encounter == null || encounter.waves == null || encounter.waves.Count == 0) return;

            _waves.Clear();
            _waves.AddRange(encounter.waves);
            _encounterCenter = center;
            _encounterActive = true;
            _currentWaveIndex = -1;

            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            AdaptiveMusicController.Instance?.PlayCombatStart();

            StartNextWave();

            Debug.Log($"[CombatWave] Encounter started: {encounter.encounterId} ({_waves.Count} waves)");
        }

        /// <summary>
        /// Call when an enemy from the current wave is defeated.
        /// </summary>
        public void OnEnemyDefeated()
        {
            if (!_encounterActive) return;
            _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);

            Debug.Log($"[CombatWave] Enemy defeated. Remaining: {_enemiesRemaining}");

            if (_enemiesRemaining <= 0)
            {
                OnWaveCleared?.Invoke(_currentWaveIndex);
                DistributeWaveReward();

                if (_currentWaveIndex + 1 < _waves.Count)
                {
                    // Next wave after delay
                    StartCoroutine(DelayedNextWave());
                }
                else
                {
                    CompleteEncounter();
                }
            }
        }

        /// <summary>
        /// Abort the current encounter (used for retreat / zone transition).
        /// </summary>
        public void AbortEncounter()
        {
            if (!_encounterActive) return;

            StopAllCoroutines();
            _spawnCoroutine = null;

            _encounterActive = false;
            _currentWaveIndex = -1;
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            AdaptiveMusicController.Instance?.ExitCombat();

            Debug.Log("[CombatWave] Encounter aborted.");
        }

        /// <summary>
        /// Build a standard zone encounter scaled to Moon difficulty.
        /// For Moon 2 (moonIndex==1) heavily features the 5 new crystal/corruption enemies
        /// and environment-synergistic patterns.
        /// </summary>
        public static WaveEncounterDef BuildZoneEncounter(int moonIndex, string encounterId)
        {
            var encounter = new WaveEncounterDef
            {
                encounterId = encounterId,
                waves = new List<WaveDefinition>()
            };

            int waveCount = Mathf.Clamp(1 + moonIndex / 3, 1, 5);
            for (int w = 0; w < waveCount; w++)
            {
                var wave = new WaveDefinition
                {
                    waveIndex = w,
                    spawns = new List<WaveSpawn>()
                };

                // Base enemy count scales with Moon
                int baseCount = 2 + moonIndex;
                float healthMultiplier = 1f + moonIndex * 0.15f;

                // Moon 2 Crystalline Caverns special composition (domain exclusive)
                if (moonIndex == 1)
                {
                    // Moon 2: Crystal theme — use new enemies + classic wraiths in env-leveraging mixes
                    // Wave 0: Shardlings + Mud (corridor swarm starter)
                    if (w == 0)
                    {
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.MudGolem, count = Mathf.Max(2, baseCount / 2), healthMultiplier = healthMultiplier, spawnDelay = 0f });
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = baseCount, healthMultiplier = healthMultiplier * 0.8f, spawnDelay = 0.8f });
                    }
                    else
                    {
                        // Mixed crystal horror: Vein + Wind + Disruptor + GravityPillar + classic
                        int shard = Mathf.Max(2, baseCount / 2);
                        int vein = Mathf.Max(1, baseCount / 3);
                        int wind = Mathf.Max(1, baseCount / 4);
                        int disrupt = 1;
                        int grav = (w == waveCount - 1) ? 1 : 0; // climax wave has GravityPillar

                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = shard, healthMultiplier = healthMultiplier, spawnDelay = 0f });
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = vein, healthMultiplier = healthMultiplier, spawnDelay = 0.7f });
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.WindveilPhantom, count = wind, healthMultiplier = healthMultiplier, spawnDelay = 1.2f });
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.ResonanceDisruptor, count = disrupt, healthMultiplier = healthMultiplier * 1.1f, spawnDelay = 1.5f });
                        if (grav > 0)
                            wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.GravityPillar, count = grav, healthMultiplier = healthMultiplier * 1.3f, spawnDelay = 2.2f });

                        // Sprinkle classic Moon2 wraiths for continuity
                        wave.spawns.Add(new WaveSpawn { enemyType = EnemyTypeId.FractalWraith, count = Mathf.Max(1, baseCount / 3), healthMultiplier = healthMultiplier, spawnDelay = 0.4f });
                    }
                }
                else if (w == 0 || moonIndex < 2)
                {
                    wave.spawns.Add(new WaveSpawn
                    {
                        enemyType = EnemyTypeId.MudGolem,
                        count = baseCount,
                        healthMultiplier = healthMultiplier,
                        spawnDelay = 0f
                    });
                }
                // Later waves: mixed composition for non-Moon2
                else
                {
                    int golemCount = Mathf.Max(1, baseCount / 2);
                    int wraithCount = baseCount - golemCount;

                    wave.spawns.Add(new WaveSpawn
                    {
                        enemyType = EnemyTypeId.MudGolem,
                        count = golemCount,
                        healthMultiplier = healthMultiplier,
                        spawnDelay = 0f
                    });

                    if (moonIndex >= 3)
                    {
                        int fractalCount = Mathf.Max(1, wraithCount / 2);
                        int mirrorCount = wraithCount - fractalCount;

                        wave.spawns.Add(new WaveSpawn
                        {
                            enemyType = EnemyTypeId.FractalWraith,
                            count = fractalCount,
                            healthMultiplier = healthMultiplier,
                            spawnDelay = 1f
                        });

                        if (moonIndex >= 5 && mirrorCount > 0)
                        {
                            wave.spawns.Add(new WaveSpawn
                            {
                                enemyType = EnemyTypeId.MirrorWraith,
                                count = mirrorCount,
                                healthMultiplier = healthMultiplier,
                                spawnDelay = 2f
                            });
                        }
                    }
                    else
                    {
                        wave.spawns.Add(new WaveSpawn
                        {
                            enemyType = EnemyTypeId.FractalWraith,
                            count = wraithCount,
                            healthMultiplier = healthMultiplier,
                            spawnDelay = 1f
                        });
                    }
                }

                // RS reward scales with wave difficulty
                wave.rsReward = 5f + w * 3f + moonIndex * 2f;
                encounter.waves.Add(wave);
            }

            return encounter;
        }

        /// <summary>
        /// Moon 2 exclusive: Creates 4 memorable combat encounters designed around
        /// the Crystalline Caverns environment (crystals, veins, wind, gravity, narrow corridors).
        /// These are called by MoonMechanicActivator for the DissonancePurge mechanic on Moon 2.
        /// Each encounter tells a micro-story of corruption fighting back using the living crystal.
        /// </summary>
        public static WaveEncounterDef CreateMoon2CrystalEncounter(string variant, Vector3 centerHint)
        {
            var encounter = new WaveEncounterDef { encounterId = "moon2_crystal_" + variant, waves = new List<WaveDefinition>() };

            switch (variant)
            {
                case "VeinChoke": // Memorable 1: Narrow crystal corridor swarm + gravity drops
                    // Use Shardlings for density, VeinCrawlers for vertical ambushes from veins, classic wraiths
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 0,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 8, healthMultiplier = 0.9f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = 3, healthMultiplier = 1.0f, spawnDelay = 1.2f },
                            new WaveSpawn { enemyType = EnemyTypeId.FractalWraith, count = 2, healthMultiplier = 1.1f, spawnDelay = 2.0f }
                        },
                        rsReward = 22f
                    });
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 1,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 5, healthMultiplier = 1.0f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = 4, healthMultiplier = 1.15f, spawnDelay = 0.6f },
                            new WaveSpawn { enemyType = EnemyTypeId.GravityPillar, count = 1, healthMultiplier = 1.4f, spawnDelay = 3.0f } // climax gravity in choke
                        },
                        rsReward = 28f
                    });
                    break;

                case "WindGallery": // Memorable 2: Wind tunnels + phantoms + disruptor echoes
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 0,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.WindveilPhantom, count = 4, healthMultiplier = 1.0f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.ResonanceDisruptor, count = 2, healthMultiplier = 1.1f, spawnDelay = 1.5f },
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 4, healthMultiplier = 0.85f, spawnDelay = 0.8f }
                        },
                        rsReward = 24f
                    });
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 1,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.WindveilPhantom, count = 5, healthMultiplier = 1.2f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.ResonanceDisruptor, count = 3, healthMultiplier = 1.25f, spawnDelay = 1.0f },
                            new WaveSpawn { enemyType = EnemyTypeId.MirrorWraith, count = 1, healthMultiplier = 1.3f, spawnDelay = 2.5f }
                        },
                        rsReward = 30f
                    });
                    break;

                case "GravityNexus": // Memorable 3: Gravity well chamber with pillar + support
                    // Heavy use of GravityPillar + Giant Mode opportunity; Wind/Disruptors to punish bad positioning
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 0,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.GravityPillar, count = 1, healthMultiplier = 1.5f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 6, healthMultiplier = 0.95f, spawnDelay = 1.8f },
                            new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = 2, healthMultiplier = 1.1f, spawnDelay = 0.9f }
                        },
                        rsReward = 26f
                    });
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 1,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.GravityPillar, count = 1, healthMultiplier = 1.6f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.ResonanceDisruptor, count = 2, healthMultiplier = 1.2f, spawnDelay = 1.0f },
                            new WaveSpawn { enemyType = EnemyTypeId.WindveilPhantom, count = 3, healthMultiplier = 1.15f, spawnDelay = 2.2f }
                        },
                        rsReward = 32f
                    });
                    break;

                case "ResonanceHeart": // Memorable 4: Full multi-type symphony in crystal cathedral heart (climax feel)
                    // All 5 new + classic Moon2 wraiths. Uses every environmental element. 3 waves.
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 0,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 7, healthMultiplier = 0.9f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = 3, healthMultiplier = 1.05f, spawnDelay = 0.5f },
                            new WaveSpawn { enemyType = EnemyTypeId.FractalWraith, count = 2, healthMultiplier = 1.1f, spawnDelay = 1.0f }
                        },
                        rsReward = 25f
                    });
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 1,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.WindveilPhantom, count = 3, healthMultiplier = 1.1f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.ResonanceDisruptor, count = 2, healthMultiplier = 1.15f, spawnDelay = 0.8f },
                            new WaveSpawn { enemyType = EnemyTypeId.MirrorWraith, count = 2, healthMultiplier = 1.2f, spawnDelay = 1.6f }
                        },
                        rsReward = 28f
                    });
                    encounter.waves.Add(new WaveDefinition
                    {
                        waveIndex = 2,
                        spawns = new List<WaveSpawn>
                        {
                            new WaveSpawn { enemyType = EnemyTypeId.GravityPillar, count = 1, healthMultiplier = 1.7f, spawnDelay = 0f },
                            new WaveSpawn { enemyType = EnemyTypeId.VeinCrawler, count = 4, healthMultiplier = 1.2f, spawnDelay = 1.2f },
                            new WaveSpawn { enemyType = EnemyTypeId.CrystalShardling, count = 5, healthMultiplier = 1.0f, spawnDelay = 2.0f }
                        },
                        rsReward = 35f
                    });
                    break;

                default:
                    // Fallback to standard Moon2 mix
                    return BuildZoneEncounter(1, variant);
            }

            return encounter;
        }

        // ─── Internal ────────────────────────────────

        void StartNextWave()
        {
            _currentWaveIndex++;
            if (_currentWaveIndex >= _waves.Count) { CompleteEncounter(); return; }

            var wave = _waves[_currentWaveIndex];
            _enemiesRemaining = 0;
            foreach (var spawn in wave.spawns)
                _enemiesRemaining += spawn.count;

            OnWaveStarted?.Invoke(_currentWaveIndex);

            // HUD notification
            HUDController.Instance?.ShowInteractionPrompt(
                $"Wave {_currentWaveIndex + 1}/{_waves.Count} — {_enemiesRemaining} enemies incoming!");

            _spawnCoroutine = StartCoroutine(SpawnWaveEnemies(wave));

            Debug.Log($"[CombatWave] Wave {_currentWaveIndex + 1} started: {_enemiesRemaining} enemies");
        }

        IEnumerator SpawnWaveEnemies(WaveDefinition wave)
        {
            foreach (var spawn in wave.spawns)
            {
                if (spawn.spawnDelay > 0)
                    yield return new WaitForSeconds(spawn.spawnDelay);

                for (int i = 0; i < spawn.count; i++)
                {
                    Vector3 spawnPos = _encounterCenter + UnityEngine.Random.insideUnitSphere * spawnRadius;
                    spawnPos.y = _encounterCenter.y;

                    SpawnEnemy(spawn.enemyType, spawnPos, spawn.healthMultiplier);

                    if (spawnStagger > 0)
                        yield return new WaitForSeconds(spawnStagger);
                }
            }
        }

        void SpawnEnemy(EnemyTypeId type, Vector3 position, float healthMultiplier)
        {
            // In a full implementation, this instantiates prefabs or creates ECS entities.
            // For now, log spawn — CombatBridge/ECS will handle actual entity creation via the DOTS EnemyType mapping.
            Debug.Log($"[CombatWave] Spawning {type} at {position} (HP ×{healthMultiplier:F1}) — Moon 2 crystal variant active when applicable");

            // VFX for spawn — crystal-themed for Moon 2 types
            VFXController.Instance?.PlayEnemyDissolution(position);
            AudioManager.Instance?.PlaySFX("EnemySpawn", position);
        }

        /// <summary>Spawn a single enemy at position (debug console).</summary>
        public void SpawnSingleEnemy(Vector3 position)
        {
            SpawnEnemy(EnemyTypeId.MudGolem, position, 1f);
        }

        void DistributeWaveReward()
        {
            if (_currentWaveIndex < 0 || _currentWaveIndex >= _waves.Count) return;
            float reward = _waves[_currentWaveIndex].rsReward;
            if (reward > 0)
            {
                GameLoopController.Instance?.QueueRSReward(reward, $"wave_{_currentWaveIndex + 1}");
                HapticFeedbackManager.Instance?.PlayCombatHit();
            }
        }

        IEnumerator DelayedNextWave()
        {
            HUDController.Instance?.ShowInteractionPrompt(
                $"Wave {_currentWaveIndex + 1} cleared! Next wave in {timeBetweenWaves:F0}s...");

            yield return new WaitForSeconds(timeBetweenWaves);

            HUDController.Instance?.HideInteractionPrompt();
            StartNextWave();
        }

        void CompleteEncounter()
        {
            _encounterActive = false;
            OnAllWavesCleared?.Invoke();

            HUDController.Instance?.ShowInteractionPrompt("All waves cleared! Victory!");
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            // Haptics + VFX
            HapticFeedbackManager.Instance?.PlayGolemDeath();
            AdaptiveMusicController.Instance?.ExitCombat();
            Save.SaveManager.Instance?.MarkDirty();
            DialogueManager.Instance?.PlayContextDialogue("combat_victory");
            AchievementSystem.Instance?.CheckEnemyDefeated(_currentWaveIndex, "wave", false);

            Debug.Log("[CombatWave] Encounter complete!");
        }

        // ─── Save / Load ────────────────────────────

        public CombatWaveSaveData GetSaveData()
        {
            return new CombatWaveSaveData
            {
                encounterActive = _encounterActive,
                currentWaveIndex = _currentWaveIndex,
                enemiesRemaining = _enemiesRemaining,
                totalWaves = _waves.Count,
                encounterCenter = new Save.SerializableVector3(_encounterCenter)
            };
        }

        public void LoadSaveData(CombatWaveSaveData data)
        {
            _encounterActive = data.encounterActive;
            _currentWaveIndex = data.currentWaveIndex;
            _enemiesRemaining = data.enemiesRemaining;
            if (data.encounterCenter.x != 0 || data.encounterCenter.y != 0 || data.encounterCenter.z != 0)
                _encounterCenter = data.encounterCenter.ToVector3();
        }

        [Serializable]
        public class CombatWaveSaveData
        {
            public bool encounterActive;
            public int currentWaveIndex;
            public int enemiesRemaining;
            public int totalWaves;
            public Save.SerializableVector3 encounterCenter;
        }
    }

    // ─── Data Structures (Moon 2 Crystal Caverns enemies integrated) ─────────────────────────

    public enum EnemyTypeId : byte
    {
        MudGolem = 0,
        FractalWraith = 1,
        MirrorWraith = 2,
        RailWraith = 3,
        DissonanceHarvester = 4,
        DissonanceLeviathan = 5,
        SiegeGolem = 6,
        HarmonicParasite = 7,
        DissonantConductor = 8,
        CorruptedCraft = 9,
        SkyReaver = 10,
        ProphecyGuardian = 11,
        ResetSeeker = 12,
        TemporalWraith = 13,
        LivingSludge = 14,
        SludgeLeviathan = 15,
        TitanGolem = 16,
        FrequencyWraith = 17,

        // Moon 2 Crystalline Caverns — 5 new enemy types (corruption / crystal / dissonance)
        CrystalShardling = 18,
        VeinCrawler = 19,
        ResonanceDisruptor = 20,
        WindveilPhantom = 21,
        GravityPillar = 22
    }

    [Serializable]
    public class WaveEncounterDef
    {
        public string encounterId;
        public List<WaveDefinition> waves = new();
    }

    [Serializable]
    public class WaveDefinition
    {
        public int waveIndex;
        public List<WaveSpawn> spawns = new();
        public float rsReward;
    }

    [Serializable]
    public class WaveSpawn
    {
        public EnemyTypeId enemyType;
        public int count;
        public float healthMultiplier = 1f;
        public float spawnDelay;
    }
}
