# Ticket 025: Day-change drives HUD objective text

**Destination file**: `Assets/_Project/Scripts/Integration/DayChangeObjectiveHUD.cs`
**Change type**: new file

## Spec
Per docs/15 §17th-hour pacing. MonoBehaviour subscribes to `GameEvents.OnDayChanged` (OnEnable/OnDisable). When the day reaches the configurable `seventeenthHourDay` (serialized int, default 17), call `GameEvents.RaiseHUDShowObjective` with the prophecy objective string. For earlier days, show a generic "restore the village" objective once.

## Grep-before-write checklist
- `GameEvents.RaiseDayChanged` exists in `Assets/_Project/Scripts/Core/GameEvents.cs` (confirmed).
- `GameEvents.RaiseHUDShowObjective` exists in same file (confirmed).

## Output format
Single fenced csharp block, full file, first line `// File: Assets/_Project/Scripts/Integration/DayChangeObjectiveHUD.cs`. No prose.
