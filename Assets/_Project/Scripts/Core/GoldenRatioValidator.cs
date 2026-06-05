using Unity.Mathematics;

namespace Tartaria.Core
{
    /// <summary>
    /// Golden ratio (φ) validation engine.
    /// Every RS-modifying event passes through this validator.
    /// The more beautiful a structure, the more powerful it is.
    /// </summary>
    public static class GoldenRatioValidator
    {
        public const float PHI = 1.6180339887f;
        public const float PHI_INVERSE = 0.6180339887f;
        public const float PHI_SQUARED = 2.6180339887f;
        public const float SQRT_PHI = 1.2720196495f;

        // Tolerance tiers for proportion matching
        public const float PERFECT_TOLERANCE = 0.02f;   // 2% — golden ratio match
        public const float GOOD_TOLERANCE = 0.10f;       // 10% — decent proportion
        public const float ACCEPTABLE_TOLERANCE = 0.20f;  // 20% — passable

        /// <summary>
        /// Returns a multiplier based on how close a ratio is to φ.
        /// Perfect match (within 2%): returns φ (1.618)
        /// Close (within 10%): returns 1.0–1.618 (scaled)
        /// Far: returns 1.0 (no bonus)
        /// </summary>
        public static float GetMultiplier(float ratio)
        {
            float deviation = math.abs(ratio - PHI) / PHI;

            if (deviation <= PERFECT_TOLERANCE)
                return PHI;  // Perfect golden ratio — full multiplier

            if (deviation <= GOOD_TOLERANCE)
                return 1.0f + (PHI_INVERSE * (1.0f - deviation / GOOD_TOLERANCE));

            return 1.0f;  // No bonus
        }

        /// <summary>
        /// Validates a building's proportions against golden ratio.
        /// Returns 0–1 representing how "golden" the proportions are.
        /// </summary>
        public static float ValidateBuildingProportion(float width, float height)
        {
            if (width <= 0f || height <= 0f) return 0f;

            float ratio = height > width ? height / width : width / height;
            float deviation = math.abs(ratio - PHI) / PHI;

            return math.saturate(1.0f - deviation / ACCEPTABLE_TOLERANCE);
        }

        /// <summary>
        /// Checks if two dimensions form a golden-ratio spiral segment.
        /// Used for sacred geometry snap validation.
        /// </summary>
        public static bool IsGoldenSpiral(float a, float b, float tolerance)
        {
            if (a <= 0f || b <= 0f) return false;
            float ratio = math.max(a, b) / math.min(a, b);
            return math.abs(ratio - PHI) / PHI <= tolerance;
        }

        /// <summary>
        /// Given a base dimension, returns the golden-ratio paired dimension.
        /// Used by the sacred geometry snap system.
        /// </summary>
        public static float GetGoldenPair(float baseDimension)
        {
            return baseDimension * PHI;
        }

        /// <summary>
        /// Frequency validation — checks if a frequency is harmonically
        /// aligned to 432 Hz (the universal healing harmonic).
        /// </summary>
        public static float GetFrequencyAccuracy(float frequency, float targetHz = 432f)
        {
            if (targetHz <= 0f) return 0f;
            float deviation = math.abs(frequency - targetHz) / targetHz;
            return math.saturate(1.0f - deviation);
        }
    }
}
