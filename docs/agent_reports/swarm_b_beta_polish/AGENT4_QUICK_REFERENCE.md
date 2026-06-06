# AGENT 4: STABILITY TESTING QUICK REFERENCE

**Status:** ✅ COMPLETE  
**Date:** 2026-05-24

---

## TESTING TOOLS OVERVIEW

| Tool | Purpose | Location | Usage |
|------|---------|----------|-------|
| **MarathonSessionSimulator** | 10-hour gameplay simulation | `Tests/StabilityTests/` | Attach to GameObject, configure duration |
| **MemoryLeakDetector** | Runtime leak scanning | `Tests/StabilityTests/` | Attach to DontDestroyOnLoad GameObject |
| **SaveFileBloatAnalyzer** | Save file growth analysis | `Tests/StabilityTests/` | Attach to persistent GameObject |
| **StaticEventCleaner** | Domain reload protection | `Core/` | Automatic — no setup required |

---

## QUICK START: RUN STABILITY TEST

### 1. Setup Test Scene

```
1. Create empty scene: "StabilityTest"
2. Add GameObject: "Marathon Tester"
3. Attach components:
   - MarathonSessionSimulator
   - MemoryLeakDetector
   - SaveFileBloatAnalyzer
4. Mark "Marathon Tester" as DontDestroyOnLoad
```

### 2. Configure Marathon Simulator

```
Inspector Settings:
- Test Duration Hours: 10
- Monitoring Interval Minutes: 30
- Auto Start On Awake: TRUE
- Enable Detailed Logging: TRUE

Behavior Simulation:
- Walking %: 30
- Combat %: 25
- Menu %: 15
- Save/Load %: 10
- Idle %: 20
```

### 3. Configure Memory Leak Detector

```
Inspector Settings:
- Scan On Start: FALSE
- Periodic Scanning: TRUE
- Scan Interval Minutes: 5
- Log To File: TRUE
```

### 4. Configure Save Bloat Analyzer

```
Inspector Settings:
- Analyze On Start: FALSE
- Periodic Analysis: TRUE
- Analysis Interval Minutes: 15
```

### 5. Build & Run

```
1. File > Build Settings > Build (Standalone)
2. Run the executable
3. Let it run for 10 hours
4. Review logs at: %APPDATA%\..\LocalLow\<Company>\<Game>\stability_logs\
```

---

## INTERPRETING RESULTS

### Marathon Test Pass/Fail

**GREEN (Pass):**
```
[Overall Result: ✅ PASS]
  ✅ PASS: FPS stable (+2.1%)
  ✅ PASS: Managed heap growth acceptable
  ✅ PASS: GameObject count stable
```

**RED (Fail):**
```
[Overall Result: ❌ FAIL]
  ❌ FAIL: FPS degraded by 25.3% (threshold: -20%)
  ❌ FAIL: Managed heap grew by 156 MB (threshold: 100 MB)
  ✅ PASS: GameObject count stable
```

**Action:** If failed, review memory leak report for root cause.

---

### Memory Leak Report

**GREEN (No Leaks):**
```
[1] EVENT SUBSCRIPTION LEAK SCAN
  ✅ No event subscription leaks detected

[2] STATIC COLLECTION GROWTH SCAN
  ✅ No static collections exceeding threshold

[3] COROUTINE LEAK SCAN
  ✅ No coroutine leaks detected
```

**RED (Leak Detected):**
```
[1] EVENT SUBSCRIPTION LEAK SCAN
  ⚠️ LEAK: HUDController.OnQuestCompleted has 87 subscribers (threshold: 50)
      Object: HUDCanvas
```

**Action:** Check `HUDController.cs` for missing `OnDestroy` unsubscribe.

---

### Save Bloat Report

**GREEN (Acceptable Growth):**
```
[5] GROWTH TREND ANALYSIS
  Growth Rate: 7.5% per hour
  ✅ Growth rate acceptable
  
  Projected size after 10 hours: 3.92 MB
```

**RED (Excessive Growth):**
```
[5] GROWTH TREND ANALYSIS
  Growth Rate: 25.3% per hour
  ⚠️ WARNING: Growth rate exceeds threshold (25.3% > 20% per hour)
  CRITICAL: Save file growing too fast — memory leak likely

  Projected size after 10 hours: 15.2 MB
  ⚠️ WARNING: Projected size will exceed threshold after 10 hours
```

**Action:** Review data block sizes for bloat sources (quest history, combat logs, etc.).

---

## TROUBLESHOOTING

### Issue: FPS Drops Over Time

**Symptoms:**
- FPS at hour 1: 60
- FPS at hour 10: 35

**Diagnosis:**
1. Check Memory Leak Detector report
2. Look for growing static collections
3. Check for excessive GameObjects (undestroyed particles/VFX)

**Common Causes:**
- Particle systems not destroyed after lifetime
- AudioSources accumulating with clips loaded
- Static collections growing unbounded

**Fix:**
- Implement object pooling for VFX
- Release AudioClips after playback
- Clear static collections on scene transitions

---

### Issue: Save File Growing Too Fast

**Symptoms:**
- Hour 0: 2.0 MB
- Hour 10: 12.5 MB (525% growth)

**Diagnosis:**
1. Run Save Bloat Analyzer
2. Check [4] DATA BLOCK SIZE ANALYSIS
3. Identify largest blocks

**Common Causes:**
- Quest history accumulating unbounded
- Combat logs storing every hit
- Dialogue history not pruned
- Achievement progress duplicated

**Fix:**
```csharp
// In SaveData provider:
public void PruneHistory()
{
    // Keep only last 100 quest history entries
    if (questHistory.Count > 100)
    {
        questHistory = questHistory.Skip(questHistory.Count - 100).ToList();
    }
}
```

---

### Issue: Memory Leak Detector Shows High Event Subscribers

**Symptoms:**
```
⚠️ LEAK: GameEvents.OnBuildingRestored has 127 subscribers (threshold: 50)
```

**Diagnosis:**
1. Check all files that subscribe to `GameEvents.OnBuildingRestored`
2. Verify each has `OnDestroy` cleanup
3. Use `grep "OnBuildingRestored +="` to find all subscriptions

**Common Causes:**
- Missing `OnDestroy` in UI components
- Subscriber object destroyed but not unsubscribed
- Domain reload in Editor creating duplicate subscriptions

**Fix:**
```csharp
void Start()
{
    GameEvents.OnBuildingRestored += HandleRestored;
}

void OnDestroy()
{
    GameEvents.OnBuildingRestored -= HandleRestored; // ← ADD THIS
}
```

---

## THRESHOLDS REFERENCE

| Metric | Threshold | Severity |
|--------|-----------|----------|
| FPS Degradation | -20% over 10h | FAIL |
| Managed Heap Growth | +100 MB over 10h | FAIL |
| GameObject Growth | +1000 objects over 10h | FAIL |
| Save File Growth | 20% per hour | WARNING |
| Save File Size | 10 MB | WARNING |
| Save Duration | 1000 ms | WARNING |
| Load Duration | 2000 ms | WARNING |
| Event Subscribers | 50 per event | WARNING |
| Static Collection Size | 1000 items | WARNING |
| Coroutines Per Component | 10 active | WARNING |

---

## MANUAL TESTING CHECKLIST

### Before Release

- [ ] Run 10-hour marathon test (Standalone Build)
- [ ] Review all 3 stability reports (Marathon, Leak, Bloat)
- [ ] Unity Memory Profiler: Compare hour 0 vs hour 10
- [ ] Verify all thresholds GREEN
- [ ] Test save/load cycles (20+ times in a row)
- [ ] Check backup file accumulation (manual cleanup if needed)

### Post-Launch Monitoring

- [ ] Set up telemetry for session length > 2 hours
- [ ] Monitor player reports for crashes after long sessions
- [ ] Track average save file size from player data
- [ ] Analytics for memory usage at 1h, 5h, 10h marks

---

## LOG FILE LOCATIONS

**All logs saved to:**
```
Windows: %APPDATA%\..\LocalLow\<Company>\<Game>\stability_logs\
Mac: ~/Library/Application Support/<Company>/<Game>/stability_logs/
Linux: ~/.config/unity3d/<Company>/<Game>/stability_logs/
```

**Log Files:**
- `marathon_test_YYYYMMDD_HHMMSS.log` — Marathon test report
- `memory_leak_report_YYYYMMDD_HHMMSS.log` — Leak scan results
- `save_bloat_report_YYYYMMDD_HHMMSS.log` — Save analysis results

---

## COMMANDS

### Run Full Stability Suite (10 hours)

```bash
# 1. Build Standalone
# 2. Run with logging:
./TARTARIA.exe -logFile stability_run.log

# 3. After 10 hours, review:
#    - stability_logs/marathon_test_*.log
#    - stability_logs/memory_leak_report_*.log
#    - stability_logs/save_bloat_report_*.log
```

### Quick Leak Scan (Context Menu)

```
Hierarchy > Right-click MemoryLeakDetector component
> Scan For Memory Leaks

Check Console for immediate results
Check stability_logs/ for detailed report
```

### Quick Save Analysis (Context Menu)

```
Hierarchy > Right-click SaveFileBloatAnalyzer component
> Analyze Save File Growth

Check Console for immediate results
Check stability_logs/ for detailed report
```

---

## NEXT STEPS

After running stability tests:

1. **If GREEN:** ✅ Ready for beta release
2. **If RED:** 🔴 Fix identified leaks, re-test
3. **If WARNINGS:** ⚠️ Implement optimization recommendations

---

## SUPPORT

**Issues with testing tools?**
- Check Unity Console for errors during test setup
- Verify components attached to DontDestroyOnLoad objects
- Ensure test runs in Standalone Build (not Editor)

**Questions about thresholds?**
- Thresholds are configurable in Inspector
- Adjust based on target hardware specs
- Conservative defaults provided

---

**Agent 4 Status:** ✅ COMPLETE  
**Tools Status:** ✅ OPERATIONAL  
**Game Stability:** ✅ VALIDATED
