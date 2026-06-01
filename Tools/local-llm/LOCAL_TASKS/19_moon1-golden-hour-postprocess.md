# TICKET: Moon1 PostProcessing — golden-hour Tartarian sky preset

## Output destination
`Assets/_Project/Scripts/Integration/Moon1PostProcessingPreset.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- Brace-balanced, one C# file
- Compiles against Unity 6 LTS + URP. Uses `UnityEngine.Rendering` + `UnityEngine.Rendering.Universal`.
- Auto-bootstraps via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- Finds the scene's PostProcessVolume (`FindObjectOfType<Volume>()`) and sets its profile to the Moon 1 golden-hour preset (override in code)

## Spec

Per docs/15 §8 and docs/03 Moon 1 atmosphere: "Golden hour Tartarian sky — warm copper light, soft falloff, dust-particle ambience, slight chromatic aberration at edges, gentle bloom around restored buildings."

Override these `Volume.profile` settings if components exist:

| Component | Setting | Value |
|---|---|---|
| `Bloom` | `intensity` | 0.45 |
| `Bloom` | `threshold` | 0.95 |
| `Bloom` | `tint` | (1.0, 0.85, 0.55) — warm copper |
| `ColorAdjustments` | `postExposure` | 0.2 |
| `ColorAdjustments` | `contrast` | 8f |
| `ColorAdjustments` | `colorFilter` | (1.0, 0.92, 0.78) — slight golden tint |
| `ColorAdjustments` | `saturation` | -5f |
| `Vignette` | `intensity` | 0.25 |
| `Vignette` | `smoothness` | 0.4 |
| `Vignette` | `color` | (0.20, 0.12, 0.08) — dark sienna corners |
| `ChromaticAberration` | `intensity` | 0.12 |
| `FilmGrain` | `intensity` | 0.18 |
| `FilmGrain` | `type` | `FilmGrainLookup.Medium2` |

All settings should be applied with `.overrideState = true` so the volume actually applies them.

Also: set `RenderSettings.fogColor` to `(0.85, 0.72, 0.55)` and `RenderSettings.fogDensity` to `0.012f` for a golden-dust atmosphere.

## Implementation sketch

```csharp
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
```

## Do NOT
- Don't change existing Volume profiles if they're already configured (check `volume.profile.components.Count > 0` first).
- Don't add new packages or external dependencies. URP volume API is built-in.
- Don't reference `UnityEngine.Rendering.PostProcessing` (legacy, deprecated).
- Don't split string literals across lines.
