#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Hotfix Initialization Script

.DESCRIPTION
    Initializes a new hotfix workflow:
    - Creates hotfix branch from main
    - Generates issue tracking document
    - Sets up hotfix workspace
    - Creates validation checklist

.PARAMETER IssueNumber
    Issue number for tracking (e.g., 123 for hotfix/ISSUE-123)

.PARAMETER Description
    Brief description of the hotfix (used in branch name and docs)

.PARAMETER Priority
    Hotfix priority: P0 (critical) or P1 (high)

.EXAMPLE
    .\hotfix-start.ps1 -IssueNumber 123 -Description "Fix combat crash" -Priority P0

.NOTES
    Must be run from repo root directory.
    Creates branch: hotfix/ISSUE-{number}-{description}
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,
    
    [Parameter(Mandatory=$true)]
    [string]$Description,
    
    [ValidateSet("P0", "P1")]
    [string]$Priority = "P1"
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$issueId = "ISSUE-$IssueNumber"
$branchSuffix = $Description.ToLower() -replace '\s+', '-' -replace '[^a-z0-9-]', ''
$branchName = "hotfix/$issueId-$branchSuffix"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$hotfixDir = "Logs/Hotfix"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Hotfix Initialization" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Issue:       $issueId"
Write-Host "Description: $Description"
Write-Host "Priority:    $Priority"
Write-Host "Branch:      $branchName"
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 1: CREATE HOTFIX BRANCH
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 1: Creating hotfix branch..." -ForegroundColor Yellow

# Ensure we're on main and up-to-date
git checkout main
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to checkout main branch"
    exit 1
}

git pull origin main
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to pull latest main"
    exit 1
}

# Create and checkout hotfix branch
git checkout -b $branchName
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create hotfix branch"
    exit 1
}

Write-Host "✅ Hotfix branch created: $branchName" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 2: CREATE HOTFIX TRACKING DOCUMENT
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 2: Creating tracking document..." -ForegroundColor Yellow

# Ensure hotfix directory exists
New-Item -ItemType Directory -Path $hotfixDir -Force | Out-Null

$docPath = Join-Path $hotfixDir "$issueId.md"

$doc = @"
# Hotfix: $issueId - $Description

**Priority:** $Priority  
**Created:** $timestamp  
**Branch:** ``$branchName``  
**Status:** 🟡 IN PROGRESS

---

## Issue Details

### Problem Description
<!-- Describe the bug/issue that requires a hotfix -->


### Reproduction Steps
1. 
2. 
3. 

### Expected Behavior
<!-- What should happen -->


### Actual Behavior
<!-- What actually happens -->


### Impact Assessment
- **Severity:** $Priority (Critical/High)
- **Affected Systems:** <!-- e.g., Combat, Save/Load, UI -->
- **Player Impact:** <!-- How many players affected? -->
- **Workaround Available:** ❌ No / ✅ Yes (describe)

---

## Fix Implementation

### Root Cause
<!-- Technical explanation of what caused the bug -->


### Solution Approach
<!-- High-level description of the fix -->


### Files Changed
<!-- List all files modified -->
- 


### Tests Added
<!-- List any new tests created for this fix -->
- 


### Regression Risk
**Assessment:** 🟢 LOW / 🟡 MEDIUM / 🔴 HIGH

**Explanation:**


---

## Validation Checklist

### Pre-Deployment
- [ ] Code compiles without errors
- [ ] No critical warnings introduced
- [ ] Smoke tests pass (8/8)
- [ ] Critical path tests pass (18/18)
- [ ] Integration tests pass (79/79)
- [ ] Performance benchmarks meet targets
- [ ] Save data compatibility verified
- [ ] Fix verified in local testing
- [ ] Peer review completed
- [ ] Hotfix validation report generated

### Deployment
- [ ] Merged to main
- [ ] Release tagged (v{version}-hotfix.{number})
- [ ] Build created and tested
- [ ] Deployed to distribution
- [ ] Health checks passing

### Post-Deployment
- [ ] Initial monitoring complete (30 min)
- [ ] Crash rate within threshold (<1%)
- [ ] Error rate within threshold (<2%)
- [ ] Player feedback positive/neutral
- [ ] Issue confirmed resolved
- [ ] Rollback plan NOT needed

---

## Timeline

| Milestone | Time | Status |
|-----------|------|--------|
| Issue Identified | $timestamp | ✅ |
| Hotfix Started | $timestamp | ✅ |
| Fix Implemented | | ⏳ |
| Tests Passing | | ⏳ |
| Validation Complete | | ⏳ |
| Deployed to Prod | | ⏳ |
| Monitoring Complete | | ⏳ |

**Target SLA:** <4 hours from start to deployment  
**Actual Duration:** TBD

---

## Notes

### Development Notes


### Deployment Notes


### Post-Mortem
<!-- Fill this out after hotfix is complete -->
- What went well?
- What went wrong?
- How can we prevent this in the future?
- Lessons learned?

"@

$doc | Out-File -FilePath $docPath -Encoding UTF8
Write-Host "✅ Tracking document created: $docPath" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 3: SETUP WORKSPACE
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 3: Setting up workspace..." -ForegroundColor Yellow

# Create backup of current state
$backupDir = "Builds/Backups/pre-hotfix-$issueId"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

Write-Host "✅ Backup directory created: $backupDir" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 4: DISPLAY NEXT STEPS
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Hotfix Initialization Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. 📝 Update tracking document: $docPath"
Write-Host "   - Fill in problem description and reproduction steps"
Write-Host "   - Document impact assessment"
Write-Host ""
Write-Host "2. 🔧 Implement the fix in your IDE"
Write-Host "   - Make minimal, focused changes"
Write-Host "   - Add unit tests for the fix"
Write-Host ""
Write-Host "3. ✅ Test locally:"
Write-Host "   .\scripts\run-automated-tests.ps1 -Mode Smoke"
Write-Host "   .\scripts\run-automated-tests.ps1 -Mode CriticalPath"
Write-Host ""
Write-Host "4. 📤 Commit changes:"
Write-Host "   git add ."
Write-Host "   git commit -m `"hotfix($issueId): $Description`""
Write-Host "   git push origin $branchName"
Write-Host ""
Write-Host "5. ✔️  Validate before deploy:"
Write-Host "   .\scripts\hotfix-validate.ps1 -Branch $branchName"
Write-Host ""
Write-Host "6. 🚀 Deploy to production:"
Write-Host "   .\scripts\hotfix-deploy.ps1 -IssueNumber $IssueNumber -Environment Production"
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Current Branch: $branchName" -ForegroundColor Cyan
Write-Host "Priority: $Priority — SLA: <4 hours" -ForegroundColor $(if ($Priority -eq "P0") { "Red" } else { "Yellow" })
Write-Host ""

exit 0
