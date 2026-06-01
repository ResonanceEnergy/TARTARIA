using UnityEngine;
using Tartaria.Core;

namespace Tartaria.LiveOps
{
    /// <summary>
    /// MonitoringDashboard - Live ops dashboard.
    /// Agent 9 requirement from Phase 5.
    /// </summary>
    public class MonitoringDashboard : MonoBehaviour
    {
        public static MonitoringDashboard Instance { get; private set; }

        [Header("Dashboard Data")]
        [SerializeField] private bool showDashboard = false;
        [SerializeField] private string dashboardStatus = "GREEN";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                showDashboard = !showDashboard;
            }

            UpdateDashboardStatus();
        }

        void UpdateDashboardStatus()
        {
            var perf = PerformanceProfiler.Instance;
            var crash = CrashReporter.Instance;

            if (crash != null && crash.GetCrashCount() > 0)
                dashboardStatus = "RED";
            else if (perf != null && !perf.IsMeetingTarget())
                dashboardStatus = "YELLOW";
            else
                dashboardStatus = "GREEN";
        }

        void OnGUI()
        {
            if (!showDashboard) return;

            GUI.Box(new Rect(Screen.width - 310, 10, 300, 400), "LIVE OPS DASHBOARD");

            int y = 35;
            DrawLabel(ref y, $"Status: {dashboardStatus}");
            DrawLabel(ref y, "═══════════════════════");

            // Performance
            if (PerformanceProfiler.Instance != null)
            {
                DrawLabel(ref y, $"FPS: {PerformanceProfiler.Instance.GetAverageFPS():F1}");
            }

            // Crashes
            if (CrashReporter.Instance != null)
            {
                DrawLabel(ref y, $"Crashes: {CrashReporter.Instance.GetCrashCount()}");
                DrawLabel(ref y, $"Crash Rate: {CrashReporter.Instance.GetCrashRate():F3}/min");
            }

            // Memory
            if (MemoryProfiler.Instance != null)
            {
                DrawLabel(ref y, $"Memory: {MemoryProfiler.Instance.GetUsedMemoryMB()} MB");
            }

            // Telemetry
            if (TelemetrySystem.Instance != null)
            {
                DrawLabel(ref y, "═══════════════════════");
                DrawLabel(ref y, "Top Events:");
                var events = TelemetrySystem.Instance.GetAllEventCounts();
                int shown = 0;
                foreach (var kvp in events)
                {
                    if (shown++ >= 5) break;
                    DrawLabel(ref y, $"  {kvp.Key}: {kvp.Value}");
                }
            }

            DrawLabel(ref y, "═══════════════════════");
            DrawLabel(ref y, "Press F12 to toggle");
        }

        void DrawLabel(ref int y, string text)
        {
            GUI.Label(new Rect(Screen.width - 300, y, 280, 20), text);
            y += 20;
        }
    }
}
