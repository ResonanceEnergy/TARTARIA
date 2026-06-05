using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// ScriptableObject holding zone configuration data.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Zone Definition")]
    public class ZoneDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string zoneName;
        public string sceneName; // Scene to load for this zone
        public string subtitle;
        [TextArea(2, 4)]
        public string loreIntro;

        [Header("Gameplay")]
        public int zoneIndex;
        public float rsRequirementToUnlock;
        [Tooltip("Quest that must be completed before this zone unlocks. Leave empty for no requirement.")]
        public string prerequisiteQuestId;
        public int buildingCount = 3;
        public Vector3 playerSpawnPosition;

        [Header("NPC Spawns")]
        public NPCSpawnPoint[] npcSpawnPoints = System.Array.Empty<NPCSpawnPoint>();

        [Header("Atmosphere")]
        public Color fogColorLow = new(0.3f, 0.25f, 0.2f);
        public Color fogColorHigh = new(0.8f, 0.75f, 0.5f);
        public float startingFogDensity = 0.03f;
        public Color ambientLow = new(0.15f, 0.12f, 0.1f);
        public Color ambientHigh = new(0.6f, 0.55f, 0.4f);

        [Header("Loading Screen")]
        [TextArea(1, 2)]
        public string loadingTip;
    }

    [System.Serializable]
    public class NPCSpawnPoint
    {
        public string npcId;
        public Vector3 position;
        public float yRotation;
        public bool requiresIntroduction;
    }
}
