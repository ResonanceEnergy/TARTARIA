param(
    [string]$TargetNamespace,
    [string[]]$Files
)

$updated = 0
$skipped = 0

foreach ($file in $Files) {
    $fullPath = "C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\$file"

    if (-not (Test-Path $fullPath)) {
        Write-Host "SKIP: $file (not found)" -ForegroundColor Gray
        $skipped++
        continue
    }

    $content = Get-Content $fullPath -Raw

    # Check if using directive already exists
    if ($content -match "using $TargetNamespace;") {
        Write-Host "SKIP: $file (already has $TargetNamespace)" -ForegroundColor Gray
        $skipped++
        continue
    }

    # Find the last using directive and add after it
    if ($content -match '(using [^;]+;)\s*(namespace )') {
        $newContent = $content -replace '(using [^;]+;)\s*(namespace )', "`$1`r`nusing $TargetNamespace;`r`n`r`n`$2"
        [System.IO.File]::WriteAllText($fullPath, $newContent, [System.Text.UTF8Encoding]::new($false))
        Write-Host "ADD: $file" -ForegroundColor Green
        $updated++
    }
    else {
        Write-Host "WARN: $file (couldn't find insertion point)" -ForegroundColor Yellow
        $skipped++
    }
}

Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Updated: $updated files" -ForegroundColor Green
Write-Host "Skipped: $skipped files" -ForegroundColor Gray
