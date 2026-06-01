# GameEvents.cs Fix Script
$source = "Assets\_Project\Scripts\Core\GameEvents.cs"

# Read all content
$content = [System.IO.File]::ReadAllText($source)

# The problem: There are two sections that need to be removed:
# 1. Lines starting with incomplete documentation through the stray });
# 2. Duplicate events that are outside the class

# Strategy: Find the first "public static class GameEvents" and keep everything from there
$classStart = $content.IndexOf("public static class GameEvents")

if ($classStart -gt 0) {
    # Keep "using" statements + "namespace Tartaria.Core {" + the class definition onwards
    $fixed = $content.Substring(0, 119) + "`n" + $content.Substring($classStart)
    
    [System.IO.File]::WriteAllText($source, $fixed, [System.Text.Encoding]::UTF8)
    Write-Host "FIXED! Class definition moved inside namespace."
} else {
    Write-Host "ERROR: Could not find class declaration"
}
