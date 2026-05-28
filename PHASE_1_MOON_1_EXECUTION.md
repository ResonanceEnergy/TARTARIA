# 🚀 PHASE 1 EXECUTION — MOON 1 MAGNETIC CATHEDRAL

**UnityForge Director:** Phase 1 - Core Asset Creation  
**Start Date:** May 28, 2026 (Evening)  
**Target Completion:** May 31, 2026 (3 days)  
**Prerequisites:** Phase 0 tools complete ✅ Play Mode crash fixed ✅

---

## 📋 PHASE 1 OVERVIEW

**Goal:** Create playable Moon 1 foundation with modular cathedral system

**Deliverables:**
1. Moon1_MagneticMoon.unity scene (terrain + lighting)
2. Modular Cathedral Kit (10+ prefab pieces)
3. Master PBR Material System (3 shader graphs)
4. Moon 1 scene layout (buried cathedral 60%, golden hour lighting)
5. Performance baseline (60 FPS target)

**Success Criteria:**
- Player can spawn, walk on terrain, see cathedral partially buried
- Cathedral uses modular pieces (walls, arches, dome, spire)
- Golden hour lighting with amber/dust brown palette
- Aether glow emission on Tartarian geometry
- 60 FPS in Editor, <1500 draw calls, <3GB memory

---

## 🎯 EXECUTION SEQUENCE

### **TASK 1: Create Moon 1 Scene** (10 minutes)

**Prerequisites:** Unity in Edit Mode (NOT Play Mode!)

**Steps:**
1. Press **Ctrl+P** to stop Play Mode if running
2. Wait for Unity to finish recompiling
3. Menu: `Tools → TARTARIA → Build Moon Scene`
4. Fill in:
   - **Moon Number:** 1
   - **Moon Name:** MagneticMoon
   - **Terrain Size:** 500 (meters)
   - **Ambient Color:** Click picker → Golden yellow (#FFA726)
5. Click **"Create Moon Scene"** button

**Expected Output:**
- New scene: `Assets/_Project/Scenes/Moon1_MagneticMoon.unity`
- Scene contains:
  - Terrain (500m × 500m)
  - Directional Light (sun, warm golden color)
  - Reflection Probe (for metallic surfaces)
  - Volume component (URP post-processing) OR warning if URP not installed
  - Main Camera placeholder

**Verify:**
- Scene appears in Project window: `_Project/Scenes/Moon1_MagneticMoon.unity`
- Double-click scene to open it
- Should see flat terrain with golden lighting

---

### **TASK 2: Sculpt Terrain** (30 minutes)

**Context:** Moon 1 = Post-Mud Flood excavation site with buried cathedral

**Tools:**
- Unity Terrain Editor (Inspector → Terrain component)
- Sculpting brushes: Raise/Lower, Smooth, Flatten

**Target Layout:**
```
┌─────────────────────────────────────┐
│  Muddy Plains (rolling hills)       │
│                                      │
│     ╔════════════════╗               │
│     ║   Excavation   ║  ← 50m×50m   │
│     ║      Pit       ║     -20m deep│
│     ║                ║               │
│     ║  [Cathedral]   ║  ← 60% buried│
│     ╚════════════════╝               │
│                                      │
│  Mud mounds • scattered debris       │
└─────────────────────────────────────┘
```

**Steps:**
1. Select Terrain in Hierarchy
2. Inspector → Paint Terrain → Raise or Lower Terrain
3. Create central depression (50m × 50m, 20m deep)
4. Smooth edges with Smooth Height tool
5. Add rolling hills around perimeter (5-10m elevation variance)
6. Create mud mounds (small bumps 2-3m high)

**Materials (temporary):**
- Use default terrain texture for now
- Brown/mud color (#8B7355)
- Will replace with PBR materials in Task 4

---

### **TASK 3: Build Modular Cathedral Kit** (4-5 hours) ⚠️ **CORE WORK**

**Context:** Tartarian sacred-geometry cathedral with golden ratio proportions

**Source Assets:**
- Fantasy Adventure Environment (already imported)
- Kitbash existing architecture pieces
- Apply golden ratio scaling (φ = 1.618)

**Required Pieces (minimum 10):**

#### **Foundation & Walls**
1. **Wall_4x4m.prefab**
   - 4m wide × 6.472m tall (4 × φ)
   - Precision-cut stone texture
   - Modular snap points (0.5m grid)

2. **Wall_Corner_90deg.prefab**
   - L-shaped corner piece
   - Same proportions as wall

3. **Archway_Gothic.prefab**
   - Gothic pointed arch
   - 4m wide × 8m tall (4 × φ²/π)
   - Golden ratio curve

#### **Dome System**
4-11. **Dome_Segment_N.prefab** (8 pieces)
   - Octagonal dome (sacred geometry: 8 = infinity)
   - Each segment: 45° arc
   - 10m diameter base, 5m height at apex
   - Mercury ball connector at apex (Aether focusing sphere)

#### **Spire (Mercury Ball Tower)**
12. **Spire_Base.prefab**
    - Square base: 4m × 4m
    - Height: 4m
    - Mercury ball housing at top

13. **Spire_Mid.prefab**
    - Tapered mid-section
    - Height: 6m
    - Golden ratio taper (4m → 2.472m)

14. **Spire_Top_MercuryBall.prefab**
    - Ornate tip with mercury sphere
    - Height: 16m total (4 × φ³)
    - Ball diameter: 2m
    - Metal (gold/bronze) with high metallic value

#### **Details**
15. **Column_Fluted.prefab**
    - Height: 6.472m (4 × φ)
    - Diameter: 1m
    - Classical fluting (24 grooves)

16. **RoseWindow_Circular.prefab**
    - Diameter: 6.472m (φ² × 2.472)
    - 12 segments (sacred number)
    - Stained glass with golden light

17. **Door_Main_Ornate.prefab**
    - Width: 4m, Height: 6.472m (φ ratio)
    - Precision-cut stone frame
    - Double doors with geometric patterns

18. **Foundation_Block.prefab**
    - Precision-cut megalithic base
    - 4m × 4m × 2m
    - Polygonal fitting (Inca-style)

**Workflow:**
1. Create prefab folder: `Assets/_Project/Prefabs/Moon1/Cathedral/`
2. For each piece:
   - Find suitable mesh from Fantasy Adventure Environment
   - Duplicate and modify in Scene
   - Scale to golden ratio proportions
   - Apply placeholder material
   - Add Box Collider for physics
   - Create prefab in Moon1/Cathedral/
3. Test assembly: Build mini cathedral in scene to verify snap points

---

### **TASK 4: Create Master PBR Materials** (2-3 hours)

**Context:** Shader Graphs for Tartarian visual style across all 13 Moons

**Output Folder:** `Assets/_Project/Materials/Master/`

#### **Material 1: Stone_Tartarian.shadergraph**

**Properties:**
- **BaseColor:** Texture input (albedo map)
- **NormalMap:** Bump detail
- **Roughness:** 0.6-0.8 (weathered stone)
- **Emission:** Golden Aether glow (HDR color, intensity 0.5-2.0)
- **Glow Pattern:** Geometric lines following sacred geometry (optional mask texture)

**Shader Graph Nodes:**
1. Texture Sample (BaseColor) → Multiply(Color) → Main Texture
2. Normal Map node → Normal input
3. Roughness slider → Smoothness
4. Emission: HDR Color × Intensity × Pattern Mask → Emission output
5. Final: Lit Master Node (URP)

**Usage:** Cathedral walls, floors, megalithic foundations

#### **Material 2: Metal_Ornate.shadergraph**

**Properties:**
- **BaseColor:** Gold/Bronze texture
- **NormalMap:** Fine detail (engravings, wear)
- **Metallic:** 0.8-1.0 (pure metal)
- **Roughness:** 0.2-0.4 (polished but aged)
- **Emission:** Subtle glow on edges (rim light effect)

**Shader Graph Nodes:**
1. Texture Sample (BaseColor) → Metallic workflow
2. Normal Map → Micro-detail
3. Metallic = 0.9 (slider)
4. Roughness = 0.3 (slider)
5. Fresnel Effect → Edge glow (optional)
6. Lit Master Node (URP)

**Usage:** Spire mercury balls, ornate details, doors, columns

#### **Material 3: Crystal_Aether.shadergraph**

**Properties:**
- **BaseColor:** Translucent crystal tint
- **Emission:** Pulsing glow (animated with Time node)
- **Transparency:** Alpha blend
- **Fresnel Rim:** Edge glow effect
- **Refraction:** Light bending (optional, performance cost)

**Shader Graph Nodes:**
1. Base Color (transparent blue/white)
2. Emission: HDR Color × Sin(Time × Speed) → Pulsing
3. Fresnel node → Rim intensity
4. Alpha = 0.3-0.6 (transparency)
5. Lit Master Node (Alpha blend, Transparent queue)

**Usage:** Aether crystals, energy fields, dome inner glow

**Moon 1 Variant Materials:**
After creating shader graphs, create material instances:
1. **Stone_Tartarian_Moon1_GoldenHour.mat**
   - Amber base (#FFA726)
   - Golden emission (HDR #FFD700, intensity 1.5)
2. **Metal_Ornate_Moon1_Bronze.mat**
   - Bronze base (#CD7F32)
   - Warm glow
3. **Crystal_Aether_Moon1_Amber.mat**
   - Amber tint (#FFBF00)
   - Slow pulse (0.5 Hz)

---

### **TASK 5: Apply Materials to Cathedral Prefabs** (30 minutes)

**Steps:**
1. Open each cathedral prefab in Prefab Mode
2. Select mesh renderer component
3. Assign appropriate material:
   - Walls/Columns → Stone_Tartarian_Moon1_GoldenHour
   - Spire/Mercury Ball → Metal_Ornate_Moon1_Bronze
   - Dome inner surface → Crystal_Aether_Moon1_Amber (optional glow)
4. Save prefab
5. Repeat for all 18 prefab pieces

**Verify:** Materials show correctly in Scene view with golden hour lighting

---

### **TASK 6: Moon 1 Scene Layout** (2-3 hours)

**Context:** Assemble buried cathedral in excavation pit

**Target Scene:**
```
Camera POV (looking down into pit):

        [Mud Plains]
     ___________________
    /                   \
   /   ╔═══════════╗     \  ← Ground level
  │    ║           ║      │
  │    ║  Cathedral║      │  ← 60% buried
  │    ║  [Visible]║      │     (only top 40% visible)
  │    ║   Spire   ║      │
  │    ║     ↑     ║      │
  │    ╚═══════════╝      │
   \                     /
    \___________________/
         Excavation Pit
```

**Assembly Steps:**

1. **Place Foundation (buried):**
   - Drag 16x Foundation_Block.prefab into scene
   - Arrange in 4×4 grid (16m × 16m base)
   - Position Y = -15m (mostly buried)

2. **Build Walls:**
   - Use Wall_4x4m prefabs to create perimeter
   - 4 walls per side = 16m length per side
   - Add Wall_Corner_90deg at corners
   - Height extends from Y=-15m to Y=-8m (top 7m visible above pit floor)

3. **Add Entrance:**
   - Replace one wall section with Archway_Gothic
   - Place Door_Main_Ornate in archway
   - Add stairs (temporary cubes) leading down from pit edge

4. **Assemble Dome:**
   - Place 8x Dome_Segment_N prefabs in octagon
   - Center at (0, -5m, 0) relative to cathedral base
   - Snap together at 45° intervals
   - Should appear partially visible above mud level

5. **Build Spire (visible landmark):**
   - Spire_Base at dome apex (Y = 0m, at ground level)
   - Spire_Mid stacked on top (Y = 4m)
   - Spire_Top_MercuryBall at apex (Y = 10m, extends 16m above ground)
   - This is the landmark players see from distance!

6. **Detail Pass:**
   - Add Column_Fluted at cathedral corners (4 total)
   - Place RoseWindow_Circular on main facade (facing player spawn)
   - Scatter debris (broken wall chunks) around pit edge
   - Add excavation props (shovels, crates, ropes - use existing assets)

7. **Player Spawn:**
   - Create empty GameObject: "PlayerSpawnPoint"
   - Position at pit edge (X=30m, Y=0m, Z=30m)
   - Tag as "Respawn" (for gameplay system)

---

### **TASK 7: Lighting & Post-Processing** (1 hour)

**Context:** Golden hour atmosphere for Moon 1

**Directional Light (Sun) Settings:**
- **Intensity:** 1.2 (bright but not harsh)
- **Color:** Warm orange (#FFAA66)
- **Rotation:** X=45°, Y=-45° (late afternoon angle)
- **Shadow Type:** Soft Shadows
- **Shadow Resolution:** High (2048)
- **Shadow Distance:** 150m

**Volume Profile (URP):**
1. If Volume component wasn't auto-created, add manually:
   - GameObject → Volume → Global Volume
2. Create Volume Profile asset:
   - Project: Create → Volume Profile
   - Name: "VolumeProfile_Moon1_GoldenHour"
3. Add overrides:
   - **Bloom:**
     - Intensity: 0.3
     - Threshold: 0.9 (only bright emissive materials bloom)
     - Color: Warm gold (#FFD700)
   - **Color Grading:**
     - Temperature: +10 (warmer)
     - Tint: +5 (slight magenta for sunset feel)
     - Saturation: +10 (richer colors)
   - **Vignette:**
     - Intensity: 0.2 (subtle edge darkening)
     - Smoothness: 0.4
     - Color: Dark brown (#3E2723)
4. Assign profile to Volume component

**Ambient Lighting:**
- Window → Rendering → Lighting
- Environment tab:
  - **Source:** Skybox (default) OR HDRI (if downloaded)
  - **Intensity:** 0.8
  - **Ambient Color:** Warm gray (#B8A99A)
- Generate Lighting (bottom button, if needed)

**HDRI Skybox (Optional - if time permits):**
- Download "sunset.exr" from Polyhaven.com (4K, free)
- Import to `Assets/_Project/Textures/Skyboxes/`
- Create → Material, Shader = Skybox/Panoramic
- Assign sunset.exr as Spherical texture
- Lighting Settings → Skybox Material = new skybox
- **Result:** Realistic sunset sky with natural ambient lighting

---

### **TASK 8: Performance Baseline** (30 minutes)

**Context:** Establish 60 FPS target, measure draw calls/memory

**Steps:**
1. **Enable Stats Window:**
   - Game view → Stats button (top-right)
   - Shows FPS, draw calls, triangles, memory

2. **Run Scene in Play Mode:**
   - Press Play (Ctrl+P)
   - Fly camera around cathedral (WASD + mouse)
   - Watch Stats panel

3. **Target Metrics:**
   - **FPS:** 60+ in Editor (will be higher in build)
   - **Draw Calls:** <1500 (URP batches efficiently)
   - **Triangles:** <500K (cathedral should be <100K)
   - **Memory:** <3GB total

4. **If Performance Issues:**
   - **Too many draw calls?**
     - Enable GPU Instancing on materials (checkbox in Material inspector)
     - Use Addressables batching (future phase)
   - **Low FPS?**
     - Reduce shadow distance (Lighting Settings)
     - Lower shadow resolution to Medium
     - Disable Reflection Probe temporarily
   - **High triangle count?**
     - Simplify cathedral meshes (use lower-poly Fantasy Adventure variants)
     - Enable LOD groups (future optimization)

5. **Profile with Unity Profiler:**
   - Window → Analysis → Profiler
   - Record while in Play Mode
   - Check CPU/GPU/Memory tabs
   - Look for red spikes (bottlenecks)

6. **Document Results:**
   - Take screenshot of Stats panel
   - Note FPS/draw calls/memory in production tracker

---

## 📊 PHASE 1 SUCCESS CHECKLIST

- [ ] Moon1_MagneticMoon.unity scene created ✅
- [ ] Terrain sculpted (excavation pit, mud plains)
- [ ] 18 modular cathedral prefabs created
- [ ] 3 master PBR shader graphs created
- [ ] 3 Moon 1 material variants applied
- [ ] Cathedral assembled in scene (60% buried)
- [ ] Golden hour lighting configured
- [ ] URP Volume Profile with Bloom/Color Grading/Vignette
- [ ] Player spawn point placed
- [ ] Performance baseline met (60 FPS, <1500 draw calls)
- [ ] Scene saved and committed to GitHub

---

## 🚀 NEXT STEPS (Phase 2)

After Phase 1 complete:
- **Phase 2:** Moon 2-3 scene creation (reuse modular cathedral kit)
- **Phase 3:** Gameplay integration (player spawn, basic movement, Aether interaction)
- **Phase 4:** Polish & VFX (Aether particles, aurora sky, ambient audio)

**Total Moon 1 Time Estimate:** 8-10 hours (can spread over 3 days)

---

## 📋 TROUBLESHOOTING

**"Cannot Create Scene in Play Mode" error:**
- Press Ctrl+P to stop Play Mode
- Wait for Unity to recompile
- Try again in Edit Mode

**Volume component not working:**
- Check URP package installed: Window → Package Manager → Unity Registry → Universal RP
- If not installed, click Install
- Restart Unity Editor

**Materials not showing correctly:**
- Verify URP pipeline asset assigned: Edit → Project Settings → Graphics → Scriptable Render Pipeline Settings
- Should point to `UniversalRenderPipelineAsset`

**Terrain sculpting not working:**
- Make sure Terrain GameObject selected in Hierarchy
- Inspector → Terrain component → Paint Terrain tools should appear

**Low FPS in Editor:**
- Normal! Editor has overhead. Standalone build will be 2-3x faster.
- Target 30-40 FPS in Editor = 60+ in build

---

**Phase 1 Ready to Execute!** 🎬