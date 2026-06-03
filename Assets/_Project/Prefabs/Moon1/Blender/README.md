# Moon1/Blender — Blender-exported Prefabs (flat, pending categorization)

This directory is the import target for `tools/blender/gen_*.py` → `BlenderImportPostprocessor.cs`.

## Current state (Prefab Hygiene Sprint, 2026-06-03)

All 347 Blender-generated prefabs live flat here. Six category subfolders are scaffolded but empty:

- `NPCs/`
- `Props/`
- `Architecture/`
- `VFX/`
- `Audio/`
- `Plates/`

## Why the files are still flat

Many editor wireup scripts hardcode paths like `Assets/_Project/Prefabs/Moon1/Blender/<Name>.prefab`. Moving the files without updating every consumer would break Moon 1 wireup. See `docs/PREFAB_LAYOUT.md` for the full migration plan and the canonical per-file category.

## Affected consumers (must update in lockstep)

- `Assets/_Project/Scripts/Editor/Moon1WireSpawnerPrefabs.cs` (search arrays — additive, safe to extend)
- `Assets/_Project/Scripts/Editor/Moon1BuildOutVillage.cs` (`PREFAB_DIR`)
- `Assets/_Project/Scripts/Editor/Moon1BuildOutNPCs.cs` (hardcoded `BobInnkeeper`)
- `Assets/_Project/Scripts/Editor/Moon1CathedralKitDressing.cs` (`PathPipeOrgan`)
- `Assets/_Project/Scripts/Editor/Moon1HeroBuildingMeshReplace.cs` (`BlenderRoot`)
- `Assets/_Project/Scripts/Editor/EchohavenContentBaker.cs` (5 hardcoded `NewEntry` paths)
