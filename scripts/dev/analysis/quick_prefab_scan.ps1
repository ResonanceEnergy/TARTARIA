cd C:\dev\TARTARIA_new

Write-Host "`n═══ PREFAB SCAN ═══`n" -ForegroundColor Yellow

$allPrefabs = Get-ChildItem -Path "Assets\_Project" -Filter "*.prefab" -Recurse

Write-Host "Scanning $($allPrefabs.Count) prefabs...`n" -ForegroundColor Cyan

$results = @()
$totalMissing = 0

foreach ($prefab in $allPrefabs) {
    $content = Get-Content $prefab.FullName -Raw
    
    $missing = ([regex]::Matches($content, 'm_Script: \{fileID: 0\}|m_Script: \{fileID: [^,]+, guid: 00000000000000000000000000000000')).Count
    
    if ($missing -gt 0) {
        $totalMissing += $missing
        $results += [PSCustomObject]@{
            Prefab = $prefab.Name
            Missing = $missing
            Path = $prefab.FullName.Replace("$PWD\", "")
        }
    }
}

$results | Sort-Object -Property Missing -Descending | Format-Table -AutoSize

Write-Host "`nAffected Prefabs: $($results.Count)" -ForegroundColor Yellow
Write-Host "Total Missing Scripts: $totalMissing" -ForegroundColor Red
