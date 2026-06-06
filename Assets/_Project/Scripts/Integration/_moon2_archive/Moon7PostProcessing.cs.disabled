using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Post-Processing — The Abyssal Depths
    /// Underwater atmosphere: blue tint, soft bloom, depth fog, chromatic aberration
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon7PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon7PostProcessing] 🎨 Applying underwater post-processing...");

            var volumeGO = new GameObject("Moon7_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Soft bloom - bioluminescent glow
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0.5f);
                bloom.threshold.Override(0.7f);
                bloom.scatter.Override(0.8f);
                bloom.tint.Override(new Color(0.3f, 0.7f, 0.9f)); // Cyan-blue
            }

            // Chromatic Aberration - water refraction
            if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.intensity.Override(0.2f);
            }

            // Vignette - deep water darkness
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.4f);
                vignette.smoothness.Override(0.5f);
                vignette.color.Override(new Color(0f, 0.05f, 0.1f)); // Deep blue-black
            }

            // Color Adjustments - underwater palette
            if (!profile.Has<ColorAdjustments>())
            {
                var colorAdj = profile.Add<ColorAdjustments>();
                colorAdj.saturation.Override(-5f); // Muted underwater
                colorAdj.contrast.Override(5f); // Low contrast
                colorAdj.colorFilter.Override(new Color(0.6f, 0.8f, 1f)); // Blue filter
            }

            // White Balance - cool underwater
            if (!profile.Has<WhiteBalance>())
            {
                var wb = profile.Add<WhiteBalance>();
                wb.temperature.Override(-15f);
            }

            Debug.Log("[Moon7PostProcessing] ✅ Submerged in abyssal depths!");
        }
    }
}
