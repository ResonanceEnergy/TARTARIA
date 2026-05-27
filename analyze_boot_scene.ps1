cd C:\dev\TARTARIA_new

Write-Host "`n═══ BOOT.UNITY AUDIT REPORT ═══`n" -ForegroundColor Cyan

# Parse Boot.unity
$bootUnityPath = "Assets\_Project\Scenes\Boot.unity"
$bootUnity = Get-Content $bootUnityPath -Raw

# Count GameObjects
$gameObjectMatches = [regex]::Matches($bootUnity, '--- !u!1 &(\d+)\s+GameObject:')
$totalGameObjects = $gameObjectMatches.Count
Write-Host "Total GameObjects: $totalGameObjects" -ForegroundColor White

# Extract GameObject names
$gameObjectMap = @{}
foreach ($match in $gameObjectMatches) {
    $id = $match.Groups[1].Value
    # Find the name for this GameObject
    if ($bootUnity -match "(?s)--- !u!1 &$id\s+GameObject:.*?m_Name: (.+?)[\r\n]") {
        $gameObjectMap[$id] = $Matches[1]
    }
}

# Count total components
$totalComponents = ([regex]::Matches($bootUnity, '--- !u!\d+ &\d+')).Count
Write-Host "Total Component Blocks: $totalComponents" -ForegroundColor White

# Extract all MonoBehaviour components with GUIDs
$monoBehaviourPattern = '(?s)--- !u!114 &(\d+)\s+MonoBehaviour:.*?m_GameObject: \{fileID: (\d+)\}.*?m_Script: \{fileID: [^,]+, guid: ([a-f0-9]{32}), type: \d+\}'
$monoBehaviours = [regex]::Matches($bootUnity, $monoBehaviourPattern)

Write-Host "Total MonoBehaviour Components: $($monoBehaviours.Count)" -ForegroundColor White

# Check for missing script patterns
$missingPattern1 = 'm_Script: \{fileID: 0\}'
$missingPattern2 = 'm_Script: \{fileID: [^,]+, guid: 00000000000000000000000000000000'

$missing1 = ([regex]::Matches($bootUnity, $missingPattern1)).Count
$missing2 = ([regex]::Matches($bootUnity, $missingPattern2)).Count

Write-Host "Missing Script References (fileID: 0): $missing1" -ForegroundColor Yellow
Write-Host "Missing Script References (null GUID): $missing2" -ForegroundColor Yellow
Write-Host "Total Missing Script References: $($missing1 + $missing2)" -ForegroundColor Red

# Build GUID mapping
$guidInfo = @{}
foreach ($match in $monoBehaviours) {
    $componentID = $match.Groups[1].Value
    $gameObjectID = $match.Groups[2].Value
    $guid = $match.Groups[3].Value
    
    if (-not $guidInfo.ContainsKey($guid)) {
        $guidInfo[$guid] = @{
            GUID = $guid
            GameObjectID = $gameObjectID
            GameObjectName = $gameObjectMap[$gameObjectID]
            ComponentID = $componentID
            Count = 1
        }
    } else {
        $guidInfo[$guid].Count++
    }
}

Write-Host "`nUnique Script GUIDs Referenced: $($guidInfo.Count)" -ForegroundColor White

# Check for corresponding .meta files
Write-Host "`n=== Checking for .meta Files ===" -ForegroundColor Cyan
$results = @()

foreach ($guid in $guidInfo.Keys) {
    $info = $guidInfo[$guid]
    
    # Search for .meta file with this GUID
    $metaFiles = Get-ChildItem -Path "Assets\_Project\Scripts" -Filter "*.meta" -Recurse -ErrorAction SilentlyContinue | 
        Where-Object { 
            $content = Get-Content $_.FullName -Raw
            $content -match "guid: $guid"
        }
    
    $status = if ($metaFiles) { "FOUND" } else { "MISSING" }
    $scriptPath = if ($metaFiles) { 
        $metaFiles[0].FullName.Replace("$PWD\", "").Replace(".meta", "")
    } else { 
        "N/A" 
    }
    
    $results += [PSCustomObject]@{
        Status = $status
        GUID = $guid
        GameObject = $info.GameObjectName
        ScriptPath = $scriptPath
        UsageCount = $info.Count
    }
}

# Summary
$foundScripts = ($results | Where-Object { $_.Status -eq "FOUND" }).Count
$missingScripts = ($results | Where-Object { $_.Status -eq "MISSING" }).Count

Write-Host "`nScripts FOUND: $foundScripts" -ForegroundColor Green
Write-Host "Scripts MISSING: $missingScripts" -ForegroundColor Red

# Top 10 Missing Scripts
$missingOnly = $results | Where-Object { $_.Status -eq "MISSING" } | Sort-Object -Property UsageCount -Descending | Select-Object -First 10

if ($missingOnly) {
    Write-Host "`n=== TOP 10 MISSING SCRIPT GUIDs ===" -ForegroundColor Red
    $counter = 1
    foreach ($item in $missingOnly) {
        Write-Host "$counter. GUID: $($item.GUID) on GameObject: '$($item.GameObject)' (used $($item.UsageCount) time(s))" -ForegroundColor Yellow
        $counter++
    }
}

# All Found Scripts (for reference)
$foundOnly = $results | Where-Object { $_.Status -eq "FOUND" }
if ($foundOnly) {
    Write-Host "`n=== VALID SCRIPT REFERENCES ===" -ForegroundColor Green
    $foundOnly | Format-Table -AutoSize
}

Write-Host "`n=== RECOMMENDATIONS ===" -ForegroundColor Cyan
Write-Host "1. $missingScripts script(s) in Boot.unity are missing from Assets\_Project\Scripts\" -ForegroundColor White
Write-Host "2. Check if scripts were moved/renamed/deleted" -ForegroundColor White  
Write-Host "3. Run Unity Asset Database refresh to regenerate .meta files if scripts exist" -ForegroundColor White
Write-Host "4. Remove missing components from GameObjects if scripts are permanently deleted" -ForegroundColor White
Write-Host "`n═══ END REPORT ═══`n" -ForegroundColor Cyan
