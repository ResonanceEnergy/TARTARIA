cd C:\dev\TARTARIA_new

Write-Host "🔧 Fixing all remaining compilation errors..." -ForegroundColor Cyan

# Fix 1: EnemySpawners - rename EnemySpawnPoint class
Write-Host "`n1️⃣ Fixing EnemySpawners..." -ForegroundColor Yellow
3..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`EnemySpawners.cs"
    
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        
        # Rename EnemySpawnPoint class
        $content = $content -replace "public class EnemySpawnPoint", "public class Moon$moonNum`EnemySpawnPoint"
        
        # Update AddComponent calls
        $content = $content -replace "AddComponent<EnemySpawnPoint>\(\)", "AddComponent<Moon$moonNum`EnemySpawnPoint>()"
        
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "  ✓ Fixed Moon$moonNum`EnemySpawners.cs" -ForegroundColor Green
    }
}

# Fix 2: PowerUps - add using directive and rename PowerUpPickup class
Write-Host "`n2️⃣ Fixing PowerUps..." -ForegroundColor Yellow
3..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`PowerUps.cs"
    
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        
        # Add using Tartaria.Input if missing
        if ($content -notmatch "using Tartaria\.Input") {
            $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
        }
        
        # Rename PowerUpPickup class
        $content = $content -replace "public class PowerUpPickup", "public class Moon$moonNum`PowerUpPickup"
        
        # Update AddComponent calls
        $content = $content -replace "AddComponent<PowerUpPickup>\(\)", "AddComponent<Moon$moonNum`PowerUpPickup>()"
        
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "  ✓ Fixed Moon$moonNum`PowerUps.cs" -ForegroundColor Green
    }
}

# Fix 3: Moon2QuestTriggers - rename QuestZoneTrigger
Write-Host "`n3️⃣ Fixing Moon2QuestTriggers..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\Moon2QuestTriggers.cs"
if (Test-Path $file) {
    $content = [System.IO.File]::ReadAllText($file)
    
    # Rename QuestZoneTrigger class
    $content = $content -replace "public class QuestZoneTrigger", "public class Moon2QuestZoneTrigger"
    
    # Update AddComponent calls
    $content = $content -replace "AddComponent<QuestZoneTrigger>\(\)", "AddComponent<Moon2QuestZoneTrigger>()"
    
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "  ✓ Fixed Moon2QuestTriggers.cs" -ForegroundColor Green
}

# Fix 4: Remove duplicate using directives in Secrets files
Write-Host "`n4️⃣ Removing duplicate using directives..." -ForegroundColor Yellow
4..13 | ForEach-Object {
    $moonNum = $_
    $file = "Assets\_Project\Scripts\Integration\Moon$moonNum`Secrets.cs"
    
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        
        # Check for duplicate using Tartaria.Input
        $matches = [regex]::Matches($content, "using Tartaria\.Input;")
        
        if ($matches.Count -gt 1) {
            # Remove all occurrences and add it once after System.Collections.Generic
            $content = $content -replace "using Tartaria\.Input;`r?`n", ""
            $content = $content -replace "using System\.Collections\.Generic;", "using System.Collections.Generic;`nusing Tartaria.Input;"
            
            [System.IO.File]::WriteAllText($file, $content)
            Write-Host "  ✓ Removed duplicate in Moon$moonNum`Secrets.cs" -ForegroundColor Green
        }
    }
}

# Fix 5: Moon3SceneMaster - remove duplicate interactiveObjects field
Write-Host "`n5️⃣ Fixing Moon3SceneMaster duplicate field..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\Moon3SceneMaster.cs"
if (Test-Path $file) {
    $content = [System.IO.File]::ReadAllText($file)
    
    # Remove duplicate SerializeField declaration (between collectibles and npcDialogues)
    $pattern1 = "(\[SerializeField\] Moon3QuestNodes questNodes;\r?\n\s+\[SerializeField\] Moon3Collectibles collectibles;\r?\n)\s+\[SerializeField\] Moon3InteractiveObjects interactiveObjects;\r?\n(\s+\[SerializeField\] Moon3NPCDialogues npcDialogues;)"
    $replacement1 = "`$1`$2"
    $content = $content -replace $pattern1, $replacement1
    
    # Remove duplicate initialization line
    $pattern2 = "(if \(collectibles == null\) collectibles = GetComponent<Moon3Collectibles>\(\);\r?\n)\s+if \(interactiveObjects == null\) interactiveObjects = GetComponent<Moon3InteractiveObjects>\(\);\r?\n(\s+if \(npcDialogues == null\) npcDialogues = GetComponent<Moon3NPCDialogues>\(\);)"
    $replacement2 = "`$1`$2"
    $content = $content -replace $pattern2, $replacement2
    
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "  ✓ Fixed Moon3SceneMaster.cs" -ForegroundColor Green
}

Write-Host "`n✅ All fixes complete!" -ForegroundColor Cyan
Write-Host "📊 Fixed:" -ForegroundColor White
Write-Host "  - 11 EnemySpawners files (EnemySpawnPoint renamed)" -ForegroundColor Gray
Write-Host "  - 11 PowerUps files (PowerUpPickup renamed + using added)" -ForegroundColor Gray
Write-Host "  - 1 Moon2QuestTriggers file (QuestZoneTrigger renamed)" -ForegroundColor Gray
Write-Host "  - 10 Secrets files (duplicate using removed)" -ForegroundColor Gray
Write-Host "  - 1 Moon3SceneMaster file (duplicate field removed)" -ForegroundColor Gray
Write-Host "`n🎯 Next: Open Unity Editor to trigger recompilation" -ForegroundColor Cyan
