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
        [SerializeField] GameObject pickupVFX;     // Optional VFX prefab; null → runtime ParticleSystem fallback
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
            return $"[E] Pick up {displayName}";
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
                // GAP #1 fix: spawn pickup VFX. If pickupVFX prefab is assigned, instantiate it.
                // Otherwise build a URP-safe runtime ParticleSystem burst (same pattern as BuildingSpawner.cs:259-279 RestoreSparkle fallback).
                SpawnPickupVFX(transform.position);

                Destroy(gameObject);
            }
            else
            {
                // GAP #2 fix: replace silent warning with player-facing HUD banner via ServiceLocator.
                // Debug.LogWarning kept as a fallback diagnostic so headless/test runs still see the event.
                string warnMsg = $"Inventory full, couldn't pick up {itemCount}x {itemId} on '{gameObject.name}'. Pickup left in world for retry.";
                Debug.LogWarning($"[Pickup] {warnMsg}");
                try
                {
                    var hud = ServiceLocator.HUD;
                    if (hud != null)
                    {
                        hud.ShowBanner("Inventory Full", $"Couldn't pick up {displayName}", 2.5f);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Pickup] ServiceLocator.HUD.ShowBanner threw {ex.GetType().Name}: {ex.Message}. Banner skipped, warning still logged.");
                }
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            // Use the actual InventorySystem API (IsFull) — GetAllItems doesn't exist
            return !_pickedUp && (InventorySystem.Instance == null || !InventorySystem.Instance.IsFull());
        }

        // GAP #1 helper: spawn pickup VFX. Assigned prefab → Instantiate + auto-destroy at 2s.
        // No prefab → runtime ParticleSystem (URP-safe) parented to a transient GameObject, auto-destroyed at 2s.
        void SpawnPickupVFX(Vector3 position)
        {
            try
            {
                if (pickupVFX != null)
                {
                    var go = Instantiate(pickupVFX, position, Quaternion.identity);
                    Destroy(go, 2f);
                    return;
                }

                // Fallback: runtime ParticleSystem burst (URP-safe). Pattern from BuildingSpawner.cs:259-279.
                var marker = new GameObject($"PickupVFX_Fallback_{itemId}");
                marker.transform.position = position;

                var ps = marker.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.duration = 1.0f;
                main.startLifetime = 1.2f;
                main.startSpeed = 1.5f;
                main.startSize = 0.18f;
                main.startColor = new Color(0.6f, 0.95f, 1f, 0.9f);
                main.maxParticles = 40;
                main.loop = false;

                var emission = ps.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.25f;

                var renderer = marker.GetComponent<ParticleSystemRenderer>();
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader != null)
                {
                    var mat = new Material(shader);     // URP-safe
                    mat.SetColor("_BaseColor", new Color(0.6f, 0.95f, 1f));
                    renderer.material = mat;
                }

                ps.Play();
                Destroy(marker, 2f);
            }
            catch (System.Exception ex)
            {
                // Safety guard: VFX must NEVER break the pickup happy path.
                Debug.LogWarning($"[Pickup] SpawnPickupVFX threw {ex.GetType().Name}: {ex.Message}. Item already added, continuing.");
            }
        }
    }
}
