using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Bridges PlayerInputHandler to Animator component for Capoeira animations.
    /// Updates Animator parameters based on player movement and actions.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerInputHandler inputHandler;
        [SerializeField] CharacterController characterController;

        Animator _animator;

        // Animator parameter IDs (cached for performance)
        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
        static readonly int JumpId = Animator.StringToHash("Jump");
        static readonly int AttackId = Animator.StringToHash("Attack");

        void Awake()
        {
            _animator = GetComponent<Animator>();

            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        void OnEnable()  { PlayerCombat.OnSwing += HandleSwing; }
        void OnDisable() { PlayerCombat.OnSwing -= HandleSwing; }

        void HandleSwing()
        {
            if (_animator != null) _animator.SetTrigger(AttackId);
        }

        void Update()
        {
            if (_animator == null) return;

            // Day-? sprint blends with stamina; use 1.5 multiplier when sprinting.
            float speed = 0f;
            if (inputHandler != null && inputHandler.IsMoving)
                speed = (PlayerStamina.Instance != null && PlayerStamina.Instance.IsSprinting) ? 1.5f : 1f;
            _animator.SetFloat(SpeedId, speed);

            bool isGrounded = characterController != null ? characterController.isGrounded : true;
            _animator.SetBool(IsGroundedId, isGrounded);

            if (characterController != null && characterController.velocity.y > 1f)
                _animator.SetTrigger(JumpId);
        }
    }
}
