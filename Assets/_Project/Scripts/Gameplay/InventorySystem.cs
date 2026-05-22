using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;
// using Tartaria.Save;  // B1 cycle-break: removed assembly dependency

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("InventorySystem");
            DontDestroyOnLoad(go);
            go.AddComponent<InventorySystem>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            // Wire save/load events
            if (Tartaria.Save.SaveManager.Instance != null)
            {
                Tartaria.Save.SaveManager.Instance.OnBeforeSave += OnSave;
                Tartaria.Save.SaveManager.Instance.OnAfterLoad += OnLoad;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            
            // Cleanup save/load event handlers
            if (Tartaria.Save.SaveManager.Instance != null)
            {
                Tartaria.Save.SaveManager.Instance.OnBeforeSave -= OnSave;
                Tartaria.Save.SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
        }
        
        void OnSave(Tartaria.Save.SaveData sd)
        {
            // Persist inventory to SaveData.player
            if (sd.player != null)
            {
                var itemIds = new List<string>();
                var itemCounts = new List<int>();
                
                foreach (var kvp in _items)
                {
                    itemIds.Add(kvp.Key);
                    itemCounts.Add(kvp.Value);
                }
                
                sd.player.inventoryItemIds = itemIds.ToArray();
                sd.player.inventoryItemCounts = itemCounts.ToArray();
                
                Debug.Log($"[Inventory] Saved {_items.Count} unique items");
            }
        }
        
        void OnLoad(Tartaria.Save.SaveData sd)
        {
            // Restore inventory from SaveData.player
            _items.Clear();
            
            if (sd.player != null && sd.player.inventoryItemIds != null && sd.player.inventoryItemCounts != null)
            {
                int count = Mathf.Min(sd.player.inventoryItemIds.Length, sd.player.inventoryItemCounts.Length);
                for (int i = 0; i < count; i++)
                {
                    string itemId = sd.player.inventoryItemIds[i];
                    int itemCount = sd.player.inventoryItemCounts[i];
                    
                    if (!string.IsNullOrEmpty(itemId) && itemCount > 0)
                    {
                        _items[itemId] = itemCount;
                    }
                }
                
                Debug.Log($"[Inventory] Loaded {_items.Count} unique items");
                OnInventoryChanged?.Invoke();
            }
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
            
            // Mark save dirty
            SaveManager.Instance?.MarkDirty();

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
            
            // Mark save dirty
            SaveManager.Instance?.MarkDirty();

            return true;
        }

        /// <summary>
        /// Returns all items in inventory (id→count dictionary).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllItems() => _items;

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
        /// Clears inventory (use with caution — no undo).
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            Debug.Log("[Inventory] Cleared");
            OnInventoryChanged?.Invoke();
            SaveManager.Instance?.MarkDirty();
        }
    }
}
