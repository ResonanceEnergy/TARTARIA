<#
.SYNOPSIS
    TARTARIA Polyhaven Texture Batch Import
    Extracts textures from .blend folders and imports to Unity Resources

.DESCRIPTION
    Dr. Vex Aurelian — Year 2100 Automated Texture Importer
    Processes NEW ASSETS MAY 2626 Polyhaven .blend folders, extracts PBR maps,
    copies to Unity Resources/Textures/Polyhaven/ with standardized naming

.PARAMETER SkipExisting
    Skip files that already exist in Unity project (default: true)

.PARAMETER LogPath
    Output log file path (default: Logs\texture-import-report.txt)

.EXAMPLE
    .\tartaria-import-textures.ps1
    .\tartaria-import-textures.ps1 -SkipExisting:$false
#>

param(
    [switch]$SkipExisting = $true,
    [string]$LogPath = "Logs\texture-import-report.txt"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# -- Banner
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  TARTARIA TEXTURE IMPORT AUTOMATION" -ForegroundColor Cyan
Write-Host "  Dr. Vex Aurelian | Year 2100 Protocol" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# -- Paths
$projectRoot = "C:\dev\TARTARIA_new"
$sourceRoot = Join-Path $projectRoot "NEW ASSETS MAY 2626"
$unityTexturesRoot = Join-Path $projectRoot "Assets\_Project\Resources\Textures\Polyhaven"
$logDir = Join-Path $projectRoot "Logs"

# -- Validate
if (-not (Test-Path $sourceRoot)) {
    Write-Host "[ERROR] Source folder not found: $sourceRoot" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

if (-not (Test-Path $unityTexturesRoot)) {
    Write-Host "[INIT] Creating Unity textures folder: $unityTexturesRoot" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $unityTexturesRoot -Force | Out-Null
}

# -- Stats
$imported = 0
$skipped = 0
$errors = 0
$textureFiles = @()

# -- Start Log
$logContent = @"
TARTARIA TEXTURE IMPORT REPORT
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Source: $sourceRoot
Destination: $unityTexturesRoot
Skip Existing: $SkipExisting

========================================

"@

Write-Host "[SCAN] Searching for Polyhaven .blend folders..." -ForegroundColor Cyan

# -- Find all .blend folders with textures subdirectory
$blendFolders = Get-ChildItem -Path $sourceRoot -Directory |
    Where-Object { $_.Name -like "*_4k.blend" -and (Test-Path (Join-Path $_.FullName "textures")) }

Write-Host "[FOUND] $($blendFolders.Count) Polyhaven texture sets`n" -ForegroundColor Green

foreach ($blendFolder in $blendFolders) {
    $texturesPath = Join-Path $blendFolder.FullName "textures"
    $textureName = $blendFolder.Name -replace "\.blend$", ""

    Write-Host "[PROCESS] $textureName" -ForegroundColor Yellow
    $logContent += "`n## $textureName`n"

    # Get all texture files (.jpg, .png, .exr)
    $files = Get-ChildItem -Path $texturesPath -File |
        Where-Object { $_.Extension -in @(".jpg", ".png", ".exr") }

    if ($files.Count -eq 0) {
        Write-Host "  [WARN] No texture files found in $texturesPath" -ForegroundColor Red
        $logContent += "  WARNING: No texture files found`n"
        continue
    }

    Write-Host "  [MAPS] Found $($files.Count) texture maps:" -ForegroundColor Gray

    foreach ($file in $files) {
        $destPath = Join-Path $unityTexturesRoot $file.Name

        # Check if file exists
        if ($SkipExisting -and (Test-Path $destPath)) {
            Write-Host "    -- $($file.Name) (exists, skipped)" -ForegroundColor DarkGray
            $logContent += "    SKIP: $($file.Name) (already exists)`n"
            $skipped++
            continue
        }

        # Copy file
        try {
            Copy-Item -Path $file.FullName -Destination $destPath -Force

            # Detect map type
            $mapType = switch -Regex ($file.Name) {
                "_diff_"  { "Diffuse/Albedo" }
                "_nor_"   { "Normal Map" }
                "_rough_" { "Roughness" }
                "_disp_"  { "Displacement/Height" }
                "_ao_"    { "Ambient Occlusion" }
                default   { "Unknown" }
            }

            $sizeMB = [math]::Round($file.Length / 1MB, 2)
            Write-Host "    ✓ $($file.Name) ($mapType, $sizeMB MB)" -ForegroundColor Green
            $logContent += "    IMPORT: $($file.Name) | $mapType | $sizeMB MB`n"
            $imported++

            $textureFiles += [PSCustomObject]@{
                Name = $file.Name
                Type = $mapType
                Size = $sizeMB
                Source = $textureName
            }
        }
        catch {
            Write-Host "    [ERROR] Failed to copy $($file.Name): $_" -ForegroundColor Red
            $logContent += "    ERROR: $($file.Name) -- $_`n"
            $errors++
        }
    }
}

# -- Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  IMPORT COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Imported: $imported files" -ForegroundColor Green
Write-Host "Skipped:  $skipped files" -ForegroundColor Yellow
Write-Host "Errors:   $errors files" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })

$totalSizeMB = [math]::Round(($textureFiles | Measure-Object -Property Size -Sum).Sum, 2)
Write-Host "Total Size: $totalSizeMB MB`n" -ForegroundColor Cyan

# -- Texture Breakdown by Type
$byType = $textureFiles | Group-Object -Property Type |
    Select-Object Name, Count |
    Sort-Object Count -Descending

Write-Host "[BREAKDOWN] Texture Maps by Type:" -ForegroundColor Cyan
foreach ($type in $byType) {
    Write-Host "  $($type.Name): $($type.Count) files" -ForegroundColor Gray
}

# -- Write Log
$logContent += "`n========================================`n"
$logContent += "SUMMARY`n"
$logContent += "========================================`n"
$logContent += "Imported: $imported files`n"
$logContent += "Skipped:  $skipped files`n"
$logContent += "Errors:   $errors files`n"
$logContent += "Total Size: $totalSizeMB MB`n`n"

$logContent += "TEXTURE BREAKDOWN BY TYPE:`n"
foreach ($type in $byType) {
    $logContent += "  $($type.Name): $($type.Count) files`n"
}

# Save log with UTF-8 BOM
[System.IO.File]::WriteAllText($LogPath, $logContent, (New-Object System.Text.UTF8Encoding($true)))

Write-Host "`n[LOG] Report saved: $LogPath" -ForegroundColor Green

# -- Next Steps
Write-Host "`n[NEXT STEPS]" -ForegroundColor Yellow
Write-Host "1. Open Unity Editor (will auto-detect new textures)" -ForegroundColor Gray
Write-Host "2. Wait for import (2-3 minutes for $imported files)" -ForegroundColor Gray
Write-Host "3. Check Console for import warnings" -ForegroundColor Gray
Write-Host "4. Create Materials: Menu --> Tartaria --> Create Materials from Textures`n" -ForegroundColor Gray

exit 0
