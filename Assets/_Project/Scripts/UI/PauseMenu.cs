using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// MVP Pause Menu (2026-06-01 lane: UI Programmer).
    ///
    /// Wires Escape key → Toggle, Resume → Hide, Settings → loud "not in MVP"
    /// warning (per NO-DEBT rule 4: never silent), Quit → exit to editor stop
    /// or Application.Quit on a built player.
    ///
    /// Canvas sortingOrder = 30000 (below WinScreen at 32000, above HUD).
    ///
    /// Per CLAUDE.md NO-DEBT mandate (2026-06-02):
    /// - No silent fails (rule 3): catch blocks rethrow or log loud
    /// - No silent fallbacks (rule 4): missing Canvas logs error with GO path + fix
    /// - Singleton .Instance exposed so reflection-based callers (soak test,
    ///   click-to-tune harness) can drive Toggle() without scene lookups.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenu : MonoBehaviour
    {
        // ---- Singleton ----
        public static PauseMenu Instance { get; private set; }

        // ---- Canvas / state ----
        [Tooltip("Sorting order for the pause Canvas. WinScreen sits at 32000, HUD at 10000-20000. Pause at 30000 puts it above gameplay HUD but below the WinScreen.")]
        [SerializeField] private int sortingOrder = 30000;

        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Buttons (optional — wired via Inspector or auto-found by name)")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Main menu fallback (optional)")]
        [Tooltip("Scene name to load on Quit when not running in the Editor. Leave empty to call Application.Quit instead.")]
        [SerializeField] private string mainMenuSceneName = "";

        public bool IsShown { get; private set; }

        // ---- Lifecycle ----
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[PauseMenu] Duplicate instance detected on '{GetHierarchyPath(gameObject)}'. Destroying this duplicate; existing Instance is on '{GetHierarchyPath(Instance.gameObject)}'.");
                Destroy(this);
                return;
            }
            Instance = this;

            // Auto-resolve Canvas if not assigned in Inspector.
            if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
                // Ensure overlay rendering regardless of camera reference state.
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                {
                    Debug.LogWarning($"[PauseMenu] Canvas on '{GetHierarchyPath(canvas.gameObject)}' is ScreenSpaceCamera with no worldCamera assigned. Falling back to ScreenSpaceOverlay so the pause UI still renders.");
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            // Start hidden.
            ApplyShownState(false);

            // Wire button handlers if assigned.
            WireButtons();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Defensive: restore time scale if we owned the pause.
            if (IsShown && Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = 1f;
            }

            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void Update()
        {
            // Esc → Toggle. Input System path (project uses Input System Package, NOT
            // the legacy UnityEngine.Input — see CLAUDE.md F310 section).
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        // ---- Public API ----

        /// <summary>Flip the pause state. Sets Time.timeScale and CanvasGroup interactivity.</summary>
        public void Toggle()
        {
            ApplyShownState(!IsShown);
        }

        public void Show() { ApplyShownState(true); }
        public void Hide() { ApplyShownState(false); }

        // Legacy aliases preserved so existing references continue to compile.
        public void Open()  { Show(); }
        public void Close() { Hide(); }

        // ---- Internals ----

        private void ApplyShownState(bool shown)
        {
            IsShown = shown;

            // Per rule 4 (no silent fallbacks): if Toggle is called with no Canvas,
            // log an error with the GameObject's hierarchy path + remediation hint.
            // We still flip Time.timeScale because pausing is the load-bearing
            // contract — UI visibility is the cosmetic half of that contract.
            if (canvas == null && canvasGroup == null)
            {
                Debug.LogError($"[PauseMenu] Toggle() called but no Canvas or CanvasGroup is wired on '{GetHierarchyPath(gameObject)}'. Assign 'canvas' and 'canvasGroup' in the Inspector, or place a child Canvas with a CanvasGroup under this GameObject. Time.timeScale will still flip so gameplay pause works; only the visual overlay is missing.");
            }

            if (canvas != null) canvas.enabled = shown;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = shown ? 1f : 0f;
                canvasGroup.interactable = shown;
                canvasGroup.blocksRaycasts = shown;
            }

            Time.timeScale = shown ? 0f : 1f;

            // Cursor management — let mouse free when paused so buttons are clickable.
            // Restore previous lock state otherwise. We don't try to snapshot the
            // pre-pause cursor state because Echohaven gameplay defaults to Confined
            // for the orbit camera; ConfineLockState.None when paused, Confined when not.
            try
            {
                if (shown)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true; // Echohaven uses visible cursor for click-to-interact.
                }
            }
            catch (Exception ex)
            {
                // Rethrow per NO-DEBT rule 3. Cursor calls should never throw, so if
                // they do something is genuinely wrong (e.g. running headless without
                // a display) and we want the failure surfaced.
                Debug.LogError($"[PauseMenu] Cursor state update failed on '{GetHierarchyPath(gameObject)}': {ex}");
                throw;
            }
        }

        private void WireButtons()
        {
            // Auto-find by name if not assigned in Inspector. Walk children.
            if (resumeButton == null)   resumeButton   = FindButtonByName("Resume", "ResumeButton", "Btn_Resume");
            if (settingsButton == null) settingsButton = FindButtonByName("Settings", "SettingsButton", "Btn_Settings");
            if (quitButton == null)     quitButton     = FindButtonByName("Quit", "QuitButton", "Btn_Quit", "Exit", "ExitButton");

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
                resumeButton.onClick.AddListener(OnResumeClicked);
            }
            else
            {
                Debug.LogWarning($"[PauseMenu] Resume button not found on '{GetHierarchyPath(gameObject)}'. Wire 'resumeButton' in the Inspector or name a child Button 'Resume' / 'ResumeButton' / 'Btn_Resume'.");
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            else
            {
                Debug.LogWarning($"[PauseMenu] Settings button not found on '{GetHierarchyPath(gameObject)}'. Wire 'settingsButton' in the Inspector or name a child Button 'Settings' / 'SettingsButton' / 'Btn_Settings'.");
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            else
            {
                Debug.LogWarning($"[PauseMenu] Quit button not found on '{GetHierarchyPath(gameObject)}'. Wire 'quitButton' in the Inspector or name a child Button 'Quit' / 'QuitButton' / 'Btn_Quit'.");
            }
        }

        private Button FindButtonByName(params string[] candidateNames)
        {
            // Search all child Buttons (active and inactive) for a name match.
            var buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var b = buttons[i];
                if (b == null) continue;
                for (int n = 0; n < candidateNames.Length; n++)
                {
                    if (string.Equals(b.gameObject.name, candidateNames[n], StringComparison.OrdinalIgnoreCase))
                    {
                        return b;
                    }
                }
            }
            return null;
        }

        // ---- Button handlers ----

        private void OnResumeClicked()
        {
            Hide();
        }

        private void OnSettingsClicked()
        {
            // Per NO-DEBT rule 4 (no silent fallbacks): warn loud so playtesters
            // and devs see the gap, AND surface a HUD banner so the user gets
            // visible feedback instead of a dead button.
            Debug.LogWarning("[PauseMenu] Settings menu not in MVP — Moon 1 ship priority. See ROADMAP Phase 2.");

            try
            {
                // GameEvents.RaiseHUDShowBanner(title, subtitle, duration) — signature
                // verified at Assets/_Project/Scripts/Core/GameEvents.cs:623.
                GameEvents.RaiseHUDShowBanner(
                    "Settings",
                    "Not in MVP — Moon 1 ship priority. See ROADMAP Phase 2.",
                    4f);
            }
            catch (Exception ex)
            {
                // Log loud per rule 3. Swallow is correct here because a banner
                // failure must not block the Resume/Quit buttons from working — the
                // Debug.LogWarning above already documented the gap.
                Debug.LogError($"[PauseMenu] Failed to raise HUD banner for Settings stub on '{GetHierarchyPath(gameObject)}': {ex}");
            }
        }

        private void OnQuitClicked()
        {
            // Restore time scale before quitting so the editor doesn't stay stuck at 0.
            Time.timeScale = 1f;

            // If a main-menu scene is configured AND we're in a player build, prefer
            // returning to the main menu over hard-quitting the app.
            if (!Application.isEditor && !string.IsNullOrEmpty(mainMenuSceneName))
            {
                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
                    return;
                }
                catch (Exception ex)
                {
                    // Log loud per rule 3, then fall through to Application.Quit so the
                    // player still has a way out.
                    Debug.LogError($"[PauseMenu] Failed to load main menu scene '{mainMenuSceneName}': {ex}. Falling back to Application.Quit.");
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---- Utility ----

        private static string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "<null>";
            var t = go.transform;
            var path = go.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
