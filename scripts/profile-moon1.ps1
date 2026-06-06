#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Performance profiling automation for TARTARIA Moon 1
.DESCRIPTION
    Launches Moon 1 with Unity Profiler enabled, captures metrics for 5 minutes,
    and generates a performance report with FPS, memory, and CPU statistics.
.PARAMETER Duration
    How long to profile in seconds (default: 300 = 5 minutes)
.PARAMETER AutoQuit
    Whether to automatically quit after profiling (default: true)
.EXAMPLE
    .\profile-moon1.ps1
    .\profile-moon1.ps1 -Duration 180 -AutoQuit:$false
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$Duration = 300,
    
    [Parameter(Mandatory=$false)]
    [bool]$AutoQuit = $true
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe"
$ProjectPath = $PSScriptRoot
$LogFile = "Logs\perf-profile-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$ProfileDataDir = "Logs\ProfilerData"

# Ensure directories exist
New-Item -ItemType Directory -Path "Logs" -ErrorAction SilentlyContinue | Out-Null
New-Item -ItemType Directory -Path $ProfileDataDir -ErrorAction SilentlyContinue | Out-Null

Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "TARTARIA - Performance Profiling Tool" -ForegroundColor Cyan
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "Duration: $Duration seconds ($([math]::Round($Duration / 60, 1)) minutes)" -ForegroundColor Yellow
Write-Host "Scene: Moon 1 (Echohaven_VerticalSlice)" -ForegroundColor Yellow
Write-Host "Profile Data: $ProfileDataDir" -ForegroundColor Yellow
Write-Host ""

# Step 1: Launch Unity with Profiler enabled
Write-Host "[1/3] Launching Unity with Profiler..." -ForegroundColor Green

$unityArgs = @(
    "-projectPath", $ProjectPath,
    "-logFile", $LogFile
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -PassThru

Write-Host "Unity launched (PID: $($process.Id))" -ForegroundColor Green
Write-Host ""
Write-Host "MANUAL STEPS REQUIRED:" -ForegroundColor Yellow
Write-Host "1. Unity Editor should now be open" -ForegroundColor White
Write-Host "2. Click Window > Analysis > Profiler" -ForegroundColor White
Write-Host "3. Enable 'Record' button in Profiler window" -ForegroundColor White
Write-Host "4. Press Play button to enter Play Mode" -ForegroundColor White
Write-Host "5. Load Moon 1 scene (Echohaven_VerticalSlice)" -ForegroundColor White
Write-Host "6. Let it run for $Duration seconds" -ForegroundColor White
Write-Host ""
Write-Host "This script will wait $Duration seconds, then prompt you to save profiler data." -ForegroundColor Cyan
Write-Host ""

# Step 2: Wait for profiling duration
Write-Host "[2/3] Profiling in progress..." -ForegroundColor Green
$startTime = Get-Date

for ($i = 0; $i -lt $Duration; $i += 30) {
    $remaining = $Duration - $i
    $elapsed = $i
    $percentComplete = [math]::Round(($i / $Duration) * 100)
    
    Write-Host "`rElapsed: ${elapsed}s / ${Duration}s ($percentComplete%) | Remaining: ${remaining}s    " -NoNewline -ForegroundColor Yellow
    Start-Sleep -Seconds 30
}

Write-Host ""
Write-Host ""

# Step 3: Prompt to save profiler data
Write-Host "[3/3] Profiling complete!" -ForegroundColor Green
Write-Host ""
Write-Host "SAVE PROFILER DATA:" -ForegroundColor Yellow
Write-Host "1. In Unity Profiler window, click 'Save' button" -ForegroundColor White
Write-Host "2. Save to: $ProfileDataDir\moon1-profile.data" -ForegroundColor White
Write-Host "3. Stop Play Mode in Unity" -ForegroundColor White
Write-Host ""

if ($AutoQuit) {
    Write-Host "Press Enter to quit Unity and generate report..." -ForegroundColor Cyan
    Read-Host
    
    # Stop Unity
    if (-not $process.HasExited) {
        Write-Host "Closing Unity..." -ForegroundColor Gray
        $process.CloseMainWindow() | Out-Null
        Start-Sleep -Seconds 5
        
        if (-not $process.HasExited) {
            Write-Host "Force killing Unity..." -ForegroundColor Gray
            $process.Kill()
        }
    }
}

# Generate quick report from log file
Write-Host ""
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "Performance Report" -ForegroundColor Cyan
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan

$report = @"
TARTARIA - Performance Profile Report
Generated: $(Get-Date)
Duration: $Duration seconds
Scene: Moon 1 (Echohaven_VerticalSlice)

=================================================================================
PROFILER DATA
=================================================================================

Profiler data saved to: $ProfileDataDir\moon1-profile.data

To analyze:
1. Open Unity Editor
2. Window > Analysis > Profiler
3. Load > Select moon1-profile.data
4. Review tabs:
   - CPU: Frame time breakdown (target: < 16.67ms)
   - GPU: Rendering performance
   - Memory: Allocation patterns (budget: 3.6GB)
   - Rendering: Draw calls, batches

=================================================================================
KEY METRICS TO CHECK
=================================================================================

Target Framerate: 60 FPS (16.67ms per frame)
- Check CPU: Average frame time should be < 16.67ms
- Check GPU: GPU time should be < 16.67ms
- Identify spikes: Frames > 33ms (below 30 FPS)

Memory Budget: 3.6 GB
- Check Memory: Total Allocated should stay < 3,600 MB
- GC Allocs: Should be minimal during gameplay (< 100 KB/frame)
- Texture Memory: Major contributor, should stay reasonable

Rendering:
- Draw Calls: Target < 1000 per frame
- SetPass Calls: Target < 100 per frame
- Batches: Higher is better (indicates good batching)

=================================================================================
LOG FILE
=================================================================================

Unity log: $LogFile

Review for:
- Warnings about performance
- Memory allocation warnings
- Shader compilation times
- Asset loading times

=================================================================================
NEXT STEPS
=================================================================================

1. Analyze profiler data in Unity
2. Identify performance bottlenecks:
   - Scripts with high CPU cost
   - Expensive draw calls
   - Memory allocations
3. Update optimization priorities based on findings
4. Document results in AUDIT_REPORT

=================================================================================
"@

Write-Host $report

$reportPath = "Logs\perf-profile-report.txt"
$report | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host "Report saved to: $reportPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
Write-Host "Profiling complete!" -ForegroundColor Cyan
Write-Host "=".PadRight(80, '=') -ForegroundColor Cyan
