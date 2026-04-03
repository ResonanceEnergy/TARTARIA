using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Save;
using Tartaria.UI;
using Tartaria.Input;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Central gameplay orchestrator — the nervous system that wires every
    /// isolated manager into a coherent gameplay loop.
    ///
    /// Each frame:
    ///   1. Read ResonanceScore from ECS singleton
    ///   2. Push RS → HUD, Music, Save, VFX palette
    ///   3. Detect RS threshold crossings → trigger events
    ///   4. Monitor GameState transitions → dispatch to all systems
    ///   5. Sync player ECS state ↔ MonoBehaviour world
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)] // Run before most MonoBehaviours
    public class GameLoopController : MonoBehaviour, IGameLoopService
    {
        public static GameLoopController Instance { get; private set; }

        [Header("References")]
        [SerializeField] PlayerInputHandler playerInput;
        [SerializeField] Camera.CameraController cameraController;

        // Cached ECS references
        World _ecsWorld;
        EntityManager _em;
        Entity _rsEntity;
        Entity _playerEntity;
        EntityQuery _rsQuery;
        EntityQuery _playerQuery;
        bool _rsQueryCreated;
        bool _playerQueryCreated;
        bool _ecsReady;

        // Cached scene queries
        InteractableBuilding[] _cachedBuildings = System.Array.Empty<InteractableBuilding>();
        float _buildingCacheAge = float.MaxValue;
        const float BUILDING_CACHE_TTL = 2f;

        // RS tracking
        float _lastRS;
        int _lastThreshold;
        float _rsUpdateTimer;
        const float RS_POLL_INTERVAL = 0.1f; // 10Hz is enough for UI

        // Save sync
        float _saveSyncTimer;
        const float SAVE_SYNC_INTERVAL = 5f;

        // Moon tracking
        int _currentMoonIndex;

        // Cached non-singleton mini-games
        AetherConduitMiniGame _conduitMiniGame;
        OrphanTrainPuzzle _orphanTrainPuzzle;

        // Victory
        bool _zoneVictoryTriggered;

        // Buff tracking
        float _rsBuffTimer;
        const float RS_BUFF_MULTIPLIER = 1.25f;
        const float RS_BUFF_DURATION = 60f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.GameLoop = this;
            GameEvents.OnRequestActivateRSBuff += HandleRequestActivateRSBuff;
        }

        void Start()
        {
            // Subscribe to state changes
            GameStateManager.Instance.OnStateChanged += OnGameStateChanged;

            // Wire player combat events → CombatBridge
            if (playerInput != null)
            {
                playerInput.OnResonancePulse += HandleResonancePulse;
                playerInput.OnHarmonicStrike += HandleHarmonicStrike;
                playerInput.OnFrequencyShield += HandleFrequencyShield;
            }

            // Wire save events for subsystem sync
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnBeforeSave;
                SaveManager.Instance.OnAfterLoad += OnAfterLoad;
            }

            // Wire Workshop UI → WorkshopSystem
            if (WorkshopUIPanel.Instance != null)
                WorkshopUIPanel.Instance.OnUpgradeRequested += HandleWorkshopUpgrade;

            // Wire tutorial completion → first quest unlock
            if (TutorialSystem.Instance != null)
                TutorialSystem.Instance.OnTutorialComplete += HandleTutorialComplete;

            // Wire boss defeated → climax trigger + quest advancement
            if (BossEncounterSystem.Instance != null)
            {
                BossEncounterSystem.Instance.OnBossDefeated += HandleBossDefeated;
                BossEncounterSystem.Instance.OnBossHealthChanged += HandleBossHealthChanged;
            }

            // Wire climax events → HUD + state
            if (ClimaxSequenceSystem.Instance != null)
                ClimaxSequenceSystem.Instance.OnClimaxCompleted += HandleClimaxCompleted;

            // Wire combat wave events → HUD + state
            var waveManager = CombatWaveManager.Instance;
            if (waveManager != null)
            {
                waveManager.OnWaveStarted += HandleWaveStarted;
                waveManager.OnWaveCleared += HandleWaveCleared;
                waveManager.OnAllWavesCleared += HandleAllWavesCleared;
            }

            // Wire achievement events → HUD toast + notifications
            if (AchievementSystem.Instance != null)
                AchievementSystem.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;

            // Wire companion trust changes → notifications
            if (CompanionManager.Instance != null)
                CompanionManager.Instance.OnTrustChanged += HandleCompanionTrustChanged;

            // Wire mini-game failure events → HUD feedback
            if (ChoirHarmonicsMiniGame.Instance != null)
                ChoirHarmonicsMiniGame.Instance.OnPerformanceFailed += HandleChoirFailed;
            if (CosmicConvergenceMiniGame.Instance != null)
                CosmicConvergenceMiniGame.Instance.OnConvergenceFailed += HandleConvergenceFailed;

            // Wire ley line restoration → RS reward + VFX
            if (LeyLineManager.Instance != null)
                LeyLineManager.Instance.OnLineRestored += HandleLeyLineRestored;

            // Wire Day Out of Time memory zone transitions → HUD + atmosphere
            if (DayOutOfTimeController.Instance != null)
                DayOutOfTimeController.Instance.OnMemoryZoneChanged += HandleMemoryZoneChanged;

            // Wire economy events → HUD currency display
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnCurrencyChanged += HandleCurrencyChanged;

            // Wire crafting events → HUD toast + VFX + audio stinger
            if (CraftingSystem.Instance != null)
            {
                CraftingSystem.Instance.OnItemCrafted += HandleItemCrafted;
                CraftingSystem.Instance.OnRecipeDiscovered += HandleRecipeDiscovered;
                CraftingSystem.Instance.OnCraftFailed += HandleCraftFailed;
                CraftingSystem.Instance.OnItemUsed += HandleItemUsed;
                CraftingSystem.Instance.OnItemCollected += HandleItemCollected;
            }

            // Wire corruption events → VFX + HUD + haptic
            if (CorruptionSystem.Instance != null)
            {
                CorruptionSystem.Instance.OnCorruptionChanged += HandleCorruptionChanged;
                CorruptionSystem.Instance.OnCorruptionPurged += HandleCorruptionPurged;
                CorruptionSystem.Instance.OnCorruptionSpread += HandleCorruptionSpread;
            }

            // Wire excavation events → VFX + HUD + audio + quest
            if (ExcavationSystem.Instance != null)
            {
                ExcavationSystem.Instance.OnSiteDiscovered += HandleSiteDiscovered;
                ExcavationSystem.Instance.OnLayerCleared += HandleLayerCleared;
                ExcavationSystem.Instance.OnExcavationComplete += HandleExcavationComplete;
                ExcavationSystem.Instance.OnRSYielded += HandleExcavationRS;
            }

            // Wire dialogue tree events → camera + music + quest state
            if (DialogueTreeRunner.Instance != null)
            {
                DialogueTreeRunner.Instance.OnDialogueStarted += HandleDialogueStarted;
                DialogueTreeRunner.Instance.OnDialogueEnded += HandleDialogueEnded;
                DialogueTreeRunner.Instance.OnChoiceMade += HandleChoiceMade;
            }

            // Wire boss spawn/phase/dialogue events → HUD + VFX + music + haptic
            if (BossEncounterSystem.Instance != null)
            {
                BossEncounterSystem.Instance.OnBossSpawned += HandleBossSpawned;
                BossEncounterSystem.Instance.OnPhaseChanged += HandleBossPhaseChanged;
                BossEncounterSystem.Instance.OnBossDialogue += HandleBossDialogue;
            }

            // Wire mini-game completion events → RS rewards + VFX + achievement
            if (HarmonicRockCutting.Instance != null)
            {
                HarmonicRockCutting.Instance.OnCutComplete += HandleRockCutComplete;
                HarmonicRockCutting.Instance.OnCutFailed += HandleRockCutFailed;
            }
            if (PipeOrganMiniGame.Instance != null)
            {
                PipeOrganMiniGame.Instance.OnOrganComplete += HandleOrganComplete;
                PipeOrganMiniGame.Instance.OnOrganFailed += HandleOrganFailed;
            }

            // Wire workshop upgrade completion → VFX + economy + achievement
            if (WorkshopSystem.Instance != null)
                WorkshopSystem.Instance.OnBuildingUpgraded += HandleBuildingUpgraded;

            // Wire campaign moon start + ending → zone setup + music + save
            if (CampaignFlowController.Instance != null)
            {
                CampaignFlowController.Instance.OnMoonStarted += HandleMoonStarted;
                CampaignFlowController.Instance.OnEndingChosen += HandleEndingChosen;
            }

            // Wire Anastasia narrative events → camera + music + VFX
            if (AnastasiaController.Instance != null)
            {
                AnastasiaController.Instance.OnModeChanged += HandleAnastasiaMode;
                AnastasiaController.Instance.OnSolidificationPhaseChanged += HandleAnastasiaSolidification;
                AnastasiaController.Instance.OnLineDelivered += HandleAnastasiaLine;
            }

            // Wire world map codex unlock → achievement + HUD
            if (CodexSystem.Instance != null)
                CodexSystem.Instance.OnEntryUnlocked += HandleCodexEntryUnlocked;

            // Wire Cassian intel events → quest + HUD
            if (CassianNPCController.Instance != null)
                CassianNPCController.Instance.OnIntelShared += HandleCassianIntel;

            // Wire ley line node events → per-node VFX
            if (LeyLineManager.Instance != null)
            {
                LeyLineManager.Instance.OnNodeActivated += HandleNodeActivated;
                LeyLineManager.Instance.OnNodeSevered += HandleNodeSevered;
            }

            // Wire airship fleet events → HUD + VFX + music
            if (AirshipFleetManager.Instance != null)
            {
                AirshipFleetManager.Instance.OnAirshipRestored += HandleAirshipRestored;
                AirshipFleetManager.Instance.OnMercuryOrbTuned += HandleMercuryOrbTuned;
                AirshipFleetManager.Instance.OnFormationChanged += HandleFormationChanged;
                AirshipFleetManager.Instance.OnFleetOperational += HandleFleetOperational;
            }

            // Wire choir harmonics events → haptic + music + VFX
            if (ChoirHarmonicsMiniGame.Instance != null)
            {
                ChoirHarmonicsMiniGame.Instance.OnVoiceEntered += HandleVoiceEntered;
                ChoirHarmonicsMiniGame.Instance.OnVoiceDrifted += HandleVoiceDrifted;
                ChoirHarmonicsMiniGame.Instance.OnFullHarmonyAchieved += HandleFullHarmony;
                ChoirHarmonicsMiniGame.Instance.OnTranscendentMoment += HandleTranscendentMoment;
            }

            // Wire aquifer purge events → VFX + haptic
            if (AquiferPurgeMiniGame.Instance != null)
            {
                AquiferPurgeMiniGame.Instance.OnLayerPurged += HandleAquiferLayerPurged;
                AquiferPurgeMiniGame.Instance.OnBossSpawned += HandleAquiferBossSpawned;
            }

            // Wire Aether field RS changes → HUD sync
            if (AetherFieldManager.Instance != null)
                AetherFieldManager.Instance.OnResonanceScoreChanged += HandleAetherRSChanged;

            // Wire economy building income → HUD toast
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnBuildingIncomeCollected += HandleBuildingIncome;

            // Wire achievement progress → HUD
            if (AchievementSystem.Instance != null)
                AchievementSystem.Instance.OnProgressUpdated += HandleAchievementProgress;

            // Wire localization changes (static class) → UI refresh
            LocalizationManager.OnLanguageChanged += HandleLanguageChanged;

            // Wire dissonance lens toggle → music + VFX
            if (DissonanceLensOverlay.Instance != null)
                DissonanceLensOverlay.Instance.OnLensToggled += HandleLensToggled;

            // Wire aether conduit mini-game (no singleton — find by type)
            _conduitMiniGame = FindAnyObjectByType<AetherConduitMiniGame>();
            if (_conduitMiniGame != null)
            {
                _conduitMiniGame.OnBastionPlaced += HandleBastionPlaced;
                _conduitMiniGame.OnRockCut += HandleRockCut;
                _conduitMiniGame.OnResonancePeak += HandleConduitResonancePeak;
            }

            // Wire orphan train puzzle (no singleton — find by type)
            _orphanTrainPuzzle = FindAnyObjectByType<OrphanTrainPuzzle>();
            if (_orphanTrainPuzzle != null)
            {
                _orphanTrainPuzzle.OnSegmentAligned += HandleTrainSegmentAligned;
                _orphanTrainPuzzle.OnChainBroken += HandleTrainChainBroken;
            }

            // Wire harmonic rock cutting mini-game
            if (HarmonicRockCutting.Instance != null)
            {
                HarmonicRockCutting.Instance.OnVeinTraced += HandleVeinTraced;
                HarmonicRockCutting.Instance.OnComboChanged += HandleRockComboChanged;
            }

            // Wire rail alignment mini-game
            if (RailAlignmentMiniGame.Instance != null)
            {
                RailAlignmentMiniGame.Instance.OnSegmentRotated += HandleSegmentRotated;
                RailAlignmentMiniGame.Instance.OnFlowChanged += HandleFlowChanged;
            }

            // Wire pipe organ mini-game
            if (PipeOrganMiniGame.Instance != null)
            {
                PipeOrganMiniGame.Instance.OnPipePlayed += HandlePipePlayed;
                PipeOrganMiniGame.Instance.OnChordAdvanced += HandleChordAdvanced;
            }

            // Wire boss-fail event
            if (BossEncounterSystem.Instance != null)
                BossEncounterSystem.Instance.OnBossFailed += HandleBossFailed;

            // Wire climax-start event
            if (ClimaxSequenceSystem.Instance != null)
                ClimaxSequenceSystem.Instance.OnClimaxStarted += HandleClimaxStarted;

            // Wire resonance scanner events → HUD + map markers + sound
            if (ResonanceScannerSystem.Instance != null)
            {
                ResonanceScannerSystem.Instance.OnScanStarted += HandleScanStarted;
                ResonanceScannerSystem.Instance.OnScanComplete += HandleScanComplete;
                ResonanceScannerSystem.Instance.OnPOIRevealed += HandlePOIRevealed;
            }

            // Wire continental rail events → quest + music + haptic
            if (ContinentalRailSystem.Instance != null)
            {
                ContinentalRailSystem.Instance.OnSegmentRestored += HandleRailSegmentRestored;
                ContinentalRailSystem.Instance.OnStationDiscovered += HandleStationDiscovered;
                ContinentalRailSystem.Instance.OnTrainDeparted += HandleTrainDeparted;
                ContinentalRailSystem.Instance.OnTrainArrived += HandleTrainArrived;
                ContinentalRailSystem.Instance.OnRailLeviathan += HandleRailLeviathan;
            }

            // Wire bell tower sync mini-game events → RS + VFX + haptic
            if (BellTowerSyncMiniGame.Instance != null)
            {
                BellTowerSyncMiniGame.Instance.OnTowerTuned += HandleTowerTuned;
                BellTowerSyncMiniGame.Instance.OnTowerSynced += HandleTowerSynced;
                BellTowerSyncMiniGame.Instance.OnTowerDesynced += HandleTowerDesynced;
                BellTowerSyncMiniGame.Instance.OnResonanceScoreChanged += HandleBellTowerRSChanged;
            }

            // Deferred ECS init (world may not be ready in Awake)
            InitECS();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (Instance == this) Instance = null;
            if (_rsQueryCreated) { _rsQuery.Dispose(); _rsQueryCreated = false; }
            if (_playerQueryCreated) { _playerQuery.Dispose(); _playerQueryCreated = false; }

            GameEvents.OnRequestActivateRSBuff -= HandleRequestActivateRSBuff;
            GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
            if (playerInput != null)
            {
                playerInput.OnResonancePulse -= HandleResonancePulse;
                playerInput.OnHarmonicStrike -= HandleHarmonicStrike;
                playerInput.OnFrequencyShield -= HandleFrequencyShield;
            }
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnBeforeSave;
                SaveManager.Instance.OnAfterLoad -= OnAfterLoad;
            }
            if (WorkshopUIPanel.Instance != null)
                WorkshopUIPanel.Instance.OnUpgradeRequested -= HandleWorkshopUpgrade;
            if (TutorialSystem.Instance != null)
                TutorialSystem.Instance.OnTutorialComplete -= HandleTutorialComplete;
            if (BossEncounterSystem.Instance != null)
            {
                BossEncounterSystem.Instance.OnBossDefeated -= HandleBossDefeated;
                BossEncounterSystem.Instance.OnBossHealthChanged -= HandleBossHealthChanged;
            }
            if (ClimaxSequenceSystem.Instance != null)
                ClimaxSequenceSystem.Instance.OnClimaxCompleted -= HandleClimaxCompleted;
            var waveManager = CombatWaveManager.Instance;
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= HandleWaveStarted;
                waveManager.OnWaveCleared -= HandleWaveCleared;
                waveManager.OnAllWavesCleared -= HandleAllWavesCleared;
            }
            if (AchievementSystem.Instance != null)
                AchievementSystem.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
            if (CompanionManager.Instance != null)
                CompanionManager.Instance.OnTrustChanged -= HandleCompanionTrustChanged;
            if (ChoirHarmonicsMiniGame.Instance != null)
                ChoirHarmonicsMiniGame.Instance.OnPerformanceFailed -= HandleChoirFailed;
            if (CosmicConvergenceMiniGame.Instance != null)
                CosmicConvergenceMiniGame.Instance.OnConvergenceFailed -= HandleConvergenceFailed;
            if (LeyLineManager.Instance != null)
                LeyLineManager.Instance.OnLineRestored -= HandleLeyLineRestored;
            if (DayOutOfTimeController.Instance != null)
                DayOutOfTimeController.Instance.OnMemoryZoneChanged -= HandleMemoryZoneChanged;
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            if (CraftingSystem.Instance != null)
            {
                CraftingSystem.Instance.OnItemCrafted -= HandleItemCrafted;
                CraftingSystem.Instance.OnRecipeDiscovered -= HandleRecipeDiscovered;
                CraftingSystem.Instance.OnCraftFailed -= HandleCraftFailed;
                CraftingSystem.Instance.OnItemUsed -= HandleItemUsed;
                CraftingSystem.Instance.OnItemCollected -= HandleItemCollected;
            }
            if (CorruptionSystem.Instance != null)
            {
                CorruptionSystem.Instance.OnCorruptionChanged -= HandleCorruptionChanged;
                CorruptionSystem.Instance.OnCorruptionPurged -= HandleCorruptionPurged;
                CorruptionSystem.Instance.OnCorruptionSpread -= HandleCorruptionSpread;
            }
            if (ExcavationSystem.Instance != null)
            {
                ExcavationSystem.Instance.OnSiteDiscovered -= HandleSiteDiscovered;
                ExcavationSystem.Instance.OnLayerCleared -= HandleLayerCleared;
                ExcavationSystem.Instance.OnExcavationComplete -= HandleExcavationComplete;
                ExcavationSystem.Instance.OnRSYielded -= HandleExcavationRS;
            }
            if (DialogueTreeRunner.Instance != null)
            {
                DialogueTreeRunner.Instance.OnDialogueStarted -= HandleDialogueStarted;
                DialogueTreeRunner.Instance.OnDialogueEnded -= HandleDialogueEnded;
                DialogueTreeRunner.Instance.OnChoiceMade -= HandleChoiceMade;
            }
            if (BossEncounterSystem.Instance != null)
            {
                BossEncounterSystem.Instance.OnBossSpawned -= HandleBossSpawned;
                BossEncounterSystem.Instance.OnPhaseChanged -= HandleBossPhaseChanged;
                BossEncounterSystem.Instance.OnBossDialogue -= HandleBossDialogue;
            }
            if (HarmonicRockCutting.Instance != null)
            {
                HarmonicRockCutting.Instance.OnCutComplete -= HandleRockCutComplete;
                HarmonicRockCutting.Instance.OnCutFailed -= HandleRockCutFailed;
            }
            if (PipeOrganMiniGame.Instance != null)
            {
                PipeOrganMiniGame.Instance.OnOrganComplete -= HandleOrganComplete;
                PipeOrganMiniGame.Instance.OnOrganFailed -= HandleOrganFailed;
            }
            if (WorkshopSystem.Instance != null)
                WorkshopSystem.Instance.OnBuildingUpgraded -= HandleBuildingUpgraded;
            if (CampaignFlowController.Instance != null)
            {
                CampaignFlowController.Instance.OnMoonStarted -= HandleMoonStarted;
                CampaignFlowController.Instance.OnEndingChosen -= HandleEndingChosen;
            }
            if (AnastasiaController.Instance != null)
            {
                AnastasiaController.Instance.OnModeChanged -= HandleAnastasiaMode;
                AnastasiaController.Instance.OnSolidificationPhaseChanged -= HandleAnastasiaSolidification;
                AnastasiaController.Instance.OnLineDelivered -= HandleAnastasiaLine;
            }
            if (CodexSystem.Instance != null)
                CodexSystem.Instance.OnEntryUnlocked -= HandleCodexEntryUnlocked;
            if (CassianNPCController.Instance != null)
                CassianNPCController.Instance.OnIntelShared -= HandleCassianIntel;
            if (LeyLineManager.Instance != null)
            {
                LeyLineManager.Instance.OnNodeActivated -= HandleNodeActivated;
                LeyLineManager.Instance.OnNodeSevered -= HandleNodeSevered;
            }
            if (AirshipFleetManager.Instance != null)
            {
                AirshipFleetManager.Instance.OnAirshipRestored -= HandleAirshipRestored;
                AirshipFleetManager.Instance.OnMercuryOrbTuned -= HandleMercuryOrbTuned;
                AirshipFleetManager.Instance.OnFormationChanged -= HandleFormationChanged;
                AirshipFleetManager.Instance.OnFleetOperational -= HandleFleetOperational;
            }
            if (ChoirHarmonicsMiniGame.Instance != null)
            {
                ChoirHarmonicsMiniGame.Instance.OnVoiceEntered -= HandleVoiceEntered;
                ChoirHarmonicsMiniGame.Instance.OnVoiceDrifted -= HandleVoiceDrifted;
                ChoirHarmonicsMiniGame.Instance.OnFullHarmonyAchieved -= HandleFullHarmony;
                ChoirHarmonicsMiniGame.Instance.OnTranscendentMoment -= HandleTranscendentMoment;
            }
            if (AquiferPurgeMiniGame.Instance != null)
            {
                AquiferPurgeMiniGame.Instance.OnLayerPurged -= HandleAquiferLayerPurged;
                AquiferPurgeMiniGame.Instance.OnBossSpawned -= HandleAquiferBossSpawned;
            }
            if (AetherFieldManager.Instance != null)
                AetherFieldManager.Instance.OnResonanceScoreChanged -= HandleAetherRSChanged;
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnBuildingIncomeCollected -= HandleBuildingIncome;
            if (AchievementSystem.Instance != null)
                AchievementSystem.Instance.OnProgressUpdated -= HandleAchievementProgress;
            LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
            if (DissonanceLensOverlay.Instance != null)
                DissonanceLensOverlay.Instance.OnLensToggled -= HandleLensToggled;
            if (_conduitMiniGame != null)
            {
                _conduitMiniGame.OnBastionPlaced -= HandleBastionPlaced;
                _conduitMiniGame.OnRockCut -= HandleRockCut;
                _conduitMiniGame.OnResonancePeak -= HandleConduitResonancePeak;
            }
            if (_orphanTrainPuzzle != null)
            {
                _orphanTrainPuzzle.OnSegmentAligned -= HandleTrainSegmentAligned;
                _orphanTrainPuzzle.OnChainBroken -= HandleTrainChainBroken;
            }
            if (HarmonicRockCutting.Instance != null)
            {
                HarmonicRockCutting.Instance.OnVeinTraced -= HandleVeinTraced;
                HarmonicRockCutting.Instance.OnComboChanged -= HandleRockComboChanged;
            }
            if (RailAlignmentMiniGame.Instance != null)
            {
                RailAlignmentMiniGame.Instance.OnSegmentRotated -= HandleSegmentRotated;
                RailAlignmentMiniGame.Instance.OnFlowChanged -= HandleFlowChanged;
            }
            if (PipeOrganMiniGame.Instance != null)
            {
                PipeOrganMiniGame.Instance.OnPipePlayed -= HandlePipePlayed;
                PipeOrganMiniGame.Instance.OnChordAdvanced -= HandleChordAdvanced;
            }
            if (BossEncounterSystem.Instance != null)
                BossEncounterSystem.Instance.OnBossFailed -= HandleBossFailed;
            if (ClimaxSequenceSystem.Instance != null)
                ClimaxSequenceSystem.Instance.OnClimaxStarted -= HandleClimaxStarted;
            if (ResonanceScannerSystem.Instance != null)
            {
                ResonanceScannerSystem.Instance.OnScanStarted -= HandleScanStarted;
                ResonanceScannerSystem.Instance.OnScanComplete -= HandleScanComplete;
                ResonanceScannerSystem.Instance.OnPOIRevealed -= HandlePOIRevealed;
            }
            if (ContinentalRailSystem.Instance != null)
            {
                ContinentalRailSystem.Instance.OnSegmentRestored -= HandleRailSegmentRestored;
                ContinentalRailSystem.Instance.OnStationDiscovered -= HandleStationDiscovered;
                ContinentalRailSystem.Instance.OnTrainDeparted -= HandleTrainDeparted;
                ContinentalRailSystem.Instance.OnTrainArrived -= HandleTrainArrived;
                ContinentalRailSystem.Instance.OnRailLeviathan -= HandleRailLeviathan;
            }
            if (BellTowerSyncMiniGame.Instance != null)
            {
                BellTowerSyncMiniGame.Instance.OnTowerTuned -= HandleTowerTuned;
                BellTowerSyncMiniGame.Instance.OnTowerSynced -= HandleTowerSynced;
                BellTowerSyncMiniGame.Instance.OnTowerDesynced -= HandleTowerDesynced;
                BellTowerSyncMiniGame.Instance.OnResonanceScoreChanged -= HandleBellTowerRSChanged;
            }
        }

        void InitECS()
        {
            _ecsWorld = World.DefaultGameObjectInjectionWorld;
            if (_ecsWorld == null) return;
            _em = _ecsWorld.EntityManager;

            // Create queries once and reuse
            if (!_rsQueryCreated)
            {
                _rsQuery = _em.CreateEntityQuery(typeof(ResonanceScore));
                _rsQueryCreated = true;
            }
            if (!_playerQueryCreated)
            {
                _playerQuery = _em.CreateEntityQuery(typeof(PlayerTag), typeof(LocalTransform));
                _playerQueryCreated = true;
            }

            // Find ResonanceScore singleton
            if (_rsQuery.CalculateEntityCount() > 0)
            {
                _rsEntity = _rsQuery.GetSingletonEntity();
                _ecsReady = true;
            }

            // Find player entity for position sync
            if (_playerQuery.CalculateEntityCount() > 0)
                _playerEntity = _playerQuery.GetSingletonEntity();
        }

        void Update()
        {
            if (!_ecsReady) { InitECS(); return; }

            PollResonanceScore();
            SyncRSModifiers();
            SyncPlayerPositionToECS();
            SyncSaveData();
        }

        // ─── RS Modifier Sync ─────────────────────────

        /// <summary>
        /// Push SkillTree RS multiplier and Moon RS multiplier into
        /// the ECS ResonanceScore component (Burst can't call managed code).
        /// </summary>
        void SyncRSModifiers()
        {
            if (!_em.Exists(_rsEntity)) return;

            var rs = _em.GetComponentData<ResonanceScore>(_rsEntity);

            // SkillTree RS multiplier (base 1.0 + modifier value)
            float skillMod = 1f;
            if (SkillTreeSystem.Instance != null)
                skillMod = 1f + SkillTreeSystem.Instance.GetModifier(SkillModifierType.RSMultiplier);

            // Moon campaign RS multiplier
            float moonMod = MoonModifierProvider.Active.rsGainMultiplier;

            // Buff timer countdown
            float buffMod = 1f;
            if (_rsBuffTimer > 0f)
            {
                _rsBuffTimer -= Time.deltaTime;
                buffMod = RS_BUFF_MULTIPLIER;
                if (_rsBuffTimer <= 0f)
                {
                    _rsBuffTimer = 0f;
                    HUDController.Instance?.ShowInteractionPrompt("Resonance Amplifier expired.");
                }
            }

            if (Mathf.Abs(rs.SkillRSMultiplier - skillMod) > 0.001f
                || Mathf.Abs(rs.MoonRSMultiplier - moonMod) > 0.001f
                || Mathf.Abs(rs.BuffRSMultiplier - buffMod) > 0.001f)
            {
                rs.SkillRSMultiplier = skillMod;
                rs.MoonRSMultiplier = moonMod;
                rs.BuffRSMultiplier = buffMod;
                _em.SetComponentData(_rsEntity, rs);
            }
        }

        // ─── RS Polling & Distribution ───────────────

        void PollResonanceScore()
        {
            _rsUpdateTimer += Time.deltaTime;
            if (_rsUpdateTimer < RS_POLL_INTERVAL) return;
            _rsUpdateTimer = 0f;

            if (!_em.Exists(_rsEntity)) { _ecsReady = false; return; }

            var rs = _em.GetComponentData<ResonanceScore>(_rsEntity);
            float currentRS = rs.CurrentRS;

            // Distribute to all consumers
            if (Mathf.Abs(currentRS - _lastRS) > 0.01f)
            {
                // HUD
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.UpdateRS(currentRS);
                    if (currentRS > _lastRS)
                        HUDController.Instance.FlashRSGain(currentRS - _lastRS);
                }

                // Music
                if (AdaptiveMusicController.Instance != null)
                    AdaptiveMusicController.Instance.UpdateResonanceScore(currentRS);

                // VFX palette
                if (VFXController.Instance != null)
                    VFXController.Instance.UpdateWorldPalette(currentRS);

                // Zone atmosphere
                if (ZoneController.Instance != null)
                    ZoneController.Instance.UpdateRS(currentRS);

                _lastRS = currentRS;
            }

            // Threshold crossing detection
            int threshold = rs.ThresholdReached;
            if (threshold > _lastThreshold)
            {
                OnThresholdCrossed((RSThreshold)threshold);
                _lastThreshold = threshold;
            }
        }

        void OnThresholdCrossed(RSThreshold threshold)
        {
            Debug.Log($"[GameLoop] RS threshold crossed: {threshold}");

            switch (threshold)
            {
                case RSThreshold.FirstGolem: // RS 25
                    // First enemy spawns (handled by ECS EnemySpawnSystem)
                    // Trigger stinger + haptics
                    AdaptiveMusicController.Instance?.PlayCombatStart();
                    HapticFeedbackManager.Instance?.PlayGolemSpawn();
                    if (HUDController.Instance != null)
                        HUDController.Instance.SetZoneName("Echohaven — Awakening");
                    break;

                case RSThreshold.AetherWake: // RS 50
                    AdaptiveMusicController.Instance?.PlayZoneShift();
                    VFXController.Instance?.TriggerAetherWake();
                    if (HUDController.Instance != null)
                        HUDController.Instance.SetZoneName("Echohaven — Aether Flows");
                    break;

                case RSThreshold.ZoneShift: // RS 75
                    AdaptiveMusicController.Instance?.PlayZoneShift();
                    VFXController.Instance?.TriggerZoneShift();
                    if (HUDController.Instance != null)
                        HUDController.Instance.SetZoneName("Echohaven — Resonance");
                    break;

                case RSThreshold.ZoneComplete: // RS 100
                    AdaptiveMusicController.Instance?.PlayRestoration();
                    VFXController.Instance?.TriggerZoneComplete();
                    if (HUDController.Instance != null)
                        HUDController.Instance.SetZoneName("Echohaven — Restored");
                    StartZoneVictorySequence();
                    break;
            }

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.ReachRS, threshold.ToString());

            // Mark dirty for save
            SaveManager.Instance?.MarkDirty();
        }

        // ─── GameState Transition Handling ────────────

        void OnGameStateChanged(GameState oldState, GameState newState)
        {
            // Combat transitions
            if (newState == GameState.Combat && oldState != GameState.Combat)
            {
                AdaptiveMusicController.Instance?.PlayCombatStart();
                HapticFeedbackManager.Instance?.PlayGolemSpawn();
            }

            // Return from combat
            if (oldState == GameState.Combat && newState == GameState.Exploration)
            {
                // RS reward already queued by CombatBridge
            }

            // Tuning mini-game start
            if (newState == GameState.Tuning)
            {
                // Camera switches automatically via CameraController
            }

            // Cinematic (building restoration reveal)
            if (newState == GameState.Cinematic)
            {
                AdaptiveMusicController.Instance?.PlayRestoration();
            }

            // Save on meaningful transitions
            if (newState == GameState.Exploration)
                SaveManager.Instance?.MarkDirty();
        }

        // ─── Tutorial / Boss Event Handlers ─────────

        void HandleTutorialComplete()
        {
            Debug.Log("[GameLoop] Tutorial complete — unlocking first quest.");
            QuestManager.Instance?.UnlockQuest("echohaven_awakening");
            HUDController.Instance?.ShowInteractionPrompt(
                "Tutorial complete! Open your Quest Log (J) to begin.");
        }

        void HandleBossDefeated(BossResult result)
        {
            Debug.Log($"[GameLoop] Boss defeated: {result.bossName} — Score {result.performanceScore:P0}");

            // Trigger climax sequence for the current Moon
            int moonIdx = CampaignFlowController.Instance?.CurrentMoonIndex ?? 0;
            ClimaxSequenceSystem.Instance?.TriggerClimax(moonIdx);

            // Advance Moon campaign
            CampaignFlowController.Instance?.AdvanceToNextMoon();

            // Companion celebration
            MiloController.Instance?.NotifyCombatVictory();
            LiraelController.Instance?.NotifyZoneComplete();

            // Hide boss health bar
            HUDController.Instance?.HideBossHealth();

            // Return to exploration after cinematic
            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
            SaveManager.Instance?.MarkDirty();
        }

        void HandleBossHealthChanged(float normalizedHealth)
        {
            HUDController.Instance?.UpdateBossHealth(normalizedHealth);
        }

        void HandleClimaxCompleted(int moonIndex)
        {
            Debug.Log($"[GameLoop] Climax sequence completed for Moon {moonIndex + 1} — triggering rewards.");

            // Show moon trophy banner
            string moonName = ZoneTransitionSystem.Instance?.CurrentZone?.zoneName ?? $"Moon {moonIndex + 1}";
            HUDController.Instance?.ShowMoonTrophy(
                $"{moonName} Restored!",
                "The resonance deepens. A new path opens.");

            // Achievement progress notification
            NotificationSystem.Instance?.Show("Moon climax complete!", NotificationType.Achievement);

            // RS bonus for climax completion
            QueueRSReward(25f, "climax_bonus");

            // Flash the RS gauge
            HUDController.Instance?.FlashRSGain(25f);

            SaveManager.Instance?.MarkDirty();
        }

        void HandleWaveStarted(int waveIndex)
        {
            var wm = CombatWaveManager.Instance;
            int totalWaves = wm != null ? wm.TotalWaves : 0;
            int enemies = wm != null ? wm.EnemiesRemaining : 0;
            HUDController.Instance?.ShowWaveCounter(waveIndex, totalWaves, enemies);

            // Show boss health bar when boss encounters start
            if (BossEncounterSystem.Instance != null && BossEncounterSystem.Instance.IsActive)
                HUDController.Instance?.ShowBossHealth(
                    BossEncounterSystem.Instance.CurrentBoss?.bossName ?? "Boss", 1f);
        }

        void HandleWaveCleared(int waveIndex)
        {
            HUDController.Instance?.UpdateWaveEnemies(0);
            NotificationSystem.Instance?.Show($"Wave {waveIndex + 1} cleared!", NotificationType.Combat);
        }

        void HandleAllWavesCleared()
        {
            Debug.Log("[GameLoop] All combat waves cleared.");
            HUDController.Instance?.HideWaveCounter();
            HUDController.Instance?.HideBossHealth();
            NotificationSystem.Instance?.Show("All waves eliminated! Victory!", NotificationType.Combat);
            SaveManager.Instance?.MarkDirty();
        }

        void HandleAchievementUnlocked(AchievementSystem.AchievementDef def)
        {
            HUDController.Instance?.ShowAchievementToast(def.title);
            NotificationSystem.Instance?.Show($"Achievement: {def.title}", NotificationType.Achievement);
            Debug.Log($"[GameLoop] Achievement unlocked: {def.id} — {def.title}");
        }

        void HandleCompanionTrustChanged(string companionId, float newTrust)
        {
            NotificationSystem.Instance?.ShowTrustChange(companionId, Mathf.RoundToInt(newTrust).ToString());
        }

        // ─── Combat Input Forwarding to ECS ──────────

        void HandleResonancePulse()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.FireResonancePulse();
            TutorialSystem.Instance?.ForceComplete(TutorialStep.ResonancePulse);
        }

        void HandleHarmonicStrike()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.FireHarmonicStrike();
            TutorialSystem.Instance?.ForceComplete(TutorialStep.HarmonicStrike);
        }

        void HandleFrequencyShield()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.ActivateFrequencyShield();
            TutorialSystem.Instance?.ForceComplete(TutorialStep.FrequencyShield);
        }

        // ─── Save Sync ──────────────────────────────

        void SyncSaveData()
        {
            _saveSyncTimer += Time.deltaTime;
            if (_saveSyncTimer < SAVE_SYNC_INTERVAL) return;
            _saveSyncTimer = 0f;

            var save = SaveManager.Instance;
            if (save == null || save.CurrentSave == null) return;

            // Sync player position
            if (playerInput != null)
            {
                var pos = playerInput.transform.position;
                save.CurrentSave.player.position = new SerializableVector3(pos);
            }

            // Sync RS
            save.CurrentSave.world.resonanceScore = _lastRS;

            // Save indicator flash
            if (save.CurrentSave != null)
                UIManager.Instance?.ShowSaveIndicator();

            save.MarkDirty();
        }

        // ─── Public API for other systems ────────────

        /// <summary>
        /// Called by InteractableBuilding when a building is discovered.
        /// </summary>
        public void OnBuildingDiscovered(string buildingName, Vector3 position)
        {
            Debug.Log($"[GameLoop] Building discovered: {buildingName}");
            TutorialSystem.Instance?.ForceComplete(TutorialStep.Discovery);

            // Queue RS reward via ECS
            if (_ecsReady)
                ResonanceEventHelper.QueueDiscovery(_em, _rsEntity, 0.9f);

            // Audio + Haptics
            AdaptiveMusicController.Instance?.PlayDiscovery();
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Camera close-up
            cameraController?.FocusOnPoint(position, 3f);

            // VFX
            VFXController.Instance?.PlayDiscoveryBurst(position);

            // Milo speaks
            DialogueManager.Instance?.PlayContextDialogue("discovery");

            // Anastasia: lore whisper on discovery
            AnastasiaController.Instance?.TryDeliverLine("discovery");

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DiscoverBuilding, buildingName);
        }

        /// <summary>
        /// Called when a tuning node is completed.
        /// </summary>
        public void OnTuningNodeComplete(string buildingId, int nodeIndex, float accuracy)
        {
            Debug.Log($"[GameLoop] Tuning complete: {buildingId} node {nodeIndex} accuracy {accuracy:P0}");

            bool isPerfect = accuracy >= 0.95f;
            float goldenMatch = accuracy; // accuracy already factors golden ratio

            if (_ecsReady)
            {
                ResonanceEventHelper.QueueTuneNode(_em, _rsEntity, accuracy, goldenMatch);
            }

            // Haptics
            if (isPerfect)
                HapticFeedbackManager.Instance?.PlayPerfectTune();

            // VFX
            VFXController.Instance?.PlayTuningSuccess(
                playerInput != null ? playerInput.transform.position : Vector3.zero,
                isPerfect);

            SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Called when all nodes complete and building fully restores.
        /// </summary>
        public void OnBuildingRestored(string buildingName, Vector3 position, bool allPerfect)
        {
            Debug.Log($"[GameLoop] Building restored: {buildingName} allPerfect={allPerfect}");

            if (_ecsReady)
                ResonanceEventHelper.QueueBuildingRestored(_em, _rsEntity, allPerfect, allPerfect ? 1f : 0.8f);

            AdaptiveMusicController.Instance?.PlayRestoration();
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();

            // Cinematic reveal
            cameraController?.FocusOnPoint(position, 5f);

            VFXController.Instance?.PlayBuildingEmergence(position);
            DialogueManager.Instance?.PlayContextDialogue("restoration");

            // Anastasia: first restoration triggers manifestation, subsequent ones get memory fragments
            if (AnastasiaController.Instance != null)
            {
                if (!AnastasiaController.Instance.HasManifested)
                    AnastasiaController.Instance.TriggerFirstManifestation();
                else
                    AnastasiaController.Instance.TryDeliverLine("restoration");
            }

            // Companion notifications
            MiloController.Instance?.NotifyBuildingRestored();
            LiraelController.Instance?.NotifyBuildingRestored();

            SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Called by CombatBridge when an enemy is defeated.
        /// </summary>
        public void OnEnemyDefeated(Vector3 position)
        {
            if (_ecsReady)
                ResonanceEventHelper.QueueEnemyDefeated(_em, _rsEntity, false);

            HapticFeedbackManager.Instance?.PlayGolemDeath();
            VFXController.Instance?.PlayEnemyDissolution(position);
            DialogueManager.Instance?.PlayContextDialogue("combat_victory");

            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DefeatEnemies);

            // Companion notifications
            MiloController.Instance?.NotifyCombatVictory();

            // Return to exploration
            GameStateManager.Instance.TransitionTo(GameState.Exploration);

            SaveManager.Instance?.MarkDirty();
        }

        // ─── RS Reward Queue (generic) ───────────────

        /// <summary>
        /// Queue a generic RS reward via ECS. Used by QuestManager, WorkshopSystem, etc.
        /// </summary>
        public void QueueRSReward(float amount, string source)
        {
            if (!_ecsReady) return;
            Debug.Log($"[GameLoop] RS reward +{amount} from {source}");
            var buffer = _em.GetBuffer<ResonanceEvent>(_rsEntity);
            buffer.Add(new ResonanceEvent
            {
                Action = ResonanceAction.CollectAether,
                BaseReward = amount,
                Multiplier = 1f,
                GoldenRatioMatch = 0f
            });
        }

        public void ActivateRSBuff(float duration = RS_BUFF_DURATION)
        {
            _rsBuffTimer = duration;
            HUDController.Instance?.ShowInteractionPrompt($"Resonance Amplifier active! +25% RS for {duration:F0}s");
            Debug.Log($"[GameLoop] RS buff activated for {duration}s");
        }

        void HandleRequestActivateRSBuff() => ActivateRSBuff();

        // ─── Debug / Cheat API ───────────────────────

        /// <summary>Set RS to an absolute value (debug console).</summary>
        public void SetResonanceScore(float amount)
        {
            if (!_ecsReady || !_em.Exists(_rsEntity)) return;
            var rs = _em.GetComponentData<ResonanceScore>(_rsEntity);
            rs.CurrentRS = Mathf.Max(0f, amount);
            _em.SetComponentData(_rsEntity, rs);
            _lastRS = rs.CurrentRS;
        }

        /// <summary>Add to current RS (debug console).</summary>
        public void AddResonanceScore(float amount)
        {
            QueueRSReward(amount, "debug_console");
        }

        /// <summary>Set Aether charge display (debug console). 0-100 range.</summary>
        public void SetAetherCharge(float amount)
        {
            HUDController.Instance?.UpdateAetherCharge(Mathf.Clamp(amount, 0f, 100f));
        }

        /// <summary>Spawn a fallback enemy at position (debug console).</summary>
        public void SpawnEnemyAt(Vector3 position)
        {
            // Try CombatWaveManager first
            if (CombatWaveManager.Instance != null)
            {
                CombatWaveManager.Instance.SpawnSingleEnemy(position);
                return;
            }
            // Fallback: create greybox golem
            var golem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            golem.name = "DebugEnemy_MudGolem";
            golem.transform.position = position;
            golem.transform.localScale = Vector3.one * 1.8f;
            golem.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.2f);
            golem.tag = "Enemy";
        }

        // ─── Mini-Game → Building Bonus Bridge ───────

        /// <summary>
        /// Called when any mini-game completes. Broadcasts RS bonus to all
        /// active/in-progress buildings in the scene + queues direct RS reward.
        /// </summary>
        public void OnMiniGameCompleted(float rsReward, string miniGameType)
        {
            Debug.Log($"[GameLoop] Mini-game completed: {miniGameType}, RS {rsReward:F1}");

            QueueRSReward(rsReward, miniGameType);

            // Broadcast to all buildings
            RefreshBuildingCache();
            foreach (var b in _cachedBuildings)
                b.ReceiveMiniGameBonus(rsReward, miniGameType);
        }

        void RefreshBuildingCache()
        {
            _buildingCacheAge += Time.deltaTime;
            if (_buildingCacheAge >= BUILDING_CACHE_TTL)
            {
                _buildingCacheAge = 0f;
                _cachedBuildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            }
        }

        // ─── Player ECS Position Sync ────────────────

        /// <summary>
        /// Copies the MonoBehaviour player world position into the ECS
        /// PlayerTag entity's LocalTransform so DiscoverySystem, EnemyAISystem,
        /// and CompanionBehaviorSystem see the correct position.
        /// </summary>
        void SyncPlayerPositionToECS()
        {
            if (playerInput == null || !_em.Exists(_playerEntity)) return;

            var pos = playerInput.transform.position;
            var rot = playerInput.transform.rotation;

            _em.SetComponentData(_playerEntity, new LocalTransform
            {
                Position = new float3(pos.x, pos.y, pos.z),
                Rotation = new quaternion(rot.x, rot.y, rot.z, rot.w),
                Scale = 1f
            });
        }

        // ─── Zone Victory Sequence ───────────────────

        void StartZoneVictorySequence()
        {
            if (_zoneVictoryTriggered) return;
            _zoneVictoryTriggered = true;

            string zoneName = ZoneTransitionSystem.Instance?.CurrentZone?.zoneName ?? $"Moon {_currentMoonIndex + 1}";
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompleteZone, zoneName);

            StartCoroutine(ZoneVictoryCoroutine());
        }

        System.Collections.IEnumerator ZoneVictoryCoroutine()
        {
            Debug.Log("[GameLoop] Zone victory sequence started!");

            GameStateManager.Instance.TransitionTo(GameState.Cinematic);
            DialogueManager.Instance?.PlayContextDialogue("zone_complete");

            // Companion zone-complete notifications
            MiloController.Instance?.NotifyZoneComplete();
            LiraelController.Instance?.NotifyZoneComplete();

            // Let the celebration effects play for a few seconds
            yield return new WaitForSeconds(3f);

            // Camera sweep of restored zone
            if (cameraController != null)
            {
                var zone = ZoneController.Instance;
                if (zone != null)
                    cameraController.FocusOnPoint(zone.transform.position, 6f);
            }

            yield return new WaitForSeconds(6f);

            // Display zone complete message via HUD
            HUDController.Instance?.ShowInteractionPrompt(
                "ECHOHAVEN RESTORED\nResonance Score: 100 — Zone Complete");

            yield return new WaitForSeconds(5f);

            HUDController.Instance?.HideInteractionPrompt();

            // Return to exploration — in full game this would advance to next zone
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            Debug.Log("[GameLoop] Zone victory sequence complete. Vertical slice finished!");
        }

        // ─── Save/Load Subsystem Sync ────────────────

        void OnBeforeSave(SaveData save)
        {
            // Anastasia
            var ana = AnastasiaController.Instance;
            if (ana != null)
            {
                var d = ana.GetSaveData();
                save.anastasia.bitmaskLow = d.bitmaskLow;
                save.anastasia.bitmaskHigh = d.bitmaskHigh;
                save.anastasia.motesCollected = d.motesCollected;
                save.anastasia.currentMoon = d.currentMoon;
                save.anastasia.hasManifested = d.hasManifested;
                save.anastasia.postSolidWarmGlow = d.postSolidWarmGlow;
                save.anastasia.solidPhase = d.solidPhase;
            }

            // Quests
            var qm = QuestManager.Instance;
            if (qm != null)
            {
                var states = qm.GetAllStatesForSave();
                var entries = new QuestSaveEntry[states.Count];
                int i = 0;
                foreach (var kvp in states)
                {
                    entries[i++] = new QuestSaveEntry
                    {
                        questId = kvp.Key,
                        status = (int)kvp.Value.status,
                        objectiveProgress = kvp.Value.objectiveProgress ?? System.Array.Empty<int>()
                    };
                }
                save.quests.entries = entries;
            }

            // Workshop
            var ws = WorkshopSystem.Instance;
            if (ws != null)
            {
                var tiers = ws.GetTiersForSave();
                var entries = new WorkshopSaveEntry[tiers.Count];
                int i = 0;
                foreach (var kvp in tiers)
                {
                    entries[i++] = new WorkshopSaveEntry
                    {
                        buildingId = kvp.Key,
                        tier = kvp.Value
                    };
                }
                save.workshop.entries = entries;
            }

            // Zone
            var zt = ZoneTransitionSystem.Instance;
            if (zt != null)
            {
                save.zone.currentZoneIndex = zt.CurrentZoneIndex;
            }

            // Corruption
            var cs = CorruptionSystem.Instance;
            if (cs != null)
            {
                var states = cs.GetAllStates();
                var entries = new CorruptionSaveEntry[states.Count];
                int ci = 0;
                foreach (var s in states)
                {
                    entries[ci++] = new CorruptionSaveEntry
                    {
                        buildingId = s.buildingId,
                        corruptionLevel = s.corruptionLevel,
                        stage = (int)s.stage,
                        identified = s.identified,
                        isolated = s.isolated,
                        purged = s.purged
                    };
                }
                save.corruption.entries = entries;
            }

            // Campaign
            var cf = CampaignFlowController.Instance;
            if (cf != null)
            {
                var cData = cf.GetSaveData();
                save.campaign.currentMoon = cData.currentMoon;
                save.campaign.completedMoons = cData.completedMoons?.ToArray()
                    ?? System.Array.Empty<int>();
            }

            // Skill Tree
            var st = Gameplay.SkillTreeSystem.Instance;
            if (st != null)
            {
                var stData = st.GetSaveData();
                save.skillTree.unlockedSkillIds = stData.unlockedSkills?.ToArray()
                    ?? System.Array.Empty<int>();
            }

            // Cassian
            var cas = CassianNPCController.Instance;
            if (cas != null)
            {
                var casData = cas.GetSaveData();
                save.cassian.trustLevel = casData.trustLevel;
                save.cassian.interactionCount = casData.interactionCount;
                save.cassian.introduced = casData.introduced;
                save.cassian.sharedIntelIds = casData.sharedIntelIds?.ToArray()
                    ?? System.Array.Empty<string>();
            }

            // Economy
            var econ = Core.EconomySystem.Instance;
            if (econ != null)
            {
                var econData = econ.GetSaveData();
                save.economy.aetherShards = econData.aetherShards;
                save.economy.resonanceCrystals = econData.resonanceCrystals;
                save.economy.starFragments = econData.starFragments;
                save.economy.harmonicFragments = econData.harmonicFragments;
                save.economy.echoMemories = econData.echoMemories;
                save.economy.crystallineDust = econData.crystallineDust;
                save.economy.forgeTokens = econData.forgeTokens;
                var bList = new System.Collections.Generic.List<Save.EconomyBuildingEntry>();
                if (econData.buildings != null)
                {
                    foreach (var b in econData.buildings)
                        bList.Add(new Save.EconomyBuildingEntry
                        {
                            buildingId = b.buildingId,
                            baseIncome = b.baseIncome,
                            outputType = b.outputType,
                            level = b.level,
                            active = b.active
                        });
                }
                save.economy.buildings = bList.ToArray();
            }

            // Thorne
            var thorne = ThorneController.Instance;
            if (thorne != null)
            {
                var td = thorne.GetSaveData();
                save.thorne.trust = td.trust;
                save.thorne.introduced = td.introduced;
                save.thorne.militiaActive = td.militiaActive;
                save.thorne.combatBriefingsGiven = td.combatBriefingsGiven;
                save.thorne.zonesSecuredTogether = td.zonesSecuredTogether;
            }

            // Korath
            var korath = KorathController.Instance;
            if (korath != null)
            {
                var kd = korath.GetSaveData();
                save.korath.trust = kd.trust;
                save.korath.introduced = kd.introduced;
                save.korath.dayOutOfTimeRevealed = kd.dayOutOfTimeRevealed;
                save.korath.teachingsGiven = kd.teachingsGiven;
                save.korath.revelationsUnlocked = kd.revelationsUnlocked;
                save.korath.highestPlayerRS = kd.highestPlayerRS;
            }

            // Milo
            var milo = MiloController.Instance;
            if (milo != null)
            {
                var md = milo.GetSaveData();
                save.milo.trust = md.trust;
                save.milo.introduced = md.introduced;
                save.milo.artifactsAppraised = md.artifactsAppraised;
                save.milo.jokesDelivered = md.jokesDelivered;
                save.milo.sincereMoments = md.sincereMoments;
                save.milo.orphanTrainWitnessed = md.orphanTrainWitnessed;
                save.milo.whiteCityOutburst = md.whiteCityOutburst;
                save.milo.korathSacrificeWitnessed = md.korathSacrificeWitnessed;
            }

            // Lirael
            var lirael = LiraelController.Instance;
            if (lirael != null)
            {
                var ld = lirael.GetSaveData();
                save.lirael.trust = ld.trust;
                save.lirael.introduced = ld.introduced;
                save.lirael.solidity = ld.solidity;
                save.lirael.songsRemembered = ld.songsRemembered;
                save.lirael.dissonanceWarningsGiven = ld.dissonanceWarningsGiven;
                save.lirael.orphanTrainRemembered = ld.orphanTrainRemembered;
                save.lirael.childrenChoirConducted = ld.childrenChoirConducted;
                save.lirael.korathSongsLearned = ld.korathSongsLearned;
                save.lirael.fountainHealed = ld.fountainHealed;
                save.lirael.fullyManifested = ld.fullyManifested;
            }

            // Tutorial
            var tut = TutorialSystem.Instance;
            if (tut != null)
            {
                var td = tut.GetSaveData();
                var stepIds = new System.Collections.Generic.List<string>();
                if (td.completedSteps != null)
                    foreach (int s in td.completedSteps)
                        stepIds.Add(s.ToString());
                save.tutorial.completedStepIds = stepIds.ToArray();
                save.tutorial.tutorialFinished = td.finished;
            }

            // DialogueTree
            var dtr = DialogueTreeRunner.Instance;
            if (dtr != null)
            {
                var dd = dtr.GetSaveData();
                save.dialogueTree.seenDialogueIds = dd.visitedNodeIds?.ToArray()
                    ?? System.Array.Empty<string>();
                var branchIds = new System.Collections.Generic.List<string>();
                if (dd.choiceRecords != null)
                    foreach (var r in dd.choiceRecords)
                        branchIds.Add($"{r.nodeId}:{r.choiceIndex}");
                save.dialogueTree.chosenBranchIds = branchIds.ToArray();
            }

            // Codex
            var codex = UI.CodexSystem.Instance;
            if (codex != null)
            {
                var cd = codex.GetSaveData();
                save.codex.unlockedEntryIds = cd.unlockedEntryIds?.ToArray()
                    ?? System.Array.Empty<string>();
            }

            // Zereth
            var zereth = ZerethController.Instance;
            if (zereth != null)
            {
                var zd = zereth.GetSaveData();
                save.zereth.presenceLevel = zd.presenceLevel;
                save.zereth.phase = zd.phase;
                save.zereth.prophecyStonesTriggered = zd.prophecyStonesTriggered;
                save.zereth.voiceResponsesPlayed = zd.voiceResponsesPlayed;
                save.zereth.korathRevelationHeard = zd.korathRevelationHeard;
                save.zereth.triggerRoomDiscovered = zd.triggerRoomDiscovered;
                save.zereth.physicallyManifested = zd.physicallyManifested;
                save.zereth.finalConfrontationStarted = zd.finalConfrontationStarted;
                save.zereth.redeemed = zd.redeemed;
            }

            // Veritas
            var veritas = VeritasController.Instance;
            if (veritas != null)
            {
                var vd = veritas.GetSaveData();
                save.veritas.trust = vd.trust;
                save.veritas.introduced = vd.introduced;
                save.veritas.trustTier = (int)vd.trustTier;
                save.veritas.registersRestored = vd.registersRestored;
                save.veritas.performanceAccuracy = vd.performanceAccuracy;
                save.veritas.requiemPerformed = vd.requiemPerformed;
                save.veritas.bellTowerSyncComplete = vd.bellTowerSyncComplete;
                save.veritas.finalNoteDelivered = vd.finalNoteDelivered;
            }

            // AirshipFleet
            var fleet = AirshipFleetManager.Instance;
            if (fleet != null)
            {
                var fd = fleet.GetSaveData();
                save.airshipFleet.formation = fd.formation;
                save.airshipFleet.shipsRestored = fd.shipsRestored;
                save.airshipFleet.totalMercuryOrbsTuned = fd.totalMercuryOrbsTuned;
                save.airshipFleet.fleetOperational = fd.fleetOperational;
                if (fd.ships != null)
                {
                    int sc = fd.ships.Length;
                    save.airshipFleet.shipStates = new int[sc];
                    save.airshipFleet.shipHealth = new float[sc];
                    save.airshipFleet.shipMercuryOrbs = new int[sc];
                    save.airshipFleet.shipRestored = new bool[sc];
                    for (int si = 0; si < sc; si++)
                    {
                        save.airshipFleet.shipStates[si] = fd.ships[si].state;
                        save.airshipFleet.shipHealth[si] = fd.ships[si].health;
                        save.airshipFleet.shipMercuryOrbs[si] = fd.ships[si].mercuryOrbsTuned;
                        save.airshipFleet.shipRestored[si] = fd.ships[si].restored;
                    }
                }
            }

            // LeyLineProphecy
            var ley = LeyLineProphecyMiniGame.Instance;
            if (ley != null)
            {
                var ld = ley.GetSaveData();
                save.leyLineProphecy.stonesActivated = ld.stonesActivated ?? System.Array.Empty<bool>();
                save.leyLineProphecy.stonesCompleted = ld.stonesCompleted;
                save.leyLineProphecy.dreamspellClock = ld.dreamspellClock;
                save.leyLineProphecy.miniGameActive = ld.miniGameActive;
            }

            // BellTowerSync
            var bells = BellTowerSyncMiniGame.Instance;
            if (bells != null)
            {
                var bd = bells.GetSaveData();
                save.bellTowerSync.towerFrequencies = bd.towerFrequencies ?? System.Array.Empty<float>();
                save.bellTowerSync.towersSynced = bd.towersSynced;
                save.bellTowerSync.resonanceScore = bd.resonanceScore;
                save.bellTowerSync.miniGameActive = bd.miniGameActive;
                save.bellTowerSync.cascadeTriggered = bd.cascadeTriggered;
            }

            // GiantMode
            var giant = GiantModeController.Instance;
            if (giant != null)
            {
                var gd = giant.GetSaveData();
                save.giantMode.totalActivations = gd.totalActivations;
                save.giantMode.buildingsLifted = gd.buildingsLifted;
                save.giantMode.rubbleCleared = gd.rubbleCleared;
                save.giantMode.totalTimeAsGiant = gd.totalTimeAsGiant;
            }

            // WorldChoice
            var wc = WorldChoiceTracker.Instance;
            if (wc != null)
            {
                var wd = wc.GetSaveData();
                save.worldChoice.choiceIds = wd.choiceIds ?? System.Array.Empty<int>();
                save.worldChoice.choiceValues = wd.choiceValues ?? System.Array.Empty<int>();
            }

            // Achievement
            var ach = AchievementSystem.Instance;
            if (ach != null)
            {
                var ad = ach.GetSaveData();
                save.achievementData.unlockedIds = ad.unlockedIds ?? System.Array.Empty<string>();
                save.achievementData.progressKeys = ad.progressKeys ?? System.Array.Empty<string>();
                save.achievementData.progressValues = ad.progressValues ?? System.Array.Empty<float>();
                save.achievementData.totalUnlocked = ad.totalUnlocked;
            }

            // Dialogue Arcs
            var dia = CompanionDialogueArcs.Instance;
            if (dia != null)
            {
                var dd = dia.GetSaveData();
                save.dialogueArcs.companionIds = dd.companionIds ?? System.Array.Empty<int>();
                save.dialogueArcs.trustLevels = dd.trustLevels ?? System.Array.Empty<int>();
                save.dialogueArcs.seenKeys = dd.seenKeys ?? System.Array.Empty<string>();
            }

            // ─── v7 Save Blocks ──────────────────────────

            // Excavation
            var exc = Gameplay.ExcavationSystem.Instance;
            if (exc != null)
            {
                var ed = exc.GetSaveData();
                var siteIds = new System.Collections.Generic.List<string>();
                var stages = new System.Collections.Generic.List<int>();
                var progress = new System.Collections.Generic.List<float>();
                if (ed.sites != null)
                {
                    foreach (var s in ed.sites)
                    {
                        siteIds.Add(s.siteId);
                        stages.Add(s.layersCleared);
                        progress.Add(s.scanAccuracy);
                    }
                }
                save.excavation.discoveredSiteIds = siteIds.ToArray();
                save.excavation.siteStages = stages.ToArray();
                save.excavation.siteProgress = progress.ToArray();
                save.excavation.totalExcavations = siteIds.Count;
            }

            // Crafting
            var craft = Gameplay.CraftingSystem.Instance;
            if (craft != null)
            {
                var cd = craft.GetSaveData();
                save.crafting.knownRecipeIds = cd.discoveredRecipes?.ToArray() ?? System.Array.Empty<string>();
                var itemIds = new System.Collections.Generic.List<string>();
                var itemCounts = new System.Collections.Generic.List<int>();
                if (cd.inventory != null)
                {
                    foreach (var inv in cd.inventory)
                    {
                        itemIds.Add(inv.itemId);
                        itemCounts.Add(inv.count);
                    }
                }
                save.crafting.inventoryItemIds = itemIds.ToArray();
                save.crafting.inventoryItemCounts = itemCounts.ToArray();
                save.crafting.totalCrafted = cd.inventory?.Count ?? 0;
            }

            // Scanner
            var scan = Gameplay.ResonanceScannerSystem.Instance;
            if (scan != null)
            {
                var sd = scan.GetSaveData();
                save.scanner.scannedObjectIds = sd.revealedPOIs?.ToArray() ?? System.Array.Empty<string>();
                save.scanner.totalScans = sd.revealedPOIs?.Count ?? 0;
            }

            // Continental Rail
            var rail = ContinentalRailSystem.Instance;
            if (rail != null)
            {
                var rd = rail.GetSaveData();
                save.rail.segmentRestored = rd.segmentRestored ?? System.Array.Empty<bool>();
                save.rail.segmentHasBoss = rd.segmentHasBoss ?? System.Array.Empty<bool>();
                save.rail.segmentCorruption = rd.segmentCorruption ?? System.Array.Empty<float>();
                save.rail.stationsDiscovered = rd.stationsDiscovered ?? System.Array.Empty<bool>();
                save.rail.segmentsRestored = rd.segmentsRestored;
                save.rail.networkComplete = rd.networkComplete;
                save.rail.trainActive = rd.trainActive;
                save.rail.trainCurrentStation = rd.trainCurrentStation;
            }

            // Aquifer Purge
            var aquifer = AquiferPurgeMiniGame.Instance;
            if (aquifer != null)
            {
                var ad2 = aquifer.GetSaveData();
                save.aquiferPurge.layerStates = ad2.layerStates ?? System.Array.Empty<int>();
                save.aquiferPurge.layerPurity = ad2.layerPurity ?? System.Array.Empty<float>();
                save.aquiferPurge.layerAccuracy = ad2.layerAccuracy ?? System.Array.Empty<float>();
                save.aquiferPurge.currentLayer = ad2.currentLayer;
            }

            // Cosmic Convergence
            var cosmic = CosmicConvergenceMiniGame.Instance;
            if (cosmic != null)
            {
                var cd2 = cosmic.GetSaveData();
                save.cosmicConvergence.currentPhase = cd2.currentPhase;
                save.cosmicConvergence.phasesComplete = cd2.phasesComplete ?? System.Array.Empty<bool>();
                save.cosmicConvergence.phaseAccuracy = cd2.phaseAccuracy ?? System.Array.Empty<float>();
                save.cosmicConvergence.convergenceScore = cd2.convergenceScore;
            }

            // Day Out of Time
            var dott = DayOutOfTimeController.Instance;
            if (dott != null)
            {
                var dd2 = dott.GetSaveData();
                save.dayOutOfTime.eventCompleted = dd2.eventCompleted;
                save.dayOutOfTime.festivalCurrency = dd2.festivalCurrency;
                save.dayOutOfTime.currentMemoryZone = dd2.currentMemoryZone;
                save.dayOutOfTime.bestChallengeScore = dd2.bestChallengeScore;
            }

            // Companion Manager
            var comp = CompanionManager.Instance;
            if (comp != null)
            {
                var cmd = comp.GetSaveData();
                save.companionManager.companionIds = cmd.companionIds ?? System.Array.Empty<string>();
                save.companionManager.companionUnlocked = cmd.companionUnlocked ?? System.Array.Empty<bool>();
                save.companionManager.companionTrust = cmd.companionTrust ?? System.Array.Empty<float>();
            }

            // ─── v8 Save Blocks ──────────────────────────

            // CombatWave
            var wave = CombatWaveManager.Instance;
            if (wave != null)
            {
                var wd = wave.GetSaveData();
                save.combatWave.encounterActive = wd.encounterActive;
                save.combatWave.currentWaveIndex = wd.currentWaveIndex;
                save.combatWave.enemiesRemaining = wd.enemiesRemaining;
                save.combatWave.totalWaves = wd.totalWaves;
            }
        }

        void OnAfterLoad(SaveData save)
        {
            // Anastasia
            var ana = AnastasiaController.Instance;
            if (ana != null && save.anastasia != null)
            {
                ana.RestoreFromSave(new AnastasiaController.AnastasiaSaveData
                {
                    bitmaskLow = save.anastasia.bitmaskLow,
                    bitmaskHigh = save.anastasia.bitmaskHigh,
                    motesCollected = save.anastasia.motesCollected,
                    currentMoon = save.anastasia.currentMoon,
                    hasManifested = save.anastasia.hasManifested,
                    postSolidWarmGlow = save.anastasia.postSolidWarmGlow,
                    solidPhase = save.anastasia.solidPhase
                });
            }

            // Quests
            var qm = QuestManager.Instance;
            if (qm != null && save.quests?.entries != null)
            {
                var dict = new System.Collections.Generic.Dictionary<string, QuestState>();
                foreach (var e in save.quests.entries)
                {
                    dict[e.questId] = new QuestState
                    {
                        status = (QuestStatus)e.status,
                        objectiveProgress = e.objectiveProgress
                    };
                }
                qm.RestoreFromSave(dict);
            }

            // Workshop
            var ws = WorkshopSystem.Instance;
            if (ws != null && save.workshop?.entries != null)
            {
                var dict = new System.Collections.Generic.Dictionary<string, int>();
                foreach (var e in save.workshop.entries)
                    dict[e.buildingId] = e.tier;
                ws.RestoreFromSave(dict);
            }

            // Zone
            var zt = ZoneTransitionSystem.Instance;
            if (zt != null && save.zone != null)
            {
                if (save.zone.currentZoneIndex > 0)
                    zt.TransitionToZone(save.zone.currentZoneIndex);
            }

            // Corruption
            var cs = CorruptionSystem.Instance;
            if (cs != null && save.corruption?.entries != null)
            {
                var cStates = new System.Collections.Generic.List<CorruptionState>();
                foreach (var e in save.corruption.entries)
                {
                    cStates.Add(new CorruptionState
                    {
                        buildingId = e.buildingId,
                        corruptionLevel = e.corruptionLevel,
                        stage = (PurgeStage)e.stage,
                        identified = e.identified,
                        isolated = e.isolated,
                        purged = e.purged
                    });
                }
                cs.RestoreFromSave(cStates);
            }

            // Campaign
            var cf = CampaignFlowController.Instance;
            if (cf != null && save.campaign != null)
            {
                cf.RestoreFromSave(new CampaignSaveData
                {
                    currentMoon = save.campaign.currentMoon,
                    completedMoons = save.campaign.completedMoons != null
                        ? new System.Collections.Generic.List<int>(save.campaign.completedMoons)
                        : new System.Collections.Generic.List<int>()
                });
            }

            // Skill Tree
            var st = Gameplay.SkillTreeSystem.Instance;
            if (st != null && save.skillTree != null)
            {
                st.RestoreFromSave(new Gameplay.SkillTreeSaveData
                {
                    unlockedSkills = save.skillTree.unlockedSkillIds != null
                        ? new System.Collections.Generic.List<int>(save.skillTree.unlockedSkillIds)
                        : new System.Collections.Generic.List<int>()
                });
            }

            // Cassian
            var cas = CassianNPCController.Instance;
            if (cas != null && save.cassian != null)
            {
                cas.RestoreFromSave(new CassianSaveData
                {
                    trustLevel = save.cassian.trustLevel,
                    interactionCount = save.cassian.interactionCount,
                    introduced = save.cassian.introduced,
                    sharedIntelIds = save.cassian.sharedIntelIds != null
                        ? new System.Collections.Generic.List<string>(save.cassian.sharedIntelIds)
                        : new System.Collections.Generic.List<string>()
                });
            }

            // Economy
            var econ = Core.EconomySystem.Instance;
            if (econ != null && save.economy != null)
            {
                var buildings = new System.Collections.Generic.List<Core.BuildingIncomeSave>();
                if (save.economy.buildings != null)
                {
                    foreach (var e in save.economy.buildings)
                        buildings.Add(new Core.BuildingIncomeSave
                        {
                            buildingId = e.buildingId,
                            baseIncome = e.baseIncome,
                            outputType = e.outputType,
                            level = e.level,
                            active = e.active
                        });
                }
                econ.LoadSaveData(new Core.EconomySaveData
                {
                    aetherShards = save.economy.aetherShards,
                    resonanceCrystals = save.economy.resonanceCrystals,
                    starFragments = save.economy.starFragments,
                    harmonicFragments = save.economy.harmonicFragments,
                    echoMemories = save.economy.echoMemories,
                    crystallineDust = save.economy.crystallineDust,
                    forgeTokens = save.economy.forgeTokens,
                    buildings = buildings
                });
            }

            // Thorne
            var thorne = ThorneController.Instance;
            if (thorne != null && save.thorne != null)
            {
                thorne.LoadSaveData(new ThorneSaveData
                {
                    trust = save.thorne.trust,
                    introduced = save.thorne.introduced,
                    militiaActive = save.thorne.militiaActive,
                    combatBriefingsGiven = save.thorne.combatBriefingsGiven,
                    zonesSecuredTogether = save.thorne.zonesSecuredTogether
                });
            }

            // Korath
            var korath = KorathController.Instance;
            if (korath != null && save.korath != null)
            {
                korath.LoadSaveData(new KorathSaveData
                {
                    trust = save.korath.trust,
                    introduced = save.korath.introduced,
                    dayOutOfTimeRevealed = save.korath.dayOutOfTimeRevealed,
                    teachingsGiven = save.korath.teachingsGiven,
                    revelationsUnlocked = save.korath.revelationsUnlocked,
                    highestPlayerRS = save.korath.highestPlayerRS
                });
            }

            // Milo
            var milo = MiloController.Instance;
            if (milo != null && save.milo != null)
            {
                milo.LoadSaveData(new MiloSaveData
                {
                    trust = save.milo.trust,
                    introduced = save.milo.introduced,
                    artifactsAppraised = save.milo.artifactsAppraised,
                    jokesDelivered = save.milo.jokesDelivered,
                    sincereMoments = save.milo.sincereMoments,
                    orphanTrainWitnessed = save.milo.orphanTrainWitnessed,
                    whiteCityOutburst = save.milo.whiteCityOutburst,
                    korathSacrificeWitnessed = save.milo.korathSacrificeWitnessed
                });
            }

            // Lirael
            var lirael = LiraelController.Instance;
            if (lirael != null && save.lirael != null)
            {
                lirael.LoadSaveData(new LiraelSaveData
                {
                    trust = save.lirael.trust,
                    introduced = save.lirael.introduced,
                    solidity = save.lirael.solidity,
                    songsRemembered = save.lirael.songsRemembered,
                    dissonanceWarningsGiven = save.lirael.dissonanceWarningsGiven,
                    orphanTrainRemembered = save.lirael.orphanTrainRemembered,
                    childrenChoirConducted = save.lirael.childrenChoirConducted,
                    korathSongsLearned = save.lirael.korathSongsLearned,
                    fountainHealed = save.lirael.fountainHealed,
                    fullyManifested = save.lirael.fullyManifested
                });
            }

            // Tutorial
            var tut = TutorialSystem.Instance;
            if (tut != null && save.tutorial != null)
            {
                var completedSteps = new System.Collections.Generic.List<int>();
                if (save.tutorial.completedStepIds != null)
                    foreach (var id in save.tutorial.completedStepIds)
                        if (int.TryParse(id, out int step))
                            completedSteps.Add(step);
                tut.RestoreFromSave(new TutorialSaveData
                {
                    completedSteps = completedSteps,
                    currentIndex = completedSteps.Count,
                    finished = save.tutorial.tutorialFinished
                });
            }

            // DialogueTree
            var dtr = DialogueTreeRunner.Instance;
            if (dtr != null && save.dialogueTree != null)
            {
                var visited = save.dialogueTree.seenDialogueIds != null
                    ? new System.Collections.Generic.List<string>(save.dialogueTree.seenDialogueIds)
                    : new System.Collections.Generic.List<string>();
                var choices = new System.Collections.Generic.List<DialogueChoiceRecord>();
                if (save.dialogueTree.chosenBranchIds != null)
                {
                    foreach (var entry in save.dialogueTree.chosenBranchIds)
                    {
                        int sep = entry.LastIndexOf(':');
                        if (sep > 0 && int.TryParse(entry.Substring(sep + 1), out int ci))
                            choices.Add(new DialogueChoiceRecord
                            {
                                nodeId = entry.Substring(0, sep),
                                choiceIndex = ci
                            });
                    }
                }
                dtr.RestoreFromSave(new DialogueTreeSaveData
                {
                    visitedNodeIds = visited,
                    choiceRecords = choices
                });
            }

            // Codex
            var codex = UI.CodexSystem.Instance;
            if (codex != null && save.codex != null)
            {
                codex.LoadSaveData(new UI.CodexSaveData
                {
                    unlockedEntryIds = save.codex.unlockedEntryIds != null
                        ? new System.Collections.Generic.List<string>(save.codex.unlockedEntryIds)
                        : new System.Collections.Generic.List<string>()
                });
            }

            // Zereth
            var zereth = ZerethController.Instance;
            if (zereth != null && save.zereth != null)
            {
                zereth.LoadSaveData(new ZerethSaveData
                {
                    presenceLevel = save.zereth.presenceLevel,
                    phase = save.zereth.phase,
                    prophecyStonesTriggered = save.zereth.prophecyStonesTriggered,
                    voiceResponsesPlayed = save.zereth.voiceResponsesPlayed,
                    korathRevelationHeard = save.zereth.korathRevelationHeard,
                    triggerRoomDiscovered = save.zereth.triggerRoomDiscovered,
                    physicallyManifested = save.zereth.physicallyManifested,
                    finalConfrontationStarted = save.zereth.finalConfrontationStarted,
                    redeemed = save.zereth.redeemed
                });
            }

            // Veritas
            var veritasLoad = VeritasController.Instance;
            if (veritasLoad != null && save.veritas != null)
            {
                veritasLoad.LoadSaveData(new VeritasSaveData
                {
                    trust = save.veritas.trust,
                    introduced = save.veritas.introduced,
                    trustTier = (int)save.veritas.trustTier,
                    registersRestored = save.veritas.registersRestored,
                    performanceAccuracy = save.veritas.performanceAccuracy,
                    requiemPerformed = save.veritas.requiemPerformed,
                    bellTowerSyncComplete = save.veritas.bellTowerSyncComplete,
                    finalNoteDelivered = save.veritas.finalNoteDelivered
                });
            }

            // AirshipFleet
            var fleetLoad = AirshipFleetManager.Instance;
            if (fleetLoad != null && save.airshipFleet != null)
            {
                int sc = save.airshipFleet.shipStates?.Length ?? 0;
                var ships = new AirshipShipSave[sc];
                for (int si = 0; si < sc; si++)
                {
                    ships[si] = new AirshipShipSave
                    {
                        state = save.airshipFleet.shipStates[si],
                        health = si < (save.airshipFleet.shipHealth?.Length ?? 0) ? save.airshipFleet.shipHealth[si] : 100f,
                        mercuryOrbsTuned = si < (save.airshipFleet.shipMercuryOrbs?.Length ?? 0) ? save.airshipFleet.shipMercuryOrbs[si] : 0,
                        restored = si < (save.airshipFleet.shipRestored?.Length ?? 0) && save.airshipFleet.shipRestored[si]
                    };
                }
                fleetLoad.LoadSaveData(new AirshipFleetSaveData
                {
                    ships = ships,
                    formation = save.airshipFleet.formation,
                    shipsRestored = save.airshipFleet.shipsRestored,
                    totalMercuryOrbsTuned = save.airshipFleet.totalMercuryOrbsTuned,
                    fleetOperational = save.airshipFleet.fleetOperational
                });
            }

            // LeyLineProphecy
            var leyLoad = LeyLineProphecyMiniGame.Instance;
            if (leyLoad != null && save.leyLineProphecy != null)
            {
                leyLoad.LoadSaveData(new LeyLineSaveData
                {
                    stonesActivated = save.leyLineProphecy.stonesActivated,
                    stonesCompleted = save.leyLineProphecy.stonesCompleted,
                    dreamspellClock = save.leyLineProphecy.dreamspellClock,
                    miniGameActive = save.leyLineProphecy.miniGameActive
                });
            }

            // BellTowerSync
            var bellsLoad = BellTowerSyncMiniGame.Instance;
            if (bellsLoad != null && save.bellTowerSync != null)
            {
                bellsLoad.LoadSaveData(new BellTowerSaveData
                {
                    towerFrequencies = save.bellTowerSync.towerFrequencies,
                    towersSynced = save.bellTowerSync.towersSynced,
                    resonanceScore = save.bellTowerSync.resonanceScore,
                    miniGameActive = save.bellTowerSync.miniGameActive,
                    cascadeTriggered = save.bellTowerSync.cascadeTriggered
                });
            }

            // GiantMode
            var giantLoad = GiantModeController.Instance;
            if (giantLoad != null && save.giantMode != null)
            {
                giantLoad.LoadSaveData(save.giantMode);
            }

            // WorldChoice
            var wcLoad = WorldChoiceTracker.Instance;
            if (wcLoad != null && save.worldChoice != null)
            {
                wcLoad.LoadSaveData(new WorldChoiceSaveData
                {
                    choiceIds = save.worldChoice.choiceIds,
                    choiceValues = save.worldChoice.choiceValues
                });
            }

            // Achievement
            var achLoad = AchievementSystem.Instance;
            if (achLoad != null && save.achievementData != null)
            {
                achLoad.LoadSaveData(new AchievementSaveData
                {
                    unlockedIds = save.achievementData.unlockedIds,
                    progressKeys = save.achievementData.progressKeys,
                    progressValues = save.achievementData.progressValues,
                    totalUnlocked = save.achievementData.totalUnlocked
                });
            }

            // Dialogue Arcs
            var diaLoad = CompanionDialogueArcs.Instance;
            if (diaLoad != null && save.dialogueArcs != null)
            {
                diaLoad.LoadSaveData(new CompanionDialogueArcs.DialogueArcSaveData
                {
                    companionIds = save.dialogueArcs.companionIds,
                    trustLevels = save.dialogueArcs.trustLevels,
                    seenKeys = save.dialogueArcs.seenKeys
                });
            }

            // ─── v7 Load Blocks ──────────────────────────

            // Excavation
            var excLoad = Gameplay.ExcavationSystem.Instance;
            if (excLoad != null && save.excavation != null)
            {
                var sites = new System.Collections.Generic.List<Gameplay.ExcavationSiteEntry>();
                if (save.excavation.discoveredSiteIds != null)
                {
                    for (int i = 0; i < save.excavation.discoveredSiteIds.Length; i++)
                    {
                        sites.Add(new Gameplay.ExcavationSiteEntry
                        {
                            siteId = save.excavation.discoveredSiteIds[i],
                            layersCleared = save.excavation.siteStages != null && i < save.excavation.siteStages.Length ? save.excavation.siteStages[i] : 0,
                            scanAccuracy = save.excavation.siteProgress != null && i < save.excavation.siteProgress.Length ? save.excavation.siteProgress[i] : 0f,
                            isDiscovered = true
                        });
                    }
                }
                excLoad.LoadSaveData(new Gameplay.ExcavationSaveData { sites = sites });
            }

            // Crafting
            var craftLoad = Gameplay.CraftingSystem.Instance;
            if (craftLoad != null && save.crafting != null)
            {
                var recipes = save.crafting.knownRecipeIds != null
                    ? new System.Collections.Generic.List<string>(save.crafting.knownRecipeIds)
                    : new System.Collections.Generic.List<string>();
                var inventory = new System.Collections.Generic.List<Gameplay.CraftingInventoryEntry>();
                if (save.crafting.inventoryItemIds != null)
                {
                    for (int i = 0; i < save.crafting.inventoryItemIds.Length; i++)
                    {
                        inventory.Add(new Gameplay.CraftingInventoryEntry
                        {
                            itemId = save.crafting.inventoryItemIds[i],
                            count = save.crafting.inventoryItemCounts != null && i < save.crafting.inventoryItemCounts.Length ? save.crafting.inventoryItemCounts[i] : 0
                        });
                    }
                }
                craftLoad.LoadSaveData(new Gameplay.CraftingSaveData
                {
                    discoveredRecipes = recipes,
                    inventory = inventory
                });
            }

            // Scanner
            var scanLoad = Gameplay.ResonanceScannerSystem.Instance;
            if (scanLoad != null && save.scanner != null)
            {
                var pois = save.scanner.scannedObjectIds != null
                    ? new System.Collections.Generic.List<string>(save.scanner.scannedObjectIds)
                    : new System.Collections.Generic.List<string>();
                scanLoad.LoadSaveData(new Gameplay.ScannerSaveData { revealedPOIs = pois });
            }

            // Continental Rail
            var railLoad = ContinentalRailSystem.Instance;
            if (railLoad != null && save.rail != null)
            {
                railLoad.LoadSaveData(new ContinentalRailSystem.RailSavePayload
                {
                    segmentRestored = save.rail.segmentRestored,
                    segmentHasBoss = save.rail.segmentHasBoss,
                    segmentCorruption = save.rail.segmentCorruption,
                    stationsDiscovered = save.rail.stationsDiscovered,
                    segmentsRestored = save.rail.segmentsRestored,
                    networkComplete = save.rail.networkComplete,
                    trainActive = save.rail.trainActive,
                    trainCurrentStation = save.rail.trainCurrentStation
                });
            }

            // Aquifer Purge
            var aquiferLoad = AquiferPurgeMiniGame.Instance;
            if (aquiferLoad != null && save.aquiferPurge != null)
            {
                aquiferLoad.LoadSaveData(new AquiferPurgeMiniGame.AquiferSavePayload
                {
                    layerStates = save.aquiferPurge.layerStates,
                    layerPurity = save.aquiferPurge.layerPurity,
                    layerAccuracy = save.aquiferPurge.layerAccuracy,
                    currentLayer = save.aquiferPurge.currentLayer
                });
            }

            // Cosmic Convergence
            var cosmicLoad = CosmicConvergenceMiniGame.Instance;
            if (cosmicLoad != null && save.cosmicConvergence != null)
            {
                cosmicLoad.LoadSaveData(new CosmicConvergenceMiniGame.CosmicSavePayload
                {
                    currentPhase = save.cosmicConvergence.currentPhase,
                    phasesComplete = save.cosmicConvergence.phasesComplete,
                    phaseAccuracy = save.cosmicConvergence.phaseAccuracy,
                    convergenceScore = save.cosmicConvergence.convergenceScore
                });
            }

            // Day Out of Time
            var dottLoad = DayOutOfTimeController.Instance;
            if (dottLoad != null && save.dayOutOfTime != null)
            {
                dottLoad.LoadSaveData(new DayOutOfTimeController.DotTSavePayload
                {
                    eventCompleted = save.dayOutOfTime.eventCompleted,
                    festivalCurrency = save.dayOutOfTime.festivalCurrency,
                    currentMemoryZone = save.dayOutOfTime.currentMemoryZone,
                    bestChallengeScore = save.dayOutOfTime.bestChallengeScore
                });
            }

            // Companion Manager
            var compLoad = CompanionManager.Instance;
            if (compLoad != null && save.companionManager != null)
            {
                compLoad.LoadSaveData(new CompanionManager.CompanionManagerSavePayload
                {
                    companionIds = save.companionManager.companionIds,
                    companionUnlocked = save.companionManager.companionUnlocked,
                    companionTrust = save.companionManager.companionTrust
                });
            }

            // ─── v8 Load Blocks ──────────────────────────

            // CombatWave
            var waveLoad = CombatWaveManager.Instance;
            if (waveLoad != null && save.combatWave != null)
            {
                waveLoad.LoadSaveData(new CombatWaveManager.CombatWaveSaveData
                {
                    encounterActive = save.combatWave.encounterActive,
                    currentWaveIndex = save.combatWave.currentWaveIndex,
                    enemiesRemaining = save.combatWave.enemiesRemaining,
                    totalWaves = save.combatWave.totalWaves
                });
            }

            // ─── ECS World State Restore ────────────────

            RestoreECSWorldState(save);
        }

        void RestoreECSWorldState(SaveData save)
        {
            if (!_ecsReady || save == null) return;

            // Restore Resonance Score
            if (save.world != null && _rsQuery.CalculateEntityCount() > 0)
            {
                var rsEntity = _rsQuery.GetSingletonEntity();
                var rs = _em.GetComponentData<ResonanceScore>(rsEntity);
                rs.CurrentRS = save.world.resonanceScore;
                _em.SetComponentData(rsEntity, rs);
                _lastRS = save.world.resonanceScore;
                Debug.Log($"[GameLoop] RS restored: {save.world.resonanceScore:F1}");
            }

            // Restore Player ECS position
            if (save.player != null && _playerQuery.CalculateEntityCount() > 0)
            {
                var playerEntity = _playerQuery.GetSingletonEntity();
                var lt = _em.GetComponentData<LocalTransform>(playerEntity);
                lt.Position = new float3(save.player.position.x, save.player.position.y, save.player.position.z);
                _em.SetComponentData(playerEntity, lt);

                // Sync MonoBehaviour player too
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null)
                    playerGO.transform.position = new Vector3(save.player.position.x, save.player.position.y, save.player.position.z);
            }

            // Restore Building ECS states
            if (save.world?.buildings != null)
            {
                var buildingQuery = _em.CreateEntityQuery(typeof(TartarianBuilding));
                var buildings = buildingQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < buildings.Length && i < save.world.buildings.Length; i++)
                {
                    var building = _em.GetComponentData<TartarianBuilding>(buildings[i]);
                    building.State = (BuildingRestorationState)save.world.buildings[i].state;
                    building.NodesCompleted = save.world.buildings[i].nodesComplete != null
                        ? CountTrue(save.world.buildings[i].nodesComplete) : 0;
                    building.RestorationProgress = save.world.buildings[i].restorationProgress;
                    _em.SetComponentData(buildings[i], building);
                }
                buildings.Dispose();
                buildingQuery.Dispose();
            }

            // Restore Enemy spawn states
            if (save.world?.enemySpawns != null)
            {
                var spawnQuery = _em.CreateEntityQuery(typeof(EnemySpawnTrigger));
                var spawns = spawnQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < spawns.Length && i < save.world.enemySpawns.Length; i++)
                {
                    var trigger = _em.GetComponentData<EnemySpawnTrigger>(spawns[i]);
                    trigger.HasSpawned = save.world.enemySpawns[i].hasSpawned;
                    _em.SetComponentData(spawns[i], trigger);
                }
                spawns.Dispose();
                spawnQuery.Dispose();
            }
        }

        static int CountTrue(bool[] arr)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i]) count++;
            return count;
        }

        // ─── Workshop UI Bridge ──────────────────────

        void HandleWorkshopUpgrade(string buildingId)
        {
            var ws = WorkshopSystem.Instance;
            if (ws == null) return;

            if (ws.TryUpgrade(buildingId))
            {
                TutorialSystem.Instance?.ForceComplete(TutorialStep.WorkshopUpgrade);

                // Refresh the UI with updated data
                var panel = WorkshopUIPanel.Instance;
                if (panel != null)
                    panel.RefreshBuilding(CreateBuildingDisplayData(buildingId, ws));
            }
        }

        /// <summary>
        /// Populate Workshop UI with all buildings in the current zone.
        /// Call when opening the panel or when zone changes.
        /// </summary>
        public void RefreshWorkshopUI()
        {
            var ws = WorkshopSystem.Instance;
            var panel = WorkshopUIPanel.Instance;
            if (ws == null || panel == null) return;

            RefreshBuildingCache();
            var displayList = new System.Collections.Generic.List<BuildingDisplayData>();

            foreach (var b in _cachedBuildings)
            {
                if (b.State != BuildingRestorationState.Active) continue;
                displayList.Add(CreateBuildingDisplayData(b.BuildingId, ws));
            }

            panel.SetBuildings(displayList);
        }

        BuildingDisplayData CreateBuildingDisplayData(string buildingId, WorkshopSystem ws)
        {
            int tier = ws.GetTier(buildingId);
            var nextInfo = ws.GetNextTierInfo(buildingId);
            bool isMax = string.IsNullOrEmpty(nextInfo.tierName);
            float currentRS = 0f;

            if (_ecsReady)
                currentRS = _em.GetComponentData<ResonanceScore>(_rsEntity).CurrentRS;

            var building = FindBuilding(buildingId);
            string name = building?.Definition?.buildingName ?? buildingId;

            return new BuildingDisplayData
            {
                buildingId = buildingId,
                buildingName = name,
                currentTier = tier,
                maxTier = 5,
                currentTierName = tier == 0 ? "Restored" : GetTierName(tier),
                nextTierName = isMax ? "" : nextInfo.tierName,
                nextTierDescription = isMax ? "" : nextInfo.description,
                rsRequired = isMax ? 0 : nextInfo.rsRequirement,
                currentRS = currentRS,
                outputMultiplier = ws.GetOutputMultiplier(buildingId),
                canUpgrade = ws.CanUpgrade(buildingId),
                isMaxTier = isMax
            };
        }

        InteractableBuilding FindBuilding(string buildingId)
        {
            RefreshBuildingCache();
            foreach (var b in _cachedBuildings)
                if (b != null && b.BuildingId == buildingId) return b;
            return null;
        }

        static string GetTierName(int tier)
        {
            return tier switch
            {
                1 => "Reinforced",
                2 => "Harmonized",
                3 => "Resonant",
                4 => "Ascended",
                5 => "Perfected",
                _ => "Unknown"
            };
        }

        // ─── Mini-Game Failure / Event Handlers ──────

        void HandleChoirFailed()
        {
            HUDController.Instance?.ShowInteractionPrompt("The choir's harmony collapsed...");
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log("[GameLoop] Choir Harmonics performance failed.");
        }

        void HandleConvergenceFailed()
        {
            HUDController.Instance?.ShowInteractionPrompt("The cosmic alignment slipped away...");
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log("[GameLoop] Cosmic Convergence failed.");
        }

        void HandleLeyLineRestored(int nodeA, int nodeB)
        {
            float rsReward = 25f;
            QueueRSReward(rsReward, $"ley_line_{nodeA}_{nodeB}");
            VFXController.Instance?.PlayLeyLineRestore(
                UnityEngine.Vector3.Lerp(
                    UnityEngine.Vector3.zero, UnityEngine.Vector3.one, 0.5f));
            HUDController.Instance?.ShowInteractionPrompt($"Ley line restored between nodes {nodeA} and {nodeB}!");
            Debug.Log($"[GameLoop] Ley line restored: {nodeA} ↔ {nodeB}, +{rsReward} RS");
        }

        void HandleMemoryZoneChanged(int zoneIndex)
        {
            string[] zoneNames = {
                "Origin", "Paradise", "Granite City", "Amber Coast",
                "Iron Steppe", "Convergence", "Crystal Cavern",
                "Obsidian Reach", "Frozen Archive", "Prophecy Spire",
                "Twilight Garden", "Titan's Breach", "Resonance Peak"
            };
            string zoneName = zoneIndex < zoneNames.Length ? zoneNames[zoneIndex] : $"Zone {zoneIndex}";
            HUDController.Instance?.ShowInteractionPrompt($"Memory: {zoneName}");
            Debug.Log($"[GameLoop] Memory zone changed to: {zoneName} ({zoneIndex})");
        }

        // ─── Economy Event Handlers ──────────────────

        void HandleCurrencyChanged(CurrencyType type, int oldAmt, int newAmt)
        {
            int delta = newAmt - oldAmt;
            string sign = delta > 0 ? "+" : "";
            HUDController.Instance?.ShowInteractionPrompt($"{type}: {sign}{delta} (now {newAmt})");
            if (delta > 0)
                AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            Debug.Log($"[GameLoop] Currency {type}: {oldAmt} → {newAmt}");
        }

        // ─── Crafting Event Handlers ─────────────────

        void HandleItemCrafted(string recipeId)
        {
            HUDController.Instance?.ShowAchievementToast($"Crafted: {recipeId}");
            VFXController.Instance?.PlayDiscoveryBurst(
                _playerEntity != Entity.Null && _ecsReady
                    ? _em.GetComponentData<LocalTransform>(_playerEntity).Position
                    : Vector3.zero);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CraftItem, recipeId);
            OnMiniGameCompleted(15f, "crafting");
            Debug.Log($"[GameLoop] Item crafted: {recipeId}");
        }

        void HandleRecipeDiscovered(string recipeId)
        {
            HUDController.Instance?.ShowInteractionPrompt($"New recipe discovered: {recipeId}");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            Debug.Log($"[GameLoop] Recipe discovered: {recipeId}");
        }

        void HandleCraftFailed(string recipeId, string reason)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Craft failed: {reason}");
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log($"[GameLoop] Craft failed: {recipeId} — {reason}");
        }

        void HandleItemUsed(string itemId)
        {
            HUDController.Instance?.ShowAchievementToast($"Used: {itemId}");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            Debug.Log($"[GameLoop] Item used: {itemId}");
        }

        void HandleItemCollected(string itemId, int amount)
        {
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CollectItem, itemId, amount);
            Debug.Log($"[GameLoop] Item collected: {itemId} x{amount}");
        }

        // ─── Corruption Event Handlers ───────────────

        void HandleCorruptionChanged(string buildingId, float newLevel)
        {
            if (newLevel > 0.7f)
                HUDController.Instance?.ShowInteractionPrompt($"Warning: {buildingId} corruption critical!");
            Debug.Log($"[GameLoop] Corruption changed: {buildingId} → {newLevel:F2}");
        }

        void HandleCorruptionPurged(string buildingId)
        {
            HUDController.Instance?.ShowAchievementToast($"Purified: {buildingId}");
            VFXController.Instance?.PlayResonancePulse(Vector3.zero, 15f);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.QuestComplete);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            QueueRSReward(20f, $"purify_{buildingId}");
            Debug.Log($"[GameLoop] Corruption purged: {buildingId}");
        }

        void HandleCorruptionSpread(string from, string to)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Corruption spreading from {from}!");
            VFXController.Instance?.PlayDissonancePulse(Vector3.zero, 10f);
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log($"[GameLoop] Corruption spread: {from} → {to}");
        }

        // ─── Excavation Event Handlers ───────────────

        void HandleSiteDiscovered(ExcavationSite site)
        {
            HUDController.Instance?.ShowAchievementToast($"Site discovered: {site.siteId}");
            VFXController.Instance?.PlayDiscoveryBurst(site.position);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Excavation site discovered: {site.siteId}");
        }

        void HandleLayerCleared(ExcavationSite site, int layerIndex)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Layer {layerIndex + 1} cleared at {site.siteId}");
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            Debug.Log($"[GameLoop] Layer {layerIndex} cleared at {site.siteId}");
        }

        void HandleExcavationComplete(ExcavationSite site)
        {
            HUDController.Instance?.ShowAchievementToast($"Excavation complete: {site.siteId}");
            VFXController.Instance?.PlayBuildingEmergence(site.position);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.QuestComplete);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.ExcavateRuin, site.siteId);
            Debug.Log($"[GameLoop] Excavation complete: {site.siteId}");
        }

        void HandleExcavationRS(ExcavationSite site, float rsAmount)
        {
            QueueRSReward(rsAmount, $"excavation_{site.siteId}");
            HUDController.Instance?.FlashRSGain(rsAmount);
            Debug.Log($"[GameLoop] Excavation RS yield: +{rsAmount} from {site.siteId}");
        }

        // ─── Dialogue Tree Handlers ──────────────────

        void HandleDialogueStarted(string treeId, string speaker)
        {
            AdaptiveMusicController.Instance?.ExitCombat();
            Debug.Log($"[GameLoop] Dialogue started: {treeId} with {speaker}");
        }

        void HandleDialogueEnded(string treeId)
        {
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.TalkToNPC, treeId);
            Debug.Log($"[GameLoop] Dialogue ended: {treeId}");
        }

        void HandleChoiceMade(string nodeId, int choiceIndex)
        {
            Debug.Log($"[GameLoop] Dialogue choice: node={nodeId}, choice={choiceIndex}");
        }

        // ─── Boss Spawn / Phase / Dialogue Handlers ─

        void HandleBossSpawned(BossDefinition boss)
        {
            HUDController.Instance?.ShowBossHealth(boss.bossName, 1f);
            AdaptiveMusicController.Instance?.EnterBossEncounter();
            HapticFeedbackManager.Instance?.PlayGolemSpawn();
            VFXController.Instance?.PlayDissonancePulse(Vector3.zero, 20f);
            Debug.Log($"[GameLoop] Boss spawned: {boss.bossName}");
        }

        void HandleBossPhaseChanged(int phase)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Boss entering phase {phase + 1}!");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.BossPhase);
            HapticFeedbackManager.Instance?.PlayMoonHaptic(_currentMoonIndex, HapticContext.BossPhaseShift);
            Debug.Log($"[GameLoop] Boss phase changed: {phase}");
        }

        void HandleBossDialogue(string line)
        {
            UIManager.Instance?.ShowDialogue("???", line);
            Debug.Log($"[GameLoop] Boss dialogue: {line}");
        }

        // ─── Mini-Game Completion Handlers ───────────

        void HandleRockCutComplete(float accuracy)
        {
            float reward = 30f * accuracy;
            OnMiniGameCompleted(reward, "harmonic_rock_cutting");
            VFXController.Instance?.PlayTuningSuccess(Vector3.zero, accuracy > 0.9f);
            AdaptiveMusicController.Instance?.PlayStinger(
                accuracy > 0.9f ? StingerType.TuningSuccess : StingerType.Discovery);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            Debug.Log($"[GameLoop] Rock cutting complete: accuracy={accuracy:F2}, +{reward:F0} RS");
        }

        void HandleRockCutFailed()
        {
            HUDController.Instance?.ShowInteractionPrompt("The rock shattered... resonance lost.");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningFail);
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log("[GameLoop] Rock cutting failed.");
        }

        void HandleOrganComplete(float accuracy)
        {
            float reward = 35f * accuracy;
            OnMiniGameCompleted(reward, "pipe_organ");
            VFXController.Instance?.PlayTuningSuccess(Vector3.zero, accuracy > 0.9f);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            Debug.Log($"[GameLoop] Pipe organ complete: accuracy={accuracy:F2}, +{reward:F0} RS");
        }

        void HandleOrganFailed()
        {
            HUDController.Instance?.ShowInteractionPrompt("The organ pipes fell silent...");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningFail);
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log("[GameLoop] Pipe organ failed.");
        }

        // ─── Workshop Upgrade Handler ────────────────

        void HandleBuildingUpgraded(string buildingId, int newTier)
        {
            HUDController.Instance?.ShowAchievementToast(
                $"{buildingId} upgraded to {GetTierName(newTier)}!");
            VFXController.Instance?.PlayBuildingUpgrade(Vector3.zero, newTier);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.QuestComplete);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            Debug.Log($"[GameLoop] Building upgraded: {buildingId} → tier {newTier}");
        }

        // ─── Campaign Flow Handlers ──────────────────

        void HandleMoonStarted(int moonIndex)
        {
            _currentMoonIndex = moonIndex;
            HUDController.Instance?.SetZoneName($"Moon {moonIndex + 1}");
            AdaptiveMusicController.Instance?.SetZone(moonIndex);
            VFXController.Instance?.TriggerZoneShift();
            HapticFeedbackManager.Instance?.PlayMoonHaptic(moonIndex, HapticContext.ZoneTransition);
            Debug.Log($"[GameLoop] Moon started: {moonIndex}");
        }

        void HandleEndingChosen(CampaignFlowController.EndingPath ending)
        {
            HUDController.Instance?.ShowMoonTrophy("THE END",
                $"Path: {ending}");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.ZoneComplete);
            Debug.Log($"[GameLoop] Ending chosen: {ending}");
        }

        // ─── Anastasia Narrative Handlers ────────────

        void HandleAnastasiaMode(AnastasiaMode oldMode, AnastasiaMode newMode)
        {
            if (newMode == AnastasiaMode.Invisible)
            {
                VFXController.Instance?.SpawnAnastasiaSolidificationEffect(Vector3.zero);
                AdaptiveMusicController.Instance?.PlayStinger(StingerType.Discovery);
            }
            Debug.Log($"[GameLoop] Anastasia mode: {oldMode} → {newMode}");
        }

        void HandleAnastasiaSolidification(SolidificationPhase phase)
        {
            HUDController.Instance?.ShowInteractionPrompt(
                $"Anastasia solidification: phase {(int)phase}");
            Debug.Log($"[GameLoop] Anastasia solidification phase: {phase}");
        }

        // ─── Cassian Intel Handler ───────────────────

        void HandleCassianIntel(string intel)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Cassian: \"{intel}\"");
            DialogueManager.Instance?.PlayContextDialogue("discovery");
            Debug.Log($"[GameLoop] Cassian shared intel: {intel}");
        }

        // ─── Ley Line Node Handlers ──────────────────

        void HandleNodeActivated(int nodeIndex)
        {
            VFXController.Instance?.PlayResonancePulse(Vector3.zero, 8f);
            HUDController.Instance?.ShowInteractionPrompt($"Ley node {nodeIndex} activated!");
            QueueRSReward(10f, $"ley_node_{nodeIndex}");
            Debug.Log($"[GameLoop] Ley node activated: {nodeIndex}");
        }

        void HandleNodeSevered(int nodeIndex)
        {
            VFXController.Instance?.PlayDissonancePulse(Vector3.zero, 8f);
            HUDController.Instance?.ShowInteractionPrompt($"Ley node {nodeIndex} severed!");
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            Debug.Log($"[GameLoop] Ley node severed: {nodeIndex}");
        }

        // ─── Airship Fleet Handlers ──────────────────

        void HandleAirshipRestored(int shipIndex)
        {
            HUDController.Instance?.ShowAchievementToast($"Airship {shipIndex + 1} restored!");
            VFXController.Instance?.PlayBuildingEmergence(Vector3.up * 50f);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.QuestComplete);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            Debug.Log($"[GameLoop] Airship restored: {shipIndex}");
        }

        void HandleMercuryOrbTuned(int shipIndex, int orbCount)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Mercury orb {orbCount} tuned on airship {shipIndex + 1}");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            Debug.Log($"[GameLoop] Mercury orb tuned: ship={shipIndex}, orbs={orbCount}");
        }

        void HandleFormationChanged(AirshipFleetManager.FleetFormation formation)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Fleet formation: {formation}");
            Debug.Log($"[GameLoop] Fleet formation changed: {formation}");
        }

        void HandleFleetOperational()
        {
            HUDController.Instance?.ShowAchievementToast("Fleet fully operational!");
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.ZoneComplete);
            VFXController.Instance?.TriggerAetherWake();
            Debug.Log("[GameLoop] Fleet fully operational");
        }

        // ─── Choir Harmonics Handlers ────────────────

        void HandleVoiceEntered(int voiceIndex)
        {
            HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
            Debug.Log($"[GameLoop] Choir voice entered: {voiceIndex}");
        }

        void HandleVoiceDrifted(int voiceIndex)
        {
            HapticFeedbackManager.Instance?.PlayTuningOffFrequency();
            Debug.Log($"[GameLoop] Choir voice drifted: {voiceIndex}");
        }

        void HandleFullHarmony()
        {
            HUDController.Instance?.ShowAchievementToast("Full harmony achieved!");
            VFXController.Instance?.PlayResonancePulse(Vector3.zero, 15f);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            QueueRSReward(30f, "choir_harmony");
            Debug.Log("[GameLoop] Full choir harmony achieved");
        }

        void HandleTranscendentMoment()
        {
            HUDController.Instance?.ShowAchievementToast("Transcendent moment!");
            VFXController.Instance?.TriggerAetherWake();
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.ZoneComplete);
            QueueRSReward(50f, "choir_transcendent");
            Debug.Log("[GameLoop] Choir transcendent moment");
        }

        // ─── Aquifer Purge Handlers ──────────────────

        void HandleAquiferLayerPurged(int layer, float accuracy)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Aquifer layer {layer + 1} purged ({accuracy:P0})");
            VFXController.Instance?.PlayResonancePulse(Vector3.down * layer * 5f, 10f);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            QueueRSReward(15f * accuracy, $"aquifer_layer_{layer}");
            Debug.Log($"[GameLoop] Aquifer layer purged: {layer}, accuracy={accuracy:F2}");
        }

        void HandleAquiferBossSpawned()
        {
            HUDController.Instance?.ShowBossHealth("Sludge Leviathan", 1f);
            AdaptiveMusicController.Instance?.EnterBossEncounter();
            HapticFeedbackManager.Instance?.PlayGolemSpawn();
            Debug.Log("[GameLoop] Aquifer boss spawned: Sludge Leviathan");
        }

        // ─── Aether / Economy / Achievement Handlers ─

        void HandleAetherRSChanged(float newRS)
        {
            HUDController.Instance?.UpdateRS(newRS);
        }

        void HandleBuildingIncome(string buildingId, int income)
        {
            HUDController.Instance?.FlashRSGain(income);
            Debug.Log($"[GameLoop] Building income: {buildingId} → +{income}");
        }

        void HandleAchievementProgress(string achievementId, float progress)
        {
            if (progress >= 0.5f && progress < 0.51f) // halfway milestone
                HUDController.Instance?.ShowInteractionPrompt($"Achievement progress: {achievementId} 50%");
            Debug.Log($"[GameLoop] Achievement progress: {achievementId} → {progress:P0}");
        }

        // ─── Localization / UI Handlers ──────────────

        void HandleLanguageChanged(LocalizationManager.Language lang)
        {
            HUDController.Instance?.SetZoneName(LocalizationManager.Get("hud_zone_name"));
            Debug.Log($"[GameLoop] Language changed: {lang}");
        }

        void HandleLensToggled(bool active)
        {
            if (active)
            {
                AdaptiveMusicController.Instance?.PlayDiscovery();
                HapticFeedbackManager.Instance?.PlayDiscovery();
            }
            Debug.Log($"[GameLoop] Dissonance lens toggled: {active}");
        }

        // ─── Aether Conduit Mini-Game Handlers ──────

        void HandleBastionPlaced(int index, float accuracy)
        {
            VFXController.Instance?.PlayBuildingEmergence(Vector3.up * index * 3f);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            if (accuracy > 0.95f)
                AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            QueueRSReward(accuracy * 4f, $"bastion_{index}");
            Debug.Log($"[GameLoop] Bastion placed: index={index}, accuracy={accuracy:F2}");
        }

        void HandleRockCut(int index, float cutQuality)
        {
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            VFXController.Instance?.PlayHarmonicStrike(Vector3.up * index * 3f, Vector3.down);
            QueueRSReward(cutQuality * 2f, $"rock_cut_{index}");
            Debug.Log($"[GameLoop] Rock cut: index={index}, quality={cutQuality:F2}");
        }

        void HandleConduitResonancePeak()
        {
            VFXController.Instance?.PlayResonancePulse(Vector3.zero, 20f);
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            HUDController.Instance?.ShowInteractionPrompt("Resonance peak!");
            Debug.Log("[GameLoop] Conduit resonance peak");
        }

        // ─── Harmonic Rock Cutting Handlers ──────────

        void HandleVeinTraced(int veinIndex, float accuracy)
        {
            VFXController.Instance?.PlayHarmonicStrike(Vector3.up * veinIndex, Vector3.forward);
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            QueueRSReward(accuracy * 3f, $"vein_traced_{veinIndex}");
            Debug.Log($"[GameLoop] Vein traced: {veinIndex}, accuracy={accuracy:F2}");
        }

        void HandleRockComboChanged(int combo)
        {
            if (combo >= 3)
                HUDController.Instance?.ShowInteractionPrompt($"Combo x{combo}!");
            if (combo >= 5)
                AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            Debug.Log($"[GameLoop] Rock cutting combo: {combo}");
        }

        // ─── Orphan Train Puzzle Handlers ────────────

        void HandleTrainSegmentAligned(int index, float accuracy)
        {
            VFXController.Instance?.PlayTuningSuccess(Vector3.right * index * 5f, accuracy > 0.9f);
            HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
            QueueRSReward(accuracy * 2f, $"train_segment_{index}");
            Debug.Log($"[GameLoop] Train segment aligned: {index}, accuracy={accuracy:F2}");
        }

        void HandleTrainChainBroken(int segmentIndex)
        {
            VFXController.Instance?.PlayDissonancePulse(Vector3.zero, 5f);
            HapticFeedbackManager.Instance?.PlayDissonanceAlert();
            HUDController.Instance?.ShowInteractionPrompt("Chain broken! Re-align segments.");
            Debug.Log($"[GameLoop] Train chain broken at segment {segmentIndex}");
        }

        // ─── Rail Alignment Handlers ─────────────────

        void HandleSegmentRotated(int segIndex, int newAngle)
        {
            HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
            Debug.Log($"[GameLoop] Rail segment rotated: {segIndex} → {newAngle}°");
        }

        void HandleFlowChanged(float flowStrength)
        {
            HUDController.Instance?.UpdateRS(flowStrength * 100f); // Visual flow meter
            if (flowStrength >= 1f)
            {
                AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
                VFXController.Instance?.PlayResonancePulse(Vector3.zero, 15f);
            }
            Debug.Log($"[GameLoop] Rail flow changed: {flowStrength:F2}");
        }

        // ─── Pipe Organ Handlers ─────────────────────

        void HandlePipePlayed(int pipeIndex, bool correct)
        {
            if (correct)
            {
                HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
                VFXController.Instance?.PlayTuningSuccess(Vector3.up * pipeIndex, false);
            }
            else
            {
                HapticFeedbackManager.Instance?.PlayTuningOffFrequency();
                VFXController.Instance?.PlayDissonancePulse(Vector3.up * pipeIndex, 3f);
            }
            Debug.Log($"[GameLoop] Pipe played: {pipeIndex}, correct={correct}");
        }

        void HandleChordAdvanced(int chordIndex)
        {
            AdaptiveMusicController.Instance?.PlayStinger(StingerType.TuningSuccess);
            HUDController.Instance?.ShowInteractionPrompt($"Chord {chordIndex + 1} complete!");
            QueueRSReward(5f, $"chord_{chordIndex}");
            Debug.Log($"[GameLoop] Chord advanced: {chordIndex}");
        }

        // ─── Anastasia Line + Codex Entry Handlers ──

        void HandleAnastasiaLine(AnastasiaLine line)
        {
            HUDController.Instance?.ShowInteractionPrompt(line.text);
            Debug.Log($"[GameLoop] Anastasia line delivered: #{line.id} [{line.category}]");
        }

        void HandleCodexEntryUnlocked(string entryId)
        {
            HUDController.Instance?.ShowAchievementToast($"Codex entry unlocked: {entryId}");
            AdaptiveMusicController.Instance?.PlayDiscovery();
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Codex entry unlocked: {entryId}");
        }

        // ─── Boss / Climax / Scanner / Rail / BellTower Handlers ──

        void HandleBossFailed()
        {
            HUDController.Instance?.ShowInteractionPrompt("Boss encounter failed — regroup and try again.");
            AdaptiveMusicController.Instance?.ExitCombat();
            HapticFeedbackManager.Instance?.PlayCombatHit();
            Debug.Log("[GameLoop] Boss encounter failed.");
        }

        void HandleClimaxStarted(int moonIndex)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Climax sequence beginning for Moon {moonIndex + 1}!");
            AdaptiveMusicController.Instance?.EnterCombat();
            HapticFeedbackManager.Instance?.PlayMoonHaptic(moonIndex, HapticContext.BossPhaseShift);
            Debug.Log($"[GameLoop] Climax started: Moon {moonIndex}");
        }

        void HandleScanStarted()
        {
            HUDController.Instance?.ShowInteractionPrompt("Resonance scan initiated...");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log("[GameLoop] Resonance scan started.");
        }

        void HandleScanComplete(System.Collections.Generic.List<ScanResult> results)
        {
            int count = results?.Count ?? 0;
            HUDController.Instance?.ShowInteractionPrompt($"Scan complete — {count} signal(s) detected.");
            if (count > 0)
                AdaptiveMusicController.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Scan complete: {count} results.");
        }

        void HandlePOIRevealed(ScanResult result)
        {
            HUDController.Instance?.ShowAchievementToast($"POI revealed: {result.poiId}");
            VFXController.Instance?.PlayDiscoveryBurst(result.worldPosition);
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] POI revealed: {result.poiId} at distance {result.distance:F1}m");
        }

        void HandleRailSegmentRestored(int from, int to)
        {
            QueueRSReward(15f, "rail_segment");
            HUDController.Instance?.ShowAchievementToast($"Rail segment restored: {from} → {to}");
            AdaptiveMusicController.Instance?.PlayDiscovery();
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Rail segment restored: {from} → {to}");
        }

        void HandleStationDiscovered(int stationIndex)
        {
            QueueRSReward(20f, "station_discovered");
            HUDController.Instance?.ShowAchievementToast($"Station discovered: #{stationIndex}");
            AdaptiveMusicController.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Station discovered: {stationIndex}");
        }

        void HandleTrainDeparted(int from, int to)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Train departing: station {from} → {to}");
            HapticFeedbackManager.Instance?.PlayMoonHaptic(_currentMoonIndex, HapticContext.ZoneTransition);
            Debug.Log($"[GameLoop] Train departed: {from} → {to}");
        }

        void HandleTrainArrived(int stationIndex)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Arrived at station {stationIndex}");
            AdaptiveMusicController.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Train arrived: station {stationIndex}");
        }

        void HandleRailLeviathan()
        {
            HUDController.Instance?.ShowInteractionPrompt("Rail Leviathan approaches!");
            AdaptiveMusicController.Instance?.EnterCombat();
            HapticFeedbackManager.Instance?.PlayCombatHit();
            Debug.Log("[GameLoop] Rail Leviathan encounter triggered.");
        }

        void HandleTowerTuned(int towerIndex, float accuracy)
        {
            QueueRSReward(5f * accuracy, "tower_tuned");
            HUDController.Instance?.ShowInteractionPrompt($"Tower {towerIndex} tuned — accuracy {accuracy:P0}");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[GameLoop] Tower tuned: {towerIndex}, accuracy {accuracy:F2}");
        }

        void HandleTowerSynced(int towerIndex)
        {
            QueueRSReward(10f, "tower_synced");
            HUDController.Instance?.ShowAchievementToast($"Tower {towerIndex} synchronized!");
            AdaptiveMusicController.Instance?.PlayDiscovery();
            VFXController.Instance?.PlayDiscoveryBurst(
                PlayerInputHandler.Instance != null
                    ? PlayerInputHandler.Instance.transform.position
                    : Vector3.zero);
            Debug.Log($"[GameLoop] Tower synced: {towerIndex}");
        }

        void HandleTowerDesynced(int towerIndex)
        {
            HUDController.Instance?.ShowInteractionPrompt($"Tower {towerIndex} lost synchronization!");
            HapticFeedbackManager.Instance?.PlayCombatHit();
            Debug.Log($"[GameLoop] Tower desynced: {towerIndex}");
        }

        void HandleBellTowerRSChanged(float newScore)
        {
            HUDController.Instance?.UpdateRS(newScore);
            Debug.Log($"[GameLoop] Bell tower RS changed: {newScore:F1}");
        }
    }
}
