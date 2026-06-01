# Input Deep Dive — 2026-05-31

NATRIX reported: **no input on keyboard or controller** despite the BYPASS DRIVER overlay reading `KB OK`, `PAD OK (Xbox Controller)`, `Focus True`, `CC 'Player'`.

This document captures the deep-dive findings (Unity manual + code audit) and the patches that landed in this session.

## Smoking-gun symptoms

The overlay in screenshots showed:

```
[NUCLEAR BYPASS DRIVER v2]
Frame 1  Focus True  TS 1.0
CC 'Player'  pos (0,2.0,-100.9)
KB OK  PAD OK (Xbox Controller)
Stick (0.00, 0.00)
RbKilled=True CompKilled=True TSForced=False
f1 mv(0.00,0.00) sp=False Int=False
```

`Frame 1` + `f1` never advanced across 3+ minute span. `pos.y` did not fall under gravity. `Last key/btn (none) 41.9s ago` from the InputProbeHUD also never updated. This is the signature of one of three things:

1. **Editor is Paused** — Update + OnGUI both stop ticking, last rendered frame stays on screen
2. **Game view is not the focused tab in the Editor** — Play Focused mode gates input there
3. **SimplePlayerDriver.Update threw an uncaught exception on frame 1** and was disabled

## Top 5 root causes (per parallel agent audit)

### From repo audit
1. **Race: SimplePlayerDriver.Start vs PlayerSpawner.Start** — both run in `RuntimeInitializeLoadType.AfterSceneLoad → Start()` on the same frame. If SimplePlayerDriver wins, it has nothing to acquire. Old code cached null forever; new code re-acquired only on frames 1–120.
2. **Once `_cc` was non-null (even pointing at the wrong CC like ResetScout_Q at z=-55), the driver never re-targeted.**
3. PlayerInputHandler is added by PlayerSpawner to the spawned Player — SimplePlayerDriver only disabled it on whatever CC it grabbed FIRST.
4. **Eight files set `Time.timeScale = 0`** (DeathOverlay, UIManager, GameCompleteOverlay, DialogueChoiceOverlay, InventoryUI, PauseAndGameOverMenu × 2 sites). Any of them auto-firing on scene-load would freeze Update.
5. PlayerInputHandler's `EnsureSafetyFloor()` disables CC briefly during Awake — race window.

### From Unity manual / forum research
1. **Game view "Play Focused" toggle desync (active Unity bug)** — issuetracker: "Game view Focused toggle not functioning when entering Play mode" + "Game stops accepting input when Game view undocked/redocked during Play Mode using Input System". Symptom: Application.isFocused returns True but Game view never gets keystrokes.
2. **EventSystem holding a UI Selectable** — InputSystemUIInputModule routes through UI first. A leftover canvas can swallow keys before SimplePlayerDriver sees them.
3. **InputSettings.BackgroundBehavior = IgnoreFocus is silently overridden by per-device `canRunInBackground = false`** on XInput on Windows.
4. **Input System v1.9 partial-fix regression** — Keyboard.current isPressed false on same frame as wasPressedThisFrame true. Common when gating on `(isPressed && wasPressedThisFrame)`.
5. **Stray `UnityEngine.Input.*` call under Input System Package mode** — throws `InvalidOperationException` and kills Update silently. CLAUDE.md already warns about this.

## Patches landed this session

### `Assets/_Project/Scripts/Input/SimplePlayerDriver.cs` → v3

- **Re-acquire every frame** when `_cc` is null OR the cached CC isn't the Player-tagged GameObject (race-condition fix; no more permanent latch on ResetScout_Q).
- **`EventSystem.current.SetSelectedGameObject(null)` in Start** — addresses cause #2 from web research. Logs a warning naming the offending Selectable before clearing.
- **Heartbeat `Debug.Log` every 60 frames** — `[SimplePlayerDriver] HEARTBEAT f={n} ts={ts} focus={f} cc={pos} pad={name} stick=({x},{y}) kbAny={bool}` — visible in Console even when OnGUI overlay is frozen. **This is the diagnostic that distinguishes Pause from input-block from hang.**
- **Overlay v3 layout** — now shows `DriverFrame` AND `EngineFrame` (Time.frameCount) on separate lines, plus `Time.realtimeSinceStartup` and `AnyKey={Keyboard.current.anyKey.isPressed}`. If DriverFrame and EngineFrame both advance but `AnyKey` stays false even with keys pressed → Game-view focus issue. If both freeze → Pause. If EngineFrame advances but DriverFrame freezes → driver Update threw.

### `Assets/_Project/Scripts/Editor/GameViewFocusFix.cs` → new

- Auto-fires on `PlayModeStateChange.EnteredPlayMode` via `[InitializeOnLoad]`
- Reflects into `UnityEditor.GameView` (internal type), calls `Focus()` + `Repaint()`
- Manual menu: **`Tartaria → 9 Debug → Force Focus Game View`**
- Directly mitigates the #1 ranked web-research root cause (Play Focused desync)

### `Assets/_Project/Scripts/Integration/Moon1InnRestTrigger.cs:35`

- `AddComponent<Renderer>()` → `AddComponent<MeshRenderer>()` (Renderer is abstract — was throwing at runtime)

### `Assets/_Project/Scripts/Integration/Moon1VillagerAmbient.cs`

- Added `_hasIsWalking` Awake-time parameter cache + `SetWalking()` guarded helper
- Eliminates 8× "Parameter 'IsWalking' does not exist" console spam

## Runbook for NATRIX

When you next hit Play, look at the Console FIRST, not the screen. The new heartbeat log will tell you which failure mode you're in:

| What Console shows | Meaning | Fix |
|---|---|---|
| No `[SimplePlayerDriver] HEARTBEAT` logs at all | Editor is **paused** (or Update never started) | Click the Pause button next to Play to un-pause; verify Play Focused toggle is ON |
| Heartbeats every ~1s, `kbAny=False` even when pressing keys | Game view doesn't have focus | Click Game viewport, or run `Tartaria → 9 Debug → Force Focus Game View` |
| Heartbeats fire, `kbAny=True` when pressing W, but `cc` position doesn't change | CharacterController is wedged | Look at `cc=` y-coord — if falling, you're outside terrain; if stuck, check `_cc.isGrounded` against geometry |
| Heartbeats fire, stick `(x,y)` non-zero when moving F310, but `cc` unchanged | Same wedge as above | Same fix |

The most likely scenario based on the screenshots: **Pause was active** (one of the eight `Time.timeScale = 0` callers fired on scene-load) and/or **Game view lost focus to the weather widget**. The new GameViewFocusFix script will fire automatically on EnteredPlayMode and should keep focus on the Game viewport.

## Open follow-ups

- If the heartbeats reveal "Game view focus" is the blocker, consider whether to also disable "Play Focused" mode in Unity preferences project-wide.
- The eight `Time.timeScale = 0` sites in UI should be reviewed for any that fire pre-spawn (e.g., a DialogueChoiceOverlay or PauseAndGameOverMenu auto-showing as part of scene-load).
