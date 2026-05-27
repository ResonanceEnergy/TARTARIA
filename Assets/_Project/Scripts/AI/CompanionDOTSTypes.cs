using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.AI
{
    /// <summary>
    /// DEPRECATED: DOTS-era companion behavior component.
    /// Kept for compilation compatibility with WorldInitializer and CompanionManager (hybrid bridge code).
    /// New companion system uses MonoBehaviour controllers (MiloController, LiraelController, etc.).
    /// </summary>
    public struct CompanionTag : IComponentData
    {
        public int CompanionId;
    }

    /// <summary>
    /// DEPRECATED: DOTS-era companion behavior state.
    /// Kept for compilation compatibility with WorldInitializer and CompanionManager (hybrid bridge code).
    /// New companion system uses MonoBehaviour controllers.
    /// </summary>
    public struct CompanionBehavior : IComponentData
    {
        public CompanionState State;
        public CompanionState PreviousState;
        public float StateTimer;
        public float FollowDistance;
        public float IdleThreshold;
        public float ReactRadius;
        public float HideRadius;
        public float CelebrateTimer;
        public float3 TargetPosition;
        public float WalkSpeed;
        public float SprintSpeed;
        public float SprintDistanceThreshold;
        public float MaxIdleTime;
        public float EscortSpeed;
        public float VFXIntensity;
        public int WorldMutationTier;
        public int CompanionBondLevel;
        public float PhysicalTellIntensity;
        public float EscortLeanAngle;
        public bool IsEscorting;
        public float3 EscortTarget;
        public bool SolidificationActive;
        public int RedemptionLevel;
        public bool RedemptionChoiceMade;
        public bool In17thHourMode;
        public bool GiantSynergyActive;
        public bool CalendarEchoActive;
    }

    /// <summary>
    /// DEPRECATED: DOTS-era Milo personality traits.
    /// Kept for compilation compatibility with WorldInitializer.
    /// New companion system uses personality data in MiloController.
    /// </summary>
    public struct MiloPersonality : IComponentData
    {
        public float Curiosity;
        public float Encouragement;
        public float Sarcasm;
    }
}
