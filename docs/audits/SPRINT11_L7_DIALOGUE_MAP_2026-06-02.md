# Sprint 11 Lane 7 — Dialogue Speaker → Yarn Node Map Gap Audit

**Date:** 2026-06-02
**Branch:** `agent/audit/dialogue-speaker-map`
**Worktree:** `C:\dev\_wt_s11_l7_dialogue`
**Tree SHA:** `e07660306026c2da2a1c222f26189c99a8fc4a3c`
**Scope:** Verify that every speaker name passed to `GameEvents.RaiseHUDShowDialogue` resolves through `YarnTutorialBinding.DefaultSpeakerToNode` to an actual Yarn node loaded into the `DialogueRunner`.

> Docs-only audit. No source edits. All claims cited to `file:line` at HEAD.

---

## 1. Method

1. **Callers** of `GameEvents.RaiseHUDShowDialogue` enumerated via `git grep -nE "RaiseHUDShowDialogue\(" -- "*.cs"`.
2. **Speaker → Yarn node lookup** read from `Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:31-37` (`DefaultSpeakerToNode`).
3. **Defined Yarn nodes** enumerated via `git grep -nE "^title:" -- "Assets/_Project/Dialogue/**/*.yarn"` plus `Assets/_Project/Data/Dialogue/*.yarn`.
4. **Runtime contract:** `YarnTutorialBinding.HandleHUDShowDialogue` (file `Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:101-135`) does:
   - `_speakerToNode.TryGetValue(speaker, out nodeName)` → warns `"No Yarn node registered for speaker \"{speaker}\""` on miss (`:112`).
   - `runner.NodeExists(nodeName)` → warns `"node \"{nodeName}\" is not loaded"` on miss (`:128`).
   - Yarn node names are **case-sensitive exact match** in `runner.NodeExists`.

---

## 2. Inventory

### 2.1 Every `RaiseHUDShowDialogue` call site (production code)

`git grep -n "RaiseHUDShowDialogue" HEAD -- "*.cs"` (filtered to actual invocations, excluding comments and the definition itself):

| # | Call site (`file:line`) | First arg (speaker string) | Source |
|---|---|---|---|
| 1 | `Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:447` | `speaker` (parameter of `SafeRaiseDialogue`) | wrapper |
| → | callers of `SafeRaiseDialogue` inside `MiloTutorialFlow`: |||
|  | `MiloTutorialFlow.cs:209` (Step1_Greet) | `kSpeaker` = `"Milo"` (def `:78`) | constant |
|  | `MiloTutorialFlow.cs:215` (Step2_PressE) | `kSpeaker` = `"Milo"` | constant |
|  | `MiloTutorialFlow.cs:222` (Step3_WalkWaypoint) | `kSpeaker` = `"Milo"` | constant |
|  | `MiloTutorialFlow.cs:229` (Step4_StartTuning) | `kSpeaker` = `"Milo"` | constant |
|  | `MiloTutorialFlow.cs:235` (Step5_RestoreComplete) | `kSpeaker` = `"Milo"` | constant |
|  | `MiloTutorialFlow.cs:241` (Step6_FreePlay) | `kSpeaker` = `"Milo"` | constant |
|  | `MiloTutorialFlow.cs:280` (SkipRemaining) | `kSpeaker` = `"Milo"` | constant |

**Single producer of speaker strings in the whole codebase: the constant `kSpeaker = "Milo"`** declared at `Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:78`.

No other `.cs` file calls `RaiseHUDShowDialogue` (verified `git grep -n "RaiseHUDShowDialogue" HEAD -- "*.cs"` returns only the wrapper definition at `MiloTutorialFlow.cs:447`, the `GameEvents.cs:617` method, and doc comments).

### 2.2 `YarnTutorialBinding.DefaultSpeakerToNode` entries

From `Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:31-37`:

| Map key (speaker) | Map value (Yarn node) | Defined at |
|---|---|---|
| `"Milo Brightway"` | `"Milo_TutorialIntro"` | `YarnTutorialBinding.cs:33` |
| `"Lirael"` | `"Lirael_Lullaby"` | `YarnTutorialBinding.cs:34` |
| `"Anastasia"` | `"Anastasia_Greeting"` | `YarnTutorialBinding.cs:35` |
| `"Cassian"` | `"Cassian_BossIntro"` | `YarnTutorialBinding.cs:36` |

**Runtime extensions via `RegisterSpeaker(...)`**: `git grep -n "RegisterSpeaker"` finds **zero** callers — the public method at `YarnTutorialBinding.cs:88` is never invoked by anything in the project. Map is frozen to the 4 defaults at runtime.

### 2.3 Every Yarn node defined in the project

From `git grep -nE "^title:" -- "*.yarn"` across:
- `Assets/_Project/Data/Dialogue/Milo_Intro.yarn` (1 node)
- `Assets/_Project/Dialogue/Echohaven/anastasia_reveal.yarn` (1 node)
- `Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn` (7 nodes)
- `Assets/_Project/Dialogue/Moon1/anastasia_greeting.yarn` (3 nodes)
- `Assets/_Project/Dialogue/Moon1/cassian.yarn` (10 nodes)
- `Assets/_Project/Dialogue/Moon1/lirael.yarn` (10 nodes)
- `Assets/_Project/Dialogue/Moon1/lore_whispers.yarn` (6 nodes)
- `Assets/_Project/Dialogue/Moon1/milo_intro.yarn` (40 nodes)

**Total: 78 defined Yarn nodes** across 8 files. Full list of relevant titles:

**Milo (41 nodes total):** `Milo_Intro`, plus the 7 `milo_tutorial_step_*` and `milo_tutorial_skipped` titles in `milo_tutorial.yarn`, plus 32 `milo_intro` / `milo_warming_up` / `milo_sincere` / `milo_tutorial_*` / `milo_discovery_*` / `milo_lore_*` / `milo_ambient_*` / `milo_combat_*` / `milo_celebration_*` titles in `milo_intro.yarn`.

**Lirael (10):** `lirael_first_meet`, `lirael_lullaby_request`, `lirael_about_tartaria`, `lirael_aether_band`, `lirael_about_blueprints`, `lirael_ambient_{1,2,3}`, `lirael_celebration`, `lirael_farewell`.

**Anastasia (4):** `anastasia_reveal`, `anastasia_greeting`, `anastasia_dome_restored`, `anastasia_fountain_restored`.

**Cassian (10):** `cassian_first_meet`, `cassian_about_tuning`, `cassian_about_restoration`, `cassian_milo_warning`, `cassian_anastasia_question`, `cassian_ambient_{1,2,3}`, `cassian_farewell_moon1`, `cassian_repeat_dialogue`.

**Lore whispers (6):** all `lore_*` titles in `lore_whispers.yarn`.

> **Convention:** every Yarn title in the repo is `snake_case_lower`. Not a single node is `PascalCase` or `CamelCase`.

---

## 3. Cross-reference table — speaker chain end-to-end

| Caller (`file:line`) | Speaker string passed | `YarnTutorialBinding` map hit? | Mapped node name | Yarn node exists? | Chain status |
|---|---|---|---|---|---|
| `MiloTutorialFlow.cs:209` | `"Milo"` | NO (only `"Milo Brightway"` is keyed) | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:215` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:222` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:229` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:235` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:241` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| `MiloTutorialFlow.cs:280` | `"Milo"` | NO | — | — | BROKEN (no map entry) |
| *(no caller)* | `"Milo Brightway"` | YES (`YarnTutorialBinding.cs:33`) | `Milo_TutorialIntro` | NO | BROKEN (orphan key + dead node) |
| *(no caller)* | `"Lirael"` | YES (`YarnTutorialBinding.cs:34`) | `Lirael_Lullaby` | NO (closest: `lirael_lullaby_request`) | BROKEN (orphan key + dead node) |
| *(no caller)* | `"Anastasia"` | YES (`YarnTutorialBinding.cs:35`) | `Anastasia_Greeting` | NO (closest: `anastasia_greeting`) | BROKEN (orphan key + dead node) |
| *(no caller)* | `"Cassian"` | YES (`YarnTutorialBinding.cs:36`) | `Cassian_BossIntro` | NO (closest: `cassian_first_meet`) | BROKEN (orphan key + dead node) |

---

## 4. Findings

### 4.1 RED — Speakers called but no map entry (`YarnTutorialBinding` warns at `:112`)

**1 distinct speaker × 7 call sites = the entire tutorial dialogue produces the warning observed in the console.**

- **`"Milo"`** — `MiloTutorialFlow.cs:78` defines `const string kSpeaker = "Milo"`. Used at 7 raise sites:
  - `MiloTutorialFlow.cs:209` Step1_Greet
  - `MiloTutorialFlow.cs:215` Step2_PressE
  - `MiloTutorialFlow.cs:222` Step3_WalkWaypoint
  - `MiloTutorialFlow.cs:229` Step4_StartTuning
  - `MiloTutorialFlow.cs:235` Step5_RestoreComplete
  - `MiloTutorialFlow.cs:241` Step6_FreePlay
  - `MiloTutorialFlow.cs:280` SkipRemaining

Every one of those raises produces:
```
[YarnTutorialBinding] No Yarn node registered for speaker "Milo" (message="…"). Skipping.
```
which matches the symptom NATRIX reported.

### 4.2 RED — Map entries whose target Yarn node is missing (`YarnTutorialBinding` warns at `:128`)

**All 4 default entries map to nodes that do not exist in any `.yarn` file.** Even if a future caller used these exact speaker strings, the runner would reject the node.

| Map entry (`YarnTutorialBinding.cs:line`) | Target node | Exists? | Closest existing node |
|---|---|---|---|
| `"Milo Brightway"` → `"Milo_TutorialIntro"` (`:33`) | `Milo_TutorialIntro` | NO | `milo_tutorial_step_1_brazier` (`milo_tutorial.yarn:1`) or `milo_intro` (`milo_intro.yarn:1`) |
| `"Lirael"` → `"Lirael_Lullaby"` (`:34`) | `Lirael_Lullaby` | NO | `lirael_lullaby_request` (`lirael.yarn`) |
| `"Anastasia"` → `"Anastasia_Greeting"` (`:35`) | `Anastasia_Greeting` | NO | `anastasia_greeting` (`anastasia_greeting.yarn:1`) |
| `"Cassian"` → `"Cassian_BossIntro"` (`:36`) | `Cassian_BossIntro` | NO | `cassian_first_meet` (`cassian.yarn:1`) |

Root cause: the map was authored in PascalCase/CamelCase per Sprint 7 Lane 6 spec, but the actual `.yarn` files use `snake_case` titles. **Casing mismatch across the board.** Yarn's `NodeExists` is case-sensitive.

### 4.3 Orphan Yarn nodes (defined but no map entry references them)

All **78** defined nodes are orphans relative to the speaker map — no `_speakerToNode` value points to any of them. The two most operationally important orphans are:

- The 7 tutorial nodes `milo_tutorial_step_{1..6}_*` and `milo_tutorial_skipped` in `Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn` — these were clearly authored to back the 7 `MiloTutorialFlow` raise sites but are wired to nothing.
- `anastasia_greeting`, `lirael_lullaby_request`, `cassian_first_meet` — the obvious target of the 3 broken default entries.

### 4.4 WHITE — Healthy speaker chains

**Zero.** Every chain in the table is broken.

---

## 5. Milo-specific call inventory (Sprint 11 Lane 7 mandatory section)

`MiloTutorialFlow.cs` declares one speaker constant and uses it at every raise site:

| Line | Code | Speaker passed | Line message |
|---|---|---|---|
| `MiloTutorialFlow.cs:78` | `const string kSpeaker = "Milo";` | (definition) | — |
| `MiloTutorialFlow.cs:209` | `SafeRaiseDialogue(kSpeaker, kStep1Line);` | `"Milo"` | `"Look toward the firelight, traveler."` (`:80`) |
| `MiloTutorialFlow.cs:215` | `SafeRaiseDialogue(kSpeaker, kStep2Line);` | `"Milo"` | `"Press E to interact with the world."` (`:81`) |
| `MiloTutorialFlow.cs:222` | `SafeRaiseDialogue(kSpeaker, kStep3Line);` | `"Milo"` | `"Follow the arrow. The buried cathedral is closer than it looks."` (`:82`) |
| `MiloTutorialFlow.cs:229` | `SafeRaiseDialogue(kSpeaker, kStep4Line);` | `"Milo"` | `"Press E at the green light."` (`:83`) |
| `MiloTutorialFlow.cs:235` | `SafeRaiseDialogue(kSpeaker, kStep5Line);` | `"Milo"` | `"One building back from the silence. You've got the knack now."` (`:84`) |
| `MiloTutorialFlow.cs:241` | `SafeRaiseDialogue(kSpeaker, kStep6Line);` | `"Milo"` | `"Explore. You have buildings to wake."` (`:85`) |
| `MiloTutorialFlow.cs:280` | `SafeRaiseDialogue(kSpeaker, kSkipLine);` | `"Milo"` | `"Suit yourself. The valley's yours to read."` (`:86`) |

**Every Milo raise site uses the exact string `"Milo"`.** No code path produces `"Milo Brightway"`. The console warning NATRIX saw — `[YarnTutorialBinding] No Yarn node registered for speaker "Milo"` — fires on the very first step of the tutorial and again on each subsequent step (7 times per tutorial run unless ESC is pressed first).

The intended target nodes for these 7 call sites already exist in `Assets/_Project/Dialogue/Echohaven/milo_tutorial.yarn`:

| `MiloTutorialFlow` step | Existing Yarn node (verified `title:` in `milo_tutorial.yarn`) |
|---|---|
| Step1_Greet (`:209`) | `milo_tutorial_step_1_brazier` |
| Step2_PressE (`:215`) | `milo_tutorial_step_2_interact` |
| Step3_WalkWaypoint (`:222`) | `milo_tutorial_step_3_waypoint` |
| Step4_StartTuning (`:229`) | `milo_tutorial_step_4_tune` |
| Step5_RestoreComplete (`:235`) | `milo_tutorial_step_5_restored` |
| Step6_FreePlay (`:241`) | `milo_tutorial_step_6_free` |
| SkipRemaining (`:280`) | `milo_tutorial_skipped` |

But the current map only knows how to route one Milo speaker label (`"Milo Brightway"`) to one node name (`Milo_TutorialIntro`) — and both sides of that arrow are wrong.

The structural mismatch: `MiloTutorialFlow` distinguishes seven different lines by *message*, not by *speaker*. `YarnTutorialBinding.HandleHUDShowDialogue` only routes by *speaker*. So even with a fixed `"Milo" → milo_tutorial_step_1_brazier` entry, steps 2-6 + skip would all play step 1's node. **The current binding shape cannot distinguish the 7 tutorial steps without an additional message-keyed lookup.**

---

## 6. Recommended fix (per `YarnTutorialBinding.DefaultSpeakerToNode`)

These are the minimum changes to clear the 4 broken default entries AND make Milo's tutorial play. Casing follows the existing `.yarn` titles (lowercase snake_case).

### 6.1 Replace the 4 broken default entries (`YarnTutorialBinding.cs:33-36`)

```csharp
// Speakers match exactly the strings passed to GameEvents.RaiseHUDShowDialogue
// by MiloTutorialFlow (kSpeaker = "Milo", MiloTutorialFlow.cs:78) and the planned
// Sprint 11 NPC dialogue raisers for Lirael / Anastasia / Cassian.
private static readonly Dictionary<string, string> DefaultSpeakerToNode =
    new Dictionary<string, string>
    {
        { "Milo",      "milo_tutorial_step_1_brazier" },  // entry node; subsequent steps wired via message lookup (see §6.2)
        { "Lirael",    "lirael_first_meet"            },
        { "Anastasia", "anastasia_greeting"           },
        { "Cassian",   "cassian_first_meet"           },
    };
```

That alone closes:
- the `"Milo Brightway"` orphan key (replaced with the actually-used `"Milo"`),
- the `Milo_TutorialIntro` dead node (replaced with the existing `milo_tutorial_step_1_brazier`),
- the `Lirael_Lullaby` / `Anastasia_Greeting` / `Cassian_BossIntro` dead nodes.

### 6.2 (Structural follow-up, NOT a one-line change — flag for Lane 7's downstream design ticket)

To make all 7 Milo tutorial steps route to their dedicated yarn nodes, `HandleHUDShowDialogue` needs a `(speaker, message) → nodeName` second-level lookup, OR `MiloTutorialFlow` needs to push the yarn-node name directly through a new event. Today the binding is single-key (speaker only), so steps 2-7 cannot reach their intended `milo_tutorial_step_{2..6}_*` / `milo_tutorial_skipped` nodes without an API extension. Recommend opening a ticket: *"Extend YarnTutorialBinding with message-keyed routing for multi-step tutorial speakers"*.

### 6.3 Optional — pre-validate node existence on bootstrap

`YarnTutorialBinding` already warns at runtime when a mapped node fails `NodeExists`. Add an editor-time validation pass that walks `DefaultSpeakerToNode` against the imported Yarn project so this regression cannot ship again silently. Out of scope for L7 (docs-only) but cited for the next Integration sprint.

---

## 7. Summary scorecard

| Metric | Value |
|---|---|
| Distinct speaker strings produced by callers | **1** (`"Milo"`) |
| `RaiseHUDShowDialogue` call sites (production code) | **7** (all in `MiloTutorialFlow`) |
| `DefaultSpeakerToNode` entries | **4** |
| Entries with caller backing | **0** |
| Entries pointing at a real Yarn node | **0** |
| Healthy speaker chains end-to-end | **0** |
| Broken chains (speaker without map entry) | **1 speaker / 7 call sites** |
| Broken chains (map entry → dead node) | **4 / 4** map entries |
| Orphan Yarn nodes (defined, unreferenced by map) | **78** |
| `RegisterSpeaker` runtime callers | **0** |

**Bottom line:** the dialogue→Yarn binding is 100% broken at runtime. Every Milo tutorial raise warns "No Yarn node registered". The 4 seeded fallback entries are unreachable AND point at non-existent nodes. Fix in §6.1 unblocks the tutorial's first node; §6.2 needed for the remaining 6 steps + skip.

---

*Audit produced by Sprint 11 Lane 7 agent. Tree SHA: `e07660306026c2da2a1c222f26189c99a8fc4a3c`. Docs-only — no source modified.*
