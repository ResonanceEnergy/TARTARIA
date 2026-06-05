using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Save;

namespace Tartaria.UI
{
    /// <summary>
    /// Game Complete Overlay — fullscreen IMGUI credits panel shown when Moon 13 fires
    /// GameEvents.OnCriticalSaveTrigger("game_complete"). Self-bootstraps, survives scene loads.
    ///
    /// Shows: title, flavour quote, completion stats, two actions:
    ///   [Continue Exploring]  — dismisses overlay, game keeps running
    ///   [Return to Main Menu] — reloads scene index 0
    /// </summary>
    [DisallowMultipleComponent]
    public class GameCompleteOverlay : MonoBehaviour
    {
        public static GameCompleteOverlay Instance { get; private set; }

        // ------------------------------------------------------------------ bootstrap
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[GameCompleteOverlay]");
            DontDestroyOnLoad(go);
            go.AddComponent<GameCompleteOverlay>();
        }

        // ------------------------------------------------------------------ state
        private bool   _shown;
        private float  _shownAt;
        private float  _alpha;          // fade-in 0..1
        private bool   _dismissed;

        private Texture2D _bgTex;
        private GUIStyle  _titleStyle;
        private GUIStyle  _bodyStyle;
        private GUIStyle  _quoteStyle;
        private GUIStyle  _btnStyle;
        private bool      _stylesBuilt;

        // completion stats captured when triggered
        private int   _moonsCleared;
        private float _playTimeMinutes;
        private int   _orphansAdopted;
        private bool  _liraelFullyManifested;

        private const float FadeSeconds   = 2.5f;
        private const float StatsDelay    = 1.2f;   // seconds after fade before stats appear
        private const float ButtonDelay   = 3f;     // seconds before buttons appear

        private static readonly string[] CREDITS_LINES =
        {
            "Music & Sound — Tartaria Audio",
            "Narrative Design — Resonance Energy",
            "World Architecture — Mud Flood Canon",
            "Dedicated to the children who sang anyway.",
        };

        // ------------------------------------------------------------------ lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            GameEvents.OnCriticalSaveTrigger += OnSaveTrigger;
        }

        void OnDisable()
        {
            GameEvents.OnCriticalSaveTrigger -= OnSaveTrigger;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_bgTex != null) Destroy(_bgTex);
        }

        void OnApplicationQuit()
        {
            // Ensure time is never stuck at 0 if the app is quit while overlay is visible
            PauseService.ForceReset();
        }

        void Update()
        {
            if (!_shown || _dismissed) return;
            float elapsed = Time.realtimeSinceStartup - _shownAt;
            _alpha = Mathf.Clamp01(elapsed / FadeSeconds);
        }

        // ------------------------------------------------------------------ trigger
        private void OnSaveTrigger(string reason)
        {
            if (reason != "game_complete") return;
            if (_shown) return;
            Show();
        }

        /// <summary>Trigger manually (e.g. from Moon13CosmicArc or debug console).</summary>
        public void Show()
        {
            if (_shown) return;
            _shown   = true;
            _shownAt = Time.realtimeSinceStartup;
            _alpha   = 0f;

            // Capture save stats
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
            {
                _moonsCleared          = CountMoonsCleared(save);
                _orphansAdopted        = save.GetMoonFlag(3, "adopted", 0);
                _liraelFullyManifested = save.GetMoonFlag(1, "lirael_manifested");
            }
            _playTimeMinutes = Time.realtimeSinceStartup / 60f;

            // Pause time but keep audio
            PauseService.Push();

            AudioManager.Instance?.PlaySFX2D("game_complete_credits_theme");
            Debug.Log("[GameCompleteOverlay] GAME COMPLETE — showing credits overlay.");
        }

        // ------------------------------------------------------------------ IMGUI
        void OnGUI()
        {
            if (!_shown || _dismissed) return;

            BuildStyles();

            float a       = _alpha;
            float elapsed = Time.realtimeSinceStartup - _shownAt;
            float sw      = Screen.width;
            float sh      = Screen.height;

            // Full-screen black fade
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(a * 0.92f));
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _bgTex);
            GUI.color = Color.white;

            // Early-out if not faded enough for text
            if (a < 0.3f) return;

            float textAlpha = Mathf.Clamp01((a - 0.3f) / 0.7f);

            // --- Title ---
            _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.4f, textAlpha);
            float titleY = sh * 0.08f;
            GUI.Label(new Rect(sw * 0.1f, titleY, sw * 0.8f, 90f), "TARTARIA", _titleStyle);

            float subY = titleY + 88f;
            _bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 1f, textAlpha * 0.9f);
            _bodyStyle.fontSize = Mathf.RoundToInt(sh * 0.028f);
            GUI.Label(new Rect(sw * 0.1f, subY, sw * 0.8f, 50f),
                "All 13 Moons. All 13 Bells. The Golden Age Begins Again.", _bodyStyle);

            // --- Flavour quote ---
            float quoteY = subY + 60f;
            _quoteStyle.normal.textColor = new Color(0.7f, 0.85f, 1f, textAlpha * 0.75f);
            GUI.Label(new Rect(sw * 0.15f, quoteY, sw * 0.7f, 80f),
                "\"They buried the city. They silenced the bells. They could not silence the children.\"\n — Lirael, Moon 3",
                _quoteStyle);

            // --- Stats (appear after StatsDelay) ---
            if (elapsed > StatsDelay)
            {
                float statsAlpha  = Mathf.Clamp01((elapsed - StatsDelay) / 1.5f) * textAlpha;
                float statsY      = quoteY + 90f;
                _bodyStyle.normal.textColor  = new Color(0.85f, 1f, 0.85f, statsAlpha);
                _bodyStyle.fontSize          = Mathf.RoundToInt(sh * 0.022f);
                _bodyStyle.alignment         = TextAnchor.UpperCenter;

                string statsText =
                    $"Moons Cleared: {_moonsCleared} / 13    |    Orphans Adopted: {_orphansAdopted}" +
                    (_liraelFullyManifested ? "    |    Lirael: Fully Manifested" : "");
                GUI.Label(new Rect(sw * 0.1f, statsY, sw * 0.8f, 40f), statsText, _bodyStyle);

                // Credits lines
                float credY = statsY + 50f;
                _bodyStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, statsAlpha * 0.8f);
                _bodyStyle.fontSize         = Mathf.RoundToInt(sh * 0.018f);
                foreach (var line in CREDITS_LINES)
                {
                    GUI.Label(new Rect(sw * 0.1f, credY, sw * 0.8f, 30f), line, _bodyStyle);
                    credY += 28f;
                }
            }

            // --- Buttons (appear after ButtonDelay) ---
            if (elapsed > ButtonDelay)
            {
                float btnAlpha = Mathf.Clamp01((elapsed - ButtonDelay) / 1.0f) * textAlpha;
                _btnStyle.normal.textColor = new Color(1f, 1f, 1f, btnAlpha);

                float btnW  = Mathf.Min(sw * 0.26f, 340f);
                float btnH  = 52f;
                float btnY  = sh * 0.82f;
                float gap   = 30f;
                float totalW = btnW * 2 + gap;
                float startX = (sw - totalW) * 0.5f;

                // [Continue Exploring]
                if (GUI.Button(new Rect(startX, btnY, btnW, btnH), "Continue Exploring", _btnStyle))
                {
                    Dismiss();
                }

                // [Return to Main Menu]
                if (GUI.Button(new Rect(startX + btnW + gap, btnY, btnW, btnH), "Return to Main Menu", _btnStyle))
                {
                    ReturnToMainMenu();
                }
            }
        }

        // ------------------------------------------------------------------ actions
        private void Dismiss()
        {
            _dismissed = true;
            PauseService.Pop();
            AudioManager.Instance?.PlaySFX2D("ui_confirm");
            Debug.Log("[GameCompleteOverlay] Dismissed — continuing exploration.");
        }

        private void ReturnToMainMenu()
        {
            _dismissed = true;
            PauseService.Pop();
            AudioManager.Instance?.PlaySFX2D("ui_confirm");
            PlayerPrefs.DeleteKey("TARTARIA_SkipMainMenu");
            PlayerPrefs.Save();
            Debug.Log("[GameCompleteOverlay] Returning to main menu.");
            SceneManager.LoadScene(0);
        }

        // ------------------------------------------------------------------ helpers
        private static int CountMoonsCleared(SaveData save)
        {
            int count = 0;
            for (int m = 1; m <= 13; m++)
                if (save.GetMoonFlag(m, "moon_cleared")) count++;
            return count;
        }

        private void BuildStyles()
        {
            if (_stylesBuilt) return;
            _stylesBuilt = true;

            if (_bgTex == null)
            {
                _bgTex = new Texture2D(1, 1);
                _bgTex.SetPixel(0, 0, Color.black);
                _bgTex.Apply();
            }

            int titleSize = Mathf.RoundToInt(Screen.height * 0.10f);
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = titleSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                wordWrap  = false,
            };

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(Screen.height * 0.025f),
                alignment = TextAnchor.UpperCenter,
                wordWrap  = true,
            };

            _quoteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(Screen.height * 0.022f),
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.UpperCenter,
                wordWrap  = true,
            };

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Mathf.RoundToInt(Screen.height * 0.025f),
                fontStyle = FontStyle.Bold,
            };
        }
    }
}
