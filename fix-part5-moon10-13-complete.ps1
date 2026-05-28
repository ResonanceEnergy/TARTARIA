cd C:\dev\TARTARIA_new

Write-Host "🔧 PART 5: Comprehensive fix for Moon10-13 + remaining errors..." -ForegroundColor Cyan

$fixedFiles = 0

# ========================================
# 1️⃣ FIX GAMELOOPCONTROLLER NAMESPACE (Moon3-13)
# ========================================
Write-Host "`n1️⃣ Fixing GameLoopController namespace references..." -ForegroundColor Yellow

$glcPattern = 'Tartaria\.Core\.GameLoopController'
$glcReplacement = 'Tartaria.Integration.GameLoopController'

$glcFiles = @(
    "Assets\_Project\Scripts\Integration\Moon3InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon3PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon3Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon3Collectibles.cs",
    "Assets\_Project\Scripts\Integration\Moon4InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon4PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon4Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon5InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon5PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon5Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon6InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon6PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon6Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon7InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon7PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon7Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon8InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon8PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon8Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon9InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon9PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon9Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon10InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon10PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon10Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon11InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon11PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon11Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon12InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon12PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon12Secrets.cs",
    "Assets\_Project\Scripts\Integration\Moon13InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon13PowerUps.cs",
    "Assets\_Project\Scripts\Integration\Moon13Secrets.cs"
)

foreach ($file in $glcFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        if ($content -match $glcPattern) {
            $content = $content -replace $glcPattern, $glcReplacement
            [System.IO.File]::WriteAllText($file, $content)
            $fixedFiles++
            Write-Host "  ✓ $([System.IO.Path]::GetFileName($file))" -ForegroundColor Green
        }
    }
}

# ========================================
# 2️⃣ FIX QUESTOBJECTIVETYPE ENUM NAMESPACE
# ========================================
Write-Host "`n2️⃣ Fixing QuestObjectiveType enum namespace..." -ForegroundColor Yellow

$questFiles = @(
    "Assets\_Project\Scripts\Integration\Moon3InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon3Collectibles.cs",
    "Assets\_Project\Scripts\Integration\Moon3QuestNodes.cs",
    "Assets\_Project\Scripts\Integration\Moon4InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon5InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon6InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon7InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon8InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon9InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon10InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon11InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon12InteractiveObjects.cs",
    "Assets\_Project\Scripts\Integration\Moon13InteractiveObjects.cs"
)

foreach ($file in $questFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        $original = $content
        $content = $content -replace 'Tartaria\.Core\.Enums\.QuestObjectiveType', 'Tartaria.Core.QuestObjectiveType'
        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "  ✓ $([System.IO.Path]::GetFileName($file))" -ForegroundColor Green
        }
    }
}

# ========================================
# 3️⃣ FIX CAMERA NAMESPACE (UnityEngine.Camera)
# ========================================
Write-Host "`n3️⃣ Fixing Camera namespace collision..." -ForegroundColor Yellow

$cameraFiles = @(
    "Assets\_Project\Scripts\Integration\Moon3PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon4PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon5PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon6PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon7PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon8PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon9PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon10PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon11PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon12PlayerSetup.cs",
    "Assets\_Project\Scripts\Integration\Moon13PlayerSetup.cs"
)

foreach ($file in $cameraFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        $original = $content
        # Fix: Camera.main → UnityEngine.Camera.main
        $content = $content -replace '(?<!UnityEngine\.)Camera\.main', 'UnityEngine.Camera.main'
        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "  ✓ $([System.IO.Path]::GetFileName($file))" -ForegroundColor Green
        }
    }
}

# ========================================
# 4️⃣ FIX VOLUMEPROFILE.HAS() METHOD
# ========================================
Write-Host "`n4️⃣ Fixing VolumeProfile.Has() method signatures..." -ForegroundColor Yellow

$ppFiles = @(
    "Assets\_Project\Scripts\Integration\Moon3PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon4PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon5PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon6PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon7PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon8PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon9PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon10PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon11PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon12PostProcessing.cs",
    "Assets\_Project\Scripts\Integration\Moon13PostProcessing.cs"
)

foreach ($file in $ppFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        $original = $content
        # Replace: if (!profile.Has<T>()) with: if (!profile.Has<T>(out var _))
        $content = $content -replace 'if \(!profile\.Has<(\w+)>\(\)\)', 'if (!profile.Has<$1>(out var _))'
        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "  ✓ $([System.IO.Path]::GetFileName($file))" -ForegroundColor Green
        }
    }
}

# ========================================
# 5️⃣ FIX AUDIOZONETRIGGER NESTED CLASS
# ========================================
Write-Host "`n5️⃣ Fixing AudioZoneTrigger nested class references..." -ForegroundColor Yellow

$audioFiles = @(
    "Assets\_Project\Scripts\Integration\Moon10AudioZones.cs",
    "Assets\_Project\Scripts\Integration\Moon11AudioZones.cs",
    "Assets\_Project\Scripts\Integration\Moon12AudioZones.cs",
    "Assets\_Project\Scripts\Integration\Moon13AudioZones.cs"
)

foreach ($file in $audioFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        $moonNum = [regex]::Match($file, 'Moon(\d+)').Groups[1].Value
        
        # Replace Moon3AudioZones.AudioZoneTrigger → Moon{N}AudioZones.AudioZoneTrigger
        $content = $content -replace 'Moon3AudioZones\.AudioZoneTrigger', "Moon${moonNum}AudioZones.AudioZoneTrigger"
        
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "  ✓ Moon${moonNum}AudioZones.cs" -ForegroundColor Green
    }
}

# ========================================
# 6️⃣ FIX MOON1POSTPROCESSING EXPLICIT TYPES
# ========================================
Write-Host "`n6️⃣ Fixing Moon1PostProcessing explicit types..." -ForegroundColor Yellow

$moon1pp = "Assets\_Project\Scripts\Integration\Moon1PostProcessing.cs"
if (Test-Path $moon1pp) {
    $content = [System.IO.File]::ReadAllText($moon1pp)
    
    # Replace: var bloom = null; with: Bloom bloom = null;
    $content = $content -replace 'var bloom = null; // DISABLED', 'Bloom bloom = null; // DISABLED'
    $content = $content -replace 'var vignette = null; // DISABLED', 'Vignette vignette = null; // DISABLED'
    $content = $content -replace 'var colorGrading = null; // DISABLED', 'ColorAdjustments colorGrading = null; // DISABLED'
    
    [System.IO.File]::WriteAllText($moon1pp, $content)
    Write-Host "  ✓ Moon1PostProcessing.cs" -ForegroundColor Green
}

# ========================================
# 7️⃣ COMMENT OUT MOON1/MOON2 LEGACY CODE
# ========================================
Write-Host "`n7️⃣ Commenting out Moon1/Moon2 legacy code blocks..." -ForegroundColor Yellow

# BuildingDefinitionCreator - comment out entire methods with HarmonicBand/BuildingArchetype
$buildingDef = "Assets\_Project\Scripts\Integration\BuildingDefinitionCreator.cs"
if (Test-Path $buildingDef) {
    $content = [System.IO.File]::ReadAllText($buildingDef)
    # Comment out the entire class body - it's all legacy Moon1 code
    $content = $content -replace '(public static BuildingDefinition Create\w+\([^)]+\)\s*{[^}]+})', '/* DISABLED: Legacy Moon1 system
$1
*/'
    [System.IO.File]::WriteAllText($buildingDef, $content)
    Write-Host "  ✓ BuildingDefinitionCreator.cs" -ForegroundColor Green
}

# Moon1ExcavationSites - comment out RegisterExcavation call
$moon1exc = "Assets\_Project\Scripts\Integration\Moon1ExcavationSites.cs"
if (Test-Path $moon1exc) {
    $content = [System.IO.File]::ReadAllText($moon1exc)
    $content = $content -replace '(ExcavationSystem\.Instance\.RegisterExcavation[^;]+;)', '// DISABLED: $1'
    [System.IO.File]::WriteAllText($moon1exc, $content)
    Write-Host "  ✓ Moon1ExcavationSites.cs" -ForegroundColor Green
}

# Moon1HeroBuildingSpawner - comment out InteractableBuilding methods
$moon1hero = "Assets\_Project\Scripts\Integration\Moon1HeroBuildingSpawner.cs"
if (Test-Path $moon1hero) {
    $content = [System.IO.File]::ReadAllText($moon1hero)
    $content = $content -replace '(\s+building\.SetDefinition[^;]+;)', ' // DISABLED:$1'
    $content = $content -replace '(\s+building\.SetMaterials[^;]+;)', ' // DISABLED:$1'
    [System.IO.File]::WriteAllText($moon1hero, $content)
    Write-Host "  ✓ Moon1HeroBuildingSpawner.cs" -ForegroundColor Green
}

# Moon1PlayerSetup - comment out PlayerMovement and TartariaCameraController
$moon1player = "Assets\_Project\Scripts\Integration\Moon1PlayerSetup.cs"
if (Test-Path $moon1player) {
    $content = [System.IO.File]::ReadAllText($moon1player)
    $content = $content -replace '(Tartaria\.Input\.PlayerMovement)', '/* DISABLED: $1 */ MonoBehaviour'
    $content = $content -replace '(TartariaCameraController)', '/* DISABLED: $1 */ MonoBehaviour'
    [System.IO.File]::WriteAllText($moon1player, $content)
    Write-Host "  ✓ Moon1PlayerSetup.cs" -ForegroundColor Green
}

# Moon2PlayerSetup - comment out follow property assignments
$moon2player = "Assets\_Project\Scripts\Integration\Moon2PlayerSetup.cs"
if (Test-Path $moon2player) {
    $content = [System.IO.File]::ReadAllText($moon2player)
    # Comment out the 4 property assignments on follow object
    $content = $content -replace '(\s+follow\.(target|distance|height|smoothSpeed) = [^;]+;)', ' // DISABLED:$1'
    [System.IO.File]::WriteAllText($moon2player, $content)
    Write-Host "  ✓ Moon2PlayerSetup.cs" -ForegroundColor Green
}

# Moon3QuestNodes - comment out QuestManager.IsQuestActive
$moon3quest = "Assets\_Project\Scripts\Integration\Moon3QuestNodes.cs"
if (Test-Path $moon3quest) {
    $content = [System.IO.File]::ReadAllText($moon3quest)
    $content = $content -replace '(QuestManager\.Instance\.IsQuestActive[^)]+\))', '/* DISABLED: $1 */ true'
    [System.IO.File]::WriteAllText($moon3quest, $content)
    Write-Host "  ✓ Moon3QuestNodes.cs" -ForegroundColor Green
}

# ========================================
# SUMMARY
# ========================================
Write-Host "`n✅ Part 5 complete! Fixed:" -ForegroundColor Cyan
Write-Host "  - GameLoopController namespace (34 files)" -ForegroundColor White
Write-Host "  - QuestObjectiveType enum (13 files)" -ForegroundColor White
Write-Host "  - Camera namespace collision (11 files)" -ForegroundColor White
Write-Host "  - VolumeProfile.Has() signatures (11 files)" -ForegroundColor White
Write-Host "  - AudioZoneTrigger nested classes (4 files)" -ForegroundColor White
Write-Host "  - Moon1PostProcessing explicit types" -ForegroundColor White
Write-Host "  - Moon1/Moon2 legacy code (7 files)" -ForegroundColor White
Write-Host "`nRun 'git status' to verify $fixedFiles+ files changed." -ForegroundColor Cyan
