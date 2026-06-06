using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Tartaria.UI
{
    /// <summary>
    /// Dynamic Button Prompts — shows KB or gamepad icons based on active input device.
    /// Updates automatically when the player switches between keyboard and controller.
    /// Accessibility: ensures players always see relevant prompts for their input method.
    /// </summary>
    public class DynamicButtonPrompts : MonoBehaviour
    {
        public static DynamicButtonPrompts Instance { get; private set; }

        [Header("Prompt Icons")]
        [SerializeField] Sprite keyboardInteractIcon;
        [SerializeField] Sprite gamepadInteractIcon;
        [SerializeField] Sprite keyboardAttackIcon;
        [SerializeField] Sprite gamepadAttackIcon;
        [SerializeField] Sprite keyboardPauseIcon;
        [SerializeField] Sprite gamepadPauseIcon;

        [Header("Active Prompts")]
        [SerializeField] Image interactPromptImage;
        [SerializeField] TextMeshProUGUI interactPromptText;
        [SerializeField] Image attackPromptImage;
        [SerializeField] Image pausePromptImage;

        InputDevice _lastUsedDevice;
        bool _isGamepad;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Subscribe to input device changes
            InputSystem.onActionChange += OnActionChange;
            DetectActiveDevice();
            UpdatePrompts();
        }

        void OnDestroy()
        {
            InputSystem.onActionChange -= OnActionChange;
            if (Instance == this) Instance = null;
        }

        void OnActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                DetectActiveDevice();
            }
        }

        void DetectActiveDevice()
        {
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool wasGamepad = _isGamepad;

            if (gamepad != null && gamepad.wasUpdatedThisFrame)
            {
                _isGamepad = true;
                _lastUsedDevice = gamepad;
            }
            else if ((keyboard != null && keyboard.wasUpdatedThisFrame) || 
                     (mouse != null && mouse.wasUpdatedThisFrame))
            {
                _isGamepad = false;
                _lastUsedDevice = keyboard ?? (InputDevice)mouse;
            }

            if (wasGamepad != _isGamepad)
            {
                UpdatePrompts();
                Debug.Log($"[ButtonPrompts] Input device switched to {(_isGamepad ? "Gamepad" : "Keyboard/Mouse")}");
            }
        }

        void UpdatePrompts()
        {
            // Update interact prompt
            if (interactPromptImage != null)
            {
                interactPromptImage.sprite = _isGamepad ? gamepadInteractIcon : keyboardInteractIcon;
                interactPromptImage.SetNativeSize();
            }

            if (interactPromptText != null)
            {
                interactPromptText.text = _isGamepad ? "[A] Interact" : "[E] Interact";
            }

            // Update attack prompt
            if (attackPromptImage != null)
            {
                attackPromptImage.sprite = _isGamepad ? gamepadAttackIcon : keyboardAttackIcon;
                attackPromptImage.SetNativeSize();
            }

            // Update pause prompt
            if (pausePromptImage != null)
            {
                pausePromptImage.sprite = _isGamepad ? gamepadPauseIcon : keyboardPauseIcon;
                pausePromptImage.SetNativeSize();
            }

            // Apply accessibility scaling
            ApplyAccessibilityScaling();
        }

        void ApplyAccessibilityScaling()
        {
            if (AccessibilityManager.Instance == null) return;

            float textScale = AccessibilityManager.Instance.TextScale;
            float buttonSize = AccessibilityManager.Instance.ButtonSizeMultiplier;

            // Scale prompt text
            if (interactPromptText != null)
            {
                var baseSize = 16f;
                interactPromptText.fontSize = baseSize * textScale;
            }

            // Scale prompt icons
            if (interactPromptImage != null)
            {
                var rt = interactPromptImage.GetComponent<RectTransform>();
                if (rt != null) rt.localScale = Vector3.one * buttonSize;
            }
        }

        /// <summary>
        /// Get the appropriate icon for a given action name.
        /// </summary>
        public Sprite GetIconForAction(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "interact":
                    return _isGamepad ? gamepadInteractIcon : keyboardInteractIcon;
                case "attack":
                case "resonancepulse":
                    return _isGamepad ? gamepadAttackIcon : keyboardAttackIcon;
                case "pause":
                    return _isGamepad ? gamepadPauseIcon : keyboardPauseIcon;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the appropriate text label for a given action.
        /// </summary>
        public string GetLabelForAction(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "interact":
                    return _isGamepad ? "[A] Interact" : "[E] Interact";
                case "attack":
                    return _isGamepad ? "[X] Attack" : "[LMB] Attack";
                case "pause":
                    return _isGamepad ? "[Start] Pause" : "[ESC] Pause";
                case "sprint":
                    return _isGamepad ? "[L3] Sprint" : "[Shift] Sprint";
                case "scan":
                    return _isGamepad ? "[Y] Scan" : "[Space] Scan";
                default:
                    return actionName;
            }
        }
    }
}
