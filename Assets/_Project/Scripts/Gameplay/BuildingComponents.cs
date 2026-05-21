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
        Other = 99
    }

    // Core interactable contract used by PlayerInputHandler, buildings, pickups, NPCs
    public interface IInteractable
    {
        void Interact(GameObject player);
        string GetInteractPrompt();
    }

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
        public BuildingRestorationState state;
        public BuildingRestorationState State;
        public float emergenceProgress;
        public int NodesCompleted;
        public int TotalNodes;
        public float RestorationProgress;
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
    }
}