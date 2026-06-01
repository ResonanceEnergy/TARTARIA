# download-polyhaven-textures.ps1 — Auto-download 7 critical Polyhaven textures for TARTARIA buildings
# Dr. Vex Aurelian (Unity 2100 → 2026 TARTARIA)

Write-Host "`n=== POLYHAVEN ARCHITECTURAL TEXTURES DOWNLOADER ===" -ForegroundColor Cyan
Write-Host "Downloading 7 texture sets (2K-PNG, ~280 MB total)`n" -ForegroundColor White

$outputDir = "C:\Users\gripa\Downloads\Polyhaven_Architectural"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    Write-Host "✓ Created: $outputDir`n" -ForegroundColor Green
}

# Polyhaven texture definitions (texture_name → use case)
$textures = @{
    "medieval_brick_wall" = "Star Dome exterior brick walls"
    "copper_patina" = "Harmonic Fountain copper pipes + organ"
    "quartz_crystal" = "Crystal Spire crystalline structure"
    "carved_stone_gothic" = "Cathedral carved stone pillars"
    "slate_roof" = "Building roof tiles"
    "wood_planks_dark" = "Cathedral interior wood beams"
    "stained_glass" = "Cathedral stained glass windows"
}

# Map types per texture (Polyhaven standard)
$mapTypes = @("diff", "nor", "rough", "disp", "ao")  # diffuse, normal, roughness, displacement, ambient occlusion
$mapNames = @("Diffuse", "Normal", "Roughness", "Displacement", "AO")

$totalFiles = $textures.Count * $mapTypes.Count  # 7 textures × 5 maps = 35 files
$downloadedCount = 0
$failedDownloads = @()

Write-Host "Texture Sets to Download:" -ForegroundColor Yellow
$textures.GetEnumerator() | Sort-Object Key | ForEach-Object {
    Write-Host "  • $($_.Key) → $($_.Value)" -ForegroundColor Gray
}
Write-Host ""

foreach ($texturePair in $textures.GetEnumerator()) {
    $textureName = $texturePair.Key
    $useCase = $texturePair.Value

    Write-Host "[Downloading] $textureName" -ForegroundColor Cyan
    Write-Host "  Use: $useCase" -ForegroundColor Gray

    # Create subfolder for this texture set
    $textureDir = Join-Path $outputDir $textureName
    if (-not (Test-Path $textureDir)) {
        New-Item -ItemType Directory -Path $textureDir -Force | Out-Null
    }

    for ($i = 0; $i -lt $mapTypes.Count; $i++) {
        $mapType = $mapTypes[$i]
        $mapName = $mapNames[$i]

        # Polyhaven CDN URL format: https://dl.polyhaven.org/file/ph-assets/Textures/png/2k/[texture]/[texture]_[map].png
        $url = "https://dl.polyhaven.org/file/ph-assets/Textures/png/2k/$textureName/${textureName}_$mapType.png"
        $outputFile = Join-Path $textureDir "${textureName}_$mapType.png"

        # Skip if already downloaded
        if (Test-Path $outputFile) {
            Write-Host "  ✓ $mapName (cached)" -ForegroundColor Green
            $downloadedCount++
            continue
        }

        try {
            Write-Host "  ↓ $mapName..." -NoNewline -ForegroundColor Yellow

            # Download with progress suppression (faster)
            $ProgressPreference = 'SilentlyContinue'
            Invoke-WebRequest -Uri $url -OutFile $outputFile -UseBasicParsing -ErrorAction Stop
            $ProgressPreference = 'Continue'

            $fileSizeMB = [math]::Round((Get-Item $outputFile).Length / 1MB, 2)
            Write-Host " ✓ ($fileSizeMB MB)" -ForegroundColor Green
            $downloadedCount++
        }
        catch {
            Write-Host " ✗ FAILED" -ForegroundColor Red
            $failedDownloads += "$textureName - $mapName (URL: $url)"
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host ""
}

Write-Host "=== DOWNLOAD COMPLETE ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Downloaded: $downloadedCount / $totalFiles files" -ForegroundColor $(if ($downloadedCount -eq $totalFiles) { "Green" } else { "Yellow" })

if ($failedDownloads.Count -gt 0) {
    Write-Host "`nFailed Downloads ($($failedDownloads.Count)):" -ForegroundColor Red
    $failedDownloads | ForEach-Object { Write-Host "  • $_" -ForegroundColor Red }
    Write-Host "`nNote: Some Polyhaven textures may have different naming conventions." -ForegroundColor Yellow
    Write-Host "Visit https://polyhaven.com/textures to manually download missing files." -ForegroundColor Yellow
}

Write-Host "`nOutput Location: $outputDir" -ForegroundColor Cyan
Write-Host ""

# Show folder size
$totalSizeMB = [math]::Round((Get-ChildItem $outputDir -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB, 2)
Write-Host "Total Downloaded: $totalSizeMB MB" -ForegroundColor Green

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Wait for Unity to finish importing 3D models (~5 more minutes)" -ForegroundColor White
Write-Host "2. Run: .\tartaria-import-textures.ps1" -ForegroundColor White
Write-Host "   (Copies textures to Unity project Assets folder)" -ForegroundColor Gray
Write-Host "3. Unity will auto-import texture files (~2 minutes)" -ForegroundColor White
Write-Host "4. Apply textures to modular dungeon materials" -ForegroundColor White
Write-Host ""
Write-Host "✓ Texture download complete!`n" -ForegroundColor Green
