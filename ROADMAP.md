# TARTARIA — ROADMAP

> Long-arc plan: Moon 1 → Moon 13 → Polish → Day Out of Time → Ship. Updated each Moon completion. Historical entries archived to `docs/_archive_pre_2026_06_05/ROADMAP_v_pre_06_05.md`.

**Last updated:** 2026-06-05 (foundation reset)

---

## 1. PHASE OVERVIEW

| Phase | Scope | Status |
|---|---|---|
| **Phase 1 — Moon 1 (Echohaven)** | Vertical slice + GATE 1 criteria. Establishes all systems. | 🟡 disk-side 22/22 · runtime 0/8 |
| Phase 2 — Moon 2 (Dissonance Wastes) | Combat depth + first boss + Echo Beasts | ⏸ blocked on Phase 1 |
| Phase 3 — Moon 3 (Voltaic Halls) | Electricity puzzles + Tesla shrines | ⏸ |
| Phase 4 — Moon 4 (Cathedral Spires) | Vertical traversal + stained-glass cymatics | ⏸ |
| Phase 5 — Moon 5 (Sunken Atrium) | Water mechanics + Addressables migration | ⏸ |
| Phase 6 — Moon 6 (Crystal Reservoir) | Refraction puzzles + chord harmonics | ⏸ |
| Phase 7 — Moon 7 (Mire of Whispers) | Stealth + Reset Scout patrol depth | ⏸ |
| Phase 8 — Moon 8 (Aetheric Forge) | Crafting depth + reactor cooling | ⏸ |
| Phase 9 — Moon 9 (Skybridge Network) | Wind mechanics + glider system | ⏸ |
| Phase 10 — Moon 10 (Sunken Causeway) | Bridge restoration + tide mechanics | ⏸ |
| Phase 11 — Moon 11 (Reset Sanctum) | Reset Cabal boss + ideological choice | ⏸ |
| Phase 12 — Moon 12 (Aether Lattice) | Skybox traversal + giant resonance | ⏸ |
| Phase 13 — Moon 13 (Day Out of Time) | Final boss + ending branches | ⏸ |
| **Phase 14 — Polish + Ship** | Sound mix, accessibility, ending cinematics, release framing | ⏸ all 13 must be GATE-clean |

---

## 2. PER-MOON GATE CRITERIA (universal)

Each Moon must pass the same two-part gate before its phase closes:

### Part A — 8-step smoke test (`CLAUDE.md §2`)

1. Click Play → 0 errors
2. Player visible at spawn (no magenta / T-pose / clip)
3. Movement (WASD / F310 left-stick)
4. Camera follows
5. Reach the Moon-canonical interactable
6. Press E / A — interaction UI appears
7. Complete interaction — state changes, VFX/audio fires
8. HUD updates (Moon-specific tracker)

### Part B — runtime artifacts (`docs/15_MVP_BUILD_SPEC.md §16`)

1. **§16.1** — 15-minute uncut play-through video, checked into `docs/audits/MoonN_playthrough_DATE.mp4`
2. **§16.2** — Profiler capture at 1080p mid-spec, 60 FPS sustained
3. **§16.3** — Profiler capture at 1080p low-spec, 30 FPS sustained
4. **§16.4** — RAM ceiling check (≤4 GB after 30 min)
5. **§16.12** — 30-minute soak test, no NRE accumulation, no leak

Both parts pass → Moon is GATE-clean. STATUS.md updates. Next Moon begins.

---

## 3. MOON 1 — current phase

### Scope (from `docs/15_MVP_BUILD_SPEC.md`)

**Echohaven Village restoration.** Player = Cassian (Carter). Companions = Milo + Lirael (Day 7).

- 3 hero buildings (Cathedral, StarDome, CrystalSpire) buried at spec depths
- 9 village buildings (Apothecary, Bakery, Cottages A/B/C, Inn, Mill, Smithy, TownHall, Watchtower)
- 4 NPCs (Milo, Anastasia, Lirael, Cassian) + Bob Innkeeper
- 4 POIs (Mud Pools, Carved Stone, Overlook, Root Chamber)
- 3 tuning mini-game variants (A: Frequency, B: Waveform, C: Harmonic)
- 28-day cycle, 17th-hour cinematic
- Combat: Mud Golem waves + Reset Scout patrols
- Restoration loop: scan → excavate → tune → restore
- Save/load round-trip via F5/F9

### Current state

- ✅ Disk-side: 22/22 ship-verify rows green (`MOON1_SHIP_VERIFY.txt`)
- ❌ Runtime: 0/8 smoke test rows confirmed (`STATUS.md §1`)
- ⏳ Artifacts: §16.1-4 + §16.12 not yet captured

### Path to close

| # | Action | Owner |
|---|---|---|
| 1 | Verify step 2 — Cassian renders post `skinWeights = Standard` fix | NATRIX drives Play |
| 2 | Walk steps 3-8, single-fix-per-failure | Claude on probe + edit |
| 3 | Capture §16.1 (15-min video) | NATRIX records |
| 4 | Capture §16.2-4 (profiler captures) | NATRIX runs Unity Profiler |
| 5 | Capture §16.12 (30-min soak) | Unattended Play session |
| 6 | Commit artifacts to `docs/audits/` | Claude |
| 7 | Update STATUS.md → "Moon 1 GATE 1 done" | Claude |

---

## 4. MOON 2-13 — derivation pattern

The Moon 2-13 build pipeline copies the Moon 1 blueprint (`docs/MOON_BLUEPRINT.md`). For each Moon:

1. Author `docs/16+N_MOONN_BUILD_SPEC.md` from the blueprint (15 sections + §16 GATE)
2. Author `tools/blender/moonN/gen_*.py` for new asset families
3. Build `Assets/_Project/Scenes/Moons/MoonN.unity` scene composition
4. Wire `Moon1_Systems` equivalent (`MoonN_Systems` GameObject in scene)
5. Bake NavMesh + lighting
6. Run 8-step smoke test → fix single failures
7. Capture §16 artifacts
8. Update STATUS.md, ROADMAP.md → next Moon

Cross-Moon shared work (one-time, done during Moon 1 or as separate workstreams):

- **Save system** — `IPersistable` interface, JSON snapshots. Done (Moon 1 ship-verify A1.x).
- **Input system** — TartariaInputActions + F310 + WASD. Done.
- **Audio architecture** — `AdaptiveMusicController` 4-layer mix. Done.
- **HUD framework** — `HUD_Root.prefab` with quest tracker, RS counter, day cycle. Done.
- **NPC armature pipeline** — Stage A (skeleton) + B (T-pose + accessories) done. Stage C (animation clips) pending Moon 1.
- **Combat framework** — `EnemyHealth` + `LootDrop` + AI states. Done for MudGolem; clone for each Moon's enemy.
- **Cinematic framework** — `CinematicMoments` system. Done.
- **Addressables migration** — schedule for Moon 5 (per `CLAUDE.md §4`).

---

## 5. POST-MOON-13 (Phase 14)

When all 13 Moons are GATE-clean:

1. **Final polish** — sound mix, light bake quality, animation transitions.
2. **Accessibility pass** — subtitles, colorblind modes, remappable controls (per `docs/24_ACCESSIBILITY.md`).
3. **Ending cinematics** — 3 ending branches per `docs/03_CAMPAIGN_13_MOONS.md`.
4. **Day Out of Time content** — secret 14th day epilogue.
5. **Localization** — at least EN + JA + DE (per `docs/27_LOCALIZATION.md`).
6. **Release framing** — pricing, store page, build pipeline. This is when itch.io / Win64 / Steam discussion is unlocked.

Until Phase 14, no release framing exists in any doc.

---

## 6. ARCHITECTURAL DECISIONS PER-MOON

| Decision | When | Notes |
|---|---|---|
| Cinemachine 3 migration vs keep custom CameraController | Moon 2 design phase | Per Unity 6 manual, Cinemachine is canonical. If `CameraController.cs` passes Moon 1 GATE clean, keep it for Moon 1; migrate to CinemachineCamera in Moon 2. |
| Addressables migration | Moon 5 design phase | `Resources.Load` works for ≤4 Moons. Above that, Addressables avoids the load-time stall. Unity manual: "Use Addressables when content count exceeds Resources practical limit." |
| Save migration to Steam Cloud | Phase 14 | Not before. Local `Application.persistentDataPath` until then. |
| Localization tables | Moon 5 design phase | Unity Localization package install + scrape strings. Before Moon 5 = English-only is fine. |

---

## 7. RULES (DO NOT CHANGE WITHOUT NATRIX APPROVAL)

- **Build order is fixed.** Moon 1 → 2 → 3 → ... → 13 → Phase 14. No skipping. No "let's do Moon 5 in parallel."
- **No release talk pre-Phase 14.** The 9 quarantined files in `CLAUDE.md` historical context stay dormant.
- **Disk-side audits are necessary but not sufficient.** Every Moon's GATE requires both Part A (smoke test) and Part B (runtime artifacts).
- **The 8-step smoke test is the verification.** Not a 22-row disk audit. Not file presence. Behavior.

---

*ROADMAP.md v2.0 · 2026-06-05*
