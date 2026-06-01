# PHASE 1 — Moon 1 Scope Lock  *(ARCHIVED 2026-05-30)*

> ⚠️ **THIS DOC IS ARCHIVED.** It described an itch.io vertical-slice ship plan that NATRIX explicitly reversed on 2026-05-30: build all 13 Moons fully before any release talk. The "scope lock" framing is no longer load-bearing.
>
> Source of truth now: `CLAUDE.md` (2026-05-30 mandate) → `ROADMAP.md` (Moon-by-Moon build order) → `STATUS.md` (week-by-week state) → `docs/03_CAMPAIGN_13_MOONS.md` (per-Moon spec).
>
> This file is preserved unchanged below for historical context, but its `Target ship`, `itch.io`, "deferred / out of scope" sections are dead. Treat doc 15's "vertical slice" sections as the **minimum** for Moon 1, not the maximum.

---

**Owner:** NATRIX
**Original target (now void):** itch.io public beta, ~12 weeks from 2026-05-29
**Reference:** `TARTARIA_MASTER_PLAN.md` § 3 (Track A), `docs/15_MVP_BUILD_SPEC.md` (the canonical MVP spec from 2026-03-25)

---

## In scope (the playable artifact)

The thing a stranger downloads, installs, plays for 15+ minutes, and tells a friend about. Per `docs/15_MVP_BUILD_SPEC.md` § 1:

### Zone — Echohaven (1 zone only)
- 500m-radius single zone
- Mud-covered surface revealing Tartarian architecture beneath
- 3 restorable buildings: **The Dome ("Listeners' Hall"), The Fountain ("Thread of Memory"), The Spire ("The First Note")**
- 4 non-restorable POIs: Mud Pools, Carved Stone, Overlook, Root Chamber
- Day/night cycle (visual only — 17-hour Tartarian day mapped to ~17 min real-time)

### Player
- WASD movement + camera follow
- 3 abilities: **Resonance Pulse** (left-click AOE), **Harmonic Strike** (right-click directed), **Frequency Shield** (Ctrl hold absorb)
- Tab toggles Aether vision (highlights buried structures)
- Gamepad support (Xbox + DualSense + Logitech F310)

### Companion — Milo
- Follow, idle, react, speak, hide states
- ~10 voice lines minimum (placeholder TTS acceptable for beta)
- Lore + tutorial trigger system

### Enemy — Mud Golem (1 type only)
- HP 100, 0.6× player speed, telegraphed slam attack
- Spawns at RS thresholds 25, 50, 75 (1 each)
- 3–4 Harmonic Strikes to defeat

### Tuning mini-game (3 variants per § 9 of MVP spec)
- Variant A: Frequency Slider (drag to match 432 Hz target)
- Variant B: Waveform Trace
- Variant C: Harmonic Pattern (5-circle rhythm tap)

### Resonance Score system
- Real DOTS ECS system (already exists in `Tartaria.Core`)
- Threshold events fire at RS 25 / 50 / 75 / 100
- Golden-ratio validator applies × 1.618 multiplier on perfect tuning

### Aether field
- 3-band visualization (Telluric 7.83 Hz / Harmonic 432 Hz / Celestial 528 Hz — using 528 not 1296 per the resolved contradiction)
- GPU compute shader at 64³ grid
- 8k particle billboards max for ambient flow

### Audio
- 1 ambient 432 Hz music track (Drake Stafford, already imported)
- 15 minimum gameplay SFX: footsteps × 2, scan pulse, restore, hit × 2, golem footstep, golem death, pickup, building emergence, threshold cross, tuning success, ambient hum, crystal chime, victory

### VFX
- 3 wired: ScanPulse, RestoreSparkle, AetherCollect
- Mud dissolution shader on building restoration (5-sec animation)

### Save / Persistence
- Local JSON to `Application.persistentDataPath`
- Atomic write with checksum (already implemented + AES-256)
- 3 manual slots + 1 auto-save (auto on RS change, building state change, alt-tab)

### UI
- HUD: RS counter (live), health bar (live), Aether meter (live)
- Main menu: New Game / Continue / Settings / Quit
- Pause menu: Resume / Save / Settings / Main Menu
- Settings: Volume sliders, key rebinding, gamepad detection, resolution, V-Sync

### Haptics
- Per `docs/14_HAPTIC_FEEDBACK.md` § 13 table — 9 patterns minimum (footstep, discovery, tuning on/off frequency, perfect tune, building emergence, golem spawn, combat hit, golem death)

### Performance target
- 60 FPS sustained on RTX 3060 / 16 GB RAM at 1080p
- 30 FPS sustained on GTX 1070 / 8 GB RAM at 1080p (FSR Performance mode)
- ≤ 4 GB RAM after 30 minutes of play

### The 15-minute demo arc
Per § 1 of `docs/15_MVP_BUILD_SPEC.md`, the player must be able to complete this sequence:

```
0:00–2:00   Awaken in mud, first movement, hear hum
2:00–4:00   Milo appears, dome discovery, Aether scan tutorial
4:00–6:00   First tuning node, dome begins emerging
6:00–8:00   Second + third tuning nodes
9:00–10:00  Building restored — fountain inside flows
10:00–12:00 Mud Golem spawns, first combat
12:00–14:00 RS crosses 75 — zone color shift
14:00–15:00 Vista from restored dome, "this is just the beginning"
```

---

## Out of scope — deferred to post-launch

Per the Master Plan mandate "Do not ship Moon 2 in the same launch":

- **Moons 2–13** (all stub-removal work, all scene content, all narrative beats)
- **All 10 DLCs** (DLC_01 through DLC_10 stay as design docs only)
- **Day Out of Time festival** (post-Moon 13 event)
- **Additional companions** beyond Milo (Lirael, Thorne, Korath, Veritas, Cassian, Anastasia silence-design — all post-launch)
- **Additional enemies** beyond Mud Golem (Skeletons, Dissonance Defender, Wind Wraith, etc.)
- **Skill tree** progression (Resonator / Architect / Guardian / Historian trees)
- **Inventory + crafting** (system code exists; UI does not need to ship for Moon 1 demo)
- **Quest system** beyond 3 starter quests (the 184-quest database is design-doc-only for ship)
- **Boss encounters** (Moon 1 has no boss; Mud Golem is the apex threat)
- **Giant Mode** (Moon 2+ mechanic)
- **Cymatic water tuning, harmonic rock cutting, pipe organ symphony, bell tower sync, resonance rail alignment, micro-giant fractal purge** (all 6 mini-games beyond the 3 tuning variants are out)
- **F2P stack** (battle pass, subscription, rewarded ads, Resonance Crystal IAP) — delete entirely from Moon 1 build; itch.io is premium/pay-what-you-want
- **Steam integration** (Cloud, Achievements, Trading Cards) — itch.io first, Steam later
- **Cloud save sync** (local-only for Moon 1 ship)
- **Localization beyond English**
- **WCAG 2.1 AA Tier-C** (subtitles + colorblind + remappable controls is in; full screen-reader gameplay narration is out)
- **All Track B work** — module loader, contribution flow, Discord, GitHub Pages hub, content engine (per `TARTARIA_MASTER_PLAN.md` § 4)

---

## Scope-change protocol

If any of the following happens, this doc is **frozen** until you explicitly approve a change:

- A swarm proposes adding a Moon 2+ system to "round out the experience" → **NO**
- An agent claims it found bandwidth to also build the inventory UI → **NO**
- A "quick refactor" of `Tartaria.Core` is suggested mid-Phase A → **NO**
- A music composer offers to do all 13 zones in parallel → ship the Moon 1 track first
- A 2-week side-project to "stand up the module loader" → defer to Track B after Moon 1

The only valid scope changes are:
- **Reductions** — cutting an in-scope item if it's blocking Moon 1 ship.
- **Bug fixes** to listed in-scope items.
- **Tooling** improvements that accelerate the Day 1–4 execution plan in `STATUS.md`.

Anything else: file a note for post-launch and keep moving.

---

## Definition of done

Moon 1 ships when:

1. CS:0 build (zero compile errors).
2. The 15-minute demo arc above is playable end-to-end, recorded as video.
3. 60 FPS sustained on RTX 3060.
4. 30 FPS sustained on GTX 1070.
5. No crash in 1-hour session.
6. Save/Load round-trip: quit mid-tuning, restart, continue at correct state.
7. The 12 GATE 1 mandatory criteria from `docs/15_MVP_BUILD_SPEC.md` § 16 are all checked.
8. After all of the above, NATRIX and at least 2 internal testers play the 15-min demo and answer unanimously **"yes"** to *"Do I want to keep playing?"* If anyone says no, iterate before shipping.
9. itch.io page is published, screenshots + 30-sec trailer attached, free or pay-what-you-want pricing.

---

*Scope lock v1.0 · 2026-05-29 · Cited in every Phase A1–A4 PR description until Moon 1 ships.*
