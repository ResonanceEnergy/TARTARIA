using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Inventory UI — runtime overlay displaying player inventory grid.
    /// Uses UI Toolkit (runtime) for lightweight, responsive layout.
    /// 
    /// Toggle with I-key or gamepad menu.
    /// Displays all items from InventorySystem.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        [Header("UI")]
        [SerializeField] UIDocument uiDocument;

        VisualElement _root;
        VisualElement _inventoryPanel;
        ListView _itemListView;
        Label _titleLabel;
        Label _capacityLabel;

        bool _isOpen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            if (_root == null)
                BuildUI();

            if (Tartaria.Gameplay.InventorySystem.Instance != null)
                Tartaria.Gameplay.InventorySystem.Instance.OnInventoryChanged += RefreshUI;
        }

        void OnDisable()
        {
            if (Tartaria.Gameplay.InventorySystem.Instance != null)
                Tartaria.Gameplay.InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Toggle with I-key
            if (UnityEngine.InputSystem.Keyboard.current?.iKey.wasPressedThisFrame ?? false)
                ToggleInventory();

            // Toggle with gamepad Select/Back button
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null && pad.selectButton.wasPressedThisFrame)
                ToggleInventory();
        }

        void BuildUI()
        {
            _root = uiDocument.rootVisualElement;
            _root.Clear();

            // Panel container (hidden by default)
            _inventoryPanel = new VisualElement();
            _inventoryPanel.name = "inventory-panel";
            _inventoryPanel.style.position = Position.Absolute;
            _inventoryPanel.style.top = 100;
            _inventoryPanel.style.left = 100;
            _inventoryPanel.style.width = 400;
            _inventoryPanel.style.minHeight = 300;
            _inventoryPanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            _inventoryPanel.style.borderBottomLeftRadius = 8;
            _inventoryPanel.style.borderBottomRightRadius = 8;
            _inventoryPanel.style.borderTopLeftRadius = 8;
            _inventoryPanel.style.borderTopRightRadius = 8;
            _inventoryPanel.style.paddingBottom = 16;
            _inventoryPanel.style.paddingLeft = 16;
            _inventoryPanel.style.paddingRight = 16;
            _inventoryPanel.style.paddingTop = 16;
            _inventoryPanel.style.display = DisplayStyle.None; // hidden by default

            // Title
            _titleLabel = new Label("Inventory");
            _titleLabel.style.fontSize = 24;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.color = new Color(0.9f, 0.85f, 0.6f);
            _titleLabel.style.marginBottom = 12;
            _inventoryPanel.Add(_titleLabel);

            // Capacity label
            _capacityLabel = new Label("0 / 10 slots");
            _capacityLabel.style.fontSize = 14;
            _capacityLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            _capacityLabel.style.marginBottom = 8;
            _inventoryPanel.Add(_capacityLabel);

            // Item list (scrollable)
            _itemListView = new ListView();
            _itemListView.style.flexGrow = 1;
            _itemListView.style.minHeight = 200;
            _itemListView.makeItem = MakeItem;
            _itemListView.bindItem = BindItem;
            _inventoryPanel.Add(_itemListView);

            // Close hint
            var closeHint = new Label("[I] or [Esc] to close");
            closeHint.style.fontSize = 12;
            closeHint.style.color = new Color(0.5f, 0.5f, 0.5f);
            closeHint.style.marginTop = 8;
            _inventoryPanel.Add(closeHint);

            _root.Add(_inventoryPanel);

            RefreshUI();
        }

        VisualElement MakeItem()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.paddingTop = 6;
            container.style.paddingBottom = 6;
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.marginBottom = 4;
            container.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
            container.style.borderBottomLeftRadius = 4;
            container.style.borderBottomRightRadius = 4;
            container.style.borderTopLeftRadius = 4;
            container.style.borderTopRightRadius = 4;

            var nameLabel = new Label();
            nameLabel.name = "item-name";
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            container.Add(nameLabel);

            var countLabel = new Label();
            countLabel.name = "item-count";
            countLabel.style.fontSize = 14;
            countLabel.style.color = new Color(0.8f, 0.8f, 0.5f);
            container.Add(countLabel);

            return container;
        }

        void BindItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _itemListView.itemsSource.Count)
                return;

            var kvp = (KeyValuePair<string, int>)_itemListView.itemsSource[index];
            var nameLabel = element.Q<Label>("item-name");
            var countLabel = element.Q<Label>("item-count");

            if (nameLabel != null)
                nameLabel.text = FormatItemName(kvp.Key);

            if (countLabel != null)
                countLabel.text = $"x{kvp.Value}";
        }

        string FormatItemName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return "Unknown Item";

            // Convert snake_case to Title Case
            return itemId.Replace("_", " ").ToUpper()[0] + itemId.Replace("_", " ").Substring(1);
        }

        void RefreshUI()
        {
            if (_itemListView == null || Tartaria.Gameplay.InventorySystem.Instance == null)
                return;

            var items = Tartaria.Gameplay.InventorySystem.Instance.GetAllItems();
            var itemList = new List<KeyValuePair<string, int>>(items);

            _itemListView.itemsSource = itemList;
            _itemListView.Rebuild();

            if (_capacityLabel != null)
                _capacityLabel.text = $"{items.Count} / 10 slots";
        }

        public void ToggleInventory()
        {
            _isOpen = !_isOpen;

            if (_inventoryPanel != null)
            {
                _inventoryPanel.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;

                if (_isOpen)
                {
                    RefreshUI();
                    // Pause game while inventory is open
                    Time.timeScale = 0f;
                }
                else
                {
                    // Resume game
                    Time.timeScale = 1f;
                }
            }

            Debug.Log($"[InventoryUI] {(_isOpen ? "Opened" : "Closed")}");
        }

        public void CloseInventory()
        {
            if (_isOpen)
                ToggleInventory();
        }
    }
}
