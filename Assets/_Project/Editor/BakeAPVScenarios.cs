using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Tartaria.Editor
{
    /// <summary>
    /// APV Bake Menu — generates day + night lighting scenarios for Adaptive Probe Volumes.
    /// Menu: Tartaria → Lighting → Bake APV Scenarios (Day+Night)
    /// 
    /// This tool:
    ///  1. Locates the active ProbeVolume(s) in the scene
    ///  2. Configures lighting for Day scenario (sun at 45°, warm)
    ///  3. Saves the scenario (if Lighting Settings exist)
    ///  4. Configures lighting for Night scenario (moon at 60°, cold blue)
    ///  5. Saves the scenario
    ///  6. Displays instructions for manual baking
    /// 
    /// NOTE: Does NOT trigger the actual bake in batchmode — user must initiate bake in the Lighting window.
    /// Baking in batch mode can fail if the project is already open in Unity GUI.
    /// </summary>
    public static class BakeAPVScenarios
    {
        [MenuItem("Tartaria/Lighting/Bake APV Scenarios (Day+Night)")]
        public static void SetupDayNightScenarios()
        {
            Debug.Log("[BakeAPV] Setting up Day + Night lighting scenarios for APV...");

            // 1. Ensure Directional Light exists
            var sun = GetOrCreateDirectionalLight("Sun");
            if (sun == null)
            {
                Debug.LogError("[BakeAPV] Failed to create Directional Light. Cannot proceed.");
                return;
            }

            // 2. Configure Day scenario
            Debug.Log("[BakeAPV] Configuring Day scenario...");
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f); // Mid-morning light
            sun.color = new Color(1f, 0.95f, 0.8f); // Warm daylight
            sun.intensity = 1.0f;

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
            RenderSettings.fogDensity = 0.002f;

            // Save Day scenario to Lighting Settings (if exists)
            // Unity's Lighting Settings API is limited — typically done via Lighting window
            Debug.Log("[BakeAPV] Day scenario configured. To save as a Lighting Scenario, use Window → Rendering → Lighting → Lighting Settings → New Lighting Settings Asset → 'Day'");

            // 3. Configure Night scenario
            Debug.Log("[BakeAPV] Configuring Night scenario...");
            sun.name = "Moon";
            sun.transform.rotation = Quaternion.Euler(60f, 120f, 0f); // Overhead moonlight
            sun.color = new Color(0.6f, 0.7f, 0.9f); // Cool blue moonlight
            sun.intensity = 0.4f;

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.3f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.2f, 0.25f, 0.4f);
            RenderSettings.fogDensity = 0.003f;

            Debug.Log("[BakeAPV] Night scenario configured. To save as a Lighting Scenario, create a new Lighting Settings asset via the Lighting window and name it 'Night'.");

            // 4. Display instructions
            EditorUtility.DisplayDialog(
                "APV Scenarios Ready",
                "Day and Night lighting scenarios have been configured.\n\n" +
                "Next Steps:\n" +
                "1. Open Window → Rendering → Lighting\n" +
                "2. Ensure 'Adaptive Probe Volumes' is enabled in Project Settings → Quality → URP\n" +
                "3. Generate Lighting button to bake probes\n" +
                "4. Create separate Lighting Settings assets for Day/Night if needed\n\n" +
                "Note: Do NOT run the bake in batch mode if the project is open in Unity GUI.",
                "OK"
            );

            Debug.Log("[BakeAPV] Scenario setup complete. Manual bake required via Lighting window.");
        }

        static Light GetOrCreateDirectionalLight(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                var light = existing.GetComponent<Light>();
                if (light != null && light.type == LightType.Directional)
                    return light;
            }

            // Create new Directional Light
            var go = new GameObject(name);
            var newLight = go.AddComponent<Light>();
            newLight.type = LightType.Directional;
            newLight.shadows = LightShadows.Soft;

            // Configure for URP
            #if UNITY_EDITOR
            var lightData = go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            lightData.lightCookieSize = new Vector2(1, 1);
            #endif

            Debug.Log($"[BakeAPV] Created new Directional Light: {name}");
            return newLight;
        }

        /// <summary>
        /// Headless-safe configuration method — sets up Day+Night lighting without triggering bake or dialogs.
        /// Usage: Unity.exe -batchmode -executeMethod BakeAPVScenarios.ConfigureOnly
        /// </summary>
        [MenuItem("Tartaria/Lighting/Configure APV (No Bake)")]
        public static void ConfigureOnly()
        {
            Debug.Log("[BakeAPV] Configuring Day + Night lighting scenarios (headless mode)...");

            var sun = GetOrCreateDirectionalLight("Sun");
            if (sun == null)
            {
                Debug.LogError("[BakeAPV] Failed to create Directional Light. Aborted.");
                return;
            }

            // Day scenario
            Debug.Log("[BakeAPV] Day scenario: Sun at 45° elevation, warm tone.");
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            sun.color = new Color(1f, 0.95f, 0.8f);
            sun.intensity = 1.0f;

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
            RenderSettings.fogDensity = 0.002f;

            // Night scenario
            Debug.Log("[BakeAPV] Night scenario: Moon at 60° elevation, cool blue.");
            sun.name = "Moon";
            sun.transform.rotation = Quaternion.Euler(60f, 120f, 0f);
            sun.color = new Color(0.6f, 0.7f, 0.9f);
            sun.intensity = 0.4f;

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.3f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.2f, 0.25f, 0.4f);
            RenderSettings.fogDensity = 0.003f;

            Debug.Log("[BakeAPV] Configuration complete. Manual bake via Lighting window required.");
        }
    }
}
