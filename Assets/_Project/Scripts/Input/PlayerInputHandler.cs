using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Audio;

#if ENABLE_INPUT_SYSTEM
using Pointer = UnityEngine.InputSystem.Pointer;
#endif

namespace Tartaria.Input
{
#pragma warning disable CS0414 // Field assigned but never used - reserved for future implementation
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
        [SerializeField, Min(0.1f), Tooltip("Max distance to interact with objects")] float interactRadius = 5.0f;
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
        readonly Collider[] _interactBuffer = new Collider[10];

        /// <summary>Debug: External speed multiplier (set via DebugConsole /speed command).</summary>
        public float SpeedMultiplier { get => _externalMoveMultiplier; set => _externalMoveMultiplier = Mathf.Clamp(value, 0.1f, 10f); }

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

            // 2026-05-31: defeat OS-focus-loss bug (weather widget, etc steal focus
            // and Unity stops polling input). Keep input flowing regardless.
            Application.runInBackground = true;
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior =
                UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode =
                UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif

            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
            EnsureSafetyFloor();
        }

        void EnsureSafetyFloor()
        {
            // H.L1 NRE-hunt: defensive guard. _controller is GetComponent<CharacterController>()
            // in Awake; if Player.prefab ever ships without one (regression), the unconditional
            // _controller.enabled access here was an NRE source on Play start. Log and bail.
            if (_controller == null)
            {
                Debug.LogError($"[PlayerInputHandler] EnsureSafetyFloor: CharacterController missing on '{name}'. " +
                               "Player.prefab must include CharacterController (Sprint 11 L6 / P4.L2 bake). Skipping safety-floor raycast.");
                _groundHeight = 0f;
                return;
            }
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
            // P2.L3: Death/Respawn input gate — disable input on death, re-enable on respawn.
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
            GameEvents.OnPlayerRespawned += HandlePlayerRespawned;
        }

        void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            GameEvents.OnPlayerRespawned -= HandlePlayerRespawned;
            CleanupInputActions();
        }

        // P2.L3: Death disables player input fully (movement, actions, fallbacks).
        // Respawn re-enables and clears pending velocity so the player doesn't fall through floor.
        bool _isDeadInputGated;
        void HandlePlayerDeath()
        {
            _isDeadInputGated = true;
            _moveInput = Vector2.zero;
            _velocity = Vector3.zero;
            _isSprinting = false;
        }
        void HandlePlayerRespawned()
        {
            _isDeadInputGated = false;
            _moveInput = Vector2.zero;
            _velocity = Vector3.zero;
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
            // P4.L3: Restore Sprint 11 L4 IsPlaying gate. Player must NOT walk during
            // Boot/Loading/Menu/Paused/Cinematic/Dialogue, and must NOT walk while dead.
            // IsPlaying => Exploration || Tuning || Combat (see GameStateManager.cs:62-65).
            if (_isDeadInputGated) return;
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

        // ─────────────────────────────────────────────────────────────────────
        // F310 button edge-detect state (was-pressed-last-frame tracking)
        // ─────────────────────────────────────────────────────────────────────
        bool _prevPadA, _prevPadB, _prevPadX, _prevPadY;
        bool _prevPadLB, _prevPadRB, _prevPadLT;
        bool _prevPadStart, _prevPadSelect;
        bool _prevPadDpadUp, _prevPadDpadDown, _prevPadDpadLeft, _prevPadDpadRight;
        bool _prevPadL3, _prevPadR3;

        /// <summary>
        /// Logitech F310 gamepad fallback — always runs (even when InputAction asset
        /// is loaded) so every F310 button drives a real game feature.
        /// Per CLAUDE.md no-stubs mandate: every button has a real binding.
        ///
        /// F310 X-mode button map (XInput identifiers — Input System normalizes):
        ///   A  (south)   = Interact (Exploration) / Resonance Pulse (Combat)
        ///   B  (east)    = Scan / Cancel
        ///   X  (west)    = Resonance Pulse (Combat) / Interact alt
        ///   Y  (north)   = Aether Vision toggle
        ///   LB           = Sprint hold
        ///   RB           = Harmonic Strike (Combat)
        ///   LT (analog)  = Frequency Shield (Combat) — threshold > 0.5
        ///   RT (analog)  = Sprint hold (alt) — threshold > 0.5
        ///   Start        = Pause menu
        ///   Back/Select  = Aether Vision toggle (alt)
        ///   D-Pad ←/→    = Frequency adjust (Tuning + Combat)
        ///   D-Pad ↑      = Scan
        ///   D-Pad ↓      = Crouch / Cancel (reserved)
        ///   L3 click     = Sprint toggle
        ///   R3 click     = Recenter camera (CameraController handles)
        /// </summary>
        void HandleGamepadButtonFallbacks()
        {
            var pad = Gamepad.current;
            if (pad == null) return;
            var state = GameStateManager.Instance?.CurrentState;

            // A / South — Interact OR Resonance Pulse (combat)
            bool aDown = pad.buttonSouth.isPressed;
            if (aDown && !_prevPadA)
            {
                if (state == GameState.Combat) OnResonancePulse?.Invoke();
                else TryInteract();
            }
            _prevPadA = aDown;

            // B / East — Scan
            bool bDown = pad.buttonEast.isPressed;
            if (bDown && !_prevPadB)
            {
                if (state == GameState.Exploration || state == GameState.Combat)
                    OnScan?.Invoke(transform.position);
            }
            _prevPadB = bDown;

            // X / West — Resonance Pulse / Interact alt
            bool xDownPad = pad.buttonWest.isPressed;
            if (xDownPad && !_prevPadX)
            {
                if (state == GameState.Combat) OnResonancePulse?.Invoke();
                else TryInteract();
            }
            _prevPadX = xDownPad;

            // Y / North — Aether Vision toggle
            bool yDownPad = pad.buttonNorth.isPressed;
            if (yDownPad && !_prevPadY)
            {
                AetherVisionActive = !AetherVisionActive;
            }
            _prevPadY = yDownPad;

            // LB — Sprint hold
            if (pad.leftShoulder.isPressed) _isSprinting = true;

            // RB — Harmonic Strike
            bool rbDown = pad.rightShoulder.isPressed;
            if (rbDown && !_prevPadRB)
            {
                if (state == GameState.Combat) OnHarmonicStrike?.Invoke();
            }
            _prevPadRB = rbDown;

            // LT (analog) — Frequency Shield
            float lt = pad.leftTrigger.ReadValue();
            bool ltDown = lt > 0.5f;
            if (ltDown && !_prevPadLT)
            {
                if (state == GameState.Combat) OnFrequencyShield?.Invoke();
            }
            _prevPadLT = ltDown;

            // RT (analog) — alternate sprint
            if (pad.rightTrigger.ReadValue() > 0.5f) _isSprinting = true;

            // Start — Pause
            bool startDown = pad.startButton.isPressed;
            if (startDown && !_prevPadStart)
            {
                OnPauseToggled?.Invoke();
            }
            _prevPadStart = startDown;

            // Back/Select — Aether Vision (alt)
            bool selectDown = pad.selectButton.isPressed;
            if (selectDown && !_prevPadSelect)
            {
                AetherVisionActive = !AetherVisionActive;
            }
            _prevPadSelect = selectDown;

            // D-Pad ←/→ frequency adjust during Tuning/Combat
            bool dLeft = pad.dpad.left.isPressed;
            bool dRight = pad.dpad.right.isPressed;
            if (state == GameState.Tuning || state == GameState.Combat)
            {
                float dpadAdjust = 0f;
                if (dLeft) dpadAdjust -= 1f;
                if (dRight) dpadAdjust += 1f;
                if (Mathf.Abs(dpadAdjust) > 0.01f) OnFrequencyAdjust?.Invoke(dpadAdjust);
            }
            _prevPadDpadLeft = dLeft;
            _prevPadDpadRight = dRight;

            // D-Pad ↑ — Scan
            bool dUp = pad.dpad.up.isPressed;
            if (dUp && !_prevPadDpadUp && (state == GameState.Exploration || state == GameState.Combat))
            {
                OnScan?.Invoke(transform.position);
            }
            _prevPadDpadUp = dUp;

            // D-Pad ↓ — reserved for crouch/cancel
            bool dDownBtn = pad.dpad.down.isPressed;
            _prevPadDpadDown = dDownBtn;

            // L3 — Sprint toggle
            bool l3Down = pad.leftStickButton.isPressed;
            if (l3Down && !_prevPadL3) _isSprinting = !_isSprinting;
            _prevPadL3 = l3Down;
        }

        void HandleActionFallbacks()
        {
            // F310 gamepad fallback ALWAYS runs — even when InputAction asset is bound —
            // so missing button bindings can't kill controller play.
            HandleGamepadButtonFallbacks();

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
            // 2026-06-02: Restored canonical (x=strafe, z=forward) mapping. The prior
            // (y, 0, -x) swap rotated WASD 90deg CW (W -> strafe-right). See docs/HANDOFFS.md.
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

                    // Audio + haptic feedback
                    Audio.AudioManager.Instance?.PlaySFX2D("interact_confirm");
                    HapticFeedbackManager.Instance?.PlayContextual();

                    return;
                }
            }
            // Fallback sphere cast if raycast misses
            int colCount = Physics.OverlapSphereNonAlloc(transform.position + transform.forward * 1.5f, interactRadius, _interactBuffer, interactableLayer);
            for (int i = 0; i < colCount; i++)
            {
                var c = _interactBuffer[i];
                var mb = c.GetComponent<MonoBehaviour>();
                if (mb != null)
                {
                    mb.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
                    OnInteract?.Invoke();

                    // Audio + haptic feedback
                    Audio.AudioManager.Instance?.PlaySFX2D("interact_confirm");
                    HapticFeedbackManager.Instance?.PlayContextual();

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
