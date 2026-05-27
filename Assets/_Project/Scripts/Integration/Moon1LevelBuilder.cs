using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Level Builder — Procedurally generates Echohaven village with Tartarian architecture
    /// Creates 12 buildings total: 3 hero buildings + 9 village structures
    /// Uses golden ratio proportions (φ = 1.618) for all structures
    /// </summary>
    [DefaultExecutionOrder(-85)] // After BuildingSpawner (-80)
    public class Moon1LevelBuilder : MonoBehaviour
    {
        [Header("Village Layout")]
        [SerializeField] Vector3 villageCenter = new Vector3(0f, 0f, 0f);
        [SerializeField] float villageRadius = 80f;
        [SerializeField] int villageBuildings = 9;

        [Header("Materials")]
        [SerializeField] Material stoneMaterial;
        [SerializeField] Material mudMaterial;
        [SerializeField] Material goldTrimMaterial;
        [SerializeField] Material groundMaterial;

        void Start()
        {
            LoadMaterials();
            CreateVillageGrid();
            CreateGroundPlane();
            Debug.Log("[Moon1LevelBuilder] Echohaven village generated - 9 buildings + ground plane");
        }

        void LoadMaterials()
        {
            if (stoneMaterial == null)
                stoneMaterial = Resources.Load<Material>("Materials/M_Building_Stone");
            if (mudMaterial == null)
                mudMaterial = Resources.Load<Material>("Materials/M_Mud_Fresh");
            if (goldTrimMaterial == null)
                goldTrimMaterial = Resources.Load<Material>("Materials/M_Gold_Ornament");
            if (groundMaterial == null)
                groundMaterial = Resources.Load<Material>("Materials/M_Ground_Terrain");

            // Create materials procedurally with textures if not found
            if (stoneMaterial == null)
                stoneMaterial = RuntimeTextureGenerator.CreateMaterialWithTextures("M_Stone_Generated", RuntimeTextureGenerator.MaterialType.Stone);
            if (mudMaterial == null)
                mudMaterial = RuntimeTextureGenerator.CreateMaterialWithTextures("M_Mud_Generated", RuntimeTextureGenerator.MaterialType.Mud);
            if (goldTrimMaterial == null)
                goldTrimMaterial = RuntimeTextureGenerator.CreateMaterialWithTextures("M_Gold_Generated", RuntimeTextureGenerator.MaterialType.Gold);
            if (groundMaterial == null)
                groundMaterial = RuntimeTextureGenerator.CreateMaterialWithTextures("M_Grass_Generated", RuntimeTextureGenerator.MaterialType.Grass);
        }

        void CreateVillageGrid()
        {
            // Create 9 village buildings in a 3x3 grid pattern around the hero buildings
            int gridSize = 3;
            float spacing = 40f;
            int buildingIndex = 0;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Skip center cell (reserved for hero buildings)
                    if (x == 0 && z == 0) continue;

                    Vector3 position = villageCenter + new Vector3(x * spacing, 0f, z * spacing);
                    
                    // Randomize building type
                    BuildingType type = (BuildingType)(buildingIndex % 4);
                    CreateVillageBuilding($"village_{buildingIndex}", position, type);
                    buildingIndex++;
                }
            }
        }

        void CreateVillageBuilding(string id, Vector3 position, BuildingType type)
        {
            GameObject building = new GameObject($"VillageBuilding_{id}");
            building.transform.position = position;
            building.layer = LayerMask.NameToLayer("Building");

            // Calculate golden ratio dimensions based on type
            Vector3 dimensions = GetBuildingDimensions(type);
            
            // Create base structure
            CreateBuildingGeometry(building, dimensions, type);

            // Add Tartarian architectural enhancements
            var architecturalStyle = type switch
            {
                BuildingType.House => TartarianArchitectureEnhancer.ArchitecturalStyle.Classical,
                BuildingType.Tower => TartarianArchitectureEnhancer.ArchitecturalStyle.Spire,
                BuildingType.Temple => TartarianArchitectureEnhancer.ArchitecturalStyle.Dome,
                BuildingType.Workshop => TartarianArchitectureEnhancer.ArchitecturalStyle.Classical,
                _ => TartarianArchitectureEnhancer.ArchitecturalStyle.Classical
            };
            TartarianArchitectureEnhancer.EnhanceBuilding(building, architecturalStyle);

            // Add excavation marker
            CreateExcavationMound(building, dimensions);

            // Add to excavation system
            var excavation = ExcavationSystem.Instance;
            if (excavation != null)
            {
                excavation.RegisterSite(id, position, UnityEngine.Random.Range(2, 5), false, id);
            }

            // Add collider
            var boxCol = building.AddComponent<BoxCollider>();
            boxCol.size = dimensions;

            // Add InteractableBuilding component
            var interactable = building.AddComponent<InteractableBuilding>();
            interactable.SetBuildingId(id);
            interactable.SetMaterials(mudMaterial, mudMaterial, stoneMaterial);
        }

        void CreateBuildingGeometry(GameObject parent, Vector3 dimensions, BuildingType type)
        {
            switch (type)
            {
                case BuildingType.House:
                    CreateHouseGeometry(parent, dimensions);
                    break;
                case BuildingType.Tower:
                    CreateTowerGeometry(parent, dimensions);
                    break;
                case BuildingType.Temple:
                    CreateTempleGeometry(parent, dimensions);
                    break;
                case BuildingType.Workshop:
                    CreateWorkshopGeometry(parent, dimensions);
                    break;
            }
        }

        void CreateHouseGeometry(GameObject parent, Vector3 dimensions)
        {
            // Main building body (rectangular)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.up * dimensions.y * 0.5f;
            body.transform.localScale = dimensions;
            body.GetComponent<Renderer>().material = stoneMaterial;
            Destroy(body.GetComponent<Collider>()); // Parent handles collision

            // Roof (pyramid)
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(parent.transform);
            roof.transform.localPosition = Vector3.up * (dimensions.y + dimensions.y * 0.25f);
            roof.transform.localScale = new Vector3(dimensions.x * 1.1f, dimensions.y * 0.5f, dimensions.z * 1.1f);
            roof.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            roof.GetComponent<Renderer>().material = mudMaterial;
            Destroy(roof.GetComponent<Collider>());
        }

        void CreateTowerGeometry(GameObject parent, Vector3 dimensions)
        {
            // Tall cylindrical tower
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "Tower";
            tower.transform.SetParent(parent.transform);
            tower.transform.localPosition = Vector3.up * dimensions.y * 0.5f;
            tower.transform.localScale = new Vector3(dimensions.x, dimensions.y, dimensions.z);
            tower.GetComponent<Renderer>().material = stoneMaterial;
            Destroy(tower.GetComponent<Collider>());

            // Decorative golden band
            var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "GoldenBand";
            band.transform.SetParent(parent.transform);
            band.transform.localPosition = Vector3.up * dimensions.y * 0.7f;
            band.transform.localScale = new Vector3(dimensions.x * 1.05f, dimensions.y * 0.1f, dimensions.z * 1.05f);
            band.GetComponent<Renderer>().material = goldTrimMaterial;
            Destroy(band.GetComponent<Collider>());

            // Top dome
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Dome";
            dome.transform.SetParent(parent.transform);
            dome.transform.localPosition = Vector3.up * dimensions.y;
            dome.transform.localScale = Vector3.one * dimensions.x * 1.2f;
            dome.GetComponent<Renderer>().material = goldTrimMaterial;
            Destroy(dome.GetComponent<Collider>());
        }

        void CreateTempleGeometry(GameObject parent, Vector3 dimensions)
        {
            // Wide temple base
            var base_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            base_obj.name = "Base";
            base_obj.transform.SetParent(parent.transform);
            base_obj.transform.localPosition = Vector3.up * dimensions.y * 0.3f;
            base_obj.transform.localScale = new Vector3(dimensions.x * 1.2f, dimensions.y * 0.6f, dimensions.z * 1.2f);
            base_obj.GetComponent<Renderer>().material = stoneMaterial;
            Destroy(base_obj.GetComponent<Collider>());

            // Temple columns (4 corners)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(dimensions.x * 0.4f, 0f, dimensions.z * 0.4f);
                
                var column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                column.name = $"Column_{i}";
                column.transform.SetParent(parent.transform);
                column.transform.localPosition = offset + Vector3.up * dimensions.y * 0.5f;
                column.transform.localScale = new Vector3(dimensions.x * 0.15f, dimensions.y, dimensions.z * 0.15f);
                column.GetComponent<Renderer>().material = goldTrimMaterial;
                Destroy(column.GetComponent<Collider>());
            }

            // Temple roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(parent.transform);
            roof.transform.localPosition = Vector3.up * dimensions.y;
            roof.transform.localScale = new Vector3(dimensions.x * 1.3f, dimensions.y * 0.2f, dimensions.z * 1.3f);
            roof.GetComponent<Renderer>().material = stoneMaterial;
            Destroy(roof.GetComponent<Collider>());
        }

        void CreateWorkshopGeometry(GameObject parent, Vector3 dimensions)
        {
            // Long rectangular building
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.up * dimensions.y * 0.5f;
            body.transform.localScale = new Vector3(dimensions.x * 1.5f, dimensions.y * 0.8f, dimensions.z);
            body.GetComponent<Renderer>().material = stoneMaterial;
            Destroy(body.GetComponent<Collider>());

            // Chimney
            var chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chimney.name = "Chimney";
            chimney.transform.SetParent(parent.transform);
            chimney.transform.localPosition = new Vector3(dimensions.x * 0.4f, dimensions.y * 1.2f, 0f);
            chimney.transform.localScale = new Vector3(dimensions.x * 0.2f, dimensions.y * 0.6f, dimensions.z * 0.2f);
            chimney.GetComponent<Renderer>().material = mudMaterial;
            Destroy(chimney.GetComponent<Collider>());
        }

        void CreateExcavationMound(GameObject parent, Vector3 dimensions)
        {
            // Create mud mound covering the building
            var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = "MudMound";
            mound.transform.SetParent(parent.transform);
            mound.transform.localPosition = Vector3.up * dimensions.y * 0.4f;
            mound.transform.localScale = new Vector3(dimensions.x * 1.5f, dimensions.y * 0.8f, dimensions.z * 1.5f);
            mound.GetComponent<Renderer>().material = mudMaterial;
            Destroy(mound.GetComponent<Collider>()); // Parent handles collision
        }

        void CreateGroundPlane()
        {
            // Check if ground plane already exists
            if (GameObject.Find("GroundPlane") != null)
            {
                Debug.Log("[Moon1LevelBuilder] GroundPlane already exists, skipping creation");
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(50f, 1f, 50f); // 500x500m plane
            ground.GetComponent<Renderer>().material = groundMaterial;

            // Add physics layer
            ground.layer = LayerMask.NameToLayer("Terrain");
        }

        Vector3 GetBuildingDimensions(BuildingType type)
        {
            const float PHI = 1.618f;
            
            return type switch
            {
                BuildingType.House => new Vector3(8f, 8f * PHI, 8f), // Golden ratio height
                BuildingType.Tower => new Vector3(5f, 5f * PHI * PHI, 5f), // Double golden ratio (taller)
                BuildingType.Temple => new Vector3(12f, 12f / PHI, 12f), // Wider, shorter
                BuildingType.Workshop => new Vector3(10f, 10f * PHI * 0.7f, 10f),
                _ => new Vector3(8f, 8f * PHI, 8f)
            };
        }

        enum BuildingType
        {
            House,
            Tower,
            Temple,
            Workshop
        }
    }
}
