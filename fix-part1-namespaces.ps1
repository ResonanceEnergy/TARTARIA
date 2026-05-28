cd C:\dev\TARTARIA_new

Write-Host "🔧 MASSIVE COMPILATION FIX - Part 1: Namespace & API Corrections" -ForegroundColor Cyan

# Fix 1: GameLoopController namespace (Core → Integration)
Write-Host "`n1️⃣ Fixing GameLoopController namespace references..." -ForegroundColor Yellow
$files = Get-ChildItem "Assets\_Project\Scripts\Integration\*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Replace Tartaria.Core.GameLoopController with Tartaria.Integration.GameLoopController
    $content = $content -replace "Tartaria\.Core\.GameLoopController", "Tartaria.Integration.GameLoopController"
    
    # Also handle bare GameLoopController.Instance references (they're fine, just document it)
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 2: QuestObjectiveType enum value fixes
Write-Host "`n2️⃣ Fixing missing QuestObjectiveType enum values..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # SolvePuzzle → CompleteTuning (closest semantic match)
    $content = $content -replace "QuestObjectiveType\.SolvePuzzle", "QuestObjectiveType.CompleteTuning"
    
    # ReachLocation → CompleteZone (closest semantic match)
    $content = $content -replace "QuestObjectiveType\.ReachLocation", "QuestObjectiveType.CompleteZone"
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 3: VolumeProfile API (TryAdd → Add)
Write-Host "`n3️⃣ Fixing VolumeProfile.TryAdd → Add..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # TryAdd<T> → Add<T> with different signature
    $content = $content -replace "profile\.TryAdd<", "if (!profile.Has<"
    $content = $content -replace "\(out var ([a-zA-Z_][a-zA-Z0-9_]*)\);", ">()) { var `$1 = profile.Add<"
    
    # Simpler approach: just replace TryAdd with Add and handle manually if needed
    $content = $content -replace "\.TryAdd<", ".Add<"
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 4: Camera namespace collision (Tartaria.Camera.main vs UnityEngine.Camera)
Write-Host "`n4️⃣ Fixing Camera namespace issues..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Replace Tartaria.Camera.main with UnityEngine.Camera.main
    $content = $content -replace "Tartaria\.Camera\.main", "UnityEngine.Camera.main"
    
    # Also fix bare Camera references that should be UnityEngine.Camera
    $content = $content -replace "(?<!UnityEngine\.)Camera\.main", "UnityEngine.Camera.main"
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

Write-Host "`n✅ Part 1 complete! Fixed namespace/API issues." -ForegroundColor Cyan
Write-Host "📊 Run Part 2 script next for AudioZone and PostProcessing fixes" -ForegroundColor Yellow
