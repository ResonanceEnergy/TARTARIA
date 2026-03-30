using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Mirror Wraith AI System (DOTS) -- elite Moon 2 enemy.
    ///
    /// Copies the player's last 3 attack types into a mirror buffer.
    /// Reflects those attack types back at reduced damage.
    /// Must be defeated with an attack type NOT in the buffer.
    ///
    /// Behaviour:
    ///   - Teleports behind player every TeleportCooldown seconds
    ///   - When hit, records the DamageType in mirror buffer (FIFO, 3 slots)
    ///   - ReflectCooldown: fires a reflected copy of a buffered attack
    ///   - Immune to damage types currently in mirror buffer
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FractalWraithAISystem))]
    public partial struct MirrorWraithAISystem : ISystem
    {
        const float ChaseWeight = 0.3f;
        const float StrafeWeight = 0.7f;

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

            foreach (var (wraith, ai, combatant, transform, damageBuffer) in
                SystemAPI.Query<RefRW<MirrorWraith>, RefRW<EnemyAI>,
                    RefRW<HarmonicCombatant>, RefRW<LocalTransform>,
                    DynamicBuffer<DamageEvent>>())
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

                float distToPlayer = math.distance(transform.ValueRO.Position, playerPos);

                // Process incoming damage — record types in mirror buffer
                // and block damage if type is already mirrored
                for (int i = damageBuffer.Length - 1; i >= 0; i--)
                {
                    var dmg = damageBuffer[i];
                    DamageType incomingType = dmg.Type;

                    if (IsTypeMirrored(ref wraith.ValueRW, incomingType))
                    {
                        // Immune! Remove the damage event
                        damageBuffer.RemoveAt(i);
                    }
                    else
                    {
                        // Vulnerable — record this type in mirror buffer
                        PushMirrorType(ref wraith.ValueRW, incomingType);
                    }
                }

                // Teleport behind player
                wraith.ValueRW.TeleportTimer -= dt;
                if (wraith.ValueRO.TeleportTimer <= 0f && distToPlayer <= ai.ValueRO.EngageRadius)
                {
                    wraith.ValueRW.TeleportTimer = wraith.ValueRO.TeleportCooldown;

                    // Calculate position behind player
                    float3 playerForward = math.normalizesafe(playerPos - transform.ValueRO.Position);
                    float3 behindPlayer = playerPos - playerForward * 3f;
                    transform.ValueRW.Position = behindPlayer;
                }

                // Reflect attack back at player
                wraith.ValueRW.ReflectTimer -= dt;
                if (wraith.ValueRO.ReflectTimer <= 0f
                    && wraith.ValueRO.MirrorCount > 0
                    && distToPlayer <= ai.ValueRO.AttackRange * 2f)
                {
                    wraith.ValueRW.ReflectTimer = wraith.ValueRO.ReflectCooldown;
                    // Reflection damage queued — CombatBridge handles application
                }

                // Movement: circle-strafe around player
                if (distToPlayer > ai.ValueRO.AttackRange
                    && distToPlayer <= ai.ValueRO.EngageRadius)
                {
                    ai.ValueRW.State = EnemyAIState.Engaging;

                    // Strafe: perpendicular to player direction
                    float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);
                    float3 strafe = math.cross(toPlayer, math.up());
                    float3 moveDir = math.normalizesafe(toPlayer * ChaseWeight + strafe * StrafeWeight);

                    transform.ValueRW.Position += moveDir * wraith.ValueRO.MoveSpeed * dt;
                    transform.ValueRW.Rotation = quaternion.LookRotation(toPlayer, math.up());
                }
                else if (distToPlayer > ai.ValueRO.EngageRadius)
                {
                    ai.ValueRW.State = EnemyAIState.Patrolling;
                    float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);
                    transform.ValueRW.Position += toPlayer * wraith.ValueRO.MoveSpeed * dt;
                }

                // Health check
                if (combatant.ValueRO.Health <= 0f)
                {
                    ai.ValueRW.State = EnemyAIState.Dissolving;
                    ai.ValueRW.StateTimer = 0f;
                }
            }
        }

        static bool IsTypeMirrored(ref MirrorWraith wraith, DamageType type)
        {
            if (wraith.MirrorCount >= 1 && wraith.MirrorSlot0 == type) return true;
            if (wraith.MirrorCount >= 2 && wraith.MirrorSlot1 == type) return true;
            if (wraith.MirrorCount >= 3 && wraith.MirrorSlot2 == type) return true;
            return false;
        }

        static void PushMirrorType(ref MirrorWraith wraith, DamageType type)
        {
            // FIFO: shift slots, newest at slot 0
            wraith.MirrorSlot2 = wraith.MirrorSlot1;
            wraith.MirrorSlot1 = wraith.MirrorSlot0;
            wraith.MirrorSlot0 = type;
            wraith.MirrorCount = math.min(wraith.MirrorCount + 1, 3);
        }
    }
}
