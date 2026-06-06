# TARTARIA — Moon 1 (Echohaven) Beta Test Guide

> **Closed beta · 2026-06-02 · Build `moon1-ship-candidate`**
> Thank you for helping us shape Moon 1. Your job: play. Our job: listen.

---

## What this is

You're holding the first playable Moon of **TARTARIA WORLD OF WONDER — Aether Awakening**, a Unity 6 single-player restoration / city-builder hybrid. Moon 1 is the **Echohaven** valley — a buried village waiting to be sung awake.

This is **not** a finished game. It's a vertical slice of the first Moon, ~20–30 minutes of playable content, intended to surface bugs, balance issues, and emotional pacing problems before the rest of the 13 Moons get built on top.

---

## System requirements

| | Minimum | Recommended |
|---|---|---|
| OS | Windows 10 64-bit | Windows 11 64-bit |
| CPU | 4-core 2.5 GHz | 6-core 3.5 GHz |
| RAM | 8 GB | 16 GB |
| GPU | Vulkan 1.1, 4 GB VRAM | Vulkan 1.2, 8 GB VRAM |
| Disk | 12 GB | 12 GB SSD |
| Input | Keyboard + mouse | Logitech F310 / Xbox-style controller (X-mode) |

---

## How to run

1. Unzip `TARTARIA_Moon1.zip` anywhere with 12 GB free
2. Run `TARTARIA_Moon1.exe`
3. If Windows SmartScreen blocks it: click "More info" → "Run anyway" (the build is unsigned for the beta — final release will have a code-signing cert)

---

## Controls

**Keyboard + Mouse**
- WASD — movement (camera-relative)
- Mouse — look
- E — interact
- Y — Aether Vision (toggle, 10-second stamina)
- Tab — Lorebook
- F5 — Quicksave
- F9 — Quickload
- Esc — pause

**Logitech F310 gamepad (X-mode — switch on back set to X)**
- Left stick — move
- Right stick — camera
- A — interact
- B — scan/cancel
- X — resonance pulse
- Y — Aether Vision
- LB — sprint hold
- RB — harmonic strike (combat)
- Start — pause

Full controls reference: `docs/appendices/D_CONTROLS_F310.md` in the source repo.

---

## What to try

The 20–30 minute happy path:

1. **Boot** — main menu loads, click New Game
2. **Spawn** — you appear at the Echohaven overlook, looking at the buried village
3. **Meet Milo** — he greets you with a tutorial flow (6 steps, you can press Esc to skip if you've played before)
4. **Light braziers** — walk to any of the 3 ring braziers, press E. Light all 3 to complete the ring — a banner reads "The Braziers Wake".
5. **Find a tuning pedestal** — green glow near a buried building. Press E.
6. **Mini-game** — match the frequency (Variant A slider, B waveform trace, or C harmonic pattern). Each building has a per-node variant per docs/15 §9. Success → the building rises.
7. **Restore all 3 hero buildings** — Cathedral, Star Dome, Spire. Order doesn't matter.
8. **Post-restoration cinematic** — when the 3rd lands, a 30-second sequence plays. Lighting shifts from "muddy dusk" to "golden hour". Fountain water flows. Spire pulses Telluric brown.
9. **Walk the village** — 5 named NPCs have ambient idle + a one-line greeting (E key). Bram the Smith, Marisol the Weaver, Old Tobias, Wren the Apprentice, Father Caelum.
10. **Quit cleanly** — Esc → main menu → Quit.

Want to test save/load? Save with F5 between any two steps, quit, re-launch, Continue.

Want to test difficulty? Settings → Difficulty → Story / Standard / Hardened. Mud Golem HP + mini-game forgiveness change accordingly.

Want to test the Day-25 Lirael gate? Currently requires playing 25 in-game days OR using a debug menu (developer-only — let us know if you want a build with the dev menu exposed).

---

## What we want from you

**Tier 1 (most useful):** crashes, soft-locks, things you can't undo.

**Tier 2:** missing assets, magenta materials, NPC T-poses, audio glitches.

**Tier 3:** balance — mini-games too hard/easy, combat damage feels wrong, controls feel awkward.

**Tier 4:** vibes — does any moment feel emotionally flat? Does Milo's tutorial feel patronizing? Does the post-restoration cinematic earn its 30 seconds?

Please file each report against the template in `BETA_FEEDBACK_TEMPLATE.md`. One file per session is fine — link reports to specific moments.

---

## What we know is rough

- 17 deprecation warnings in the Unity Editor (Sprint 10 cleared the worst; remainder is non-blocking)
- 3 NPC FBX models are first-pass Blender humanoids (Lirael, Anastasia, Cassian) — no proper rig yet; they'll T-pose during walk animations
- 5 named villagers fall through to capsule + sphere placeholders if KayKit prefabs aren't found at the expected paths
- Some VFX prefab refs throw asset-import errors on first scene load — visuals work anyway
- Carved Stone POI placement may overlap a building wall on hilly terrain
- itch.io distribution gated on artist sign-off — for now beta distribution is Discord + Drive
- Mud Lord boss spawn is currently disabled (Sprint 5 file deletion) — Moon 1 ends at the cathedral-light sequence

None of these should block your playthrough, but worth knowing what's intentional vs a bug.

---

## How to send feedback

| Channel | What for |
|---|---|
| Discord `#moon1-beta` | Quick reports, screenshots, video clips |
| Discord DM @NATRIX | Private feedback or save files |
| Email nate@gripandripphdd.com | Long-form writeups, longer save attachments |

Save files live at `%APPDATA%\..\LocalLow\ResonanceEnergy\TARTARIA\saves\` — zip the whole folder if you're sending one.

---

## Privacy

The build contains no telemetry or analytics. We don't know you ran it unless you tell us. Save files stay on your machine.

---

*v1 · 2026-06-02 · ship-candidate beta*
