using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player Animation Controller — drives player animation state based on movement and actions.
    /// Auto-wires Animator component to PlayerAnimatorController asset.
    /// Updates animation parameters: Speed, IsGrounded, IsJumping, Attack trigger.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] Animator animator;
        [SerializeField] string animatorControllerPath = "Animations/PlayerAnimatorController";

        [Header("Movement Parameters")]
        [SerializeField] string speedParam = "Speed";
        [SerializeField] string isGroundedParam = "IsGrounded";
        [SerializeField] string isJumpingParam = "IsJumping";
        [SerializeField] string attackTrigger = "Attack";

        CharacterController _characterController;
        PlayerCombat _playerCombat;

        // Cached animator parameter hashes
        int _speedHash;
        int _isGroundedHash;
        int _isJumpingHash;
        int _attackHash;

        // Animation state
        float _currentSpeed;
        bool _wasSwinging;

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerCombat = GetComponent<PlayerCombat>();

            // Find or create Animator
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                // Try to find in children (visual mesh)
                var child = transform.Find("PlayerMesh");
                if (child != null)
                    animator = child.GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogWarning("[PlayerAnimation] No Animator found on player or children");
                enabled = false;
                return;
            }

            // Load animator controller if not set
            if (animator.runtimeAnimatorController == null)
            {
                var controller = Resources.Load<RuntimeAnimatorController>(animatorControllerPath);
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log($"[PlayerAnimation] Loaded animator controller: {animatorControllerPath}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerAnimation] Failed to load animator controller at {animatorControllerPath}");
                }
            }

            // Cache parameter hashes
            _speedHash = Animator.StringToHash(speedParam);
            _isGroundedHash = Animator.StringToHash(isGroundedParam);
            _isJumpingHash = Animator.StringToHash(isJumpingParam);
            _attackHash = Animator.StringToHash(attackTrigger);
        }

        void OnEnable()
        {
            if (_playerCombat != null)
                PlayerCombat.OnSwing += HandleAttackAnimation;
        }

        void OnDisable()
        {
            if (_playerCombat != null)
                PlayerCombat.OnSwing -= HandleAttackAnimation;
        }

        void Update()
        {
            if (animator == null) return;

            UpdateMovementAnimation();
            UpdateGroundedState();
        }

        void UpdateMovementAnimation()
        {
            if (_characterController == null) return;

            // Calculate horizontal movement speed
            Vector3 velocity = _characterController.velocity;
            velocity.y = 0f; // Ignore vertical component
            float speed = velocity.magnitude;

            // Smooth speed for animation blending
            _currentSpeed = Mathf.Lerp(_currentSpeed, speed, Time.deltaTime * 10f);

            // Update animator
            animator.SetFloat(_speedHash, _currentSpeed);
        }

        void UpdateGroundedState()
        {
            if (_characterController == null) return;

            bool isGrounded = _characterController.isGrounded;
            animator.SetBool(_isGroundedHash, isGrounded);

            // Simple jump detection (velocity upward while not grounded)
            bool isJumping = !isGrounded && _characterController.velocity.y > 0.1f;
            animator.SetBool(_isJumpingHash, isJumping);
        }

        void HandleAttackAnimation()
        {
            if (animator == null) return;
            animator.SetTrigger(_attackHash);
        }

        // ─── Animation Event Receivers (called from animation clips) ───

        /// <summary>
        /// Called from attack animation clip at the moment of impact.
        /// Animation clip should have an Animation Event calling this method.
        /// </summary>
        public void OnAttackHit()
        {
            var combat = GetComponent<PlayerCombatController>();
            if (combat != null)
            {
                combat.TryAttack();
                Debug.Log("[PlayerAnimation] Attack hit frame triggered");
            }
        }

        /// <summary>
        /// Called from Harmonic Strike animation at the moment of AoE explosion.
        /// </summary>
        public void OnHarmonicStrikeHit()
        {
            Debug.Log("[PlayerAnimation] Harmonic Strike hit frame triggered (animation event)");
            // HarmonicStrike is triggered immediately on input, so this is just for VFX timing
        }

        /// <summary>
        /// Called when attack animation completes.
        /// </summary>
        public void OnAttackEnd()
        {
            Debug.Log("[PlayerAnimation] Attack animation completed");
        }

        void OnValidate()
        {
            // Auto-find Animator in children if not set
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
    }
}
