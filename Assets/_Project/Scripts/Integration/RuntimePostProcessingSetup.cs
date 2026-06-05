using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime Post-Processing Setup — Creates Global Volume with URP effects
    /// Adds Bloom, ACES tonemapping, and Vignette for 2026 AAA visual quality
    /// Runs once on scene load
    /// </summary>
    [DefaultExecutionOrder(-80)]
    public class RuntimePostProcessingSetup : MonoBehaviour
    {
        [Header("Post-Processing Settings")]
        [SerializeField, Range(0f, 1f)] float bloomIntensity = 0.25f;
        [SerializeField, Range(0f, 10f)] float bloomThreshold = 0.9f;
        [SerializeField, Range(0f, 1f)] float vignetteIntensity = 0.3f;
        [SerializeField] bool enableColorAdjustments = true;

        void Awake()
        {
            SetupGlobalVolume();
        }

        void SetupGlobalVolume()
        {
            // Check if Global Volume already exists
            var existingVolume = FindFirstObjectByType<Volume>();
            if (existingVolume != null && existingVolume.isGlobal)
            {
                Debug.Log("[RuntimePostProcessing] Global Volume already exists, skipping setup");
                return;
            }

            // Create Global Volume GameObject
            var volumeGO = new GameObject("Global Volume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            // Create Volume Profile
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;

            // Add Bloom
            if (profile.TryGet(out Bloom bloom))
            {
                // Already exists
            }
            else
            {
                bloom = profile.Add<Bloom>();
            }
            bloom.active = true;
            bloom.intensity.value = bloomIntensity;
            bloom.intensity.overrideState = true;
            bloom.threshold.value = bloomThreshold;
            bloom.threshold.overrideState = true;
            bloom.scatter.value = 0.7f;
            bloom.scatter.overrideState = true;

            // Add Tonemapping (ACES)
            if (profile.TryGet(out Tonemapping tonemap))
            {
                // Already exists
            }
            else
            {
                tonemap = profile.Add<Tonemapping>();
            }
            tonemap.active = true;
            tonemap.mode.value = TonemappingMode.ACES;
            tonemap.mode.overrideState = true;

            // Add Vignette
            if (profile.TryGet(out Vignette vignette))
            {
                // Already exists
            }
            else
            {
                vignette = profile.Add<Vignette>();
            }
            vignette.active = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.intensity.overrideState = true;
            vignette.smoothness.value = 0.4f;
            vignette.smoothness.overrideState = true;
            vignette.color.value = new Color(0.05f, 0.05f, 0.1f); // Subtle blue tint
            vignette.color.overrideState = true;

            // Add Color Adjustments (optional - subtle warmth)
            if (enableColorAdjustments)
            {
                if (profile.TryGet(out ColorAdjustments colorAdj))
                {
                    // Already exists
                }
                else
                {
                    colorAdj = profile.Add<ColorAdjustments>();
                }
                colorAdj.active = true;
                colorAdj.postExposure.value = 0.2f; // Slight brightness boost
                colorAdj.postExposure.overrideState = true;
                colorAdj.contrast.value = 5f; // Subtle contrast increase
                colorAdj.contrast.overrideState = true;
                colorAdj.saturation.value = 5f; // Slight saturation boost
                colorAdj.saturation.overrideState = true;
            }

            Debug.Log("[RuntimePostProcessing] Created Global Volume with Bloom + ACES + Vignette");

            // Make it persistent across scene loads (optional)
            // DontDestroyOnLoad(volumeGO);
        }
    }
}
