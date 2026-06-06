using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 4 Environment Decorator — The Sunscorched Oasis
    /// Spawns cacti, tumbleweeds, desert rocks, and palm trees
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon4EnvironmentDecorator : MonoBehaviour
    {
        [Header("Desert Props")]
        [SerializeField] int cactusCount = 35;
        [SerializeField] int desertRockCount = 45;
        [SerializeField] int palmTreeCount = 12;
        [SerializeField] int tumbleweedCount = 20;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon4EnvironmentDecorator] 🌵 Decorating desert with {cactusCount + desertRockCount + palmTreeCount + tumbleweedCount} props...");

            // Cacti - scattered in dunes
            for (int i = 0; i < cactusCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    1f,
                    Random.Range(-100f, 100f)
                );
                float height = Random.Range(1.5f, 3.5f);
                CreateProp($"Cactus_{i}", pos, new Vector3(0.4f, height, 0.4f), new Color(0.4f, 0.6f, 0.3f));
            }

            // Desert Rocks
            for (int i = 0; i < desertRockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    0.4f,
                    Random.Range(-110f, 110f)
                );
                float scale = Random.Range(0.6f, 2.2f);
                CreateProp($"DesertRock_{i}", pos, Vector3.one * scale, new Color(0.8f, 0.7f, 0.5f));
            }

            // Palm Trees - near oasis
            for (int i = 0; i < palmTreeCount; i++)
            {
                float angle = i * (360f / palmTreeCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * Random.Range(15f, 30f),
                    2.5f,
                    Mathf.Sin(angle) * Random.Range(15f, 30f)
                );
                CreateProp($"PalmTree_{i}", pos, new Vector3(0.6f, 5f, 0.6f), new Color(0.5f, 0.4f, 0.2f));
            }

            // Tumbleweeds
            for (int i = 0; i < tumbleweedCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    0.3f,
                    Random.Range(-90f, 90f)
                );
                CreateProp($"Tumbleweed_{i}", pos, Vector3.one * 0.8f, new Color(0.7f, 0.6f, 0.4f));
            }

            Debug.Log($"[Moon4EnvironmentDecorator] ✅ Desert decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
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
