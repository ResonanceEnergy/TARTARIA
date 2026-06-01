# Moon 1 Scene Composition Audit — Echohaven_VerticalSlice

**Date:** 2026-05-31 late. Scene is binary-serialized (601 KB). Names via `strings -n 4`. Spec refs: `docs/15_MVP_BUILD_SPEC.md`, `Moon1BuildOut*.cs` editor menus as authoritative placement spec.

## 1. Spawn area

Present: `PlayerSpawner`, `PlayerSpawn`, `_SpawnPlatform`, `SpawnRing` (x3), `SpawnMarker`, `Player`.
**Missing:** any worldspace tutorial signage GameObject (e.g. `TutorialSign_WASD`). Only the runtime `TutorialSystem` manager exists.

## 2. Cathedral / hero buildings

Present: `Building_echohaven_stardome`, `Building_echohaven_harmonicfountain`, `Building_echohaven_crystalspire` + children (Cathedral_Door, Cathedral_Facade, Cathedral_Interior, Cathedral_Stairs, Cathedral_RoseWindow, StarDome_AetherGlow, StarDome_GoldRing, Fountain_Water, Fountain_Interior, Spire_AetherBase, Spire_Crown) + lights (Light_StarDome, Light_Fountain, Light_Spire).

**LEFTOVERS that SceneCleanup should have removed:**
- `StarDome_Placeholder`
- `HarmonicFountain_Placeholder`
- `CrystalSpire_Placeholder`

`Tartaria → Moon 1 → Scene Cleanup` did NOT dedupe these. Re-run required.

## 3. Village square (south)

Spec from `Moon1BuildOutVillage.cs` = 10 placements.

| Object | Status |
|---|---|
| `TownHall` | **MISSING** |
| `VillageInn` | **MISSING** |
| `VillageBakery` | **MISSING** |
| `VillageWell` | Present |
| `VillageMill` | **MISSING** |
| `VillageSmithy` | **MISSING** |
| `VillageCottageA/B/C` | **MISSING (all 3)** |
| `Watchtower` | **MISSING** |
| `VillagerSignpost` | Present |

Extras present: `VillageArch_Entry`, `VillageLantern_0..5` (6), `Villager_AtWell`. Prop scatter ran; building placement did not. **8 of 10 spec'd village buildings absent.**

## 4. POIs

| Object | Status |
|---|---|
| `MudPool_NW`, `MudPool_NE`, `MudPool_SW` | All 3 present |
| `MudMound_0..3` | Present (4) |
| `MudGolem_AtPool_NW`, `_NE` | Present (2 of 3) |
| `CarvedStone`, `SkeletonAtCarvedStone` | Present |
| `POI_Overlook` | **MISSING** |
| `POI_RootChamber` | **MISSING** |

`Moon1BuildOutEnvironment` has not been run since scene last saved.

## 5. Vegetation

16 unique placed `Perimeter_*Tree_*` (Oak, Pine, Willow, Birch, Hawthorn, Cypress, DeadOak) + 30+ KayKit prop references (`Prop_Tree_*`, `Prop_Bush_*`, `Prop_Grass_*`) + `WorldTree_S` landmark. **CLAUDE.md "120 vegetation instances" claim is inflated** — only 16 explicit perimeter placements baked.

## 6. Lighting

All Present: `Sun_GoldenHour`, `FillLight`, `AetherWellLight`, `APV_Global_Echohaven`, `APV_Local_StarDome`, all per-building lights. **Most complete tier.**

## 7. Audio sources

`AudioListener`, `AudioManager`, `Moon1AudioAtmosphere` manager Present. `CathedralChoirSpirit_Inside` placed.
**NOT placed as GameObjects:** `Ambient_HarmonicChoir`, `Ambient_Wind`, `Building_Hum`. Created at runtime by Moon1AudioAtmosphere — acceptable but spec'd GameObjects are not baked.

## 8. NavMesh

**2 × `NavMeshSurface`** components + `NavMeshData` + `NavMeshObstacle`. Baked.

## 9. Camera + post

`MainCamera`, `Camera`, `CameraRig`, `PostProcessVolume` all Present.

## 10. UI

Present: `HUDController`, `UIManager`, `WorldMapUI`, `QuestLogUI`, `DialogueManager`, `DialogueTreeRunner`, `RuntimeHUDBuilder`.
Not detected as discrete GOs: Compass, Inventory UI.

---

## Missing-script entries (legacy components, archived sources)

Scene serialization still references 4 component class names whose source classes are archived at `Assets/_Project/Scripts/Integration/_deleted_2026_05_31/Moon1*.cs.archived`. Each one fires a yellow "missing script" warning at scene load — these are the init errors the Error Pause trap in `CLAUDE.md` warns about:

- `Moon1NPCSpawner`
- `Moon1AmbientCreatures`
- `Moon1MaterialSetup`
- `Moon1HeroBuildingSpawner`

Likely attached to `Moon1_Systems` and/or `Moon1HeroBuildingSpawner` / `Moon1_NewAssetsPlacements` parent GOs. `Tartaria → Clean Missing Scripts` has not been run since archival.

## Other notable GameObjects present

`AetherFieldManager`, `WorldBoundary`, `WorldChoiceTracker`, `WorldInitializer`, `QuestManager`, `LeyLineManager`, `LeyLine_0..2`, **`Brazier_0..7`** (8 braziers), **`TuningPedestal_0..8`** (9 pedestals — Moon 1 tuning mini-game variant A network), `Crystal_0..2`, `MiloSatchelPickup`, `AnastasiaLantern`, `Anastasia_Chair`, `AnastasiaBed`, `AnastasiaStool`, `AnastasiaRug`, `AnastasiaNightStand`, `Breastplate_OnStand`, `Donkey_Scholar`, `GiantKeyClaw`, `ChairA`, `ChairB`, `Moon1_Terrain`, `Moon1_Systems`, `Moon1_BlenderPlacements`, `Moon1_NewAssetsPlacements`.

**Polluting Moon 1 scene:** mini-game shell objects for OTHER Moons — `LeyLineProphecyMiniGame`, `PipeOrganMiniGame`, `AquiferPurgeMiniGame`, `BellTowerSyncMiniGame`, `CosmicConvergenceMiniGame`, `RailAlignmentMiniGame`. These belong in their own Moon scenes.

## Characters

Present: `Milo`, `Milo_NearSpawn`, `MiloController`, `Anastasia_Cathedral`, `AnastasiaController`, `Lirael_AtFountain`, `LiraelController`, `Cassian_AtSpire`, `CassianNPCController`. Full Moon 1 hero NPC set baked.

## Total GameObject count estimate

Binary serialization prevents exact count. Strings extraction = ~2,297 unique tokens, ~250–300 plausibly map to scene GameObjects. **Rough estimate: 200–280 GameObjects.**

## Top remediation priorities

1. `Tartaria → Moon 1 → Scene Cleanup` — drop 3 leftover `*_Placeholder` GOs.
2. `Tartaria → Clean Missing Scripts` — drop 4 archived-class component slots (root cause of Error Pause trap).
3. `Tartaria → Moon 1 → Build Out Village` — 8 of 10 buildings absent.
4. `Tartaria → Moon 1 → Build Out Environment` — adds `POI_Overlook` + `POI_RootChamber`.
5. Audit & remove 6 wrong-Moon mini-game shells polluting Moon 1 scene.
6. (optional) Place explicit Ambient_HarmonicChoir / Ambient_Wind / Building_Hum AudioSource GOs at fixed positions if procedural placement is undesirable.
