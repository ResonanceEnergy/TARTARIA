# TICKET: QuestObjectiveTrackerUI — top-right active-objective HUD

## Output destination
`Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs`

## Acceptance criteria
- Namespace: `Tartaria.UI`
- One C# file, brace-balanced
- Compiles against `Tartaria.Core` + `Tartaria.UI`
- Auto-bootstraps via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- Builds its own Canvas + Panel + Text (similar pattern to TuningMiniGame auto-build) so no scene wiring needed
- ALL `Image` components must have `sprite = white1x1Sprite` to render (Unity 6 quirk)
- Canvas sortingOrder = 200 (below tuning UI 32000, above HUD)

## Spec

A minimalist objective tracker pinned to the top-right of the screen, just under the ley-line mini-map. Shows the player's CURRENT primary objective + secondary count.

Visual:
```
┌─────────────────────────────────┐
│ ▶ Restore the Listeners' Hall   │   ← primary objective text (15pt golden)
│   2 / 3 tuning nodes complete   │   ← progress sub-line (11pt gray)
│                                  │
│ + 2 side objectives             │   ← secondary count (11pt gray, only if > 0)
└─────────────────────────────────┘
```

API:

```csharp
namespace Tartaria.UI
{
    public static class QuestObjectiveTracker  // static-facing API
    {
        public static void SetPrimary(string title, string subline = "");
        public static void AddSecondary(string id, string title);
        public static void RemoveSecondary(string id);
        public static void Clear();
    }
}
```

Behind the scenes, a private `QuestObjectiveTrackerUI` MonoBehaviour holds the state and rebuilds the text every frame the dirty flag is set.

### Bootstrap

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void Bootstrap()
{
    if (_instance != null) return;
    var go = new GameObject("QuestObjectiveTrackerUI");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<QuestObjectiveTrackerUI>();
    _instance.BuildCanvas();
}
```

### Canvas build

- Create Canvas at sortingOrder 200, ScreenSpaceOverlay, ScaleWithScreenSize 1920x1080
- Panel anchored top-right: anchorMin/Max (1, 1), pivot (1, 1), anchoredPosition (-20, -120), size (380, 90)
- Background Image: dark brown 80% alpha, **must set sprite to a 1x1 white texture** (see helper below)
- Three Text children stacked vertically:
  - PrimaryText (LegacyRuntime.ttf, 15pt, golden 0.85/0.65/0.10)
  - SublineText (LegacyRuntime.ttf, 11pt, gray 0.75/0.75/0.75)
  - SecondaryCountText (LegacyRuntime.ttf, 11pt, gray)

### White-sprite helper (Unity 6 fix — same as TuningMiniGame)

```csharp
static Sprite _whiteSprite;
static Sprite GetWhite()
{
    if (_whiteSprite != null) return _whiteSprite;
    var tex = Texture2D.whiteTexture;
    _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    return _whiteSprite;
}
```

### Default objective on bootstrap

If `PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) == 0`, show:
- Primary: "Restore the buried buildings"
- Subline: "Find and tune at least one hero structure"

If Moon 1 complete, show:
- Primary: "Rest at the Inn"
- Subline: "Find the warm-glowing platform east of spawn"

### Wire to GameEvents.OnBuildingRestoredTyped

Subscribe to update progress. After each restoration:
- Count current `TARTARIA_Restored_*` PlayerPrefs (if you track per-building)
- OR derive from `Tartaria.Integration.InteractableBuilding.State == Restored` if `FindObjectsOfType<InteractableBuilding>()` is acceptable

## Do NOT
- Don't depend on TextMeshPro — use legacy `UnityEngine.UI.Text` with the built-in font.
- Don't split string literals across lines.
- Don't reference `Tartaria.AI`.
- Don't omit the white-sprite assignment on Image components (will render nothing).
- Don't make Bootstrap re-spawn if `_instance` already exists across scene loads (it's DontDestroyOnLoad).
