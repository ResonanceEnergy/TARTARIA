<#
.SYNOPSIS
    TARTARIA -- One-click Build + Validate + Play launcher with automated error handling.

.DESCRIPTION
    Automates the full pipeline:
      1. (Optional) Batch-mode BUILD EVERYTHING + Validate (headless, fast)
      2. Drop a sentinel file so Unity GUI auto-triggers Play mode on open
      3. Launch Unity GUI with the project
      4. Monitor Editor.log for pipeline completion or errors
      5. Parse and display the build report

    Modes:
      .\tartaria-play.ps1              -- Sentinel + open Unity + monitor (default)
      .\tartaria-play.ps1 -BatchFirst  -- Headless build+validate first, then GUI play
      .\tartaria-play.ps1 -BatchOnly   -- Headless build+validate only, no GUI
      .\tartaria-play.ps1 -NoMonitor   -- Sentinel + open Unity, skip log monitoring

.PARAMETER BatchFirst
    Run headless build+validate before opening Unity GUI.

.PARAMETER BatchOnly
    Run headless build+validate and exit. Do not open Unity GUI.

.PARAMETER NoMonitor
    Skip post-launch log monitoring. Just drop sentinel and open Unity.
#>
[CmdletBinding()]
param(
    [switch]$BatchFirst,
    [switch]$BatchOnly,
    [switch]$NoMonitor
)

$ErrorActionPreference = "Stop"

# ── Configuration ──
$ProjectPath    = "C:\dev\TARTARIA_new"
$UnityEditor    = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$LogFile        = Join-Path $ProjectPath "Logs\tartaria-build.log"
$EditorLog      = Join-Path $env:LOCALAPPDATA "Unity\Editor\Editor.log"
$BuildReport    = Join-Path $ProjectPath "Logs\tartaria-build-report.txt"
$MonitorTimeout = 300  # 5 minutes max wait for pipeline

# ── Banner ──
Write-Host ""
Write-Host "  ============================================" -ForegroundColor DarkCyan
Write-Host "   TARTARIA -- Automated Build Pipeline" -ForegroundColor Cyan
Write-Host "  ============================================" -ForegroundColor DarkCyan
Write-Host ""

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
function Close-UnityForProject {
    $unityProcs = Get-Process -Name Unity -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }
    if ($unityProcs) {
        Write-Host "Closing existing Unity instance..." -ForegroundColor Yellow
        $unityProcs | ForEach-Object { $_.CloseMainWindow() | Out-Null }
        Start-Sleep -Seconds 3
        $unityProcs = Get-Process -Name Unity -ErrorAction SilentlyContinue |
            Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }
        if ($unityProcs) {
            $unityProcs | Stop-Process -Force
            Start-Sleep -Seconds 2
        }
    }
}

# ── Clean up stale build report ──
function Clear-BuildReport {
    if (Test-Path $BuildReport) {
        Remove-Item $BuildReport -Force -ErrorAction SilentlyContinue
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

    $batchArgs = @(
        "-batchmode",
        "-projectPath", $ProjectPath,
        "-executeMethod", $Method,
        "-logFile", $LogFile,
        "-quit"
    )

    $proc = Start-Process -FilePath $UnityEditor -ArgumentList $batchArgs -NoNewWindow -PassThru -Wait
    $exitCode = $proc.ExitCode

    # Show [Tartaria] lines from log
    if (Test-Path $LogFile) {
        Write-Host "-- Build Log ([Tartaria] lines) --" -ForegroundColor DarkGray
        Get-Content $LogFile -Tail 60 | ForEach-Object {
            if ($_ -match "\[Tartaria\]") {
                if ($_ -match "FAIL|ERROR|CRASH") {
                    Write-Host "  $_" -ForegroundColor Red
                } elseif ($_ -match "OK|PASSED|complete") {
                    Write-Host "  $_" -ForegroundColor Green
                } else {
                    Write-Host "  $_" -ForegroundColor Cyan
                }
            }
        }
        Write-Host "-- End --" -ForegroundColor DarkGray
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

# ── Function: Monitor Editor.log for pipeline progress ──
function Watch-EditorLog {
    Write-Host ""
    Write-Host "  Monitoring Unity Editor.log for pipeline output..." -ForegroundColor DarkGray
    Write-Host "  Timeout: ${MonitorTimeout}s | Report: $BuildReport" -ForegroundColor DarkGray
    Write-Host ""

    $startTime = Get-Date
    $lastPos = 0
    $pipelineStarted = $false
    $pipelineFinished = $false
    $compileErrors = @()
    $tartariaErrors = @()
    $phasesSeen = 0

    # Find current end of Editor.log so we only watch new content
    if (Test-Path $EditorLog) {
        $lastPos = (Get-Item $EditorLog).Length
    }

    while (-not $pipelineFinished) {
        $elapsed = ((Get-Date) - $startTime).TotalSeconds
        if ($elapsed -gt $MonitorTimeout) {
            Write-Host ""
            Write-Host "  TIMEOUT: Pipeline did not complete within ${MonitorTimeout}s" -ForegroundColor Red
            Write-Host "  Unity may still be compiling or importing. Check Unity Console." -ForegroundColor Yellow
            break
        }

        Start-Sleep -Milliseconds 500

        if (-not (Test-Path $EditorLog)) { continue }

        $fileSize = (Get-Item $EditorLog).Length
        if ($fileSize -le $lastPos) { continue }

        # Read new content
        try {
            $stream = [System.IO.FileStream]::new($EditorLog, [System.IO.FileMode]::Open,
                [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
            $stream.Seek($lastPos, [System.IO.SeekOrigin]::Begin) | Out-Null
            $reader = [System.IO.StreamReader]::new($stream)
            $newContent = $reader.ReadToEnd()
            $lastPos = $stream.Position
            $reader.Close()
            $stream.Close()
        } catch {
            continue
        }

        $lines = $newContent -split "`n"
        foreach ($line in $lines) {
            $trimmed = $line.Trim()
            if (-not $trimmed) { continue }

            # Compilation errors
            if ($trimmed -match "^Assets.*error CS\d+") {
                $compileErrors += $trimmed
                Write-Host "  COMPILE: $trimmed" -ForegroundColor Red
            }

            # Tartaria pipeline messages
            if ($trimmed -match "\[Tartaria\]") {
                $pipelineStarted = $true

                if ($trimmed -match "SENTINEL DETECTED|AUTO-PLAY SENTINEL") {
                    Write-Host "  >> Pipeline started" -ForegroundColor Cyan
                }
                elseif ($trimmed -match "Phase \d+/\d+") {
                    $phasesSeen++
                    Write-Host "  $trimmed" -ForegroundColor Cyan
                }
                elseif ($trimmed -match "FAIL|ERROR|CRASH") {
                    $tartariaErrors += $trimmed
                    Write-Host "  $trimmed" -ForegroundColor Red
                }
                elseif ($trimmed -match "OK\s|PASSED|complete") {
                    Write-Host "  $trimmed" -ForegroundColor Green
                }
                elseif ($trimmed -match "RESULT:|ALL.*PHASES|entering Play") {
                    Write-Host "  $trimmed" -ForegroundColor Green
                    $pipelineFinished = $true
                }
                elseif ($trimmed -match "NOT entering Play") {
                    Write-Host "  $trimmed" -ForegroundColor Yellow
                    $pipelineFinished = $true
                }
                elseif ($trimmed -match "SKIP") {
                    Write-Host "  $trimmed" -ForegroundColor Yellow
                }
                else {
                    Write-Host "  $trimmed" -ForegroundColor DarkCyan
                }
            }

            # Unity internal errors (not ours, but worth flagging)
            if ($trimmed -match "NullReferenceException" -and $trimmed -notmatch "DeviceSimul") {
                Write-Host "  UNITY: $trimmed" -ForegroundColor DarkRed
            }
        }
    }

    # ── Summary ──
    Write-Host ""
    Write-Host "  ============================================" -ForegroundColor DarkCyan
    Write-Host "   POST-LAUNCH SUMMARY" -ForegroundColor Cyan
    Write-Host "  ============================================" -ForegroundColor DarkCyan

    if ($compileErrors.Count -gt 0) {
        Write-Host ""
        Write-Host "  COMPILE ERRORS: $($compileErrors.Count)" -ForegroundColor Red
        $compileErrors | Select-Object -First 10 | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
        if ($compileErrors.Count -gt 10) {
            Write-Host "    ... and $($compileErrors.Count - 10) more" -ForegroundColor DarkRed
        }
    }

    if ($tartariaErrors.Count -gt 0) {
        Write-Host ""
        Write-Host "  PIPELINE ERRORS: $($tartariaErrors.Count)" -ForegroundColor Red
        $tartariaErrors | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
    }

    # Show build report if it was written
    if (Test-Path $BuildReport) {
        Write-Host ""
        Write-Host "  BUILD REPORT:" -ForegroundColor Cyan
        Get-Content $BuildReport | ForEach-Object {
            if ($_ -match "FAIL") {
                Write-Host "    $_" -ForegroundColor Red
            } elseif ($_ -match "OK|PASSED|RESULT.*ALL") {
                Write-Host "    $_" -ForegroundColor Green
            } elseif ($_ -match "SKIP") {
                Write-Host "    $_" -ForegroundColor Yellow
            } else {
                Write-Host "    $_" -ForegroundColor DarkGray
            }
        }
    }

    if ($compileErrors.Count -eq 0 -and $tartariaErrors.Count -eq 0 -and $pipelineFinished) {
        Write-Host ""
        Write-Host "  ALL CLEAR -- Game should be running in Unity" -ForegroundColor Green
    } elseif (-not $pipelineStarted) {
        Write-Host ""
        Write-Host "  Pipeline never started. Possible causes:" -ForegroundColor Yellow
        Write-Host "    - Compilation errors prevented AutoPlayBoot from loading" -ForegroundColor Yellow
        Write-Host "    - Unity is still importing assets (wait and check Console)" -ForegroundColor Yellow
        Write-Host "    - Sentinel file was not in Temp/ when Unity loaded" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "  Editor.log: $EditorLog" -ForegroundColor DarkGray
    Write-Host "  ============================================" -ForegroundColor DarkCyan
    Write-Host ""
}


# ══════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════

Close-UnityForProject
Clear-BuildReport

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
        if ($validOK) { exit 0 } else { exit 1 }
    }
}

# ── Drop sentinel file for AutoPlayBoot ──
$sentinelPath = Join-Path $ProjectPath "Temp\TARTARIA_AUTOPLAY"
$tempDir = Join-Path $ProjectPath "Temp"
if (-not (Test-Path $tempDir)) { New-Item -ItemType Directory -Path $tempDir -Force | Out-Null }
"autoplay" | Set-Content -Path $sentinelPath -NoNewline
Write-Host "Sentinel file created" -ForegroundColor Green

# ── Launch Unity GUI ──
Write-Host "Launching Unity..." -ForegroundColor Cyan
$guiArgs = @("-projectPath", $ProjectPath)
Start-Process -FilePath $UnityEditor -ArgumentList $guiArgs
Write-Host ""
Write-Host "  Unity will:" -ForegroundColor White
Write-Host "    1. Close Device Simulator (prevents NullRef)" -ForegroundColor White
Write-Host "    2. BUILD EVERYTHING (scenes, prefabs, SOs, managers)" -ForegroundColor White
Write-Host "    3. Run Readiness Validation" -ForegroundColor White
Write-Host "    4. Open Boot scene and enter Play mode" -ForegroundColor White
Write-Host ""

# ── Monitor or exit ──
if ($NoMonitor) {
    Write-Host "Monitoring disabled. Watch Unity Console for progress." -ForegroundColor Yellow
    exit 0
}

# Wait for Unity to start writing to Editor.log
Write-Host "Waiting for Unity to initialize..." -ForegroundColor DarkGray
$waitStart = Get-Date
$unityReady = $false
while (-not $unityReady -and ((Get-Date) - $waitStart).TotalSeconds -lt 60) {
    Start-Sleep -Seconds 2
    $unityProc = Get-Process -Name Unity -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }
    if ($unityProc -and $unityProc.MainWindowHandle -ne 0) {
        $unityReady = $true
    }
}

if (-not $unityReady) {
    Write-Host "Unity process not detected after 60s. Check manually." -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity running (PID $($unityProc.Id)). Monitoring pipeline..." -ForegroundColor Green
Watch-EditorLog
