# capture-itch-screenshots.ps1
# Sprint 6 Lane 8 — Marketing screenshot pipeline for the itch.io page.
#
# Drives the Tartaria/Marketing/Capture itch Screenshots editor action via -executeMethod.
# Output:  Builds/itch_assets/shot_00_*.png ... shot_07_*.png  (Game-view resolution)
#
# IMPORTANT: ScreenCapture.CaptureScreenshot writes the Game view, which requires a
# display surface. Headless capture (-batchmode -nographics) will NOT produce PNGs.
# This wrapper therefore launches Unity WITH a display (no -nographics) and lets the
# Editor render frames for the camera walk.
#
# Editor logic lives in:  Assets/_Project/Scripts/Editor/Moon1ItchScreenshotCapture.cs
#
# Usage:
#   .\scripts\dev\capture-itch-screenshots.ps1
#   .\scripts\dev\capture-itch-screenshots.ps1 -UnityVersion "6000.3.6f1" -TimeoutSeconds 600
#   .\scripts\dev\capture-itch-screenshots.ps1 -NoExit   # keep Unity open after capture for review
#
# Exit codes:
#   0  shots written
#   1  Unity exited non-zero (capture errors — see Unity log)
#   2  Unity executable not found
#   3  output directory empty after Unity exit
#   124 Unity timed out

param(
    [string]$UnityVersion = "6000.3.6f1",
    [int]$TimeoutSeconds = 600,
    [switch]$NoExit
)

$ErrorActionPreference = "Stop"
$repoRoot    = (Resolve-Path "$PSScriptRoot\..\..").Path
$projectPath = $repoRoot
$outputDir   = Join-Path $repoRoot "Builds\itch_assets"
$unityLog    = Join-Path $repoRoot "Logs\itch_screenshot_capture.log"

# Find Unity executable
$candidates = @(
    "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe",
    "C:\Program Files\Unity\Editor\Unity.exe",
    "${env:ProgramFiles}\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
)
$unityExe = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $unityExe) {
    Write-Host "ERROR: Unity $UnityVersion not found. Tried:" -ForegroundColor Red
    $candidates | ForEach-Object { Write-Host "  $_" }
    Write-Host "Set -UnityVersion to your installed Unity 6 version, or edit the candidates list."
    exit 2
}

Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "  ITCH.IO MARKETING SCREENSHOT CAPTURE (Moon 1)" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "Unity:     $unityExe"
Write-Host "Project:   $projectPath"
Write-Host "Output:    $outputDir"
Write-Host "Unity log: $unityLog"
Write-Host ""
Write-Host "NOTE: This launches Unity WITH a display (no -nographics)."
Write-Host "      ScreenCapture.CaptureScreenshot writes the Game view, which requires"
Write-Host "      a render surface. Headless mode produces no PNGs."
Write-Host ""

# Ensure dirs
New-Item -ItemType Directory -Force -Path $outputDir         | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $unityLog) | Out-Null

# Stale-PNG cleanup so we know this run wrote the files
Get-ChildItem -Path $outputDir -Filter "shot_*.png" -ErrorAction SilentlyContinue | Remove-Item -Force

# Compose Unity arguments. NOT using -nographics — see header note.
$unityArgs = @(
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode",
    "-logFile", "`"$unityLog`""
)
if (-not $NoExit) { $unityArgs += "-quit" }

Write-Host "Launching Unity..." -ForegroundColor Yellow
$startTime = Get-Date
$proc = Start-Process -FilePath $unityExe -ArgumentList $unityArgs -PassThru -NoNewWindow

if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
    Write-Host "TIMEOUT: Unity did not exit within $TimeoutSeconds seconds." -ForegroundColor Red
    try { $proc.Kill() } catch { Write-Host "  (could not kill Unity process: $($_.Exception.Message))" -ForegroundColor Yellow }
    exit 124
}
$elapsed = [int]((Get-Date) - $startTime).TotalSeconds
Write-Host "Unity exited (code $($proc.ExitCode)) after ${elapsed}s." -ForegroundColor Yellow
Write-Host ""

# Check output
$shots = Get-ChildItem -Path $outputDir -Filter "shot_*.png" -ErrorAction SilentlyContinue
if ($null -eq $shots -or $shots.Count -eq 0) {
    Write-Host "FAIL: no PNGs in $outputDir after capture." -ForegroundColor Red
    Write-Host ""
    if (Test-Path $unityLog) {
        Write-Host "Last 40 lines of Unity log:" -ForegroundColor Yellow
        Get-Content $unityLog -Tail 40
    }
    exit 3
}

Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "  SHOTS WRITTEN ($($shots.Count))" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan
foreach ($s in $shots | Sort-Object Name) {
    $sizeKB = [math]::Round($s.Length / 1024, 1)
    Write-Host ("  {0,-48} {1,8} KB" -f $s.Name, $sizeKB)
}
Write-Host ""
Write-Host "Output dir: $outputDir"
Write-Host "Unity log:  $unityLog"
Write-Host ""

exit $proc.ExitCode
