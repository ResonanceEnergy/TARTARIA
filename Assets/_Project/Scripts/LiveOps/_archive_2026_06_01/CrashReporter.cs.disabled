using UnityEngine;
using Tartaria.Core;

namespace Tartaria.LiveOps
{
    /// <summary>
    /// CrashReporter - Crash detection and reporting.
    /// Agent 1 requirement from Phase 5.
    /// </summary>
    public class CrashReporter : MonoBehaviour
    {
        public static CrashReporter Instance { get; private set; }

        [Header("Crash Stats")]
        [SerializeField] private int crashCount = 0;
        [SerializeField] private int errorCount = 0;
        [SerializeField] private int warningCount = 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.logMessageReceived += HandleLog;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    errorCount++;
                    ReportCrash(logString, stackTrace);
                    break;
                case LogType.Warning:
                    warningCount++;
                    break;
            }
        }

        void ReportCrash(string message, string stackTrace)
        {
            crashCount++;
            Debug.LogError($"[CrashReporter] CRASH #{crashCount}: {message}\n{stackTrace}");
            
            // In production: send to cloud service (Backtrace, Sentry, Unity Cloud Diagnostics)
        }

        public int GetCrashCount() => crashCount;
        public float GetCrashRate() => crashCount / Mathf.Max(1f, Time.time / 60f); // crashes per minute
    }
}
