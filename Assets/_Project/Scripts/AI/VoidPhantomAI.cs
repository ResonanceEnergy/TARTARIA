using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Void Phantom AI — teleporting melee enemy for Moons 7-10.
    /// Behavior: Teleports around player, unpredictable attack angles, phases out when damaged.
    /// Combat design: Tests player reaction time and spatial prediction.
    /// Difficulty: High (HP: 180, Damage: 40, Speed: Instant teleport)
    /// </summary>
    public class VoidPhantomAI : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] float maxHealth = 180f;
        [SerializeField] float attackDamage = 40f;
        [SerializeField] float attackRange = 2.5f;
        [SerializeField] float attackCooldown = 2f;

        [Header("Teleport Mechanics")]
        [SerializeField] float teleportCooldown = 3f;
        [SerializeField] float teleportRadius = 8f;     // how far to teleport
        [SerializeField] float teleportMinDistance = 4f; // minimum distance from player
        [SerializeField] float phaseOutDuration = 1f;   // invulnerable after taking damage

        Transform _player;
        float _currentHealth;
        float _attackTimer;
        float _teleportTimer;
        float _phaseOutTimer;
        bool _isPhasedOut;
        Renderer[] _renderers;
        Color _originalColor;

        enum PhantomState { Stalking, Attacking, PhasedOut, Dead }
        PhantomState _state = PhantomState.Stalking;

        void Awake()
        {
            _currentHealth = maxHealth;
            _renderers = GetComponentsInChildren<Renderer>();
            if (_renderers.Length > 0)
                _originalColor = _renderers[0].material.color;
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;

            // Start with a teleport
            _teleportTimer = 0f;
        }

        void Update()
        {
            if (_state == PhantomState.Dead || _player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            _attackTimer -= Time.deltaTime;
            _teleportTimer -= Time.deltaTime;

            // Update phase out effect
            if (_isPhasedOut)
            {
                _phaseOutTimer -= Time.deltaTime;
                if (_phaseOutTimer <= 0f)
                {
                    _isPhasedOut = false;
                    _state = PhantomState.Stalking;
                    UpdateVisibility(1f);
                    Debug.Log("[VoidPhantom] Phase-in complete");
                }
                else
                {
                    // Flicker effect
                    float alpha = 0.3f + Mathf.Sin(Time.time * 20f) * 0.2f;
                    UpdateVisibility(alpha);
                }
                return;
            }

            switch (_state)
            {
                case PhantomState.Stalking:
                    // Teleport around player
                    if (_teleportTimer <= 0f && distanceToPlayer > teleportMinDistance)
                    {
                        TeleportNearPlayer();
                        _teleportTimer = teleportCooldown;
                    }

                    // Face player
                    Vector3 lookDir = (_player.position - transform.position);
                    lookDir.y = 0f;
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);
                    }

                    // Attack if in range
                    if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
                    {
                        _state = PhantomState.Attacking;
                        PerformAttack();
                        _attackTimer = attackCooldown;
                    }
                    break;

                case PhantomState.Attacking:
                    // Return to stalking immediately
                    _state = PhantomState.Stalking;
                    break;
            }
        }

        void TeleportNearPlayer()
        {
            if (_player == null) return;

            // VFX at current position
            VFXEventSystem.RequestVFX(VFXEffect.AetherVortex, transform.position);

            // Calculate random position around player
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(teleportMinDistance, teleportRadius);
            Vector3 targetPos = _player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Ensure position is on ground
            if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                targetPos = hit.point;
            }

            transform.position = targetPos;

            // VFX at new position
            VFXEventSystem.RequestVFX(VFXEffect.AetherVortex, transform.position);
            Audio.AudioManager.Instance?.PlayTone(256f, 0.4f);

            Debug.Log($"[VoidPhantom] Teleported to {targetPos}");
        }

        void PerformAttack()
        {
            if (_player == null) return;

            var playerHealth = _player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(attackDamage));
                VFXEventSystem.RequestVFX(VFXEffect.Spark, _player.position);
                Debug.Log($"[VoidPhantom] Attack dealt {attackDamage} damage");
            }
        }

        void UpdateVisibility(float alpha)
        {
            foreach (var rend in _renderers)
            {
                if (rend == null) continue;
                Color c = rend.material.color;
                c.a = alpha;
                { var __m = rend.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", c); else __m.color = c; }
            }
        }

        public void TakeDamage(float damage)
        {
            if (_state == PhantomState.Dead || _isPhasedOut) return;

            _currentHealth -= damage;

            // Phase out on hit (temporary invulnerability)
            _isPhasedOut = true;
            _phaseOutTimer = phaseOutDuration;
            _state = PhantomState.PhasedOut;

            // Teleport away immediately
            TeleportNearPlayer();

            if (_currentHealth <= 0f)
            {
                Die();
            }
            else
            {
                Debug.Log($"[VoidPhantom] Phasing out! HP: {_currentHealth}/{maxHealth}");
            }
        }

        void Die()
        {
            _state = PhantomState.Dead;
            Debug.Log("[VoidPhantom] Defeated");
            
            VFXEventSystem.RequestVFX(VFXEffect.AetherVortex, transform.position);
            
            // Drop loot
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem("Void Essence", 1);
            }

            Destroy(gameObject, 1f);
        }

        /// <summary>Procedurally build a Void Phantom at runtime.</summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("VoidPhantom");
            root.transform.SetPositionAndRotation(position, rotation);

            // Ghostly body (sphere)
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1f, 1.3f, 1f);
            var mat = body.GetComponent<Renderer>().material;
            { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.5f, 0.2f, 0.8f, 0.7f)); else mat.color = new Color(0.5f, 0.2f, 0.8f, 0.7f); } // purple translucent

            // Add components
            root.AddComponent<SphereCollider>().radius = 0.6f;
            var ai = root.AddComponent<VoidPhantomAI>();

            return root;
        }
    }
}
