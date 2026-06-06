# TARTARIA — Development Guide

**Version:** 1.0.0-beta  
**Unity Version:** 6000.3.6f1  
**Target Platform:** Windows PC (Steam)  
**Last Updated:** May 24, 2026

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Project Structure](#project-structure)
3. [Assembly Architecture](#assembly-architecture)
4. [Code Conventions](#code-conventions)
5. [Quest Creation Workflow](#quest-creation-workflow)
6. [Dialogue Authoring Guide](#dialogue-authoring-guide)
7. [Testing Procedures](#testing-procedures)
8. [Performance Guidelines](#performance-guidelines)
9. [Git Workflow](#git-workflow)

---

## Project Overview

### Vision

**TARTARIA WORLD OF WONDER** is a session-based, open-world restoration RPG where players awaken as a Tartarian descendant in a post-Mud Flood world, excavating buried wonders, tuning atmospheric Aether through sacred-geometry architecture, and restoring a globe-spanning free-energy grid.

### Core Pillars

1. **Restoration** — Excavate and restore Tartarian architecture
2. **Tuning** — Play harmonic sequences and solve cymatic puzzles
3. **Exploration** — Discover hidden wonders across 13 Moons
4. **Combat** — Defend against Reset agents with resonance weapons
5. **Narrative** — Uncover the truth behind the Mud Flood

### Development Philosophy

- **Modular Architecture** — Clean assembly boundaries, minimal coupling
- **Data-Driven Design** — ScriptableObject databases for all content
- **Event-Driven Systems** — GameEvents for cross-system communication
- **Offline-First** — Local persistence, no network dependency
- **Performance Target** — 60 FPS on GTX 1070 (Medium tier)

---

## Project Structure

### Repository Layout

```
TARTARIA_new/
├── Assets/
│   └── _Project/
│       ├── Prefabs/          # Game objects, buildings, characters
│       ├── Scenes/           # Unity scenes (Boot, Echohaven, Moons 1-13)
│       ├── ScriptableObjects/# Data assets (quests, items, crafting)
│       ├── Scripts/          # C# source code (22 assemblies)
│       ├── Audio/            # Music, SFX, VO
│       ├── Materials/        # Shaders, textures
│       ├── Models/           # 3D meshes
│       └── VFX/              # Visual effects (particle systems)
├── docs/                     # Game design documents (30+ GDD files)
├── Build/                    # Standalone builds (.exe output)
├── Logs/                     # Unity logs, build reports
├── ProjectSettings/          # Unity project configuration
├── Packages/                 # Unity Package Manager dependencies
└── UserSettings/             # Per-user Unity settings (git-ignored)
```

### Script Organization (22 Assemblies)

#### Core Runtime (11 assemblies)
1. **Tartaria.Core** — Bootstrap, ServiceLocator, GameEvents, interfaces
2. **Tartaria.Data** — Quest/Item/Crafting databases, validation
3. **Tartaria.Gameplay** — Player systems, combat, abilities, crafting
4. **Tartaria.Integration** — Moon content, quests, dialogue, companions
5. **Tartaria.UI** — HUD, menus, overlays, accessibility
6. **Tartaria.Save** — Persistence, serialization, cloud sync
7. **Tartaria.AI** — Enemy behaviors, NPC schedules, companion AI
8. **Tartaria.Audio** — Music, SFX, VO, adaptive audio
9. **Tartaria.Input** — Gamepad, keyboard, haptics (DualSense)
10. **Tartaria.Camera** — Third-person, cinematic, dialogue rigs
11. **Tartaria.World** — Day/night, APV, weather, environment

#### Specialized (6 assemblies)
12. **Tartaria.Save.Serialization** — Binary/JSON/Hybrid serializers
13. **Tartaria.Localization** — Multi-language support
14. **Tartaria.Vendor** — Third-party assets (KayKit, PCSS shaders)
15. **Tartaria.Examples** — GameEvents usage examples

#### Editor Tools (3 assemblies)
16. **Tartaria.Editor** — Data inspectors, generators, wiring tools
17. **Tartaria.Scripts.Editor** — Quest/Item/Dialogue editors
18. **Tartaria.Save.Serialization.Editor** — Serialization benchmarks
19. **Tartaria.Vendor.Editor** — PCSS shader editor tools

#### Testing (3 assemblies)
20. **Tartaria.Tests** — Core test infrastructure
21. **Tartaria.Tests.PlayMode** — Runtime integration tests
22. **Tartaria.Tests.EditMode** — Editor-time unit tests

### Dependency Rules

**Strict one-way dependencies:**

```
Core (foundation)
  ↓
Data (definitions)
  ↓
Gameplay (systems)
  ↓
Integration (composition) ← UI, AI (parallel)
```

**Forbidden:**
- ❌ Data cannot reference Gameplay
- ❌ Core cannot reference Data or Gameplay
- ❌ Circular dependencies between any assemblies

**See:** [ASSEMBLY_DEPENDENCY_GRAPH.md](ASSEMBLY_DEPENDENCY_GRAPH.md) for full validation matrix.

---

## Assembly Architecture

### Core Assembly — Foundation Layer

**Purpose:** Shared interfaces, enums, events, validation infrastructure  
**Namespace:** `Tartaria.Core`  
**Dependencies:** Unity.Entities, Unity.Burst, Unity.Collections, Tartaria.Localization

#### Key Components

**ServiceLocator Pattern:**
```csharp
public static class ServiceLocator
{
    public static IQuestService Quest { get; set; }
    public static ISaveService Save { get; set; }
    public static IAudioService Audio { get; set; }
}
```

**GameEvents System:**
```csharp
public static class GameEvents
{
    public static event Action<BuildingRestoredEventArgs> OnBuildingRestoredTyped;
    public static event Action<QuestStatusChangedEventArgs> OnQuestStatusChanged;
    public static event Action<EnemyKilledEventArgs> OnEnemyKilled;
    // ... 40+ decoupled events
}
```

**Global Enums:**
- `GameplayEnums.cs` — QuestStatus, ItemRarity, EnemyType
- `QuestEnums.cs` — QuestCategory, ObjectiveType
- `SkillEnums.cs` — SkillCategory, SkillTier

**Validation:**
```csharp
public interface IValidatable
{
    ValidationResult Validate();
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
}
```

### Data Assembly — Definitions Layer

**Purpose:** ScriptableObject databases, query systems  
**Namespace:** `Tartaria.Data`  
**Dependencies:** Tartaria.Core, Tartaria.Localization

#### Key ScriptableObjects

**QuestDefinition:**
```csharp
[CreateAssetMenu(menuName = "Tartaria/Quest Definition")]
public class QuestDefinition : ScriptableObject, IValidatable
{
    public string questId;
    public string questName;
    public QuestCategory category;
    public string[] prerequisites;
    public ObjectiveData[] objectives;
    public RewardData rewards;
}
```

**ItemData:**
```csharp
[CreateAssetMenu(menuName = "Tartaria/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public ItemRarity rarity;
    public Sprite icon;
    public int maxStackSize;
}
```

**CraftingRecipeData:**
```csharp
[CreateAssetMenu(menuName = "Tartaria/Crafting Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public ItemData outputItem;
    public int outputQuantity;
    public IngredientData[] ingredients;
    public float craftingTime;
}
```

#### Query Systems

**QuestDatabase:**
```csharp
public class QuestDatabase : ScriptableObject
{
    [SerializeField] QuestDefinition[] quests;
    
    public QuestDefinition GetQuest(string questId);
    public List<string> GetAllQuestIds();
    public List<QuestDefinition> GetQuestsByCategory(QuestCategory category);
}
```

**ItemRegistry:**
```csharp
public static class ItemRegistry
{
    public static ItemData GetItem(string itemId);
    public static List<ItemData> GetItemsByRarity(ItemRarity rarity);
}
```

### Gameplay Assembly — Systems Layer

**Purpose:** Player mechanics, combat, progression, crafting  
**Namespace:** `Tartaria.Gameplay`  
**Dependencies:** Tartaria.Core, Tartaria.Data, Tartaria.Input, Tartaria.Audio

#### Key Systems

**PlayerProgression:**
```csharp
public class PlayerProgression : MonoBehaviour
{
    public int CurrentLevel { get; private set; }
    public int CurrentXP { get; private set; }
    public int XPToNextLevel { get; private set; }
    
    public void GainXP(int amount);
    public void LevelUp();
}
```

**PlayerCombat:**
```csharp
public class PlayerCombat : MonoBehaviour
{
    public void Attack();
    public void Block();
    public void Dodge();
    public void UseAbility(string abilityId);
}
```

**CraftingSystem:**
```csharp
public class CraftingSystem : MonoBehaviour
{
    public bool CanCraft(CraftingRecipeData recipe);
    public void Craft(CraftingRecipeData recipe);
}
```

### Integration Assembly — Composition Layer

**Purpose:** Moon content, quests, dialogue, companions, NPCs  
**Namespace:** `Tartaria.Integration`  
**Dependencies:** Tartaria.Core, Tartaria.Data, Tartaria.Gameplay

#### Key Managers

**QuestManager:**
```csharp
public class QuestManager : MonoBehaviour, IQuestProvider, IQuestService
{
    public event Action<string, QuestStatus> OnQuestStatusChanged;
    
    public void ActivateQuest(string questId);
    public void CompleteQuest(string questId);
    public QuestState GetQuestState(string questId);
}
```

**DialogueManager:**
```csharp
public class DialogueManager : MonoBehaviour
{
    public void PlayContextDialogue(string context);
    public void PlayLineById(string lineId);
    public bool IsPlaying { get; }
}
```

**MoonContentSpawner** (example — Moon2ContentSpawner):
```csharp
public class Moon2ContentSpawner : MonoBehaviour
{
    void Start()
    {
        SpawnCassianNPC();
        SpawnDissonanceCrystals(12);
        WireFountainClimax();
    }
}
```

### UI Assembly — Presentation Layer

**Purpose:** HUD, menus, overlays, accessibility  
**Namespace:** `Tartaria.UI`  
**Dependencies:** Tartaria.Core, Tartaria.Data, Tartaria.Gameplay

#### Key Components

**HUDController:**
```csharp
public class HUDController : MonoBehaviour
{
    public void ShowObjective(string text);
    public void ShowDialogue(string characterName, string line);
    public void ShowBanner(string text);
}
```

**QuestLogUI:**
```csharp
public class QuestLogUI : MonoBehaviour
{
    public void Refresh();
    public void ShowQuestDetails(string questId);
}
```

### Save Assembly — Persistence Layer

**Purpose:** Save/load, serialization, cloud sync  
**Namespace:** `Tartaria.Save`  
**Dependencies:** Tartaria.Core

**See:** [API_REFERENCE.md](API_REFERENCE.md) for complete SaveManager API.

---

## Code Conventions

### Naming Conventions

**C# Style:**
```csharp
// Classes, structs, enums — PascalCase
public class QuestManager { }
public struct QuestState { }
public enum QuestStatus { Active, Completed, Failed }

// Public fields, properties, methods — PascalCase
public int CurrentLevel { get; private set; }
public void ActivateQuest(string questId) { }

// Private fields — _camelCase with leading underscore
private int _currentXP;
private readonly Dictionary<string, QuestState> _questStates = new();

// Constants — PascalCase
public const string DefaultQuestId = "moon1_main";

// Parameters, local variables — camelCase
public void GainXP(int amount)
{
    int newTotal = _currentXP + amount;
}
```

**Unity Specifics:**
```csharp
// SerializeField — camelCase with lowercase first letter
[SerializeField] float autoSaveIntervalSeconds = 10f;
[SerializeField] QuestDatabase questDatabaseAsset;

// Tooltips — sentence case
[Tooltip("Duration in seconds between auto-saves")]
[SerializeField] float autoSaveIntervalSeconds = 10f;
```

### File Organization

**One class per file:**
```
PlayerProgression.cs — contains only PlayerProgression class
QuestManager.cs — contains only QuestManager class
```

**Exceptions allowed:**
- Small helper structs (e.g., `QuestState` in `QuestManager.cs`)
- Nested private classes
- EventArgs classes alongside events

**File header:**
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Data;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Brief description of class purpose.
    /// Design principles, key responsibilities.
    /// </summary>
    public class PlayerProgression : MonoBehaviour
    {
        // Implementation
    }
}
```

### Documentation Standards

**XML documentation for public APIs:**
```csharp
/// <summary>
/// Activates a quest by ID, making it trackable in the quest log.
/// </summary>
/// <param name="questId">Unique quest identifier from QuestDatabase</param>
/// <returns>True if quest was activated successfully, false if already active or not found</returns>
public bool ActivateQuest(string questId)
{
    // Implementation
}
```

**Inline comments for complex logic:**
```csharp
// Phase 1: Validate prerequisites
foreach (var prereqId in quest.prerequisites)
{
    if (!IsQuestCompleted(prereqId))
        return false; // Block activation if prereq not met
}
```

### Unity Best Practices

**Component lifecycle:**
```csharp
// Singleton pattern with DontDestroyOnLoad
void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    transform.SetParent(null);
    DontDestroyOnLoad(gameObject);
}

void OnDestroy()
{
    if (Instance == this) Instance = null;
    
    // CRITICAL: Always unsubscribe from events to prevent memory leaks
    GameEvents.OnBuildingRestored -= HandleBuildingRestored;
}
```

**Coroutine management:**
```csharp
private Coroutine _activeCoroutine;

void StartMyCoroutine()
{
    // Stop existing coroutine before starting new one
    if (_activeCoroutine != null)
        StopCoroutine(_activeCoroutine);
    
    _activeCoroutine = StartCoroutine(MyCoroutineMethod());
}

void OnDestroy()
{
    // Stop all coroutines on destruction
    StopAllCoroutines();
    _activeCoroutine = null;
}
```

**FindObjectOfType (Unity 6 API):**
```csharp
// Unity 6 — use FindFirstObjectByType (deprecated: FindObjectOfType)
QuestManager questManager = FindFirstObjectByType<QuestManager>();

// For multiple objects — use FindObjectsByType
EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
```

### Performance Guidelines

**Avoid Update-heavy logic:**
```csharp
// ❌ BAD — runs every frame
void Update()
{
    if (QuestManager.Instance.IsQuestActive("moon1_main"))
    {
        // Expensive check every frame
    }
}

// ✅ GOOD — cache result, subscribe to events
bool _isMoon1Active;

void Start()
{
    _isMoon1Active = QuestManager.Instance.IsQuestActive("moon1_main");
    GameEvents.OnQuestStatusChanged += HandleQuestStatusChanged;
}

void HandleQuestStatusChanged(QuestStatusChangedEventArgs e)
{
    if (e.questId == "moon1_main")
        _isMoon1Active = (e.newStatus == QuestStatus.Active);
}
```

**Object pooling for projectiles:**
```csharp
// Use object pooling for frequently spawned objects
ObjectPool<ArrowProjectile> _arrowPool;

void Awake()
{
    _arrowPool = new ObjectPool<ArrowProjectile>(
        createFunc: () => Instantiate(arrowPrefab),
        actionOnGet: (arrow) => arrow.gameObject.SetActive(true),
        actionOnRelease: (arrow) => arrow.gameObject.SetActive(false),
        actionOnDestroy: (arrow) => Destroy(arrow.gameObject),
        defaultCapacity: 20,
        maxSize: 100
    );
}
```

**String concatenation:**
```csharp
// ❌ BAD — creates garbage
string message = "Quest " + questName + " completed!";

// ✅ GOOD — uses string interpolation (minimal garbage)
string message = $"Quest {questName} completed!";

// ✅ BEST — use StringBuilder for complex/repeated concatenation
StringBuilder sb = new StringBuilder();
sb.Append("Quest ");
sb.Append(questName);
sb.Append(" completed!");
string message = sb.ToString();
```

---

## Quest Creation Workflow

### Step 1: Design Quest Structure

**Use Quest Template (docs/quest_template.md):**

```yaml
questId: moon2_dissonance_purge
questName: Crystal Clarity
category: MainStory
moon: 2
description: Purge 12 dissonance crystals to restore the Lunar Cathedral
prerequisites:
  - moon1_main_complete
objectives:
  - type: Interact
    description: Purge dissonance crystals (0/12)
    targetCount: 12
    targetIds: [crystal_01, crystal_02, ..., crystal_12]
rewards:
  xp: 500
  resonanceShards: 100
  items:
    - itemId: harmonic_tuner_tier2
      quantity: 1
```

### Step 2: Create QuestDefinition Asset

**Via Unity Editor:**

1. Right-click in `Assets/_Project/ScriptableObjects/Quests/`
2. Create → Tartaria → Quest Definition
3. Name: `Moon2_DissonancePurge.asset`
4. Fill in fields:
   - Quest ID: `moon2_dissonance_purge`
   - Quest Name: `Crystal Clarity`
   - Category: `MainStory`
   - Prerequisites: Add `moon1_main_complete`
5. Add objectives:
   - Type: `Interact`
   - Description: `Purge dissonance crystals`
   - Target Count: `12`
6. Set rewards:
   - XP: `500`
   - Resonance Shards: `100`
   - Item: `harmonic_tuner_tier2`

**Via C# Builder (automated):**

```csharp
using Tartaria.Data;
using Tartaria.Editor;

[MenuItem("Tartaria/Generate Moon 2 Quests")]
static void GenerateMoon2Quests()
{
    var quest = QuestBuilder.Create("moon2_dissonance_purge")
        .WithName("Crystal Clarity")
        .WithCategory(QuestCategory.MainStory)
        .WithPrerequisite("moon1_main_complete")
        .AddObjective(ObjectiveType.Interact, "Purge dissonance crystals", 12)
        .WithReward(xp: 500, resonanceShards: 100)
        .WithItemReward("harmonic_tuner_tier2", 1)
        .SaveToAssets("Assets/_Project/ScriptableObjects/Quests/");
}
```

### Step 3: Add Quest to QuestDatabase

1. Open `Assets/_Project/ScriptableObjects/Databases/QuestDatabase.asset`
2. Expand `Quests` array
3. Add new element: drag `Moon2_DissonancePurge.asset` into slot
4. Click **Validate Database** button (Inspector)
5. Fix any errors reported (missing prerequisites, duplicate IDs, etc.)

### Step 4: Implement Quest Activation Logic

**In Moon content spawner:**

```csharp
// Moon2ContentSpawner.cs
void Start()
{
    // Activate Moon 2 main quest when Moon 1 complete
    if (QuestManager.Instance.IsQuestCompleted("moon1_main_complete"))
    {
        QuestManager.Instance.ActivateQuest("moon2_dissonance_purge");
    }
}
```

**In interactive object:**

```csharp
// DissonanceCrystal.cs
public class DissonanceCrystal : MonoBehaviour, IInteractable
{
    [SerializeField] string questObjectiveId = "crystal_01";
    
    public void Interact(GameObject player)
    {
        // Play purge VFX + SFX
        GameEvents.FireBuildingRestored(questObjectiveId);
        
        // Progress quest objective
        QuestManager.Instance.ProgressObjective("moon2_dissonance_purge", questObjectiveId);
        
        Destroy(gameObject);
    }
}
```

### Step 5: Add Dialogue Beats

**See:** [Dialogue Authoring Guide](#dialogue-authoring-guide) below.

### Step 6: Test Quest Flow

**Automated test (EditMode):**

```csharp
[Test]
public void Moon2Quest_ActivatesAfterMoon1Complete()
{
    // Setup
    QuestManager.Instance.CompleteQuest("moon1_main_complete");
    
    // Act
    QuestManager.Instance.ActivateQuest("moon2_dissonance_purge");
    
    // Assert
    Assert.IsTrue(QuestManager.Instance.IsQuestActive("moon2_dissonance_purge"));
}
```

**Manual playtest:**

1. Load Echohaven scene
2. Complete Moon 1 quests (or use cheat: `QuestManager.Instance.CompleteQuest("moon1_main_complete")`)
3. Verify Moon 2 quest appears in Quest Log
4. Interact with all 12 dissonance crystals
5. Verify quest completes, rewards granted, dialogue triggers

---

## Dialogue Authoring Guide

### Dialogue System Overview

**DialogueManager** loads context-sensitive dialogue lines and plays them via:
- **Context-based triggers** — `PlayContextDialogue("discovery")` picks a random appropriate line
- **ID-based playback** — `PlayLineById("milo_moon1_excited")` plays specific line

**Dialogue contexts:**
- `discovery` — Player discovers new building/zone
- `tuning_start` — Player begins tuning puzzle
- `tuning_success` — Tuning puzzle completed
- `tuning_fail` — Tuning puzzle failed
- `restoration` — Building restoration complete
- `combat_start` — Combat encounter begins
- `combat_victory` — Enemy defeated
- `exploration_idle` — Idle exploration chatter
- `aether_wake` — Aether energy detected
- `zone_shift` — Transition between zones
- `zone_complete` — Zone fully restored

### Character Voice Guidelines

**See:** [CHARACTER_VOICE_GUIDE.md](CHARACTER_VOICE_GUIDE.md) for full voice profiles.

**Quick Reference:**

**Milo (Companion Dog) — Witty, Sensory:**
- Exclamation-heavy: "I SMELL something incredible!"
- Physical tells: *tail wagging*, *barking*, *howling*
- Sensory language: "This crystal HUMS like a thousand bells!"

**Lirael (Echo Healer) — Empathetic, Musical:**
- Crystal/frequency metaphors: "Your resonance is... beautiful."
- Gentle qualifiers: "Perhaps...", "I wonder if...", "May I suggest..."
- Physical tells: *flickers*, *tears*, *voice trembles*

**Cassian (Architect Spy) — Analytical, Scholarly:**
- Measured, cautious: "Fascinating. This structure predates the Flood by centuries."
- Redemption arc: evolves from detached to committed
- Professional detachment → emotional investment

### Adding New Dialogue Lines

**Step 1: Choose context or ID approach**

**Context approach (flexible):**
```csharp
// Trigger in gameplay code
GameEvents.OnBuildingRestored += (buildingId) =>
{
    DialogueManager.Instance.PlayContextDialogue("restoration");
};
```

**ID approach (precise):**
```csharp
// Trigger specific line
void OnMoonComplete()
{
    DialogueManager.Instance.PlayLineById("lirael_moon2_fountain_climax");
}
```

**Step 2: Add lines to DialogueManager.BuildDatabase()**

**In `DialogueManager.cs` → `BuildDatabase()` method:**

```csharp
void BuildDatabase()
{
    // Moon 2 — Lunar Cathedral dialogue
    
    // Discovery context
    AddLine("lirael_moon2_discovery_fracture", "Lirael", 
        "*flickers* I can't hold form here. The dissonance... it's too strong!",
        "discovery", oneShot: true, volume: 0.9f);
    
    AddLine("cassian_moon2_discovery_beckon", "Cassian",
        "*gestures toward the fountain* The Lunar Cathedral. Follow me—carefully.",
        "discovery", oneShot: true);
    
    // Restoration context
    AddLine("moon2_restoration_tuning_success", "Lirael",
        "*solidifies slightly* Yes! The frequency is stabilizing!",
        "restoration", oneShot: false);
    
    // Combat context
    AddLine("moon2_conflict_first_golem", "Milo",
        "*growling* These crystal golems don't like us messing with their veins!",
        "combat_start", oneShot: true);
    
    // Climax (ID-based, not context)
    AddLine("lirael_moon2_fountain_climax", "Lirael",
        "*tears streaming* I remember now. I was here when the cathedral fell.",
        null, oneShot: true, volume: 1.0f);
}
```

**Step 3: Trigger dialogue in gameplay**

**Context-based:**
```csharp
// In Moon2ContentSpawner.cs
void OnFirstCrystalPurged()
{
    DialogueManager.Instance.PlayContextDialogue("combat_victory");
}
```

**ID-based:**
```csharp
// In Moon2FountainClimax.cs
void TriggerClimaxCutscene()
{
    DialogueManager.Instance.PlayLineById("lirael_moon2_fountain_climax");
    // ... cinematic sequence
}
```

### Dialogue Testing

**Manual test:**
1. Load scene with dialogue
2. Trigger context (e.g., restore building)
3. Verify dialogue appears in HUD subtitle area
4. Check character name, line text, voice (if VO implemented)

**Automated test:**
```csharp
[Test]
public void DialogueManager_PlaysContextDialogue()
{
    // Arrange
    DialogueManager dm = new GameObject().AddComponent<DialogueManager>();
    
    // Act
    dm.PlayContextDialogue("discovery");
    
    // Assert
    Assert.IsTrue(dm.IsPlaying, "Dialogue should be playing");
}
```

---

## Testing Procedures

### Unity Test Framework

**PlayMode tests** — Runtime integration tests (in-play testing)  
**EditMode tests** — Unit tests (no Unity scene required)

**Test assemblies:**
- `Tartaria.Tests` — Base test infrastructure
- `Tartaria.Tests.PlayMode` — Runtime tests (requires scene load)
- `Tartaria.Tests.EditMode` — Editor tests (fast, no scene)

### Running Tests

**Via Unity Editor:**

1. Window → General → Test Runner
2. Select **PlayMode** or **EditMode** tab
3. Click **Run All** or select specific test
4. View results in Test Runner panel

**Via PowerShell (automated):**

```powershell
# Run all PlayMode tests
.\run-automated-tests.ps1 -SceneName Echohaven

# Run specific test category
.\run-automated-tests.ps1 -Category Moon1

# Run EditMode tests only
.\run-automated-tests.ps1 -EditModeOnly
```

### Writing Tests

**PlayMode test example:**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Tartaria.Integration;

namespace Tartaria.Tests.PlayMode
{
    public class QuestSystemTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator QuestActivation_FiresEvent()
        {
            // Arrange
            QuestManager qm = FindFirstObjectByType<QuestManager>();
            bool eventFired = false;
            GameEvents.OnQuestStatusChanged += (e) => eventFired = true;
            
            // Act
            qm.ActivateQuest("moon1_main");
            yield return null; // Wait one frame
            
            // Assert
            Assert.IsTrue(eventFired, "Quest activation should fire event");
        }
    }
}
```

**EditMode test example:**

```csharp
using NUnit.Framework;
using Tartaria.Data;

namespace Tartaria.Tests.EditMode
{
    public class QuestDatabaseTests
    {
        [Test]
        public void QuestDatabase_GetQuest_ReturnsValidQuest()
        {
            // Arrange
            QuestDatabase db = CreateTestDatabase();
            
            // Act
            QuestDefinition quest = db.GetQuest("moon1_main");
            
            // Assert
            Assert.IsNotNull(quest);
            Assert.AreEqual("moon1_main", quest.questId);
        }
        
        QuestDatabase CreateTestDatabase()
        {
            // Create test database with sample quests
            var db = ScriptableObject.CreateInstance<QuestDatabase>();
            // ... populate test data
            return db;
        }
    }
}
```

### Test Coverage Goals

**Minimum coverage per system:**

- **Quest System:** 80% (activate, complete, prerequisite validation)
- **Dialogue System:** 70% (context playback, line selection)
- **Save System:** 90% (save, load, corruption recovery)
- **Combat System:** 75% (damage, death, enemy AI)
- **Inventory System:** 80% (add, remove, crafting)

**Current coverage:**

- Moon 1-5 integration tests: 19 tests (all GREEN)
- Unit tests: 42 tests across Core, Data, Gameplay
- Total coverage: ~65% (target 80% by v1.0)

### Performance Testing

**Run performance gates:**

```powershell
.\run-moon-tests.ps1 -PerformanceGates
```

**Validates:**
- Avg FPS ≥ 52 (Medium tier, GTX 1070)
- 1% Low FPS ≥ 28
- Peak RAM ≤ 3.6 GB
- Scene load time ≤ 8 seconds

**Manual profiling:**

1. Open Unity Profiler (Window → Analysis → Profiler)
2. Enable Deep Profiling (slow, use only for specific issues)
3. Play scene
4. Look for:
   - CPU spikes > 16ms (60 FPS threshold)
   - GC.Alloc > 10 MB/frame (memory pressure)
   - Draw calls > 1500 (GPU bottleneck)

---

## Performance Guidelines

### Target Specifications

**Medium Tier (GTX 1070 / 8 GB RAM):**
- 60 FPS average
- 28 FPS 1% low
- 3.6 GB peak RAM
- 8 second scene load

### Optimization Checklist

**Asset Optimization:**
- [ ] Textures: 2K max (1K for UI), BC7 compression
- [ ] Meshes: < 50K tris for characters, < 10K for props
- [ ] Audio: Ogg Vorbis, 128 kbps for music, 64 kbps for SFX
- [ ] VFX: Particle count < 500 per system, GPU instancing enabled

**Code Optimization:**
- [ ] No FindObjectOfType in Update() loops
- [ ] Object pooling for projectiles/particles
- [ ] Event-driven over polling (GameEvents preferred)
- [ ] Avoid string concatenation in hot paths
- [ ] Cache component references in Awake/Start

**Scene Optimization:**
- [ ] Occlusion culling enabled (large environments)
- [ ] LOD groups on distant objects
- [ ] Static batching for non-moving geometry
- [ ] GPU instancing for repeated meshes

**Unity Project Settings:**
- [ ] URP Asset: Medium quality tier
- [ ] Shadow Resolution: 1024 (Medium), 2048 (High)
- [ ] MSAA: 4x (Medium), 8x (High)
- [ ] VSync: Disabled (use frame limiter instead)

---

## Git Workflow

### Branch Strategy

**Main branches:**
- `main` — Production-ready code (protected)
- `develop` — Integration branch for features

**Feature branches:**
- `feature/moon3-rail-puzzle` — New features
- `bugfix/save-corruption` — Bug fixes
- `hotfix/crash-on-load` — Emergency fixes for production

### Commit Message Format

**Use conventional commits:**

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat` — New feature
- `fix` — Bug fix
- `docs` — Documentation only
- `style` — Code style (formatting, no logic change)
- `refactor` — Code restructure (no behavior change)
- `perf` — Performance improvement
- `test` — Add/update tests
- `chore` — Build process, dependencies

**Examples:**

```
feat(quest): add Moon 3 Orphan Train main quest

Implemented RailEscortController with 13 rail segments, passenger echo
system, and Lirael backstory reveal.

Closes #42
```

```
fix(save): prevent corruption on Alt-F4 quit

Added emergency save hook on OnApplicationQuit with < 2s timeout.
Double-write + checksum validation ensures no data loss.

Fixes #87
```

### Pull Request Process

**Before creating PR:**

1. Run full test suite: `.\run-automated-tests.ps1`
2. Verify compilation: `CS:0` (zero errors)
3. Run performance gates (if gameplay changes): `.\run-moon-tests.ps1 -PerformanceGates`
4. Update CHANGELOG.md with changes

**PR checklist:**

- [ ] All tests passing (PlayMode + EditMode)
- [ ] Zero compilation errors/warnings
- [ ] Code follows style guide
- [ ] XML docs added for public APIs
- [ ] CHANGELOG.md updated
- [ ] Performance regression check (if applicable)

**PR review criteria:**

- Code quality (readability, maintainability)
- Test coverage (new code has tests)
- Performance impact (no frame drops, GC spikes)
- Assembly dependency rules (no circular deps)

---

## Additional Resources

**Documentation:**
- [API_REFERENCE.md](API_REFERENCE.md) — Complete API documentation
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) — Build and deployment guide
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — Common issues and solutions
- [docs/](docs/) — Game design documents (30+ GDD files)

**Editor Tools:**
- `Menu → Tartaria → One-Click Build & Play` — Automated build pipeline
- `Menu → Tartaria → Generate Quest Assets` — Batch quest creation
- `Menu → Tartaria → Validate All Databases` — Data integrity check

**Scripts:**
- `tartaria-play.ps1` — Build + play automation
- `run-automated-tests.ps1` — Test runner
- `run-moon-tests.ps1` — Moon-specific tests
- `perf-profile.ps1` — Performance profiling

**External:**
- Unity Manual: https://docs.unity3d.com/6000.0/Documentation/Manual/
- URP Documentation: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/
- Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/

---

**Version History:**

- **1.0.0-beta** (May 24, 2026) — Initial comprehensive development guide
