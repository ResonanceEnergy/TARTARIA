using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// First-launch tutorial cheatsheet. Shown the first time a player reaches
    /// gameplay (after BeginGameplay). Dismiss with Space / Enter / Gamepad-South.
    ///
    /// Persists "seen" state in PlayerPrefs so it only shows once.
    /// Re-openable via F1 in-game.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialOverlay : MonoBehaviour
    {
        const string PP_SEEN = "TARTARIA_TutorialSeen_v1";

        static TutorialOverlay _instance;
        bool _visible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("TutorialOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<TutorialOverlay>();
        }

        void Start()
        {
            // Only auto-show on first launch (after main menu has been dismissed).
            if (PlayerPrefs.GetInt(PP_SEEN, 0) == 0 && !Tartaria.Core.GameBootstrap.MainMenuActive)
            {
                _visible = true;
            }
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            var pad = UnityEngine.InputSystem.Gamepad.current;

            // F1 toggle
            if (kb != null && kb.f1Key.wasPressedThisFrame)
            {
                _visible = !_visible;
                if (_visible) UnlockCursor();
                else RestoreCursor();
                return;
            }

            if (!_visible) return;

            bool dismiss =
                (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)) ||
                (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));

            if (dismiss)
            {
                _visible = false;
                PlayerPrefs.SetInt(PP_SEEN, 1);
                PlayerPrefs.Save();
                RestoreCursor();
            }
            else
            {
                UnlockCursor();
            }
        }

        void OnGUI()
        {
            if (!_visible) return;

            const int W = 580, H = 480;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            var c = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = c;

            GUI.Box(new Rect(x, y, W, H), "");

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.92f, 0.6f) }
            };
            GUI.Label(new Rect(x, y + 16, W, 36), "WELCOME TO TARTARIA", title);

            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };
            GUI.Label(new Rect(x, y + 52, W, 22), "13 Moons of the Sundered Empire — Quick Reference", sub);

            var label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            var key   = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.4f, 0.9f, 1f) } };

            int row = y + 96;
            const int kx = 60, ky = 240;
            void Line(string k, string v)
            {
                GUI.Label(new Rect(x + kx,   row, 180, 22), k, key);
                GUI.Label(new Rect(x + ky,   row, W - 240, 22), v, label);
                row += 26;
            }

            Line("WASD",         "Move");
            Line("Mouse",        "Look / Aim");
            Line("Left Mouse",   "Attack");
            Line("Space",        "Jump");
            Line("E",            "Interact (NPCs, portals, beacons)");
            Line("Shift",        "Sprint");
            Line("Tab",          "Inventory & Quest Log");
            Line("M",            "Moon Selector help");
            Line("F1",           "Toggle this help");
            Line("F5 / F9",      "Quick Save / Quick Load");
            Line("F10",          "Settings");
            Line("Esc",          "Pause");
            Line("F1 - F12, `",  "Warp to Moons 1 - 13 (after first clear)");

            row += 16;
            var foot = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(1f, 0.92f, 0.6f) }
            };
            GUI.Label(new Rect(x, y + H - 56, W, 22), "Press SPACE / ENTER / Ⓐ to begin", foot);
        }

        static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        static void RestoreCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
