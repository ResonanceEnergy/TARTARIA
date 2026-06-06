using UnityEngine;
using System;

namespace Tartaria.AI
{
    /// <summary>
    /// Enemy health management component.
    /// Tracks HP, handles damage/healing, fires death events.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float currentHealth = 100f;

        public event Action OnDeath;
        public event Action<float> OnHealthChanged;
        public event Action<float> OnDamageTaken;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0f;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth);
        }

        void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"[EnemyHealth] {gameObject.name} died.");
        }

        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
}
