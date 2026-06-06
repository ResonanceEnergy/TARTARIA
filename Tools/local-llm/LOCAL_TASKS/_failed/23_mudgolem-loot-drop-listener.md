# Ticket 023: Mud Golem loot drop on death

**Destination file**: `Assets/_Project/Scripts/Integration/MudGolemLootDropListener.cs`
**Change type**: new file

## Spec
Per docs/15 §combat polish ("real loot drops"). Create a MonoBehaviour that subscribes to `GameEvents.OnEnemyKilled` in OnEnable and unsubscribes in OnDisable. When a Mud Golem dies, instantiate a Resources-loaded pickup prefab at the death position and call `GameEvents.RaiseItemPickup` with a Resonance Shard. Guard against null prefab. No primitives.

## Grep-before-write checklist
- `GameEvents.RaiseEnemyKilled` exists in `Assets/_Project/Scripts/Core/GameEvents.cs` (confirmed via grep Raise[A-Z]\w+).
- `GameEvents.RaiseItemPickup` exists in same file (confirmed).
- Load pickup via `Resources.Load<GameObject>` only; if null, log warning and return (no CreatePrimitive).

## Output format
Output a single fenced csharp code block, full file, first line `// File: Assets/_Project/Scripts/Integration/MudGolemLootDropListener.cs`. No prose.
