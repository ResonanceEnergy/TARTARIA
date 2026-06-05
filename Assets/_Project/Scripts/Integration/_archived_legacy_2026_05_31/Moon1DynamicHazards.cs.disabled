using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Dynamic Hazards — Environmental dangers in Echohaven
    /// Mud pools, falling debris, resonance dissonance zones, collapsing floors
    /// Adds challenge and encourages careful navigation
    /// </summary>
    [DefaultExecutionOrder(-83)]
    public class Moon1DynamicHazards : MonoBehaviour
    {
        [Header("Hazard Configuration")]
        [SerializeField] GameObject mudPoolPrefab;
        [SerializeField] GameObject fallingDebrisPrefab;
        [SerializeField] GameObject dissonanceZonePrefab;
        [SerializeField] GameObject collapsingFloorPrefab;
        
        [Header("Hazard Counts")]
        [SerializeField] int mudPoolCount = 12;
        [SerializeField] int debrisSpawnPoints = 6;
        [SerializeField] int dissonanceZones = 4;
        [SerializeField] int collapsingFloors = 3;
        
        [Header("Damage Settings")]
        [SerializeField] float mudPoolDamage = 5f;
        [SerializeField] float debrisDamage = 15f;
        [SerializeField] float dissonanceDamage = 3f;  // Per second
        [SerializeField] float fallDamage = 20f;
        
        readonly List<GameObject> _activeHazards = new();
        readonly List<FallingDebrisSpawner> _debrisSpawners = new();
        
        void Start()
        {
            SpawnHazards();
            
            Debug.Log($"[Moon1DynamicHazards] ✅ Initialized - {_activeHazards.Count} hazards active");
        }
        
        void SpawnHazards()
        {
            SpawnMudPools();
            SpawnDebrisSpawners();
            SpawnDissonanceZones();
            SpawnCollapsingFloors();
        }
        
        void SpawnMudPools()
        {
            // Mud pools block paths and damage player
            Vector3[] mudPoolLocations = new Vector3[]
            {
                new Vector3(5f, 0.1f, 12f),
                new Vector3(-12f, 0.1f, 8f),
                new Vector3(18f, 0.1f, -5f),
                new Vector3(-8f, 0.1f, -15f),
                new Vector3(22f, 0.1f, 15f),
                new Vector3(-20f, 0.1f, -10f),
                new Vector3(10f, 0.1f, -18f),
                new Vector3(-15f, 0.1f, 20f),
                new Vector3(8f, 0.1f, 25f),
                new Vector3(-5f, 0.1f, -22f),
                new Vector3(25f, 0.1f, -15f),
                new Vector3(-18f, 0.1f, 12f),
            };
            
            for (int i = 0; i < Mathf.Min(mudPoolCount, mudPoolLocations.Length); i++)
            {
                if (mudPoolPrefab == null) continue;
                
                GameObject pool = Instantiate(mudPoolPrefab, mudPoolLocations[i], Quaternion.identity, transform);
                pool.name = $"MudPool_{i}";
                
                MudPoolHazard hazard = pool.GetOrAddComponent<MudPoolHazard>();
                hazard.damagePerSecond = mudPoolDamage;
                
                _activeHazards.Add(pool);
            }
        }
        
        void SpawnDebrisSpawners()
        {
            // Falling debris spawners (periodic danger from ceiling)
            Vector3[] spawnerLocations = new Vector3[]
            {
                new Vector3(0f, 15f, 15f),
                new Vector3(-10f, 12f, -8f),
                new Vector3(15f, 18f, 5f),
                new Vector3(-18f, 14f, 18f),
                new Vector3(12f, 16f, -12f),
                new Vector3(-8f, 13f, 22f),
            };
            
            for (int i = 0; i < Mathf.Min(debrisSpawnPoints, spawnerLocations.Length); i++)
            {
                GameObject spawnerObj = new GameObject($"DebrisSpawner_{i}");
                spawnerObj.transform.SetParent(transform);
                spawnerObj.transform.position = spawnerLocations[i];
                
                FallingDebrisSpawner spawner = spawnerObj.AddComponent<FallingDebrisSpawner>();
                spawner.debrisPrefab = fallingDebrisPrefab;
                spawner.debrisDamage = debrisDamage;
                spawner.spawnInterval = Random.Range(8f, 15f);
                
                _debrisSpawners.Add(spawner);
                _activeHazards.Add(spawnerObj);
            }
        }
        
        void SpawnDissonanceZones()
        {
            // Dissonance damage zones (standing in them hurts)
            Vector3[] dissonanceLocations = new Vector3[]
            {
                new Vector3(-15f, 1f, 10f),
                new Vector3(20f, 1f, -8f),
                new Vector3(-10f, 1f, -18f),
                new Vector3(12f, 1f, 20f),
            };
            
            for (int i = 0; i < Mathf.Min(dissonanceZones, dissonanceLocations.Length); i++)
            {
                if (dissonanceZonePrefab == null) continue;
                
                GameObject zone = Instantiate(dissonanceZonePrefab, dissonanceLocations[i], Quaternion.identity, transform);
                zone.name = $"DissonanceZone_{i}";
                
                DissonanceZoneHazard hazard = zone.GetOrAddComponent<DissonanceZoneHazard>();
                hazard.damagePerSecond = dissonanceDamage;
                hazard.zoneRadius = 5f;
                
                _activeHazards.Add(zone);
            }
        }
        
        void SpawnCollapsingFloors()
        {
            // Collapsing floor triggers
            Vector3[] floorLocations = new Vector3[]
            {
                new Vector3(8f, 3f, -10f),
                new Vector3(-12f, 5f, 15f),
                new Vector3(15f, 4f, 18f),
            };
            
            for (int i = 0; i < Mathf.Min(collapsingFloors, floorLocations.Length); i++)
            {
                if (collapsingFloorPrefab == null) continue;
                
                GameObject floor = Instantiate(collapsingFloorPrefab, floorLocations[i], Quaternion.identity, transform);
                floor.name = $"CollapsingFloor_{i}";
                
                CollapsingFloorHazard hazard = floor.GetOrAddComponent<CollapsingFloorHazard>();
                hazard.fallDamage = fallDamage;
                hazard.collapseDelay = 1f;
                
                _activeHazards.Add(floor);
            }
        }
        
        void OnDestroy()
        {
            foreach (var hazard in _activeHazards)
            {
                if (hazard != null)
                    Destroy(hazard);
            }
        }
    }
    
    /// <summary>
    /// Mud pool that damages player over time
    /// </summary>
    public class MudPoolHazard : MonoBehaviour
    {
        public float damagePerSecond = 5f;
        readonly List<GameObject> _playersInPool = new();
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !_playersInPool.Contains(other.gameObject))
            {
                _playersInPool.Add(other.gameObject);
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playersInPool.Remove(other.gameObject);
            }
        }
        
        void Update()
        {
            foreach (GameObject player in _playersInPool)
            {
                if (player != null && PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }
        }
    }
    
    /// <summary>
    /// Spawns falling debris periodically
    /// </summary>
    public class FallingDebrisSpawner : MonoBehaviour
    {
        public GameObject debrisPrefab;
        public float debrisDamage = 15f;
        public float spawnInterval = 10f;
        
        float _nextSpawnTime;
        
        void Start()
        {
            _nextSpawnTime = Time.time + spawnInterval;
        }
        
        void Update()
        {
            if (Time.time >= _nextSpawnTime)
            {
                SpawnDebris();
                _nextSpawnTime = Time.time + spawnInterval;
            }
        }
        
        void SpawnDebris()
        {
            if (debrisPrefab == null) return;
            
            GameObject debris = Instantiate(debrisPrefab, transform.position, Random.rotation);
            
            // Add damage component
            DebrisProjectile projectile = debris.GetOrAddComponent<DebrisProjectile>();
            projectile.damage = debrisDamage;
            
            // Destroy after 10 seconds
            Destroy(debris, 10f);
        }
    }
    
    public class DebrisProjectile : MonoBehaviour
    {
        public float damage = 15f;
        bool _hasHit;
        
        void OnCollisionEnter(Collision collision)
        {
            if (_hasHit) return;
            
            if (collision.gameObject.CompareTag("Player"))
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.TakeDamage(damage);
                    Debug.Log($"[DebrisProjectile] Hit player for {damage} damage!");
                }
                _hasHit = true;
            }
            
            // Destroy on impact
            Destroy(gameObject, 0.5f);
        }
    }
    
    /// <summary>
    /// Dissonance damage zone
    /// </summary>
    public class DissonanceZoneHazard : MonoBehaviour
    {
        public float damagePerSecond = 3f;
        public float zoneRadius = 5f;
        
        GameObject _player;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            // Add sphere collider trigger
            SphereCollider trigger = gameObject.GetOrAddComponent<SphereCollider>();
            trigger.radius = zoneRadius;
            trigger.isTrigger = true;
        }
        
        void Update()
        {
            if (_player == null) return;
            
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            
            if (distance <= zoneRadius && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Collapsing floor that falls when player steps on it
    /// </summary>
    public class CollapsingFloorHazard : MonoBehaviour
    {
        public float fallDamage = 20f;
        public float collapseDelay = 1f;
        
        bool _triggered;
        
        void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            
            if (other.CompareTag("Player"))
            {
                _triggered = true;
                Invoke(nameof(Collapse), collapseDelay);
            }
        }
        
        void Collapse()
        {
            // Fall animation
            LeanTween.moveY(gameObject, transform.position.y - 20f, 2f)
                .setEaseInQuad()
                .setOnComplete(() =>
                {
                    // Damage player if still on it
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float distance = Vector3.Distance(transform.position, player.transform.position);
                        if (distance < 3f && PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.TakeDamage(fallDamage);
                        }
                    }
                    
                    Destroy(gameObject, 1f);
                });
        }
    }
}
