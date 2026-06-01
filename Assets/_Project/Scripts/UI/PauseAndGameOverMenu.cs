using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Day-6: Esc-pause overlay + game-over screen.
    /// Self-bootstraps after every scene load. Listens to PlayerHealth.OnDeath
    /// for game-over. Pauses Time.timeScale and renders an IMGUI menu with
    /// Resume / Restart Moon / Return to Echohaven / Quit.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseAndGameOverMenu : MonoBehaviour
    {
        const string EchohavenScene = "Echohaven_VerticalSlice";
        static PauseAndGameOverMenu _instance;

        enum State { Hidden, Paused, GameOver }
        State _state = State.Hidden;
        PlayerHealth _health;
        float _prevTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) { _instance.Rebind(); return; }
            var go = new GameObject("PauseAndGameOverMenu");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PauseAndGameOverMenu>();
            SceneManager.sceneLoaded += (s, m) => { if (_instance != null) _instance.OnSceneLoaded(); };
        }

        void OnSceneLoaded()
        {
            // Always unpause on scene change so a paused state from previous scene clears.
            if (_state != State.Hidden) Resume();
            Rebind();
        }

        void Rebind() => StartCoroutine(RebindNextFrame());

        System.Collections.IEnumerator RebindNextFrame()
        {
            yield return null;
            if (_health != null) _health.OnDeath -= OnPlayerDeath;
            var p = GameObject.FindGameObjectWithTag("Player");
            _health = p != null ? p.GetComponent<PlayerHealth>() : null;
            if (_health != null) _health.OnDeath += OnPlayerDeath;
        }

        void OnPlayerDeath()
        {
            if (_state == State.GameOver) return;
            _state = State.GameOver;
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Debug.Log("[GameOver] Player died — showing menu.");
        }

        void Update()
        {
            if (_state == State.GameOver) return;

            // 2026-05-30 playtest fix: Input System Esc was sometimes missed during
            // playtest (window focus / Time.timeScale=0 / multiple pause overlays
            // competing). Read BOTH the new Input System AND legacy Input as a
            // defensive fallback — whichever fires first wins.
            bool escPressed = false;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) escPressed = true;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif
            if (escPressed)
            {
                if (_state == State.Hidden) Pause();
                else if (_state == State.Paused) Resume();
            }
        }

        void Pause()
        {
            _state = State.Paused;
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        void Resume()
        {
            _state = State.Hidden;
            Time.timeScale = _prevTimeScale > 0f ? _prevTimeScale : 1f;
        }

        void RestartScene()
        {
            Time.timeScale = 1f;
            _state = State.Hidden;
            // If player died, respawn HP first so reload doesn't insta-die
            if (_health != null) _health.Respawn();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void GoToEchohaven()
        {
            Time.timeScale = 1f;
            _state = State.Hidden;
            if (_health != null) _health.Respawn();
            try { SceneManager.LoadScene(EchohavenScene); }
            catch (System.Exception e) { Debug.LogWarning($"[Pause] Echohaven load failed: {e.Message}"); }
        }

        void OnGUI()
        {
            if (_state == State.Hidden) return;

            // Dim background
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;

            const int W = 380, H = 320;
            int x = (Screen.width - W) / 2, y = (Screen.height - H) / 2;
            GUI.Box(new Rect(x, y, W, H), "");

            string title = _state == State.GameOver ? "<b>YOU FELL</b>" : "<b>PAUSED</b>";
            string subtitle = _state == State.GameOver
                ? "The aether reclaims you for now."
                : "Tartaria waits.";

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32, alignment = TextAnchor.MiddleCenter, richText = true,
                normal = { textColor = _state == State.GameOver ? new Color(1f, 0.4f, 0.4f) : Color.white }
            };
            GUI.Label(new Rect(x, y + 24, W, 40), title, titleStyle);

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };
            GUI.Label(new Rect(x, y + 70, W, 24), subtitle, subStyle);

            const int BW = 240, BH = 38;
            int bx = x + (W - BW) / 2;
            int by = y + 110;

            var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold };

            if (_state == State.Paused && GUI.Button(new Rect(bx, by, BW, BH), "Resume", btnStyle)) Resume();
            by += BH + 10;

            if (GUI.Button(new Rect(bx, by, BW, BH),
                _state == State.GameOver ? "Respawn (Restart Scene)" : "Restart Scene", btnStyle))
                RestartScene();
            by += BH + 10;
            if (GUI.Button(new Rect(bx, by, BW, BH), "Return to Echohaven", btnStyle)) GoToEchohaven();
            by += BH + 10;

            if (GUI.Button(new Rect(bx, by, BW, BH), "Settings", btnStyle))
                SettingsOverlay.Open();
            by += BH + 10;

            if (GUI.Button(new Rect(bx, by, BW, BH), "Quit to Desktop", btnStyle))
            {
                Time.timeScale = 1f;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
