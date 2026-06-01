# Moon 1 — Full Top-to-Bottom Audit, 2026-05-31

> NATRIX walked Moon 1 after the input fix and reported it's messy. This is the complete top-to-bottom audit: spec vs scripts vs scene vs prefabs vs assets, with every gap ranked by impact.

Sources: parallel agent passes against `docs/15_MVP_BUILD_SPEC.md` §§7-15, `docs/03_CAMPAIGN_13_MOONS.md` Moon 1, `docs/03C_MOON_MECHANICS_DETAILED.md`, the 4 prior audit reports in `docs/audits/`, every `Moon1*.cs` runtime + editor file (~60 files), the Echohaven scene file, the prefab and asset trees.

---

## Executive summary — what NATRIX is seeing

| Category | Status | Headline |
|---|---|---|
| Hero buildings (3) | **DUPLICATE IN SCENE** | Both real (`Building_echohaven_*`) AND placeholder (`*_Placeholder`) versions present |
| Village buildings (9-11) | **AUTHORED, NOT PLACED** | All 11 Blender prefabs exist on disk; zero placed in the scene |
| NPCs (Milo / Anastasia / Lirael / Cassian / Bob / villagers) | Placed | But Anastasia at Cathedral, not at Spire per spec |
| Mini-game (3 variants) | Variant A only | Variants B (Waveform Trace) and C (Harmonic Pattern) wired in code but no in-scene pedestals |
| Enemies (MudGolem, ResetScout) | Spawned | But fighting magenta primitive fallbacks in some paths |
| Input | **WORKING** as of this session | Was blocked by Console Error Pause toggle, now fixed |
| Console errors at scene init | **2-4 missing scripts on `Moon1_Systems`** | These trigger the Error Pause stall |
| Player driver | **3-WAY CONFLICT** | `Moon1Lifeline` kills `SimplePlayerDriver` kills `PlayerInputHandler` — canonical (per CLAUDE.md) is dead code |
| Audio | 3 lore stingers present | But at `Audio/Moon1_Lore/`, not `Audio/Moon1/` — wiring may miss them |
| VFX | 4 climactic prefabs present | Spawning + triggering paths exist; not visually verified post-Error-Pause fix |
| Save / progression | Partial (PlayerPrefs only) | Spec wants full JSON SaveData with versioning + cloud-ready |

---

## TIER A — Playability blockers (fix first)

These are the things actively breaking the game right now, not "improvements".

### A1. Echohaven_VerticalSlice.unity has 4 missing script references on `Moon1_Systems`

The scene file references `Tartaria.Integration.Moon1NPCSpawner`, `Moon1AmbientCreatures`, `Moon1MaterialSetup`, `Moon1HeroBuildingSpawner` as components on `Moon1_Systems`, but these classes were **archived to `Scripts/Integration/_deleted_2026_05_31/`** during the cleanup pass. The MonoBehaviour records are now orphans.

**Impact:** Every Play session throws `Debug.LogError("The referenced script (Tartaria.Integration.…) on this Behaviour is missing!")`. When the Console's "Error Pause" toggle is ON (Unity 6 default), Unity auto-pauses the Editor at frame 1 of Play. **This is the root cause of the multi-hour "no input" stall we just resolved.**

**Fix:** Open scene in Editor, run `Tartaria → 8 Fix → CleanMissingScripts` (or the Unity built-in `GameObject → Remove Missing Scripts` on Moon1_Systems). Save scene. Commit.

### A2. Three input drivers killing each other — canonical PlayerInputHandler is dead code

CLAUDE.md states PlayerInputHandler is the canonical input driver. In practice:

- `Moon1Lifeline` (DefaultExecutionOrder -32000) runs FIRST. In Start: `Destroy(__SimplePlayerDriver)`, disables every MonoBehaviour on the Player except CharacterController. **PlayerInputHandler is dead.**
- `SimplePlayerDriver` (-9999) bootstraps a separate GameObject `__SimplePlayerDriver`. In its own Start: disables PlayerInputHandler + Moon1HardOverrideDriver. But Lifeline already destroyed it.
- `PlayerInputHandler` (default 0): never runs because both bypass drivers disable it in their Start methods.

**Impact:** All input goes through Lifeline (which I wrote two hours ago as a diagnostic). The canonical input mapping system, action assets, gamepad button fallbacks, focus-loss fix, F310 X-mode mappings — all dead code. NATRIX's button bindings from `PlayerInputHandler.HandleGamepadButtonFallbacks` (A=Interact, RB=Harmonic Strike, LT=Frequency Shield, etc.) don't fire.

**Fix:** Delete `Moon1Lifeline.cs` and `SimplePlayerDriver.cs` (or fence both behind `#if DEBUG_INPUT_DIAG`). Verify PlayerInputHandler comes alive on the Player. Re-test F310 button map.

### A3. `Moon1PlayerSetup` and `PlayerSpawner` both spawn the player

Both `Integration/Moon1PlayerSetup.cs` and `Integration/PlayerSpawner.cs` instantiate a Player on Start. Whichever runs second hits "Player already exists" branch and logs a warning. **Race condition** — outcome non-deterministic between sessions.

**Fix:** Pick one. PlayerSpawner is the older / simpler. Moon1PlayerSetup is bootstrap-friendly. Delete the runtime branch of the loser and confirm via Console there's only one spawn log per Play.

### A4. `Moon1PostProcessing` and `Moon1PostProcessingPreset` both build a URP Volume

`Moon1PostProcessing.cs` (component, Start-attached) and `Moon1PostProcessingPreset.cs` (bootstrap) both create a URP Volume profile independently. Two volumes stack — exposure/bloom/grading values multiply.

**Fix:** Delete `Moon1PostProcessing.cs`; keep the newer bootstrap-driven `Moon1PostProcessingPreset.cs`.

### A5. Placeholder hero buildings still in scene alongside the real ones

The scene contains BOTH `Building_echohaven_stardome`, `Building_echohaven_harmonicfountain`, `Building_echohaven_crystalspire` (the real ones, dressed with Cathedral kit pieces) AND `StarDome_Placeholder`, `HarmonicFountain_Placeholder`, `CrystalSpire_Placeholder`. Z-fighting + tuning trigger confusion likely.

**Fix:** Delete the 3 placeholder GameObjects from the scene. Save.

---

## TIER B — The mess NATRIX walked into

These are what makes the playthrough feel rough, not engine-breaking but visually + experientially "messy."

### B1. Village is empty

All 11 Village building prefabs exist as real Blender-FBX-backed prefabs at `Assets/_Project/Prefabs/Moon1/Blender/`:
`VillageBakery, VillageCottageA, VillageCottageB, VillageCottageC, VillageInn, VillageMill, VillageSmithy, VillageWell, TownHall, Watchtower, VillagerSignpost`

**Zero are placed in the Echohaven scene.** Only village-tier objects placed: `VillageWell`, `VillagerSignpost`, `VillageArch_Entry`, `VillageLantern_0..5`. Per the CLAUDE.md 2026-05-30 mandate ("12-building minimum, ALL village buildings"), this is the marquee open task.

**Fix:** Run `Tartaria → 1 Build → Build Out Moon 1 Village (9 buildings)` (the `Editor/Moon1BuildOutVillage.cs` menu). If the menu places primitives instead of the real prefabs, patch it to load the Blender prefab variants via `AssetDatabase.LoadAssetAtPath`.

### B2. NPCs are placed but possibly mis-positioned

Scene contains `Anastasia_Cathedral`, `Lirael_AtFountain`, `Cassian_AtSpire`, `Bob_AtInn`, `Milo_NearSpawn`, `Villager_AtWell`, `CathedralChoirSpirit_Inside`, `SkeletonAtCarvedStone`. But:

- Per spec, Anastasia first appears at the **restored Spire** (not Cathedral) after first Dome restoration
- Per spec, Lirael appears Days 25-28 in the Revelation beat (gated by Day, not at Fountain at start)
- Per spec, Milo emerges from Dome ventilation shaft Day 1 (not "near spawn")

**Fix:** Audit NPC initial positions vs `docs/15` §11 NPC arrivals. Move Anastasia to Spire. Set Lirael inactive until Day 25 trigger. Confirm Milo's intro path is the Dome shaft, not the spawn point.

### B3. Mini-game variants B and C wired in code but no pedestals

`Variant A — Frequency Slider` is the only one with placed tuning pedestals. The scene has `TuningPedestal_0..8` (9 pedestals) but `Moon1BuildOutBuildings` was not patched to assign Variant B/C IDs to nodes 2 and 3 per `docs/15 §9` difficulty escalation rule. The code for Variants B+C exists at `Assets/_Project/Scripts/Integration/TuningMiniGameVariant*.cs` but no node references them.

**Fix:** Walk through each `InteractableBuilding` component on the 3 hero buildings, assign `nodePuzzles[]` = [VariantA, VariantBorC, VariantCorA]. Verify each is reachable in 2 minutes per spec.

### B4. POIs partially placed

Spec requires 4 POIs (Mud Pools × 3 hot spots, Carved Stone, Overlook, Root Chamber). Scene has Mud Pool spawns + `SkeletonAtCarvedStone` (so 2/4). No `Overlook` or `Root Chamber` GameObjects found in scene strings.

**Fix:** Run `Tartaria → 1 Build → Build Out Moon 1 Environment` to add the 4 POI rigs. Verify each fires its `+5/+10 RS` discovery event.

### B5. 9 tuning pedestals but only Variant A audio

`TuningPedestal_0..8` exist but each plays the same 432 Hz sine (Variant A). Need Variant B (waveform trace audio) and Variant C (chord build) hookup per spec §9.

### B6. Lighting is golden-hour, but no 17-hour cycle

`TartarianHourCycle.cs` is in Moon1MasterBootstrap, but the scene's directional light (`Sun_GoldenHour`) is statically posed. Per spec the sun drives a 17-hour visual day. The Cathedral Light Eruption beat is timed to "17th hour alignment" — but the cycle component is wired without anyone moving the sun. The beat may fire on time-based fallback only.

**Fix:** Open `TartarianHourCycle.cs`, verify it actually rotates the directional light. Or wire `Sun_GoldenHour.transform.rotation` to the cycle's `CurrentHourPhase` property.

---

## TIER C — Spec gaps (vs canonical design)

Things the spec says Moon 1 should have but nothing implements yet.

### C1. Adaptive music — only 1 of 2 layers

Spec calls for Layer 1 (Bed: minor key sparse cello at RS 0-50, major key + crystalline synth at RS 50-100) AND Layer 2 (Reactive: discovery arpeggio harp, tuning real-time pitch, combat percussive dissonance, restoration brass + choir). Code has `AdaptiveMusicController.cs` with Layer 1 only. Layer 2 is absent. The Drake Stafford 432 Hz licensed asset is not in the project.

### C2. Cinemachine — none

Spec wants pre-authored Cinemachine paths for: restoration dolly (5s orbit), Cathedral Light Eruption beam pan, Overlook discovery, prologue 3-min cinematic. Current code uses pure `Camera.transform.position` lerp via `RestorationCinemachine.cs` despite the name. **No actual Cinemachine package usage found.**

### C3. Aether 3-band volumetric compute shader — not present

Spec §3 calls for a 64×64×32 voxel grid driving a 3D texture sampled by URP shaders, ≤2 ms/frame. **Search returns nothing.** AetherFieldManager is the closest — it's a managed-object stand-in. No compute shader.

### C4. Save schema — partial

`SaveManager.cs` exists, writes JSON. But `BuildingState[]` per spec §6 should include `nodesComplete[3]` + `nodeAccuracy[3]` per building — current schema has flag-only restored state. `playedDialogueIds[]`, `discoveredPOIs[]`, `RS threshold persistence`, `last-crossed threshold` not in schema.

### C5. Aether GPU compute simulation absent

Spec §3 wants a full Aether field simulation (semi-Lagrangian advection, source/sink injection, golden-ratio dissipation). Currently it's a placeholder. Marked Phase 2 deferred per CLAUDE.md.

### C6. Mud dissolution shader — exists but may be unwired

`Moon1MaterialSetup.cs` was archived (and is now one of the missing-script refs). The mud dissolution shader path exists at `Assets/_Project/Materials/Shaders/`, but no scene component drives the 5-second dissolution transition on building restoration. The `BuildingRestorationCeremony` script in scene may handle this — needs verification.

### C7. Two CLAUDE.md "no stubs" violations remain

- `Moon1CompletionTracker.cs` — 9 lines, "pending regen". Should track restoration count + lore artifacts + best mini-game score. Could be **deleted** and let `Moon1ProgressPersistence` own it.
- `Moon1FirstTimeHints.cs` — 9 lines, "pending regen". Should fire single-shot HUD prompts. Could subscribe to `GameEvents.OnFirst*` events.

### C8. Lore artifact collection — 6 expected, none confirmed

Spec wants 6 collectible Tartarian scroll/tablet pickups. Search for `LoreArtifact*` returns the spawn logic but no in-scene pickups visible in the strings dump. The Giant Skeleton Key #1 IS there.

---

## TIER D — Code hygiene (dead / duplicate / stale)

These don't break the game but compound the messy feel and slow future work.

### D1. Three runtime duplicates of Editor builders

- `Moon1LevelBuilder.cs` (628 lines, runtime) duplicates `Editor/Moon1BuildOutBuildings` + `Editor/Moon1BuildOutVillage`.
- The canonical is the Editor menu (per `Moon1MasterBootstrap` comment). Delete the runtime version or strip its building-spawn paths.

### D2. Stale `.restored` snapshots

- `AI/MudGolemAI.cs.restored` (630 lines) — older copy, not compiled, **delete**.
- `AI/ResetScout.cs.restored` (1 line) — leftover, **delete**.

### D3. `EchohavenContentSpawner` split across files

- `Integration/EchohavenContentSpawner.cs` — 3-line empty marker.
- `Integration/Phase2Stubs.cs:236` — real 200+-line class buried under "Phase2".

Move the real implementation back into its own file. Anyone doing `AssetDatabase.LoadAssetAtPath` lookups gets surprised.

### D4. `_deleted_2026_05_31/` purgatory

11 files sitting in `Scripts/Integration/_deleted_2026_05_31/`. Linux rm blocked them during last cleanup. They're not compiled (filename excluded), but they're in git, take disk, and scene file references some by name (those generate the missing-script warnings). Either restore-and-fix or rm-and-strip-scene-refs. Don't leave in limbo.

### D5. Dead Editor menus

- `Tartaria/_ Legacy/Auto-Wire Moon 1 Buildings` — explicitly superseded.
- `Tartaria/_ Legacy/Spawn Milo Only` — explicitly superseded.

Move to Editor `#if false` or delete.

### D6. Two MASTER buttons confused

`Tartaria → 0 ★ MASTER → Run ALL Tiers` (Tier 1+2+3 asset generation) and `Tartaria → 0 ★ MASTER → Bootstrap All Moon 1 Systems` (scene wiring) do different things. Easy to confuse during stress. Rename to `Generate ALL Tier Assets` vs `Wire Moon1_Systems Components`.

### D7. Audio path mismatch

`Moon1 Lore audio at Audio/Moon1_Lore/` but code looking for `Audio/Moon1/`. Mismatch.

### D8. FBX-to-prefab ratio

347 Blender prefabs vs 70 source FBX. Most prefabs must be variants or nested compositions. Worth a spot-check that no large fraction are empty placeholder prefabs masquerading as art. Especially the `Moon1NewAssetsPlacer` references — many of its target asset names may not have corresponding FBX.

---

## What ISN'T in this audit

These should be verified but were out of scope this pass:

- Per-prefab inspection (whether each of 347 Blender prefabs has a valid MeshFilter + non-magenta material)
- Profiler captures to verify the AetherFieldManager doesn't tank framerate
- NavMesh coverage check (is the 500 m terrain fully baked? do NPCs path correctly across mud pools?)
- Yarn dialogue file completeness (does Anastasia actually have 4 lines? does Milo have 40?)
- The 18 cathedral kit pieces — are they all dressed in scene per the Cathedral interior spec, or just half?
- Haptics — F310 vibration tested per spec §9 (Footstep / Discovery / Perfect tune / Combat hit / Golem death)?

---

## Recommended fix order

Working top to bottom; each fix is 15 min - 2 hours unless noted.

1. **A1** — Strip 4 missing script refs from Moon1_Systems. Save scene. (15 min, makes Error Pause stop firing.)
2. **A2** — Delete `Moon1Lifeline.cs` + `SimplePlayerDriver.cs`. Verify PlayerInputHandler drives Player. Test F310 button map. (1 hour.)
3. **A3** — Pick PlayerSpawner OR Moon1PlayerSetup. Delete the loser's spawn path. (30 min.)
4. **A4** — Delete `Moon1PostProcessing.cs`. (10 min.)
5. **A5** — Delete 3 placeholder hero building GameObjects from scene. (5 min.)
6. **B1** — Run `Tartaria → 1 Build → Build Out Moon 1 Village`. Patch menu if it places primitives. Save scene. (1-3 hours.)
7. **B2** — Audit + reposition NPCs per spec §11. (1 hour.)
8. **B3** — Wire Variants B+C to Nodes 2+3 in `InteractableBuilding`. (1 hour.)
9. **B4** — Run `Tartaria → 1 Build → Build Out Moon 1 Environment` for POIs. (30 min.)
10. **B6** — Verify `TartarianHourCycle` rotates the directional light. (30 min.)
11. **D1-D7** — Delete duplicate runtime + `.restored` files + dead Editor menus + fix audio path. (1 hour total.)
12. **C7** — Delete or flesh out the two 9-line stubs. (30 min.)
13. **C1** — Add Layer 2 to AdaptiveMusicController (discovery arpeggio + combat percussive). (2-3 hours.)
14. **C4** — Extend SaveData schema per spec §6. (2 hours.)
15. **C2 / C3 / C5** — Cinemachine, Aether compute, full Aether sim — defer to Phase 2 per CLAUDE.md (the 2026-05-30 mandate covers content not engine rewrites).

Total estimated effort for **A + B + D7 + C7**: **10-14 focused hours.** That gets Moon 1 from "messy" to "playable end-to-end with the spec's 12-building footprint, 4 mini-game variants, 4 POIs, correct NPC placement, and zero Console errors at scene init."

---

*Audit generated 2026-05-31 in response to NATRIX's "walked around and its messy" report after the Error-Pause input fix unblocked Play. Four parallel agents fed this synthesis: spec inventory (1.6k words), code inventory (3.5k words), scene + asset inventory (2k words), prior-audits primer (600 words).*
