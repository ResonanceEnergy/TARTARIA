using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Central constants ScriptableObject — golden ratio, 432 Hz, 3-6-9 bands.
    /// Single source of truth for all resonance and harmonic constants.
    ///
    /// Canonical Aether band naming per CLAUDE.md (2026-05-29 decision, resolves
    /// the doc 02 vs doc 15 contradiction):
    ///   Telluric  (7.83 Hz) — Schumann earth resonance
    ///   Harmonic  (432  Hz) — Verdi tuning, water
    ///   Celestial (528  Hz) — solfeggio, light
    /// The earlier "1296 Hz Celestial" is retired; 1296 survives only as a
    /// musical overtone (3x432), not as a band label.
    /// </summary>
    [CreateAssetMenu(fileName = "TartariaConstants", menuName = "Tartaria/Game Constants")]
    public class TartariaConstants : ScriptableObject
    {
        [Header("Golden Ratio")]
        public float phi = 1.6180339887f;
        public float phiInverse = 0.6180339887f;
        public float phiSquared = 2.6180339887f;
        public float sqrtPhi = 1.2720196495f;

        [Header("Base Frequency (canonical 3 Aether bands)")]
        public float baseFrequencyHz       = 432f;   // Harmonic band carrier
        public float telluricFrequencyHz   = 7.83f;  // Telluric band — Schumann
        public float harmonicFrequencyHz   = 432f;   // Harmonic band — Verdi tuning
        public float celestialFrequencyHz  = 528f;   // Celestial band — solfeggio (was 1296, fixed per CLAUDE.md)
        // Back-compat alias (older code referenced healingFrequencyHz; it is the same as celestial 528).
        public float healingFrequencyHz    = 528f;
        // Musical overtone (3x432) used by sound design — NOT a band frequency.
        public float overtoneHighHz        = 1296f;

        [Header("Aether Bands — 3-6-9 System (canonical)")]
        public float band3Frequency = 7.83f;     // Telluric  (low)   — Schumann
        public float band6Frequency = 432f;      // Harmonic  (mid)   — Verdi
        public float band9Frequency = 528f;      // Celestial (high)  — solfeggio (was 1296, fixed per CLAUDE.md)

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
