# 🎮 GAMEPAD QUICK START

## ⚠️ CRITICAL FIX APPLIED (2026-05-27)

**The bug:** Left stick was opening moon portal menu instead of moving character.

**The fix:** Portal system now **disabled by default**. Left stick moves player normally.

**To enable portal testing (designers only):** Press `Ctrl+Shift+M` in Play mode.

---

## THE #1 ISSUE: F310 Switch Position ⚠️

**Your F310 has a physical switch on the BACK:**

```
[D]----------[X]
OLD MODE    UNITY MODE ← YOU NEED THIS!
```

### Fix Right Now:
1. **Turn controller over**
2. **Find the switch** (between the shoulder buttons)
3. **Slide it to X** (right position)
4. **Unplug USB**
5. **Replug USB**
6. **Test:** `Win+R` → `joy.cpl` → should show "**Controller (Xbox 360 For Windows)**"

---

## The #2 Issue: Game State Stuck

**If joystick doesn't move character, the game might be stuck in Boot/Loading state.**

### In Unity Editor Play Mode:
1. Menu → **Tartaria** → **DIAGNOSE: Input System**
2. Check Console output
3. If it says `Current State: Boot` or `Loading`:
   - Menu → **Tartaria** → **FIX: Force Exploration State**
   - Try moving again

### Quick Test:
- Press **WASD** keys
- If keyboard moves character but gamepad doesn't → **gamepad not detected** (check switch!)
- If keyboard also doesn't move → **game state stuck** (run diagnostic!)

---

## Controls Overview

| Action | Gamepad | Keyboard |
|--------|---------|----------|
| Move | Left Stick | WASD |
| Camera | Right Stick | Mouse |
| Sprint | L3 (click stick) | Left Shift |
| Attack | RT | Space / Left Click |
| Interact | A button | E |
| Scan | B button | G |
| Shield | LT | R |
| Vision | LB | Tab |
| Heavy Attack | Y button | F |

---

## Full Guide

See [GAMEPAD_GUIDE.md](GAMEPAD_GUIDE.md) for complete reference including:
- All button mappings
- Combat flow examples
- Troubleshooting
- Technical details
- Diagnostic tools

---

## Emergency Diagnostics

**In Unity Editor → Menu:**
- `Tartaria` → **DIAGNOSE: Input System** — full system check
- `Tartaria` → **DIAGNOSE: Check Runtime State** — spawners/buildings/NPCs
- `Tartaria` → **FIX: Force Exploration State** — unblock input

**In Windows:**
- `Win+R` → `joy.cpl` — test gamepad detection
- Left stick should show movement in calibration test

---

## Portal System (For Designers)

**By default: DISABLED** (won't interfere with gameplay)

**To enable:**
1. Press `Ctrl+Shift+M` in Play mode
2. Console shows: `[MoonPortal] Design mode ENABLED`
3. Red indicator appears at bottom: "DESIGN MODE ACTIVE"

**To use:**
- **Keyboard:** F1-F12 keys (direct warp)
- **Gamepad:** Press **Select** → open menu → **Left Stick** scrolls → **A** warps

**To disable:** Press `Ctrl+Shift+M` again

---

**TL;DR:** Flip F310 switch to X, unplug+replug, run diagnostics if still broken.
