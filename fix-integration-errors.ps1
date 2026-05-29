#!/usr/bin/env pwsh
# Fix remaining Integration layer compilation errors

cd c:\dev\TARTARIA_new

Write-Host "=== Fixing Integration Layer Errors ===" -ForegroundColor Cyan

# Fix HUDController references (need using directive)
$files = @(
    "Assets\_Project\Scripts\Integration\PipeOrganMiniGame.cs",
    "Assets\_Project\Scripts\Integration\TuningMiniGameRestorationSystem.cs",
    "Assets\_Project\Scripts\Integration\PlayerAbilitiesComplete.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -notmatch "using Tartaria\.UI;") {
            # Add using directive after other usings
            $content = $content -replace '(using Tartaria\.Integration;)', "`$1`nusing Tartaria.UI;"
            [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
            Write-Host "✅ Added using Tartaria.UI to $file" -ForegroundColor Green
        }
    }
}

# Fix Tartaria.Input.GetKeyDown references (should be Input.GetKeyDown)
$file = "Assets\_Project\Scripts\Integration\PlayerAbilitiesComplete.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    $original = $content
    $content = $content -replace 'Tartaria\.Input\.GetKeyDown', 'Input.GetKeyDown'
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Fixed Input.GetKeyDown in $file" -ForegroundColor Green
    }
}

# Fix Camera.main reference (needs using UnityEngine)
$file = "Assets\_Project\Scripts\Integration\AudioFeedbackController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    # Change Tartaria.Camera.main to Camera.main
    $original = $content
    $content = $content -replace 'Tartaria\.Camera\.main', 'Camera.main'
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Fixed Camera.main reference in $file" -ForegroundColor Green
    }
}

Write-Host "`n=== Done ===" -ForegroundColor Cyan
