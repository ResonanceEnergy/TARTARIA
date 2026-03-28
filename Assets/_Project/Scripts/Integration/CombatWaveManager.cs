using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Combat Wave Manager — orchestrates multi-wave enemy encounters
    /// triggered by RS thresholds, zone entry, or quest events.
    ///
    /// Wave Composition per GDD §06:
    ///   - Mud Golems: melee brawlers, stun after 3 Resonance Pulses
    ///   - Fractal Wraiths: phase/materialise cycle, Aether drain
    ///   - Mirror Wraiths: reflect last 3 attacks, teleport behind player
    ///
    /// Each wave has spawn delays, enemy type mix, and completion rewards.
    /// Boss encounters use the BossEncounterSystem separately.
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

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
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

            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);

            _encounterActive = false;
            _currentWaveIndex = -1;
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            Debug.Log("[CombatWave] Encounter aborted.");
        }

        /// <summary>
        /// Build a standard zone encounter scaled to Moon difficulty.
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

                // Wave 0: Mud Golems only
                if (w == 0 || moonIndex < 2)
                {
                    wave.spawns.Add(new WaveSpawn
                    {
                        enemyType = EnemyTypeId.MudGolem,
                        count = baseCount,
                        healthMultiplier = healthMultiplier,
                        spawnDelay = 0f
                    });
                }
                // Later waves: mixed composition
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
            // For now, log spawn — CombatBridge/ECS will handle actual entity creation.
            Debug.Log($"[CombatWave] Spawning {type} at {position} (HP ×{healthMultiplier:F1})");

            // VFX for spawn
            VFXController.Instance?.PlayEnemyDissolution(position);
        }

        void DistributeWaveReward()
        {
            if (_currentWaveIndex < 0 || _currentWaveIndex >= _waves.Count) return;
            float reward = _waves[_currentWaveIndex].rsReward;
            if (reward > 0)
                GameLoopController.Instance?.QueueRSReward(reward, $"wave_{_currentWaveIndex + 1}");
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
            DialogueManager.Instance?.PlayContextDialogue("combat_victory");

            Debug.Log("[CombatWave] Encounter complete!");
        }
    }

    // ─── Data Structures ─────────────────────────

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
        TitanGolem = 16
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
