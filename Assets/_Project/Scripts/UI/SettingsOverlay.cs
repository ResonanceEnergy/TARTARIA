using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Tartaria.UI
{
    /// <summary>
    /// Polish: Settings overlay (F10 toggle, also opened by Main Menu / Pause).
    /// Master volume, mouse sensitivity, colorblind mode, text scale,
    /// resolution, quality preset, mixer-bus volumes (Music/SFX/UI/Ambience).
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsOverlay : MonoBehaviour
    {
        const string PP_VOLUME = "TARTARIA_MasterVolume";
        const string PP_MUSIC  = "TARTARIA_MusicVolume";
        const string PP_SFX    = "TARTARIA_SFXVolume";
        const string PP_AMB    = "TARTARIA_AmbienceVolume";
        const string PP_SENS   = "TARTARIA_MouseSens";
        const string PP_TEXT   = "TARTARIA_TextScale";
        const string PP_CB     = "TARTARIA_ColorblindMode";
        const string PP_RES    = "TARTARIA_ResolutionIdx";
        const string PP_QUAL   = "TARTARIA_QualityIdx";
        const string PP_FS     = "TARTARIA_Fullscreen";

        const string MixerPath = "Assets/_Project/Audio/Mixers/MasterMixer.mixer";

        static SettingsOverlay _instance;
        public static bool IsOpen => _instance != null && _instance._visible;

        bool _visible;
        bool _wasCursorLocked;
        float _volume = 1f;
        float _music = 1f;
        float _sfx   = 1f;
        float _amb   = 1f;
        float _sens = 1f;
        float _textScale = 1f;
        int _colorblind; // 0=None,1=Protanopia,2=Deuteranopia,3=Tritanopia
        readonly string[] _cbLabels = { "None", "Protanopia", "Deuteranopia", "Tritanopia" };

        Resolution[] _resolutions = System.Array.Empty<Resolution>();
        string[] _resLabels = System.Array.Empty<string>();
        int _resIdx;
        int _qualityIdx;
        bool _fullscreen = true;

        AudioMixer _mixer;
        Vector2 _scroll;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("SettingsOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SettingsOverlay>();
        }

        void Awake()
        {
            _volume    = PlayerPrefs.GetFloat(PP_VOLUME, 1f);
            _music     = PlayerPrefs.GetFloat(PP_MUSIC, 0.85f);
            _sfx       = PlayerPrefs.GetFloat(PP_SFX, 1f);
            _amb       = PlayerPrefs.GetFloat(PP_AMB, 0.7f);
            _sens      = PlayerPrefs.GetFloat(PP_SENS, 1f);
            _textScale = PlayerPrefs.GetFloat(PP_TEXT, 1f);
            _colorblind = PlayerPrefs.GetInt(PP_CB, 0);
            _qualityIdx = PlayerPrefs.GetInt(PP_QUAL, QualitySettings.GetQualityLevel());
            _fullscreen = PlayerPrefs.GetInt(PP_FS, 1) == 1;

            _resolutions = Screen.resolutions;
            _resLabels = new string[_resolutions.Length];
            int currentIdx = 0;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                var r = _resolutions[i];
                _resLabels[i] = $"{r.width} x {r.height} @ {Mathf.RoundToInt((float)r.refreshRateRatio.value)}Hz";
                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                    currentIdx = i;
            }
            _resIdx = Mathf.Clamp(PlayerPrefs.GetInt(PP_RES, currentIdx), 0, Mathf.Max(0, _resolutions.Length - 1));

            // Locate mixer for runtime SetFloat on exposed parameters.
            _mixer = Tartaria.Audio.MasterMixerLocator.Load();

            ApplyAll();
        }

        public static void Open()
        {
            if (_instance == null) Bootstrap();
            _instance!._visible = true;
            _instance.UnlockCursorForUI();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f10Key.wasPressedThisFrame)
            {
                _visible = !_visible;
                if (_visible) UnlockCursorForUI(); else RestoreCursor();
            }
            if (_visible && kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                _visible = false;
                RestoreCursor();
            }
        }

        void UnlockCursorForUI()
        {
            _wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void RestoreCursor()
        {
            if (_wasCursorLocked && !Tartaria.Core.GameBootstrap.MainMenuActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void OnGUI()
        {
            if (!_visible) return;

            const int W = 560, H = 600;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            // Dim
            var c = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = c;

            GUI.Box(new Rect(x, y, W, H), "");
            var title = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            GUI.Label(new Rect(x, y + 8, W, 32), "SETTINGS", title);

            int row = y + 56;
            int lx = x + 24, sx = x + 200, sw = W - 240;

            // Master
            GUI.Label(new Rect(lx, row, 180, 24), $"Master Volume: {_volume:P0}");
            float v = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _volume, 0f, 1f);
            if (!Mathf.Approximately(v, _volume)) { _volume = v; AudioListener.volume = _volume; SetMixer("MasterVol", v); PlayerPrefs.SetFloat(PP_VOLUME, v); }
            row += 32;

            // Music
            GUI.Label(new Rect(lx, row, 180, 24), $"Music: {_music:P0}");
            float mu = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _music, 0f, 1f);
            if (!Mathf.Approximately(mu, _music)) { _music = mu; SetMixer("MusicVol", mu); PlayerPrefs.SetFloat(PP_MUSIC, mu); }
            row += 32;

            // SFX
            GUI.Label(new Rect(lx, row, 180, 24), $"SFX: {_sfx:P0}");
            float sf = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _sfx, 0f, 1f);
            if (!Mathf.Approximately(sf, _sfx)) { _sfx = sf; SetMixer("SFXVol", sf); PlayerPrefs.SetFloat(PP_SFX, sf); }
            row += 32;

            // Ambience
            GUI.Label(new Rect(lx, row, 180, 24), $"Ambience: {_amb:P0}");
            float am = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _amb, 0f, 1f);
            if (!Mathf.Approximately(am, _amb)) { _amb = am; SetMixer("AmbienceVol", am); PlayerPrefs.SetFloat(PP_AMB, am); }
            row += 40;

            // Mouse sens
            GUI.Label(new Rect(lx, row, 180, 24), $"Mouse Sensitivity: {_sens:F2}");
            float s = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _sens, 0.25f, 3f);
            if (!Mathf.Approximately(s, _sens)) { _sens = s; PlayerPrefs.SetFloat(PP_SENS, _sens); }
            row += 32;

            // Text scale
            GUI.Label(new Rect(lx, row, 180, 24), $"Text Scale: {_textScale:F2}x");
            float t = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _textScale, 0.7f, 2f);
            if (!Mathf.Approximately(t, _textScale)) { _textScale = t; PlayerPrefs.SetFloat(PP_TEXT, _textScale); ApplyTextScale(); }
            row += 32;

            // Colorblind
            GUI.Label(new Rect(lx, row, 180, 24), "Colorblind Mode:");
            int newCB = _colorblind;
            for (int i = 0; i < _cbLabels.Length; i++)
            {
                if (GUI.Toggle(new Rect(sx + i * 80, row, 78, 22), _colorblind == i, _cbLabels[i]))
                    newCB = i;
            }
            if (newCB != _colorblind) { _colorblind = newCB; PlayerPrefs.SetInt(PP_CB, _colorblind); ApplyColorblind(); }
            row += 36;

            // Resolution
            GUI.Label(new Rect(lx, row, 180, 24), "Resolution:");
            if (_resolutions.Length > 0)
            {
                if (GUI.Button(new Rect(sx, row, 26, 24), "<")) { _resIdx = (_resIdx - 1 + _resolutions.Length) % _resolutions.Length; }
                GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 22), _resLabels[_resIdx]);
                if (GUI.Button(new Rect(sx + sw - 26, row, 26, 24), ">")) { _resIdx = (_resIdx + 1) % _resolutions.Length; }
            }
            row += 28;

            // Quality
            GUI.Label(new Rect(lx, row, 180, 24), "Quality:");
            string[] qNames = QualitySettings.names;
            if (qNames.Length > 0)
            {
                if (GUI.Button(new Rect(sx, row, 26, 24), "<")) { _qualityIdx = (_qualityIdx - 1 + qNames.Length) % qNames.Length; }
                GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 22), qNames[Mathf.Clamp(_qualityIdx, 0, qNames.Length - 1)]);
                if (GUI.Button(new Rect(sx + sw - 26, row, 26, 24), ">")) { _qualityIdx = (_qualityIdx + 1) % qNames.Length; }
            }
            row += 28;

            // Fullscreen
            bool newFS = GUI.Toggle(new Rect(lx, row, 200, 22), _fullscreen, "Fullscreen");
            if (newFS != _fullscreen) { _fullscreen = newFS; }
            row += 28;

            if (GUI.Button(new Rect(lx, row, 240, 26), "Apply Display Settings"))
            {
                ApplyDisplaySettings();
            }
            row += 36;

            // Skip-menu toggle for dev
            bool skip = PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1;
            bool newSkip = GUI.Toggle(new Rect(lx, row, W - 48, 22), skip, "Skip main menu next launch (dev)");
            if (newSkip != skip) PlayerPrefs.SetInt("TARTARIA_SkipMainMenu", newSkip ? 1 : 0);
            row += 36;

            if (GUI.Button(new Rect(x + W - 120, y + H - 40, 96, 28), "Close"))
            {
                _visible = false;
                PlayerPrefs.Save();
                RestoreCursor();
            }
        }

        void ApplyDisplaySettings()
        {
            if (_resolutions.Length > 0)
            {
                var r = _resolutions[Mathf.Clamp(_resIdx, 0, _resolutions.Length - 1)];
                Screen.SetResolution(r.width, r.height, _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, r.refreshRateRatio);
                PlayerPrefs.SetInt(PP_RES, _resIdx);
            }
            QualitySettings.SetQualityLevel(_qualityIdx, true);
            PlayerPrefs.SetInt(PP_QUAL, _qualityIdx);
            PlayerPrefs.SetInt(PP_FS, _fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        void ApplyAll()
        {
            AudioListener.volume = _volume;
            SetMixer("MasterVol", _volume);
            SetMixer("MusicVol", _music);
            SetMixer("SFXVol", _sfx);
            SetMixer("AmbienceVol", _amb);
            ApplyTextScale();
            ApplyColorblind();
            QualitySettings.SetQualityLevel(_qualityIdx, true);
        }

        void SetMixer(string param, float linear01)
        {
            if (_mixer == null) return;
            // Convert linear 0..1 to dB, with -80 dB floor at 0 and 0 dB at 1.
            float dB = linear01 <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
            _mixer.SetFloat(param, dB);
        }

        void ApplyTextScale()
        {
            // Bridge to AccessibilityManager via reflection (asmdef-cycle safe).
            TryInvoke("Tartaria.UI.AccessibilityManager", "SetTextScale", _textScale);
        }

        void ApplyColorblind()
        {
            // ColorblindMode enum: 0=None,1=Protanopia,2=Deuteranopia,3=Tritanopia
            TryInvokeEnum("Tartaria.UI.AccessibilityManager", "SetColorblindMode", "Tartaria.UI.ColorblindMode", _colorblind);
        }

        static void TryInvoke(string typeName, string method, object arg)
        {
            try
            {
                var t = System.Type.GetType(typeName) ?? System.Type.GetType($"{typeName}, Tartaria.UI");
                if (t == null) return;
                var inst = t.GetProperty("Instance")?.GetValue(null);
                if (inst == null) return;
                t.GetMethod(method)?.Invoke(inst, new[] { arg });
            }
            catch { }
        }

        static void TryInvokeEnum(string typeName, string method, string enumTypeName, int value)
        {
            try
            {
                var t = System.Type.GetType(typeName) ?? System.Type.GetType($"{typeName}, Tartaria.UI");
                var et = System.Type.GetType(enumTypeName) ?? System.Type.GetType($"{enumTypeName}, Tartaria.UI");
                if (t == null || et == null) return;
                var inst = t.GetProperty("Instance")?.GetValue(null);
                if (inst == null) return;
                object enumVal = System.Enum.ToObject(et, value);
                t.GetMethod(method)?.Invoke(inst, new[] { enumVal });
            }
            catch { }
        }
    }
}
