using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.Gameplay
{
    // ─────────────────────────────────────────────
    //  Combat Components (DOTS/ECS)
    //  Combat is harmonic, not violent — retune, don't destroy
    // ─────────────────────────────────────────────

    public struct HarmonicCombatant : IComponentData
    {
        public float Health;
        public float MaxHealth;
        public float AetherCharge;
        public float MaxAetherCharge;
        public float CurrentFrequency;    // Hz
        public int ComboCount;            // 0–12 (Golden Cascade max)
        public bool IsGiantMode;
    }

    // ─── Player Combat State ─────────────────────

    public struct PlayerCombatState : IComponentData
    {
        public float ResonancePulseCooldown;    // 2s
        public float HarmonicStrikeCooldown;    // 3s
        public float FrequencyShieldCooldown;   // 8s
        public float ShieldActiveTime;          // 3s duration
        public bool IsShielding;
    }

    // ─── Enemy Components ────────────────────────

    public enum EnemyType : byte
    {
        MudGolem            = 0,  // Moon 1-6: slow melee lumberer, shatter into Aether shards
        FractalWraith       = 1,  // Moon 2-12: phases through matter, drains Aether
        MirrorWraith        = 2,  // Moon 2-12 elite: copies player's last 3 attacks
        RailWraith          = 3,  // Moon 3: corrupted train guardian on dissonant rail
        DissonanceHarvester = 4,  // Moon 3-6: drains spectral children during escort
        DissonanceLeviathan = 5,  // Moon 3 boss: serpentine, attacks orphan train
        SiegeGolem          = 6,  // Moon 4,7: massive armored charger, breaches walls
        HarmonicParasite    = 7,  // Moon 6: feeds on musical energy, wrong notes spawn it
        DissonantConductor  = 8,  // Moon 6 elite: coordinates parasites into formations
        CorruptedCraft      = 9,  // Moon 8: anti-Aether drone swarms
        SkyReaver           = 10, // Moon 8: airborne, phases through solids, vulnerable during attacks
        ProphecyGuardian    = 11, // Moon 9: ancient automated prophecy stone defender
        ResetSeeker         = 12, // Moon 9: human Reset agents, fast + dissonance grenades
        TemporalWraith      = 13, // Moon 9-13: attacks from outside time, erratic phasing
        LivingSludge        = 14, // Moon 11: sentient corruption, regenerates unless purified at source
        SludgeLeviathan     = 15, // Moon 11 boss: massive corruption entity in deepest aquifer
        TitanGolem          = 16, // Moon 7,12: 20ft giant requiring Giant Mode
        FrequencyWraith     = 17  // Moon 7-11: shifts frequency every 5s mid-combat
    }

    public struct EnemyTag : IComponentData
    {
        public EnemyType Type;
    }

    public struct MudGolem : IComponentData
    {
        public float AttackWindup;           // 1s telegraph
        public float AttackDamage;           // 20
        public float StunDuration;           // 2s after 3 consecutive Resonance Pulses
        public int ConsecutivePulseHits;     // Tracks stun condition
        public float StunTimer;
        public float PatrolRadius;           // 20m
    }

    // ─── Damage Events ───────────────────────────

    /// <summary>
    /// Fractal Wraith -- Moon 2 enemy. Phases through matter, drains Aether
    /// from buildings it touches. Must be hit with Resonance Pulse during
    /// its 1.5s materialise window.
    /// </summary>
    public struct FractalWraith : IComponentData
    {
        public float PhaseCycleDuration;       // 4s total (2.5 phased + 1.5 material)
        public float PhaseTimer;
        public bool IsPhased;                  // True = intangible, draining Aether
        public float AetherDrainPerSecond;     // Drains from nearest building
        public float MaterialiseWindow;        // 1.5s window when hittable
        public float AttackDamage;             // 12
        public float MoveSpeed;               // 0.8x player speed
        public float3 TargetBuildingPos;       // Current building being drained
    }

    /// <summary>
    /// Mirror Wraith -- Moon 2 elite. Copies the player's last 3 attacks
    /// and reflects them back. Must be defeated with an attack type NOT
    /// in its mirror buffer.
    /// </summary>
    public struct MirrorWraith : IComponentData
    {
        public DamageType MirrorSlot0;         // Last attack received
        public DamageType MirrorSlot1;         // Second-to-last
        public DamageType MirrorSlot2;         // Third-to-last
        public int MirrorCount;                // How many slots filled (0-3)
        public float ReflectDamageMultiplier;  // 0.75x of original
        public float ReflectCooldown;          // 2s between reflections
        public float ReflectTimer;
        public float AttackDamage;             // 18
        public float MoveSpeed;               // 0.7x player speed
        public float TeleportCooldown;         // Blinks behind player
        public float TeleportTimer;
    }

    // ─── Moon 3+ Enemy Components ────────────────

    public struct RailWraith : IComponentData
    {
        public float RailSpeed;                // Movement speed along rail segments
        public float AttackDamage;             // 22
        public float TunedRailDamage;          // Damage taken from tuned rail contact
        public float3 RailDirection;           // Current rail segment direction
        public bool IsOnTunedRail;             // Takes damage while true
    }

    public struct DissonanceHarvester : IComponentData
    {
        public float DrainRate;                // Energy drained per second from escort targets
        public float AttackDamage;             // 14
        public float MoveSpeed;               // 0.9x player speed — fast interceptor
        public float DetectionRadius;          // 30m — drawn to 432 Hz lullaby zones
        public float3 TargetPosition;          // Current escort target position
    }

    public struct DissonanceLeviathan : IComponentData
    {
        public float TotalHP;                  // Multi-phase boss HP pool
        public float AttackDamage;             // 30
        public int CurrentPhase;               // 0-2 escalating phases
        public float BodyLength;               // Serpentine — multiple hit segments
        public float ChargeSpeed;              // Lunges at the train
        public float LullabySusceptibility;    // Damage multiplier from children's lullaby
    }

    public struct SiegeGolem : IComponentData
    {
        public float AttackDamage;             // 35 — massive charge impact
        public float ChargeSpeed;              // 2x normal speed during charge
        public float ChargeCooldown;           // 6s between charges
        public float ChargeTimer;
        public float ArmorReduction;           // Damage reduction (0.5 = 50%)
        public float WallBreachDamage;         // Damage to fortifications on hit
        public bool IsCharging;
    }

    // ─── Moon 6 Enemy Components ─────────────────

    public struct HarmonicParasite : IComponentData
    {
        public float FeedRate;                 // Musical energy consumed per second
        public float AttackDamage;             // 10 — weak individually
        public float MoveSpeed;               // 1.1x player speed — agile
        public DamageType CounterPattern;      // Specific harmonic that kills it
        public float SpawnCooldown;            // Spawns on wrong organ notes
    }

    public struct DissonantConductor : IComponentData
    {
        public float CommandRadius;            // 25m — parasites in range are coordinated
        public float AttackDamage;             // 20
        public int MaxControlledParasites;     // 6 simultaneous
        public float CompositionTimer;         // Player must out-conduct within this window
        public float CompositionWindow;        // 10s real-time performance window
        public bool IsPerforming;              // True during coordination phase
    }

    // ─── Moon 8 Enemy Components ─────────────────

    public struct CorruptedCraft : IComponentData
    {
        public float FlightSpeed;              // 1.5x player speed — fast drone
        public float AttackDamage;             // 8 — weak but numerous
        public float BombCooldown;             // 3s between anti-Aether bomb drops
        public float BombTimer;
        public float3 FlightPattern;           // Predictable patrol direction
        public bool IsInFormation;             // Part of a swarm
    }

    public struct SkyReaver : IComponentData
    {
        public float AttackDamage;             // 25
        public float MoveSpeed;               // 1.2x player speed — airborne
        public float PhaseDuration;            // 3s phased, 2s solid
        public float PhaseTimer;
        public bool IsPhased;                  // Intangible except during attacks
        public float LeyLineDamageMultiplier;  // Extra damage from ley-line ambient resonance
    }

    // ─── Moon 9 Enemy Components ─────────────────

    public struct ProphecyGuardian : IComponentData
    {
        public float AttackDamage;             // 28 — ancient automated defense
        public float HarmonicTestThreshold;    // Player must demonstrate skill level
        public float TestTimer;                // Time allowed for puzzle-combat hybrid
        public bool IsHostile;                 // Becomes non-hostile after demonstration
        public float ArmorReduction;           // 0.4 — heavily armored until pacified
    }

    public struct ResetSeeker : IComponentData
    {
        public float AttackDamage;             // 16
        public float MoveSpeed;               // 1.3x player speed — fastest enemy
        public float GrenadeCooldown;          // 4s between dissonance grenades
        public float GrenadeTimer;
        public float GrenadeRadius;            // 8m AOE
        public float DissonanceDuration;       // 3s stone resonance disabled
        public int SquadSize;                  // Coordinated groups of 3-5
    }

    public struct TemporalWraith : IComponentData
    {
        public float AttackDamage;             // 22
        public float PhaseInDuration;          // 1.5s visible window
        public float PhaseOutDuration;         // 3s invisible
        public float PhaseTimer;
        public bool IsPhased;                  // Outside normal time
        public int PredictedMoveCount;         // Prophecy Shard reveals next 3 moves
        public float FrequencyShift;           // No fixed frequency — shifts constantly
    }

    // ─── Moon 11 Enemy Components ────────────────

    public struct LivingSludge : IComponentData
    {
        public float AttackDamage;             // 12 — weak but persistent
        public float RegenerationRate;         // HP/s regen unless source destroyed
        public float MoveSpeed;               // 0.5x player speed — slow
        public float TendrilRange;             // 5m — clogs pipes
        public float PurificationVulnerability; // Damage multiplier from fountain water
        public bool SourceDestroyed;           // Stops regen when true
    }

    public struct SludgeLeviathan : IComponentData
    {
        public float TotalHP;                  // Massive multi-stage boss
        public float AttackDamage;             // 40
        public int CurrentStage;               // 0-3 environmental stages
        public float WaterPressureDamage;      // Damage from directed fountain pressure
        public float GiantModeDamageMultiplier; // Extra damage when player is giant
        public float PurificationThreshold;    // Required purification to advance stage
    }

    // ─── Moon 7+ Enemy Components ────────────────

    public struct TitanGolem : IComponentData
    {
        public float AttackDamage;             // 45 — devastating melee
        public float ArmorReduction;           // 0.6 — heaviest armor in game
        public float Height;                   // 20+ ft — requires Giant Mode
        public float StompRadius;              // 10m AOE ground pound
        public float StompCooldown;            // 8s between stomps
        public float StompTimer;
        public bool RequiresGiantMode;         // True — regular attacks ineffective
    }

    public struct FrequencyWraith : IComponentData
    {
        public float AttackDamage;             // 20
        public float MoveSpeed;               // 0.9x player speed
        public float CurrentFrequency;         // Shifts every 5s
        public float FrequencyShiftInterval;   // 5s
        public float ShiftTimer;
        public float FrequencyTolerance;       // Hz window for player to match
        public bool IsVulnerable;              // True when player matches frequency
    }

    // ─── Damage Events ───────────────────────────
    public struct DamageEvent : IBufferElementData
    {
        public Entity Source;
        public Entity Target;
        public float Amount;
        public float Frequency;              // Frequency used in attack
        public DamageType Type;
    }

    public enum DamageType : byte
    {
        ResonancePulse  = 0, // AOE pushback, minor damage
        HarmonicStrike  = 1, // Directed, high single-target
        GolemSlam       = 2, // Enemy melee
        Environmental   = 3
    }

    // ─── Combat Spawn Trigger ────────────────────

    public struct EnemySpawnTrigger : IComponentData
    {
        public float RSThreshold;            // 25, 50, 75
        public EnemyType EnemyToSpawn;
        public float3 SpawnPosition;
        public bool HasSpawned;
    }

    // ─── Enemy AI State Machine ──────────────────
    //  Moved from Tartaria.AI to avoid circular
    //  assembly dependency (AI references Gameplay).

    public enum EnemyAIState : byte
    {
        Spawning   = 0,  // 3s grace period, forming from mud
        Patrolling = 1,  // Move toward nearest restored building
        Engaging   = 2,  // Player within range, attacking
        Stunned    = 3,  // Hit by 3 consecutive Resonance Pulses
        Dissolving = 4   // HP <= 0, playing death animation
    }

    public struct EnemyAI : IComponentData
    {
        public EnemyAIState State;
        public float StateTimer;
        public float3 PatrolTarget;
        public float EngageRadius;          // 50m
        public float AttackRange;           // 3m (melee)
        public float AttackCooldown;
        public float SpawnGracePeriod;      // 3s
        public float MoveSpeed;             // 0.6× player speed
    }
}
