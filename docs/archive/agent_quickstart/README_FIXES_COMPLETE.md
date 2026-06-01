# 🎮 TARTARIA STARTUP & GAMEPLAY FIXES — COMPLETE REFERENCE

## 📋 ISSUE SUMMARY

**User-Reported Problems:**
1. Some buttons work but **can't walk around**
2. Camera is **zoomed out and not behind player**
3. **Characters standing around frozen**
4. Left stick was opening moon portal (FIXED)

**Status:** Diagnostic tools created, fixes ready to apply.

---

## 🔧 FIXES CREATED (In Order of Creation)

### **1. Input System Fixes**
- **InputSystemDiagnostics.cs** — Checks game state, gamepad, player handler, input actions
- **ForceExplorationState.cs** — Emergency fix to transition from Boot/Loading to Exploration
- **MoonPortalSelector.cs** — FIXED left stick hijacking (portal system now disabled by default)

### **2. Button Mapping Fixes**
- **TartariaInputActions.inputactions** — Updated RT→ResonancePulse, LT→FrequencyShield, LB→AetherVision
- **GAMEPAD_CONTROLS.md** — Mapping cheat sheet
- **GAMEPAD_GUIDE.md** — Comprehensive control reference (2000+ words)
- **GAMEPAD_QUICKSTART.md** — TL;DR version

### **3. Startup Diagnostic Tools**
- **FullStartupDiagnostics.cs** — Comprehensive audit of ALL startup systems (8 categories)
- **STARTUP_ISSUES_ANALYSIS.md** — Detailed problem analysis with root causes

---

## 🎯 ROOT CAUSES (Identified)

### **Problem 1: Can't Walk Around**
**Cause:** GameStateManager stuck in `Boot` or `Loading` state  
**Why:** PlayerInputHandler.Update() has guard:
```csharp
if (!GameStateManager.Instance.IsPlaying) return;  // Blocks ALL input!
```
`IsPlaying` only true when state is `Exploration`, `Tuning`, or `Combat`.

**Fix:**
```
Menu → Tartaria → FIX: Force Exploration State
```

---

### **Problem 2: Camera Zoomed Out / Not Following**
**Cause:** CameraController.followTarget is null  
**Why:** Camera searches for player by tag every 0.25s:
```csharp
var player = GameObject.FindWithTag("Player");
```
If player not spawned OR has wrong tag → camera never locks target.

**Diagnostic:**
```
Menu → Tartaria → DIAGNOSE: Full Startup Audit
```
Check "Camera" section for "followTarget: NULL" or distance > 20m.

**Fix:**
1. Check Hierarchy for "Player" GameObject
2. Inspector → Tag dropdown → set to "Player"
3. Exit Play → Re-enter Play

---

### **Problem 3: Characters Frozen**
**Cause:** NavMesh not baked in scene  
**Why:** NPCAIBehavior.WanderToRandomPoint() calls:
```csharp
if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
{
    _agent.SetDestination(hit.position);  // ← FAILS if no NavMesh!
}
```
Without baked NavMesh data, this always fails → NPCs never get destinations → stand frozen.

**Fix:**
1. Window → AI → Navigation
2. Bake tab
3. Click "Bake" button (wait 10-30 seconds)
4. Verify blue NavMesh overlay in Scene view
5. Exit Play → Re-enter Play

---

## 🧪 COMPLETE TESTING WORKFLOW

### **Step 1: Enter Play Mode**
```
Press Ctrl+P or click Play button
Wait for scene to load
```

### **Step 2: Run Full Diagnostic**
```
Menu → Tartaria → DIAGNOSE: Full Startup Audit
```

**Read Console output for:**
- `CRITICAL Issues: X` (must be 0)
- `Warnings: Y` (should be minimal)
- RED text showing exact problems
- "TOP FIXES" section at bottom

### **Step 3: Apply Fixes Based on Diagnostic**

**If "State is 'Boot'" found:**
```
Menu → Tartaria → FIX: Force Exploration State
Check Console for: [GameState] Boot → Exploration
```

**If "NavMesh has NO BAKED DATA":**
```
Exit Play mode first
Window → AI → Navigation → Bake tab → Click "Bake"
Wait for completion (~20 seconds)
Scene view should show blue NavMesh overlay
Re-enter Play mode
```

**If "followTarget is NULL" OR "Distance to player: >20m":**
```
Hierarchy → find "Player" GameObject
Inspector → Tag dropdown → change to "Player"
Exit Play → Re-enter Play
```

**If "Player NOT FOUND":**
```
Check Console for [PlayerSpawner] logs
Look for exceptions during spawn
Check PlayerSpawner prefab is assigned in scene
```

### **Step 4: Verify Fixes**
After applying fixes, run diagnostic again:
```
Menu → Tartaria → DIAGNOSE: Full Startup Audit
Should show: CRITICAL Issues: 0, Warnings: 0
```

### **Step 5: Test Gameplay**
1. **Movement:** Push left stick → character should move smoothly
2. **Camera:** Should follow 6-9m behind player, rotate with right stick
3. **Buttons:** RT=attack, A=interact, B=scan, etc.
4. **NPCs:** Should idle 3-8 seconds, then wander to new position
5. **Buildings:** Should be visible (3 total: Star Dome, Fountain, Spire)

---

## 📊 DIAGNOSTIC MENU REFERENCE

**All in Unity Editor → Menu → Tartaria:**

| Tool | Purpose | When to Use |
|------|---------|-------------|
| **DIAGNOSE: Full Startup Audit** | Checks ALL 8 systems | FIRST STEP — always run this |
| **DIAGNOSE: Input System** | Focused on input/gamepad | If movement doesn't work |
| **DIAGNOSE: Check Runtime State** | Checks spawners/buildings/NPCs | If content missing |
| **FIX: Force Exploration State** | Transitions to Exploration | If state stuck in Boot/Loading |
| **FIX: Add Missing Spawners** | Creates missing spawners | If spawners not in scene |

---

## 🎮 GAMEPAD CONTROLS (Fixed)

| Button | Action | Notes |
|--------|--------|-------|
| **Left Stick** | **Move player** | **FIXED** — no longer opens portal |
| L3 (click) | Sprint | Toggle |
| Right Stick | Camera rotate | Look around |
| **RT** | **ResonancePulse (PRIMARY ATTACK)** | **FIXED** — was AetherVision |
| **LT** | **FrequencyShield** | **FIXED** — was unbound |
| **LB** | **AetherVision** | **FIXED** — was FrequencyShield |
| RB | Camera zoom | Alternative |
| A | Interact | Talk/pickup |
| B | Scan | Reveal secrets |
| X | ResonancePulse | Alt attack |
| Y | HarmonicStrike | Heavy attack |
| Start | Pause | Menu |
| **Select** | Portal menu | Only in design mode (Ctrl+Shift+M) |

**For full details:** See [GAMEPAD_GUIDE.md](GAMEPAD_GUIDE.md)

---

## 🔍 EXPECTED DIAGNOSTIC OUTPUT

### **If Everything Works (Target State):**
```
╔══════════════════════════════════════════════════════╗
║       FULL STARTUP DIAGNOSTICS                      ║
╚══════════════════════════════════════════════════════╝

[1] GAME STATE
  Current State: Exploration
  IsPlaying: True
  [OK] State allows gameplay

[2] PLAYER
  GameObject: Player
  Position: (0.0, 1.0, -20.0)
  PlayerInputHandler: ENABLED
  CharacterController: ENABLED
  [OK] Player exists

[3] CAMERA
  GameObject: Main Camera
  CameraController: FOUND on CameraRig
  followTarget: Player at (0.0, 1.0, -20.0)
  Distance to player: 7.2m
  [OK] Camera has target

[4] SPAWNERS
  BuildingSpawner: FOUND
  EchohavenContentSpawner: FOUND
  PlayerSpawner: FOUND

[5] BUILDINGS
  Found 3 InteractableBuilding components
    - StarDome_Greybox at (30.0, 0.0, 20.0)
    - HarmonicFountain_Greybox at (-20.0, 0.0, 35.0)
    - CrystalSpire_Greybox at (0.0, 0.0, -30.0)

[6] NPCs & AI
  Found 3 potential NPC objects
    - Milo:
        NPCAIBehavior: YES
        NavMeshAgent: YES
        NavMesh on mesh: True
    - Cassian:
        NPCAIBehavior: YES
        NavMeshAgent: YES
        NavMesh on mesh: True
    - Lirael:
        NPCAIBehavior: YES
        NavMeshAgent: YES
        NavMesh on mesh: True
  
  Summary:
    NPCs with AI: 3/3
    NPCs with NavMesh: 3/3
    NPCs with Animator: 3/3

[7] NAVMESH
  NavMeshSurface: FOUND on GroundPlane
  NavMesh vertices: 2847
  NavMesh triangles: 949
  [OK] NavMesh is baked

[8] INPUT
  Gamepad: Controller (Xbox 360 For Windows)
  Left stick: (0.000, 0.000)

╔══════════════════════════════════════════════════════╗
║       SUMMARY                                        ║
╚══════════════════════════════════════════════════════╝

  CRITICAL Issues: 0
  Warnings: 0

  [OK] All systems operational!
```

### **If Broken (Common Issues):**
```
[1] GAME STATE
  Current State: Boot
  [CRITICAL] State is 'Boot' — should be Exploration!

[3] CAMERA
  Distance to player: 45.2m
  [WARNING] Camera is VERY FAR from player!

[7] NAVMESH
  [CRITICAL] NavMesh has NO BAKED DATA!

SUMMARY:
  CRITICAL Issues: 2
  Warnings: 1

  TOP FIXES:
    1. Fix game state: Tartaria → FIX: Force Exploration State
    2. Bake NavMesh: Window → AI → Navigation → Bake
    3. Camera too far - check CameraController followTarget
```

---

## 📁 ALL FILES CREATED/MODIFIED

### **Diagnostic Tools**
- `Assets\_Project\Scripts\Editor\FullStartupDiagnostics.cs` — Comprehensive audit (8 systems)
- `Assets\_Project\Scripts\Editor\InputSystemDiagnostics.cs` — Input-focused diagnostic
- `Assets\_Project\Scripts\Editor\DiagnoseRuntime.cs` — Runtime state check
- `Assets\_Project\Scripts\Editor\ForceExplorationState.cs` — Emergency state fix
- `Assets\_Project\Scripts\Editor\EmergencySpawnerFix.cs` — Manual spawner creation

### **Input System**
- `Assets\_Project\Input\TartariaInputActions.inputactions` — Button mapping (RT/LT/LB fixed)
- `Assets\_Project\Scripts\Integration\MoonPortalSelector.cs` — Portal hijacking fixed

### **Documentation**
- `GAMEPAD_GUIDE.md` — Complete control reference (2000+ words)
- `GAMEPAD_QUICKSTART.md` — TL;DR troubleshooting
- `GAMEPAD_CONTROLS.md` — Button mapping cheat sheet
- `STARTUP_ISSUES_ANALYSIS.md` — Root cause analysis
- `FIX_LEFT_STICK_HIJACKING.md` — Portal bug details
- `THIS_FILE.md` — Master reference

### **Runtime Insurance**
- `Assets\_Project\Scripts\Integration\RuntimeSpawnerInsurance.cs` — Auto-creates missing spawners
- `Assets\_Project\Scripts\Integration\EchohavenContentSpawner.cs` — EnableNPCAI() added

---

## ⚡ QUICK REFERENCE CARD

**Problem: Can't move**
→ `Tartaria → DIAGNOSE: Input System`
→ Check game state
→ `Tartaria → FIX: Force Exploration State`

**Problem: Camera wrong**
→ `Tartaria → DIAGNOSE: Full Startup Audit`
→ Check player tag = "Player"
→ Exit Play → Re-enter Play

**Problem: NPCs frozen**
→ `Window → AI → Navigation → Bake`
→ Wait for completion
→ Exit Play → Re-enter Play

**Problem: Buildings missing**
→ `Tartaria → DIAGNOSE: Check Runtime State`
→ Check Console for spawner logs
→ `Tartaria → FIX: Add Missing Spawners`

**Problem: Buttons wrong**
→ See [GAMEPAD_GUIDE.md](GAMEPAD_GUIDE.md)
→ RT=attack, LT=shield, LB=vision

**F310 Switch Issue**
→ Flip to **X** position (back of controller)
→ Unplug + replug USB
→ Test: `Win+R` → `joy.cpl`

---

## 🚀 NEXT STEPS

1. **Enter Play Mode** (Ctrl+P)
2. **Run Full Diagnostic** (`Tartaria` → `DIAGNOSE: Full Startup Audit`)
3. **Read Console Output** (count CRITICAL + WARNING)
4. **Apply Recommended Fixes** (in order shown)
5. **Exit Play → Re-enter Play** (fresh start)
6. **Test All Systems** (movement, camera, NPCs, buttons)
7. **Report Results** (what works, what doesn't)

---

**STATUS: All diagnostic tools deployed. Ready for testing!**
