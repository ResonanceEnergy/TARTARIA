using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Post-Processing — The Molten Forge
    /// Volcanic atmosphere: intense bloom, red-orange tint, heat distortion
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon6PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon6PostProcessing] 🎨 Applying volcanic forge post-processing...");

            var volumeGO = new GameObject("Moon6_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Intense bloom - molten glow
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(1.0f); // Maximum intensity
                bloom.threshold.Override(0.5f);
                bloom.scatter.Override(0.9f);
                bloom.tint.Override(new Color(1f, 0.4f, 0.1f)); // Lava orange
            }

            // Chromatic Aberration - extreme heat distortion
            if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.intensity.Override(0.3f);
            }

            // Vignette - dark volcanic edges
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.35f);
                vignette.smoothness.Override(0.4f);
                vignette.color.Override(new Color(0.1f, 0.05f, 0f)); // Dark red-black
            }

            // Color Adjustments - hot forge palette
            if (!profile.Has<ColorAdjustments>())
            {
                colorAdj.saturation.Override(20f); // Very saturated
                colorAdj.contrast.Override(15f);
                colorAdj.colorFilter.Override(new Color(1f, 0.7f, 0.5f)); // Hot orange filter
            }

            // White Balance - extreme heat
            if (!profile.Has<WhiteBalance>())
            {
                wb.temperature.Override(40f); // Very hot
            }

            Debug.Log("[Moon6PostProcessing] ✅ Volcanic heat radiating!");
        }
    }
}
