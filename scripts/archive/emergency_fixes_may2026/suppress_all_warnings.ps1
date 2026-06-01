# Add #pragma warning disable to files with unused field/variable/event warnings
$suppressions = @'
#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used

'@

$files = @(
    "Assets\_Project\Scripts\Integration\Moon3RailAudioManager.cs",
    "Assets\_Project\Scripts\Integration\CassianNPCController.cs",
    "Assets\_Project\Scripts\Integration\ObjectiveTrackerUI.cs",
    "Assets\_Project\Scripts\Integration\CompanionFarewellSystem.cs",
    "Assets\_Project\Scripts\Integration\Moon9ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\CombatDialogue.cs",
    "Assets\_Project\Scripts\Integration\BossEncounterSystem.cs",
    "Assets\_Project\Scripts\Integration\Moon5OvertoneArc.cs",
    "Assets\_Project\Scripts\Integration\NPCArchetypes.cs",
    "Assets\_Project\Scripts\Integration\SceneFadeTransition.cs",
    "Assets\_Project\Scripts\Integration\Moon2ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\RailEscortController.cs",
    "Assets\_Project\Scripts\Integration\Moon5NPCsAndSystems.cs",
    "Assets\_Project\Scripts\Integration\Moon2DissonanceVeinPuzzle.cs",
    "Assets\_Project\Scripts\Integration\Moon3ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon7ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon6OrganPuzzle.cs",
    "Assets\_Project\Scripts\Integration\QuestLogUIPanel.cs",
    "Assets\_Project\Scripts\Integration\Moon2ExplorationSecrets.cs",
    "Assets\_Project\Scripts\Integration\Moon3ElectricArc.cs",
    "Assets\_Project\Scripts\Integration\Moon6ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon5ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\CombatBridge.cs"
)

$fixed = 0
$skipped = 0

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

        # Skip if already has pragma
        if ($content -match "#pragma warning disable CS0414") {
            Write-Host "[SKIP] $($file.Split('\')[-1]) - already suppressed" -ForegroundColor DarkGray
            $skipped++
            continue
        }

        # Find namespace line
        $lines = $content -split "`r`n|`n"
        $namespaceIndex = -1

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "^namespace ") {
                $namespaceIndex = $i
                break
            }
        }

        if ($namespaceIndex -gt 0) {
            # Insert pragma block before namespace
            $before = $lines[0..($namespaceIndex-1)] -join "`n"
            $after = $lines[$namespaceIndex..($lines.Count-1)] -join "`n"
            $newContent = $before + "`n" + $suppressions + $after

            [System.IO.File]::WriteAllText($file, $newContent, [System.Text.Encoding]::UTF8)
            Write-Host "[OK] $($file.Split('\')[-1])" -ForegroundColor Green
            $fixed++
        } else {
            Write-Host "[WARN] $($file.Split('\')[-1]) - no namespace found" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[MISS] $($file.Split('\')[-1]) - not found" -ForegroundColor Red
    }
}

Write-Host "`nSuppressed warnings in $fixed files ($skipped already done)" -ForegroundColor Cyan
Write-Host "Unity recompiling..." -ForegroundColor Gray
