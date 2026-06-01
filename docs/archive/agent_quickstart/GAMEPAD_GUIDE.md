# TARTARIA GAMEPAD CONTROLS — Complete Reference

## 🎮 F310 Hardware Setup (CRITICAL!)

**The F310 has a physical mode switch on the BACK of the controller:**

```
[D]----------[X]
DirectInput  XInput
(OLD)        (NEW - REQUIRED!)
```

**FOR UNITY TO DETECT THE CONTROLLER:**
1. **Flip switch to X position** (right side)
2. **Unplug USB cable** from PC
3. **Replug USB cable**
4. **Verify in Windows:**
   - Press `Win+R` → type `joy.cpl` → Enter
   - Should show "Controller (Xbox 360 For Windows)"
   - If it says "Logitech Gamepad F310" → switch is in D mode (won't work!)

---

## 🕹️ Full Control Mapping

### Movement & Camera
| Control | Action | Notes |
|---------|--------|-------|
| **Left Stick** | Move player | All directions, analog speed |
| **Left Stick Click (L3)** | Sprint toggle | Hold to run faster |
| **Right Stick** | Camera look/rotate | Analog camera control |
| **D-Pad Up/Down** | Camera zoom in/out | Alternative to RB |

### Face Buttons (Right Side)
| Button | Action | GDD Context |
|--------|--------|-------------|
| **A (buttonSouth)** | Interact / Talk | Primary interaction - NPCs, buildings, objects |
| **B (buttonEast)** | Scan | Reveal secrets, dig sites, hidden paths |
| **X (buttonWest)** | ResonancePulse (light attack) | Basic attack, also triggers Aether abilities |
| **Y (buttonNorth)** | HarmonicStrike (heavy attack) | Powerful attack with cooldown |

### Triggers & Bumpers
| Control | Action | GDD Context |
|---------|--------|-------------|
| **RT (Right Trigger)** | **ResonancePulse (PRIMARY ATTACK)** | Main combat button - standard for action games |
| **LT (Left Trigger)** | FrequencyShield | Defensive ability - blocks/deflects |
| **LB (Left Bumper)** | AetherVision | Toggle vision mode - reveals Aether fields |
| **RB (Right Bumper)** | Camera zoom | Alternative to D-Pad |

### System
| Button | Action |
|--------|--------|
| **Start** | Pause menu |
| **Select/Back** | *(not mapped yet)* |

---

## 📖 Detailed Button Descriptions

### **RT (Right Trigger) — ResonancePulse**
- **Most frequently used combat button**
- Light attack with fast cooldown
- Can be pressed repeatedly for combo chains
- Also activates Aether resonance when near buildings
- **Why RT?** Industry standard — most action games use RT for primary attack (shooting, melee, magic)

### **LT (Left Trigger) — FrequencyShield**
- Defensive ability with energy cost
- Hold to maintain shield, release to stop
- Depletes Aether energy while active
- Essential for boss fights and combat waves

### **LB (Left Bumper) — AetherVision**
- **Toggle** ability (press once to enable, press again to disable)
- Reveals hidden Aether fields, ley lines, and corruption zones
- Shows interactable objects through walls (limited range)
- Visual: blue/cyan overlay with particle effects
- No energy cost, but camera FOV narrows slightly

### **A Button — Interact**
- Context-sensitive action:
  - Talk to NPCs (Milo, Cassian, Lirael)
  - Excavate buildings (Star Dome, Fountain, Spire)
  - Pick up collectibles (Aether shards, lore fragments)
  - Activate switches and mechanisms
  - Enter portals
- **Range: 3 meters** (configurable in PlayerInputHandler)
- Shows "Press [A] to interact" prompt on HUD when near valid target

### **B Button — Scan**
- Area-of-effect ability centered on player
- Reveals secrets within 10-15 meter radius:
  - Hidden dig sites (shows ground markers)
  - Collectibles behind walls
  - Secret passages
  - Corruption veins
- Cooldown: ~8 seconds
- Audio: pulse sound effect + visual ripple

### **X Button — ResonancePulse (alternate binding)**
- Same action as RT
- **Why two bindings?** Player choice — some prefer trigger, some prefer face button
- Useful during non-combat exploration when trigger finger is on camera (RT)

### **Y Button — HarmonicStrike**
- Heavy attack with longer cooldown (~5 seconds)
- Higher damage than ResonancePulse
- Slower animation — can be interrupted
- Best used after stunning enemy or creating distance
- Depletes more Aether energy

---

## 🎯 Combat Flow Example

**Standard combat rotation:**
1. **RT (ResonancePulse)** × 3 → light attack combo
2. **Y (HarmonicStrike)** → heavy finisher
3. **LT (FrequencyShield)** → block counterattack
4. **Repeat**

**Defensive play:**
1. **LT (hold)** → shield raised
2. Wait for enemy attack to hit shield
3. **Release LT** + **Y** → counter with HarmonicStrike

---

## 🐛 Troubleshooting

### Problem: Joystick won't move character

**DIAGNOSIS:**
1. **Check game state:**
   - In Unity Editor: Menu → `Tartaria` → `DIAGNOSE: Input System`
   - Look for "Current State" in Console output
   - **Must be `Exploration`, `Tuning`, or `Combat`**
   - If stuck in `Boot` or `Loading` → see fix below

2. **Check F310 switch:**
   - Physical switch on **back** of controller
   - Must be in **X position** (right side)
   - If in D position, Unity won't detect it

3. **Check Windows detection:**
   - `Win+R` → `joy.cpl` → Enter
   - Should show "Controller (Xbox 360 For Windows)"
   - If "Logitech Gamepad F310" → switch is wrong

**QUICK FIXES:**
```
1. F310 switch issue:
   - Flip switch from D to X
   - Unplug + replug USB
   - Wait 5 seconds
   - Check joy.cpl

2. Game state stuck in Boot/Loading:
   - In Unity Editor: Menu → Tartaria → FIX: Force Exploration State
   - OR: Exit Play mode, re-enter

3. Player not spawned:
   - Check Console for "[PlayerSpawner] Player spawned at..."
   - If missing: Exit Play, check Echohaven scene has PlayerSpawn marker
```

### Problem: Buttons do nothing

**CHECK:**
1. Is the HUD visible? (means game state is correct)
2. Can you move? (if yes, buttons should work)
3. Check Console for errors
4. Run `Tartaria` → `DIAGNOSE: Input System`

**COMMON CAUSES:**
- Dialogue panel open (blocks input until closed)
- Pause menu open (press Start to close)
- Game state is Cinematic (cutscene playing)
- InputActions asset not assigned to PlayerInputHandler

### Problem: RT doesn't attack, does vision mode instead

**FIX:** You have old input mapping!
1. Check file: `Assets\_Project\Input\TartariaInputActions.inputactions`
2. Find binding for `<Gamepad>/rightTrigger`
3. Should be under action: `ResonancePulse` (not `AetherVision`)
4. If wrong, see [GAMEPAD_CONTROLS.md](GAMEPAD_CONTROLS.md) for current mapping

---

## 📁 Technical Reference

### Input System Files
- **InputActions config:** `Assets\_Project\Input\TartariaInputActions.inputactions`
- **Handler script:** `Assets\_Project\Scripts\Input\PlayerInputHandler.cs`
- **Spawner:** `Assets\_Project\Scripts\Integration\PlayerSpawner.cs`
- **State manager:** `Assets\_Project\Scripts\Core\GameStateManager.cs`

### Layer Masks (for interaction raycasts)
- **Building** (layer 8): Tartarian buildings
- **Interactable** (layer 9): NPCs, collectibles, switches
- **Player** (layer 10): Player character
- **Trigger** (layer 11): Portal triggers, zone boundaries
- **Enemy** (layer 12): Combat targets

**Interact raycast mask:** `0x1B01` (Building + Interactable + Trigger + Default + Enemy)

### Key Code Paths

**Input blocking check (PlayerInputHandler.cs line 206):**
```csharp
if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;
```
- `IsPlaying` = true ONLY when state is `Exploration`, `Tuning`, or `Combat`
- If state is `Boot`, `Loading`, `Menu`, `Cinematic`, or `Paused` → **ALL INPUT BLOCKED**

**Movement input (PlayerInputHandler.cs line 320+):**
```csharp
void HandleMovementInput()
{
    _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
    
    // Fallback: read gamepad directly if InputActions failed
    var pad = Gamepad.current;
    if (pad != null && _moveInput.sqrMagnitude < 0.01f)
    {
        Vector2 stick = pad.leftStick.ReadValue();
        if (stick.sqrMagnitude > 0.0225f) _moveInput = stick;
    }
    // ... rest of movement code
}
```

**State transition (SceneLoader.cs line 220):**
```csharp
GameStateManager.Instance.TransitionTo(GameState.Exploration);
```
- Called after both gameplay + UI scenes load
- Triggered by `PlayerSpawner.Start()` completing
- If this doesn't fire, movement won't work!

---

## 🎓 GDD References

See full gameplay design in:
- **docs/07_PC_UX.md** — PC/gamepad control scheme design
- **docs/00_MASTER_GDD.md** — Core gameplay loops
- **docs/09_TECHNICAL_SPEC.md** — Input System architecture

---

## ✅ Diagnostic Checklist

Before reporting gamepad issues, run through this checklist:

- [ ] F310 switch in X position (back of controller, right side)
- [ ] Unplugged + replugged USB after switch flip
- [ ] Windows joy.cpl shows "Controller (Xbox 360 For Windows)"
- [ ] Unity Play mode active (not Edit mode)
- [ ] Console shows `[PlayerSpawner] Player spawned at...`
- [ ] Console shows `[GameState] Loading → Exploration`
- [ ] Run `Tartaria` → `DIAGNOSE: Input System` (check for issues)
- [ ] HUD visible on screen (means UI loaded correctly)
- [ ] No active dialogue panel or pause menu
- [ ] Can move with WASD keyboard (tests if state is correct)

**If ALL checks pass but gamepad still doesn't work:**
→ File bug with full Console log + diagnostic output

---

## 🔄 Version History

- **v1.3 (2026-05-27)** — Fixed RT→ResonancePulse mapping, added comprehensive diagnostics
- **v1.2** — Added LT→FrequencyShield, LB→AetherVision swap
- **v1.1** — F310 XInput mode documentation
- **v1.0** — Initial gamepad mapping
