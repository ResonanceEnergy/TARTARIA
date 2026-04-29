# Capture-MixamoToken.ps1
# PowerShell wrapper around the Playwright capture script.
# After capture, sets $env:MIXAMO_TOKEN for the current shell and triggers OpenClaw.
#
# Usage:
#   pwsh .\Assets\_Project\Tools\Capture-MixamoToken.ps1
#   pwsh .\Assets\_Project\Tools\Capture-MixamoToken.ps1 -RunFetch   # auto-run Mixamo fetch after capture
#
param(
    [switch]$RunFetch,
    [string]$Python = "python"
)
$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

$tokenFile = Join-Path $env:USERPROFILE ".mixamo_token"
$captureScript = Join-Path $PSScriptRoot "Capture-MixamoToken.py"

Write-Host ""
Write-Host "=== Mixamo Token Capture (Playwright) ===" -ForegroundColor Cyan
Write-Host "  Browser will open. Log in once (Adobe SSO) — profile is remembered." -ForegroundColor DarkGray
Write-Host ""

# Run capture script; surface MIXAMO_TOKEN= line.
& $Python $captureScript

if (-not (Test-Path $tokenFile)) {
    Write-Host "ERROR: Token capture failed (no $tokenFile)" -ForegroundColor Red
    exit 1
}

$token = (Get-Content $tokenFile -Raw).Trim()
$env:MIXAMO_TOKEN = $token
Write-Host ""
Write-Host "Token saved to env: MIXAMO_TOKEN ($($token.Length) chars)" -ForegroundColor Green
Write-Host "  Persisted to: $tokenFile" -ForegroundColor DarkGray
Write-Host "  To reuse in another shell: `$env:MIXAMO_TOKEN = (Get-Content $tokenFile -Raw).Trim()" -ForegroundColor DarkGray

if ($RunFetch) {
    Write-Host ""
    Write-Host "Running OpenClaw Mixamo fetch ..." -ForegroundColor Yellow
    & "$PSScriptRoot\OpenClaw-FetchAssets.ps1" -SkipHDRI
}
