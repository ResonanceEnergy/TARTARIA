using UnityEngine;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Day-13: Main menu overlay. BeforeSceneLoad → marks GameBootstrap.MainMenuActive
    /// so the bootstrap pauses before loading gameplay scenes. Renders an IMGUI panel
    /// in the Boot scene with NEW GAME / CONTINUE / SETTINGS / QUIT.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuOverlay : MonoBehaviour
    {
        static MainMenuOverlay _instance;
        bool _visible = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            // Bypass for dev / replay sessions.
            if (PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1) return;
            GameBootstrap.MainMenuActive = true;
            var go = new GameObject("MainMenuOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MainMenuOverlay>();
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

            int by = y + 120;
            const int bw = 280, bh = 44, gap = 12;
            int bx = x + (W - bw) / 2;

            if (GUI.Button(new Rect(bx, by, bw, bh), "NEW GAME", btn))
            {
                ResetProgressViaReflection();
                StartGame();
            }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "CONTINUE", btn))
            {
                StartGame();
            }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "SETTINGS", btn))
            {
                SettingsOverlay.Open();
            }
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "QUIT", btn))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
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
