// Skill system enums moved from Gameplay to Core.Enums to break circular dependency
// Data ScriptableObjects need these types, but Data cannot reference Gameplay (Gameplay already references Data)
// CANONICAL SOURCE - DO NOT DUPLICATE in Gameplay namespace

namespace Tartaria.Core.Enums
{
    public enum SkillTreeType : byte
    {
        Resonator = 0,   // Frequency manipulation
        Architect = 1,   // Building enhancement
        Guardian  = 2,   // Combat skills
        Historian = 3    // Lore and discovery
    }

    public enum SkillId : int
    {
        None = 0,

        // Resonator tree (100+)
        Res_FreqSense   = 100,
        Res_TuneSpeed   = 101,
        Res_AetherPool  = 102,
        Res_Cascade     = 103,
        Res_MasterFreq  = 104,

        // Architect tree (200+)
        Arc_BlueprintScan = 200,
        Arc_QuickRepair   = 201,
        Arc_Fortify       = 202,
        Arc_MassRestore   = 203,
        Arc_GoldenRatio   = 204,

        // Guardian tree (300+)
        Grd_StrongPulse    = 300,
        Grd_ShieldDuration = 301,
        Grd_StrikeRange    = 302,
        Grd_AOEPurge       = 303,
        Grd_Invulnerable   = 304,
        // Round 4: Giant advanced (flight, EarthShaper, WorldMover, Ancestral/Colossus/Avatar, Cassian/Anastasia harmony, Titan 180s, cooldowns)
        Grd_TitanFlight = 305,
        Grd_EarthShaper = 306,
        Grd_WorldMover = 307,
        Grd_AncestralTitan = 308,
        Grd_ColossusForm = 309,
        Grd_AvatarForm = 310,
        Grd_GiantResonanceHarmony = 311,
        Grd_TitanStability = 312,
        Grd_AbilityCooldownMastery = 313,

        // Historian tree (400+)
        His_LoreReveal   = 400,
        His_SecretPaths  = 401,
        His_MemoryEcho   = 402,
        His_AncientMap   = 403,
        His_TrueHistory  = 404,

        // Moon 2 (Lunar Moon / Crystalline Caverns) Permanent Purge Blessings & Mutations (500+)
        M2_CathedralBreath   = 500,
        M2_BellCleansing     = 501,
        M2_FountainSpring    = 502,
        M2_CrystalLens       = 503,
        M2_LeyBond           = 504,
        M2_TrueLunarPurifier = 505,

        // Moon 1 Echohaven Early Progression Permanent Hub Blessings (600+)
        E_FountainEcho   = 600,
        E_DomeInsight    = 601,
        E_SpireResonance = 602,
        E_HubAwakened    = 603
    }

    public enum SkillModifierType : byte
    {
        TuningPrecision    = 0,
        TuningSpeed        = 1,
        AetherCapacity     = 2,
        ComboDuration      = 3,
        RepairSpeed        = 4,
        BuildingResistance = 5,
        RSMultiplier       = 6,
        PulseDamage        = 7,
        ShieldDuration     = 8,
        StrikeRange        = 9,
        CorruptionResistance = 10,
        LunarRSBonus         = 11,
        MicroGiantExtend     = 12
    }
}
