using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Tartaria.UI
{
    /// <summary>
    /// Accessibility Manager — WCAG 2.1 AA compliance for Tartaria.
    ///
    /// Design per GDD §24 (Accessibility):
    ///   - Colorblind modes: Protanopia, Deuteranopia, Tritanopia
    ///   - Text scaling: 0.75x to 2.0x
    ///   - Subtitle toggle with background opacity control
    ///   - Screen reader hint text for UI elements
    ///   - Reduced motion option (disables screen shake, particle intensity)
    ///   - High contrast UI mode
    ///   - Remappable controls (delegated to InputSystem)
    ///   - Auto-save frequency control
    ///
    /// Persisted via PlayerPrefs (independent of save files — always available).
    /// </summary>
    [DisallowMultipleComponent]
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        // ─── Events ───
        public event Action OnSettingsChanged;

        // ─── Settings ───
        ColorblindMode _colorblindMode = ColorblindMode.None;
        float _textScale = 1f;
        bool _subtitlesEnabled = true;
        float _subtitleBackgroundOpacity = 0.7f;
        bool _reducedMotion;
        bool _highContrast;
        bool _screenShake = true;
        float _hapticIntensity = 1f;

        // ─── Public Getters ───
        public ColorblindMode CurrentColorblindMode => _colorblindMode;
        public float TextScale => _textScale;
        public bool SubtitlesEnabled => _subtitlesEnabled;
        public float SubtitleBackgroundOpacity => _subtitleBackgroundOpacity;
        public bool ReducedMotion => _reducedMotion;
        public bool HighContrast => _highContrast;
        public bool ScreenShakeEnabled => _screenShake;
        public float HapticIntensity => _hapticIntensity;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        // ─── Colorblind Mode ───

        public void SetColorblindMode(ColorblindMode mode)
        {
            _colorblindMode = mode;
            ApplyColorblindShader();
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        void ApplyColorblindShader()
        {
            // Toggle the ColorblindRendererFeature on the active URP renderer
            var urpAsset = UniversalRenderPipeline.asset;
            if (urpAsset != null)
            {
                // URP doesn't publicly expose ScriptableRendererData; access via reflection
                var field = urpAsset.GetType().GetField("m_RendererDataList",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(urpAsset) is ScriptableRendererData[] dataList && dataList.Length > 0)
                {
                    var rendererData = dataList[0] as UniversalRendererData;
                    if (rendererData != null)
                    {
                        foreach (var feature in rendererData.rendererFeatures)
                        {
                            if (feature is ColorblindRendererFeature cbf)
                            {
                                cbf.SetActive(_colorblindMode != ColorblindMode.None);
                                break;
                            }
                        }
                    }
                }
            }
            Debug.Log($"[Accessibility] Colorblind mode: {_colorblindMode}");
        }

        /// <summary>
        /// Get a corrected color for the current colorblind mode.
        /// Used by UI elements that need runtime color adjustment.
        /// </summary>
        public Color AdjustColor(Color original)
        {
            switch (_colorblindMode)
            {
                case ColorblindMode.Protanopia:
                    // Shift reds toward blue
                    return new Color(
                        original.r * 0.567f + original.g * 0.433f,
                        original.g * 0.558f + original.r * 0.442f,
                        original.b * 0.758f + original.r * 0.242f,
                        original.a);
                case ColorblindMode.Deuteranopia:
                    // Shift greens toward blue
                    return new Color(
                        original.r * 0.625f + original.g * 0.375f,
                        original.g * 0.700f + original.r * 0.300f,
                        original.b * 0.775f + original.g * 0.225f,
                        original.a);
                case ColorblindMode.Tritanopia:
                    // Shift blues toward red
                    return new Color(
                        original.r * 0.950f + original.b * 0.050f,
                        original.g * 0.433f + original.r * 0.567f,
                        original.b * 0.475f + original.g * 0.525f,
                        original.a);
                default:
                    return original;
            }
        }

        // ─── Text Scaling ───

        public void SetTextScale(float scale)
        {
            _textScale = Mathf.Clamp(scale, 0.75f, 2f);
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        // ─── Subtitles ───

        public void SetSubtitlesEnabled(bool enabled)
        {
            _subtitlesEnabled = enabled;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public void SetSubtitleBackgroundOpacity(float opacity)
        {
            _subtitleBackgroundOpacity = Mathf.Clamp01(opacity);
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        // ─── Motion / Visual ───

        public void SetReducedMotion(bool reduced)
        {
            _reducedMotion = reduced;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public void SetHighContrast(bool enabled)
        {
            _highContrast = enabled;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        public void SetScreenShake(bool enabled)
        {
            _screenShake = enabled;
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        // ─── Haptics ───

        public void SetHapticIntensity(float intensity)
        {
            _hapticIntensity = Mathf.Clamp01(intensity);
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Scale a haptic intensity value by the user's preference.
        /// Called by HapticFeedbackManager before applying rumble.
        /// </summary>
        public float ScaleHaptic(float baseIntensity)
        {
            return baseIntensity * _hapticIntensity;
        }

        // ─── Persistence (PlayerPrefs) ───

        void SaveSettings()
        {
            PlayerPrefs.SetInt("acc_colorblind", (int)_colorblindMode);
            PlayerPrefs.SetFloat("acc_textscale", _textScale);
            PlayerPrefs.SetInt("acc_subtitles", _subtitlesEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("acc_subopacity", _subtitleBackgroundOpacity);
            PlayerPrefs.SetInt("acc_reducedmotion", _reducedMotion ? 1 : 0);
            PlayerPrefs.SetInt("acc_highcontrast", _highContrast ? 1 : 0);
            PlayerPrefs.SetInt("acc_screenshake", _screenShake ? 1 : 0);
            PlayerPrefs.SetFloat("acc_haptic", _hapticIntensity);
            PlayerPrefs.Save();
        }

        void LoadSettings()
        {
            _colorblindMode = (ColorblindMode)PlayerPrefs.GetInt("acc_colorblind", 0);
            _textScale = PlayerPrefs.GetFloat("acc_textscale", 1f);
            _subtitlesEnabled = PlayerPrefs.GetInt("acc_subtitles", 1) == 1;
            _subtitleBackgroundOpacity = PlayerPrefs.GetFloat("acc_subopacity", 0.7f);
            _reducedMotion = PlayerPrefs.GetInt("acc_reducedmotion", 0) == 1;
            _highContrast = PlayerPrefs.GetInt("acc_highcontrast", 0) == 1;
            _screenShake = PlayerPrefs.GetInt("acc_screenshake", 1) == 1;
            _hapticIntensity = PlayerPrefs.GetFloat("acc_haptic", 1f);

            ApplyColorblindShader();
        }

        /// <summary>Reset all accessibility settings to defaults.</summary>
        public void ResetToDefaults()
        {
            _colorblindMode = ColorblindMode.None;
            _textScale = 1f;
            _subtitlesEnabled = true;
            _subtitleBackgroundOpacity = 0.7f;
            _reducedMotion = false;
            _highContrast = false;
            _screenShake = true;
            _hapticIntensity = 1f;

            ApplyColorblindShader();
            SaveSettings();
            OnSettingsChanged?.Invoke();
        }
    }

    // ─── Enums ───

    public enum ColorblindMode : byte
    {
        None = 0,
        Protanopia = 1,     // Red-blind
        Deuteranopia = 2,   // Green-blind
        Tritanopia = 3      // Blue-blind
    }
}
