using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Environment Decorator — The Prismatic Nexus
    /// Spawns prism shards, rainbow crystals, light beams, and refraction nodes
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon11EnvironmentDecorator : MonoBehaviour
    {
        [Header("Prismatic Props")]
        [SerializeField] int prismShardCount = 40;
        [SerializeField] int rainbowCrystalCount = 30;
        [SerializeField] int lightBeamCount = 20;
        [SerializeField] int refractionNodeCount = 15;

        List<GameObject> spawnedProps = new List<GameObject>();

        // Spectrum colors for variety
        Color[] spectrumColors = new Color[]
        {
            new Color(1f, 0f, 0f),      // Red
            new Color(1f, 0.5f, 0f),    // Orange
            new Color(1f, 1f, 0f),      // Yellow
            new Color(0f, 1f, 0f),      // Green
            new Color(0f, 1f, 1f),      // Cyan
            new Color(0f, 0f, 1f),      // Blue
            new Color(0.5f, 0f, 1f)     // Violet
        };

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon11EnvironmentDecorator] 🌈 Decorating prismatic nexus with {prismShardCount + rainbowCrystalCount + lightBeamCount + refractionNodeCount} props...");

            // Prism Shards - scattered spectrum fragments
            for (int i = 0; i < prismShardCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-100f, 100f)
                );
                Color color = spectrumColors[Random.Range(0, spectrumColors.Length)];
                CreateProp($"PrismShard_{i}", pos, new Vector3(0.5f, Random.Range(1.5f, 3f), 0.5f), color, PrimitiveType.Cube);
            }

            // Rainbow Crystals - multi-colored formations
            for (int i = 0; i < rainbowCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-110f, 110f)
                );
                Color color = spectrumColors[i % spectrumColors.Length];
                CreateProp($"RainbowCrystal_{i}", pos, Vector3.one * Random.Range(1f, 2.5f), color, PrimitiveType.Cube);
            }

            // Light Beams - rays of pure color
            for (int i = 0; i < lightBeamCount; i++)
            {
                float angle = i * (360f / lightBeamCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 60f,
                    Random.Range(5f, 15f),
                    Mathf.Sin(angle) * 60f
                );
                Color color = spectrumColors[i % spectrumColors.Length];
                CreateProp($"LightBeam_{i}", pos, new Vector3(0.3f, 10f, 0.3f), color, PrimitiveType.Cylinder);
            }

            // Refraction Nodes - focusing points
            for (int i = 0; i < refractionNodeCount; i++)
            {
                float angle = i * (360f / refractionNodeCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 75f,
                    3f,
                    Mathf.Sin(angle) * 75f
                );
                CreateProp($"RefractionNode_{i}", pos, Vector3.one * 2f, Color.white, PrimitiveType.Sphere);
            }

            Debug.Log($"[Moon11EnvironmentDecorator] ✅ Prismatic nexus decorated with {spawnedProps.Count} environmental props");
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
