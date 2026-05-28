using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Lighting Setup — The Verdant Labyrinth
    /// Dappled sunlight through dense canopy, green-tinted atmosphere
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon3LightingSetup : MonoBehaviour
    {
        [Header("Lighting Configuration")]
        [SerializeField] Color sunlightColor = new Color(0.9f, 1f, 0.7f); // Greenish sunlight
        [SerializeField] float sunIntensity = 1.2f;
        [SerializeField] Color ambientColor = new Color(0.3f, 0.5f, 0.3f); // Dark green ambient

        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 3 LIGHTING — Verdant Labyrinth Atmosphere");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Main directional sunlight (filtered through canopy)
            var sun = new GameObject("Directional_Sun");
            sun.transform.SetParent(transform);
            var sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = sunlightColor;
            sunLight.intensity = sunIntensity;
            sunLight.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, 135f, 0f);

            // Ambient jungle lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.6f;
            
            // Fog for depth (dense jungle)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.4f, 0.6f, 0.4f);
            RenderSettings.fogDensity = 0.008f; // Thick jungle fog

            // Accent lights in clearings (8)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(40f, 12f, 0f);
                
                var clearingLight = new GameObject($"Clearing_Light_{i}");
                clearingLight.transform.SetParent(transform);
                clearingLight.transform.position = pos;
                
                var light = clearingLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.95f, 0.7f); // Warm sunbeam
                light.intensity = 2.5f;
                light.range = 25f;
                light.shadows = LightShadows.Soft;
            }

            Debug.Log("[Moon3LightingSetup] ✅ Jungle atmosphere complete!");
            Debug.Log("  • Filtered sunlight (greenish, 45° angle)");
            Debug.Log("  • Dense fog (0.008 density)");
            Debug.Log("  • 8 clearing accent lights");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
