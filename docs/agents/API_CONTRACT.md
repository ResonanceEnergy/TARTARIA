# API_CONTRACT.md

> The single source of truth for every API name, banned identifier, and Unity-6 deprecation gate in TARTARIA.
> Every agent (Cowork, VS Code, swarm subagents) MUST read this BEFORE writing a single line of `using` / `subscribe to` / `call` against shared systems.
>
> v2 - 2026-06-02 - Updated with Sprint 6 grep evidence.

---

## How to use this document

1. **Before** you write code that calls a `GameEvents`, `SaveManager`, `ServiceLocator`, or `AudioMixer` API:
   - **grep the canonical source file** for the symbol
   - **quote the file:line** in your PR summary
   - **never invent** an event name based on intuition
2. **Before** you choose a namespace name for new code:
   - check the **banned namespace** list below
3. **Before** you call a Unity Editor / Runtime API older than 2 years:
   - check the **Unity 6 deprecation gates** below
4. **Before** you wrap a method body in `try { ... } catch { ... }`:
   - confirm your catch logs `e.GetType().Name`, `e.Message`, AND the value that broke
   - **no silent catches, ever** (see `NO-DEBT MANDATE` in CLAUDE.md)

---

## 1 - Banned namespace names

These names **shadow `UnityEngine` types** at the language level. Picking any of them creates the kind of `CS0234 / CS0117 / ambiguous-type` cascades that ate hours in sprints 4-5.

| Banned | Why |
|---|---|
| `Tartaria.Time` / `Tartaria.Core.Time` | Shadows `UnityEngine.Time`. Use `Tartaria.Core.GameTime` instead. |
| `Tartaria.Input` | Shadows `UnityEngine.Input`. Use `Tartaria.Core.InputBindings` / `Tartaria.Input` ONLY if it does NOT contain a class named `Input`. |
| `Tartaria.Camera` | Shadows `UnityEngine.Camera`. **Already in use** - every `Camera.main` site in this repo must be `global::UnityEngine.Camera.main`. New code must NOT add types to this namespace. |
| `Tartaria.Animation` | Shadows `UnityEngine.Animation`. Use `Tartaria.Anim` or `Tartaria.AnimationSystem`. |
| `Tartaria.Random` | Shadows `UnityEngine.Random` / `System.Random`. Don't. |
| `Tartaria.Color` | Shadows `UnityEngine.Color`. Don't. |
| `Tartaria.Object` | Shadows `UnityEngine.Object`. Don't. |
| `Tartaria.Debug` | Shadows `UnityEngine.Debug`. Don't. |
| `Tartaria.Mathf` | Shadows `UnityEngine.Mathf`. Don't. |

**Safe namespace roots:** `Tartaria.AI`, `Tartaria.Audio`, `Tartaria.Combat`, `Tartaria.Core`, `Tartaria.Core.GameTime`, `Tartaria.Editor`, `Tartaria.Gameplay`, `Tartaria.Integration`, `Tartaria.Save`, `Tartaria.UI`, `Tartaria.VFX`.

---

## 2 - Canonical GameEvents API (`Assets/_Project/Scripts/Core/GameEvents.cs`)

**RULE:** every `GameEvents.On*` subscription and `GameEvents.Raise*` call must be grep-verified against the file below. If grep returns 0 matches, the event **does not exist** - do not invent one. If you need an event that isn't there, either add it to GameEvents.cs (and update this table in the same PR), or use a static-method direct-call fallback (with a loud log explaining why).

### 2.1 Events confirmed to exist (Sprint 6 grep evidence)

| Event | Type | File:Line | Verified by |
|---|---|---|---|
| `OnBuildingRestored` | `Action<string>` | `GameEvents.cs:56` | Sprint 6 Lane 6 |
| `OnQuestStatusChanged` | `Action<QuestStatusChangedEventArgs>` | `GameEvents.cs:89` | API_CONTRACT v1 |
| `OnPlayerDamaged` | `Action<PlayerDamagedEventArgs>` | `GameEvents.cs:123` | Sprint 6 Lane 5 |
| `OnMoonCompleted` | `Action<MoonCompletedEventArgs>` | `GameEvents.cs:192` | Sprint 6 Lane 9 |
| `OnSeventeenthHour` | `Action` | (verify before use) | - |
| `OnAetherVisionToggled` | `Action<bool>` | (verify before use) | - |
| `RaiseHUDShowBanner(title, sub, dur)` | `void` | `GameEvents.cs:623` | Sprint 6 Lane 6 |
| `RaiseHUDShowDialogue(speaker, msg)` | `void` | `GameEvents.cs:617` | Sprint 6 Lane 6 |
| `RaiseHUDShowInteractionPrompt(msg)` | `void` | `GameEvents.cs:659` | Sprint 6 Lane 6 |
| `RaiseHUDHideInteractionPrompt()` | `void` | `GameEvents.cs:665` | Sprint 6 Lane 6 |
| `RaiseMoonCompleted(MoonCompletedEventArgs)` | `void` | `GameEvents.cs:584` | Sprint 6 Lane 9 |
| `RaiseAetherVisionToggled(bool)` | `void` | `GameEvents.cs:596` | Sprint 5 |
| `OnBrazierLit` | `Action<string>` | `GameEvents.cs:463` | Sprint 9 Lane 7 (audit v2 #8.1) |
| `OnBrazierRingComplete` | `Action` | `GameEvents.cs:467` | Sprint 9 Lane 7 (audit v2 #8.1) |
| `RaiseBrazierLit(string)` | `void` | `GameEvents.cs:464` | Sprint 9 Lane 7 (audit v2 #8.1) |
| `RaiseBrazierRingComplete()` | `void` | `GameEvents.cs:468` | Sprint 9 Lane 7 (audit v2 #8.1) |

### 2.2 EventArgs classes

| Class | File:Line | Fields |
|---|---|---|
| `MoonCompletedEventArgs` | `GameEvents.cs:797-803` | `int moonIndex`, `string moonName`, `int rsReward`, `float completionTime` |
| `QuestStatusChangedEventArgs` | `GameEvents.cs` (search) | `string questId`, `QuestStatus oldStatus`, `QuestStatus newStatus` |
| `PlayerDamagedEventArgs` | `GameEvents.cs` (search) | `float damageAmount`, `Vector3 hitPosition`, `bool isCritical` |

### 2.3 Events that AGENTS INVENTED and DO NOT EXIST

> If you find yourself reaching for these, **stop**. Either reroute to a canonical event below, or add the event to `GameEvents.cs` in your PR and update this table.

| Invented name | Correct alternative |
|---|---|
| `OnEnemyHit` | No equivalent. Use `OnPlayerDamaged` for player-side feedback. Enemy-side: use direct-call pattern - expose `static NotifyHit(Vector3, float, bool)` and log a loud warning the first time it's invoked. |
| `OnEnemyDamaged` | Same as above. |
| `OnDamageDealt` | Same as above. |
| `OnTuneAttemptComplete` | `OnBuildingRestored` (`Action<string>`, fires after restoration succeeds). |
| `OnInteractStart` | Subscribe to the trigger collider directly OR use UI-side state via `RaiseHUDShowInteractionPrompt`. |
| `OnQuestActivated` / `OnQuestCompleted` | `OnQuestStatusChanged`, then branch on `args.newStatus`. |

---

## 3 - Canonical SaveManager API (`Assets/_Project/Scripts/Save/SaveManager.cs`)

| Method / Property | Signature | File:Line | Notes |
|---|---|---|---|
| `Instance` | `static SaveManager` | `SaveManager.cs:36` | Singleton accessor. |
| `CurrentSave` | `SaveData` (property) | `SaveManager.cs:75` | Read-only snapshot of in-memory save. |
| `QuickSave()` | `void` | (grep before use) | F5 hook. |
| `QuickLoad()` | `void` | `SaveManager.cs:246` | F9 hook. **Use this, not `LoadSlot`** - `LoadSlot` does NOT exist. |
| `GetCurrentSlot()` | `int` | `SaveManager.cs:616` | Returns the active slot index. |
| `GetSaveInfo(int slot)` | `SaveSlotInfo` | `SaveManager.cs:654` | Returns metadata for a slot, or null. |
| `SwitchToSlot(int slot)` | `void` | `SaveManager.cs:595` | **The canonical Load API.** Updates paths and reloads via `LoadOrCreate`. |
| `DeleteSlot(int slot)` | `bool` | `SaveManager.cs:693` | Deletes saved data + sidecars. |
| `OnBeforeSave` event | `Action<SaveData>` | `SaveManager.cs:1376` | Fires immediately before write. |
| `OnAfterLoad` event | `Action<SaveData>` | `SaveManager.cs:1381` | Fires after load completes. |

### 3.1 SaveData shape (`Assets/_Project/Scripts/Save/SaveData.cs`)

| Type | File:Line | Notable fields |
|---|---|---|
| `SaveSlotInfo` | `SaveData.cs:930` | Sparse - does NOT include moon name, shards, or buildings restored. If you need those for slot UI, read `CurrentSave.header.currentMoon` / `header.buildingsRestored` / `economy.aetherShards` and persist your own sidecar JSON. |
| `SaveHeader.currentMoon` | `SaveData.cs:234` | Moon index. |
| `SaveHeader.buildingsRestored` | `SaveData.cs:238` | Count. |
| `EconomySaveBlock.aetherShards` | `SaveData.cs:455` | Resonance Shards earned. |

### 3.2 SaveManager events that DO NOT EXIST

| Invented | Alternative |
|---|---|
| `OnSaveComplete` / `OnSaved` | Use `OnBeforeSave` (fires right before) - or poll the file mtime if you genuinely need "after disk write" (log loudly which path you took). |
| `LoadSlot(int)` | **Use `SwitchToSlot(int)`**. The agents who invented `LoadSlot` for Sprint 6 had to refactor. |

---

## 4 - Canonical AudioMixer exposed parameters

**Mixer asset:** `Assets/_Project/Audio/Mixers/MasterMixer.mixer`

| Exposed name | YAML line | Purpose |
|---|---|---|
| `MasterVol` | 110 | Master output trim. |
| `MusicVol` | 112 | Music bus. |
| `SFXVol` | 114 | SFX bus. |
| `UIVol` | 116 | UI sound. |
| `AmbienceVol` | 118 | Ambient zones. |
| `VoiceVol` | 120 | Voice / dialogue. |

> **Conversion:** slider value `v` in [0, 1] -> dB via `Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f`. Pass that to `mixer.SetFloat(exposedName, dB)`.

### 4.1 Wrong names that exist in older code

> `Assets/_Project/Scripts/Audio/AudioMixerController.cs` defaults to `MasterVolume / MusicVolume / SFXVolume` - these **do not match the mixer asset**. Any PR touching that file must fix the defaults to the canonical names above.

---

## 5 - Unity 6 deprecation gates

These APIs were valid in Unity 2022 LTS but are deprecated (`CS0618`) or removed in Unity 6 (6000.x). Replace before commit, not after.

| Deprecated | Replacement | Notes |
|---|---|---|
| `FindObjectOfType<T>()` | `Object.FindFirstObjectByType<T>()` | Pass `FindObjectsInactive.Include` if you need disabled objects. |
| `FindObjectsOfType<T>()` | `Object.FindObjectsByType<T>(FindObjectsSortMode.None)` | Sort mode is required; `None` is fastest. |
| `LightmapEditorSettings.*` | `LightingSettings` ScriptableObject + `Lightmapping.lightingSettings` | Whole new pipeline. See `Editor/Moon1LightingBake.cs` for an in-repo example. |
| `Lightmapping.giWorkflowMode` | `LightingSettings.bakedGI` / `realtimeGI` | Workflow-mode is gone. |
| `UnityEditor.AI.NavMeshBuilder.BuildNavMesh()` | Use `NavMeshSurface` component, OR keep the legacy call wrapped in `#pragma warning disable CS0618 ... restore CS0618` (only if there's no surface in the scene). |
| `UnityEngine.Input.*` | `UnityEngine.InputSystem.Keyboard.current` / `Gamepad.current` / `Mouse.current` | Input System Package is the only supported path in this repo. Mixing legacy `Input.*` will throw at runtime. |
| `UnityEngine.UI.Toggle.isOn` setter inside a layout pass | Use `SetIsOnWithoutNotify(bool)` to avoid recursion. |
| `Application.OpenURL` from Editor | OK at runtime; in Editor use `EditorUtility.OpenWithDefaultApp`. |

---

## 6 - Ambiguous type qualification

When a type name exists in multiple namespaces the project depends on, **fully qualify it at the call site** - do NOT add a `using` that hides which path you took.

| Type | Required form |
|---|---|
| `CompressionLevel` | `System.IO.Compression.CompressionLevel.Optimal` (NOT just `CompressionLevel`) |
| `Random` | `UnityEngine.Random.Range(...)` OR `System.Random` - never bare `Random` |
| `Object` | `UnityEngine.Object` OR `System.Object` - never bare `Object` |
| `Debug` | `UnityEngine.Debug` OR `System.Diagnostics.Debug` - never bare `Debug` |
| `Camera.main` | `global::UnityEngine.Camera.main` (because `Tartaria.Camera` namespace exists and will shadow) |
| `Color` | `global::UnityEngine.Color` if you're inside or near `Tartaria.Color`-adjacent code |
| `TaskCompletionSource` | `System.Threading.Tasks.TaskCompletionSource<T>` (Unity has internal `Tasks` types) |

---

## 7 - Pre-edit checklist

Print this in your head before opening the editor:

```
[ ] Read CLAUDE.md (top mandate stack)
[ ] Read docs/agents/COORDINATION.md (path ownership)
[ ] Open GameEvents.cs and grep for every On*/Raise* I will touch
[ ] Quote file:line in my PR summary for every grep hit
[ ] If grep returns 0, I will NOT invent - I either add the event in the same PR or use a direct-call fallback with loud logging
[ ] My namespace name is not in section 1 banned list
[ ] Every Unity API I call is in the section 5 replacement column (NOT the deprecated column)
[ ] Every catch block in my diff logs e.GetType().Name + e.Message + the offending value
[ ] No method body is just // TODO or Debug.LogWarning("not implemented") and returns nothing
[ ] No primitive GameObject.CreatePrimitive(...) calls outside of explicit URP-safe blocks
```

## 8 - Post-edit checklist (before push)

```
[ ] git status - only files I touched are modified
[ ] tundra.log.json (or your IDE compile log) is clean - no CS0234, CS0117, CS0136, CS0618
[ ] Every event subscription has a paired Unsubscribe in OnDisable / OnDestroy
[ ] Every coroutine has cleanup in OnDisable / OnDestroy
[ ] If I qualified global::UnityEngine.* anywhere, I added an inline comment explaining the namespace shadow
[ ] PR summary cites file:line for every external API I called
[ ] Banned-identifier sweep: grep my diff for the section 1 names - zero hits
[ ] HANDOFFS.md updated if I discovered something the Director needs to act on
```

---

*v2 - 2026-06-02 - Update this file when an agent surfaces a new canonical API location or an invented-name pattern.*
