# MOON 1 ACCEPTANCE AUDIT — SPRINT 13 (Post-MicroSprint MS.L1-L5)

**Date:** 2026-06-02
**HEAD audited:** `c2756942` (feature/consolidate-moon-architecture)
**Branch:** `agent/sprint13/audit`
**Auditor:** Sprint 13 acceptance lane
**Predecessor:** `MOON1_ACCEPTANCE_SPRINT12_2026-06-02.md`

---

## TOP-LINE VERDICT

> **Moon 1 SHIPPABLE (pending dirty-worktree caveat).**
>
> All five MicroSprint ship-blocker fixes (MS.L1 HUD live data, MS.L2 NavMesh bake, MS.L3 DayNight boost, MS.L4 Weapon switcher, MS.L5 Aether player pos) are landed at HEAD `c2756942` and grep-verifiable. The three Sprint-12-blocking "commented-out wiring" lanes are now live code. Silent fails dropped from 11 to 1 (intentional UnityException at PointOfInterest.cs:94). GameEvents pairs are complete (3/3 subscribers wired). Dialogue speaker map fixed to snake_case + short speaker keys. Moon1_Systems orphan blocks remain at 0. Cathedral invalid material GUID retired.
>
> **Caveat:** The local worktree at `C:/dev/_wt_sprint13` showed dirty state on 5 of the same MS-touched files (notably DayNightCycleController.cs truncated mid-method in the working tree). This audit measures the **committed** state at SHA `c2756942`, which is intact. If anyone commits the dirty truncation, MS.L3 regresses to broken.

---

## 9-DIMENSION DELTA TABLE

| # | Lane | Sprint 12 baseline | Sprint 13 HEAD | Status |
|---|---|---|---|---|
| 1 | Stubs sweep | 2 TODO + Moon 2-13 OOS | 0 ship-blockers (2 doc-comment refs only) | ✓ HOLD/IMPROVED |
| 2 | Silent fails | 11 empty catches | **1** intentional (PointOfInterest.cs:94) | ✓ IMPROVED |
| 3 | Commented-out wiring | **3 active-path bugs** | **0** — all 3 live | ✓ **FIXED** |
| 4 | Workarounds | RuntimeSpawnerInsurance + IsPlaying gate intact | Same — no new bypasses, IsPlaying gate at PIH:234,252 | ✓ HOLD |
| 5 | Moon1_Systems orphan | 0 inline `!u!115` | **0** | ✓ HOLD |
| 6 | Prefab integrity | Cathedral GUID fixed, Moon1 FBX healthy | Cathedral GUID 0 hits, Moon1 Blender FBX 18k-84k bytes | ✓ HOLD (LFS vendor anims still pointers — non-Moon-1) |
| 7 | Dialogue speaker map | Fixed snake_case | Verified snake_case + short keys | ✓ HOLD |
| 8 | Scene authoring | Runtime-heavy spawners | 127 `new GameObject` in EchohavenContentSpawner, 64 in RuntimeHUDBuilder | ⚠ HOLD — unchanged, deferred |
| 9 | GameEvents pairs | OnDayChanged/Brazier* missing | **All 3 declared** + 3 RuntimeHUDBuilder subs wired (MS.L1) | ✓ **FIXED** |

---

## PER-DIMENSION FINDINGS

### Lane 1 — Stubs sweep
- `git grep -nE "// TODO: implement|// stub|throw new NotImplementedException" Assets/_Project/Scripts/` → 2 hits.
- Both at `Assets/_Project/Scripts/Editor/Moon1SystemsPrefabDeepClean.cs:23,27` — doc-comment references to the Moon1_Systems orphan history, **not** actual stubs.
- Sprint 12's 2 TODOs and 2 NotImplementedExceptions appear to have been resolved or moved out of scope.

### Lane 2 — Silent fails
- Empty `catch (...) { }` blocks: **0** matches (regex incl. multiline whitespace).
- Catches with only comments: **1** at `Assets/_Project/Scripts/Integration/PointOfInterest.cs:94` — `catch (UnityException)` where comment notes "playerTag undefined" is intentional Tag-check guard. Acceptable.
- Down from Sprint 12 baseline of 11.

### Lane 3 — Commented-out wiring **(PRIMARY FIX TARGET — CLEAN)**
- `git grep -nE "// (PlayerAbility|PlayerWeaponSwitcher|DayNightCycle|AutomatedPrefab|AetherFieldSystem)" Assets/_Project/Scripts/` → 2 hits, both docstring summary comments (not commented-out code).
- **PlayerWeaponSwitcher.cs:23** — `Awake()` body live: `_melee = GetComponent<PlayerCombat>(); ... _ranged = GetComponent<PlayerRanged>()`. MS.L4 ✓
- **DayNightCycleController.cs:88,104,135-156** — `AetherYieldMultiplier` static field declared, seeded in init at L88-89, updated in `UpdateAetherBoost()`. MS.L3 ✓
- **AutomatedPrefabWiring.cs:147-163** — `if (bakeNavMesh) BakeMoonNavMesh(moonNum)` live, comment "// Sprint 12 #2 fix: NavMesh bake live" present. MS.L2 ✓
- **AetherFieldSystem.cs:54-66** — `SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>()` used; hardcoded `(0,1,0)` replaced. MS.L5 ✓
- **RuntimeHUDBuilder.cs:281-283** — 3 GameEvents subscribers + matching unsubs at L114-116. MS.L1 ✓

### Lane 4 — Workarounds
- `Insurance|RescueDriver|GodMode|HardOverride|EmergencyBypass` hits in non-archived runtime: only `RuntimeSpawnerInsurance.cs` (Sprint 12 baseline).
- Bypass drivers (`SimplePlayerDriver`, `Moon1Lifeline`) remain `.archived` under `_archived_bypass_drivers_2026_05_31/`.
- `PlayerInputHandler.cs:234,252` — `GameStateManager.Instance?.IsPlaying` gate intact. No `// EMERGENCY BYPASS` comments anywhere in `PlayerInputHandler.cs`.
- Editor-only `Moon1SceneRescue.cs:262-265` still uses legacy `UnityEngine.Input.GetKey` — Editor-tool scope, non-runtime.

### Lane 5 — Moon1_Systems orphan
- `git show HEAD:Assets/_Project/Scenes/Echohaven_VerticalSlice.unity | grep -c "^!u!115"` → **0**.
- Sprint 11 L5 fix retained.

### Lane 6 — Prefab integrity
- Cathedral invalid 16-char material GUID `d4f8e2c9a7b3f5e1`: **0 hits** in `Assets/_Project/Prefabs/Moon1/Cathedral/`.
- Moon1 Blender FBX directory: all 18,956 - 84,076 bytes (real binary FBX, not LFS pointers). Sample: `EchohavenBrazier.fbx` 84,076, `AnastasiaPrincess.fbx` 46,492, `BobsInn.fbx` 22,380.
- `Assets/_Project/Prefabs/Characters/Player.prefab`: 5,236 bytes.
- Echohaven scene: 123,860 bytes / 4,247 lines / 149 GameObjects.
- **Outstanding from Sprint 11 L6:** 547 vendor FBX (Mixamo Capoeira anim library) still LFS pointers at ~130-132 bytes. NON-Moon-1 critical (anim library), but counts against "0 of 390 healthy".

### Lane 7 — Dialogue speaker map
- `YarnTutorialBinding.cs:54-60` — 6 Milo tutorial steps + skip use `new SpeakerLine("Milo", "...")` → snake_case node title (`milo_tutorial_step_1_brazier`, etc.).
- L74-76 — default lookups: `{"Milo", "milo_intro"}`, `{"Lirael", "lirael_first_meet"}`, `{"Anastasia", "anastasia_greeting"}`.
- L18-28 docstring documents the prior PascalCase bug and the snake_case + short-key resolution.

### Lane 8 — Scene authoring
- `git show HEAD:Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs | grep -c "new GameObject"` → **127** (Sprint 12 reported ~70).
- `RuntimeHUDBuilder.cs` → **64**.
- `Moon1AnastasiaRocker.cs` → 1.
- Runtime-spawn debt **unchanged / increased**. Documented as deferred since Sprint 11 L8.

### Lane 9 — GameEvents pairs **(SECONDARY FIX TARGET — CLEAN)**
- `RuntimeHUDBuilder.cs` subscriber count: **3** (`OnRSChanged`, `OnAetherEnergyChanged`, `OnPlayerHealthChanged`) — exact MS.L1 expectation.
- `GameEvents.cs:475` `public static Action<int> OnDayChanged;`
- `GameEvents.cs:482` `public static Action<string> OnBrazierLit;`
- `GameEvents.cs:489` `public static Action OnBrazierRingComplete;`
- All 3 invokers wired at L645/657/669 with LogError-wrapped Invoke (no silent swallow).

---

## REMAINING ISSUES (BY SEVERITY)

### MEDIUM — Dirty worktree at C:/dev/_wt_sprint13
- 5 MS-touched files modified in worktree: `AetherFieldSystem.cs`, `AutomatedPrefabWiring.cs`, `PlayerWeaponSwitcher.cs`, `DayNightCycleController.cs`, `RuntimeHUDBuilder.cs`, plus assets.
- `DayNightCycleController.cs` working-tree copy is **truncated at line 125** mid-comment ("`// Note: Skybox lerping requires custom shader or RenderSettings.skybox `"). No closing braces, no `UpdateAetherBoost()` body, no namespace close. **Would not compile if committed.**
- Audit measures committed HEAD `c2756942`, which is 159 lines and intact. **Recommendation: `git checkout -- .` in the worktree before any further work.**

### LOW — Runtime scene authoring debt unchanged
- `EchohavenContentSpawner.cs` still fires 127 `new GameObject` at runtime. `RuntimeHUDBuilder.cs` 64.
- Deferred from Sprint 11 L8. Not a ship blocker but a performance / debuggability concern.

### LOW — LFS vendor anim library still pointers
- 547 FBX files in `Assets/_Project/Models/Animations/` (Capoeira, Mixamo etc.) are 130-132 byte LFS pointers.
- Not directly Moon 1 critical; Moon 1 character anims source elsewhere. Run `git lfs pull` before any character-anim QA.

---

## METHODOLOGY

- All 9 lanes evaluated against `git show HEAD:<path>` content at SHA `c2756942`, not the dirty worktree.
- Sprint 12 baseline read from `docs/audits/MOON1_ACCEPTANCE_SPRINT12_2026-06-02.md`.
- Stub regex: `// TODO: implement|// stub|throw new NotImplementedException`.
- Silent-catch detection: Python regex `catch\s*\([^)]*\)\s*\{((?:\s*//[^\n]*\n|\s)*)\}` with comment-stripping to detect both empty and comment-only catches.
- Commented-wiring regex: `// (PlayerAbility|PlayerWeaponSwitcher|DayNightCycle|AutomatedPrefab|AetherFieldSystem)`.
- Workaround regex: `Insurance|RescueDriver|GodMode|HardOverride|EmergencyBypass`.
- `!u!115` orphan check via raw scene YAML grep at HEAD.
- FBX size sweep via `find -size -1000c` and `-size +1000c`.

---

*Sprint 13 audit · 2026-06-02 · SHA c2756942 · feature/consolidate-moon-architecture*
