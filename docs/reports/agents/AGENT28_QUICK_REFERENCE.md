# AGENT 28: PERFORMANCE OPTIMIZATION QUICK REFERENCE

## 🚀 Quick Access Tools

### VFX Object Pooling
```csharp
// Spawn pooled particle effect
using Tartaria.Core;

var poolManager = VFXPoolManager.Instance;
ParticleSystem ps = poolManager.SpawnParticle(
    vfxPrefab, 
    position, 
    rotation, 
    autoReturnDelay: 2f  // Auto-return after 2s
);

// Manual return
poolManager.ReturnParticle(ps);

// Pool stats
Debug.Log(poolManager.GetPoolStats());
```

### MaterialPropertyBlock (No Material Instances)
```csharp
// Set color without creating material instance
MaterialPropertyBlockHelper.SetColor(renderer, Color.red);

// Set color + emission
MaterialPropertyBlockHelper.SetColorAndEmission(
    renderer, 
    baseColor: Color.blue, 
    emissionColor: Color.cyan, 
    emissionMultiplier: 2.5f
);

// Batch property changes
MaterialPropertyBlockHelper.SetProperties(renderer, block => {
    block.SetColor("_BaseColor", color);
    block.SetFloat("_Metallic", 0.5f);
    block.SetFloat("_Smoothness", 0.8f);
});
```

### LOD Management
```csharp
// Add standard LOD (0-30m, 30-60m, 60-120m, >120m cull)
Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
LODGroup lodGroup = LODHelper.AddStandardLOD(building, renderers);

// Enable smooth transitions
LODHelper.EnableCrossfade(lodGroup, fadeTime: 0.5f);

// Manual distance culling
LODHelper.SetCullingDistance(renderer, camera.transform, maxDistance: 120f);
```

### Performance Profiler
```csharp
// Add PerformanceProfiler component to GameObject in scene
// Configure in Inspector:
//   - autoProfile: true
//   - profileDurationSeconds: 30
//   - warmupFrames: 120
//   - targetFPS: 60

// Or start manually:
var profiler = gameObject.AddComponent<PerformanceProfiler>();
profiler.StartProfiling();

// Report auto-generated to: Logs/PerformanceProfile_{scene}_{timestamp}.md
```

### Draw Call Analyzer
```csharp
// Add DrawCallAnalyzer component to GameObject
// Configure in Inspector:
//   - analyzeOnStart: true
//   - continuousAnalysis: true
//   - analysisInterval: 5.0

// Or analyze manually:
var analyzer = gameObject.AddComponent<DrawCallAnalyzer>();
analyzer.Analyze();

// Output: Renderers, static batched, instanced, unique materials
```

---

## 📊 Performance Targets

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Average FPS | 60fps | 54fps (90%) |
| Frame Time | 16.67ms | 18.5ms |
| P95 Frame Time | <16.67ms | <20ms |
| P99 Frame Time | <20ms | <25ms |
| Draw Calls | <300 | <400 |
| SetPass Calls | <150 | <200 |
| Memory Usage | <4GB | <5GB |
| GC Allocations | <10KB/frame | <50KB/frame |

---

## 🔧 Optimization Checklist

### Before Spawning VFX
- [ ] Use `VFXPoolManager.SpawnParticle()` instead of `Instantiate()`
- [ ] Set `autoReturnDelay` based on particle lifetime
- [ ] Use `MaterialPropertyBlockHelper.SetColor()` for color changes

### Before Creating Materials
- [ ] Check if `MaterialPropertyBlock` can achieve the same effect
- [ ] Use `renderer.sharedMaterial` instead of `renderer.material`
- [ ] Cache material references, don't call `new Material()` repeatedly

### Before Adding Complex Geometry
- [ ] Add LODGroup with `LODHelper.AddStandardLOD()`
- [ ] Enable crossfade for smooth transitions
- [ ] Set static flag for non-moving objects (enables static batching)

### Before Shipping Scene
- [ ] Run `PerformanceProfiler` for 30s+ gameplay
- [ ] Check report: FPS, memory, draw calls
- [ ] Run `DrawCallAnalyzer` to verify batching
- [ ] Ensure <300 draw calls, <4GB memory

---

## 🎯 Per-Moon Optimization Priority

### HIGH PRIORITY (44-49fps baseline)
1. **Moon 10 (PlanetaryNexus)** — Add LOD to 27 mega-stations → 44fps → 55fps
2. **Moon 8 (SunkenColosseum)** — Add LOD to 12 airships → 47fps → 56fps
3. **Moon 6 (LivingLibrary)** — Add LOD to organ pipes → 49fps → 58fps
4. **Moon 13 (PlanetaryRing)** — Distance culling for echo realms → 48fps → 57fps

### MEDIUM PRIORITY (50-53fps baseline)
5. **Moon 3 (WindsweptHighlands)** — Manual LOD addition → 46fps → 57fps
6. **Moon 4 (StarFortBastion)** — LOD for fort structures → 50fps → 61fps
7. **Moon 7 (DeepForge)** — Apply MaterialPropertyBlock → 51fps → 61fps

### LOW PRIORITY (54fps+ baseline)
8. **Moon 2, 5, 9, 11, 12** — Already optimized, maintain

---

## 🚨 Common Performance Pitfalls

### ❌ DON'T DO THIS
```csharp
// Creates new material instance every frame (breaks batching)
renderer.material.color = Color.red;

// Allocates memory every spawn
GameObject vfx = Instantiate(vfxPrefab);
Destroy(vfx, 2f);

// Scene-wide search every frame
PlayerHealth health = FindObjectOfType<PlayerHealth>();

// Physics query every frame
Collider[] cols = Physics.OverlapSphere(pos, radius);
```

### ✅ DO THIS INSTEAD
```csharp
// Preserves GPU instancing
MaterialPropertyBlockHelper.SetColor(renderer, Color.red);

// Zero allocations
var poolManager = VFXPoolManager.Instance;
poolManager.SpawnParticle(vfxPrefab, pos, rot, autoReturnDelay: 2f);

// Cache reference once
PlayerHealth _cachedHealth;
void Start() { _cachedHealth = FindObjectOfType<PlayerHealth>(); }

// Event-driven or cached results
Collider[] _cachedCols = new Collider[32];
int count = Physics.OverlapSphereNonAlloc(pos, radius, _cachedCols);
```

---

## 📈 Performance Monitoring

### Real-Time Monitoring (In Inspector)
- **PerformanceGuard.Instance:** View live FPS, budget status
- **PerformanceProfiler:** Current FPS, memory, draw calls
- **DrawCallAnalyzer:** Renderer counts, batching stats

### Post-Session Analysis
- **Performance Reports:** `Logs/PerformanceProfile_{scene}_{timestamp}.md`
- **Unity Profiler:** Deep Profiling mode for CPU/GPU breakdown
- **Frame Debugger:** Visual draw call inspection

### CI/CD Integration
```powershell
# Run automated performance test
Unity.exe -batchmode -quit -projectPath "C:\dev\TARTARIA_new" `
    -executeMethod "Tartaria.Tools.PerformanceProfiler.RunAllScenes"
    
# Parse reports for regression detection
# Fail build if FPS < 54fps or memory > 4.5GB
```

---

## 🔍 Debugging Performance Issues

### Symptom: Low FPS (<54fps)
1. Check `PerformanceProfiler` report for bottlenecks
2. Run Unity Profiler in Deep Profiling mode
3. Identify hot functions (>2ms frame time)
4. Apply optimizations: pooling, LOD, culling

### Symptom: High Draw Calls (>300)
1. Run `DrawCallAnalyzer`
2. Check for material instances (`renderer.material` usage)
3. Replace with `MaterialPropertyBlock`
4. Verify static batching for non-moving objects

### Symptom: Memory Leaks
1. Unity Profiler → Memory Profiler module
2. Check for unreleased VFX (not returned to pool)
3. Check for material instances not destroyed
4. Check for event subscriptions not unsubscribed

### Symptom: GC Spikes
1. Unity Profiler → GC Alloc column
2. Check for `Instantiate/Destroy` in hot paths
3. Replace with object pooling
4. Check for string concatenation in Update loops

---

## 📚 Additional Resources

- **AGENT28_PERFORMANCE_PROFILING_REPORT.md** — Full optimization report
- **PERFORMANCE_BASELINE_REPORT.md** — Pre-optimization metrics
- **AGENT4_PERFORMANCE_OPTIMIZATION_REPORT.md** — Prior optimization pass
- **Unity Profiler Docs:** https://docs.unity3d.com/Manual/Profiler.html
- **URP Best Practices:** https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

---

**Last Updated:** May 24, 2026  
**AGENT 28 Status:** ✅ COMPLETE  
**Compilation:** ✅ GREEN
