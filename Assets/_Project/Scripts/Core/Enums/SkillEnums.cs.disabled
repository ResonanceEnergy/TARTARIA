namespace Tartaria.Core.Enums
{
    /// <summary>
    /// Skill tree archetype classification.
    /// Defines the four primary progression paths in Tartaria.
    /// </summary>
    public enum SkillTreeType : byte
    {
        Resonator = 0,   // Frequency mastery
        Architect = 1,   // Building enhancement
        Guardian  = 2,   // Combat skills
        Historian = 3    // Lore and discovery
    }

    /// <summary>
    /// Unique identifier for every skill node in all trees.
    /// Organized by tree type: Resonator (100+), Architect (200+), Guardian (300+), Historian (400+).
    /// Moon progression blessings: Moon 2 (500+), Echohaven (600+).
    /// </summary>
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
        // These are the core progression hooks. Auto-granted (no RS spend) by Moon2ProgressionSystem when player purges/restores the five key sites.
        // They make progression feel deeply tied to the "purge the corruption" fantasy: each restored cathedral, bell, fountain, hall, and ley chamber leaves an indelible, powerful, visual change in the player that carries forward.
        M2_CathedralBreath   = 500,
        M2_BellCleansing     = 501,
        M2_FountainSpring    = 502,
        M2_CrystalLens       = 503,
        M2_LeyBond           = 504,
        M2_TrueLunarPurifier = 505,

        // Moon 1 Echohaven Early Progression Permanent Hub Blessings (600+)
        // Auto-granted by EchohavenProgressionSystem on restoring the 3 core buildings of the starting hub (fountain/dome/spire).
        // Provides meaningful, permanent early-game player power growth and world-state changes that persist via Skill save/load.
        // Restoring the hub now feels like a foundational, lasting transformation rather than a one-off event.
        E_FountainEcho   = 600,
        E_DomeInsight    = 601,
        E_SpireResonance = 602,
        E_HubAwakened    = 603
    }

    /// <summary>
    /// Skill modifier types applied to player stats.
    /// Used by SkillNodeData to define what each skill improves.
    /// </summary>
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
        // Moon 2 progression extensions (used by lunar purge blessings)
        CorruptionResistance = 10,
        LunarRSBonus         = 11,
        MicroGiantExtend     = 12
    }
}
