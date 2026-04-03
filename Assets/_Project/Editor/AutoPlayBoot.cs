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
        static int _retryCount;

        static AutoPlayBoot()
        {
            _retryCount = 0;
            // Use update loop instead of single delayCall — more reliable after long imports
            EditorApplication.update += CheckSentinelUpdate;
        }

        static void CheckSentinelUpdate()
        {
            // Only try during first 30 update ticks after domain reload
            _retryCount++;
            if (_retryCount > 30)
            {
                EditorApplication.update -= CheckSentinelUpdate;
                return;
            }

            // Wait until editor is fully ready (not compiling, not importing)
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string fullPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath), SentinelPath);

            if (!System.IO.File.Exists(fullPath))
            {
                EditorApplication.update -= CheckSentinelUpdate;
                return;
            }

            // Consume sentinel
            EditorApplication.update -= CheckSentinelUpdate;
            System.IO.File.Delete(fullPath);
            Debug.Log("[Tartaria] AutoPlay sentinel detected -- running full pipeline");

            try
            {
                RunBuildValidatePlay();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tartaria] AutoPlay pipeline failed: {ex}");
            }
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
