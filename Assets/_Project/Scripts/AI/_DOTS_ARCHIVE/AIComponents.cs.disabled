using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.AI
{
    // ─────────────────────────────────────────────
    //  Companion States (DOTS State Machine)
    //  FOLLOW → IDLE → REACT → SPEAK → HIDE → CELEBRATE → ESCORT (physical train positioning) → PHYSICAL_BOND (Anastasia/Cassian solidification interplay, Moon 5+ redemption)
    //  Round 5: Wired full DOTS Cassian spawning consumption + animator/VFX flags + escort for Milo/Lirael/Korath + Cassian
    //  Round 6: Production hybrid DOTS-Mono full sync bridge + save persistence for all new states + Korath/Thorne deeper escort + 17th Hour integration + VO prep + redemption/solidif playtest paths
    //  Round 7: Full 7-companion (Veritas ID=6) + trust arc world mutations + deep physical tells for all major beats (restoration/combat/giant/17th/World's Fair/escort) + calendar/live-ops + real VO playback + giant synergies (Companion Giant, Giant's Song) + cross-Moon memory
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
        public int CompanionId;  // 0 = Milo, 1 = Cassian, 2 = Lirael, 3 = Korath, 4 = Thorne, 5 = Anastasia, 6 = Veritas (R7 full 7-companion)
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

        // Round 6: Production DOTS-Mono hybrid sync bridge, persistence fields, deeper Korath/Thorne escort + 17th Hour
        public int CompanionBondLevel;        // 0-100 trust mirrored from Mono for DOTS behaviors (all companions)
        public bool In17thHourMode;           // Triggers special 17th Hour escort positioning, VFX density, dialogue density hooks
        public bool RedemptionChoiceMade;     // Cassian player redemption choice flag (unlocks PhysicalBond ally path)
        public float EscortLeanAngle;         // Per-companion physical lean for train (Korath star-gaze, Thorne vigilant)

        // Round 7: Deep physical tells + giant synergy + world mutation flags + calendar echoes
        public bool GiantSynergyActive;       // High-trust Companion Giant assist / Giant's Song auto-match active
        public float GiantSongMatchQuality;   // 0-1 freq match quality from bond (auto-matches player harmonic during giant)
        public int WorldMutationTier;         // Permanent world mutation level (trust arc payoffs: 0-4) persisted cross-Moon
        public bool CalendarEchoActive;       // 17th Hour / daily live-ops echo changed this companion's state (trust bump or tell)
        public float PhysicalTellIntensity;   // Per-beat reactivity (restoration 0.9, combat 0.6, escort 0.8, giant 1.0)
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

    /// <summary>
    /// Veritas (R7) bell/organ keeper personality — precision, truth through resonance.
    /// </summary>
    public struct VeritasPersonality : IComponentData
    {
        public float Precision;     // High = exact freq matches in giant song / bell sync
        public float TruthWeight;   // Influences shared history reveals and calendar echoes
        public float ResonanceEcho; // 17th Hour + giant synergy multiplier
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