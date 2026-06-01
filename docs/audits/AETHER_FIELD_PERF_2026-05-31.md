# Aether Field — Performance Audit (2026-05-31)

Static-only analysis. Unity Profiler was NOT opened.

## Implementation status: **CPU_PLACEHOLDER** (with two caveats)

The spec calls for a 64x64x32 compute-shader-backed voxel sim with 3 bands at ≤ 2 ms/frame. What actually exists:

| File | Role | Reality |
|---|---|---|
| `Assets/_Project/Scripts/Core/AetherFieldManager.cs` (80 lines) | The thing the user asked about | **Not a sim.** A 2-float singleton (`ResonanceScore`, `AetherCharge`) with clamp setters and 2 events. No grid, no bands, no Update loop. |
| `Assets/_Project/Scripts/Core/AetherFieldSystem.cs` | The actual sim | Burst-compiled DOTS `ISystem` running per-entity field math on the **CPU via Burst+IJobEntity**, NOT a `ComputeShader`. Wrapped in `ProfilerMarker("Tartaria.AetherField.R6")`. |
| `Assets/_Project/Scripts/Core/AetherFieldRenderBridge.cs` | ECS → 3D-texture upload | CPU pixel-buffer write, `SetPixels`+`Apply`+`Graphics.CopyTexture` every `LateUpdate`. Comment at line 123 admits: *"Real production would use ComputeShader for better perf"*. |
| `*.compute` | Compute shader | **None exist in the repo.** Glob `**/*.compute` returns zero files. |
| `AetherFog.shader` + `AetherFogRendererFeature.cs` | Volumetric consumer | Real URP RenderGraph pass that samples `_AetherVolume`. This part is real. |

**Caveats:**
1. The DOTS path *is* real, Burst-compiled, and parallel (`ScheduleParallel`). It is not a "stub" — it just is not a compute shader and it operates on `AetherNode` entities (1 per voxel), not on a 3D texture directly.
2. **No code in the repo ever spawns an `AetherNode` entity.** Grep for `AddComponentData.*AetherNode`, `new AetherNode`, `CreateEntity.*AetherNode` — zero hits. So at runtime the IJobEntity iterates over **zero entities** and the bridge uploads an all-zero texture. The system is "running" but doing nothing visible.

## Per-frame work estimate

Assuming the missing voxel spawner gets added (full 64×64×32 = **131,072 nodes**):

| Stage | Where | Cost estimate |
|---|---|---|
| Source/sink gather (managed `foreach` over ECS query, `NativeList.Add`) | `AetherFieldSystem.cs:42-73` | ~5–20 µs for handful of sources |
| Burst parallel job, 131,072 nodes × (Sources+Sinks) distance/falloff math | `AetherFieldSystem.cs:114-152` | ~600 µs – 1.5 ms on a modern 8-core (Burst SIMD-friendly, no branches in hot loop) |
| **Bridge `ToComponentDataArray` (Allocator.Temp) × 2** | `AetherFieldRenderBridge.cs:124-125` | ~1–2 ms allocate+copy for 131k entities |
| **Color[] clear loop, 131,072 iters** | `AetherFieldRenderBridge.cs:128-129` | ~150 µs managed |
| **Per-node SetPixel into managed `Color[]`** | `AetherFieldRenderBridge.cs:132-159` | ~1–2 ms managed (struct copy + Mathf.Max × 4) |
| `Texture3D.SetPixels` + `Apply(false)` | `AetherFieldRenderBridge.cs:165-166` | ~1–3 ms CPU→GPU staging on RGBAHalf 64x64x32 |
| `Graphics.CopyTexture` (Texture3D → RT3D) | `AetherFieldRenderBridge.cs:168` | ~100 µs GPU |

**Realistic total at full grid: 3–8 ms/frame on the bridge alone**, which is 1.5×–4× over the 2 ms budget. The Burst sim itself would likely fit. Today, with zero AetherNode entities, **actual cost is ~50 µs** (empty job dispatch + an empty pixel-buffer clear of a managed Color[] that was never re-sized but still 131,072 long).

## Top 3 perf concerns

1. **`AetherFieldRenderBridge.cs:124-125`** — `ToComponentDataArray(Allocator.Temp)` × 2 every `LateUpdate`. This is a synchronous, full-archetype copy. At 131k entities this is the dominant cost and re-allocs every frame. The "Real production would use ComputeShader" TODO at line 123 acknowledges this.
2. **`AetherFieldRenderBridge.cs:128-159`** — Managed-side `Color[]` clear + per-node random-access write + `Mathf.Max` × 4 per voxel, all on the main thread in `LateUpdate`. No SIMD, no Burst. ~2 ms wasted vs. a 1-call `CommandBuffer.SetBufferData` + compute dispatch.
3. **`AetherFieldSystem.cs:42-43`** — Two `NativeList(Allocator.TempJob)` allocs every frame for source/sink gather, disposed via `state.Dependency` (fine), but the gather itself runs on the main thread (managed `foreach` + `RefRO`), not in a job. With dense Moon 2/3 fountains this becomes a measurable spike. The "R6 spatial pre-filter" uses `playerApprox = float3.zero` (`AetherFieldSystem.cs:45`), so the cull is centered on the world origin, not the actual player — most of the time it culls nothing useful or culls everything wrong.

## Recommendation: **leave alone for now (defer optimization), but fix the playerApprox bug**

Justification:
- Per the **2026-05-30 late-night CLAUDE.md mandate**, no stubs and no placeholders. This isn't a stub in the "method body is empty" sense — the DOTS sim is real code. But it also doesn't actually run because nothing spawns AetherNodes, so it isn't a perf problem today and isn't blocking Moon 1.
- The bridge "should be a compute shader" TODO is a Phase 2 concern. Rewriting the bridge as a ComputeShader is a 1–2 day rabbit hole. Per CLAUDE.md, the current build-order is **Buildings → Objects → Environment → Mini-games → NPCs → Combat polish**. The Aether field visualization is downstream of those.
- One quick fix worth doing now (10 min): replace `float3 playerApprox = float3.zero;` at `AetherFieldSystem.cs:45` with a query against `PlayerTag` (the entity is already created in `GameBootstrap.cs:156-163`). Otherwise the R6 "spatial pre-filter" is silently broken across all 13 Moons.
- Defer the compute-shader rewrite of the bridge to a Phase 2 ticket gated on "AetherNode spawner exists AND profiler shows bridge > 2 ms".

No need to profile or optimize today. The system is effectively dormant.
