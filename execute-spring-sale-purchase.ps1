# Unity Asset Store Spring Sale — Auto Purchase Script
# Opens all 6 asset pages in browser tabs for easy add-to-cart
# DEADLINE: May 6, 2026 (6 days remaining!)

param(
    [switch]$SkipOptional,  # Skip Motion Library ($10 item)
    [switch]$CartOnly       # Just open the cart page
)

cd C:\dev\TARTARIA_new

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TARTARIA — Unity Asset Store Spring Sale Automation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  CRITICAL: Sale ends May 6, 2026 (6 DAYS LEFT!)" -ForegroundColor Yellow
Write-Host "💰 Total Cost: ~`$280 (saves `$320 vs regular pricing)" -ForegroundColor Green
Write-Host ""

if ($CartOnly) {
    Write-Host "Opening Unity Asset Store cart..." -ForegroundColor Cyan
    Start-Process "https://assetstore.unity.com/cart"
    Write-Host "✓ Cart opened. Complete your purchase!" -ForegroundColor Green
    exit 0
}

# Define all Spring Sale assets with metadata
$assets = @(
    @{
        Name = "Fantasy Adventure Environment"
        Price = "`$2.25"
        Regular = "`$45.00"
        Savings = "`$42.75"
        Priority = "🔴 CRITICAL"
        URL = "https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-adventure-environment-70354"
        Note = "FLASH DEAL -- May end ANY TIME!"
    },
    @{
        Name = "Modular Fantasy Kingdom"
        Price = "`$150.00"
        Regular = "`$300.00"
        Savings = "`$150.00"
        Priority = "🔴 CRITICAL"
        URL = "https://assetstore.unity.com/packages/3d/environments/fantasy/modular-fantasy-kingdom-complete-pack-232986"
        Note = "500+ castle pieces, highest value item"
    },
    @{
        Name = "Polygon Character Pack"
        Price = "`$25.00"
        Regular = "`$50.00"
        Savings = "`$25.00"
        Priority = "🟡 HIGH"
        URL = "https://assetstore.unity.com/packages/3d/characters/humanoids/polygon-modular-fantasy-hero-characters-143468"
        Note = "12 rigged characters, covers all NPCs"
    },
    @{
        Name = "Realistic Water VFX"
        Price = "`$22.49"
        Regular = "`$44.99"
        Savings = "`$22.50"
        Priority = "🟡 HIGH"
        URL = "https://assetstore.unity.com/packages/vfx/particles/environment/realistic-water-vfx-pack-urp-218234"
        Note = "50+ water effects, URP 17 compatible"
    },
    @{
        Name = "Feel"
        Price = "`$25.00"
        Regular = "`$50.00"
        Savings = "`$25.00"
        Priority = "🟢 MEDIUM"
        URL = "https://assetstore.unity.com/packages/tools/particles-effects/feel-183370"
        Note = "Game juice plugin, industry standard"
    },
    @{
        Name = "Motion Library"
        Price = "`$10.00"
        Regular = "`$20.00"
        Savings = "`$10.00"
        Priority = "🔵 LOW"
        URL = "https://assetstore.unity.com/packages/3d/animations/huge-mocap-library-vol-1-17386"
        Note = "OPTIONAL -- Skip if budget tight (Mixamo covers animations)"
        Optional = $true
    }
)

Write-Host "📋 PURCHASE PLAN" -ForegroundColor Cyan
Write-Host "──────────────────────────────────────────────────────────" -ForegroundColor DarkGray
Write-Host ""

$totalPrice = 0
$totalSavings = 0
$itemCount = 0

foreach ($asset in $assets) {
    if ($asset.Optional -and $SkipOptional) {
        Write-Host "⏭️  SKIPPED: $($asset.Name)" -ForegroundColor DarkGray
        continue
    }

    $itemCount++
    Write-Host "$itemCount. $($asset.Name)" -ForegroundColor White
    Write-Host "   Price: $($asset.Price) (was $($asset.Regular)) -- $($asset.Priority)" -ForegroundColor $(
        if ($asset.Priority -match "CRITICAL") { "Red" }
        elseif ($asset.Priority -match "HIGH") { "Yellow" }
        elseif ($asset.Priority -match "MEDIUM") { "Green" }
        else { "Cyan" }
    )
    Write-Host "   Note: $($asset.Note)" -ForegroundColor DarkGray
    
    # Parse price for totals (remove $ and convert to decimal)
    $priceValue = [decimal]($asset.Price -replace '[\$,]', '')
    $savingsValue = [decimal]($asset.Savings -replace '[\$,]', '')
    $totalPrice += $priceValue
    $totalSavings += $savingsValue
    
    Write-Host ""
}

Write-Host "──────────────────────────────────────────────────────────" -ForegroundColor DarkGray
Write-Host "TOTAL: `$$totalPrice (saves `$$totalSavings)" -ForegroundColor Green
Write-Host ""

# Confirmation prompt
Write-Host "This script will open $itemCount browser tabs (one per asset)." -ForegroundColor Cyan
Write-Host ""
Write-Host "INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host "  1. Each tab opens the asset's Unity Asset Store page" -ForegroundColor White
Write-Host "  2. Click 'Add to Cart' on EACH tab" -ForegroundColor White
Write-Host "  3. After all items added, final tab opens your cart" -ForegroundColor White
Write-Host "  4. Review cart totals, then click 'Proceed to Checkout'" -ForegroundColor White
Write-Host "  5. Complete payment (card or PayPal)" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  DO NOT CLOSE TABS until all items are in cart!" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Ready to open tabs? (Y/N)"
if ($confirm -ne 'Y' -and $confirm -ne 'y') {
    Write-Host "❌ Purchase canceled. Run script again when ready." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Opening Unity Asset Store tabs..." -ForegroundColor Cyan
Write-Host ""

$tabNumber = 1
foreach ($asset in $assets) {
    if ($asset.Optional -and $SkipOptional) {
        continue
    }

    Write-Host "[$tabNumber/$itemCount] Opening: $($asset.Name)..." -ForegroundColor White
    Start-Process $asset.URL
    Start-Sleep -Milliseconds 800  # Delay between tabs to avoid browser overload
    $tabNumber++
}

Write-Host ""
Write-Host "Waiting 3 seconds before opening cart..." -ForegroundColor DarkGray
Start-Sleep -Seconds 3

Write-Host "Opening Unity Asset Store cart..." -ForegroundColor Cyan
Start-Process "https://assetstore.unity.com/cart"

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✓ All tabs opened!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "  1. Go through each tab and click 'Add to Cart'" -ForegroundColor White
Write-Host "  2. Verify price matches expected (see shopping list)" -ForegroundColor White
Write-Host "  3. On cart tab, review totals (should be ~`$280)" -ForegroundColor White
Write-Host "  4. Click 'Proceed to Checkout'" -ForegroundColor White
Write-Host "  5. Complete purchase before May 6!" -ForegroundColor White
Write-Host ""
Write-Host "📄 Full shopping list: docs\SPRING_SALE_SHOPPING_LIST.md" -ForegroundColor Cyan
Write-Host ""

# Create a purchase tracking file
$trackingFile = "docs\spring-sale-purchase-status.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

@"
TARTARIA Spring Sale Purchase Tracking
Generated: $timestamp
──────────────────────────────────────────────────────────

ITEMS TO PURCHASE ($itemCount total):

"@ | Out-File -FilePath $trackingFile -Encoding UTF8

foreach ($asset in $assets) {
    if ($asset.Optional -and $SkipOptional) {
        "[ SKIPPED ] $($asset.Name)" | Out-File -FilePath $trackingFile -Append -Encoding UTF8
        continue
    }
    "[ ] $($asset.Name) -- $($asset.Price)" | Out-File -FilePath $trackingFile -Append -Encoding UTF8
}

@"

Expected Total: `$$totalPrice

──────────────────────────────────────────────────────────
CHECKLIST:
[ ] All items added to cart
[ ] Cart total matches expected
[ ] Promo code applied (if available)
[ ] Payment completed
[ ] Receipt email saved
[ ] Assets appear in 'My Assets'
[ ] Imported into Unity project
[ ] No Console errors after import
[ ] Build pipeline GREEN after import

──────────────────────────────────────────────────────────
NOTES:

(Add any price discrepancies, issues, or observations here)

"@ | Out-File -FilePath $trackingFile -Append -Encoding UTF8

Write-Host "✓ Tracking file created: $trackingFile" -ForegroundColor Green
Write-Host "  (Check off items as you add them to cart)" -ForegroundColor DarkGray
Write-Host ""

Write-Host "Good luck! May the Spring Sale be with you! 🎮" -ForegroundColor Cyan
Write-Host ""
