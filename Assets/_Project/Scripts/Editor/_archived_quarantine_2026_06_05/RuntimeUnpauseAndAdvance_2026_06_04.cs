// RuntimeUnpauseAndAdvance_2026_06_04.cs
// Diagnostic fix harness for 2026-06-04 NATRIX report:
//   "can't see main character, camera doesn't move, character doesn't move"
//
// Runtime probe revealed:
//   - EditorApplication.isPaused = True (Error Pause toggle on + missing-script errors)
//   - Time.deltaTime = 0.0000 (paused, not stepping)
//   - GameStateManager.CurrentState = Boot (Direct-Play exploration transition never fired
//     because it was suppressed by the error-pause before AfterSceneLoad hooks ran)
//
// This menu:
//   1. Turns OFF Error Pause toggle (Console preferences)
//   2. Sets EditorApplication.isPaused = false (manually un-pauses)
//   3. Transitions GameStateManager to Exploration if still in Boot
//   4. Logs the final state so we can verify with the runtime probe

using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class RuntimeUnpauseAndAdvance_2026_06_04
    {
        [MenuItem("Tartaria/9 Debug/Runtime Unpause + Advance State 2026-06-04")]
        public static void Run()
        {
            // 1. Disable Console Error Pause toggle via reflection (no public API).
            try
            {
                var logEntriesType = System.Type.GetType("UnityEditor.LogEntries,UnityEditor");
                if (logEntriesType != null)
                {
                    var setUnityConsoleErrorPause = logEntriesType.GetMethod("SetUnityConsoleErrorPause",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (setUnityConsoleErrorPause != null)
                    {
                        setUnityConsoleErrorPause.Invoke(null, new object[] { false });
                        Debug.Log("[UnpauseFix] LogEntries.SetUnityConsoleErrorPause(false) invoked.");
                    }
                    else
                    {
                        Debug.LogWarning("[UnpauseFix] LogEntries.SetUnityConsoleErrorPause method not found via reflection.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[UnpauseFix] Reflection to disable Error Pause failed: " + e.Message);
            }

            // 2. Try the ConsoleWindow approach as well — toggles the actual UI flag.
            try
            {
                var consoleWindowType = System.Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
                if (consoleWindowType != null)
                {
                    // ConsoleFlags.ErrorPause is an internal enum value 4
                    var setConsoleFlag = consoleWindowType.GetMethod("SetConsoleErrorPause",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if (setConsoleFlag != null)
                    {
                        setConsoleFlag.Invoke(null, new object[] { false });
                        Debug.Log("[UnpauseFix] ConsoleWindow.SetConsoleErrorPause(false) invoked.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[UnpauseFix] ConsoleWindow reflection: " + e.Message);
            }

            // 3. Force EditorApplication.isPaused = false.
            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
                Debug.Log("[UnpauseFix] EditorApplication.isPaused set to false.");
            }
            else
            {
                Debug.Log("[UnpauseFix] EditorApplication.isPaused already false.");
            }

            // 3b. Restore Time.timeScale to 1.0 if a stuck hit-stop / pause UI left it low.
            if (Application.isPlaying && Time.timeScale < 0.99f)
            {
                Debug.Log("[UnpauseFix] Time.timeScale was " + Time.timeScale + " — restoring to 1.0 (likely stuck HitFeedback hit-stop or unreleased pause UI).");
                Time.timeScale = 1f;
            }

            // 4. Force GameState → Exploration if still in Boot.
            if (Application.isPlaying)
            {
                var gsmType = System.Type.GetType("Tartaria.Core.GameStateManager, Tartaria.Core");
                if (gsmType != null)
                {
                    var instProp = gsmType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var inst = instProp?.GetValue(null);
                    if (inst != null)
                    {
                        var stateProp = gsmType.GetProperty("CurrentState");
                        var stateNow = stateProp?.GetValue(inst)?.ToString() ?? "?";
                        Debug.Log("[UnpauseFix] Pre-transition GameState: " + stateNow);

                        if (stateNow == "Boot" || stateNow == "Loading")
                        {
                            var transitionTo = gsmType.GetMethod("TransitionTo");
                            var gameStateType = System.Type.GetType("Tartaria.Core.GameState, Tartaria.Core");
                            if (transitionTo != null && gameStateType != null)
                            {
                                var explorationValue = System.Enum.Parse(gameStateType, "Exploration");
                                transitionTo.Invoke(inst, new object[] { explorationValue });
                                Debug.Log("[UnpauseFix] Forced GameState → Exploration.");
                            }
                        }
                    }
                    else Debug.LogWarning("[UnpauseFix] GameStateManager.Instance is null. Cannot transition.");
                }
                else Debug.LogWarning("[UnpauseFix] GameStateManager type not found.");
            }
            else Debug.Log("[UnpauseFix] Not in Play mode — skipped GameState transition.");

            Debug.Log("[UnpauseFix] Done. Move L-stick / WASD now and watch player.");
        }
    }
}
