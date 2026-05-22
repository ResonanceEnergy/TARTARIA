using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data
{
    /// <summary>
    /// Quest objective definition - can be created as sub-asset of QuestData.
    /// Represents a single objective within a quest with progress tracking.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Data/Quest Objective", order = 100)]
    public class ObjectiveData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique objective ID within quest scope")]
        public string objectiveId;

        [Header("Description")]
        [TextArea(2, 4)]
        [Tooltip("Player-facing description shown in quest log")]
        public string description;

        [Header("Type & Target")]
        [Tooltip("Category of objective (Restore, Defeat, Collect, etc.)")]
        public QuestObjectiveType targetType;

        [Tooltip("Specific target identifier (building ID, enemy type, item name, etc.)")]
        public string targetId;

        [Tooltip("Number of times target action must be completed")]
        public int targetCount = 1;

        [Header("Optional")]
        [Tooltip("If true, objective is not required for quest completion")]
        public bool isOptional;

        [Tooltip("If true, objective is hidden until revealed by progression")]
        public bool isHidden;

        /// <summary>
        /// Convert to runtime QuestObjective struct for backwards compatibility.
        /// </summary>
        public QuestObjective ToRuntimeObjective()
        {
            return new QuestObjective
            {
                description = this.description,
                type = this.targetType,
                targetId = this.targetId,
                targetCount = this.targetCount
            };
        }
    }
}
