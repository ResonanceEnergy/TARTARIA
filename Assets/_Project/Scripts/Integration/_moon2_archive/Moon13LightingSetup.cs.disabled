using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Lighting Setup — The Aether Convergence
    /// FINAL LEVEL — Radiant aether energy, all spectrum colors, epic scale
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon13LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  ✨ MOON 13 LIGHTING — The Aether Convergence ✨");
            Debug.Log("  FINAL LEVEL ATMOSPHERE");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Brilliant white ambient (aether radiance)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.8f, 0.85f, 0.9f);
            RenderSettings.ambientIntensity = 1f;

            // Aether Core light (massive central sphere)
            var coreLight = new GameObject("Aether_Core_Light");
            coreLight.transform.SetParent(transform);
            coreLight.transform.position = new Vector3(0f, 50f, 0f);
            var light = coreLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.95f, 1f, 1f); // Bright cyan-white
            light.intensity = 8f; // Massive intensity
            light.range = 180f;
            light.shadows = LightShadows.Soft;

            // 12 Tribute platform lights (one for each moon)
            Color[] moonColors = {
                new Color(1f, 0.9f, 0.7f),    // Moon 1: Warm golden
                new Color(0.5f, 0.7f, 1f),    // Moon 2: Blue cavern
                new Color(0.4f, 0.8f, 0.3f),  // Moon 3: Green jungle
                new Color(1f, 0.85f, 0.6f),   // Moon 4: Desert gold
                new Color(0.7f, 0.85f, 1f),   // Moon 5: Ice blue
                new Color(1f, 0.4f, 0.1f),    // Moon 6: Lava orange
                new Color(0.2f, 0.6f, 0.8f),  // Moon 7: Deep water
                new Color(0.9f, 0.95f, 1f),   // Moon 8: Sky white
                new Color(0.6f, 0.2f, 0.8f),  // Moon 9: Corruption purple
                new Color(0.7f, 0.8f, 0.9f),  // Moon 10: Time neutral
                new Color(1f, 1f, 1f),        // Moon 11: Prismatic white
                new Color(0.3f, 0.2f, 0.5f)   // Moon 12: Shadow dark
            };

            // Dodecahedron positions (12 vertices)
            float phi = 1.618033988749895f;
            float phi_inv = 1f / phi;
            Vector3[] positions = {
                new Vector3(1, 1, 1).normalized * 90f,
                new Vector3(1, 1, -1).normalized * 90f,
                new Vector3(1, -1, 1).normalized * 90f,
                new Vector3(1, -1, -1).normalized * 90f,
                new Vector3(-1, 1, 1).normalized * 90f,
                new Vector3(-1, 1, -1).normalized * 90f,
                new Vector3(-1, -1, 1).normalized * 90f,
                new Vector3(-1, -1, -1).normalized * 90f,
                new Vector3(0, phi_inv, phi).normalized * 90f,
                new Vector3(0, phi_inv, -phi).normalized * 90f,
                new Vector3(0, -phi_inv, phi).normalized * 90f,
                new Vector3(0, -phi_inv, -phi).normalized * 90f
            };

            for (int i = 0; i < 12; i++)
            {
                var platformLight = new GameObject($"Tribute_Moon{i + 1}_Light");
                platformLight.transform.SetParent(transform);
                platformLight.transform.position = positions[i] + new Vector3(0f, 30f, 0f);
                var pl = platformLight.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = moonColors[i];
                pl.intensity = 3.5f;
                pl.range = 35f;
                pl.shadows = LightShadows.Soft;
            }

            // 3 Pillar ring lights (inner/middle/outer)
            CreateRingLight(60f, new Color(0.8f, 0.9f, 1f), "Inner_Ring", 2f);
            CreateRingLight(100f, new Color(0.85f, 0.95f, 1f), "Middle_Ring", 2.5f);
            CreateRingLight(140f, new Color(0.9f, 1f, 1f), "Outer_Ring", 3f);

            // Final altar light (peak at 100m)
            var altarLight = new GameObject("Final_Altar_Light");
            altarLight.transform.SetParent(transform);
            altarLight.transform.position = new Vector3(0f, 100f, 0f);
            var al = altarLight.AddComponent<Light>();
            al.type = LightType.Point;
            al.color = new Color(1f, 1f, 0.95f); // Warm white
            al.intensity = 5f;
            al.range = 50f;
            al.shadows = LightShadows.Soft;

            // Aether fog (ethereal mist)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.85f, 0.9f, 0.95f);
            RenderSettings.fogStartDistance = 100f;
            RenderSettings.fogEndDistance = 400f;

            Debug.Log("[Moon13LightingSetup] ✅ Final level atmosphere complete!");
            Debug.Log("  • Aether Core (8 intensity, 180m range)");
            Debug.Log("  • 12 Tribute lights (all moon colors)");
            Debug.Log("  • 3 Pillar ring lights");
            Debug.Log("  • Final Altar at peak");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateRingLight(float radius, Color color, string ringName, float intensity)
        {
            var ringLight = new GameObject($"Ring_Light_{ringName}");
            ringLight.transform.SetParent(transform);
            ringLight.transform.position = new Vector3(0f, 15f, 0f);
            var light = ringLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = radius;
        }
    }
}
