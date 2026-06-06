# Fix top compilation errors by frequency
Write-Host "Fixing top compilation errors..." -ForegroundColor Cyan

# Fix 1: Tartaria.Input.GetKeyDown → UnityEngine.Input.GetKeyDown (15x errors)
$files = Get-ChildItem "Assets\_Project\Scripts\Integration" -Filter "*.cs" -Recurse
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    $content = $content -replace 'Tartaria\.Input\.GetKeyDown', 'UnityEngine.Input.GetKeyDown'
    $content = $content -replace 'Tartaria\.Input\.GetKey\b', 'UnityEngine.Input.GetKey'
    if ($content -ne $original) {
        Set-Content $file.FullName $content -NoNewline
        Write-Host "Fixed Input namespace in: $($file.Name)" -ForegroundColor Green
    }
}

# Fix 2: Tartaria.Camera.main → UnityEngine.Camera.main (9x errors)
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    $content = $content -replace 'Tartaria\.Camera\.main', 'UnityEngine.Camera.main'
    if ($content -ne $original) {
        Set-Content $file.FullName $content -NoNewline
        Write-Host "Fixed Camera namespace in: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nFixed namespace issues. Now add missing methods to Phase2Stubs..." -ForegroundColor Yellow
