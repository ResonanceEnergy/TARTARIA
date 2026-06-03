# Moon 1 Village Audit — 9 Buildings + Props

Date: 2026-06-03
Branch: agent/c/village-9-audit
Base SHA: d4c71a0e

## Spec reference

`docs/15_MVP_BUILD_SPEC.md` + CLAUDE.md 2026-06-03 NATRIX MANDATE: Moon 1 requires **9 village buildings** plus the 3 hero buildings:
Apothecary, Bakery, Cottage A, Cottage B, Cottage C, Inn, Mill, Smithy, TownHall, Watchtower.
(Spec wording "9 village" — counting CottageA/B/C as three distinct buildings = 10 names total. Punch list reads as the 10 names above.)

## Prefab existence post C.L1 migration

All architecture prefabs resolved under `Assets/_Project/Prefabs/Moon1/Blender/Architecture/`:

| Building       | Prefab path                                          | Resolved |
|----------------|------------------------------------------------------|---------:|
| Apothecary     | Architecture/Apothecary.prefab                       | ✅ |
| Bakery         | Architecture/VillageBakery.prefab                    | ✅ |
| Cottage A      | Architecture/VillageCottageA.prefab                  | ✅ |
| Cottage B      | Architecture/VillageCottageB.prefab                  | ✅ |
| Cottage C      | Architecture/VillageCottageC.prefab                  | ✅ |
| Inn            | Architecture/VillageInn.prefab (BobsInn also exists) | ✅ |
| Mill           | Architecture/VillageMill.prefab                      | ✅ |
| Smithy         | Architecture/VillageSmithy.prefab                    | ✅ |
| TownHall       | Architecture/TownHall.prefab                         | ✅ |
| Watchtower     | Architecture/Watchtower.prefab                       | ✅ |

Bonus assets shipped with C.L1 migration:
- `Architecture/VillageWell.prefab` — plaza landmark
- `Architecture/BobsInn.prefab` — second Inn variant
- `NPCs/VillagerSignpost.prefab` — south entrance signpost

## Placer (Moon1BuildOutVillage.cs)

Menu: `Tartaria/1 Build/Build Out Moon 1 Village (9 Buildings)`
File: `Assets/_Project/Scripts/Editor/Moon1BuildOutVillage.cs`

| Building       | Placer entry @ line | Status |
|----------------|--------------------:|--------|
| TownHall       | 48 | placed |
| VillageInn     | 49 | placed |
| VillageBakery  | 50 | placed |
| **Apothecary** | **51** | **ADDED this session (was missing)** |
| VillageWell    | 52 | placed |
| VillageMill    | 53 | placed |
| VillageSmithy  | 54 | placed |
| VillageCottageA| 55 | placed |
| VillageCottageB| 56 | placed |
| VillageCottageC| 57 | placed |
| Watchtower     | 58 | placed |
| VillagerSignpost | 59 | placed (NPC) |

Placer is idempotent (skips if instance already exists under `Village_Buildings` parent), terrain-snaps Y, faces buildings toward VILLAGE_CENTER (0,0,50).

## Prop scatter (Moon1VillagePropScatter.cs)

Menu: `Tartaria/1 Build/Moon 1 — Scatter Village Props`
File: `Assets/_Project/Scripts/Editor/Moon1VillagePropScatter.cs`

18 explicit `PlaceProp` calls + 5-iteration market bucket loop + 8-iteration FAE rock cluster loop = **31 prop instances** under `Moon1_VillageProps_Root`. KayKit FBX (anvil, hammer, lantern×6, bucket×6, blueprint, grindstone, mallet, compass, journal, mapRolled, pencil) + FAE RockCluster A/B. Pattern follows CLAUDE.md NO-PRIMITIVES rule (loads real FBX via `AssetDatabase.LoadAssetAtPath<GameObject>`).

## Runtime spawner reference

`EchohavenContentSpawner.cs` (Integration assembly) does **NOT** spawn village buildings — verified by `grep -i "village|cottage|smith|bakery|apothecary|watchtower|townhall"` returning 0 hits. Village buildings are baked into the scene as static GameObjects by the editor menu placer. Runtime spawner handles only NPCs / golems / shovel / rocks / foliage. This is by design per the editor-time pattern in CLAUDE.md.

## Action taken this session

- Added `Apothecary` to `Moon1BuildOutVillage.PLACEMENTS` at `(-40, 0, 45)` (west flank near the Inn), `face: true`. File `Moon1BuildOutVillage.cs:51`.

## Remaining for next session

1. Fire the placer menu in Unity to instantiate the 11 placements (now 12 with Apothecary) into `Echohaven_VerticalSlice.unity` and save the scene.
2. Re-bake NavMesh after placement (placer dialog prompts this).
3. Verify the prop scatter root is present and not deduped by Moon1SceneRescue.
