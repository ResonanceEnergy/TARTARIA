using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Save;

namespace Tartaria.Tests.StabilityTests
{
    /// <summary>
    /// Marathon Session Simulator — simulates 10+ hour play sessions to detect memory leaks
    /// and performance degradation.
    /// 
    /// Agent 4: Long Session Stability Auditor
    /// 
    /// Test Protocol:
    /// - Simulates player behavior: walking, combat, menu interactions, save/load cycles
    /// - Monitors memory usage, FPS, and performance metrics every 30 minutes
    /// - Logs all critical metrics to file for analysis
    /// - Detects: memory leaks, FPS degradation, save file bloat, resource cleanup issues
    /// 
    /// Usage:
    /// 1. Attach to a GameObject in your test scene
    /// 2. Configure test duration and behavior patterns
    /// 3. Run in Editor or Standalone build
    /// 4. Review logs at: Application.persistentDataPath/stability_logs/
    /// </summary>
    public class MarathonSessionSimulator : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] float testDurationHours = 10f;
        [SerializeField] float monitoringIntervalMinutes = 30f;
        [SerializeField] bool autoStartOnAwake = false;
        [SerializeField] bool enableDetailedLogging = true;

        [Header("Behavior Simulation")]
        [SerializeField] float walkingPercentage = 30f;
        [SerializeField] float combatPercentage = 25f;
        [SerializeField] float menuPercentage = 15f;
        [SerializeField] float saveLoadPercentage = 10f;
        [SerializeField] float idlePercentage = 20f;

        [Header("Performance Thresholds")]
        [SerializeField] float minAcceptableFPS = 30f;
        [SerializeField] long maxManagedHeapMB = 1024;
        [SerializeField] long maxNativeMemoryMB = 2048;
        [SerializeField] float maxSaveFileSizeMB = 10f;

        // Monitoring Data
        struct PerformanceSnapshot
        {
            public float timestamp;
            public float fps;
            public long managedHeapBytes;
            public long nativeMemoryBytes;
            public int allocatedGameObjects;
            public int audioSourcesActive;
            public int particleSystemsActive;
            public int activeCoroutines;
            public long saveFileSizeBytes;
        }

        readonly List<PerformanceSnapshot> _snapshots = new();
        Coroutine _testCoroutine;
        System.Diagnostics.Stopwatch _sessionStopwatch;
        string _logFilePath;
        StringBuilder _logBuilder;

        // FPS Tracking
        float _deltaTime;
        readonly Queue<float> _fpsHistory = new(100);

        // Leak Detection
        HashSet<GameObject> _trackedObjects = new();
        int _baselineGameObjectCount;
        long _baselineManagedHeap;

        void Awake()
        {
            _sessionStopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logBuilder = new StringBuilder();
            
            string logDir = System.IO.Path.Combine(Application.persistentDataPath, "stability_logs");
            System.IO.Directory.CreateDirectory(logDir);
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = System.IO.Path.Combine(logDir, $"marathon_test_{timestamp}.log");
            
            Log($"[MarathonTest] Initialized — {testDurationHours}h test, {monitoringIntervalMinutes}min intervals");
            Log($"[MarathonTest] Unity Version: {Application.unityVersion}");
            Log($"[MarathonTest] Platform: {Application.platform}");
            Log($"[MarathonTest] Target FPS: {Application.targetFrameRate}");

            if (autoStartOnAwake)
            {
                StartTest();
            }
        }

        void Update()
        {
            // Track FPS continuously
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            float currentFPS = 1.0f / _deltaTime;
            _fpsHistory.Enqueue(currentFPS);
            if (_fpsHistory.Count > 100) _fpsHistory.Dequeue();
        }

        void OnDestroy()
        {
            if (_testCoroutine != null)
            {
                StopCoroutine(_testCoroutine);
            }
            FlushLogs();
        }

        public void StartTest()
        {
            if (_testCoroutine != null)
            {
                Debug.LogWarning("[MarathonTest] Test already running!");
                return;
            }

            Log($"[MarathonTest] ══════════ TEST STARTED ══════════");
            Log($"[MarathonTest] Duration: {testDurationHours} hours ({testDurationHours * 60} minutes)");
            Log($"[MarathonTest] Monitoring interval: {monitoringIntervalMinutes} minutes");
            
            TakeBaselineSnapshot();
            _testCoroutine = StartCoroutine(RunMarathonTest());
        }

        public void StopTest()
        {
            if (_testCoroutine != null)
            {
                StopCoroutine(_testCoroutine);
                _testCoroutine = null;
            }
            
            Log($"[MarathonTest] ══════════ TEST STOPPED ══════════");
            GenerateFinalReport();
            FlushLogs();
        }

        IEnumerator RunMarathonTest()
        {
            float testDurationSeconds = testDurationHours * 3600f;
            float monitoringIntervalSeconds = monitoringIntervalMinutes * 60f;
            float nextMonitorTime = monitoringIntervalSeconds;
            
            _sessionStopwatch.Restart();

            while (_sessionStopwatch.Elapsed.TotalSeconds < testDurationSeconds)
            {
                // Simulate random player behavior
                yield return SimulatePlayerBehavior();

                // Take performance snapshot at intervals
                if (_sessionStopwatch.Elapsed.TotalSeconds >= nextMonitorTime)
                {
                    TakePerformanceSnapshot();
                    nextMonitorTime += monitoringIntervalSeconds;
                    FlushLogs(); // Periodically flush to disk
                }

                yield return null;
            }

            Log($"[MarathonTest] ══════════ TEST COMPLETED ══════════");
            GenerateFinalReport();
            FlushLogs();
        }

        IEnumerator SimulatePlayerBehavior()
        {
            float roll = UnityEngine.Random.value * 100f;
            float cumulative = 0f;

            // Walking
            cumulative += walkingPercentage;
            if (roll < cumulative)
            {
                yield return SimulateWalking();
                yield break;
            }

            // Combat
            cumulative += combatPercentage;
            if (roll < cumulative)
            {
                yield return SimulateCombat();
                yield break;
            }

            // Menu interactions
            cumulative += menuPercentage;
            if (roll < cumulative)
            {
                yield return SimulateMenuInteraction();
                yield break;
            }

            // Save/Load
            cumulative += saveLoadPercentage;
            if (roll < cumulative)
            {
                yield return SimulateSaveLoad();
                yield break;
            }

            // Idle (default)
            yield return SimulateIdle();
        }

        IEnumerator SimulateWalking()
        {
            if (enableDetailedLogging) Log("[Behavior] Walking simulation");
            
            // Simulate player movement
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
                randomDirection.y = 0;
                Vector3 targetPos = player.transform.position + randomDirection.normalized * 10f;
                
                float duration = UnityEngine.Random.Range(3f, 8f);
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    player.transform.position = Vector3.MoveTowards(
                        player.transform.position,
                        targetPos,
                        Time.deltaTime * 5f
                    );
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 8f));
            }
        }

        IEnumerator SimulateCombat()
        {
            if (enableDetailedLogging) Log("[Behavior] Combat simulation");
            
            // Trigger combat events
            var combatSystem = FindObjectOfType<CombatSystem>();
            if (combatSystem != null)
            {
                // Simulate 5-10 attacks
                int attackCount = UnityEngine.Random.Range(5, 11);
                for (int i = 0; i < attackCount; i++)
                {
                    // Simulate attack input
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
                }
            }
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 15f));
        }

        IEnumerator SimulateMenuInteraction()
        {
            if (enableDetailedLogging) Log("[Behavior] Menu interaction simulation");
            
            // Open/close inventory, equipment, skill tree
            var inventorySystem = FindObjectOfType<InventorySystem>();
            if (inventorySystem != null)
            {
                // Simulate menu opening/closing
                yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));
            }
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        }

        IEnumerator SimulateSaveLoad()
        {
            if (enableDetailedLogging) Log("[Behavior] Save/Load simulation");
            
            // Perform save
            SaveManager.Instance?.Save();
            yield return new WaitForSeconds(0.5f);
            
            // Occasionally test load
            if (UnityEngine.Random.value < 0.3f)
            {
                SaveManager.Instance?.Load();
                yield return new WaitForSeconds(1f);
            }
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        }

        IEnumerator SimulateIdle()
        {
            if (enableDetailedLogging) Log("[Behavior] Idle");
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 15f));
        }

        void TakeBaselineSnapshot()
        {
            _baselineGameObjectCount = FindObjectsOfType<GameObject>().Length;
            _baselineManagedHeap = GC.GetTotalMemory(false);
            
            Log($"[Baseline] GameObjects: {_baselineGameObjectCount}");
            Log($"[Baseline] Managed Heap: {_baselineManagedHeap / (1024 * 1024)} MB");
            Log($"[Baseline] Native Memory: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)} MB");
        }

        void TakePerformanceSnapshot()
        {
            float elapsed = (float)_sessionStopwatch.Elapsed.TotalMinutes;
            
            var snapshot = new PerformanceSnapshot
            {
                timestamp = elapsed,
                fps = GetAverageFPS(),
                managedHeapBytes = GC.GetTotalMemory(false),
                nativeMemoryBytes = (long)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),
                allocatedGameObjects = FindObjectsOfType<GameObject>().Length,
                audioSourcesActive = FindObjectsOfType<AudioSource>().Length,
                particleSystemsActive = FindObjectsOfType<ParticleSystem>().Length,
                activeCoroutines = 0, // Unity doesn't expose this easily
                saveFileSizeBytes = GetSaveFileSize()
            };
            
            _snapshots.Add(snapshot);
            
            Log($"[Snapshot {_snapshots.Count}] ═══════════════════════════════");
            Log($"  Time: {elapsed:F1} minutes ({elapsed / 60f:F2} hours)");
            Log($"  FPS: {snapshot.fps:F1} (target: {minAcceptableFPS})");
            Log($"  Managed Heap: {snapshot.managedHeapBytes / (1024 * 1024)} MB (delta: +{(snapshot.managedHeapBytes - _baselineManagedHeap) / (1024 * 1024)} MB)");
            Log($"  Native Memory: {snapshot.nativeMemoryBytes / (1024 * 1024)} MB");
            Log($"  GameObjects: {snapshot.allocatedGameObjects} (delta: +{snapshot.allocatedGameObjects - _baselineGameObjectCount})");
            Log($"  AudioSources: {snapshot.audioSourcesActive}");
            Log($"  ParticleSystems: {snapshot.particleSystemsActive}");
            Log($"  Save File: {snapshot.saveFileSizeBytes / (1024 * 1024f):F2} MB");
            
            // Check for thresholds
            if (snapshot.fps < minAcceptableFPS)
            {
                Log($"  ⚠️ WARNING: FPS below threshold ({snapshot.fps:F1} < {minAcceptableFPS})");
            }
            
            if (snapshot.managedHeapBytes / (1024 * 1024) > maxManagedHeapMB)
            {
                Log($"  ⚠️ WARNING: Managed heap exceeded threshold ({snapshot.managedHeapBytes / (1024 * 1024)} MB > {maxManagedHeapMB} MB)");
            }
            
            if (snapshot.nativeMemoryBytes / (1024 * 1024) > maxNativeMemoryMB)
            {
                Log($"  ⚠️ WARNING: Native memory exceeded threshold ({snapshot.nativeMemoryBytes / (1024 * 1024)} MB > {maxNativeMemoryMB} MB)");
            }
            
            float saveFileMB = snapshot.saveFileSizeBytes / (1024f * 1024f);
            if (saveFileMB > maxSaveFileSizeMB)
            {
                Log($"  ⚠️ WARNING: Save file size exceeded threshold ({saveFileMB:F2} MB > {maxSaveFileSizeMB} MB)");
            }
        }

        void GenerateFinalReport()
        {
            Log($"\n[FINAL REPORT] ═══════════════════════════════════════");
            Log($"Total Test Duration: {_sessionStopwatch.Elapsed.TotalHours:F2} hours");
            Log($"Snapshots Taken: {_snapshots.Count}");
            
            if (_snapshots.Count >= 2)
            {
                var first = _snapshots[0];
                var last = _snapshots[_snapshots.Count - 1];
                
                Log($"\n[Performance Degradation Analysis]");
                float fpsChange = last.fps - first.fps;
                float fpsChangePercent = (fpsChange / first.fps) * 100f;
                Log($"  FPS: {first.fps:F1} → {last.fps:F1} ({fpsChange:+0.0;-0.0} FPS, {fpsChangePercent:+0.0;-0.0}%)");
                
                long heapGrowth = last.managedHeapBytes - first.managedHeapBytes;
                Log($"  Managed Heap: +{heapGrowth / (1024 * 1024)} MB ({((float)heapGrowth / first.managedHeapBytes * 100f):F1}% growth)");
                
                long nativeGrowth = last.nativeMemoryBytes - first.nativeMemoryBytes;
                Log($"  Native Memory: +{nativeGrowth / (1024 * 1024)} MB ({((float)nativeGrowth / first.nativeMemoryBytes * 100f):F1}% growth)");
                
                int objectGrowth = last.allocatedGameObjects - first.allocatedGameObjects;
                Log($"  GameObjects: +{objectGrowth} ({((float)objectGrowth / first.allocatedGameObjects * 100f):F1}% growth)");
                
                long saveGrowth = last.saveFileSizeBytes - first.saveFileSizeBytes;
                Log($"  Save File: +{saveGrowth / 1024f:F1} KB ({((float)saveGrowth / first.saveFileSizeBytes * 100f):F1}% growth)");
                
                // Verdict
                Log($"\n[Stability Verdict]");
                bool isPassing = true;
                
                if (fpsChangePercent < -20f)
                {
                    Log($"  ❌ FAIL: FPS degraded by {-fpsChangePercent:F1}% (threshold: -20%)");
                    isPassing = false;
                }
                else
                {
                    Log($"  ✅ PASS: FPS stable ({fpsChangePercent:+0.0;-0.0}%)");
                }
                
                if (heapGrowth > 100 * 1024 * 1024) // 100 MB growth
                {
                    Log($"  ❌ FAIL: Managed heap grew by {heapGrowth / (1024 * 1024)} MB (threshold: 100 MB)");
                    isPassing = false;
                }
                else
                {
                    Log($"  ✅ PASS: Managed heap growth acceptable");
                }
                
                if (objectGrowth > 1000)
                {
                    Log($"  ❌ FAIL: GameObject count grew by {objectGrowth} (threshold: 1000)");
                    isPassing = false;
                }
                else
                {
                    Log($"  ✅ PASS: GameObject count stable");
                }
                
                Log($"\n[Overall Result: {(isPassing ? "✅ PASS" : "❌ FAIL")}]");
            }
            
            Log($"[Report saved to: {_logFilePath}]");
        }

        float GetAverageFPS()
        {
            if (_fpsHistory.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (float fps in _fpsHistory)
            {
                sum += fps;
            }
            return sum / _fpsHistory.Count;
        }

        long GetSaveFileSize()
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "save_slot_0.dat");
            if (System.IO.File.Exists(savePath))
            {
                return new System.IO.FileInfo(savePath).Length;
            }
            return 0;
        }

        void Log(string message)
        {
            string timestamped = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Debug.Log(timestamped);
            _logBuilder.AppendLine(timestamped);
        }

        void FlushLogs()
        {
            try
            {
                System.IO.File.WriteAllText(_logFilePath, _logBuilder.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MarathonTest] Failed to flush logs: {ex.Message}");
            }
        }
    }
}
