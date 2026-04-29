using UnityEngine;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Generic item pickup — attach to any GameObject to make it collectible.
    /// On E-key interaction, adds item to player inventory and destroys this GameObject.
    /// 
    /// Usage:
    ///   1. Create item prefab (e.g., shovel, aether_shard, crystal)
    ///   2. Add this component
    ///   3. Set itemId (matches inventory string key)
    ///   4. Set Layer to Interactable (9) or include in PlayerInputHandler's interactableLayer mask
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        [Header("Item Configuration")]
        [SerializeField, Tooltip("Unique item ID (e.g., 'shovel', 'aether_shard')")] 
        string itemId = "unknown_item";

        [SerializeField, Range(1, 100), Tooltip("Quantity to add on pickup")] 
        int quantity = 1;

        [SerializeField, Tooltip("Display name shown in UI prompts")] 
        string displayName = "Item";

        [Header("Feedback")]
        [SerializeField, Tooltip("Optional VFX prefab spawned on pickup")] 
        GameObject pickupVFX;

        [SerializeField, Tooltip("Audio clip to play on pickup (overrides default)")] 
        AudioClip pickupSFX;

        [SerializeField, Range(0f, 2f), Tooltip("Delay before destroying GameObject")] 
        float destroyDelay = 0.1f;

        [Header("VFX (Feature 3)")]
        [SerializeField, Tooltip("Shard collect VFX (auto-loaded if null)")] 
        GameObject shardCollectVFX;

        bool _wasPickedUp;

        // ─── IInteractable Implementation ────────────

        public string GetInteractPrompt()
        {
            return $"[E] Pick up {displayName} ({quantity}x)";
        }

        public void Interact(GameObject player)
        {
            if (_wasPickedUp)
                return;

            if (InventorySystem.Instance == null)
            {
                Debug.LogWarning($"[Pickup] InventorySystem not ready — cannot pick up {itemId}");
                return;
            }

            // Attempt to add to inventory
            bool success = InventorySystem.Instance.AddItem(itemId, quantity);
            if (!success)
            {
                Debug.LogWarning($"[Pickup] Inventory full — cannot pick up {itemId}");
                AudioManager.Instance?.PlaySFX2D("InventoryFull");
                return;
            }

            _wasPickedUp = true;

            // Feedback: VFX (Feature 3)
            if (pickupVFX != null)
                Instantiate(pickupVFX, transform.position, Quaternion.identity);
            else if (shardCollectVFX != null)
                Instantiate(shardCollectVFX, transform.position, Quaternion.identity);
            else
            {
                // Auto-load from Resources if not assigned
                var defaultVFX = UnityEngine.Resources.Load<GameObject>("VFX/ShardCollect");
                if (defaultVFX != null)
                    Instantiate(defaultVFX, transform.position, Quaternion.identity);
            }

            if (pickupSFX != null)
                AudioManager.Instance?.PlaySFX(pickupSFX, transform.position, 0.6f);
            else
                AudioManager.Instance?.PlaySFX2D("ItemPickup");

            HapticFeedbackManager.Instance?.PlayDiscovery();

            Debug.Log($"[Pickup] Collected {quantity}x {itemId} at {transform.position}");

            // Destroy this pickup
            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            // Show interaction radius reference (assumes PlayerInputHandler default of 3m)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
}
