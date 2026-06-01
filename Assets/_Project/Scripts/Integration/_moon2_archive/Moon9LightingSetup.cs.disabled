using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 Lighting Setup — The Blighted Wastes
    /// Sickly green-purple corruption aura, dark energy
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon9LightingSetup : MonoBehaviour
    {
        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon9LightingSetup] Setting up corrupted atmosphere...");

            // Sickly ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.2f, 0.4f); // Purple-gray
            RenderSettings.ambientIntensity = 0.3f;

            // Corruption nexus light (pulsing purple)
            var nexusLight = new GameObject("Corruption_Nexus_Light");
            nexusLight.transform.SetParent(transform);
            nexusLight.transform.position = new Vector3(0f, 10f, 0f);
            var light = nexusLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.2f, 0.8f); // Purple
            light.intensity = 4f;
            light.range = 80f;
            light.shadows = LightShadows.Soft;

            // 5 Twisted spire lights
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(50f, 10f, 0f);
                var spireLight = new GameObject($"Spire_Light_{i}");
                spireLight.transform.SetParent(transform);
                spireLight.transform.position = pos;
                var sl = spireLight.AddComponent<Light>();
                sl.type = LightType.Spot;
                sl.color = new Color(0.4f, 0.8f, 0.3f); // Sickly green
                sl.intensity = 2f;
                sl.range = 40f;
                sl.spotAngle = 60f;
                spireLight.transform.LookAt(Vector3.zero);
            }

            // Corruption fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.4f, 0.3f, 0.5f); // Dark purple
            RenderSettings.fogDensity = 0.012f;

            Debug.Log("[Moon9LightingSetup] ✅ Corrupted atmosphere complete!");
        }
    }
}
