param(
    [string]$RepoRoot = 'C:\dev\_wt_s11_l9_events',
    [string]$CsvPath  = 'docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.csv',
    [string]$OutPath  = 'docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.md'
)

Set-Location $RepoRoot

$rows = Import-Csv $CsvPath
$healthy = ($rows | Where-Object Status -eq 'HEALTHY').Count
$unused  = ($rows | Where-Object Status -eq 'UNUSED').Count
$broken  = ($rows | Where-Object Status -eq 'BROKEN').Count
$total   = $rows.Count

# Status emoji map (white = healthy, yellow = unused, red = broken)
function StatusEmoji($s) {
    switch ($s) {
        'HEALTHY' { 'HEALTHY (white)' }
        'UNUSED'  { 'UNUSED (yellow)' }
        'BROKEN'  { 'BROKEN (red)' }
    }
}

# Trim cell content so the markdown table stays readable; full lists live in CSV
function Trunc([string]$s, [int]$max = 240) {
    if ([string]::IsNullOrEmpty($s)) { return '_(none)_' }
    if ($s.Length -le $max) { return $s }
    return $s.Substring(0, $max) + ' ...'
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# Sprint 11 Lane 9 - GameEvents Publisher / Subscriber Pair Audit")
[void]$sb.AppendLine()
[void]$sb.AppendLine("**Date:** 2026-06-02  ")
[void]$sb.AppendLine("**Branch:** ``agent/audit/gameevents-pairs`` (worktree ``C:\dev\_wt_s11_l9_events``)  ")
[void]$sb.AppendLine("**Scope:** every event declared in ``Assets/_Project/Scripts/Core/GameEvents.cs`` audited for at least one publisher and at least one subscriber across ``Assets/_Project/Scripts/**/*.cs`` (548 files).  ")
[void]$sb.AppendLine("**Companion CSV:** ``docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.csv`` (full file:line lists, machine-readable).  ")
[void]$sb.AppendLine()
[void]$sb.AppendLine("## Summary")
[void]$sb.AppendLine()
[void]$sb.AppendLine("| Metric | Count |")
[void]$sb.AppendLine("|---|---:|")
[void]$sb.AppendLine("| Total events declared | $total |")
[void]$sb.AppendLine("| HEALTHY (>=1 publisher AND >=1 subscriber) | $healthy |")
[void]$sb.AppendLine("| UNUSED (0 publishers AND 0 subscribers) | $unused |")
[void]$sb.AppendLine("| BROKEN (publishers but no subscribers, OR subscribers but no publishers) | $broken |")
[void]$sb.AppendLine()
[void]$sb.AppendLine("## Methodology")
[void]$sb.AppendLine()
[void]$sb.AppendLine("1. Parsed every ``public static event Action`` line in ``GameEvents.cs`` (declared events).")
[void]$sb.AppendLine("2. For each event ``OnFoo``, parsed ``GameEvents.cs`` to discover which ``Fire*`` / ``Raise*`` helper methods invoke that event. The publisher count is the number of CALLERS of those helpers (plus any direct ``OnFoo?.Invoke`` outside ``GameEvents.cs``).")
[void]$sb.AppendLine("3. For each event ``OnFoo``, counted ``OnFoo +=`` and ``OnFoo -=`` occurrences outside ``GameEvents.cs`` as subscribers.")
[void]$sb.AppendLine("4. Classification: HEALTHY = both sides present; UNUSED = both sides absent (dead event); BROKEN = exactly one side missing.")
[void]$sb.AppendLine()
[void]$sb.AppendLine("Note: ``+=``/``-=`` pairs on the same target both count as subscriber sites (so 1 logical subscriber typically shows as 2 hits — one Subscribe, one Unsubscribe). This is intentional: each line is an independent claim against the wiring contract.")
[void]$sb.AppendLine()

# Headline finding callout
[void]$sb.AppendLine("## Headline finding: CLAUDE.md canonical-facts mismatch")
[void]$sb.AppendLine()
[void]$sb.AppendLine("The ``CLAUDE.md`` canonical-facts table mentions eight events. Three of them DO NOT EXIST in ``GameEvents.cs``:")
[void]$sb.AppendLine()
[void]$sb.AppendLine("| Event | Status in code |")
[void]$sb.AppendLine("|---|---|")
[void]$sb.AppendLine("| ``OnBuildingRestored`` | HEALTHY - 5 publishers, 27 subscriber sites (Moon 1 ship-gate core) |")
[void]$sb.AppendLine("| ``OnMoonCompleted`` | HEALTHY - 2 publishers, 8 subscriber sites |")
[void]$sb.AppendLine("| ``OnPlayerDamaged`` | HEALTHY - 1 publisher, 2 subscriber sites |")
[void]$sb.AppendLine("| ``OnQuestStatusChanged`` | HEALTHY - 6 publishers, 8 subscriber sites |")
[void]$sb.AppendLine("| ``OnHUDShowDialogue`` | HEALTHY - 2 publishers, 4 subscriber sites |")
[void]$sb.AppendLine("| ``OnBrazierLit`` | **DOES NOT EXIST** - 0 declarations in ``GameEvents.cs`` |")
[void]$sb.AppendLine("| ``OnBrazierRingComplete`` | **DOES NOT EXIST** - 0 declarations in ``GameEvents.cs`` |")
[void]$sb.AppendLine("| ``OnDayChanged`` | **DOES NOT EXIST** - 0 declarations (``Moon1LiraelDay25Gate.cs:7`` notes the gap; ``Moon1BuildOutNPCs.cs:63,89`` and ``Moon1DaySmokeMenus.cs:44,66,128`` all carry TODOs waiting for it; ``API_CONTRACT.md sec 2`` claimed it at line 461 but that claim is stale) |")
[void]$sb.AppendLine()
[void]$sb.AppendLine("**Recommendation:** either declare the three missing events and wire them (Lirael's Day 25 gate, Anastasia brazier ring, day-cycle progression all depend on them), or update CLAUDE.md and ``API_CONTRACT.md`` to remove the false canonical claim. Both are documented as load-bearing for Moon 1 ship-gate.")
[void]$sb.AppendLine()

# Moon 1 ship-gate event spotlight
[void]$sb.AppendLine("## Moon 1 ship-gate event spotlight")
[void]$sb.AppendLine()
[void]$sb.AppendLine("The four events that must fire reliably for the Moon 1 happy path (per ``PHASE_1_SCOPE.md`` + ``STATUS.md``):")
[void]$sb.AppendLine()
[void]$sb.AppendLine("| Event | Publishers | Subscribers | Status | Notes |")
[void]$sb.AppendLine("|---|---:|---:|---|---|")

$shipGate = @('OnBuildingRestored','OnMoonCompleted','OnBrazierLit','OnDayChanged')
foreach ($eName in $shipGate) {
    $r = $rows | Where-Object Name -eq $eName | Select-Object -First 1
    if (-not $r) {
        [void]$sb.AppendLine("| ``$eName`` | n/a | n/a | **MISSING** | Event not declared in ``GameEvents.cs``. Wiring expected by CLAUDE.md but unfulfilled. |")
    } else {
        $note = switch ($eName) {
            'OnBuildingRestored' { "Core restoration event. ``Raise/FireBuildingRestored`` reachable from ``InteractableBuilding.cs:647``, ``CathedralRestorationSystem.cs:187``, ``DomeRestorationSystem.cs:23``, ``FountainRestorationSystem.cs:22``, ``SpireRestorationSystem.cs:22``. Wide subscriber base across HUD, Quest, Audio, Camera. Healthy." }
            'OnMoonCompleted'    { "``RaiseMoonCompleted`` called twice (verify: ``GameLoopController`` / ``MoonCompletionTracker``). 8 subscriber sites. Healthy but verify publisher actually triggers when last building of Moon 1 restored." }
            default              { "Missing - see headline finding above." }
        }
        [void]$sb.AppendLine("| ``$($r.Name)`` | $($r.PublisherCount) | $($r.SubscriberCount) | $(StatusEmoji $r.Status) | $note |")
    }
}
[void]$sb.AppendLine()

# Full table - all events, three sections by status
[void]$sb.AppendLine("## Full event audit")
[void]$sb.AppendLine()
[void]$sb.AppendLine("Cells truncate at 240 chars when long; consult the CSV for full lists.")
[void]$sb.AppendLine()

foreach ($section in @('HEALTHY','UNUSED','BROKEN')) {
    $secRows = $rows | Where-Object Status -eq $section | Sort-Object Name
    if ($secRows.Count -eq 0) { continue }
    [void]$sb.AppendLine("### " + (StatusEmoji $section) + " - $($secRows.Count) events")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("| Event | Decl line | Publisher count (file:line) | Subscriber count (file:line) |")
    [void]$sb.AppendLine("|---|---:|---|---|")
    foreach ($r in $secRows) {
        $pubCell = "**$($r.PublisherCount)** " + (Trunc $r.Publishers)
        $subCell = "**$($r.SubscriberCount)** " + (Trunc $r.Subscribers)
        [void]$sb.AppendLine("| ``$($r.Name)`` | $($r.DeclLine) | $pubCell | $subCell |")
    }
    [void]$sb.AppendLine()
}

# Top broken pairs with recommended fixes
[void]$sb.AppendLine("## Top broken events - recommended fixes")
[void]$sb.AppendLine()

$broken_list = $rows | Where-Object Status -eq 'BROKEN' | Sort-Object { -([int]$_.SubscriberCount + [int]$_.PublisherCount) }
$top = $broken_list | Select-Object -First 12
foreach ($r in $top) {
    [void]$sb.AppendLine("### ``$($r.Name)`` (pub=$($r.PublisherCount), sub=$($r.SubscriberCount))")
    [void]$sb.AppendLine("- Decl: ``GameEvents.cs:$($r.DeclLine)``")
    [void]$sb.AppendLine("- Helpers: ``$($r.Helpers)``")
    [void]$sb.AppendLine("- Publishers: $(Trunc $r.Publishers 320)")
    [void]$sb.AppendLine("- Subscribers: $(Trunc $r.Subscribers 320)")
    $fix = switch -Wildcard ($r.Name) {
        'OnHUD*' {
            if ($r.PublisherCount -eq 0) {
                "**Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls ``Raise$($r.Name.Substring(2))(...)``. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for ``OnHUDShowBossNameplate``, the boss-encounter trigger should call ``GameEvents.Raise$($r.Name.Substring(2))``). Until then, the listed HUD feature is dead UI code."
            } else { "Investigate per-case." }
        }
        'OnPlayerDeath'      { "**Publishers fire into void.** ``RaisePlayerDeath`` is called from PlayerHealth, but no subscriber re-enables player respawn UI / camera fade. Fix: subscribe HUDController.HandlePlayerDeath and CameraController.OnPlayerDeath." }
        'OnPlayerRespawned'  { "**Publishers fire into void.** ``RaisePlayerRespawned`` called from respawn logic but nothing listens. Fix: HUDController subscribe to clear death overlay; PlayerInputHandler to re-enable input." }
        'OnPlayerHealthChanged' { "**Subscribers wait forever.** HUD health bar listens but the change event is never raised. Fix: PlayerHealthController should call ``GameEvents.RaisePlayerHealthChanged(current, max)`` inside its damage / heal paths. Currently the HUD only updates via the typed ``OnPlayerDamaged`` route, missing heals." }
        'OnAetherEnergyChanged' { "**Subscribers wait forever.** Fix: AetherEnergyController.UpdateAether should call ``GameEvents.RaiseAetherEnergyChanged(value)`` when value changes; HUD aether meter stays stuck without it." }
        'OnAetherVisionToggledTyped' { "**Publishers fire into void.** ``RaiseAetherVisionToggled`` fires the typed event but only the legacy ``OnToggleAetherVision`` has listeners. Fix: migrate ``BuildingRenderer`` and ``CollectibleRenderer`` to subscribe to the typed event (so they receive the on/off bool) or remove the typed event." }
        'OnBuildingDiscoveredTyped' { "**Publishers fire into void.** ``RaiseBuildingDiscovered`` fires typed event; only legacy ``OnBuildingDiscovered`` (string, Vector3) has subscribers. Fix: migrate the 2 listed subscribers to the typed payload, or remove the typed variant." }
        'OnBossDefeated'     { "**Publishers fire into void.** No QuestManager or HUD subscriber. Fix: wire QuestManager.OnBossDefeated handler so boss kills can complete boss-objective quests, and HUD trophy display." }
        'OnQuestObjectiveProgressed' { "**Publishers fire into void.** ``FireQuestObjectiveCompleted`` / ``RaiseQuestObjectiveProgressed`` callers exist but no HUD tracker subscribes. Fix: HUDController and QuestLogUIPanel should subscribe to update the live quest tracker UI (e.g. 'Collect 5/10 shards')." }
        'OnNewGamePlusStarted' { "Phase-3 feature not in flight. Either keep stub until NG+ work begins or move declaration to a future-phase file." }
        'OnPermanentUnlockEarned' { "Phase-3 feature not in flight - see ``OnNewGamePlusStarted`` recommendation." }
        'OnWeatherHazardStarted' { "Publishers fire into void. WeatherController.PerformanceFallback / hazard logic calls Fire* but no HUD/AudioController subscribes. Fix: wire HUD weather banner subscriber, or remove if weather hazards are deferred." }
        'OnWeatherHazardEnded'   { "See ``OnWeatherHazardStarted``." }
        'OnPerformanceFallback'  { "Publishers in PerformanceManager fire but no overlay/diagnostic subscriber. Fix: wire diagnostic HUD or remove if fallback is silent-only." }
        'OnTuningNodeActivated'  { "Stub Action field with one Fire* caller but no listener. Fix: TuningMiniGame controller should subscribe to advance the tuning sequence, or remove if superseded by ``OnTuningProgress``." }
        'OnTuningProgress'       { "Subscribers wait forever. ``FireTuningProgress`` callers exist but no per-frame subscriber. Confirmed broken - HUD frequency wheel listens to ``OnHUDUpdateFrequencyWheel`` instead. Either delete ``OnTuningProgress`` (dead alt path) or migrate consumers." }
        'OnRequestActivateRSBuff' { "Publisher in input layer fires but no buff system listens. Fix: ResonanceShardBuffController should subscribe, or remove if RS buff feature is cut." }
        'OnCollectibleGathered'  { "Stub Action with one ``FireCollectibleGathered`` callsite producing only Debug.Log. No real listener. Fix: connect to QuestManager item-collection objectives or remove stub." }
        default                  { "Investigate per-case." }
    }
    [void]$sb.AppendLine("- Recommended fix: $fix")
    [void]$sb.AppendLine()
}

[void]$sb.AppendLine("## Unused events - prune or wire")
[void]$sb.AppendLine()
$unusedRows = $rows | Where-Object Status -eq 'UNUSED' | Sort-Object Name
foreach ($r in $unusedRows) {
    [void]$sb.AppendLine("- ``$($r.Name)`` (decl ``GameEvents.cs:$($r.DeclLine)``, helper ``$($r.Helpers)``) - no publisher, no subscriber.")
}
[void]$sb.AppendLine()
[void]$sb.AppendLine("These are dead declarations. Either delete the event + helper, or document why they are preserved for forward compatibility (e.g. ``OnTartarianHourChanged`` is referenced as a planned hook by Moon 1 day-cycle smoke tests - has a ``FireTartarianHourChanged`` helper but nothing calls it). ``OnMoonUnlocked`` and ``OnDialogueStateChanged`` are typed-modern variants for which the legacy events ``OnMoonCleared`` / direct DialogueManager calls still carry traffic.")
[void]$sb.AppendLine()

[void]$sb.AppendLine("## Constraints + provenance")
[void]$sb.AppendLine()
[void]$sb.AppendLine("- Docs-only audit. No source files modified.")
[void]$sb.AppendLine("- Generator script: ``scripts/audits/sprint11_l9_gameevents_pairs.ps1`` + ``scripts/audits/sprint11_l9_render_markdown.ps1``. Re-run from this worktree to refresh.")
[void]$sb.AppendLine("- All file:line citations are from ``git ls-files`` at branch ``agent/audit/gameevents-pairs`` head.")
[void]$sb.AppendLine()
[void]$sb.AppendLine("---")
[void]$sb.AppendLine("*Sprint 11 Lane 9 - 2026-06-02*")

$outFull = Join-Path $RepoRoot $OutPath
[System.IO.File]::WriteAllText($outFull, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $OutPath ($((Get-Item $outFull).Length) bytes)"
