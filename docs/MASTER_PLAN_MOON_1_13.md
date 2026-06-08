# MASTER PLAN — Moon 1 → Moon 13

> **STATUS:** LOCKED · v1.0 · 2026-06-08 (R146) · single source of truth for per-Moon sprint scope
> Reads upstream from `docs/15_MVP_BUILD_SPEC.md` (Moon 1) + `docs/03_CAMPAIGN_13_MOONS.md` (Moons 2-13 narrative) + `docs/MOON_BLUEPRINT.md` (template) + `docs/32_ART_BIBLE.md` (style law).

Companion to `CLAUDE.md §R146 CANON LOCK`. Override authority over any older planning doc.

---

## How this plan is used

1. Every sprint targets **ONE Moon at a time**. No multi-Moon hammer rounds.
2. A Moon ships when its **8-step smoke test** passes (per `MOON_BLUEPRINT §16`) with screenshot proof.
3. A Moon's content is **only what its spec says** (linked below). New ideas go in the spec first, not the scene.
4. Each Moon inherits 13 system pillars but adds its **distinct mechanic** + **distinct aesthetic note** (still within Art Bible 3-hue palette).
5. The 8 FOUNDATIONS phases (`docs/plans/MOON1_FOUNDATIONS_FIRST.md`) are infrastructure shared by ALL Moons. Build foundation once, reuse 13 times.

---

## Moon-by-Moon scope (canonical, pulled from `03_CAMPAIGN_13_MOONS.md` + `03C_MOON_MECHANICS_DETAILED.md`)

| # | Moon name | Setting | Mechanic | Hero structures | Boss / climax | Companion focus | Canonical aesthetic note |
|---|---|---|---|---|---|---|---|
| 1 | **Magnetic Moon — Echohaven** | New Chicago underground, buried Tartarian dome district | Restore first Listeners' Hall via 3-band tuning + 28-day arc | Dome (25×18 m) + Fountain (8×5 m) + Spire (3×15 m) | Day 28 Dome Awakening cinematic + first Mud Golem | Milo intro + Lirael Day 25 lullaby; Anastasia + Cassian cameo at climax | Warm amber daylight, gold seam emissive, 3 hues: Gold + Cyan + neutrals |
| 2 | **Lunar Moon — Shadow & Purge** | New Chicago upper district, Crystal Cathedral | Dissonance purge mini-game + micro-giant burst (60s) | Crystal Cathedral + corruption ribbons | First Reset Agent encounter | Cassian fully introduced; Milo grows | Cool violet shadow + cyan corruption purge; 3 hues: Violet + Cyan + Crimson hints |
| 3 | **Electric Moon — Resonance Trains** | Orphanage line + railway crystals | Train-line restoration + adopt 4 orphans for choir | Resonance Locomotive + 2 stations | Orphan train rescue cinematic | Lirael leads choir; Anastasia archives | Electric arcs + Aether-Cyan rails; 3 hues: Cyan + Gold + neutrals |
| 4 | **Self-Existing Moon — Star Fort** | Tartarian star-fort archipelago | Polygon-snap building + reveal that the boss is Maelix, Korath's corrupted brother | Star Fort bastion + 5 outwork ravelins | Maelix golem boss | Korath ally hint; Cassian deepens | Bronze + cyan; gothic sacred geometry; 3 hues: Gold + Violet + neutrals |
| 5 | **Overtone Moon — White City** | Pavilion zones around the lost White City | Overtone tuning chord puzzles + Thorne intro | 7 pavilion buildings around central rotunda | Pavilion harmonic chord climax | Captain Thorne airship pilot introduction | Bright marble + glass; 3 hues: Gold + Cyan + Violet (max palette stretch) |
| 6 | **Rhythmic Moon — Sunken Cathedral** | Underwater Aether organ ruins | Rhythm/cymatic organ mini-game | Sunken Cathedral pipe organ + 4 antiphonal towers | Pipe organ awakening | Lirael conducts | Underwater blue-green refraction; 3 hues: Cyan + Gold + emerald accent |
| 7 | **Resonant Moon — Korath's Thaw** | Frozen mountain Aether vault | Multi-band layered tuning; Korath sacrifice cinematic | Vault doors + Korath statue + first true Cabal site | Cassian moral choice + Korath sacrifices himself | Korath dies; Cassian confronted | Frozen-then-warming gold thaw; 3 hues: Cyan + Gold + white-warm neutral |
| 8 | **Galactic Moon — Airship Armada** | Sky armada vs Cabal fleet | Air combat + boarding | Captain Thorne's flagship + 5 escort airships | Cabal airship boss fight | Thorne party-lock | Sky violet + gold sun; 3 hues: Violet + Gold + neutrals |
| 9 | **Solar Moon — Prophecy Stones** | Sun temple desert | Collect 6/12 prophecy stones + sun-ray puzzles | Sun Temple + 6 satellite obelisks | Prophecy reveal cinematic | Anastasia channels prophecy | Hot amber + corruption crimson; 3 hues: Gold + Crimson + neutrals |
| 10 | **Planetary Moon — Continental Trains** | Cross-continental rail + Mud Flood Trigger Room | Planetary rail-grid restoration + Mud Flood Trigger Room reveal | Continental Rail Hub + Trigger Room | Mud Flood trigger reveal cinematic | All companions share screen | Cool blue + corruption crimson; 3 hues: Cyan + Crimson + Gold edge |
| 11 | **Spectral Moon — Planetary Aquifer** | Underground river network | Aquifer purification chain; planetary fountain link | 13 aquifer nodes + planetary fountain | Aquifer purification climax | Lirael's lullaby planet-wide | Spectral cyan + gold purification glow; 3 hues: Cyan + Gold + violet accent |
| 12 | **Crystal Moon — Bell Tower Ring** | Planetary bell-tower chain | Bell-tower tuning chain (φ ratio across 13 towers) | 13 Crystal Bell Towers + ring conduit | Ring resonance climax | Anastasia + Lirael harmonize | All 3 Aether colors balance; 3 hues: Gold + Cyan + Violet |
| 13 | **Cosmic Moon — Echo Realms** | Outside-time confrontation in the Cosmic Cathedral | 3-way ending choice: Harmony / Echo / Reset | Cosmic Cathedral + Zereth's chamber + Cabal sanctum | Zereth truth reveal; Cabal final boss | Full party present | Harmony: all 3 Aether colors balanced gold-dominant; Echo: 2-timeline split; Reset: corrupted crimson + violet |

Per-Moon spec docs (one file each) live at `docs/moons/moon_NN_<name>.md`. Currently authored:
- ✅ `docs/15_MVP_BUILD_SPEC.md` (Moon 1, 37 KB, canonical)
- ✅ `docs/16_MOON2_BUILD_SPEC.md` (Moon 2, exists per audit)
- ⏸ Moons 3-13 — author one per sprint using `MOON_BLUEPRINT.md` template

---

## FOUNDATIONS phases shared by all 13 Moons (`docs/plans/MOON1_FOUNDATIONS_FIRST.md`)

| Phase | Status | Notes |
|---|---|---|
| 0 — Git LFS pull | ✅ DONE (R124) — 4.4 GB |
| 1 — Quarantine 5 banned mutators | ✅ DONE (R133c) |
| 2 — URP settings (LinearIntensity + SSAO + Saturation + ReflectionProbe) | ✅ DONE (R124) |
| 3 — Bake APV + lightmaps + NavMesh | ⏸ pending — do in R146-R150 cleanup |
| 4 — Real Unity Terrain | ✅ DONE (R132) but with WRONG splat textures (Polyhaven 4K — replace with stylized matte per Art Bible) |
| 5 — Art pack | 🚫 RECONSIDER per Art Bible whitelist (Quaternius Ruins + KayKit Medieval Builder Legacy, NOT KayKit Hexagon, NOT gen_v2 PBR cubes) |
| 6 — Hero buildings | 🚫 REJECT R126-R142 — only 3 buildings spec'd, not 12. Replace with primitive blockouts then proper Blender authoring per Art Bible |
| 7 — Mecanim humanoid NPCs | ✅ DONE (R134) — KayKit Adventurer placeholders work for slice |
| 8 — Real 8-step smoke test (VIDEO proof) | ⏸ blocked on R146-R150 cleanup |

---

## R146-R150 cleanup sprint (immediate)

| Round | Action | Done check |
|---|---|---|
| R146 | Lock canon in CLAUDE.md + author this MASTER_PLAN + take "before" screenshot | done |
| R147 | Quarantine `Moon1_Props`, `Moon1_Vegetation`, `Moon1_VFX`, `Moon1_LeyLines_V2`, `Moon1_DensityPlus`, `Moon1_Density` scene parents (set inactive — recoverable) + delete 12 placed gen_v2 buildings from scene. Take "after" screenshot. | pending |
| R148 | Drop 3 primitive blockouts for Dome (25×18m hemisphere on cylinder), Fountain (8m basin + 5m column), Spire (3m × 15m cone-on-cube) at canonical positions per `15_MVP §7`. Tag each Static + ContributeGI. Take Game-view screenshot. | pending |
| R149 | Wire 1 Mud Golem placeholder (KayKit Skeleton_Warrior) + 4 NPC placeholders at canonical spawns (Milo near Dome, Lirael near Spire, Anastasia post-restore at Fountain, Cassian at Overlook). Apply Art Bible lighting (warm key + cool fill + Neutral tonemap + bloom 1.1). | pending |
| R150 | Run 8-step smoke test programmatically (`manage_editor play` + screenshot each beat). If fails, file gap + iterate ONE step before declaring done. | pending |

---

## Sprint cadence (after R146-R150)

| Sprint | Scope |
|---|---|
| Sprint A (R151-R170) | Author the 3 hero buildings PROPERLY in Blender per Art Bible (matte stone, gold seam emissive, sacred geometry). Replace primitive blockouts. |
| Sprint B (R171-R180) | Tuning mini-game variants A/B/C fully wired + RS economy + Day 1-28 progression |
| Sprint C (R181-R190) | 4 NPCs with dialogue trees + Anastasia rocker beat + Lirael Day 25 lullaby + Cassian intro |
| Sprint D (R191-R200) | 17th-hour cinematic + Dome Awakening climax + Moon 2 portal seed |
| GATE 1 ship | Moon 1 8-step smoke test passes with video proof. Then Moon 2 sprint cycle begins. |

---

## Rules of engagement (every sprint)

1. **Read `docs/15` (Moon 1) and `docs/32` (Art Bible) before any work.** They override CLAUDE.md.
2. **Screenshot every claim.** No `tool returned success → ✅ shipped`.
3. **One Moon at a time.** Don't sprint Moon 2 content while Moon 1 isn't GATE 1.
4. **3-hue discipline per shot.** If a screenshot has 4+ primary colors, it's wrong.
5. **No purchases without Art Bible amendment.** Synty / asset packs not on the whitelist need spec-side approval first.
6. **Per-Moon spec doc OR don't ship.** Moons 3-13 each need a `docs/moons/moon_NN_*.md` before scope authoring begins.

---

*v1.0 · 2026-06-08 · LOCKED. Amend in-place when a Moon ships. Authority over any older planning doc including the FOUNDATIONS_FIRST plan.*
