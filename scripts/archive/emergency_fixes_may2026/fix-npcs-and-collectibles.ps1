cd C:\dev\TARTARIA_new

# Re-add using Tartaria.Input to Moon7-9 InteractiveObjects (got lost)
@(7,8,9) | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`InteractiveObjects.cs"

    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)

        # Add using Tartaria.Input if missing
        if ($content -notmatch "using Tartaria\.Input") {
            $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "✓ Re-added using directive to Moon$moonNum`InteractiveObjects.cs" -ForegroundColor Green
        }
    }
}

# Fix Moon3-13 NPCDialogues (rename DialogueNPC class)
3..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`NPCDialogues.cs"

    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)

        # Add using Tartaria.Input
        if ($content -notmatch "using Tartaria\.Input") {
            $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
        }

        # Rename DialogueNPC class
        $content = $content -replace "public class DialogueNPC", "public class Moon$moonNum`DialogueNPC"

        # Update AddComponent calls
        $content = $content -replace "AddComponent<DialogueNPC>\(\)", "AddComponent<Moon$moonNum`DialogueNPC>()"

        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "✓ Fixed Moon$moonNum`NPCDialogues.cs" -ForegroundColor Green
    }
}

# Fix Moon3Collectibles
$file = "Assets\_Project\Scripts\Integration\Moon3Collectibles.cs"
if (Test-Path $file) {
    $content = [System.IO.File]::ReadAllText($file)
    if ($content -notmatch "using Tartaria\.Input") {
        $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "✓ Fixed Moon3Collectibles.cs" -ForegroundColor Green
    }
}

Write-Host "`n✅ All NPCDialogues and IInteractable fixes complete!" -ForegroundColor Cyan
