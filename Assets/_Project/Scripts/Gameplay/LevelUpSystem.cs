using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// LevelUpSystem — player experience, leveling, stat allocation.
    /// Tracks XP from combat/quests/discoveries → level ups → stat point allocation.
    /// Exponential XP curve (100 * level^1.5), max level 50.
    /// 
    /// Stats (5 base + scaling):
    /// - Vitality → +10 HP per point
    /// - Resonance → +5 RS per point, ability power +2%
    /// - Strength → melee damage +3%, carry weight +5 kg
    /// - Agility → dodge chance +1%, movement speed +2%
    /// - Attunement → magic damage +3%, RS regen +10%
    /// 
    /// Leveling:
    /// - XP sources: enemy kill (10-100), quest (50-500), discovery (25)
    /// - 3 stat points per level
    /// - Respec cost: 100 RS per stat point moved
    /// 
    /// Usage:
    /// - LevelUpSystem.Instance.AddXP(50)
    /// - LevelUpSystem.Instance.AllocateStat(StatType.Vitality, 1)
    /// - Subscribe to OnLevelUp event for UI notification
    /// 
    /// GDD refs: §06 (Player Progression), §09 (Combat Scaling)
    /// </summary>
    public class LevelUpSystem : MonoBehaviour
    {
        public static LevelUpSystem Instance { get; private set; }

        [Header("Level Settings")]
        [SerializeField] int maxLevel = 50;
        [SerializeField] int baseXPRequirement = 100;
        [SerializeField] float xpExponent = 1.5f;
        [SerializeField] int statPointsPerLevel = 3;

        [Header("Current State")]
        [SerializeField] int currentLevel = 1;
        [SerializeField] int currentXP = 0;
        [SerializeField] int availableStatPoints = 0;

        [Header("Stats")]
        [SerializeField] int vitality = 5;
        [SerializeField] int resonance = 5;
        [SerializeField] int strength = 5;
        [SerializeField] int agility = 5;
        [SerializeField] int attunement = 5;

        public event Action<int> OnLevelUp;  // New level
        public event Action<int> OnXPGained;  // XP amount
        public event Action<StatType, int> OnStatAllocated;  // Stat type, new value

        public int CurrentLevel => currentLevel;
        public int CurrentXP => currentXP;
        public int AvailableStatPoints => availableStatPoints;

        public int Vitality => vitality;
        public int Resonance => resonance;
        public int Strength => strength;
        public int Agility => agility;
        public int Attunement => attunement;

        // Derived stats
        public int MaxHP => 100 + (vitality * 10);
        public int MaxRS => 100 + (resonance * 5);
        public float MeleeDamageMultiplier => 1f + (strength * 0.03f);
        public float DodgeChance => 0.05f + (agility * 0.01f);
        public float MagicDamageMultiplier => 1f + (attunement * 0.03f);
        public float RSRegenRate => 1f + (attunement * 0.1f);
        public float MovementSpeedMultiplier => 1f + (agility * 0.02f);
        public int CarryWeight => 50 + (strength * 5);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadPlayerStats();
        }

        /// <summary>
        /// Add experience points.
        /// </summary>
        public void AddXP(int amount)
        {
            if (currentLevel >= maxLevel)
            {
                Debug.Log("[LevelUp] Already at max level");
                return;
            }

            currentXP += amount;

            OnXPGained?.Invoke(amount);

            Debug.Log($"[LevelUp] +{amount} XP (total {currentXP})");

            // Check for level up
            int xpRequired = GetXPRequiredForNextLevel();
            while (currentXP >= xpRequired && currentLevel < maxLevel)
            {
                LevelUp();
                xpRequired = GetXPRequiredForNextLevel();
            }

            SavePlayerStats();
        }

        void LevelUp()
        {
            currentLevel++;
            availableStatPoints += statPointsPerLevel;

            // Roll over excess XP
            int xpRequired = GetXPRequiredForPreviousLevel();
            currentXP -= xpRequired;

            Debug.Log($"[LevelUp] LEVEL UP! Now level {currentLevel} (+{statPointsPerLevel} stat points)");

            OnLevelUp?.Invoke(currentLevel);

            // TODO: Show level up UI, play fanfare SFX
            Audio.AudioManager.Instance?.PlaySFX("level_up", Vector3.zero);
        }

        /// <summary>
        /// Allocate stat point.
        /// </summary>
        public bool AllocateStat(StatType statType, int points = 1)
        {
            if (availableStatPoints < points)
            {
                Debug.LogWarning($"[LevelUp] Not enough stat points ({availableStatPoints} available)");
                return false;
            }

            switch (statType)
            {
                case StatType.Vitality:
                    vitality += points;
                    break;
                case StatType.Resonance:
                    resonance += points;
                    break;
                case StatType.Strength:
                    strength += points;
                    break;
                case StatType.Agility:
                    agility += points;
                    break;
                case StatType.Attunement:
                    attunement += points;
                    break;
            }

            availableStatPoints -= points;

            Debug.Log($"[LevelUp] Allocated {points} point(s) to {statType} (now {GetStatValue(statType)})");

            OnStatAllocated?.Invoke(statType, GetStatValue(statType));

            SavePlayerStats();

            return true;
        }

        /// <summary>
        /// Get stat value by type.
        /// </summary>
        public int GetStatValue(StatType statType)
        {
            return statType switch
            {
                StatType.Vitality => vitality,
                StatType.Resonance => resonance,
                StatType.Strength => strength,
                StatType.Agility => agility,
                StatType.Attunement => attunement,
                _ => 0
            };
        }

        /// <summary>
        /// Get XP required for next level.
        /// </summary>
        public int GetXPRequiredForNextLevel()
        {
            if (currentLevel >= maxLevel) return int.MaxValue;

            return Mathf.RoundToInt(baseXPRequirement * Mathf.Pow(currentLevel, xpExponent));
        }

        /// <summary>
        /// Get XP required for previous level (for overflow calc).
        /// </summary>
        int GetXPRequiredForPreviousLevel()
        {
            if (currentLevel <= 1) return 0;

            return Mathf.RoundToInt(baseXPRequirement * Mathf.Pow(currentLevel - 1, xpExponent));
        }

        /// <summary>
        /// Get XP progress as 0-1 float.
        /// </summary>
        public float GetXPProgress()
        {
            int xpRequired = GetXPRequiredForNextLevel();
            return Mathf.Clamp01((float)currentXP / xpRequired);
        }

        /// <summary>
        /// Reset stats (costs RS).
        /// </summary>
        public bool RespecStats(int rsCost = 100)
        {
            // TODO: Check if player has enough RS

            // Reset to base stats (5 each)
            int totalPointsSpent = (vitality - 5) + (resonance - 5) + (strength - 5) + (agility - 5) + (attunement - 5);

            vitality = 5;
            resonance = 5;
            strength = 5;
            agility = 5;
            attunement = 5;

            availableStatPoints += totalPointsSpent;

            Debug.Log($"[LevelUp] Respec complete, {totalPointsSpent} points refunded");

            SavePlayerStats();

            return true;
        }

        void LoadPlayerStats()
        {
            currentLevel = PlayerPrefs.GetInt("LevelUpSystem_Level", 1);
            currentXP = PlayerPrefs.GetInt("LevelUpSystem_XP", 0);
            availableStatPoints = PlayerPrefs.GetInt("LevelUpSystem_AvailablePoints", 0);

            vitality = PlayerPrefs.GetInt("LevelUpSystem_Vitality", 5);
            resonance = PlayerPrefs.GetInt("LevelUpSystem_Resonance", 5);
            strength = PlayerPrefs.GetInt("LevelUpSystem_Strength", 5);
            agility = PlayerPrefs.GetInt("LevelUpSystem_Agility", 5);
            attunement = PlayerPrefs.GetInt("LevelUpSystem_Attunement", 5);

            Debug.Log($"[LevelUp] Loaded stats: Level {currentLevel}, {availableStatPoints} points available");
        }

        void SavePlayerStats()
        {
            PlayerPrefs.SetInt("LevelUpSystem_Level", currentLevel);
            PlayerPrefs.SetInt("LevelUpSystem_XP", currentXP);
            PlayerPrefs.SetInt("LevelUpSystem_AvailablePoints", availableStatPoints);

            PlayerPrefs.SetInt("LevelUpSystem_Vitality", vitality);
            PlayerPrefs.SetInt("LevelUpSystem_Resonance", resonance);
            PlayerPrefs.SetInt("LevelUpSystem_Strength", strength);
            PlayerPrefs.SetInt("LevelUpSystem_Agility", agility);
            PlayerPrefs.SetInt("LevelUpSystem_Attunement", attunement);

            PlayerPrefs.Save();
        }

        public enum StatType : byte
        {
            Vitality = 0,
            Resonance = 1,
            Strength = 2,
            Agility = 3,
            Attunement = 4
        }
    }
}
