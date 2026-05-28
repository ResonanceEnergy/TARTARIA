using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Building Prefab Creator — Generates prefabs from KayKit rock compositions
    /// Creates 5 building types: Small House, Large House, Tower, Temple, Workshop
    /// Each uses 8-16 modular rock pieces with PBR materials
    /// </summary>
    public class Moon1BuildingPrefabCreator : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Source Assets")]
        [SerializeField] GameObject[] rockPrefabs;
        [SerializeField] Material rockMaterial;
        [SerializeField] Material marbleMaterial;
        [SerializeField] Material bricksMaterial;

        [Header("Output")]
        [SerializeField] string prefabOutputPath = "Assets/_Project/Prefabs/Buildings/Moon1/";

        [ContextMenu("Create All Building Prefabs")]
        public void CreateAllBuildingPrefabs()
        {
            if (rockPrefabs == null || rockPrefabs.Length == 0)
            {
                Debug.LogError("[Moon1BuildingPrefabCreator] No rock prefabs assigned!");
                return;
            }

            Debug.Log("[Moon1BuildingPrefabCreator] Creating building prefabs...");

            // Ensure output directory exists
            if (!AssetDatabase.IsValidFolder(prefabOutputPath))
            {
                System.IO.Directory.CreateDirectory(prefabOutputPath);
                AssetDatabase.Refresh();
            }

            CreateSmallHousePrefab();
            CreateLargeHousePrefab();
            CreateTowerPrefab();
            CreateTemplePrefab();
            CreateWorkshopPrefab();

            Debug.Log("[Moon1BuildingPrefabCreator] ✅ Created 5 building prefabs!");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void CreateSmallHousePrefab()
        {
            var house = new GameObject("Moon1_SmallHouse");
            
            // Base: 4 large rocks in corners
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(2f, 0f, 2f);
                InstantiateRock(house, offset, Vector3.one * 1.2f, rockMaterial);
            }

            // Walls: 4 medium rocks
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(2.5f, 1f, 0f);
                InstantiateRock(house, offset, Vector3.one * 0.8f, bricksMaterial);
            }

            SavePrefab(house, "Moon1_SmallHouse");
        }

        void CreateLargeHousePrefab()
        {
            var house = new GameObject("Moon1_LargeHouse");
            
            // Base: 8 rocks in octagon
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(3f, 0f, 0f);
                InstantiateRock(house, offset, Vector3.one * 1.5f, rockMaterial);
            }

            // Upper level: 4 rocks
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(2f, 2.5f, 2f);
                InstantiateRock(house, offset, Vector3.one * 1.0f, bricksMaterial);
            }

            SavePrefab(house, "Moon1_LargeHouse");
        }

        void CreateTowerPrefab()
        {
            var tower = new GameObject("Moon1_Tower");
            
            // Vertical stack: 12 rocks
            for (int level = 0; level < 4; level++)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = i * 120f + (level * 30f); // Rotate each level
                    float radius = 1.5f - (level * 0.1f); // Taper slightly
                    Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, level * 2f, 0f);
                    InstantiateRock(tower, offset, Vector3.one * (1.2f - level * 0.1f), rockMaterial);
                }
            }

            // Top dome
            Vector3 domePos = Vector3.up * 8f;
            InstantiateRock(tower, domePos, Vector3.one * 1.5f, marbleMaterial);

            SavePrefab(tower, "Moon1_Tower");
        }

        void CreateTemplePrefab()
        {
            var temple = new GameObject("Moon1_Temple");
            
            // Wide base: 12 rocks in circle
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(4f, 0f, 0f);
                InstantiateRock(temple, offset, Vector3.one * 1.8f, marbleMaterial);
            }

            // 4 corner pillars
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(3f, 2f, 3f);
                InstantiateRock(temple, offset, new Vector3(0.6f, 2.5f, 0.6f), marbleMaterial);
            }

            SavePrefab(temple, "Moon1_Temple");
        }

        void CreateWorkshopPrefab()
        {
            var workshop = new GameObject("Moon1_Workshop");
            
            // Long rectangular base: 6 rocks in 2 rows
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Vector3 offset = new Vector3(col * 2f - 2f, 0f, row * 2f - 1f);
                    InstantiateRock(workshop, offset, new Vector3(1.2f, 1f, 1.2f), rockMaterial);
                }
            }

            // Chimney: 3 stacked rocks
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(2f, i * 1.5f + 1f, 0f);
                InstantiateRock(workshop, offset, new Vector3(0.5f, 0.8f, 0.5f), bricksMaterial);
            }

            SavePrefab(workshop, "Moon1_Workshop");
        }

        void InstantiateRock(GameObject parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            if (rockPrefabs == null || rockPrefabs.Length == 0) return;

            GameObject rockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
            GameObject rock = Instantiate(rockPrefab, parent.transform);
            rock.transform.localPosition = localPosition;
            rock.transform.localScale = scale;
            rock.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Apply material
            var renderers = rock.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (material != null)
                    rend.material = material;
            }
        }

        void SavePrefab(GameObject obj, string name)
        {
            string path = prefabOutputPath + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);
            Debug.Log($"[Moon1BuildingPrefabCreator] ✓ Saved {name} to {path}");
        }
#endif
    }
}
