# TARTARIA — ROADMAP

> Last updated: 2026-06-04 LATEST HAMMER — post-session refresh. Supersedes the 2026-06-04 LATE AUDIT + FIX SWEEP roadmap below; preserves prior waves as historical context.
> Working under the 2026-06-03 NIGHT MANDATE in `CLAUDE.md`: **no halfway files, no patching, no playtest until all gaps closed.**

---

## 2026-06-04 LATEST HAMMER — post-session priorities

Disk-side lock-down pushed from "high" to "very-high" (~98%) this session. NPC armature pipeline shipped through Stages A + B + D (19→23-bone Humanoid skeleton with strict T-pose rest + accessory weight overrides; Animator wiring one-shot ready). Player.prefab now has a visible mesh (Cassian nested under `_CharacterVisual`). HUD_Root.prefab on disk. MudGolem prefab fully rewired. Music §16.11 4-layer verified. REORG-4 reverted 11-of-12 moves due to circular asmdef dep — architectural lesson captured in CLAUDE.md "Patterns to avoid" note. Char_Knight confirmed as vendor KayKit LFS-pointer casualty; mitigated via Cassian-as-Player nest. Compile clean post-revert. See `STATUS.md` 2026-06-04 LATEST HAMMER section + `docs/MOON1_GAP_REPORT_2026-06-04.md` "Closed POST-LATEST-HAMMER" subsection.

**Frame (POST-LATEST-HAMMER 2026-06-04):** Moon 1 BUILD PHASE locked at "very-high" on disk (~98%), runtime artifacts (b) still pending, **NOT GATE 1 done**. Per HONEST RESET rule in `CLAUDE.md`, every closure is a disk-side win — never "Moon 1 ship-complete". No release / itch / Win64 / Steam framing per 2026-06-03 NATRIX MANDATE.

### Next-session priorities (in this order)

1. **§16 runtime artifact capture** (the (b) half of the HONEST RESET gate — the only remaining blocker to GATE 1):
   - §16.1 — record 15-min uncut play-through video, check into `docs/audits/`.
   - §16.2 — Profiler at 1080p mid-spec, confirm 60 FPS sustained.
   - §16.3 — Profiler at 1080p low-spec, confirm 30 FPS sustained.
   - §16.4 — RAM ceiling check.
   - §16.12 — 30-min soak test, no leaks / NRE accumulation.
2. **NPC armature Stage C** — animation keyframes binding to the 23-bone skeleton (Stage A + B authored skeleton + rest pose this session; clips not yet bound). Without Stage C, NPCs idle in T-pose-rest even after Avatars resolve.
3. **REORG-4 retry — dependency-first migration plan**:
   - Map every Integration-scope dependency for each candidate file (companion controllers, UI panels, MudGolemEnemy, combat scripts).
   - EITHER move dependencies in lockstep with consumers, OR introduce a new asmdef below Integration in the dep graph for the AI-pure subset, OR add a type-forwarding shim in Integration that re-exports the AI-side types.
   - Transparent-namespace move (the path that failed this session) is permanently quarantined.
4. **Char_Knight vendor LFS pull** — run `git lfs fetch --all` on the Windows host to materialise the 3 KayKit Knight LFS payloads (vendor fbx + glb + `_Project/Models/Characters/KayKit/` copy). If that fails or vendor reset is preferred, author `tools/blender/gen_player_hero.py` for a dedicated hero through the established Blender pipeline. Either path retires the Cassian twin-spawn caveat.
5. **5 deferred combat asmdef moves** — re-plan with dependency-first method from step 3.

When 1 lands with artifacts on disk, Moon 1 is honestly GATE 1 complete. Then `docs/16_MOON2_BUILD_SPEC.md` authoring + Moon 2 BUILD start.

---

## 2026-06-04 LATE AUDIT + FIX SWEEP — post-session priorities (HISTORICAL — superseded by LATEST HAMMER above)

The disk-side lock-down was further extended in this session. Render pipeline verified clean (108 textures + 195 materials, zero magenta, URP/Lit, Linear color, 14/14 Tartaria custom shaders compile). Scene/prefab YAML cleanup landed (legacy Directional Light disabled, PostProcessVolume wired, 5 village localScale resets, MudGolem tagging). Blender re-bakes shipped (ResetScout was a 130-byte LFS pointer; now real binary). 351 FBXs total ZERO LFS pointer stubs remaining. Script renames cleared the quarantine grep tripwire. 63 untracked prefab duplicates purged. See `STATUS.md` 2026-06-04 LATE AUDIT + FIX SWEEP SESSION + `docs/MOON1_GAP_REPORT_2026-06-04.md` "Closed POST-AUDIT-SWEEP" subsection.

**Frame (POST-AUDIT-SWEEP 2026-06-04):** Moon 1 BUILD PHASE locked further on disk (call it "high"), runtime artifacts (b) still pending, **NOT GATE 1 done**. Per HONEST RESET rule in `CLAUDE.md`, every closure is a disk-side win — never "Moon 1 ship-complete". No release / itch / Win64 / Steam framing per 2026-06-03 NATRIX MANDATE.

### Next-session priorities (in this order)

1. **NPC armature pipeline — Stages B–D** — Stage A is in parallel HAMMER LANE 1 this session. Stages B–D need armature-rigged NPC FBX so the wired `Animator` components actually deform joints. Static joined meshes are sentinel-only placeholder.
2. **Player Char_Knight skin nest** — `Char_Knight.prefab` has no renderers (KayKit import didn't extract mesh). Need to nest the KayKit mesh into Player.prefab's `_CharacterVisual` child OR re-import the KayKit FBX with proper mesh extraction.
3. **Player.prefab visibility one-shot** — fires on next Editor launch; verify visibility live.
4. **17 cross-asmdef script moves** — if HAMMER LANE 2 defers them this session, pick up in next session.
5. **§16 runtime artifact capture** (the (b) half of the HONEST RESET gate):
   - §16.1 — record 15-min uncut play-through video, check into `docs/audits/`.
   - §16.2 — Profiler at 1080p mid-spec, confirm 60 FPS sustained.
   - §16.3 — Profiler at 1080p low-spec, confirm 30 FPS sustained.
   - §16.4 — RAM ceiling check.
   - §16.12 — 30-min soak test, no leaks / NRE accumulation.
6. **Music layer 1/2 length match** (non-blocking follow-up) — re-author layer1/2 to 60s so all 4 stems have matched loop lengths.

When 5 lands with artifacts on disk, Moon 1 is honestly GATE 1 complete. Then `docs/16_MOON2_BUILD_SPEC.md` authoring + Moon 2 BUILD start.

---

## 2026-06-04 LATE LOCK-DOWN — post-session priorities (HISTORICAL — superseded by AUDIT + FIX SWEEP above)

The disk-side bulk of the 2026-06-04 wave plan is closed. NPCs, MudGolem FBX, village undershoots, NPC name collision, dead code, AnastasiaRocker sub-path, hero buildings (myth-busted), `run_all_moon1.py` are all done on disk. See `STATUS.md` 2026-06-04 LATE LOCK-DOWN SESSION + `docs/MOON1_GAP_REPORT_2026-06-04.md` CLOSE-OUT LOG.

**Frame (POST-AUTO 2026-06-04):** MOON 1 READY FOR §16 RUNTIME ARTIFACT CAPTURE. Disk-side 100% locked. NATRIX running the 10-min playtest now — if clean, GATE 1 closes when the recording is captured. Until then, NOT "Moon 1 GATE 1 done."

### Next-session priorities (Unity-driven by NATRIX, in this order)

1. ✅ **DONE 2026-06-04** — **Unity MCP reconnect** — MCP reached Editor; live state probe confirmed all AUTO-1 through AUTO-5 closures.
2. ✅ **DONE 2026-06-04** — **Fire documented one-shot snippets** — MudGolem rewire fired via `Tartaria/8 Fix/Run MudGolem Rewire NOW`. Probe: FBX bounds `(1.75, 2.62, 1.00)` real 2.5m mesh.
3. ✅ **DONE 2026-06-04** — **Bake HUD_Root.prefab** — EditorPref `Tartaria.OneShot.HUDRootBake.2026-06-04` = True. Skeleton at `Resources/Prefabs/UI/HUD_Root.prefab`. `RuntimeHUDBuilder.cs:83` prefab-first path resolves.
4. ✅ **DONE 2026-06-04** — **Fill missing music stems** — `Resources.Load<AudioClip>("Audio/Music/ambient_layer3")` = 60s, layer4 = 60s. AdaptiveMusicController 4-layer resolves all 4 stems. *Caveat: layer1/2 still 10s placeholders — non-blocking follow-up.*
5. ✅ **DONE 2026-06-04** — **Clean BobsInn scene-scale hack** — `Echohaven_VerticalSlice.unity:1529/1533/1537` `m_LocalScale` reset 0.083 → 1; probe confirms `BobsInn scale: (1.00, 1.00, 1.00)`.
6. **§16 runtime artifact capture** (NATRIX in progress NOW):
   - §16.1 — record 15-min uncut play-through video, check into `docs/audits/`.
   - §16.2 — Profiler at 1080p mid-spec, confirm 60 FPS sustained.
   - §16.3 — Profiler at 1080p low-spec, confirm 30 FPS sustained.
   - §16.4 — RAM ceiling check.
   - §16.12 — 30-min soak test, no leaks / NRE accumulation.
7. **NPC armature pipeline upgrade** (future Blender sprint, after Moon 1 lock-down) — author armature-rigged NPC FBX so the wired Animators actually deform joints. Static joined meshes are sentinel-only placeholder.

When 6 lands with artifacts on disk, Moon 1 is honestly GATE 1 complete. Then `docs/16_MOON2_BUILD_SPEC.md` authoring + Moon 2 BUILD start.

---

## 2026-06-04 — MOON 1 FINISH PLAN (wave-based — historical, mostly closed)

The 2026-06-03 NIGHT GATE 1 COMPLETE claim is retracted. A 3-agent audit found 30+ gaps documented in `docs/MOON1_GAP_REPORT_2026-06-04.md`. Below is the closeout plan in strict wave order. **Each wave finishes fully on disk before the next starts.** No partial verification, no playtests in between. **STATUS: most waves closed in 2026-06-04 LATE LOCK-DOWN — see CLOSE-OUT LOG.**

### Wave 1 — Render + Resources.Load floor (P0, ~3 hrs)

- **R1** Flip `ProjectSettings/ProjectSettings.asset:50` `m_ActiveColorSpace` from `0` (Gamma) → `1` (Linear).
- **RL1–RL15** Fix every `Resources.Load` path that returns null:
  - Move/relocate the missing prefabs/materials/effects under `Resources/` subfolders, OR
  - Correct the path strings to point at where the assets actually live, OR
  - Replace the `Resources.Load` calls with direct prefab refs and serialize them.
- **P2** Wire URP/Lit materials onto all 8 children of `AnastasiaRocker.prefab`; re-bake.

### Wave 2 — NPC animation + colliders (P1, ~3 hrs)

- **N1–N4** Author 4 humanoid Animator controllers for Milo / Anastasia / Lirael / Cassian, OR re-parent each NPC as a **Prefab Variant of `Char_Knight.prefab`** with mesh swap (faster, reuses the existing working controller).
- **N5** Rescale all 4 NPC `CapsuleCollider` to Height=2.0, Center=(0, 1, 0). Verify against scene Y at runtime.

### Wave 3 — Hero buildings (P0, ~3 hrs)

- **P3–P5** Replace the empty stubs `Echohaven_CrystalSpire.prefab` / `Echohaven_StarDome.prefab` / `Echohaven_HarmonicFountain.prefab` (all ~33-line containers) with real kit compositions. Use `Echohaven_StarDome_Built.prefab` (3041 lines, 118 PrefabInstances) as the pattern.
- **P6** Create the missing `Echohaven_Cathedral.prefab` from the Cathedral kit.

### Wave 4 — Enemies + audio (P1, ~5 hrs)

- **A3 + P1** Author `Models/Blender/Moon1/MudGolem.fbx` via `tools/blender/gen_mud_golem.py` (new script). Rewire `MudGolem.prefab` off the Unity built-in spheres.
- **A4** Move `Models/Blender/Shared/ResetScout.fbx` → `Models/Blender/Moon1/ResetScout.fbx`. Update any path consumers.
- **A1** Author `Resources/Audio/` tree: ambient zone hum, tuning SFX (3 variants × success/fail), restoration stinger orchestra, 17th-hour cinematic music, skeleton hum, prophecy fragment cue. Drop AudioClips at the paths the `Resources.Load<AudioClip>` consumers expect.

### Wave 5 — Village scale-bake redo (P1, ~1 hr)

- **V1–V5** Redo TownHall / VillageMill / Watchtower / VillageCottage_A/B/C / Apothecary scales with **uniform scaling** derived from the actual FBX bounds. Vary the cottages so A/B/C aren't identical.

### Wave 6 — HUD prefab bake (P2, ~1 hr)

- **A2** Run `RuntimeHUDBuilder` once in Editor to assemble the 64-GameObject hierarchy. Save as `Resources/Prefabs/UI/HUD_Root.prefab`. Replace the runtime construction with `Resources.Load<GameObject>("Prefabs/UI/HUD_Root")` + `Instantiate`.

### Wave 7 — Doc reconciliation + code hygiene (P2, ~3 hrs, can parallel Waves 5–6)

- **D1** Update `docs/02_AETHER_ENERGY_SYSTEM.md` to canonical Telluric 7.83 / Harmonic 432 / Celestial 528 (drop 3/6/9-Band terminology).
- **D2** `docs/15_MVP_BUILD_SPEC.md §13 line 697` — change 1296 Hz row to 528 Hz.
- **D3** `docs/15_MVP_BUILD_SPEC.md §1 line 62` — move Lirael, Anastasia, Cassian from "Phase 2+ NOT in" to Moon 1 roster.
- **D4** Quarantine `KNOWN_ISSUES.md` (move to `docs/archive/superseded_status/KNOWN_ISSUES_2026-05-21.md` or rewrite under the 2026-06-03 mandate).
- **C1** Implement the 9 `GameEvents.Fire*` stub methods (CollectibleGathered, AchievementUnlocked, CompanionTrustChanged, LeverPulled, MoonProgressUpdate, PlayerEnteredZone, TutorialStep, TuningNodeActivated) — invoke the matching `Action` events, drop the `Debug.Log` shims.
- **C2–C3** Delete `PauseMenu.cs` + `PauseOverlay.cs` no-op stubs.
- **C4–C7** Clear the 6 `// TODO` markers in `CompanionBehaviorSystem.cs:12-13`, `EnemyAIController.cs:216`, `DayNightController.cs:123`, `TartarianCalendar.cs:47/76/128/130`.
- **E1–E4** Canonicalize `OnSeventeenthHour`: keep `TartarianHourCycle.cs:37` (the firer), delete the declarations in `GameEvents.cs:447` and `TartarianCalendar.cs:44`, rewrite `RailEscortController.OnSeventeenthHourTriggered` to subscribe.

### Wave 8 — Moon 2 build spec authoring (P1, ~3 hrs)

- **D5** Author `docs/16_MOON2_BUILD_SPEC.md` from `docs/03_CAMPAIGN_13_MOONS.md` Moon 2 section + spec template borrowed from `docs/15`. Cover:
  - Crystalline Caverns zone layout + scene mapping
  - Dissonance crystals enemy
  - Micro-giant mode mechanic
  - Cassian-betrayal trust system
  - Moon 2 mini-game variants
  - Moon-number → scene mapping doc (cross-reference for all 13 Moons)

### Wave 9 — §16 runtime verification (after ALL of Waves 1–7, ~1 hr)

- **§16.1** Record 15-minute uncut play-through video; check into `docs/release/` (or `docs/audits/` since release framing is dormant).
- **§16.2** Run Profiler at 1080p mid-spec — confirm 60 FPS sustained.
- **§16.3** Run Profiler at 1080p low-spec — confirm 30 FPS sustained.
- **§16.4** RAM ceiling check via Profiler memory module.
- **§16.12** 30-minute soak test — no leaks, no NRE accumulation.

### Then — and only then — Moon 2

When every row in `docs/MOON1_GAP_REPORT_2026-06-04.md` is closed AND Wave 9 artifacts exist on disk, Moon 1 is honestly done. Start `docs/16_MOON2_BUILD_SPEC.md` driven content build:

- Crystalline Caverns scene composition
- Cassian companion controller + Animator (reuse Wave 2 pattern)
- Dissonance Crystals enemy (Blender mesh + AI controller, reuse Wave 4 pattern)
- Micro-giant mode (60-second 6-inches-tall burst, opposite of Moon 1 giant)
- Fountain cleansing first-restoration
- First Mud Golem encounter (full reuse from Moon 1)
- Moon 2 audio matrix
- Moon 2 quest tree + Yarn dialogue

---

## The honest baseline (2026-05-30, preserved)

Previous versions of this file declared Moons 1–13 "complete" based on file existence + agent self-reports. A real audit (see `STATUS.md`) found:

- ~310 of 343 `MoonN*.cs` files are template stubs that `GameObject.CreatePrimitive(Sphere)` and `Debug.Log` an emoji.
- The 11 active `Moon1*.cs` Integration scripts (Lighting, PostProcessing, AmbientCreatures, QuestTriggers, ExcavationSites, LevelBuilder, HeroBuildingSpawner, NPCSpawner, PlayerSetup, MaterialSetup, BuildingPrefabCreator) have **ZERO scene references** — written but never instantiated.
- The `Echohaven_VerticalSlice.unity` scene has a shell (3 hero buildings + props + NPCs + atmosphere) but is missing the Moon-1-specific gameplay called for in `docs/03_CAMPAIGN_13_MOONS.md` and `docs/15_MVP_BUILD_SPEC.md`.

So the real roadmap below is what we have to *build*, not what's been "shipped".

---

## Track: Moon 1 — Magnetic / Echohaven

**Status:** ~40% — shell + props + NPCs + atmosphere done; canonical gameplay mostly absent.

### Done
- Player movement (left stick + WASD), camera follow, HUD live, audio listener clean, URP material colors clean.
- 3 hero buildings buried at correct depths (Spire 60% / Dome 80% / Fountain 95%) with `InteractableBuilding` + 3-node tuning + restoration VFX + 5s raise animation.
- 9 village structures from cathedral kit (decorative).
- 6 POIs (Mud Pools / Carved Stone / Overlook / Root Chamber).
- 120 vegetation, 69 props, 4 NPC placements.
- Golden-hour ambient + fog.
- 3 Yarn dialogue files written (not yet hooked to runner).
- 3 tuning mini-game variants (generic — see below).

### Building (in order)
1. **Master bootstrap:** wire the 11 dormant `Moon1*.cs` systems via `Tartaria → MASTER: Bootstrap All Moon 1 Systems`.
2. **Pipe organ centerpiece** inside the Cathedral — 3-note tuning puzzle (the canonical Moon 1 first restoration per docs/03 Days 6–12). Replaces or augments the generic slider as the Moon-1-specific puzzle.
3. **Reset Scout enemy** — Victorian-costumed enemies with clipboards/jackhammers, distinct from Mud Golem.
4. **Giant Mode** — 60-second 15-feet-tall burst (toss enemies, smash mud piles).
5. **Rose window cymatic projection** on the floor after dome restoration.
6. **Pure water font** — particle + audio trickle-back when fountain restored.
7. **Spire placement ceremony** with blue-white sparks climbing at night.
8. **Ley line mini-map** lighting up after first restoration.
9. **17th-hour alignment** mechanic for cathedral light eruption.
10. **Lirael 432 Hz lullaby** audio + animated appearance.
11. **Skeleton hum first-prophecy fragment** (figure on star fort).
12. **Giant skeleton key #1** of 8 collectible.
13. **Dialogue runner** — hook the 3 Yarn files to in-game triggers.

### Moon 1 done = ready to start Moon 2.

---

## Track: Moons 2 – 13

Per `docs/03_CAMPAIGN_13_MOONS.md`. Build order is sequential — start each only after the prior Moon is fully delivered. Stub files exist for all of them but most are template-spawned and need to be replaced or upgraded.

| Moon | Theme | Companion | One-line scope |
|---|---|---|---|
| 2 | Lunar — Crystalline Caverns | Cassian | Micro-giant mode, dissonance crystals, fountain cleansing, first Mud Golem |
| 3 | Electric — Orphan Train | Lirael | Resonance trains, junior architects (orphans), Lullaby Crystal |
| 4 | Self-Existing — Settlement | Junior architects | Autonomous building, ley-line nodes |
| 5 | Overtone — White City | Thorne | World's Fair holograms, Spire-fragment bloom, airship dock |
| 6 | Rhythmic — Living Library | Milo | Pipe organ requiem, Milo's awakening |
| 7 | Resonant — Resonant Spire | Korath | Aether beacon tower, Korath sacrifice |
| 8 | Galactic — Airship Armada | Thorne | Aerial combat, fleet assembly |
| 9 | Solar — Sun-Mirror Array | (mixed) | Mirror-array puzzle, solar Aether band |
| 10 | Planetary — Continental Trains | (children) | Train logistics across the continent |
| 11 | Spectral — Fountain Network | Lirael | Planetary cleansing |
| 12 | Crystal — Planetary Bell Sync | Korath echo | Planetary scalar wave |
| 13 | Cosmic — Convergence | All | Finale, full fleet, all bands |

Each Moon's per-day breakdown lives in `docs/03_CAMPAIGN_13_MOONS.md`.

---

## Track: Cross-cutting systems (touched by every Moon)

These get fleshed out as the Moons that need them come online. Don't try to perfect them up-front.

- **Aether Field** — 3-band visualization, GPU compute shader, flow sim. Foundation lives in `Tartaria.Core`.
- **Resonance Score (RS)** — DOTS ECS system, golden-ratio validator, threshold events at 25/50/75/100.
- **Tuning Mini-Game** — 3 variants done at the generic level; per-Moon special variants (pipe organ, bell tower, scalar mirror, train rails) layer on top.
- **Companion AI** — Milo follow + introduce wired. Trust arcs, voice, banter to add per Moon.
- **Combat** — 3 player abilities (Resonance Pulse / Harmonic Strike / Frequency Shield) + Mud Golem AI exist. Enemy roster grows per Moon (Reset Scouts in M1, Dissonance Crystals in M2, etc.).
- **Save / Load** — AES-256 encrypted JSON, schema v18. Already covers Moon flags + companion trust.
- **Day / Night** — 17-hour Tartarian day mapped to ~17 minutes real-time. Visual cycle only at first; 17th-hour alignment is gameplay-critical from Moon 1 climax onward.
- **HUD** — RS counter live, health bar, Aether meter, banners, interaction prompts all working. Mini-map needs to come online for Moon 1 ley-line reveal.
- **Audio** — AudioManager singleton ready. ~50 SFX keys referenced but not all wired to clips. Drake Stafford 432 Hz ambient is the only confirmed music track.
- **VFX** — RestoreSparkle, ScanPulse, Aurora prefabs exist. Mud dissolve shader needs runtime wiring for emergence animation.

---

## What we are NOT doing

Per the 2026-05-30 mandate:

- ❌ No itch.io release planning.
- ❌ No Steam page work.
- ❌ No demo / vertical-slice ship gates.
- ❌ No marketing copy.
- ❌ No "Track A vs Track B" branching of the work.

We build the game. We talk about distribution after it plays end-to-end.

---

## Where to look for the truth

- `STATUS.md` — current week-by-week state.
- `CLAUDE.md` — instructions for future Claude sessions + the build-order mandate.
- `docs/03_CAMPAIGN_13_MOONS.md` — Moon-by-Moon narrative + mechanics spec.
- `docs/15_MVP_BUILD_SPEC.md` — system-level depth spec (treat as minimum for Moon 1, not maximum).
- `docs/agent_reports/` — historical noise. Do not trust status claims from there.
- This file (`ROADMAP.md`) — what's left, in order.

*ROADMAP v2.0 · 2026-05-30 · Update when a Moon track ships.*
