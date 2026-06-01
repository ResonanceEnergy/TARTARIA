using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player Health — tracks player HP, handles damage/healing,
    /// triggers death/respawn flow.
    ///
    /// Attach to Player prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] int maxHealth = 100;
        [SerializeField] float regenDelay = 5f;
        [SerializeField] int regenAmountPerSecond = 5;

        int _currentHealth;
        float _lastDamageTime;
        bool _isDead;
        bool _godMode; // Debug: invincibility
        Vector3 _spawnPosition;
        Quaternion _spawnRotation;
        bool _spawnRecorded;

        public event System.Action<int, int> OnHealthChanged; // current, max
        public event System.Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;
        public bool GodMode { get => _godMode; set => _godMode = value; }

        void Awake()
        {
            _currentHealth = maxHealth;
        }

        void Start()
        {
            // Day-14: capture spawn point for respawn-in-place.
            // (Deferred to Start so any scene-side spawner has placed us first.)
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _spawnRecorded = true;
        }

        /// <summary>Day-14: reset checkpoint to current position (call from Checkpoint trigger).</summary>
        public void SetCheckpoint(Vector3 pos, Quaternion rot)
        {
            _spawnPosition = pos;
            _spawnRotation = rot;
            _spawnRecorded = true;
        }

        void Update()
        {
            if (_isDead) return;

            // Auto-regen after delay
            if (_currentHealth < maxHealth && Time.time - _lastDamageTime >= regenDelay)
            {
                int regenThisFrame = Mathf.CeilToInt(regenAmountPerSecond * Time.deltaTime);
                Heal(regenThisFrame);
            }
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || _godMode) return;

            // SECURITY: Block negative damage (healing exploit)
            if (amount < 0)
            {
                Debug.LogWarning($"[PlayerHealth] Rejected negative damage: {amount}");
                return;
            }

            // SECURITY: Cap damage to prevent overflow
            if (amount > 10000)
            {
                Debug.LogWarning($"[PlayerHealth] Capped excessive damage: {amount} -> 10000");
                amount = 10000;
            }

            // Sprint: Check for i-frames from dodge
            var dodge = GetComponent<PlayerDodge>();
            if (dodge != null && dodge.IsInvulnerable)
            {
                Debug.Log("[PlayerHealth] Dodged! No damage taken (i-frames)");
                return;
            }

            // Check for Frequency Shield damage reduction
            var combat = GetComponent<PlayerCombatController>();
            if (combat != null && combat.IsShieldActive())
            {
                float absorption = combat.GetShieldAbsorption();
                int originalAmount = amount;
                amount = Mathf.RoundToInt(amount * (1f - absorption));
                Debug.Log($"[PlayerHealth] Frequency Shield absorbed {(absorption * 100):F0}% damage ({originalAmount} -> {amount})");
            }

            _currentHealth -= amount;
            _lastDamageTime = Time.time;

            // Audio + haptic feedback
            Audio.AudioManager.Instance?.PlaySFX3D("player_damage", transform.position);
            Input.HapticFeedbackManager.Instance?.PlayCombatHit();

            // Trigger hit reactor VFX/SFX (CombatHitReactor disabled - Phase 23)
            // var reactor = GetComponent<CombatHitReactor>();
            // if (reactor != null)
            // {
            //     reactor.OnHit(transform.position, Vector3.up);
            // }

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            Debug.Log($"[PlayerHealth] Took {amount} damage, HP={_currentHealth}/{maxHealth}");
        }

        public void Heal(int amount)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        void Die()
        {
            _isDead = true;
            OnDeath?.Invoke();

            // Audio + haptic feedback
            Audio.AudioManager.Instance?.PlaySFX3D("player_death", transform.position);
            Input.HapticFeedbackManager.Instance?.PlayGolemDeath();

            Debug.Log("[PlayerHealth] Player died");
            // Day-15: DeathOverlay subscribes via FindFirstObjectByType + OnDeath, drives Respawn().
        }

        public void Respawn()
        {
            _currentHealth = maxHealth;
            _isDead = false;
            // Day-14: teleport back to last checkpoint / scene spawn.
            if (_spawnRecorded)
            {
                var cc = GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
                if (cc != null) cc.enabled = true;
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            Debug.Log($"[PlayerHealth] Player respawned at {_spawnPosition}");
        }
    }
}
