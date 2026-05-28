using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Post-Processing — The Umbral Sanctum
    /// Shadow atmosphere: minimal bloom, heavy vignette, desaturated darkness
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon12PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon12PostProcessing] 🎨 Applying shadow realm post-processing...");

            var volumeGO = new GameObject("Moon12_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Minimal bloom - only faintest glows
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0.2f);
                bloom.threshold.Override(1.2f); // Very high threshold
                bloom.scatter.Override(0.4f);
                bloom.tint.Override(new Color(0.6f, 0.6f, 0.7f)); // Dim blue-gray
            }

            // Subtle Chromatic Aberration - shadow distortion
            if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.intensity.Override(0.12f);
            }

            // Heavy Vignette - consuming darkness
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.55f); // Very dark edges
                vignette.smoothness.Override(0.3f);
                vignette.color.Override(new Color(0f, 0f, 0.05f)); // Near-black
            }

            // Color Adjustments - drained palette
            if (!profile.Has<ColorAdjustments>())
            {
                var colorAdj = profile.Add<ColorAdjustments>();
                colorAdj.saturation.Override(-30f); // Heavy desaturation
                colorAdj.contrast.Override(10f);
                colorAdj.colorFilter.Override(new Color(0.7f, 0.7f, 0.8f)); // Dark blue-gray filter
            }

            // White Balance - cold void
            if (!profile.Has<WhiteBalance>())
            {
                var wb = profile.Add<WhiteBalance>();
                wb.temperature.Override(-25f); // Very cold
            }

            Debug.Log("[Moon12PostProcessing] ✅ Shadow realm embraces you!");
        }
    }
}
