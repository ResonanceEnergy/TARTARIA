using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// ResourceNodeSpawner — manages harvestable resource nodes (aether crystals, herbs, ore).
    /// Spawns nodes in clusters based on biome rules, handles respawn timers.
    /// Player interacts via IInteractable to harvest resources → adds to InventorySystem.
    /// 
    /// Resource Types:
    /// - AetherCrystal → resonance energy, used for building tuning + RS restoration
    /// - ResonantHerb → crafting material for potions
    /// - DissonantOre → purifiable metal for equipment upgrades
    /// 
    /// Spawning Rules:
    /// - Clusters of 3-7 nodes
    /// - Min distance between clusters: 30m
    /// - Respawn time: 60-120s per node
    /// - Biome restrictions: crystals in caverns, herbs in forests, ore near ruins
    /// 
    /// Usage:
    /// - Attach to scene root GameObject
    /// - Define spawn zones (array of Transform waypoints)
    /// - Call SpawnResourceNodes(resourceType, count) on scene load
    /// 
    /// GDD refs: §02 (Aether Economy), §07 (Crafting), §09 (Resource Gathering)
    /// </summary>
    public class ResourceNodeSpawner : MonoBehaviour
    {
        public static ResourceNodeSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] int crystalNodesPerZone = 15;
        [SerializeField] int herbNodesPerZone = 10;
        [SerializeField] int oreNodesPerZone = 8;
        [SerializeField] float minNodeSpacing = 5f;
        [SerializeField] float maxNodeSpacing = 15f;
        [SerializeField] int clusterSizeMin = 3;
        [SerializeField] int clusterSizeMax = 7;

        [Header("Respawn")]
        [SerializeField] float respawnTimeMin = 60f;
        [SerializeField] float respawnTimeMax = 120f;

        [Header("Spawn Zones")]
        [SerializeField] Transform[] crystalSpawnZones;
        [SerializeField] Transform[] herbSpawnZones;
        [SerializeField] Transform[] oreSpawnZones;

        [Header("Prefabs")]
        [SerializeField] GameObject crystalPrefab;
        [SerializeField] GameObject herbPrefab;
        [SerializeField] GameObject orePrefab;

        readonly List<ResourceNode> _activeNodes = new();
        readonly List<ResourceNode> _respawnQueue = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            SpawnInitialNodes();
        }

        void Update()
        {
            // Process respawn queue
            for (int i = _respawnQueue.Count - 1; i >= 0; i--)
            {
                var node = _respawnQueue[i];
                node.respawnTimer -= Time.deltaTime;

                if (node.respawnTimer <= 0f)
                {
                    RespawnNode(node);
                    _respawnQueue.RemoveAt(i);
                }
            }
        }

        void SpawnInitialNodes()
        {
            Debug.Log("[ResourceSpawner] Spawning initial resource nodes");

            // Crystals
            if (crystalSpawnZones != null && crystalSpawnZones.Length > 0)
            {
                SpawnResourceClusters(ResourceType.AetherCrystal, crystalSpawnZones, crystalNodesPerZone);
            }

            // Herbs
            if (herbSpawnZones != null && herbSpawnZones.Length > 0)
            {
                SpawnResourceClusters(ResourceType.ResonantHerb, herbSpawnZones, herbNodesPerZone);
            }

            // Ore
            if (oreSpawnZones != null && oreSpawnZones.Length > 0)
            {
                SpawnResourceClusters(ResourceType.DissonantOre, oreSpawnZones, oreNodesPerZone);
            }

            Debug.Log($"[ResourceSpawner] Spawned {_activeNodes.Count} resource nodes");
        }

        void SpawnResourceClusters(ResourceType type, Transform[] zones, int nodesPerZone)
        {
            foreach (var zone in zones)
            {
                if (zone == null) continue;

                // Calculate cluster count
                int clustersInZone = Mathf.CeilToInt(nodesPerZone / ((clusterSizeMin + clusterSizeMax) * 0.5f));

                for (int i = 0; i < clustersInZone; i++)
                {
                    Vector3 clusterCenter = zone.position + Random.insideUnitSphere * 50f;
                    clusterCenter.y = zone.position.y;  // Keep at zone height

                    int clusterSize = Random.Range(clusterSizeMin, clusterSizeMax + 1);

                    for (int j = 0; j < clusterSize; j++)
                    {
                        Vector3 offset = Random.insideUnitSphere * maxNodeSpacing;
                        offset.y = 0f;
                        Vector3 spawnPos = clusterCenter + offset;

                        // Check spacing
                        if (IsValidSpawnPosition(spawnPos))
                        {
                            SpawnNode(type, spawnPos);
                        }
                    }
                }
            }
        }

        bool IsValidSpawnPosition(Vector3 position)
        {
            foreach (var node in _activeNodes)
            {
                if (Vector3.Distance(position, node.transform.position) < minNodeSpacing)
                {
                    return false;
                }
            }
            return true;
        }

        void SpawnNode(ResourceType type, Vector3 position)
        {
            GameObject prefab = GetPrefabForType(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[ResourceSpawner] No prefab defined for {type}");
                return;
            }

            GameObject nodeGO = Instantiate(prefab, position, Quaternion.identity, transform);
            nodeGO.name = $"{type}Node_{_activeNodes.Count}";

            var node = nodeGO.GetComponent<ResourceNode>();
            if (node == null)
            {
                node = nodeGO.AddComponent<ResourceNode>();
            }

            node.Initialize(type, this);
            _activeNodes.Add(node);
        }

        void RespawnNode(ResourceNode node)
        {
            node.gameObject.SetActive(true);
            node.Reset();
            _activeNodes.Add(node);

            Debug.Log($"[ResourceSpawner] Respawned {node.Type} node at {node.transform.position}");
        }

        public void OnNodeHarvested(ResourceNode node)
        {
            _activeNodes.Remove(node);

            // Hide node
            node.gameObject.SetActive(false);

            // Queue for respawn
            node.respawnTimer = Random.Range(respawnTimeMin, respawnTimeMax);
            _respawnQueue.Add(node);

            Debug.Log($"[ResourceSpawner] {node.Type} node harvested, respawns in {node.respawnTimer:F0}s");
        }

        GameObject GetPrefabForType(ResourceType type)
        {
            return type switch
            {
                ResourceType.AetherCrystal => crystalPrefab,
                ResourceType.ResonantHerb => herbPrefab,
                ResourceType.DissonantOre => orePrefab,
                _ => null
            };
        }

        public enum ResourceType : byte
        {
            AetherCrystal = 0,
            ResonantHerb = 1,
            DissonantOre = 2
        }
    }

    /// <summary>
    /// ResourceNode component — attach to harvestable resource GameObjects.
    /// </summary>
    public class ResourceNode : MonoBehaviour, IInteractable
    {
        public ResourceNodeSpawner.ResourceType Type { get; private set; }
        public float respawnTimer;

        ResourceNodeSpawner _spawner;
        bool _isHarvested;

        public void Initialize(ResourceNodeSpawner.ResourceType type, ResourceNodeSpawner spawner)
        {
            Type = type;
            _spawner = spawner;
            _isHarvested = false;

            // Set layer to Interactable
            gameObject.layer = LayerMask.NameToLayer("Interactable");

            // Add collider if missing
            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<SphereCollider>();
                col.radius = 1f;
                col.isTrigger = true;
            }

            // Visual: create placeholder mesh
            if (GetComponentInChildren<MeshRenderer>() == null)
            {
                var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh.transform.SetParent(transform);
                mesh.transform.localPosition = Vector3.up * 0.5f;
                mesh.transform.localScale = Vector3.one * 0.8f;
                Destroy(mesh.GetComponent<Collider>());

                var renderer = mesh.GetComponent<MeshRenderer>();
                renderer.material.color = GetColorForType(Type);
            }
        }

        public void Reset()
        {
            _isHarvested = false;
        }

        public void Interact(GameObject player)
        {
            if (_isHarvested) return;

            _isHarvested = true;

            // Add to player inventory
            string itemId = Type.ToString();
            InventorySystem.Instance?.AddItem(itemId, 1);

            // Play harvest SFX
            Audio.AudioManager.Instance?.PlaySFX("resource_harvest", transform.position);

            // VFX (sparkle particles)
            // TODO: ParticleEffectPool.Instance?.PlayEffect("AetherCollect", transform.position);

            // Notify spawner
            _spawner?.OnNodeHarvested(this);

            Debug.Log($"[ResourceNode] Harvested {Type} by {player.name}");
        }

        public string GetInteractPrompt()
        {
            return _isHarvested ? "" : $"Harvest {Type}";
        }

        Color GetColorForType(ResourceNodeSpawner.ResourceType type)
        {
            return type switch
            {
                ResourceNodeSpawner.ResourceType.AetherCrystal => new Color(0.2f, 0.8f, 1f),  // Cyan
                ResourceNodeSpawner.ResourceType.ResonantHerb => new Color(0.3f, 1f, 0.4f),  // Green
                ResourceNodeSpawner.ResourceType.DissonantOre => new Color(0.8f, 0.3f, 0.6f),  // Purple
                _ => Color.white
            };
        }
    }
}
