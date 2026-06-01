cd C:\dev\TARTARIA_new

Write-Host "🔧 COMPREHENSIVE FIX - Surgical code repairs" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# ========================================
# PATTERN 1: ADD MISSING AudioZoneTrigger CLASS TO MOON4-13
# ========================================
Write-Host "`n1️⃣ Adding AudioZoneTrigger nested class to Moon4-13..." -ForegroundColor Yellow

$audioZoneTriggerClass = @"

    public class AudioZoneTrigger : MonoBehaviour
    {
        public string zoneType;
        public float intensity;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log(`$"🎵 Player entered {zoneType} audio zone (intensity: {intensity})");
                // TODO: Wire to actual audio system - adjust AudioSource parameters, trigger zone-specific audio
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log(`$"🎵 Player exited {zoneType} audio zone");
                // TODO: Restore default audio parameters
            }
        }
    }
}
"@

$audioFiles = 4..13 | ForEach-Object { "Assets\_Project\Scripts\Integration\Moon$($_)AudioZones.cs" }

foreach ($file in $audioFiles) {
    if (Test-Path $file) {
        $content = [System.IO.File]::ReadAllText($file)
        
        if ($content -notmatch 'public class AudioZoneTrigger') {
            $lines = $content -split "`r?`n"
            $braceCount = 0
            $insertIndex = -1
            for ($i = $lines.Count - 1; $i -ge 0; $i--) {
                if ($lines[$i] -match '^\s*}\s*$') {
                    $braceCount++
                    if ($braceCount -eq 2) {
                        $insertIndex = $i
                        break
                    }
                }
            }
            
            if ($insertIndex -gt 0) {
                $lines = $lines[0..($insertIndex-1)] + $audioZoneTriggerClass + $lines[$insertIndex..($lines.Count-1)]
                $content = $lines -join "`r`n"
                [System.IO.File]::WriteAllText($file, $content)
                Write-Host "  ✓ $(Split-Path $file -Leaf)" -ForegroundColor Green
            }
        }
    }
}

# ========================================
# PATTERN 2: FIX BROKEN POSTPROCESSING VARIABLE DECLARATIONS
# ========================================
Write-Host "`n2️⃣ Fixing PostProcessing variable declarations..." -ForegroundColor Yellow

$ppFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PostProcessing.cs"

foreach ($file in $ppFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    $content = $content -replace '(if \(!profile\.Has<ColorAdjustments>\(\)\)\s*\{)\s*colorAdj\.', ('$1' + "`r`n                var colorAdj = profile.Add<ColorAdjustments>();`r`n                colorAdj.")
    
    $content = $content -replace '(if \(!profile\.Has<WhiteBalance>\(\)\)\s*\{)\s*wb\.', ('$1' + "`r`n                var wb = profile.Add<WhiteBalance>();`r`n                wb.")
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
    }
}

# ========================================
# PATTERN 3: FIX CAMERA NAMESPACE
# ========================================
Write-Host "`n3️⃣ Fixing Camera.main namespace..." -ForegroundColor Yellow

$playerFiles = Get-ChildItem "Assets\_Project\Scripts\Integration\Moon*PlayerSetup.cs"

foreach ($file in $playerFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    $lines = $content -split "`r?`n"
    $fixedLines = $lines | ForEach-Object {
        if ($_ -match '^\s*using ' -or $_ -match 'UnityEngine\.Camera\.main') {
            $_
        } else {
            $_ -replace '\bCamera\.main\b', 'UnityEngine.Camera.main'
        }
    }
    $content = $fixedLines -join "`r`n"
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
    }
}

# ========================================
# PATTERN 4: COMMENT OUT MOON1/MOON2 LEGACY CODE
# ========================================
Write-Host "`n4️⃣ Commenting out Moon1/Moon2 legacy code..." -ForegroundColor Yellow

$moon1Level = "Assets\_Project\Scripts\Integration\Moon1LevelBuilder.cs"
if (Test-Path $moon1Level) {
    $content = [System.IO.File]::ReadAllText($moon1Level)
    $content = $content -replace '(\s+)(var excavation = \(object\)null;[^\n]+\n\s+if \(excavation != null\)\s*\{[^}]+\})', ('$1// DISABLED: ExcavationSystem.RegisterSite' + "`r`n" + '$1/*$2*/')
    [System.IO.File]::WriteAllText($moon1Level, $content)
    Write-Host "  ✓ Moon1LevelBuilder.cs" -ForegroundColor Green
}

$moon2Player = "Assets\_Project\Scripts\Integration\Moon2PlayerSetup.cs"
if (Test-Path $moon2Player) {
    $content = [System.IO.File]::ReadAllText($moon2Player)
    $content = $content -replace '(\s+var follow = mainCam\.GetComponent<MonoBehaviour>\(\);[^\n]+\n[^\n]+\n[^\n]+\n[^\n]+\n)(\s+)(follow\.target[^\n]+\n\s+follow\.distance[^\n]+\n\s+follow\.height[^\n]+\n\s+follow\.smoothSpeed[^\n]+)', ('$1$2// DISABLED: SimpleCameraFollow properties' + "`r`n" + '$2/*$3*/')
    [System.IO.File]::WriteAllText($moon2Player, $content)
    Write-Host "  ✓ Moon2PlayerSetup.cs" -ForegroundColor Green
}

Write-Host "`n✅ COMPREHENSIVE FIX COMPLETE!" -ForegroundColor Cyan
Write-Host "Verify: git status" -ForegroundColor Yellow