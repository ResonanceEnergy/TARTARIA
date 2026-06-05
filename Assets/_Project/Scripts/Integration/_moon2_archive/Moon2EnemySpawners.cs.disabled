using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using UnityEngine.AI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Enemy Spawners — Dissonance Defenders (crystalline corrupted entities)
    /// Crystal cavern enemies that spawn near dissonance crystals, harder than Moon 1 golems
    /// </summary>
    [DefaultExecutionOrder(-86)]
    public class Moon2EnemySpawners : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] GameObject dissonanceDefenderPrefab;
        [SerializeField] GameObject crystalSentinelPrefab;  // Elite variant
        
        [Header("Spawn Settings")]
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] int maxActiveDefenders = 8;
        [SerializeField] float spawnInterval = 40f;  // Slightly faster than Moon 1
        [SerializeField] float spawnRadius = 60f;
        [SerializeField] int killGoal = 15;  // Higher than Moon 1
        
        [Header("Elite Spawning")]
        [SerializeField] float eliteSpawnChance = 0.15f;
        [SerializeField] int maxElites = 2;
        
        readonly List<GameObject> _activeDefenders = new();
        int _killCount;
        float _spawnTimer;
        bool _tutorialComplete;
        
        void Start()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            
            // Spawn initial wave
            SpawnInitialWave();
            
            Debug.Log("[Moon2EnemySpawners] ✅ Initialized - Dissonance Defenders spawning system active");
        }
        
        void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        }
        
        void Update()
        {
            // Procedural spawning
            _spawnTimer += Time.deltaTime;
            
            if (_spawnTimer >= spawnInterval && _activeDefenders.Count < maxActiveDefenders)
            {
                SpawnDefender();
                _spawnTimer = 0f;
            }
            
            // Clean up dead references
            _activeDefenders.RemoveAll(d => d == null);
        }
        
        void SpawnInitialWave()
        {
            // Spawn 3 defenders at start
            for (int i = 0; i < 3; i++)
            {
                SpawnDefender();
            }
        }
        
        void SpawnDefender()
        {
            if (dissonanceDefenderPrefab == null) return;
            
            Vector3 spawnPos = GetSpawnPosition();
            
            // Elite chance
            bool isElite = Random.value < eliteSpawnChance && CountElites() < maxElites;
            GameObject prefabToSpawn = isElite && crystalSentinelPrefab != null ? 
                crystalSentinelPrefab : dissonanceDefenderPrefab;
            
            GameObject defender = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);
            defender.name = isElite ? $"CrystalSentinel_{_activeDefenders.Count}" : $"DissonanceDefender_{_activeDefenders.Count}";
            
            // Setup NavMeshAgent
            NavMeshAgent agent = defender.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = isElite ? 3.5f : 2.8f;  // Faster than Moon 1
                agent.acceleration = 10f;
                agent.angularSpeed = 180f;
            }
            
            // Add to tracking
            _activeDefenders.Add(defender);
            
            // Spawn VFX
            SpawnCrystalShatterEffect(spawnPos);
            
            Debug.Log($"[Moon2EnemySpawners] Spawned {(isElite ? "Crystal Sentinel" : "Dissonance Defender")} at {spawnPos}");
        }
        
        Vector3 GetSpawnPosition()
        {
            // Prefer spawn points if assigned
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (spawnPoint != null)
                {
                    return spawnPoint.position + Random.insideUnitSphere * 5f;
                }
            }
            
            // Fallback: random circle around player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 center = player != null ? player.transform.position : Vector3.zero;
            
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = center + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            // Raycast to ground
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                return hit.point;
            }
            
            return spawnPos;
        }
        
        void SpawnCrystalShatterEffect(Vector3 position)
        {
            // Purple crystal shatter particles
            GameObject vfx = new GameObject("CrystalShatterVFX");
            vfx.transform.position = position;
            
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 0.5f;
            main.startSpeed = 8f;
            main.startSize = 0.3f;
            main.startColor = new Color(0.6f, 0.2f, 0.8f);  // Purple
            main.maxParticles = 30;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });
            
            Destroy(vfx, 2f);
        }
        
        int CountElites()
        {
            int count = 0;
            foreach (GameObject defender in _activeDefenders)
            {
                if (defender != null && defender.name.Contains("Sentinel"))
                    count++;
            }
            return count;
        }
        
        void HandleEnemyKilled(EnemyKilledEventArgs args)
        {
            // Only count our enemies
            if (args.enemyType != "DissonanceDefender" && args.enemyType != "CrystalSentinel")
                return;
            
            _killCount++;
            
            // Progress milestone at 5 kills
            if (_killCount == 5)
            {
                GameStateManager.Instance?.AddMoonProgress("Moon2", 10f);
                Debug.Log("[Moon2EnemySpawners] First combat milestone - 5 defenders defeated");
            }
            
            // Progress milestone at kill goal
            if (_killCount >= killGoal)
            {
                GameStateManager.Instance?.AddMoonProgress("Moon2", 15f);
                Debug.Log($"[Moon2EnemySpawners] Combat mastery achieved - {killGoal} defenders defeated");
            }
            
            Debug.Log($"[Moon2EnemySpawners] Kill count: {_killCount}/{killGoal}");
        }
        
        public void SpawnWave(Vector3 location, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 8f;
                offset.y = 0f;
                
                GameObject defender = Instantiate(dissonanceDefenderPrefab, location + offset, Quaternion.identity, transform);
                defender.name = $"WaveDefender_{i}";
                _activeDefenders.Add(defender);
            }
        }
    }
}
