using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 10 Environment Decorator — The Temporal Rift
    /// Spawns time fragments, distortion fields, temporal anchors, and chronoclasts
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon10EnvironmentDecorator : MonoBehaviour
    {
        [Header("Temporal Props")]
        [SerializeField] int timeFragmentCount = 30;
        [SerializeField] int distortionFieldCount = 20;
        [SerializeField] int temporalAnchorCount = 12;
        [SerializeField] int chronoclastCount = 25;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon10EnvironmentDecorator] ⏳ Decorating temporal rift with {timeFragmentCount + distortionFieldCount + temporalAnchorCount + chronoclastCount} props...");

            // Time Fragments - suspended in air
            for (int i = 0; i < timeFragmentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(2f, 15f),
                    Random.Range(-100f, 100f)
                );
                CreateProp($"TimeFragment_{i}", pos, Vector3.one * Random.Range(0.5f, 1.2f), new Color(0.8f, 0.8f, 0.9f), PrimitiveType.Cube);
            }

            // Distortion Fields - warped space
            for (int i = 0; i < distortionFieldCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    Random.Range(1f, 10f),
                    Random.Range(-90f, 90f)
                );
                CreateProp($"DistortionField_{i}", pos, new Vector3(3f, 3f, 3f), new Color(0.7f, 0.75f, 0.85f, 0.3f), PrimitiveType.Sphere);
            }

            // Temporal Anchors - stabilizing points
            for (int i = 0; i < temporalAnchorCount; i++)
            {
                float angle = i * (360f / temporalAnchorCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 75f,
                    3f,
                    Mathf.Sin(angle) * 75f
                );
                CreateProp($"TemporalAnchor_{i}", pos, new Vector3(1.5f, 5f, 1.5f), new Color(0.6f, 0.65f, 0.75f), PrimitiveType.Cylinder);
            }

            // Chronoclasts - time crystals
            for (int i = 0; i < chronoclastCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-100f, 100f)
                );
                CreateProp($"Chronoclast_{i}", pos, new Vector3(0.7f, Random.Range(2f, 4f), 0.7f), new Color(0.85f, 0.85f, 0.95f), PrimitiveType.Cube);
            }

            Debug.Log($"[Moon10EnvironmentDecorator] ✅ Temporal rift decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
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
