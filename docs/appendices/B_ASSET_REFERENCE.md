# TARTARIA WORLD OF WONDER — Appendix B: Asset Reference & Art Direction
## Visual Reference Boards, Material Palettes, Architecture Templates & Asset Specifications

---

> *Every pixel must serve the wonder. Every texture must whisper "this was real." The Tartarian aesthetic is not fantasy — it is hidden history rendered with reverence.*

**Cross-References:**
- [04_ARCHITECTURE_GUIDE.md](../04_ARCHITECTURE_GUIDE.md) — Zone layouts, building types, sacred geometry rules
- [12_VIVID_VISUALS.md](../12_VIVID_VISUALS.md) — Key visual moment cinematography
- [09_TECHNICAL_SPEC.md](../09_TECHNICAL_SPEC.md) — Unity 6 rendering pipeline, Metal 3, memory budgets

---

## Table of Contents

1. [Art Direction Pillars](#1-art-direction-pillars)
2. [Master Color Palette](#2-master-color-palette)
3. [Architecture Asset Templates](#3-architecture-asset-templates)
4. [Character & Companion Art Guide](#4-character--companion-art-guide)
5. [Environment Art: Zone Styles](#5-environment-art-zone-styles)
6. [VFX & Particle Systems](#6-vfx--particle-systems)
7. [UI Art Direction](#7-ui-art-direction)
8. [Material & Texture Standards](#8-material--texture-standards)
9. [Lighting Design Guide](#9-lighting-design-guide)
10. [Camera & Cinematography](#10-camera--cinematography)
11. [Asset Production Pipeline](#11-asset-production-pipeline)
12. [Real-World Visual References](#12-real-world-visual-references)

---

## 1. Art Direction Pillars

### Core Visual Philosophy
**Hyper-detailed ornate realism with fantastical glow.** Ground every element in photographic truth — then add living Aether veins, floating crystals, giant-scale proportions, and aurora-lit skies.

### The Four Pillars

| Pillar | Principle | Visual Expression |
|---|---|---|
| **Grounded Grandeur** | Real architectural details at impossible scale | Hand-carved column capitals on 100-foot pillars; precision mortarless joints on megaliths; patinated copper domes 300 feet across |
| **Living Architecture** | Buildings breathe, pulse, respond | Aether veins pulse along stone seams; domes glow brighter as RS increases; bell towers physically vibrate when rung |
| **Atmospheric Wonder** | Air itself is alive with energy | Aurora curtains, ley-line rivers of light, ionized mist, Aether fireflies, golden dust motes in sunbeams |
| **Emotional Contrast** | Beauty deepened by loss | Mud-buried grandeur vs. restored splendor; broken organ vs. thundering symphony; gray present vs. golden memory |

### Scale Reference

| Element | Human | Giant | Mega-Structure |
|---|---|---|---|
| **Height** | 1.8m (player) | 7.5m (Korath) | 100m+ (cathedral dome) |
| **Door Height** | 2.5m | 10m | 30m (processional gate) |
| **Column Diameter** | 0.5m | 2m | 5m (load-bearing) |
| **Step Height** | 0.2m | 0.8m | 2m (giant staircase) |

---

## 2. Master Color Palette

### Aether Band Colors

| Band | Primary Color | Secondary | Emission Intensity | Hex Reference |
|---|---|---|---|---|
| **3-Band** | Electric blue-white | Ice blue | Moderate pulse | `#B8D4FF` / `#E0F0FF` |
| **6-Band** | Warm gold-green | Honeyed amber | Steady warm glow | `#FFD700` / `#C5A63D` |
| **9-Band** | Violet-aurora | Shifting magenta-teal | Intense shimmer | `#9B59B6` / `#00CED1` |
| **Corrupted** | Sickly black-green | Dark red veins | Irregular throb | `#1A1A1A` / `#4A0000` |
| **Transcendent** | Pure white-gold | All bands shifting | Overwhelming radiance | `#FFFDE7` / `#FFD54F` |

### Zone State Palettes

**Buried / Unrestrored**
- Dominant: Mud browns (#5D4037, #795548), slate grays (#607D8B, #455A64)
- Accent: Faint Aether glow through cracks (3-Band blue at 10% intensity)
- Atmosphere: Overcast, desaturated, dust particles in air
- Mood: Melancholy, hidden beauty, potential

**Partially Restored (25-75% RS)**
- Dominant: Stone cream (#F5F5DC), copper patina (#B87333), marble white (#FAFAFA)
- Accent: Growing Aether veins (6-Band gold at 40% intensity), first green vegetation
- Atmosphere: Clearing skies, occasional sun breaks, warm tones emerging
- Mood: Hope, discovery, warming

**Fully Restored (75-100% RS)**
- Dominant: Gleaming gold (#FFD700), Living marble (#FFF8E1), Polished copper (#DA8A67)
- Accent: Full Aether spectrum, lush vegetation, crystal accents, rainbow mist from fountains
- Atmosphere: Golden-hour perpetual, aurora active, air shimmering with energy
- Mood: Triumph, wonder, living paradise

**Night Cycle**
- Stronger Aether glow — ley lines as liquid starlight rivers
- Domes pulse with internal light (bioluminescent quality)
- Auroras tied to grid resonance percentage
- Stars visible through gaps in aurora — 13-Moon astronomical accuracy

### Companion Color Language

| Companion | Signature Color | Meaning |
|---|---|---|
| **Milo** | Russet orange (#D2691E) | Earthy, grounded, mercantile |
| **Lirael** | Silver-blue shimmer (#C0C0C0 shifting to #87CEEB) | Spectral, musical, evolving |
| **Thorne** | Deep navy + brass (#1B3A5C, #B8860B) | Military, sky, command |
| **Korath** | Amber-stone (#DAA520 fading to #FFD700) | Ancient, warm, sacrificial |
| **Cassian** | Charcoal with red flickers (#2F4F4F, #DC143C) | Ambiguous, conflicted, dangerous |
| **Zereth** | Corrupted violet → healed aurora (#4B0082 → #7FFFD4) | Tormented → transcendent |
| **Veritas** | Ivory + crystal (#FFFFF0, #E0E0E0) | Pure, precise, musical |

---

## 3. Architecture Asset Templates

### Cathedral / Dome Complex

**Components (Modular Kit):**
- **Foundation Ring:** Octagonal base, 8 anchor points for ley-line connections
- **Column Array:** Corinthian capitals with Aether-vine motif (4 LOD levels)
- **Dome Shell:** Double-shell with internal light cavity for Aether glow (outer: copper patina, inner: gold leaf)
- **Rose Window:** 12-petal cymatic pattern — procedural shader responds to RS level
- **Spire / Finial:** Mercury-ball topped, conductive antenna design — emission point for ley-line connection
- **Pipe Organ:** Interior asset, 5 registers × 12 notes = 60 visible pipes (bass pipes extend through roof)
- **Bell Tower (Campanile):** Attached or separate, 1-12 bell slots, scalar wave emission ring at top
- **Pure Water Font:** Carved stone basin with perpetual-motion water flow (VFX when active)
- **Nave:** Central hall, giant-scale dimensions (15m ceiling minimum), processional layout
- **Apse:** Half-dome east end with concentrated Aether focus point

**Restoration States (per module):**
1. **Buried:** 90% submerged in mud, only finial tips or window arches visible
2. **Excavated:** Mud cleared, structure revealed — cracked, vine-covered, weathered
3. **Structural:** Walls repaired, columns straightened — clean but dark, no Aether
4. **Tuned:** Aether flowing, veins glowing, windows projecting — alive but not maximized
5. **Perfected:** Full RS, golden-hour glow, music audible, fountains active, living architecture

### Star Fort

**Components:**
- **Bastion Points:** 6-pointed star geometry (hexagonal symmetry), each point mounts a resonance cannon
- **Curtain Walls:** Precision-cut stone, golden-ratio proportioned, Aether-conductive mortar
- **Moat:** Pure water channel — conductive barrier, resonance amplifier, visual reflections
- **Central Tower:** Bell tower + clock tower hybrid (17-hour dial on all four faces)
- **Gate Complex:** Giant-scale processional entrance with acoustic chamber properties
- **Parade Ground:** Interior open space with golden-ratio grid overlay in stonework

### Resonance Train

**Components:**
- **Crystal Rail Segments:** Translucent Aether-conductive material, emits golden glow when tuned
- **Engine Car:** Mercury-orb anti-grav drive, sacred-geometry chassis, crystal viewing windows
- **Passenger Cars:** Giant-scale interior (humans and giants ride together), crystal-pane windows
- **Caboose:** Observation platform, oversized (Thorne-sized), ornate railing
- **Station Platform:** Copper-inlaid stone, crystal canopy, waiting hall with giant-scale benches

### Airship

**Components:**
- **Hull:** Sacred-geometry frame — visible golden-ratio ribs through translucent outer skin
- **Mercury Orb Engines:** 4 orbs in nacelles, visible liquid mercury swirling inside crystal spheres
- **Bridge:** Giant-scale command deck, crystal navigation console, compass rose inlaid floor
- **Cargo Bay:** Megalith cradle with anti-grav clamps, loading ramp
- **Resonance Sails:** Crystal lattice wings that catch Aether currents (procedural animation)
- **Beacon Mast:** Ley-line communication antenna at the bow

---

## 4. Character & Companion Art Guide

### Player Character

**Design Principles:**
- Gender-neutral default with customization options
- Modern clothing that gradually transforms: starts in jeans/jacket → gains Tartarian elements (golden embroidery, Aether-thread accents, crystal accessories) as the game progresses
- Aether bloodline glow: faint golden veins appear on hands/arms, intensify with ability use
- Giant-mode transformation: proportional scale-up, clothing adapts (magical expansion), eyes glow solid gold

### Milo (Fox Spirit)

**Visual Design:**
- Russet-orange fox with unusually bright amber eyes and slightly too-intelligent expression
- Fur has a faint shimmer — not fully physical, slight spectral edge
- Size: Large fox (waist-height to player in human form, ankle-height in giant form)
- Wears a single piece of swag: a tiny brass compass on a leather cord (stolen, obviously)
- Animation personality: quick, darting movements, ears constantly swiveling, tail expressively flicking
- Emotional tells: ears flatten (scared), tail puffs (startled), sits very still (genuinely moved)

### Lirael (Spectral Architect)

**Visual Design — Progressive Manifestation:**
- **Moon 1-3:** Translucent silver-blue, features indistinct, edges dissolving into sparkles, voice echoes
- **Moon 4-7:** More defined features, warm tones emerging through the silver, can hold shape for minutes
- **Moon 8-11:** Nearly solid, features clear (elegant, ageless, sad eyes), clothes visible (flowing Tartarian gown with harmonic notation embroidered in gold), slight shimmer at edges
- **Moon 12-13:** Fully manifest — solid, warm, silver hair with gold streaks, clear blue eyes, voice carries without spectral distortion, can physically interact with the world

### Captain Thorne

**Visual Design:**
- Tall, weathered, military bearing — a man who has been alone too long
- Tartarian naval uniform: deep navy coat with brass buttons embossed with sacred geometry, gold epaulettes tarnished by centuries
- Aviator goggles permanently on forehead (Mercury-lens — lets him see Aether currents)
- Hands: callused, oil-stained, competent — the hands of someone who maintains a ship alone for 200 years
- Age: physically 50s but eyes carry millennia, silver-streaked dark hair, close-trimmed beard

### Korath the Builder

**Visual Design:**
- 25 feet tall, proportioned like a broad-shouldered craftsman, not a warrior
- Skin: warm sandstone color with visible golden Aether veins running beneath the surface
- Face: ancient, kind, deeply lined, eyes like liquid amber — enormous gentleness
- Clothing: Simple stone-cutter's tunic (enormous), tool belt with crystal hammers and golden measuring rods
- Hands: massively detailed — each finger as thick as the player's arm, scarred from millennia of construction, but precise and delicate in movement
- Death/sacrifice VFX: dissolves into golden light particles that flow into the bell tower, leaving behind an echo outline

### Cassian

**Visual Design:**
- Handsome, scholar's build, always impeccably dressed (Victorian-influenced but with subtle Reset symbols the observant player can spot)
- Constantly adjusting something — cufflinks, collar, hair — subtle nervous tell
- Eyes: warm brown that flash red-black when he's lying (blink-and-miss detail)
- **Redeemed:** Clothes become simpler, hands get dirty, Reset symbols fade to scars
- **Purged:** Ghost-echo version — charcoal outline with red flickers, face frozen in moment of defeat

### Zereth (The Dissonant One)

**Visual Design — Progressive Reveal:**
- **Moons 1-9:** Shadow only — dark shimmer at vision edges, silhouette on star fort in prophecies
- **Moon 10-12:** Corrupted form visible — giant-scale like Korath but twisted, violet-black Aether crackling around him, face obscured by dissonance distortion
- **Moon 13 (corrupted):** Full reveal — a tormented giant, clearly related to Korath (same bone structure), Aether veins are dark purple instead of gold, eyes burning violet with pain not malice
- **Moon 13 (healed):** Transformation — violet fades to aurora teal-green, scars remain as silver lines, eyes clear to gentle lavender, expression shifts from agony to profound relief

---

## 5. Environment Art: Zone Styles

### New Chicago Underground (Moon 1)

- **Above ground:** Modern urban decay — concrete, rebar, parking lots, utterly mundane
- **Below ground:** Revelation — precision-cut marble behind inches of dried mud, sunken windows staring at dirt walls, half-buried spires barely protruding, ornate iron gates frozen mid-swing by solidified mud
- **Lighting:** Harsh fluorescent above transitioning to warm Aether glow below
- **Key detail:** Mud layers contain artifacts (newspaper fragments, broken tools, children's toys) — environmental storytelling of the burial

### Sunken Cathedral Sanctum (Moon 6)

- **Impossible scale:** Cathedral-within-cathedral geometry — fractal vaulting where every interior surface contains a smaller version of the whole
- **Acoustic visualization:** Sound waves visible as golden ripples in the air — singing/organ playing creates expanding rings of light
- **Water presence:** Standing water on the floor reflects everything with crystalline clarity — slightly too-perfect reflections (the water remembers what the building looked like)
- **Organ dominance:** 32-foot bass pipes extend through the roof into open sky — visible from outside the zone

### Floating Sky Isles (Moon 8)

- **Impossible architecture:** Inverted structures — buildings hanging from the underside of floating landmasses, root systems of enormous crystal trees serving as structural support
- **Open sky everywhere:** No horizon occlusion — the world curves away visibly, cloud layers below and above
- **Wind VFX:** Constant atmospheric motion — banners, flags, Aether streamers, crystal chimes tinkling
- **Airship integration:** Docking towers rise from island edges, landing platforms with anti-grav guide lights

### Echo Realms (Moon 13)

**Golden Age Realm:**
- Everything in its fullest glory — the most saturated, most detailed, most beautiful zone in the game
- No damage, no decay, no mud — pristine golden-age architecture at peak operation
- Population: giants and humans walking together, children in floating gardens
- Lighting: perpetual golden hour, sky has 13 visible moons

**Dissonant Realm:**
- Monochrome gray-brown, no color, no light except faint red from corruption veins
- Everything broken — not romantically ruined, just *gone*
- Silence rendered visually: no particle effects, no movement, no life
- The most uncomfortable zone — beauty's absolute absence

---

## 6. VFX & Particle Systems

### Aether Vein System
- **Appearance:** Golden liquid light flowing through stone seams and ley lines
- **Behavior:** Flows toward higher-RS areas, pools at junction points, brightens with grid activity
- **Implementation:** Flow map shader on UV-mapped architecture meshes + GPU particle trails for ley lines
- **Performance:** LOD system — distant veins use simple emission, close-up veins use full flow simulation

### Aurora System
- **Layers:** Base curtain (vertex-animated mesh) + detail ribbons (GPU particles) + color shift (shader-driven)
- **Colors:** Tied to dominant Aether band — green/teal (3-Band), gold/amber (6-Band), violet/magenta (9-Band)
- **Reactive:** Intensity scales with global grid percentage — barely visible at 10%, full sky coverage at 90%+
- **Performance target:** 0.5ms GPU overhead maximum — ASTC-compressed curtain texture, billboard particles for distant ribbons

### Fountain Spray & Ionized Mist
- **Spray:** Physics-simulated arc (simplified — 3-point bezier) + GPU sprite particles for droplets
- **Ionized effect:** Rainbow prismatic refraction on each droplet (simple fresnel shader, no raytracing)
- **Mist:** Volumetric fog card with scrolling noise texture + Aether color tint
- **Interaction:** Player walking through mist triggers subtle sparkle particles + healing VFX on character

### Mud Excavation
- **Layers:** 3 mud density layers with unique textures (surface silt, hardened clay, ancient compacted)
- **Clearing:** Swipe gestures drive a deformation shader that reveals the architecture mesh beneath
- **Particles:** Mud chunks break off with physics (simplified rigidbody, 2-second lifetime), dust clouds puff
- **Reveal moment:** When architecture first appears, brief slow-motion + golden glow emission + choir audio sting

### Corruption / Dissonance
- **Visual:** Black fractal patterns that spread along stone surfaces — animated Perlin noise driving a mask shader
- **Particles:** Dark angular shards floating in irregular patterns (anti-music — visually jarring)
- **Edge effect:** Where corruption meets healthy Aether, a flickering boundary of sparks (visual tension)
- **Sound sync:** Corruption pulses are timed to anti-harmonic audio beats

---

## 7. UI Art Direction

### Design Language
- **Frame elements:** Golden-ratio rectangles, sacred geometry corner ornaments, thin gold lines
- **Background:** Translucent dark with subtle Aether shimmer (not opaque — the world shows through)
- **Typography:** Serif for headers (Tartarian aesthetic), clean sans-serif for body text (readability)
- **Iconography:** Minimal line-art style — golden on dark, consistent 2px stroke weight
- **Touch targets:** Minimum 44pt (Apple guidelines), generous padding, clear active states

### HUD Elements
- **RS Meter:** Arc gauge (top-center) — golden fill, 3 segment markers (3-6-9 bands), subtle pulse at high values
- **Aether Reserve:** Crystalline orb (top-left) — liquid fill animation, color shifts with dominant band
- **Companion Portraits:** Circular frames (bottom-left) — painted style, subtle animation (breathing, blinking)
- **Mini-Map:** Golden-ratio spiral design — north always up, ley lines visible, RS heat-map overlay toggle
- **17-Hour Clock:** Small dial (top-right) — 17 markers, current hour highlighted, golden glow during alignment windows

### Menu Design
- **Inventory:** Grid layout with 3D object previews on touch, golden-ratio proportioned slots
- **Skill Tree:** Radial design — 4 branches (Architect/Resonator/Guardian/Historian) spreading from center
- **Map Screen:** Full-globe view with ley-line network, pinch-zoom from planetary to street level
- **Settings:** Clean, accessible, generous spacing — all text legible at arm's length

---

## 8. Material & Texture Standards

### Texture Specifications

| Category | Max Resolution | Format | Memory Budget |
|---|---|---|---|
| **Hero Architecture** (close-up) | 2048×2048 | ASTC 4×4 (high) | 2.67 MB |
| **Standard Architecture** | 1024×1024 | ASTC 6×6 (mid) | 0.44 MB |
| **Terrain / Mud** | 2048×2048 (tiling) | ASTC 8×8 (low) | 1.0 MB |
| **Character / Companion** | 2048×2048 | ASTC 4×4 (high) | 2.67 MB |
| **VFX / Particles** | 512×512 | ASTC 4×4 | 0.17 MB |
| **UI Elements** | 1024×1024 atlas | ASTC 4×4 | 0.67 MB |

**Total texture budget:** 1.5 GB (as specified in technical spec)

### PBR Material Standards
- **Albedo:** Realistic color values — stone ~130-180 sRGB, metal ~180-220 sRGB, gold ~200-230 sRGB
- **Metallic:** Binary where possible (0 for stone/wood, 1 for metal) — saves texture bandwidth
- **Roughness:** Key differentiation — polished marble 0.1, weathered stone 0.7, rusted copper 0.5, mud 0.9
- **Normal maps:** Tangent-space, generated from high-poly sculpts — critical for detail at distance
- **Emission:** Aether veins, crystal glow, RS-reactive elements — driven by gameplay state, not baked

### Key Material Types

| Material | Albedo Character | Metallic | Roughness Range | Special |
|---|---|---|---|---|
| **Tartarian Marble** | Cream-white with warm veins | 0 | 0.1-0.3 | Subsurface scattering for translucency at thin edges |
| **Precision-Cut Stone** | Warm gray-gold | 0 | 0.2-0.4 | Sharp edge definition, no mortar visible in perfect cuts |
| **Patinated Copper** | Green-brown over orange-pink | 1 | 0.4-0.6 | Dual-layer: base copper + patina overlay driven by RS |
| **Crystal (Aether-Conductive)** | Clear with internal color shift | 0.5 | 0.05-0.1 | Refraction shader, internal glow, responsive to nearby band |
| **Mud (Various Layers)** | Dark brown → gray → black | 0 | 0.8-1.0 | Parallax occlusion for depth, moisture map for wet surfaces |
| **Mercury (Liquid)** | Mirror-silver | 1 | 0.0 | Fluid sim shader, reflection probe, surface tension animation |
| **Gold Leaf** | Rich warm gold | 0.9 | 0.1-0.2 | Thin-film effect, wears at edges revealing wood/stone beneath |
| **Pure Water** | Transparent, faint blue tint | 0 | 0.02 | Caustic projection, Aether color tinting, ionized sparkle particles |

---

## 9. Lighting Design Guide

### Lighting Philosophy
Natural lighting enhanced by Aether emission. No artificial electric lights exist in the Tartarian world — all illumination comes from sun, moon, Aether, fire, or crystal.

### Day/Night Cycle

| Phase | Duration (Game Time) | Light Character | Aether Visibility |
|---|---|---|---|
| **Dawn (Hour 1-3)** | 15 real min | Warm orange-pink, low shadows | Faint, retreating |
| **Morning (Hour 4-6)** | 15 real min | Clean white-yellow, crisp | Visible in shade |
| **Midday (Hour 7-11)** | 25 real min | Bright warm, minimal shadow | Low (sun overwhelms) |
| **Afternoon (Hour 12-14)** | 15 real min | Golden deepening, long shadows | Emerging glow |
| **Dusk (Hour 15-16)** | 10 real min | Rich amber to purple, dramatic | Strong, competing with sunset |
| **Hidden Hour (17)** | 5 real min (special) | Unusual silver-gold, doubled shadows | Maximum (all bands visible) |
| **Night (Post-17)** | 15 real min | Deep blue + aurora + Aether glow | Dominant light source |

### Zone-Specific Lighting

**Underground / Excavation:**
- Primary: Player's resonance tool glow (warm circle, 5m radius)
- Secondary: Aether vein emission through walls (directional guide)
- Ambient: Very low, warm — enough to see architecture silhouettes
- Drama: Discovery moments — sudden bloom of golden light as Aether activates

**Cathedral Interior:**
- Primary: Shaft light through windows (volumetric, dust particles)
- Secondary: Pipe organ emission (increases during performance)
- Ambient: Stone-bounce warm, low saturation
- Drama: Rose window projection — colored light patterns on floor, rotating with sun position

**Airship / Sky:**
- Primary: Open sky (HDR skybox with aurora overlay)
- Secondary: Mercury-orb engine glow (warm orange from below)
- Ambient: Very high, desaturated — cloud-reflected light
- Drama: Entering/exiting cloud layers — dramatic reveal of ground below

---

## 10. Camera & Cinematography

### Gameplay Camera
- **Default:** 3/4 isometric, 45° pitch, ~20m altitude above player
- **Zoom range:** 10m (intimate, building interiors) to 100m (zone overview)
- **Giant mode:** Camera altitude adjusts proportionally — maintains same relative framing
- **Touch controls:** Two-finger rotate, pinch zoom, single-finger pan (optional)

### Cinematic Camera Rules
- **Discovery moments:** Slow push-in on newly revealed architecture, slight rack focus from mud to stone
- **Restoration climax:** 360° orbit around completed structure, pulling out to show ley-line connections
- **Moon-end spectacles:** Escalating sequence: close-up → medium → wide → orbital (always pulls OUT to show scale)
- **Companion emotional beats:** Medium close-up, shallow depth of field, hold 3 seconds minimum
- **The Choice (Moon 13):** Static camera — player and Zereth framed in golden-ratio composition, world turning behind them

### Reference Cinematography Style
- **Scale revelation:** Shot language borrowed from nature documentaries — start intimately, pull back to reveal impossible scale
- **Architecture worship:** Slow tilts up columns, lingering on carved details, using light to draw the eye
- **Emotional grounding:** Never let spectacle overwhelm character — always return to a face, a reaction, a human moment
- **The Money Shot:** Every Moon ends with a single image that could be a poster — composition, lighting, and timing perfected

---

## 11. Asset Production Pipeline

### Workflow: Architecture Module

1. **Reference Gather:** Real-world photos of target style (grayed-out European cathedrals, star forts, White City pavilions)
2. **Blockout:** Gray-box in Unity — validate scale, camera framing, player navigation
3. **High-Poly Sculpt:** ZBrush/Blender — ornate detail pass (capitals, moldings, rose windows)
4. **Low-Poly Retopo:** Target poly counts per LOD:
   - LOD0 (hero/close): 50k-100k tris
   - LOD1 (medium): 15k-30k tris
   - LOD2 (distant): 3k-8k tris
   - LOD3 (silhouette): 500-1.5k tris
5. **UV & Bake:** Unique UVs for hero pieces, tiling textures for repeating elements
6. **Material Application:** PBR materials per standard (Section 8), Aether-reactive shaders applied
7. **State Variants:** Create 5 restoration states (buried → perfected) per module
8. **VFX Integration:** Attach Aether vein flow maps, emission points, particle anchors
9. **LOD Setup:** Assign LOD levels, test at camera distances, verify on target device
10. **Addressable Pack:** Bundle per zone for on-demand download (see technical spec)

### Budget Per Zone

| Element | Tri Budget | Texture Budget | Draw Calls |
|---|---|---|---|
| **Hero Architecture** (1-2 per zone) | 100k tris | 16 MB | 8-12 |
| **Standard Architecture** (5-10) | 150k tris total | 20 MB | 15-25 |
| **Terrain** | 50k tris | 8 MB | 4-6 |
| **Vegetation** | 30k tris (instanced) | 4 MB | 6-10 (GPU instancing) |
| **NPCs/Companions** | 40k tris total | 12 MB | 8-12 |
| **VFX/Particles** | GPU particles | 2 MB | 4-8 |
| **Zone Total** | ~370k tris | ~62 MB | ~50-70 |

**Target: 60 FPS on A19 chip at dynamic 720p-1080p with MetalFX upscaling**

---

## 12. Real-World Visual References

### Architecture Reference Sources

| Reference | Relevance | Key Details to Capture |
|---|---|---|
| **1893 World's Columbian Exposition** | White City pavilions (Moon 5) | Beaux-Arts columns, enormous domes, ornate entablatures, fountain courts — photograph the grandeur before it was "torn down" |
| **European Cathedral Interiors** | Sunken Cathedral (Moon 6) | Vaulted ceilings at impossible height, shaft light through rose windows, pipe organ scale, stone acoustics |
| **Star Fort Aerial Photography** | Star forts (Moon 4) | Geometric precision from above, moat reflections, bastion angles, the satisfying symmetry |
| **Buried Basement Photography** | Excavation zones (Moon 1) | Sunken windows staring at dirt, half-buried doorways, the eerie beauty of buildings consumed by earth |
| **Aurora Borealis Photography** | Global sky events | Color gradation, curtain movement, reflection on water, the scale of sky-filling light |
| **Antique Tartary Maps** | World map design | Ornate cartography, golden-age city depictions, fantastical but detailed geography |
| **Capitol Dome Interiors** | Dome complex assets | Coffered ceilings, rotunda proportions, oculus light wells, fresco integration |
| **Tesla Coil Photography** | Aether VFX | Electrical discharge patterns, the way energy arcs through air, plasma aesthetics |
| **Copper Patina Close-ups** | Material reference | The precise color gradation of aging copper, from bright orange through brown to verdigris green |
| **Mercury in Glass** | Mercury orb engines | Liquid metal behavior, surface tension, mirror-like reflection, mesmerizing fluid motion |

### Photography Direction for Reference Boards
When gathering reference, prioritize:
1. **Scale indicators:** Images with people for scale, showing how enormous these structures truly were
2. **Detail close-ups:** Carved capitals, joinery, ironwork — the craft that proves intentionality
3. **Light interaction:** How sunlight plays on marble, projects through stained glass, catches on copper
4. **Decay beauty:** Where nature meets architecture — the aesthetic of patient burial and potential resurrection
5. **Emotional composition:** Images that create the feeling of "this was real" — the core emotional hook

---

**Document Status:** DRAFT  
**Cross-References:** `04_ARCHITECTURE_GUIDE.md`, `09_TECHNICAL_SPEC.md`, `12_VIVID_VISUALS.md`  
**Last Updated:** March 23, 2026
