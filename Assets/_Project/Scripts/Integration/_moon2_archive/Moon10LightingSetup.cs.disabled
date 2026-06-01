using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Lighting Setup — The Temporal Rift
    /// Shifting time-layer colors, temporal distortion effects
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon10LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon10LightingSetup] Setting up temporal atmosphere...");

            // Neutral ambient (time neutral)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f); // Gray
            RenderSettings.ambientIntensity = 0.4f;

            // Time vortex light (white-blue)
            var vortexLight = new GameObject("Time_Vortex_Light");
            vortexLight.transform.SetParent(transform);
            vortexLight.transform.position = new Vector3(0f, 15f, 0f);
            var light = vortexLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.9f, 0.95f, 1f); // Bright white-blue
            light.intensity = 5f;
            light.range = 100f;
            light.shadows = LightShadows.Soft;

            // 3 Time layer lights (past/present/future)
            CreateTimeLayerLight(Vector3.zero, new Color(0.8f, 0.6f, 0.4f), "Past"); // Sepia
            CreateTimeLayerLight(new Vector3(0f, 5f, 0f), Color.white, "Present"); // White
            CreateTimeLayerLight(new Vector3(0f, 10f, 0f), new Color(0.5f, 0.7f, 1f), "Future"); // Blue

            // 8 Temporal anchor lights
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(55f, 8f, 0f);
                var anchorLight = new GameObject($"Anchor_Light_{i}");
                anchorLight.transform.SetParent(transform);
                anchorLight.transform.position = pos;
                var al = anchorLight.AddComponent<Light>();
                al.type = LightType.Point;
                al.color = new Color(0.7f, 0.8f, 0.9f);
                al.intensity = 2f;
                al.range = 25f;
            }

            // Temporal fog (shifting)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.6f, 0.6f, 0.7f);
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 150f;

            Debug.Log("[Moon10LightingSetup] ✅ Temporal atmosphere complete!");
        }

        void CreateTimeLayerLight(Vector3 offset, Color color, string layerName)
        {
            var layerLight = new GameObject($"TimeLayer_{layerName}_Light");
            layerLight.transform.SetParent(transform);
            layerLight.transform.position = offset;
            var light = layerLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 1.5f;
            light.range = 50f;
        }
    }
}
