# API_CONTRACT.md — Required reading before any code edit
*Every agent in every sprint reads this before writing any line. Director enforces.*

---

## Why this file exists

Sprint-4 shipped 4 regressions I (Cowork co-director) should have caught at the prompt stage:

| Regression | Class | What got past me |
|---|---|---|
| `namespace Tartaria.Core.Time` shadowed `UnityEngine.Time` | Namespace shadow | We'd already fixed this exact bug for `Tartaria.Camera`. I should have banned `Time`, `Input`, `Animation`, `Random` namespace names. |
| `GameEvents.OnQuestActivated` doesn't exist | Invented API | UI agent never grepped GameEvents.cs. Real canonical event is `OnQuestStatusChanged`. |
| `CompressionLevel` ambiguous between `System.IO.Compression` and `UnityEngine` | using-conflict | Editor agent didn't qualify. Always fully-qualify when 2+ assemblies define the same name. |
| `FindObjectOfType<T>()` deprecated in Unity 6 | Wrong-version API | Agent used pre-Unity-6 API. Should be `FindFirstObjectByType<T>(FindObjectsInactive)`. |

This file fixes the *workflow* so the next sprint doesn't ship the same class of regression.

---

## Banned identifiers in `namespace Tartaria.*`

DO NOT create a namespace whose last segment matches a class name in `UnityEngine`. Inside any `Tartaria.X.*` namespace, a bare reference to that name resolves to *your* namespace first, breaking every `UnityEngine.<X>.member` call in the file.

Banned suffixes (do not create `namespace Tartaria.Core.X`, `namespace Tartaria.X`, etc):

- ❌ `Time` — class with `deltaTime`, `time`, `timeScale`, `realtimeSinceStartup`, `unscaledDeltaTime`
- ❌ `Input` — class with `GetKey`, `GetAxis` (legacy) + namespace
- ❌ `Camera` — class with `main`, `transform`, `fieldOfView`
- ❌ `Animation` — class for legacy animation playback
- ❌ `Random` — class with `Range`, `value`, `insideUnitSphere`
- ❌ `Object` — base type for Unity objects
- ❌ `Color` — struct with `red`, `white`
- ❌ `Material`, `Renderer`, `Transform`, `GameObject`, `MonoBehaviour` — all UnityEngine

Allowed alternatives: `Tartaria.Core.GameTime`, `Tartaria.Core.PlayerInput`, `Tartaria.Core.MainCamera`, `Tartaria.Core.AnimationSystem`, `Tartaria.Core.GameRandom`, etc.

If you MUST use a banned name (rare), fully qualify every UnityEngine reference inside that namespace's files: `UnityEngine.Time.deltaTime`, not `Time.deltaTime`. And document the rationale in the file header.

---

## GameEvents API — canonical reference

Before writing ANY `GameEvents.X` line, grep `Assets/_Project/Scripts/Core/GameEvents.cs` for the exact name. Inventing event names is a hard reject.

Current canonical events (verify against GameEvents.cs before each new subscribe):

| Domain | Event | EventArgs / signature |
|---|---|---|
| Building restoration | `OnBuildingRestored` | `Action<string>` (buildingId) |
| Building restoration (typed) | `OnBuildingRestoredTyped` | `Action<BuildingRestoredEventArgs>` |
| Moon completion | `OnMoonCompleted` | `Action<MoonCompletedEventArgs>` |
| Quest lifecycle | `OnQuestStatusChanged` | `Action<QuestStatusChangedEventArgs>` — branch on `newStatus` |
| Quest objective | `OnQuestObjectiveProgressed` | `Action<QuestObjectiveProgressedEventArgs>` |
| HUD banner | `RaiseHUDShowBanner(title, sub, dur)` | method, fan-outs to subscribers |
| HUD interaction prompt | `RaiseHUDShowInteractionPrompt(text)` | method |
| Enemy kill | `OnEnemyKilled` | `Action<EnemyKilledEventArgs>` |
| Hour transition | `OnTartarianHourChanged` (Fire: `FireTartarianHourChanged(int)`) | check before using |
| 17th hour | `OnSeventeenthHour` (Fire: `FireSeventeenthHour()`) | check before using |

There are NO standalone `OnQuestActivated` / `OnQuestCompleted` events. The single `OnQuestStatusChanged` event carries `newStatus = Active` or `Completed`. Branch in your handler.

**Procedure for any UI / Integration agent before writing a subscribe:**
```
1. Grep GameEvents.cs for the domain word ("Quest", "Building", "Moon")
2. Note the EXACT event name + signature
3. If your prompt assumed a different name, STOP and append a HANDOFFS entry instead
```

---

## Unity 6 API replacements (deprecation gates)

Banned obsolete identifiers — using them logs CS0618 every compile. Replace at write-time:

| Banned (Unity 6 deprecated) | Use instead |
|---|---|
| `Object.FindObjectOfType<T>()` | `Object.FindFirstObjectByType<T>(FindObjectsInactive)` |
| `Object.FindObjectsOfType<T>()` | `Object.FindObjectsByType<T>(FindObjectsSortMode.None)` |
| `LightmapEditorSettings.bakeResolution` | `LightingSettings.lightmapResolution` |
| `LightmapEditorSettings.maxAtlasSize` | `LightingSettings.lightmapMaxSize` |
| `LightmapEditorSettings.lightmapper` | `LightingSettings.lightmapper` |
| `LightmapEditorSettings.directSampleCount` | `LightingSettings.directSampleCount` |
| `LightmapEditorSettings.indirectSampleCount` | `LightingSettings.indirectSampleCount` |
| `LightmapEditorSettings.bounces` | `LightingSettings.maxBounces` |
| `LightmapEditorSettings.padding` | `LightingSettings.lightmapPadding` |
| `Lightmapping.giWorkflowMode` | (obsolete, no replacement — remove call) |
| `UnityEditor.AI.NavMeshBuilder` | `UnityEngine.AI.NavMeshBuilder` |

---

## Ambiguous-type qualification rules

When two assemblies define the same simple type name, fully qualify. Known collisions in this project:

| Simple name | Source A | Source B | Required form |
|---|---|---|---|
| `CompressionLevel` | `System.IO.Compression` | `UnityEngine` | `System.IO.Compression.CompressionLevel.Optimal` |
| `Random` | `UnityEngine` | `System` | `UnityEngine.Random.Range` or `using Random = UnityEngine.Random;` |
| `Object` | `UnityEngine` | `System` | usually `UnityEngine.Object` from MonoBehaviour context |
| `Debug` | `UnityEngine` | `System.Diagnostics` | `UnityEngine.Debug.Log` |

When in doubt: fully qualify. Faster than chasing CS0104.

---

## Pre-edit checklist (every agent runs before writing code)

1. **Read the file fully** if editing existing code (rule 11). Adjacent lines often hold the actual root cause.
2. **Grep for any API name you plan to call** before writing the call. If not found, the API doesn't exist — append HANDOFFS, don't invent.
3. **Banned identifier check** — is any namespace name on the banned list above? Is any class name on the deprecated list?
4. **Qualify ambiguous types** per the collision table.
5. **Compile mentally** — would `Library/Bee/tundra.log.json` be clean after my edit? If not, fix before commit.

---

## Post-edit verification (every PR)

Director runs this checklist before merging any agent PR:

- [ ] `tundra.log.json` shows 0 CS errors after Unity reloads the changed assembly
- [ ] No banned namespace name introduced
- [ ] No deprecated Unity 6 API call introduced (CS0618 = reject)
- [ ] Every `GameEvents.X` reference grep-verifies against the actual `GameEvents.cs`
- [ ] No `// TODO` / no empty method bodies / no `catch { }`
- [ ] No new override drivers (rule 2 of no-debt mandate)

If any item fails: reject the PR, append a HANDOFFS entry, re-issue. Do not merge "with follow-up".

---

## Director (Cowork or VS Code) commits the alignment workflow

Before dispatching any multi-agent sprint, the Director:

1. Reads this file
2. Lists each agent's planned API touchpoints in the prompt
3. For each touchpoint, includes an explicit instruction: "grep `<file>` for `<symbol>` and quote the exact signature back to me before writing the subscribe"
4. Bans obsolete identifiers and banned namespaces explicitly in the prompt body

If the Director skips this preflight, regressions ship. Sprint-4 was the cautionary example.

---

*API_CONTRACT.md v1.0 · 2026-06-02 · Read this before EVERY edit. Director enforces. Add new entries when the next regression class surfaces.*
