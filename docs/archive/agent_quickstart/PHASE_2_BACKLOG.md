# PHASE 2 AUTONOMOUS BACKLOG — Post-Asset Integration

**Prerequisites:** Phase 1 runtime test GREEN (Star Dome Gothic hall spawning correctly)  
**Estimated Time:** 6-8 hours of autonomous work  
**Expected Quality Jump:** 78/100 → 88/100 (near-AAA visual standard)

---

## IMMEDIATE (High Impact, Low Risk)

### 1. Post-Processing Volume Setup (30 minutes, +8 quality points)
**Impact:** Immediate visual upgrade across entire game  
**Risk:** Low (URP standard asset, no gameplay changes)  
**Tasks:**
- Create `Assets/_Project/Settings/TartariaPostProcessVolume.asset`
- Configure bloom (intensity 0.3, threshold 0.9, soft knee 0.5)
- Configure color grading (temperature +5, tint +2, contrast 1.05, saturation 1.1)
- Configure vignette (intensity 0.25, smoothness 0.4, rounded false)
- Configure ambient occlusion (intensity 0.5, thickness modifier 1.0)
- Add Volume component to Main Camera in EchohavenStartScene
- Set weight to 1.0, priority to 0, isGlobal to true
- Test in Play mode (verify no performance regression)

**Files:**
- Create: `Assets/_Project/Settings/TartariaPostProcessVolume.asset`
- Modify: `Assets/_Project/Scenes/EchohavenStartScene.unity` (add Volume to Main Camera)
- Validate: Editor.log shows no Volume errors, FPS maintains 60+

---

### 2. Fantasy Ruins Exterior Wrapper (45 minutes, +5 quality points)
**Impact:** Star Dome becomes Gothic cathedral with interior/exterior  
**Risk:** Low (additive only, no changes to existing CreateModularDungeonStarDome)  
**Tasks:**
- Add `CreateFantasyRuinsExterior()` method to BuildingSpawner.cs
- Load `Resources.Load<GameObject>("Models/Buildings/FantasyRuins/CathedralRuins_01")`
- Position at Star Dome center (0, 0, 0) rotated to face player spawn
- Scale to 1.2× to wrap around 40m diameter modular dungeon interior
- Add `CreateFoundationRuins()` method for ground scatter
- Load `foundationRuin_01.dae`, spawn 8 copies in ring around dome (radius 25m)
- Wire into CreateModularDungeonStarDome: create interior first, then exterior wrapper
- Test collision (player can enter cathedral via arched doorway)

**Files:**
- Modify: `Assets/_Project/Scripts/Integration/BuildingSpawner.cs` (+80 lines)
- Validate: Star Dome shows as full cathedral (interior + exterior) in Play mode

---

### 3. Animation Rig Polish — Mixamo Retargeting (60 minutes, +3 quality points)
**Impact:** Player + NPC animations blend smoothly, no T-pose flashing  
**Risk:** Low (Mixamo Auto-Rig compatible with Unity Mecanim)  
**Tasks:**
- Run `Assets/_Project/Tools/Auto-MixamoFetch.py` to download 12 animations:
  - Idle, Walk, Run, Jump, Land, Attack1, Attack2, Dodge, Interact, Death, Victory, Dialogue_Idle
- Configure FBX import settings: Animation Type = Humanoid, Avatar Definition = Create From This Model
- Create Animator Controller: `Assets/_Project/Animations/PlayerAnimatorController.controller`
- Add blend tree for locomotion (Idle/Walk/Run based on velocity magnitude)
- Add transition states for Attack/Dodge/Interact (trigger parameters)
- Assign to Player prefab's Animator component
- Test in Play mode (verify WASD movement triggers Walk/Run, E triggers Interact)

**Files:**
- Create: `Assets/_Project/Animations/Mixamo/*.fbx` (12 files)
- Create: `Assets/_Project/Animations/PlayerAnimatorController.controller`
- Modify: Player prefab (assign Animator Controller)
- Validate: No T-pose in Play mode, smooth blend transitions

---

## SECONDARY (Medium Impact, Medium Risk)

### 4. Terrain LOD Setup (90 minutes, +4 quality points)
**Impact:** Performance optimization + visual quality upgrade  
**Risk:** Medium (Unity Terrain Tools can be finicky)  
**Tasks:**
- Create Terrain GameObject in EchohavenStartScene (2048×2048 heightmap, 256 detail resolution)
- Import heightmap texture (generate procedurally via Perlin noise: flat center valley, hills at edges)
- Paint terrain layers: grass (center), dirt paths (Star Dome → player spawn), rock (hills)
- Add detail meshes: grass clumps (density 0.5), wildflowers (density 0.2)
- Configure LOD settings: Base Map Distance 1500, Detail Distance 150, Tree Distance 2000
- Add Terrain Collider for physics
- Test performance (GPU Instancing for details, no per-frame GC alloc)

**Files:**
- Modify: `Assets/_Project/Scenes/EchohavenStartScene.unity` (add Terrain GameObject)
- Create: `Assets/_Project/Terrain/Echohaven_Heightmap.raw` (2048×2048 16-bit grayscale)
- Create: `Assets/_Project/Terrain/TerrainLayers/` (Grass_01, Dirt_01, Rock_01 layer assets)
- Validate: FPS maintains 60+, no terrain holes, collider works

---

### 5. Adaptive Music Layer Wiring (60 minutes, +2 quality points)
**Impact:** Audio responds to RS thresholds, combat intensity  
**Risk:** Low (AudioManager.cs already has layer support)  
**Tasks:**
- Verify AdaptiveMusicController has 2-layer prototype (base + intensity)
- Wire GameLoopController.OnThresholdCrossed to trigger music transitions:
  - RS 0-24: Exploration_Calm (layer 1 only)
  - RS 25-49: Exploration_Rising (layer 1 + layer 2 at 50% volume)
  - RS 50-74: Exploration_Energized (layer 1 + layer 2 at 100% volume)
  - RS 75-99: Restoration_Approaching (layer 1 + layer 2 + percussion stinger)
  - RS 100: Restoration_Complete (full orchestral crescendo)
- Add crossfade transitions (0.5s duration, logarithmic curve)
- Test in Play mode (verify layer volume changes as RS increases)

**Files:**
- Modify: `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs` (+30 lines)
- Modify: `Assets/_Project/Scripts/Integration/GameLoopController.cs` (wire OnThresholdCrossed)
- Validate: Audio layers crossfade smoothly as RS increases

---

### 6. NPC Dialogue Tree Expansion — Milo Emotional Payoffs (90 minutes, +3 quality points)
**Impact:** Story depth, player emotional engagement  
**Risk:** Low (dialogue system already functional)  
**Tasks:**
- Read `docs/05_CHARACTERS_DIALOGUE.md` for Milo dialogue arcs
- Add 6 new dialogue nodes to MiloController.cs:
  - First building discovered: "The resonance… it's alive again! I'd forgotten how beautiful it sounds."
  - RS threshold 50: "You're getting stronger. The Giants would be proud."
  - RS threshold 75: "We're close. I can feel the old world stirring."
  - RS threshold 100: "You did it! Echohaven breathes again. Thank you, friend."
  - First combat victory: "You fight like the old Guard. Natural harmony."
  - All 3 buildings discovered: "The trinity is complete. The lattice can heal now."
- Wire to CompanionManager.OnTrustChanged (unlock dialogue based on trust level)
- Wire to GameLoopController.OnThresholdCrossed (trigger RS dialogue)
- Wire to BuildingSpawner.OnBuildingDiscovered (trigger building dialogue)
- Test in Play mode (verify dialogue triggers at correct moments)

**Files:**
- Modify: `Assets/_Project/Scripts/AI/MiloController.cs` (+60 lines)
- Modify: `Assets/_Project/Scripts/Integration/GameLoopController.cs` (wire dialogue triggers)
- Validate: Milo speaks at key story beats, no duplicate triggers

---

## TERTIARY (Polish, Low Risk)

### 7. Save/Load Stress Test (45 minutes, +1 quality point)
**Impact:** Prevent save corruption edge cases  
**Risk:** Low (validation only, no code changes)  
**Tasks:**
- Run automated save/load test script:
  - Create save → modify 10+ systems → save again → load → verify all data persists
  - Test edge cases: save during combat, save during mini-game, save with 0 RS, save with 100 RS
  - Test corruption recovery: manually corrupt save file → load → verify fallback to default
- Check Editor.log for "SaveManager" errors
- Verify ISaveDataProvider implementations return valid JSON

**Files:**
- Create: `Assets/_Project/Scripts/Editor/Tests/SaveLoadStressTest.cs` (Unity Test Framework)
- Validate: 100% test pass rate, no corruption errors in log

---

### 8. Accessibility Test Pass (60 minutes, +2 quality points)
**Impact:** Usability for colorblind, motor-impaired, screen reader users  
**Risk:** Low (validation + minor UI adjustments)  
**Tasks:**
- Enable Unity Accessibility package (Unity 6 built-in)
- Test colorblind modes: Simulate protanopia/deuteranopia/tritanopia in Editor
  - Verify RS gauge colors distinguishable (red/yellow/green)
  - Verify dialogue name colors distinguishable
  - Verify frequency wheel colors distinguishable
- Test motor accessibility:
  - Verify all interactions have hold-duration option (default 0.5s, adjustable to 2.0s)
  - Verify no rapid button mashing required (combat combos use timing windows, not speed)
- Test screen reader:
  - Verify HUDController exposes Accessibility traits for RS/Aether/HP values
  - Verify dialogue text exposes to screen reader API
- Add Accessibility settings panel to MainMenu (colorblind mode toggle, hold duration slider)

**Files:**
- Create: `Assets/_Project/Scripts/UI/AccessibilitySettingsPanel.cs` (Unity UI panel)
- Modify: `Assets/_Project/Scripts/UI/HUDController.cs` (expose Accessibility traits)
- Validate: Colorblind simulator shows distinguishable UI, screen reader reads HUD values

---

### 9. Performance Profiling Deep Dive (90 minutes, +2 quality points)
**Impact:** Identify performance bottlenecks before optimization pass  
**Risk:** Low (data gathering only, no code changes)  
**Tasks:**
- Run Unity Profiler in Play mode for 5 minutes of gameplay
- Capture CPU frame breakdown: identify any >2ms spikes (target: 60 FPS = 16.6ms budget)
- Capture GPU frame breakdown: identify any >5ms draw calls
- Capture memory snapshot: check for GC alloc spikes (target: 0 per-frame allocs)
- Check for common anti-patterns:
  - FindObjectOfType per frame (should be cached)
  - GetComponent per frame (should be cached in Awake)
  - string concatenation in Update (use StringBuilder)
  - Boxing allocations in hot paths (use structs)
- Generate performance report: `Logs/performance_profile_baseline.txt`
- Tag any >5% CPU/GPU functions for optimization in Phase 3

**Files:**
- Create: `Logs/performance_profile_baseline.txt` (Profiler export)
- Validate: Frame budget <16.6ms average, GC allocs <1KB/frame

---

### 10. Audio Mix Polish — Spatial Reverb Zones (60 minutes, +2 quality points)
**Impact:** Environmental audio realism (interiors sound different from exteriors)  
**Risk:** Low (URP Audio Reverb Zones, non-gameplay)  
**Tasks:**
- Add Audio Reverb Zone to Star Dome interior:
  - Reverb Preset: Cathedral (decay time 4.5s, reflections delay 0.07s)
  - Min Distance 10m, Max Distance 50m
  - Volume rolloff: Linear
- Add Audio Reverb Zone to Echohaven exterior:
  - Reverb Preset: Outdoors (decay time 0.5s, minimal reflections)
  - Min Distance 20m, Max Distance 100m
- Test in Play mode:
  - Walk into Star Dome → verify audio gains reverb (echoey cathedral sound)
  - Walk outside → verify audio loses reverb (dry outdoor sound)
  - Verify no audio pops or volume spikes during transitions

**Files:**
- Modify: `Assets/_Project/Scenes/EchohavenStartScene.unity` (add Reverb Zones to Star Dome + scene root)
- Validate: Smooth reverb transitions, no audio artifacts

---

## PHASE 2 SUCCESS CRITERIA

**Visual Quality:** 78/100 → 88/100 (+13%)
- Post-processing bloom + color grading: +8 points
- Fantasy Ruins exterior wrapper: +5 points
- Terrain LOD setup: +4 points
- Animation blend polish: +3 points

**Audio Quality:** 82/100 → 90/100 (+10%)
- Adaptive music layers: +2 points
- Spatial reverb zones: +2 points
- Dialogue emotional payoffs: +3 points

**Performance:** 85/100 → 91/100 (+7%)
- Terrain LOD optimization: +4 points
- Profiler-driven bottleneck identification: +2 points

**Overall:** 81/100 → 89/100 (8-point jump, near-AAA standard)

---

## AUTONOMOUS EXECUTION PLAN

**Batch 1 (High Impact, 2 hours)**
1. Post-processing volume setup (30 min)
2. Fantasy Ruins exterior (45 min)
3. Animation rig polish (45 min)

**Batch 2 (Medium Impact, 3 hours)**
4. Terrain LOD setup (90 min)
5. Adaptive music wiring (60 min)
6. NPC dialogue expansion (60 min)

**Batch 3 (Polish, 2.5 hours)**
7. Save/load stress test (45 min)
8. Accessibility test pass (60 min)
9. Performance profiling (60 min)

**Batch 4 (Audio, 1 hour)**
10. Spatial reverb zones (60 min)

**Total Time:** 8.5 hours of autonomous work  
**Validation:** Run `.\tartaria-play.ps1 -BatchOnly` after each batch (5 min)  
**Human Check-ins:** After Batch 1 (2h mark), Batch 2 (5h mark), Batch 3 (7.5h mark)

---

**ACTIVATION COMMAND:** `auto hammer phase 2` or `continue autonomous upgrades`  
**PREREQUISITE:** Phase 1 runtime test GREEN  
**BLOCKER CHECK:** None (all tasks are additive, no breaking changes)

---

*Dr. Vex Aurelian — Unity 2100 Principal Engine Architect*  
*TARTARIA Phase 2 Autonomous Backlog — Ready for Execution*
