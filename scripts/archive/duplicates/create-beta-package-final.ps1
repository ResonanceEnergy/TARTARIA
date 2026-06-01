# TARTARIA Beta v0.9 — Final Package Creation
# Execute after Unity build completes

param(
    [switch]$SkipWait
)

cd C:\dev\TARTARIA_new

Write-Host "`n=== TARTARIA BETA PACKAGING ===" -ForegroundColor Cyan

# Wait for exe if needed
if (-not $SkipWait) {
    Write-Host "Waiting for build completion..." -ForegroundColor Yellow
    while (-not (Test-Path "Build\Windows\Tartaria.exe")) { 
        Start-Sleep -Seconds 10 
    }
}

# Verify exe exists
if (-not (Test-Path "Build\Windows\Tartaria.exe")) {
    Write-Host "ERROR: Tartaria.exe not found!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build found - creating package..." -ForegroundColor Green

# Package directory name
$pkgDir = "TARTARIA_Beta_v0.9_Win64"

# Clean old package if exists
if (Test-Path $pkgDir) {
    Remove-Item $pkgDir -Recurse -Force
}
if (Test-Path "$pkgDir.zip") {
    Remove-Item "$pkgDir.zip" -Force
}

# Create package directory
New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null

# Copy build files
Write-Host "Copying build files..." -ForegroundColor Yellow
Copy-Item -Path "Build\Windows\*" -Destination $pkgDir -Recurse -Force

# Copy documentation
Write-Host "Copying documentation..." -ForegroundColor Yellow
Copy-Item README.md $pkgDir\ -EA 0
Copy-Item BETA_RELEASE_NOTES.md $pkgDir\ -EA 0
Copy-Item BUILD_METADATA.md $pkgDir\ -EA 0

# Copy known issues as KNOWN_ISSUES.txt
if (Test-Path SHIP_CHECKLIST.md) {
    Copy-Item SHIP_CHECKLIST.md $pkgDir\KNOWN_ISSUES.txt -EA 0
}

# Create ZIP
Write-Host "Creating ZIP archive..." -ForegroundColor Yellow
Compress-Archive -Path $pkgDir -DestinationPath "$pkgDir.zip" -CompressionLevel Optimal -Force

# Generate checksum
Write-Host "Generating checksum..." -ForegroundColor Yellow
$hash = (Get-FileHash "$pkgDir.zip" -Algorithm SHA256).Hash
"$hash  $pkgDir.zip" | Out-File "$pkgDir.zip.sha256" -Encoding ascii

# Collect statistics
$exeSize = [math]::Round((Get-Item "Build\Windows\Tartaria.exe").Length / 1MB, 1)
$dataSize = [math]::Round((Get-Item "Build\Windows\Tartaria_Data").Length / 1MB, 1)
$totalBuildSize = [math]::Round((Get-ChildItem "Build\Windows" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 1)
$zipSize = [math]::Round((Get-Item "$pkgDir.zip").Length / 1MB, 1)

# Report results
Write-Host "`n✅ PACKAGE READY" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "Package: $pkgDir.zip" -ForegroundColor Cyan
Write-Host "Size: $zipSize MB (compressed from $totalBuildSize MB)" -ForegroundColor Cyan
Write-Host "SHA256: $hash" -ForegroundColor Gray
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

# Return stats for BUILD_FINAL_STATS.md
return @{
    ExeSize = $exeSize
    DataSize = $dataSize
    TotalSize = $totalBuildSize
    ZipSize = $zipSize
    Hash = $hash
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
}
