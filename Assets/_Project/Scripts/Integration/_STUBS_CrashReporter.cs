using UnityEngine;
using System;

namespace Tartaria.Integration
{
    /// <summary>
    /// STUB: Placeholder for CrashReporter until real implementation.
    /// TODO: Referenced by LiveOpsStabilityMonitor (also disabled).
    /// Replace with actual crash reporting integration (Sentry, Backtrace, etc).
    /// </summary>
    public static class CrashReporter
    {
        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            IsInitialized = true;
            Debug.Log("[CrashReporter STUB] Initialized (no-op)");
        }

        public static void ReportException(Exception exception, string context = null)
        {
            Debug.LogError($"[CrashReporter STUB] Exception: {exception.Message}\nContext: {context}");
        }

        public static void ReportError(string message, string stackTrace = null)
        {
            Debug.LogError($"[CrashReporter STUB] Error: {message}\nStack: {stackTrace}");
        }

        public static void SetUserIdentifier(string userId) { }
        public static void SetCustomData(string key, string value) { }
    }
}
