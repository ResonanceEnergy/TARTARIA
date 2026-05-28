using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Lighting Setup — The Umbral Sanctum
    /// Deep shadows with minimal void lights, darkness contrast
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon12LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon12LightingSetup] Setting up shadow realm atmosphere...");

            // Very dark ambient (void)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.1f); // Near-black
            RenderSettings.ambientIntensity = 0.15f;

            // Void core light (dim purple)
            var voidLight = new GameObject("Void_Core_Light");
            voidLight.transform.SetParent(transform);
            voidLight.transform.position = new Vector3(0f, 15f, 0f);
            var light = voidLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 0.2f, 0.5f); // Dark purple
            light.intensity = 2.5f;
            light.range = 70f;
            light.shadows = LightShadows.Hard; // Sharp shadow contrast

            // 6 Shadow spire lights (minimal blue glow)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 12f, 0f);
                var spireLight = new GameObject($"Spire_Light_{i}");
                spireLight.transform.SetParent(transform);
                spireLight.transform.position = pos;
                var sl = spireLight.AddComponent<Light>();
                sl.type = LightType.Point;
                sl.color = new Color(0.1f, 0.2f, 0.4f); // Dim blue
                sl.intensity = 1f;
                sl.range = 20f;
            }

            // 12 Obelisk void lights (barely visible)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(30f, 50f), 10f, 0f);
                var obeliskLight = new GameObject($"Obelisk_Light_{i}");
                obeliskLight.transform.SetParent(transform);
                obeliskLight.transform.position = pos;
                var ol = obeliskLight.AddComponent<Light>();
                ol.type = LightType.Point;
                ol.color = new Color(0.2f, 0.1f, 0.3f); // Purple-black
                ol.intensity = 0.6f;
                ol.range = 12f;
            }

            // Shadow fog (very dark)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f); // Near-black
            RenderSettings.fogDensity = 0.008f;

            Debug.Log("[Moon12LightingSetup] ✅ Shadow realm atmosphere complete!");
        }
    }
}
