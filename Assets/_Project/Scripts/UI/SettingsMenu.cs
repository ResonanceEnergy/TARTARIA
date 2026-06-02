// SettingsMenu.cs
// Sprint 6 Lane 2 — agent/ui/settings-menu-real
// Owner: UI agent. Path: Assets/_Project/Scripts/UI/SettingsMenu.cs
//
// Real settings panel as specified in the lane brief:
//   - Resolution dropdown (Screen.resolutions, refreshRate >= 50Hz)
//   - Fullscreen toggle (Screen.fullScreenMode)
//   - Master / Music / SFX volume sliders → AudioMixer.SetFloat on the
//     exposed parameters declared in MasterMixer.mixer (MasterVol / MusicVol /
//     SFXVol). Conversion: dB = log10(max(v, 0.0001)) * 20.
//   - Invert-Y toggle (writes TARTARIA_SET_InvertY + mirrors to the legacy
//     TARTARIA_InvertY key read by CameraController.Awake).
//   - Controller rumble on/off toggle.
//   - Language dropdown (English locked, 12 Moons-13 future locales suffixed
//     "(Coming)").
//   - Apply: stages all PlayerPrefs writes → AudioMixer.SetFloat →
//     Screen.SetResolution → Commit → close.
//   - Cancel: re-reads PlayerPrefs into the live UI, then closes.
//
// API_CONTRACT compliance:
//   - No banned namespace name (Tartaria.UI not in banned list).
//   - Unity 6: FindFirstObjectByType(FindObjectsInactive.Include); no
//     FindObjectOfType anywhere.
//   - AudioMixer dB conversion uses Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20
//     per lane brief.
//   - No silent catches.
//   - Every persistence load logs through SettingsPersistence.LoadXxx,
//     which prints `[SettingsMenu] Loaded {key}={value}` per no-debt rule 4.
//
// Mixer exposed parameter evidence:
//   Assets/_Project/Audio/Mixers/MasterMixer.mixer:110  name: MasterVol
//   Assets/_Project/Audio/Mixers/MasterMixer.mixer:112  name: MusicVol
//   Assets/_Project/Audio/Mixers/MasterMixer.mixer:114  name: SFXVol
//
// CameraController invert-Y bridge:
//   Assets/_Project/Scripts/Camera/CameraController.cs:75   SetInvertCameraY → PlayerPrefs "TARTARIA_InvertY"
//   Assets/_Project/Scripts/Camera/CameraController.cs:120  Awake reads PlayerPrefs "TARTARIA_InvertY"

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Tartaria.Audio;

namespace Tartaria.UI
{
    /// <summary>
    /// Real settings panel. Drives Screen / AudioMixer / PlayerPrefs.
    /// Headless-callable via public Apply / Cancel — IMGUI rendered for now
    /// (matches SettingsOverlay convention until UI Toolkit panel is wired).
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsMenu : MonoBehaviour
    {
        // === Singleton bootstrap ======================================
        static SettingsMenu _instance;
        public static SettingsMenu Instance => _instance;
        public static bool IsOpen => _instance != null && _instance._visible;

        bool _visible;
        bool _wasCursorLocked;

        // === Mixer ====================================================
        // Exposed-parameter names match Assets/_Project/Audio/Mixers/MasterMixer.mixer
        // lines 110/112/114 (MasterVol/MusicVol/SFXVol). Verified by grep — see
        // file header comment block above.
        const string MIXER_PARAM_MASTER = "MasterVol";
        const string MIXER_PARAM_MUSIC  = "MusicVol";
        const string MIXER_PARAM_SFX    = "SFXVol";

        AudioMixer _mixer;

        // === Live UI state (mirrors PlayerPrefs until Apply/Cancel) ====
        float _masterVolume;
        float _musicVolume;
        float _sfxVolume;
        bool  _fullscreen;
        bool  _invertY;
        bool  _rumble;

        Resolution[] _resolutions = System.Array.Empty<Resolution>();
        string[]     _resolutionLabels = System.Array.Empty<string>();
        int          _resolutionIdx;

        // === Language list (English active; rest are Moons-13 future
        // locales — shown "(Coming)" so the UI is honest about scope).
        struct LanguageEntry
        {
            public string code;
            public string label;
            public bool   available;
            public LanguageEntry(string c, string l, bool a) { code = c; label = l; available = a; }
        }
        static readonly LanguageEntry[] LANGUAGES =
        {
            new LanguageEntry("en",    "English",                    true),
            new LanguageEntry("fr",    "Français (Coming)",          false),
            new LanguageEntry("es",    "Español (Coming)",           false),
            new LanguageEntry("de",    "Deutsch (Coming)",           false),
            new LanguageEntry("ja",    "日本語 (Coming)",            false),
            new LanguageEntry("pt-BR", "Português (Brasil) (Coming)", false),
            new LanguageEntry("ru",    "Русский (Coming)",           false),
            new LanguageEntry("it",    "Italiano (Coming)",          false),
            new LanguageEntry("zh-CN", "简体中文 (Coming)",          false),
            new LanguageEntry("ko",    "한국어 (Coming)",            false),
            new LanguageEntry("ar",    "العربية (Coming)",          false),
            new LanguageEntry("hi",    "हिन्दी (Coming)",          false),
            new LanguageEntry("tr",    "Türkçe (Coming)",            false),
        };
        int _languageIdx;

        Vector2 _scroll;

        // === Bootstrap ================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var existing = Object.FindFirstObjectByType<SettingsMenu>(FindObjectsInactive.Include);
            if (existing != null) { _instance = existing; return; }
            var go = new GameObject("SettingsMenu");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SettingsMenu>();
        }

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            SettingsPersistence.EnsureSchema();
            BuildResolutionList();
            _mixer = MasterMixerLocator.Load();
            if (_mixer == null)
            {
                Debug.LogWarning(
                    "[SettingsMenu] MasterMixerLocator returned null — slider " +
                    "writes will not reach AudioMixer until a MasterMixerLocator " +
                    "ScriptableObject is placed at Resources/MasterMixerLocator.asset.");
            }
            LoadFromPrefs();
            ApplyAudioMixer();
        }

        // === Public API ===============================================
        public static void Open()
        {
            if (_instance == null) Bootstrap();
            if (_instance == null) return;
            _instance.LoadFromPrefs(); // honest re-read on open
            _instance._visible = true;
            _instance.UnlockCursorForUI();
        }

        public static void Close()
        {
            if (_instance == null || !_instance._visible) return;
            _instance._visible = false;
            _instance.RestoreCursor();
        }

        /// <summary>
        /// Apply: stage PlayerPrefs writes, push to AudioMixer, push to
        /// Screen, commit prefs, close.
        /// </summary>
        public void Apply()
        {
            // Stage prefs (versioned TARTARIA_SET_* keys)
            SettingsPersistence.StoreFloat(SettingsPersistence.K_MasterVolume, _masterVolume);
            SettingsPersistence.StoreFloat(SettingsPersistence.K_MusicVolume,  _musicVolume);
            SettingsPersistence.StoreFloat(SettingsPersistence.K_SFXVolume,    _sfxVolume);
            SettingsPersistence.StoreBool (SettingsPersistence.K_Fullscreen,   _fullscreen);
            SettingsPersistence.StoreBool (SettingsPersistence.K_InvertY,      _invertY);
            SettingsPersistence.StoreBool (SettingsPersistence.K_Rumble,       _rumble);
            SettingsPersistence.StoreInt  (SettingsPersistence.K_ResolutionIdx,_resolutionIdx);
            SettingsPersistence.StoreString(SettingsPersistence.K_LanguageCode,LANGUAGES[_languageIdx].code);

            // Mirror invert-Y into the legacy key CameraController reads on Awake.
            // CameraController.cs:75 / :120 use "TARTARIA_InvertY" — keep
            // a single source of runtime truth so live cameras see the change
            // without rebooting.
            PlayerPrefs.SetInt("TARTARIA_InvertY", _invertY ? 1 : 0);

            // Push to runtime systems
            ApplyAudioMixer();
            ApplyResolution();
            ApplyRumble();
            ApplyCameraInvertY();

            SettingsPersistence.Commit();
            Debug.Log(
                $"[SettingsMenu] Apply complete: master={_masterVolume:F2} " +
                $"music={_musicVolume:F2} sfx={_sfxVolume:F2} fs={_fullscreen} " +
                $"invertY={_invertY} rumble={_rumble} res={_resolutionIdx} " +
                $"lang={LANGUAGES[_languageIdx].code}");
            Close();
        }

        /// <summary>
        /// Cancel: discard live UI state, re-read PlayerPrefs into UI, close.
        /// </summary>
        public void Cancel()
        {
            LoadFromPrefs();
            Debug.Log("[SettingsMenu] Cancel — UI state reverted from PlayerPrefs.");
            Close();
        }

        // === Load / apply helpers ====================================
        void LoadFromPrefs()
        {
            _masterVolume = SettingsPersistence.LoadFloat(SettingsPersistence.K_MasterVolume, SettingsPersistence.DefaultMasterVolume);
            _musicVolume  = SettingsPersistence.LoadFloat(SettingsPersistence.K_MusicVolume,  SettingsPersistence.DefaultMusicVolume);
            _sfxVolume    = SettingsPersistence.LoadFloat(SettingsPersistence.K_SFXVolume,    SettingsPersistence.DefaultSFXVolume);
            _fullscreen   = SettingsPersistence.LoadBool (SettingsPersistence.K_Fullscreen,   SettingsPersistence.DefaultFullscreen);
            _invertY      = SettingsPersistence.LoadBool (SettingsPersistence.K_InvertY,      SettingsPersistence.DefaultInvertY);
            _rumble       = SettingsPersistence.LoadBool (SettingsPersistence.K_Rumble,       SettingsPersistence.DefaultRumble);
            int storedRes = SettingsPersistence.LoadInt  (SettingsPersistence.K_ResolutionIdx,GuessCurrentResolutionIdx());
            _resolutionIdx = Mathf.Clamp(storedRes, 0, Mathf.Max(0, _resolutions.Length - 1));
            string lang   = SettingsPersistence.LoadString(SettingsPersistence.K_LanguageCode, SettingsPersistence.DefaultLanguageCode);
            _languageIdx = LookupLanguageIdx(lang);
        }

        void BuildResolutionList()
        {
            var raw = Screen.resolutions;
            var list = new List<Resolution>(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                // Lane brief: filter to refreshRate >= 50.
                double hz = raw[i].refreshRateRatio.value;
                if (hz >= 50.0)
                    list.Add(raw[i]);
            }
            if (list.Count == 0 && raw.Length > 0)
            {
                // Honest fallback (not silent): log and take everything.
                Debug.LogWarning(
                    "[SettingsMenu] No resolutions >= 50Hz detected; falling " +
                    "back to the full Screen.resolutions list.");
                list.AddRange(raw);
            }
            _resolutions = list.ToArray();
            _resolutionLabels = new string[_resolutions.Length];
            for (int i = 0; i < _resolutions.Length; i++)
            {
                var r = _resolutions[i];
                int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
                _resolutionLabels[i] = $"{r.width} x {r.height} @ {hz}Hz";
            }
        }

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
            Debug.LogWarning($"[SettingsMenu] Unknown language code '{code}', defaulting to English.");
            return 0;
        }

        void ApplyAudioMixer()
        {
            if (_mixer == null) return;
            _mixer.SetFloat(MIXER_PARAM_MASTER, LinearToDb(_masterVolume));
            _mixer.SetFloat(MIXER_PARAM_MUSIC,  LinearToDb(_musicVolume));
            _mixer.SetFloat(MIXER_PARAM_SFX,    LinearToDb(_sfxVolume));
        }

        // dB conversion per lane brief: log10(max(v, 0.0001)) * 20.
        static float LinearToDb(float v) => Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;

        void ApplyResolution()
        {
            if (_resolutions.Length == 0)
            {
                Debug.LogWarning("[SettingsMenu] Resolution list is empty; skipping Screen.SetResolution.");
                return;
            }
            int idx = Mathf.Clamp(_resolutionIdx, 0, _resolutions.Length - 1);
            var r = _resolutions[idx];
            var mode = _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(r.width, r.height, mode, r.refreshRateRatio);
            Screen.fullScreenMode = mode;
        }

        void ApplyRumble()
        {
            if (_rumble) return;
            // Stop any in-flight rumble. Future haptic systems should poll
            // GetRumbleEnabled() before issuing SetMotorSpeeds.
            var gp = Gamepad.current;
            if (gp != null) gp.SetMotorSpeeds(0f, 0f);
        }

        void ApplyCameraInvertY()
        {
            // Push to live CameraController instance (if present in scene)
            // via the existing static setter so the change takes effect this
            // frame, not on next scene load.
            var cam = Object.FindFirstObjectByType<Tartaria.Camera.CameraController>(FindObjectsInactive.Include);
            if (cam != null) Tartaria.Camera.CameraController.SetInvertCameraY(_invertY);
        }

        /// <summary>Public accessor for any haptic driver that needs to honor the rumble toggle.</summary>
        public static bool GetRumbleEnabled()
            => SettingsPersistence.LoadBool(SettingsPersistence.K_Rumble, SettingsPersistence.DefaultRumble);

        // === Cursor ===================================================
        void UnlockCursorForUI()
        {
            _wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void RestoreCursor()
        {
            if (!_wasCursorLocked) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // === IMGUI (matches SettingsOverlay convention until UI Toolkit
        // panel lands; OnGUI is the simplest way to ship an interactive
        // panel that responds to mouse + gamepad without a uxml file).
        void OnGUI()
        {
            if (!_visible) return;

            const int W = 560, H = 540;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            // Dim
            var prevColor = GUI.color;
            GUI.color = new Color(0.02f, 0.01f, 0.04f, 0.78f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prevColor;

            GUI.Box(new Rect(x, y, W, H), "");
            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.98f, 0.9f, 0.55f) }
            };
            GUI.Label(new Rect(x, y + 8, W, 28), "SETTINGS", title);

            int row = y + 50;
            int lx = x + 24, sx = x + 200, sw = W - 240;

            // --- Audio
            DrawHeader("AUDIO", lx, ref row, W - 48);
            DrawSlider($"Master Volume: {_masterVolume:P0}", ref _masterVolume, lx, sx, sw, ref row);
            DrawSlider($"Music: {_musicVolume:P0}",          ref _musicVolume,  lx, sx, sw, ref row);
            DrawSlider($"SFX: {_sfxVolume:P0}",              ref _sfxVolume,    lx, sx, sw, ref row);
            row += 6;

            // --- Display
            DrawHeader("DISPLAY", lx, ref row, W - 48);
            GUI.Label(new Rect(lx, row, 180, 22), "Resolution:");
            if (_resolutions.Length > 0)
            {
                if (GUI.Button(new Rect(sx, row, 26, 22), "<"))
                    _resolutionIdx = (_resolutionIdx - 1 + _resolutions.Length) % _resolutions.Length;
                GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 20), _resolutionLabels[_resolutionIdx]);
                if (GUI.Button(new Rect(sx + sw - 26, row, 26, 22), ">"))
                    _resolutionIdx = (_resolutionIdx + 1) % _resolutions.Length;
            }
            else
            {
                GUI.Label(new Rect(sx, row + 2, sw, 20), "(no resolutions available)");
            }
            row += 28;
            _fullscreen = GUI.Toggle(new Rect(lx, row, sw, 22), _fullscreen, "Fullscreen");
            row += 28;

            // --- Controls
            DrawHeader("CONTROLS", lx, ref row, W - 48);
            _invertY = GUI.Toggle(new Rect(lx, row, sw, 22), _invertY, "Invert Camera Y");
            row += 26;
            _rumble  = GUI.Toggle(new Rect(lx, row, sw, 22), _rumble,  "Controller Rumble");
            row += 28;

            // --- Language
            DrawHeader("LANGUAGE", lx, ref row, W - 48);
            GUI.Label(new Rect(lx, row, 180, 22), "Language:");
            if (GUI.Button(new Rect(sx, row, 26, 22), "<"))
                _languageIdx = (_languageIdx - 1 + LANGUAGES.Length) % LANGUAGES.Length;
            GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 20), LANGUAGES[_languageIdx].label);
            if (GUI.Button(new Rect(sx + sw - 26, row, 26, 22), ">"))
                _languageIdx = (_languageIdx + 1) % LANGUAGES.Length;
            row += 28;
            if (!LANGUAGES[_languageIdx].available)
            {
                var warn = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(1f, 0.78f, 0.4f) }
                };
                GUI.Label(new Rect(lx, row, sw + 60, 18),
                    "This locale is not localized yet — text will remain English on Apply.", warn);
                row += 20;
            }

            // --- Buttons ---
            int btnY = y + H - 44;
            if (GUI.Button(new Rect(x + W - 230, btnY, 100, 28), "Cancel")) Cancel();
            if (GUI.Button(new Rect(x + W - 120, btnY, 100, 28), "Apply"))  Apply();
        }

        static void DrawHeader(string label, int lx, ref int row, int wide)
        {
            var hdr = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.5f) }
            };
            GUI.Label(new Rect(lx, row, wide, 18), "▸ " + label, hdr);
            row += 22;
        }

        static void DrawSlider(string label, ref float value, int lx, int sx, int sw, ref int row)
        {
            GUI.Label(new Rect(lx, row, 180, 22), label);
            value = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), value, 0f, 1f);
            row += 26;
        }
    }
}
