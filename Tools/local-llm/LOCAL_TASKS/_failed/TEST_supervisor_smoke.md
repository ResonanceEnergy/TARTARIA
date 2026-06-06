# Ticket TEST_supervisor_smoke: trivial spawn-position helper

**Destination file**: `Assets/_Project/Scripts/Integration/SupervisorSmokeTest.cs`
**Change type**: new file

## Spec

Write a trivial MonoBehaviour `SupervisorSmokeTest` in namespace `Tartaria.Integration`. It has a public `Vector3 GetTestPosition()` method that returns `new Vector3(0f, 1f, 0f)`. Add a public field `int testRunCount = 0` that increments in `Awake()`. Log `"[SupervisorSmokeTest] Initialized"` in `Awake()`.

This is a smoke test for the supervisor — proves the loop is closed end-to-end (model → file on disk → Gate F auto-commit).

## Grep-before-write checklist

- `Tartaria.Integration` namespace exists — used throughout `Assets/_Project/Scripts/Integration/*.cs`
- `MonoBehaviour` base class — `UnityEngine.MonoBehaviour`

## Output format

Output a single fenced code block starting with `// File: Assets/_Project/Scripts/Integration/SupervisorSmokeTest.cs`. Include `using UnityEngine;` and the `Tartaria.Integration` namespace.
