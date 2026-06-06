# TARTARIA: Aether Awakening — Moon 1 (Echohaven)

> Moon 1 ship-candidate release notes. Player-facing.
> Channel: itch.io `moon1-windows` (Windows x64, pay-what-you-want).
> Date: 2026-06-02.

---

## What you get

Moon 1 is the opening chapter of TARTARIA: Aether Awakening — the buried village of Echohaven, restored from beneath the mud-flood by tuning its resonant landmarks back into the world. You'll restore **3 hero buildings** (the Cathedral, the Star Dome, and the Aether Spire) using three distinct tuning mini-games (Frequency Slider, Waveform Trace, Harmonic Pattern), repair **9 surrounding village buildings**, explore **6 points of interest** (3 Mud Pools, the Carved Stone, the Overlook, the Root Chamber), meet **named villagers** (Bram the Smith, Marisol the Weaver, Old Tobias, Wren the Apprentice, Father Caelum) plus the core Moon 1 cast (Milo, Lirael, Cassian, and Anastasia at the Day-25 reveal), light the **3-brazier ritual** that wakes the dome, and finish the moon with the post-restoration cinematic. **Save/load** runs on a 5-slot system with thumbnails and JSON sidecar metadata. **Settings** persists audio (Master/Music/SFX), graphics quality, and input config. **Full Logitech F310 controller support** is in (X-mode recommended) alongside keyboard + mouse — both work simultaneously and you can swap mid-play.

## Known issues (non-functional)

- **17 deprecation warnings at compile time.** These are Unity 6 API deprecation notices (mostly `FindObjectOfType` → `FindFirstObjectByType`). No runtime impact — the game runs identically. Will be cleaned up in a follow-up patch.
- A handful of cosmetic ⚠ items remain on the v3 acceptance audit (vegetation density below spec target, one POI placement not yet auto-wired). None affect gameplay completion. See `STATUS.md` § "Final punch list" for the full list with file:line citations.
- The 3 SaveSlotPanel UI implementations on disk will be triaged to one canonical version in a follow-up; the active one (registered via `MainMenu.Continue` and `Pause.Load`) works as intended.

## Credits

See `docs/credits/credits_roll.md` for the full credits roll (canonical source of truth — the in-game credits scene is generated from this file).

---

*v1 · 2026-06-02 · Sprint 10 Lane 8 · `agent/release/moon1-ship-candidate`*
