using UnityEngine;
using UnityEngine.Rendering;

namespace Tartaria.Core
{
    /// <summary>
    /// Frame budget monitor: captures FrameTiming every 60 frames,
    /// logs 1%-low and 0.1%-low frame times to console.
    /// 
    /// Self-bootstraps via [RuntimeInitializeOnLoadMethod].
    /// </summary>
    public class FrameBudgetMonitor : MonoBehaviour
    {
        const int SAMPLE_INTERVAL = 60;
        const int HISTORY_SIZE = 100;

        static FrameBudgetMonitor _instance;
        FrameTiming[] _timings = new FrameTiming[1];
        float[] _frameTimeHistory = new float[HISTORY_SIZE];
        int _frameCount;
        int _historyIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[FrameBudgetMonitor]");
            _instance = go.AddComponent<FrameBudgetMonitor>();
            DontDestroyOnLoad(go);
        }

        void Update()
        {
            _frameCount++;

            if (_frameCount % SAMPLE_INTERVAL == 0)
            {
                FrameTimingManager.CaptureFrameTimings();
                uint available = FrameTimingManager.GetLatestTimings(1, _timings);
                
                if (available > 0)
                {
                    float cpuFrameTime = (float)_timings[0].cpuFrameTime;
                    _frameTimeHistory[_historyIndex] = cpuFrameTime;
                    _historyIndex = (_historyIndex + 1) % HISTORY_SIZE;

                    // Calculate percentiles every SAMPLE_INTERVAL frames
                    if (_frameCount >= HISTORY_SIZE)
                    {
                        var sorted = new float[HISTORY_SIZE];
                        System.Array.Copy(_frameTimeHistory, sorted, HISTORY_SIZE);
                        System.Array.Sort(sorted);

                        int index1pct = Mathf.Max(0, HISTORY_SIZE - (HISTORY_SIZE / 100) - 1);
                        int index01pct = Mathf.Max(0, HISTORY_SIZE - (HISTORY_SIZE / 1000) - 1);

                        float low1pct = sorted[index1pct];
                        float low01pct = sorted[index01pct];

                        Debug.Log($"[FrameBudget] 1%-low: {low1pct:F2}ms, 0.1%-low: {low01pct:F2}ms");
                    }
                }
            }
        }
    }
}
