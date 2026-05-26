using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Core.Enums;

namespace Tartaria.Data
{
    /// <summary>
    /// Item Database — ScriptableObject collection of all game items.
    /// Provides centralized lookup and validation for item IDs.
    ///
    /// Create asset via: Assets → Create → Tartaria → Item Database
    /// Place at: Assets/_Project/Resources/ItemDatabase.asset
    ///
    /// Usage:
    ///   var db = ItemDatabase.LoadDatabase();
    ///   ItemData item = db.GetItem("aether_shard");
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Tartaria/Item Database", order = 99)]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Item Registry")]
        [Tooltip("All items in the game")]
        [SerializeField] List<ItemData> items = new();

        // Cached lookup dictionary (built on first access)
        Dictionary<string, ItemData> _itemLookup;

        /// <summary>
        /// Loads the singleton ItemDatabase from Resources.
        /// Returns null if not found (with error log).
        /// </summary>
        public static ItemDatabase LoadDatabase()
        {
            var db = Resources.Load<ItemDatabase>("ItemDatabase");
            if (db == null)
            {
                Debug.LogError("[ItemDatabase] Failed to load from Resources/ItemDatabase.asset — create it via Assets → Create → Tartaria → Item Database");
                return null;
            }
            return db;
        }

        /// <summary>
        /// Gets item data by ID. Returns null if not found.
        /// </summary>
        public ItemData GetItem(string itemID)
        {
            if (string.IsNullOrWhiteSpace(itemID))
                return null;

            // Build lookup dictionary on first use
            if (_itemLookup == null)
            {
                BuildLookup();
            }

            _itemLookup.TryGetValue(itemID, out ItemData item);

            if (item == null)
            {
                Debug.LogWarning($"[ItemDatabase] Item '{itemID}' not found in database");
            }

            return item;
        }

        /// <summary>
        /// Validates if an item ID exists in the database.
        /// </summary>
        public bool HasItem(string itemID)
        {
            if (string.IsNullOrWhiteSpace(itemID))
                return false;

            if (_itemLookup == null)
            {
                BuildLookup();
            }

            return _itemLookup.ContainsKey(itemID);
        }

        /// <summary>
        /// Returns all items in the database.
        /// </summary>
        public IReadOnlyList<ItemData> GetAllItems() => items;

        /// <summary>
        /// Returns all items matching a category.
        /// NOTE: For better performance, use ItemRegistry.GetByCategory() after initialization.
        /// This method falls back to O(n) search if registry is not initialized.
        /// </summary>
        public List<ItemData> GetItemsByCategory(ItemCategory category)
        {
            // Query.ItemRegistry disabled (Phase 11) — using fallback O(n) search
            // #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // if (Query.ItemRegistry.Count > 0)
            // {
            //     return Query.ItemRegistry.GetByCategory(category).ToList();
            // }
            // #endif

            // Fallback to O(n) search (pre-initialization or build-time)
            return items.Where(item => item.category == category).ToList();
        }

        /// <summary>
        /// Returns all items matching a rarity.
        /// NOTE: For better performance, use ItemRegistry.GetByRarity() after initialization.
        /// This method falls back to O(n) search if registry is not initialized.
        /// </summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            // Query.ItemRegistry disabled (Phase 11) — using fallback O(n) search
            // #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // if (Query.ItemRegistry.Count > 0)
            // {
            //     return Query.ItemRegistry.GetByRarity(rarity).ToList();
            // }
            // #endif

            // Fallback to O(n) search (pre-initialization or build-time)
            return items.Where(item => item.rarity == rarity).ToList();
        }

        /// <summary>
        /// Adds an item to the database (editor only).
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item == null)
                return;

            if (items.Contains(item))
            {
                Debug.LogWarning($"[ItemDatabase] Item '{item.itemID}' already in database");
                return;
            }

            // Check for duplicate IDs
            if (items.Any(existing => existing.itemID == item.itemID))
            {
                Debug.LogError($"[ItemDatabase] Duplicate itemID '{item.itemID}' — cannot add item '{item.name}'");
                return;
            }

            items.Add(item);
            _itemLookup = null; // Invalidate cache

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Removes an item from the database (editor only).
        /// </summary>
        public void RemoveItem(ItemData item)
        {
            if (item == null)
                return;

            items.Remove(item);
            _itemLookup = null; // Invalidate cache

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Builds the internal lookup dictionary from the items list.
        /// </summary>
        void BuildLookup()
        {
            _itemLookup = new Dictionary<string, ItemData>(items.Count);

            foreach (var item in items)
            {
                if (item == null)
                {
                    Debug.LogWarning("[ItemDatabase] Null item in database list");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.itemID))
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has empty itemID");
                    continue;
                }

                if (_itemLookup.ContainsKey(item.itemID))
                {
                    Debug.LogError($"[ItemDatabase] Duplicate itemID '{item.itemID}' found — keeping first instance");
                    continue;
                }

                _itemLookup[item.itemID] = item;
            }

            Debug.Log($"[ItemDatabase] Built lookup with {_itemLookup.Count} items");
        }

        /// <summary>
        /// Validates database integrity (called in editor).
        /// </summary>
        void OnValidate()
        {
            // Clear cache on edit
            _itemLookup = null;

            // Check for duplicates
            var ids = new HashSet<string>();
            var duplicates = new HashSet<string>();

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.itemID))
                    continue;

                if (!ids.Add(item.itemID))
                {
                    duplicates.Add(item.itemID);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogError($"[ItemDatabase] Found {duplicates.Count} duplicate itemIDs: {string.Join(", ", duplicates)}");
            }
        }
    }
}
