#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Build Out Moon 1 Lighting Bake (Golden-Hour Preset)
    /// One-click lighting bake at the golden-hour preset per docs/15 §13.
    /// Configures Lightmapping settings + triggers async bake.
    /// </summary>
    public static class Moon1LightingBake
    {
        [MenuItem("Tartaria/1 Build/Moon 1 — Lighting Bake (Golden Hour)", priority = 180)]
        public static void Run()
        {
            // Golden-hour ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.95f, 0.78f, 0.50f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.65f, 0.45f, 0.30f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.15f, 0.10f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.85f, 0.65f, 0.45f, 1f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.008f;

            // Directional light → sun at low angle
            var sun = GameObject.Find("Directional Light");
            if (sun == null)
            {
                var go = new GameObject("Directional Light");
                sun = go;
                var lc = go.AddComponent<Light>();
                lc.type = LightType.Directional;
            }
            var l = sun.GetComponent<Light>();
            if (l != null)
            {
                l.intensity = 1.2f;
                l.color = new Color(1.0f, 0.85f, 0.65f, 1f);
                l.shadows = LightShadows.Soft;
            }
            sun.transform.rotation = Quaternion.Euler(28f, -25f, 0f); // Low golden-hour angle

            // Lightmap settings
            LightmapEditorSettings.bakeResolution = 20f;
            LightmapEditorSettings.padding = 4;
            LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveGPU;
            LightmapEditorSettings.maxAtlasSize = 1024;
            LightmapEditorSettings.directSampleCount = 32;
            LightmapEditorSettings.indirectSampleCount = 256;
            LightmapEditorSettings.bounces = 2;

            // Save scene first
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            if (!EditorUtility.DisplayDialog("Begin lighting bake?",
                "Golden-hour ambient + fog applied. Sun set to low angle.\n\n" +
                "Lightmap settings configured. Bake now? (May take 1-5 minutes.)",
                "Bake", "Skip bake"))
            {
                Debug.Log("[Moon1LightingBake] Settings applied, bake skipped.");
                return;
            }

            Lightmapping.bakedGI = true;
            Lightmapping.realtimeGI = false;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.BakeAsync();
            Debug.Log("[Moon1LightingBake] Bake started — check Lighting window for progress.");
            EditorUtility.DisplayDialog("Baking",
                "Lighting bake started. Watch progress in the Lighting window (Window → Rendering → Lighting).",
                "OK");
        }
    }
}
#endif
