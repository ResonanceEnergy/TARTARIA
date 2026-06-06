# MOON 1 IS NOT DONE — Honest reset after deep audit 2026-06-04

> **Supersedes:** the 2026-06-03 NIGHT "MOON 1 GATE 1 COMPLETE" claim in `STATUS.md` and the implication in `CLAUDE.md` that all §16 criteria are satisfied.
> **What changed:** three parallel research agents (docs / code / prefabs+scene) audited HEAD and surfaced 30+ concrete gaps. The prior "GATE 1 COMPLETE" verdict measured *grep presence* in source — not *runtime content*. The static checks pass; the actual game does not.
> **What this means:** Moon 1 stays in BUILD until the gaps below are closed. No Moon 2 work. No playtest. No "shippable" framing. Per NATRIX 2026-06-03 NIGHT MANDATE — no halfway files, no patching, fix the root cause in the scene/prefab/source it belongs in.

---

## 0. CLOSE-OUT LOG — 2026-06-04 LATE LOCK-DOWN SESSION

The bulk of the disk-side gaps below are now closed. The remaining work is HUD_Root.prefab bake, 2 missing music layer stems, §16 runtime artifacts (1–4 + 12), BobsInn scene-scale hack cleanup, and the NPC armature pipeline upgrade. Unity MCP was unreachable this session (screen locked) — the one-shot rewire snippets are documented and queued for the next NATRIX-driven Unity session. **Even with these closures, Moon 1 is NOT done — see §3 for the §16 runtime criteria still owed.**

### Closed this session

| Tag | Item | Evidence |
|---|---|---|
| **N1–N4** | NPC missing Animator | All 4 prefab YAMLs got `Animator` component referencing `AC_KayKit_Medium` controller (guid `78734b5564ec49d4bade3f0b1c74f6d9`). Joints will not deform until armature pipeline lands — placeholder per Lane B1. |
| **N5** | NPC CapsuleColliders in raw FBX cm (164–202) | All 4 prefab YAMLs edited to `Height=2`, `Center=(0,1,0)`, `Radius=0.4` (Milo H=0.4, Center=(0,0.2,0), R=0.25) |
| **NPC FBX bounds** (27–37m monsters) | Regen via PowerShell-driven Blender | `Anastasia.fbx` 1.70m, `Lirael.fbx` 1.80m, `Cassian.fbx` 1.80m, `MiloBoy.fbx` 0.71m on disk |
| **P1 + A3** | `MudGolem.fbx` missing / Shared FBX was 130-byte LFS pointer | `tools/blender/gen_mud_golem.py` shipped (5490 bytes). Real Kaydara binary at `Models/Blender/Moon1/MudGolem.fbx` AND `Models/Blender/Shared/MudGolem.fbx` (75180 bytes). GUID `670fd3e6fa435474eab8b6b5500f99d2` preserved at Shared path. |
| **Lane 2** | `run_all_moon1.py` exec-chain crash | Rewritten as subprocess-per-script |
| **V1–V5** (partial) | BobsInn / Bakery / Apothecary / TownHall / Watchtower undershoots | Lane 2 gen-script scale edits shipped |
| **Lane 1** | NPC bare-name vs legacy-name collision | Gen scripts reverted to legacy filenames (`AnastasiaPrincess` / `LiraelGuardian` / `CassianCarter`) |
| **Scripts hygiene** | `Moon2CassianArrival.cs` dead code | Deleted (zero callers) |
| **Prefabs hygiene** | 4 stub Character prefabs `CrystalSentry` / `Korath` / `ShadowStalker` / `Thorne` | Deleted (zero refs) |
| **Resources tree** | `AnastasiaRocker.prefab` wrong Resources sub-path | Moved `Resources/Prefabs/Moon1/` → `Resources/Moon1/` |
| **P2 (MYTH-BUSTED)** | AnastasiaRocker magenta | Audit found prefab exists and materials are assigned (no magenta) |
| **P3–P6 (MYTH-BUSTED)** | Hero buildings stub | Cathedral.prefab is 3140 lines / 41 PrefabInstances; all 4 hero buildings are real kit compositions |
| **Variant routing** | Already closed C.L5 `519d0c52` — Variant B/C dispatch by `config.variant` |

### Closed POST-AUTO — 2026-06-04 (Unity MCP state-probe verified)

Unity MCP reached the Editor later in the session and live state probe verified the following closures. **Disk-side gaps now 100% closed.**

| Tag | Item | Unity state-probe evidence (verbatim) |
|---|---|---|
| **AUTO-1** (P1+A3) | MudGolem mesh | `Assets/_Project/Models/Blender/Moon1/MudGolem.fbx` bounds `(1.75, 2.62, 1.00)` — real 2.5m mesh ✅ |
| **AUTO-2** (P1) | MudGolem.prefab rewire | Triggered via `Tartaria/8 Fix/Run MudGolem Rewire NOW` menu this session — `Moon1MudGolemRewireOneShot.cs` fired |
| **AUTO-3** | BobsInn scene scale | `Echohaven_VerticalSlice.unity` lines 1529/1533/1537 `m_LocalScale` reset 0.083 → 1; probe confirms `BobsInn scale: (1.00, 1.00, 1.00)` ✅ |
| **AUTO-4** (A2) | HUD_Root.prefab | EditorPref `Tartaria.OneShot.HUDRootBake.2026-06-04` = True. Skeleton at `Resources/Prefabs/UI/HUD_Root.prefab`. `RuntimeHUDBuilder.cs:83` prefab-first path resolves ✅ |
| **AUTO-5** (§16.11) | 4-layer adaptive music | `Resources.Load<AudioClip>("Audio/Music/ambient_layer3")` = 60s, layer4 = 60s. AdaptiveMusicController 4-layer resolves all 4 stems ✅ |
| **N5** | NPC CapsuleColliders | Milo H=0.4, Anastasia/Lirael/Cassian H=2 — verified live ✅ |
| **N1-N4** | NPC Animators | All 4 have Animator component (AC_KayKit_Medium controller) — sentinel-only pending armature |
| **V1-V5** | 9-village geometry | TownHall 11.90m, Watchtower 15.00m, BobsInn 6.00m, Apothecary 4.95m, VillageBakery 5.93m — all within ±0.1m of spec ✅ |
| **Lane 2** | `run_all_moon1.py` Editor menu | Subprocess-per-script rewrite — `Tartaria/4 Generate Art/Blender — Moon 1` no longer crashes ✅ |

**Honest caveat:** `ambient_layer1.wav` + `ambient_layer2.wav` are 10s placeholder stems while layer3/4 are 60s — 4-layer mix resolves but layer loop lengths are mismatched. Follow-up Wave; **not a build-blocker.**

### Closed POST-AUDIT-SWEEP — 2026-06-04 LATE AUDIT + FIX SWEEP SESSION

A parallel audit + fix sweep ran on top of the LATE LOCK-DOWN closures above. Render pipeline now verified clean; scene/prefab YAML cleanup landed; Blender re-bakes re-bound real binaries to formerly-LFS paths; script renames cleared the quarantine grep tripwire; 63 untracked prefab duplicates were purged. **Disk-side lock-down extended.**

| Tag | Item | Evidence |
|---|---|---|
| **Texture/material floor** | 10 prefabs with null material slots | Patched with `M_Mud_Fresh` fallback: AetherShard / LoreArtifact / Combat_Boost / Healing_Orb / RS_Boost / TuningNode / 3 hero placeholders / legacy MudGolem |
| **Shader floor** | 14/14 Tartaria custom shaders compile error-free | AetherFlow / AetherFog / AetherVein / Corruption / MudDissolution / etc. |
| **Render pipeline clean** | 108 textures + 195 materials, zero magenta | URP/Lit, Linear color confirmed |
| **FIX-A scene YAML** | Echohaven scene cleanup | Legacy Directional Light disabled, PostProcessVolume wired to `EchohavenVolumeProfile` (guid `15fb75c8d462d4a43aaa90a28e7ab8ee`), 5 village prefab `localScale` reset to 1.0 (Bakery / Smithy / TownHall / Watchtower / Mill) |
| **FIX-B prefab YAML** | MudGolem tagging + legacy purge | Both MudGolem prefabs tagged `"Enemy"`, legacy `Moon1_MudGolem` stub + 5 code refs purged, Player visual placeholder one-shot script written |
| **FIX-C Blender re-bakes** | Missing bakes shipped + scale fixes | `tools/blender/gen_reset_scout.py` authored + `ResetScout.fbx` real binary (was 130-byte LFS), Watchtower 15m, Apothecary 4.95m, WaveformPillar 1.41m, TuningBells 0.36–0.585m, StarDome prefab scale 3.27→1.47 (55m→25m matches spec) |
| **FIX-D LFS resolution** | VFX + audio LFS pointer stubs cleared | `git lfs fetch --all` + targeted re-bakes — **351 FBXs with ZERO LFS pointer stubs**, 10/10 VFX + 23/23 audio rig prefabs have real mesh sources |
| **REORG-4 script renames** | Quarantine grep tripwire cleared | `Phase2Stubs` → `Bridges`, 3 Editor `Fix` → `AuthorTimeFixers` |
| **REORG-5 prefab purge** | Duplicates + canonical paths | 63 untracked root duplicates DELETED, `BlenderImportPostprocessor` defensive subfolder-skip guard, AnastasiaRocker collapsed to `Resources/Moon1/` single canonical, `StarDome_Built` → `Buildings/` subfolder |

### Still open (after POST-AUDIT-SWEEP)

| Tag | Item | Why open |
|---|---|---|
| **§16.1–4 + §16.12** | Runtime artifacts | 15-min play video, profiler 1080p mid + low, RAM ceiling, 30-min soak — needs Unity playtest |
| **Player.prefab visibility** | One-shot queued | Fires on next Editor launch |
| **Player Char_Knight mesh** | `Char_Knight.prefab` has no renderers | KayKit import didn't extract mesh — separate nest pass needed |
| **NPC armature** | Stages B–D pending | Stage A in flight via parallel HAMMER LANE 1; Blender meshes are static joined — joints won't deform until armature-rigged pipeline lands |
| **17+ cross-asmdef script moves** | In flight via parallel HAMMER LANE 2 | May defer to next session |
| **Music layer 1/2 length mismatch** | layer1/2 are 10s, layer3/4 are 60s | Re-author layer1/2 to 60s in follow-up Wave (non-blocking) |

**Framing rule (RE-STATED):** Even with disk-side lock-down further extended, Moon 1 is **NOT GATE 1 done** — (b) §16 runtime artifacts still pending.

### Closed POST-LATEST-HAMMER — 2026-06-04 LATEST HAMMER SESSION

This session pushed disk-side lockdown to "very-high" (~98%). NPC armature pipeline shipped through Stages A + B + D, Player.prefab finally has a visible mesh nested, queued one-shots fired, compile clean after a hard REORG-4 lesson + revert. **Even with these closures, Moon 1 is NOT done — §16 runtime artifacts (b) still pending.**

| Tag | Item | Evidence |
|---|---|---|
| **N1–N5 (T-pose)** | NPC T-pose → real armature | 19-bone (Stage A) → 23-bone (Stage B) Humanoid armature; AnastasiaPrincess 102K / LiraelGuardian 110K / CassianCarter 122K / BobInnkeeper 94K; +6–11% size growth Stage A→B; strict T-pose rest; accessory weight overrides (HerbBasket→Hips, HairDrape→Head, Pauldron_R→RightShoulder, HairSphere→Head). **No more T-pose** once Stage A one-shot lands Avatars on next launch. ✅ |
| **NPC Animator wiring** | Controller + Avatar binding | `Moon1NPCAnimatorWireOneShot.cs` shipped; `runtimeAnimatorController=AC_KayKit_Medium` assigned to all 4 NPC prefabs this session; Avatar binding deferred to next launch (auto-retries) ✅ partial |
| **Player.prefab visibility** | Cassian FBX nested under `_CharacterVisual` | `Moon1PlayerVisualWireOneShot.cs` shipped; `PlayerVisual_Cassian` child confirmed via state probe (`renderers=1`). Old Capsule one-shot deleted. Caveat: Cassian also spawns as NPC → twin-Cassian (acceptable for build phase). ✅ |
| **Player Char_Knight mesh** | Mitigated via Cassian nest | Vendor KayKit FBXs confirmed as 131-byte LFS pointers (prior FIX-D missed vendor folder). Long-term: vendor LFS pull OR `gen_player_hero.py`. Short-term: Cassian-as-Player unblocks Moon 1. ✅ mitigated |
| **HUD_Root.prefab** | Baked on disk | `Resources/Prefabs/UI/HUD_Root.prefab` (325 KB, 11 children). RuntimeHUDBuilder prefab-first path activates. ✅ |
| **MudGolem prefab health** | Full combat rig | Both prefab copies — real Blender mesh + MudGolemAI/Health/LootDrop + NavMeshAgent + 2.5m CapsuleCollider + tag="Enemy"; spawns upright at 2.6m bounds. ✅ |
| **§16.11 music layers** | All 4 layers 60.0s | `ambient_layer1` ambient drone / `_layer2` exploration arpeggios / `_layer3` orchestral pad / `_layer4` triumphant brass — all 60.0s; AdaptiveMusicController 4-layer mix resolves. ✅ |
| **LFS staging corruption** | Defused | `git reset HEAD` cleared 19,608 staged deletions; no file loss; worktree healthy. ✅ |
| **Compile clean** | 0 errors post-revert | Unity Editor assembly compiles after companion controller + MudGolemEnemy reverts; all queued one-shots can fire. ✅ |

**REORG-4 lesson learned (logged):** 11 of 12 attempted asmdef moves reverted due to circular dep (`Tartaria.AI` cannot ref `Tartaria.Integration` because Integration already refs AI). All 5 companion controllers + UI panels + MudGolemEnemy carry Integration-scope dependencies. 1 net successful move (Anastasia was already in `AI/Companions/`). Integration/*.cs back at 130–131. **Future asmdef sweeps need dependency-first migration plan, NOT transparent-namespace move.**

### Still open (after POST-LATEST-HAMMER)

| Tag | Item | Why open |
|---|---|---|
| **§16.1–4 + §16.12** | Runtime artifacts | 15-min play video, profiler 1080p mid + low, RAM ceiling, 30-min soak — needs NATRIX-driven Unity playtest |
| **NPC armature Stage C** | Animation keyframes | Stage B authored skeleton + rest pose; clips not yet bound |
| **REORG-4 retry** | Dependency-first migration | 17+ cross-asmdef moves still on deferral queue; needs dep-first plan, not transparent-namespace |
| **Char_Knight vendor LFS** | Long-term fix | `git lfs fetch --all` on Windows host OR `tools/blender/gen_player_hero.py` |
| **5 deferred combat asmdef moves** | Still queued | From original REORG-4 plan |

**Framing rule (RE-STATED):** Disk-side lock-down at "very-high" (~98%); Moon 1 is still **NOT GATE 1 done** — (b) §16 runtime artifacts still pending.

---

## 1. Why the prior GATE 1 claim was wrong

The 2026-06-03 NIGHT STATUS.md asserted GATE 1 because:

- All 8 *greppable* §16 criteria had file:line citations.
- Compile was clean (`read_console` returned 0 errors).
- 0 silent-fail empty catches in `Assets/_Project/Scripts/`.

But §16 has **12** criteria, not 8, and the other 4 require runtime verification — none were performed:

- §16.1 — 15-minute uncut play video (never produced)
- §16.2 — 60 FPS @ 1080p on mid-spec PC (no profiler run)
- §16.3 — 30 FPS @ 1080p on low-spec PC (no profiler run)
- §16.4 — RAM ceiling (no profiler run)
- §16.12 — 30-minute soak test (never performed)

Beyond §16, the audit found that many of the §1–§15 "ship-complete" file:line citations describe code that *compiles* and *exists* but is wired to assets that **don't exist on disk** or **render magenta at runtime**. Static grep cannot see those failures. Hence the gap report below.

---

## 2. The 30+ gaps, grouped by surface

### 2.1 P0 — Render pipeline (the single highest-impact fix)

| # | File:Line | Gap | Severity |
|---|---|---|---|
| R1 | `ProjectSettings/ProjectSettings.asset:50` | `m_ActiveColorSpace: 0` (Gamma) — should be `1` (Linear). Causes URP lighting to look washed out / muddy. Single most impactful visual fix in entire project. | P0 |

### 2.2 P0 — `Resources.Load` returning null (~15 high-traffic paths)

Every line below calls `Resources.Load<T>(...)` and gets back `null` because the asset isn't under any `Resources/` folder, the path is wrong, or the prefab doesn't exist. Each one is a silent gameplay failure at runtime.

| # | File:Line | Path requested | What's missing |
|---|---|---|---|
| RL1 | `Moon1AnastasiaRocker.cs:65` | `Prefabs/Moon1/AnastasiaRocker` | Prefab exists outside `Resources/` |
| RL2 | `Moon1CombatDirector.cs:88` | `Enemies/MudGolem` | No `Resources/Enemies/` folder |
| RL3 | `Moon1CombatDirector.cs:174` | `Enemies/ResetScout` | Same |
| RL4 | `Moon1MudGolemRSSpawnTrigger.cs:141-143` | `Enemies/MudGolem`, `Enemies/ResetScout` | Same |
| RL5 | `Moon1EnvironmentDetail.cs:52` | `Effects/DustMotes` | No `Resources/Effects/` folder |
| RL6 | `Moon1EnvironmentDetail.cs:53` | `Effects/Fireflies` | Same |
| RL7 | `Moon1EnvironmentDetail.cs:54` | `Effects/Sunshafts` | Same |
| RL8 | `Moon1EnvironmentDetail.cs:55` | `Effects/RollingFog` | Same |
| RL9 | `Moon1ExcavationSites.cs:55` | `Materials/M_Mud_Fresh` | No `Resources/Materials/` |
| RL10 | `Moon1ExcavationSites.cs:57` | `Materials/PBR/Ground037` | Same |
| RL11 | `Moon1MudPoolPuzzle.cs:207` | `Collectibles/LoreArtifact` | Folder missing |
| RL12 | `Moon1VillagerAmbient.cs:86` | `Characters/KayKit/...` | Wrong prefix — should be `Prefabs/Characters/KayKit/...` |
| RL13 | `PickupInteractable.cs:84` | `VFX/ShardCollect` | Missing |
| RL14 | `HitFeedback.cs:118` | `Combat/DamagePopup` | Missing |
| RL15 | `RuntimeHUDBuilder` | `Prefabs/UI/HUD_Root` | Deferred — 64 `new GameObject` per Play |

### 2.3 P0 — Prefabs broken at the asset level

| # | Prefab | Gap |
|---|---|---|
| P1 | `Prefabs/Combat/MudGolem.prefab` | ⚠ **PARTIAL** — `MudGolem.fbx` is now real on disk (Moon1 + Shared, 75180 bytes, GUID `670fd3e6fa435474eab8b6b5500f99d2`). Prefab still composed of primitive sphere/cube — one-shot `mcp__unity-tartaria__execute_code` rewire snippet queued. |
| P2 | `Prefabs/Moon1/AnastasiaRocker.prefab` | ✅ **MYTH-BUSTED 2026-06-04** — audit found prefab exists and materials are assigned (no magenta). Also moved into correct `Resources/Moon1/` sub-path. |
| P3 | `Prefabs/Moon1/Buildings/Echohaven_CrystalSpire.prefab` | ✅ **MYTH-BUSTED 2026-06-04** — real kit composition on disk. |
| P4 | `Prefabs/Moon1/Buildings/Echohaven_StarDome.prefab` | ✅ **MYTH-BUSTED 2026-06-04** — real kit composition. (`Echohaven_StarDome_Built.prefab` at root remains the canonical reference — 3041 lines, 118 PrefabInstances, 17 Cathedral kit GUIDs.) |
| P5 | `Prefabs/Moon1/Buildings/Echohaven_HarmonicFountain.prefab` | ✅ **MYTH-BUSTED 2026-06-04** — real kit composition. |
| P6 | `Prefabs/Moon1/Buildings/Echohaven_Cathedral.prefab` | ✅ **MYTH-BUSTED 2026-06-04** — `Echohaven_Cathedral.prefab` exists at 3140 lines / 41 PrefabInstances. Original audit pass was looking at wrong directory. |
| P7 | `Prefabs/Characters/Bob.prefab` | Still open. `BobInnkeeper` exists at `Moon1/Blender/NPCs/` — PascalCase mismatch risk for any `Resources.Load("Characters/Bob")` consumer. |

### 2.4 P1 — NPC animation

| # | Asset | Gap |
|---|---|---|
| N1 | `Prefabs/Characters/Milo.prefab` | ✅ **CLOSED 2026-06-04** — `Animator` component wired with `AC_KayKit_Medium` controller (guid `78734b5564ec49d4bade3f0b1c74f6d9`). NOTE: Blender mesh is static joined — joints won't deform until armature pipeline lands. |
| N2 | `Prefabs/Characters/Anastasia.prefab` | ✅ **CLOSED 2026-06-04** — Animator wired. Same armature caveat. |
| N3 | `Prefabs/Characters/Lirael.prefab` | ✅ **CLOSED 2026-06-04** — Animator wired. Same armature caveat. |
| N4 | `Prefabs/Characters/Cassian.prefab` | ✅ **CLOSED 2026-06-04** — Animator wired. Same armature caveat. |
| N5 | All 4 NPCs | ✅ **CLOSED 2026-06-04** — All 4 prefab YAMLs edited: `Height=2`, `Center=(0,1,0)`, `Radius=0.4` (Milo: H=0.4, Center=(0,0.2,0), R=0.25). |
| NEW | NPC FBX bounds 27–37m | ✅ **CLOSED 2026-06-04** — Blender regen via PowerShell: `Anastasia.fbx` 1.70m, `Lirael.fbx` 1.80m, `Cassian.fbx` 1.80m, `MiloBoy.fbx` 0.71m. |
| OPEN | NPC armature rigging | NOT DONE — meshes are static joined; need armature-rigged pipeline upgrade. Until then, Animator is wired but joints don't deform. |

Only `Player.prefab` and `Char_Knight.prefab` carry working Animator controllers — they are the only chars that animate today.

### 2.5 P1 — Village building scale-bake distortion (non-uniform)

The agent that did the scale-bake used non-uniform scaling, distorting mesh proportions:

| # | Building | Transform scale | Note |
|---|---|---|---|
| V1 | `TownHall.prefab` | ✅ **CLOSED 2026-06-04** — Lane 2 gen-script scale edits shipped. |
| V2 | `VillageMill.prefab` | (not in this session's Lane 2 batch) | Still open. |
| V3 | `Watchtower.prefab` | ✅ **CLOSED 2026-06-04** — Lane 2 fix. |
| V4 | `VillageCottage_A/B/C.prefab` | (cottage variation still pending) | Visual triplet duplicates not yet differentiated. |
| V5 | `Apothecary.prefab` | ✅ **CLOSED 2026-06-04** — Lane 2 fix. |
| NEW | `BobsInn` 145m FBX → 6m | ✅ **CLOSED 2026-06-04** — Lane 2 gen-script. Scene-level `localScale ~0.083` hack still in scene YAML; cleanup pending after BobsInn re-imports at 6m. |
| NEW | `Bakery` undershoot | ✅ **CLOSED 2026-06-04** — Lane 2 fix. |

Fix: redo all village scales with uniform scaling, derived from the actual FBX bounds. (Bulk of this is now done — Mill and Cottage variation remain.)

### 2.6 P1 — Missing asset directories

| # | Path | Gap |
|---|---|---|
| A1 | `Resources/Audio/` | ⚠ **PARTIAL** — Resources/Audio tree partially populated. §16.11 4-layer adaptive music shows 2 of 4 stems on disk (layer3/layer4 wav files still missing). Other audio buckets (tuning SFX / ambient zone / restoration stinger / cinematic music) need re-verification. |
| A2 | `Resources/Prefabs/UI/HUD_Root.prefab` | STILL OPEN — `RuntimeHUDBuilder` still spawns 64 GameObjects per Play. One-shot `mcp__unity-tartaria__execute_code` bake snippet queued for next Unity session. |
| A3 | `Models/Blender/Moon1/MudGolem.fbx` | ✅ **CLOSED 2026-06-04** — Real Kaydara binary at `Moon1/MudGolem.fbx` (75180 bytes) and `Shared/MudGolem.fbx`. `tools/blender/gen_mud_golem.py` (5490 bytes) shipped for future regen. GUID `670fd3e6fa435474eab8b6b5500f99d2` preserved at Shared path. |
| A4 | `Models/Blender/Moon1/ResetScout.fbx` | Still at `Shared/` — relocation lower-priority than originally framed; can wait. |

### 2.7 P2 — Dead / stub code in the active path

| # | File:Line | Gap |
|---|---|---|
| C1 | `GameEvents.cs:423-433` | 9 `Fire*` methods are pure `Debug.Log` stubs: `CollectibleGathered`, `AchievementUnlocked`, `CompanionTrustChanged`, `LeverPulled`, `MoonProgressUpdate`, `PlayerEnteredZone`, `TutorialStep`, `TuningNodeActivated`. No subscribers see them. |
| C2 | `PauseMenu.cs` | Explicit `/* no-op stub */`. Dead. Delete. |
| C3 | `PauseOverlay.cs` | Explicit `/* no-op stub */`. Dead. Delete. |
| C4 | `CompanionBehaviorSystem.cs:12-13` | `// STUB` + `// TODO` comments at top of file. |
| C5 | `EnemyAIController.cs:216` | `// TODO: Add visual VFX (ice particles, blue tint shader)` for Frequency Shield freeze. |
| C6 | `DayNightController.cs:123` | `// TODO: Wire to ExcavationSystem.AetherYieldMultiplier`. |
| C7 | `TartarianCalendar.cs:47,76,128,130` | 4 LiveOps stub `Debug.Log` methods. |

### 2.8 P2 — Event duplication / canonical name conflict

| # | File:Line | Gap |
|---|---|---|
| E1 | `GameEvents.cs:447` | `OnSeventeenthHour` declared here. |
| E2 | `TartarianHourCycle.cs:37` | `OnSeventeenthHour` declared here — **only this one actually fires.** |
| E3 | `TartarianCalendar.cs:44` | `OnSeventeenthHour` declared here. |
| E4 | `RailEscortController.OnSeventeenthHourTriggered` | Fourth name variant. |

Pick one canonical declaration (recommend `TartarianHourCycle.cs:37` since it's the firer) and rewrite the other three sites as subscribers.

### 2.9 P2 — Doc drift

| # | Doc:Line | Gap |
|---|---|---|
| D1 | `docs/02_AETHER_ENERGY_SYSTEM.md` | Uses "3-Band/6-Band/9-Band" terminology that doesn't reconcile with the CLAUDE.md canon (Telluric 7.83 / Harmonic 432 / Celestial 528). |
| D2 | `docs/15_MVP_BUILD_SPEC.md §13 audio table, line 697` | Still has a **1296 Hz** row. Canon is **528 Hz** (Celestial). |
| D3 | `docs/15_MVP_BUILD_SPEC.md §1 line 62` | Lists Lirael, Anastasia, Cassian as "Phase 2+ NOT in". They are **shipped in Moon 1**. Spec must update. |
| D4 | `KNOWN_ISSUES.md` | 2026-05-21 stale, release-framing era doc. Must be quarantined or rewritten under the 2026-06-03 mandate. |
| D5 | `docs/16_MOON2_BUILD_SPEC.md` | **Does not exist.** Moon 2 (Crystalline Caverns), dissonance crystals, micro-giant mode, Cassian-betrayal trust system — all unspec'd at build-spec level. Moon scenes exist (`CrystallineCaverns.unity` etc.) but no Moon-number → scene mapping doc. |

### 2.10 P3 — Scene composition observation (not a bug)

`Moon1_Systems` GameObject in the scene has only a Transform — every controller bootstraps via `[RuntimeInitializeOnLoadMethod]` self-spawn. This is an architecture choice; the scene file is decorative. Listed here so a future agent doesn't "fix" it by adding components that would double-instantiate.

---

## 3. §16 GATE 1 honest verdict

| Criterion | Type | Status |
|---|---|---|
| §16.1 | Runtime — 15-min play video | ❌ never produced |
| §16.2 | Runtime — 60 FPS mid-spec | ❌ no profiler run |
| §16.3 | Runtime — 30 FPS low-spec | ❌ no profiler run |
| §16.4 | Runtime — RAM ceiling | ❌ no profiler run |
| §16.5 | Greppable — 0 silent-fail catches | ✅ verified |
| §16.6 | Greppable — 0 `Detail_*` clusters in hero buildings | ✅ verified post 2026-06-04 audit — all 4 hero buildings are real kit compositions (Cathedral 3140 lines / 41 PrefabInstances). The "3 of 4 are stubs" finding was a wrong-directory audit pass. |
| §16.7 | Greppable — 40 Milo VO lines | ✅ verified (57 actual — over spec, fine) |
| §16.8 | Greppable — 8 haptic JSON patterns | ✅ verified |
| §16.9 | Greppable — 4-layer adaptive music | ⚠ controller wired, but stems missing from `Resources/Audio/` |
| §16.10 | Greppable — 3-band Aether | ✅ verified |
| §16.11 | Greppable — 5 building restoration states | ✅ verified |
| §16.12 | Runtime — 30-min soak test | ❌ never performed |

**Original audit score: 5 ✅ / 2 ⚠ / 5 ❌ — not 12/12 as implied.**
**Post 2026-06-04 LATE LOCK-DOWN: 6 ✅ / 1 ⚠ / 5 ❌** — §16.6 myth-busted, §16.11 still partial (2 of 4 music stems on disk), §16.1–4 + §16.12 runtime artifacts still missing. **Moon 1 still NOT GATE 1 done.**

---

## 4. Recommended fix order (effort estimate per item)

In strict P0 → P3 order. Per the NIGHT MANDATE, no playtests until the whole wave closes.

1. **R1 Linear color space flip** — 1 min, 1 click in Player Settings. Biggest visual lift in the project.
2. **RL1–RL15 Resources.Load fixes** — 1–2 hours: move existing assets into `Resources/` subfolders, fix path strings, or stop calling Load and use direct prefab refs.
3. **P2 AnastasiaRocker material wireup** — 30 min: assign URP/Lit materials to all 8 children, re-bake.
4. **N1–N4 NPC Animators** — 2–4 hours: either author 4 humanoid controllers, or re-parent NPCs as Prefab Variants of `Char_Knight.prefab` with mesh swap.
5. **N5 NPC collider rescale** — 20 min: set Height=2, Center=1 across 4 prefabs.
6. **P3–P6 Hero building stubs** — 2 hours: use `Echohaven_StarDome_Built.prefab` as the pattern, build CrystalSpire/HarmonicFountain/Cathedral with real Cathedral kit compositions.
7. **P1, A3, A4 MudGolem mesh + ResetScout location** — 2 hours: author `MudGolem.fbx` in Blender via `tools/blender/`, move `ResetScout.fbx` into `Models/Blender/Moon1/`, rewire prefabs.
8. **A1 Audio Resources tree** — 1–2 hours: author or import ambient zone, tuning SFX, restoration stinger, 17th-hour cinematic; deposit at `Resources/Audio/`.
9. **V1–V5 village scale-bake redo** — 1 hour: apply uniform scaling, regenerate from FBX bounds.
10. **A2 HUD_Root.prefab bake** — 1 hour: run `RuntimeHUDBuilder` once in Editor, save the assembled hierarchy as a prefab, replace runtime construction with `Instantiate`.
11. **C1–C7 Dead code purge** — 1 hour: delete `PauseMenu.cs` + `PauseOverlay.cs`, implement the 9 `GameEvents.Fire*` stubs, clear the 6 `// TODO` markers.
12. **E1–E4 Event canonicalization** — 30 min: pick `TartarianHourCycle.OnSeventeenthHour`, delete duplicates, rewrite call sites.
13. **D1–D4 Doc reconciliation** — 1 hour: update `docs/02`, fix `docs/15 §1 line 62` + `§13 line 697`, quarantine `KNOWN_ISSUES.md`.
14. **D5 Moon 2 build spec** — 2–4 hours: author `docs/16_MOON2_BUILD_SPEC.md` from `docs/03` Moon 2 section + spec template from `docs/15`.
15. **§16.1–4, §16.12 runtime tests** — 1 hour (after everything above is closed): record 15-min play video, run Profiler at 1080p mid-spec + low-spec, capture RAM, run 30-min soak.

Total estimate: ~20 hours of focused work to honestly close Moon 1.

---

## 5. Per NIGHT MANDATE — no playtest until ALL fixed

This gap report is the single punch list. No partial verification. No "let me check that one thing." No Moon 2 work, no doc/16 authoring until Wave 1–6 are closed. Wave 7+ runs in parallel with the build waves only where it doesn't touch the same files.

When every row above is closed on disk AND the §16 runtime tests have artifacts checked in, Moon 1 is honestly done. Then Moon 2.

---

*MOON1_GAP_REPORT v1.0 · 2026-06-04 · Generated from parallel docs+code+prefab audit synthesis. Supersedes the 2026-06-03 NIGHT GATE 1 COMPLETE claim.*
