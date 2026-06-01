# TICKET: Moon1InnRestTrigger — "Rest at the Inn" interactable to begin Moon 2

## Output destination
`Assets/_Project/Scripts/Integration/Moon1InnRestTrigger.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- One C# file, one class, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- Uses URP material conventions (`SetColor("_BaseColor", ...)`) for any visual
- Reads `PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) == 1` to gate availability
- Auto-bootstraps in scene via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`

## Spec

A MonoBehaviour that:
1. On bootstrap, creates a GameObject named `Moon1InnRestTrigger` at world position `(10f, 0.5f, 5f)` (in the Echohaven village area, slightly east of spawn)
2. Adds a SphereCollider radius 3.5f, `isTrigger = true`
3. Adds a child visual: `PrimitiveType.Cube` scaled `(1.4, 0.4, 1.4)` with URP/Lit material BaseColor `(0.55, 0.42, 0.28)` (warm wood color), emission enabled with color `(0.95, 0.78, 0.40) * 0.6f` so it glows softly (the "warm hearth" suggestion)
4. The visual gently bobs `transform.position.y` between `0.5f` and `0.7f` with `0.6 Hz` sine
5. Tracks `_playerInRange` via `OnTriggerEnter` / `OnTriggerExit` checking `other.CompareTag("Player")`
6. In `Update`:
   - Skip entirely if PlayerPref `TARTARIA_Moon1Complete != 1` (don't fire prompt, don't accept E)
   - When player enters range AND Moon 1 is complete, call `ServiceLocator.HUD?.ShowInteractionPrompt("[E / A]  Rest at the Inn — begin Moon 2: The Lunar Hour")`
   - When player exits range, call `ServiceLocator.HUD?.HideContextPrompt()`
   - When in range AND `Keyboard.current.eKey.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame`: trigger rest
7. On rest trigger:
   - Show banner `"You rest at the Inn"` / `"Dawn breaks on the Lunar Moon. Lirael waits at the gate."` for 10 seconds
   - Hide the interaction prompt
   - Set `PlayerPrefs.SetInt("TARTARIA_CurrentMoon", 2)` and Save
   - Log `[Moon1InnRestTrigger] Player rested. Moon 1 → Moon 2 transition staged.`
   - DESTROY the visual cube (player has used the inn — Moon 1's instance is consumed)
   - Disable this component (don't re-fire)

The actual scene load to Moon 2 is OUT OF SCOPE for this ticket — Moon 2 scene doesn't exist yet. This ticket just persists the moon counter and stages the narrative beat.

## Reference excerpt — URP material pattern

```csharp
var urpLit = Shader.Find("Universal Render Pipeline/Lit");
if (urpLit != null)
{
    var mat = new Material(urpLit);
    mat.SetColor("_BaseColor", new Color(0.55f, 0.42f, 0.28f));
    mat.EnableKeyword("_EMISSION");
    mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 0.40f) * 0.6f);
    foreach (var r in cube.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
}
```

## Reference excerpt — auto-bootstrap pattern (singleton GameObject in scene)

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Bootstrap()
{
    if (_instance != null) return;
    var go = new GameObject("Moon1InnRestTrigger");
    _instance = go.AddComponent<Moon1InnRestTrigger>();
    // NOTE: NOT DontDestroyOnLoad — this is per-scene
}
```

## Reference excerpt — interaction prompt API (already exists on ServiceLocator.HUD)

```csharp
ServiceLocator.HUD?.ShowInteractionPrompt("text");
ServiceLocator.HUD?.HideContextPrompt();
ServiceLocator.HUD?.ShowBanner("title", "body", 8f);
```

## Do NOT
- Do not load a scene with `SceneManager.LoadScene` — Moon 2 doesn't exist yet.
- Do not modify any existing Moon1*.cs file.
- Do not invent new GameEvents channels.
- Do not use `mat.color = ...`. URP requires `SetColor("_BaseColor", ...)`.
- Do not skip the `_playerInRange` check — only handle E inside the trigger.
