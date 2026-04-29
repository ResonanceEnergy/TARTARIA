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
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField, Tooltip("Transform to follow and orbit around")] Transform followTarget;

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
        Coroutine _closeUpCoroutine;
        GameState _preCloseUpState;
        float _playerSearchCooldown;
        int _diagCounter;

        // Camera-local InputAction instances — avoids shared-state issues with PlayerInputHandler's clone
        InputAction _zoomAction;
        InputAction _gamepadOrbitAction;

        void Awake()
        {
            // Runtime safety: older scenes may carry stale serialized camera values
            // (18m distance / 55 deg pitch). Clamp to the intended tighter framing.
            exploreDistance = 9f;
            explorePitch = 28f;
            exploreFOV = 65f;
            combatDistance = 12f;
            combatPitch = 32f;
            combatFOV = 65f;
            zoomMin = 4f;
            zoomMax = 18f;

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
        }

        void OnEnable()
        {
            _zoomAction = new InputAction("CameraZoom", InputActionType.Value);
            _zoomAction.AddBinding("<Mouse>/scroll/y")
                .WithProcessor("Normalize(min=-120,max=120)");
            _zoomAction.AddBinding("<Gamepad>/dpad/y");
            _zoomAction.Enable();

            _gamepadOrbitAction = new InputAction("GamepadOrbit", InputActionType.Value,
                binding: "<Gamepad>/rightStick",
                processors: "StickDeadzone(min=0.15)");
            _gamepadOrbitAction.Enable();
        }

        void OnDisable()
        {
            _zoomAction?.Disable();
            _zoomAction?.Dispose();
            _zoomAction = null;
            _gamepadOrbitAction?.Disable();
            _gamepadOrbitAction?.Dispose();
            _gamepadOrbitAction = null;
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
                _zoomOffset -= zoomInput * zoomSpeed;
                _zoomOffset = Mathf.Clamp(_zoomOffset, zoomMin - exploreDistance, zoomMax - exploreDistance);
            }

            // Mouse orbit: middle-button + delta (direct read — modifier gating pattern)
            var mouse = Mouse.current;
            if (mouse != null && mouse.middleButton.isPressed)
            {
                float mouseX = mouse.delta.ReadValue().x * 0.1f;
                _currentYaw += mouseX * orbitSpeed * Time.deltaTime;
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

            // Gamepad orbit via Input System action (right stick with deadzone processor)
            Vector2 rightStick = _gamepadOrbitAction != null ? _gamepadOrbitAction.ReadValue<Vector2>() : Vector2.zero;
            if (rightStick.sqrMagnitude > 0.02f)
            {
                _currentYaw += rightStick.x * gamepadOrbitSpeed * Time.deltaTime;
                _currentPitch = Mathf.Clamp(
                    _currentPitch - rightStick.y * gamepadOrbitSpeed * 0.5f * Time.deltaTime,
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
