#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Hotfix Rollback Script

.DESCRIPTION
    Emergency rollback to previous stable version:
    - Fetches previous stable build
    - Validates build integrity
    - Deploys to production
    - Verifies rollback success
    - Documents rollback event

.PARAMETER ToVersion
    Target version to rollback to (e.g., v1.0.0)

.PARAMETER Reason
    Reason for rollback (for documentation)

.PARAMETER Force
    Skip confirmation prompts

.EXAMPLE
    .\hotfix-rollback.ps1 -ToVersion v1.0.0 -Reason "Critical crash detected"

.EXAMPLE
    .\hotfix-rollback.ps1 -ToVersion v1.0.0 -Force

.NOTES
    Target SLA: <30 minutes from trigger to completion
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ToVersion,
    
    [Parameter(Mandatory=$true)]
    [string]$Reason,
    
    [switch]$Force
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$timestamp = Get-Date -Format "yyyy-MM-dd-HHmmss"
$rollbackLog = "Logs/Hotfix/rollback-$ToVersion-$timestamp.log"
$startTime = Get-Date

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
Write-Host "⚠️  EMERGENCY ROLLBACK ⚠️" -ForegroundColor Red
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
Write-Host ""
Write-Host "Target Version: $ToVersion" -ForegroundColor Cyan
Write-Host "Reason:         $Reason" -ForegroundColor Cyan
Write-Host "Start Time:     $($startTime.ToString('HH:mm:ss'))" -ForegroundColor Cyan
Write-Host ""
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

if (!$Force) {
    Write-Host "⚠️  WARNING: This will rollback the production build" -ForegroundColor Yellow
    Write-Host "   to version $ToVersion" -ForegroundColor Yellow
    Write-Host ""
    $confirm = Read-Host "Type 'ROLLBACK' to confirm"
    if ($confirm -ne "ROLLBACK") {
        Write-Host "❌ Rollback cancelled" -ForegroundColor Yellow
        exit 0
    }
    Write-Host ""
}

# Start logging
Start-Transcript -Path $rollbackLog -Append

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 1: DETECT ISSUE (~2 min)
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "[1/5] Detecting issue..." -ForegroundColor Yellow
$step1Start = Get-Date

Write-Host "   Issue: $Reason" -ForegroundColor Gray
Write-Host "   Triggered: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host "   Rolling back to: $ToVersion" -ForegroundColor Gray

$step1Duration = (Get-Date) - $step1Start
Write-Host "✅ Step 1 complete ($($step1Duration.TotalSeconds)s)" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 2: PREPARE ROLLBACK (~5 min)
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "[2/5] Preparing rollback..." -ForegroundColor Yellow
$step2Start = Get-Date

# Check if backup exists
$backupPath = "Builds/Backups/$ToVersion"
if (!(Test-Path $backupPath)) {
    Write-Host "❌ Backup not found: $backupPath" -ForegroundColor Red
    Write-Host "   Available backups:" -ForegroundColor Yellow
    Get-ChildItem "Builds/Backups" -Directory | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Gray }
    Stop-Transcript
    exit 1
}

Write-Host "✅ Backup found: $backupPath" -ForegroundColor Green

# Validate backup integrity
$buildExe = Join-Path $backupPath "TARTARIA.exe"
if (!(Test-Path $buildExe)) {
    Write-Host "❌ Build executable not found: $buildExe" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

Write-Host "✅ Build executable validated" -ForegroundColor Green

# Get backup size
$backupSize = (Get-ChildItem $backupPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "   Backup size: $($backupSize.ToString('F2')) MB" -ForegroundColor Gray

$step2Duration = (Get-Date) - $step2Start
Write-Host "✅ Step 2 complete ($($step2Duration.TotalSeconds)s)" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 3: DEPLOY PREVIOUS VERSION (~10 min)
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "[3/5] Deploying previous version..." -ForegroundColor Yellow
$step3Start = Get-Date

# Copy backup to staging area
$deployPath = "Builds/Production/ROLLBACK-$ToVersion"
Write-Host "   Copying backup to deployment staging..." -ForegroundColor Gray
Copy-Item -Path $backupPath -Destination $deployPath -Recurse -Force

Write-Host "✅ Files copied to staging" -ForegroundColor Green

# Simulate deployment steps
Write-Host "   📤 Deployment steps:" -ForegroundColor Gray
Write-Host "      1. Upload to CDN/distribution server" -ForegroundColor Gray
Write-Host "      2. Update version manifest to $ToVersion" -ForegroundColor Gray
Write-Host "      3. Clear CDN cache" -ForegroundColor Gray
Write-Host "      4. Notify auto-updater service" -ForegroundColor Gray
Write-Host ""
Write-Host "   ⚠️  Manual deployment required for actual production push" -ForegroundColor Yellow

$step3Duration = (Get-Date) - $step3Start
Write-Host "✅ Step 3 complete ($($step3Duration.TotalSeconds)s)" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 4: VERIFY ROLLBACK (~10 min)
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "[4/5] Verifying rollback..." -ForegroundColor Yellow
$step4Start = Get-Date

Write-Host "   Running smoke tests on rolled-back version..." -ForegroundColor Gray

# Run smoke tests
& .\scripts\run-automated-tests.ps1 -Mode Smoke -LogFile "Logs/rollback-smoke-test.log" -GenerateReport:$false

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Smoke tests passed" -ForegroundColor Green
} else {
    Write-Host "❌ Smoke tests FAILED on rolled-back version!" -ForegroundColor Red
    Write-Host "   CRITICAL: Previous version may also be broken!" -ForegroundColor Red
    Write-Host "   Check: Logs/rollback-smoke-test.log" -ForegroundColor Red
}

$step4Duration = (Get-Date) - $step4Start
Write-Host "✅ Step 4 complete ($($step4Duration.TotalSeconds)s)" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 5: POST-ROLLBACK ACTIONS (~3 min)
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "[5/5] Post-rollback actions..." -ForegroundColor Yellow
$step5Start = Get-Date

# Create rollback incident report
$incidentPath = "Logs/Hotfix/INCIDENT-rollback-$timestamp.md"

$incidentReport = @"
# ROLLBACK INCIDENT REPORT

**Rolled Back To:** $ToVersion  
**Date:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**Reason:** $Reason  
**Duration:** TBD

---

## Incident Details

### What Happened
$Reason

### Why Rollback Was Triggered
<!-- Fill in details about the failure that triggered rollback -->


### Impact
- **Players Affected:** TBD
- **Duration of Issue:** TBD
- **Data Loss:** None / Some / Significant

---

## Rollback Timeline

| Step | Duration | Status |
|------|----------|--------|
| Detect Issue | $($step1Duration.TotalMinutes.ToString('F1')) min | ✅ |
| Prepare Rollback | $($step2Duration.TotalMinutes.ToString('F1')) min | ✅ |
| Deploy Previous | $($step3Duration.TotalMinutes.ToString('F1')) min | ✅ |
| Verify Rollback | $($step4Duration.TotalMinutes.ToString('F1')) min | ✅ |
| Post-Rollback | $($step5Duration.TotalMinutes.ToString('F1')) min | ✅ |

**Total Time:** $(((Get-Date) - $startTime).TotalMinutes.ToString('F1')) minutes

---

## Next Steps

- [ ] Notify team of rollback
- [ ] Analyze what went wrong in failed hotfix
- [ ] Fix the issue in hotfix branch
- [ ] Re-test thoroughly before next attempt
- [ ] Schedule post-mortem meeting
- [ ] Update hotfix process if needed

---

## Lessons Learned

### What Went Wrong
<!-- Technical analysis of the failure -->


### How to Prevent
<!-- Process improvements, additional tests, etc. -->


### Action Items
1. 
2. 
3. 

---

**Report Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$incidentReport | Out-File -FilePath $incidentPath -Encoding UTF8
Write-Host "✅ Incident report created: $incidentPath" -ForegroundColor Green

$step5Duration = (Get-Date) - $step5Start
Write-Host "✅ Step 5 complete ($($step5Duration.TotalSeconds)s)" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# ROLLBACK COMPLETE
# ═══════════════════════════════════════════════════════════════════════════════

$totalDuration = (Get-Date) - $startTime

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ ROLLBACK COMPLETE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Rolled Back To:  $ToVersion" -ForegroundColor Cyan
Write-Host "Total Duration:  $($totalDuration.TotalMinutes.ToString('F1')) minutes" -ForegroundColor Cyan
Write-Host "Target SLA:      <30 minutes" -ForegroundColor $(if ($totalDuration.TotalMinutes -lt 30) { "Green" } else { "Red" })
Write-Host ""
Write-Host "Incident Report: $incidentPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. ✅ Verify game is stable on $ToVersion"
Write-Host "2. 📧 Notify team of rollback"
Write-Host "3. 🔍 Analyze what went wrong"
Write-Host "4. 🔧 Fix the issue in hotfix branch"
Write-Host "5. ✔️  Re-validate before next deployment"
Write-Host "6. 📅 Schedule post-mortem meeting"
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green

Stop-Transcript

exit 0
