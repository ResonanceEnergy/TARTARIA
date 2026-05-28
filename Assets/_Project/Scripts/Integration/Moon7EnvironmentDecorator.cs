using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Environment Decorator — The Abyssal Depths
    /// Spawns coral formations, seaweed, underwater rocks, and bioluminescent plants
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon7EnvironmentDecorator : MonoBehaviour
    {
        [Header("Underwater Props")]
        [SerializeField] int coralCount = 45;
        [SerializeField] int seaweedCount = 50;
        [SerializeField] int underwaterRockCount = 35;
        [SerializeField] int biolumPlantCount = 25;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon7EnvironmentDecorator] 🌊 Decorating abyssal depths with {coralCount + seaweedCount + underwaterRockCount + biolumPlantCount} props...");

            // Coral Formations - colorful clusters
            for (int i = 0; i < coralCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-100f, 100f)
                );
                Color coralColor = Random.value > 0.5f ? new Color(1f, 0.4f, 0.6f) : new Color(0.6f, 0.3f, 0.8f);
                CreateProp($"Coral_{i}", pos, new Vector3(0.8f, Random.Range(1f, 2.5f), 0.8f), coralColor, PrimitiveType.Cylinder);
            }

            // Seaweed - swaying strands
            for (int i = 0; i < seaweedCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    Random.Range(1f, 4f),
                    Random.Range(-110f, 110f)
                );
                CreateProp($"Seaweed_{i}", pos, new Vector3(0.2f, Random.Range(2f, 5f), 0.2f), new Color(0.2f, 0.5f, 0.3f), PrimitiveType.Cylinder);
            }

            // Underwater Rocks
            for (int i = 0; i < underwaterRockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(0.3f, 1.5f),
                    Random.Range(-100f, 100f)
                );
                float scale = Random.Range(1f, 3f);
                CreateProp($"UnderwaterRock_{i}", pos, Vector3.one * scale, new Color(0.3f, 0.4f, 0.5f), PrimitiveType.Sphere);
            }

            // Bioluminescent Plants - glowing
            for (int i = 0; i < biolumPlantCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-90f, 90f)
                );
                CreateProp($"BiolumPlant_{i}", pos, new Vector3(0.5f, 1.5f, 0.5f), new Color(0.3f, 0.8f, 0.9f), PrimitiveType.Sphere);
            }

            Debug.Log($"[Moon7EnvironmentDecorator] ✅ Abyssal depths decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
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
