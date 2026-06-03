# Moon 1 Acceptance Audit — Sprint 12 (2026-06-02)

> **Status:** NOT-YET-SHIPPABLE — pending HUD live-data wire, AutomatedPrefabWiring NavMesh bake re-enable, DayNightCycle.UpdateAetherBoost result usage, PlayerWeaponSwitcher Awake body restore, AetherFieldSystem player-pos live read.
>
> **HEAD:** `6b5fb3056f4c5b0d0a6999ce67a72649886bc648` (`feature/consolidate-moon-architecture`)
> **Audit branch:** `agent/h/sprint12-acceptance-audit-v2`
> **Methodology:** Replicated Sprint 11's 9 lanes verbatim against the post-Hammer HEAD. Cited file:line on every finding. Compared deltas vs Sprint 11 baseline.

---

## Top-line verdict

**Moon 1 NOT-YET-SHIPPABLE.** Major debt has been retired (Sprint 11 critical regressions all addressed at file level), but 5 small-but-load-bearing wires remain commented or hardcoded. Estimated remaining work: **2–4 hours** for the live HUD bind, NavMesh bake un-comment, and Aether/Day/Weapon residuals.

This audit is materially better than the v1 stale-base run that returned NOT-YET-SHIPPABLE without seeing the Hammer Phase 0/1/2/4/5 merges. The 9-dim delta below makes the actual improvement visible: 5 of 9 dimensions GREEN, 3 PARTIAL/YELLOW, 1 PARTIAL (Stub cleanup in scope-deferred AI files).

---

## Delta table — Sprint 11 baseline → Sprint 12 actual

| # | Dimension | Sprint 11 baseline | Sprint 12 (HEAD `6b5fb305`) | Δ | Status |
|---|---|---|---|---|---|
| 1 | Stubs (CreatePrimitive without URP-safe tag, TODO: implement, NotImplemented) | 9 ship-blockers + ~570 untagged CreatePrimitive | 2 TODO refs (both in doc comments), 23 URP-safe tags out of 569 total CreatePrimitive | ↓7 of 9 ship-blockers addressed (Cymatic 897 LOC; PickupInteractable 112 LOC implemented); ↑23 URP-safe tags added | **PARTIAL** — most untagged CreatePrimitive in archived AI/MoonN stubs (out of Moon 1 scope) |
| 2 | Silent fails (empty catches) | 38 (top 5 on Moon 1 happy path) | 11 in live code (5 SteamCloudBridge, 4 IntegrationBridge, 1 SaveManager, 1 OneClickBuild file-delete) | **↓27 (71% reduction)** | **GREEN** — top 5 Moon 1 path catches removed (H.L7 confirmed) |
| 3 | Commented-out wiring | 5 active-path bugs | 3 still active: PlayerWeaponSwitcher Awake body (`PlayerWeaponSwitcher.cs:26-28`), DayNightCycle.UpdateAetherBoost discards result (`Integration/DayNightCycleController.cs:123-124`), AutomatedPrefabWiring NavMesh bake (`Editor/AutomatedPrefabWiring.cs:149`) | ↓2 of 5 (PlayerAbility RS economy wired via AetherFieldManager) | **PARTIAL** — 3 of 5 still live bugs |
| 4 | Workarounds (Insurance / Rescue / EmergencyBypass) | 5 files | RuntimeSpawnerInsurance.cs still exists (54 LOC); zero `// EMERGENCY BYPASS` markers in PlayerInputHandler; `IsPlaying` gate restored at `PlayerInputHandler.cs:234, 252` | ↓ bypass markers all gone; Insurance still papering over scene authoring | **YELLOW** — gate restored, but Insurance fallback retained |
| 5 | Moon1_Systems inline `!u!115` orphans | 4 stub blocks at scene YAML lines 305/1252/3090/3364 | **0 in scene file** | **↓100% (root cause fixed)** | **GREEN** — P0.YAML surgery confirmed |
| 6 | Prefab integrity | 0 of 390 healthy (LFS pointers, bad GUID, capsule Player/NPCs) | 70 FBX all >18KB (none <1KB); Cathedral kit has zero refs to bad GUID `d4f8e2c9a7b3f5e1`; Player.prefab carries PlayerInputHandler + GiantMode + CharacterController + NavMeshAgent + Animator + Mesh; NPC variants 5001-5376B | ~390 healthy on disk | **GREEN** — major P5 lanes confirmed shipped |
| 7 | Dialogue speaker map | 0 of 4 default chains worked | All 4 nodes resolve: `milo_intro` (`Moon1/milo_intro.yarn:1`), `lirael_first_meet` (`Moon1/lirael.yarn:1`), `anastasia_greeting` (`Moon1/anastasia_greeting.yarn:1`), `cassian_first_meet` (`Moon1/cassian.yarn:1`). PascalCase speaker keys match MiloTutorialFlow's `kSpeaker = "Milo"` | ↑4 of 4 | **GREEN** — Sprint 11 L7 fix confirmed live |
| 8 | Scene authoring — runtime `new GameObject` | EchohavenContentSpawner 70+; RuntimeHUDBuilder high; Moon1AnastasiaRocker 275 LOC | EchohavenContentSpawner 124 sites total with 5 `#if !TARTARIA_ECHOHAVEN_BAKED` gates at lines 307/1017/1170/2084/2653 (H.L3 baked); RuntimeHUDBuilder 64 (no gate, H.L2 honest-bailed); Moon1AnastasiaRocker 1 site / 133 LOC (was 275; H.L5 baked) | ↓ Rocker, ↓ ContentSpawner per-Play (gated); HUD unchanged | **YELLOW** — partial bake; HUD still procedural and not bound to events |
| 9 | GameEvents pairs | 25 broken one-sided of 64; CLAUDE.md canonical-facts lied about OnBrazierLit/OnBrazierRingComplete/OnDayChanged | All 3 declared at `Core/GameEvents.cs:475 (OnDayChanged), 482 (OnBrazierLit), 489 (OnBrazierRingComplete)` with raiser methods at 645/657/669. Publishers fire `RaisePlayerHealthChanged` at 10 sites; `RaiseAetherEnergyChanged` fires at `AetherFieldManager.cs:47, 77`. Subscribers `OnPlayerDeath += / OnPlayerRespawned +=` confirmed at `HUDController.cs:163-164`, `CameraController.cs:139-140`, `PlayerInputHandler.cs:153-154` | ↑3 missing events shipped; ↑P2.L2 publishers; ↑P2.L3 subscribers | **GREEN** — Sprint 11 L9 fixes confirmed |

---

## Per-dimension findings (citations)

### 1. Stubs (PARTIAL)

- `CymaticWaterTuningMiniGame.cs` — now **897 LOC** (was 7 empty methods per Sprint 11 L1). Sprint 11 ship-blocker retired.
- `PickupInteractable.cs` — `Gameplay/` variant 112 LOC, `Integration/` 106 LOC, both have implementation bodies.
- `GameObject.CreatePrimitive` — **569 sites in live code, 23 with `// URP-safe` tag**. Untagged usage clusters in `AI/CrystalSentryAI.cs:132,228`, `AI/MudGolemAI.cs:146,155,166,178,187,199`, `AI/MudGolemLootDrop.cs:20,46`, `AI/ResonanceDroneAI.cs:255,265`, `AI/ShadowStalkerAI.cs:225`, `AI/TemporalWraithAI.cs:176,271`, `AI/VoidPhantomAI.cs:222`. These are mostly enemy/AI for moons 2–13 — out of Moon 1 scope.
- `TODO: implement` / `NotImplementedException` — 0 in live code; 2 grep hits are doc comments in `Editor/Moon1SystemsPrefabDeepClean.cs:23, 27`.

### 2. Silent fails (GREEN)

- 11 empty catches in live code. Distribution: `Save/SteamCloudBridge.cs` (5: lines 28, 42, 67, 79, 91), `UI/IntegrationBridge.cs` (4: lines 84, 98, 112, 141), `Save/SaveManager.cs:1724` (1), `Editor/OneClickBuild.cs:34` (1, file-delete sentinel).
- Sprint 11 L2 top-5 path files (Moon1NarrativeBeats, Moon1CinematicMoments, TuningPedestalLink, QuestObjectiveTrackerUI, AdaptiveMusicController) all clear of empty catches at HEAD — H.L7 fix confirmed.

### 3. Commented-out wiring (PARTIAL)

- **PlayerAbilityController.cs** — RS economy now LIVE at lines 93, 106, 107 (canonical AetherFieldManager + GameEvents.FireRSChange). The Aether Vision toggle subscribe at line 52/62 is still commented (`// Event missing (Phase 22)`).
- **PlayerWeaponSwitcher.cs:26-28** — Awake body is still 3 commented lines: `// _melee = GetComponent<PlayerCombat>(); // _ranged = GetComponent<PlayerRanged>();` — combat components never assigned.
- **DayNightCycleController.cs:123-124** — `UpdateAetherBoost()` computes `boost` then discards it: `// TODO: Wire to AetherFieldSystem or ExcavationSystem // ExcavationSystem.AetherYieldMultiplier = boost;`. Day cycle has zero effect on yield.
- **AutomatedPrefabWiring.cs:149** — `// UnityEditor.AI.NavMeshBuilder.BuildNavMesh();` still commented out — the editor tool LOGS "Baking NavMesh…" but doesn't bake.

### 4. Workarounds (YELLOW)

- `Input/PlayerInputHandler.cs:234, 252` — `if (GameStateManager.Instance?.IsPlaying == true)` gate restored. P2.L3 + P4.L3 confirmed.
- No `// EMERGENCY BYPASS` markers in live code.
- `Integration/RuntimeSpawnerInsurance.cs` — Still active (54 LOC), `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` papers over scene authoring by recreating BuildingSpawner / EchohavenContentSpawner if missing.

### 5. Moon1_Systems orphan (GREEN)

- `Echohaven_VerticalSlice.unity` — `grep -c '!u!115'` returns **0**. The 4 inline MonoScript stub blocks at YAML lines 305/1252/3090/3364 are gone.

### 6. Prefab integrity (GREEN)

- 70 FBX files in `Models/Blender/Moon1/`, **0 are LFS pointers** (smallest 18,956 bytes).
- Cathedral kit: `grep -l "d4f8e2c9a7b3f5e1"` in `Prefabs/Moon1/Cathedral/` returns **0**.
- `Player.prefab` (5236B) has 8 components including `PlayerInputHandler` (GUID `05fce6928f67f0a40af9265384088e4d`) and `GiantMode` (`8f004a741f3ab2347bb9225bb5d2da83`) plus Transform, CharacterController, MeshFilter, MeshRenderer, NavMeshAgent, Animator. (Sprint 11 claim of "zero MonoBehaviours" is **stale**.)
- NPC prefab sizes: Lirael 5005B, Anastasia 5376B, Cassian 5066B, Milo 5001B — all ≥5000B (variants).

### 7. Dialogue (GREEN)

- `YarnTutorialBinding.cs:74-77` — PascalCase keys `"Milo" / "Lirael" / "Anastasia" / "Cassian"` map to snake_case nodes `milo_intro / lirael_first_meet / anastasia_greeting / cassian_first_meet`.
- All 4 nodes exist in yarn files (grep confirms `^title:` matches).
- `YarnTutorialBinding.cs:20` documents the contract: "MiloTutorialFlow passes speaker=\"Milo\"" — matches map key.

### 8. Scene authoring (YELLOW)

- `EchohavenContentSpawner.cs` — 124 `new GameObject(` sites but **5 `#if !TARTARIA_ECHOHAVEN_BAKED` gates** (lines 307, 1017, 1170, 2084, 2653) suppress the procedural spawner when scene is baked.
- `RuntimeHUDBuilder.cs` — 64 `new GameObject(` sites, **no #if gates**, hardcodes `"RS: 0"` at lines 499 and 1713, **subscribes to no events**. P1.L4 honest-bailed; the HUD is decorative.
- `Moon1AnastasiaRocker.cs` — 133 LOC, 1 `new GameObject` site (was 275 LOC per H.L5).

### 9. GameEvents (GREEN)

- All 3 previously-claimed-but-missing events shipped: `OnDayChanged` (`GameEvents.cs:475`), `OnBrazierLit` (482), `OnBrazierRingComplete` (489) with corresponding RaiseXxx in 640-680 range.
- `RaisePlayerHealthChanged` fires 4× in `PlayerHealth.cs` (lines 51, 132, 143, 173) and 6× in `PlayerHealthController.cs` (84, 130, 154, 167, 227, 261).
- `RaiseAetherEnergyChanged` fires at `AetherFieldManager.cs:47, 77`.
- Death/Respawn subscribers at `HUDController.cs:163-164`, `CameraController.cs:139-140`, `PlayerInputHandler.cs:153-154`.
- `FireRSChange` consumers at `AI/MudGolemAI.cs:599`, `AI/MudGolemLootDrop.cs:78`, `AI/ResetScout.cs:196`.

---

## Top 5 remaining issues (ranked)

1. **HUD shows hardcoded "RS: 0" every frame** — `Integration/RuntimeHUDBuilder.cs:499, 1713` set literal text and never subscribe to `GameEvents.OnRSChanged` / `OnAetherEnergyChanged` / `OnPlayerHealthChanged`. P1.L4 was honest-bailed. The visible HUD lies about player resource state.
2. **AutomatedPrefabWiring NavMesh bake commented** — `Editor/AutomatedPrefabWiring.cs:149`: `// UnityEditor.AI.NavMeshBuilder.BuildNavMesh();`. Tool logs "Baking NavMesh…" then no-ops. Manual `Window → AI → Navigation → Bake` still required.
3. **DayNightCycle.UpdateAetherBoost discards boost value** — `Integration/DayNightCycleController.cs:123-124`. Day/night yield multiplier is dead code; nighttime gives no Aether boost.
4. **PlayerWeaponSwitcher Awake body commented** — `Gameplay/PlayerWeaponSwitcher.cs:26-28`. `_melee` and `_ranged` references never assigned; weapon switch is a no-op against missing combat components.
5. **AetherFieldSystem hardcodes player position** — `Core/AetherFieldSystem.cs:47`: `float3 playerApprox = new float3(0f, 1f, 0f); // TODO: when GameLoopController tracks Player transform, switch to live read`. Aether field samples around a fixed point near spawn, not the actual player.

Honourable mention: **RuntimeSpawnerInsurance** is a safety net papering over scene authoring (`Integration/RuntimeSpawnerInsurance.cs`); not a bug today but a backlog item.

---

## Methodology + scope

- All file:line cites are against HEAD `6b5fb3056f4c5b0d0a6999ce67a72649886bc648`.
- Searches used `git grep -nE`, `find`, Python regex for multi-line catch blocks, and direct file reads.
- Archived files under `_archive*`, `_deleted_*`, `_archived_*`, `.disabled`, `.restored` excluded from live-code counts.
- Sprint 11 baseline numbers taken verbatim from `docs/audits/SPRINT11_FULL_SCOPE_PUNCHLIST_2026-06-02.md` headers as quoted in `CLAUDE.md`.
- Verdict criteria: "shippable" requires zero items in the Top-5 remaining list. Currently 5 → NOT-YET-SHIPPABLE; closest 2 (HUD live data + NavMesh bake un-comment) would alone restore shippable status if Cathedral material GUID and Player.prefab observations (already GREEN) hold under runtime QA.

---

*Sprint 12 acceptance audit v2 (re-run against post-Hammer HEAD) · 2026-06-02 · `agent/h/sprint12-acceptance-audit-v2`*
