using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-play trigger: when Unity opens and finds the sentinel file
    /// "Temp/TARTARIA_AUTOPLAY", it runs BUILD EVERYTHING → Validate → Enter Play Mode.
    ///
    /// The sentinel file is created by the PowerShell launcher script
    /// and deleted after consumption.
    ///
    /// Also available manually: Tartaria > Build + Validate + Play
    /// </summary>
    [InitializeOnLoad]
    public static class AutoPlayBoot
    {
        const string SentinelPath = "Temp/TARTARIA_AUTOPLAY";

        static AutoPlayBoot()
        {
            // Check on next editor update (after domain reload completes)
            EditorApplication.delayCall += CheckSentinel;
        }

        static void CheckSentinel()
        {
            string fullPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath), SentinelPath);

            if (!System.IO.File.Exists(fullPath)) return;

            // Consume sentinel
            System.IO.File.Delete(fullPath);
            Debug.Log("[Tartaria] AutoPlay sentinel detected -- running full pipeline");

            RunBuildValidatePlay();
        }

        [MenuItem("Tartaria/Build + Validate + Play", false, -90)]
        public static void RunBuildValidatePlay()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            // Phase 1: Build Everything
            Debug.Log("[Tartaria] ══ AUTO-PLAY: Phase 1/3 -- BUILD EVERYTHING ══");
            OneClickBuild.RunBuild();

            // Phase 2: Validate
            Debug.Log("[Tartaria] ══ AUTO-PLAY: Phase 2/3 -- READINESS CHECK ══");
            BatchReadinessValidator.Validate();

            // Phase 3: Open Boot scene and enter Play mode
            Debug.Log("[Tartaria] ══ AUTO-PLAY: Phase 3/3 -- ENTERING PLAY MODE ══");
            string bootPath = "Assets/_Project/Scenes/Boot.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath) != null)
            {
                EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
                EditorSceneManager.SaveOpenScenes();
            }

            timer.Stop();
            Debug.Log($"[Tartaria] Full pipeline completed in {timer.ElapsedMilliseconds / 1000f:F1}s -- entering Play mode");

            // Enter play mode on next frame (must not be in the middle of domain reload)
            EditorApplication.delayCall += () =>
            {
                EditorApplication.isPlaying = true;
            };
        }
    }
}
