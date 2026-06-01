using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tartaria.Tests
{
    /// <summary>
    /// 60-second scripted profiler walkthrough for the Profiler_Baseline scene.
    /// Auto-bootstraps only when the active scene name is "Profiler_Baseline".
    /// On Start: spawns a moving probe (circle around origin, radius 20m, 4 m/s),
    /// samples per-frame Time.unscaledDeltaTime, plus batches/drawCalls/triangles
    /// once per second. At t=60s computes avg/p95/p99 frame time, logs a single
    /// JSON line to the console, writes the same JSON to
    /// Application.persistentDataPath/profiler_baseline_YYYYMMDD_HHmmss.json,
    /// and asserts avg &lt; 16.6 ms AND p95 &lt; 33 ms.
    /// </summary>
    public class PerformanceTest : MonoBehaviour
    {
        private const string TargetSceneName = "Profiler_Baseline";
        private const float WalkRadius = 20f;       // metres
        private const float WalkSpeed = 4f;         // m/s
        private const float Duration = 60f;         // seconds
        private const float AvgBudgetMs = 16.6f;    // 60 FPS budget
        private const float P95BudgetMs = 33f;      // worst-case spike budget

        private readonly List<float> _frameMs = new List<float>(8192);
        private int _batches;
        private int _drawCalls;
        private int _triangles;
        private bool _running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != TargetSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<PerformanceTest>() != null)
            {
                return;
            }

            var host = new GameObject("PerformanceTest_AutoBootstrap");
            host.AddComponent<PerformanceTest>();
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                // Defensive: if dropped into a non-baseline scene manually, do nothing.
                return;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            _running = true;
            StartCoroutine(RunWalkthrough());
        }

        private IEnumerator RunWalkthrough()
        {
            // Spawn probe (visible-ish so the scene isn't empty under camera)
            var probe = new GameObject("ProfilerProbe");
            probe.transform.position = new Vector3(WalkRadius, 1f, 0f);

            float omega = WalkSpeed / WalkRadius; // angular velocity (rad/s)
            float t = 0f;
            float nextStatSample = 1f;

            // Discard the very first frame (scene-load spike skews avg)
            yield return null;

            while (t < Duration)
            {
                float dt = Time.unscaledDeltaTime;
                _frameMs.Add(dt * 1000f);

                float angle = omega * t;
                probe.transform.position = new Vector3(
                    WalkRadius * Mathf.Cos(angle),
                    1f,
                    WalkRadius * Mathf.Sin(angle));

                if (t >= nextStatSample)
                {
                    SampleRenderStats();
                    nextStatSample += 1f;
                }

                t += dt;
                yield return null;
            }

            FinishAndReport(probe);
        }

        private void SampleRenderStats()
        {
#if UNITY_EDITOR
            _batches = UnityEditor.UnityStats.batches;
            _drawCalls = UnityEditor.UnityStats.drawCalls;
            _triangles = UnityEditor.UnityStats.triangles;
#else
            // UnityStats is editor-only; standalone runs report 0 (Cowork uses Editor menu).
            _batches = 0;
            _drawCalls = 0;
            _triangles = 0;
#endif
        }

        private void FinishAndReport(GameObject probe)
        {
            _running = false;

            if (_frameMs.Count == 0)
            {
                Debug.LogError("[PerformanceTest] No frame samples collected.");
                return;
            }

            // Final render-stats sample so the report reflects steady-state numbers
            SampleRenderStats();

            float avg = ComputeMean(_frameMs);
            float p95 = ComputePercentile(_frameMs, 0.95f);
            float p99 = ComputePercentile(_frameMs, 0.99f);

            string json = string.Format(
                CultureInfo.InvariantCulture,
                "{{\"avgMs\":{0:F3},\"p95Ms\":{1:F3},\"p99Ms\":{2:F3},\"batches\":{3},\"drawCalls\":{4},\"tris\":{5},\"samples\":{6}}}",
                avg, p95, p99, _batches, _drawCalls, _triangles, _frameMs.Count);

            Debug.Log("[PerformanceTest] " + json);

            WriteJsonReport(json);

            bool avgOk = avg < AvgBudgetMs;
            bool p95Ok = p95 < P95BudgetMs;
            if (!avgOk || !p95Ok)
            {
                Debug.LogError(string.Format(
                    CultureInfo.InvariantCulture,
                    "[PerformanceTest] FRAME BUDGET FAIL — avg={0:F2}ms (limit {1}ms, ok={2}); p95={3:F2}ms (limit {4}ms, ok={5}); p99={6:F2}ms",
                    avg, AvgBudgetMs, avgOk, p95, P95BudgetMs, p95Ok, p99));
            }
            else
            {
                Debug.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "[PerformanceTest] PASS — avg={0:F2}ms < {1}ms, p95={2:F2}ms < {3}ms",
                    avg, AvgBudgetMs, p95, P95BudgetMs));
            }

            if (probe != null)
            {
                Destroy(probe);
            }
        }

        private void WriteJsonReport(string json)
        {
            try
            {
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                string path = Path.Combine(Application.persistentDataPath, $"profiler_baseline_{stamp}.json");
                File.WriteAllText(path, json, new UTF8Encoding(false));
                Debug.Log("[PerformanceTest] Report written: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PerformanceTest] Failed to write JSON report: " + ex.Message);
            }
        }

        private static float ComputeMean(List<float> values)
        {
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }
            return (float)(sum / values.Count);
        }

        private static float ComputePercentile(List<float> values, float pct)
        {
            // Non-destructive: copy + sort
            var sorted = new List<float>(values);
            sorted.Sort();
            int idx = Mathf.Clamp(Mathf.CeilToInt(pct * sorted.Count) - 1, 0, sorted.Count - 1);
            return sorted[idx];
        }
    }
}
