using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Environment Decorator — The Verdant Labyrinth
    /// Spawns jungle foliage, vines, moss-covered ruins, and natural props
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon3EnvironmentDecorator : MonoBehaviour
    {
        [Header("Jungle Foliage")]
        [SerializeField] int fernCount = 40;
        [SerializeField] int vineCount = 25;
        [SerializeField] int mossRockCount = 30;
        [SerializeField] int ruinFragmentCount = 15;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon3EnvironmentDecorator] 🌿 Decorating jungle with {fernCount + vineCount + mossRockCount + ruinFragmentCount} props...");

            // Ferns - scattered ground cover
            for (int i = 0; i < fernCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    0.5f,
                    Random.Range(-100f, 100f)
                );
                CreateProp($"Fern_{i}", pos, new Vector3(0.5f, 1.2f, 0.5f), new Color(0.2f, 0.6f, 0.2f));
            }

            // Hanging Vines - near walls
            for (int i = 0; i < vineCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * Random.Range(70f, 95f),
                    Random.Range(5f, 12f),
                    Mathf.Sin(angle) * Random.Range(70f, 95f)
                );
                CreateProp($"Vine_{i}", pos, new Vector3(0.2f, 3f, 0.2f), new Color(0.3f, 0.5f, 0.2f));
            }

            // Moss-Covered Rocks
            for (int i = 0; i < mossRockCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    0.3f,
                    Random.Range(-90f, 90f)
                );
                float scale = Random.Range(0.8f, 2.5f);
                CreateProp($"MossRock_{i}", pos, Vector3.one * scale, new Color(0.4f, 0.5f, 0.3f));
            }

            // Ancient Ruin Fragments
            for (int i = 0; i < ruinFragmentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-80f, 80f),
                    Random.Range(0f, 2f),
                    Random.Range(-80f, 80f)
                );
                CreateProp($"RuinFragment_{i}", pos, new Vector3(1.5f, 2f, 0.3f), new Color(0.6f, 0.6f, 0.5f));
            }

            Debug.Log($"[Moon3EnvironmentDecorator] ✅ Jungle decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
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
