using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    /// <summary>
    /// PostProcessingSetup — Global Volume setup with Bloom + ACES + Vignette.
    /// TODO from REALITY_CHECK Phase 2.
    /// </summary>
    public class PostProcessingSetup : MonoBehaviour
    {
        public static PostProcessingSetup Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }
        [Header("Post-Processing Settings")]
        [SerializeField] private Volume globalVolume;
        [SerializeField] private VolumeProfile profile;

        [Header("Effect Settings")]
        [SerializeField] private float bloomIntensity = 0.3f;
        [SerializeField] private float vignetteIntensity = 0.2f;
        [SerializeField] private bool enableDepthOfField = false;

        void Start()
        {
            SetupPostProcessing();
        }

        void SetupPostProcessing()
        {
            // Create Global Volume if not exists
            if (globalVolume == null)
            {
                var volumeObj = new GameObject("Global Volume");
                volumeObj.transform.SetParent(transform);
                globalVolume = volumeObj.AddComponent<Volume>();
                globalVolume.isGlobal = true;
                globalVolume.priority = 1;
            }

            // Create profile if not exists
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                globalVolume.profile = profile;
            }

            // Add Bloom
            if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>(true);
                bloom.intensity.value = bloomIntensity;
                bloom.threshold.value = 1.0f;
                bloom.scatter.value = 0.7f;
                Debug.Log("[PostProcessingSetup] Added Bloom");
            }

            // Add Tonemapping (ACES)
            if (!profile.Has<Tonemapping>())
            {
                var tonemapping = profile.Add<Tonemapping>(true);
                tonemapping.mode.value = TonemappingMode.ACES;
                Debug.Log("[PostProcessingSetup] Added ACES Tonemapping");
            }

            // Add Vignette
            if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.value = vignetteIntensity;
                vignette.smoothness.value = 0.4f;
                vignette.color.value = Color.black;
                Debug.Log("[PostProcessingSetup] Added Vignette");
            }

            // Add Depth of Field (for tuning mini-game)
            if (enableDepthOfField && !profile.Has<DepthOfField>())
            {
                var dof = profile.Add<DepthOfField>(true);
                dof.mode.value = DepthOfFieldMode.Bokeh;
                dof.focusDistance.value = 5f;
                dof.aperture.value = 4f;
                dof.focalLength.value = 50f;
                dof.active = false; // Disabled by default
                Debug.Log("[PostProcessingSetup] Added Depth of Field");
            }

            Debug.Log("[PostProcessingSetup] ✅ Post-processing complete!");
        }

        /// <summary>
        /// Enable depth of field for tuning mini-game focus effect.
        /// </summary>
        public void EnableDepthOfField(bool enable)
        {
            if (profile.Has<DepthOfField>())
            {
                profile.Get<DepthOfField>().active = enable;
            }
        }

        /// <summary>
        /// Adjust bloom intensity dynamically (e.g., during restoration).
        /// </summary>
        public void SetBloomIntensity(float intensity)
        {
            if (profile.Has<Bloom>())
            {
                profile.Get<Bloom>().intensity.value = intensity;
            }
        }
    }
}
