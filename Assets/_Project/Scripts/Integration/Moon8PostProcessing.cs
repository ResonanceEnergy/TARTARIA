using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Post-Processing — The Celestial Spires
    /// Sky atmosphere: bright bloom, ethereal glow, soft focus
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon8PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon8PostProcessing] 🎨 Applying celestial sky post-processing...");

            var volumeGO = new GameObject("Moon8_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Bright bloom - heavenly radiance
            if (if (!profile.Has<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.7f);
                bloom.threshold.Override(0.7f);
                bloom.scatter.Override(0.7f);
                bloom.tint.Override(new Color(1f, 1f, 1f)); // Pure white
            }

            // Subtle Chromatic Aberration - ethereal
            if (if (!profile.Has<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.08f);
            }

            // Vignette - soft cloud edges
            if (if (!profile.Has<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.15f);
                vignette.smoothness.Override(0.6f);
                vignette.color.Override(new Color(0.7f, 0.8f, 0.9f)); // Light blue
            }

            // Color Adjustments - bright sky palette
            if (if (!profile.Has<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(10f);
                colorAdj.contrast.Override(5f); // Soft contrast
                colorAdj.colorFilter.Override(new Color(1f, 1f, 1f)); // No filter
            }

            // White Balance - neutral bright
            if (if (!profile.Has<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(5f); // Slightly warm
            }

            Debug.Log("[Moon8PostProcessing] ✅ Ascending to celestial heights!");
        }
    }
}
