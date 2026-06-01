# Playtest 2026-06-02 — WASD movement debug

## Symptom
WASD keys produced rotated motion in the Moon 1 Echohaven scene. Pressing W strafed right instead of moving forward; A moved forward; etc. Reproduced in three sessions across Editor Play mode.

## Root cause
[Assets/_Project/Scripts/Input/PlayerInputHandler.cs](../../Assets/_Project/Scripts/Input/PlayerInputHandler.cs#L508), pre-fix line 508:

```csharp
Vector3 move = new Vector3(_moveInput.y, 0, -_moveInput.x); // Fixed: Joystick was rotated 90° CW
```

Both keyboard fallback (`HandleMovementInput` lines 487–498) and `Gamepad.leftStick.ReadValue()` return Vector2 with `x = horizontal, y = vertical`. The canonical Unity world-space mapping is `(x, 0, y)` so that `W → +z` (forward) and `D → +x` (strafe-right). The `(y, 0, -x)` form rotated the input 90° CW.

The `// Fixed: Joystick was rotated 90° CW` comment is the smoking gun — a previous fix attempted to compensate for a downstream camera-relative transform that was *also* swapping axes. Once that downstream transform was repaired (history not surfaced in current grep), the input mapping became wrong but the workaround stayed.

## Fix
Single-line replacement of line 508 with the canonical mapping, plus an explanatory comment.

## Validation plan (Cowork drives)
1. `Tartaria → 9 QA → Open Test_PlayerOnly` (from this sprint's QA lane)
2. Play
3. W → forward, S → backward, A → strafe-left, D → strafe-right
4. Gamepad left stick mirrors WASD
5. Camera-relative motion still rotates correctly when player faces non-default heading

## Out of scope
- Joystick deadzone tuning
- Diagonal speed normalization (already handled by `move = move.normalized` on line 510)
- Sprint/gravity behavior (untouched)
