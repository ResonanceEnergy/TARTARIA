<#
.SYNOPSIS
    TARTARIA -- One-click Build + Validate + Play launcher.

.DESCRIPTION
    Automates the full pipeline:
      1. (Optional) Batch-mode BUILD EVERYTHING + Validate (headless, fast)
      2. Drop a sentinel file so Unity GUI auto-triggers Play mode on open
      3. Launch Unity GUI with the project

    Modes:
      .\tartaria-play.ps1              -- Sentinel + open Unity (build inside GUI)
      .\tartaria-play.ps1 -BatchFirst  -- Headless build+validate first, then GUI play
      .\tartaria-play.ps1 -BatchOnly   -- Headless build+validate only, no GUI

.PARAMETER BatchFirst
    Run headless build+validate before opening Unity GUI.

.PARAMETER BatchOnly
    Run headless build+validate and exit. Do not open Unity GUI.
#>
[CmdletBinding()]
param(
    [switch]$BatchFirst,
    [switch]$BatchOnly
)

$ErrorActionPreference = "Stop"

# ── Configuration ──
$ProjectPath = "C:\dev\TARTARIA_new"
$UnityEditor = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$LogFile     = Join-Path $ProjectPath "Logs\tartaria-build.log"

# ── Verify paths ──
if (-not (Test-Path $UnityEditor)) {
    Write-Host "ERROR: Unity editor not found at $UnityEditor" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $ProjectPath)) {
    Write-Host "ERROR: Project not found at $ProjectPath" -ForegroundColor Red
    exit 1
}

# ── Kill any running Unity instance for this project ──
$unityProcs = Get-Process -Name Unity -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }
if ($unityProcs) {
    Write-Host "Closing existing Unity instance for TARTARIA_new..." -ForegroundColor Yellow
    $unityProcs | ForEach-Object { $_.CloseMainWindow() | Out-Null }
    Start-Sleep -Seconds 3
    # Force-kill if still running
    $unityProcs = Get-Process -Name Unity -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }
    if ($unityProcs) {
        $unityProcs | Stop-Process -Force
        Start-Sleep -Seconds 2
    }
}

# ── Function: Run batch mode ──
function Invoke-BatchMode {
    param([string]$Method, [string]$Description)

    Write-Host ""
    Write-Host "== $Description ==" -ForegroundColor Cyan
    Write-Host "  Method: $Method"
    Write-Host "  Log:    $LogFile"
    Write-Host ""

    $args = @(
        "-batchmode",
        "-projectPath", $ProjectPath,
        "-executeMethod", $Method,
        "-logFile", $LogFile,
        "-quit"
    )

    $proc = Start-Process -FilePath $UnityEditor -ArgumentList $args -NoNewWindow -PassThru -Wait
    $exitCode = $proc.ExitCode

    # Show last 30 lines of log for context
    if (Test-Path $LogFile) {
        Write-Host "-- Last 30 lines of Unity log --" -ForegroundColor DarkGray
        Get-Content $LogFile -Tail 30 | ForEach-Object {
            if ($_ -match "\[Tartaria\]") {
                if ($_ -match "FAIL") {
                    Write-Host "  $_" -ForegroundColor Red
                } elseif ($_ -match "OK|PASSED|complete") {
                    Write-Host "  $_" -ForegroundColor Green
                } else {
                    Write-Host "  $_" -ForegroundColor Cyan
                }
            }
        }
        Write-Host "-- End log --" -ForegroundColor DarkGray
    }

    if ($exitCode -ne 0) {
        Write-Host ""
        Write-Host "FAILED with exit code $exitCode" -ForegroundColor Red
        Write-Host "Full log: $LogFile" -ForegroundColor Yellow
        return $false
    }

    Write-Host "  PASSED" -ForegroundColor Green
    return $true
}

# ── Batch-mode build + validate ──
if ($BatchFirst -or $BatchOnly) {
    # Step 1: Build
    $buildOK = Invoke-BatchMode "Tartaria.Editor.OneClickBuild.RunBuild" "BUILD EVERYTHING (headless)"
    if (-not $buildOK) {
        Write-Host "Build failed -- aborting." -ForegroundColor Red
        exit 1
    }

    # Step 2: Validate
    $validOK = Invoke-BatchMode "Tartaria.Editor.BatchReadinessValidator.Validate" "READINESS VALIDATION (headless)"
    if (-not $validOK) {
        Write-Host "Validation failed -- check log for details." -ForegroundColor Red
        Write-Host "Launching Unity GUI anyway so you can inspect..." -ForegroundColor Yellow
    }

    if ($BatchOnly) {
        Write-Host ""
        if ($validOK) {
            Write-Host "All checks passed. Ready to play." -ForegroundColor Green
        }
        exit ($validOK ? 0 : 1)
    }
}

# ── Drop sentinel file for AutoPlayBoot ──
$sentinelPath = Join-Path $ProjectPath "Temp\TARTARIA_AUTOPLAY"
$tempDir = Join-Path $ProjectPath "Temp"
if (-not (Test-Path $tempDir)) { New-Item -ItemType Directory -Path $tempDir -Force | Out-Null }
"autoplay" | Set-Content -Path $sentinelPath -NoNewline
Write-Host ""
Write-Host "Sentinel file created -- Unity will auto-build + enter Play mode" -ForegroundColor Green

# ── Launch Unity GUI ──
Write-Host "Launching Unity..." -ForegroundColor Cyan
$guiArgs = @("-projectPath", $ProjectPath)
Start-Process -FilePath $UnityEditor -ArgumentList $guiArgs
Write-Host ""
Write-Host "Unity is opening TARTARIA. On load it will:" -ForegroundColor White
Write-Host "  1. BUILD EVERYTHING (create scenes, prefabs, SOs, managers)" -ForegroundColor White
Write-Host "  2. Run Readiness Validation" -ForegroundColor White
Write-Host "  3. Open Boot scene and enter Play mode" -ForegroundColor White
Write-Host ""
Write-Host "Done. Watch the Unity console for progress." -ForegroundColor Green
