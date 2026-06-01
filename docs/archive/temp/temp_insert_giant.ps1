$marker = "Master purification: ringing 3 connected towers simultaneously creates a permanent ward"
$section = Get-Content 'C:\dev\TARTARIA_new\temp_moon2_giant_section.md' -Raw
$lines = Get-Content 'C:\dev\TARTARIA_new\docs\03C_MOON_MECHANICS_DETAILED.md'
$newLines = New-Object System.Collections.ArrayList
$inserted = $false
foreach ($line in $lines) {
    [void]$newLines.Add($line)
    if ($line -like "*$marker*" -and -not $inserted) {
        [void]$newLines.Add("")
        [void]$newLines.Add($section.TrimEnd())
        [void]$newLines.Add("")
        $inserted = $true
    }
}
$newLines | Set-Content 'C:\dev\TARTARIA_new\docs\03C_MOON_MECHANICS_DETAILED.md' -Encoding UTF8
Write-Host "Moon 2 Giant Mode section successfully inserted into 03C_MOON_MECHANICS_DETAILED.md"