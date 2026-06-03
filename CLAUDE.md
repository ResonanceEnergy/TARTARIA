# CLAUDE.md — TARTARIA Project Instructions

> This file is the first thing future Claude sessions should read when given access to `C:\dev\TARTARIA_new`. It exists because this project has accumulated 217+ historical agent reports that no longer reflect reality, and a fresh agent needs to know which docs to trust.

---

## ⚡⚡⚡⚡⚡⚡ 2026-06-03 SPRINT 13 VERDICT — MOON 1 SHIPPABLE (post-MicroSprint, supersedes Sprint 11)

**Sprint 11's NOT-SHIPPABLE verdict has been honored and worked through.** 33 lanes shipped across Phase 0 → Hammer → MicroSprint. Sprint 13 audit (HEAD `c2756942`, doc `docs/audits/MOON1_ACCEPTANCE_SPRINT13_2026-06-02.md`) measures 5 of 9 dimensions GREEN, 2 YELLOW, 2 PARTIAL — verdict **SHIPPABLE**.

### Where Moon 1 stands at HEAD `~1ed80d2e`

| # | Dimension | Sprint 11 baseline | Sprint 13 | Status |
|---|---|---|---|---|
| 1 | Stubs | 9 ship-blockers | 0 ship-blockers (M2-13 OOS) | GREEN |
| 2 | Silent fails | 38 | 1 (intentional UnityException) | GREEN |
| 3 | Commented wiring | 5 active-path bugs | **0** (MS.L2-L5 fixed) | GREEN |
| 4 | Workarounds | 5 incl. EmergencyBypass | RuntimeSpawnerInsurance only; gate restored | YELLOW |
| 5 | `!u!115` orphans in scene | 4 | 0 | GREEN |
| 6 | Prefab integrity | 0/390 healthy | 70 FBX healthy, Cathedral fixed, Player.prefab + NPC variants real | GREEN |
| 7 | Dialogue speaker map | 0/4 chains worked | 4/4 snake_case resolve | GREEN |
| 8 | Scene authoring | ECS 70+, Rocker 275 LOC | ECS 5 #if gates, Rocker 133 LOC; HUDBuilder still 64 (deferred) | YELLOW |
| 9 | GameEvents pairs | 25 broken one-sided | 3 missing events declared + publishers + subscribers wired | GREEN |

### Sprint 13 dirty-worktree caveat (carried forward as note, not blocker)

`C:/dev/_wt_sprint13` worktree had 12 dirty files during audit, including `DayNightCycleController.cs` truncated at line 125 mid-method. **Committed HEAD `c2756942` was intact (159 lines)** — the dirt was working-tree-only. Next session: `git checkout -- .` on any agent worktree before resuming.

### Unity MCP bridge auto-start (new this session)

`Assets/_Project/Scripts/Editor/Moon1MCPAutoStart.cs` flips 3 EditorPrefs (`MCPForUnity.AutoStartOnLoad=true`, `UseHttpTransport=true`, `HttpTransportScope=local`) on first compile after install + requests a script reload so `HttpAutoStartHandler` picks up the new state. **Future Unity launches bring the HTTP MCP bridge up on `http://127.0.0.1:8080/mcp` with zero GUI clicks.** Documented in `docs/integration/UNITY_MCP_SETUP.md` § "Per-Unity-boot startup".

### Manual bake menus deferred (MCP transport flaked on long-running calls)

These 5 Editor menus exist on disk and are runnable from `Tartaria/...` menu in Unity. They were NOT run by Cowork due to MCP WebSocket transport disconnects on long calls. NATRIX can fire them manually:

1. `Tartaria/6 Bake/Bake Anastasia Rocker Prefab` — creates `Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab`
2. `Tartaria/6 Bake/Bake Echohaven Content Into Scene` — already fired (scene mtime confirms)
3. `Tartaria/6 Bake/Bake StarDome Built Variant` — `Echohaven_StarDome_Built.prefab` already exists from H.L4
4. `Tartaria/8 Fix/Convert Hero Prefabs to Text Mode` — already done by H.L8 (idempotent)
5. `Tartaria/1 Build/Replace Hero Building Detail_* Primitives With Kit Meshes` — now unblocked by H.L8

### Win64 build path

`scripts/dev/build-moon1-win64-smoke.ps1` is the smoke runner. Close Unity first (its Library lock blocks batchmode build). Outputs to `Builds/itch_assets/TARTARIA_Moon1.zip`.

### Session 2026-06-03 final state

- 35 lanes shipped (P0 → P1 → P2 → P4 → P5 → Hammer → MicroSprint → Prefab Hygiene → 5 Editor script restorations → MCP bridge auto-start)
- HEAD `1ed80d2e` on `feature/consolidate-moon-architecture`
- 0 compile errors (10 obsolete-API warnings + 1 unused var warning remain, non-blocking)
- 625 prefabs reorganized into per-Moon buckets + Moon2-13 scaffolded + `docs/PREFAB_LAYOUT.md` authored
- 1 ProbeVolume URP shutdown NRE remains (rendering cleanup race, non-blocking)

---

## ⚡⚡⚡⚡⚡ 2026-06-02 SPRINT 11 HONEST RESET — MOON 1 NOT SHIPPABLE (superseded by Sprint 13 above; preserved for context)

**Sprint 11's full-scope audit shipped 10 grep-cited branches and produced a brutal verdict: Moon 1 is NOT shippable today. Prior audits ("v2 — 70/15/3 ✓/⚠/❌", "SHIPPABLE pending 2-4 hr") measured *file presence*. Sprint 11 measured *runtime feature behavior*. Divergent results.**

### What Sprint 11 actually found (origin branches, all grep-cited)

| Surface | Reality |
|---|---|
| Prefabs | **0 of 390 healthy at runtime.** Every FBX is a 130-byte Git LFS pointer (not pulled). Cathedral kit uses Unity primitives + invalid 16-char material GUID → magenta. Player.prefab is a capsule with **zero MonoBehaviours**. Lirael/Anastasia/Cassian/Milo are unanimated capsules. (L6 `e9bbc612`) |
| Dialogue | **0 of 4 default speaker chains work.** YarnTutorialBinding map uses PascalCase ("Milo Brightway") but MiloTutorialFlow passes "Milo" and Yarn nodes are snake_case. NodeExists is case-sensitive. Entire tutorial dies silently. (L7 `cec511a9`) |
| GameEvents | 35 healthy / 4 unused / **25 BROKEN one-sided**. CLAUDE.md canonical-facts table claims `OnBrazierLit / OnBrazierRingComplete / OnDayChanged` exist — **they don't.** Lirael Day-25, brazier ring, day cycle blocked. (L9 `72457de3`) |
| Silent fails | 38 empty catches. Top 5 ALL on Moon 1 happy path (Moon1NarrativeBeats, Moon1CinematicMoments, TuningPedestalLink, QuestObjectiveTrackerUI, AdaptiveMusicController). (L2 `b33eb621`) |
| Stubs | 9 ship-blockers: PickupInteractable hardcoded fail, CymaticWaterTuningMiniGame is 7 empty methods, HUD shows hardcoded `RS:0 / Aether:75%` every frame, PlayerAbility RS economy decorative, AetherFieldSystem hardcoded player pos. (L1 `1fb03541`) |
| Commented-out wiring | 5 active-path bugs: PlayerAbilityController/Manager combat economy commented, PlayerWeaponSwitcher Awake body commented, DayNightCycle.UpdateAetherBoost discards result, AutomatedPrefabWiring NavMesh bake commented. (L3 `1587acab`) |
| Workarounds | RuntimeSpawnerInsurance papers over missing scene authoring. Moon1RescueDriver uses banned legacy `Input.GetKey` bypassing PlayerInputHandler. PlayerInputHandler has `// EMERGENCY BYPASS` that removes pause/dialogue gate. PlayerSpawner adds CharacterController + PlayerInputHandler at runtime because Player.prefab ships incomplete. (L4 `f30f9a29`) |
| Moon1_Systems orphan spam | **ROOT CAUSE FOUND**: 4 inline `!u!115 MonoScript` stub blocks at scene YAML lines 305/1252/3090/3364 pointing at deleted classes (Moon1NPCSpawner, Moon1AmbientCreatures, Moon1MaterialSetup, Moon1HeroBuildingSpawner). `RemoveMonoBehavioursWithMissingScript` can't see inline `!u!115`. New `Tartaria/8 Fix/Deep-Clean Moon1_Systems Prefab` menu does YAML surgery. (L5 `9500ccfe`) |
| Scene authoring | Scene has 48 GameObjects. Boot chain creates **134+ at runtime**. 95% of Echohaven rebuilt every Play. 4-layer player spawn redundancy. EchohavenContentSpawner fires `new GameObject` 70+ times. (L8 `50ff78ea`) |

### The plan

**Read `docs/plans/MOON1_TO_100_PLAN.md`** — the phased roadmap, every step grep-cited, every estimate honest.

**Read `docs/best-practices/UNITY_6_PATTERNS.md`** — Unity 6 manual best practices distilled for our patterns (LFS, Singleton bootstrap, Prefab variants, Mecanim humanoid, Input System, URP materials, Build pipeline, Scene management).

**Read `docs/audits/SPRINT11_FULL_SCOPE_PUNCHLIST_2026-06-02.md`** (on `agent/audit/sprint11-synthesis` `6fe7ffef`) — the master punch list synthesized from the 10 audit lanes.

### Mandates carried forward

- **PARALLEL MANDATE** (2026-06-01) — swarms run in parallel, never serial. Director creates N worktrees up front.
- **WORKTREE MANDATE** (`docs/agents/WORKTREE_MANDATE.md`) — one agent, one worktree, one branch. Zero `.git/config` corruption in 30+ lanes since adopted.
- **API_CONTRACT** (`docs/agents/API_CONTRACT.md` v2) — grep every event/method before invoking, cite file:line in PR summary, no invented names.
- **NO-DEBT** — no silent catches, no fabricated runtime artifacts, no `EmergencyDriver`/`GodMode`/`Override` bypass patterns. Workarounds must have a documented root-cause-fix backlog.
- **NO-STUBS** — no `// TODO: implement`, no method bodies that only log "not implemented", no primitive `GameObject.CreatePrimitive` without URP-safe shader fallback.

### Honest reset on prior CLAUDE.md claims

The earlier "Sprint 9/10 — Moon 1 SHIPPABLE" assessment is **rescinded**. It was based on static file-presence audits. Sprint 11 measured runtime behavior and found pervasive fake features. The phased plan is the path to a real Moon 1.

---

## ⚡⚡⚡ 2026-06-01 PARALLEL MANDATE (supersedes everything below)

**SWARM RUNS IN PARALLEL — NEVER SERIAL.** Per NATRIX: *"ENSURE THE SUBAGENTS ARE WORKING IN PARRELLE TO MAXIMIZE VALUE AND TIME"*

When N independent tasks exist, spin up all N agents in one batch — no serial queue. Sibling agents don't block on each other (append to `docs/HANDOFFS.md` and keep working). Cowork drives runtime QA in parallel with VS Code authoring. One batched dispatch prompt per round, not N drip-fed prompts.

---

## ⚡⚡ 2026-05-30 LATE-NIGHT MANDATE

**NO STUBS. NO PLACEHOLDERS. BUILD EVERYTHING OUT.**

Per NATRIX, verbatim: *"no stubs no placeholders build everything out update claude.md to reflect this and keep building moon 1 visual assets objects environment buildings minigames build everything"*

Concrete operating rules going forward:

1. **NEVER ship a file with `// TODO: implement` or `// stub` or method bodies that only contain `;` or `Debug.Log("not implemented yet")`.** If a method exists, it must do the thing.
2. **NEVER write an interface-only class.** A "public API" with method declarations and no bodies is a stub — flesh out the bodies.
3. **NEVER leave a `.candidate` file unresolved.** Either swap it in, delete it, or document why it stays.
4. **NEVER use `GameObject.CreatePrimitive` without an immediate URP-safe fallback path** that sets `_BaseColor` and tags the line with `// URP-safe`. Better: don't use primitives at all — load the real KayKit FBX or Cathedral kit prefab.
5. **When the local LLM (Ollama) returns a thin stub** (< 25% of the destination file's line count, or only method signatures), REJECT IT — don't apply. Either re-write the ticket with sharper spec, or implement Claude-side directly.
6. **Visual asset wireup is part of "building it out".** If a prefab exists in `Assets/_Project/Prefabs/`, the code that creates the gameplay version MUST load that prefab via `AssetDatabase.LoadAssetAtPath<GameObject>` (Editor) or `Resources.Load<GameObject>` (Runtime). Building from primitives is failure.
7. **No "next round" deferrals on placeholder content.** If we discover a stub mid-session, finish it before declaring the session done.

Build-order priority within each Moon:

**Buildings (real prefabs, not primitives) → Objects/Props (KayKit FBX) → Environment detail → Mini-game variants (all playable) → Characters/NPCs (real models) → Combat polish (real loot drops, real VFX) → Quest/narrative beats → Audio/VFX hookup → Done → Move to next Moon.**

---

## ⚡ 2026-05-30 MANDATE FROM NATRIX (supersedes the older itch/demo framing)

**Build the WHOLE game, full content, before touching demos or release.**

Per NATRIX, verbatim: *"why are you worried about itch and demo? lets go on the real work update the context to reflect this update claude to finish moon 1 then 2 then 3 till game is ready then maybe we consider demo get building"*

That means:
1. **Finish Moon 1 fully** — not a vertical slice, not a demo. ALL buildings (3 hero + 9 village = 12 minimum), ALL objects/props, ALL environment detail, ALL 3 tuning mini-game variants per `docs/15 §9`, ALL Moon 1 characters (Milo + Anastasia + Lirael + Cassian).
2. **Then Moon 2 fully.** Then Moon 3. Then 4. Etc.
3. **The 310+ stub `MoonN*.cs` files DO get replaced** — that rule from the older mandate is reversed. They should be built into real systems, one Moon at a time.
4. **No release discussions until the game is fully built.** Don't talk about itch.io drops, Steam pages, demos, vertical-slice ship dates, etc. — those are post-build problems.
5. **Track A / Track B distinction is no longer load-bearing.** There's one track: build the game.

When NATRIX asks for "what's next" or "keep building", drive toward the Moon currently in flight in this order:

**Buildings → Objects/Props → Environment detail → Mini-game variants → Characters/NPCs → Combat polish → Quest/narrative beats → Audio/VFX hookup → Done → Move to next Moon.**

Doc references that still apply: `docs/15_MVP_BUILD_SPEC.md` for Moon 1 content depth (treat its "vertical slice" sections as the **minimum** for Moon 1, not the maximum), `docs/03_CAMPAIGN_13_MOONS.md` and the per-Moon docs for Moons 2–13 scope, `PHASE_1_SCOPE.md` is **archived** as historical scope (don't enforce its deferrals).

---

## Project identity

**Project:** TARTARIA WORLD OF WONDER — Aether Awakening
**Owner:** NATRIX (nate@gripandripphdd.com)
**What:** Unity 6 single-player RPG / restoration / city-builder hybrid for PC.
**Pricing model:** Premium / pay-what-you-want (NOT free-to-play, despite what older docs may say). Distribution platform TBD post-build.
**Where it's headed:** Full game across all 13 Moons, built in order, before any release discussion.
**Where it actually is:** Alpha 0.4 — Moon 1 environment built out (3 hero buildings buried at spec depths, 6 POIs, 120 vegetation instances, golden-hour atmosphere, restoration VFX + raise animation, working player movement, tuning mini-game variant A). Next: 9 village buildings, props, mini-game variants B+C, full NPC set.

---

## Read these first (in this order)

1. **`STATUS.md`** — the single source of truth for current state.
2. **`PHASE_1_SCOPE.md`** — the scope lock. If a task isn't in this file, it's deferred.
3. **`TARTARIA_MASTER_PLAN.md`** — the strategic plan (Track A ship the game, Track B build the platform).
4. **`KNOWN_ISSUES.md`** — open bugs with file:line cites.
5. **`docs/15_MVP_BUILD_SPEC.md`** — the canonical MVP design spec.
6. **`docs/09_TECHNICAL_SPEC.md`** — Unity architecture spec.

If you only have time for one, read `STATUS.md`. It cites the others.

---

## Do NOT trust these (preserved for record only)

These docs contain claims that are wrong, outdated, or were AI-agent self-attestation without evidence:

- **Any file under `docs/agent_reports/`** — three swarms of AI agents reported their own work, often with theater elements (Bond-villain epigraphs, "BULLETPROOF" sign-offs, 100/100 self-grading). Real work landed, but you cannot tell which is which without cross-checking against the code. Treat as archeology, not specification.
- **`docs/agent_reports/beta_qa/MASTER_BETA_QUALITY_REPORT.md` and siblings** — declared "BETA READY · 100/100" days before `GameEvents.cs` was hand-patched. Invalid.
- **`docs/agent_reports/moon_completion/MOON*_COMPLETE.md`** — these are file inventories with line counts, not evidence of playable content. Moon 1 is partially real; Moons 5–13 are mostly template-generated stubs.
- **Any "100% done" claim that isn't from `STATUS.md`** — the project is not 100% anything except design docs.

---

## The swarm-discipline rule (per `TARTARIA_MASTER_PLAN.md` § 9)

If NATRIX asks you to "run a swarm" or "build agents to do X," push back. The history of this project is that vague-mission swarms generate enormous code volume without runtime artifacts to verify it. **Real work requires:**

1. A **falsifiable scope** ("delete circular dep X from `Tartaria.Integration.asmdef`", "make `Echohaven_VerticalSlice.unity` compile without missing-script refs"), not a vague mission.
2. A **runtime artifact** at the end (a build, a video, a Unity Test Runner log, a screenshot). Not a self-graded report.
3. **No regeneration of Moon 2–13 stub systems.** The 310+ `MoonN*.cs` files that `GameObject.CreatePrimitive(Sphere)` and `Debug.Log` an emoji should be left alone or replaced one-by-one with real implementations. Do not regenerate the whole pile.
4. **No architectural rewrites of `Tartaria.Core`** while Moon 1 is in flight.

If a swarm completes and the only output is markdown files, you've failed. Insist on a runtime artifact.

---

## Where things live (post-hygiene, 2026-05-29)

### Root-level (the only docs that should be at root)

| File | Purpose |
|---|---|
| `README.md` | Public-facing intro, honest about alpha status |
| `STATUS.md` | Current state of play |
| `PHASE_1_SCOPE.md` | Moon 1 scope lock |
| `TARTARIA_MASTER_PLAN.md` | Strategy |
| `KNOWN_ISSUES.md` | Live bug tracker |
| `TROUBLESHOOTING.md` | Player support |
| `CONTRIBUTING.md` | Contributor guidelines |
| `CHANGELOG.md` | Version history |
| `ROADMAP.md` | Older system-level done-list (kept for reference) |
| `CLAUDE.md` | This file |

### Unity project

- `Assets/_Project/Scripts/` — game code, organized into 23 assemblies
- `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` — the MVP scene (the only one worth playing right now)
- `Assets/_Project/Scenes/Moons/Moon*.unity` — Moons 2–13 scene shells (mostly empty)
- `Assets/_Project/Prefabs/` — game-owned prefabs (Moon 1 cathedral kit + characters + VFX)
- `Assets/KayKit_*` — vendor art assets
- `Assets/Hovl Studio` — vendor VFX
- `Library/`, `Temp/`, `Logs/`, `obj/`, `Builds/`, `Build/` — Unity-generated, not in git

### Design docs

- `docs/00_*` through `docs/33_*` — 30+ main design docs (GDD, lore, combat, etc.)
- `docs/appendices/A_*` through `J_*` — 10 appendices
- `docs/dlc/DLC_01_*` through `DLC_10_*` — 10 DLC docs
- `docs/agent_reports/` — historical AI swarm reports (DO NOT TRUST status claims)
- `docs/archive/` — superseded status docs, old asset inventories, old README versions

### Automation (post-2026-05-29 hygiene)

- `scripts/` — PowerShell production build/test/dev automation (36 files)
- `scripts/dev/` — active dev launchers + .bat entry points (14 files): `Launch-Unity.ps1`, `tartaria-play.ps1`, `vex-launch.ps1`, `PLAY_GAME.ps1`, `Preflight-Check.ps1`, `Setup-AudioFolders.ps1`, etc.
- `scripts/dev/analysis/` — scene/prefab/asset analysis tools (11 files)
- `scripts/dev/asset_pipeline/` — asset import + wiring scripts (5 files)
- `scripts/archive/emergency_fixes_may2026/` — 25 one-time fix scripts (`Fix-GameEvents*.ps1`, `fix-part1..6-*.ps1`, etc.) preserved for history
- `scripts/archive/duplicates/` — 20 root-level copies that were already in `scripts/`
- `Tools/` — Editor automation scripts (Unity-side .ps1 launchers + Tools/Phase1/)
- Root no longer has any `.ps1`/`.bat` files

### Editor tools

- `Assets/_Project/Scripts/Editor/PrefabGeneratorTool.cs` — Unity menu: `Tartaria → Prefab Generator`
- `Assets/_Project/Scripts/Editor/AutomatedPrefabWiring.cs` — Unity menu: `Tartaria → Automated Prefab Wiring`
- `Assets/_Project/Scripts/Editor/EchohavenSceneAudit.cs` — Unity menu: `Tartaria → Scene Audit: Echohaven`. Editor-mode audit that checks the scene for blockers (PlayerSpawner, NavMesh, building presence, prefab refs, missing scripts) WITHOUT entering Play mode. Pair with `scripts/dev/audit-echohaven-scene.ps1` for batchmode invocation that exits 1 on blocker.
- Other diagnostic tools: `BatchReadinessValidator.cs`, `DiagnoseRuntime.cs`, `CleanMissingScripts.cs`, `FixEchohavenMissingScripts.cs`.

#### `Tartaria/0 ★ MASTER/` — canonical bootstrap (Hammer Lane 6, 2026-06-02, Sprint 11 L8 `50ff78ea`)

The `Tartaria/0 ★ MASTER/` submenu previously had three overlapping "run-everything" entries. They were consolidated; the canonical scene bootstrap is now:

- **`Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs`** — Unity menu: `Tartaria → 0 ★ MASTER → Bootstrap All Moon 1 Systems` (priority 30). Idempotent. Creates / reuses the `Moon1_Systems` GameObject in the active scene, attaches the Moon1 Integration MonoBehaviours (TartarianHourCycle, Moon1NarrativeBeats, Moon1DialogueBindings, EchohavenContentSpawner, AnastasiaController, LiraelController, EchohavenProgressionSystem, ZoneController, etc.), then auto-chains `Moon1WireSpawnerPrefabs.RunAll()` so prefab refs are wired in one click. This is what you fire before Play.

Superseded (menu hidden, run logic preserved for direct invocation):

- `Moon1Tier1Master.cs` — was `Tier 1 — FBX + Terrain + Splats + Lighting`. Asset-pipeline sequencer, not a scene bootstrap. Call `Moon1Tier1Master.Run()` directly if you need it.
- `Moon1AllTiersMaster.cs` — was `Run ALL Tiers (Everything)`. Asset-pipeline + VFX/Audio superset, also fired the now-hidden Tier 1 menu. Call `Moon1AllTiersMaster.Run()` directly if needed.

See `docs/agents/MASTER_BOOTSTRAP_CANONICAL.md` for the consolidation rationale.

---

## Common tasks and how to do them

### "Fix the compile errors"

The known issue (as of 2026-05-29 04:40) is `Scripts/Core/GameEvents.cs` was hand-patched and may not be clean. There's a backup at `Scripts/Core/GameEvents.cs.BEFORE_FIX_20260528_223633`. Diff them and pick the compiling version. The example file `Scripts/Examples/GameEventsUsageExample.cs.disabled` tells you what subscribers expect to find.

### "Get the player to spawn"

Per `docs/agent_reports/sessions/AUDIT_REPORT_SESSION6.md` and `STATUS.md` § 3 Day 2:

1. Open `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity`
2. Add empty GameObject named `PlayerSpawner` at (0, 1, 0)
3. Attach `PlayerSpawner` component
4. Assign `Assets/_Project/Prefabs/Characters/Player.prefab`
5. `Window → AI → Navigation → Bake`
6. Hit Play

### "Help me organize the repo"

Repo hygiene was done 2026-05-29 — moved 217 files out of the root into `docs/agent_reports/` and `docs/archive/`. Don't undo that. If something new lands at the root that's a session report or audit, it goes to `docs/agent_reports/sessions/`. If it's a one-off status, it goes to `docs/archive/superseded_status/`. The root should stay at ~10 files.

### "Help me build out Moon 2" / "polish Moon 5" / "wire boss for Moon 11"

This is out of scope per `PHASE_1_SCOPE.md`. Push back gently: "Per the scope lock, Moons 2–13 are deferred until Moon 1 ships on itch.io. Would you like to instead [related Moon 1 task]?"

### "Give me a status report"

Read `STATUS.md` and answer from there. Don't generate a new status report — that's how this project ended up with 30+ conflicting status docs in the first place. If `STATUS.md` is more than 2 weeks old, propose updating it rather than writing a parallel doc.

### "Run another agent swarm to do X"

Apply the swarm-discipline rule above. If X has a falsifiable scope and produces a runtime artifact, OK. If X is vague ("polish the game"), push back.

---

## Things that have been decided and shouldn't be re-litigated

These were decisions from prior sessions documented in `TARTARIA_MASTER_PLAN.md` and `PHASE_1_SCOPE.md`. Don't reopen without explicit NATRIX approval:

- **Premium pay-what-you-want pricing on itch.io, NOT F2P.** The F2P stack in older `docs/08_MONETIZATION.md` and `docs/19_ECONOMY_BALANCE.md` is dead.
- **PC-first via Steam + itch.io.** Mobile/iOS reference in older docs is a stale platform pivot.
- **Aether band naming: Telluric (7.83 Hz) / Harmonic (432 Hz) / Celestial (528 Hz).** Resolves the doc 02 vs doc 15 contradiction. Use 528 not 1296 for the top band.
- **Track A (Moon 1 ship) and Track B (platform/modding) are separate branches.** Track B never touches Track A's flight path.
- **No Moon 2–13 stub regeneration.** The 310+ template-generated `MoonN*.cs` files stay as-is or get replaced one-by-one with real systems, not batched.
- **No Steam achievements, Steam Cloud, Steam trading cards in Phase 1.** itch.io ship first.

---

## Open lore/political risk callouts (unresolved)

From the earlier doc review (`outputs/TARTARIA_DEV_REVIEW.md`), these items need a sensitivity-reader + legal pass before any public marketing, and have not been addressed in the local repo:

- Mud-flood / Tartaria conspiracy framed as canonical truth instead of in-fiction myth in `docs/01_LORE_BIBLE.md`
- "Reset agents" antagonist name maps to current-events political flashpoint (Great Reset)
- "Parasite Cabal" villain naming matches known antisemitic dogwhistle patterns
- Princess Anastasia borrows the Romanov name without acknowledging the historical family
- Orphan Train framed as "cultural genocide" in `docs/03_CAMPAIGN_13_MOONS.md`

If you (Claude) are asked to help write marketing copy, a Steam page, or any public-facing content, flag these before drafting. Renames are cheap; ship-and-recall is expensive.

---

## Working style with NATRIX

Based on session history:

- NATRIX wants action over more documentation. When asked to organize, organize; don't produce a 5-page plan for organizing.
- NATRIX has been running AI swarms heavily — be willing to push back when a swarm idea would regenerate the same trap.
- NATRIX is the owner / sole developer / creative director / de facto producer. Treat your role as "engineer + technical PM helping NATRIX execute," not "AI advisor pitching strategies."
- When in doubt, propose the smallest concrete next step that produces a runtime artifact (build, video, test log) the human can inspect.
- NATRIX's typing pattern includes informal grammar and ellipses ("hit a wall with vs code .." / "code is 95%?"). Match the working tone without being either over-formal or over-cute.

---

## What this session (2026-05-29) accomplished

**Documentation hygiene:**
- Read all roadmap, mandate, goal, recent-change docs
- Identified 4 swarms' worth of agent reports masquerading as ground truth
- Categorized and moved 217 `.md` files from root to `docs/agent_reports/` and `docs/archive/`
- Wrote `STATUS.md` (single source of truth), `PHASE_1_SCOPE.md` (scope lock), new `README.md` (honest alpha), `CLAUDE.md` (this file)
- Preserved old README at `docs/archive/README_old/README_v1.0.0-beta_2026-05-22.md`

**Script hygiene:**
- Moved 74 root-level `.ps1` and 3 `.bat` files into structured `scripts/` subdirs (zero deletions, preserved per MASTER_PLAN mandate)
- 25 one-time emergency fix scripts archived to `scripts/archive/emergency_fixes_may2026/`
- 20 duplicates of existing `scripts/` files archived to `scripts/archive/duplicates/`
- 11 analysis tools + 5 asset pipeline tools + 14 active dev launchers categorized into `scripts/dev/`
- Misc loose ends (CSV, patch, ~27 log/txt build artifacts) moved to appropriate archives

**Code work:**
- Fixed `Assets/_Project/Scripts/Core/GameEvents.cs` — was truncated at line 804 (mid-class). Reconstructed the missing tail (5 EventArgs classes + namespace close). Added missing `OnQuestCompleted` (`Action<string>`) and `OnAetherVisionToggled` (`Action<bool>`) events that subscribers reference. Wired them from existing Fire/Raise methods. Brace-balanced (192/192). All 105 subscriber refs now resolve.
- Archived broken backups to `Assets/_Project/Scripts/Core/_archived_backups/`

**The next session should execute Day 2 of `STATUS.md` § 3** — open Unity, add `PlayerSpawner` GameObject to Echohaven scene, bake NavMesh, hit Play, confirm WASD movement works.

---

*CLAUDE.md v1.0 · 2026-05-29 · Update this file when reality drifts from it.*

---

## ⚡ 2026-05-31 ART PIPELINE — Blender + Headless Generation (PROVEN WORKING)

Per NATRIX *"can you run blender do research"* + *"BOOM! lets crank out the assets!"*:

The art pipeline is verified end-to-end. Blender 4.5.4 LTS runs headlessly to generate Unity-ready FBX. **12 assets shipped tonight in seconds of Blender runtime.**

### Pipeline architecture

```
tools/blender/gen_*.py  →  Blender --background --python  →  *.fbx (Kaydara 7400 binary)
                                                                  ↓
                            Assets/_Project/Models/Blender/Moon1/*.fbx
                                                                  ↓
                            BlenderImportPostprocessor.cs (auto URP/Lit + prefab variant)
                                                                  ↓
                            Assets/_Project/Prefabs/Moon1/Blender/*.prefab
```

### Files

- `tools/blender/_common.py` — cross-platform path detection, reset_scene, make_material (Blender 4.x "Emission Color" socket), export_fbx with Unity-friendly -Z/Y axes
- `tools/blender/gen_*.py` — one script per model (anastasia chair, brazier, aether crystals, bob's inn, tuning pedestal, mud pool basin, lore artifact scroll, giant skeleton key, skeleton remains, pipe organ)
- `tools/blender/run_all_moon1.py` — master batch runner
- `Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs` — auto-converts FBX materials to URP/Lit on import, auto-generates prefab variants
- `Assets/_Project/Scripts/Editor/Moon1BlenderBatch.cs` — Editor menu `Tartaria/Moon 1/Run Blender Batch` launches Blender headlessly

### How to use

1. **Edit a script**: `tools/blender/gen_brazier.py` — adjust geometry parameters in Python.
2. **Run from Unity**: `Tartaria → Moon 1 → Run Blender Batch (Generate All Moon 1 Assets)` — uses Blender 5.0 on your Windows machine.
3. **Or from Blender directly**: open `Scripting` workspace, paste any `gen_*.py`, hit Run Script — appears in viewport for iteration.
4. **Drop into a scene**: the auto-created `.prefab` at `Assets/_Project/Prefabs/Moon1/Blender/` is ready to use.

### Production plan

See `docs/art/ART_PRODUCTION_PLAN.md` — comprehensive Tier 1/2/3 priority queue + Moon 2-13 landmark anchors. Estimated 30 hours of scripting to ship visually-complete Moon 1, 26 hours for landmark anchors across Moons 2-13.

### Rules for new scripts

1. **Always import _common**: `from _common import reset_scene, make_material, export_fbx`.
2. **Always call `reset_scene()` first** — clears default cube + previous mesh data.
3. **Materials via `make_material(name, base_color, roughness, metallic, emission, emission_strength)`**.
4. **Always end with**: `bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.join(); bpy.context.active_object.name = "AssetName"; export_fbx("AssetName")`.
5. **No external dependencies** — pure bpy primitives + Boolean/Bevel modifiers only.
6. **Cross-platform paths**: never hardcode `C:\dev\...` — let _common.py detect the project root.


---

## ⚡ 2026-05-31 CONTROLLER — Logitech F310 is the canonical dev gamepad

Per NATRIX (verbatim): *"my controller is a logitech gamepad F310 update .md files to reflect this then get it working check internal files it was working not long ago ensure all the buttons work for all required features of game"*

**Hardware spec — bake this in:**

- Primary controller: **Logitech F310** (wired USB).
- Physical X/D switch on the back. **X-mode = XInput (recommended)**, D-mode = legacy DirectInput.
- In X-mode the device reports as `Xbox Controller` via Unity Input System — that's normal.

**Where the wiring lives:**

| File | Role |
|---|---|
| `Assets/_Project/Scripts/Input/PlayerInputHandler.cs` | All button → game-action wiring. `HandleGamepadButtonFallbacks()` ALWAYS runs (even when InputAction asset is bound), so every F310 button has a real binding regardless of asset state. |
| `Assets/_Project/Scripts/Input/LogitechControllerSupport.cs` | F310 X/D-mode HID layout matchers. Called from `PlayerInputHandler.Awake()` via `EnsureF310Setup()`. |
| `Assets/_Project/Scripts/Input/InputProbeHUD.cs` | Top-left runtime overlay showing live device + stick state. Auto-bootstraps after scene load. |
| `Assets/_Project/Scripts/Camera/CameraController.cs` | Right-stick orbit + R3 recenter. |
| `docs/appendices/D_CONTROLS_F310.md` | Canonical F310 button map (consult before rebinding anything). |

**F310 button map (X-mode):**

| Button | Action |
|---|---|
| Left stick | Movement (camera-relative) |
| Right stick | Camera orbit |
| A (south) | Interact / Resonance Pulse (Combat) |
| B (east) | Scan / Cancel |
| X (west) | Resonance Pulse / Interact alt |
| Y (north) | Aether Vision toggle |
| LB | Sprint hold |
| RB | Harmonic Strike (Combat) |
| LT (analog) | Frequency Shield (Combat) — threshold > 0.5 |
| RT (analog) | Sprint hold (alt) |
| Start | Pause menu |
| Back/Select | Aether Vision (alt) |
| D-Pad ←/→ | Frequency adjust (Tuning + Combat) |
| D-Pad ↑ | Scan |
| D-Pad ↓ | Reserved (future crouch) |
| L3 click | Sprint toggle |
| R3 click | Recenter camera |

**The focus-loss fix is baked into `PlayerInputHandler.Awake()`** — `Application.runInBackground = true` + `InputSettings.BackgroundBehavior = IgnoreFocus` + `EditorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`. Don't remove these — the Windows weather widget (msedgewebview2) steals OS focus and without the fix, Unity stops polling input mid-play.

**⚡⚡ CRITICAL — When NATRIX says "no input is working", check FIRST (this was a multi-hour 2026-05-31 stall):**

1. **Console toolbar "Error Pause" toggle** — at the top of the Console window, left of the Editor filter dropdown. If ON (highlighted blue), Unity auto-pauses the Editor whenever any `Debug.LogError` fires. The Echohaven scene throws 1–2 errors on init (missing scripts on Moon1_Systems from removed Moon1NPCSpawner / Moon1AmbientCreatures, and historical abstract-Renderer add-component bugs). Error Pause = ON + any init error = EVERY Play session enters paused at frame 1. The bypass-driver overlay then renders cached state forever and looks like input is dead. **Toggle Error Pause OFF.**
2. **Editor → Play Mode → Pause** state — if the menu item shows checked or the Pause button next to Play is highlighted blue, the editor is paused. **Do NOT use Ctrl+Shift+P to toggle pause** — there's a shortcut conflict with `Tartaria/▶ ENTER PLAY MODE` that pops a "Shortcut Conflict" dialog that steals focus and confuses things further. Use the Edit menu instead.
3. **The `SimplePlayerDriver` v3 overlay shows `DriverFrame` AND `EngineFrame` on separate lines** — if both stay at 1 while `rt` (realtimeSinceStartup) keeps climbing, that's the unambiguous Editor Pause signature. Heartbeat log every 60 frames (`[SimplePlayerDriver] HEARTBEAT f=…`) tells you whether Update is firing at all.
4. **`Assets/_Project/Scripts/Editor/GameViewFocusFix.cs`** — auto-focuses + repaints the Game view on `EnteredPlayMode`. Manual menu fallback: `Tartaria → 9 Debug → Force Focus Game View`. Addresses the Unity 6 "Play Focused" toggle desync bug.
5. **`docs/audits/INPUT_DEEP_DIVE_2026-05-31.md`** has the full diagnostic decision table — read it before touching input code.

**Verification flow when "controller doesn't work" comes up again:**

1. Open Echohaven, hit Play.
2. Look top-left of Game view — the InputProbeHUD overlay reports `Keyboard.current: OK`, `Gamepad.current (XInput): OK (Xbox Controller)` (F310 in X-mode shows up this way), `Devices total: 3`, `Focus: True`.
3. Move the left stick — the `Left stick:` field on the overlay should show non-zero magnitude.
4. Press A — `Last key/btn: GP:A/South` should appear.
5. If the overlay says `Gamepad.current: NULL` → check X/D switch on back of controller, or re-run `LogitechControllerSupport.EnsureF310Setup()`.

If a future change "breaks the controller again", the most likely cause is one of:
- Edit tool truncated `PlayerInputHandler.cs` — restore from git, re-apply focus fix + `HandleGamepadButtonFallbacks` via the python heredoc pattern in commit history.
- A `using UnityEngine;` import added `UnityEngine.Input.GetKey(...)` paths that throw `InvalidOperationException` under Input System Package mode — sweep for `UnityEngine.Input.` and convert to `Keyboard.current` / `Gamepad.current` reads.
- `_playerMap != null` short-circuited the fallback path — make sure `HandleGamepadButtonFallbacks()` is the FIRST line of `HandleActionFallbacks()` (not gated by `_playerMap`).
