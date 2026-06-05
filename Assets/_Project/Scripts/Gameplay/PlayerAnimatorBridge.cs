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

        // Cached parameter existence flags — Animator.SetX with a missing parameter logs an error every frame.
        bool _hasSpeed, _hasIsGrounded, _hasIsSprinting, _hasJump, _hasAttack;

        void Awake()
        {
            _animator = GetComponent<Animator>();

            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            CacheAnimatorParams();
        }

        void CacheAnimatorParams()
        {
            _hasSpeed = _hasIsGrounded = _hasIsSprinting = _hasJump = _hasAttack = false;
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            foreach (var p in _animator.parameters)
            {
                if (p.nameHash == SpeedId) _hasSpeed = true;
                else if (p.nameHash == IsGroundedId) _hasIsGrounded = true;
                else if (p.nameHash == IsSprintingId) _hasIsSprinting = true;
                else if (p.nameHash == JumpId) _hasJump = true;
                else if (p.nameHash == AttackId) _hasAttack = true;
            }
        }

        void OnEnable()  { /* PlayerCombat.OnSwing += HandleSwing; */ } // PlayerCombat disabled (Phase 12)
        void OnDisable() { /* PlayerCombat.OnSwing -= HandleSwing; */ }

        void HandleSwing()
        {
            if (_animator != null && _hasAttack) _animator.SetTrigger(AttackId);
        }

        void Update()
        {
            if (_animator == null) return;

            // Speed parameter: 0..1 normalized to sprint max
            float speed = 0f;
            bool isSprinting = false;
            if (inputHandler != null && inputHandler.IsMoving)
            {
                // PlayerStamina disabled (Phase 12)
                // isSprinting = PlayerStamina.Instance != null && PlayerStamina.Instance.IsSprinting;
                speed = isSprinting ? 1.5f : 1f;
            }
            if (_hasSpeed) _animator.SetFloat(SpeedId, speed);
            if (_hasIsSprinting) _animator.SetBool(IsSprintingId, isSprinting);

            bool isGrounded = characterController != null ? characterController.isGrounded : true;
            if (_hasIsGrounded) _animator.SetBool(IsGroundedId, isGrounded);

            if (_hasJump && characterController != null && characterController.velocity.y > 1f)
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
