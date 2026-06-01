# TARTARIA — Logitech F310 Gamepad Reference

> **Last verified:** 2026-05-31 by NATRIX. F310 is the primary tested controller for development.

The F310 has a physical X/D mode switch on its back:

- **X mode (recommended)** — reports as XInput "Xbox Controller" (Windows native).
  - Unity Input System sees it as `Gamepad.current` of type `XInputController`.
  - This is the path the production input handler binds to.
- **D mode (legacy DirectInput)** — reports as a HID Joystick.
  - `LogitechControllerSupport.cs` registers a custom layout matcher so it shows up as a `Gamepad`.
  - All bindings still resolve, but only when the matcher fires before the device is enumerated.

---

## Button map (X mode — XInput identifiers)

The Input System normalizes button names across XInput / DirectInput / Bluetooth.

| F310 Physical | Input System Name           | Game Action                                       | Notes                              |
|---------------|------------------------------|---------------------------------------------------|------------------------------------|
| Left Stick    | `leftStick`                  | Movement                                          | Camera-relative; deadzone 0.15     |
| Right Stick   | `rightStick`                  | Camera orbit (yaw + pitch)                        | Deadzone 0.08                      |
| A (south)     | `buttonSouth`                 | Interact (Exploration) / Resonance Pulse (Combat) | Edge-triggered                     |
| B (east)      | `buttonEast`                  | Scan / Cancel                                     | Edge-triggered                     |
| X (west)      | `buttonWest`                  | Resonance Pulse / Interact (alt)                  | Edge-triggered                     |
| Y (north)     | `buttonNorth`                 | Aether Vision toggle                              | Edge-triggered                     |
| LB            | `leftShoulder`                | Sprint (hold)                                     | Held = sprint                      |
| RB            | `rightShoulder`               | Harmonic Strike (Combat)                          | Edge-triggered                     |
| LT (analog)   | `leftTrigger`                 | Frequency Shield (Combat)                         | Hold > 0.5 threshold               |
| RT (analog)   | `rightTrigger`                | Sprint (alt hold)                                 | > 0.5 = sprint                     |
| Start         | `startButton`                 | Pause menu                                        | Edge-triggered                     |
| Back / Select | `selectButton`                | Aether Vision (alt)                               | Edge-triggered                     |
| D-Pad ←       | `dpad.left`                   | Frequency adjust −                                | Active in Tuning/Combat            |
| D-Pad →       | `dpad.right`                  | Frequency adjust +                                | Active in Tuning/Combat            |
| D-Pad ↑       | `dpad.up`                     | Scan                                              | Active in Exploration/Combat       |
| D-Pad ↓       | `dpad.down`                   | Crouch / Cancel                                   | Reserved (future crouch)           |
| L3 click      | `leftStickButton`             | Sprint toggle                                     | Edge-triggered                     |
| R3 click      | `rightStickButton`            | Recenter camera                                   | Handled in `CameraController`      |

---

## Keyboard parity

Every gamepad button has a keyboard equivalent so a missing controller never blocks play.

| Keyboard          | Game Action                  |
|--------------------|------------------------------|
| WASD / Arrows     | Movement                     |
| E                  | Interact / Resonance Pulse   |
| Space              | Resonance Pulse (Combat)     |
| F                  | Harmonic Strike (Combat)     |
| R                  | Frequency Shield (Combat)    |
| G                  | Scan                         |
| Tab                | Aether Vision toggle         |
| Esc                | Pause menu                   |
| L-Shift            | Sprint (hold)                |
| ← / → (or A/D)    | Frequency adjust             |
| Y                  | Giant Mode (Moon 2+)         |

---

## Verification path

1. Plug F310 into a USB port. X-switch position recommended.
2. Open `Echohaven_VerticalSlice.unity` and hit Play.
3. The `InputProbeHUD` overlay (top-left of Game view) reports:
   - `Keyboard.current` ≠ NULL
   - `Gamepad.current (XInput)` ≠ NULL (will read `Xbox Controller` in X-mode)
   - `Devices total: 3` (keyboard + mouse + gamepad)
   - `Focus: True`
4. Press any button in the table above. The `Last key/btn:` field should update.

If `Focus` flips to `False` mid-play (e.g., the Windows weather widget steals it),
`PlayerInputHandler.Awake()` sets `Application.runInBackground = true` and
`InputSettings.BackgroundBehavior = IgnoreFocus`, so input continues flowing.

---

## Where this is implemented

| File                                                       | Role                                                      |
|------------------------------------------------------------|-----------------------------------------------------------|
| `Assets/_Project/Scripts/Input/PlayerInputHandler.cs`       | All button → action wiring (focus fix + gamepad fallback) |
| `Assets/_Project/Scripts/Input/LogitechControllerSupport.cs` | F310 X/D-mode HID layout matchers                         |
| `Assets/_Project/Scripts/Input/InputProbeHUD.cs`            | Top-left runtime overlay confirming device state           |
| `Assets/_Project/Scripts/Camera/CameraController.cs`        | Right-stick orbit + R3 recenter                            |

---

*If you need to remap any binding, edit `PlayerInputHandler.HandleGamepadButtonFallbacks()` — every button has a real implementation per the CLAUDE.md no-stubs mandate.*
