# WORKTREE_MANDATE.md

> **One agent. One worktree. One branch.** Mandatory for every multi-agent sprint from Sprint 7 onward.
>
> 2026-06-02 - Born from Sprint 6 forensics: 8 of 10 lanes hit `.git/index.lock` races, 4 lanes had `.git/config` truncated mid-write, 1 lane lost in-flight edits to a sibling clobber, 1 lane couldn't push because credentials weren't mounted in its worktree.

---

## The rule

When the Director dispatches N parallel agents, the Director **creates N worktrees up front** and assigns one to each agent. Agents never share the `C:\dev\TARTARIA_new` checkout. Agents never call `git checkout` themselves - the worktree is already on the right branch.

```
C:\dev\TARTARIA_new                              <- Director's only checkout, sacred
C:\dev\_wt_<sprint>_<lane_short>                 <- per-lane worktree, agent's playground
```

## Director setup (before dispatch)

For each lane `L1..LN`:

```powershell
git worktree add C:\dev\_wt_sprintN_<short> -b agent/<area>/<short> feature/consolidate-moon-architecture
```

Then the agent prompt opens with:

> Your worktree is at `C:\dev\_wt_sprintN_<short>`. Your branch is `agent/<area>/<short>` (already checked out, already tracking origin via `-u` on first push). Do NOT touch `C:\dev\TARTARIA_new`. Do NOT call `git checkout`. Do NOT call `git stash`. Use only the worktree path I gave you.

## Director cleanup (after merge)

```powershell
git worktree remove C:\dev\_wt_sprintN_<short>
# Branch already merged: prune locally
git branch -d agent/<area>/<short>
```

## Why this works

- `.git/index.lock` lives per-worktree. No two agents touch the same lock file.
- `.git/config` is shared (single file at `C:\dev\TARTARIA_new\.git\config`), but worktrees don't write to it during normal operation - only `git remote add`, `git config`, `git push -u` (sets up tracking once). Agents that never touch remote config can't corrupt it.
- A failed agent leaves the worktree state behind for forensics - safer than `git stash` ping-pong.
- The shared `.git/objects` directory is append-only and lock-coordinated by git itself - no risk there.

## When the rule does NOT apply

- Solo work where you're the only writer (no parallel agents in flight).
- Doc-only edits in `C:\dev\TARTARIA_new` while no sprint is in flight.
- Bug-fix branches dispatched serially.

## Anti-patterns observed in Sprint 6

| Anti-pattern | What happened | Fix |
|---|---|---|
| Agent ran `git stash` to "save" mid-edit work before checking out a different branch | Other agent's stash collided, work lost | Never stash. Work only on your assigned worktree. |
| Agent ran `git checkout <other-branch>` to "verify a sibling's file" | Working tree files swapped under sibling agent's nose, edits lost | Read the file via raw git: `git show feature/...:path/to/file`. Never check out a sibling's branch. |
| Two agents `git push`ed concurrently and both wrote to `.git/config` `[remote "origin"]` | One of them truncated mid-line, the other re-added the remote section, the third pushed and lost tracking | Push happens once per agent, late in its lifecycle. Worktrees inherit the parent `.git`'s `[remote "origin"]` - no need for the agent to touch it. |
| Agent worked directly in `C:\dev\TARTARIA_new` while another agent was on a different branch there | Sibling deleted 17k files via `git checkout -- .` mid-edit | Agents NEVER touch `C:\dev\TARTARIA_new`. Worktree only. |
| Director gave an agent a branch name without first creating the worktree | Agent had to `git checkout -b` and inherited whatever working tree was current - usually the dirty Lane 10 state | Director creates worktree FIRST with `-b <branch>`, THEN gives the agent the path. |

## Dispatch prompt template

When you fire a parallel-swarm message, this is the per-lane prompt opener:

```
Lane <N>: <title>

WORKTREE (do not deviate):
- Path: C:\dev\_wt_sprint<N>_<short>
- Branch: agent/<area>/<short> (already checked out, tracking origin)
- Do NOT cd anywhere else. Do NOT call git checkout, git stash, git worktree, git remote, or git config.

PROCESS:
1. cd C:\dev\_wt_sprint<N>_<short>
2. Read CLAUDE.md (mandates), docs/agents/API_CONTRACT.md, docs/agents/COORDINATION.md
3. Grep canonical sources for every external API you touch; quote file:line in your report
4. Write code
5. git add <files> && git commit -m "<msg>"
6. git push -u origin agent/<area>/<short>
7. Report back: files, grep evidence, branch, SHA, blockers
```

---

*Born from Sprint 6 forensics. Enforced from Sprint 7.*
