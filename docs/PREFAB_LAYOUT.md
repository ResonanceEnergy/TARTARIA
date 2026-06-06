# PREFAB_LAYOUT.md — Tartaria Prefab Conventions

> Authoritative layout for `Assets/_Project/Prefabs/` and `Assets/_Project/Resources/`. Established by the Prefab Hygiene Sprint (branch `agent/hygiene/prefabs`, 2026-06-03).

## Top-level conventions

### `Assets/_Project/Prefabs/MoonN/` — per-Moon content roots

Each Moon owns its content under `Prefabs/MoonN/`. Sub-categories below.

| Sub-folder | Contents |
|---|---|
| `MoonN/Buildings/` | Hero / village / landmark buildings authored for this Moon (e.g. `Echohaven_StarDome.prefab`, `Echohaven_HarmonicFountain.prefab`, `Echohaven_CrystalSpire.prefab`). |
| `MoonN/Blender/` | Blender-generated FBX-import prefabs (the `tools/blender/gen_*.py` pipeline writes here). Categorized further into `NPCs/`, `Props/`, `Architecture/`, `VFX/`, `Audio/`, `Plates/`. |
| `MoonN/Cathedral/` | Cathedral kit pieces specific to that Moon. |
| `MoonN/...` | Any other Moon-specific dressing / Variant Prefabs. |

`Moon2/` … `Moon13/` are scaffolded empty (with `.gitkeep`) and become populated as each Moon comes online.

### `Assets/_Project/Prefabs/MoonN/Blender/<Category>/`

Subdivisions of the Blender-generated content. Any new Blender export should land in the correct category:

| Category | Contents |
|---|---|
| `NPCs/` | Named characters (Anastasia, Cassian, Lirael, Milo, Bob, etc.), ambient villagers, fauna (Wolf, Horse, Owl…), creature classes (MudGolem, ResetScout, ShadowStalker, CrystalSentry, ResonanceDrone…). |
| `Architecture/` | Buildings, walls, pillars, arches, fences, statues, monuments, sacred-geometry floors, mosaics, fountains, structural ornament. Includes hero village buildings (`VillageInn`, `TownHall`, `VillageWell`, etc.) and ritual architecture (`StoneCircle`, `PentagramFloor`, `VesicaPiscisFloor`, `LeyLineNode`). |
| `Props/` | Movable items, dressing, tools (`Hammer`, `Anvil`, `Ladle`), weapons + armor (`LongSword`, `Crossbow`, `BreastplateFull`), furniture (`Bookshelf`, `PeasantChair`, `WoodenBed`), containers (`Crate*`, `Barrel*`, `Sack*`), flora (`OakTree`, `Fern`, `LotusFlower`), vehicles, kitchenware, scrolls, collectibles. Catch-all default bucket. |
| `VFX/` | Particle/effect prefabs that are visual-only (auroras, magical glows, floating elements). Does NOT include the runtime-loaded `Resources/VFX/Moon1/*` — those are canonical for `Resources.Load`. |
| `Audio/` | Sound-emitting prefabs and musical instruments (bells, tuning forks, drums, harps, lutes, drones, wind chimes). |
| `Plates/` | Interactive plates and tuning surfaces (`TuningPedestal`, `ResonancePlate`, `MudPoolResonancePad`, `CymaticGardenBed`, `SkeletonKeySlot`). |

> **MIGRATION COMPLETE (2026-06-03, branch `agent/c/blender-prefab-migrate`).** All 347 flat Blender prefabs were moved into their category subfolders. The six hardcoded path consumers were updated in lockstep: `Moon1WireSpawnerPrefabs.cs` (15 arrays got categorized paths prepended; legacy paths kept as fallbacks), `Moon1BuildOutVillage.cs` (Placement struct gained a `category` field), `Moon1BuildOutNPCs.cs` (`BobInnkeeper.prefab` → `NPCs/BobInnkeeper.prefab`), `Moon1CathedralKitDressing.cs` (`PathPipeOrgan` → `Architecture/PipeOrganCathedral.prefab`), `Moon1HeroBuildingMeshReplace.cs` (`PathPipeOrgan` → same), `EchohavenContentBaker.cs` (5 hardcoded `NewEntry` paths). Display-string references in `Moon1BlenderBatch.cs`, `Next100BlenderBatch.cs`, and `gen_npc_*.py` docstrings retain the `Blender/` root, which still exists. New Blender exports continue to land in the root via `BlenderImportPostprocessor.cs`; the import path was NOT updated by this sprint so authors can manually triage to a category — a future task may auto-route by filename match.

### `Assets/_Project/Prefabs/Props/Vendor/` — vendor / third-party assets

Vendor art (KayKit, Hovl, etc.) shared across Moons goes under a `Vendor/` sub-tree. Existing folders:

| Path | Vendor |
|---|---|
| `Prefabs/KayKit/` | KayKit Forest, KayKit RPGTools, etc. (top-level legacy location, still active). |
| `Prefabs/Characters/KayKit/Mannequin/` | Reference mannequins used by `Moon1AnimatorBinder.cs` — kept here because it's the only consumer. |

### `Assets/_Project/Prefabs/{Buildings, Characters, Collectibles, Enemies, Interactive, PowerUps, Props, UI, VFX, Shared}/` — cross-Moon buckets

These pre-existed; they hold cross-Moon shared prefabs (Player, Aether shards, etc.). Per-Moon content should NOT land here — it goes under `MoonN/`.

The `Prefabs/Buildings/` bucket previously held Moon 1 hero buildings (`Echohaven_*`); those have been moved into `Prefabs/Moon1/Buildings/` by this sprint.

## `Assets/_Project/Resources/` — runtime-loadable

`Resources.Load<T>(string)` resolves under `Assets/_Project/Resources/`. Any prefab that needs runtime loading by string MUST live here. Canonical sub-folders:

| Resources path | Active consumer |
|---|---|
| `Resources/Prefabs/Characters/KayKit/` | `EchohavenContentSpawner.cs` (Cassian, Anastasia), `Moon1VillagerAmbient.cs`, archived Moon spawners. |
| `Resources/Prefabs/Characters/KayKit/Skeletons/` | Archived Moon spawners (Moon2/3/4/7/12). |
| `Resources/Prefabs/Buildings/` | (reserved) |
| `Resources/VFX/Aurora.prefab` | `EchohavenContentSpawner.cs:1671`. |
| `Resources/VFX/Moon1/VFX_*.prefab` | `GiantMode.cs:130`, `Moon1CinematicMoments.cs:115`, `Moon1NarrativeBeats.cs:59`. |

### Rule: ONE canonical copy

If a prefab is loaded by `Resources.Load`, the canonical copy lives in `Resources/` — **do not** keep a shadow copy in `Prefabs/`. This sprint deleted the shadows that existed at:

- `Assets/_Project/Prefabs/Characters/KayKit/Char_{Barbarian,Knight,Mage,Ranger,Rogue,Rogue_Hooded}.prefab` (canonical in `Resources/Prefabs/Characters/KayKit/`).
- `Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_*.prefab` (canonical in `Resources/Prefabs/Characters/KayKit/Skeletons/`).
- `Assets/_Project/Prefabs/VFX/Aurora.prefab` (canonical in `Resources/VFX/`).
- `Assets/_Project/Prefabs/VFX/Moon1/VFX_*.prefab` (canonical in `Resources/VFX/Moon1/`).

## Author-time-only assets

If a prefab is referenced only via `AssetDatabase.LoadAssetAtPath` (Editor scripts, ScriptableObjects, scene references), it lives in `Prefabs/` — not in `Resources/`. This avoids paying the runtime memory cost for editor-only content.

## Pending migration — Moon1/Blender flat → categorized

The flat `Moon1/Blender/*.prefab` layout is preserved by this sprint to avoid breaking the hardcoded paths in the editor wireup chain. The follow-up sprint that migrates these files must, in one atomic commit:

1. `git mv` each `.prefab` + `.prefab.meta` into the category subfolder per the table below.
2. Update `Moon1WireSpawnerPrefabs.cs` — append new paths to each `string[] xxxSearch` array (the array-walk already supports fallbacks, so adding new paths is additive and safe).
3. Update `Moon1BuildOutVillage.cs` `PREFAB_DIR` to `Moon1/Blender/Architecture/` (and split the 11 placements that span Architecture + VillagerSignpost which is also Architecture).
4. Update `Moon1BuildOutNPCs.cs` hardcoded `BobInnkeeper.prefab` path.
5. Update `Moon1CathedralKitDressing.cs` `PathPipeOrgan` (→ `Audio/PipeOrganCathedral.prefab`).
6. Update `Moon1HeroBuildingMeshReplace.cs` `PathPipeOrgan`.
7. Update `EchohavenContentBaker.cs` 5 hardcoded `NewEntry` paths.

### Categorization map (347 prefabs)

For the migration, treat the following as the canonical category for each file:

- **NPCs (33):** AnastasiaPrincess, BobInnkeeper, Butterfly, CassianCarter, CathedralChoirSpirit, CrystalSentry, Donkey, Dragonfly, Eagle, FishBass, FishKoi, FishTrout, Frog, HollowKnight, Horse, LiraelGuardian, MiloBoy, Mule, MudGolem, Owl, Ox, Ram, Raven, ResetScout, ResonanceDrone, ResonanceLion, SerpentLarge, ShadowStalker, Sparrow, Turtle, VillagerSignpost, Villager_GenericA, Wolf.
- **Architecture (112):** AlabasterColumn, AnkhWallPlaque, Apothecary, ArchKeystone, Archway, BalconyRail, BannerPole, BastionGate, BellTowerNarrow, BobsInn, Bookshelf, BrickPile, CarvedStoneObelisk, CelestialOrrery, CitadelChimney, ClockFace, ClockworkArm, ClockworkGiantGear, ClockworkSmallGear, CrackedFlagstone, DistillationTower, DomedRotunda, Dormer, EchohavenBrazier, EyeOfProvidenceRelief, FencePanel, Finial, FireplaceHearth, FlyingButtress, FountainHead, FrequencySliderStand, GableEnd, GardenStatueCherub, Gargoyle, Gate, GrainSack, GrandBellLarge, GrandBellTower, Greenhouse, GridIntersection, HangingChain, HangingLantern, HarmonicTile_Flower, HarmonicTile_Spiral, HarmonicTile_Square, Hourglass, LeyLineNode, Lighthouse, LunarPhaseWheel, MercurialPool, MercuryBallSpireHero, MicroGiantPortal, NexusObelisk, Observatory, ObservatoryDome, OrphanChildBust, OrphanTrainCar, OuroborosRingLarge, Palanquin, PendulumWeight, PentagramFloor, PillarCapital, PillarCorinthian, PillarDoric, PillarIonic, PipeOrganCathedral, PlanetaryGrid, PlanetaryNexusGlobe, PureWaterFont, RailTrackSegment, RampartCannon, RoseWindowCymatic, RuinedColumn, RuinedFoundation, ScaffoldPiece, SephirothPillarTrio, SkyShrine, SkyTemple, SoldierStatue, Staircase, StarBeacon, StarFortBastion, StarMapTable, StoneCircle, StoneFireBrazier, StoneUrn, Sundial, Tapestry, TempleBellLantern, ThirteenthCrescendoOrb, TownHall, TriskeleTile, VesicaPiscisFloor, VictoryArch, VillageBakery, VillageCottageA, VillageCottageB, VillageCottageC, VillageInn, VillageMill, VillageSmithy, VillageWell, WallBanner, WallSconceIron, Watchtower, WaterGridChannel, WaveformPillar, WeatherVane, WhiteCitySpire, WindowStainedGlass, ZodiacWheel.
- **VFX (10):** AetherLantern, AuroralBeacon, AuroralRing, CavernWallCrystals, CloudPlatform, FloatingTome, GlowingFlowerPatch, KnowledgeColumnGlowing, MushroomBlueGlow, StalactiteCluster.
- **Audio (23):** Bagpipe, Didgeridoo, Fiddle, Flute, GlassArmonica, Gong, HandDrum, Harp, Kalimba, Lute, MusicBox, Ocarina, Rattle, ResonanceTuningFork, Tambourine, Theremin, TuningBell_High, TuningBell_Low, TuningBell_Mid, TuningForkLarge_D4, TuningForkMed_A3, TuningForkSmall_E3, WindChime.
- **Plates (8):** CymaticGardenBed, CymaticTray, MudPoolBasin, MudPoolResonancePad, ResonanceAltar, ResonancePlate, SkeletonKeySlot, TuningPedestal.
- **Props (162):** everything else — collectibles, weapons, armor, furniture, kitchenware, tools, vegetation, vehicles, books, lamps, etc. (default bucket).

## Quick recipes

- New Moon-1 building prefab: drop in `Prefabs/Moon1/Buildings/`.
- New Blender-exported NPC: drop in `Prefabs/Moon1/Blender/NPCs/` (the BlenderImportPostprocessor will route here after the pending migration; for now it lands in `Moon1/Blender/`).
- New runtime-loadable VFX: drop in `Resources/VFX/MoonN/` and reference via `Resources.Load("VFX/MoonN/VFX_Name")`.
- New editor-only diagnostic prefab: drop anywhere under `Prefabs/` and reference via `AssetDatabase.LoadAssetAtPath`.
