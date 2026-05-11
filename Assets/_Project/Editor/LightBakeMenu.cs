using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Opt-in lighting bake for Echohaven (Moon 1).
    ///
    /// Light baking is intentionally NOT in OneClickBuild because batchmode
    /// bakes can take minutes-to-hours. Run from the menu before a content
    /// freeze / vertical slice cut. APV scenarios must already be authored
    /// (Phase 9g handles that).
    ///
    /// Workflow:
    ///   1. TARTARIA → Lighting → Bake Echohaven (this menu)
    ///   2. Wait for Lightmapping.BakeAsync to finish (Console will say "Bake completed").
    ///   3. Run normal build pipeline.
    /// </summary>
    public static class LightBakeMenu
    {
        const string EchohavenScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";

        [MenuItem("TARTARIA/Lighting/Bake Echohaven (Async)")]
        public static void BakeEchohavenAsync()
        {
            EnsureSceneOpen();
            Debug.Log("[LightBake] Starting async lightmap bake for Echohaven. " +
                      "This may take several minutes. The Editor will remain responsive.");
            if (Lightmapping.isRunning)
            {
                Debug.LogWarning("[LightBake] A bake is already running.");
                return;
            }
            // Subscribe once so we get a clear completion line in the console.
            Lightmapping.bakeCompleted += OnBakeCompleted;
            if (!Lightmapping.BakeAsync())
            {
                Debug.LogWarning("[LightBake] Lightmapping.BakeAsync returned false. " +
                                 "Check that there is at least one baked or mixed light and Lighting Settings are valid.");
                Lightmapping.bakeCompleted -= OnBakeCompleted;
            }
        }

        [MenuItem("TARTARIA/Lighting/Cancel Bake")]
        public static void CancelBake()
        {
            if (Lightmapping.isRunning)
            {
                Lightmapping.Cancel();
                Debug.Log("[LightBake] Bake cancelled.");
            }
            else
            {
                Debug.Log("[LightBake] No bake is currently running.");
            }
        }

        static void OnBakeCompleted()
        {
            Lightmapping.bakeCompleted -= OnBakeCompleted;
            Debug.Log("[LightBake] Bake completed. Save the scene to persist lightmaps.");
            // Persist lightmap GI data to disk.
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        static void EnsureSceneOpen()
        {
            var active = SceneManager.GetActiveScene();
            if (active.path == EchohavenScenePath) return;
            if (!System.IO.File.Exists(EchohavenScenePath))
            {
                Debug.LogWarning($"[LightBake] Echohaven scene not found at {EchohavenScenePath}. " +
                                 "Run the build pipeline first to generate it.");
                return;
            }
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                EchohavenScenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
        }
    }
}
