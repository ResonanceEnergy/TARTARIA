using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// M2: Basic PauseMenu foundation (toggle with Esc).
    /// Options: Resume, Settings (wired to existing SettingsOverlay), Save & Quit, Quit to Menu.
    /// Uses IMGUI for rapid foundation (matches PauseOverlay style). 
    /// Displays simple "Save Slot: X" label.
    /// Coordinates with GameBootstrap.LoadMainMenu() for return-to-menu flows.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PauseMenu");
            DontDestroyOnLoad(go);
            go.AddComponent<PauseMenu>();
        }

        bool _open;
        float _restoreTimeScale = 1f;
        int _selectedIndex; // 0=Resume, 1=Settings, 2=Save Game, 3=Save & Quit
        float _navCooldown;
        const int ButtonCount = 4;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Tartaria.Input.PlayerInputHandler.OnPauseToggled += HandlePauseToggled;
        }

        void OnDestroy()
        {
            Tartaria.Input.PlayerInputHandler.OnPauseToggled -= HandlePauseToggled;
            if (Instance == this) Instance = null;
        }

        void HandlePauseToggled()
        {
            if (GameBootstrap.MainMenuActive) return;
            if (SettingsOverlay.IsOpen) return;
            Toggle();
        }

        void Update()
        {
            // Don't compete with main menu in Boot scene.
            if (GameBootstrap.MainMenuActive) return;

            bool kb = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool gp = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            if (kb || gp) Toggle();

            if (!_open) return;

            // Simple keyboard/gamepad nav (basic foundation)
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

            var pad = Gamepad.current;
            if (pad != null)
            {
                if (_navCooldown <= 0f)
                {
                    if (pad.dpad.down.wasPressedThisFrame || pad.leftStick.ReadValue().y < -0.6f)
                    {
                        _selectedIndex = (_selectedIndex + 1) % ButtonCount;
                        _navCooldown = 0.18f;
                    }
                    if (pad.dpad.up.wasPressedThisFrame || pad.leftStick.ReadValue().y > 0.6f)
                    {
                        _selectedIndex = (_selectedIndex - 1 + ButtonCount) % ButtonCount;
                        _navCooldown = 0.18f;
                    }
                }
                else
                {
                    _navCooldown -= Time.unscaledDeltaTime;
                }

                if (pad.buttonSouth.wasPressedThisFrame) ActivateSelected();
                if (pad.buttonEast.wasPressedThisFrame) Close();
            }
        }

        void ActivateSelected()
        {
            switch (_selectedIndex)
            {
                case 0: Close(); break;                           // Resume
                case 1: SettingsOverlay.Open(); Close(); break;   // Settings
                case 2:                                           // Save Game (quick save, stay in game)
                    ServiceLocator.Save?.Save();
                    Close();
                    break;
                case 3:                                           // Save & Quit to Menu
                    ServiceLocator.Save?.Save();
                    GameBootstrap.LoadMainMenu();
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

            const int W = 420, H = 340;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            // Strong Tartarian dim background
            GUI.color = new Color(0, 0, 0, 0.82f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Box(new Rect(x, y, W, H), "PAUSE");
            GUI.Box(new Rect(x + 3, y + 3, W - 6, H - 6), ""); // inner border for weight

            // Title + Save Slot (M2 polished)
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.9f, 0.6f) }
            };
            GUI.Label(new Rect(x, y + 14, W, 34), "PAUSED", titleStyle);

            int currentSlot = ServiceLocator.Save != null ? ServiceLocator.Save.GetCurrentSlot() : 0;
            var slotStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = new Color(0.75f, 0.75f, 0.65f) }
            };
            GUI.Label(new Rect(x, y + 46, W, 20), $"Save Slot {currentSlot}  •  The Aether Remembers", slotStyle);

            var btn = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 42 };
            var sel = new GUIStyle(btn);
            sel.normal.textColor = new Color(0.1f, 0.08f, 0.05f);
            sel.fontStyle = FontStyle.Bold;
            sel.normal.background = sel.hover.background; // strong gold selected for F310 gamepad feedback

            const int bw = 280, bh = 42, gap = 12;
            int bx = x + (W - bw) / 2;
            int by = y + 78;

            string Mark(int i, string label) => _selectedIndex == i ? "> " + label + " <" : label;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(0, "RESUME"), _selectedIndex == 0 ? sel : btn))
            { _selectedIndex = 0; Close(); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(1, "SETTINGS"), _selectedIndex == 1 ? sel : btn))
            { _selectedIndex = 1; SettingsOverlay.Open(); Close(); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(2, "SAVE GAME"), _selectedIndex == 2 ? sel : btn))
            { _selectedIndex = 2; ActivateSelected(); }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), Mark(3, "SAVE & QUIT"), _selectedIndex == 3 ? sel : btn))
            { _selectedIndex = 3; ActivateSelected(); }

            // Footer
            var hint = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            GUI.Label(new Rect(x, y + H - 22, W, 18), "Esc: Resume  |  Arrows: Nav  |  Enter: Select  |  Save Game = quick save", hint);
        }
    }
}
