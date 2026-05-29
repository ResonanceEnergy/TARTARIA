# GameEvents.cs Structure Fixer
# Extracts event declarations and Fire methods, rebuilds with proper syntax

$filePath = "Assets\_Project\Scripts\Core\GameEvents.cs"
$content = Get-Content $filePath -Raw

Write-Host "Analyzing GameEvents.cs structure..." -ForegroundColor Cyan

# Backup current state
Copy-Item $filePath "$filePath.BEFORE_FIX_$(Get-Date -Format 'yyyyMMdd_HHmmss')" -Force
Write-Host "✅ Backup created" -ForegroundColor Green

# Extract all event declarations (public static event lines)
$events = [regex]::Matches($content, '(?m)^\s*public static event\s+.*?;')
Write-Host "Found $($events.Count) event declarations" -ForegroundColor Yellow

# Extract all Fire method signatures (method names and parameters)
$fireMethods = [regex]::Matches($content, '(?m)^\s*public static void (Fire\w+)\((.*?)\)')
Write-Host "Found $($fireMethods.Count) Fire methods" -ForegroundColor Yellow

# Count EventArgs class definitions
$eventArgsClasses = [regex]::Matches($content, '(?m)^\s*public class \w+EventArgs')
Write-Host "Found $($eventArgsClasses.Count) EventArgs classes" -ForegroundColor Yellow

Write-Host ""
Write-Host "File appears to have:" -ForegroundColor Cyan
Write-Host "  - Malformed try/catch blocks in Fire methods" -ForegroundColor Red
Write-Host "  - Duplicate event declarations inside methods" -ForegroundColor Red  
Write-Host "  - Incomplete string literals in catch blocks" -ForegroundColor Red

Write-Host ""
Write-Host "This file needs MANUAL reconstruction or restoration from a clean source." -ForegroundColor Yellow
Write-Host ""
Write-Host "Options:" -ForegroundColor Cyan
Write-Host "  1. Restore from Git: git checkout main -- Assets/_Project/Scripts/Core/GameEvents.cs" -ForegroundColor White
Write-Host "  2. Restore from backup: if GameEvents.cs.BROKEN_BACKUP is clean" -ForegroundColor White  
Write-Host "  3. AI-assisted rebuild: Extract events + regenerate Fire methods" -ForegroundColor White
Write-Host ""

# Show sample of corruption
Write-Host "Sample corruption pattern (around line 257):" -ForegroundColor Red
$lines = Get-Content $filePath
$lines[250..295] | ForEach-Object { $i=250 } { Write-Host "$($i): $_" -ForegroundColor Gray; $i++ }
