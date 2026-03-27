using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Fractal Wraith AI System (DOTS) -- handles the phase/materialise cycle.
    ///
    /// Behaviour loop:
    ///   PHASED (2.5s): Intangible, drifts toward nearest restored building,
    ///                   drains Aether on contact. Cannot be damaged.
    ///   MATERIALISE (1.5s): Becomes solid, attacks if player nearby.
    ///                       Vulnerable to Resonance Pulse only in this window.
    ///
    /// When HP reaches 0 during materialise window → Dissolving state.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CompanionBehaviorSystem))]
    public partial struct FractalWraithAISystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Get player position
            float3 playerPos = float3.zero;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                break;
            }

            foreach (var (wraith, ai, combatant, transform) in
                SystemAPI.Query<RefRW<FractalWraith>, RefRW<EnemyAI>,
                    RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning)
                {
                    ai.ValueRW.StateTimer += dt;
                    if (ai.ValueRW.StateTimer >= ai.ValueRO.SpawnGracePeriod)
                    {
                        ai.ValueRW.State = EnemyAIState.Patrolling;
                        ai.ValueRW.StateTimer = 0f;
                    }
                    continue;
                }

                // Phase cycle timer
                wraith.ValueRW.PhaseTimer += dt;
                if (wraith.ValueRW.PhaseTimer >= wraith.ValueRO.PhaseCycleDuration)
                    wraith.ValueRW.PhaseTimer = 0f;

                // Determine phase state: first 2.5s = phased, last 1.5s = material
                float materialiseStart = wraith.ValueRO.PhaseCycleDuration
                    - wraith.ValueRO.MaterialiseWindow;
                bool wasPhased = wraith.ValueRO.IsPhased;
                wraith.ValueRW.IsPhased = wraith.ValueRO.PhaseTimer < materialiseStart;

                if (wraith.ValueRO.IsPhased)
                {
                    // PHASED: Move toward target building, drain Aether
                    float3 dir = math.normalizesafe(
                        wraith.ValueRO.TargetBuildingPos - transform.ValueRO.Position);
                    transform.ValueRW.Position += dir * wraith.ValueRO.MoveSpeed * dt;

                    if (dir.x != 0 || dir.z != 0)
                        transform.ValueRW.Rotation = quaternion.LookRotation(
                            new float3(dir.x, 0, dir.z), math.up());

                    // Aether drain handled by MonoBehaviour bridge (needs CorruptionSystem)
                }
                else
                {
                    // MATERIALISED: Engage player if close
                    float distToPlayer = math.distance(
                        transform.ValueRO.Position, playerPos);

                    if (distToPlayer <= ai.ValueRO.AttackRange)
                    {
                        ai.ValueRW.State = EnemyAIState.Engaging;
                        ai.ValueRW.AttackCooldown -= dt;

                        if (ai.ValueRO.AttackCooldown <= 0f)
                        {
                            ai.ValueRW.AttackCooldown = 2f;
                            // Attack queued — damage applied by CombatBridge
                        }
                    }
                    else if (distToPlayer <= ai.ValueRO.EngageRadius)
                    {
                        // Move toward player while materialised
                        float3 toPlayer = math.normalizesafe(
                            playerPos - transform.ValueRO.Position);
                        transform.ValueRW.Position += toPlayer
                            * wraith.ValueRO.MoveSpeed * 0.8f * dt;
                    }
                }

                // Health check
                if (combatant.ValueRO.Health <= 0f)
                {
                    ai.ValueRW.State = EnemyAIState.Dissolving;
                    ai.ValueRW.StateTimer = 0f;
                }
            }
        }
    }
}
