using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Data;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Inventory System — 10-slot player inventory for items and consumables.
    /// 
    /// Design:
    ///   - Fixed 10-slot grid (expandable to 20 in later phases)
    ///   - Add/Remove/GetCount API
    ///   - ISaveDataProvider pattern (v17 modular extensibility)
    ///   - Events trigger UI updates
    ///   - Items referenced by string id (e.g., "shovel", "aether_shard", "resonance_crystal")
    ///   - Validates item IDs against ItemDatabase
    /// 
    /// Performance: event-driven, no per-frame cost.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventorySystem : MonoBehaviour, ISaveDataProvider
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Capacity")]
        [SerializeField, Range(5, 50)] int maxSlots = 10;
        
        [Header("Database")]
        [SerializeField] bool validateItemIDs = true;

        // ─── Events ───
        public event Action<string, int> OnItemAdded;      // itemId, newCount
        public event Action<string, int> OnItemRemoved;    // itemId, remainingCount
        public event Action OnInventoryChanged;             // generic refresh signal

        readonly Dictionary<string, int> _items = new();
        ItemDatabase _itemDatabase;

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
            
            // Load item database
            if (validateItemIDs)
            {
                _itemDatabase = ItemDatabase.LoadDatabase();
                if (_itemDatabase == null)
                {
                    Debug.LogWarning("[Inventory] ItemDatabase not found — item validation disabled");
                    validateItemIDs = false;
                }
            }
            
            // Register with SaveManager (ISaveDataProvider pattern)
            if (SaveManager.Instance != null)
                SaveManager.Instance.RegisterProvider(this);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            
            // Unregister from SaveManager
            if (SaveManager.Instance != null)
                SaveManager.Instance.UnregisterProvider(this);
        }

        // ═══════════════════════════════════════════════════════════════
        // ISaveDataProvider Implementation (v17 modular save pattern)
        // ═══════════════════════════════════════════════════════════════

        public string GetProviderKey() => "Inventory";

        public object GetSaveData()
        {
            var itemIds = new List<string>();
            var itemCounts = new List<int>();

            foreach (var kvp in _items)
            {
                itemIds.Add(kvp.Key);
                itemCounts.Add(kvp.Value);
            }

            return new InventoryData
            {
                itemIds = itemIds.ToArray(),
                itemCounts = itemCounts.ToArray()
            };
        }

        public void RestoreSaveData(object data)
        {
            _items.Clear();

            if (data == null)
            {
                Debug.Log("[Inventory] No saved data — initialized empty");
                OnInventoryChanged?.Invoke();
                return;
            }

            // Provider receives JSON string from SaveManager
            if (data is string json)
            {
                try
                {
                    var invData = JsonUtility.FromJson<InventoryData>(json);

                    if (invData.itemIds != null && invData.itemCounts != null)
                    {
                        int count = Mathf.Min(invData.itemIds.Length, invData.itemCounts.Length);
                        for (int i = 0; i < count; i++)
                        {
                            string itemId = invData.itemIds[i];
                            int itemCount = invData.itemCounts[i];

                            if (!string.IsNullOrEmpty(itemId) && itemCount > 0)
                            {
                                _items[itemId] = itemCount;
                            }
                        }
                    }

                    Debug.Log($"[Inventory] Loaded {_items.Count} unique items");
                    OnInventoryChanged?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Inventory] Failed to deserialize: {e.Message}");
                }
            }
        }

        // ─── API ───────────────────────────────────

        /// <summary>
        /// Adds items to inventory. Returns false if no space or invalid item ID.
        /// </summary>
        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return false;

            // Validate item ID against database
            if (validateItemIDs && _itemDatabase != null)
            {
                if (!_itemDatabase.HasItem(itemId))
                {
                    Debug.LogWarning($"[Inventory] Invalid item ID '{itemId}' — not found in ItemDatabase");
                    return false;
                }
            }

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
            
            // Fire legacy events
            OnItemAdded?.Invoke(itemId, newCount);
            OnInventoryChanged?.Invoke();
            
            // Fire GameEvents (decoupled pub/sub)
            Core.GameEvents.RaiseItemPickup(new Core.ItemPickupEventArgs
            {
                itemId = itemId,
                count = count,
                totalCount = newCount
            });

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
            
            // Fire GameEvents (decoupled pub/sub)
            Core.GameEvents.RaiseItemRemoved(new Core.ItemRemovedEventArgs
            {
                itemId = itemId,
                count = count,
                remainingCount = remaining,
                reason = "manual_removal"
            });

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
        /// Gets ItemData for an item ID from the database.
        /// Returns null if database is not loaded or item not found.
        /// </summary>
        public ItemData GetItemData(string itemId)
        {
            if (_itemDatabase == null)
                return null;

            return _itemDatabase.GetItem(itemId);
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

    /// <summary>
    /// Serializable data class for Inventory provider.
    /// MUST be serializable by JsonUtility (no generics, no null collections).
    /// </summary>
    [Serializable]
    public class InventoryData
    {
        public string[] itemIds = Array.Empty<string>();
        public int[] itemCounts = Array.Empty<int>();
    }
}
