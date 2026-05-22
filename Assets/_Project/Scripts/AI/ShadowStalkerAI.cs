using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Shadow Stalker AI — stealth ambush enemy for Moons 4-6.
    /// Behavior: Stalks player from shadows, invisible until close, quick melee burst attacks.
    /// Combat design: Punishes inattention, rewards spatial awareness.
    /// Difficulty: Medium (HP: 200, Damage: 30, Speed: Fast)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ShadowStalkerAI : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] float maxHealth = 200f;
        [SerializeField] float attackDamage = 30f;
        [SerializeField] float attackRange = 2.2f;
        [SerializeField] float attackCooldown = 1.2f;

        [Header("Stealth Mechanics")]
        [SerializeField] float stealthRadius = 8f;  // invisible beyond this distance
        [SerializeField] float ambushRange = 3.5f;  // triggers ambush attack
        [SerializeField] float revealDuration = 2f; // stays visible after hitting

        [Header("Movement")]
        [SerializeField] float stalkSpeed = 5f;
        [SerializeField] float chaseSpeed = 7f;

        NavMeshAgent _agent;
        Transform _player;
        float _currentHealth;
        float _attackTimer;
        float _revealTimer;
        bool _isRevealed;
        Renderer[] _renderers;
        Color _originalColor;

        enum StalkerState { Stalking, Ambushing, Revealed, Dead }
        StalkerState _state = StalkerState.Stalking;

        void Awake()
        {
            _currentHealth = maxHealth;
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = stalkSpeed;
            _renderers = GetComponentsInChildren<Renderer>();
            if (_renderers.Length > 0)
                _originalColor = _renderers[0].material.color;
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;
        }

        void Update()
        {
            if (_state == StalkerState.Dead || _player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // Update stealth visibility
            bool shouldBeVisible = distanceToPlayer <= stealthRadius || _isRevealed;
            UpdateVisibility(shouldBeVisible);

            _attackTimer -= Time.deltaTime;

            switch (_state)
            {
                case StalkerState.Stalking:
                    // Circle around player, staying just outside ambush range
                    Vector3 stalkTarget = _player.position + (transform.position - _player.position).normalized * (ambushRange + 2f);
                    _agent.SetDestination(stalkTarget);
                    _agent.speed = stalkSpeed;

                    // Trigger ambush when close enough
                    if (distanceToPlayer <= ambushRange)
                    {
                        _state = StalkerState.Ambushing;
                        _agent.speed = chaseSpeed;
                        Debug.Log("[ShadowStalker] Ambush triggered!");
                    }
                    break;

                case StalkerState.Ambushing:
                    // Rush player for quick strikes
                    _agent.SetDestination(_player.position);

                    if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
                    {
                        PerformAmbushAttack();
                        _attackTimer = attackCooldown;
                        _state = StalkerState.Revealed;
                        _isRevealed = true;
                        _revealTimer = revealDuration;
                    }
                    break;

                case StalkerState.Revealed:
                    // Continue attacking while visible
                    _agent.SetDestination(_player.position);

                    if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
                    {
                        PerformAttack();
                        _attackTimer = attackCooldown;
                    }

                    // Return to stealth after reveal duration
                    _revealTimer -= Time.deltaTime;
                    if (_revealTimer <= 0f)
                    {
                        _isRevealed = false;
                        _state = StalkerState.Stalking;
                        _agent.speed = stalkSpeed;
                        Debug.Log("[ShadowStalker] Returning to stealth...");
                    }
                    break;
            }
        }

        void UpdateVisibility(bool visible)
        {
            foreach (var rend in _renderers)
            {
                if (rend == null) continue;
                Color c = rend.material.color;
                c.a = visible ? 1f : 0.2f;
                rend.material.color = c;
            }
        }

        void PerformAmbushAttack()
        {
            // Ambush deals extra damage
            float ambushDamage = attackDamage * 1.5f;
            DamagePlayer(ambushDamage);
            VFXEventSystem.RequestVFX(VFXEffect.Spark, transform.position);
            Debug.Log($"[ShadowStalker] Ambush attack! {ambushDamage} damage");
        }

        void PerformAttack()
        {
            DamagePlayer(attackDamage);
            VFXEventSystem.RequestVFX(VFXEffect.Spark, transform.position);
        }

        void DamagePlayer(float damage)
        {
            var playerHealth = _player?.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(damage));
            }
        }

        public void TakeDamage(float damage)
        {
            if (_state == StalkerState.Dead) return;

            _currentHealth -= damage;

            // Force reveal when hit
            _isRevealed = true;
            _revealTimer = revealDuration;
            _state = StalkerState.Revealed;

            // Visual feedback
            foreach (var rend in _renderers)
            {
                if (rend != null)
                    rend.material.color = Color.red;
            }
            Invoke(nameof(ResetColor), 0.15f);

            if (_currentHealth <= 0f)
            {
                Die();
            }

            Debug.Log($"[ShadowStalker] Took {damage} damage, HP: {_currentHealth}/{maxHealth}");
        }

        void ResetColor()
        {
            foreach (var rend in _renderers)
            {
                if (rend != null)
                    rend.material.color = _originalColor;
            }
        }

        void Die()
        {
            _state = StalkerState.Dead;
            _agent.isStopped = true;
            Debug.Log("[ShadowStalker] Defeated");
            
            // Drop loot
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem("Aether Shard", 1);
            }

            Destroy(gameObject, 2f);
        }

        /// <summary>Procedurally build a Shadow Stalker at runtime.</summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("ShadowStalker");
            root.transform.SetPositionAndRotation(position, rotation);

            // Dark sleek body
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
            var mat = body.GetComponent<Renderer>().material;
            mat.color = new Color(0.1f, 0.08f, 0.12f, 0.8f); // dark purple translucent

            // Add components
            root.AddComponent<NavMeshAgent>();
            root.AddComponent<CapsuleCollider>().radius = 0.5f;
            var ai = root.AddComponent<ShadowStalkerAI>();

            return root;
        }
    }
}
