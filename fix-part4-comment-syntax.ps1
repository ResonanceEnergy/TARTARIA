cd C:\dev\TARTARIA_new

Write-Host "🔧 Fixing malformed comment syntax from previous fixes..." -ForegroundColor Cyan

# ========================================
# 1️⃣ FIX POSTPROCESSING FILES - Remove doubled 'if'
# ========================================
Write-Host "`n1️⃣ Fixing PostProcessing files (doubled 'if')..." -ForegroundColor Yellow

$postProcessFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PostProcessing.cs"
$fixedPost = 0

foreach ($file in $postProcessFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $originalContent = $content

    # Fix: if (if (!profile.Has<  →  if (!profile.Has<
    $content = $content -replace 'if \(if \(!profile\.Has<', 'if (!profile.Has<'

    if ($content -ne $originalContent) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $fixedPost++
        Write-Host "  ✓ Fixed $($file.Name)" -ForegroundColor Green
    }
}
Write-Host "  ✅ Fixed $fixedPost PostProcessing files" -ForegroundColor Green

# ========================================
# 2️⃣ FIX MOON1POSTPROCESSING - Comment out entire TryGet blocks
# ========================================
Write-Host "`n2️⃣ Fixing Moon1PostProcessing.cs (TryGet blocks)..." -ForegroundColor Yellow

$moon1Post = "Assets\_Project\Scripts\Integration\Moon1PostProcessing.cs"
$content = [System.IO.File]::ReadAllText($moon1Post)

# Replace: postProcessVolume.// DISABLED: profile.TryGet<Bloom>()
# With: null // DISABLED: postProcessVolume.profile.TryGet<Bloom>()
$content = $content -replace 'var bloom = postProcessVolume\.// DISABLED: profile\.TryGet<Bloom>\(\);', 'var bloom = null; // DISABLED: postProcessVolume.profile.TryGet<Bloom>()'
$content = $content -replace 'var vignette = postProcessVolume\.// DISABLED: profile\.TryGet<Vignette>\(\);', 'var vignette = null; // DISABLED: postProcessVolume.profile.TryGet<Vignette>()'
$content = $content -replace 'var colorGrading = postProcessVolume\.// DISABLED: profile\.TryGet<ColorAdjustments>\(\);', 'var colorGrading = null; // DISABLED: postProcessVolume.profile.TryGet<ColorAdjustments>()'

[System.IO.File]::WriteAllText($moon1Post, $content)
Write-Host "  ✓ Fixed Moon1PostProcessing.cs" -ForegroundColor Green

# ========================================
# 3️⃣ FIX MOON2PLAYERSETUP - Replace generic type with object
# ========================================
Write-Host "`n3️⃣ Fixing Moon2PlayerSetup.cs (generic type)..." -ForegroundColor Yellow

$moon2Player = "Assets\_Project\Scripts\Integration\Moon2PlayerSetup.cs"
$content = [System.IO.File]::ReadAllText($moon2Player)

# Replace: GetComponent<// DISABLED: Moon1PlayerSetup.SimpleCameraFollow>()
# With: GetComponent<MonoBehaviour>() // DISABLED: was SimpleCameraFollow
$content = $content -replace 'GetComponent<// DISABLED: Moon1PlayerSetup\.SimpleCameraFollow>\(\)', 'GetComponent<MonoBehaviour>() // DISABLED: was SimpleCameraFollow'
$content = $content -replace 'AddComponent<// DISABLED: Moon1PlayerSetup\.SimpleCameraFollow>\(\)', 'AddComponent<MonoBehaviour>() // DISABLED: was SimpleCameraFollow'

[System.IO.File]::WriteAllText($moon2Player, $content)
Write-Host "  ✓ Fixed Moon2PlayerSetup.cs" -ForegroundColor Green

# ========================================
# 4️⃣ FIX PICKUPINTERACTABLE - Assign default values
# ========================================
Write-Host "`n4️⃣ Fixing PickupInteractable.cs (assignments)..." -ForegroundColor Yellow

$pickup = "Assets\_Project\Scripts\Integration\PickupInteractable.cs"
$content = [System.IO.File]::ReadAllText($pickup)

# Replace: bool added = // DISABLED: InventorySystem.Instance.Add(...)
# With: bool added = false; // DISABLED: InventorySystem.Instance.Add(...)
$content = $content -replace 'bool added = // DISABLED: InventorySystem\.Instance\.Add\(itemId, itemCount\);', 'bool added = false; // DISABLED: InventorySystem.Instance.Add(itemId, itemCount)'

# Replace: Count < // DISABLED: InventorySystem.MaxSlots;
# With: Count < 100; // DISABLED: InventorySystem.MaxSlots
$content = $content -replace 'Count < // DISABLED: InventorySystem\.MaxSlots;', 'Count < 100; // DISABLED: InventorySystem.MaxSlots (hardcoded)'

[System.IO.File]::WriteAllText($pickup, $content)
Write-Host "  ✓ Fixed PickupInteractable.cs" -ForegroundColor Green

# ========================================
# 5️⃣ FIX MOON1LEVELBUILDER - Assign null
# ========================================
Write-Host "`n5️⃣ Fixing Moon1LevelBuilder.cs (assignment)..." -ForegroundColor Yellow

$moon1Level = "Assets\_Project\Scripts\Integration\Moon1LevelBuilder.cs"
$content = [System.IO.File]::ReadAllText($moon1Level)

# Replace: var excavation = // DISABLED: ExcavationSystem.Instance;
# With: var excavation = (object)null; // DISABLED: ExcavationSystem.Instance
$content = $content -replace 'var excavation = // DISABLED: ExcavationSystem\.Instance;', 'var excavation = (object)null; // DISABLED: ExcavationSystem.Instance'

[System.IO.File]::WriteAllText($moon1Level, $content)
Write-Host "  ✓ Fixed Moon1LevelBuilder.cs" -ForegroundColor Green

# ========================================
# SUMMARY
# ========================================
Write-Host "`n✅ Part 4 complete! Fixed:" -ForegroundColor Cyan
Write-Host "  - $fixedPost PostProcessing files (doubled 'if')" -ForegroundColor White
Write-Host "  - Moon1PostProcessing.cs (TryGet blocks)" -ForegroundColor White
Write-Host "  - Moon2PlayerSetup.cs (generic types)" -ForegroundColor White
Write-Host "  - PickupInteractable.cs (assignments)" -ForegroundColor White
Write-Host "  - Moon1LevelBuilder.cs (assignment)" -ForegroundColor White
Write-Host "`nRun 'git status' to verify changes." -ForegroundColor Cyan
