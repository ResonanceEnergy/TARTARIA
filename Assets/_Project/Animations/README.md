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

*Animation README · created alongside `NPCWalkAnimator` + `NPCWalkAnimatorAutoAttach` wiring.*
