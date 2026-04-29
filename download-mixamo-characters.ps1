# download-mixamo-characters.ps1
# Semi-automated Mixamo character downloader with download monitoring
#
# Opens browser tabs for each character with search pre-filled,
# monitors Downloads folder for FBX files, renames and moves them automatically.
#
# Usage: .\download-mixamo-characters.ps1

param(
    [string]$DownloadsFolder = "$env:USERPROFILE\Downloads"
)

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

$OutputDir = "Assets\_Project\Models\Characters"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Character mapping: (Mixamo search query, output filename, description)
$characters = @(
    @{Query="Adventurer"; File="Player_Mesh.fbx"; Desc="Male adventurer for Player"},
    @{Query="Queen"; File="Anastasia_Mesh.fbx"; Desc="Female royal for Anastasia"},
    @{Query="Worker"; File="Milo_Mesh.fbx"; Desc="Male engineer for Milo"},
    @{Query="Mutant"; File="MudGolem_Mesh.fbx"; Desc="Creature for MudGolem"}
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " MIXAMO CHARACTER DOWNLOADER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "  1. Open browser tabs for each character" -ForegroundColor Gray
Write-Host "  2. Monitor your Downloads folder for .fbx files" -ForegroundColor Gray
Write-Host "  3. Auto-rename and move files to Assets\_Project\Models\Characters\" -ForegroundColor Gray
Write-Host ""
Write-Host "YOUR TASK (in each tab):" -ForegroundColor Yellow
Write-Host "  1. Click the FIRST character result" -ForegroundColor White
Write-Host "  2. Click DOWNLOAD button (top-right)" -ForegroundColor White
Write-Host "  3. In download dialog: confirm FBX for Unity, T-pose" -ForegroundColor White
Write-Host "  4. Click final DOWNLOAD button" -ForegroundColor White
Write-Host "  5. Wait for script to detect it (~2 seconds)" -ForegroundColor White
Write-Host "  6. Move to next tab" -ForegroundColor White
Write-Host ""

# Check existing files
$completed = @()
foreach ($char in $characters) {
    $outPath = Join-Path $OutputDir $char.File
    if (Test-Path $outPath) {
        Write-Host "  ✓ SKIP: $($char.File) already exists" -ForegroundColor Green
        $completed += $char.Query
    }
}

$remaining = $characters | Where-Object { $_.Query -notin $completed }
if ($remaining.Count -eq 0) {
    Write-Host ""
    Write-Host "All 4 characters already downloaded!" -ForegroundColor Green
    Write-Host "Output: $OutputDir" -ForegroundColor Gray
    exit 0
}

Write-Host ""
Write-Host "Opening browser tabs..." -ForegroundColor Cyan
Start-Sleep -Seconds 1

# Open browser tabs
foreach ($char in $remaining) {
    $searchUrl = "https://www.mixamo.com/#/?page=1&query=$($char.Query)&type=Character"
    Start-Process $searchUrl
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "Monitoring Downloads folder for .fbx files..." -ForegroundColor Cyan
Write-Host "  Watching: $DownloadsFolder" -ForegroundColor Gray
Write-Host ""

$downloadedCount = 0
$expectedCount = $remaining.Count
$processedFiles = @()

# Monitor loop
$timeout = [DateTime]::Now.AddMinutes(15)
while ($downloadedCount -lt $expectedCount -and [DateTime]::Now -lt $timeout) {
    
    # Find newest .fbx file in Downloads
    $fbxFiles = Get-ChildItem -Path $DownloadsFolder -Filter "*.fbx" -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notin $processedFiles } |
        Sort-Object LastWriteTime -Descending

    if ($fbxFiles.Count -gt 0) {
        $newFile = $fbxFiles[0]
        
        # Wait for file to finish writing (check size stability)
        $stableSize = $false
        $lastSize = 0
        for ($i = 0; $i -lt 5; $i++) {
            Start-Sleep -Milliseconds 500
            try {
                $currentSize = (Get-Item $newFile.FullName).Length
                if ($currentSize -eq $lastSize -and $currentSize -gt 0) {
                    $stableSize = $true
                    break
                }
                $lastSize = $currentSize
            } catch {
                # File still being written
            }
        }

        if (-not $stableSize) {
            Start-Sleep -Seconds 1
            continue
        }

        # Determine which character this is (by order)
        $targetChar = $remaining[$downloadedCount]
        $destPath = Join-Path $OutputDir $targetChar.File

        try {
            Move-Item -Path $newFile.FullName -Destination $destPath -Force
            Write-Host "  [$($downloadedCount + 1)/$expectedCount] ✓ $($targetChar.Query) → $($targetChar.File)" -ForegroundColor Green
            Write-Host "       Size: $([math]::Round($lastSize / 1MB, 2)) MB" -ForegroundColor Gray
            $processedFiles += $newFile.Name
            $downloadedCount++
        } catch {
            Write-Host "  ✗ Failed to move $($newFile.Name): $_" -ForegroundColor Red
        }
    }

    Start-Sleep -Milliseconds 1000
}

Write-Host ""
if ($downloadedCount -eq $expectedCount) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host " ✓ ALL CHARACTERS DOWNLOADED!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Downloaded files:" -ForegroundColor Cyan
    foreach ($char in $remaining) {
        $path = Join-Path $OutputDir $char.File
        if (Test-Path $path) {
            $size = [math]::Round((Get-Item $path).Length / 1MB, 2)
            Write-Host "  • $($char.File) ($size MB) - $($char.Desc)" -ForegroundColor Gray
        }
    }
    Write-Host ""
    Write-Host "Next step: Run .\tartaria-play.ps1 to auto-import FBX files!" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host " TIMEOUT - Only $downloadedCount/$expectedCount downloaded" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Re-run this script to resume." -ForegroundColor Gray
    Write-Host ""
}
