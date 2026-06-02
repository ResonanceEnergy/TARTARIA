# MOON 1 SHIP CHECKLIST — Alpha 0.4 Pre-Tag

> Pre-tag verification gate NATRIX runs through before invoking `scripts/release/tag-moon1-ship-candidate.ps1`. Companion to `docs/audits/MOON1_ACCEPTANCE_2026-06-02_v4.md` (the audit declared the build SHIPPABLE pending this checklist).

**Branch under verification:** `feature/consolidate-moon-architecture`
**Target tag:** `moon1-ship-candidate-alpha-0.4` (set in tag script)
**Tag-day operator:** NATRIX
**Companion audit:** `docs/audits/MOON1_ACCEPTANCE_2026-06-02_v4.md`

---

## Stage 0 — Pre-flight (trunk-merge round, before opening Unity)

Per v4 audit §SHIP-GATE: all the ✓-scored work in v3/v4 is **branch-only** on origin. Trunk must absorb the 10 lanes below before this checklist means anything.

```
[ ] Merge agent/integration/sprint9-feature-merge (2ea11442) into feature/consolidate-moon-architecture
[ ] Merge agent/tools/butler-upload (38925669)
[ ] Merge agent/content/npc-fbx-render (01e0034d)
[ ] Merge agent/content/npc-prefab-rebind (566ebdaf)
[ ] Merge agent/gameplay/brazier-ritual (963fc750)
[ ] Merge agent/content/named-villagers (0eeceeea)
[ ] Merge agent/fix/findobjecttype-sweep (8a55527b)
[ ] Merge agent/fix/lightmap-editor-settings-sweep (bf939df7)
[ ] Merge agent/fix/difficulty-sapplied-guard (97ebbf3c)
[ ] Merge agent/content/npc-fbx-import-config (af241b97)
[ ] (Skip agent/fix/inputprobehud-warn — empty no-op)
[ ] (Skip agent/fix/saveslot-triage — deliberate no-op confirm)
[ ] git push origin feature/consolidate-moon-architecture
```

Estimated time: 1–2 hours review + conflict resolution. Likely conflict zone: `Assets/_Project/Scripts/Core/GameEvents.cs` (Sprint 9 L7 adds OnBrazierLit / OnBrazierRingComplete; integration branch may already have the slot).

---

## Stage 1 — Unity compile-clean verification

```
[ ] git switch feature/consolidate-moon-architecture
[ ] git pull
[ ] Open Unity Hub → open TARTARIA_new project
[ ] Wait for "Reloading Domain..." to finish
[ ] Open Console window (Ctrl+Shift+C)
[ ] CONFIRM: 0 compile errors
[ ] CONFIRM: Warnings ≤ 2 (NavMeshBuilder deprecation is known acceptable)
[ ] CONFIRM: "Error Pause" toolbar toggle is OFF (top of Console, left of filter dropdown)
[ ] CONFIRM: not in Safe Mode (no orange banner at top of Editor)
```

If any compile error: STOP. Surface to a Sprint 11 hotfix lane before tagging.

---

## Stage 2 — Content rebind (Cowork-side Editor clicks)

Per v4 audit §6.7: S9 L5 + S9 L6 + S10 L6 deliver FBX binaries + import config + rebind menu on origin. The scene file still references the primitive prefabs until you click these:

```
[ ] Tartaria → Content → Reimport Moon 1 NPC FBX (forces Generic rig per S10 L6 BlenderImportPostprocessor)
[ ] (BlenderImportPostprocessor auto-generates prefab variants under Assets/_Project/Prefabs/Moon1/Blender/)
[ ] Tartaria → Content → Rebind Moon 1 NPC Prefabs (idempotent — re-points scene NPCs to new Blender prefab variants)
[ ] Open Assets/_Project/Scenes/Echohaven_VerticalSlice.unity
[ ] Visually confirm Lirael / Anastasia / Cassian / Milo render as the new FBX meshes (not primitives)
[ ] File → Save (Ctrl+S)
[ ] git add Assets/_Project/Scenes/Echohaven_VerticalSlice.unity
[ ] git commit -m "Moon 1 ship-prep: rebind NPC prefabs to Blender FBX variants"
[ ] git push origin feature/consolidate-moon-architecture
```

---

## Stage 3 — Main-menu boot smoke

```
[ ] Open Assets/_Project/Scenes/MainMenu.unity (or whichever scene is set as the build's first scene)
[ ] Hit Play (Ctrl+P)
[ ] CONFIRM: main menu UI renders cleanly (no missing-sprite squares, no console errors)
[ ] CONFIRM: New Game / Continue / Options / Quit buttons all clickable
[ ] CONFIRM: difficulty selector visible
[ ] Stop Play
```

---

## Stage 4 — New-Game end-to-end loop (the gold path)

The 20-minute Moon 1 alpha experience. Run this once, all the way through, without leaving Play.

```
[ ] Open Assets/_Project/Scenes/Echohaven_VerticalSlice.unity (or boot via MainMenu → New Game)
[ ] Hit Play
[ ] CONFIRM: Player spawns at (0, 2, -10) facing north, no magenta materials
[ ] CONFIRM: InputProbeHUD overlay (top-left) shows Keyboard.current OK and Gamepad.current OK (F310 in X-mode)
[ ] WASD / left-stick → Player walks
[ ] Walk toward nearest Brazier (per S9 L7)
[ ] CONFIRM: E-prompt "Light brazier" appears
[ ] Press E → flame ignites + audio cue
[ ] Light ring of braziers → CONFIRM OnBrazierRingComplete event triggers (look for "[BrazierRitual] Ring complete" in console)
[ ] Walk to TuningPedestal_0 (Crystal Spire node 1)
[ ] CONFIRM: E-prompt "Press [E] to tune (FrequencySlider)" appears (Variant A — per S9 L1 per-node rule)
[ ] Press E → Variant A slider UI renders → complete tuning
[ ] Walk to TuningPedestal_3 (Star Dome node 1) → CONFIRM Variant A
[ ] Walk to TuningPedestal_4 (Star Dome node 2) → CONFIRM Variant B or C deterministic per-node
[ ] Complete all 3 nodes on Crystal Spire → building rises from mud → restoration ceremony plays
[ ] Repeat for Star Dome (3 nodes)
[ ] Repeat for Stone Fountain (3 nodes)
[ ] CONFIRM: 3 hero buildings restorable end-to-end
[ ] Pause menu (Start button / Esc) → Save → CONFIRM save slot UI renders (per consolidated SaveSlotPanel S10 L5)
[ ] Save to Slot 1
[ ] Quit Play
[ ] Re-enter Play → Continue → Load Slot 1
[ ] CONFIRM: state restored (3 buildings still up, RS count preserved, player position OK)
[ ] Pause → Quit cleanly to main menu
[ ] Pause → Quit to desktop
```

If any step fails: STOP. Document in Sprint 11 punch-list, do not tag.

---

## Stage 5 — F310 controller verification

```
[ ] Connect F310 (X-mode switch on back) before hitting Play
[ ] InputProbeHUD shows Gamepad.current (XInput): OK (Xbox Controller)
[ ] Left stick → Player walks (camera-relative)
[ ] Right stick → camera orbits
[ ] A (south) → Interact prompt fires
[ ] Y (north) → Aether Vision toggles
[ ] LB → sprint held
[ ] Start → pause menu opens
[ ] R3 click → camera recenters
```

---

## Stage 6 — Tag the ship candidate

Once Stages 0–5 pass without escalations:

```
[ ] cd C:\dev\TARTARIA_new (or your trunk worktree)
[ ] git switch feature/consolidate-moon-architecture
[ ] git pull
[ ] git log --oneline -5  (sanity check — should show all 10 Sprint 9/10 merges)
[ ] pwsh scripts/release/tag-moon1-ship-candidate.ps1
[ ] git push origin --tags
[ ] git ls-remote --tags origin  (verify the tag is on origin)
```

---

## Stage 7 — (Optional) butler dry-run to itch.io draft channel

For NATRIX's own pre-flight before going public. Does NOT publish — pushes to a draft channel only.

Per `docs/release/BUTLER_SETUP.md` (Sprint 9 L4) — install butler + log in:

```
[ ] butler login (one-time)
[ ] butler status <user>/<game>:moon1-draft
[ ] pwsh scripts/release/build-itch.ps1 -Channel "moon1-draft" -SkipPublish:$false
[ ] (Or with -DryRun if you want to test the orchestrator without pushing)
[ ] itch.io dashboard → confirm draft build appears
[ ] Download draft build → run smoke test as if you were a player
[ ] Promote draft → public when satisfied
```

---

## v4 audit punch-list — known deferrals at tag time

NATRIX accepts these into Sprint 11 backlog by tagging without them:

1. **Lirael Day-25 reveal gate** (v4 §6.3) — S10 L7 stalled with uncommitted code on disk in `_wt_s10_l7_day25_smoke`. Nice-to-have, not gold-path.
2. **CymaticEngine class shim or spec rename** (v4 §9.4) — doc-decision item, no gameplay impact.
3. **InputProbeHUD CS-warn** (v4 §16.3) — cosmetic, S10 L3 stalled.
4. **Vegetation density** (v4 §4.1) — 120 instances vs spec ~5k. Aesthetically thin but doesn't block traversal.
5. **Milo HIDE/CELEBRATE state machine** (v4 §6.6) — Milo follows but doesn't switch animation states by quest beat.
6. **Carved Stone POI** (v4 §3.2 / §3.5) — DLC tease prop not yet placed.
7. **40-voice-line distribution** (v4 §6.5) — spec depth not met.
8. **Cinemachine cinematic on Lirael lullaby** (v4 §8.2) — current reveal is camera-cut, not cinemachine pan.
9. **`IsMoon1Step` tutorial filter** (v4 §12.5).
10. **PauseMenu vs PauseAndGameOverMenu reconciliation** (v4 §13.2).
11. **MudGolemAI speed/damage verification vs spec §11** (v4 §7.2).
12. **displayName literals for "Thread of Memory" / "First Note"** (v4 §1.9).

---

*SHIP_CHECKLIST.md — Sprint 10 Lane 10 companion to MOON1_ACCEPTANCE_2026-06-02_v4.md*
*Date: 2026-06-02 · Author: Claude (Opus 4.7 1M) · For NATRIX execution*
