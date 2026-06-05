using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tartaria.UI
{
    /// <summary>
    /// Sprint 6 Lane 1: Canvas-based Main Menu controller for the standalone MainMenu.unity scene.
    /// Wires the 5 menu buttons (New Game, Continue, Settings, Credits, Quit) to their target actions
    /// and renders Application.version in the bottom-right.
    ///
    /// Complements (does not replace) <see cref="MainMenuOverlay"/>, which is an IMGUI overlay
    /// bootstrapped on top of the Boot scene. This controller drives the dedicated MainMenu.unity
    /// scene built by the editor menu Tartaria/UI/Build Main Menu Scene.
    ///
    /// API CONTRACT compliance notes:
    /// - No namespace shadow of UnityEngine identifiers (uses Tartaria.UI only).
    /// - No deprecated Unity 6 API (no FindObjectOfType).
    /// - Continue button opens SaveSlotsMenu overlay (Sprint 8 Lane 6) which routes Load
    ///   to SaveManager.SwitchToSlot(int) at Assets/_Project/Scripts/Save/SaveManager.cs:595,
    ///   per docs/agents/API_CONTRACT.md section 3 (NOT a non-existent LoadSlot(int)).
    /// - Settings opens <see cref="SettingsOverlay"/>.Open() — verified at SettingsOverlay.cs:104.
    /// - No silent catches: every catch logs file:line + the offending value.
    /// - No stubs: every button handler does the real thing.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuController : MonoBehaviour
    {
        // ─── Scene names (match what the build picks up) ─────────────────────
        public const string GAMEPLAY_SCENE_NAME = "Echohaven_VerticalSlice";
        public const string CREDITS_SCENE_NAME = "Credits";

        // ─── Inspector wiring — assigned by the scene builder ────────────────
        [Header("Buttons")]
        [SerializeField] Button _newGameButton;
        [SerializeField] Button _continueButton;
        [SerializeField] Button _settingsButton;
        [SerializeField] Button _creditsButton;
        [SerializeField] Button _quitButton;

        [Header("Labels")]
        [SerializeField] TMP_Text _titleLabel;
        [SerializeField] TMP_Text _subtitleLabel;
        [SerializeField] TMP_Text _versionLabel;

        [Header("Behaviour")]
        [Tooltip("Title shown at top of the menu.")]
        [SerializeField] string _titleText = "TARTARIA WORLD OF WONDER";

        [Tooltip("Subtitle shown under the title.")]
        [SerializeField] string _subtitleText = "Aether Awakening";

        void Awake()
        {
            ApplyLabels();
            WireButtons();
        }

        void OnEnable()
        {
            // Refresh in case Application.version updated between scene reloads
            ApplyVersionLabel();
        }

        // ─── Label setup ─────────────────────────────────────────────────────

        void ApplyLabels()
        {
            if (_titleLabel != null) _titleLabel.text = _titleText;
            if (_subtitleLabel != null) _subtitleLabel.text = _subtitleText;
            ApplyVersionLabel();
        }

        void ApplyVersionLabel()
        {
            if (_versionLabel == null) return;
            string version = Application.version;
            if (string.IsNullOrEmpty(version)) version = "alpha";
            _versionLabel.text = $"v{version}";
        }

        // ─── Button wiring ───────────────────────────────────────────────────

        void WireButtons()
        {
            BindButton(_newGameButton, OnNewGame, "NewGame");
            BindButton(_continueButton, OnContinue, "Continue");
            BindButton(_settingsButton, OnSettings, "Settings");
            BindButton(_creditsButton, OnCredits, "Credits");
            BindButton(_quitButton, OnQuit, "Quit");

            // Settings panel discovery — if Settings UI is missing entirely, disable + warn loud.
            // We reflect against Tartaria.UI.SettingsOverlay to keep this code resilient if the
            // settings UI is replaced by a prefab variant in a later sprint.
            if (_settingsButton != null && !IsSettingsPanelAvailable(out string settingsReason))
            {
                _settingsButton.interactable = false;
                Debug.LogWarning(
                    $"[MainMenuController] Settings button disabled (MainMenuController.cs:WireButtons) — reason: {settingsReason}. " +
                    "Sibling lane is expected to extract the Pause settings panel into its own prefab.");
            }
        }

        void BindButton(Button b, UnityEngine.Events.UnityAction action, string label)
        {
            if (b == null)
            {
                Debug.LogError(
                    $"[MainMenuController] Button '{label}' is NOT assigned in inspector (MainMenuController.cs:WireButtons). " +
                    "Either run Tartaria/UI/Build Main Menu Scene to regenerate, or wire the field manually.");
                return;
            }
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }

        // ─── Button handlers ─────────────────────────────────────────────────

        void OnNewGame()
        {
            Debug.Log($"[MainMenuController] New Game pressed — loading '{GAMEPLAY_SCENE_NAME}'.");
            try
            {
                SceneManager.LoadScene(GAMEPLAY_SCENE_NAME);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MainMenuController] OnNewGame failed (MainMenuController.cs:OnNewGame) — scene='{GAMEPLAY_SCENE_NAME}' ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void OnContinue()
        {
            // Sprint 8 Lane 6 (audit blocker #4 wire-up): the Continue button now opens
            // the SaveSlotsMenu overlay so the player can pick which slot to load.
            // The overlay's individual slot cards then call SaveManager.SwitchToSlot(int)
            // (canonical at Assets/_Project/Scripts/Save/SaveManager.cs:595) per
            // docs/agents/API_CONTRACT.md section 3. We no longer reflection-call QuickLoad here.
            Debug.Log("[MainMenuController] Continue pressed - opening SaveSlotsMenu overlay (SaveSlotsMenu.Open).");
            try
            {
                SaveSlotsMenu.Open();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MainMenuController] OnContinue failed to open SaveSlotsMenu (MainMenuController.cs:OnContinue) - ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void OnSettings()
        {
            Debug.Log("[MainMenuController] Settings pressed — opening SettingsOverlay.");
            try
            {
                if (!IsSettingsPanelAvailable(out string reason))
                {
                    Debug.LogWarning(
                        $"[MainMenuController] OnSettings (MainMenuController.cs:OnSettings) — settings panel unavailable: {reason}.");
                    return;
                }
                SettingsOverlay.Open();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MainMenuController] OnSettings failed (MainMenuController.cs:OnSettings) — ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void OnCredits()
        {
            Debug.Log($"[MainMenuController] Credits pressed — loading '{CREDITS_SCENE_NAME}'.");
            try
            {
                // Sibling lane builds Credits.unity. If it isn't in Build Settings yet, Unity throws.
                // We catch and log the exact missing-scene value so the dispatcher knows what to fix.
                SceneManager.LoadScene(CREDITS_SCENE_NAME);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MainMenuController] OnCredits failed (MainMenuController.cs:OnCredits) — scene='{CREDITS_SCENE_NAME}' not in Build Settings or missing. " +
                    $"ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void OnQuit()
        {
            Debug.Log("[MainMenuController] Quit pressed.");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Discovers whether a usable settings panel is reachable. Currently checks for
        /// <see cref="SettingsOverlay"/>.Open() — the canonical settings UI. If a later sprint
        /// extracts settings into its own prefab, swap the discovery here without touching
        /// the button-handler code path.
        /// </summary>
        bool IsSettingsPanelAvailable(out string reason)
        {
            // SettingsOverlay is part of Tartaria.UI — its presence is guaranteed at compile time,
            // but we still verify the Open() method is intact in case it's renamed in the future.
            MethodInfo open = typeof(SettingsOverlay).GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
            if (open == null)
            {
                reason = "Tartaria.UI.SettingsOverlay.Open() not found (signature changed?)";
                return false;
            }
            reason = "ok";
            return true;
        }
    }
}
