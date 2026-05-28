using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 Post-Processing — The Blighted Wastes
    /// Corruption atmosphere: purple tint, twisted bloom, high contrast
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon9PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon9PostProcessing] 🎨 Applying corruption post-processing...");

            var volumeGO = new GameObject("Moon9_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Purple bloom - corrupt energy
            if (profile.TryAdd<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.6f);
                bloom.threshold.Override(0.6f);
                bloom.scatter.Override(0.9f);
                bloom.tint.Override(new Color(0.7f, 0.3f, 0.9f)); // Purple
            }

            // Heavy Chromatic Aberration - reality distortion
            if (profile.TryAdd<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.35f);
            }

            // Vignette - dark corruption
            if (profile.TryAdd<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.45f);
                vignette.smoothness.Override(0.35f);
                vignette.color.Override(new Color(0.1f, 0.05f, 0.15f)); // Dark purple
            }

            // Color Adjustments - twisted palette
            if (profile.TryAdd<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(25f); // Hyper-saturated
                colorAdj.contrast.Override(30f); // Extreme contrast
                colorAdj.colorFilter.Override(new Color(0.9f, 0.8f, 1f)); // Purple filter
            }

            // White Balance - unnatural tint
            if (profile.TryAdd<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(-10f);
                wb.tint.Override(15f); // Magenta shift
            }

            Debug.Log("[Moon9PostProcessing] ✅ Corruption spreads across vision!");
        }
    }
}
