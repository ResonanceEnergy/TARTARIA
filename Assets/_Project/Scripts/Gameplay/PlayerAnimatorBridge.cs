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
        static readonly int IsSprintingId = Animator.StringToHash("IsSprinting");
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

            // Speed parameter: 0..1 normalized to sprint max
            float speed = 0f;
            bool isSprinting = false;
            if (inputHandler != null && inputHandler.IsMoving)
            {
                isSprinting = PlayerStamina.Instance != null && PlayerStamina.Instance.IsSprinting;
                speed = isSprinting ? 1.5f : 1f;
            }
            _animator.SetFloat(SpeedId, speed);
            _animator.SetBool(IsSprintingId, isSprinting);

            bool isGrounded = characterController != null ? characterController.isGrounded : true;
            _animator.SetBool(IsGroundedId, isGrounded);

            if (characterController != null && characterController.velocity.y > 1f)
                _animator.SetTrigger(JumpId);
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null || !_animator.isHuman) return;

            // 2-bone foot IK: raycast below feet, adjust IK goals
            // Left foot
            Vector3 leftFootPos = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            if (Physics.Raycast(leftFootPos + Vector3.up * 0.5f, Vector3.down, out RaycastHit leftHit, 1.0f))
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.8f);
                _animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftHit.point);
            }

            // Right foot
            Vector3 rightFootPos = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
            if (Physics.Raycast(rightFootPos + Vector3.up * 0.5f, Vector3.down, out RaycastHit rightHit, 1.0f))
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.8f);
                _animator.SetIKPosition(AvatarIKGoal.RightFoot, rightHit.point);
            }
        }
    }
}
