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
        [SerializeField, Tooltip("Camera distance in exploration mode")] float exploreDistance = 15f;
        [SerializeField, Tooltip("Camera pitch angle in exploration mode")] float explorePitch = 45f;
        [SerializeField, Tooltip("Field of view in exploration mode")] float exploreFOV = 50f;

        [Header("Combat Mode")]
        [SerializeField, Tooltip("Camera distance in combat mode")] float combatDistance = 20f;
        [SerializeField, Tooltip("Camera pitch angle in combat mode")] float combatPitch = 50f;
        [SerializeField, Tooltip("Field of view in combat mode")] float combatFOV = 60f;

        [Header("Close-Up Mode")]
        [SerializeField, Tooltip("Camera distance for close-up inspections")] float closeUpDistance = 5f;
        [SerializeField, Tooltip("Field of view for close-up inspections")] float closeUpFOV = 40f;

        [Header("Tuning Mode")]
        [SerializeField, Tooltip("Camera distance during building tuning")] float tuningDistance = 8f;
        [SerializeField, Tooltip("Camera pitch angle during building tuning")] float tuningPitch = 70f;

        [Header("Controls")]
        [SerializeField, Tooltip("Mouse scroll zoom speed")] float zoomSpeed = 3f;
        [SerializeField, Tooltip("Minimum allowed zoom distance")] float zoomMin = 5f;
        [SerializeField, Tooltip("Maximum allowed zoom distance")] float zoomMax = 25f;
        [SerializeField, Tooltip("Camera orbit rotation speed (degrees/sec)")] float orbitSpeed = 120f;
        [SerializeField, Tooltip("Gamepad right-stick orbit sensitivity")] float gamepadOrbitSpeed = 150f;
        [SerializeField, Tooltip("Gamepad zoom speed (D-pad / right shoulder)")] float gamepadZoomSpeed = 8f;
        [SerializeField, Tooltip("Camera movement interpolation speed")] float smoothSpeed = 8f;

        UnityEngine.Camera _camera;
        float _currentDistance;
        float _currentPitch;
        float _currentYaw;
        float _targetFOV;
        float _zoomOffset;
        Coroutine _closeUpCoroutine;
        GameState _preCloseUpState;
        float _playerSearchCooldown;

        void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera == null)
                _camera = GetComponentInChildren<UnityEngine.Camera>();
            if (_camera == null)
                Debug.LogError("[Camera] No Camera component found on CameraController or children.");

            _currentDistance = exploreDistance;
            _currentPitch = explorePitch;
            _targetFOV = exploreFOV;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (_closeUpCoroutine != null)
                GameStateManager.Instance?.TransitionTo(_preCloseUpState);
        }

        void LateUpdate()
        {
            if (followTarget == null)
            {
                _playerSearchCooldown -= Time.deltaTime;
                if (_playerSearchCooldown > 0f) return;
                _playerSearchCooldown = 0.5f;
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    followTarget = player.transform;
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
                    _currentDistance = Mathf.Lerp(_currentDistance, combatDistance + _zoomOffset, Time.deltaTime * smoothSpeed);
                    _currentPitch = Mathf.Lerp(_currentPitch, combatPitch, Time.deltaTime * smoothSpeed);
                    _targetFOV = combatFOV;
                    break;

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
            if (GameStateManager.Instance.IsPaused) return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            // Scroll wheel = zoom
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y / 120f;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _zoomOffset -= scroll * zoomSpeed;
                    _zoomOffset = Mathf.Clamp(_zoomOffset, zoomMin - exploreDistance, zoomMax - exploreDistance);
                }

                // Middle mouse = orbit
                if (mouse.middleButton.isPressed)
                {
                    float mouseX = mouse.delta.ReadValue().x * 0.1f;
                    _currentYaw += mouseX * orbitSpeed * Time.deltaTime;
                }
            }

            // Q/E = orbit
            if (keyboard != null)
            {
                if (keyboard.qKey.isPressed)
                    _currentYaw -= orbitSpeed * Time.deltaTime;
                if (keyboard.eKey.isPressed)
                    _currentYaw += orbitSpeed * Time.deltaTime;
            }

            // Gamepad right stick = orbit (read from PlayerInputHandler action map)
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 rightStick = gamepad.rightStick.ReadValue();
                if (rightStick.sqrMagnitude > 0.02f)
                {
                    _currentYaw += rightStick.x * gamepadOrbitSpeed * Time.deltaTime;
                    // Right stick Y adjusts pitch within bounds
                    _currentPitch = Mathf.Clamp(
                        _currentPitch - rightStick.y * gamepadOrbitSpeed * 0.5f * Time.deltaTime,
                        20f, 80f);
                }

                // D-pad Y / right shoulder = zoom
                float dpadY = gamepad.dpad.y.ReadValue();
                if (Mathf.Abs(dpadY) > 0.1f)
                {
                    _zoomOffset -= dpadY * gamepadZoomSpeed * Time.deltaTime;
                    _zoomOffset = Mathf.Clamp(_zoomOffset, zoomMin - exploreDistance, zoomMax - exploreDistance);
                }
            }
        }

        void ApplyCamera()
        {
            // Calculate camera position from spherical coordinates
            float pitchRad = _currentPitch * Mathf.Deg2Rad;
            float yawRad = _currentYaw * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
                Mathf.Sin(pitchRad),
                Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
            ) * _currentDistance;

            Vector3 targetPos = followTarget.position + offset;

            transform.position = Vector3.Lerp(
                transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(followTarget.position);

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
        /// Triggers a close-up shot at a specific position (building discovery, etc.)
        /// </summary>
        public void FocusOnPoint(Vector3 worldPoint, float duration = 2f)
        {
            // Will integrate with Cinemachine virtual cameras
            _closeUpCoroutine = StartCoroutine(CloseUpSequence(worldPoint, duration));
        }

        System.Collections.IEnumerator CloseUpSequence(Vector3 point, float duration)
        {
            _preCloseUpState = GameStateManager.Instance?.CurrentState ?? GameState.Exploration;
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
