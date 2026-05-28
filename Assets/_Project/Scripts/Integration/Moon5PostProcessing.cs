using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Post-Processing — The Frostbound Citadel
    /// Ice atmosphere: blue tint, sharp bloom, high contrast
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon5PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon5PostProcessing] 🎨 Applying frozen citadel post-processing...");

            var volumeGO = new GameObject("Moon5_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Sharp bloom - ice crystal sparkle
            if (profile.TryAdd<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.6f);
                bloom.threshold.Override(0.9f); // Only brightest surfaces
                bloom.scatter.Override(0.5f);
                bloom.tint.Override(new Color(0.8f, 0.9f, 1f)); // Ice blue
            }

            // Chromatic Aberration - ice refraction
            if (profile.TryAdd<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.1f);
            }

            // Vignette - cold darkness
            if (profile.TryAdd<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.3f);
                vignette.smoothness.Override(0.3f);
                vignette.color.Override(new Color(0.05f, 0.1f, 0.15f)); // Deep blue-black
            }

            // Color Adjustments - frozen palette
            if (profile.TryAdd<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(5f);
                colorAdj.contrast.Override(25f); // Very high contrast
                colorAdj.colorFilter.Override(new Color(0.9f, 0.95f, 1f)); // Cool blue filter
            }

            // White Balance - cold temperature
            if (profile.TryAdd<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(-20f); // Cold
            }

            Debug.Log("[Moon5PostProcessing] ✅ Ice atmosphere frozen in place!");
        }
    }
}
