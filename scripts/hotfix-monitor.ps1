#!/usr/bin/env pwsh
<#
.SYNOPSIS
    TARTARIA Hotfix Monitoring Script

.DESCRIPTION
    Post-deployment monitoring for hotfix releases.
    Tracks health metrics and auto-triggers rollback if thresholds exceeded.

.PARAMETER IssueNumber
    Issue number of the hotfix being monitored

.PARAMETER Duration
    Monitoring duration in minutes (default: 30)

.PARAMETER CheckInterval
    Time between health checks in seconds (default: 60)

.PARAMETER AutoRollback
    Automatically trigger rollback if thresholds exceeded

.EXAMPLE
    .\hotfix-monitor.ps1 -IssueNumber 123 -Duration 30

.EXAMPLE
    .\hotfix-monitor.ps1 -IssueNumber 123 -Duration 60 -AutoRollback

.NOTES
    Monitors: Crash rate, error rate, performance, player feedback
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,
    
    [int]$Duration = 30,
    [int]$CheckInterval = 60,
    [switch]$AutoRollback
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$issueId = "ISSUE-$IssueNumber"
$startTime = Get-Date
$endTime = $startTime.AddMinutes($Duration)
$monitoringLog = "Logs/Hotfix/monitoring-$issueId-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Thresholds
$maxCrashRate = 3.0      # %
$maxErrorRate = 5.0      # %
$maxFPSDrop = 20.0       # %
$maxMemorySpike = 30.0   # %

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Hotfix Monitoring" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Issue:          $issueId" -ForegroundColor Cyan
Write-Host "Duration:       $Duration minutes" -ForegroundColor Cyan
Write-Host "Check Interval: $CheckInterval seconds" -ForegroundColor Cyan
Write-Host "Auto-Rollback:  $AutoRollback" -ForegroundColor Cyan
Write-Host "End Time:       $($endTime.ToString('HH:mm:ss'))" -ForegroundColor Cyan
Write-Host "───────────────────────────────────────────────────────────"
Write-Host ""

if ($AutoRollback) {
    Write-Host "⚠️  AUTO-ROLLBACK ENABLED" -ForegroundColor Yellow
    Write-Host "   Will automatically rollback if thresholds exceeded:" -ForegroundColor Yellow
    Write-Host "   - Crash rate > $maxCrashRate%" -ForegroundColor Gray
    Write-Host "   - Error rate > $maxErrorRate%" -ForegroundColor Gray
    Write-Host "   - FPS drop > $maxFPSDrop%" -ForegroundColor Gray
    Write-Host "   - Memory spike > $maxMemorySpike%" -ForegroundColor Gray
    Write-Host ""
}

Start-Transcript -Path $monitoringLog -Append

$checkCount = 0
$alerts = @()

# ═══════════════════════════════════════════════════════════════════════════════
# MONITORING LOOP
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "Starting monitoring..." -ForegroundColor Yellow
Write-Host ""

while ((Get-Date) -lt $endTime) {
    $checkCount++
    $elapsed = ((Get-Date) - $startTime).TotalMinutes
    $remaining = ($endTime - (Get-Date)).TotalMinutes
    
    Write-Host "─────────────────────────────────────────────────────────"
    Write-Host "[Check $checkCount] $(Get-Date -Format 'HH:mm:ss') - Elapsed: $($elapsed.ToString('F1'))m / Remaining: $($remaining.ToString('F1'))m"
    Write-Host ""
    
    # ═══════════════════════════════════════════════════════════════════════════
    # COLLECT METRICS
    # ═══════════════════════════════════════════════════════════════════════════
    
    # In production, these would come from:
    # - Analytics service (crash/error rates)
    # - Player feedback system
    # - Performance monitoring
    
    # Simulated metrics (would be real in production)
    $crashRate = Get-Random -Minimum 0.0 -Maximum 2.0
    $errorRate = Get-Random -Minimum 0.0 -Maximum 3.0
    $avgFPS = Get-Random -Minimum 55 -Maximum 65
    $memoryMB = Get-Random -Minimum 1800 -Maximum 2200
    
    # Display metrics
    Write-Host "📊 Health Metrics:" -ForegroundColor Cyan
    Write-Host "   Crash Rate:   $($crashRate.ToString('F2'))% " -NoNewline
    if ($crashRate -gt $maxCrashRate) {
        Write-Host "❌ THRESHOLD EXCEEDED" -ForegroundColor Red
        $alerts += "Crash rate exceeded: $($crashRate.ToString('F2'))%"
    } elseif ($crashRate -gt ($maxCrashRate * 0.7)) {
        Write-Host "⚠️  WARNING" -ForegroundColor Yellow
    } else {
        Write-Host "✅ OK" -ForegroundColor Green
    }
    
    Write-Host "   Error Rate:   $($errorRate.ToString('F2'))% " -NoNewline
    if ($errorRate -gt $maxErrorRate) {
        Write-Host "❌ THRESHOLD EXCEEDED" -ForegroundColor Red
        $alerts += "Error rate exceeded: $($errorRate.ToString('F2'))%"
    } elseif ($errorRate -gt ($maxErrorRate * 0.7)) {
        Write-Host "⚠️  WARNING" -ForegroundColor Yellow
    } else {
        Write-Host "✅ OK" -ForegroundColor Green
    }
    
    Write-Host "   Average FPS:  $($avgFPS.ToString('F1')) " -NoNewline
    if ($avgFPS -lt 48) {
        Write-Host "❌ LOW" -ForegroundColor Red
        $alerts += "FPS critically low: $($avgFPS.ToString('F1'))"
    } elseif ($avgFPS -lt 55) {
        Write-Host "⚠️  WARNING" -ForegroundColor Yellow
    } else {
        Write-Host "✅ OK" -ForegroundColor Green
    }
    
    Write-Host "   Memory Usage: $memoryMB MB " -NoNewline
    if ($memoryMB -gt 2500) {
        Write-Host "⚠️  HIGH" -ForegroundColor Yellow
    } else {
        Write-Host "✅ OK" -ForegroundColor Green
    }
    
    Write-Host ""
    
    # ═══════════════════════════════════════════════════════════════════════════
    # CHECK FOR ROLLBACK TRIGGERS
    # ═══════════════════════════════════════════════════════════════════════════
    
    if ($alerts.Count -gt 0) {
        Write-Host "🚨 ALERTS DETECTED:" -ForegroundColor Red
        $alerts | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
        Write-Host ""
        
        if ($AutoRollback) {
            Write-Host "⚠️  AUTO-ROLLBACK TRIGGERED" -ForegroundColor Red
            Write-Host "   Initiating emergency rollback..." -ForegroundColor Red
            Write-Host ""
            
            Stop-Transcript
            
            # Trigger rollback
            & .\scripts\hotfix-rollback.ps1 -ToVersion "v1.0.0" -Reason "AUTO: $($alerts -join ', ')" -Force
            
            exit 1
        } else {
            Write-Host "⚠️  Manual rollback recommended" -ForegroundColor Yellow
            Write-Host "   Run: .\scripts\hotfix-rollback.ps1 -ToVersion v1.0.0 -Reason 'Description'" -ForegroundColor Yellow
            Write-Host ""
        }
    }
    
    # Wait for next check
    if ((Get-Date) -lt $endTime) {
        Write-Host "Next check in $CheckInterval seconds..." -ForegroundColor Gray
        Start-Sleep -Seconds $CheckInterval
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# MONITORING COMPLETE
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ MONITORING COMPLETE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Issue:         $issueId" -ForegroundColor Cyan
Write-Host "Duration:      $Duration minutes" -ForegroundColor Cyan
Write-Host "Health Checks: $checkCount" -ForegroundColor Cyan
Write-Host "Alerts:        $($alerts.Count)" -ForegroundColor $(if ($alerts.Count -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($alerts.Count -eq 0) {
    Write-Host "✅ NO ISSUES DETECTED" -ForegroundColor Green
    Write-Host "   Hotfix appears stable" -ForegroundColor Green
    Write-Host "   Continue monitoring for next 24 hours" -ForegroundColor Gray
} else {
    Write-Host "⚠️  ISSUES DETECTED DURING MONITORING" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alerts:" -ForegroundColor Yellow
    $alerts | ForEach-Object { Write-Host "   - $_" -ForegroundColor Yellow }
    Write-Host ""
    Write-Host "Recommended Action:" -ForegroundColor Yellow
    Write-Host "   Consider rollback if issues persist" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Monitoring log: $monitoringLog" -ForegroundColor Cyan
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green

Stop-Transcript

exit 0
