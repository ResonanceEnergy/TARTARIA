# Suppress future-feature unused field/event warnings
$files = @(
    "Assets\_Project\Scripts\Integration\Moon3RailAudioManager.cs",
    "Assets\_Project\Scripts\Integration\CassianNPCController.cs",
    "Assets\_Project\Scripts\Integration\NPCArchetypes.cs",
    "Assets\_Project\Scripts\Integration\RailEscortController.cs",
    "Assets\_Project\Scripts\Integration\ObjectiveTrackerUI.cs",
    "Assets\_Project\Scripts\Integration\Moon2ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\CombatBridge.cs",
    "Assets\_Project\Scripts\Integration\QuestLogUIPanel.cs",
    "Assets\_Project\Scripts\Integration\Moon2ExplorationSecrets.cs",
    "Assets\_Project\Scripts\Integration\Moon5NPCsAndSystems.cs",
    "Assets\_Project\Scripts\Integration\CompanionFarewellSystem.cs",
    "Assets\_Project\Scripts\Integration\Moon2DissonanceVeinPuzzle.cs",
    "Assets\_Project\Scripts\Integration\CombatDialogue.cs",
    "Assets\_Project\Scripts\Integration\BossEncounterSystem.cs",
    "Assets\_Project\Scripts\Integration\Moon7ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon3ElectricArc.cs",
    "Assets\_Project\Scripts\Integration\Moon5ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon6ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon9ContentSpawner.cs",
    "Assets\_Project\Scripts\Integration\Moon5OvertoneArc.cs",
    "Assets\_Project\Scripts\Integration\Moon6OrganPuzzle.cs",
    "Assets\_Project\Scripts\Integration\SceneFadeTransition.cs",
    "Assets\_Project\Scripts\Integration\Moon3ContentSpawner.cs"
)

$suppressBlock = @"
#pragma warning disable CS0067  // Event never used (future integration)
#pragma warning disable CS0219  // Variable assigned but not used (future integration)
#pragma warning disable CS0414  // Field assigned but not used (future integration)

"@

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw

        # Check if pragma already exists
        if ($content -notmatch "#pragma warning disable CS0414") {
            # Find the first using or namespace line
            $lines = $content -split "`r`n|`n"
            $insertIndex = 0

            for ($i = 0; $i -lt $lines.Count; $i++) {
                if ($lines[$i] -match "^(using |namespace )") {
                    $insertIndex = $i
                    break
                }
            }

            # Insert pragma block after usings, before namespace
            $beforeNamespace = $lines[0..$insertIndex] -join "`n"
            $afterNamespace = $lines[($insertIndex+1)..($lines.Count-1)] -join "`n"

            # Find first namespace line
            $namespaceIndex = -1
            for ($i = 0; $i -lt $lines.Count; $i++) {
                if ($lines[$i] -match "^namespace ") {
                    $namespaceIndex = $i
                    break
                }
            }

            if ($namespaceIndex -gt 0) {
                $before = $lines[0..($namespaceIndex-1)] -join "`n"
                $after = $lines[$namespaceIndex..($lines.Count-1)] -join "`n"
                $newContent = $before + "`n`n" + $suppressBlock + $after
                [System.IO.File]::WriteAllText($file, $newContent, [System.Text.Encoding]::UTF8)
                Write-Host "[OK] $file" -ForegroundColor Green
            } else {
                Write-Host "[SKIP] $file - no namespace found" -ForegroundColor Yellow
            }
        } else {
            Write-Host "[SKIP] $file - already suppressed" -ForegroundColor Gray
        }
    } else {
        Write-Host "[MISS] $file - not found" -ForegroundColor Red
    }
}

Write-Host "`nWarning suppression complete" -ForegroundColor Cyan
