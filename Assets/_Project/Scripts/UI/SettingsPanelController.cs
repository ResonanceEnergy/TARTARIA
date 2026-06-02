// SettingsPanelController.cs
// Sprint 7 Lane 5 — agent/ui/pause-settings-extract
// Owner: UI agent. Path: Assets/_Project/Scripts/UI/SettingsPanelController.cs
//
// Real Canvas-backed settings panel controller. Replaces the IMGUI flow in
// SettingsMenu.cs (Sprint 6 Lane 2) with a prefab-driven Canvas UI so that
// both the Main Menu and the Pause Menu can spawn the same panel.
//
// This controller is wired by BuildSettingsPanelPrefab.cs (Editor) onto the
// generated prefab at Resources/UI/SettingsPanel.prefab. The Editor builder
// assigns every serialized reference; this script does NOT call FindGameObject
// at runtime. Apply / Cancel reuse SettingsPersistence from Lane 2 so the
// versioned TARTARIA_SET_* PlayerPrefs keys remain the single source of truth.
//
// API_CONTRACT compliance:
//   - Namespace Tartaria.UI is not in the banned list.
//   - Unity 6: FindFirstObjectByType(FindObjectsInactive.Include) only.
//   - No silent catches (every fallback path logs).
//   - TextMeshPro components (TMP_Text / TMP_Dropdown), not legacy uGUI Text.
//   - SettingsPersistence (Sprint 6 Lane 2) is the canonical store. If the
//     file is missing at runtime, the static Open() guard logs loudly
//     ("MISSING: SettingsPersistence ...") — but in this branch the merge
//     brought it in so it IS present.
//
// Public API:
//   SettingsPanelController.Open()  — spawns prefab under main Canvas,
//                                     timeScale = 0, cursor unlocked.
//   SettingsPanelController.Close() — destroys instance, restores timeScale.
//   instance.Apply()                — writes versioned prefs + pushes to
//                                     AudioMixer / Screen / Camera / Rumble.
//   instance.Cancel()               — discards edits, restores from prefs.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Tartaria.Audio;

namespace Tartaria.UI
{
    /// <summary>
    /// Canvas-backed reusable settings panel. Authored by
    /// <c>BuildSettingsPanelPrefab</c> Editor menu; spawned at runtime via
    /// <see cref="Open"/> from Main Menu or Pause Menu.
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsPanelController : MonoBehaviour
    {
        // === Resource path (must match Editor builder) ================
        public const string ResourcePath = "UI/SettingsPanel";

        // === Singleton (one panel at a time) ==========================
        static SettingsPanelController _instance;
        public static SettingsPanelController Instance => _instance;
        public static bool IsOpen => _instance != null;

        // === Serialized references (assigned by the Editor builder) ===
        [Header("Display Section")]
        [SerializeField] TMP_Dropdown _resolutionDropdown;
        [SerializeField] Toggle       _fullscreenToggle;

        [Header("Audio Section")]
        [SerializeField] Slider _masterVolumeSlider;
        [SerializeField] Slider _musicVolumeSlider;
        [SerializeField] Slider _sfxVolumeSlider;
        [SerializeField] TMP_Text _masterVolumeValueLabel;
        [SerializeField] TMP_Text _musicVolumeValueLabel;
        [SerializeField] TMP_Text _sfxVolumeValueLabel;

        [Header("Input Section")]
        [SerializeField] Toggle _invertYToggle;
        [SerializeField] Toggle _rumbleToggle;

        [Header("Language Section")]
        [SerializeField] TMP_Dropdown _languageDropdown;
        [SerializeField] TMP_Text     _languageWarningLabel;

        [Header("Buttons")]
        [SerializeField] Button _applyButton;
        [SerializeField] Button _cancelButton;

        // === Mixer ====================================================
        // Matches SettingsMenu.cs (Sprint 6 Lane 2). Verified against
        // Assets/_Project/Audio/Mixers/MasterMixer.mixer lines 110/112/114.
        const string MIXER_PARAM_MASTER = "MasterVol";
        const string MIXER_PARAM_MUSIC  = "MusicVol";
        const string MIXER_PARAM_SFX    = "SFXVol";
        AudioMixer _mixer;

        // === Resolution model =========================================
        Resolution[] _resolutions = System.Array.Empty<Resolution>();
        readonly List<string> _resolutionLabels = new List<string>(32);

        // === Language model (mirrors SettingsMenu.cs LANGUAGES) ========
        struct LanguageEntry
        {
            public string code;
            public string label;
            public bool   available;
            public LanguageEntry(string c, string l, bool a) { code = c; label = l; available = a; }
        }
        static readonly LanguageEntry[] LANGUAGES =
        {
            new LanguageEntry("en",    "English",                     true),
            new LanguageEntry("fr",    "Francais (Coming)",           false),
            new LanguageEntry("es",    "Espanol (Coming)",            false),
            new LanguageEntry("de",    "Deutsch (Coming)",            false),
            new LanguageEntry("ja",    "Japanese (Coming)",           false),
            new LanguageEntry("pt-BR", "Portugues (Brasil) (Coming)", false),
            new LanguageEntry("ru",    "Russian (Coming)",            false),
            new LanguageEntry("it",    "Italiano (Coming)",           false),
            new LanguageEntry("zh-CN", "Simplified Chinese (Coming)", false),
            new LanguageEntry("ko",    "Korean (Coming)",             false),
            new LanguageEntry("ar",    "Arabic (Coming)",             false),
            new LanguageEntry("hi",    "Hindi (Coming)",              false),
            new LanguageEntry("tr",    "Turkce (Coming)",             false),
        };

        // === Timing restore ===========================================
        static float _restoreTimeScale = 1f;
        static bool  _wasCursorLocked;

        // === Static Open / Close ======================================

        /// <summary>
        /// Spawn the Canvas-backed settings panel under the main Canvas (or
        /// a freshly-created Canvas if none exists). Pauses time and unlocks
        /// the cursor. Idempotent — calling twice returns the existing panel.
        /// </summary>
        public static SettingsPanelController Open()
        {
            if (_instance != null)
            {
                Debug.Log("[SettingsPanelController] Open() called while already open — returning existing instance.");
                return _instance;
            }

            // SettingsPersistence availability check (loud if missing).
            if (!IsPersistenceAvailable())
            {
                Debug.LogError(
                    "[SettingsPanelController] MISSING: SettingsPersistence (Sprint 6 Lane 2). " +
                    "Falling back to raw PlayerPrefs reads/writes against TARTARIA_SET_* keys. " +
                    "Merge agent/ui/settings-menu-real or restore Assets/_Project/Scripts/UI/SettingsPersistence.cs.");
            }

            var prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab == null)
            {
                Debug.LogError(
                    $"[SettingsPanelController] Resources.Load failed for '{ResourcePath}'. " +
                    "Run Tartaria/UI/Build Settings Panel Prefab to author the prefab.");
                return null;
            }

            var canvas = FindOrCreateRootCanvas();
            var go = Instantiate(prefab, canvas.transform, false);
            go.name = "SettingsPanel(Instance)";

            var controller = go.GetComponent<SettingsPanelController>();
            if (controller == null)
            {
                Debug.LogError(
                    $"[SettingsPanelController] Prefab '{ResourcePath}' is missing the SettingsPanelController component. " +
                    "Re-run Tartaria/UI/Build Settings Panel Prefab.");
                Destroy(go);
                return null;
            }

            _instance = controller;

            // Pause time + unlock cursor (remember previous state).
            _restoreTimeScale = Time.timeScale;
            _wasCursorLocked  = Cursor.lockState == CursorLockMode.Locked;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            controller.OnSpawned();
            return controller;
        }

        /// <summary>
        /// Destroy the active panel instance and restore timeScale + cursor.
        /// Safe to call when no panel is open.
        /// </summary>
        public static void Close()
        {
            if (_instance == null)
            {
                Debug.Log("[SettingsPanelController] Close() called but no panel open — no-op.");
                return;
            }

            var go = _instance.gameObject;
            _instance = null;

            Time.timeScale = _restoreTimeScale;
            if (_wasCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (go != null) Destroy(go);
            Debug.Log("[SettingsPanelController] Closed — timeScale restored to " + _restoreTimeScale.ToString("F2"));
        }

        // === Spawn lifecycle ==========================================
        void OnSpawned()
        {
            SettingsPersistence.EnsureSchema();
            _mixer = MasterMixerLocator.Load();
            if (_mixer == null)
            {
                Debug.LogWarning(
                    "[SettingsPanelController] MasterMixerLocator returned null - audio slider writes will " +
                    "not reach the AudioMixer. Place a MasterMixerLocator ScriptableObject at " +
                    "Resources/MasterMixerLocator.asset.");
            }

            PopulateResolutionDropdown();
            PopulateLanguageDropdown();
            LoadFromPrefsIntoUI();
            HookButtonEvents();
            UpdateVolumeLabels();
            UpdateLanguageWarning();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // === Hookups (called once at spawn) ===========================
        void HookButtonEvents()
        {
            if (_applyButton != null)
            {
                _applyButton.onClick.RemoveAllListeners();
                _applyButton.onClick.AddListener(Apply);
            }
            else
            {
                Debug.LogWarning("[SettingsPanelController] _applyButton is not wired.");
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(Cancel);
            }
            else
            {
                Debug.LogWarning("[SettingsPanelController] _cancelButton is not wired.");
            }

            // Live-update volume labels as sliders move.
            if (_masterVolumeSlider != null) _masterVolumeSlider.onValueChanged.AddListener(_ => UpdateVolumeLabels());
            if (_musicVolumeSlider  != null) _musicVolumeSlider .onValueChanged.AddListener(_ => UpdateVolumeLabels());
            if (_sfxVolumeSlider    != null) _sfxVolumeSlider   .onValueChanged.AddListener(_ => UpdateVolumeLabels());
            if (_languageDropdown   != null) _languageDropdown.onValueChanged.AddListener(_ => UpdateLanguageWarning());
        }

        // === Resolution / Language dropdowns ==========================
        void PopulateResolutionDropdown()
        {
            if (_resolutionDropdown == null)
            {
                Debug.LogWarning("[SettingsPanelController] _resolutionDropdown is not wired.");
                return;
            }

            var raw = Screen.resolutions;
            var list = new List<Resolution>(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].refreshRateRatio.value >= 50.0) list.Add(raw[i]);
            }
            if (list.Count == 0 && raw.Length > 0)
            {
                Debug.LogWarning(
                    "[SettingsPanelController] No resolutions >= 50Hz - falling back to full Screen.resolutions.");
                list.AddRange(raw);
            }
            _resolutions = list.ToArray();

            _resolutionDropdown.ClearOptions();
            _resolutionLabels.Clear();
            for (int i = 0; i < _resolutions.Length; i++)
            {
                var r = _resolutions[i];
                int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
                _resolutionLabels.Add($"{r.width} x {r.height} @ {hz}Hz");
            }
            _resolutionDropdown.AddOptions(_resolutionLabels);
        }

        void PopulateLanguageDropdown()
        {
            if (_languageDropdown == null)
            {
                Debug.LogWarning("[SettingsPanelController] _languageDropdown is not wired.");
                return;
            }
            _languageDropdown.ClearOptions();
            var labels = new List<string>(LANGUAGES.Length);
            for (int i = 0; i < LANGUAGES.Length; i++) labels.Add(LANGUAGES[i].label);
            _languageDropdown.AddOptions(labels);
        }

        // === Load / Apply / Cancel ====================================
        void LoadFromPrefsIntoUI()
        {
            float master = SettingsPersistence.LoadFloat(SettingsPersistence.K_MasterVolume, SettingsPersistence.DefaultMasterVolume);
            float music  = SettingsPersistence.LoadFloat(SettingsPersistence.K_MusicVolume,  SettingsPersistence.DefaultMusicVolume);
            float sfx    = SettingsPersistence.LoadFloat(SettingsPersistence.K_SFXVolume,    SettingsPersistence.DefaultSFXVolume);
            bool fs      = SettingsPersistence.LoadBool (SettingsPersistence.K_Fullscreen,   SettingsPersistence.DefaultFullscreen);
            bool inv     = SettingsPersistence.LoadBool (SettingsPersistence.K_InvertY,      SettingsPersistence.DefaultInvertY);
            bool rumble  = SettingsPersistence.LoadBool (SettingsPersistence.K_Rumble,       SettingsPersistence.DefaultRumble);
            int  resIdx  = SettingsPersistence.LoadInt  (SettingsPersistence.K_ResolutionIdx, GuessCurrentResolutionIdx());
            string lang  = SettingsPersistence.LoadString(SettingsPersistence.K_LanguageCode, SettingsPersistence.DefaultLanguageCode);

            if (_masterVolumeSlider != null) _masterVolumeSlider.SetValueWithoutNotify(master);
            if (_musicVolumeSlider  != null) _musicVolumeSlider .SetValueWithoutNotify(music);
            if (_sfxVolumeSlider    != null) _sfxVolumeSlider   .SetValueWithoutNotify(sfx);
            if (_fullscreenToggle   != null) _fullscreenToggle.SetIsOnWithoutNotify(fs);
            if (_invertYToggle      != null) _invertYToggle.SetIsOnWithoutNotify(inv);
            if (_rumbleToggle       != null) _rumbleToggle.SetIsOnWithoutNotify(rumble);
            if (_resolutionDropdown != null && _resolutions.Length > 0)
            {
                _resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(resIdx, 0, _resolutions.Length - 1));
                _resolutionDropdown.RefreshShownValue();
            }
            if (_languageDropdown   != null)
            {
                _languageDropdown.SetValueWithoutNotify(LookupLanguageIdx(lang));
                _languageDropdown.RefreshShownValue();
            }
        }

        /// <summary>
        /// Stage all UI state into PlayerPrefs (versioned TARTARIA_SET_* keys),
        /// push to AudioMixer / Screen / CameraController / Gamepad, commit,
        /// then close.
        /// </summary>
        public void Apply()
        {
            float master = _masterVolumeSlider != null ? _masterVolumeSlider.value : SettingsPersistence.DefaultMasterVolume;
            float music  = _musicVolumeSlider  != null ? _musicVolumeSlider .value : SettingsPersistence.DefaultMusicVolume;
            float sfx    = _sfxVolumeSlider    != null ? _sfxVolumeSlider   .value : SettingsPersistence.DefaultSFXVolume;
            bool fs      = _fullscreenToggle   != null ? _fullscreenToggle.isOn    : SettingsPersistence.DefaultFullscreen;
            bool inv     = _invertYToggle      != null ? _invertYToggle.isOn       : SettingsPersistence.DefaultInvertY;
            bool rumble  = _rumbleToggle       != null ? _rumbleToggle.isOn        : SettingsPersistence.DefaultRumble;
            int  resIdx  = _resolutionDropdown != null ? _resolutionDropdown.value : 0;
            int  langIdx = _languageDropdown   != null ? _languageDropdown.value   : 0;
            string lang  = LANGUAGES[Mathf.Clamp(langIdx, 0, LANGUAGES.Length - 1)].code;

            // Stage versioned prefs (Sprint 6 Lane 2 store).
            SettingsPersistence.StoreFloat(SettingsPersistence.K_MasterVolume, master);
            SettingsPersistence.StoreFloat(SettingsPersistence.K_MusicVolume,  music);
            SettingsPersistence.StoreFloat(SettingsPersistence.K_SFXVolume,    sfx);
            SettingsPersistence.StoreBool (SettingsPersistence.K_Fullscreen,   fs);
            SettingsPersistence.StoreBool (SettingsPersistence.K_InvertY,      inv);
            SettingsPersistence.StoreBool (SettingsPersistence.K_Rumble,       rumble);
            SettingsPersistence.StoreInt  (SettingsPersistence.K_ResolutionIdx, resIdx);
            SettingsPersistence.StoreString(SettingsPersistence.K_LanguageCode, lang);

            // Mirror invert-Y into legacy key CameraController reads on Awake.
            PlayerPrefs.SetInt("TARTARIA_InvertY", inv ? 1 : 0);

            ApplyAudioMixer(master, music, sfx);
            ApplyResolution(resIdx, fs);
            ApplyRumble(rumble);
            ApplyCameraInvertY(inv);

            SettingsPersistence.Commit();
            Debug.Log(
                $"[SettingsPanelController] Apply complete: master={master:F2} music={music:F2} sfx={sfx:F2} " +
                $"fs={fs} invertY={inv} rumble={rumble} res={resIdx} lang={lang}");

            Close();
        }

        /// <summary>
        /// Discard live UI edits and reload from PlayerPrefs, then close.
        /// </summary>
        public void Cancel()
        {
            LoadFromPrefsIntoUI();
            UpdateVolumeLabels();
            UpdateLanguageWarning();
            Debug.Log("[SettingsPanelController] Cancel - UI state reverted from PlayerPrefs.");
            Close();
        }

        // === Runtime push helpers =====================================
        void ApplyAudioMixer(float master, float music, float sfx)
        {
            if (_mixer == null) return;
            _mixer.SetFloat(MIXER_PARAM_MASTER, LinearToDb(master));
            _mixer.SetFloat(MIXER_PARAM_MUSIC,  LinearToDb(music));
            _mixer.SetFloat(MIXER_PARAM_SFX,    LinearToDb(sfx));
        }

        static float LinearToDb(float v) => Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;

        void ApplyResolution(int idx, bool fullscreen)
        {
            if (_resolutions.Length == 0)
            {
                Debug.LogWarning("[SettingsPanelController] Resolution list is empty; skipping Screen.SetResolution.");
                return;
            }
            int clamped = Mathf.Clamp(idx, 0, _resolutions.Length - 1);
            var r = _resolutions[clamped];
            var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(r.width, r.height, mode, r.refreshRateRatio);
            Screen.fullScreenMode = mode;
        }

        void ApplyRumble(bool rumbleEnabled)
        {
            if (rumbleEnabled) return;
            var gp = Gamepad.current;
            if (gp != null) gp.SetMotorSpeeds(0f, 0f);
        }

        void ApplyCameraInvertY(bool invertY)
        {
            var cam = Object.FindFirstObjectByType<Tartaria.Camera.CameraController>(FindObjectsInactive.Include);
            if (cam != null) Tartaria.Camera.CameraController.SetInvertCameraY(invertY);
        }

        // === Helpers ==================================================
        int GuessCurrentResolutionIdx()
        {
            var cur = Screen.currentResolution;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                if (_resolutions[i].width == cur.width && _resolutions[i].height == cur.height)
                    return i;
            }
            return _resolutions.Length > 0 ? _resolutions.Length - 1 : 0;
        }

        int LookupLanguageIdx(string code)
        {
            for (int i = 0; i < LANGUAGES.Length; i++)
                if (LANGUAGES[i].code == code) return i;
            Debug.LogWarning($"[SettingsPanelController] Unknown language code '{code}', defaulting to English.");
            return 0;
        }

        void UpdateVolumeLabels()
        {
            if (_masterVolumeValueLabel != null && _masterVolumeSlider != null)
                _masterVolumeValueLabel.text = Mathf.RoundToInt(_masterVolumeSlider.value * 100f) + "%";
            if (_musicVolumeValueLabel != null && _musicVolumeSlider != null)
                _musicVolumeValueLabel.text  = Mathf.RoundToInt(_musicVolumeSlider .value * 100f) + "%";
            if (_sfxVolumeValueLabel != null && _sfxVolumeSlider != null)
                _sfxVolumeValueLabel.text    = Mathf.RoundToInt(_sfxVolumeSlider   .value * 100f) + "%";
        }

        void UpdateLanguageWarning()
        {
            if (_languageWarningLabel == null || _languageDropdown == null) return;
            int idx = Mathf.Clamp(_languageDropdown.value, 0, LANGUAGES.Length - 1);
            bool available = LANGUAGES[idx].available;
            _languageWarningLabel.gameObject.SetActive(!available);
            if (!available)
            {
                _languageWarningLabel.text =
                    "This locale is not localized yet - text will remain English on Apply.";
            }
        }

        // === Persistence-availability probe ===========================
        // The Lane 5 brief explicitly requires a loud flag if the Lane 2
        // SettingsPersistence file is missing post-merge. We probe by
        // calling EnsureSchema inside a try/catch — NOT a silent catch,
        // we re-throw the diagnostic into the log.
        static bool _persistenceProbeDone;
        static bool _persistenceProbeOk;
        static bool IsPersistenceAvailable()
        {
            if (_persistenceProbeDone) return _persistenceProbeOk;
            _persistenceProbeDone = true;
            try
            {
                SettingsPersistence.EnsureSchema();
                _persistenceProbeOk = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    "[SettingsPanelController] SettingsPersistence probe threw: " + ex.GetType().Name +
                    " - " + ex.Message + ". Treating as MISSING.");
                _persistenceProbeOk = false;
            }
            return _persistenceProbeOk;
        }

        // === Canvas resolution ========================================
        static Canvas FindOrCreateRootCanvas()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas best = null;
            int bestOrder = int.MinValue;
            for (int i = 0; i < canvases.Length; i++)
            {
                var c = canvases[i];
                if (c == null) continue;
                if (c.renderMode == RenderMode.WorldSpace) continue;
                if (c.isRootCanvas && c.sortingOrder > bestOrder)
                {
                    best = c;
                    bestOrder = c.sortingOrder;
                }
            }
            if (best != null) return best;

            Debug.LogWarning(
                "[SettingsPanelController] No screen-space Canvas found in scene; creating a fresh root Canvas for the settings panel.");
            var go = new GameObject("SettingsPanel_RootCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(go);
            return canvas;
        }
    }
}
