# TARTARIA QA Test Suite - Quick Start Guide
## Ready to Execute - 90 Minute Test Plan

---

## WHAT WAS DELIVERED

✓ **10 Automated PlayMode Tests** - Moon progression, save/load, performance  
✓ **2 PowerShell Execution Scripts** - Automated test runner + profiler  
✓ **Unity Editor Test Runner** - GUI-based test execution  
✓ **Manual Test Checklist** - 90-minute structured test plan  
✓ **Comprehensive Documentation** - See `TEST_SUITE_REPORT.md`

**Compilation Status:** ✓ 0 C# errors

---

## OPTION 1: Automated Tests (Fastest - 10 minutes)

### Run All Tests:
```powershell
cd C:\dev\TARTARIA_new
.\run-moon-tests.ps1 -TestSuite All
```

**What it does:**
- Launches Unity in batch mode
- Runs 10 automated PlayMode tests
- Generates XML results + report
- Shows pass/fail summary

**Output:**
- `Logs/test-results.xml` - NUnit results
- `Logs/moon-test-report.txt` - Summary report
- `Logs/moon-tests-<timestamp>.log` - Detailed log

### Run Specific Test Suites:
```powershell
.\run-moon-tests.ps1 -TestSuite Moon1-4      # Regression tests
.\run-moon-tests.ps1 -TestSuite Moon5-10     # Smoke tests
.\run-moon-tests.ps1 -TestSuite Moon11-13    # Finale tests
.\run-moon-tests.ps1 -TestSuite Performance  # FPS + memory
```

---

## OPTION 2: Unity Editor GUI (Visual - 15 minutes)

1. Open Unity Editor
2. Menu: **`Tartaria/QA/Moon Test Runner`**
3. Click test buttons:
   - "Run Moon 1-4 Regression Tests"
   - "Run Moon 5-10 Smoke Tests"
   - "Run Moon 11-13 Finale Tests"
   - "Run Performance Tests"
   - "Run ALL Tests"
4. View results in Test Log panel
5. Click "Generate Test Report"

**Alternative:** Open Unity's built-in Test Runner:
- Menu: `Window > General > Test Runner`
- Click "PlayMode" tab
- Click "Run All" or select individual tests

---

## OPTION 3: Manual Testing (Thorough - 90 minutes)

### Open the Checklist:
**Via Unity Editor:**
1. Menu: `Tartaria/QA/Moon Test Runner`
2. Click "Open Manual Test Checklist"

**Via File Explorer:**
- Open: `Assets\_Project\Docs\MANUAL_TEST_CHECKLIST.md`

### Test Plan:
- **Moon 1-4 Regression:** 30 min
  - New game flow, building restoration, save/load
- **Moon 5-10 Smoke Test:** 30 min  
  - Quick validation of each Moon's core mechanic
- **Moon 11-13 Finale:** 20 min
  - Ending choice UI + cinematic
- **Performance Profiling:** 10 min
  - Unity Profiler capture

---

## OPTION 4: Performance Profiling (5-minute capture)

```powershell
cd C:\dev\TARTARIA_new
.\profile-moon1.ps1
```

**What it does:**
1. Launches Unity Editor with project
2. Prompts you to:
   - Open Profiler window (`Window > Analysis > Profiler`)
   - Enable "Record" button
   - Press Play and load Moon 1
3. Waits 5 minutes (300 seconds)
4. Prompts you to save profiler data
5. Generates performance report

**Output:**
- `Logs/ProfilerData/moon1-profile.data` - Unity Profiler capture
- `Logs/perf-profile-report.txt` - Performance summary

**Metrics to Check:**
- Average FPS (target: ≥60)
- Memory usage (target: <3.6GB)
- CPU frame time (target: <16.67ms)
- GPU time (target: <16.67ms)

---

## RECOMMENDED WORKFLOW

### For Quick Validation (15 min):
```powershell
# 1. Run automated tests
.\run-moon-tests.ps1 -TestSuite All

# 2. Check results
Get-Content Logs\test-results.xml  # See pass/fail
Get-Content Logs\moon-test-report.txt  # See summary
```

### For Full QA Pass (90 min):
1. **Automated Tests (10 min)**
   ```powershell
   .\run-moon-tests.ps1 -TestSuite All
   ```

2. **Manual Checklist (60 min)**
   - Boot → Main Menu → New Game
   - Test each Moon's core mechanic
   - Save/load validation
   - Ending UI check

3. **Performance Profile (15 min)**
   ```powershell
   .\profile-moon1.ps1
   ```

4. **Report Findings (5 min)**
   - Update `AUDIT_REPORT_SESSION6.md`
   - Document blocker issues
   - Note performance metrics

---

## INTERPRETING RESULTS

### Automated Tests - Success Criteria:
✓ **10/10 tests pass** = Ready for manual testing  
✗ **Any test fails** = Fix issue and re-run

### Manual Checklist - Success Criteria:
✓ **All critical flows work** = Beta-ready  
✗ **Blocker issues found** = Fix and re-test

### Performance - Success Criteria:
✓ **FPS ≥60, Memory <3.6GB** = Ship-ready  
⚠ **FPS 30-60, Memory <4GB** = Needs optimization  
✗ **FPS <30, Memory >4GB** = Major performance work needed

---

## TROUBLESHOOTING

### If Automated Tests Fail:
1. Check Unity Console for errors
2. Review `Logs/moon-tests-<timestamp>.log`
3. Run individual test suite to isolate failure
4. Verify scenes/spawners exist

### If Build Doesn't Compile:
```powershell
.\tartaria-play.ps1 -BatchOnly -NoMonitor
# Check Logs\tartaria-build.log for errors
```

### If Unity Test Runner Doesn't Show Tests:
1. Verify `Tartaria.Tests.PlayMode.asmdef` exists
2. Reimport test files (right-click > Reimport)
3. Restart Unity Editor

### If Performance Below Target:
1. Open Unity Profiler window
2. Load `Logs/ProfilerData/moon1-profile.data`
3. Check CPU/GPU/Memory tabs
4. Identify bottlenecks
5. See `TEST_SUITE_REPORT.md` for optimization tips

---

## WHAT TO DO WITH RESULTS

### After Running Tests:

1. **Review Results:**
   - `Logs/test-results.xml` - Detailed pass/fail
   - `Logs/moon-test-report.txt` - Summary

2. **Document Findings:**
   - Update `AUDIT_REPORT_SESSION6.md` with results
   - Add new issues to `KNOWN_ISSUES.md`
   - Update `ROADMAP.md` priorities

3. **Fix Critical Issues:**
   - Address any test failures first
   - Fix manual checklist blockers
   - Optimize performance if needed

4. **Re-test:**
   - Run tests again after fixes
   - Verify issues resolved
   - Update documentation

---

## TEST COVERAGE SUMMARY

### What's Tested:
✓ New game creation + player spawn  
✓ Building restoration mechanics  
✓ Save/load persistence  
✓ Moon unlock progression  
✓ Content spawner instantiation  
✓ Performance (FPS + memory)  

### What's NOT Tested (Manual Required):
✗ UI button clicks / interactions  
✗ Player movement feel  
✗ Combat mechanics  
✗ Audio quality  
✗ Visual polish / VFX  
✗ Narrative coherence  

---

## NEXT STEPS

**Immediate (Do Now):**
```powershell
cd C:\dev\TARTARIA_new
.\run-moon-tests.ps1 -TestSuite All
```

**Then:**
1. Review test results
2. Fix any failures
3. Run manual checklist
4. Profile performance
5. Update AUDIT_REPORT

**Questions?**
- See `TEST_SUITE_REPORT.md` for detailed documentation
- Check test code: `Assets\_Project\Scripts\Tests\PlayMode\MoonProgressionTests.cs`
- Review test runner: `Assets\_Project\Scripts\Editor\QA\MoonTestRunner.cs`

---

**Test Infrastructure by:** Autonomous QA Agent  
**Date Created:** 2026-05-22  
**Status:** ✓ READY TO EXECUTE  
**Time Budget:** 10-90 minutes (depending on test depth)

