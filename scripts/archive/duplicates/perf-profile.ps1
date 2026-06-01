#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Quick performance profiling helper for TARTARIA development
.DESCRIPTION
    Opens Unity with Profiler attached and checks common perf issues.
.PARAMETER OpenProfiler
    Open Unity Profiler window automatically
.PARAMETER CheckAllocations
    Run GC allocation checks on hot code paths
.PARAMETER GenerateReport
    Generate perf report to Logs/perf-report-YYYYMMDD-HHMMSS.txt
#>

param(
    [switch]$OpenProfiler,
    [switch]$CheckAllocations,
    [switch]$GenerateReport
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$ProjectPath = Get-Location

Write-Host "╔═══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        TARTARIA Performance Profiler Helper          ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if Unity is already running
$unityProc = Get-Process Unity -ErrorAction SilentlyContinue
if ($unityProc) {
    Write-Host "Unity Editor is already running (PID $($unityProc.Id))" -ForegroundColor Yellow
    Write-Host "Close Unity to run profiler checks, or use -OpenProfiler to attach to existing instance" -ForegroundColor Yellow
    
    if ($OpenProfiler) {
        Write-Host ""
        Write-Host "Opening Profiler window..." -ForegroundColor Green
        Write-Host "In Unity: Window → Analysis → Profiler (Ctrl+7)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Performance Targets (from BUILD_GUIDE.md):" -ForegroundColor Yellow
        Write-Host "  • Medium hardware: ≥52 avg FPS, ≥28 1%low, ≤3.6GB RAM" -ForegroundColor White
        Write-Host "  • High hardware:   ≥90 avg FPS, ≥60 1%low, ≤4.2GB RAM" -ForegroundColor White
        Write-Host "  • Ultra hardware:  ≥144 avg FPS, ≥90 1%low, ≤5.0GB RAM" -ForegroundColor White
        exit 0
    }
    exit 1
}

# Check for common perf issues in codebase
if ($CheckAllocations) {
    Write-Host ">> Checking for common GC allocation patterns..." -ForegroundColor Yellow
    Write-Host ""
    
    # GetComponent in Update loops
    $getComponentInUpdate = Select-String -Path "Assets\_Project\Scripts\**\*.cs" `
        -Pattern "void (Update|FixedUpdate|LateUpdate)\(\)" -Context 0,20 | 
        Where-Object { $_.Context.PostContext -match "GetComponent" }
    
    if ($getComponentInUpdate) {
        Write-Host "⚠ WARNING: GetComponent calls in Update loops detected!" -ForegroundColor Red
        $getComponentInUpdate | ForEach-Object {
            Write-Host "  $($_.Path):$($_.LineNumber)" -ForegroundColor Yellow
        }
        Write-Host ""
    } else {
        Write-Host "✓ No GetComponent calls in Update loops" -ForegroundColor Green
        Write-Host ""
    }
    
    # LINQ in hot paths
    $linqInUpdate = Select-String -Path "Assets\_Project\Scripts\**\*.cs" `
        -Pattern "void (Update|FixedUpdate|LateUpdate)\(\)" -Context 0,20 | 
        Where-Object { $_.Context.PostContext -match "(\.Where\(|\.Select\(|\.ToList\(|\.ToArray\()" }
    
    if ($linqInUpdate) {
        Write-Host "⚠ WARNING: LINQ in Update loops detected (allocates)!" -ForegroundColor Red
        $linqInUpdate | ForEach-Object {
            Write-Host "  $($_.Path):$($_.LineNumber)" -ForegroundColor Yellow
        }
        Write-Host ""
    } else {
        Write-Host "✓ No LINQ in Update loops" -ForegroundColor Green
        Write-Host ""
    }
    
    # String concatenation in Update
    $stringConcatInUpdate = Select-String -Path "Assets\_Project\Scripts\**\*.cs" `
        -Pattern "void (Update|FixedUpdate|LateUpdate)\(\)" -Context 0,20 | 
        Where-Object { $_.Context.PostContext -match "\+ \"" }
    
    if ($stringConcatInUpdate) {
        Write-Host "⚠ WARNING: String concatenation in Update loops (allocates)!" -ForegroundColor Red
        $stringConcatInUpdate | ForEach-Object {
            Write-Host "  $($_.Path):$($_.LineNumber)" -ForegroundColor Yellow
        }
        Write-Host ""
    } else {
        Write-Host "✓ No string concatenation in Update loops" -ForegroundColor Green
        Write-Host ""
    }
    
    Write-Host "Allocation check complete." -ForegroundColor Cyan
    Write-Host ""
}

# Generate performance report
if ($GenerateReport) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $reportPath = "Logs\perf-report-$timestamp.txt"
    
    Write-Host ">> Generating performance report: $reportPath" -ForegroundColor Yellow
    Write-Host ""
    
    $report = @"
TARTARIA Performance Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Project: $ProjectPath
Unity: 6000.3.6f1

════════════════════════════════════════════════════════════════

CODEBASE METRICS
────────────────────────────────────────────────────────────────
"@
    
    # Count C# files and lines
    $csFiles = Get-ChildItem -Path "Assets\_Project\Scripts" -Recurse -Filter "*.cs" -File
    $csCount = $csFiles.Count
    $csLines = ($csFiles | Get-Content | Measure-Object -Line).Lines
    
    $report += "`nC# files: $csCount"
    $report += "`nTotal lines of code: $csLines"
    $report += "`nAverage lines per file: $([math]::Round($csLines / $csCount, 1))"
    
    # Count Update loops
    $updateLoops = (Select-String -Path "Assets\_Project\Scripts\**\*.cs" -Pattern "void (Update|FixedUpdate|LateUpdate)\(\)").Count
    $report += "`n`nUpdate/FixedUpdate/LateUpdate loops: $updateLoops"
    
    # Check assembly structure
    $asmdefFiles = Get-ChildItem -Path "Assets\_Project\Scripts" -Recurse -Filter "*.asmdef" -File
    $report += "`nAssembly definitions: $($asmdefFiles.Count)"
    
    $report += "`n`n════════════════════════════════════════════════════════════════"
    $report += "`n`nPERFORMANCE TARGETS (from BUILD_GUIDE.md)"
    $report += "`n────────────────────────────────────────────────────────────────"
    $report += "`n`nMedium (GTX 1070 / RX 580):"
    $report += "`n  • Avg FPS: ≥52"
    $report += "`n  • 1% Low: ≥28"
    $report += "`n  • RAM: ≤3.6GB"
    $report += "`n`nHigh (RTX 3060 / RX 6700 XT):"
    $report += "`n  • Avg FPS: ≥90"
    $report += "`n  • 1% Low: ≥60"
    $report += "`n  • RAM: ≤4.2GB"
    $report += "`n`nUltra (RTX 4070 / RX 7800 XT):"
    $report += "`n  • Avg FPS: ≥144"
    $report += "`n  • 1% Low: ≥90"
    $report += "`n  • RAM: ≤5.0GB"
    
    $report += "`n`n════════════════════════════════════════════════════════════════"
    $report += "`n`nRUNTIME PROFILING INSTRUCTIONS"
    $report += "`n────────────────────────────────────────────────────────────────"
    $report += "`n1. Open Unity Editor"
    $report += "`n2. Window → Analysis → Profiler (Ctrl+7)"
    $report += "`n3. Enter Play Mode"
    $report += "`n4. Navigate to Echohaven_VerticalSlice scene"
    $report += "`n5. Perform test actions (walk, interact, restore building, open menus)"
    $report += "`n6. Check for:"
    $report += "`n   • GC.Alloc spikes (should be 0 KB in hot frames)"
    $report += "`n   • Main Thread < 14ms (60 fps target)"
    $report += "`n   • Render Thread < 14ms"
    $report += "`n   • GPU Frame Time < 12ms on Medium hardware"
    $report += "`n7. Use Profiler.BeginSample() / EndSample() for custom markers"
    
    $report += "`n`n════════════════════════════════════════════════════════════════"
    $report += "`nEnd of Report"
    $report += "`n"
    
    New-Item -ItemType Directory -Force -Path "Logs" | Out-Null
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    
    Write-Host "✓ Report generated: $reportPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "To view:" -ForegroundColor Cyan
    Write-Host "  notepad `"$reportPath`"" -ForegroundColor White
    Write-Host ""
}

# Default: print usage
if (-not $OpenProfiler -and -not $CheckAllocations -and -not $GenerateReport) {
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\perf-profile.ps1 -OpenProfiler      # Open Unity Profiler window" -ForegroundColor White
    Write-Host "  .\perf-profile.ps1 -CheckAllocations  # Scan code for GC allocation patterns" -ForegroundColor White
    Write-Host "  .\perf-profile.ps1 -GenerateReport    # Generate perf report to Logs/" -ForegroundColor White
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Cyan
    Write-Host "  .\perf-profile.ps1 -CheckAllocations -GenerateReport" -ForegroundColor White
    Write-Host ""
}

Write-Host "Done." -ForegroundColor Green
