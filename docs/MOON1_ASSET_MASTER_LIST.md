# MOON 1 — Visual Asset Master List (2026-06-07)

> Deep-dive inventory of every authored Moon 1 visual asset on disk + what should be in scene YAML.

## A. Hero Buildings (4)

| Prefab | FBX | Material | Scene-authored? |
|---|---|---|---|
| `Prefabs/Moon1/CathedralFacade.prefab` | `Models/Blender/Moon1/CathedralFacade.fbx` | `Materials/Moon1/CathedralStone.mat` | partial (Echohaven_Cathedral wrapper exists) |
| `Prefabs/Moon1/Blender/Architecture/Apothecary.prefab` (and BellTowerNarrow, DistillationTower, DomedRotunda, GrandBellTower, Lighthouse, Mausoleum, ObservatoryDome, etc — over 50 architecture pieces) | various | various | ❌ NOT in scene |
| `Prefabs/Moon1/CrystalSpire.prefab` | `Models/Blender/Moon1/CrystalSpire.fbx` | `CrystalShardBlue.mat`, `CrystalShardWhite.mat`, `CrystalSpireBaseStone.mat` | ✅ in scene as Echohaven_CrystalSpire |
| `Prefabs/Moon1/MercurySpire.prefab` | `Models/Blender/Moon1/MercurySpire.fbx`, `MercuryBallSpireHero.fbx` | `Mercury.mat` | ✅ in scene as Echohaven_BuriedBeacon |

## B. Village Buildings (9 + supporting structures)

| Building | FBX | Authored in scene? |
|---|---|---|
| VillageInn | `VillageInn.fbx`, `BobsInn.fbx`, `Prefabs/.../BobsInn.prefab` | ✅ VillageInn empty (no FBX ref yet) |
| VillageBakery | `VillageBakery.fbx` | ✅ VillageBakery empty |
| VillageCottage A/B/C | `VillageCottageA.fbx`, `VillageCottageB.fbx`, `VillageCottageC.fbx` | ✅ VillageCottageA/B/C empty |
| VillageMill | `VillageMill.fbx`, `WindmillTower.fbx` | ✅ VillageMill empty |
| VillageSmithy | `VillageSmithy.fbx` | ✅ VillageSmithy empty |
| VillageTownHall | `TownHall.fbx` | ✅ VillageTownHall empty |
| VillageWatchtower | `Watchtower.fbx` | ✅ VillageWatchtower empty |
| VillageApothecary | `Apothecary.fbx` | ✅ VillageApothecary empty |
| Bonus: VillageBell, VillageWell, VillageFountain | individual FBXs + prefab | partial |

## C. NPCs (10 characters + ~25 animals + villagers)

| NPC | FBX | Prefab | Authored? |
|---|---|---|---|
| Milo (boy) | `MiloBoy.fbx`, `MiloSatchelAndLantern.fbx` | `Prefabs/Characters/Milo.prefab` + `Prefabs/Moon1/Blender/NPCs/MiloBoy.prefab` | partial (runtime spawned) |
| Anastasia (princess) | `AnastasiaPrincess.fbx`, `AnastasiaRockingChair.fbx` | `Prefabs/Characters/Anastasia.prefab`, `Prefabs/Moon1/Blender/NPCs/AnastasiaPrincess.prefab` | partial |
| Lirael (spectral architect) | `LiraelGuardian.fbx` | `Prefabs/Characters/Lirael.prefab`, `Prefabs/Moon1/Blender/NPCs/LiraelGuardian.prefab` | runtime spawned |
| Cassian (carter) | `CassianCarter.fbx` | `Prefabs/Characters/Cassian.prefab`, `Prefabs/Moon1/Blender/NPCs/CassianCarter.prefab` | runtime spawned |
| Bob (innkeeper) | `BobInnkeeper.fbx` | `Prefabs/Moon1/Blender/NPCs/BobInnkeeper.prefab` | ❌ NOT placed |
| Cathedral Choir Spirit | `CathedralChoirSpirit.fbx` | prefab | ❌ NOT placed |
| Villager_GenericA | — | `Prefabs/Moon1/Blender/NPCs/Villager_GenericA.prefab` | ❌ NOT placed |
| Animals: Butterfly, Donkey, Dragonfly, Eagle, Fish (Bass/Koi/Trout), Frog, Horse, Mule, Owl, Ox, Ram, Raven, Sparrow, Turtle, Wolf | various | various prefabs | ❌ NOT placed (huge content gap) |

## D. Enemies (2 + special)

| Enemy | Prefab | Authored? |
|---|---|---|
| Mud Golem | `Resources/Enemies/MudGolem.prefab` (combat MBs), `Prefabs/Characters/MudGolem.prefab`, `Prefabs/Moon1/Blender/NPCs/MudGolem.prefab` | partial (3 MudGolem_Spawn markers in scene) |
| Reset Scout | `Resources/Enemies/ResetScout.prefab`, `Prefabs/Moon1/Blender/NPCs/ResetScout.prefab` | runtime spawned |
| Special: HollowKnight, ResonanceDrone, ResonanceLion, SerpentLarge, ShadowStalker | various prefabs | ❌ NOT placed |

## E. Props (162 items — sample)

**Aether crystals (3 variants):** Aether_A3_Amber, Aether_D4_PaleGreen, Aether_E3_BlueIce

**Containers:** BarrelLarge, BarrelSmall, BasketWoven, BigMortar, CartFull, CartWagon, CartWheel, ChestStudded, ClayUrn, MetalBucket, GrainSack, WoodenBarrel, WoodenCrate

**Furniture:** AnastasiaRockingChair, Bookshelf, FireplaceHearth, LongBench, LongDiningTable, NightStand, PeasantChair, TableLantern, ThreeLeggedStool, WoodenBed, WoodenLectern

**Lighting:** CandelabraTriple, CandleHolderTable, CandleHolderWall, HangingLantern, LanternPost, TableLantern, TorchOnPost, WallSconceIron

**Combat:** AnvilHorn, ArmoredBootsPair, ArrowBundle, BattleAxe, Bow, BreastplateFull, BreastplateLamellar, Anvil

**Alchemy:** AetherVial, Alembic, BeakerLarge, BeakerMed, BeakerSmall, Bellows, BigMortar, BrewingRack, Cauldron, ResonanceTuningFork

**Lore:** GiantSkeletonKey, LoreArtifactScroll, LoreStone369, CarvedStoneObelisk, AncientStoneSign

**Misc:** BalloonBasket, MarketStall, VillagerSignpost, VillageBell, VillageFountain, VillageWell, WindmillTower, WoodenFence

## F. Mini-game Pedestals (4 variants + supporting)

| Variant | Prefab | FBX |
|---|---|---|
| A — Frequency Slider | `Prefabs/Moon1/Blender/Architecture/FrequencySliderStand.prefab` | `FrequencySliderStand.fbx` |
| B — Waveform Trace | `Prefabs/Moon1/WaveformPedestal.prefab` | `WaveformPedestal.fbx`, `WaveformPillar.fbx` |
| C — Harmonic Pattern | `Prefabs/Moon1/HarmonicPedestal.prefab` + 3 tile variants (Flower, Spiral, Square) | `HarmonicPedestal.fbx`, `HarmonicTile_*.fbx` |
| D — Cymatic Water | `Prefabs/Moon1/Blender/Plates/CymaticTray.prefab`, `CymaticGardenBed.prefab` | `CymaticTray.fbx` |
| Common: TuningPedestal | `Prefabs/Moon1/Blender/Plates/TuningPedestal.prefab` | `TuningPedestal.fbx` |

## G. VFX (10 ambient particle systems)

AetherLantern, AuroralBeacon, AuroralRing, CavernWallCrystals, CloudPlatform, FloatingTome, GlowingFlowerPatch, KnowledgeColumnGlowing, MushroomBlueGlow, StalactiteCluster

## H. Vegetation (trees, plants)

OakTree, PineTree, BirchTree, AncientSequoia, BigMushroomTree, BushClump, CattailReed, GrassPatch, ferns + bushes

## I. Audio Source Prefabs (20 instruments)

Bagpipe, Didgeridoo, Fiddle, Flute, GlassArmonica, Gong, HandDrum, Harp, Kalimba, Lute, MusicBox, Ocarina, Rattle, ResonanceTuningFork, Tambourine, Theremin, TuningBell_High/Low/Mid, TuningForkLarge_D4

## J. UI Prefabs (HUD, menus)

`Resources/Prefabs/UI/HUD_Root.prefab` (~325KB, 92 canvas refs) — confirmed real

---

## CRITICAL FINDING

**The scene YAML references parent containers (Village_Buildings, Moon1_Systems) but does NOT instantiate the FBX/prefab CONTENT into them.** Most of the 397 prefabs and 90 FBXs are baked + on disk but never placed in the scene. EchohavenContentSpawner.cs and EchohavenObelisk.cs spawn a subset at runtime via `new GameObject(...)`, but per NATRIX's mandate these should be authored into the YAML directly so the scene "looks complete" in Edit mode AND at scene load.

**The R97 obelisk fix was a band-aid for one runtime spawner. The real fix is moving 100+ NPC + prop + VFX + vegetation prefab instances from runtime spawners into the scene YAML as `PrefabInstance` blocks.**

## NEXT-SESSION ACTION ITEMS

1. **Cathedral interior:** instantiate `PipeOrganCathedral.fbx` + Bookshelf + WoodenLectern + CandelabraTriple inside Echohaven_Cathedral at (0, 1, 33)
2. **Village dressing:** for each of 9 cottages, place 2-3 props (Barrel, Cart, HangingLantern, Anvil for Smithy, BrewingRack for Apothecary, MarketStall x3 around plaza)
3. **NPC placement:** instantiate AnastasiaPrincess prefab at AnastasiaRocker spot, MiloBoy at spawn area, BobInnkeeper inside Inn, CassianCarter near gate, LiraelGuardian at z=42
4. **Mud pools:** 6 instances of MudPoolBasin around perimeter (currently empty MudPool_* markers)
5. **Vegetation:** 10 OakTrees + 8 PineTrees + 4 BirchTrees + 20 BushClump + 5 BigMushroomTree scattered
6. **Vibe props:** TorchOnPost x12 along path, HangingLantern x6 at building entrances, EchohavenBrazier x8 around hero buildings
7. **Animal wildlife:** Butterfly x4, Frog x3, Owl x2, Sparrow x6 (ambient life)
8. **Mini-game stations:** instantiate FrequencySliderStand, WaveformPedestal, HarmonicPedestal, CymaticTray at canonical 4-variant spots near Cathedral entrance
9. **VFX:** AetherLantern x4 along Cathedral path, GlowingFlowerPatch x6 in random spots
10. **Audio props:** TuningBell_High/Mid/Low x1 each at Cathedral altar; Lute + Flute prefabs near Bob's Inn

Each item above is a 5-line YAML `PrefabInstance` block referencing the prefab GUID. Approx 80 instances total = ~400 lines of YAML insertion. Achievable in 1 session focused on YAML authoring.
