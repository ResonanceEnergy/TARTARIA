using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// ResetScoutEnemy - Agent enemy with ranged attacks.
    /// More dangerous than Mud Golems.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ResetScoutEnemy : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 150f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float attackRange = 10f;
        [SerializeField] private float moveSpeed = 3.5f;

        private NavMeshAgent _navAgent;
        private Transform _player;
        private bool _isDead = false;

        void Start()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _navAgent.speed = moveSpeed;
            currentHealth = maxHealth;
            
            var spawner = PlayerSpawner.Instance;
            if (spawner != null && spawner.IsPlayerSpawned())
                _player = spawner.GetPlayer().transform;
        }

        void Update()
        {
            if (_isDead || _player == null) return;

            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance < attackRange)
            {
                RangedAttack();
            }
            else
            {
                _navAgent.SetDestination(_player.position);
            }
        }

        void RangedAttack()
        {
            Debug.Log("[ResetScout] Ranged attack!");
            var playerHealth = _player.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            currentHealth -= damage;
            if (currentHealth <= 0) Die();
        }

        void Die()
        {
            _isDead = true;
            GameEvents.OnEnemyKilled?.Invoke(new EnemyKilledEventArgs
            {
                enemyType = "ResetScout",
                position = transform.position,
                xpReward = 75
            });
            Destroy(gameObject, 2f);
        }
    }
}
