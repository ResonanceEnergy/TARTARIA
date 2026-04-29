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

        public event System.Action<int, int> OnHealthChanged; // current, max
        public event System.Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;

        void Awake()
        {
            _currentHealth = maxHealth;
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

            // TODO: Trigger death screen, respawn flow, or reload checkpoint
        }

        public void Respawn()
        {
            _currentHealth = maxHealth;
            _isDead = false;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            Debug.Log("[PlayerHealth] Player respawned");
        }
    }
}
