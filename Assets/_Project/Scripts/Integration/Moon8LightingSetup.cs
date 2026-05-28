using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Lighting Setup — The Celestial Spires
    /// Bright sky atmosphere with clouds and sunbeams
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon8LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon8LightingSetup] Setting up celestial atmosphere...");

            // Bright sky ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.7f, 0.8f, 1f); // Sky blue
            RenderSettings.ambientIntensity = 1.2f;

            // Bright sun
            var sun = new GameObject("Directional_Sun");
            sun.transform.SetParent(transform);
            var sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.98f, 0.9f); // Warm white
            sunLight.intensity = 2f;
            sunLight.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, 180f, 0f);

            // 6 Island temple lights
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(120f, 100f, 0f);
                var templeLight = new GameObject($"Temple_Light_{i}");
                templeLight.transform.SetParent(transform);
                templeLight.transform.position = offset;
                var light = templeLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.9f, 0.95f, 1f); // Cool white
                light.intensity = 2.5f;
                light.range = 40f;
            }

            // Altitude fog (clouds below)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.9f, 0.95f, 1f);
            RenderSettings.fogStartDistance = 50f;
            RenderSettings.fogEndDistance = 300f;

            Debug.Log("[Moon8LightingSetup] ✅ Sky atmosphere complete!");
        }
    }
}
