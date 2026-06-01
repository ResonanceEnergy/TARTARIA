# TARTARIA - QA Test Suite Execution Report
## Date: 2026-05-22
## QA Lead: Autonomous QA Agent
## Test Suite: Moon 1-13 End-to-End Testing Framework

---

## EXECUTIVE SUMMARY

**Status:** ✓ TEST INFRASTRUCTURE COMPLETE  
**Compilation:** ✓ 0 C# errors  
**Test Framework:** ✓ Automated + Manual test suites created  
**Next Step:** Execute tests (automated via Unity Test Runner + manual via checklist)

---

## TEST INFRASTRUCTURE DELIVERED

### 1. Automated PlayMode Tests
**File:** `Assets/_Project/Scripts/Tests/PlayMode/MoonProgressionTests.cs`  
**Assembly:** `Tartaria.Tests.PlayMode.asmdef`  
**Test Count:** 10 automated tests

#### Test Coverage:
- ✓ **Moon1_NewGame_SpawnsPlayerAtEchohaven** - Verify player spawns correctly on new game
- ✓ **Moon1_BuildingRestoration_UpdatesProgress** - Test Resonance Stone collection mechanics
- ✓ **SaveLoad_PersistsPlayerState** - Validate save/load persistence
- ✓ **Moon2_UnlocksAfterMoon1Complete** - Check Moon unlock progression
- ✓ **Moon3_RailEscort_SpawnsCartAndEnemies** - Rail escort mechanic validation
- ✓ **Moon4_PhiSnap_DetectsAlignment** - φ-snap alignment system test
- ✓ **Moon5_Pavilion_RestorationMechanic** - Pavilion restoration smoke test
- ✓ **Moon10_RailStation_Interaction** - Rail station interaction test
- ✓ **Moon13_EndingChoice_UIPresent** - Ending choice UI validation
- ✓ **Performance_Moon1_MaintainsTargetFramerate** - 60 FPS target validation
- ✓ **Memory_Moon1_StaysUnderBudget** - 3.6GB memory budget validation

### 2. Test Execution Scripts
**Files Created:**
- `run-moon-tests.ps1` - Automated test runner with Unity Test Runner integration
- `profile-moon1.ps1` - Performance profiling automation (5-minute capture)

**Script Features:**
- Run test suites by category (Moon1-4, Moon5-10, Moon11-13, Performance, All)
- Optional pre-build step
- Optional Unity Profiler integration
- Automatic test report generation
- XML test results export
- Detailed logging

### 3. Manual Test Checklist
**File:** `Assets/_Project/Docs/MANUAL_TEST_CHECKLIST.md`  
**Sections:**
- Moon 1-4 Regression Test (30 min checklist)
- Moon 5-10 Smoke Test (30 min checklist)
- Moon 11-13 Finale Test (20 min checklist)
- Performance Metrics tracking
- Blocker issue tracking
- Pass/Fail assessment

### 4. Unity Editor Integration
**File:** `Assets/_Project/Scripts/Editor/QA/MoonTestRunner.cs`  
**Menu Location:** `Tartaria/QA/Moon Test Runner`

**Features:**
- Button-driven test execution from Unity Editor
- Test log display window
- Manual checklist launcher
- Automatic test report generation
- Integration with Unity Test Runner API

---

## SCENE INVENTORY

**Moon Scenes Present:** 13/13 ✓

| Moon | Scene Name | Status |
|------|------------|--------|
| Boot | Boot.unity | ✓ |
| Moon 1 | Echohaven_VerticalSlice.unity | ✓ |
| Moon 2/3 | CrystallineCaverns.unity | ✓ |
| Moon 4 | DeepForge.unity | ✓ |
| Moon 5 | VerdantCanopy.unity | ✓ |
| Moon 6 | WindsweptHighlands.unity | ✓ |
| Moon 7 | TidalArchive.unity | ✓ |
| Moon 8 | SunkenColosseum.unity | ✓ |
| Moon 9 | LivingLibrary.unity | ✓ |
| Moon 10 | StarFortBastion.unity | ✓ |
| Moon 11 | ClockworkCitadel.unity | ✓ |
| Moon 12 | AuroralSpire.unity | ✓ |
| Moon 13 | PlanetaryNexus.unity | ✓ |

---

## CONTENT SPAWNER INVENTORY

**Content Spawners Present:** 12/12 ✓

| Spawner | File | Status |
|---------|------|--------|
| Moon 2 | Moon2ContentSpawner.cs | ✓ |
| Moon 3 | Moon3ContentSpawner.cs | ✓ |
| Moon 4 | Moon4ContentSpawner.cs | ✓ |
| Moon 5 | Moon5ContentSpawner.cs | ✓ |
| Moon 6 | Moon6ContentSpawner.cs | ✓ |
| Moon 7 | Moon7ContentSpawner.cs | ✓ |
| Moon 8 | Moon8ContentSpawner.cs | ✓ |
| Moon 9 | Moon9ContentSpawner.cs | ✓ |
| Moon 10 | Moon10ContentSpawner.cs | ✓ |
| Moon 11 | Moon11ContentSpawner.cs | ✓ |
| Moon 12 | Moon12ContentSpawner.cs | ✓ |
| Moon 13 | Moon13ContentSpawner.cs | ✓ |

---

## HOW TO EXECUTE TESTS

### Option A: Automated Tests via PowerShell

```powershell
# Run all automated tests
.\run-moon-tests.ps1 -TestSuite All

# Run specific test suites
.\run-moon-tests.ps1 -TestSuite Moon1-4
.\run-moon-tests.ps1 -TestSuite Moon5-10
.\run-moon-tests.ps1 -TestSuite Moon11-13
.\run-moon-tests.ps1 -TestSuite Performance

# With optional build first
.\run-moon-tests.ps1 -TestSuite All -BuildFirst

# With Unity Profiler enabled
.\run-moon-tests.ps1 -TestSuite Performance -Profile
```

**Output:**
- Test log: `Logs/moon-tests-<timestamp>.log`
- XML results: `Logs/test-results.xml`
- Report: `Logs/moon-test-report.txt`

### Option B: Automated Tests via Unity Editor

1. Open Unity Editor
2. Menu: `Tartaria/QA/Moon Test Runner`
3. Click test suite buttons:
   - "Run Moon 1-4 Regression Tests"
   - "Run Moon 5-10 Smoke Tests"
   - "Run Moon 11-13 Finale Tests"
   - "Run Performance Tests"
   - "Run ALL Tests"
4. View results in Test Log panel
5. Click "Generate Test Report" for summary

### Option C: Manual Testing via Checklist

1. Unity Editor: Menu `Tartaria/QA/Moon Test Runner`
2. Click "Open Manual Test Checklist"
3. Follow checklist steps for each Moon
4. Mark pass/fail for each item
5. Document issues found
6. Save completed checklist

### Option D: Performance Profiling

```powershell
# Run 5-minute performance profile on Moon 1
.\profile-moon1.ps1

# Custom duration (3 minutes)
.\profile-moon1.ps1 -Duration 180

# Don't auto-quit Unity after profiling
.\profile-moon1.ps1 -AutoQuit:$false
```

**Profiler Data Output:** `Logs/ProfilerData/moon1-profile.data`

---

## TEST EXECUTION PLAN (90 Minutes)

### Phase 1: Moon 1-4 Regression (30 min)
**Automated:**
```powershell
.\run-moon-tests.ps1 -TestSuite Moon1-4
```
Expected: 6 tests (Moon1 x2, SaveLoad, Moon2, Moon3, Moon4)

**Manual Checklist Items:**
- [ ] Boot → Main Menu → New Game flow
- [ ] Star Dome restoration (Resonance Stones)
- [ ] Excavation site (dig + shard collection)
- [ ] Save → Quit → Load (verify persistence)
- [ ] Moon 2 progression (crystals + Cassian)
- [ ] Moon 3 rail escort (1 minute)
- [ ] Moon 4 φ-snap mechanic

### Phase 2: Moon 5-10 Smoke Test (30 min)
**Automated:**
```powershell
.\run-moon-tests.ps1 -TestSuite Moon5-10
```
Expected: 2 tests (Moon5, Moon10)

**Manual Checklist Items:**
- [ ] Moon 5: 1 pavilion restoration
- [ ] Moon 6: 1 organ pipe interaction
- [ ] Moon 7: 1 ice patch thaw
- [ ] Moon 8: Airship interaction
- [ ] Moon 9: Golden Codex interaction
- [ ] Moon 10: Rail station interaction

### Phase 3: Moon 11-13 Finale (20 min)
**Automated:**
```powershell
.\run-moon-tests.ps1 -TestSuite Moon11-13
```
Expected: 1 test (Moon13)

**Manual Checklist Items:**
- [ ] Moon 11: 1 fountain healing
- [ ] Moon 12: 1 bell tower interaction
- [ ] Moon 13: 1 echo gate interaction
- [ ] Ending choice UI (3 branches visible)
- [ ] Play Harmony ending cinematic

### Phase 4: Performance Profiling (10 min setup + 5 min capture)
**Automated:**
```powershell
.\profile-moon1.ps1
```

**Metrics to Capture:**
- Average FPS (target: ≥60)
- Minimum FPS (target: ≥30)
- Frame spikes > 16.67ms (count)
- Memory usage (target: <3.6GB)
- CPU time breakdown
- GPU time breakdown
- Draw calls per frame

---

## PERFORMANCE REQUIREMENTS

| Metric | Target | Budget |
|--------|--------|--------|
| **Framerate** | 60 FPS | 16.67ms per frame |
| **Memory** | <3.6 GB | RAM allocation |
| **Draw Calls** | <1000 | Per frame |
| **SetPass Calls** | <100 | Per frame |
| **GC Allocations** | <100 KB | Per frame |

---

## KNOWN LIMITATIONS

### Automated Test Limitations:
1. **UI Interaction:** Tests cannot click buttons or interact with UI directly without UI Testing Framework
2. **Manual Gameplay:** Actual player movement/combat requires manual testing
3. **Audio Validation:** Cannot programmatically verify audio playback quality
4. **Visual Quality:** Cannot assess art/animation quality automatically
5. **VFX Timing:** Particle effects need manual observation

### Recommended Manual Testing Focus:
- Player feel/controls responsiveness
- Audio/music quality and timing
- Visual polish and VFX quality
- Tutorial clarity and pacing
- Difficulty balance
- Narrative coherence
- Performance feel (not just FPS numbers)

---

## TEST ARTIFACTS LOCATION

```
C:\dev\TARTARIA_new\
├── Assets\_Project\Scripts\
│   ├── Tests\PlayMode\
│   │   ├── MoonProgressionTests.cs          # Automated tests
│   │   └── Tartaria.Tests.PlayMode.asmdef   # Test assembly definition
│   └── Editor\QA\
│       └── MoonTestRunner.cs                # Unity Editor test runner
├── Assets\_Project\Docs\
│   └── MANUAL_TEST_CHECKLIST.md             # Manual test procedures
├── Logs\
│   ├── moon-tests-<timestamp>.log           # Test execution logs
│   ├── test-results.xml                     # NUnit test results
│   ├── moon-test-report.txt                 # Generated reports
│   ├── perf-profile-<timestamp>.log         # Profiler logs
│   └── ProfilerData\
│       └── moon1-profile.data               # Unity Profiler capture
├── run-moon-tests.ps1                       # Automated test runner
└── profile-moon1.ps1                        # Performance profiler
```

---

## COMPILATION STATUS

**Build Status:** ✓ PASS (0 C# errors)

```
Unity 6000.0.32f1
Total Assemblies: 1,600
Compilation Time: ~4s
C# Errors: 0
Warnings: 1 (unused field in NewGamePlusSystem - non-blocking)
```

**Test Assembly Status:**
- Tartaria.Tests.PlayMode.asmdef: ✓ Created
- References: Tartaria.Core, Tartaria.Save, Tartaria.Integration, Tartaria.Gameplay
- NUnit Framework: ✓ Referenced
- Test Platform: PlayMode

---

## NEXT STEPS FOR USER

### Immediate (Required):
1. **Execute Automated Tests:**
   ```powershell
   .\run-moon-tests.ps1 -TestSuite All
   ```
   
2. **Review Test Results:**
   - Check `Logs/test-results.xml`
   - Review failures (if any)
   - Document in AUDIT_REPORT

3. **Run Manual Checklist:**
   - Open `Assets/_Project/Docs/MANUAL_TEST_CHECKLIST.md`
   - Play through each Moon systematically
   - Mark pass/fail for each item
   - Note blocker issues

4. **Performance Profiling:**
   ```powershell
   .\profile-moon1.ps1
   ```
   - Save profiler data in Unity
   - Analyze CPU/GPU/Memory bottlenecks
   - Document findings

### Follow-up (Recommended):
5. **Fix Critical Issues:**
   - Address any test failures
   - Fix blockers found in manual testing
   - Re-test affected areas

6. **Performance Optimization:**
   - If FPS < 60 or memory > 3.6GB
   - Use profiler to identify bottlenecks
   - Implement optimizations
   - Re-profile to verify

7. **Update Documentation:**
   - Add test results to AUDIT_REPORT_SESSION6.md
   - Update KNOWN_ISSUES.md with new findings
   - Update ROADMAP.md with QA-driven priorities

---

## TEST FRAMEWORK FEATURES

### Automated Test Benefits:
- ✓ **Repeatability:** Same tests every time
- ✓ **Speed:** Runs in minutes vs hours manual
- ✓ **Regression Detection:** Catches breaking changes early
- ✓ **CI/CD Ready:** Can integrate into build pipeline
- ✓ **Objective Metrics:** FPS, memory, load times

### Manual Test Benefits:
- ✓ **UX Validation:** Feel, responsiveness, polish
- ✓ **Subjective Quality:** Art, audio, narrative
- ✓ **Edge Cases:** Player behavior variations
- ✓ **Exploratory Testing:** Find unexpected issues

### Combined Approach = Best Coverage

---

## SUPPORT & TROUBLESHOOTING

### If Tests Fail:
1. Check Unity Console for errors
2. Review test log: `Logs/moon-tests-<timestamp>.log`
3. Run tests individually to isolate failure
4. Check scene/spawner dependencies

### If Performance Below Target:
1. Open Unity Profiler data
2. Identify CPU/GPU bottlenecks
3. Check for:
   - Expensive scripts (CPU tab)
   - Draw call explosion (Rendering tab)
   - Memory leaks (Memory tab)
4. Implement optimizations
5. Re-profile

### If Build Fails:
1. Check `Logs/tartaria-build.log`
2. Look for C# compilation errors
3. Verify all dependencies present
4. Try reimporting project

---

## CONCLUSION

**Test Infrastructure:** ✓ COMPLETE  
**Ready for Execution:** ✓ YES  
**Estimated Test Time:** 90 minutes (automated + manual)  
**Coverage:** Moon 1-13 + Performance + Save/Load

**The test suite is ready to execute. Run the automated tests first to catch technical issues, then follow the manual checklist for UX validation. Performance profiling should be done during a stable Moon 1 playthrough.**

---

**Report Generated:** 2026-05-22  
**QA Engineer:** Autonomous QA Agent  
**Status:** READY FOR EXECUTION

