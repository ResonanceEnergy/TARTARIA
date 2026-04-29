using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Inventory System — 10-slot player inventory for items and consumables.
    /// 
    /// Design:
    ///   - Fixed 10-slot grid (expandable to 20 in later phases)
    ///   - Add/Remove/GetCount API
    ///   - Serialized to SaveData.inventoryItemIds/Counts
    ///   - Events trigger UI updates
    ///   - Items referenced by string id (e.g., "shovel", "aether_shard", "resonance_crystal")
    /// 
    /// Performance: event-driven, no per-frame cost.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Capacity")]
        [SerializeField, Range(5, 50)] int maxSlots = 10;

        // ─── Events ───
        public event Action<string, int> OnItemAdded;      // itemId, newCount
        public event Action<string, int> OnItemRemoved;    // itemId, remainingCount
        public event Action OnInventoryChanged;             // generic refresh signal

        readonly Dictionary<string, int> _items = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── API ───────────────────────────────────

        /// <summary>
        /// Adds items to inventory. Returns false if no space.
        /// </summary>
        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return false;

            // Check if we have space (unique item slots, not stack count)
            if (!_items.ContainsKey(itemId) && _items.Count >= maxSlots)
            {
                Debug.LogWarning($"[Inventory] Full — cannot add {itemId} (max {maxSlots} slots)");
                return false;
            }

            if (!_items.ContainsKey(itemId))
                _items[itemId] = 0;

            _items[itemId] += count;
            int newCount = _items[itemId];

            Debug.Log($"[Inventory] Added {count}x {itemId} (now {newCount})");
            OnItemAdded?.Invoke(itemId, newCount);
            OnInventoryChanged?.Invoke();

            AudioManager.Instance?.PlaySFX2D("ItemPickup");
            HapticFeedbackManager.Instance?.PlayDiscovery();

            return true;
        }

        /// <summary>
        /// Removes items from inventory. Returns false if not enough quantity.
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return false;

            if (!_items.TryGetValue(itemId, out int current) || current < count)
            {
                Debug.LogWarning($"[Inventory] Cannot remove {count}x {itemId} (have {current})");
                return false;
            }

            _items[itemId] -= count;
            int remaining = _items[itemId];

            if (remaining <= 0)
                _items.Remove(itemId);

            Debug.Log($"[Inventory] Removed {count}x {itemId} (remaining {remaining})");
            OnItemRemoved?.Invoke(itemId, remaining);
            OnInventoryChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Returns count of an item in inventory (0 if not present).
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return 0;
            return _items.TryGetValue(itemId, out int count) ? count : 0;
        }

        /// <summary>
        /// Checks if player has at least the specified quantity.
        /// </summary>
        public bool HasItem(string itemId, int minCount = 1)
        {
            return GetItemCount(itemId) >= minCount;
        }

        /// <summary>
        /// Returns a snapshot of all items (for UI display).
        /// </summary>
        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(_items);
        }

        /// <summary>
        /// Clears inventory (use with caution — no undo).
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            Debug.Log("[Inventory] Cleared");
            OnInventoryChanged?.Invoke();
        }
    }
}
