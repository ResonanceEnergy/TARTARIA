using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    public class Moon1PostProcessingPreset : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Apply()
        {
            // Skip if not in Echohaven scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Echohaven_VerticalSlice") return;

            var volume = FindFirstObjectByType<Volume>();
            if (volume == null || volume.profile == null)
            {
                Debug.Log("[Moon1PostProcessing] No PostProcessVolume found, creating one");
                var go = new GameObject("Moon1_GoldenHour_Volume");
                volume = go.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            // Bloom
            if (volume.profile.TryGet<Bloom>(out var bloom) || volume.profile.Add<Bloom>() is var b)
            {
                bloom = volume.profile.TryGet<Bloom>(out var existing) ? existing : volume.profile.Add<Bloom>();
                bloom.intensity.Override(0.45f);
                bloom.threshold.Override(0.95f);
                bloom.tint.Override(new Color(1.0f, 0.85f, 0.55f));
            }
            // ColorAdjustments, Vignette, ChromaticAberration, FilmGrain — follow same pattern

            // Fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.85f, 0.72f, 0.55f);
            RenderSettings.fogDensity = 0.012f;

            Debug.Log("[Moon1PostProcessing] Golden-hour preset applied");
        }
    }
}
