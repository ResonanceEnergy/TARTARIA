#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// R100 — Dev-only menu bypass for autonomous testing sessions.
    /// On Play-mode entry in Echohaven_VerticalSlice, auto-skips the main menu
    /// after 2 seconds so testers (or autonomous agents) land straight in the game.
    /// Toggle via Tartaria menu. Persists in EditorPrefs.
    /// </summary>
    [InitializeOnLoad]
    public static class TartariaDevAutoStart
    {
        const string PrefKey = "Tartaria.Dev.AutoStartGame";
        const string MenuItem = "Tartaria/9 Debug/Auto-Start Game (Skip Menu)";

        static TartariaDevAutoStart()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode) return;
            if (!EditorPrefs.GetBool(PrefKey, false)) return;
            // Defer to runtime
            EditorApplication.delayCall += () =>
            {
                var go = new GameObject("__TartariaDevAutoStart");
                go.AddComponent<TartariaDevAutoStartDriver>();
            };
        }

        [MenuItem(MenuItem)]
        static void Toggle()
        {
            var was = EditorPrefs.GetBool(PrefKey, false);
            EditorPrefs.SetBool(PrefKey, !was);
            Menu.SetChecked(MenuItem, !was);
            Debug.Log($"[TartariaDevAutoStart] Auto-Start Game: {(!was ? "ON" : "OFF")}");
        }

        [MenuItem(MenuItem, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuItem, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }
    }

    public class TartariaDevAutoStartDriver : MonoBehaviour
    {
        float _delay = 2.5f;
        bool _fired;

        void Update()
        {
            if (_fired) return;
            _delay -= Time.deltaTime;
            if (_delay > 0) return;
            _fired = true;
            FireNewGame();
        }

        void FireNewGame()
        {
            // Find any object with a MainMenuController-like type and invoke OnNewGame via reflection
            var all = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int hits = 0;
            foreach (var mb in all)
            {
                if (mb == null) continue;
                var t = mb.GetType();
                if (t.Name != "MainMenuController") continue;
                var m = t.GetMethod("OnNewGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (m != null) { try { m.Invoke(mb, null); hits++; } catch { } }
            }
            Debug.Log($"[TartariaDevAutoStart] Fired NewGame on {hits} controllers.");

            // Also hide menu overlays as a safety net
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c == null) continue;
                if (c.gameObject.name.ToLower().Contains("mainmenu") || c.gameObject.name.ToLower().Contains("titlescreen"))
                {
                    c.gameObject.SetActive(false);
                    Debug.Log($"[TartariaDevAutoStart] Hid menu canvas: {c.gameObject.name}");
                }
            }
        }
    }
}
#endif
