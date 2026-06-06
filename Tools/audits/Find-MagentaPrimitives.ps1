// File: tools/audits/Find-MagentaPrimitives.ps1
#Requires -Version 7.0

param([string]$Root = (Get-Location).Path)

$scriptsDir = Join-Path $Root "Assets/_Project/Scripts"
if (-not (Test-Path $scriptsDir)) {
    Write-Host "Scripts dir not found: $scriptsDir" -ForegroundColor Red
    exit 2
}

$offenders = @()

Get-ChildItem -Path $scriptsDir -Filter "*.cs" -Recurse |
    Where-Object {
        $_.FullName -notmatch '_archived_backups' -and
        $_.FullName -notmatch '\\Tests\\' -and
        $_.FullName -notmatch '\.disabled$'
    } |
    ForEach-Object {
        $file = $_.FullName
        $lines = Get-Content $file
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ($line -match 'GameObject\.CreatePrimitive\(') {
                # check next 40 lines
                $endIdx = [Math]::Min($i + 40, $lines.Count - 1)
                $window = $lines[$i..$endIdx] -join "`n"
                $hasUrp = ($window -match 'Shader\.Find\("Universal Render Pipeline/Lit"') -and ($window -match 'SetColor\("_BaseColor"')
                $hasEscape = $window -match '// URP-safe'
                if (-not $hasUrp -and -not $hasEscape) {
                    $offenders += [pscustomobject]@{ File=$file.Replace($Root+'\',''); Line=($i+1); Reason="primitive without URP shader assignment" }
                }
            }
            if ($line -match '\.color\s*=' -and $line -match '\b(mat|material|rend\.material|sharedMaterial)\b') {
                $offenders += [pscustomobject]@{ File=$file.Replace($Root+'\',''); Line=($i+1); Reason="material.color assignment (use SetColor _BaseColor)" }
            }
        }
    }

if ($offenders.Count -eq 0) {
    Write-Host "CLEAN - no magenta-risk primitive creations found" -ForegroundColor Green
    exit 0
}

$offenders | Format-Table -AutoSize
Write-Host ""
Write-Host "FOUND $($offenders.Count) magenta-risk sites" -ForegroundColor Red
exit 1
