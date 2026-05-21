using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Tartaria.Core;

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
        public static bool IsReducedMotion => PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 1;

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

        // M2 beta polish: dirty tracking, confirm, toast, golden Tartarian theme
        float _initialVolume, _initialMusic, _initialSfx, _initialAmb, _initialSens, _initialText;
        int _initialCB, _initialRes, _initialQual;
        bool _initialFS;
        bool _dirty;
        bool _showConfirmClose;
        string _toastMessage;
        float _toastTimer;
        const float TOAST_DURATION = 1.6f;

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

            ApplyMouseSensitivity(); // wire on bootstrap
            ApplyAll();
        }

        public static void Open()
        {
            if (_instance == null) Bootstrap();
            var inst = _instance!;
            inst._visible = true;
            inst.UnlockCursorForUI();
            inst.CaptureInitials();
            inst._dirty = false;
            inst._showConfirmClose = false;
        }

        public static void CloseIfOpen()
        {
            if (_instance != null && _instance._visible)
            {
                _instance._visible = false;
                _instance.RestoreCursor();
            }
        }

        void CaptureInitials()
        {
            _initialVolume = _volume; _initialMusic = _music; _initialSfx = _sfx; _initialAmb = _amb;
            _initialSens = _sens; _initialText = _textScale;
            _initialCB = _colorblind; _initialRes = _resIdx; _initialQual = _qualityIdx;
            _initialFS = _fullscreen;
        }

        void Update()
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;
            if (kb != null && kb.f10Key.wasPressedThisFrame)
            {
                _visible = !_visible;
                if (_visible) { UnlockCursorForUI(); CaptureInitials(); _dirty = false; _showConfirmClose = false; _toastTimer = 0; }
                else RestoreCursor();
            }
            bool backPressed = (kb != null && kb.escapeKey.wasPressedThisFrame) || (gp != null && gp.buttonEast.wasPressedThisFrame);
            if (_visible && backPressed)
            {
                if (_dirty && !_showConfirmClose)
                {
                    _showConfirmClose = true; // trigger inline confirm
                }
                else
                {
                    _visible = false;
                    _showConfirmClose = false;
                    PlayerPrefs.Save();
                    RestoreCursor();
                }
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

            const int W = 560, H = 620; // extra for polished sections + toast/confirm
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            // Dim
            var c = GUI.color;
            GUI.color = new Color(0.02f, 0.01f, 0.04f, 0.78f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = c;

            // Golden Tartarian double-frame
            GUI.color = new Color(0.95f, 0.85f, 0.5f, 0.92f);
            GUI.Box(new Rect(x - 3, y - 3, W + 6, H + 6), "");
            GUI.color = c;
            GUI.Box(new Rect(x, y, W, H), "");
            GUI.color = new Color(0.6f, 0.5f, 0.3f, 0.6f);
            GUI.Box(new Rect(x + 3, y + 3, W - 6, H - 6), "");
            GUI.color = c;

            var title = new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 22, 
                alignment = TextAnchor.MiddleCenter, 
                fontStyle = FontStyle.Bold, 
                normal = { textColor = new Color(0.98f, 0.9f, 0.55f) } 
            };
            GUI.Label(new Rect(x, y + 8, W, 28), "SETTINGS — TARTARIA", title);

            var sub = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.75f, 0.72f, 0.6f) } };
            GUI.Label(new Rect(x, y + 34, W, 16), "Golden Age • Accessible • Immersive", sub);

            var sub2 = new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 11, 
                alignment = TextAnchor.MiddleCenter, 
                normal = { textColor = new Color(0.65f, 0.62f, 0.55f) } 
            };
            GUI.Label(new Rect(x, y + 36, W, 18), "Tune the Resonance", sub2);

            int row = y + 58;
            int lx = x + 24, sx = x + 200, sw = W - 240;

            DrawSectionHeader("AUDIO", ref row, lx, W - 48);

            // Master
            GUI.Label(new Rect(lx, row, 180, 22), $"Master Volume: {_volume:P0}");
            float v = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _volume, 0f, 1f);
            if (!Mathf.Approximately(v, _volume)) { _volume = v; AudioListener.volume = _volume; SetMixer("MasterVol", v); PlayerPrefs.SetFloat(PP_VOLUME, v); SetToast("Audio Applied"); }
            row += 28;

            // Music
            GUI.Label(new Rect(lx, row, 180, 22), $"Music: {_music:P0}");
            float mu = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _music, 0f, 1f);
            if (!Mathf.Approximately(mu, _music)) { _music = mu; SetMixer("MusicVol", mu); PlayerPrefs.SetFloat(PP_MUSIC, mu); SetToast("Audio Applied"); }
            row += 28;

            // SFX
            GUI.Label(new Rect(lx, row, 180, 22), $"SFX: {_sfx:P0}");
            float sf = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _sfx, 0f, 1f);
            if (!Mathf.Approximately(sf, _sfx)) { _sfx = sf; SetMixer("SFXVol", sf); PlayerPrefs.SetFloat(PP_SFX, sf); SetToast("Audio Applied"); }
            row += 28;

            // Ambience
            GUI.Label(new Rect(lx, row, 180, 22), $"Ambience: {_amb:P0}");
            float am = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _amb, 0f, 1f);
            if (!Mathf.Approximately(am, _amb)) { _amb = am; SetMixer("AmbienceVol", am); PlayerPrefs.SetFloat(PP_AMB, am); SetToast("Audio Applied"); }
            row += 32;

            DrawSectionHeader("CONTROLS & ACCESSIBILITY", ref row, lx, W - 48);

            // Mouse sens — wired
            GUI.Label(new Rect(lx, row, 180, 22), $"Mouse Sensitivity: {_sens:F2}");
            float s = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _sens, 0.25f, 3f);
            if (!Mathf.Approximately(s, _sens)) { _sens = s; PlayerPrefs.SetFloat(PP_SENS, _sens); Tartaria.Camera.CameraController.SetMouseSensitivity(_sens); SetToast("Mouse Sensitivity Applied"); }
            row += 28;

            // Text scale — routes to HUD/dialogue via AM event
            GUI.Label(new Rect(lx, row, 180, 22), $"Text Scale: {_textScale:F2}x");
            float t = GUI.HorizontalSlider(new Rect(sx, row + 5, sw, 16), _textScale, 0.7f, 2f);
            if (!Mathf.Approximately(t, _textScale)) { _textScale = t; PlayerPrefs.SetFloat(PP_TEXT, _textScale); ApplyTextScale(); SetToast("Text Scale Applied"); }
            row += 28;

            // Colorblind (stays in same section)
            GUI.Label(new Rect(lx, row, 180, 22), "Colorblind Mode:");
            int newCB = _colorblind;
            for (int i = 0; i < _cbLabels.Length; i++)
            {
                if (GUI.Toggle(new Rect(sx + i * 80, row, 78, 20), _colorblind == i, _cbLabels[i]))
                    newCB = i;
            }
            if (newCB != _colorblind) { _colorblind = newCB; PlayerPrefs.SetInt(PP_CB, _colorblind); ApplyColorblind(); SetToast("Accessibility Applied"); }
            row += 30;

            // Reduced motion (M2 from UX doc accessibility section)
            bool reduced = PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 1;
            if (GUI.Toggle(new Rect(lx, row, 280, 20), reduced, "Reduced Motion (less shake/particles)"))
            {
                reduced = !reduced;
                PlayerPrefs.SetInt("TARTARIA_ReducedMotion", reduced ? 1 : 0);
                SetToast("Accessibility Applied");
            }
            row += 26;

            DrawSectionHeader("DISPLAY & PERFORMANCE", ref row, lx, W - 48);

            // Resolution
            GUI.Label(new Rect(lx, row, 180, 22), "Resolution:");
            if (_resolutions.Length > 0)
            {
                if (GUI.Button(new Rect(sx, row, 26, 22), "<")) { _resIdx = (_resIdx - 1 + _resolutions.Length) % _resolutions.Length; _dirty = true; }
                GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 20), _resLabels[_resIdx]);
                if (GUI.Button(new Rect(sx + sw - 26, row, 26, 22), ">")) { _resIdx = (_resIdx + 1) % _resolutions.Length; _dirty = true; }
            }
            row += 26;

            // Quality
            GUI.Label(new Rect(lx, row, 180, 22), "Quality:");
            string[] qNames = QualitySettings.names;
            if (qNames.Length > 0)
            {
                if (GUI.Button(new Rect(sx, row, 26, 22), "<")) { _qualityIdx = (_qualityIdx - 1 + qNames.Length) % qNames.Length; _dirty = true; }
                GUI.Label(new Rect(sx + 30, row + 2, sw - 90, 20), qNames[Mathf.Clamp(_qualityIdx, 0, qNames.Length - 1)]);
                if (GUI.Button(new Rect(sx + sw - 26, row, 26, 22), ">")) { _qualityIdx = (_qualityIdx + 1) % qNames.Length; _dirty = true; }
            }
            row += 26;

            // Round 4: Hardware Tier + Fallback UI feedback (persisted + auto)
            GUI.Label(new Rect(lx, row, 260, 20), $"Hardware Tier (auto): {GetCurrentPerfTier()}");
            row += 20;
            int fb = PlayerPrefs.GetInt("TARTARIA_FallbackCount", 0);
            GUI.Label(new Rect(lx, row, 260, 18), $"Auto-Fallbacks: {fb} (persisted)");
            row += 20;
            if (GUI.Button(new Rect(lx, row, 170, 18), "Force Tier Downgrade"))
            {
                GameBootstrap.TriggerAutoQualityFallback("Manual UI downgrade (dev)");
                SetToast("Tier Downgraded");
            }
            row += 22;

            // Round 5: Dynamic runtime tier switches (production hardening, no restart)
            GUI.Label(new Rect(lx, row, 260, 18), "Runtime Tier Switch:");
            row += 18;
            if (GUI.Button(new Rect(lx, row, 55, 17), "Low")) { GameBootstrap.ApplyRuntimePerformanceTier(PerformanceProfile.HardwareTier.Low); SetToast("Tier: Low"); }
            if (GUI.Button(new Rect(lx + 58, row, 55, 17), "Med")) { GameBootstrap.ApplyRuntimePerformanceTier(PerformanceProfile.HardwareTier.Medium); SetToast("Tier: Med"); }
            if (GUI.Button(new Rect(lx + 116, row, 55, 17), "High")) { GameBootstrap.ApplyRuntimePerformanceTier(PerformanceProfile.HardwareTier.High); SetToast("Tier: High"); }
            if (GUI.Button(new Rect(lx + 174, row, 55, 17), "Ultra")) { GameBootstrap.ApplyRuntimePerformanceTier(PerformanceProfile.HardwareTier.Ultra); SetToast("Tier: Ultra"); }
            row += 22;

            // Fullscreen
            bool newFS = GUI.Toggle(new Rect(lx, row, 200, 20), _fullscreen, "Fullscreen");
            if (newFS != _fullscreen) { _fullscreen = newFS; _dirty = true; }
            row += 26;

            if (GUI.Button(new Rect(lx, row, 220, 24), "Apply Display Settings"))
            {
                ApplyDisplaySettings();
                SetToast("Display Settings Applied");
                _dirty = false; // applied
            }
            row += 30;

            // Skip-menu toggle for dev
            bool skip = PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1;
            bool newSkip = GUI.Toggle(new Rect(lx, row, W - 48, 20), skip, "Skip main menu next launch (dev)");
            if (newSkip != skip) PlayerPrefs.SetInt("TARTARIA_SkipMainMenu", newSkip ? 1 : 0);
            row += 28;

            // Defaults (high-impact beta safety net)
            if (GUI.Button(new Rect(lx, row, 110, 22), "Defaults"))
            {
                ResetToDefaults();
            }

            // Close with dirty confirm
            Rect closeRect = new Rect(x + W - 115, y + H - 38, 100, 26);
            if (GUI.Button(closeRect, _showConfirmClose ? "CONFIRM CLOSE" : "Close"))
            {
                if (_showConfirmClose)
                {
                    _visible = false;
                    _showConfirmClose = false;
                    PlayerPrefs.Save();
                    RestoreCursor();
                }
                else if (_dirty)
                {
                    _showConfirmClose = true;
                }
                else
                {
                    _visible = false;
                    PlayerPrefs.Save();
                    RestoreCursor();
                }
            }

            // Inline confirm hint
            if (_showConfirmClose)
            {
                var confirmStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(1f, 0.85f, 0.5f) } };
                GUI.Label(new Rect(lx, y + H - 56, W - 48, 16), "Changes auto-saved. Close panel?", confirmStyle);
            }

            // Visible "Applied" toast / status (golden, auto-fade)
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                float a = Mathf.Clamp01(_toastTimer / TOAST_DURATION);
                var toastStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.98f, 0.92f, 0.6f, a) }
                };
                GUI.Label(new Rect(x + 20, y + H - 22, W - 40, 18), _toastMessage ?? "Applied", toastStyle);
                if (_toastTimer <= 0f) _toastMessage = null;
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
            catch (System.Exception ex) { Debug.LogWarning($"[SettingsOverlay] TryInvoke({typeName}.{method}) failed: {ex.Message}"); }
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
            catch (System.Exception ex) { Debug.LogWarning($"[SettingsOverlay] TryInvokeEnum({typeName}.{method}) failed: {ex.Message}"); }
        }

        // Round 4/5 Perf UI feedback helper — prefers live profile from bootstrap for dynamic switches
        static string GetCurrentPerfTier()
        {
            var live = GameBootstrap.GetActivePerformanceProfile();
            if (live != null) return live.GetTierSummary();
            int saved = PlayerPrefs.GetInt("TARTARIA_ActivePerfTier", PlayerPrefs.GetInt("TARTARIA_LastHardwareTier", 1));
            var t = (PerformanceProfile.HardwareTier)Mathf.Clamp(saved, 0, 3);
            return t.ToString();
        }

        // ─── M2 Beta Polish Helpers (minimal high-impact) ───
        void DrawSectionHeader(string label, ref int r, int leftX, int wide)
        {
            var hdr = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.85f, 0.5f) }
            };
            GUI.Label(new Rect(leftX, r, wide, 17), "▸ " + label, hdr);
            r += 18;
            // subtle gold underline
            var oldC = GUI.color;
            GUI.color = new Color(0.75f, 0.65f, 0.35f, 0.45f);
            GUI.DrawTexture(new Rect(leftX + 12, r - 2, wide - 60, 1), Texture2D.whiteTexture);
            GUI.color = oldC;
            r += 3;
        }

        void SetToast(string msg)
        {
            _toastMessage = msg;
            _toastTimer = TOAST_DURATION;
            _dirty = true;
        }

        void ResetToDefaults()
        {
            _volume = 1f; AudioListener.volume = 1f; SetMixer("MasterVol", 1f); PlayerPrefs.SetFloat(PP_VOLUME, 1f);
            _music = 0.85f; SetMixer("MusicVol", _music); PlayerPrefs.SetFloat(PP_MUSIC, _music);
            _sfx = 1f; SetMixer("SFXVol", 1f); PlayerPrefs.SetFloat(PP_SFX, 1f);
            _amb = 0.7f; SetMixer("AmbienceVol", _amb); PlayerPrefs.SetFloat(PP_AMB, _amb);
            _sens = 1f; PlayerPrefs.SetFloat(PP_SENS, 1f); Tartaria.Camera.CameraController.SetMouseSensitivity(1f);
            _textScale = 1f; PlayerPrefs.SetFloat(PP_TEXT, 1f); ApplyTextScale();
            _colorblind = 0; PlayerPrefs.SetInt(PP_CB, 0); ApplyColorblind();
            // Reduced motion default
            PlayerPrefs.SetInt("TARTARIA_ReducedMotion", 0);
            _qualityIdx = Mathf.Clamp(1, 0, QualitySettings.names.Length - 1);
            _fullscreen = true;
            PlayerPrefs.Save();
            ApplyAll();
            CaptureInitials();
            _dirty = false;
            SetToast("Defaults Restored — Golden Feel");
        }

        void ApplyMouseSensitivity()
        {
            // CameraController now reads "TARTARIA_MouseSens" on mouse delta for immediate effect.
            // Reflection for direct setter (safe no-op if absent).
            try
            {
                var t = System.Type.GetType("Tartaria.Camera.CameraController, Tartaria.Camera")
                     ?? System.Type.GetType("Tartaria.Camera.CameraController");
                if (t == null) return;
                var m = t.GetMethod("SetMouseSensitivity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
                if (m != null)
                {
                    var inst = FindObjectOfType(t) as MonoBehaviour;
                    m.Invoke(inst, new object[] { _sens });
                }
            }
            catch { /* non-fatal for beta */ }
        }
    }
}
