# Master Bootstrap — canonical menu

Hammer Lane 6 (Phase 6.5) · 2026-06-02 · Sprint 11 L8 `50ff78ea`

## Problem

`Tartaria/0 ★ MASTER/` had three overlapping run-everything entries. From a fresh dev / beta-tester perspective it was unclear which one to fire to "boot a play session":

| File | Menu | Priority | Scope |
|---|---|---|---|
| `Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs` | `Bootstrap All Moon 1 Systems` | 30 | Scene bootstrap — attaches Moon1 Integration MonoBehaviours to `Moon1_Systems` GameObject, auto-runs prefab-ref wiring. |
| `Assets/_Project/Scripts/Editor/Moon1Tier1Master.cs` | `Tier 1 — FBX + Terrain + Splats + Lighting` | 20 | Asset-pipeline sequencer (Blender batches + heightmap + splats + bake). No scene wiring. |
| `Assets/_Project/Scripts/Editor/Moon1AllTiersMaster.cs` | `Run ALL Tiers (Everything)` | 10 | Superset that fires Tier 1 + VFX (Tier 2) + Audio (Tier 3). No scene wiring. Tier 4 UI auto-bootstraps at Play. |

Priorities 10 / 20 / 30 also reversed natural "biggest first" intuition for a Unity menu sort.

## Decision

**Canonical: `Moon1MasterBootstrap.cs`** — it is the only one of the three that produces a runnable scene state. It:

1. Creates / reuses `Moon1_Systems` GameObject.
2. Attaches 12 cross-system MonoBehaviours from the `Tartaria.Integration` namespace (TartarianHourCycle, Moon1NarrativeBeats, Moon1DialogueBindings, EchohavenContentSpawner, AnastasiaController, LiraelController, EchohavenProgressionSystem, ZoneController, etc.).
3. Auto-chains `Moon1WireSpawnerPrefabs.RunAll()` so spawners don't fall back to magenta primitives at runtime.
4. Idempotent — re-runs reuse the same GameObject and skip components already present.
5. Pruned per `docs/audits/MOON1_BUILD_AUDIT_2026-05-31.md` to avoid attaching the 7 stub/conflicting components flagged by that audit.

Tier 1 and ALL Tiers are asset-generation pipelines, not scene bootstraps. Keeping their menus alongside Bootstrap muddied the "what do I press to play?" question and risked beta testers / dev agents firing Tier 1 expecting a scene and getting a 3-8 minute Blender pipeline instead.

## Change

- `Moon1Tier1Master.cs:25` — `[MenuItem(...)]` commented out, marker comment added. Run logic preserved; call `Moon1Tier1Master.Run()` directly.
- `Moon1AllTiersMaster.cs:23` — `[MenuItem(...)]` commented out, marker comment added. Run logic preserved; call `Moon1AllTiersMaster.Run()` directly.
- `Moon1MasterBootstrap.cs` — unchanged. Remains the only `Tartaria/0 ★ MASTER/` entry that boots a scene.

Other `Tartaria/0 ★ MASTER/` entries are unaffected (BatchReadinessValidator, Moon1NavMeshBake, Moon1WireSpawnerPrefabs, OneClickBuild).

## How to fire the asset-gen pipelines now

If you want the old Tier 1 / ALL Tiers behaviour, write a one-line custom menu in your local fork or call the static `Run()` methods from another script. They were intentionally not deleted — only de-listed from the master menu.

## Rules followed

- WORKTREE_MANDATE: branch `agent/h/consolidate-master-menus` in `C:\dev\_wt_h_master_menus`.
- API_CONTRACT: every menu path / file is grep-cited above.
- NO-DEBT: no fabricated artifacts; superseded menus kept as commented `[MenuItem]` rather than deleted-then-restored.
- NO-STUBS: `Run()` bodies untouched.
