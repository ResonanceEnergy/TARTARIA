using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// InventorySystem — Complete 10-slot grid implementation.
    /// TODO from REALITY_CHECK Phase 2.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Inventory Settings")]
        [SerializeField] private int inventorySize = 10;
        [SerializeField] private List<InventorySlot> slots = new();

        public int InventorySize => inventorySize;
        public List<InventorySlot> Slots => slots;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeInventory();
        }

        void InitializeInventory()
        {
            slots = new List<InventorySlot>(inventorySize);
            for (int i = 0; i < inventorySize; i++)
            {
                slots.Add(new InventorySlot { slotIndex = i, isEmpty = true });
            }
            Debug.Log($"[InventorySystem] ✅ Initialized {inventorySize}-slot inventory");
        }

        public bool AddItem(string itemId, int quantity = 1)
        {
            // Find existing stack
            foreach (var slot in slots)
            {
                if (!slot.isEmpty && slot.itemId == itemId && slot.quantity < 99)
                {
                    slot.quantity += quantity;
                    Debug.Log($"[InventorySystem] Added {quantity}x {itemId} to existing stack (slot {slot.slotIndex})");
                    GameEvents.FireInventoryChanged();
                    return true;
                }
            }

            // Find empty slot
            foreach (var slot in slots)
            {
                if (slot.isEmpty)
                {
                    slot.isEmpty = false;
                    slot.itemId = itemId;
                    slot.quantity = quantity;
                    Debug.Log($"[InventorySystem] Added {quantity}x {itemId} to slot {slot.slotIndex}");
                    GameEvents.FireInventoryChanged();
                    return true;
                }
            }

            Debug.LogWarning($"[InventorySystem] Inventory full! Cannot add {itemId}");
            return false; // Inventory full
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty && slot.itemId == itemId)
                {
                    if (slot.quantity >= quantity)
                    {
                        slot.quantity -= quantity;
                        if (slot.quantity <= 0)
                        {
                            slot.isEmpty = true;
                            slot.itemId = null;
                            slot.quantity = 0;
                        }
                        Debug.Log($"[InventorySystem] Removed {quantity}x {itemId}");
                        GameEvents.FireInventoryChanged();
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetItemCount(string itemId)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (!slot.isEmpty && slot.itemId == itemId)
                    total += slot.quantity;
            }
            return total;
        }

        public bool HasItem(string itemId) => GetItemCount(itemId) > 0;
        public bool IsFull() => slots.FindIndex(s => s.isEmpty) == -1;
    }

    [System.Serializable]
    public class InventorySlot
    {
        public int slotIndex;
        public bool isEmpty = true;
        public string itemId;
        public int quantity;
    }
}
