# TARTARIA — ROADMAP

> Last updated: 2026-05-30. This doc supersedes the older session-1-through-4 roadmap (preserved at `docs/archive/ROADMAP_old.md`).
> Working under the 2026-05-30 mandate in `CLAUDE.md`: **build all 13 Moons fully before any release discussion.**

---

## The honest baseline

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
