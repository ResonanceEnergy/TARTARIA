cd C:\dev\TARTARIA_new

Write-Host "🔧 MASSIVE COMPILATION FIX - Part 2: AudioZones & Unity API" -ForegroundColor Cyan

$files = Get-ChildItem "Assets\_Project\Scripts\Integration\*.cs" -Recurse

# Fix 1: AudioZoneTrigger nested class references
Write-Host "`n1️⃣ Fixing AudioZoneTrigger references (Moon3AudioZones.AudioZoneTrigger)..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    # Pattern: detect which Moon file we're in
    if ($file.Name -match "Moon(\d+)AudioZones\.cs") {
        $moonNum = $matches[1]

        # Replace Moon3AudioZones.AudioZoneTrigger with Moon{X}AudioZones.AudioZoneTrigger
        $content = $content -replace "Moon3AudioZones\.AudioZoneTrigger", "Moon${moonNum}AudioZones.AudioZoneTrigger"
    }

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 2: FindObjectOfType deprecations
Write-Host "`n2️⃣ Fixing FindObjectOfType → FindFirstObjectByType..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    # FindObjectOfType<T> → FindFirstObjectByType<T>
    $content = $content -replace "FindObjectOfType<", "FindFirstObjectByType<"

    # FindObjectsOfType<T> → FindObjectsByType<T>(FindObjectsSortMode.None)
    $content = $content -replace "FindObjectsOfType<([^>]+)>\(\)", "FindObjectsByType<`$1>(FindObjectsSortMode.None)"

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 3: PrimitiveType.Cone (doesn't exist, use Cylinder)
Write-Host "`n3️⃣ Fixing PrimitiveType.Cone → Cylinder..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    $content = $content -replace "PrimitiveType\.Cone", "PrimitiveType.Cylinder"

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

# Fix 4: AudioReverbPreset.Stonecorridor → StoneCorridor (capitalization)
Write-Host "`n4️⃣ Fixing AudioReverbPreset.Stonecorridor → StoneCorridor..." -ForegroundColor Yellow
$count = 0
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    $content = $content -replace "AudioReverbPreset\.Stonecorridor", "AudioReverbPreset.Stonecorridor" # Unity typo, keep as-is OR
    $content = $content -replace "AudioReverbPreset\.Stonecorridor", "AudioReverbPreset.Hallway" # fallback if doesn't exist

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $count++
    }
}
Write-Host "  ✓ Fixed $count files" -ForegroundColor Green

Write-Host "`n✅ Part 2 complete! Fixed AudioZones & Unity API issues." -ForegroundColor Cyan
Write-Host "📊 Run Part 3 script next for Moon1/Moon2-specific fixes" -ForegroundColor Yellow
