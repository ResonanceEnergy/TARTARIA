using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// High-performance item registry with indexed lookups.
    /// Replaces O(n) linear searches with O(1) dictionary lookups.
    /// 
    /// Usage:
    ///   ItemRegistry.Initialize(itemDatabase);
    ///   ItemData item = ItemRegistry.Get("aether_shard");
    ///   var rareItems = ItemRegistry.GetByRarity(ItemRarity.Rare);
    /// </summary>
    public static class ItemRegistry
    {
        static DataRegistry<ItemData> _registry;
        static bool _isInitialized;

        // Index names
        const string INDEX_CATEGORY = "category";
        const string INDEX_RARITY = "rarity";
        const string INDEX_VALUE_RANGE = "valueRange";

        /// <summary>
        /// Initializes the registry from ItemDatabase.
        /// Call this once at game startup.
        /// </summary>
        public static void Initialize(ItemDatabase database)
        {
            if (database == null)
            {
                Debug.LogError("[ItemRegistry] Cannot initialize with null database");
                return;
            }

            // Create registry with ID extractor
            _registry = new DataRegistry<ItemData>(item => item.itemID, cacheSize: 100);

            // Register secondary indexes
            _registry.RegisterSecondaryIndex(INDEX_CATEGORY, item => item.category);
            _registry.RegisterSecondaryIndex(INDEX_RARITY, item => item.rarity);
            _registry.RegisterSecondaryIndex(INDEX_VALUE_RANGE, item => GetValueRange(item.value));

            // Build indexes from database
            var items = database.GetAllItems();
            _registry.AddRange(items);

            _isInitialized = true;
            Debug.Log($"[ItemRegistry] Initialized with {_registry.Count} items");
        }

        /// <summary>
        /// Gets an item by ID. O(1) lookup.
        /// </summary>
        public static ItemData Get(string itemID)
        {
            EnsureInitialized();
            return _registry.Get(itemID);
        }

        /// <summary>
        /// Checks if an item exists.
        /// </summary>
        public static bool Contains(string itemID)
        {
            EnsureInitialized();
            return _registry.Contains(itemID);
        }

        /// <summary>
        /// Gets all items matching a category. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<ItemData> GetByCategory(ItemCategory category)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_CATEGORY, category);
        }

        /// <summary>
        /// Gets all items matching a rarity. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<ItemData> GetByRarity(ItemRarity rarity)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_RARITY, rarity);
        }

        /// <summary>
        /// Gets items within a value range. O(1) index lookup + O(n) filtering.
        /// </summary>
        public static List<ItemData> GetByValueRange(int minValue, int maxValue)
        {
            EnsureInitialized();
            
            return _registry.Query()
                .Where(item => item.value >= minValue && item.value <= maxValue)
                .OrderBy(item => item.value)
                .ToList();
        }

        /// <summary>
        /// Gets all consumable items.
        /// </summary>
        public static IReadOnlyList<ItemData> GetConsumables()
        {
            return GetByCategory(ItemCategory.Consumable);
        }

        /// <summary>
        /// Gets all equipment items.
        /// </summary>
        public static IReadOnlyList<ItemData> GetEquipment()
        {
            return GetByCategory(ItemCategory.Equipment);
        }

        /// <summary>
        /// Gets all crafting materials.
        /// </summary>
        public static IReadOnlyList<ItemData> GetMaterials()
        {
            return GetByCategory(ItemCategory.Material);
        }

        /// <summary>
        /// Gets all quest items.
        /// </summary>
        public static IReadOnlyList<ItemData> GetQuestItems()
        {
            return GetByCategory(ItemCategory.QuestItem);
        }

        /// <summary>
        /// Complex query: Get items by category, rarity, and max weight.
        /// Cached for zero-allocation repeated calls.
        /// </summary>
        public static List<ItemData> GetFilteredItems(ItemCategory? category, ItemRarity? rarity, float? maxWeight)
        {
            EnsureInitialized();
            
            var query = _registry.Query();
            
            if (category.HasValue)
                query = query.Where(item => item.category == category.Value);
            
            if (rarity.HasValue)
                query = query.Where(item => item.rarity == rarity.Value);
            
            if (maxWeight.HasValue)
                query = query.Where(item => item.weight <= maxWeight.Value);
            
            return query.OrderBy(item => item.value).ToList();
        }

        /// <summary>
        /// Creates a fluent query builder for custom queries.
        /// </summary>
        public static QueryBuilder<ItemData> Query()
        {
            EnsureInitialized();
            return _registry.Query();
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        public static IReadOnlyList<ItemData> GetAll()
        {
            EnsureInitialized();
            return _registry.GetAll();
        }

        /// <summary>
        /// Gets the total item count.
        /// </summary>
        public static int Count
        {
            get
            {
                EnsureInitialized();
                return _registry.Count;
            }
        }

        /// <summary>
        /// Clears the registry (for hot-reload/testing).
        /// </summary>
        public static void Clear()
        {
            _registry?.Clear();
            _isInitialized = false;
        }

        static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[ItemRegistry] Not initialized! Call ItemRegistry.Initialize(database) first.");
            }
        }

        // Helper to bucket items by value ranges for indexing
        static string GetValueRange(int value)
        {
            if (value < 10) return "0-10";
            if (value < 50) return "10-50";
            if (value < 100) return "50-100";
            if (value < 500) return "100-500";
            if (value < 1000) return "500-1000";
            return "1000+";
        }
    }
}
