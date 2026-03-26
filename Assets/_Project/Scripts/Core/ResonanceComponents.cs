using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.Core
{
    // ─────────────────────────────────────────────
    //  Resonance Score — the game's core metric
    // ─────────────────────────────────────────────
    public struct ResonanceScore : IComponentData
    {
        public float CurrentRS;          // 0–100 per zone
        public float GlobalRS;           // Cumulative across all zones
        public float HighestZoneRS;      // Best single-zone score
        public int ThresholdReached;     // 0, 25, 50, 75, 100
    }

    // ─────────────────────────────────────────────
    //  RS Event — queued when RS changes
    // ─────────────────────────────────────────────
    public struct ResonanceEvent : IBufferElementData
    {
        public ResonanceAction Action;
        public float BaseReward;
        public float Multiplier;
        public float GoldenRatioMatch;   // 0–1
    }

    public enum ResonanceAction : byte
    {
        DiscoverStructure  = 0,  // +5 base
        TuneNodeBasic      = 1,  // +10 base
        TuneNodePerfect    = 2,  // +25 base
        RestoreBuilding    = 3,  // +50 base
        DefeatEnemy        = 4,  // +15 base
        CollectAether      = 5,  // +1–3 base
    }

    // ─────────────────────────────────────────────
    //  RS Threshold Events
    // ─────────────────────────────────────────────
    public enum RSThreshold : byte
    {
        Start       = 0,   // Desaturated, grey-brown palette
        FirstGolem  = 25,  // Faint golden hue at edges
        AetherWake  = 50,  // Golden mist, music layer 2
        ZoneShift   = 75,  // Full color, Aether flowing, Celestial band
        ZoneComplete = 100  // Aurora wisps, harmonic hum, all buildings active
    }

    public struct RSThresholdCrossed : IComponentData
    {
        public RSThreshold Threshold;
        public bool Handled;
    }

    // ─────────────────────────────────────────────
    //  RS Reward Constants (from MVP spec)
    // ─────────────────────────────────────────────
    public static class ResonanceConstants
    {
        // Base rewards
        public const float DISCOVER_STRUCTURE = 5f;
        public const float TUNE_NODE_BASIC = 10f;
        public const float TUNE_NODE_PERFECT = 25f;
        public const float RESTORE_BUILDING = 50f;
        public const float DEFEAT_ENEMY = 15f;
        public const float COLLECT_AETHER_MIN = 1f;
        public const float COLLECT_AETHER_MAX = 3f;

        // Multipliers
        public const float GOLDEN_RATIO_MULTIPLIER = GoldenRatioValidator.PHI;  // 1.618
        public const float FREQUENCY_432_MULTIPLIER = 1.5f;
        public const float PERFECT_NODES_MULTIPLIER = 2.0f;
        public const float HARMONICS_ONLY_MULTIPLIER = 1.3f;
        public const float CONSECUTIVE_COLLECT_MULTIPLIER = 1.1f;
    }

    // ─────────────────────────────────────────────
    //  Player Tag — identifies the player entity
    //  Used by DiscoverySystem, CompanionBehaviorSystem,
    //  EnemyAISystem, and GameBootstrap.
    // ─────────────────────────────────────────────
    public struct PlayerTag : IComponentData { }
}
