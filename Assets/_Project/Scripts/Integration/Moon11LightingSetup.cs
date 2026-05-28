using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Lighting Setup — The Prismatic Nexus
    /// Rainbow spectrum lighting with crystal refractions
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon11LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon11LightingSetup] Setting up prismatic atmosphere...");

            // Bright white ambient (crystal clarity)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.9f, 0.9f, 0.95f);
            RenderSettings.ambientIntensity = 0.7f;

            // Central prism light (bright white)
            var prismLight = new GameObject("Central_Prism_Light");
            prismLight.transform.SetParent(transform);
            prismLight.transform.position = new Vector3(0f, 20f, 0f);
            var light = prismLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.white;
            light.intensity = 6f;
            light.range = 90f;
            light.shadows = LightShadows.Soft;

            // 7 Color chamber lights (spectrum)
            Color[] colors = {
                new Color(1f, 0f, 0f),       // Red
                new Color(1f, 0.5f, 0f),     // Orange
                new Color(1f, 1f, 0f),       // Yellow
                new Color(0f, 1f, 0f),       // Green
                new Color(0f, 1f, 1f),       // Cyan
                new Color(0f, 0f, 1f),       // Blue
                new Color(0.5f, 0f, 1f)      // Violet
            };

            for (int i = 0; i < 7; i++)
            {
                float angle = i * 51.43f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(45f, 10f, 0f);
                var chamberLight = new GameObject($"Chamber_Light_{i}");
                chamberLight.transform.SetParent(transform);
                chamberLight.transform.position = pos;
                var cl = chamberLight.AddComponent<Light>();
                cl.type = LightType.Point;
                cl.color = colors[i];
                cl.intensity = 3f;
                cl.range = 30f;
                cl.shadows = LightShadows.Soft;
            }

            // 12 Refractor lights (white sparkles)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(30f, 15f, 0f);
                var refractorLight = new GameObject($"Refractor_Light_{i}");
                refractorLight.transform.SetParent(transform);
                refractorLight.transform.position = pos;
                var rl = refractorLight.AddComponent<Light>();
                rl.type = LightType.Point;
                rl.color = Color.white;
                rl.intensity = 1.5f;
                rl.range = 15f;
            }

            // Crystal-clear fog (minimal)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.95f, 0.95f, 1f);
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 200f;

            Debug.Log("[Moon11LightingSetup] ✅ Prismatic atmosphere complete!");
        }
    }
}
