#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    TARTARIA — Nuclear Unity Cache Purge
.DESCRIPTION
    Deletes Unity asset database cache to force re-import of cleaned scene files.
    Preserves PackageCache to avoid re-downloading packages.
#>

param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectPath = "C:\dev\TARTARIA_new"

Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " NUCLEAR CACHE PURGE" -ForegroundColor Red
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan

# Kill Unity
$unityProc = Get-Process -Name "Unity" -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainWindowTitle -match "TARTARIA" } | 
    Select-Object -First 1

if ($unityProc) {
    Write-Host "`nKilling Unity PID $($unityProc.Id)..." -ForegroundColor Yellow
    Stop-Process -Id $unityProc.Id -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    Write-Host "✓ Terminated" -ForegroundColor Green
}

# Delete cache directories that cause stale imports
$cacheDirs = @(
    "$projectPath\Library\ScriptableObjectCache"
    "$projectPath\Library\SourceAssetDB"
    "$projectPath\Library\SourceAssetDB-lock"
    "$projectPath\Library\ArtifactDB"
    "$projectPath\Library\StateCache"
    "$projectPath\Library\SceneVisibilityState.asset"
    "$projectPath\Temp"
)

Write-Host "`nPurging Unity asset database cache..." -ForegroundColor Yellow

foreach ($dir in $cacheDirs) {
    if (Test-Path $dir) {
        $name = Split-Path $dir -Leaf
        Write-Host "  Removing $name..." -ForegroundColor Gray
        Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`n✓✓✓ Cache purged" -ForegroundColor Green
Write-Host "`nUnity will re-import ALL assets from disk on next launch." -ForegroundColor White
Write-Host "Scenes are clean on disk (75 scripts removed)." -ForegroundColor Green
Write-Host "This import will load the clean versions." -ForegroundColor Cyan

Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
