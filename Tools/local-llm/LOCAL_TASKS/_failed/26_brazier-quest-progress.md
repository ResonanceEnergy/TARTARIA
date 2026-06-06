# Ticket 026: Brazier ring complete advances quest objective

**Destination file**: `Assets/_Project/Scripts/Integration/BrazierQuestProgressBridge.cs`
**Change type**: new file

## Spec
Per docs/15 §quest tree. MonoBehaviour subscribes to `GameEvents.OnBrazierRingComplete` (OnEnable/OnDisable). On completion, call `GameEvents.RaiseQuestObjectiveProgressed` with quest id "echohaven_awakening" and objective id "light_brazier_ring", then `GameEvents.RaiseHUDShowObjective` to refresh the tracker. Serialize the quest/objective id strings with those defaults.

## Grep-before-write checklist
- `GameEvents.RaiseBrazierRingComplete` exists in `Assets/_Project/Scripts/Core/GameEvents.cs` (confirmed).
- `GameEvents.RaiseQuestObjectiveProgressed` exists in same file (confirmed).
- `GameEvents.RaiseHUDShowObjective` exists in same file (confirmed).

## Output format
Single fenced csharp block, full file, first line `// File: Assets/_Project/Scripts/Integration/BrazierQuestProgressBridge.cs`. No prose.
