# F310 QUICK FIX — Controller Not Working

## Problem
F310 controller not detected or buttons don't work in TARTARIA.

## Solution (30 seconds)

### 1. Check Physical Switch
- **Location:** Back of F310 controller, near USB cable
- **Current:** Probably in "D" position
- **Needed:** "X" position

```
Back of F310:
  USB
   ║
   ║
[D│X]  ← Slide this switch to X
```

### 2. Unplug & Replug
1. Unplug USB cable from PC
2. Wait 3 seconds  
3. Plug back in
4. Windows will show "Installing Xbox 360 Controller"

### 3. Verify
Press `Win+R` → Type `joy.cpl` → Enter

Should show:
```
Controller (Xbox 360 For Windows)
```

NOT:
```
Logitech RumblePad 2 USB  (this = D mode, wrong!)
```

## Why X Mode?
- **D (DirectInput):** Old PC games, varies by game
- **X (XInput):** Xbox controller emulation, Unity standard

TARTARIA uses Unity Input System → requires XInput → switch to X.

## Still Not Working?
1. Try different USB port (USB 3.0 blue port if available)
2. Check Windows Game Controllers: `joy.cpl` → Test buttons
3. Restart TARTARIA after switching mode

## References
- Full guide: `CONTROLLER_SETUP.md`
- Code: `Assets/_Project/Scripts/Input/LogitechControllerSupport.cs`
- Unity Input System requires Gamepad class (XInput provides this)
