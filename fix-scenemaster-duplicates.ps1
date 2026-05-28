cd C:\dev\TARTARIA_new

# Fix Moon5-13 SceneMaster duplicate fields
5..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`SceneMaster.cs"

    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        $original = $content

        # Remove duplicate SerializeField declaration (between collectibles and npcDialogues)
        $pattern1 = "(\[SerializeField\] Moon$moonNum`QuestNodes questNodes;\r?\n\s+\[SerializeField\] Moon$moonNum`Collectibles collectibles;\r?\n)\s+\[SerializeField\] Moon$moonNum`InteractiveObjects interactiveObjects;\r?\n(\s+\[SerializeField\] Moon$moonNum`NPCDialogues npcDialogues;)"
        $replacement1 = "`$1`$2"
        $content = $content -replace $pattern1, $replacement1

        # Remove duplicate initialization line (between collectibles and npcDialogues)
        $pattern2 = "(if \(collectibles == null\) collectibles = GetComponent<Moon$moonNum`Collectibles>\(\);\r?\n)\s+if \(interactiveObjects == null\) interactiveObjects = GetComponent<Moon$moonNum`InteractiveObjects>\(\);\r?\n(\s+if \(npcDialogues == null\) npcDialogues = GetComponent<Moon$moonNum`NPCDialogues>\(\);)"
        $replacement2 = "`$1`$2"
        $content = $content -replace $pattern2, $replacement2

        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "✓ Fixed Moon$moonNum`SceneMaster.cs" -ForegroundColor Green
        } else {
            Write-Host "- Moon$moonNum`SceneMaster.cs (no duplicates found)" -ForegroundColor Gray
        }
    }
}

Write-Host "`n✅ All SceneMaster fixes complete!" -ForegroundColor Cyan
