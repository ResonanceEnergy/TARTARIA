using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.AI
{
    // ─────────────────────────────────────────────
    //  Companion States (DOTS State Machine)
    //  FOLLOW → IDLE → REACT → SPEAK → HIDE → CELEBRATE → ESCORT (physical train positioning) → PHYSICAL_BOND (Anastasia/Cassian solidification interplay, Moon 5+ redemption)
    //  Round 5: Wired full DOTS Cassian spawning consumption + animator/VFX flags + escort for Milo/Lirael/Korath + Cassian
    // ─────────────────────────────────────────────
    public enum CompanionState : byte
    {
        Follow          = 0,  // Stay within 3m of player, match pace
        Idle            = 1,  // Ambient animations (sniff, stretch, look around)
        React           = 2,  // Face point of interest, vocalise
        Speak           = 3,  // Deliver dialogue line
        Hide            = 4,  // During combat, find cover within 10m
        Celebrate       = 5,  // Post-restoration, jump/spin animation
        Escort          = 6,  // Physical train escort positioning (Round 4 physical + DOTS wired Round 5)
        PhysicalBond    = 7   // Anastasia solidification callbacks + Cassian redemption bond interplay (Moon 5+)
    }

    public struct CompanionTag : IComponentData
    {
        public int CompanionId;  // 0 = Milo, 1 = Cassian (DOTS full spawn Round 4), 2 = Lirael, 3 = Korath, 4 = Thorne, 5 = Anastasia
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
        public float DialogueDuration;        // Set by managed code from DialogueManager
        public float3 TargetPosition;
        public float WalkSpeed;               // 3 m/s default
        public float SprintSpeed;             // 5 m/s default
        public float SprintDistanceThreshold; // 8m — sprint if further than this
        public float MaxIdleTime;             // 10s — return to follow after this

        // Round 5: Physical/DOTS escort & bond wiring
        public bool IsEscorting;              // Train escort active (consumes physical positioning from Round 4 controllers)
        public float3 EscortTarget;           // Physical position on train for DOTS sync (animator/VFX consumption)
        public float EscortSpeed;             // Specific for train motion
        public int RedemptionLevel;           // Cassian: 0 neutral, higher = redeemed ally bond (Moon 5+ branches)
        public bool SolidificationActive;     // Anastasia: triggers solidification callback into DOTS PhysicalBond state
        public float VFXIntensity;            // For animator/VFX bridge (0-1, consumed by hybrid renderers)
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

    /// <summary>
    /// Cassian DOTS personality (Round 4 full spawning + Round 5 redemption/bond wiring).
    /// Drives ambiguous ally → redemption path interplay with Anastasia/Cassian bond.
    /// </summary>
    public struct CassianPersonality : IComponentData
    {
        public float Deception;     // 0-1, lowers with player trust / wonder shown
        public float Redemption;    // 0-1, rises on Moon 5+ Cassian/Redemption branches; high = ally escort
        public float IntelAccuracy; // Dynamic based on bond with Anastasia solidification callbacks
        public float BondStrength;  // With Anastasia (deep Milo/Lirael/Korath + Anastasia/Cassian interplay)
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
