using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Auto-play trigger: when Unity opens and finds the sentinel file
    /// "Temp/TARTARIA_AUTOPLAY", it runs BUILD EVERYTHING -> Validate -> Enter Play Mode.
    ///
    /// The sentinel file is created by the PowerShell launcher script
    /// and deleted after consumption.
    ///
    /// Hardened: waits for editor readiness, per-phase error isolation,
    /// writes build report for PowerShell to parse, cleans up Device Simulator.
    ///
    /// Also available manually: Tartaria > Build + Validate + Play
    /// </summary>
    [InitializeOnLoad]
    public static class AutoPlayBoot
    {
        // Sentinel lives in Library/ — Temp/ gets wiped on project open!
        const string SentinelPath = "Library/TARTARIA_AUTOPLAY";

        // Retry budget: ~5 seconds of update ticks (editor usually ticks ~10/s during load)
        const int MaxRetries = 50;
        static int _playFrameDelay = -1;
        static int _retryCount;

        [InitializeOnLoadMethod]
        static void RegisterPlayModeKick()
        {
            EditorApplication.update += PlayModeKicker;
        }

        static void PlayModeKicker()
        {
            if (_playFrameDelay < 0) return;
            _playFrameDelay--;
            if (_playFrameDelay <= 0)
            {
                EditorApplication.update -= PlayModeKicker;
                Debug.Log("[Tartaria] PlayModeKicker firing -- isPlaying = true");
                OneClickBuild.CloseTMPImporterWindow();
                EditorApplication.isPlaying = true;
                Debug.Log($"[Tartaria] isPlaying is now: {EditorApplication.isPlaying}");
                // Focus Game View so keyboard input reaches the player
                EditorApplication.delayCall += () =>
                {
                    var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                    if (gameViewType != null)
                        EditorWindow.FocusWindowIfItsOpen(gameViewType);
                };
            }
        }

        static AutoPlayBoot()
        {
            _retryCount = 0;
            EditorApplication.update += WaitForEditorReady;
        }

        static void WaitForEditorReady()
        {
            _retryCount++;

            // Give up after budget exhausted
            if (_retryCount > MaxRetries)
            {
                EditorApplication.update -= WaitForEditorReady;
                return;
            }

            // Must not be compiling, importing, or still in a domain reload
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string fullPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath), SentinelPath);

            if (!System.IO.File.Exists(fullPath))
            {
                EditorApplication.update -= WaitForEditorReady;
                return;
            }

            // Found sentinel — consume it and run
            EditorApplication.update -= WaitForEditorReady;

            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Tartaria] Could not delete sentinel: {ex.Message}");
            }

            Debug.Log("[Tartaria] ============================================");
            Debug.Log("[Tartaria] AUTO-PLAY SENTINEL DETECTED");
            Debug.Log("[Tartaria] ============================================");

            // Clean up Device Simulator to avoid its internal NullRef
            CloseDeviceSimulator();

            // Run the full pipeline
            try
            {
                RunBuildValidatePlay();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Tartaria] AUTO-PLAY PIPELINE CRASHED: {ex}");
                // Still write a report so PowerShell can detect the failure
                BuildReport.Begin("AUTO-PLAY (crashed)");
                BuildReport.RunPhase("Pipeline", () => throw ex);
                BuildReport.Finish();
            }
        }

        /// <summary>
        /// Close any open Device Simulator windows to prevent
        /// Unity's internal NullReferenceException during serialization.
        /// </summary>
        static void CloseDeviceSimulator()
        {
            try
            {
                var simType = System.Type.GetType(
                    "UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor");
                if (simType == null) return;

                var windows = Resources.FindObjectsOfTypeAll(simType);
                foreach (var w in windows)
                {
                    if (w is EditorWindow ew)
                    {
                        Debug.Log("[Tartaria] Closing Device Simulator window (prevents NullRef)");
                        ew.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Tartaria] Could not close Device Simulator: {ex.Message}");
            }
        }

        [MenuItem("Tartaria/Build + Validate + Play", false, -90)]
        public static void RunBuildValidatePlay()
        {
            BuildReport.Begin("BUILD + VALIDATE + PLAY");

            // Phase 1: Build Everything
            Debug.Log("[Tartaria] === AUTO-PLAY: Phase 1/3 -- BUILD EVERYTHING ===");
            OneClickBuild.RunBuildPhases();

            // Phase 2: Validate (run even if build had partial failures)
            Debug.Log("[Tartaria] === AUTO-PLAY: Phase 2/3 -- READINESS CHECK ===");
            BuildReport.RunPhase("READINESS VALIDATION", () =>
            {
                BatchReadinessValidator.Validate();
            });

            // Phase 3: Open Boot scene and enter Play mode
            Debug.Log("[Tartaria] === AUTO-PLAY: Phase 3/3 -- ENTERING PLAY MODE ===");
            string bootPath = "Assets/_Project/Scenes/Boot.unity";

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath) != null)
            {
                BuildReport.RunPhase("Open Boot Scene", () =>
                {
                    EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
                    EditorSceneManager.SaveOpenScenes();
                });
            }
            else
            {
                BuildReport.Skip("Open Boot Scene", "Boot.unity not found after build");
                Debug.LogError("[Tartaria] Boot.unity was not created by BUILD EVERYTHING");
            }

            BuildReport.Finish();

            // Only enter Play if there were no critical failures
            if (!BuildReport.HasFailures)
            {
                Debug.Log("[Tartaria] Pipeline complete -- entering Play mode on next frame");

                // Close TMP importer window — it spams "Cannot import in play mode" errors
                OneClickBuild.CloseTMPImporterWindow();

                // Schedule Play mode via update kicker (delayCall doesn't fire reliably after pipeline)
                _playFrameDelay = 5; // wait 5 update ticks
                Debug.Log("[Tartaria] PlayModeKicker armed -- will fire in 5 ticks");
            }
            else
            {
                Debug.LogWarning(
                    $"[Tartaria] Pipeline had {BuildReport.FailCount} failure(s) -- NOT entering Play mode.\n" +
                    "[Tartaria] Fix errors above, then: Tartaria > Build + Validate + Play");
            }
        }
    }
}
