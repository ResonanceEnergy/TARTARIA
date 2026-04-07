using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Tartaria.Core;
using Tartaria.AI;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// World Initializer — creates ECS entities that require cross-assembly types.
    /// Runs after GameBootstrap (which creates the RS singleton and player entity).
    ///
    /// Creates:
    ///   - Milo companion entity (CompanionBehavior + CompanionTag)
    ///   - Enemy spawn triggers (3× MudGolem at RS 25, 50, 75)
    ///   - Building ECS entities (3× TartarianBuilding with TuningNodes)
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-90)] // After GameBootstrap (-100), before GameLoopController (-50)
    public class WorldInitializer : MonoBehaviour
    {
        [Header("Companion Spawn")]
        [SerializeField] Vector3 companionOffset = new(2f, 0f, -1f);

        [Header("Enemy Spawn Positions")]
        [SerializeField] Vector3 golemSpawn1 = new(40f, 0f, 30f);
        [SerializeField] Vector3 golemSpawn2 = new(-35f, 0f, 45f);
        [SerializeField] Vector3 golemSpawn3 = new(10f, 0f, -50f);

        [Header("Building Positions")]
        [SerializeField] Vector3 domePosition = new(30f, 0f, 20f);
        [SerializeField] Vector3 fountainPosition = new(-20f, 0f, 35f);
        [SerializeField] Vector3 spirePosition = new(0f, 0f, -30f);

        bool _initialized;
        EntityQuery _rsQuery;
        bool _rsQueryCreated;

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            bool worldAlive = world != null && world.IsCreated;
            if (_rsQueryCreated && worldAlive) { _rsQuery.Dispose(); _rsQueryCreated = false; }
        }

        void Update()
        {
            if (_initialized) return;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var em = world.EntityManager;

            // Verify GameBootstrap has run (RS singleton exists)
            if (!_rsQueryCreated)
            {
                _rsQuery = em.CreateEntityQuery(typeof(ResonanceScore));
                _rsQueryCreated = true;
            }
            if (_rsQuery.CalculateEntityCount() == 0) return;

            _initialized = true;
            InitializeWorldEntities(em);
        }

        void InitializeWorldEntities(EntityManager em)
        {
            CreateCompanionEntity(em);
            CreateEnemySpawnTriggers(em);
            CreateBuildingEntities(em);

            Debug.Log("[WorldInit] Companion, 3 spawn triggers, 3 buildings created in ECS.");
        }

        // ─── Companion (Milo) ────────────────────────

        void CreateCompanionEntity(EntityManager em)
        {
            // Get player position for offset
            float3 playerPos = new float3(0f, 1f, -20f);
            var playerQuery = em.CreateEntityQuery(typeof(PlayerTag), typeof(LocalTransform));
            if (playerQuery.CalculateEntityCount() > 0)
            {
                var playerEntity = playerQuery.GetSingletonEntity();
                playerPos = em.GetComponentData<LocalTransform>(playerEntity).Position;
            }
            playerQuery.Dispose();

            var milo = em.CreateEntity();
            em.AddComponentData(milo, new CompanionTag { CompanionId = 0 });
            em.AddComponentData(milo, new CompanionBehavior
            {
                State = Tartaria.AI.CompanionState.Follow,
                PreviousState = Tartaria.AI.CompanionState.Follow,
                StateTimer = 0f,
                FollowDistance = 3f,
                IdleThreshold = 5f,
                ReactRadius = 20f,
                HideRadius = 10f,
                CelebrateTimer = 3f,
                TargetPosition = playerPos,
                WalkSpeed = 3f,
                SprintSpeed = 5f,
                SprintDistanceThreshold = 8f,
                MaxIdleTime = 10f
            });
            em.AddComponentData(milo, new MiloPersonality
            {
                Curiosity = 0.8f,
                Encouragement = 0.9f,
                Sarcasm = 0.3f
            });
            em.AddComponentData(milo, new LocalTransform
            {
                Position = playerPos + new float3(companionOffset.x, companionOffset.y, companionOffset.z),
                Rotation = quaternion.identity,
                Scale = 1f
            });
        }

        // ─── Enemy Spawn Triggers ────────────────────

        void CreateEnemySpawnTriggers(EntityManager em)
        {
            CreateSpawnTrigger(em, 25f, golemSpawn1);
            CreateSpawnTrigger(em, 50f, golemSpawn2);
            CreateSpawnTrigger(em, 75f, golemSpawn3);
        }

        void CreateSpawnTrigger(EntityManager em, float rsThreshold, Vector3 pos)
        {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, new EnemySpawnTrigger
            {
                RSThreshold = rsThreshold,
                EnemyToSpawn = EnemyType.MudGolem,
                SpawnPosition = new float3(pos.x, pos.y, pos.z),
                HasSpawned = false
            });
        }

        // ─── Building ECS Entities ───────────────────

        void CreateBuildingEntities(EntityManager em)
        {
            CreateBuildingEntity(em, BuildingArchetype.Dome, domePosition, "Dome");
            CreateBuildingEntity(em, BuildingArchetype.Fountain, fountainPosition, "Fountain");
            CreateBuildingEntity(em, BuildingArchetype.Spire, spirePosition, "Spire");
        }

        void CreateBuildingEntity(EntityManager em, BuildingArchetype archetype,
            Vector3 pos, string name)
        {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, new TartarianBuilding
            {
                Archetype = archetype,
                State = BuildingRestorationState.Buried,
                RestorationProgress = 0f,
                ResonanceScore = 0f,
                GoldenRatioMatch = GoldenRatioValidator.ValidateBuildingProportion(25f, 18f * GoldenRatioValidator.PHI),
                UpgradeTier = 0,
                NodesCompleted = 0,
                TotalNodes = 3
            });
            em.AddComponentData(entity, new DiscoveryTrigger
            {
                TriggerRadius = 30f,
                RSReward = ResonanceConstants.DISCOVER_STRUCTURE,
                Discovered = false
            });
            em.AddComponentData(entity, new MudDissolution
            {
                Progress = 0f,
                Speed = 5f
            });
            em.AddComponentData(entity, new LocalTransform
            {
                Position = new float3(pos.x, pos.y, pos.z),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            Debug.Log($"[WorldInit] Building entity created: {name} at {pos}");
        }
    }
}
