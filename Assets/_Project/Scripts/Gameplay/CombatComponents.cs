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
        MudGolem = 0
        // Future: DissonanceCrystal, CorruptedGuardian, etc.
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
