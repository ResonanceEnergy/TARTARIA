using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 9 Environment Decorator — The Blighted Wastes
    /// Spawns corruption spires, dark crystals, twisted vegetation, and void rifts
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon9EnvironmentDecorator : MonoBehaviour
    {
        [Header("Corruption Props")]
        [SerializeField] int corruptionSpireCount = 25;
        [SerializeField] int darkCrystalCount = 35;
        [SerializeField] int twistedTreeCount = 20;
        [SerializeField] int voidRiftCount = 10;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon9EnvironmentDecorator] 💀 Decorating blighted wastes with {corruptionSpireCount + darkCrystalCount + twistedTreeCount + voidRiftCount} props...");

            // Corruption Spires - jutting from ground
            for (int i = 0; i < corruptionSpireCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(1f, 3f),
                    Random.Range(-100f, 100f)
                );
                float height = Random.Range(3f, 6f);
                CreateProp($"CorruptionSpire_{i}", pos, new Vector3(0.6f, height, 0.6f), new Color(0.3f, 0.15f, 0.4f), PrimitiveType.Cube);
            }

            // Dark Crystals - spreading corruption
            for (int i = 0; i < darkCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-110f, 110f)
                );
                CreateProp($"DarkCrystal_{i}", pos, new Vector3(0.8f, Random.Range(1.5f, 3f), 0.8f), new Color(0.4f, 0.2f, 0.5f), PrimitiveType.Cube);
            }

            // Twisted Trees - dead vegetation
            for (int i = 0; i < twistedTreeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-90f, 90f),
                    2f,
                    Random.Range(-90f, 90f)
                );
                CreateProp($"TwistedTree_{i}", pos, new Vector3(0.8f, 4f, 0.8f), new Color(0.25f, 0.2f, 0.3f), PrimitiveType.Cylinder);
            }

            // Void Rifts - portals to darkness
            for (int i = 0; i < voidRiftCount; i++)
            {
                float angle = i * (360f / voidRiftCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 80f,
                    1.5f,
                    Mathf.Sin(angle) * 80f
                );
                CreateProp($"VoidRift_{i}", pos, new Vector3(2f, 3f, 0.2f), new Color(0.1f, 0.05f, 0.2f), PrimitiveType.Cube);
            }

            Debug.Log($"[Moon9EnvironmentDecorator] ✅ Blighted wastes decorated with {spawnedProps.Count} environmental props");
        }

        void CreateProp(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale;
            prop.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f));
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
