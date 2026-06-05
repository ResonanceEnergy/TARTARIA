using UnityEngine;
using System;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Core.Enums;
using Tartaria.Core.Validation;
using Tartaria.Localization;

namespace Tartaria.Data
{
    /// <summary>
    /// ScriptableObject representing a single skill node in a skill tree.
    /// Designer-editable in Unity Inspector — no code changes needed for balance tweaks.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNode_", menuName = "Tartaria/Skill Node", order = 100)]
    public class SkillNodeData : ScriptableObject, IValidatable, ILocalizable
    {
        [Header("Identity")]
        [Tooltip("Unique skill identifier (must match SkillId enum)")]
        public SkillId skillId = SkillId.None;

        [Tooltip("Tier/level within tree (1-5 typically)")]
        [Range(1, 5)]
        public int tier = 1;

        [Header("Costs & Requirements")]
        [Tooltip("Resonance Score cost to unlock (0 = progression blessing)")]
        public float rsCost = 50f;

        [Tooltip("Skills that must be unlocked first")]
        public List<SkillId> prerequisiteIds = new();

        [Header("Localization")]
        [Tooltip("Localization key for skill name (skills.name.{skillId})")]
        public LocalizationKey nameKey;

        [Tooltip("Localization key for skill description (skills.desc.{skillId})")]
        public LocalizationKey descKey;

        [Header("Legacy Display (Fallback)")]
        [Tooltip("Name shown in UI (used if nameKey is empty)")]
        public string displayName = "New Skill";

        [Tooltip("Description shown in tooltip (used if descKey is empty)")]
        [TextArea(3, 6)]
        public string description = "Skill description here.";

        [Header("Mechanics")]
        [Tooltip("What type of modifier this skill grants")]
        public SkillModifierType modifierType = SkillModifierType.TuningPrecision;

        [Tooltip("Numeric value of the modifier (0.1 = +10%, etc)")]
        public float modifierValue = 0.1f;

        [Header("Editor Tools")]
        [Tooltip("Quick preview of full skill identity")]
        [SerializeField, TextArea(2, 4)]
        private string _editorPreview = "";

        private void OnValidate()
        {
            // Auto-generate localization keys from skillId
            if (skillId != SkillId.None)
            {
                string skillIdStr = skillId.ToString().ToLower();
                if (!nameKey.IsValid)
                {
                    nameKey = new LocalizationKey("skills.name", skillIdStr);
                }
                if (!descKey.IsValid)
                {
                    descKey = new LocalizationKey("skills.desc", skillIdStr);
                }
            }

            // Auto-generate preview for designer convenience
            _editorPreview = $"[{skillId}] Tier {tier} | {rsCost} RS\n" +
                             $"{displayName}: +{modifierValue:P0} {modifierType}\n" +
                             $"Prereqs: {prerequisiteIds.Count}";
        }

        #region ILocalizable Implementation

        /// <summary>
        /// Returns all localization keys used by this skill.
        /// </summary>
        public LocalizationKey[] GetLocalizationKeys()
        {
            return new[] { nameKey, descKey };
        }

        /// <summary>
        /// Returns fallback text for a given key (legacy displayName/description).
        /// </summary>
        public string GetFallbackText(LocalizationKey key)
        {
            if (key == nameKey)
                return displayName;
            if (key == descKey)
                return description;
            return string.Empty;
        }

        /// <summary>
        /// Get localized skill name with fallback.
        /// </summary>
        public string GetLocalizedName()
        {
            if (nameKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(nameKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return displayName;
        }

        /// <summary>
        /// Get localized skill description with fallback.
        /// </summary>
        public string GetLocalizedDescription()
        {
            if (descKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(descKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return description;
        }

        #endregion

        /// <summary>
        /// Comprehensive validation for skill node data integrity.
        /// </summary>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // SkillId validation
            if (skillId == SkillId.None)
            {
                results.Add(ValidationResult.Error(
                    "skillId is set to None",
                    "All skill nodes must have a valid skillId",
                    "Set skillId to a valid enum value"
                ));
            }

            // Tier validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateRange(tier, 1, 5, "tier"));

            // RS cost validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(rsCost, "rsCost"));

            if (rsCost == 0)
            {
                results.Add(ValidationResult.Warning(
                    "rsCost is 0",
                    "Skills with 0 cost may be too easy to acquire",
                    "Consider setting a meaningful cost or documenting as progression blessing"
                ));
            }

            // Display name validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateDisplayName(displayName, "displayName"));

            // Description validation
            if (string.IsNullOrWhiteSpace(description) || description == "Skill description here.")
            {
                results.Add(ValidationResult.Warning(
                    "description is empty or default",
                    "Skill descriptions help players understand effects",
                    "Write a clear description of what this skill does"
                ));
            }

            // Modifier type validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateEnum(modifierType, "modifierType"));

            // Modifier value validation
            if (modifierValue == 0)
            {
                results.Add(ValidationResult.Error(
                    "modifierValue is 0",
                    "Skills with no effect serve no gameplay purpose",
                    "Set modifierValue to a non-zero value"
                ));
            }

            if (modifierValue < 0)
            {
                results.Add(ValidationResult.Warning(
                    $"modifierValue is negative: {modifierValue}",
                    "Negative modifiers create debuffs instead of buffs",
                    "Verify this is intentional or set to positive value"
                ));
            }

            // Prerequisites validation
            if (prerequisiteIds != null)
            {
                for (int i = 0; i < prerequisiteIds.Count; i++)
                {
                    if (prerequisiteIds[i] == SkillId.None)
                    {
                        results.Add(ValidationResult.Warning(
                            $"prerequisiteIds[{i}] is None",
                            "None is not a valid prerequisite",
                            "Remove None entries or assign valid SkillId"
                        ));
                    }

                    // Check for self-reference
                    if (prerequisiteIds[i] == skillId)
                    {
                        results.Add(ValidationResult.Error(
                            $"prerequisiteIds[{i}] references self",
                            "Circular dependencies cause infinite loops",
                            "Remove self-reference from prerequisites"
                        ));
                    }
                }
            }

            return results;
        }
    }
}
