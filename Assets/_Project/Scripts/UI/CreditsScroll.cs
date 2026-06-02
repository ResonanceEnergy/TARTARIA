using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// Credits Scroll — auto-scrolling end-credits roll for Moon 13 / game completion.
    ///
    /// Source of truth: docs/credits/credits_roll.md
    /// Runtime asset:   Resources/credits_roll.txt (copied at build time via
    ///                  Tartaria/UI/Build Credits Scene editor menu, which authors
    ///                  Assets/_Project/Resources/credits_roll.txt from the .md).
    ///
    /// Behaviour:
    ///   - Loads credits text from Resources/credits_roll.txt
    ///   - Scrolls TextMeshProUGUI upward at SCROLL_SPEED_PX_PER_SEC (30 px/s)
    ///   - Auto-scrolls past the last line, then either loops or exits to MainMenu
    ///   - Esc or Space immediately returns to MainMenu scene
    ///   - If the Resources asset is missing, logs an error citing the expected path
    ///     and displays a clear fallback message (no silent fail per CLAUDE.md mandate)
    /// </summary>
    [DisallowMultipleComponent]
    public class CreditsScroll : MonoBehaviour
    {
        // ---------------------------------------------------------------- inspector
        [Header("References (auto-wired by Moon1BuildCreditsScene if blank)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _viewport;

        [Header("Scroll Behaviour")]
        [Tooltip("Pixels per second of upward scroll. Lane spec: 30 px/s.")]
        [SerializeField] private float _scrollSpeedPxPerSec = 30f;

        [Tooltip("Seconds to pause before scrolling starts (lets player read the title).")]
        [SerializeField] private float _initialPauseSeconds = 1.5f;

        [Tooltip("Seconds to hold the final line on screen before returning to MainMenu.")]
        [SerializeField] private float _holdAtEndSeconds = 4f;

        [Tooltip("Scene name to load when credits finish or user presses Esc/Space.")]
        [SerializeField] private string _returnSceneName = "MainMenu";

        // ---------------------------------------------------------------- constants
        private const string ResourcesPath = "credits_roll"; // Resources.Load(<name>) — no extension
        private const string ExpectedAssetPath = "Assets/_Project/Resources/credits_roll.txt";
        private const string ExpectedSourcePath = "docs/credits/credits_roll.md";
        private const string FallbackText =
            "Credits content missing — see docs/credits/credits_roll.md\n\n" +
            "TARTARIA WORLD OF WONDER — Aether Awakening\n" +
            "(Run Tartaria > UI > Build Credits Scene to regenerate the runtime asset.)";

        // ---------------------------------------------------------------- state
        private float _elapsed;
        private float _startY;
        private float _contentHeight;
        private float _viewportHeight;
        private bool _finished;
        private bool _exiting;

        // ---------------------------------------------------------------- lifecycle
        private void Awake()
        {
            LoadCreditsText();
        }

        private void Start()
        {
            // Capture the starting anchored Y position (set by the build-scene tool to
            // viewport bottom so the first line scrolls in from the bottom).
            if (_content != null)
            {
                _startY = _content.anchoredPosition.y;
            }

            // Cache heights for the end-of-scroll check.
            if (_text != null)
            {
                _text.ForceMeshUpdate();
                _contentHeight = _text.preferredHeight;
                if (_content != null)
                {
                    var size = _content.sizeDelta;
                    size.y = _contentHeight;
                    _content.sizeDelta = size;
                }
            }
            if (_viewport != null)
            {
                _viewportHeight = _viewport.rect.height;
            }

            _elapsed = 0f;
            _finished = false;
            _exiting = false;

            Debug.Log($"[CreditsScroll] Started — contentH={_contentHeight:F0}px viewportH={_viewportHeight:F0}px speed={_scrollSpeedPxPerSec}px/s");
        }

        private void Update()
        {
            // Esc or Space — immediate skip to MainMenu
            if (!_exiting && SkipPressed())
            {
                Debug.Log("[CreditsScroll] Skip pressed — loading MainMenu.");
                ExitToMainMenu();
                return;
            }

            if (_finished || _content == null) return;

            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < _initialPauseSeconds) return;

            // Scroll content upward by increasing anchored Y.
            float scrolled = (_elapsed - _initialPauseSeconds) * _scrollSpeedPxPerSec;
            var pos = _content.anchoredPosition;
            pos.y = _startY + scrolled;
            _content.anchoredPosition = pos;

            // Have we scrolled the entire content past the viewport top?
            // Content travels upward by (contentHeight + viewportHeight) to clear the top.
            float distanceToScroll = _contentHeight + _viewportHeight;
            if (scrolled >= distanceToScroll + (_holdAtEndSeconds * _scrollSpeedPxPerSec))
            {
                _finished = true;
                Debug.Log("[CreditsScroll] Reached end of roll — returning to MainMenu.");
                ExitToMainMenu();
            }
        }

        // ---------------------------------------------------------------- input
        private static bool SkipPressed()
        {
            // Unity 6 Input System reads — no UnityEngine.Input legacy paths.
            var kb = Keyboard.current;
            if (kb == null) return false;
            return kb.escapeKey.wasPressedThisFrame
                || kb.spaceKey.wasPressedThisFrame
                || kb.enterKey.wasPressedThisFrame;
        }

        // ---------------------------------------------------------------- loading
        private void LoadCreditsText()
        {
            var asset = Resources.Load<TextAsset>(ResourcesPath);
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                Debug.LogError(
                    $"[CreditsScroll] Resources asset '{ResourcesPath}' missing or empty. " +
                    $"Expected path: '{ExpectedAssetPath}' generated from source '{ExpectedSourcePath}'. " +
                    $"Run Tartaria > UI > Build Credits Scene to regenerate. " +
                    $"Showing fallback text.");
                if (_text != null) _text.text = FallbackText;
                return;
            }

            if (_text == null)
            {
                Debug.LogError(
                    "[CreditsScroll] TextMeshProUGUI reference (_text) not assigned. " +
                    "Run Tartaria > UI > Build Credits Scene to author the scene with proper wiring.");
                return;
            }

            _text.text = StripMarkdownPreamble(asset.text);
            Debug.Log($"[CreditsScroll] Loaded credits ({asset.text.Length} chars) from Resources/{ResourcesPath}.");
        }

        /// <summary>
        /// Strips the YAML-style markdown preamble (lines starting with '>') used by the
        /// .md source as authoring notes. Players don't need to see "Pipeline:" guidance.
        /// </summary>
        private static string StripMarkdownPreamble(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            var lines = raw.Split('\n');
            var sb = new System.Text.StringBuilder(raw.Length);
            bool inHeader = true;
            int firstContentLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r');
                if (inHeader)
                {
                    // Header ends at the first horizontal-rule line "---" after a blank-or-quote block.
                    if (line.StartsWith("#") || line.StartsWith(">") || string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.Trim() == "---")
                    {
                        inHeader = false;
                        continue;
                    }
                    // First non-header content — exit header mode.
                    inHeader = false;
                }
                if (firstContentLine < 0 && !string.IsNullOrWhiteSpace(line))
                    firstContentLine = i;
                sb.AppendLine(line);
            }
            return sb.ToString().Trim();
        }

        // ---------------------------------------------------------------- exit
        private void ExitToMainMenu()
        {
            if (_exiting) return;
            _exiting = true;
            _finished = true;

            // SceneManager from UnityEngine.SceneManagement (Unity 6).
            if (string.IsNullOrEmpty(_returnSceneName))
            {
                Debug.LogError(
                    "[CreditsScroll] _returnSceneName is empty. Falling back to scene index 0 " +
                    "(typically Boot.unity, which re-activates the main-menu overlay).");
                SceneManager.LoadScene(0);
                return;
            }

            // Ensure normal time-scale before loading the next scene (credits may run during pause).
            Time.timeScale = 1f;
            SceneManager.LoadScene(_returnSceneName);
        }

        // ---------------------------------------------------------------- editor-wiring helpers
        /// <summary>Called by Moon1BuildCreditsScene to wire references after instantiation.</summary>
        public void WireReferences(RectTransform content, TextMeshProUGUI text, RectTransform viewport)
        {
            _content = content;
            _text = text;
            _viewport = viewport;
        }
    }
}
