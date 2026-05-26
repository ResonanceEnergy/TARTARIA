# Core/Data Architecture Fix — QuestDefinition Duplication

**Created:** 2026-05-25  
**Status:** DOCUMENTED — Awaiting execution approval  
**Blocks:** Task 4 (Manager refactor), Phase 66+ (Integration activation)

---

## Problem — Duplicate Type Definitions

### Current State: TWO QuestDefinition Classes

**Tartaria.Core.QuestDefinition** (KEEP THIS)
- **Location:** `Assets\_Project\Scripts\Core\QuestDefinition.cs`
- **Status:** Active, 150+ lines
- **Dependencies:** Core.Validation, Localization (no upward refs)
- **Usage:** Should be THE canonical quest data type

**Tartaria.Data.QuestDefinition** (DELETE THIS)
- **Location:** `Assets\_Project\Scripts\Data\QuestDefinition.cs`
- **Status:** Active, ~150 lines (DUPLICATE)
- **Dependencies:** Core, Core.Validation, Core.Enums, Localization
- **Usage:** Duplicate causing CS0104 ambiguous reference errors

**Impact:** When Integration/QuestManager.cs references `QuestDefinition`, compiler sees BOTH types and throws CS0104 errors. Blocks Manager activation (Phase 66 failed).

---

## Root Cause — Architectural Violation

**Correct Assembly Layering:**
```
Core (base)
  ↓
Data (extends Core types with ScriptableObject containers)
  ↓
Integration (uses Data assets + Core interfaces)
  ↓
UI (displays Integration state via Core interfaces)
```

**Current Violation:**
- QuestDefinition exists in BOTH Core and Data
- Creates ambiguity in downstream assemblies (Integration, UI)
- Core/IQuestProvider.cs.disabled exists alongside Data/IQuestProvider.cs (active)

---

## Additional Duplications Found

### IQuestProvider Interface (2 versions)

**Tartaria.Core.IQuestProvider** (DISABLED)
- **Location:** `Assets\_Project\Scripts\Core\IQuestProvider.cs.disabled`
- **Status:** Disabled
- **Method:** `QuestDefinition GetQuestDefinition(string questId);` (returns Core.QuestDefinition)
- **Locator:** `QuestProviderLocator.Current`

**Tartaria.Data.IQuestProvider** (ACTIVE)
- **Location:** `Assets\_Project\Scripts\Data\IQuestProvider.cs`
- **Status:** Active
- **Method:** `QuestDefinition GetQuestDefinition(string questId);` (returns ambiguous QuestDefinition)
- **Locator:** `QuestProviderLocator.Current` (duplicate static class!)
- **Event:** `event Action<string, Core.Enums.QuestStatus> OnQuestStatusChanged;` (references Core.Enums)

**Impact:** Data version already references Core types (`Core.Enums.QuestStatus`), but duplicates QuestDefinition instead of using Core's version.

---

## Fix Strategy — Canonical Core Types

### Decision: Core Owns Interfaces + Core Data Types

**Rationale:**
- Core is the base layer — NO upward dependencies allowed
- Data should ONLY extend Core types, not duplicate them
- IQuestProvider interface defines contracts for downstream assemblies (UI, Gameplay)
- QuestDefinition is a core data type (like ItemDefinition, BuildingDefinition)

### Phase A: Delete Data Duplicates

**Files to DELETE:**
1. `Assets\_Project\Scripts\Data\QuestDefinition.cs` (duplicate)

**Files Already Disabled (Keep Disabled):**
1. `Assets\_Project\Scripts\Core\IQuestProvider.cs.disabled` (Core version deprecated in favor of Data version, but Data version needs refactor)

### Phase B: Update Data References

**File:** `Assets\_Project\Scripts\Data\QuestData.cs`
- **Current:** `public class QuestData : QuestDefinition`
- **Fix:** `public class QuestData : Tartaria.Core.QuestDefinition`
- **Add:** `using QuestDef = Tartaria.Core.QuestDefinition;` (type alias for clarity)

**File:** `Assets\_Project\Scripts\Data\IQuestProvider.cs`
- **Current:** `QuestDefinition GetQuestDefinition(string questId);`
- **Fix:** `Core.QuestDefinition GetQuestDefinition(string questId);`
- **OR:** Add `using QuestDef = Tartaria.Core.QuestDefinition;` at top, use `QuestDef` in signature

### Phase C: Update Integration References (AFTER DATA FIXES)

**File:** `Assets\_Project\Scripts\Integration\QuestManager.cs.disabled`
- **Current:** `using Tartaria.Data;` + bare `QuestDefinition` usage (causes CS0104)
- **Fix:** Remove `using Tartaria.Data;`, add `using Tartaria.Core;`, use `Core.QuestDefinition` explicitly
- **OR:** Keep both `using` statements, fully qualify: `var def = Core.QuestDefinition.Load(...);`

**Files to Check (grep for QuestDefinition in Integration):**
- All 139 disabled Integration files may reference QuestDefinition
- Run after Phase B: `grep -r "QuestDefinition" Assets\_Project\Scripts\Integration\`

### Phase D: Verify No Regressions

**Steps:**
1. Delete Data/QuestDefinition.cs
2. Update Data/QuestData.cs and Data/IQuestProvider.cs
3. Compile Data assembly (batch mode Unity)
4. Update Integration/QuestManager.cs.disabled references
5. Attempt Phase 66 retry (enable QuestManager)
6. Run full build pipeline: `.\tartaria-play.ps1 -BatchOnly`

---

## Assembly Reference Audit

### Core Assembly
**Depends On:** UnityEngine, Localization  
**Referenced By:** Data, AI, Audio, Camera, Gameplay, Input, Integration, Save, UI, World  
**Contains:** Interfaces (IQuestProvider, IQuestService), Core types (QuestDefinition, QuestState, QuestStatus enum), ServiceLocator, GameStateManager

**No Upward References Allowed** — Core cannot reference Data, Integration, UI

### Data Assembly
**Depends On:** Core, Localization  
**Referenced By:** Integration, UI, Editor  
**Contains:** ScriptableObject containers (QuestData, ItemData, BuildingData), IQuestProvider (ACTIVE version), QuestProviderLocator

**Current Violation:** Duplicates QuestDefinition instead of using Core.QuestDefinition

### Integration Assembly
**Depends On:** Core, Data, Audio, Gameplay, Save, Camera  
**Referenced By:** UI, Editor  
**Contains:** Manager singletons (QuestManager, CompanionManager), Game loop logic, Moon progression systems

**Current Blocker:** CS0104 ambiguous QuestDefinition reference (Core vs Data)

---

## Risk Assessment

### LOW RISK: Data Assembly Changes (Phase B)
- Only 2 files affected: QuestData.cs, IQuestProvider.cs
- Both already use `Tartaria.Core` namespace in imports
- Simple type alias fix: `using QuestDef = Tartaria.Core.QuestDefinition;`
- Data assembly compiles independently — can validate before touching Integration

### MEDIUM RISK: Integration Assembly Changes (Phase C)
- 139 disabled files, unknown how many reference QuestDefinition
- QuestManager.cs.disabled is 505+ lines with complex logic
- Compilation errors already exist from Phase 66 (CS0104, CS0738, CS0111)
- Need systematic grep + replace strategy

### HIGH RISK: Runtime Integration Breaks
- QuestDefinition is serialized in ScriptableObject assets (questdb.asset files)
- Changing namespace from `Tartaria.Data.QuestDefinition` → `Tartaria.Core.QuestDefinition` may break asset references
- Unity may show "Missing Script" errors in Inspector
- **Mitigation:** Use Unity's `[MovedFrom]` attribute to preserve serialization

---

## Serialization Safety — MovedFrom Attribute

Unity's `[MovedFrom]` attribute preserves ScriptableObject references when moving/renaming classes.

**Add to Core/QuestDefinition.cs:**
```csharp
using UnityEngine.Scripting.APIUpdating;

namespace Tartaria.Core
{
    [MovedFrom(false, "Tartaria.Data", "Tartaria.Data", "QuestDefinition")]
    [CreateAssetMenu(menuName = "Tartaria/Quest Definition")]
    public class QuestDefinition : ScriptableObject, IValidatable, ILocalizable
    {
        // ... existing fields
    }
}
```

**Parameters:**
- `false` — Class was NOT moved to a different assembly (still in same DLL)
- `"Tartaria.Data"` — Old namespace
- `"Tartaria.Data"` — Old assembly name (Tartaria.Data.dll)
- `"QuestDefinition"` — Old class name

**Effect:** Unity auto-updates all `.asset` files referencing `Tartaria.Data.QuestDefinition` to `Tartaria.Core.QuestDefinition` on next domain reload.

---

## Execution Plan — Step-by-Step

### Step 1: Backup Current State
```powershell
git add -A
git commit -m "Pre-QuestDefinition-deduplication snapshot"
```

### Step 2: Delete Data Duplicate
```powershell
Remove-Item "Assets\_Project\Scripts\Data\QuestDefinition.cs" -Force
Remove-Item "Assets\_Project\Scripts\Data\QuestDefinition.cs.meta" -Force
```

### Step 3: Update Data/QuestData.cs
**Before:**
```csharp
using Tartaria.Core;
using Tartaria.Core.Validation;
using Tartaria.Core.Enums;

namespace Tartaria.Data
{
    public class QuestData : QuestDefinition, ISerializationCallbackReceiver
    {
        // ...
    }
}
```

**After:**
```csharp
using Tartaria.Core;
using Tartaria.Core.Validation;
using Tartaria.Core.Enums;
using QuestDef = Tartaria.Core.QuestDefinition;

namespace Tartaria.Data
{
    public class QuestData : QuestDef, ISerializationCallbackReceiver
    {
        // ...
    }
}
```

### Step 4: Update Data/IQuestProvider.cs
**Before:**
```csharp
QuestDefinition GetQuestDefinition(string questId);
```

**After:**
```csharp
Core.QuestDefinition GetQuestDefinition(string questId);
```

### Step 5: Compile Data Assembly
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe" `
  -projectPath "$PWD" `
  -logFile "Logs\compile-data-asm.log" `
  -batchmode -quit
```

**Expected:** 0 errors, Data.dll compiles successfully

### Step 6: Update Integration/QuestManager.cs.disabled
```csharp
// ADD at top
using QuestDef = Tartaria.Core.QuestDefinition;

// REPLACE all bare "QuestDefinition" with "QuestDef"
// Example:
// Before: QuestDefinition def = _questDatabase.GetQuest(questId);
// After:  QuestDef def = _questDatabase.GetQuest(questId);
```

### Step 7: Retry Phase 66 (Enable QuestManager)
```powershell
Move-Item "Assets\_Project\Scripts\Integration\QuestManager.cs.disabled" `
          "Assets\_Project\Scripts\Integration\QuestManager.cs" -Force
.\tartaria-play.ps1 -BatchOnly
```

**Expected:** CS0104 errors RESOLVED, new errors (CS0738, CS0111) surface for separate fix

### Step 8: Verify Assets Load
1. Open Unity Editor GUI
2. Navigate to QuestDatabase asset in Project window
3. Verify "Script" field shows `QuestDefinition (Tartaria.Core)` (NOT "Missing")
4. Check Inspector for all quest assets — no "Missing Script" warnings

---

## Success Criteria

✅ **Data Assembly Compiles** — 0 CS errors after Phase B  
✅ **CS0104 Resolved** — No ambiguous QuestDefinition references  
✅ **Assets Intact** — All .asset files load without "Missing Script" errors  
✅ **Integration Compiles** — QuestManager.cs activates without CS0104  
✅ **Build GREEN** — `.\tartaria-play.ps1 -BatchOnly` exits 0  

---

## Rollback Plan

If execution fails:
```powershell
git reset --hard HEAD~1
git clean -fd
```

Restore from pre-deduplication commit. Re-disable QuestManager.cs if partially activated.

---

## Next Steps After Resolution

1. **Enable IQuestProvider in Core** — Move Core/IQuestProvider.cs.disabled → active, delete Data duplicate
2. **Audit Other Duplications** — Check for ItemDefinition, BuildingDefinition, CompanionData duplicates
3. **Harmonize ServiceLocator + ServiceRegistry** — Decide: extend existing ServiceLocator properties OR migrate to ServiceRegistry Dictionary
4. **Resume Task 4** — Manager refactor with clean Core/Data boundaries

---

**Generated by Dr. Vex Aurelian, 2026-05-25**  
**TARTARIA — Unity 6000.3.6f1, Phase 68 (162 files active)**
