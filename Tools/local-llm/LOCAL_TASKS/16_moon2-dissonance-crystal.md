# TICKET: Moon 2 DissonanceCrystal — first Moon-2 content scaffolding

## Output destination
`Assets/_Project/Scripts/Gameplay/DissonanceCrystal.cs`

This is the same DissonanceCrystal stub from the EXAMPLE ticket but RE-SPEC'd here as a real ticket so it gets generated in this batch. (The EXAMPLE was just to demonstrate ticket format.)

## Acceptance criteria
- Namespace: `Tartaria.Gameplay`
- Single C# file, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Gameplay`
- URP material conventions: `mat.SetColor("_BaseColor", c)`, NEVER `mat.color = c`
- No references to `Tartaria.AI` from this file
- ANY `GameObject.CreatePrimitive` must have a URP fallback that sets `_BaseColor`
- Do NOT split string literals across lines

## Spec

Per `docs/03_CAMPAIGN_13_MOONS.md` Moon 2 (Lunar / Crystalline Caverns): "Dissonance crystals" are environmental hazards that pulse with off-key frequencies and drain the player's Aether when they get within proximity. The player can tune them via `TuningMiniGame` or destroy them by overlapping with a Lirael harmonic pulse.

Required public API:

```csharp
public class DissonanceCrystal : MonoBehaviour
{
    public float DissonanceHz { get; private set; }    // 666, 777, or 888 picked at Awake
    public float DrainRadius = 4f;
    public float DrainPerSecond = 5f;
    public bool IsCleansed { get; private set; }
    public event System.Action OnCleansed;

    public void Cleanse();   // mark cleansed, fire event, swap emission to white, Destroy gameObject in 3 sec
}
```

Behavior in `Update()`:
- Find Player by tag once (cache transform)
- If `!IsCleansed` and player within `DrainRadius`, fire `GameEvents.FireRSChange(-DrainPerSecond * Time.deltaTime * 0.1f)` to drain RS as proxy for Aether
- Pulse the emission intensity on the crystal renderer with `Mathf.Sin(Time.time * 4f)` so it visibly throbs

Visual in `Awake()`:
- If `transform.childCount == 0`, generate a single primitive Cube
- Rotate 45° on Y, scale `(0.6, 1.8, 0.6)`
- Apply URP/Lit material with `_BaseColor` picked from DissonanceHz:
  - 666 Hz → violet `(0.45, 0.15, 0.6)`
  - 777 Hz → magenta-rose `(0.7, 0.2, 0.5)`
  - 888 Hz → deep blue `(0.2, 0.4, 0.7)`
- Enable `_EMISSION`, set `_EmissionColor = _BaseColor * 1.4f`
- Add a SphereCollider trigger radius `DrainRadius` for proximity check (alternative to Distance check — use either)

After `Cleanse()`:
- Stop pulsing
- Change emission color to `Color.white * 0.6f`
- Schedule `Destroy(gameObject, 3f)`

## Reference excerpt — URP shader pattern

```csharp
var urpLit = Shader.Find("Universal Render Pipeline/Lit");
if (urpLit != null)
{
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", baseColor);
    mat.EnableKeyword("_EMISSION");
    mat.SetColor("_EmissionColor", baseColor * 1.4f);
    foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Reference excerpt — GameEvents.FireRSChange (already exists)

```csharp
// In Tartaria.Core.GameEvents:
public static void FireRSChange(float delta) => OnResonanceChanged?.Invoke(delta);
```

## Do NOT
- Do not add a `DontDestroyOnLoad`.
- Do not invent new GameEvents channels.
- Do not write Editor menu items.
- Do not modify any other file.
- Do not use `mat.color = ...` — URP requires `SetColor("_BaseColor", ...)`.
