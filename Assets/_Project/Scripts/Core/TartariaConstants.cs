using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Central constants ScriptableObject — golden ratio, 432 Hz, 3-6-9 bands.
    /// Single source of truth for all resonance and harmonic constants.
    /// </summary>
    [CreateAssetMenu(fileName = "TartariaConstants", menuName = "Tartaria/Game Constants")]
    public class TartariaConstants : ScriptableObject
    {
        [Header("Golden Ratio")]
        public float phi = 1.6180339887f;
        public float phiInverse = 0.6180339887f;
        public float phiSquared = 2.6180339887f;
        public float sqrtPhi = 1.2720196495f;

        [Header("Base Frequency")]
        public float baseFrequencyHz = 432f;
        public float telluricFrequencyHz = 7.83f;
        public float healingFrequencyHz = 528f;
        public float celestialFrequencyHz = 1296f;

        [Header("Aether Bands — 3-6-9 System")]
        public float band3Frequency = 129.6f;   // Telluric  (low)
        public float band6Frequency = 432f;      // Harmonic  (mid)
        public float band9Frequency = 1296f;     // Celestial (high)

        [Header("RS Thresholds")]
        public float rsThresholdLit = 25f;
        public float rsThresholdHarmonic = 50f;
        public float rsThresholdRadiant = 75f;
        public float rsThresholdFull = 100f;

        [Header("RS Rewards")]
        public float rsDiscovery = 5f;
        public float rsTuneBasic = 10f;
        public float rsTunePerfect = 25f;
        public float rsRestore = 50f;
        public float rsDefeatEnemy = 15f;

        [Header("Multipliers")]
        public float goldenRatioMultiplier = 1.618f;
        public float freq432Multiplier = 1.5f;
        public float perfectNodeMultiplier = 2.0f;
        public float harmonicsOnlyMultiplier = 1.3f;
    }
}
