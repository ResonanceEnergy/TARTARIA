using UnityEngine;
using System.Collections;

namespace Tartaria.Integration
{
    /// <summary>
    /// Loot Animator — adds juice to loot drops.
    /// Features:
    /// - Hover + spin animation (sine wave bob, 90°/s rotation)
    /// - Spawn VFX (rarity-colored particle burst)
    /// - Vacuum pickup (flies to player in arc when in range)
    /// - Auto-cleanup (destroys after 60s if not picked up)
    /// 
    /// Attach to loot pickup GameObjects or use static helper methods.
    /// </summary>
    [DisallowMultipleComponent]
    public class LootAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] float hoverHeight = 0.25f;
        [SerializeField] float hoverSpeed = 2.5f;
        [SerializeField] float rotationSpeed = 90f; // degrees per second
        [SerializeField] float vacuumRadius = 2.5f;
        [SerializeField] float vacuumSpeed = 8f;
        [SerializeField] float autoDestroyTime = 60f;

        [Header("VFX")]
        [SerializeField] GameObject spawnVFX;
        [SerializeField] ParticleSystem pickupVFX;

        Vector3 _originPosition;
        float _phase;
        bool _isVacuuming;
        Transform _player;
        float _destroyTimer;

        void Start()
        {
            _originPosition = transform.position;
            _phase = Random.value * Mathf.PI * 2f; // Random phase for variety
            _destroyTimer = autoDestroyTime;

            // Find player for vacuum effect
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                _player = playerGO.transform;
            }

            // Spawn VFX on loot creation
            if (spawnVFX != null)
            {
                var vfx = Instantiate(spawnVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }

        void Update()
        {
            if (_isVacuuming)
            {
                UpdateVacuum();
            }
            else
            {
                UpdateHover();
                CheckVacuumRange();
            }

            // Auto-destroy timer
            _destroyTimer -= Time.deltaTime;
            if (_destroyTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        void UpdateHover()
        {
            // Bob up and down via sine wave
            float yOffset = hoverHeight + Mathf.Sin(Time.time * hoverSpeed + _phase) * 0.1f;
            transform.position = _originPosition + Vector3.up * yOffset;

            // Spin on Y-axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        void CheckVacuumRange()
        {
            if (_player == null) return;

            float distSq = (_player.position - transform.position).sqrMagnitude;
            if (distSq < vacuumRadius * vacuumRadius)
            {
                StartVacuum();
            }
        }

        void StartVacuum()
        {
            _isVacuuming = true;

            // Play pickup VFX
            if (pickupVFX != null)
            {
                pickupVFX.Play();
            }

            // Play audio
            Audio.AudioManager.Instance?.PlaySFX("ItemPickup", transform.position, 0.6f);
        }

        void UpdateVacuum()
        {
            if (_player == null)
            {
                Destroy(gameObject);
                return;
            }

            // Move toward player
            Vector3 direction = (_player.position + Vector3.up * 1f - transform.position).normalized;
            transform.position += direction * vacuumSpeed * Time.deltaTime;

            // Destroy when reached player
            float distSq = (_player.position - transform.position).sqrMagnitude;
            if (distSq < 0.5f * 0.5f)
            {
                OnPickedUp();
            }
        }

        void OnPickedUp()
        {
            // Trigger pickup logic (handled by PickupInteractable component)
            var pickup = GetComponent<Gameplay.PickupInteractable>();
            if (pickup != null)
            {
                pickup.Interact(_player.gameObject);
            }

            // Destroy loot visual
            Destroy(gameObject);
        }

        /// <summary>
        /// Static helper: Spawn animated loot at position.
        /// </summary>
        public static GameObject SpawnLoot(GameObject lootPrefab, Vector3 position, Color rarityColor)
        {
            if (lootPrefab == null)
            {
                Debug.LogWarning("[LootAnimator] No loot prefab provided");
                return null;
            }

            var lootGO = Instantiate(lootPrefab, position, Quaternion.identity);

            // Add animator if not already present
            if (lootGO.GetComponent<LootAnimator>() == null)
            {
                var animator = lootGO.AddComponent<LootAnimator>();
            }

            // Set rarity color
            var renderer = lootGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = rarityColor;
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.SetColor("_EmissionColor", rarityColor * 2.5f);
                }
            }

            return lootGO;
        }

        /// <summary>
        /// Static helper: Spawn loot with rarity tier (auto-colors).
        /// </summary>
        public static GameObject SpawnLootWithRarity(GameObject lootPrefab, Vector3 position, Data.ItemRarity rarity)
        {
            Color color = GetRarityColor(rarity);
            return SpawnLoot(lootPrefab, position, color);
        }

        static Color GetRarityColor(Data.ItemRarity rarity)
        {
            return rarity switch
            {
                Data.ItemRarity.Common => Color.white,
                Data.ItemRarity.Rare => new Color(0.3f, 0.6f, 1f), // Blue
                Data.ItemRarity.Epic => new Color(0.7f, 0.3f, 1f), // Purple
                Data.ItemRarity.Legendary => new Color(1f, 0.7f, 0.2f), // Gold
                Data.ItemRarity.Mythic => new Color(1f, 0.2f, 0.2f), // Red
                _ => Color.white
            };
        }
    }
}
