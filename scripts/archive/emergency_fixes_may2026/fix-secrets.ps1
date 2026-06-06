cd C:\dev\TARTARIA_new

# Fix Moon4-13 Secrets files
4..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`Secrets.cs"

    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)

        # Add using Tartaria.Input
        if ($content -notmatch "using Tartaria\.Input") {
            $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
        }

        # Rename SecretInteractable class
        $content = $content -replace "public class SecretInteractable", "public class Moon$moonNum`SecretInteractable"

        # Update AddComponent calls
        $content = $content -replace "AddComponent<SecretInteractable>\(\)", "AddComponent<Moon$moonNum`SecretInteractable>()"

        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "✓ Fixed Moon$moonNum`Secrets.cs" -ForegroundColor Green
    }
}

# Fix Moon3,10-13 InteractiveObjects (rename InteractableObject class)
@(3,10,11,12,13) | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`InteractiveObjects.cs"

    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)

        # Rename InteractableObject class
        $content = $content -replace "public class InteractableObject", "public class Moon$moonNum`InteractableObject"

        # Update AddComponent calls
        $content = $content -replace "AddComponent<InteractableObject>\(\)", "AddComponent<Moon$moonNum`InteractableObject>()"

        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "✓ Fixed Moon$moonNum`InteractiveObjects.cs" -ForegroundColor Green
    }
}

Write-Host "`n✅ All fixes complete!" -ForegroundColor Cyan
