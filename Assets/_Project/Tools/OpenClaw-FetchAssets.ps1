# OpenClaw-FetchAssets.ps1
# Master asset acquisition orchestrator for Tartaria.
# Runs all automatable downloads, then queues a Unity import refresh.
#
# Usage:
#   pwsh .\Assets\_Project\Tools\OpenClaw-FetchAssets.ps1
#   pwsh .\Assets\_Project\Tools\OpenClaw-FetchAssets.ps1 -MixamoToken "eyJxxx..."
#
# If MIXAMO_TOKEN env var is set, it is used automatically.
#
param(
    [string]$MixamoToken = $env:MIXAMO_TOKEN,
    [string]$HDRIResolution = "2k",
    [switch]$SkipHDRI,
    [switch]$SkipMixamo,
    [switch]$SkipImport
)
$ErrorActionPreference = "Continue"
cd C:\dev\TARTARIA_new

# Auto-load persisted Mixamo token if present and not provided.
$persistedToken = Join-Path $env:USERPROFILE ".mixamo_token"
if (-not $MixamoToken -and (Test-Path $persistedToken)) {
    $MixamoToken = (Get-Content $persistedToken -Raw).Trim()
    Write-Host "[OpenClaw] Loaded persisted Mixamo token from $persistedToken" -ForegroundColor DarkGray
}

$banner = @"
=============================================================
  OPENCLAW :: TARTARIA ASSET FETCH
  HDRI       : $(if ($SkipHDRI)   { 'SKIP' } else { "Poly Haven @ $HDRIResolution" })
  Mixamo     : $(if ($SkipMixamo) { 'SKIP' } elseif ($MixamoToken) { 'enabled (token present)' } else { 'SKIP (no token)' })
  Unity sync : $(if ($SkipImport) { 'SKIP' } else { 'queued via .meta refresh' })
=============================================================
"@
Write-Host $banner -ForegroundColor Cyan

# ── 1. Poly Haven HDRI (CC0, no auth) ──
if (-not $SkipHDRI) {
    Write-Host ""
    Write-Host "[1/3] Poly Haven HDRI ─────────────────────────────────" -ForegroundColor Yellow
    & "$PSScriptRoot\Fetch-PolyhavenHDRI.ps1" -Resolution $HDRIResolution
}

# ── 2. Mixamo (requires user-supplied bearer token) ──
if (-not $SkipMixamo) {
    Write-Host ""
    Write-Host "[2/3] Mixamo Animations ───────────────────────────────" -ForegroundColor Yellow
    if (-not $MixamoToken) {
        Write-Host "  SKIP — no Mixamo bearer token provided." -ForegroundColor DarkYellow
        Write-Host "  To enable:" -ForegroundColor DarkGray
        Write-Host "    1. Log in to https://www.mixamo.com" -ForegroundColor DarkGray
        Write-Host "    2. F12 → Network → click any Download → copy 'Authorization: Bearer xxx'" -ForegroundColor DarkGray
        Write-Host "    3. `$env:MIXAMO_TOKEN='eyJxxx...' ; pwsh .\OpenClaw-FetchAssets.ps1" -ForegroundColor DarkGray
    } else {
        & "$PSScriptRoot\Fetch-MixamoAnimations.ps1" -BearerToken $MixamoToken
    }
}

# ── 3. Trigger Unity asset import (touch any .meta to force refresh on next focus) ──
if (-not $SkipImport) {
    Write-Host ""
    Write-Host "[3/3] Unity Import Refresh ────────────────────────────" -ForegroundColor Yellow
    # Drop a sentinel that AssetPostprocessor / ProjectSetupWizard can pick up.
    $sentinel = "Assets\_Project\Tools\.openclaw_refresh"
    Set-Content -Path $sentinel -Value (Get-Date -Format o) -Encoding UTF8
    Write-Host "  Sentinel written: $sentinel" -ForegroundColor Green
    Write-Host "  Run .\tartaria-play.ps1 to let Unity import + bake new assets." -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "OpenClaw fetch complete." -ForegroundColor Green
