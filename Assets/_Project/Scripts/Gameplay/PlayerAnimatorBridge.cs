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

            // Auto-find references if not assigned
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (_animator == null || inputHandler == null)
                return;

            // Update Speed parameter (0 = idle, 1 = walking)
            float speed = inputHandler.IsMoving ? 1f : 0f;
            _animator.SetFloat(SpeedId, speed);

            // Update IsGrounded
            bool isGrounded = characterController != null ? characterController.isGrounded : true;
            _animator.SetBool(IsGroundedId, isGrounded);

            // Trigger Jump (if input system has jump input in the future)
            // For now, check vertical velocity
            if (characterController != null && characterController.velocity.y > 1f)
            {
                _animator.SetTrigger(JumpId);
            }

            // Attack trigger (placeholder - wire to combat system later)
            if (UnityEngine.Input.GetKeyDown(KeyCode.Mouse0) || UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                _animator.SetTrigger(AttackId);
            }
        }
    }
}
