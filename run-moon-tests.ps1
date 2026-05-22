#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Moon 1-13 Test Suite Execution Script
.DESCRIPTION
    Runs automated PlayMode tests for all Moon content, generates test reports,
    and optionally profiles performance using Unity Profiler.
.PARAMETER TestSuite
    Which test suite to run: Moon1-4, Moon5-10, Moon11-13, Performance, All
.PARAMETER BuildFirst
    Whether to rebuild the project before running tests (default: false)
.PARAMETER Profile
    Whether to enable Unity Profiler during tests (default: false)
.PARAMETER GenerateReport
    Whether to generate a test report after execution (default: true)
.EXAMPLE
    .\run-moon-tests.ps1 -TestSuite All
    .\run-moon-tests.ps1 -TestSuite Moon1-4 -BuildFirst
    .\run-moon-tests.ps1 -TestSuite Performance -Profile
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Moon1-4", "Moon5-10", "Moon11-13", "Performance", "All")]
    [string]$TestSuite = "All",
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildFirst,
    
    [Parameter(Mandatory=$false)]
    [switch]$Profile,
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateReport = $true
)

$ErrorActionPreference = "Stop"
$ProjectPath = $PSScriptRoot
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe"
$LogDir = Join-Path $ProjectPath "Logs"
$TestLogFile = Join-Path $LogDir "moon-tests-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Ensure logs directory exists
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir | Out-Null
}

Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "TARTARIA - Moon Test Suite Runner" -ForegroundColor Cyan
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "Test Suite: $TestSuite" -ForegroundColor Yellow
Write-Host "Build First: $BuildFirst" -ForegroundColor Yellow
Write-Host "Profile: $Profile" -ForegroundColor Yellow
Write-Host "Log File: $TestLogFile" -ForegroundColor Yellow
Write-Host ""

# Step 1: Optional build
if ($BuildFirst) {
    Write-Host "[1/3] Building project..." -ForegroundColor Green
    
    & $UnityPath `
        -batchmode `
        -nographics `
        -projectPath $ProjectPath `
        -executeMethod Tartaria.Editor.BuildPipeline.BuildWindows `
        -logFile "$LogDir\build-before-tests.log" `
        -quit
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    
    Write-Host "Build complete." -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[1/3] Skipping build (use -BuildFirst to enable)" -ForegroundColor Gray
    Write-Host ""
}

# Step 2: Run PlayMode tests
Write-Host "[2/3] Running PlayMode tests..." -ForegroundColor Green

$testFilter = switch ($TestSuite) {
    "Moon1-4"    { "Moon1;Moon2;Moon3;Moon4;SaveLoad" }
    "Moon5-10"   { "Moon5;Moon10" }
    "Moon11-13"  { "Moon13" }
    "Performance" { "Performance;Memory" }
    "All"        { "" }
}

$unityArgs = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", "PlayMode",
    "-testResults", "$LogDir\test-results.xml",
    "-logFile", $TestLogFile
)

if ($Profile) {
    $unityArgs += @("-enableCodeCoverage", "-coverageResultsPath", "$LogDir\CodeCoverage")
}

Write-Host "Unity Test Runner starting..." -ForegroundColor Cyan
Write-Host "Filter: $testFilter" -ForegroundColor Gray

$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -PassThru -NoNewWindow

# Monitor test execution
$timeout = 600 # 10 minutes
$elapsed = 0
$checkInterval = 5

while (-not $process.HasExited -and $elapsed -lt $timeout) {
    Start-Sleep -Seconds $checkInterval
    $elapsed += $checkInterval
    
    $dots = "." * (($elapsed / $checkInterval) % 4)
    Write-Host "`rTests running$dots".PadRight(40) -NoNewline -ForegroundColor Yellow
}

Write-Host "" # New line after progress dots

if ($process.HasExited) {
    $exitCode = $process.ExitCode
    Write-Host "Tests completed with exit code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })
} else {
    Write-Host "Tests timed out after $timeout seconds!" -ForegroundColor Red
    $process.Kill()
    exit 1
}

Write-Host ""

# Step 3: Generate report
if ($GenerateReport) {
    Write-Host "[3/3] Generating test report..." -ForegroundColor Green
    
    $report = @"
=================================================================================
TARTARIA - Moon Test Execution Report
=================================================================================
Execution Time: $(Get-Date)
Test Suite: $TestSuite
Build First: $BuildFirst
Profile: $Profile
Exit Code: $exitCode

=================================================================================
TEST RESULTS
=================================================================================

"@

    # Parse test results XML if available
    $xmlPath = Join-Path $LogDir "test-results.xml"
    if (Test-Path $xmlPath) {
        try {
            [xml]$xml = Get-Content $xmlPath
            $testRun = $xml.SelectSingleNode("//test-run")
            
            if ($testRun) {
                $total = $testRun.total
                $passed = $testRun.passed
                $failed = $testRun.failed
                $skipped = $testRun.skipped
                
                $report += @"
Total Tests: $total
Passed: $passed
Failed: $failed
Skipped: $skipped

Pass Rate: $([math]::Round(($passed / $total) * 100, 2))%

"@
                
                # List failed tests
                if ([int]$failed -gt 0) {
                    $report += "`nFailed Tests:`n"
                    $report += "-" * 80 + "`n"
                    
                    $failedTests = $xml.SelectNodes("//test-case[@result='Failed']")
                    foreach ($test in $failedTests) {
                        $testName = $test.name
                        $message = $test.failure.message
                        $report += "  ✗ $testName`n"
                        $report += "    $message`n`n"
                    }
                }
            }
        } catch {
            $report += "Error parsing test results XML: $_`n"
        }
    } else {
        $report += "No test results XML found at: $xmlPath`n"
    }
    
    $report += @"

=================================================================================
LOG FILES
=================================================================================
Test Log: $TestLogFile
Results XML: $xmlPath

=================================================================================
NEXT STEPS
=================================================================================

"@

    if ($exitCode -eq 0) {
        $report += @"
✓ All tests passed!

Next actions:
1. Review test log for warnings or performance issues
2. Run manual test checklist for user experience validation
3. Profile performance using Unity Profiler (Tools > Moon Test Runner > Run Performance Tests)
4. Update AUDIT_REPORT with test results

"@
    } else {
        $report += @"
✗ Tests failed or encountered errors.

Next actions:
1. Review test log: $TestLogFile
2. Check failed test details above
3. Fix reported issues and re-run tests
4. Consider running individual test suites for faster iteration

"@
    }
    
    $report += "=" * 80 + "`n"
    
    # Save report
    $reportPath = Join-Path $LogDir "moon-test-report.txt"
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    
    Write-Host "Test report saved to: $reportPath" -ForegroundColor Cyan
    Write-Host ""
    
    # Display summary
    Write-Host $report
}

Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "Test execution complete!" -ForegroundColor Cyan
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan

exit $exitCode
