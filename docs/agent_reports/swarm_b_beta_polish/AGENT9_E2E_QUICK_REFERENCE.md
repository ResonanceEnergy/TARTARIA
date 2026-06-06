# TARTARIA — E2E Test Quick Reference

**Agent 9 Deliverable: Comprehensive End-to-End Player Journey Tests**

---

## Overview

5 automated test scenarios covering complete player journeys from 0-100% completion.

---

## Test Scenarios

### 1. New Player Journey (0-10h)
- **Runtime:** ~10 minutes
- **Coverage:** Tutorial, Moon 1-3, Level 1-30, First boss
- **Run:** `.\run-e2e-tests.ps1 -TestCategory NewPlayer`

### 2. Mid-Game Journey (10-30h)
- **Runtime:** ~15 minutes
- **Coverage:** Moon 4-8, Level 30-70, Equipment, Skills, Companions
- **Run:** `.\run-e2e-tests.ps1 -TestCategory MidGame`

### 3. Endgame Journey (30-50h)
- **Runtime:** ~20 minutes
- **Coverage:** Moon 9-13, Level 70-100, Final boss, All endings
- **Run:** `.\run-e2e-tests.ps1 -TestCategory Endgame`

### 4. Critical Path Journey (~20h)
- **Runtime:** ~10 minutes
- **Coverage:** Main story only, Zero side content, Blocker detection
- **Run:** `.\run-e2e-tests.ps1 -TestCategory CriticalPath`

### 5. Completionist Journey (100%)
- **Runtime:** ~30 minutes
- **Coverage:** All 390 quests, All achievements, All collectibles
- **Run:** `.\run-e2e-tests.ps1 -TestCategory Completionist`

---

## Quick Commands

```powershell
# Run full E2E suite (all 5 journeys)
.\run-e2e-tests.ps1

# Run only critical path (fastest validation)
.\run-e2e-tests.ps1 -Quick

# Run specific journey
.\run-e2e-tests.ps1 -TestCategory NewPlayer

# Generate report from existing logs
.\run-e2e-tests.ps1 -Report
```

---

## Test Files

- **Orchestrator:** `Assets/_Project/Scripts/Tests/PlayMode/E2ETestOrchestrator.cs`
- **New Player:** `E2EJourney_NewPlayer.cs`
- **Mid-Game:** `E2EJourney_MidGame.cs`
- **Endgame:** `E2EJourney_Endgame.cs`
- **Critical Path:** `E2EJourney_CriticalPath.cs`
- **Completionist:** `E2EJourney_Completionist.cs`

---

## Output

- **Test Log:** `TestResults/E2E/e2e-test-log.txt`
- **XML Results:** `TestResults/E2E/e2e-test-results.xml`
- **Report:** `BETA_E2E_TEST_REPORT.md`

---

## Success Criteria

| Journey | Pass Condition |
|---------|----------------|
| New Player | 0 failures, tutorial complete, level 30+, first boss defeated |
| Mid-Game | 0 failures, level 70+, 8 moons cleared, equipment/skills unlocked |
| Endgame | 0 failures, level 100, all 13 moons, final boss defeated, 3 endings |
| Critical Path | **ZERO BLOCKERS** — game completable via main story alone |
| Completionist | 95%+ completion, all content accessible, platinum unlocked |

---

## Troubleshooting

### Tests fail to run
- Verify Unity path in script: `C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe`
- Check project path: `c:\dev\TARTARIA_new`
- Ensure all test files have `.meta` files

### Critical path blockers detected
- **PRIORITY FIX:** Side content must NOT be required for main story
- Review quest dependencies
- Test manual playthrough with main quests only

### Completionist test warnings
- Check for missing collectibles
- Verify all achievements are unlockable
- Ensure all gear/skills are discoverable

---

## Integration with CI/CD

```yaml
# GitHub Actions example
- name: Run E2E Tests
  run: |
    pwsh -File run-e2e-tests.ps1 -Quick
    
- name: Upload E2E Report
  uses: actions/upload-artifact@v3
  with:
    name: e2e-test-report
    path: BETA_E2E_TEST_REPORT.md
```

---

## Manual Testing Supplement

E2E tests are **simulated** and may not catch all issues. Supplement with:
- Human playtesting (1 full playthrough per journey)
- Edge case testing (skip tutorials, sequence breaks)
- Performance testing on min-spec hardware
- Telemetry validation (track real player progression)

---

## Next Steps After GREEN

1. ✅ All E2E tests pass → **BETA READY**
2. Deploy beta build to testers
3. Monitor telemetry for real-world validation
4. Run performance profiling (Agent 8 tests)
5. Address any beta feedback
6. Run E2E suite again before final release

---

*Agent 9 Complete — Zero Progression Blockers*
