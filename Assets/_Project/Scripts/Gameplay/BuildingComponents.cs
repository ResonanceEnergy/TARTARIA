using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
        Dome = 0,
        Fountain = 1,
        Spire = 2,
        Tower = 10,
        Gate = 11,
        StarFort = 12,
        Amphitheatre = 13,
        Archive = 14,
        Cathedral = 15,
        Forge = 16,
        Observatory = 17,
        Obelisk = 18,
        Unique = 50,
        Other = 99
    }

    // Core interactable contract moved to Tartaria.Input.IInteractable (single source of truth).
    // See Assets/_Project/Scripts/Input/IInteractable.cs.

    /// <summary>
    /// Tuning variant types for mini-games (Echohaven + Moon variants).
    /// </summary>
    public enum TuningVariant
    {
        FrequencyDial = 0,
        WaveformMatch = 1,
        FrequencySlider = 2,
        WaveformTrace = 3,
        HarmonicPattern = 4,
        BellTower = 5,
        WaveformTraceAlt = 6
    }

    // ECS component stubs for BuildingSystem / generated code (Moon 1/2 compatibility)
    public struct TartarianBuilding : IComponentData
    {
        public int buildingId;
        public BuildingArchetype Archetype;
        public BuildingRestorationState state;
        public BuildingRestorationState State;
        public float emergenceProgress;
        public int NodesCompleted;
        public int TotalNodes;
        public float RestorationProgress;
        public float ResonanceScore;
        public float GoldenRatioMatch;
        public int UpgradeTier;
    }

    public struct MudDissolution : IComponentData
    {
        public float progress;
        public float Progress;
        public float duration;
        public float Speed;
    }

    public struct DiscoveryTrigger : IComponentData
    {
        public int triggerId;
        public bool Discovered;
        public float TriggerRadius;
        public float RSReward;
    }
}