$file = "Assets\_Project\Scripts\Core\GameEvents.cs"
$content = Get-Content $file -Raw

# Replace the double-brace ending with two separate lines
$content = $content -replace '\}\}$', "}`n}"

# Write back
[System.IO.File]::WriteAllText((Resolve-Path $file), $content, [System.Text.Encoding]::UTF8)

Write-Host "✅ Fixed double closing brace" -ForegroundColor Green

# Verify
$lines = Get-Content $file
Write-Host "Last 5 lines:" -ForegroundColor Cyan
$lines[-5..-1] | ForEach-Object { Write-Host "  $_" }
Write-Host ""
Write-Host "Total lines: $($lines.Count)" -ForegroundColor Cyan
