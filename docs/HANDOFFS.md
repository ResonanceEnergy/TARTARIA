# HANDOFFS

Top of file = most recent. Append entries with timestamp + lane + status.

---

## 2026-06-02 — GAMEPLAY (playerinput-movement-debug) — RESOLVED IN-LANE

**Bug:** WASD did not move the player forward/back the way input dictated. Pressing W produced strafe-right motion; A produced strafe-forward; etc. Joystick reportedly looked fine post a prior "Joystick was rotated 90° CW" fix.

**Hypothesis:** [Assets/_Project/Scripts/Input/PlayerInputHandler.cs](Assets/_Project/Scripts/Input/PlayerInputHandler.cs#L508) maps `_moveInput` to world-space as `new Vector3(_moveInput.y, 0, -_moveInput.x)`. Both keyboard fallback and `Gamepad.leftStick.ReadValue()` return Vector2 in `(x=horizontal, y=vertical)` form, so the canonical mapping is `(x, 0, y)`. The 90°-rotation comment from the earlier fix likely papered over a downstream camera-relative transform that was *also* swapping axes; once that downstream path was corrected, the input mapping became wrong.

**Fix (committed in this lane):** Restore canonical `new Vector3(_moveInput.x, 0, _moveInput.y)`. Single-line edit, no other behavior change.

**Verification (Cowork required):** Play `Test_PlayerOnly` scene (Tartaria → 9 QA), press W → forward; A → strafe-left; D → strafe-right; S → backward. Same for gamepad left stick.

---
