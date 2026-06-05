using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 5 Environment Decorator — The Frostbound Citadel
    /// Spawns ice crystals, snow drifts, icicles, and frozen pillars
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon5EnvironmentDecorator : MonoBehaviour
    {
        [Header("Ice Props")]
        [SerializeField] int iceCrystalCount = 30;
        [SerializeField] int snowDriftCount = 40;
        [SerializeField] int icicleCount = 35;
        [SerializeField] int frozenPillarCount = 15;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon5EnvironmentDecorator] ❄️ Decorating frozen citadel with {iceCrystalCount + snowDriftCount + icicleCount + frozenPillarCount} props...");

            // Ice Crystals - jutting from ground
            for (int i = 0; i < iceCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-100f, 100f)
                );
                float height = Random.Range(1f, 3f);
                CreateProp($"IceCrystal_{i}", pos, new Vector3(0.5f, height, 0.5f), new Color(0.8f, 0.9f, 1f), PrimitiveType.Cube);
            }

            // Snow Drifts - ground cover
            for (int i = 0; i < snowDriftCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    0.2f,
                    Random.Range(-110f, 110f)
                );
                CreateProp($"SnowDrift_{i}", pos, new Vector3(2f, 0.4f, 1.5f), new Color(0.95f, 0.95f, 1f), PrimitiveType.Sphere);
            }

            // Icicles - hanging from walls
            for (int i = 0; i < icicleCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * Random.Range(65f, 90f),
                    Random.Range(8f, 15f),
                    Mathf.Sin(angle) * Random.Range(65f, 90f)
                );
                CreateProp($"Icicle_{i}", pos, new Vector3(0.2f, 2f, 0.2f), new Color(0.7f, 0.85f, 1f), PrimitiveType.Cylinder);
            }

            // Frozen Pillars - ancient structures
            for (int i = 0; i < frozenPillarCount; i++)
            {
                float angle = i * (360f / frozenPillarCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 50f,
                    3f,
                    Mathf.Sin(angle) * 50f
                );
                CreateProp($"FrozenPillar_{i}", pos, new Vector3(1f, 6f, 1f), new Color(0.6f, 0.7f, 0.8f), PrimitiveType.Cylinder);
            }

            Debug.Log($"[Moon5EnvironmentDecorator] ✅ Frozen citadel decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
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
