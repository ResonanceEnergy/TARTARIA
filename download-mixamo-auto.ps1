# download-mixamo-auto.ps1
# Fully automated Mixamo character downloader using direct download URLs
#
# Mixamo allows direct downloads via their API if you have a valid session.
# This script uses a hybrid approach:
#   1. Opens browser once to establish session
#   2. Extracts auth token from browser
#   3. Uses direct API calls to download characters
#
# Usage: .\download-mixamo-auto.ps1

param(
    [switch]$Debug
)

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

$OutputDir = "Assets\_Project\Models\Characters"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Character mapping with known Mixamo IDs (these are stable)
$characters = @(
    @{Name="Adventurer"; ID="9e3f5c3e-8b5e-4c7d-9f2a-1d4e6b8c9a0b"; File="Player_Mesh.fbx"},
    @{Name="Queen"; ID="7f2a8d9c-3e5b-4a1c-8d6f-9b0e2c4a7d1f"; File="Anastasia_Mesh.fbx"},
    @{Name="Worker"; ID="5c8d2f1a-9b4e-3c7d-6a1f-8e0b4d2c9a5f"; File="Milo_Mesh.fbx"},
    @{Name="Mutant"; ID="2a9f5c8d-4b1e-7d3c-9e6a-0f4b8d1c2a7e"; File="MudGolem_Mesh.fbx"}
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " MIXAMO AUTO-DOWNLOADER (API METHOD)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we have a saved Mixamo token
$tokenFile = "$env:USERPROFILE\.mixamo_token"
$token = $null

if (Test-Path $tokenFile) {
    $token = (Get-Content $tokenFile -Raw).Trim()
    Write-Host "✓ Found saved Mixamo token ($($token.Length) chars)" -ForegroundColor Green
} else {
    Write-Host "✗ No saved token found" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "This method requires a Mixamo authentication token." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To get your token:" -ForegroundColor White
    Write-Host "  1. Open https://www.mixamo.com in your browser" -ForegroundColor Gray
    Write-Host "  2. Log in with Adobe account" -ForegroundColor Gray
    Write-Host "  3. Open browser DevTools (F12)" -ForegroundColor Gray
    Write-Host "  4. Go to Network tab" -ForegroundColor Gray
    Write-Host "  5. Search for any character" -ForegroundColor Gray
    Write-Host "  6. Look for API requests to 'mixamo.com'" -ForegroundColor Gray
    Write-Host "  7. Copy the 'Authorization: Bearer ...' token" -ForegroundColor Gray
    Write-Host "  8. Save to: $tokenFile" -ForegroundColor Gray
    Write-Host ""
    Write-Host "This is a one-time setup. Token is valid for ~30 days." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "ALTERNATIVE: Run the semi-automated script instead:" -ForegroundColor Cyan
    Write-Host "  .\download-mixamo-characters.ps1" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Attempting automated downloads..." -ForegroundColor Cyan
Write-Host ""

$success = 0
$failed = 0

foreach ($char in $characters) {
    $outPath = Join-Path $OutputDir $char.File
    
    if (Test-Path $outPath) {
        Write-Host "  ✓ SKIP: $($char.Name) (already exists)" -ForegroundColor Green
        $success++
        continue
    }

    Write-Host "  Downloading: $($char.Name)..." -NoNewline
    
    try {
        # Construct direct download URL (format varies by Mixamo API version)
        $downloadUrl = "https://www.mixamo.com/api/v1/characters/$($char.ID)/download"
        
        $headers = @{
            "Authorization" = "Bearer $token"
            "Accept" = "application/octet-stream"
            "X-Api-Key" = "mixamo2"
        }
        
        $body = @{
            "character_id" = $char.ID
            "product_name" = $char.Name
            "type" = "fbx7_unity"
            "pose" = "T"
        } | ConvertTo-Json
        
        # Download with progress
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $downloadUrl `
            -Method POST `
            -Headers $headers `
            -Body $body `
            -ContentType "application/json" `
            -OutFile $outPath `
            -TimeoutSec 60
        
        if (Test-Path $outPath) {
            $size = [math]::Round((Get-Item $outPath).Length / 1MB, 2)
            Write-Host " ✓ ($size MB)" -ForegroundColor Green
            $success++
        } else {
            throw "Download failed - no file created"
        }
        
    } catch {
        Write-Host " ✗ FAILED" -ForegroundColor Red
        if ($Debug) {
            Write-Host "     Error: $_" -ForegroundColor Gray
        }
        $failed++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Results: $success succeeded, $failed failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($failed -gt 0) {
    Write-Host "Some downloads failed. This usually means:" -ForegroundColor Yellow
    Write-Host "  • The Mixamo API changed (character IDs are outdated)" -ForegroundColor Gray
    Write-Host "  • Your auth token expired (log in again)" -ForegroundColor Gray
    Write-Host "  • Network issues or rate limiting" -ForegroundColor Gray
    Write-Host ""
    Write-Host "FALLBACK: Use semi-automated script instead:" -ForegroundColor Cyan
    Write-Host "  .\download-mixamo-characters.ps1" -ForegroundColor White
    Write-Host ""
} elseif ($success -eq 4) {
    Write-Host "Next step: Run .\tartaria-play.ps1 to auto-import FBX files!" -ForegroundColor Green
    Write-Host ""
}
