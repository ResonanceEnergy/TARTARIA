using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Post-Processing — The Prismatic Nexus
    /// Rainbow spectrum atmosphere: vibrant bloom, rainbow tint cycling, high saturation
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon11PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon11PostProcessing] 🎨 Applying prismatic spectrum post-processing...");

            var volumeGO = new GameObject("Moon11_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Vibrant bloom - prismatic refraction
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0.7f);
                bloom.threshold.Override(0.6f);
                bloom.scatter.Override(0.9f);
                bloom.tint.Override(new Color(1f, 1f, 1f)); // Pure white (lets spectrum show)
            }

            // Light Chromatic Aberration - rainbow split
            if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.intensity.Override(0.2f);
            }

            // Minimal Vignette - let colors shine
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.1f);
                vignette.smoothness.Override(0.5f);
                vignette.color.Override(new Color(0.2f, 0.2f, 0.2f)); // Neutral dark
            }

            // Color Adjustments - maximum vibrancy
            if (!profile.Has<ColorAdjustments>())
            {
                var colorAdj = profile.Add<ColorAdjustments>();
                colorAdj.saturation.Override(40f); // Hyper-saturated rainbow
                colorAdj.contrast.Override(15f);
                colorAdj.colorFilter.Override(new Color(1f, 1f, 1f)); // No filter
            }

            // White Balance - neutral (let spectrum shine)
            if (!profile.Has<WhiteBalance>())
            {
                var wb = profile.Add<WhiteBalance>();
                wb.temperature.Override(0f);
            }

            Debug.Log("[Moon11PostProcessing] ✅ Prismatic spectrum refracting!");
        }
    }
}
