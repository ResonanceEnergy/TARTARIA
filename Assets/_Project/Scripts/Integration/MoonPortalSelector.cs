using UnityInput = UnityEngine.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Debug + design hotkey portal: F1=Echohaven, F2..F12 = Moons 2..12, F13/Backquote = Moon 13.
    /// Loads the moon scene by name. Survives scene reloads. Self-bootstraps.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonPortalSelector : MonoBehaviour
    {
        static MoonPortalSelector _instance;
        bool _showHelp = true;
        float _helpTimer;

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

            // F1..F12 = Moons 1..12
            for (int i = 0; i < 12; i++)
            {
                if (UnityInput.GetKeyDown(KeyCode.F1 + i))
                {
                    LoadMoon(i + 1);
                    return;
                }
            }
            // Backquote (`) = Moon 13
            if (UnityInput.GetKeyDown(KeyCode.BackQuote))
                LoadMoon(13);
            // M = toggle help overlay
            if (UnityInput.GetKeyDown(KeyCode.M))
            {
                _showHelp = !_showHelp;
                _helpTimer = _showHelp ? 8f : 0f;
            }
        }

        void LoadMoon(int n)
        {
            if (n < 1 || n > MoonScenes.Length) return;
            string sceneName = MoonScenes[n - 1];
            Debug.Log($"[MoonPortal] Loading Moon {n:D2} — {sceneName}");
            HUDController.Instance?.ShowObjective($"<b>↪ Loading Moon {n:D2}: {sceneName}</b>");
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MoonPortal] Could not load {sceneName}: {e.Message}");
            }
        }

        void OnGUI()
        {
            if (!_showHelp) return;
            const int W = 260, H = 230;
            int x = Screen.width - W - 12, y = 12;
            GUI.Box(new Rect(x, y, W, H), "Moon Portal (M to toggle)");
            var s = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            string body =
                "F1  Echohaven\n" +
                "F2  Crystalline Caverns\n" +
                "F3  Windswept Highlands\n" +
                "F4  Star Fort Bastion\n" +
                "F5  Sunken Colosseum\n" +
                "F6  Living Library\n" +
                "F7  Clockwork Citadel\n" +
                "F8  Verdant Canopy\n" +
                "F9  Auroral Spire\n" +
                "F10 Deep Forge\n" +
                "F11 Tidal Archive\n" +
                "F12 Celestial Observatory\n" +
                "`   Planetary Nexus";
            GUI.Label(new Rect(x + 8, y + 22, W - 16, H - 28), body, s);
        }
    }
}
