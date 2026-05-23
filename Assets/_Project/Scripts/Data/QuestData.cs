using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data
{
    /// <summary>
    /// Enhanced quest definition with full campaign integration.
    /// Extends QuestDefinition with moonId, category, prerequisites, XP, and items.
    /// Supports branching quest chains and prerequisite validation.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Data/Quest Data", order = 99)]
    public class QuestData : QuestDefinition, UnityEngine.ISerializationCallbackReceiver
    {
        [Header("Schema Version")]
        [SerializeField] int schemaVersion = Tartaria.Core.DataSchemaVersion.CURRENT_QUEST;

        [Header("Campaign Integration")]
        [Tooltip("Moon number (1-13, 0 for meta/hub quests)")]
        [Range(0, 13)]
        public int moonId;

        [Tooltip("Quest category for UI filtering")]
        public QuestCategory category = QuestCategory.Main;

        [Header("Prerequisites")]
        [Tooltip("Quest IDs that must be completed before this quest unlocks")]
        public string[] prerequisiteQuestIds = System.Array.Empty<string>();

        [Tooltip("Minimum RS threshold to unlock this quest")]
        public float prerequisiteRS;

        [Tooltip("Minimum player level to unlock this quest")]
        public int prerequisiteLevel;

        [Header("Enhanced Rewards")]
        [Tooltip("Experience points awarded on completion")]
        public int xpReward;

        [Tooltip("Item IDs awarded on completion")]
        public string[] itemRewards = System.Array.Empty<string>();

        [Tooltip("Unlock IDs triggered on completion (abilities, areas, etc.)")]
        public string[] unlockRewards = System.Array.Empty<string>();

        [Header("Data-Driven Objectives")]
        [Tooltip("Enhanced objective sub-assets (use if defined, else fall back to base objectives array)")]
        public ObjectiveData[] objectiveData = System.Array.Empty<ObjectiveData>();

        [Header("Quest Flow")]
        [Tooltip("If true, quest auto-activates on prerequisite completion")]
        public bool autoActivateOnPrerequisites = true;

        [Tooltip("If true, quest can be abandoned and restarted")]
        public bool canAbandon;

        [Tooltip("If true, quest is repeatable after completion")]
        public bool isRepeatable;

        /// <summary>
        /// Check if all prerequisites are satisfied.
        /// </summary>
        public bool ArePrerequisitesMet(float currentRS, int currentLevel, System.Func<string, bool> isQuestComplete)
        {
            // RS check
            if (currentRS < prerequisiteRS)
                return false;

            // Level check
            if (currentLevel < prerequisiteLevel)
                return false;

            // Prerequisite quest check
            if (prerequisiteQuestIds != null)
            {
                foreach (var prereqId in prerequisiteQuestIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    if (!isQuestComplete(prereqId))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get runtime objectives array (enhanced or fallback to base).
        /// </summary>
        public QuestObjective[] GetRuntimeObjectives()
        {
            // Use enhanced ObjectiveData if available
            if (objectiveData != null && objectiveData.Length > 0)
            {
                var result = new QuestObjective[objectiveData.Length];
                for (int i = 0; i < objectiveData.Length; i++)
                {
                    if (objectiveData[i] != null)
                        result[i] = objectiveData[i].ToRuntimeObjective();
                }
                return result;
            }

            // Fallback to base objectives array
            return objectives ?? System.Array.Empty<QuestObjective>();
        }

        #region Schema Migration (ISerializationCallbackReceiver)

        /// <summary>
        /// Called before Unity serializes this object.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // No action needed
        }

        /// <summary>
        /// Called after Unity deserializes this object.
        /// Auto-migrates to latest schema version if needed.
        /// </summary>
        public void OnAfterDeserialize()
        {
            int currentVersion = Tartaria.Core.DataSchemaVersion.CURRENT_QUEST;
            
            if (schemaVersion < currentVersion)
            {
                Debug.Log($"[QuestData] {questId}: Auto-migrating from v{schemaVersion} to v{currentVersion}");
                // Future: Apply migration logic here when v2 is released
                schemaVersion = currentVersion;
            }
        }

        #endregion
    }

    /// <summary>
    /// Quest categories for UI organization.
    /// </summary>
    public enum QuestCategory
    {
        Main,           // Critical path main story quests
        Side,           // Optional side quests
        Companion,      // Companion relationship quests
        Exploration,    // Discovery and exploration quests
        Combat,         // Combat-focused challenges
        Collection,     // Gather/collect quests
        Tutorial,       // FTUE and tutorial quests
        Repeatable,     // Daily/repeatable quests
        Hidden,         // Secret discoverable quests
        Event           // Time-limited event quests
    }
}
