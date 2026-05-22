using UnityEngine;
using Tartaria.Input;
using Tartaria.AI;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Handles player melee combat — raycast-based attacks triggered by OnResonancePulse.
    /// Deals damage to MudGolemHealth enemies, spawns hit VFX, plays audio, respects cooldown.
    /// Integrates with PlayerProgression for damage multipliers.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float baseDamage = 20f;
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float attackCooldown = 0.8f;
        [SerializeField] private LayerMask enemyLayer = ~0;
        [SerializeField] private float coneAngle = 45f;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;
        [SerializeField] private string hitVFXResourcePath = "Prefabs/VFX/HitImpact";

        [Header("Audio")]
        [SerializeField] private string attackSoundName = "Player_MeleeSwing";
        [SerializeField] private string hitSoundName = "Player_MeleeHit";

        // Cached components
        private CharacterController _characterController;
        private Camera _mainCamera;
        private PlayerProgression _progression;

        // State
        private float _cooldownTimer;
        private bool _isReady => _cooldownTimer <= 0f;

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
            _progression = GetComponent<PlayerProgression>();

            // Try to load VFX prefab from Resources if not assigned
            if (hitVFXPrefab == null)
            {
                hitVFXPrefab = Resources.Load<GameObject>(hitVFXResourcePath);
            }
        }

        void OnEnable()
        {
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnResonancePulse += HandleResonancePulse;
            }
        }

        void OnDisable()
        {
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnResonancePulse -= HandleResonancePulse;
            }
        }

        void Update()
        {
            // Update cooldown timer
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        void HandleResonancePulse()
        {
            TryAttack();
        }

        /// <summary>
        /// Attempts to perform a melee attack if cooldown is ready.
        /// Performs raycast in forward direction, deals damage to enemies hit.
        /// </summary>
        public void TryAttack()
        {
            if (!_isReady) return;

            // Start cooldown
            _cooldownTimer = attackCooldown;

            // Play attack sound
            AudioManager.Instance?.PlaySFX2D(attackSoundName, 0.6f);

            // Determine attack direction
            Vector3 attackOrigin = transform.position + Vector3.up * 1.0f; // Chest height
            Vector3 attackDirection = GetAttackDirection();

            // Perform raycast
            if (Physics.Raycast(attackOrigin, attackDirection, out RaycastHit hit, attackRange, enemyLayer))
            {
                // Check if hit is within cone angle
                float angleToHit = Vector3.Angle(attackDirection, (hit.point - attackOrigin).normalized);
                if (angleToHit > coneAngle)
                {
                    // Outside cone, miss
                    return;
                }

                // Deal damage to enemy
                MudGolemHealth enemyHealth = hit.collider.GetComponent<MudGolemHealth>();
                if (enemyHealth != null)
                {
                    float damageMultiplier = _progression != null ? _progression.MeleeDamageMultiplier : 1f;
                    float totalDamage = baseDamage * damageMultiplier;
                    
                    enemyHealth.TakeDamage(totalDamage, gameObject);

                    // Spawn hit VFX
                    SpawnHitVFX(hit.point, hit.normal);

                    // Play hit sound
                    AudioManager.Instance?.PlaySFX3D(hitSoundName, hit.point, 0.7f);

                    Debug.Log($"[PlayerCombat] Hit {hit.collider.name} for {totalDamage:F1} damage (base: {baseDamage}, multiplier: {damageMultiplier:F2})");
                }
            }
        }

        /// <summary>
        /// Determines attack direction based on camera look or character forward.
        /// </summary>
        Vector3 GetAttackDirection()
        {
            if (_mainCamera != null)
            {
                return _mainCamera.transform.forward;
            }
            else
            {
                return transform.forward;
            }
        }

        /// <summary>
        /// Spawns hit VFX at the impact point.
        /// </summary>
        void SpawnHitVFX(Vector3 position, Vector3 normal)
        {
            if (hitVFXPrefab == null) return;

            GameObject vfx = Instantiate(hitVFXPrefab, position, Quaternion.LookRotation(normal));
            
            // Auto-destroy after 2 seconds
            Destroy(vfx, 2f);
        }

        // Debug visualization
        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Vector3 origin = transform.position + Vector3.up * 1.0f;
            Vector3 direction = GetAttackDirection();

            // Draw attack range
            Gizmos.color = _isReady ? Color.green : Color.red;
            Gizmos.DrawRay(origin, direction * attackRange);

            // Draw cone angle
            Gizmos.color = Color.yellow;
            Vector3 rightEdge = Quaternion.Euler(0, coneAngle, 0) * direction;
            Vector3 leftEdge = Quaternion.Euler(0, -coneAngle, 0) * direction;
            Gizmos.DrawRay(origin, rightEdge * attackRange);
            Gizmos.DrawRay(origin, leftEdge * attackRange);
        }
    }
}
