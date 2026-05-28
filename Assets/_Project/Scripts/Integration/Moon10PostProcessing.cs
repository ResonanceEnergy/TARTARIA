using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Post-Processing — The Temporal Rift
    /// Time distortion atmosphere: chromatic aberration, shifting colors, motion blur
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon10PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon10PostProcessing] 🎨 Applying temporal distortion post-processing...");

            var volumeGO = new GameObject("Moon10_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Moderate bloom - temporal energy
            if (if (!profile.Has<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.5f);
                bloom.threshold.Override(0.8f);
                bloom.scatter.Override(0.6f);
                bloom.tint.Override(new Color(0.9f, 0.95f, 1f)); // Neutral bright
            }

            // Extreme Chromatic Aberration - time fracture
            if (if (!profile.Has<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.4f);
            }

            // Vignette - temporal edges
            if (if (!profile.Has<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.3f);
                vignette.smoothness.Override(0.45f);
                vignette.color.Override(new Color(0.15f, 0.15f, 0.2f)); // Neutral dark
            }

            // Color Adjustments - shifting time palette
            if (if (!profile.Has<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(0f); // Neutral
                colorAdj.contrast.Override(20f); // High contrast for clarity
                colorAdj.colorFilter.Override(new Color(1f, 1f, 1f)); // No filter
            }

            // White Balance - neutral (time has no color)
            if (if (!profile.Has<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(0f);
            }

            Debug.Log("[Moon10PostProcessing] ✅ Time fractures around you!");
        }
    }
}
