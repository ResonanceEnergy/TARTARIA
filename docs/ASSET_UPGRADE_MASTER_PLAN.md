# TARTARIA ASSET UPGRADE MASTER PLAN
**From Placeholders to Production Quality**  
**Date:** May 26, 2026  
**Scope:** Character models, Building architecture, Voice acting  
**Current State:** Production-ready code + intentional asset placeholders  
**Target State:** Shippable AAA indie quality (85-90/100)

---

## 📊 EXECUTIVE SUMMARY

**Current Placeholder Assets:**
- ✅ **Code/Logic:** 100% complete, 448 C# files, 31 Manager systems, 22/22 build phases GREEN
- 🟡 **Characters:** KayKit free models (functional but generic)
- 🟡 **Buildings:** Primitive cubes with custom shaders (functional but placeholder)
- 🟡 **Voice:** 432Hz procedural tones (functional but not narrative-grade)

**Upgrade Path Options:**

| Category | Option A (Free) | Option B (Paid Assets) | Option C (Commission) |
|----------|----------------|----------------------|---------------------|
| **Characters** | Mixamo + itch.io | Unity Asset Store packs | Fiverr/ArtStation artists |
| **Buildings** | OpenGameArt + KayKit | Modular Fantasy Kingdom | TurboSquid/CGTrader custom |
| **Voice Acting** | AI voice synthesis | Voice actor marketplace | Professional studio session |
| **Cost** | $0 | $500-1500 | $5000-15000 |
| **Quality** | 65-75/100 | 80-85/100 | 90-95/100 |
| **Timeline** | 2-4 weeks | 1-2 weeks | 6-12 weeks |

**Recommended Hybrid Approach:** Option B + selective Option C for hero assets  
**Total Cost:** $2000-3500  
**Timeline:** 4-8 weeks  
**Quality Target:** 85-90/100 (AAA indie competitive)

---

## 🎭 PART 1: CHARACTER MODELS

### Current State
- **KayKit models:** Char_Mage (Anastasia), Char_Barbarian (Korath), Char_Rogue_Hooded (Cassian), Char_Ranger (Thorne)
- **Status:** Functional rigs, import cleanly, animations work
- **Quality:** 60/100 — generic fantasy, not TARTARIA-specific

### Character Requirements (from GDD)

| Character | Type | Visual Requirements | Voice Profile | Priority |
|-----------|------|-------------------|---------------|----------|
| **Player (Elara)** | Humanoid | Latent giant blood, adventurer gear, tuning fork weapon | Silent (effort sounds only) | P0 |
| **Anastasia** | Spectral child (7-9yo) | Translucent projection, golden motes, period dress 1890s | Young girl, evolving clarity | P0 |
| **Korath** | Giant (15-20ft) | Ancient builder, gentle face, stone-like skin, work clothes | Deep bass, slow speech | P0 |
| **Cassian** | Humanoid spy | Intel officer uniform, cufflinks (dissonance tell), sharp features | Smooth baritone, micro-hesitations | P1 |
| **Lirael** | Spectral architect | Adult female, architect robes, blueprint tools, elegant | Soprano, ethereal reverb | P1 |
| **Captain Thorne** | Humanoid pilot | 200yo airship captain, weathered, flight jacket, goggles | Dry military crispness | P1 |
| **Milo** | Fox spirit | Six-tailed fox, glowing eyes, fur with Aether shimmer | Vocalizations (no speech) | P2 |
| **Veritas** | Spectral organist | Pipe organ conductor, formal concert attire | Musical metaphors | P2 |
| **Zereth** | Corrupted giant | Korath's brother, same build but corrupted, fractal patterns | Korath's voice + glitches | P2 |

---

### OPTION A: FREE RESOURCES ($0, 3-4 weeks)

#### **Source 1: Mixamo (Adobe)**
**URL:** https://mixamo.com  
**License:** Free commercial use  
**What to Download:**

| Character Slot | Mixamo Model | Animations Needed | Notes |
|----------------|--------------|-------------------|-------|
| Player (Elara) | "Adventurer" female | Walk, run, idle, jump, attack×3, die, climb | 12 animations × 2MB = 24MB |
| Korath | "Mutant" (scaled 6×) | Walk, idle, kneel, lift, gesture | 5 animations |
| Cassian | "Business Man" (add coat) | Walk, idle, talk, analyze | 4 animations |
| Thorne | "Military Officer" | Walk, idle, pilot stance, salute | 4 animations |

**Total Mixamo:** 8 character meshes + 50 animations (~180MB)  
**Quality:** 65/100 — rigs are excellent, meshes are generic  
**Limitation:** No child models (Anastasia), no fox (Milo), no spectral VFX

#### **Source 2: itch.io Free Packs**
**Quaternius Low Poly Pack:** https://quaternius.itch.io  
- 100+ low-poly characters CC0  
- 500-2K tris per model  
- Can find fox proxy for Milo  

**RPG Character Pack:** https://opengameart.org  
- Modular character base  
- Can kitbash Anastasia from child NPC parts  

#### **Source 3: Blender Custom Modifications**
- Download Blender 4.1 (free)  
- Modify Mixamo meshes: scale proportions, add accessories  
- Create spectral shader for Lirael/Anastasia (Shader Graph)  
- Add tails to fox model via modeling  

**Timeline Breakdown:**
- Week 1: Download 8 Mixamo models + 50 animations, import to Unity
- Week 2: Download itch.io packs, find fox/child proxies
- Week 3: Blender modifications (scale, accessories, tails)
- Week 4: Spectral shader creation, test all characters in-game

**Final Quality:** 70/100 — functional, indie-grade, not unique  
**Pros:** $0 cost, full control, can iterate  
**Cons:** Time-intensive, requires Blender skills, still generic

---

### OPTION B: PAID ASSET STORE ($400-800, 1-2 weeks)

#### **Bundle 1: Polygon Character Pack — $25**
**URL:** https://assetstore.unity.com/packages/polygon-character-pack  
**What you get:**
- 12 modular characters (humanoid rigs)
- 20 armor/outfit sets
- Modular weapon system
- 4K tris per character (optimized)

**Use for:** Player (Elara), Cassian, Thorne — all humanoid adult characters

#### **Bundle 2: Polygon Farm Animals — $20**
**URL:** https://assetstore.unity.com/packages/polygon-farm-animals  
**Contains:** Fox model (can add tails in Blender)  
**Use for:** Milo base mesh

#### **Bundle 3: Fantasy Character Mega Bundle — $150 (on sale)**
**URL:** https://assetstore.unity.com/packages/fantasy-mega-bundle  
**What you get:**
- 30+ fantasy characters
- Giants, children, adults, creatures
- 200+ animations included
- PBR materials 2K textures

**Use for:** Korath (giant base), Anastasia (child base), NPCs

#### **Bundle 4: Spectral Ghosts VFX Pack — $45**
**URL:** https://assetstore.unity.com/packages/vfx-spectral-ghosts  
**What you get:**
- Translucent character shaders
- Particle systems (motes, wisps, trails)
- Shader Graph sources (editable)

**Use for:** Lirael, Anastasia, Veritas spectral effects

#### **Bundle 5: Motion Capture Animation Library — $60**
**URL:** https://assetstore.unity.com/packages/animation-mocap-library  
**What you get:**
- 400+ mocap animations
- Humanoid + giant rigs compatible
- Walk, run, combat, idle, gestures

**Total Cost:** $300 bundles  
**Timeline:** 1 week download + import, 1 week integration + testing  
**Final Quality:** 82/100 — professional polish, modular, cohesive style  
**Pros:** Fast, proven quality, support included  
**Cons:** Not 100% custom, still recognizable from other games

---

### OPTION C: COMMISSION CUSTOM ($4000-8000, 8-12 weeks)

#### **Platform: ArtStation**
**URL:** https://artstation.com/jobs  
**Process:**
1. Post job listing: "3D character artist for Unity game — 9 hero characters"
2. Budget: $400-800 per character
3. Requirements: Humanoid rig, 4K PBR textures, LOD0/LOD1, Unity-optimized

**Artist Rates:**
- **Junior (Eastern Europe/Asia):** $300-500 per character (8-10K tris)
- **Mid-level (Worldwide):** $600-1000 per character (12-15K tris, hair cards)
- **Senior (US/UK):** $1200-2000 per character (AAA quality, cloth sim)

**Recommended:** Mix of mid-level ($700 avg) × 9 characters = $6300

#### **Deliverables Per Character:**
- High-poly sculpt (ZBrush/Blender, for portfolio)
- Game mesh with humanoid rig (Unity-compatible)
- 4K PBR texture set (Albedo, Normal, Metallic, Roughness, AO)
- 2 LOD levels (LOD0 full detail, LOD1 half tris)
- Blend shapes for facial expressions (optional, +$100/character)

#### **Timeline Per Character:**
- Week 1: Concept art review (you provide GDD descriptions)
- Week 2-3: Sculpt high-poly
- Week 4: Retopology to game mesh
- Week 5: UV unwrap + texture painting
- Week 6: Rigging + skinning
- Week 7: LOD creation + Unity import testing
- Week 8: Revisions

**Parallel Workflow:** Hire 3 artists, each takes 3 characters, overlapping schedules  
**Total Timeline:** 8 weeks (2 months) from contract to full delivery

**Final Quality:** 92/100 — fully custom, TARTARIA-specific, portfolio-grade  
**Pros:** Unique IP, perfect art direction match, scalable (hire more artists)  
**Cons:** Expensive, long timeline, requires art direction skills

---

### OPTION D: HYBRID APPROACH (RECOMMENDED) ($800-1500, 4-6 weeks)

**Strategy:** Paid assets for secondary characters + commission for heroes

**Tier 1: Buy Asset Packs ($300)**
- Polygon Character Pack ($25) → Cassian, Thorne, NPCs
- Fantasy Mega Bundle ($150) → Korath base, Anastasia base, crowd fill
- Spectral VFX Pack ($45) → Lirael, Veritas, all spectral effects
- Animation Library ($60) → All characters

**Tier 2: Commission Hero Characters ($1200)**
- **Player (Elara):** $600 — hero character, most screen time, needs perfect silhouette
- **Korath:** $600 — giant, unique proportions, stone-like skin shader

**Why This Works:**
- Asset pack characters (Cassian, Thorne, Lirael, Anastasia) get 80% of the way there
- Commissioned heroes (Elara, Korath) are 95% unique
- Budget saved on animations (buy library instead of commission each)
- Timeline parallelized (buy packs Week 1, commission starts Week 2, overlap)

**Total Cost:** $1500  
**Timeline:** 6 weeks (2 weeks asset integration + 6 weeks commission overlap)  
**Final Quality:** 87/100 — hero characters shine, supporting cast professional

---

## 🏰 PART 2: BUILDING ARCHITECTURE

### Current State
- **Primitive cubes** with custom shaders (M_AetherVein, M_Restoration, M_Corruption)
- **Status:** Shaders functional, tuning puzzles work, RS reward system operational
- **Quality:** 50/100 — gameplay works, visuals placeholder

### Building Requirements (from GDD)

| Building Type | Visual Style | Size | Complexity | Priority |
|---------------|-------------|------|------------|----------|
| **Star Dome** | Gothic cathedral, rose windows, spire | 40m diameter | Hero asset | P0 |
| **Harmonic Fountain** | Flowing water, mercury basin, crystalline | 8m diameter | Medium detail | P0 |
| **Crystal Spire** | Vertical tower, faceted surfaces, light refraction | 60m tall | Hero asset | P0 |
| **Cathedral (Moon 6)** | Pipe organ interior, vaulted ceilings, stained glass | 80m × 50m | Hero asset | P0 |
| **Star Fort (Moon 4)** | Geometric military architecture, bastions | 200m × 200m | Modular pieces | P1 |
| **White City Pavilions** | 1893 World's Fair style, ionic columns, fountains | Modular 10-20m | Modular set | P1 |
| **Bell Tower** | 12 bells, clockwork, accessible interior | 40m tall | Medium detail | P1 |
| **Airship** | Mercury-orb engine, crystal sails, copper hull | 30m length | Hero asset | P2 |

---

### OPTION A: FREE RESOURCES ($0, 4-6 weeks)

#### **Source 1: OpenGameArt.org**
**URL:** https://opengameart.org/art-search-advanced?keys=&field_art_type_tid%5B%5D=10&sort_by=count&sort_order=DESC  
**Search:** "Cathedral", "Gothic", "Tower", "Fountain"

**Top Free Packs:**
- **Gothic Cathedral Kit:** 50+ modular pieces (walls, arches, windows) — CC-BY
- **Low Poly Arena:** Stone floor tiles, pillars, domes — CC0
- **Fountain Pack:** 8 fountain variants with water VFX — CC-BY

**Quality:** 60/100 — low-poly, dated textures, mismatched styles  
**Pros:** Free, immediate download  
**Cons:** Time to kitbash into cohesive buildings

#### **Source 2: KayKit Dungeon Pack (Already Downloaded)**
**Location:** `Assets/KayKit_*/`  
**Contains:** Modular dungeon pieces, stone walls, floors, props  
**Use for:** Underground sections, tunnel interiors

#### **Source 3: Blender Procedural Generation**
**Tool:** Blender Geometry Nodes  
**Process:**
1. Download Blender 4.1 (free)
2. Use Sacred Geometry addon (free on GitHub)
3. Generate: domes via icosphere subdivision, spires via beveled curves, fountains via array modifiers
4. Export to FBX → Import to Unity

**Example Workflow (Star Dome):**
- Base shape: UV sphere (32 subdivisions) → icosphere conversion → golden ratio scaling
- Rose window: Array modifier on circle → radial pattern (12 petals = 3×4, sacred)
- Spire: Bezier curve → bevel profile → fibonacci spiral taper
- Materials: PBR stone texture from Polyhaven (CC0)

**Timeline:**
- Week 1-2: Learn Blender Geometry Nodes basics (YouTube tutorials)
- Week 3-4: Model 3 hero buildings (Star Dome, Fountain, Spire)
- Week 5: Model modular sets (Star Fort walls, Pavilion columns)
- Week 6: Materials, LODs, Unity import + colliders

**Final Quality:** 72/100 — unique geometry, indie-grade materials  
**Pros:** Fully custom, learn transferable skill, procedural = easy iteration  
**Cons:** Steep learning curve, time-intensive, materials still stock

---

### OPTION B: PAID ASSET STORE ($800-1200, 1-2 weeks)

#### **Bundle 1: Modular Fantasy Kingdom — $150 (Spring Sale)**
**URL:** https://assetstore.unity.com/packages/modular-fantasy-kingdom  
**What you get:**
- 500+ modular pieces (walls, floors, roofs, props)
- Gothic cathedral interior kit
- Bell tower components
- Fountain variants
- PBR 4K materials
- LOD0/LOD1/LOD2

**Use for:** Star Dome, Cathedral, Bell Tower, Star Fort, Pavilions  
**Coverage:** 80% of building needs

#### **Bundle 2: Fantasy Adventure Environment — $2.25 (Spring Sale, was $15)**
**URL:** https://assetstore.unity.com/packages/fantasy-adventure-environment  
**What you get:**
- Cathedral interior (ready-made)
- Stained glass window prefabs
- Ornate door/arch assets
- 4K PBR materials

**Use for:** Cathedral interior, rose windows, ornate doors

#### **Bundle 3: Realistic Water VFX — $22.49**
**URL:** https://assetstore.unity.com/packages/realistic-water-vfx  
**What you get:**
- 50+ water effects (fountains, cascades, mist)
- Shader Graph sources (editable)
- Caustics, foam, spray particles

**Use for:** Harmonic Fountain, all water features, ionized mist

#### **Bundle 4: Crystal Cave Environment — $75**
**URL:** https://assetstore.unity.com/packages/crystal-cave-environment  
**What you get:**
- Crystal formations (modular)
- Refraction shaders
- Glow materials
- Particle systems (crystal dust, light shafts)

**Use for:** Crystal Spire, underground crystal sections, Aether vein visuals

#### **Bundle 5: Polygon Airship Pack — $50**
**URL:** https://assetstore.unity.com/packages/polygon-airship-pack  
**What you get:**
- 12 airship variants
- Modular parts (hull, sails, engine, rigging)
- Animated propellers
- 2K stylized materials

**Use for:** Captain Thorne's airship, airship fleet (Moon 8)

**Total Cost:** $300 (with Spring Sale discounts = $1200 without sale)  
**Timeline:** 1 week download + import, 1 week scene assembly + lighting  
**Final Quality:** 85/100 — professional, cohesive, optimized  
**Pros:** Fast, proven performance, support, updates  
**Cons:** Not 100% unique (other games use same packs)

---

### OPTION C: COMMISSION CUSTOM ($8000-20000, 10-16 weeks)

#### **Platform: TurboSquid / CGTrader Pro Artists**
**URL:** https://turbosquid.com, https://cgtrader.com  
**Process:**
1. Search verified sellers with game-ready portfolios
2. Contact 3-5 artists for quotes
3. Provide GDD + reference images
4. Negotiate per-building rates

**Artist Rates (Game-Ready Buildings):**
- **Simple props (fountain, pillar):** $200-400
- **Medium buildings (tower, pavilion):** $800-1500
- **Hero assets (cathedral interior):** $2000-4000
- **Complex (full Star Fort):** $5000-8000

**Recommended Commission List:**
1. **Star Dome** — $3000 (hero asset, most iconic)
2. **Cathedral Interior** — $3500 (Moon 6 centerpiece)
3. **Crystal Spire** — $2500 (vertical landmark)
4. **Airship** — $2000 (hero vehicle)

**Total:** $11,000 (4 hero buildings)  
**Buy Asset Packs:** Remaining buildings ($300 from Option B)  
**Grand Total:** $11,300

#### **Deliverables Per Building:**
- High-poly model (for promotional renders)
- Game mesh with colliders (Unity-optimized)
- 4K PBR texture sets (Albedo, Normal, Metallic, Roughness, AO, Emissive)
- 3 LOD levels (LOD0 full, LOD1 50% tris, LOD2 25% tris)
- Lightmap UVs (second UV channel for baked lighting)
- Modular breakdowns (if requested)

#### **Timeline Per Hero Building:**
- Week 1-2: Concept art / blockout review
- Week 3-6: High-poly modeling (ZBrush + Blender)
- Week 7-8: Retopology + LODs
- Week 9-10: UV unwrap + texture painting (Substance Painter)
- Week 11-12: Unity import + material setup + colliders
- Week 13-14: Lighting tests + optimization
- Week 15-16: Revisions

**Parallel Workflow:** 2 artists, each takes 2 buildings, 8-week overlap  
**Final Quality:** 95/100 — museum-quality, unique IP, cinematic

**Pros:** Exactly matches vision, IP ownership, portfolio marketing value  
**Cons:** Expensive, long timeline, requires detailed art direction

---

### OPTION D: HYBRID APPROACH (RECOMMENDED) ($1500-2500, 6-8 weeks)

**Strategy:** Asset packs for modular/secondary + commission for 1-2 hero buildings

**Tier 1: Buy Asset Packs ($300 with sale)**
- Modular Fantasy Kingdom ($150) → Star Fort, Pavilions, Bell Tower, generic buildings
- Fantasy Adventure Environment ($2.25) → Cathedral INTERIOR (already done!)
- Realistic Water VFX ($22.49) → All fountains
- Crystal Cave ($75) → Crystal Spire base
- Polygon Airship ($50) → Airship base mesh

**Tier 2: Commission 2 Hero Buildings ($2200)**
- **Star Dome EXTERIOR** — $1200 (most iconic building, first impression)
- **Crystal Spire FULL** — $1000 (vertical landmark, modify asset pack base)

**Why This Works:**
- Asset packs cover 90% of buildings
- Commissioned Star Dome makes the game visually unique
- Crystal Spire gets custom treatment (important gameplay element)
- Cathedral interior already solved by $2.25 asset (steal!)
- Budget focused on highest-impact visuals

**Total Cost:** $2500  
**Timeline:** 2 weeks asset integration + 6 weeks commission overlap = 8 weeks total  
**Final Quality:** 88/100 — hero buildings shine, supporting cast professional

---

## 🎤 PART 3: VOICE ACTING

### Current State
- **VOPlaceholderLibrary:** 12 procedural 432Hz tones
- **Status:** Audio system triggers correctly, volume mixing works, dialogue timing validated
- **Quality:** 40/100 — functional for testing, not narrative-grade

### Voice Acting Requirements (from C_AUDIO_DESIGN.md)

| Character | Voice Profile | Line Count | Emotion Range | Recording Notes |
|-----------|--------------|------------|---------------|-----------------|
| **Player (Elara)** | Silent protagonist | 40 effort sounds | Grunts, gasps, breathing | Clean close-mic, minimal processing |
| **Milo (Fox)** | Vocalizations | 60 sounds | Chitters, growls, yips | Animal-like but intelligent, one word spoken |
| **Lirael** | Ethereal soprano | 120 lines | Evolving clarity | Heavy reverb early, clean late, sings once |
| **Korath** | Deep bass (giant) | 80 lines + 50 echo | Gentle, slow, stone-resonance | Very low register, pauses between sentences |
| **Cassian** | Smooth baritone | 90 lines | Micro-hesitations on lies | Trained actor only, subtle performance |
| **Thorne** | Dry military | 100 lines | Understatement, tenderness | Commands = requests, solitude-softened |
| **Anastasia** | Young girl (7-9yo) | 60 lines | Wonder, sadness, golden warmth | Spectral shimmer processing |
| **Veritas** | Musical metaphors | 40 lines | Precise, passionate | Pipe organ harmonics in voice (processing) |
| **Zereth** | Corrupted giant | 40 lines + 30 corrupted | Pain, dissonance, redemption | Korath's voice + audio glitches |
| **NPCs / Crowd** | Various | 50 background | Ambient chatter | Walla recordings |

**Total Lines:** 760 (~8-10 hours of final audio after editing)

---

### OPTION A: AI VOICE SYNTHESIS ($0-200, 1-2 weeks)

#### **Tool 1: ElevenLabs**
**URL:** https://elevenlabs.io  
**Pricing:** 
- Free tier: 10K characters/month (~2 hours audio)
- Creator plan: $22/month (100K characters = ~20 hours audio)
- Pro plan: $99/month (500K characters = ~100 hours audio)

**Process:**
1. Generate voice profiles for each character
2. Paste dialogue script lines
3. Download generated audio files
4. Import to Unity as .ogg files

**Quality:** 75/100 — natural prosody, good for NPCs, uncanny for heroes  
**Pros:** Instant, cheap, unlimited iterations, 29 languages  
**Cons:** No emotional nuance, can sound flat, ethical concerns (AI training data)

#### **Tool 2: Replica Studios**
**URL:** https://replicastudios.com  
**Pricing:** $24/month (unlimited generations)  
**Features:**
- Game-focused AI voices
- Emotion sliders (happy, sad, angry, scared)
- Pitch/speed controls
- Unity plugin (direct import)

**Quality:** 78/100 — better emotion than ElevenLabs, still AI-identifiable  
**Use case:** Perfect for background NPCs, acceptable for secondary characters

#### **Recommended AI Workflow:**
- Use AI for NPCs, crowd walla, background lines (100-150 lines)
- Commission real actors for main 9 characters (600+ lines)
- Saves $500-1000 on background VO

**Total Cost:** $200 (2 months of Replica Studios)  
**Timeline:** 1 week to generate all AI lines  
**Final Quality (hybrid):** 82/100 for main cast, 75/100 for NPCs

---

### OPTION B: VOICE ACTOR MARKETPLACE ($2000-5000, 4-6 weeks)

#### **Platform 1: Voices.com**
**URL:** https://voices.com  
**Process:**
1. Post job listing: "Video game voice acting — 9 characters, 760 lines"
2. Set budget: $50-150 per character (rate varies by experience)
3. Review auditions (100-200 responses typical)
4. Hire 9 actors, send scripts
5. Receive recorded files, review, approve

**Actor Rates:**
- **Hobbyist (Fiverr):** $5-25 per 100 words (~$100 per character)
- **Semi-pro (Voices.com):** $50-150 per hour (~$300-600 per character)
- **Union (SAG-AFTRA):** $200-500 per hour (~$1000+ per character)

**Recommended Tier:** Semi-pro ($100-200 per character)  
**Why:** Good quality, reliable, affordable, fast turnaround

#### **Platform 2: Fiverr**
**URL:** https://fiverr.com/categories/music-audio/voice-overs  
**Filter:** Top Rated, 4.9+ stars, 100+ reviews  
**Typical Rates:** $50-150 for 500 words

**Sample Gig:** "I will record 500 words in 24 hours — $75"  
**For 760 lines (~8000 words):** 16 × $75 = $1200 total if using one actor  
**For 9 characters:** $1200 ÷ 9 = $133 per character (budget-friendly)

#### **Casting Strategy:**
1. **Player (Elara):** Effort sounds only — hire on Fiverr ($50 for 1-hour session)
2. **Milo (Fox):** Voice actor with animal sound experience ($100)
3. **Lirael:** Soprano singer — post on Casting Call Club (free platform) ($150-300)
4. **Korath:** Deep bass voice — Fiverr "movie trailer voice" actors ($200)
5. **Cassian:** Trained actor with subtle performance skill ($300-500 from Voices.com)
6. **Thorne:** Military/gruff voice — Fiverr ($150)
7. **Anastasia:** Child actor (7-9yo) — HARDEST to cast, may need local SAG talent ($400-800)
8. **Veritas:** Classical music background voice ($150)
9. **Zereth:** Use Korath's actor, add audio processing ($50 extra)

**Total Cost:** $1850-3000 (depends on Cassian + Anastasia casting)  
**Timeline:** 2 weeks casting + auditions, 2 weeks recording sessions, 2 weeks editing  
**Final Quality:** 80-85/100 — professional but not AAA studio

---

### OPTION C: PROFESSIONAL STUDIO SESSION ($8000-15000, 6-8 weeks)

#### **Studio: Local Recording Studio with Union Actors**
**Process:**
1. Hire local studio (search "voice recording studio [your city]")
2. Book studio time: $100-300/hour (includes engineer)
3. Hire union actors via SAG-AFTRA talent agency
4. Record all characters in 3-day intensive session
5. Studio provides edited, mastered files

**Studio Session Breakdown:**
- **Day 1:** 4 characters × 2 hours each = 8 hours studio time
- **Day 2:** 4 characters × 2 hours each = 8 hours studio time
- **Day 3:** 1 character + retakes + wild lines = 8 hours studio time

**Cost Breakdown:**
- Studio time: 24 hours × $200/hour = $4800
- Union actors: 9 actors × $500-1000 each = $4500-9000
- Post-production (editing, mastering): $1000
- **Total:** $10,300-14,800

**Quality:** 95/100 — AAA studio grade, directed performance, clean audio  
**Pros:** Professional direction, multiple takes, immediate feedback, cohesive performances  
**Cons:** Expensive, requires you to attend sessions (3 full days), union contracts

---

### OPTION D: HYBRID APPROACH (RECOMMENDED) ($1500-3000, 6-8 weeks)

**Strategy:** Online marketplace for most + local casting for hero characters

**Tier 1: Fiverr/Voices.com for Secondary ($800-1200)**
- Thorne, Veritas, Milo, Zereth (4 characters × $150-250 avg) = $800
- Player effort sounds (Elara) = $50
- NPC background lines via AI (Replica Studios $24/month × 2) = $48

**Tier 2: Local SAG Talent for Heroes ($700-1800)**
- **Korath** — Union voice actor with bass register ($400-800, 2-hour session)
- **Lirael** — Opera singer (post on local music school board) ($300-500)
- **Cassian** — Trained theater actor ($400-800)
- **Anastasia** — Child actor via local talent agency ($600-1200) ← MOST EXPENSIVE

**Tier 3: Post-Production ($200-400)**
- Hire audio engineer on Fiverr to edit/master all files
- Noise reduction, EQ, compression, normalize to -3dB
- Trim silence 50ms head/tail
- Export as 44.1kHz mono .ogg files

**Why This Works:**
- Budget focused on hardest-to-cast characters (child, giant, subtle actor)
- Fiverr handles volume work (secondary characters)
- AI fills gaps (NPCs, crowd)
- Professional post ensures consistent quality

**Total Cost:** $1900-3000  
**Timeline:** 4 weeks casting/recording, 2 weeks post-production = 6 weeks  
**Final Quality:** 85/100 — hero characters shine, supporting cast professional

---

## 📅 MASTER TIMELINE: FULL PRODUCTION

### PHASE 1: PRE-PRODUCTION (Week 1-2)
- Create detailed asset specifications from GDD
- Build reference mood boards (Pinterest, ArtStation)
- Write casting breakdowns for voice actors
- Budget finalization and vendor selection

**Deliverables:**
- Asset spec sheet (characters, buildings, props)
- Voice actor casting call scripts
- Vendor contracts signed

---

### PHASE 2: ASSET PROCUREMENT (Week 3-10)

#### **Parallel Track A: Characters (Week 3-8)**
- **Week 3:** Purchase asset packs ($300), download, import to Unity
- **Week 3-4:** Commission contracts for Elara + Korath ($1200 total)
- **Week 4:** Integrate asset pack characters (Cassian, Thorne, etc.)
- **Week 5-8:** Commission work in progress (artist check-ins Week 5, 7)
- **Week 8:** Final character delivery, import, rig testing

#### **Parallel Track B: Buildings (Week 3-8)**
- **Week 3:** Purchase asset packs ($300), download, import to Unity
- **Week 3-4:** Commission contracts for Star Dome + Crystal Spire ($2200 total)
- **Week 4-5:** Scene assembly with asset pack buildings
- **Week 5-8:** Commission work in progress (blockout Week 4, high-poly Week 6)
- **Week 8:** Final building delivery, import, collider setup, lighting

#### **Parallel Track C: Voice Acting (Week 3-10)**
- **Week 3-4:** Post casting calls (Voices.com, Fiverr, local boards)
- **Week 4-5:** Review auditions, select actors (9 characters)
- **Week 5-6:** Send scripts to actors, schedule recording sessions
- **Week 6-8:** Recording sessions (staggered, remote submissions)
- **Week 9:** Collect all recordings, review for retakes
- **Week 10:** Post-production (editing, mastering, implementation)

---

### PHASE 3: INTEGRATION (Week 9-12)

#### **Week 9-10: Asset Integration**
- Import final commissioned assets (characters + buildings)
- Wire prefabs to existing game systems
- Replace placeholder references in ContentSpawner files
- Test all characters in-game (animations, scale, colliders)
- Test all buildings (RS rewards, tuning puzzles, VFX)

#### **Week 10-11: Voice Acting Integration**
- Replace VOPlaceholderLibrary.cs line 43 (Placeholder/ → Production/ folder)
- Import .ogg files to Assets/Resources/VO/Production/
- Test all dialogue triggers (QuestManager, DialogueManager)
- Volume balance pass (dialogue vs music vs SFX)
- Verify subtitle sync

#### **Week 11-12: Polish Pass**
- Lighting adjustments for new buildings
- Character material tweaks (PBR values, emissive)
- Audio mix refinement (reverb per environment)
- Performance optimization (LOD distances, occlusion culling)
- Build validation: `.\tartaria-play.ps1 -BatchOnly` → GREEN

---

### PHASE 4: VALIDATION (Week 13-14)

#### **Week 13: Playtest Pass**
- Full playthrough Moon 1-3 (test all new assets in context)
- Verify character animations trigger correctly
- Verify building restoration sequences
- Verify voice lines play at correct moments
- Note any visual or audio bugs

#### **Week 14: Bug Fixing + Final Build**
- Fix reported issues from playtest
- Final build: `.\tartaria-play.ps1 -BatchOnly` → GREEN
- Create backup of all asset files
- Update KNOWN_PLACEHOLDERS.md → mark all as RESOLVED
- Commit to git: "Asset upgrade complete — production-ready"

---

## 💰 COST SUMMARY: ALL OPTIONS

### BUDGET OPTION ($200-500, 4 weeks)
- Characters: Free (Mixamo + itch.io + Blender)
- Buildings: Free (OpenGameArt + Blender Geometry Nodes)
- Voice: AI synthesis ($200)
- **Total:** $200
- **Quality:** 70-75/100 — functional, indie-grade
- **Best for:** Solo developer, learning project, pre-alpha

### BALANCED OPTION ($2000-3500, 6-8 weeks) ⭐ RECOMMENDED
- Characters: Hybrid ($1500) — asset packs + 2 commissions
- Buildings: Hybrid ($2500) — asset packs + 2 commissions
- Voice: Hybrid ($2500) — marketplace + local SAG talent
- **Total:** $6500
- **Quality:** 85-88/100 — AAA indie competitive
- **Best for:** Kickstarter/publisher pitch, Steam Early Access

### PREMIUM OPTION ($15000-25000, 12-16 weeks)
- Characters: Full commission ($6500) — all 9 custom characters
- Buildings: Full commission ($11000) — 4 hero buildings + asset packs
- Voice: Studio session ($12000) — union actors, professional studio
- **Total:** $29,500
- **Quality:** 92-95/100 — AAA studio grade
- **Best for:** Funded project, Epic MegaGrant recipient, publisher-backed

---

## 🎯 RECOMMENDED EXECUTION PLAN

### FOR TARTARIA (Current State)

**Budget:** $6500  
**Timeline:** 8 weeks  
**Approach:** Balanced hybrid (Option D across all categories)

**Week 1-2: Procurement**
- Buy all asset packs ($600 total: $300 characters + $300 buildings)
- Post casting calls (voice actors)
- Contract 2 character artists ($1200 total)
- Contract 2 building artists ($2200 total)

**Week 3-6: Production**
- Integrate asset pack content (characters + buildings)
- Commission work in progress (weekly check-ins)
- Voice actor recording sessions (staggered)

**Week 7-8: Integration**
- Final commissioned assets delivered
- Voice post-production complete
- Full integration pass + polish
- Build validation GREEN

**Final Quality:** 87/100 — production-ready, shippable, competitive with AAA indies

---

## 📊 COMPARISON: FREE vs PAID vs COMMISSION

| Aspect | Free | Paid Assets | Commission |
|--------|------|-------------|------------|
| **Characters** | Mixamo generic | Professional packs | 100% custom |
| **Buildings** | Kitbashed | Modular cohesive | Unique hero assets |
| **Voice** | AI synthesis | Marketplace actors | Studio recording |
| **Cost** | $200 | $2000-3500 | $15000-25000 |
| **Timeline** | 4 weeks | 6-8 weeks | 12-16 weeks |
| **Quality** | 70-75/100 | 85-88/100 | 92-95/100 |
| **IP Ownership** | Limited (CC-BY) | Non-exclusive | Exclusive |
| **Uniqueness** | Recognizable | Some overlap | 100% unique |
| **Risk** | DIY skills needed | Proven quality | Art direction risk |

---

## 🚀 GETTING STARTED: NEXT STEPS

### If Choosing Balanced Hybrid ($6500, 8 weeks):

**TODAY:**
1. Create Unity Asset Store account, add $600 to wallet
2. Create ArtStation account, post 2 job listings (characters + buildings)
3. Create Voices.com account, post casting call
4. Create Fiverr account, search "voice actor game"

**THIS WEEK:**
5. Purchase all asset packs, begin download
6. Write detailed commission briefs (reference GDD sections)
7. Review artist portfolios, send 5-10 interview requests
8. Review voice actor auditions, shortlist 15-20 candidates

**WEEK 2:**
9. Select 2 character artists + 2 building artists, sign contracts
10. Select 9 voice actors, send scripts
11. Import asset packs to Unity, begin integration
12. Schedule weekly check-ins with all vendors

**WEEK 3-7:**
13. Monitor commission progress (weekly Milestone reviews)
14. Receive voice recordings, send feedback
15. Continue asset pack integration (replace placeholders)

**WEEK 8:**
16. Receive final commissioned assets
17. Receive final voice files (post-production complete)
18. Full integration + polish pass
19. Build validation: `.\tartaria-play.ps1 -BatchOnly` → GREEN
20. Ship production-ready game! 🎉

---

**Document Status:** FINAL  
**Cross-References:**  
- `FREE_ART_OPTIONS_COMPREHENSIVE.md` (detailed free resource links)
- `NEXT_LEVEL_PAID_ASSETS.md` (paid asset deep dives)
- `C_AUDIO_DESIGN.md` (complete voice acting requirements)
- `00_MASTER_GDD.md` (character/building specifications)

**Last Updated:** May 26, 2026
