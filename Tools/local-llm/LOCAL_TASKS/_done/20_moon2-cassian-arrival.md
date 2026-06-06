# TICKET: Moon2CassianArrival — Cassian NPC + Lirael cavern echo

## Output destination
`Assets/_Project/Scripts/Integration/Moon2CassianArrival.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- One C# file, brace-balanced
- Compiles against `Tartaria.Core` + `Tartaria.Integration`
- Auto-bootstraps via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- Only activates if `PlayerPrefs.GetInt("TARTARIA_CurrentMoon", 1) >= 2`

## Spec

Per `docs/03_CAMPAIGN_13_MOONS.md` Moon 2 Days 1-5 (Discovery beat):
"As the player enters the Crystalline Caverns, Cassian — a cynical resonance engineer in a long coat — descends from a high ledge. Lirael's echo whispers from the cavern walls, fragmented. Cassian offers tactical exposition; Lirael provides emotional hooks."

This MonoBehaviour spawns:
1. **Cassian** at world `(-65, 0.5, -5)` (just inside the cavern entry portal, on a ledge). Uses `Assets/_Project/Prefabs/Characters/Cassian.prefab` if available, otherwise a tall capsule with long-coat tint.
2. **Lirael cavern echo audio** — a procedural 432 Hz haunting hum at world `(-90, 5, 5)` (center of cavern, slightly elevated). Use the same approach as `LiraelLullaby.cs` (already exists in Integration).
3. **Triggers** — proximity radius 8m around Cassian. When player enters:
   - Show banner `"Cassian"` / `"A man in a long coat steps from the shadow. He doesn't smile."` for 6 seconds
   - After 6 seconds, banner `"Cassian"` / `"Resonance engineer. Worked for the Bureau before they Reset us. You'll need me."` for 8 seconds
   - Mark `PlayerPrefs.SetInt("TARTARIA_Moon2_CassianMet", 1)` so dialogue doesn't repeat
4. **Lirael echo trigger** — proximity radius 5m around the cavern center. When player enters:
   - Show banner `"Lirael's echo"` / `"The lullaby... 432 Hz... but broken here. Mixed with something else."` for 7 seconds

## Helper pattern (from existing LiraelLullaby)

```csharp
AudioClip GenerateEchoClip(float baseHz)
{
    const int sr = 44100;
    const float dur = 4f;
    int samples = (int)(sr * dur);
    var clip = AudioClip.Create("Moon2_Echo", samples, 1, sr, false);
    var data = new float[samples];
    for (int i = 0; i < samples; i++)
    {
        float t = (float)i / sr;
        float fundamental = Mathf.Sin(2f * Mathf.PI * baseHz * t);
        // Add a dissonant fifth (out-of-tune) for "broken" feel
        float dissonant = Mathf.Sin(2f * Mathf.PI * (baseHz * 1.51f) * t) * 0.4f;
        float env = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.18f * t);
        data[i] = (fundamental + dissonant) * 0.25f * env;
    }
    clip.SetData(data, 0);
    return clip;
}
```

## Cassian visual (fallback if prefab missing)

```csharp
var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
body.transform.SetParent(cassian.transform);
body.transform.localScale = new Vector3(0.65f, 1.05f, 0.65f);
body.transform.localPosition = new Vector3(0f, 1f, 0f);
Destroy(body.GetComponent<Collider>());
// Long coat — slightly larger capsule wrapping it
var coat = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
coat.transform.SetParent(cassian.transform);
coat.transform.localScale = new Vector3(0.85f, 1.2f, 0.85f);
coat.transform.localPosition = new Vector3(0f, 0.95f, 0f);
Destroy(coat.GetComponent<Collider>());
// Apply URP — coat: dark gray-blue, body: lighter neutral
ApplyURP(body, new Color(0.42f, 0.36f, 0.32f));
ApplyURP(coat, new Color(0.18f, 0.20f, 0.26f));
```

## Do NOT
- Don't load any scene file.
- Don't modify any existing Moon1*.cs or Moon2*.cs file.
- Don't invent new GameEvents channels.
- Don't split string literals across lines.
- Don't use `mat.color = ...` — URP requires `SetColor("_BaseColor", ...)`.
