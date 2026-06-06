# Moon 1 Asset Placement Audit — 2026-05-31 v2

Scope: read-only audit of `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (binary; verified via `strings`) against authored prefabs on disk under `Assets/_Project/Prefabs/...` and `Assets/_Project/Audio/Moon1_Lore/`.

Scene mtime: 2026-06-01 00:01. Five new BuildOut menus exist (`Moon1BuildOut{Buildings,NPCs,Props,Vegetation,Village,Environment}.cs`) but **none of their parent containers** (`Hero_Buildings`, `Village_Buildings`, `Echohaven_NPCs`, `Echohaven_Props`, `Echohaven_Vegetation`, `Echohaven_POIs`) appear in the scene strings. The current scene was populated by the older `Moon1*` runtime drivers (e.g. `Moon1Braziers`, `Moon1AnastasiaRocker`, `Moon1MudPoolPuzzle`, `Moon1AmbientCreatures`, `Moon1VillagerAmbient`), not by the new editor menus.

---

## 1. Hero buildings (target: 3)
- **On disk:** 3 placeholder prefabs (`Echohaven_StarDome`, `Echohaven_HarmonicFountain`, `Echohaven_CrystalSpire`) + 18 cathedral kit pieces under `Prefabs/Moon1/Cathedral/`.
- **In scene:** `Building_echohaven_stardome`, `Building_echohaven_harmonicfountain`, `Building_echohaven_crystalspire` all present, plus a rich cathedral interior shell (`Cathedral_Door`, `Cathedral_Facade`, `Cathedral_Interior`, `Cathedral_RoseWindow`, `Cathedral_Stairs`, `Pillar_Cathedral_L/R`, `Gargoyle_Cathedral_E/W`, `RoseWindow`, `WeatherVane_Cathedral`, dome rocks, gold ring, etc.) and fountain/spire shells (`Fountain_Interior`, `Fountain_Water`, `Sephiroth_Fountain`, `Zodiac_FountainFloor`, `Spire_AetherBase`, `Spire_Crown`, `Lunar_SpireFloor`). Placeholders sit beside the detailed runtime construction; no `Hero_Buildings` parent.
- **Verdict:** COMPLETE (legacy-driver authored; new `Moon1BuildOutBuildings` menu has NOT been run).

## 2. Village buildings (target: 11)
- **On disk:** 10 prefabs in `Prefabs/Moon1/Blender/`: VillageBakery, VillageCottageA/B/C, VillageInn, VillageMill, VillageSmithy, VillageWell, TownHall, Watchtower, VillagerSignpost. (Spec says 11 incl. VillagerSignpost — that's the 11th.) BobsInn is a separate hero/inn prefab.
- **In scene:** `BobsInn` + `Balcony_BobsInn` + `Dormer_BobsInn`, `VillageWell`, `VillagerSignpost`, smithy fragments (`Brick_Smithy`, `Horse_Smithy`, `LadderFolded_Smithy`, `Ouroboros_Smithy`, `RopeCoil_Smithy`, `Scaffold_Smithy`, `Strongbox_Smithy`), `Stove_Bakery`, `VillageArch_Entry`, 6 `VillageLantern_0..5`. NO `VillageCottageA/B/C`, `VillageMill`, `TownHall`, or `Watchtower` instances. No `Village_Buildings` parent container.
- **Verdict:** PARTIAL_NEEDS_MENU_RUN (~3 of 11 placed as full prefabs; rest are loose dressing fragments; `Moon1BuildOutVillage` menu has not been run).

## 3. Props / interactables
- **On disk:** All authored: 14 brazier candidates incl. `StoneFireBrazier`, `AnastasiaRockingChair`, `MudPoolBasin`+`MercurialPool`+`MudPoolResonancePad`, `Aether_A3/D4/E3_Crystal_*`, `LoreArtifact`+`LoreArtifactScroll`, `GiantSkeletonKey`+`SkeletonKeySlot`, `MercuryBallSpireHero`+`Finial`, `PureWaterFont`, `AncientStoneSign`+`WoodenSign`+`VillagerSignpost`, `WaveformPillar`+`PillarCapital`+`PillarCorinthian`+`PillarDoric`+`PillarIonic`+`SephirothPillarTrio`, `StoneCircle`+`CarvedStoneObelisk`+`CrackedFlagstone`+`StoneUrn`.
- **In scene:** 8 `Brazier_0..7` (target 14 — short 6), 1 `Anastasia_Chair`, 3 `MudPool_NE/NW/SW`, 3 `Crystal_0..2` + 2 `Small_CrystalCluster_9/21` (target 9 — short 4), 9 `TuningPedestal_0..8`, `GiantKeyClaw`, `Finial_Fountain`, `Ankh_CathedralWall`, `Pentagram_CathedralFloor`, `Lore_CarvedStone` + `Lore_MudPool_NE/NW` (~3 lore items; target ≥6 — short 3). No fallen-pillar `Pillar_Fallen_*` (only standing `Pillar_Cathedral_L/R`). No `PureWaterFont`, no MercuryBall-as-spire-finial named token, no dedicated lore-stone hex (6).
- **Verdict:** PARTIAL_NEEDS_MENU_RUN (all authoring exists; placement is incomplete; `Moon1BuildOutProps` not run).

## 4. NPCs (target: Player, Milo, Anastasia, Lirael, Cassian, Bob, +4 ambient = 9)
- **On disk:** `Characters/Player.prefab`, `Milo.prefab`, `Anastasia.prefab`, `Lirael.prefab`, `Cassian.prefab` + Blender prefabs `AnastasiaPrincess`, `BobInnkeeper`, `CassianCarter`, `Villager_GenericA` (only 1 ambient variant — short 3 distinct ambient villagers).
- **In scene:** `Player` + `PlayerSpawner`, `Milo_NearSpawn` + `MiloController` + `MiloSatchelPickup`, `Anastasia_Cathedral` + `AnastasiaController` + `AnastasiaBed/Lantern/NightStand/Rug/Stool`, `Lirael_AtFountain` + `LiraelController`, `Cassian_AtSpire` + `CassianNPCController`, `Bob_AtInn`, `Villager_AtWell`, plus `Moon1AmbientCreatures` and `Moon1VillagerAmbient` runtime spawners (`Donkey_Scholar`, `Raven_Cathedral`, `Owl_Cathedral_Beam`, `Dragonfly_MudPool`, `Frog_MudPool`).
- **Verdict:** COMPLETE for 6 named NPCs; PARTIAL for ambient villagers (1 prefab variant authored, 1 placed — additional Villager_GenericB/C/D missing on disk). No `Echohaven_NPCs` parent — placed by legacy drivers, not the new `Moon1BuildOutNPCs` menu.

## 5. Enemies (target: 2)
- **On disk:** `Characters/MudGolem.prefab`, `Enemies/Moon1_MudGolem/MudGolem.prefab`, and `Prefabs/Moon1/Blender/ResetScout.prefab` + `MudGolem.prefab`. No `ResetScout.prefab` under `Prefabs/Characters/` (lives in Blender folder only).
- **In scene:** `MudGolem_AtPool_NE`, `MudGolem_AtPool_NW` (2 placed), `ResetScout_Patrol_S`, `ResetScout_Patrol_SW` (2 placed). String `Enemies/MudGolem` confirms Resources path.
- **Verdict:** COMPLETE.

## 6. VFX prefabs (target: 4)
- **On disk:** All 4 present: `VFX_CathedralLightEruption`, `VFX_GiantModeBurst`, `VFX_SeventeenthHourBeam`, `VFX_SpirePlacementSparks`.
- **In scene:** GUID scan of all 4 prefab metas against the scene binary returned **0 references for every VFX prefab**. None are instanced in Echohaven; they will be Resources/Instantiate-loaded at runtime, or have not been hooked yet.
- **Verdict:** PARTIAL_NEEDS_MENU_RUN (authored, not placed — verify runtime spawner code or instantiate via VFX manager).

## 7. Audio
- **On disk:** 5 stingers at `Assets/_Project/Audio/Moon1_Lore/`: Cathedral_Restoration_Stinger.wav, Lirael_Lullaby_432Hz.wav, Milo_Blimey_Chime.wav, Reset_Scout_Taunt.wav, Skeleton_Hum_Prophecy.wav (target was 3, exceeded).
- **AudioCueLibrary.asset:** `cues: []` — empty. None of the 5 stingers are registered in the library.
- **Verdict:** PARTIAL_NEEDS_MENU_RUN (authoring complete; library wireup missing).

## 8. Vegetation / environment
- **On disk:** KayKit Forest prefabs (bushes A–G x several, grass A–D, rocks 1/2/3 with many variants, trees 1–4 + bare trees 1–2) at `Assets/_Project/Prefabs/Props/KayKit/Forest/`. Custom blender trees: `AncientSequoia`, `BirchTree`, `WillowTree`, `BigMushroomTree`, `TreeStump`, `WorldTreeSmall`.
- **In scene:** 16 `Perimeter_*Tree_*` (Oak/Pine/Birch/Hawthorn/Willow), 60 `Prop_(Tree|Bush|Rock|Grass)_*` Color1 KayKit instances. `WorldTree_S` present. No `Echohaven_Vegetation` parent.
- **Verdict:** COMPLETE (legacy environment builder authored 76+ vegetation instances; new `Moon1BuildOutVegetation` menu has NOT been run, but coverage is already deep).

---

## Roll-up

| Category | Authored | Placed | Verdict |
|---|---|---|---|
| 1 Hero buildings | 3 + 18 kit | 3 hero + cathedral shell | COMPLETE |
| 2 Village buildings | 10 prefabs | ~3 of 11 + dressing fragments | PARTIAL_NEEDS_MENU_RUN |
| 3 Props / interactables | All authored | Braziers 8/14, Crystals 5/9, Lore 3/6+, 9 tuning pedestals; no fallen-pillar/font instances | PARTIAL_NEEDS_MENU_RUN |
| 4 NPCs | 6 named + 1 ambient | 6 named + 1 villager + 5 critters | COMPLETE (named); MISSING_AUTHORING for 3 extra ambient villager variants |
| 5 Enemies | MudGolem + ResetScout | 2 + 2 placed | COMPLETE |
| 6 VFX prefabs | 4 | 0 placed (no GUID refs in scene) | PARTIAL_NEEDS_MENU_RUN |
| 7 Audio | 5 stingers (target 3) | AudioCueLibrary `cues: []` | PARTIAL_NEEDS_MENU_RUN |
| 8 Vegetation | KayKit pack | 76+ instances | COMPLETE |

## Critical follow-ups (in priority order)
1. Run `Tartaria → Moon 1 → BuildOut Village` to place the 7 missing village prefabs (Cottages A/B/C, Mill, TownHall, Watchtower) under `Village_Buildings`.
2. Run `Moon1BuildOutProps` to lift Brazier count from 8 to 14, Crystal count from 5 to 9, and stamp fallen pillars + lore stones + PureWaterFont + Giant Skeleton Key #1.
3. Wire the 5 Moon 1 lore stingers into `AudioCueLibrary.asset` (currently `cues: []`).
4. Author `Villager_GenericB/C/D` Blender prefabs to give `Moon1VillagerAmbient` real variety beyond GenericA.
5. Hook the 4 Moon 1 VFX prefabs — either instantiate-on-event from `Moon1BeatScheduler` or pre-place under a `VFX_Stage` parent; current scene has zero references.
6. New BuildOut menus have not been executed against this scene — confirm the menus are idempotent before running, since the legacy `Moon1*` driver components already populate overlapping content.
