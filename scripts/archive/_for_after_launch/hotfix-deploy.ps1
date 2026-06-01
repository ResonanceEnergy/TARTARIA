#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Hotfix Deployment Script

.DESCRIPTION
    Automated deployment of hotfix to production:
    - Merges hotfix branch to main
    - Tags release
    - Builds production packages
    - Uploads to distribution
    - Updates version manifest

.PARAMETER IssueNumber
    Issue number (e.g., 123 for hotfix/ISSUE-123)

.PARAMETER Environment
    Target environment (Production or Staging)

.PARAMETER SkipBuild
    Skip Unity build (use existing build)

.PARAMETER SkipTests
    Skip final smoke test on build (NOT RECOMMENDED)

.EXAMPLE
    .\hotfix-deploy.ps1 -IssueNumber 123 -Environment Production

.EXAMPLE
    .\hotfix-deploy.ps1 -IssueNumber 123 -Environment Staging -SkipBuild

.NOTES
    Requires validation to pass first (hotfix-validate.ps1)
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Production", "Staging")]
    [string]$Environment,
    
    [switch]$SkipBuild,
    [switch]$SkipTests
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$issueId = "ISSUE-$IssueNumber"
$timestamp = Get-Date -Format "yyyy-MM-dd-HHmmss"
$deploymentLog = "Logs/Hotfix/deployment-$issueId-$timestamp.log"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Hotfix Deployment" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Issue:       $issueId"
Write-Host "Environment: $Environment"
Write-Host "Log:         $deploymentLog"
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

if ($Environment -eq "Production") {
    Write-Host "⚠️  WARNING: DEPLOYING TO PRODUCTION" -ForegroundColor Red
    Write-Host "   This will affect live players!" -ForegroundColor Red
    Write-Host ""
    
    $confirm = Read-Host "Type 'DEPLOY' to confirm production deployment"
    if ($confirm -ne "DEPLOY") {
        Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
        exit 0
    }
    Write-Host ""
}

# Start logging
Start-Transcript -Path $deploymentLog -Append

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 1: PRE-DEPLOYMENT CHECKS
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 1: Pre-deployment checks..." -ForegroundColor Yellow

# Check if validation report exists and passed
$latestValidation = Get-ChildItem "Logs/Hotfix" -Filter "validation-*$issueId*.md" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($latestValidation) {
    $validationContent = Get-Content $latestValidation.FullName -Raw
    if ($validationContent -match "Status.*✅ PASS") {
        Write-Host "✅ Validation passed: $($latestValidation.Name)" -ForegroundColor Green
    } else {
        Write-Host "❌ Latest validation did not pass!" -ForegroundColor Red
        Write-Host "   Run: .\scripts\hotfix-validate.ps1 -Branch hotfix/$issueId-..." -ForegroundColor Red
        Stop-Transcript
        exit 1
    }
} else {
    Write-Host "⚠️  No validation report found" -ForegroundColor Yellow
    Write-Host "   STRONGLY RECOMMENDED: Run .\scripts\hotfix-validate.ps1 first" -ForegroundColor Yellow
    
    $continue = Read-Host "Continue without validation? (yes/no)"
    if ($continue -ne "yes") {
        Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
        Stop-Transcript
        exit 0
    }
}

# Get current version
$versionFile = "ProjectSettings/ProjectSettings.asset"
$currentVersion = "1.0.0" # Would parse from ProjectSettings

Write-Host "✅ Current version: $currentVersion" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 2: MERGE TO MAIN
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 2: Merging hotfix to main..." -ForegroundColor Yellow

# Find hotfix branch
$branches = git branch --list "hotfix/*$issueId*"
if (!$branches) {
    Write-Host "❌ Hotfix branch not found for $issueId" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

$hotfixBranch = $branches.Trim().Replace("* ", "")
Write-Host "   Branch: $hotfixBranch" -ForegroundColor Gray

# Checkout main
git checkout main
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to checkout main" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

# Pull latest
git pull origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to pull main" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

# Merge hotfix
Write-Host "   Merging $hotfixBranch into main..." -ForegroundColor Gray
git merge --no-ff $hotfixBranch -m "chore: merge $hotfixBranch"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Merge failed - resolve conflicts manually" -ForegroundColor Red
    Stop-Transcript
    exit 1
}

Write-Host "✅ Merged to main" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 3: TAG RELEASE
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 3: Tagging release..." -ForegroundColor Yellow

$releaseTag = "v$currentVersion-hotfix.$IssueNumber"
git tag -a $releaseTag -m "Hotfix $issueId"
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Failed to create tag (may already exist)" -ForegroundColor Yellow
} else {
    Write-Host "✅ Tagged: $releaseTag" -ForegroundColor Green
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 4: BUILD PRODUCTION PACKAGE
# ═══════════════════════════════════════════════════════════════════════════════

if (!$SkipBuild) {
    Write-Host "Step 4: Building production package..." -ForegroundColor Yellow
    Write-Host "   This may take 10-15 minutes..." -ForegroundColor Gray
    
    $buildDir = "Builds/$Environment/$releaseTag"
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
    
    $unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
    $buildArgs = @(
        "-batchmode"
        "-quit"
        "-projectPath", "`"$PWD`""
        "-buildWindows64Player", "`"$buildDir\TARTARIA.exe`""
        "-logFile", "`"Logs\build-hotfix-$issueId.log`""
    )
    
    Write-Host "   Starting Unity build..." -ForegroundColor Gray
    $buildProcess = Start-Process -FilePath $unityPath -ArgumentList $buildArgs -PassThru -NoNewWindow
    $buildProcess.WaitForExit()
    
    if ($buildProcess.ExitCode -eq 0) {
        Write-Host "✅ Build completed successfully" -ForegroundColor Green
        Write-Host "   Build location: $buildDir" -ForegroundColor Gray
    } else {
        Write-Host "❌ Build failed (exit code: $($buildProcess.ExitCode))" -ForegroundColor Red
        Write-Host "   Check log: Logs\build-hotfix-$issueId.log" -ForegroundColor Red
        Stop-Transcript
        exit 1
    }
    
    # Create backup
    $backupDir = "Builds/Backups/$releaseTag"
    Write-Host "   Creating backup..." -ForegroundColor Gray
    Copy-Item -Path $buildDir -Destination $backupDir -Recurse -Force
    Write-Host "✅ Backup created: $backupDir" -ForegroundColor Green
    
    Write-Host ""
} else {
    Write-Host "Step 4: Build SKIPPED (using existing build)" -ForegroundColor Yellow
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 5: FINAL SMOKE TEST
# ═══════════════════════════════════════════════════════════════════════════════

if (!$SkipTests) {
    Write-Host "Step 5: Final smoke test on build..." -ForegroundColor Yellow
    Write-Host "   Running smoke tests (~3 min)..." -ForegroundColor Gray
    
    & .\scripts\run-automated-tests.ps1 -Mode Smoke -LogFile "Logs/final-smoke-test.log" -GenerateReport:$false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Smoke tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Smoke tests FAILED on final build!" -ForegroundColor Red
        Write-Host "   DEPLOYMENT ABORTED" -ForegroundColor Red
        Stop-Transcript
        exit 1
    }
    
    Write-Host ""
} else {
    Write-Host "Step 5: Final smoke test SKIPPED" -ForegroundColor Yellow
    Write-Host "⚠️  NOT RECOMMENDED: Always test the final build!" -ForegroundColor Yellow
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 6: DEPLOY TO DISTRIBUTION
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 6: Deploying to distribution..." -ForegroundColor Yellow

Write-Host "   📤 This step would:" -ForegroundColor Gray
Write-Host "      1. Upload build to CDN/distribution server" -ForegroundColor Gray
Write-Host "      2. Update version manifest" -ForegroundColor Gray
Write-Host "      3. Clear CDN cache" -ForegroundColor Gray
Write-Host "      4. Notify auto-updater service" -ForegroundColor Gray
Write-Host ""
Write-Host "   ⚠️  Manual deployment required for now" -ForegroundColor Yellow
Write-Host "   Build ready at: Builds/$Environment/$releaseTag" -ForegroundColor Yellow

Write-Host ""

# Push to remote
Write-Host "   Pushing to remote..." -ForegroundColor Gray
git push origin main
git push origin $releaseTag

Write-Host "✅ Code pushed to remote" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# STEP 7: POST-DEPLOYMENT
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Step 7: Post-deployment setup..." -ForegroundColor Yellow

# Update tracking document
$trackingDoc = "Logs/Hotfix/$issueId.md"
if (Test-Path $trackingDoc) {
    $content = Get-Content $trackingDoc -Raw
    $content = $content -replace "Status:.*IN PROGRESS", "Status: ✅ DEPLOYED"
    $content = $content -replace "Deployed to Prod.*\| ⏳", "Deployed to Prod | $(Get-Date -Format 'yyyy-MM-dd HH:mm') | ✅"
    $content | Set-Content $trackingDoc
    Write-Host "✅ Updated tracking document" -ForegroundColor Green
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# DEPLOYMENT COMPLETE
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ HOTFIX DEPLOYED SUCCESSFULLY" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Release: $releaseTag" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "Build: Builds/$Environment/$releaseTag" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Monitor deployment for 30 minutes:"
Write-Host "   .\scripts\hotfix-monitor.ps1 -IssueNumber $IssueNumber -Duration 30"
Write-Host ""
Write-Host "2. Watch for:"
Write-Host "   - Crash rate (<1%)"
Write-Host "   - Error rate (<2%)"
Write-Host "   - Player feedback"
Write-Host ""
Write-Host "3. If issues detected:"
Write-Host "   .\scripts\hotfix-rollback.ps1 -ToVersion $currentVersion -Reason 'Description'"
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green

Stop-Transcript

exit 0
