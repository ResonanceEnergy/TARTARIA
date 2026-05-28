using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// CathedralRestorationSystem - Complete restoration sequence for Cathedral building.
    /// 3-node tuning mini-game + progressive reveal + corruption purge.
    /// Per 03_CAMPAIGN_13_MOONS.md Moon 1 specs.
    /// </summary>
    public class CathedralRestorationSystem : MonoBehaviour
    {
        [Header("Building State")]
        [SerializeField] private string buildingId = "cathedral";
        [SerializeField] private bool isDiscovered = false;
        [SerializeField] private bool isRestored = false;
        [SerializeField] private int nodesCompleted = 0;
        [SerializeField] private int totalNodes = 3;

        [Header("Node Positions")]
        [SerializeField] private Transform node1Transform;
        [SerializeField] private Transform node2Transform;
        [SerializeField] private Transform node3Transform;

        [Header("Visual Progression")]
        [SerializeField] private GameObject mudCovering; // Dissolves as restoration progresses
        [SerializeField] private GameObject cleanGeometry; // Reveals as restoration progresses
        [SerializeField] private ParticleSystem restorationParticles;

        [Header("Corruption")]
        [SerializeField] private int corruptionLevel = 100; // 0-100

        private TuningMiniGame _tuningGame;

        void Start()
        {
            _tuningGame = FindFirstObjectByType<TuningMiniGame>();

            // Hide clean geometry initially
            if (cleanGeometry != null)
                cleanGeometry.SetActive(false);

            Debug.Log($"[CathedralRestoration] Initialized (Nodes: 0/{totalNodes})");
        }

        void Update()
        {
            // Discovery check (player proximity)
            if (!isDiscovered && PlayerInRange(15f))
            {
                Discover();
            }
        }

        bool PlayerInRange(float range)
        {
            var player = PlayerSpawner.Instance?.GetPlayer();
            return player != null && Vector3.Distance(transform.position, player.transform.position) < range;
        }

        void Discover()
        {
            isDiscovered = true;
            Debug.Log("[CathedralRestoration] DISCOVERED!");

            GameEvents.FireBuildingDiscovered("Cathedral", transform.position);

            // VFX
            VFXWiringController.Instance?.SpawnScanPulse(transform.position);
            AudioFeedbackController.Instance?.PlayScan(transform.position);

            // UI prompt
            HUDController.Instance?.ShowBanner("Cathedral Discovered!", "Tune 3 resonance nodes to restore");
        }

        public void TuneNode(int nodeIndex)
        {
            if (isRestored) return;
            if (nodeIndex < 1 || nodeIndex > totalNodes) return;

            Debug.Log($"[CathedralRestoration] Starting tuning for Node {nodeIndex}...");

            // Get node position
            Transform nodeTransform = nodeIndex switch
            {
                1 => node1Transform,
                2 => node2Transform,
                3 => node3Transform,
                _ => null
            };

            if (nodeTransform == null)
            {
                Debug.LogWarning($"[CathedralRestoration] Node {nodeIndex} transform not assigned!");
                return;
            }

            // Start tuning mini-game
            if (_tuningGame != null)
            {
                _tuningGame.StartTuning(nodeTransform.position, () => OnNodeCompleted(nodeIndex));
            }
            else
            {
                Debug.LogWarning("[CathedralRestoration] TuningMiniGame not found!");
                OnNodeCompleted(nodeIndex); // Fallback: instant complete
            }
        }

        void OnNodeCompleted(int nodeIndex)
        {
            nodesCompleted++;
            Debug.Log($"[CathedralRestoration] Node {nodeIndex} TUNED! ({nodesCompleted}/{totalNodes})");

            // VFX at node
            Transform nodeTransform = nodeIndex switch
            {
                1 => node1Transform,
                2 => node2Transform,
                3 => node3Transform,
                _ => null
            };

            if (nodeTransform != null)
            {
                VFXWiringController.Instance?.SpawnRestoreSparkle(nodeTransform.position);
                AudioFeedbackController.Instance?.PlayRestore(nodeTransform.position);
            }

            // Progressive reveal (33% per node)
            float progress = nodesCompleted / (float)totalNodes;
            UpdateVisualProgression(progress);

            // Reduce corruption
            corruptionLevel = Mathf.Max(0, 100 - (nodesCompleted * 33));

            // Check if fully restored
            if (nodesCompleted >= totalNodes)
            {
                CompleteRestoration();
            }
            else
            {
                HUDController.Instance?.ShowBanner($"Node {nodeIndex} Tuned!", $"{totalNodes - nodesCompleted} nodes remaining");
            }
        }

        void UpdateVisualProgression(float progress)
        {
            // Dissolve mud covering
            if (mudCovering != null)
            {
                var renderer = mudCovering.GetComponent<Renderer>();
                if (renderer != null && renderer.material.HasProperty("_Dissolve"))
                {
                    renderer.material.SetFloat("_Dissolve", progress);
                }
            }

            // Reveal clean geometry
            if (cleanGeometry != null && progress > 0.3f)
            {
                cleanGeometry.SetActive(true);
                float alpha = Mathf.Lerp(0f, 1f, (progress - 0.3f) / 0.7f);
                // Fade in cleanGeometry materials here if needed
            }

            // Particles intensity
            if (restorationParticles != null)
            {
                var emission = restorationParticles.emission;
                emission.rateOverTime = progress * 50f;
            }
        }

        void CompleteRestoration()
        {
            isRestored = true;
            corruptionLevel = 0;

            Debug.Log("[CathedralRestoration] ✅ RESTORATION COMPLETE!");

            // Fire event
            GameEvents.FireBuildingRestored(buildingId);

            // Full VFX burst
            if (restorationParticles != null)
                restorationParticles.Play();

            VFXWiringController.Instance?.SpawnRestoreSparkle(transform.position + Vector3.up * 5f);
            AudioFeedbackController.Instance?.PlaySFX("RestorationComplete", transform.position);

            // Camera shake
            CameraShakeController.Instance?.Shake(0.5f, 0.5f);

            // UI celebration
            HUDController.Instance?.ShowBanner("CATHEDRAL RESTORED!", "Ancient resonance flows once more");

            // Award RS
            GameLoopController.Instance?.AwardRS(50f, "Cathedral Restoration");

            // Hide mud completely, show clean geometry
            if (mudCovering != null)
                mudCovering.SetActive(false);
            if (cleanGeometry != null)
                cleanGeometry.SetActive(true);
        }

        // Public API
        public bool IsDiscovered() => isDiscovered;
        public bool IsRestored() => isRestored;
        public int GetNodesCompleted() => nodesCompleted;
        public int GetCorruptionLevel() => corruptionLevel;

        void OnDrawGizmosSelected()
        {
            // Discovery range (yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 15f);

            // Node positions (green)
            if (node1Transform != null)
                Gizmos.DrawSphere(node1Transform.position, 0.5f);
            if (node2Transform != null)
                Gizmos.DrawSphere(node2Transform.position, 0.5f);
            if (node3Transform != null)
                Gizmos.DrawSphere(node3Transform.position, 0.5f);
        }
    }
}
