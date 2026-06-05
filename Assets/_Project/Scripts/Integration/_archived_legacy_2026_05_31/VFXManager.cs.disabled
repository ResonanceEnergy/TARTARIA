using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// VFXManager — centralized singleton for wiring VFX prefabs in scenes.
    /// Assigns prefab references to various VFX systems on Awake.
    /// Place one instance in each scene that needs VFX wiring.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)] // Before all other systems
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Hit VFX Prefabs (HitVFXController)")]
        [SerializeField] GameObject sparkVFXPrefab;
        [SerializeField] GameObject bloodVFXPrefab;
        [SerializeField] GameObject shieldVFXPrefab;

        [Header("Building VFX Prefabs")]
        [SerializeField] GameObject restoreSparkleVFXPrefab;

        [Header("Scan VFX Prefabs")]
        [SerializeField] GameObject scanPulseVFXPrefab;

        [Header("Loot VFX Prefabs")]
        [SerializeField] GameObject shardCollectVFXPrefab;

        void Awake()
        {
            // Singleton pattern (scene-scoped, not DontDestroyOnLoad)
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[VFXManager] Duplicate instance detected, destroying");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            WireVFXReferences();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void WireVFXReferences()
        {
            // Wire HitVFXController prefabs
            var hitVFX = HitVFXController.Instance;
            if (hitVFX != null && (sparkVFXPrefab != null || bloodVFXPrefab != null || shieldVFXPrefab != null))
            {
                // Use reflection to set private SerializeField values at runtime
                var hitVFXType = typeof(HitVFXController);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                if (sparkVFXPrefab != null)
                {
                    var field = hitVFXType.GetField("_sparkVfxPrefab", flags);
                    field?.SetValue(hitVFX, sparkVFXPrefab);
                    Debug.Log("[VFXManager] Assigned Spark VFX prefab to HitVFXController");
                }

                if (bloodVFXPrefab != null)
                {
                    var field = hitVFXType.GetField("_bloodVfxPrefab", flags);
                    field?.SetValue(hitVFX, bloodVFXPrefab);
                    Debug.Log("[VFXManager] Assigned Blood VFX prefab to HitVFXController");
                }

                if (shieldVFXPrefab != null)
                {
                    var field = hitVFXType.GetField("_shieldVfxPrefab", flags);
                    field?.SetValue(hitVFX, shieldVFXPrefab);
                    Debug.Log("[VFXManager] Assigned Shield VFX prefab to HitVFXController");
                }

                // Re-initialize pools with new prefabs
                var initMethod = hitVFXType.GetMethod("InitializePools", flags);
                initMethod?.Invoke(hitVFX, null);
            }

            // Wire BuildingSpawner prefab
            var buildingSpawner = FindFirstObjectByType<BuildingSpawner>();
            if (buildingSpawner != null && restoreSparkleVFXPrefab != null)
            {
                var spawnerType = typeof(BuildingSpawner);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var field = spawnerType.GetField("restoreSparkleVFX", flags);
                field?.SetValue(buildingSpawner, restoreSparkleVFXPrefab);
                Debug.Log("[VFXManager] Assigned RestoreSparkle VFX prefab to BuildingSpawner");
            }

            // Wire ResonanceScannerSystem prefab
            var scanner = ResonanceScannerSystem.Instance;
            if (scanner != null && scanPulseVFXPrefab != null)
            {
                var scannerType = typeof(ResonanceScannerSystem);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var field = scannerType.GetField("scanPulseVFX", flags);
                field?.SetValue(scanner, scanPulseVFXPrefab);
                Debug.Log("[VFXManager] Assigned ScanPulse VFX prefab to ResonanceScannerSystem");
            }

            // Wire LootDropper prefab (static class)
            if (shardCollectVFXPrefab != null)
            {
                LootDropper.ShardCollectVFX = shardCollectVFXPrefab;
                Debug.Log("[VFXManager] Assigned ShardCollect VFX prefab to LootDropper");
            }

            // Wire WhiteCityAmplificationController prefab
            var whiteCityController = WhiteCityAmplificationController.Instance;
            if (whiteCityController != null && scanPulseVFXPrefab != null)
            {
                var controllerType = typeof(WhiteCityAmplificationController);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var field = controllerType.GetField("scanPulseVFXPrefab", flags);
                field?.SetValue(whiteCityController, scanPulseVFXPrefab);
                Debug.Log("[VFXManager] Assigned ScanPulse VFX prefab to WhiteCityAmplificationController");
            }

            Debug.Log("[VFXManager] VFX prefab wiring complete");
        }

        /// <summary>
        /// Manually trigger VFX wiring (useful for runtime prefab assignment in tests).
        /// </summary>
        public void RefreshVFXWiring()
        {
            WireVFXReferences();
        }
    }
}
