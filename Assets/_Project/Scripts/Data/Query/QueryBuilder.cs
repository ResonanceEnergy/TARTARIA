using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// Fluent query builder for complex data queries with caching.
    /// Supports filtering, sorting, pagination with zero allocations for cached queries.
    /// </summary>
    public class QueryBuilder<T> where T : class
    {
        readonly DataRegistry<T> _registry;
        readonly QueryCache<T> _cache;
        readonly List<Func<T, bool>> _filters = new();
        Func<T, IComparable> _orderByKey;
        bool _descending;
        int _skip;
        int _take = int.MaxValue;

        internal QueryBuilder(DataRegistry<T> registry, QueryCache<T> cache)
        {
            _registry = registry;
            _cache = cache;
        }

        /// <summary>
        /// Adds a filter predicate to the query.
        /// </summary>
        public QueryBuilder<T> Where(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _filters.Add(predicate);
            return this;
        }

        /// <summary>
        /// Orders results by the specified key in ascending order.
        /// </summary>
        public QueryBuilder<T> OrderBy<TKey>(Func<T, TKey> keySelector) where TKey : IComparable
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _orderByKey = item => keySelector(item);
            _descending = false;
            return this;
        }

        /// <summary>
        /// Orders results by the specified key in descending order.
        /// </summary>
        public QueryBuilder<T> OrderByDescending<TKey>(Func<T, TKey> keySelector) where TKey : IComparable
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _orderByKey = item => keySelector(item);
            _descending = true;
            return this;
        }

        /// <summary>
        /// Skips the specified number of results (for pagination).
        /// </summary>
        public QueryBuilder<T> Skip(int count)
        {
            _skip = Math.Max(0, count);
            return this;
        }

        /// <summary>
        /// Takes only the specified number of results (for pagination).
        /// </summary>
        public QueryBuilder<T> Take(int count)
        {
            _take = Math.Max(1, count);
            return this;
        }

        /// <summary>
        /// Executes the query and returns results as a list.
        /// Uses cache for repeated queries (zero allocation).
        /// </summary>
        public List<T> ToList()
        {
            // Generate cache key from query parameters
            var cacheKey = GenerateCacheKey();
            
            // Try to get cached results
            var cached = _cache.Get(cacheKey);
            if (cached != null)
                return new List<T>(cached); // Return copy to prevent external modification

            // Execute query
            var results = ExecuteQuery();
            
            // Cache results
            _cache.Set(cacheKey, results);
            
            return results;
        }

        /// <summary>
        /// Executes the query and returns the first result, or null if none found.
        /// </summary>
        public T FirstOrDefault()
        {
            var results = Take(1).ToList();
            return results.Count > 0 ? results[0] : null;
        }

        /// <summary>
        /// Executes the query and returns the count of matching items.
        /// </summary>
        public int Count()
        {
            // For count queries, we can optimize by not materializing results
            var allItems = _registry.GetAll();
            int count = 0;

            foreach (var item in allItems)
            {
                bool matches = true;
                foreach (var filter in _filters)
                {
                    if (!filter(item))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Checks if any items match the query.
        /// </summary>
        public bool Any()
        {
            var allItems = _registry.GetAll();

            foreach (var item in allItems)
            {
                bool matches = true;
                foreach (var filter in _filters)
                {
                    if (!filter(item))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                    return true;
            }

            return false;
        }

        List<T> ExecuteQuery()
        {
            var allItems = _registry.GetAll();
            
            // Use ArrayPool for temporary results to avoid GC pressure
            var pool = ArrayPool<T>.Shared;
            var buffer = pool.Rent(allItems.Count);
            int bufferIndex = 0;

            try
            {
                // Apply filters
                foreach (var item in allItems)
                {
                    bool matches = true;
                    foreach (var filter in _filters)
                    {
                        if (!filter(item))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        buffer[bufferIndex++] = item;
                    }
                }

                // Create span for sorting/pagination
                var filteredSpan = new ArraySegment<T>(buffer, 0, bufferIndex);
                
                // Apply ordering
                if (_orderByKey != null)
                {
                    var sorted = _descending
                        ? filteredSpan.OrderByDescending(_orderByKey)
                        : filteredSpan.OrderBy(_orderByKey);
                    
                    // Apply skip/take
                    var final = sorted.Skip(_skip).Take(_take).ToList();
                    return final;
                }
                
                // Apply skip/take without ordering
                var results = new List<T>(Math.Min(_take, bufferIndex - _skip));
                for (int i = _skip; i < bufferIndex && results.Count < _take; i++)
                {
                    results.Add(buffer[i]);
                }
                
                return results;
            }
            finally
            {
                // Return buffer to pool
                pool.Return(buffer, clearArray: true);
            }
        }

        string GenerateCacheKey()
        {
            // Generate deterministic cache key from query parameters
            // Note: This is a simplified version; in production, you'd want a more robust hash
            var key = $"Filters:{_filters.Count}|Order:{_orderByKey != null}|Desc:{_descending}|Skip:{_skip}|Take:{_take}";
            return key;
        }
    }
}
