using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Hero Building Spawner — Creates the 3 main structures: Cathedral, Fountain, Spire
    /// Each is larger and more detailed than village buildings
    /// Uses advanced KayKit rock compositions + architectural elements
    /// </summary>
    [DefaultExecutionOrder(-86)] // Before Moon1LevelBuilder (-85)
    public class Moon1HeroBuildingSpawner : MonoBehaviour
    {
        [Header("Hero Buildings")]
        [SerializeField] Vector3 cathedralPosition = new Vector3(0f, 0f, 80f);
        [SerializeField] Vector3 fountainPosition = new Vector3(-60f, 0f, 40f);
        [SerializeField] Vector3 spirePosition = new Vector3(60f, 0f, 40f);

        [Header("KayKit Assets")]
        [SerializeField] GameObject[] largePillarRocks; // Tall rocks for pillars
        [SerializeField] GameObject[] foundationRocks; // Wide/flat rocks for bases
        [SerializeField] GameObject[] domeRocks; // Rounded rocks for domes
        [SerializeField] GameObject[] decorativeRocks; // Small detail rocks

        [Header("Materials")]
        [SerializeField] Material marbleMaterial;
        [SerializeField] Material goldTrimMaterial;
        [SerializeField] Material crystalMaterial;
        [SerializeField] Material waterMaterial;

        [Header("Building Definitions")]
        [SerializeField] BuildingDefinition cathedralDefinition;
        [SerializeField] BuildingDefinition fountainDefinition;
        [SerializeField] BuildingDefinition spireDefinition;

        const float PHI = 1.618033988749895f;

        void Start()
        {
            SpawnHeroBuildings();
        }

        void SpawnHeroBuildings()
        {
            Debug.Log("[Moon1HeroBuildingSpawner] Spawning hero buildings...");

            LoadMaterials();

            // Create parent
            var heroParent = new GameObject("Hero_Buildings");
            heroParent.transform.position = Vector3.zero;

            // Spawn the 3 hero structures
            CreateCathedral(heroParent);
            CreateFountain(heroParent);
            CreateSpire(heroParent);

            Debug.Log("[Moon1HeroBuildingSpawner] ✅ 3 hero buildings spawned!");
        }

        void LoadMaterials()
        {
            if (marbleMaterial == null)
                marbleMaterial = Resources.Load<Material>("Materials/PBR/Marble006");
            if (goldTrimMaterial == null)
                goldTrimMaterial = Resources.Load<Material>("Materials/PBR/Metal048A");
            if (crystalMaterial == null)
                crystalMaterial = Resources.Load<Material>("Materials/M_Crystal_Aether");
            if (waterMaterial == null)
                waterMaterial = Resources.Load<Material>("Materials/M_Water_Harmonic");

            // Fallbacks
            if (marbleMaterial == null)
            {
                marbleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                marbleMaterial.color = new Color(0.9f, 0.9f, 0.85f);
            }
            if (goldTrimMaterial == null)
            {
                goldTrimMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                goldTrimMaterial.color = new Color(1f, 0.84f, 0f);
                goldTrimMaterial.SetFloat("_Metallic", 0.9f);
            }
        }

        void CreateCathedral(GameObject parent)
        {
            var cathedral = new GameObject("Echohaven_Cathedral");
            cathedral.transform.SetParent(parent.transform);
            cathedral.transform.position = cathedralPosition;

            // Base platform: 40m × 25m
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Cathedral_Platform";
            platform.transform.SetParent(cathedral.transform);
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localScale = new Vector3(40f, 1f, 25f);
            platform.GetComponent<Renderer>().material = marbleMaterial;
            Destroy(platform.GetComponent<Collider>());

            // Main nave: 20m × 15m × 18m high
            var nave = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nave.name = "Cathedral_Nave";
            nave.transform.SetParent(cathedral.transform);
            nave.transform.localPosition = new Vector3(0f, 9.5f, 0f);
            nave.transform.localScale = new Vector3(20f, 18f, 15f);
            nave.GetComponent<Renderer>().material = marbleMaterial;

            // 6 pillars (3 per side)
            for (int side = 0; side < 2; side++)
            {
                float xOffset = side == 0 ? -9f : 9f;
                for (int i = 0; i < 3; i++)
                {
                    float zOffset = (i - 1) * 5f;
                    CreatePillar(cathedral, new Vector3(xOffset, 0f, zOffset), 16f);
                }
            }

            // Rose window (front)
            var window = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            window.name = "Rose_Window";
            window.transform.SetParent(cathedral.transform);
            window.transform.localPosition = new Vector3(0f, 12f, -7.5f);
            window.transform.localScale = Vector3.one * 6f;
            if (crystalMaterial != null)
                window.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(window.GetComponent<Collider>());

            // Dome (top)
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Cathedral_Dome";
            dome.transform.SetParent(cathedral.transform);
            dome.transform.localPosition = new Vector3(0f, 20f, 0f);
            dome.transform.localScale = new Vector3(12f, 8f, 12f);
            dome.GetComponent<Renderer>().material = goldTrimMaterial;
            Destroy(dome.GetComponent<Collider>());

            // Add InteractableBuilding component
            var interactable = cathedral.AddComponent<InteractableBuilding>();
            if (cathedralDefinition != null)
                interactable.SetDefinition(cathedralDefinition);
            interactable.SetMaterials(marbleMaterial, goldTrimMaterial);

            // Add BoxCollider for interaction
            var collider = cathedral.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 9f, 0f);
            collider.size = new Vector3(40f, 20f, 25f);

            Debug.Log($"  ✓ Cathedral spawned at {cathedralPosition} (40m × 25m × 20m)");
        }

        void CreateFountain(GameObject parent)
        {
            var fountain = new GameObject("Echohaven_HarmonicFountain");
            fountain.transform.SetParent(parent.transform);
            fountain.transform.position = fountainPosition;

            // Base pool: 16m diameter
            var pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pool.name = "Fountain_Pool";
            pool.transform.SetParent(fountain.transform);
            pool.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            pool.transform.localScale = new Vector3(16f, 1f, 16f);
            pool.GetComponent<Renderer>().material = marbleMaterial;

            // Water surface
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "Fountain_Water";
            water.transform.SetParent(fountain.transform);
            water.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            water.transform.localScale = new Vector3(15f, 0.1f, 15f);
            if (waterMaterial != null)
                water.GetComponent<Renderer>().material = waterMaterial;
            Destroy(water.GetComponent<Collider>());

            // Central column: 3 tiers
            float[] tierHeights = { 2f, 5f, 8f };
            float[] tierRadii = { 3f, 2f, 1f };
            for (int i = 0; i < 3; i++)
            {
                var tier = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tier.name = $"Fountain_Tier_{i + 1}";
                tier.transform.SetParent(fountain.transform);
                tier.transform.localPosition = new Vector3(0f, tierHeights[i], 0f);
                tier.transform.localScale = new Vector3(tierRadii[i] * 2f, 0.5f, tierRadii[i] * 2f);
                tier.GetComponent<Renderer>().material = goldTrimMaterial;
                Destroy(tier.GetComponent<Collider>());
            }

            // Top crystal
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "Fountain_Crystal";
            crystal.transform.SetParent(fountain.transform);
            crystal.transform.localPosition = new Vector3(0f, 9f, 0f);
            crystal.transform.localScale = Vector3.one * 2f;
            if (crystalMaterial != null)
                crystal.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(crystal.GetComponent<Collider>());

            // Add InteractableBuilding component
            var interactable = fountain.AddComponent<InteractableBuilding>();
            if (fountainDefinition != null)
                interactable.SetDefinition(fountainDefinition);
            interactable.SetMaterials(marbleMaterial, goldTrimMaterial);

            // Add CapsuleCollider for interaction
            var collider = fountain.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 4.5f, 0f);
            collider.radius = 8f;
            collider.height = 10f;

            Debug.Log($"  ✓ Fountain spawned at {fountainPosition} (16m diameter × 9m high)");
        }

        void CreateSpire(GameObject parent)
        {
            var spire = new GameObject("Echohaven_CrystalSpire");
            spire.transform.SetParent(parent.transform);
            spire.transform.position = spirePosition;

            // Base platform: 12m × 12m
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Spire_Platform";
            platform.transform.SetParent(spire.transform);
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localScale = new Vector3(12f, 1f, 12f);
            platform.GetComponent<Renderer>().material = marbleMaterial;
            Destroy(platform.GetComponent<Collider>());

            // 4 corner supports
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(4f, 0f, 0f);
                CreatePillar(spire, offset, 24f);
            }

            // Central crystal shaft: 30m tall
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Spire_Shaft";
            shaft.transform.SetParent(spire.transform);
            shaft.transform.localPosition = new Vector3(0f, 15f, 0f);
            shaft.transform.localScale = new Vector3(2f, 15f, 2f);
            if (crystalMaterial != null)
                shaft.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(shaft.GetComponent<Collider>());

            // Top pyramid
            var pyramid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pyramid.name = "Spire_Top";
            pyramid.transform.SetParent(spire.transform);
            pyramid.transform.localPosition = new Vector3(0f, 30f, 0f);
            pyramid.transform.localScale = new Vector3(3f, 6f, 3f);
            if (crystalMaterial != null)
                pyramid.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(pyramid.GetComponent<Collider>());

            // Add InteractableBuilding component
            var interactable = spire.AddComponent<InteractableBuilding>();
            if (spireDefinition != null)
                interactable.SetDefinition(spireDefinition);
            interactable.SetMaterials(marbleMaterial, goldTrimMaterial);

            // Add CapsuleCollider for interaction
            var collider = spire.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 15f, 0f);
            collider.radius = 6f;
            collider.height = 32f;

            Debug.Log($"  ✓ Spire spawned at {spirePosition} (12m base × 30m high)");
        }

        void CreatePillar(GameObject parent, Vector3 localPosition, float height)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(parent.transform);
            pillar.transform.localPosition = localPosition;
            pillar.transform.localScale = new Vector3(1f, height / 2f, 1f);
            pillar.GetComponent<Renderer>().material = marbleMaterial;
            Destroy(pillar.GetComponent<Collider>());
        }
    }
}
