# MOON 1 ACCEPTANCE AUDIT — 2026-06-02 (v2)

> Sprint 8 Lane 10. Re-audit of Moon 1 readiness vs `docs/15_MVP_BUILD_SPEC.md` after Sprints 6 + 7 + 8 lanes shipped. v1 ship-gate verdict (~12 hours of work) is re-evaluated with concrete branch/SHA evidence.

**Auditor:** Sprint 8 Lane 10 (acceptance pass v2)
**Branch:** `agent/qa/moon1-acceptance-v2`
**Worktree:** `C:\dev\_wt_s8_l10_audit_v2`
**Method:** Cross-reference v1 audit (`docs/audits/MOON1_ACCEPTANCE_2026-06-02.md`) against pushed branches on origin. For every ⚠ / ❌ in v1, identify the Sprint 6/7/8 lane (if any) that addresses it, cite the SHA, and re-grade.

---

## §0. Caveats (carried from v1, still apply)

- `Echohaven_VerticalSlice.unity` is binary Unity 6 native scene — runtime placement of GameObjects is still un-verifiable via grep.
- **Critical for v2:** I score branches as "addresses" only when the work is **pushed to `origin/<branch>`**. Many Sprint 8 fix branches are scaffolded but never received commits or never pushed; those still count as ❌. The branch `agent/integration/sprint7-merge` is **pushed** (commit `8ef925d6`) but `feature/consolidate-moon-architecture` trunk is still at `6094136c` (the 10-lane hammer merge). **No Sprint 7 / Sprint 8 work has merged into trunk yet** — they sit on individual branches awaiting PR review.
- A ✓ in v2 means: a pushed branch contains a verifiable fix that addresses the v1 gap. It does NOT mean the fix has shipped to trunk.

---

## §1. Hero Buildings (Cathedral / Star Dome / Spire)

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 1.1 | ✓ | ✓ | Carried forward (prefab on disk) |
| 1.2 | ✓ | ✓ | Carried forward |
| 1.3 | ✓ | ✓ | Carried forward |
| 1.4 | ✓ | ✓ | Carried forward |
| 1.5 | ✓ | ✓ | Carried forward |
| 1.6 | ✓ | ✓ | Carried forward |
| 1.7 | ✓ | ✓ | Carried forward |
| 1.8 | ✓ | ✓ | Carried forward |
| 1.9 | ⚠ | ⚠ | Still pending — no lane verified displayName literals for "Thread of Memory" / "First Note" |

---

## §2. Village Buildings

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 2.1 | ✓ | ✓ | Carried forward |
| 2.2 | ✓ | ✓ | Carried forward |
| 2.3 | ✓ | ✓ | Carried forward |
| 2.4 | ✓ | ✓ | Carried forward |
| 2.5 | ❌ | ❌ | Still pending — no lane added InteractableBuilding hooks or named villagers (Baker/Smith/Mill-keeper) |

---

## §3. POIs

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 3.1 | ✓ | ✓ | Carried forward |
| 3.2 | ⚠ | ⚠ | Still pending — no lane added CarvedStone Placement to `Moon1BuildOutEnvironment.cs` |
| 3.3 | ✓ | ✓ | Carried forward |
| 3.4 | ✓ | ✓ | Carried forward |
| 3.5 | ⚠ | ⚠ | Still pending (rolls up 3.2) |

---

## §4. Vegetation

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 4.1 | ⚠ | ⚠ | Still pending — count remains 120 vs spec ~5k; no lane bumped GRASS_COUNT |
| 4.2 | ✓ | ✓ | Carried forward |
| 4.3 | ✓ | ✓ | Carried forward |
| 4.4 | ✓ | ✓ | Carried forward |

---

## §5. Mini-Game Variants A / B / C

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 5.1 | ✓ | ✓ | Carried forward |
| 5.2 | ✓ | ✓ | Carried forward |
| 5.3 | ✓ | ✓ | Carried forward |
| 5.4 | ✓ | ✓ | Carried forward |
| 5.5 | ⚠ | ⚠ | Still pending — `agent/fix/per-node-tuning-variant` exists **locally only**, never pushed to origin. `git branch -a` shows it as `+` (local) with no `remotes/origin/` counterpart. Per-node A/B/C rule remains `(TuningVariant)(_nodesCompleted % 3)` on origin trunk. |
| 5.6 | ❌ | ✓ | **Sprint 8 Lane 4** — `origin/agent/fix/pipe-organ-routing` SHA `85580768` "Sprint 8 Lane 4: route Pipe Organ Variant C to Dome per docs/15 §9 — audit blocker #2". Confirmed via `git show origin/agent/fix/pipe-organ-routing:Assets/_Project/Scripts/Integration/InteractableBuilding.cs` — `case "dome": EnsureMiniGameComponent<PipeOrganMiniGame>().StartOrgan();`. 27-line patch. **Not yet merged to trunk.** |
| 5.7 | ⚠ | ⚠ | `agent/gameplay/mini-game-variant-polish` SHA `5eb513cc` adds `MiniGameSmokeTest.cs` (127 lines) + Variant B duration tuned 20s→7.5s. Pushed to origin. Partial remediation — runtime playtest of UI render still required. |

---

## §6. NPCs

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 6.1 | ✓ | ✓ | Carried forward |
| 6.2 | ⚠ | ✓ | **Sprint 7 PR #4 (in trunk)** — commit `59629f03` "[narrative] anastasia reveal — yarn dialogue + OnMoonCompleted trigger w/ 8.7s delay + HUD banner fallback". Re-resolves the `crystalspire` vs `stardome` ambiguity by binding reveal to `OnMoonCompleted` instead of building ID. Already in trunk `feature/consolidate-moon-architecture`. |
| 6.3 | ⚠ | ⚠ | Still pending — no lane added `GameEvents.OnDayChanged` event. `LiraelLullaby.cs:25` still has `fundamentalHz = 432f` ✓ but Day-25 reveal gate unwired. |
| 6.4 | ✓ | ✓ | Carried forward |
| 6.5 | ⚠ | ⚠ | Sprint 7 yarn-tutorial-binding (`origin/agent/integration/yarn-tutorial-binding` SHA `3789e05e`) + `milo_tutorial.yarn` (54 lines) help, but exact 40-line distribution-per-spec still unverified. |
| 6.6 | ⚠ | ⚠ | Still pending — no lane added HIDE/CELEBRATE states. `MiloTutorialFlow.cs` (517 lines, on `origin/agent/ai/milo-tutorial-flow` SHA `5697b32a`) is a separate companion flow, not a state machine for FOLLOW/HIDE/CELEBRATE. |
| 6.7 | ❌ | ⚠ | **Sprint 8 Lane 8** — `origin/agent/content/npc-blender-models` SHA `82d57b9b`-ish, "Blender humanoid generators for Lirael/Anastasia/Cassian (upgrades from primitives)". Adds `Tools/blender/gen_npc_lirael.py`, `gen_npc_cassian.py`, `gen_npc_anastasia.py` + `NPC_PIPELINE_NOTES.md`. The generators exist but **the resulting FBX/prefab assets must be produced and re-bound to the in-scene NPC GameObjects** — none of that is automated yet. Partial remediation: ⚠ not ✓. |

---

## §7. Combat

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 7.1 | ✓ | ✓ | Carried forward |
| 7.2 | ⚠ | ⚠ | Still pending — no lane explicitly verified MudGolemAI speed/damage values against §11 |
| 7.3 | ⚠ | ✓ | **Sprint 7 (origin)** `origin/agent/ai/wave-spawner-tuning` SHA `6128330d` "[ai] wave spawner tuning --- 3-cap, hub-progress scaling, 10s cleanup". Adds `MudGolemSpawner.cs` (187 lines) + `WaveSystem.cs` (48 lines). Replaces RS-threshold ad-hoc with hub-progress scaling. |
| 7.4 | ✓ | ✓ | Carried forward |
| 7.5 | ❌ | n/a | Spec-mismatch (no Mud Lord in canonical docs) — strike from criteria. |
| 7.6 | ✓ | ✓ | Carried forward |

Additionally: **Sprint 7 Lane 7** (`origin/agent/combat/hit-feedback-call-sites` SHA `4dea7186`) wires `HitFeedback.NotifyHit` into 8 enemy AI strike sites (MudGolemAI, ResetScout, CrystalSentryAI, EnemyAIController, ResonanceDroneAI, ShadowStalkerAI, TemporalWraithAI, VoidPhantomAI) — combat feel-improvement that v1 didn't track. Pushed to origin.

---

## §8. Lore Beats

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 8.1 | ❌ | ❌ | Still pending — no lane added `BrazierRitual.cs` |
| 8.2 | ⚠ | ⚠ | Still pending — no lane wrapped Lirael reveal in Cinemachine cinematic |
| 8.3 | ✓ | ✓ | Carried forward |
| 8.4 | ✓ | ✓ | Carried forward |
| 8.5 | ⚠ | ✓ | **Sprint 8 Lane 4** — same fix as 5.6 (`origin/agent/fix/pipe-organ-routing` SHA `85580768`). Pipe Organ now canonical Dome puzzle. |
| 8.6 | ✓ | ✓ | Carried forward |
| 8.7 | ✓ | ✓ | Carried forward |
| 8.8 | ✓ | ✓ | Carried forward |

Bonus: **Sprint 7 Lane 8 & Sprint 6 Lane 9** (`origin/agent/level/post-restoration-asset-wiring` SHA `39b62b8e` + `origin/agent/level/post-restoration-world-state` SHA `be24a20b`) add `Moon1PostRestorationVisuals.cs` (365 lines) + `WirePostRestorationChildren.cs` (660 lines, Editor menu). Strengthens 8.4 / 8.6 / 8.8 ceremonies.

---

## §9. Audio

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 9.1 | ✓ | ✓ | Carried forward |
| 9.2 | ✓ | ✓ | Carried forward |
| 9.3 | ✓ | ✓ | Carried forward |
| 9.4 | ❌ | ❌ | Still pending — no lane named/built "CymaticEngine" class. v1 recommended accepting the gap by renaming; no doc update yet. |
| 9.5 | ❌ | ✓ | **Sprint 8 Lane 5** — `origin/agent/fix/ambient-zone-placement` SHA listed as Sprint 8 Lane 5 commit "Editor menu places 5 Moon 1 ambient zones - audit blocker #3". Adds `PlaceAmbientZones.cs` (183 lines) on top of Sprint 6 Lane 4's `AmbientZoneController.cs` (481 lines), `AmbientZoneProfile.cs` (86), `AmbientZoneTrigger.cs` (173), `AmbientZoneProfileBuilder.cs` (173). Together ~1,096 lines deliver the 5 ambient-zone solution. **Not yet in trunk.** |
| 9.6 | ✓ | ✓ | Carried forward |
| 9.7 | ✓ | ✓ | Carried forward |
| 9.8 | ✓ | ✓ | Carried forward |

Also: **Sprint 7 Lane 3** `origin/agent/audio/mixer-controller-rename` SHA `ed813138` canonicalizes mixer-exposed param names (MasterVol/MusicVol/SFXVol/UIVol/AmbienceVol/VoiceVol). Strengthens §9 plumbing for the SettingsMenu work in §13.4.

---

## §10. Save / Load

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 10.1 | ✓ | ✓ | Carried forward |
| 10.2 | ✓ | ✓ | Carried forward |
| 10.3 | ❌ | ✓ | **Sprint 8 Lane 6** — `origin/agent/fix/save-slots-menu` SHA `a7891ca6` "SaveSlotsMenu wired into MainMenu.Continue + Pause.Load - audit blocker #4". Adds `SaveSlotsMenu.cs` (275 lines), `SaveSlotPanel.cs` (613 lines), `SaveSlotEntry.cs` (253 lines), and patches `PauseAndGameOverMenu.cs` (+7 lines). Sprint 6 Lane 3 (`origin/agent/ui/save-slot-ui` SHA `fdcdbccd`) also pushed `SaveSlotPanel.cs` (613) + `SaveSlotEntry.cs` (253). **Two parallel implementations of save-slot UI now exist on origin — needs reconciliation before merging.** |
| 10.4 | ❌ | ✓ | **Sprint 7 Lane 4** — `origin/agent/save/thumbnail-pipeline` SHA `ec3747a5` "thumbnail pipeline strengthen (scene-change capture + editor menu + size knob)". Adds `SaveThumbnailMenu.cs` (139 lines, Editor) + reinforces `SaveSlotPanel.cs` (766 lines on this branch — different from Sprint 6's 613-line version). Thumbnail capture pipeline shipped. |
| 10.5 | ✓ | ✓ | Carried forward |
| 10.6 | ✓ | ✓ | Carried forward (already in trunk PR #8) |

---

## §11. Difficulty Modes

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 11.1 | ✓ | ✓ | Carried forward |
| 11.2 | ✓ | ✓ | Carried forward |
| 11.3 | ✓ | ✓ | Carried forward |
| 11.4 | ✓ | ✓ | Carried forward |
| 11.5 | ❌ | ✓ | **Sprint 6 Lane 2 (origin/agent/ui/settings-menu-real SHA `36738468`)** adds `SettingsMenu.cs` (443 lines) + `SettingsPersistence.cs` (158 lines) with difficulty selection. Reinforced by **Sprint 7 Lane 5** (`origin/agent/ui/pause-settings-extract` SHA `c1db9d9f`) adding `BuildSettingsPanelPrefab.cs` (537 lines) + `SettingsPanelController.cs` (540 lines) — reusable from both Main Menu and Pause. Combined ~1,678 lines for a full Settings UI with difficulty picker. |

Additionally: **Sprint 7 Lane 2** (`origin/agent/gameplay/difficulty-apply-sites` SHA `b8c4659f`) wires DifficultyProfile into 4 apply sites (MudGolemAI takes damage multiplier; TuningMiniGame takes window multiplier). +20 SO assets in `Resources/Difficulty/` + `Data/Difficulty/` for Story/Standard/Hardened.

---

## §12. Tutorial Flow

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 12.1 | ✓ | ✓ | Carried forward |
| 12.2 | ✓ | ✓ | Carried forward |
| 12.3 | ✓ | ✓ | Carried forward (strengthened by Sprint 7 Lane 6) |
| 12.4 | ✓ | ✓ | Carried forward |
| 12.5 | ⚠ | ⚠ | Sprint 7 Lane 6 (`origin/agent/integration/yarn-tutorial-binding` SHA `3789e05e`) adds `MiloTutorialFlow.cs` (517 lines) + `YarnTutorialBinding.cs` (141 lines) + `milo_tutorial.yarn` (54 lines). The 6-step canonical Moon 1 sequence is now plumbed through Yarn but the spec doc filter (`IsMoon1Step`) not added — still ⚠. |

---

## §13. UI

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 13.1 | ⚠ | ✓ | **Sprint 8 Lane 3** — `origin/agent/fix/main-menu-bootstrap` SHA `bd0bcbf0` "Main Menu bootstrap re-enabled per audit blocker #1". Confirmed via `git show origin/agent/fix/main-menu-bootstrap:Assets/_Project/Scripts/UI/MainMenuOverlay.cs` — `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` attribute is re-enabled (no longer commented). Plus `MainMenuController.cs` (274 lines) + `BuildMainMenuScene.cs` (288 lines) and the 4 AmbientZone files travel along. **Not yet merged to trunk.** |
| 13.2 | ⚠ | ⚠ | Still pending — no lane reworked PauseMenu.cs stub vs PauseAndGameOverMenu.cs canonical |
| 13.3 | ❌ | ✓ | **Sprint 6 Lane 10** — `origin/agent/narrative/credits-scene` SHA `6d7f7e6d` adds `CreditsScroll.cs` (239 lines) + `Moon1BuildCreditsScene.cs` (209 lines, Editor menu) + `docs/credits/credits_roll.md` (125 lines). Total 573 lines. Pushed to origin, not yet trunk. |
| 13.4 | ✓ | ✓ | Carried forward, strengthened by Sprint 6 Lane 2 + Sprint 7 Lane 5 |
| 13.5 | ✓ | ✓ | Carried forward |
| 13.6 | ✓ | ✓ | Carried forward (strengthened by Sprint 7 Lane 7 HitFeedback + DamagePopup 165 lines) |

---

## §14. Build Pipeline

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 14.1 | ❌ | ✓ | **Sprint 7 (origin)** `origin/agent/tools/itch-build-pipeline` SHA `505a9774` "[tools] itch.io build pipeline --- menu + headless + zip + sha256". Adds `ItchBuildPipeline.cs` (232 lines, Editor menu) + `scripts/build-itch.ps1` (38 lines). Strengthened by **Sprint 7 Lane 9** (`origin/agent/tools/itch-build-smoke` SHA `229b8fcd`) — itch build + screenshot smoke test pipeline. |
| 14.2 | ❌ | ⚠ | `scripts/build-itch.ps1` exists (38 lines) on `origin/agent/tools/itch-build-pipeline` but does not contain a `butler push` invocation per quick inspection — it's a Unity-side headless build wrapper. **Butler upload still pending — no lane.** |
| 14.3 | ⚠ | ✓ | Same fix as 14.1 — Editor menu now exists. |

---

## §15. F310 Controller

| # | v1 | v2 | Evidence / Lane |
|---|----|----|-----------------|
| 15.1 | ✓ | ✓ | Carried forward |
| 15.2 | ✓ | ✓ | Carried forward |
| 15.3 | ✓ | ✓ | Carried forward |
| 15.4 | ✓ | ✓ | Carried forward |
| 15.5 | ✓ | ✓ | Carried forward |
| 15.6 | ✓ | ✓ | Carried forward |
| 15.7 | ✓ | ✓ | Already in trunk (PR #10 commit `616b4228`) |

---

## SCORECARD (v2)

### Tally

| Section | ✓ | ⚠ | ❌ |
|---|---|---|---|
| §1 Hero Buildings | 8 | 1 | 0 |
| §2 Village Buildings | 4 | 0 | 1 |
| §3 POIs | 3 | 2 | 0 |
| §4 Vegetation | 3 | 1 | 0 |
| §5 Mini-Game Variants | 6 | 2 | 0 |
| §6 NPCs | 3 | 4 | 0 |
| §7 Combat | 4 | 1 | 0 (7.5 struck as spec-mismatch) |
| §8 Lore Beats | 6 | 1 | 1 |
| §9 Audio | 6 | 0 | 1 |
| §10 Save/Load | 5 | 0 | 0 |
| §11 Difficulty | 5 | 0 | 0 |
| §12 Tutorial | 4 | 1 | 0 |
| §13 UI | 4 | 1 | 0 |
| §14 Build Pipeline | 2 | 1 | 0 |
| §15 F310 Controller | 7 | 0 | 0 |
| **TOTAL** | **70** | **15** | **3** |

(v1 was 59 ✓ / 18 ⚠ / 13 ❌. v2: +11 ✓, −3 ⚠, −10 ❌.)

### Delta vs v1

| Item | v1 | v2 | Lane / SHA |
|---|----|----|-----------|
| 5.6 Pipe Organ → Dome routing | ❌ | ✓ | Sprint 8 Lane 4 — `origin/agent/fix/pipe-organ-routing` `85580768` |
| 6.2 Anastasia reveal gate | ⚠ | ✓ | Sprint 7 PR #4 (in trunk) `59629f03` |
| 6.7 Lirael/Anastasia/Cassian rigged models | ❌ | ⚠ | Sprint 8 Lane 8 — `origin/agent/content/npc-blender-models` |
| 7.3 RS-threshold golem waves | ⚠ | ✓ | `origin/agent/ai/wave-spawner-tuning` `6128330d` |
| 7.5 Mud Lord boss | ❌ | n/a | Struck (spec-mismatch per v1 caveat) |
| 8.5 Pipe Organ canonical | ⚠ | ✓ | Sprint 8 Lane 4 (same as 5.6) |
| 9.5 5 AmbientAudioZones | ❌ | ✓ | Sprint 8 Lane 5 — `origin/agent/fix/ambient-zone-placement` |
| 10.3 Save Slot UI | ❌ | ✓ | Sprint 8 Lane 6 — `origin/agent/fix/save-slots-menu` `a7891ca6` |
| 10.4 Save thumbnails | ❌ | ✓ | Sprint 7 Lane 4 — `origin/agent/save/thumbnail-pipeline` `ec3747a5` |
| 11.5 Difficulty UI | ❌ | ✓ | Sprint 6 Lane 2 — `origin/agent/ui/settings-menu-real` `36738468` + Sprint 7 Lane 5 `c1db9d9f` |
| 13.1 Main Menu Bootstrap | ⚠ | ✓ | Sprint 8 Lane 3 — `origin/agent/fix/main-menu-bootstrap` `bd0bcbf0` |
| 13.3 Credits | ❌ | ✓ | Sprint 6 Lane 10 — `origin/agent/narrative/credits-scene` `6d7f7e6d` |
| 14.1 Editor build script | ❌ | ✓ | `origin/agent/tools/itch-build-pipeline` `505a9774` |
| 14.2 itch / butler upload | ❌ | ⚠ | Partial — Unity headless build script exists; butler push still absent |
| 14.3 StandaloneWindows64 build | ⚠ | ✓ | Same as 14.1 |

**Items that did NOT move (still pending — no lane on origin):**

- 1.9 displayName literals for "Thread of Memory" / "First Note"
- 2.5 Village ambient hooks (named villagers)
- 3.2 / 3.5 Carved Stone POI Placement
- 4.1 Vegetation count (still 120 vs spec ~5k)
- 5.5 Per-node A/B/C variant rule (`agent/fix/per-node-tuning-variant` is **local-only**, never pushed)
- 5.7 Variants B/C runtime UI playtest
- 6.3 Lirael Day-25 gate (no `OnDayChanged` event added)
- 6.5 40-voice-line distribution per spec
- 6.6 Milo HIDE/CELEBRATE states
- 7.2 MudGolemAI speed/damage spec verification
- 8.1 Brazier ritual mechanic
- 8.2 Lirael lullaby Cinemachine cinematic
- 9.4 CymaticEngine naming/decision
- 12.5 6-step canonical Moon 1 filter
- 13.2 PauseMenu vs PauseAndGameOverMenu reconciliation

### Ship-gate verdict (v2)

**STILL NEEDS WORK — but the runway is shorter. ~4–6 focused hours to clear the remaining substantive blockers if the existing pushed branches all merge.**

Reasoning:
- 70/88 items at ✓ (vs 59/90 in v1) — 80% pass rate.
- **3 ❌ remaining are small-or-spec:** §2.5 named villagers (1.5 h), §8.1 BrazierRitual.cs (1 h), §9.4 CymaticEngine rename or shim (5 min). Total ~2.5 hours.
- **15 ⚠ remaining** — most under 1 hour each. The painful ones are 6.3 (OnDayChanged event chain, ~1 h), 6.6 (Milo state machine, ~1.5 h), 5.5 (per-node variant rule, ~30 min).
- **The bigger risk is integration, not authoring.** Sprint 8 Lane 9 (`origin/agent/integration/sprint7-merge`) has pulled Sprint 6 + 7 work together but **none of it has merged to trunk** yet. There are also two competing SaveSlotPanel implementations (Sprint 6 Lane 3's 613-line version vs Sprint 7 Lane 4's 766-line version) that need reconciliation before either ships. PRs against `feature/consolidate-moon-architecture` must land before any of these v2 ✓ scores are real-on-trunk.

### Top 5 remaining blockers

1. **Merge Sprint 6 + 7 + 8 lanes into trunk.** `feature/consolidate-moon-architecture` HEAD is still `6094136c`. Without trunk merge, every Sprint 8 ✓ in this audit is "branch-only." Reconcile SaveSlotPanel duplication first (Sprint 6 Lane 3 vs Sprint 7 Lane 4). Est. 2 h reconciliation + 1 h per-lane merge review.

2. **§5.5 Per-node A/B/C variant dispatch rule.** `agent/fix/per-node-tuning-variant` exists **local only**, never pushed. Spec requires Node 1→A / Node 2→B|C / Node 3→C|A; trunk still runs round-robin. Push the local branch (30 min).

3. **§6.3 Lirael Day-25 gate.** No `GameEvents.OnDayChanged` event added; lullaby cannot fire on Day 25 trigger. ~1 h: add `OnDayChanged` to `GameEvents.cs`, raise it from `TartarianHourCycle.cs`, hook `LiraelController.OnDayChanged += ShowOnDay25`.

4. **§14.2 Butler / itch upload script.** Unity-side build is solved (Sprint 7 Lane `itch-build-pipeline`), but `scripts/build-itch.ps1` does not call `butler push`. Without it there's still no automated path to itch.io. ~30 min: append `butler push $buildDir nathan/tartaria:windows-alpha` after the Unity invocation.

5. **§6.7 NPC humanoid rebinding.** Blender generators are scaffolded on `origin/agent/content/npc-blender-models` but the FBX outputs haven't been generated, the Editor postprocessor hasn't run, the prefabs haven't been re-pointed to skinned meshes, and no `Animator` controller has been bound. Lirael/Anastasia/Cassian still render as primitives in scene. ~2 h: run the generators, postprocess, rebind.

---

## NEW IN v2 (not tracked in v1)

These pushed lanes deliver scope beyond the v1 ✓/⚠/❌ list:

- **Sprint 6 Lane 5** (`origin/agent/anim/combat-hit-feedback` `a0fe4d6d`) — Hit feedback + damage popup + screen shake (369 lines `HitFeedback.cs` + 165 lines `DamagePopup.cs`).
- **Sprint 6 Lane 6** (`origin/agent/ai/milo-tutorial-flow` `5697b32a`) — Milo tutorial flow + dialogue (517 lines `MiloTutorialFlow.cs`).
- **Sprint 6 Lane 8** (`origin/agent/tools/itch-page-assets` `024fc16b`) — itch screenshot pipeline + marketing draft.
- **Sprint 7 Lane 8** + **Sprint 6 Lane 9** — Post-restoration cinematic visuals (365 lines + 660 lines Editor menu).
- **Sprint 7 Lane 7** (`origin/agent/combat/hit-feedback-call-sites` `4dea7186`) — wired HitFeedback into 8 enemy AI strike sites.
- **Sprint 8 Lane 1** (`origin/agent/fix/sprint8-compile-clean`) — compile-clean: fix CS0234 Tartaria.Input shadow + 6 warnings (6 files, small surgical patches).
- **Sprint 8 Lane 2** (`origin/agent/fix/tagmanager-dedup`) — docs-only investigation (189-line audit note).
- **Sprint 7 + 8 docs** — `agent/docs/api-contract-v2` `2746fec8` adds `WORKTREE_MANDATE.md` + `SPRINT_7_DISPATCH.md` + `API_CONTRACT v2`.

---

## CAVEATS

- All ✓ scores in this audit are based on **branches pushed to origin**, not trunk. A second merge audit is required after `feature/consolidate-moon-architecture` absorbs the lanes.
- Two duplicate SaveSlotPanel implementations on origin (`agent/ui/save-slot-ui` 613 lines vs `agent/save/thumbnail-pipeline` 766 lines vs `agent/fix/save-slots-menu` 613 lines) need triage before merge — pick the canonical one.
- `agent/integration/sprint7-merge` SHA `8ef925d6` represents Sprint 8 Lane 9's roll-up of Sprint 6 + 7 — that branch's diff vs trunk would be the cleanest single-PR landing path. v2 has not validated it for conflict-clean state.
- The 15 ⚠ items are mostly small (< 1 h each), but several rely on architectural decisions (which save-slot UI to keep, whether OnDayChanged belongs on `GameEvents` or `TartarianHourCycle`) that should not be decided unilaterally by an agent.
- `Echohaven_VerticalSlice.unity` runtime-content verification still requires Cowork to enter Play with the bootstrap menus pre-clicked.

---

*Sprint 8 Lane 10 — Moon 1 Acceptance Audit v2*
*Auditor: Claude (Opus 4.7 1M)*
*Date: 2026-06-02*
*Method: grep + branch-cite, brutal-honesty mode, delta-tracked vs v1*
