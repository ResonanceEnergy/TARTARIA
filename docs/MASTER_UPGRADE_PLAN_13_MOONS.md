# TARTARIA — Master Upgrade Plan (13 Moons Campaign)
**Target:** AAA Indie Quality (85/100) across all systems  
**Timeline:** 20 weeks (5 months)  
**Budget:** $2,868 (AA tier) OR $405 (Indie tier)  
**Current State:** 50/100 (greybox prototype with primitive geometry)  
**Date:** April 29, 2026

---

## 📊 QUALITY TARGETS BY SYSTEM

| System | Current | Target | Gap | Priority |
|--------|---------|--------|-----|----------|
| **Characters** | 30/100 (primitives) | 85/100 | 55 pts | P0 🔴 |
| **Environments** | 40/100 (proc terrain) | 80/100 | 40 pts | P1 🟡 |
| **Buildings** | 35/100 (basic shapes) | 85/100 | 50 pts | P1 🟡 |
| **Objects** | 25/100 (cubes/spheres) | 75/100 | 50 pts | P2 🟢 |
| **Animations** | 50/100 (8 Mixamo) | 80/100 | 30 pts | P1 🟡 |
| **NPCs** | 30/100 (capsules) | 80/100 | 50 pts | P1 🟡 |
| **VFX** | 60/100 (custom shaders) | 85/100 | 25 pts | P2 🟢 |
| **UI/Menus** | 40/100 (placeholders) | 75/100 | 35 pts | P2 🟢 |
| **Audio** | 35/100 (4 clips) | 70/100 | 35 pts | P3 🔵 |

**Overall:** 37/100 → **85/100** (13-moon campaign complete)

---

## 🎯 PHASE-BY-PHASE BREAKDOWN

### **PHASE 0: Foundation (Weeks 1-2) — $280**
**Goal:** Establish asset library + automation pipeline  
**Quality Gain:** 37/100 → 55/100 (+18 points)

#### **Characters (P0)**
- ✅ **DONE:** Player_Mesh.fbx (18.89 MB)
- ⬜ Download remaining Mixamo:
  - Anastasia_Mesh.fbx (Queen)
  - Milo_Mesh.fbx (Worker)
  - MudGolem_Mesh.fbx (Mutant)
- ⬜ Purchase **Polygon Character Pack** ($50 → $25 Spring Sale)
  - 12 rigged characters (covers all major NPCs)
  - Export: Cassian (knight), Lirael (mage), villagers
- ⬜ Download **Quaternius Low Poly Characters** (FREE, CC0)
  - 20+ character variations
  - Use for background NPCs

**Deliverable:** 16 unique character models (4 Mixamo + 12 Polygon)

---

#### **Environments Foundation (P1)**
- ⬜ Purchase **Fantasy Adventure Environment** ($45 → $2.25 Flash Deal)
  - Cathedral interiors, stone props, torches
  - Use for Moons 1, 4, 9
- ⬜ Purchase **Modular Fantasy Kingdom** ($300 → $150 Spring Sale)
  - Castle walls, towers, gates, city pieces
  - Use for Moons 1, 4, 5, 10
- ⬜ Download **Polyhaven Textures** (FREE)
  - 10 PBR material sets (mud, stone, metal, wood)
  - Already have: Ground054, Marble006, Metal032, etc.
- ⬜ Download **Kenney Nature Pack** (FREE, CC0)
  - Trees, rocks, grass
  - Use for all outdoor moons

**Deliverable:** Environment library covering 8/13 moons

---

#### **Buildings (P1)**
- ⬜ **Modular Kingdom** covers:
  - Star Dome → Cathedral spire variant
  - Harmonic Fountain → Courtyard fountain
  - Crystal Spire → Tower piece
- ⬜ Procedural detail via **TartarianArchitectureBuilder.cs**
  - Auto-generates ornamental geometry
  - Already implemented

**Deliverable:** 3 hero buildings upgraded from primitives → modular pieces

---

#### **VFX (P2)**
- ⬜ Purchase **Realistic Water VFX** ($45 → $22.49 Spring Sale)
  - Water splashes, ripples, rain
- ⬜ Download **Brackeys VFX Pack** (FREE, CC0)
  - 50 particle effects (fire, smoke, magic)
- ⬜ Purchase **Feel** ($50 → $25 Spring Sale)
  - Juice/feedback plugin (screen shake, hitstop, camera punch)
  - Industry standard for game feel

**Deliverable:** 60+ VFX prefabs ready to apply

---

#### **Animations (P1)**
- ✅ **DONE:** 8 Mixamo animations (Idle, Walk, Run, Jump, Fall, Dig, SwordSlash, TakeDamage)
- ⬜ Download **Quaternius Animation Pack** (FREE)
  - 120 retargetable animations
  - Add: climb, swim, push, pull, interact, emotes
- ⬜ Purchase **Motion Library** ($20 → $10 Spring Sale) — OPTIONAL
  - 200+ mocap animations
  - Higher quality than Mixamo

**Deliverable:** 130+ animations across all character types

---

#### **UI/Menus (P2)**
- ⬜ Download **Kenney UI Pack** (FREE, CC0)
  - 1000+ UI elements (buttons, panels, icons)
- ⬜ Download **Monogram Font** (FREE, CC0)
  - Pixel-perfect font for UI
- ⬜ Download **Game-icons.net** (FREE, CC-BY)
  - 4000+ SVG icons (inventory, abilities, status)

**Deliverable:** Complete UI asset library

---

#### **Audio (P3)**
- ⬜ Download **Tallbeard Music** (FREE, CC0)
  - 200 music loops (ambient, combat, exploration)
- ⬜ Download **Freesound** essentials (FREE)
  - 100 core SFX (footsteps, UI clicks, ambience)
- ⬜ Download **Sonniss GDC 2026 Bundle** (FREE annual)
  - 50GB+ pro SFX library

**Deliverable:** Full audio library (music + SFX)

---

**Phase 0 Total Cost:** $279.74 (Spring Sale ends May 6!)  
**Phase 0 Total Time:** 40 hours (2 weeks)  
**Phase 0 Automation:** Already built (OneClickBuild Phases 9h-9i)

---

### **PHASE 1: Moons 1-4 Build (Weeks 3-6) — $0 (uses Phase 0 assets)**
**Goal:** Ship first 4 moons at 75-80/100 quality  
**Quality Gain:** 55/100 → 75/100 (+20 points)

#### **Moon 1: Magnetic Cathedral (Echohaven)**
**Theme:** Aether awakening, sacred geometry  
**Locations:** Star Dome, plaza, underground catacombs

**Assets:**
- Environment: Fantasy Adventure Environment (cathedral interiors)
- Building: Star Dome → Modular Kingdom cathedral spire
- NPCs: Anastasia (Queen), Milo (Worker), 3 villagers (Polygon pack)
- Props: Aether veins (custom shader), prayer candles, stone benches
- VFX: Dome awakening burst (already built), aether motes, scan pulses
- Audio: Harmonic choir (already have), organ drones, footstep echoes

**Unique Requirements:**
- APV lightmap scenarios (Dawn_PreAwakening, Dome_Awakening) — DONE
- Restoration shader on Star Dome — DONE
- DomeAwakeningBurst VFX graph — DONE

**Automation:**
- EchohavenContentSpawner.cs (already wires buildings)
- PBRSceneApplier.cs (applies cathedral materials)
- VFXController.cs (dome awakening trigger)

**Estimated Build Time:** 20 hours  
**Quality:** 85/100 (best moon, tutorial level)

---

#### **Moon 2: Lunar Corruption (Mossy Caverns)**
**Theme:** Mud golems, cleansing ritual  
**Locations:** Corrupt swamp, golem spawning pools, Harmonic Fountain

**Assets:**
- Environment: Kenney Nature (moss, vines) + procedural terrain
- Building: Harmonic Fountain → stone fountain (Modular Kingdom)
- NPCs: MudGolem (Mutant), Milo companion
- Props: Corruption nodes (custom shader), mud pits, crystal shards
- VFX: Golem spawn (Corruption shader), cleansing waves, mud splashes
- Audio: Bubbling mud, golem roars, water drips

**Unique Requirements:**
- Corruption shader on golem spawn rings — DONE
- MudZone.cs (slow movement in mud) — DONE
- MudGolemAI.cs behavior — BASIC IMPLEMENTED

**Automation:**
- BuildingSpawner.cs (fountain wiring)
- RuntimePBRApplier.cs (moss/mud materials)

**Estimated Build Time:** 18 hours  
**Quality:** 80/100 (strong visual variety)

---

#### **Moon 3: Electric Trains (Railway Junction)**
**Theme:** Orphan train puzzle, temporal anomaly  
**Locations:** Train station, rail yards, signal tower

**Assets:**
- Environment: Kitbashed industrial (Unity ProBuilder + free models)
- Building: Crystal Spire → signal tower (Modular Kingdom tower piece)
- NPCs: 5 orphan children (Polygon pack kids), station master
- Props: Train cars (kitbash cubes + wheels), tracks, lanterns
- VFX: Electric sparks, steam puffs, signal lights
- Audio: Train whistle, steam hiss, clock ticking

**Unique Requirements:**
- OrphanTrainPuzzle.cs — BASIC IMPLEMENTED
- Train movement (Timeline or DOTween)
- Track switching mechanic

**Automation:**
- ProBuilder for train geometry
- Timeline for cutscenes

**Estimated Build Time:** 25 hours (most complex puzzle)  
**Quality:** 70/100 (kitbashed, but functional)

---

#### **Moon 4: Self-Existing Star Fort (Military Fortress)**
**Theme:** Resonance defense, harmonic weaponry  
**Locations:** Star-shaped fortress, ramparts, command center

**Assets:**
- Environment: Modular Kingdom (castle walls, battlements)
- Building: All 3 buildings (Dome, Fountain, Spire as fortress pieces)
- NPCs: Cassian (knight), 8 soldiers (Polygon pack)
- Props: Cannons, flags, weapon racks, fortifications
- VFX: Resonance cannon fire, shield barriers, explosions
- Audio: Cannon booms, marching, battle horns

**Unique Requirements:**
- Defense minigame (waves of golems)
- Resonance cannon aiming/firing
- Fort integrity system (destructible walls)

**Automation:**
- Wave spawner (adapt MudGolemAI)
- Destructible environment (Unity Physics)

**Estimated Build Time:** 22 hours  
**Quality:** 85/100 (most polished combat)

---

**Phase 1 Total Cost:** $0 (uses Phase 0 library)  
**Phase 1 Total Time:** 85 hours (3-4 weeks with breaks)  
**Phase 1 Output:** 4/13 moons complete, ~3-4 hours gameplay

---

### **PHASE 2: Moons 5-8 Build (Weeks 7-10) — $505 OR AI alternative $40**
**Goal:** Expand to 8/13 moons at 75/100 quality  
**Quality Gain:** 75/100 → 78/100 (+3 points, diminishing returns)

#### **Asset Procurement Options:**

**Option A: Purchase Phase 2 Packs ($505)**
- Victorian Train Station ($99 → $50 sale)
- Gothic Cathedral Expansion ($150 → $75 sale)
- Airship Fleet ($200 → $100 sale)
- Medieval Props Pack ($560 → $280 sale)

**Option B: AI Generation ($40 for 1 month)**
- Meshy.ai ($20/mo) — 200 3D model generations
- Blockade Labs Skybox AI ($10/mo) — 50 skyboxes
- Suno AI ($10/mo) — 500 music generations

**Recommendation:** **Option B (AI)** for budget efficiency

---

#### **Moon 5: Overtone White City (Celestial Metropolis)**
**Theme:** Harmonic civilization peak, marble architecture  
**Locations:** White pavilions, concert hall, sky gardens

**Assets (AI Generated):**
- Meshy.ai: "white marble pavilion with gold trim" (×5 buildings)
- Blockade Labs: "golden hour city skyline, white buildings"
- Kenney modular pieces (kitbashed)
- Polygon pack citizens (×12)

**Estimated Build Time:** 20 hours  
**Quality:** 80/100 (AI + kitbash hybrid)

---

#### **Moon 6: Rhythmic Pipe Organ (Resonance Cathedral)**
**Theme:** Musical puzzle, harmonic patterns  
**Locations:** Pipe organ chamber, resonance halls, bell tower

**Assets:**
- Modular Kingdom cathedral pieces
- ProBuilder for organ pipes (procedural)
- Polygon pack musicians
- Tallbeard organ music (free loops)

**Unique Requirements:**
- Musical puzzle (Simon Says pattern matching)
- Organ keyboard interaction
- Harmonic resonance visualization

**Estimated Build Time:** 18 hours  
**Quality:** 75/100 (music-driven)

---

#### **Moon 7: Resonant Giant (Korath Boss Fight)**
**Theme:** Giant awakening, titan scale  
**Locations:** Giant's chamber, resonance amplifiers, observation deck

**Assets:**
- **CRITICAL GAP:** Korath giant model
  - **Option A:** Commission on Fiverr ($400, 2-4 weeks)
  - **Option B:** Meshy.ai "stone titan giant, 50ft tall" ($20/mo, 5 minutes)
  - **Option C:** Scale up MudGolem (×10 scale, shader variant)
- Modular Kingdom stone chamber
- Polygon pack scientists/observers

**Unique Requirements:**
- Shadow of the Colossus-style boss fight
- Climbing mechanics (player scales Korath)
- Weak point targeting system

**Estimated Build Time:** 25 hours (boss complexity)  
**Quality:** 65/100 with Option C, 85/100 with commissioned model

---

#### **Moon 8: Galactic Airships (Sky Fleet)**
**Theme:** Aerial navigation, airship battles  
**Locations:** Flagship deck, sky docks, crow's nest

**Assets:**
- Kitbashed airship (Unity primitives + Modular Kingdom pieces)
- Polygon pack crew (captain, sailors)
- Blockade Labs skybox "clouds at sunset"
- Brackeys VFX (wind, propellers)

**Unique Requirements:**
- Airship movement (Timeline or rigidbody physics)
- Sky navigation system
- Aerial combat (optional)

**Estimated Build Time:** 22 hours  
**Quality:** 70/100 (kitbashed but functional)

---

**Phase 2 Total Cost:** $40 (AI subscriptions) OR $505 (asset packs)  
**Phase 2 Total Time:** 85 hours (4 weeks)  
**Phase 2 Output:** 8/13 moons complete, ~6-8 hours gameplay

---

### **PHASE 3: Moons 9-13 Build (Weeks 11-16) — FREE (greybox + AI)**
**Goal:** Complete all 13 moons at 60/100 minimum quality  
**Quality Gain:** 78/100 → 68/100 average (-10 points, but SHIPPED)

**Strategy:** Accept lower quality for final 5 moons, ship complete campaign, improve in updates

#### **Moon 9: Solar Clock Tower**
**Assets:** FREE kitbashing + ProBuilder greybox  
**Time:** 15 hours | **Quality:** 55/100

#### **Moon 10: Planetary Grid + Trains**
**Assets:** Reuse Moon 3 trains, add grid overlay  
**Time:** 12 hours | **Quality:** 55/100

#### **Moon 11: Spectral Ghosts (Anastasia)**
**Assets:** SpectralGhost shader (DONE) + Polygon pack ghosts  
**Time:** 18 hours | **Quality:** 60/100

#### **Moon 12: Crystal Bell Tower**
**Assets:** ProBuilder tower + crystal shaders  
**Time:** 20 hours | **Quality:** 50/100 (weakest visually)

#### **Moon 13: Cosmic Convergence (Finale)**
**Assets:** Kitbash all previous moon pieces, VFX-heavy  
**Time:** 25 hours | **Quality:** 55/100

**Phase 3 Total Cost:** $0  
**Phase 3 Total Time:** 90 hours (6 weeks)  
**Phase 3 Output:** 13/13 moons complete, ~10-13 hours gameplay

---

### **PHASE 4: Polish Pass (Weeks 17-20) — $300 (optional)**
**Goal:** Raise average quality from 68/100 → 75/100  
**Quality Gain:** +7 points across weakest systems

#### **Focus Areas:**
1. **UI/Menus Overhaul** (20 hours)
   - Implement Kenney UI pack
   - Add animations (DOTween)
   - Settings menu (graphics, audio, controls)
   - Inventory redesign

2. **Animation Blending** (15 hours)
   - Smooth transitions between all 130+ animations
   - IK foot placement
   - Root motion for combat

3. **Audio Mix** (25 hours)
   - FMOD integration ($0 for indie)
   - Adaptive music (per-moon themes)
   - 3D spatial audio for all SFX

4. **Performance Optimization** (30 hours)
   - LOD generation (Unity Mesh Simplifier)
   - Occlusion culling setup
   - Batching/instancing for props
   - Target: 60 FPS on mid-range PC

5. **Narrative Polish** (20 hours)
   - Voice acting for Anastasia (ElevenLabs AI, $5/mo)
   - Dialogue timing improvements
   - Quest journal UI

**Phase 4 Total Cost:** $300 (optional commissions)  
**Phase 4 Total Time:** 110 hours (4 weeks)

---

## 📅 MASTER TIMELINE

| Week | Phase | Focus | Cost | Output |
|------|-------|-------|------|--------|
| **1-2** | P0 | Asset Procurement | $280 | Library established |
| **3-4** | P1.1 | Moons 1-2 | $0 | 2 moons @ 80/100 |
| **5-6** | P1.2 | Moons 3-4 | $0 | 4 moons @ 75/100 |
| **7-8** | P2.1 | Moons 5-6 + AI setup | $40 | 6 moons @ 75/100 |
| **9-10** | P2.2 | Moons 7-8 | $0 | 8 moons @ 70/100 |
| **11-12** | P3.1 | Moons 9-11 | $0 | 11 moons @ 60/100 |
| **13-14** | P3.2 | Moons 12-13 | $0 | 13 moons COMPLETE |
| **15-16** | P4.1 | UI/Animation | $0 | Quality +5 pts |
| **17-18** | P4.2 | Audio/Performance | $0 | Quality +2 pts |
| **19-20** | P4.3 | Narrative/Final QA | $300 | SHIP READY |

**Total Timeline:** 20 weeks (May → September 2026)  
**Total Cost:** $320 (bare minimum) to $2,868 (full AA)

---

## 💰 COST BREAKDOWN BY TIER

### **Tier 1: Bare Minimum ($320)**
- Phase 0 Spring Sale: $280
- Phase 2 AI tools: $40
- **Result:** 13 moons @ 65/100 average, shippable but rough

### **Tier 2: Indie Quality ($725)**
- Tier 1 baseline: $320
- Phase 2 asset packs: $505 (instead of AI)
- **Result:** 13 moons @ 72/100 average, competitive indie quality

### **Tier 3: AA Quality ($2,868)**
- Tier 2 baseline: $725
- Korath commission: $400
- Phase 3 asset packs: $900
- Phase 4 polish: $300
- Voice acting: $43
- Additional tools: $500
- **Result:** 13 moons @ 85/100 average, Fortnite Chapter 1 equivalent

---

## 🤖 AUTOMATION CHECKLIST

### **Already Automated:**
- ✅ Character FBX import (FBXImportWizard.cs)
- ✅ PBR material application (PBRMaterialBinder.cs)
- ✅ Custom shader creation (CustomShaderApplicator.cs)
- ✅ VFX prefab generation (VFXUpgradeTool.cs)
- ✅ Scene population (EchohavenContentSpawner.cs)
- ✅ Build validation (BatchReadinessValidator, 25 checks)
- ✅ One-click build (OneClickBuild.cs, 17 phases)

### **To Be Automated:**
- ⬜ Asset Store package import (Unity Package Manager CLI)
- ⬜ Prefab variant generator (1 model → 10 color variants)
- ⬜ LOD auto-generation (Unity Mesh Simplifier API)
- ⬜ Audio batch processor (normalize, compress, tag)
- ⬜ Per-moon skybox assignment (SkyboxFactory.cs expansion)
- ⬜ VFX template duplication (one-click variants)
- ⬜ Animation retargeting (Mixamo → Polygon characters)
- ⬜ Lightmap baking per-moon (APV scenario expansion)

**Automation ROI:** 150 hours manual work → 15 hours automated = **135 hours saved**

---

## 🎯 CRITICAL PATH DEPENDENCIES

### **Week 1 (NOW):**
1. ⚠️ **BUY Spring Sale assets before May 6** (saves $320)
2. ⚠️ **Download all Mixamo characters** (baseline NPCs)
3. ⚠️ **Download FREE asset packs** (Kenney, Quaternius, Polyhaven)

### **Week 2:**
1. Import all Phase 0 assets
2. Test character rigs in-game
3. Verify build pipeline GREEN

### **Weeks 3-6:**
1. Build Moons 1-4 sequentially
2. Playtest each before moving to next
3. Hit 4-hour gameplay milestone

### **Decision Point (Week 6):**
- **If quality ≥75/100:** Continue to Phase 2 (Moons 5-8)
- **If quality <75/100:** Loop back, improve Moons 1-4 first
- **If budget tight:** Ship Moons 1-8 only, call it "Act 1"

---

## 📊 RISK MITIGATION

### **High Risk:**
1. **Spring Sale ends May 6** → Buy now or lose $320 savings
2. **Scope creep** → Lock features after Phase 1
3. **Asset compatibility** → Test all imports Week 2

### **Medium Risk:**
1. **AI quality variance** → Generate 3 options, pick best
2. **Performance issues** → Profile early, optimize often
3. **Animation jank** → Use Mixamo as fallback

### **Low Risk:**
1. **Audio gaps** → Freesound + Tallbeard cover 90% needs
2. **UI placeholder** → Kenney pack is industry-proven
3. **Build pipeline breaks** → Already validated GREEN

---

## 🚀 SHIPPING OPTIONS

### **Option A: Full 13 Moons (September 2026)**
- **Pros:** Complete vision, full campaign
- **Cons:** 5 months dev time, quality variance
- **Price:** $14.99 Steam Early Access

### **Option B: Moons 1-8 Only (June 2026)**
- **Pros:** Shippable in 6 weeks, consistent quality
- **Cons:** Incomplete story, smaller scope
- **Price:** $9.99 Early Access, "Act 1 of 3"
- **Update Path:** Add Moons 9-13 in Q3 2026

### **Option C: Vertical Slice (May 2026)**
- **Pros:** Moon 1 polished to 90/100, demo-ready
- **Cons:** 90 minutes gameplay only
- **Price:** FREE on itch.io
- **Goal:** Kickstarter June 2026, fund Phases 2-4

---

## 🎮 RECOMMENDED PATH

**Execute Option B (Moons 1-8):**

1. **Now (Week 1):** Buy Spring Sale assets ($280), download FREE packs
2. **Week 2:** Import + test all assets
3. **Weeks 3-6:** Build Moons 1-4 @ 75-80/100
4. **Weeks 7-10:** Build Moons 5-8 @ 70-75/100
5. **Ship June 9, 2026:** Steam Early Access, $9.99
6. **Gather revenue + feedback:** Validate market fit
7. **Q3 2026:** Use revenue to fund Moons 9-13 completion

**Why this works:**
- ✅ Achievable timeline (6 weeks vs 20 weeks)
- ✅ Consistent quality (70-80/100 vs 50-85/100 variance)
- ✅ Revenue earlier (validate demand before full investment)
- ✅ Lower risk (8 moons shippable vs betting on 13)
- ✅ Community feedback loop (improve based on real players)

---

## 📈 SUCCESS METRICS

### **Phase 0 (Asset Library):**
- [ ] All Spring Sale purchases complete
- [ ] 16 character models imported
- [ ] 60+ VFX prefabs ready
- [ ] 200+ audio clips organized
- [ ] Build pipeline GREEN with new assets

### **Phase 1 (Moons 1-4):**
- [ ] Each moon playable 45-60 minutes
- [ ] Zero game-breaking bugs
- [ ] 60 FPS on mid-range PC
- [ ] All core mechanics working (inventory, dialogue, quests)

### **Phase 2 (Moons 5-8):**
- [ ] 6-8 hours total gameplay
- [ ] Save/load tested across all moons
- [ ] All character animations blend smoothly
- [ ] Audio mix balanced

### **Phase 3 (Moons 9-13):**
- [ ] 10-13 hours campaign complete
- [ ] Final boss functional
- [ ] Ending cinematic plays
- [ ] Credits roll

### **Phase 4 (Polish):**
- [ ] Steam Early Access page live
- [ ] Trailer recorded
- [ ] Wishlist campaign launched
- [ ] First 10 beta testers recruited

---

## 🛠️ TOOLS & SOFTWARE STACK

### **Asset Creation:**
- Unity 6000.3.6f1 (engine)
- Blender 4.1 (3D modeling — FREE)
- Substance Player (material application — FREE)
- GIMP 2.10 (texture editing — FREE)
- Audacity (audio editing — FREE)

### **AI Tools:**
- Meshy.ai ($20/mo) — 3D model generation
- Blockade Labs ($10/mo) — Skybox generation
- Suno AI ($10/mo) — Music generation
- ElevenLabs ($5/mo) — Voice acting

### **Automation:**
- Unity Package Manager (CLI)
- PowerShell scripts (download automation)
- Python scripts (batch processing)
- EditorWindow tools (already built)

### **Version Control:**
- Git + GitHub (code)
- Git LFS (large assets)
- Unity Plastic SCM (optional, Unity-native)

---

## 📝 NEXT STEPS (IMMEDIATE)

1. **READ THIS PLAN** — Understand full scope
2. **DECIDE ON TIER** — $320 minimum, $725 indie, $2,868 AA
3. **BUY SPRING SALE** — Before May 6 (6 days left!)
4. **DOWNLOAD FREE PACKS** — Kenney, Quaternius, Polyhaven
5. **FINISH MIXAMO DOWNLOADS** — 3 characters remaining
6. **RUN IMPORT TEST** — Verify all assets work in Unity
7. **START MOON 1** — Use existing Echohaven as foundation

**Want me to start automating any of these systems?**

- Asset Store auto-importer
- Prefab variant generator
- LOD pipeline
- Per-moon build scripts
- AI generation integration
- Skybox auto-switcher

---

**END OF MASTER PLAN**

