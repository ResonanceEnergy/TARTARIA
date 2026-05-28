using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 8 Environment Decorator — The Celestial Spires
    /// Spawns cloud wisps, floating platforms, wind chimes, and sky crystals
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon8EnvironmentDecorator : MonoBehaviour
    {
        [Header("Sky Props")]
        [SerializeField] int cloudWispCount = 40;
        [SerializeField] int floatingPlatformCount = 20;
        [SerializeField] int windChimeCount = 15;
        [SerializeField] int skyCrystalCount = 25;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon8EnvironmentDecorator] ☁️ Decorating celestial spires with {cloudWispCount + floatingPlatformCount + windChimeCount + skyCrystalCount} props...");

            // Cloud Wisps - ethereal fog
            for (int i = 0; i < cloudWispCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(5f, 25f),
                    Random.Range(-100f, 100f)
                );
                CreateProp($"CloudWisp_{i}", pos, new Vector3(3f, 1f, 3f), new Color(0.95f, 0.95f, 1f, 0.5f), PrimitiveType.Sphere);
            }

            // Floating Platforms
            for (int i = 0; i < floatingPlatformCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(10f, 30f),
                    Random.Range(-90f, 90f)
                );
                CreateProp($"FloatingPlatform_{i}", pos, new Vector3(4f, 0.5f, 4f), new Color(0.9f, 0.9f, 0.95f), PrimitiveType.Cube);
            }

            // Wind Chimes - at spire tops
            for (int i = 0; i < windChimeCount; i++)
            {
                float angle = i * (360f / windChimeCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    25f,
                    Mathf.Sin(angle) * 70f
                );
                CreateProp($"WindChime_{i}", pos, new Vector3(0.3f, 2f, 0.3f), new Color(0.8f, 0.85f, 0.9f), PrimitiveType.Cylinder);
            }

            // Sky Crystals - floating
            for (int i = 0; i < skyCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    Random.Range(15f, 35f),
                    Random.Range(-80f, 80f)
                );
                CreateProp($"SkyCrystal_{i}", pos, new Vector3(0.6f, 1.5f, 0.6f), new Color(0.9f, 0.95f, 1f), PrimitiveType.Cube);
            }

            Debug.Log($"[Moon8EnvironmentDecorator] ✅ Celestial spires decorated with {spawnedProps.Count} environmental props");
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
