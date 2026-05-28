# Golden Ratio Calculator for Tartarian Cathedral
# φ (phi) = 1.618033988749895

param(
    [Parameter(Mandatory=$false)]
    [double]$BaseSize = 4.0
)

$phi = 1.618033988749895

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🏛️  TARTARIAN CATHEDRAL — GOLDEN RATIO CALCULATOR" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📐 Base Unit: $BaseSize meters`n" -ForegroundColor Yellow

Write-Host "📏 FOUNDATION & WALLS" -ForegroundColor Green
Write-Host "   Wall Width:              $BaseSize m" -ForegroundColor White
Write-Host "   Wall Height:             $("{0:N3}" -f ($BaseSize * $phi)) m  (Base × φ)" -ForegroundColor White
Write-Host "   Wall_Corner:             $BaseSize m × $("{0:N3}" -f ($BaseSize * $phi)) m" -ForegroundColor White
Write-Host "   Foundation_Block:        $BaseSize m × $BaseSize m × $($BaseSize / 2) m`n" -ForegroundColor White

Write-Host "🔵 DOME SYSTEM (Octagonal)" -ForegroundColor Green
$domeRadius = $BaseSize * $phi
$domeHeight = $domeRadius / $phi
Write-Host "   Dome Diameter:           $("{0:N3}" -f ($domeRadius * 2)) m  (Base × φ × 2)" -ForegroundColor White
Write-Host "   Dome Height:             $("{0:N3}" -f $domeHeight) m  (Radius ÷ φ)" -ForegroundColor White
Write-Host "   Segment Arc:             45° (8 segments)" -ForegroundColor White
Write-Host "   Mercury Ball:            $("{0:N3}" -f ($BaseSize / 2)) m diameter`n" -ForegroundColor White

Write-Host "🗼 SPIRE (Mercury Ball Tower)" -ForegroundColor Green
$spireBase = $BaseSize
$spireMid = $BaseSize * $phi
$spireTop = $BaseSize * ($phi * $phi)
$spireTotal = $BaseSize * ($phi * $phi * $phi)
Write-Host "   Spire_Base Height:       $("{0:N3}" -f $spireBase) m  (Base)" -ForegroundColor White
Write-Host "   Spire_Mid Height:        $("{0:N3}" -f $spireMid) m  (Base × φ)" -ForegroundColor White
Write-Host "   Spire_Top Height:        $("{0:N3}" -f $spireTop) m  (Base × φ²)" -ForegroundColor White
Write-Host "   TOTAL Spire Height:      $("{0:N3}" -f $spireTotal) m  (Base × φ³)" -ForegroundColor Cyan
Write-Host "   Mercury Ball Diameter:   $("{0:N3}" -f ($BaseSize / 2)) m`n" -ForegroundColor White

Write-Host "🎨 DETAILS" -ForegroundColor Green
Write-Host "   Column Height:           $("{0:N3}" -f ($BaseSize * $phi)) m  (Base × φ)" -ForegroundColor White
Write-Host "   Column Diameter:         $("{0:N3}" -f ($BaseSize / 4)) m" -ForegroundColor White
Write-Host "   Rose Window Diameter:    $("{0:N3}" -f ($BaseSize * $phi)) m  (Base × φ)" -ForegroundColor White
Write-Host "   Door Width:              $BaseSize m" -ForegroundColor White
Write-Host "   Door Height:             $("{0:N3}" -f ($BaseSize * $phi)) m  (Base × φ)`n" -ForegroundColor White

Write-Host "🏗️  CATHEDRAL FOOTPRINT" -ForegroundColor Green
$cathedrałWidth = $BaseSize * 4  # 4 walls
$cathedralLength = $BaseSize * 4
Write-Host "   Base Footprint:          $cathedrałWidth m × $cathedralLength m" -ForegroundColor White
Write-Host "   Total Height (w/spire):  $("{0:N3}" -f ($spireTotal + $domeHeight)) m" -ForegroundColor Cyan
Write-Host "   Buried Depth:            $("{0:N3}" -f (($spireTotal + $domeHeight) * 0.6)) m  (60% underground)" -ForegroundColor White
Write-Host "   Visible Height:          $("{0:N3}" -f (($spireTotal + $domeHeight) * 0.4)) m  (40% above ground)`n" -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "φ = $phi" -ForegroundColor Gray
Write-Host "All measurements maintain sacred geometry proportions ✨" -ForegroundColor Gray
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Export to CSV for reference
$measurements = @(
    [PSCustomObject]@{Component="Wall_4x4m"; Width=$BaseSize; Height=($BaseSize * $phi); Depth=$BaseSize}
    [PSCustomObject]@{Component="Wall_Corner_90deg"; Width=$BaseSize; Height=($BaseSize * $phi); Depth=$BaseSize}
    [PSCustomObject]@{Component="Archway_Gothic"; Width=$BaseSize; Height=($BaseSize * $phi * 1.236); Depth=$BaseSize}
    [PSCustomObject]@{Component="Dome_Segment (×8)"; Width=($domeRadius * 2 / 8); Height=$domeHeight; Depth=($domeRadius * 2 / 8)}
    [PSCustomObject]@{Component="Spire_Base"; Width=$BaseSize; Height=$spireBase; Depth=$BaseSize}
    [PSCustomObject]@{Component="Spire_Mid"; Width=($BaseSize * 0.618); Height=$spireMid; Depth=($BaseSize * 0.618)}
    [PSCustomObject]@{Component="Spire_Top_MercuryBall"; Width=($BaseSize / 2); Height=$spireTop; Depth=($BaseSize / 2)}
    [PSCustomObject]@{Component="Column_Fluted"; Width=($BaseSize / 4); Height=($BaseSize * $phi); Depth=($BaseSize / 4)}
    [PSCustomObject]@{Component="RoseWindow_Circular"; Width=($BaseSize * $phi); Height=($BaseSize * $phi); Depth=0.1}
    [PSCustomObject]@{Component="Door_Main_Ornate"; Width=$BaseSize; Height=($BaseSize * $phi); Depth=0.3}
    [PSCustomObject]@{Component="Foundation_Block"; Width=$BaseSize; Height=($BaseSize / 2); Depth=$BaseSize}
)

$measurements | Export-Csv -Path "Tools\Phase1\Cathedral_Measurements.csv" -NoTypeInformation
Write-Host "📊 Exported measurements to: Tools\Phase1\Cathedral_Measurements.csv`n" -ForegroundColor Green