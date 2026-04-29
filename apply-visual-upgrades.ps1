#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Visual Upgrade Automation Script
.DESCRIPTION
    Executes P1 (Custom Shaders) and P2 (VFX Upgrade) via Unity batch mode.
    Fully automated - no GUI interaction required.
.EXAMPLE
    .\apply-visual-upgrades.ps1
    .\apply-visual-upgrades.ps1 -P0Only    # Only run FBX import (requires manual Mixamo download first)
#>
param(
    [switch]$P0Only,
    [switch]$SkipP0
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$ProjectPath = $PWD.Path
$LogDir = Join-Path $ProjectPath "Logs"

Write-Host ""
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host "   TARTARIA -- Visual Upgrade Automation" -ForegroundColor Cyan
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $UnityPath)) {
    Write-Host "ERROR: Unity not found at: $UnityPath" -ForegroundColor Red
    exit 1
}

# Create Logs directory if needed
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir | Out-Null
}

function Invoke-UnityBatchMethod {
    param(
        [string]$MethodName,
        [string]$Description,
        [string]$LogFile
    )

    Write-Host ""
    Write-Host "== $Description ==" -ForegroundColor Yellow
    Write-Host "  Method: $MethodName"
    Write-Host "  Log:    $LogFile"
    Write-Host ""

    $logPath = Join-Path $LogDir $LogFile

    $unityArgs = @(
        "-batchmode"
        "-projectPath", "`"$ProjectPath`""
        "-executeMethod", $MethodName
        "-logFile", "`"$logPath`""
        "-quit"
    )

    Write-Host "  Unity command: $UnityPath $($unityArgs -join ' ')" -ForegroundColor Gray

    & $UnityPath $unityArgs

    $exitCode = $LASTEXITCODE
    
    # Wait a moment for log file to be written
    Start-Sleep -Seconds 2

    if ($exitCode -ne 0) {
        Write-Host "  FAILED (exit code $exitCode)" -ForegroundColor Red
        Write-Host ""
        Write-Host "-- Last 50 lines of log --" -ForegroundColor Yellow
        Get-Content $logPath -Tail 50
        Write-Host "-- End --" -ForegroundColor Yellow
        return $false
    } else {
        Write-Host "  PASSED" -ForegroundColor Green
        
        # Extract [CustomShaders] / [VFX] / [FBXWizard] lines from log
        Write-Host ""
        Write-Host "-- Operation Log --" -ForegroundColor Yellow
        Get-Content $logPath | Where-Object { $_ -match "\[(CustomShaders|VFX|FBXWizard)\]" } | ForEach-Object {
            Write-Host "  $_"
        }
        Write-Host "-- End --" -ForegroundColor Yellow
        return $true
    }
}

if ($P0Only) {
    Write-Host "P0 ONLY MODE: Importing character meshes from FBX files..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "PREREQUISITE: You must manually download 4 FBX files from Mixamo first:" -ForegroundColor Yellow
    Write-Host "  1. Visit https://www.mixamo.com/" -ForegroundColor Yellow
    Write-Host "  2. Download these characters (T-pose, FBX for Unity):" -ForegroundColor Yellow
    Write-Host "     • Adventurer → Player_Mesh.fbx" -ForegroundColor Yellow
    Write-Host "     • Queen → Anastasia_Mesh.fbx" -ForegroundColor Yellow
    Write-Host "     • Worker → Milo_Mesh.fbx" -ForegroundColor Yellow
    Write-Host "     • Mutant → MudGolem_Mesh.fbx" -ForegroundColor Yellow
    Write-Host "  3. Save to: Assets\_Project\Models\Characters\" -ForegroundColor Yellow
    Write-Host ""
    
    $charDir = Join-Path $ProjectPath "Assets\_Project\Models\Characters"
    if (-not (Test-Path $charDir)) {
        New-Item -ItemType Directory -Path $charDir -Force | Out-Null
        Write-Host "Created: $charDir" -ForegroundColor Green
    }
    
    $fbxFiles = Get-ChildItem -Path $charDir -Filter "*.fbx" -File
    if ($fbxFiles.Count -eq 0) {
        Write-Host "ERROR: No FBX files found in $charDir" -ForegroundColor Red
        Write-Host "Please download the 4 character meshes from Mixamo first." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Found $($fbxFiles.Count) FBX file(s):" -ForegroundColor Green
    $fbxFiles | ForEach-Object { Write-Host "  • $($_.Name)" -ForegroundColor Green }
    Write-Host ""
    
    $success = Invoke-UnityBatchMethod `
        -MethodName "Tartaria.Editor.FBXImportWizard.ImportAllCLI" `
        -Description "P0: Import Character Meshes" `
        -LogFile "visual-upgrade-p0.log"
    
    if ($success) {
        Write-Host ""
        Write-Host "P0 CHARACTER MESH IMPORT COMPLETE!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "P0 FAILED - Check log for details" -ForegroundColor Red
        exit 1
    }
    
    exit 0
}

# Full pipeline: P1 → P2 → (optional P0)

$results = @()

Write-Host "Executing P1: Custom Shaders (materials + scene application)..." -ForegroundColor Cyan
$p1Success = Invoke-UnityBatchMethod `
    -MethodName "Tartaria.Editor.CustomShaderApplicator.ApplyAllCLI" `
    -Description "P1: Apply Custom Shaders" `
    -LogFile "visual-upgrade-p1.log"
$results += @{ Name = "P1 (Custom Shaders)"; Success = $p1Success }

Write-Host ""
Write-Host "Executing P2: VFX Upgrade (500-2000 particle enhancements)..." -ForegroundColor Cyan
$p2Success = Invoke-UnityBatchMethod `
    -MethodName "Tartaria.Editor.VFXUpgradeTool.UpgradeAllCLI" `
    -Description "P2: Upgrade VFX" `
    -LogFile "visual-upgrade-p2.log"
$results += @{ Name = "P2 (VFX Upgrade)"; Success = $p2Success }

if (-not $SkipP0) {
    Write-Host ""
    Write-Host "Checking for P0 FBX files (optional character mesh import)..." -ForegroundColor Cyan
    
    $charDir = Join-Path $ProjectPath "Assets\_Project\Models\Characters"
    if (Test-Path $charDir) {
        $fbxFiles = Get-ChildItem -Path $charDir -Filter "*.fbx" -File
        if ($fbxFiles.Count -gt 0) {
            Write-Host "Found $($fbxFiles.Count) FBX file(s) - importing..." -ForegroundColor Green
            
            $p0Success = Invoke-UnityBatchMethod `
                -MethodName "Tartaria.Editor.FBXImportWizard.ImportAllCLI" `
                -Description "P0: Import Character Meshes" `
                -LogFile "visual-upgrade-p0.log"
            $results += @{ Name = "P0 (Character Meshes)"; Success = $p0Success }
        } else {
            Write-Host "No FBX files found - skipping P0 (characters remain Unity primitives)" -ForegroundColor Yellow
            $results += @{ Name = "P0 (Character Meshes)"; Success = $null }
        }
    } else {
        Write-Host "Characters directory doesn't exist - skipping P0" -ForegroundColor Yellow
        $results += @{ Name = "P0 (Character Meshes)"; Success = $null }
    }
}

Write-Host ""
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host "   VISUAL UPGRADE AUTOMATION COMPLETE" -ForegroundColor Cyan
Write-Host "  ============================================" -ForegroundColor Cyan
Write-Host ""

$results | ForEach-Object {
    $status = if ($_.Success -eq $true) {
        "✓ PASS" 
    } elseif ($_.Success -eq $false) {
        "✗ FAIL"
    } else {
        "- SKIP"
    }
    
    $color = if ($_.Success -eq $true) { "Green" } elseif ($_.Success -eq $false) { "Red" } else { "Yellow" }
    Write-Host "  $status  $($_.Name)" -ForegroundColor $color
}

Write-Host ""

$failed = ($results | Where-Object { $_.Success -eq $false }).Count
if ($failed -gt 0) {
    Write-Host "Some operations failed. Check logs in Logs/ directory." -ForegroundColor Red
    exit 1
} else {
    Write-Host "All operations completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "  1. Run: .\tartaria-play.ps1 -BatchOnly" -ForegroundColor White
    Write-Host "     (Validate compilation)" -ForegroundColor Gray
    Write-Host "  2. Run: .\tartaria-play.ps1" -ForegroundColor White
    Write-Host "     (Launch and test visual upgrades in-game)" -ForegroundColor Gray
    
    if (($results | Where-Object { $_.Name -eq "P0 (Character Meshes)" -and $_.Success -eq $null })) {
        Write-Host ""
        Write-Host "  Optional P0: Download 4 Mixamo FBX files, then run:" -ForegroundColor Yellow
        Write-Host "    .\apply-visual-upgrades.ps1 -P0Only" -ForegroundColor Yellow
    }
    
    exit 0
}
