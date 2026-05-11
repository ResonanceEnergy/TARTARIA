using UnityEngine;

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
        Vector3 _spawnPosition;
        Quaternion _spawnRotation;
        bool _spawnRecorded;

        public event System.Action<int, int> OnHealthChanged; // current, max
        public event System.Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;

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
            if (_isDead) return;

            _currentHealth -= amount;
            _lastDamageTime = Time.time;

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
