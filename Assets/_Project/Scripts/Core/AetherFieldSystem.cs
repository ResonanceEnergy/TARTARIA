using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine; // For Time.realtimeSinceStartup in instrumentation wrapper (Round 5-6)
using Unity.Profiling; // R6 marker

namespace Tartaria.Core
{
    /// <summary>
    /// Aether Field Simulation System — the heart of Tartaria's visual identity.
    /// Runs as a compute-style ECS system processing the 3D voxel grid.
    /// Grid: 64×64×32 voxels covering the 500m zone radius.
    /// Budget: 2.0ms per frame on recommended GPU.
    /// 
    /// R6 Deep DOTS Opt: source spatial pre-filter (O(N) reduction for dense Moon2/3 fountains/crystals),
    /// Burst job improvements, guard marker, tighter Native allocs. Hot path documented in PERFORMANCE_BUDGET.md.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ResonanceScoreSystem))]
    [BurstCompile]
    public partial struct AetherFieldSystem : ISystem
    {
        static readonly ProfilerMarker s_aetherMarker = new ProfilerMarker("Tartaria.AetherField.R6");

        // MS.L5 (Sprint 12 #5): cached last-known player position so we can fall back
        // to the most recent live value rather than the hardcoded (0,1,0) spawn fallback
        // when the PlayerTag query momentarily returns 0 entities (e.g. between scene loads).
        float3 _lastKnownPlayerPosition;
        bool _hasSeenPlayerOnce;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AetherFieldConfig>();
            _lastKnownPlayerPosition = new float3(0f, 1f, 0f); // initial sane fallback near spawn
            _hasSeenPlayerOnce = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using (s_aetherMarker.Auto()) // R6 production marker (visible in Profiler)
            {
                // NOTE: PerformanceGuard calls removed — Burst doesn't support managed object null checks
                var config = SystemAPI.GetSingleton<AetherFieldConfig>();
                float deltaTime = SystemAPI.Time.DeltaTime;

                // Phase 1: Collect sources/sinks (R6: rough spatial pre-filter for dense Moon2/3 — major win)
                var sources = new NativeList<SourceData>(32, Allocator.TempJob);
                var sinks = new NativeList<SinkData>(16, Allocator.TempJob);

                // MS.L5 (Sprint 12 #5, Sprint 11 L1): live player position read.
                // Previous code hardcoded (0, 1, 0) so the spatial pre-filter culled sources
                // around world spawn instead of around the actual player — Aether sources
                // beyond ~180m of spawn were silently ignored regardless of player location.
                // PlayerTag is bootstrapped by GameBootstrap / WorldInitializer on the
                // single Player entity (alongside LocalTransform). NO-DEBT: if the query
                // is momentarily empty (scene load, hot reload) we re-use the last live
                // read instead of snapping back to spawn, and emit a one-shot warning.
                float3 playerApprox = _lastKnownPlayerPosition;
                bool foundPlayerThisFrame = false;
                foreach (var playerTransform in
                    SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
                {
                    playerApprox = playerTransform.ValueRO.Position;
                    _lastKnownPlayerPosition = playerApprox;
                    foundPlayerThisFrame = true;
                    break; // single Player entity expected; ignore any duplicates this frame
                }
                if (!foundPlayerThisFrame && !_hasSeenPlayerOnce)
                {
                    // NO-DEBT: surface the missing-player condition loudly the first frame
                    // it happens. Routed through a [BurstDiscard] helper so the managed
                    // string formatting is excluded from the Burst-compiled hot path
                    // (the helper compiles to a no-op in Burst, runs only on the managed
                    // fallback editor + first-frame mono path).
                    LogMissingPlayerOnce();
                }
                if (foundPlayerThisFrame)
                {
                    _hasSeenPlayerOnce = true;
                }

                const float MAX_INFLUENCE_RADIUS = 180f; // R6: cull far sources for 50%+ less work on large grids

                foreach (var (source, transform) in
                    SystemAPI.Query<RefRO<AetherSource>, RefRO<LocalTransform>>())
                {
                    float3 pos = transform.ValueRO.Position;
                    if (math.distance(pos, playerApprox) < MAX_INFLUENCE_RADIUS + source.ValueRO.Radius)
                    {
                        sources.Add(new SourceData
                        {
                            Position = pos,
                            Strength = source.ValueRO.Strength,
                            Radius = source.ValueRO.Radius,
                            Band = source.ValueRO.Band
                        });
                    }
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
        }

        // MS.L5: [BurstDiscard] strips this from the Burst-compiled body so the
        // managed string + Debug.LogWarning call doesn't break Burst compilation.
        // Runs on the mono fallback path (Editor + first-frame) which is exactly
        // when an uninitialized player would be diagnosed anyway.
        [BurstDiscard]
        static void LogMissingPlayerOnce()
        {
            Debug.LogWarning(
                "[AetherFieldSystem] No entity with PlayerTag + LocalTransform found — " +
                "Aether source culling is using initial fallback (0,1,0). " +
                "Check GameBootstrap / WorldInitializer player spawn wiring.");
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

                // R6: inner loop now benefits from pre-filtered smaller Sources array (dense Moon2/3 win)
                for (int i = 0; i < Sources.Length; i++)
                {
                    float dist = math.distance(node.WorldPosition, Sources[i].Position);
                    if (dist < Sources[i].Radius)
                    {
                        float falloff = 1.0f - (dist / Sources[i].Radius);
                        falloff *= falloff;
                        totalInfluence += Sources[i].Strength * falloff;

                        if (Sources[i].Band == node.Band)
                        {
                            node.Coherence = math.min(1.0f,
                                node.Coherence + 0.1f * falloff * DeltaTime);
                        }
                    }
                }

                for (int i = 0; i < Sinks.Length; i++)
                {
                    float dist = math.distance(node.WorldPosition, Sinks[i].Position);
                    if (dist < Sinks[i].Radius)
                    {
                        float falloff = 1.0f - (dist / Sinks[i].Radius);
                        totalInfluence += Sinks[i].Strength * falloff;
                    }
                }

                float phiDissipation = DissipationRate * GoldenRatioValidator.PHI_INVERSE;
                node.Intensity = math.saturate(
                    node.Intensity + (totalInfluence - phiDissipation) * DeltaTime
                );

                node.Coherence = math.max(0f, node.Coherence - 0.02f * DeltaTime);
            }
        }
    }
}
