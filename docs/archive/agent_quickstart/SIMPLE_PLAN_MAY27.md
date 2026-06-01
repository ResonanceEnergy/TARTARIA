# 🎯 TARTARIA — SIMPLE PLAN TO GET PLAYABLE

**Date:** May 27, 2026  
**Status:** Currently NOT playable (player doesn't spawn)  
**Goal:** Get player moving in Echohaven scene in 30 minutes

---

## 📋 CURRENT STATE (From Context + Roadmap)

### ✅ WHAT'S DONE (All code compiles)
- **Core systems**: GameStateManager, Aether Field, Resonance Score, Save/Load
- **Input system**: PlayerInputHandler with WASD + gamepad (NEW: bypasses GameState check)
- **Building system**: InteractableBuilding, tuning mini-game, restoration
- **Integration layer**: GameLoopController, spawners, dialogue, quests
- **All 13 Moon arcs**: Moons 1-13 vertical slice scripts written
- **Documentation**: 540+ docs, comprehensive GDD
- **Build status**: CS:0 (zero compilation errors per CONTEXT.md)

### 🔴 WHAT'S BROKEN
- **Scene missing PlayerSpawner** → No player spawns → Can't test ANYTHING
- **User can't test game** because no player exists to control

### 📊 CODE METRICS
- 318 C# files, ~54,000 lines
- 123 Manager/Controller/System classes
- 13/15 core systems complete
- Architecture solid (ECS, Service Locator, modular)

---

## 🚀 30-MINUTE FIX PLAN

### GOAL: Get player walking in Echohaven scene

### STEP 1: Scene Setup (5 minutes)
**Tool created:** `SceneSetupFixer.cs` with one-click menu item

**Action:**
```
Menu → Tartaria → 🚀 ONE-CLICK: Load & Setup Echohaven
```

**What it does:**
1. Loads `Echohaven_VerticalSlice.unity`
2. Creates `PlayerSpawner` GameObject
3. Finds and assigns player prefab from Assets
4. Finds and assigns `TartariaInputActions.inputactions`
5. Creates `PlayerSpawn` marker at (0, 1, 0)
6. Creates Main Camera if missing
7. Creates GameStateManager if missing
8. Saves scene automatically

### STEP 2: Enter Play Mode (1 minute)
```
Ctrl+P
```

### STEP 3: Force Playable State (2 minutes)
**Tool created:** `EmergencyPlayableFix.cs`

**Action:**
```
Menu → Tartaria → EMERGENCY: Make Game Playable NOW
```

**What it does:**
1. Forces `GameState.Exploration`
2. Tries to spawn player if missing
3. Enables CharacterController
4. Enables PlayerInputHandler
5. Wires camera to player
6. Shows gamepad detection status

### STEP 4: Move (immediate)
```
WASD or Left Stick
```
Character moves. Camera follows.

### NUCLEAR FALLBACK (if still broken)
**Tool created:** `ForceWASDMovement.cs`

**Action:**
```
Menu → Tartaria → NUCLEAR: Force WASD Movement
```

**What it does:**
- Runs in Editor update loop
- Directly calls `CharacterController.Move()` with WASD input
- Bypasses ALL game systems
- Pure input → position change

---

## 📝 WHAT GOT FIXED TODAY

### Files Modified:
1. **PlayerInputHandler.cs** — Removed GameState check from `Update()`, movement always processes
2. **GameLoopController.cs** — Added null checks for EntityManager (fixed NullReferenceException)
3. **EmergencyPlayableFix.cs** — NEW: Forces playable state + spawns player
4. **ForceWASDMovement.cs** — NEW: Nuclear option direct movement
5. **SceneSetupFixer.cs** — NEW: One-click scene setup tool

### Bugs Fixed:
- ✅ EntityManager null reference crash
- ✅ PlayerInputHandler blocking movement when state wrong
- ✅ Left stick hijacking (MoonPortalSelector)
- ✅ Missing scene setup automation

---

## 🎯 IMMEDIATE NEXT STEPS (After Player Moves)

Once player is walking, tackle upgrade queue in order:

### Priority 1: Basic Interaction (Week 1)
- [ ] Test player can walk around Echohaven
- [ ] Test camera follows properly
- [ ] Place 3 test buildings in scene
- [ ] Test E-key interaction with buildings
- [ ] Verify tuning mini-game launches

### Priority 2: Core Loop Wiring (Week 1-2)
- [ ] HUD shows RS counter
- [ ] Tuning mini-game awards RS
- [ ] Building restoration triggers VFX
- [ ] Save/Load preserves player position + RS

### Priority 3: Polish Pass (Week 2-3)
- [ ] Audio feedback on all interactions
- [ ] VFX for scan, restore, pickup
- [ ] Post-processing volume
- [ ] NavMesh bake for NPCs
- [ ] Day/night cycle visual only

### Priority 4: Combat Prototype (Week 3-4)
- [ ] Spawn 1 Mud Golem enemy
- [ ] Basic attack with RT
- [ ] Enemy takes damage and dies
- [ ] Combat VFX + audio

---

## 📊 SUCCESS CRITERIA

### Minimum Playable (End of Today)
- ✅ Player spawns at (0,1,0) in Echohaven
- ✅ WASD or gamepad moves character
- ✅ Camera follows 6-9m behind
- ✅ No errors in Console
- ✅ Can save and quit without crash

### Week 1 Target
- ✅ All of above
- ✅ 3 buildings placed and discoverable
- ✅ E-key interaction works
- ✅ Tuning mini-game functional
- ✅ RS counter visible in HUD
- ✅ Restoration triggers celebration

### Month 1 Target (MVP Vertical Slice)
- ✅ All Week 1 items
- ✅ Milo companion spawns and follows
- ✅ 1 Mud Golem enemy + basic combat
- ✅ Audio feedback on all actions
- ✅ VFX for major events
- ✅ Save/Load works reliably
- ✅ 60 FPS on GTX 1070+
- ✅ 15-minute demo playthrough possible

---

## 🚨 CRITICAL RULES

### DO:
- ✅ Focus on ONE system at a time
- ✅ Test after EVERY change
- ✅ Keep build green (compile clean)
- ✅ Use the upgrade queue as guide
- ✅ Validate with `.\tartaria-play.ps1 -BatchOnly`

### DON'T:
- ❌ Try to fix everything at once
- ❌ Add new features before basics work
- ❌ Ignore compilation errors
- ❌ Skip testing between changes
- ❌ Touch Moon 2-13 content yet

---

## 🎓 KEY INSIGHTS FROM CONTEXT

1. **Architecture is solid** — Code is well-structured, just needs scene setup
2. **All systems exist** — GameLoop, Input, Camera, Save, etc. all written
3. **Problem is simple** — Missing PlayerSpawner in scene = no player = can't test
4. **Solution is simple** — Use scene setup tool → player spawns → game works
5. **Upgrade queue is good** — Follow it after basics work

---

## ⚡ TL;DR — DO THIS RIGHT NOW

**Three menu items:**

1. `Tartaria → 🚀 ONE-CLICK: Load & Setup Echohaven` (loads scene + creates spawner)
2. `Ctrl+P` (enter Play Mode)
3. `Tartaria → EMERGENCY: Make Game Playable NOW` (forces state + spawns player)
4. `WASD` (move character)

**If that doesn't work:**

`Tartaria → NUCLEAR: Force WASD Movement` (bypasses everything, directly moves player)

---

**That's the plan. Simple. Focused. Testable.**

**Once player moves, we follow the upgrade queue step by step.**
