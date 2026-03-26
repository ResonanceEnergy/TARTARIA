using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.AI
{
    // ─────────────────────────────────────────────
    //  Companion States (DOTS State Machine)
    //  FOLLOW → IDLE → REACT → SPEAK → HIDE → CELEBRATE
    // ─────────────────────────────────────────────
    public enum CompanionState : byte
    {
        Follow    = 0,  // Stay within 3m of player, match pace
        Idle      = 1,  // Ambient animations (sniff, stretch, look around)
        React     = 2,  // Face point of interest, vocalise
        Speak     = 3,  // Deliver dialogue line
        Hide      = 4,  // During combat, find cover within 10m
        Celebrate = 5   // Post-restoration, jump/spin animation
    }

    public struct CompanionTag : IComponentData
    {
        public int CompanionId;  // 0 = Milo (Phase 1)
    }

    public struct CompanionBehavior : IComponentData
    {
        public CompanionState State;
        public CompanionState PreviousState;
        public float StateTimer;              // Time in current state
        public float FollowDistance;          // 3m default
        public float IdleThreshold;           // 5s of player stationary
        public float ReactRadius;             // 20m for POI detection
        public float HideRadius;              // 10m for finding cover
        public float CelebrateTimer;          // 3s celebration
        public float3 TargetPosition;
    }

    /// <summary>
    /// Milo-specific personality data.
    /// </summary>
    public struct MiloPersonality : IComponentData
    {
        public float Curiosity;    // Affects react frequency
        public float Encouragement;// Affects combat dialogue
        public float Sarcasm;      // Idle chatter personality
    }

    // ─────────────────────────────────────────────
    //  Dialogue Trigger
    // ─────────────────────────────────────────────
    public struct DialogueTrigger : IComponentData
    {
        public int TriggerHash;          // Hashed trigger ID
        public float TriggerRadius;      // Distance to activate
        public int Priority;             // Higher = more important
        public bool PlayOnce;
        public bool HasPlayed;
    }

    // ─────────────────────────────────────────────
    //  Enemy AI States
    //  NOTE: EnemyAIState + EnemyAI moved to
    //  Tartaria.Gameplay.CombatComponents to avoid
    //  circular assembly dependency.
    // ─────────────────────────────────────────────
}
