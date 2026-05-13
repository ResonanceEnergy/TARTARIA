using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Knockback System — applies knockback impulse to entities, decays magnitude
    /// exponentially over time. When magnitude drops below threshold, removes component.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HarmonicCombatSystem))]
    [BurstCompile]
    public partial struct KnockbackSystem : ISystem
    {
        const float RemovalThreshold = 0.1f; // m/s, below this we remove the component

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (knockback, transform, entity) in
                SystemAPI.Query<RefRW<KnockbackImpulse>, RefRW<LocalTransform>>()
                    .WithEntityAccess())
            {
                // Apply translation
                float3 displacement = knockback.ValueRO.Direction * knockback.ValueRO.Magnitude * dt;
                transform.ValueRW.Position += displacement;

                // Exponential decay
                knockback.ValueRW.Magnitude *= math.exp(-knockback.ValueRO.DecayRate * dt);

                // Remove if below threshold
                if (knockback.ValueRO.Magnitude < RemovalThreshold)
                {
                    state.EntityManager.RemoveComponent<KnockbackImpulse>(entity);
                }
            }

            // Decrement hitstun timers
            foreach (var (hitstun, entity) in
                SystemAPI.Query<RefRW<HitStunTimer>>()
                    .WithEntityAccess())
            {
                hitstun.ValueRW.Remaining -= dt;
                if (hitstun.ValueRO.Remaining <= 0f)
                {
                    state.EntityManager.RemoveComponent<HitStunTimer>(entity);
                }
            }
        }
    }
}
