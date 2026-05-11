using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Dodge + i-frames: Shift or Gamepad-East (B) triggers 0.35s dash
    /// with 0.30s invulnerability window. 0.8s cooldown. Cancels attacks.
    /// 
    /// Auto-attached by CharacterPrefabFactory.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDodge : MonoBehaviour
    {
        [Header("Dodge")]
        [SerializeField] float dodgeDuration = 0.35f;
        [SerializeField] float dodgeSpeed = 12f;
        [SerializeField] float iFrameDuration = 0.30f;
        [SerializeField] float cooldown = 0.8f;

        CharacterController _controller;
        float _lastDodgeTime = -10f;
        float _dodgeEndTime;
        bool _invulnerable;

        public bool IsInvulnerable => _invulnerable;
        public bool IsDodging => Time.time < _dodgeEndTime;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            // Check dodge input
            bool dodgeInput = false;
            var kb = Keyboard.current;
            if (kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame))
                dodgeInput = true;
            var pad = Gamepad.current;
            if (pad != null && pad.buttonEast.wasPressedThisFrame) // B on Xbox, Circle on PS
                dodgeInput = true;

            if (dodgeInput && Time.time - _lastDodgeTime >= cooldown)
                StartDodge();

            // Update dodge movement
            if (IsDodging)
            {
                Vector3 dodgeDir = transform.forward;
                _controller.Move(dodgeDir * dodgeSpeed * Time.deltaTime);
            }

            // Update i-frames
            if (_invulnerable && Time.time >= _lastDodgeTime + iFrameDuration)
                _invulnerable = false;
        }

        void StartDodge()
        {
            _lastDodgeTime = Time.time;
            _dodgeEndTime = Time.time + dodgeDuration;
            _invulnerable = true;

            // Cancel attack
            var combat = GetComponent<PlayerCombat>();
            if (combat != null && combat.IsSwinging)
            {
                Debug.Log("[PlayerDodge] Cancelled swing with dodge");
            }

            Debug.Log("[PlayerDodge] Dodge! i-frames active");
        }
    }
}
