using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.Gameplay;
using System.Collections;

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
        MaterialPropertyBlock _propBlock;
        Renderer[] _renderers;
        CharacterController _controller;
        Vector3 _knockbackVelocity;

        enum GolemState { Patrol, Chase, Attack, Dead }

        void Awake()
        {
            _currentHealth = maxHealth;
            _spawnPosition = transform.position;
            _agent = GetComponent<NavMeshAgent>();
            _controller = GetComponent<CharacterController>();
            _propBlock = new MaterialPropertyBlock();
            _renderers = GetComponentsInChildren<Renderer>();

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

        // ─── Procedural visual builder ───────────────
        /// <summary>
        /// Builds a fully-formed Mud Golem GameObject from primitives:
        /// torso, head, two arms, two legs, glowing eyes, and a muddy material.
        /// Adds NavMeshAgent + CapsuleCollider + Rigidbody + MudGolemAI so the
        /// returned object is gameplay-ready. Used by both the runtime spawn
        /// fallback and the editor RuntimeSetupWizard.
        /// </summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("MudGolem");
            root.transform.SetPositionAndRotation(position, rotation);

            // Materials
            var mudShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mudMat = new Material(mudShader) { name = "MudGolem_Body" };
            mudMat.color = new Color(0.32f, 0.24f, 0.16f);
            if (mudMat.HasProperty("_Smoothness")) mudMat.SetFloat("_Smoothness", 0.15f);
            if (mudMat.HasProperty("_Metallic"))   mudMat.SetFloat("_Metallic", 0.05f);

            var eyeMat = new Material(mudShader) { name = "MudGolem_Eye" };
            eyeMat.color = new Color(1.0f, 0.45f, 0.10f);
            if (eyeMat.HasProperty("_EmissionColor"))
            {
                eyeMat.EnableKeyword("_EMISSION");
                eyeMat.SetColor("_EmissionColor", new Color(2.4f, 1.0f, 0.2f));
            }

            // Torso (squashed sphere)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            torso.transform.localScale = new Vector3(1.4f, 1.6f, 1.0f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = mudMat;
            Object.Destroy(torso.GetComponent<Collider>());

            // Head (cube)
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            head.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            head.GetComponent<MeshRenderer>().sharedMaterial = mudMat;
            Object.Destroy(head.GetComponent<Collider>());

            // Eyes
            for (int i = 0; i < 2; i++)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = i == 0 ? "Eye_L" : "Eye_R";
                eye.transform.SetParent(head.transform, false);
                eye.transform.localPosition = new Vector3(i == 0 ? -0.22f : 0.22f, 0.05f, -0.46f);
                eye.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                eye.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;
                Object.Destroy(eye.GetComponent<Collider>());
            }

            // Arms (cylinders)
            for (int i = 0; i < 2; i++)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = i == 0 ? "Arm_L" : "Arm_R";
                arm.transform.SetParent(root.transform, false);
                arm.transform.localPosition = new Vector3(i == 0 ? -0.95f : 0.95f, 1.0f, 0f);
                arm.transform.localScale = new Vector3(0.32f, 0.7f, 0.32f);
                arm.GetComponent<MeshRenderer>().sharedMaterial = mudMat;
                Object.Destroy(arm.GetComponent<Collider>());

                // Fist
                var fist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fist.name = "Fist";
                fist.transform.SetParent(arm.transform, false);
                fist.transform.localPosition = new Vector3(0f, -1.05f, 0f);
                fist.transform.localScale = new Vector3(1.6f, 0.7f, 1.6f);
                fist.GetComponent<MeshRenderer>().sharedMaterial = mudMat;
                Object.Destroy(fist.GetComponent<Collider>());
            }

            // Legs (cubes)
            for (int i = 0; i < 2; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = i == 0 ? "Leg_L" : "Leg_R";
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = new Vector3(i == 0 ? -0.4f : 0.4f, 0.0f, 0f);
                leg.transform.localScale = new Vector3(0.55f, 1.0f, 0.55f);
                leg.GetComponent<MeshRenderer>().sharedMaterial = mudMat;
                Object.Destroy(leg.GetComponent<Collider>());
            }

            // Gameplay components
            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = 2.6f;
            collider.radius = 0.85f;
            collider.center = new Vector3(0f, 1.0f, 0f);

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var agent = root.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 240f;
            agent.acceleration = 10f;
            agent.stoppingDistance = 2.5f;
            agent.radius = 0.85f;
            agent.height = 2.6f;

            // Layer
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            root.layer = enemyLayer >= 0 ? enemyLayer : 12;
            root.tag = "Enemy";

            root.AddComponent<MudGolemAI>();
            return root;
        }

        void Update()
        {
            if (_state == GolemState.Dead) return;

            // Sprint Batch 2: Apply knockback velocity
            if (_knockbackVelocity.sqrMagnitude > 0.01f)
            {
                if (_controller != null)
                    _controller.Move(_knockbackVelocity * Time.deltaTime);
                _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
            }

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

            // Sprint: Spawn damage number
            DamageNumberPool.Spawn(damage, transform.position);

            // Sprint: Hit-flash (white emission for 0.08s)
            StartCoroutine(HitFlash());

            // Sprint Batch 2: Apply knockback from player direction
            if (_player != null)
            {
                Vector3 dir = (transform.position - _player.position).normalized;
                OnHit(dir, damage * 0.1f);
            }

            if (_currentHealth <= 0)
                Die();
        }

        /// <summary>Sprint Batch 2: Knockback on hit</summary>
        public void OnHit(Vector3 direction, float force)
        {
            _knockbackVelocity = direction * force;
            Debug.Log($"[MudGolem] Knockback applied: {force:F1}");
        }

        IEnumerator HitFlash()
        {
            // Set white emission
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_EmissionColor", Color.white * 2f);
                r.SetPropertyBlock(_propBlock);
            }

            yield return new WaitForSeconds(0.08f);

            // Restore original emission
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_EmissionColor", Color.black);
                r.SetPropertyBlock(_propBlock);
            }
        }

        void Die()
        {
            TransitionTo(GolemState.Dead);

            if (_hasNavMesh && _agent != null)
                _agent.enabled = false;

            // Sprint Batch 2: Ragdoll on death
            EnableRagdoll();

            // Death handled by GameLoopController.OnEnemyDefeated via GameEvents
            // Drop loot, VFX, SFX all handled there

            // Drop Aether shard
            if (aetherShardPrefab != null)
            {
                Instantiate(aetherShardPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            // Award RS via GameEvents
            GameEvents.FireRSChange(5f);

            // Notify combat arena / moon activator wave-tracking via the
            // MudGolemHealth bridge (Integration assembly) using SendMessage
            // to avoid an asmdef cycle (AI must NOT reference Integration).
            SendMessage("KillFromAI", SendMessageOptions.DontRequireReceiver);

            // Destroy after 4s (longer for ragdoll to settle)
            Destroy(gameObject, 4f);

            Debug.Log("[MudGolem] Dead");
        }

        /// <summary>Sprint Batch 2: Enable ragdoll on death</summary>
        void EnableRagdoll()
        {
            // Disable Animator and CharacterController
            var animator = GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            if (_controller != null) _controller.enabled = false;

            // Enable Rigidbody and apply death impulse
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
                
                // Death impulse (backward from player)
                if (_player != null)
                {
                    Vector3 dir = (transform.position - _player.position).normalized;
                    rb.AddForce(dir * 5f + Vector3.up * 3f, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                }
            }

            Debug.Log("[MudGolem] Ragdoll enabled");
        }
    }
}
