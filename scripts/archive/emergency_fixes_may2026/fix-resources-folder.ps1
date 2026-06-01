# fix-resources-folder.ps1 — Relocate imported assets to Resources folder for Unity Resource loading
# Dr. Vex Aurelian (Unity 2100 → 2026 TARTARIA)

Write-Host "`n=== TARTARIA Resources Folder Fix ===" -ForegroundColor Cyan
Write-Host "Relocating imported assets to Assets\_Project\Resources\ for Resource.Load() compatibility`n" -ForegroundColor White

$projectRoot = "C:\dev\TARTARIA_new"
cd $projectRoot

# Source paths (where tartaria-import-assets.ps1 copied files)
$sourceModelsBase = "Assets\_Project\Models\Buildings"
$sourceModularDungeon = "$sourceModelsBase\ModularDungeon2"
$sourceFantasyRuins = "$sourceModelsBase\FantasyRuins"
$sourceKayKitHexagon = "$sourceModelsBase\KayKit_Hexagon"

# Destination paths (where Resources.Load() expects them)
$destResourcesBase = "Assets\_Project\Resources\Models\Buildings"
$destModularDungeon = "$destResourcesBase\ModularDungeon2"
$destFantasyRuins = "$destResourcesBase\FantasyRuins"
$destKayKitHexagon = "$destResourcesBase\KayKit_Hexagon"

# Prefabs destination (for TartariaAssetImporter automation)
$destPrefabsBase = "Assets\_Project\Resources\Prefabs\Buildings"
$destPrefabsModularDungeon = "$destPrefabsBase\ModularDungeon2"
$destPrefabsFantasyRuins = "$destPrefabsBase\FantasyRuins"
$destPrefabsKayKitHexagon = "$destPrefabsBase\KayKit_Hexagon"

# Create destination directories
Write-Host "[1/4] Creating Resources folder structure..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $destModularDungeon -Force | Out-Null
New-Item -ItemType Directory -Path $destFantasyRuins -Force | Out-Null
New-Item -ItemType Directory -Path $destKayKitHexagon -Force | Out-Null
New-Item -ItemType Directory -Path $destPrefabsModularDungeon -Force | Out-Null
New-Item -ItemType Directory -Path $destPrefabsFantasyRuins -Force | Out-Null
New-Item -ItemType Directory -Path $destPrefabsKayKitHexagon -Force | Out-Null

# Move Modular Dungeon assets
if (Test-Path $sourceModularDungeon) {
    Write-Host "[2/4] Moving Modular Dungeon 2 assets (90 OBJ + 90 MTL)..." -ForegroundColor Yellow
    $objFiles = Get-ChildItem -Path $sourceModularDungeon -Filter "*.obj"
    $mtlFiles = Get-ChildItem -Path $sourceModularDungeon -Filter "*.mtl"

    foreach ($file in $objFiles) {
        Move-Item -Path $file.FullName -Destination $destModularDungeon -Force
    }
    foreach ($file in $mtlFiles) {
        Move-Item -Path $file.FullName -Destination $destModularDungeon -Force
    }

    Write-Host "  ✓ Moved $($objFiles.Count) OBJ files" -ForegroundColor Green
    Write-Host "  ✓ Moved $($mtlFiles.Count) MTL files" -ForegroundColor Green
}

# Move Fantasy Ruins assets
if (Test-Path $sourceFantasyRuins) {
    Write-Host "[3/4] Moving Fantasy Ruins assets (12 DAE models)..." -ForegroundColor Yellow
    $daeFiles = Get-ChildItem -Path $sourceFantasyRuins -Recurse -Filter "*.dae" -File

    foreach ($file in $daeFiles) {
        Move-Item -Path $file.FullName -Destination $destFantasyRuins -Force
    }

    Write-Host "  ✓ Moved $($daeFiles.Count) DAE files" -ForegroundColor Green
}

# Move KayKit Hexagon assets
if (Test-Path $sourceKayKitHexagon) {
    Write-Host "[4/4] Moving KayKit Medieval Hexagon assets (18 FBX blue variant)..." -ForegroundColor Yellow
    $fbxFiles = Get-ChildItem -Path $sourceKayKitHexagon -Recurse -Filter "*.fbx" -File

    foreach ($file in $fbxFiles) {
        Move-Item -Path $file.FullName -Destination $destKayKitHexagon -Force
    }

    Write-Host "  ✓ Moved $($fbxFiles.Count) FBX files" -ForegroundColor Green
}

# Clean up empty source directories (if all files moved)
Write-Host "`nCleaning up empty source directories..." -ForegroundColor Yellow
if (Test-Path $sourceModularDungeon) {
    $remaining = Get-ChildItem -Path $sourceModularDungeon -File
    if ($remaining.Count -eq 0) {
        Remove-Item -Path $sourceModularDungeon -Recurse -Force
        Write-Host "  ✓ Removed empty $sourceModularDungeon" -ForegroundColor Green
    }
}
if (Test-Path $sourceFantasyRuins) {
    $remaining = Get-ChildItem -Path $sourceFantasyRuins -Recurse -File
    if ($remaining.Count -eq 0) {
        Remove-Item -Path $sourceFantasyRuins -Recurse -Force
        Write-Host "  ✓ Removed empty $sourceFantasyRuins" -ForegroundColor Green
    }
}
if (Test-Path $sourceKayKitHexagon) {
    $remaining = Get-ChildItem -Path $sourceKayKitHexagon -Recurse -File
    if ($remaining.Count -eq 0) {
        Remove-Item -Path $sourceKayKitHexagon -Recurse -Force
        Write-Host "  ✓ Removed empty $sourceKayKitHexagon" -ForegroundColor Green
    }
}

# Verification
Write-Host "`n=== Verification ===" -ForegroundColor Cyan
$modularCount = (Get-ChildItem -Path $destModularDungeon -File).Count
$ruinsCount = (Get-ChildItem -Path $destFantasyRuins -File).Count
$kayKitCount = (Get-ChildItem -Path $destKayKitHexagon -File).Count

Write-Host "Resources/Models/Buildings/ModularDungeon2: $modularCount files" -ForegroundColor White
Write-Host "Resources/Models/Buildings/FantasyRuins: $ruinsCount files" -ForegroundColor White
Write-Host "Resources/Models/Buildings/KayKit_Hexagon: $kayKitCount files" -ForegroundColor White

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Unity will detect moved files and re-import them (~1-2 minutes)" -ForegroundColor White
Write-Host "2. Run: .\tartaria-play.ps1 -BatchOnly" -ForegroundColor White
Write-Host "3. Check: BuildingSpawner can now load from Resources.Load<GameObject>()" -ForegroundColor White
Write-Host "4. Test: Star Dome should spawn with modular dungeon assets (not primitive sphere)" -ForegroundColor White

Write-Host "`n✓ Resources folder fix complete.`n" -ForegroundColor Green
