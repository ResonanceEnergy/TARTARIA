using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tartaria.Core
{
    /// <summary>
    /// R6 Memory Profiling + Leak Hunting for Echohaven and Moon density.
    /// </summary>
    [DisallowMultipleComponent]
    public class MemoryWatchdog : MonoBehaviour
    {
        public static MemoryWatchdog Instance { get; private set; }

        public int entityLeakThreshold = 2500;
        public float textureMemSpikeGB = 2.2f;
        public float pollInterval = 4.0f;

        long _lastEntityCount;
        long _peakEntities;
        float _lastTextureMemGB;
        float _peakTextureMemGB;
        int _leakEvents;
        readonly Dictionary<string, int> _poolSizes = new();
        readonly List<AsyncOperationHandle> _trackedHandles = new();

        float _pollTimer;
        bool _sceneLoaded;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _sceneLoaded = true;
            ResetPeaksForNewScene();
        }

        void OnSceneUnloaded(Scene scene)
        {
            _sceneLoaded = false;
            ReleaseAllTrackedMoonHandles();
        }

        void Update()
        {
            _pollTimer += Time.deltaTime;
            if (_pollTimer >= pollInterval)
            {
                _pollTimer = 0f;
                PollMemory();
            }
        }

        void PollMemory()
        {
            if (!_sceneLoaded) return;

            long entities = 0;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                entities = world.EntityManager.UniversalQuery.CalculateEntityCount();
            }
            _lastEntityCount = entities;
            if (entities > _peakEntities) _peakEntities = entities;

            float texGB = Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f * 1024f);
            _lastTextureMemGB = texGB;
            if (texGB > _peakTextureMemGB) _peakTextureMemGB = texGB;

            if (entities > entityLeakThreshold)
            {
                _leakEvents++;
                Debug.LogWarning($"[MemoryWatchdog] Potential leak: {entities} entities.");
            }
            if (texGB > textureMemSpikeGB)
            {
                Debug.LogWarning($"[MemoryWatchdog] VRAM spike: {texGB:F2}GB");
            }

            _poolSizes["MudGolem"] = 12;
            _poolSizes["Foliage"] = 60;
        }

        void ResetPeaksForNewScene()
        {
            _peakEntities = 0;
            _peakTextureMemGB = 0;
            _leakEvents = 0;
            _poolSizes.Clear();
        }

        public void TrackAddressableHandle(AsyncOperationHandle h, string label = "Moon")
        {
            if (!h.IsValid()) return;
            _trackedHandles.Add(h);
        }

        void ReleaseAllTrackedMoonHandles()
        {
            _trackedHandles.Clear();
        }

        public string GetMemoryReport()
        {
            return $"E:{_lastEntityCount} peak:{_peakEntities} Tex:{_lastTextureMemGB:F2}";
        }
    }
}
