# TICKET: Moon1FirstTimeHints — first-time-player tutorial overlay

## Output destination
`Assets/_Project/Scripts/Integration/Moon1FirstTimeHints.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- One C# file, one class, brace-balanced, ends on namespace close `}`
- Compiles against Unity 6 LTS, assemblies `Tartaria.Core` + `Tartaria.Integration`
- Uses ONLY existing APIs: `ServiceLocator.HUD?.ShowBanner(title, body, duration)`, `PlayerPrefs.GetInt/SetInt`, `Keyboard.current.*` (UnityEngine.InputSystem)
- Auto-bootstraps via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- Idempotent per PlayerPref keys — once each hint shown, never shown again
- Skip hints entirely if PlayerPref `TARTARIA_TutorialDone == 1`

## Spec

Show one hint at a time, gated by PlayerPref. The hint sequence:

| Order | Trigger | PlayerPref key | Banner (title, body) |
|---|---|---|---|
| 1 | Scene load + 2.0s delay | `TARTARIA_Hint_Welcome` | "Welcome to Echohaven", "Walk with WASD or the left stick. Look around with the mouse or right stick." |
| 2 | Player has not pressed any movement key for 4s after hint #1 dismissed | `TARTARIA_Hint_Movement` | "Movement", "Try WASD now. The buildings ahead are buried — get close to one." |
| 3 | Player enters any `InteractableBuilding` trigger (detect via `GameEvents.OnBuildingDiscovered`) | `TARTARIA_Hint_Interact` | "Press E", "Press E (or A on gamepad) near a glowing building to begin tuning." |
| 4 | First `OnTuningComplete` event from any building | `TARTARIA_Hint_Restoration` | "First Restoration", "Three nodes per building. Restore all three Moon 1 hero buildings to complete this Moon." |
| 5 | Anytime after hint #4, if RS >= 25 | `TARTARIA_Hint_Combat` | "Mud Golems Awaken", "Restoration draws enemies. Press G or RT for Giant Mode if overwhelmed." |

After hint #5 fires (or any hint with key #5 is set), set `TARTARIA_TutorialDone = 1`.

## Behaviour notes

- Each banner shown via `ServiceLocator.HUD?.ShowBanner(title, body, 8f)` (8 second duration)
- Don't show two hints in the same frame — use a `_lastHintTime` cooldown of 2s
- Read keyboard movement keys: `Keyboard.current.wKey.isPressed || .aKey.isPressed || .sKey.isPressed || .dKey.isPressed`
- Read RS: subscribe to `GameEvents.OnResonanceChanged` if it exists; otherwise track via FindObjectOfType<Tartaria.Core.GameLoopController>() if available, otherwise SKIP hint #5 (it's optional)
- Use a coroutine for the 2s startup delay
- Use a Coroutine for the 4s "no movement detected" check after hint #1

## Reference excerpt — auto-bootstrap

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Bootstrap()
{
    if (_instance != null) return;
    var go = new GameObject("Moon1FirstTimeHints");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<Moon1FirstTimeHints>();
}
```

## Reference excerpt — GameEvents events that exist

```csharp
public static event Action<BuildingDiscoveredEventArgs> OnBuildingDiscovered;
public static event Action<BuildingRestoredEventArgs> OnBuildingRestoredTyped;
// Tuning completion event — check if exists; if not, fall back to OnBuildingRestoredTyped
public static event Action<float> OnTuningComplete; // accuracy 0..1
```

## Do NOT
- Do not write any UI prefab or Canvas — use `ServiceLocator.HUD?.ShowBanner` only.
- Do not regenerate `Moon1NarrativeBeats.cs` or any other Moon1*.cs.
- Do not call `Time.timeScale = 0`.
- Do not reset PlayerPrefs anywhere — only WRITE to them.
- Do not subscribe to events you can't find — if `GameEvents.OnTuningComplete` doesn't exist, skip hint #4 gracefully with `if (typeof(GameEvents).GetEvent("OnTuningComplete") != null)` reflection check.
