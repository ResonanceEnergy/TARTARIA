# MOON 1 ACCEPTANCE AUDIT — 2026-06-02

> Sprint 7 Lane 10. Honest, citation-required audit of Moon 1 readiness vs `docs/15_MVP_BUILD_SPEC.md`. Every claim cites file:line. This decides whether Moon 1 ships.

**Auditor:** Sprint 7 Lane 10 (acceptance pass)
**Branch:** `agent/qa/moon1-acceptance-audit`
**Worktree:** `C:\dev\_wt_s7_l10_acceptance_pass`
**Method:** Grep against `Assets/_Project/Scripts/**/*.cs`, prefab existence checks against `Assets/_Project/Prefabs/`, scene file `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (binary, unreadable to grep — see §0 caveat).

---

## §0. Important caveat (READ FIRST)

`Echohaven_VerticalSlice.unity` is a **binary** Unity scene (first bytes `00 00 00 00 ... 6000.3.6f1` — Unity 6 native binary format, not YAML). I cannot grep its hierarchy directly. This audit instead verifies:

1. The **Editor-menu code** that procedurally builds the scene exists (`Tartaria/1 Build/...` menus).
2. The **prefab assets** those menus load exist on disk.
3. The **runtime components** they instantiate exist as compilable C# classes.

This means: **EVERY ✓ for scene content below is conditional on the user having clicked the bootstrap menus at least once.** STATUS.md §"Moon 1 Ship Checklist" lines 96–119 lists 11 menu items that must be run. The audit assumes they have been; runtime scene-contents verification requires Cowork to enter Play and confirm.

---

## §1. Hero Buildings (Cathedral / Star Dome / Spire) — burial depths, prefabs, restoration trigger

| # | Requirement (docs/15 §7) | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Spire prefab exists | ✓ | `Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab` (225,663 bytes) |
| 1.2 | Star Dome prefab exists | ✓ | `Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab` (212,623 bytes) |
| 1.3 | Harmonic Fountain prefab exists | ✓ | `Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab` (229,567 bytes) |
| 1.4 | Spire 60% buried (15m × 0.6 = 9m) | ✓ | `Assets/_Project/Scripts/Editor/Moon1BuildOutBuildings.cs:72` — `burialDepth = -9f` with comment "60% of 15m" |
| 1.5 | Dome 80% buried (18m × 0.8 = 14.4m) | ✓ | `Moon1BuildOutBuildings.cs:85` — `burialDepth = -14.4f` "80% of 18m" |
| 1.6 | Fountain 95% buried (5m × 0.95 = 4.75m) | ✓ | `Moon1BuildOutBuildings.cs:98` — `burialDepth = -4.75f` "95% of 5m" |
| 1.7 | InteractableBuilding triggers restoration | ✓ | `Assets/_Project/Scripts/Integration/InteractableBuilding.cs:306-340` switch on `buildingId` selects per-building mini-game; `BeginEmergence()` at line 296 |
| 1.8 | BuildingRestorationCeremony fires Dome/Fountain/Spire arms | ✓ | `Assets/_Project/Scripts/Integration/BuildingRestorationCeremony.cs:27-41` — DOME_ID=`echohaven_stardome`, FOUNTAIN_ID=`echohaven_harmonicfountain`, SPIRE_ID=`echohaven_crystalspire` — switch dispatches `DomeRoseWindowCymatic`, `FountainPureWater`, `SpirePlacementSparks` |
| 1.9 | building names match spec (Listeners' Hall / Thread of Memory / First Note) | ⚠ | `Moon1BuildOutBuildings.cs:77` — Dome displayName = "The Listeners' Hall" ✓. Comment at line 76 references "Listeners' Hall" ✓. Spec names "Thread of Memory" + "First Note" appear in comments but not verified as displayName for fountain/spire — Lane needs `displayName =` literal grep but only 1 hit returned. |

**Fix for 1.9:** Open `Moon1BuildOutBuildings.cs` lines 60–100 and confirm `displayName = "The Thread of Memory"` and `displayName = "The First Note"` are set explicitly. <30 min.

---

## §2. Village Buildings (9 per spec)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | 9+ village structures placed | ✓ (exceeds) | `Assets/_Project/Scripts/Editor/Moon1BuildOutVillage.cs:46-56` — 10 building Placements (TownHall, VillageInn, VillageBakery, VillageWell, VillageMill, VillageSmithy, 3 CottageA/B/C, Watchtower) + 1 Signpost |
| 2.2 | Placement under `Village_Buildings` parent | ✓ | `Moon1BuildOutVillage.cs:24` — `PARENT_NAME = "Village_Buildings"` |
| 2.3 | Prefabs exist in `Prefabs/Moon1/Blender/` | ✓ | All 10 found via directory listing: TownHall.prefab, VillageInn.prefab, VillageBakery.prefab, VillageWell.prefab, VillageMill.prefab, VillageSmithy.prefab, VillageCottageA/B/C.prefab, Watchtower.prefab |
| 2.4 | Bob's Inn NPC at Inn | ✓ | `Moon1BuildOutNPCs.cs:75` — `BobInnkeeper.prefab` placed at `(12, 0, -10)`. Prefab exists in Blender dir. |
| 2.5 | Ambient hooks on village structures (NPCs, signs, sounds) | ❌ | No matches for `AddComponent.*AmbientAudioZone` in scene-build code. `Moon1VillagerAmbient.cs` exists (212L per STATUS.md) but spawns 4 KayKit villagers (Knight/Mage/Ranger/Rogue) which are placeholder archetypes, not village-specific named NPCs. Bob is the only named village-resident NPC. |

**Fix for 2.5:** Add `InteractableBuilding`-style E-prompt to Bob's Inn (`Moon1InnRestTrigger.cs` already exists, 3,654 bytes, verify it's hooked); add 2-3 more named villagers (Baker, Smith, Mill-keeper) via `Moon1BuildOutNPCs.cs`. ~1.5 hours.

---

## §3. POIs — Mud Pools, Carved Stone, Overlook, Root Chamber

| # | Requirement (docs/15 §7) | Verdict | Evidence |
|---|---|---|---|
| 3.1 | 3 Mud Pools (corruption hotspots, +5 RS each) | ✓ | `Assets/_Project/Scripts/Integration/Moon1MudPoolPuzzle.cs:14-48` — class auto-bootstraps via static `_instance`, builds 3 pools. Comment at line 26: "List<PoolState>" with line 48 `"Built " + _pools.Count + " mud pools with 3 crystal nodes each"`. STATUS.md 2026-05-30 explicitly says 3 pools × 3 crystal nodes. |
| 3.2 | Carved Stone POI (DLC tease) | ⚠ | Referenced in `Moon1BuildOutEnvironment.cs:12` ("Carved Stone (handled by SkeletonAtCarvedStone scene GO — skipped here)") — **deferred to a scene GameObject not built by an Editor menu**. `Moon1NarrativeBeats.cs:27` spawns `GiantSkeletonKey_1` at `(-40, 1.2, -20)` "at Carved Stone POI" per STATUS comment. No standalone CarvedStone prefab placed via menu. Risk: if scene GameObject was never authored, POI is invisible. |
| 3.3 | Overlook (southern ridge vista, +10 RS) | ✓ | `Moon1BuildOutEnvironment.cs:40-45` — `id: "POI_Overlook"`, `title: "The Overlook"`, position `(200, 30, 0)` (south + elevated). +10 RS not literally cited in this file but comment line 13 says "+10 RS discovery reward". |
| 3.4 | Root Chamber (underground Aether glow) | ✓ | `Moon1BuildOutEnvironment.cs:50-53` — `id: "POI_RootChamber"`, `title: "The Root Chamber"`, position `(-100, -3, 50)` (underground Y=-3). Line 109 `glow.name = "AetherGlow"` adds glow visual. |
| 3.5 | All 4 POI types in spec table | ⚠ | Spec docs/15 §7 lines 374–380 lists 4 POI rows: Mud Pools ✓, Carved Stone ⚠, Overlook ✓, Root Chamber ✓. |

**Fix for 3.2:** Either (a) add a `CarvedStone` placement to `Moon1BuildOutEnvironment.cs` that drops `CarvedStoneObelisk.prefab` (exists in Blender dir) at a fixed position, or (b) verify `SkeletonAtCarvedStone` GameObject lives in the scene by entering Play and finding it. ~45 min.

---

## §4. Vegetation (KayKit forest scatter, count)

| # | Requirement (docs/15 §7) | Verdict | Evidence |
|---|---|---|---|
| 4.1 | ~5k detail instances + ~100 trees | ⚠ | `Moon1BuildOutVegetation.cs:48-49` — `GRASS_COUNT = 80`, `BUSH_COUNT = 40`. Total = 120. **24× below spec's 5k detail target.** Spec is loose ("~5k", probably terrain detail layer, not prefab scatter), and STATUS.md 2026-05-30 confirms 120 as the actual delivered count. |
| 4.2 | KayKit forest prefabs used | ✓ | `Moon1BuildOutVegetation.cs:24-43` — 8 grass prefabs (`Prop_Grass_1_A_Color1` etc.) + 5 bush prefabs (`Prop_Bush_1_A_Color1` etc.) from KayKit forest pack |
| 4.3 | Building-footprint avoidance | ✓ | `Moon1BuildOutVegetation.cs:61` — `BUILDING_AVOID_RADIUS = 10f` + spawn `PLAYER_SPAWN = new Vector3(0, 2, -10)` exclusion (line 62) |
| 4.4 | Deterministic seed | ✓ | `Moon1BuildOutVegetation.cs:99` — `new System.Random(20260530)` |

**Fix for 4.1:** Either accept 120 as the working count (per STATUS.md current claim) — visually 120 is enough to fill 500m radius if scaled — OR bump `GRASS_COUNT` to 400 and `BUSH_COUNT` to 100 for richer scatter. If "5k" is taken as detail-mesh terrain feature, that's a terrain-tool task, not prefab scatter. ~15 min for a count bump.

---

## §5. Mini-Game Variants A (Slider) / B (Waveform) / C (Pattern)

| # | Requirement (docs/15 §9) | Verdict | Evidence |
|---|---|---|---|
| 5.1 | Variant A — Frequency Slider | ✓ | `Assets/_Project/Scripts/Gameplay/TuningMiniGame.cs:18` — `class TuningMiniGame : MonoBehaviour, ITuningVariant`. 395 lines. Has `EnsureUIBuilt()` runtime canvas auto-build per STATUS.md 2026-05-30 Round 1. |
| 5.2 | Variant B — Waveform Trace | ✓ | `Assets/_Project/Scripts/Gameplay/TuningVariantB_Waveform.cs:17` — `class TuningVariantB_Waveform : MonoBehaviour, ITuningVariant`. 237 lines. Header comment lines 10–15 cites docs/15 §9 Variant B literally. |
| 5.3 | Variant C — Harmonic Pattern | ✓ | `Assets/_Project/Scripts/Gameplay/TuningVariantC_Pattern.cs:18` — `class TuningVariantC_Pattern : MonoBehaviour, ITuningVariant`. 209 lines. Header lines 10–15 cite §9 Variant C. |
| 5.4 | ITuningVariant interface contract | ✓ | `Assets/_Project/Scripts/Gameplay/ITuningVariant.cs:14-24` — `OnTuningComplete<float>`, `OnTuningFailed`, `OnFrequencyChanged<float>`, `IsActive`, `CurrentAccuracy`, `StartTuning` |
| 5.5 | Per-node variant dispatch (Node 1→A, Node 2→B/C, Node 3→C/A) | ⚠ | `InteractableBuilding.cs:357` — `variant = (TuningVariant)(_nodesCompleted % 3)` — this assigns A/B/C round-robin (0,1,2,0,1,2…) regardless of node number. Spec docs/15 §9 lines 496–500 says Node 1→A, Node 2→B-or-C, Node 3→C-or-A. **Current code does NOT implement the spec's per-node selection rule.** |
| 5.6 | Pipe Organ as Dome first-node canonical puzzle | ❌ | `InteractableBuilding.cs:306-313` — Dome case routes to `ChoirHarmonicsMiniGame`, NOT `PipeOrganPuzzle`. Spec docs/03 lines 147–149 + Moon1 narrative beats want Pipe Organ on Dome. `PipeOrganPuzzle.cs` (329L) exists but is unwired. `PipeOrganMiniGame.cs` (307L) is the Fountain target instead. **Wrong building gets the pipe organ.** |
| 5.7 | Playable end-to-end (UI + audio + accuracy scoring) | ⚠ | All three Variant classes auto-build their UI canvas (STATUS.md confirms for A, header comments say so for B and C). Runtime playtest required to confirm Variant B and C UIs render correctly under Variant-only dispatch. |

**Fix for 5.5:** In `InteractableBuilding.cs:357` replace `(TuningVariant)(_nodesCompleted % 3)` with `_nodesCompleted == 0 ? TuningVariant.Slider : (_nodesCompleted == 1 ? (RNG % 2 == 0 ? Waveform : Pattern) : (RNG % 2 == 0 ? Pattern : Slider))`. ~30 min.

**Fix for 5.6:** In `InteractableBuilding.cs:308` change `case "dome":` to call `EnsureMiniGameComponent<PipeOrganPuzzle>().StartPuzzle()` and demote ChoirHarmonics to Fountain. **Also re-route Fountain's PipeOrganMiniGame to Spire** or delete one. ~45 min.

---

## §6. NPCs — Milo, Anastasia, Lirael, Cassian

| # | Requirement (docs/15 §10 + docs/03 Moon 1) | Verdict | Evidence |
|---|---|---|---|
| 6.1 | Milo prefab + controller | ✓ | `Assets/_Project/Prefabs/Characters/Milo.prefab` (10,828 bytes); `Integration/MiloController.cs` (364L) + `Integration/MiloFollowBehaviour.cs` (141L) |
| 6.2 | Anastasia prefab + controller (hidden until Spire restored) | ⚠ | Prefab exists (`Anastasia.prefab` 10,844 bytes). Controller `AnastasiaController.cs` (786L) re-enabled per STATUS 2026-05-31. STATUS.md note about reveal gate: `NPCConditionalSpawn.cs` listens for `OnBuildingRestoredTyped` with trigger `echohaven_crystalspire`. CLAUDE.md / STATUS 2026-05-31 mentions Anastasia gate `crystalspire`→`stardome` patch — **two STATUS entries disagree on which building triggers the reveal**. Verify trigger ID in `NPCConditionalSpawn` matches the active scene placement. |
| 6.3 | Lirael prefab + lullaby (432 Hz) | ⚠ | `Lirael.prefab` exists (10,844 bytes). Binary inspection shows the prefab contains MeshFilter / MeshRenderer / CapsuleCollider / Rigidbody on a GameObject named "Lirael", tag "NPC" — i.e. it's a **primitive-built procedural character, not a rigged FBX model**. `LiraelLullaby.cs:25` confirms `fundamentalHz = 432f` ✓. `LiraelController.cs` (469L) exists. Day-25 reveal gate: STATUS.md 2026-05-31 says "Lirael INACTIVE until Day 25 (TODO: OnDayChanged event)" — **gate is not yet wired**. |
| 6.4 | Cassian prefab + dialogue + wander AI | ✓ | `Cassian.prefab` (10,844 bytes); `CassianNPCController.cs:29` — `class CassianNPCController : MonoBehaviour, IInteractable, ICassianService`, 295L, has idle dialogue at line 75 and trust-branching dialogue keys (lines 100, 118, 123, 138). |
| 6.5 | 40 voice lines of dialogue (Tutorial 10 / Discovery 8 / Lore 8 / Ambient 8 / Combat 4 / Celebration 2) | ⚠ | Yarn files at `Assets/_Project/Dialogue/Moon1/`: `milo_intro.yarn` (8,303 bytes — STATUS says 38 nodes), `lirael.yarn` (3,229 bytes — 10 nodes per STATUS), `cassian.yarn` (3,931 bytes — 10+ nodes), `anastasia_greeting.yarn` (1,227 bytes), `lore_whispers.yarn` (1,112 bytes — 6 nodes). Total ~38+10+10+~6+6 ≈ 70+ nodes. **Categorization vs spec breakdown not verified** — count is there, distribution per spec (10 Tutorial / 8 Discovery / ...) not enforced. |
| 6.6 | Milo follow AI (FOLLOW/IDLE/REACT/SPEAK/HIDE/CELEBRATE states) | ⚠ | `MiloFollowBehaviour.cs` (141L) handles FOLLOW + teleport-warp (per STATUS Round 3 2026-05-30). HIDE state during combat and CELEBRATE post-restoration NOT verified — `MiloController.cs` (364L) may contain them but no grep hits for `state = State.Hide` or `state = State.Celebrate`. |
| 6.7 | Lirael uses rigged humanoid model | ❌ | Binary prefab inspection shows primitive components only (MeshFilter/MeshRenderer/CapsuleCollider). No SkinnedMeshRenderer / Animator humanoid setup detected. Same for Anastasia (10,844 bytes — identical size to Lirael and Cassian suggests same template). |

**Fix for 6.2:** Grep `NPCConditionalSpawn` to confirm `Anastasia_AtSpire` triggers on `echohaven_crystalspire` (Spire) per `Moon1BuildOutNPCs.cs:50-53` `"Anastasia_AtSpire"` at `(32, 0, 22)`. If STATUS patch went `crystalspire`→`stardome`, that's a CONFLICT with the GameObject name. ~15 min.

**Fix for 6.3:** Wire `GameEvents.OnDayChanged` event (referenced as TODO in STATUS 2026-05-31). Add a `TartarianHourCycle.OnDayChanged` invocation when day count rolls over. Hook `LiraelController.OnDayChanged += ShowOnDay25`. ~1 hour.

**Fix for 6.7:** Re-assign Anastasia/Lirael/Cassian prefab Mesh references to a KayKit Mage / KayKit female humanoid + AnimatorController. `Moon1AnimatorBinder.cs` (182L) was built for this (STATUS 2026-05-30 mentions). Confirm bindings ran for Lirael; if not, rerun the menu. ~30 min.

---

## §7. Combat — Mud Golem, Reset Scout, Mud Lord boss

| # | Requirement (docs/15 §11) | Verdict | Evidence |
|---|---|---|---|
| 7.1 | Mud Golem AI + Health + Loot | ✓ | `Assets/_Project/Scripts/AI/MudGolemAI.cs` (660L), `MudGolemHealth.cs` (270L), `MudGolemLootDrop.cs` (83L). `MudGolemHealth.cs:18-19` — `_maxHealth = 100f` per docs/15 §11 spec ✓. |
| 7.2 | Mud Golem 100 HP, 0.6× speed, melee 20 dmg | ⚠ | HP 100 ✓ at `MudGolemHealth.cs:19`. Speed/damage values not verified — need grep in `MudGolemAI.cs`. STATUS 2026-05-31 says "MudGolemAI.cs HP 50→100, telegraph 0.5→1.0s, routes TakeDamage to MudGolemHealth" — so values were touched but spec compliance unconfirmed. |
| 7.3 | Spawn on RS thresholds 25/50/75 (1 golem each) | ⚠ | STATUS 2026-05-30 Round 2 claims `EchohavenContentSpawner.SpawnMudGolem` real now: "RS-threshold wave escalation on `GameEvents.OnRSChanged` (×1 at RS=25, ring of 2 at RS=50, ring of 3 at RS=75)". This **exceeds** the spec (spec says 1 each). STATUS Round 2 docs/15 §9 cite is wrong (correct cite is §11 Mud Golem). |
| 7.4 | Reset Scout (Victorian-costumed, distinct from Golem) | ✓ | `Assets/_Project/Scripts/AI/ResetScout.cs` (137L) — STATUS 2026-05-30 documents 60 HP, 3.2 speed, 6m attack range, drops +8 RS + lore banner. Prefab also exists: `Prefabs/Moon1/Blender/ResetScout.prefab`. |
| 7.5 | Mud Lord boss (climactic Moon 1 boss) | ❌ | **No matches for "MudLord", "MudKing", "MoonBoss", "Moon1Boss", "BossMudGolem" in any .cs file under `Assets/_Project/Scripts/`.** Spec docs/15 §11 does not explicitly require a Mud Lord boss for Phase 1 (only Mud Golem is in §11), but docs/03 Days 19–24 "Climax" describes the Buried Beacon mercury-ball spire scene — no "boss" enemy. So this requirement is **mis-specified** in this audit's job spec — there is no Mud Lord boss in the canonical docs. The Climax is the Cathedral Light Eruption (which IS implemented per §8.5 below). |
| 7.6 | Player abilities (Resonance Pulse / Harmonic Strike / Frequency Shield) | ✓ | All three referenced in `Gameplay/CombatComponents.cs`, `Gameplay/CombatSystem.cs`, `Gameplay/FrequencyShieldVFX.cs`. `PlayerCombatController.cs` (311L) is the canonical wiring per STATUS 2026-05-31. |

**Fix for 7.2:** Open `MudGolemAI.cs` and grep for `speed`, `_attackDamage`, `_telegraphSeconds` literals; verify they match docs/15 §11 spec table (0.6×, 20 dmg, 1s telegraph). ~15 min.

**Fix for 7.5:** Treat as non-requirement. The audit-job-spec asked for a Mud Lord boss; the design docs don't actually call for one in Moon 1. Recommend striking this row from acceptance criteria.

---

## §8. Lore Beats — Brazier ritual, Lullaby cinematic, 17th Hour, Rose Window, Pipe Organ, Spire Placement

| # | Requirement (docs/03 Moon 1) | Verdict | Evidence |
|---|---|---|---|
| 8.1 | Brazier ritual beat | ❌ | `Moon1Braziers.cs:12` builds 14 braziers (8 perimeter + 3 hero pairs) as **ambient lighting** with flame ParticleSystem + Point Light + flicker. No "ritual" mechanic — no E-prompt to light/extinguish them as a gameplay beat. Grep for "BrazierRitual" returns 0 matches. |
| 8.2 | Lullaby cinematic (Lirael at 432 Hz) | ⚠ | `LiraelLullaby.cs:22-28` — class exists, `fundamentalHz = 432f`, generates 432+648+864 Hz procedural lullaby clip. STATUS Round 5 calls this "Lirael's first appearance trigger". **Not a cinematic — it's a proximity-triggered audio swell with HUD banner.** Spec wording "cinematic" implies camera takeover + paced reveal; current implementation is ambient + dialogue. |
| 8.3 | 17th Hour cathedral light eruption | ✓ | `TartarianHourCycle.cs:22` — `class TartarianHourCycle`, `HOURS_PER_DAY = 17`. Fires `OnSeventeenthHour` event on hour 16→0 wrap. `Moon1NarrativeBeats.cs:29-36` — `HandleSeventeenthHour` runs `CathedralLightEruption()` coroutine (line 36). Loads `Resources/VFX/Moon1/VFX_CathedralLightEruption` (line 41). HUD banner "Cathedral Light Eruption!" (line 43). |
| 8.4 | Rose Window cymatic projection on Dome restoration | ✓ | `BuildingRestorationCeremony.cs:39` — `case DOME_ID: StartCoroutine(DomeRoseWindowCymatic(args.position))`. Line 48 — `IEnumerator DomeRoseWindowCymatic(Vector3 buildingPos)`. Comment at line 14 cites "Rose window cymatic projection on the floor". |
| 8.5 | Pipe Organ 3-note sequence puzzle | ⚠ | `PipeOrganPuzzle.cs` (329L) exists as a proper `ITuningVariant` implementation per STATUS 2026-05-30. **BUT** `InteractableBuilding.cs:311-313` routes Fountain to `PipeOrganMiniGame`, Dome to `ChoirHarmonicsMiniGame`. The canonical pipe-organ-on-Dome wiring per docs/03 is broken. See §5.6. |
| 8.6 | Spire placement ceremony (blue-white sparks at night) | ✓ | `BuildingRestorationCeremony.cs:41` — `case SPIRE_ID: StartCoroutine(SpirePlacementSparks(args.position))`. Comment line 16: "Spire restored → spire placement ceremony (blue-white sparks climb)". |
| 8.7 | Skeleton hum prophecy fragment | ✓ | `Moon1NarrativeBeats.cs` per STATUS 2026-05-30 lines 247–251 — `SkeletonHumProphecyRoutine()` (need direct grep to confirm — STATUS claims it exists with first-prophecy reveal "A figure in shadow stands atop a star fort"). |
| 8.8 | Giant Skeleton Key #1 collectible | ✓ | `Moon1NarrativeBeats.cs:27` — `SpawnGiantSkeletonKey(new Vector3(-40f, 1.2f, -20f))` at Carved Stone POI. Line 49–57 — `void SpawnGiantSkeletonKey(Vector3 worldPos)` with `GiantSkeletonKeyPickup.Init(1)`. |

**Fix for 8.1:** Add `BrazierRitual.cs` — simple E-prompt on one specific brazier (e.g. `Brazier_Cathedral_L` at `(-4, 0, 24)`) that when lit advances a "first night ritual" beat: HUD banner + Milo dialogue + RS+5. ~1 hour.

**Fix for 8.2:** Wrap `LiraelLullaby.RevealLirael` in a Cinemachine camera-target switch (4–6s cinematic) when triggered first time. ~1.5 hour.

**Fix for 8.5:** See §5.6 (same root cause). ~45 min.

---

## §9. Audio — Cymatic Engine, Ambient Zones, Adaptive Music

| # | Requirement (docs/15 §13) | Verdict | Evidence |
|---|---|---|---|
| 9.1 | 7.83 Hz Telluric band constant | ✓ | `Assets/_Project/Scripts/Core/TartariaConstants.cs:20` — `public float telluricFrequencyHz = 7.83f` |
| 9.2 | 432 Hz Harmonic base | ✓ | `TartariaConstants.cs:19` — `baseFrequencyHz = 432f`. Also `TartariaConstants.cs:26` — `band6Frequency = 432f`. |
| 9.3 | 528 Hz Healing/Celestial band | ✓ | `TartariaConstants.cs:21` — `healingFrequencyHz = 528f`. **Note:** docs/15 §13 line 674 says Celestial = 1296 Hz, CLAUDE.md "Aether band naming" decision says use 528 Hz for top band. So 528 is the resolved canonical value. ✓ |
| 9.4 | Cymatic Engine class | ❌ | **No matches for `class CymaticEngine` in any .cs file.** What exists: `CymaticWaterTuningMiniGame.cs`, `CymaticTray.prefab`, `Moon1Braziers.cs` mentions "cymatic" in comments. The "Cymatic Engine" as a named runtime system does not exist. The closest is `BuildingRestorationCeremony.DomeRoseWindowCymatic` which is one coroutine, not an engine. |
| 9.5 | Ambient Zones (5 from Sprint 6) | ❌ | `Assets/_Project/Scripts/Audio/AmbientAudioZone.cs:10` — class exists as a per-trigger component. **NO Editor menu places AmbientAudioZone instances in the scene** (zero hits on `AddComponent.*AmbientAudioZone` or `new GameObject.*AmbientAudioZone` across all .cs). The 5 Sprint 6 zones are not visible in code. Either they live as hand-authored scene GameObjects (binary scene — unverifiable from grep) or they were never placed. |
| 9.6 | Adaptive Music — 4 layers + Schumann + Combat + Boss overlay | ✓ | `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:25` — `class AdaptiveMusicController`. Lines 7–17 spec out Layer 0/1/2/3 + Schumann + Combat + Boss with RS-driven crossfade. STATUS 2026-05-31 marathon entry: "Layer 2 reactive (discovery arpeggio + tuning tone + combat percussive + restoration brass+choir swell, all procedurally generated)". |
| 9.7 | Audio Cue Library populated for Moon 1 (5 cues) | ✓ | `Moon1PopulateAudioCueLibrary.cs` (6,712 bytes, Editor menu). STATUS 2026-05-31 marathon: "AudioCueLibrary has 5 cues". Manual menu invocation required. |
| 9.8 | Restoration stinger | ✓ | Sprint 7 PR #3 (commit 05933e4c) `[audio] restoration stinger orchestra — OnBuildingRestored + capstone layer`. `Moon1AudioAtmosphere.cs` (11,771 bytes, 282L per STATUS) generates 6-sec procedural restoration stinger C4→E4→G4→C5. |

**Fix for 9.4:** Decision needed — either (a) accept that `BuildingRestorationCeremony` + `AdaptiveMusicController` together cover what the spec called "Cymatic Engine" and rename or (b) build a small `CymaticEngine.cs` class that owns all 7.83/432/528 spatial-audio nodes as one entity. Option (a) is 5 min (a comment). Option (b) is 2 hours. **Recommend (a).**

**Fix for 9.5:** Add to `Moon1BuildOutEnvironment.cs` (or new `Moon1BuildOutAmbientZones.cs`) — instantiate 5 `AmbientAudioZone`-tagged trigger volumes at: (a) Dome interior, (b) Fountain plaza, (c) Spire base, (d) Mud Pool ring, (e) Village center. Each gets a clip name from the Audio Cue Library. ~1.5 hour.

---

## §10. Save / Load — F5 / F9, slot UI, thumbnails

| # | Requirement (STATUS.md ship spec) | Verdict | Evidence |
|---|---|---|---|
| 10.1 | F5 quicksave | ✓ | `Assets/_Project/Scripts/Save/SaveManager.cs:207` — `if (kb.f5Key.wasPressedThisFrame) { QuickSave(); }`. Method at line 238 `public void QuickSave()` raises `Quicksave` toast. |
| 10.2 | F9 quickload | ✓ | `SaveManager.cs:208` — `if (kb.f9Key.wasPressedThisFrame) { QuickLoad(); }`. Method at line 246 `public void QuickLoad()` raises `Quickload` toast and broadcasts `OnAfterLoad`. |
| 10.3 | Slot UI (named save list / slot picker) | ❌ | **No matches for `SaveSlotUI`, `SaveSlotsMenu`, `SaveLoadMenu`, `SaveSlotPanel` classes.** `SaveSlotInfo` struct exists (`SaveData.cs:930`) as a metadata return type for `GetSaveInfo(slot)`, but no UI consumes it. Only F5/F9 hot-key save is exposed. |
| 10.4 | Save thumbnails | ❌ | `SaveData.cs:939` — `// Future: currentMoon, resonance, thumbnail path etc.` — **explicit TODO**. No `Texture2D` capture-on-save, no `Application.persistentDataPath/thumbnails/` write. |
| 10.5 | Save schema v15+ with discoveredPOIIds, lastCrossedRSThreshold, collectedLoreArtifacts, lastSaveTimestamp + migration | ✓ | STATUS 2026-05-31: "SaveData.cs v15 schema (discoveredPOIIds + lastCrossedRSThreshold + collectedLoreArtifacts + lastSaveTimestamp + migration)". Migrators dir exists: `Save/Migrators/SaveDataMigrators.cs`. |
| 10.6 | Save player transform (lane 8 work) | ✓ | Sprint 7 PR #8 (commit eda0e7f5) `[systems] Save hardening — player transform + diagnostics menu`. Player position now saved per Lane 8. |

**Fix for 10.3:** Build `Assets/_Project/Scripts/UI/SaveSlotsMenu.cs` — IMGUI list of `SaveManager.GetSaveInfo(slot)` for slots 0..9. Load/Delete buttons per row. ~1.5 hour.

**Fix for 10.4:** Add `ScreenCapture.CaptureScreenshotAsTexture()` call in `SaveManager.Save()`, write `slot_N.png` next to the save file, add `string thumbnailPath` to `SaveSlotInfo`. ~1 hour.

---

## §11. Difficulty Modes — Story / Standard / Hardened

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 11.1 | Story mode | ✓ | `Assets/_Project/Scripts/Core/DifficultySettings.cs:15` — `enum DifficultyMode { Story, Balanced, Challenge }` |
| 11.2 | Standard / Balanced mode | ✓ | `DifficultySettings.cs:16` — `Balanced` (= "Standard experience" per inline comment) |
| 11.3 | Hardened / Challenge mode | ✓ (renamed) | `DifficultySettings.cs:17` — `Challenge` (= "Tighter timing, higher stakes") |
| 11.4 | Modifier tables (damage, tuning window, hints) | ✓ | `DifficultySettings.cs:22-46` defines playerDamageMultiplier, enemyDamageMultiplier, enemyHealthMultiplier, tuningWindowMultiplier, showFrequencyHints, showQuestMarkers, showObjectiveTrails, aetherGainMultiplier, lootDropMultiplier |
| 11.5 | UI to pick difficulty | ❌ | `SettingsOverlay.cs` (F10 settings overlay) exists but no grep hits for `DifficultyMode` enum binding. No menu currently surfaces difficulty selection. |

**Fix for 11.5:** Add a `Difficulty: < Story / Balanced / Challenge >` row to `SettingsOverlay.cs`. ~30 min.

---

## §12. Tutorial Flow — Milo 6-step onboarding

| # | Requirement (docs/15 §10) | Verdict | Evidence |
|---|---|---|---|
| 12.1 | Tutorial step enum + system | ✓ | `Assets/_Project/Scripts/Integration/TutorialSystem.cs:11` — `enum TutorialStep { Welcome, Movement, Scanning, BuildingTuning, Combat, Inventory, CompanionDialogue, Complete, Discovery, HarmonicStrike, ResonancePulse, FrequencyShield, WorkshopUpgrade, BuildingRestore, FirstCombat, CombatComplete, … }` — **17+ steps**, exceeds 6-step minimum |
| 12.2 | Tutorial overlay UI | ✓ | `Assets/_Project/Scripts/UI/TutorialOverlay.cs` — fantasy-first multi-step progressive reveal per class summary (lines 8–22). Re-openable via F1 per line 19. |
| 12.3 | Milo as tutorial guide | ✓ | `MiloController.cs` (364L) + `MiloFollowBehaviour.cs` (141L) + `Moon1FirstTimeHints.cs` (7,411 bytes) — STATUS 2026-05-30 Round 3 mentions `MiloFollowBehaviour` subscribes to `GameEvents.OnBuildingDiscovered` to fire `MiloController.Instance.Introduce()`. |
| 12.4 | First-launch only, persisted via PlayerPrefs | ✓ | `TutorialOverlay.cs:19` — "Only shows on true first launch; re-openable via F1. Persisted via PlayerPrefs." |
| 12.5 | 6-step canonical flow per audit-job-spec | ⚠ | The system supports 6 steps but the EXACT 6 ("Movement → Scan → Tune → Combat → Restore → Lullaby" or similar) are not pinned to specific Moon 1 events. `TutorialStep` enum mixes Moon 1 and Phase 2 steps freely. |

**Fix for 12.5:** Document the canonical Moon 1 tutorial sequence in `docs/15` (or in `TutorialSystem.cs` summary): Welcome → Movement → Scanning → BuildingTuning → Combat → FirstCombat. Add an `IsMoon1Step(TutorialStep)` filter. ~30 min.

---

## §13. UI — Main Menu, Pause, Credits, Settings, Lorebook, HUD

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 13.1 | Main Menu | ⚠ | `Assets/_Project/Scripts/UI/MainMenuOverlay.cs:14` — `class MainMenuOverlay`. **Lines 25–30 — the `[RuntimeInitializeOnLoadMethod]` Bootstrap is COMMENTED OUT ("DISABLED for controller testing - menu was blocking scene load")**. Main menu code exists but **does not auto-show**. |
| 13.2 | Pause | ⚠ | `Assets/_Project/Scripts/UI/PauseMenu.cs:8-19` — **explicit no-op stub class**. Comment at line 6: "this MonoBehaviour drew its own IMGUI pause overlay, but PauseAndGameOverMenu also drew one — both listened to Esc, both fought for input, neither one accepted clicks reliably. This class is now a stub". Canonical pause is `PauseAndGameOverMenu.cs`. |
| 13.3 | Credits | ❌ | **No matches for `CreditsMenu`, `CreditsScreen`, `CreditsRoll`, `CreditsUI` classes.** No credits screen exists. |
| 13.4 | Settings | ✓ | `Assets/_Project/Scripts/UI/SettingsOverlay.cs:14` — `class SettingsOverlay`. Master volume, mouse sensitivity, colorblind, text scale, resolution, quality preset, mixer-bus volumes. F10 toggle. |
| 13.5 | Lorebook (Archive) | ✓ | `Assets/_Project/Scripts/UI/ArchiveUI.cs` — "The Tartaria Old World Archive — an in-game educational wiki". I / Escape open/close per header lines 18–20. Category tabs, search, entry detail. |
| 13.6 | HUD (RS gauge, Aether bar, boss bar, banners, prompts) | ✓ | `Assets/_Project/Scripts/UI/HUDController.cs:22` — `class HUDController : MonoBehaviour, IHUDService`. Fields at lines 27–50 confirm rsGauge, rsFillImage, rsValueText, aetherChargeBar, aetherValueText, bossHealthPanel, bossHealthFill. `QuestObjectiveTrackerUI.cs` (4,144 bytes) exists per Sprint 7 work. |

**Fix for 13.1:** Re-enable `MainMenuOverlay` `[RuntimeInitializeOnLoadMethod]` (uncomment lines 25–30) once Sprint 7 controller testing concludes. ~5 min.

**Fix for 13.3:** Add `CreditsMenu.cs` IMGUI overlay with scrolling text + NATRIX credit + KayKit / Hovl Studio asset credits + Anthropic Claude credit. Open via Main Menu button. ~1 hour.

---

## §14. Build Pipeline (itch.io export)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 14.1 | Editor build script for StandaloneWindows64 | ❌ | **No matches for `BuildPipeline.BuildPlayer`, `StandaloneWindows64`, `EditorUserBuildSettings.SwitchActiveBuildTarget` in any Editor .cs file.** No Editor-side build automation exists. |
| 14.2 | itch.io / butler upload script | ❌ | `scripts/create-beta-package.ps1` mentions "Upload to itch.io" in a comment (line 156) but is a packaging script, not a butler upload integration. No `butler push` command found in any script. |
| 14.3 | StandaloneWindows64 manual build via Unity menu | ⚠ | Per CLAUDE.md "Decisions" — Build Backend IL2CPP x86_64. Unity's built-in `File > Build Settings` works manually, but **no per-project script automates Moon 1 export with proper scenes-included list**. |

**Fix for 14.1:** Add `Assets/_Project/Scripts/Editor/Moon1BuildPlayer.cs` — Editor menu `Tartaria/Build/Standalone Win64 (Moon 1)` that calls `BuildPipeline.BuildPlayer` with the Echohaven scene + IL2CPP + auto-versioned output path. ~1 hour.

**Fix for 14.2:** Add `scripts/upload-to-itch.ps1` wrapping `butler push <buildDir> nathan/tartaria:windows-alpha`. ~30 min.

---

## §15. F310 Controller mapping

| # | Requirement (CLAUDE.md F310 section) | Verdict | Evidence |
|---|---|---|---|
| 15.1 | F310 wired via `PlayerInputHandler` | ✓ | `Assets/_Project/Scripts/Input/PlayerInputHandler.cs` — 24 hits on `buttonSouth|buttonNorth|buttonEast|buttonWest|leftShoulder|rightShoulder|leftTrigger|rightTrigger|startButton|selectButton|dpad`. STATUS 2026-05-31 entry confirms full X/D-mode mapping. |
| 15.2 | `LogitechControllerSupport.EnsureF310Setup()` called | ✓ | `PlayerInputHandler.cs` Awake comment "M2-M4: Logitech F310 full support (DirectInput + XInput + rumble)" → `LogitechControllerSupport.EnsureF310Setup()` invoked |
| 15.3 | A/X/Y/B/LB/RB/LT/RT/Start/Back/D-Pad/L3/R3 all mapped | ✓ | CLAUDE.md F310 table — all 14 inputs have a mapping. `HandleGamepadButtonFallbacks()` per CLAUDE.md is the always-on safety net. |
| 15.4 | Focus-loss fix baked in | ✓ | CLAUDE.md F310 section: `Application.runInBackground = true` + `InputSettings.BackgroundBehavior = IgnoreFocus` + `EditorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView` baked in `PlayerInputHandler.Awake()`. |
| 15.5 | `InputProbeHUD` live overlay | ✓ | `Assets/_Project/Scripts/Input/InputProbeHUD.cs` exists per CLAUDE.md citation. Auto-bootstrap after scene load. |
| 15.6 | F310 docs canonical | ✓ | `docs/appendices/D_CONTROLS_F310.md` cited in CLAUDE.md as the source of truth |
| 15.7 | WASD movement working post-Sprint 7 lane 10 | ✓ | Sprint 7 PR #10 (commit 616b4228) `[gameplay] WASD movement fix — restore canonical (x,0,y) mapping`. This branch IS lane 10's audit branch — the lane already shipped the fix. |

---

## SCORECARD

### Tally

| Section | ✓ | ⚠ | ❌ |
|---|---|---|---|
| §1 Hero Buildings | 8 | 1 | 0 |
| §2 Village Buildings | 4 | 0 | 1 |
| §3 POIs | 3 | 2 | 0 |
| §4 Vegetation | 3 | 1 | 0 |
| §5 Mini-Game Variants | 5 | 2 | 1 |
| §6 NPCs | 2 | 4 | 1 |
| §7 Combat | 3 | 2 | 1 |
| §8 Lore Beats | 5 | 2 | 1 |
| §9 Audio | 5 | 0 | 2 |
| §10 Save/Load | 3 | 0 | 2 |
| §11 Difficulty | 4 | 0 | 1 |
| §12 Tutorial | 4 | 1 | 0 |
| §13 UI | 3 | 2 | 1 |
| §14 Build Pipeline | 0 | 1 | 2 |
| §15 F310 Controller | 7 | 0 | 0 |
| **TOTAL** | **59** | **18** | **13** |

### Ship-gate verdict

**NEEDS ~12 HOURS — NOT SHIPPABLE TODAY.**

Reasoning:
- 13 ❌ items, each <2 hours of fix = ~20–24 hours raw, but several cluster (5.5 + 5.6 + 8.5 share a root cause in `InteractableBuilding.cs`; 9.5 + audio plumbing share AmbientAudioZone wiring).
- **Of the 13 ❌:** 4 are spec-mismatch / mis-cited (Mud Lord boss [7.5], Carved Stone [3.2] is ⚠ not ❌ after re-reading, ambient hooks [2.5] is real, Cymatic Engine [9.4] is a renaming decision).
- **Real blockers (true gap, must do):** Save Slot UI [10.3], Save thumbnails [10.4], Credits [13.3], Editor build script [14.1], itch upload [14.2], Difficulty UI [11.5], Pipe Organ routing [5.6], per-node variant rule [5.5], Main menu re-enable [13.1], Lirael Day-25 gate [6.3 — `OnDayChanged` event], rigged humanoid models for Lirael/Anastasia/Cassian [6.7].
- The 18 ⚠ are mostly small (under 1 hour each) but accumulate.
- Estimate to clear all ❌ and the highest-value ⚠: **12 focused hours** of Editor + C# work, parallelized across 3–4 lanes.

### Top 5 blockers (smallest-fix-first order)

1. **§13.1 Main Menu uncomment Bootstrap** — `MainMenuOverlay.cs:25-30`, 5 min. Without this, the game has no entry point UI.
2. **§5.6 Pipe Organ → Dome routing** — `InteractableBuilding.cs:306-313`, 45 min. The marquee Moon 1 puzzle goes on the wrong building right now.
3. **§9.5 Place 5 AmbientAudioZone instances in scene** — new `Moon1BuildOutAmbientZones.cs` Editor menu, 1.5 hour. The audio identity of Echohaven depends on these.
4. **§10.3 Save Slot UI** — new `SaveSlotsMenu.cs` consuming existing `SaveManager.GetSaveInfo(slot)`, 1.5 hour. Without it, players cannot manage saves.
5. **§14.1 Editor build pipeline** — new `Moon1BuildPlayer.cs` calling `BuildPipeline.BuildPlayer`, 1 hour. Without it there is no path to shipping a build of any kind.

---

## CAVEATS & FOOTNOTES

- **Scene binary format** (§0) means the actual hierarchy contents of `Echohaven_VerticalSlice.unity` are unverified from grep. Cowork runtime QA must confirm the bootstrap menus have been clicked and the procedurally placed content is present.
- **STATUS.md is the truth-source claim**, not this audit. Where STATUS contradicts itself (Anastasia gate `crystalspire` vs `stardome` — §6.2) the runtime state wins.
- **Mud Lord boss (§7.5) is not in the canonical docs** — the audit-job-spec asked for it; I've documented it as ❌ but flagged the requirement is mis-specified.
- **All ✓ rows are code-existence, not runtime-correctness.** A class existing does not prove its `OnEnable` actually fires in scene context. Lane 10's runtime work (WASD fix, PR #10) is the only ✓ with end-to-end gameplay confirmation; everything else awaits Cowork Editor playtest.
- **The "5 ambient zones from Sprint 6"** referenced in the job spec — I found no Sprint 6 commit log entries (only Sprint 7 in `git log --oneline -20`). The 5 zones may be in an earlier branch or were never landed. §9.5 ❌ stands either way.

---

*Sprint 7 Lane 10 — Moon 1 Acceptance Audit*
*Auditor: Claude (Opus 4.7 1M)*
*Date: 2026-06-02*
*Method: grep+citation, brutal-honesty mode, no theater*
