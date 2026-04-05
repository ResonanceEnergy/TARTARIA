using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Building Spawner — wires up the MonoBehaviour side of Tartarian buildings:
    /// InteractableBuilding + ProximityTrigger + colliders.
    /// Runs after WorldInitializer creates ECS entities.
    ///
    /// First looks for existing scene buildings placed by EchohavenScenePopulator.
    /// Only creates greybox fallbacks if no matching object is found.
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

        // Scene object names from EchohavenScenePopulator
        static readonly string[] DomeNames = { "StarDome_Placeholder", "Echohaven_StarDome" };
        static readonly string[] FountainNames = { "HarmonicFountain_Placeholder", "Echohaven_HarmonicFountain" };
        static readonly string[] SpireNames = { "CrystalSpire_Placeholder", "Echohaven_CrystalSpire" };

        void Start()
        {
            WireBuilding("dome", domePosition, DomeNames, PrimitiveType.Sphere, new Vector3(8f, 6f, 8f));
            WireBuilding("fountain", fountainPosition, FountainNames, PrimitiveType.Cylinder, new Vector3(4f, 3f, 4f));
            WireBuilding("spire", spirePosition, SpireNames, PrimitiveType.Cylinder, new Vector3(3f, 12f, 3f));

            Debug.Log("[BuildingSpawner] 3 buildings wired with interaction + discovery triggers.");
        }

        void WireBuilding(string buildingId, Vector3 position, string[] sceneNames,
            PrimitiveType fallbackShape, Vector3 fallbackScale)
        {
            // Try to find existing scene object first
            GameObject building = null;
            foreach (var name in sceneNames)
            {
                building = GameObject.Find(name);
                if (building != null) break;
            }

            // Fallback: create greybox if nothing in scene
            if (building == null)
            {
                building = CreateGreyboxBuilding(buildingId, position, fallbackShape, fallbackScale);
                building.name = $"Building_{buildingId}";
            }

            // Ensure InteractableBuilding component
            var interactable = building.GetComponent<InteractableBuilding>();
            if (interactable == null)
                interactable = building.AddComponent<InteractableBuilding>();

            // Ensure collider for interaction raycasts
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

            // Add discovery proximity trigger as child (if not already present)
            string triggerName = $"DiscoveryTrigger_{buildingId}";
            if (building.transform.Find(triggerName) == null)
            {
                var triggerGO = new GameObject(triggerName);
                triggerGO.transform.SetParent(building.transform, false);
                var trigger = triggerGO.AddComponent<ProximityTrigger>();
                trigger.Configure(ProximityTrigger.TriggerAction.DiscoverBuilding, discoveryRadius, interactable);
            }

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
