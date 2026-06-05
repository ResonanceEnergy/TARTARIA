using System;
using UnityEngine;
using QuestStatus = Tartaria.Core.Enums.QuestStatus;

namespace Tartaria.Core
{
    /// <summary>
    /// All events are thread-safe with null-check before invoke.
    /// </summary>
    public static class GameEvents
    {
        /// Raised when player gains XP.
        /// Subscribers: HUDController (XP bar update), StatsTracker (analytics).
        /// </summary>
        public static event Action<XPGainedEventArgs> OnXPGained;

        // ═══════════════════════════════════════════════════════════════════
        // PLAYER HEALTH EVENTS (New — reduces PlayerHealthController coupling)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when player takes damage.
        /// Subscribers: HUDController (health bar + damage indicators), AudioController (damage SFX),
        ///              CameraController (screen shake), QuestManager (damage tracking).
        /// </summary>
        public static event Action<PlayerDamagedEventArgs> OnPlayerDamaged;

        /// <summary>
        /// Raised when player dies.
        /// Subscribers: HUDController (death screen), PlayerController (disable input),
        ///              AudioController (death SFX), StatsTracker (death count), QuestManager.
        /// </summary>
        public static event Action OnPlayerDeath;

        /// <summary>
        /// Raised when player respawns after death.
        /// Subscribers: HUDController (fade-in effect), CameraController (reset),
        ///              AudioController (respawn SFX), PlayerController (re-enable input).
        /// </summary>
        public static event Action OnPlayerRespawned;

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
        // HUD DISPLAY EVENTS (New — breaks Tartaria.UI → Tartaria.Integration cyclic dependency)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised when Integration systems need to display an objective message.
        /// Subscribers: HUDController (objective text display).
        /// </summary>
        public static event Action<string> OnHUDShowObjective;

        /// <summary>
        /// Raised when Integration systems need to display dialogue.
        /// Subscribers: HUDController (dialogue panel), DialoguePanel (text display).
        /// </summary>
        public static event Action<string, string> OnHUDShowDialogue; // speaker, message

        /// <summary>
        /// Raised when Integration systems need to display a full-screen banner.
        /// Subscribers: HUDController (banner UI).
        /// </summary>
        public static event Action<string, string, float> OnHUDShowBanner; // title, subtitle, duration

        /// <summary>
        /// Raised when Integration systems need to display a subtitle.
        /// Subscribers: HUDController (subtitle display).
        /// </summary>
        public static event Action<string, float> OnHUDShowSubtitle; // message, duration

        /// <summary>
        /// Raised when a Moon is completed and trophy should be shown.
        /// Subscribers: HUDController (trophy UI).
        /// </summary>
        public static event Action<string, string> OnHUDShowMoonTrophy; // title, subtitle

        /// <summary>
        /// Raised when a boss encounter starts.
        /// Subscribers: HUDController (boss health bar initialization).
        /// </summary>
        public static event Action<string, float> OnHUDShowBossHealth; // bossName, normalizedHealth

        /// <summary>
        /// Raised when boss health changes during combat.
        /// Subscribers: HUDController (boss health bar update).
        /// </summary>
        public static event Action<float> OnHUDUpdateBossHealth; // normalizedHealth

        /// <summary>
        /// Raised when boss is defeated or encounter ends.
        /// Subscribers: HUDController (hide boss health bar).
        /// </summary>
        public static event Action OnHUDHideBossHealth;

        /// <summary>
        /// Raised when an interaction prompt should be shown.
        /// Subscribers: HUDController (interaction prompt UI).
        /// </summary>
        public static event Action<string> OnHUDShowInteractionPrompt; // message

        /// <summary>
        /// Raised when interaction prompt should be hidden.
        /// Subscribers: HUDController (hide interaction prompt UI).
        /// </summary>
        public static event Action OnHUDHideInteractionPrompt;

        /// <summary>
        /// Raised when RS (Resonance Shards) gain should be shown with flash effect.
        /// Subscribers: HUDController (RS counter flash animation).
        /// </summary>
        public static event Action<float> OnHUDFlashRSGain; // amount

        /// <summary>
        /// Raised when boss nameplate should be shown (pre-combat intro).
        /// Subscribers: HUDController (boss nameplate display).
        /// </summary>
        public static event Action<string, string> OnHUDShowBossNameplate; // bossName, bossTitle

        /// <summary>
        /// Raised when enemy combat bark should be shown.
        /// Subscribers: HUDController (enemy bark UI).
        /// </summary>
        public static event Action<string, float> OnHUDShowEnemyBark; // message, duration

        /// <summary>
        /// Raised when corruption whisper effect should be shown.
        /// Subscribers: HUDController (corruption whisper UI).
        /// </summary>
        public static event Action<string, float> OnHUDShowCorruptionWhisper; // message, duration

        /// <summary>
        /// Raised when frequency wheel should be updated (tuning mini-game).
        /// Subscribers: HUDController (frequency wheel UI).
        /// </summary>
        public static event Action<float, float> OnHUDUpdateFrequencyWheel; // frequency, param

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
            
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBuildingRestored: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseBuildingDiscovered(BuildingDiscoveredEventArgs args)
        {
            try
            {
                OnBuildingDiscoveredTyped?.Invoke(args);
                // Also fire legacy event for backward compat
                OnBuildingDiscovered?.Invoke(args.buildingId, args.position);
            
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBuildingDiscovered: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseEnemyKilled(EnemyKilledEventArgs args)
        {
            try { OnEnemyKilled?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnEnemyKilled: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseBossDefeated(BossDefeatedEventArgs args)
        {
            try { OnBossDefeated?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnBossDefeated: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseQuestStatusChanged(QuestStatusChangedEventArgs args)
        {
            try { OnQuestStatusChanged?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnQuestStatusChanged: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseQuestObjectiveProgressed(QuestObjectiveProgressedEventArgs args)
        {
            try { OnQuestObjectiveProgressed?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnQuestObjectiveProgressed: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseLevelUp(LevelUpEventArgs args)
        {
            try { OnLevelUp?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnLevelUp: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseXPGained(XPGainedEventArgs args)
        {
            try { OnXPGained?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnXPGained: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseItemPickup(ItemPickupEventArgs args)
        {
            try { OnItemPickup?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnItemPickup: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseItemRemoved(ItemRemovedEventArgs args)
        {
            try { OnItemRemoved?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnItemRemoved: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaisePlayerDamaged(float damageAmount, float remainingHealth)
        {
            try
            {
                var args = new PlayerDamagedEventArgs
                {
                    damageAmount = damageAmount,
                    remainingHealth = remainingHealth
                
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
};
                OnPlayerDamaged?.Invoke(args);
            
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerDamaged: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaisePlayerDeath()
        {
            try { OnPlayerDeath?.Invoke(); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerDeath: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaisePlayerRespawned()
        {
            try { OnPlayerRespawned?.Invoke(); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerRespawned: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseMoonUnlocked(MoonUnlockedEventArgs args)
        {
            try
            {
                OnMoonUnlocked?.Invoke(args);
                // Also fire legacy event for backward compat
                OnMoonCleared?.Invoke(args.moonIndex);
            
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnMoonUnlocked: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseMoonCompleted(MoonCompletedEventArgs args)
        {
            try { OnMoonCompleted?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnMoonCompleted: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseDialogueStateChanged(DialogueEventArgs args)
        {
            try { OnDialogueStateChanged?.Invoke(args); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnDialogueStateChanged: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseAetherVisionToggled(bool enabled)
        {
            try
            {
                OnAetherVisionToggledTyped?.Invoke(new AetherVisionToggledEventArgs { enabled = enabled 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
});
                // Also fire legacy event for backward compat
                if (enabled) OnToggleAetherVision?.Invoke();
            
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnAetherVisionToggled: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        // ═══════════════════════════════════════════════════════════════════
        // HUD RAISE METHODS (Thread-safe with null-check + exception handling)
        // ═══════════════════════════════════════════════════════════════════

        public static void RaiseHUDShowObjective(string message)
        {
            try { OnHUDShowObjective?.Invoke(message); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowObjective: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowDialogue(string speaker, string message)
        {
            try { OnHUDShowDialogue?.Invoke(speaker, message); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowDialogue: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowBanner(string title, string subtitle, float duration = 5f)
        {
            try { OnHUDShowBanner?.Invoke(title, subtitle, duration); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBanner: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowSubtitle(string message, float duration)
        {
            try { OnHUDShowSubtitle?.Invoke(message, duration); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowSubtitle: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowMoonTrophy(string title, string subtitle)
        {
            try { OnHUDShowMoonTrophy?.Invoke(title, subtitle); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowMoonTrophy: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowBossHealth(string bossName, float normalizedHealth)
        {
            try { OnHUDShowBossHealth?.Invoke(bossName, normalizedHealth); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBossHealth: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDUpdateBossHealth(float normalizedHealth)
        {
            try { OnHUDUpdateBossHealth?.Invoke(normalizedHealth); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDUpdateBossHealth: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDHideBossHealth()
        {
            try { OnHUDHideBossHealth?.Invoke(); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDHideBossHealth: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowInteractionPrompt(string message)
        {
            try { OnHUDShowInteractionPrompt?.Invoke(message); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowInteractionPrompt: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDHideInteractionPrompt()
        {
            try { OnHUDHideInteractionPrompt?.Invoke(); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDHideInteractionPrompt: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDFlashRSGain(float amount)
        {
            try { OnHUDFlashRSGain?.Invoke(amount); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDFlashRSGain: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowBossNameplate(string bossName, string bossTitle)
        {
            try { OnHUDShowBossNameplate?.Invoke(bossName, bossTitle); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBossNameplate: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowEnemyBark(string message, float duration)
        {
            try { OnHUDShowEnemyBark?.Invoke(message, duration); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowEnemyBark: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDShowCorruptionWhisper(string message, float duration)
        {
            try { OnHUDShowCorruptionWhisper?.Invoke(message, duration); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowCorruptionWhisper: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

        public static void RaiseHUDUpdateFrequencyWheel(float frequency, float param)
        {
            try { OnHUDUpdateFrequencyWheel?.Invoke(frequency, param); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDUpdateFrequencyWheel: {ex
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}"); 
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
        
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
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
        public GameObject Building;  // Reference to the restored building (for checkpoint tracking)
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class BuildingDiscoveredEventArgs
    {
        public string buildingId;
        public Vector3 position;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class EnemyKilledEventArgs
    {
        public string enemyType;
        public int xpReward;
        public string lootItemId;
        public int lootCount;
        public Vector3 position;
        public GameObject killedBy;  // Player or companion
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class BossDefeatedEventArgs
    {
        public string bossId;
        public int xpReward;
        public int rsReward;
        public Vector3 position;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class QuestStatusChangedEventArgs
    {
        public string questId;
        public QuestStatus newStatus;
        public QuestStatus oldStatus;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class QuestObjectiveProgressedEventArgs
    {
        public string questId;
        public int objectiveIndex;
        public int current;
        public int target;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class LevelUpEventArgs
    {
        public int newLevel;
        public int oldLevel;
        public float maxHealthBonus;
        public float damageBonus;
        public float movementSpeedBonus;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class XPGainedEventArgs
    {
        public float amount;
        public string source;  // "enemy_kill", "quest_complete", "building_restore", etc.
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class PlayerDamagedEventArgs
    {
        public float damageAmount;
        public float remainingHealth;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class ItemPickupEventArgs
    {
        public string itemId;
        public int count;
        public int totalCount;  // New total in inventory
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class ItemRemovedEventArgs
    {
        public string itemId;
        public int count;
        public int remainingCount;
        public string reason;  // "consumed", "discarded", "crafted", "quest_turn_in"
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class MoonUnlockedEventArgs
    {
        public int moonIndex;
        public string moonName;
        public Vector3 portalPosition;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class MoonCompletedEventArgs
    {
        public int moonIndex;
        public string moonName;
        public int rsReward;
        public float completionTime;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class DialogueEventArgs
    {
        public bool isActive;
        public string speakerName;
        public string dialogueId;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    public class AetherVisionToggledEventArgs
    {
        public bool enabled;
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
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
    
    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}

    // ====================================
    // NEW EVENTS FOR BUILT SYSTEMS (2025)
    // ====================================

    // Player Spawned
    public static event System.Action<GameObject> OnPlayerSpawned;
    public static void FirePlayerSpawned(GameObject player) => OnPlayerSpawned?.Invoke(player);

    // Resonance Score Changed
    public static event System.Action<float> OnResonanceScoreChanged;
    public static void FireResonanceScoreChanged(float rsValue) => OnResonanceScoreChanged?.Invoke(rsValue);

    // Player Health Changed
    public static event System.Action<float, float> OnPlayerHealthChanged;
    public static void FirePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

    // Aether Energy Changed
    public static event System.Action<float> OnAetherEnergyChanged;
    public static void FireAetherEnergyChanged(float value) => OnAetherEnergyChanged?.Invoke(value);

    // Inventory Changed
    public static event System.Action OnInventoryChanged;
    public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    // Quest Events
    public static event System.Action<string> OnQuestActivated;
    public static void FireQuestActivated(string questId) => OnQuestActivated?.Invoke(questId);

    public static event System.Action<string, int> OnQuestObjectiveCompleted;
    public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex) => OnQuestObjectiveCompleted?.Invoke(questId, objectiveIndex);

    public static event System.Action<string> OnQuestCompleted;
    public static void FireQuestCompleted(string questId) => OnQuestCompleted?.Invoke(questId);
}
}
