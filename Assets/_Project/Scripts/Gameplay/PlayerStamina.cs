using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Stamina system with sprint (LeftShift / GamepadLeftStickButton) and
    /// dodge-roll (Space / GamepadSouth). Auto-attached by CharacterPrefabFactory.
    /// Reads & modifies the rig's CharacterController velocity through a small
    /// dodge impulse coroutine; sprint state is exposed via IsSprinting for the
    /// movement layer to multiply its base speed by 1.5x.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStamina : MonoBehaviour
    {
        public static PlayerStamina Instance { get; private set; }

        [Header("Stamina")]
        [SerializeField] float maxStamina = 100f;
        [SerializeField] float regenPerSecond = 18f;
        [SerializeField] float regenDelay = 0.8f;
        [SerializeField] float sprintCostPerSecond = 22f;
        [SerializeField] float dodgeCost = 30f;

        [Header("Dodge")]
        [SerializeField] float dodgeSpeed = 12f;
        [SerializeField] float dodgeDuration = 0.35f;
        [SerializeField] float dodgeCooldown = 0.6f;

        public float Stamina { get; private set; }
        public float MaxStamina => maxStamina;
        public bool IsSprinting { get; private set; }
        public bool IsDodging { get; private set; }

        public event System.Action<float, float> OnStaminaChanged; // current, max

        float _lastDrainTime;
        float _lastDodgeTime;
        Vector3 _dodgeDir;
        float _dodgeUntil;
        CharacterController _cc;

        void Awake()
        {
            Instance = this;
            Stamina = maxStamina;
            _cc = GetComponent<CharacterController>();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Update()
        {
            float dt = Time.deltaTime;
            var kb = Keyboard.current;
            var pad = Gamepad.current;

            // Sprint (held).
            bool wantSprint =
                (kb != null && kb.leftShiftKey.isPressed) ||
                (pad != null && pad.leftStickButton.isPressed);
            IsSprinting = wantSprint && Stamina > 0f && IsMoving();
            if (IsSprinting)
            {
                Stamina = Mathf.Max(0f, Stamina - sprintCostPerSecond * dt);
                _lastDrainTime = Time.time;
                if (Stamina <= 0f) IsSprinting = false;
            }

            // Dodge (tap).
            bool wantDodge =
                (kb != null && kb.spaceKey.wasPressedThisFrame) ||
                (pad != null && pad.buttonSouth.wasPressedThisFrame);
            if (wantDodge && Stamina >= dodgeCost && Time.time - _lastDodgeTime >= dodgeCooldown)
                StartDodge();

            // Tick dodge.
            if (IsDodging)
            {
                if (Time.time >= _dodgeUntil) IsDodging = false;
                else if (_cc != null)
                {
                    var move = _dodgeDir * dodgeSpeed * dt;
                    move.y -= 9.81f * dt;
                    _cc.Move(move);
                }
            }

            // Regen.
            if (!IsSprinting && Time.time - _lastDrainTime >= regenDelay && Stamina < maxStamina)
            {
                Stamina = Mathf.Min(maxStamina, Stamina + regenPerSecond * dt);
            }

            OnStaminaChanged?.Invoke(Stamina, maxStamina);
        }

        bool IsMoving()
        {
            if (_cc == null) return false;
            var v = _cc.velocity;
            v.y = 0f;
            return v.sqrMagnitude > 0.04f;
        }

        void StartDodge()
        {
            Stamina -= dodgeCost;
            _lastDrainTime = Time.time;
            _lastDodgeTime = Time.time;
            IsDodging = true;
            _dodgeUntil = Time.time + dodgeDuration;
            // Direction: forward unless camera-relative input is moving us elsewhere.
            _dodgeDir = transform.forward;
            if (_cc != null)
            {
                var v = _cc.velocity;
                v.y = 0f;
                if (v.sqrMagnitude > 0.04f) _dodgeDir = v.normalized;
            }
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("Dodge", transform.position);
        }
    }
}
