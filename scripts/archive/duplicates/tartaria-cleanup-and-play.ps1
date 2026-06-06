#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    TARTARIA — Automated Scene Cleanup + Build Pipeline
.DESCRIPTION
    1. Kills running Unity instance
    2. Runs Unity batch mode to remove missing script references
    3. Launches normal build pipeline (tartaria-play.ps1)
.NOTES
    Created: 2026-05-26 (Dr. Vex Aurelian, Unity 2100 agent)
    Reason: 154 missing MonoBehaviour refs blocking Play mode entry
#>

param(
    [switch]$SkipCleanup,    # Skip the cleanup step (jump straight to play)
    [switch]$BatchOnly       # Pass through to tartaria-play.ps1
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $projectPath

Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " TARTARIA -- Automated Cleanup + Build Pipeline" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# ────────────────────────────────────────────────────────
# STEP 1: Kill running Unity
# ────────────────────────────────────────────────────────
Write-Host "`n[1/3] Terminating running Unity instances..." -ForegroundColor Yellow

$unityProc = Get-Process -Name "Unity" -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainWindowTitle -match "TARTARIA" } | 
    Select-Object -First 1

if ($unityProc) {
    Write-Host "  Killing Unity PID $($unityProc.Id)..." -ForegroundColor White
    Stop-Process -Id $unityProc.Id -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    Write-Host "  ✓ Terminated" -ForegroundColor Green
} else {
    Write-Host "  No Unity instance running" -ForegroundColor Gray
}

# ────────────────────────────────────────────────────────
# STEP 2: Run Unity batch mode cleanup (optional)
# ────────────────────────────────────────────────────────
if (-not $SkipCleanup) {
    Write-Host "`n[2/3] Running scene cleanup (batch mode)..." -ForegroundColor Yellow
    Write-Host "  Removing missing MonoBehaviour script references..." -ForegroundColor White

    $unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
    $logFile = "$projectPath\Logs\cleanup-batch.log"
    
    if (-not (Test-Path $unityExe)) {
        Write-Host "  ⚠️  Unity not found at: $unityExe" -ForegroundColor Red
        Write-Host "  Skipping cleanup..." -ForegroundColor Yellow
    } else {
        # Clear old log
        if (Test-Path $logFile) {
            Remove-Item $logFile -Force -ErrorAction SilentlyContinue
        }

        $batchArgs = @(
            "-quit"
            "-batchmode"
            "-nographics"
            "-projectPath", "`"$projectPath`""
            "-executeMethod", "Tartaria.Editor.CleanupMissingScripts.CleanupAllScenesBatch"
            "-logFile", "`"$logFile`""
        )

        Write-Host "  Unity batch mode executing cleanup..." -ForegroundColor Gray
        $process = Start-Process -FilePath $unityExe -ArgumentList $batchArgs -PassThru -NoNewWindow
        
        # Wait for completion (max 120s)
        $timeout = 120
        $elapsed = 0
        while (-not $process.HasExited -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 5
            $elapsed += 5
            Write-Host "    ... cleanup running (${elapsed}s)" -ForegroundColor Gray
        }

        if ($process.HasExited) {
            Write-Host "  ✓ Cleanup completed (exit code: $($process.ExitCode))" -ForegroundColor Green
            
            if (Test-Path $logFile) {
                Write-Host "`n  [CLEANUP LOG]" -ForegroundColor Yellow
                $logLines = Get-Content $logFile | Select-String -Pattern "CleanupMissingScripts|Removed.*missing"
                if ($logLines) {
                    $logLines | ForEach-Object { Write-Host "    $_" -ForegroundColor White }
                } else {
                    Write-Host "    No cleanup messages found in log" -ForegroundColor Gray
                }
            }
        } else {
            Write-Host "  ⚠️  Cleanup timed out after ${timeout}s" -ForegroundColor Yellow
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }
    }
} else {
    Write-Host "`n[2/3] Skipping cleanup (--SkipCleanup flag)" -ForegroundColor Gray
}

# ────────────────────────────────────────────────────────
# STEP 3: Launch normal build pipeline
# ────────────────────────────────────────────────────────
Write-Host "`n[3/3] Launching build pipeline..." -ForegroundColor Yellow

if ($BatchOnly) {
    Write-Host "  Mode: Batch validation only" -ForegroundColor Gray
    & "$projectPath\tartaria-play.ps1" -BatchOnly
} else {
    Write-Host "  Mode: Build + Play" -ForegroundColor Green
    & "$projectPath\tartaria-play.ps1"
}

Pop-Location
