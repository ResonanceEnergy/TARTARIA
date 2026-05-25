<#
.SYNOPSIS
    TARTARIA Beta Build — Standalone Windows x64 .exe Generator

.DESCRIPTION
    Generates a production-ready standalone Windows build for beta distribution.
    
    Output: Build\Windows\Tartaria.exe
    
    Steps:
    1. Closes Unity Editor (if open)
    2. Runs Unity in batchmode to execute build pipeline
    3. Validates output .exe exists
    4. Reports build size + warnings/errors
    
.NOTES
    File: build-beta.ps1
    Generated: 2026-05-21 (Session 6 -- Dr. Vex Aurelian autonomous beta sprint)
    Unity: 6000.3.6f1
    Target: GTX 1070 @ Medium tier (60 FPS, 3.6GB RAM)
#>

[CmdletBinding()]
param(
    [switch]$SkipEditorClose
)

$ErrorActionPreference = "Stop"

# ── Configuration ──
$ProjectPath = "C:\dev\TARTARIA_new"
$UnityEditor = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$OutputExe   = Join-Path $ProjectPath "Build\Windows\Tartaria.exe"
$BuildLog    = Join-Path $ProjectPath "Logs\standalone-build.log"

Write-Host ""
Write-Host "  ══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   TARTARIA -- BETA BUILD GENERATOR" -ForegroundColor Yellow
Write-Host "  ══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ── Step 1: Close Unity Editor (if running) ──
if (-not $SkipEditorClose) {
    Write-Host "[1/4] Checking for running Unity Editor..." -ForegroundColor DarkCyan
    $unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*Editor*" }
    if ($unityProcs) {
        Write-Host "  Found $($unityProcs.Count) Unity Editor process(es). Closing..." -ForegroundColor Yellow
        $unityProcs | ForEach-Object { $_.CloseMainWindow() | Out-Null }
        Start-Sleep -Seconds 3
        $unityProcs | Where-Object { -not $_.HasExited } | ForEach-Object { $_.Kill() }
        Write-Host "  ✓ Unity Editor closed." -ForegroundColor Green
    } else {
        Write-Host "  ✓ No Unity Editor running." -ForegroundColor Green
    }
}

# ── Step 2: Run Unity batchmode build ──
Write-Host "[2/4] Running Unity batchmode build pipeline..." -ForegroundColor DarkCyan
Write-Host "  Method: Tartaria.EditorTools.BuildPlayerPipeline.BuildWindows" -ForegroundColor Gray
Write-Host "  Output: $OutputExe" -ForegroundColor Gray

Remove-Item $BuildLog -ErrorAction SilentlyContinue

$buildArgs = @(
    "-batchmode"
    "-projectPath", "`"$ProjectPath`""
    "-executeMethod", "Tartaria.EditorTools.BuildPlayerPipeline.BuildWindows"
    "-logFile", "`"$BuildLog`""
    "-quit"
)

$proc = Start-Process -FilePath $UnityEditor -ArgumentList $buildArgs -NoNewWindow -PassThru -Wait
$exitCode = $proc.ExitCode

Write-Host "  Unity exit code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })

# ── Step 3: Validate output ──
Write-Host "[3/4] Validating build output..." -ForegroundColor DarkCyan

if (-not (Test-Path $OutputExe)) {
    Write-Host "  ✗ Build FAILED -- .exe not found at $OutputExe" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check build log for errors:" -ForegroundColor Yellow
    Write-Host "  $BuildLog" -ForegroundColor Gray
    exit 1
}

$exeSize = (Get-Item $OutputExe).Length / 1MB
Write-Host "  ✓ Build SUCCEEDED" -ForegroundColor Green
Write-Host "  Size: $($exeSize.ToString('F2')) MB" -ForegroundColor Green
Write-Host "  Path: $OutputExe" -ForegroundColor Gray

# ── Step 4: Parse build log for warnings/errors ──
Write-Host "[4/4] Analyzing build log..." -ForegroundColor DarkCyan

if (Test-Path $BuildLog) {
    $logContent = Get-Content $BuildLog -Raw
    $warnings = ([regex]::Matches($logContent, "warning", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
    $errors = ([regex]::Matches($logContent, "error", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count - ([regex]::Matches($logContent, "0 errors", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
    
    Write-Host "  Warnings: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })
    Write-Host "  Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
}

Write-Host ""
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " BETA BUILD COMPLETE" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Test the build: .\Build\Windows\Tartaria.exe" -ForegroundColor Gray
Write-Host "  2. Package for distribution: compress Build\Windows\ to Tartaria_Beta_v1.0.zip" -ForegroundColor Gray
Write-Host "  3. Upload to itch.io or Steam playtest" -ForegroundColor Gray
Write-Host ""
