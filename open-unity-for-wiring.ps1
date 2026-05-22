# Open Unity and Wire Characters
# Opens Unity GUI so you can run the wiring tool via menu

Write-Host "=== OPENING UNITY FOR CHARACTER WIRING ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host "1. Wait for Unity to fully load and compile scripts (~30 seconds)"
Write-Host "2. Menu → Tartaria → Character Wiring → Wire All Characters to KayKit Models"
Write-Host "3. Click 'Wire All Characters' button in the tool window"
Write-Host "4. Wait for completion message"
Write-Host "5. Open Echohaven_VerticalSlice scene and press Play to test"
Write-Host ""
Write-Host "Opening Unity in 3 seconds..." -ForegroundColor Green
Start-Sleep -Seconds 3

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = $PWD.Path

Write-Host "Launching Unity..." -ForegroundColor Gray
Start-Process -FilePath $unityPath -ArgumentList "-projectPath", $projectPath

Write-Host ""
Write-Host "√ Unity launching!" -ForegroundColor Green
Write-Host ""
Write-Host "Refer to KAYKIT_WIRING_GUIDE.md for full instructions" -ForegroundColor Gray
