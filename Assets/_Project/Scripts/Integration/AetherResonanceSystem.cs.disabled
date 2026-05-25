using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// STUB: Global Aether Resonance System — tracks and visualizes harmonic restoration progress.
    /// Manages the "Aether Field" that strengthens as buildings are purged and corruption clears.
    /// TODO: Implement full aether resonance mechanics with field strength, harmonic frequencies, and global effects.
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherResonanceSystem : MonoBehaviour
    {
        public static AetherResonanceSystem Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Registers a building purge event to strengthen the aether field.</summary>
        public void RegisterBuildingPurge(string buildingName)
        {
            // TODO: Implement aether field strength tracking
            Debug.Log($"[AetherResonanceSystem] RegisterBuildingPurge({buildingName}) - STUB");
        }

        /// <summary>Resets the aether field to baseline (e.g., on moon transition).</summary>
        public void ResetField()
        {
            // TODO: Implement field reset logic
            Debug.Log("[AetherResonanceSystem] ResetField() - STUB");
        }

        /// <summary>Returns the current aether field strength (0.0 to 1.0).</summary>
        public float GetFieldStrength()
        {
            // TODO: Calculate actual field strength based on purged buildings
            return 0f;
        }

        /// <summary>Add resonance score with description (Moon 2 restoration rewards).</summary>
        public void AddResonance(float amount, string description)
        {
            Debug.Log($"[AetherResonanceSystem] AddResonance: +{amount} RS - {description}");
            // TODO: Integrate with ResonanceScoreSystem
        }
    }
}
