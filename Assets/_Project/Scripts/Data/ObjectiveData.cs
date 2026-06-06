using UnityEngine;
using Tartaria.Core;
using Tartaria.Localization;

namespace Tartaria.Data
{
    /// <summary>
    /// Quest objective definition - can be created as sub-asset of QuestData.
    /// Represents a single objective within a quest with progress tracking.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Data/Quest Objective", order = 100)]
    public class ObjectiveData : ScriptableObject, ILocalizable
    {
        [Header("Identity")]
        [Tooltip("Unique objective ID within quest scope")]
        public string objectiveId;

        [Header("Localization")]
        [Tooltip("Localization key for objective text (quests.objective.{objectiveId})")]
        public LocalizationKey textKey;

        [Header("Description")]
        [TextArea(2, 4)]
        [Tooltip("Player-facing description shown in quest log (used if textKey is empty)")]
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

        private void OnValidate()
        {
            // Auto-generate localization key from objectiveId
            if (!string.IsNullOrWhiteSpace(objectiveId))
            {
                if (!textKey.IsValid)
                {
                    textKey = new LocalizationKey("quests.objective", objectiveId);
                }
            }
        }

        #region ILocalizable Implementation

        public LocalizationKey[] GetLocalizationKeys()
        {
            return new[] { textKey };
        }

        public string GetFallbackText(LocalizationKey key)
        {
            if (key == textKey)
                return description;
            return string.Empty;
        }

        public string GetLocalizedDescription()
        {
            if (textKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(textKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return description;
        }

        #endregion

        /// <summary>
        /// Convert to runtime QuestObjective struct for backwards compatibility.
        /// Uses localized description if available.
        /// </summary>
        public QuestObjective ToRuntimeObjective()
        {
            return new QuestObjective
            {
                description = GetLocalizedDescription(),
                type = this.targetType,
                targetId = this.targetId,
                targetCount = this.targetCount
            };
        }
    }
}
