using System;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Centralized Game Events System — decouples cross-assembly communication.
    /// Reduces direct Instance?.Method() calls that create tight coupling.
    /// All events are thread-safe with null-check before invoke.
    /// 
    /// USAGE:
    ///   Subscribe: GameEvents.OnEnemyKilled += HandleEnemyKilled;
    ///   Unsubscribe: GameEvents.OnEnemyKilled -= HandleEnemyKilled; (in OnDestroy!)
    ///   Raise: GameEvents.RaiseEnemyKilled(new EnemyKilledEventArgs { enemyType = "golem", xpReward = 50 });
    /// 
    /// MEMORY SAFETY:
    ///   Always unsubscribe in OnDestroy to prevent memory leaks.
    ///   Example:
    ///     void OnDestroy() { GameEvents.OnEnemyKilled -= HandleEnemyKilled; }
    /// </summary>
    public static class GameEvents
    {
        // ═══════════════════════════════════════════════════════════════════
        // INPUT & UI CONTROL EVENTS (Legacy — preserved for backward compat)
        // ═══════════════════════════════════════════════════════════════════

        public static event Action OnToggleAetherVision;
        public static event Action OnTogglePause;
        public static event Action<string, float> OnRequestPurgeCorruption;
        public static event Action OnRequestActivateRSBuff;

        public static void FireToggleAetherVision() => OnToggleAetherVision?.Invoke();
        public static void FireTogglePause() => OnTogglePause?.Invoke();
        public static void FireRequestPurgeCorruption(string buildingId, float amount) => OnRequestPurgeCorruption?.Invoke(buildingId, amount);
        public static void FireRequestActivateRSBuff() => OnRequestActivateRSBuff?.Invoke();

        // ═══════════════════════════════════════════════════════════════════
        // BUILDING RESTORATION EVENTS (Enhanced with typed EventArgs)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when a Tartarian building completes restoration (tuning complete).
        /// Subscribers: QuestManager (quest progress), HUDController (UI feedback), 
        ///              GameLoopController (RS award), AudioController (SFX).
        /// </summary>
        public static event Action<BuildingRestoredEventArgs> OnBuildingRestoredTyped;

        /// <summary>
        /// Raised when player discovers a buried building (via ResonanceScanner proximity).
        /// Subscribers: QuestManager (objective progress), HUDController (discovery tooltip).
        /// </summary>
        public static event Action<BuildingDiscoveredEventArgs> OnBuildingDiscoveredTyped;

        // Legacy events for backward compat (deprecated, use typed versions)
        public static event Action<string> OnBuildingRestored;   // buildingId
        public static event Action<string, Vector3> OnBuildingDiscovered; // buildingName, position

        public static void FireBuildingRestored(string buildingId) => OnBuildingRestored?.Invoke(buildingId);
        public static void FireBuildingDiscovered(string buildingName, Vector3 position) => OnBuildingDiscovered?.Invoke(buildingName, position);

        // ═══════════════════════════════════════════════════════════════════
        // COMBAT EVENTS (New — reduces PlayerHealth/EnemyAI coupling)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when any enemy is defeated.
        /// Subscribers: PlayerProgression (XP award), QuestManager (kill count tracking),
        ///              HUDController (kill feed), StatsTracker (analytics).
        /// </summary>
        public static event Action<EnemyKilledEventArgs> OnEnemyKilled;

        /// <summary>
        /// Raised when a boss enemy is defeated.
        /// Subscribers: QuestManager (boss quest completion), HUDController (trophy UI),
        ///              CinematicController (post-boss sequence), MoonProgressionSystem.
        /// </summary>
        public static event Action<BossDefeatedEventArgs> OnBossDefeated;

        // ═══════════════════════════════════════════════════════════════════
        // QUEST EVENTS (New — reduces QuestManager direct calls)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when a quest is activated, completed, or failed.
        /// Subscribers: HUDController (quest notification), AudioController (quest complete SFX),
        ///              DialogueManager (trigger narrative beats), SaveManager (persist state).
        /// </summary>
        public static event Action<QuestStatusChangedEventArgs> OnQuestStatusChanged;

        /// <summary>
        /// Raised when quest objective progress updates (e.g., "Collect 5/10 shards").
        /// Subscribers: HUDController (quest tracker UI), QuestLogUIPanel (live updates).
        /// </summary>
        public static event Action<QuestObjectiveProgressedEventArgs> OnQuestObjectiveProgressed;

        // ═══════════════════════════════════════════════════════════════════
        // PLAYER PROGRESSION EVENTS (New — reduces PlayerProgression coupling)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when player gains a level.
        /// Subscribers: HUDController (level-up notification), PlayerController (stat update),
        ///              AudioController (level-up fanfare), AchievementSystem.
        /// </summary>
        public static event Action<LevelUpEventArgs> OnLevelUp;

        /// <summary>
        /// Raised when player gains XP.
        /// Subscribers: HUDController (XP bar update), StatsTracker (analytics).
        /// </summary>
        public static event Action<XPGainedEventArgs> OnXPGained;

        // ═══════════════════════════════════════════════════════════════════
        // INVENTORY EVENTS (New — reduces InventorySystem coupling)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when an item is picked up or added to inventory.
        /// Subscribers: HUDController (pickup notification), QuestManager (item collection objectives),
        ///              InventoryUIPanel (refresh grid), AudioController (pickup SFX).
        /// </summary>
        public static event Action<ItemPickupEventArgs> OnItemPickup;

        /// <summary>
        /// Raised when an item is removed from inventory (consumed, discarded, crafted).
        /// Subscribers: InventoryUIPanel (refresh grid), StatsTracker (item usage analytics).
        /// </summary>
        public static event Action<ItemRemovedEventArgs> OnItemRemoved;

        // ═══════════════════════════════════════════════════════════════════
        // MOON PROGRESSION EVENTS (Enhanced with typed EventArgs)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when a Moon zone is unlocked (portal activated).
        /// Subscribers: HUDController (Moon unlock notification), MapController (reveal zone),
        ///              QuestManager (trigger Moon intro quest), SaveManager (persist unlock).
        /// </summary>
        public static event Action<MoonUnlockedEventArgs> OnMoonUnlocked;

        /// <summary>
        /// Raised when a Moon zone is fully completed (all objectives met).
        /// Subscribers: HUDController (trophy display), QuestManager (mark complete),
        ///              ProgressionController (unlock next Moon), CinematicController (outro).
        /// </summary>
        public static event Action<MoonCompletedEventArgs> OnMoonCompleted;

        // Legacy Moon events for backward compat (deprecated)
        public static event Action<int> OnMoonCleared;           // moonNum 1-13
        public static event Action OnMoon3FastTravelUnlocked;

        public static void FireMoonCleared(int moonNum) => OnMoonCleared?.Invoke(moonNum);
        public static void FireMoon3FastTravelUnlocked() => OnMoon3FastTravelUnlocked?.Invoke();

        // ═══════════════════════════════════════════════════════════════════
        // DIALOGUE & NARRATIVE EVENTS (New)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when dialogue begins or ends.
        /// Subscribers: PlayerController (disable movement), CameraController (focus on NPC),
        ///              InputManager (block gameplay input), HUDController (show dialogue panel).
        /// </summary>
        public static event Action<DialogueEventArgs> OnDialogueStateChanged;

        // ═══════════════════════════════════════════════════════════════════
        // PLAYER ABILITY EVENTS (New)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when Aether Vision is toggled on/off.
        /// Subscribers: BuildingRenderer (highlight interactive objects), CollectibleRenderer (glow),
        ///              HUDController (visual feedback), AudioController (toggle SFX).
        /// </summary>
        public static event Action<AetherVisionToggledEventArgs> OnAetherVisionToggledTyped;

        // ═══════════════════════════════════════════════════════════════════
        // RESONANCE SHARDS (RS) ECONOMY EVENTS (Legacy — preserved)
        // ═══════════════════════════════════════════════════════════════════

        public static event Action<float> OnRSChanged;
        public static void FireRSChange(float amount) => OnRSChanged?.Invoke(amount);

        // ═══════════════════════════════════════════════════════════════════
        // SAVE & CLOUD EVENTS (Legacy — preserved for Phase 3 R5/R6)
        // ═══════════════════════════════════════════════════════════════════

        public static event Action<string> OnCriticalSaveTrigger;
        public static event Action<SaveConflictInfo> OnCloudConflictDetected;
        public static event Action<string> OnRemotePushNotificationReceived;
        public static event Action<string> OnHUDAchievementToast;
        public static event Action<string> OnHUDCloudQueueToast;
        public static event Action<string, string, string> OnHUDSaveConflictPrompt;

        public static void FireCriticalSaveTrigger(string reason) => OnCriticalSaveTrigger?.Invoke(reason);
        public static void FireCloudConflictDetected(SaveConflictInfo info) => OnCloudConflictDetected?.Invoke(info);
        public static void FireRemotePushNotification(string payload) => OnRemotePushNotificationReceived?.Invoke(payload);
        public static void FireHUDAchievementToast(string msg) => OnHUDAchievementToast?.Invoke(msg);
        public static void FireHUDCloudQueueToast(string msg) => OnHUDCloudQueueToast?.Invoke(msg);
        public static void FireHUDSaveConflictPrompt(string localSummary, string cloudSummary, string act) => OnHUDSaveConflictPrompt?.Invoke(localSummary, cloudSummary, act);

        // ═══════════════════════════════════════════════════════════════════
        // PERFORMANCE & WEATHER EVENTS (Legacy — preserved)
        // ═══════════════════════════════════════════════════════════════════

        public static event Action<string, string> OnPerformanceFallback;
        public static event Action<int, float> OnWeatherHazardStarted;
        public static event Action<int> OnWeatherHazardEnded;

        public static void FirePerformanceFallback(string tierName, string reason) => OnPerformanceFallback?.Invoke(tierName, reason);
        public static void FireWeatherHazardStarted(int hazardType, float duration) => OnWeatherHazardStarted?.Invoke(hazardType, duration);
        public static void FireWeatherHazardEnded(int hazardType) => OnWeatherHazardEnded?.Invoke(hazardType);

        // ═══════════════════════════════════════════════════════════════════
        // NEW GAME PLUS EVENTS (Legacy — preserved)
        // ═══════════════════════════════════════════════════════════════════

        public static event Action<int> OnNewGamePlusStarted;
        public static event Action<int> OnPermanentUnlockEarned;

        public static void FireNewGamePlusStarted(int ngPlusCycle) => OnNewGamePlusStarted?.Invoke(ngPlusCycle);
        public static void FirePermanentUnlockEarned(int rewardId) => OnPermanentUnlockEarned?.Invoke(rewardId);

        // ═══════════════════════════════════════════════════════════════════
        // RAISE METHODS (Thread-safe with null-check + exception handling)
        // ═══════════════════════════════════════════════════════════════════

        public static void RaiseBuildingRestored(BuildingRestoredEventArgs args)
        {
            try
            {
                OnBuildingRestoredTyped?.Invoke(args);
                // Also fire legacy event for backward compat
                OnBuildingRestored?.Invoke(args.buildingId);
            }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBuildingRestored: {ex}"); }
        }

        public static void RaiseBuildingDiscovered(BuildingDiscoveredEventArgs args)
        {
            try
            {
                OnBuildingDiscoveredTyped?.Invoke(args);
                // Also fire legacy event for backward compat
                OnBuildingDiscovered?.Invoke(args.buildingId, args.position);
            }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBuildingDiscovered: {ex}"); }
        }

        public static void RaiseEnemyKilled(EnemyKilledEventArgs args)
        {
            try { OnEnemyKilled?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnEnemyKilled: {ex}"); }
        }

        public static void RaiseBossDefeated(BossDefeatedEventArgs args)
        {
            try { OnBossDefeated?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBossDefeated: {ex}"); }
        }

        public static void RaiseQuestStatusChanged(QuestStatusChangedEventArgs args)
        {
            try { OnQuestStatusChanged?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnQuestStatusChanged: {ex}"); }
        }

        public static void RaiseQuestObjectiveProgressed(QuestObjectiveProgressedEventArgs args)
        {
            try { OnQuestObjectiveProgressed?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnQuestObjectiveProgressed: {ex}"); }
        }

        public static void RaiseLevelUp(LevelUpEventArgs args)
        {
            try { OnLevelUp?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnLevelUp: {ex}"); }
        }

        public static void RaiseXPGained(XPGainedEventArgs args)
        {
            try { OnXPGained?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnXPGained: {ex}"); }
        }

        public static void RaiseItemPickup(ItemPickupEventArgs args)
        {
            try { OnItemPickup?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnItemPickup: {ex}"); }
        }

        public static void RaiseItemRemoved(ItemRemovedEventArgs args)
        {
            try { OnItemRemoved?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnItemRemoved: {ex}"); }
        }

        public static void RaiseMoonUnlocked(MoonUnlockedEventArgs args)
        {
            try
            {
                OnMoonUnlocked?.Invoke(args);
                // Also fire legacy event for backward compat
                OnMoonCleared?.Invoke(args.moonIndex);
            }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnMoonUnlocked: {ex}"); }
        }

        public static void RaiseMoonCompleted(MoonCompletedEventArgs args)
        {
            try { OnMoonCompleted?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnMoonCompleted: {ex}"); }
        }

        public static void RaiseDialogueStateChanged(DialogueEventArgs args)
        {
            try { OnDialogueStateChanged?.Invoke(args); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnDialogueStateChanged: {ex}"); }
        }

        public static void RaiseAetherVisionToggled(bool enabled)
        {
            try
            {
                OnAetherVisionToggledTyped?.Invoke(new AetherVisionToggledEventArgs { enabled = enabled });
                // Also fire legacy event for backward compat
                if (enabled) OnToggleAetherVision?.Invoke();
            }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnAetherVisionToggled: {ex}"); }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // EVENT ARGS DEFINITIONS (Typed payloads for decoupled communication)
    // ═══════════════════════════════════════════════════════════════════

    public class BuildingRestoredEventArgs
    {
        public string buildingId;
        public int rsReward;
        public Vector3 position;
        public float tuningAccuracy;  // 0-1, affects RS bonus
    }

    public class BuildingDiscoveredEventArgs
    {
        public string buildingId;
        public Vector3 position;
    }

    public class EnemyKilledEventArgs
    {
        public string enemyType;
        public int xpReward;
        public string lootItemId;
        public int lootCount;
        public Vector3 position;
        public GameObject killedBy;  // Player or companion
    }

    public class BossDefeatedEventArgs
    {
        public string bossId;
        public int xpReward;
        public int rsReward;
        public Vector3 position;
    }

    public class QuestStatusChangedEventArgs
    {
        public string questId;
        public QuestStatus newStatus;
        public QuestStatus oldStatus;
    }

    public class QuestObjectiveProgressedEventArgs
    {
        public string questId;
        public int objectiveIndex;
        public int current;
        public int target;
    }

    public class LevelUpEventArgs
    {
        public int newLevel;
        public int oldLevel;
        public float maxHealthBonus;
        public float damageBonus;
        public float movementSpeedBonus;
    }

    public class XPGainedEventArgs
    {
        public float amount;
        public string source;  // "enemy_kill", "quest_complete", "building_restore", etc.
    }

    public class ItemPickupEventArgs
    {
        public string itemId;
        public int count;
        public int totalCount;  // New total in inventory
    }

    public class ItemRemovedEventArgs
    {
        public string itemId;
        public int count;
        public int remainingCount;
        public string reason;  // "consumed", "discarded", "crafted", "quest_turn_in"
    }

    public class MoonUnlockedEventArgs
    {
        public int moonIndex;
        public string moonName;
        public Vector3 portalPosition;
    }

    public class MoonCompletedEventArgs
    {
        public int moonIndex;
        public string moonName;
        public int rsReward;
        public float completionTime;
    }

    public class DialogueEventArgs
    {
        public bool isActive;
        public string speakerName;
        public string dialogueId;
    }

    public class AetherVisionToggledEventArgs
    {
        public bool enabled;
    }

    /// <summary>
    /// Payload for player-facing save conflict UI (Phase 3 R5). Contains summary stats for "This Device vs Cloud" dialog.
    /// </summary>
    [Serializable]
    public class SaveConflictInfo
    {
        public string localModified;
        public string cloudModified;
        public float localPlayTime;
        public float cloudPlayTime;
        public int localBuildingsRestored;
        public int cloudBuildingsRestored;
        public int localMoon;
        public int cloudMoon;
        public string recommendedAction; // "merge", "local", "cloud"
        public string details;
    }
}
