using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Environment Decorator — The Molten Forge
    /// Spawns lava rocks, ember particles, forge equipment, and volcanic formations
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon6EnvironmentDecorator : MonoBehaviour
    {
        [Header("Forge Props")]
        [SerializeField] int lavaRockCount = 50;
        [SerializeField] int anvilCount = 8;
        [SerializeField] int emberClusterCount = 30;
        [SerializeField] int obsidianFormationCount = 20;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon6EnvironmentDecorator] 🔥 Decorating molten forge with {lavaRockCount + anvilCount + emberClusterCount + obsidianFormationCount} props...");

            // Lava Rocks - scattered everywhere
            for (int i = 0; i < lavaRockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    Random.Range(0.3f, 1f),
                    Random.Range(-110f, 110f)
                );
                float scale = Random.Range(0.5f, 2f);
                CreateProp($"LavaRock_{i}", pos, Vector3.one * scale, new Color(0.3f, 0.2f, 0.15f), PrimitiveType.Sphere);
            }

            // Anvils - at forging stations
            for (int i = 0; i < anvilCount; i++)
            {
                float angle = i * (360f / anvilCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 55f,
                    0.8f,
                    Mathf.Sin(angle) * 55f
                );
                CreateProp($"Anvil_{i}", pos, new Vector3(1.5f, 0.8f, 0.8f), new Color(0.4f, 0.4f, 0.4f), PrimitiveType.Cube);
            }

            // Ember Clusters - glowing spots
            for (int i = 0; i < emberClusterCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    0.5f,
                    Random.Range(-100f, 100f)
                );
                CreateProp($"EmberCluster_{i}", pos, Vector3.one * 0.6f, new Color(1f, 0.5f, 0f), PrimitiveType.Sphere);
            }

            // Obsidian Formations - sharp volcanic glass
            for (int i = 0; i < obsidianFormationCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(-90f, 90f)
                );
                CreateProp($"ObsidianFormation_{i}", pos, new Vector3(0.8f, Random.Range(2f, 4f), 0.8f), new Color(0.1f, 0.1f, 0.15f), PrimitiveType.Cube);
            }

            Debug.Log($"[Moon6EnvironmentDecorator] ✅ Molten forge decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            prop.transform.SetParent(transform);

            var renderer = prop.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.material = mat;

            spawnedProps.Add(prop);
        }

        void OnDestroy()
        {
            foreach (var prop in spawnedProps)
            {
                if (prop != null) Destroy(prop);
            }
        }
    }
}
