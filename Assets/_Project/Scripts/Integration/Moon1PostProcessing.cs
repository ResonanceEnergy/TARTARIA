using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Post Processing — Configures URP post effects for Echohaven atmosphere
    /// Warm golden lighting, soft bloom, subtle vignette for ancient ruins feel
    /// </summary>
    [DefaultExecutionOrder(-77)] // After player setup (-78)
    public class Moon1PostProcessing : MonoBehaviour
    {
        [Header("Post Process Settings")]
        [SerializeField] bool enableBloom = true;
        [SerializeField] float bloomIntensity = 0.3f;
        [SerializeField] bool enableVignette = true;
        [SerializeField] float vignetteIntensity = 0.25f;
        [SerializeField] bool enableColorGrading = true;

        private Volume postProcessVolume;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            Debug.Log("[Moon1PostProcessing] Setting up post-processing effects...");

            // Find or create global volume
            postProcessVolume = FindFirstObjectByType<Volume>();
            if (postProcessVolume == null)
            {
                var volumeObj = new GameObject("Global Post Process Volume");
                postProcessVolume = volumeObj.AddComponent<Volume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 1;
            }

            // Create or get profile
            if (postProcessVolume.profile == null)
            {
                postProcessVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            ConfigureBloom();
            ConfigureVignette();
            ConfigureColorGrading();
            ConfigureAmbientOcclusion();

            Debug.Log("[Moon1PostProcessing] ✅ Post-processing configured!");
        }

        void ConfigureBloom()
        {
            if (!enableBloom) return;

            var bloom = postProcessVolume.// DISABLED: profile.TryGet<Bloom>();
            if (bloom == null)
            {
                bloom = postProcessVolume.profile.Add<Bloom>();
            }

            bloom.active = true;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomIntensity;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.9f; // Only brightest areas
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.5f;

            Debug.Log($"  ✓ Bloom: intensity={bloomIntensity}, threshold=0.9");
        }

        void ConfigureVignette()
        {
            if (!enableVignette) return;

            var vignette = postProcessVolume.// DISABLED: profile.TryGet<Vignette>();
            if (vignette == null)
            {
                vignette = postProcessVolume.profile.Add<Vignette>();
            }

            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.4f;
            vignette.color.overrideState = true;
            vignette.color.value = new Color(0.1f, 0.05f, 0f); // Warm sepia

            Debug.Log($"  ✓ Vignette: intensity={vignetteIntensity}, warm sepia tone");
        }

        void ConfigureColorGrading()
        {
            if (!enableColorGrading) return;

            var colorGrading = postProcessVolume.// DISABLED: profile.TryGet<ColorAdjustments>();
            if (colorGrading == null)
            {
                colorGrading = postProcessVolume.profile.Add<ColorAdjustments>();
            }

            colorGrading.active = true;

            // Warm, golden tone for ancient ruins
            colorGrading.colorFilter.overrideState = true;
            colorGrading.colorFilter.value = new Color(1f, 0.95f, 0.85f); // Warm white

            colorGrading.saturation.overrideState = true;
            colorGrading.saturation.value = 5f; // Slightly more saturated

            colorGrading.contrast.overrideState = true;
            colorGrading.contrast.value = 8f; // Increased contrast

            Debug.Log("  ✓ Color Grading: warm golden tone, +5 saturation, +8 contrast");
        }

        void ConfigureAmbientOcclusion()
        {
            // Note: SSAO may not be available in all URP versions
            // This is optional enhancement for depth

            Debug.Log("  ℹ Ambient Occlusion: Configure via URP Renderer settings");
        }
    }
}
