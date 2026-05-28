# CATHEDRAL PREFAB CREATION CHECKLIST
**Location:** Assets/_Project/Prefabs/Moon1/Cathedral/

## ✅ FOUNDATION & WALLS (4 prefabs)

### [ ] 1. Wall_4x4m.prefab
- **Dimensions:** 4m × 6.472m × 0.5m (W × H × D)
- **Source:** Fantasy Adventure Environment stone wall
- **Material:** Stone_Tartarian_Moon1_GoldenHour
- **Collider:** Box Collider (4 × 6.472 × 0.5)
- **Pivot:** Bottom-center
- **Snap Points:** Left/Right edges at 0.5m intervals
- **Sacred Geometry:** Height = Base × φ (1.618)

### [ ] 2. Wall_Corner_90deg.prefab  
- **Dimensions:** 4m × 6.472m (L-shape, 90° angle)
- **Source:** Combine 2x Wall_4x4m at 90° or kitbash corner piece
- **Material:** Stone_Tartarian_Moon1_GoldenHour
- **Collider:** 2x Box Colliders (one per wall section)
- **Pivot:** Inner corner at base
- **Snap Points:** Outer edges align with Wall_4x4m grid

### [ ] 3. Archway_Gothic.prefab
- **Dimensions:** 4m wide × 8m tall × 1m deep
- **Source:** Fantasy Adventure archway or custom curve
- **Material:** Stone_Tartarian_Moon1_GoldenHour (with Aether glow on arch curve)
- **Collider:** Mesh Collider (concave) OR multiple box colliders
- **Pivot:** Base-center (ground level)
- **Sacred Geometry:** Height = Base × φ² ÷ π

### [ ] 4. Foundation_Block.prefab
- **Dimensions:** 4m × 2m × 4m (W × H × D)
- **Source:** Fantasy Adventure megalithic base OR scaled cube with precision-cut texture
- **Material:** Stone_Tartarian_Moon1_GoldenHour (darker variant for underground)
- **Collider:** Box Collider (4 × 2 × 4)
- **Pivot:** Bottom-center
- **Polygonal Fit:** Slight chamfer on edges (Inca-style) - Optional visual detail

---

## ✅ DOME SYSTEM (8 prefabs)

### [ ] 5-12. Dome_Segment_1 through Dome_Segment_8.prefab
- **Dimensions:** 12.944m diameter dome ÷ 8 segments = 1.618m arc width each
- **Height:** 4m from base to apex
- **Source:** Model in Blender/Unity OR kitbash arched pieces
- **Material:** 
  - Outer: Stone_Tartarian_Moon1_GoldenHour
  - Inner (optional): Crystal_Aether_Moon1_Amber (glowing interior)
- **Collider:** Mesh Collider (convex) per segment
- **Pivot:** Center base (where all 8 segments meet)
- **Assembly:** 
  - Each segment rotates 45° from previous (360° ÷ 8)
  - Snap together at apex with mercury ball connector
- **Sacred Geometry:** Octagon = symbol of infinity (∞)

**Assembly Order:**
1. Place Dome_Segment_1 at (0, 0, 0) facing North
2. Duplicate and rotate 45° clockwise → Dome_Segment_2
3. Repeat until all 8 segments form complete octagonal dome
4. Mercury ball sits at apex (0, 4m, 0)

---

## ✅ SPIRE (3 prefabs)

### [ ] 13. Spire_Base.prefab
- **Dimensions:** 4m × 4m × 4m (square base with tapered top)
- **Source:** Fantasy Adventure tower base OR custom model
- **Material:** Stone_Tartarian_Moon1_GoldenHour (base) + Metal_Ornate_Moon1_Bronze (crown)
- **Collider:** Box Collider (4 × 4 × 4)
- **Pivot:** Bottom-center
- **Details:** Mercury ball housing at top (2m diameter socket)

### [ ] 14. Spire_Mid.prefab
- **Dimensions:** 4m base tapering to 2.472m top × 6.472m height
- **Source:** Custom tapered cylinder OR scaled cone
- **Material:** Metal_Ornate_Moon1_Bronze (polished, high metallic)
- **Collider:** Mesh Collider (convex)
- **Pivot:** Bottom-center
- **Sacred Geometry:** Taper ratio = φ⁻¹ (0.618)

### [ ] 15. Spire_Top_MercuryBall.prefab
- **Dimensions:** 
  - Shaft: 2.472m base × 10.472m height (ornate tip)
  - Mercury Ball: 2m diameter sphere
- **Source:** 
  - Shaft: Fantasy Adventure spire OR custom model
  - Ball: Unity Sphere primitive scaled to 2m
- **Material:** 
  - Shaft: Metal_Ornate_Moon1_Bronze
  - Ball: Metal_Ornate_Moon1_Bronze (Metallic = 1.0, Smoothness = 0.9)
- **Collider:** 
  - Shaft: Capsule Collider
  - Ball: Sphere Collider (radius 1m)
- **Pivot:** Bottom-center (base of shaft)
- **VFX (Optional):** Add Point Light at ball apex (golden glow, intensity 2.0, range 50m)

---

## ✅ DETAILS (3 prefabs)

### [ ] 16. Column_Fluted.prefab
- **Dimensions:** 1m diameter × 6.472m height
- **Source:** Fantasy Adventure column OR Unity Cylinder with carved grooves
- **Material:** Stone_Tartarian_Moon1_GoldenHour
- **Collider:** Capsule Collider (radius 0.5m, height 6.472m)
- **Pivot:** Bottom-center
- **Details:** 24 vertical flutes (classical Greek style)
- **Sacred Geometry:** Height = Base × φ

### [ ] 17. RoseWindow_Circular.prefab
- **Dimensions:** 6.472m diameter × 0.1m thick
- **Source:** Fantasy Adventure window frame OR custom 12-segment radial pattern
- **Material:** 
  - Frame: Stone_Tartarian_Moon1_GoldenHour
  - Glass (inner): Crystal_Aether_Moon1_Amber (Transparent, Emission glow)
- **Collider:** Mesh Collider (concave, NOT walkable - trigger only)
- **Pivot:** Center
- **Sacred Geometry:** 12 segments (sacred number, 12 Moons)
- **Placement:** Facade wall at height 3m from ground

### [ ] 18. Door_Main_Ornate.prefab
- **Dimensions:** 4m wide × 6.472m tall × 0.3m deep (double doors)
- **Source:** Fantasy Adventure door OR kitbash 2x door panels
- **Material:** 
  - Frame: Stone_Tartarian_Moon1_GoldenHour
  - Doors: Metal_Ornate_Moon1_Bronze (with geometric engravings)
- **Collider:** Box Collider (4 × 6.472 × 0.3)
- **Pivot:** Bottom-center (between doors)
- **Animation:** Hinge on left/right edges (for opening - Phase 2)
- **Sacred Geometry:** φ ratio door (4m × 6.472m = 1.618)

---

## 🎨 MATERIAL ASSIGNMENT REFERENCE

| Prefab | Primary Material | Secondary Material | Emission |
|--------|------------------|-------------------|----------|
| Walls, Columns, Foundation | Stone_Tartarian_Moon1_GoldenHour | - | Golden glow lines |
| Dome Outer | Stone_Tartarian_Moon1_GoldenHour | - | Geometric patterns |
| Dome Inner | Crystal_Aether_Moon1_Amber | - | Pulsing glow |
| Spire Base | Stone_Tartarian_Moon1_GoldenHour | Metal_Ornate (crown) | Subtle rim |
| Spire Mid/Top | Metal_Ornate_Moon1_Bronze | - | None |
| Mercury Ball | Metal_Ornate_Moon1_Bronze | - | Point Light |
| Door Frame | Stone_Tartarian_Moon1_GoldenHour | - | Border glow |
| Door Panels | Metal_Ornate_Moon1_Bronze | - | Engraving highlights |
| Rose Window Frame | Stone_Tartarian_Moon1_GoldenHour | - | None |
| Rose Window Glass | Crystal_Aether_Moon1_Amber | - | Strong emission |

---

## 🏗️ ASSEMBLY TEST (Unity Editor)

After creating all 18 prefabs:

1. **Create Test Scene:** `Assets/_Project/Scenes/CathedralTest.unity`
2. **Snap Grid:** Edit → Snap Settings → Move: 0.5m, Rotate: 45°
3. **Build Foundation:**
   - Place 16× Foundation_Block in 4×4 grid (16m × 16m base)
4. **Raise Walls:**
   - 12× Wall_4x4m around perimeter (3 per side)
   - 4× Wall_Corner_90deg at corners
   - 1× Archway_Gothic on main facade (replace wall)
   - 1× Door_Main_Ornate in archway
5. **Assemble Dome:**
   - Place 8× Dome_Segment in octagonal pattern
   - Rotate each 45° from previous
   - Check seams align perfectly
6. **Build Spire:**
   - Spire_Base on dome apex
   - Spire_Mid stacked on base
   - Spire_Top_MercuryBall at top
7. **Add Details:**
   - 4× Column_Fluted at corners
   - 1× RoseWindow_Circular on facade (above door)

**Expected Result:** Complete modular cathedral, 16m × 16m footprint, 20.944m total height

---

## ⚙️ TECHNICAL REQUIREMENTS

- **LOD Groups:** Not needed for Phase 1 (add in Phase 3 optimization)
- **Lightmap UVs:** Auto-generate in prefab settings (check "Generate Lightmap UVs")
- **Collision Layers:** Default (Layer 0) OR create custom "Architecture" layer
- **Static Flags:** Check "Lightmap Static" and "Reflection Probe Static" for all prefabs
- **Pivot Points:** CRITICAL - All must snap on 0.5m grid for modular assembly
- **Naming Convention:** PascalCase with underscores (Wall_4x4m, NOT wall_4x4m or Wall4x4m)

---

## 🎯 SUCCESS CRITERIA

- [ ] All 18 prefabs created and saved in `Assets/_Project/Prefabs/Moon1/Cathedral/`
- [ ] Each prefab has correct dimensions per Cathedral_Measurements.csv
- [ ] Materials assigned (Stone/Metal/Crystal variants)
- [ ] Colliders configured (walkable surfaces)
- [ ] Prefabs snap together on 0.5m grid with no gaps
- [ ] Test cathedral assembles in <5 minutes
- [ ] Total triangle count <100K for all 18 prefabs combined
- [ ] Golden ratio proportions maintained (verify with GoldenRatioCalculator.ps1)

**Time Estimate:** 4-5 hours for all 18 prefabs (first-time creation)  
**Future Reuse:** These prefabs work for Moon 2-13 with only material swaps! 🎨