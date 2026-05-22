using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Skill Tree Save Data Provider — demonstrates ISaveDataProvider pattern.
    /// 
    /// Design:
    ///   - Manages unlocked skills and skill point allocation
    ///   - Registers with SaveManager in Awake (automatic discovery)
    ///   - Serializes to SkillTreeData POCO (plain C# class)
    ///   - No direct SaveData dependency — fully modular
    /// 
    /// Skills organized in 3 trees:
    ///   - Combat: damage, health, crit chance
    ///   - Resonance: RS multipliers, shard efficiency
    ///   - Exploration: movement speed, vision range, interaction radius
    /// 
    /// Example extensibility: adding new skill trees requires NO SaveData changes.
    /// </summary>
    public class SkillTreeSaveDataProvider : MonoBehaviour, ISaveDataProvider
    {
        public static SkillTreeSaveDataProvider Instance { get; private set; }

        [Header("Skill Points")]
        [SerializeField] int availableSkillPoints = 0;
        [SerializeField] int totalSkillPointsEarned = 0;

        [Header("Runtime State")]
        readonly HashSet<string> _unlockedSkills = new();
        readonly Dictionary<string, int> _skillLevels = new(); // skill_id -> level (1-5)

        public event Action<string> OnSkillUnlocked;
        public event Action<int> OnSkillPointsChanged;

        public int AvailableSkillPoints => availableSkillPoints;
        public int TotalSkillPointsEarned => totalSkillPointsEarned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("SkillTreeSaveDataProvider");
            DontDestroyOnLoad(go);
            go.AddComponent<SkillTreeSaveDataProvider>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // Register with SaveManager (provider pattern)
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
        // ISaveDataProvider Implementation
        // ═══════════════════════════════════════════════════════════════

        public string GetProviderKey() => "SkillTree";

        public object GetSaveData()
        {
            return new SkillTreeData
            {
                availablePoints = availableSkillPoints,
                totalPointsEarned = totalSkillPointsEarned,
                unlockedSkills = _unlockedSkills.ToArray(),
                skillLevelKeys = _skillLevels.Keys.ToArray(),
                skillLevelValues = _skillLevels.Values.ToArray()
            };
        }

        public void RestoreSaveData(object data)
        {
            _unlockedSkills.Clear();
            _skillLevels.Clear();

            if (data == null)
            {
                // Fresh save — reset to defaults
                availableSkillPoints = 0;
                totalSkillPointsEarned = 0;
                Debug.Log("[SkillTree] No saved data — initialized to defaults");
                return;
            }

            // Provider receives JSON string from SaveManager — deserialize it
            if (data is string json)
            {
                try
                {
                    var std = JsonUtility.FromJson<SkillTreeData>(json);
                    availableSkillPoints = std.availablePoints;
                    totalSkillPointsEarned = std.totalPointsEarned;

                    // Restore unlocked skills
                    if (std.unlockedSkills != null)
                    {
                        foreach (var skill in std.unlockedSkills)
                            _unlockedSkills.Add(skill);
                    }

                    // Restore skill levels
                    if (std.skillLevelKeys != null && std.skillLevelValues != null)
                    {
                        int count = Mathf.Min(std.skillLevelKeys.Length, std.skillLevelValues.Length);
                        for (int i = 0; i < count; i++)
                        {
                            _skillLevels[std.skillLevelKeys[i]] = std.skillLevelValues[i];
                        }
                    }

                    Debug.Log($"[SkillTree] Loaded {_unlockedSkills.Count} skills, {availableSkillPoints} points available");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SkillTree] Failed to deserialize: {e.Message}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Public API
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Awards skill points (on level-up, quest completion, etc.)</summary>
        public void AwardSkillPoints(int points)
        {
            if (points <= 0) return;

            availableSkillPoints += points;
            totalSkillPointsEarned += points;

            Debug.Log($"[SkillTree] Awarded {points} skill points (now {availableSkillPoints} available)");
            OnSkillPointsChanged?.Invoke(availableSkillPoints);

            SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Unlock a skill (costs 1 point per level)</summary>
        public bool UnlockSkill(string skillId, int cost = 1)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            if (availableSkillPoints < cost) return false;

            availableSkillPoints -= cost;

            if (!_unlockedSkills.Contains(skillId))
            {
                _unlockedSkills.Add(skillId);
                _skillLevels[skillId] = 1;
            }
            else
            {
                // Level up existing skill (max 5)
                int currentLevel = _skillLevels.GetValueOrDefault(skillId, 1);
                if (currentLevel >= 5)
                {
                    Debug.LogWarning($"[SkillTree] Skill {skillId} already at max level");
                    availableSkillPoints += cost; // refund
                    return false;
                }
                _skillLevels[skillId] = currentLevel + 1;
            }

            Debug.Log($"[SkillTree] Unlocked/leveled {skillId} to level {_skillLevels[skillId]}");
            OnSkillUnlocked?.Invoke(skillId);
            OnSkillPointsChanged?.Invoke(availableSkillPoints);

            SaveManager.Instance?.MarkDirty();
            return true;
        }

        /// <summary>Check if skill is unlocked</summary>
        public bool IsSkillUnlocked(string skillId)
        {
            return _unlockedSkills.Contains(skillId);
        }

        /// <summary>Get skill level (0 if not unlocked, 1-5 if unlocked)</summary>
        public int GetSkillLevel(string skillId)
        {
            return _skillLevels.GetValueOrDefault(skillId, 0);
        }

        /// <summary>Get all unlocked skill IDs</summary>
        public string[] GetUnlockedSkills()
        {
            return _unlockedSkills.ToArray();
        }
    }

    /// <summary>
    /// Serializable data class for SkillTree provider.
    /// MUST be serializable by JsonUtility (no generics, no null collections).
    /// </summary>
    [Serializable]
    public class SkillTreeData
    {
        public int availablePoints;
        public int totalPointsEarned;
        public string[] unlockedSkills = Array.Empty<string>();
        public string[] skillLevelKeys = Array.Empty<string>();
        public int[] skillLevelValues = Array.Empty<int>();
    }
}
