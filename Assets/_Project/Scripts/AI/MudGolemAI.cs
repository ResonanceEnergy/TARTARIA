using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// Mud Golem AI — hostile enemy that spawns when Resonance Score exceeds
    /// thresholds. Patrols randomly, chases player on sight, melee attacks
    /// within range, drops Aether shard on death.
    ///
    /// States: Patrol → Chase → Attack → Dead
    ///
    /// NavMesh-aware: attempts to use NavMeshAgent for navigation. If NavMesh
    /// is not baked, falls back to CharacterController-style direct movement.
    /// </summary>
    [DisallowMultipleComponent]
    public class MudGolemAI : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] int maxHealth = 50;
        [SerializeField] int meleeDamage = 10;

        [Header("Behavior")]
        [SerializeField] float patrolRadius = 20f;
        [SerializeField] float chaseRange = 15f;
        [SerializeField] float attackRange = 3f;
        [SerializeField] float attackCooldown = 1.5f;
        [SerializeField] float patrolWaitTime = 5f;

        [Header("Movement (fallback if no NavMesh)")]
        [SerializeField] float moveSpeed = 3f;
        [SerializeField] float chaseSpeed = 5f;

        [Header("Loot")]
        [SerializeField] GameObject aetherShardPrefab;

        NavMeshAgent _agent;
        Transform _player;
        int _currentHealth;
        GolemState _state;
        float _stateEnterTime;
        float _lastAttackTime;
        Vector3 _spawnPosition;
        Vector3 _patrolTarget;
        bool _hasNavMesh;

        enum GolemState { Patrol, Chase, Attack, Dead }

        void Awake()
        {
            _currentHealth = maxHealth;
            _spawnPosition = transform.position;
            _agent = GetComponent<NavMeshAgent>();

            // Check if NavMesh is baked
            if (_agent != null && NavMesh.SamplePosition(transform.position, out _, 2f, NavMesh.AllAreas))
            {
                _hasNavMesh = true;
                _agent.speed = moveSpeed;
            }
            else
            {
                _hasNavMesh = false;
                if (_agent != null) _agent.enabled = false;
            }
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;

            TransitionTo(GolemState.Patrol);
            Debug.Log($"[MudGolem] Spawned at {transform.position}, HP={_currentHealth}, NavMesh={_hasNavMesh}");
        }

        void Update()
        {
            if (_state == GolemState.Dead) return;

            float distToPlayer = _player != null
                ? Vector3.Distance(transform.position, _player.position)
                : float.MaxValue;

            switch (_state)
            {
                case GolemState.Patrol:
                    UpdatePatrol(distToPlayer);
                    break;
                case GolemState.Chase:
                    UpdateChase(distToPlayer);
                    break;
                case GolemState.Attack:
                    UpdateAttack(distToPlayer);
                    break;
            }
        }

        // ─── State Machine ───────────────────────────

        void TransitionTo(GolemState newState)
        {
            if (_state == newState) return;

            _state = newState;
            _stateEnterTime = Time.time;

            Debug.Log($"[MudGolem] State: {newState}, HP={_currentHealth}");

            switch (newState)
            {
                case GolemState.Patrol:
                    SetNewPatrolTarget();
                    break;
                case GolemState.Chase:
                    if (_hasNavMesh) _agent.speed = chaseSpeed;
                    break;
                case GolemState.Attack:
                    if (_hasNavMesh) _agent.isStopped = true;
                    break;
            }
        }

        void UpdatePatrol(float distToPlayer)
        {
            if (distToPlayer <= chaseRange && _player != null)
            {
                TransitionTo(GolemState.Chase);
                return;
            }

            if (_hasNavMesh)
            {
                if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                {
                    // Reached patrol point, wait then pick new target
                    if (Time.time - _stateEnterTime >= patrolWaitTime)
                        SetNewPatrolTarget();
                }
            }
            else
            {
                // Fallback: walk toward patrol target
                Vector3 dir = (_patrolTarget - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.forward = dir;

                if (Vector3.Distance(transform.position, _patrolTarget) < 1f)
                {
                    if (Time.time - _stateEnterTime >= patrolWaitTime)
                        SetNewPatrolTarget();
                }
            }
        }

        void UpdateChase(float distToPlayer)
        {
            if (distToPlayer > chaseRange)
            {
                TransitionTo(GolemState.Patrol);
                return;
            }

            if (distToPlayer <= attackRange)
            {
                TransitionTo(GolemState.Attack);
                return;
            }

            if (_player == null) return;

            if (_hasNavMesh)
            {
                _agent.SetDestination(_player.position);
            }
            else
            {
                Vector3 dir = (_player.position - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * chaseSpeed * Time.deltaTime;
                transform.forward = dir;
            }
        }

        void UpdateAttack(float distToPlayer)
        {
            if (distToPlayer > attackRange)
            {
                TransitionTo(GolemState.Chase);
                return;
            }

            if (_player == null) return;

            // Face player
            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0f;
            transform.forward = lookDir;

            // Attack on cooldown
            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                _lastAttackTime = Time.time;
                PerformMeleeAttack();
            }
        }

        void SetNewPatrolTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 target = _spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (_hasNavMesh)
            {
                // Sample nearest valid NavMesh position
                if (NavMesh.SamplePosition(target, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
                {
                    _agent.SetDestination(hit.position);
                    _patrolTarget = hit.position;
                }
            }
            else
            {
                _patrolTarget = target;
            }

            _stateEnterTime = Time.time;
        }

        void PerformMeleeAttack()
        {
            // Raycast forward to check for player hit
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, attackRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // Deal damage via PlayerHealth component (assumed)
                    var health = hit.collider.GetComponent<Gameplay.PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(meleeDamage);
                        Debug.Log($"[MudGolem] Hit player for {meleeDamage} damage");
                    }

                    // SFX via GameEvents (no direct Audio dependency)
                    // VFX handled elsewhere
                }
            }
        }

        // ─── Public API ──────────────────────────────

        public void TakeDamage(int damage)
        {
            if (_state == GolemState.Dead) return;

            _currentHealth -= damage;
            Debug.Log($"[MudGolem] Took {damage} damage, HP={_currentHealth}");

            if (_currentHealth <= 0)
                Die();
        }

        void Die()
        {
            TransitionTo(GolemState.Dead);

            if (_hasNavMesh && _agent != null)
                _agent.enabled = false;

            // Death handled by GameLoopController.OnEnemyDefeated via GameEvents
            // Drop loot, VFX, SFX all handled there

            // Drop Aether shard
            if (aetherShardPrefab != null)
            {
                Instantiate(aetherShardPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            // Award RS via GameEvents
            GameEvents.FireRSChange(5f);

            // Destroy after 2s
            Destroy(gameObject, 2f);

            Debug.Log("[MudGolem] Dead");
        }
    }
}
