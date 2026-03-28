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
        MudGolem = 0,
        FractalWraith = 1,   // Moon 2: phases through matter, drains Aether
        MirrorWraith = 2     // Moon 2 elite: copies player's last 3 attacks
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
