using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.AI;
using System.Collections;

namespace Tartaria.Integration
{
    /// <summary>
    /// MudGolemEnemy - Complete AI + Combat Implementation
    /// Corruption-formed enemy with patrol, chase, attack behaviors.
    /// Per 15_MVP_BUILD_SPEC.md + 02_AETHER_ENERGY_SYSTEM.md.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class MudGolemEnemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("AI Behavior")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float patrolRadius = 10f;
        [SerializeField] private Vector3 spawnPosition;

        [Header("References")]
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform player;

        private MudGolemState _currentState = MudGolemState.Patrol;
        private Vector3 _patrolTarget;
        private float _lastAttackTime;
        private bool _isDead = false;

        void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;
            spawnPosition = transform.position;
        }

        void Start()
        {
            navAgent.speed = moveSpeed;
            SetNewPatrolTarget();

            // Find player
            if (player == null)
            {
                var spawner = PlayerSpawner.Instance;
                if (spawner != null && spawner.IsPlayerSpawned())
                    player = spawner.GetPlayer().transform;
            }

            Debug.Log($"[MudGolem] Spawned at {spawnPosition}");
        }

        void Update()
        {
            if (_isDead) return;

            float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : 999f;

            switch (_currentState)
            {
                case MudGolemState.Patrol:
                    UpdatePatrol(distanceToPlayer);
                    break;
                case MudGolemState.Chase:
                    UpdateChase(distanceToPlayer);
                    break;
                case MudGolemState.Attack:
                    UpdateAttack(distanceToPlayer);
                    break;
            }

            UpdateAnimations();
        }

        void UpdatePatrol(float distanceToPlayer)
        {
            // Check if player in range
            if (distanceToPlayer < detectionRange)
            {
                _currentState = MudGolemState.Chase;
                Debug.Log("[MudGolem] Player detected! Chasing...");
                return;
            }

            // Patrol between random points
            if (Vector3.Distance(transform.position, _patrolTarget) < 1f)
            {
                SetNewPatrolTarget();
            }

            navAgent.SetDestination(_patrolTarget);
        }

        void UpdateChase(float distanceToPlayer)
        {
            // Lost player?
            if (distanceToPlayer > detectionRange * 1.5f)
            {
                _currentState = MudGolemState.Patrol;
                Debug.Log("[MudGolem] Lost player. Returning to patrol.");
                return;
            }

            // In attack range?
            if (distanceToPlayer < attackRange)
            {
                _currentState = MudGolemState.Attack;
                navAgent.isStopped = true;
                return;
            }

            // Chase player
            if (player != null)
                navAgent.SetDestination(player.position);
        }

        void UpdateAttack(float distanceToPlayer)
        {
            // Player escaped?
            if (distanceToPlayer > attackRange * 1.5f)
            {
                _currentState = MudGolemState.Chase;
                navAgent.isStopped = false;
                return;
            }

            // Face player
            if (player != null)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }

            // Attack cooldown
            if (Time.time - _lastAttackTime > attackCooldown)
            {
                Attack();
                _lastAttackTime = Time.time;
            }
        }

        void UpdateAnimations()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", navAgent.velocity.magnitude);
            animator.SetBool("IsAttacking", _currentState == MudGolemState.Attack);
            animator.SetBool("IsDead", _isDead);
        }

        void SetNewPatrolTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            _patrolTarget = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        void Attack()
        {
            if (player == null) return;

            Debug.Log($"[MudGolem] ATTACK! Damage: {attackDamage}");

            // Damage player
            var playerHealth = player.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            // VFX + Audio
            VFXWiringController.Instance?.SpawnHitImpact(transform.position + transform.forward);
            AudioFeedbackController.Instance?.PlayHit(transform.position);

            // Camera shake
            CameraShakeController.Instance?.Shake(0.3f, 0.2f);
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            currentHealth -= damage;
            Debug.Log($"[MudGolem] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

            // VFX
            VFXWiringController.Instance?.SpawnHitImpact(transform.position + Vector3.up);
            AudioFeedbackController.Instance?.PlayHit(transform.position);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            _isDead = true;
            navAgent.isStopped = true;
            navAgent.enabled = false;

            Debug.Log("[MudGolem] DIED!");

            // Fire event
            GameEvents.OnEnemyKilled?.Invoke(new EnemyKilledEventArgs
            {
                enemyType = "MudGolem",
                position = transform.position,
                xpReward = 50
            });

            // VFX + Audio
            VFXWiringController.Instance?.SpawnDeathBurst(transform.position);
            AudioFeedbackController.Instance?.PlaySFX("EnemyDeath", transform.position);

            // Destroy after animation
            Destroy(gameObject, 2f);
        }

        void OnDrawGizmosSelected()
        {
            // Detection range (yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range (red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Patrol radius (green)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition != Vector3.zero ? spawnPosition : transform.position, patrolRadius);
        }
    }

    public enum MudGolemState
    {
        Patrol,
        Chase,
        Attack
    }
}
