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

        public string GetInteractionPrompt()
        {
            return $"[E] Pick up {displayName}";
        }

        public void Interact(GameObject interactor)
        {
            if (_pickedUp) return;

            bool added = InventorySystem.Instance.Add(itemId, itemCount);
            if (added)
            {
                _pickedUp = true;
                Debug.Log($"[Pickup] {interactor.name} picked up {itemCount}x {itemId}");

                // Play pickup SFX + VFX
                Audio.AudioManager.Instance?.PlaySFX2D("pickup");
                // TODO: Instantiate(pickupVFX, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Pickup] Inventory full, couldn't pick up {itemId}");
                // TODO: Show "Inventory Full" UI message
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_pickedUp && InventorySystem.Instance.GetAllItems().Count < InventorySystem.MaxSlots;
        }
    }
}
