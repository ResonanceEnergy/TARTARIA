# lfs-migrate-history.ps1
# DESTRUCTIVE: rewrites git history to move all matching binary files from
# regular git storage into LFS. After this you MUST force-push, and any other
# clones of this repo become invalid until re-cloned.
#
# Solo dev = generally safe. Run only when ready.
#
# Effect: shrinks the .git folder dramatically (often 5–20×) and makes future
# clones fast.

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

Write-Host @"
================================================================================
  Git LFS history migration — DESTRUCTIVE OPERATION
================================================================================
  This will rewrite ALL commits on the current branch to move matching binary
  files into LFS storage. After this you must force-push.

  Run this ONLY if you are the sole user of this repo, or you have coordinated
  with all collaborators to re-clone after the force-push.

  A backup of .git is recommended before proceeding.
================================================================================
"@ -ForegroundColor Yellow

$confirm = Read-Host "Type 'MIGRATE' to proceed, anything else to abort"
if ($confirm -ne "MIGRATE") {
    Write-Host "Aborted." -ForegroundColor Cyan
    exit 0
}

Write-Host "`n[1/4] Backing up .git folder to .git.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')..." -ForegroundColor Cyan
$backupName = ".git.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -Path ".git" -Destination $backupName -Recurse -Force
Write-Host "  Backup at: $backupName" -ForegroundColor Green

Write-Host "`n[2/4] Migrating binary file types into LFS across all branches and tags..." -ForegroundColor Cyan
$includes = "*.fbx,*.FBX,*.dae,*.obj,*.OBJ,*.blend," +
            "*.wav,*.WAV,*.mp3,*.ogg,*.aif,*.aiff,*.flac," +
            "*.png,*.PNG,*.jpg,*.JPG,*.jpeg," +
            "*.tga,*.TGA,*.tif,*.tiff,*.psd,*.PSD," +
            "*.hdr,*.exr,*.EXR," +
            "*.mp4,*.mov,*.webm," +
            "*.bytes,*.unitypackage,*.bundle," +
            "*.zip,*.7z,*.gz"

git lfs migrate import --include="$includes" --everything
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Migration failed. Restore from $backupName if needed." -ForegroundColor Red
    exit 1
}
Write-Host "  Migration complete." -ForegroundColor Green

Write-Host "`n[3/4] Repo size after migration:" -ForegroundColor Cyan
$gitSize = (Get-ChildItem -Path .git -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host ("  .git: {0:N1} MB" -f $gitSize)

Write-Host "`n[4/4] Force-push the rewritten history..." -ForegroundColor Cyan
$confirm2 = Read-Host "Type 'PUSH' to force-push, anything else to skip (you can push later manually)"
if ($confirm2 -eq "PUSH") {
    git push --force-with-lease origin --all
    git push --force-with-lease origin --tags
    Write-Host "  Force-push complete." -ForegroundColor Green
} else {
    Write-Host "  Skipped. To push later: git push --force-with-lease origin --all" -ForegroundColor Yellow
}

Write-Host "`nDone. Backup at $backupName — delete it when you've verified everything works." -ForegroundColor Cyan
