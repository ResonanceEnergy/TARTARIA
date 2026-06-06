using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 12 Environment Decorator — The Umbral Sanctum
    /// Spawns shadow obelisks, dark crystals, void portals, and umbral monuments
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon12EnvironmentDecorator : MonoBehaviour
    {
        [Header("Shadow Props")]
        [SerializeField] int shadowObeliskCount = 20;
        [SerializeField] int darkCrystalCount = 35;
        [SerializeField] int voidPortalCount = 8;
        [SerializeField] int umbralMonumentCount = 12;

        List<GameObject> spawnedProps = new List<GameObject>();

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log($"[Moon12EnvironmentDecorator] 🌑 Decorating umbral sanctum with {shadowObeliskCount + darkCrystalCount + voidPortalCount + umbralMonumentCount} props...");

            // Shadow Obelisks - ancient dark structures
            for (int i = 0; i < shadowObeliskCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    3f,
                    Random.Range(-100f, 100f)
                );
                CreateProp($"ShadowObelisk_{i}", pos, new Vector3(1.2f, 6f, 1.2f), new Color(0.08f, 0.08f, 0.12f), PrimitiveType.Cube);
            }

            // Dark Crystals - absorbing light
            for (int i = 0; i < darkCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-110f, 110f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-110f, 110f)
                );
                CreateProp($"DarkCrystal_{i}", pos, new Vector3(0.7f, Random.Range(2f, 4f), 0.7f), new Color(0.05f, 0.05f, 0.1f), PrimitiveType.Cube);
            }

            // Void Portals - gateways to darkness
            for (int i = 0; i < voidPortalCount; i++)
            {
                float angle = i * (360f / voidPortalCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 70f,
                    2.5f,
                    Mathf.Sin(angle) * 70f
                );
                CreateProp($"VoidPortal_{i}", pos, new Vector3(3f, 4f, 0.3f), new Color(0.03f, 0.03f, 0.08f), PrimitiveType.Cube);
            }

            // Umbral Monuments - towering darkness
            for (int i = 0; i < umbralMonumentCount; i++)
            {
                float angle = i * (360f / umbralMonumentCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 50f,
                    4f,
                    Mathf.Sin(angle) * 50f
                );
                CreateProp($"UmbralMonument_{i}", pos, new Vector3(2f, 8f, 2f), new Color(0.1f, 0.1f, 0.15f), PrimitiveType.Cylinder);
            }

            Debug.Log($"[Moon12EnvironmentDecorator] ✅ Umbral sanctum decorated with {spawnedProps.Count} environmental props");
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
