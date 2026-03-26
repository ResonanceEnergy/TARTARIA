using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Building Restoration System — manages the state machine for
    /// Tartarian buildings: BURIED → REVEALED → TUNING → EMERGING → ACTIVE.
    /// Each transition triggers visual, audio, and haptic feedback.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BuildingRestorationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResonanceScore>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Update mud dissolution progress for emerging buildings
            foreach (var (building, dissolution) in
                SystemAPI.Query<RefRO<TartarianBuilding>, RefRW<MudDissolution>>())
            {
                if (building.ValueRO.State == BuildingRestorationState.Emerging)
                {
                    dissolution.ValueRW.Progress = math.min(1.0f,
                        dissolution.ValueRO.Progress + (dt / dissolution.ValueRO.Speed));
                }
            }

            // Check for state transitions
            foreach (var (building, entity) in
                SystemAPI.Query<RefRW<TartarianBuilding>>().WithEntityAccess())
            {
                switch (building.ValueRO.State)
                {
                    case BuildingRestorationState.Tuning:
                        // All nodes complete → transition to Emerging
                        if (building.ValueRO.NodesCompleted >= building.ValueRO.TotalNodes)
                        {
                            building.ValueRW.State = BuildingRestorationState.Emerging;
                        }
                        break;

                    case BuildingRestorationState.Emerging:
                        // Check if dissolution is complete
                        // (Dissolution component checked separately)
                        break;
                }
            }

            // Check dissolution completion → transition to Active
            foreach (var (building, dissolution, entity) in
                SystemAPI.Query<RefRW<TartarianBuilding>, RefRO<MudDissolution>>()
                    .WithEntityAccess())
            {
                if (building.ValueRO.State == BuildingRestorationState.Emerging
                    && dissolution.ValueRO.Progress >= 1.0f)
                {
                    building.ValueRW.State = BuildingRestorationState.Active;
                    building.ValueRW.RestorationProgress = 1.0f;
                }
            }
        }
    }

    /// <summary>
    /// Discovery System — detects when the player is near undiscovered
    /// buildings or POIs and triggers discovery events.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DiscoverySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get player position
            float3 playerPos = float3.zero;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                break;
            }

            // Check all undiscovered triggers
            foreach (var (trigger, transform, building) in
                SystemAPI.Query<RefRW<DiscoveryTrigger>, RefRO<LocalTransform>,
                    RefRW<TartarianBuilding>>())
            {
                if (trigger.ValueRO.Discovered) continue;

                float dist = math.distance(playerPos, transform.ValueRO.Position);
                if (dist <= trigger.ValueRO.TriggerRadius)
                {
                    trigger.ValueRW.Discovered = true;

                    // Building transitions from Buried → Revealed
                    if (building.ValueRO.State == BuildingRestorationState.Buried)
                    {
                        building.ValueRW.State = BuildingRestorationState.Revealed;
                    }
                }
            }
        }
    }

    // NOTE: PlayerTag moved to Tartaria.Core.ResonanceComponents
    // to allow GameBootstrap (in Core) to create the player entity.
}
