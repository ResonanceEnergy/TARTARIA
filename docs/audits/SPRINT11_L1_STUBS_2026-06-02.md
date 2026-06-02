# Sprint 11 — Lane 1 — Stub / TODO / NotImplemented Sweep

**Date:** 2026-06-02
**Branch:** `agent/audit/stub-sweep`
**Worktree base SHA:** `e07660306026c2da2a1c222f26189c99a8fc4a3c`
**Scope:** Every `.cs` file under `Assets/_Project/Scripts/` (excluding `Library/`, `Temp/`, `obj/`).
**Mandate context:** CLAUDE.md "NO STUBS NO PLACEHOLDERS BUILD EVERYTHING OUT" (2026-05-30).

This is a docs-only lane. Code unchanged.

---

## 1. Methodology

Eight passes via `git grep -nE` against the active script tree:

| # | Pattern | Purpose | Raw hits |
|---|---|---|---|
| 1 | `TODO\|FIXME\|HACK\|XXX\|NotImplemented\|throw new System\.NotImplementedException\|// stub\|// placeholder` | Marker-string scan | 72 |
| 2 | `return null; *// .*(stub\|todo\|placeholder\|fix)` | Inline placeholder-returns | 0 |
| 3 | `^\s*\{\s*\}\s*$` then `^\s*\{\s*\}` | Empty-only braces on a line | 0 / 0 |
| 4 | `Debug\.Log.*todo\|Debug\.LogWarning.*todo\|Debug\.LogError.*todo` (case-insensitive) | Log-only stub bodies | 0 |
| 5 | `\) *\{ *\} *$` | Single-line `) { }` empty method bodies | 27 |
| 6 | `Debug\.LogWarning\(.*(not implemented\|stub\|placeholder\|TODO\|coming soon)` | Log-only NotImplemented | 3 |
| 7 | `// (stub\|STUB\|Stub)` | Self-declared stub comments | 12 |
| 8 | `DISABLED:` | Hand-disabled call sites | 18 |

`throw new NotImplementedException` returned **0** hits in the active tree — the project's stub pattern is empty bodies + Debug.Log + `// TODO`, never thrown exceptions. The four `.cs.disabled` files (459 total) were not scanned in detail per regex; flagged below as a class.

Note on regex pass 1: 9 of the 72 hits are NOT TODO/stubs but doc-comments / negated declarations (e.g. `// no TODO bodies`, `// no TODO comments`), and are excluded from the counts below.

---

## 2. Totals by category

| Severity | Count | Description |
|---|---|---|
| 🔴 BLOCKER (Moon 1 happy path placeholder behavior) | **9** | Code that runs in Echohaven and silently no-ops, hardcodes, or refuses to do the gameplay action |
| 🟡 DEFERRED (Phase 2 / Moon 2+ / LiveOps / Steam) | **35** | Stub but not on Moon 1 critical path |
| ⚪ COMMENT-ONLY (marker only, real impl present, or superseded file) | **20** | TODO comment with working code behind it, intentional architectural marker file |
| 📁 Disabled tree (`.cs.disabled`) | **459 files** | Treated as deferred-by-default; not enumerated line-by-line |

Total active `.cs` findings: **64** distinct lines / blocks worth flagging (sum of severities above).

---

## 3. 🔴 BLOCKERS — Moon 1 happy-path placeholders

### B-01. CymaticWaterTuningMiniGame.cs — entire mini-game is empty stubs
**File:** `Assets/_Project/Scripts/Gameplay/CymaticWaterTuningMiniGame.cs:11,33-34,64-69`
**Severity:** 🔴 BLOCKER — Moon 1 spec § 9 calls for 3 Tuning mini-game variants; this is one of them.
**Excerpt (the class doc-comment says it itself):**
```csharp
/// Cymatic Water Tuning Mini-Game (Echohaven Fountain) — Moon 1 vertical slice.
/// Minimal stub for clean Moon 1 build.
...
public void StartMiniGame(float customTime = -1) { _active = true; Debug.Log("[Cymatic] Mini-game started."); }
public void OnTuningInput(float freq, float amp) { }     // empty
public void EnsurePermanentCymaticVisuals() { }          // empty
void PulseFountainCrystals(float strength) { }           // empty
void FinishCymatic() { }                                  // empty
void UpdateAccuracy() { }                                 // empty
void UpdateCymaticPattern() { }                           // empty
void HandleInput() { }                                    // empty
void Update() { if (!_active) return; }                   // no body
```
Self-described "Minimal stub for clean Moon 1 build". 7 empty methods. No accuracy is ever computed (`_bestAccuracy` is set to 0 and never updated), no input is read, no visuals pulse. SaveData round-trips dead numbers. **Direct violation of CLAUDE.md rule #1 + #2.**

### B-02. NavMeshBaker.cs — corrupted source from TODO-merge accident
**File:** `Assets/_Project/Scripts/Integration/NavMeshBaker.cs:15,21-26,38-40`
**Severity:** 🔴 BLOCKER — Moon 1 NPC pathing is gated on a working NavMesh runtime bake path.
**Excerpt:**
```csharp
[SerializeField] private /*NavMeshSurface*/ MonoBehaviour // TODO: Install AI Navigation package /*NavMeshSurface*/ MonoBehaviour // TODO: Install AI Navigation package;
```
A `// TODO` comment that began *inside* the type-substitution `/* */` block has been re-emitted on every Edit-tool pass, doubling each time. The field declaration is malformed. The whole file is gated `#if UNITY_AI_NAVIGATION` so it doesn't break compile when the define is off — but the moment the define is set the file will not parse. Even when the define is off the file has zero runtime contribution → NavMesh has to be hand-baked from the Editor menu, which is the documented Day-2 STATUS.md workaround.

### B-03. Moon1PlayerSetup.cs — `AddComponent<MonoBehaviour>()` corruption
**File:** `Assets/_Project/Scripts/Integration/Moon1PlayerSetup.cs:83,138-148`
**Severity:** 🔴 BLOCKER — this is the canonical Moon 1 player spawner setup file. The camera-controller add-path is silently broken.
**Excerpt:**
```csharp
var movement = playerInstance.GetComponent</* DISABLED: Tartaria.Input.PlayerMovement */ MonoBehaviour>();
...
var cameraController = mainCam.GetComponent</* DISABLED: TartariaCameraController */ MonoBehaviour>();
if (cameraController == null)
{
    cameraController = mainCam.gameObject.AddComponent</* DISABLED: TartariaCameraController */ MonoBehaviour>();
}
```
The `/* */` comments lex to nothing → `GetComponent<MonoBehaviour>()` (finds any MonoBehaviour, not the intended type) and `AddComponent<MonoBehaviour>()` which **throws at runtime** because `MonoBehaviour` is abstract. The fallback `SimpleCameraFollow` saves the player from a black screen, but the intended `TartariaCameraController` is permanently bypassed.

### B-04. PickupInteractable.cs — pickups always fail
**File:** `Assets/_Project/Scripts/Integration/PickupInteractable.cs:55`
**Severity:** 🔴 BLOCKER — this is the universal `IInteractable` that Moon 1 collectibles attach to.
**Excerpt:**
```csharp
bool added = false; // DISABLED: InventorySystem.Instance.Add(itemId, itemCount)
if (added) { _pickedUp = true; ... Destroy(gameObject); }
else { Debug.LogWarning($"[Pickup] Inventory full, couldn't pick up {itemId}"); }
```
`added` is hardcoded `false`, so EVERY pickup logs "Inventory full" and the object is never destroyed. This breaks every loot drop on Moon 1 even when the inventory is empty.

### B-05. AetherFieldSystem.cs — player position hardcoded to spawn fallback
**File:** `Assets/_Project/Scripts/Core/AetherFieldSystem.cs:45-46`
**Severity:** 🔴 BLOCKER — Y-button Aether Vision (per CLAUDE.md F310 map) renders relative to a stale (0,1,0) position.
**Excerpt:**
```csharp
float3 playerApprox = new float3(0f, 1f, 0f); // Bucket 3 fix: was float3.zero
// TODO: when GameLoopController tracks Player transform, switch to live read
```
Source/sink culling and field strength sampling all use the spawn point forever. Walking away from spawn means the Aether grid is cooking from where you started, not where you are. Visible to the player as wrong-looking Vision toggle once they move ~20 m.

### B-06. HUDLiveDataWiring.cs — hardcoded RS=0, Aether=75%
**File:** `Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:50,69-71`
**Severity:** 🔴 BLOCKER — this script is named "LiveData" but the fallback path (which runs every `Update()`) writes placeholders.
**Excerpt:**
```csharp
if (rsCounterText != null)
    rsCounterText.text = "RS: 0"; // Placeholder until ServiceLocator.GameLoop wired
...
aetherMeter.value = 0.75f; // Placeholder
if (aetherText != null) aetherText.text = "Aether: 75%";
```
Even though events fire `UpdateRSCounter` / `UpdateAetherMeter`, the `Update()` polling overwrites them back to "0" / "75 %" the very next frame.

### B-07. PlayerHealthController.cs — SetInvulnerable is a Debug.Log
**File:** `Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:255-260`
**Severity:** 🔴 BLOCKER (cinematic correctness) — cutscenes & raise animation can't grant temporary invuln.
**Excerpt:**
```csharp
public void SetInvulnerable(bool value)
{
    // TODO Phase 2: Implement invulnerability mechanic
    Debug.Log($"[PlayerHealth] Invulnerability: {value}");
}
```
The class has a real `IsInvulnerable` field (set by damage-flash); this PUBLIC setter ignores its argument. Means Moon1CinematicMoments + Moon1NarrativeBeats cannot freeze player damage during the cathedral-raise sequence — if a Mud Golem swings during the cinematic, the player dies.

### B-08. PlayerAbilities.cs — ChannelResonance not wired
**File:** `Assets/_Project/Scripts/Gameplay/PlayerAbilities.cs:49`
**Severity:** 🔴 BLOCKER — Resonance Pulse (A/X button per F310 map) routes through this method.
**Excerpt:**
```csharp
public float ChannelResonance(float amount, float deltaTime)
{
    ...
    // TODO: Integrate with ResonanceScoreSystem (ECS)
    // For now, just return the requested amount capped by channel rate
    return actualAmount;
}
```
The method returns a number but never writes it anywhere — no `GameEvents.FireRSChange`, no ECS write. So the Resonance pool the HUD displays never changes from pulses. Combined with B-06 the HUD is double-broken.

### B-09. BellTowerSyncMiniGame.cs — Moon 1 Bell-tower mini-game has no audio
**File:** `Assets/_Project/Scripts/Gameplay/BellTowerSyncMiniGame.cs:74,103,151,163`
**Severity:** 🔴 BLOCKER (gameplay feel) — the "Sync" mini-game is by definition audio-driven; no bell tone plays.
**Excerpt:**
```csharp
foreach (int bellIndex in _targetSequence)
{
    if (bellIndex < bellButtons.Count)
    {
        HighlightBell(bellIndex);
        // TODO: Add audio service to ServiceLocator
        yield return new WaitForSeconds(0.8f);
    }
}
...
// TODO: PlaySFX("BellTowerSuccess")
...
// TODO: PlaySFX("BellTowerFail")
```
Bell pattern is shown visually only — player can win by memorising the flash sequence but the "sync" framing makes no sense without bell tones. AudioManager exists (see AudioFeedbackController.cs) so this is a 4-line ServiceLocator hookup.

---

## 4. 🟡 DEFERRED (Phase 2 / Moon 2+ / LiveOps / Steam)

Grouped findings — none of these are happy-path Moon 1.

### Phase 2 telemetry (LiveOps)
- `Core/BreadcrumbLogger.cs:67` — `// TODO: Restore after implementing event-driven telemetry (CrashReporter moved to LiveOps)`.
- `Core/FeedbackReporter.cs:205,323,339,355,386` — 5 TODOs around cloud sync + UI notif + disk persistence + Gameplay-abstraction interface.
- `Core/PlayerSentimentTracker.cs:260,268,305,315,329` — 5 TODOs all "Restore after implementing event-driven telemetry".
- `Integration/_STUBS_CrashReporter.cs:31-32` — `SetUserIdentifier`/`SetCustomData` empty bodies. Whole file is `_STUBS_*` namespace.
- `Integration/_STUBS_LiveOpsEventService.cs:27-29` — `TrackEvent`/`TrackPlayerAction`/`TrackPerformanceMetric` empty bodies.
- `Integration/_STUBS_TutorialHookManager.cs:27-28` — `RegisterHook`/`TriggerHook` empty bodies.
*Per CLAUDE.md decision lock: "No Steam achievements/Cloud/cards in Phase 1" → these stay deferred for the LiveOps phase.*

### Steam (excluded from Phase 1 by decision lock)
- `Integration/SteamBridge.cs:48,95` — production stubs guarded by `#if STEAMWORKS` with `_steamAvailable = true; // placeholder until real package` and `// Stub / sim path (used until SDK wired)`.

### Phase 2 dialogue / tutorial / VFX-by-name
- `Integration/DialogueManager.cs:876` — `ShowDialogue(string, string)` Debug.Log fallback. Real path via `UIManager.Instance?.ShowDialogue(...)` is used by all real callers (`AnastasiaController.cs:518`, `DialoguePlayer.cs:208`, `InteractableBuilding.cs:191`, `TartariaLineView.cs:40`). DEFERRED.
- `Integration/TutorialSystem.cs:118,128` — `ForceComplete` + `ResetTutorial` are dev-debug shims; main flow uses event-driven progression already wired in the file.
- `Integration/VFXWiringController.cs:99` — string-keyed `SpawnVFX(name, pos)` is a `Debug.Log` stub. The GameObject-keyed overload is real. Moon 1 callers use the GameObject overload.
- `Integration/Phase2Stubs.cs:15-148` — explicit "Stub MonoBehaviour singletons" file. Contains `CameraShakeController.Shake(...) => Debug.Log(...)`, `AchievementSystem`, `MoonProgressTracker`, `GameLoopController`, `VFXManager`. All exist to satisfy compile dependencies of disabled systems; the `GameLoopController` stub is the only one with real behavior (Moon 1 RS award, fires `GameEvents.FireRSChange`).

### Moon 2+ scope
- `Integration/_moon2_archive/Moon2ProgressionSystem.cs:917` — `// TODO: Grant actual capstone benefits` (Moon 2 archive folder).
- `Integration/RailEscortController.cs:1558` — `// TODO: Update rail network state, spawn effects` (Moon 3 rail puzzle).
- `Integration/Moon1ExcavationSites.cs:133` — `// DISABLED: ExcavationSystem.Instance.RegisterExcavation(siteId);` (cross-Moon system gated off).

### Lower-priority Moon 1 polish
- `AI/EnemyAIController.cs:216` — `// TODO: Add visual VFX (ice particles, blue tint shader)` on frozen state.
- `Core/AddressableAssetLoader.cs:273` — `// Could swap to lower LOD variants` (perf future-work, not a stub).
- `Editor/BlenderImportPostprocessor.cs:30` — `// Sprint 11 TODO: once the Blender NPC scripts emit a real armature` — Editor pipeline note.
- `Editor/Moon1WireSpawnerPrefabs.cs:342` — Arrow stays canonical pending Blender Arrow bundle.
- `Editor/Moon1BuildOutNPCs.cs:14,63,89` — Lirael Day-25 gate hook waits on `GameEvents.OnDayChanged`. Note this is documented in Lirael spawning logic — not a true stub, an explicit gate.
- `Gameplay/DayNightController.cs:123` and `Integration/DayNightCycleController.cs:123` — both have `// TODO: Wire to AetherFieldSystem` for the night-aether boost. Boost is computed and stored in `AetherYieldMultiplier` static; ExcavationSystem reads it. Marked 🟡 since the wire-through exists.
- `Gameplay/PlayerHealthController.cs:109` — `// TODO: Re-enable when ability system restored` for shield mitigation. Damage still applies; just no shield reduction.
- `Integration/PickupInteractable.cs:63,70` — sibling TODOs to B-04 (VFX + UI text). Not in B-04's blocker because they're additive polish.
- `Integration/PostProcessingSetup.cs:9` — class doc-comment "TODO from REALITY_CHECK Phase 2"; real impl is present.
- `Integration/AudioFeedbackController.cs:9`, `Integration/InventorySystem.cs:10`, `Integration/DayNightCycleController.cs:8`, `Integration/VFXWiringController.cs:8`, `UI/HUDLiveDataWiring.cs:11` — same "TODO from REALITY_CHECK Phase 2" doc-header marker; real impl present in all five.
- `Integration/QuestManager.cs:315` — `// TODO: Enable when TutorialSystem is active` — gated activation.
- `Integration/QuestLogUIPanel.cs:84,305` — placeholder render when QuestManager absent (`Debug.LogWarning` + `// TODO: Implementation pending`) — non-Moon-1 quest detail panel.
- `Data/Query/DataRegistry.cs:270` — `// TODO: Store extractors for efficient updates` (perf optimisation note).
- `Localization/LocalizationManager.cs:200-201` — `STUB: Static accessor` + `TODO: Migrate callers` (backward-compat shim, real path wired).
- `Save/SaveManager.cs:383,458,797` — compression + Agent-9 backward-compat — gated, gameplay save works without.
- `Save/SaveEncryptionHelper.cs:18` — comment about prior TODOs that this file *fills* (i.e. resolved).
- `UI/SaveSlotPanel.cs:257,634` — placeholder render fallback when SaveManager null in Start (defensive).
- `UI/EquipmentUI.cs:316` — `// TODO: Pass candidate item for comparison` — UI polish, equipment stat preview.

### Cross-Moon companion system (DOTS-era)
- `AI/CompanionBehaviorSystem.cs:11-13` — `ApplyPhysicalTellForBeat` body empty, doc says "DEPRECATED: DOTS-era". MonoBehaviour controllers replaced it; no Moon 1 caller invokes this path.

### Core Validation (Phase 5 deferred)
- `Core/Validation/IValidatable.cs:7`, `Core/Validation/ValidationResult.cs:5` — both class doc-comments mark "Stub implementation (Phase 5) — full validation deferred."

### LeanTween (3rd-party)
- `ThirdParty/LeanTween/*.cs` 7 matches — all `{ }` in docstring examples or no-arg ctors of `LTBezierPath`, `LeanAudioOptions`. False positives.

### Note: Phase 2 declared dead
- `Integration/Phase2Stubs.cs:148` — `// Stubs removed 2026-06-01.` is the cleanup marker comment, not a stub.

---

## 5. ⚪ COMMENT-ONLY (markers, doc-comments, intentional supersedes)

Excluded from action items — included only for completeness.

- `AI/MiloTutorialFlow.cs:38` — header comment `// - No TODO bodies, no stubs, no override drivers` (anti-stub claim).
- `Combat/HitFeedback.cs:24` — header comment `// Per CLAUDE.md no-debt mandate: no silent catches, no TODOs`.
- `Editor/Moon1AcceptanceAudit.cs:15` — comment `No placeholder "TODO check this" lines`.
- `Editor/Moon1BlenderPrefabPlacer.cs:17` — comment `no TODO bodies`.
- `Editor/Moon1BuildCreditsScene.cs:20` — comment `no TODO bodies`.
- `Editor/Moon1MasterBootstrap.cs:17-18` — comment listing TWO archived stubs (Moon1MaterialSetup, Moon1AmbientCreatures) that have already been moved to `_deleted_2026_05_31/*.cs.archived`. **CLEAN.**
- `Editor/Moon1PopulateAudioCueLibrary.cs:27` — comment `No "TODO" stubs`.
- `Editor/WirePostRestorationChildren.cs:22` — comment `no TODO comments`.
- `Integration/Moon1ProgressPersistence.cs:15` — comment `no TODO bodies`.
- `Input/PlayerInputHandler.cs:471` — comment `// Stub keeps call site happy for Moon 1 scenes (Echohaven does not require giant flight).` — intentional inert path for a Moon-2 ability.
- `Integration/Moon1GodMode.cs` — 6-line **superseded marker file** (per CLAUDE.md, PlayerInputHandler is canonical).
- `Integration/Moon1HardOverrideDriver.cs` — 3-line **superseded marker file**.
- `Integration/Moon1PostRestorationVisuals.cs:156` — `if (...) { }` is a no-op intentional guard, all real wiring is below.
- `Audio/AudioController.cs:28` — `AudioController() { }` is a singleton-lock private ctor (intentional pattern, not a stub).
- `Gameplay/CymaticWaterTuningMiniGame.cs` `CymaticConfig` factory methods `Default/Easy/Advanced` return `new CymaticConfig()` — these ARE stubs but fold into B-01.

---

## 6. 📁 Disabled files

`Get-ChildItem -Recurse Assets/_Project/Scripts/ -Filter *.cs.disabled` reports **459 files**. By policy these are not on the compile path and per CLAUDE.md "No regeneration of Moon 2–13 stub systems" they stay archived. Not individually enumerated.

The four-file `_moon2_archive/` folder is in this category structurally even though the files have a `.cs` extension — Phase 2Stubs intentionally provides the type shims those files reference.

---

## 7. Top 10 priority fixes

Ranked by Moon 1 blast radius × fix-cost ratio:

| # | File:line | Recommendation |
|---|---|---|
| 1 | `Integration/PickupInteractable.cs:55` | Replace `bool added = false; // DISABLED:...` with `bool added = InventorySystem.Instance != null && InventorySystem.Instance.AddItem(itemId, itemCount);`. **1-line fix unblocks ALL Moon 1 pickups.** |
| 2 | `UI/HUDLiveDataWiring.cs:50,69-71` | Delete the two placeholder string writes in `UpdateAllDisplays()`; let event-driven `UpdateRSCounter`/`UpdateAetherMeter` own those fields. Replace polled aether with `AetherFieldSystem.CurrentEnergyForPlayer()` once #5 lands. |
| 3 | `Gameplay/PlayerAbilities.cs:49` | After `actualAmount` is computed, fire `GameEvents.FireRSChange(actualAmount)` and write to `GameLoopController.Instance?.AwardRS(actualAmount, "channel")`. Lets HUD reflect Resonance Pulse. |
| 4 | `Gameplay/CymaticWaterTuningMiniGame.cs:64-69` | Build the 7 empty methods out: `HandleInput()` reads gamepad L-stick + dpad LR (per CLAUDE.md F310 map), `UpdateCymaticPattern()` drives the fountain shader, `FinishCymatic()` raises `GameEventsPhase2.FireTuningNodeActivated("Fountain")`, `UpdateAccuracy()` writes `_bestAccuracy`. Scope: ~150 LOC. |
| 5 | `Core/AetherFieldSystem.cs:45-46` | Replace hardcoded `(0,1,0)` with `PlayerSpawner.PlayerInstance?.transform.position` (or a `static Transform AetherFieldSystem.PlayerRef`). One inject site, big visual improvement. |
| 6 | `Gameplay/PlayerHealthController.cs:255-260` | Make `SetInvulnerable(value)` set `IsInvulnerable = value` and clear/set `_invulnerabilityEndTime` accordingly. 3-line fix — enables cinematic invuln. |
| 7 | `Integration/Moon1PlayerSetup.cs:83,138-148` | Remove the `/* DISABLED */ MonoBehaviour` placeholders. Either re-enable `TartariaCameraController` (preferred) or commit fully to `SimpleCameraFollow` and delete the dead `AddComponent<MonoBehaviour>()` call. Eliminates a runtime abstract-add exception. |
| 8 | `Integration/NavMeshBaker.cs:15,21-26,38-40` | Either delete the file (Moon1NavMeshBake.cs Editor menu already covers bake) or hard-undo the TODO/comment merge: type should be `NavMeshSurface` from `UnityEngine.AI`, defined-block kept intact. |
| 9 | `Gameplay/BellTowerSyncMiniGame.cs:74,103,151,163` | Replace each `// TODO: PlaySFX(...)` with `Audio.AudioManager.Instance?.PlaySFX2D("BellTone_" + bellIndex)` and the success/fail variants. 4-line wireup. |
| 10 | `Integration/DialogueManager.cs:876` | Either delete the stub overload (callers already use `UIManager.Instance?.ShowDialogue(...)`) or route this method to `UI.UIManager.Instance?.ShowDialogue(characterName, line)`. Removes the Debug.Log-only fallback so future callers don't accidentally route to dev console. |

---

## 8. Findings worth a callout

- **No `throw new NotImplementedException()` anywhere in the active tree.** The project's stub idiom is empty body + Debug.Log + `// TODO`. This is *better* than thrown stubs (no crashes) but *worse* for visibility (silent no-ops).
- **The Moon 1 happy path has more placeholder leakage than expected for an "alpha 0.4" build.** Six of the nine blockers (B-04, B-05, B-06, B-07, B-08, B-09) silently degrade visible Moon 1 systems without throwing — they will pass Editor compile + manual smoke but break automated acceptance audits that test inventory count, HUD readouts, or mini-game completion state.
- **Three files (NavMeshBaker, Moon1PlayerSetup, an Edit-tool artefact pattern) share the same corruption signature**: the `/* DISABLED: X */ Y` substitution where `Y = MonoBehaviour` was a sed-style scripted edit that didn't account for in-progress TODO comments. Worth a Lane 5 sweep to clean across the codebase.
- **`Phase2Stubs.cs` is the canonical "stub-by-design" file.** It is explicitly named, namespaced, and the `Moon1MasterBootstrap` already prunes its components from auto-attach. Leave it alone until Phase 2.
- **Moon1MasterBootstrap.cs's "REMOVED 6 conflicting / stub components" cleanup** is already in effect (archive folder confirms). That work does not need to be redone.

---

*End of audit — Sprint 11 Lane 1.*
