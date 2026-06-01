using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// ResonanceScannerSystem - Detects buried buildings and secrets.
    /// Core exploration mechanic.
    /// </summary>
    public class ResonanceScannerSystem : MonoBehaviour
    {
        public static ResonanceScannerSystem Instance { get; private set; }

        [Header("Scanner Settings")]
        [SerializeField] private float scanRadius = 50f;
        [SerializeField] private float scanCooldown = 3f;
        [SerializeField] private LayerMask scanLayers;

        private float _lastScanTime = -9999f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && Time.time - _lastScanTime > scanCooldown)
            {
                PerformScan();
            }
        }

        void PerformScan()
        {
            _lastScanTime = Time.time;
            Debug.Log("[ResonanceScanner] Scanning...");

            Vector3 playerPos = PlayerSpawner.Instance?.GetPlayer()?.transform.position ?? Vector3.zero;
            
            // VFX + Audio
            VFXWiringController.Instance?.SpawnScanPulse(playerPos);
            AudioFeedbackController.Instance?.PlayScan(playerPos);

            // Find nearby buildings
            Collider[] hits = Physics.OverlapSphere(playerPos, scanRadius, scanLayers);
            List<string> discovered = new();

            foreach (var hit in hits)
            {
                var building = hit.GetComponent<CathedralRestorationSystem>();
                if (building != null && !building.IsDiscovered())
                {
                    GameEvents.FireBuildingDiscovered("Cathedral", hit.transform.position);
                    discovered.Add("Cathedral");
                }
            }

            if (discovered.Count > 0)
            {
                HUDController.Instance?.ShowBanner($"DISCOVERED!", $"{discovered.Count} buildings detected");
            }
            else
            {
                HUDController.Instance?.ShowBanner("SCAN COMPLETE", "No new discoveries");
            }
        }

        public void ForceScan() => PerformScan();
    }
}
