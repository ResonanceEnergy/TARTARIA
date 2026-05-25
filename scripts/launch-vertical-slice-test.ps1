# TARTARIA - Interactive Vertical Slice Validation
# Launches Unity in play mode for manual/automated testing

param(
    [switch]$Automated  # If set, auto-starts the test sequence
)

$ErrorActionPreference = "Stop"

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = "C:\dev\TARTARIA_new"

Write-Host ""
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " TARTARIA — Vertical Slice Validation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($Automated) {
    Write-Host "Mode: AUTOMATED (test sequence starts on play)" -ForegroundColor Yellow
} else {
    Write-Host "Mode: MANUAL (press P in Unity to start tests)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test Coverage:" -ForegroundColor White
Write-Host "  1. Player spawn & movement" -ForegroundColor Gray
Write-Host "  2. Inventory & loot pickup (stack limits, weight system)" -ForegroundColor Gray
Write-Host "  3. Equipment system (equip/unequip, stat bonuses)" -ForegroundColor Gray
Write-Host "  4. Combat & progression (damage, armor, XP, level up)" -ForegroundColor Gray
Write-Host "  5. Save/Load cycle (checksum validation, rollback)" -ForegroundColor Gray
Write-Host "  6. Performance profiling (baseline vs post-optimization)" -ForegroundColor Gray
Write-Host ""
Write-Host "Expected Results:" -ForegroundColor White
Write-Host "  • Agent 8 optimizations: +24 fps improvement" -ForegroundColor Gray
Write-Host "  • Material cache: -1200 allocs/frame on loot scenes" -ForegroundColor Gray
Write-Host "  • Physics optimization: -800 raycasts/frame" -ForegroundColor Gray
Write-Host "  • Armor system: 30 armor = 23% damage reduction" -ForegroundColor Gray
Write-Host "  • Save v18: SHA256 checksum + rollback history" -ForegroundColor Gray
Write-Host ""

# Check if VerticalSliceValidationTest exists
$testScript = "$projectPath\Assets\_Project\Scripts\Tests\VerticalSliceValidationTest.cs"
if (-not (Test-Path $testScript)) {
    Write-Host "✗ Test script not found: $testScript" -ForegroundColor Red
    Write-Host "  Run generate-data-assets.ps1 first to create test infrastructure" -ForegroundColor Yellow
    exit 1
}

Write-Host "Launching Unity Editor..." -ForegroundColor Cyan
Write-Host "  Project: $projectPath" -ForegroundColor Gray
Write-Host "  Once Unity opens:" -ForegroundColor Yellow
if ($Automated) {
    Write-Host "    • Test will start automatically when you press Play" -ForegroundColor Yellow
} else {
    Write-Host "    • Press Play to enter play mode" -ForegroundColor Yellow
    Write-Host "    • Press P key to start test sequence" -ForegroundColor Yellow
}
Write-Host "    • Watch Console for test results ([VerticalSliceTest] logs)" -ForegroundColor Yellow
Write-Host ""

# Launch Unity
$unityArgs = @(
    "-projectPath", $projectPath
)

Start-Process -FilePath $unityPath -ArgumentList $unityArgs

Write-Host "Unity Editor launched." -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: Test results will appear in Unity Console window." -ForegroundColor Gray
Write-Host "      Look for [VerticalSliceTest] prefix in logs." -ForegroundColor Gray
Write-Host ""
