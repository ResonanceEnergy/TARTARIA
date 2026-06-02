# itch-smoke-test.ps1
# Sprint 7 Lane 9 — End-to-end smoke test for the itch.io build + screenshot pipeline.
#
# What it does:
#   step 1  Capture environment (git SHA, branch, timestamp, Unity path)
#   step 2  Locate Unity executable
#   step 3  Pre-clean Builds/itch_assets/ stale outputs
#   step 4  Invoke Unity in -batchmode WITH a display surface (NO -nographics) to run:
#             a) Tartaria.Editor.Moon1ItchBuild.BuildWin64
#             b) Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode
#   step 5  Validate TARTARIA_Moon1.zip exists, 500MB <= size <= 4GB
#   step 6  Validate shot_00..shot_07 PNGs exist, each <2MB, 1280x720 <= dim <= 1920x1080
#   step 7  Write Builds/itch_assets/build_report.txt with all findings
#   step 8  On Unity failure: dump last 200 lines of Editor.log to docs/build_failures/
#
# Loud logging at every step ([SmokeTest] step N: ...). No silent catches.
#
# Editor entry points (verified at write time):
#   - Tartaria.Editor.Moon1ItchBuild.BuildWin64                          (created Sprint 7 Lane 9)
#   - Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode    (merged from Lane 8)
#
# Usage:
#   .\scripts\dev\itch-smoke-test.ps1
#   .\scripts\dev\itch-smoke-test.ps1 -UnityVersion "6000.3.6f1" -TimeoutSeconds 1800
#   .\scripts\dev\itch-smoke-test.ps1 -SkipBuild     # only screenshot pass (faster iteration)
#   .\scripts\dev\itch-smoke-test.ps1 -SkipCapture   # only build pass
#
# Exit codes:
#   0  every validation passed
#   1+ step-N specific failure (1=env, 2=unity-not-found, 3=preclean, 4=unity-invoke,
#                                5=zip-validation, 6=png-validation, 7=report-write)

param(
    [string]$UnityVersion = "6000.3.6f1",
    [int]$TimeoutSeconds  = 1800,
    [switch]$SkipBuild,
    [switch]$SkipCapture
)

$ErrorActionPreference = "Stop"
$repoRoot       = (Resolve-Path "$PSScriptRoot\..\..").Path
$projectPath    = $repoRoot
$buildsDir      = Join-Path $repoRoot "Builds"
$itchAssetsDir  = Join-Path $buildsDir "itch_assets"
$win64Dir       = Join-Path $buildsDir "Win64"
$reportPath     = Join-Path $itchAssetsDir "build_report.txt"
$editorLog      = Join-Path $repoRoot "Logs\itch_smoke_test.log"
$failuresDir    = Join-Path $repoRoot "docs\build_failures"

# Filled in step 1, referenced by Dump-EditorLogToFailures
$script:gitSha    = ""
$script:gitBranch = ""

# Accumulator for the final report.
$script:report = [System.Collections.Generic.List[string]]::new()

# ----------------------------------------------------------------------------
# Helpers (declared up front — PowerShell parses these before script body runs,
# but listing them at the top keeps the failure paths visible and obvious).
# ----------------------------------------------------------------------------

function Add-Line([string]$line) {
    $script:report.Add($line) | Out-Null
    Write-Host $line
}

function Log-Step([int]$n, [string]$msg) {
    Write-Host "[SmokeTest] step ${n}: $msg" -ForegroundColor Cyan
}

function Try-WriteReport {
    try {
        New-Item -ItemType Directory -Force -Path $itchAssetsDir | Out-Null
        $script:report -join "`r`n" | Out-File -FilePath $reportPath -Encoding utf8 -Force
        Write-Host "[SmokeTest] report written: $reportPath" -ForegroundColor Yellow
    } catch {
        Write-Host "[SmokeTest] could not write report to $reportPath — $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Dump-EditorLogToFailures([string]$tag) {
    try {
        if (-not (Test-Path $editorLog)) {
            Write-Host "[SmokeTest] no Editor log at $editorLog to dump" -ForegroundColor Yellow
            return
        }
        New-Item -ItemType Directory -Force -Path $failuresDir | Out-Null
        $stamp = (Get-Date).ToString("yyyy-MM-dd")
        $failPath = Join-Path $failuresDir "$stamp-itch-smoke-$tag.md"
        $tail = Get-Content $editorLog -Tail 200
        $body = @()
        $body += "# itch.io smoke test failure — $tag"
        $body += ""
        $body += "- date: $((Get-Date).ToString('o'))"
        $body += "- git_branch: $script:gitBranch"
        $body += "- git_sha: $script:gitSha"
        $body += "- unity_log: $editorLog"
        $body += ""
        $body += "## Last 200 lines of Editor.log"
        $body += ""
        $body += '```'
        $body += $tail
        $body += '```'
        $body -join "`r`n" | Out-File -FilePath $failPath -Encoding utf8 -Force
        Add-Line "failure_doc     : $failPath"
        Write-Host "[SmokeTest] failure dump: $failPath" -ForegroundColor Yellow
    } catch {
        Write-Host "[SmokeTest] could not write failure dump — $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Fail-Step([int]$n, [string]$reason) {
    Write-Host "FAIL: step $n — $reason" -ForegroundColor Red
    Add-Line "FAIL: step $n — $reason"
    Try-WriteReport
    exit (1 + $n)
}

# ============================================================================
# step 1 — environment capture
# ============================================================================
Log-Step 1 "capturing environment (git SHA, branch, timestamp)"
try {
    Push-Location $repoRoot
    $script:gitSha    = (git rev-parse HEAD 2>&1).Trim()
    $script:gitBranch = (git rev-parse --abbrev-ref HEAD 2>&1).Trim()
    $gitDirty         = (git status --porcelain 2>&1)
    Pop-Location
    $timestamp = (Get-Date).ToString("o")
    $hostname  = $env:COMPUTERNAME
    Add-Line "=========================================================="
    Add-Line " TARTARIA itch.io build + screenshot smoke test report"
    Add-Line "=========================================================="
    Add-Line "timestamp_local : $timestamp"
    Add-Line "host            : $hostname"
    Add-Line "repo_root       : $repoRoot"
    Add-Line "git_branch      : $script:gitBranch"
    Add-Line "git_sha         : $script:gitSha"
    Add-Line "git_clean       : $([string]::IsNullOrWhiteSpace($gitDirty))"
    Add-Line "skip_build      : $SkipBuild"
    Add-Line "skip_capture    : $SkipCapture"
    Add-Line ""
} catch {
    Fail-Step 1 "environment capture threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ============================================================================
# step 2 — locate Unity executable
# ============================================================================
Log-Step 2 "locating Unity $UnityVersion"
try {
    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe",
        "C:\Program Files\Unity\Editor\Unity.exe",
        "${env:ProgramFiles}\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
    )
    $unityExe = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $unityExe) {
        Add-Line "unity_exe       : NOT FOUND (tried: $($candidates -join '; '))"
        Fail-Step 2 "Unity $UnityVersion not found in any candidate path"
    }
    Add-Line "unity_exe       : $unityExe"
    Add-Line "unity_version   : $UnityVersion"
    Add-Line ""
} catch {
    Fail-Step 2 "Unity locate threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ============================================================================
# step 3 — pre-clean itch_assets so the validation reflects THIS run
# ============================================================================
Log-Step 3 "pre-cleaning $itchAssetsDir of stale shots + zip"
try {
    New-Item -ItemType Directory -Force -Path $itchAssetsDir | Out-Null
    New-Item -ItemType Directory -Force -Path (Split-Path $editorLog) | Out-Null
    Get-ChildItem -Path $itchAssetsDir -Filter "shot_*.png" -ErrorAction SilentlyContinue | Remove-Item -Force
    $staleZip = Join-Path $itchAssetsDir "TARTARIA_Moon1.zip"
    if (Test-Path $staleZip) { Remove-Item -Force $staleZip }
    if (Test-Path $reportPath) { Remove-Item -Force $reportPath }
    Add-Line "preclean        : OK ($itchAssetsDir)"
    Add-Line ""
} catch {
    Fail-Step 3 "preclean threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ============================================================================
# step 4 — invoke Unity in -batchmode (WITH display, NO -nographics)
# ============================================================================
Log-Step 4 "invoking Unity batchmode with build + screenshot chain"
try {
    $methods = @()
    if (-not $SkipBuild)   { $methods += "Tartaria.Editor.Moon1ItchBuild.BuildWin64" }
    if (-not $SkipCapture) { $methods += "Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode" }
    if ($methods.Count -eq 0) {
        Fail-Step 4 "both -SkipBuild and -SkipCapture set — nothing to do"
    }

    # Unity -executeMethod runs ONE method per invocation; chain via repeat launch.
    foreach ($method in $methods) {
        Log-Step 4 "running -executeMethod $method"
        $unityArgs = @(
            "-batchmode",
            "-projectPath", "`"$projectPath`"",
            "-executeMethod", $method,
            "-logFile", "`"$editorLog`"",
            "-quit"
        )
        $startTime = Get-Date
        $proc = Start-Process -FilePath $unityExe -ArgumentList $unityArgs -PassThru -NoNewWindow
        if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
            try { $proc.Kill() } catch { Write-Host "(could not kill Unity: $($_.Exception.Message))" -ForegroundColor Yellow }
            Add-Line "unity_invoke    : TIMEOUT after $TimeoutSeconds s on $method"
            Dump-EditorLogToFailures "timeout-$($method.Replace('.','-'))"
            Fail-Step 4 "Unity timed out on $method"
        }
        $elapsed = [int]((Get-Date) - $startTime).TotalSeconds
        $code = $proc.ExitCode
        Add-Line "unity_method    : $method"
        Add-Line "unity_exitcode  : $code"
        Add-Line "unity_elapsed_s : $elapsed"
        if ($code -ne 0) {
            Add-Line "unity_log_path  : $editorLog"
            Dump-EditorLogToFailures "exit-$code-$($method.Replace('.','-'))"
            Fail-Step 4 "Unity exited $code on $method (see $editorLog and docs/build_failures/)"
        }
    }
    Add-Line ""
} catch {
    Add-Line "unity_invoke    : THREW $($_.Exception.GetType().Name): $($_.Exception.Message)"
    Dump-EditorLogToFailures "exception"
    Fail-Step 4 "Unity invoke threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ============================================================================
# step 5 — validate TARTARIA_Moon1.zip
# ============================================================================
if (-not $SkipBuild) {
    Log-Step 5 "validating TARTARIA_Moon1.zip (500MB <= size <= 4GB)"
    try {
        $zipPath = Join-Path $itchAssetsDir "TARTARIA_Moon1.zip"
        if (-not (Test-Path $zipPath)) {
            Add-Line "zip_path        : MISSING ($zipPath)"
            Fail-Step 5 "TARTARIA_Moon1.zip not found at $zipPath"
        }
        $zipInfo  = Get-Item $zipPath
        $zipBytes = $zipInfo.Length
        $minBytes = 500MB
        $maxBytes = 4GB
        Add-Line "zip_path        : $zipPath"
        Add-Line "zip_size_bytes  : $zipBytes"
        Add-Line "zip_size_mb     : $([math]::Round($zipBytes / 1MB, 1))"
        if ($zipBytes -lt $minBytes) {
            Fail-Step 5 "zip too small ($([math]::Round($zipBytes/1MB,1)) MB < 500 MB) — likely a stub or empty build"
        }
        if ($zipBytes -gt $maxBytes) {
            Fail-Step 5 "zip too large ($([math]::Round($zipBytes/1GB,2)) GB > 4 GB) — itch upload limit risk"
        }
        Add-Line "zip_validation  : OK"
        Add-Line ""
    } catch {
        Fail-Step 5 "zip validation threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
    }
} else {
    Add-Line "zip_validation  : SKIPPED (-SkipBuild)"
    Add-Line ""
}

# ============================================================================
# step 6 — validate shot_00..shot_07 PNGs
# ============================================================================
if (-not $SkipCapture) {
    Log-Step 6 "validating shot_00..shot_07 PNGs (size <2MB, 1280x720..1920x1080)"
    try {
        # Load System.Drawing for dimension probing without Unity.
        Add-Type -AssemblyName System.Drawing -ErrorAction Stop

        $minW = 1280; $maxW = 1920
        $minH = 720;  $maxH = 1080
        $maxSize = 2MB
        $missing = 0
        $tooBig  = 0
        $tooSmall= 0
        $badDim  = 0

        for ($i = 0; $i -le 7; $i++) {
            $idxPrefix = "shot_{0:D2}_" -f $i
            # The capture script writes shot_00_<label>.png — match the prefix.
            $shotMatches = Get-ChildItem -Path $itchAssetsDir -Filter "${idxPrefix}*.png" -ErrorAction SilentlyContinue
            if (-not $shotMatches -or $shotMatches.Count -eq 0) {
                Add-Line ("shot_{0:D2}          : MISSING" -f $i)
                $missing++
                continue
            }
            $shot = $shotMatches | Select-Object -First 1
            $size = $shot.Length
            $w = -1; $h = -1
            $img = $null
            try {
                $img = [System.Drawing.Image]::FromFile($shot.FullName)
                $w = $img.Width
                $h = $img.Height
            } catch {
                Add-Line ("shot_{0:D2}          : DIMENSION READ FAILED — {1}" -f $i, $_.Exception.Message)
                $badDim++
                continue
            } finally {
                if ($null -ne $img) { $img.Dispose() }
            }
            $ok = $true
            $reasons = @()
            if ($size -gt $maxSize) { $ok = $false; $tooBig++;   $reasons += "size=$size>2MB" }
            if ($size -lt 1024)     { $ok = $false; $tooSmall++; $reasons += "size=$size<1KB" }
            if ($w -lt $minW -or $w -gt $maxW -or $h -lt $minH -or $h -gt $maxH) {
                $ok = $false; $badDim++
                $reasons += "dim=${w}x${h} outside [${minW}x${minH}..${maxW}x${maxH}]"
            }
            $verdict = if ($ok) { "OK" } else { "FAIL (" + ($reasons -join "; ") + ")" }
            Add-Line ("shot_{0:D2}          : {1} — {2} bytes, {3}x{4} — {5}" -f $i, $shot.Name, $size, $w, $h, $verdict)
        }
        Add-Line "shot_missing    : $missing"
        Add-Line "shot_oversize   : $tooBig"
        Add-Line "shot_undersize  : $tooSmall"
        Add-Line "shot_bad_dim    : $badDim"
        if ($missing -gt 0 -or $tooBig -gt 0 -or $tooSmall -gt 0 -or $badDim -gt 0) {
            Fail-Step 6 "PNG validation failed (missing=$missing oversize=$tooBig undersize=$tooSmall bad_dim=$badDim)"
        }
        Add-Line "png_validation  : OK (8/8)"
        Add-Line ""
    } catch {
        Fail-Step 6 "PNG validation threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
    }
} else {
    Add-Line "png_validation  : SKIPPED (-SkipCapture)"
    Add-Line ""
}

# ============================================================================
# step 7 — write final report
# ============================================================================
Log-Step 7 "writing final build_report.txt"
try {
    Add-Line "=========================================================="
    Add-Line " smoke test PASSED"
    Add-Line "=========================================================="
    Try-WriteReport
    Write-Host "[SmokeTest] PASS — report at $reportPath" -ForegroundColor Green
    exit 0
} catch {
    Fail-Step 7 "report write threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}
