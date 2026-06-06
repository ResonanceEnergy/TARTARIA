# Prefabs Index — TARTARIA Project
*Generated 2026-06-01 — what's where, what's canonical*

## Top-level structure

```
Assets/_Project/Prefabs/
├── Buildings/              Hero buildings (StarDome, HarmonicFountain, CrystalSpire)
│   └── ModularDungeon2/    Separate modular dungeon kit
├── Characters/             Canonical character prefabs (script-attached)
│   └── KayKit/             Vendor character archetypes (Combat/Equipment refs ONLY, NOT used for Moon 1 NPCs)
├── Collectibles/           Aether shards, lore artifacts
│   ├── AetherShard/
│   └── LoreArtifact/
├── Enemies/                Per-enemy folders
│   └── Moon1_MudGolem/
├── Interactive/            Trigger volumes
│   └── TuningNode/
├── Moon1/                  Moon 1 specific prefabs
│   ├── Blender/            ★ 347 Blender FBX-derived prefab variants (CANONICAL Moon 1 visuals)
│   └── Cathedral/          Cathedral kit pieces
├── Moon2/ through Moon13/  Per-moon prefab folders (mostly sparse)
├── PowerUps/               Pickups
├── Props/                  General props
│   └── KayKit/             Vendor weapons + armor (used by Equipment system)
└── VFX/                    Particle effects
    └── Moon1/              Moon 1 VFX (GiantModeBurst, SpirePlacementSparks)
```

## Character prefab resolution (post-KayKit-purge)

For Moon 1 NPCs the spawner wires to **Blender prefabs first**, falling through to canonical Characters/:

| Character | Primary path | Fallback path |
|---|---|---|
| Milo | `Moon1/Blender/MiloBoy.prefab` | `Characters/Milo.prefab` |
| Cassian | `Moon1/Blender/CassianCarter.prefab` | `Characters/Cassian.prefab` |
| Anastasia | `Moon1/Blender/AnastasiaPrincess.prefab` | `Characters/Anastasia.prefab` |
| Lirael | `Moon1/Blender/LiraelGuardian.prefab` | `Characters/Lirael.prefab` |
| Bob | `Moon1/Blender/BobInnkeeper.prefab` | — |
| MudGolem | `Moon1/Blender/MudGolem.prefab` | `Characters/MudGolem.prefab` |
| ResetScout | `Moon1/Blender/ResetScout.prefab` | — |
| CrystalSentry | `Moon1/Blender/CrystalSentry.prefab` | `Characters/CrystalSentry.prefab` |
| ShadowStalker | `Moon1/Blender/ShadowStalker.prefab` | `Characters/ShadowStalker.prefab` |
| ResonanceDrone | `Moon1/Blender/ResonanceDrone.prefab` | — |
| PlayerHero | `Shared/Blender/PlayerHero.prefab` | `Characters/Player.prefab` |
| GiantGolem | `Shared/Blender/GiantGolem.prefab` | `Characters/MudGolem.prefab` |
| VoidPhantom | `Shared/Blender/VoidPhantom.prefab` | `Characters/ShadowStalker.prefab` |
| TemporalWraith | `Shared/Blender/TemporalWraith.prefab` | `Characters/ShadowStalker.prefab` |

`Moon1WireSpawnerPrefabs.cs` resolves these via `ResolveFirstExisting()` helper.

## Village buildings (avoid brown-cube fallback)

`Moon1BuildOutVillage.cs` Editor menu places these 11 Blender prefabs at hard-coded positions under a `Village_Buildings` parent:

| Building | Path | Position |
|---|---|---|
| VillageCottageA | `Moon1/Blender/VillageCottageA.prefab` | (-20, 0, 80) |
| VillageCottageB | `Moon1/Blender/VillageCottageB.prefab` | (0, 0, 80) |
| VillageCottageC | `Moon1/Blender/VillageCottageC.prefab` | (20, 0, 80) |
| VillageInn | `Moon1/Blender/VillageInn.prefab` | (varies) |
| Watchtower | `Moon1/Blender/Watchtower.prefab` | (varies) |
| Lighthouse | `Moon1/Blender/Lighthouse.prefab` | (varies) |
| GrandBellTower | `Moon1/Blender/GrandBellTower.prefab` | (varies) |
| DistillationTower | `Moon1/Blender/DistillationTower.prefab` | (varies) |
| Greenhouse | `Moon1/Blender/Greenhouse.prefab` | (varies) |
| BellTowerNarrow | `Moon1/Blender/BellTowerNarrow.prefab` | (varies) |
| VillageSmithy | `Moon1/Blender/VillageSmithy.prefab` | (varies) |

**Without running this menu, BuildingSpawner.cs falls back to brown `Cube.fbx` primitives at mud color `(0.45, 0.35, 0.25)`.** This is the root cause of "brown rectangles everywhere".

## Moon 1 Blender prefab inventory (347 total)

Categories:
- **Characters (16):** MiloBoy, MiloSatchelAndLantern, CassianCarter, AnastasiaPrincess, AnastasiaRockingChair, LiraelGuardian, BobInnkeeper, ResetScout, ShadowStalker, ResonanceDrone, CrystalSentry, HelmKnight, HollowKnight, CathedralChoirSpirit, MudGolem, VillagerSignpost
- **Buildings (11):** VillageCottage A/B/C, VillageInn, Watchtower, Lighthouse, GrandBellTower, DistillationTower, Greenhouse, BellTowerNarrow, VillageSmithy
- **Cathedral:** PipeOrganCathedral, WhiteCitySpire, BobsInn (also a building), CavernWallCrystals
- **Trees/Foliage (~30):** OakTree, PineTree, BirchTree, MagnoliaTree, HawthornTree, PalmTree, GoldenBoughTree, BigMushroomTree, Fern, MushroomCluster, MushroomBlueGlow, MushroomRed, HangingMoss, LeafPile, GlowingFlowerPatch, LotusFlower, Sunflower, etc.
- **Rocks (~11):** BoulderLarge/Med/Small, CrackedFlagstone, CarvedStoneObelisk, StoneCircle, ArchKeystone, AncientStoneSign, StoneUrn, StoneFireBrazier, AnkhWallPlaque
- **Crystals/Aether:** Aether_A3_Crystal_Amber, Aether_D4_Crystal_PaleGreen, Aether_E3_Crystal_BlueIce, CrystalCluster, CrystalHall, CrystalThrone, DissonanceCrystal (+ Black/Green/Red variants), CavernWallCrystals
- **Props (rest):** Shovel, GiantSkeletonKey, MudPoolBasin, MudPoolResonancePad, SkeletonKeySlot, SkeletonRemains, AnastasiaRockingChair, ClayUrn, CymaticTray, Bookshelf, CandelabraTriple, ArrowBundle, Sparrow, etc.

## Vendor prefabs (Props/KayKit) — kept for Equipment + Combat systems

Used by `EquipmentManager.cs`, `PlayerCombatController.cs`, `PlayerRanged.cs`:
- AdventurerGear/: Prop_arrow_bow, Prop_axe, Prop_dagger, Prop_shield, Prop_sword, etc.
- Stones/: BoulderLarge/Med/Small (also referenced in Moon1WireSpawnerPrefabs rock array)
- Skeletons/: Prop_Skeleton_Arrow + variants
- Tools/: Prop_shovel (used as PlayerSpawner's `kayKitShovelPrefab` slot)

**Do not delete the KayKit folder** — Combat/Equipment still references it. Only the CHARACTER references were purged (verified via `Moon1KayKitPurgeAudit.cs`).

## VFX prefabs

- `VFX/ScanPulse.prefab` — Aether Vision scan
- `VFX/RestoreSparkle.prefab` — restoration milestone
- `VFX/ShardCollect.prefab` — pickup
- `VFX/Moon1/VFX_GiantModeBurst.prefab` — Giant Mode activation
- `VFX/Moon1/VFX_SpirePlacementSparks.prefab` — Spire ceremony

Wired by `Moon1WireSpawnerPrefabs.cs` into `VFXWiringController` + `HitVFXController` + `CombatHitReactor`.

---

*This index lives at `docs/PREFABS_INDEX.md`. Update when you add or move prefabs.*
