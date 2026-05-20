# TARTARIA — Active Development Context

**Date**: 2026-05-20  
**Canonical Working Directory**: `C:\dev\TARTARIA_new`

---

## CRITICAL RULE — Location

**Only ever build, edit, or run from here:**
`C:\dev\TARTARIA_new`

**Strictly forbidden:**
- Any path containing `OneDrive`
- Previous copies under `C:\dev\TARTARIA` (older)
- Any desktop or Documents sync folders

Unity projects + OneDrive file sync = constant corruption of `.meta` files, `Library/`, `Temp/`, and asset database. We do **not** fight OneDrive. We stay on raw local SSD under `C:\dev`.

If you ever see a path with `OneDrive` in any swarm output, agent log, or suggestion — immediately reject it and redirect to `C:\dev\TARTARIA_new`.

---

## Current Focus (Echohaven Vertical Slice)

**Primary Target**: Make the **Harmonic Fountain restoration** the strongest, most memorable 5–10 minute player experience in the game.

Pillars being actively hammered:

1. **Visuals & World Building** (KayKit composition for fountain + dome + spire, emergence, VFX, plaza density)
2. **Player Feel** (Excavation weight, tuning juice, HarmonicStrike reactivity, Resonance High buff, combat synergy)
3. **Aether / Ley Lines / Simulation** (Making the injected `AetherSource` produce visible field and ley line reactions)
4. **Companions & Zone Life** (Milo + others reacting physically and narratively, zone transformation, music shift)
5. **Integration & Polish** (Making all the above feel like one cohesive, production-quality moment rather than systems firing)

---

## Recent Major Swarm Deliveries

### Companions / Dialogue / Zone Reactivity Agent (Completed successfully)
- Full multi-NPC reactions to first fountain restoration (Milo gets the biggest treatment with physical behavior + sincere dialogue).
- `GameEvents.OnFountainFirstRestored` + orchestration in `GameLoopController`.
- Zone visual/audio transformation (golden motes, light, fog, music crescendo via `AdaptiveMusicController`).
- Persistent progress acknowledgments in later idle dialogue.
- Save persistence wired.

### Player Experience Agent (Completed successfully)
- Deep excavation feel: progressive dirt bursts, layered rumble, intensifying camera shake, proper "mound breaking" cinematic for all three buildings.
- Tuning juice on Pipe Organ: camera breathing, rising aether motes on correct inputs, resonance crystals pulsing in sync with the player.
- Strengthened HarmonicStrike feedback on the restored crystals (chain reactions, combat tutorial).
- Polished Resonance High post-restoration buff with visible player-attached motes and footstep aether trails.
- First MudGolems turned into an explicit "use your new resonance power" tutorial with vulnerability and extra payoff.

### First Combat Encounter Polish — Echohaven MudGolems + Resonance Tutorial (Completed)
- Made the opening 1-2 waves tight, clear, and deeply rewarding: wave sizes reduced to {2,3,4}, fountain-centered spawns for wave 0 so the fight literally happens "near the restored crystals".
- Critical fix: Harmonic Strike (F) and Resonance Pulse now correctly damage AI-backed MudGolems via the GameObject fallback path (previously the resonance multiplier and tutorial health never applied to actual deaths).
- All wave-0 golems are explicitly resonance-vulnerable (2.8x), low-health (16), with strong visual aether shimmer + cyan light telegraph so the player instantly understands "these are the resonance targets".
- Big amplified damage numbers, white hitflash, knockback, and on-death "RESONANCE OVERLOAD" VFX + crystal chain pulses + bonus RS fire beautifully when using F near the fountain — the exact teaching moment now lands with satisfying weight.
- Improved pacing, objective text, banners, and prompts that repeatedly teach + celebrate using the new power ("F near crystals = bigger hits!").
- Ragdoll lifetime extended slightly for death impact; dual health paths unified for tutorial.
- The moment now ties directly and memorably into the Harmonic Fountain restoration fantasy. Closes the "first combat encounter polish" gap.

---

## Currently Active Agents (as of latest check)

- **Player Feel** agent → Just finished strong.
- **Visuals & KayKit** agent → **DELIVERED** (major KayKit composition + plaza density + emergence polish for all 3 Echohaven buildings; see "Recently Delivered" section).
- **Aether/LeyLine** (safe exploration mode) → Relaunched, still running.
- **Integration & Vertical Slice Closer** (safe mode) → Relaunched, still running.

---

## Open High-Priority Gaps (next swarm waves should target)

- Actual ley line connections lighting up visibly when the three buildings are restored
- Overall Echohaven flow, pacing, and onboarding around the fountain moment
- Save / persistence for the full restoration state across sessions
- (Tuning mini-game deep polish delivered 2026-05-20 — see Recently Delivered section)

## Recently Delivered: Visuals & KayKit Composition (Echohaven Plaza) — 2026-05-20

**Visuals & KayKit agent — Major delivery (direct main-thread)**

- **StarDome + CrystalSpire now receive full high-quality KayKit rock/prop composition** (exact symmetry with Harmonic Fountain):
  - Added `ComposeDomeWithKayKit` + `ComposeSpireWithKayKit` in TartarianArchitectureBuilder.
  - Dome: heavy 12-stone foundation ring, 6 radial standing pillars, 5 star-crystal shards, 11 foliage overgrowth.
  - Spire: 10-stone wide base rubble, 8 tall vertical rock ribs, 4 upper crystal finial shards, 8 base foliage.
  - All use live KayKit Forest prefabs, instance-seeded RNG, Composition child roots, subtle tilt, 15-20 props/building for rich excavated-site readability.
- **Cleaned legacy broken KayKit code** in BuildingSpawner (misindented "light dressing" block was unreachable inside fountain if).
- **Major plaza density + "excavated ancient site" upgrade**:
  - AmbientVillage: boosted counts + new `PlaceIntentionalCluster` for tight 13-16 prop (rock/bush) clusters *intentionally* around each of the 3 buildings (Dome/Fountain/SpireCluster) — complements global scatter and per-building compositions.
- **Emergence VFX + lighting polished for *all three* buildings**:
  - EmergenceSequence now emits 3 layered dirt/mud bursts (18/26/35 particles, cone spray, gravity) on restore for every building.
  - Tailored permanent point lights post-emergence: StarDome (aether blue 2.2/18), CrystalSpire (violet 2.6/24), Fountain (gold 2.0/22) + emission work.
  - Extra VFX replay + retained fountain-specific upgrades (water color, resonance crystals).
- 100% runtime, zero new assets, follows all existing patterns (RNG, prefabs from wirer, low-cost temp particles, C:\dev\TARTARIA_new only). Directly resolves the top listed gap. Plaza now has production-quality visual density and world-building for the fountain restoration experience.

---

## Recently Delivered: Tuning Mini-Game Deep Polish (Fountain / Pipe Organ + 3-Node) — 2026-05-20

**Tuning / Player Feel follow-up — Direct hammer on Echohaven fountain experience**

- **Targeted the exact open gap**: "Deeper tuning mini-game feedback (PipeOrgan + other variants)" for the Harmonic Fountain 3-node restoration process and post-restoration PipeOrgan conduction.
- **Made the "conducting the harmonic grid" fantasy significantly stronger** across both the sequential 3-node TuningMiniGameController (the core vertical slice moment) and the PipeOrganMiniGame.
- **High-leverage feedback upgrades (visual / audio / camera / haptics on every correct/incorrect input)**:
  - **HapticFeedbackManager** (C:\dev\TARTARIA_new\Assets\_Project\Scripts\Input\HapticFeedbackManager.cs): New juicy discrete methods `PlayTuningCorrectHit(bool perfect, float intensity)`, `PlayTuningMiss()`, `PlayTuningChordComplete()`, `PlayTuningNodeComplete()`. Rich resonant pings for hits (longer tails on perfects), dissonant rattles for misses, celebratory cascades for chord/node milestones. Updated legacy on/off + added BriefDissonanceRattle coroutine. Makes every tap/note/trace feel alive and musical.
  - **PipeOrganMiniGame** (C:\dev\TARTARIA_new\Assets\_Project\Scripts\Gameplay\PipeOrganMiniGame.cs): 
    - Upgraded input handling: perfect timing now drives stronger haptics + double-layered audio + boosted VFX.
    - Richer `SpawnRisingAetherMotes` (perfects get larger radial bursts, more particles, longer life).
    - New `SpawnHarmonicChordSurge()` + `DimFountainResonanceCrystalsBriefly()` + restore coroutine for miss feedback.
    - Dramatically improved `CameraBreathingRoutine`: now includes slow conducting "look sweeps", directed interest leans toward fountain crystals, chord-timed kicks — sells the player as conductor scanning/leaning into registers.
    - Stronger crystal sync + tiny per-chord AetherFieldManager RS injection + final surge on high-accuracy completion. Misses now visibly dim crystals briefly.
  - **TuningMiniGameController** (C:\dev\TARTARIA_new\Assets\_Project\Scripts\Gameplay\TuningMiniGame.cs — the 3-node process for fountain nodes):
    - Injected fountain-aware world reactivity into *all four variants* (FrequencySlider, WaveformTrace, HarmonicPattern, BellTower): every meaningful input now pulses KK_ResonanceCrystals (via CrystalAetherBob + emission MPB), spawns rising golden aether motes (stronger on good/perfect), injects small RS to aether field, and uses the new rich haptics (correct hits with perfect timing, misses, chord/node payoffs).
    - On node success (>=65%+): node-complete haptic + crystal surge + mote burst + aether boost.
    - On fail: explicit miss haptic. Each of the 3 nodes now feels like a distinct "voice of the pipe organ / grid" awakening.
  - **InteractableBuilding** (C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\InteractableBuilding.cs): Enhanced fountain-specific StartTuning branch with per-node crystal BoostPulse + emission spikes + dedicated "NodeStartAetherSurge" particle burst. Every node transition now opens with a powerful "the grid is listening / conducting begins" moment.
- **Integration**: All changes tightly coupled to existing fountain crystals (KK_ResonanceCrystal + CrystalAetherBob), AetherFieldManager, GameState.Tuning camera mode, and the 3-node progression. Zero new assets, fully runtime, uses temp GameObjects + ParticleSystem for VFX (consistent with prior Player Feel delivery).
- Directly hammers the "Harmonic Fountain restoration" as the strongest 5–10 min experience. The 3-node tuning + PipeOrgan now deliver memorable, physically reactive "I am conducting the world" juice.

---



## Process Notes

- We are using a **multi-agent swarm** approach for parallel progress.
- Agents that hit permission walls on file writes are relaunched in "safe exploration + proposal only" mode.
- When an agent produces good work, we review and integrate.
- Direct main-thread hammering is used when swarm agents are blocked.

**Golden Rule**: When in doubt, stay on `C:\dev\TARTARIA_new` and keep the hammer moving on the Echohaven fountain experience.

---

*This file is the single source of truth for current operational context. Update it whenever the swarm delivers major work or the focus shifts.*

---

## Phase 3 Next Wave — Moon 2 Visual Polish & KayKit Follow-up (Agent 01, 2026-05-20)

**Exclusive domain executed**: Visual/artistic work ONLY for Moon 2 (Crystalline Caverns / Murmuring Hollows / Lunar Corruption zone). Zero gameplay mechanics touched. All changes in C:\dev\TARTARIA_new.

**Built directly on previous KayKit dressing** (TartarianArchitectureBuilder.Compose*WithKayKit + Moon2ZoneScaffold ApplyMoon2VisualPolishAndKayKitDressing + per-building tints + clusters + global 160-scatter + lighting/VFX).

**Implemented (all visual polish):**
- **Dynamic restoration reactivity on props**: New pure-visual `Moon2CavernVisualManager` (added to VFXController.cs for hosting) attached to dressing root. Subscribes to existing `GameEvents.OnBuildingRestored`. On any Moon2 building restore:
  - Corruption fade: rock/foliage props lerp from initial dark bruised violet-black baseColor (via MPB) to clean vibrant emerald/amber.
  - Crystal glow: KK_*Crystal/Shard props ramp emission to strong restored glow (2.8x) + mid-pulse sparkles.
  - Self-discovers all KK_ props at init, groups by building inference from names, caches orig colors for safe lerp. Reuses MPB + existing CrystalAetherBob patterns.
- **Wind-responsive foliage**: 
  - Created 5+ `WindZone` components (spherical, gentle 0.22–0.43 main + turbulence) across cluster centers + ambient cavern in `CreateMoon2WindZones`.
  - Runtime: manager's Update applies lightweight multi-sine sway (x/z euler + tiny bob on bushes) to all discovered KK_Foliage/Bush/Grass/Overgrowth transforms. Simulates living wind through crystal glades.
- **Better interior cavern dressing**: New `AddInteriorCavernCrystalDressing` places 7–11 additional KK crystal shards per cluster *inside* building volumes (negative/inner y offsets, varied heights for walls + ceiling lattices). Sells the "fractal cathedral within cathedral" + "translucent amber/violet walls" from 12_VIVID_VISUALS.md and GDD Moon 2 purge vision. All static, named KK_InteriorCrystal_*.
- **Performance cleanup on dense scatter**: 
  - Global forest scatter count reduced 160 → 95.
  - Every placed prop (clusters, global, interior, composition children, lore details) now has `.isStatic = true` for Unity static batching / SRP batcher gains.
  - Single manager for all wind sway + reactivity (no per-prop heavy components). MPB reuse, limited Update iteration.

**Files edited (visual lane only)**:
- `Assets/_Project/Editor/Moon2ZoneScaffold.cs`: enhanced Apply..., Place* helpers, new interior/wind methods, manager wiring, static flags, count reduction.
- `Assets/_Project/Scripts/Integration/TartarianArchitectureBuilder.cs`: (minor static marks in flow, no logic change).
- `Assets/_Project/Scripts/Integration/VFXController.cs`: appended `Moon2CavernVisualManager` visual-only class (event driven MPB anims + wind sway).
- `CONTEXT.md`: this delivery note.

**How to apply**: Open CrystallineCaverns.unity → Tartaria > Moon 2 > Visual Polish & KayKit Dressing (or via full Populate). Re-run after any terrain/building changes. Props become reactive on future restores (or re-init for pre-restored state).

**Remaining gaps (visual/art lane for Moon 2)**:
- Full vertex-color baking on all KayKit foliage meshes so they can use the existing Tartaria/GrassWind shader (current sway is transform-based fallback; FoliageFactory pass would upgrade).
- Shader/material variants for explicit "corruption overlay" texture lerp (current is simple base/emission MPB — good but not fractal vein GDD).
- Interior-specific lighting probes or reflection probes inside the 5 buildings for micro-giant cavern beauty.
- LOD groups or impostors on the densest 95+ prop scatter for very low-end hardware.
- Integration of actual fractal corruption vein decals/meshes (beyond tint) that visibly burn away on purge (would require minor additional VFX calls but still visual).
- Scene-specific post-process volume tweaks for Moon 2 (amber/violet interior contrast) — currently inherits Echohaven profiles.
- Automated re-application of dressing when buildings are procedurally re-spawned (current is editor menu driven).

This closes the "Moon 2 Visual Polish & KayKit (Follow-up)" task per roadmap Phase 3 polish sprint + 12_VIVID_VISUALS + GDD fractal cavern vision. Plaza/Echohaven KayKit now matched and exceeded for second zone with dynamic life. 

Next visual wave could target ley-line visual connections or Moon 3 scaffolding.

---

## Phase 3 Round 4 — Agent 04: Bosses & Advanced Enemies - Puzzle Integration & Visual Polish (2026-05-20)

**Exclusive domain**: Bosses & Enemies ONLY (BossEncounterSystem, CombatBridge hooks, MudGolemAI-adjacent, HUD/loop wiring for boss combat). All work strictly in `C:\dev\TARTARIA_new`. Absolute paths. Built directly on:
- Mud Colossus as post-fountain climax
- RailWraith + SludgeLeviathan 3-phase behaviors (VoidArchitect phases + new procedural proxies)
- Deepened frequency puzzle (vulnerability windows already existed; now real submission)

**Delivered (real combat integration + polish):**
- **Frequency puzzle wired into real combat via HarmonicStrike/ResonancePulse hooks** (C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\BossEncounterSystem.cs:176-209 SubmitFrequencyPuzzle; CombatBridge.cs:163-170 & 188-195 call sites during Fire* using 432f tuned harmonic as submission vs CurrentTargetFrequency).
- **Dynamic health sync for Mud Colossus visuals**: procedural proxy (capsule) scales + color/emission lerp on normalized HP; VFX pulses on damage (BossEncounterSystem.cs:406-420 SyncMudColossusVisuals + 187-191 call in DealDamage).
- **Procedural visuals / DOTS-feel for RailWraith & SludgeLeviathan**: 3-phase color + bob/scale/pulse proxies spawned on vuln/phase (BossEncounterSystem.cs:248-268 SpawnOrUpdate..., 300-340 phase updaters, 343-360 UpdateBossVisuals, per-frame motion).
- **In-boss HUD for CurrentTargetFrequency**: live display + pulse during vuln windows (HUDController.cs:112-113 + 380-420 new methods + GameLoopController.cs:878-882, 2770-2775, 3158 wiring + health change polling).
- **Graceful fail/abort handling**: FailBoss(reason) + auto timeout (>par*2.8) / hit-cap (>14) + cleanup proxies + improved messaging + state return (BossEncounterSystem.cs:172-185 FailBoss + 366-380 Update checks; GameLoopController.cs:3160-3168 HandleBossFailed).
- **Mud Colossus post-fountain climax wiring**: OnBuildingRestored special-cases "fountain"/"harmonic" → delayed SpawnBoss("mud_colossus") + HUD objective (GameLoopController.cs:1125-1150 + coroutine).
- **Additional**: CurrentTargetFrequency + IsFrequencyPuzzleActive public getters; boss-specific target ranges (Mud 174, Rail 210, Sludge 155); dynamic nudge on strong puzzle hits; visual proxy cleanup on all exit paths; 386 net LOC.

**Files edited (boss/enemy domain + tight supporting)**:
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\BossEncounterSystem.cs`: +~340 LOC (core puzzle, visuals, sync, graceful, state, UpdateVuln/Deal/phase).
  - Key anchors: fields 57-65; getters 71-72; StartBoss reset 156-160; Submit 176-209; Fail/Cleanup 172-185; SpawnOrUpdate 248-268; SyncColossus 300-320; Rail/Sludge phase 322-340; UpdateVisuals 343-360; vuln set target+puzzle+visuals 533-563; health sync 406-410; phase refresh 596-598; defeat cleanup 641-642; graceful timer 366-380.
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\CombatBridge.cs`: +12 LOC (ResonancePulse 163-170 hook; HarmonicStrike 188-195 hook).
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\Integration\GameLoopController.cs`: +40 LOC (OnBuildingRestored post-fountain 1125-1150; Handle* boss freq HUD 878-882/2770-2775; graceful fail 3160-3168; health poll).
- `C:\dev\TARTARIA_new\Assets\_Project\Scripts\UI\HUDController.cs`: +34 LOC (fields 112-113; Show/Update/Hide/ pulse 380-420; Update call 155).
- `C:\dev\TARTARIA_new\CONTEXT.md`: this note.

**How to verify**:
- Open Echohaven scene → restore Harmonic Fountain → Mud Colossus spawns as post-climax boss with procedural visual + freq HUD + vuln puzzle (use F or Resonance near it during "target ~XXX Hz" windows for real damage via hooks).
- Trigger rail_leviathan / sludge_leviathan via ContinentalRailSystem / AquiferPurge → see 3-phase procedural visuals + freq puzzle.
- Let boss run long or take many hits → graceful FailBoss auto-abort with cleanup.
- Git: `cd C:\dev\TARTARIA_new && git diff -- "Assets/_Project/Scripts/Integration/BossEncounterSystem.cs" ...`

**Remaining gaps (boss/enemy lane)**:
- Pull real player CurrentFrequency from HarmonicCombatant in CombatBridge for true variable-freq puzzle submissions (instead of hardcoded 432f).
- Full DOTS rendering for proxies (current GameObject primitives for zero-asset compatibility).
- Boss-specific AI behaviors beyond abstract (MudGolemAI extensions for Colossus?).
- Save persistence for in-progress boss state + CurrentTargetFrequency.
- Real 3D animated models for Rail/Sludge proxies (KayKit or future).
- Frequency telegraph VFX (rings pulsing at exact target Hz) during vuln.

This delivers Phase 3 Round 4 "Puzzle Integration & Visual Polish" for bosses, making the Harmonic Fountain → Mud Colossus the memorable combat climax, with deepened frequency fantasy wired directly to player attacks. All runtime, no new assets, follows prior patterns.

Next round ideas (Phase 3 R5): Ley line visual connections lighting on restores (Aether/LeyLine lane), full Mud Colossus 3-phase behaviors + special attacks, player frequency tuning input during boss vuln (slider HUD), Moon 3 boss polish (RailWraith escort integration), save state for active boss freq windows.
