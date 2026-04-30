# TARTARIA Asset Integration Helper
# Opens Unity Editor and provides integration checklist

param(
    [switch]$TestBuild  # Run a quick test build after opening
)

cd C:\dev\TARTARIA_new

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TARTARIA - Manual Asset Integration Mode" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`n✅ IMPORTED ASSETS READY:" -ForegroundColor Green
Write-Host "  • 40 Capoeira animations"
Write-Host "  • Ch14_nonPBR character mesh"
Write-Host "  • Drake Stafford - 432 Hz.mp3"
Write-Host "  • 4 custom shader materials"

Write-Host "`n📋 INTEGRATION CHECKLIST:" -ForegroundColor Yellow
Write-Host "  [ ] Priority 1: Apply Capoeira animations to Player.prefab"
Write-Host "  [ ] Priority 2: Wire 432 Hz music to GameAudioManager"
Write-Host "  [ ] Priority 3: Replace Player capsule with Player_Mesh.fbx"
Write-Host "  [ ] Priority 4: Validate custom shaders on buildings"

Write-Host "`n📖 DETAILED GUIDE:" -ForegroundColor Magenta
Write-Host "  See: docs\INTEGRATION_STATUS.md for step-by-step instructions"

Write-Host "`nOpening Unity Editor..." -ForegroundColor Cyan

# Open Unity Editor (not batch mode - manual GUI)
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.33f1\Editor\Unity.exe"
$projectPath = "C:\dev\TARTARIA_new"

if (-not (Test-Path $unityPath)) {
    Write-Host "❌ Unity not found at: $unityPath" -ForegroundColor Red
    Write-Host "   Opening Unity Hub instead..." -ForegroundColor Yellow
    Start-Process "unityhub://6000.0.33f1/$projectPath"
} else {
    # Open Unity with Echohaven scene
    Start-Process $unityPath -ArgumentList "-projectPath `"$projectPath`" -openscene `"Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`""
}

Write-Host "`n✅ Unity Editor launching..." -ForegroundColor Green
Write-Host "`nWORKFLOW:" -ForegroundColor Cyan
Write-Host "  1. Wait for Unity to load (~30-60s)"
Write-Host "  2. Check Console for any import errors"
Write-Host "  3. Navigate to Assets/_Project/Prefabs/Characters/Player.prefab"
Write-Host "  4. Follow Priority 1 steps in INTEGRATION_STATUS.md"
Write-Host "  5. Test in Play mode (Ctrl+P)"

Write-Host "`n💡 TIP: Focus on ONE priority at a time - test after each change" -ForegroundColor Yellow

if ($TestBuild) {
    Write-Host "`n⏳ Waiting 60s for Unity to load before test build..." -ForegroundColor Yellow
    Start-Sleep -Seconds 60
    
    Write-Host "`nRunning quick test build..." -ForegroundColor Cyan
    # This would trigger automated build - but for manual mode, skip this
    Write-Host "   (Skipped in manual mode - use Play button in Unity instead)" -ForegroundColor Gray
}
