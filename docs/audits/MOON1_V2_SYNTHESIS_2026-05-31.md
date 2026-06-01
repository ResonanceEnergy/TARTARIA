# Moon 1 — v2 Audit Synthesis, 2026-05-31

After this session's fix sprint (scene cleanup menu, village placement menu, NPC repositioning, POI placement, FirstTimeHints, AdaptiveMusic Layer 2, SaveData v15 schema, GameEvents additions), 6 parallel agents re-audited Moon 1. Synthesis below.

The session's 15 promised code changes all landed (confirmed by Audit B). The remaining gaps fall into 5 buckets, ordered by urgency.

---

## ⚡ BUCKET 1 — Compile blockers (must fix before Play)

| # | File:Line | Issue | Status |
|---|---|---|---|
| 1 | `Scripts/Audio/AdaptiveMusicController.cs` | Layer 2 fields/methods landed inside `StingerType` enum (C# illegal) | **FIXED this turn** — moved inside class, 88/88 braces, structure verified |
| 2 | `Scripts/Gameplay/PipeOrganMiniGame.cs` + `Scripts/Integration/PipeOrganMiniGame.cs` | Duplicate type `Tartaria.Gameplay.PipeOrganMiniGame` in two files — CS0101 | OPEN. Delete the Integration copy. |
| 3 | `Scripts/Integration/InteractableBuilding.cs` | Calls `_tuningController.StartTuning(TuningPuzzleConfig)` overload that doesn't exist on the current class | OPEN. Either add the overload to TuningMiniGame or change the call site. |

---

## 🚨 BUCKET 2 — Editor menus NOT YET RUN in Unity (the new code is just dormant code)

Audit A and C both confirmed: **none of the 6 new BuildOut menus' parent containers exist in the scene file**. `Hero_Buildings`, `Village_Buildings`, `Echohaven_NPCs`, `Echohaven_Props`, `Echohaven_Vegetation`, `Echohaven_POIs` — zero hits. The menus are wired but you haven't pressed them yet.

Run these in Unity in this order:

1. **`Tartaria → 8 Fix → Moon 1 Scene Cleanup`** — strips the 4 missing-script refs (Moon1NPCSpawner, Moon1AmbientCreatures, Moon1MaterialSetup, Moon1HeroBuildingSpawner) AND deletes the 3 placeholder hero buildings still in the scene. Without this the Editor "Error Pause" trap fires every Play.
2. **`Tartaria → 1 Build → Build Out Moon 1 Village (9 Buildings)`** — fills the empty village (currently only Well + Signpost are baked; 8 buildings missing).
3. **`Tartaria → 1 Build → Build Out Moon 1 NPCs`** — positions Milo at Dome shaft, Anastasia inactive at Spire (reveal-on-restoration), Lirael inactive at Fountain (Day-25 gate TODO), Cassian at village square, Bob at Inn.
4. **`Tartaria → 1 Build → Build Out Moon 1 Environment (POIs)`** — adds Overlook + Root Chamber POIs.
5. **`Tartaria → 1 Build → Build Out Moon 1 Vegetation`** (existing menu) — scatter KayKit foliage. Audit C says only 16 trees placed vs the 120-instance target.
6. **Manual scene cleanup:** delete the 6 wrong-Moon mini-game GameObjects from the scene tree — `LeyLineProphecyMiniGame`, `AquiferPurgeMiniGame`, `BellTowerSyncMiniGame`, `CosmicConvergenceMiniGame`, `RailAlignmentMiniGame`, `HarmonicRockCutting`. Those belong in Moons 2-13 scenes.

---

## 🔧 BUCKET 3 — Wiring fixes (code is half-done)

Each item is small but real. Roughly 10-30 minutes per fix.

- **Anastasia gate is wrong.** `NPCConditionalSpawn` listens for `OnBuildingRestored("echohaven_crystalspire")` but spec wants `echohaven_stardome` (post-Dome restoration). Fix the buildingId string.
- **Two ability controllers fight.** `PlayerCombatController.cs` AND `PlayerAbilityController.cs` both subscribe to `OnHarmonicStrike` / `OnFrequencyShield`. Either delete one or guard with a single-source flag — double-fires hit the enemy twice.
- **Two cinematic systems fight on OnBuildingRestored.** `RestorationCinemachine.cs` AND `Moon1CinematicMoments.cs` both lerp the camera. Pick one.
- **Climactic VFX prefabs are orphaned.** All 4 `VFX_*.prefab` exist on disk but `Moon1NarrativeBeats.CathedralLightEruption` builds the pillar inline with `new GameObject` instead of `Instantiate(VFX_CathedralLightEruption.prefab)`. Same likely for Spire sparks, Giant burst, 17th-hour beam. Swap in `Resources.Load` or `AssetDatabase.LoadAssetAtPath` (Editor-time wiring) calls.
- **AudioCueLibrary.asset is empty `cues: []`.** The 5 Moon 1 stingers on disk are not registered. Populate the SO.
- **TuningMiniGame doesn't fire `OnTuningProgress`.** `AdaptiveMusicController.HandleTuningProgress` is dead wiring until the slider/needle update emits the event each frame. Add `GameEvents.FireTuningProgress(offset)` to Variant A's Update.
- **Quest IDs not registered.** Spec quests `AwakenStarDome / AwakenFountain / RaiseTheSpire` aren't in `QuestSystem`. Only `moon1_meet_milo / moon1_restore_first / moon1_collect_shards` exist. Either rename to match spec or align spec.
- **`QuestObjectiveTrackerUI` not wired to QuestSystem.** It watches building-restored events + PlayerPrefs only. Add a `QuestSystem.OnQuestActivated` subscription.
- **NavMesh: mud pools don't carve.** Per Audit (separate NavMesh report) NPCs walk through mud pools. Add a `NavMeshObstacle` to each pool's collider with `carve=true`.
- **AetherFieldManager `playerApprox = float3.zero` bug.** Per Audit 2 the spatial pre-filter culls against world origin not actual player. 1-line fix.
- **3 haptic patches.** Per Audit 6 — Footstep (FootstepController.cs:99), Golem death (MudGolemHealth.cs:181), Building emergence (AudioFeedbackController.cs:50-53). ~10 LoC total to wire the chain.

---

## 🧱 BUCKET 4 — Authoring gaps (content not yet created)

These need actual content creation, not just wiring.

- **Lirael yarn dialogue file** — `lirael.yarn` doesn't exist. Spec wants the 432Hz lullaby + "why grown-ups live in the attic" line. ~10-15 nodes.
- **Cassian yarn dialogue file** — `cassian.yarn` doesn't exist. Spec wants foreshadow lines that don't yet contradict the false-ally reveal. ~8-10 nodes.
- **`Lirael.prefab.corrupt` + `Cassian.prefab.corrupt`** are corrupt files. Both need to be re-exported from KayKit or re-authored.
- **`CassianController.cs`** does not exist. Need a minimal MonoBehaviour for wander + dialogue trigger.
- **Milo dialogue 11/40 → 40/40.** Per Audit 4: Milo has 11 of his 40 spec lines (tutorial 10 / discovery 8 / lore 8 / ambient 8 / combat 4 / celebration 2). The 5 missing categories are EMPTY.
- **`PipeOrganCathedral.prefab` not placed in scene.** Visual model exists in Blender prefabs but the `PipeOrganMiniGame` trigger GameObject has no visual.
- **No "Purified Mud" loot** — spec says Mud Golem drops 3× Purified Mud; code drops generic `aether_shard`. Either add the item or align spec.
- **18 Cathedral kit pieces — 0/18 placed.** Per Audit 5 the dome itself is a single placeholder + `Building_echohaven_stardome` shell. The 8 dome segments, archway, columns, walls, foundation, rose window, spire pieces are all on disk but not placed. The cathedral has rich INTERIOR dressing (5/5 sacred geometry) but no structural KIT use.
- **Spire pieces 0/3** — spire is custom GameObjects (`Spire_AetherBase`, `Spire_Crown`, etc.) instead of the kit `Spire_Base / Mid_Taper / Top_MercuryBall` prefabs.

---

## 🗑️ BUCKET 5 — Code hygiene (low priority but compounds mess)

- **Duplicate `MudGolemHealth`** classes — `Tartaria.AI.MudGolemHealth` (real, 263 lines) AND a hidden `Tartaria.Integration.MudGolemHealth` (~80 lines) inside `EchohavenContentSpawner.cs`. Delete the Integration copy.
- **`EchohavenContentSpawner.cs` is 3168 lines** hiding 8 unrelated MonoBehaviours (PerfImpostorBillboard, AetherShardPickup, DigSiteInteraction, ShovelPickup, MiloInteractable, etc.). Split.
- **`Phase2Stubs.cs` still has 10 stub classes** in 254 lines. CLAUDE.md no-stubs mandate violation.
- **`Moon1QuestTriggers.cs:111`** has literal `// TODO: Integrate with NotificationSystem`.
- **`Moon1GodMode.cs` (6L) and `Moon1HardOverrideDriver.cs` (11L)** are "superseded" marker stubs still in the live Integration folder. Move to `_archived_2026_05_31_stub_deletions/`.
- **Wrong-Moon mini-game shells in scene** (already listed in Bucket 2 as manual scene cleanup).
- **`Moon1QuestTriggers` Milo zone at `(-40,0,20)`** is offset 14m from the actual Milo NPC at `(-26,0,26)`. Recenter.
- **`PlayerInputHandler.interactRadius = 3.0f`** vs spec 5m. Adjust.
- **Mud Golem two parallel HP systems** — `MudGolemAI.maxHealth=50` and `MudGolemHealth._maxHealth=300` both intercept `TakeDamage`. Pick one.
- **`MudGolemAI` telegraph 0.5s** vs spec 1s. Adjust.
- **Mud dissolution shader + 3 materials exist but no script animates `_Dissolution`** over time. Wire it.

---

## What's clean now (closed since v1 audit)

- Bypass drivers archived (A2)
- Player spawn race resolved (A3)
- Duplicate URP volume builder removed (A4)
- `Moon1CompletionTracker` archived (C7)
- `Moon1LevelBuilder` archived (D1)
- `.restored` AI snapshots archived (D2)
- Legacy auto-wire / spawn-milo menus commented out (D5)
- `Moon1FirstTimeHints` fleshed out (FTUE prompts)
- `PointOfInterest` runtime component shipped
- `TartarianHourCycle` rebuilt with sun rotation + `OnSeventeenthHour` event
- `Moon1SceneCleanup` menu shipped (just needs running)
- `Moon1BuildOutVillage` rewritten with real prefabs (just needs running)
- `Moon1BuildOutNPCs` shipped (just needs running)
- `Moon1BuildOutEnvironment` shipped (just needs running)
- GameEvents got 6 new event signatures (OnPOIDiscovered, OnSeventeenthHour, OnTartarianHourChanged, OnTuningProgress, FireCombatStarted/Ended)
- AdaptiveMusic Layer 2 reactive sub-cues (fixed nesting bug this turn)
- SaveData v15 schema additions
- Per-prefab spot-check: 60/60 sample VALID; population estimate 94%+ valid

---

## Recommended fix order

1. **Compile blockers Bucket 1** (15 min — delete duplicate PipeOrganMiniGame, fix StartTuning overload, AdaptiveMusic enum-nesting already fixed)
2. **Run the 5 Editor menus + manual delete of wrong-Moon shells** (Bucket 2; you do this in Unity, 5-10 min total)
3. **Wiring fixes Bucket 3** (Anastasia building id, AudioCueLibrary populate, OnTuningProgress fire, NavMesh carve, AetherFieldManager bug, haptic patches) — ~2-3 hours
4. **Hygiene Bucket 5** — ~1 hour
5. **Content Bucket 4** — biggest swallow. Lirael/Cassian yarn + prefab fixes + Milo missing 29 lines + Cathedral kit dressing + Spire pieces. Probably a follow-up session.

If Buckets 1+2+3 land, Moon 1 is in a playable + fairly complete state and the remaining gaps are clearly content rather than wiring.
