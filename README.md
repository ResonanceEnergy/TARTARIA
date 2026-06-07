# TARTARIA WORLD OF WONDER — Aether Awakening

> *Tune the World. Light the Ley Lines. Reclaim the Golden Age.*

A session-based, open-world restoration RPG / light city-builder / harmonic puzzle hybrid for PC. Players awaken as a Tartarian descendant in a post-Mud Flood world, excavating buried wonders, tuning atmospheric Aether through sacred-geometry architecture, and restoring a globe-spanning free-energy grid — one glowing dome at a time.

---

## Status

**Phase:** Alpha 0.4 — Moon 1 shell playable, **on a FOUNDATIONS FIRST implementation pause** while the Unity 6 URP setup is brought up to spec. (See plan below.)
**Target:** All 13 Moons content-complete before any distribution decision (per 2026-05-30 + 2026-06-03 owner mandates). No release framing of any kind. No Win64 build pipeline. No beta program.
**Build:** No public build yet — in development. No public build is planned for any Moon individually.
**Engine:** Unity 6 LTS (6000.3.6f1) with URP Forward+.

> **2026-06-07 audit:** the project has been going in circles because 1252 of 1284 FBX are Git-LFS pointer stubs (`git lfs pull` was never run), no real medieval architecture pack exists on disk, 103 `RuntimeInitializeOnLoadMethod` scripts race to build the scene every Play, and 8 critical Unity 6 URP fundamentals are unconfigured. The path forward is documented as 8 phases in **[docs/plans/MOON1_FOUNDATIONS_FIRST.md](docs/plans/MOON1_FOUNDATIONS_FIRST.md)** — read that before any Moon-1-content work.

For the honest current-state snapshot, see **[STATUS.md](STATUS.md)**.
For the realistic implementation plan, see **[docs/plans/MOON1_FOUNDATIONS_FIRST.md](docs/plans/MOON1_FOUNDATIONS_FIRST.md)**.
For the build order across all 13 Moons, see **[ROADMAP.md](ROADMAP.md)**.
For guidance future Claude sessions need on the codebase, see **[CLAUDE.md](CLAUDE.md)**.
For the full campaign design, see **[docs/03_CAMPAIGN_13_MOONS.md](docs/03_CAMPAIGN_13_MOONS.md)**.
For the art production pipeline + plan, see **[docs/art/ART_PRODUCTION_PLAN.md](docs/art/ART_PRODUCTION_PLAN.md)**.

## Art pipeline (Blender)

This project includes a working headless-Blender pipeline. Run `Tartaria → Moon 1 → Run Blender Batch (Generate All Moon 1 Assets)` from within Unity to regenerate 12 hand-authored FBX models — brazier, pipe organ, mud pool basin, lore artifact, giant skeleton key, skeleton remains, rocking chair, Bob's Inn, tuning pedestal, 3 Aether crystals. Each model is defined in `tools/blender/gen_*.py` as parametric Python; edit the script, re-run the batch, the FBX + auto-generated URP/Lit prefab variant update in seconds. Verified working with Blender 4.5 LTS + 5.0.

`PHASE_1_SCOPE.md` and `TARTARIA_MASTER_PLAN.md` are **archived** — they framed a vertical-slice / release-first agenda that was reversed by the 2026-05-30 mandate and re-confirmed by the 2026-06-03 mandate. Any reference to "ship Moon 1", "Win64 build", "itch.io", "Steam", "beta program" in those archived docs is historical only.


## Controller (Logitech F310)

Primary dev controller is the Logitech F310, wired USB. The X/D switch on the back should be set to **X** (XInput mode). The button map is canonical in [docs/appendices/D_CONTROLS_F310.md](docs/appendices/D_CONTROLS_F310.md) — A=Interact/Pulse, B=Scan, X=Pulse, Y=Aether Vision, LB=Sprint, RB=Harmonic Strike, LT=Frequency Shield, RT=Sprint alt, Start=Pause, Back=Aether Vision alt, D-Pad ←/→=Frequency adjust, D-Pad ↑=Scan, L3=Sprint toggle, R3=Recenter camera. Every button has a real implementation in `PlayerInputHandler.HandleGamepadButtonFallbacks()` — no stubs.

Verify in Play mode via the `InputProbeHUD` overlay (top-left of Game view) — it lists `Keyboard.current`, `Gamepad.current (XInput)`, `Joystick.current (DInput)`, device count, focus state, live left-stick values, and last button pressed.

### What's actually working today

- ~810 C# scripts, 23 assemblies, clean dependency graph, 0 compile errors.
- DOTS ECS Resonance Score system, GameEvents pub/sub bus, AES-256-encrypted save (schema v18).
- Echohaven scene playable end-to-end: player spawns, moves (left stick + WASD), camera follows, 3 hero buildings present + buried at correct depths, tuning mini-game (3 variants), HUD live (RS, Aether, objective, interaction prompt).
- 12 structures + 6 POIs + 120 vegetation + 69 props + 4 named NPCs in Echohaven.
- 12 Editor menus under `Tartaria/` for one-click scene composition (`MASTER: Bootstrap All Moon 1 Systems`, `Build Out Moon 1 Buildings/Environment/Vegetation/Village/Props/NPCs`, `Wire Echohaven Audio`, `Combat Verify`, `Ready Check`).
- 110+ KayKit models, 80+ Hovl/Unity VFX prefabs, 33 Polyhaven PBR materials, Kenney UI audio, 25 RPG ambient tracks.

### What's NOT yet built (Moon 1)

Per `docs/03 Days 6–28` and `docs/15`:
- Pipe organ centerpiece with 3-note tuning puzzle.
- Rose window cymatic projection.
- Pure water font particle + audio restore.
- Spire placement ceremony.
- Ley line mini-map reveal.
- Reset Scout enemy (distinct from Mud Golem).
- Giant Mode 60-sec burst.
- 17th-hour cathedral light eruption.
- Lirael's 432 Hz lullaby + animated appearance.
- Skeleton hum first-prophecy fragment.
- Giant skeleton key #1.
- Dialogue runner hookup for the 3 existing Yarn files.

### How to play the current build

(Assuming a fresh clone — these are click-by-click in Unity 6.)

1. Open `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`.
2. Run, in order, the menus:
   - `Tartaria → MASTER: Bootstrap All Moon 1 Systems` (wires 11 dormant subsystems)
   - `Tartaria → Build Out Moon 1 Buildings (3 Hero)`
   - `Tartaria → Build Out Moon 1 Environment (POIs + Mud)`
   - `Tartaria → Build Out Moon 1 Vegetation (Grass+Bushes)`
   - `Tartaria → Build Out Moon 1 Village (9 secondary structures)`
   - `Tartaria → Build Out Moon 1 Props (Rocks + Lore Stones + Fallen Pillars)`
   - `Tartaria → Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian)`
   - `Tartaria → Wire Echohaven Audio (Ambient + SFX)`
   - `Tartaria → Combat Verify (Moon 1)`
   - `Tartaria → Ready Check (Audit + Bake + Save)`
3. Hit Play. WASD or left stick to walk forward toward the buildings. E or gamepad A to interact.
- 3 building interactions wired (StarDome, HarmonicFountain, CrystalSpire)
- One Resonance Score event captured on video

### What's still missing for Moon 1 to be 100% (then Moon 2 begins)

Per `CLAUDE.md` § "What Moon 1 100% actually means" + `STATUS.md` punch list:

- `Prefabs/Moon1/AnastasiaRocker.prefab` — Editor bake menu exists, never invoked
- Hero buildings still composed of `Detail_*` primitive clusters (mesh replace menu unblocked, not invoked)
- 347 flat `Prefabs/Moon1/Blender/*.prefab` need categorical migration
- 11 silent-fail empty `catch {}` blocks outside Moon 1 happy path
- `RuntimeHUDBuilder.cs` 64 runtime `new GameObject` calls (full HUD_Root prefab bake deferred)
- `RuntimeSpawnerInsurance.cs` dead-weight file
- Real end-to-end Moon 1 play-through to witness the 17th-hour beat, skeleton hum, giant key #1, restored ley line mini-map
- Per-Moon-1 mini-game variants A/B/C/D all playable + randomly assigned per `docs/15 §9`
- 9 village buildings + props re-verified post Prefab Hygiene path moves

**Per 2026-06-03 NATRIX mandate: no itch.io, no Steam, no Win64 build, no beta — until all 13 Moons are content-complete.**

---

## Repository map

```
TARTARIA_new/
├── README.md                 # This file
├── STATUS.md                 # Current state of play (single source of truth)
├── PHASE_1_SCOPE.md          # Moon 1 scope lock — nothing outside this ships first
├── TARTARIA_MASTER_PLAN.md   # Strategic plan (Track A ship + Track B platform)
├── KNOWN_ISSUES.md           # Live bug tracker
├── TROUBLESHOOTING.md        # Player support
├── CONTRIBUTING.md           # Contributor guidelines
├── CHANGELOG.md              # Version history
├── ROADMAP.md                # System-level done-list (older, kept for reference)
│
├── Assets/                   # Unity project — models, audio, scenes, scripts
│   └── _Project/Scripts/     # Game code (23 assemblies)
│
├── docs/                     # All design docs
│   ├── 00_MASTER_GDD.md      # Master GDD
│   ├── 01_LORE_BIBLE.md
│   ├── 02_AETHER_ENERGY_SYSTEM.md
│   ├── ...                   # (30 main docs + 10 appendices + 10 DLC docs)
│   ├── agent_reports/        # Historical AI agent swarm reports (preserved, mostly invalid)
│   └── archive/              # Superseded status, asset inventories, old README
│
├── scripts/                  # PowerShell build/test/dev automation
├── Tools/                    # Editor automation tools
├── Build/ Builds/            # Output (empty — no .exe built yet)
└── memories/                 # Agent memory store
```

---

## Vision (design pillars)

These are the design pillars from `docs/00_MASTER_GDD.md`. They are stable — what's changing is the realistic *scope* and *timeline* to deliver them.

- **Explore & Excavate** — Dig through physics-based mud layers to reveal Tartarian grandeur
- **Tune & Align** — Play 3-6-9 harmonic sequences on pipe organs and cymatic puzzles
- **Restore Architecture** — Snap sacred-geometry templates to golden-ratio grids
- **Harvest Aether** — Watch wireless energy flow as glowing ley lines across the world map
- **Defend & Expand** — Combat dissonance entities with resonance weapons

All 13 Moons are built fully in order — Moon 1 → Moon 2 → … → Moon 13 — before any release discussion. No "Moon 1 ships first" detour. The 10 DLCs are post-base-game scope, not on the active flight path.

---

## Tech stack

- **Engine:** Unity 6 LTS (6000.3.6f1) — DOTS/ECS, URP, Addressables, Burst + Jobs
- **Platform:** Windows 10/11 — Min: GTX 1070 / 8 GB RAM; Recommended: RTX 3060 / 16 GB RAM
- **Graphics:** Vulkan / DX12 + FSR 2 / DLSS, baked lighting, BC7 compression
- **Audio:** 432 Hz adaptive soundtrack + cymatic sound design
- **Distribution:** TBD post-build. Per 2026-06-03 NATRIX mandate, no distribution channel decision is being made until all 13 Moons are content-complete.

---

## Quick navigation — design docs

| Category | Docs |
|----------|------|
| **Core Design** | [00 Master GDD](docs/00_MASTER_GDD.md) · [01 Lore Bible](docs/01_LORE_BIBLE.md) · [02 Aether System](docs/02_AETHER_ENERGY_SYSTEM.md) |
| **Campaign** | [03 13 Moons](docs/03_CAMPAIGN_13_MOONS.md) · [03A Storyline](docs/03A_MAIN_STORYLINE_REWRITE.md) · [03B Expansions](docs/03B_EXPANSION_PACKS.md) · [03C Moon Mechanics](docs/03C_MOON_MECHANICS_DETAILED.md) |
| **World** | [04 Architecture](docs/04_ARCHITECTURE_GUIDE.md) · [26 Level Design](docs/26_LEVEL_DESIGN.md) · [12 Visuals](docs/12_VIVID_VISUALS.md) |
| **Characters** | [05 Characters](docs/05_CHARACTERS_DIALOGUE.md) · [18 Anastasia](docs/18_PRINCESS_ANASTASIA.md) · [22 Dialogue Branching](docs/22_DIALOGUE_BRANCHING.md) |
| **Systems** | [06 Combat](docs/06_COMBAT_PROGRESSION.md) · [13 Mini-Games](docs/13_MINI_GAMES.md) · [19 Economy](docs/19_ECONOMY_BALANCE.md) · [20 Quests](docs/20_QUEST_DATABASE.md) |
| **PC Experience** | [07 PC UX](docs/07_PC_UX.md) · [14 Haptics](docs/14_HAPTIC_FEEDBACK.md) · [25 Save System](docs/25_SAVE_SYSTEM.md) · [27 Tutorial](docs/27_TUTORIAL_ONBOARDING.md) |
| **Production** | [09 Tech Spec](docs/09_TECHNICAL_SPEC.md) · [10 Roadmap](docs/10_ROADMAP.md) · [15 MVP Build Spec](docs/15_MVP_BUILD_SPEC.md) · [29 Pipeline](docs/29_PRODUCTION_PIPELINE.md) |
| **Appendices** | [A](docs/appendices/A_GLOSSARY.md) · [B](docs/appendices/B_ASSET_REFERENCE.md) · [C](docs/appendices/C_AUDIO_DESIGN.md) · [D](docs/appendices/D_CONTROLS.md) · [E](docs/appendices/E_METRICS.md) · [F](docs/appendices/F_MOON_INDEX.md) · [G](docs/appendices/G_NPC_INDEX.md) · [H](docs/appendices/H_MECHANIC_INDEX.md) · [I](docs/appendices/I_DLC_INDEX.md) · [J](docs/appendices/J_ENEMY_INDEX.md) |
| **DLC** | [01](docs/dlc/DLC_01_BURIED_BEACON.md) – [10](docs/dlc/DLC_10_TRUE_TIMELINE.md) |

---

## A note on the older "BETA READY" claim

E