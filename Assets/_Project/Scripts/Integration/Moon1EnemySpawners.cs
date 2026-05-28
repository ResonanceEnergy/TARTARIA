using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.AI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Enemy Spawners — Echohaven Mud Golem encounters
    /// Spawns and manages enemy waves for first Moon combat experience.
    /// Progression: Tutorial combat → roaming enemies → mini-boss → ongoing spawns
    /// </summary>
    [DefaultExecutionOrder(-75)]
    public class Moon1EnemySpawners : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] GameObject mudGolemPrefab;
        [SerializeField] int maxActiveGolems = 5;
        [SerializeField] float spawnInterval = 45f;  // 45s between spawns
        [SerializeField] float spawnRadius = 50f;    // Spawn within 50m of player
        
        [Header("Spawn Points")]
        [SerializeField] Transform[] spawnPoints;    // Manual spawn markers
        [SerializeField] bool useProceduralSpawns = true;
        
        [Header("Tutorial")]
        [SerializeField] bool tutorialMode = true;   // First encounter is controlled
        [SerializeField] Vector3 tutorialSpawnPoint = new Vector3(15f, 1f, 20f);
        
        [Header("Progression")]
        [SerializeField] int enemiesKilledForProgress = 10;  // Kill 10 for Moon progression
        
        readonly List<GameObject> _activeGolems = new();
        int _enemiesKilled;
        float _nextSpawnTime;
        bool _tutorialComplete;
        bool _isActive;
        
        public int EnemiesKilled => _enemiesKilled;
        public bool TutorialComplete => _tutorialComplete;
        
        void Start()
        {
            _isActive = true;
            
            // Wire to GameEvents for enemy death tracking
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            
            // Spawn tutorial enemy if in tutorial mode
            if (tutorialMode && !_tutorialComplete)
            {
                Invoke(nameof(SpawnTutorialEnemy), 5f);  // 5s delay after scene start
            }
            
            _nextSpawnTime = Time.time + spawnInterval;
            
            Debug.Log("[Moon1EnemySpawners] ✅ Initialized - Tutorial mode: " + tutorialMode);
        }
        
        void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        }
        
        void Update()
        {
            if (!_isActive) return;
            
            // Remove destroyed golems from active list
            _activeGolems.RemoveAll(g => g == null);
            
            // Spawn new enemies if below max and enough time passed
            if (_tutorialComplete && Time.time >= _nextSpawnTime && _activeGolems.Count < maxActiveGolems)
            {
                SpawnGolem();
                _nextSpawnTime = Time.time + spawnInterval;
            }
        }
        
        void SpawnTutorialEnemy()
        {
            if (_tutorialComplete) return;
            
            GameObject golem = SpawnGolemAt(tutorialSpawnPoint);
            
            if (golem != null)
            {
                // Tag as tutorial enemy (weaker, slower)
                var ai = golem.GetComponent<MudGolemAI>();
                if (ai != null)
                {
                    ai.SetTutorialMode(true);  // Reduced health/damage
                }
                
                Debug.Log("[Moon1EnemySpawners] Tutorial Mud Golem spawned");
                
                // Show tutorial prompt
                GameEvents.FireTutorialStep(TutorialStep.FirstCombat);
            }
        }
        
        void SpawnGolem()
        {
            Vector3 spawnPos = GetSpawnPosition();
            SpawnGolemAt(spawnPos);
        }
        
        GameObject SpawnGolemAt(Vector3 position)
        {
            if (mudGolemPrefab == null)
            {
                Debug.LogError("[Moon1EnemySpawners] No MudGolem prefab assigned!");
                return null;
            }
            
            GameObject golem = Instantiate(mudGolemPrefab, position, Quaternion.identity);
            golem.name = "MudGolem_" + _activeGolems.Count;
            _activeGolems.Add(golem);
            
            // Ensure golem has AI component
            if (golem.GetComponent<MudGolemAI>() == null)
            {
                Debug.LogWarning("[Moon1EnemySpawners] MudGolem prefab missing MudGolemAI component!");
            }
            
            return golem;
        }
        
        Vector3 GetSpawnPosition()
        {
            if (useProceduralSpawns)
            {
                // Spawn near player but not too close
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y);
                    Vector3 spawnPos = player.transform.position + offset;
                    
                    // Ensure spawn is on ground
                    if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
                    {
                        return hit.point + Vector3.up * 0.5f;
                    }
                    
                    return spawnPos;
                }
            }
            
            // Fallback to manual spawn points
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                return spawnPoints[randomIndex].position;
            }
            
            // Last resort: random position near origin
            return new Vector3(
                Random.Range(-20f, 20f),
                1f,
                Random.Range(-20f, 20f)
            );
        }
        
        void HandleEnemyKilled(EnemyKilledEventArgs args)
        {
            // Only count Mud Golems in Moon 1
            if (args.enemyType != "MudGolem") return;
            
            _enemiesKilled++;
            
            Debug.Log($"[Moon1EnemySpawners] Enemy killed: {_enemiesKilled}/{enemiesKilledForProgress}");
            
            // Complete tutorial after first kill
            if (!_tutorialComplete && _enemiesKilled >= 1)
            {
                _tutorialComplete = true;
                GameEvents.FireTutorialStep(TutorialStep.CombatComplete);
                Debug.Log("[Moon1EnemySpawners] Tutorial combat complete!");
            }
            
            // Check Moon progression
            if (_enemiesKilled >= enemiesKilledForProgress)
            {
                GameEvents.FireMoonProgressUpdate(1, 0.25f);  // 25% Moon 1 progress from combat
            }
        }
        
        /// <summary>
        /// Spawns a specific number of enemies at a location (for scripted events)
        /// </summary>
        public void SpawnWave(Vector3 location, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-5f, 5f),
                    0f,
                    Random.Range(-5f, 5f)
                );
                SpawnGolemAt(location + offset);
            }
            
            Debug.Log($"[Moon1EnemySpawners] Spawned wave of {count} enemies at {location}");
        }
        
        /// <summary>
        /// Clears all active enemies (for scene transitions)
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (GameObject golem in _activeGolems)
            {
                if (golem != null)
                    Destroy(golem);
            }
            
            _activeGolems.Clear();
            Debug.Log("[Moon1EnemySpawners] All enemies cleared");
        }
        
        /// <summary>
        /// Pauses/resumes enemy spawning
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive = active;
        }
    }
}
