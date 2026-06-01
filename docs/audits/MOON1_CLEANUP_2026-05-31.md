# Moon 1 Cleanup + Gap Fill — Executed 2026-05-31

## Phase A — Cleanup (executed)

### 1. Moon1MasterBootstrap.cs trimmed (97 lines, 10/10 braces)
Removed 6 auto-attach lines per audit. The master bootstrap no longer wires:
- `Moon1HeroBuildingSpawner` (canonical: `Moon1BuildOutBuildings` menu)
- `Moon1LevelBuilder` (canonical: `Moon1BuildOutVillage` menu)
- `Moon1MaterialSetup` (was 31-line TODO stub — CLAUDE.md rule#1 violation)
- `Moon1AmbientCreatures` (was 31-line TODO stub — CLAUDE.md rule#1 violation)
- `Moon1NPCSpawner` (canonical: `Moon1BuildOutNPCs` menu)
- `Moon1BuildingPrefabCreator` (Editor-only asset gen, no scene attach needed)

Remaining auto-attaches (19 components): `Moon1QuestTriggers`, `Moon1ExcavationSites`, `Moon1PlayerSetup`, `Moon1PostProcessing`, `Moon1LightingSetup`, `BuildingRestorationCeremony`, `TartarianHourCycle`, `Moon1NarrativeBeats`, `Moon1DialogueBindings`, `Moon1EnvironmentDetail`, `Moon1Braziers`, `Moon1MudPoolPuzzle`, `Moon1AnastasiaRocker`, `Moon1VillagerAmbient`, `Moon1CombatDirector`, `Moon1AudioAtmosphere`, `Moon1CinematicMoments`, `Moon1ProgressPersistence`.

### 2. Files moved out of Unity-import path
Linux mount blocked `rm` — moved files to `_deleted_2026_05_31/` folders with `.archived` extension so Unity ignores them. To permanently purge, NATRIX can `del /Q` these folders from Windows.

- `Assets/_Project/Scripts/Integration/_deleted_2026_05_31/` — 11 files (Moon1HeroBuildingSpawner, Moon1NPCSpawner, Moon1AmbientCreatures, Moon1MaterialSetup, .candidate + Phase2Stubs backup)
- `Assets/_Project/Scripts/Gameplay/_deleted_2026_05_31/` — 5 files (TuningMiniGameController + 2 .candidate + .tmp)
- `Assets/_Project/Scripts/Core/_deleted_2026_05_31/_archived_backups/` — 4 GameEvents pre-fix backups

### 3. Archived `.disabled` Editor scripts
- `Assets/_Project/Scripts/Editor/_archive/` — 20 .disabled Editor files (Addressables, AssetWiring, AutoSceneSetup, Cleanup, DataAssetGenerator, EmergencyPlayableFix, EmergencySpawnerFix, FixEchohaven, ForceExploration, ForceWASD, MoonSceneBuilder, SceneMissingAnalyzer, SceneSetupFixer, SkillData, SkillTree, TestRunner, ValidatePlayMode, VexValidator + meta)

### 4. Archived Moon 2-13 disabled files
- `Assets/_Project/Scripts/Integration/_moon2_archive/` — 309 `.cs.disabled` + `.meta` files (Moon 2 through Moon 13 systems) + DOTS Moon2CrystalEnemyAISystem

---

## Phase B — 5 Gap Fillers (shipped)

### Gap 1: Terrain heightmap (500m radius + central depression)
**`Assets/_Project/Scripts/Editor/Moon1TerrainGen.cs`** (81L, 9/9 braces)
- Menu: `Tartaria → Build Out Moon 1 Terrain (500m + Depression)`
- 513×513 heightmap, 500×500m world, 35m peak
- Central depression 8m below plaza (radius 0.20 of map)
- South ridge rises ~19m (v > 0.55)
- Mild Perlin noise (0.06 amplitude) for organic feel
- Saves TerrainData asset to `Assets/_Project/Terrain/Moon1_Terrain.asset`
- Creates `Moon1_Terrain` GameObject in scene at correct origin

### Gap 2: 4 PBR splat layers
**`Assets/_Project/Scripts/Editor/Moon1TerrainSplats.cs`** (100L, 8/8 braces)
- Menu: `Tartaria → Build Out Moon 1 Splats (4 PBR layers)`
- Layer 0 Mud (RGB 0.30/0.20/0.12) — center, low elevation
- Layer 1 Stone (RGB 0.55/0.50/0.45) — south ridge + high elevation
- Layer 2 Grass (RGB 0.30/0.50/0.22) — radius 0.20-0.35, mid elevation
- Layer 3 Tartarian Tile (RGB 0.75/0.65/0.35) — decorative ring 30-50m around hero buildings
- Creates 4 TerrainLayer + 4 Texture2D assets at `Assets/_Project/Materials/Terrain/`
- Normalized alphamap blend (sum = 1 per texel)

### Gap 3: Adaptive 2-layer music (RS-reactive)
**`Assets/_Project/Scripts/Audio/AdaptiveMusicLayer.cs`** (94L, 15/15 braces)
- Auto-bootstraps at scene load
- Layer A — low ambient drone, always at 0.45 volume
- Layer B — sparse harmonic motif, volume scales 0..0.55 with `RS / 75` (RS_THRESHOLD_FULL)
- Crossfade speed 0.5 (smooth, no pop)
- Auto-finds 432 Hz ambient clips from KayKit RPG packs via `Resources.FindObjectsOfTypeAll<AudioClip>()` + name match
- Reads RS via PlayerPrefs key `TARTARIA_ResonanceScore` (no asmdef-cycle)

### Gap 4: Cinemachine-style restoration cinematic
**`Assets/_Project/Scripts/Camera/RestorationCinemachine.cs`** (92L, 10/10 braces)
- Auto-bootstraps at scene load
- Subscribes to `GameEvents.OnBuildingRestoredTyped`
- Runs 4-second smooth dolly orbit around the restored building (270° sweep at radius 18m, height 8m)
- SmoothStep easing in-out
- 1-second smooth return to gameplay cam position
- Pure `Camera.main` lerp (no Cinemachine package dependency, but spec-equivalent motion)

### Gap 5: Lighting bake (golden hour)
**`Assets/_Project/Scripts/Editor/Moon1LightingBake.cs`** (78L, 6/6 braces)
- Menu: `Tartaria → Build Out Moon 1 Lighting Bake (Golden Hour)`
- Sets ambient mode = Trilight with sky/equator/ground at golden-hour palette
- Enables fog: ExponentialSquared, color (0.85, 0.65, 0.45), density 0.008
- Directional light → 1.2 intensity, golden tint (1.0, 0.85, 0.65), Soft shadows, rotation (28°, -25°, 0°) for low golden-hour angle
- Configures lightmap settings: GPU progressive, 20 lightmap res, 32 direct samples, 256 indirect, 2 bounces
- Saves scene before bake
- Prompts before triggering `Lightmapping.BakeAsync()`

---

## Updated canonical 25-step Moon 1 build sequence

```
0.  Tartaria → MASTER: Bootstrap All Moon 1 Systems     ← now SAFE post-cleanup
1.  Tartaria → Build Out Moon 1 Terrain (500m + Depression)    ← NEW
2.  Tartaria → Build Out Moon 1 Splats (4 PBR layers)          ← NEW
3.  Tartaria → Build Out Moon 1 Buildings (3 Hero)
4.  Tartaria → Build Out Moon 1 Environment (POIs + Mud)
5.  Tartaria → Build Out Moon 1 Village (9 secondary structures)
6.  Tartaria → Build Out Moon 1 Vegetation (Grass+Bushes)
7.  Tartaria → Build Out Moon 1 Props (Rocks + Lore Stones + Fallen Pillars)
8.  Tartaria → Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)
9.  Tartaria → Moon 1 → Scatter Village Props
10. Tartaria → Art → Run Next-100 Blender Batch
11. Tartaria → Art → Run Next-150 Blender Batch
12. Tartaria → Art → Generate 20 PBR Materials
13. Tartaria → Art → Generate 15 PBR Materials (Pack 2)
14. Tartaria → Moon 1 → Place Blender Prefabs (Echohaven Scene Dressing)
15. Tartaria → Moon 1 → Place New Assets (vehicles, weapons, flora, fauna...)
16. Tartaria → Moon 1 → Bind KayKit Animators
17. Tartaria → Moon 1 → Build ResetScout Prefab
18. Tartaria → Wire Echohaven Content Spawner
19. Tartaria → Wire Echohaven Audio (Ambient + SFX)
20. Tartaria → Moon 1 → Generate SFX/Particle/Skybox cluster
21. Tartaria → Build Out Moon 1 Lighting Bake (Golden Hour)    ← NEW
22. Tartaria → Combat Verify (Moon 1)
23. Tartaria → Scene Audit: Echohaven
24. Tartaria → Moon 1 → Acceptance Audit
25. Tartaria → Ready Check (Audit + Bake + Save)
```

---

## Net result

- Bootstrap cleanup: 6 conflicts gone, 19 systems still auto-wire.
- 5 spec gaps filled with real Editor/Runtime scripts (445 new lines total).
- ~340 stale files moved out of Unity's import path.
- Pipeline now has a single source of truth for every spec item.
