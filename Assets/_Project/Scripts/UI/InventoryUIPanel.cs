using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.UI
{
    /// <summary>
    /// Inventory UI Panel — displays player inventory grid, item tooltips.
    /// Attach to Canvas panel, wire to InventorySystem events.
    /// </summary>
    public class InventoryUIPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject itemSlotPrefab;
        [SerializeField] Transform gridContainer;
        [SerializeField] Text itemNameText;
        [SerializeField] Text itemDescriptionText;
        [SerializeField] Image itemIconImage;

        [Header("Settings")]
        [SerializeField] int gridColumns = 6;
        [SerializeField] int gridRows = 5;

        readonly List<InventorySlotUI> _slots = new();
        readonly Dictionary<string, Sprite> _itemIcons = new();  // Cached icons

        void Awake()
        {
            BuildInventoryGrid();
        }

        void Start()
        {
            // Subscribe to InventorySystem events
            var inventory = Gameplay.InventorySystem.Instance;
            if (inventory != null)
            {
                inventory.OnInventoryChanged += RefreshInventory;
                inventory.OnItemAdded += OnItemAddedHandler;
                inventory.OnItemRemoved += OnItemRemovedHandler;
            }

            gameObject.SetActive(false);  // Hidden by default
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            var inventory = Gameplay.InventorySystem.Instance;
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= RefreshInventory;
                inventory.OnItemAdded -= OnItemAddedHandler;
                inventory.OnItemRemoved -= OnItemRemovedHandler;
            }
        }

        void OnItemAddedHandler(string itemId, int newCount)
        {
            Debug.Log($"[InventoryUI] Item added: {itemId} x{newCount}");
            RefreshInventory();
        }

        void OnItemRemovedHandler(string itemId, int remainingCount)
        {
            Debug.Log($"[InventoryUI] Item removed: {itemId}, remaining: {remainingCount}");
            RefreshInventory();
        }

        void BuildInventoryGrid()
        {
            if (itemSlotPrefab == null || gridContainer == null) return;

            int totalSlots = gridColumns * gridRows;
            for (int i = 0; i < totalSlots; i++)
            {
                var slotGO = Instantiate(itemSlotPrefab, gridContainer);
                var slot = slotGO.GetComponent<InventorySlotUI>();
                if (slot != null)
                {
                    slot.SetSlotIndex(i);
                    slot.OnSlotClicked += HandleSlotClicked;
                    slot.OnSlotHovered += HandleSlotHovered;
                    _slots.Add(slot);
                }
            }

            Debug.Log($"[InventoryUI] Built {totalSlots} slots");
        }

        void HandleSlotClicked(int slotIndex)
        {
            Debug.Log($"[InventoryUI] Slot {slotIndex} clicked");
            // Note: Item use/equip/drop requires ItemDatabase (tracked in KNOWN_PLACEHOLDERS.md)
        }

        void HandleSlotHovered(int slotIndex, bool entered)
        {
            if (entered)
            {
                // Show tooltip
                ShowTooltip(slotIndex);
            }
            else
            {
                HideTooltip();
            }
        }

        void ShowTooltip(int slotIndex)
        {
            // Fetch item data from InventorySystem for tooltip
            var inventory = Gameplay.InventorySystem.Instance;
            if (inventory != null)
            {
                var allItems = inventory.GetAllItems();
                if (slotIndex < allItems.Count)
                {
                    var item = allItems.ElementAt(slotIndex);
                    if (itemNameText != null) itemNameText.text = item.Key;
                    if (itemDescriptionText != null) itemDescriptionText.text = $"Quantity: {item.Value}";
                    return;
                }
            }

            // Fallback if slot empty
            if (itemNameText != null) itemNameText.text = "Empty";
            if (itemDescriptionText != null) itemDescriptionText.text = "";
        }

        void HideTooltip()
        {
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescriptionText != null) itemDescriptionText.text = "";
        }

        public void ToggleInventory()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void RefreshInventory()
        {
            var inventory = Gameplay.InventorySystem.Instance;
            if (inventory == null)
            {
                Debug.LogWarning("[InventoryUI] InventorySystem not found");
                return;
            }

            // Clear all slots first
            foreach (var slot in _slots)
            {
                slot.ClearSlot();
            }

            // Populate slots with inventory items
            var items = inventory.GetAllItems();
            int slotIndex = 0;
            foreach (var kvp in items)
            {
                if (slotIndex >= _slots.Count) break;

                string itemId = kvp.Key;
                int count = kvp.Value;

                // Get icon sprite from Resources (ItemDatabase integration pending)
                Sprite icon = GetItemIcon(itemId);

                _slots[slotIndex].SetItem(itemId, count, icon);
                slotIndex++;
            }

            Debug.Log($"[InventoryUI] Refreshed {slotIndex} items across {_slots.Count} slots");
        }

        Sprite GetItemIcon(string itemId)
        {
            // Cache lookup
            if (_itemIcons.TryGetValue(itemId, out Sprite cached))
                return cached;

            // Try loading from Resources (convention: Resources/Items/{itemId}.png)
            Sprite icon = Resources.Load<Sprite>($"Items/{itemId}");
            if (icon != null)
                _itemIcons[itemId] = icon;

            return icon;  // May be null if not found
        }
    }
}
