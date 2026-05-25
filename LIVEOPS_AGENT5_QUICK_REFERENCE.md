# LIVEOPS Agent 5: Quick Reference
**Player Load Performance Testing — TARTARIA Beta Launch**

---

## 🚀 Quick Start

### 1. Run Load Test

```csharp
// In Editor: Create GameObject → Add Component → PlayerLoadSimulator

// Configure in Inspector:
loadLevel = Medium;              // Light(10) / Medium(50) / Heavy(100) / Moon13Boss(150)
testDurationSeconds = 120f;      // Test duration
autoStartOnAwake = true;         // Auto-start on Play

// OR via script:
var loadSim = FindObjectOfType<PlayerLoadSimulator>();
loadSim.loadLevel = LoadLevel.Medium;
loadSim.StartTest();
```

**Output:** `Logs/load_tests/LoadTest_Medium_{timestamp}.md`

### 2. Run Performance Benchmark

```csharp
// In any Moon scene:
PerformanceBenchmark.Instance.RunBenchmark();

// Baseline saved to:
// Logs/Benchmarks/Baselines/Moon13.json

// Report saved to:
// Logs/Benchmarks/Reports/Moon13_{timestamp}.md
```

### 3. Check for Regression

```csharp
// After running new benchmark:
var baseline = PerformanceBenchmark.Instance.GetBaseline("Moon13");
if (baseline.avgFPS < 55f) {
    Debug.LogError("⚠️ REGRESSION DETECTED!");
}

// Automatic regression check runs on every benchmark
```

---

## 📊 Test Scenarios

| Load Level | Virtual Players | Target FPS | Duration | Memory Limit |
|------------|----------------|------------|----------|--------------|
| **Light** | 10 | 60fps | 120s | 512MB |
| **Medium** | 50 | 55fps | 120s | 768MB |
| **Heavy** | 100 | 45fps | 120s | 1024MB |
| **Moon13Boss** | 150 | 45fps | 180s | 1024MB |

---

## 🎯 Performance Targets

### FPS Baselines (GTX 1060 @ 1080p)

| Moon Range | Target FPS | Intensity |
|------------|------------|-----------|
| Moon 1-3 | 60fps | Tutorial, light combat |
| Moon 4-6 | 58fps | Moderate enemies, VFX |
| Moon 7-9 | 57fps | Dense environments |
| Moon 10-12 | 56fps | Boss fights |
| **Moon 13** | **55fps** | Final boss (maximum) |

---

## 🔧 Key Files

### New Files (Agent 5)
- `Assets/_Project/Scripts/Tests/StabilityTests/PlayerLoadSimulator.cs` (1,100 lines)
- `Assets/_Project/Scripts/Tools/PerformanceBenchmark.cs` (500 lines)
- `LIVEOPS_AGENT5_PERFORMANCE_REPORT.md` (comprehensive analysis)

### Existing Files (Leveraged)
- `Assets/_Project/Scripts/Tools/PerformanceProfiler.cs` (Agent 28)
- `Assets/_Project/Scripts/Core/ObjectPool.cs` (pooling system)
- `Assets/_Project/Scripts/Tests/StabilityTests/MarathonSessionSimulator.cs` (Agent 4)

---

## 📈 Metrics Tracked

### PlayerLoadSimulator
- **FPS:** Average, min, p95, p99
- **Memory:** Managed heap, native memory, GC allocs/frame
- **GameObjects:** Active count, spawned enemies, loot items
- **Particles:** Active emitters, total particles
- **Audio:** 3D spatialized sources

### PerformanceBenchmark
- **FPS:** Average, min, p95 frame time
- **Memory:** Average, peak (managed + native)
- **Regression:** % change vs. previous baseline
- **Alerts:** >10% FPS drop triggers warning

---

## 🚨 Hot Paths (Multiplayer Bottlenecks)

### Critical Issues (Fix Before Multiplayer)

| File | Issue | Fix |
|------|-------|-----|
| `SaveManager.cs:1430` | `FindObjectsOfType<ISaveDataProvider>()` | Registry pattern |
| `MarathonSessionSimulator.cs:313` | `FindObjectsOfType<GameObject>()` | Cache baseline count |
| `AccessibilityManager.cs:345` | `FindObjectsOfType<Button>()` | Cache button refs |

**Total FindObjectsOfType calls:** 39 instances 🔴

---

## 🎮 Usage Examples

### Example 1: Light Load Test (10 Players)
```csharp
// Setup
var loadSim = gameObject.AddComponent<PlayerLoadSimulator>();
loadSim.loadLevel = LoadLevel.Light;
loadSim.testDurationSeconds = 60f;
loadSim.enemyPrefab = Resources.Load<GameObject>("Enemies/SkeletonWarrior");
loadSim.vfxHitPrefab = Resources.Load<GameObject>("VFX/HitEffect");

// Run
loadSim.StartTest();

// Expected: 60fps, <512MB memory
```

### Example 2: Benchmark Current Scene
```csharp
// Quick benchmark
PerformanceBenchmark.Instance.RunBenchmark();

// Wait 60s...

// Result saved to:
// Logs/Benchmarks/Baselines/Moon7.json
// { "sceneName": "Moon7", "avgFPS": 57.2, "peakMemoryMB": 612, ... }
```

### Example 3: Regression Check in CI/CD
```csharp
// In automated test:
[Test]
public void TestPerformanceRegression_Moon13()
{
    SceneManager.LoadScene("Moon13");
    yield return new WaitForSeconds(5f); // Scene load
    
    var baseline = PerformanceBenchmark.Instance.GetBaseline("Moon13");
    Assert.IsNotNull(baseline, "Baseline not found!");
    Assert.GreaterOrEqual(baseline.avgFPS, 55f, "FPS below target!");
    
    Debug.Log($"✅ Moon13 baseline: {baseline.avgFPS:F1}fps");
}
```

---

## 📦 Object Pooling Recommendations

### Current Coverage ✅
- VFX particles (VFXPoolManager)
- Generic GameObjects (ObjectPool<T>)
- Moon 2 props

### Missing Coverage 🔴
- **Enemies** → Create `EnemyPoolManager.cs`
- **Loot drops** → Create `LootPoolManager.cs`
- **Projectiles** → Use `ObjectPool<Projectile>`

### Pool Capacity Targets
- Enemies: 200 instances
- Loot: 100 instances
- VFX: 150 emitters
- Projectiles: 500 instances

---

## 🌐 Network Readiness (Future)

### Multiplayer Bottlenecks Flagged
1. **FindObjectsOfType** — 39 calls → Registry pattern needed
2. **Combat damage** — Local calcs → Server-authoritative required
3. **Loot spawning** — Instant spawn → Server-controlled drops
4. **VFX scaling** — No LOD → Network LOD for distant players

### Recommended Optimizations
- **Spatial Hashing:** O(n) → O(1) proximity queries
- **Unity DOTS:** 10x entity throughput for 1000+ enemies
- **Network LOD:** 70% bandwidth reduction
- **Delta Compression:** <5KB/sec per player

---

## 📋 Testing Checklist

### Pre-Launch Tests
- [ ] Light load (10 players) — 60s
- [ ] Medium load (50 players) — 120s
- [ ] Heavy load (100 players) — 120s
- [ ] Moon13 boss fight — 180s
- [ ] All 13 Moon baselines captured

### Post-Launch Monitoring
- [ ] Daily benchmark runs (Medium load)
- [ ] Weekly stress tests (Heavy load)
- [ ] Regression alerts configured
- [ ] CI/CD integration (automated benchmarks)

---

## 🛠️ Troubleshooting

### Issue: Test runs but no report generated
**Fix:** Check `generateDetailedReport = true` in Inspector

### Issue: FPS far below target
**Fix:** 
1. Run Unity Profiler to identify bottlenecks
2. Check for excessive FindObjectsOfType calls
3. Profile particle system counts (target: <50 emitters)

### Issue: Baseline not saving
**Fix:** Check write permissions to `Logs/Benchmarks/Baselines/`

### Issue: Memory exceeding limits
**Fix:**
1. Run Memory Profiler
2. Expand object pooling (reduce Instantiate/Destroy)
3. Check for texture/mesh leaks

---

## 📞 Support

**Agent:** Agent 5 — Player Load Performance  
**Files:** PlayerLoadSimulator.cs, PerformanceBenchmark.cs  
**Reports:** LIVEOPS_AGENT5_PERFORMANCE_REPORT.md

**Test Execution Status:**
- Infrastructure: ✅ Ready
- Light load: 🟡 Pending
- Medium load: 🟡 Pending
- Heavy load: 🟡 Pending

**Next Steps:** Execute tests and validate FPS at scale!

---

**Quick Commands:**
```csharp
// Start light test
PlayerLoadSimulator.Instance.loadLevel = LoadLevel.Light;
PlayerLoadSimulator.Instance.StartTest();

// Benchmark current scene
PerformanceBenchmark.Instance.RunBenchmark();

// Check Moon 13 baseline
var bl = PerformanceBenchmark.Instance.GetBaseline("Moon13");
Debug.Log($"Moon13: {bl.avgFPS}fps");
```
