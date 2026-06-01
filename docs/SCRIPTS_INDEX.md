# Scripts Index — TARTARIA Project
*Generated 2026-06-01 cleanup pass — single source of truth for what's where*

## Active Scripts (502 .cs files compile)

### `Assets/_Project/Scripts/Editor/` — 42 Moon1*.cs Editor menus

**Core (canonical workflow):**
- `Moon1MasterBootstrap.cs` — `Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems` — attaches 12 components to Moon1_Systems + auto-chains Wire-All
- `Moon1WireSpawnerPrefabs.cs` — `Tartaria/0 ★ MASTER/Wire ALL Scene Prefab Refs (full sweep, Blender-only)` — wires every spawner/VFX/AI prefab field from Blender folders
- `Moon1BlenderBatch.cs` — `Tartaria/4 Generate Art/Blender — Moon 1 (original 42 assets)` — runs all 60 Blender Python scripts, regenerates FBX + auto-imports as URP-Lit prefab variants
- `Moon1NavMeshBake.cs` — `Tartaria/6 Scene Tools/Bake NavMesh` — bakes walkable terrain
- `Moon1SceneCleanup.cs` — `Tartaria/8 Fix/Moon 1 Scene Cleanup (Missing Refs + Placeholders)` — strips broken refs

**Build-out menus (scene population):**
- `Moon1BuildOutBuildings.cs` — places 3 hero buildings (StarDome / Fountain / Spire)
- `Moon1BuildOutVillage.cs` — places 11 Blender village prefabs (cottages, inn, watchtower, etc.) — **REQUIRED to avoid brown-cube fallback from BuildingSpawner**
- `Moon1BuildOutNPCs.cs` — places Milo + Anastasia + Lirael + Cassian + Bob
- `Moon1BuildOutEnvironment.cs` — places 6 POIs
- `Moon1BuildOutProps.cs` — interactable village objects
- `Moon1BuildOutVegetation.cs` — grass + bushes
- `Moon1CathedralKitDressing.cs` — cathedral kit pieces + organ
- `Moon1WireTuningPedestals.cs` — wires 9 pedestals → 3 hero buildings

**Fix/repair menus:**
- `Moon1AddSceneCamera.cs` — `Tartaria/8 Fix/Add + Position Main Camera` — drops Main Camera at (0, 12, -18) when scene has none
- `Moon1MegaCleanup.cs` — deletes 5-item visible mess (3 placeholders + 6 wrong-Moon shells + 4 misaligned + 1 magenta Player fix)
- `Moon1NuclearRebuild.cs` — `Tartaria/8 Fix/☢ NUCLEAR — Empty Scene + Rebuild from Blender` — deletes 17 stale scene-root GameObjects + auto-runs Bootstrap → NavMesh → Save
- `Moon1FixRuntime.cs` — runtime fixes
- `Moon1KayKitPurgeAudit.cs` — `Tartaria/7 Diagnose/Audit KayKit character refs in scene` — confirms 0 KayKit refs remain
- `Moon1SceneRescue.cs` — dedupe + force player + camera follow

**Generators (asset pipeline):**
- `Moon1AssetGenerators.cs`, `Moon1PopulateAudioCueLibrary.cs`, `Moon1ProceduralLore.cs`, `Moon1ClimacticVFX.cs`, `Moon1TerrainGen.cs`, `Moon1TerrainSplats.cs`, `Moon1LightingBake.cs`

**Audits/verifiers:**
- `Moon1AcceptanceAudit.cs`, `Moon1CombatVerify.cs`, `Moon1PlaytestDiag.cs`

**Pipeline composition:**
- `Moon1AllTiersMaster.cs` — runs ALL tiers in one click
- `Moon1Tier1Master.cs` — Tier 1 alone

### `Assets/_Project/Scripts/Integration/` — runtime gameplay systems

**Moon 1 core systems (auto-attached by MasterBootstrap):**
- `Moon1QuestTriggers.cs`, `Moon1ExcavationSites.cs`, `Moon1PlayerSetup.cs`, `Moon1LightingSetup.cs`
- `TartarianHourCycle.cs` (17-hour day)
- `Moon1NarrativeBeats.cs` (cathedral eruption + skeleton hum + giant key #1)
- `Moon1DialogueBindings.cs` (3 yarn files → in-game events)
- `EchohavenContentSpawner.cs` (Blender prefab spawn driver — 3082 lines)
- `AnastasiaController.cs`, `LiraelController.cs`, `EchohavenProgressionSystem.cs`, `ZoneController.cs`

**Player/HUD/FTUE:**
- `Moon1FirstTimeHints.cs` — 4-stage one-shot prompts
- `Moon1CameraFollowPlayer.cs` — runtime camera follow (auto-bootstraps via RuntimeInitializeOnLoadMethod)
- `Moon1HardOverrideDriver.cs`, `Moon1GodMode.cs` — bypass paths for emergencies

**Subsystems:**
- `Moon1AudioAtmosphere.cs`, `Moon1Braziers.cs`, `Moon1MudPoolPuzzle.cs`, `Moon1ExcavationSites.cs`
- `Moon1CinematicMoments.cs`, `Moon1AnastasiaRocker.cs`, `Moon1VillagerAmbient.cs`
- `EchohavenCombatArena.cs`, `EchohavenObelisk.cs`, `CombatWaveManager.cs`
- `TuningPedestalLink.cs`, `Moon1Lifeline.cs`, `Moon1ProgressPersistence.cs`

### Other key folders
- `Scripts/AI/` — MudGolemAI, CrystalSentry, ShadowStalker, ResetScout
- `Scripts/Gameplay/` — TuningMiniGame, InteractableBuilding, GiantMode, PlayerCombatController
- `Scripts/Core/` — GameEvents, SaveManager, PlayerSentimentTracker, ServiceLocator
- `Scripts/Input/` — PlayerInputHandler, LogitechControllerSupport, InputProbeHUD
- `Scripts/Camera/` — TartariaCameraController (currently disabled — see Moon1PlayerSetup logs)
- `Scripts/UI/` — HUD, dialogue, inventory, quest log
- `Scripts/Data/` — ScriptableObject databases (EnemyData, ItemData, etc.)

## Archived (459 .cs.disabled — historical, do not edit)

| Folder | Count | Notes |
|---|---|---|
| `Scripts/Integration/_archived_legacy_2026_05_31/` | 76 | Loose .disabled files moved 2026-06-01 |
| `Scripts/Integration/_archived_2026_05_31_stub_deletions/` | 8 | Stub deletions from May audit |
| `Scripts/Integration/_archived_duplicates_2026_05_31/` | 6 | Duplicate files |
| `Scripts/AI/_archived_restored_2026_05_31/` | 4 | LLM-gutted files restored |
| `Scripts/Core/_deleted_2026_05_31/_archived_backups/` | 4 | Core scripts backed up before deletion |
| `Scripts/Input/_archived_bypass_drivers_2026_05_31/` | 8 | Bypass drivers from F310 troubleshooting |
| `Scripts/Editor/_archive/` | 38 | Prior session Editor menus |
| `Scripts/UI/_archive_2026_06_01/` | 6 | UI scripts |
| `Scripts/LiveOps/_archive_2026_06_01/` | 8 | LiveOps stubs |
| `Scripts/Testing/_archive_2026_06_01/` | 15 | Test scripts |
| `Scripts/Core/_archive_2026_06_01/` | 2 | Core backups |
| `Scripts/Examples/_archive_2026_06_01/` | 2 | Example files |

## Workflow (canonical sequence)

After ANY scene reset or compile change:
1. `Tartaria → 4 Generate Art → Blender — Moon 1` (if Blender script changed; takes ~5 min)
2. `Tartaria → 0 ★ MASTER → Bootstrap All Moon 1 Systems` (auto-wires)
3. `Tartaria → 1 Build → Build Out Moon 1 Village (9 Buildings)` (avoids brown-cube fallback)
4. `Tartaria → 6 Scene Tools → Bake NavMesh`
5. Ctrl+S
6. Play

If broken:
- Check `Library/Bee/tundra.log.json` for actual compile errors (sandbox-readable)
- `Tartaria → 7 Diagnose → Audit KayKit character refs in scene` to verify Blender-only
- `Tartaria → 8 Fix → ☢ NUCLEAR — Empty Scene + Rebuild` to start clean

---

*This index lives at `docs/SCRIPTS_INDEX.md`. Update when you add or move scripts.*
