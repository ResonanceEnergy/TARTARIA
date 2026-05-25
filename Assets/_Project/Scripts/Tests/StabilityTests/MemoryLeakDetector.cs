using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Tartaria.Tests.StabilityTests
{
    /// <summary>
    /// Memory Leak Detector — scans for common leak patterns in runtime.
    /// 
    /// Agent 4: Long Session Stability Auditor
    /// 
    /// Detects:
    /// - Event subscriptions not cleaned up in OnDestroy
    /// - Static collections growing unbounded
    /// - Coroutines not stopped on scene unload
    /// - AudioClips/Textures/Materials not released
    /// - Particle systems not destroyed
    /// - Timers/callbacks not cancelled
    /// 
    /// Usage:
    /// 1. Attach to a persistent GameObject (DontDestroyOnLoad)
    /// 2. Call ScanForLeaks() periodically or on demand
    /// 3. Review detailed leak report in console and log file
    /// </summary>
    public class MemoryLeakDetector : MonoBehaviour
    {
        [Header("Scan Configuration")]
        [SerializeField] bool scanOnStart = true;
        [SerializeField] bool periodicScanning = true;
        [SerializeField] float scanIntervalMinutes = 5f;
        [SerializeField] bool logToFile = true;

        [Header("Detection Thresholds")]
        [SerializeField] int maxStaticCollectionSize = 1000;
        [SerializeField] int maxEventSubscribers = 50;
        [SerializeField] int maxCoroutinesPerComponent = 10;

        StringBuilder _reportBuilder;
        float _nextScanTime;
        string _logFilePath;

        // Static collection tracking
        Dictionary<string, int> _staticCollectionSizes = new();
        Dictionary<string, int> _staticCollectionGrowth = new();

        void Awake()
        {
            string logDir = System.IO.Path.Combine(Application.persistentDataPath, "stability_logs");
            System.IO.Directory.CreateDirectory(logDir);
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = System.IO.Path.Combine(logDir, $"memory_leak_report_{timestamp}.log");
            
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (scanOnStart)
            {
                ScanForLeaks();
            }
            
            if (periodicScanning)
            {
                _nextScanTime = Time.time + (scanIntervalMinutes * 60f);
            }
        }

        void Update()
        {
            if (periodicScanning && Time.time >= _nextScanTime)
            {
                ScanForLeaks();
                _nextScanTime = Time.time + (scanIntervalMinutes * 60f);
            }
        }

        [ContextMenu("Scan For Memory Leaks")]
        public void ScanForLeaks()
        {
            _reportBuilder = new StringBuilder();
            
            Log("═══════════════════════════════════════════════════════");
            Log($"[Memory Leak Scan] Started at {DateTime.Now:HH:mm:ss}");
            Log("═══════════════════════════════════════════════════════\n");

            // Scan categories
            ScanEventSubscriptions();
            ScanStaticCollections();
            ScanCoroutineLeaks();
            ScanResourceLeaks();
            ScanUndestroyedObjects();
            ScanTimerCallbacks();

            Log("\n═══════════════════════════════════════════════════════");
            Log("[Memory Leak Scan] Completed");
            Log("═══════════════════════════════════════════════════════");

            if (logToFile)
            {
                try
                {
                    System.IO.File.AppendAllText(_logFilePath, _reportBuilder.ToString() + "\n\n");
                    Debug.Log($"[MemoryLeakDetector] Report saved to: {_logFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MemoryLeakDetector] Failed to save report: {ex.Message}");
                }
            }
        }

        void ScanEventSubscriptions()
        {
            Log("[1] EVENT SUBSCRIPTION LEAK SCAN");
            Log("───────────────────────────────────────────────────────");
            
            int leakCount = 0;
            var monoBehaviours = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var mb in monoBehaviours)
            {
                if (mb == null) continue;
                
                var type = mb.GetType();
                var events = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                
                foreach (var eventInfo in events)
                {
                    try
                    {
                        var field = type.GetField(eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        if (field == null) continue;
                        
                        var eventDelegate = field.GetValue(mb) as Delegate;
                        if (eventDelegate != null)
                        {
                            int subscriberCount = eventDelegate.GetInvocationList().Length;
                            
                            if (subscriberCount > maxEventSubscribers)
                            {
                                Log($"  ⚠️ LEAK: {type.Name}.{eventInfo.Name} has {subscriberCount} subscribers (threshold: {maxEventSubscribers})");
                                Log($"      Object: {mb.gameObject.name}");
                                leakCount++;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Some events can't be inspected, skip silently
                    }
                }
            }
            
            // Check static events
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("Tartaria")) continue;
                
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var staticEvents = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        
                        foreach (var eventInfo in staticEvents)
                        {
                            try
                            {
                                var field = type.GetField(eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Static);
                                if (field == null) continue;
                                
                                var eventDelegate = field.GetValue(null) as Delegate;
                                if (eventDelegate != null)
                                {
                                    int subscriberCount = eventDelegate.GetInvocationList().Length;
                                    
                                    if (subscriberCount > maxEventSubscribers)
                                    {
                                        Log($"  ⚠️ LEAK: {type.Name}.{eventInfo.Name} (STATIC) has {subscriberCount} subscribers");
                                        leakCount++;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Skip inspection failures
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip assembly failures
                }
            }
            
            if (leakCount == 0)
            {
                Log("  ✅ No event subscription leaks detected");
            }
            else
            {
                Log($"  ❌ Found {leakCount} potential event subscription leaks");
            }
            
            Log("");
        }

        void ScanStaticCollections()
        {
            Log("[2] STATIC COLLECTION GROWTH SCAN");
            Log("───────────────────────────────────────────────────────");
            
            int leakCount = 0;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("Tartaria")) continue;
                
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var staticFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        
                        foreach (var field in staticFields)
                        {
                            try
                            {
                                var value = field.GetValue(null);
                                if (value == null) continue;
                                
                                int collectionSize = -1;
                                string collectionType = "";
                                
                                // Check for common collection types
                                if (value is System.Collections.ICollection collection)
                                {
                                    collectionSize = collection.Count;
                                    collectionType = value.GetType().Name;
                                }
                                
                                if (collectionSize > 0)
                                {
                                    string key = $"{type.Name}.{field.Name}";
                                    
                                    // Track growth
                                    if (_staticCollectionSizes.TryGetValue(key, out int previousSize))
                                    {
                                        int growth = collectionSize - previousSize;
                                        if (growth > 0)
                                        {
                                            if (!_staticCollectionGrowth.ContainsKey(key))
                                            {
                                                _staticCollectionGrowth[key] = 0;
                                            }
                                            _staticCollectionGrowth[key] += growth;
                                        }
                                    }
                                    
                                    _staticCollectionSizes[key] = collectionSize;
                                    
                                    // Check threshold
                                    if (collectionSize > maxStaticCollectionSize)
                                    {
                                        Log($"  ⚠️ LEAK: {key} ({collectionType}) has {collectionSize} items (threshold: {maxStaticCollectionSize})");
                                        
                                        if (_staticCollectionGrowth.TryGetValue(key, out int totalGrowth))
                                        {
                                            Log($"      Growth: +{totalGrowth} items since first scan");
                                        }
                                        
                                        leakCount++;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Skip field inspection failures
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip assembly failures
                }
            }
            
            // Report on growing collections even if under threshold
            var growingCollections = _staticCollectionGrowth.Where(kvp => kvp.Value > 100).OrderByDescending(kvp => kvp.Value);
            if (growingCollections.Any())
            {
                Log($"  📈 Collections showing significant growth:");
                foreach (var kvp in growingCollections.Take(10))
                {
                    int currentSize = _staticCollectionSizes.GetValueOrDefault(kvp.Key, 0);
                    Log($"      {kvp.Key}: {currentSize} items (+{kvp.Value} total growth)");
                }
            }
            
            if (leakCount == 0)
            {
                Log("  ✅ No static collection leaks detected");
            }
            else
            {
                Log($"  ❌ Found {leakCount} static collections exceeding threshold");
            }
            
            Log("");
        }

        void ScanCoroutineLeaks()
        {
            Log("[3] COROUTINE LEAK SCAN");
            Log("───────────────────────────────────────────────────────");
            
            int leakCount = 0;
            var monoBehaviours = FindObjectsOfType<MonoBehaviour>();
            
            // This is approximate - Unity doesn't expose coroutine count directly
            // We scan for coroutine tracking fields
            foreach (var mb in monoBehaviours)
            {
                if (mb == null) continue;
                
                var type = mb.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                int coroutineFieldCount = 0;
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Coroutine))
                    {
                        var coroutine = field.GetValue(mb) as Coroutine;
                        if (coroutine != null)
                        {
                            coroutineFieldCount++;
                        }
                    }
                    else if (field.FieldType == typeof(List<Coroutine>))
                    {
                        var coroutines = field.GetValue(mb) as List<Coroutine>;
                        if (coroutines != null)
                        {
                            coroutineFieldCount += coroutines.Count;
                        }
                    }
                }
                
                if (coroutineFieldCount > maxCoroutinesPerComponent)
                {
                    Log($"  ⚠️ POTENTIAL LEAK: {type.Name} has {coroutineFieldCount} tracked coroutines (threshold: {maxCoroutinesPerComponent})");
                    Log($"      Object: {mb.gameObject.name}");
                    Log($"      Scene: {mb.gameObject.scene.name}");
                    leakCount++;
                }
            }
            
            if (leakCount == 0)
            {
                Log("  ✅ No coroutine leaks detected");
            }
            else
            {
                Log($"  ❌ Found {leakCount} components with excessive coroutines");
            }
            
            Log("");
        }

        void ScanResourceLeaks()
        {
            Log("[4] RESOURCE LEAK SCAN");
            Log("───────────────────────────────────────────────────────");
            
            // AudioSources
            var audioSources = FindObjectsOfType<AudioSource>();
            int idleAudioSources = audioSources.Count(a => !a.isPlaying && a.clip != null);
            Log($"  AudioSources: {audioSources.Length} total, {idleAudioSources} idle with clips loaded");
            
            if (idleAudioSources > 50)
            {
                Log($"      ⚠️ WARNING: {idleAudioSources} idle AudioSources (possible leak)");
            }
            
            // Particle Systems
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            int stoppedParticleSystems = particleSystems.Count(ps => !ps.isPlaying);
            Log($"  ParticleSystems: {particleSystems.Length} total, {stoppedParticleSystems} stopped");
            
            if (stoppedParticleSystems > 100)
            {
                Log($"      ⚠️ WARNING: {stoppedParticleSystems} stopped ParticleSystems (should be destroyed)");
            }
            
            // Textures (requires Unity Profiler API)
            var textures = Resources.FindObjectsOfTypeAll<Texture>();
            int loadedTextures = textures.Length;
            long textureMemoryMB = textures.Sum(t => UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)) / (1024 * 1024);
            Log($"  Textures: {loadedTextures} loaded, {textureMemoryMB} MB");
            
            // Materials
            var materials = Resources.FindObjectsOfTypeAll<Material>();
            int loadedMaterials = materials.Length;
            Log($"  Materials: {loadedMaterials} loaded");
            
            // AudioClips
            var audioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            int loadedAudioClips = audioClips.Length;
            long audioMemoryMB = audioClips.Sum(a => UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(a)) / (1024 * 1024);
            Log($"  AudioClips: {loadedAudioClips} loaded, {audioMemoryMB} MB");
            
            Log("");
        }

        void ScanUndestroyedObjects()
        {
            Log("[5] UNDESTROYED OBJECT SCAN");
            Log("───────────────────────────────────────────────────────");
            
            var allObjects = FindObjectsOfType<GameObject>();
            int dontDestroyCount = allObjects.Count(go => go.scene.name == "DontDestroyOnLoad");
            
            Log($"  DontDestroyOnLoad objects: {dontDestroyCount}");
            
            if (dontDestroyCount > 50)
            {
                Log($"      ⚠️ WARNING: High count of persistent objects");
                
                // List top offenders
                var dontDestroyObjects = allObjects.Where(go => go.scene.name == "DontDestroyOnLoad")
                    .OrderByDescending(go => go.transform.childCount)
                    .Take(10);
                
                foreach (var go in dontDestroyObjects)
                {
                    Log($"      - {go.name} ({go.transform.childCount} children)");
                }
            }
            
            Log("");
        }

        void ScanTimerCallbacks()
        {
            Log("[6] TIMER/CALLBACK LEAK SCAN");
            Log("───────────────────────────────────────────────────────");
            
            // This is hard to detect automatically - log warning to manually check
            Log("  ⚠️ Manual check required:");
            Log("      - Invoke/InvokeRepeating not cancelled");
            Log("      - DOTween sequences not killed");
            Log("      - Custom timer systems not stopped");
            Log("  Run: GameObject.FindObjectsOfType<MonoBehaviour>().Where(m => m.IsInvoking())");
            
            var invoking = FindObjectsOfType<MonoBehaviour>().Where(m => m.IsInvoking()).ToList();
            if (invoking.Any())
            {
                Log($"  Found {invoking.Count} MonoBehaviours with active Invoke calls:");
                foreach (var mb in invoking.Take(10))
                {
                    Log($"      - {mb.GetType().Name} on {mb.gameObject.name}");
                }
            }
            else
            {
                Log("  ✅ No active Invoke calls detected");
            }
            
            Log("");
        }

        void Log(string message)
        {
            Debug.Log($"[MemoryLeakDetector] {message}");
            _reportBuilder?.AppendLine(message);
        }

        [ContextMenu("Force GC Collection")]
        public void ForceGCCollection()
        {
            long before = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long after = GC.GetTotalMemory(false);
            
            Debug.Log($"[MemoryLeakDetector] GC Collection: {before / (1024 * 1024)} MB → {after / (1024 * 1024)} MB (freed {(before - after) / (1024 * 1024)} MB)");
        }
    }
}
