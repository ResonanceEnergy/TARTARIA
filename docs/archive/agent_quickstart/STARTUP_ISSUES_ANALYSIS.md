# 🚨 STARTUP ISSUES IDENTIFIED

Based on your report:
- **"Some buttons work but can't walk around"** → Game state or input issue
- **"Camera is zoomed out and not behind player"** → Camera not tracking player
- **"Characters standing around frozen"** → NavMesh not baked OR NPCs missing AI components

---

## ROOT CAUSES (Most Likely)

### 1. **Game State Stuck**
**Symptom:** Buttons work but movement doesn't  
**Cause:** GameStateManager stuck in `Boot` or `Loading` state  
**Why:** PlayerInputHandler.Update() blocks ALL input when `!IsPlaying`  
**Fix:** Run `Tartaria` → `FIX: Force Exploration State`

### 2. **NavMesh Not Baked**
**Symptom:** NPCs standing frozen  
**Cause:** No NavMesh data in scene  
**Why:** NPCAIBehavior requires NavMesh to path-find, NavMeshAgent.SetDestination() fails silently  
**Fix:**
1. Window → AI → Navigation
2. Bake tab → Click "Bake" button
3. Wait for completion (~10-30 seconds)
4. Check "Navigation" window shows blue NavMesh overlay

### 3. **Camera Not Following Player**
**Symptom:** Camera stuck at wrong position/distance  
**Cause:** CameraController.followTarget is null OR player not spawned with "Player" tag  
**Why:** CameraController searches for `GameObject.FindWithTag("Player")` every 0.25s  
**Diagnostic:** Check if player exists and has "Player" tag in Inspector

---

## 🔧 FIXES CREATED

### **FullStartupDiagnostics.cs** (Comprehensive audit tool)
**Run:** Unity menu → `Tartaria` → `DIAGNOSE: Full Startup Audit`

**Checks:**
1. Game state (Boot/Loading/Exploration?)
2. Player spawning (exists? has components?)
3. Camera setup (following player? distance correct?)
4. Spawners (BuildingSpawner, EchohavenContentSpawner, PlayerSpawner)
5. Buildings (InteractableBuilding count)
6. NPCs (count, AI components, NavMeshAgent, Animator)
7. NavMesh (baked? has data? NPCs on mesh?)
8. Input (gamepad detected? left stick working?)

**Output:** Detailed Console report with exact issue counts + recommended fixes

---

## 🧪 TEST STEPS

### **Step 1: Run Full Diagnostic**
```
Unity Editor Play Mode → Menu → Tartaria → DIAGNOSE: Full Startup Audit
```

Check Console output for:
- `CRITICAL Issues: X`
- `Warnings: Y`
- Look for RED text showing exact problems

### **Step 2: Fix Game State (if stuck)**
```
Menu → Tartaria → FIX: Force Exploration State
```

Verify Console shows:
- `[GameState] Boot → Exploration` OR
- `[GameState] Loading → Exploration`

### **Step 3: Bake NavMesh (if NPCs frozen)**
```
1. Window → AI → Navigation
2. Bake tab
3. Click "Bake" button
4. Wait for completion
5. Verify blue NavMesh overlay visible in Scene view
```

### **Step 4: Check Player Tag**
```
1. Hierarchy → find "Player" GameObject
2. Inspector → top dropdown → should show "Player" tag
3. If untagged, change to "Player"
```

### **Step 5: Test Again**
1. Exit Play mode (if in it)
2. Enter Play mode (Ctrl+P)
3. Move left stick → character should move
4. Camera should follow player 6-9m behind
5. NPCs should wander around (if NavMesh baked)

---

## 📊 EXPECTED DIAGNOSTIC OUTPUT

**If everything works:**
```
╔══════════════════════════════════════════════════════╗
║       SUMMARY                                        ║
╚══════════════════════════════════════════════════════╝

  CRITICAL Issues: 0
  Warnings: 0

  [OK] All systems operational!
```

**If broken (common issues):**
```
[1] GAME STATE
  Current State: Boot  ← CRITICAL!
  [CRITICAL] State is 'Boot' — should be Exploration!

[3] CAMERA
  Distance to player: 45.2m  ← WARNING!
  [WARNING] Camera is VERY FAR from player!

[7] NAVMESH
  [CRITICAL] NavMesh has NO BAKED DATA!
  Window → AI → Navigation → Bake

SUMMARY:
  CRITICAL Issues: 3
  Warnings: 2

  TOP FIXES:
    1. Fix game state: Tartaria → FIX: Force Exploration State
    2. Bake NavMesh: Window → AI → Navigation → Bake
    3. Camera too far - check CameraController followTarget
```

---

## 🎯 QUICK FIX CHECKLIST

Run this in order:

- [ ] **Enter Play mode** (Ctrl+P)
- [ ] **Run diagnostic:** `Tartaria` → `DIAGNOSE: Full Startup Audit`
- [ ] **Read Console output** — count CRITICAL issues
- [ ] **If state wrong:** `Tartaria` → `FIX: Force Exploration State`
- [ ] **If NavMesh missing:** Window → AI → Navigation → Bake
- [ ] **If camera far:** Check player has "Player" tag
- [ ] **Exit Play → Re-enter Play** (restart fresh)
- [ ] **Test movement:** Left stick should move character
- [ ] **Test camera:** Should follow 6-9m behind, rotate smoothly
- [ ] **Test NPCs:** Should wander around after 3-8 seconds idle

---

## 🔍 WHY EACH ISSUE HAPPENS

### **Game State Stuck in Boot/Loading**
**Startup sequence:**
1. GameBootstrap.Start() → initializes ECS
2. SceneLoader.LoadGameplayScenes() → loads Echohaven + UI_Overlay
3. PlayerSpawner.Start() → spawns player
4. SceneLoader.FinishSceneLoad() → `TransitionTo(Exploration)`

**If ANY step fails:** State never reaches Exploration → input blocked

**Common causes:**
- PlayerSpawner prefab missing
- Scene load timeout
- Exception during spawn

### **NavMesh Not Baked**
**Unity doesn't auto-bake NavMesh** — must be done manually or via script

**NPCAIBehavior.WanderToRandomPoint():**
```csharp
if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
{
    _agent.SetDestination(hit.position);  // ← FAILS if no NavMesh!
}
```

**Result:** NPCs never get valid destinations → stand still forever

### **Camera Zoomed Out / Wrong Position**
**CameraController.LateUpdate():**
```csharp
if (followTarget == null)
{
    _playerSearchCooldown -= Time.deltaTime;
    if (_playerSearchCooldown > 0f) return;  // ← Keeps searching
    _playerSearchCooldown = 0.25f;
    var player = GameObject.FindWithTag("Player");
    if (player != null)
    {
        followTarget = player.transform;
        Debug.Log("[CameraController] Player found and locked.");
    }
}
```

**If player has wrong tag OR doesn't exist:** Camera never locks target → stays at spawn position (likely far from gameplay area)

---

## 📁 Files Modified

1. **FullStartupDiagnostics.cs** (NEW)
   - Comprehensive startup audit
   - Checks all 8 critical systems
   - Provides actionable fix recommendations

2. **MoonPortalSelector.cs** (FIXED)
   - Left stick hijacking resolved
   - Portal system disabled by default

3. **InputSystemDiagnostics.cs** (NEW)
   - Input-focused diagnostic
   - Gamepad detection
   - Game state check

4. **ForceExplorationState.cs** (NEW)
   - Emergency state fix
   - Forces transition to Exploration

---

## 🎮 After Fixes Applied

**Expected behavior:**
- ✅ Left stick moves character smoothly
- ✅ Camera follows 6-9m behind player
- ✅ Camera rotates with right stick
- ✅ Buttons work (A=interact, RT=attack, etc.)
- ✅ NPCs wander around zone
- ✅ Buildings visible and interactable
- ✅ HUD shows on screen

---

**STATUS: Diagnostic tools ready — run full audit in Play mode!**
