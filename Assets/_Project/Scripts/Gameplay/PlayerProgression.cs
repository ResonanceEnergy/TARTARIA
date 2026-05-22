using UnityEngine;
using System;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player Progression System — unified level/XP/stat allocation.
    /// Singleton instance. Replaces LevelUpSystem (eliminated duplicate).
    /// 
    /// Features:
    /// - XP/Leveling: exponential curve (100 * level^1.5), max level 50
    /// - Stat Allocation: 3 points per level across 5 stats
    /// - Stats: Vitality, Resonance, Strength, Agility, Attunement
    /// - Derived Stats: MaxHP, MaxRS, damage multipliers, dodge, movement speed
    /// - Respec: refund all allocated points (costs RS, pending economy integration)
    /// - Save Integration: ISaveDataProvider pattern (v17 modular extensibility)
    /// 
    /// Stats (5 base + scaling):
    /// - Vitality → +10 HP per point
    /// - Resonance → +5 RS per point, ability power +2%
    /// - Strength → melee damage +3%, carry weight +5 kg
    /// - Agility → dodge chance +1%, movement speed +2%
    /// - Attunement → magic damage +3%, RS regen +10%
    /// 
    /// Usage:
    /// - PlayerProgression.Instance.AddXP(50)
    /// - PlayerProgression.Instance.AllocateStat(StatType.Vitality, 1)
    /// - Subscribe to OnLevelUp/OnStatAllocated events
    /// 
    /// GDD refs: §06 (Player Progression), §09 (Combat Scaling)
    /// </summary>
    public class PlayerProgression : MonoBehaviour, ISaveDataProvider
    {
        public static PlayerProgression Instance { get; private set; }

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

        // Events
        public event Action<int> OnLevelUp;  // New level
        public event Action<int> OnXPGained;  // XP amount
        public event Action<StatType, int> OnStatAllocated;  // Stat type, new value

        // Properties
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
        public float AbilityPowerMultiplier => 1f + (resonance * 0.02f);
        public float MeleeDamageMultiplier => 1f + (strength * 0.03f);
        public int CarryWeight => 50 + (strength * 5);
        public float DodgeChance => 0.05f + (agility * 0.01f);
        public float MovementSpeedMultiplier => 1f + (agility * 0.02f);
        public float MagicDamageMultiplier => 1f + (attunement * 0.03f);
        public float RSRegenRate => 1f + (attunement * 0.1f);

        // XP progress (0-1 for UI)
        public float XPProgress
        {
            get
            {
                int xpRequired = GetXPRequiredForNextLevel();
                return Mathf.Clamp01((float)currentXP / xpRequired);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PlayerProgression");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PlayerProgression>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            // Register with SaveManager (ISaveDataProvider pattern)
            if (SaveManager.Instance != null)
                SaveManager.Instance.RegisterProvider(this);
        }
        
        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            
            // Unregister from SaveManager
            if (SaveManager.Instance != null)
                SaveManager.Instance.UnregisterProvider(this);
        }

        // ═══════════════════════════════════════════════════════════════
        // ISaveDataProvider Implementation (v17 modular save pattern)
        // ═══════════════════════════════════════════════════════════════

        public string GetProviderKey() => "PlayerProgression";

        public object GetSaveData()
        {
            return new PlayerProgressionData
            {
                level = currentLevel,
                xp = currentXP,
                statPoints = availableStatPoints,
                vitality = this.vitality,
                resonance = this.resonance,
                strength = this.strength,
                agility = this.agility,
                attunement = this.attunement
            };
        }

        public void RestoreSaveData(object data)
        {
            if (data == null)
            {
                // Fresh save — defaults already set
                Debug.Log("[PlayerProgression] No saved data — initialized to defaults");
                return;
            }

            // Provider receives JSON string from SaveManager
            if (data is string json)
            {
                try
                {
                    var ppd = JsonUtility.FromJson<PlayerProgressionData>(json);
                    currentLevel = Mathf.Max(1, ppd.level);
                    currentXP = Mathf.Max(0, ppd.xp);
                    availableStatPoints = Mathf.Max(0, ppd.statPoints);
                    vitality = Mathf.Max(5, ppd.vitality);
                    resonance = Mathf.Max(5, ppd.resonance);
                    strength = Mathf.Max(5, ppd.strength);
                    agility = Mathf.Max(5, ppd.agility);
                    attunement = Mathf.Max(5, ppd.attunement);

                    Debug.Log($"[PlayerProgression] Loaded: Level {currentLevel}, XP {currentXP}, " +
                              $"Stats V{vitality}/R{resonance}/S{strength}/A{agility}/At{attunement}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PlayerProgression] Failed to deserialize: {e.Message}");
                }
            }
        }

        void Start()
        {
            Debug.Log($"[PlayerProgression] Initialized at Level {currentLevel}, XP {currentXP}/{GetXPRequiredForNextLevel()}, {availableStatPoints} stat points available");
        }

        // === Public API ===

        /// <summary>
        /// Add experience points. Triggers level-up if threshold reached.
        /// </summary>
        public void AddXP(int amount, string source = "unknown")
        {
            if (currentLevel >= maxLevel)
            {
                Debug.Log("[PlayerProgression] Already at max level");
                return;
            }

            currentXP += amount;
            
            // Fire both legacy event and new GameEvents
            OnXPGained?.Invoke(amount);
            Core.GameEvents.RaiseXPGained(new Core.XPGainedEventArgs
            {
                amount = amount,
                source = source
            });

            Debug.Log($"[PlayerProgression] +{amount} XP from {source} (total {currentXP}/{GetXPRequiredForNextLevel()})");

            // Check for level up
            while (currentXP >= GetXPRequiredForNextLevel() && currentLevel < maxLevel)
            {
                LevelUp();
            }

            SaveManager.Instance?.MarkDirty();
        }

        void LevelUp()
        {
            // Roll over excess XP
            int xpRequired = GetXPRequiredForPreviousLevel();
            currentXP -= xpRequired;

            int oldLevel = currentLevel;
            currentLevel++;
            availableStatPoints += statPointsPerLevel;

            Debug.Log($"[PlayerProgression] LEVEL UP! → Level {currentLevel} (+{statPointsPerLevel} stat points, {availableStatPoints} total)");
            
            // Fire both legacy event and new GameEvents
            OnLevelUp?.Invoke(currentLevel);
            Core.GameEvents.RaiseLevelUp(new Core.LevelUpEventArgs
            {
                newLevel = currentLevel,
                oldLevel = oldLevel,
                maxHealthBonus = maxHealthBonus,
                damageBonus = damageBonus,
                movementSpeedBonus = movementSpeedBonus
            });

            // Play level-up VFX/SFX
            Audio.AudioManager.Instance?.PlaySFX2D("LevelUp", 0.7f);

            SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Allocate stat point(s) to a specific stat.
        /// </summary>
        public bool AllocateStat(StatType statType, int points = 1)
        {
            if (availableStatPoints < points)
            {
                Debug.LogWarning($"[PlayerProgression] Not enough stat points ({availableStatPoints} available, {points} requested)");
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

            Debug.Log($"[PlayerProgression] Allocated {points} point(s) to {statType} (now {GetStatValue(statType)}, {availableStatPoints} remaining)");

            OnStatAllocated?.Invoke(statType, GetStatValue(statType));

            SaveManager.Instance?.MarkDirty();

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
        /// Reset stats (costs RS, pending economy integration).
        /// </summary>
        public bool RespecStats(int rsCost = 100)
        {
            // TODO: Integrate with economy system to deduct RS cost
            Debug.Log($"[PlayerProgression] Respec requested (cost: {rsCost} RS) - economy integration pending");

            // Reset to base stats (5 each)
            int totalPointsSpent = (vitality - 5) + (resonance - 5) + (strength - 5) + (agility - 5) + (attunement - 5);

            vitality = 5;
            resonance = 5;
            strength = 5;
            agility = 5;
            attunement = 5;

            availableStatPoints += totalPointsSpent;

            Debug.Log($"[PlayerProgression] Respec complete, {totalPointsSpent} points refunded ({availableStatPoints} total available)");

            SaveManager.Instance?.MarkDirty();

            return true;
        }

        /// <summary>
        /// Set level directly (debug/testing).
        /// </summary>
        public void SetLevel(int level)
        {
            currentLevel = Mathf.Clamp(level, 1, maxLevel);
            currentXP = 0;
            Debug.Log($"[PlayerProgression] Set to level {currentLevel}");
            SaveManager.Instance?.MarkDirty();
        }

        // === Stat Type Enum ===

        public enum StatType : byte
        {
            Vitality = 0,
            Resonance = 1,
            Strength = 2,
            Agility = 3,
            Attunement = 4
        }
    }

    /// <summary>
    /// Serializable data class for PlayerProgression provider.
    /// MUST be serializable by JsonUtility (no generics, no null collections).
    /// </summary>
    [Serializable]
    public class PlayerProgressionData
    {
        public int level = 1;
        public int xp = 0;
        public int statPoints = 0;
        public int vitality = 5;
        public int resonance = 5;
        public int strength = 5;
        public int agility = 5;
        public int attunement = 5;
    }
}
