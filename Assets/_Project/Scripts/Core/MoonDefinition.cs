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
    }
}
