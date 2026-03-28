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
    public class GameLoopController : MonoBehaviour
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
        bool _ecsReady;

        // RS tracking
        float _lastRS;
        int _lastThreshold;
        float _rsUpdateTimer;
        const float RS_POLL_INTERVAL = 0.1f; // 10Hz is enough for UI

        // Save sync
        float _saveSyncTimer;
        const float SAVE_SYNC_INTERVAL = 5f;

        // Victory
        bool _zoneVictoryTriggered;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
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

            // Deferred ECS init (world may not be ready in Awake)
            InitECS();
        }

        void OnDestroy()
        {
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
        }

        void InitECS()
        {
            _ecsWorld = World.DefaultGameObjectInjectionWorld;
            if (_ecsWorld == null) return;
            _em = _ecsWorld.EntityManager;

            // Find ResonanceScore singleton
            var query = _em.CreateEntityQuery(typeof(ResonanceScore));
            if (query.CalculateEntityCount() > 0)
            {
                _rsEntity = query.GetSingletonEntity();
                _ecsReady = true;
            }

            // Find player entity for position sync
            var playerQuery = _em.CreateEntityQuery(typeof(PlayerTag), typeof(LocalTransform));
            if (playerQuery.CalculateEntityCount() > 0)
                _playerEntity = playerQuery.GetSingletonEntity();
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

            if (Mathf.Abs(rs.SkillRSMultiplier - skillMod) > 0.001f
                || Mathf.Abs(rs.MoonRSMultiplier - moonMod) > 0.001f)
            {
                rs.SkillRSMultiplier = skillMod;
                rs.MoonRSMultiplier = moonMod;
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

        // ─── Combat Input Forwarding to ECS ──────────

        void HandleResonancePulse()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.FireResonancePulse();
        }

        void HandleHarmonicStrike()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.FireHarmonicStrike();
        }

        void HandleFrequencyShield()
        {
            if (!_ecsReady) return;
            CombatBridge.Instance?.ActivateFrequencyShield();
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

            StartCoroutine(ZoneVictoryCoroutine());
        }

        System.Collections.IEnumerator ZoneVictoryCoroutine()
        {
            Debug.Log("[GameLoop] Zone victory sequence started!");

            GameStateManager.Instance.TransitionTo(GameState.Cinematic);
            DialogueManager.Instance?.PlayContextDialogue("zone_complete");

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
            GameStateManager.Instance.TransitionTo(GameState.Exploration);

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
                var cStates = new System.Collections.Generic.List<CorruptionSystem.CorruptionState>();
                foreach (var e in save.corruption.entries)
                {
                    cStates.Add(new CorruptionSystem.CorruptionState
                    {
                        buildingId = e.buildingId,
                        corruptionLevel = e.corruptionLevel,
                        stage = (CorruptionSystem.PurgeStage)e.stage,
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
        }

        // ─── Workshop UI Bridge ──────────────────────

        void HandleWorkshopUpgrade(string buildingId)
        {
            var ws = WorkshopSystem.Instance;
            if (ws == null) return;

            if (ws.TryUpgrade(buildingId))
            {
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

            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            var displayList = new System.Collections.Generic.List<BuildingDisplayData>();

            foreach (var b in buildings)
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
            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            foreach (var b in buildings)
                if (b.BuildingId == buildingId) return b;
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
    }
}
