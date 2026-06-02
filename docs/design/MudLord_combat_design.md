# Mud Lord — Moon 1 Boss Combat Design

> **Status:** Spec draft, 2026-06-02. Companion to `Assets/_Project/Scripts/AI/MudLordBoss.cs`.
> **Trigger:** `GameEvents.OnMoonCompleted(MoonCompletedEventArgs { moonIndex == 1 })` — canonical per `docs/agents/API_CONTRACT.md`.

---

## 1. Premise & Stage

The Mud Lord is the drowned king of Echohaven — a barnacled, mud-encrusted colossus buried beneath the central Mud Pool POI. He awakens when Milo restores the three hero buildings (Cathedral, Anastasia's Tower, Bob's Inn). The pool surface erupts and he ascends through `RunSpawning()` (a ~4.5s vertical lerp, ease-in, 4 m of clearance) into the village square.

The arena is the existing Mud Pool clearing: roughly 22 m radius, ringed by partially-restored buildings (cover), with the central mud crater as a hazard zone. NavMesh must be baked over the perimeter ring — the Phase 3 minion spawns use `NavMesh.SamplePosition`.

| Property | Value |
|---|---|
| Total HP | 600 |
| Phase 1 window | 100% → 66% HP |
| Phase 2 window | 65% → 33% HP |
| Phase 3 window | 32% → 0% HP |
| Frontal damage multiplier | 0.15× (absorbed by mud plating) |
| Back-cone weak-point bonus | 1.5× |
| Back-cone arc | 70° (configurable) |
| Designed encounter length | 3–4 min |

The weak point is a child `Transform` anchored to the back of the rig, between the shoulder blades. The player must circle behind the Mud Lord during recovery / telegraph windows and land a **Harmonic Strike** (RB on F310, per `docs/appendices/D_CONTROLS_F310.md`) within the back cone.

---

## 2. Phase 1 — Telegraphed Charges (100% → 66%)

**State:** `State.Charge`, looped from `RunFSM()` until `NormalizedHealth ≤ 0.66`.

**Beat structure (repeated):**

1. **Telegraph (1.1 s).** Mud Lord faces the player; bark "*The mud surges!*" appears via `GameEvents.RaiseHUDShowEnemyBark`. Player has ~1 s to position perpendicular to the charge axis.
2. **Charge (≈1.5 s, 9.5 m/s × 14 m).** Straight-line dash along the locked-in facing. Anything in the 2.4 m impact radius takes 22 damage via `RaisePlayerDamaged`. If the player sidesteps cleanly, the Mud Lord overshoots.
3. **Recovery (1.8 s).** Mud Lord stops, back exposed. **This is the harmonic-strike window.** A clean back-cone strike during recovery deals `damage × 1.5`; a frontal strike deals `damage × 0.15` (absorbed by mud plating, plays a "thud" SFX hint).

**Designer intent:** teach the weak-point loop. Charges are the slowest, most readable attack — if the player can't learn "dodge → circle behind → strike" here, the encounter is mistuned. Failure state in Phase 1 is recoverable; the player can outheal one missed dodge.

**Tunable knobs (Inspector):** `chargeSpeed`, `chargeTelegraphSeconds`, `chargeRecoverySeconds`, `chargeDistance`, `chargeImpactDamage`, `chargeImpactRadius`.

---

## 3. Phase 2 — Ground-Pound Rhythm @ 7.83 Hz Telluric (65% → 33%)

**State:** `State.GroundPound`, gated by `EnterPhase2()` which fires a one-shot bark "*Hear the heartbeat of the deep.*"

**Beat structure (3-strike pattern, repeated):**

1. **Telegraph (0.7 s).** Mud Lord raises a fist over the player's last-known position.
2. **Pound 1.** AoE radius 6.5 m, 35 damage. The shockwave is centered on the Mud Lord, so the player must be **outside** the radius — kiting outward is the survival vector.
3. **Interval (≈0.635 s = 5 beats @ 7.83 Hz).** This interval is **load-bearing for the audio team**: the pound cadence syncs to the Telluric band carrier (7.83 Hz Schumann resonance, per `docs/02_AETHER_ENERGY_SYSTEM.md`). Audio should pulse a low sub-bass on the same period.
4. **Pound 2.** Same radius, same damage.
5. **Interval (≈0.635 s).**
6. **Pound 3.** Same.
7. **Rest (2.4 s).** Long opening — best Phase 2 weak-point window. Mud Lord plants, panting; back fully exposed.

**Designer intent:** introduce timing. The 3-strike rhythm is the player's first taste of the Tartarian timing minigame style being applied to combat. After 1–2 sequences the player should recognize they can land **3 back-cone strikes** during the rest window if they pre-position during Pound 3.

**Failure state in Phase 2:** standing still = guaranteed full-burst (3 × 35 = 105 dmg). Forces motion.

**Tunable knobs:** `groundPoundIntervalSeconds`, `groundPoundRadius`, `groundPoundDamage`, `groundPoundTelegraphSeconds`, `groundPoundRestSeconds`.

---

## 4. Phase 3 — Enraged: Mud Golem Spawns + Aether Vision Gating (32% → 0%)

**States:** `State.EnragedSpawn` interleaved with `State.GroundPound`, gated by `EnterPhase3()` ("*THE TIDE TAKES YOU!*").

**Beat structure (repeated):**

1. **Enraged Spawn wave.** Every `enragedSpawnIntervalSeconds` (8 s), the Mud Lord summons `enragedGolemsPerWave` (2) Mud Golems via the `golemSpawnPrefab` Inspector ref. Spawn positions are `NavMesh.SamplePosition`-snapped to within 6 m of the boss. Golems reuse `MudGolemAI` (existing canonical enemy, `Scripts/AI/MudGolemAI.cs`).
2. **Continued ground pounds** between waves (same 3-strike pattern as Phase 2 but with no rest window — Mud Lord pivots straight from rest into the next telegraph).
3. **Weak-point obscured.** Visually, the Mud Lord coats his back with mud — the persistent weak-point glow fades. **The player must activate Aether Vision** (Y button on F310, fires `GameEvents.RaiseAetherVisionToggled(true)`) to see the true weak point pulse through the mud.
   - Code note: `MudLordBoss.ComputeWeakPointMultiplier()` currently still grants back-cone bonus damage regardless of vision state. Gating that bonus on Aether Vision is documented as a follow-up — the design intent is recorded here; the gating PR will pass a `bool isAetherVisionActive` from the player strike code into `TryStrikeWeakPoint`. This is **intentional decoupling**, not a stub: the AI assembly does not have a clean accessor for the player's vision flag yet, and inventing one would violate the API contract.

**Designer intent:** stress test. Phase 3 forces the player to manage 3 concurrent demands — boss telegraphs, minion pressure, ability resource (Aether Vision drains energy). This is the ship-gate fight for Moon 1; if the player can't clear Phase 3 in 90 s without dying, Moon 1 is unwinnable on standard difficulty.

**Tunable knobs:** `golemSpawnPrefab`, `enragedGolemsPerWave`, `enragedSpawnIntervalSeconds`, `enragedSpawnRadius`.

---

## 5. Defeat — Cinematic Hook

**State:** `State.Defeated`. Entered via `ApplyDamage()` when `hp ≤ 0`, or via the safety fallback at the end of `RunFSM()`.

**`RunDefeated()` fires the following, in order:**

| Call | Purpose |
|---|---|
| `_agent.isStopped = true` | Halt navigation if a `NavMeshAgent` is attached. |
| `GameEvents.RaiseHUDShowBanner("MUD LORD DEFEATED", "+200 RS", 5f)` | Full-screen banner. Signature verified against `GameEvents.cs:623`. |
| `GameEvents.RaiseHUDHideBossHealth()` | Removes the boss health bar UI. |
| `GameEvents.RaiseHUDFlashRSGain(200f)` | Flashes the RS counter for the +200 award. |
| `GameEvents.RaiseBossDefeated(new BossDefeatedEventArgs { bossId = "moon1_mud_lord", xpReward = 500, rsReward = 200, position = transform.position })` | Canonical boss-defeat event. Subscribers: QuestManager, CinematicController, ProgressionController. |
| `OnDefeated?.Invoke(this)` (local event) | **Cinematic hook.** The Moon 1 defeat cutscene controller subscribes here in the same scene so it can chain a custom beat (camera pull-back, Anastasia walks up, dialogue beat) without needing to filter `OnBossDefeated` by `bossId` from a global subscriber. |

**Subscriber pattern for the cinematic:**

```csharp
// In Moon1DefeatCinematic.cs (out of scope for this PR — owned by Cinematics path):
void Awake()
{
    var boss = FindFirstObjectByType<MudLordBoss>(FindObjectsInactive.Include);
    if (boss != null) boss.OnDefeated += HandleMudLordDefeated;
    else Debug.LogWarning("[Moon1DefeatCinematic] No MudLordBoss in scene. Identifier searched: 'MudLordBoss'.");
}
```

This local-event pattern is preferred over a global `OnBossDefeated` filter because it cannot fire for the wrong boss, and it does not require the cinematic to know the `bossId` magic string.

---

## 6. Failure / Edge Cases Recorded for QA

- **NavMesh not baked around mud pool** → enraged minions spawn at raw offsets; the boss still progresses. Log fires (`NavMesh.SamplePosition failed near {pos}`).
- **Player dies mid-fight** → `OnMoonCompleted` does not re-fire on respawn; the boss is already armed and continues. Respawn logic must teleport player back into the arena ring.
- **`golemSpawnPrefab` unassigned** → Phase 3 logs the warning and skips spawning; boss is beatable but trivial in Phase 3. Wire `Assets/_Project/Prefabs/Enemies/MudGolem.prefab` before ship.
- **`weakPointTransform` unassigned** → falls back to root; back cone still works geometrically but the visual indicator (Aether Vision pulse) has no anchor. Wire a back-of-rig bone before ship.
- **Player triggers Moon 1 completion twice** → `_armed` guard suppresses second `StartEncounter()`. Log fires.

---

*MudLord_combat_design.md v0.1 · 2026-06-02 · Companion to `Scripts/AI/MudLordBoss.cs`. Update when phase tuning shifts.*
