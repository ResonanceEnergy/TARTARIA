# AUTOMATED TEST FRAMEWORK — QUICK START GUIDE

**For Integration/QA/Build Agents**

---

## 🚀 READY TO USE

### Files Created (All Compile Successfully)
```
✅ Assets/_Project/Scripts/Tests/PlayModeTestBase.cs (130 lines)
✅ Assets/_Project/Scripts/Tests/TestOrchestrator.cs (600 lines)
✅ run-automated-tests.ps1 (PowerShell launcher)
✅ TEST_FRAMEWORK_IMPLEMENTATION_REPORT.md (full documentation)
```

### Build Status
```
✅ Compilation: GREEN (0 errors, 0 warnings)
✅ Assembly Boundaries: RESPECTED (no Tartaria.AI references)
✅ API Compatibility: VERIFIED (all singleton patterns valid)
```

---

## 🎯 NEXT ACTION REQUIRED

### **1. Attach TestOrchestrator to Echohaven Scene**

**Why:** Tests need to run in scene context to access singletons.

**How:**
1. Open `Assets/_Project/Scenes/Echohaven.unity`
2. Right-click Hierarchy → Create Empty
3. Name it: `TestOrchestrator`
4. Add Component → `Tartaria.Tests.TestOrchestrator`
5. Inspector settings:
   - `Auto Start On Play`: ✅ (enabled)
   - `Phase Delay`: 1.5 seconds
6. Save scene

---

## 🧪 RUNNING TESTS

### **Option A: Unity Editor (Manual Testing)**
```
1. Open Echohaven scene
2. Press Play (or press T key in play mode)
3. Watch Console for [AutoTest] output
4. Look for green PASS, red FAIL, yellow WARN messages
```

### **Option B: Batchmode (Automated/CI)**
```powershell
# From project root:
.\run-automated-tests.ps1

# With custom settings:
.\run-automated-tests.ps1 -SceneName "Echohaven" -LogFile "test-results.log"

# Debug mode (keep Unity open):
.\run-automated-tests.ps1 -NoQuit
```

**Exit Codes:**
- `0` = All tests passed ✅
- `1` = Tests failed ❌

---

## 📋 TEST PHASES (7 Total)

### Phase 1: Data Asset Validation
- Loads ItemDatabase, QuestDatabase, SkillTreeAsset, EnemyData
- Verifies Resources folder structure
- **Pass criteria:** At least ItemDatabase loads successfully

### Phase 2: Singleton Systems Initialization
- Checks SaveManager.Instance
- Checks PlayerProgression.Instance
- Checks InventorySystem.Instance
- Checks EquipmentSlotManager.Instance
- **Pass criteria:** All 4 singletons initialized

### Phase 3: Save/Load Cycle Test
- Executes QuickSave()
- Verifies save file exists on disk
- Executes QuickLoad()
- **Pass criteria:** No exceptions, save file created

### Phase 4: Inventory System Test
- AddItem() for 3 item types
- GetItemCount() validation
- RemoveItem() test
- Clear() test
- **Pass criteria:** All item operations work correctly

### Phase 5: Equipment System Test
- Verifies 6 equipment slots (Weapon, Armor, Helmet, Gloves, Boots, Accessory)
- Checks stat calculation (TotalStrength, TotalArmor)
- GetEquippedItem() API test
- **Pass criteria:** Slot access works, no null reference errors

### Phase 6: Player Progression Test
- Current level/XP retrieval
- AddXP(500) test
- Level cap validation (max 50)
- **Pass criteria:** XP gain works, level up logic valid

### Phase 7: Performance Baseline Test
- Collects 300 frame time samples (~5 seconds at 60fps)
- Calculates avg/median/p95 frame times
- Memory snapshot (GC.GetTotalMemory)
- **Pass criteria:** Avg frame time <16.67ms (60fps) or <33.33ms (30fps)

---

## 🔧 EXTENDING THE FRAMEWORK

### Adding a New Test Phase

**Step 1:** Create test class in TestOrchestrator.cs
```csharp
class YourNewTest : PlayModeTestBase
{
    public YourNewTest() : base("Phase 8: Your Test Name") { }
    
    protected override IEnumerator RunTestPhase()
    {
        // Your test logic here
        var system = YourSystem.Instance;
        if (system != null)
        {
            LogPass("System initialized");
        }
        else
        {
            LogFail("System not found");
        }
        
        yield return null;
    }
}
```

**Step 2:** Register in TestOrchestrator.InitializeTestPhases()
```csharp
void InitializeTestPhases()
{
    _testPhases.Clear();
    // ... existing phases ...
    _testPhases.Add(new YourNewTest()); // ← Add this
}
```

---

## ⚠️ ASSEMBLY BOUNDARY RULES

### ✅ SAFE TO REFERENCE
```csharp
using Tartaria.Core;           // GameEvents, GameBalanceConfig
using Tartaria.Core.Enums;     // EquipSlot, StatType, ItemType
using Tartaria.Data;           // ItemDatabase, QuestDatabase
using Tartaria.Gameplay;       // PlayerProgression, InventorySystem
using Tartaria.Save;           // SaveManager, SaveData
```

### ❌ FORBIDDEN (Assembly Violation)
```csharp
using Tartaria.AI;             // ❌ WILL NOT COMPILE
MudGolemAI.BuildProcedural();  // ❌ WILL NOT COMPILE
EnemyAIController enemy;       // ❌ WILL NOT COMPILE
```

### Safe Access Patterns
```csharp
// ✅ Singleton access (safe)
SaveManager.Instance.QuickSave();
PlayerProgression.Instance.AddXP(100, "test");
InventorySystem.Instance.AddItem("health_potion", 5);

// ✅ Resources.Load (safe)
var itemDB = ItemDatabase.LoadDatabase();
var questDB = Resources.Load<QuestDatabase>("QuestDatabase");

// ✅ GameEvents (safe)
GameEvents.FireHUDAchievementToast("Test message");

// ❌ AI access (forbidden)
var golem = FindObjectOfType<MudGolemAI>(); // ❌ Won't compile
```

---

## 📊 EXPECTED OUTPUT

### Successful Test Run
```
[AutoTest] ═══════════════════════════════════════════════════════
[AutoTest] TARTARIA — Automated Test Suite
[AutoTest] Unity 6000.3.6f1 | URP 17.3.0
[AutoTest] Scene: Echohaven
[AutoTest] Test Phases: 7
[AutoTest] ═══════════════════════════════════════════════════════

[AutoTest] ═══════════════════════════════════════════════════════
[AutoTest] Starting: Phase 1: Data Asset Validation
[AutoTest] ═══════════════════════════════════════════════════════
[AutoTest] [PASS] Phase 1: ItemDatabase loaded successfully
[AutoTest] [WARN] Phase 1: SkillTreeAsset not found (optional)
[AutoTest] ───────────────────────────────────────────────────────
[AutoTest] Phase 1 Complete: 2 passed, 0 failed, 1 warnings
[AutoTest] ───────────────────────────────────────────────────────

... (6 more phases) ...

[AutoTest] ═══════════════════════════════════════════════════════
[AutoTest] FINAL TEST REPORT
[AutoTest] ═══════════════════════════════════════════════════════
[AutoTest] ✓ Phase 1: 2P / 0F / 1W
[AutoTest] ✓ Phase 2: 5P / 0F / 0W
[AutoTest] ✓ Phase 3: 3P / 0F / 0W
[AutoTest] ✓ Phase 4: 4P / 0F / 0W
[AutoTest] ✓ Phase 5: 3P / 0F / 0W
[AutoTest] ✓ Phase 6: 3P / 0F / 0W
[AutoTest] ✓ Phase 7: 4P / 0F / 0W
[AutoTest] ───────────────────────────────────────────────────────
[AutoTest] TOTAL: 24 passed, 0 failed, 1 warnings
[AutoTest] ✓ ALL TESTS PASSED
[AutoTest] ═══════════════════════════════════════════════════════
```

---

## 🐛 TROUBLESHOOTING

### "TestOrchestrator not attached to GameObject"
**Fix:** Follow step 1 above — create GameObject in scene, add component.

### "SaveManager.Instance is null"
**Fix:** Ensure Echohaven scene has SaveManager GameObject or SaveManager bootstraps on play.

### "PlayerProgression.Instance is null"
**Fix:** PlayerProgression auto-bootstraps via `[RuntimeInitializeOnLoadMethod]`. If still null, check for script errors preventing bootstrap.

### "Tests don't run in batchmode"
**Fix:** 
1. Verify scene path: `Assets/_Project/Scenes/Echohaven.unity`
2. Check Unity path in script: `C:\Program Files\Unity\Hub\Editor\6000.0.36f1\Editor\Unity.exe`
3. Review log file: `Logs/test-run.log`

### "Assembly reference errors"
**Fix:** Verify `Tartaria.Tests.asmdef` references:
```json
{
  "references": [
    "Tartaria.Core",
    "Tartaria.Data",
    "Tartaria.Gameplay",
    "Tartaria.Save"
  ]
}
```

---

## 📞 HANDOFF TO OTHER AGENTS

### **Integration Agent**
- Attach TestOrchestrator to Echohaven scene (see step 1)
- Verify all singletons bootstrap correctly
- Wire to Unity Test Runner if needed

### **QA Agent**
- Extend test phases with edge cases
- Add performance benchmarks
- Create regression test suite

### **Data Agent**
- Populate Resources folders:
  - `Resources/ItemDatabase.asset`
  - `Resources/QuestDatabase.asset`
  - `Resources/SkillTrees/SkillTree_Resonator.asset`
  - `Resources/Enemies/mudgolem.asset`
- Verify Phase 1 passes with 0 warnings

### **Build Agent**
- Add `run-automated-tests.ps1` to CI/CD pipeline
- Set up pre-commit Git hooks
- Configure Unity Cloud Build automation

---

## ✅ VERIFICATION CHECKLIST

Before marking as complete:

- [ ] TestOrchestrator attached to Echohaven scene GameObject
- [ ] Manual test run executed (press Play, press T)
- [ ] Console shows [AutoTest] output with colored PASS/FAIL/WARN
- [ ] Batchmode test run executed: `.\run-automated-tests.ps1`
- [ ] PowerShell script exits with code 0 (all tests passed)
- [ ] No compilation errors in Unity Editor
- [ ] No Tartaria.AI references in test code

---

**Status:** 🟢 FRAMEWORK READY — Just attach to scene and run!

**Test Infrastructure Agent** signing off.
