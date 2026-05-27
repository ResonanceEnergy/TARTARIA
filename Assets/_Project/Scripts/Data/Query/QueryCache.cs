using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// LRU (Least Recently Used) cache for query results.
    /// Provides zero-allocation repeated queries by caching results.
    /// Automatically evicts oldest entries when capacity is reached.
    /// </summary>
    public class QueryCache<T> where T : class
    {
        readonly Dictionary<string, CacheEntry> _cache = new();
        readonly LinkedList<string> _lruList = new();
        readonly int _maxCapacity;
        readonly object _lock = new();

        class CacheEntry
        {
            public List<T> Results;
            public LinkedListNode<string> LruNode;
        }

        /// <summary>
        /// Creates a new query cache with the specified capacity.
        /// </summary>
        /// <param name="maxCapacity">Maximum number of cached queries (default 100)</param>
        public QueryCache(int maxCapacity = 100)
        {
            _maxCapacity = maxCapacity > 0 ? maxCapacity : 100;
        }

        /// <summary>
        /// Gets cached query results by key.
        /// Returns null if not found. Updates LRU position.
        /// </summary>
        public List<T> Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            lock (_lock)
            {
                if (!_cache.TryGetValue(key, out var entry))
                    return null;

                // Move to front of LRU list (most recently used)
                _lruList.Remove(entry.LruNode);
                _lruList.AddFirst(entry.LruNode);

                return entry.Results;
            }
        }

        /// <summary>
        /// Caches query results with the specified key.
        /// Evicts least recently used entry if cache is full.
        /// </summary>
        public void Set(string key, List<T> results)
        {
            if (string.IsNullOrEmpty(key) || results == null)
                return;

            lock (_lock)
            {
                // If key already exists, update it
                if (_cache.TryGetValue(key, out var existingEntry))
                {
                    existingEntry.Results = results;
                    
                    // Move to front
                    _lruList.Remove(existingEntry.LruNode);
                    _lruList.AddFirst(existingEntry.LruNode);
                    
                    return;
                }

                // Evict least recently used if at capacity
                if (_cache.Count >= _maxCapacity)
                {
                    var lruKey = _lruList.Last.Value;
                    _lruList.RemoveLast();
                    _cache.Remove(lruKey);
                }

                // Add new entry
                var node = _lruList.AddFirst(key);
                var entry = new CacheEntry
                {
                    Results = results,
                    LruNode = node
                };
                
                _cache[key] = entry;
            }
        }

        /// <summary>
        /// Clears all cached queries.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
                _lruList.Clear();
            }
        }

        /// <summary>
        /// Gets the current number of cached queries.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Gets cache statistics for debugging/monitoring.
        /// </summary>
        public CacheStats GetStats()
        {
            lock (_lock)
            {
                return new CacheStats
                {
                    CachedQueries = _cache.Count,
                    MaxCapacity = _maxCapacity,
                    Utilization = (float)_cache.Count / _maxCapacity
                };
            }
        }

        /// <summary>
        /// Cache statistics structure.
        /// </summary>
        public struct CacheStats
        {
            public int CachedQueries;
            public int MaxCapacity;
            public float Utilization;

            public override string ToString()
            {
                return $"Cache: {CachedQueries}/{MaxCapacity} ({Utilization:P0} full)";
            }
        }
    }
}
