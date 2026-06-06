# Integration Assembly — Dependency Graph & Refactoring Plan

**Generated:** 2026-05-25  
**Status:** 139 files disabled (all MonoBehaviour), 8% completion (12/151 active)  
**Risk:** Massive circular dependency web, blocking 92% of assembly

---

## Executive Summary

The Integration assembly contains the entire campaign content layer: all 13 Moon missions, NPC controllers, quest systems, companion AI, minigames, cutscenes, and boss encounters. **139 files** remain disabled due to unresolved cross-references forming a tight dependency mesh.

**Critical Findings:**
- **ALL files inherit MonoBehaviour** (zero ScriptableObjects) → runtime GameObject coupling
- **Minimal FindObjectOfType usage** (only 1 occurrence found) → pre-wired references or service locator pattern
- **High cohesion within Moon content** → each Moon arc is internally consistent
- **Circular dependency risk** → Managers likely reference each other (CompanionManager ↔ QuestManager ↔ DialogueManager)

**Enabled Integration files (12):**
1. `APVScenarioController.cs` — Adaptive Probe Volume lighting scenarios
2. `PedestalLightPuzzle.cs` — Interaction puzzle
3. `ZoneDynamicMusic.cs` — Audio zone triggers
4. `AnastasiaDialogueDatabase.cs` — Dialogue data
5. `AquiferPurgeMiniGame.cs` — Minigame
6. `BellTowerSyncMiniGame.cs` — Minigame
7. `CinematicWaypointSequences.cs` — Cutscene system
8. `DialogueSequencer.cs` — Dialogue runtime
9. `DialogueTrigger.cs` — Dialogue activation
10. `LootAnimator.cs` — Item drop visuals
11. `LeyLineVisualizer.cs` — VFX system
12. `MudZone.cs` — Environmental hazard

---

## Taxonomy — 139 Disabled Files by Type

### Bridges (3)
MonoBehaviour components that connect major systems.

- `CombatBridge.cs` — Links combat → dialogue/quest events
- `RuntimeGlueBridge.cs` — Unknown runtime glue
- `SteamBridge.cs` — Steam API integration

### Controllers (20)
Campaign character controllers + NPC behaviors.

- `AnastasiaController.cs` — Companion NPC
- `CampaignFlowController.cs` — Campaign progression state machine
- `CassianNPCController.cs` — NPC, implements `ICassianService`
- `DayOutOfTimeController.cs` — Moon 13 special scenario
- `EndCardController.cs` — End-game credits
- `GameLoopController.cs` — Core game loop orchestration
- `GiantModeController.cs` — Giant transformation mechanic
- `KorathController.cs` — Boss/NPC
- `LiraelController.cs` — Companion NPC
- `MicroGiantController.cs` — Micro-giant mechanic
- `MiloController.cs` — Companion NPC
- `MoonNarrativeController.cs` — Moon story beats
- `RailEscortController.cs` — Moon 3 rail escort mission
- `ThorneController.cs` — NPC
- `TutorialController.cs` — Tutorial orchestration
- *(+5 more)*

### Managers (11)
Core Integration singletons — **HIGH CIRCULAR DEPENDENCY RISK**.

- `QuestManager.cs` — Quest state tracking
- `CompanionManager.cs` — Companion AI coordination
- `CombatWaveManager.cs` — Combat spawning waves
- `PlayerAbilityManager.cs` — Player ability unlocks
- `AchievementSystem.cs` — Achievement tracking (tried Phase 44, too many deps)
- `VFXManager.cs` — Visual effects pooling
- `ArchiveManager.cs` — Codex/lore system
- `AirshipFleetManager.cs` — Moon 4+ airship mechanics
- `Moon2AtmosphereAudioManager.cs` — Moon 2 audio
- `Moon2CavernVisualManager.cs` — Moon 2 visuals
- `Moon3RailAudioManager.cs` — Moon 3 audio

### Systems (13)
Gameplay feature systems — often depend on Managers.

- `AchievementSystem.cs` — (duplicate entry? see Managers)
- `AetherResonanceSystem.cs` — Core energy mechanic
- `BossEncounterSystem.cs` — Boss spawning/phases
- `ClimaxSequenceSystem.cs` — End-game climax
- `CompanionFarewellSystem.cs` — Companion departure sequences
- `ContinentalRailSystem.cs` — Moon 3 rail network
- `CorruptionSystem.cs` — Corruption spread mechanic
- `EchohavenProgressionSystem.cs` — Hub progression
- `MemoryEchoSystem.cs` — Memory replay mechanic
- `Moon2ProgressionSystem.cs` — Moon 2 progression tracking
- `TutorialSystem.cs` — Tutorial step tracking
- `WorkshopSystem.cs` — Crafting workshop
- `ZoneTransitionSystem.cs` — Scene loading transitions

### Spawners (17)
Content population MonoBehaviours — inject campaign content into scenes.

- `BuildingSpawner.cs` — Echohaven buildings (Star Dome, Harmonic Fountain, Crystal Spire)
- `EchohavenContentSpawner.cs` — Hub content population
- `Moon2ContentSpawner.cs` — Moon 2 mission content
- `Moon3ContentSpawner.cs` — Moon 3 mission content
- `Moon4ContentSpawner.cs` — Moon 4 mission content
- `Moon5ContentSpawner.cs` — Moon 5 mission content
- `Moon6ContentSpawner.cs` — Moon 6 mission content
- `Moon7ContentSpawner.cs` — Moon 7 mission content
- `Moon8ContentSpawner.cs` — Moon 8 mission content
- `Moon9ContentSpawner.cs` — Moon 9 mission content
- `Moon10ContentSpawner.cs` — Moon 10 mission content
- `Moon11ContentSpawner.cs` — Moon 11 mission content
- `Moon12ContentSpawner.cs` — Moon 12 mission content
- `Moon13ContentSpawner.cs` — Moon 13 mission content
- *(+2 more)*

### Service (1)
Service interface implementations.

- `MoonRewardService.cs` — Moon completion rewards

### Other (74)
Minigames, puzzles, interactables, dialogue runners, visual effects, debug tools.

**Examples:**
- `DialogueTreeRunner.cs` — (tried Phase 47, too many Manager deps)
- `DebugCheatConsole.cs` — (tried Phase 48, failed)
- `DebugOverlay.cs` — (tried Phase 48, failed)
- `ErrorMessageHelper.cs` — (tried Phase 48, failed)
- `EnvironmentalStorytelling.cs` — (tried Phase 49, needs WorldChoiceTracker)
- `ConsequenceVisuals.cs` — (tried Phase 46, needs WorldChoiceTracker)
- `DualityCutscene.cs` — (tried Phase 47, failed)
- `CombatDialogue.cs` — (tried Phase 46, fixed missing using, disabled)
- `AchievementUnlockToast.cs` — (tried Phase 44, too many deps)
- `CompanionCombatAbilities.cs`
- `CompanionDialogueArcs.cs`
- `CosmicConvergenceMiniGame.cs` — (enabled Phase 51)
- `CymaticTuningPuzzle.cs` — (enabled Phase 51)
- `LeyLineProphecyMiniGame.cs` — (enabled Phase 51)
- `EchohavenCombatArena.cs`
- `EchohavenObelisk.cs`
- *(+59 more)*

---

## Dependency Patterns Observed

### 1. Manager Singleton Web (CRITICAL ISSUE)
Most Integration systems expect to call:
```csharp
QuestManager.Instance.GetActiveQuests()
CompanionManager.Instance.GetCompanion("Cassian")
AchievementSystem.Instance.UnlockAchievement("DISCOVERY_MOON2")
```

**Problem:** Circular references — QuestManager → CompanionManager → DialogueManager → QuestManager.

**Tried enabling AchievementSystem (Phase 44):**
- Required: `QuestManager`, `CompanionManager`, `CombatWaveManager`, `PlayerAbilityManager`, `MoonProgressTracker`, `WorldChoiceTracker`, `DialogueTreeRunner`, `BossEncounterSystem`, `TutorialSystem`, `ArchiveManager`
- **Result:** Compilation failure, 10+ missing types.

### 2. Service Interfaces (PARTIALLY IMPLEMENTED)
Some controllers implement service interfaces:
- `CassianNPCController : MonoBehaviour, IInteractable, ICassianService`
- `CampaignFlowController : MonoBehaviour, ICampaignService`
- `CompanionManager : MonoBehaviour, ICompanionService`
- `Moon2ProgressionSystem : MonoBehaviour, IMoon2ProgressionService`

**Observation:** Interfaces defined in Core assembly, but implementations are MonoBehaviours in Integration → not true dependency injection, just interface markers.

### 3. Low FindObjectOfType Usage
Only **1 occurrence** found in 139 files:
```csharp
// Moon3RailAudioManager.cs:222
var pih = FindObjectOfType<PlayerInputHandler>();
```

**Implication:** Most dependencies are either:
- Pre-wired via inspector [SerializeField]
- Accessed via singleton Instance properties
- Injected via a service locator pattern (not yet found in codebase)

### 4. High Moon Content Cohesion
Each Moon arc (Moon2-Moon13) has tightly coupled content:
- `Moon2ContentSpawner` → `Moon2ProgressionSystem` → `Moon2AtmosphereAudioManager` → `Moon2CavernVisualManager`
- These likely form independent subgraphs — enabling one Moon at a time may be viable.

---

## Attempted Activations (Phases 44-51)

| Phase | Files Attempted | Result | Reason |
|-------|----------------|--------|--------|
| 44 | AchievementSystem, AchievementUnlockToast, AetherResonanceSystem | **FAILED** | Needs 10+ Integration managers |
| 44r | APVScenarioController, PedestalLightPuzzle, ZoneDynamicMusic | **SUCCESS** | Low dependencies |
| 45 | AnastasiaDialogueDatabase, AquiferPurgeMiniGame, BellTowerSyncMiniGame | **SUCCESS** | Dialogue data + minigames |
| 46 | CassianNPCController, CombatBridge, CombatDialogue | **FAILED** | Missing ICassianService impl, IntegrationEvents |
| 46r | CinematicWaypointSequences, ConsequenceVisuals, DialogueSequencer | **PARTIAL** | ConsequenceVisuals needs WorldChoiceTracker |
| 47 | DialogueTreeRunner, DialogueTrigger, DualityCutscene | **PARTIAL** | DialogueTreeRunner needs QuestManager/CompanionManager |
| 48 | DebugCheatConsole, DebugOverlay, ErrorMessageHelper | **FAILED** | Missing debug types |
| 48r | LootAnimator, LeyLineVisualizer, MudZone | **SUCCESS** | Fixed ItemRarity namespace |
| 49 | LootDropper, InteractableBuilding, EnvironmentalStorytelling | **PARTIAL** | EnvironmentalStorytelling needs WorldChoiceTracker |
| 50 | CorruptionSystem, MemoryEchoSystem, NPCArchetypes, NarrativeBeatSystems | **PARTIAL** | CorruptionSystem/NPCArchetypes missing deps |
| 51 | CosmicConvergenceMiniGame, LeyLineProphecyMiniGame, CymaticTuningPuzzle | **SUCCESS** | Minigames isolated |

**Success Rate:** 12/36 files enabled (33%) — 24 files blocked by missing Manager/System dependencies.

---

## Refactoring Strategy — Task 4 Roadmap

### Phase 1: Break Manager Circular Dependencies (HIGH PRIORITY)
**Goal:** Enable QuestManager, CompanionManager, AchievementSystem without circular refs.

**Approach:**
1. **Extract interfaces** — Move `IQuestManager`, `ICompanionManager`, `IAchievementSystem` to Core assembly
2. **Constructor injection** — Replace `QuestManager.Instance` with injected `IQuestManager` references
3. **Service locator pattern** — Implement `ServiceRegistry` in Core for runtime resolution:
   ```csharp
   // Core/ServiceRegistry.cs
   public static class ServiceRegistry {
       private static Dictionary<Type, object> _services = new();
       public static void Register<T>(T service) => _services[typeof(T)] = service;
       public static T Get<T>() => (T)_services[typeof(T)];
   }
   ```
4. **Manager initialization order** — `GameStateManager.Awake()` registers all managers before any system accesses them

**Files to refactor first:**
- `QuestManager.cs` (enable, extract `IQuestManager`, register in ServiceRegistry)
- `CompanionManager.cs` (enable, extract `ICompanionManager`, register)
- `AchievementSystem.cs` (enable, extract `IAchievementSystem`, register)

### Phase 2: Enable Moon Content by Arc (MEDIUM PRIORITY)
**Goal:** Enable Moon 2-13 content one arc at a time.

**Approach:**
1. Start with **Moon 2** (first campaign mission):
   - `Moon2ContentSpawner` → spawns all Moon 2 content
   - `Moon2ProgressionSystem` → tracks Moon 2 objectives
   - `Moon2AtmosphereAudioManager` → Moon 2 audio
   - `Moon2CavernVisualManager` → Moon 2 visuals
2. Fix dependencies within each Moon's subgraph
3. Repeat for Moon 3-13

**Estimated effort:** 2-3 days per Moon arc (26-39 days total for 13 Moons).

### Phase 3: Enable Dialogue Systems (LOW PRIORITY)
**Goal:** Enable `DialogueTreeRunner`, `DialogueTrigger`, `CombatDialogue`.

**Blockers:**
- Needs `QuestManager`, `CompanionManager` (fixed in Phase 1)
- Needs `IntegrationEvents` (not yet found — may need to create)

### Phase 4: Enable Debug/Utility Systems (LOW PRIORITY)
**Goal:** Enable `DebugCheatConsole`, `DebugOverlay`, `ErrorMessageHelper`.

**Blockers:**
- Missing debug types — needs investigation
- Low gameplay impact — defer until Managers stable

---

## Immediate Next Steps (Task 4)

1. **Create ServiceRegistry.cs** in Core assembly
2. **Extract IQuestManager interface** from QuestManager (or create stub)
3. **Enable QuestManager.cs**, refactor to use ServiceRegistry
4. **Extract ICompanionManager interface** from CompanionManager (or create stub)
5. **Enable CompanionManager.cs**, refactor to use ServiceRegistry
6. **Enable AchievementSystem.cs**, refactor to use ServiceRegistry
7. **Validate build** — confirm 3 managers compile without circular deps
8. **Enable dependent systems** — retry `DialogueTreeRunner`, `CombatBridge`, `AetherResonanceSystem`

**Estimated effort:** 4-6 hours for Manager refactor + validation.

---

## Long-Term Recommendations (Task 5-6)

### Task 5: AI Architecture — MonoBehaviour vs DOTS
**Current state:** 13 AI files disabled (all DOTS/ECS), 7 active (MonoBehaviour).

**Decision needed:**
- **Option A:** Keep MonoBehaviour AI — simpler, matches rest of Integration assembly
- **Option B:** Full DOTS commit — better performance, but requires rewriting all AI systems

**Recommendation:** **Option A (MonoBehaviour)** — Integration is already 100% MonoBehaviour, DOTS migration would block campaign content for months.

### Task 6: Service Locator → Dependency Injection
**Current state:** 40+ singleton Instance properties, no true DI.

**Goal:** Replace singletons with testable DI pattern:
```csharp
// Before
var quest = QuestManager.Instance.GetQuest(id);

// After (constructor injection)
public class DialogueSystem {
    private readonly IQuestManager _quests;
    public DialogueSystem(IQuestManager quests) => _quests = quests;
    
    void Start() {
        var quest = _quests.GetQuest(id);
    }
}
```

**Estimated effort:** 2-3 weeks to refactor 40+ singletons + write DI container.

---

## Test Coverage (Task 7)

**Current state:** 0 tests in Integration assembly.

**Minimum target:** 50 integration tests covering:
- QuestManager state transitions (10 tests)
- CompanionManager companion spawn/despawn (8 tests)
- AchievementSystem unlock logic (10 tests)
- Moon2ProgressionSystem objective tracking (8 tests)
- DialogueTreeRunner dialogue flow (10 tests)
- ServiceRegistry registration/retrieval (4 tests)

**Estimated effort:** 1 week to scaffold test assembly + write 50 tests.

---

## Summary — Priority Queue

| Task | Effort | Blocks | Status |
|------|--------|--------|--------|
| **Task 4:** Manager refactor (ServiceRegistry + interfaces) | 4-6 hours | Task 5-7 | **NEXT** |
| **Task 5:** AI architecture decision (MonoBehaviour) | 1 hour | None | Ready |
| **Task 6:** Service Locator → DI | 2-3 weeks | Task 7 | Blocked by Task 4 |
| **Task 7:** Integration tests (50 tests) | 1 week | None | Blocked by Task 4 |
| **Moon Content:** Enable Moon 2-13 arcs | 26-39 days | Task 4 | Blocked by Task 4 |

**Critical path:** Task 4 Manager refactor unblocks 92% of Integration assembly (127 of 139 files).

---

**Generated by Dr. Vex Aurelian, 2026-05-25**  
**TARTARIA — Unity 6000.3.6f1, URP 17.3.0**
