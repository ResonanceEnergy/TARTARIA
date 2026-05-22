using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Tartaria.Integration
{
    /// <summary>
    /// Debug + design hotkey portal: F1=Echohaven, F2..F12 = Moons 2..12, BackQuote = Moon 13.
    /// Gamepad: D-pad/L-stick up/down to cycle, buttonSouth (A) to warp, Select/View to toggle overlay.
    /// Loads the moon scene by name. Survives scene reloads. Self-bootstraps.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonPortalSelector : MonoBehaviour
    {
        static MoonPortalSelector _instance;
        bool _showHelp = true;
        float _helpTimer;

        // Gamepad nav state
        int  _gpSelected = 1;       // 1-13
        bool _gpActive   = false;   // true once a gamepad nav key was used
        float _gpNavCooldown = 0f;

        static readonly string[] MoonNames =
        {
            "Echohaven",            // 1
            "Crystalline Caverns",  // 2
            "Windswept Highlands",  // 3
            "Star Fort Bastion",    // 4
            "Sunken Colosseum",     // 5
            "Living Library",       // 6
            "Clockwork Citadel",    // 7
            "Verdant Canopy",       // 8
            "Auroral Spire",        // 9
            "Deep Forge",           // 10
            "Tidal Archive",        // 11
            "Celestial Observatory",// 12
            "Planetary Nexus",      // 13
        };

        // sceneName per moon — must match MoonScenesFactory.
        static readonly string[] MoonScenes =
        {
            "Echohaven_VerticalSlice",   // 1
            "CrystallineCaverns",        // 2
            "WindsweptHighlands",        // 3
            "StarFortBastion",           // 4
            "SunkenColosseum",           // 5
            "LivingLibrary",             // 6
            "ClockworkCitadel",          // 7
            "VerdantCanopy",             // 8
            "AuroralSpire",              // 9
            "DeepForge",                 // 10
            "TidalArchive",              // 11
            "CelestialObservatory",      // 12
            "PlanetaryNexus",            // 13
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("MoonPortalSelector");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MoonPortalSelector>();
        }

        void OnEnable()
        {
            _showHelp = true;
            _helpTimer = 8f;
        }

        void Update()
        {
            if (_helpTimer > 0f)
            {
                _helpTimer -= Time.unscaledDeltaTime;
                if (_helpTimer <= 0f) _showHelp = false;
            }
            if (_gpNavCooldown > 0f) _gpNavCooldown -= Time.unscaledDeltaTime;

#if ENABLE_INPUT_SYSTEM
            HandleNewInputSystem();
#else
            HandleLegacyInput();
#endif
        }

#if ENABLE_INPUT_SYSTEM
        void HandleNewInputSystem()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                // F1..F12 = Moons 1..12
                var fKeys = new[] {
                    kb.f1Key, kb.f2Key, kb.f3Key, kb.f4Key, kb.f5Key, kb.f6Key,
                    kb.f7Key, kb.f8Key, kb.f9Key, kb.f10Key, kb.f11Key, kb.f12Key
                };
                for (int i = 0; i < 12; i++)
                {
                    if (fKeys[i] != null && fKeys[i].wasPressedThisFrame)
                    {
                        LoadMoon(i + 1);
                        return;
                    }
                }
                if (kb.backquoteKey.wasPressedThisFrame) { LoadMoon(13); return; }
                if (kb.mKey.wasPressedThisFrame)
                {
                    _showHelp = !_showHelp;
                    _helpTimer = _showHelp ? 8f : 0f;
                }
            }

            // Gamepad navigation
            var gp = Gamepad.current;
            if (gp == null) return;

            // Select/View button = toggle overlay
            if (gp.selectButton.wasPressedThisFrame)
            {
                _gpActive  = true;
                _showHelp  = !_showHelp;
                _helpTimer = _showHelp ? 30f : 0f;
            }

            // D-pad / left stick nav (with cooldown to prevent runaway)
            if (_gpNavCooldown <= 0f)
            {
                bool dpadUp   = gp.dpad.up.isPressed;
                bool dpadDown = gp.dpad.down.isPressed;
                float stick   = gp.leftStick.ReadValue().y;
                bool stickUp  = stick >  0.5f;
                bool stickDown= stick < -0.5f;

                if (dpadUp || stickUp)
                {
                    _gpActive   = true;
                    _showHelp   = true;
                    _helpTimer  = 30f;
                    _gpSelected = _gpSelected > 1 ? _gpSelected - 1 : MoonScenes.Length;
                    _gpNavCooldown = 0.18f;
                }
                else if (dpadDown || stickDown)
                {
                    _gpActive   = true;
                    _showHelp   = true;
                    _helpTimer  = 30f;
                    _gpSelected = _gpSelected < MoonScenes.Length ? _gpSelected + 1 : 1;
                    _gpNavCooldown = 0.18f;
                }
            }

            // A (buttonSouth) = confirm warp
            if (gp.buttonSouth.wasPressedThisFrame && _gpActive)
            {
                LoadMoon(_gpSelected);
            }
            // B (buttonEast) = dismiss overlay
            if (gp.buttonEast.wasPressedThisFrame && _gpActive && _showHelp)
            {
                _showHelp = false;
                _helpTimer = 0f;
            }
        }
#else
        void HandleLegacyInput()
        {
            // F1..F12 = Moons 1..12
            for (int i = 0; i < 12; i++)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.F1 + i))
                {
                    LoadMoon(i + 1);
                    return;
                }
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote)) LoadMoon(13);
            if (UnityEngine.Input.GetKeyDown(KeyCode.M))
            {
                _showHelp = !_showHelp;
                _helpTimer = _showHelp ? 8f : 0f;
            }
        }
#endif

        void LoadMoon(int n)
        {
            if (n < 1 || n > MoonScenes.Length) return;
            string sceneName = MoonScenes[n - 1];
            Debug.Log($"[MoonPortal] Loading Moon {n:D2} — {sceneName}");
            HUDController.Instance?.ShowObjective($"<b>↪ Loading Moon {n:D2}: {sceneName}</b>");
            SceneFadeTransition.LoadScene(sceneName);
        }

        static readonly string[] _moonLabels =
        {
            "1  Echohaven",            "2  Crystalline Caverns", "3  Windswept Highlands",
            "4  Star Fort Bastion",    "5  Sunken Colosseum",    "6  Living Library",
            "7  Clockwork Citadel",    "8  Verdant Canopy",      "9  Auroral Spire",
            "10 Deep Forge",           "11 Tidal Archive",       "12 Celestial Observatory",
            "13 Planetary Nexus",
        };

        void OnGUI()
        {
            if (!_showHelp) return;

            bool useGamepad = _gpActive;
            const int W = 270, ROW = 17;
            int rows = useGamepad ? MoonScenes.Length : MoonScenes.Length;
            int H = 22 + rows * ROW + (useGamepad ? 24 : 0);
            int x = Screen.width - W - 12, y = 12;

            GUI.Box(new Rect(x, y, W, H), useGamepad ? "Moon Portal  [A]=Warp  [B]=Close" : "Moon Portal (M to toggle)");

            var normal   = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            var selected = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            selected.normal.textColor = new Color(1f, 0.92f, 0.2f);

            for (int i = 0; i < _moonLabels.Length; i++)
            {
                int moonNum = i + 1;
                var style = (useGamepad && _gpSelected == moonNum) ? selected : normal;
                string prefix = (useGamepad && _gpSelected == moonNum) ? "> " : "  ";
                string label  = useGamepad ? prefix + _moonLabels[i] : (moonNum < 13 ? $"F{moonNum,-2} {MoonNames[i]}" : $"`  {MoonNames[i]}");
                GUI.Label(new Rect(x + 8, y + 20 + i * ROW, W - 16, ROW), label, style);
            }

            if (useGamepad)
            {
                var hint = new GUIStyle(GUI.skin.label) { fontSize = 10 };
                hint.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                GUI.Label(new Rect(x + 8, y + H - 22, W - 16, 20), "D-Pad/L-Stick: scroll  |  Select: toggle", hint);
            }
        }
    }
}
