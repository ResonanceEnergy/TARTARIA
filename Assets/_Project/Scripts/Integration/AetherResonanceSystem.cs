using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Global Aether Resonance System — tracks and visualizes harmonic restoration progress.
    /// Manages the "Aether Field" that strengthens as buildings are purged and corruption clears.
    /// Field strength: 0.0 (baseline) to 1.0 (max harmony), increments by 0.1 per building purge.
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherResonanceSystem : MonoBehaviour
    {
        public static AetherResonanceSystem Instance { get; private set; }

        float _fieldStrength = 0f; // 0.0 to 1.0
        int _purgedBuildingsCount = 0;

        public float FieldStrength => _fieldStrength;
        public int PurgedBuildingsCount => _purgedBuildingsCount;

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
            _purgedBuildingsCount++;
            _fieldStrength = Mathf.Min(_fieldStrength + 0.1f, 1.0f);
            Debug.Log($"[AetherResonanceSystem] Building '{buildingName}' purged | Field strength: {_fieldStrength:F2} | Total purges: {_purgedBuildingsCount}");
        }

        /// <summary>Resets the aether field to baseline (e.g., on moon transition).</summary>
        public void ResetField()
        {
            _fieldStrength = 0f;
            _purgedBuildingsCount = 0;
            Debug.Log("[AetherResonanceSystem] Aether field reset to baseline");
        }

        /// <summary>Returns the current aether field strength (0.0 to 1.0).</summary>
        public float GetFieldStrength()
        {
            return _fieldStrength;
        }

        /// <summary>Add resonance score with description (Moon 2 restoration rewards).</summary>
        public void AddResonance(float amount, string description)
        {
            Debug.Log($"[AetherResonanceSystem] AddResonance: +{amount} RS - {description}");

            // Try to integrate with economy system if available
            var economy = GameObject.FindFirstObjectByType<Tartaria.Core.EconomySystem>();
            if (economy != null)
            {
                // EconomySystem.AwardResonanceScore method integration (if available)
                economy.SendMessage("AwardResonanceScore", amount, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[AetherResonanceSystem] EconomySystem not found — RS reward not applied");
            }
        }
    }
}
