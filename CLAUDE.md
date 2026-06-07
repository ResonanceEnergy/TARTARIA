# CLAUDE.md — TARTARIA Operating Manual

> Read this first, every session, before any tool call. This file replaces the 9 layered mandates from before 2026-06-05. Historical originals are archived under `docs/_archive_pre_2026_06_05/`.

---

## 🎯 2026-06-07 LATEST PROGRESS (R71-R75)

**Moon 1 visual density shipping fast.** Real walk-throughs + 24 screenshots inline this session.

| Round | Win |
|---|---|
| R66 | CrystalSpire Blender FBX baked (cube+shards), scene mesh swap, 8 renderers + blue emissive |
| R67 | Mercury-Ball Spire landmark (Day 19-24 Buried Beacon) placed @ (45, 0, 25), obsidian + mercury orb + 3 satellites |
| R68 | 3-6-9 Lore Stone (Day 1-5 prophecy fragment) menhir with carved golden glyph rings near spawn |
| R71 | HUDController public API — added `SetRSCount(int)` + `SetAetherPercent(float)` + real `UpdateRS()` impl |
| R72 | 9 village buildings authored as scene children (Inn, Bakery, Cottage A/B/C, Mill, Smithy, TownHall, Watchtower, Apothecary) |
| R74 | Scale + ground fixes — village 0.18→0.9, CrystalSpire 2x, LoreStone 2x. All Y=0 ground-locked. |
| R75 | Real **VillageHouse FBX** — cube body + pyramid roof + glowing windows + door + chimney + foundation. 9/9 cottage swap. Unique color per building. |

**Audit swarm carry-forward gaps (still pending fill):**
- 1,251 KayKit FBXs are LFS pointer stubs (97.7% unpulled) — `git lfs pull` required
- 8 fragile `GameObject.Find()` calls in `EchohavenContentSpawner.cs` — replace with cached refs
- MudGolem prefab 4-way duplicate — only `Resources/Enemies/MudGolem.prefab` has combat MBs
- TownHall stubborn pyramid placeholder (DestroyImmediate edge case, nested unreachable child)
- AnastasiaRocker.prefab missing — bake menu unfired
- 105 `.cs.disabled` Editor scripts cleanup
- 11 silent catches outside Moon 1 happy path (top 5 fixed C.L2)

**Doc references:**
- `STATUS.md` — full R71-R75 session log
- `Logs/R59*-R75*.png` — 24 visual proof screenshots
- `Tools/blender/gen_*.py` — 5 new Blender bake scripts this session

---

## 1. PROJECT IDENTITY

- **Project:** TARTARIA WORLD OF WONDER — Aether Awakening
- **Owner:** NATRIX (nate@gripandripphdd.com)
- **Engine:** Unity 6.3.6f1 LTS, URP, single-player PC.
- **Genre:** RPG + restoration + city-builder hybrid across 13 in-game Moons + Day Out of Time.
- **Scope:** Full game ships when all 13 Moons pass their 8-step smoke test and §16 GATE criteria. **No release framing (itch.io / Win64 / Steam) until then.**
- **Current Moon in flight:** Moon 1 (Echohaven). See `STATUS.md` for live state.

---

## 2. THE 8-STEP SMOKE TEST — the only valid verification

A Moon is "GATE-clean" when this 8-step loop runs end-to-end without error, once per session, ideally recorded.

1. **Click Play** → 0 console errors, scene loads.
2. **Player visible** at spawn — no magenta, no T-pose, no clipping below ground.
3. **Movement works** — WASD or F310 left-stick walks the player.
4. **Camera follows** — third-person, smooth, no orbit drift, no overlap with geometry.
5. **Reach a Moon-canonical interactable** (e.g. Moon 1 = brazier or pedestal) — walk distance ≤30 m.
6. **Press E / A** — interaction UI appears.
7. **Complete the interaction** — mini-game succeeds, state changes, VFX/audio fires.
8. **HUD updates** — quest tracker, RS counter, day cycle, or whatever the Moon's loop tracks.

If a step fails: that step is the ONLY thing this session works on. Fix the smallest existing file that owns that step. Re-run the test. Stop after the first green pass.

This replaces the prior "no stop-and-test" mandate that produced 5+ sessions of false-98% loops.

---

## 3. TOOL-CALL DISCIPLINE GATE

Before every tool call, answer these 5 questions:

1. **Does this move the 8-step smoke test forward for the current Moon?** If no, justify why it's a hard prerequisite.
2. **Am I about to CREATE a new file?** The default answer is NO. If yes, name the existing file this duplicates and explain why a 5-line edit to that file is insufficient.
3. **Am I patching a SYMPTOM?** If the fix is at the runtime layer when the defect is at the import/scene/prefab YAML layer, STOP — fix the root.
4. **Have I touched this surface 3+ times this session?** If yes, walk away and read the spec, don't keep editing.
5. **Could a 5-line edit to an existing file replace a 50-line new script?** Almost always yes. Take the 5-line edit.

If any of #2-5 fail, do not make the call. Pick a different action.

---

## 4. PATTERNS WE FOLLOW (from Unity 6 manual)

| Subsystem | Canonical pattern | Reference |
|---|---|---|
| Rendering | URP (TartariaURP.asset), Linear color space | Unity Manual → URP Settings |
| Input | Input System Package + InputActionAsset bound to PlayerInput on prefab. Direct `Keyboard.current` polling = fallback only. | Unity Manual → Input System → Background behavior |
| Characters | FBX imported with Animation Type = Humanoid + Skin Weights = Standard (4 bones). Avatar auto-generated. Prefab Variants for skin swaps. | Unity Manual → Rigging → Avatar |
| AI / NavMesh | NavMeshAgent + baked NavMesh in scene. Mud Golem, Reset Scout = the canonical examples. | Unity Manual → Navigation |
| Camera | Cinemachine 3 (installed in `Packages/manifest.json` but currently unused). The custom `CameraController.cs` should migrate to a CinemachineCamera, OR Cinemachine should be removed. Open decision per Moon 2. | Unity Manual → Cinemachine 3 |
| Audio | One enabled `AudioListener` per scene (on Player.prefab). Music = 4-layer adaptive via `AdaptiveMusicController`. | Unity Manual → Audio |
| Save | `Application.persistentDataPath` + `JsonUtility` + atomic write via `File.Replace`. | Unity Manual → Persistent data |
| Static content | Compose in scene YAML, not runtime `new GameObject`. Mark immovable env Static (Batching + GI + Occluder). | Unity Manual → Static GameObjects |
| Dynamic content | Pool, don't allocate. Resources.Load is the Moon 1 ship pattern; Addressables is a Moon 5+ prerequisite. | Unity Manual → Asset workflow |

---

## 5. PROJECT STRUCTURE (where things live)

```
Assets/_Project/
├── Scripts/              # game code, 23 asmdefs
│   ├── Input/PlayerInputHandler.cs       — canonical player input (one file, no overrides/drivers)
│   ├── Integration/PlayerSpawner.cs      — canonical spawn
│   ├── Camera/CameraController.cs        — canonical 3rd-person camera
│   ├── Editor/                           — Editor-only tooling (menus, bake one-shots, postprocessors)
│   ├── AI/, Combat/, Gameplay/, UI/, ...
│   └── _archived_*/, *.disabled, *.archived  — DO NOT DELETE in bulk (see §7)
├── Scenes/
│   ├── Boot.unity, UI_Overlay.unity
│   ├── Echohaven_VerticalSlice.unity     — Moon 1 (current playable)
│   └── Moons/*.unity                     — Moons 2-13 shells (mostly empty)
├── Prefabs/
│   ├── Characters/                       — Player + 4 NPCs + Bob (gameplay wrappers)
│   └── Moon1/Buildings/, Moon1/Blender/  — Moon 1 buildings + Blender mesh sources
├── Resources/
│   ├── Enemies/MudGolem.prefab           — combat-ready (canonical, loaded via Resources.Load)
│   ├── Prefabs/UI/HUD_Root.prefab
│   └── Audio/Music/ambient_layer{1..4}.wav (60s each)
├── Models/Blender/Moon1/                 — 4 NPC FBXs + MudGolem + ResetScout (Stage B 23-bone armatures)
├── Materials/                            — 54 URP/Lit + 14 custom Tartaria shaders
└── Input/TartariaInputActions.inputactions

docs/
├── 15_MVP_BUILD_SPEC.md                  — Moon 1 spec (§1-15 content, §16 GATE 1 criteria)
├── 03_CAMPAIGN_13_MOONS.md               — Moon overview
├── 03C_MOON_MECHANICS_DETAILED.md        — per-Moon mechanics
├── MOON_BLUEPRINT.md                     — shared template for Moons 1-13
├── MOON1_RUN_CHECKLIST.md                — §A1-A7 disk + §B runtime gate
└── _archive_pre_2026_06_05/              — old foundation files preserved
```

---

## 6. COMMON TASKS

| Task | How |
|---|---|
| "Why doesn't X work?" | Run the 8-step smoke test. Find the failing step. Fix the file that owns that step. |
| "Player won't move" | Check `Application.isFocused` — Game view focus is the usual cause. Then read `Input/PlayerInputHandler.cs:519` (HandleMovementInput). Don't add Hard Move Drivers. |
| "Player is magenta" | `BlenderImportPostprocessor.cs` `skinWeights = Standard` + Quality Settings `m_BlendWeights: 4`. URP shader stripping in `TartariaURP.asset`. Single root cause: URP variant collection. |
| "Edit a scene" | Edit `.unity` YAML directly via Edit tool, or do it in the Editor and Save Scene. Never both in the same session — Unity overwrites. |
| "Need to bake something into a prefab" | Use `Tartaria/8 Fix/...` menu where available; if absent, edit prefab YAML directly. Don't create new bake one-shots. |
| "Want to clean up scripts" | Move to `_archive_*/` folder, never delete unless duplicate-byte-identical. |

---

## 7. QUARANTINED PATTERNS (DO NOT CREATE)

These produced 9 months of debt and 487 archived files. Do not author new:

- `Moon*Safety.cs`, `Moon*Fix.cs`, `Moon*Override.cs`, `Moon*Daemon.cs`, `Moon*Rescue.cs`, `Moon*GodMode*.cs`, `Moon*HardOverride*.cs`, `Moon*HardMoveDriver*.cs`
- `Debug_Input*.cs`, `*KeyPressLogger*.cs`, `*RuntimeStateProbe*.cs`, `*RuntimeInputOverlay*.cs` (we already have canonical `Input/InputProbeHUD.cs`)
- Any new `[RuntimeInitializeOnLoadMethod]` that mutates scene state
- Files named with a date suffix (`_2026_06_05.cs`) — these always become orphans
- Files prefixed `_STUBS_*`, `_MinimalStub*`, `_TempPatch*`

**Bulk deletion of `_archived_*/.disabled/.archived/.BEFORE_FIX` files is also quarantined** — those are 9 months of attempted-but-disabled work that may be needed for reference. Only delete files that are exact byte-duplicates of a canonical file.

Quarantine grep (run before session close):
```bash
grep -l "Moon.*Driver\|Moon.*GodMode\|Moon.*Hard.*Override\|Moon.*Safety\|Moon.*Rescue\|Moon.*Daemon\|Moon.*Lifeline" Assets/_Project/Scripts/
grep -l "Debug_Input\|KeyPress.*Logger\|RuntimeStateProbe\|RuntimeInputOverlay" Assets/_Project/Scripts/Editor/
```

---

## 8. WORKING STYLE WITH NATRIX

- NATRIX = owner / sole dev / creative director / producer. Treat the role as engineer + technical PM helping NATRIX execute, not advisor.
- NATRIX pays per token. Long preambles cost real money. Reply with action, not commentary.
- When NATRIX says "build" or "hammer" — execute, don't audit.
- When NATRIX says "audit" or "check" — audit, don't execute.
- When NATRIX is frustrated, the cause is usually that I'm circling. Re-read this doc, find the step the work belongs to, do that step, stop.
- NATRIX's typing has informal grammar and ellipses. Match the working tone — not over-formal, not over-cute.

---

## 9. SESSION-END CHECKLIST

Before sign-off:

- [ ] Quarantine grep returns 0 new hits for the date-suffixed pattern.
- [ ] No new `.cs` files this session OR I named the existing file each one supersedes.
- [ ] 8-step smoke test status updated in STATUS.md.
- [ ] Compile clean (`mcp__unity-tartaria__read_console` returns 0 errors).

If any are unchecked, the session is incomplete.

---

## 10. HISTORICAL CONTEXT (compressed)

Pre-2026-06-05 history: see `docs/_archive_pre_2026_06_05/` for the 9 layered mandates this single doc replaces. Key facts preserved:

- **2026-05-29 hygiene:** moved 217 .md files out of root into `docs/agent_reports/` + `docs/archive/`. Don't undo that.
- **GameEvents.cs reconciled** 2026-05-29 — was truncated, now whole; old backups archived.
- **Logitech F310 X-mode** is the canonical dev gamepad. Right-stick orbit + WASD bound via TartariaInputActions.inputactions.
- **Blender + Headless FBX pipeline** is the canonical art source. Scripts at `tools/blender/gen_*.py`. Auto-imports via `BlenderImportPostprocessor.cs`.
- **351 FBXs, 0 LFS pointer stubs** (after FIX-D), 108 textures, 195 materials, all URP/Lit / Linear color.
- **Game view focus is the recurring runtime gotcha** — when input feels broken, `Application.isFocused` is the first thing to check. `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView` helps but Unity Editor must still be the foreground OS app.

---

*CLAUDE.md v2.0 · 2026-06-05 · Update this doc when reality drifts from it. Replace, don't layer.*
