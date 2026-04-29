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
        float _groundHeight = 8f; // detected at runtime via raycast
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

        public Vector3 MoveDirection { get; private set; }
        public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
        public bool AetherVisionActive { get; private set; }

        public void SetExternalMoveMultiplier(float multiplier)
        {
            _externalMoveMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
            EnsureSafetyFloor();
        }

        /// <summary>
        /// Creates an invisible safety floor at Y=-1 to catch the player if the
        /// terrain MeshCollider fails. Also performs a ground raycast diagnostic.
        /// Excludes Building layer (8) so the player lands on terrain, not rooftops.
        /// </summary>
        void EnsureSafetyFloor()
        {
            int groundMask = GetGroundMask();

            // Raycast from high up to find actual terrain height
            // Temporarily disable own collider so we don't hit ourselves
            _controller.enabled = false;
            Vector3 origin = new Vector3(transform.position.x, 100f, transform.position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
            {
                _groundHeight = hit.point.y;
            }
            else
            {
                _groundHeight = 0f; // fallback to world origin
                Debug.LogWarning("[GroundDiag] No ground found, using fallback Y=0");
            }

            // Snap player above the detected ground
            float safeY = _groundHeight + 1.5f;
            transform.position = new Vector3(transform.position.x, safeY, transform.position.z);
            _controller.enabled = true;
            _velocity = Vector3.zero;
        }

        int GetGroundMask()
        {
            // Include everything EXCEPT Building (8), Player (10), Trigger (11)
            int excludeLayers = (1 << 8) | (1 << 10) | (1 << 11);
            return Physics.DefaultRaycastLayers & ~excludeLayers;
        }

        bool TrySampleGroundHeight(Vector3 worldPos, out float groundHeight)
        {
            int groundMask = GetGroundMask();
            Vector3 origin = worldPos + Vector3.up * 2f;
            // Disable our own CharacterController so the downward raycast can't
            // self-hit our capsule (the layer-mask exclusion alone is insufficient
            // because the player prefab may be on the Default layer).
            bool wasEnabled = _controller != null && _controller.enabled;
            if (wasEnabled) _controller.enabled = false;
            bool found = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 30f, groundMask, QueryTriggerInteraction.Ignore);
            if (wasEnabled) _controller.enabled = true;
            if (found)
            {
                groundHeight = hit.point.y;
                return true;
            }

            groundHeight = _groundHeight;
            return false;
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

        // Runtime clone of the InputActionAsset — required so ReadValue<T>()
        // works on composite actions (2DVector WASD). The serialized asset is
        // shared; polling composites on a shared asset returns zero because the
        // action doesn't own device state. Cloning gives us a private copy that
        // properly accumulates control values.
        InputActionAsset _runtimeActions;

        void SetupInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("[PlayerInput] inputActions is NULL — WASD will use keyboard fallback");
                return;
            }

            // Clone the asset so this instance owns the device bindings.
            // Without this, ReadValue<Vector2>() on the Move composite returns
            // zero even though button callbacks (Interact) fire normally.
            _runtimeActions = Instantiate(inputActions);
            _playerMap = _runtimeActions.FindActionMap("Player");
            if (_playerMap == null)
            {
                Debug.LogWarning("[PlayerInput] 'Player' action map not found in InputActionAsset");
                return;
            }

            _moveAction = _playerMap.FindAction("Move");
            _sprintAction = _playerMap.FindAction("Sprint");
            _interactAction = _playerMap.FindAction("Interact");
            _attackAction = _playerMap.FindAction("ResonancePulse");
            _harmonicStrikeAction = _playerMap.FindAction("HarmonicStrike");
            _shieldAction = _playerMap.FindAction("FrequencyShield");
            _aetherVisionAction = _playerMap.FindAction("AetherVision");
            _pauseAction = _playerMap.FindAction("Pause");
            _scanAction = _playerMap.FindAction("Scan");
            _frequencyAdjustAction = _playerMap.FindAction("FrequencyAdjust");

            // Subscribe to button callbacks
            if (_interactAction != null) _interactAction.performed += OnInteractPerformed;
            if (_aetherVisionAction != null) _aetherVisionAction.performed += OnAetherVisionPerformed;
            if (_pauseAction != null) _pauseAction.performed += OnPausePerformed;
            if (_attackAction != null) _attackAction.performed += OnResonancePulsePerformed;
            if (_harmonicStrikeAction != null) _harmonicStrikeAction.performed += OnHarmonicStrikePerformed;
            if (_shieldAction != null) _shieldAction.performed += OnFrequencyShieldPerformed;

            // Scan action (Gap 3)
            if (_scanAction != null) _scanAction.performed += OnScanPerformed;

            _playerMap.Enable();

            // Enable background input in Editor so WASD works even when Game View lacks OS focus
            #if UNITY_EDITOR
            if (UnityEngine.InputSystem.InputSystem.settings != null)
            {
                UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior =
                    UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
                UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode =
                    UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            }
            #endif

            Debug.Log($"[PlayerInput] SetupInputActions OK: map={_playerMap.name} enabled={_playerMap.enabled} moveAction={_moveAction?.name} moveEnabled={_moveAction?.enabled} moveType={_moveAction?.type} bindings={_moveAction?.bindings.Count}");
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

            // Destroy the runtime clone to prevent InputActionAsset leak on scene transitions
            if (_runtimeActions != null)
            {
                Destroy(_runtimeActions);
                _runtimeActions = null;
            }
        }

        void Update()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;

            HandleMovementInput();
            HandleContinuousActions();
            HandleActionFallbacks();
        }

        void HandleContinuousActions()
        {
            if (GameStateManager.Instance?.CurrentState != GameState.Tuning) return;

            float adjust = _frequencyAdjustAction != null ? _frequencyAdjustAction.ReadValue<float>() : 0f;
            if (Mathf.Abs(adjust) > 0.01f)
                OnFrequencyAdjust?.Invoke(adjust);
        }

        // Key state for edge-detect (wasPressedThisFrame equivalent with manual tracking
        // so it also works when InputSystem's frame state isn't flushed on headless builds)
        bool _prevEKey, _prevEscKey, _prevTabKey, _prevGKey;
        bool _prevSpaceKey, _prevFKey, _prevRKey;

        // Keyboard fallbacks for every gameplay action.
        // Only fires when InputActionAsset is NOT loaded (_playerMap == null).
        // This ensures all interactions, combat, and UI work out-of-the-box without
        // needing the .inputactions asset assigned in the Inspector.
        void HandleActionFallbacks()
        {
            if (_playerMap != null) return; // InputActionAsset handles it via callbacks

            var kb = Keyboard.current;
            if (kb == null) return;

            var state = GameStateManager.Instance?.CurrentState;

            // E — Interact (Exploration) or Resonance Pulse (Combat)
            bool eDown = kb.eKey.isPressed;
            if (eDown && !_prevEKey)
            {
                if (state == GameState.Combat)
                    OnResonancePulse?.Invoke();
                else
                    TryInteract();
            }
            _prevEKey = eDown;

            // Escape — Pause toggle
            bool escDown = kb.escapeKey.isPressed;
            if (escDown && !_prevEscKey)
                GameEvents.FireTogglePause();
            _prevEscKey = escDown;

            // Tab — AetherVision toggle
            bool tabDown = kb.tabKey.isPressed;
            if (tabDown && !_prevTabKey)
            {
                AetherVisionActive = !AetherVisionActive;
                AudioManager.Instance?.PlaySFX2D(AetherVisionActive ? "AetherVisionOn" : "AetherVisionOff");
                GameEvents.FireToggleAetherVision();
            }
            _prevTabKey = tabDown;

            // G — Area Scan
            bool gDown = kb.gKey.isPressed;
            if (gDown && !_prevGKey &&
                (state == GameState.Exploration || state == GameState.Combat))
                OnScan?.Invoke(transform.position);
            _prevGKey = gDown;

            // Tuning fallback axis (when no InputActionAsset is loaded)
            if (state == GameState.Tuning)
            {
                float tuningAdjust = 0f;
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) tuningAdjust -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) tuningAdjust += 1f;

                var pad = Gamepad.current;
                if (pad != null)
                {
                    float stickY = pad.rightStick.ReadValue().y;
                    if (Mathf.Abs(stickY) > 0.2f)
                        tuningAdjust = stickY;
                }

                if (Mathf.Abs(tuningAdjust) > 0.01f)
                    OnFrequencyAdjust?.Invoke(tuningAdjust);
            }

            // Combat-only keys
            if (state == GameState.Combat)
            {
                // Space — Resonance Pulse
                bool spaceDown = kb.spaceKey.isPressed;
                if (spaceDown && !_prevSpaceKey) OnResonancePulse?.Invoke();
                _prevSpaceKey = spaceDown;

                // F — Harmonic Strike
                bool fDown = kb.fKey.isPressed;
                if (fDown && !_prevFKey) OnHarmonicStrike?.Invoke();
                _prevFKey = fDown;

                // R — Frequency Shield
                bool rDown = kb.rKey.isPressed;
                if (rDown && !_prevRKey) OnFrequencyShield?.Invoke();
                _prevRKey = rDown;
            }
            else
            {
                // Reset combat key state when leaving combat to avoid phantom fires on re-entry
                _prevSpaceKey = false;
                _prevFKey = false;
                _prevRKey = false;
            }
        }

        void HandleMovementInput()
        {
            // Read from Input System actions
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

            // Fallback: if InputAction returns zero, try reading keyboard directly
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
                    if (direct.sqrMagnitude > 0.01f)
                    {
                        _moveInput = direct.normalized;
                        if (!_loggedMoveFallback)
                        {
                            _loggedMoveFallback = true;
                            Debug.LogWarning($"[PlayerInput] Using direct keyboard fallback -- InputAction Move not reading WASD. _moveAction={((_moveAction != null) ? "exists enabled=" + _moveAction.enabled : "NULL")}");
                        }
                    }
                }

                // Gamepad left-stick direct fallback (if InputActionAsset isn't loaded yet)
                if (_moveInput.sqrMagnitude < 0.01f)
                {
                    var pad = Gamepad.current;
                    if (pad != null)
                    {
                        Vector2 stick = pad.leftStick.ReadValue();
                        if (stick.sqrMagnitude > 0.0225f) // 0.15 deadzone squared
                            _moveInput = stick;
                    }
                }
            }
            else if (!_loggedMoveActionOk)
            {
                _loggedMoveActionOk = true;
                Debug.Log("[PlayerInput] InputAction Move reading WASD correctly");
            }

            _isSprinting = _sprintAction != null ? _sprintAction.IsPressed() : (Keyboard.current?.leftShiftKey.isPressed ?? false);

            // Refresh camera ref if lost (zone transition, cutscene swap)
            if (_mainCamera == null)
            {
                _cameraRetryTimer -= Time.deltaTime;
                if (_cameraRetryTimer <= 0f)
                {
                    _cameraRetryTimer = 0.25f;
                    _mainCamera = Camera.main;
                }
            }

            // Build horizontal movement vector (applied later in single Move call)
            Vector3 horizontalMove = Vector3.zero;
            if (_mainCamera != null && _moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 camForward = _mainCamera.transform.forward;
                Vector3 camRight = _mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                MoveDirection = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

                float speed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f) * _externalMoveMultiplier;
                horizontalMove = MoveDirection * speed * Time.deltaTime;

                // Footstep SFX (throttled)
                _footstepTimer -= Time.deltaTime;
                if (_footstepTimer <= 0f)
                {
                    _footstepTimer = _isSprinting ? 0.28f : 0.42f;
                    AudioManager.Instance?.PlaySFX(_isSprinting ? "FootstepSprint" : "Footstep", transform.position, 0.35f);
                }

                // Rotate toward movement direction
                if (MoveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            // Fall-through safety net: teleport back above terrain if fallen too far
            if (transform.position.y < _groundHeight - 10f)
            {
                Debug.LogWarning($"[PlayerInput] Fall-through at Y={transform.position.y:F1}, ground={_groundHeight:F1}, resetting");
                float resetX = transform.position.x;
                float resetZ = transform.position.z;
                int groundMask = GetGroundMask();
                if (Physics.Raycast(new Vector3(resetX, 100f, resetZ), Vector3.down, out RaycastHit gHit, 300f, groundMask, QueryTriggerInteraction.Ignore))
                    _groundHeight = gHit.point.y;
                else
                    _groundHeight = 0f;
                _controller.enabled = false;
                transform.position = new Vector3(resetX, _groundHeight + 1.5f, resetZ);
                _controller.enabled = true;
                _velocity = Vector3.zero;
                return;
            }

            // Ground detection: CC check + raycast fallback
            bool grounded = _controller.isGrounded;
            if (!grounded)
            {
                // Raycast from slightly above feet — if ground is within step distance, treat as grounded.
                // Disable own CC during raycast so we don't self-hit our capsule.
                int gMask = GetGroundMask();
                bool ccWasEnabled = _controller.enabled;
                if (ccWasEnabled) _controller.enabled = false;
                bool stepFound = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit stepHit, 0.4f, gMask, QueryTriggerInteraction.Ignore);
                if (ccWasEnabled) _controller.enabled = true;
                if (stepFound)
                {
                    grounded = true;
                    _groundHeight = stepHit.point.y;
                }
            }

            // Continuously refresh ground reference while traversing uneven terrain.
            if (TrySampleGroundHeight(transform.position, out float sampledGroundY))
                _groundHeight = sampledGroundY;

            // Gravity
            if (grounded && _velocity.y < 0)
                _velocity.y = -2f;

            _velocity.y += gravity * Time.deltaTime;

            // Ensure physics colliders are synced before the first CC.Move()
            // Without this, the CC can pass through MeshColliders that were
            // set up before the CC was enabled (e.g. during Awake/scene load).
            if (_firstMove)
            {
                _firstMove = false;
                Physics.SyncTransforms();
            }

            // Single Move() call — horizontal + vertical combined
            // (calling Move() twice per frame causes CC collision detection bugs)
            horizontalMove.y = _velocity.y * Time.deltaTime;
            _controller.Move(horizontalMove);

            // Runaway depenetration guard: if CC.Move() shoved us upward by
            // more than 5m in a single frame (common when spawned overlapping
            // a wall collider), snap back to ground level.
            if (transform.position.y > _groundHeight + 10f)
            {
                Debug.LogWarning($"[PlayerInput] Depenetration spike Y={transform.position.y:F1}, ground={_groundHeight:F1}, snapping down");
                _controller.enabled = false;
                transform.position = new Vector3(
                    transform.position.x,
                    _groundHeight + 1.5f,
                    transform.position.z);
                _controller.enabled = true;
                _velocity = Vector3.zero;
                return;
            }

            // Immediate ground clamp — if CC.Move() let us sink below terrain,
            // snap back above it.  This catches first-frame and edge-case
            // penetrations that the distant fall-through net (-10m) would miss.
            if (transform.position.y < _groundHeight - 0.5f)
            {
                _controller.enabled = false;
                transform.position = new Vector3(
                    transform.position.x,
                    _groundHeight + 0.1f,
                    transform.position.z);
                _controller.enabled = true;
                _velocity.y = -2f;
            }
        }

        // ─── Input Action Callbacks ──────────────────

        void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            // Allow interaction in any non-paused state. Gating on IsPlaying
            // silently dropped E-key presses if state hadn't been set yet
            // (e.g. first frame after scene load) — the #1 reason "interact
            // does nothing".
            var state = GameStateManager.Instance?.CurrentState ?? GameState.Exploration;
            if (state == GameState.Paused || state == GameState.Menu || state == GameState.Loading) return;

            if (state == GameState.Combat)
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
            AudioManager.Instance?.PlaySFX2D(AetherVisionActive ? "AetherVisionOn" : "AetherVisionOff");
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

        void OnScanPerformed(InputAction.CallbackContext ctx)
        {
            var state = GameStateManager.Instance?.CurrentState;
            if (state == GameState.Exploration || state == GameState.Combat)
                OnScan?.Invoke(transform.position);
        }

        void TryInteract()
        {
            // Robustness: if interactableLayer wasn't configured (mask = 0),
            // fall back to ALL layers except Ignore Raycast and Water.
            // Without this guard, Physics.Raycast with mask=0 hits nothing
            // and the player can never interact with anything.
            int mask = interactableLayer.value;
            if (mask == 0) mask = ~((1 << 2) | (1 << 4)); // exclude IgnoreRaycast, Water

            // 1) Camera raycast (mouse aim or screen center)
            if (_mainCamera != null)
            {
                Vector2 screenPos;
                if (Gamepad.current != null && (Pointer.current == null || !Pointer.current.press.isPressed))
                    screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                else if (Pointer.current != null)
                    screenPos = Pointer.current.position.ReadValue();
                else
                    screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

                Ray ray = _mainCamera.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask, QueryTriggerInteraction.Collide))
                {
                    var interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        float dist = Vector3.Distance(transform.position, hit.point);
                        if (dist <= interactRadius)
                        {
                            interactable.Interact(gameObject);
                            AudioManager.Instance?.PlaySFX("Interact", hit.point, 0.5f);
                            return;
                        }
                    }
                }
            }

            // 2) Always-on proximity fallback: pick nearest interactable in radius.
            //    Works for keyboard, mouse and gamepad alike.
            Collider[] nearby = Physics.OverlapSphere(transform.position, interactRadius, mask, QueryTriggerInteraction.Collide);
            float bestDist = float.MaxValue;
            IInteractable bestTarget = null;
            Vector3 bestPos = Vector3.zero;
            foreach (var col in nearby)
            {
                var target = col.GetComponentInParent<IInteractable>();
                if (target == null) continue;
                float d = Vector3.Distance(transform.position, col.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestTarget = target;
                    bestPos = col.transform.position;
                }
            }
            if (bestTarget != null)
            {
                bestTarget.Interact(gameObject);
                AudioManager.Instance?.PlaySFX("Interact", bestPos, 0.5f);
            }
            else
            {
                AudioManager.Instance?.PlaySFX2D("InteractFail", 0.3f);
                Debug.Log($"[PlayerInput] TryInteract: no IInteractable in {interactRadius}m (mask=0x{mask:X})");
            }
        }

        // Combat events -- consumed by CombatController
        public event System.Action OnResonancePulse;
        public event System.Action OnHarmonicStrike;
        public event System.Action OnFrequencyShield;
        public event System.Action<Vector3> OnScan;
        public event System.Action<float> OnFrequencyAdjust;
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
