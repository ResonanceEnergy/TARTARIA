using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Camera
{
    /// <summary>
    /// Camera Controller — manages multiple camera modes:
    ///   Exploration: 3/4 top-down, 45° pitch, 15m follow distance
    ///   Close-Up:    Zoom to 5m on POI approach
    ///   Tuning:      Fixed overhead, slight tilt
    ///   Combat:      Pull back to 20m, wider FOV
    ///   Cinematic:   Pre-authored paths for restoration reveals
    ///
    /// Works alongside Cinemachine for smooth blends.
    /// </summary>
    /// <summary>
    /// Five explicit camera modes per docs/15_MVP_BUILD_SPEC.md §12.
    /// CameraController.SetMode(...) interpolates between mode configs over 0.6s.
    /// Event-driven: GameEvents.OnTuningStart/End, OnCombatEngaged/Ended,
    /// OnCinematicStart/End, OnBuildingRestored route directly into SetMode.
    /// </summary>
    public enum CameraMode
    {
        /// <summary>3rd-person follow, orbit allowed. Default while walking the world.</summary>
        Exploration,
        /// <summary>Tight pull-in for inspecting a building or NPC. No orbit, brief.</summary>
        CloseUp,
        /// <summary>Focus on tuning pedestal. Orbit disabled to keep node centered.</summary>
        Tuning,
        /// <summary>Wider FOV + pulled-back, orbit allowed for situational awareness.</summary>
        Combat,
        /// <summary>Cinematic system owns the camera transform — no follow / no input.</summary>
        Cinematic
    }

    /// <summary>
    /// Per-mode camera configuration. Used by CameraController.SetMode() to
    /// interpolate distance / height / FOV / dampening when the active mode changes.
    /// </summary>
    [System.Serializable]
    public struct CameraModeConfig
    {
        public float distance;
        public float height;
        public float fov;
        /// <summary>Lerp dampening multiplier per mode (lower = snappier, higher = smoother).</summary>
        public float dampening;
        public bool orbitAllowed;
    }

    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour, ICameraShakeService
    {
        [Header("Target")]
        [SerializeField, Tooltip("Transform to follow and orbit around")] Transform followTarget;

        [Header("Camera Mode (docs/15 §12)")]
        [SerializeField, Tooltip("Current camera rig mode — drives distance/FOV/dampening")] CameraMode _currentMode = CameraMode.Exploration;
        /// <summary>Active camera mode. Set via SetMode(...); reflects post-interpolation target.</summary>
        public CameraMode CurrentMode { get; private set; } = CameraMode.Exploration;
        /// <summary>True while the active mode is Cinematic — consumers (e.g. PlayerInputHandler) should gate input.</summary>
        public bool IsCinematic => CurrentMode == CameraMode.Cinematic;

        // Per-mode configs — defaults per task spec & docs/15 §12.
        CameraModeConfig _cfgExploration = new CameraModeConfig { distance = 8f,  height = 4f, fov = 65f, dampening = 0.3f, orbitAllowed = true };
        CameraModeConfig _cfgCloseUp     = new CameraModeConfig { distance = 3f,  height = 2f, fov = 50f, dampening = 0.1f, orbitAllowed = false };
        CameraModeConfig _cfgTuning      = new CameraModeConfig { distance = 5f,  height = 3f, fov = 55f, dampening = 0.5f, orbitAllowed = false };
        CameraModeConfig _cfgCombat      = new CameraModeConfig { distance = 10f, height = 5f, fov = 75f, dampening = 0.4f, orbitAllowed = true  };
        CameraModeConfig _cfgCinematic   = new CameraModeConfig { distance = 0f,  height = 0f, fov = 60f, dampening = 1.0f, orbitAllowed = false };

        Coroutine _modeBlendCo;
        Coroutine _closeUpAutoReturnCo;
        /// <summary>Default mode-blend duration (seconds). Matches docs/15 §12 "smooth lerp" feel.</summary>
        const float ModeBlendDuration = 0.6f;

        [Header("Exploration Mode")]
        [SerializeField, Tooltip("Camera distance in exploration mode")] float exploreDistance = 9f;
        [SerializeField, Tooltip("Camera pitch angle in exploration mode")] float explorePitch = 28f;
        [SerializeField, Tooltip("Field of view in exploration mode")] float exploreFOV = 65f;

        [Header("Combat Mode")]
        [SerializeField, Tooltip("Camera distance in combat mode")] float combatDistance = 12f;
        [SerializeField, Tooltip("Camera pitch angle in combat mode")] float combatPitch = 32f;
        [SerializeField, Tooltip("Field of view in combat mode")] float combatFOV = 65f;

        [Header("Close-Up Mode")]
        [SerializeField, Tooltip("Camera distance for close-up inspections")] float closeUpDistance = 5f;
        [SerializeField, Tooltip("Field of view for close-up inspections")] float closeUpFOV = 40f;

        [Header("Tuning Mode")]
        [SerializeField, Tooltip("Camera distance during building tuning")] float tuningDistance = 8f;
        [SerializeField, Tooltip("Camera pitch angle during building tuning")] float tuningPitch = 70f;

        [Header("Controls")]
        [SerializeField, Tooltip("Mouse scroll zoom speed")] float zoomSpeed = 3f;
        [SerializeField, Tooltip("Minimum allowed zoom distance")] float zoomMin = 4f;
        [SerializeField, Tooltip("Maximum allowed zoom distance")] float zoomMax = 18f;
        [SerializeField, Tooltip("Camera orbit rotation speed (degrees/sec)")] float orbitSpeed = 120f;
        [SerializeField, Tooltip("Gamepad right-stick orbit sensitivity")] float gamepadOrbitSpeed = 150f;
        [SerializeField, Tooltip("Gamepad zoom speed (D-pad / right shoulder)")] float gamepadZoomSpeed = 8f;
        [SerializeField, Tooltip("Camera movement interpolation speed")] float smoothSpeed = 8f;
        [SerializeField, Tooltip("Enable verbose runtime camera diagnostics")]
        bool enableDiagnostics;

        UnityEngine.Camera _camera;
        Transform _lookTarget; // CameraTarget child (chest height) — falls back to followTarget
        float _currentDistance;
        float _currentPitch;
        float _currentYaw;
        float _targetFOV;
        float _zoomOffset;
        bool _rightStickLogged;  // diagnostic: log first right-stick read per session

        // M2: Live mouse sensitivity from Settings
        public static float MouseSensitivityMultiplier { get; private set; } = 1f;
        public static bool InvertCameraY { get; private set; } = false; // Default: not inverted (push up = look up)
        public static bool InvertCameraX { get; private set; } = false; // Default: not inverted (push right = orbit right)

        public static void SetMouseSensitivity(float value)
        {
            MouseSensitivityMultiplier = Mathf.Clamp(value, 0.25f, 3f);
            PlayerPrefs.SetFloat("TARTARIA_MouseSens", MouseSensitivityMultiplier);
        }

        public static void SetInvertCameraY(bool inverted)
        {
            InvertCameraY = inverted;
            PlayerPrefs.SetInt("TARTARIA_InvertY", inverted ? 1 : 0);
        }

        public static void SetInvertCameraX(bool inverted)
        {
            InvertCameraX = inverted;
            PlayerPrefs.SetInt("TARTARIA_InvertX", inverted ? 1 : 0);
        }
        Coroutine _closeUpCoroutine;
        GameState _preCloseUpState;
        float _playerSearchCooldown;
        int _diagCounter;

        // Camera-local InputAction instances — avoids shared-state issues with PlayerInputHandler's clone
        InputAction _zoomAction;
        InputAction _gamepadOrbitAction;

        void Awake()
        {
            ServiceLocator.CameraShake = this;
            // Runtime safety: older scenes may carry stale serialized camera values
            // (18m distance / 55 deg pitch). Clamp to the intended tighter framing.
            // 2026-06-03 playtest: explorePitch=18° at dist=6.5 framed the camera so low
            // it skimmed the ground — capsule's bottom half was the only thing on screen.
            // Bumped to 30° (third-person classic over-shoulder angle) and dist 8m so the
            // FULL character is in frame from head to feet with terrain horizon mid-screen.
            exploreDistance = 8f;    // Was 6.5f — more headroom for full character
            explorePitch = 30f;      // Was 18f — proper 3rd-person elevated framing
            exploreFOV = 65f;        // Was 70f — slightly tighter to compensate for wider angle
            combatDistance = 10f;    // Was 12f — adjusted proportionally
            combatPitch = 25f;       // Was 32f
            combatFOV = 70f;         // Was 65f
            zoomMin = 4f;
            zoomMax = 18f;
            smoothSpeed = 12f;       // Was 8f — faster interpolation

            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera == null)
                _camera = GetComponentInChildren<UnityEngine.Camera>();
            if (_camera == null)
                Debug.LogError("[Camera] No Camera component found on CameraController or children.");

            _currentDistance = exploreDistance;
            _currentPitch = explorePitch;
            _currentYaw = 180f; // Face north — toward the StarDome / Fountain cluster
            _targetFOV = exploreFOV;
            _playerSearchCooldown = 0f; // Search immediately on first frame

            // Load camera inversion settings
            InvertCameraY = PlayerPrefs.GetInt("TARTARIA_InvertY", 0) == 1;
            InvertCameraX = PlayerPrefs.GetInt("TARTARIA_InvertX", 0) == 1;
            MouseSensitivityMultiplier = PlayerPrefs.GetFloat("TARTARIA_MouseSens", 1f);
        }

        void OnEnable()
        {
            _zoomAction = new InputAction("CameraZoom", InputActionType.Value);
            _zoomAction.AddBinding("<Mouse>/scroll/y")
                .WithProcessor("Normalize(min=-120,max=120)");
            _zoomAction.AddBinding("<Gamepad>/dpad/y");
            _zoomAction.Enable();

            // 2026-06-03 R7: F310 reports as `XInputControllerWindows` and on that layout
            // the `<Gamepad>/rightStick` binding sometimes fails to resolve when there's no
            // InputUser pairing. Add an explicit `<XInputController>/rightStick` binding
            // alongside the generic one so the action fires regardless of layout reflection.
            _gamepadOrbitAction = new InputAction("GamepadOrbit", InputActionType.Value,
                processors: "StickDeadzone(min=0.08)");
            _gamepadOrbitAction.AddBinding("<Gamepad>/rightStick");
            _gamepadOrbitAction.AddBinding("<XInputController>/rightStick");
            _gamepadOrbitAction.Enable();

            // P2.L3 — Death/Respawn camera fade (Sprint 11 L9 fix).
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
            GameEvents.OnPlayerRespawned += HandlePlayerRespawned;

            // ─── docs/15 §12 camera-mode wiring (2026-06-03 C.L6) ───
            GameEvents.OnTuningStart      += HandleTuningStart;
            GameEvents.OnTuningEnd        += HandleTuningEnd;
            GameEvents.OnCombatEngaged    += HandleCombatEngaged;
            GameEvents.OnCombatStarted    += HandleCombatEngaged;  // legacy alias
            GameEvents.OnCombatEnded      += HandleCombatEnded;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            GameEvents.OnCinematicStart   += HandleCinematicStart;
            GameEvents.OnCinematicEnd     += HandleCinematicEnd;
        }

        void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            GameEvents.OnPlayerRespawned -= HandlePlayerRespawned;

            GameEvents.OnTuningStart      -= HandleTuningStart;
            GameEvents.OnTuningEnd        -= HandleTuningEnd;
            GameEvents.OnCombatEngaged    -= HandleCombatEngaged;
            GameEvents.OnCombatStarted    -= HandleCombatEngaged;
            GameEvents.OnCombatEnded      -= HandleCombatEnded;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnCinematicStart   -= HandleCinematicStart;
            GameEvents.OnCinematicEnd     -= HandleCinematicEnd;

            _zoomAction?.Disable();
            _zoomAction?.Dispose();
            _zoomAction = null;
            _gamepadOrbitAction?.Disable();
            _gamepadOrbitAction?.Dispose();
            _gamepadOrbitAction = null;
        }

        // ─── P2.L3 Death/Respawn camera fade ───
        bool _deathFadeActive;
        float _deathFadeAmount; // 0 = normal, 1 = full grey
        const float DeathFadeSpeed = 1.5f;

        void HandlePlayerDeath()
        {
            _deathFadeActive = true;
        }

        void HandlePlayerRespawned()
        {
            _deathFadeActive = false;
        }

        void OnGUI()
        {
            // Drive grey fade target.
            float target = _deathFadeActive ? 1f : 0f;
            _deathFadeAmount = Mathf.MoveTowards(_deathFadeAmount, target, Time.unscaledDeltaTime * DeathFadeSpeed);
            if (_deathFadeAmount <= 0.001f) return;

            // Full-screen desaturated grey overlay (sits behind HUDController's death overlay due to script-order).
            var prev = GUI.color;
            GUI.color = new Color(0.18f, 0.18f, 0.2f, 0.65f * _deathFadeAmount);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }

        void OnDestroy()
        {
            bool wasInCloseUp = _closeUpCoroutine != null;
            StopAllCoroutines();
            _closeUpCoroutine = null;
            if (wasInCloseUp)
                GameStateManager.Instance?.TransitionTo(_preCloseUpState);
        }

        void LateUpdate()
        {
            if (followTarget == null)
            {
                _playerSearchCooldown -= Time.deltaTime;
                if (_playerSearchCooldown > 0f) return;
                _playerSearchCooldown = 0.25f; // Retry every 0.25s (was 0.5s)
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    followTarget = player.transform;
                    // Use CameraTarget child (chest height) for look-at if available
                    var ct = followTarget.Find("CameraTarget");
                    _lookTarget = ct != null ? ct : followTarget;
                    Debug.Log("[CameraController] Player found and locked.");
                }
                else
                    return;
            }

            UpdateCameraMode();
            HandleInput();
            ApplyCamera();
        }

        void UpdateCameraMode()
        {
            if (GameStateManager.Instance == null) return;
            var state = GameStateManager.Instance.CurrentState;

            switch (state)
            {
                case GameState.Exploration:
                    _currentDistance = Mathf.Lerp(_currentDistance, exploreDistance + _zoomOffset, Time.deltaTime * smoothSpeed);
                    _currentPitch = Mathf.Lerp(_currentPitch, explorePitch, Time.deltaTime * smoothSpeed);
                    _targetFOV = exploreFOV;
                    break;

                case GameState.Combat:
                {
                    // Prevent combat camera from drifting too far back due to exploration zoom offset.
                    float combatZoomOffset = Mathf.Clamp(_zoomOffset, -2f, 0f);
                    _currentDistance = Mathf.Lerp(_currentDistance, combatDistance + combatZoomOffset, Time.deltaTime * smoothSpeed);
                    _currentPitch = Mathf.Lerp(_currentPitch, combatPitch, Time.deltaTime * smoothSpeed);
                    _targetFOV = combatFOV;
                    break;
                }

                case GameState.Tuning:
                    _currentDistance = Mathf.Lerp(_currentDistance, tuningDistance, Time.deltaTime * smoothSpeed);
                    _currentPitch = Mathf.Lerp(_currentPitch, tuningPitch, Time.deltaTime * smoothSpeed);
                    _targetFOV = closeUpFOV;
                    break;

                case GameState.Cinematic:
                    // Lock camera during cinematics — hold current position, no lerp
                    break;

                case GameState.Paused:
                case GameState.Menu:
                case GameState.Boot:
                case GameState.Loading:
                    // Freeze camera movement during non-gameplay states
                    break;
            }
        }

        void HandleInput()
        {
            if (GameStateManager.Instance == null || GameStateManager.Instance.IsPaused) return;

            // Zoom via Input System action (mouse scroll + gamepad dpad — normalized + deadzone)
            float zoomInput = _zoomAction != null ? _zoomAction.ReadValue<float>() : 0f;
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                // Use gamepadZoomSpeed for gamepad, zoomSpeed for mouse
                bool isGamepad = Gamepad.current != null && (Gamepad.current.dpad.up.isPressed || Gamepad.current.dpad.down.isPressed);
                float speed = isGamepad ? gamepadZoomSpeed : zoomSpeed;
                _zoomOffset -= zoomInput * speed;
                _zoomOffset = Mathf.Clamp(_zoomOffset, zoomMin - exploreDistance, zoomMax - exploreDistance);
            }

            // Mouse orbit: middle-button + delta (direct read — modifier gating pattern)
            // Wired to SettingsOverlay _sens (TARTARIA_MouseSens) for production mouse feel.
            var mouse = Mouse.current;
            if (mouse != null && mouse.middleButton.isPressed)
            {
                float mouseX = mouse.delta.ReadValue().x * 0.1f;
                var kb = Keyboard.current;
                if (PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 0 || (kb == null || !kb.leftAltKey.isPressed)) // reduced motion skips free look shake
                    _currentYaw += mouseX * orbitSpeed * MouseSensitivityMultiplier * Time.deltaTime;
            }

            // Keyboard orbit: Q/E (direct read — dual-purpose keys shared with FrequencyShield/Interact)
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.qKey.isPressed)
                    _currentYaw -= orbitSpeed * Time.deltaTime;
                if (keyboard.eKey.isPressed)
                    _currentYaw += orbitSpeed * Time.deltaTime;
            }

            // Gamepad orbit — direct device read is authoritative (action-based read fails
            // when F310/XInputControllerWindows isn't paired to an InputUser, see 2026-06-03 R8).
            // Per Unity manual ActionAssets.html: code-side InputActions need either
            // InputUser pairing OR direct control read. We use the latter.
            Vector2 rightStick = Vector2.zero;
            var pad = Gamepad.current;
            if (pad != null)
            {
                // Read each axis explicitly — rightStick.ReadValue() goes through a Vector2
                // control that some HID-remapped devices fail to populate. Reading the X/Y
                // sub-controls directly bypasses that path and works for F310 in X-mode,
                // XInputControllerWindows, and Logitech-remapped HID gamepads.
                float rx = pad.rightStick.x.ReadValue();
                float ry = pad.rightStick.y.ReadValue();
                rightStick = new Vector2(rx, ry);
                if (rightStick.sqrMagnitude < 0.0064f) rightStick = Vector2.zero; // 0.08 deadzone
            }
            if (rightStick.sqrMagnitude > 0.01f)
            {
                // One-shot diagnostic per Play session so we know the stick is being read
                if (!_rightStickLogged)
                {
                    Debug.Log($"[CameraController] Right stick LIVE — first read: {rightStick}");
                    _rightStickLogged = true;
                }
                // Apply inversion settings (default: not inverted = push up looks up, push right orbits right)
                float yawInput = InvertCameraX ? -rightStick.x : rightStick.x;
                float pitchInput = InvertCameraY ? -rightStick.y : rightStick.y;

                _currentYaw += yawInput * gamepadOrbitSpeed * Time.deltaTime;
                _currentPitch = Mathf.Clamp(
                    _currentPitch + pitchInput * gamepadOrbitSpeed * 0.5f * Time.deltaTime,
                    20f, 80f);
            }
        }

        void ApplyCamera()
        {
            Vector3 lookPos = _lookTarget != null ? _lookTarget.position : followTarget.position;

            // Calculate camera position from spherical coordinates
            float pitchRad = _currentPitch * Mathf.Deg2Rad;
            float yawRad = _currentYaw * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
                Mathf.Sin(pitchRad),
                Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
            ) * _currentDistance;

            Vector3 targetPos = lookPos + offset;

            transform.position = Vector3.Lerp(
                transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(lookPos);

            // Periodic diag — fires on first frame, then every 60 frames (~1s at 60fps).
            // Lets us see the exact moment the camera math changes after the
            // Loading->Exploration transition.
            bool firstFrame = _diagCounter == 0;
            bool periodic = _diagCounter > 0 && (_diagCounter % 60) == 0;
            _diagCounter++;
            if (enableDiagnostics && (firstFrame || periodic))
            {
                var st = GameStateManager.Instance?.CurrentState;
                var pl = followTarget != null ? followTarget.position : Vector3.zero;
                var camParent = transform.parent != null ? transform.parent.name : "<none>";
                var camWorld = transform.position;
                Debug.Log($"[CameraController] DIAG f={_diagCounter - 1} state={st} pitch={_currentPitch:F1} yaw={_currentYaw:F1} dist={_currentDistance:F1} zoff={_zoomOffset:F1} look={lookPos} player={pl} camPos={camWorld} fwd={transform.forward} parent={camParent}");
            }
            if (enableDiagnostics && firstFrame)
            {
                // SCENE DIAG: dump ground/terrain mesh state so we can verify the
                // user actually has a visible floor under the camera.
                var gp = GameObject.Find("GroundPlane");
                if (gp == null)
                {
                    Debug.LogError("[CameraController] SCENE DIAG: GroundPlane NOT FOUND in scene.");
                }
                else
                {
                    var mf = gp.GetComponent<MeshFilter>();
                    var mr = gp.GetComponent<MeshRenderer>();
                    var mesh = mf != null ? mf.sharedMesh : null;
                    Vector3 firstNormal = Vector3.zero;
                    if (mesh != null && mesh.triangles.Length >= 3 && mesh.vertices.Length > 0)
                    {
                        var v0 = mesh.vertices[mesh.triangles[0]];
                        var v1 = mesh.vertices[mesh.triangles[1]];
                        var v2 = mesh.vertices[mesh.triangles[2]];
                        firstNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                    }
                    Debug.Log($"[CameraController] SCENE DIAG: GroundPlane pos={gp.transform.position} active={gp.activeInHierarchy} mr.enabled={(mr != null && mr.enabled)} mat={(mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.name : "<none>")} meshName={(mesh != null ? mesh.name : "<null>")} verts={(mesh != null ? mesh.vertexCount : 0)} tris={(mesh != null ? mesh.triangles.Length / 3 : 0)} bounds={(mesh != null ? mesh.bounds.ToString() : "<n/a>")} firstTriNormal={firstNormal}");
                }
                // Camera count + which is main
                var allCams = GameObject.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
                string camList = "";
                foreach (var cc in allCams)
                {
                    camList += $"  - {cc.gameObject.name} enabled={cc.enabled} depth={cc.depth} pos={cc.transform.position} display={cc.targetDisplay} mask={cc.cullingMask:X}\n";
                }
                Debug.Log($"[CameraController] SCENE DIAG: {allCams.Length} Camera(s); MainCamera={(UnityEngine.Camera.main != null ? UnityEngine.Camera.main.gameObject.name : "<null>")}\n{camList}");
            }

            // Smooth FOV transition
            if (_camera != null)
            {
                _camera.fieldOfView = Mathf.Lerp(
                    _camera.fieldOfView, _targetFOV, Time.deltaTime * smoothSpeed);
            }
        }

        // ─── Giant / Micro Mode ─────────────────────

        /// <summary>
        /// Switch camera to Giant Mode — extreme pull-back for oversized player.
        /// Called by GiantModeController when player grows to Tartarian scale.
        /// </summary>
        public void SetGiantMode(bool active)
        {
            if (active)
            {
                _currentDistance = zoomMax * 1.5f;
                _currentPitch = 65f;
                _targetFOV = 70f;
            }
            else
            {
                // Restore based on current game state
                UpdateCameraMode();
            }
        }

        /// <summary>
        /// Round 4 production Titan flight camera support (soaring chase, dynamic for giant input/physics).
        /// </summary>
        public void SetGiantFlight(bool flying)
        {
            if (flying)
            {
                _currentDistance = zoomMax * 2.2f;
                _currentPitch = 42f;
                _targetFOV = 78f;
            }
            else
            {
                _currentDistance = zoomMax * 1.5f;
                _currentPitch = 65f;
                _targetFOV = 70f;
            }
        }

        /// <summary>
        /// Switch camera to Micro Mode — extreme zoom-in for shrunken player.
        /// Called by MicroGiantController for ant-scale exploration segments.
        /// </summary>
        public void SetMicroMode(bool active)
        {
            if (active)
            {
                _currentDistance = 2f;
                _currentPitch = 30f;
                _targetFOV = 35f;
            }
            else
            {
                UpdateCameraMode();
            }
        }

        // ─── docs/15 §12 Camera Mode API (2026-06-03 C.L6) ────────────────────
        // 5 modes (Exploration / CloseUp / Tuning / Combat / Cinematic) with
        // per-mode CameraModeConfig. SetMode(...) interpolates the rig params
        // (distance / pitch derived from height / FOV / dampening) over
        // ModeBlendDuration so transitions feel smooth not snappy.
        //
        // Wired to GameEvents (see OnEnable). Cinematic mode also flips
        // IsCinematic so PlayerInputHandler can gate input independently of
        // GameStateManager (defense-in-depth: state-based gate AND mode-based
        // gate both deny input during cinematics).

        CameraModeConfig GetConfig(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.CloseUp:    return _cfgCloseUp;
                case CameraMode.Tuning:     return _cfgTuning;
                case CameraMode.Combat:     return _cfgCombat;
                case CameraMode.Cinematic:  return _cfgCinematic;
                case CameraMode.Exploration:
                default:                    return _cfgExploration;
            }
        }

        /// <summary>
        /// Switches the active camera mode. Interpolates distance / pitch / FOV /
        /// dampening from current to target over <paramref name="blendDuration"/>
        /// seconds (default 0.6s). Idempotent — no-op when already in <paramref name="mode"/>.
        /// </summary>
        public void SetMode(CameraMode mode, float blendDuration = ModeBlendDuration)
        {
            if (CurrentMode == mode) return;
            var prev = CurrentMode;
            CurrentMode = mode;
            _currentMode = mode;
            if (_modeBlendCo != null) StopCoroutine(_modeBlendCo);
            _modeBlendCo = StartCoroutine(BlendToMode(mode, blendDuration));
            Debug.Log($"[CameraController] Mode {prev} → {mode} (blend {blendDuration:F2}s)");
        }

        System.Collections.IEnumerator BlendToMode(CameraMode mode, float blendDuration)
        {
            var cfg = GetConfig(mode);

            // Source values for the lerp — read current rig state at start of blend.
            float fromDistance = _currentDistance;
            float fromPitch = _currentPitch;
            float fromFOV = _targetFOV;
            float fromSmoothSpeed = smoothSpeed;

            // Map dampening (0 = snappy, 1 = molasses) into smoothSpeed
            // (high smoothSpeed = faster Lerp). Inverse mapping: 12 → 2.4.
            float toSmoothSpeed = Mathf.Lerp(12f, 2.4f, Mathf.Clamp01(cfg.dampening));

            // Derive a pitch from "height" — height is the camera-target Y offset
            // relative to follow target. Convert into a pitch angle for the existing
            // spherical-coord rig. distance>0 always for modes that follow.
            float toPitch = cfg.distance > 0.01f
                ? Mathf.Rad2Deg * Mathf.Atan2(cfg.height, cfg.distance)
                : _currentPitch;
            // Clamp pitch into rig range (20°-80°).
            toPitch = Mathf.Clamp(toPitch + 22f, 20f, 80f); // +22° offset to keep horizon mid-screen

            float toDistance = cfg.distance > 0.01f ? cfg.distance : _currentDistance;
            float toFOV = cfg.fov;

            float elapsed = 0f;
            float dur = Mathf.Max(0.01f, blendDuration);
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = t * t * (3f - 2f * t); // smoothstep
                _currentDistance = Mathf.Lerp(fromDistance, toDistance, s);
                _currentPitch    = Mathf.Lerp(fromPitch, toPitch, s);
                _targetFOV       = Mathf.Lerp(fromFOV, toFOV, s);
                smoothSpeed      = Mathf.Lerp(fromSmoothSpeed, toSmoothSpeed, s);
                yield return null;
            }
            _currentDistance = toDistance;
            _currentPitch = toPitch;
            _targetFOV = toFOV;
            smoothSpeed = toSmoothSpeed;
            _modeBlendCo = null;
        }

        // ─── Event handlers (docs/15 §12 wiring) ───
        void HandleTuningStart()   => SetMode(CameraMode.Tuning);
        void HandleTuningEnd()     => SetMode(CameraMode.Exploration);
        void HandleCombatEngaged() => SetMode(CameraMode.Combat);
        void HandleCombatEnded()   => SetMode(CameraMode.Exploration);
        void HandleCinematicStart()=> SetMode(CameraMode.Cinematic);
        void HandleCinematicEnd()  => SetMode(CameraMode.Exploration);

        void HandleBuildingRestored(string buildingId)
        {
            // Snap to CloseUp to celebrate the restored building, then auto-return
            // to Exploration after 4s. Cancels any prior auto-return coroutine.
            if (_closeUpAutoReturnCo != null) StopCoroutine(_closeUpAutoReturnCo);
            SetMode(CameraMode.CloseUp);
            _closeUpAutoReturnCo = StartCoroutine(CloseUpAutoReturn(4f));
        }

        System.Collections.IEnumerator CloseUpAutoReturn(float holdSeconds)
        {
            yield return new WaitForSecondsRealtime(holdSeconds);
            // Only auto-return if we're still in CloseUp — a subsequent event
            // (combat / tuning / cinematic) may have already moved us elsewhere.
            if (CurrentMode == CameraMode.CloseUp)
                SetMode(CameraMode.Exploration);
            _closeUpAutoReturnCo = null;
        }

        /// <summary>
        /// Triggers a brief positional camera shake (does not disrupt follow target).
        /// </summary>
        public void TriggerShake(float intensity = 0.3f, float duration = 0.45f)
        {
            StartCoroutine(ShakeSequence(intensity, duration));
        }

        System.Collections.IEnumerator ShakeSequence(float intensity, float duration)
        {
            float elapsed = 0f;
            Vector3 origin = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float envelope = 1f - Mathf.Clamp01(elapsed / duration); // fade out
                float ox = (UnityEngine.Random.value * 2f - 1f) * intensity * envelope;
                float oy = (UnityEngine.Random.value * 2f - 1f) * intensity * envelope * 0.5f;
                transform.position = origin + new Vector3(ox, oy, 0f);
                yield return null;
            }
            transform.position = origin;
        }

        /// <summary>
        /// Triggers a close-up shot at a specific position (building discovery, etc.)
        /// Skips if a close-up is already in progress to prevent state corruption
        /// from multiple simultaneous building discoveries.
        /// </summary>
        public void FocusOnPoint(Vector3 worldPoint, float duration = 2f)
        {
            if (_closeUpCoroutine != null) return; // Already in a close-up
            _closeUpCoroutine = StartCoroutine(CloseUpSequence(worldPoint, duration));
        }

        System.Collections.IEnumerator CloseUpSequence(Vector3 point, float duration)
        {
            var currentState = GameStateManager.Instance?.CurrentState ?? GameState.Exploration;
            // Never save Cinematic/Boot/Loading as return state — always fall back to Exploration
            _preCloseUpState = (currentState == GameState.Exploration || currentState == GameState.Combat || currentState == GameState.Tuning)
                ? currentState : GameState.Exploration;
            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);

            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 endPos = point + Vector3.up * closeUpDistance + Vector3.back * closeUpDistance * 0.5f;
            Quaternion endRot = Quaternion.LookRotation(point - endPos);

            while (elapsed < duration * 0.4f) // Ease in
            {
                float t = elapsed / (duration * 0.4f);
                t = t * t * (3f - 2f * t); // Smoothstep

                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(duration * 0.3f); // Hold

            elapsed = 0f;
            while (elapsed < duration * 0.3f) // Ease out
            {
                float t = elapsed / (duration * 0.3f);
                transform.position = Vector3.Lerp(endPos, startPos, t);
                transform.rotation = Quaternion.Slerp(endRot, startRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            GameStateManager.Instance?.TransitionTo(_preCloseUpState);
            _closeUpCoroutine = null;
        }
    }
}
