# Unity Editor Reopen Script for Phase 1
# Launches Unity with TARTARIA_new project after recompile

param(
    [Parameter(Mandatory=$false)]
    [switch]$WaitForCompile
)

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = "C:\dev\TARTARIA_new"

Write-Host "`n🎬 UNITY EDITOR LAUNCHER — Phase 1`n" -ForegroundColor Cyan

# Check if Unity is already running
$runningUnity = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($runningUnity) {
    Write-Host "⚠️  Unity Editor is already running (PID: $($runningUnity.Id -join ', '))`n" -ForegroundColor Yellow
    Write-Host "Choose an action:" -ForegroundColor White
    Write-Host "  1. Kill existing Unity and restart" -ForegroundColor Gray
    Write-Host "  2. Continue with existing Unity instance" -ForegroundColor Gray
    Write-Host "  3. Cancel`n" -ForegroundColor Gray
    
    $choice = Read-Host "Enter choice (1-3)"
    
    switch ($choice) {
        "1" {
            Write-Host "`nKilling Unity processes..." -ForegroundColor Yellow
            $runningUnity | Stop-Process -Force
            Start-Sleep -Seconds 3
            Write-Host "✅ Unity closed`n" -ForegroundColor Green
        }
        "2" {
            Write-Host "`n✅ Continuing with existing Unity instance`n" -ForegroundColor Green
            Write-Host "⚠️  Make sure to check Console for recompile status!`n" -ForegroundColor Yellow
            exit 0
        }
        "3" {
            Write-Host "`nCancelled.`n" -ForegroundColor Gray
            exit 0
        }
        default {
            Write-Host "`nInvalid choice. Exiting.`n" -ForegroundColor Red
            exit 1
        }
    }
}

# Launch Unity
Write-Host "🚀 Launching Unity Editor..." -ForegroundColor Green
Write-Host "   Project: $projectPath" -ForegroundColor Gray
Write-Host "   Unity:   $unityPath`n" -ForegroundColor Gray

Start-Process -FilePath $unityPath -ArgumentList "-projectPath", "`"$projectPath`""

if ($WaitForCompile) {
    Write-Host "⏳ Waiting for Unity to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    Write-Host "✅ Unity should be opening now`n" -ForegroundColor Green
    Write-Host "WATCH FOR:" -ForegroundColor Cyan
    Write-Host "  • Bottom-right: 'Compiling scripts...' progress bar" -ForegroundColor White
    Write-Host "  • Wait until spinning icon disappears (30-60 seconds)" -ForegroundColor White
    Write-Host "  • Check Console for 0 errors ✅`n" -ForegroundColor White
}

Write-Host "🎯 PHASE 1 TASKS (After recompile):" -ForegroundColor Magenta
Write-Host "   1. Verify Edit Mode (press Ctrl+P if in Play Mode)" -ForegroundColor White
Write-Host "   2. Tools → TARTARIA → Build Moon Scene" -ForegroundColor White
Write-Host "      → Moon Number: 1, Name: MagneticMoon, Size: 500" -ForegroundColor Gray
Write-Host "   3. Open: Tools\Phase1\PREFAB_CREATION_CHECKLIST.md" -ForegroundColor White
Write-Host "   4. Build 18 cathedral prefabs (4-5 hours)" -ForegroundColor White
Write-Host "   5. Create 3 master Shader Graphs (2-3 hours)" -ForegroundColor White
Write-Host "   6. Assemble cathedral in Moon1_MagneticMoon.unity`n" -ForegroundColor White

Write-Host "📚 REFERENCE DOCS:" -ForegroundColor Yellow
Write-Host "   • PHASE_1_MOON_1_EXECUTION.md (280 lines, full guide)" -ForegroundColor Gray
Write-Host "   • Tools\Phase1\GoldenRatioCalculator.ps1 (measurements)" -ForegroundColor Gray
Write-Host "   • Tools\Phase1\Cathedral_Measurements.csv (exact dimensions)" -ForegroundColor Gray
Write-Host "   • Tools\Phase1\PREFAB_CREATION_CHECKLIST.md (18 prefabs)" -ForegroundColor Gray
Write-Host "   • Tools\Phase1\MaterialPresets.json (Shader Graph configs)`n" -ForegroundColor Gray

Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan