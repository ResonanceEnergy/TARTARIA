using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.Core
{
    /// <summary>
    /// Resonance Score System — processes RS events, validates golden ratio,
    /// applies multipliers, and triggers threshold events.
    ///
    /// RS Thresholds (Vertical Slice):
    ///   0  → Start (desaturated, grey-brown)
    ///   25 → First Golem spawns (faint golden hue)
    ///   50 → Aether visible (golden mist, music layer 2)
    ///   75 → Zone shift (full color, Celestial band)
    ///   100 → Zone complete (aurora wisps, all buildings active)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AetherFieldSystem))]
    public partial struct ResonanceScoreSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Create the singleton RS entity
            var rsEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(rsEntity, new ResonanceScore
            {
                CurrentRS = 0f,
                GlobalRS = 0f,
                HighestZoneRS = 0f,
                ThresholdReached = 0,
                SkillRSMultiplier = 1f,
                MoonRSMultiplier = 1f
            });
            state.EntityManager.AddBuffer<ResonanceEvent>(rsEntity);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Process all queued RS events
            foreach (var (score, events) in
                SystemAPI.Query<RefRW<ResonanceScore>, DynamicBuffer<ResonanceEvent>>())
            {
                for (int i = 0; i < events.Length; i++)
                {
                    var evt = events[i];

                    // Apply golden ratio multiplier
                    float goldenMultiplier = GoldenRatioValidator.GetMultiplier(
                        evt.GoldenRatioMatch > 0f
                            ? GoldenRatioValidator.PHI * evt.GoldenRatioMatch
                            : 1.0f);

                    float finalReward = evt.BaseReward * evt.Multiplier * goldenMultiplier
                        * score.ValueRO.SkillRSMultiplier
                        * score.ValueRO.MoonRSMultiplier;
                    score.ValueRW.CurrentRS = math.min(100f,
                        score.ValueRO.CurrentRS + finalReward);
                    score.ValueRW.GlobalRS += finalReward;

                    if (score.ValueRO.CurrentRS > score.ValueRO.HighestZoneRS)
                        score.ValueRW.HighestZoneRS = score.ValueRO.CurrentRS;
                }

                events.Clear();

                // Check threshold crossings
                int currentThreshold = score.ValueRO.ThresholdReached;
                float rs = score.ValueRO.CurrentRS;

                if (rs >= 100f && currentThreshold < 100)
                    score.ValueRW.ThresholdReached = 100;
                else if (rs >= 75f && currentThreshold < 75)
                    score.ValueRW.ThresholdReached = 75;
                else if (rs >= 50f && currentThreshold < 50)
                    score.ValueRW.ThresholdReached = 50;
                else if (rs >= 25f && currentThreshold < 25)
                    score.ValueRW.ThresholdReached = 25;
            }
        }
    }

    /// <summary>
    /// Helper to queue RS events from gameplay code.
    /// </summary>
    public static class ResonanceEventHelper
    {
        public static void QueueDiscovery(EntityManager em, Entity rsEntity,
            float goldenRatioMatch = 0f)
        {
            var buffer = em.GetBuffer<ResonanceEvent>(rsEntity);
            buffer.Add(new ResonanceEvent
            {
                Action = ResonanceAction.DiscoverStructure,
                BaseReward = ResonanceConstants.DISCOVER_STRUCTURE,
                Multiplier = 1.0f,
                GoldenRatioMatch = goldenRatioMatch
            });
        }

        public static void QueueTuneNode(EntityManager em, Entity rsEntity,
            float accuracy, float goldenRatioMatch = 0f)
        {
            bool isPerfect = accuracy >= 0.95f;
            var buffer = em.GetBuffer<ResonanceEvent>(rsEntity);
            buffer.Add(new ResonanceEvent
            {
                Action = isPerfect
                    ? ResonanceAction.TuneNodePerfect
                    : ResonanceAction.TuneNodeBasic,
                BaseReward = isPerfect
                    ? ResonanceConstants.TUNE_NODE_PERFECT
                    : ResonanceConstants.TUNE_NODE_BASIC,
                Multiplier = isPerfect
                    ? ResonanceConstants.GOLDEN_RATIO_MULTIPLIER
                    : GetAccuracyMultiplier(accuracy),
                GoldenRatioMatch = goldenRatioMatch
            });
        }

        public static void QueueBuildingRestored(EntityManager em, Entity rsEntity,
            bool allNodesPerfect, float goldenRatioMatch = 0f)
        {
            var buffer = em.GetBuffer<ResonanceEvent>(rsEntity);
            buffer.Add(new ResonanceEvent
            {
                Action = ResonanceAction.RestoreBuilding,
                BaseReward = ResonanceConstants.RESTORE_BUILDING,
                Multiplier = allNodesPerfect
                    ? ResonanceConstants.PERFECT_NODES_MULTIPLIER
                    : 1.0f,
                GoldenRatioMatch = goldenRatioMatch
            });
        }

        public static void QueueEnemyDefeated(EntityManager em, Entity rsEntity,
            bool harmonicsOnly)
        {
            var buffer = em.GetBuffer<ResonanceEvent>(rsEntity);
            buffer.Add(new ResonanceEvent
            {
                Action = ResonanceAction.DefeatEnemy,
                BaseReward = ResonanceConstants.DEFEAT_ENEMY,
                Multiplier = harmonicsOnly
                    ? ResonanceConstants.HARMONICS_ONLY_MULTIPLIER
                    : 1.0f,
                GoldenRatioMatch = 0f
            });
        }

        static float GetAccuracyMultiplier(float accuracy)
        {
            // >95% = Perfect (handled above), 80-95% = 1.3, 60-80% = 1.0
            if (accuracy >= 0.80f) return 1.3f;
            return 1.0f;
        }
    }
}
