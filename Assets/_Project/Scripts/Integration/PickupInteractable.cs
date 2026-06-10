using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Pickup Interactable — Generic item pickup (shards, tools, quest items)
    /// Implements IInteractable, auto-adds to inventory on E-key
    /// </summary>
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        [Header("Item Data")]
        [SerializeField] string itemId = "aether_shard";
        [SerializeField] int itemCount = 1;
        [SerializeField] string displayName = "Aether Shard";

        [Header("Visual Feedback")]
        [SerializeField] GameObject visualObject;
        [SerializeField] float bobSpeed = 1f;
        [SerializeField] float bobHeight = 0.2f;
        [SerializeField] float rotateSpeed = 45f;

        Vector3 _startPos;
        bool _pickedUp;

        void Start()
        {
            _startPos = transform.position;
        }

        void Update()
        {
            if (_pickedUp) return;

            // Bob up/down
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);

            // Rotate
            if (visualObject != null)
                visualObject.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        public string GetInteractPrompt()
        {
            // R412: F310 controller shows [A] when gamepad active
            return Tartaria.Input.InputPromptHelper.Localize($"[E] Pick up {displayName}");
        }

        public void Interact(GameObject interactor)
        {
            if (_pickedUp) return;

            // Sprint 11 L1 fix (origin 1fb03541): real inventory add via Tartaria.Integration.InventorySystem.AddItem.
            // Both PickupInteractable and InventorySystem live in Tartaria.Integration.asmdef — direct singleton call is in-asmdef, no boundary leak.
            var inventory = InventorySystem.Instance;
            if (inventory == null)
            {
                Debug.LogError($"[Pickup] InventorySystem.Instance is NULL — cannot pick up '{itemId}' x{itemCount} on '{gameObject.name}'. Inventory bootstrap missing from scene. Pickup left in world.");
                return;
            }

            bool added;
            try
            {
                added = inventory.AddItem(itemId, itemCount);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Pickup] InventorySystem.AddItem threw {ex.GetType().Name}: {ex.Message} (itemId='{itemId}', count={itemCount}, pickup='{gameObject.name}'). Pickup left in world.");
                return;
            }

            if (added)
            {
                _pickedUp = true;
                Debug.Log($"[Pickup] {interactor.name} picked up {itemCount}x {itemId}");

                // Play pickup SFX + VFX
                try
                {
                    Audio.AudioManager.Instance?.PlaySFX2D("pickup");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Pickup] AudioManager.PlaySFX2D threw {ex.GetType().Name}: {ex.Message} (clip='pickup'). Item already added, continuing.");
                }
                // TODO: Instantiate(pickupVFX, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Pickup] Inventory full, couldn't pick up {itemCount}x {itemId} on '{gameObject.name}'. Pickup left in world for retry.");
                // TODO: Show "Inventory Full" UI message
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            // Use the actual InventorySystem API (IsFull) — GetAllItems doesn't exist
            return !_pickedUp && (InventorySystem.Instance == null || !InventorySystem.Instance.IsFull());
        }
    }
}
