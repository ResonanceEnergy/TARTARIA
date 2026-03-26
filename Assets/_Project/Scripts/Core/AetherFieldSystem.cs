using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tartaria.Core
{
    /// <summary>
    /// Aether Field Simulation System — the heart of Tartaria's visual identity.
    /// Runs as a compute-style ECS system processing the 3D voxel grid.
    /// Grid: 64×64×32 voxels covering the 500m zone radius.
    /// Budget: 2.0ms per frame on recommended GPU.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ResonanceScoreSystem))]
    public partial struct AetherFieldSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AetherFieldConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<AetherFieldConfig>();
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Phase 1: Collect all sources and sinks
            var sources = new NativeList<SourceData>(16, Allocator.TempJob);
            var sinks = new NativeList<SinkData>(16, Allocator.TempJob);

            foreach (var (source, transform) in
                SystemAPI.Query<RefRO<AetherSource>, RefRO<LocalTransform>>())
            {
                sources.Add(new SourceData
                {
                    Position = transform.ValueRO.Position,
                    Strength = source.ValueRO.Strength,
                    Radius = source.ValueRO.Radius,
                    Band = source.ValueRO.Band
                });
            }

            foreach (var (sink, transform) in
                SystemAPI.Query<RefRO<AetherSink>, RefRO<LocalTransform>>())
            {
                sinks.Add(new SinkData
                {
                    Position = transform.ValueRO.Position,
                    Strength = sink.ValueRO.Strength,
                    Radius = sink.ValueRO.Radius
                });
            }

            // Phase 2: Update all Aether nodes based on sources/sinks
            var sourceArray = sources.AsArray();
            var sinkArray = sinks.AsArray();

            new UpdateAetherNodesJob
            {
                DeltaTime = deltaTime,
                Sources = sourceArray,
                Sinks = sinkArray,
                DissipationRate = config.DissipationRate
            }.ScheduleParallel();

            sources.Dispose(state.Dependency);
            sinks.Dispose(state.Dependency);
        }

        struct SourceData
        {
            public float3 Position;
            public float Strength;
            public float Radius;
            public HarmonicBand Band;
        }

        struct SinkData
        {
            public float3 Position;
            public float Strength;
            public float Radius;
        }

        [BurstCompile]
        partial struct UpdateAetherNodesJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public NativeArray<SourceData> Sources;
            [ReadOnly] public NativeArray<SinkData> Sinks;
            [ReadOnly] public float DissipationRate;

            void Execute(ref AetherNode node)
            {
                float totalInfluence = 0f;

                // Accumulate source contributions (inverse-square falloff)
                for (int i = 0; i < Sources.Length; i++)
                {
                    float dist = math.distance(node.WorldPosition, Sources[i].Position);
                    if (dist < Sources[i].Radius)
                    {
                        float falloff = 1.0f - (dist / Sources[i].Radius);
                        falloff *= falloff; // Squared for smoother falloff
                        totalInfluence += Sources[i].Strength * falloff;

                        // Band resonance: same band amplifies coherence
                        if (Sources[i].Band == node.Band)
                        {
                            node.Coherence = math.min(1.0f,
                                node.Coherence + 0.1f * falloff * DeltaTime);
                        }
                    }
                }

                // Apply sink absorption
                for (int i = 0; i < Sinks.Length; i++)
                {
                    float dist = math.distance(node.WorldPosition, Sinks[i].Position);
                    if (dist < Sinks[i].Radius)
                    {
                        float falloff = 1.0f - (dist / Sinks[i].Radius);
                        totalInfluence += Sinks[i].Strength * falloff; // Negative strength
                    }
                }

                // Apply φ-proportioned dissipation
                float phiDissipation = DissipationRate * GoldenRatioValidator.PHI_INVERSE;
                node.Intensity = math.saturate(
                    node.Intensity + (totalInfluence - phiDissipation) * DeltaTime
                );

                // Coherence naturally decays without source reinforcement
                node.Coherence = math.max(0f,
                    node.Coherence - 0.02f * DeltaTime);
            }
        }
    }
}
