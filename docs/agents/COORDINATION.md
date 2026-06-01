# COORDINATION.md — Agent Fork Pattern + Hand-off Protocol
*How 10 agents work in parallel without stepping on each other.*

---

## ⚡⚡⚡ PARALLEL-BY-DEFAULT MANDATE (2026-06-01, from NATRIX)

**THE SWARM RUNS IN PARALLEL — NEVER SERIAL.** Mirror of CLAUDE.md top-of-file rule. Every entry in this doc must be read with this constraint in mind.

Operating rules:

1. **Concurrent dispatch.** When N independent tasks exist (different files, different folders, different success criteria), ALL N workers spin up in a single batch. No serial queue.
2. **No sibling waiting.** Agent N needs something from agent M? Append a HANDOFFS.md entry, KEEP WORKING in N's other scope. Don't block.
3. **Parallel authoring of dependent code.** If lane B depends on lane A's not-yet-merged work, B authors against A's branch in parallel, rebases on develop after A merges. Code work parallelizes even when merges sequence.
4. **Single dispatch prompt to VS Code.** One Director prompt lists all N lanes + the parallel contract. No drip-feed.
5. **Checkpoint reporting at +30 min.** Director surfaces status of every concurrent agent: writing / PR-open / blocked. Don't poll one to completion before checking others.
6. **Cowork drives runtime QA in parallel with VS Code authoring.** Verification on PR #(K-1) happens while VS Code authors PR #K.

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
  agent/gameplay/playerinput-movement-debug
  │ │ │ │ │ │ │ │
  agent/ai/mudgolem-cleanup
  │ │ │ │ │ │ │
  agent/ui/interaction-prompt-polish
  │ │ │ │ │ │
  agent/tools/spawn-override-fix
  │ │ │ │ │
  agent/level/moonconfig-factory-seed
  │ │ │ │
  agent/narrative/anastasia-reveal-yarn
  │ │ │
  agent/audio/restoration-stinger-chain
  │ │
  agent/anim/mecanim-humanoid-retarget
  │
  agent/qa/test-scenes-mvp
```

**Rule:** Each agent works only on their `agent/<role>/<task>` branch. Never edit `develop`. Never edit `main`. ALL active branches above are authored CONCURRENTLY by the swarm — not sequenced.

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

Task slug: lowercase, hyphen-separated, descriptive.

---

## Path ownership (no overlap — enables parallel)

This table makes parallel safe. Two agents can run concurrently ONLY when their paths don't overlap.

| Folder | Owner |
|---|---|
| `Assets/_Project/Scripts/Core/` | Systems |
| `Assets/_Project/Scripts/Gameplay/` | Gameplay |
| `Assets/_Project/Scripts/Input/` | Gameplay |
| `Assets/_Project/Scripts/AI/` | AI |
| `Assets/_Project/Scripts/UI/` | UI |
| `Assets/_Project/Scripts/Editor/` | Tools |
| `Assets/_Project/Scripts/Integration/Moon1*.cs` (gameplay-flavored) | Gameplay |
| `Assets/_Project/Scripts/Integration/Moon1*NPC*/Villager*/Anastasia*` | AI / Narrative (split by file) |
| `Assets/_Project/Scripts/Integration/Moon1*UI*/InteractionPrompt*` | UI |
| `Assets/_Project/Scripts/Integration/Moon1Audio*` | Audio |
| `Assets/_Project/Scripts/Integration/Moon1Anastasia*` | Narrative |
| `Assets/_Project/ScriptableObjects/` | Systems (config schemas) + Level (instance assets) |
| `Assets/_Project/ScriptableObjects/Moons/` | Level |
| `Assets/_Project/Scenes/` | Level |
| `Assets/_Project/Scenes/Tests/` | QA |
| `Assets/_Project/Dialogue/` | Narrative |
| `Assets/_Project/Audio/` | Audio |
| `Assets/_Project/Animations/` | Animation |
| `Assets/_Project/Prefabs/Moon1/Blender/` (read-only consumers; writer = Blender batch) | Tools |
| `Assets/_Project/Models/Blender/` (write via Blender batch only) | Tools |
| `tools/blender/` | Tools |
| `docs/` (except agents/ + HANDOFFS) | Director |
| `docs/agents/` | Director |
| `docs/HANDOFFS.md` | every agent appends; Director archives |
| `docs/playtests/` | QA + Cowork (runtime artifacts) |

**Conflict rule:** When a task naturally spans two owners (e.g., wiring a UI prompt to a gameplay event), the **publisher's PR ships first** by merge order; the **subscriber authors in parallel against the publisher branch**, then rebases. Both PRs are written CONCURRENTLY.

---

## Hand-off protocol

When Agent A needs Agent B to make a change in B's territory, A does NOT block. A appends to `docs/HANDOFFS.md`:

```
## 2026-06-01 14:30 — Gameplay → UI
Need: Update HUDController.cs to subscribe to GameEvents.OnTuningSucceeded
      and show "Building tuned!" toast for 3 seconds.
Why: Win condition flow shipped in agent/gameplay/wire-e-key-chain
Blocking: Phase 1 Milestone 1.2 completion
```

Director picks it up on the next checkpoint, assigns to B with a target time. A keeps working on A's remaining scope.

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
<compile log excerpt, code diff link, or "runtime QA pending Cowork drive" if N/A for this PR>

## Hand-offs created
- None / docs/HANDOFFS.md#<id>
EOF
)"
```

**Director review checklist:**
- ☐ Pull branch + verify compile via `Library/Bee/tundra.log.json` (0 CS errors)
- ☐ Success criteria from agent's prompt demonstrably met (compile log, code diff, file:line evidence)
- ☐ No edits outside agent's path ownership
- ☐ Commit messages follow `[role] verb what` convention
- ☐ Runtime walkthroughs are Cowork's job — VS Code agents do NOT fabricate screenshots

**If reject:** Comment specific reason. Don't merge. Agent fixes in same branch.

**If accept:** Squash merge to `develop`. Delete branch. Update STATUS.md.

---

## When sequencing IS required (rare)

The parallel-by-default rule has narrow exceptions. SEQUENCE (not parallelize) only when:
- Task A modifies a file Task B also needs to modify AND there's no clean split. Resolution: split into two PRs by owner, queue them — but author concurrently with one rebasing.
- Both touch the same scene file (single Echohaven_VerticalSlice.unity). Resolution: scene work goes through Level agent only.
- Both touch the same prefab. Resolution: one PR; the other agent files a HANDOFFS request.

**Default to parallel when in doubt.** 9-10 parallel agents is normal.

---

## Daily / per-session cadence

```
1. NATRIX gives the Director a goal
2. Director reads ROADMAP.md, picks next milestone
3. Director CONCURRENTLY dispatches N agents to parallel tasks (single batch prompt)
4. Agents author code IN PARALLEL, open PRs as each finishes
5. Director reviews + merges in dependency order (code work was parallel; merges sequence)
6. Cowork drives runtime QA per merge, IN PARALLEL with next round authoring
7. Repeat until milestone done
8. Director updates STATUS.md + commits
9. NATRIX gets a session summary at end
```

**Hard rule:** No session ends without STATUS.md updated, even if just "no change."

---

## Anti-patterns

- ❌ Agent edits `develop` directly — never
- ❌ Two agents on same branch
- ❌ PR with no runtime artifact (compile log, code diff, file:line cite — Cowork supplies Play-mode artifacts)
- ❌ Agent self-merges
- ❌ Director writes code (they direct, not implement)
- ❌ Force push to shared branches
- ❌ Editing `main` outside a release tag
- ❌ **Serial dispatch when parallel was possible** — biggest waste of swarm capacity
- ❌ **Drip-feeding individual agent prompts** — should be one batched dispatch prompt
- ❌ **Cowork waits for VS Code's next PR before driving runtime QA on the prior one** — runtime + authoring overlap
- ❌ **Fabricated runtime screenshots from CLI-only agents** — VS Code reports code+compile, Cowork reports Play-mode

---

## What this enables

When working right, you can run **9-10 parallel agent sessions** without conflict. All branches land in develop within an hour. Without this protocol, agents step on each other and at least one loses work.

---

*COORDINATION.md v1.2 · Source of truth for multi-agent ops · Parallel-by-default mandate added 2026-06-01*
