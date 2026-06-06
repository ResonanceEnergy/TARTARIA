using UnityEngine;
using UnityEngine.AI;
using Tartaria.Gameplay.Combat;

namespace Tartaria.AI
{
    /// <summary>
    /// Enemy AI Controller — chase/attack player behavior for combat enemies.
    /// Extends NPCAIBehavior with hostile actions. Attach to enemy GameObjects.
    /// Integrates with MudGolemHealth and CombatHitReactor.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAIController : MonoBehaviour
    {
        [Header("Combat Config")]
        [SerializeField] float detectionRadius = 15f;
        [SerializeField] float attackRange = 2.5f;
        [SerializeField] float attackCooldown = 1.5f;
        [SerializeField] float attackDamage = 20f;
        [SerializeField] float chaseSpeed = 4f;
        [SerializeField] float wanderSpeed = 1.5f;

        [Header("Behavior")]
        [SerializeField] bool startHostile = false;
        [SerializeField] float losePlayerTimeout = 5f;

        NavMeshAgent _agent;
        Transform _player;
        float _attackTimer;
        float _losePlayerTimer;
        EnemyState _state = EnemyState.Idle;

        enum EnemyState
        {
            Idle,
            Chasing,
            Attacking,
            Retreating
        }

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = wanderSpeed;
        }

        void Start()
        {
            // Find player
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                _player = playerGO.transform;
            }

            if (startHostile)
            {
                _state = EnemyState.Chasing;
            }
        }

        void Update()
        {
            if (_player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            switch (_state)
            {
                case EnemyState.Idle:
                    // Check for player in detection radius
                    if (distanceToPlayer <= detectionRadius)
                    {
                        EnterChaseState();
                    }
                    break;

                case EnemyState.Chasing:
                    // Chase player
                    _agent.SetDestination(_player.position);

                    if (distanceToPlayer <= attackRange)
                    {
                        EnterAttackState();
                    }
                    else if (distanceToPlayer > detectionRadius * 1.5f)
                    {
                        // Player escaped
                        _losePlayerTimer += Time.deltaTime;
                        if (_losePlayerTimer >= losePlayerTimeout)
                        {
                            EnterIdleState();
                        }
                    }
                    else
                    {
                        _losePlayerTimer = 0f;
                    }
                    break;

                case EnemyState.Attacking:
                    // Face player
                    Vector3 lookDir = (_player.position - transform.position);
                    lookDir.y = 0f;
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
                    }

                    // Attack cooldown
                    _attackTimer -= Time.deltaTime;
                    if (_attackTimer <= 0f)
                    {
                        PerformAttack();
                        _attackTimer = attackCooldown;
                    }

                    // Check if player moved out of range
                    if (distanceToPlayer > attackRange * 1.2f)
                    {
                        EnterChaseState();
                    }
                    break;

                case EnemyState.Retreating:
                    // Note: Enemies don't retreat (corrupted constructs, fight to death by design)
                    break;
            }
        }

        void EnterIdleState()
        {
            _state = EnemyState.Idle;
            _agent.speed = wanderSpeed;
            _agent.ResetPath();
            _losePlayerTimer = 0f;
            Debug.Log($"[EnemyAI] {gameObject.name} → Idle");
        }

        void EnterChaseState()
        {
            _state = EnemyState.Chasing;
            _agent.speed = chaseSpeed;
            Debug.Log($"[EnemyAI] {gameObject.name} → Chasing player");
        }

        void EnterAttackState()
        {
            _state = EnemyState.Attacking;
            _agent.ResetPath();
            _attackTimer = 0f;  // Attack immediately on first enter
            Debug.Log($"[EnemyAI] {gameObject.name} → Attacking");
        }

        void PerformAttack()
        {
            if (_player == null) return;

            // Deal damage to player (SendMessage pattern - AI↔Gameplay circular dep broken)
            _player.SendMessage("TakeDamage", Mathf.RoundToInt(attackDamage), SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[EnemyAI] {gameObject.name} attacked player for {attackDamage} damage");

            // Sprint 7 Lane 7: HitFeedback feedback (popup + hitstop + shake)
            try { HitFeedback.NotifyHit(_player.position, attackDamage, false); }
            catch (System.NullReferenceException) { Debug.LogWarning("[HitCallSite] HitFeedback not initialized at EnemyAIController.cs:PerformAttack"); }

            // Play attack SFX (fully qualified to avoid assembly dependency)
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("EnemyAttack", transform.position, 0.5f);

            // Trigger attack animation
            var animator = GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        public void SetHostile(bool hostile)
        {
            if (hostile && _state == EnemyState.Idle)
            {
                EnterChaseState();
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw detection radius
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw attack range
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        /// <summary>
        /// Apply freeze debuff - stops enemy movement for duration.
        /// </summary>
        public void ApplyFreeze(float duration)
        {
            if (_agent != null && _agent.enabled)
            {
                StopAllCoroutines();
                StartCoroutine(FreezeCoroutine(duration));
                Debug.Log($"[EnemyAI] {name} frozen for {duration}s");
            }
        }

        System.Collections.IEnumerator FreezeCoroutine(float duration)
        {
            float originalSpeed = _agent.speed;
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;

            // TODO: Add visual VFX (ice particles, blue tint shader)

            yield return new WaitForSeconds(duration);

            if (_agent != null && _agent.enabled)
            {
                _agent.isStopped = false;
                _agent.speed = originalSpeed;
                Debug.Log($"[EnemyAI] {name} unfrozen");
            }
        }
    }
}
