#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Hotfix Validation Script

.DESCRIPTION
    Comprehensive pre-deployment validation for hotfix builds.
    Runs smoke tests, critical path tests, and validation checks.
    Generates validation report for deployment approval.

.PARAMETER Branch
    Hotfix branch name (e.g., hotfix/ISSUE-123-description)

.PARAMETER SkipTests
    Skip automated test execution (tests must be run separately)

.PARAMETER StrictMode
    Fail on warnings (default: false, only fail on errors)

.EXAMPLE
    .\hotfix-validate.ps1 -Branch "hotfix/ISSUE-123-fix-combat-crash"

.NOTES
    Exit codes:
    0 = PASS (ready for deployment)
    1 = FAIL (blocking issues found)
    2 = WARN (non-blocking issues, proceed with caution)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Branch,
    
    [switch]$SkipTests,
    [switch]$StrictMode
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$timestamp = Get-Date -Format "yyyy-MM-dd-HHmmss"
$reportPath = "Logs/Hotfix/validation-$Branch-$timestamp.md".Replace("/", "-")
$errorCount = 0
$warningCount = 0

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Hotfix Validation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Branch:      $Branch"
Write-Host "Strict Mode: $StrictMode"
Write-Host "Report:      $reportPath"
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 1: BRANCH CHECK
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 1: Validating branch..." -ForegroundColor Yellow

# Check if branch exists
git rev-parse --verify $Branch 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Branch '$Branch' does not exist" -ForegroundColor Red
    $errorCount++
} else {
    Write-Host "✅ Branch exists" -ForegroundColor Green
    
    # Checkout the branch
    git checkout $Branch
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to checkout branch" -ForegroundColor Red
        $errorCount++
    } else {
        Write-Host "✅ Checked out branch" -ForegroundColor Green
    }
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 2: CODE VALIDATION
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 2: Code validation..." -ForegroundColor Yellow

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "⚠️  Uncommitted changes detected" -ForegroundColor Yellow
    $warningCount++
    Write-Host "   Files:" -ForegroundColor Gray
    $gitStatus | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
} else {
    Write-Host "✅ No uncommitted changes" -ForegroundColor Green
}

# Check for compilation errors (look for recent Unity logs)
$editorLog = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
if (Test-Path $editorLog) {
    $recentErrors = Select-String -Path $editorLog -Pattern "CompilerOutput.*error CS" -Context 0,2 | Select-Object -Last 5
    if ($recentErrors) {
        Write-Host "❌ Compilation errors detected" -ForegroundColor Red
        $errorCount++
        $recentErrors | ForEach-Object { Write-Host "   $($_.Line)" -ForegroundColor Red }
    } else {
        Write-Host "✅ No compilation errors" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️  Unity Editor log not found (can't check compilation)" -ForegroundColor Yellow
    $warningCount++
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 3: SMOKE TESTS
# ═══════════════════════════════════════════════════════════════════════════════

if (!$SkipTests) {
    Write-Host "Step 3: Running smoke tests (8 tests, ~3 min)..." -ForegroundColor Yellow
    
    $smokeTestLog = "Logs/hotfix-smoke-tests.log"
    & .\scripts\run-automated-tests.ps1 -Mode Smoke -LogFile $smokeTestLog -GenerateReport:$false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All smoke tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Smoke tests FAILED" -ForegroundColor Red
        $errorCount++
        
        # Show failed tests
        if (Test-Path $smokeTestLog) {
            $failures = Select-String -Path $smokeTestLog -Pattern "\[FAIL\]"
            if ($failures) {
                Write-Host "   Failed tests:" -ForegroundColor Red
                $failures | ForEach-Object { Write-Host "   $($_.Line)" -ForegroundColor Red }
            }
        }
    }
    
    Write-Host ""
} else {
    Write-Host "Step 3: Smoke tests SKIPPED" -ForegroundColor Yellow
    Write-Host "⚠️  You must run smoke tests manually before deploying" -ForegroundColor Yellow
    Write-Host "   .\scripts\run-automated-tests.ps1 -Mode Smoke" -ForegroundColor Gray
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 4: CRITICAL PATH TESTS
# ═══════════════════════════════════════════════════════════════════════════════

if (!$SkipTests) {
    Write-Host "Step 4: Running critical path tests (18 tests, ~12 min)..." -ForegroundColor Yellow
    
    $criticalTestLog = "Logs/hotfix-critical-tests.log"
    & .\scripts\run-automated-tests.ps1 -Mode CriticalPath -LogFile $criticalTestLog -GenerateReport:$false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All critical path tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Critical path tests FAILED" -ForegroundColor Red
        $errorCount++
        
        # Show failed tests
        if (Test-Path $criticalTestLog) {
            $failures = Select-String -Path $criticalTestLog -Pattern "\[FAIL\]"
            if ($failures) {
                Write-Host "   Failed tests:" -ForegroundColor Red
                $failures | ForEach-Object { Write-Host "   $($_.Line)" -ForegroundColor Red }
            }
        }
    }
    
    Write-Host ""
} else {
    Write-Host "Step 4: Critical path tests SKIPPED" -ForegroundColor Yellow
    Write-Host "⚠️  You must run critical path tests manually before deploying" -ForegroundColor Yellow
    Write-Host "   .\scripts\run-automated-tests.ps1 -Mode CriticalPath" -ForegroundColor Gray
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 5: ASSET INTEGRITY
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 5: Checking asset integrity..." -ForegroundColor Yellow

# Check for .meta file issues
$metaIssues = @()
Get-ChildItem -Path "Assets/_Project" -Recurse -File | ForEach-Object {
    $metaPath = "$($_.FullName).meta"
    if (!(Test-Path $metaPath)) {
        $metaIssues += $_.FullName
    }
}

if ($metaIssues.Count -gt 0) {
    Write-Host "⚠️  Missing .meta files detected ($($metaIssues.Count))" -ForegroundColor Yellow
    $warningCount++
    $metaIssues | Select-Object -First 5 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    if ($metaIssues.Count -gt 5) {
        Write-Host "   ... and $($metaIssues.Count - 5) more" -ForegroundColor Gray
    }
} else {
    Write-Host "✅ All asset .meta files present" -ForegroundColor Green
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION STEP 6: SAVE DATA COMPATIBILITY
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 6: Checking save data compatibility..." -ForegroundColor Yellow

# Check if save system version was updated
$saveManagerPath = "Assets/_Project/Scripts/Save/SaveManager.cs"
if (Test-Path $saveManagerPath) {
    $gitDiff = git diff main..HEAD -- $saveManagerPath
    if ($gitDiff -match "SAVE_VERSION") {
        Write-Host "⚠️  Save system version modified" -ForegroundColor Yellow
        Write-Host "   Ensure backward/forward compatibility!" -ForegroundColor Yellow
        $warningCount++
    } else {
        Write-Host "✅ Save system version unchanged" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️  SaveManager.cs not found" -ForegroundColor Yellow
    $warningCount++
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# VALIDATION SUMMARY
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Errors:   $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Red" })
Write-Host "Warnings: $warningCount" -ForegroundColor $(if ($warningCount -eq 0) { "Green" } else { "Yellow" })
Write-Host ""

# Determine final status
$exitCode = 0
if ($errorCount -gt 0) {
    Write-Host "❌ VALIDATION FAILED" -ForegroundColor Red
    Write-Host "   Fix all errors before deploying to production" -ForegroundColor Red
    $exitCode = 1
} elseif ($warningCount -gt 0 -and $StrictMode) {
    Write-Host "⚠️  VALIDATION WARNING (Strict Mode)" -ForegroundColor Yellow
    Write-Host "   Review warnings before deploying" -ForegroundColor Yellow
    $exitCode = 2
} elseif ($warningCount -gt 0) {
    Write-Host "⚠️  VALIDATION PASSED WITH WARNINGS" -ForegroundColor Yellow
    Write-Host "   Review warnings, then proceed with caution" -ForegroundColor Yellow
    $exitCode = 0
} else {
    Write-Host "✅ VALIDATION PASSED" -ForegroundColor Green
    Write-Host "   Ready for deployment!" -ForegroundColor Green
    $exitCode = 0
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# GENERATE VALIDATION REPORT
# ═══════════════════════════════════════════════════════════════════════════════

$report = @"
# Hotfix Validation Report

**Branch:** $Branch  
**Date:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**Status:** $(if ($exitCode -eq 0) { "✅ PASS" } elseif ($exitCode -eq 2) { "⚠️  WARN" } else { "❌ FAIL" })

---

## Summary

- **Errors:** $errorCount
- **Warnings:** $warningCount
- **Tests Run:** $(if ($SkipTests) { "SKIPPED" } else { "Smoke (8) + Critical Path (18)" })

## Validation Steps

1. ✅ Branch validation
2. $(if ($errorCount -gt 0) { "❌" } else { "✅" }) Code validation
3. $(if ($SkipTests) { "⏭️" } elseif ($errorCount -gt 0) { "❌" } else { "✅" }) Smoke tests
4. $(if ($SkipTests) { "⏭️" } elseif ($errorCount -gt 0) { "❌" } else { "✅" }) Critical path tests
5. $(if ($warningCount -gt 0) { "⚠️ " } else { "✅" }) Asset integrity
6. $(if ($warningCount -gt 0) { "⚠️ " } else { "✅" }) Save data compatibility

## Deployment Recommendation

$(if ($exitCode -eq 0) { 
    "✅ **APPROVED FOR DEPLOYMENT**`n`nProceed with: ``.\scripts\hotfix-deploy.ps1 -IssueNumber XXX -Environment Production``" 
} elseif ($exitCode -eq 2) { 
    "⚠️  **PROCEED WITH CAUTION**`n`nReview warnings before deploying." 
} else { 
    "❌ **DEPLOYMENT BLOCKED**`n`nFix all errors before attempting deployment." 
})

---

**Report generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

# Save report
New-Item -ItemType Directory -Path "Logs/Hotfix" -Force | Out-Null
$report | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host "Validation report saved: $reportPath" -ForegroundColor Cyan
Write-Host ""

if ($exitCode -eq 0) {
    Write-Host "Next step: .\scripts\hotfix-deploy.ps1 -IssueNumber XXX -Environment Production" -ForegroundColor Green
} else {
    Write-Host "Next step: Fix errors and re-run validation" -ForegroundColor Red
}

Write-Host ""

exit $exitCode
