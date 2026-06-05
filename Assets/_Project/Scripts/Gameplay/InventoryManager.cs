using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Data;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Inventory Manager — player inventory system.
    /// Tracks item collection, quantities, and provides add/remove/query API.
    /// Integrates with SaveManager for persistence.
    /// </summary>
    public class InventoryManager : MonoBehaviour, ISaveDataProvider
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxSlots = 48;
        [SerializeField] private bool enableDebugLogs = true;

        // itemID → quantity
        private Dictionary<string, int> _inventory = new Dictionary<string, int>();

        // Events
        public System.Action<string, int> OnItemAdded;
        public System.Action<string, int> OnItemRemoved;
        public System.Action OnInventoryChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[InventoryManager] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // Register with SaveManager
            var saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager != null)
            {
                saveManager.RegisterProvider(this);
                if (enableDebugLogs)
                    Debug.Log("[InventoryManager] Registered with SaveManager");
            }
        }

        /// <summary>
        /// Add item to inventory. Returns true if successful.
        /// </summary>
        public bool AddItem(string itemID, int count = 1)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogError("[InventoryManager] Cannot add item: itemID is null/empty");
                return false;
            }

            if (count <= 0)
            {
                Debug.LogWarning($"[InventoryManager] AddItem called with count {count}, ignoring");
                return false;
            }

            // Check if inventory is full (slot count limit)
            if (!_inventory.ContainsKey(itemID) && _inventory.Count >= maxSlots)
            {
                Debug.LogWarning($"[InventoryManager] Inventory full ({maxSlots} slots), cannot add {itemID}");
                return false;
            }

            // Add or increment
            if (_inventory.ContainsKey(itemID))
            {
                _inventory[itemID] += count;
            }
            else
            {
                _inventory[itemID] = count;
            }

            OnItemAdded?.Invoke(itemID, count);
            OnInventoryChanged?.Invoke();

            if (enableDebugLogs)
                Debug.Log($"[InventoryManager] Added {count}x {itemID} (total: {_inventory[itemID]})");

            return true;
        }

        /// <summary>
        /// Remove item from inventory. Returns true if successful.
        /// </summary>
        public bool RemoveItem(string itemID, int count = 1)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogError("[InventoryManager] Cannot remove item: itemID is null/empty");
                return false;
            }

            if (!_inventory.ContainsKey(itemID))
            {
                Debug.LogWarning($"[InventoryManager] Cannot remove {itemID}: not in inventory");
                return false;
            }

            if (_inventory[itemID] < count)
            {
                Debug.LogWarning($"[InventoryManager] Cannot remove {count}x {itemID}: only {_inventory[itemID]} available");
                return false;
            }

            _inventory[itemID] -= count;

            if (_inventory[itemID] <= 0)
            {
                _inventory.Remove(itemID);
            }

            OnItemRemoved?.Invoke(itemID, count);
            OnInventoryChanged?.Invoke();

            if (enableDebugLogs)
                Debug.Log($"[InventoryManager] Removed {count}x {itemID}");

            return true;
        }

        /// <summary>
        /// Check if inventory contains at least [count] of an item.
        /// </summary>
        public bool HasItem(string itemID, int count = 1)
        {
            if (string.IsNullOrEmpty(itemID))
                return false;

            return _inventory.TryGetValue(itemID, out int currentCount) && currentCount >= count;
        }

        /// <summary>
        /// Get quantity of an item in inventory.
        /// </summary>
        public int GetItemCount(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
                return 0;

            return _inventory.TryGetValue(itemID, out int count) ? count : 0;
        }

        /// <summary>
        /// Get all items in inventory as (itemID, count) pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> GetAllItems()
        {
            return _inventory;
        }

        /// <summary>
        /// Clear entire inventory.
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            OnInventoryChanged?.Invoke();

            if (enableDebugLogs)
                Debug.Log("[InventoryManager] Inventory cleared");
        }

        /// <summary>
        /// Get total number of unique items (slot count).
        /// </summary>
        public int GetOccupiedSlotCount()
        {
            return _inventory.Count;
        }

        /// <summary>
        /// Get available slot count.
        /// </summary>
        public int GetAvailableSlotCount()
        {
            return maxSlots - _inventory.Count;
        }

        #region Save/Load Integration

        public string GetProviderKey()
        {
            return "inventory";
        }

        public object GetSaveData()
        {
            var saveData = new InventorySaveData
            {
                items = _inventory.Select(kvp => new ItemEntry
                {
                    itemID = kvp.Key,
                    count = kvp.Value
                }).ToArray()
            };

            return saveData;
        }

        public void RestoreSaveData(object data)
        {
            if (data is string json)
            {
                var saveData = JsonUtility.FromJson<InventorySaveData>(json);
                if (saveData != null)
                {
                    _inventory.Clear();

                    if (saveData.items != null)
                    {
                        foreach (var entry in saveData.items)
                        {
                            _inventory[entry.itemID] = entry.count;
                        }
                    }

                    OnInventoryChanged?.Invoke();

                    if (enableDebugLogs)
                        Debug.Log($"[InventoryManager] Loaded {_inventory.Count} unique items");
                }
            }
            else
            {
                Debug.LogWarning("[InventoryManager] RestoreSaveData received invalid data type");
            }
        }

        [System.Serializable]
        class InventorySaveData
        {
            public ItemEntry[] items;
        }

        [System.Serializable]
        class ItemEntry
        {
            public string itemID;
            public int count;
        }

        #endregion
    }
}
