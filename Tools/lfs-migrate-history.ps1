# lfs-migrate-history.ps1
# DESTRUCTIVE: rewrites git history to move all matching binary files from
# regular git storage into LFS. After this you MUST force-push, and any other
# clones of this repo become invalid until re-cloned.
#
# Solo dev = generally safe. Run only when ready.
#
# Effect: shrinks the .git folder dramatically (often 5-20x) and makes future
# clones fast.
#
# Usage:
#   .\Tools\lfs-migrate-history.ps1            (interactive, prompts MIGRATE/PUSH)
#   .\Tools\lfs-migrate-history.ps1 -Force     (non-interactive, full migrate + push)
#   .\Tools\lfs-migrate-history.ps1 -DryRun    (analyze only, no rewrite)

param(
    [switch]$Force,
    [switch]$DryRun,
    [switch]$NoPush
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

function Section($t) { Write-Host "`n=== $t ===" -ForegroundColor Cyan }
function Ok($t)      { Write-Host "  [OK] $t"     -ForegroundColor Green }
function Warn($t)    { Write-Host "  [WARN] $t"   -ForegroundColor Yellow }
function Fail($t)    { Write-Host "  [FAIL] $t"   -ForegroundColor Red; exit 1 }

Write-Host @"
================================================================================
  Git LFS history migration - DESTRUCTIVE OPERATION
================================================================================
  This will rewrite ALL commits on the current branch to move matching binary
  files into LFS storage. After this you must force-push.

  Run this ONLY if you are the sole user of this repo, or you have coordinated
  with all collaborators to re-clone after the force-push.

  Force flag : $Force
  DryRun     : $DryRun
  NoPush     : $NoPush
================================================================================
"@ -ForegroundColor Yellow

if (-not $Force -and -not $DryRun) {
    $confirm = Read-Host "Type 'MIGRATE' to proceed, anything else to abort"
    if ($confirm -ne "MIGRATE") {
        Write-Host "Aborted." -ForegroundColor Cyan
        exit 0
    }
}

# --- Backup ---
$backupName = ".git.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Section "[1/5] Backing up .git folder to $backupName"
if ($DryRun) {
    Warn "DryRun: skipping backup"
} else {
    Copy-Item -Path ".git" -Destination $backupName -Recurse -Force
    Ok "Backup at: $backupName"
}

# --- Pre-migration size ---
Section "[2/5] Pre-migration .git size"
$preSize = (Get-ChildItem -Path .git -Recurse -Force -ErrorAction SilentlyContinue |
            Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host ("  .git: {0:N1} MB" -f $preSize)

# --- Migrate ---
Section "[3/5] Migrating binary file types into LFS across all refs"
$includes = "*.fbx,*.FBX,*.dae,*.obj,*.OBJ,*.blend," +
            "*.wav,*.WAV,*.mp3,*.ogg,*.aif,*.aiff,*.flac," +
            "*.png,*.PNG,*.jpg,*.JPG,*.jpeg," +
            "*.tga,*.TGA,*.tif,*.tiff,*.psd,*.PSD," +
            "*.hdr,*.exr,*.EXR," +
            "*.mp4,*.mov,*.webm," +
            "*.bytes,*.unitypackage,*.bundle," +
            "*.zip,*.7z,*.gz"

if ($DryRun) {
    Write-Host "  DryRun: showing files that WOULD migrate..."
    git lfs migrate info --include="$includes" --everything
    Section "[5/5] DryRun done"
    Write-Host "  No changes made. Re-run without -DryRun to actually migrate." -ForegroundColor Yellow
    exit 0
}

git lfs migrate import --include="$includes" --everything
if ($LASTEXITCODE -ne 0) {
    Fail "Migration failed. Restore from $backupName if needed."
}
Ok "Migration complete"

# --- Post-migration size ---
Section "[4/5] Post-migration .git size"
$postSize = (Get-ChildItem -Path .git -Recurse -Force -ErrorAction SilentlyContinue |
             Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host ("  .git: {0:N1} MB  (was {1:N1} MB - {2:N1} MB saved)" -f $postSize, $preSize, ($preSize - $postSize))

# --- Force push ---
Section "[5/5] Force-push rewritten history"
if ($NoPush) {
    Warn "NoPush flag set - skipping push. Manual: git push --force-with-lease origin --all"
} elseif ($Force) {
    git push --force-with-lease origin --all
    if ($LASTEXITCODE -ne 0) { Fail "git push --all failed" }
    git push --force-with-lease origin --tags
    Ok "Force-push complete"
} else {
    $confirm2 = Read-Host "Type 'PUSH' to force-push, anything else to skip"
    if ($confirm2 -eq "PUSH") {
        git push --force-with-lease origin --all
        if ($LASTEXITCODE -ne 0) { Fail "git push --all failed" }
        git push --force-with-lease origin --tags
        Ok "Force-push complete"
    } else {
        Warn "Skipped. To push later: git push --force-with-lease origin --all"
    }
}

Write-Host "`nDone. Backup at $backupName - delete it when you've verified everything works." -ForegroundColor Cyan
