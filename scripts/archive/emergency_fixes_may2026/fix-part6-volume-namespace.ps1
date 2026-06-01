cd C:\dev\TARTARIA_new

Write-Host "🔧 PART 6: Fix VolumeProfile API + namespace qualifiers..." -ForegroundColor Cyan

# ========================================
# 1️⃣ FIX VOLUMEPROFILE.HAS() - REMOVE OUT PARAMETER
# ========================================
Write-Host "`n1️⃣ Fixing VolumeProfile.Has() - correct API signature..." -ForegroundColor Yellow

$ppFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PostProcessing.cs"
$fixedPP = 0

foreach ($file in $ppFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    # Pattern 1: if (!profile.Has<T>(out var x)) → if (!profile.Has<T>())
    $content = $content -replace 'if \(!profile\.Has<(\w+)>\(out var \w+\)\)', 'if (!profile.Has<$1>())'

    # Pattern 2: The code inside tries to use the variable, need to add it back
    # Replace the entire block structure
    $content = $content -replace 'if \(!profile\.Has<(Bloom)>\(\)\)\s*{\s*bloom\.', @'
if (!profile.Has<Bloom>())
            {
                var bloom = profile.Add<Bloom>();
                bloom.
'@

    $content = $content -replace 'if \(!profile\.Has<(ChromaticAberration)>\(\)\)\s*{\s*ca\.', @'
if (!profile.Has<ChromaticAberration>())
            {
                var ca = profile.Add<ChromaticAberration>();
                ca.
'@

    $content = $content -replace 'if \(!profile\.Has<(Vignette)>\(\)\)\s*{\s*vignette\.', @'
if (!profile.Has<Vignette>())
            {
                var vignette = profile.Add<Vignette>();
                vignette.
'@

    $content = $content -replace 'if \(!profile\.Has<(ColorAdjustments)>\(\)\)\s*{\s*colorGrading\.', @'
if (!profile.Has<ColorAdjustments>())
            {
                var colorGrading = profile.Add<ColorAdjustments>();
                colorGrading.
'@

    $content = $content -replace 'if \(!profile\.Has<(FilmGrain)>\(\)\)\s*{\s*filmGrain\.', @'
if (!profile.Has<FilmGrain>())
            {
                var filmGrain = profile.Add<FilmGrain>();
                filmGrain.
'@

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $fixedPP++
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "  Fixed $fixedPP PostProcessing files" -ForegroundColor Green

# ========================================
# 2️⃣ FIX NAMESPACE QUALIFIERS (Core. → Integration.)
# ========================================
Write-Host "`n2️⃣ Fixing namespace qualifiers (Core → Integration)..." -ForegroundColor Yellow

$nsFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*InteractiveObjects.cs",
                          "Assets\_Project\Scripts\Integration\Moon*PowerUps.cs",
                          "Assets\_Project\Scripts\Integration\Moon*Secrets.cs",
                          "Assets\_Project\Scripts\Integration\Moon*Collectibles.cs"
$fixedNS = 0

foreach ($file in $nsFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    # Fix: Core.GameLoopController → Integration.GameLoopController
    $content = $content -replace 'Core\.GameLoopController', 'Integration.GameLoopController'

    # Fix: Core.Enums.QuestObjectiveType → Core.QuestObjectiveType
    $content = $content -replace 'Core\.Enums\.QuestObjectiveType', 'Core.QuestObjectiveType'

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $fixedNS++
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "  Fixed $fixedNS namespace qualifier files" -ForegroundColor Green

# ========================================
# 3️⃣ FIX CAMERA NAMESPACE
# ========================================
Write-Host "`n3️⃣ Fixing Camera namespace..." -ForegroundColor Yellow

$camFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PlayerSetup.cs"
$fixedCam = 0

foreach ($file in $camFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content

    # Fix instances where Camera is used without UnityEngine qualifier
    # But only in lines with Camera.main, not the using statement
    $lines = $content -split "`r?`n"
    $fixedLines = $lines | ForEach-Object {
        if ($_ -match '^\s*using ') {
            $_ # Don't modify using statements
        } else {
            $_ -replace '(?<!UnityEngine\.)Camera\.main', 'UnityEngine.Camera.main'
        }
    }
    $content = $fixedLines -join "`r`n"

    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $fixedCam++
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "  Fixed $fixedCam Camera namespace files" -ForegroundColor Green

# ========================================
# SUMMARY
# ========================================
Write-Host "`n✅ Part 6 complete!" -ForegroundColor Cyan
Write-Host "  - VolumeProfile.Has() API: $fixedPP files" -ForegroundColor White
Write-Host "  - Namespace qualifiers: $fixedNS files" -ForegroundColor White
Write-Host "  - Camera namespace: $fixedCam files" -ForegroundColor White
Write-Host "`nRun 'git diff' to review changes." -ForegroundColor Cyan
