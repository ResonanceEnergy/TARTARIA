# TARTARIA — Complete M3 + M4 Pipeline
# Run after M1 manual test passes and Unity is closed
# Executes: Performance gates → Standalone build → Beta package
# Dr. Vex Aurelian — Session 5 master automation

param(
    [switch]$SkipPerf,      # Skip performance gates (if already run)
    [switch]$SkipBuild,     # Skip standalone build (if already exists)
    [switch]$PackageOnly    # Only create beta package (assumes M3 complete)
)

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " TARTARIA — M3 + M4 COMPLETE PIPELINE" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Run performance gates (PerformanceGateRunner)" -ForegroundColor White
Write-Host "  2. Build standalone Windows .exe" -ForegroundColor White
Write-Host "  3. Create beta package .zip" -ForegroundColor White
Write-Host ""

# Check Unity not running
$unityProc = Get-Process Unity -ErrorAction SilentlyContinue
if ($unityProc) {
    Write-Host "ERROR: Unity Editor is running (PID $($unityProc.Id))" -ForegroundColor Red
    Write-Host "Close Unity Editor first, then re-run this script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "Unity Editor not running -- proceeding" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to start, or Ctrl+C to cancel..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Write-Host ""

$startTime = Get-Date

# M3: Performance + Build
if (-not $PackageOnly) {
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " PHASE 1: M3 GATES" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
    
    & .\run-m3-gates.ps1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "M3 gates failed. Check logs and fix issues before packaging." -ForegroundColor Red
        Write-Host ""
        Write-Host "Press any key to exit..." -ForegroundColor DarkGray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit $LASTEXITCODE
    }
    
    Write-Host ""
    Write-Host "M3 GATES PASSED" -ForegroundColor Green
    Write-Host ""
}

# M4: Beta Package
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " PHASE 2: M4 BETA PACKAGE" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

& .\create-beta-package.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Beta package creation failed." -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit $LASTEXITCODE
}

$duration = (Get-Date) - $startTime

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " M3 + M4 COMPLETE" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total time: $([math]::Round($duration.TotalMinutes, 1)) minutes" -ForegroundColor Cyan
Write-Host ""

# Summary
$zipFile = Get-ChildItem "TARTARIA_Beta_Echohaven_VerticalSlice_*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($zipFile) {
    Write-Host "BETA PACKAGE READY FOR DISTRIBUTION:" -ForegroundColor Yellow
    Write-Host "   $($zipFile.FullName)" -ForegroundColor Cyan
    Write-Host "   Size: $([math]::Round($zipFile.Length / 1MB, 1)) MB" -ForegroundColor Cyan
    Write-Host ""
}

$exeFile = "Build\Windows\Tartaria.exe"
if (Test-Path $exeFile) {
    Write-Host "STANDALONE BUILD:" -ForegroundColor Yellow
    Write-Host "   $(Resolve-Path $exeFile)" -ForegroundColor Cyan
    Write-Host ""
}

$perfResults = Get-ChildItem "Assets\_Project\Generated\CI_Results\R6_PerfGates_*.json" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($perfResults) {
    Write-Host "PERFORMANCE RESULTS:" -ForegroundColor Yellow
    $json = Get-Content $perfResults.FullName -Raw | ConvertFrom-Json
    Write-Host "   Tier: $($json.tier)" -ForegroundColor Cyan
    Write-Host "   Avg FPS: $($json.avgFps)" -ForegroundColor Cyan
    Write-Host "   1% Low: $($json.onePercentLow)" -ForegroundColor Cyan
    Write-Host "   Peak RAM: $($json.peakMemoryMB) MB" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Test the standalone .exe launch" -ForegroundColor White
Write-Host "  2. Extract and test the .zip package" -ForegroundColor White
Write-Host "  3. Upload to distribution platform (itch.io / Steam)" -ForegroundColor White
Write-Host "  4. Share with beta testers" -ForegroundColor White
Write-Host ""
Write-Host "12-HOUR BETA SPRINT COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
