# Agent 1 - Comprehensive HUDController → GameEvents Conversion
# Replaces all HUDController.Instance calls with GameEvents.RaiseHUDXxx calls

cd c:\dev\TARTARIA_new

$integrationPath = "Assets\_Project\Scripts\Integration"
$files = Get-ChildItem -Path $integrationPath -Filter "*.cs" -Recurse

$replacements = @{
    'HUDController\.Instance\?\.ShowObjective\(' = 'GameEvents.RaiseHUDShowObjective('
    'HUDController\.Instance\?\.ShowDialogue\(' = 'GameEvents.RaiseHUDShowDialogue('
    'HUDController\.Instance\?\.ShowBanner\(' = 'GameEvents.RaiseHUDShowBanner('
    'HUDController\.Instance\?\.ShowSubtitle\(' = 'GameEvents.RaiseHUDShowSubtitle('
    'HUDController\.Instance\?\.ShowMoonTrophy\(' = 'GameEvents.RaiseHUDShowMoonTrophy('
    'HUDController\.Instance\?\.ShowBossHealth\(' = 'GameEvents.RaiseHUDShowBossHealth('
    'HUDController\.Instance\?\.UpdateBossHealth\(' = 'GameEvents.RaiseHUDUpdateBossHealth('
    'HUDController\.Instance\?\.HideBossHealth\(\)' = 'GameEvents.RaiseHUDHideBossHealth()'
    'HUDController\.Instance\?\.ShowInteractionPrompt\(' = 'GameEvents.RaiseHUDShowInteractionPrompt('
    'HUDController\.Instance\?\.HideInteractionPrompt\(\)' = 'GameEvents.RaiseHUDHideInteractionPrompt()'
    'HUDController\.Instance\?\.FlashRSGain\(' = 'GameEvents.RaiseHUDFlashRSGain('
    'HUDController\.Instance\?\.ShowBossNameplate\(' = 'GameEvents.RaiseHUDShowBossNameplate('
    'HUDController\.Instance\?\.ShowEnemyBark\(' = 'GameEvents.RaiseHUDShowEnemyBark('
    'HUDController\.Instance\?\.ShowCorruptionWhisper\(' = 'GameEvents.RaiseHUDShowCorruptionWhisper('
    'HUDController\.Instance\?\.UpdateFrequencyWheel\(' = 'GameEvents.RaiseHUDUpdateFrequencyWheel('
    'UI\.HUDController\.Instance\?\.ShowObjective\(' = 'GameEvents.RaiseHUDShowObjective('
    'UI\.HUDController\.Instance\?\.ShowDialogue\(' = 'GameEvents.RaiseHUDShowDialogue('
    'UI\.HUDController\.Instance\?\.ShowBanner\(' = 'GameEvents.RaiseHUDShowBanner('
    'UI\.HUDController\.Instance\?\.ShowSubtitle\(' = 'GameEvents.RaiseHUDShowSubtitle('
    'UI\.HUDController\.Instance\?\.ShowMoonTrophy\(' = 'GameEvents.RaiseHUDShowMoonTrophy('
    'UI\.HUDController\.Instance\?\.ShowBossHealth\(' = 'GameEvents.RaiseHUDShowBossHealth('
    'UI\.HUDController\.Instance\?\.UpdateBossHealth\(' = 'GameEvents.RaiseHUDUpdateBossHealth('
    'UI\.HUDController\.Instance\?\.HideBossHealth\(\)' = 'GameEvents.RaiseHUDHideBossHealth()'
    'UI\.HUDController\.Instance\?\.ShowInteractionPrompt\(' = 'GameEvents.RaiseHUDShowInteractionPrompt('
    'UI\.HUDController\.Instance\?\.HideInteractionPrompt\(\)' = 'GameEvents.RaiseHUDHideInteractionPrompt()'
    'UI\.HUDController\.Instance\?\.UpdateFrequencyWheel\(' = 'GameEvents.RaiseHUDUpdateFrequencyWheel('
}

$totalReplacements = 0
$filesModified = 0

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    $modified = $false
    $fileReplacements = 0
    
    foreach ($pattern in $replacements.Keys) {
        if ($content -match $pattern) {
            $content = $content -replace $pattern, $replacements[$pattern]
            $modified = $true
            $matchCount = ([regex]::Matches($content, [regex]::Escape($replacements[$pattern]))).Count
            $fileReplacements += $matchCount
        }
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $filesModified++
        $totalReplacements += $fileReplacements
        Write-Host "✓ $($file.Name) - $fileReplacements replacements" -ForegroundColor Green
    }
}

Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "HUDController → GameEvents Conversion Complete" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Files modified: $filesModified" -ForegroundColor Yellow
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Yellow
