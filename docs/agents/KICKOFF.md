# KICKOFF.md — How to start the swarm in VS Code
*The 5-minute setup that gets all 11 agents working.*

---

## Prerequisites

- VS Code installed with GitHub Copilot enabled
- Copilot Chat in Agent mode available (Ctrl+Shift+I → switch to "Agent")
- Project open at `C:\dev\TARTARIA_new`
- Terminal panel visible (Ctrl+`)
- Git authenticated to push to `feature/consolidate-moon-architecture`

## One-time setup

```powershell
cd C:\dev\TARTARIA_new
git checkout feature/consolidate-moon-architecture
git pull
git checkout -b develop
git push -u origin develop
```

Branch protection (optional but recommended): set `main` and `develop` to require PR review before merge.

## Starting a session — the script

**Step 1.** Open VS Code Copilot Chat in Agent mode. Paste this:

```
You are the DIRECTOR. Read docs/agents/DIRECTOR.md fully now. Then read STATUS.md and ROADMAP.md. Tell me the top 3 tasks for this session based on Phase 1 milestone progress, with which agent each should be assigned to.
```

**Step 2.** Director responds with 3 tasks like:
> 1. Gameplay agent → wire E-key chain on TuningPedestalLink.cs:60
> 2. AI agent → fix MudGolem MeshRenderer spam in MudGolemAI.cs:120
> 3. Tools agent → reposition camera spawn for village visibility

**Step 3.** For each task, open a NEW Copilot Chat (Ctrl+Shift+I again), switch to Agent mode, paste:

```
You are the GAMEPLAY PROGRAMMER. Read docs/agents/TEAM.md section "AGENT 2 — Gameplay Programmer" fully. Read docs/agents/COORDINATION.md.

Your task this session: Wire the E-key chain end-to-end. Read TuningPedestalLink.cs, TuningMiniGame.cs, InteractableBuilding.cs first. Then implement the success chain so OnTuningSucceeded → OnBuildingRestored → OnMoonComplete(1) fires when 3 buildings restored.

Branch: agent/gameplay/wire-e-key-chain
Open a PR to develop when done with a screenshot of OnMoonComplete firing in console.
```

**Step 4.** Repeat for each agent (each in its own Copilot Chat session, in parallel).

**Step 5.** When agents push PRs, switch back to the Director chat:

```
Review PR <branch-name>. Pull it, run Bootstrap → Wire-All → Play. Verify success criteria. Merge or reject with reason.
```

**Step 6.** End of session — Director:

```
Update STATUS.md with what landed this session. Commit + push docs changes. Write 3-bullet handoff in docs/HANDOFFS.md for next session.
```

---

## Realistic parallelism

Even with the protocol, **3 agents in parallel is the practical max** in a Copilot Chat workflow:
- Each agent costs context tokens
- VS Code can have many Chat windows open but only one Agent mode active at a time per workspace
- Director needs to review one PR at a time

**For solo NATRIX:** Run 1-2 agents per sitting, rotate roles across sessions.

**For NATRIX + Claude (me):** I can drive agents through computer-use while you observe. Tell me which 2-3 roles to spin up.

---

## Recovery if things break

**Compile broken on develop?**
1. Director rolls back: `git reset --hard <last-good-commit>`
2. Force-push develop with NATRIX's approval only (rare)
3. Re-issue task to agent with the error from `Library/Bee/tundra.log.json`

**Two agents conflict on the same file?**
1. Director rejects both PRs
2. Sequences them: Agent A merges first
3. Agent B rebases their branch on the new develop
4. Agent B's PR re-opens

**Agent goes off-script (edits forbidden paths)?**
1. Director rejects PR
2. Cite the path ownership rule from COORDINATION.md
3. Issue hand-off to the correct owner

---

## Token budget reality

Each agent invocation in Copilot Chat costs context. Strategy:
- **Director** stays loaded across whole session (high context, persistent)
- **Worker agents** spawn fresh per task (low context, ephemeral)
- Don't ask an agent to "do everything" — break into single tasks with single success criteria

A reasonable per-session output: 3 tasks merged, ~6 PR rounds, ~4 hours wall time.

---

## Phase 1 ship checklist (mirror of CLAUDE.md)

When all 10 of these are checked, Moon 1 ships:

1. ☐ Player walks village from spawn to first hero building
2. ☐ "Press E to tune" prompt appears
3. ☐ Mini-game opens, plays, succeeds or fails
4. ☐ Restoration VFX + audio on success
5. ☐ Win condition fires after 3 buildings
6. ☐ Anastasia reveal dialogue
7. ☐ Save/load preserves state
8. ☐ 30 minutes no exceptions
9. ☐ 60+ fps sustained
10. ☐ itch.io build uploaded

**Estimated:** 30 hours of agent time across all roles, depending on parallelism.

---

## What to do RIGHT NOW

If you've read this far and want to start:

1. Open VS Code in `C:\dev\TARTARIA_new`
2. Ctrl+Shift+I to open Copilot Chat
3. Switch to Agent mode (top of chat)
4. Paste the DIRECTOR prompt from above (step 1)
5. Let it propose 3 tasks
6. Open 3 more Copilot Chats and paste worker prompts
7. Watch the PRs land

If you want me (Claude in Cowork) to drive instead, just say "Claude, start the swarm" and I'll act as Director plus the 3 highest-impact agents in parallel.

---

*KICKOFF.md v1.0 · Read once. Then go.*
