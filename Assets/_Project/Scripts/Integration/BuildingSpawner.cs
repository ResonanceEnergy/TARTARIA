using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Building Spawner — creates the MonoBehaviour side of Tartarian buildings:
    /// greybox mesh + InteractableBuilding + ProximityTrigger + colliders.
    /// Runs after WorldInitializer creates ECS entities.
    ///
    /// Place this in the gameplay scene (Echohaven_VerticalSlice).
    /// Positions match WorldInitializer's serialized building positions.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-80)] // After WorldInitializer (-90), before GameLoopController (-50)
    public class BuildingSpawner : MonoBehaviour
    {
        [Header("Building Positions (must match WorldInitializer)")]
        [SerializeField] Vector3 domePosition = new(30f, 0f, 20f);
        [SerializeField] Vector3 fountainPosition = new(-20f, 0f, 35f);
        [SerializeField] Vector3 spirePosition = new(0f, 0f, -30f);

        [Header("Discovery Radius")]
        [SerializeField] float discoveryRadius = 15f;

        [Header("Prefabs (optional — falls back to greybox if null)")]
        [SerializeField] GameObject domePrefab;
        [SerializeField] GameObject fountainPrefab;
        [SerializeField] GameObject spirePrefab;

        void Start()
        {
            SpawnBuilding("dome", domePosition, domePrefab, PrimitiveType.Sphere, new Vector3(8f, 6f, 8f));
            SpawnBuilding("fountain", fountainPosition, fountainPrefab, PrimitiveType.Cylinder, new Vector3(4f, 3f, 4f));
            SpawnBuilding("spire", spirePosition, spirePrefab, PrimitiveType.Cylinder, new Vector3(3f, 12f, 3f));

            Debug.Log("[BuildingSpawner] 3 buildings spawned with interaction + discovery triggers.");
        }

        void SpawnBuilding(string buildingId, Vector3 position, GameObject prefab,
            PrimitiveType fallbackShape, Vector3 fallbackScale)
        {
            GameObject building;

            if (prefab != null)
            {
                building = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                building = CreateGreyboxBuilding(buildingId, position, fallbackShape, fallbackScale);
            }

            building.name = $"Building_{buildingId}";

            // Ensure InteractableBuilding component
            var interactable = building.GetComponent<InteractableBuilding>();
            if (interactable == null)
                interactable = building.AddComponent<InteractableBuilding>();

            // Ensure box collider for interaction raycasts
            var col = building.GetComponent<Collider>();
            if (col == null)
            {
                var box = building.AddComponent<BoxCollider>();
                box.size = fallbackScale;
            }

            // Set building layer
            int buildingLayer = LayerMask.NameToLayer("Building");
            if (buildingLayer >= 0)
                building.layer = buildingLayer;

            // Add discovery proximity trigger as child
            var triggerGO = new GameObject($"DiscoveryTrigger_{buildingId}");
            triggerGO.transform.SetParent(building.transform, false);
            var trigger = triggerGO.AddComponent<ProximityTrigger>();
            trigger.Configure(ProximityTrigger.TriggerAction.DiscoverBuilding, discoveryRadius, interactable);

            // Register as scanner POI
            var scanner = ResonanceScannerSystem.Instance;
            if (scanner != null)
            {
                scanner.RegisterPOI(new ScanPOI
                {
                    poiId = buildingId,
                    poiType = ScanPOIType.BuriedStructure,
                    position = position,
                    isRevealed = false
                });
            }
        }

        GameObject CreateGreyboxBuilding(string id, Vector3 position,
            PrimitiveType shape, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.transform.position = position + Vector3.up * (scale.y * 0.5f);
            go.transform.localScale = scale;

            // Mud-colored material
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.45f, 0.35f, 0.25f); // Mud brown
                renderer.material = mat;
            }

            return go;
        }
    }
}
