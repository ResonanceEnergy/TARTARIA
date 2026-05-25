# TARTARIA Integration Test Suite - Quick Start Guide

## RUN TESTS (3 Methods)

### Method 1: PowerShell Script (Recommended)
```powershell
cd C:\dev\TARTARIA_new
.\run-automated-tests.ps1
```

### Method 2: Unity Editor (Interactive)
1. Open: Assets/_Project/Scenes/Echohaven.unity
2. Find GameObject with TestOrchestrator component
3. Press Play in Unity Editor
4. Watch Console for [AutoTest] logs
5. OR press **T** key to manually trigger tests

### Method 3: Test Runner (Individual Phases)
1. Window → General → Test Runner
2. PlayMode tab → Tartaria.Tests
3. Run individual phases or full suite

---

## TEST PHASES (15 Total)

**Component Tests (1-10):**
- Phase 1: Data Asset Validation
- Phase 2: Singleton Systems
- Phase 3: Save/Load Cycle
- Phase 4: Inventory System
- Phase 5: Equipment System
- Phase 6: Player Progression
- Phase 7: Combat Mechanics
- Phase 8: Performance Baseline
- Phase 9: Player Spawn
- Phase 10: Performance Profiling

**Integration & E2E Tests (11-15):**
- Phase 11: Combat → Quest Integration
- Phase 12: Economy → Inventory Integration
- Phase 13: Dialogue → Quest Integration
- Phase 14: Save/Load → All Systems
- Phase 15: Full Gameplay Loop (E2E)

---

## EXPECTED RESULTS

**Console Output:**
```
[AutoTest] TARTARIA — Automated Test Suite
[AutoTest] Test Phases: 15
[AutoTest] Starting: Phase 1...
[AutoTest] [PASS] Phase 1: ...
...
[AutoTest] FINAL TEST REPORT
[AutoTest] TOTAL: 185 passed, 0 failed, 14 warnings, 0 timeouts
[AutoTest] ✓ ALL TESTS PASSED
```

**Test Duration:** ~2-5 minutes (all 15 phases)

**Output Location:** 
- Console: Unity Editor Console window
- Log file: `Logs/test-run.log` (if using PowerShell script)

---

## TROUBLESHOOTING

**Problem: Tests don't start**
- Verify TestOrchestrator attached to GameObject in Echohaven scene
- Set `autoStartOnPlay = true` in Inspector
- Press **T** key to manually trigger

**Problem: Phases 2-15 timeout**
- Check Console for initialization errors
- Verify all singletons (InventorySystem, QuestManager, etc.) exist in scene
- Increase `phaseTimeout` to 60s in TestOrchestrator Inspector

**Problem: Quest tests fail**
- Ensure QuestDatabaseBuilder has at least 1 DefeatEnemies quest
- Check QuestManager singleton initialized
- Verify GameLoopController exists

**Problem: Economy tests fail**
- Check PlayerProgression singleton
- Verify InventorySystem initialized
- Ensure ItemData assets exist in Resources/Items/

**Problem: Dialogue tests warn**
- DialogueManager is optional - warnings are expected if not present
- CassianNPCController needed for Trust tests
- WorldChoiceTracker needed for world choice tests

---

## KNOWN LIMITATIONS

⚠️ **Batchmode NOT supported** for Phases 2-15
- Only Phase 1 runs in `-batchmode`
- Use interactive play mode (via run-automated-tests.ps1)

⚠️ **Assembly boundary constraint**
- No Tartaria.AI references (by design)
- AI behavior tested indirectly via integration points

⚠️ **Reflection-based gold manipulation**
- PlayerProgression.SetGold() not public
- Tests use reflection to modify _gold field

---

## NEXT STEPS

1. Run full test suite: `.\run-automated-tests.ps1`
2. Review results in Console / Logs/test-run.log
3. Address any [FAIL] assertions
4. Investigate [WARN] logs for improvements
5. Commit test classes to version control

---

For detailed documentation, see: `INTEGRATION_TEST_SUITE_REPORT.md`
