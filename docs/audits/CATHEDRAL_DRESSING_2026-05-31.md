# Cathedral Kit Dressing Audit — 2026-05-31

> Read-only audit of how the 18 Cathedral kit prefabs at `Assets/_Project/Prefabs/Moon1/Cathedral/` are (or aren't) dressed into the Dome interior of `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`.

## Method

The scene file is binary-serialized Unity 6. Two probes:

1. **Name probe** — `Grep` on the binary scene for every kit-piece prefab name (`Foundation_16x16m`, `Dome_Segment_*`, `Spire_*`, `Wall_4x4m_Stone`, `Wall_Corner_4x4m`, `Column_Ornate_6.5m`, `Archway_4x7m`, `Door_Grand_3x6m`, `RoseWindow_4x4m`).
2. **GUID probe** — extracted the 18 prefab guids from their `.meta` files and `Grep`ed the scene + the three hero building prefabs (`Echohaven_StarDome.prefab`, `Echohaven_HarmonicFountain.prefab`, `Echohaven_CrystalSpire.prefab`) for the guid strings.

Also surveyed every C# script that loads any of the 18 prefab paths.

## Results

### 1. Kit pieces placed in scene: **0 / 18**

Zero matches for any of the 18 kit-piece prefab names AND zero matches for any of the 18 prefab guids inside `Echohaven_VerticalSlice.unity` or inside the three hero building prefabs. The 18 Cathedral kit pieces are authored on disk but **never instantiated in the playable scene, and not embedded in the hero building prefabs either.**

### 2. Dome segment completeness: **0 / 8**

None of `Dome_Segment_N`, `_NE`, `_E`, `_SE`, `_S`, `_SW`, `_W`, `_NW` are placed. There is no full or partial dome rendered from the kit. The Dome appears in scene only as a single placeholder + the `Building_echohaven_stardome` GameObject (8 references in the binary).

### 3. Interior dressing — **PRESENT (5/5)**

The following sacred-geometry dressings ARE in the scene (1 occurrence each):

- `Pentagram_CathedralFloor`
- `Ankh_CathedralWall`
- `Eye_CathedralWall`
- `Sephiroth_Fountain`
- `Zodiac_FountainFloor`

Plus supporting cathedral atmosphere objects: `Pillar_Cathedral_L/R`, `Sconce_Cathedral_L/R`, `Chain_CathedralCeiling`, `Gargoyle_Cathedral_E/W`, `Gong_Cathedral`, `Owl_Cathedral_Beam`, `Raven_Cathedral`, `Palanquin_Cathedral`, `WallBanner_Cathedral_L/R`, `WeatherVane_Cathedral`, `CathedralChoirSpirit_Inside`. Plus higher-level placeholders `Cathedral_Facade`, `Cathedral_Interior`, `Cathedral_Door`, `Cathedral_RoseWindow`, `Cathedral_Stairs`. None of these are built from the 18-piece kit — they are independent placeholder GameObjects.

### 4. Spire completeness: **0 / 3**

`Spire_Base_2x2m`, `Spire_Mid_Taper`, `Spire_Top_MercuryBall` — none placed. The scene contains generically-named `Spire_AetherBase`, `Spire_Crown`, `Light_Spire`, `Lunar_SpireFloor`, `Finial_Fountain`, `Building_echohaven_crystalspire`, `CrystalSpire_Placeholder` instead. The Spire is dressed from custom GameObjects, not from the kit's three-piece spire stack.

### 5. Pipe Organ status: **MINIGAME LOGIC PLACED, VISUAL MODEL ABSENT**

- `PipeOrganMiniGame` GameObject (script + trigger): **present** (1 occurrence).
- `PipeOrganCathedral.prefab` (the real visual model at `Assets/_Project/Prefabs/Moon1/Blender/PipeOrganCathedral.prefab`): **not placed** — zero matches by name in the scene.

So the player can interact with the organ minigame, but there is no visible pipe-organ mesh at the interaction point.

## Why the kit isn't dressed

The only scripts that ever loaded the 18 kit prefabs by path were:

- `Assets/_Project/Scripts/Integration/_deleted_2026_05_31/Moon1LevelBuilder.cs.archived`
- `Assets/_Project/Scripts/Integration/_deleted_2026_05_31/Moon1HeroBuildingSpawner.cs.archived`

Both were archived during the 2026-05-31 cleanup (the same archival that left the 4 missing-script refs on `Moon1_Systems` flagged in `MOON1_FULL_AUDIT_2026-05-31.md` § A1). The only live reference to the kit in current code is read-only existence checks in `Moon1AcceptanceAudit.cs` plus a column-prefab borrow in `Moon1BuildOutProps.cs` for fallen-pillar decor.

`Moon1BuildOutBuildings.cs` (the current canonical Editor builder) does not reference any of the 18 kit prefabs by path.

## Verdict: **SHELL_ONLY**

The Cathedral in Echohaven is a name-only shell:

- Hero placeholders (`Building_echohaven_stardome`, `..._harmonicfountain`, `..._crystalspire`) plus duplicate primitive placeholders (`StarDome_Placeholder`, etc.) carry the silhouette.
- A handful of bespoke atmosphere objects (sconces, banners, gargoyles, sacred geometry) sit inside the placeholder hierarchy and ARE present (the 5 sacred-geometry pieces score a clean 5/5).
- The 18 modular Cathedral kit pieces on disk — Foundation, Walls, Columns, Archways, Door, Rose Window, 8 Dome segments, 3 Spire pieces — contribute **nothing** to the current scene. Dome 0/8. Spire 0/3. Foundation/Walls/Columns/Archway/Door/Rose all absent.
- The Pipe Organ centerpiece exists as a minigame trigger but has no visual model placed.

Per the CLAUDE.md 2026-05-30 mandate ("Buildings (real prefabs, not primitives)"), dressing the Dome interior from the 18-piece kit + placing `PipeOrganCathedral.prefab` are TIER A unfinished items. The archived `Moon1HeroBuildingSpawner.cs` would be the natural restore-and-fix path; alternatively, a new pass on `Moon1BuildOutBuildings.cs` to load + position the kit pieces around the existing `Building_echohaven_stardome` anchor.

---

*Audited 2026-05-31 by Claude Opus 4.7. Sources: `Echohaven_VerticalSlice.unity` (binary grep, name + guid probes), 18 prefab `.meta` files, full repo grep for kit-piece path references.*
