# Sprint 11 Lane 8 — Scene Authoring Audit (Echohaven_VerticalSlice)

Date: 2026-06-02
Branch: `agent/audit/scene-authoring` (worktree `C:\dev\_wt_s11_l8_scene`)
Base SHA: `e07660306026c2da2a1c222f26189c99a8fc4a3c`
Scene file: `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (124,652 B)

> Mission: catalogue what is **baked into the scene file** vs what is **created at runtime** by `[RuntimeInitializeOnLoadMethod]` static bootstrappers and `Awake/Start` rescue paths. Every runtime-created GameObject is a brittle wire: sibling agents flipping a sceneName check, a singleton guard, or an `EnsureCalled` flag breaks the chain silently.

---

## TL;DR

- The Echohaven scene file contains **48 unique GameObject names** (PlayerSpawn marker, lighting rig, terrain, walls, UI shells, the `Moon1_Systems` host).
- Moon 1 boot chain creates an estimated **400+ GameObjects at runtime** across ~30 bootstrappers — every prop, mound, mob, building detail, crystal, brazier, mud pool, foliage tuft, antenna spire, HUD canvas, dialogue panel, post-process volume, and lighting rig.
- The scene file itself only persists the **frame** (lighting + walls + Moon1_Systems host); 95%+ of perceived "Echohaven content" is rebuilt every Play.
- `Moon1_Systems` GameObject (scene line 2984) still references a missing script `Moon1HeroBuildingSpawner` (scene line 1264 / 3048) — the Moon1MasterBootstrap cleanup pass removed the class but did not strip the serialized component entry.

---

## 1. Editor menus that scaffold INTO the scene file

These write to disk via `PrefabUtility.InstantiatePrefab` + `EditorSceneManager.MarkSceneDirty`. Their output is persisted, not runtime-rescued.

| Menu | File:line | What it bakes |
|---|---|---|
| `Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems` | `Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs:28` | Creates `Moon1_Systems` GameObject + attaches 12 Moon1*/Echohaven*/Anastasia/Lirael/Zone/Hour-cycle components |
| `Tartaria/0 ★ MASTER/Tier 1 — FBX + Terrain + Splats + Lighting` | `Assets/_Project/Scripts/Editor/Moon1Tier1Master.cs:25` | Chains 5 sub-menus (FBX gen, terrain, splats, lighting bake) |
| `Tartaria/0 ★ MASTER/Run ALL Tiers (Everything)` | `Assets/_Project/Scripts/Editor/Moon1AllTiersMaster.cs:23` | Sequences Tier 1 + VFX + Audio menus |
| `Tartaria/0 ★ MASTER/Wire ALL Scene Prefab Refs (full sweep, Blender-only)` | `Assets/_Project/Scripts/Editor/Moon1WireSpawnerPrefabs.cs:95` | Populates EchohavenContentSpawner / BuildingSpawner prefab fields |
| `Tartaria/1 Build/Moon 1 — Buildings (3 Hero)` | `Assets/_Project/Scripts/Editor/Moon1BuildOutBuildings.cs:102` | Bakes `Hero_Buildings` parent + 3 PrefabUtility instances |
| `Tartaria/1 Build/Build Out Moon 1 Village (9 Buildings)` | `Assets/_Project/Scripts/Editor/Moon1BuildOutVillage.cs:59` | Bakes `Village_Buildings` parent + 9 PrefabUtility instances |
| `Tartaria/1 Build/Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian + Bob)` | `Assets/_Project/Scripts/Editor/Moon1BuildOutNPCs.cs:22` | Bakes `Echohaven_NPCs` parent + 5 NPC prefab instances |
| `Tartaria/1 Build/Moon 1 — Props / Vegetation / VFX / Audio Lore` | `Moon1BuildOutProps.cs`, `Moon1BuildOutVegetation.cs`, `Moon1ClimacticVFX.cs`, `Moon1ProceduralLore.cs` | Bakes props / vegetation / VFX prefabs |
| `Tartaria/2 Place/Moon 1 — Blender Prefabs` | `Assets/_Project/Scripts/Editor/Moon1BlenderPrefabPlacer.cs` | Places generated Blender FBX prefabs |
| `Tartaria/8 Fix/PlayerSpawner Position` etc. | `Moon1FixSpawn.cs:lines 51/52/53/60/61` | Hot-patches spawn marker transforms |

**Status of scene file vs these menus:** scene only contains the `Moon1_Systems` host (with the stale missing-script ref). No `Hero_Buildings`, no `Village_Buildings` children, no `Echohaven_NPCs` parent. That implies either the build-out menus have not been re-run after the most recent scene save, or their results were stripped by `Moon1MegaCleanup` / `Moon1SceneCleanup`.

---

## 2. Runtime-created GameObjects in the Moon 1 boot chain

The dominant pattern is `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)] static void Bootstrap()` → `new GameObject(...) → AddComponent<T>()` → singleton stash. 30 Moon1* / Echohaven* / runtime-singleton bootstrappers fire on every Echohaven Play.

Below is the catalogue of runtime-created GameObjects in the Moon 1 boot chain, classified per the rubric: ⚪ OK Bootstrap / 🟡 SCENE-AUTHORING-GAP / 🔴 SHOULD-LIVE-IN-PREFAB.

### 2.1 Singleton-host bootstrappers (one GameObject, holds the script)

| # | GameObject name | Created at | Class | Severity | Notes |
|---|---|---|---|---|---|
| 1 | `MiloTutorialFlow` | `Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:127` | MiloTutorialFlow | 🟡 SCENE-AUTHORING-GAP | Tutorial driver — should be a scene-baked GameObject so SerializeFields are inspector-editable, not hardcoded |
| 2 | `MiloTutorial_WaypointArrow` | `MiloTutorialFlow.cs:376` | (new GO) | 🔴 SHOULD-LIVE-IN-PREFAB | Visual waypoint arrow built from primitives — should be `Assets/_Project/Prefabs/UI/WaypointArrow.prefab` |
| 3 | `AdaptiveMusicController` | `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:60` | AdaptiveMusicController | ⚪ OK | Cross-scene singleton, DontDestroyOnLoad — bootstrap pattern is correct |
| 4 | `GameBootstrap` | `Assets/_Project/Scripts/Core/GameBootstrap.cs:42` | GameBootstrap | ⚪ OK | ECS world setup, intentionally global |
| 5 | `MemoryWatchdog_R6` | `Assets/_Project/Scripts/Core/GameBootstrap.cs:57` | MemoryWatchdog | ⚪ OK | Diagnostic singleton |
| 6 | `DialogueManager` | `Assets/_Project/Scripts/Integration/DialogueManager.cs:881` (AutoBootstrap) | DialogueManager | ⚪ OK | Per CLAUDE.md AutoBootstrap is the canonical pattern (commit `14d5ecc4`) |
| 7 | `DialoguePlayer` (child of DialogueManager) | `DialogueManager.cs:782` | DialoguePlayer | ⚪ OK | Sub-component lazy-created on first conversation |
| 8 | `Moon1AudioOrchestra` | `Assets/_Project/Scripts/Integration/Moon1AudioOrchestra.cs:29` | Moon1AudioOrchestra | 🟡 SCENE-AUTHORING-GAP | Loads clips by name from Resources — should be a prefab w/ inspector-assigned clip refs |
| 9 | `Moon1AudioAtmosphere` host + `Moon1_Audio_Root` + child sources | `Moon1AudioAtmosphere.cs:38, :55, :82, :117` | Moon1AudioAtmosphere | 🟡 SCENE-AUTHORING-GAP | Builds full ambient + stinger rig at runtime; spatial AudioSource transforms hardcoded |
| 10 | `Moon1Braziers` host + `Moon1_Braziers_Root` + N braziers (each w/ Flame + Light child) | `Moon1Braziers.cs:22, :29, :53, :76, :127` | Moon1Braziers | 🔴 SHOULD-LIVE-IN-PREFAB | Braziers are environment props — every flame, mesh, and Light component is built from primitives. Replace with `Assets/_Project/Prefabs/Moon1/Brazier.prefab` placed in scene |
| 11 | `Moon1MudPoolPuzzle` host + `Moon1_MudPools_Root` + N pools (each w/ MudBubbles ParticleSystem + crystal nodes + lights) | `Moon1MudPoolPuzzle.cs:34, :41, :53, :75, :114, :128` | Moon1MudPoolPuzzle | 🔴 SHOULD-LIVE-IN-PREFAB | Tuning puzzle props — pool meshes + crystal interactables should be authored prefabs |
| 12 | `Moon1_GoldenHour_Volume` (URP Volume + profile) | `Moon1PostProcessingPreset.cs:19` | (Volume) | 🟡 SCENE-AUTHORING-GAP | Post-process volume already has scene authoring (`PostProcessVolume` at scene line — see m_Name list); duplicate runtime volume |
| 13 | `__Moon1CinematicMoments` | `Moon1CinematicMoments.cs:25` | Moon1CinematicMoments | 🟡 SCENE-AUTHORING-GAP | Timeline director — should be scene-baked w/ inspector cue list |
| 14 | `__Moon1NarrativeBeats` | `Moon1NarrativeBeats.cs:19` | Moon1NarrativeBeats | 🟡 SCENE-AUTHORING-GAP | Same as above |
| 15 | `GiantSkeletonKey_1` | `Moon1NarrativeBeats.cs:52` | (trigger) | 🔴 SHOULD-LIVE-IN-PREFAB | Key prop interactable — bake at (-40, 1.2, -20) in scene |
| 16 | `Moon1ProgressPersistence` | `Moon1ProgressPersistence.cs:36` | Moon1ProgressPersistence | ⚪ OK | Pure save/load logic singleton |
| 17 | `Moon1FirstTimeHints` | `Moon1FirstTimeHints.cs:50` | Moon1FirstTimeHints | ⚪ OK | UI hint state singleton |
| 18 | `Moon1InteractionPrompt` + child `InteractionPromptCanvas` + `PromptLabel` | `Moon1InteractionPrompt.cs:34, :63, :82` | Moon1InteractionPrompt | 🟡 SCENE-AUTHORING-GAP | Should be a prefab `Prefabs/UI/InteractionPrompt.prefab` so font + layout are editable |
| 19 | `Moon1InnRestTrigger` + `InnCube` | `Moon1InnRestTrigger.cs:22, :33` | Moon1InnRestTrigger | 🔴 SHOULD-LIVE-IN-PREFAB | Trigger + visual cube — author in scene at Bob's Inn position |
| 20 | `Moon1AnastasiaController` | `Moon1AnastasiaController.cs:35` | Moon1AnastasiaController | 🟡 SCENE-AUTHORING-GAP | Logic singleton — gates Anastasia reveal. Should be a Moon1_Systems sibling already, not runtime-spawned |
| 21 | `Moon1AnastasiaRocker` host + `AnastasiaRockingChair` + procedural `Anastasia_Procedural` + `AnastasiaProximityTrigger` + `HumSource` | `Moon1AnastasiaRocker.cs:32, :47, :123, :154, :166` | Moon1AnastasiaRocker | 🔴 SHOULD-LIVE-IN-PREFAB | Whole Anastasia setpiece built procedurally including the character mesh — should be `Prefabs/Characters/Anastasia.prefab` placed at the StarDome center |
| 22 | `Moon1CombatDirector` + `Moon1_Combat_Root` + procedural `ResetScout_Procedural` + procedural `MudGolem_Procedural` (per encounter) | `Moon1CombatDirector.cs:40, :57, :97, :183` | Moon1CombatDirector | 🔴 SHOULD-LIVE-IN-PREFAB | Per CLAUDE.md late-night mandate rule#4: never use primitives for enemies. Real KayKit prefabs exist — should `Instantiate(prefab)` not `new GameObject` |
| 23 | `Moon1VillagerAmbient` + `Moon1_Villagers_Root` + N procedural `Villager_*` | `Moon1VillagerAmbient.cs:58, :66, :96` | Moon1VillagerAmbient | 🔴 SHOULD-LIVE-IN-PREFAB | Same rule as #22 — KayKit villager prefabs exist |
| 24 | `Moon1EnvironmentDetail` + `Moon1_Environment_Root` + N props/path/`Fireflies_Proc`/`RollingFog_Proc` | `Moon1EnvironmentDetail.cs:28, :38, :79, :107, :143, :176` | Moon1EnvironmentDetail | 🔴 SHOULD-LIVE-IN-PREFAB | Environment details — Blender pipeline + scene authoring is the canonical replacement |
| 25 | `Moon1WinScreen` + `BG` + N labels | `Moon1WinScreen.cs:32, :61, :84` | Moon1WinScreen | 🟡 SCENE-AUTHORING-GAP | Should be `Prefabs/UI/WinScreen.prefab` |
| 26 | `Moon1PostRestorationVisuals` + N labelled VFX hosts | `Moon1PostRestorationVisuals.cs:57, :271` | Moon1PostRestorationVisuals | 🟡 SCENE-AUTHORING-GAP | VFX controller — should reference prefab pool, not build hosts |
| 27 | `RuntimeHUDBuilder` + `HUD_Canvas` + `EventSystem` + `HUDController` + `ControlsHint` + `QuestToast` + `DialoguePanel` + `DialogueSpeaker` + text children | `Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:64, :149, :169, :182, :243, :274, :353, :366` | RuntimeHUDBuilder | 🔴 SHOULD-LIVE-IN-PREFAB | Entire HUD constructed from raw Canvas + Image + TMP at runtime. Should be `Prefabs/UI/MainHUD.prefab` w/ inspector-tunable layout |
| 28 | `EchohavenObelisk` + `CrownRing_VFX` + `CrownOrb_VFX` | `EchohavenObelisk.cs:38, :70, :99` | EchohavenObelisk | 🔴 SHOULD-LIVE-IN-PREFAB | Obelisk is a hero prop |
| 29 | `ZoneController` | `ZoneController.cs:54` | ZoneController | ⚪ OK (but redundant — see § 5) | Already added by Moon1MasterBootstrap to Moon1_Systems |
| 30 | `EchohavenProgressionSystem` | `EchohavenProgressionSystem.cs:47` | EchohavenProgressionSystem | ⚪ OK (but redundant) | Same — already on Moon1_Systems |
| 31 | `BossEncounterSystem` + `MudColossus_VisualProxy` + `RailWraith_VisualProxy` + `SludgeLeviathan_VisualProxy` + `SkyReaver_VisualProxy` + `FrequencyWraith_VisualProxy_R7` | `BossEncounterSystem.cs:142, :737, :753, :768, :784, :801` | BossEncounterSystem | 🔴 SHOULD-LIVE-IN-PREFAB | Five boss "VisualProxy" GameObjects built from primitives — bosses should be real prefabs per CLAUDE.md mandate |
| 32 | `CampaignFlowController` | `CampaignFlowController.cs:49` | CampaignFlowController | ⚪ OK | Cross-scene flow singleton |
| 33 | Moon1CameraFollowPlayer host | `Moon1CameraFollowPlayer.cs:33` (Bootstrap) | Moon1CameraFollowPlayer | 🟡 SCENE-AUTHORING-GAP | Camera follow should be a component on `Main Camera` (already in scene) — not a separate runtime singleton |

### 2.2 Insurance / panic-rescue patterns (run when scene authoring is missing)

| # | GameObject | File:line | Severity | Notes |
|---|---|---|---|---|
| 34 | `--- GAME MANAGERS ---` parent | `Assets/_Project/Scripts/Integration/RuntimeSpawnerInsurance.cs:22` | 🟡 SCENE-AUTHORING-GAP | Papers over missing scene authoring |
| 35 | `BuildingSpawner` (rescue) | `RuntimeSpawnerInsurance.cs:28` | 🟡 SCENE-AUTHORING-GAP | Per CLAUDE.md commit `e0766030`: this was restored after a sibling-agent regression. Insurance pattern by definition |
| 36 | `PlayerSpawner` (rescue) | `RuntimeSpawnerInsurance.cs:43` | 🟡 SCENE-AUTHORING-GAP | Same — scene has `PlayerSpawn` marker but if PlayerSpawner component is missing, rescue creates it |
| 37 | `Sun` (rescue Directional Light) | `Assets/_Project/Scripts/Integration/Moon1LightingSetup.cs:46` | 🟡 SCENE-AUTHORING-GAP | Scene already has `Sun_GoldenHour` + `Directional Light`. The rescue is dead code in practice |
| 38 | `AccentLight` per building | `Moon1LightingSetup.cs:110` | 🟡 SCENE-AUTHORING-GAP | Should be a child light authored on each building prefab |
| 39 | `Excavation_Sites` parent + `Excavation_Site_{1..N}` | `Moon1ExcavationSites.cs:35, :69` | 🔴 SHOULD-LIVE-IN-PREFAB | Excavation sites are gameplay interactables with fixed positions — author in scene |
| 40 | `Quest_Triggers` parent + N zones | `Moon1QuestTriggers.cs:37, :57` | 🔴 SHOULD-LIVE-IN-PREFAB | Quest trigger zones are static gameplay — author in scene |
| 41 | `Main Camera` (if absent) + `SimpleCameraFollow` | `Moon1PlayerSetup.cs:120, :155` | 🟡 SCENE-AUTHORING-GAP | Scene already authors `Main Camera`. Hot-fix path that forces tag + repositions player |

### 2.3 EchohavenContentSpawner (mega runtime builder)

The biggest single offender. Spawned itself was archived from `RuntimeSpawnerInsurance` (lines 38-42, commented out), but the component on `Moon1_Systems` still runs when added. Method `Spawn*` creates **dozens of GameObjects per call**.

| # | GameObject (sample) | File:line | Severity |
|---|---|---|---|
| 42 | `MoonFramework (Moon1 Runtime)` | `Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs:178` | 🟡 SCENE-AUTHORING-GAP — duplicates scene-baked `MoonFramework` |
| 43-55 | `ShovelPickup` + Handle/Shaft/Grip/Pommel/Blade/BladeMain/BladeEdge/BladeSideL/BladeSideR/Pommel | `EchohavenContentSpawner.cs:299, :308, :312, :317, :324, :332, :341, :345, :352, :360, :368` | 🔴 SHOULD-LIVE-IN-PREFAB — shovel is a single prefab |
| 56-60 | `--- DIG MOUNDS ---` parent + 12 × `MudMound_{c}_{i}` (each w/ Base/Top/ChunkL/ChunkR) | `EchohavenContentSpawner.cs:395, :414, :418, :425, :433, :441` | 🔴 SHOULD-LIVE-IN-PREFAB — but scene already has 4 baked `MudMound_0..3`; this is the runtime *extra* set |
| 61 | `AmbientAetherMotes` | `EchohavenContentSpawner.cs:503` | 🟡 SCENE-AUTHORING-GAP — particle prefab |
| 62-67 | `FoliageRoot` + `Grass` + N tufts + 3 blades each | `EchohavenContentSpawner.cs:534, :535, :559, :567, :575, :584` | 🔴 SHOULD-LIVE-IN-PREFAB — KayKit foliage prefabs exist |
| 68-72 | StarDome `Detail_AntennaSpire` + Base/Shaft/MidRing/Tip/Crystal | `EchohavenContentSpawner.cs:647-683` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 73-77 | 4 × buttresses, each w/ Base/Column/Capital/Band | `EchohavenContentSpawner.cs:698-727` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 78-83 | Fountain `Detail_OrbFinial` + Core/Shell/Ring1/Ring2/Ring3 | `EchohavenContentSpawner.cs:743-777` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 84-91 | Spire `Detail_CrystalCluster` + N shards w/ Base/Tip/Accent | `EchohavenContentSpawner.cs:792-822` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 92 | `Milo` + procedural Body/Head/Antenna hierarchy (~30 sub-GOs) | `EchohavenContentSpawner.cs:1011-1090` | 🔴 SHOULD-LIVE-IN-PREFAB — KayKit Milo prefab exists, this is the primitive fallback branch from CLAUDE.md rule#4 |
| 93 | `Cassian_MISSING_PREFAB` fallback | `EchohavenContentSpawner.cs:1160` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 94-98 | 5 × `AetherShard` w/ Core/Ring1/Ring2/Ring3/Ring4 | `EchohavenContentSpawner.cs:1230-1264` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 99-110 | `--- ENV PROPS ---` parent + RuinedColumn (5 sub-GOs) + RubblePile + N Rocks (Main/Frag1/Frag2) + InscriptionStone (4 sub-GOs) | `EchohavenContentSpawner.cs:1334-1505` | 🔴 SHOULD-LIVE-IN-PREFAB — these are Moon 1 hero props |
| 111-115 | N × `CorruptionZone_*` w/ Base/Ring1/Ring2/Core | `EchohavenContentSpawner.cs:1557-1581` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 116 | `VFX` root + `Sky_Aurora` Instantiate | `EchohavenContentSpawner.cs:1650, :1662, :1678` | 🟡 SCENE-AUTHORING-GAP — VFX prefab already exists, parent should be authored |
| 117 | `Anastasia_MISSING_PREFAB` fallback + `AnastasiaGlow` | `EchohavenContentSpawner.cs:2632, :2693` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 118 | `KayKit_Scatter` foliage parent | `EchohavenContentSpawner.cs:2718` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 119 | `MudGolem` procedural fallback (~25 sub-GOs Torso/Head/Eye/etc.) | `EchohavenContentSpawner.cs:2067-2155` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 120 | `VFX_Burst` + `Nameplate` w/ Background/Frame/Glow/TextHolder | `EchohavenContentSpawner.cs:2303, :2347-2375` | 🟡 SCENE-AUTHORING-GAP |
| 121 | `--- DIG SITES ---` parent + N `DigMarker_*` (w/ Beam_VFX + DigLight + GroundRing_VFX) | `EchohavenContentSpawner.cs:2454, :2472, :2477, :2505, :2516` | 🔴 SHOULD-LIVE-IN-PREFAB |

### 2.4 BuildingSpawner (other mega runtime builder)

| # | GameObject (sample) | File:line | Severity |
|---|---|---|---|
| 122 | Per-building `triggerName` GameObject | `Assets/_Project/Scripts/Integration/BuildingSpawner.cs:143` | 🟡 SCENE-AUTHORING-GAP |
| 123 | Per-building `zoneName` child | `BuildingSpawner.cs:178` | 🟡 SCENE-AUTHORING-GAP |
| 124 | `restoreSparkleVFX` Instantiate + fallback `new GameObject(markerName)` | `BuildingSpawner.cs:207, :223` | 🟡 SCENE-AUTHORING-GAP |
| 125 | `AmbientVillage` root | `BuildingSpawner.cs:281` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 126-130 | N × Rock/Bush/Tree Instantiates from KayKit prefabs | `BuildingSpawner.cs:295, :311, :328` | ⚪ OK — they DO instantiate real prefabs, just at runtime; could be authored in scene for consistent positions |
| 131 | `StarDome_ModularComposite` root + walls + floor tiles + pillars + torches | `BuildingSpawner.cs:344, :408, :434, :457, :481` | 🔴 SHOULD-LIVE-IN-PREFAB — modular StarDome assembly should be one composite prefab |
| 132 | `Building_{id}` primitive fallback | `BuildingSpawner.cs:518` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 133 | `Building_{id}_Composite` root + scatter rocks | `BuildingSpawner.cs:566, :585` | 🔴 SHOULD-LIVE-IN-PREFAB |
| 134 | `sprayGO` fountain spray | `BuildingSpawner.cs:662` | 🟡 SCENE-AUTHORING-GAP |

---

## 3. Top 10 priority bakings (Editor menu work needed)

These rank by (a) impact on stability if a sibling agent flips a flag, (b) frequency of regression, (c) how much primitive-mesh fallback would be replaced by real KayKit prefabs (per CLAUDE.md late-night mandate rule#4).

1. **Replace EchohavenContentSpawner.SpawnMilo procedural fallback (`:1011-1090`) with the existing KayKit Milo prefab placed in scene.** The procedural primitive Milo is what tester screenshots show as "primitive scarecrow" — and the prefab already exists at `Prefabs/Characters/Milo.prefab`.
2. **Replace EchohavenContentSpawner.SpawnCassian / SpawnAnastasia primitive fallbacks (`:1160, :2632`) with scene-baked NPCs from `Moon1BuildOutNPCs` menu output.** Currently the build-out menu is never re-run, so the runtime fallback dominates.
3. **Bake the StarDome composite (`BuildingSpawner.cs:344-481`) as `Prefabs/Moon1/Buildings/StarDome.prefab`** and place once in scene at canonical position. Eliminates 30+ runtime Instantiates per Play.
4. **Bake the Moon1Braziers placements (`Moon1Braziers.cs:53, :76, :127`) as `Prefabs/Moon1/Brazier.prefab` and place N copies in scene** at fixed positions. Flame ParticleSystem + Light become inspector-tunable.
5. **Replace RuntimeHUDBuilder entirely with `Prefabs/UI/MainHUD.prefab`.** Lines 149-450 build Canvas/EventSystem/HUDController/ControlsHint/QuestToast/DialoguePanel from scratch — every UI layout regression goes through this file.
6. **Bake the 12 mud mounds (`EchohavenContentSpawner.cs:395-441`) as `Prefabs/Moon1/MudMound.prefab` × 12 instances in scene.** Scene already has 4 (`MudMound_0..3`); add 8 more authored copies and disable the runtime mound spawn.
7. **Bake Excavation_Sites (`Moon1ExcavationSites.cs:35-72`) and Quest_Triggers (`Moon1QuestTriggers.cs:37-60`) hierarchies into the scene file.** Both are static gameplay structure with fixed coordinates.
8. **Bake the AnastasiaRocker setpiece (`Moon1AnastasiaRocker.cs:47-166`) as `Prefabs/Moon1/AnastasiaRocker.prefab`** — rocking chair + ProximityTrigger + HumSource. Currently the entire setpiece including the character is built from primitives.
9. **Bake the MudPoolPuzzle setpieces (`Moon1MudPoolPuzzle.cs:53-128`) as `Prefabs/Moon1/MudPoolPuzzle.prefab` and place N per scene.** Tuning crystal nodes (114-128) are interactables — they belong in scene authoring.
10. **Strip the stale `Moon1HeroBuildingSpawner` missing-script entry from `Moon1_Systems` (scene line 1264 / 3048)** — class was deleted in `Moon1MasterBootstrap.cs:14` cleanup pass but the scene component reference was never cleaned. Use `Tartaria/6 Scene Tools/Clean Missing Scripts` menu.

---

## 4. Other key runtime-rescue patterns

| Script | Path | Severity | Rationale |
|---|---|---|---|
| **PlayerSpawner.Start** | `Assets/_Project/Scripts/Integration/PlayerSpawner.cs:41-85` | ⚪ OK | Spawns canonical Player prefab; CLAUDE.md confirms this is the right pattern |
| **Moon1PlayerSetup.WaitForPlayerAndConfigure** | `Assets/_Project/Scripts/Integration/Moon1PlayerSetup.cs:55-72` | 🔴 SHOULD-LIVE-IN-PREFAB | Forces player position post-spawn from a SerializeField vector + reconfigures camera. Camera follow + spawn position should be set on the prefab itself (or the scene-baked PlayerSpawn marker) — having both PlayerSpawner *and* Moon1PlayerSetup hot-patch the player is the kind of competing flow that breaks when one agent edits the wrong file |
| **MiloTutorialFlow Bootstrap** | `Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:104-135` | ⚪ OK | PlayerPrefs-gated, idempotent, per CLAUDE.md auto-bootstrap pattern is canonical |
| **Moon1NarrativeBeats Start spawns GiantSkeletonKey_1 at hardcoded (-40, 1.2, -20)** | `Moon1NarrativeBeats.cs:27` | 🔴 SHOULD-LIVE-IN-PREFAB | Key location is a gameplay-critical waypoint; hardcoded Vector3 means moving the StarDome later requires editing C# |
| **GameViewFocusFix** | `Editor/GameViewFocusFix.cs` | ⚪ OK | Editor-only hot-patch documented in CLAUDE.md |

---

## 5. Redundancy / competing bootstrap flows (multiple-cooks problem)

This is the most dangerous category — multiple scripts attempt to manage the same GameObject, and the order of operations decides who wins.

### 5.1 Three master "do everything" menus

| Menu | File | Conflict |
|---|---|---|
| `Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems` | `Moon1MasterBootstrap.cs:28` | Adds 12 components to `Moon1_Systems` |
| `Tartaria/0 ★ MASTER/Tier 1 — FBX + Terrain + Splats + Lighting` | `Moon1Tier1Master.cs:25` | Sequences 5 sub-menus for assets |
| `Tartaria/0 ★ MASTER/Run ALL Tiers (Everything)` | `Moon1AllTiersMaster.cs:23` | Sequences Tier 1 + VFX + Audio menus |

These three have **overlapping but inconsistent scopes**. `Bootstrap All Moon 1 Systems` does NOT call `Tier 1`; `Run ALL Tiers` calls Tier 1 but NOT Bootstrap. No single menu does both, so users routinely run one and skip the other → half-authored scene.

### 5.2 Duplicate post-process volume

- Scene file authors `PostProcessVolume` (in m_Name list).
- `Moon1PostProcessingPreset.cs:19` creates `Moon1_GoldenHour_Volume` at runtime regardless.
- Both end up alive at Play; whichever's priority wins gets visible.

### 5.3 Duplicate MoonFramework root

- Scene file authors `MoonFramework` GameObject.
- `EchohavenContentSpawner.cs:178` creates `MoonFramework (Moon1 Runtime)` at runtime via `AddComponent<MoonBeatRunner>()`. Two MoonFrameworks running in parallel.

### 5.4 ZoneController / EchohavenProgressionSystem — added twice

- `Moon1MasterBootstrap.cs:60, :61` (Editor) adds these to `Moon1_Systems`.
- `ZoneController.cs:54` and `EchohavenProgressionSystem.cs:47` ALSO have `[RuntimeInitializeOnLoadMethod]` Bootstrap that creates a separate GameObject + component if no instance is found.
- Race: if the scene Moon1_Systems instance hasn't woken yet when the static bootstrap runs, two instances exist briefly until OnEnable singleton guard kills the runtime one. Logs show this as harmless but it's a known noise source.

### 5.5 Three layers of player spawn / setup

- **Scene authoring**: `PlayerSpawn` marker GameObject (in m_Name list)
- **PlayerSpawner.Start** (`PlayerSpawner.cs:85`) — canonical, Instantiates player prefab
- **Moon1PlayerSetup.WaitForPlayerAndConfigure** (`Moon1PlayerSetup.cs:55`) — post-spawn force-position via SerializeField vector @ (0, 2, 15), force-tag, force-camera. Notes in comments record at least 2 sibling-agent regressions where the Z offset disagreed
- **RuntimeSpawnerInsurance.EnsureSpawners** (`RuntimeSpawnerInsurance.cs:43`) — if PlayerSpawner is missing, creates one

Four entry points to "where does the player end up". Per CLAUDE.md commit `14d5ecc4` the `Player` tag has been chronically broken because Moon1PlayerSetup waits 5s for a tagged player while PlayerSpawner sets the tag on the prefab.

### 5.6 Moon1BuildOut* (4 menus) vs procedural Moon1*BuildOut* runtime

- Editor `Moon1BuildOutBuildings.cs:102` bakes 3 hero buildings via PrefabUtility.
- Editor `Moon1BuildOutVillage.cs:59` bakes 9 village buildings.
- Runtime `BuildingSpawner.cs:344-481` ALSO assembles a StarDome from primitive walls/floors if the prefab refs are null.
- Runtime `EchohavenContentSpawner.cs:647-727` ALSO adds antennae + buttresses to a "dome" GameObject.

If the Editor menu was never run on this branch (and scene contents suggest it wasn't), the runtime path takes over and builds primitive-mesh approximations of buildings that the prefab pipeline already shipped.

### 5.7 Moon1HardOverrideDriver — superseded stub still present

`Moon1HardOverrideDriver.cs:3` is now a 3-line "superseded" comment marker. Class is gone, but `Moon1_Systems` may still hold a missing-script reference. See § 3 item 10.

---

## 6. What's actually baked into the Echohaven scene file

For reference, the **48 unique GameObject names** found in `Echohaven_VerticalSlice.unity` (Sprint 11 L8 census):

```
_FallSafetyFloor, _SpawnPlatform, --- MINI-GAMES ---, --- UI ---,
AccessibilityManager, AetherWellLight, APV_Global_Echohaven,
APV_Local_StarDome, APVScenarioController, CentralPlaza,
Directional Light, DissonanceLensOverlay, Echohaven_Lighting,
EchohavenCombatArena, EchohavenTerrain, FillLight, GroundPlane,
LeyLine_0, LeyLine_1, LeyLine_2, Light_Fountain, Light_Spire,
Light_StarDome, Main Camera, Moon1_Systems, MoonFramework,
MudMound_0, MudMound_1, MudMound_2, MudMound_3, PipeOrganMiniGame,
PlayerSpawn, PostProcessVolume, ProbeVolumePerSceneData,
QuestLogUI, RuntimePBRApplier, SkillTreeUI, SpawnMarker,
Sun_GoldenHour, Universal Render Pipeline/Lit, Village_Buildings,
Wall_East, Wall_North, Wall_South, Wall_West, WorkshopUIPanel,
WorldBoundary, WorldMapUI
```

Notable absences: no buildings (Cathedral, StarDome composite, Bob's Inn, Anastasia House, etc.), no NPCs (Milo, Anastasia, Lirael, Cassian, Bob), no foliage parent, no excavation sites, no quest triggers, no obelisk, no HUD canvas, no dialogue panel.

`Village_Buildings` is named but holds no PrefabInstance children — empty container.

---

## 7. Conclusion

The Echohaven scene file is **a skeleton**. Lighting + terrain + walls + UI shell + 4 mud mounds + `Moon1_Systems` host with stale missing-script references. Everything a tester sees in Play (HUD, NPCs, props, buildings, dig sites, foliage, fireflies, mud pools, braziers, boss visuals, Win/Lose UI, dialogue panels) is rebuilt from scratch on every `RuntimeInitializeOnLoadMethod` pass.

Per CLAUDE.md mandates:
- **2026-05-30 late-night rule#4** ("never use `GameObject.CreatePrimitive` without an immediate URP-safe fallback path... Better: don't use primitives at all"): violated by `EchohavenContentSpawner.SpawnMilo`, `BuildingSpawner.BuildStarDomeFromPrimitives`, `Moon1CombatDirector.ProceduralResetScout/MudGolem`, `Moon1AnastasiaRocker.BuildProceduralAnastasia`, `BossEncounterSystem.*VisualProxy` (×5).
- **2026-05-30 late-night rule#6** ("Visual asset wireup is part of building it out... MUST load that prefab via `AssetDatabase.LoadAssetAtPath<GameObject>` (Editor) or `Resources.Load<GameObject>` (Runtime)"): violated by every procedural spawn path that has a prefab field at `null` and falls through to primitives.
- **2026-06-01 parallel mandate**: every runtime-created GameObject is a sibling-agent breakage vector. The most-edited files in this repo's recent history (`PlayerSpawner.cs`, `Moon1PlayerSetup.cs`, `RuntimeSpawnerInsurance.cs`, `DialogueManager.AutoBootstrap`) are all on the runtime-rescue list.

The next concrete agent task should be **bake-and-prune**: run `Moon1MasterBootstrap` + all `Moon1BuildOut*` + `Moon1WireSpawnerPrefabs` once, save scene, then delete the matching `[RuntimeInitializeOnLoadMethod]` fallbacks from sources so the scene is the single source of truth.

---

*Sprint 11 Lane 8 · scene-authoring audit · base SHA `e0766030`.*
