# Sprint 11 Lane 3 — Commented-Out Code Audit

**Date:** 2026-06-02
**Branch:** `agent/audit/commented-code`
**Worktree:** `C:\dev\_wt_s11_l3_commented`
**Base SHA:** `e0766030` (runtime fix: RuntimeSpawnerInsurance.AddComponent restored)
**Scope:** Every commented-out chunk under `Assets/_Project/Scripts/` — classified for restore vs. delete vs. leave alone.

> Triggered by the RuntimeSpawnerInsurance regression: sibling agent had `go.AddComponent<EchohavenContentSpawner>();` commented out, which left an empty GameObject named `EchohavenContentSpawner` with no spawner component, so no NPCs / props / golems / artifacts ever appeared in the scene at runtime. That bug class is exactly what this audit is hunting.

---

## Method

Five PowerShell sweeps over `git ls-files Assets/_Project/Scripts/*.cs`:

1. `// SUPERSEDED` markers (sibling-agent leftover labels) — 34 hits
2. Commented-out wiring calls (`// .AddComponent<>`, `// .SetParent(`, `// Instantiate(`) — 4 hits
3. Commented-out subscription / spawn / register / RaiseEvent / Trigger statements with `;` terminator — 2 explanatory hits
4. Commented-out code-shaped lines (`// Identifier.Method(args);` / `// var = expr;`) — **46 hits** (non-ThirdParty), 107 hits in `ThirdParty/LeanTween` debug logs (ignored as vendor noise)
5. `/* … */` blocks > 5 lines — 9 hits
6. `.candidate` files — 0 (none tracked, none untracked)

---

## Totals

| Category | Count |
|---|---:|
| 🔴 ACTIVE-PATH WIRING (restore or fix) | **9** |
| 🟡 SUPERSEDED MARKER (orphan label, safe to delete) | **34** |
| ⚪ EXPLANATORY / FUTURE / GATED (leave alone) | **51** |
| ThirdParty / vendor (LeanTween dev logs) | 107 |
| Big `/* */` blocks (mostly assembly-circular-dep stubs) | 9 |
| **Total project hits inspected** | **103** |

Of the 9 🔴 entries, **7** are confirmed runtime bugs where the active code path now silently no-ops something the player or game state was supposed to feel.

---

## File-by-file table

### 🔴 ACTIVE-PATH WIRING — Restore or rewrite

| File:Line | Comment | Why it's RED | Suggested fix |
|---|---|---|---|
| `Assets/_Project/Scripts/Gameplay/PlayerWeaponSwitcher.cs:27` | `// _melee = GetComponent<PlayerCombat>();` | `Awake()` no longer caches melee/ranged refs — `_melee` / `_ranged` fields themselves are commented out at L17–18. | Either delete the whole file (Phase 12 disabled both `PlayerCombat` + `PlayerRanged`) or wire to `PlayerAbilityController` instead. Currently fires `OnWeaponChanged` event with no listeners and no enable/disable side-effect. |
| `Assets/_Project/Scripts/Gameplay/PlayerWeaponSwitcher.cs:28` | `// _ranged = GetComponent<PlayerRanged>();` | Same as above. | Same as above. |
| `Assets/_Project/Scripts/Gameplay/PlayerWeaponSwitcher.cs:62` | `// if (_melee != null) _melee.enabled = currentWeapon == WeaponType.Melee;` | `ApplyWeaponState()` body is fully commented — Q / D-pad-Up does nothing visible. | Replace body with a call into `PlayerAbilityController` to swap an `activeWeapon` state, or delete the file. |
| `Assets/_Project/Scripts/Gameplay/PlayerWeaponSwitcher.cs:63` | `// if (_ranged != null) _ranged.enabled = currentWeapon == WeaponType.Bow;` | Same as L62. | Same. |
| `Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:101` | `// EconomySystem.Instance.SpendResonanceScore(harmonicRSCost);` | **Harmonic Strike never spends RS.** Player can spam it forever — combat economy is dead. Cost field exists, cooldown gating works, but the spend is a comment. | Re-wire via `RunProgressTracker.Instance` (Integration assembly) using the pattern already used in `PlayerAbilityManager.cs:130`, OR add a thin `Tartaria.Core` event `GameEvents.RaiseRSSpendRequested(cost)` and let `EconomySystem` listen. |
| `Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:142` | `// EconomySystem.Instance.SpendResonanceScore(shieldRSCost);` | Same as L101 — Frequency Shield is free. | Same fix. |
| `Assets/_Project/Scripts/Integration/PlayerAbilityManager.cs:116` & L133 | `// if (currentRS < ability.rsCost) return false;` / `Debug.Log($"Would consume {ability.rsCost} RS here");` | The ability cast pipeline literally logs "Would consume" instead of consuming. Together with `PlayerAbilityController.cs` above, the entire ability-economy is **0% wired**. | Add `RunProgressTracker.ConsumeRS(float)` API (currently "pending" per the comment) and call it here. Block 116 says "Note: ConsumeRS() API pending in ResonanceScoreTracker" — that API needs to actually land. |
| `Assets/_Project/Scripts/Integration/DayNightCycleController.cs:124` | `// ExcavationSystem.AetherYieldMultiplier = boost;` | `UpdateAetherBoost()` computes a 1.0× or 1.2× multiplier and then throws it away. Night-Aether-boost gameplay rule is a no-op. | Either add `ExcavationSystem.AetherYieldMultiplier` static (it doesn't exist) or surface via `GameEvents.OnAetherYieldChanged`. The TODO is honest about being unwired. |
| `Assets/_Project/Scripts/Editor/AutomatedPrefabWiring.cs:149` | `// UnityEditor.AI.NavMeshBuilder.BuildNavMesh();` | The `bakeNavMesh` bool is exposed and the success log is unconditional, but the call is commented. Anyone toggling `bakeNavMesh = true` gets a fake success with no nav surface. | Restore the call (the `using` for `UnityEditor.AI` may be needed) or rename the toggle to `// TODO bakeNavMesh disabled — Unity 6 nav surface API`. |

### 🟡 SUPERSEDED MARKER — Orphan labels, safe to delete

All 34 `// SUPERSEDED 2026-05-31 …` lines are sibling-agent leftovers from the missing-symbol stub-removal pass. They name classes that don't exist (`AnastasiaController`, `GameLoopController`, `LiraelController`, `RuntimeHUDBuilder`, `TutorialSystem.ForceComplete(TutorialStep.Discovery)`, `CombatWaveManager.BuildZoneEncounter`, `WorldBoundary`). The replacement wiring already lives next to each label (e.g., `GameEvents.RaiseBuildingRestored` at `InteractableBuilding.cs:651–658` directly above the SUPERSEDED label at L657).

| File | Lines | Replacement already wired? |
|---|---|---|
| `Camera/RestorationCinemachine.cs` | 9, 19 | ✅ `Moon1CinematicMoments` owns this |
| `Editor/Moon1AutoWire.cs` | 25 | ✅ `Tartaria/1 Build/Build Out Moon 1 NPCs` menu |
| `Editor/Moon1WireMilo.cs` | 28 | ✅ Same menu as above |
| `Integration/EchohavenContentSpawner.cs` | 259, 1000, 1002, 1005, 1006, 1940–1942, 2271, 2277, 2278, 2296, 2298, 2572, 2603, 2604, 2615, 2943, 2993, 3037, 3038 | Partially — some referenced classes never reappeared (LiraelController, AnastasiaController, RuntimeHUDBuilder, TutorialStep.Discovery). The labels point at dead code. Lirael/Anastasia behaviors are in their `.prefab` files now, not in code instances. |
| `Integration/InteractableBuilding.cs` | 239, 491, 495, 656 | ✅ `GameEvents.RaiseBuildingRestored` + `TutorialSystem.Instance?.ForceComplete` calls live immediately around each label |
| `Integration/RuntimeSpawnerInsurance.cs` | 36, 38, 39, 40, 41 | ❌ **This is the one that just bit us** — `e0766030` restored the `AddComponent<>` call. Lines 36–41 are now historical labels that should be deleted in cleanup. |

**Recommendation:** A single sweep can delete every `// SUPERSEDED 2026-05-31` line. Zero behavior change, big readability win. Combine with deletion of the empty-shell lines they wrap.

### ⚪ EXPLANATORY / FUTURE / GATED — Leave alone

| Bucket | Example | Why it's WHITE |
|---|---|---|
| `// CrashReporter.AddBreadcrumb(...)` × 9 | `Core/BreadcrumbLogger.cs:71`, `Core/PlayerSentimentTracker.cs:261/269/306/316/330`, `Core/FeedbackReporter.cs:206/210/211` | All paired with `// TODO: Restore after implementing event-driven telemetry (CrashReporter moved to LiveOps)`. Telemetry pipeline is intentionally offline pending LiveOps split. |
| `// Steamworks.* SDK calls` × 7 | `Integration/SteamBridge.cs:30, 46, 47, 64, 65, 73` | Gated by `#if STEAMWORKS` directive that has no scripting-define. Pre-Steam-package drop-in stubs — comment is part of the interface contract. Phase-1 ships pre-Steam per `PHASE_1_SCOPE.md`. |
| `// CompressionHelper.Compress/Decompress` × 4 | `Save/SaveManager.cs:386, 462, 465, 802` | Awaiting `Serialization` assembly that hasn't been added. Path is functional without compression (raw bytes work). |
| Moon3 / Boss / Map deferred behavior × 8 | `Gameplay/Moon3OrphanTrainPuzzle.cs:183, 199, 263`, `Integration/BossEncounterSystem.cs:1690`, `Gameplay/PlayerDodge.cs:72`, `Gameplay/PlayerHealth.cs:117`, `Gameplay/PlayerAnimatorBridge.cs:75`, `Integration/RuntimePostProcessingSetup.cs:116` | Cross-assembly forward refs (`VFXController`, `QuestManager`, `HUDController`, `RailEscortController`, `CombatHitReactor`, `PlayerStamina`) deliberately commented to break Tartaria.* circular deps. Audio/haptic fallback lines are wired and firing. |
| `Editor/Moon1AcceptanceAudit.cs:70-72` | Self-referential — this file COUNTS `.candidate` files | Not a behavior comment — leave alone. |
| Single-shot `// EconomySystem.Instance.SpendResonanceScore(...)` debug-log pair × 2 | `Gameplay/PlayerAbilityController.cs:96, 137` | These are the `Debug.Log("Not enough RS for …")` warnings above the spend call — they're paired with the 🔴 entries at L101/L142. Captured under RED above. |
| Anastasia footstep / NewGamePlus / UI accessibility / MainMenu / WorldAmbient post-processing × 6 | `Integration/AnastasiaController.cs:665`, `Save/NewGamePlusSystem.cs:91`, `UI/AccessibilityManager.cs:121`, `UI/MainMenuOverlay.cs:38`, `Gameplay/WorldAmbientController.cs:266`, `Integration/RuntimePostProcessingSetup.cs:116` | All paired with the canonical replacement on the next line (`AudioManager.Instance?.PlaySFX(...)`) or a `// Note: API pending` TODO. The comment serves as before/after documentation. |

### Big `/* */` blocks (>5 lines)

| File:Line | Lines | Classification | Notes |
|---|---|---|---|
| `Core/EconomyBalanceMonitor.cs:117, 140, 212` | 7+7+7 | ⚪ WHITE | All three labelled `// DISABLED: InventorySystem is in Tartaria.Gameplay (circular dependency)`. Intentional asmdef firewall. |
| `Core/ObjectPool.cs:214, 256, 286` | 23+15+6 | ⚪ WHITE | `// DISABLED: Generic GameObject pooling broken (GameObject is not Component)` — replaced by `SpawnParticle` API. The block is documentation of why the generic API is gone. |
| `Editor/Moon1RebuildCharacterPrefabsFromBlender.cs:16` | 141 | ⚪ WHITE | Top-of-file XML-style doc summary, not commented code. Misclassified by the regex. Leave alone. |
| `Gameplay/SkillTreeSystem.cs:352` | 32 | 🟡 SUPERSEDED-EQUIVALENT | `// DISABLED: SkillTreeAsset references disabled due to circular dependency` — returns an empty tree and warns. **Functionally a stub**, but the disabled comment is honest. Consider escalating to 🔴 if Skill Tree gameplay needs to ship in Phase 1 (it's not in `PHASE_1_SCOPE.md`). |
| `UI/EquipmentSlotUI.cs:132` | 8 | ⚪ WHITE | `// Simple pulsing glow effect (LeanTween disabled - Phase 34)` — cosmetic, no behavior loss. |

---

## Top 10 🔴 ACTIVE-PATH offenders (restore priority)

The 9 RED entries collapse into 4 distinct bugs by domain. Listed by **player-visible severity**:

1. **`PlayerAbilityController.cs:101, 142`** — Harmonic Strike + Frequency Shield never spend RS. Combat economy is decorative. **Severity: HIGH.** Restore via the same `RunProgressTracker` pattern already started in `PlayerAbilityManager.cs:130` (which itself needs fix #2 below).
2. **`PlayerAbilityManager.cs:116, 133`** — Ability cast pipeline logs `"Would consume {rsCost} RS here"` instead of consuming. Same root cause as #1. **Severity: HIGH.** Implement `ResonanceScoreTracker.ConsumeRS(float)` and `HasRS(float)` — currently marked `// Note: ConsumeRS() API pending`.
3. **`PlayerWeaponSwitcher.cs:17, 18, 27, 28, 62, 63`** — Q / D-pad-Up emits `OnWeaponChanged` but the actual weapon-component toggling is fully commented. PlayerCombat and PlayerRanged were removed in Phase 12 but the switcher still ships. **Severity: MEDIUM.** Either delete the file outright (no listeners on `OnWeaponChanged` in the project) or rewire to a `PlayerAbilityController.SetActiveWeapon(WeaponType)` API.
4. **`DayNightCycleController.cs:124`** — Night-Aether-yield-boost is computed and discarded. Excavation rewards never get the 1.2× multiplier. **Severity: MEDIUM.** Add `ExcavationSystem.AetherYieldMultiplier` static or raise `GameEvents.OnAetherYieldChanged(float)`.
5. **`AutomatedPrefabWiring.cs:149`** — Editor-only `bakeNavMesh` toggle is non-functional but logs success. Affects Moons 2–13 wiring tool, not Moon 1 runtime. **Severity: LOW.** Restore the `UnityEditor.AI.NavMeshBuilder.BuildNavMesh()` call (Unity 6 may need the new `NavMeshSurface` API instead).

Items 6–10 don't exist — the 9 RED file:line entries collapse to these 5 distinct bugs.

---

## Recommended action plan

1. **Immediate (this sprint):**
   - Fix items #1 + #2 together (single PR: implement `ResonanceScoreTracker.ConsumeRS/HasRS`, then wire from both controllers). Restores combat economy.
   - Delete the 34 `// SUPERSEDED 2026-05-31` lines via single sed pass. Zero behavior risk.
2. **Next sprint:**
   - Decide PlayerWeaponSwitcher fate (delete vs. rewire). If deleting, also remove the `OnWeaponChanged` event surface — nothing listens.
   - Fix `DayNightCycleController.UpdateAetherBoost()` to actually apply the multiplier.
3. **Leave alone:**
   - All `Steamworks.*`, `CompressionHelper.*`, and `CrashReporter.*` comments — gated by absent dependencies, intentional pre-integration drop-ins.
   - All Moon 3 cross-assembly stubs in `Moon3OrphanTrainPuzzle.cs` — circular-dep firewalls.
   - All `ObjectPool.cs` and `EconomyBalanceMonitor.cs` block comments — explicit "DISABLED:" with replacement APIs documented.
   - All `ThirdParty/LeanTween/*` debug-log comments (107 hits) — vendor code.

---

## Sources

- Audit script & raw hit list: regenerable from this file's Method section.
- Sibling bug-trigger: commit `e0766030` (`runtime fix: RuntimeSpawnerInsurance.AddComponent restored — sibling-agent regression — was creating empty GameObjects with no spawners`).
- Mandate reference: `CLAUDE.md` § 2026-05-30 LATE-NIGHT MANDATE — "NEVER ship a file with `// TODO: implement` or `// stub` or method bodies that only contain `;`". The RED entries above all violate this in spirit (signature exists, body is commented out).
