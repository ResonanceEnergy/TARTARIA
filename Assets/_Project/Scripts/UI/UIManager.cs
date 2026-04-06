using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.UI
{
    /// <summary>
    /// UI Manager — central coordinator for all UI panels and screens:
    ///   - HUD (in-game overlay)
    ///   - Pause Menu
    ///   - Settings
    ///   - Aether Vision overlay
    ///   - Dialogue box
    ///   - Save/Load indicators
    ///
    /// Responds to GameStateManager transitions.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panel References")]
        [SerializeField] GameObject hudPanel;
        [SerializeField] GameObject pauseMenuPanel;
        [SerializeField] GameObject settingsPanel;
        [SerializeField] GameObject dialoguePanel;
        [SerializeField] GameObject loadingPanel;
        [SerializeField] GameObject aetherVisionOverlay;

        [Header("Dialogue")]
        [SerializeField] TMPro.TextMeshProUGUI dialogueSpeakerText;
        [SerializeField] TMPro.TextMeshProUGUI dialogueBodyText;
        [SerializeField] UnityEngine.UI.Image dialoguePortrait;

        [Header("Loading")]
        [SerializeField] UnityEngine.UI.Image loadingBar;
        [SerializeField] TMPro.TextMeshProUGUI loadingTipText;

        [Header("Save Indicator")]
        [SerializeField] GameObject saveIndicator;
        [SerializeField, Min(0.1f)] float saveIndicatorDuration = 2f;

        float _saveIndicatorTimer;
        bool _aetherVisionActive;
        float _prePauseTimeScale = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChange;
            GameEvents.OnToggleAetherVision += ToggleAetherVision;
            GameEvents.OnTogglePause += TogglePause;
        }

        void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChange;
            GameEvents.OnToggleAetherVision -= ToggleAetherVision;
            GameEvents.OnTogglePause -= TogglePause;
        }

        void Update()
        {
            // Save indicator auto-hide
            if (_saveIndicatorTimer > 0)
            {
                _saveIndicatorTimer -= Time.unscaledDeltaTime;
                if (_saveIndicatorTimer <= 0 && saveIndicator != null)
                    saveIndicator.SetActive(false);
            }
        }

        // ─── State Management ────────────────────────

        void HandleStateChange(GameState previous, GameState current)
        {
            // Manage panel visibility by game state
            SetPanelActive(hudPanel, current == GameState.Exploration ||
                                     current == GameState.Combat ||
                                     current == GameState.Tuning);
            SetPanelActive(pauseMenuPanel, current == GameState.Paused);
            SetPanelActive(loadingPanel, current == GameState.Loading);

            if (current == GameState.Paused)
            {
                _prePauseTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else if (previous == GameState.Paused)
                Time.timeScale = _prePauseTimeScale;
        }

        void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

        // ─── Public API ──────────────────────────────

        public void TogglePause()
        {
            var state = GameStateManager.Instance;
            if (state == null) return;
            if (state.CurrentState == GameState.Paused)
            {
                state.ReturnToPrevious();
                AudioManager.Instance?.PlaySFX2D("UIClose");
            }
            else if (state.IsPlaying)
            {
                state.TransitionTo(GameState.Paused);
                AudioManager.Instance?.PlaySFX2D("UIOpen");
            }
        }

        public void ShowDialogue(string speaker, string text, Sprite portrait = null)
        {
            if (string.IsNullOrEmpty(speaker) || string.IsNullOrEmpty(text)) return;
            SetPanelActive(dialoguePanel, true);
            if (dialogueSpeakerText != null) dialogueSpeakerText.text = speaker;
            if (dialogueBodyText != null) dialogueBodyText.text = text;
            if (dialoguePortrait != null && portrait != null) dialoguePortrait.sprite = portrait;
        }

        public void HideDialogue()
        {
            SetPanelActive(dialoguePanel, false);
        }

        public void ToggleAetherVision()
        {
            _aetherVisionActive = !_aetherVisionActive;
            SetPanelActive(aetherVisionOverlay, _aetherVisionActive);
            AudioManager.Instance?.PlaySFX2D(_aetherVisionActive ? "AetherVisionOn" : "AetherVisionOff");
        }

        public void ShowSaveIndicator()
        {
            if (saveIndicator != null)
            {
                saveIndicator.SetActive(true);
                _saveIndicatorTimer = saveIndicatorDuration;
            }
            AudioManager.Instance?.PlaySFX2D("SaveConfirm");
        }

        public void UpdateLoadingProgress(float progress, string tip = null)
        {
            if (loadingBar != null)
                loadingBar.fillAmount = progress;
            if (loadingTipText != null && tip != null)
                loadingTipText.text = tip;
        }

        public void ShowSettings()
        {
            SetPanelActive(settingsPanel, true);
            SetPanelActive(pauseMenuPanel, false);
        }

        public void HideSettings()
        {
            SetPanelActive(settingsPanel, false);
            SetPanelActive(pauseMenuPanel, true);
        }

        /// <summary>
        /// Sets the alpha of the full-screen fade CanvasGroup (used by zone transitions).
        /// </summary>
        public void SetFadeAlpha(float alpha)
        {
            // CanvasGroup-based fade — look for a CanvasGroup on the loading panel
            if (loadingPanel != null)
            {
                var cg = loadingPanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = alpha;
            }
        }
    }
}
