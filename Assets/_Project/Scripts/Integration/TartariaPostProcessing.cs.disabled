using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Creates a global URP post-processing Volume at runtime and keeps it in sync
    /// with the game's Resonance Score (RS).
    ///
    /// Visual identity ("future-proof 2100"):
    ///   Bloom    — Tartarian-gold glow that intensifies as RS rises. Low RS = dim ruins,
    ///              high RS = radiant resonant city.
    ///   Vignette — Deep purple darkness at the screen edges, always present at low
    ///              intensity to frame the world.
    ///   Color    — Slight saturation boost (+15) so muddy terrain and golden structures
    ///              read with cinematic contrast.
    ///
    /// RS mapping: 0 → dim, 100 → full radiance.
    /// </summary>
    [DefaultExecutionOrder(-60)]
    public class TartariaPostProcessing : MonoBehaviour
    {
        public static TartariaPostProcessing Instance { get; private set; }

        Volume _volume;
        Bloom _bloom;
        Vignette _vignette;
        ColorAdjustments _colorAdj;

        // Bloom intensity range driven by RS
        const float BloomLow  = 0.15f;
        const float BloomHigh = 1.4f;

        // Colour temperature shifts: cold ruins (-8) → warm resonance (+6)
        const float TempLow  = -8f;
        const float TempHigh =  6f;

        float _currentRS;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CreateVolume();
        }

        void OnEnable()
        {
            GameEvents.OnRSChanged += OnRSChanged;
        }

        void OnDisable()
        {
            GameEvents.OnRSChanged -= OnRSChanged;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void CreateVolume()
        {
            var volumeGO = new GameObject("TartariaGlobalPostProcess");
            volumeGO.transform.SetParent(transform);

            _volume = volumeGO.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _volume.profile = profile;

            // ─── Bloom ───────────────────────────────────────────────────
            // URP 17 uses the Bloom override in UnityEngine.Rendering.Universal.
            _bloom = profile.Add<Bloom>(true);
            _bloom.active = true;

            // Intensity — starts low (ruins look grim), rises with RS
            _bloom.intensity.Override(BloomLow);
            _bloom.threshold.Override(0.82f);   // only very bright surfaces glow
            _bloom.scatter.Override(0.68f);      // wide golden diffusion
            _bloom.tint.Override(new Color(1f, 0.92f, 0.55f)); // Tartarian gold tint

            // ─── Vignette ────────────────────────────────────────────────
            _vignette = profile.Add<Vignette>(true);
            _vignette.active = true;
            _vignette.color.Override(new Color(0.06f, 0.02f, 0.12f)); // deep purple
            _vignette.intensity.Override(0.28f);
            _vignette.smoothness.Override(0.45f);
            _vignette.rounded.Override(true);

            // ─── Color Adjustments ───────────────────────────────────────
            _colorAdj = profile.Add<ColorAdjustments>(true);
            _colorAdj.active = true;
            _colorAdj.postExposure.Override(0.15f);   // slight brightening
            _colorAdj.saturation.Override(12f);        // richer colours
            _colorAdj.colorFilter.Override(new Color(1f, 0.97f, 0.92f)); // warm off-white

            Debug.Log("[TartariaPostProcessing] Global URP Volume created — Bloom + Vignette + ColorAdj.");
        }

        void OnRSChanged(float rs)
        {
            _currentRS = Mathf.Clamp(rs, 0f, 100f);
            ApplyRS(_currentRS / 100f);
        }

        void ApplyRS(float t)
        {
            if (_bloom != null)
            {
                _bloom.intensity.Override(Mathf.Lerp(BloomLow, BloomHigh, t));
                // Tint drifts from pale-blue (cold ruins) to warm gold (resonant)
                _bloom.tint.Override(Color.Lerp(
                    new Color(0.65f, 0.75f, 1.0f),  // cold blue — buried, corrupted
                    new Color(1.00f, 0.92f, 0.55f),  // Tartarian gold — restored
                    t));
            }

            if (_colorAdj != null)
            {
                _colorAdj.postExposure.Override(Mathf.Lerp(0.05f, 0.25f, t));
                _colorAdj.saturation.Override(Mathf.Lerp(8f, 22f, t));
                // Colour temperature: cold buried world → warm resonant city
                _colorAdj.colorFilter.Override(Color.Lerp(
                    new Color(0.85f, 0.88f, 1.00f),  // cold/blue tint
                    new Color(1.00f, 0.97f, 0.88f),  // warm/gold tint
                    t));
            }

            if (_vignette != null)
            {
                // Vignette softens as RS rises (darkness lifts as Tartaria is restored)
                _vignette.intensity.Override(Mathf.Lerp(0.38f, 0.18f, t));
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tartaria/Debug/Reset PostProcessing RS")]
        static void ResetRS()
        {
            Instance?.ApplyRS(0f);
        }
#endif
    }
}
