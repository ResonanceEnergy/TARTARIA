# Moon 1 Combat / NPC / Dialogue / Quest Readiness Audit

**Date:** 2026-05-31 (v2, read-only)
**Scope:** Cross-check the 15 audit items against actual code in `Assets/_Project/Scripts/`.

---

## 1. Combat

### 1. `MudGolemAI.cs` (612 lines, intact)
- FSM `Patrol → Chase → Attack → Dead` is intact (enum at L71, `TransitionTo` L266, all 4 cases handled).
- Uses `NavMeshAgent` with CharacterController/direct-move fallback (L77-92).
- Telegraph IS implemented as `IEnumerator TelegraphAttack()` (L467) — but `telegraphDuration = 0.5f` (L45), **NOT the 1s windup per spec §4**.
- `BuildProcedural()` still pure primitives + URP `_BaseColor` paint — **violates "no primitives" mandate**, and `Assets/_Project/Prefabs/Characters/MudGolem.prefab` exists but is not loaded.
- Death drops `aetherShardPrefab` (L565-568) and fires `GameEvents.FireRSChange(5f)` — **no 3× Purified Mud drop**; "purified_mud" string appears nowhere in `Scripts/`.
- `EnableRagdoll()` is a documented stub (L610) — `Debug.Log("EnableRagdoll stub called")`.

### 2. `MudGolemHealth.cs` (262 lines, separate codepath)
- Default `_maxHealth = 300f` (L17) — **spec says 100 HP / 3-4 Harmonic Strikes**. With `PlayerCombatController.harmonicStrikeDamage = 50f`, 300 HP requires 6 strikes, not 3-4.
- Fires `Core.GameEvents.RaiseEnemyKilled(EnemyKilledEventArgs{...enemyType="mud_golem"...})` (L163-171). Correctly wired.
- Loot: random 1-3 `aether_shard` via `InventoryManager.AddItem` (L147-159). **Not "3× Purified Mud".**
- Two parallel HP systems exist: `MudGolemAI._currentHealth` (`maxHealth=50`) and `MudGolemHealth._maxHealth=300`. The AI's own `TakeDamage` (L496) and Health's `TakeDamage` (L74) are unsynchronized — whichever component receives the `SendMessage("TakeDamage", ...)` first wins. Sprint pulse + Harmonic Strike both `SendMessage`, so behavior depends on component order on the prefab.

### 3. `ResetScout.cs` (137 lines)
- 60 HP, melee `attackRange=6f` + `attackDamage=6f`, aggro 22m (L9-15). Patrol behavior is **absent** — no idle wander, only chase-when-in-aggro.
- Visual is primitives: capsule body + top-hat cube + clipboard cube (L42-71). **No dissonance crystal.** Spec calls for Victorian harasser with dissonance crystal — only the top hat lands.
- Death banner: "Per Bureau directive 3-9..." (L132). RS reward +8. No drop.

### 4. `GiantMode.cs` (165 lines) — IN GOOD SHAPE
- 60-second duration, 3× scale, toss radius 8m, force 22 (L25-36). Activate via `G` key or right-trigger.
- `TossNearby()` iterates `OverlapSphere` for tag `"Enemy"`, applies `Rigidbody.AddForce` or `CharacterController.Move` (L122-163). Works against ResetScouts (they are tagged `Enemy` in `ResetScout.Awake`).
- **Cooldown is 90 seconds in code (L31), not "1 in-game day" per spec.** Reasonable shortcut; no day-tracker hookup.

### 5. Combat abilities
- `PlayerInputHandler.cs` fires events `OnResonancePulse`, `OnHarmonicStrike`, `OnFrequencyShield` (L83-85). Wired to gamepad fallbacks (A/X = Pulse, RB = Harmonic, LT = Shield, L286-336) AND keyboard fallbacks (Space/F/R, L445-456) AND InputAction asset (L160-174). All three paths active.
- **Two abstract implementations of the abilities exist simultaneously**: `PlayerCombatController.cs` (Gameplay) and `PlayerAbilityController.cs` (Gameplay). Both subscribe to same events — if both attached to Player, abilities fire twice and cooldowns desync. Also `PlayerAbilityManager.cs` (Integration) has its own ability slot model that does NOT subscribe to `PlayerInputHandler` and is dormant.
- **Top 5 combat gaps:** (a) 1s windup spec vs 0.5s code, (b) HP duplication MudGolemAI/Health, (c) 300 HP vs 100 spec, (d) Purified Mud not implemented anywhere, (e) two ability controllers double-subscribe.

---

## 2. NPCs

### 6. Milo
- `MiloController.cs` (361 lines) is trust-arc + dialogue + save data, **NOT** the FOLLOW/IDLE/REACT/SPEAK/HIDE/CELEBRATE FSM described in the audit. No such enum exists in `Scripts/`.
- `MiloFollowBehaviour.cs` (141 lines) is a simple NavMeshAgent follow with chatter timer. No HIDE/CELEBRATE states.
- Voice line *contexts* counted in code: 11 distinct `PlayContextDialogue("milo_…")` keys (intro, warming_up, sincere, appraise_genuine, appraise_scam, market_intel, no_intel, orphan_train, white_city_rage, impressed_build, combat_quip) + 8 joke tier keys + boss intro/victory pairs. **Of those, only `milo_intro`, `milo_warming_up`, `milo_sincere` exist in `milo_intro.yarn`** (3 of 11+ → 27%).

### 7. Anastasia — wired
- `NPCConditionalSpawn.cs` listens to `GameEvents.OnBuildingRestoredTyped`, default `_triggerBuildingId = "echohaven_crystalspire"` (L17) — **NOT "echohaven_stardome"** as the audit item describes. The `Moon1DialogueBindings` switch L67-78 also keys the greeting off `crystalspire`, dome restoration off `stardome`, fountain off `harmonicfountain`. Reveal activates the child named `"Visual"` (L42), so prefab must have that exact child name.

### 8. Lirael
- `LiraelLullaby.cs` (130 lines) procedurally generates a 432 Hz hum (with 1.5× perfect-fifth + 2× octave overtones) and a spatial AudioSource. **No day-count gate.** Spawn-out is controlled by `Moon1BuildOutNPCs.cs` setting `activeAtStart: false` (L62) with comment `gateNote: "Day >= 25 (TODO: hook GameEvents.OnDayChanged when it exists)"`. The Day 25 hook does not exist in code.

### 9. Cassian — controller missing
- **No `CassianController.cs` exists.** `Scripts/` has zero files matching `Cassian*`. The prefab `Assets/_Project/Prefabs/Characters/Cassian.prefab` exists and `Moon1BuildOutNPCs` places him at `(3,0,35)` active Day 1 (L65-71), but there is no behaviour script and no foreshadow lines in any `.yarn` file.

### 10. Bob — works but minimal
- `Moon1InnRestTrigger.cs` (113 lines) auto-bootstraps via `RuntimeInitializeOnLoadMethod`. Player walks into sphere at `(10, 0.5, 5)` (L26), prompt shows ONLY if `PlayerPrefs["TARTARIA_Moon1Complete"] == 1`. E or A press calls `TriggerRest()` → sets `TARTARIA_CurrentMoon = 2` + banner (L102-111). **Bob is just a cube — `BobInnkeeper.prefab` placement is in `Moon1BuildOutNPCs` but no innkeeper dialogue.**
- **Top 5 NPC gaps:** (a) Milo FSM not present (controller is trust-state, not behaviour-state), (b) 8 of 11+ Milo context keys lack yarn nodes, (c) Anastasia spawn ID is `crystalspire`, not `stardome` per audit (intentional drift?), (d) Cassian has no script/dialogue, (e) Lirael day-gate is TODO, gate is currently `activeAtStart=false` with no opener.

---

## 3. Dialogue

### 11. Yarn files
- `milo_intro.yarn` — 3 nodes (intro, warming_up, sincere). Intro has 3 player branches. Total `Milo:` lines ≈ 11 incl. branches. **Prior audit's "11/40" count matches; no change.**
- `anastasia_greeting.yarn` — 3 nodes (greeting, dome_restored, fountain_restored). 4 author lines + 6 branch lines. Prior "4 lines" count was undercounted; she actually has ~10 lines across 3 nodes.
- `lore_whispers.yarn` — 6 lore stone nodes. NOT a character file.
- **Lirael yarn: still missing.** Whispers are inline strings in `LiraelLullaby.whisperLines[]` only.
- **Cassian yarn: still missing.**

### 12. `Moon1DialogueBindings.cs` (109 lines)
- Subscribes `OnBuildingDiscoveredTyped` → fires `milo_intro` once (L52-56).
- Subscribes `OnBuildingRestoredTyped` → fires `anastasia_greeting` (delayed 2s after spire), `anastasia_dome_restored`, `anastasia_fountain_restored`, and `milo_warming_up`/`milo_sincere` based on count (L60-83).
- Static helper `PlayLoreContext(string)` for lore stones (L91).
- **The handler resolves to `DialogueManager.Instance?.PlayContextDialogue(...)`.** Verifying that DialogueManager actually surfaces a UI panel was not in this audit's scope, but the call path exists.
- **Top 5 dialogue gaps:** (a) Lirael .yarn missing, (b) Cassian .yarn missing, (c) 8+ Milo context keys referenced by `MiloController` have no yarn node, (d) anastasia_greeting wired to spire-restore (matches NPCConditionalSpawn) not dome-restore as audit expected, (e) no Bob/innkeeper dialogue file.

---

## 4. Quests

### 13. `Moon1QuestTriggers.cs` (155 lines)
- Creates 3 SphereCollider trigger zones at `(-40,0,20)` Milo, `(0,0,80)` Cathedral, `(60,0,40)` Spire (L15-17). All radius 8m, trigger once.
- Each calls `QuestSystem.Instance.ActivateQuest(...)`.
- **Note:** trigger positions do NOT match Moon1BuildOutNPCs Milo position `(-26,0,26)`. Player must walk to `(-40,0,20)` to activate the Milo quest, but Milo is at `(-26,0,26)` — 14m offset. Player may hit Milo first and intro fires before quest activates.

### 14. Quest IDs
- `QuestSystem.cs` `InitializeStarterQuests()` (L29-74) defines: `moon1_restore_first`, `moon1_collect_shards`, `moon1_meet_milo` — all 3 IDs the audit asked about.
- **`AwakenStarDome` / `AwakenFountain` / `RaiseTheSpire` quest IDs DO NOT EXIST anywhere in `Scripts/`.** Building restoration is tracked via `OnBuildingRestoredTyped` events on `echohaven_stardome` / `echohaven_harmonicfountain` / `echohaven_crystalspire`, but these are not registered as QuestSystem quests with objectives.

### 15. `QuestObjectiveTrackerUI.cs` (200+ lines)
- **TOP-RIGHT** position confirmed: anchor `(1,1)`, pivot `(1,1)`, anchoredPosition `(-20,-120)` (L116-121).
- Auto-bootstraps via `RuntimeInitializeOnLoadMethod` (L31). Subscribes `OnBuildingRestoredTyped` and updates subline with `"Restorations: X / 3 hero buildings"` (L188). Switches to "Rest at the Inn" when 3 restored.
- **Does NOT subscribe to `QuestSystem.OnQuestActivated` or `OnObjectiveCompleted`.** The tracker is decoupled from QuestSystem; it only watches building events + PlayerPrefs `TARTARIA_Moon1Complete`. So Milo / shard quests show no on-screen objective.
- **Top 5 quest gaps:** (a) QuestSystem and QuestObjectiveTrackerUI are not connected — UI ignores QuestSystem events, (b) `AwakenStarDome`/`AwakenFountain`/`RaiseTheSpire` quest IDs absent, (c) Milo quest trigger at `(-40,0,20)` mis-aligned with Milo NPC at `(-26,0,26)`, (d) Spire quest only completes objective index 0 of `moon1_collect_shards`, no shard count tracking present, (e) `ShowQuestNotification` ends at `Debug.Log` only — no in-game banner per audit (`// TODO: Integrate with NotificationSystem`).

---

## Summary scoreboard

| Subsystem | Real | Stub/missing/drift |
|---|---|---|
| MudGolemAI FSM | yes | telegraph 0.5s vs 1s, EnableRagdoll stub, BuildProcedural primitives, no Purified Mud |
| MudGolemHealth | yes | 300 HP vs 100 spec, parallel HP with AI |
| ResetScout | partial | no patrol, no dissonance crystal |
| GiantMode | yes | day-cooldown approximated as 90s |
| Combat input wiring | yes | TWO controllers (PlayerCombat + PlayerAbility) double-subscribe |
| Milo | partial | controller is trust-state, no FSM |
| Anastasia conditional spawn | yes | gated on `crystalspire` (not `stardome` as audit expected) |
| Lirael lullaby | yes | day-25 gate is TODO |
| Cassian | NO | no script, no yarn |
| Bob inn rest | yes | minimal; Moon1Complete pref gate |
| Yarn files | 3 of 5 | Lirael & Cassian missing; 8+ Milo keys unresolved |
| Moon1DialogueBindings | yes | aligned to actual yarn nodes |
| Moon1QuestTriggers | yes | Milo zone offset from Milo NPC |
| Quest IDs | 3 expected, 3 found | Awaken*/Raise* IDs absent |
| Quest tracker UI | top-right, autoboot | not wired to QuestSystem events |
