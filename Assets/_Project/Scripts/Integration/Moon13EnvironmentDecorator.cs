using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414, CS0219 // Placeholder counts for planned features
    /// <summary>
    /// Moon 13 Environment Decorator — The Aether Convergence
    /// FINAL LEVEL - Spawns aether crystals, convergence pillars, tribute markers, and representation of all 12 moons
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class Moon13EnvironmentDecorator : MonoBehaviour
    {
        [Header("Convergence Props")]
        [SerializeField] int aetherCrystalCount = 30;
        [SerializeField] int convergencePillarCount = 12; // One per moon
        [SerializeField] int tributeMarkerCount = 12; // Golden markers
        [SerializeField] int celestialOrbCount = 20;

        List<GameObject> spawnedProps = new List<GameObject>();

        // Moon tribute colors
        Color[] moonColors = new Color[]
        {
            new Color(1f, 0.85f, 0.5f),    // Moon 1: Warm golden
            new Color(0.4f, 0.6f, 0.9f),   // Moon 2: Blue cavern
            new Color(0.3f, 0.6f, 0.3f),   // Moon 3: Green jungle
            new Color(0.9f, 0.8f, 0.5f),   // Moon 4: Desert gold
            new Color(0.7f, 0.85f, 1f),    // Moon 5: Ice blue
            new Color(1f, 0.4f, 0.1f),     // Moon 6: Lava orange
            new Color(0.3f, 0.6f, 0.8f),   // Moon 7: Deep water
            new Color(0.95f, 0.95f, 1f),   // Moon 8: Sky white
            new Color(0.6f, 0.3f, 0.8f),   // Moon 9: Corruption purple
            new Color(0.8f, 0.8f, 0.8f),   // Moon 10: Time neutral
            new Color(1f, 1f, 1f),         // Moon 11: Prismatic white
            new Color(0.1f, 0.1f, 0.15f)   // Moon 12: Shadow dark
        };

        void Start()
        {
            SpawnEnvironmentProps();
        }

        void SpawnEnvironmentProps()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log($"[Moon13EnvironmentDecorator] ✨ DECORATING FINAL LEVEL with {aetherCrystalCount + convergencePillarCount + tributeMarkerCount + celestialOrbCount} props");
            Debug.Log("    🌟 AETHER CONVERGENCE - TRIBUTES FROM ALL 12 MOONS 🌟");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Aether Crystals - brilliant cyan-white
            for (int i = 0; i < aetherCrystalCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    Random.Range(1f, 5f),
                    Random.Range(-100f, 100f)
                );
                CreateProp($"AetherCrystal_{i}", pos, new Vector3(0.8f, Random.Range(3f, 6f), 0.8f), new Color(0.9f, 1f, 1f), PrimitiveType.Cube);
            }

            // Convergence Pillars - one per moon, in a circle
            for (int i = 0; i < convergencePillarCount; i++)
            {
                float angle = i * (360f / convergencePillarCount) * Mathf.Deg2Rad;
                float phi = 1.618033988749895f; // Golden ratio
                float radius = 90f;
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    5f,
                    Mathf.Sin(angle) * radius
                );
                CreateProp($"ConvergencePillar_Moon{i + 1}", pos, new Vector3(2.5f, 10f, 2.5f), moonColors[i], PrimitiveType.Cylinder);
                Debug.Log($"  → Convergence Pillar for Moon {i + 1} erected");
            }

            // Tribute Markers - golden platforms at tribute sites
            for (int i = 0; i < tributeMarkerCount; i++)
            {
                float angle = i * (360f / tributeMarkerCount) * Mathf.Deg2Rad;
                float phi = 1.618033988749895f;
                float radius = 80f + 20f * Mathf.Sin(i * phi);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.5f,
                    Mathf.Sin(angle) * radius
                );
                CreateProp($"TributeMarker_Moon{i + 1}", pos, new Vector3(5f, 1f, 5f), new Color(1f, 0.85f, 0.3f), PrimitiveType.Cylinder);
            }

            // Celestial Orbs - floating above central convergence
            for (int i = 0; i < celestialOrbCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-30f, 30f),
                    Random.Range(10f, 30f),
                    Random.Range(-30f, 30f)
                );
                CreateProp($"CelestialOrb_{i}", pos, Vector3.one * Random.Range(1f, 2f), new Color(0.95f, 1f, 1f), PrimitiveType.Sphere);
            }

            Debug.Log($"[Moon13EnvironmentDecorator] ✅ FINAL LEVEL DECORATED with {spawnedProps.Count} convergence props!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
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
