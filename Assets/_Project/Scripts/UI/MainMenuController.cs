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
    /// - Continue button uses reflection against <see cref="Tartaria.Save.SaveManager"/>.QuickLoad
    ///   (verified canonical method at Assets/_Project/Scripts/Save/SaveManager.cs:246 —
    ///   there is NO LoadSlot(int) method on SaveManager; QuickLoad() is the canonical reload API).
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
            Debug.Log("[MainMenuController] Continue pressed — invoking SaveManager.QuickLoad via reflection.");
            try
            {
                // SaveManager lives in Tartaria.Save assembly which Tartaria.UI already references,
                // but per the lane spec we reflection-call to stay loose-coupled against future
                // signature changes. Resolves Tartaria.Save.SaveManager + reads the Instance singleton,
                // then invokes QuickLoad().
                Type saveManagerType = Type.GetType("Tartaria.Save.SaveManager, Tartaria.Save");
                if (saveManagerType == null)
                {
                    Debug.LogError(
                        "[MainMenuController] OnContinue (MainMenuController.cs:OnContinue) — could not resolve Type 'Tartaria.Save.SaveManager, Tartaria.Save'. " +
                        "Did the Save assembly rename? Check Assets/_Project/Scripts/Save/Tartaria.Save.asmdef.");
                    return;
                }

                PropertyInfo instanceProp = saveManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    Debug.LogError(
                        "[MainMenuController] OnContinue (MainMenuController.cs:OnContinue) — SaveManager.Instance static property not found. " +
                        "Expected at SaveManager.cs:36 (public static SaveManager Instance { get; private set; }).");
                    return;
                }

                object instance = instanceProp.GetValue(null);
                if (instance == null)
                {
                    Debug.LogWarning(
                        "[MainMenuController] OnContinue (MainMenuController.cs:OnContinue) — SaveManager.Instance is null. " +
                        "Save subsystem not bootstrapped yet — falling through to New Game flow so the player isn't blocked.");
                    SceneManager.LoadScene(GAMEPLAY_SCENE_NAME);
                    return;
                }

                MethodInfo quickLoad = saveManagerType.GetMethod("QuickLoad", BindingFlags.Public | BindingFlags.Instance);
                if (quickLoad == null)
                {
                    Debug.LogError(
                        "[MainMenuController] OnContinue (MainMenuController.cs:OnContinue) — SaveManager.QuickLoad() method not found. " +
                        "Expected canonical signature 'public void QuickLoad()' at SaveManager.cs:246.");
                    return;
                }

                quickLoad.Invoke(instance, null);

                // QuickLoad re-reads disk + broadcasts OnAfterLoad — load the gameplay scene so we're
                // actually inside the game instead of staring at the main menu.
                SceneManager.LoadScene(GAMEPLAY_SCENE_NAME);
            }
            catch (TargetInvocationException tie)
            {
                Exception inner = tie.InnerException ?? tie;
                Debug.LogError(
                    $"[MainMenuController] OnContinue threw inside SaveManager.QuickLoad (MainMenuController.cs:OnContinue) — inner={inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MainMenuController] OnContinue failed (MainMenuController.cs:OnContinue) — ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
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
