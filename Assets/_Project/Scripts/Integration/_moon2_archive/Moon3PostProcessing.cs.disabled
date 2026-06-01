using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Post-Processing — The Verdant Labyrinth
    /// Jungle atmosphere: bloom, chromatic aberration, vignette
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon3PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon3PostProcessing] 🎨 Applying jungle post-processing...");

            // Create volume game object
            var volumeGO = new GameObject("Moon3_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            // Create profile
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Bloom - soft jungle glow
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0.4f);
                bloom.threshold.Override(0.8f);
                bloom.scatter.Override(0.7f);
                bloom.tint.Override(new Color(0.8f, 1f, 0.7f)); // Green tint
            }

            // Chromatic Aberration - subtle lens distortion
            if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.intensity.Override(0.15f);
            }

            // Vignette - focus on center
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.25f);
                vignette.smoothness.Override(0.4f);
                vignette.color.Override(new Color(0.1f, 0.2f, 0.1f)); // Dark green
            }

            // Color Adjustments - enhance jungle greens
            if (!profile.Has<ColorAdjustments>())
            {
                var colorAdj = profile.Add<ColorAdjustments>();
                colorAdj.saturation.Override(15f); // Boost saturation
                colorAdj.contrast.Override(10f);
            }

            Debug.Log("[Moon3PostProcessing] ✅ Jungle atmosphere enhanced!");
        }
    }
}
