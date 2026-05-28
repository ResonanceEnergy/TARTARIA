using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Lighting Setup — The Sunscorched Oasis
    /// Harsh overhead sun, strong shadows, heat shimmer atmosphere
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon4LightingSetup : MonoBehaviour
    {
        [Header("Lighting Configuration")]
        [SerializeField] Color sunlightColor = new Color(1f, 0.95f, 0.8f); // Warm desert sun
        [SerializeField] float sunIntensity = 2.2f; // Intense
        [SerializeField] Color ambientColor = new Color(0.9f, 0.85f, 0.7f); // Hot ambient

        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 4 LIGHTING — Sunscorched Oasis Atmosphere");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Harsh overhead sun
            var sun = new GameObject("Directional_Sun");
            sun.transform.SetParent(transform);
            var sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = sunlightColor;
            sunLight.intensity = sunIntensity;
            sunLight.shadows = LightShadows.Hard; // Sharp desert shadows
            sun.transform.rotation = Quaternion.Euler(75f, 45f, 0f); // High overhead

            // Hot desert ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.8f; // Bright ambient
            
            // Heat haze fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.95f, 0.9f, 0.8f); // Yellowish haze
            RenderSettings.fogStartDistance = 50f;
            RenderSettings.fogEndDistance = 200f;

            // Oasis water reflection lights (3)
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(15f, 3f, 0f);
                
                var waterLight = new GameObject($"Oasis_Reflection_{i}");
                waterLight.transform.SetParent(transform);
                waterLight.transform.position = pos;
                
                var light = waterLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.5f, 0.7f, 1f); // Cool blue water
                light.intensity = 1.5f;
                light.range = 20f;
            }

            Debug.Log("[Moon4LightingSetup] ✅ Desert atmosphere complete!");
            Debug.Log("  • Harsh sun (2.2 intensity, 75° overhead)");
            Debug.Log("  • Heat haze fog (50-200m linear)");
            Debug.Log("  • 3 oasis water reflection lights");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
