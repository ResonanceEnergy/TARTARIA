# 33 — Vertical Slice Script (5 min Moon 1)

**Status**: LOCKED scope. Nothing outside this list ships in the slice.
**Author**: Vex / 2026-04-29.
**Replaces**: any earlier "vertical slice" notes scattered across docs 15 / 16 / 26.

---

## Pitch (one sentence)

> Elara wakes in Echohaven's ruined plaza, restores the Star Dome with the Resonance Tool, and accepts the first quest.

That's it. No combat. No inventory UI. No save. No second moon. No NPC dialogue tree branching. **Just the loop above, polished, in 5 minutes of player time.**

---

## Beat Sheet (target 5:00 of player time)

| t | Beat | Player action | System state |
|---|---|---|---|
| 0:00 | **COLD OPEN** — fade from black, ambient drone fades in | none | `GameState.Boot` → `Exploration` |
| 0:10 | **SPAWN** — capsule Elara stands in plaza, Star Dome 20m north | look around | camera Cinemachine free-look armed |
| 0:30 | **PROMPT 1** — "WASD to move" tutorial overlay | walks forward | TutorialOverlay shows once, dismisses on first WASD |
| 1:00 | **DISCOVERY** — approaches Star Dome, `IInteractable.Discover()` fires, glow pulse VFX, audio chime | walks within 5m | building marked Discovered in QuestData |
| 1:30 | **PROMPT 2** — "[E] to interact" overlay | press E | InteractionPrompt UI hides on press |
| 1:35 | **DIALOGUE** — DialogueManager opens: 4 lines from Anastasia (off-screen voice for now) | reads, presses Space to advance | GameState briefly `Tuning` for dialogue lock |
| 2:30 | **TASK** — final dialogue line: "Restore the Dome. Channel the resonance." | dialogue closes | quest "AwakenStarDome" set Active |
| 2:35 | **CHANNEL** — hold E for 3s near Dome, ScanPulse VFX builds | hold E | RestoreSparkle VFX plays on Dome |
| 3:30 | **CLIMAX** — Dome rotates from grey → AetherVein gold, DomeAwakeningBurst fires, music swell | watches | quest marked Complete, Dome material swap |
| 4:30 | **HOOK** — Anastasia VO: "The Fountain calls next..." — fountain marker pulses on horizon | optional walk toward fountain | quest "AwakenFountain" set Available (not Active) |
| 5:00 | **FADE** — gentle fade to title card "TARTARIA — Demo Build" + URL | end | log play session, reset scene on Enter |

Total runtime: **5 minutes if you stand still and watch**, can complete in 3 if you rush.

---

## Acceptance Criteria (must all be true to ship)

1. ☐ Boots into `Echohaven_VerticalSlice.unity` from clean editor in <10s.
2. ☐ Capsule Elara visible at spawn, Capoeira ginga loop running on idle.
3. ☐ WASD + look + jump all responsive (existing `PlayerInputHandler`).
4. ☐ Star Dome `Discover()` fires on proximity (existing `BuildingSpawner.WireBuilding`).
5. ☐ Pressing E within range opens 4-line dialogue (existing `DialogueManager` + new asset `Dialogue_Anastasia_AwakenStarDome.asset`).
6. ☐ "Hold E" channeling triggers `RestoreSparkle` VFX (existing).
7. ☐ Dome material swaps grey → `M_AetherVein` on completion (existing shader).
8. ☐ Quest state persists in `GameStateManager` (existing).
9. ☐ Music ambient bed audible throughout (existing `AudioManager` + 1 looping track).
10. ☐ Fade-out + title card after climax (NEW: tiny `EndCardController` MonoBehaviour).

---

## What's NEW vs already built

ONLY these are new code/assets:
- `Dialogue_Anastasia_AwakenStarDome.asset` (4 lines, ScriptableObject — schema already exists)
- `EndCardController.cs` (~40 lines, fades a CanvasGroup, shows title card)
- `Quest_AwakenStarDome.asset` (existing schema)
- 1 ambient music track from Pixabay (CC0, ≤5MB, single file — no Sonniss bundle, no LFS pressure)

**That's it.** No mixer. No cue library. No visual profile. No mesh swap. No new shaders. No second scene. No save. No pause menu. No settings. No accessibility pass. No localization.

---

## Explicit out-of-scope (NOT in slice — defer)

- Inventory UI (system exists, no UI in slice)
- Combat (no enemies in slice)
- Save/load (no save points in slice)
- Day/night cycle
- NPC wandering
- Settings menu
- AudioMixer + cue library (single AudioSource is fine for 5 min)
- CharacterVisualProfile / mesh swap (capsule is canon for the slice)
- Mixamo female humanoid (defer until after slice ships)
- Quaternius / KayKit kits (defer until after slice ships — also avoids LFS bloat)
- Light baking (realtime is fine for 1 scene at 5 min)
- Decals / footprints
- Animation Rigging / IK
- 32_ART_BIBLE.md polish — already authored, do not re-edit

---

## Build pipeline impact

- Phase 9k stays disabled.
- No new build phases.
- BatchReadinessValidator gets ONE new check: `Slice_HasEndCard` (verifies CanvasGroup + title card text in scene).

---

## Definition of "Shipped"

- Builds GREEN via `.\tartaria-play.ps1 -BatchOnly`.
- Plays end-to-end with no NullRef in Editor.log.
- Recorded 5-min capture exists at `docs/captures/slice_v1.mp4` (manual OBS, not pipeline).
- Tagged `slice-v1` in git, pushed to `main`.

When all four are true, slice is shipped. Then — and only then — pick the next item from doc 31 §8.
