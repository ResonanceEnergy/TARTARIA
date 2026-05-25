using UnityEngine;

namespace Tartaria.Data
{
    /// <summary>
    /// GameBalanceConfig — Single source of truth for all game balance values.
    /// ScriptableObject singleton loaded from Resources/GameBalanceConfig.asset
    /// Agent 11: Centralized tuning for Gameplay assembly numeric literals
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Tartaria/Game Balance Config")]
    public class GameBalanceConfig : ScriptableObject
    {
        static GameBalanceConfig _instance;
        public static GameBalanceConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
                    if (_instance == null)
                    {
                        Debug.LogError("[GameBalanceConfig] Asset not found in Resources/ — using fallback defaults");
                        _instance = CreateInstance<GameBalanceConfig>();
                    }
                }
                return _instance;
            }
        }

        [Header("=== PLAYER PROGRESSION ===")]
        [Tooltip("Maximum player level")]
        public int maxLevel = 50;
        
        [Tooltip("Base XP required for level 2 (scales exponentially)")]
        public int baseXPRequirement = 100;
        
        [Tooltip("XP curve exponent (xp = base * level^exponent)")]
        public float xpExponent = 1.5f;
        
        [Tooltip("Stat points awarded per level up")]
        public int statPointsPerLevel = 3;
        
        [Tooltip("Starting value for all base stats")]
        public int baseStatValue = 5;

        [Header("Derived Stats - Base Values")]
        [Tooltip("Base HP before vitality scaling")]
        public int baseMaxHP = 100;
        
        [Tooltip("Base RS before resonance scaling")]
        public int baseMaxRS = 100;
        
        [Tooltip("Base carry weight before strength scaling")]
        public int baseCarryWeight = 50;
        
        [Tooltip("Base dodge chance before agility scaling")]
        public float baseDodgeChance = 0.05f;

        [Header("Derived Stats - Scaling Multipliers")]
        [Tooltip("HP gained per vitality point")]
        public int hpPerVitality = 10;
        
        [Tooltip("RS gained per resonance point")]
        public int rsPerResonance = 5;
        
        [Tooltip("Ability power bonus per resonance point")]
        public float abilityPowerPerResonance = 0.02f;
        
        [Tooltip("Melee damage bonus per strength point")]
        public float meleeDamagePerStrength = 0.03f;
        
        [Tooltip("Carry weight gained per strength point")]
        public int carryWeightPerStrength = 5;
        
        [Tooltip("Dodge chance gained per agility point")]
        public float dodgeChancePerAgility = 0.01f;
        
        [Tooltip("Movement speed bonus per agility point")]
        public float movementSpeedPerAgility = 0.02f;
        
        [Tooltip("Magic damage bonus per attunement point")]
        public float magicDamagePerAttunement = 0.03f;
        
        [Tooltip("RS regen bonus per attunement point")]
        public float rsRegenPerAttunement = 0.1f;

        [Header("=== COMBAT SYSTEM ===")]
        [Header("Player Combat")]
        [Tooltip("Base player melee damage")]
        public float playerBaseMeleeDamage = 20f;
        
        [Tooltip("Player melee attack damage (PlayerCombat.cs)")]
        public int playerMeleeDamage = 25;
        
        [Tooltip("Player melee reach distance")]
        public float playerMeleeReach = 2.6f;
        
        [Tooltip("Player melee attack radius")]
        public float playerMeleeRadius = 1.4f;
        
        [Tooltip("Player melee cooldown (seconds)")]
        public float playerMeleeCooldown = 0.45f;
        
        [Tooltip("Player melee swing animation duration")]
        public float playerSwingDuration = 0.25f;
        
        [Tooltip("Vertical offset for melee attack origin")]
        public float meleeVerticalOffset = 1.2f;
        
        [Tooltip("Forward offset multiplier for melee sphere cast")]
        public float meleeForwardOffsetMultiplier = 0.5f;
        
        [Tooltip("Damage scaling from skill tree pulse damage modifier")]
        public float pulseDamageSkillScaling = 0.5f;
        
        [Tooltip("Camera impulse magnitude on hit")]
        public float meleeHitImpulseMagnitude = 0.5f;

        [Header("Enemy Combat")]
        [Tooltip("Golem melee attack damage")]
        public float golemAttackDamage = 15f;
        
        [Tooltip("Default enemy HP (Golem)")]
        public float defaultEnemyHP = 300f;
        
        [Tooltip("Default enemy dissonant frequency")]
        public float defaultDissonantFreq = 174f;
        
        [Tooltip("Default enemy movement speed")]
        public float defaultMoveSpeed = 4f;
        
        [Tooltip("Default enemy attack range")]
        public float defaultAttackRange = 3f;
        
        [Tooltip("Boss enemy attack range")]
        public float bossAttackRange = 8f;

        [Header("Combat Tuning — Agent 4 Fixes")]
        [Tooltip("Armor damage reduction formula: damageReduction = armor / (armor + armorEffectivenessConstant)")]
        public float armorEffectivenessConstant = 100f;

        [Tooltip("Base armor value for standard enemies (reduces damage by ~23% at 30 armor)")]
        public float enemyBaseArmor = 30f;

        [Tooltip("Boss armor multiplier (2x = ~37% reduction at 60 armor)")]
        public float bossArmorMultiplier = 2f;

        [Tooltip("Player damage scaling per level above enemy (1.05 = +5% per level)")]
        public float damageScalingPerLevel = 1.05f;

        [Tooltip("Enemy damage scaling per level above player (0.95 = -5% per level difference)")]
        public float enemyDamageScalingPerLevel = 0.95f;

        [Header("Resonance Pulse (AOE)")]
        [Tooltip("Frequency tolerance for pulse bonus damage")]
        public float pulseFreqTolerance = 20f;
        
        [Tooltip("Damage multiplier on frequency match")]
        public float pulseFreqMatchBonus = 1.5f;

        [Header("Harmonic Strike (Directed)")]
        [Tooltip("Base damage multiplier for harmonic strike")]
        public float strikeBaseMultiplier = 5f;
        
        [Tooltip("Frequency tolerance for tight match")]
        public float strikeFreqTolerance = 10f;
        
        [Tooltip("Damage multiplier on tight frequency match")]
        public float strikeTightMatchBonus = 1.6f;

        [Header("Tuning Mini-Game")]
        [Tooltip("Frequency tolerance for easy tuning")]
        public float tuningToleranceEasy = 10f;
        
        [Tooltip("Frequency tolerance for hard tuning")]
        public float tuningToleranceHard = 5f;

        [Header("Combat Knockback/Hitstun")]
        [Tooltip("Knockback magnitude range min")]
        public float knockbackMagnitudeMin = 0.5f;
        
        [Tooltip("Knockback magnitude range max")]
        public float knockbackMagnitudeMax = 1.0f;
        
        [Tooltip("Knockback velocity multiplier")]
        public float knockbackVelocityMultiplier = 8f;
        
        [Tooltip("Hitstun duration range min (seconds)")]
        public float hitstunDurationMin = 0.15f;
        
        [Tooltip("Hitstun duration range max (seconds)")]
        public float hitstunDurationMax = 0.4f;
        
        [Tooltip("Pulse knockback quality fallback")]
        public float pulseKnockbackQuality = 0.5f;
        
        [Tooltip("Strike knockback quality fallback")]
        public float strikeKnockbackQuality = 0.6f;

        [Header("=== REWARDS & ECONOMY ===")]
        [Tooltip("RS reward for building restoration")]
        public int rsPerBuilding = 50;
        
        [Tooltip("RS reward per enemy defeated")]
        public int rsPerEnemy = 10;
        
        [Tooltip("XP reward for building restoration")]
        public int xpPerBuilding = 25;
        
        [Tooltip("XP reward per enemy defeated")]
        public int xpPerEnemy = 10;
        
        [Tooltip("XP reward for quest completion")]
        public int xpPerQuest = 100;
        
        [Tooltip("XP required to reach level 2")]
        public int level2XPRequirement = 150;

        [Header("=== INVENTORY SYSTEM ===")]
        [Tooltip("Maximum inventory slots (expandable)")]
        public int maxInventorySlots = 10;

        [Header("=== AUDIO LEVELS ===")]
        [Tooltip("Level-up SFX volume")]
        public float levelUpSFXVolume = 0.7f;
    }
}
