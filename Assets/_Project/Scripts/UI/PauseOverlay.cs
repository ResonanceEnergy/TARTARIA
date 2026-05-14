using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Pause Overlay — Esc / gamepad Start opens an IMGUI pause menu with
    /// RESUME / QUICK SAVE / QUICK LOAD / SETTINGS / QUIT TO DESKTOP. Pauses
    /// Time.timeScale while open. Hidden in the Boot scene while the main menu
    /// is up so the two overlays don't fight each other.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseOverlay : MonoBehaviour
    {
        public static PauseOverlay Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PauseOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<PauseOverlay>();
        }

        bool _open;
        float _restoreTimeScale = 1f;
        int  _selectedIndex;            // gamepad nav: which button is highlighted
        float _navCooldown;             // d-pad/stick repeat throttle
        const int ButtonCount = 5;      // RESUME / SAVE / LOAD / SETTINGS / QUIT

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Don't compete with the main menu in the Boot scene.
            if (GameBootstrap.MainMenuActive) return;

            bool kb = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool gp = Gamepad.current  != null && Gamepad.current.startButton.wasPressedThisFrame;
            if (kb || gp) Toggle();

            if (!_open) return;

            // Gamepad navigation while paused.
            var pad = Gamepad.current;
            if (pad != null)
            {
                _navCooldown -= Time.unscaledDeltaTime;

                float vy = pad.leftStick.ReadValue().y;
                bool down = pad.dpad.down.wasPressedThisFrame  || (vy < -0.55f && _navCooldown <= 0f);
                bool up   = pad.dpad.up.wasPressedThisFrame    || (vy >  0.55f && _navCooldown <= 0f);

                if (down) { _selectedIndex = (_selectedIndex + 1) % ButtonCount; _navCooldown = 0.18f; }
                if (up)   { _selectedIndex = (_selectedIndex - 1 + ButtonCount) % ButtonCount; _navCooldown = 0.18f; }

                if (pad.buttonSouth.wasPressedThisFrame) ActivateSelected();
                if (pad.buttonEast.wasPressedThisFrame)  Close(); // B = back/resume
            }

            // Keyboard arrow nav too.
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex + 1) % ButtonCount;
                if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex - 1 + ButtonCount) % ButtonCount;
                if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)
                    ActivateSelected();
            }
        }

        void ActivateSelected()
        {
            switch (_selectedIndex)
            {
                case 0: Close(); break;
                case 1: CallSaveManager("QuickSave"); break;
                case 2: CallSaveManager("QuickLoad"); break;
                case 3: SettingsOverlay.Open(); break;
                case 4:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        public void Toggle()
        {
            if (_open) Close();
            else Open();
        }

        public void Open()
        {
            if (_open) return;
            _open = true;
            _selectedIndex = 0;
            _navCooldown = 0f;
            _restoreTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        public void Close()
        {
            if (!_open) return;
            _open = false;
            Time.timeScale = _restoreTimeScale > 0f ? _restoreTimeScale : 1f;
        }

        void OnGUI()
        {
            if (!_open) return;

            // Dim background.
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;

            const int W = 400, H = 360;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;
            GUI.Box(new Rect(x, y, W, H), "");

            var title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 28, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.92f, 0.6f) }
            };
            GUI.Label(new Rect(x, y + 16, W, 40), "PAUSED", title);

            var btn = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 40 };
            var sel = new GUIStyle(btn);
            sel.normal.textColor = new Color(1f, 0.95f, 0.55f);
            sel.fontStyle = FontStyle.Bold;
            sel.normal.background = sel.active.background; // visually pressed-look for highlight
            const int bw = 260, bh = 40, gap = 10;
            int bx = x + (W - bw) / 2;
            int by = y + 70;

            string Mark(int i, string label) => _selectedIndex == i ? "> " + label + " <" : label;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(0, "RESUME"), _selectedIndex == 0 ? sel : btn)) { _selectedIndex = 0; Close(); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(1, "QUICK SAVE"), _selectedIndex == 1 ? sel : btn))
            { _selectedIndex = 1; CallSaveManager("QuickSave"); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(2, "QUICK LOAD"), _selectedIndex == 2 ? sel : btn))
            { _selectedIndex = 2; CallSaveManager("QuickLoad"); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(3, "SETTINGS"), _selectedIndex == 3 ? sel : btn))
            { _selectedIndex = 3; SettingsOverlay.Open(); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(4, "QUIT TO DESKTOP"), _selectedIndex == 4 ? sel : btn))
            {
                _selectedIndex = 4;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            // Footer hint — make controls discoverable for both schemes.
            var hint = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 12,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
            };
            string nav  = Tartaria.Input.InputPromptHelper.GamepadActive ? "D-Pad / L-Stick: Navigate" : "Arrows / WS: Navigate";
            string ok   = Tartaria.Input.InputPromptHelper.GamepadActive ? "A: Select" : "Enter: Select";
            string back = Tartaria.Input.InputPromptHelper.GamepadActive ? "B / Start: Resume" : "Esc: Resume";
            GUI.Label(new Rect(x, y + H - 26, W, 20), $"{nav}    {ok}    {back}", hint);
        }

        // Reflection so UI doesn't need an asmdef ref to Save.
        static void CallSaveManager(string method)
        {
            try
            {
                var t = System.Type.GetType("Tartaria.Save.SaveManager, Tartaria.Save");
                if (t == null) return;
                var inst = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                if (inst == null) return;
                var m = t.GetMethod(method, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                m?.Invoke(inst, null);
            }
            catch (System.Exception ex) { Debug.LogWarning($"[PauseOverlay] CallSaveManager({method}) failed: {ex.Message}"); }
        }
    }
}
