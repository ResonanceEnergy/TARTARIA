# TARTARIA Performance Audit — Quick Reference

**Overall Score: 72/100 (B+)** | **Current: 70 FPS** | **Target: 68 FPS ✅ MET**

---

## 🔥 Critical Bottlenecks (HIGH IMPACT)

### 1. Camera.main Per-Frame Lookups (-4 FPS)
**Fix:** Cache in Awake  
**Files:** `DamageNumberPool.cs`, `DissonanceLensOverlay.cs`  
```csharp
private Camera _mainCamera;
void Awake() { _mainCamera = Camera.main; }
```

### 2. Uncached Material Instantiation (-3 FPS)
**Fix:** Material cache dictionary (copy LootDropper pattern)  
**Files:** `Moon10ContentSpawner`, `EchohavenObelisk`, `ReturnPortal`, `MoonCompanionSpawner`, `DecalHitPool`, `HitVFXController`, `Moon3OrphanTrainPuzzle`, `MoonMechanicActivator`  
```csharp
static readonly Dictionary<string, Material> _materialCache = new();
```

### 3. A* Pathfinding Allocations (-1-2 FPS)
**Fix:** Pool collections  
**File:** `Moon10ContentSpawner.cs` lines 667-670

---

## ✅ Optimization Wins (Already Implemented)

1. **LootDropper Material Cache** (+24 FPS baseline improvement)
2. **Component Reference Caching** (+5-8 FPS, 80+ scripts)
3. **PerformanceGuard Monitoring** (auto quality fallback)
4. **Addressables Memory Management** (leak-free, handle tracking)

---

## 📊 Performance Metrics (Agent 8 Test Results)

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| **Avg FPS** | 68 | 70 | ✅ MET (+2 FPS) |
| **P95 Frame Time** | <16.67ms | 18.7ms | 🔴 2ms OVER |
| **Per-Frame Allocs** | <10 KB | 4.32 KB | ✅ EXCELLENT |
| **GC Pressure** | <5 MB/2s | 3.12 MB | ✅ LOW |

---

## 🎯 Quick Win Action Plan (3 Days → +10 FPS)

**Day 1:** Cache Camera.main → +4 FPS  
**Day 2:** Material caching (4 spawners) → +3 FPS  
**Day 3:** A* pooling + UI CanvasGroup → +3 FPS  

**Expected:** 70 FPS → 80 FPS baseline

---

## 🔬 Next Steps

1. Run `.\tartaria-play.ps1 -BatchOnly` to execute PerformanceProfilingTest
2. Implement fixes in priority order
3. Re-run tests to validate FPS gains
4. Update PerformanceGuard budgets if targets change

---

## 📁 Key Files

**Tests:** `Assets/_Project/Scripts/Tests/PerformanceProfilingTest.cs`  
**Monitoring:** `Assets/_Project/Scripts/Core/PerformanceGuard.cs`  
**Model (Material Cache):** `Assets/_Project/Scripts/Integration/LootDropper.cs`  
**Full Report:** `PERFORMANCE_AUDIT_REPORT.md`

---

**Last Updated:** 2026-05-23
