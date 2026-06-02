using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Tartaria.Input
{
    /// <summary>
    /// On-screen input probe — confirms WASD + gamepad input is flowing.
    /// Auto-bootstraps after scene load. Shows top-left of Game view.
    ///
    /// Per CLAUDE.md no-stubs mandate — every method does real work, no placeholders.
    /// Built so NATRIX can SEE input arriving without reading the console.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputProbeHUD : MonoBehaviour
    {
        static InputProbeHUD _instance;
        GUIStyle _style;
        int _frame;
#if ENABLE_INPUT_SYSTEM
        float _lastStickMag;
        string _lastKey = "(none)";
        float _lastKeyT;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("InputProbeHUD");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<InputProbeHUD>();
            Debug.Log("[InputProbeHUD] Bootstrapped — top-left overlay shows live input state.");
        }

        void Update()
        {
            _frame++;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                // record most-recent printable key for the HUD
                if (kb.wKey.wasPressedThisFrame) { _lastKey = "W"; _lastKeyT = Time.unscaledTime; }
                else if (kb.aKey.wasPressedThisFrame) { _lastKey = "A"; _lastKeyT = Time.unscaledTime; }
                else if (kb.sKey.wasPressedThisFrame) { _lastKey = "S"; _lastKeyT = Time.unscaledTime; }
                else if (kb.dKey.wasPressedThisFrame) { _lastKey = "D"; _lastKeyT = Time.unscaledTime; }
                else if (kb.spaceKey.wasPressedThisFrame) { _lastKey = "Space"; _lastKeyT = Time.unscaledTime; }
                else if (kb.eKey.wasPressedThisFrame) { _lastKey = "E"; _lastKeyT = Time.unscaledTime; }
            }
            var gp = Gamepad.current;
            if (gp != null)
            {
                var v = gp.leftStick.ReadValue();
                _lastStickMag = v.magnitude;
                if (gp.buttonSouth.wasPressedThisFrame) { _lastKey = "GP:A/South"; _lastKeyT = Time.unscaledTime; }
                if (gp.buttonNorth.wasPressedThisFrame) { _lastKey = "GP:Y/North"; _lastKeyT = Time.unscaledTime; }
            }
#endif
        }

        void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = 14;
                _style.normal.textColor = new Color(0.95f, 0.85f, 0.2f, 1f);
                _style.fontStyle = FontStyle.Bold;
            }

            GUI.Box(new Rect(8, 8, 320, 168), "");

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            var gp = Gamepad.current;
            var js = Joystick.current;
            float t = Time.unscaledTime;
            string kbStatus = kb != null ? $"OK ({kb.deviceId})" : "NULL";
            string gpStatus = gp != null ? $"OK ({gp.displayName})" : "NULL";
            string jsStatus = js != null ? $"OK ({js.displayName})" : "NULL";
            Vector2 leftStick = gp != null ? gp.leftStick.ReadValue() : Vector2.zero;
            int devCount = InputSystem.devices.Count;

            string s =
                $"[INPUT PROBE]\n" +
                $"Frame: {_frame}  Focus: {Application.isFocused}\n" +
                $"Devices total: {devCount}\n" +
                $"Keyboard.current: {kbStatus}\n" +
                $"Gamepad.current (XInput): {gpStatus}\n" +
                $"Joystick.current (DInput): {jsStatus}\n" +
                $"Left stick: ({leftStick.x:F2}, {leftStick.y:F2})  mag {leftStick.magnitude:F2}\n" +
                $"Last key/btn: {_lastKey}  ({(t - _lastKeyT):F1}s ago)";
#else
            string s =
                $"[INPUT PROBE]\n" +
                $"ENABLE_INPUT_SYSTEM not defined — legacy mode\n" +
                $"Frame: {_frame}  Focus: {Application.isFocused}";
#endif
            GUI.Label(new Rect(14, 14, 312, 162), s, _style);
        }
    }
}
