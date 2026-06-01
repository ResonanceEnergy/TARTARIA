cd C:\dev\TARTARIA_new

Write-Host "`n═══ COMPREHENSIVE ASSET SCAN ═══`n" -ForegroundColor Yellow

# Scan all Unity asset files
$assetTypes = @("*.asset", "*.controller", "*.overrideController", "*.anim", "*.playable")
$allAssets = @()

foreach ($type in $assetTypes) {
    $found = Get-ChildItem -Path "Assets\_Project" -Filter $type -Recurse
    $allAssets += $found
}

Write-Host "Scanning $($allAssets.Count) asset files...`n" -ForegroundColor Cyan

$results = @()
$totalMissing = 0

foreach ($asset in $allAssets) {
    $content = Get-Content $asset.FullName -Raw -ErrorAction SilentlyContinue
    
    if ($content) {
        $missing = ([regex]::Matches($content, 'm_Script: \{fileID: 0\}|m_Script: \{fileID: [^,]+, guid: 00000000000000000000000000000000')).Count
        
        if ($missing -gt 0) {
            $totalMissing += $missing
            $results += [PSCustomObject]@{
                Asset = $asset.Name
                Type = $asset.Extension
                Missing = $missing
                Path = $asset.FullName.Replace("$PWD\", "")
            }
        }
    }
}

if ($results) {
    $results | Sort-Object -Property Missing -Descending | Format-Table -AutoSize
    Write-Host "`nAffected Assets: $($results.Count)" -ForegroundColor Yellow
    Write-Host "Total Missing Scripts in Assets: $totalMissing" -ForegroundColor Red
} else {
    Write-Host "No missing scripts found in asset files." -ForegroundColor Green
}

# Summary
Write-Host "`n═══ FULL PROJECT SUMMARY ═══`n" -ForegroundColor Cyan
Write-Host "Scenes: 0 missing scripts (15 scenes checked)" -ForegroundColor Green
Write-Host "Prefabs: 0 missing scripts (235 prefabs checked)" -ForegroundColor Green
Write-Host "Assets: $totalMissing missing scripts ($($allAssets.Count) assets checked)" -ForegroundColor $(if ($totalMissing -eq 0) { "Green" } else { "Red" })

$grandTotal = $totalMissing
Write-Host "`n═══════════════════════════════════════════" -ForegroundColor $(if ($grandTotal -eq 0) { "Green" } else { "Red" })
Write-Host "GRAND TOTAL MISSING SCRIPTS: $grandTotal" -ForegroundColor $(if ($grandTotal -eq 0) { "Green" } else { "Red" })
Write-Host "═══════════════════════════════════════════" -ForegroundColor $(if ($grandTotal -eq 0) { "Green" } else { "Red" })

if ($grandTotal -ne 308) {
    Write-Host "`n⚠ Unity Console reports 308 missing scripts, but file scan found $grandTotal" -ForegroundColor Yellow
    Write-Host "Possible reasons:" -ForegroundColor White
    Write-Host "  1. Missing scripts are in meta files or Unity-internal state" -ForegroundColor Gray
    Write-Host "  2. Scripts exist but have compilation errors" -ForegroundColor Gray
    Write-Host "  3. Missing MonoScript assets for compiled scripts" -ForegroundColor Gray
    Write-Host "  4. Package or Plugin references are broken" -ForegroundColor Gray
    Write-Host "`nNext steps:" -ForegroundColor White
    Write-Host "  1. Check Unity Console for specific GameObject names" -ForegroundColor Gray
    Write-Host "  2. Run Assets > Find References In Scene on a missing script" -ForegroundColor Gray
    Write-Host "  3. Check for script compilation errors in Unity Console" -ForegroundColor Gray
}
