# TICKET: Moon1CompletionTracker — fires "Moon 1 Complete" when all 3 hero buildings restored

## Output destination
`Assets/_Project/Scripts/Integration/Moon1CompletionTracker.cs`

## Acceptance criteria
- Namespace: `Tartaria.Integration`
- One C# file, one class, brace-balanced, ends on namespace close `}`
- Uses UTF-8 BOM, CRLF acceptable
- Compiles against Unity 6 LTS with assemblies: `Tartaria.Core` (already references this), `Tartaria.Integration` (this file's home)
- Uses ONLY existing GameEvents channels — do NOT invent new events on `GameEvents` static class
- Must be a MonoBehaviour that auto-bootstraps via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- Listens to `GameEvents.OnBuildingRestoredTyped` (this event already exists — receives `BuildingRestoredEventArgs` with `buildingId`)
- Tracks restorations by `buildingId`; the three Moon 1 hero buildings are `"echohaven_dome"`, `"echohaven_fountain"`, `"echohaven_spire"` (these strings already used by InteractableBuilding instances)
- When all 3 are restored: show a banner via `ServiceLocator.HUD?.ShowBanner(title, body, duration)`, persist a PlayerPref `TARTARIA_Moon1Complete = 1`, fire telemetry log `[Moon1CompletionTracker] MOON 1 COMPLETE — duration: <seconds>s`
- Idempotent — if PlayerPref already set on Awake, don't show banner again

## Spec

```csharp
namespace Tartaria.Integration
{
    public class Moon1CompletionTracker : MonoBehaviour
    {
        const string MOON1_DONE_PREF = "TARTARIA_Moon1Complete";
        static readonly string[] HERO_BUILDINGS = { "echohaven_dome", "echohaven_fountain", "echohaven_spire" };
        
        HashSet<string> _restoredThisSession;
        float _startTimeSec;
        bool _alreadyCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // create the singleton GameObject if not already present this session
            // DontDestroyOnLoad on it
            // mark it tagged so other code can find it via FindWithTag if needed (no tag invention — skip)
        }

        void OnEnable()
        {
            // subscribe to GameEvents.OnBuildingRestoredTyped
            // record _startTimeSec = Time.realtimeSinceStartup
            // _alreadyCompleted = PlayerPrefs.GetInt(MOON1_DONE_PREF, 0) == 1
        }

        void OnDisable()
        {
            // unsubscribe
        }

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            // if _alreadyCompleted return
            // add args.buildingId to _restoredThisSession
            // log: "[Moon1CompletionTracker] Restoration count: {count}/3 — id={args.buildingId}"
            // if all 3 HERO_BUILDINGS present in _restoredThisSession → FireMoonComplete()
        }

        void FireMoonComplete()
        {
            _alreadyCompleted = true;
            float dur = Time.realtimeSinceStartup - _startTimeSec;
            Debug.Log($"[Moon1CompletionTracker] MOON 1 COMPLETE — duration: {dur:F0}s");
            ServiceLocator.HUD?.ShowBanner(
                "MOON 1 COMPLETE",
                "The Listeners' Hall, the Pure Water Font, and the Cosmic Spire are restored. Rest at the Inn to begin the Lunar Moon.",
                12f);
            PlayerPrefs.SetInt(MOON1_DONE_PREF, 1);
            PlayerPrefs.Save();
        }
    }
}
```

## Reference excerpt — auto-bootstrap pattern (from PauseAndGameOverMenu.cs)

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Bootstrap()
{
    if (_instance != null) return;
    var go = new GameObject("Moon1CompletionTracker");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<Moon1CompletionTracker>();
}
```

## Reference excerpt — GameEvents pattern (already exists in GameEvents.cs)

```csharp
public static event Action<BuildingRestoredEventArgs> OnBuildingRestoredTyped;
public static void RaiseBuildingRestored(BuildingRestoredEventArgs args) =>
    OnBuildingRestoredTyped?.Invoke(args);
```

You can read `args.buildingId` and `args.position`.

## Do NOT
- Do not add new fields to `GameEvents` or `BuildingRestoredEventArgs`.
- Do not write Editor menu items (Tartaria menu wiring is my job).
- Do not modify InteractableBuilding.cs or any existing file.
- Do not add `using Tartaria.AI;` — this is in `Tartaria.Integration` asmdef which can't ref AI.
- Do not call `Application.Quit()` or load any scene — Moon 2 transition is a separate ticket.
