using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Lighting Setup — The Frostbound Citadel
    /// Cold blue moonlight, ice reflections, aurora effects
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon5LightingSetup : MonoBehaviour
    {
        [Header("Lighting Configuration")]
        [SerializeField] Color moonlightColor = new Color(0.7f, 0.85f, 1f); // Cold blue
        [SerializeField] float moonIntensity = 1.5f;
        [SerializeField] Color ambientColor = new Color(0.4f, 0.5f, 0.7f); // Icy blue

        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 5 LIGHTING — Frostbound Citadel Atmosphere");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Cold moonlight
            var moon = new GameObject("Directional_Moonlight");
            moon.transform.SetParent(transform);
            var moonLight = moon.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = moonlightColor;
            moonLight.intensity = moonIntensity;
            moonLight.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(60f, 225f, 0f); // High angle

            // Icy ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.5f;

            // Frost fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.8f, 0.9f, 1f); // Light blue
            RenderSettings.fogDensity = 0.005f; // Light frost mist

            // Ice crystal lights (12 around perimeter)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(55f, 8f, 0f);

                var crystalLight = new GameObject($"Ice_Crystal_Light_{i}");
                crystalLight.transform.SetParent(transform);
                crystalLight.transform.position = pos;

                var light = crystalLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.6f, 0.8f, 1f); // Cyan ice glow
                light.intensity = 1.8f;
                light.range = 20f;
                light.shadows = LightShadows.Soft;
            }

            // Tower beacon lights (4)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 20f, 0f);

                var beacon = new GameObject($"Tower_Beacon_{i}");
                beacon.transform.SetParent(transform);
                beacon.transform.position = pos;

                var light = beacon.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = new Color(0.5f, 0.7f, 1f); // Cold beacon
                light.intensity = 3f;
                light.range = 50f;
                light.spotAngle = 45f;
                beacon.transform.LookAt(Vector3.zero);
            }

            Debug.Log("[Moon5LightingSetup] ✅ Frozen atmosphere complete!");
            Debug.Log("  • Cold moonlight (1.5 intensity, blue-tinted)");
            Debug.Log("  • Frost fog (0.005 density)");
            Debug.Log("  • 12 ice crystal lights + 4 tower beacons");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
