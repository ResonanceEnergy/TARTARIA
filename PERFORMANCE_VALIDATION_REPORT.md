# TARTARIA Beta — Performance Validation Report
**Session:** Hour 3 — Performance QA  
**Date:** May 22, 2026  
**Validator:** Performance Validation Lead  
**Target:** 60 FPS sustained on GTX 1070 / RX 580 (4GB VRAM)

---

## Executive Summary
**Automated Gate Status:** ❌ CRASHED (Vulkan driver issue in batchmode)  
**Manual Profiling Status:** 🔄 IN PROGRESS  
**Build Status:** ✅ CS:0 MAINTAINED  

---

## 1. Automated Performance Gates

### Execution Log
- **Command:** `Unity.exe -batchmode -executeMethod PerformanceGateRunner.RunCIGates`
- **Result:** CRASH during GPU readback
- **Log:** `Logs/perf-gate-validation.log`

### Crash Details
```
========== STACK TRACE ==================
amdvlk64.dll (AMD Vulkan driver)
  -> vk::TaskExecutor::HandleCommandStream
  -> GfxDeviceVK::ReadbackImage
  -> GfxDeviceWorker::RunCommand

Crash intercepted at:
C:/Users/gripa/AppData/Local/Temp/Unity/Editor/Crashes
```

### Root Cause Analysis
- **Issue:** Unity batchmode GPU readback incompatibility with AMD Vulkan driver
- **Impact:** Cannot run automated performance gates in CI/batchmode
- **Workaround:** Manual profiling in Editor play mode (current approach)
- **Recommendation:** Switch to Unity Profiler API for headless perf capture, or use local gate runner (non-batchmode)

---

## 2. Manual Performance Profiling

### Test Environment
- **Unity Version:** 6000.3.6f1
- **Render Pipeline:** URP 17.3.0 (Forward+)
- **GPU Resident Drawer:** Enabled
- **Structured Buffers:** Enabled (STP)
- **APV Lighting:** Enabled
- **Build Configuration:** Development build, profiler enabled

### Test Scenarios

#### Moon 1: Echohaven (Vertical Slice)
**Scene:** `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`  
**Duration:** 3 minutes  
**Activities:**
- Building restoration (resonance interaction)
- Combat with shadows (sword + abilities)
- Traversal (platforming, exploration)

**Results:** [AWAITING USER INPUT]
- Minimum FPS: _____
- Average FPS: _____
- Worst-case frame time: _____ ms
- GPU bottlenecks: _____
- CPU bottlenecks: _____

#### Moon 2-3: [PENDING]
**Status:** Awaiting Moon 1 results

---

## 3. Performance Budget Analysis

### Target Budget (60 FPS)
- **Frame Budget:** 16.67ms
- **CPU Budget:** ~10ms (gameplay, physics, scripts)
- **GPU Budget:** ~6ms (rendering, post-processing)
- **Memory Budget:** <4GB VRAM

### Known Optimization Features
✅ GPU Resident Drawer (reduces draw call overhead)  
✅ Structured Buffers (batch transforms)  
✅ APV baking (runtime GI cost eliminated)  
✅ Occlusion culling (visible set reduction)  
✅ LOD system (triangle count scaling)  

### Potential Bottlenecks (Unverified)
⚠️ Post-processing stack (bloom, color grading)  
⚠️ Shadow cascades (4x cascades = 4x shadow map draws)  
⚠️ Particle systems (overdraw in combat)  
⚠️ Transparent effects (order-dependent sorting)  

---

## 4. Pass/Fail Criteria

### P0 Performance Gates (MUST PASS for beta)
- [ ] **Moon 1 sustained FPS ≥55** (avg across 3-min session)
- [ ] **Worst-case frame time ≤20ms** (no hitches >50ms)
- [ ] **No GPU memory warnings** (<4GB VRAM usage)
- [ ] **No CPU spikes >30ms** (excluding scene load)

### P1 Quality Gates (NICE TO HAVE)
- [ ] **Moon 1 sustained FPS ≥60** (locked 60 FPS)
- [ ] **99th percentile frame time ≤18ms**
- [ ] **VRAM usage <3.5GB** (headroom for background apps)

---

## 5. Recommendations

### Immediate Actions (if FPS <55)
1. **Profile GPU via Unity Profiler** (`Window > Analysis > Profiler`)
   - Check GPU module for expensive render passes
   - Identify overdraw hotspots (Scene View > Overdraw mode)

2. **Quick Wins:**
   - Reduce shadow cascade count (4→3 or 2)
   - Lower shadow resolution (2048→1024)
   - Disable expensive post-processing (motion blur, bloom quality)
   - Check for unintended double-rendering (cameras, reflection probes)

3. **Code Profiling:**
   - Deep Profile mode for script bottlenecks
   - Check `Update()` loops for expensive operations
   - Verify object pooling (no GC spikes)

### Deferred Actions (if FPS ≥55)
- Schedule full Moon 2-3 profiling in Hour 4
- Run stress test (100+ enemies, max particles)
- Verify frame pacing consistency (frame time variance)

---

## 6. Next Steps

**IF GATES PASS (FPS ≥55):**
✅ Approve proceed to Hour 4 (Content Integration)  
✅ Document baseline performance metrics  
✅ Schedule Moon 2-3 validation  

**IF GATES FAIL (FPS <50):**
❌ BLOCK beta delivery  
🔧 Implement quick-win optimizations  
🔄 Re-test after fixes  

---

## 7. Testing Instructions (FOR MANUAL TESTER)

### How to Profile Moon 1
1. **Launch:** Unity Editor should be running in Play mode (Echohaven scene)
2. **Enable Stats:** Press `F3` to show FPS overlay (or Window > Analysis > Stats)
3. **Test Activities:**
   - Restore 2-3 buildings (resonance mechanic)
   - Fight 5+ shadows (combat stress test)
   - Explore village (traversal, camera movement)
4. **Record Metrics:**
   - Note minimum FPS during combat
   - Note average FPS during exploration
   - Watch for hitches (sudden frame drops)
5. **Stop:** Press ESC, exit Play mode

### Where to Find Results
- **Stats Overlay:** Bottom-right corner (FPS, frame time)
- **Profiler:** Window > Analysis > Profiler (detailed breakdown)
- **Console Warnings:** Check for performance warnings (overdraw, batching)

---

## Appendix A: System Specs

### Test Hardware
- **GPU:** AMD Radeon (driver: amdvlk64)
- **Unity Version:** 6000.3.6f1
- **OS:** Windows 11
- **Project:** TARTARIA_new (C:/dev/TARTARIA_new)

### Unity Settings
- **Quality:** Custom (optimized for 60 FPS)
- **VSync:** Disabled (for accurate frame time measurement)
- **Frame Rate Cap:** None (measure raw performance)

---

## Appendix B: Log Files
- **Performance Gate Log:** `Logs/perf-gate-validation.log` (crashed)
- **Build Log:** `Logs/tartaria-build.log` (CS:0)
- **Unity Crash Report:** `%TEMP%/Unity/Editor/Crashes/` (latest crash dump)

---

**END OF REPORT**  
*Awaiting manual profiling results to complete validation.*
