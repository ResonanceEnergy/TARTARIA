using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.Gameplay
{
    // ─────────────────────────────────────────────
    //  Building State Machine
    //  BURIED → REVEALED → TUNING → EMERGING → ACTIVE
    // ─────────────────────────────────────────────
    public enum BuildingRestorationState : byte
    {
        Buried   = 0,
        Revealed = 1,
        Tuning   = 2,
        Emerging = 3,
        Active   = 4
    }

    public enum BuildingArchetype : byte
    {
        Dome       = 0,
        Fountain   = 1,
        Spire      = 2,
        StarFort   = 3,
        Cathedral  = 4,
        Tower      = 5
    }

    // ─────────────────────────────────────────────
    //  ECS Component — Tartarian Building
    // ─────────────────────────────────────────────
    public struct TartarianBuilding : IComponentData
    {
        public BuildingArchetype Archetype;
        public BuildingRestorationState State;
        public float RestorationProgress;    // 0–1
        public float ResonanceScore;         // 0–100
        public float GoldenRatioMatch;       // 0–1 (proportion accuracy)
        public int UpgradeTier;              // 0–5
        public int NodesCompleted;           // 0–3
        public int TotalNodes;               // 3 for Phase 1
    }

    // ─────────────────────────────────────────────
    //  Tuning Node — 3 per building
    // ─────────────────────────────────────────────
    public struct TuningNode : IComponentData
    {
        public Entity ParentBuilding;
        public int NodeIndex;               // 0, 1, 2
        public TuningVariant Variant;
        public bool IsComplete;
        public float Accuracy;              // 0–1 (result of mini-game)
        public float TargetFrequency;       // 432 Hz default
    }

    public enum TuningVariant : byte
    {
        FrequencySlider = 0,   // Variant A: horizontal slider
        WaveformTrace   = 1,   // Variant B: trace golden waveform
        HarmonicPattern = 2,   // Variant C: rhythm tap pattern
        BellTower       = 3,   // Variant D: rhythmic bell ringing, scalar wave shield
        FrequencyDial   = 4,   // Variant E: rotary dial tuning
        WaveformMatch   = 5    // Variant F: match target waveform
    }

    // ─────────────────────────────────────────────
    //  Aether Generator — active buildings produce aether
    // ─────────────────────────────────────────────
    public struct AetherGenerator : IComponentData
    {
        public float OutputRate;            // Aether per second
        public float OutputRadius;          // meters
        public Core.HarmonicBand OutputBand;
    }

    // ─────────────────────────────────────────────
    //  Mud Dissolution — shader control data
    // ─────────────────────────────────────────────
    public struct MudDissolution : IComponentData
    {
        public float Progress;              // 0–1 (drives _DissolveProgress)
        public float Speed;                 // Duration: 5 seconds default
    }

    // ─────────────────────────────────────────────
    //  Discovery Trigger — for Points of Interest
    // ─────────────────────────────────────────────
    public struct DiscoveryTrigger : IComponentData
    {
        public float TriggerRadius;         // 30m for buildings, varies for POIs
        public float RSReward;              // +5 default
        public bool Discovered;
    }
}
