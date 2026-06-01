# TARTARIA M4 — Beta Package Preparation
# Creates distributable .zip with standalone build + documentation
# Dr. Vex Aurelian — Session 5 automation

$ErrorActionPreference = "Stop"
$ProjectPath = "C:\dev\TARTARIA_new"
$BuildDir = "$ProjectPath\Build\Windows"
$PackageDir = "$ProjectPath\BetaPackage_Echohaven"
$ZipPath = "$ProjectPath\TARTARIA_Beta_Echohaven_VerticalSlice_$(Get-Date -Format 'yyyyMMdd').zip"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " TARTARIA M4 — Beta Package Preparation" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check standalone build exists
if (-not (Test-Path "$BuildDir\Tartaria.exe")) {
    Write-Host "ERROR: Standalone build not found at $BuildDir\Tartaria.exe" -ForegroundColor Red
    Write-Host "Run .\run-m3-gates.ps1 first to build the standalone .exe" -ForegroundColor Yellow
    exit 1
}

Write-Host "Standalone build found: $BuildDir\Tartaria.exe" -ForegroundColor Green
$exeSize = (Get-Item "$BuildDir\Tartaria.exe").Length / 1MB
Write-Host "   Size: $([math]::Round($exeSize, 1)) MB" -ForegroundColor Cyan
Write-Host ""

# Clean package directory
if (Test-Path $PackageDir) {
    Write-Host "Cleaning existing package directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $PackageDir
}

Write-Host "Creating package directory: $PackageDir" -ForegroundColor Yellow
New-Item -ItemType Directory -Path $PackageDir | Out-Null

# Copy standalone build
Write-Host ">> Copying standalone build..." -ForegroundColor Yellow
Copy-Item -Recurse "$BuildDir\*" "$PackageDir\" -Force
Write-Host "   OK  Standalone build copied" -ForegroundColor Green

# Copy documentation
Write-Host ">> Copying documentation..." -ForegroundColor Yellow
$docs = @(
    "README.md",
    "BUILD_GUIDE.md",
    "KNOWN_ISSUES.md",
    "TROUBLESHOOTING.md",
    "CONTRIBUTING.md",
    "CHANGELOG.md"
)

foreach ($doc in $docs) {
    if (Test-Path "$ProjectPath\$doc") {
        Copy-Item "$ProjectPath\$doc" "$PackageDir\" -Force
        Write-Host "   Copied: $doc" -ForegroundColor Cyan
    } else {
        Write-Host "   WARNING: $doc not found, skipping" -ForegroundColor Yellow
    }
}

# Copy docs/ folder (design documents)
if (Test-Path "$ProjectPath\docs") {
    Write-Host ">> Copying docs/ folder..." -ForegroundColor Yellow
    Copy-Item -Recurse "$ProjectPath\docs" "$PackageDir\docs" -Force
    $docCount = (Get-ChildItem "$PackageDir\docs" -File).Count
    Write-Host "   OK  Copied $docCount design documents" -ForegroundColor Green
} else {
    Write-Host "   WARNING: docs/ folder not found" -ForegroundColor Yellow
}

# Create INSTALL.txt
Write-Host ">> Creating INSTALL.txt..." -ForegroundColor Yellow
$installText = @"
TARTARIA — Echohaven Vertical Slice (Beta)
===========================================

INSTALLATION:
1. Extract all files from this archive to a folder on your PC
2. Double-click Tartaria.exe to launch the game
3. No installation required, runs portable

SYSTEM REQUIREMENTS:
- OS: Windows 10/11 (64-bit)
- CPU: 4-core 2.5 GHz (6-core 3.0 GHz recommended)
- GPU: GTX 1050 / 4 GB (GTX 1070 / 8 GB recommended)
- RAM: 8 GB (16 GB recommended)
- DirectX: Version 11 or 12
- Storage: 3 GB available space

CONTROLS:
- Keyboard + Mouse or Xbox/PlayStation gamepad supported
- See BUILD_GUIDE.md for full control reference

DOCUMENTATION:
- README.md — Project overview
- BUILD_GUIDE.md — Build instructions (for developers)
- KNOWN_ISSUES.md — Known issues and workarounds
- CHANGELOG.md — Version history
- docs/ — Full design documentation (30+ files)

SUPPORT:
- GitHub: https://github.com/ResonanceEnergy/TARTARIA
- Issues: https://github.com/ResonanceEnergy/TARTARIA/issues

FEEDBACK:
Please report any bugs, crashes, or performance issues via GitHub Issues.
Include your system specs, what you were doing when the issue occurred,
and any error messages from the logs.

GAME LOGS:
- Windows: %USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Player.log
- Attach this file when reporting crashes

Build Date: $(Get-Date -Format 'yyyy-MM-dd')
Build Version: Beta Vertical Slice — Echohaven (Moon 1)

Thank you for playtesting TARTARIA!
— Resonance Energy Team
"@

Set-Content -Path "$PackageDir\INSTALL.txt" -Value $installText -Encoding UTF8
Write-Host "   OK  INSTALL.txt created" -ForegroundColor Green

# Calculate package size
Write-Host ""
Write-Host ">> Calculating package size..." -ForegroundColor Yellow
$packageSize = (Get-ChildItem -Recurse "$PackageDir" | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "   Uncompressed: $([math]::Round($packageSize, 1)) MB" -ForegroundColor Cyan

# Create .zip archive
Write-Host ""
Write-Host ">> Creating .zip archive..." -ForegroundColor Yellow
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path "$PackageDir\*" -DestinationPath $ZipPath -CompressionLevel Optimal
Write-Host "   OK  Archive created: $ZipPath" -ForegroundColor Green

$zipSize = (Get-Item $ZipPath).Length / 1MB
Write-Host "   Compressed: $([math]::Round($zipSize, 1)) MB" -ForegroundColor Cyan
$compressionRatio = ($zipSize / $packageSize) * 100
Write-Host "   Compression ratio: $([math]::Round($compressionRatio, 1))%" -ForegroundColor Cyan

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " M4 BETA PACKAGE COMPLETE" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package: $ZipPath" -ForegroundColor Yellow
Write-Host "Size: $([math]::Round($zipSize, 1)) MB (compressed)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Test the .zip extraction and Tartaria.exe launch" -ForegroundColor White
Write-Host "  2. Upload to itch.io / Steam / distribution platform" -ForegroundColor White
Write-Host "  3. Share with beta testers" -ForegroundColor White
Write-Host ""
Write-Host "Beta package ready for distribution!" -ForegroundColor Green
