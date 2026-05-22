using UnityEngine;
using System;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player Progression System — level, XP, stat bonuses on level-up.
    /// Singleton instance. Grants stat increases every 5 levels.
    /// Max level 50 (accommodates full 13-Moon campaign + replayability).
    /// </summary>
    public class PlayerProgression : MonoBehaviour
    {
        public static PlayerProgression Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] int currentLevel = 1;
        [SerializeField] float currentXP = 0f;

        [Header("Progression Config")]
        [SerializeField] int maxLevel = 50;
        [SerializeField] float baseXPRequired = 100f;
        [SerializeField] float xpScaling = 1.15f;  // XP required increases 15% per level

        [Header("Stat Bonuses (per 5 levels)")]
        [SerializeField] float maxHealthBonus = 25f;
        [SerializeField] float damageBonus = 5f;
        [SerializeField] float movementSpeedBonus = 0.1f;

        public event Action<int> OnLevelUp;
        public event Action<float> OnXPGained;

        public int CurrentLevel => currentLevel;
        public float CurrentXP => currentXP;
        public float XPToNextLevel => CalculateXPForLevel(currentLevel + 1);
        public float XPProgress => currentXP / XPToNextLevel;

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
        }

        void Start()
        {
            LoadProgressionFromSave();
        }

        // === Public API ===

        public void AddXP(float amount)
        {
            if (currentLevel >= maxLevel) return;  // Max level reached

            currentXP += amount;
            OnXPGained?.Invoke(amount);

            Debug.Log($"[PlayerProgression] +{amount} XP (Total: {currentXP}/{XPToNextLevel})");

            // Check for level up
            while (currentXP >= XPToNextLevel && currentLevel < maxLevel)
            {
                LevelUp();
            }
        }

        void LevelUp()
        {
            currentLevel++;
            currentXP -= XPToNextLevel;

            Debug.Log($"[PlayerProgression] LEVEL UP! → Level {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);

            // Apply stat bonuses every 5 levels
            if (currentLevel % 5 == 0)
            {
                ApplyStatBonuses();
            }

            // Play level-up VFX/SFX
            Audio.AudioManager.Instance?.PlaySFX2D("LevelUp", 0.7f);

            // Show UI notification (GameEvents.OnLevelUp event for UI decoupling)
            Debug.Log($"[PlayerProgression] LEVEL UP! You are now level {currentLevel}");

            SaveProgression();
        }

        void ApplyStatBonuses()
        {
            var playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Note: PlayerHealth.IncreaseMaxHealth() method pending
                Debug.Log($"[PlayerProgression] +{maxHealthBonus} Max Health (API pending)");
            }

            // Note: Stat bonus system pending (damage/speed multipliers)
            // Apply movement speed bonus to PlayerInputHandler
            var inputHandler = FindFirstObjectByType<Input.PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SpeedMultiplier += movementSpeedBonus;
                Debug.Log($"[PlayerProgression] +{movementSpeedBonus} Speed Multiplier");
            }

            Debug.Log($"[PlayerProgression] Stat bonuses applied at level {currentLevel}");
        }

        float CalculateXPForLevel(int level)
        {
            // Exponential scaling: XP = base * (scaling ^ (level - 1))
            return baseXPRequired * Mathf.Pow(xpScaling, level - 1);
        }

        public void SetLevel(int level)
        {
            currentLevel = Mathf.Clamp(level, 1, maxLevel);
            currentXP = 0f;
            Debug.Log($"[PlayerProgression] Set to level {currentLevel}");
        }

        void LoadProgressionFromSave()
        {
            // Note: SaveManager integration pending (save schema migration)
            Debug.Log("[PlayerProgression] Load from save (stub)");
        }

        public void SaveProgression()
        {
            // Note: SaveManager integration pending (save schema migration)
            Debug.Log("[PlayerProgression] Save progression (stub)");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
