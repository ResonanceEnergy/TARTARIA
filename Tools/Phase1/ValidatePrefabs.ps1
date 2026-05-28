# Cathedral Prefab Progress Validator
# Scans Assets/_Project/Prefabs/Moon1/Cathedral/ and reports completion status

param(
    [switch]$ShowMissing,
    [switch]$Detailed
)

$cathedralPath = "Assets\_Project\Prefabs\Moon1\Cathedral"
if (-not (Test-Path $cathedralPath)) {
    Write-Host "⚠️  Cathedral folder not found: $cathedralPath" -ForegroundColor Red
    Write-Host "   Run Phase 1 directory setup first.`n" -ForegroundColor Yellow
    exit 1
}

$requiredPrefabs = @(
    "Foundation_16x16m.prefab",
    "Wall_4x4m_Stone.prefab",
    "Wall_Corner_4x4m.prefab",
    "Archway_4x7m.prefab",
    "Dome_Segment_N.prefab",
    "Dome_Segment_NE.prefab",
    "Dome_Segment_E.prefab",
    "Dome_Segment_SE.prefab",
    "Dome_Segment_S.prefab",
    "Dome_Segment_SW.prefab",
    "Dome_Segment_W.prefab",
    "Dome_Segment_NW.prefab",
    "Spire_Base_2x2m.prefab",
    "Spire_Mid_Taper.prefab",
    "Spire_Top_MercuryBall.prefab",
    "Column_Ornate_6.5m.prefab",
    "RoseWindow_4x4m.prefab",
    "Door_Grand_3x6m.prefab"
)

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  📦 CATHEDRAL PREFAB VALIDATION" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$foundPrefabs = Get-ChildItem -Path $cathedralPath -Filter "*.prefab" -Recurse
$foundCount = $foundPrefabs.Count
$totalRequired = $requiredPrefabs.Count

Write-Host "Prefabs Created: $foundCount / $totalRequired" -ForegroundColor $(if ($foundCount -eq $totalRequired) { "Green" } elseif ($foundCount -gt 0) { "Yellow" } else { "Red" })

$progressPercent = [math]::Round(($foundCount / $totalRequired) * 100, 1)
Write-Host "Progress: $progressPercent%`n" -ForegroundColor Cyan

# Check each required prefab
$missing = @()
foreach ($required in $requiredPrefabs) {
    $exists = $foundPrefabs | Where-Object { $_.Name -eq $required }
    if ($exists) {
        if ($Detailed) {
            $size = [math]::Round($exists.Length / 1KB, 1)
            Write-Host "  ✅ $required ($size KB)" -ForegroundColor Green
        }
    } else {
        $missing += $required
        if ($ShowMissing -or $Detailed) {
            Write-Host "  ❌ $required (NOT FOUND)" -ForegroundColor Red
        }
    }
}

if ($missing.Count -eq 0) {
    Write-Host "`n🎉 ALL 18 CATHEDRAL PREFABS COMPLETE! 🎉`n" -ForegroundColor Green
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Create 3 master Shader Graphs (Stone/Metal/Crystal)" -ForegroundColor White
    Write-Host "  2. Create 3 Moon1 material variants" -ForegroundColor White
    Write-Host "  3. Assign materials to prefabs" -ForegroundColor White
    Write-Host "  4. Assemble cathedral in Moon1_MagneticMoon.unity`n" -ForegroundColor White
} else {
    Write-Host "`n⚠️  Missing $($missing.Count) prefabs:`n" -ForegroundColor Yellow
    foreach ($m in $missing) {
        $category = switch -Wildcard ($m) {
            "Foundation*" { "Foundation" }
            "Wall*" { "Walls" }
            "Archway*" { "Walls" }
            "Dome*" { "Dome (8 segments)" }
            "Spire*" { "Spire (3 sections)" }
            "Column*" { "Details" }
            "RoseWindow*" { "Details" }
            "Door*" { "Details" }
        }
        Write-Host "  • $m" -ForegroundColor Gray -NoNewline
        Write-Host " [$category]" -ForegroundColor Cyan
    }
    Write-Host "`nReference: Tools\Phase1\PREFAB_CREATION_CHECKLIST.md`n" -ForegroundColor Yellow
}

Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan