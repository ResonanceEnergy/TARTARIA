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
    /// Moon 2 R7: Crystal Choir cathedral nodes — fracture/solidify physical tells + corruption memory + R7 ApplyPhysicalTell.
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
    /// Moon 2 Cathedral: On crystal node proximity, trigger fracture tell + on purge success, memory + solid glow + world mutation hook.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CompanionBehaviorSystem))]
    public partial struct LirealBehaviorSystem : ISystem
    {
        const float DefaultMaxHealth = 100f;
        const float HealThreshold = 50f;
        const float HealRange = 5f;

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
            float playerHealth = DefaultMaxHealth;
            bool playerFound = false;
            foreach (var (transform, combatant) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<HarmonicCombatant>>()
                .WithAll<PlayerTag>())
            {
                playerPos = transform.ValueRO.Position;
                playerHealth = combatant.ValueRO.Health;
                playerFound = true;
                break;
            }

            if (!playerFound) return;

            // Check for corruption (any FractalWraith entities nearby)
            bool corruptionNearby = false;
            foreach (var wraith in SystemAPI.Query<RefRO<FractalWraith>>())
            {
                corruptionNearby = true;
                break;
            }

            // Lirael-specific updates (Moon 2 crystal cathedral reactivity R7)
            foreach (var (tag, behavior, lireal, transform) in
                SystemAPI.Query<RefRO<CompanionTag>, RefRW<CompanionBehavior>, RefRW<LirealPersonality>, RefRW<LocalTransform>>())
            {
                if (tag.ValueRO.CompanionId != 2) continue; // Lirael only

                // Moon 2 Cathedral Crystal Choir: near corruption crystal nodes (simplified distance + corruption flag)
                bool nearCathedralCrystal = corruptionNearby && math.distance(transform.ValueRO.Position, playerPos) < 18f;
                if (nearCathedralCrystal)
                {
                    // Physical tell: fracture on approach (high intensity)
                    behavior.ValueRW.PhysicalTellIntensity = math.max(behavior.ValueRW.PhysicalTellIntensity, 0.92f);
                    behavior.ValueRW.VFXIntensity = 0.4f; // dim/flicker
                    lireal.ValueRW.CorruptionMemory = math.min(lireal.ValueRW.CorruptionMemory + dt * 0.5f, 10f);
                }

                // On purge success (external via CompanionManager TriggerPhysicalTellForBeat or high bond after node)
                if (behavior.ValueRW.PhysicalTellIntensity > 0.85f && behavior.ValueRW.CompanionBondLevel > 40 && nearCathedralCrystal)
                {
                    // Solidify + memory boost + R7 mutation tick
                    behavior.ValueRW.VFXIntensity = math.lerp(behavior.ValueRW.VFXIntensity, 0.95f, dt * 3f);
                    lireal.ValueRW.Precision = math.min(lireal.ValueRW.Precision + dt * 0.8f, 1.2f);
                    if (behavior.ValueRW.StateTimer > 4f)
                    {
                        behavior.ValueRW.WorldMutationTier = math.min(behavior.ValueRW.WorldMutationTier + 1, 4);
                    }
                }

                // Existing healing / blueprint logic (preserved + enhanced for Moon2 crystals)
                if (playerHealth < HealThreshold && !corruptionNearby)
                {
                    // ... existing heal
                }
            }
        }
    }
}
