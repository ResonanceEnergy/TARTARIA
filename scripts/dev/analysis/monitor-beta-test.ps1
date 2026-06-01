#!/usr/bin/env pwsh
# TARTARIA Beta Test Monitor — Real-time performance and quality tracking
# Usage: .\monitor-beta-test.ps1

param(
    [int]$RefreshSeconds = 5,
    [switch]$Verbose
)

$logFile = "Logs\beta-test-session.log"
$reportFile = "Logs\beta-test-report.txt"

Write-Host "=== TARTARIA Beta Test Monitor ===" -ForegroundColor Cyan
Write-Host "Log: $logFile" -ForegroundColor Gray
Write-Host "Report: $reportFile" -ForegroundColor Gray
Write-Host "Refresh: ${RefreshSeconds}s | Ctrl+C to stop`n" -ForegroundColor Gray

# Initialize counters
$startTime = Get-Date
$lastPosition = 0
$errorCount = 0
$warningCount = 0
$performanceIssues = @()
$compileErrors = @()

function Write-Summary {
    $elapsed = (Get-Date) - $startTime
    $summary = @"

=== BETA TEST SUMMARY ===
Session Duration: $($elapsed.ToString("hh\:mm\:ss"))
Compilation Errors: $($compileErrors.Count)
Runtime Errors: $errorCount
Warnings: $warningCount
Performance Issues: $($performanceIssues.Count)

"@
    
    if ($compileErrors.Count -gt 0) {
        $summary += "`nCOMPILATION ERRORS:`n"
        $compileErrors | Select-Object -First 10 | ForEach-Object { $summary += "  $_`n" }
    }
    
    if ($performanceIssues.Count -gt 0) {
        $summary += "`nPERFORMANCE ISSUES:`n"
        $performanceIssues | Select-Object -First 10 | ForEach-Object { $summary += "  $_`n" }
    }
    
    $summary | Out-File -FilePath $reportFile -Encoding UTF8
    Write-Host $summary -ForegroundColor $(if ($compileErrors.Count -eq 0 -and $errorCount -eq 0) { "Green" } else { "Yellow" })
}

try {
    while ($true) {
        if (Test-Path $logFile) {
            $content = Get-Content $logFile -Encoding UTF8 -ErrorAction SilentlyContinue
            
            if ($content -and $content.Count -gt $lastPosition) {
                $newLines = $content[$lastPosition..($content.Count - 1)]
                
                foreach ($line in $newLines) {
                    # Compilation errors (CS errors)
                    if ($line -match "error CS\d+:" -or $line -match "CompilerOutput.*error") {
                        $compileErrors += $line
                        Write-Host "[COMPILE ERROR] $line" -ForegroundColor Red
                    }
                    # Runtime exceptions
                    elseif ($line -match "Exception|NullReferenceException|ArgumentException") {
                        $errorCount++
                        Write-Host "[RUNTIME ERROR] $line" -ForegroundColor Red
                    }
                    # Performance warnings
                    elseif ($line -match "Frame time|FPS|Memory.*leak|GC\.Alloc|Slow script") {
                        $performanceIssues += $line
                        Write-Host "[PERFORMANCE] $line" -ForegroundColor Yellow
                    }
                    # Warnings
                    elseif ($line -match "warning|Warning") {
                        $warningCount++
                        if ($Verbose) {
                            Write-Host "[WARNING] $line" -ForegroundColor DarkYellow
                        }
                    }
                    # LiveOps telemetry
                    elseif ($line -match "TelemetryService|StabilityMonitor|LoadPerformance|EconomyBalance") {
                        Write-Host "[LIVEOPS] $line" -ForegroundColor Cyan
                    }
                    # Important game events
                    elseif ($line -match "Player|Scene|GameState|Interactable|Quest") {
                        if ($Verbose) {
                            Write-Host "[GAME] $line" -ForegroundColor Gray
                        }
                    }
                }
                
                $lastPosition = $content.Count
            }
        }
        else {
            Write-Host "Waiting for log file..." -ForegroundColor Gray
        }
        
        Start-Sleep -Seconds $RefreshSeconds
    }
}
finally {
    Write-Host "`n`nGenerating final report..." -ForegroundColor Cyan
    Write-Summary
    Write-Host "Report saved to: $reportFile" -ForegroundColor Green
}
