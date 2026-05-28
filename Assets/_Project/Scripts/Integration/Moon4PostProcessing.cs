using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Post-Processing — The Sunscorched Oasis
    /// Desert atmosphere: intense bloom, warm color grading, heat haze
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon4PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon4PostProcessing] 🎨 Applying desert post-processing...");

            var volumeGO = new GameObject("Moon4_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Intense bloom - harsh sunlight
            if (!profile.Has<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.8f);
                bloom.threshold.Override(0.6f);
                bloom.scatter.Override(0.8f);
                bloom.tint.Override(new Color(1f, 0.9f, 0.7f)); // Warm golden
            }

            // Chromatic Aberration - heat distortion
            if (!profile.Has<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.25f);
            }

            // Vignette - sun-bleached edges
            if (!profile.Has<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.2f);
                vignette.smoothness.Override(0.5f);
                vignette.color.Override(new Color(0.3f, 0.25f, 0.15f)); // Dusty brown
            }

            // Color Adjustments - hot desert palette
            if (!profile.Has<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(-10f); // Slight desaturation
                colorAdj.contrast.Override(20f); // High contrast
                colorAdj.colorFilter.Override(new Color(1f, 0.95f, 0.85f)); // Warm filter
            }

            // White Balance - hot temperature
            if (!profile.Has<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(25f); // Warm
            }

            Debug.Log("[Moon4PostProcessing] ✅ Desert heat applied!");
        }
    }
}
