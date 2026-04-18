using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Proximity Trigger — fires events when the Player enters a sphere trigger.
    /// Used for building discovery, enemy spawns, and POI revelation.
    /// Attach to any GameObject with a SphereCollider set to isTrigger=true.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public class ProximityTrigger : MonoBehaviour
    {
        public enum TriggerAction
        {
            DiscoverBuilding,
            SpawnEnemy,
            RevealPOI
        }

        [Header("Trigger Settings")]
        [SerializeField] TriggerAction action = TriggerAction.DiscoverBuilding;
        [SerializeField] float triggerRadius = 10f;
        [SerializeField] bool oneShot = true;

        [Header("References")]
        [SerializeField] InteractableBuilding linkedBuilding;
        [SerializeField] GameObject enemyPrefab;
        [SerializeField] string poiId;

        bool _triggered;
        SphereCollider _collider;

        float _spawnTime;

        void Awake()
        {
            _spawnTime = Time.time;
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;

            // Compensate for parent scale — a child SphereCollider inside a building
            // scaled 12x would multiply the radius, creating a 180+ unit trigger zone.
            float maxScale = Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.y,
                transform.lossyScale.z);
            _collider.radius = maxScale > 0.01f ? triggerRadius / maxScale : triggerRadius;

            // Ensure trigger is on a physics layer
            if (gameObject.layer == 0)
            {
                int triggerLayer = LayerMask.NameToLayer("Trigger");
                if (triggerLayer >= 0) gameObject.layer = triggerLayer;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_triggered && oneShot) return;
            if (!other.CompareTag("Player")) return;

            // Suppress discovery triggers during first 5s after spawn (scene load grace period)
            if (action == TriggerAction.DiscoverBuilding && Time.time - _spawnTime <= 5f) return;

            _triggered = true;

            switch (action)
            {
                case TriggerAction.DiscoverBuilding:
                    HandleBuildingDiscovery();
                    break;
                case TriggerAction.SpawnEnemy:
                    HandleEnemySpawn();
                    break;
                case TriggerAction.RevealPOI:
                    HandlePOIReveal();
                    break;
            }
        }

        void HandleBuildingDiscovery()
        {
            if (linkedBuilding == null)
            {
                linkedBuilding = GetComponentInParent<InteractableBuilding>();
                if (linkedBuilding == null)
                    linkedBuilding = GetComponentInChildren<InteractableBuilding>();
            }

            if (linkedBuilding != null)
            {
                linkedBuilding.Discover();
                Tartaria.Audio.AudioManager.Instance?.PlaySFX2D("BuildingDiscovered");
                Debug.Log($"[ProximityTrigger] Building discovered: {linkedBuilding.BuildingId}");
            }
        }

        void HandleEnemySpawn()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[ProximityTrigger] No enemy prefab assigned.");
                return;
            }

            var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemy.name = $"MudGolem_{transform.position.x:F0}_{transform.position.z:F0}";
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("EnemySpawn", transform.position);
            Debug.Log($"[ProximityTrigger] Enemy spawned: {enemy.name}");
        }

        void HandlePOIReveal()
        {
            if (string.IsNullOrEmpty(poiId)) return;

            var scanner = ResonanceScannerSystem.Instance;
            if (scanner != null)
            {
                scanner.RegisterPOI(new ScanPOI
                {
                    poiId = poiId,
                    position = transform.position,
                    poiType = ScanPOIType.BuriedStructure,
                    isRevealed = true
                });
                Tartaria.Input.HapticFeedbackManager.Instance?.PlayDiscovery();
                Debug.Log($"[ProximityTrigger] POI revealed: {poiId}");
            }
        }

        /// <summary>
        /// Configure this trigger at runtime (used by BuildingSpawner).
        /// </summary>
        public void Configure(TriggerAction triggerAction, float radius, InteractableBuilding building = null)
        {
            action = triggerAction;
            triggerRadius = radius;
            linkedBuilding = building;
            if (_collider != null)
            {
                float maxScale = Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.y,
                    transform.lossyScale.z);
                _collider.radius = maxScale > 0.01f ? radius / maxScale : radius;
            }
        }
    }
}
