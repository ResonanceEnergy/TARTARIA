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
            SyncPlayerPositionToECS();
            SyncSaveData();
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
                if (isPerfect)
                    ResonanceEventHelper.QueueTuneNode(_em, _rsEntity, accuracy, goldenMatch);
                else
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
    }
}
