# Assets/_Project/Animations

Owned by: **Animation Engineer** agent (per `docs/agents/COORDINATION.md`).

This folder holds animation clips, the `EchohavenHumanoid.controller` (Sprint-2 PR #6), and any
animator override/state-machine assets used by Moon 1 characters and ambient NPCs.

---

## Runtime wiring: NPC walk loop

The `EchohavenHumanoid` controller exposes a single bool parameter — **`IsWalking`** — that gates
the `Idle ↔ Walk` transition. Two scripts (under `Assets/_Project/Scripts/Animation/`) keep that
bool in sync with whatever is actually moving the NPC at runtime.

### 1. `NPCWalkAnimator.cs`

`[DisallowMultipleComponent] [RequireComponent(typeof(Animator))]`

Drives `IsWalking` from per-frame motion:

- If a `NavMeshAgent` is present and active, uses `agent.velocity.magnitude`.
- Otherwise falls back to `(transform.position - lastPos).magnitude / Time.deltaTime`, so it still
  works for NPCs animated via Timeline, DOTween, or hand-scripted transform moves.
- Sets `IsWalking = (speed > walkSpeedThreshold)` (default `0.15` m/s) each `Update()`.
- Guards against a missing `runtimeAnimatorController` so it's safe on placeholder rigs.

### 2. `NPCWalkAnimatorAutoAttach.cs`

`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`

After a scene loads, walks every `Animator` in the scene via
`Object.FindObjectsByType<Animator>(FindObjectsSortMode.None)` and attaches an
`NPCWalkAnimator` to GameObjects whose tag is one of:

| Tag | Used for |
|---|---|
| `NPC` | Generic ambient NPCs |
| `Villager` | Echohaven villagers |
| `Milo` | Milo (Moon 1 mentor) |
| `Cassian` | Cassian |
| `Anastasia` | Princess Anastasia |
| `Lirael` | Lirael |

Skips GameObjects that already carry the component (idempotent across additive scene loads).
Logs a one-line summary: `Attached N · skipped existing M · skipped untagged K · scanned T`.

### Wire pattern

```
Scene Play
   │
   ▼
[AfterSceneLoad] NPCWalkAnimatorAutoAttach.Bootstrap()
   │   FindObjectsByType<Animator>()
   │   ├── tag in {NPC, Villager, Milo, Cassian, Anastasia, Lirael}?
   │   │     └── AddComponent<NPCWalkAnimator>()
   │   └── else: skip
   ▼
[Per frame] NPCWalkAnimator.Update()
   │   speed = NavMeshAgent.velocity.magnitude  (or position-delta fallback)
   ▼
   Animator.SetBool("IsWalking", speed > 0.15f)
   ▼
EchohavenHumanoid.controller transitions Idle ↔ Walk
```

### Why scripts (not a prefab edit)?

Per `docs/agents/COORDINATION.md`, the Animation agent does **not** own
`Assets/_Project/Prefabs/`. Auto-attaching at runtime keeps wiring entirely inside the
Animation agent's path ownership — no prefab churn, no scene churn, no hand-off needed when a
new NPC prefab lands as long as it carries an `Animator` and the right tag.

### Cowork verification (Editor Play)

When the Cowork runtime-QA pass runs `Echohaven_VerticalSlice.unity`:

1. Enter Play.
2. Console shows the `[NPCWalkAnimatorAutoAttach]` line with `Attached >= 1`.
3. NPCs that move along their nav routes visibly cycle into the `Walk` clip; idle NPCs stay on
   `Idle`.
4. When an NPC reaches a waypoint and its agent stops, the clip returns to `Idle` within ~1 frame
   of `agent.velocity.magnitude` dropping below `0.15`.

If verification fails: check the GameObject's tag (must be one of the six above) and confirm the
`EchohavenHumanoid` controller is the `runtimeAnimatorController` on the Animator.

---

## Cymatic Rose Window (`RoseWindowCymatic.cs`)

`[DisallowMultipleComponent] [RequireComponent(typeof(Renderer))]`

When the cathedral is restored, the rose window blooms with a procedural cymatic glow — emission ramps
from black to a warm gold over `fadeInDuration` seconds, then enters a perpetual breathing pulse driven
by `Mathf.Sin(Time.time * waveSpeed * 2pi)`.

### Trigger

Subscribes to `Tartaria.Core.GameEvents.OnBuildingRestored` in `Awake()`, unsubscribes in `OnDestroy()`.
Filters by `buildingIdFilter` (default `"cathedral"`, case-insensitive `Contains` check) so it only fires
for the cathedral, not for every village building restoration. Re-triggering while already active stops
the previous coroutine and replays the intro — restoring twice (e.g. in tests) doesn't double up.

### Animation sequence

| Phase | Duration | Behavior |
|---|---|---|
| Fade-in | `fadeInDuration` (2s) | `_activeT` ramps 0 -> fadeInDuration; `fadeIn = _activeT / fadeInDuration` scales emission and lerps `_BaseColor` from black to `baseEmission`. |
| Sustain (intro) | `sustainDuration` (6s) | Pulse continues at full amplitude; coroutine waits, logs completion. |
| Perpetual | forever | `_active` stays true; pulse breathes via `0.5 + 0.5 * sin(time * waveSpeed * 2pi)` scaled by `waveAmplitude` (1.2). Window stays lit until the GameObject is destroyed. |

Drives material via `MaterialPropertyBlock` (no per-instance material allocation): writes
`_EmissionColor` and `_BaseColor` shader property IDs each frame while active.

### Cowork scene wiring

The Animation agent does NOT touch `.mat` files or scene/prefab GUIDs. Cowork handles:

1. Open `Echohaven_VerticalSlice.unity`.
2. Locate the cathedral prefab instance, find the rose window child GameObject (the mesh that should
   bloom — typically `RoseWindow` or similar geometry on the cathedral facade).
3. Add Component -> `Tartaria.Animation.RoseWindowCymatic`.
4. Confirm the Renderer's Material is **URP/Lit** with **Emission enabled** (Surface Inputs ->
   Emission checkbox on; HDR color slot present). The shader properties the script writes
   (`_EmissionColor`, `_BaseColor`) are standard URP/Lit names.
5. In Play mode: trigger `GameEvents.FireBuildingRestored("cathedral_echohaven")` (or whatever the
   canonical cathedral id is per `GameEvents.cs`) via the restoration mini-game; the rose window
   should fade up over 2s and breathe.

### Verification logs

Look for these in the Console:

- `[RoseWindowCymatic] Subscribed to GameEvents.OnBuildingRestored on '<path>' (filter='cathedral')` on Awake
- `[RoseWindowCymatic] Activating on '<path>' for buildingId='<id>' (fadeIn=2s, sustain=6s)` on trigger
- `[RoseWindowCymatic] Intro complete on '<path>' — entering perpetual sustain.` 8s later
- `[RoseWindowCymatic] Unsubscribed from GameEvents.OnBuildingRestored on '<path>'` on scene unload

If the window never lights: confirm the buildingId fired from the restoration system actually contains
"cathedral" (override `buildingIdFilter` in the Inspector if your canonical id is different — e.g.
`"echohaven_cathedral"` matches the default filter, but `"e_cath_01"` would not).

---

*Animation README · created alongside `NPCWalkAnimator` + `NPCWalkAnimatorAutoAttach` wiring; extended for `RoseWindowCymatic`.*
