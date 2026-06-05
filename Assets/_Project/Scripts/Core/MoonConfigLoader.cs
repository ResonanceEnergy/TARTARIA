using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// MoonConfigLoader — lazy lookup of <see cref="MoonConfig"/> assets by
    /// 1-based moon index. Loads from <c>Resources/MoonConfigs/Moon{N}Config</c>
    /// on first request and caches.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Level Designer (moonconfig-factory-seed).
    ///
    /// Authoring: place <c>Moon1Config.asset</c> under
    /// <c>Assets/_Project/Resources/MoonConfigs/</c> so <c>Resources.Load</c>
    /// resolves at runtime. The factory is intentionally read-only —
    /// gameplay systems consume the SO and never mutate.
    /// </summary>
    public static class MoonConfigLoader
    {
        const string ResourcePathFmt = "MoonConfigs/Moon{0}Config";
        static readonly Dictionary<int, MoonConfig> s_cache = new Dictionary<int, MoonConfig>(8);

        /// <summary>Returns the config for <paramref name="moonIndex"/> (1-based), or null if absent.</summary>
        public static MoonConfig Get(int moonIndex)
        {
            if (s_cache.TryGetValue(moonIndex, out var cached) && cached != null)
            {
                return cached;
            }
            var path = string.Format(ResourcePathFmt, moonIndex);
            var loaded = Resources.Load<MoonConfig>(path);
            if (loaded == null)
            {
                Debug.LogWarning($"[MoonConfigLoader] Missing MoonConfig for moon {moonIndex} at Resources/{path}.asset");
                return null;
            }
            s_cache[moonIndex] = loaded;
            return loaded;
        }

        /// <summary>Test/editor reset hook.</summary>
        public static void ClearCache() => s_cache.Clear();
    }
}
