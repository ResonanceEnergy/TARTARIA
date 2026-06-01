# Create Audio Folder Structure
# Quick setup for TARTARIA audio organization

Write-Host ""
Write-Host "Creating audio folder structure..." -ForegroundColor Cyan
Write-Host ""

$folders = @(
    "Assets\_Project\Audio\Music",
    "Assets\_Project\Audio\SFX\Player\Movement",
    "Assets\_Project\Audio\SFX\Player\Combat",
    "Assets\_Project\Audio\SFX\Enemies",
    "Assets\_Project\Audio\SFX\Environment"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        Write-Host "✅ Created: $folder" -ForegroundColor Green
    } else {
        Write-Host "✅ Exists:  $folder" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Audio folder structure ready!" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "  1. Open: code FREE_ASSET_LINKS.md" -ForegroundColor White
Write-Host "  2. Start downloading music from Pixabay" -ForegroundColor White
Write-Host "  3. Save files to: Assets\_Project\Audio\Music\" -ForegroundColor White
Write-Host ""
Write-Host "MUSIC QUICK START:" -ForegroundColor Yellow
Write-Host "  https://pixabay.com/music/search/ambient%20432hz/" -ForegroundColor Cyan
Write-Host ""
Write-Host "SFX QUICK START (requires free account):" -ForegroundColor Yellow
Write-Host "  https://freesound.org/home/register/" -ForegroundColor Cyan
Write-Host ""
