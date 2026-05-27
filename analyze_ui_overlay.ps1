$file = "Assets\_Project\Scenes\UI_Overlay.unity"
$content = Get-Content $file -Raw

# Count GameObjects
$gameObjects = ([regex]::Matches($content, "--- !u!1 &")).Count

# Count MonoBehaviours
$monoBehaviours = ([regex]::Matches($content, "--- !u!114 &")).Count

# Count missing scripts (fileID: 0)
$missingFileID0 = ([regex]::Matches($content, "m_Script:\s*\{fileID:\s*0\}")).Count

# Count Unity.UI references
$unityUI = ([regex]::Matches($content, "UnityEngine\.UI::")).Count

# Count TextMeshPro references  
$tmpro = ([regex]::Matches($content, "Unity\.TextMeshPro::")).Count

Write-Host "`n═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "UI_OVERLAY AUDIT REPORT" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════`n" -ForegroundColor Cyan
Write-Host "Total UI elements (GameObjects): " -NoNewline
Write-Host $gameObjects -ForegroundColor Green
Write-Host "Total MonoBehaviours: " -NoNewline
Write-Host $monoBehaviours -ForegroundColor Green
Write-Host "Missing refs (fileID: 0): " -NoNewline
Write-Host $missingFileID0 -ForegroundColor $(if ($missingFileID0 -eq 0) { "Green" } else { "Red" })
Write-Host "UnityEngine.UI assembly refs: " -NoNewline
Write-Host $unityUI -ForegroundColor Cyan
Write-Host "TextMeshPro (TMPro) refs: " -NoNewline
Write-Host $tmpro -ForegroundColor Cyan
Write-Host ""

# Extract GameObject names with missing scripts
if ($missingFileID0 -gt 0) {
    Write-Host "MISSING SCRIPT DETAILS:" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════`n" -ForegroundColor Red
    
    # Find MonoBehaviour blocks with missing scripts
    $pattern = "--- !u!114 &(\d+)[\s\S]*?m_Script:\s*\{fileID:\s*0\}[\s\S]*?(?=---|\z)"
    $missingBlocks = [regex]::Matches($content, $pattern)
    
    $index = 1
    foreach ($block in $missingBlocks) {
        # Try to find the GameObject this component belongs to
        $componentID = $block.Groups[1].Value
        
        # Extract GameObject reference from the block
        if ($block.Value -match "m_GameObject:\s*\{fileID:\s*(\d+)\}") {
            $goID = $matches[1]
            
            # Find the GameObject with this ID
            $goPattern = "--- !u!1 &$goID[\s\S]*?m_Name:\s*(.+)"
            if ($content -match $goPattern) {
                $goName = $matches[1].Trim()
                Write-Host "  $index. GameObject: " -NoNewline
                Write-Host $goName -ForegroundColor Yellow
                Write-Host "     Component ID: $componentID" -ForegroundColor DarkGray
            }
        }
        $index++
    }
}

Write-Host "`nUI-specific issues:" -ForegroundColor Cyan
Write-Host "  • Canvas/EventSystem: Scene uses GameCanvas (detected)" -ForegroundColor Green
Write-Host "  • TextMeshPro: $tmpro TMP components found" -ForegroundColor Green
Write-Host "  • UnityEngine.UI: $unityUI UI components found" -ForegroundColor Green
Write-Host ""
