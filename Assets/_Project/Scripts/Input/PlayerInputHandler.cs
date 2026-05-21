using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Audio;

#if ENABLE_INPUT_SYSTEM
using Pointer = UnityEngine.InputSystem.Pointer;
#endif

namespace Tartaria.Input
{
    /// <summary>
    /// Player Input Handler — processes keyboard/mouse/gamepad input
    /// and dispatches to the appropriate game system based on GameState.
    /// Uses Unity Input System for cross-platform support.
    /// Includes keyboard fallbacks for robust playtesting without .inputactions asset.
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
        float _cameraRetryTimer;
        Vector3 _velocity;
        Vector2 _moveInput;
        bool _isSprinting;
        float _footstepTimer;
        float _groundHeight = 8f;
        bool _loggedMoveFallback;
        bool _loggedMoveActionOk;
        bool _firstMove = true;
        float _externalMoveMultiplier = 1f;

        // Input actions (bound from InputActionAsset)
        InputAction _moveAction;
        InputAction _sprintAction;
        InputAction _interactAction;
        InputAction _attackAction;
        InputAction _shieldAction;
        InputAction _harmonicStrikeAction;
        InputAction _aetherVisionAction;
        InputAction _pauseAction;
        InputAction _scanAction;
        InputAction _frequencyAdjustAction;

        InputActionMap _playerMap;
        InputActionAsset _runtimeActions;

        public Vector3 MoveDirection { get; private set; }
        public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
        public bool AetherVisionActive { get; private set; }

        public void SetExternalMoveMultiplier(float multiplier)
        {
            _externalMoveMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
        }

        // Events for systems to subscribe (combat, interact, scan, frequency)
        public event System.Action OnInteract;
        public event System.Action<Vector3> OnScan;
        public event System.Action OnResonancePulse;
        public event System.Action OnHarmonicStrike;
        public event System.Action OnFrequencyShield;
        public event System.Action<float> OnFrequencyAdjust;
        /// <summary>Pause toggle event — UI tier (PauseMenu) subscribes; keeps Input asmdef free of UI references.</summary>
        public static event System.Action OnPauseToggled;

        // Key state for edge-detect
        bool _prevEKey, _prevEscKey, _prevTabKey, _prevGKey;
        bool _prevSpaceKey, _prevFKey, _prevRKey;
        bool _prevVKey, _prevQKey, _prevXKey;
        bool _prevYKey; // Giant Mode toggle (Y) for playtest

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // M2-M4: Logitech F310 full support (DirectInput + XInput + rumble)
            LogitechControllerSupport.EnsureF310Setup();

            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
            EnsureSafetyFloor();
        }

        void EnsureSafetyFloor()
        {
            int groundMask = GetGroundMask();
            _controller.enabled = false;
            Vector3 origin = new Vector3(transform.position.x, 100f, transform.position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
            {
                _groundHeight = hit.point.y;
            }
            _controller.enabled = true;
            if (_groundHeight < -100f) _groundHeight = 0f;
        }

        int GetGroundMask()
        {
            int mask = LayerMask.GetMask("Default", "Terrain", "Ground");
            if (mask == 0) mask = ~LayerMask.GetMask("Building", "Player", "Ignore Raycast");
            return mask;
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
            if (inputActions != null)
            {
                _runtimeActions = Instantiate(inputActions);
                _playerMap = _runtimeActions.FindActionMap("Player", true);
                if (_playerMap != null)
                {
                    _moveAction = _playerMap.FindAction("Move");
                    _sprintAction = _playerMap.FindAction("Sprint");
                    _interactAction = _playerMap.FindAction("Interact");
                    _attackAction = _playerMap.FindAction("Attack");
                    _shieldAction = _playerMap.FindAction("Shield");
                    _harmonicStrikeAction = _playerMap.FindAction("HarmonicStrike");
                    _aetherVisionAction = _playerMap.FindAction("AetherVision");
                    _pauseAction = _playerMap.FindAction("Pause");
                    _scanAction = _playerMap.FindAction("Scan");
                    _frequencyAdjustAction = _playerMap.FindAction("FrequencyAdjust");

                    if (_interactAction != null) _interactAction.performed += OnInteractPerformed;
                    if (_aetherVisionAction != null) _aetherVisionAction.performed += OnAetherVisionPerformed;
                    if (_pauseAction != null) _pauseAction.performed += OnPausePerformed;
                    if (_attackAction != null) _attackAction.performed += OnResonancePulsePerformed;
                    if (_harmonicStrikeAction != null) _harmonicStrikeAction.performed += OnHarmonicStrikePerformed;
                    if (_shieldAction != null) _shieldAction.performed += OnFrequencyShieldPerformed;
                    if (_scanAction != null) _scanAction.performed += OnScanPerformed;

                    _playerMap.Enable();
                }
            }

            Debug.Log($"[PlayerInput] Setup OK (fallback always available)");
        }

        void CleanupInputActions()
        {
            if (_interactAction != null) _interactAction.performed -= OnInteractPerformed;
            if (_aetherVisionAction != null) _aetherVisionAction.performed -= OnAetherVisionPerformed;
            if (_pauseAction != null) _pauseAction.performed -= OnPausePerformed;
            if (_attackAction != null) _attackAction.performed -= OnResonancePulsePerformed;
            if (_harmonicStrikeAction != null) _harmonicStrikeAction.performed -= OnHarmonicStrikePerformed;
            if (_shieldAction != null) _shieldAction.performed -= OnFrequencyShieldPerformed;
            if (_scanAction != null) _scanAction.performed -= OnScanPerformed;

            _playerMap?.Disable();

            if (_runtimeActions != null)
            {
                Destroy(_runtimeActions);
                _runtimeActions = null;
            }
        }

        void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { if (GameStateManager.Instance?.IsPlaying == true) TryInteract(); }
        void OnAetherVisionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { AetherVisionActive = !AetherVisionActive; }
        void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            // M2: Real pause menu — published via static event so UI tier owns the toggle (asmdef-safe).
            OnPauseToggled?.Invoke();
        }
        void OnResonancePulsePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { OnResonancePulse?.Invoke(); }
        void OnHarmonicStrikePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { OnHarmonicStrike?.Invoke(); }
        void OnFrequencyShieldPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { OnFrequencyShield?.Invoke(); }
        void OnScanPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) { OnScan?.Invoke(transform.position); }

        void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            HandleMovementInput();
            HandleContinuousActions();
            HandleActionFallbacks();
            HandleGiantAdvancedInput();
        }

        void HandleContinuousActions()
        {
            var state = GameStateManager.Instance?.CurrentState;
            if (state != GameState.Tuning && state != GameState.Combat) return;

            float adjust = _frequencyAdjustAction != null ? _frequencyAdjustAction.ReadValue<float>() : 0f;
            if (Mathf.Abs(adjust) > 0.01f)
            {
                OnFrequencyAdjust?.Invoke(adjust);
                if (state == GameState.Combat)
                {
                    // [decoupled] Tartaria.UI.HUDController frequency delta — UI layer subscribes to OnFrequencyAdjust event instead
                    // Tartaria.UI.HUDController.Instance?.ApplyCombatFrequencyDelta(adjust * 28f);
                }
            }
        }

        void HandleActionFallbacks()
        {
            if (_playerMap != null) return; // InputSystem handles

            var kb = Keyboard.current;
            if (kb == null) return;

            var state = GameStateManager.Instance?.CurrentState;

            // E — Interact / Resonance Pulse
            bool eDown = kb.eKey.isPressed;
            if (eDown && !_prevEKey)
            {
                if (state == GameState.Combat) OnResonancePulse?.Invoke();
                else TryInteract();
            }
            _prevEKey = eDown;

            // G — Scan
            bool gDown = kb.gKey.isPressed;
            if (gDown && !_prevGKey && (state == GameState.Exploration || state == GameState.Combat))
                OnScan?.Invoke(transform.position);
            _prevGKey = gDown;

            // Y — Giant Mode Toggle (playtest critical) — [Moon 1/Echohaven build fix: decoupled]
            bool yDown = kb.yKey.isPressed;
            if (yDown && !_prevYKey)
            {
                // GiantModeController lives in Tartaria.Integration (asm cycle prevented).
                // Input dispatches via events or GiantModeController subscribes directly in its init.
                // For Echohaven vertical slice: giant abilities active only in Moon 2+ scenes.
            }
            _prevYKey = yDown;

            // Tuning / Combat frequency
            if (state == GameState.Tuning || state == GameState.Combat)
            {
                float tuningAdjust = 0f;
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) tuningAdjust -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) tuningAdjust += 1f;
                if (Mathf.Abs(tuningAdjust) > 0.01f)
                {
                    OnFrequencyAdjust?.Invoke(tuningAdjust);
                    if (state == GameState.Combat)
                    {
                        // [decoupled for asm safety] HUD frequency UI via event
                        // Tartaria.UI.HUDController.Instance?.ApplyCombatFrequencyDelta(tuningAdjust * 26f);
                    }
                }
            }

            if (state == GameState.Combat)
            {
                bool spaceDown = kb.spaceKey.isPressed;
                if (spaceDown && !_prevSpaceKey) OnResonancePulse?.Invoke();
                _prevSpaceKey = spaceDown;

                bool fDown = kb.fKey.isPressed;
                if (fDown && !_prevFKey) OnHarmonicStrike?.Invoke();
                _prevFKey = fDown;

                bool rDown = kb.rKey.isPressed;
                if (rDown && !_prevRKey) OnFrequencyShield?.Invoke();
                _prevRKey = rDown;
            }
            else
            {
                _prevSpaceKey = false;
                _prevFKey = false;
                _prevRKey = false;
            }
        }

        void HandleGiantAdvancedInput()
        {
            // [Moon 1 finishing / Echohaven_VerticalSlice build fix]
            // GiantModeController (Integration) direct access removed to break asmdef cycle with Tartaria.Input.
            // Giant input handling moved to GiantModeController listening to GameEvents / input broadcasts.
            // Stub keeps call site happy for Moon 1 scenes (Echohaven does not require giant flight).
            return;
        }

        Vector3 GetGiantAbilityTargetPoint()
        {
            if (_mainCamera == null) return transform.position + transform.forward * 12f;
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 50f)) return hit.point;
            return transform.position + transform.forward * 15f;
        }

        void HandleMovementInput()
        {
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

            if (_moveInput.sqrMagnitude < 0.01f)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    Vector2 direct = Vector2.zero;
                    if (kb.wKey.isPressed) direct.y += 1f;
                    if (kb.sKey.isPressed) direct.y -= 1f;
                    if (kb.aKey.isPressed) direct.x -= 1f;
                    if (kb.dKey.isPressed) direct.x += 1f;
                    if (direct.sqrMagnitude > 0.01f) _moveInput = direct.normalized;
                }
                var pad = Gamepad.current;
                if (pad != null && _moveInput.sqrMagnitude < 0.01f)
                {
                    Vector2 stick = pad.leftStick.ReadValue();
                    if (stick.sqrMagnitude > 0.0225f) _moveInput = stick;
                }
            }

            // Apply movement
            Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
            if (move.sqrMagnitude > 0.01f)
            {
                move = move.normalized;
                // rotate toward move
                if (_mainCamera != null)
                {
                    Vector3 camForward = Vector3.Scale(_mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 camRight = _mainCamera.transform.right;
                    move = camForward * move.z + camRight * move.x;
                }
                MoveDirection = move;
            }
            else MoveDirection = Vector3.zero;

            float speed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f) * _externalMoveMultiplier;
            Vector3 motion = MoveDirection * speed * Time.deltaTime;

            // gravity
            _velocity.y += gravity * Time.deltaTime;
            if (_controller.isGrounded) _velocity.y = -2f;

            motion += _velocity * Time.deltaTime;
            _controller.Move(motion);

            // simple sprint toggle
            if (Keyboard.current != null)
            {
                _isSprinting = Keyboard.current.leftShiftKey.isPressed;
            }
        }

        void TryInteract()
        {
            // Ray from camera or forward — decoupled from IInteractable (Gameplay) to avoid Input asm cycle
            Vector3 origin = _mainCamera != null ? _mainCamera.transform.position : transform.position + Vector3.up;
            Vector3 dir = _mainCamera != null ? _mainCamera.transform.forward : transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, interactRadius, interactableLayer))
            {
                // Use SendMessage for decoupling (any MB with public void Interact(GameObject))
                var mb = hit.collider.GetComponent<MonoBehaviour>();
                if (mb != null)
                {
                    mb.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
                    OnInteract?.Invoke();
                    return;
                }
            }
            // Fallback sphere
            var cols = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, interactRadius, interactableLayer);
            foreach (var c in cols)
            {
                var mb = c.GetComponent<MonoBehaviour>();
                if (mb != null)
                {
                    mb.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
                    OnInteract?.Invoke();
                    break;
                }
            }
        }

        // Public API used by other systems — [Moon 1 fix: stub, giant debug only in Integration scenes]
        public void ForceGiantToggleForDebug(float aether)
        {
            Debug.Log($"[PlayerInput] ForceGiantToggleForDebug stubbed for Moon 1 build (aether={aether})");
        }
    }
}
