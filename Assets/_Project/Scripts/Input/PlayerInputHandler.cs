using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

#if ENABLE_INPUT_SYSTEM
using Pointer = UnityEngine.InputSystem.Pointer;
#endif

namespace Tartaria.Input
{
    /// <summary>
    /// Player Input Handler — processes keyboard/mouse/gamepad input
    /// and dispatches to the appropriate game system based on GameState.
    /// Uses Unity Input System for cross-platform support.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        public static PlayerInputHandler Instance { get; private set; }

        [Header("Input")]
        [SerializeField, Tooltip("Unity Input System action asset")] InputActionAsset inputActions;

        [Header("Movement")]
        [SerializeField, Min(0.1f), Tooltip("Base movement speed (m/s)")] float moveSpeed = 6.0f;
        [SerializeField, Min(1f), Tooltip("Multiplier applied while sprinting")] float sprintMultiplier = 1.6f;
        [SerializeField, Min(0f), Tooltip("Character turn speed (degrees/sec)")] float rotationSpeed = 720f;
        [SerializeField, Range(-50f, -1f), Tooltip("Gravity acceleration applied each frame")] float gravity = -20f;

        [Header("Interaction")]
        [SerializeField, Min(0.1f), Tooltip("Max distance to interact with objects")] float interactRadius = 3.0f;
        [SerializeField, Tooltip("Layers that can receive interactions")] LayerMask interactableLayer;
        [SerializeField, Tooltip("Layers that identify enemies for combat")] LayerMask enemyLayer;

        CharacterController _controller;
        Camera _mainCamera;
        Vector3 _velocity;
        Vector2 _moveInput;
        bool _isSprinting;

        // Input actions (bound from InputActionAsset)
        InputAction _moveAction;
        InputAction _sprintAction;
        InputAction _interactAction;
        InputAction _attackAction;
        InputAction _shieldAction;
        InputAction _harmonicStrikeAction;
        InputAction _aetherVisionAction;
        InputAction _pauseAction;

        InputActionMap _playerMap;

        public Vector3 MoveDirection { get; private set; }
        public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
        public bool AetherVisionActive { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            SetupInputActions();
        }

        void OnDisable()
        {
            CleanupInputActions();
        }

        void SetupInputActions()
        {
            if (inputActions == null) return;

            _playerMap = inputActions.FindActionMap("Player");
            if (_playerMap == null) return;

            _moveAction = _playerMap.FindAction("Move");
            _sprintAction = _playerMap.FindAction("Sprint");
            _interactAction = _playerMap.FindAction("Interact");
            _attackAction = _playerMap.FindAction("ResonancePulse");
            _harmonicStrikeAction = _playerMap.FindAction("HarmonicStrike");
            _shieldAction = _playerMap.FindAction("FrequencyShield");
            _aetherVisionAction = _playerMap.FindAction("AetherVision");
            _pauseAction = _playerMap.FindAction("Pause");

            // Subscribe to button callbacks
            if (_interactAction != null) _interactAction.performed += OnInteractPerformed;
            if (_aetherVisionAction != null) _aetherVisionAction.performed += OnAetherVisionPerformed;
            if (_pauseAction != null) _pauseAction.performed += OnPausePerformed;
            if (_attackAction != null) _attackAction.performed += OnResonancePulsePerformed;
            if (_harmonicStrikeAction != null) _harmonicStrikeAction.performed += OnHarmonicStrikePerformed;
            if (_shieldAction != null) _shieldAction.performed += OnFrequencyShieldPerformed;

            _playerMap.Enable();
        }

        void CleanupInputActions()
        {
            if (_interactAction != null) _interactAction.performed -= OnInteractPerformed;
            if (_aetherVisionAction != null) _aetherVisionAction.performed -= OnAetherVisionPerformed;
            if (_pauseAction != null) _pauseAction.performed -= OnPausePerformed;
            if (_attackAction != null) _attackAction.performed -= OnResonancePulsePerformed;
            if (_harmonicStrikeAction != null) _harmonicStrikeAction.performed -= OnHarmonicStrikePerformed;
            if (_shieldAction != null) _shieldAction.performed -= OnFrequencyShieldPerformed;

            _playerMap?.Disable();
        }

        void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            HandleMovementInput();

        }

        void HandleMovementInput()
        {
            // Read from Input System actions
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            _isSprinting = _sprintAction != null && _sprintAction.IsPressed();

            // Refresh camera ref if lost (zone transition, cutscene swap)
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            // Camera-relative movement
            if (_mainCamera != null && _moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 camForward = _mainCamera.transform.forward;
                Vector3 camRight = _mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                MoveDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

                float speed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
                _controller.Move(MoveDirection * speed * Time.deltaTime);

                // Rotate toward movement direction
                if (MoveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            // Gravity
            if (_controller.isGrounded && _velocity.y < 0)
                _velocity.y = -2f;

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        // ─── Input Action Callbacks ──────────────────

        void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            if (GameStateManager.Instance.CurrentState == GameState.Combat)
            {
                // Left-click in combat = Resonance Pulse
                OnResonancePulse?.Invoke();
            }
            else
            {
                TryInteract();
            }
        }

        void OnAetherVisionPerformed(InputAction.CallbackContext ctx)
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;
            AetherVisionActive = !AetherVisionActive;
            GameEvents.FireToggleAetherVision();
        }

        void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            GameEvents.FireTogglePause();
        }

        void OnResonancePulsePerformed(InputAction.CallbackContext ctx)
        {
            if (GameStateManager.Instance?.CurrentState == GameState.Combat)
                OnResonancePulse?.Invoke();
        }

        void OnFrequencyShieldPerformed(InputAction.CallbackContext ctx)
        {
            if (GameStateManager.Instance?.CurrentState == GameState.Combat)
                OnFrequencyShield?.Invoke();
        }

        void OnHarmonicStrikePerformed(InputAction.CallbackContext ctx)
        {
            if (GameStateManager.Instance?.CurrentState == GameState.Combat)
                OnHarmonicStrike?.Invoke();
        }

        void TryInteract()
        {
            if (_mainCamera == null) return;

            Vector2 pointerPos = Pointer.current != null
                ? Pointer.current.position.ReadValue()
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = _mainCamera.ScreenPointToRay(pointerPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float dist = Vector3.Distance(transform.position, hit.point);
                    if (dist <= interactRadius)
                    {
                        interactable.Interact(gameObject);
                    }
                }
            }
        }

        // Combat events -- consumed by CombatController
        public event System.Action OnResonancePulse;
        public event System.Action OnHarmonicStrike;
        public event System.Action OnFrequencyShield;
    }

    /// <summary>
    /// Interface for interactable objects in the world.
    /// Buildings, tuning nodes, and NPCs implement this.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject player);
        string GetInteractPrompt();
    }
}
