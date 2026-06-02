# build-itch.ps1
# Sprint 9 Lane 4 - Single-command itch.io build chain.
#
# Pipeline (each step exits 1+N on failure, like the smoke-test wrapper):
#   step 1  Environment capture (git SHA/branch, Unity locate, paths)
#   step 2  Unity batchmode: Tartaria.Editor.Moon1ItchBuild.BuildWin64
#           -> writes Builds/Win64/TARTARIA_Moon1.exe + Builds/itch_assets/TARTARIA_Moon1.zip
#                     + Builds/itch_assets/build_manifest.txt
#   step 3  Unity batchmode: Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode
#           -> writes Builds/itch_assets/shot_00..shot_07_*.png
#   step 4  Locate butler.exe on PATH (or %LOCALAPPDATA%\itch\apps\butler\...).
#           If missing, log the install URL and exit cleanly.
#   step 5  Prepare userversion-file - rewrite Builds/itch_assets/build_manifest.txt so
#           its sole content is a butler-compatible version line. The detailed manifest
#           is preserved at Builds/itch_assets/build_manifest_detail.txt.
#   step 6  Invoke: butler push <zip> <target>:<channel> --userversion-file <manifest>
#
# Loud logging at every step. No silent catches.
#
# Editor entry points (verified at write time):
#   - Tartaria.Editor.Moon1ItchBuild.BuildWin64                       (Sprint 7 Lane 9)
#   - Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode (Sprint 6 Lane 8 / S7 L9)
#
# Usage:
#   .\scripts\build-itch.ps1
#   .\scripts\build-itch.ps1 -ItchTarget "natrix/tartaria-aether-awakening"
#   .\scripts\build-itch.ps1 -Channel "moon1-windows-beta"
#   .\scripts\build-itch.ps1 -DryRun          # everything except the final butler push
#   .\scripts\build-itch.ps1 -SkipBuild       # screenshots + butler only
#   .\scripts\build-itch.ps1 -SkipCapture     # build + butler only
#   .\scripts\build-itch.ps1 -SkipPush        # build + screenshots, no butler
#
# Exit codes (1 + step-number):
#   0   every step passed (or push intentionally skipped)
#   2   step 1 - environment capture
#   3   step 2 - Unity build
#   4   step 3 - Unity screenshot capture
#   5   step 4 - butler not on PATH
#   6   step 5 - userversion file write
#   7   step 6 - butler push failed

param(
    [string]$UnityVersion  = "6000.3.6f1",
    [int]$TimeoutSeconds   = 1800,
    [string]$ItchTarget    = "resonanceenergy/tartaria-aether-awakening",
    [string]$Channel       = "moon1-windows",
    [switch]$DryRun,
    [switch]$SkipBuild,
    [switch]$SkipCapture,
    [switch]$SkipPush
)

$ErrorActionPreference = "Stop"

# --------------------------------------------------------------------------
# Paths
# --------------------------------------------------------------------------
$repoRoot      = (Resolve-Path "$PSScriptRoot\..").Path
$projectPath   = $repoRoot
$buildsDir     = Join-Path $repoRoot "Builds"
$itchAssetsDir = Join-Path $buildsDir "itch_assets"
$win64Dir      = Join-Path $buildsDir "Win64"
$zipPath       = Join-Path $itchAssetsDir "TARTARIA_Moon1.zip"
$manifestPath  = Join-Path $itchAssetsDir "build_manifest.txt"
$manifestDetail= Join-Path $itchAssetsDir "build_manifest_detail.txt"
$editorBuildLog= Join-Path $repoRoot "Logs\itch_build_chain.build.log"
$editorShotLog = Join-Path $repoRoot "Logs\itch_build_chain.capture.log"
$butlerLog     = Join-Path $repoRoot "Logs\itch_build_chain.butler.log"
$failuresDir   = Join-Path $repoRoot "docs\build_failures"

# State filled in step 1
$script:gitSha    = ""
$script:gitBranch = ""

# --------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------
function Log-Step([int]$n, [string]$msg) {
    Write-Host "[BuildItch] step ${n}: $msg" -ForegroundColor Cyan
}

function Log-Info([string]$msg) {
    Write-Host "[BuildItch] $msg" -ForegroundColor Gray
}

function Log-Ok([string]$msg) {
    Write-Host "[BuildItch] OK - $msg" -ForegroundColor Green
}

function Log-Warn([string]$msg) {
    Write-Host "[BuildItch] WARN - $msg" -ForegroundColor Yellow
}

function Dump-EditorLogToFailures([string]$tag, [string]$logPath) {
    try {
        if (-not (Test-Path $logPath)) {
            Write-Host "[BuildItch] no log at $logPath to dump" -ForegroundColor Yellow
            return
        }
        New-Item -ItemType Directory -Force -Path $failuresDir | Out-Null
        $stamp = (Get-Date).ToString("yyyy-MM-dd")
        $failPath = Join-Path $failuresDir "$stamp-build-itch-$tag.md"
        $tail = Get-Content $logPath -Tail 200
        $body = @()
        $body += "# build-itch.ps1 failure - $tag"
        $body += ""
        $body += "- date: $((Get-Date).ToString('o'))"
        $body += "- git_branch: $script:gitBranch"
        $body += "- git_sha: $script:gitSha"
        $body += "- log: $logPath"
        $body += ""
        $body += "## Last 200 lines"
        $body += ""
        $body += '```'
        $body += $tail
        $body += '```'
        $body -join "`r`n" | Out-File -FilePath $failPath -Encoding utf8 -Force
        Write-Host "[BuildItch] failure dump: $failPath" -ForegroundColor Yellow
    } catch {
        Write-Host "[BuildItch] could not write failure dump - $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Fail-Step([int]$n, [string]$reason) {
    Write-Host "FAIL: step $n - $reason" -ForegroundColor Red
    exit (1 + $n)
}

function Find-UnityExe([string]$version) {
    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe",
        "C:\Program Files\Unity\Editor\Unity.exe",
        "${env:ProgramFiles}\Unity\Hub\Editor\$version\Editor\Unity.exe"
    )
    return $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

function Get-ItchApiKey {
    # Sprint 10 Lane 9 - single accessor for the itch.io API key.
    #
    # Lookup order:
    #   1) $env:ITCH_API_KEY                                 (preferred)
    #   2) <repoRoot>\.local-secrets\itch_key.txt            (fallback file - gitignored)
    #   3) neither -> return $null and let butler's own creds cache handle it
    #
    # Returns a hashtable @{ Key = "...."; Source = "env:ITCH_API_KEY" | "file:..." | "none" }.
    # NEVER logs the key value. Logs only the source for auditability.
    #
    # See docs/release/BUTLER_CREDS_SETUP.md for the security policy.

    # (1) env var
    if ($env:ITCH_API_KEY -and $env:ITCH_API_KEY.Trim().Length -gt 0) {
        return @{ Key = $env:ITCH_API_KEY.Trim(); Source = "env:ITCH_API_KEY" }
    }

    # (2) fallback file - same pattern as .local-secrets/github_pat.txt
    $secretFile = Join-Path $repoRoot ".local-secrets\itch_key.txt"
    if (Test-Path $secretFile) {
        try {
            $raw = Get-Content -Path $secretFile -Raw -ErrorAction Stop
            $trimmed = $raw.Trim()
            if ($trimmed.Length -gt 0) {
                return @{ Key = $trimmed; Source = "file:.local-secrets/itch_key.txt" }
            } else {
                Log-Warn "$secretFile exists but is empty - falling through to butler cache"
            }
        } catch {
            # Loud, no silent catch.
            Log-Warn "could not read $secretFile - $($_.Exception.GetType().Name): $($_.Exception.Message)"
        }
    }

    return @{ Key = $null; Source = "none" }
}

function Find-ButlerExe {
    # 1) PATH
    $onPath = Get-Command butler -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }
    # 2) itch app default install location
    $itchCandidates = @(
        "$env:LOCALAPPDATA\itch\apps\butler\butler.exe",
        "$env:USERPROFILE\.itch\apps\butler\butler.exe",
        "$env:ProgramFiles\butler\butler.exe"
    )
    return $itchCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

function Invoke-UnityMethod([string]$unityExe, [string]$method, [string]$logPath, [int]$timeoutSec) {
    Log-Info "executeMethod: $method"
    Log-Info "log:           $logPath"
    New-Item -ItemType Directory -Force -Path (Split-Path $logPath) | Out-Null
    $unityArgs = @(
        "-batchmode",
        "-projectPath", "`"$projectPath`"",
        "-executeMethod", $method,
        "-logFile", "`"$logPath`"",
        "-quit"
    )
    $startTime = Get-Date
    $proc = Start-Process -FilePath $unityExe -ArgumentList $unityArgs -PassThru -NoNewWindow
    if (-not $proc.WaitForExit($timeoutSec * 1000)) {
        try { $proc.Kill() } catch { Write-Host "(could not kill Unity: $($_.Exception.Message))" -ForegroundColor Yellow }
        return @{ ExitCode = -1; ElapsedSec = $timeoutSec; TimedOut = $true }
    }
    $elapsed = [int]((Get-Date) - $startTime).TotalSeconds
    return @{ ExitCode = $proc.ExitCode; ElapsedSec = $elapsed; TimedOut = $false }
}

# ==========================================================================
# step 1 - environment capture
# ==========================================================================
Log-Step 1 "capturing environment + locating Unity"
try {
    Push-Location $repoRoot
    $script:gitSha    = (git rev-parse HEAD 2>&1).Trim()
    $script:gitBranch = (git rev-parse --abbrev-ref HEAD 2>&1).Trim()
    Pop-Location

    Write-Host "=================================================================="
    Write-Host " TARTARIA itch.io build chain (Sprint 9 Lane 4)"
    Write-Host "=================================================================="
    Log-Info "timestamp:    $((Get-Date).ToString('o'))"
    Log-Info "repo_root:    $repoRoot"
    Log-Info "git_branch:   $script:gitBranch"
    Log-Info "git_sha:      $script:gitSha"
    Log-Info "itch_target:  $ItchTarget"
    Log-Info "channel:      $Channel"
    Log-Info "dry_run:      $DryRun"
    Log-Info "skip_build:   $SkipBuild"
    Log-Info "skip_capture: $SkipCapture"
    Log-Info "skip_push:    $SkipPush"

    $unityExe = Find-UnityExe $UnityVersion
    if (-not $unityExe) {
        Fail-Step 1 "Unity $UnityVersion not found on disk"
    }
    Log-Info "unity_exe:    $unityExe"

    New-Item -ItemType Directory -Force -Path $itchAssetsDir | Out-Null
    New-Item -ItemType Directory -Force -Path $win64Dir | Out-Null
    Log-Ok "environment ready"
    Write-Host ""
} catch {
    Fail-Step 1 "environment capture threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ==========================================================================
# step 2 - Unity build
# ==========================================================================
if ($SkipBuild) {
    Log-Step 2 "SKIPPED (-SkipBuild)"
    Write-Host ""
} else {
    Log-Step 2 "Unity build (Moon1ItchBuild.BuildWin64)"
    try {
        $result = Invoke-UnityMethod $unityExe "Tartaria.Editor.Moon1ItchBuild.BuildWin64" $editorBuildLog $TimeoutSeconds
        Log-Info "exit_code:    $($result.ExitCode)"
        Log-Info "elapsed_s:    $($result.ElapsedSec)"
        if ($result.TimedOut) {
            Dump-EditorLogToFailures "build-timeout" $editorBuildLog
            Fail-Step 2 "Unity build timed out after $TimeoutSeconds s"
        }
        if ($result.ExitCode -ne 0) {
            Dump-EditorLogToFailures "build-exit-$($result.ExitCode)" $editorBuildLog
            Fail-Step 2 "Unity build exited $($result.ExitCode) (see $editorBuildLog and docs/build_failures/)"
        }
        if (-not (Test-Path $zipPath)) {
            Dump-EditorLogToFailures "build-no-zip" $editorBuildLog
            Fail-Step 2 "Unity build reported success but $zipPath is missing"
        }
        $zipBytes = (Get-Item $zipPath).Length
        Log-Ok "build complete - zip $([math]::Round($zipBytes / 1MB, 1)) MB at $zipPath"
        Write-Host ""
    } catch {
        Dump-EditorLogToFailures "build-exception" $editorBuildLog
        Fail-Step 2 "Unity build threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
    }
}

# ==========================================================================
# step 3 - screenshot capture
# ==========================================================================
if ($SkipCapture) {
    Log-Step 3 "SKIPPED (-SkipCapture)"
    Write-Host ""
} else {
    Log-Step 3 "screenshot capture (Moon1ItchScreenshotCapture.CaptureFromBatchmode)"
    try {
        $result = Invoke-UnityMethod $unityExe "Tartaria.Editor.Moon1ItchScreenshotCapture.CaptureFromBatchmode" $editorShotLog $TimeoutSeconds
        Log-Info "exit_code:    $($result.ExitCode)"
        Log-Info "elapsed_s:    $($result.ElapsedSec)"
        if ($result.TimedOut) {
            Dump-EditorLogToFailures "capture-timeout" $editorShotLog
            Fail-Step 3 "Unity screenshot capture timed out after $TimeoutSeconds s"
        }
        if ($result.ExitCode -ne 0) {
            Dump-EditorLogToFailures "capture-exit-$($result.ExitCode)" $editorShotLog
            Fail-Step 3 "Unity capture exited $($result.ExitCode) (see $editorShotLog)"
        }
        $shots = Get-ChildItem -Path $itchAssetsDir -Filter "shot_*.png" -ErrorAction SilentlyContinue
        if (-not $shots -or $shots.Count -eq 0) {
            Dump-EditorLogToFailures "capture-no-pngs" $editorShotLog
            Fail-Step 3 "Unity capture reported success but no shot_*.png files in $itchAssetsDir"
        }
        Log-Ok "capture complete - $($shots.Count) PNGs in $itchAssetsDir"
        Write-Host ""
    } catch {
        Dump-EditorLogToFailures "capture-exception" $editorShotLog
        Fail-Step 3 "Unity capture threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
    }
}

# ==========================================================================
# step 4 - locate butler
# ==========================================================================
Log-Step 4 "locating butler.exe"
$butlerExe = $null
try {
    $butlerExe = Find-ButlerExe
    if (-not $butlerExe) {
        Write-Host ""
        Write-Host "ERROR: butler.exe not found on PATH or in standard install locations." -ForegroundColor Red
        Write-Host "       Install butler from https://itch.io/docs/butler/installing.html" -ForegroundColor Red
        Write-Host "       Then run 'butler login' once to authenticate this machine." -ForegroundColor Red
        Write-Host "       See docs/release/BUTLER_SETUP.md for full setup notes." -ForegroundColor Red
        Write-Host ""
        if ($SkipPush) {
            Log-Warn "butler missing but -SkipPush set - continuing anyway"
        } else {
            Fail-Step 4 "butler.exe missing - see error above"
        }
    } else {
        Log-Ok "butler at $butlerExe"

        # Sprint 10 Lane 9 - load ITCH_API_KEY before invoking butler.
        # All credential reads go through Get-ItchApiKey for auditability.
        $creds = Get-ItchApiKey
        if ($creds.Key) {
            $env:BUTLER_API_KEY = $creds.Key
            Log-Ok "Loaded ITCH_API_KEY from $($creds.Source)"
            Log-Info "(exported to `$env:BUTLER_API_KEY for child butler process - value never logged)"
        } else {
            if ($SkipPush -or $DryRun) {
                Log-Warn "No ITCH_API_KEY in env or .local-secrets - relying on butler login cache (push is $(if($SkipPush){'skipped'}else{'dry-run'}), proceeding)"
            } else {
                Write-Host ""
                Write-Host "ERROR: no ITCH_API_KEY found in env or .local-secrets/itch_key.txt." -ForegroundColor Red
                Write-Host "       Either run 'butler login' (interactive, browser-based)," -ForegroundColor Red
                Write-Host "       set the ITCH_API_KEY env var, or drop a key file at" -ForegroundColor Red
                Write-Host "       <repo>\.local-secrets\itch_key.txt." -ForegroundColor Red
                Write-Host "       Full instructions: docs/release/BUTLER_CREDS_SETUP.md" -ForegroundColor Red
                Write-Host ""
                Fail-Step 4 "no ITCH_API_KEY available - see docs/release/BUTLER_CREDS_SETUP.md"
            }
        }

        # Log butler version for the report
        try {
            $ver = & $butlerExe -V 2>&1
            Log-Info "butler_version: $ver"
        } catch {
            Log-Warn "could not query butler version: $($_.Exception.Message)"
        }
    }
    Write-Host ""
} catch {
    Fail-Step 4 "butler locate threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ==========================================================================
# step 5 - prepare userversion-file
# ==========================================================================
if ($SkipPush -or $DryRun) {
    Log-Step 5 "userversion-file prep"
} else {
    Log-Step 5 "preparing userversion-file at $manifestPath"
}
try {
    if (-not (Test-Path $manifestPath)) {
        if ($SkipBuild) {
            Log-Warn "build_manifest.txt missing (probably because -SkipBuild was set) - butler push will fail"
        } else {
            Fail-Step 5 "build_manifest.txt missing at $manifestPath after Unity build"
        }
    } else {
        # Preserve the detailed manifest before we overwrite the file with a
        # butler-friendly version string. butler --userversion-file reads the
        # entire file as the version, so it must contain ONLY a version line.
        Copy-Item -Path $manifestPath -Destination $manifestDetail -Force
        $shortSha = if ($script:gitSha) { $script:gitSha.Substring(0, [Math]::Min(8, $script:gitSha.Length)) } else { "nogit" }
        $stamp = (Get-Date).ToString("yyyyMMdd-HHmm")
        $version = "0.4.0-moon1-$stamp-$shortSha"
        $version | Out-File -FilePath $manifestPath -Encoding ascii -NoNewline -Force
        Log-Ok "version: $version"
        Log-Info "detail preserved at: $manifestDetail"
    }
    Write-Host ""
} catch {
    Fail-Step 5 "userversion prep threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}

# ==========================================================================
# step 6 - butler push
# ==========================================================================
if ($SkipPush) {
    Log-Step 6 "SKIPPED (-SkipPush) - chain complete without upload"
    Write-Host ""
    Write-Host "=================================================================="
    Write-Host " build chain PASSED (push skipped)" -ForegroundColor Green
    Write-Host "=================================================================="
    exit 0
}

if ($DryRun) {
    Log-Step 6 "DRY RUN - would invoke:"
    Write-Host "  $butlerExe push `"$zipPath`" `"${ItchTarget}:${Channel}`" --userversion-file `"$manifestPath`"" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "=================================================================="
    Write-Host " dry run complete - nothing pushed" -ForegroundColor Green
    Write-Host "=================================================================="
    exit 0
}

Log-Step 6 "butler push to ${ItchTarget}:${Channel}"
try {
    if (-not $butlerExe) {
        Fail-Step 6 "butler.exe not located in step 4 - cannot push"
    }
    if (-not (Test-Path $zipPath)) {
        Fail-Step 6 "zip missing at $zipPath - cannot push (run without -SkipBuild)"
    }
    if (-not (Test-Path $manifestPath)) {
        Fail-Step 6 "userversion file missing at $manifestPath - cannot push"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path $butlerLog) | Out-Null
    $pushArgs = @(
        "push",
        "`"$zipPath`"",
        "`"${ItchTarget}:${Channel}`"",
        "--userversion-file", "`"$manifestPath`""
    )
    Log-Info "command: $butlerExe $($pushArgs -join ' ')"
    Log-Info "log:     $butlerLog"

    # Stream butler output to console AND log
    $startTime = Get-Date
    $proc = Start-Process -FilePath $butlerExe -ArgumentList $pushArgs -PassThru -NoNewWindow `
        -RedirectStandardOutput $butlerLog -RedirectStandardError "$butlerLog.err"
    if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
        try { $proc.Kill() } catch { Write-Host "(could not kill butler: $($_.Exception.Message))" -ForegroundColor Yellow }
        Dump-EditorLogToFailures "butler-timeout" $butlerLog
        Fail-Step 6 "butler push timed out after $TimeoutSeconds s"
    }
    $elapsed = [int]((Get-Date) - $startTime).TotalSeconds
    $code = $proc.ExitCode

    # Surface butler's output
    if (Test-Path $butlerLog) {
        Get-Content $butlerLog | ForEach-Object { Write-Host "  $_" }
    }
    if (Test-Path "$butlerLog.err") {
        $errContent = Get-Content "$butlerLog.err"
        if ($errContent) {
            Write-Host "  --- stderr ---" -ForegroundColor Yellow
            $errContent | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        }
    }

    Log-Info "exit_code: $code"
    Log-Info "elapsed_s: $elapsed"

    if ($code -ne 0) {
        Dump-EditorLogToFailures "butler-exit-$code" $butlerLog
        Fail-Step 6 "butler push exited $code (see $butlerLog)"
    }

    Log-Ok "butler push succeeded"
    Write-Host ""
    Write-Host "=================================================================="
    Write-Host " itch.io build chain PASSED" -ForegroundColor Green
    Write-Host "=================================================================="
    Write-Host " uploaded: $zipPath"
    Write-Host " target:   ${ItchTarget}:${Channel}"
    Write-Host " verify:   $butlerExe status $ItchTarget"
    exit 0
} catch {
    Dump-EditorLogToFailures "butler-exception" $butlerLog
    Fail-Step 6 "butler push threw $($_.Exception.GetType().Name): $($_.Exception.Message)"
}
