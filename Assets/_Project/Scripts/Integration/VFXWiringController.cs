using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// VFXWiringController — Wires VFX to gameplay events.
    /// </summary>
    public class VFXWiringController : MonoBehaviour
    {
        public static VFXWiringController Instance { get; private set; }

        [Header("VFX Prefabs")]
        [SerializeField] private GameObject scanPulsePrefab;
        [SerializeField] private GameObject restoreSparklePrefab;
        [SerializeField] private GameObject shardCollectPrefab;
        [SerializeField] private GameObject hitImpactPrefab;
        [SerializeField] private GameObject deathBurstPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Subscribe to game events
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnInventoryChanged += OnInventoryChanged;

            Debug.Log("[VFXWiringController] ✅ VFX wired to events");
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnInventoryChanged -= OnInventoryChanged;
        }

        void OnBuildingRestored(string buildingId)
        {
            var building = GameObject.Find(buildingId);
            if (building != null)
            {
                SpawnVFX(restoreSparklePrefab, building.transform.position + Vector3.up * 2f);
            }
        }

        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            SpawnVFX(deathBurstPrefab, args.position);
        }

        void OnInventoryChanged()
        {
            // Triggered by pickup — VFX spawned at pickup location
        }

        public void SpawnScanPulse(Vector3 position)
        {
            SpawnVFX(scanPulsePrefab, position);
        }

        public void SpawnRestoreSparkle(Vector3 position)
        {
            SpawnVFX(restoreSparklePrefab, position);
        }

        public void SpawnShardCollect(Vector3 position)
        {
            SpawnVFX(shardCollectPrefab, position);
        }

        public void SpawnHitImpact(Vector3 position)
        {
            SpawnVFX(hitImpactPrefab, position);
        }

        public void SpawnDeathBurst(Vector3 position)
        {
            SpawnVFX(deathBurstPrefab, position);
        }

        /// <summary>
        /// Spawn VFX by name (convenience method for Integration layer).
        /// Looks up prefab from VFXController and spawns it.
        /// </summary>
        public void SpawnVFX(string vfxName, Vector3 position)
        {
            // Stub: In Phase 2, look up from VFXController prefab registry
            Debug.Log($"[VFXWiring] SpawnVFX({vfxName}) at {position}");
            // VFXController.Instance?.SpawnByName(vfxName, position);
        }

        public void SpawnVFX(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                var vfx = Instantiate(prefab, position, Quaternion.identity);
                Destroy(vfx, 3f); // Auto-cleanup after 3 seconds
            }
            else
            {
                // Fallback: simple particle burst
                Debug.Log($"[VFXWiringController] VFX at {position} (prefab missing)");
            }
        }
    }
}
