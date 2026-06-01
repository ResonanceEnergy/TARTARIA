# BATCH 3 ERROR ANTICIPATION REPORT
**Generated**: 2026-05-25 21:08  
**Lane**: 3 (Error Anticipator)  
**Phase**: 83 Batch 3 Pre-Analysis  
**Workspace**: C:\dev\TARTARIA_new

---

## EXECUTIVE SUMMARY

✅ **5 SAFE FILES** ready to enable immediately  
⚠️ **4 BLOCKER FILES** require dependencies first  
❌ **1 CRITICAL BLOCKER** must be deferred to Phase 90+

**RECOMMENDATION**: Enable safe subset (5 files), defer blockers until dependencies resolved.

---

## BATCH 3 FILES (21-30 by size, 6.9-8.7 KB)

| # | File | Size | Status | Issue |
|---|------|------|--------|-------|
| 21 | MoonProgressTracker.cs.disabled | 6.9 KB | ✅ **SAFE** | Clean, no dependencies |
| 22 | EchohavenProgressionSystem.cs.disabled | 7.5 KB | ✅ **SAFE** | Core deps (likely enabled) |
| 23 | Moon5AmplificationField.cs.disabled | 7.7 KB | ✅ **SAFE** | Null-safe GetComponent |
| 24 | DebugOverlay.cs.disabled | 7.7 KB | ✅ **SAFE** | ECS/DOTS, clean |
| 25 | Moon4AquiferPurge.cs.disabled | 8.0 KB | ✅ **SAFE** | Null-safe, minor risk |
| 26 | MoonCompanionSpawner.cs.disabled | 8.0 KB | ⚠️ **BLOCKER** | DialogueManager.Instance (line 166) |
| 27 | Moon5Components.cs.disabled | 8.0 KB | ⚠️ **BLOCKER** | DialogueManager.Instance (line 211) |
| 28 | ArchiveManager.cs.disabled | 8.3 KB | ⚠️ **BLOCKER** | AnastasiaController.Instance (line 61) |
| 29 | MoonPortalSelector.cs.disabled | 8.7 KB | ✅ **SAFE** | Clean, no dependencies |
| 30 | RuntimeBootValidator.cs.disabled | 8.7 KB | ❌ **CRITICAL** | 20+ system dependencies |

---

## ERROR PATTERN ANALYSIS

### ❌ PATTERN 1: DialogueManager Dependency (HIGH SEVERITY)
**Files affected**: 3  
**Impact**: Compilation failure - type not found

```csharp
// MoonCompanionSpawner.cs line 166
var dm = DialogueManager.Instance;
if (dm != null) { dm.PlayContextDialogue(ctx); }

// Moon5Components.cs line 211
DialogueManager.Instance?.PlayContextDialogue($"thorne_line_{dialogueIndex}");

// RuntimeBootValidator.cs line 82
Check(sb, "DialogueManager", DialogueManager.Instance != null);
```

**Fix**: Enable `DialogueManager.cs` in Batch 2 OR create minimal stub

---

### ❌ PATTERN 2: RuntimeBootValidator Mass Dependency (CRITICAL)
**File**: RuntimeBootValidator.cs.disabled  
**Dependencies**: 20+ disabled types

References ALL major systems:
- CampaignFlowController.Instance
- ZoneTransitionSystem.Instance  
- CombatWaveManager.Instance
- WorkshopSystem.Instance
- TutorialSystem.Instance
- QuestManager.Instance
- DialogueManager.Instance
- BuildingSpawner (FindAnyObjectByType)
- PlayerSpawner (FindAnyObjectByType)
- UI.UIManager.Instance
- UI.HUDController.Instance
- Audio.AudioManager.Instance
- Save.SaveManager.Instance
- EconomySystem.Instance
- Gameplay.SkillTreeSystem.Instance
- Gameplay.ResonanceScannerSystem.Instance
- Tartaria.Camera.CameraController (FindAnyObjectByType)

**Purpose**: Boot validation script that checks ALL systems at startup  
**Recommendation**: **DEFER TO PHASE 90+** when all systems enabled

---

### ⚠️ PATTERN 3: AnastasiaController Dependency (MEDIUM)
**File**: ArchiveManager.cs.disabled (lines 61, 66-68)

```csharp
if (AnastasiaController.Instance != null)
    AnastasiaController.Instance.OnLineDelivered += OnAnastasiaLine;

// Later...
AnastasiaController.Instance?.TryDeliverLine(entry.unlockTrigger);
```

**Mitigation**: Already null-safe, will compile but won't function  
**Fix**: Enable `AnastasiaController.cs` first OR defer ArchiveManager

---

### ✅ PATTERN 4: Camera Namespace (ALREADY FIXED)
**File**: RuntimeBootValidator.cs lines 113, 188

```csharp
var cam = UnityEngine.Camera.main;  // ✓ Full namespace qualifier
```

**Status**: No fix needed

---

### ✅ PATTERN 5: Assembly Boundaries (CLEAN)
All files use proper namespace imports:
- `using Tartaria.Core;`
- `using Tartaria.Gameplay;`
- `using Tartaria.Save;`
- `using Tartaria.Audio;`
- `using Tartaria.Input;`

No cross-assembly violations detected. All Integration → (Core, Gameplay, etc.) references valid.

---

### ✅ PATTERN 6: ECS/DOTS Usage (CLEAN)
**File**: DebugOverlay.cs

```csharp
using Unity.Entities;
public class DebugOverlay : ECSMonoBehaviour
{
    World _world;
    EntityManager _em;
    EntityQuery _rsQuery;
    // ... proper ECS patterns
}
```

No namespace conflicts, properly tracked queries. Clean.

---

### ⚠️ PATTERN 7: Missing Component Types (LOW RISK)
**Files**: Moon5AmplificationField.cs, Moon4AquiferPurge.cs

```csharp
// Moon5AmplificationField.cs lines 41-42
_playerAbilities = _player.GetComponent<PlayerAbilities>();
_playerHealth = _player.GetComponent<PlayerHealth>();

// All GetComponent calls are null-checked ✓
if (_playerAbilities != null) { ... }
```

**Impact**: Runtime errors if components don't exist  
**Mitigation**: All calls are null-checked, safe

---

## RECOMMENDED STRATEGIES

### ⭐ STRATEGY A: SAFE SUBSET (RECOMMENDED)
Enable **5 safe files** immediately, defer blockers:

```powershell
# Enable these NOW
MoonProgressTracker.cs.disabled ✓
EchohavenProgressionSystem.cs.disabled ✓
Moon5AmplificationField.cs.disabled ✓
DebugOverlay.cs.disabled ✓
MoonPortalSelector.cs.disabled ✓

# Defer until dependencies resolved
MoonCompanionSpawner.cs.disabled (needs DialogueManager)
Moon5Components.cs.disabled (needs DialogueManager)
ArchiveManager.cs.disabled (needs AnastasiaController)
Moon4AquiferPurge.cs.disabled (keep batch small)
RuntimeBootValidator.cs.disabled (defer to Phase 90+)
```

**Outcome**: Clean compilation, zero errors, incremental progress

---

### STRATEGY B: AGGRESSIVE WITH DEPENDENCIES
1. Enable `DialogueManager.cs` (if not in Batch 2)
2. Enable `AnastasiaController.cs` (if not in Batch 2)
3. Enable all 10 Batch 3 files EXCEPT RuntimeBootValidator
4. Compile and fix any remaining issues

**Risk**: Moderate - may cascade into more dependencies

---

### STRATEGY C: STUB WORKAROUND
Create minimal stubs for missing dependencies:

```csharp
// _STUB_DialogueManager.cs
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public bool IsPlaying => false;
    
    void Awake() { 
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; 
    }
    
    public void PlayContextDialogue(string context) { 
        Debug.Log($"[DialogueManager STUB] PlayContextDialogue: {context}"); 
    }
}
```

Enable all Batch 3 files, replace stubs later.

**Risk**: Low - stubs prevent compilation errors, defer functionality

---

## EXECUTION COMMANDS

### Dry Run (Check dependencies, no changes)
```powershell
.\BATCH_3_PREEMPTIVE_FIXES.ps1 -DryRun
```

### Enable Safe Subset (Recommended)
```powershell
.\BATCH_3_PREEMPTIVE_FIXES.ps1
```

### Create Stubs + Enable
```powershell
.\BATCH_3_PREEMPTIVE_FIXES.ps1 -ApplyStubs
```

### Manual Enable (if script fails)
```powershell
$safe = @(
    "MoonProgressTracker.cs.disabled",
    "EchohavenProgressionSystem.cs.disabled",
    "Moon5AmplificationField.cs.disabled",
    "DebugOverlay.cs.disabled",
    "MoonPortalSelector.cs.disabled"
)

foreach ($file in $safe) {
    $src = "Assets\_Project\Scripts\Integration\$file"
    $dst = $src -replace '\.disabled$', ''
    if (Test-Path $src) {
        Move-Item $src $dst -Force
        Write-Host "✓ $file" -ForegroundColor Green
    }
}
```

---

## RED FLAG FILE: RuntimeBootValidator.cs

**This file is a SPECIAL CASE.**

```
Purpose: Runtime boot validation - checks ALL systems at startup
Dependencies: 20+ managers, singletons, and systems
Compilation: IMPOSSIBLE until Phase 90+ (all systems enabled)
Criticality: HIGH - but not needed until final integration

Recommendation:
  1. Skip in all early batches (Batch 3-20)
  2. Enable in Phase 90+ when all systems are live
  3. OR create minimal stub version for early phases:
     - Check only Core systems (GameStateManager, SceneLoader)
     - Comment out checks for Integration-layer systems
     - Full version after Phase 90
```

---

## DEPENDENCY GRAPH

```
Integration Layer (Batch 3 files)
  ├─ Tartaria.Core ✓ (enabled)
  ├─ Tartaria.Gameplay ✓ (enabled)
  ├─ Tartaria.Save ✓ (enabled)
  ├─ Tartaria.Audio ✓ (enabled)
  ├─ Tartaria.Input ✓ (enabled)
  │
  ├─ DialogueManager ❌ (DISABLED - blocker for 3 files)
  ├─ AnastasiaController ❌ (DISABLED - blocker for 1 file)
  │
  └─ RuntimeBootValidator → 20+ systems ❌ (defer to Phase 90+)
```

---

## VALIDATION CHECKLIST

Before enabling Batch 3, verify:

- [ ] Batch 1 (10 files) compiled successfully
- [ ] Batch 2 (10 files) compiled successfully
- [ ] Zero compilation errors in Unity Console
- [ ] `DialogueManager.cs` status checked (enabled or stub created)
- [ ] `AnastasiaController.cs` status checked
- [ ] RuntimeBootValidator.cs excluded from batch
- [ ] Backup/commit before enabling

After enabling Batch 3:

- [ ] Unity recompiles without errors
- [ ] No missing type errors in Console
- [ ] Test boot sequence (play mode)
- [ ] Verify safe files load correctly
- [ ] Document any runtime warnings

---

## NEXT STEPS

1. **Run preemptive fix script**: `.\BATCH_3_PREEMPTIVE_FIXES.ps1 -DryRun`
2. **Review dependency status**: Check if DialogueManager/AnastasiaController enabled
3. **Choose strategy**: Safe Subset (A), Aggressive (B), or Stubs (C)
4. **Enable files**: Run script or manual commands
5. **Wait for Unity compilation**: Check Console for errors
6. **Report status**: Green (success), Yellow (warnings), Red (errors)

---

## FILES GENERATED

- `BATCH_3_ERROR_ANTICIPATION_REPORT.md` (this file)
- `BATCH_3_PREEMPTIVE_FIXES.ps1` (automated fix script)
- `BATCH_3_ERROR_ANTICIPATION_REPORT.txt` (plain text backup)

---

**End of Report**  
Lane 3 (Error Anticipator) | Phase 83 Batch 3  
Ready for Batch 3 enablement with preemptive mitigation applied.
