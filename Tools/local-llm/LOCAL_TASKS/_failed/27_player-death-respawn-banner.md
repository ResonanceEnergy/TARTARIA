# Ticket 027: Player death + respawn banner bridge

**Destination file**: `Assets/_Project/Scripts/Integration/PlayerDeathRespawnBanner.cs`
**Change type**: new file

## Spec
Per docs/15 §combat polish. MonoBehaviour subscribes to `GameEvents.OnPlayerDeath` and `GameEvents.OnPlayerRespawned` (both paired OnEnable/OnDisable). On death, call `GameEvents.RaiseHUDShowBanner` with "You fell..."; on respawn, call `GameEvents.RaiseHUDShowBanner` with "Echohaven endures." Keep both banner strings serialized.

## Grep-before-write checklist
- `GameEvents.RaisePlayerDeath` exists in `Assets/_Project/Scripts/Core/GameEvents.cs` (confirmed).
- `GameEvents.RaisePlayerRespawned` exists in same file (confirmed).
- `GameEvents.RaiseHUDShowBanner` exists in same file (confirmed).

## Output format
Single fenced csharp block, full file, first line `// File: Assets/_Project/Scripts/Integration/PlayerDeathRespawnBanner.cs`. No prose.
