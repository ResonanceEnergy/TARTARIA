using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Day-13: Main menu overlay. BeforeSceneLoad → marks GameBootstrap.MainMenuActive
    /// so the bootstrap pauses before loading gameplay scenes. Renders an IMGUI panel
    /// in the Boot scene with NEW GAME / CONTINUE / SETTINGS / QUIT.
    /// Gamepad: D-pad/L-stick navigates, A confirms, B quits. Keyboard: arrows + Enter + Esc.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuOverlay : MonoBehaviour
    {
#pragma warning disable CS0414 // Assigned in Bootstrap to keep the overlay GameObject alive across scene loads.
        static MainMenuOverlay _instance;
#pragma warning restore CS0414
        bool _visible = true;
        int _selected = 0;            // currently highlighted button index
        const int BUTTON_COUNT = 4;   // NEW GAME / CONTINUE / SETTINGS / QUIT
        float _navCooldown;           // debounce stick repeat
#pragma warning disable CS0414 // Field assigned but never used - reserved for future overwrite-save confirmation modal
        bool _showNewGameConfirm;     // overwrite-save confirmation modal
#pragma warning restore CS0414

        // Sprint 8 Lane 3 (2026-06-02): Re-enabled per Moon 1 acceptance audit blocker #13.1.
        // Was disabled during Sprint 7 controller-testing because the overlay drew on top of
        // the Boot scene and made it look like input was dead (see CLAUDE.md F310 section —
        // Error Pause + missing-script init errors made every Play session enter paused).
        // Those root causes are now fixed (PlayerInputHandler focus fix + EchohavenSceneAudit),
        // so the menu is safe to re-enable. Without this Bootstrap the game has no entry-point
        // UI — Boot scene loads straight into gameplay with no New Game / Continue affordance.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // Bypass for dev / replay sessions. Set PlayerPrefs key from a debug menu or via
            //   PlayerPrefs.SetInt("TARTARIA_SkipMainMenu", 1); PlayerPrefs.Save();
            // to skip the overlay and jump straight into the gameplay scene.
            if (PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1) return;

            // Tell GameBootstrap to wait for an explicit New Game / Continue click before
            // calling TriggerSceneLoad() (see GameBootstrap.cs:85 — the gate is
            // if (autoStart || !MainMenuActive) TriggerSceneLoad();).
            GameBootstrap.MainMenuActive = true;

            var go = new GameObject("MainMenuOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MainMenuOverlay>();
        }

        void Update()
        {
            if (!_visible) return;
            HandleNavigation();
        }

        void HandleNavigation()
        {
            int dy = 0;
            bool confirm = false;
            bool cancel = false;

            // Keyboard
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) dy = -1;
                else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) dy = 1;
                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) confirm = true;
                if (kb.escapeKey.wasPressedThisFrame) cancel = true;
            }

            // Gamepad
            var gp = Gamepad.current;
            if (gp != null)
            {
                if (gp.dpad.up.wasPressedThisFrame) dy = -1;
                else if (gp.dpad.down.wasPressedThisFrame) dy = 1;

                if (_navCooldown <= 0f)
                {
                    float ly = gp.leftStick.ReadValue().y;
                    if (ly > 0.55f) { dy = -1; _navCooldown = 0.18f; }
                    else if (ly < -0.55f) { dy = 1; _navCooldown = 0.18f; }
                }
                else _navCooldown -= Time.unscaledDeltaTime;

                if (gp.buttonSouth.wasPressedThisFrame || gp.startButton.wasPressedThisFrame) confirm = true;
                if (gp.buttonEast.wasPressedThisFrame) cancel = true;
            }

            if (dy != 0)
            {
                _selected = ((_selected + dy) % BUTTON_COUNT + BUTTON_COUNT) % BUTTON_COUNT;
            }
            if (confirm) ActivateSelected();
            else if (cancel) Quit();
        }

        void ActivateSelected()
        {
            switch (_selected)
            {
                case 0: // NEW GAME
                    if (ServiceLocator.Save != null && ServiceLocator.Save.HasAnySave())
                    {
                        _showNewGameConfirm = true;
                        return;
                    }
                    ResetProgressViaReflection();
                    StartGame();
                    break;

                case 1: StartGame(); break; // CONTINUE
                case 2: SettingsOverlay.Open(); break;
                case 3: Quit(); break;
            }
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnGUI()
        {
            if (!_visible) return;

            // Full-screen dim.
            var dim = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = dim;

            const int W = 460, H = 380;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            GUI.Box(new Rect(x, y, W, H), "");

            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.92f, 0.6f) } };
            GUI.Label(new Rect(x, y + 24, W, 56), "TARTARIA", titleStyle);

            var sub = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
            GUI.Label(new Rect(x, y + 78, W, 22), "13 Moons of the Sundered Empire", sub);

            var btn = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 44 };
            var btnSel = new GUIStyle(btn) { fontStyle = FontStyle.Bold };
            btnSel.normal.textColor = new Color(1f, 0.92f, 0.4f);
            btnSel.hover.textColor = new Color(1f, 0.92f, 0.4f);

            int by = y + 120;
            const int bw = 280, bh = 44, gap = 12;
            int bx = x + (W - bw) / 2;

            bool hasSave = ServiceLocator.Save != null && ServiceLocator.Save.HasAnySave();
            string continueLabel = "CONTINUE (No Save Found)";
            if (hasSave)
            {
                string label = ServiceLocator.Save.GetCurrentSaveLabel();
                continueLabel = string.IsNullOrEmpty(label) ? "CONTINUE" : $"CONTINUE [{label}]";
            }
            string[] labels = { "NEW GAME", continueLabel, "SETTINGS", "QUIT" };

            // Guard against null GUI.skin during UIElements init race
            if (GUI.skin == null) return;

            for (int i = 0; i < labels.Length; i++)
            {
                bool sel = (i == _selected);
                string label = sel ? "▶ " + labels[i] + " ◀" : labels[i];
                if (GUI.Button(new Rect(bx, by, bw, bh), label, sel ? btnSel : btn))
                {
                    _selected = i;
                    ActivateSelected();
                }
                by += bh + gap;
            }

            // Footer hint — adapts to active input scheme via InputPromptHelper if loaded.
            var foot = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.7f, 0.7f, 0.75f) } };
            string hint = (Gamepad.current != null)
                ? "D-Pad / L-Stick to navigate    ⓐ Confirm    ⓑ Quit"
                : "↑/↓ to navigate    Enter Confirm    Esc Quit";
            GUI.Label(new Rect(x, y + H - 28, W, 22), hint, foot);
        }

        void StartGame()
        {
            _visible = false;
            GameBootstrap.BeginGameplay();
            // Self-destruct so we don't intercept input mid-game.
            Destroy(gameObject, 0.05f);
        }

        static void ResetProgressViaReflection()
        {
            // Avoid hard asmdef refs to Integration; reset via reflection if present.
            TryCallStaticInstance("Tartaria.Integration.RunProgressTracker, Tartaria.Integration", "ResetRun");
            TryCallStaticInstance("Tartaria.Integration.MoonProgressTracker, Tartaria.Integration", "ResetAll");
        }

        static void TryCallStaticInstance(string typeName, string methodName)
        {
            try
            {
                var t = System.Type.GetType(typeName);
                if (t == null) return;
                var inst = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                if (inst == null) return;
                var m = t.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                m?.Invoke(inst, null);
            }
            catch (System.Exception ex) { Debug.LogWarning($"[MainMenuOverlay] TryInvoke({typeName}.{methodName}) failed: {ex.Message}"); }
        }
    }
}
