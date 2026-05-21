using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tartaria.Core
{
    /// <summary>
    /// AddressableAssetLoader — central safe wrapper for Unity Addressables 2.x.
    /// Provides group labels, async prefab/scene loads, streaming ring helpers.
    /// Falls back gracefully for editor / pre-Addressables bootstrap.
    /// 
    /// Groups (per 09_TECHNICAL_SPEC.md + Phase 2):
    /// - Echohaven_Core : core Echohaven prefabs, initial content
    /// - KayKit_Assets : all KayKit forest/character/props (high reuse)
    /// - VFX_Common : shared particle/VFX prefabs
    /// - Audio_Common : SFX, ambient clips (music streamed separately)
    /// - Zone_Moon1_Echohaven : Moon 1 / Echohaven specific assets + subscene
    /// - Zone_Moon2 : Moon 2 zone assets
    /// 
    /// Memory budgets enforced via release handles. 500m streaming ring support.
    /// </summary>
    public static class AddressableAssetLoader
    {
        // === GROUP LABEL CONSTANTS (use these for Addressables groups / labels) ===
        public const string LABEL_ECHOHAVEN_CORE = "Echohaven_Core";
        public const string LABEL_KAYKIT_ASSETS = "KayKit_Assets";
        public const string LABEL_VFX_COMMON = "VFX_Common";
        public const string LABEL_AUDIO_COMMON = "Audio_Common";
        public const string LABEL_ZONE_MOON1 = "Zone_Moon1_Echohaven";
        public const string LABEL_ZONE_MOON2 = "Zone_Moon2";

        // 500m zone streaming ring constants (per roadmap / spec)
        public const float ZONE_RADIUS = 500f;
        public const float LOD_RING_OUTER = 1000f; // active -> LOD ring
        public const float UNLOAD_THRESHOLD = 1500f; // beyond this, unload

        static bool _initialized;
        static readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedPrefabs = new();
        static readonly Dictionary<string, List<AsyncOperationHandle>> _labelHandles = new(); // for batch release by group

        /// <summary>
        /// Initialize Addressables (call once early, e.g. from GameBootstrap or SceneLoader).
        /// Safe to call multiple times.
        /// </summary>
        public static async Task InitializeAsync()
        {
            if (_initialized) return;
            try
            {
                var initOp = Addressables.InitializeAsync();
                await initOp.Task;
                if (initOp.Status == AsyncOperationStatus.Succeeded)
                {
                    _initialized = true;
                    Debug.Log("[AddressableAssetLoader] Addressables initialized successfully.");
                }
                else
                {
                    Debug.LogWarning("[AddressableAssetLoader] Initialize failed, falling back to direct references.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AddressableAssetLoader] Initialize exception (safe fallback): {ex.Message}");
            }
        }

        /// <summary>
        /// Load a prefab by Addressables key or label. Returns cached if already loaded.
        /// Use for high-impact spawners (KayKit, VFX, content).
        /// </summary>
        public static async Task<GameObject> LoadPrefabAsync(string keyOrLabel, bool useLabel = false)
        {
            if (string.IsNullOrEmpty(keyOrLabel)) return null;

            string cacheKey = useLabel ? $"label:{keyOrLabel}" : keyOrLabel;
            if (_loadedPrefabs.TryGetValue(cacheKey, out var cached) && cached.IsValid())
            {
                return cached.Result;
            }

            if (!_initialized)
            {
                // Fallback: attempt direct Resources load or return null (caller handles)
                Debug.LogWarning($"[AddressableAssetLoader] Not initialized — cannot load '{keyOrLabel}'. Ensure InitializeAsync called early.");
                return null;
            }

            AsyncOperationHandle<GameObject> handle;
            if (useLabel)
            {
                // Load first asset matching label (for groups with single representative or use LoadAssets)
                handle = Addressables.LoadAssetAsync<GameObject>(new AssetLabelReference { labelString = keyOrLabel });
            }
            else
            {
                handle = Addressables.LoadAssetAsync<GameObject>(keyOrLabel);
            }

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _loadedPrefabs[cacheKey] = handle;
                TrackHandleForGroup(keyOrLabel, handle);
                return handle.Result;
            }

            Debug.LogWarning($"[AddressableAssetLoader] Failed to load prefab '{keyOrLabel}': {handle.Status}");
            if (handle.IsValid()) Addressables.Release(handle);
            return null;
        }

        /// <summary>
        /// Load multiple prefabs for a label (e.g. all KayKit rocks). Returns list.
        /// Caller responsible for instantiating.
        /// </summary>
        public static async Task<List<GameObject>> LoadPrefabsByLabelAsync(string label)
        {
            var results = new List<GameObject>();
            if (!_initialized || string.IsNullOrEmpty(label)) return results;

            var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
            await locHandle.Task;

            if (locHandle.Status != AsyncOperationStatus.Succeeded) return results;

            var loadTasks = new List<Task<GameObject>>();
            foreach (var loc in locHandle.Result)
            {
                var h = Addressables.LoadAssetAsync<GameObject>(loc);
                loadTasks.Add(h.Task.ContinueWith(t => h.Result, TaskContinuationOptions.OnlyOnRanToCompletion));
                TrackHandleForGroup(label, h);
            }

            Addressables.Release(locHandle);

            var loaded = await Task.WhenAll(loadTasks);
            foreach (var go in loaded)
            {
                if (go != null) results.Add(go);
            }
            return results;
        }

        /// <summary>
        /// Instantiate a prefab via Addressables (preferred over direct Instantiate for streaming).
        /// Auto-tracks handle for later release.
        /// </summary>
        public static async Task<GameObject> InstantiateAsync(string keyOrLabel, Vector3 position, Quaternion rotation, Transform parent = null, bool useLabel = false)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AddressableAssetLoader] Cannot InstantiateAsync before init.");
                return null;
            }

            AsyncOperationHandle<GameObject> handle = useLabel
                ? Addressables.InstantiateAsync(new AssetLabelReference { labelString = keyOrLabel }, position, rotation, parent)
                : Addressables.InstantiateAsync(keyOrLabel, position, rotation, parent);

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                TrackHandleForGroup(keyOrLabel, handle);
                return handle.Result;
            }

            Debug.LogWarning($"[AddressableAssetLoader] Instantiate failed for {keyOrLabel}");
            return null;
        }

        /// <summary>
        /// Release a previously loaded/ instantiated asset by key. Critical for memory budgets.
        /// </summary>
        public static void Release(string keyOrLabel, bool useLabel = false)
        {
            string cacheKey = useLabel ? $"label:{keyOrLabel}" : keyOrLabel;
            if (_loadedPrefabs.TryGetValue(cacheKey, out var handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _loadedPrefabs.Remove(cacheKey);
            }

            // Also release group tracked handles
            if (_labelHandles.TryGetValue(keyOrLabel, out var list))
            {
                foreach (var h in list)
                {
                    if (h.IsValid()) Addressables.Release(h);
                }
                _labelHandles.Remove(keyOrLabel);
            }
        }

        /// <summary>
        /// Release ALL assets for a group label (used on zone exit for streaming).
        /// </summary>
        public static void ReleaseGroup(string label)
        {
            if (_labelHandles.TryGetValue(label, out var handles))
            {
                foreach (var h in handles)
                {
                    if (h.IsValid()) Addressables.Release(h);
                }
                _labelHandles.Remove(label);
            }

            // Clean matching cache entries
            var toRemove = new List<string>();
            foreach (var kv in _loadedPrefabs)
            {
                if (kv.Key.Contains(label)) toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove)
            {
                var h = _loadedPrefabs[k];
                if (h.IsValid()) Addressables.Release(h);
                _loadedPrefabs.Remove(k);
            }
        }

        static void TrackHandleForGroup(string labelOrKey, AsyncOperationHandle handle)
        {
            if (!_labelHandles.ContainsKey(labelOrKey))
                _labelHandles[labelOrKey] = new List<AsyncOperationHandle>();
            _labelHandles[labelOrKey].Add(handle);
        }

        // === 500m ZONE STREAMING RING LOGIC (basic) ===
        // Call periodically (e.g. from a ZoneStreamer monobehaviour or GameLoop) with player pos.
        // For MVP: logs ring state; production would load/unload labeled sub-assets + LOD swaps.

        public struct StreamingRingState
        {
            public bool InActiveRing;   // <= 500m
            public bool InLODRing;      // 500-1000m
            public bool ShouldUnload;   // >1500m
            public float Distance;
        }

        public static StreamingRingState GetRingState(Vector3 playerPos, Vector3 zoneCenter)
        {
            float dist = Vector3.Distance(playerPos, zoneCenter);
            return new StreamingRingState
            {
                Distance = dist,
                InActiveRing = dist <= ZONE_RADIUS,
                InLODRing = dist > ZONE_RADIUS && dist <= LOD_RING_OUTER,
                ShouldUnload = dist > UNLOAD_THRESHOLD
            };
        }

        /// <summary>
        /// Example usage hook for zone transition / player move: decide load/release based on ring.
        /// For Echohaven (Moon1) center at ~origin.
        /// </summary>
        public static void EvaluateZoneStreaming(string zoneLabel, Vector3 zoneCenter, Vector3 playerPos, System.Action<string> onLoadGroup, System.Action<string> onUnloadGroup)
        {
            var state = GetRingState(playerPos, zoneCenter);
            if (state.InActiveRing)
            {
                // Ensure core + zone assets loaded (high priority)
                onLoadGroup?.Invoke(zoneLabel);
            }
            else if (state.InLODRing)
            {
                // Could swap to lower LOD variants (future: label "Zone_XXX_LOD")
            }
            else if (state.ShouldUnload)
            {
                onUnloadGroup?.Invoke(zoneLabel);
                ReleaseGroup(zoneLabel);
            }
        }

        /// <summary>
        /// Debug: report current memory-tracked loads.
        /// </summary>
        public static void LogMemoryReport()
        {
            Debug.Log($"[AddressableAssetLoader] Tracked prefabs: {_loadedPrefabs.Count}, groups: {_labelHandles.Count}. (Use Profiler for real budgets)");
        }

        /// <summary>
        /// Default implementation of IAssetService that delegates to this static loader.
        /// Register via ServiceLocator.Asset = new AddressableAssetLoader.DefaultAssetService();
        /// </summary>
        public class DefaultAssetService : IAssetService
        {
            public async System.Threading.Tasks.Task<GameObject> LoadPrefabAsync(string keyOrLabel, bool useLabel = false)
                => await AddressableAssetLoader.LoadPrefabAsync(keyOrLabel, useLabel);

            public async System.Threading.Tasks.Task<GameObject> InstantiateAsync(string keyOrLabel, Vector3 position, Quaternion rotation, Transform parent = null, bool useLabel = false)
                => await AddressableAssetLoader.InstantiateAsync(keyOrLabel, position, rotation, parent, useLabel);

            public void Release(string keyOrLabel, bool useLabel = false)
                => AddressableAssetLoader.Release(keyOrLabel, useLabel);

            public void ReleaseGroup(string label)
                => AddressableAssetLoader.ReleaseGroup(label);

            public StreamingRingState GetRingState(Vector3 playerPos, Vector3 zoneCenter)
                => AddressableAssetLoader.GetRingState(playerPos, zoneCenter);
        }
    }
}
