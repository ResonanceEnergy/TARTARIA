# ═══════════════════════════════════════════════════════════════
# BATCH 3 PREEMPTIVE FIXES
# Generated: 2026-05-25
# Run BEFORE enabling Batch 3 files to prevent compilation failures
# ═══════════════════════════════════════════════════════════════

param(
    [switch]$DryRun,
    [switch]$ApplyStubs
)

cd C:\dev\TARTARIA_new

Write-Host "`n═══ BATCH 3 PREEMPTIVE FIX SCRIPT ═══`n" -ForegroundColor Cyan

# ─────────────────────────────────────────────────────────────
# STRATEGY 1: CHECK DEPENDENCIES
# ─────────────────────────────────────────────────────────────

Write-Host "Step 1: Checking critical dependencies..." -ForegroundColor Yellow

$criticalDeps = @(
    "DialogueManager.cs",
    "AnastasiaController.cs",
    "QuestManager.cs",
    "TutorialSystem.cs"
)

$missingDeps = @()
foreach ($dep in $criticalDeps) {
    $enabled = Test-Path "Assets\_Project\Scripts\Integration\$dep"
    $disabled = Test-Path "Assets\_Project\Scripts\Integration\$dep.disabled"

    if ($enabled) {
        Write-Host "  ✓ $dep (enabled)" -ForegroundColor Green
    } elseif ($disabled) {
        Write-Host "  ⚠ $dep (STILL DISABLED)" -ForegroundColor Red
        $missingDeps += $dep
    } else {
        Write-Host "  ? $dep (not found)" -ForegroundColor Magenta
    }
}

if ($missingDeps.Count -gt 0) {
    Write-Host "`n❌ CRITICAL: Missing dependencies detected!" -ForegroundColor Red
    Write-Host "   The following types must be enabled BEFORE Batch 3:" -ForegroundColor Yellow
    $missingDeps | ForEach-Object { Write-Host "     • $_" -ForegroundColor Yellow }
    Write-Host "`n   RECOMMENDATION: Enable these files in Batch 2, then retry Batch 3.`n" -ForegroundColor Cyan

    if (!$ApplyStubs) {
        Write-Host "   Run with -ApplyStubs to create minimal stub versions.`n" -ForegroundColor Gray
        exit 1
    }
}

# ─────────────────────────────────────────────────────────────
# STRATEGY 2: EXCLUDE BLOCKER FILES
# ─────────────────────────────────────────────────────────────

Write-Host "`nStep 2: Identifying blocker files to exclude..." -ForegroundColor Yellow

$excludeFiles = @(
    "RuntimeBootValidator.cs.disabled"  # 20+ dependencies, defer to Phase 90+
)

$conditionalExclude = @()
if ($missingDeps -contains "DialogueManager.cs") {
    $conditionalExclude += "MoonCompanionSpawner.cs.disabled"
    $conditionalExclude += "Moon5Components.cs.disabled"
}
if ($missingDeps -contains "AnastasiaController.cs") {
    $conditionalExclude += "ArchiveManager.cs.disabled"
}

Write-Host "`n  ALWAYS EXCLUDE (critical blockers):" -ForegroundColor Red
$excludeFiles | ForEach-Object { Write-Host "    • $_" -ForegroundColor Red }

if ($conditionalExclude.Count -gt 0) {
    Write-Host "`n  CONDITIONAL EXCLUDE (missing dependencies):" -ForegroundColor Yellow
    $conditionalExclude | ForEach-Object { Write-Host "    • $_" -ForegroundColor Yellow }
}

# ─────────────────────────────────────────────────────────────
# STRATEGY 3: SAFE BATCH 3 FILE LIST
# ─────────────────────────────────────────────────────────────

Write-Host "`nStep 3: Generating safe Batch 3 enable list..." -ForegroundColor Yellow

$allBatch3 = @(
    "MoonProgressTracker.cs.disabled",
    "EchohavenProgressionSystem.cs.disabled",
    "Moon5AmplificationField.cs.disabled",
    "DebugOverlay.cs.disabled",
    "Moon4AquiferPurge.cs.disabled",
    "MoonCompanionSpawner.cs.disabled",
    "Moon5Components.cs.disabled",
    "ArchiveManager.cs.disabled",
    "MoonPortalSelector.cs.disabled",
    "RuntimeBootValidator.cs.disabled"
)

$safeFiles = $allBatch3 | Where-Object {
    $excludeFiles -notcontains $_ -and $conditionalExclude -notcontains $_
}

Write-Host "`n  SAFE TO ENABLE ($($safeFiles.Count)/$($allBatch3.Count)):" -ForegroundColor Green
$safeFiles | ForEach-Object { Write-Host "    ✓ $_" -ForegroundColor Green }

# ─────────────────────────────────────────────────────────────
# STRATEGY 4: APPLY FIXES (if not dry run)
# ─────────────────────────────────────────────────────────────

if ($DryRun) {
    Write-Host "`n[DRY RUN] No changes applied. Remove -DryRun to execute.`n" -ForegroundColor Gray
    exit 0
}

Write-Host "`nStep 4: Enabling safe Batch 3 files..." -ForegroundColor Yellow

$enabled = 0
$skipped = 0
foreach ($file in $safeFiles) {
    $src = "Assets\_Project\Scripts\Integration\$file"
    $dst = $src -replace '\.disabled$', ''

    if (Test-Path $src) {
        Move-Item $src $dst -Force
        Write-Host "  ✓ Enabled: $file" -ForegroundColor Green
        $enabled++
    } else {
        Write-Host "  ? Not found: $file" -ForegroundColor Gray
        $skipped++
    }
}

Write-Host "`n═══ BATCH 3 PREEMPTIVE FIX COMPLETE ═══" -ForegroundColor Cyan
Write-Host "  Enabled: $enabled files" -ForegroundColor Green
Write-Host "  Skipped: $skipped files" -ForegroundColor Gray
Write-Host "  Excluded: $($excludeFiles.Count + $conditionalExclude.Count) files (deferred)`n" -ForegroundColor Yellow

if ($excludeFiles.Count -gt 0 -or $conditionalExclude.Count -gt 0) {
    Write-Host "⚠ DEFERRED FILES (enable in later batches):" -ForegroundColor Yellow
    ($excludeFiles + $conditionalExclude) | ForEach-Object {
        Write-Host "    • $_" -ForegroundColor Yellow
    }
    Write-Host ""
}

Write-Host "Next: Wait for Unity compilation, check Console for errors.`n" -ForegroundColor Cyan

# ─────────────────────────────────────────────────────────────
# STRATEGY 5: CREATE STUB IF REQUESTED
# ─────────────────────────────────────────────────────────────

if ($ApplyStubs -and $missingDeps.Count -gt 0) {
    Write-Host "Step 5: Creating minimal stubs for missing dependencies...`n" -ForegroundColor Magenta

    if ($missingDeps -contains "DialogueManager.cs") {
        $stubContent = @"
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>MINIMAL STUB for phased activation. Replace with full implementation later.</summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        public bool IsPlaying => false;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void PlayContextDialogue(string context) {
            Debug.Log(`$"[DialogueManager STUB] PlayContextDialogue: {context}");
        }
    }
}
"@
        $stubPath = "Assets\_Project\Scripts\Integration\_STUB_DialogueManager.cs"
        $stubContent | Out-File -FilePath $stubPath -Encoding UTF8
        Write-Host "  ✓ Created stub: _STUB_DialogueManager.cs" -ForegroundColor Green
    }

    Write-Host "`n⚠ Stubs created. Remember to replace with full implementations later!`n" -ForegroundColor Yellow
}
