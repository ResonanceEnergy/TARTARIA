# Moon 1 Build Pipeline Audit — 2026-05-31

## TL;DR

NATRIX's vibe is right: this is messy. Five specific symptoms:

- **Two competing hero-building spawners.** Old `Moon1HeroBuildingSpawner.cs` (primitives at `(0,0,80)` etc.) and new `Moon1BuildOutBuildings.cs` (real 225 KB prefabs at `(35,0,25)` etc.). Both are wired into the build chain — the master bootstrap auto-runs the old one, then the menu sequence runs the new one. Whichever ran last "wins" but you get an `Hero_Buildings` GameObject with primitives plus a second `Hero_Buildings` from the new builder. See conflict map below.
- **Three competing NPC spawners.** Old `Moon1NPCSpawner.cs` (capsule Milo), Editor `Moon1WireMilo.cs` (legacy single Milo), and new `Moon1BuildOutNPCs.cs` (4 KayKit characters). Old one is still auto-attached by `Moon1MasterBootstrap` (`Editor/Moon1MasterBootstrap.cs:55`).
- **Two TODO-only stub Integration scripts get instantiated every run.** `Moon1AmbientCreatures.cs` and `Moon1MaterialSetup.cs` are 31-line `Debug.Log("Initialized")` stubs — both attached at `Editor/Moon1MasterBootstrap.cs:50, 48`. Direct violation of CLAUDE.md mandate rule #1.
- **`Moon1NewAssetsPlacer` references ~120 asset names; only a handful exist on disk.** Spot-checked the first 30 of ~130 referenced names (`MiloBoy`, `AnastasiaPrincess`, `LiraelGuardian`, `Wagon`, `LongSword`, `Lute`, etc.) — all 30 missing from both `Assets/_Project/Prefabs/Moon1/Blender/` and `Assets/_Project/Models/Blender/**`. The `Next100BlenderBatch` and `Next150BlenderBatch` gen scripts now have the `sys.path` fix in place but the FBX files have not been regenerated — `Models/Blender/Moon1/` holds only 42 FBX (vs 119 prefabs created from older runs). Re-running the batches is required before this placer does anything useful.
- **20 stale `.disabled` Editor scripts, 3 `.candidate` files, 1 `.tmp` orphan, 1 stale `_archived_backups/` folder, 36 `Moon2*.cs.disabled` siblings in Integration.** Cleanup overdue.

Net: the WORK is real (5,187 lines of Moon 1 Editor scripts, 119 Blender prefabs on disk, 3 hero building prefabs, full Cathedral kit). The PIPELINE is messy: two generations of scripts coexist, the bootstrap runs both, no doc tells you which sequence is canonical.

---

## Spec summary — what Moon 1 IS per docs

Cross-referenced `docs/15_MVP_BUILD_SPEC.md`, `docs/03_CAMPAIGN_13_MOONS.md` Moon 1 section, `STATUS.md`, and `CLAUDE.md` 2026-05-30 mandate.

### Buildings (12 minimum per CLAUDE.md mandate)
- **3 hero restorable** — Dome ("Listeners' Hall", 25m × 18m, buried 80%), Fountain ("Thread of Memory", 8m × 5m, buried 95%), Spire ("First Note", 3m × 15m, buried 60%). Each has 3-node tuning, 50 RS reward. `docs/15 § 7-8`.
- **9 village structures (decorative, ruined)** — Old Well, 2 Hovels, Sunken Shrine, Lookout Spire, Broken Gate, Column Trio, Memorial, Forgotten Tower. Per `CLAUDE.md` 2026-05-30 mandate ("12 minimum"). `docs/15 § 7` references "village" feel.
- **Pipe organ centerpiece** inside the Dome (3-note sequence, canonical first-restoration puzzle). `docs/03` Days 6-12.

### NPCs (4 named, per docs/03 + 05)
- **Milo** (fox/companion in canon spec, but Ranger KayKit in current build) — near spawn, NavMeshAgent follow.
- **Anastasia** — hidden until Crystal Spire restored, royal gold mage.
- **Lirael** (ghostly, 432 Hz lullaby) — near Dome, translucent.
- **Cassian** (hooded scholar) — wanders village.

### Enemies
- **Mud Golem** — primary Phase-1 enemy, RS-threshold waves at 25/50/75 (`docs/15 § 11`).
- **Reset Scout** — Victorian-costumed goons with clipboards (`docs/03` Days 13-18).

### Mini-game variants (3, per `docs/15 § 9`)
- **Variant A: Frequency Slider** — Node 1, ±8% tolerance, 15s.
- **Variant B: Waveform Trace** — Node 2, ±5%, 20s.
- **Variant C: Harmonic Pattern** — Node 3, ±3%, 10s.

### Points of Interest (6)
- 3 Mud Pools, Carved Stone (DLC tease), Overlook ridge, Root Chamber. `docs/15 § 7`.

### Environment / atmosphere
- 500m radius zone, central depression, golden-hour ambient, Tartarian fog. 4 splat layers (mud/stone/grass/Tartarian tile), 100 trees, 5k detail instances.
- 14 braziers (perimeter + hero-entrance).
- 3 cobblestone paths, perimeter stone wall.
- Vegetation: 120 KayKit grass+bush, 60 rocks, 6 lore stones, 3 fallen pillars.

### Mechanics (12 docs/03 mechanics)
Pipe organ, Reset Scout, Giant Mode (60s burst), Rose window cymatic projection, Pure water font, Spire placement ceremony, Lirael lullaby (432 Hz), Ley line map, 17th-hour cathedral eruption, Skeleton hum prophecy, Giant skeleton key #1 of 8, Dialogue runner (3 yarn files).

### Audio
6 procedural ambient sources + 432 Hz tuning baseline + restoration stinger.

### Narrative beats
Cathedral Light Eruption (17th hour), Skeleton Hum Prophecy, Giant Skeleton Key pickup, Lirael appearance.

---

## Editor-script inventory

Every script in `Assets/_Project/Scripts/Editor/Moon1*.cs` + `Next*.cs` + `EchohavenSceneAudit.cs` + `BatchReadinessValidator.cs`:

| File | Menu entry | What it builds / does | Lines | Notes |
|---|---|---|---|---|
| `Moon1MasterBootstrap.cs` | `Tartaria/MASTER: Bootstrap All Moon 1 Systems` | Adds 24 Integration components to `Moon1_Systems` GameObject. Includes 2 TODO stubs (`Moon1AmbientCreatures`, `Moon1MaterialSetup`). Drives most runtime systems. | 101 | **Auto-attaches old Hero/NPC spawners that conflict with the newer "Build Out" menus.** |
| `Moon1BuildOutBuildings.cs` | `Tartaria/Build Out Moon 1 Buildings (3 Hero)` | Instantiates real 225 KB prefabs (`Echohaven_CrystalSpire/StarDome/HarmonicFountain.prefab`) at burial depths from docs/15 §7. Adds SphereCollider trigger + `InteractableBuilding` + NavMeshObstacle. | 243 | Canonical hero-building builder (newer). |
| `Moon1HeroBuildingSpawner.cs` *(Integration, MonoBehaviour)* | n/a (auto-runs via bootstrap) | Builds Cathedral/Fountain/Spire from `GameObject.CreatePrimitive(Cube)` rocks + KayKit fallbacks at positions `(0,0,80) / (-60,0,40) / (60,0,40)`. Tries to load real prefab first (lines 88-95) but falls back to primitives. | 335 | **CONFLICTS with `Moon1BuildOutBuildings.cs` — different positions, different geometry.** |
| `Moon1BuildOutEnvironment.cs` | `Tartaria/Build Out Moon 1 Environment (POIs + Mud)` | Places 6 POIs (3 Mud Pools, Carved Stone, Overlook, Root Chamber) under `Echohaven_Environment`. Primitives + URP materials. | 215 | Canonical environment builder. |
| `Moon1BuildOutVegetation.cs` | `Tartaria/Build Out Moon 1 Vegetation (Grass+Bushes)` | Polar-scatter 80 KayKit grass + 40 bushes around plaza, deterministic seed 20260530, no-go zones around buildings + spawn. | 174 | Canonical vegetation. |
| `Moon1BuildOutVillage.cs` | `Tartaria/Build Out Moon 1 Village (9 secondary structures)` | 9 ruined village structures from Cathedral kit pieces (Foundation/Wall/Column/Archway/RoseWindow/Spire). Buried 0.5-3m. | 275 | Canonical village builder. |
| `Moon1BuildOutProps.cs` | `Tartaria/Build Out Moon 1 Props (Rocks + Lore Stones + Fallen Pillars)` | 60 KayKit rocks + 6 lore stones + 3 fallen pillars. | 235 | Canonical props. |
| `Moon1BuildOutNPCs.cs` | `Tartaria/Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)` | 4 KayKit characters at thematic positions, Anastasia disabled until `OnBuildingRestored`. | 249 | Canonical NPC builder. |
| `Moon1NPCSpawner.cs` *(Integration, MonoBehaviour)* | n/a (auto-runs via bootstrap) | Spawns Milo (capsule primitive) + 5 excavation workers + 3 scholars + 2 merchants under `NPCs` GameObject. Uses URP `material.color` which is `_Color` not `_BaseColor` → magenta. | 259 | **CONFLICTS with `Moon1BuildOutNPCs.cs` — different roster, primitive geometry.** |
| `Moon1WireMilo.cs` | `Tartaria/Legacy/Spawn Milo Only (use 'Build Out Moon 1 NPCs' for full cast)` | Standalone Milo placer with `MiloController` + `MiloFollowBehaviour`. | 98 | Self-flagged as superseded. |
| `Moon1WireSpawner.cs` | `Tartaria/Wire Echohaven Content Spawner` | Creates `EchohavenContentSpawner` GameObject, auto-assigns MudGolem prefab. | 79 | One-shot wiring. |
| `Moon1AutoWire.cs` | `Tartaria/Legacy/Auto-Wire Moon 1 Buildings (placeholders ...)` | Wires `InteractableBuilding` to placeholder GameObjects (`CrystalSpire_Placeholder` etc.) in-place. | 97 | Self-flagged as superseded. |
| `Moon1AudioWire.cs` | `Tartaria/Wire Echohaven Audio (Ambient + SFX)` | Drops `Audio_Ambient` GameObject with KayKit ambient track. | 97 | Canonical audio wiring. |
| `Moon1CombatVerify.cs` | `Tartaria/Combat Verify (Moon 1)` | Auto-detects EchohavenCombatArena, checks MudGolem + PlayerCombat refs. | 103 | Verification only. |
| `Moon1NavMeshBake.cs` | `Tartaria/Bake NavMesh For Active Scene` + `Save Scene` + `Ready Check (Audit + Bake + Save)` | NavMesh bake / scene save / combined "ready check" sequence. | 66 | Final-step utility. |
| `Moon1FixSpawn.cs` | `Tartaria/Fix PlayerSpawner Position`, `Add Fall-Through Safety Net`, `Toggle Camera Y/X Inversion` | One-shot fixes for player spawn position + camera. | 119 | Stays useful. |
| `Moon1FixRuntime.cs` | `Tartaria/Fix: Ensure Exactly One AudioListener` + `Convert Magenta Materials` + `Re-attach EchohavenContentSpawner Script` + `DIAGNOSE: List All Material Shaders` + `Fix: ALL Moon 1 Runtime Issues` | Cluster of runtime cleanup fixes. | 243 | Cluster — stays useful. |
| `Moon1AcceptanceAudit.cs` | `Tartaria/Moon 1/Acceptance Audit` | 17 falsifiable checks (asset existence, class reflection, file content needles, line counts, .candidate count). | 140 | Canonical verifier. |
| `Moon1PlaytestDiag.cs` | `Tartaria/Moon 1/Diag Player + Camera State` | Editor scene-state probe. | 101 | Diagnostic. |
| `Moon1SceneRescue.cs` | `Tartaria/Moon 1/Scene Rescue (Dedupe + Force Player + Camera Follow)` | Dedupes scene, force-spawns Player, parents camera. | 274 | Recovery utility. |
| `Moon1VillagePropScatter.cs` | `Tartaria/Moon 1/Scatter Village Props` | 30+ KayKit RPG props (anvil/hammer/grindstone) at blacksmith/engineer zones. | 119 | Add-on to village. |
| `Moon1AssetGenerators.cs` | `Tartaria/Moon 1/Generate SFX Library` + `Particle Textures` + `Golden-Hour Skybox` + `Diagnose Custom Shaders` + `Add Hero Post-State Markers` | Asset generator cluster — SFX, particle PNGs, skybox, marker objects. | 396 | Useful asset gen. |
| `Moon1AnimatorBinder.cs` | `Tartaria/Moon 1/Bind KayKit Animators` | Rig_Medium + Rig_Large AnimatorControllers, binds to 16+ characters. | 182 | One-shot. |
| `Moon1CharacterPipeline.cs` | `Tartaria/Moon 1/Build ResetScout Prefab` + `Triage Corrupt Characters` + `Attach KayKit Equipment` | ResetScout creator + .corrupt triage + equipment attach. | 185 | One-shot. |
| `Moon1BlenderBatch.cs` | `Tartaria/Moon 1/Run Blender Batch (Generate All Moon 1 Assets)` | Launches Blender headlessly, runs `run_all_moon1.py`. | 72 | Primary art-batch entry. |
| `Moon1BlenderPrefabPlacer.cs` | `Tartaria/Moon 1/Place Blender Prefabs (Echohaven Scene Dressing)` | 93 placements: hero props + 8 braziers + 3 mud pools + 9 crystals + Anastasia chair + 9 tuning pedestals + Bob's Inn + Carved Stone + skeleton + giant key (per audit). | 232 | Working (93 prefabs placed). |
| `Moon1NewAssetsPlacer.cs` | `Tartaria/Moon 1/Place New Assets (vehicles, weapons, flora, fauna...)` | ~130 placements: 6 NPCs + 5 enemies + 13 vehicles + 10 weapons + 3 armor + 6 instruments + 12 cooking/alchemy + 10 containers + 22 trees + 24 small flora + 10 fauna + 12 arch + 10 sigils + 13 extras. | 244 | **BROKEN: most referenced assets are missing.** Only places ~1 if you run today. |
| `Next100BlenderBatch.cs` | `Tartaria/Art/Run Next-100 Blender Batch` | Headless Blender — 9 gen scripts: humanoid + enemies + village + special buildings + tools + furniture2 + ritual + minigame + extras. | 99 | sys.path fix verified in `gen_*.py`. Re-run needed. |
| `Next150BlenderBatch.cs` | `Tartaria/Art/Run Next-150 Blender Batch` | Headless Blender — similar batch for "next 150" assets. | 94 | sys.path fix verified. Re-run needed. |
| `EchohavenSceneAudit.cs` | `Tartaria/Scene Audit: Echohaven` | 8 scene checks (scene exists, PlayerSpawner, NavMesh, 3 buildings, ContentSpawner, missing-scripts, MainCamera, DirectionalLight). | 397 | Canonical scene audit. |
| `BatchReadinessValidator.cs` | `Tartaria/Validate Build Readiness` | Build-readiness validation gate. | 258 | Pre-build gate. |

**Total Moon 1-touching Editor lines:** 5,187.

**Plus 2 Integration MonoBehaviours that auto-run via bootstrap:**
- `Moon1HeroBuildingSpawner.cs` (335 lines) — competes with `Moon1BuildOutBuildings.cs`.
- `Moon1NPCSpawner.cs` (259 lines) — competes with `Moon1BuildOutNPCs.cs`.

---

## Overlap / conflict map

| Spec target | Authoritative builder (canonical, newer) | Conflicting / duplicate builder (older, still wired) | Outcome |
|---|---|---|---|
| 3 hero buildings | `Moon1BuildOutBuildings.cs` — real prefabs at `(35,0,25) / (-30,0,30) / (5,0,50)` with burial depths from docs/15 | `Moon1HeroBuildingSpawner.cs` (auto-runs via bootstrap) — primitive Cubes at `(0,0,80) / (-60,0,40) / (60,0,40)`; tries prefab first, falls back to primitives | **Two `Hero_Buildings` GameObjects can exist; old auto-runs first via `[DefaultExecutionOrder(-86)]`** |
| 9 village structures | `Moon1BuildOutVillage.cs` (Cathedral kit pieces) | `Moon1LevelBuilder.cs` (auto-runs `CreateVillageGrid()`, builds 9 buildings at `±40,0,±40` from KayKit rocks) | **Both build 9 buildings, different geometry, different positions. Order is `Moon1LevelBuilder` (-85) → `Moon1BuildOutVillage` (manual menu).** |
| 4 NPCs | `Moon1BuildOutNPCs.cs` (4 KayKit characters) | `Moon1NPCSpawner.cs` (auto-runs via bootstrap; Milo + 10 generic primitives) | **`NPCs` GameObject (primitives) AND `Echohaven_NPCs` GameObject (KayKit) coexist.** |
| Milo only | `Moon1BuildOutNPCs.cs` covers it | `Moon1WireMilo.cs` legacy menu + `Moon1NPCSpawner.SpawnMilo` (autoruns) | **3 Milos possible.** Self-flagged in `Moon1WireMilo.cs:11`. |
| Hero building wiring (InteractableBuilding) | `Moon1BuildOutBuildings.cs` handles SerializedObject wiring | `Moon1AutoWire.cs` (legacy menu) wires placeholder GameObjects | Both work on different name patterns. `Moon1AutoWire` self-flagged as legacy. |
| Pipe organ puzzle | `PipeOrganPuzzle.cs` (329 lines, ITuningVariant) | `PipeOrganMiniGame.cs` (307 lines, namespace `Tartaria.Gameplay` — possibly Moon 2-targeted per file comment) | **Two pipe organ classes coexist; risk of confusion.** |
| Mud Golem spawning | `EchohavenContentSpawner` in `Phase2Stubs.cs` | (none) | Single source. Good. |
| 3 tuning mini-game variants | `TuningMiniGame.cs` + `TuningVariantB_Waveform.cs` + `TuningVariantC_Pattern.cs` | (none) — but `TuningMiniGameController.cs.disabled` + `TuningMiniGame.cs.tmp` orphan exists | Single live source. Cleanup stale files. |
| Master bootstrap wiring | `Moon1MasterBootstrap.cs` (24 components) | (none) | Single source. But wires 2 known stubs. |

---

## Gap map — spec items that nothing builds

Cross-referenced spec → Editor scripts. These spec items have NO dedicated Editor-script builder:

| Spec item | Status | Where it lives (if anywhere) |
|---|---|---|
| **Terrain heightmap** (500m radius, 1025×1025, central depression rising 30m to south ridge) | **Not built.** No terrain generator. | Scene relies on flat default ground + primitive `(14×14 brown platform)` from `Moon1FixSpawn.cs:50`. |
| **4 terrain splat layers** (mud/stone/grass/Tartarian tile) | **Not built.** | n/a |
| **Adaptive 2-layer music** (RS-reactive) | Not built, audio is single ambient loop. | `Moon1AudioWire.cs:31` drops single track. |
| **Save/Persistence** (full schema) | Partial — PlayerPrefs only via `Moon1ProgressPersistence.cs` (139 lines). Spec wants `SaveData` JSON. | `Integration/Moon1ProgressPersistence.cs` (PlayerPrefs subset). |
| **Cinemachine restoration cinematic** | Partial — `Moon1CinematicMoments.cs` (194 lines) does dolly orbit. Spec wants pre-authored Cinemachine paths. | Manual camera lerp, no Cinemachine. |
| **17-hour day/night cycle visual** | `TartarianHourCycle.cs` (124 lines) exists. | OK. |
| **Lighting bake** | No Editor script. | Scene-level only. |
| **Aether 3-band volumetric compute shader** | Not in Moon 1 scripts. Spec says full. | Out of scope for this audit but a gap per spec. |

Most spec items DO have a builder. The biggest gap is the terrain itself — there's no scripted heightmap generation; the scene relies on whatever was authored manually, which is a flat plane.

---

## Stale / duplicate cleanup recommendations

### Editor `.disabled` files (20) — `Assets/_Project/Scripts/Editor/`
All have no `.disabled` extension consumers and predate the current Editor menu set. Recommendation: **archive to `Assets/_Project/Scripts/Editor/_archive/`** (don't delete — git history preserves them but Unity's import + meta tracking is cleaner with them out of the tree).

- `AddressablesConfigurator.cs.disabled` — archive
- `AssetWiringTool.cs.disabled` — archive
- `AutoSceneSetup.cs.disabled` — archive
- `CheckRealMissingScripts.cs.disabled` — archive
- `CleanupMissingScripts.cs.disabled` — archive
- `DataAssetGenerator.cs.disabled` — archive
- `EmergencyPlayableFix.cs.disabled` — archive
- `EmergencySpawnerFix.cs.disabled` — archive
- `FixEchohavenMissingScripts.cs.disabled` — archive
- `ForceExplorationState.cs.disabled` — archive
- `ForceWASDMovement.cs.disabled` — archive
- `Moon1LevelBuilderAutoSetup.cs.disabled` — **DELETE** (superseded by `Moon1MasterBootstrap`)
- `MoonSceneBuilder.cs.disabled` — archive
- `SceneMissingScriptAnalyzer.cs.disabled` — archive
- `SceneSetupFixer.cs.disabled` — archive
- `SkillDataEditor.cs.disabled` — archive
- `SkillTreeAssetGenerator.cs.disabled` — archive
- `TestRunner.cs.disabled` — archive
- `ValidatePlayModeEntry.cs.disabled` — archive
- `VexValidator.cs.disabled` — archive

### `.candidate` files (3) — violation of CLAUDE.md rule #3 ("never leave .candidate unresolved")
- `Assets/_Project/Scripts/Integration/Moon1LevelBuilder.cs.candidate` (51 lines) — **DELETE.** Live `Moon1LevelBuilder.cs` is 628 lines and already calls `TryBuildFromCathedralKit`. The 51-line candidate is a strictly smaller stub.
- `Assets/_Project/Scripts/Gameplay/DissonanceCrystal.cs.candidate` (72 lines) — **DELETE or apply.** Check if `DissonanceCrystal.cs` exists, diff, decide.
- `Assets/_Project/Scripts/Gameplay/PlayerRanged.cs.candidate` (60 lines) — **DELETE or apply.** Same.

### `.tmp` / `.partial_backup` orphans
- `Assets/_Project/Scripts/Gameplay/TuningMiniGame.cs.tmp` (1 byte) — **DELETE.**
- `Assets/_Project/Scripts/Integration/Phase2Stubs.cs.partial_backup.disabled` (36 lines, LeanTween stub) — **DELETE.**

### `_archived_backups/` folder — `Assets/_Project/Scripts/Core/_archived_backups/`
- `GameEvents.cs.BEFORE_FIX_20260528_223633` + `.meta` — **DELETE** (GameEvents.cs is healed per STATUS.md).
- `GameEvents.cs.BROKEN_BACKUP` + `.meta` — **DELETE.**
- Then **DELETE the folder.**

### Integration MonoBehaviours that should be retired
- `Moon1HeroBuildingSpawner.cs` (335 lines) — **REMOVE from `Moon1MasterBootstrap.cs:46`** (don't auto-attach). Either delete the file entirely OR keep as a fallback that ONLY runs if no `Moon1BuildOutBuildings`-instantiated `Hero_Buildings` group exists. Recommendation: **delete the file** — `Moon1BuildOutBuildings.cs` is the spec-aligned builder.
- `Moon1NPCSpawner.cs` (259 lines) — **REMOVE from `Moon1MasterBootstrap.cs:55`.** Recommendation: **delete the file** — `Moon1BuildOutNPCs.cs` covers the canonical roster.
- `Moon1AmbientCreatures.cs` (31 lines, TODO stub) — either flesh it out OR remove from bootstrap (`Moon1MasterBootstrap.cs:50`) AND delete the file. CLAUDE.md mandate rule #1 says no stubs.
- `Moon1MaterialSetup.cs` (31 lines, TODO stub) — same.
- `Moon1BuildingPrefabCreator.cs` (198 lines) — Editor-only asset gen, but attached as a MonoBehaviour via `Moon1MasterBootstrap.cs:57`. Review whether it should be Editor-time only.
- `Moon1LevelBuilder.cs` (628 lines) — keeps building 9 village buildings via `CreateVillageGrid()` at `±40,0,±40` from KayKit rocks. **Conflicts** with `Moon1BuildOutVillage.cs`. Either remove from bootstrap (`Moon1MasterBootstrap.cs:47`) OR have it gate on existence of `Echohaven_Village` group.

### Moon 2-13 `.disabled` Integration files (36)
- `Assets/_Project/Scripts/Integration/Moon2*.cs.disabled` (35 files) + `Assets/_Project/Scripts/AI/_DOTS_ARCHIVE/Moon2CrystalEnemyAISystem.cs.disabled` (1) — **archive to `Assets/_Project/Scripts/Integration/_moon2_archive/`** until Moon 2 actually starts. Reduces noise in current Moon 1 work.

### Pipe organ duplicate
- `Assets/_Project/Scripts/Gameplay/PipeOrganMiniGame.cs` (307 lines) — comment says "Moon 2 (Crystalline Caverns) harmonic puzzle." If true, **rename to `Moon2PipeOrganMiniGame.cs`** to disambiguate from `PipeOrganPuzzle.cs` (the Moon 1 ITuningVariant). Or **DELETE if `PipeOrganPuzzle.cs` already covers both Moons.**
- `Assets/_Project/Scripts/Gameplay/TuningMiniGameController.cs.disabled` — **DELETE.**

---

## Recommended canonical sequence — "build Moon 1 according to spec"

Single-path-of-truth menu order. Each step is idempotent. **Skip the master bootstrap** (it auto-attaches old conflicting spawners); explicitly run only what you need.

```
1. Tartaria → Moon 1 → Run Blender Batch (Generate All Moon 1 Assets)
       └─ Optional. Only re-run if Models/Blender/Moon1/ is missing FBX.
2. Tartaria → Art → Run Next-100 Blender Batch
3. Tartaria → Art → Run Next-150 Blender Batch
       └─ Run together. These produce the ~250 assets that Moon1NewAssetsPlacer
          currently fails to find. After they finish, AssetDatabase.Refresh
          should auto-create prefab variants via BlenderImportPostprocessor.cs.

4. Tartaria → Build Out Moon 1 Buildings (3 Hero)
5. Tartaria → Build Out Moon 1 Environment (POIs + Mud)
6. Tartaria → Build Out Moon 1 Village (9 secondary structures)
7. Tartaria → Build Out Moon 1 Vegetation (Grass+Bushes)
8. Tartaria → Build Out Moon 1 Props (Rocks + Lore Stones + Fallen Pillars)
9. Tartaria → Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)
10. Tartaria → Moon 1 → Scatter Village Props
11. Tartaria → Moon 1 → Place Blender Prefabs (Echohaven Scene Dressing)
12. Tartaria → Moon 1 → Place New Assets (vehicles, weapons, flora, fauna...)

13. Tartaria → Moon 1 → Bind KayKit Animators
14. Tartaria → Moon 1 → Build ResetScout Prefab
15. Tartaria → Wire Echohaven Content Spawner
16. Tartaria → Wire Echohaven Audio (Ambient + SFX)

17. Tartaria → Moon 1 → Generate SFX Library
18. Tartaria → Moon 1 → Generate Particle Textures
19. Tartaria → Moon 1 → Generate Golden-Hour Skybox

20. Tartaria → Combat Verify (Moon 1)
21. Tartaria → Scene Audit: Echohaven
22. Tartaria → Moon 1 → Acceptance Audit
23. Tartaria → Ready Check (Audit + Bake + Save)
```

**What about `Tartaria/MASTER: Bootstrap All Moon 1 Systems`?** It's tempting (one click adds 24 components), but it auto-attaches conflicting old spawners. **Recommendation:** edit the bootstrap to drop `Moon1HeroBuildingSpawner`, `Moon1NPCSpawner`, `Moon1LevelBuilder`, `Moon1AmbientCreatures`, `Moon1MaterialSetup`, `Moon1BuildingPrefabCreator` — those 6 lines. Then bootstrap becomes safe and step 0 of the sequence above.

---

## Honest verdict on pipeline health

The Moon 1 build pipeline is mid-grade messy, not catastrophic. The work is real: 5,187 lines of Editor scripts, 119 Blender prefabs, 3 hero building prefabs (225 KB each), full Cathedral kit, 4 yarn dialogue files, working tuning variant A, and 22 menus that actually fire. The acceptance audit (`Moon1AcceptanceAudit.cs`) is a real falsifiable verifier. What's messy is that two generations of builders coexist without anyone deleting the old generation — `Moon1HeroBuildingSpawner` keeps shipping primitives even though `Moon1BuildOutBuildings` ships the real prefab, and the master bootstrap dutifully runs both. The 2 TODO-only Integration stubs (`Moon1AmbientCreatures`, `Moon1MaterialSetup`) are CLAUDE.md mandate violations that have survived two cleanup passes. The `Moon1NewAssetsPlacer` was authored for assets that the Next-100/Next-150 Blender batches were supposed to produce but didn't (because of the now-fixed `sys.path` issue) — so it tries to place ~130 things and silently no-ops on most of them. None of this prevents the game from running; all of it makes "build Moon 1 from a clean checkout" harder than it should be. A 30-minute pass that (a) deletes 6 conflicting auto-attaches from `Moon1MasterBootstrap.cs`, (b) re-runs the two Blender batches, (c) archives the 20 `.disabled` Editor files + 3 `.candidate` files + 1 `_archived_backups/` folder, would clear most of the smell.
