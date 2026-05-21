$header = Get-Content 'C:\dev\TARTARIA_new\temp_context_giant_header.md' -Raw
$existing = Get-Content 'C:\dev\TARTARIA_new\CONTEXT.md' -Raw
$combined = $header.TrimEnd() + "`n`n" + $existing
$combined | Set-Content 'C:\dev\TARTARIA_new\CONTEXT.md' -Encoding UTF8
Write-Host "Prepended Moon 2 Giant Mode R9 delivery header to CONTEXT.md"