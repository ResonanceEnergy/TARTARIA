using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Lighting Setup — The Abyssal Depths
    /// Dark underwater atmosphere with bioluminescent lights
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon7LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon7LightingSetup] Setting up abyssal atmosphere...");

            // Very dark ambient (deep water)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.1f, 0.2f, 0.3f); // Deep blue-black
            RenderSettings.ambientIntensity = 0.2f;

            // Dim surface light (filtered sunlight from above)
            var surfaceLight = new GameObject("Surface_Light");
            surfaceLight.transform.SetParent(transform);
            surfaceLight.transform.position = new Vector3(0f, 10f, 0f);
            var light = surfaceLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.3f, 0.5f, 0.7f);
            light.intensity = 0.4f;
            light.shadows = LightShadows.Soft;
            surfaceLight.transform.rotation = Quaternion.Euler(70f, 45f, 0f);

            // 30 Bioluminescent coral lights
            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-40f, 40f), Random.Range(-50f, 0f), Random.Range(-40f, 40f));
                var coralLight = new GameObject($"Coral_Light_{i}");
                coralLight.transform.SetParent(transform);
                coralLight.transform.position = pos;
                var coral = coralLight.AddComponent<Light>();
                coral.type = LightType.Point;
                coral.color = new Color(0.2f, 0.6f, 0.8f); // Cyan glow
                coral.intensity = Random.Range(0.8f, 1.5f);
                coral.range = Random.Range(8f, 15f);
            }

            // Underwater fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.1f, 0.2f, 0.35f);
            RenderSettings.fogDensity = 0.015f; // Dense water

            Debug.Log("[Moon7LightingSetup] ✅ Underwater atmosphere complete!");
        }
    }
}
