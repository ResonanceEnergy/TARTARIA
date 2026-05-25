#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Test Report Generator — Parse Unity test logs and generate detailed reports.

.DESCRIPTION
    Parses Unity Editor.log and test-run.log for [AutoTest] lines and generates:
    - Structured TestReport.md with pass/fail metrics
    - Performance benchmarks (FPS, memory)
    - HTML report for CI/CD integration
    - JSON export for programmatic analysis

.PARAMETER LogFile
    Path to Unity test log (default: Logs/test-run.log)

.PARAMETER EditorLog
    Path to Unity Editor.log (optional, for extended diagnostics)

.PARAMETER OutputFormat
    Report output format: Markdown, HTML, JSON, All (default: All)

.PARAMETER OutputDir
    Directory for generated reports (default: Logs/Reports)

.EXAMPLE
    .\test-report-generator.ps1
    # Parse test-run.log and generate all report formats

.EXAMPLE
    .\test-report-generator.ps1 -LogFile "custom-test.log" -OutputFormat HTML

.NOTES
    Parses [AutoTest] lines from Unity logs.
    Extracts: pass/fail counts, performance metrics, test phase details.
#>

param(
    [string]$LogFile = "Logs/test-run.log",
    [string]$EditorLog = "",
    [ValidateSet("Markdown", "HTML", "JSON", "All")]
    [string]$OutputFormat = "All",
    [string]$OutputDir = "Logs/Reports"
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# DATA STRUCTURES
# ═══════════════════════════════════════════════════════════════════════════════

class TestPhase {
    [string]$Name
    [int]$Pass = 0
    [int]$Fail = 0
    [int]$Warn = 0
    [string]$Status = "Unknown"
    [System.Collections.ArrayList]$Details = @()
}

class TestReport {
    [string]$Timestamp
    [string]$Scene
    [string]$UnityVersion
    [int]$TotalPass = 0
    [int]$TotalFail = 0
    [int]$TotalWarn = 0
    [string]$Status = "Unknown"
    [System.Collections.ArrayList]$Phases = @()
    [hashtable]$Performance = @{
        AvgFPS = 0.0
        MinFPS = 0.0
        MaxFPS = 0.0
        AvgFrameTime = 0.0
        HeapMemoryMB = 0.0
        TotalMemoryMB = 0.0
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# LOG PARSING
# ═══════════════════════════════════════════════════════════════════════════════

function Parse-TestLog {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Error "Log file not found: $Path"
        exit 1
    }
    
    $report = [TestReport]::new()
    $report.Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    
    $lines = Get-Content $Path
    $currentPhase = $null
    
    foreach ($line in $lines) {
        if ($line -notmatch "\[AutoTest\]") { continue }
        
        # Extract Unity version
        if ($line -match "Unity (\d+\.\d+\.\d+\w+)") {
            $report.UnityVersion = $matches[1]
        }
        
        # Extract scene name
        if ($line -match "Scene: (\w+)") {
            $report.Scene = $matches[1]
        }
        
        # Phase detection: "Phase N: PhaseNameHere"
        if ($line -match "Phase \d+: (.+?)(?:\s*\(|$)") {
            $phaseName = $matches[1].Trim()
            $currentPhase = [TestPhase]::new()
            $currentPhase.Name = $phaseName
            [void]$report.Phases.Add($currentPhase)
        }
        
        # Pass/Fail/Warn detection
        if ($line -match "\[PASS\]") {
            $currentPhase.Pass++
            $report.TotalPass++
            # Extract detail message
            if ($line -match "\[PASS\].*?:\s*(.+)") {
                [void]$currentPhase.Details.Add("[✓] " + $matches[1])
            }
        }
        elseif ($line -match "\[FAIL\]") {
            $currentPhase.Fail++
            $report.TotalFail++
            if ($line -match "\[FAIL\].*?:\s*(.+)") {
                [void]$currentPhase.Details.Add("[✗] " + $matches[1])
            }
        }
        elseif ($line -match "\[WARN\]") {
            $currentPhase.Warn++
            $report.TotalWarn++
            if ($line -match "\[WARN\].*?:\s*(.+)") {
                [void]$currentPhase.Details.Add("[⚠] " + $matches[1])
            }
        }
        
        # Performance metrics
        if ($line -match "Avg FPS: ([\d.]+)") {
            $report.Performance.AvgFPS = [double]$matches[1]
        }
        if ($line -match "Min FPS: ([\d.]+)") {
            $report.Performance.MinFPS = [double]$matches[1]
        }
        if ($line -match "Max FPS: ([\d.]+)") {
            $report.Performance.MaxFPS = [double]$matches[1]
        }
        if ($line -match "Avg Frame Time: ([\d.]+)ms") {
            $report.Performance.AvgFrameTime = [double]$matches[1]
        }
        if ($line -match "Heap Memory: ([\d.]+) MB") {
            $report.Performance.HeapMemoryMB = [double]$matches[1]
        }
        if ($line -match "Total Memory: ([\d.]+) MB") {
            $report.Performance.TotalMemoryMB = [double]$matches[1]
        }
        
        # Final status
        if ($line -match "ALL TESTS PASSED") {
            $report.Status = "PASSED"
        }
        elseif ($line -match "TESTS FAILED") {
            $report.Status = "FAILED"
        }
    }
    
    # Set phase status based on fail count
    foreach ($phase in $report.Phases) {
        $phase.Status = if ($phase.Fail -eq 0) { "PASSED" } else { "FAILED" }
    }
    
    return $report
}

# ═══════════════════════════════════════════════════════════════════════════════
# REPORT GENERATION
# ═══════════════════════════════════════════════════════════════════════════════

function Generate-MarkdownReport {
    param([TestReport]$Report, [string]$OutputPath)
    
    $md = @"
# TARTARIA — Test Execution Report

**Generated:** $($Report.Timestamp)  
**Scene:** $($Report.Scene)  
**Unity Version:** $($Report.UnityVersion)  
**Status:** **$($Report.Status)** 🔴

---

## Executive Summary

| Metric | Count |
|--------|-------|
| **Total Passed** | $($Report.TotalPass) ✓ |
| **Total Failed** | $($Report.TotalFail) ✗ |
| **Total Warnings** | $($Report.TotalWarn) ⚠ |
| **Test Phases** | $($Report.Phases.Count) |

---

## Test Phase Results

"@

    foreach ($phase in $Report.Phases) {
        $statusIcon = if ($phase.Status -eq "PASSED") { "✓" } else { "✗" }
        $md += @"

### $statusIcon $($phase.Name)

| Metric | Count |
|--------|-------|
| Passed | $($phase.Pass) |
| Failed | $($phase.Fail) |
| Warnings | $($phase.Warn) |

"@
        
        if ($phase.Details.Count -gt 0) {
            $md += "**Details:**`n`n"
            foreach ($detail in $phase.Details) {
                $md += "- $detail`n"
            }
            $md += "`n"
        }
    }
    
    # Performance metrics
    if ($Report.Performance.AvgFPS -gt 0) {
        $md += @"

---

## Performance Metrics

| Metric | Value |
|--------|-------|
| Average FPS | $($Report.Performance.AvgFPS.ToString("F2")) |
| Min FPS | $($Report.Performance.MinFPS.ToString("F2")) |
| Max FPS | $($Report.Performance.MaxFPS.ToString("F2")) |
| Avg Frame Time | $($Report.Performance.AvgFrameTime.ToString("F2"))ms |
| Heap Memory | $($Report.Performance.HeapMemoryMB.ToString("F2")) MB |
| Total Memory | $($Report.Performance.TotalMemoryMB.ToString("F2")) MB |

**Performance Gate:** $( if ($Report.Performance.AvgFPS -ge 60) { "✓ PASSED (≥60 FPS)" } else { "✗ FAILED (<60 FPS)" } )

"@
    }
    
    # Failed tests section
    $failedPhases = $Report.Phases | Where-Object { $_.Fail -gt 0 }
    if ($failedPhases.Count -gt 0) {
        $md += @"

---

## Failed Tests Details

"@
        foreach ($phase in $failedPhases) {
            $md += "### $($phase.Name)`n`n"
            $failedDetails = $phase.Details | Where-Object { $_ -match "\[✗\]" }
            foreach ($detail in $failedDetails) {
                $md += "- $detail`n"
            }
            $md += "`n"
        }
    }
    
    $md += @"

---

## CI/CD Integration

**Exit Code:** $( if ($Report.Status -eq "PASSED") { "0" } else { "1" } )  
**Report Path:** ``$OutputPath``  
**Log File:** ``$LogFile``

"@

    Set-Content -Path $OutputPath -Value $md -Encoding UTF8
    Write-Host "✓ Markdown report generated: $OutputPath" -ForegroundColor Green
}

function Generate-HTMLReport {
    param([TestReport]$Report, [string]$OutputPath)
    
    $statusColor = if ($Report.Status -eq "PASSED") { "#28a745" } else { "#dc3545" }
    
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>TARTARIA Test Report</title>
    <style>
        body { font-family: 'Segoe UI', sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #333; border-bottom: 3px solid $statusColor; padding-bottom: 10px; }
        .status { display: inline-block; padding: 5px 15px; border-radius: 4px; color: white; font-weight: bold; background: $statusColor; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { text-align: left; padding: 12px; border: 1px solid #ddd; }
        th { background: #f0f0f0; font-weight: 600; }
        .passed { color: #28a745; font-weight: bold; }
        .failed { color: #dc3545; font-weight: bold; }
        .warning { color: #ffc107; font-weight: bold; }
        .phase { margin: 20px 0; padding: 15px; border-left: 4px solid #007bff; background: #f8f9fa; }
        .detail { margin: 5px 0 5px 20px; }
        .perf-gate { padding: 10px; margin: 10px 0; border-radius: 4px; }
        .perf-pass { background: #d4edda; border: 1px solid #c3e6cb; }
        .perf-fail { background: #f8d7da; border: 1px solid #f5c6cb; }
    </style>
</head>
<body>
    <div class="container">
        <h1>TARTARIA — Test Execution Report</h1>
        
        <p><strong>Generated:</strong> $($Report.Timestamp)</p>
        <p><strong>Scene:</strong> $($Report.Scene)</p>
        <p><strong>Unity Version:</strong> $($Report.UnityVersion)</p>
        <p><strong>Status:</strong> <span class="status">$($Report.Status)</span></p>
        
        <h2>Executive Summary</h2>
        <table>
            <tr><th>Metric</th><th>Count</th></tr>
            <tr><td>Total Passed</td><td class="passed">$($Report.TotalPass) ✓</td></tr>
            <tr><td>Total Failed</td><td class="failed">$($Report.TotalFail) ✗</td></tr>
            <tr><td>Total Warnings</td><td class="warning">$($Report.TotalWarn) ⚠</td></tr>
            <tr><td>Test Phases</td><td>$($Report.Phases.Count)</td></tr>
        </table>
        
        <h2>Test Phase Results</h2>
"@

    foreach ($phase in $Report.Phases) {
        $statusIcon = if ($phase.Status -eq "PASSED") { "✓" } else { "✗" }
        $statusClass = if ($phase.Status -eq "PASSED") { "passed" } else { "failed" }
        
        $html += @"
        <div class="phase">
            <h3 class="$statusClass">$statusIcon $($phase.Name)</h3>
            <p><strong>Passed:</strong> $($phase.Pass) | <strong>Failed:</strong> $($phase.Fail) | <strong>Warnings:</strong> $($phase.Warn)</p>
"@
        
        if ($phase.Details.Count -gt 0) {
            $html += "<div>"
            foreach ($detail in $phase.Details) {
                $detailClass = if ($detail -match "\[✓\]") { "passed" } elseif ($detail -match "\[✗\]") { "failed" } else { "warning" }
                $html += "<div class='detail $detailClass'>$([System.Web.HttpUtility]::HtmlEncode($detail))</div>`n"
            }
            $html += "</div>"
        }
        
        $html += "</div>`n"
    }
    
    # Performance metrics
    if ($Report.Performance.AvgFPS -gt 0) {
        $perfGateClass = if ($Report.Performance.AvgFPS -ge 60) { "perf-pass" } else { "perf-fail" }
        $perfGateText = if ($Report.Performance.AvgFPS -ge 60) { "✓ PASSED (≥60 FPS)" } else { "✗ FAILED (<60 FPS)" }
        
        $html += @"
        <h2>Performance Metrics</h2>
        <table>
            <tr><th>Metric</th><th>Value</th></tr>
            <tr><td>Average FPS</td><td>$($Report.Performance.AvgFPS.ToString("F2"))</td></tr>
            <tr><td>Min FPS</td><td>$($Report.Performance.MinFPS.ToString("F2"))</td></tr>
            <tr><td>Max FPS</td><td>$($Report.Performance.MaxFPS.ToString("F2"))</td></tr>
            <tr><td>Avg Frame Time</td><td>$($Report.Performance.AvgFrameTime.ToString("F2"))ms</td></tr>
            <tr><td>Heap Memory</td><td>$($Report.Performance.HeapMemoryMB.ToString("F2")) MB</td></tr>
            <tr><td>Total Memory</td><td>$($Report.Performance.TotalMemoryMB.ToString("F2")) MB</td></tr>
        </table>
        <div class="perf-gate $perfGateClass">
            <strong>Performance Gate:</strong> $perfGateText
        </div>
"@
    }
    
    $html += @"
    </div>
</body>
</html>
"@

    Set-Content -Path $OutputPath -Value $html -Encoding UTF8
    Write-Host "✓ HTML report generated: $OutputPath" -ForegroundColor Green
}

function Generate-JSONReport {
    param([TestReport]$Report, [string]$OutputPath)
    
    $json = @{
        timestamp = $Report.Timestamp
        scene = $Report.Scene
        unityVersion = $Report.UnityVersion
        status = $Report.Status
        summary = @{
            totalPass = $Report.TotalPass
            totalFail = $Report.TotalFail
            totalWarn = $Report.TotalWarn
            phaseCount = $Report.Phases.Count
        }
        phases = @($Report.Phases | ForEach-Object {
            @{
                name = $_.Name
                status = $_.Status
                pass = $_.Pass
                fail = $_.Fail
                warn = $_.Warn
                details = @($_.Details)
            }
        })
        performance = $Report.Performance
    } | ConvertTo-Json -Depth 10
    
    Set-Content -Path $OutputPath -Value $json -Encoding UTF8
    Write-Host "✓ JSON report generated: $OutputPath" -ForegroundColor Green
}

# ═══════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Test Report Generator" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Ensure output directory exists
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Parse test log
Write-Host "Parsing test log: $LogFile" -ForegroundColor Yellow
$report = Parse-TestLog -Path $LogFile

# Display summary
Write-Host ""
Write-Host "Test Summary:" -ForegroundColor Cyan
Write-Host "  Scene:     $($report.Scene)"
Write-Host "  Status:    $($report.Status)"
Write-Host "  Passed:    $($report.TotalPass)" -ForegroundColor Green
Write-Host "  Failed:    $($report.TotalFail)" -ForegroundColor $(if ($report.TotalFail -eq 0) { "Gray" } else { "Red" })
Write-Host "  Warnings:  $($report.TotalWarn)" -ForegroundColor $(if ($report.TotalWarn -eq 0) { "Gray" } else { "Yellow" })
Write-Host ""

# Generate reports
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

if ($OutputFormat -eq "All" -or $OutputFormat -eq "Markdown") {
    $mdPath = Join-Path $OutputDir "TestReport-$timestamp.md"
    Generate-MarkdownReport -Report $report -OutputPath $mdPath
    
    # Also create a "latest" symlink
    $latestPath = Join-Path $OutputDir "TestReport-Latest.md"
    Copy-Item $mdPath $latestPath -Force
}

if ($OutputFormat -eq "All" -or $OutputFormat -eq "HTML") {
    $htmlPath = Join-Path $OutputDir "TestReport-$timestamp.html"
    Generate-HTMLReport -Report $report -OutputPath $htmlPath
    
    $latestPath = Join-Path $OutputDir "TestReport-Latest.html"
    Copy-Item $htmlPath $latestPath -Force
}

if ($OutputFormat -eq "All" -or $OutputFormat -eq "JSON") {
    $jsonPath = Join-Path $OutputDir "TestReport-$timestamp.json"
    Generate-JSONReport -Report $report -OutputPath $jsonPath
    
    $latestPath = Join-Path $OutputDir "TestReport-Latest.json"
    Copy-Item $jsonPath $latestPath -Force
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Reports generated in: $OutputDir" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

exit $(if ($report.Status -eq "PASSED") { 0 } else { 1 })
