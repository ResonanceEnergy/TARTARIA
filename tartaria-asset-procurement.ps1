# TARTARIA — Asset Procurement Automation Script
# Automates download of free assets + generates shopping cart for paid assets
# Run: .\tartaria-asset-procurement.ps1 -Tier "Indie" -ExecuteFreeDownloads

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Free","Indie","AA","AAA")]
    [string]$Tier = "Indie",
    
    [Parameter(Mandatory=$false)]
    [switch]$ExecuteFreeDownloads,
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateShoppingList,
    
    [Parameter(Mandatory=$false)]
    [switch]$OpenAssetStore
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " TARTARIA — 13 MOONS ART PROCUREMENT" -ForegroundColor Cyan
Write-Host " Fortnite Quality on Budget" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Asset definitions
$AssetStorePacks = @{
    "Indie" = @(
        @{Name="Fantasy Adventure Environment"; Price=2.25; WasPrice=45.00; URL="https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-adventure-environment-70354"; Priority="FLASH DEAL"; Sale="95%"}
        @{Name="Polygon Fantasy Character Pack"; Price=50.00; WasPrice=100.00; URL="https://assetstore.unity.com/packages/3d/characters/humanoids/polygon-fantasy-character-pack-XXXXX"; Priority="High"; Sale="50%"}
        @{Name="RPG Monster BUNDLE Polyart"; Price=30.00; WasPrice=60.00; URL="https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-bundle-polyart-261480"; Priority="High"; Sale="50%"}
        @{Name="Modular Fantasy Kingdom"; Price=150.00; WasPrice=300.00; URL="https://assetstore.unity.com/packages/3d/environments/fantasy/modular-fantasy-kingdom-XXXXX"; Priority="Critical"; Sale="50%"}
        @{Name="Feel"; Price=25.00; WasPrice=50.00; URL="https://assetstore.unity.com/packages/tools/particles-effects/feel-183370"; Priority="High"; Sale="50%"}
        @{Name="Realistic Water VFX - URP"; Price=22.49; WasPrice=44.99; URL="https://assetstore.unity.com/packages/vfx/particles/realistic-water-vfx-urp-278035"; Priority="Medium"; Sale="50%"}
    )
    "AA" = @(
        @{Name="Victorian Train Station Pack"; Price=100.00; WasPrice=200.00; URL="https://assetstore.unity.com/packages/3d/environments/urban/victorian-train-station-XXXXX"; Priority="Moon3"; Sale="Est 50%"}
        @{Name="Gothic Cathedral Mega Pack"; Price=180.00; WasPrice=360.00; URL="https://assetstore.unity.com/packages/3d/environments/historic/gothic-cathedral-XXXXX"; Priority="Moon1,6,12"; Sale="Est 50%"}
        @{Name="Airship Fleet Assets"; Price=150.00; WasPrice=300.00; URL="https://assetstore.unity.com/packages/3d/vehicles/air/airship-fleet-XXXXX"; Priority="Moon8"; Sale="Est 50%"}
        @{Name="Epic Toon FX"; Price=75.00; WasPrice=150.00; URL="https://assetstore.unity.com/packages/vfx/particles/spells/epic-toon-fx-XXXXX"; Priority="AllMoons"; Sale="Est 50%"}
    )
    "AAA" = @(
        @{Name="ArtStation Commission: 7 Characters"; Price=2500.00; WasPrice=2500.00; URL="https://www.artstation.com/jobs"; Priority="Custom"; Sale="N/A"}
        @{Name="ArtStation Commission: 5 Hero Buildings"; Price=3000.00; WasPrice=3000.00; URL="https://www.artstation.com/jobs"; Priority="Custom"; Sale="N/A"}
        @{Name="ArtStation Commission: VFX Suite"; Price=1500.00; WasPrice=1500.00; URL="https://www.artstation.com/jobs"; Priority="Custom"; Sale="N/A"}
    )
}

$FreeAssets = @{
    "Mixamo" = @(
        @{Name="Adventurer (Player)"; URL="https://www.mixamo.com/#/?page=1&query=adventurer"; Type="Character"; Priority="P0"}
        @{Name="Queen (Anastasia)"; URL="https://www.mixamo.com/#/?page=1&query=queen"; Type="Character"; Priority="P0"}
        @{Name="Worker (Milo)"; URL="https://www.mixamo.com/#/?page=1&query=worker"; Type="Character"; Priority="P0"}
        @{Name="Mutant (MudGolem)"; URL="https://www.mixamo.com/#/?page=1&query=mutant"; Type="Character"; Priority="P0"}
        @{Name="Knight (Cassian)"; URL="https://www.mixamo.com/#/?page=1&query=knight"; Type="Character"; Priority="P1"}
        @{Name="Girl (Lirael)"; URL="https://www.mixamo.com/#/?page=1&query=girl"; Type="Character"; Priority="P1"}
    )
    "Polyhaven" = @(
        @{Name="Mud PBR Material"; URL="https://polyhaven.com/a/mud_cracked_dry"; Type="Texture"; Priority="Moon1"}
        @{Name="Stone Cathedral PBR"; URL="https://polyhaven.com/a/limestone_rough"; Type="Texture"; Priority="Moon1"}
        @{Name="Metal Ornament PBR"; URL="https://polyhaven.com/a/metal_rusty"; Type="Texture"; Priority="AllMoons"}
        @{Name="Water Clean PBR"; URL="https://polyhaven.com/a/water_well"; Type="Texture"; Priority="Moon2,4"}
        @{Name="Dawn HDRI"; URL="https://polyhaven.com/a/kloofendal_48d_partly_cloudy"; Type="HDRI"; Priority="Moon1,5,9"}
        @{Name="Dusk HDRI"; URL="https://polyhaven.com/a/evening_road_01"; Type="HDRI"; Priority="Moon3,7,11"}
        @{Name="Night Aurora HDRI"; URL="https://polyhaven.com/a/snowy_forest"; Type="HDRI"; Priority="Moon2,6,10"}
    )
    "UnityAssetStore" = @(
        @{Name="Human Basic Motions FREE"; URL="https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271"; Type="Animation"; Priority="P0"}
        @{Name="PrimeTween FREE"; URL="https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960"; Type="Tool"; Priority="P1"}
        @{Name="In-game Debug Console"; URL="https://assetstore.unity.com/packages/tools/gui/in-game-debug-console-68068"; Type="Tool"; Priority="P1"}
    )
}

function Show-TierSummary {
    param([string]$SelectedTier)
    
    Write-Host "`n=== TIER: $SelectedTier ===" -ForegroundColor Yellow
    
    $cost = switch ($SelectedTier) {
        "Free" { 0 }
        "Indie" { 279.74 }
        "AA" { 2429.24 }
        "AAA" { 7000.00 }
    }
    
    $quality = switch ($SelectedTier) {
        "Free" { "50-60/100 (Mobile game)" }
        "Indie" { "70-80/100 (Indie PC game)" }
        "AA" { "85/100 (Fortnite Chapter 1)" }
        "AAA" { "95/100 (Fortnite Chapter 3)" }
    }
    
    $time = switch ($SelectedTier) {
        "Free" { "120 hours labor" }
        "Indie" { "40 hours + 2 weeks asset integration" }
        "AA" { "12-16 weeks production" }
        "AAA" { "20-24 weeks production" }
    }
    
    Write-Host "  Cost: `$$cost" -ForegroundColor Cyan
    Write-Host "  Quality: $quality" -ForegroundColor Cyan
    Write-Host "  Timeline: $time" -ForegroundColor Cyan
    Write-Host ""
}

function Generate-ShoppingList {
    param([string]$SelectedTier)
    
    Write-Host "`n=== SHOPPING LIST ($SelectedTier TIER) ===" -ForegroundColor Green
    
    $total = 0
    $savings = 0
    
    # Always include Indie tier packs
    $packs = $AssetStorePacks["Indie"]
    if ($SelectedTier -eq "AA" -or $SelectedTier -eq "AAA") {
        $packs += $AssetStorePacks["AA"]
    }
    if ($SelectedTier -eq "AAA") {
        $packs += $AssetStorePacks["AAA"]
    }
    
    Write-Host "`nPAID ASSETS:" -ForegroundColor Yellow
    $packs | ForEach-Object {
        $saving = $_.WasPrice - $_.Price
        $total += $_.Price
        $savings += $saving
        
        $priorityColor = if ($_.Priority -eq "FLASH DEAL") { "Red" } 
                        elseif ($_.Priority -eq "Critical") { "Magenta" }
                        elseif ($_.Priority -eq "High") { "Yellow" }
                        else { "White" }
        
        Write-Host "  [$($_.Priority)]" -NoNewline -ForegroundColor $priorityColor
        Write-Host " $($_.Name)" -ForegroundColor White
        Write-Host "    Price: `$$($_.Price) (was `$$($_.WasPrice), save `$$saving - $($_.Sale) off)" -ForegroundColor Cyan
        Write-Host "    URL: $($_.URL)" -ForegroundColor Gray
        Write-Host ""
    }
    
    Write-Host "`nFREE ASSETS:" -ForegroundColor Yellow
    
    Write-Host "`n  MIXAMO CHARACTERS (requires Adobe account):" -ForegroundColor Cyan
    $FreeAssets["Mixamo"] | ForEach-Object {
        Write-Host "    [$($_.Priority)] $($_.Name)" -ForegroundColor White
        Write-Host "      URL: $($_.URL)" -ForegroundColor Gray
    }
    
    Write-Host "`n  POLYHAVEN TEXTURES/HDRIs (CC0 license):" -ForegroundColor Cyan
    $FreeAssets["Polyhaven"] | ForEach-Object {
        Write-Host "    [$($_.Priority)] $($_.Name)" -ForegroundColor White
        Write-Host "      URL: $($_.URL)" -ForegroundColor Gray
    }
    
    Write-Host "`n  UNITY ASSET STORE (FREE):" -ForegroundColor Cyan
    $FreeAssets["UnityAssetStore"] | ForEach-Object {
        Write-Host "    [$($_.Priority)] $($_.Name)" -ForegroundColor White
        Write-Host "      URL: $($_.URL)" -ForegroundColor Gray
    }
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "TOTAL PAID COST: `$$total" -ForegroundColor Cyan
    Write-Host "TOTAL SAVINGS: `$$savings (vs regular price)" -ForegroundColor Green
    Write-Host "FREE ASSETS: $($FreeAssets["Mixamo"].Count + $FreeAssets["Polyhaven"].Count + $FreeAssets["UnityAssetStore"].Count) items" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    # Export to file
    $listPath = "docs\SHOPPING_LIST_$SelectedTier.md"
    $content = @"
# TARTARIA — Shopping List ($SelectedTier Tier)
**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm")  
**Total Cost:** `$$total  
**Total Savings:** `$$savings  

---

## PAID ASSETS

"@
    
    $packs | ForEach-Object {
        $content += @"

### $($_.Name)
- **Price:** `$$($_.Price) (was `$$($_.WasPrice))
- **Sale:** $($_.Sale) off
- **Priority:** $($_.Priority)
- **URL:** [$($_.URL)]($($_.URL))

"@
    }
    
    $content += @"

---

## FREE ASSETS

### Mixamo Characters
"@
    
    $FreeAssets["Mixamo"] | ForEach-Object {
        $content += "- [$($_.Priority)] **$($_.Name)** — [$($_.URL)]($($_.URL))`n"
    }
    
    $content += @"

### Polyhaven Textures/HDRIs
"@
    
    $FreeAssets["Polyhaven"] | ForEach-Object {
        $content += "- [$($_.Priority)] **$($_.Name)** — [$($_.URL)]($($_.URL))`n"
    }
    
    $content += @"

### Unity Asset Store (FREE)
"@
    
    $FreeAssets["UnityAssetStore"] | ForEach-Object {
        $content += "- [$($_.Priority)] **$($_.Name)** — [$($_.URL)]($($_.URL))`n"
    }
    
    [System.IO.File]::WriteAllText($listPath, $content, [System.Text.Encoding]::UTF8)
    Write-Host "Shopping list exported to: $listPath`n" -ForegroundColor Green
}

function Open-AssetStoreLinks {
    Write-Host "`n=== Opening Asset Store in browser... ===" -ForegroundColor Yellow
    
    # Open only FLASH DEAL first (highest priority)
    $flashDeal = $AssetStorePacks["Indie"] | Where-Object { $_.Priority -eq "FLASH DEAL" }
    if ($flashDeal) {
        Write-Host "Opening FLASH DEAL (ends in 21 hours!): $($flashDeal.Name)" -ForegroundColor Red
        Start-Process $flashDeal.URL
        Start-Sleep 2
    }
    
    Write-Host "Opening Critical priority packs..." -ForegroundColor Magenta
    $AssetStorePacks["Indie"] | Where-Object { $_.Priority -eq "Critical" } | ForEach-Object {
        Write-Host "  $($_.Name)" -ForegroundColor White
        Start-Process $_.URL
        Start-Sleep 1
    }
    
    Write-Host "`nOther links exported to shopping list — open manually" -ForegroundColor Cyan
}

function Download-FreeAssets {
    Write-Host "`n=== AUTOMATED FREE ASSET DOWNLOAD ===" -ForegroundColor Yellow
    Write-Host "Note: This script CANNOT auto-download Mixamo (requires browser login)" -ForegroundColor Red
    Write-Host "      Opening Mixamo in browser for manual download...`n" -ForegroundColor Red
    
    # Create directories
    $dirs = @(
        "Assets\_Project\Models\Characters",
        "Assets\_Project\Textures\Polyhaven",
        "Assets\_Project\HDRIs"
    )
    
    $dirs | ForEach-Object {
        if (-not (Test-Path $_)) {
            New-Item -ItemType Directory -Force -Path $_ | Out-Null
            Write-Host "Created: $_" -ForegroundColor Green
        }
    }
    
    # Open Mixamo in browser with instructions
    Write-Host "`n--- MIXAMO CHARACTER DOWNLOAD INSTRUCTIONS ---" -ForegroundColor Cyan
    Write-Host "1. Log in to Mixamo.com (Adobe account required)" -ForegroundColor White
    Write-Host "2. For EACH character below:" -ForegroundColor White
    Write-Host "   a. Search character name" -ForegroundColor White
    Write-Host "   b. Select T-POSE (not animation)" -ForegroundColor White
    Write-Host "   c. Download as FBX" -ForegroundColor White
    Write-Host "   d. Save to: Assets\_Project\Models\Characters\" -ForegroundColor White
    Write-Host "`nCharacters to download:" -ForegroundColor Yellow
    
    $FreeAssets["Mixamo"] | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor White
    }
    
    Write-Host "`nOpening Mixamo in 5 seconds..." -ForegroundColor Cyan
    Start-Sleep 2
    Start-Process "https://www.mixamo.com"
    
    # Polyhaven textures (can be downloaded via curl, but manual is easier)
    Write-Host "`n--- POLYHAVEN TEXTURE DOWNLOAD ---" -ForegroundColor Cyan
    Write-Host "Opening Polyhaven.com — download these textures:" -ForegroundColor White
    
    $FreeAssets["Polyhaven"] | ForEach-Object {
        Write-Host "  - $($_.Name) (2K PNG)" -ForegroundColor White
    }
    
    Write-Host "`nSave to: Assets\_Project\Textures\Polyhaven\`n" -ForegroundColor Yellow
    Start-Sleep 1
    Start-Process "https://polyhaven.com/textures"
    
    # Unity Asset Store free packs
    Write-Host "`n--- UNITY ASSET STORE FREE PACKS ---" -ForegroundColor Cyan
    Write-Host "Opening in Unity Hub — add to My Assets, then import to project:" -ForegroundColor White
    
    $FreeAssets["UnityAssetStore"] | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor White
    }
    
    Write-Host ""
    Start-Sleep 1
    $FreeAssets["UnityAssetStore"] | ForEach-Object {
        Start-Process $_.URL
        Start-Sleep 1
    }
    
    Write-Host "`n=== FREE ASSET DOWNLOAD COMPLETE ===" -ForegroundColor Green
    Write-Host "Total time estimate: 4-8 hours (manual download)" -ForegroundColor Cyan
}

# Main execution
Show-TierSummary -SelectedTier $Tier

if ($GenerateShoppingList -or -not $ExecuteFreeDownloads -and -not $OpenAssetStore) {
    Generate-ShoppingList -SelectedTier $Tier
}

if ($ExecuteFreeDownloads) {
    Download-FreeAssets
}

if ($OpenAssetStore) {
    Open-AssetStoreLinks
}

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Yellow
Write-Host "1. Review shopping list: docs\SHOPPING_LIST_$Tier.md" -ForegroundColor White
Write-Host "2. Buy Asset Store packs (Spring Sale ends May 6, 2026)" -ForegroundColor White
Write-Host "3. Download Mixamo characters (4-8 hours)" -ForegroundColor White
Write-Host "4. Download Polyhaven textures (2-4 hours)" -ForegroundColor White
Write-Host "5. Import all assets to Unity" -ForegroundColor White
Write-Host "6. Run: .\apply-visual-upgrades.ps1 -P0Only (character import)" -ForegroundColor White
Write-Host "7. Run: .\tartaria-play.ps1 (test with new assets)`n" -ForegroundColor White

Write-Host "========================================`n" -ForegroundColor Cyan
