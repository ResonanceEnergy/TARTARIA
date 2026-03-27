using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Lirael-specific ECS component — Crystal Singer companion.
    ///
    /// Lirael joins at Moon 2 and has unique abilities:
    ///   - Blueprint Projection: highlights building restoration targets
    ///   - Corruption Memory: remembers purified corruption patterns
    ///   - Healing Harmony: passive Aether regen near player
    ///   - Crystal Resonance: boosts tuning accuracy in mini-games
    ///
    /// Personality: Calm, deliberate, melancholic about lost Tartarian beauty.
    /// </summary>
    public struct LirealPersonality : IComponentData
    {
        public float Wisdom;           // Affects dialogue depth
        public float Empathy;          // Affects healing potency
        public float Precision;        // Affects blueprint accuracy
        public float CorruptionMemory; // How many patterns remembered (0-10)
    }

    /// <summary>
    /// Lirael Behavior System (DOTS) — extends base CompanionBehavior.
    ///
    /// Additional states beyond base companion:
    ///   BLUEPRINT: Projects holographic building outline when near buried structures
    ///   HEALING:   Generates passive Aether field when player health is low
    ///   SCANNING:  Detects nearby corruption (works with Dissonance Lens)
    ///
    /// State machine extends CompanionBehaviorSystem transitions.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CompanionBehaviorSystem))]
    public partial struct LirealBehaviorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Get player position and health
            float3 playerPos = float3.zero;
            float playerHealth = 100f;
            foreach (var (transform, combatant) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<HarmonicCombatant>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                playerHealth = combatant.ValueRO.Health;
                break;
            }

            // Check for corruption (any FractalWraith entities nearby)
            bool corruptionNearby = false;
            foreach (var wraith in SystemAPI.Query<RefRO<FractalWraith>>())
            {
                corruptionNearby = true;
                break;
            }

            foreach (var (personality, behavior, transform) in
                SystemAPI.Query<RefRW<LirealPersonality>, RefRW<CompanionBehavior>,
                    RefRW<LocalTransform>>()
                .WithAll<CompanionTag>())
            {
                float dist = math.distance(transform.ValueRO.Position, playerPos);

                // Lirael-specific behavior layered on top of base states
                switch (behavior.ValueRO.State)
                {
                    case CompanionState.Follow:
                        // While following, check for special triggers
                        if (playerHealth < 50f && dist < 5f)
                        {
                            // Healing mode: stay close, boost regen
                            float3 healPos = playerPos + new float3(1.5f, 0, 1.5f);
                            float3 dir = math.normalizesafe(healPos - transform.ValueRO.Position);

                            if (math.distance(transform.ValueRO.Position, healPos) > 1f)
                                transform.ValueRW.Position += dir * 4f * dt;
                        }
                        else if (corruptionNearby)
                        {
                            // Scanning mode: face toward nearest corruption
                            behavior.ValueRW.State = CompanionState.React;
                            behavior.ValueRW.StateTimer = 0f;
                        }
                        break;

                    case CompanionState.React:
                        // Lirael reacts to corruption by projecting a blueprint
                        if (behavior.ValueRO.StateTimer > 3f)
                        {
                            behavior.ValueRW.State = CompanionState.Speak;
                            behavior.ValueRW.StateTimer = 0f;
                        }
                        break;

                    case CompanionState.Speak:
                        // After speaking, return to follow
                        if (behavior.ValueRO.StateTimer > 2f)
                        {
                            behavior.ValueRW.State = CompanionState.Follow;
                            behavior.ValueRW.StateTimer = 0f;
                        }
                        break;

                    case CompanionState.Hide:
                        // Lirael doesn't flee — she generates a protective field
                        // Stay near player and boost shield
                        float3 shieldPos = playerPos - new float3(0, 0, 2f);
                        float3 toShield = math.normalizesafe(
                            shieldPos - transform.ValueRO.Position);

                        if (math.distance(transform.ValueRO.Position, shieldPos) > 1.5f)
                            transform.ValueRW.Position += toShield * 3f * dt;
                        break;

                    case CompanionState.Celebrate:
                        // Crystal harmonics celebration
                        if (behavior.ValueRO.StateTimer > behavior.ValueRO.CelebrateTimer)
                        {
                            behavior.ValueRW.State = CompanionState.Follow;
                            behavior.ValueRW.StateTimer = 0f;
                        }
                        break;
                }
            }
        }
    }
}
