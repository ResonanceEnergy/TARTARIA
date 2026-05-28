# TARTARIA — Proper Moon System Builder
# Standardizes all 13 Moons to 23-system architecture

$moonSystems = @(
    "AmbientAudio", "AmbientCreatures", "AmbientParticles", "AudioZones",
    "Collectibles", "ContentSpawner", "DynamicHazards", "EnemySpawners",
    "EnvironmentDecorator", "InteractiveObjects", "LevelBuilder", "LightingSetup",
    "MaterialSetup", "NPCDialogues", "NPCSpawner", "PlayerSetup",
    "PostProcessing", "PowerUps", "QuestNodes", "SceneMaster",
    "Secrets", "VisualLandmarks", "WeatherSystem"
)

Write-Host "=== MOON SYSTEM AUDIT ===" -ForegroundColor Cyan

1..13 | ForEach-Object {
    $moon = $_
    Write-Host "`nMoon $moon Status:" -ForegroundColor Yellow
    
    $existing = Get-ChildItem "Assets\_Project\Scripts\Integration" -Filter "*.cs" | 
        Where-Object { $_.Name -match "^Moon$moon[^0-9]" }
    
    Write-Host "  Existing: $($existing.Count) files"
    
    $missing = $moonSystems | Where-Object {
        $systemName = "Moon$moon$_.cs"
        -not (Test-Path "Assets\_Project\Scripts\Integration\$systemName")
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "  MISSING ($($missing.Count)):" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "    - $_" }
    } else {
        Write-Host "  ✅ COMPLETE" -ForegroundColor Green
    }
}

Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
1..13 | ForEach-Object {
    $count = (Get-ChildItem "Assets\_Project\Scripts\Integration" -Filter "*.cs" | 
        Where-Object { $_.Name -match "^Moon$_[^0-9]" }).Count
    $status = if ($count -eq 23) { "✅" } else { "❌ $count/23" }
    Write-Host "Moon $_ : $status"
}
