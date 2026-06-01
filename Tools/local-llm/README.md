# Local-LLM hand-off — TARTARIA

> Lightweight queue that lets a local model (Ollama) burn down boilerplate work
> so Claude tokens are reserved for reasoning-heavy stuff (cross-file refactors,
> bug diagnosis, playtest loop, strategic decisions).
>
> Created 2026-05-30 in response to NATRIX's question: *"can we set up local
> reasoning to build this game to cut down on claude tokens?"*

---

## What goes local vs. what stays on Claude

**Hand off to local model (high volume, well-specified):**

- Stub MonoBehaviour scaffolding from a clear field/method spec
- Boilerplate per-Moon enemy/NPC classes when the design doc is the spec
- One-off Python/PowerShell utilities (log parsers, asset import helpers, scene auditors)
- Doc regen from existing data (per-Moon tables from a JSON spec)
- Lint-style cleanups (missing usings, brace balance, formatting)
- Yarn dialogue stubs from a beat sheet
- ScriptableObject data assets from a CSV/table

**Keep on Claude (reasoning-heavy):**

- Bug diagnosis (Unity Console + screenshots + cross-file root-cause work)
- Refactors that touch >3 files or any assembly definition
- The playtest loop (driving Unity, reading visual feedback)
- Strategic decisions (Moon scope, when to pivot, swarm push-back)
- Anything that needs to synthesize `docs/15` + `STATUS.md` + 10 scripts + a runtime artifact

---

## Workflow

1. **Claude writes a ticket** into `LOCAL_TASKS/<name>.md`. Ticket format below.
2. **Run the launcher**: `pwsh tools/local-llm/Run-LocalLLM.ps1 [-Model qwen3-coder:30b]`
   - It iterates every `.md` in `LOCAL_TASKS/`, pipes the prompt into Ollama,
     and writes the output to `LOCAL_OUTPUTS/<name>/`.
   - The launcher does NOT touch `Assets/` directly — it only writes to its own
     outbox so a human (or Claude) reviews before commit.
3. **NATRIX (or Claude) reviews the outputs** and copies useful files to their
   real destinations. Bad outputs get re-ticketed with a tighter spec.

---

## Ticket format

A ticket is a markdown file in `LOCAL_TASKS/`. It must have:

```markdown
# TICKET: <one-line summary>

## Output destination
`Assets/_Project/Scripts/AI/MoonN_EnemyName.cs` (relative to repo root)

## Acceptance criteria
- Compiles against Unity 6 + Tartaria.AI asmdef (which already references
  Tartaria.Core and Tartaria.Gameplay).
- Brace balanced. No mixed line endings.
- One C# class per file. No Phase2Stubs duplicates.

## Spec
<the actual instructions — fields, methods, behavior, dependencies>

## Reference files (paste excerpts, don't link)
<inline excerpts from existing similar classes the model should mimic>
```

Keep tickets self-contained — the local model doesn't have repo access by
default. If a model NEEDS to look at files, set `-LocalRepoAccess` on the
launcher (advanced).

---

## Model recommendations

For C# scaffolding on consumer GPUs (24 GB or less):

| Model | Size | Notes |
|---|---|---|
| **Qwen3-Coder-30B** | 30 B (~18 GB) | First choice. Strong at C# + Unity API. |
| **DeepSeek-Coder-V2-16B** | 16 B (~10 GB) | Smaller, slightly weaker on Unity-specifics. |
| **Codestral-22B** | 22 B (~13 GB) | Solid generalist; Apache 2 license. |
| **gpt-oss-20b** | 20 B (~12 GB) | Good ratio if you want OpenAI weights. |

Pull with `ollama pull qwen3-coder:30b` then point the launcher at it.

---

## What the launcher does NOT do

- It does **not** modify `Assets/` directly. Outputs go to `LOCAL_OUTPUTS/`
  and a human reviews them.
- It does **not** run Unity. If a ticket needs Unity Editor automation, it
  belongs on Claude (or the local model can write the script, Claude/NATRIX
  runs it).
- It does **not** make architectural decisions. If a ticket is vague, the
  launcher refuses with `[LocalLLM] ticket too underspecified — kicking back`.
- It does **not** edit existing files. Always creates new files in
  `LOCAL_OUTPUTS/<ticket>/`. Merging into the real tree is a human step.

---

## Files in this folder

- `README.md` — this file
- `Run-LocalLLM.ps1` — the launcher
- `LOCAL_TASKS/` — inbox (Claude drops tickets here)
- `LOCAL_OUTPUTS/` — outbox (Ollama writes results here)
- `LOCAL_TASKS/EXAMPLE_stub-moon2-enemies.md` — proof-of-concept ticket
