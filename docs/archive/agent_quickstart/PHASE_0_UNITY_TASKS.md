# 🔨 PHASE 0 EXECUTION — UNITY EDITOR TASKS

**Status:** Code/scripts complete ✅ — Unity Editor execution required  
**Completion Target:** June 7, 2026 (9 days remaining)  
**Current Progress:** 15% → Target 100%

---

## ⚡ CRITICAL PATH — DO THESE FIRST (1-2 hours)

### **STEP 1: Open Unity Project**
```
Location: C:\dev\TARTARIA_new
Unity Version: Unity 6000.3.6f1
```

**Expected:** Unity will recompile new Editor scripts (3 files added)
- AddressablesConfigurator.cs
- MoonSceneBuilder.cs
- AssetInventoryTool.cs

**Verify:** Check Console for compilation success (no errors)

---

### **STEP 2: Execute AssetWiringTool** ⚠️ **CRITICAL - DO FIRST**

**Menu:** `Tools → TARTARIA → Wire Assets Automatically`

**What it does:**
- Wires 220 Interactive Objects with door sounds, breakable VFX
- Wires 88 NPC Dialogues with talk sounds, dialogue UI
- Wires 110 Power-Ups with collect sounds, buff VFX
- Wires 165 Enemy Spawners with spawn VFX, portal effects
- Wires 55 Environmental Secrets with discovery VFX, reveal sounds

**Expected Duration:** 5-10 minutes  
**Output:** Console logs showing "✅ Wired [N] objects in [Scene]"

**Why Critical:** All existing gameplay systems need these asset connections before scene building

---

### **STEP 3: Run Asset Inventory Tool**

**Menu:** `Tools → TARTARIA → Generate Asset Inventory`

**What it does:**
- Scans ALL imported assets (models, prefabs, materials, textures, audio)
- Generates comprehensive inventory report
- Creates Moon 1-3 gap analysis (identifies missing assets)

**Output File:** `docs/ASSET_INVENTORY_FULL.md`

**Expected Duration:** 2-3 minutes  
**Result:** Know exactly what assets we have vs. need for Moon 1-3

---

### **STEP 4: Create Moon 1 Scene**

**Menu:** `Tools → TARTARIA → Build Moon Scene`

**Settings:**
- Moon Number: `1`
- Moon Name: `MagneticMoon`
- Terrain Size: `500` (meters)
- Ambient Color: Golden hour (default: RGB 255, 204, 153)

**Click:** "Create Moon Scene"

**What it creates:**
- Scene file: `Assets/_Project/Scenes/Moon1_MagneticMoon.unity`
- Terrain (500m × 500m, flat)
- Directional Light (sun at 50° elevation, -30° azimuth)
- Reflection Probe (baked, 500m coverage)
- Global Volume (URP post-processing)
- Main Camera (positioned 50m up, looking at terrain center)

**Expected Duration:** 1 minute  
**Result:** Playable (but empty) Moon 1 scene

---

## 🎨 PHASE 0 COMPLETION TASKS (6-8 hours)

### **STEP 5: Configure Addressables** (30 minutes)

**Menu:** `Tools → TARTARIA → Configure Addressables for Moons`

**Follow dialog instructions:**
1. Install `com.unity.addressables` via Package Manager
2. Open: `Window → Asset Management → Addressables → Groups`
3. Create 16 groups:
   - `Moon1_Assets` through `Moon13_Assets` (labels: moon1-moon13, LoadMode: Explicit)
   - `SharedArchitecture` (label: shared, LoadMode: Cached)
   - `SharedMaterials` (label: materials, LoadMode: Cached)
   - `SharedVFX` (label: vfx, LoadMode: Cached)

**Menu (after setup):** `Tools → TARTARIA → Generate Addressables Report`  
**Output:** `docs/ADDRESSABLES_STRUCTURE.md` (documentation)

---

### **STEP 6: Build Moon 1 Cathedral Kit** (4-5 hours)

**Location:** `Assets/_Project/Prefabs/Moon1/`

**Source Assets:** `Assets/Fantasy Adventure Environment/`

**Modular Pieces to Create (minimum 10):**
1. `Cathedral_Wall_4x4m.prefab` (clean, muddy, damaged variants)
2. `Cathedral_Archway.prefab`
3. `Cathedral_Dome_Segment.prefab` (8 pieces → full dome)
4. `Cathedral_Spire_Base.prefab`
5. `Cathedral_Spire_Mid.prefab`
6. `Cathedral_Spire_Top_MercuryBall.prefab`
7. `Cathedral_Column.prefab`
8. `Cathedral_Window_Rose.prefab`
9. `Cathedral_Door_Main.prefab`
10. `Cathedral_Foundation.prefab`

**Golden Ratio Proportions:**
- Base width: 4m
- Height: 6.472m (4 × φ)
- Dome radius: 10m
- Spire height: 26m (4 × φ³)

**Materials:** Use Fantasy Adventure Environment materials as base, add golden glow emission

---

### **STEP 7: Create Master PBR Materials** (2-3 hours)

**Location:** `Assets/_Project/Materials/Master/`

**Create 3 Shader Graphs:**

1. **Stone_Tartarian.shadergraph**
   - Inputs: BaseColor, Normal, Roughness, Emission (for Aether glow)
   - Properties: ColorTint (for Moon variants), EmissionStrength, GoldenRatioDecal
   - Output: URP/Lit

2. **Metal_Ornate.shadergraph**
   - Inputs: BaseColor, Normal, Metallic, Roughness
   - Properties: ColorTint, Metallic (0.8-1.0), Roughness (0.2-0.4)
   - Output: URP/Lit

3. **Crystal_Aether.shadergraph**
   - Inputs: BaseColor, Normal, Emission, Transparency
   - Properties: ColorTint, EmissionStrength, FresnelPower
   - Special: Fresnel rim lighting, pulsing emission (time-based)
   - Output: URP/Lit (Transparent)

**Create 13 Material Instances per master = 39 total:**
- `Stone_Tartarian_Moon1_GoldenHour.mat`
- `Stone_Tartarian_Moon2_Emerald.mat`
- `Stone_Tartarian_Moon3_ColdBlue.mat`
- _(etc. for all 13 Moons × 3 master materials)_

---

### **STEP 8: Moon 1 Scene Layout** (2-3 hours)

**Open Scene:** `Moon1_MagneticMoon.unity`

**Layout Plan:**
1. **Terrain Sculpting**
   - Center: Cathedral excavation pit (50m × 50m, 10m deep)
   - Surround: Mud plains (brown terrain texture)
   - Edges: Distant hills (subtle variation)

2. **Place Cathedral**
   - Position: Terrain center (250m, -6m, 250m) — buried 60%
   - Assembly: Use modular kit pieces
   - State: Partially restored (some clean stone, some mud-covered)

3. **Lighting Setup**
   - Sun: 50° elevation, warm golden color
   - Ambient: Flat mode, golden tint, 0.3 intensity
   - Skybox: Polyhaven "sunset" HDRI (download from polyhaven.com)

4. **Volume Profile**
   - Bloom: Threshold 1.0, Intensity 0.3 (for Aether glow)
   - Color Grading: Warm LUT, +5 exposure
   - Vignette: Subtle (0.2 intensity)

5. **Props**
   - Giant skeleton (KayKit Skeletons scaled 3× + custom skull)
   - Mud piles (KayKit Forest rocks + brown material)
   - Excavation tools (KayKit RPG Tools props)

**Playtesting:**
- Press Play
- WASD to move, Mouse to look
- Verify 60 FPS, no errors in Console

---

### **STEP 9: Download Free Assets** (1 hour)

**Polyhaven HDRIs (13 skyboxes):**
- Visit: polyhaven.com/hdris
- Download (4K EXR format):
  - `sunset.exr` (Moon 1 - golden hour)
  - `moonlit_golf.exr` (Moon 2 - green moonlight)
  - `rainy_night.exr` (Moon 3 - cold blue)
  - `sunflowers.exr` (Moon 5 - white marble)
  - _(find/download 9 more matching Moon palettes)_

**Mixamo Characters (4 humanoids):**
- Visit: mixamo.com (requires free Adobe account)
- Download FBX with T-pose + animations:
  - "Adventurer" male → Player base
  - "Queen" female → Anastasia base
  - "Worker" male → Milo base
  - "Knight" male → Cassian base

**Import to Unity:**
- Drag HDRIs to `Assets/_Project/Textures/Skyboxes/`
- Drag Mixamo FBX to `Assets/_Project/Models/Characters/`

---

## ✅ PHASE 0 COMPLETION CHECKLIST

Mark complete when ALL items checked:

**Asset Integration:**
- [ ] AssetWiringTool executed successfully (638 objects wired)
- [ ] Addressables groups configured (16 total)
- [ ] Fantasy Adventure Environment explored and documented

**Scene Foundation:**
- [ ] Moon1_MagneticMoon.unity created and playable
- [ ] Terrain sculpted (excavation pit + mud plains)
- [ ] Cathedral modular kit assembled (minimum 10 pieces placed)

**Material System:**
- [ ] 3 master Shader Graphs created (Stone, Metal, Crystal)
- [ ] 13 material instances created for Moon 1 (from 3 masters)

**Documentation:**
- [ ] Asset inventory report generated (`docs/ASSET_INVENTORY_FULL.md`)
- [ ] Addressables structure documented (`docs/ADDRESSABLES_STRUCTURE.md`)
- [ ] Production tracker updated (Phase 0 → 100%)

**Performance Baseline:**
- [ ] Moon 1 scene plays at 60 FPS in Editor
- [ ] Console shows 0 errors, <10 warnings
- [ ] Profiler shows <1500 draw calls, <3GB memory

---

## 🎬 DIRECTOR'S NOTES

**You've now built the foundation.** These tools and workflows will accelerate Moon 2-13 development 10×.

**Key Principles to Maintain:**
1. **Modular everything** — Every wall/door/window must be reusable prefab
2. **Material instances** — Never duplicate master materials, create instances
3. **Golden ratio** — Use φ (1.618) for ALL architectural proportions
4. **Addressables hygiene** — Moon-specific assets go in Moon# groups, shared assets in Shared groups

**Phase 0 Success = Moon 1 Playable + Tools Working**

Once you execute these steps, you'll have:
- 1 complete Moon scene (template for remaining 12)
- Modular building system (reusable for all Moons)
- Material pipeline (recolor for Moon 2-13)
- Asset inventory (know exactly what to buy/build next)

**Phase 1 starts June 8:** Moon 2-3 batch (21 days to complete both)

---

**Questions? Issues? Progress updates?**  
Tag me with your Unity Console screenshots or scene screenshots.

**— UnityForge Director** 🎬
