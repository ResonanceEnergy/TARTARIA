using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Shop UI — displays merchant inventory, handles purchases.
    /// Stub implementation for NPC dialogue integration.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
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
            Debug.Log($"[ShopUI] Opening shop: {merchantName} with {items.Length} items.");
            // TODO: Implement shop UI (item grid, purchase logic, currency check)
        }

        /// <summary>
        /// Close shop UI.
        /// </summary>
        public void CloseShop()
        {
            Debug.Log("[ShopUI] Closing shop.");
            gameObject.SetActive(false);
        }
    }
}
