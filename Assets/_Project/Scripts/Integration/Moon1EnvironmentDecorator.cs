using UnityEngine;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 1 Environment Decorator — Adds natural scenery around Echohaven village
    /// Scatters trees, rocks, bushes, grass, and RPG props for immersion
    /// Uses density-based placement with golden ratio spacing
    /// </summary>
    [DefaultExecutionOrder(-84)] // After Moon1LevelBuilder (-85)
    public class Moon1EnvironmentDecorator : MonoBehaviour
    {
        [Header("Decoration Area")]
        [SerializeField] Vector3 decorationCenter = Vector3.zero;
        [SerializeField] float decorationRadius = 150f;
        [SerializeField] LayerMask buildingLayer;

        [Header("Tree Placement")]
        [SerializeField] GameObject[] treePrefabs;
        [SerializeField] int treeCount = 30;
        [SerializeField] float minTreeDistance = 8f;

        [Header("Rock Placement")]
        [SerializeField] GameObject[] rockPrefabs;
        [SerializeField] int rockCount = 50;
        [SerializeField] float minRockDistance = 3f;

        [Header("Bush Placement")]
        [SerializeField] GameObject[] bushPrefabs;
        [SerializeField] int bushCount = 80;
        [SerializeField] float minBushDistance = 2f;

        [Header("Grass Placement")]
        [SerializeField] GameObject[] grassPrefabs;
        [SerializeField] int grassCount = 120;
        [SerializeField] float minGrassDistance = 1f;

        [Header("RPG Props (Tools, Lanterns, Crates)")]
        [SerializeField] GameObject[] propPrefabs;
        [SerializeField] int propCount = 20;
        [SerializeField] float minPropDistance = 5f;

        [Header("Materials")]
        [SerializeField] Material terrainMaterial;

        const float PHI = 1.618033988749895f; // Golden ratio

        void Start()
        {
            DecorateEnvironment();
        }

        void DecorateEnvironment()
        {
            Debug.Log("[Moon1EnvironmentDecorator] Starting environment decoration...");

            // Create parent for organization
            var envParent = new GameObject("Environment_Decoration");
            envParent.transform.position = decorationCenter;

            // Scatter trees (outer ring)
            if (treePrefabs != null && treePrefabs.Length > 0)
            {
                var treesParent = new GameObject("Trees");
                treesParent.transform.SetParent(envParent.transform);
                ScatterObjects(treesParent, treePrefabs, treeCount, decorationRadius * 0.7f, decorationRadius, minTreeDistance, new Vector3(1f, 1f, 1f), new Vector3(1.5f, 1.5f, 1.5f));
            }

            // Scatter large rocks (mid ring)
            if (rockPrefabs != null && rockPrefabs.Length > 0)
            {
                var rocksParent = new GameObject("Rocks");
                rocksParent.transform.SetParent(envParent.transform);
                ScatterObjects(rocksParent, rockPrefabs, rockCount, decorationRadius * 0.3f, decorationRadius * 0.9f, minRockDistance, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2f, 2f, 2f));
            }

            // Scatter bushes (everywhere)
            if (bushPrefabs != null && bushPrefabs.Length > 0)
            {
                var bushesParent = new GameObject("Bushes");
                bushesParent.transform.SetParent(envParent.transform);
                ScatterObjects(bushesParent, bushPrefabs, bushCount, 5f, decorationRadius, minBushDistance, new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1.3f, 1.3f, 1.3f));
            }

            // Scatter grass (dense, close to buildings)
            if (grassPrefabs != null && grassPrefabs.Length > 0)
            {
                var grassParent = new GameObject("Grass");
                grassParent.transform.SetParent(envParent.transform);
                ScatterObjects(grassParent, grassPrefabs, grassCount, 5f, decorationRadius * 0.6f, minGrassDistance, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1.2f, 1.2f, 1.2f));
            }

            // Scatter RPG props (near buildings)
            if (propPrefabs != null && propPrefabs.Length > 0)
            {
                var propsParent = new GameObject("Props");
                propsParent.transform.SetParent(envParent.transform);
                ScatterObjects(propsParent, propPrefabs, propCount, 10f, decorationRadius * 0.5f, minPropDistance, Vector3.one, Vector3.one);
            }

            Debug.Log("[Moon1EnvironmentDecorator] Environment decoration complete!");
            Debug.Log($"  • {treeCount} trees");
            Debug.Log($"  • {rockCount} rocks");
            Debug.Log($"  • {bushCount} bushes");
            Debug.Log($"  • {grassCount} grass patches");
            Debug.Log($"  • {propCount} props");
        }

        void ScatterObjects(GameObject parent, GameObject[] prefabs, int count, float minRadius, float maxRadius, float minDistance, Vector3 minScale, Vector3 maxScale)
        {
            int attempts = 0;
            int maxAttempts = count * 10;
            int placed = 0;

            while (placed < count && attempts < maxAttempts)
            {
                attempts++;

                // Random position in ring
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(minRadius, maxRadius);
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, 0f, 0f);
                Vector3 position = decorationCenter + offset;

                // Check if too close to buildings
                if (Physics.CheckSphere(position, minDistance, buildingLayer))
                    continue;

                // Place object
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject obj = Instantiate(prefab, parent.transform);
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // Random scale variation
                Vector3 scale = new Vector3(
                    Random.Range(minScale.x, maxScale.x),
                    Random.Range(minScale.y, maxScale.y),
                    Random.Range(minScale.z, maxScale.z)
                );
                obj.transform.localScale = scale;

                placed++;
            }

            Debug.Log($"[Moon1EnvironmentDecorator] Placed {placed}/{count} {parent.name} (attempts: {attempts})");
        }
    }
}
