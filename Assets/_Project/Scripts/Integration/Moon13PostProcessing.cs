using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Post-Processing — The Aether Convergence
    /// FINAL LEVEL atmosphere: brilliant bloom, radiant glow, epic cinematics
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon13PostProcessing : MonoBehaviour
    {
        Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("[Moon13PostProcessing] 🎨 APPLYING FINAL LEVEL POST-PROCESSING");
            Debug.Log("    ✨ AETHER CONVERGENCE - EPIC RADIANCE ✨");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var volumeGO = new GameObject("Moon13_PostProcessVolume");
            volumeGO.transform.SetParent(transform);
            
            postProcessVolume = volumeGO.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postProcessVolume.profile = profile;

            // Maximum bloom - aether radiance
            if (profile.TryAdd<Bloom>(out var bloom))
            {
                bloom.intensity.Override(1.2f); // Beyond standard max
                bloom.threshold.Override(0.5f);
                bloom.scatter.Override(1.0f); // Full scatter
                bloom.tint.Override(new Color(0.95f, 1f, 1f)); // Brilliant cyan-white
            }

            // Light Chromatic Aberration - aether shimmer
            if (profile.TryAdd<ChromaticAberration>(out var ca))
            {
                ca.intensity.Override(0.15f);
            }

            // Subtle Vignette - focus on convergence
            if (profile.TryAdd<Vignette>(out var vignette))
            {
                vignette.intensity.Override(0.2f);
                vignette.smoothness.Override(0.6f);
                vignette.color.Override(new Color(0.3f, 0.4f, 0.5f)); // Subtle blue-gray
            }

            // Color Adjustments - epic finale palette
            if (profile.TryAdd<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.saturation.Override(30f); // Very saturated
                colorAdj.contrast.Override(20f);
                colorAdj.colorFilter.Override(new Color(1f, 1f, 1f)); // Pure
            }

            // White Balance - brilliant neutral
            if (profile.TryAdd<WhiteBalance>(out var wb))
            {
                wb.temperature.Override(5f); // Slightly warm
            }

            Debug.Log("[Moon13PostProcessing] ✅ AETHER BRILLIANCE AT MAXIMUM!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }
    }
}
