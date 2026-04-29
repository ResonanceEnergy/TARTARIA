# automate-lfs-and-push.ps1
# Solo-dev: install LFS, stage edits from this Cowork session, commit + push.
# Safe: does NOT rewrite history. The 699 MB binaries from commit 429a9b3 stay
# in history; LFS only applies to FUTURE binary additions. Run lfs-migrate-history.ps1
# separately when you're ready to shrink history (destructive, force-push).

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

function Section($t) { Write-Host "`n=== $t ===" -ForegroundColor Cyan }
function Ok($t)      { Write-Host "  [OK] $t"     -ForegroundColor Green }
function Warn($t)    { Write-Host "  [WARN] $t"   -ForegroundColor Yellow }
function Fail($t)    { Write-Host "  [FAIL] $t"   -ForegroundColor Red; exit 1 }

Section "0. Sanity"
if (-not (Test-Path ".git")) { Fail "Not a git repo: $RepoRoot" }

# Stale index.lock cleanup - Unity sometimes leaves these behind on Editor crash
$lock = ".git/index.lock"
if (Test-Path $lock) {
    $unityRunning = $null -ne (Get-Process -Name Unity -ErrorAction SilentlyContinue)
    if ($unityRunning) {
        Fail "Unity Editor is running (holds .git/index.lock). Close Unity, then re-run."
    }
    $age = (Get-Date) - (Get-Item $lock).LastWriteTime
    if ($age.TotalMinutes -ge 1) {
        Remove-Item $lock -Force
        $ageStr = $age.TotalMinutes.ToString("N1")
        Ok "Removed stale .git/index.lock (age $ageStr min)"
    } else {
        Fail "Fresh index.lock present - another git process may be running. Wait 30s and retry."
    }
}

$branch = (git rev-parse --abbrev-ref HEAD).Trim()
Write-Host "  Branch: $branch"
Write-Host "  Remote: $((git remote get-url origin).Trim())"

Section "1. Install Git LFS hooks"
git lfs version 2>$null
if ($LASTEXITCODE -ne 0) {
    Fail "git-lfs not installed. Install from https://git-lfs.com/ or via 'winget install GitHub.GitLFS' then re-run."
}
git lfs install --local
if ($LASTEXITCODE -ne 0) { Fail "git lfs install failed" }
Ok "LFS hooks installed in this repo"

Section "2. Verify .gitattributes is the new comprehensive file"
if (-not (Select-String -Path ".gitattributes" -Pattern "filter=lfs" -Quiet)) {
    Fail ".gitattributes missing LFS rules. Did .gitattributes write get reverted?"
}
$lfsRuleCount = (Select-String -Path ".gitattributes" -Pattern "filter=lfs").Count
Ok ".gitattributes has $lfsRuleCount LFS rules"

Section "3. Show pending changes Cowork made"
git status --short | Select-Object -First 40

Section "4. Stage curated set of changes"
# Only stage the files we touched + any meta files Unity regenerated for them.
# Avoid sweeping in the 1900-file dirty tree blindly.
$files = @(
    ".gitattributes",
    ".github/workflows/unity-build.yml",
    "Assets/_Project/Scripts/Core/GameBootstrap.cs",
    "Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs",
    "Tools/automate-lfs-and-push.ps1",
    "Tools/lfs-migrate-history.ps1",
    "AUTOMATE.bat"
)
foreach ($f in $files) {
    if (Test-Path $f) {
        git add -- $f
        Ok "staged $f"
    } else {
        Warn "missing $f (not added)"
    }
}

# Stage matching .meta files for the .cs edits if they exist
foreach ($m in @("Assets/_Project/Scripts/Core/GameBootstrap.cs.meta",
                 "Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs.meta")) {
    if (Test-Path $m) { git add -- $m }
}

Section "5. Diff stat of what will be committed"
git diff --cached --stat

Section "6. Commit"
$msgPath = Join-Path $env:TEMP "tartaria-commit-msg.txt"
$lines = @(
    "chore(infra): Git LFS rules + CI workflow + Inventory bridge",
    "",
    "- .gitattributes: full LFS coverage for FBX/audio/textures/HDR/EXR/.bytes/video/archives.",
    "  Unity YAML files (.unity/.prefab/.mat/.asset/.controller/.anim/.meta) kept as text",
    "  for git merge.",
    "- .github/workflows/unity-build.yml: validate job + Win64 build (gated on",
    "  HAS_UNITY_LICENSE repo var + UNITY_LICENSE/EMAIL/PASSWORD secrets).",
    "- GameBootstrap.cs: EnsureInventorySystem() auto-spawns InventorySystem singleton",
    "  at boot so PickupInteractable.Interact() and ShovelPickup.Interact() can call",
    "  it safely.",
    "- EchohavenContentSpawner.cs: ShovelPickup.Interact() now bridges into",
    "  InventorySystem.Instance.AddItem('shovel', 1). Legacy ShovelAcquired flag",
    "  preserved.",
    "",
    "Note: this DOES NOT migrate the 699 MB of binaries already committed in 429a9b3",
    "into LFS. Run Tools/lfs-migrate-history.ps1 when ready (rewrites history)."
)
$lines | Set-Content -Path $msgPath -Encoding ASCII

git commit -F $msgPath
$commitRc = $LASTEXITCODE
Remove-Item $msgPath -ErrorAction SilentlyContinue

if ($commitRc -ne 0) {
    Warn "git commit returned non-zero (maybe nothing to commit?). Continuing anyway."
} else {
    Ok "Commit created"
}

Section "7. Push"
git push origin $branch
if ($LASTEXITCODE -ne 0) { Fail "git push failed - check network / credentials and retry manually" }
Ok "Pushed to origin/$branch"

Section "8. Done"
Write-Host "  Visit: https://github.com/ResonanceEnergy/TARTARIA/actions  to watch CI fire"
Write-Host "  Workflow runs even WITHOUT a Unity license - the 'validate' job has no license dep."
Write-Host "  To enable the actual Win64 build job, set repo variable HAS_UNITY_LICENSE=true and"
Write-Host "  add secrets UNITY_LICENSE / UNITY_EMAIL / UNITY_PASSWORD."
