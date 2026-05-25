using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Difficulty Settings — cognitive accessibility presets for TARTARIA.
    /// Adjusts combat damage, tuning windows, quest guidance, and resource availability.
    /// Can be changed at any time without penalty.
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        public enum DifficultyMode
        {
            Story,      // Focus on narrative, minimal combat challenge
            Balanced,   // Standard experience
            Challenge   // Tighter timing, higher stakes
        }

        public DifficultyMode mode = DifficultyMode.Balanced;

        // Combat modifiers
        public float playerDamageMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float enemyHealthMultiplier = 1f;

        // Tuning modifiers
        public float tuningWindowMultiplier = 1f;
        public bool showFrequencyHints = true;

        // Navigation assistance
        public bool showQuestMarkers = true;
        public bool showObjectiveTrails = false;
        public bool showEnemyHealthBars = true;

        // Resource modifiers
        public float aetherGainMultiplier = 1f;
        public float lootDropMultiplier = 1f;

        // Auto-assist options
        public bool autoEvadeAtLowHealth = false;
        public float autoEvadeThreshold = 0.2f;

        public static DifficultySettings CreatePreset(DifficultyMode mode)
        {
            var settings = new DifficultySettings { mode = mode };

            switch (mode)
            {
                case DifficultyMode.Story:
                    settings.playerDamageMultiplier = 1.5f;
                    settings.enemyDamageMultiplier = 0.6f;
                    settings.enemyHealthMultiplier = 0.7f;
                    settings.tuningWindowMultiplier = 2f;
                    settings.showFrequencyHints = true;
                    settings.showQuestMarkers = true;
                    settings.showObjectiveTrails = true;
                    settings.showEnemyHealthBars = true;
                    settings.aetherGainMultiplier = 1.5f;
                    settings.lootDropMultiplier = 1.3f;
                    settings.autoEvadeAtLowHealth = true;
                    settings.autoEvadeThreshold = 0.3f;
                    break;

                case DifficultyMode.Balanced:
                    settings.playerDamageMultiplier = 1f;
                    settings.enemyDamageMultiplier = 1f;
                    settings.enemyHealthMultiplier = 1f;
                    settings.tuningWindowMultiplier = 1f;
                    settings.showFrequencyHints = true;
                    settings.showQuestMarkers = true;
                    settings.showObjectiveTrails = false;
                    settings.showEnemyHealthBars = true;
                    settings.aetherGainMultiplier = 1f;
                    settings.lootDropMultiplier = 1f;
                    settings.autoEvadeAtLowHealth = false;
                    break;

                case DifficultyMode.Challenge:
                    settings.playerDamageMultiplier = 0.8f;
                    settings.enemyDamageMultiplier = 1.3f;
                    settings.enemyHealthMultiplier = 1.4f;
                    settings.tuningWindowMultiplier = 0.7f;
                    settings.showFrequencyHints = false;
                    settings.showQuestMarkers = true;
                    settings.showObjectiveTrails = false;
                    settings.showEnemyHealthBars = false;
                    settings.aetherGainMultiplier = 0.8f;
                    settings.lootDropMultiplier = 0.9f;
                    settings.autoEvadeAtLowHealth = false;
                    break;
            }

            return settings;
        }

        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetInt("TARTARIA_DifficultyMode", (int)mode);
            PlayerPrefs.SetFloat("TARTARIA_PlayerDamageMulti", playerDamageMultiplier);
            PlayerPrefs.SetFloat("TARTARIA_EnemyDamageMulti", enemyDamageMultiplier);
            PlayerPrefs.SetFloat("TARTARIA_EnemyHealthMulti", enemyHealthMultiplier);
            PlayerPrefs.SetFloat("TARTARIA_TuningWindowMulti", tuningWindowMultiplier);
            PlayerPrefs.SetInt("TARTARIA_ShowFreqHints", showFrequencyHints ? 1 : 0);
            PlayerPrefs.SetInt("TARTARIA_ShowQuestMarkers", showQuestMarkers ? 1 : 0);
            PlayerPrefs.SetInt("TARTARIA_ShowObjectiveTrails", showObjectiveTrails ? 1 : 0);
            PlayerPrefs.SetInt("TARTARIA_AutoEvade", autoEvadeAtLowHealth ? 1 : 0);
            PlayerPrefs.SetFloat("TARTARIA_AutoEvadeThreshold", autoEvadeThreshold);
            PlayerPrefs.Save();
        }

        public static DifficultySettings LoadFromPlayerPrefs()
        {
            var mode = (DifficultyMode)PlayerPrefs.GetInt("TARTARIA_DifficultyMode", (int)DifficultyMode.Balanced);
            var settings = CreatePreset(mode);

            // Load custom overrides if present
            if (PlayerPrefs.HasKey("TARTARIA_PlayerDamageMulti"))
            {
                settings.playerDamageMultiplier = PlayerPrefs.GetFloat("TARTARIA_PlayerDamageMulti");
                settings.enemyDamageMultiplier = PlayerPrefs.GetFloat("TARTARIA_EnemyDamageMulti");
                settings.enemyHealthMultiplier = PlayerPrefs.GetFloat("TARTARIA_EnemyHealthMulti");
                settings.tuningWindowMultiplier = PlayerPrefs.GetFloat("TARTARIA_TuningWindowMulti");
                settings.showFrequencyHints = PlayerPrefs.GetInt("TARTARIA_ShowFreqHints") == 1;
                settings.showQuestMarkers = PlayerPrefs.GetInt("TARTARIA_ShowQuestMarkers") == 1;
                settings.showObjectiveTrails = PlayerPrefs.GetInt("TARTARIA_ShowObjectiveTrails") == 1;
                settings.autoEvadeAtLowHealth = PlayerPrefs.GetInt("TARTARIA_AutoEvade") == 1;
                settings.autoEvadeThreshold = PlayerPrefs.GetFloat("TARTARIA_AutoEvadeThreshold", 0.2f);
            }

            return settings;
        }
    }
}
