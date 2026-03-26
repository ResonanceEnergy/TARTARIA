using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// ScriptableObject definition for Tartarian buildings.
    /// Defines art, audio, haptic, and gameplay data per building type.
    /// Used by the BuildingRestorationSystem to configure runtime entities.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string buildingName;
        [TextArea(3, 6)]
        public string loreDescription;
        public BuildingArchetype archetype;

        [Header("Geometry")]
        public float width = 25f;
        public float height = 18f;
        public float goldenRatioTarget = GoldenRatioValidator.PHI;

        [Header("Aether")]
        public float aetherSourceStrength = 1.0f;
        public float aetherSourceRadius = 50f;
        public HarmonicBand outputBand = HarmonicBand.Harmonic;

        [Header("Tuning")]
        public int nodeCount = 3;
        public TuningPuzzleConfig[] nodePuzzles;

        [Header("Prefabs")]
        public GameObject buriedPrefab;
        public GameObject revealedPrefab;
        public GameObject activePrefab;

        [Header("Audio")]
        public AudioClip emergenceSound;
        public AudioClip ambientHum;

        [Header("Haptics")]
        public float emergenceHapticDuration = 5.0f;

        [Header("Mud Dissolution")]
        public float dissolutionDuration = 5.0f;

        /// <summary>
        /// Returns the golden ratio accuracy for this building's proportions.
        /// </summary>
        public float GetGoldenRatioMatch()
        {
            return GoldenRatioValidator.ValidateBuildingProportion(width, height);
        }
    }

    [System.Serializable]
    public class TuningPuzzleConfig
    {
        public TuningVariant variant;
        public float targetFrequency = 432f;
        public float timeLimitSeconds = 15f;
        public float tolerancePercent = 0.08f; // 8% for first node
        [Range(0f, 1f)]
        public float difficultySpeed = 0.3f;
    }
}
