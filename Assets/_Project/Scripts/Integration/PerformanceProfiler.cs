using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// PerformanceProfiler - FPS monitoring and performance tracking.
    /// Phase 3 requirement from REALITY_CHECK.
    /// </summary>
    public class PerformanceProfiler : MonoBehaviour
    {
        public static PerformanceProfiler Instance { get; private set; }

        [Header("Performance Metrics")]
        [SerializeField] private float currentFPS = 60f;
        [SerializeField] private float averageFPS = 60f;
        [SerializeField] private float minFPS = 60f;
        [SerializeField] private float maxFPS = 60f;
        [SerializeField] private int frameCount = 0;

        [Header("Targets")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private bool showDebugUI = false;

        private float _fpsAccumulator = 0f;
        private float _fpsUpdateInterval = 0.5f;
        private float _lastUpdateTime = 0f;

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
            frameCount++;
            float deltaTime = Time.unscaledDeltaTime;
            currentFPS = 1f / deltaTime;

            _fpsAccumulator += currentFPS;

            if (Time.time - _lastUpdateTime > _fpsUpdateInterval)
            {
                averageFPS = _fpsAccumulator / frameCount;
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);

                _fpsAccumulator = 0f;
                frameCount = 0;
                _lastUpdateTime = Time.time;

                // Log performance warnings
                if (averageFPS < targetFPS * 0.8f)
                {
                    Debug.LogWarning($"[PerformanceProfiler] FPS below target: {averageFPS:F1} / {targetFPS}");
                }
            }
        }

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUI.Box(new Rect(10, 10, 200, 100), "Performance");
            GUI.Label(new Rect(20, 35, 180, 20), $"FPS: {currentFPS:F1}");
            GUI.Label(new Rect(20, 55, 180, 20), $"Avg: {averageFPS:F1}");
            GUI.Label(new Rect(20, 75, 180, 20), $"Min: {minFPS:F1} | Max: {maxFPS:F1}");
        }

        public float GetAverageFPS() => averageFPS;
        public bool IsMeetingTarget() => averageFPS >= targetFPS * 0.9f;
    }
}
