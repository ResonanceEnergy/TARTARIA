using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// H.L1 NRE-hunt: surfaces NullReferenceExceptions that appear without stack trace
    /// in Play mode (the "no stack" symptom flagged by Sprint 11 post-Phase 0-5 audit).
    ///
    /// Subscribes to Application.logMessageReceivedThreaded on game start. When any
    /// log message contains "NullReferenceException" without a usable stack trace
    /// (Unity sometimes prints just the message when the throw happens in native
    /// callbacks or coroutine yields), this logger re-emits an ERROR with a synthetic
    /// stack trace captured at log time, plus the active scene + frame + active
    /// GameStateManager state so the source surface is visible.
    ///
    /// Wired via [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] so it runs before
    /// any NRE on Play start.
    ///
    /// NO-DEBT compliance: catch block logs ex.GetType().Name + Message.
    /// </summary>
    public static class NREDiagnosticLogger
    {
        static bool _installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (_installed) return;
            _installed = true;
            Application.logMessageReceivedThreaded += HandleLog;
            Debug.Log("[NREDiagnosticLogger] Installed — will surface stack-less NullReferenceExceptions.");
        }

        static void HandleLog(string condition, string stackTrace, LogType type)
        {
            // Only interested in exceptions; ignore our own re-emissions.
            if (type != LogType.Exception && type != LogType.Error) return;
            if (string.IsNullOrEmpty(condition)) return;
            if (!condition.Contains("NullReferenceException")) return;
            if (condition.Contains("[NREDiagnosticLogger]")) return; // re-entrance guard

            // If Unity already gave us a stack trace, the original log is sufficient.
            // Only re-emit when stack is empty/minimal — that's the "no stack trace"
            // symptom the brief flagged.
            bool hasUsefulStack = !string.IsNullOrWhiteSpace(stackTrace) && stackTrace.Length > 32;
            if (hasUsefulStack) return;

            // Build a synthetic context block. Capture caller stack here so the
            // log carries the line/method that's currently executing.
            try
            {
                string synthStack = new System.Diagnostics.StackTrace(true).ToString();
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                int frame = Time.frameCount;
                string state = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState.ToString() : "<no GSM>";

                Debug.LogError(
                    "[NREDiagnosticLogger] STACK-LESS NRE SURFACED " +
                    $"| scene={sceneName} frame={frame} state={state}\n" +
                    $"  originalCondition: {condition}\n" +
                    $"  syntheticStack:\n{synthStack}");
            }
            catch (System.Exception ex)
            {
                // NO-DEBT: log type+message.
                Debug.LogWarning($"[NREDiagnosticLogger] HandleLog failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}