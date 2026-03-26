using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

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
        [Header("Movement")]
        [SerializeField] float moveSpeed = 6.0f;
        [SerializeField] float sprintMultiplier = 1.6f;
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float gravity = -20f;

        [Header("Interaction")]
        [SerializeField] float interactRadius = 3.0f;
        [SerializeField] LayerMask interactableLayer;
        [SerializeField] LayerMask enemyLayer;

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
        InputAction _aetherVisionAction;
        InputAction _pauseAction;

        public Vector3 MoveDirection { get; private set; }
        public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
        public bool AetherVisionActive { get; private set; }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
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
            // These will be populated from the InputActionAsset in Unity Editor.
            // For now, use direct keyboard polling as fallback.
        }

        void CleanupInputActions()
        {
            _moveAction?.Disable();
            _sprintAction?.Disable();
            _interactAction?.Disable();
            _attackAction?.Disable();
            _shieldAction?.Disable();
            _aetherVisionAction?.Disable();
            _pauseAction?.Disable();
        }

        void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            HandleMovementInput();
            HandleInteractionInput();

            if (GameStateManager.Instance.CurrentState == GameState.Combat)
                HandleCombatInput();
        }

        void HandleMovementInput()
        {
            // WASD / Left Stick
            _moveInput = new Vector2(
                UnityEngine.Input.GetAxisRaw("Horizontal"),
                UnityEngine.Input.GetAxisRaw("Vertical")
            );
            _isSprinting = UnityEngine.Input.GetKey(KeyCode.LeftShift);

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

        void HandleInteractionInput()
        {
            // Left-click = interact
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryInteract();
            }

            // Tab = toggle Aether vision
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                AetherVisionActive = !AetherVisionActive;
            }

            // Escape = pause
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameStateManager.Instance.IsPaused)
                    GameStateManager.Instance.ReturnToPrevious();
                else
                    GameStateManager.Instance.TransitionTo(GameState.Paused);
            }
        }

        void HandleCombatInput()
        {
            // Left-click in combat = Resonance Pulse
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnResonancePulse?.Invoke();
            }

            // Right-click = Harmonic Strike
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                OnHarmonicStrike?.Invoke();
            }

            // Ctrl / Left trigger = Frequency Shield
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl))
            {
                OnFrequencyShield?.Invoke();
            }
        }

        void TryInteract()
        {
            // Raycast from mouse position
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
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

        // Combat events — consumed by CombatController
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
