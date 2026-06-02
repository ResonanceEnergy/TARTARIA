# Sprint 11 Lane 4 — Workaround / Override / Insurance Audit

**Branch:** `agent/audit/workarounds` (worktree `C:\dev\_wt_s11_l4_workarounds`)
**Base SHA:** `e07660306026c2da2a1c222f26189c99a8fc4a3c`
**Date:** 2026-06-02
**Scope:** Every script under `Assets/_Project/Scripts/` whose name or contents indicate it papers over a real bug instead of fixing it. Per CLAUDE.md NO-DEBT / NO-STUBS mandate (rules #1, #4, #6) any file that bypasses the canonical code path is suspect by default.

## Method

1. `Get-ChildItem -Recurse Assets/_Project/Scripts/ -Filter '*.cs' | Where-Object Name -match 'Override|Insurance|Emergency|Rescue|GodMode|HardOverride|FallBack|Driver|Hack|Bypass|Bootstrap|Force|Patch|Repair|Fixer|Failsafe|Safety|Auto|Spawn|SimplePlayer|GameView|Focus|InputProbe|HotFix|Recover|Heal|HardReset'` — 31 hits.
2. `git grep` of every `// HOTFIX | // TEMP | // SAFETY | // HACK | // EMERGENCY | // WORKAROUND | // BAND-AID` comment — 3 active hits.
3. `git grep` of the runtime-rescue smell `GetComponent<X>() == null → AddComponent<X>` — 23 hits.
4. `git grep` of `[RuntimeInitializeOnLoadMethod]` — 38+ hits (catalogued; most are legitimate service bootstraps, only one is rescue insurance).
5. `git grep` of `UnityEngine.Input.GetKey*` (banned per CLAUDE.md F310 section, line 372) — 11 hits.
6. For each candidate, read top 30–60 lines + ran `git grep -c <ClassName>` to count callsites.

## Findings — verdict table

Legend:
- **DELETE-FIX-ROOT** — workaround papering over an asset/scene/prefab/event-wiring bug; remove file + fix the root cause.
- **KEEP-DEFENSIVE** — defensible engineering (vendor-bug shim, real service, idempotent dev tool, deliberately superseded marker).
- **STUB-REMOVE** — already-superseded marker that exists only so `csproj` resolves; safe to delete once asmdef + project file regenerate.

| # | File | Type | What it papers over | Verdict | Pre-req to remove |
|---|------|------|---------------------|---------|-------------------|
| 1 | `Assets/_Project/Scripts/Integration/RuntimeSpawnerInsurance.cs:10-53` | Runtime rescue | `Echohaven_VerticalSlice.unity` is missing `BuildingSpawner` / `PlayerSpawner` GameObjects from the scene file — this `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` injects them at runtime instead. `EchohavenContentSpawner` block already commented-out with `// SUPERSEDED 2026-05-31` (lines 35-41) leaving an orphan dangling `{}` block (lines 36-41) — file is half-dead. | **DELETE-FIX-ROOT** | Open scene in Unity Editor → drop `BuildingSpawner` + `PlayerSpawner` GameObjects under `--- GAME MANAGERS ---` parent → save scene → delete this file + `.meta`. Cross-ref to `EchohavenSceneAudit` Editor tool (CLAUDE.md line 270) which already detects the missing GameObjects. |
| 2 | `Assets/_Project/Scripts/Editor/Moon1SceneRescue.cs:18-244` + nested `Moon1RescueDriver` MonoBehaviour at `:249-272` | Bypass-driver | Menu `Tartaria/6 Scene Tools/Scene Rescue` (a) dedupes scene duplicates the Editor should never have created, (b) `ForceSpawnPlayer` at `(0,2,-10)`, (c) **attaches `Moon1RescueDriver` MonoBehaviour** to the player at `:184-187` — this driver uses banned `UnityEngine.Input.GetKey` (`:262-265`) to drive `CharacterController.Move` directly, **bypassing `PlayerInputHandler`**. Identical lineage to archived `SimplePlayerDriver.cs.archived` (Input/_archived_bypass_drivers_2026_05_31). | **DELETE-FIX-ROOT** | Convert the dedupe-helper portion (lines 56-100) to a separate `Moon1SceneDedupe.cs` if useful, then delete the rest. Banned per CLAUDE.md F310 section ("Edit tool truncated PlayerInputHandler → restore from git" — bypass driver IS the trap). 1 internal callsite only. |
| 3 | `Assets/_Project/Scripts/Integration/Moon1HardOverrideDriver.cs:1-3` | Superseded marker | 3-line stub left so csproj reference resolves. Per CLAUDE.md "NEVER ship a file with `// stub`" (rule #1). | **STUB-REMOVE** | Drop the file + its 1 stale reference at `Assets/_Project/Scripts/Editor/DiagnosePlayerSetup.cs` (4 matches) and `Moon1GodMode.cs` (1 match), then domain reload. |
| 4 | `Assets/_Project/Scripts/Integration/Moon1GodMode.cs:1-5` | Superseded marker | 5-line stub — same lineage as #3 ("Moon1GodMode → Moon1HardOverrideDriver → SimplePlayerDriver → Moon1Lifeline retired"). | **STUB-REMOVE** | Drop with #3. Referenced by `DiagnosePlayerSetup.cs` (2) + `DiagnosePlayerSetupFull.cs` (1) — purge those refs first. |
| 5 | `Assets/_Project/Scripts/Input/PlayerInputHandler.cs:215-217` (comment block `// EMERGENCY BYPASS: Always allow movement for debugging`) | In-file bypass | `Update()` no longer gates `HandleMovementInput()` behind `GameStateManager.Instance?.IsPlaying`. So during cutscenes, pause, dialog, tuning mini-game, dead state — the player can still walk. Original guard preserved as a comment on `:216`. | **DELETE-FIX-ROOT** | Restore the original `if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;` guard, then add a SINGLE explicit `GameState.Playing | GameState.Combat | GameState.Tuning` allow-list. Root cause: `GameStateManager` initial state was `MainMenu` instead of `Playing` when the bypass was added. Verify `GameStateManager.SetState(GameState.Playing)` is called from `EchohavenContentSpawner` Awake. |
| 6 | `Assets/_Project/Scripts/Integration/PlayerSpawner.cs:118-138` (block prefixed `// SAFETY: ensure movement components exist even if the prefab is incomplete`) | Runtime AddComponent rescue | After `Instantiate(playerPrefab)` it null-checks + `AddComponent<CharacterController>`, `AddComponent<PlayerInputHandler>`, `AddComponent<GiantMode>`. Per the comment: *"2026-05-30: was finding Player capsules with no PlayerInputHandler → couldn't walk."* This is the prefab-is-wrong smell. | **DELETE-FIX-ROOT** | Fix the `Assets/_Project/Prefabs/Characters/Player.prefab` (and any Blender-built `PlayerHero` variant). Open in prefab mode, add the 3 components, apply, then delete lines 118-138. The "WireSpawnerPrefabs" Editor tool already exists for this. |
| 7 | `Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:742` (comment `// WORKAROUND: AddComponent<TextMeshProUGUI> fails on some GameObjects - use child GameObject pattern`) | Vendor workaround | TextMeshPro will not allow `TextMeshProUGUI` on the same GameObject that owns an `Image` raycaster (font-asset population race). The "create child GameObject" pattern repeats 3× in the file. | **KEEP-DEFENSIVE** | Real TMP quirk. The workaround is the canonical TMP pattern Unity itself recommends. Annotate as `// TMP-CANONICAL` rather than `// WORKAROUND` to remove the smell. |
| 8 | `Assets/_Project/Scripts/AI/ResetScout.cs:34-39` (`if (_cc == null) _cc = gameObject.AddComponent<CharacterController>()` plus `EnsureVisual()` that does `GameObject.CreatePrimitive(PrimitiveType.Capsule)` at `:44+`) | Prefab-is-wrong rescue | Spawning the AI from a prefab that has no CharacterController and no visuals — both get built at runtime, with primitives. Violates CLAUDE.md rule #4 ("NEVER use `GameObject.CreatePrimitive` without an immediate URP-safe fallback path") and rule #6 ("Building from primitives is failure"). | **DELETE-FIX-ROOT** | Build `Assets/_Project/Prefabs/Enemies/ResetScout.prefab` properly with CC + Blender FBX visuals, then delete `EnsureVisual()` + the AddComponent. |
| 9 | `Assets/_Project/Scripts/Editor/GameViewFocusFix.cs:1-69` | Vendor-bug shim | Unity 6 known bug — Game view's "Play Focused" toggle desyncs and the Game view never receives input. Includes Unity issuetracker URLs. `[InitializeOnLoad]` + `playModeStateChanged` reflection-Focus call. | **KEEP-DEFENSIVE** | Real Unity bug, cited in 2 official issuetracker URLs (lines 14-15). Pinned by CLAUDE.md F310 section (line 366). Leave until Unity 6.4 ships with the fix. |
| 10 | `Assets/_Project/Scripts/Input/InputProbeHUD.cs:1-104` | Diagnostic overlay | Top-left runtime overlay showing live device + stick state. `[RuntimeInitializeOnLoadMethod]` auto-bootstraps it. Not a bypass — it only reads input. | **KEEP-DEFENSIVE** | Explicit dev tool pinned by CLAUDE.md F310 verification flow (line 374-379). Leave as-is — annotate `// DEV-ONLY` and `#if !UNITY_DEV` guard for ship build. |
| 11 | `Assets/_Project/Scripts/Editor/AutoCreateMissingAssets.cs:1-84` | Missing-asset rescue | `[InitializeOnLoad]` + `delayCall` that auto-creates missing critical ScriptableObject assets so runtime `Resources.Load` calls don't NRE. | **DELETE-FIX-ROOT** | The missing assets should EXIST in `Assets/_Project/Resources/` and be checked into git. Run the menu manually once, commit the assets, delete this file. 2 callsites in repo. |
| 12 | `Assets/_Project/Scripts/Editor/Moon1AutoWire.cs:1-98` | Superseded Editor menu | Explicitly marked **SUPERSEDED** in the doc comment (lines 12-14) — replaced by `Moon1BuildOutBuildings.cs`. Menu attribute commented out at `:24`. | **DELETE-FIX-ROOT** | 2 callsites — both inside this file (self-refs). Safe delete. |
| 13 | `Assets/_Project/Scripts/Editor/Moon1FixSpawn.cs:14-247` | Repeated scene-fix menu | Menu `Tartaria/8 Fix/PlayerSpawner Position` + `Camera Inversion` + `Fall-Through Safety`. Latest update 2026-06-01 (`:25-27`) hard-codes spawn at `(0, 2, 15)` because the **scene's `PlayerSpawner.defaultSpawnPosition` field is wrong**. Creates a `_SpawnPlatform` primitive cube (`:36-39`). | **DELETE-FIX-ROOT** | Open scene, set `PlayerSpawner.defaultSpawnPosition = (0,2,15)` on the actual GameObject + bake a real spawn platform mesh, save scene. Delete file. 3 internal callsites only. |
| 14 | `Assets/_Project/Scripts/Editor/Moon1WireSpawner.cs:1-79` | Repeated wire menu | One-shot creator of `EchohavenContentSpawner` with reflection-assigned MudGolem prefab. Idempotent but indicates spawner is **still not in the saved scene**. | **DELETE-FIX-ROOT** | Same scene fix as #1 — once spawner exists in the saved scene with prefabs assigned in Inspector, this menu is dead weight. |
| 15 | `Assets/_Project/Scripts/Editor/Moon1WireSpawnerPrefabs.cs:1-440` | Idempotent prefab wiring | Wires every prefab field on every scene-attached spawner via reflection (private SerializeField). Idempotent. Workaround for "serialized field renames break serialization" (per doc comment `:14-17`). | **KEEP-DEFENSIVE** | Real refactor-protection tool, well documented. Doesn't bypass any code path — just sets fields. Keep, but move to `_Project/Scripts/Editor/_dev_tools/`. |
| 16 | `Assets/_Project/Scripts/Editor/AutomatedPrefabWiring.cs:1-470` | Dev tool | EditorWindow for batched scene wiring across all 13 Moons. Real tool, not a rescue. | **KEEP-DEFENSIVE** | Per CLAUDE.md line 269 it's a canonical Tartaria menu (`Tartaria → Automated Prefab Wiring`). |
| 17 | `Assets/_Project/Scripts/Editor/Moon1MasterBootstrap.cs:1-101` | Idempotent Editor wire | Top-level menu that drops Moon1_Systems GameObject + child components. Already cleaned-up 2026-05-31 (removed 6 conflicting components per doc comment `:13-21`). | **KEEP-DEFENSIVE** | Canonical entry-point per `Tartaria/0 ★ MASTER` menu group. Idempotent + documented. |
| 18 | `Assets/_Project/Scripts/Integration/NPCConditionalSpawn.cs:1-52` | Real gameplay system | Hides NPC until `GameEvents.OnBuildingRestored` fires. Proper event subscription, not a workaround. | **KEEP-DEFENSIVE** | Canonical event-driven NPC reveal. 6 callsites. Not a workaround. |
| 19 | `Assets/_Project/Scripts/Tests/Bootstrap_AICombat.cs` + `Bootstrap_PlayerOnly.cs` + `Bootstrap_Restoration.cs` | Test-scene factories | Editor menu spawns isolated test scene for QA. Per `HANDOFFS 2026-06-01 22:30 → QA Lead`. | **KEEP-DEFENSIVE** | Per QA contract. Keep. |
| 20 | `Assets/_Project/Scripts/Core/GameBootstrap.cs` | Canonical service bootstrap | Real ECS world init. Not a workaround. | **KEEP-DEFENSIVE** | Canonical. |
| 21 | `Assets/_Project/Scripts/AI/EnemySpawnerManager.cs` | Wave system | Real gameplay. Not a workaround. | **KEEP-DEFENSIVE** | Canonical. |
| 22 | `Assets/_Project/Scripts/Gameplay/ResourceNodeSpawner.cs` | Real gameplay | Not a workaround. | **KEEP-DEFENSIVE** | Canonical. |
| 23 | `Assets/_Project/Scripts/Integration/BuildingSpawner.cs` | Real gameplay | Discovers/wires placeholders or creates greybox fallback. Not a workaround per the canonical comment at `:7-13`. | **KEEP-DEFENSIVE** | Canonical. |
| 24 | `Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs` | Real gameplay | 3082-line canonical content spawner. Contains scattered `if (X == null) AddComponent<X>` at `:378`, `:869`, `:2653` — keep watching but these are MonoBehaviour-wiring on dynamically-built GameObjects (legit). | **KEEP-DEFENSIVE** | Canonical. |
| 25 | `Assets/_Project/Scripts/Integration/PlayerSpawner.cs` (excluding SAFETY block at `:118-138`) | Real gameplay | Canonical player spawn. | **KEEP-DEFENSIVE** (after row #6 fix) | See row 6. |
| 26 | `Assets/_Project/Scripts/Editor/DiagnosePlayerSetup.cs` + `DiagnosePlayerSetupFull.cs` | Diagnostic Editor | Read-only audit. Currently has stale refs to `Moon1GodMode` + `Moon1HardOverrideDriver` superseded markers. | **KEEP-DEFENSIVE** (purge stale refs) | Strip the dead refs as part of row #3+#4 cleanup. |

## Smoking-gun deep-dive: RuntimeSpawnerInsurance

Per the task brief, calling out specifically:

**File:** `Assets/_Project/Scripts/Integration/RuntimeSpawnerInsurance.cs:10-53`

This file is the archetypal symptom of the disease this session keeps tripping on:

1. **The scene file is wrong.** `Echohaven_VerticalSlice.unity` does NOT have `BuildingSpawner` / `PlayerSpawner` GameObjects under the `--- GAME MANAGERS ---` parent. Every Editor session that tries to Play discovers this at runtime.
2. **The "fix" is to inject the missing GameObjects at `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`.** This works in Play mode but means:
    - The scene's wired prefab references (Inspector-assigned `playerPrefab`, `kayKitMudGolem`, etc.) are **all null** in the runtime-injected version, because nobody filled the Inspector slots — the GameObject did not exist at edit time.
    - Each spawner falls back to its hard-coded defaults / `Resources.Load` paths, which produces the "spawned 6 Milo duplicates" / "no Mud Golem" bugs seen in `MOON1_LIVE_PLAYTEST_GAPS_2026-05-31.md`.
    - On the next session somebody re-runs `Moon1WireSpawner` Editor menu to fix it again — endless loop.
3. **The `EchohavenContentSpawner` block is already a half-commented zombie** at lines 35-41:

    ```csharp
    // SUPERSEDED 2026-05-31 — archived: if (Object.FindFirstObjectByType<EchohavenContentSpawner>() == null)
    {
        // SUPERSEDED 2026-05-31 — archived: var go = new GameObject("EchohavenContentSpawner");
        // SUPERSEDED — orphan go ref: go.transform.SetParent(managers.transform);
        ...
    }
    ```

    — that `{}` block now executes unconditionally as a no-op, and per CLAUDE.md rule #3 should never have been left in this state.

**Root cause:** the saved `Echohaven_VerticalSlice.unity` does not match the Editor expectation. The fix is to either (a) commit the scene with the spawners present + Inspector references filled in, or (b) move all spawner construction into a single `Moon1SceneInitializer` MonoBehaviour that lives in the scene and owns the prefab references — the *current* approach (3 separate auto-attach insurance / repair scripts) is the failure mode.

## Top-5 DELETE-FIX-ROOT recommendations (priority order)

1. **`Integration/RuntimeSpawnerInsurance.cs:10-53`** — fix scene; delete the insurance script. Removes the root cause of "spawners missing on Play."
2. **`Editor/Moon1SceneRescue.cs:184-187` + `:249-272`** — delete the entire `Moon1RescueDriver` MonoBehaviour and the menu line that attaches it. This is the banned bypass-driver pattern explicitly flagged in CLAUDE.md F310 section (line 372). Prefab-is-wrong + bypass + banned legacy `UnityEngine.Input.GetKey` triple-violation.
3. **`Input/PlayerInputHandler.cs:215-217`** — remove the `// EMERGENCY BYPASS` comment block, restore the original `if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying) return;` guard. Investigate why `GameStateManager.CurrentState` wasn't `Playing` when the bypass landed (almost certainly because `GameBootstrap.Awake` runs before `EchohavenContentSpawner.Awake` calls `SetState(Playing)`).
4. **`Integration/PlayerSpawner.cs:118-138` (SAFETY block)** — fix the Player prefab to ship `CharacterController + PlayerInputHandler + GiantMode` so the SAFETY AddComponent rescue can be deleted. Run `Moon1WireSpawnerPrefabs` once, commit, then strip the block.
5. **`Editor/AutoCreateMissingAssets.cs:1-84`** — run it once, commit the missing `Resources/` ScriptableObjects, then delete the auto-create-on-domain-reload `[InitializeOnLoad]` shim. Workaround for missing source-controlled assets.

## Bonus risk callouts (not strictly the audit scope, surfaced while sweeping)

- `Editor/Moon1SceneRescue.cs:262-265` + `Gameplay/Moon2FirstPurgeTrigger.cs:105-106` + `Integration/TuningPedestalLink.cs:48` + `Save/SaveManager.cs:211-212` + `UI/PauseAndGameOverMenu.cs:73` — all read `UnityEngine.Input.GetKey*` while project is in Input System Package mode. Per CLAUDE.md line 372 these throw `InvalidOperationException` at runtime. Recommend a follow-up sprint lane to convert each to `Keyboard.current.<key>.wasPressedThisFrame`.
- `AI/MiloTutorialFlow.cs:187` explicitly documents the same rule in a comment — the codebase already knows.

## Workaround file count

- **Total files in initial keyword search:** 31
- **Of those, true workarounds/bypasses:** 7 → 4 DELETE-FIX-ROOT, 2 STUB-REMOVE markers, 1 in-place EMERGENCY-BYPASS comment block to revert.
- **Defensive / canonical / dev-tool keepers:** 24 (all rows marked KEEP-DEFENSIVE above).
- **Plus** 1 in-file `// SAFETY` block (PlayerSpawner) + 1 in-file `// EMERGENCY BYPASS` comment (PlayerInputHandler) for a grand total of **9 active root-cause backlog items**.

## Recommended sequencing

1. Open `Echohaven_VerticalSlice.unity` in Editor → add saved `BuildingSpawner` + `PlayerSpawner` GameObjects with Inspector slots filled → save scene. **Unblocks rows #1, #13, #14.**
2. Rebuild `Player.prefab` with `CharacterController + PlayerInputHandler + GiantMode + Animator`. **Unblocks row #6.**
3. Revert `PlayerInputHandler.cs:215-217` to the gated `Update()` + commit `GameStateManager.SetState(Playing)` in `EchohavenContentSpawner.Awake`. **Unblocks row #5.**
4. Delete `Moon1SceneRescue.cs:249-272` (Moon1RescueDriver) + remove `:184-187` attach lines. **Unblocks row #2.**
5. Run `AutoCreateMissingAssets` once, commit `Resources/` SOs, delete file. **Unblocks row #11.**
6. Delete superseded markers (`Moon1GodMode.cs`, `Moon1HardOverrideDriver.cs`, `Moon1AutoWire.cs`) + scrub `DiagnosePlayerSetup*.cs` refs. **Unblocks rows #3, #4, #12.**

After all six steps, the project no longer has any `RuntimeInitializeOnLoadMethod` "insurance" injector, any bypass driver, any "emergency" comment block, or any superseded marker — and the scene/prefab files are the single source of truth.

---

*Sprint 11 Lane 4 — audit only. No code mutated. Branch `agent/audit/workarounds`. Base `e07660306026c2da2a1c222f26189c99a8fc4a3c`.*
