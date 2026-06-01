# Package & Ship — Post-Build Instructions

## When Build Completes Successfully

Run this script to create the distribution ZIP:

```powershell
# ========================================
# TARTARIA Beta v0.9 — Package Script
# ========================================

$ErrorActionPreference = "Stop"
$buildDir = "C:\dev\TARTARIA_new\Build\Windows"
$outputZip = "C:\dev\TARTARIA_new\TARTARIA_Beta_v0.9_Win64.zip"
$tempPkg = "C:\dev\TARTARIA_new\Temp\Package"

Write-Host "`n=== TARTARIA BETA v0.9 PACKAGING ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Verify build exists
if (-not (Test-Path "$buildDir\Tartaria.exe")) {
    Write-Host "✗ Build not found at $buildDir\Tartaria.exe" -ForegroundColor Red
    Write-Host "  Run build-beta.ps1 first!" -ForegroundColor Yellow
    exit 1
}

Write-Host "[1/5] Build verified" -ForegroundColor Green
$exe = Get-Item "$buildDir\Tartaria.exe"
Write-Host "  Tartaria.exe: $([math]::Round($exe.Length/1MB, 2)) MB"

# Step 2: Create temp package folder
Write-Host "[2/5] Creating package structure..." -ForegroundColor Yellow
if (Test-Path $tempPkg) { Remove-Item $tempPkg -Recurse -Force }
New-Item -ItemType Directory -Path $tempPkg -Force | Out-Null

# Step 3: Copy build files
Write-Host "[3/5] Copying build files..." -ForegroundColor Yellow
Copy-Item "$buildDir\*" -Destination $tempPkg -Recurse -Force

# Step 4: Add documentation
Write-Host "[4/5] Adding documentation..." -ForegroundColor Yellow
Copy-Item "C:\dev\TARTARIA_new\README.md" -Destination "$tempPkg\" -Force
Copy-Item "C:\dev\TARTARIA_new\BETA_RELEASE_NOTES.md" -Destination "$tempPkg\" -Force
Copy-Item "C:\dev\TARTARIA_new\BUILD_METADATA.md" -Destination "$tempPkg\" -Force
Copy-Item "C:\dev\TARTARIA_new\KNOWN_ISSUES.md" -Destination "$tempPkg\" -Force

# Check for LICENSE
if (Test-Path "C:\dev\TARTARIA_new\LICENSE") {
    Copy-Item "C:\dev\TARTARIA_new\LICENSE" -Destination "$tempPkg\" -Force
    Write-Host "  + LICENSE"
}

Write-Host "  + README.md"
Write-Host "  + BETA_RELEASE_NOTES.md"
Write-Host "  + BUILD_METADATA.md"
Write-Host "  + KNOWN_ISSUES.md"

# Step 5: Create ZIP
Write-Host "[5/5] Creating ZIP archive..." -ForegroundColor Yellow
if (Test-Path $outputZip) { Remove-Item $outputZip -Force }

# Use .NET compression (faster than Compress-Archive for large files)
Add-Type -Assembly System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempPkg, $outputZip, "Optimal", $false)

Write-Host ""
Write-Host "✓ PACKAGE COMPLETE" -ForegroundColor Green
Write-Host ""

# Report stats
$zipInfo = Get-Item $outputZip
$allFiles = Get-ChildItem $tempPkg -Recurse -File
$totalSize = ($allFiles | Measure-Object -Property Length -Sum).Sum

Write-Host "=== BUILD STATS ===" -ForegroundColor Cyan
Write-Host "  Build folder: $([math]::Round($totalSize/1MB, 2)) MB uncompressed"
Write-Host "  ZIP file: $([math]::Round($zipInfo.Length/1MB, 2)) MB compressed"
Write-Host "  Compression ratio: $([math]::Round(($zipInfo.Length / $totalSize) * 100, 1))%"
Write-Host "  Total files: $($allFiles.Count)"
Write-Host ""
Write-Host "  Output: $outputZip"
Write-Host ""

# Generate SHA256 checksum
Write-Host "=== SHA256 CHECKSUM ===" -ForegroundColor Cyan
$hash = (Get-FileHash $outputZip -Algorithm SHA256).Hash
Write-Host "  $hash"
Write-Host ""
Write-Host "  (Include this checksum in distribution announcement for integrity verification)"
Write-Host ""

# Cleanup
Write-Host "Cleaning up temp files..." -ForegroundColor Gray
Remove-Item $tempPkg -Recurse -Force

Write-Host ""
Write-Host "? READY TO SHIP!" -ForegroundColor Green
Write-Host "  Next: Upload $outputZip to itch.io, Steam, or manual distribution"
Write-Host "  See SHIP_CHECKLIST.md for upload instructions"
Write-Host ""
```

## Manual Steps (After Running Script Above)

### 1. Verify Package
```powershell
# Extract to test folder
Expand-Archive TARTARIA_Beta_v0.9_Win64.zip -DestinationPath C:\Temp\TARTARIA_Test
# Launch and verify game starts
C:\Temp\TARTARIA_Test\Tartaria.exe
```

### 2. Upload (Choose One Platform)

**Option A: itch.io**
- Go to https://itch.io/game/new
- Upload `TARTARIA_Beta_v0.9_Win64.zip`
- Set pricing to Free or Pay What You Want
- Mark as "In development" (Beta)
- Generate beta keys if needed

**Option B: Steam Playtest**
- Use Steamworks SDK + SteamPipe
- Configure `app_build_XXXXXX.vdf`
- Upload via: `steamcmd.exe +login +run_app_build +quit`
- Set live on "beta" branch in Steamworks portal

**Option C: Manual (Google Drive / Dropbox)**
- Upload ZIP to cloud storage
- Share link (set to "Anyone with the link")
- Include SHA256 checksum in announcement

### 3. Git Commit
```powershell
cd C:\dev\TARTARIA_new
if (Test-Path .\nul) { Remove-Item .\nul -Force }
git add -A
git commit -m "BETA v0.9 READY: Windows x64 build complete. ZIP packaged. All 13 Moons, 4 companions, 3 endings, CS:0. Fixed Burst compilation + IL2CPP→Mono. Known issues documented."
git tag -a v0.9.0-beta -m "Beta v0.9.0 Release Candidate"
git push origin main --tags
```

### 4. Announce Beta
- Discord `#beta-testing` channel
- Email beta testers list
- Social media (Twitter/X, Reddit r/tartaria)
- GitHub release page with ZIP attached

---

## If Build Fails

Check `C:\dev\TARTARIA_new\Logs\standalone-build.log` for errors:
```powershell
$errors = Select-String -Path Logs\standalone-build.log -Pattern "error|Error building"
$errors | ForEach-Object { $_.Line }
```

Common issues:
- Missing scenes in Build Settings → Run `MoonScenesFactory.ConfigureFullGameBuildSettings()`
- Asset import errors → Check Editor.log for red errors
- Shader compilation failures → Update graphics drivers, try Vulkan instead of DX12
