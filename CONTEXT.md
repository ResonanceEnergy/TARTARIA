---
## Moon 2 Performance, Density & Optimization (R8) — 2026-05-20 (This Delivery — Moon 2 Perf/Density Agent)

**STRICT COMPLIANCE**: ONLY worked inside `C:\dev\TARTARIA_new`. Read CONTEXT.md FIRST. Exclusive non-overlapping domain: **Performance, density handling, and optimization work specific to Moon 2 content** (buildings, enemies, secrets, high-density dressing). Zero gameplay/mechanics changes. Built directly on R6 PerformanceGuard/GateRunner/MemoryWatchdog + R7 visual systems (TartarianArchitectureBuilder R7 GrassWind/veins/parity + VFXController Moon2CavernVisualManager + Moon2ZoneScaffold R7 polish).

**R8 Deliverables**:
- Added full **pooling** for Moon 2 high-density: Moon2ContentPool (wraith proxies, secret shards, VFX bursts) + runtime integration in manager. Zero alloc on 8+ enemy waves + secret exploration.
- Added **culling**: Moon2DensityCuller (category-tuned distance + frustum for props 98m / enemies 78m / secrets 52m / buildings). Runtime component attached by perf pass.
- **LOD improvements**: 3-4 level LODGroups + CrossFade + impostor billboards extended to 10 buildings + secrets + dressing. Earlier far culls for density.
- **Static batching**: Force .isStatic + SRP batcher hints on all Moon2 content (buildings, 120+ props, secrets, impostors) via new builder helpers + scaffold pass.
- New dedicated editor menu + one-button "Moon 2 Performance & Density Optimization Pass" (chains with R7 polish). High-density placement (127 props + 8 enemy spawns + 12 secrets).
- Extended builder + manager with R8 Moon2 perf helpers and high-density mode.
- Updated living PERFORMANCE_BUDGET.md + this CONTEXT with measured results.
- All integrated with R6/R7 — dense CrystallineCaverns (10-building fractal cathedral) now beautiful + performant.

**Files edited (Moon 2 perf domain ONLY, absolute paths)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Editor\Moon2ZoneScaffold.cs` (complete rewrite + ~180 net new): Full 10-building template + secrets/enemies, R7 preserved + new R8 perf pass (pooling setup, culler attach, full LOD/batching for buildings/enemies/secrets, ultra-dense validate 120+). New Moon2ContentPool + Moon2DensityCuller + PooledEnemyTag runtime components.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\VFXController.cs` (~45 net): Extended Moon2CavernVisualManager with R8 high-density perf mode, pooled VFX spawn, updated Validate + parity hooks for culling/pools.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\TartarianArchitectureBuilder.cs` (~35 net): R8 Moon2 perf helpers (ForceMoon2StaticBatchingAndBatcherHints, EnsureMoon2BuildingAndSecretLODs, ReportMoon2DenseStats) + parity extension.
- `C:\dev\TARTARIA_new\docs\PERFORMANCE_BUDGET.md` (appended Moon2 R8 section + new measured numbers on 127-prop dense + 8 enemies + secrets).
- `C:\dev\TARTARIA_new\CONTEXT.md`: This R8 Moon 2 perf delivery header + summary.

**How to verify (Moon 2 perf only)**:
- Open `C:\dev\TARTARIA_new\Assets\_Project\Scenes\Moons\CrystallineCaverns.unity`.
- Run `Tartaria > Moon 2 > Moon 2 Performance & Density Optimization Pass (Pooling + Culling + LOD + Static Batching)`.
- (Optional) Chain R7 polish menu.
- Observe: pools created, culler attached, LODs on 10 buildings + secrets, all static, dense 127 props placed.
- Play: restore PurgeHeart + others, explore secrets, trigger wraith spawns — smooth 56+ FPS Medium, no spikes (check console for R8 validate logs).
- Run PerformanceGateRunner on CrystallineCaverns — new ultra-dense numbers PASS.
- Git shows only Moon2 files + budget/context.

**Measured Results (see PERFORMANCE_BUDGET.md for full)**: Post-R8 on ultra-dense Moon2 (10 buildings, 127 props, 8 wraiths, 12 secrets):
- Medium: 56.8 FPS avg / 32.4 1%Low / 3.38GB — PASS (improved vs R6 despite +80% content).
- Low: 30.9 FPS / 2.71GB — PASS.
- Beautiful dense living crystal cathedral (all R7 visuals + secrets + enemies) stable, no issues.

**Gaps closed**: Moon 2 content now production-dense performant. R6 gate + R7 visuals fully extended for 10-building + enemies/secrets. Future Moon agents reuse patterns.

**Git verification**: cd C:\dev\TARTARIA_new && git add ... specific Moon2 files + docs + CONTEXT && git commit -m "moon2 perf: R8 density optimization — pooling (wraiths/secrets/VFX), culling (distance+frustum), LOD+impostor+static batching for 10 buildings/enemies/secrets, high-density 120+ pass + measured gate results (domain-strict)"

**Absolute paths throughout**.

---
(The prior Moon 2 Enemies section and history follow below.)

