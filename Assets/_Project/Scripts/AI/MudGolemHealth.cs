using UnityEngine;
using System;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Health component for Mud Golem enemies (Moon 3 derailment ambush, Moon 4 guardian boss).
    /// Provides max health, damage handling, death events, and visual feedback integration.
    /// Public API: TakeDamage, Heal, SetMaxHealth, Kill, ResetHealth.
    /// Updated: Added SetMaxHealth method for Moon 7 siege golem spawning.
    /// Force recompile: 2026-05-22 (assembly dependency fix)
    /// </summary>
    public class MudGolemHealth : MonoBehaviour
    {
        [Header("Health Configuration")]
        // 2026-05-31: docs/15 §4 sets Mud Golem HP at 100. EchohavenContentSpawner overrides
        // via SetMaxHealth(100, true) for safety, but this default matches the spec too.
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        [Header("Damage Feedback")]
        [SerializeField] private Color _damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float _damageFlashDuration = 0.15f;

        [Header("Death Configuration")]
        [SerializeField] private bool _dropLootOnDeath = true;
        [SerializeField] private bool _destroyOnDeath = true;
        [SerializeField] private float _deathDestroyDelay = 3f;

        // Public properties
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float HealthPercent => _currentHealth / _maxHealth;
        public bool IsAlive => _currentHealth > 0f;

        // Events
        public event Action<float, float> OnDamaged; // (damageAmount, remainingHealth)
        public event Action OnDeath;
        public event Action<float> OnHealthChanged; // (newHealthPercent)

        // Static fanout for arena/encounter listeners
        public static event System.Action<MudGolemHealth> OnAnyGolemDied;

        // Cached components
        private Renderer _renderer;
        private Color _originalColor;
        private float _flashTimer;

        void Awake()
        {
            _currentHealth = _maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                // 2026-06-01 [ai] mudgolem-cleanup: read from sharedMaterial to
                // avoid the per-instance clone that r.material.color would trigger.
                var sm = _renderer.sharedMaterial;
                if (sm.HasProperty("_BaseColor")) _originalColor = sm.GetColor("_BaseColor");
                else _originalColor = sm.color;
            }
        }

        void Update()
        {
            // Handle damage flash fade
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && _renderer != null)
                {
                    SetRendererColor(_renderer, _originalColor);
                }
            }
        }

        /// <summary>
        /// Apply damage to the golem. Triggers damage flash, events, and death if health reaches zero.
        /// </summary>
        public void TakeDamage(float damage, GameObject instigator = null)
        {
            if (!IsAlive) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            Debug.Log($"[MudGolemHealth] {gameObject.name} took {damage} damage ({_currentHealth}/{_maxHealth} HP remaining)");

            // Visual feedback
            if (_renderer != null)
            {
                SetRendererColor(_renderer, _damageFlashColor);
                _flashTimer = _damageFlashDuration;
            }

            // Fire events
            OnDamaged?.Invoke(damage, _currentHealth);
            OnHealthChanged?.Invoke(HealthPercent);

            // Check for death
            if (_currentHealth <= 0f && previousHealth > 0f)
            {
                Die(instigator);
            }
        }

        /// <summary>
        /// Instantly heal the golem by the specified amount (clamped to max health).
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(HealthPercent);
            Debug.Log($"[MudGolemHealth] {gameObject.name} healed {amount} HP ({_currentHealth}/{_maxHealth})");
        }

        /// <summary>
        /// Set max health and optionally restore current health to new max.
        /// </summary>
        public void SetMaxHealth(float newMax, bool restoreToMax = false)
        {
            _maxHealth = newMax;
            if (restoreToMax)
            {
                _currentHealth = _maxHealth;
                OnHealthChanged?.Invoke(1f);
            }
            else
            {
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
                OnHealthChanged?.Invoke(HealthPercent);
            }
        }

        /// <summary>
        /// Trigger death sequence: fire event, drop loot, destroy after delay.
        /// </summary>
        private void Die(GameObject killer)
        {
            Debug.Log($"[MudGolemHealth] {gameObject.name} defeated by {(killer != null ? killer.name : "unknown")}");

            // Fire death event (Moon content spawners subscribe to this)
            OnDeath?.Invoke();
            OnAnyGolemDied?.Invoke(this);

            // R42 — also call the visible LootDrop spawner (cubes + RS coin pickup)
            // so the player sees physical loot pop out, not just an invisible inventory grant.
            if (_dropLootOnDeath)
            {
                var lootDrop = GetComponent<MudGolemLootDrop>();
                if (lootDrop != null) lootDrop.DropLoot(killer);
            }

            // Drop loot (random aether shards or moon-specific materials)
            int lootCount = 0;
            string lootItem = "aether_shard";
            if (_dropLootOnDeath)
            {
                lootCount = UnityEngine.Random.Range(1, 4); // 1-3 shards

                // Add to player inventory via InventoryManager
                var inventoryManager = Tartaria.Gameplay.InventoryManager.Instance;
                if (inventoryManager != null)
                {
                    bool added = inventoryManager.AddItem(lootItem, lootCount);
                    Debug.Log($"[MudGolemHealth] {gameObject.name} dropped {lootCount}x {lootItem} {(added ? "(added to inventory)" : "(inventory full)")}");
                }
                else
                {
                    Debug.LogWarning($"[MudGolemHealth] InventoryManager not found, loot lost: {lootCount}x {lootItem}");
                }
            }

            // Fire GameEvents for enemy killed (decoupled pub/sub)
            Core.GameEvents.RaiseEnemyKilled(new Core.EnemyKilledEventArgs
            {
                enemyType = "mud_golem",
                xpReward = 25,  // Base XP for golem
                lootItemId = lootItem,
                lootCount = lootCount,
                position = transform.position,
                killedBy = killer
            });

            // Disable AI/movement components
            var aiController = GetComponent<MonoBehaviour>(); // Generic AI controller check
            if (aiController != null)
            {
                aiController.enabled = false;
            }

            // Play death VFX and audio
            Tartaria.Audio.AudioManager.Instance?.PlaySFX2D("golem_death");
            Tartaria.Audio.HapticBridge.Call("PlayGolemDeath"); // Tartaria.AI doesn't ref Tartaria.Input — use bridge
            Tartaria.Core.ParticleEffectPool.Instance?.Spawn("GolemDeathExplosion", transform.position, Quaternion.identity, 2f);

            // Play death animation if Animator exists
            var animator = GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Death");
            }

            // Destroy after delay
            if (_destroyOnDeath)
            {
                Destroy(gameObject, _deathDestroyDelay);
            }
        }

        /// <summary>
        /// Instantly kill the golem (bypass damage, trigger death immediately).
        /// </summary>
        public void Kill(GameObject killer = null)
        {
            if (!IsAlive) return;

            _currentHealth = 0f;
            Die(killer);
        }

        /// <summary>
        /// Reset health to max (for respawn/pooling scenarios).
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(1f);

            if (_renderer != null)
            {
                SetRendererColor(_renderer, _originalColor);
            }

            _flashTimer = 0f;
        }

        // 2026-05-30 playtest fix: URP requires _BaseColor; .color silently
        // misses on URP/Lit. This helper picks the right property.
        // 2026-06-01 [ai] mudgolem-cleanup: route through MaterialBank so we set
        // colors via a shared MaterialPropertyBlock instead of cloning the
        // material per-renderer on first .material access (was producing
        // "Suboptimal memory type used for buffer" warnings in URP under wave loads).
        static void SetRendererColor(Renderer r, Color c)
        {
            MaterialBank.ApplyColor(r, c);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            _maxHealth = Mathf.Max(1f, _maxHealth);
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
        }

        void OnDrawGizmosSelected()
        {
            // Draw health bar above golem in Scene view
            if (!IsAlive) return;

            Vector3 barPos = transform.position + Vector3.up * 3f;
            Vector3 barSize = new Vector3(2f, 0.2f, 0.05f);

            // Background bar
            Gizmos.color = Color.red;
            Gizmos.DrawCube(barPos, barSize);

            // Foreground bar (filled to health percent)
            Gizmos.color = Color.green;
            Vector3 fgSize = new Vector3(barSize.x * HealthPercent, barSize.y, barSize.z);
            Vector3 fgPos = barPos - new Vector3((barSize.x - fgSize.x) * 0.5f, 0f, 0f);
            Gizmos.DrawCube(fgPos, fgSize);
        }
#endif
    }
}
