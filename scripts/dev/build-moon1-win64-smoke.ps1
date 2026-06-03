# build-moon1-win64-smoke.ps1
# Phase 7.3 Hammer Lane 10 — Win64 build smoke runner.
#
# Purpose:
#   Validate that BuildPipeline.BuildPlayer succeeds end-to-end on the
#   post-Phase 0-5 codebase by invoking
#   Tartaria.Editor.Moon1ItchBuild.BuildWin64 in Unity batchmode and
#   confirming the resulting EXE and zip exist with sane sizes.
#
#   Scope is intentionally narrower than scripts/dev/itch-smoke-test.ps1:
#   THIS script only does the BUILD validation (no screenshot capture),
#   so it can be run as a fast CI gate before the heavier itch pipeline.
#
# Editor entry points (grep-verified at write time):
#   - Tartaria.Editor.Moon1ItchBuild.BuildWin64
#       Assets/_Project/Scripts/Editor/Moon1ItchBuild.cs:49
#       Calls BuildPipeline.BuildPlayer with:
#         scenes = [ Boot.unity, Echohaven_VerticalSlice.unity ]
#         target = StandaloneWindows64
#         locationPathName = Builds/Win64/TARTARIA_Moon1.exe
#       Writes Builds/itch_assets/TARTARIA_Moon1.zip and build_manifest.txt.
#       Returns exit code 0 on success; 2/3/4/9 on various failures.
#
# Scenes in build (cross-checked against ProjectSettings/EditorBuildSettings.asset):
#   - Assets/_Project/Scenes/Boot.unity                          (enabled, guid e239cbf8...)
#   - Assets/_Project/Scenes/Echohaven_VerticalSlice.unity       (enabled, guid be7de6ea...)
#   Note: Moon1ItchBuild.cs explicitly picks ONLY these two scenes by file
#   existence check, ignoring the 12 Moon2-13 entries in EditorBuildSettings.
#   This is intentional for Moon 1 ship.
#
# Pre-flight guard:
#   Unity locks Library/ when an Editor is open against the project root.
#   This script REFUSES to run if Unity.exe is already running with the same
#   -projectPath — exit code 2.
#
# Usage:
#   .\scripts\dev\build-moon1-win64-smoke.ps1
#   .\scripts\dev\build-moon1-win64-smoke.ps1 -UnityVersion "6000.3.6f1" -TimeoutSeconds 1800
#   .\scripts\dev\build-moon1-win64-smoke.ps1 -DryRun                # validate setup, do not invoke Unity
#
# Exit codes:
#   0  build succeeded, exe + zip present, sizes within bounds
#   1  environment capture failed
#   2  Unity is currently running against the project — refuse
#   3  Unity executable not found at -UnityVersion path
#   4  Unity batchmode invocation failed (timeout or non-zero exit)
#   5  exe missing after successful build
#   6  zip missing or out-of-bounds size

param(
    [string]$UnityVersion  = "6000.3.6f1",
    [int]   $TimeoutSeconds = 1800,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot      = (Resolve-Path "$PSScriptRoot\..\..").Path
$projectPath   = $repoRoot
$buildsDir     = Join-Path $repoRoot "Builds"
$win64Dir      = Join-Path $buildsDir "Win64"
$itchAssetsDir = Join-Path $buildsDir "itch_assets"
$exePath       = Join-Path $win64Dir      "TARTARIA_Moon1.exe"
$zipPath       = Join-Path $itchAssetsDir "TARTARIA_Moon1.zip"
$manifestPath  = Join-Path $itchAssetsDir "build_manifest.txt"
$editorLog     = Join-Path $repoRoot "Logs\win64_smoke.log"
$readinessDoc  = Join-Path $repoRoot "docs\release\WIN64_BUILD_SMOKE.md"

function Log-Step([int]$n, [string]$msg) {
    Write-Host "[Win64Smoke] step ${n}: $msg" -ForegroundColor Cyan
}
function Fail-Step([int]$code, [string]$reason) {
    Write-Host "FAIL: exit $code — $reason" -ForegroundColor Red
    exit $code
}

# ----------------------------------------------------------------------------
# step 1 — environment capture
# ----------------------------------------------------------------------------
Log-Step 1 "capturing environment"
try {
    Push-Location $repoRoot
    $gitSha    = (git rev-parse HEAD 2>&1).Trim()
    $gitBranch = (git rev-parse --abbrev-ref HEAD 2>&1).Trim()
    Pop-Location
    Write-Host "  repo_root  : $repoRoot"
    Write-Host "  git_branch : $gitBranch"
    Write-Host "  git_sha    : $gitSha"
    Write-Host "  unity_ver  : $UnityVersion"
    Write-Host "  timeout_s  : $TimeoutSeconds"
    Write-Host "  dry_run    : $DryRun"
} catch {
    Fail-Step 1 "environment capture threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ----------------------------------------------------------------------------
# step 2 — Unity-running guard (Library/ lock check)
# ----------------------------------------------------------------------------
Log-Step 2 "checking whether Unity is already open against this project"
try {
    # Two signals: a Unity.exe process whose command line references this path,
    # OR a Library/UnityLockfile sitting on disk.
    $lockFile = Join-Path $repoRoot "Library\UnityLockfile"
    $lockPresent = Test-Path $lockFile
    $runningSelf = $false

    $procs = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue
    if ($procs) {
        foreach ($p in $procs) {
            $cl = $p.CommandLine
            if ($cl -and $cl -match [Regex]::Escape($repoRoot)) {
                $runningSelf = $true
                Write-Host "  PID $($p.ProcessId) -> $cl" -ForegroundColor Yellow
            }
        }
    }

    if ($runningSelf) {
        Fail-Step 2 "Unity.exe is already running against $repoRoot. Close the Editor and re-run."
    }
    if ($lockPresent) {
        Write-Host "  WARNING: Library\UnityLockfile present but no running Unity matched — possibly stale." -ForegroundColor Yellow
        Write-Host "           If batchmode invocation errors with 'multiple Unity instances', delete the lockfile and retry." -ForegroundColor Yellow
    }
    Write-Host "  Library lock OK — proceeding."
} catch {
    Fail-Step 2 "Unity-running guard threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ----------------------------------------------------------------------------
# step 3 — locate Unity.exe
# ----------------------------------------------------------------------------
Log-Step 3 "locating Unity $UnityVersion"
$candidates = @(
    "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe",
    "C:\Program Files\Unity\Editor\Unity.exe",
    "${env:ProgramFiles}\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
)
$unityExe = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $unityExe) {
    Fail-Step 3 "Unity $UnityVersion not found. Tried: $($candidates -join '; ')"
}
Write-Host "  unity_exe  : $unityExe"

if ($DryRun) {
    Write-Host ""
    Write-Host "[Win64Smoke] DRY-RUN complete — environment validated, Unity NOT invoked." -ForegroundColor Green
    Write-Host "             To execute the actual build, re-run without -DryRun (Unity must be closed)." -ForegroundColor Green
    exit 0
}

# ----------------------------------------------------------------------------
# step 4 — pre-clean stale build artifacts so size checks reflect this run
# ----------------------------------------------------------------------------
Log-Step 4 "pre-cleaning stale build outputs"
try {
    New-Item -ItemType Directory -Force -Path $win64Dir      | Out-Null
    New-Item -ItemType Directory -Force -Path $itchAssetsDir | Out-Null
    New-Item -ItemType Directory -Force -Path (Split-Path $editorLog) | Out-Null
    if (Test-Path $exePath)      { Remove-Item -Force -Recurse $win64Dir; New-Item -ItemType Directory -Force -Path $win64Dir | Out-Null }
    if (Test-Path $zipPath)      { Remove-Item -Force $zipPath }
    if (Test-Path $manifestPath) { Remove-Item -Force $manifestPath }
    if (Test-Path $editorLog)    { Remove-Item -Force $editorLog }
    Write-Host "  preclean OK"
} catch {
    Fail-Step 4 "preclean threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ----------------------------------------------------------------------------
# step 5 — invoke Unity batchmode -nographics -executeMethod
# ----------------------------------------------------------------------------
Log-Step 5 "invoking Unity -batchmode -nographics Moon1ItchBuild.BuildWin64"
try {
    $unityArgs = @(
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$projectPath`"",
        "-executeMethod", "Tartaria.Editor.Moon1ItchBuild.BuildWin64",
        "-logFile", "`"$editorLog`"",
        "-quit"
    )
    Write-Host "  cmd: `"$unityExe`" $($unityArgs -join ' ')"
    $startTime = Get-Date
    $proc = Start-Process -FilePath $unityExe -ArgumentList $unityArgs -PassThru -NoNewWindow
    if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
        try { $proc.Kill() } catch { Write-Host "  (could not kill Unity: $($_.Exception.Message))" -ForegroundColor Yellow }
        Write-Host "  --- last 100 lines of $editorLog ---" -ForegroundColor Yellow
        if (Test-Path $editorLog) { Get-Content $editorLog -Tail 100 | Write-Host }
        Fail-Step 4 "Unity timed out after $TimeoutSeconds s"
    }
    $elapsed = [int]((Get-Date) - $startTime).TotalSeconds
    $code = $proc.ExitCode
    Write-Host "  unity_exit : $code"
    Write-Host "  elapsed_s  : $elapsed"
    if ($code -ne 0) {
        Write-Host "  --- last 100 lines of $editorLog ---" -ForegroundColor Yellow
        if (Test-Path $editorLog) { Get-Content $editorLog -Tail 100 | Write-Host }
        Fail-Step 4 "Unity exited $code (see $editorLog)"
    }
} catch {
    Fail-Step 4 "Unity invoke threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ----------------------------------------------------------------------------
# step 6 — verify EXE artifact
# ----------------------------------------------------------------------------
Log-Step 6 "verifying $exePath exists"
if (-not (Test-Path $exePath)) {
    Fail-Step 5 "EXE missing after successful build: $exePath"
}
$exeInfo = Get-Item $exePath
Write-Host "  exe_size_bytes : $($exeInfo.Length)"
Write-Host "  exe_size_mb    : $([math]::Round($exeInfo.Length / 1MB, 1))"

# ----------------------------------------------------------------------------
# step 7 — verify ZIP artifact (Moon1ItchBuild zips Builds/Win64 -> itch_assets/TARTARIA_Moon1.zip)
# ----------------------------------------------------------------------------
Log-Step 7 "verifying $zipPath exists and size is sane"
if (-not (Test-Path $zipPath)) {
    Fail-Step 6 "Zip missing: $zipPath"
}
$zipInfo  = Get-Item $zipPath
$zipBytes = $zipInfo.Length
$minBytes = 50MB    # smoke-test lower bound; itch validation enforces 500MB
$maxBytes = 4GB
Write-Host "  zip_size_bytes : $zipBytes"
Write-Host "  zip_size_mb    : $([math]::Round($zipBytes / 1MB, 1))"
if ($zipBytes -lt $minBytes) {
    Fail-Step 6 "Zip too small ($([math]::Round($zipBytes/1MB,1)) MB < 50 MB) — likely stub build"
}
if ($zipBytes -gt $maxBytes) {
    Fail-Step 6 "Zip too large ($([math]::Round($zipBytes/1GB,2)) GB > 4 GB) — itch upload limit risk"
}

# ----------------------------------------------------------------------------
# done
# ----------------------------------------------------------------------------
Write-Host ""
Write-Host "[Win64Smoke] PASS — Win64 build smoke succeeded." -ForegroundColor Green
Write-Host "  exe       : $exePath"
Write-Host "  zip       : $zipPath"
Write-Host "  manifest  : $manifestPath"
Write-Host "  log       : $editorLog"
if (Test-Path $manifestPath) {
    Write-Host "  --- manifest ---"
    Get-Content $manifestPath | Write-Host
}
exit 0
