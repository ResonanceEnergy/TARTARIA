# NavMesh Coverage Audit — Echohaven_VerticalSlice

Date: 2026-05-31
Scope: Echohaven scene NavMesh status, surface count, obstacle setup, agent inventory.
Method: Read-only filesystem inspection. No Unity Editor required.

---

## 1. NavMesh asset

- **Present:** `Assets/_Project/Scenes/Echohaven_VerticalSlice/NavMesh.asset`
- **File size:** 74,144 bytes (~72 KB) — well above the < 1 KB empty-asset threshold, well below the 1-5 MB you'd expect from a full 500 m terrain bake.
- **Header:** Unity binary serialization, version `6000.3.6f1`.
- **Tile count:** 36 `VAND` (Vault NavData) tile blocks embedded. Real baked geometry, not a placeholder.
- **Interpretation:** Baked, but with a relatively small footprint. Consistent with the Moon 1 playable area (~200 m square, agent radius 0.5 m default), not a 500 m terrain.

## 2. NavMeshSurface in scene

- **Count:** 1 logical `NavMeshSurface` component, encoded with 3 serialized references in the binary `.unity` file (`Unity.AI.Navigation::NavMeshSurface` type ref + a tile-data block + a settings block).
- **Bake bounds (from embedded tile data):** corners around X = -100 to +64, Z = -85 to -99, plus center-volume entries at (-85, 64, -85). The bake clearly covers the Echohaven valley but the visible authored size pattern is roughly **200 × 200 m** around world origin. (Mud pools at X = -50, +55, -45 fall inside it.)
- **Tag:** `Untagged`. **Agent type:** default (0). **Default area:** 0 (Walkable).

## 3. Mud Pool obstacle setup

- File: `Assets/_Project/Scripts/Integration/Moon1MudPoolPuzzle.cs`
- **No `NavMeshObstacle` anywhere in the file** (grep clean).
- Pools are runtime-spawned by `Moon1MudPoolPuzzle.Bootstrap()` (RuntimeInitializeOnLoadMethod, AfterSceneLoad). The pool disc is a `PrimitiveType.Cylinder` with its `Collider` **destroyed immediately** (line 62). Crystals are triggers only.
- **Verdict:** Pools are unmarked. NPCs with `NavMeshAgent` will walk straight across the mud. There is no carve, no obstacle, no NavMeshLink jump. This is a content gap if AI authoring expected pools to block paths.

## 4. Bake menu

- File: `Assets/_Project/Scripts/Editor/Moon1NavMeshBake.cs` — present and clean.
- Menu items registered:
  - `Tartaria/6 Scene Tools/Bake NavMesh`
  - `Tartaria/6 Scene Tools/Save Scene`
  - `Tartaria/0 ★ MASTER/Ready Check (Audit + Bake + Save)`
- Uses legacy `NavMeshBuilder.ClearAllNavMeshes()` + `NavMeshBuilder.BuildNavMesh()` (synchronous). Respects current scene's NavMeshObstacles + bake settings.

## 5. Agents that depend on NavMesh

Code references (all real, not stubs):

- `MiloFollowBehaviour` — `[RequireComponent(typeof(NavMeshAgent))]`. Echohaven companion follow loop.
- `MudGolemEnemy` — `[RequireComponent(typeof(NavMeshAgent))]`. Combat chase.
- `EchohavenContentSpawner.EnableNPCAI()` — adds `NavMeshAgent` + `NPCAIBehavior` to runtime NPCs (Milo, Cassian, Lirael, KayKit characters) at scene load.
- `Reset Scout` — script exists only as `ResetScoutEnemy.cs.disabled` — currently inactive. No live patrol agent in scene.
- `NPCAIBehavior`, `TemporalWraithAI`, `ShadowStalkerAI`, `MudGolemAI`, `EnemyAIController` — all reference `NavMeshAgent`.

The scene file itself contains 0 serialized `NavMeshAgent` instances — every agent is runtime-attached by `EchohavenContentSpawner` after scene load. This works only if NavMesh is baked **before** Play.

## Verdict

**BAKED_HEALTHY** — with one caveat.

- NavMesh asset is present, real, 72 KB, 36 tiles, covers the ~200 m Moon 1 playable area.
- Bake menu wired through `Tartaria → 6 Scene Tools → Bake NavMesh` plus the one-click `Ready Check`.
- All NavMesh-dependent agents (Milo follow, Mud Golem chase, NPC wander) have real implementations and are attached at runtime.
- **Caveat:** Mud Pools have zero `NavMeshObstacle` markers. If gameplay intends pools to block AI paths or force player detours, this is a missing-content bug. Currently mud pools are visual-only obstacles.

## How to rebake from scratch (3 steps)

1. Open `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` in Unity Editor.
2. Run **Tartaria → 6 Scene Tools → Bake NavMesh** (or **Tartaria → 0 ★ MASTER → Ready Check** for audit + bake + save).
3. Ctrl+S to save the scene; `NavMesh.asset` updates in place under `Assets/_Project/Scenes/Echohaven_VerticalSlice/`.

(If the bake menu is unavailable due to compile errors, fall back to **Window → AI → Navigation → Bake → Bake** with the scene's bake settings.)

## Optional follow-up if mud pools should block NavMesh

In `Moon1MudPoolPuzzle.BuildPool()` (around line 62), after creating the `disc` cylinder, add:

```csharp
var obstacle = poolRoot.AddComponent<UnityEngine.AI.NavMeshObstacle>();
obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Capsule;
obstacle.center = new Vector3(0f, 0.5f, 0f);
obstacle.radius = 4f;     // match disc radius
obstacle.height = 1f;
obstacle.carving = true;  // dynamically carve baked NavMesh
obstacle.carveOnlyStationary = true;
```

This carves the baked mesh at runtime when pools spawn, blocking AI pathing. No rebake required because Mud Pools are runtime-instantiated post-load — carving is the right tool.
