using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core.Validation;
using Tartaria.Localization;

namespace Tartaria.Core
{
    [CreateAssetMenu(menuName = "Tartaria/Quest Definition")]
    public class QuestDefinition : ScriptableObject, IValidatable, ILocalizable
    {
        [Header("Identity")]
        public string questId;

        [Header("Localization")]
        [Tooltip("Localization key for quest title (quests.title.{questId})")]
        public LocalizationKey titleKey;

        [Tooltip("Localization key for quest description (quests.desc.{questId})")]
        public LocalizationKey descKey;

        [Header("Legacy Text (Fallback)")]
        public string displayName;
        [TextArea(2, 5)]
        public string description;

        [Header("Type")]
        public bool isMainQuest;
        public bool autoActivate;
        public float rsRequirement;

        [Header("Objectives")]
        public QuestObjective[] objectives;

        [Header("Rewards")]
        public float rsReward;

        [Header("Chain")]
        public string[] followUpQuestIds = System.Array.Empty<string>();

        /// <summary>
        /// Comprehensive validation for quest data integrity.
        /// </summary>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // ID validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateID(questId, "questId"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateIDFormat(questId, "questId"));

            // Display name validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateDisplayName(displayName, "displayName"));

            // Description validation
            if (string.IsNullOrWhiteSpace(description))
            {
                results.Add(ValidationResult.Warning(
                    "description is empty",
                    "Quest descriptions help players understand objectives",
                    "Add a description of the quest goals"
                ));
            }

            // Objectives validation
            if (objectives == null || objectives.Length == 0)
            {
                results.Add(ValidationResult.Error(
                    "objectives array is empty",
                    "Quests must have at least one objective",
                    "Add QuestObjective entries to objectives array"
                ));
            }
            else
            {
                // Validate each objective
                for (int i = 0; i < objectives.Length; i++)
                {
                    var obj = objectives[i];
                    if (obj == null)
                    {
                        results.Add(ValidationResult.Error(
                            $"objectives[{i}] is null",
                            "Null objectives cause runtime crashes",
                            $"Remove null entry or assign a valid QuestObjective at index {i}"
                        ));
                        continue;
                    }

                    // Validate objective description
                    if (string.IsNullOrWhiteSpace(obj.description))
                    {
                        results.Add(ValidationResult.Warning(
                            $"objectives[{i}].description is empty",
                            "Objective descriptions improve quest clarity",
                            "Add a description string to this QuestObjective"
                        ));
                    }

                    // Validate target count
                    if (obj.targetCount <= 0)
                    {
                        results.Add(ValidationResult.Error(
                            $"objectives[{i}].targetCount must be > 0 (current: {obj.targetCount})",
                            "Zero or negative target counts cause completion bugs",
                            $"Set targetCount to 1 or higher for objective {i}"
                        ));
                    }
                }
            }

            // RS requirement validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(rsRequirement, "rsRequirement"));

            // RS reward validation
            if (rsReward < 0)
            {
                results.Add(ValidationResult.Warning(
                    $"rsReward is negative: {rsReward}",
                    "Negative rewards may confuse players",
                    "Use 0 for no reward or positive values for rewards"
                ));
            }

            // Follow-up quest validation
            if (followUpQuestIds != null)
            {
                for (int i = 0; i < followUpQuestIds.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(followUpQuestIds[i]))
                    {
                        results.Add(ValidationResult.Warning(
                            $"followUpQuestIds[{i}] is empty",
                            "Empty follow-up quest IDs cause lookup failures",
                            $"Remove empty entry or assign valid quest ID at index {i}"
                        ));
                    }

                    // Check for circular dependency
                    if (followUpQuestIds[i] == questId)
                    {
                        results.Add(ValidationResult.Error(
                            $"followUpQuestIds[{i}] references self",
                            "Circular quest dependencies cause infinite loops",
                            "Remove self-reference from follow-up quests"
                        ));
                    }
                }
            }

            return results;
        }

        #region ILocalizable Implementation

        /// <summary>
        /// Returns all localization keys used by this quest.
        /// Used by editor extraction tools to generate string tables.
        /// </summary>
        public virtual LocalizationKey[] GetLocalizationKeys()
        {
            var keys = new List<LocalizationKey> { titleKey, descKey };

            // Add objective description keys if objectives exist
            if (objectives != null)
            {
                foreach (var obj in objectives)
                {
                    if (obj != null)
                    {
                        var objKey = new LocalizationKey("quests.objective", $"{questId}_{obj.description?.Replace(" ", "_").ToLower()}");
                        keys.Add(objKey);
                    }
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Returns fallback text for a given key (legacy displayName/description).
        /// </summary>
        public virtual string GetFallbackText(LocalizationKey key)
        {
            if (key == titleKey)
                return displayName;
            if (key == descKey)
                return description;
            return string.Empty;
        }

        #endregion

        #region Localized Text Accessors

        /// <summary>
        /// Get localized quest title with fallback to legacy displayName field.
        /// </summary>
        public string GetLocalizedTitle()
        {
            // LocalizationManager.Instance disabled (Phase 8) — assembly visibility issue
            // TODO: Investigate why Core can't see Localization.Instance despite asmdef reference
            // if (titleKey.IsValid && LocalizationManager.Instance != null)
            // {
            //     string localized = LocalizationManager.Instance.GetText(titleKey);
            //     if (!localized.StartsWith("[MISSING:"))
            //         return localized;
            // }
            return displayName;
        }

        /// <summary>
        /// Get localized quest description with fallback to legacy description field.
        /// </summary>
        public string GetLocalizedDescription()
        {
            // LocalizationManager.Instance disabled (Phase 8) — assembly visibility issue
            // if (descKey.IsValid && LocalizationManager.Instance != null)
            // {
            //     string localized = LocalizationManager.Instance.GetText(descKey);
            //     if (!localized.StartsWith("[MISSING:"))
            //         return localized;
            // }
            return description;
        }

        #endregion

        /// <summary>
        /// Auto-generate localization keys from questId in editor.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(questId))
            {
                if (!titleKey.IsValid)
                {
                    titleKey = new LocalizationKey("quests.title", questId);
                }
                if (!descKey.IsValid)
                {
                    descKey = new LocalizationKey("quests.desc", questId);
                }
            }
        }
    }
}
