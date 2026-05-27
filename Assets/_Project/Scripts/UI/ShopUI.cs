using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Data;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Shop UI — displays merchant inventory, handles purchases.
    /// Minimal functional implementation: vertical list + buy buttons.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        GameObject _shopPanel;
        Transform _itemListContainer;
        TextMeshProUGUI _merchantNameText;
        TextMeshProUGUI _playerRSText;
        Button _closeButton;

        ItemDatabase _itemDB;
        string[] _currentItems;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Load item database
            _itemDB = ItemDatabase.LoadDatabase();
            if (_itemDB == null)
            {
                Debug.LogError("[ShopUI] ItemDatabase failed to load — shop will not function");
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Open shop with merchant name and item list.
        /// </summary>
        public void OpenShop(string merchantName, string[] items)
        {
            if (_itemDB == null)
            {
                Debug.LogError("[ShopUI] Cannot open shop — ItemDatabase not loaded");
                return;
            }

            _currentItems = items;

            // Build UI if not exists
            if (_shopPanel == null)
            {
                BuildShopUI();
            }

            // Populate shop
            _merchantNameText.text = $"{merchantName} - Shop";
            UpdatePlayerRS();
            PopulateItemList(items);

            _shopPanel.SetActive(true);
            Debug.Log($"[ShopUI] Opened shop: {merchantName} with {items.Length} items.");
        }

        /// <summary>
        /// Close shop UI.
        /// </summary>
        public void CloseShop()
        {
            if (_shopPanel != null)
            {
                _shopPanel.SetActive(false);
            }
            Debug.Log("[ShopUI] Closing shop.");
        }

        void BuildShopUI()
        {
            // Create shop panel
            _shopPanel = new GameObject("ShopPanel");
            _shopPanel.transform.SetParent(transform, false);

            var rt = _shopPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.25f, 0.15f);
            rt.anchorMax = new Vector2(0.75f, 0.85f);
            rt.sizeDelta = Vector2.zero;

            var bg = _shopPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Merchant name header
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(_shopPanel.transform, false);
            var headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 0.9f);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.sizeDelta = Vector2.zero;

            _merchantNameText = headerGO.AddComponent<TextMeshProUGUI>();
            _merchantNameText.text = "Merchant Shop";
            _merchantNameText.fontSize = 32;
            _merchantNameText.alignment = TextAlignmentOptions.Center;
            _merchantNameText.color = Color.white;

            // Player RS display
            var rsGO = new GameObject("PlayerRS");
            rsGO.transform.SetParent(_shopPanel.transform, false);
            var rsRT = rsGO.AddComponent<RectTransform>();
            rsRT.anchorMin = new Vector2(0, 0.82f);
            rsRT.anchorMax = new Vector2(1, 0.88f);
            rsRT.sizeDelta = Vector2.zero;

            _playerRSText = rsGO.AddComponent<TextMeshProUGUI>();
            _playerRSText.text = "RS: 0";
            _playerRSText.fontSize = 24;
            _playerRSText.alignment = TextAlignmentOptions.Center;
            _playerRSText.color = new Color(0.4f, 0.85f, 1f);

            // Item list container (scrollable)
            var listGO = new GameObject("ItemList");
            listGO.transform.SetParent(_shopPanel.transform, false);
            var listRT = listGO.AddComponent<RectTransform>();
            listRT.anchorMin = new Vector2(0.05f, 0.15f);
            listRT.anchorMax = new Vector2(0.95f, 0.8f);
            listRT.sizeDelta = Vector2.zero;

            _itemListContainer = listGO.transform;

            // Close button
            var closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(_shopPanel.transform, false);
            var closeRT = closeGO.AddComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.35f, 0.05f);
            closeRT.anchorMax = new Vector2(0.65f, 0.12f);
            closeRT.sizeDelta = Vector2.zero;

            _closeButton = closeGO.AddComponent<Button>();
            _closeButton.onClick.AddListener(CloseShop);

            var closeBG = closeGO.AddComponent<Image>();
            closeBG.color = new Color(0.8f, 0.2f, 0.2f);

            var closeTMP = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            closeTMP.transform.SetParent(closeGO.transform, false);
            var closeTextRT = closeTMP.GetComponent<RectTransform>();
            closeTextRT.anchorMin = Vector2.zero;
            closeTextRT.anchorMax = Vector2.one;
            closeTextRT.sizeDelta = Vector2.zero;
            closeTMP.text = "Close";
            closeTMP.fontSize = 24;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.color = Color.white;

            _shopPanel.SetActive(false);
        }

        void PopulateItemList(string[] itemIds)
        {
            // Clear existing items
            foreach (Transform child in _itemListContainer)
            {
                Destroy(child.gameObject);
            }

            if (_itemDB == null || itemIds == null || itemIds.Length == 0)
            {
                var noItemsGO = new GameObject("NoItems");
                noItemsGO.transform.SetParent(_itemListContainer, false);
                var tmp = noItemsGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "No items available";
                tmp.fontSize = 20;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.gray;
                return;
            }

            // Create item row for each
            for (int i = 0; i < itemIds.Length; i++)
            {
                string itemId = itemIds[i];
                ItemData itemData = _itemDB.GetItem(itemId);
                if (itemData == null)
                {
                    Debug.LogWarning($"[ShopUI] Item '{itemId}' not found in database");
                    continue;
                }

                CreateItemRow(itemData, i);
            }
        }

        void CreateItemRow(ItemData itemData, int index)
        {
            float rowHeight = 60f;
            float yPos = -index * (rowHeight + 10f);

            var rowGO = new GameObject($"Item_{itemData.itemID}");
            rowGO.transform.SetParent(_itemListContainer, false);
            var rowRT = rowGO.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0, 1);
            rowRT.anchorMax = new Vector2(1, 1);
            rowRT.anchoredPosition = new Vector2(0, yPos);
            rowRT.sizeDelta = new Vector2(0, rowHeight);

            // Item name + price label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.7f, 1);
            labelRT.sizeDelta = Vector2.zero;

            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = $"{itemData.displayName}\n<size=16>Price: {itemData.value} RS</size>";
            labelTMP.fontSize = 20;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.color = Color.white;

            // Buy button
            var buyGO = new GameObject("BuyButton");
            buyGO.transform.SetParent(rowGO.transform, false);
            var buyRT = buyGO.AddComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.72f, 0.1f);
            buyRT.anchorMax = new Vector2(0.98f, 0.9f);
            buyRT.sizeDelta = Vector2.zero;

            var buyButton = buyGO.AddComponent<Button>();
            buyButton.onClick.AddListener(() => PurchaseItem(itemData));

            var buyBG = buyGO.AddComponent<Image>();
            buyBG.color = new Color(0.2f, 0.7f, 0.3f);

            var buyTMP = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            buyTMP.transform.SetParent(buyGO.transform, false);
            var buyTextRT = buyTMP.GetComponent<RectTransform>();
            buyTextRT.anchorMin = Vector2.zero;
            buyTextRT.anchorMax = Vector2.one;
            buyTextRT.sizeDelta = Vector2.zero;
            buyTMP.text = "Buy";
            buyTMP.fontSize = 18;
            buyTMP.alignment = TextAlignmentOptions.Center;
            buyTMP.color = Color.white;
        }

        void PurchaseItem(ItemData item)
        {
            var aether = AetherFieldManager.Instance;
            var inventory = InventorySystem.Instance;

            if (aether == null || inventory == null)
            {
                Debug.LogError("[ShopUI] Cannot purchase — AetherFieldManager or InventorySystem not found");
                return;
            }

            float currentRS = aether.ResonanceScore;
            int price = item.value;

            if (currentRS < price)
            {
                Debug.Log($"[ShopUI] Insufficient RS: have {currentRS:F0}, need {price}");
                Audio.AudioManager.Instance?.PlaySFX2D("UI_Error");
                return;
            }

            // Deduct RS
            aether.DeductRS(price);

            // Add to inventory
            bool added = inventory.AddItem(item.itemID, 1);
            if (!added)
            {
                Debug.LogWarning($"[ShopUI] Failed to add item {item.itemID} to inventory (full?)");
                // Refund RS
                aether.AddResonanceScore(price);
                return;
            }

            Debug.Log($"[ShopUI] Purchased {item.displayName} for {price} RS");
            Audio.AudioManager.Instance?.PlaySFX2D("UI_Purchase");

            // Update RS display
            UpdatePlayerRS();
        }

        void UpdatePlayerRS()
        {
            if (_playerRSText != null && AetherFieldManager.Instance != null)
            {
                _playerRSText.text = $"RS: {AetherFieldManager.Instance.ResonanceScore:F0}";
            }
        }
    }
}
