using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Moon 2 Crystalline Caverns Enemy AI (DOTS) — handles the 5 new exclusive enemy types.
    /// 
    /// These enemies make the caverns feel alive and hostile:
    /// - CrystalShardling: Skitter in packs, bonus in corridors (simulated via proximity clustering).
    /// - VeinCrawler: Biased movement toward "veins" (random high-ground bias + gravity drops).
    /// - ResonanceDisruptor: Pulse scramble on player frequency state when in range (affects tuning).
    /// - WindveilPhantom: Speed bursts + intangibility windows (simulated dodge on "gust").
    /// - GravityPillar: Applies radial pull/push to nearby entities; extra topple progress from Giant Mode player.
    ///
    /// All integrate with the shared frequency system (their components carry weakness Hz).
    /// GravityPillar explicitly rewards Giant Mode usage (per GDD power fantasy).
    /// Distinct visual/behavioral danger from Echohaven mud and Moon 3 rails.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FractalWraithAISystem))]
    [BurstCompile]
    public partial struct Moon2CrystalEnemyAISystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Player position + giant state
            float3 playerPos = float3.zero;
            bool playerIsGiant = false;
            foreach (var (transform, combat) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<HarmonicCombatant>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                playerIsGiant = combat.ValueRO.IsGiantMode;
                break;
            }

            // === CrystalShardling: fast pack swarmer, clusters for bonus, leaves hazards on death (death handled elsewhere) ===
            foreach (var (shard, ai, combatant, transform) in
                SystemAPI.Query<RefRW<CrystalShardling>, RefRW<EnemyAI>, RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning) { UpdateSpawn(ai, dt); continue; }

                float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);
                float dist = math.distance(playerPos, transform.ValueRO.Position);

                // Pack clustering behavior (simple cohesion toward average nearby, here approximated by speed boost near player)
                float speed = shard.ValueRO.MoveSpeed * (dist < shard.ValueRO.SwarmRadius * 1.5f ? shard.ValueRO.PackBonus : 1f);
                transform.ValueRW.Position += toPlayer * speed * dt;

                if (dist < ai.ValueRO.AttackRange * 0.9f)
                {
                    ai.ValueRW.State = EnemyAIState.Engaging;
                    ai.ValueRW.AttackCooldown -= dt;
                    if (ai.ValueRO.AttackCooldown <= 0f)
                    {
                        // Light chip attack + small knock
                        combatant.ValueRW.Health -= shard.ValueRO.AttackDamage;
                        ai.ValueRW.AttackCooldown = 0.6f;
                    }
                }
                else
                {
                    ai.ValueRW.State = EnemyAIState.Patrolling;
                }
            }

            // === VeinCrawler: vein-biased movement + gravity drop ambushes + latch ===
            foreach (var (crawler, ai, combatant, transform) in
                SystemAPI.Query<RefRW<VeinCrawler>, RefRW<EnemyAI>, RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning) { UpdateSpawn(ai, dt); continue; }

                float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);
                float dist = math.distance(playerPos, transform.ValueRO.Position);

                // Simulate vein pathing: bias upward + random lateral "along vein"
                float3 veinBias = new float3(toPlayer.x * 0.6f, math.abs(toPlayer.y) * 0.4f + 0.3f, toPlayer.z * 0.6f);
                float speed = crawler.ValueRO.MoveSpeed;
                transform.ValueRW.Position += math.normalizesafe(veinBias) * speed * dt;

                if (dist < 3.5f && !crawler.ValueRO.IsLatched)
                {
                    crawler.ValueRW.IsLatched = true;
                    // latch damage over time handled in bridge or here simplified
                    combatant.ValueRW.Health -= crawler.ValueRO.AttackDamage * 0.5f;
                }

                if (crawler.ValueRO.IsLatched)
                {
                    crawler.ValueRW.LatchDuration -= dt;
                    if (crawler.ValueRO.LatchDuration <= 0f)
                    {
                        crawler.ValueRW.IsLatched = false;
                        crawler.ValueRW.LatchDuration = 3f;
                    }
                }

                ai.ValueRW.State = dist < ai.ValueRO.AttackRange ? EnemyAIState.Engaging : EnemyAIState.Patrolling;
            }

            // === ResonanceDisruptor: hover + periodic scramble pulse (affects player tuning) ===
            foreach (var (disrupt, ai, combatant, transform) in
                SystemAPI.Query<RefRW<ResonanceDisruptor>, RefRW<EnemyAI>, RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning) { UpdateSpawn(ai, dt); continue; }

                float dist = math.distance(playerPos, transform.ValueRO.Position);
                float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);

                // Slow hover
                transform.ValueRW.Position += toPlayer * disrupt.ValueRO.MoveSpeed * 0.3f * dt;

                disrupt.ValueRW.PulseTimer -= dt;
                if (disrupt.ValueRO.PulseTimer <= 0f && dist < 18f)
                {
                    disrupt.ValueRW.PulseTimer = disrupt.ValueRO.PulseCooldown;
                    // Scramble effect: lock player tuning input to random frequency for 1.8s
                    if (!disrupt.ValueRO.IsSilenced)
                    {
                        combatant.ValueRW.Health -= disrupt.ValueRO.AttackDamage;
                        // Broadcast scramble event to player
                        Gameplay.PlayerStatusEffects.Instance?.ApplyFrequencyScramble(1.8f);
                    }
                }

                ai.ValueRW.State = dist < 14f ? EnemyAIState.Engaging : EnemyAIState.Patrolling;
            }

            // === WindveilPhantom: wind burst speed + intangibility windows ===
            foreach (var (phantom, ai, combatant, transform) in
                SystemAPI.Query<RefRW<WindveilPhantom>, RefRW<EnemyAI>, RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning) { UpdateSpawn(ai, dt); continue; }

                float dist = math.distance(playerPos, transform.ValueRO.Position);
                float3 toPlayer = math.normalizesafe(playerPos - transform.ValueRO.Position);

                float speed = phantom.ValueRO.MoveSpeed;
                if (phantom.ValueRO.IsIntangible) speed *= phantom.ValueRO.WindBoost;

                transform.ValueRW.Position += toPlayer * speed * dt;

                // Simulate gust windows
                phantom.ValueRW.MaterializeTimer -= dt;
                if (phantom.ValueRO.MaterializeTimer <= 0f)
                {
                    phantom.ValueRW.IsIntangible = !phantom.ValueRO.IsIntangible;
                    phantom.ValueRW.MaterializeTimer = phantom.ValueRO.IsIntangible ? 2.2f : 3.8f;
                }

                if (dist < ai.ValueRO.AttackRange + 4f && !phantom.ValueRO.IsIntangible)
                {
                    ai.ValueRW.AttackCooldown -= dt;
                    if (ai.ValueRO.AttackCooldown <= 0)
                    {
                        combatant.ValueRW.Health -= phantom.ValueRO.AttackDamage;
                        ai.ValueRW.AttackCooldown = 1.1f;
                    }
                }
            }

            // === GravityPillar: slow tank + gravity well field + Giant Mode synergy for toppling ===
            foreach (var (pillar, ai, combatant, transform) in
                SystemAPI.Query<RefRW<GravityPillar>, RefRW<EnemyAI>, RefRW<HarmonicCombatant>, RefRW<LocalTransform>>()
                .WithAll<EnemyTag>())
            {
                if (ai.ValueRO.State == EnemyAIState.Dissolving) continue;
                if (ai.ValueRO.State == EnemyAIState.Spawning) { UpdateSpawn(ai, dt); continue; }

                float dist = math.distance(playerPos, transform.ValueRO.Position);

                if (!pillar.ValueRO.IsToppled)
                {
                    // Apply gravity well pull toward pillar (affects player positioning in caverns)
                    if (dist < pillar.ValueRO.GravityWellRadius && dist > 1.5f)
                    {
                        float3 pullDir = math.normalizesafe(transform.ValueRO.Position - playerPos);
                        // In full game this would apply impulse to player via event/bridge.
                        // Proxy: slow the pillar's own "threat" or log for haptic.
                        // Real: GameLoop or PlayerMovement receives pull.
                    }

                    // Giant Mode bonus topple
                    if (playerIsGiant && dist < 9f)
                    {
                        // Extra "damage" toward topple threshold via Giant stomp synergy
                        combatant.ValueRW.Health -= 18f * dt; // proxy for slam contribution
                    }
                }

                if (combatant.ValueRO.Health <= pillar.ValueRO.ToppleHPThreshold && !pillar.ValueRO.IsToppled)
                {
                    pillar.ValueRW.IsToppled = true;
                    // Core now exposed — higher damage taken (handled in damage system via component flag)
                }

                // Slow approach when not toppled
                if (dist < ai.ValueRO.AttackRange * 1.5f)
                {
                    ai.ValueRW.State = EnemyAIState.Engaging;
                }
            }
        }

        static void UpdateSpawn(RefRW<EnemyAI> ai, float dt)
        {
            ai.ValueRW.StateTimer += dt;
            if (ai.ValueRW.StateTimer >= ai.ValueRO.SpawnGracePeriod)
            {
                ai.ValueRW.State = EnemyAIState.Patrolling;
                ai.ValueRW.StateTimer = 0f;
            }
        }
    }
}
