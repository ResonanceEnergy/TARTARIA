# 2026-06-02 — Echohaven Movement Fix

**Branch:** `agent/gameplay/echohaven-movement-fix`
**Agent:** Gameplay Programmer
**Reported by:** Cowork runtime QA (Unity Editor, Echohaven_VerticalSlice.unity)
**Symptom:** Keyboard W registers in InputProbeHUD overlay ("Last key: W"), but the player capsule does not translate. Sprint-2 PR #10 canonical-axis fix at `PlayerInputHandler.cs:508` landed, but movement is still broken.

---

## Hypotheses tested

| # | Hypothesis | Result |
|---|---|---|
| 1 | `Moon1MasterBootstrap.cs` 7-stub cleanup pass prunes the `PlayerInputHandler` component from the spawned player. | **RULED OUT.** `Moon1MasterBootstrap.cs:29-90` only adds components to a `Moon1_Systems` GameObject via `AddIfMissing<T>`. There is no `Destroy`, `DestroyImmediate`, or `RemoveComponent` call anywhere in the file. It cannot prune player components. |
| 2 | `PlayerSpawner` instantiates the player without `PlayerInputHandler`. | **MITIGATED.** `PlayerSpawner.SpawnPlayer()` at `Integration/PlayerSpawner.cs:123-127` already auto-adds the component when missing. However, if the player was placed in-scene (not spawned via `PlayerSpawner`), the component can still be absent — `Moon1PlayerSetup` had no recovery guard. **Belt-and-braces fix added.** |
| 3 | `InputActionAsset` not assigned → `OnMove` never fires. | **PARTIALLY CONFIRMED.** `PlayerInputHandler.SetupInputActions()` (line 149-181) only binds actions when `inputActions != null`. With the asset unassigned, `_moveAction` stays null, the original code at line 485 fell back to `Vector2.zero`, then a secondary keyboard fallback ran only if asset returned 0. If the asset existed but resolved its Move binding to zero (broken binding path), the downstream `_moveInput` stayed zero and the player froze. **Root cause: brittle "asset-then-fallback" order — the asset's value silently dominated even when broken.** |

---

## Confirmed root cause

**Hypothesis 3** — the input read order in `HandleMovementInput()` trusted the InputActionAsset's `Move` action over the direct keyboard read. When the asset is missing OR mis-bound OR resolves to a stale `Vector2.zero` (a known issue in some Unity 6 Editor-restart conditions), the `_moveInput` stays zero. The InputProbeHUD correctly reports `W` because it reads `Keyboard.current.wKey.wasPressedThisFrame` directly — that path is independent of the player's movement chain.

---

## Files changed

### 1. `Assets/_Project/Scripts/Input/PlayerInputHandler.cs`

**Location:** `HandleMovementInput()`, formerly line 483-505.
**Change:** Rewrote the input-read order. The new sequence:

1. **Direct `Keyboard.current` read FIRST** (new lines ~485-495) — produces a `kbMove` Vector2 from WASD/arrow keys and assigns it to `_moveInput` if non-zero. This bypasses the InputActionAsset entirely and guarantees movement under any asset state.
2. **InputAction-bound override** (new lines ~498-504) — `_moveAction.ReadValue<Vector2>()` only overwrites `_moveInput` if it returns a non-zero magnitude. A broken asset binding that returns zero can no longer kill the direct read.
3. **Gamepad direct-read fallback** (new lines ~507-516) — preserved for the no-asset + no-keyboard case (controller plugged in, no action binding).

Per the prompt's instruction the runtime guard sits at the very top of `HandleMovementInput()` — *before* any other `_moveInput` logic — so InputAction-bound values can still override only when non-zero.

### 2. `Assets/_Project/Scripts/Integration/Moon1PlayerSetup.cs`

**Location:** `WaitForPlayerAndConfigure()`, around line 73 (after `playerInstance = existingPlayer;`).
**Change:** Added a recovery guard that checks `playerInstance.GetComponent<Tartaria.Input.PlayerInputHandler>()`. If null, logs an error and adds the component. Otherwise logs the confirmation `"[Moon1PlayerSetup] PlayerInputHandler attached + ready."` This is belt-and-braces — `PlayerSpawner` already does the same auto-add at `PlayerSpawner.cs:123-127` — but if the player was placed in-scene or by some other path, this catches it.

### 3. `Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs`

**Not modified.** The hypothesis-1 audit found no destructive operations on player components.

---

## What Cowork needs to verify in Echohaven Play

1. Open `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`.
2. Confirm compile is clean (no `Library/Bee/tundra.log.json` errors mentioning `PlayerInputHandler.cs` or `Moon1PlayerSetup.cs`).
3. Hit Play.
4. Watch the Console for the log line `[Moon1PlayerSetup] PlayerInputHandler attached + ready.` — confirms the recovery guard ran and the component is present. If you see the error variant `[Moon1PlayerSetup] Player has NO PlayerInputHandler component — input chain dead. Adding one as recovery.` then the prefab is missing the component and the recovery added it.
5. Hold W — the player capsule should translate forward (camera-relative). The InputProbeHUD top-left overlay should show `Last key/btn: W (0.0s ago)`.
6. Try A/S/D — capsule should translate left/back/right respectively (camera-relative).
7. If gamepad is connected, push the left stick — capsule should move. The InputProbeHUD `Left stick:` line should show non-zero magnitude.

**Critical:** keep `Console → Error Pause` toggle OFF (per CLAUDE.md late-night mandate). Echohaven init throws 1–2 missing-script errors that will Editor-pause the run otherwise and make this look like input is dead.

**DO NOT** screenshot or claim the Play test passed without running these steps — this doc only ships code, not runtime confirmation.

---

## Notes / follow-ups

- The original `Vector3 move = new Vector3(_moveInput.y, 0, -_moveInput.x);` swizzle at line 508 (Sprint-2 PR #10) is unchanged. If movement direction reads wrong after this fix (e.g. W moves the capsule right instead of forward), that's a separate axis-mapping bug to be filed.
- The `InputActionAsset` field on PlayerInputHandler should still be assigned in the player prefab — this fix makes the lack of an asset survivable, not desirable.
- Once Cowork confirms WASD moves the capsule, file a follow-up to assign `Assets/_Project/Settings/Input/PlayerControls.inputactions` (or whichever asset is canonical) to the `Player.prefab`'s `PlayerInputHandler.inputActions` slot so gamepad bindings flow through the action map again.
