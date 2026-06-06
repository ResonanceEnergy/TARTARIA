# Ticket 024: Building restored audio+banner stinger

**Destination file**: `Assets/_Project/Scripts/Integration/BuildingRestoredStinger.cs`
**Change type**: new file

## Spec
Per docs/15 §audio ("restoration stinger"). MonoBehaviour subscribes to `GameEvents.OnBuildingRestored` (OnEnable/OnDisable pairing). On restore: play a one-shot AudioClip via a cached AudioSource and call `GameEvents.RaiseHUDShowBanner` with the building name. Serialize the AudioClip field; null-guard before PlayOneShot.

## Grep-before-write checklist
- `GameEvents.RaiseBuildingRestored` exists in `Assets/_Project/Scripts/Core/GameEvents.cs` (confirmed).
- `GameEvents.RaiseHUDShowBanner` exists in same file (confirmed).

## Output format
Single fenced csharp block, full file, first line `// File: Assets/_Project/Scripts/Integration/BuildingRestoredStinger.cs`. No prose.
