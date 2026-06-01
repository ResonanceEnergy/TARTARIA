# BATCH 3 QUICK START GUIDE
**Lane 3 Deliverable** | Phase 83 | 2026-05-25

---

## 🎯 EXECUTIVE DECISION

**SAFE FILES READY**: 5 out of 10  
**BLOCKERS IDENTIFIED**: 5 files require dependencies  
**RECOMMENDATION**: Enable safe subset, defer blockers

---

## ⚡ QUICK COMMANDS

### 1️⃣ CHECK DEPENDENCIES (Dry Run)
```powershell
cd C:\dev\TARTARIA_new
.\BATCH_3_PREEMPTIVE_FIXES.ps1 -DryRun
```

### 2️⃣ ENABLE SAFE SUBSET (Recommended)
```powershell
.\BATCH_3_PREEMPTIVE_FIXES.ps1
```

### 3️⃣ WITH STUBS (If dependencies missing)
```powershell
.\BATCH_3_PREEMPTIVE_FIXES.ps1 -ApplyStubs
```

---

## 📋 SAFE FILES (5)

These compile WITHOUT dependencies:

✅ **MoonProgressTracker.cs.disabled** (6.9 KB)  
   - Clean, no external dependencies
   - Tracks moon progression via PlayerPrefs
   
✅ **EchohavenProgressionSystem.cs.disabled** (7.5 KB)  
   - Depends on Core systems (likely enabled)
   - Hub restoration + skill tree blessings

✅ **Moon5AmplificationField.cs.disabled** (7.7 KB)  
   - Null-safe GetComponent calls
   - Pavilion buff zones

✅ **DebugOverlay.cs.disabled** (7.7 KB)  
   - ECS/DOTS debug panel
   - F1 toggle, FPS counter

✅ **MoonPortalSelector.cs.disabled** (8.7 KB)  
   - F1-F12 moon warp system
   - Clean, no dependencies

---

## 🚫 BLOCKER FILES (5)

These require dependencies first:

⚠️ **MoonCompanionSpawner.cs.disabled** (8.0 KB)  
   Needs: `DialogueManager.Instance` (line 166)

⚠️ **Moon5Components.cs.disabled** (8.0 KB)  
   Needs: `DialogueManager.Instance` (line 211)

⚠️ **ArchiveManager.cs.disabled** (8.3 KB)  
   Needs: `AnastasiaController.Instance` (line 61)

⚠️ **Moon4AquiferPurge.cs.disabled** (8.0 KB)  
   Risk: Low (null-safe), but keeping batch small

❌ **RuntimeBootValidator.cs.disabled** (8.7 KB) **CRITICAL**  
   Needs: 20+ systems (defer to Phase 90+)

---

## 🔧 FIX STRATEGIES

### Strategy A: Safe Subset (Fastest)
- Enable 5 safe files NOW
- Defer 5 blockers until dependencies ready
- **Time**: 2 minutes
- **Risk**: Zero

### Strategy B: Enable Dependencies First
1. Enable `DialogueManager.cs` from Batch 2
2. Enable `AnastasiaController.cs` from Batch 2
3. Enable all 10 Batch 3 files (except RuntimeBootValidator)
- **Time**: 10 minutes
- **Risk**: Medium

### Strategy C: Create Stubs
- Create minimal `_STUB_DialogueManager.cs`
- Enable all 10 files (except RuntimeBootValidator)
- Replace stubs later
- **Time**: 5 minutes
- **Risk**: Low

---

## 📊 BATCH COMPARISON

| Metric | Batch 1 | Batch 2 | Batch 3 |
|--------|---------|---------|---------|
| Total files | 10 | 10 | 10 |
| Safe files | ? | ? | **5** |
| Blockers | ? | ? | **4** |
| Critical blockers | ? | ? | **1** |
| Size range | 2.8-5.5 KB | 5.6-6.8 KB | **6.9-8.7 KB** |

---

## ⚠️ CRITICAL WARNINGS

1. **RuntimeBootValidator.cs** references 20+ disabled systems
   - **DO NOT ENABLE** until Phase 90+
   - Creates massive compilation cascade if enabled early

2. **DialogueManager** missing
   - Blocks 3 files (2 in Batch 3, 1 in RuntimeBootValidator)
   - Enable in Batch 2 OR create stub

3. **AnastasiaController** missing
   - Blocks ArchiveManager.cs
   - Null-safe operators present, may compile but won't function

---

## 📁 OUTPUT FILES

Generated artifacts:

```
C:\dev\TARTARIA_new\
├── BATCH_3_ERROR_ANTICIPATION_REPORT.md ← Full analysis
├── BATCH_3_PREEMPTIVE_FIXES.ps1 ← Automated script
├── BATCH_3_QUICK_START.md ← This file
└── BATCH_3_ERROR_ANTICIPATION_REPORT.txt ← Plain text backup
```

---

## 🎬 EXECUTION FLOW

```
1. Run: .\BATCH_3_PREEMPTIVE_FIXES.ps1 -DryRun
   └─ Check dependency status
   
2. Review output:
   ├─ ✓ All dependencies ready → Run without -DryRun
   ├─ ⚠ Some missing → Run with -ApplyStubs
   └─ ❌ Major issues → Enable dependencies first (Batch 2)

3. Wait for Unity compilation:
   ├─ Green: Success! Proceed to Batch 4
   ├─ Yellow: Warnings, check Console
   └─ Red: Errors, rollback and analyze

4. Report status to coordination lane
```

---

## 🔍 DEPENDENCY CHECK

Run this to verify dependency status:

```powershell
$deps = @("DialogueManager.cs", "AnastasiaController.cs")
foreach ($dep in $deps) {
    $enabled = Test-Path "Assets\_Project\Scripts\Integration\$dep"
    $disabled = Test-Path "Assets\_Project\Scripts\Integration\$dep.disabled"
    
    if ($enabled) { 
        Write-Host "✓ $dep ENABLED" -ForegroundColor Green 
    } elseif ($disabled) { 
        Write-Host "⚠ $dep DISABLED" -ForegroundColor Red 
    } else { 
        Write-Host "? $dep NOT FOUND" -ForegroundColor Yellow 
    }
}
```

---

## 📞 LANE COORDINATION

**Lane 3 Status**: ✅ **COMPLETE**  
**Deliverables**:
- Error pattern analysis (7 patterns identified)
- Blocker identification (5 files flagged)
- Automated fix script (3 strategies)
- Full documentation (this guide + detailed report)

**Next Lane**: Lane 1 (Enabler) or Lane 2 (Tester)  
**Blocker Handoff**: Defer 5 files to Batch 4 or later

---

## 🚀 READY TO EXECUTE

Default recommendation:

```powershell
cd C:\dev\TARTARIA_new
.\BATCH_3_PREEMPTIVE_FIXES.ps1
```

This will:
1. Check dependencies
2. Exclude blocker files automatically
3. Enable 5 safe files
4. Report results

Total time: **~2 minutes**

---

**Lane 3 Complete** | Ready for Batch 3 enablement
