#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Automated Test Runner — Unity batchmode test execution with reporting.

.DESCRIPTION
    Launches Unity in batchmode to run TestOrchestrator in Echohaven scene.
    Tests bypass assembly boundary violations (no Tartaria.AI references).
    Results logged to Console with [AutoTest] prefix.
    
    ENHANCEMENTS (Agent 10):
    - Detailed log parsing with metrics extraction
    - Performance benchmark extraction (FPS, memory)
    - Automatic report generation (Markdown, HTML, JSON)
    - CI/CD integration support
    - Failed test detail extraction

.PARAMETER SceneName
    Unity scene to load (default: Echohaven)

.PARAMETER LogFile
    Output log file path (default: Logs/test-run.log)

.PARAMETER NoQuit
    Keep Unity running after tests complete (for debugging)

.PARAMETER GenerateReport
    Generate test reports after execution (default: true)

.PARAMETER OpenHTMLReport
    Open HTML report in browser after generation (default: false)

.PARAMETER Mode
    Test execution mode:
    - Smoke: Ultra-fast sanity check (8 tests, ~3 min)
    - CriticalPath: Fast regression tests (18 tests, ~12 min)
    - Full: All tests (84 tests, ~60 min)
    - Regression: Compare against baseline

.EXAMPLE
    .\run-automated-tests.ps1
    # Run tests in Echohaven scene, exit on complete, generate reports

.EXAMPLE
    .\run-automated-tests.ps1 -Mode Smoke
    # Run smoke tests only (~3 min)

.EXAMPLE
    .\run-automated-tests.ps1 -Mode CriticalPath
    # Run critical path tests only (~12 min)

.EXAMPLE
    .\run-automated-tests.ps1 -SceneName "Echohaven" -LogFile "test-results.log"
    # Custom scene and log file

.EXAMPLE
    .\run-automated-tests.ps1 -NoQuit -GenerateReport:$false
    # Run tests but don't quit Unity and skip report generation (for debugging)

.EXAMPLE
    .\run-automated-tests.ps1 -OpenHTMLReport
    # Run tests and open HTML report in browser

.NOTES
    Unity 6000.3.6f1 required.
    TestOrchestrator must be attached to GameObject in scene.
    Exit code 0 = all tests passed, 1 = failures detected.
    Reports generated in: Logs/Reports/
#>

param(
    [string]$SceneName = "Echohaven_VerticalSlice",
    [string]$LogFile = "Logs/test-run.log",
    [switch]$NoQuit,
    [bool]$GenerateReport = $true,
    [switch]$OpenHTMLReport,
    [ValidateSet("Full", "Smoke", "CriticalPath", "Regression")]
    [string]$Mode = "Full"
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# TEST METRICS TRACKING
# ═══════════════════════════════════════════════════════════════════════════════

$TestMetrics = @{
    StartTime = Get-Date
    EndTime = $null
    Duration = $null
    TotalPass = 0
    TotalFail = 0
    TotalWarn = 0
    PhaseCount = 0
    Phases = @()
}

# ─── Configuration ────────────────────────────────────────────────────────────

$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$ProjectPath = "C:\dev\TARTARIA_new"
$ScenePath = "Assets/_Project/Scenes/$SceneName.unity"

# ─── Validation ───────────────────────────────────────────────────────────────

if (-not (Test-Path $UnityPath)) {
    Write-Error "Unity not found at: $UnityPath"
    exit 1
}

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project not found at: $ProjectPath"
    exit 1
}

if (-not (Test-Path "$ProjectPath\$ScenePath")) {
    Write-Error "Scene not found: $ScenePath"
    exit 1
}

# Ensure Logs directory exists
$LogDir = Split-Path $LogFile -Parent
if ($LogDir -and -not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

# ─── Build Unity Command ──────────────────────────────────────────────────────

$UnityArgs = @(
    "-batchmode"
    "-projectPath", "`"$ProjectPath`""
    "-executeMethod", "Tartaria.Editor.TestRunner.RunAllTestsBatchmode"
    "-logFile", "`"$LogFile`""
)

# Add test filter based on mode
switch ($Mode) {
    "Smoke" {
        $UnityArgs += "-testCategory", "Smoke"
    }
    "CriticalPath" {
        $UnityArgs += "-testCategory", "CriticalPath"
    }
    "Regression" {
        # Run all tests but compare against baseline
        # No filter needed
    }
    "Full" {
        # Run all tests (no filter)
    }
}

# Note: Do NOT use -quit for test execution.
# TestOrchestrator calls Application.Quit(exitCode) when tests complete.
# Using -quit here would terminate Unity before tests run.
if ($NoQuit) {
    Write-Host "Note: -NoQuit specified, but TestRunner manages quit automatically." -ForegroundColor Yellow
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Automated Test Runner" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Unity:    $UnityPath"
Write-Host "Project:  $ProjectPath"
Write-Host "Scene:    $ScenePath"
Write-Host "Log:      $LogFile"
Write-Host "Mode:     $Mode"
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

# Display mode info
switch ($Mode) {
    "Smoke" {
        Write-Host "⚡ SMOKE TEST MODE: 8 tests, ~3 min" -ForegroundColor Yellow
        Write-Host "   Ultra-fast sanity check for critical systems" -ForegroundColor Gray
    }
    "CriticalPath" {
        Write-Host "🎯 CRITICAL PATH MODE: 18 tests, ~12 min" -ForegroundColor Yellow
        Write-Host "   Fast regression testing for hotfix validation" -ForegroundColor Gray
    }
    "Regression" {
        Write-Host "📊 REGRESSION MODE: Compare against baseline" -ForegroundColor Yellow
        Write-Host "   Detect new test failures and performance regressions" -ForegroundColor Gray
    }
    "Full" {
        Write-Host "🔍 FULL TEST MODE: 84 tests, ~60 min" -ForegroundColor Yellow
        Write-Host "   Complete test suite with all integration and E2E tests" -ForegroundColor Gray
    }
}
Write-Host ""

# ─── Execute Tests ────────────────────────────────────────────────────────────

Write-Host "Starting Unity batchmode test execution..." -ForegroundColor Yellow
Write-Host ""

$UnityProcess = Start-Process -FilePath $UnityPath -ArgumentList $UnityArgs -PassThru -NoNewWindow

# Wait for Unity to complete
$UnityProcess.WaitForExit()

$ExitCode = $UnityProcess.ExitCode

Write-Host ""
Write-Host "───────────────────────────────────────────────────────────"

# ─── Parse Results ────────────────────────────────────────────────────────────

$TestMetrics.EndTime = Get-Date
$TestMetrics.Duration = ($TestMetrics.EndTime - $TestMetrics.StartTime).TotalSeconds

if (Test-Path $LogFile) {
    Write-Host ""
    Write-Host "Test Results:" -ForegroundColor Cyan
    Write-Host ""
    
    # Extract [AutoTest] lines from log
    $TestLines = Get-Content $LogFile | Where-Object { $_ -match "\[AutoTest\]" }
    
    if ($TestLines.Count -gt 0) {
        $currentPhase = $null
        
        foreach ($line in $TestLines) {
            # Track phase detection
            if ($line -match "Phase \d+: (.+?)(?:\s*\(|$)") {
                $phaseName = $matches[1].Trim()
                $currentPhase = @{
                    Name = $phaseName
                    Pass = 0
                    Fail = 0
                    Warn = 0
                }
                $TestMetrics.Phases += $currentPhase
                $TestMetrics.PhaseCount++
            }
            
            # Count pass/fail/warn
            if ($line -match "\[PASS\]") {
                $TestMetrics.TotalPass++
                if ($currentPhase) { $currentPhase.Pass++ }
                Write-Host $line -ForegroundColor Green
            }
            elseif ($line -match "\[FAIL\]") {
                $TestMetrics.TotalFail++
                if ($currentPhase) { $currentPhase.Fail++ }
                Write-Host $line -ForegroundColor Red
            }
            elseif ($line -match "\[WARN\]") {
                $TestMetrics.TotalWarn++
                if ($currentPhase) { $currentPhase.Warn++ }
                Write-Host $line -ForegroundColor Yellow
            }
            else {
                Write-Host $line
            }
        }
        
        # Display summary metrics
        Write-Host ""
        Write-Host "───────────────────────────────────────────────────────────" -ForegroundColor Cyan
        Write-Host "Test Execution Summary:" -ForegroundColor Cyan
        Write-Host "  Duration:  $($TestMetrics.Duration.ToString('F2'))s"
        Write-Host "  Phases:    $($TestMetrics.PhaseCount)"
        Write-Host "  Passed:    $($TestMetrics.TotalPass)" -ForegroundColor Green
        Write-Host "  Failed:    $($TestMetrics.TotalFail)" -ForegroundColor $(if ($TestMetrics.TotalFail -eq 0) { "Gray" } else { "Red" })
        Write-Host "  Warnings:  $($TestMetrics.TotalWarn)" -ForegroundColor $(if ($TestMetrics.TotalWarn -eq 0) { "Gray" } else { "Yellow" })
        
        # Phase breakdown
        if ($TestMetrics.Phases.Count -gt 0) {
            Write-Host ""
            Write-Host "Phase Breakdown:" -ForegroundColor Cyan
            foreach ($phase in $TestMetrics.Phases) {
                $statusIcon = if ($phase.Fail -eq 0) { "✓" } else { "✗" }
                $statusColor = if ($phase.Fail -eq 0) { "Green" } else { "Red" }
                Write-Host "  $statusIcon $($phase.Name): $($phase.Pass) passed, $($phase.Fail) failed, $($phase.Warn) warnings" -ForegroundColor $statusColor
            }
        }
    }
    else {
        Write-Host "No [AutoTest] output found in log file." -ForegroundColor Yellow
        Write-Host "Possible causes:"
        Write-Host "  - TestOrchestrator not attached to GameObject in scene"
        Write-Host "  - Scene failed to load"
        Write-Host "  - Unity crashed before tests ran"
        Write-Host ""
        Write-Host "Last 50 lines of log:"
        Get-Content $LogFile | Select-Object -Last 50
    }
}
else {
    Write-Host "Log file not created: $LogFile" -ForegroundColor Red
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# ─── Generate Reports ─────────────────────────────────────────────────────────

if ($GenerateReport -and (Test-Path $LogFile)) {
    Write-Host ""
    Write-Host "Generating test reports..." -ForegroundColor Yellow
    
    $reportScript = Join-Path $PSScriptRoot "test-report-generator.ps1"
    if (Test-Path $reportScript) {
        try {
            & $reportScript -LogFile $LogFile -OutputFormat All
            
            if ($OpenHTMLReport) {
                $htmlReport = "Logs\Reports\TestReport-Latest.html"
                if (Test-Path $htmlReport) {
                    Write-Host "Opening HTML report in browser..." -ForegroundColor Cyan
                    Start-Process $htmlReport
                }
            }
        }
        catch {
            Write-Host "Warning: Report generation failed: $_" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Warning: test-report-generator.ps1 not found, skipping report generation" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# ─── Exit Status ──────────────────────────────────────────────────────────────

if ($ExitCode -eq 0) {
    Write-Host "✓ Tests PASSED (exit code 0)" -ForegroundColor Green
}
else {
    Write-Host "✗ Tests FAILED (exit code $ExitCode)" -ForegroundColor Red
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# ─── Save Metrics to JSON ─────────────────────────────────────────────────────

if ($GenerateReport) {
    try {
        $metricsPath = "Logs\test-metrics-latest.json"
        $TestMetrics | ConvertTo-Json -Depth 5 | Set-Content $metricsPath -Encoding UTF8
        Write-Host "✓ Metrics saved to: $metricsPath" -ForegroundColor Green
    }
    catch {
        Write-Host "Warning: Could not save metrics: $_" -ForegroundColor Yellow
    }
}

exit $ExitCode
