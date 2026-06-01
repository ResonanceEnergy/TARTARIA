using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Testing
{
    /// <summary>
    /// MemoryProfiler - Agent 6: Memory and optimization monitoring.
    /// </summary>
    public class MemoryProfiler : MonoBehaviour
    {
        public static MemoryProfiler Instance { get; private set; }

        [Header("Memory Metrics")]
        [SerializeField] private long totalMemory = 0;
        [SerializeField] private long usedMemory = 0;
        [SerializeField] private long peakMemory = 0;
        [SerializeField] private int gcCollections = 0;

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
            totalMemory = System.GC.GetTotalMemory(false);
            usedMemory = System.GC.GetTotalMemory(false);
            peakMemory = Mathf.Max(peakMemory, usedMemory);

            // Log memory warnings
            if (usedMemory > 1024 * 1024 * 1024) // 1 GB
            {
                Debug.LogWarning($"[MemoryProfiler] High memory usage: {usedMemory / (1024 * 1024)} MB");
            }
        }

        public void ForceGC()
        {
            System.GC.Collect();
            gcCollections++;
            Debug.Log($"[MemoryProfiler] GC forced. Collections: {gcCollections}");
        }

        public long GetUsedMemoryMB() => usedMemory / (1024 * 1024);
    }
}
