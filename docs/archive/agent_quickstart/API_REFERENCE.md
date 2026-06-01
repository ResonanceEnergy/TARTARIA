# TARTARIA — API Reference

**Version:** 1.0.0-beta  
**Unity Version:** 6000.3.6f1  
**Last Updated:** May 24, 2026

---

## Table of Contents

1. [Core Systems](#core-systems)
2. [QuestManager API](#questmanager-api)
3. [DialogueManager API](#dialoguemanager-api)
4. [SaveManager API](#savemanager-api)
5. [GameEvents Catalog](#gameevents-catalog)
6. [Audio System Reference](#audio-system-reference)
7. [VFX System Reference](#vfx-system-reference)
8. [Data Architecture](#data-architecture)
9. [Service Locator Pattern](#service-locator-pattern)

---

## Core Systems

### ServiceLocator

**Purpose:** Global service access point for cross-assembly communication  
**Namespace:** `Tartaria.Core`  
**Location:** `Assets/_Project/Scripts/Core/ServiceLocator.cs`

#### API

```csharp
public static class ServiceLocator
{
    // Quest system access
    public static IQuestService Quest { get; set; }
    
    // Save system access
    public static ISaveService Save { get; set; }
    
    // Audio system access
    public static IAudioService Audio { get; set; }
    
    // Input system access
    public static IInputService Input { get; set; }
}
```

#### Usage

```csharp
// Access quest system
ServiceLocator.Quest.ActivateQuest("moon1_main");

// Access save system
ServiceLocator.Save.Save();

// Access audio system
ServiceLocator.Audio.PlaySFX("building_restore");
```

#### Implementation Pattern

```csharp
public class QuestManager : MonoBehaviour, IQuestService
{
    void Awake()
    {
        // Register with ServiceLocator
        ServiceLocator.Quest = this;
    }
    
    void OnDestroy()
    {
        // Unregister from ServiceLocator
        if (ServiceLocator.Quest == this)
            ServiceLocator.Quest = null;
    }
}
```

### GameEvents

**Purpose:** Decoupled event system for cross-assembly communication  
**Namespace:** `Tartaria.Core`  
**Location:** `Assets/_Project/Scripts/Core/GameEvents.cs`

**See:** [GameEvents Catalog](#gameevents-catalog) for complete event list.

---

## QuestManager API

**Purpose:** Quest activation, progression, and completion tracking  
**Namespace:** `Tartaria.Integration`  
**Location:** `Assets/_Project/Scripts/Integration/QuestManager.cs`  
**Interfaces:** `IQuestProvider`, `IQuestService`

### Properties

```csharp
/// <summary>
/// Singleton instance (initialized in Awake)
/// </summary>
public static QuestManager Instance { get; private set; }
```

### Events

```csharp
/// <summary>
/// Raised when any quest changes status (Active → Completed, etc.)
/// </summary>
public event Action<string, QuestStatus> OnQuestStatusChanged;

/// <summary>
/// Raised when quest objective progress updates (e.g., "Collect 5/10 shards")
/// </summary>
public event Action<string, int> OnObjectiveProgressed;
```

### Public Methods

#### ActivateQuest

```csharp
/// <summary>
/// Activates a quest by ID, making it trackable in the quest log.
/// Validates prerequisites before activation.
/// </summary>
/// <param name="questId">Unique quest identifier from QuestDatabase</param>
/// <returns>True if quest was activated successfully, false if already active, missing, or prerequisites not met</returns>
public bool ActivateQuest(string questId);
```

**Example:**
```csharp
bool success = QuestManager.Instance.ActivateQuest("moon1_main");
if (success)
{
    Debug.Log("Moon 1 main quest activated!");
}
else
{
    Debug.LogWarning("Failed to activate quest (check prerequisites)");
}
```

#### CompleteQuest

```csharp
/// <summary>
/// Marks a quest as completed. Awards rewards (XP, items, RS).
/// Fires OnQuestStatusChanged event.
/// </summary>
/// <param name="questId">Unique quest identifier</param>
public void CompleteQuest(string questId);
```

**Example:**
```csharp
QuestManager.Instance.CompleteQuest("moon1_main");
// Quest marked complete, rewards granted, event fired
```

#### FailQuest

```csharp
/// <summary>
/// Marks a quest as failed (optional state for time-limited quests).
/// Fires OnQuestStatusChanged event.
/// </summary>
/// <param name="questId">Unique quest identifier</param>
public void FailQuest(string questId);
```

#### ProgressObjective

```csharp
/// <summary>
/// Increments progress on a specific quest objective.
/// Auto-completes quest when all objectives met.
/// </summary>
/// <param name="questId">Quest containing the objective</param>
/// <param name="objectiveId">Objective identifier (e.g., "building_star_dome")</param>
/// <param name="amount">Amount to increment (default 1)</param>
public void ProgressObjective(string questId, string objectiveId, int amount = 1);
```

**Example:**
```csharp
// Player purges a dissonance crystal
QuestManager.Instance.ProgressObjective("moon2_dissonance_purge", "crystal_01");

// Player collects 5 resonance shards
QuestManager.Instance.ProgressObjective("moon1_shard_collect", "resonance_shards", 5);
```

#### GetQuestState

```csharp
/// <summary>
/// Retrieves current state of a quest (status + objective progress).
/// </summary>
/// <param name="questId">Unique quest identifier</param>
/// <returns>QuestState struct with status and progress arrays, or null if not found</returns>
public QuestState? GetQuestState(string questId);
```

**Example:**
```csharp
QuestState? state = QuestManager.Instance.GetQuestState("moon1_main");
if (state.HasValue)
{
    Debug.Log($"Quest status: {state.Value.status}");
    Debug.Log($"Objective progress: {state.Value.objectiveProgress[0]}/3");
}
```

#### IsQuestActive

```csharp
/// <summary>
/// Checks if a quest is currently active.
/// </summary>
/// <param name="questId">Unique quest identifier</param>
/// <returns>True if quest status is Active</returns>
public bool IsQuestActive(string questId);
```

#### IsQuestCompleted

```csharp
/// <summary>
/// Checks if a quest has been completed.
/// </summary>
/// <param name="questId">Unique quest identifier</param>
/// <returns>True if quest status is Completed</returns>
public bool IsQuestCompleted(string questId);
```

#### GetActiveQuests

```csharp
/// <summary>
/// Returns list of all active quest IDs.
/// Cached internally, only rebuilds when quest status changes.
/// </summary>
/// <returns>Read-only list of quest IDs with status Active</returns>
public IReadOnlyList<string> GetActiveQuests();
```

#### GetCompletedQuests

```csharp
/// <summary>
/// Returns list of all completed quest IDs.
/// Cached internally, only rebuilds when quest status changes.
/// </summary>
/// <returns>Read-only list of quest IDs with status Completed</returns>
public IReadOnlyList<string> GetCompletedQuests();
```

### Data Structures

#### QuestState

```csharp
public struct QuestState
{
    /// <summary>Quest status (Active, Completed, Failed, Locked)</summary>
    public QuestStatus status;
    
    /// <summary>Progress for each objective (parallel array to quest.objectives)</summary>
    public int[] objectiveProgress;
}
```

#### QuestStatus Enum

```csharp
public enum QuestStatus
{
    Locked = 0,      // Prerequisites not met
    Active = 1,      // In progress
    Completed = 2,   // Successfully completed
    Failed = 3       // Failed (time-limited or critical objective missed)
}
```

### Integration with GameEvents

**QuestManager automatically fires events:**

```csharp
// Quest activation
GameEvents.FireQuestStatusChanged(new QuestStatusChangedEventArgs
{
    questId = "moon1_main",
    oldStatus = QuestStatus.Locked,
    newStatus = QuestStatus.Active
});

// Objective progress
GameEvents.FireQuestObjectiveProgressed(new QuestObjectiveProgressedEventArgs
{
    questId = "moon1_main",
    objectiveIndex = 0,
    currentProgress = 2,
    targetProgress = 3
});
```

**Subscribers can listen:**

```csharp
void Start()
{
    GameEvents.OnQuestStatusChanged += HandleQuestStatusChanged;
}

void HandleQuestStatusChanged(QuestStatusChangedEventArgs e)
{
    if (e.newStatus == QuestStatus.Completed)
    {
        ShowQuestCompleteBanner(e.questId);
    }
}

void OnDestroy()
{
    GameEvents.OnQuestStatusChanged -= HandleQuestStatusChanged;
}
```

---

## DialogueManager API

**Purpose:** Context-sensitive dialogue playback  
**Namespace:** `Tartaria.Integration`  
**Location:** `Assets/_Project/Scripts/Integration/DialogueManager.cs`

### Properties

```csharp
/// <summary>
/// Singleton instance (initialized in Awake)
/// </summary>
public static DialogueManager Instance { get; private set; }

/// <summary>
/// True while a dialogue line is displayed on screen
/// </summary>
public bool IsPlaying { get; }

/// <summary>
/// Duration of the currently displayed line (autoCloseDelay)
/// </summary>
public float CurrentLineDuration { get; }
```

### Public Methods

#### PlayContextDialogue

```csharp
/// <summary>
/// Plays a random dialogue line appropriate for the given context.
/// Respects minTimeBetweenLines cooldown.
/// </summary>
/// <param name="context">
/// Dialogue context:
///   - "discovery" — Player discovers new building/zone
///   - "tuning_start" — Player begins tuning puzzle
///   - "tuning_success" — Tuning puzzle completed
///   - "tuning_fail" — Tuning puzzle failed
///   - "restoration" — Building restoration complete
///   - "combat_start" — Combat encounter begins
///   - "combat_victory" — Enemy defeated
///   - "exploration_idle" — Idle exploration chatter
///   - "aether_wake" — Aether energy detected
///   - "zone_shift" — Transition between zones
///   - "zone_complete" — Zone fully restored
/// </param>
public void PlayContextDialogue(string context);
```

**Example:**
```csharp
// Trigger on building discovery
void OnBuildingDiscovered()
{
    DialogueManager.Instance.PlayContextDialogue("discovery");
    // Milo might say: "I SMELL something incredible beneath us!"
}

// Trigger on combat start
void OnCombatStart()
{
    DialogueManager.Instance.PlayContextDialogue("combat_start");
    // Lirael might say: "Watch out! Corruption ahead!"
}
```

#### PlayLineById

```csharp
/// <summary>
/// Plays a specific dialogue line by ID.
/// Use for scripted narrative beats or cinematics.
/// </summary>
/// <param name="lineId">Unique line identifier from BuildDatabase()</param>
public void PlayLineById(string lineId);

/// <summary>
/// Plays a specific dialogue line by ID at a given volume.
/// </summary>
/// <param name="lineId">Unique line identifier</param>
/// <param name="volume">Volume multiplier (0.0 to 1.0)</param>
public void PlayLineById(string lineId, float volume);
```

**Example:**
```csharp
// Trigger specific line in Moon 2 climax cutscene
void TriggerFountainClimaxDialogue()
{
    DialogueManager.Instance.PlayLineById("lirael_moon2_fountain_climax");
    // Lirael: "*tears streaming* I remember now. I was here when the cathedral fell."
}

// Whispered line (lower volume)
DialogueManager.Instance.PlayLineById("cassian_confession_trust", 0.7f);
```

### Dialogue Contexts Reference

| Context | When to Use | Example Lines |
|---------|-------------|---------------|
| `discovery` | Player finds new building, zone, or secret | "What IS that?!" (Milo), "The architecture... it's intact!" (Cassian) |
| `tuning_start` | Player begins tuning puzzle (organ, cymatic, etc.) | "Let's tune this crystal!" (Player implied), "Focus on the frequency..." (Lirael) |
| `tuning_success` | Tuning puzzle solved | "YES! Perfect harmony!" (Milo), "The frequency is stabilizing..." (Lirael) |
| `tuning_fail` | Tuning puzzle failed (incorrect sequence) | "Ouch! That hurt my ears!" (Milo), "Try again, slower this time." (Cassian) |
| `restoration` | Building restoration complete | "It's ALIVE!" (Milo), "Another star fort reclaimed." (Cassian) |
| `combat_start` | Combat encounter triggered | "*growling* These golems don't like us!" (Milo), "Defend yourself!" (Lirael) |
| `combat_victory` | Enemy defeated | "Got 'em!" (Milo), "Well fought." (Cassian) |
| `exploration_idle` | Player exploring, no active objective | "I wonder what's over that ridge..." (Milo), "*humming a tune*" (Lirael) |
| `aether_wake` | Player uses Aether Vision or detects energy | "I can FEEL it!" (Milo), "The ley lines are waking..." (Lirael) |
| `zone_shift` | Transition between zones (Echohaven → Lunar Cathedral) | "New zone, new mysteries!" (Milo), "The air feels different here." (Cassian) |
| `zone_complete` | Zone fully restored (all buildings + boss defeated) | "We did it!" (Milo), "This zone pulses with life again." (Lirael) |

### Data Structures

#### DialogueLine

```csharp
public class DialogueLine
{
    /// <summary>Unique line identifier</summary>
    public string id;
    
    /// <summary>Character name (Milo, Lirael, Cassian, etc.)</summary>
    public string character;
    
    /// <summary>Dialogue text (displayed in HUD subtitle area)</summary>
    public string text;
    
    /// <summary>Context for random selection (discovery, combat_start, etc.)</summary>
    public string context;
    
    /// <summary>If true, line can only be played once per playthrough</summary>
    public bool oneShot;
    
    /// <summary>Volume multiplier (0.0 to 1.0, default 1.0)</summary>
    public float volume;
}
```

### Adding New Dialogue

**Edit `DialogueManager.cs` → `BuildDatabase()` method:**

```csharp
void BuildDatabase()
{
    // Moon 1 — Echohaven dialogue
    AddLine("milo_moon1_excited", "Milo",
        "*tail wagging* I SMELL something incredible beneath us!",
        "discovery", oneShot: false, volume: 1.0f);
    
    AddLine("lirael_moon1_tuning_success", "Lirael",
        "*glows brighter* Yes! The frequency is perfect!",
        "tuning_success", oneShot: false, volume: 0.9f);
    
    // Moon 2 — Lunar Cathedral dialogue
    AddLine("cassian_moon2_fountain_climax", "Cassian",
        "*exhales heavily* My orders were to document your work and report back. But... I can't. I won't.",
        null, oneShot: true, volume: 1.0f);
}
```

**Trigger in gameplay:**

```csharp
// Context-based
GameEvents.OnBuildingRestored += (buildingId) =>
{
    DialogueManager.Instance.PlayContextDialogue("restoration");
};

// ID-based
void TriggerClimaxCutscene()
{
    DialogueManager.Instance.PlayLineById("cassian_moon2_fountain_climax");
}
```

---

## SaveManager API

**Purpose:** Save/load game state, auto-save, corruption recovery  
**Namespace:** `Tartaria.Save`  
**Location:** `Assets/_Project/Scripts/Save/SaveManager.cs`  
**Interface:** `ISaveService`

### Properties

```csharp
/// <summary>
/// Singleton instance (auto-bootstrapped via RuntimeInitializeOnLoadMethod)
/// </summary>
public static SaveManager Instance { get; private set; }

/// <summary>
/// Current save data (mutable, call Save() to persist)
/// </summary>
public SaveData CurrentSave { get; }
```

### Events

```csharp
/// <summary>
/// Raised before save data is serialized.
/// Subscribers can update CurrentSave fields before persistence.
/// </summary>
public event Action<SaveData> OnBeforeSave;

/// <summary>
/// Raised after save data is deserialized.
/// Subscribers can restore state from loaded SaveData.
/// </summary>
public event Action<SaveData> OnAfterLoad;
```

### Public Methods

#### Save

```csharp
/// <summary>
/// Serializes CurrentSave to disk.
/// Uses double-write + checksum for corruption protection.
/// Fires OnBeforeSave event before serialization.
/// </summary>
public void Save();
```

**Example:**
```csharp
// Manual save (F5 quicksave)
void OnQuicksaveInput()
{
    SaveManager.Instance.Save();
    ShowToast("Game Saved");
}

// Auto-save trigger (building restored)
GameEvents.OnBuildingRestored += (buildingId) =>
{
    SaveManager.Instance.MarkDirty();
    // Auto-save will trigger within 10 seconds
};
```

#### Load

```csharp
/// <summary>
/// Deserializes save data from disk.
/// Validates checksum, falls back to backup if primary corrupted.
/// Fires OnAfterLoad event after deserialization.
/// </summary>
public void Load();
```

**Example:**
```csharp
// Load from "Continue" button
void OnContinueButton()
{
    SaveManager.Instance.Load();
    SceneManager.LoadScene("Echohaven");
}
```

#### LoadOrCreate

```csharp
/// <summary>
/// Loads save if exists, otherwise creates new save with defaults.
/// Called automatically in Start().
/// </summary>
public void LoadOrCreate();
```

#### MarkDirty

```csharp
/// <summary>
/// Flags save data as dirty (pending save).
/// Auto-save will trigger within autoSaveIntervalSeconds (default 10s).
/// </summary>
public void MarkDirty();
```

**Example:**
```csharp
// Mark dirty when player gains XP
void GainXP(int amount)
{
    CurrentSave.playerXP += amount;
    SaveManager.Instance.MarkDirty();
    // Auto-save within 10 seconds
}
```

#### QuickSave / QuickLoad

```csharp
/// <summary>
/// Immediate save (hotkey: F5)
/// </summary>
public void QuickSave();

/// <summary>
/// Immediate load (hotkey: F9)
/// </summary>
public void QuickLoad();
```

#### SwitchToSlot

```csharp
/// <summary>
/// Switches active save slot (0-2 for 3 save slots).
/// Saves current slot, loads new slot.
/// </summary>
/// <param name="slotIndex">Slot number (0, 1, or 2)</param>
public void SwitchToSlot(int slotIndex);
```

**Example:**
```csharp
// Switch to save slot 2
SaveManager.Instance.SwitchToSlot(2);
```

#### SetSerializer

```csharp
/// <summary>
/// Sets custom serializer (Binary, JSON, or Hybrid).
/// Must be called before any save/load operations.
/// </summary>
/// <param name="serializer">IGameSerializer implementation</param>
public void SetSerializer(IGameSerializer serializer);
```

**Example:**
```csharp
// Use binary serializer (10x faster, 10x smaller)
using Tartaria.Save.Serialization;

void Awake()
{
    SaveManager.Instance.SetSerializer(new BinaryGameSerializer());
}
```

### Data Structures

#### SaveData

```csharp
public class SaveData
{
    // Metadata
    public string saveVersion = "1.0.0";
    public long saveTimestamp;
    public float playtimeSeconds;
    
    // Player state
    public int playerLevel;
    public int playerXP;
    public float playerHealth;
    public float playerMaxHealth;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    
    // Progression
    public int currentMoon; // 1-13
    public int[] buildingsRestored; // Building IDs
    public QuestSaveContainer quests;
    public InventorySaveContainer inventory;
    
    // Settings
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public int qualityLevel; // 0=Low, 1=Medium, 2=High, 3=Ultra
    public string locale; // en-US, es-ES, fr-FR
}
```

#### QuestSaveContainer

```csharp
public class QuestSaveContainer
{
    public QuestSaveEntry[] entries;
}

public struct QuestSaveEntry
{
    public string questId;
    public int status; // QuestStatus enum as int
    public int[] objectiveProgress;
}
```

### Serializers

**Available serializers (Tartaria.Save.Serialization assembly):**

| Serializer | Speed | Size | Human-Readable | Use Case |
|------------|-------|------|----------------|----------|
| **DefaultJsonSerializer** | Slow | Large | Yes | Debug builds |
| **BinaryGameSerializer** | Fast | Small | No | Release builds |
| **HybridGameSerializer** | Medium | Medium | Partial | Beta builds (debugging + performance) |

**Performance comparison (typical save file):**

| Metric | JSON | Binary | Hybrid |
|--------|------|--------|--------|
| **Save time** | 45ms | 4ms | 12ms |
| **Load time** | 52ms | 5ms | 15ms |
| **File size** | 120 KB | 12 KB | 35 KB |

**Configure in SaveManager:**

```csharp
void Awake()
{
    if (Application.isEditor)
    {
        // Debug builds — use JSON for readability
        SetSerializer(new DefaultJsonSerializer());
    }
    else
    {
        // Release builds — use Binary for performance
        SetSerializer(new BinaryGameSerializer());
    }
}
```

### Save/Load Event Integration

**Subscribe to save events:**

```csharp
void Start()
{
    SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
    SaveManager.Instance.OnAfterLoad += HandleAfterLoad;
}

void HandleBeforeSave(SaveData sd)
{
    // Update CurrentSave before serialization
    sd.playerPosition = transform.position;
    sd.playerRotation = transform.rotation;
    sd.playtimeSeconds = Time.timeSinceLevelLoad;
}

void HandleAfterLoad(SaveData sd)
{
    // Restore state from loaded SaveData
    transform.position = sd.playerPosition;
    transform.rotation = sd.playerRotation;
    playerHealth = sd.playerHealth;
}

void OnDestroy()
{
    // CRITICAL: Unsubscribe to prevent memory leaks
    if (SaveManager.Instance != null)
    {
        SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
        SaveManager.Instance.OnAfterLoad -= HandleAfterLoad;
    }
}
```

### Cloud Save Integration

**Phase 3 feature (optional):**

```csharp
// Cloud sync enabled via CloudSaveService (internal)
// Auto-syncs to Firebase/Steam Cloud when online
// Offline-first design: local save always works

// Manual cloud sync trigger
SaveManager.Instance.SyncToCloud();

// Conflict resolution
GameEvents.OnCloudConflictDetected += (conflictInfo) =>
{
    ShowCloudConflictDialog(conflictInfo);
};
```

---

## GameEvents Catalog

**Purpose:** Decoupled event system for cross-assembly communication  
**Namespace:** `Tartaria.Core`  
**Location:** `Assets/_Project/Scripts/Core/GameEvents.cs`

### Building & Restoration Events

```csharp
/// <summary>
/// Raised when a Tartarian building completes restoration (tuning complete).
/// Subscribers: QuestManager, HUDController, GameLoopController, AudioController
/// </summary>
public static event Action<BuildingRestoredEventArgs> OnBuildingRestoredTyped;

public struct BuildingRestoredEventArgs
{
    public string buildingId;       // e.g., "star_dome"
    public string buildingName;     // e.g., "Great Star Dome"
    public Vector3 position;
    public int resonanceShardsAwarded;
    public int xpAwarded;
}

// Fire event:
GameEvents.FireBuildingRestored(new BuildingRestoredEventArgs
{
    buildingId = "star_dome",
    buildingName = "Great Star Dome",
    position = transform.position,
    resonanceShardsAwarded = 100,
    xpAwarded = 50
});
```

```csharp
/// <summary>
/// Raised when player discovers a buried building (via ResonanceScanner proximity).
/// Subscribers: QuestManager, HUDController
/// </summary>
public static event Action<BuildingDiscoveredEventArgs> OnBuildingDiscoveredTyped;

public struct BuildingDiscoveredEventArgs
{
    public string buildingId;
    public string buildingName;
    public Vector3 position;
}
```

### Combat Events

```csharp
/// <summary>
/// Raised when any enemy is defeated.
/// Subscribers: PlayerProgression (XP), QuestManager (kill tracking), HUDController
/// </summary>
public static event Action<EnemyKilledEventArgs> OnEnemyKilled;

public struct EnemyKilledEventArgs
{
    public string enemyType;        // "golem", "wraith", "reset_agent"
    public int xpReward;
    public Vector3 position;
    public GameObject killedBy;     // Player or companion
}

// Fire event:
GameEvents.RaiseEnemyKilled(new EnemyKilledEventArgs
{
    enemyType = "golem",
    xpReward = 25,
    position = transform.position,
    killedBy = player.gameObject
});
```

```csharp
/// <summary>
/// Raised when a boss enemy is defeated.
/// Subscribers: QuestManager, HUDController, CinematicController, MoonProgressionSystem
/// </summary>
public static event Action<BossDefeatedEventArgs> OnBossDefeated;

public struct BossDefeatedEventArgs
{
    public string bossId;           // "vein_core_moon2", "guardian_golem_moon4"
    public string bossName;
    public int moonId;              // 1-13
    public int xpReward;
    public ItemData[] itemRewards;
}
```

### Quest Events

```csharp
/// <summary>
/// Raised when a quest is activated, completed, or failed.
/// Subscribers: HUDController, AudioController, DialogueManager, SaveManager
/// </summary>
public static event Action<QuestStatusChangedEventArgs> OnQuestStatusChanged;

public struct QuestStatusChangedEventArgs
{
    public string questId;
    public QuestStatus oldStatus;
    public QuestStatus newStatus;
    public string questName;        // For UI display
}

// Fire event:
GameEvents.FireQuestStatusChanged(new QuestStatusChangedEventArgs
{
    questId = "moon1_main",
    oldStatus = QuestStatus.Active,
    newStatus = QuestStatus.Completed,
    questName = "Echoes of the Buried City"
});
```

```csharp
/// <summary>
/// Raised when quest objective progress updates (e.g., "Collect 5/10 shards").
/// Subscribers: HUDController (quest tracker), QuestLogUIPanel
/// </summary>
public static event Action<QuestObjectiveProgressedEventArgs> OnQuestObjectiveProgressed;

public struct QuestObjectiveProgressedEventArgs
{
    public string questId;
    public int objectiveIndex;
    public int currentProgress;
    public int targetProgress;
}
```

### Player Progression Events

```csharp
/// <summary>
/// Raised when player gains a level.
/// Subscribers: HUDController, PlayerController, AudioController, AchievementSystem
/// </summary>
public static event Action<LevelUpEventArgs> OnLevelUp;

public struct LevelUpEventArgs
{
    public int newLevel;
    public int skillPointsGained;   // Spent in skill tree
    public int statPointsGained;    // Spent on stats (HP, stamina, etc.)
}

// Fire event:
GameEvents.RaiseLevelUp(new LevelUpEventArgs
{
    newLevel = 5,
    skillPointsGained = 3,
    statPointsGained = 5
});
```

```csharp
/// <summary>
/// Raised when player gains XP.
/// Subscribers: HUDController (XP bar), StatsTracker
/// </summary>
public static event Action<XPGainedEventArgs> OnXPGained;

public struct XPGainedEventArgs
{
    public int xpGained;
    public int currentXP;
    public int xpToNextLevel;
    public string source;           // "enemy_killed", "quest_complete", "building_restored"
}
```

### Player Health Events

```csharp
/// <summary>
/// Raised when player takes damage.
/// Subscribers: HUDController, AudioController, CameraController, QuestManager
/// </summary>
public static event Action<PlayerDamagedEventArgs> OnPlayerDamaged;

public struct PlayerDamagedEventArgs
{
    public float damageAmount;
    public float currentHealth;
    public float maxHealth;
    public GameObject damageSource;  // Enemy, hazard, etc.
    public DamageType damageType;    // Physical, Corruption, Fall, etc.
}

// Fire event:
GameEvents.RaisePlayerDamaged(new PlayerDamagedEventArgs
{
    damageAmount = 25f,
    currentHealth = 75f,
    maxHealth = 100f,
    damageSource = enemy.gameObject,
    damageType = DamageType.Physical
});
```

```csharp
/// <summary>
/// Raised when player dies.
/// Subscribers: HUDController, PlayerController, AudioController, StatsTracker
/// </summary>
public static event Action OnPlayerDeath;

/// <summary>
/// Raised when player respawns after death.
/// Subscribers: HUDController, CameraController, AudioController, PlayerController
/// </summary>
public static event Action OnPlayerRespawned;
```

### Inventory Events

```csharp
/// <summary>
/// Raised when player picks up an item.
/// Subscribers: HUDController (pickup toast), InventoryUI, QuestManager
/// </summary>
public static event Action<ItemPickupEventArgs> OnItemPickup;

public struct ItemPickupEventArgs
{
    public ItemData item;
    public int quantity;
    public Vector3 pickupPosition;
}

// Fire event:
GameEvents.RaiseItemPickup(new ItemPickupEventArgs
{
    item = itemData,
    quantity = 1,
    pickupPosition = transform.position
});
```

```csharp
/// <summary>
/// Raised when item is removed from inventory (consumed, crafted, dropped).
/// Subscribers: InventoryUI, QuestManager
/// </summary>
public static event Action<ItemRemovedEventArgs> OnItemRemoved;

public struct ItemRemovedEventArgs
{
    public ItemData item;
    public int quantity;
    public string reason;           // "consumed", "crafted", "dropped", "sold"
}
```

### Moon Progression Events

```csharp
/// <summary>
/// Raised when a Moon is unlocked (prerequisites met).
/// Subscribers: HUDController, WorldMapUI, QuestManager
/// </summary>
public static event Action<int> OnMoonUnlocked; // moonId (1-13)

/// <summary>
/// Raised when a Moon is completed (all quests + boss defeated).
/// Subscribers: HUDController, CinematicController, SaveManager, AchievementSystem
/// </summary>
public static event Action<int> OnMoonCompleted; // moonId (1-13)

// Fire event:
GameEvents.FireMoonCompleted(2); // Moon 2 (Lunar Cathedral) complete
```

### HUD Events

```csharp
/// <summary>Displays quest objective text in HUD.</summary>
public static event Action<string> OnHUDShowObjective;

/// <summary>Displays dialogue subtitle in HUD.</summary>
public static event Action<string, string> OnHUDShowDialogue; // characterName, lineText

/// <summary>Displays fullscreen banner (e.g., "Moon 2 Complete").</summary>
public static event Action<string> OnHUDShowBanner;

/// <summary>Displays subtitle (for cutscenes, environmental narration).</summary>
public static event Action<string> OnHUDShowSubtitle;

/// <summary>Shows boss health bar.</summary>
public static event Action<string, float, float> OnHUDShowBossHealth; // bossName, currentHP, maxHP

/// <summary>Updates boss health bar (during combat).</summary>
public static event Action<float, float> OnHUDUpdateBossHealth; // currentHP, maxHP

/// <summary>Hides boss health bar (boss defeated or fled).</summary>
public static event Action OnHUDHideBossHealth;

// Fire HUD events:
GameEvents.FireHUDShowObjective("Purge 12 dissonance crystals");
GameEvents.FireHUDShowDialogue("Milo", "*tail wagging* I SMELL something incredible!");
GameEvents.FireHUDShowBanner("Moon 2 Complete!");
```

### Save Events

```csharp
/// <summary>
/// Raised when a critical save point is reached (Moon complete, 17th hour, etc.).
/// Triggers immediate save + cloud queue.
/// Subscribers: SaveManager, CloudSaveService
/// </summary>
public static event Action<string> OnCriticalSaveTrigger; // reason

// Fire event:
GameEvents.FireCriticalSaveTrigger("moon2_complete");
```

---

## Audio System Reference

**Namespace:** `Tartaria.Audio`  
**Manager:** `AudioManager` (singleton)  
**Location:** `Assets/_Project/Scripts/Audio/AudioManager.cs`

### Public Methods

#### PlaySFX

```csharp
/// <summary>
/// Plays a one-shot sound effect (non-looping).
/// </summary>
/// <param name="sfxId">SFX identifier (e.g., "building_restore", "sword_swing")</param>
/// <param name="volume">Volume multiplier (0.0 to 1.0, default 1.0)</param>
public void PlaySFX(string sfxId, float volume = 1.0f);
```

**Example:**
```csharp
AudioManager.Instance.PlaySFX("building_restore");
AudioManager.Instance.PlaySFX("sword_swing", 0.8f);
```

#### PlayMusic

```csharp
/// <summary>
/// Plays music track (looping, crossfades with current track).
/// </summary>
/// <param name="musicId">Music track identifier (e.g., "echohaven_ambient", "combat_boss")</param>
/// <param name="fadeDuration">Crossfade duration in seconds (default 2.0)</param>
public void PlayMusic(string musicId, float fadeDuration = 2.0f);
```

**Example:**
```csharp
// Transition to boss music
AudioManager.Instance.PlayMusic("combat_boss", 1.5f);
```

#### StopMusic

```csharp
/// <summary>
/// Stops current music with fade-out.
/// </summary>
/// <param name="fadeDuration">Fade-out duration in seconds (default 2.0)</param>
public void StopMusic(float fadeDuration = 2.0f);
```

#### PlayVoiceLine

```csharp
/// <summary>
/// Plays voice-over line (interrupts previous VO).
/// </summary>
/// <param name="voiceId">VO clip identifier (e.g., "milo_excited_01")</param>
/// <param name="volume">Volume multiplier (0.0 to 1.0, default 1.0)</param>
public void PlayVoiceLine(string voiceId, float volume = 1.0f);
```

**Example:**
```csharp
// Play Milo dialogue
AudioManager.Instance.PlayVoiceLine("milo_moon1_excited", 0.9f);
```

### Audio Mixer Groups

**Master → Music → Ambient, Combat, Cinematic**  
**Master → SFX → UI, Gameplay, Environment**  
**Master → Voice → Dialogue, Narration**

**Set volume via script:**

```csharp
// Set master volume (0.0 to 1.0)
AudioManager.Instance.SetMasterVolume(0.8f);

// Set music volume
AudioManager.Instance.SetMusicVolume(0.6f);

// Set SFX volume
AudioManager.Instance.SetSFXVolume(0.7f);
```

---

## VFX System Reference

**Namespace:** `Tartaria.Gameplay`  
**Manager:** `VFXController` (singleton)  
**Location:** `Assets/_Project/Scripts/Gameplay/VFXController.cs`

### Public Methods

#### PlayVFX

```csharp
/// <summary>
/// Plays a visual effect at a position (one-shot).
/// </summary>
/// <param name="vfxId">VFX identifier (e.g., "building_restore_explosion", "sword_slash")</param>
/// <param name="position">World position</param>
/// <param name="rotation">World rotation (optional, defaults to Quaternion.identity)</param>
public void PlayVFX(string vfxId, Vector3 position, Quaternion rotation = default);
```

**Example:**
```csharp
// Play restoration VFX
VFXController.Instance.PlayVFX("building_restore_explosion", transform.position);

// Play sword slash VFX (oriented to player forward)
VFXController.Instance.PlayVFX("sword_slash", transform.position, transform.rotation);
```

#### PlayVFXAttached

```csharp
/// <summary>
/// Plays a VFX attached to a GameObject (follows transform).
/// </summary>
/// <param name="vfxId">VFX identifier</param>
/// <param name="parent">GameObject to attach to</param>
/// <param name="localPosition">Local offset from parent (default Vector3.zero)</param>
/// <returns>Instantiated VFX GameObject (can be stopped manually)</returns>
public GameObject PlayVFXAttached(string vfxId, GameObject parent, Vector3 localPosition = default);
```

**Example:**
```csharp
// Attach buff VFX to player
GameObject buffVFX = VFXController.Instance.PlayVFXAttached("resonance_buff", player.gameObject);

// Stop VFX after 10 seconds
Destroy(buffVFX, 10f);
```

#### StopVFX

```csharp
/// <summary>
/// Stops a VFX instance (for looping VFX).
/// </summary>
/// <param name="vfxInstance">GameObject returned by PlayVFXAttached()</param>
public void StopVFX(GameObject vfxInstance);
```

---

## Data Architecture

### ScriptableObject Databases

**QuestDatabase**  
**Location:** `Assets/_Project/ScriptableObjects/Databases/QuestDatabase.asset`  
**Contains:** All quest definitions (Moon 1-13, 184 total quests)

**ItemDatabase**  
**Location:** `Assets/_Project/ScriptableObjects/Databases/ItemDatabase.asset`  
**Contains:** All item definitions (weapons, armor, consumables, materials)

**CraftingRecipeDatabase**  
**Location:** `Assets/_Project/ScriptableObjects/Databases/CraftingRecipeDatabase.asset`  
**Contains:** All crafting recipes (equipment, consumables, upgrades)

**EnemyDatabase**  
**Location:** `Assets/_Project/ScriptableObjects/Databases/EnemyDatabase.asset`  
**Contains:** All enemy definitions (stats, behaviors, loot tables)

### Query System

**Centralized query API via ServiceLocator:**

```csharp
// Query quests
QuestDefinition quest = ServiceLocator.Quest.GetQuest("moon1_main");
List<QuestDefinition> moonQuests = ServiceLocator.Quest.GetQuestsByMoon(1);

// Query items
ItemData item = ItemRegistry.GetItem("resonance_shard");
List<ItemData> legendaryItems = ItemRegistry.GetItemsByRarity(ItemRarity.Legendary);

// Query recipes
CraftingRecipeData recipe = CraftingRecipeRegistry.GetRecipe("harmonic_tuner_tier2");
List<CraftingRecipeData> availableRecipes = CraftingRecipeRegistry.GetCraftableRecipes(playerInventory);
```

---

## Service Locator Pattern

**Centralized access to major systems:**

```csharp
// Quest system
ServiceLocator.Quest.ActivateQuest("moon1_main");

// Save system
ServiceLocator.Save.Save();

// Audio system
ServiceLocator.Audio.PlaySFX("building_restore");

// Input system
bool jumpPressed = ServiceLocator.Input.GetJumpPressed();
```

**Registration pattern:**

```csharp
public class QuestManager : MonoBehaviour, IQuestService
{
    void Awake()
    {
        // Register with ServiceLocator
        ServiceLocator.Quest = this;
    }
    
    void OnDestroy()
    {
        // Unregister from ServiceLocator
        if (ServiceLocator.Quest == this)
            ServiceLocator.Quest = null;
    }
}
```

---

## Additional Resources

**Documentation:**
- [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) — Development workflows
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) — Build and deployment guide
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — Common issues and solutions

**Source Files:**
- `Assets/_Project/Scripts/Core/ServiceLocator.cs`
- `Assets/_Project/Scripts/Core/GameEvents.cs`
- `Assets/_Project/Scripts/Integration/QuestManager.cs`
- `Assets/_Project/Scripts/Integration/DialogueManager.cs`
- `Assets/_Project/Scripts/Save/SaveManager.cs`

---

**Version History:**

- **1.0.0-beta** (May 24, 2026) — Initial comprehensive API reference
