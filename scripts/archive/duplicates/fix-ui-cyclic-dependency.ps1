# Agent 1 - Cyclic Dependency Fix Script
# Removes 'using Tartaria.UI;' from all Integration files that don't actually need it
# Verifies which files have real HUDController.Instance calls

$integrationPath = "c:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration"
$files = Get-ChildItem -Path $integrationPath -Filter "*.cs" -Recurse

$report = @()

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Check if file has 'using Tartaria.UI;'
    if ($content -match 'using Tartaria\.UI;') {
        
        # Check if file actually uses HUDController or other UI types
        $usesHUD = $content -match 'HUDController\.Instance|UI\.HUDController'
        $usesDialoguePanel = $content -match 'DialoguePanel\.Instance'
        $usesQuestTracker = $content -match 'QuestTracker\.Instance'
        $usesFadeCanvas = $content -match 'FadeCanvas\.Instance'
        $usesNotificationToast = $content -match 'NotificationToast\.Instance'
        $usesUIManager = $content -match 'UIManager\.Instance'
        
        $hasAnyUICall = $usesHUD -or $usesDialoguePanel -or $usesQuestTracker -or $usesFadeCanvas -or $usesNotificationToast -or $usesUIManager
        
        $report += [PSCustomObject]@{
            File = $file.Name
            HasUIUsing = $true
            UsesHUD = $usesHUD
            UsesDialoguePanel = $usesDialoguePanel
            UsesQuestTracker = $usesQuestTracker
            UsesFadeCanvas = $usesFadeCanvas
            UsesNotificationToast = $usesNotificationToast
            UsesUIManager = $usesUIManager
            HasAnyUICall = $hasAnyUICall
        }
        
        # If file has 'using Tartaria.UI;' but NO UI calls, remove the using
        if (-not $hasAnyUICall) {
            Write-Host "Removing unused 'using Tartaria.UI;' from $($file.Name)" -ForegroundColor Green
            $newContent = $content -replace 'using Tartaria\.UI;\r?\n', ''
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
        }
    }
}

# Export report
$report | Where-Object { $_.HasAnyUICall } | Format-Table -AutoSize
Write-Host "`nFiles WITH UI calls that need GameEvents conversion: $($report | Where-Object { $_.HasAnyUICall } | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Yellow
Write-Host "Files with unused UI usings (cleaned): $($report | Where-Object { -not $_.HasAnyUICall } | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Green
