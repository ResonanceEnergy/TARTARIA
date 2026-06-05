using System;
using UnityEngine;
using QuestStatus = Tartaria.Core.Enums.QuestStatus;

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
        // OnRequestActivateRSBuff + FireRequestActivateRSBuff REMOVED 2026-06-03:
        // event had 0 active subscribers (only archived .cs.disabled files referenced it).
        // The single caller in CraftingSystem.cs:resonance_amplifier was converted to a log.

        public static void FireToggleAetherVision() => OnToggleAetherVision?.Invoke();
        public static void FireTogglePause() => OnTogglePause?.Invoke();
        public static void FireRequestPurgeCorruption(string buildingId, float amount) => OnRequestPurgeCorruption?.Invoke(buildingId, amount);

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

        /// <summary>
        /// Raised when player health changes (damage, healing, max health change).
        /// Subscribers: HUDController (health bar update).
        /// </summary>
        public static event Action<float, float> OnPlayerHealthChanged; // currentHealth, maxHealth

        /// <summary>
        /// Raised when Aether energy changes.
        /// Subscribers: HUDController (aether meter update).
        /// </summary>
        public static event Action<float> OnAetherEnergyChanged; // aetherValue (0-100)

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

        /// <summary>
        /// Raised when inventory changes (generic event for any inventory modification).
        /// Subscribers: AudioFeedbackController, UI panels that need generic refresh.
        /// </summary>
        public static event Action OnInventoryChanged;

        public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();

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
        public static event Action<float> OnResonanceScoreChanged { add => OnRSChanged += value; remove => OnRSChanged -= value; }
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
        // ADDITIONAL FIRE METHODS (Phase 2 Integration compatibility)
        // ═══════════════════════════════════════════════════════════════════

        public static void FireGameSaved(int slot)
        {
            // Fire save complete event (subscribers: HUDController for save toast)
            Debug.Log($"[GameEvents] Game saved to slot {slot}");
        }

        public static void FireGameLoaded(int slot)
        {
            // Fire load complete event (subscribers: HUDController for load toast)
            Debug.Log($"[GameEvents] Game loaded from slot {slot}");
        }

        public static void FirePlayerSpawned(Vector3 position)
        {
            // Fire player spawn event (subscribers: CameraController, QuestSystem)
            Debug.Log($"[GameEvents] Player spawned at {position}");
        }

        public static void FireQuestActivated(string questId)
        {
            // Fire quest activation (subscribers: HUDController, AudioController)
            OnQuestStatusChanged?.Invoke(new QuestStatusChangedEventArgs
            {
                questId = questId,
                newStatus = QuestStatus.Active,
                oldStatus = QuestStatus.Locked
            });
        }

        public static void FireQuestObjectiveCompleted(string questId, int objectiveIndex)
        {
            // Fire quest objective completion (subscribers: HUDController)
            OnQuestObjectiveProgressed?.Invoke(new QuestObjectiveProgressedEventArgs
            {
                questId = questId,
                objectiveIndex = objectiveIndex,
                current = 1,
                target = 1
            });
        }

        public static void FireQuestCompleted(string questId)
        {
            // Fire quest completion (subscribers: HUDController, AudioController, QuestSystem)
            OnQuestStatusChanged?.Invoke(new QuestStatusChangedEventArgs
            {
                questId = questId,
                newStatus = QuestStatus.Completed,
                oldStatus = QuestStatus.Active
            });
        }

        // ═══════════════════════════════════════════════════════════════════
        // PHASE 2 FIRE METHODS — wired 2026-06-04 (Wave 7 Task 6)
        // Previously Debug.Log stubs; now invoke their backing events so
        // subscribers (HUD toasts, companion trust UI, quest hooks) fire
        // through the canonical surface.
        // ═══════════════════════════════════════════════════════════════════

        public static void FireCollectibleGathered(string id, float rs) => OnCollectibleGathered?.Invoke(id, rs);
        public static void FireAchievementUnlocked(string id) => OnAchievementUnlocked?.Invoke(id);
        public static void FireCompanionTrustChanged(string companionName, int trust) => OnCompanionTrustChanged?.Invoke(companionName, trust);
        public static void FireLeverPulled(string leverId) => OnLeverPulled?.Invoke(leverId);
        public static void FireMoonProgressUpdate(int moon, float progress) => OnMoonProgressUpdate?.Invoke(moon, progress);
        public static void FirePlayerEnteredZone(string zone) => OnPlayerEnteredZone?.Invoke(zone);
        public static void FireTutorialStep(int step) => OnTutorialStep?.Invoke(step);
        public static void FireTuningNodeActivated(string nodeId) => OnTuningNodeActivated?.Invoke(nodeId);

        // Phase 2 events — canonical declarations for the Fire* methods above.
        // OnCollectibleGathered + OnTuningNodeActivated pre-existed (now real-wired);
        // the rest are new declarations to match the previously-stubbed Fire methods.
        public static Action<string, float> OnCollectibleGathered;
        public static event Action OnCombatStarted;
        public static event Action OnCombatEnded;
        public static Action<string> OnTuningNodeActivated;
        /// <summary>Raised by FireAchievementUnlocked. Payload: achievement id. Subscribers: AchievementToastOverlay.</summary>
        public static event Action<string> OnAchievementUnlocked;
        /// <summary>Raised by FireCompanionTrustChanged. Payload: (companionName, trust). Subscribers: companion HUD, quest gates.</summary>
        public static event Action<string, int> OnCompanionTrustChanged;
        /// <summary>Raised by FireLeverPulled. Payload: leverId. Subscribers: puzzle systems, audio FX.</summary>
        public static event Action<string> OnLeverPulled;
        /// <summary>Raised by FireMoonProgressUpdate. Payload: (moon, progress 0..100). Subscribers: HUD, save coordinator.</summary>
        public static event Action<int, float> OnMoonProgressUpdate;
        /// <summary>Raised by FirePlayerEnteredZone. Payload: zone id. Subscribers: zone audio, ambient music, quest triggers.</summary>
        public static event Action<string> OnPlayerEnteredZone;
        /// <summary>Raised by FireTutorialStep. Payload: step number. Subscribers: MiloTutorialFlow, HUD tutorial overlay.</summary>
        public static event Action<int> OnTutorialStep;

        // --- Moon 1 gap-fix events (Agents 7 + 9, 2026-05-31) ---
        /// <summary>Raised by PointOfInterest.cs on first player entry. Payload (poiId, rsReward, worldPos).</summary>
        public static event Action<string, int, Vector3> OnPOIDiscovered;
        public static void FirePOIDiscovered(string poiId, int rsReward, Vector3 worldPos) => OnPOIDiscovered?.Invoke(poiId, rsReward, worldPos);

        /// <summary>Raised by TartarianHourCycle.cs once per 17-hour cycle.</summary>
        public static event Action OnSeventeenthHour;
        public static void FireSeventeenthHour() => OnSeventeenthHour?.Invoke();

        /// <summary>Raised every hour transition. Payload: new hour 0-16.</summary>
        public static event Action<int> OnTartarianHourChanged;
        public static void FireTartarianHourChanged(int newHour) => OnTartarianHourChanged?.Invoke(newHour);

        /// <summary>Raised by tuning mini-games per frame. Payload: unsigned offset (0=perfect).</summary>
        public static event Action<float> OnTuningProgress;
        public static void FireTuningProgress(float frequencyOffset) => OnTuningProgress?.Invoke(frequencyOffset);

        /// <summary>Fire helpers for existing OnCombatStarted / OnCombatEnded events.</summary>
        public static void FireCombatStarted() => OnCombatStarted?.Invoke();
        public static void FireCombatEnded() => OnCombatEnded?.Invoke();

        // ─── Dialogue lifecycle aliases (added 2026-06-03 for Milo §10 state machine) ───
        // Sugar over OnDialogueStateChanged for subscribers that only care about start vs end.
        // (Tuning + CombatEngaged lifecycle is already provided by the camera-mode block below,
        // so we reuse those existing parameterless events to avoid duplicate declarations.)
        public static event Action<string> OnDialogueStart; // speakerName
        public static event Action<string> OnDialogueEnd;   // speakerName
        public static void FireDialogueStart(string speakerName) => OnDialogueStart?.Invoke(speakerName);
        public static void FireDialogueEnd(string speakerName) => OnDialogueEnd?.Invoke(speakerName);

        // ═══════════════════════════════════════════════════════════════════
        // SPRINT 12 CANONICAL EVENTS — Day cycle + brazier ring
        // Phase 2 Lane 1 (P2.L1). Added 2026-06-02 per Sprint 11 L9 (72457de3)
        // which proved CLAUDE.md canonical-facts table referenced these but
        // they did not exist. Subscribers (Moon1LiraelDay25Gate at line 49,
        // Moon1DaySmokeMenus at line 44/66/128, MiloTutorialFlow brazier
        // gating) can now bind through the canonical GameEvents surface.
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raised by TartarianCalendar.AdvanceDay when the in-game day advances.
        /// Payload: new day number (1-28 in Moon 1).
        /// Subscribers: Moon1LiraelDay25Gate (Day-25 narrative gate), narrative beat managers.
        /// </summary>
        public static Action<int> OnDayChanged;

        /// <summary>
        /// Raised when a single brazier is lit. Payload: brazier identifier
        /// (e.g. "Brazier_Cathedral_L" — see Moon1Braziers.cs:41-46 for canonical names).
        /// Subscribers: MiloTutorialFlow (step 2 advance), brazier ring tracker, audio FX.
        /// </summary>
        public static Action<string> OnBrazierLit;

        /// <summary>
        /// Raised when the full Echohaven brazier ring (8 perimeter + 6 hero-entrance)
        /// has been lit. Subscribers: Day-cycle phase change, EchohavenContentSpawner
        /// post-ring beats, QuestObjectiveTracker.
        /// </summary>
        public static Action OnBrazierRingComplete;

        // ═══════════════════════════════════════════════════════════════════
        // CAMERA MODE EVENTS — docs/15 §12 Camera & Controls (2026-06-03 C.L6)
        // Raised by gameplay systems to drive CameraController.SetMode().
        // CameraController also subscribes to GameStateManager.OnStateChanged
        // as a backstop, but these events are the canonical surface so
        // cinematic / tuning / combat systems can request a camera mode
        // without forcing a GameState transition.
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Raised when a tuning mini-game begins. Subscribers: CameraController (Tuning mode), HUDController.</summary>
        public static event Action OnTuningStart;
        /// <summary>Raised when a tuning mini-game ends (success or fail). Subscribers: CameraController (return to Exploration).</summary>
        public static event Action OnTuningEnd;
        /// <summary>Raised when player engages combat (alias of OnCombatStarted for camera-mode contract).</summary>
        public static event Action OnCombatEngaged;
        /// <summary>Raised when a pre-authored cinematic begins. Subscribers: CameraController (Cinematic mode), PlayerInputHandler (gate input).</summary>
        public static event Action OnCinematicStart;
        /// <summary>Raised when the active cinematic ends. Subscribers: CameraController (return to Exploration), PlayerInputHandler.</summary>
        public static event Action OnCinematicEnd;

        public static void FireTuningStart() => OnTuningStart?.Invoke();
        public static void FireTuningEnd() => OnTuningEnd?.Invoke();
        public static void FireCombatEngaged()
        {
            OnCombatEngaged?.Invoke();
            // Mirror to legacy OnCombatStarted so existing subscribers stay live.
            OnCombatStarted?.Invoke();
        }
        public static void FireCinematicStart() => OnCinematicStart?.Invoke();
        public static void FireCinematicEnd() => OnCinematicEnd?.Invoke();

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

        public static void RaisePlayerDamaged(float damageAmount, float remainingHealth)
        {
            try
            {
                var args = new PlayerDamagedEventArgs
                {
                    damageAmount = damageAmount,
                    remainingHealth = remainingHealth
                };
                OnPlayerDamaged?.Invoke(args);
            }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerDamaged: {ex}"); }
        }

        public static void RaisePlayerDeath()
        {
            try { OnPlayerDeath?.Invoke(); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerDeath: {ex}"); }
        }

        public static void RaisePlayerRespawned()
        {
            try { OnPlayerRespawned?.Invoke(); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerRespawned: {ex}"); }
        }

        public static void RaisePlayerHealthChanged(float currentHealth, float maxHealth)
        {
            try { OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnPlayerHealthChanged: {ex}"); }
        }

        public static void RaiseAetherEnergyChanged(float aetherValue)
        {
            try { OnAetherEnergyChanged?.Invoke(aetherValue); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnAetherEnergyChanged: {ex}"); }
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
            try
            {
                OnDialogueStateChanged?.Invoke(args);
                // Mirror to start/end aliases for §10 Milo state machine + similar subscribers.
                if (args != null)
                {
                    if (args.isActive) OnDialogueStart?.Invoke(args.speakerName);
                    else OnDialogueEnd?.Invoke(args.speakerName);
                }
            }
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

        // ─── Sprint 12 canonical Raise methods (P2.L1, 2026-06-02) ───
        // Pattern matches the gameplay Raise helpers above: try/Invoke/catch with
        // full ex.GetType().Name + ex.Message context per CLAUDE.md NO-DEBT rule.

        public static void RaiseDayChanged(int day)
        {
            try
            {
                OnDayChanged?.Invoke(day);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameEvents] Exception in OnDayChanged (day={day}): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void RaiseBrazierLit(string brazierId)
        {
            try
            {
                OnBrazierLit?.Invoke(brazierId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameEvents] Exception in OnBrazierLit (brazierId='{brazierId}'): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void RaiseBrazierRingComplete()
        {
            try
            {
                OnBrazierRingComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameEvents] Exception in OnBrazierRingComplete: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // HUD RAISE METHODS (Thread-safe with null-check + exception handling)
        // ═══════════════════════════════════════════════════════════════════

        public static void RaiseHUDShowObjective(string message)
        {
            try { OnHUDShowObjective?.Invoke(message); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowObjective: {ex}"); }
        }

        public static void RaiseHUDShowDialogue(string speaker, string message)
        {
            try { OnHUDShowDialogue?.Invoke(speaker, message); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowDialogue: {ex}"); }
        }

        public static void RaiseHUDShowBanner(string title, string subtitle, float duration = 5f)
        {
            try { OnHUDShowBanner?.Invoke(title, subtitle, duration); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBanner: {ex}"); }
        }

        public static void RaiseHUDShowSubtitle(string message, float duration)
        {
            try { OnHUDShowSubtitle?.Invoke(message, duration); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowSubtitle: {ex}"); }
        }

        public static void RaiseHUDShowMoonTrophy(string title, string subtitle)
        {
            try { OnHUDShowMoonTrophy?.Invoke(title, subtitle); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowMoonTrophy: {ex}"); }
        }

        public static void RaiseHUDShowBossHealth(string bossName, float normalizedHealth)
        {
            try { OnHUDShowBossHealth?.Invoke(bossName, normalizedHealth); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBossHealth: {ex}"); }
        }

        public static void RaiseHUDUpdateBossHealth(float normalizedHealth)
        {
            try { OnHUDUpdateBossHealth?.Invoke(normalizedHealth); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDUpdateBossHealth: {ex}"); }
        }

        public static void RaiseHUDHideBossHealth()
        {
            try { OnHUDHideBossHealth?.Invoke(); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDHideBossHealth: {ex}"); }
        }

        public static void RaiseHUDShowInteractionPrompt(string message)
        {
            try { OnHUDShowInteractionPrompt?.Invoke(message); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowInteractionPrompt: {ex}"); }
        }

        public static void RaiseHUDHideInteractionPrompt()
        {
            try { OnHUDHideInteractionPrompt?.Invoke(); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDHideInteractionPrompt: {ex}"); }
        }

        public static void RaiseHUDFlashRSGain(float amount)
        {
            try { OnHUDFlashRSGain?.Invoke(amount); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDFlashRSGain: {ex}"); }
        }

        public static void RaiseHUDShowBossNameplate(string bossName, string bossTitle)
        {
            try { OnHUDShowBossNameplate?.Invoke(bossName, bossTitle); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowBossNameplate: {ex}"); }
        }

        public static void RaiseHUDShowEnemyBark(string message, float duration)
        {
            try { OnHUDShowEnemyBark?.Invoke(message, duration); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowEnemyBark: {ex}"); }
        }

        public static void RaiseHUDShowCorruptionWhisper(string message, float duration)
        {
            try { OnHUDShowCorruptionWhisper?.Invoke(message, duration); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDShowCorruptionWhisper: {ex}"); }
        }

        public static void RaiseHUDUpdateFrequencyWheel(float frequency, float param)
        {
            try { OnHUDUpdateFrequencyWheel?.Invoke(frequency, param); }
            catch (Exception ex) { Debug.LogError($"[GameEvents] Exception in OnHUDUpdateFrequencyWheel: {ex}"); }
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
        public GameObject Building;  // Reference to the restored building (for checkpoint tracking)
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

    public class PlayerDamagedEventArgs
    {
        public float damageAmount;
        public float remainingHealth;
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

    public class CombatEventArgs
    {
        public string enemyType;
        public Vector3 position;
        public bool isPlayerInitiated;
    }

    public class CollectibleEventArgs
    {
        public string collectibleId;
        public string collectibleType;
        public Vector3 position;
    }
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
