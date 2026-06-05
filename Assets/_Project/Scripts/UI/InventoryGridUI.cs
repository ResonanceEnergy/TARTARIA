using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tartaria.Data;

namespace Tartaria.UI
{
#pragma warning disable CS0414 // Field assigned but never used - reserved for future implementation
    /// <summary>
    /// Inventory grid UI - displays player inventory in grid layout.
    /// Called by EquipmentUI when player clicks equipment slot to swap items.
    /// </summary>
    public class InventoryGridUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject gridPanel;
        [SerializeField] Transform gridContainer;
        [SerializeField] GameObject slotPrefab;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Button closeButton;

        [Header("Grid Config")]
        [SerializeField] int columns = 8;
        [SerializeField] int totalSlots = 48;

        List<InventorySlot> _slots = new List<InventorySlot>();
        int _highlightedEquipSlot = -1;

        void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            // Initialize grid slots (deferred until first open to avoid startup lag)
        }

        void Start()
        {
            if (gridPanel != null)
            {
                gridPanel.SetActive(false);
                AdjustForAspectRatio();
            }
        }

        /// <summary>
        /// Adjust gridPanel anchors for ultrawide displays (21:9, 32:9).
        /// Ensures grid stays centered and doesn't clip off-screen.
        /// </summary>
        void AdjustForAspectRatio()
        {
            if (gridPanel == null) return;

            var rectTransform = gridPanel.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            float aspectRatio = (float)Screen.width / Screen.height;

            // Standard 16:9 = 1.778, 21:9 = 2.333, 32:9 = 3.556
            if (aspectRatio > 2.2f) // Ultrawide detected (21:9+)
            {
                // Center anchors (stretch from center)
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Clamp width to prevent overflow at extreme aspect ratios
                float maxWidth = aspectRatio > 3.0f ? 1400f : 1600f; // 32:9 vs 21:9
                float height = 900f;

                rectTransform.sizeDelta = new Vector2(maxWidth, height);
                rectTransform.anchoredPosition = Vector2.zero; // Centered

                Debug.Log($"[InventoryGridUI] Ultrawide aspect {aspectRatio:F2} detected - adjusted anchors (width={maxWidth})");
            }
            else // 16:9 or narrower - use default layout
            {
                // Default anchors should be set in Unity Editor
                // If not set, use safe centered defaults
                if (rectTransform.anchorMin == rectTransform.anchorMax &&
                    rectTransform.anchorMin == Vector2.zero)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.sizeDelta = new Vector2(1600f, 900f);
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Open inventory grid with specified equipment slot highlighted.
        /// Called from EquipmentUI.HandleSlotClick.
        /// </summary>
        public void OpenAt(int equipSlotIndex)
        {
            _highlightedEquipSlot = equipSlotIndex;

            // Lazy-initialize grid on first open
            if (_slots.Count == 0)
                InitializeGrid();

            if (gridPanel != null)
            {
                gridPanel.SetActive(true);
                // Re-adjust on every open in case resolution changed
                AdjustForAspectRatio();
            }

            if (titleText != null)
                titleText.text = $"Inventory (Select item for Slot {equipSlotIndex})";

            RefreshInventory();

            Debug.Log($"[InventoryGridUI] Opened for equipment slot {equipSlotIndex}");
        }

        public void Close()
        {
            if (gridPanel != null)
                gridPanel.SetActive(false);

            _highlightedEquipSlot = -1;
        }

        void InitializeGrid()
        {
            if (gridContainer == null || slotPrefab == null)
            {
                Debug.LogWarning("[InventoryGridUI] Missing grid container or slot prefab - cannot initialize");
                return;
            }

            // Create grid slots
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer);
                slotObj.name = $"InventorySlot_{i}";

                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot == null)
                    slot = slotObj.AddComponent<InventorySlot>();

                slot.slotIndex = i;
                slot.grid = this;

                _slots.Add(slot);
            }

            Debug.Log($"[InventoryGridUI] Initialized {totalSlots} inventory slots");
        }

        void RefreshInventory()
        {
            var inventoryManager = Tartaria.Gameplay.InventoryManager.Instance;
            if (inventoryManager == null)
            {
                Debug.LogWarning("[InventoryGridUI] No InventoryManager found");
                return;
            }

            // Clear all slots
            foreach (var slot in _slots)
            {
                slot.SetEmpty();
            }

            // Load inventory items
            int slotIndex = 0;
            foreach (var kvp in inventoryManager.GetAllItems())
            {
                if (slotIndex >= _slots.Count)
                {
                    Debug.LogWarning("[InventoryGridUI] More items than slots");
                    break;
                }

                // Load item data from ItemDatabase
                var itemDatabase = ItemDatabase.LoadDatabase();
                if (itemDatabase != null)
                {
                    var itemData = itemDatabase.GetItem(kvp.Key);
                    if (itemData != null)
                    {
                        _slots[slotIndex].SetItem(itemData, kvp.Value);
                        _slots[slotIndex].itemID = kvp.Key; // Store itemID for click handling
                    }
                    else
                    {
                        Debug.LogWarning($"[InventoryGridUI] Item {kvp.Key} not found in database");
                    }
                }

                slotIndex++;
            }
        }

        public void OnSlotClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count)
            {
                Debug.LogError($"[InventoryGridUI] Invalid slot index: {slotIndex}");
                return;
            }

            var slot = _slots[slotIndex];
            if (string.IsNullOrEmpty(slot.itemID))
            {
                Debug.Log("[InventoryGridUI] Clicked empty slot");
                return;
            }

            // Get managers
            var inventoryManager = Tartaria.Gameplay.InventoryManager.Instance;
            var equipmentManager = FindFirstObjectByType<Tartaria.Gameplay.EquipmentSlotManager>();

            if (inventoryManager == null || equipmentManager == null)
            {
                Debug.LogError("[InventoryGridUI] InventoryManager or EquipmentSlotManager not found");
                return;
            }

            // Load equipment item data from Resources
            string itemID = slot.itemID;
            var equipmentData = Resources.Load<EquipmentItemData>($"Equipment/{itemID}");
            if (equipmentData == null)
            {
                equipmentData = Resources.Load<EquipmentItemData>(itemID); // Fallback to root
            }

            if (equipmentData == null)
            {
                Debug.LogWarning($"[InventoryGridUI] {itemID} is not an equipment item");
                return;
            }

            // Get equipment slot enum from equipment data
            EquipSlot targetSlot = equipmentData.slot;

            // Check if clicked slot matches the highlighted equipment slot
            if ((int)targetSlot != _highlightedEquipSlot)
            {
                Debug.LogWarning($"[InventoryGridUI] {itemID} cannot be equipped in slot {_highlightedEquipSlot} (requires {targetSlot})");
                return;
            }

            // Get currently equipped item (if any)
            var currentEquipment = equipmentManager.GetEquippedItem(targetSlot);

            // Perform swap
            // 1. Remove item from inventory
            inventoryManager.RemoveItem(itemID, 1);

            // 2. Equip new item
            equipmentManager.EquipItem(targetSlot, equipmentData);

            // 3. Add old equipment back to inventory (if any)
            if (currentEquipment != null)
            {
                inventoryManager.AddItem(currentEquipment.itemID, 1);
            }

            Debug.Log($"[InventoryGridUI] Swapped {itemID} into {targetSlot} slot (old: {currentEquipment?.itemID ?? "none"})");

            // Refresh inventory UI
            RefreshInventory();

            // EquipmentUI will auto-refresh via OnEquipmentChanged event subscription

            // Close inventory after swap
            Close();
        }
    }

    /// <summary>
    /// Individual inventory slot component.
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        public int slotIndex;
        public InventoryGridUI grid;
        public string itemID; // Item ID stored for swap logic

        Image _icon;
        Button _button;
        TextMeshProUGUI _countText;

        void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(OnClick);

            _icon = GetComponentInChildren<Image>();
            _countText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetEmpty()
        {
            itemID = null;

            if (_icon != null)
                _icon.enabled = false;

            if (_countText != null)
                _countText.text = "";
        }

        public void SetItem(ItemData item, int count)
        {
            itemID = item.itemID;

            if (_icon != null)
            {
                _icon.enabled = true;
                _icon.sprite = item.icon;
            }

            if (_countText != null && count > 1)
                _countText.text = count.ToString();
        }

        void OnClick()
        {
            if (grid != null)
                grid.OnSlotClicked(slotIndex);
        }
    }
}
