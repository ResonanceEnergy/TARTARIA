using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// HUDLiveDataWiring — Binds HUD to live game data via GameEvents.
    /// Event-driven only. No polling, no hardcoded fallbacks.
    /// Publishers: GameEvents.FireRSChange / FirePlayerHealthChange / FireAetherEnergyChange
    /// (see GameEvents.cs:321-323, 563-564, 569-570).
    /// </summary>
    public class HUDLiveDataWiring : MonoBehaviour
    {
        // Sentinel string for "no data yet" — dimmed/empty state before any event fires.
        private const string EmptyRsLabel = "RS: --";
        private const string EmptyHealthLabel = "--/--";
        private const string EmptyAetherLabel = "Aether: --";

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI rsCounterText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider aetherMeter;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI aetherText;

        void Start()
        {
            // Subscribe to game events — these are the ONLY source of HUD values.
            GameEvents.OnResonanceScoreChanged += UpdateRSCounter;
            GameEvents.OnPlayerHealthChanged += UpdateHealthBar;
            GameEvents.OnAetherEnergyChanged += UpdateAetherMeter;

            // Initialize to explicit "no data yet" state — NOT a fake value.
            // The first matching GameEvents.Fire* call will replace these.
            if (rsCounterText != null) rsCounterText.text = EmptyRsLabel;
            if (healthBar != null) healthBar.value = 0f;
            if (healthText != null) healthText.text = EmptyHealthLabel;
            if (aetherMeter != null) aetherMeter.value = 0f;
            if (aetherText != null) aetherText.text = EmptyAetherLabel;

            Debug.Log("[HUDLiveDataWiring] HUD subscribed to GameEvents (event-driven, no polling).");
        }

        void OnDestroy()
        {
            GameEvents.OnResonanceScoreChanged -= UpdateRSCounter;
            GameEvents.OnPlayerHealthChanged -= UpdateHealthBar;
            GameEvents.OnAetherEnergyChanged -= UpdateAetherMeter;
        }

        void UpdateRSCounter(float rsValue)
        {
            if (rsCounterText != null)
                rsCounterText.text = $"RS: {rsValue:F0}";
        }

        void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthBar != null)
            {
                healthBar.value = maxHealth > 0f ? currentHealth / maxHealth : 0f;
                if (healthText != null)
                    healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }

        void UpdateAetherMeter(float aetherValue)
        {
            if (aetherMeter != null)
            {
                aetherMeter.value = aetherValue / 100f;
                if (aetherText != null)
                    aetherText.text = $"Aether: {aetherValue:F0}%";
            }
        }
    }
}
