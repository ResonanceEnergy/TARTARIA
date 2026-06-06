# HANDOFFS

Top of file = most recent. Append entries with timestamp + lane + status.

---

## 2026-06-02 — UI (main-menu-scene) Sprint 6 Lane 1 — DONE / 2 follow-ups

**Delivered:**
- `Assets/_Project/Scripts/UI/MainMenuController.cs` — Canvas-based controller with 5 wired buttons (New Game / Continue / Settings / Credits / Quit), title + subtitle + version labels.
- `Assets/_Project/Scripts/Editor/BuildMainMenuScene.cs` — `Tartaria/UI/Build Main Menu Scene` menu that scaffolds `Assets/_Project/Scenes/MainMenu.unity` (Canvas, EventSystem + InputSystemUIInputModule, UI camera, all 5 buttons wired via SerializedObject) and registers the scene at index 0 of EditorBuildSettings.scenes.

**API verified before write:**
- `Continue` reflection-calls `Tartaria.Save.SaveManager.QuickLoad()` (instance method at `Assets/_Project/Scripts/Save/SaveManager.cs:246`). **There is NO `LoadSlot(int)` method on SaveManager** (grep returned 0 hits) — the lane prompt assumed a non-existent API. QuickLoad is the canonical reload path; this is the closest equivalent and is what was implemented. If a true multi-slot UI lands later, swap reflection target to that.
- `Settings` calls `Tartaria.UI.SettingsOverlay.Open()` at `Assets/_Project/Scripts/UI/SettingsOverlay.cs:104`. The existing `PauseMenu.cs:13-19` is a stub (the entire IMGUI pause overlay lives in `PauseAndGameOverMenu`), so there is **no separate Settings Panel prefab** to instantiate. SettingsOverlay is a static IMGUI panel and is the actual reusable settings UI.

**Hand-offs for Director / sibling lanes:**
1. **Credits.unity scene** — `Credits` button calls `SceneManager.LoadScene("Credits")`. A sibling lane must build that scene + add it to Build Settings. If unavailable at runtime, controller catches and logs the missing-scene path; player isn't crashed.
2. **Settings panel prefab extraction (optional polish)** — current implementation calls the static `SettingsOverlay.Open()`, which works but means the settings UI is an IMGUI overlay drawn on top, not a Canvas panel parented under MainMenuCanvas. If a future sprint wants a proper Canvas-parent settings panel, extract `SettingsOverlay` into a prefab and wire it as a child of the menu canvas; the discovery helper `MainMenuController.IsSettingsPanelAvailable` can switch detection target without touching the button handler.

**No-debt / API-contract checks passed:**
- No `using Tartaria.Core.Time`, no banned namespace shadow.
- No `FindObjectOfType` (Unity 6 deprecated).
- Every `catch` logs `file:lineHint` + exception type + message + stack.
- No stubs, no TODOs, no empty bodies.
- `Application.Quit()` inside `#if UNITY_EDITOR EditorApplication.isPlaying = false` per spec.
- No invented `GameEvents.*` calls (this lane does not subscribe to any GameEvents).

---

## 2026-06-02 — GAMEPLAY (playerinput-movement-debug) — RESOLVED IN-LANE

**Bug:** WASD did not move the player forward/back the way input dictated. Pressing W produced strafe-right motion; A produced strafe-forward; etc. Joystick reportedly looked fine post a prior "Joystick was rotated 90° CW" fix.

**Hypothesis:** [Assets/_Project/Scripts/Input/PlayerInputHandler.cs](Assets/_Project/Scripts/Input/PlayerInputHandler.cs#L508) maps `_moveInput` to world-space as `new Vector3(_moveInput.y, 0, -_moveInput.x)`. Both keyboard fallback and `Gamepad.leftStick.ReadValue()` return Vector2 in `(x=horizontal, y=vertical)` form, so the canonical mapping is `(x, 0, y)`. The 90°-rotation comment from the earlier fix likely papered over a downstream camera-relative transform that was *also* swapping axes; once that downstream path was corrected, the input mapping became wrong.

**Fix (committed in this lane):** Restore canonical `new Vector3(_moveInput.x, 0, _moveInput.y)`. Single-line edit, no other behavior change.

**Verification (Cowork required):** Play `Test_PlayerOnly` scene (Tartaria → 9 QA), press W → forward; A → strafe-left; D → strafe-right; S → backward. Same for gamepad left stick.

---
