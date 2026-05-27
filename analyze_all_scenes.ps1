cd C:\dev\TARTARIA_new

Write-Host "`n═══ BOOT.UNITY DETAILED SCRIPT INVENTORY ═══`n" -ForegroundColor Cyan

$guids = @(
    "70791d4d01c38ca4e9a856114ef16721",
    "df3c9216c944c46418bc2df606c15ae1", 
    "f53e59b4f0f90f14da09613f93491f59",
    "a6c5ca6dc6f0b4b419645792486651e9",
    "0f062e02d10563545a8210a196dbe77f"
)

foreach ($guid in $guids) {
    $metaFile = Get-ChildItem -Path "Assets\_Project\Scripts" -Filter "*.meta" -Recurse | 
        Where-Object { (Get-Content $_.FullName -Raw) -match "guid: $guid" } | 
        Select-Object -First 1
    
    if ($metaFile) {
        $scriptFile = $metaFile.FullName.Replace(".meta", "").Replace("$PWD\", "")
        $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile)
        $className = if (Test-Path $metaFile.FullName.Replace(".meta", "")) {
            $csContent = Get-Content $metaFile.FullName.Replace(".meta", "") -Raw
            if ($csContent -match 'public class (\w+)') {
                $Matches[1]
            } else {
                "N/A"
            }
        } else {
            "N/A"
        }
        
        Write-Host "✓ $scriptName" -ForegroundColor Green
        Write-Host "  Class: $className" -ForegroundColor White
        Write-Host "  Path: $scriptFile" -ForegroundColor Gray
        Write-Host "  GUID: $guid`n" -ForegroundColor DarkGray
    }
}

Write-Host "`n═══ PROJECT-WIDE SCENE SCAN ═══`n" -ForegroundColor Yellow

$allScenes = Get-ChildItem -Path "Assets\_Project\Scenes" -Filter "*.unity" -Recurse -ErrorAction SilentlyContinue

Write-Host "Found $($allScenes.Count) scene(s). Scanning for missing scripts...`n" -ForegroundColor Cyan

$sceneResults = @()
foreach ($scene in $allScenes) {
    $content = Get-Content $scene.FullName -Raw
    $missing1 = ([regex]::Matches($content, 'm_Script: \{fileID: 0\}')).Count
    $missing2 = ([regex]::Matches($content, 'm_Script: \{fileID: [^,]+, guid: 00000000000000000000000000000000')).Count
    $totalMissing = $missing1 + $missing2
    
    $sceneResults += [PSCustomObject]@{
        Scene = $scene.Name
        Missing = $totalMissing
        Path = $scene.FullName.Replace("$PWD\", "")
    }
}

$sceneResults | Sort-Object -Property Missing -Descending | Format-Table -AutoSize

$totalMissingInProject = ($sceneResults | Measure-Object -Property Missing -Sum).Sum
$color = if ($totalMissingInProject -eq 0) { "Green" } else { "Red" }
Write-Host "═══════════════════════════════════════════" -ForegroundColor $color
Write-Host "TOTAL MISSING SCRIPTS ACROSS ALL SCENES: $totalMissingInProject" -ForegroundColor $color  
Write-Host "═══════════════════════════════════════════" -ForegroundColor $color

if ($totalMissingInProject -eq 308) {
    Write-Host "`n✓ This matches the reported 308 missing script references" -ForegroundColor Yellow
}
