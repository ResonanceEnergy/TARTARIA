# Moon 1 — Script-State Audit v2, 2026-05-31 (post-session)

> Re-inventory after the session's 2026-05-31 "fix the messy stuff" pass. Compares against `MOON1_FULL_AUDIT_2026-05-31.md` (v1). Read-only.

## Verification of the 15 session changes

| # | Item | Status | Evidence |
|---|------|--------|----------|
| 1 | `Moon1Lifeline.cs` + `SimplePlayerDriver.cs` archived | **PRESENT** | Both at `Scripts/Input/_archived_bypass_drivers_2026_05_31/*.cs.archived` |
| 2 | `Moon1PlayerSetup.cs` rewritten to find-and-configure | **PRESENT** | 195 lines; comment "PlayerSpawner is the canonical spawner"; calls `GameObject.FindGameObjectWithTag("Player")` not Instantiate (no `Instantiate(` matches) |
| 3 | `Moon1PostProcessing.cs` archived | **PRESENT** | `Scripts/Integration/_archived_duplicates_2026_05_31/Moon1PostProcessing.cs.archived` exists; `Moon1PostProcessingPreset.cs` (44 lines) is the survivor |
| 4 | `Moon1CompletionTracker.cs` archived | **PRESENT** | `Scripts/Integration/_archived_2026_05_31_stub_deletions/Moon1CompletionTracker.cs.archived` |
| 5 | `Moon1LevelBuilder.cs` archived | **PRESENT** | `Scripts/Integration/_deleted_2026_05_31/Moon1LevelBuilder.cs.archived` (+ `.candidate.archived`) |
| 6 | `Moon1FirstTimeHints.cs` fleshed out | **PRESENT** | 201 lines (up from 9); singleton bootstrap, 4 hint keys (MOVE/VISION/E/STRIKE), real HUD wiring |
| 7 | `PointOfInterest.cs` exists | **PRESENT** | `Scripts/Integration/PointOfInterest.cs` 164 lines, `[RequireComponent(typeof(Collider))]`, fires `OnPOIDiscovered` + `FireRSChange` + banner/subtitle |
| 8 | `TartarianHourCycle.cs` with `OnSeventeenthHour` | **PRESENT** | 119 lines, `HOURS_PER_DAY = 17`, rotates Sun_GoldenHour, fires the event |
| 9 | `Moon1SceneCleanup.cs` Editor menu | **PRESENT** | 247 lines, `[MenuItem("Tartaria/8 Fix/Moon 1 Scene Cleanup (Missing Refs + Placeholders)", priority = 805)]` |
| 10 | `Moon1BuildOutVillage.cs` loads real Blender prefabs | **PRESENT** | 169 lines (close to spec ~160) |
| 11 | `Moon1BuildOutNPCs.cs` with spec positioning | **PRESENT** | 138 lines (spec ~140) |
| 12 | `Moon1BuildOutEnvironment.cs` Overlook + Root Chamber | **PRESENT** | 137 lines (spec ~140) |
| 13 | `GameEvents.cs` has 6 new events | **PRESENT** | `OnPOIDiscovered` L442, `OnSeventeenthHour` L446, `OnTartarianHourChanged` L450, `OnTuningProgress` L454, `FireCombatStarted/Ended` L458-459 |
| 14 | `AdaptiveMusicController` Layer 2 | **PRESENT** | `BindLayer2Events` L431, `GenDiscoveryArpeggio` L516, `GenCombatPercussive` L530, 4 L2 sources + reactive update loop L490 |
| 15 | `SaveData.WorldSaveData` new fields | **PRESENT** | `Scripts/Save/SaveData.cs` L293 `class WorldSaveData` with `discoveredPOIIds` L303, `lastCrossedRSThreshold` L305, `collectedLoreArtifacts` L307, `lastSaveTimestamp` L309 |

**All 15 session changes landed.**

## File-by-file classification

### `Scripts/Integration/Moon1*.cs` + `Echohaven*.cs` (24 files)

| File | Lines | Class |
|---|---:|---|
| `Moon1AnastasiaRocker.cs` | 275 | **REAL** |
| `Moon1AudioAtmosphere.cs` | 282 | **REAL** |
| `Moon1Braziers.cs` | 187 | **REAL** |
| `Moon1BuildingPrefabCreator.cs` | 198 | **REAL** |
| `Moon1CinematicMoments.cs` | 194 | **REAL** |
| `Moon1CombatDirector.cs` | 325 | **REAL** |
| `Moon1DialogueBindings.cs` | 108 | **REAL** |
| `Moon1EnvironmentDetail.cs` | 240 | **REAL** |
| `Moon1ExcavationSites.cs` | 143 | **REAL** |
| `Moon1FirstTimeHints.cs` | 201 | **REAL** (was 9 → rebuilt) |
| `Moon1GodMode.cs` | 6 | **ARCHIVED-in-place** (disabled stub marker, comment says superseded) |
| `Moon1HardOverrideDriver.cs` | 11 | **ARCHIVED-in-place** (superseded note only) |
| `Moon1InnRestTrigger.cs` | 113 | **REAL** |
| `Moon1LightingSetup.cs` | 190 | **REAL** |
| `Moon1MudPoolPuzzle.cs` | 323 | **REAL** |
| `Moon1NarrativeBeats.cs` | 298 | **REAL** |
| `Moon1PlayerSetup.cs` | 195 | **REAL** (rewritten this session) |
| `Moon1PostProcessingPreset.cs` | 44 | **REAL** (survivor; the duplicate `Moon1PostProcessing.cs` is archived) |
| `Moon1ProgressPersistence.cs` | 139 | **REAL** |
| `Moon1QuestTriggers.cs` | 155 | **REAL with 1 TODO** (line 111: NotificationSystem hookup deferred) |
| `Moon1VillagerAmbient.cs` | 219 | **REAL** |
| `EchohavenCombatArena.cs` | 159 | **REAL** |
| `EchohavenContentSpawner.cs` | **3168** | **REAL** — the implementation was moved back out of `Phase2Stubs.cs` (v1 D3 resolved); but it now hosts 8 sub-classes including `MudGolemHealth`, `AetherShardPickup`, `DigSiteInteraction` (see NEW gap N1) |
| `EchohavenObelisk.cs` | 200 | **REAL** |

### `Scripts/Editor/Moon1*.cs` (32 files)

All present and named per the build-out menu chain. New this session: `Moon1BuildOutNPCs.cs`, `Moon1BuildOutEnvironment.cs`, `Moon1SceneCleanup.cs`, `Moon1BuildOutVegetation.cs`, `Moon1BuildOutProps.cs`. The disabled file `Moon1LevelBuilderAutoSetup.cs.disabled` is still parked (intentional).

### `Scripts/Input/` (7 files)

- `PlayerInputHandler.cs` 588 lines — **REAL CANONICAL** (Awake calls `EnsureF310Setup`, runInBackground=true, HandleGamepadButtonFallbacks always runs)
- `LogitechControllerSupport.cs`, `InputProbeHUD.cs`, `InputPromptHelper.cs`, `HapticFeedbackManager.cs`, `RunInBackgroundGuard.cs`, `IInteractable.cs` — all **REAL**
- `SimplePlayerDriver.cs` — **ARCHIVED** (now at `_archived_bypass_drivers_2026_05_31/SimplePlayerDriver.cs.archived`) — matches spec

### `Scripts/AI/MudGolem*.cs` + `ResetScout.cs`

- `MudGolemAI.cs` 612 lines — **REAL**
- `MudGolemHealth.cs` 263 lines (`namespace Tartaria.AI`) — **REAL**
- `MudGolemLootDrop.cs` 83 lines — **REAL**
- `ResetScout.cs` 137 lines — **REAL**
- `_archived_restored_2026_05_31/ResetScout.cs.restored` — **ARCHIVED** (v1 D2 resolved)

## NEW gaps the v1 audit missed

**N1. Duplicate `MudGolemHealth` class across namespaces.** `Tartaria.AI.MudGolemHealth` (263 lines, real combat) AND `Tartaria.Integration.MudGolemHealth` at `EchohavenContentSpawner.cs:2978` (~80 lines, simpler stand-in). Both compile because of distinct namespaces, but `EchohavenContentSpawner.cs` line ~1700-1900 spawns MudGolems and may attach the wrong one. Pick one — recommend deleting the Integration copy.

**N2. `EchohavenContentSpawner.cs` swelled to 3168 lines and now hides 8 unrelated MonoBehaviours.** Including `PerfImpostorBillboard`, `AetherShardPickup`, `BillboardFacer`, `DigSiteInteraction`, `ShovelPickup`, `MiloInteractable`. Discovery via `AssetDatabase.LoadAssetAtPath<MiloInteractable>` works but file-by-name lookups fail. Split into their own files (one per class) — direct violation of the 2026-05-30 mandate's "no hidden classes" spirit even though no `// stub` markers.

**N3. `Phase2Stubs.cs` still has 10 stub classes (254 lines).** v1's D3 only addressed the `EchohavenContentSpawner` migration. Remaining stubs: `CameraShakeController`, `ResonanceScannerSystemStub`, `TutorialSaveData`, `CollectibleEventArgs`, `TuningNodeEventArgs`, `QuestReward`, `ZoneEventArgs`, `TuningNodeEventArgsLegacy`, `GameEventsPhase2`, `LeanTweenStub`. Each violates the 2026-05-30 mandate explicitly: a class file named `Phase2Stubs` is the dictionary definition of "TODO: implement later".

**N4. `Moon1QuestTriggers.cs:111` has a literal `// TODO: Integrate with NotificationSystem when available`.** Real implementation otherwise (155 lines), just one path defers to `Debug.Log`. Cheap to wire to `GameEvents.OnHUDShowBanner`.

**N5. Scene-side missing-script refs cannot be GUID-verified.** `Echohaven_VerticalSlice.unity` is binary YAML (Unity 6 default). GUIDs of the archived scripts (Moon1NPCSpawner `44336fa…`, Moon1AmbientCreatures `3e4118c…`, Moon1MaterialSetup `1e4b384…`, Moon1HeroBuildingSpawner `fe69b6c…`) don't grep-match the binary — could mean the scene was already cleaned OR it just means the binary encoding hides them. **Cannot confirm v1 A1 fully without opening Unity and running `Tartaria → 8 Fix → Moon 1 Scene Cleanup` to inspect.** The cleanup *script* is in place; whether it has been *run on the scene* is unverified.

**N6. `SaveData.cs` lives at `Scripts/Save/SaveData.cs`, not `Scripts/Core/`** as the ticket assumed. The new fields are present; flagging the path mismatch so future work doesn't grep the wrong dir.

**N7. `Moon1GodMode.cs` and `Moon1HardOverrideDriver.cs` are 6 and 11-line "superseded" stubs left in the live folder** (not archived). Compile harmless but violate the "no stub marker files" reading of the mandate. Either move them to `_archived_bypass_drivers_2026_05_31/` alongside their siblings, or delete outright.

**N8. v1 issue A3 (PlayerSpawner vs Moon1PlayerSetup race) is RESOLVED in code** — `Moon1PlayerSetup` no longer spawns, only configures (verified: zero `Instantiate(` calls; only `GameObject.FindGameObjectWithTag("Player")`). PlayerSpawner is now sole owner of spawn.

## Net delta vs v1

**Closed:** v1 A2 (bypass drivers gone), A3 (no double spawn), A4 (PostProcessing dedupe), C7 (FirstTimeHints fleshed; CompletionTracker archived), D1 (Moon1LevelBuilder archived), D2 (.restored files archived), D3-partial (EchohavenContentSpawner is its own file again).

**Still open from v1:** A1 (needs Unity-side scene save), A5 placeholder hero buildings, B1-B6 (village placement, NPC positions, mini-game variants, POI placement, hour cycle wiring), C1-C6, D4 (`_deleted_*` purgatory now intentional/named), D5-D8.

**Newly surfaced:** N1 duplicate MudGolemHealth, N2 EchohavenContentSpawner megafile, N3 Phase2Stubs still 10 stubs, N4 single TODO in QuestTriggers, N7 two 6-11 line marker files in live folder.

---

*Read-only audit. ~970 words. Scene-side state requires Unity Editor to fully verify.*
