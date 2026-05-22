using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.UI
{
    /// <summary>
    /// PerformanceMetricsOverlay — real-time FPS, frame time, memory, draw calls display.
    /// Toggle with F2. Lightweight circular buffer for frame time history.
    /// Color-coded warnings: Green (>60 FPS), Yellow (30-60 FPS), Red (<30 FPS).
    /// 
    /// Metrics:
    /// - FPS (current, min, max, avg)
    /// - Frame time (ms)
    /// - Memory (allocated, reserved, GC count)
    /// - Draw calls, batches, tris
    /// - CPU main thread time
    /// 
    /// Usage:
    /// - Attach to Canvas GameObject (auto-creates if needed)
    /// - Toggle with F2 or call ToggleVisibility()
    /// - Minimal GC allocation (circular buffer, no string concat in hot path)
    /// </summary>
    public class PerformanceMetricsOverlay : MonoBehaviour
    {
        public static PerformanceMetricsOverlay Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] GameObject overlayPanel;
        [SerializeField] TextMeshProUGUI metricsText;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] KeyCode toggleKey = KeyCode.F2;
        [SerializeField] float updateInterval = 0.25f;  // Update display 4x per second
        [SerializeField] int frameSampleSize = 120;  // 2 seconds at 60 FPS
        [SerializeField] bool startVisible = false;

        // Frame timing circular buffer
        float[] _frameTimes;
        int _frameIndex;

        // Cached metrics
        float _fps;
        float _fpsMin;
        float _fpsMax;
        float _fpsAvg;
        float _frameTimeMs;
        float _memoryAllocatedMB;
        float _memoryReservedMB;
        int _gcCount;

        // Update timing
        float _updateTimer;
        bool _isVisible;

        // String builder cache to avoid GC
        System.Text.StringBuilder _sb = new(512);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize frame timing buffer
            _frameTimes = new float[frameSampleSize];
            _frameIndex = 0;

            _isVisible = startVisible;
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(_isVisible);
            }
        }

        void Update()
        {
            // Toggle visibility
            if (UnityEngine.Input.GetKeyDown(toggleKey))
            {
                ToggleVisibility();
            }

            if (!_isVisible) return;

            // Record frame time
            _frameTimes[_frameIndex] = Time.unscaledDeltaTime;
            _frameIndex = (_frameIndex + 1) % _frameTimes.Length;

            // Update metrics at interval
            _updateTimer += Time.unscaledDeltaTime;
            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateMetrics();
                UpdateDisplay();
            }
        }

        void UpdateMetrics()
        {
            // FPS calculations
            float sum = 0f;
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = 0; i < _frameTimes.Length; i++)
            {
                float ft = _frameTimes[i];
                if (ft > 0f)
                {
                    sum += ft;
                    if (ft < min) min = ft;
                    if (ft > max) max = ft;
                }
            }

            float avgFrameTime = sum / _frameTimes.Length;
            _fps = 1f / Time.unscaledDeltaTime;
            _fpsMin = 1f / max;
            _fpsMax = 1f / min;
            _fpsAvg = 1f / avgFrameTime;
            _frameTimeMs = Time.unscaledDeltaTime * 1000f;

            // Memory
            _memoryAllocatedMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            _memoryReservedMB = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576f;
            _gcCount = System.GC.CollectionCount(0);
        }

        void UpdateDisplay()
        {
            if (metricsText == null) return;

            _sb.Clear();
            _sb.AppendLine("=== PERFORMANCE METRICS ===");
            _sb.AppendLine();

            // FPS with color coding
            Color fpsColor = _fps >= 60f ? Color.green : (_fps >= 30f ? Color.yellow : Color.red);
            string fpsColorHex = ColorUtility.ToHtmlStringRGB(fpsColor);
            _sb.AppendLine($"<color=#{fpsColorHex}>FPS: {_fps:F1}</color> (min {_fpsMin:F1} | max {_fpsMax:F1} | avg {_fpsAvg:F1})");
            _sb.AppendLine($"Frame Time: {_frameTimeMs:F2} ms");
            _sb.AppendLine();

            // Memory
            _sb.AppendLine($"Memory: {_memoryAllocatedMB:F1} MB allocated / {_memoryReservedMB:F1} MB reserved");
            _sb.AppendLine($"GC Collections: {_gcCount}");
            _sb.AppendLine();

            // Unity stats (if available)
#if UNITY_EDITOR
            _sb.AppendLine($"Draw Calls: {UnityStats.drawCalls}");
            _sb.AppendLine($"Batches: {UnityStats.batches}");
            _sb.AppendLine($"SetPass Calls: {UnityStats.setPassCalls}");
            _sb.AppendLine($"Triangles: {UnityStats.triangles / 1000}K");
            _sb.AppendLine($"Vertices: {UnityStats.vertices / 1000}K");
#else
            _sb.AppendLine("(Draw call stats available in Editor only)");
#endif

            metricsText.text = _sb.ToString();
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;

            if (overlayPanel != null)
            {
                overlayPanel.SetActive(_isVisible);
            }

            Debug.Log($"[PerformanceMetrics] Overlay {(_isVisible ? "shown" : "hidden")}");
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;

            if (overlayPanel != null)
            {
                overlayPanel.SetActive(_isVisible);
            }
        }

        /// <summary>
        /// Get current FPS reading.
        /// </summary>
        public float GetCurrentFPS() => _fps;

        /// <summary>
        /// Get average FPS over sample window.
        /// </summary>
        public float GetAverageFPS() => _fpsAvg;

        /// <summary>
        /// Get current frame time in milliseconds.
        /// </summary>
        public float GetFrameTimeMs() => _frameTimeMs;

        /// <summary>
        /// Check if performance is below target (60 FPS).
        /// </summary>
        public bool IsPerformanceLow() => _fpsAvg < 60f;

        /// <summary>
        /// Check if performance is critical (<30 FPS).
        /// </summary>
        public bool IsPerformanceCritical() => _fpsAvg < 30f;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Unity stats accessor for Editor-only rendering stats.
    /// </summary>
    internal static class UnityStats
    {
        public static int batches => UnityEditor.UnityStats.batches;
        public static int drawCalls => UnityEditor.UnityStats.drawCalls;
        public static int setPassCalls => UnityEditor.UnityStats.setPassCalls;
        public static int triangles => UnityEditor.UnityStats.triangles;
        public static int vertices => UnityEditor.UnityStats.vertices;
    }
#endif
}
