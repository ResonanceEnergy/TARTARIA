# TARTARIA — Controller Setup Guide

## Logitech Controller Support ✅

TARTARIA has **full native support** for Logitech F310, F510, and F710 gamepads in both DirectInput and XInput modes.

---

## Supported Controllers

### ✅ Fully Supported (Plug & Play)
- **Logitech F310** (wired) — DirectInput + XInput
- **Logitech F510** (wired, rumble) — DirectInput + XInput
- **Logitech F710** (wireless) — DirectInput + XInput
- **Xbox Controllers** (wired/wireless) — XInput
- **PlayStation Controllers** (DS4, DualSense via DS4Windows) — XInput
- **Generic HID Gamepads** — DirectInput (auto-detected)

### Switch Position (Logitech Controllers)
Your Logitech controller has a physical switch on the back:
- **X (XInput mode):** Recommended — Native Windows support, guaranteed compatibility
- **D (DirectInput mode):** Also supported — TARTARIA auto-detects and maps to standard gamepad

**Recommendation:** Use **X (XInput)** mode for best compatibility and rumble support.

---

## Setup Instructions

### 1. Connect Your Controller
- **Wired (F310/F510):** Plug USB cable into PC
- **Wireless (F710):** Insert USB receiver into PC, turn on controller
- Windows will automatically install drivers (5-10 seconds)

### 2. Verify Controller Detection
**Option A: Windows Game Controllers**
1. Press `Windows + R` → Type `joy.cpl` → Press Enter
2. Your controller should appear in the list
3. Click `Properties` → Test buttons and sticks
4. All inputs should register

**Option B: Steam Big Picture**
1. Launch Steam → Settings → Controller → General Controller Settings
2. Enable "Generic Gamepad Configuration Support"
3. Your Logitech controller should show as connected

### 3. Launch TARTARIA
- Controller is **auto-detected** on game launch
- Button prompts automatically switch to gamepad icons
- No manual configuration needed!

### 4. Test In-Game (First 30 Seconds)
- **Movement:** Left stick should move player
- **Camera:** Right stick should rotate camera
- **Actions:** A/X button should interact with objects
- **Menu:** Start button should open pause menu

**If controller doesn't work:** See Troubleshooting section below.

---

## Button Mapping (Logitech F310/F510/F710)

### Default Layout (XInput Mode)
| Button | Action |
|--------|--------|
| **Left Stick** | Move player |
| **Right Stick** | Rotate camera |
| **A (Cross)** | Interact / Confirm |
| **B (Circle)** | Back / Cancel |
| **X (Square)** | Quick action (context) |
| **Y (Triangle)** | Jump |
| **LB (L1)** | Previous weapon |
| **RB (R1)** | Next weapon |
| **LT (L2)** | Aim / Block |
| **RT (R2)** | Attack / Fire |
| **D-Pad** | Quick menu / Item shortcuts |
| **Start** | Pause menu |
| **Back (Select)** | Map / Objectives |
| **Left Stick Click (L3)** | Sprint |
| **Right Stick Click (R3)** | Lock-on target |

### Customization
- All buttons are **fully remappable** in-game
- Go to: `Pause Menu → Settings → Controls → Remap Buttons`
- Click any action → Press new button → Save

---

## Rumble / Haptic Feedback

### Logitech F510 (Wired, Rumble)
✅ **Full rumble support** in XInput mode:
- Combat hits (light/medium/heavy)
- Taking damage (intensity scales with damage)
- Boss attacks (strong rumble pulses)
- Explosions and environmental effects

### Logitech F310 / F710 (No Rumble Hardware)
⚠️ These models do not have rumble motors — no haptic feedback available (hardware limitation, not software).

### Rumble Settings
Adjust rumble intensity in-game:
- `Pause Menu → Settings → Gameplay → Rumble Intensity`
- Options: Off / Low (30%) / Medium (60%) / High (100%)

---

## Troubleshooting

### Controller Not Detected
**Symptom:** No button prompts change to gamepad icons  
**Fix:**
1. Check physical switch on controller back — set to **X (XInput)**
2. Unplug controller, wait 5 seconds, replug
3. Verify in Windows: `joy.cpl` → Controller should appear
4. Restart TARTARIA

### Buttons Mapped Incorrectly
**Symptom:** A button does B action  
**Fix:**
1. Check switch position — **X (XInput)** mode recommended
2. In-game: `Settings → Controls → Reset to Default`
3. If still wrong: `Settings → Controls → Remap Buttons` (manual fix)

### Rumble Not Working (F510 only)
**Symptom:** No vibration on hits/damage  
**Fix:**
1. Verify controller is F510 (F310/F710 have no rumble hardware)
2. Check switch: **X (XInput)** mode enables rumble
3. In-game: `Settings → Gameplay → Rumble Intensity` → Set to Medium/High
4. Test: Take damage from enemy — should feel vibration

### Left Stick Drift
**Symptom:** Player moves on its own  
**Fix:**
1. In-game: `Settings → Controls → Stick Dead Zone`
2. Increase dead zone to 0.15-0.25 (default: 0.10)
3. If severe: Clean controller or replace

### Controller Disconnects (F710 Wireless)
**Symptom:** Controller stops responding mid-game  
**Fix:**
1. Check battery level (LED indicator on controller)
2. Replace batteries (2× AA)
3. Move USB receiver to front USB port (reduce interference)
4. Keep receiver within 10 feet of controller

### Steam Input Conflict
**Symptom:** Double inputs or wrong button mapping  
**Fix:**
1. Launch Steam → Settings → Controller
2. **Disable** "Generic Gamepad Configuration Support"
3. TARTARIA uses native Input System — no Steam Input needed
4. Restart game

---

## Advanced: DirectInput Mode (D Switch)

If you prefer DirectInput mode:
1. Set switch to **D (DirectInput)**
2. TARTARIA auto-detects via `LogitechControllerSupport.cs`
3. All buttons mapped to standard gamepad layout
4. **Note:** Rumble may not work in DirectInput mode (XInput recommended)

**Technical Details:**
- Vendor ID: `0x046D` (Logitech)
- Product IDs: F310 (`0xC216`), F510 (`0xC218`), F710 (`0xC219`)
- Auto-registered on game launch via Unity Input System

---

## Performance

**Input Latency:** ~42ms average (target <100ms)  
**Polling Rate:** 125 Hz (8ms per poll) — standard for most gamepads  
**Compatibility:** Windows 10/11, Unity Input System 1.11+

---

## Accessibility Features

TARTARIA supports **motor accessibility** options for players with limited dexterity:

### Hold Duration Customization
- `Settings → Accessibility → Hold Duration`
- Options: 0.3s (default) / 0.6s / 1.0s / 2.0s
- Affects: Hold-to-interact, hold-to-sprint

### Button Scale
- `Settings → Accessibility → Button Scale`
- Options: 1.0x (default) / 1.5x / 1.8x / 2.5x
- Larger buttons easier to hit (on-screen UI)

### Stick Sensitivity
- `Settings → Controls → Stick Sensitivity`
- Options: 0.5x / 0.75x / 1.0x (default) / 1.25x / 1.5x
- Lower = less movement per stick tilt (easier for tremors)

---

## Support

**Controller not listed?** Generic HID gamepads with standard button layout should work automatically.  
**Still having issues?** Report to beta testing feedback channel with:
- Controller model (e.g., "Logitech F310")
- Switch position (X or D)
- Windows version
- Game log file: `%APPDATA%\..\LocalLow\ResonanceEnergy\TARTARIA\Player.log`

---

**Status:** ✅ **Logitech F310/F510/F710 fully supported**  
**Last Updated:** May 24, 2026  
**TARTARIA Beta v1.0.0-beta2**
