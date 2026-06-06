# MOON BLUEPRINT — universal template for Moons 1-13

> The single template each Moon spec follows. When Moon 1 is GATE-clean, this blueprint captures the load-bearing structure so Moons 2-13 can be authored quickly without re-deriving conventions every time.
>
> Use this as the table-of-contents for every `docs/16+N_MOONN_BUILD_SPEC.md`.

---

## §1. MOON IDENTITY

- **Number:** N (1-13)
- **Name:** [Echohaven / Dissonance Wastes / Voltaic Halls / ...]
- **Element / Theme:** [Magnetic / Resonant / Voltaic / ...]
- **Day range:** [Days 1-28 = Moon 1, Days 29-56 = Moon 2, ...]
- **Aether band focus:** Telluric (7.83 Hz) / Harmonic (432 Hz) / Celestial (528 Hz)
- **Companion(s) unlocked this Moon:** [Milo + Lirael for M1; Anastasia M2; etc per `docs/03_CAMPAIGN_13_MOONS.md`]

## §2. NARRATIVE BEATS

Top-line story moments (each gets a Yarn .yarn node):

- **Day 1:** [intro beat]
- **Day 7:** [companion join / first revelation]
- **Day 14:** [mid-Moon escalation]
- **Day 21:** [companion crisis]
- **Day 28 (17th hour):** [Moon-defining cinematic]

## §3. CORE GAMEPLAY LOOP

What the player does for 90% of in-game minutes. Must be expressible in ≤5 verbs:

- [Scan / excavate / tune / restore — Moon 1]
- [Channel / refract / amplify / shield — Moon 3]
- etc.

## §4. RESTORATION TARGET COUNTS

| Building tier | Count | Spec depth |
|---|---|---|
| Hero | 3 | Buried 80-95% |
| Village | 9-12 | Buried 30-60% |
| POI | 4-6 | At surface |
| Hidden | 1-3 | Secret discovery |

## §5. NPCs

| Role | Name | Speaker GUID | Schedule | Yarn nodes |
|---|---|---|---|---|
| Companion | [name] | [id] | [time-of-day pattern] | intro / lullaby / day14 / day21 / final |
| Innkeeper | [name] | [id] | [Inn 24/7] | greet / rest / quest |
| Villager 1 | [name] | [id] | [routine] | greet / lore |
| ... | | | | |

## §6. ENEMIES

| Enemy | Spawn condition | HP / damage / speed | Loot | Special |
|---|---|---|---|---|
| [Moon-specific enemy] | [trigger] | [stats] | [drops] | [behavior] |

## §7. MINI-GAME VARIANTS

Each Moon should reuse the established tuning variants and may add 0-1 new:

| Variant | Existing? | Used by |
|---|---|---|
| A — Frequency Slider | ✅ since M1 | every Moon |
| B — Waveform Trace | ✅ since M1 | every Moon |
| C — Harmonic Pattern | ✅ since M1 | every Moon |
| D — Cymatic Water | ✅ since M1 | M5+ water tier |
| [E new for this Moon] | new | [specific puzzle] |

## §8. ASSETS REQUIRED

### Blender FBX

```
tools/blender/moonN/gen_*.py — new asset script per item
```

- Buildings: [list]
- Props: [list]
- Characters: [if new]
- VFX rigs: [if new]

### Audio

- Ambient drone layer (60s, loop) — `Resources/Audio/Music/moonN_ambient.wav`
- Exploration arpeggio (60s) — `moonN_exploration.wav`
- Orchestral pad (60s) — `moonN_pad.wav`
- Triumphant brass (60s) — `moonN_triumph.wav`
- SFX: scan / interact / restore / death / pickup / dialogue advance

### Materials

- URP/Lit per building family
- 1 custom shader if needed (e.g. `Tartaria/CrystalRefract` for M6)

## §9. SCENE COMPOSITION

`Assets/_Project/Scenes/Moons/MoonN.unity`:

- `MoonN_Systems` root GameObject with: progression, narrative beats, dialogue bindings, content spawner, hour cycle, zone controller
- Static environment (terrain, large props) composed in scene YAML — not runtime spawned
- `PlayerSpawner` at canonical entry point
- NavMesh baked
- Lighting: Skybox + Directional Light tuned to the Moon's color palette
- PostProcessVolume → `MoonN_PostProcess.asset`

## §10. INPUT (reused, no per-Moon edits)

Already wired in `TartariaInputActions.inputactions`:

- Move (left stick / WASD)
- Look (right stick / mouse)
- Interact (A / E)
- Scan (B / Q)
- Jump (Y / Space) — if Moon mechanics include verticality
- Pause (Start / Esc)

## §11. UI

`Resources/Prefabs/UI/HUD_Root.prefab` is the shared HUD. Per-Moon overlays:

- Quest tracker — feeds from `QuestSystem.cs`
- RS counter
- Day cycle clock
- Companion portraits (if companions present)
- Moon-specific overlay (e.g. tuning mini-game pop-up)

## §12. SAVE INTEGRATION

`Moon1SaveCoordinator.cs` is the reference pattern. For Moon N:

- Add `MoonN_State` to the save schema
- Implement `IPersistable` on `MoonN_Systems` controllers
- Verify F5/F9 round-trip in 8-step smoke test step 8

## §13. AUDIO STATE MACHINE

`AdaptiveMusicController` 4-layer mix:

- Layer 1 — ambient (always playing)
- Layer 2 — exploration (when player moves)
- Layer 3 — tension (when enemies near)
- Layer 4 — triumph (after restoration / completion)

Per Moon: author 4 new stems. Wire via `AdaptiveMusicConfig` ScriptableObject.

## §14. CINEMATICS

Moon-defining moments (`CinematicMoments.cs` handles playback):

- Intro on Day 1
- Companion join
- Mid-Moon crisis
- 17th-hour finale
- Moon completion → next-Moon transition

## §15. PROGRESSION HOOKS

- Day cycle counter advance (auto)
- `MoonN_Completed` GameEvent fires when win condition met
- `ProgressionTracker` ticks +1 Moon completed
- Saves auto-persist
- Next Moon scene unlocks

---

## §16. GATE CRITERIA (per Moon — universal)

### Part A — 8-step smoke test (`CLAUDE.md §2`)

1. Click Play → 0 errors
2. Player visible at spawn (no magenta / T-pose / clip)
3. Movement (WASD / F310 left-stick)
4. Camera follows
5. Reach Moon-canonical interactable ≤30 m walk
6. Press E / A — interaction UI appears
7. Complete interaction — state changes, VFX/audio fires
8. HUD updates (Moon-specific tracker)

### Part B — runtime artifacts

| # | Artifact | Location |
|---|---|---|
| 16.1 | 15-min uncut play-through MP4 | `docs/audits/MoonN_playthrough_DATE.mp4` |
| 16.2 | Profiler capture 1080p mid-spec, 60 FPS sustained | `docs/audits/MoonN_profiler_mid.profile` |
| 16.3 | Profiler capture 1080p low-spec, 30 FPS sustained | `docs/audits/MoonN_profiler_low.profile` |
| 16.4 | RAM ceiling check ≤4 GB after 30 min | `docs/audits/MoonN_ram.txt` |
| 16.12 | 30-min soak test, 0 NRE accumulation | `docs/audits/MoonN_soak.log` |

When all 8 smoke-test steps pass AND all 5 artifacts are committed → **Moon N is GATE-clean**.

Then: update STATUS.md, advance ROADMAP.md, fork the next Moon's spec from this blueprint.

---

## HOW TO USE THIS BLUEPRINT

1. Copy this file to `docs/16+N_MOONN_BUILD_SPEC.md`
2. Fill in every §1-§15 section with that Moon's specific content
3. Author asset scripts under `tools/blender/moonN/`
4. Compose scene under `Assets/_Project/Scenes/Moons/MoonN.unity`
5. Run the 8-step smoke test until green
6. Capture §16 artifacts
7. Commit, update STATUS.md + ROADMAP.md, move on

The reason Moon 1 took 9 months: every system had to be invented. The reason Moons 2-13 should each take 2-3 sessions: the systems already exist, only the **content** changes.

---

*MOON_BLUEPRINT.md v1.0 · 2026-06-05*
