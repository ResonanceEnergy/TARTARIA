using UnityEngine;
using Tartaria.Input;
// NOTE: Cannot use 'using Tartaria.AI;' - would create circular dependency (AI depends on Gameplay)
using Tartaria.Audio;
using Tartaria.Core;

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

        [Header("Harmonic Strike (AoE)")]
        [SerializeField] private float harmonicStrikeDamage = 50f;
        [SerializeField] private float harmonicStrikeRadius = 5f;
        [SerializeField] private float harmonicStrikeCooldown = 3f;
        [SerializeField] private float harmonicStrikeAetherCost = 25f;

        [Header("Frequency Shield")]
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private float shieldAbsorption = 0.5f; // 50% damage reduction
        [SerializeField] private float shieldCooldown = 8f;
        [SerializeField] private float shieldAetherCost = 30f;

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

        private float _harmonicStrikeCooldownTimer;
        private bool _harmonicStrikeReady => _harmonicStrikeCooldownTimer <= 0f;

        private float _shieldCooldownTimer;
        private float _shieldEndTime;
        private bool _shieldReady => _shieldCooldownTimer <= 0f;
        private bool _shieldActive => Time.time < _shieldEndTime;

        // Pre-allocated buffer for sphere overlap (performance)
        private readonly Collider[] _aoeHitBuffer = new Collider[32];

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
                PlayerInputHandler.Instance.OnHarmonicStrike += HandleHarmonicStrike;
                PlayerInputHandler.Instance.OnFrequencyShield += HandleFrequencyShield;
            }
        }

        void OnDisable()
        {
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnResonancePulse -= HandleResonancePulse;
                PlayerInputHandler.Instance.OnHarmonicStrike -= HandleHarmonicStrike;
                PlayerInputHandler.Instance.OnFrequencyShield -= HandleFrequencyShield;
            }
        }

        void Update()
        {
            // Update cooldown timers
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (_harmonicStrikeCooldownTimer > 0f)
                _harmonicStrikeCooldownTimer -= Time.deltaTime;

            if (_shieldCooldownTimer > 0f)
                _shieldCooldownTimer -= Time.deltaTime;
        }

        void HandleResonancePulse()
        {
            TryAttack();
        }

        void HandleHarmonicStrike()
        {
            if (!_harmonicStrikeReady)
            {
                Debug.Log($"[PlayerCombat] Harmonic Strike on cooldown ({_harmonicStrikeCooldownTimer:F1}s remaining)");
                return;
            }

            // Check aether cost
            var aetherMgr = AetherFieldManager.Instance;
            if (aetherMgr == null || !aetherMgr.CanSpendAetherCharge(harmonicStrikeAetherCost))
            {
                Debug.Log($"[PlayerCombat] Harmonic Strike failed -- insufficient aether (need {harmonicStrikeAetherCost}, have {aetherMgr?.AetherCharge ?? 0f})");
                return;
            }

            // Deduct aether cost
            aetherMgr.DeductAetherCharge(harmonicStrikeAetherCost);

            // Start cooldown
            _harmonicStrikeCooldownTimer = harmonicStrikeCooldown;

            // AoE damage in sphere around player
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            int colCount = Physics.OverlapSphereNonAlloc(origin, harmonicStrikeRadius, _aoeHitBuffer, enemyLayer, QueryTriggerInteraction.Collide);

            int hitCount = 0;
            float damageMultiplier = _progression != null ? _progression.MeleeDamageMultiplier : 1f;
            float totalDamage = harmonicStrikeDamage * damageMultiplier;

            for (int i = 0; i < colCount; i++)
            {
                var c = _aoeHitBuffer[i];
                if (c == null || c.transform.IsChildOf(transform) || c.transform == transform) continue;

                // Apply AoE damage
                c.SendMessage("TakeDamage", totalDamage, SendMessageOptions.DontRequireReceiver);
                c.SendMessage("TakeDamage", (int)totalDamage, SendMessageOptions.DontRequireReceiver);

                // Spawn VFX at hit location
                SpawnHitVFX(c.transform.position, Vector3.up);

                hitCount++;
            }

            // Audio and feedback
            AudioManager.Instance?.PlaySFX("HarmonicStrike", origin, 0.8f);

            // Spawn shockwave VFX
            HarmonicStrikeVFX.Spawn(origin, harmonicStrikeRadius);

            Debug.Log($"[PlayerCombat] Harmonic Strike: {hitCount} enemies hit for {totalDamage:F1} damage (radius: {harmonicStrikeRadius}m)");
        }

        void HandleFrequencyShield()
        {
            if (!_shieldReady)
            {
                Debug.Log($"[PlayerCombat] Frequency Shield on cooldown ({_shieldCooldownTimer:F1}s remaining)");
                return;
            }

            // Check aether cost
            var aetherMgr = AetherFieldManager.Instance;
            if (aetherMgr == null || !aetherMgr.CanSpendAetherCharge(shieldAetherCost))
            {
                Debug.Log($"[PlayerCombat] Frequency Shield failed -- insufficient aether (need {shieldAetherCost}, have {aetherMgr?.AetherCharge ?? 0f})");
                return;
            }

            // Deduct aether cost
            aetherMgr.DeductAetherCharge(shieldAetherCost);

            // Start cooldown and activate shield
            _shieldCooldownTimer = shieldCooldown;
            _shieldEndTime = Time.time + shieldDuration;

            // Audio and feedback
            AudioManager.Instance?.PlaySFX2D("FrequencyShield", 0.8f);

            // Spawn shield VFX
            FrequencyShieldVFX.Spawn(transform, 1.5f, shieldDuration);

            Debug.Log($"[PlayerCombat] Frequency Shield activated for {shieldDuration}s ({shieldAbsorption * 100}% absorption)");
        }

        /// <summary>
        /// Returns the damage absorption multiplier if shield is active (0.0-1.0).
        /// PlayerHealth should call this to reduce incoming damage.
        /// </summary>
        public float GetShieldAbsorption()
        {
            return _shieldActive ? shieldAbsorption : 0f;
        }

        /// <summary>
        /// Returns true if Frequency Shield is currently active.
        /// </summary>
        public bool IsShieldActive() => _shieldActive;

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

                // Calculate damage
                float damageMultiplier = _progression != null ? _progression.MeleeDamageMultiplier : 1f;
                float totalDamage = baseDamage * damageMultiplier;

                // Deal damage to enemy using SendMessage to avoid circular dependency
                // MudGolemHealth.TakeDamage(float damage, GameObject instigator = null)
                // SendMessage only supports single parameter, so we pass just damage
                hit.collider.SendMessage("TakeDamage", totalDamage, SendMessageOptions.DontRequireReceiver);

                // Spawn hit VFX
                SpawnHitVFX(hit.point, hit.normal);

                // Play hit sound
                AudioManager.Instance?.PlaySFX3D(hitSoundName, hit.point, 0.7f);

                Debug.Log($"[PlayerCombat] Hit {hit.collider.name} for {totalDamage:F1} damage (base: {baseDamage}, multiplier: {damageMultiplier:F2})");
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
