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
    /// <summary>Combat balance constants — single source of truth for tuning values.</summary>
    public static class CombatBalance
    {
        // Resonance Pulse — AOE frequency-matched attack
        public const float PulseFreqTolerance = 20f;
        public const float PulseFreqMatchBonus = 1.5f;

        // Harmonic Strike — directed frequency-matched attack
        public const float StrikeBaseMultiplier = 1.25f;
        public const float StrikeFreqTolerance = 10f;
        public const float StrikeTightMatchBonus = 1.6f;

        // Enemy defaults
        public const float DefaultEnemyHP = 100f;
        public const float DefaultDissonantFreq = 174f;
        public const float DefaultMoveSpeed = 4f;
        public const float DefaultAttackRange = 3f;
        public const float BossAttackRange = 8f;
    }

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
                        // Damage type modifiers
                        float finalDamage = dmg.Amount;
                        switch (dmg.Type)
                        {
                            case DamageType.ResonancePulse:
                                // AOE: base damage, bonus if frequency near target's
                                float freqDelta = math.abs(dmg.Frequency - combatant.ValueRO.CurrentFrequency);
                                if (freqDelta < CombatBalance.PulseFreqTolerance)
                                    finalDamage *= CombatBalance.PulseFreqMatchBonus;
                                break;
                            case DamageType.HarmonicStrike:
                                // Directed: 1.25x base, 2x if frequency-matched
                                finalDamage *= CombatBalance.StrikeBaseMultiplier;
                                float hsDelta = math.abs(dmg.Frequency - combatant.ValueRO.CurrentFrequency);
                                if (hsDelta < CombatBalance.StrikeFreqTolerance)
                                    finalDamage *= CombatBalance.StrikeTightMatchBonus;
                                break;
                            case DamageType.GolemSlam:
                                // Enemy melee: flat damage, ignores frequency
                                break;
                            case DamageType.Environmental:
                                // Environmental: bypasses armor entirely
                                break;
                            default:
                                break;
                        }
                        combatant.ValueRW.Health -= finalDamage;
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
    /// Uses deferred structural changes to avoid modifying entities mid-iteration.
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

            // Collect triggers that need spawning (cannot do structural changes mid-iteration)
            var pendingSpawns = new NativeList<EnemySpawnTrigger>(4, Allocator.Temp);

            foreach (var trigger in SystemAPI.Query<RefRW<EnemySpawnTrigger>>())
            {
                if (!trigger.ValueRO.HasSpawned && currentRS >= trigger.ValueRO.RSThreshold)
                {
                    trigger.ValueRW.HasSpawned = true;
                    pendingSpawns.Add(trigger.ValueRO);
                }
            }

            // Now perform structural changes outside the query iteration
            for (int i = 0; i < pendingSpawns.Length; i++)
            {
                SpawnEnemy(ref state, pendingSpawns[i]);
            }

            pendingSpawns.Dispose();
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

            // Per-type stats: HP, frequency, move speed, and type-specific component
            float hp = CombatBalance.DefaultEnemyHP;
            float freq = CombatBalance.DefaultDissonantFreq;
            float moveSpeed = CombatBalance.DefaultMoveSpeed;
            float attackRange = CombatBalance.DefaultAttackRange;

            switch (trigger.EnemyToSpawn)
            {
                case EnemyType.MudGolem:
                    hp = 100f; moveSpeed = 3.5f;
                    em.AddComponentData(entity, new MudGolem { AttackWindup = 1f, AttackDamage = 20f, StunDuration = 2f, PatrolRadius = 20f });
                    break;
                case EnemyType.FractalWraith:
                    hp = 60f; moveSpeed = 3.2f; freq = 220f;
                    em.AddComponentData(entity, new FractalWraith { PhaseCycleDuration = 4f, MaterialiseWindow = 1.5f, AttackDamage = 12f, MoveSpeed = 3.2f, AetherDrainPerSecond = 5f });
                    break;
                case EnemyType.MirrorWraith:
                    hp = 80f; moveSpeed = 2.8f; freq = 256f;
                    em.AddComponentData(entity, new MirrorWraith { AttackDamage = 18f, MoveSpeed = 2.8f, ReflectDamageMultiplier = 0.75f, ReflectCooldown = 2f, TeleportCooldown = 5f });
                    break;
                case EnemyType.RailWraith:
                    hp = 90f; moveSpeed = 6f; freq = 190f;
                    em.AddComponentData(entity, new RailWraith { RailSpeed = 6f, AttackDamage = 22f, TunedRailDamage = 40f });
                    break;
                case EnemyType.DissonanceHarvester:
                    hp = 70f; moveSpeed = 3.6f; freq = 200f;
                    em.AddComponentData(entity, new DissonanceHarvester { DrainRate = 8f, AttackDamage = 14f, MoveSpeed = 3.6f, DetectionRadius = 30f });
                    break;
                case EnemyType.DissonanceLeviathan:
                    hp = 500f; moveSpeed = 5f; freq = 160f; attackRange = 8f;
                    em.AddComponentData(entity, new DissonanceLeviathan { TotalHP = 500f, AttackDamage = 30f, BodyLength = 12f, ChargeSpeed = 8f, LullabySusceptibility = 2f });
                    break;
                case EnemyType.SiegeGolem:
                    hp = 250f; moveSpeed = 3f; freq = 174f;
                    em.AddComponentData(entity, new SiegeGolem { AttackDamage = 35f, ChargeSpeed = 8f, ChargeCooldown = 6f, ArmorReduction = 0.5f, WallBreachDamage = 50f });
                    break;
                case EnemyType.HarmonicParasite:
                    hp = 30f; moveSpeed = 4.4f; freq = 300f;
                    em.AddComponentData(entity, new HarmonicParasite { FeedRate = 3f, AttackDamage = 10f, MoveSpeed = 4.4f, SpawnCooldown = 4f });
                    break;
                case EnemyType.DissonantConductor:
                    hp = 120f; moveSpeed = 2.5f; freq = 180f;
                    em.AddComponentData(entity, new DissonantConductor { CommandRadius = 25f, AttackDamage = 20f, MaxControlledParasites = 6, CompositionWindow = 10f });
                    break;
                case EnemyType.CorruptedCraft:
                    hp = 40f; moveSpeed = 6f; freq = 240f;
                    em.AddComponentData(entity, new CorruptedCraft { FlightSpeed = 6f, AttackDamage = 8f, BombCooldown = 3f });
                    break;
                case EnemyType.SkyReaver:
                    hp = 110f; moveSpeed = 4.8f; freq = 270f;
                    em.AddComponentData(entity, new SkyReaver { AttackDamage = 25f, MoveSpeed = 4.8f, PhaseDuration = 3f, LeyLineDamageMultiplier = 1.5f });
                    break;
                case EnemyType.ProphecyGuardian:
                    hp = 200f; moveSpeed = 2f; freq = 432f;
                    em.AddComponentData(entity, new ProphecyGuardian { AttackDamage = 28f, HarmonicTestThreshold = 60f, TestTimer = 15f, ArmorReduction = 0.4f });
                    break;
                case EnemyType.ResetSeeker:
                    hp = 65f; moveSpeed = 5.2f; freq = 210f;
                    em.AddComponentData(entity, new ResetSeeker { AttackDamage = 16f, MoveSpeed = 5.2f, GrenadeCooldown = 4f, GrenadeRadius = 8f, DissonanceDuration = 3f, SquadSize = 3 });
                    break;
                case EnemyType.TemporalWraith:
                    hp = 85f; moveSpeed = 4f; freq = 0f;
                    em.AddComponentData(entity, new TemporalWraith { AttackDamage = 22f, PhaseInDuration = 1.5f, PhaseOutDuration = 3f, FrequencyShift = 50f });
                    break;
                case EnemyType.LivingSludge:
                    hp = 80f; moveSpeed = 2f; freq = 150f;
                    em.AddComponentData(entity, new LivingSludge { AttackDamage = 12f, RegenerationRate = 5f, MoveSpeed = 2f, TendrilRange = 5f, PurificationVulnerability = 2f });
                    break;
                case EnemyType.SludgeLeviathan:
                    hp = 600f; moveSpeed = 3f; freq = 140f; attackRange = 10f;
                    em.AddComponentData(entity, new SludgeLeviathan { TotalHP = 600f, AttackDamage = 40f, WaterPressureDamage = 30f, GiantModeDamageMultiplier = 1.5f, PurificationThreshold = 100f });
                    break;
                case EnemyType.TitanGolem:
                    hp = 400f; moveSpeed = 2.5f; freq = 174f; attackRange = 10f;
                    em.AddComponentData(entity, new TitanGolem { AttackDamage = 45f, ArmorReduction = 0.6f, Height = 20f, StompRadius = 10f, StompCooldown = 8f, RequiresGiantMode = true });
                    break;
                case EnemyType.FrequencyWraith:
                    hp = 75f; moveSpeed = 3.6f; freq = 200f;
                    em.AddComponentData(entity, new FrequencyWraith { AttackDamage = 20f, MoveSpeed = 3.6f, FrequencyShiftInterval = 5f, FrequencyTolerance = 15f });
                    break;
                default:
                    em.AddComponentData(entity, new MudGolem { AttackWindup = 1f, AttackDamage = 20f, StunDuration = 2f, PatrolRadius = 20f });
                    break;
            }

            em.AddComponentData(entity, new HarmonicCombatant
            {
                Health = hp,
                MaxHealth = hp,
                AetherCharge = 0f,
                MaxAetherCharge = 50f,
                CurrentFrequency = freq,
                ComboCount = 0,
                IsGiantMode = false
            });

            em.AddComponentData(entity, new EnemyAI
            {
                State = EnemyAIState.Spawning,
                StateTimer = 0f,
                PatrolTarget = trigger.SpawnPosition,
                EngageRadius = 50f,
                AttackRange = attackRange,
                SpawnGracePeriod = 3f,
                MoveSpeed = moveSpeed,
                AttackCooldown = 0f
            });

            em.AddComponentData(entity, new LocalToWorld { Value = float4x4.TRS(
                trigger.SpawnPosition, quaternion.identity, new float3(1f)) });

            em.AddBuffer<DamageEvent>(entity);
        }
    }
}
