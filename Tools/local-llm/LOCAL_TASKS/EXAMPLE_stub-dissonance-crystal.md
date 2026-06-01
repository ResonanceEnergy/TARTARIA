# TICKET: Stub Moon 2 DissonanceCrystal MonoBehaviour

## Output destination
`Assets/_Project/Scripts/Gameplay/DissonanceCrystal.cs`

## Acceptance criteria
- Namespace: `Tartaria.Gameplay`
- One C# file, one class, brace-balanced, ends on namespace close
- Compiles against Unity 6 LTS with assemblies: Tartaria.Core, Tartaria.Gameplay
- Uses URP material conventions: `mat.SetColor("_BaseColor", c)`, NEVER `mat.color = c`
- No references to Tartaria.AI from this file (asmdef one-way rule)
- No GameObject.CreatePrimitive without a URP/Lit Shader.Find fallback assigning `_BaseColor`
- All UI Image components (if any) MUST set `sprite` to a non-null Sprite, or Unity 6 renders nothing

## Spec
Per `docs/03_CAMPAIGN_13_MOONS.md` Moon 2 (Lunar / Crystalline Caverns):
"Dissonance crystals" are environmental hazards that pulse with off-key frequencies
and drain the player's Aether when they get within proximity. The player must
either tune them (using TuningMiniGame) or destroy them by overlapping with a
Lirael harmonic pulse.

Required public API:
- `public float DissonanceHz` — random pick from [666f, 777f, 888f] on Awake
- `public float DrainRadius = 4f`
- `public float DrainPerSecond = 5f`
- `public bool IsCleansed { get; private set; }`
- `public event System.Action OnCleansed`
- `public void Cleanse()` — sets IsCleansed=true, fires event, swaps emission color, schedules Destroy(gameObject, 3f)

Behavior in `Update()`:
- Find Player by tag once, cache transform
- If `!IsCleansed` and player within DrainRadius, fire `GameEvents.RaiseAetherChange(-DrainPerSecond * Time.deltaTime)` (assume the static event exists on `Tartaria.Core.GameEvents`)

Visual:
- On Awake, if `transform.childCount == 0`, generate a stretched primitive (Cube)
  rotated 45° on Y, scaled (0.6, 1.8, 0.6), URP/Lit material with `_BaseColor`
  set to one of: (0.45f, 0.15f, 0.6f) violet, (0.7f, 0.2f, 0.5f) magenta-rose,
  (0.2f, 0.4f, 0.7f) deep blue — pick by DissonanceHz value.
- Enable `_EMISSION` keyword, set `_EmissionColor` to `_BaseColor * 1.4f`.
- After `Cleanse()`, change emission to `Color.white * 0.6f`.

## Reference excerpt — mimic this URP shader pattern from existing code

```csharp
var urpLit = Shader.Find("Universal Render Pipeline/Lit");
if (urpLit != null)
{
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", new Color(1f, 0.86f, 0.30f));
    mat.EnableKeyword("_EMISSION");
    mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 0.20f) * 1.2f);
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Reference excerpt — namespace + using imports to match

```csharp
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    public class DissonanceCrystal : MonoBehaviour
    {
        // ... your implementation ...
    }
}
```

## Do NOT
- Do not add a `DontDestroyOnLoad` — these are per-scene.
- Do not invent new `GameEvents` channels. If you don't see `RaiseAetherChange`
  in the convention above, fall back to `GameEvents.FireRSChange(-x * 0.1f)`.
- Do not write Editor menu items. Leave that for Claude.
- Do not regenerate any other file (no Moon1*.cs touches).
