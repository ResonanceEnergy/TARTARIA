using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Harmonic Combat System — processes attacks, applies frequency-matched
    /// damage, manages combos, and handles enemy dissolution.
    ///
    /// Combat philosophy: retune, don't destroy. Enemies dissolve into
    /// purified Aether when their dissonant frequency is corrected.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct HarmonicCombatSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResonanceScore>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Update player cooldowns
            foreach (var combatState in SystemAPI.Query<RefRW<PlayerCombatState>>())
            {
                if (combatState.ValueRO.ResonancePulseCooldown > 0f)
                    combatState.ValueRW.ResonancePulseCooldown -= dt;
                if (combatState.ValueRO.HarmonicStrikeCooldown > 0f)
                    combatState.ValueRW.HarmonicStrikeCooldown -= dt;
                if (combatState.ValueRO.FrequencyShieldCooldown > 0f)
                    combatState.ValueRW.FrequencyShieldCooldown -= dt;

                // Shield expires
                if (combatState.ValueRO.IsShielding)
                {
                    combatState.ValueRW.ShieldActiveTime -= dt;
                    if (combatState.ValueRO.ShieldActiveTime <= 0f)
                        combatState.ValueRW.IsShielding = false;
                }
            }

            // Update Mud Golem stun timers
            foreach (var golem in SystemAPI.Query<RefRW<MudGolem>>())
            {
                if (golem.ValueRO.StunTimer > 0f)
                    golem.ValueRW.StunTimer -= dt;
            }

            // Process damage events
            foreach (var (combatant, damageBuffer, entity) in
                SystemAPI.Query<RefRW<HarmonicCombatant>, DynamicBuffer<DamageEvent>>()
                    .WithEntityAccess())
            {
                for (int i = 0; i < damageBuffer.Length; i++)
                {
                    var dmg = damageBuffer[i];

                    // Shields absorb damage
                    bool absorbed = false;
                    if (state.EntityManager.HasComponent<PlayerCombatState>(entity))
                    {
                        var pcs = state.EntityManager.GetComponentData<PlayerCombatState>(entity);
                        if (pcs.IsShielding)
                        {
                            absorbed = true;
                            continue;
                        }
                    }

                    if (!absorbed)
                    {
                        combatant.ValueRW.Health -= dmg.Amount;
                    }
                }
                damageBuffer.Clear();

                // Check for death/dissolution
                if (combatant.ValueRO.Health <= 0f &&
                    state.EntityManager.HasComponent<EnemyTag>(entity))
                {
                    // Enemy dissolved — will be cleaned up by dissolution system
                    combatant.ValueRW.Health = 0f;
                }
            }
        }
    }

    /// <summary>
    /// Enemy Spawn System — spawns enemies when RS thresholds are crossed.
    /// Phase 1: 1 Mud Golem each at RS 25, 50, 75.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResonanceScore>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float currentRS = 0f;
            foreach (var score in SystemAPI.Query<RefRO<ResonanceScore>>())
            {
                currentRS = score.ValueRO.CurrentRS;
                break;
            }

            // Check spawn triggers
            foreach (var trigger in SystemAPI.Query<RefRW<EnemySpawnTrigger>>())
            {
                if (!trigger.ValueRO.HasSpawned && currentRS >= trigger.ValueRO.RSThreshold)
                {
                    trigger.ValueRW.HasSpawned = true;
                    SpawnEnemy(ref state, trigger.ValueRO);
                }
            }
        }

        void SpawnEnemy(ref SystemState state, EnemySpawnTrigger trigger)
        {
            var em = state.EntityManager;
            var entity = em.CreateEntity();

            em.AddComponentData(entity, new LocalTransform
            {
                Position = trigger.SpawnPosition,
                Rotation = quaternion.identity,
                Scale = 1f
            });

            em.AddComponentData(entity, new EnemyTag { Type = trigger.EnemyToSpawn });

            em.AddComponentData(entity, new HarmonicCombatant
            {
                Health = 100f,
                MaxHealth = 100f,
                AetherCharge = 0f,
                MaxAetherCharge = 50f,
                CurrentFrequency = 174f, // Dissonant
                ComboCount = 0,
                IsGiantMode = false
            });

            em.AddComponentData(entity, new MudGolem
            {
                AttackWindup = 1.0f,
                AttackDamage = 20f,
                StunDuration = 2.0f,
                ConsecutivePulseHits = 0,
                StunTimer = 0f,
                PatrolRadius = 20f
            });

            em.AddComponentData(entity, new EnemyAI
            {
                State = EnemyAIState.Spawning,
                StateTimer = 0f,
                PatrolTarget = trigger.SpawnPosition,
                EngageRadius = 50f,
                AttackRange = 3f,
                SpawnGracePeriod = 3f,
                MoveSpeed = 4f,
                AttackCooldown = 0f
            });

            em.AddComponentData(entity, new LocalToWorld { Value = float4x4.TRS(
                trigger.SpawnPosition, quaternion.identity, new float3(1f)) });

            em.AddBuffer<DamageEvent>(entity);
        }
    }
}
