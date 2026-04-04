using UnityEngine;
using Unity.Profiling;

namespace Tartaria.Core
{
    /// <summary>
    /// Performance Guard -- runtime budget verification.
    ///
    /// Monitors frame times against the target 60fps (16.67ms) budget
    /// on minimum spec (GTX 1070 / 6-core CPU). Tracks per-system
    /// timing budgets and alerts when thresholds are exceeded.
    ///
    /// Budget allocation (16.67ms total):
    ///   AetherField tick:   2.0ms max
    ///   Combat ECS:         2.0ms max
    ///   AI systems:         1.5ms max
    ///   Corruption tick:    1.0ms max
    ///   Rendering:          8.0ms (GPU-bound, URP)
    ///   Overhead:           2.17ms
    /// </summary>
    [DisallowMultipleComponent]
    public class PerformanceGuard : MonoBehaviour
    {
        public static PerformanceGuard Instance { get; private set; }

        [Header("Budget Targets (ms)")]
        [SerializeField, Min(1f), Tooltip("Target frame time in ms (16.67 = 60fps)")] float targetFrameTimeMs = 16.67f;
        [SerializeField, Min(0.1f), Tooltip("Max ms budget for Aether field tick")] float aetherBudgetMs = 2.0f;
        [SerializeField, Min(0.1f), Tooltip("Max ms budget for combat ECS systems")] float combatBudgetMs = 2.0f;
        [SerializeField, Min(0.1f), Tooltip("Max ms budget for AI decision making")] float aiBudgetMs = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Max ms budget for corruption tick")] float corruptionBudgetMs = 1.0f;

        [Header("Monitoring")]
        [SerializeField, Min(10), Tooltip("Frames to average for performance stats")] int sampleWindowFrames = 120;
        [SerializeField, Min(1f), Tooltip("Seconds between budget-exceeded alerts")] float alertCooldownSeconds = 10f;

        // Frame time tracking
        float[] _frameTimes;
        int _frameIndex;
        int _sampleCount;
        float _alertCooldown;

        // Per-system profiler markers
        static readonly ProfilerMarker s_markerAether = new("Tartaria.AetherField");
        static readonly ProfilerMarker s_markerCombat = new("Tartaria.Combat");
        static readonly ProfilerMarker s_markerAI = new("Tartaria.AI");
        static readonly ProfilerMarker s_markerCorruption = new("Tartaria.Corruption");

        // Per-system last measured times (ms)
        float _lastAetherMs;
        float _lastCombatMs;
        float _lastAIMs;
        float _lastCorruptionMs;

        // Stats
        int _budgetExceededCount;
        float _worstFrameMs;

        public float AverageFrameTimeMs => CalculateAverage();
        public float WorstFrameMs => _worstFrameMs;
        public int BudgetExceededCount => _budgetExceededCount;
        public bool IsInBudget => AverageFrameTimeMs <= targetFrameTimeMs;

        /// <summary>
        /// Use to wrap system ticks: using (PerformanceGuard.Profile(SystemTag.Aether)) { ... }
        /// </summary>
        public static ProfilerMarker.AutoScope Profile(SystemTag tag)
        {
            return tag switch
            {
                SystemTag.Aether => s_markerAether.Auto(),
                SystemTag.Combat => s_markerCombat.Auto(),
                SystemTag.AI => s_markerAI.Auto(),
                SystemTag.Corruption => s_markerCorruption.Auto(),
                _ => s_markerAether.Auto()
            };
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            _frameTimes = new float[sampleWindowFrames];
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float rawMs = Time.unscaledDeltaTime * 1000f;
            // Clamp to 1 second — anything larger is an editor pause, not real gameplay
            float frameMs = Mathf.Min(rawMs, 1000f);

            // Record frame time
            _frameTimes[_frameIndex] = frameMs;
            _frameIndex = (_frameIndex + 1) % sampleWindowFrames;
            if (_sampleCount < sampleWindowFrames) _sampleCount++;

            // Track worst
            if (frameMs > _worstFrameMs) _worstFrameMs = frameMs;

            // Tick alert cooldown regardless of budget status
            if (_alertCooldown > 0f)
                _alertCooldown -= Time.unscaledDeltaTime;

            // Budget check
            if (frameMs > targetFrameTimeMs)
            {
                _budgetExceededCount++;

                if (_alertCooldown <= 0f)
                {
                    _alertCooldown = alertCooldownSeconds;
                    LogBudgetViolation(frameMs);
                }
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Record a system's measured time for budget comparison.
        /// Called by systems after they complete their tick.
        /// </summary>
        public void RecordSystemTime(SystemTag tag, float ms)
        {
            switch (tag)
            {
                case SystemTag.Aether: _lastAetherMs = ms; break;
                case SystemTag.Combat: _lastCombatMs = ms; break;
                case SystemTag.AI: _lastAIMs = ms; break;
                case SystemTag.Corruption: _lastCorruptionMs = ms; break;
            }
        }

        /// <summary>
        /// Get a performance summary string for the debug overlay.
        /// </summary>
        public string GetSummary()
        {
            float avg = CalculateAverage();
            float fps = avg > 0f ? 1000f / avg : 0f;
            return $"FPS: {fps:F0} | Avg: {avg:F1}ms | Worst: {_worstFrameMs:F1}ms\n" +
                   $"Aether: {_lastAetherMs:F2}/{aetherBudgetMs}ms | " +
                   $"Combat: {_lastCombatMs:F2}/{combatBudgetMs}ms\n" +
                   $"AI: {_lastAIMs:F2}/{aiBudgetMs}ms | " +
                   $"Corruption: {_lastCorruptionMs:F2}/{corruptionBudgetMs}ms\n" +
                   $"Budget violations: {_budgetExceededCount}";
        }

        /// <summary>
        /// Reset all tracking stats. Call on scene load or zone transition.
        /// </summary>
        public void ResetStats()
        {
            _sampleCount = 0;
            _frameIndex = 0;
            _worstFrameMs = 0f;
            _budgetExceededCount = 0;
            System.Array.Clear(_frameTimes, 0, _frameTimes.Length);
        }

        // ─── Internal ────────────────────────────────

        float CalculateAverage()
        {
            if (_sampleCount == 0) return 0f;
            float sum = 0f;
            for (int i = 0; i < _sampleCount; i++)
                sum += _frameTimes[i];
            return sum / _sampleCount;
        }

        void LogBudgetViolation(float frameMs)
        {
            string culprit = "unknown";
            float worstBudgetRatio = 0f;

            CheckBudget("Aether", _lastAetherMs, aetherBudgetMs, ref culprit, ref worstBudgetRatio);
            CheckBudget("Combat", _lastCombatMs, combatBudgetMs, ref culprit, ref worstBudgetRatio);
            CheckBudget("AI", _lastAIMs, aiBudgetMs, ref culprit, ref worstBudgetRatio);
            CheckBudget("Corruption", _lastCorruptionMs, corruptionBudgetMs, ref culprit, ref worstBudgetRatio);

            Debug.LogWarning(
                $"[PerfGuard] Frame budget exceeded: {frameMs:F1}ms " +
                $"(target {targetFrameTimeMs:F1}ms). Primary offender: {culprit}");
        }

        void CheckBudget(string name, float actual, float budget,
            ref string culprit, ref float worstRatio)
        {
            if (budget <= 0f) return;
            float ratio = actual / budget;
            if (ratio > worstRatio)
            {
                worstRatio = ratio;
                culprit = $"{name} ({actual:F2}/{budget}ms)";
            }
        }
    }

    public enum SystemTag : byte
    {
        Aether     = 0,
        Combat     = 1,
        AI         = 2,
        Corruption = 3
    }
}
