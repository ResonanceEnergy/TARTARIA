# F310 Gamepad Controls (FIXED)

## Movement
- **Left Stick** → Move player
- **Right Stick** → Camera look
- **Left Stick Click** → Sprint toggle

## Combat/Actions
- **A (buttonSouth)** → Interact / Talk / Pick up
- **B (buttonEast)** → Scan (find secrets/dig sites)
- **X (buttonWest)** → ResonancePulse (light attack)
- **Y (buttonNorth)** → HarmonicStrike (heavy attack)

## Triggers & Bumpers
- **RT (right trigger)** → ResonancePulse (PRIMARY ATTACK) ⭐
- **LT (left trigger)** → FrequencyShield (defensive ability)
- **LB (left shoulder)** → AetherVision (toggle vision mode)
- **RB (right shoulder)** → Camera zoom

## System
- **Start** → Pause menu

---

## What Changed

**Before (broken):**
- RT → AetherVision ❌
- LB → FrequencyShield ❌
- No LT binding ❌

**After (fixed):**
- RT → ResonancePulse (primary attack) ✓
- LT → FrequencyShield (shield/defense) ✓
- LB → AetherVision (vision mode) ✓

**Why:** Most action games use **RT for primary attack** (like shooting/combat). The old mapping had RT triggering a vision mode which felt wrong.

---

## Controller Setup Reminder

**F310 must be in X (XInput) mode:**
1. Physical switch on BACK of controller → X position
2. Unplug + replug USB
3. Verify in Windows: `joy.cpl` → should show "Controller (Xbox 360 For Windows)"

See [F310_QUICK_FIX.md](F310_QUICK_FIX.md) for details.
