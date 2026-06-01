# Agent 1 - Remove all remaining 'using Tartaria.UI;' from Integration files

cd c:\dev\TARTARIA_new

$integrationPath = "Assets\_Project\Scripts\Integration"
$files = Get-ChildItem -Path $integrationPath -Filter "*.cs" -Recurse

$cleaned = 0

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    if ($content -match 'using Tartaria\.UI;') {
        $newContent = $content -replace 'using Tartaria\.UI;\r?\n', ''
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        $cleaned++
        Write-Host "✓ Removed UI using from $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Cleanup Complete - All UI usings removed" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Files cleaned: $cleaned" -ForegroundColor Yellow
