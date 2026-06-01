using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Collectibles — Aether Shards and lore artifacts
    /// Manages spawning, collection, and tracking of collectible items in Echohaven.
    /// Integrates with save system for persistence.
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon1Collectibles : MonoBehaviour
    {
        [Header("Aether Shards")]
        [SerializeField] GameObject aetherShardPrefab;
        [SerializeField] int totalShards = 15;           // 15 shards scattered in Echohaven
        [SerializeField] float shardRSReward = 2f;       // +2 RS per shard
        [SerializeField] float collectRadius = 2.5f;
        
        [Header("Lore Artifacts")]
        [SerializeField] GameObject loreArtifactPrefab;
        [SerializeField] int totalArtifacts = 5;          // 5 lore items (optional)
        
        [Header("Spawn Areas")]
        [SerializeField] Vector3[] shardPositions;        // Manual positions (set in editor)
        [SerializeField] bool useProceduralSpawns = true;
        [SerializeField] float spawnAreaRadius = 40f;
        
        readonly List<GameObject> _activeShards = new();
        readonly List<GameObject> _activeArtifacts = new();
        readonly HashSet<int> _collectedShards = new();   // Track by ID for save
        readonly HashSet<int> _collectedArtifacts = new();
        
        int _shardsCollected;
        
        public int ShardsCollected => _shardsCollected;
        public int TotalShards => totalShards;
        public float CollectionProgress => _shardsCollected / (float)totalShards;
        
        void Start()
        {
            // Load saved collection state
            LoadCollectionState();
            
            // Spawn collectibles
            SpawnAetherShards();
            SpawnLoreArtifacts();
            
            // Wire save events
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnSave;
                SaveManager.Instance.OnAfterLoad += OnLoad;
            }
            
            Debug.Log($"[Moon1Collectibles] ✅ Initialized - {_shardsCollected}/{totalShards} shards collected");
        }
        
        void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnSave;
                SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
        }
        
        void SpawnAetherShards()
        {
            // Use manual positions if provided
            if (shardPositions != null && shardPositions.Length > 0)
            {
                for (int i = 0; i < Mathf.Min(shardPositions.Length, totalShards); i++)
                {
                    if (!_collectedShards.Contains(i))  // Don't spawn already-collected
                    {
                        SpawnShard(i, shardPositions[i]);
                    }
                }
            }
            // Otherwise procedural
            else if (useProceduralSpawns)
            {
                for (int i = 0; i < totalShards; i++)
                {
                    if (!_collectedShards.Contains(i))
                    {
                        Vector3 pos = GetRandomSpawnPosition();
                        SpawnShard(i, pos);
                    }
                }
            }
        }
        
        void SpawnShard(int id, Vector3 position)
        {
            if (aetherShardPrefab == null)
            {
                Debug.LogWarning("[Moon1Collectibles] No Aether Shard prefab assigned!");
                return;
            }
            
            GameObject shard = Instantiate(aetherShardPrefab, position, Quaternion.identity);
            shard.name = $"AetherShard_{id}";
            
            // Add collectible component if missing
            var collectible = shard.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = shard.AddComponent<Collectible>();
            }
            
            // Configure collectible
            collectible.collectibleID = id;
            collectible.collectibleType = "AetherShard";
            collectible.rsReward = shardRSReward;
            collectible.collectRadius = collectRadius;
            collectible.onCollected += () => OnShardCollected(id);
            
            _activeShards.Add(shard);
        }
        
        void SpawnLoreArtifacts()
        {
            if (loreArtifactPrefab == null) return;
            
            for (int i = 0; i < totalArtifacts; i++)
            {
                if (!_collectedArtifacts.Contains(i))
                {
                    Vector3 pos = GetRandomSpawnPosition();
                    GameObject artifact = Instantiate(loreArtifactPrefab, pos, Quaternion.identity);
                    artifact.name = $"LoreArtifact_{i}";
                    
                    var collectible = artifact.GetOrAddComponent<Collectible>();
                    collectible.collectibleID = i;
                    collectible.collectibleType = "LoreArtifact";
                    collectible.rsReward = 5f;  // Bonus RS for optional lore
                    collectible.onCollected += () => OnArtifactCollected(i);
                    
                    _activeArtifacts.Add(artifact);
                }
            }
        }
        
        Vector3 GetRandomSpawnPosition()
        {
            // Spawn in circle around origin
            Vector2 randomCircle = Random.insideUnitCircle * spawnAreaRadius;
            Vector3 position = new Vector3(randomCircle.x, 5f, randomCircle.y);  // Start above ground
            
            // Raycast down to find ground
            if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 20f))
            {
                return hit.point + Vector3.up * 1f;  // 1m above ground
            }
            
            return new Vector3(position.x, 1f, position.z);  // Fallback
        }
        
        void OnShardCollected(int id)
        {
            if (_collectedShards.Contains(id)) return;  // Already collected
            
            _collectedShards.Add(id);
            _shardsCollected++;
            
            // Reward player
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.AddResonancePoints(shardRSReward);
            }
            
            // Fire event
            GameEvents.FireCollectibleGathered(new CollectibleEventArgs
            {
                collectibleType = "AetherShard",
                collectibleID = id,
                rsReward = shardRSReward
            });
            
            // VFX and audio
            // TODO: Trigger particle effect and collection sound
            
            Debug.Log($"[Moon1Collectibles] Aether Shard {id} collected ({_shardsCollected}/{totalShards})");
            
            // Check completion
            if (_shardsCollected >= totalShards)
            {
                OnAllShardsCollected();
            }
        }
        
        void OnArtifactCollected(int id)
        {
            if (_collectedArtifacts.Contains(id)) return;
            
            _collectedArtifacts.Add(id);
            
            // Bonus RS
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.AddResonancePoints(5f);
            }
            
            // Unlock lore entry
            // TODO: Unlock lore panel entry
            
            Debug.Log($"[Moon1Collectibles] Lore Artifact {id} collected");
        }
        
        void OnAllShardsCollected()
        {
            Debug.Log("[Moon1Collectibles] ✨ ALL AETHER SHARDS COLLECTED!");
            
            // Bonus reward
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.AddResonancePoints(10f);  // Completion bonus
            }
            
            // Achievement/trophy
            GameEvents.FireAchievementUnlocked("echohaven_shards_complete");
            
            // Moon progression
            GameEvents.FireMoonProgressUpdate(1, 0.15f);  // 15% Moon 1 progress
        }
        
        void OnSave(SaveData sd)
        {
            // Save collection state
            sd.SetMoonData(1, "shardsCollected", _shardsCollected);
            sd.SetMoonData(1, "collectedShardIDs", string.Join(",", _collectedShards));
            sd.SetMoonData(1, "collectedArtifactIDs", string.Join(",", _collectedArtifacts));
        }
        
        void OnLoad(SaveData sd)
        {
            // Restore collection state
            _shardsCollected = sd.GetMoonData(1, "shardsCollected", 0);
            
            // Parse collected IDs
            string shardIDs = sd.GetMoonData(1, "collectedShardIDs", "");
            if (!string.IsNullOrEmpty(shardIDs))
            {
                foreach (string idStr in shardIDs.Split(','))
                {
                    if (int.TryParse(idStr, out int id))
                        _collectedShards.Add(id);
                }
            }
            
            string artifactIDs = sd.GetMoonData(1, "collectedArtifactIDs", "");
            if (!string.IsNullOrEmpty(artifactIDs))
            {
                foreach (string idStr in artifactIDs.Split(','))
                {
                    if (int.TryParse(idStr, out int id))
                        _collectedArtifacts.Add(id);
                }
            }
            
            Debug.Log($"[Moon1Collectibles] Loaded: {_shardsCollected}/{totalShards} shards");
        }
        
        void LoadCollectionState()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                OnLoad(SaveManager.Instance.CurrentSave);
            }
        }
    }
    
    /// <summary>
    /// Simple collectible component - attach to collectible prefabs
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        public int collectibleID;
        public string collectibleType;
        public float rsReward;
        public float collectRadius = 2.5f;
        public System.Action onCollected;
        
        GameObject _player;
        bool _collected;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            // Simple bob animation
            LeanTween.moveY(gameObject, transform.position.y + 0.5f, 1.5f)
                .setEaseInOutSine()
                .setLoopPingPong();
                
            // Rotate
            LeanTween.rotateY(gameObject, 360f, 3f)
                .setLoopClamp();
        }
        
        void Update()
        {
            if (_collected || _player == null) return;
            
            // Auto-collect when player is near
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance <= collectRadius)
            {
                Collect();
            }
        }
        
        void Collect()
        {
            if (_collected) return;
            
            _collected = true;
            onCollected?.Invoke();
            
            // Simple collect animation
            LeanTween.scale(gameObject, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
