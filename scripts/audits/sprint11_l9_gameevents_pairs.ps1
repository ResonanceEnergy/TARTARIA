param(
    [string]$RepoRoot = 'C:\dev\_wt_s11_l9_events'
)

Set-Location $RepoRoot

$files = git ls-files '*.cs' | Where-Object { $_ -like 'Assets/_Project/Scripts/*' }
$gameEventsRel = 'Assets/_Project/Scripts/Core/GameEvents.cs'

# Load file contents once
$fileMap = @{}
foreach ($f in $files) {
    $full = Join-Path $RepoRoot $f
    if (Test-Path $full) {
        $fileMap[$f] = [System.IO.File]::ReadAllText($full)
    }
}

# Discover declared event names
$eventMatches = Select-String -Path $gameEventsRel -Pattern '^\s*public static (event )?Action'
$eventInfos = @()
foreach ($m in $eventMatches) {
    if ($m.Line -match '\b(On[A-Za-z0-9_]+)') {
        $name = $Matches[1]
        $eventInfos += [pscustomobject]@{ Name=$name; DeclLine=$m.LineNumber }
    }
}
$eventInfos = $eventInfos | Sort-Object Name -Unique

# Parse GameEvents.cs to build a map: <eventName> -> list of helper method names (Fire*/Raise*)
# that internally invoke that event. This lets us count CALLERS of those helpers as publishers.
$gameEventsTxt = $fileMap[$gameEventsRel]
$lines = $gameEventsTxt -split "`r?`n"
$eventToHelpers = @{}
foreach ($ev in $eventInfos) { $eventToHelpers[$ev.Name] = New-Object System.Collections.Generic.HashSet[string] }

# Walk lines: track current helper method name; when we see an OnFoo invocation, attribute it.
$currentHelpers = New-Object System.Collections.Generic.List[string]
$braceDepth = 0
$inHelper = $false
for ($i=0; $i -lt $lines.Length; $i++) {
    $L = $lines[$i]
    # Detect helper method declaration (Fire* or Raise*)
    if ($L -match 'public static void (Fire[A-Za-z0-9_]+|Raise[A-Za-z0-9_]+)\s*\(') {
        $helperName = $Matches[1]
        # Single-line expression-bodied: => OnFoo?.Invoke(...)
        if ($L -match '=>\s*(On[A-Za-z0-9_]+)\??\.Invoke') {
            $evt = $Matches[1]
            if ($eventToHelpers.ContainsKey($evt)) { [void]$eventToHelpers[$evt].Add($helperName) }
            continue
        }
        # Block-bodied — scan forward until matching closing brace
        $currentHelper = $helperName
        $depth = 0
        $started = $false
        for ($j=$i; $j -lt $lines.Length; $j++) {
            $line2 = $lines[$j]
            $openCount = ([regex]::Matches($line2, '\{')).Count
            $closeCount = ([regex]::Matches($line2, '\}')).Count
            if ($openCount -gt 0) { $started = $true }
            $depth += $openCount - $closeCount
            # Detect OnFoo invocation lines inside the body
            $invMatches = [regex]::Matches($line2, '\b(On[A-Za-z0-9_]+)\s*\??\.Invoke')
            foreach ($im in $invMatches) {
                $evt = $im.Groups[1].Value
                if ($eventToHelpers.ContainsKey($evt)) { [void]$eventToHelpers[$evt].Add($currentHelper) }
            }
            if ($started -and $depth -le 0) { $i = $j; break }
        }
    }
}

function Find-Matches {
    param([string]$Pattern, [bool]$ExcludeGameEvents=$true)
    $hits = @()
    foreach ($kv in $fileMap.GetEnumerator()) {
        $rel = $kv.Key
        if ($ExcludeGameEvents -and $rel -eq $gameEventsRel) { continue }
        $txt = $kv.Value
        $lineNum = 0
        foreach ($line in ($txt -split "`r?`n")) {
            $lineNum++
            if ($line -match $Pattern) { $hits += "$rel`:$lineNum" }
        }
    }
    return ,$hits
}

$rows = @()
foreach ($ev in $eventInfos) {
    $name = $ev.Name
    $helpers = $eventToHelpers[$name] | Sort-Object -Unique
    $pubPatterns = @()
    foreach ($h in $helpers) { $pubPatterns += "\b${h}\s*\(" }
    # Plus direct invokes outside GameEvents.cs
    $pubPatterns += "\b${name}\s*\?\.Invoke"
    $pubPatterns += "\b${name}\s*\.Invoke"
    $pubPat = '(?:' + ($pubPatterns -join '|') + ')'

    $pubs = Find-Matches -Pattern $pubPat -ExcludeGameEvents $true

    # Subscribers: name += or name -=
    $subPat = "\b${name}\s*[+\-]="
    $subs = Find-Matches -Pattern $subPat -ExcludeGameEvents $true

    $status = if ($pubs.Count -ge 1 -and $subs.Count -ge 1) { 'HEALTHY' }
              elseif ($pubs.Count -eq 0 -and $subs.Count -eq 0) { 'UNUSED' }
              else { 'BROKEN' }

    $rows += [pscustomobject]@{
        Name=$name
        DeclLine=$ev.DeclLine
        Helpers=($helpers -join ',')
        PublisherCount=$pubs.Count
        Publishers=($pubs -join '; ')
        SubscriberCount=$subs.Count
        Subscribers=($subs -join '; ')
        Status=$status
    }
}

$csvOut = Join-Path $RepoRoot 'docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.csv'
$rows | Export-Csv -Path $csvOut -NoTypeInformation -Encoding UTF8

$healthy = ($rows | Where-Object Status -eq 'HEALTHY').Count
$unused  = ($rows | Where-Object Status -eq 'UNUSED').Count
$broken  = ($rows | Where-Object Status -eq 'BROKEN').Count
Write-Host "Events: $($rows.Count) | Healthy: $healthy | Unused: $unused | Broken: $broken"

$global:GAMEEVENTS_AUDIT_ROWS = $rows
