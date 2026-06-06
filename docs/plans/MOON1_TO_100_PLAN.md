# MOON 1 → 100% PLAN

> Phased roadmap to make Moon 1 actually shippable. Born from Sprint 11's honest audit.
> 2026-06-02 · supersedes "ship-candidate" claims from Sprints 9-10.
>
> **Pre-requisite for every phase:** read the Sprint 11 finding it addresses, cite file:line in your commit message, no invented APIs (per `docs/agents/API_CONTRACT.md`).

---

## Why this plan exists

Sprint 11 demonstrated that prior "shippable" claims measured *file presence* — Sprint 11 measured *runtime feature behavior* via grep-cited audit and the two diverged sharply. This plan is the path from the actual current state to a Moon 1 that a beta tester can complete end-to-end without silent failures.

**Source of truth for the diagnosis:** `docs/audits/SPRINT11_FULL_SCOPE_PUNCHLIST_2026-06-02.md` (synthesis of 9 audit lanes, all on origin with grep-cited findings).

**Estimated total effort:** ~30 focused engineering hours across 7 phases. Each phase ends with a runtime checkpoint — if the checkpoint fails, do NOT proceed.

---

## Phase 0 — Foundation Repair (BLOCKS EVERYTHING)

**Goal:** make the repo trustworthy as a starting point. Without this, every later phase will hit bedrock issues.

**Estimated effort:** 30-45 minutes. **Cannot be parallelized.**

### Steps

1. **`git lfs pull`** — every FBX in the repo is currently a 130-byte LFS pointer. Until pulled, the Blender pipeline, KayKit characters, weapons, environment models — all are inert stubs at runtime. (Sprint 11 L6 `e9bbc612`)

2. **Merge `agent/fix/moon1systems-orphan-deep-clean` (Sprint 11 L5 `9500ccfe`) into feature.** This branch adds `Assets/_Project/Scripts/Editor/Moon1SystemsPrefabDeepClean.cs` which does YAML-surgery on `Echohaven_VerticalSlice.unity` to remove the 4 inline `!u!115 MonoScript` stubs at scene lines 305/1252/3090/3364. Without this menu, `CleanMissingScripts` spam returns every domain reload because `RemoveMonoBehavioursWithMissingScript` can't see inline `!u!115` blocks.

3. **Run `Tartaria/8 Fix/Deep-Clean Moon1_Systems Prefab` once.** Expected output: `scenesTouched=1, yamlBlocksRemoved=8` (4 MonoScript stubs + 4 MonoBehaviour entries). Commit the cleaned scene file.

4. **Verify Sprint 11 audit branches are all reachable on origin** — list them for traceability:
   - `agent/audit/stub-sweep` `1fb03541`
   - `agent/audit/silent-fails` `b33eb621`
   - `agent/audit/commented-code` `1587acab`
   - `agent/audit/workarounds` `f30f9a29`
   - `agent/fix/moon1systems-orphan-deep-clean` `9500ccfe`
   - `agent/audit/prefab-integrity` `e9bbc612`
   - `agent/audit/dialogue-speaker-map` `cec511a9`
   - `agent/audit/scene-authoring` `50ff78ea`
   - `agent/audit/gameevents-pairs` `72457de3`
   - `agent/audit/sprint11-synthesis` `6fe7ffef`

### Runtime checkpoint (Phase 0)

- Open Unity → compile clean.
- Look at the Project window → `Assets/_Project/Models/Blender/Moon1/Lirael.fbx` shows a real preview (not blank). `git lfs ls-files | head` shows files that have been pulled.
- Enter Play → Console no longer spams `[CleanMissingScripts] Removing missing script from: Moon1_Systems`.
- Exit Play. Save scene. Re-enter Play. Spam still gone. (If it returns, the deep-clean menu didn't fully save — re-run with verify-second-pass logging.)

---

## Phase 1 — Critical Code Paths (the "fake feature" fixes)

**Goal:** make Moon 1's happy-path systems actually do what they claim.

**Estimated effort:** 3-4 hours. **Parallelizable: 5 lanes.**

Each lane is a self-contained surgical fix with a grep citation:

### Lane 1.1 — YarnTutorialBinding speaker map (~5 min)
`Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:31-36`

- Replace PascalCase keys with the actual strings MiloTutorialFlow passes (`"Milo"` per `MiloTutorialFlow.cs:78`).
- Replace PascalCase node targets (`Milo_TutorialIntro`) with the actual snake_case Yarn titles (`milo_tutorial_step_1_brazier` etc).
- Add a `(speaker, message) → node` second-level lookup so steps 2-6 can reach their dedicated nodes.

After: tutorial dialogue actually displays.

### Lane 1.2 — PickupInteractable (~5 min)
`Assets/_Project/Scripts/Integration/PickupInteractable.cs:55`

- Uncomment `bool added = InventorySystem.Instance.Add(...)`. If `InventorySystem.Instance` isn't accessible from this asmdef, add an `event Action<PickupData> OnPickedUp` on PickupInteractable and have InventorySystem subscribe — no static singleton dependency leak.
- Destroy the pickup GameObject only on success.

After: collectibles actually enter the inventory.

### Lane 1.3 — PlayerAbility RS economy (~30 min)
`Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:101,142` + `Assets/_Project/Scripts/Integration/PlayerAbilityManager.cs:116,133` + `Assets/_Project/Scripts/Gameplay/PlayerAbilities.cs:49`

- The combat-cost calls are commented with "Note: ConsumeRS() API pending." Either implement `ResonanceScoreTracker.ConsumeRS(float)` + `HasRS(float)` (canonical fix) OR call the existing `GameEvents.FireRSChange(-cost)` if that's the canonical path. Grep `ResonanceScoreTracker.cs` for the actual public API before invoking.
- PlayerAbilities.ChannelResonance computes `actualAmount` and discards it — fire the RS change event.

After: Harmonic Strike + Frequency Shield + Resonance Pulse actually cost RS.

### Lane 1.4 — HUDLiveDataWiring stop overwriting events (~15 min)
`Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:50,69-71`

- Delete the `Update()` fallback that overwrites event-driven HUD values with hardcoded `RS:0 / Aether:75%`.
- If the HUD needs a "no data" state, render an empty/dim variant — do not lie with a hardcoded number.

After: HUD values actually reflect game state.

### Lane 1.5 — CymaticWaterTuningMiniGame implementation (~2 hours)
`Assets/_Project/Scripts/Gameplay/CymaticWaterTuningMiniGame.cs:33-69`

- 7 empty methods need real bodies: `OnTuningInput`, `EnsurePermanentCymaticVisuals`, `PulseFountainCrystals`, `FinishCymatic`, `UpdateAccuracy`, `UpdateCymaticPattern`, `HandleInput`.
- Per docs/15 §9, this is the Variant 2 (Cymatic Water) mini-game. Pattern after the existing TuningVariantC (Harmonic Pattern) — same `TuningMiniGameBase` interface, same `OnFrequencyChanged` event, same accuracy tier ladder.
- Wire to Building_fountain (Sprint 8 L4 routed fountain→CymaticWater).

After: the fountain has a real tuning mini-game.

### Runtime checkpoint (Phase 1)

- Hit Play → Milo greets, his line appears in the dialogue UI.
- Walk to a brazier → press E → flame lights, "+1 RS" floats up, HUD value increments.
- Walk to a building → press E → mini-game launches, completing it triggers building restoration.
- Save with F5 → quit → restart → Continue → state restored.

---

## Phase 2 — GameEvents Wiring (close the 25 broken pairs)

**Goal:** every event in GameEvents.cs has a real publisher AND a real subscriber.

**Estimated effort:** 2-3 hours. **Parallelizable: 4 lanes.**

### Lane 2.1 — Add missing canonical events (~15 min)
`Assets/_Project/Scripts/Core/GameEvents.cs`

- Add `public static Action<int> OnDayChanged;` + `RaiseDayChanged(int)`
- Add `public static Action<string> OnBrazierLit;` + `RaiseBrazierLit(string)`
- Add `public static Action OnBrazierRingComplete;` + `RaiseBrazierRingComplete()`

These were claimed in CLAUDE.md canonical-facts table but never declared (Sprint 11 L9 finding). Subscribers wait forever.

### Lane 2.2 — Wire OnPlayerHealthChanged / OnAetherEnergyChanged publishers (~30 min)
- `Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs` — every damage/heal site fires `GameEvents.RaisePlayerHealthChanged(newHP, maxHP)`.
- `Assets/_Project/Scripts/Gameplay/AetherEnergyController.cs` — drain/restore fires `GameEvents.RaiseAetherEnergyChanged(newE, maxE)`.

After: HUD health + aether bars actually move.

### Lane 2.3 — Wire OnPlayerDeath / OnPlayerRespawned subscribers (~30 min)
- `Assets/_Project/Scripts/UI/HUDController.cs` — `OnPlayerDeath` subscriber shows death overlay; `OnPlayerRespawned` clears it.
- `Assets/_Project/Scripts/Camera/CameraController.cs` — `OnPlayerDeath` fades to grey; `OnPlayerRespawned` restores.
- `Assets/_Project/Scripts/Input/PlayerInputHandler.cs` — `OnPlayerDeath` disables input; `OnPlayerRespawned` re-enables.

### Lane 2.4 — Boss encounter UI publishers (~1 hour)
8 events fire into void: `OnHUDShowMoonTrophy`, `OnHUDShowBossNameplate`, `OnHUDShowBossHealth`, `OnHUDHideBossHealth`, `OnHUDUpdateBossHealth`, `OnHUDShowCorruptionWhisper`, `OnHUDShowEnemyBark`, `OnHUDFlashRSGain`. Wire publishers in `BossEncounterSystem.cs` + `MudGolemAI.cs`. Defer if Moon 1 boss not in scope; mark as `// Sprint 12+` in code.

### Runtime checkpoint (Phase 2)

- Take damage → HUD health bar moves. Drain aether → bar moves.
- Die → death overlay shows, input disabled. Respawn → cleared, input restored.
- Tick day cycle → `[Moon1LiraelDay25Gate] day=N` log appears each day.

---

## Phase 3 — Silent Fail Sweep (close the 38 empty catches)

**Goal:** every exception either does something meaningful OR logs loudly with context.

**Estimated effort:** 2-3 hours. **Parallelizable: 5 lanes by file.**

### Top 5 Moon-1-touching first (per Sprint 11 L2):

1. `Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:24,25,43,44,75,76` — 6 empty catches around `OnSeventeenthHour` subscribe + cathedral eruption + skeleton-key beats. Each catch should log `[Moon1NarrativeBeats] subscribe failed: {ex.GetType().Name}: {ex.Message}` and report which beat is now silently dead.
2. `Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs:32-33,38-39` — restoration dolly camera + 17th-hour cinematic. Same fix pattern.
3. `Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:28,35,66` — "Press E to tune" prompt + tuning objective HUD. Silent catches here mean the mini-game gate fails invisibly.
4. `Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs:33-34,39-40` — canonical quest tracker.
5. `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:431-435,442-446` — 10 catches gate combat/tuning music layer transitions.

### General pattern (CLAUDE.md NO-DEBT mandate)

```csharp
try { dangerousCall(); }
catch (Exception ex)
{
    Debug.LogError($"[{nameof(MyClass)}] {nameof(method)} failed: {ex.GetType().Name}: {ex.Message}\n  context: {key}={value}\n{ex.StackTrace}");
    // Do NOT silently swallow. Either rethrow OR fall through with a documented fallback.
}
```

### Runtime checkpoint (Phase 3)

- Walk through the happy path again. Look at the console. **No exception is silently swallowed.** Any logs that appear name the system + the value that broke.

---

## Phase 4 — Workaround Removal (root-cause fixes)

**Goal:** delete the override/insurance/emergency-bypass files and fix the actual scene/prefab gaps they paper over.

**Estimated effort:** 3-4 hours. **Mostly serial — each deletion requires the underlying fix first.**

### Lane 4.1 — Bake PlayerSpawner + BuildingSpawner into the scene
- Open `Echohaven_VerticalSlice.unity`. Add empty GameObjects "PlayerSpawner" + "BuildingSpawner" with their respective components.
- Save scene.
- Delete `Assets/_Project/Scripts/Integration/RuntimeSpawnerInsurance.cs` entirely.

### Lane 4.2 — Fix Player.prefab (real, not capsule)
- Open `Assets/_Project/Prefabs/Characters/Player.prefab` for editing.
- Add: PlayerInputHandler, CharacterController, Animator, NavMeshAgent, MeshRenderer with URP/Lit material.
- If using Blender FBX: set the FBX as the model source, prefab variant on top.
- Remove the runtime AddComponent block in `PlayerSpawner.SpawnPlayer()` lines 118-138.

### Lane 4.3 — Delete Moon1RescueDriver + restore PlayerInputHandler gate
- Delete `Assets/_Project/Scripts/Editor/Moon1SceneRescue.cs` lines 184-187 + 249-272 (the `Moon1RescueDriver` component + AddComponent call).
- Restore `Assets/_Project/Scripts/Input/PlayerInputHandler.cs:215-217` — uncomment the `GameStateManager.IsPlaying` gate. Player should NOT walk during cutscenes/dialog/pause/dead.

### Lane 4.4 — ResetScout real visual
- `Assets/_Project/Scripts/AI/ResetScout.cs:34-39,44+` — replace `GameObject.CreatePrimitive(Capsule)` with `Resources.Load<GameObject>("Characters/ResetScout")` or the Blender FBX variant.

### Lane 4.5 — Stubs flagged DELETE
- `Assets/_Project/Scripts/Editor/Moon1GodMode.cs` — 3-line stub. Delete the file + remove asmdef references.
- `Assets/_Project/Scripts/Editor/Moon1HardOverrideDriver.cs` — 5-line stub. Delete.
- `Assets/_Project/Scripts/Editor/Moon1AutoWire.cs` — superseded menu. Delete.

### Runtime checkpoint (Phase 4)

- Open scene. Inspector shows `PlayerSpawner` and `BuildingSpawner` GameObjects with their components — not creating at runtime.
- Hit Play. Console has zero `[RuntimeInsurance]` warnings.
- During a dialogue scene, WASD does NOT move the player.
- `git grep -nE "Insurance|RescueDriver|GodMode|HardOverride|EmergencyBypass"` in `Assets/_Project/Scripts/` returns minimal results.

---

## Phase 5 — Prefab Restoration (the visible quality)

**Goal:** every prefab in `Assets/_Project/Prefabs/Moon1/` and `Characters/` is healthy (real mesh, real material, real scripts).

**Estimated effort:** 6-8 hours. **Parallelizable: prefab-per-lane.**

### Prerequisites
- Phase 0 complete (`git lfs pull` done — FBX files have real content).
- The Sprint 9 Lane 6 `Moon1RebindNPCPrefabs.cs` Editor menu, OR the Sprint 10 Lane 6 `BlenderImportPostprocessor` configuration.

### Lane 5.1 — NPC humanoid models (Lirael, Anastasia, Cassian, Milo)
- Run Tartaria/Content/Reimport Moon 1 NPC FBX (Sprint 10 L6 menu) — sets ModelImporter to Generic, scale 1.0, axis convert.
- For each NPC prefab variant: ensure it points at the Blender FBX (not capsule), has CapsuleCollider, NavMeshAgent, the relevant Controller script (LiraelController, AnastasiaController, etc.).
- Smoke-test: in scene, the NPC stands up at the right scale, not in T-pose at scale 0.01.

### Lane 5.2 — Cathedral kit material GUID corruption (~20 min)
- 18 prefabs under `Assets/_Project/Prefabs/Moon1/Cathedral/*.prefab` share an invalid 16-char material GUID `d4f8e2c9a7b3f5e1`. Unity expects 32-char GUIDs — these resolve to magenta.
- Either: regenerate the material (`Tartaria/4 Generate Art/Cathedral Materials` Editor menu, if it exists; else write one), OR globally find-replace the bad GUID in the YAML with a real one and re-import.

### Lane 5.3 — Echohaven hero buildings real meshes
- `Echohaven_Cathedral.prefab`, `Echohaven_StarDome.prefab`, `Echohaven_CrystalSpire.prefab` — currently composed of 60+ Unity primitive `Detail_*` children. Replace with real KayKit kit or Blender procedural-from-script result.

### Lane 5.4 — Player.prefab full bake (already partial in Phase 4)
- Final pass: check the Player prefab has every component a Player should have at edit time, no runtime AddComponent fallbacks needed.

### Runtime checkpoint (Phase 5)

- Walk the scene. No T-poses. No magenta surfaces. NPCs animate idle loops. Cathedral has visible architectural geometry, not 60 stacked cubes.

---

## Phase 6 — Scene Authoring (bake the runtime gen)

**Goal:** the saved scene file contains what gets played. No more "boot chain creates 134 GameObjects at Play time."

**Estimated effort:** 4-5 hours. **Serial — each bake step depends on the prior state.**

### Per Sprint 11 L8 top offenders

### Lane 6.1 — RuntimeHUDBuilder → prefab
- `RuntimeHUDBuilder.cs` builds the entire HUD (Canvas, EventSystem, HUDController, ControlsHint, QuestToast, DialoguePanel) from raw `new GameObject` calls.
- Author `Assets/_Project/Prefabs/UI/HUD_Root.prefab` once. Replace RuntimeHUDBuilder with a stub that just Instantiates the prefab if not present.

### Lane 6.2 — EchohavenContentSpawner.SpawnMilo / villagers → scene
- 70+ procedural `new GameObject(...)` calls in `EchohavenContentSpawner.cs:178+` create primitive Milo + villagers.
- KayKit prefabs exist for these (Phase 5 wired them). Instantiate the prefab variants in the scene at edit time via a "Bake Echohaven Content" Editor menu, save scene. Delete the runtime spawn paths.

### Lane 6.3 — BuildingSpawner.BuildStarDome → prefab variant
- `BuildingSpawner.cs:344-481` does 30+ runtime Instantiate calls for walls/floors/pillars/torches.
- Compose once as `Echohaven_StarDome_Built.prefab` (the post-restoration state). Toggle visibility based on `OnBuildingRestored` event.

### Lane 6.4 — Moon1AnastasiaRocker procedural → real prefab
- `Moon1AnastasiaRocker.cs:32-166` builds Anastasia + rocking chair + trigger + HumSource from primitives every Play.
- Author once as a prefab. Place in scene. Delete the runtime build path.

### Lane 6.5 — Consolidate competing Master menus
- Three "0 ★ MASTER" menus with overlapping scopes: `Bootstrap All Moon 1 Systems` ≠ `Tier 1` ≠ `Run ALL Tiers`.
- Decide canonical bootstrap = the one that boots a fresh play session correctly. Delete the others. Update docs.

### Runtime checkpoint (Phase 6)

- Hit Play on a fresh scene load (no Bootstrap menus pre-run). Scene contents are visible BEFORE Play (in the Hierarchy at edit time).
- Boot chain `[RuntimeInitializeOnLoadMethod]` logs are dramatically fewer.
- The scene file size grew (you baked in content) — that's correct.

---

## Phase 7 — Validation + Beta Build

**Goal:** prove Moon 1 actually works end-to-end, then beta-tester drop.

**Estimated effort:** 2-3 hours.

### Lane 7.1 — Sprint 12 acceptance audit
- Re-run the Sprint 11 audit lanes against the post-fix branch. Compare to Sprint 11 deltas.
- New audit doc: `docs/audits/MOON1_ACCEPTANCE_SPRINT12_<date>.md`.

### Lane 7.2 — End-to-end happy-path walk (Cowork-driven via Unity MCP bridge)
- Boot main menu → New Game → spawn → Milo dialogue plays (verify line appears) → walk → light 3 braziers → tune 3 buildings → save → quit → reload → continue → finish.
- Console at end: 0 errors, 0 silent catches reached, 0 missing-script warnings.

### Lane 7.3 — Win64 build via `Moon1ItchBuild.BuildWin64`
- Produces `Builds/itch_assets/TARTARIA_Moon1.zip`. Smoke-test via Sprint 7 Lane 9 `itch-smoke-test.ps1`.

### Lane 7.4 — Beta drop
- Distribute zip via Discord or Drive (NOT itch.io until artist sign-off per CLAUDE.md callout).
- Testers use `docs/release/BETA_TEST_GUIDE.md` + `BETA_FEEDBACK_TEMPLATE.md` (already in repo).

### Runtime checkpoint (Phase 7)

- Beta build runs on a clean Windows 10/11 machine.
- 3 external testers complete Moon 1 happy path.
- Bug reports filed against the BETA_FEEDBACK_TEMPLATE.

**Then and only then, the `moon1-ship-candidate` tag is meaningful.**

---

## Cross-cutting rules (apply to every phase)

1. **Per-phase runtime checkpoint is mandatory.** If the checkpoint fails, fix that phase before starting the next.
2. **Worktree mandate** — every parallel lane gets its own worktree via `git worktree add C:\dev\_wt_pX_lY_<short> -b agent/<lane> origin/<base>`.
3. **API_CONTRACT** — grep every external API before invoking. No invented event names. Cite file:line in PR.
4. **NO silent catches.** Every catch logs `e.GetType().Name`, `e.Message`, and the offending value.
5. **NO new workarounds.** If you find yourself writing `*Insurance` or `*Override` or `*Bypass`, stop and fix the root cause instead.
6. **NO new audit lanes.** Sprint 11's 10-lane audit is the canonical state baseline. Subsequent sprints fix, then Phase 7's Sprint 12 audit re-measures.
7. **Each phase commits to its own branch.** No mega-commits. Each commit lists the Sprint 11 audit finding it addresses.

---

## Estimates summary

| Phase | Description | Effort | Parallelizable? |
|---|---|---|---|
| 0 | Foundation Repair | 30-45 min | No |
| 1 | Critical Code Paths | 3-4 hr | Yes (5 lanes) |
| 2 | GameEvents Wiring | 2-3 hr | Yes (4 lanes) |
| 3 | Silent Fail Sweep | 2-3 hr | Yes (5 lanes) |
| 4 | Workaround Removal | 3-4 hr | Partial |
| 5 | Prefab Restoration | 6-8 hr | Yes (per-prefab) |
| 6 | Scene Authoring | 4-5 hr | No (sequential) |
| 7 | Validation + Beta | 2-3 hr | No |
| **Total** | **22-35 focused engineering hours** | | |

If lanes within phases run in parallel (per the WORKTREE_MANDATE), wall-clock time compresses to ~12-15 hours.

---

## Anti-patterns observed in prior sprints (do not repeat)

- ❌ Audits that only measure file presence — Sprint 11 demonstrated this gives false-positive "shippable" verdicts.
- ❌ Multiple competing bootstrap menus — `0 ★ MASTER / Bootstrap All / Tier 1 / Run ALL Tiers` overlap with no single canonical entry.
- ❌ Runtime AddComponent rescue patterns — papers over missing prefab/scene authoring.
- ❌ EmergencyBypass / GodMode / Override drivers — bypass canonical input/spawn paths.
- ❌ Inline `!u!115 MonoScript` stubs left in scene YAML after class deletion — orphan persistence.
- ❌ PascalCase Yarn node references when actual nodes are snake_case.
- ❌ Catch blocks that swallow exceptions and silently fall through.

---

*v1.0 · 2026-06-02 · Sprint 11 close. Future Claude sessions: read this BEFORE writing any Moon 1 fix.*
