# TARTARIA - Pre-Flight Check
# Verifies all assets and scripts are ready before Unity execution

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           TARTARIA - PRE-FLIGHT CHECKLIST                      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check 1: Moon Systems
Write-Host "━━━ MOON SYSTEMS ━━━" -ForegroundColor Yellow
$moonSystems = @(
    "Assets\_Project\Scripts\Integration\Moon1EnemySpawners.cs",
    "Assets\_Project\Scripts\Integration\Moon2EnemySpawners.cs",
    "Assets\_Project\Scripts\Integration\Moon13CosmicArc.cs"
)

foreach ($system in $moonSystems) {
    if (Test-Path $system) {
        Write-Host "  ✅ $(Split-Path $system -Leaf)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $(Split-Path $system -Leaf) MISSING" -ForegroundColor Red
        $allGood = $false
    }
}
Write-Host ""

# Check 2: Editor Tools
Write-Host "━━━ EDITOR TOOLS ━━━" -ForegroundColor Yellow
$editorTools = @(
    "Assets\_Project\Scripts\Editor\PrefabGeneratorTool.cs",
    "Assets\_Project\Scripts\Editor\AutomatedPrefabWiring.cs"
)

foreach ($tool in $editorTools) {
    if (Test-Path $tool) {
        Write-Host "  ✅ $(Split-Path $tool -Leaf)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $(Split-Path $tool -Leaf) MISSING" -ForegroundColor Red
        $allGood = $false
    }
}
Write-Host ""

# Check 3: KayKit Assets
Write-Host "━━━ KAYKIT ASSETS ━━━" -ForegroundColor Yellow
$kaykitDirs = @(
    "Assets\KayKit_Adventurers_2.0_FREE",
    "Assets\KayKit_Skeletons_1.1_FREE",
    "Assets\KayKit_Forest_Nature_Pack_1.0_FREE"
)

foreach ($dir in $kaykitDirs) {
    if (Test-Path $dir) {
        $count = (Get-ChildItem $dir -Recurse -Include "*.glb","*.gltf" -ErrorAction SilentlyContinue).Count
        Write-Host "  ✅ $(Split-Path $dir -Leaf) ($count models)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $(Split-Path $dir -Leaf) MISSING" -ForegroundColor Red
        $allGood = $false
    }
}
Write-Host ""

# Check 4: VFX Assets
Write-Host "━━━ VFX ASSETS ━━━" -ForegroundColor Yellow
if (Test-Path "Assets\Hovl Studio") {
    $vfxCount = (Get-ChildItem "Assets\Hovl Studio" -Recurse -Filter "*.prefab" -ErrorAction SilentlyContinue).Count
    Write-Host "  ✅ Hovl Studio Magic Effects ($vfxCount prefabs)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Hovl Studio not found (VFX will be missing)" -ForegroundColor Yellow
}

if (Test-Path "Assets\EffectExamples") {
    $effectCount = (Get-ChildItem "Assets\EffectExamples" -Recurse -Filter "*.prefab" -ErrorAction SilentlyContinue).Count
    Write-Host "  ✅ Unity Particle Effects ($effectCount prefabs)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  EffectExamples not found" -ForegroundColor Yellow
}
Write-Host ""

# Check 5: Audio
Write-Host "━━━ AUDIO ASSETS ━━━" -ForegroundColor Yellow
if (Test-Path "Assets\_Project\Audio\UI") {
    $uiAudioCount = (Get-ChildItem "Assets\_Project\Audio\UI" -Filter "*.ogg" -ErrorAction SilentlyContinue).Count
    Write-Host "  ✅ UI Audio ($uiAudioCount sounds)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  UI Audio not imported yet" -ForegroundColor Yellow
}

if (Test-Path "Assets\_Project\Audio\Music") {
    Write-Host "  ✅ Music folder exists" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Music folder missing" -ForegroundColor Yellow
}
Write-Host ""

# Check 6: Scenes
Write-Host "━━━ UNITY SCENES ━━━" -ForegroundColor Yellow
if (Test-Path "Assets\_Project\Scenes\Echohaven_VerticalSlice.unity") {
    Write-Host "  ✅ Echohaven_VerticalSlice.unity" -ForegroundColor Green
} else {
    Write-Host "  ❌ Echohaven_VerticalSlice.unity MISSING" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# Check 7: Prefabs Directory
Write-Host "━━━ PREFAB DIRECTORIES ━━━" -ForegroundColor Yellow
$prefabDirs = @(
    "Assets\_Project\Prefabs\Characters",
    "Assets\_Project\Prefabs\Enemies",
    "Assets\_Project\Prefabs\Collectibles"
)

$prefabsExist = $false
foreach ($dir in $prefabDirs) {
    if (Test-Path $dir) {
        $count = (Get-ChildItem $dir -Filter "*.prefab" -Recurse -ErrorAction SilentlyContinue).Count
        if ($count -gt 0) {
            Write-Host "  ✅ $(Split-Path $dir -Leaf) ($count prefabs)" -ForegroundColor Green
            $prefabsExist = $true
        } else {
            Write-Host "  ⏳ $(Split-Path $dir -Leaf) (empty - will generate)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⏳ $(Split-Path $dir -Leaf) (will create)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Final Status
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

if ($allGood) {
    Write-Host "✅ PRE-FLIGHT CHECK PASSED!" -ForegroundColor Green
    Write-Host ""
    Write-Host "All critical assets and scripts are present." -ForegroundColor White
    Write-Host ""
    if (!$prefabsExist) {
        Write-Host "⚠️  PREFABS NOT YET GENERATED" -ForegroundColor Yellow
        Write-Host "   This is expected. Generate them in Unity." -ForegroundColor Gray
        Write-Host ""
    }
    Write-Host "Ready to launch Unity!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Run: .\Launch-Unity.ps1" -ForegroundColor Cyan
} else {
    Write-Host "❌ PRE-FLIGHT CHECK FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Some critical files are missing." -ForegroundColor Yellow
    Write-Host "Review errors above before launching Unity." -ForegroundColor Yellow
}

Write-Host ""
