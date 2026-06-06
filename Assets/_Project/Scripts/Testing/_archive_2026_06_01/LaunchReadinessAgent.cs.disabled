using UnityEngine;

namespace Tartaria.Testing
{
    /// <summary>
    /// LaunchReadinessAgent - Agent 10: Beta launch readiness.
    /// </summary>
    public class LaunchReadinessAgent : MonoBehaviour
    {
        [SerializeField] private int readinessScore = 0;

        public void CalculateReadiness()
        {
            Debug.Log("[LaunchReadinessAgent] Calculating launch readiness...");
            readinessScore = 0;

            // Check all systems
            if (BugHunterAgent.FindFirstObjectByType<BugHunterAgent>() != null) readinessScore += 10;
            if (StressTester.FindFirstObjectByType<StressTester>() != null) readinessScore += 10;
            if (MemoryProfiler.Instance != null) readinessScore += 10;
            if (PerformanceProfiler.Instance != null) readinessScore += 10;
            if (TelemetrySystem.Instance != null) readinessScore += 10;
            if (CrashReporter.Instance != null) readinessScore += 10;
            if (SaveLoadSystem.Instance != null) readinessScore += 10;
            if (PlayerSpawner.Instance != null) readinessScore += 10;
            if (QuestSystem.Instance != null) readinessScore += 10;
            if (InventorySystem.Instance != null) readinessScore += 10;

            Debug.Log($"[LaunchReadinessAgent] ✅ Readiness Score: {readinessScore}/100");
        }

        public int GetReadinessScore() => readinessScore;
    }
}
