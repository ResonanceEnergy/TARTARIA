using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Input
{
    /// <summary>
    /// Single source of truth for action-prompt glyphs. Returns "[A] Interact"
    /// when a gamepad has been used recently, "[E] Interact" when the keyboard
    /// has been used most recently. Auto-detects on every input event so
    /// hot-swapping a controller mid-play just works.
    /// </summary>
    public static class InputPromptHelper
    {
        /// <summary>True when gamepad was the last device touched.</summary>
        public static bool GamepadActive { get; private set; }

        // Standard Xbox/Logitech XInput button labels — match what's mapped
        // in TartariaInputActions.inputactions and the gamepad audit doc.
        public static string Interact      => GamepadActive ? "[A]" : "[E]";
        public static string Confirm       => GamepadActive ? "[A]" : "[Enter]";
        public static string Back          => GamepadActive ? "[B]" : "[Esc]";
        public static string Pulse         => GamepadActive ? "[X]" : "[Space]";
        public static string Strike        => GamepadActive ? "[Y]" : "[Ctrl]";
        public static string Scan          => GamepadActive ? "[B]" : "[G]";
        public static string Shield        => GamepadActive ? "[LB]" : "[Q]";
        public static string AetherVision  => GamepadActive ? "[RT]" : "[Tab]";
        public static string Pause         => GamepadActive ? "[Start]" : "[Esc]";
        public static string Map           => GamepadActive ? "[Select]" : "[M]";
        public static string Sprint        => GamepadActive ? "[L3]" : "[Shift]";
        public static string Inventory     => GamepadActive ? "[Select]" : "[I]";

        /// <summary>
        /// Replace bracketed keyboard tokens in a string with the active scheme's
        /// equivalent. Cheap to call from GetInteractPrompt() on every frame.
        /// </summary>
        public static string Localize(string raw)
        {
            if (string.IsNullOrEmpty(raw) || !GamepadActive) return raw;

            // Most common interact prompt.
            if (raw.IndexOf("[E]") >= 0) raw = raw.Replace("[E]", "[A]");
            if (raw.IndexOf("[Q]") >= 0) raw = raw.Replace("[Q]", "[LB]");
            if (raw.IndexOf("[Tab]") >= 0) raw = raw.Replace("[Tab]", "[RT]");
            if (raw.IndexOf("[Space]") >= 0) raw = raw.Replace("[Space]", "[X]");
            if (raw.IndexOf("[Ctrl]") >= 0) raw = raw.Replace("[Ctrl]", "[Y]");
            if (raw.IndexOf("[G]") >= 0) raw = raw.Replace("[G]", "[B]");
            if (raw.IndexOf("[Esc]") >= 0) raw = raw.Replace("[Esc]", "[Start]");
            if (raw.IndexOf("[Enter]") >= 0) raw = raw.Replace("[Enter]", "[A]");
            return raw;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // Default: gamepad active iff one is already plugged in.
            GamepadActive = Gamepad.current != null;

            InputSystem.onEvent += OnInputEvent;
            InputSystem.onDeviceChange += OnDeviceChange;
            Debug.Log($"[InputPrompt] Initialized — GamepadActive={GamepadActive}");
        }

        static void OnInputEvent(UnityEngine.InputSystem.LowLevel.InputEventPtr evt, InputDevice device)
        {
            if (device == null) return;

            // Ignore pure cursor jitter — we only flip the scheme on real input.
            // Any keyboard key press → keyboard mode.
            if (device is Keyboard)
            {
                if (GamepadActive) { GamepadActive = false; }
                return;
            }
            // Any gamepad button / stick press → gamepad mode.
            if (device is Gamepad gp)
            {
                // Only flip on actual user intent (button press OR strong stick deflection).
                bool flip = false;
                if (gp.leftStick.ReadValue().sqrMagnitude > 0.25f) flip = true;
                else if (gp.rightStick.ReadValue().sqrMagnitude > 0.25f) flip = true;
                else if (AnyButtonHeld(gp)) flip = true;

                if (flip && !GamepadActive) GamepadActive = true;
            }
        }

        static bool AnyButtonHeld(Gamepad gp)
        {
            return gp.buttonSouth.isPressed || gp.buttonNorth.isPressed ||
                   gp.buttonEast.isPressed  || gp.buttonWest.isPressed  ||
                   gp.leftShoulder.isPressed || gp.rightShoulder.isPressed ||
                   gp.leftTrigger.isPressed  || gp.rightTrigger.isPressed ||
                   gp.startButton.isPressed  || gp.selectButton.isPressed ||
                   gp.dpad.up.isPressed      || gp.dpad.down.isPressed    ||
                   gp.dpad.left.isPressed    || gp.dpad.right.isPressed;
        }

        static void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added && device is Gamepad)
            {
                Debug.Log($"[InputPrompt] Gamepad connected: {device.displayName}");
            }
            else if (change == InputDeviceChange.Removed && device is Gamepad)
            {
                Debug.Log($"[InputPrompt] Gamepad disconnected: {device.displayName}");
                if (Gamepad.current == null) GamepadActive = false;
            }
        }
    }
}
