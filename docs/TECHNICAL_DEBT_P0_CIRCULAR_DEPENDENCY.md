# TECHNICAL DEBT: Circular Assembly Dependency (P0)

**Status:** 🔴 BLOCKING — Prevents compilation  
**Severity:** P0 Critical  
**Estimated Fix Time:** 4-6 hours (dedicated refactor sprint)  
**Discovered:** 2026-05-22 (commit b9a08ce)  

---

## Problem Statement

Unity reports "One or more cyclic dependencies detected between assemblies" involving 10 .asmdef files:
- `Assembly-CSharp-Editor`
- `Assembly-CSharp` 
- `Tartaria.AI`
- `Tartaria.Data` ✅ **ROOT CAUSE**
- `Tartaria.Editor`
- `Tartaria.Gameplay` ✅ **ROOT CAUSE**
- `Tartaria.Integration`
- `Tartaria.Tests.EditMode`
- `Tartaria.Tests.PlayMode`
- `Tartaria.UI`

**Primary Cycle:** `Tartaria.Data` ↔ `Tartaria.Gameplay`
- **Forward:** Data references Gameplay (for enum definitions: `SkillId`, `SkillTreeType`, `SkillModifierType`, `StationType`)
- **Backward:** Gameplay references Data (to load `ScriptableObject` assets)

---

## Root Cause Analysis

### Enums in Wrong Assembly
**Problem:** Data layer ScriptableObjects (`SkillNodeData`, `SkillTreeAsset`, `CraftingRecipeData`) reference enum types defined in Gameplay (`SkillTreeSystem.cs` lines 287-370, `CraftingRecipeData.cs` for `StationType`).

**Why It's Wrong:** Data should be downstream of Core, not dependent on Gameplay. Enums should live in Core or Data, not Gameplay.

### Validation Infrastructure Split
**Problem:** `IValidatable`, `ValidationResult`, `DataValidator` live in `Data.Validation` namespace but:
- `QuestDefinition` (Data asset) lives in Core assembly
- Core.asmdef doesn't reference Data.asmdef
- Causes CS0234 "The type or namespace name 'Data' does not exist in the namespace 'Tartaria'"

### Quest System Entanglement
**Problem:** `IQuestProvider` interface lives in Core but references:
- `QuestDefinition` (Data type)
- `QuestStatus`, `QuestState` (undefined, need to be in Data or Core)

### Save Manager Issues
**Hidden Problem (Exposed During Fix Attempt):** `SaveManager.cs` has 18 syntax errors (tuple issues, missing parens) that only appear when breaking the circular dependency. These were masked by transitive dependencies.

### Duplicate Enum Definitions
**Problem:** When enums are extracted to break cycles, duplicates appear:
- `StatType` defined in both `StatusEffectData.cs` and (location TBD)
- `ItemCategory`, `ItemRarity` defined in both `ItemData.cs` and (location TBD)
- `EquipSlot` defined in both `EquipmentItemData.cs` and (location TBD)

---

## Failed Fix Attempts (2026-05-22 Session)

### Attempt 1: Extract Enums to Core
**Actions:**
- Created `SkillEnums.cs` in Core with `SkillId`, `SkillTreeType`, `SkillModifierType`
- Updated 4 Data files to use Core namespace
- Removed `Tartaria.Gameplay` from Data.asmdef references

**Result:** ❌ Exposed 40+ cascading errors:
- `SaveManager.cs`: 18 syntax errors (tuple parsing, missing parens)
- `DialogueNodeData.cs`: Missing `using Tartaria.Localization`
- `IQuestProvider`: Missing `QuestStatus`/`QuestState` types
- `ObjectiveData`, `StatusEffectData`: Missing `ISerializationCallbackReceiver` implementations
- 4 duplicate enum definitions

### Attempt 2: Move Files Between Assemblies
**Actions:**
- Moved `InventoryTransaction.cs` Core → Gameplay
- Moved `IValidatable.cs`, `ValidationResult.cs`, `DataValidator.cs` Data.Validation → Core
- Moved `QuestDefinition.cs` Core → Data
- Moved `IQuestProvider.cs` Core → Data

**Result:** ❌ Same 40+ errors, namespace issues multiplied

**Time Spent:** 2.5 hours (within this session)  
**Conclusion:** Problem runs deeper than anticipated. Requires full architectural refactor, not piecemeal fixes.

---

## Correct Fix Strategy (4-6h Sprint)

### Phase 1: Centralize Enums (1h)
1. Create `Tartaria.Core.Enums` namespace
2. Move ALL enums to Core:
   - `SkillId`, `SkillTreeType`, `SkillModifierType` (from Gameplay)
   - `StationType` (from CraftingRecipeData)
   - `StatType`, `ItemCategory`, `ItemRarity`, `EquipSlot` (consolidate duplicates)
   - `QuestStatus`, `QuestState` (define if missing)
3. Update all references across Data/Gameplay

### Phase 2: Fix Validation Infrastructure (1h)
1. Keep `IValidatable`, `ValidationResult`, `DataValidator` in Core
2. Move `QuestDefinition` to Data assembly (it's a ScriptableObject)
3. Add `Tartaria.Core` reference to Data.asmdef (legal: Core has no dependencies)
4. Update all `using` statements

### Phase 3: Fix SaveManager Syntax Errors (2h)
1. Fix 18 tuple parsing errors (lines 328, 356, 401, 763-770, 1097, 1109, 1723-1724)
2. Add missing parens/semicolons
3. Fix top-level statement errors
4. Test save/load pipeline

### Phase 4: Complete Missing Implementations (1h)
1. `ObjectiveData`: Add `OnBeforeSerialize()` implementation
2. `StatusEffectData`: Add `OnBeforeSerialize()` implementation  
3. `DialogueNodeData`: Add `using Tartaria.Localization`
4. Define `QuestStatus`/`QuestState` types in appropriate assembly

### Phase 5: Validation & Testing (1h)
1. Verify `.\tartaria-play.ps1 -BatchOnly` passes (exit 0)
2. Run Editor play test (no runtime exceptions)
3. Grep for any remaining `using Tartaria.Gameplay` in Data files
4. Commit with message: `ARCH: Break Data↔Gameplay circular dependency (4-6h refactor)`

---

## Why This Wasn't Done Today

1. **Time Budget:** User allocated 2h for ".asmdef fixes", actual fix is 4-6h
2. **Scope Creep:** Breaking one cycle exposed 6 additional architectural issues
3. **Risk:** SaveManager syntax errors indicate brittle codebase, high regression risk
4. **Priority:** 6 other P0 fixes already complete (75% done), better ROI on remaining work

---

## Workaround (Current State)

**Accept circular dependency as technical debt until dedicated refactor sprint.**

Alternative path:
1. ✅ Continue with memory leak cleanup (doesn't require build)
2. ✅ Continue with GameBalanceConfig adoption (search/replace work)
3. ⏸️ Defer circular dependency fix to Phase 3
4. ⏸️ Defer dialogue system migration to Phase 4 (separate 80h sprint)

---

## Files Involved in Fix

### Must Edit (16 files):
- `Assets/_Project/Scripts/Core/QuestDefinition.cs`
- `Assets/_Project/Scripts/Core/IQuestProvider.cs`
- `Assets/_Project/Scripts/Data/Tartaria.Data.asmdef`
- `Assets/_Project/Scripts/Data/SkillNodeData.cs`
- `Assets/_Project/Scripts/Data/SkillTreeAsset.cs`
- `Assets/_Project/Scripts/Data/Query/SkillRegistry.cs`
- `Assets/_Project/Scripts/Data/CraftingRecipeData.cs`
- `Assets/_Project/Scripts/Data/ItemData.cs`
- `Assets/_Project/Scripts/Data/EquipmentItemData.cs`
- `Assets/_Project/Scripts/Data/StatusEffectData.cs`
- `Assets/_Project/Scripts/Data/DialogueNodeData.cs`
- `Assets/_Project/Scripts/Data/ObjectiveData.cs`
- `Assets/_Project/Scripts/Gameplay/SkillTreeSystem.cs`
- `Assets/_Project/Scripts/Gameplay/InventorySystem.cs`
- `Assets/_Project/Scripts/Gameplay/CraftingSystem.cs`
- `Assets/_Project/Scripts/Save/SaveManager.cs` ⚠️ **HIGH RISK**

### Must Create (1 file):
- `Assets/_Project/Scripts/Core/SkillEnums.cs`

### Must Move (3 files):
- `Assets/_Project/Scripts/Data/Validation/*` → `Assets/_Project/Scripts/Core/`

---

## Acceptance Criteria

✅ **Build GREEN:** `.\tartaria-play.ps1 -BatchOnly` exits with code 0  
✅ **No Cycles:** Unity console shows zero "cyclic dependency" warnings  
✅ **Runtime Clean:** No `NullReferenceException`, no missing references  
✅ **Data Isolation:** Data.asmdef only references Core (not Gameplay, not Integration)  
✅ **All Tests Pass:** 991 existing tests still pass  

---

## References

- Original Error: `One or more cyclic dependencies detected between assemblies`
- Session: 2026-05-22 "10 AGENT SWARM FIX" → "15 AGENT SWARM HAMMER"
- Commit: b9a08ce "AGENT 5: Add Executive Summary for stakeholders"
- Master Audit: `MASTER_RPG_ARCHITECTURE_AUDIT.md` (P0-8: Circular Dependency)
- Unity Docs: [Assembly Definition Files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

---

**Next Action:** Dedicated 4-6h refactor sprint with senior engineer (not autonomous agents — too risky for SaveManager fixes).
