using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Enemy AI System — Mud Golem behavior:
    ///   SPAWNING → PATROLLING → ENGAGING → (STUNNED) → DISSOLVING
    ///
    /// Golems patrol toward the nearest restored building.
    /// Speed: 0.6× player speed. Attack: melee slam (20 dmg, 1s windup).
    /// Weakness: 3 consecutive Resonance Pulses → 2s stun.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemyAISystem : ISystem
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

            // Update each enemy
            foreach (var (ai, combatant, transform, golem) in
                SystemAPI.Query<RefRW<EnemyAI>, RefRW<HarmonicCombatant>,
                    RefRW<LocalTransform>, RefRW<MudGolem>>())
            {
                ai.ValueRW.StateTimer += dt;

                switch (ai.ValueRO.State)
                {
                    case EnemyAIState.Spawning:
                        UpdateSpawning(ref ai.ValueRW);
                        break;

                    case EnemyAIState.Patrolling:
                        UpdatePatrolling(ref ai.ValueRW, ref transform.ValueRW,
                            playerPos, dt);
                        break;

                    case EnemyAIState.Engaging:
                        UpdateEngaging(ref ai.ValueRW, ref transform.ValueRW,
                            ref golem.ValueRW, playerPos, dt);
                        break;

                    case EnemyAIState.Stunned:
                        UpdateStunned(ref ai.ValueRW, ref golem.ValueRW, dt);
                        break;

                    case EnemyAIState.Dissolving:
                        // Handled by dissolution VFX system
                        break;
                }

                // Check for death → dissolution
                if (combatant.ValueRO.Health <= 0f &&
                    ai.ValueRO.State != EnemyAIState.Dissolving)
                {
                    ai.ValueRW.State = EnemyAIState.Dissolving;
                    ai.ValueRW.StateTimer = 0f;
                }

                // Check stun condition (3 consecutive Resonance Pulse hits)
                if (golem.ValueRO.ConsecutivePulseHits >= 3 &&
                    ai.ValueRO.State == EnemyAIState.Engaging)
                {
                    ai.ValueRW.State = EnemyAIState.Stunned;
                    ai.ValueRW.StateTimer = 0f;
                    golem.ValueRW.StunTimer = golem.ValueRO.StunDuration;
                    golem.ValueRW.ConsecutivePulseHits = 0;
                }
            }
        }

        void UpdateSpawning(ref EnemyAI ai)
        {
            // 3s grace period — golem forming from mud
            if (ai.StateTimer >= ai.SpawnGracePeriod)
            {
                ai.State = EnemyAIState.Patrolling;
                ai.StateTimer = 0f;
            }
        }

        void UpdatePatrolling(ref EnemyAI ai, ref LocalTransform transform,
            float3 playerPos, float dt)
        {
            // Move toward patrol target (nearest restored building)
            float3 direction = math.normalizesafe(ai.PatrolTarget - transform.Position);
            transform.Position += direction * ai.MoveSpeed * dt;
            if (!direction.Equals(float3.zero))
                transform.Rotation = quaternion.LookRotation(direction, math.up());

            // Check if player is within engagement radius
            float distToPlayer = math.distance(transform.Position, playerPos);
            if (distToPlayer <= ai.EngageRadius)
            {
                ai.State = EnemyAIState.Engaging;
                ai.StateTimer = 0f;
            }
        }

        void UpdateEngaging(ref EnemyAI ai, ref LocalTransform transform,
            ref MudGolem golem, float3 playerPos, float dt)
        {
            float distToPlayer = math.distance(transform.Position, playerPos);

            // Move toward player
            if (distToPlayer > ai.AttackRange)
            {
                float3 direction = math.normalizesafe(playerPos - transform.Position);
                transform.Position += direction * ai.MoveSpeed * dt;
                if (!direction.Equals(float3.zero))
                    transform.Rotation = quaternion.LookRotation(direction, math.up());
            }

            // Attack when in range and cooldown expired
            if (distToPlayer <= ai.AttackRange && ai.AttackCooldown <= 0f)
            {
                // Queue attack (1s windup)
                ai.AttackCooldown = golem.AttackWindup + 1.0f;
                // Damage event will be created after windup completes
            }

            ai.AttackCooldown = math.max(0f, ai.AttackCooldown - dt);

            // Disengage if player moves far away
            if (distToPlayer > ai.EngageRadius * 1.5f)
            {
                ai.State = EnemyAIState.Patrolling;
                ai.StateTimer = 0f;
            }
        }

        void UpdateStunned(ref EnemyAI ai, ref MudGolem golem, float dt)
        {
            golem.StunTimer -= dt;
            if (golem.StunTimer <= 0f)
            {
                ai.State = EnemyAIState.Engaging;
                ai.StateTimer = 0f;
            }
        }
    }
}
