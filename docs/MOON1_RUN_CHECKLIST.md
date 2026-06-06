# MOON 1 — MASTER RUN CHECKLIST

> **Single source of truth** for "is Moon 1 ready to ship?"
> Tracks **(a)** disk-side lockdown items + **(b)** §16 runtime artifacts per the HONEST RESET rule.
> Both (a) **and** (b) must be ✅ before any "Moon 1 GATE 1 done" claim.
>
> Origin docs: `docs/15_MVP_BUILD_SPEC.md §16`, `docs/MOON1_GAP_REPORT_2026-06-04.md`, `CLAUDE.md` HONEST RESET section.
>
> Last updated: 2026-06-04 (LATEST HAMMER session post LANE A–F + DEBUG-1 + RUNCHK-1).

---

## A. DISK-SIDE LOCKDOWN — verified by static grep + runtime probe

### A1. Project-level Unity settings (verified 2026-06-04)

| # | Setting | Required | Current | Status |
|---|---|---|---|---|
| A1.1 | `PlayerSettings.colorSpace` | Linear | Linear | ✅ |
| A1.2 | `GraphicsSettings.defaultRenderPipeline` | TartariaURP | TartariaURP | ✅ |
| A1.3 | `PlayerSettings.runInBackground` | True | True | ✅ (set this session) |
| A1.4 | Input System Package active | Yes | Keyboard.current+Gamepad.current resolve | ✅ |
| A1.5 | `Application.runInBackground` | True (project) | True | ✅ (set this session) |
| A1.6 | Build Scenes count | 15 (Boot + Echohaven + 13 Moons + UI_Overlay) | 15 | ✅ |
| A1.7 | NavMesh baked in Echohaven | >0 triangles | 126 triangles | ✅ |
| A1.8 | Tags present: Player, Building, Enemy, NPC, Interactable, Pickup | All 6 | 6 (Interactable+Pickup added this session) | ✅ |
| A1.9 | Layers: Player=10, Enemy=12, Building=8, Interactable=9 | All present | All present | ✅ |
| A1.10 | `Time.fixedDeltaTime` | 0.02 (50Hz) or 0.0166 (60Hz) | 0.02 | ✅ |
| A1.11 | Static batching on terrain + buildings | 20+ static flags | 24 transforms marked static | ✅ (this session) |
| A1.12 | Exactly 1 enabled `AudioListener` in scene | 1 | 1 on Player.prefab | ✅ (added this session) |
| A1.13 | URP shader stripping flags set | StripUnusedVariants ON | UNSET (default behavior) | ⚠️ |
| A1.14 | Quality tier count | 1–3 | 6 (Very Low → Ultra) | ⚠️ |

### A2. Player + characters (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A2.1 | Player.prefab has CharacterController + PlayerInputHandler | Yes | ✅ |
| A2.2 | Player.prefab has Animator with `AC_KayKit_Medium` controller | Yes | ✅ |
| A2.3 | Player.Animator.avatar = CassianCarterAvatar (Humanoid) | Yes | ✅ (wired this session) |
| A2.4 | Player.prefab has `inputActions = TartariaInputActions.inputactions` | Yes | ✅ (wired this session) |
| A2.5 | Player visual: Cassian nested under `_CharacterVisual/PlayerVisual_Cassian` | Renderer present | ✅ |
| A2.6 | All 4 NPC FBXs have Humanoid Avatar sub-assets | 4/4 | 4/4 (Bob fixed this session) |
| A2.7 | All 4 NPC prefabs have AC_KayKit_Medium controller + Avatar wired | 4/4 | ✅ |
| A2.8 | `BlenderImportPostprocessor.NPC_FILENAMES` includes BobInnkeeper.fbx | Yes | ✅ (added this session) |

### A3. Scene + placement (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A3.1 | Exactly 1 active Main Camera | 1 | 1 (orphan deactivated this session) |
| A3.2 | `_SpawnPlatform` BoxCollider `isTrigger=true` | Trigger | ✅ (fixed this session) |
| A3.3 | Cathedral_Facade Y position (40% buried, mesh ~28m) | ~-10 | -10 ✅ (fixed this session) |
| A3.4 | StarDome Y position (30% buried, mesh ~25m) | ~-7.5 | -7.5 ✅ (fixed this session) |
| A3.5 | CrystalSpire Y position (25% buried) | ~-5 | -5 ✅ (fixed this session) |
| A3.6 | 9 village buildings scale = (1,1,1) | All 1.0 | Apothecary 0.626→1.0, 3 Cottages 0.798→1.0, Inn 0.995→1.0 ✅ (fixed this session) |
| A3.7 | 10 village building prefabs rotation = identity | All identity | ✅ (prior session) |
| A3.8 | BobsInn scene-level localScale = (1,1,1) | (1,1,1) | ✅ (closed AUTO-3) |
| A3.9 | PlayerSpawner location | sensible (~0, 2, 15) | (0, 2, 15) ✅ |
| A3.10 | No duplicate Apothecary | 1 | 1 (VillageApothecary) ✅ |
| A3.11 | NavMesh covers village walkable area | yes | 126 triangles ✅ |

### A4. Materials + textures (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A4.1 | All Moon 1 materials URP/Lit | All | 54 URP/Lit materials ✅ |
| A4.2 | Zero `Hidden/InternalErrorShader` references in assets | 0 | 0 ✅ |
| A4.3 | Zero null material slots (`m_Materials: - {fileID: 0}`) in Moon1 prefabs | 0 | 0 ✅ |
| A4.4 | M_Mud_Fresh fallback material exists | Yes | ✅ |
| A4.5 | NPC skin materials embedded in FBX (auto-extracted) | Yes | All 4 NPCs ✅ |

### A5. Music + audio (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A5.1 | 4 adaptive music layers at 60s each in Resources | 4 stems | layer1+2+3+4 all 60.0s ✅ |
| A5.2 | AdaptiveMusicController 4-layer mix resolves | Yes | ✅ |
| A5.3 | `Resources/Audio/SFX/discovery_chime` loads | Yes | ✅ |
| A5.4 | `Resources/VO/Placeholder/` directory exists | Yes | ✅ (created this session) |
| A5.5 | `Resources/HovlVFX/` directory exists | Yes | ✅ (created this session) |

### A6. Combat + enemies (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A6.1 | MudGolem.prefab has real Blender mesh + tag=Enemy + 2.5m collider | Yes | ✅ |
| A6.2 | MudGolem has MudGolemAI + MudGolemHealth + MudGolemLootDrop | All 3 | ✅ |
| A6.3 | MudGolem has NavMeshAgent | Yes | ✅ |
| A6.4 | ResetScout.fbx is real binary (not LFS pointer) | Yes | ✅ |
| A6.5 | All FBXs are real binaries (351 total, 0 LFS pointers) | 0 stubs | ✅ |

### A7. Code quality (verified 2026-06-04)

| # | Item | Required | Status |
|---|---|---|---|
| A7.1 | Compile clean (0 errors) | 0 | 0 ✅ |
| A7.2 | Zero silent-fail empty catches in Moon 1 happy path | 0 | 0 ✅ |
| A7.3 | All `Resources.Load` paths resolve on disk | 100% | 14/14 spot-checked ✅ |
| A7.4 | InteractableBuilding.cs:804 MaterialPropertyBlock NRE | Not present | Fixed ✅ (this session) |

---

## B. RUNTIME ARTIFACTS — required for §16 GATE 1 (THE HARD GATE)

**Per the 2026-06-03 NIGHT MANDATE + 2026-06-04 HONEST RESET:** NATRIX-driven Unity Play session with OBS recording. Without these, "Moon 1 done" is NOT a valid claim regardless of A1–A7 ✅.

| # | Criterion (docs/15 §16) | Measurement | Status |
|---|---|---|---|
| §16.1 | 15-minute play session complete | Documented playtest video | ❌ NOT PRODUCED |
| §16.2 | 60 FPS on Recommended PC (sustained) | Unity Profiler, 30-min session | ❌ NO RUN |
| §16.3 | 30 FPS on Minimum PC (sustained) | Same | ❌ NO RUN |
| §16.4 | Memory ≤ 4 GB after 30 min | Profiler reading | ❌ NO RUN |
| §16.5 | Aether field visible, flowing, RS-responsive | Qualitative + visual test | ⚠️ greppable ✅, runtime ⏳ |
| §16.6 | 3 buildings restorable with tuning mini-games | Functional test | ⚠️ disk ✅, runtime ⏳ |
| §16.7 | Mud dissolution shader working | Visual quality assessment | ⚠️ disk ✅, runtime ⏳ |
| §16.8 | Milo functional (follow, speak, hide) | Behavior tree test matrix | ⚠️ disk ✅, runtime ⏳ |
| §16.9 | 1 enemy type engageable and defeatable | Combat flow test | ⚠️ disk ✅, runtime ⏳ |
| §16.10 | Gamepad haptics working | Haptic review with F310/DualSense | ⚠️ greppable ✅, runtime ⏳ |
| §16.11 | Adaptive music responds to RS changes | Audio review | ⚠️ 4 stems ✅, runtime ⏳ |
| §16.12 | No crashes in 1-hour stress test | Automated + manual test | ❌ NO RUN |

**Subjective gate (after 1–12 pass):** Core team plays the 15-minute demo and answers *"Do I want to keep playing?"* If not unanimously yes → iterate before Phase 2.

---

## C. OPEN ANIMATIONS GAP (Stage C — known follow-up)

Avatars wired ✅. Animator clips NOT bound — `AC_KayKit_Medium` has only one Locomotion BlendTree state. No Attack / Hit / Die / Talk / Idle states.

**Consequence:** Characters move-blend correctly when walking, but cannot trigger combat / dialogue / interact animations. NPCs will stand idle in their default pose.

**Fix scope:** ~4 hours to author Attack, Hit, Die, Talk states + assign clips per-NPC.

---

## D. OPEN POLISH GAPS (low priority, non-blocking)

| Gap | Severity | Effort |
|---|---|---|
| 6 quality tiers should collapse to 2 (URP best practice) | 🟡 perf | 10 min |
| URP shader stripping flags unset (build size + variant count unconstrained) | 🟡 perf | 5 min |
| TownHall (0,0,50) crowds VillageApothecary (15,0,50) — 15m gap | 🟢 cosmetic | 5 min |
| Crystal_0/1/2 each ×3, VillageWell ×2, VillagerSignpost ×2 — possible duplicates | 🟢 cosmetic | 10 min |
| NPC FBX file sizes ~50% of CLAUDE.md Stage B claims — possible silent regression | 🟡 verify | 30 min |
| Char_Knight vendor LFS pull OR `gen_player_hero.py` | 🟢 mitigated (Cassian as Player works) | 1-2 hr |

---

## E. HOW TO USE THIS CHECKLIST

1. **Before claiming Moon 1 done:** every row in §A1–A7 must be ✅. ANY ❌ row means do not claim done.
2. **Before claiming Moon 1 GATE 1 done:** every row in §B must be ✅ (NATRIX-driven Play session required for §16.1–4, §16.12; runtime verification required for §16.5–11).
3. **For visual / playtest debugging:** use `Tartaria/9 Debug/Runtime State Probe 2026-06-04` (writes `RUNTIME_PROBE_2026_06_04.txt`) and `Tartaria/9 Debug/Attach Input Overlay 2026-06-04`.
4. **When something breaks:** read this doc, find the row, check the "Status" cell. If was ✅ and now isn't, that's a regression — fix it.
5. **When adding new items:** if the item is disk-verifiable, add to §A. If runtime-only, add to §B.

---

*MOON1_RUN_CHECKLIST.md v1.0 · 2026-06-04 · The single canonical run checklist.*
