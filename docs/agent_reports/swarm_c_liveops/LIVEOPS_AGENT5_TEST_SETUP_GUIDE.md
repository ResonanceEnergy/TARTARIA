# Agent 5: Load Test Scene Setup Guide

## Quick Setup (5 minutes)

### Step 1: Create Test Scene
1. Open Unity
2. Create new scene: `File → New Scene → Basic (Built-in)`
3. Save as: `Assets/_Project/Scenes/Tests/LoadTestScene.unity`

### Step 2: Add PlayerLoadSimulator
1. Create GameObject: `Hierarchy → Create Empty`
2. Rename to: `LoadTestManager`
3. Add component: `PlayerLoadSimulator`
4. Configure in Inspector:
   ```
   Load Level: Medium (50 players)
   Test Duration Seconds: 120
   Auto Start On Awake: true
   Generate Detailed Report: true
   
   Movement Radius: 50
   Combat Intensity: 0.7
   VFX Spawn Rate: 2
   Loot Drop Rate: 0.5
   ```

### Step 3: Assign Prefabs (Optional)
If you want to spawn real enemies/VFX:
```
Enemy Prefab: Assets/_Project/Prefabs/Enemies/SkeletonWarrior.prefab
Loot Prefab: Assets/_Project/Prefabs/Items/LootBag.prefab
VFX Hit Prefab: Assets/_Project/Prefabs/VFX/HitEffect.prefab
VFX Explosion Prefab: Assets/_Project/Prefabs/VFX/Explosion.prefab
Combat Sound: Assets/_Project/Audio/SFX/CombatHit.wav
```

**Note:** If prefabs are not assigned, the simulator will run without spawning actual objects (pure performance test).

### Step 4: Run Test
1. Press Play
2. Watch Console for progress logs every 15s
3. Test runs for 120 seconds (2 minutes)
4. Report auto-generates to: `Logs/load_tests/LoadTest_Medium_{timestamp}.md`

---

## Test Matrix

Run these 4 tests in sequence:

### Test 1: Light Load (Baseline)
```
Load Level: Light
Test Duration: 60 seconds
Expected: 60fps, <512MB
```

### Test 2: Medium Load (Realistic)
```
Load Level: Medium
Test Duration: 120 seconds
Expected: 55fps, <768MB
```

### Test 3: Heavy Load (Stress)
```
Load Level: Heavy
Test Duration: 120 seconds
Expected: 45fps, <1024MB
```

### Test 4: Moon13 Boss (Max Intensity)
```
Load Level: Moon13Boss
Test Duration: 180 seconds
Expected: 45fps, <1024MB
```

---

## Performance Benchmark Setup

### Run in Existing Moon Scene
1. Open any Moon scene (e.g., `Moon13.unity`)
2. Create GameObject: `Hierarchy → Create Empty`
3. Rename to: `BenchmarkManager`
4. Add component: `PerformanceBenchmark`
5. In Inspector: Right-click on component → `Run Benchmark (Current Scene)`
6. Wait 60 seconds (120 warmup frames + 60s capture)
7. Baseline saved to: `Logs/Benchmarks/Baselines/Moon13.json`
8. Report saved to: `Logs/Benchmarks/Reports/Moon13_{timestamp}.md`

### Run via Script
```csharp
// In any MonoBehaviour:
void Start() {
    PerformanceBenchmark.Instance.RunBenchmark();
}
```

---

## Console Output Examples

### PlayerLoadSimulator Output
```
[LoadTest] Initialized — Medium load level, 120s duration
[LoadTest] Target: 50 virtual players
[LoadTest] Starting Medium load test...
[LoadTest] Spawned 50 virtual players
[LoadTest] Enemy pool initialized: 50 objects
[LoadTest] Loot pool initialized: 20 objects
[LoadTest] Progress: 25.0% — FPS: 57.2, Memory: 612MB
[LoadTest] Progress: 50.0% — FPS: 56.8, Memory: 641MB
[LoadTest] Progress: 75.0% — FPS: 55.3, Memory: 687MB
[LoadTest] Test duration reached, stopping...
[LoadTest] Test stopped. Captured 240 snapshots
[LoadTest] Report saved: C:/dev/TARTARIA_new/Logs/load_tests/LoadTest_Medium_20260524_143251.md
```

### PerformanceBenchmark Output
```
[Benchmark] Starting benchmark for Moon13...
[Benchmark] Benchmark complete for Moon13 — 1800 frames captured
[Benchmark] Regression Check for Moon13:
  FPS: 55.7 → 55.3 (-0.7%)
  Memory: 812MB → 825MB (+13MB)
✅ [Benchmark] Performance within acceptable range (-0.7%)
[Benchmark] Baseline saved: C:/dev/TARTARIA_new/Logs/Benchmarks/Baselines/Moon13.json
[Benchmark] Report saved: C:/dev/TARTARIA_new/Logs/Benchmarks/Reports/Moon13_20260524_143310.md
```

---

## Troubleshooting

### Issue: NullReferenceException on Start
**Cause:** Prefabs not assigned  
**Fix:** Leave prefabs empty (simulator runs without spawning)

### Issue: Test runs but FPS is unstable
**Cause:** Editor overhead  
**Fix:** Run in Standalone build for accurate results

### Issue: Report not generating
**Cause:** Write permissions  
**Fix:** Check `Logs/` folder exists and is writable

### Issue: Memory exceeds limits
**Cause:** Prefabs spawning too much  
**Fix:** Reduce `vfxSpawnRate` and `lootDropRate` in Inspector

---

## Standalone Build Testing (Recommended)

For most accurate results, run tests in standalone build:

1. Build settings: `File → Build Settings`
2. Platform: Windows
3. Target: Development Build + Autoconnect Profiler
4. Build to: `Builds/Windows/LoadTest.exe`
5. Run executable
6. Reports still saved to: `{BuildFolder}/Logs/load_tests/`

---

## Viewing Reports

### Load Test Report
Location: `Logs/load_tests/LoadTest_Medium_{timestamp}.md`

Open in VS Code or any markdown viewer. Contains:
- Performance summary (FPS, memory)
- ASCII graphs (FPS over time, memory over time)
- GameObject/particle/audio counts
- Bottleneck analysis
- Network readiness audit
- Optimization recommendations
- Test verdict (PASS/FAIL)

### Benchmark Report
Location: `Logs/Benchmarks/Reports/Moon13_{timestamp}.md`

Contains:
- Metrics table (FPS, memory, targets, status)
- Comparison with previous baseline
- Regression analysis
- Verdict

---

## CI/CD Integration (Future)

To run benchmarks in Unity batchmode:

```bash
Unity.exe -batchmode -projectPath "C:/dev/TARTARIA_new" \
  -executeMethod PerformanceBenchmark.RunAllScenes \
  -logFile "Logs/benchmark_ci.log" \
  -quit

# Check exit code
if [ $? -ne 0 ]; then
  echo "Benchmark failed!"
  exit 1
fi
```

---

## Quick Commands Reference

```csharp
// Start light test
var loadSim = FindObjectOfType<PlayerLoadSimulator>();
loadSim.loadLevel = LoadLevel.Light;
loadSim.StartTest();

// Stop test early
loadSim.StopTest();

// Benchmark current scene
PerformanceBenchmark.Instance.RunBenchmark();

// Get baseline
var baseline = PerformanceBenchmark.Instance.GetBaseline("Moon13");
Debug.Log($"Moon13 baseline: {baseline.avgFPS}fps, {baseline.peakMemoryMB}MB");

// Check if baseline exists
if (PerformanceBenchmark.Instance.GetBaseline("Moon7") == null) {
    Debug.Log("No baseline for Moon7 yet");
}
```

---

## Expected Test Duration

| Test | Setup | Warmup | Capture | Report | Total |
|------|-------|--------|---------|--------|-------|
| Light (10p) | 10s | 2s | 60s | 5s | ~1.5 min |
| Medium (50p) | 10s | 2s | 120s | 5s | ~2.5 min |
| Heavy (100p) | 10s | 2s | 120s | 5s | ~2.5 min |
| Moon13Boss (150p) | 10s | 2s | 180s | 5s | ~3.5 min |
| **Total All Tests** | | | | | **~10 minutes** |

Benchmark per Moon: ~1.5 minutes each × 13 Moons = **~20 minutes**

**Total Test Suite:** ~30 minutes for all load tests + all Moon baselines

---

## Success Criteria

### Load Tests
- ✅ Light: Average FPS ≥ 60fps
- ✅ Medium: Average FPS ≥ 55fps
- ✅ Heavy: Average FPS ≥ 45fps
- ✅ Moon13Boss: Average FPS ≥ 45fps
- ✅ All tests: Peak memory ≤ target

### Benchmarks
- ✅ All 13 Moons: Average FPS ≥ target (see baseline table)
- ✅ No regressions: <10% FPS drop from previous baseline
- ✅ Memory stable: Peak memory ≤ 1024MB

---

## Ready to Execute! 🚀

All infrastructure is in place. Follow the steps above to:
1. Create LoadTestScene.unity
2. Add PlayerLoadSimulator component
3. Run tests (Light → Medium → Heavy → Moon13Boss)
4. Benchmark all 13 Moon scenes
5. Review reports and validate FPS targets

**Estimated Time:** ~30 minutes for complete test suite

Good luck! 🎯
