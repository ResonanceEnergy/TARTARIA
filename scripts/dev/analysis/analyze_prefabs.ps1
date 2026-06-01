cd C:\dev\TARTARIA_new

Write-Host "`n═══ PREFAB MISSING SCRIPT SCAN ═══`n" -ForegroundColor Yellow

$allPrefabs = Get-ChildItem -Path "Assets\_Project" -Filter "*.prefab" -Recurse -ErrorAction SilentlyContinue

Write-Host "Found $($allPrefabs.Count) prefab(s). Scanning for missing scripts...`n" -ForegroundColor Cyan

$prefabResults = @()
$detailedMissing = @()
$counter = 0

foreach ($prefab in $allPrefabs) {
    $counter++
    Write-Progress -Activity "Scanning Prefabs" -Status "$counter of $($allPrefabs.Count)" -PercentComplete (($counter / $allPrefabs.Count) * 100)
    
    $content = Get-Content $prefab.FullName -Raw
    
    # Count missing scripts
    $missing1 = [regex]::Matches($content, 'm_Script: \{fileID: 0\}')
    $missing2 = [regex]::Matches($content, 'm_Script: \{fileID: [^,]+, guid: 00000000000000000000000000000000')
    $totalMissing = $missing1.Count + $missing2.Count
    
    if ($totalMissing -gt 0) {
        # Extract GameObject names for this prefab
        $gameObjectMatches = [regex]::Matches($content, '--- !u!1 &(\d+)\s+GameObject:[\s\S]*?m_Name: (.+)')
        $gameObjectMap = @{}
        foreach ($match in $gameObjectMatches) {
            $gameObjectMap[$match.Groups[1].Value] = $match.Groups[2].Value
        }
        
        # Extract GUIDs from missing scripts
        $guidMatches = [regex]::Matches($content, '(?s)--- !u!114 &(\d+)\s+MonoBehaviour:.*?m_GameObject: \{fileID: (\d+)\}.*?m_Script: \{fileID: [^,]+, guid: ([a-f0-9]{32}), type: \d+\}')
        
        foreach ($guidMatch in $guidMatches) {
            $guid = $guidMatch.Groups[3].Value
            if ($guid -eq "00000000000000000000000000000000") {
                $gameObjectID = $guidMatch.Groups[2].Value
                $gameObjectName = $gameObjectMap[$gameObjectID]
                
                $detailedMissing += [PSCustomObject]@{
                    Prefab = $prefab.Name
                    GameObject = $gameObjectName
                    GUID = $guid
                    PrefabPath = $prefab.FullName.Replace("$PWD\", "")
                }
            }
        }
    }
    
    $prefabResults += [PSCustomObject]@{
        Prefab = $prefab.Name
        Missing = $totalMissing
        Path = $prefab.FullName.Replace("$PWD\", "")
    }
}

Write-Progress -Activity "Scanning Prefabs" -Completed

# Show prefabs with missing scripts
$problemPrefabs = $prefabResults | Where-Object { $_.Missing -gt 0 } | Sort-Object -Property Missing -Descending

if ($problemPrefabs) {
    Write-Host "`n═══ PREFABS WITH MISSING SCRIPTS ═══`n" -ForegroundColor Red
    $problemPrefabs | Format-Table -AutoSize
    
    Write-Host "`n═══ TOP 10 PROBLEM PREFABS ═══`n" -ForegroundColor Yellow
    $top10 = $problemPrefabs | Select-Object -First 10
    $rank = 1
    foreach ($item in $top10) {
        Write-Host "$rank. $($item.Prefab)" -ForegroundColor Red
        Write-Host "   Missing: $($item.Missing)" -ForegroundColor White
        Write-Host "   Path: $($item.Path)`n" -ForegroundColor Gray
        $rank++
    }
}

$totalMissingInPrefabs = ($prefabResults | Measure-Object -Property Missing -Sum).Sum
$affectedPrefabs = ($prefabResults | Where-Object { $_.Missing -gt 0 }).Count

Write-Host "`n═══════════════════════════════════════════" -ForegroundColor Red
Write-Host "TOTAL MISSING SCRIPTS IN PREFABS: $totalMissingInPrefabs" -ForegroundColor Red
Write-Host "AFFECTED PREFABS: $affectedPrefabs of $($allPrefabs.Count)" -ForegroundColor Red
Write-Host "═══════════════════════════════════════════" -ForegroundColor Red

if ($totalMissingInPrefabs -eq 308) {
    Write-Host "`n✓ This matches the reported 308 missing script references!" -ForegroundColor Yellow
}

# Export detailed list
if ($detailedMissing) {
    $outputPath = "missing_scripts_detailed.csv"
    $detailedMissing | Export-Csv -Path $outputPath -NoTypeInformation
    Write-Host "`nDetailed missing script list exported to: $outputPath" -ForegroundColor Cyan
}
