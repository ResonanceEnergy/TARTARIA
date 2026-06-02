# Sprint 11 — Full Scope Moon 1 Punch List

**Date:** 2026-06-02
**Branch:** `agent/audit/sprint11-synthesis`
**Worktree:** `C:\dev\_wt_s11_l10_synth`
**Base trunk SHA:** `e07660306026c2da2a1c222f26189c99a8fc4a3c` (the post-`RuntimeSpawnerInsurance`-restore fix)
**Method:** Synthesis of 9 parallel Sprint 11 audit lanes (L1–L9) cross-referenced against the v2 acceptance audit (`docs/audits/MOON1_ACCEPTANCE_2026-06-02_v2.md`) and the unmerged Sprint 6/7/8 fix branches sitting on origin.

> This is the full-scope triage doc NATRIX asked for. No marketing language. No theater. Every claim cites a `file:line` or a sibling lane. The order top-to-bottom is the order to fix.

---

## 0. Executive summary — what Moon 1 actually is, today

The audit lanes converged on a hard truth: **Moon 1 is not a playable game today.** The v2 acceptance audit's 70/88 ✓ rate measured against design-doc deliverables; the Sprint 11 lanes measured against **does the code actually do the thing at runtime**. Those two scores diverge violently.

### What is genuinely 100% (with grep evidence)

These are sub-systems with healthy publisher/subscriber pairs in L9 + no stubs in L1 + no commented-out wiring in L3 + no empty catches blocking the path in L2 + scene-baked or correctly-rescued in L4/L8:

| Sub-system | Evidence |
|---|---|
| **OnBuildingRestored event chain** | L9: 5 publishers, 27 subscriber sites (`InteractableBuilding.cs:647`, `CathedralRestorationSystem.cs:187`, `DomeRestorationSystem.cs:23`, `FountainRestorationSystem.cs:22`, `SpireRestorationSystem.cs:22`). Healthy. |
| **F310 controller input wiring** | v2 §15 carried forward 7/7 ✓; L4 confirms `PlayerInputHandler.cs` ships the fallbacks even with InputAction asset bound. (One in-file bypass remains — see PUNCH item 8.) |
| **Save / Cloud conflict + critical save events** | L9: `OnCloudConflictDetected` (pub=1, sub=2), `OnCriticalSaveTrigger` (pub=14, sub=4), `OnHUDAchievementToast` (pub=10, sub=2), `OnHUDCloudQueueToast` (pub=14, sub=2). All healthy. |
| **Quest status change event** | L9: `OnQuestStatusChanged` healthy (pub=6, sub=8). |
| **Inventory event chain** | L9: `OnInventoryChanged` healthy (pub=12, sub=8). |
| **17th-hour cathedral light eruption (event itself)** | L9: `OnSeventeenthHour` (pub=1 at `TartarianHourCycle.cs:90`, sub=4 in NarrativeBeats + CinematicMoments). Event fires — but the consumers swallow exceptions silently (PUNCH #6). |

### What is silently broken (looks done; isn't)

Per L1 + L2 + L9 + L7 cross-referenced:

| Sub-system | Symptom | Root |
|---|---|---|
| **Milo tutorial dialogue** | Every tutorial line warns `"No Yarn node registered for speaker 'Milo'"` and never displays | L7: speaker map at `YarnTutorialBinding.cs:31-36` keys `"Milo Brightway"` → `Milo_TutorialIntro` (both don't exist); caller passes `"Milo"` (defined `MiloTutorialFlow.cs:78`). **4/4 default map entries dead, 1/1 caller speaker mismatched.** |
| **Inventory pickups** | Every Moon 1 collectible logs "Inventory full" and never destroys | L1 B-04: `PickupInteractable.cs:55` hardcodes `bool added = false;`. |
| **HUD live data** | RS counter shows "RS: 0", Aether meter shows "75%" forever | L1 B-06: `HUDLiveDataWiring.cs:50,69-71` writes placeholders in `Update()` AND `OnPlayerHealthChanged` / `OnAetherEnergyChanged` have **zero publishers** per L9 (subscribers wait forever). |
| **Resonance Score economy** | Harmonic Strike + Frequency Shield never spend RS; Resonance Pulse never grants it | L3 🔴: `PlayerAbilityController.cs:101,142` (spend lines commented), `PlayerAbilityManager.cs:116,133` (cast pipeline logs "Would consume" instead of consuming), `PlayerAbilities.cs:49` (TODO never wired). |
| **Cymatic Water Tuning Mini-Game** | Mini-game starts but does nothing measurable | L1 B-01: 7 empty method bodies in `CymaticWaterTuningMiniGame.cs` (`OnTuningInput`, `EnsurePermanentCymaticVisuals`, `PulseFountainCrystals`, `FinishCymatic`, `UpdateAccuracy`, `UpdateCymaticPattern`, `HandleInput`). Self-described "Minimal stub for clean Moon 1 build". |
| **Bell Tower Sync Mini-Game** | Visual only — no bell tones, no audio cue | L1 B-09: `BellTowerSyncMiniGame.cs:74,103,151,163` — four `// TODO: PlaySFX` markers. |
| **Lirael Day-25 reveal** | Event chain doesn't exist | L9: **`OnDayChanged` event is NOT declared in `GameEvents.cs`** (CLAUDE.md claims it; code disagrees). `Moon1LiraelDay25Gate.cs:7`, `Moon1BuildOutNPCs.cs:63,89`, `Moon1DaySmokeMenus.cs:44,66,128` all TODO-waiting for it. |
| **Anastasia brazier ring** | Event chain doesn't exist | L9: `OnBrazierLit`, `OnBrazierRingComplete` not declared. CLAUDE.md canonical-facts table is wrong. |
| **Cathedral light eruption (consumer side)** | Subscribe-throw swallowed silently; eruption RS payout + HUD banner dropped if Raise throws | L2 #1–#6: `Moon1NarrativeBeats.cs:24,25,43,44,75,76` — 6 empty `catch {}` on the headline cinematic. |
| **Restoration dolly + 17th-hour camera move** | Subscribe-throw swallowed silently | L2 #7–#8: `Moon1CinematicMoments.cs:32-33,38-39` — 4 empty `catch {}`. |
| **Adaptive music layer 2** | Combat/tuning music layer never engages if any subscribe throws | L2 #14–#15: `AdaptiveMusicController.cs:431-435,442-446` — 10 empty `catch {}`. |
| **Boss HUD (entire)** | 6 events declared, subscribers ready, **zero publishers** | L9 broken: `OnHUDShowBossHealth`, `OnHUDUpdateBossHealth`, `OnHUDHideBossHealth`, `OnHUDShowBossNameplate`, `OnHUDShowEnemyBark`, `OnHUDShowCorruptionWhisper`, `OnHUDShowMoonTrophy`, `OnHUDFlashRSGain` — 8 events × 2 subscribers each waiting forever. |
| **Player health bar / heals** | Heals never reflect in HUD | L9: `OnPlayerHealthChanged` (pub=0, sub=2) — heals never raised. Damage path works via `OnPlayerDamaged`. |
| **Player death / respawn UI** | Death overlay never clears; respawn doesn't re-enable input | L9: `OnPlayerDeath` (pub=2, sub=0), `OnPlayerRespawned` (pub=2, sub=0) — publishers fire into void. |

### What is 80% but blocked by one specific defect

| Sub-system | Block |
|---|---|
| **Pipe Organ Dome routing (Variant C)** | Fix exists on `origin/agent/fix/pipe-organ-routing` SHA `85580768`. **Not merged to trunk.** v2 §5.6, §8.5. |
| **Main Menu bootstrap** | Fix exists on `origin/agent/fix/main-menu-bootstrap` SHA `bd0bcbf0`. **Not merged to trunk.** v2 §13.1. |
| **Save Slot UI (Load Game / Continue)** | Fix on `origin/agent/fix/save-slots-menu` SHA `a7891ca6`. **Not merged.** Also DUP-collides with `origin/agent/ui/save-slot-ui` SHA `fdcdbccd` and `origin/agent/save/thumbnail-pipeline` SHA `ec3747a5` — three competing implementations need reconciliation. |
| **5 Ambient zones** | Fix on `origin/agent/fix/ambient-zone-placement`. **Not merged.** v2 §9.5. |
| **Per-node A/B/C tuning variant rule** | Fix exists **local-only** on `agent/fix/per-node-tuning-variant`, never pushed to origin. v2 §5.5. |
| **NPC visual rebind (Lirael/Anastasia/Cassian)** | Blender generators pushed on `origin/agent/content/npc-blender-models` but FBX outputs not generated, prefabs not re-pointed. v2 §6.7. NPCs render as capsule primitives per L6. |
| **Hero buildings + Cathedral kit** | All 18 Cathedral pieces share a 16-char truncated material GUID `d4f8e2c9a7b3f5e1` (should be 32 hex). L6: every Cathedral mesh renders magenta + uses URP error shader. |
| **All 347 `Prefabs/Moon1/Blender/*.prefab`** | Source FBX files are 130-byte Git LFS pointers. `git lfs pull` has never been run in any work-tree. L6 §1.1. |

---

## 1. THE PUNCH LIST

### 1.A 🔴 SHIP-BLOCKING — player cannot finish Moon 1 without these

Ordered top-to-bottom by impact on the Moon 1 happy path (tutorial → first building restored → second building restored → final cathedral eruption).

---

#### 🔴 #1 — Dialogue speaker map is 100% broken (tutorial doesn't speak)

**Lane:** L7 (Dialogue Speaker → Yarn Map)
**Files:** `Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:31-36` + `Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:78,209,215,222,229,235,241,280`
**Impact:** Every single Milo tutorial line emits `[YarnTutorialBinding] No Yarn node registered for speaker "Milo"` and **no dialogue appears on screen**. Moon 1's first 10 minutes are silent.
**Root cause (L7 §4.1, §4.2):**
- Caller (`MiloTutorialFlow.cs:78`) passes `"Milo"`.
- Map keys are `"Milo Brightway"`, `"Lirael"`, `"Anastasia"`, `"Cassian"`.
- Map values (`Milo_TutorialIntro`, `Lirael_Lullaby`, `Anastasia_Greeting`, `Cassian_BossIntro`) point to **non-existent Yarn nodes**. Actual nodes use `snake_case_lower` (e.g. `milo_tutorial_step_1_brazier`, `lirael_first_meet`, `anastasia_greeting`, `cassian_first_meet`).
- Yarn's `NodeExists` is case-sensitive.
**Effort:** 5 min for the 4-line fix in L7 §6.1. **Plus** a ~2 h structural follow-up to add `(speaker, message) → node` second-level lookup so steps 2–7 reach their `milo_tutorial_step_{2..6}_*` nodes instead of all firing step 1.
**Fix recommendation (verbatim L7 §6.1):**
```csharp
private static readonly Dictionary<string, string> DefaultSpeakerToNode =
    new Dictionary<string, string>
    {
        { "Milo",      "milo_tutorial_step_1_brazier" },
        { "Lirael",    "lirael_first_meet"            },
        { "Anastasia", "anastasia_greeting"           },
        { "Cassian",   "cassian_first_meet"           },
    };
```

---

#### 🔴 #2 — Inventory pickups are universally broken

**Lane:** L1 (Stubs)
**File:** `Assets/_Project/Scripts/Integration/PickupInteractable.cs:55`
**Impact:** EVERY Moon 1 collectible (Aether shards, lore artifacts, skeleton keys via the pickup path) logs `[Pickup] Inventory full` and is never destroyed. The player picks up the same item N times, the inventory never updates, the world never empties.
**Root cause:** `bool added = false; // DISABLED: InventorySystem.Instance.Add(itemId, itemCount)` — hardcoded false.
**Effort:** 1-line fix.
**Fix recommendation (L1 #1):**
```csharp
bool added = InventorySystem.Instance != null
          && InventorySystem.Instance.AddItem(itemId, itemCount);
```

---

#### 🔴 #3 — HUD live data shows placeholder strings forever

**Lane:** L1 + L9
**Files:** `Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:50,69-71` + `GameEvents.cs:143` + `GameEvents.cs:149`
**Impact:** RS counter is locked at `"RS: 0"`, Aether meter at `0.75 / "Aether: 75%"`. Even when events fire `UpdateRSCounter` / `UpdateAetherMeter`, the `Update()` polling overwrites them back the next frame.
**Root cause:** L1 B-06. Compounded by L9: `OnPlayerHealthChanged` and `OnAetherEnergyChanged` are **broken — 0 publishers, 2 subscribers each**. HUD waits forever for events that are never raised.
**Effort:** 30 min — delete placeholder writes in `UpdateAllDisplays()`, wire `PlayerHealthController` to call `GameEvents.RaisePlayerHealthChanged` on heal/damage, wire `AetherFieldManager.UpdateAether` to call `RaiseAetherEnergyChanged`.

---

#### 🔴 #4 — Resonance Score economy is 100% decorative

**Lane:** L3 (Commented-out code)
**Files:**
- `Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:101` (Harmonic Strike spend)
- `Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:142` (Frequency Shield spend)
- `Assets/_Project/Scripts/Integration/PlayerAbilityManager.cs:116,133` (cast pipeline logs `"Would consume {rsCost} RS here"`)
- `Assets/_Project/Scripts/Gameplay/PlayerAbilities.cs:49` (`ChannelResonance` never raises `GameEvents.FireRSChange`)
**Impact:** Player can spam Harmonic Strike + Frequency Shield infinitely. Resonance Pulse never grants RS to the HUD. Combat economy is dead. Per CLAUDE.md mandate rule #1 (`NEVER ship a file with method bodies that only contain Debug.Log("not implemented yet")`), `PlayerAbilityManager.cs:133` is a literal mandate violation.
**Effort:** 1.5 h — implement `ResonanceScoreTracker.ConsumeRS(float)` + `HasRS(float)` (currently marked `// Note: ConsumeRS() API pending`), then wire from all 4 sites. Single PR.

---

#### 🔴 #5 — Cymatic Water Tuning Mini-Game is empty stubs

**Lane:** L1 (Stubs B-01)
**File:** `Assets/_Project/Scripts/Gameplay/CymaticWaterTuningMiniGame.cs:11,33-34,64-69`
**Impact:** Moon 1 spec §9 requires 3 Tuning mini-game variants. This is one. The class doc says verbatim *"Minimal stub for clean Moon 1 build"*. 7 empty methods: `OnTuningInput`, `EnsurePermanentCymaticVisuals`, `PulseFountainCrystals`, `FinishCymatic`, `UpdateAccuracy`, `UpdateCymaticPattern`, `HandleInput`. Accuracy is set to 0 and never updated. No input read. No visuals pulse.
**Direct mandate violation** (CLAUDE.md 2026-05-30 rules #1 and #2).
**Effort:** ~150 LOC (~3 h). `HandleInput()` reads L-stick + D-pad LR per F310 map; `UpdateCymaticPattern()` drives fountain shader; `FinishCymatic()` raises `GameEventsPhase2.FireTuningNodeActivated("Fountain")`; `UpdateAccuracy()` writes `_bestAccuracy`.

---

#### 🔴 #6 — Cathedral 17th-hour eruption silently swallows errors (headline beat)

**Lane:** L2 (Silent fails)
**File:** `Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:24,25,43,44,75,76`
**Impact:** The headline Moon 1 cinematic. 6 empty `catch { }` blocks across the file. If the event subscribe at OnEnable throws, the 17th-hour cathedral light eruption never fires. If `RaiseHUDShowObjective` throws, the player never sees the eruption banner. If `FireRSChange(20f)` throws, the 20 RS payout is silently dropped. Each of the 8 skeleton keys is similarly silent-fail.
**Root cause:** Per CLAUDE.md "no silent catches" mandate, all 6 are mandate violations.
**Effort:** 15 min — mechanical sweep, every `catch { }` becomes `catch (System.Exception ex) { Debug.LogError($"[Moon1NarrativeBeats] X failed: {ex}"); }`.

---

#### 🔴 #7 — Restoration dolly + 17th-hour camera move silently dies

**Lane:** L2 (Silent fails)
**File:** `Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs:32-33,38-39`
**Impact:** Same pattern as #6. If event subscribe throws on `OnBuildingRestoredTyped` or `OnSeventeenthHour`, the restoration-dolly cinematic and seventeenth-hour camera move never trigger. Both are headline Moon 1 beats.
**Effort:** 15 min — same pattern as #6.

---

#### 🔴 #8 — Player input has no state guards (player walks during cutscenes / pause / dialog)

**Lane:** L4 (Workarounds) row #5
**File:** `Assets/_Project/Scripts/Input/PlayerInputHandler.cs:215-217`
**Impact:** `Update()` no longer gates `HandleMovementInput()` behind `GameStateManager.Instance?.IsPlaying`. During cutscenes, pause, dialog, the tuning mini-game, and the dead state — the player can still walk. Comment block reads `// EMERGENCY BYPASS: Always allow movement for debugging` with the original guard preserved as a comment.
**Root cause:** `GameStateManager` initial state was `MainMenu` instead of `Playing` when the bypass was added.
**Effort:** 20 min — restore the original `if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;` guard, add an allow-list `GameState.Playing | GameState.Combat | GameState.Tuning`, verify `GameStateManager.SetState(GameState.Playing)` is called from `EchohavenContentSpawner.Awake`.

---

#### 🔴 #9 — Echohaven scene `Moon1_Systems` ghost-script spam

**Lane:** L5 (Orphan deep-clean) + L4 row #1 + L8 §3 item 10
**File:** `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (lines 305, 1252, 2974–3056, 3090, 3364)
**Impact:** Every domain reload spams `[CleanMissingScripts] Removing missing script from: Moon1_Systems` and never actually removes it. The four `!u!114 MonoBehaviour` orphans (referencing dead classes `Moon1AmbientCreatures`, `Moon1HeroBuildingSpawner`, `Moon1MaterialSetup`, `Moon1NPCSpawner`) plus their four `!u!115 MonoScript` stubs persist in YAML. **Possible Error Pause trap** (CLAUDE.md flags this exact failure mode: any `Debug.LogError` on init + Error Pause toggle ON = every Play enters paused at frame 1, looks like input is dead).
**Root cause:** L5 §"Root cause" — Unity's managed API doesn't iterate the embedded MonoScript blocks; YAML survives every "fix."
**Fix:** **Already implemented.** `agent/fix/moon1systems-orphan-deep-clean` SHA `9500ccfe` adds the menu `Tartaria / 8 Fix / Deep-Clean Moon1_Systems Prefab` with YAML surgery. **Not merged to trunk.** Run the menu once, verify scene re-opens clean.
**Effort:** 5 min (run menu + verify + merge branch).

---

#### 🔴 #10 — Three master "do everything" Editor menus disagree (half-authored scene)

**Lane:** L8 §5.1 + §1
**Files:** `Editor/Moon1MasterBootstrap.cs:28`, `Editor/Moon1Tier1Master.cs:25`, `Editor/Moon1AllTiersMaster.cs:23`
**Impact:** `Bootstrap All Moon 1 Systems` adds the 12 Moon1_Systems components but does NOT chain Tier 1. `Run ALL Tiers` runs Tier 1 + VFX + Audio but skips Bootstrap. No single menu both bootstraps the systems AND bakes the buildings/NPCs/props. Users routinely run one and skip the other → the scene saved in trunk is missing `Hero_Buildings`, `Village_Buildings`, `Echohaven_NPCs`, `Excavation_Sites`, `Quest_Triggers`, `Obelisk` — none of which are in the 48-name scene census per L8 §6.
**Effort:** 30 min — add a `Tartaria/0 ★ MASTER/Bake Moon 1 EVERYTHING (Systems + Tiers + BuildOuts + Wire)` umbrella menu that sequences all three. Then RUN IT and save the scene.

---

### 1.B 🟡 CONTENT / POLISH — works but rough (blocks ship-quality, not ship-completability)

#### 🟡 #11 — Every Cathedral prefab has a truncated 16-char material GUID

**Lane:** L6 §2a + §4 row 9
**Files:** All 18 `Assets/_Project/Prefabs/Moon1/Cathedral/*.prefab`
**Impact:** Material GUID `d4f8e2c9a7b3f5e1` is 16 hex chars; valid Unity GUIDs are 32. Every Cathedral mesh renders **magenta** at runtime via the URP error shader.
**Effort:** 20 min — open one prefab in Inspector, re-assign URP/Lit material, save, propagate to siblings.

#### 🟡 #12 — Three Echohaven hero buildings + 4 NPCs + Player are primitives only

**Lane:** L6 §2a + §3 + §4
**Files:**
- `Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab`
- `Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab`
- `Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab`
- `Assets/_Project/Prefabs/Characters/Player.prefab` (capsule, 0 MonoBehaviours, no `PlayerInputHandler`)
- `Assets/_Project/Prefabs/Characters/{Anastasia,Lirael,Cassian,Milo}.prefab` (all capsules, 0 MonoBehaviour, no NavMeshAgent, no Animator)
- `Assets/_Project/Prefabs/Characters/{Korath,Thorne,CrystalSentry,ShadowStalker}.prefab` (4 728-byte empty PrefabInstances)
**Impact:** Per CLAUDE.md late-night mandate rule #4 (`NEVER use GameObject.CreatePrimitive`), these are mandate-violation prefabs. Visible to player as boxes/capsules.
**Pre-req for fix:** `git lfs install && git lfs pull` (see #13).
**Effort:** ~2 h after LFS lands — rebuild Player.prefab from KayKit Knight, rebuild NPCs as Prefab Variants of resolved Blender FBX.

#### 🟡 #13 — Every FBX in repo is a 130-byte Git LFS pointer

**Lane:** L6 §1.1
**Files:** All 1,269 `.fbx` files under `Assets/`.
**Impact:** All 347 `Prefabs/Moon1/Blender/*.prefab` resolve to empty named GameObjects at runtime. All 12 KayKit character variants render empty. The `BlenderImportPostprocessor` (the auto-variant generator) is inert because every FBX it processes is a stub.
**Effort:** 2 min `git lfs install && git lfs pull` in every active worktree. **Then** re-run `Tartaria → Moon 1 → Run Blender Batch`. Verify `[System.IO.File]::ReadAllBytes('Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx').Length` is ~46,940, not 130.

#### 🟡 #14 — Boss HUD entire surface area is dead UI code

**Lane:** L9 (broken events)
**Files:** `GameEvents.cs:261,267,273,297,303,309,255,291` (8 events) + `HUDController.cs:150-159,186-195` (16 subscribe sites)
**Impact:** 8 boss-HUD events declared, 16 subscribers ready, **0 publishers**. `OnHUDShowBossHealth`, `OnHUDUpdateBossHealth`, `OnHUDHideBossHealth`, `OnHUDShowBossNameplate`, `OnHUDShowEnemyBark`, `OnHUDShowCorruptionWhisper`, `OnHUDShowMoonTrophy`, `OnHUDFlashRSGain` — every one waits forever.
**Effort:** 1 h — wire publishers in the Integration system that owns each trigger (BossEncounterSystem → raise boss HUD; QuestManager → raise moon-trophy; CorruptionSystem → raise corruption whisper). Or, if Moon 1 has no boss, mark dead and skip.

#### 🟡 #15 — Player death + respawn never closes the loop

**Lane:** L9
**Files:** `GameEvents.cs:130,137` + `PlayerHealthController.cs:170,171,214,216`
**Impact:** Publishers fire `OnPlayerDeath` and `OnPlayerRespawned` into void. No subscribers re-enable input, clear death overlay, or fade camera. Moon 1 has Mud Golems → dying happens → player gets stuck on a "you died" screen the HUD never paints.
**Effort:** 30 min — subscribe `HUDController.HandlePlayerDeath` + `CameraController.OnPlayerDeath` + `PlayerInputHandler.OnPlayerRespawned`.

#### 🟡 #16 — Cinematic invulnerability is a Debug.Log

**Lane:** L1 (Stubs B-07)
**File:** `Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:255-260`
**Impact:** `SetInvulnerable(value)` logs but ignores its argument. Moon1CinematicMoments + Moon1NarrativeBeats cannot freeze player damage during the cathedral-raise sequence. If a Mud Golem swings during the headline cinematic, the player dies during the cutscene.
**Effort:** 3 min — set `IsInvulnerable = value`, clear/set `_invulnerabilityEndTime` accordingly.

#### 🟡 #17 — Aether Field samples from spawn point forever

**Lane:** L1 (Stubs B-05)
**File:** `Assets/_Project/Scripts/Core/AetherFieldSystem.cs:45-46`
**Impact:** Y-button Aether Vision toggle (per F310 map) renders Aether grid from (0, 1, 0) regardless of where the player is. Walking >~20m from spawn makes the Vision toggle visibly wrong.
**Effort:** 5 min — replace hardcoded `float3(0,1,0)` with `PlayerSpawner.PlayerInstance?.transform.position`.

#### 🟡 #18 — Bell Tower Sync Mini-Game has no audio

**Lane:** L1 (Stubs B-09)
**File:** `Assets/_Project/Scripts/Gameplay/BellTowerSyncMiniGame.cs:74,103,151,163`
**Impact:** "Sync" mini-game by definition needs bell tones. Visual flash only — player can memorise the flash sequence but the framing makes no sense.
**Effort:** 10 min — 4-line `AudioManager.Instance?.PlaySFX2D("BellTone_" + bellIndex)`.

#### 🟡 #19 — NavMeshBaker.cs is sed-corrupted

**Lane:** L1 (Stubs B-02)
**File:** `Assets/_Project/Scripts/Integration/NavMeshBaker.cs:15,21-26,38-40`
**Impact:** TODO comment was inserted inside the `/* */` type-substitution block and got doubled on each Edit-tool pass. The field declaration is malformed. The file is gated `#if UNITY_AI_NAVIGATION` so it doesn't break compile today, but the moment that define is set the file will not parse. Moon 1 NPC pathing currently requires manual Editor-menu NavMesh bake.
**Effort:** 20 min — either delete the file or hard-restore the canonical `NavMeshSurface` declaration.

#### 🟡 #20 — Moon1PlayerSetup uses `AddComponent<MonoBehaviour>()` (abstract — throws at runtime)

**Lane:** L1 (Stubs B-03)
**File:** `Assets/_Project/Scripts/Integration/Moon1PlayerSetup.cs:83,138-148`
**Impact:** `AddComponent</* DISABLED: TartariaCameraController */ MonoBehaviour>()` lexes to abstract-add → throws `ArgumentException` at runtime. The `SimpleCameraFollow` fallback saves the player from a black screen but the intended camera controller never attaches. Falls into L4 row #2's "competing player-setup paths" category.
**Effort:** 15 min — either re-enable `TartariaCameraController` (preferred) or commit to `SimpleCameraFollow` and delete the dead code.

#### 🟡 #21 — Adaptive music layer 2 silently dies on any subscribe throw

**Lane:** L2 (Silent fails) #14–#15
**File:** `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:431-435,442-446`
**Impact:** 10 empty `catch { }` around event subscriptions. Combat/tuning music layer stuck at exploration layer 1 if any wiring throws. Player never hears the music swell when combat starts.
**Effort:** 5 min — mechanical sweep.

#### 🟡 #22 — Tuning pedestal prompts swallow errors silently

**Lane:** L2 (Silent fails) #9–#11
**File:** `Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:28,35,66`
**Impact:** Tutorial-blocking. Without the "Press [E] to tune" prompt, player doesn't know they can tune. Tuning is the Moon 1 gating mini-game.
**Effort:** 5 min — wrap with logging catches.

#### 🟡 #23 — Quest Objective Tracker UI silently dead

**Lane:** L2 (Silent fails) #12–#13
**File:** `Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs:33-34,39-40`
**Impact:** Canonical Moon 1 progression UI. If `OnBuildingRestored += HandleBuildingRestored` subscribe throws, the tracker never updates and the player has no way to know progress.
**Effort:** 5 min.

#### 🟡 #24 — VoiceLine playback silently no-ops on miss

**Lane:** L2 (Silent fails) #22
**File:** `Assets/_Project/Scripts/Audio/AudioManager.cs:308`
**Impact:** `Resources.Load<AudioClip>("VoiceLines/" + lineId)` returning null logs nothing. Any missing VO line silently no-ops. Explains why Milo tutorial VO regressions go undetected.
**Effort:** 3 min — add `LogWarning` on null.

#### 🟡 #25 — Echohaven scene is a skeleton; 95% rebuilt every Play

**Lane:** L8 (Scene authoring) §1 + §7
**File:** `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`
**Impact:** Scene file has 48 unique GameObjects (lighting, terrain, walls, UI shell, 4 mud mounds, Moon1_Systems host). EVERYTHING else (HUD, NPCs, props, buildings, dig sites, foliage, fireflies, mud pools, braziers, boss visuals, Win/Lose UI, dialogue panels — estimated 400+ GameObjects) is built from scratch on every `[RuntimeInitializeOnLoadMethod]` pass. The most-edited files in recent history (`PlayerSpawner`, `Moon1PlayerSetup`, `RuntimeSpawnerInsurance`, `DialogueManager.AutoBootstrap`) are ALL on the runtime-rescue list — every sibling-agent edit risks breaking a chain.
**Fix recommendation (L8 §7):** Run `Moon1MasterBootstrap` + all `Moon1BuildOut*` + `Moon1WireSpawnerPrefabs` once, save scene, then **delete the matching `[RuntimeInitializeOnLoadMethod]` fallbacks**. The bake-and-prune pass.
**Effort:** 4 h.

#### 🟡 #26 — RuntimeHUDBuilder constructs entire HUD at runtime

**Lane:** L8 §2.1 row 27
**File:** `Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:64,149,169,182,243,274,353,366`
**Impact:** Canvas + EventSystem + HUDController + ControlsHint + QuestToast + DialoguePanel + DialogueSpeaker + text children — all built from raw `new GameObject` + `AddComponent` every Play. Every UI layout regression in this project's history routes through this file.
**Fix:** Bake `Prefabs/UI/MainHUD.prefab` with inspector-tunable layout.
**Effort:** 3 h (one-time bake + prune the builder code).

#### 🟡 #27 — EchohavenContentSpawner is a 3,082-line procedural rebuild of everything

**Lane:** L8 §2.3 (~80 catalogued GameObjects across 11 sub-systems)
**File:** `Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs:178,299-369,395-441,534-584,647-822,1011-1090,1160,1230-1264,1334-1505,1557-1581,2067-2155,2454-2516,2632-2718`
**Impact:** Builds shovel + 12 mud mounds + Milo (procedural primitive) + Cassian (`Cassian_MISSING_PREFAB` fallback) + Anastasia (`Anastasia_MISSING_PREFAB` fallback) + Sky_Aurora + KayKit_Scatter + MudGolem (~25 primitive sub-GOs) + dig markers + corruption zones + ENV props + obelisk details. Every one of those would be a single prefab placement in scene.
**Effort:** 6–8 h.

---

### 1.C ⚪ FUTURE — doesn't block Moon 1 ship

These were captured in the audits but classified as deferred-by-design or Moon 2–13 scope.

| Item | Lane | File:line | Why deferred |
|---|---|---|---|
| Steam SDK stubs (7 sites in `SteamBridge.cs`) | L1 §4 + L3 ⚪ | `Integration/SteamBridge.cs:30,46,47,64,65,73` | Gated by `#if STEAMWORKS` (no scripting-define). Phase-1 ships pre-Steam per `PHASE_1_SCOPE.md`. |
| CrashReporter / FeedbackReporter / PlayerSentimentTracker telemetry (15 sites) | L1 §4 + L3 ⚪ | `Core/{BreadcrumbLogger,FeedbackReporter,PlayerSentimentTracker}.cs` | LiveOps split, deferred per CLAUDE.md. |
| CompressionHelper save compression | L3 ⚪ | `Save/SaveManager.cs:386,462,465,802` | Raw bytes work; compression awaits Serialization assembly. |
| Moon3 / Moon2 / RailEscort cross-Moon TODOs | L1 §4 + L3 ⚪ | `Gameplay/Moon3OrphanTrainPuzzle.cs:183,199,263`, `Integration/_moon2_archive/Moon2ProgressionSystem.cs:917`, `Integration/RailEscortController.cs:1558` | Moon 2+ scope. |
| Frozen-enemy ice-particle VFX | L1 §4 | `AI/EnemyAIController.cs:216` | Polish, not blocking. |
| SkillTree disabled block (32 LOC) | L3 ⚪ | `Gameplay/SkillTreeSystem.cs:352` | Out of `PHASE_1_SCOPE.md`. |
| LeanTween vendor noise (107 hits) | L3 ThirdParty | `ThirdParty/LeanTween/*` | Vendor code. |
| 459 `.cs.disabled` files | L1 §6 | `Assets/_Project/Scripts/**/*.cs.disabled` | Per CLAUDE.md "no Moon 2–13 stub regeneration." Stays archived. |
| 4 UNUSED events (`OnDialogueStateChanged`, `OnMoonUnlocked`, `OnRemotePushNotificationReceived`, `OnTartarianHourChanged`) | L9 | `GameEvents.cs:185,210,331,450` | Dead declarations — delete or document why preserved. Not blocking. |
| All 34 `// SUPERSEDED 2026-05-31` orphan labels | L3 🟡 | 7 files | Sweep + delete in single cleanup PR. Zero behavior risk. Cosmetic. |
| `PlayerWeaponSwitcher.cs` (`OnWeaponChanged` event with 0 listeners) | L3 🔴 #3 | `Gameplay/PlayerWeaponSwitcher.cs:17,18,27,28,62,63` | Either delete or rewire to `PlayerAbilityController.SetActiveWeapon` — no Moon 1 caller depends on it. |
| Night-Aether boost multiplier | L3 🔴 #4 | `Integration/DayNightCycleController.cs:124` | Computed and discarded. Not a Moon 1 player-visible feature today. |
| `AutomatedPrefabWiring.bakeNavMesh` toggle | L3 🔴 #5 | `Editor/AutomatedPrefabWiring.cs:149` | Editor-only, Moons 2–13. |

---

## 2. Cross-reference: Sprint 11 vs unmerged prior-sprint branches

Per the v2 acceptance audit, several Sprint 11 findings are already addressed by branches sitting on origin awaiting merge. Merging them clears trunk progress without new authoring work.

| Sprint 11 punch item | Addressed by branch | SHA | Status |
|---|---|---|---|
| #9 Moon1_Systems orphan spam | `agent/fix/moon1systems-orphan-deep-clean` | `9500ccfe` | **Pushed (this sprint). Merge to trunk.** |
| Dialogue speaker map (#1) | None yet — Sprint 11 finding, no prior-sprint fix exists | — | New PR needed. |
| §5.6 Pipe Organ Dome routing | `agent/fix/pipe-organ-routing` | `85580768` | Pushed; not merged. |
| §13.1 Main Menu bootstrap | `agent/fix/main-menu-bootstrap` | `bd0bcbf0` | Pushed; not merged. |
| §10.3 Save Slot UI | `agent/fix/save-slots-menu` + 2 dupes | `a7891ca6`, `fdcdbccd`, `ec3747a5` | **3 competing implementations — reconcile then merge.** |
| §9.5 5 Ambient zones | `agent/fix/ambient-zone-placement` | `8c6383ac` | Pushed; not merged. |
| §11.5 Difficulty UI | `agent/ui/settings-menu-real` + `agent/ui/pause-settings-extract` | `36738468`, `c1db9d9f` | Pushed; not merged. |
| §13.3 Credits | `agent/narrative/credits-scene` | `6d7f7e6d` | Pushed; not merged. |
| §14.1 Editor build pipeline | `agent/tools/itch-build-pipeline` | `505a9774` | Pushed; not merged. (#14.2 butler push still missing.) |
| §6.2 Anastasia reveal gate | Already in trunk (PR #4) | `59629f03` | ✓ Done. |
| §7.3 Wave spawner | `agent/ai/wave-spawner-tuning` | `6128330d` | Pushed; not merged. |
| §5.5 Per-node A/B/C variant | `agent/fix/per-node-tuning-variant` | local-only | **Never pushed.** Push first. |
| Compile cleanliness | `agent/fix/sprint8-compile-clean` | `f9e3a265` | Pushed; not merged. |
| FindObjectOfType modernisation | `agent/fix/findobjecttype-sweep` | `8a55527b` | Pushed; not merged. |
| InputProbeHUD warn | `agent/fix/inputprobehud-warn` | `8cb50d64` | Pushed; not merged. |
| TagManager dedup | `agent/fix/tagmanager-dedup` | `e51942e9` | Pushed; not merged. |
| Pipe Organ duplicate delete | `agent/fix/pipe-organ-dup-delete` | `b7e937ce` | Pushed; not merged. |
| Lightmap EditorSettings sweep | `agent/fix/lightmap-editor-settings-sweep` | `bf939df7` | Pushed; not merged. |
| Difficulty sApplied guard | `agent/fix/difficulty-sapplied-guard` | `97ebbf3c` | Pushed; not merged. |
| SaveSlot triage | `agent/fix/saveslot-triage` | `22ff33c8` | Pushed; not merged. |

**Bottom line: there are 17+ fix branches sitting on origin that no one has merged.** Trunk `feature/consolidate-moon-architecture` has NOT advanced past `6094136c` since Sprint 8. Sprint 11's audit work compounds that backlog rather than clearing it.

---

## 3. Honest verdict

**Moon 1 is not shippable today.** The v2 acceptance audit's 70/88 ✓ rate is a measurement of "does the feature exist as a file" — Sprint 11's lanes measured "does the feature do the thing at runtime" and the result is:

- 8 silently-broken HUD events (#14)
- 0 working dialogue lines (#1)
- 0 working pickups (#2)
- 0 spending of RS (#4)
- 1 empty-stubs mini-game (#5)
- 1 silently-swallowed cathedral cinematic (#6, #7)
- All 18 cathedral kit pieces magenta (#11)
- All hero buildings + characters + Player primitive-only (#12)
- All Blender FBX files are 130-byte LFS pointers (#13)
- Player has no state guards during cutscenes/pause/dialog (#8)
- Scene is a 48-name skeleton rebuilt from 400+ runtime `new GameObject` calls (#25, #26, #27)

**Estimated work to clear THE PUNCH LIST 🔴 SHIP-BLOCKING section (#1–#10):**
- Quick wins (#1, #2, #3, #6, #7, #8, #9): ~2 hours total.
- Medium (#4, #5, #10): ~6 hours.
- **Total: ~8 hours of focused engineering** to make Moon 1 minimum-playable.

**Estimated work to also clear the 🟡 CONTENT/POLISH section (#11–#27):**
- LFS pull + Cathedral material GUID fix: ~30 min.
- NPC + Player prefab rebuilds: ~3 hours.
- Bake-and-prune scene authoring (#25, #26, #27): ~12 hours.
- Remaining polish (#14–#24): ~3 hours.
- **Total: ~19 hours additional.**

**Plus** ~3 hours to merge and reconcile the 17 pushed-but-unmerged Sprint 6/7/8 branches.

**Realistic Moon 1 ship-ready estimate: 30 focused engineering hours across 1–2 sprints.** No more architectural rewrites. No more new lanes. **Merge, fix the punch list top-to-bottom, then re-audit.**

---

## 4. Top-10 SHIP-BLOCKING priority order (for NATRIX triage)

| # | Item | Effort | File:line head |
|---|---|---|---|
| 1 | Dialogue speaker map broken (tutorial silent) | 5 min + 2 h follow-up | `YarnTutorialBinding.cs:31-36` |
| 2 | Inventory pickups always fail | 1 min | `PickupInteractable.cs:55` |
| 3 | HUD RS / Aether / health stuck on placeholders | 30 min | `HUDLiveDataWiring.cs:50,69-71` + `GameEvents.cs:143,149` |
| 4 | RS economy 100% decorative | 1.5 h | `PlayerAbilityController.cs:101,142` + `PlayerAbilityManager.cs:116,133` + `PlayerAbilities.cs:49` |
| 5 | Cymatic Water Tuning Mini-Game empty stubs | 3 h | `CymaticWaterTuningMiniGame.cs:64-69` |
| 6 | Cathedral 17th-hour eruption silently swallows errors | 15 min | `Moon1NarrativeBeats.cs:24,25,43,44,75,76` |
| 7 | Restoration dolly + 17th-hour camera silently dies | 15 min | `Moon1CinematicMoments.cs:32-33,38-39` |
| 8 | Player input has no state guards | 20 min | `PlayerInputHandler.cs:215-217` |
| 9 | Moon1_Systems ghost-script spam (Error Pause risk) | 5 min | merge `agent/fix/moon1systems-orphan-deep-clean` + run menu |
| 10 | Three master Editor menus disagree → half-authored scene | 30 min | umbrella menu + run + save |

---

## 5. Top-5 quick wins (high impact, low effort)

| # | Item | Effort | Why high impact |
|---|---|---|---|
| 1 | Pickups (#2) | 1 min | Every Moon 1 collectible suddenly works. |
| 2 | Speaker map (#1, single-line) | 5 min | Tutorial step 1 line displays. (Steps 2-7 need structural follow-up.) |
| 3 | Cathedral eruption catches (#6) + Cinematic catches (#7) | 30 min combined | Headline cinematic stops silently dying. |
| 4 | Cathedral material GUID (#11) | 20 min | All 18 cathedral pieces stop rendering magenta. |
| 5 | `git lfs pull` (#13) | 2 min wait | 1,269 FBX files resolve; unlocks Cathedral + 347 Blender prefabs + 12 KayKit characters + the Player rebuild path. |

---

## 6. Provenance

- **L1 — Stubs/TODOs:** `origin/agent/audit/stub-sweep` SHA `1fb03541` → `docs/audits/SPRINT11_L1_STUBS_2026-06-02.md`
- **L2 — Silent fails:** `origin/agent/audit/silent-fails` SHA `b33eb621` → `docs/audits/SPRINT11_L2_SILENT_FAILS_2026-06-02.md`
- **L3 — Commented code:** `origin/agent/audit/commented-code` SHA `1587acab` → `docs/audits/SPRINT11_L3_COMMENTED_CODE_2026-06-02.md`
- **L4 — Workarounds:** `origin/agent/audit/workarounds` SHA `f30f9a29` → `docs/audits/SPRINT11_L4_WORKAROUNDS_2026-06-02.md`
- **L5 — Moon1_Systems orphan deep-clean:** `origin/agent/fix/moon1systems-orphan-deep-clean` SHA `9500ccfe` → `docs/audits/SPRINT11_L5_ORPHAN_ROOT_CAUSE_2026-06-02.md`
- **L6 — Prefab integrity:** `origin/agent/audit/prefab-integrity` SHA `e9bbc612` → `docs/audits/SPRINT11_L6_PREFAB_INTEGRITY_2026-06-02.md`
- **L7 — Dialogue speaker map:** `origin/agent/audit/dialogue-speaker-map` SHA `cec511a9` → `docs/audits/SPRINT11_L7_DIALOGUE_MAP_2026-06-02.md`
- **L8 — Scene authoring:** `origin/agent/audit/scene-authoring` SHA `50ff78ea` → `docs/audits/SPRINT11_L8_SCENE_AUTHORING_2026-06-02.md`
- **L9 — GameEvents pair audit:** `origin/agent/audit/gameevents-pairs` SHA `72457de3` → `docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.md` + `.csv`
- **v2 acceptance:** `docs/audits/MOON1_ACCEPTANCE_2026-06-02_v2.md`

---

*Sprint 11 Lane 10 — Full Scope Synthesis. Base SHA `e0766030`. Auditor: Claude (Opus 4.7 1M).*
