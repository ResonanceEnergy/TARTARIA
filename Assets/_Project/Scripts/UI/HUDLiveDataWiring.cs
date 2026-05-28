using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// HUDLiveDataWiring — Binds HUD to live game data.
    /// TODO from REALITY_CHECK Phase 2.
    /// </summary>
    public class HUDLiveDataWiring : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI rsCounterText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider aetherMeter;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI aetherText;

        void Start()
        {
            // Subscribe to game events
            GameEvents.OnResonanceScoreChanged += UpdateRSCounter;
            GameEvents.OnPlayerHealthChanged += UpdateHealthBar;
            GameEvents.OnAetherEnergyChanged += UpdateAetherMeter;

            // Initial update
            UpdateAllDisplays();

            Debug.Log("[HUDLiveDataWiring] ✅ HUD wired to live data");
        }

        void OnDestroy()
        {
            GameEvents.OnResonanceScoreChanged -= UpdateRSCounter;
            GameEvents.OnPlayerHealthChanged -= UpdateHealthBar;
            GameEvents.OnAetherEnergyChanged -= UpdateAetherMeter;
        }

        void Update()
        {
            // Fallback polling for systems without events
            UpdateAllDisplays();
        }

        void UpdateAllDisplays()
        {
            // RS Counter
            if (GameLoopController.Instance != null && rsCounterText != null)
            {
                rsCounterText.text = $"RS: {GameLoopController.Instance.GetCurrentRS():F0}";
            }

            // Health Bar
            var player = FindFirstObjectByType<PlayerHealthController>();
            if (player != null && healthBar != null)
            {
                float currentHealth = player.CurrentHealth;
                float maxHealth = player.MaxHealth;
                healthBar.value = currentHealth / maxHealth;
                if (healthText != null)
                    healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }

            // Aether Meter (placeholder - needs AetherFieldSystem integration)
            if (aetherMeter != null)
            {
                aetherMeter.value = 0.75f; // Placeholder
                if (aetherText != null)
                    aetherText.text = "Aether: 75%";
            }
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
                healthBar.value = currentHealth / maxHealth;
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
