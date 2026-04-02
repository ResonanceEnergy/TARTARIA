using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Game Bootstrap — initializes the ECS world, creates singleton entities,
    /// and sets up the Aether field configuration.
    /// Attached to the Boot scene's bootstrap GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Aether Field Configuration")]
        [SerializeField, Min(1)] int aetherGridX = 64;
        [SerializeField, Min(1)] int aetherGridY = 64;
        [SerializeField, Min(1)] int aetherGridZ = 32;
        [SerializeField, Min(0.1f)] float aetherCellSize = 2.0f;
        [SerializeField, Range(0f, 1f)] float aetherDissipation = 0.05f;
        [SerializeField, Min(0f)] float aetherAdvectionSpeed = 1.0f;

        void Start()
        {
            if (!InitializeECSWorld())
            {
                Debug.LogError("[Tartaria] ECS world initialization failed — cannot proceed.");
                return;
            }
            GameStateManager.Instance.TransitionTo(GameState.Loading);

            // Proceed to main scene load
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
        }

        bool InitializeECSWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("[Tartaria] DefaultGameObjectInjectionWorld is null — Entities package may not be initialized.");
                return false;
            }

            if (aetherGridX <= 0 || aetherGridY <= 0 || aetherGridZ <= 0)
            {
                Debug.LogError($"[Tartaria] Invalid Aether grid dimensions: {aetherGridX}x{aetherGridY}x{aetherGridZ}");
                return false;
            }

            var em = world.EntityManager;

            // Create Aether field configuration singleton
            var configEntity = em.CreateEntity();
            em.AddComponentData(configEntity, new AetherFieldConfig
            {
                GridSizeX = aetherGridX,
                GridSizeY = aetherGridY,
                GridSizeZ = aetherGridZ,
                CellSize = aetherCellSize,
                DissipationRate = aetherDissipation,
                AdvectionSpeed = aetherAdvectionSpeed
            });

            // Create player tag singleton (required by DiscoverySystem, AI systems)
            var playerEntity = em.CreateEntity();
            em.AddComponentData(playerEntity, new PlayerTag());
            em.AddComponentData(playerEntity, new LocalTransform
            {
                Position = new float3(0f, 1f, -20f),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            Debug.Log("[Tartaria] ECS world initialized. Aether field configured.");
            return true;
        }
    }
}
