<#
.SYNOPSIS
  Cut and push the moon1-ship-candidate annotated tag.

.DESCRIPTION
  Reads current HEAD SHA of feature/consolidate-moon-architecture, creates the
  annotated tag `moon1-ship-candidate` with a payload containing the SHA, the
  v3 acceptance audit tally (77 OK / 12 WARN / 2 FAIL), and a branch-coverage
  summary, then pushes the tag to origin.

  RUN MANUALLY after Sprint 10 has merged into feature/consolidate-moon-architecture.
  Sprint 10 Lane 8 does NOT auto-invoke this script.

.NOTES
  Owner: NATRIX (nate@gripandripphdd.com)
  Branch: feature/consolidate-moon-architecture
  Tag:    moon1-ship-candidate
  Channel: itch.io moon1-windows
  Date created: 2026-06-02 (Sprint 10 Lane 8)
#>

[CmdletBinding()]
param(
    [string] $RepoRoot = (Get-Location).Path,
    [string] $Branch   = 'feature/consolidate-moon-architecture',
    [string] $TagName  = 'moon1-ship-candidate',
    [string] $Remote   = 'origin',
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([string] $Msg)
    Write-Host ''
    Write-Host '================================================================' -ForegroundColor Cyan
    Write-Host "  $Msg" -ForegroundColor Cyan
    Write-Host '================================================================' -ForegroundColor Cyan
}

function Write-Ok    { param([string] $Msg) Write-Host "[OK]   $Msg" -ForegroundColor Green }
function Write-Info  { param([string] $Msg) Write-Host "[INFO] $Msg" -ForegroundColor Yellow }
function Write-Fail  { param([string] $Msg) Write-Host "[FAIL] $Msg" -ForegroundColor Red }

# -------------------------------------------------------------------
# Step 1 - sanity check repo + branch
# -------------------------------------------------------------------
Write-Step 'Step 1 / 5: Sanity-check repository and branch'

Push-Location $RepoRoot
try {
    $insideRepo = & git rev-parse --is-inside-work-tree 2>$null
    if ($insideRepo -ne 'true') {
        Write-Fail "Not inside a git working tree: $RepoRoot"
        exit 1
    }
    Write-Ok "Git work tree: $RepoRoot"

    # Make sure the remote ref exists locally before we tag against it.
    & git fetch $Remote $Branch --tags 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "git fetch $Remote $Branch failed (exit $LASTEXITCODE)"
        exit 1
    }
    Write-Ok "Fetched $Remote/$Branch + tags"
}
finally {
    Pop-Location
}

# -------------------------------------------------------------------
# Step 2 - resolve HEAD SHA of target branch
# -------------------------------------------------------------------
Write-Step 'Step 2 / 5: Resolve target branch HEAD SHA'

Push-Location $RepoRoot
try {
    $sha = (& git rev-parse "$Remote/$Branch").Trim()
    if (-not $sha) {
        Write-Fail "Could not resolve $Remote/$Branch HEAD SHA"
        exit 1
    }
    $shortSha = $sha.Substring(0, 8)
    Write-Ok "Target SHA: $sha"
    Write-Ok "Short SHA:  $shortSha"

    $headSubject = (& git log -1 --format='%s' $sha).Trim()
    Write-Info "HEAD commit: $headSubject"
}
finally {
    Pop-Location
}

# -------------------------------------------------------------------
# Step 3 - check tag does not already exist
# -------------------------------------------------------------------
Write-Step 'Step 3 / 5: Verify tag does not already exist'

Push-Location $RepoRoot
try {
    $existingLocal  = & git tag -l $TagName
    $existingRemote = & git ls-remote --tags $Remote "refs/tags/$TagName"

    if ($existingLocal) {
        Write-Fail "Local tag '$TagName' already exists. Delete it first with: git tag -d $TagName"
        exit 1
    }
    if ($existingRemote) {
        Write-Fail "Remote tag '$TagName' already exists on $Remote. Delete it first with: git push $Remote :refs/tags/$TagName"
        exit 1
    }
    Write-Ok "Tag '$TagName' is free locally and on $Remote"
}
finally {
    Pop-Location
}

# -------------------------------------------------------------------
# Step 4 - create annotated tag
# -------------------------------------------------------------------
Write-Step 'Step 4 / 5: Create annotated tag'

$msg = @"
TARTARIA Moon 1 (Echohaven) ship candidate

Branch:  $Branch
SHA:     $sha
HEAD:    $headSubject

Acceptance audit v3 (post-Sprint-9, see docs/release/PR_DRAFT_sprint9_to_feature.md):
  77 OK / 12 WARN / 2 FAIL
  Closest on-disk artifact: docs/audits/MOON1_ACCEPTANCE_2026-06-02_v2.md

Branch coverage (10 sprints, all landed on origin):
  Sprint  1 - Repo hygiene + scope lock + GameEvents.cs reconstruction
  Sprint  2 - Moon 1 environment + 3 hero buildings + atmosphere
  Sprint  3 - Tuning mini-game A + pedestal wiring + URP fixes
  Sprint  4 - Save/Load v15 + AdaptiveMusic Layer 2 + F310 fixes
  Sprint  5 - Blender headless art pipeline (12 assets)
  Sprint  6 - SHIP POLISH (Main Menu, Settings, SaveSlots, ambient, hit fb, tutorial, difficulty, credits)
  Sprint  7 - PR landing + content fill (hit feedback wired at 8 strike sites)
  Sprint  8 - SHIP-GATE BLITZ (compile clean, Pipe Organ routing, named villager scaffold)
  Sprint  9 - SHIP THE GATE (OnDayChanged + Lirael gate, real Blender FBX NPCs, named villagers, braziers, Celestial=528Hz canon)
  Sprint 10 - RELEASE CUT (STATUS update, release notes, this tag, butler runbook, post-merge hotfixes)

Distribution:
  Channel:        itch.io / moon1-windows
  Pricing:        pay-what-you-want
  Release notes:  docs/release/RELEASE_NOTES_moon1.md
  Build pipeline: scripts/build-itch.ps1
  Butler setup:   docs/release/BUTLER_SETUP.md

This is a ship CANDIDATE - not '100% done'. See STATUS.md punch list for
non-ship-gating polish items (CarvedStone placement, vegetation density,
17 deprecation warnings, SaveSlotPanel triage).
"@

if ($DryRun) {
    Write-Info 'DryRun set - skipping git tag invocation'
    Write-Info 'Tag message that would be applied:'
    Write-Host $msg
}
else {
    Push-Location $RepoRoot
    try {
        & git tag -a $TagName $sha -m $msg
        if ($LASTEXITCODE -ne 0) {
            Write-Fail "git tag failed (exit $LASTEXITCODE)"
            exit 1
        }
        Write-Ok "Annotated tag '$TagName' created locally at $shortSha"

        Write-Info 'Tag payload:'
        & git show --no-patch --format='' $TagName | Out-Host
    }
    finally {
        Pop-Location
    }
}

# -------------------------------------------------------------------
# Step 5 - push tag to origin
# -------------------------------------------------------------------
Write-Step "Step 5 / 5: Push tag to $Remote"

if ($DryRun) {
    Write-Info "DryRun set - skipping: git push $Remote $TagName"
}
else {
    Push-Location $RepoRoot
    try {
        & git push $Remote $TagName
        if ($LASTEXITCODE -ne 0) {
            Write-Fail "git push $Remote $TagName failed (exit $LASTEXITCODE)"
            exit 1
        }
        Write-Ok "Pushed '$TagName' to $Remote"
    }
    finally {
        Pop-Location
    }
}

Write-Step 'DONE'
Write-Ok  "Moon 1 ship-candidate tag '$TagName' -> $shortSha is on $Remote."
Write-Info "Next: run scripts/build-itch.ps1 to push the Windows x64 build to itch.io moon1-windows."
