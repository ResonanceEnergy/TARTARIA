# Generate BUILD_FINAL_STATS.md after package creation
param(
    [Parameter(Mandatory=$true)]
    [hashtable]$Stats
)

cd C:\dev\TARTARIA_new

# Read template
$template = Get-Content "BUILD_FINAL_STATS_TEMPLATE.md" -Raw

# Read known issues from SHIP_CHECKLIST.md
$knownIssues = @"
### Priority 2 (Polish)
- Giant Mode visual scaling: Player scale transitions may jitter on low-end hardware
- Companion pathfinding: Occasional stuck behavior in dense forests
- Save file migration: Beta saves may not be compatible with v1.0 final release

### Priority 3 (Minor)
- Moon 4-13 custom SFX: Some late-game audio still using placeholder tones
- Cutscene camera: Cinematic sequences may not respect player-configured FOV
- Quest log sorting: Completed quests not visually separated from active quests
- Performance spikes: First load of each Moon may cause 1-2 second hitches
- Tutorial tooltips: Some interactions lack hover hints
- Localization: English only in this beta
- Achievements: System implemented but no Steam integration yet
"@

# Calculate compression ratio
$compressionRatio = [math]::Round((1 - ($Stats.ZipSize / $Stats.TotalSize)) * 100, 1)

# Populate template
$final = $template -replace '\[BUILDDATE\]', $Stats.BuildDate `
    -replace '\[EXESIZE\]', $Stats.ExeSize `
    -replace '\[DATASIZE\]', $Stats.DataSize `
    -replace '\[TOTALSIZE\]', $Stats.TotalSize `
    -replace '\[ZIPSIZE\]', $Stats.ZipSize `
    -replace '\[COMPRATIO\]', $compressionRatio `
    -replace '\[HASH\]', $Stats.Hash `
    -replace '\[ISSUES\]', $knownIssues `
    -replace '\[TIMESTAMP\]', (Get-Date -Format 'yyyyMMdd-HHmmss')

# Write final stats
$final | Out-File "BUILD_FINAL_STATS.md" -Encoding utf8

Write-Host "✅ BUILD_FINAL_STATS.md created" -ForegroundColor Green
