using UnityEngine;
using System.IO;

namespace Tartaria.Core
{
    /// <summary>
    /// Crash reporter: hooks Application.logMessageReceivedThreaded,
    /// writes Exception + StackTrace to Logs/crash-{timestamp}.txt.
    /// 
    /// Self-bootstraps via [RuntimeInitializeOnLoadMethod].
    /// Note: Sentry SDK integration pending (requires package + subscription).
    /// Current implementation logs crashes to file (documented in KNOWN_PLACEHOLDERS.md).
    /// </summary>
    public class CrashReporter : MonoBehaviour
    {
        static CrashReporter _instance;
        string _logDir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[CrashReporter]");
            _instance = go.AddComponent<CrashReporter>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            _logDir = Path.Combine(Application.dataPath, "..", "Logs");
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);

            Application.logMessageReceivedThreaded += HandleLog;
            Debug.Log("[CrashReporter] Initialized. Crash logs will be written to: " + _logDir);
        }

        void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string filename = $"crash-{timestamp}.txt";
                string filepath = Path.Combine(_logDir, filename);

                try
                {
                    using (var writer = new StreamWriter(filepath, true))
                    {
                        writer.WriteLine($"[{type}] {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine(logString);
                        writer.WriteLine("Stack Trace:");
                        writer.WriteLine(stackTrace);
                        writer.WriteLine("---");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[CrashReporter] Failed to write crash log: {ex.Message}");
                }
            }
        }
    }
}
