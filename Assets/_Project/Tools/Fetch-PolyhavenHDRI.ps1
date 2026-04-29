# Fetch-PolyhavenHDRI.ps1
# Downloads CC0 HDRI environments from Poly Haven public REST API.
# No authentication required — Poly Haven license is CC0.
#
# Usage:
#   pwsh .\Assets\_Project\Tools\Fetch-PolyhavenHDRI.ps1
#   pwsh .\Assets\_Project\Tools\Fetch-PolyhavenHDRI.ps1 -Resolution 4k
#
param(
    [string]$Resolution = "2k",
    [string]$OutDir = "Assets\_Project\Textures\HDRI",
    [string[]]$Slugs = @(
        "kloofendal_43d_clear_puresky",   # bright dawn  (Moon 1 day)
        "kloppenheim_06_puresky",         # warm dusk    (Moon 1 dusk)
        "moonless_golf",                  # night        (Moon 1 night)
        "spruit_sunrise",                 # ethereal     (Aether shrine)
        "industrial_sunset_puresky"       # epic         (boss/climax)
    )
)
$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }

foreach ($slug in $Slugs) {
    $outFile = Join-Path $OutDir "$slug.hdr"
    if (Test-Path $outFile) { Write-Host "  SKIP $slug (exists)" -ForegroundColor DarkGray; continue }

    Write-Host "  GET  $slug @ $Resolution" -ForegroundColor Cyan
    $apiUrl = "https://api.polyhaven.com/files/$slug"
    try {
        $files = Invoke-RestMethod -Uri $apiUrl -TimeoutSec 30
        $hdrUrl = $files.hdri.$Resolution.hdr.url
        if (-not $hdrUrl) {
            Write-Host "    FAIL no $Resolution hdr for $slug" -ForegroundColor Red
            continue
        }
        Invoke-WebRequest -Uri $hdrUrl -OutFile $outFile -TimeoutSec 300
        $size = [math]::Round((Get-Item $outFile).Length / 1MB, 2)
        Write-Host "    OK   $slug.hdr (${size} MB)" -ForegroundColor Green
    } catch {
        Write-Host "    FAIL $slug : $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "HDRI fetch complete. Files in: $OutDir" -ForegroundColor Green
