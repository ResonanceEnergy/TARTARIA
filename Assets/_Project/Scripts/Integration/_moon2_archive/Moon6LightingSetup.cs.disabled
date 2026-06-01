using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Lighting Setup — The Molten Forge
    /// Volcanic red-orange glow, lava light sources, intense heat shimmer
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon6LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon6LightingSetup] Setting up Molten Forge atmosphere...");

            // Dim red ambient (forge interior)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.6f, 0.2f, 0.1f); // Deep red
            RenderSettings.ambientIntensity = 0.4f;

            // Main forge light (overhead)
            var forgeLight = new GameObject("Forge_Main_Light");
            forgeLight.transform.SetParent(transform);
            forgeLight.transform.position = new Vector3(0f, 25f, 0f);
            var light = forgeLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.4f, 0.1f); // Orange-red
            light.intensity = 3.5f;
            light.range = 60f;
            light.shadows = LightShadows.Soft;

            // 8 Lava pool lights
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(35f, 1f, 0f);
                var lavaLight = new GameObject($"Lava_Light_{i}");
                lavaLight.transform.SetParent(transform);
                lavaLight.transform.position = pos;
                var lava = lavaLight.AddComponent<Light>();
                lava.type = LightType.Point;
                lava.color = new Color(1f, 0.3f, 0f); // Bright orange
                lava.intensity = 2.5f;
                lava.range = 20f;
            }

            // Heat haze fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.8f, 0.3f, 0.2f); // Red haze
            RenderSettings.fogDensity = 0.01f;

            Debug.Log("[Moon6LightingSetup] ✅ Volcanic atmosphere complete!");
        }
    }
}
