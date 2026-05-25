using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Tools
{
    /// <summary>
    /// AGENT 5: Performance Benchmark & Regression Suite
    /// 
    /// Automated performance benchmarking for all 13 Moon scenes.
    /// Tracks FPS baselines per scene and alerts on >10% degradation.
    /// 
    /// Baseline FPS Targets (GTX 1060 @ 1080p):
    /// - Moon 1-3:   60fps (tutorial, light combat)
    /// - Moon 4-6:   58fps (moderate enemies, VFX)
    /// - Moon 7-9:   57fps (dense environments, particles)
    /// - Moon 10-12: 56fps (boss fights, heavy VFX)
    /// - Moon 13:    55fps (final boss, maximum intensity)
    /// 
    /// Usage:
    /// 1. Run benchmark in each Moon scene: PerformanceBenchmark.Instance.RunBenchmark()
    /// 2. Baselines saved to: Logs/Benchmarks/Baselines/{SceneName}.json
    /// 3. Regression checks run automatically on subsequent tests
    /// 4. Alerts trigger if FPS drops >10% from baseline
    /// </summary>
    public class PerformanceBenchmark : MonoBehaviour
    {
        #region Singleton

        static PerformanceBenchmark _instance;
        public static PerformanceBenchmark Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PerformanceBenchmark");
                    _instance = go.AddComponent<PerformanceBenchmark>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Configuration

        [Header("Benchmark Configuration")]
        [SerializeField] float benchmarkDurationSeconds = 60f;
        [SerializeField] int warmupFrames = 120;
        [SerializeField] float regressionThresholdPercent = 10f;

        [Header("Scene Baselines")]
        [SerializeField] bool useCustomBaselines = false;
        
        // Default baselines (can be overridden)
        static readonly Dictionary<string, float> _defaultBaselines = new()
        {
            // Tutorial moons (light)
            { "Moon1", 60f },
            { "Moon2", 60f },
            { "Moon3", 60f },
            
            // Mid-game (moderate)
            { "Moon4", 58f },
            { "Moon5", 58f },
            { "Moon6", 58f },
            
            // Late-game (heavy)
            { "Moon7", 57f },
            { "Moon8", 57f },
            { "Moon9", 57f },
            
            // Endgame (boss fights)
            { "Moon10", 56f },
            { "Moon11", 56f },
            { "Moon12", 56f },
            { "Moon13", 55f }
        };

        #endregion

        #region State

        [Serializable]
        public class BenchmarkBaseline
        {
            public string sceneName;
            public float avgFPS;
            public float minFPS;
            public float p95FrameTimeMs;
            public long avgMemoryMB;
            public long peakMemoryMB;
            public string timestamp;
            public string unityVersion;
            public string platform;
        }

        bool _isRunning;
        List<float> _frameTimes = new();
        List<long> _memorySnapshots = new();
        int _warmupCount;
        float _testTimer;
        
        string _baselineDir;
        Dictionary<string, BenchmarkBaseline> _loadedBaselines = new();

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            _baselineDir = Path.Combine(Application.dataPath, "..", "Logs", "Benchmarks", "Baselines");
            Directory.CreateDirectory(_baselineDir);
            LoadAllBaselines();
        }

        void Update()
        {
            if (!_isRunning) return;

            // Warmup
            if (_warmupCount < warmupFrames)
            {
                _warmupCount++;
                return;
            }

            // Capture frame data
            _frameTimes.Add(Time.unscaledDeltaTime * 1000f);
            _memorySnapshots.Add(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());

            _testTimer += Time.unscaledDeltaTime;

            // Stop when duration reached
            if (_testTimer >= benchmarkDurationSeconds)
            {
                StopBenchmark();
            }
        }

        #endregion

        #region Benchmark Control

        public void RunBenchmark()
        {
            if (_isRunning)
            {
                Debug.LogWarning("[Benchmark] Already running");
                return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[Benchmark] Starting benchmark for {sceneName}...");

            _isRunning = true;
            _warmupCount = 0;
            _testTimer = 0f;
            _frameTimes.Clear();
            _memorySnapshots.Clear();
        }

        void StopBenchmark()
        {
            _isRunning = false;

            if (_frameTimes.Count == 0)
            {
                Debug.LogWarning("[Benchmark] No frames captured");
                return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[Benchmark] Benchmark complete for {sceneName} — {_frameTimes.Count} frames captured");

            // Calculate metrics
            var baseline = new BenchmarkBaseline
            {
                sceneName = sceneName,
                avgFPS = CalculateAverageFPS(),
                minFPS = CalculateMinFPS(),
                p95FrameTimeMs = CalculatePercentile(95),
                avgMemoryMB = CalculateAverageMemory(),
                peakMemoryMB = CalculatePeakMemory(),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString()
            };

            // Check for regression
            CheckRegression(baseline);

            // Save new baseline
            SaveBaseline(baseline);

            // Generate report
            GenerateReport(baseline);
        }

        #endregion

        #region Baseline Management

        void LoadAllBaselines()
        {
            _loadedBaselines.Clear();

            if (!Directory.Exists(_baselineDir))
            {
                Debug.LogWarning($"[Benchmark] Baseline directory not found: {_baselineDir}");
                return;
            }

            var files = Directory.GetFiles(_baselineDir, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var baseline = JsonUtility.FromJson<BenchmarkBaseline>(json);
                    _loadedBaselines[baseline.sceneName] = baseline;
                    Debug.Log($"[Benchmark] Loaded baseline: {baseline.sceneName} — {baseline.avgFPS:F1}fps");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Benchmark] Failed to load baseline {file}: {ex.Message}");
                }
            }

            Debug.Log($"[Benchmark] Loaded {_loadedBaselines.Count} baselines");
        }

        void SaveBaseline(BenchmarkBaseline baseline)
        {
            string filePath = Path.Combine(_baselineDir, $"{baseline.sceneName}.json");

            try
            {
                string json = JsonUtility.ToJson(baseline, prettyPrint: true);
                File.WriteAllText(filePath, json);
                _loadedBaselines[baseline.sceneName] = baseline;
                Debug.Log($"[Benchmark] Baseline saved: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Benchmark] Failed to save baseline: {ex.Message}");
            }
        }

        public BenchmarkBaseline GetBaseline(string sceneName)
        {
            if (_loadedBaselines.TryGetValue(sceneName, out var baseline))
                return baseline;
            
            // Use default baseline if no saved baseline exists
            if (_defaultBaselines.TryGetValue(sceneName, out float targetFPS))
            {
                return new BenchmarkBaseline
                {
                    sceneName = sceneName,
                    avgFPS = targetFPS,
                    minFPS = targetFPS * 0.9f,
                    p95FrameTimeMs = 1000f / (targetFPS * 0.95f),
                    avgMemoryMB = 512,
                    peakMemoryMB = 768,
                    timestamp = "Default",
                    unityVersion = Application.unityVersion,
                    platform = Application.platform.ToString()
                };
            }

            return null;
        }

        #endregion

        #region Regression Detection

        void CheckRegression(BenchmarkBaseline current)
        {
            var previousBaseline = GetBaseline(current.sceneName);
            
            if (previousBaseline == null || previousBaseline.timestamp == "Default")
            {
                Debug.Log($"[Benchmark] No previous baseline for {current.sceneName} — this will be the new baseline");
                return;
            }

            float fpsChange = ((current.avgFPS - previousBaseline.avgFPS) / previousBaseline.avgFPS) * 100f;
            long memoryChange = current.peakMemoryMB - previousBaseline.peakMemoryMB;

            Debug.Log($"[Benchmark] Regression Check for {current.sceneName}:");
            Debug.Log($"  FPS: {previousBaseline.avgFPS:F1} → {current.avgFPS:F1} ({fpsChange:+0.0}%)");
            Debug.Log($"  Memory: {previousBaseline.peakMemoryMB}MB → {current.peakMemoryMB}MB ({memoryChange:+0}MB)");

            // Alert on regression
            if (fpsChange < -regressionThresholdPercent)
            {
                Debug.LogError($"⚠️ [Benchmark] PERFORMANCE REGRESSION DETECTED!");
                Debug.LogError($"   FPS dropped {Mathf.Abs(fpsChange):F1}% — threshold: {regressionThresholdPercent}%");
                Debug.LogError($"   Previous: {previousBaseline.avgFPS:F1}fps → Current: {current.avgFPS:F1}fps");
            }
            else if (fpsChange > 10f)
            {
                Debug.Log($"✅ [Benchmark] PERFORMANCE IMPROVEMENT! FPS increased {fpsChange:F1}%");
            }
            else
            {
                Debug.Log($"✅ [Benchmark] Performance within acceptable range ({fpsChange:+0.0}%)");
            }

            if (memoryChange > 100)
            {
                Debug.LogWarning($"⚠️ [Benchmark] Memory increased significantly: +{memoryChange}MB");
            }
        }

        #endregion

        #region Metrics Calculation

        float CalculateAverageFPS()
        {
            float avgFrameTime = _frameTimes.Average();
            return 1000f / avgFrameTime;
        }

        float CalculateMinFPS()
        {
            float maxFrameTime = _frameTimes.Max();
            return 1000f / maxFrameTime;
        }

        float CalculatePercentile(int percentile)
        {
            var sorted = _frameTimes.OrderBy(t => t).ToList();
            int index = (int)(sorted.Count * percentile / 100f);
            return sorted[Mathf.Clamp(index, 0, sorted.Count - 1)];
        }

        long CalculateAverageMemory()
        {
            return (long)(_memorySnapshots.Average() / (1024 * 1024));  // Explicit cast
        }

        long CalculatePeakMemory()
        {
            return _memorySnapshots.Max() / (1024 * 1024);
        }

        #endregion

        #region Report Generation

        void GenerateReport(BenchmarkBaseline baseline)
        {
            var report = new StringBuilder();

            report.AppendLine($"# Performance Benchmark Report: {baseline.sceneName}");
            report.AppendLine($"**Date:** {baseline.timestamp}");
            report.AppendLine($"**Unity:** {baseline.unityVersion}");
            report.AppendLine($"**Platform:** {baseline.platform}");
            report.AppendLine();

            report.AppendLine("## Metrics");
            report.AppendLine("| Metric | Value | Target | Status |");
            report.AppendLine("|--------|-------|--------|--------|");

            float targetFPS = GetTargetFPS(baseline.sceneName);
            report.AppendLine($"| Average FPS | {baseline.avgFPS:F1} | {targetFPS} | {GetStatus(baseline.avgFPS, targetFPS)} |");
            report.AppendLine($"| Minimum FPS | {baseline.minFPS:F1} | {targetFPS * 0.9f:F1} | {GetStatus(baseline.minFPS, targetFPS * 0.9f)} |");
            report.AppendLine($"| P95 Frame Time | {baseline.p95FrameTimeMs:F2}ms | 16.67ms | {GetStatus(16.67f, baseline.p95FrameTimeMs)} |");
            report.AppendLine($"| Average Memory | {baseline.avgMemoryMB} MB | 768 MB | {GetStatus(768, baseline.avgMemoryMB)} |");
            report.AppendLine($"| Peak Memory | {baseline.peakMemoryMB} MB | 1024 MB | {GetStatus(1024, baseline.peakMemoryMB)} |");
            report.AppendLine();

            // Comparison with previous baseline
            var previousBaseline = GetBaseline(baseline.sceneName);
            if (previousBaseline != null && previousBaseline.timestamp != baseline.timestamp)
            {
                report.AppendLine("## Comparison with Previous Baseline");
                float fpsChange = ((baseline.avgFPS - previousBaseline.avgFPS) / previousBaseline.avgFPS) * 100f;
                long memoryChange = baseline.peakMemoryMB - previousBaseline.peakMemoryMB;

                report.AppendLine($"- **FPS Change:** {previousBaseline.avgFPS:F1} → {baseline.avgFPS:F1} ({fpsChange:+0.0}%)");
                report.AppendLine($"- **Memory Change:** {previousBaseline.peakMemoryMB}MB → {baseline.peakMemoryMB}MB ({memoryChange:+0}MB)");
                report.AppendLine($"- **Previous Baseline:** {previousBaseline.timestamp}");
                report.AppendLine();
            }

            // Verdict
            report.AppendLine("## Verdict");
            string verdict = baseline.avgFPS >= targetFPS ? "✅ PASS" : "❌ FAIL";
            report.AppendLine($"**{verdict}** — Baseline established for {baseline.sceneName}");
            report.AppendLine();

            // Save report
            string reportDir = Path.Combine(Application.dataPath, "..", "Logs", "Benchmarks", "Reports");
            Directory.CreateDirectory(reportDir);
            string reportPath = Path.Combine(reportDir, $"{baseline.sceneName}_{DateTime.Now:yyyyMMdd_HHmmss}.md");

            try
            {
                File.WriteAllText(reportPath, report.ToString());
                Debug.Log($"[Benchmark] Report saved: {reportPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Benchmark] Failed to save report: {ex.Message}");
            }
        }

        float GetTargetFPS(string sceneName)
        {
            return _defaultBaselines.TryGetValue(sceneName, out float target) ? target : 60f;
        }

        string GetStatus(float value, float target)
        {
            return value >= target ? "✅" : "❌";
        }

        #endregion

        #region Editor Utilities

        [ContextMenu("Run Benchmark (Current Scene)")]
        void RunBenchmarkFromMenu()
        {
            RunBenchmark();
        }

        [ContextMenu("Load All Baselines")]
        void LoadBaselinesFromMenu()
        {
            LoadAllBaselines();
        }

        [ContextMenu("Print Loaded Baselines")]
        void PrintBaselinesFromMenu()
        {
            Debug.Log($"[Benchmark] Loaded Baselines ({_loadedBaselines.Count}):");
            foreach (var kvp in _loadedBaselines)
            {
                var bl = kvp.Value;
                Debug.Log($"  {bl.sceneName}: {bl.avgFPS:F1}fps, {bl.peakMemoryMB}MB (saved {bl.timestamp})");
            }
        }

        #endregion
    }
}
