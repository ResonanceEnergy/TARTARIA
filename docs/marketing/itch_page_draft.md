# TARTARIA WORLD OF WONDER — itch.io Page Draft

> Sprint 6 Lane 8 — Marketing copy for the Moon 1 itch.io drop.
>
> Honest scope: this page describes **Moon 1: Echohaven only** — the first chapter
> of a planned 13-Moon campaign. Do not promise unfinished Moons in public copy.
>
> Lore framing: every reference to the buried civilization is in-fiction myth.
> No real-world conspiracy framing, no real historical family names, no real-world
> political flashpoint vocabulary. See `CLAUDE.md` "Open lore/political risk
> callouts" for the canonical scrub list.

---

## Title

**TARTARIA WORLD OF WONDER — Aether Awakening (Moon 1: Echohaven)**

## Tagline

*Wake the buildings. Tune the world. Restore what time forgot.*

## Cover image suggestion

Use `shot_07_full_moon1_vista.png` from the capture pipeline — the buried Star
Dome silhouette at golden hour with the Crystal Spire pulsing in the distance.

---

## Short Description (~250 words)

In the world of Tartaria, an ancient civilization sleeps beneath the earth. Its
cathedrals, star domes, and resonant spires were buried by a forgotten cataclysm
ages before living memory — only the wind through their broken roofs still
remembers their song.

You are Lirael, a wandering tuner who can hear what the world has forgotten. With
nothing but a tuning fork, a stubborn heart, and the company of a mute boy named
Milo, you arrive in the half-buried village of Echohaven on the eve of the first
Moon of Aether Awakening. The villagers — what few remain — believe the buried
ruins are cursed. You believe they are simply out of tune.

Moon 1: Echohaven is the opening chapter of a planned 13-Moon restoration
campaign. In this chapter you'll:

- Explore a buried cathedral district hand-built from a heritage architecture kit
- Tune three hero buildings (Star Dome, Harmonic Fountain, Crystal Spire) using a
  rhythm-and-frequency mini-game across three difficulty variants
- Toggle **Aether Vision** to see resonance harmonics layered over the world
- Walk a small cast of villagers through their first night of remembering — Milo,
  Anastasia the herb-keeper, and Cassian the displaced caretaker
- Meet the Mud Lord, the first of the campaign's harmonic guardians

This is an **early alpha** snapshot. Moons 2 through 13 are in active development
and will land as separate updates. Save files carry forward.

---

## What's in Moon 1

- **3 hero buildings** restorable via the tuning mini-game
- **9 village structures** dressed with hand-placed props
- **6 points of interest** including the Mud Pool, the Lirael Grotto, and the
  Hidden Grotto behind the Spire
- **3 tuning mini-game variants** (Listen, Match, Sustain) per the build spec
- **4 named characters** with voice-direction notes and Yarn-scripted dialogue
- **1 boss encounter** — the Mud Lord, three-phase
- A 30-second cinematic lullaby beat at the threshold of the seventeenth hour
- Adaptive music engine layering three harmonic bands (Telluric 7.83 Hz,
  Harmonic 432 Hz, Celestial 528 Hz) under the soundtrack
- A 12-node Aether Resonance progression tree

---

## Controls

| Action | Keyboard / Mouse | Logitech F310 (X-mode) |
|---|---|---|
| Move | WASD | Left stick |
| Look | Mouse | Right stick |
| Interact | E | A (south) |
| Quicksave | F5 | — (menu only) |
| Quickload | F9 | — (menu only) |
| Aether Vision | Y | Y (north) / Back |
| Lorebook | Tab | — (menu only) |
| Pause | Esc | Start |
| Sprint | Shift | LB hold |
| Recenter camera | — | R3 click |

Full F310 button map: `docs/appendices/D_CONTROLS_F310.md`.

---

## System Requirements

**Minimum**

- Windows 10 (64-bit) or Windows 11
- 8 GB RAM
- GPU with Vulkan 1.1 support, 4 GB VRAM
- 12 GB free disk space
- 1080p display recommended

**Recommended**

- Windows 11
- 16 GB RAM
- Dedicated GPU with 6 GB+ VRAM (GTX 1660 / RX 5600 or better)
- SSD install location for faster scene streaming

The build target is `StandaloneWindows64`, Unity 6 (6.3.6f1 LTS), URP. macOS and
Linux builds are not on the Moon 1 release plan.

---

## Screenshot Grid

The capture pipeline (`scripts/dev/capture-itch-screenshots.ps1`) writes 8 hero
shots into `Builds/itch_assets/`. Place them in the itch.io page in this order:

| Slot | File | Caption |
|---|---|---|
| 1 | `shot_00_cathedral_exterior_dusk.png` | "The cathedral remembers its name." |
| 2 | `shot_01_star_dome_lit.png` | "Star Dome — the first building to wake." |
| 3 | `shot_02_spire_pulsing.png` | "The Crystal Spire answers in harmonic light." |
| 4 | `shot_03_village_center_wide.png` | "Echohaven at the threshold of the first Moon." |
| 5 | `shot_04_mud_pool_poi.png` | "The Mud Pool is older than the village above it." |
| 6 | `shot_05_lirael_grotto.png` | "A tuner's grotto, kept secret across seasons." |
| 7 | `shot_06_aether_vision_overlay.png` | "Aether Vision: see what the world still hums." |
| 8 | `shot_07_full_moon1_vista.png` | "Moon 1, in full view, before the seventeenth hour." |

---

## Pricing

**Pay what you want.** A suggested minimum of $5 USD helps fund Moons 2 through
13. Anyone who buys Moon 1 receives every subsequent Moon update at no extra
cost — this is a single ongoing project, not a season pass.

If you cannot afford the suggested minimum, take the build anyway and consider
sharing it with one other person who'd enjoy it. The restoration mythos runs on
word of mouth.

---

## Known Limitations (Moon 1 alpha)

- Saved games from this build are forward-compatible with Moons 2–13 but may need
  one auto-migration step on first load of a future Moon.
- The Aether Vision overlay is a URP screen feature; very old integrated GPUs may
  render it as a flat tint rather than full harmonic layering.
- Controller hot-swap during play is unreliable; reconnect from the pause menu if
  the F310 disconnects mid-session.
- One known soft-lock: standing inside the Mud Pool with Aether Vision toggled
  AND Frequency Shield held will pin the audio mixer in the Telluric band. Toggle
  Aether Vision off to recover.

---

## Credits Slug

Single-developer alpha. Built in Unity 6 with the KayKit heritage architecture
kit, Hovl Studio VFX, Polyhaven textures, and a small batch of bespoke Blender
assets. Music composed against a 7.83 / 432 / 528 Hz harmonic stack. Voice notes
authored; recordings deferred to a later Moon.

Cover art: in-engine screenshots from this build. No external concept art.

---

## Itch.io Tags

`adventure` `singleplayer` `exploration` `unity` `early-access` `windows`
`mythic` `restoration` `crafting-light` `controller-supported`

Do **not** add tags that imply finished scope (`epic`, `complete`, `13-moons`).
This page is honest about being Moon 1 only.

---

## Pre-publish review checklist (DO NOT REMOVE)

Before pushing this copy to the live itch.io page, verify against
`CLAUDE.md` -> "Open lore/political risk callouts":

- [ ] No "Tartaria conspiracy" framing — this draft frames the buried civilization
      as **in-fiction myth**, not as a hidden real-world truth.
- [ ] No "Great Reset" / "Reset agents" vocabulary — those antagonist names are
      not used here. If they appear in the in-game lore by ship time, the page
      copy should still avoid the explicit phrasing.
- [ ] No "Parasite Cabal" — antagonist faction not named in this copy.
- [ ] No Romanov / imperial-Russian framing of Anastasia — she is described only
      as "the herb-keeper", no royal title, no historical-family parallel. If
      marketing wants a portrait shot, do not stage her in Romanov-coded regalia.
- [ ] No "Orphan Train cultural genocide" framing — Moon 1 copy does not touch
      that storyline; if a later Moon page does, route it through a sensitivity
      reader first.
- [ ] Honest Moon 1-only scope: the copy never promises Moons 2-13 as shipped
      content, only as planned updates.

*Draft authored Sprint 6 Lane 8 (2026-06-02). Re-read CLAUDE.md political-risk
callouts before publishing any change.*
