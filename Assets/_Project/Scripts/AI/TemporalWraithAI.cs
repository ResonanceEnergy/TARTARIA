using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Temporal Wraith AI — time-manipulation enemy for Moons 10-13.
    /// Behavior: Slows time around player, rewinds health, creates temporal clones.
    /// Combat design: Elite late-game enemy, requires mastery of all mechanics.
    /// Difficulty: Very High (HP: 350, Damage: 45, Speed: Variable)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class TemporalWraithAI : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] float maxHealth = 350f;
        [SerializeField] float attackDamage = 45f;
        [SerializeField] float attackRange = 3f;
        [SerializeField] float attackCooldown = 2.5f;

        [Header("Temporal Abilities")]
        [SerializeField] float timeSlowRadius = 12f;
        [SerializeField] float timeSlowStrength = 0.5f; // 50% slow
        [SerializeField] float rewindThreshold = 0.3f;  // rewinds health at 30% HP
        [SerializeField] float cloneSpawnInterval = 15f;

        [Header("Movement")]
        [SerializeField] float moveSpeed = 4.5f;
        [SerializeField] float phaseSpeed = 8f;

        NavMeshAgent _agent;
        Transform _player;
        float _currentHealth;
        float _previousHealth;
        float _attackTimer;
        float _cloneTimer;
        bool _hasRewound;
        Renderer[] _renderers;
        Color _originalColor;
        GameObject _activeClone;

        enum WraithState { Phasing, Attacking, Rewinding, Dead }
        WraithState _state = WraithState.Phasing;

        void Awake()
        {
            _currentHealth = maxHealth;
            _previousHealth = maxHealth;
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = moveSpeed;
            _renderers = GetComponentsInChildren<Renderer>();
            if (_renderers.Length > 0)
                _originalColor = _renderers[0].material.color;
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;

            _cloneTimer = cloneSpawnInterval;
        }

        void Update()
        {
            if (_state == WraithState.Dead || _player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // Continuous time slow aura
            ApplyTimeSlowAura(distanceToPlayer);

            _attackTimer -= Time.deltaTime;
            _cloneTimer -= Time.deltaTime;

            // Check for health rewind trigger
            if (!_hasRewound && (_currentHealth / maxHealth) <= rewindThreshold)
            {
                TriggerTimeRewind();
                return;
            }

            // Spawn temporal clone
            if (_cloneTimer <= 0f && _activeClone == null)
            {
                SpawnTemporalClone();
                _cloneTimer = cloneSpawnInterval;
            }

            switch (_state)
            {
                case WraithState.Phasing:
                    // Phase through obstacles toward player
                    _agent.speed = phaseSpeed;
                    _agent.SetDestination(_player.position);

                    if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
                    {
                        _state = WraithState.Attacking;
                        PerformAttack();
                        _attackTimer = attackCooldown;
                    }
                    break;

                case WraithState.Attacking:
                    // Return to phasing
                    _state = WraithState.Phasing;
                    _agent.speed = moveSpeed;
                    break;

                case WraithState.Rewinding:
                    // Locked during rewind animation
                    break;
            }

            // Store health for rewind
            _previousHealth = _currentHealth;
        }

        void ApplyTimeSlowAura(float distanceToPlayer)
        {
            if (distanceToPlayer <= timeSlowRadius)
            {
                // Visual effect every 60 frames
                if (Time.frameCount % 60 == 0)
                {
                    VFXController.Instance?.PlayEffect(VFXEffect.CorruptionPulse, transform.position);
                }

                // Note: In full implementation, this would modify Time.timeScale or player movement speed
                // For now, just visual feedback
            }
        }

        void TriggerTimeRewind()
        {
            _state = WraithState.Rewinding;
            _hasRewound = true;

            // Rewind health to previous checkpoint
            float rewindAmount = maxHealth * 0.4f; // Restore 40% max HP
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + rewindAmount);

            Debug.Log($"[TemporalWraith] TIME REWIND! Health restored to {_currentHealth}/{maxHealth}");

            // VFX
            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position);
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position + Vector3.up * 2f);
            Audio.AudioManager.Instance?.PlayTone(432f, 0.8f);

            // Return to combat after brief delay
            Invoke(nameof(EndRewind), 1.5f);
        }

        void EndRewind()
        {
            _state = WraithState.Phasing;
            Debug.Log("[TemporalWraith] Rewind complete, returning to combat");
        }

        void SpawnTemporalClone()
        {
            // Create a weaker copy that mimics behavior
            Vector3 clonePos = transform.position + Random.insideUnitSphere * 5f;
            clonePos.y = transform.position.y;

            _activeClone = new GameObject("TemporalClone");
            _activeClone.transform.position = clonePos;

            // Visual copy
            var cloneBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cloneBody.transform.SetParent(_activeClone.transform, false);
            cloneBody.transform.localScale = transform.localScale * 0.7f;
            var mat = cloneBody.GetComponent<Renderer>().material;
            mat.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0.5f); // translucent

            // Basic AI for clone
            var cloneAI = _activeClone.AddComponent<TemporalCloneAI>();
            cloneAI.Initialize(_player, attackDamage * 0.5f, 10f);

            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, clonePos);

            Debug.Log("[TemporalWraith] Spawned temporal clone");

            // Clone expires after 10 seconds
            Destroy(_activeClone, 10f);
        }

        void PerformAttack()
        {
            if (_player == null) return;

            var playerHealth = _player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(attackDamage));
                VFXController.Instance?.PlayEffect(VFXEffect.Spark, _player.position);
                Debug.Log($"[TemporalWraith] Attack dealt {attackDamage} damage");
            }
        }

        public void TakeDamage(float damage)
        {
            if (_state == WraithState.Dead || _state == WraithState.Rewinding) return;

            _currentHealth -= damage;

            // Visual feedback
            foreach (var rend in _renderers)
            {
                if (rend != null)
                    rend.material.color = Color.cyan;
            }
            Invoke(nameof(ResetColor), 0.15f);

            if (_currentHealth <= 0f)
            {
                Die();
            }

            Debug.Log($"[TemporalWraith] Took {damage} damage, HP: {_currentHealth}/{maxHealth}");
        }

        void ResetColor()
        {
            foreach (var rend in _renderers)
            {
                if (rend != null && _state != WraithState.Dead)
                    rend.material.color = _originalColor;
            }
        }

        void Die()
        {
            _state = WraithState.Dead;
            _agent.isStopped = true;
            Debug.Log("[TemporalWraith] Defeated");
            
            VFXController.Instance?.PlayEffect(VFXEffect.AetherVortex, transform.position);
            VFXController.Instance?.PlayEffect(VFXEffect.HarmonicCascade, transform.position);
            
            // Clean up clone
            if (_activeClone != null)
                Destroy(_activeClone);

            // Drop rare loot
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem("Temporal Shard", 1);
            }

            Destroy(gameObject, 2f);
        }

        /// <summary>Procedurally build a Temporal Wraith at runtime.</summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("TemporalWraith");
            root.transform.SetPositionAndRotation(position, rotation);

            // Ethereal body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1f, 1.5f, 1f);
            var mat = body.GetComponent<Renderer>().material;
            mat.color = new Color(0.2f, 0.8f, 0.9f, 0.8f); // cyan ghostly

            // Add components
            root.AddComponent<NavMeshAgent>();
            root.AddComponent<CapsuleCollider>().radius = 0.6f;
            var ai = root.AddComponent<TemporalWraithAI>();

            return root;
        }
    }

    /// <summary>Simple AI for temporal clones — chase and attack player.</summary>
    public class TemporalCloneAI : MonoBehaviour
    {
        Transform _player;
        float _damage;
        float _lifetime;
        float _attackCooldown = 2f;

        public void Initialize(Transform player, float damage, float lifetime)
        {
            _player = player;
            _damage = damage;
            _lifetime = lifetime;
        }

        void Update()
        {
            if (_player == null) return;

            // Move toward player
            Vector3 direction = (_player.position - transform.position).normalized;
            transform.position += direction * 3f * Time.deltaTime;

            // Face player
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }

            // Attack if close
            float distance = Vector3.Distance(transform.position, _player.position);
            if (distance <= 2.5f)
            {
                _attackCooldown -= Time.deltaTime;
                if (_attackCooldown <= 0f)
                {
                    var playerHealth = _player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(Mathf.RoundToInt(_damage));
                        VFXController.Instance?.PlayEffect(VFXEffect.Spark, transform.position);
                    }
                    _attackCooldown = 2f;
                }
            }

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
