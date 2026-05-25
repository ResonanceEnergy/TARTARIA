using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tartaria.Editor
{
    /// <summary>
    /// Accumulates per-phase build results and writes a structured report file.
    /// Used by OneClickBuild and AutoPlayBoot to track what succeeded/failed
    /// so the PowerShell launcher can parse results without scraping Unity logs.
    ///
    /// Report path: Logs/tartaria-build-report.txt
    /// </summary>
    public static class BuildReport
    {
        public enum PhaseStatus { OK, Skipped, Failed }

        public struct PhaseResult
        {
            public string Name;
            public PhaseStatus Status;
            public long ElapsedMs;
            public string Error;
        }

        static readonly List<PhaseResult> _phases = new();
        static readonly Stopwatch _totalTimer = new();
        static string _pipelineName;

        public static int PassCount { get; private set; }
        public static int FailCount { get; private set; }
        public static int SkipCount { get; private set; }
        public static bool HasFailures => FailCount > 0;
        public static bool IsRunning => _totalTimer.IsRunning;
        public static IReadOnlyList<PhaseResult> Phases => _phases;

        /// <summary>Call at the start of any pipeline run.</summary>
        public static void Begin(string pipelineName)
        {
            _phases.Clear();
            PassCount = 0;
            FailCount = 0;
            SkipCount = 0;
            _pipelineName = pipelineName;
            _totalTimer.Restart();
        }

        /// <summary>
        /// Run a named phase with automatic timing and error capture.
        /// Returns true if the phase succeeded.
        /// </summary>
        public static bool RunPhase(string name, Action action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                Debug.Log($"[Tartaria] >> {name}");
                action();
                sw.Stop();

                _phases.Add(new PhaseResult
                {
                    Name = name,
                    Status = PhaseStatus.OK,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Error = null
                });
                PassCount++;

                Debug.Log($"[Tartaria]    OK  {name} ({sw.ElapsedMilliseconds}ms)");
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();

                _phases.Add(new PhaseResult
                {
                    Name = name,
                    Status = PhaseStatus.Failed,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Error = ex.Message
                });
                FailCount++;

                Debug.LogError($"[Tartaria]  FAIL {name}: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        /// <summary>Record a phase that was intentionally skipped.</summary>
        public static void Skip(string name, string reason)
        {
            _phases.Add(new PhaseResult
            {
                Name = name,
                Status = PhaseStatus.Skipped,
                ElapsedMs = 0,
                Error = reason
            });
            SkipCount++;
            Debug.Log($"[Tartaria]  SKIP {name}: {reason}");
        }

        /// <summary>
        /// Finalize the report and write to Logs/tartaria-build-report.txt.
        /// Also logs the summary to the Unity console.
        /// </summary>
        public static void Finish()
        {
            _totalTimer.Stop();
            float totalSec = _totalTimer.ElapsedMilliseconds / 1000f;

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"TARTARIA BUILD REPORT — {_pipelineName}");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total: {totalSec:F1}s | Pass: {PassCount} | Fail: {FailCount} | Skip: {SkipCount}");
            sb.AppendLine("═══════════════════════════════════════════════════");

            foreach (var p in _phases)
            {
                string icon = p.Status switch
                {
                    PhaseStatus.OK => " OK ",
                    PhaseStatus.Failed => "FAIL",
                    PhaseStatus.Skipped => "SKIP",
                    _ => "????"
                };
                string time = p.ElapsedMs > 0 ? $" ({p.ElapsedMs}ms)" : "";
                string err = !string.IsNullOrEmpty(p.Error) ? $" — {p.Error}" : "";
                sb.AppendLine($"  [{icon}] {p.Name}{time}{err}");
            }

            sb.AppendLine("═══════════════════════════════════════════════════");

            if (FailCount == 0)
                sb.AppendLine($"RESULT: ALL {PassCount} PHASES PASSED in {totalSec:F1}s");
            else
                sb.AppendLine($"RESULT: {FailCount} FAILED / {PassCount} passed / {SkipCount} skipped");

            sb.AppendLine("═══════════════════════════════════════════════════");

            string report = sb.ToString();

            // Log to Unity console
            if (FailCount == 0)
                Debug.Log($"[Tartaria] {report}");
            else
                Debug.LogError($"[Tartaria] {report}");

            // Write to file
            try
            {
                string logDir = Path.Combine(Application.dataPath, "..", "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string path = Path.Combine(logDir, "tartaria-build-report.txt");
                File.WriteAllText(path, report, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Tartaria] Could not write build report file: {ex.Message}");
            }
        }
    }
}
