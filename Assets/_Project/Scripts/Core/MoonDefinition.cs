using UnityEngine;

namespace Tartaria.Core
{
    public enum MoonMechanic
    {
        Excavation,        // 1
        DissonancePurge,   // 2
        OrphanTrain,       // 3
        FortifyDefense,    // 4
        Amplification,     // 5
        OrganRequiem,      // 6
        GiantMode,         // 7
        AirshipArmada,     // 8
        LeyProphecy,       // 9
        LivingGrid,        // 10
        SpectralVeil,      // 11
        BellTower,         // 12
        Convergence        // 13
    }

    public enum MoonCompanion
    {
        Milo,
        Cassian,
        Lirael,
        Korath,
        Thorne,
        Anastasia
    }

    /// <summary>
    /// Per-Moon design data — drives runtime bootstrapping (fog, ambient, music,
    /// companion spawn, mechanic activation, quest activation) without baking
    /// any per-Moon logic into MonoBehaviours. Authored by MoonDefinitionsFactory.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Moon Definition", fileName = "MoonDefinition")]
    public class MoonDefinition : ScriptableObject
    {
        [Header("Identity")]
        public int           number;
        public string        sceneName;
        public string        zoneName;
        public string        headline;        // e.g. "Magnetic Moon: Awakening"

        [Header("Narrative")]
        [TextArea(2, 5)] public string aetherWhisper;
        public MoonCompanion companion;
        public MoonMechanic  mechanic;

        [Header("Atmosphere")]
        public Color   fogColor    = new(0.5f, 0.5f, 0.5f);
        public Color   ambientHigh = new(0.5f, 0.5f, 0.5f);
        public Color   ambientLow  = new(0.2f, 0.2f, 0.2f);
        public float   fogDensity  = 0.02f;
        public Vector3 spawnPos;

        [Header("Progression")]
        public float   rsRequirement;        // resonance score threshold to unlock
        public string  questId;              // QuestDefinition asset stem ("Moon02_Awaken", etc.)

        // ─── B1 Moon Framework v2 (additive, all optional w/ safe defaults) ───
        [Header("Framework v2 — Beats")]
        [Tooltip("Headlines shown by MoonHUDBanner for each of the 5 beats: Discovery, Restoration, Conflict, Climax, Revelation. Leave empty for auto-generated stub.")]
        public string[] beatHeadlines = new string[5];
        [Tooltip("Per-beat duration in seconds; the Conflict beat will instead wait for MoonProgressTracker.MarkCleared if a MoonMechanicActivator is on the same root.")]
        public float[]  beatDurations = new float[] { 4f, 6f, 12f, 8f, 6f };

        [Header("Framework v2 — Reward + Crossover")]
        [Tooltip("Total RS payout on full clear. 0 = legacy compute (15 + number*2).")]
        public float    rewardRS;
        [Tooltip("Optional next-moon ID to mark unlocked on full clear. 0 = no unlock.")]
        public int      unlockMoonId;
        [Tooltip("Optional Addressable / Resources key for a climax set-piece prefab.")]
        public string   climaxPrefabKey;
    }
}
