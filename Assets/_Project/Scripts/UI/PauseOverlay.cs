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
            const int bw = 260, bh = 40, gap = 10;
            int bx = x + (W - bw) / 2;
            int by = y + 70;

            if (GUI.Button(new Rect(bx, by, bw, bh), "RESUME", btn)) Close();
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "QUICK SAVE", btn))
                CallSaveManager("QuickSave");
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "QUICK LOAD", btn))
                CallSaveManager("QuickLoad");
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "SETTINGS", btn))
                SettingsOverlay.Open();
            by += bh + gap;

            if (GUI.Button(new Rect(bx, by, bw, bh), "QUIT TO DESKTOP", btn))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
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
            catch { /* best effort */ }
        }
    }
}
