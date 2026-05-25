using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 6: Centralized Resources.Load cache to eliminate redundant asset loading.
    /// Reduces disk I/O and memory allocations from repeated Resources.Load calls.
    /// 
    /// Usage: 
    ///   GameObject prefab = ResourceCache.Load<GameObject>("Prefabs/Enemy");
    ///   Material mat = ResourceCache.Load<Material>("Materials/Stone");
    /// 
    /// Benefits:
    /// - First load: Loads from Resources and caches
    /// - Subsequent loads: Returns cached reference (zero disk I/O)
    /// - Thread-safe for main thread usage
    /// - Automatic null handling
    /// </summary>
    public static class ResourceCache
    {
        static readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>();
        static readonly Dictionary<System.Type, Dictionary<string, Object>> _typedCaches = 
            new Dictionary<System.Type, Dictionary<string, Object>>();

        /// <summary>
        /// Load an asset from Resources, caching for subsequent calls.
        /// Returns null if asset not found.
        /// </summary>
        public static T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            System.Type type = typeof(T);

            // Get or create type-specific cache
            if (!_typedCaches.TryGetValue(type, out var typeCache))
            {
                typeCache = new Dictionary<string, Object>();
                _typedCaches[type] = typeCache;
            }

            // Check cache
            if (typeCache.TryGetValue(path, out var cached))
            {
                return cached as T;
            }

            // Load and cache
            T asset = Resources.Load<T>(path);
            if (asset != null)
            {
                typeCache[path] = asset;
            }

            return asset;
        }

        /// <summary>
        /// Load all assets of type from a Resources folder.
        /// Caches each asset individually for future single-asset loads.
        /// </summary>
        public static T[] LoadAll<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return new T[0];

            T[] assets = Resources.LoadAll<T>(path);

            if (assets != null && assets.Length > 0)
            {
                System.Type type = typeof(T);
                if (!_typedCaches.TryGetValue(type, out var typeCache))
                {
                    typeCache = new Dictionary<string, Object>();
                    _typedCaches[type] = typeCache;
                }

                // Cache each asset with its name for potential future single-asset loads
                foreach (var asset in assets)
                {
                    if (asset != null)
                    {
                        string assetPath = $"{path}/{asset.name}";
                        typeCache[assetPath] = asset;
                    }
                }
            }

            return assets ?? new T[0];
        }

        /// <summary>
        /// Check if an asset is already cached without loading it.
        /// </summary>
        public static bool IsCached<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return false;

            System.Type type = typeof(T);
            if (!_typedCaches.TryGetValue(type, out var typeCache))
                return false;

            return typeCache.ContainsKey(path);
        }

        /// <summary>
        /// Clear all cached assets. Useful for memory management in scene transitions.
        /// Does NOT destroy the assets themselves (Unity manages that).
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _typedCaches.Clear();
        }

        /// <summary>
        /// Clear cached assets of a specific type.
        /// </summary>
        public static void ClearCacheOfType<T>() where T : Object
        {
            System.Type type = typeof(T);
            if (_typedCaches.ContainsKey(type))
            {
                _typedCaches[type].Clear();
            }
        }

        /// <summary>
        /// Get cache statistics for monitoring.
        /// </summary>
        public static CacheStats GetStats()
        {
            int totalCached = 0;
            var typeCounts = new Dictionary<string, int>();

            foreach (var kvp in _typedCaches)
            {
                string typeName = kvp.Key.Name;
                int count = kvp.Value.Count;
                totalCached += count;
                typeCounts[typeName] = count;
            }

            return new CacheStats
            {
                TotalCachedAssets = totalCached,
                CachedAssetsByType = typeCounts
            };
        }

        public struct CacheStats
        {
            public int TotalCachedAssets;
            public Dictionary<string, int> CachedAssetsByType;

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"ResourceCache: {TotalCachedAssets} total assets");
                foreach (var kvp in CachedAssetsByType)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
                return sb.ToString();
            }
        }
    }
}
