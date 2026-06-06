# HDRI Skybox Downloader for Moon 1
# Downloads golden hour HDRI from Polyhaven (CC0 license)

param(
    [string]$Resolution = "2k",  # Options: 1k, 2k, 4k, 8k
    [switch]$ForceDownload
)

$targetDir = "Assets\_Project\Textures\Skyboxes"
$outputFile = "$targetDir\Moon1_GoldenHour_HDRI.exr"

if (-not (Test-Path $targetDir)) {
    Write-Host "⚠️  Skybox directory missing: $targetDir" -ForegroundColor Red
    exit 1
}

Write-Host "`n🌅 MOON 1 GOLDEN HOUR HDRI DOWNLOADER`n" -ForegroundColor Cyan

# Check if already downloaded
if ((Test-Path $outputFile) -and -not $ForceDownload) {
    $size = (Get-Item $outputFile).Length / 1MB
    Write-Host "✅ HDRI already exists: Moon1_GoldenHour_HDRI.exr ($([math]::Round($size, 1)) MB)" -ForegroundColor Green
    Write-Host "   Use -ForceDownload to re-download`n" -ForegroundColor Gray
    exit 0
}

# Polyhaven API - Golden hour HDRIs
$hdriOptions = @{
    "kloppenheim_02" = @{
        name = "Kloppenheim Sunset"
        desc = "Golden hour sunset with warm amber tones"
        url_2k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/2k/kloppenheim_02_2k.exr"
        url_4k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/4k/kloppenheim_02_4k.exr"
    }
    "venice_sunset" = @{
        name = "Venice Sunset"
        desc = "Warm architectural sunset lighting"
        url_2k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/2k/venice_sunset_2k.exr"
        url_4k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/4k/venice_sunset_4k.exr"
    }
    "sunflowers" = @{
        name = "Sunflowers"
        desc = "Late afternoon golden light"
        url_2k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/2k/sunflowers_2k.exr"
        url_4k = "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/4k/sunflowers_4k.exr"
    }
}

Write-Host "Available Golden Hour HDRIs:" -ForegroundColor Yellow
$hdriOptions.Keys | ForEach-Object {
    $hdri = $hdriOptions[$_]
    Write-Host "  • $($hdri.name) - $($hdri.desc)" -ForegroundColor White
}

# Default to first option
$selectedKey = "kloppenheim_02"
$selected = $hdriOptions[$selectedKey]

Write-Host "`nDownloading: $($selected.name) [$Resolution resolution]" -ForegroundColor Cyan
$url = if ($Resolution -eq "4k") { $selected.url_4k } else { $selected.url_2k }

Write-Host "Source: $url" -ForegroundColor Gray

try {
    $ProgressPreference = 'SilentlyContinue'  # Faster download
    Invoke-WebRequest -Uri $url -OutFile $outputFile -UseBasicParsing
    $size = (Get-Item $outputFile).Length / 1MB
    Write-Host "`n✅ Downloaded: $([math]::Round($size, 1)) MB" -ForegroundColor Green
    Write-Host "   Saved to: $outputFile`n" -ForegroundColor Gray
    
    Write-Host "🎬 IN UNITY EDITOR:" -ForegroundColor Magenta
    Write-Host "  1. Assets → Import → Moon1_GoldenHour_HDRI.exr" -ForegroundColor White
    Write-Host "  2. Set Texture Shape: Cube" -ForegroundColor White
    Write-Host "  3. Create → Material → Skybox/Cubemap" -ForegroundColor White
    Write-Host "  4. Assign HDRI to Cubemap slot" -ForegroundColor White
    Write-Host "  5. Window → Rendering → Lighting" -ForegroundColor White
    Write-Host "  6. Environment → Skybox Material → Assign new material`n" -ForegroundColor White
    
} catch {
    Write-Host "`n❌ Download failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Check internet connection or try different resolution`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "License: CC0 (Public Domain) - Polyhaven.com`n" -ForegroundColor Gray