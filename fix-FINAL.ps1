cd C:\dev\TARTARIA_new
Write-Host "COMPREHENSIVE FIX" -ForegroundColor Cyan
$audioFiles = 4..13 | ForEach-Object { "Assets\_Project\Scripts\Integration\Moon$($_)AudioZones.cs" }
foreach ($file in $audioFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        if ($content -notmatch "public class AudioZoneTrigger") {
            $lines = $content -split "`r?`n"
            $bc = 0; $idx = -1
            for ($i = $lines.Count - 1; $i -ge 0; $i--) {
                if ($lines[$i] -match "^\s*}\s*$") { $bc++; if ($bc -eq 2) { $idx = $i; break } }
            }
            if ($idx -gt 0) {
                $class = "`r`n    public class AudioZoneTrigger : MonoBehaviour`r`n    {`r`n        public string zoneType;`r`n        public float intensity;`r`n        void OnTriggerEnter(Collider other) { if (other.CompareTag(`"Player`")) Debug.Log(`$`"Player entered {zoneType}`"); }`r`n        void OnTriggerExit(Collider other) { if (other.CompareTag(`"Player`")) Debug.Log(`$`"Player exited {zoneType}`"); }`r`n    }`r`n}`r`n"
                $lines = $lines[0..($idx-1)] + $class + $lines[$idx..($lines.Count-1)]
                [System.IO.File]::WriteAllText($file, ($lines -join "`r`n"))
                Write-Host "  OK $(Split-Path $file -Leaf)" -ForegroundColor Green
            }
        }
    }
}
$ppFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PostProcessing.cs"
foreach ($file in $ppFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $orig = $content
    $content = $content -replace "(if \(!profile\.Has<ColorAdjustments>\(\)\)\s*\{)\s*colorAdj\.", ("`$1`r`n                var colorAdj = profile.Add<ColorAdjustments>();`r`n                colorAdj.")
    $content = $content -replace "(if \(!profile\.Has<WhiteBalance>\(\)\)\s*\{)\s*wb\.", ("`$1`r`n                var wb = profile.Add<WhiteBalance>();`r`n                wb.")
    if ($content -ne $orig) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Host "  OK $($file.Name)" -ForegroundColor Green
    }
}
$playerFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PlayerSetup.cs"
foreach ($file in $playerFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $orig = $content
    $lines = $content -split "`r?`n"
    $fixed = $lines | ForEach-Object { if ($_ -match "^\s*using " -or $_ -match "UnityEngine\.Camera\.main") { $_ } else { $_ -replace "\bCamera\.main\b", "UnityEngine.Camera.main" } }
    $content = $fixed -join "`r`n"
    if ($content -ne $orig) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Host "  OK $($file.Name)" -ForegroundColor Green
    }
}
Write-Host "DONE!" -ForegroundColor Cyan
