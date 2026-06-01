# COORDINATION.md — Agent Fork Pattern + Hand-off Protocol
*How 10 agents work in parallel without stepping on each other.*

---

## The branch graph

```
main                                                       (NATRIX merges here only)
  ↑ ↑
develop                                                    (Director merges here)
  ↑ ↑ ↑ ↑ ↑ ↑ ↑ ↑ ↑ ↑
  │ │ │ │ │ │ │ │ │ │
  agent/systems/save-load
  │ │ │ │ │ │ │ │ │
  agent/gameplay/wire-e-key-chain
  │ │ │ │ │ │ │ │
  agent/ai/mudgolem-mesh-fix
  │ │ │ │ │ │ │
  agent/ui/interaction-prompt
  │ │ │ │ │ │
  agent/tools/camera-reposition
  │ │ │ │ │
  agent/level/spawn-position-fix
  │ │ │ │
  agent/narrative/milo-intro-yarn
  │ │ │
  agent/audio/restoration-stinger
  │ │
  agent/anim/humanoid-retarget
  │
  agent/qa/test-scenes
```

**Rule:** Each agent works only on their `agent/<role>/<task>` branch. Never edit `develop`. Never edit `main`.

---

## Branch naming

```
agent/<role>/<task-slug>
```

| Role slug | Agent name |
|---|---|
| `systems` | Systems Architect |
| `gameplay` | Gameplay Programmer |
| `ai` | AI Programmer |
| `ui` | UI Programmer |
| `tools` | Tools Engineer |
| `level` | Level Designer |
| `narrative` | Narrative Designer |
| `audio` | Audio Engineer |
| `anim` | Animation Engineer |
| `qa` | QA Engineer |

Task slug: lowercase, hyphen-separated, descriptive. Examples:
- `agent/gameplay/wire-e-key-chain`
- `agent/ai/mudgolem-mesh-spam-fix`
- `agent/tools/camera-reposition`

---

## Path ownership (no overlap)

| Folder | Owner |
|---|---|
| `Assets/_Project/Scripts/Core/` | Systems |
| `Assets/_Project/Scripts/Gameplay/` | Gameplay |
| `Assets/_Project/Scripts/AI/` | AI |
| `Assets/_Project/Scripts/UI/` | UI |
| `Assets/_Project/Scripts/Editor/` | Tools |
| `Assets/_Project/Scripts/Integration/Moon1*.cs` (gameplay-flavored) | Gameplay |
| `Assets/_Project/Scripts/Integration/Moon1*NPC*/Villager*/Anastasia*` | AI |
| `Assets/_Project/Scripts/Integration/*UI*` | UI |
| `Assets/_Project/Scripts/Integration/Moon1Camera*/Moon1Audio*` | (see role) |
| `Assets/_Project/ScriptableObjects/` | Systems |
| `Assets/_Project/Scenes/` | Level |
| `Assets/_Project/Scenes/Tests/` | QA |
| `Assets/_Project/Dialogue/` | Narrative |
| `Assets/_Project/Audio/` | Audio |
| `Assets/_Project/Animations/` | Animation |
| `Assets/_Project/Prefabs/Moon1/Blender/` (read-only consumers; writer = Blender batch) | Tools |
| `Assets/_Project/Models/Blender/` (write via Blender batch only) | Tools |
| `tools/blender/` | Tools |
| `docs/` | Director |
| `docs/agents/` | Director |

**Conflict rule:** When a task naturally spans two owners (e.g., wiring a UI prompt to a gameplay event), the **gameplay owner does the wiring**, the **UI owner adds the visual prompt**. Two PRs, sequenced.

---

## Hand-off protocol

When Agent A needs Agent B to make a change in B's territory:

1. Agent A opens `docs/HANDOFFS.md` and appends:
   ```
   ## 2026-06-01 14:30 — Gameplay → UI
   Need: Update HUDController.cs to subscribe to GameEvents.OnTuningSucceeded
         and show "Building tuned!" toast for 3 seconds.
   Why: Win condition flow shipped in agent/gameplay/wire-e-key-chain
   Blocking: Phase 1 Milestone 1.2 completion
   ```
2. Agent A pings the Director in chat with the hand-off ID
3. Director assigns to Agent B with a target time
4. Agent B picks it up, branches `agent/ui/handoff-2026-06-01-1430`
5. Agent B PRs back to develop with the hand-off ID in commit message

---

## PR workflow

**Open PR:**
```bash
git checkout develop
git pull
git checkout -b agent/<role>/<task>
# ... do work ...
git add -A
git commit -m "[<role>] <verb> <what>"
git push -u origin agent/<role>/<task>
gh pr create --base develop --title "[<role>] <task>" --body "$(cat <<EOF
## What changed
<short summary>

## Success criteria from my prompt
- [x] criterion 1
- [x] criterion 2
- [ ] criterion 3 (deferred to follow-up)

## Runtime artifact
<screenshot or log excerpt>

## Hand-offs created
- None / docs/HANDOFFS.md#<id>
EOF
)"
```

**Director review checklist:**
- ☐ Pull branch + open Unity
- ☐ Compile clean? Check `Library/Bee/tundra.log.json`
- ☐ Bootstrap → Wire-All → Play succeeds
- ☐ Success criteria from agent's prompt demonstrably met (screenshot/log evidence)
- ☐ No edits outside agent's path ownership
- ☐ Commit messages follow `[role] verb what` convention

**If reject:** Comment specific reason. Don't merge. Agent fixes in same branch.

**If accept:** Squash merge to `develop`. Delete branch. Update STATUS.md.

---

## When to NOT parallelize

The pattern only works when tasks are independent. **Sequence (don't parallelize) when:**
- Task A modifies a file Task B also needs to modify
- Task B depends on Task A's output (e.g., AI needs Systems' new IPersistable interface first)
- Both touch the same scene
- Both touch the same prefab

**Director's job:** Read the queue and decide what parallelizes safely. Default to sequential when in doubt. 3 parallel agents max.

---

## Daily / per-session cadence

```
1. NATRIX gives the Director a goal
2. Director reads ROADMAP.md, picks next milestone
3. Director assigns 1-3 agents to parallel tasks
4. Agents work, open PRs
5. Director reviews + merges
6. Repeat until milestone done
7. Director updates STATUS.md + commits
8. NATRIX gets a session summary at end
```

**Hard rule:** No session ends without STATUS.md updated, even if just "no change."

---

## Anti-patterns

- ❌ Agent edits `develop` directly — never
- ❌ Two agents on same branch
- ❌ PR with no runtime artifact
- ❌ Agent self-merges
- ❌ Director writes code (they direct, not implement)
- ❌ NATRIX bypasses Director and assigns 10 tasks at once (defeats coordination)
- ❌ Force push to shared branches
- ❌ Editing `main` outside a release tag

---

## What this enables

When working right, you can run **3 parallel agent sessions** without conflict:
- Session A: Gameplay agent wires E-key chain (`Scripts/Gameplay/`)
- Session B: Audio agent adds restoration stinger (`Audio/`)
- Session C: Narrative agent writes Anastasia yarn (`Dialogue/`)

All three land in develop within an hour. Without this protocol, all three would step on each other and one of them would lose work.

---

*COORDINATION.md v1.0 · Source of truth for multi-agent ops.*
