using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core.Validation;
using Tartaria.Localization;

namespace Tartaria.Data
{
    /// <summary>
    /// EquipmentItemData — ScriptableObject for equipment items.
    /// Replaces serializable EquipmentItem class with editor-friendly asset-based system.
    ///
    /// Features:
    /// - Create equipment assets in Project window
    /// - Define stat bonuses (STR/AGI/VIT/RES/ATT/ARM)
    /// - Assign icons for UI display
    /// - Special effects (passive bonuses, procs)
    ///
    /// Usage:
    /// - Create via: Assets > Create > Tartaria > Equipment Item
    /// - Reference in EquipmentSlotManager
    /// - Equip via InventorySystem integration
    ///
    /// GDD refs: §07 (Equipment System), §06 (Character Stats)
    /// </summary>
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Tartaria/Equipment Item", order = 200)]
    public class EquipmentItemData : ScriptableObject, IValidatable, ILocalizable
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for save/load and inventory tracking")]
        public string itemID;

        [Header("Localization")]
        [Tooltip("Localization key for equipment name (equipment.name.{itemID})")]
        public LocalizationKey nameKey;

        [Tooltip("Localization key for equipment description (equipment.desc.{itemID})")]
        public LocalizationKey descKey;

        [Header("Legacy Display (Fallback)")]
        [Tooltip("Display name shown in UI (used if nameKey is empty)")]
        public string itemName;

        [Tooltip("Which equipment slot this item occupies")]
        public EquipSlot slot;

        [Header("Visuals")]
        [Tooltip("Icon displayed in inventory/character UI")]
        public Sprite icon;

        [Tooltip("3D model shown when equipped (optional)")]
        public GameObject meshPrefab;

        [Header("Stat Bonuses")]
        [Tooltip("Physical damage and carrying capacity")]
        public int strengthBonus;

        [Tooltip("Attack speed and dodge chance")]
        public int agilityBonus;

        [Tooltip("Max health and health regen")]
        public int vitalityBonus;

        [Tooltip("Resonance Stone capacity and regen rate")]
        public int resonanceBonus;

        [Tooltip("Magic damage and ability power")]
        public int attunementBonus;

        [Tooltip("Physical damage reduction (flat reduction)")]
        public int armorValue;

        [Header("Special Effects")]
        [Tooltip("Passive effects and procs (e.g., '+5% crit chance', '+10% move speed', 'Reflects 10% damage')")]
        public string[] specialEffects;

        [Header("Description")]
        [TextArea(2, 4)]
        [Tooltip("Lore text or item description (used if descKey is empty)")]
        public string description;

        private void OnValidate()
        {
            // Auto-generate localization keys from itemID
            if (!string.IsNullOrWhiteSpace(itemID))
            {
                if (!nameKey.IsValid)
                {
                    nameKey = new LocalizationKey("equipment.name", itemID);
                }
                if (!descKey.IsValid)
                {
                    descKey = new LocalizationKey("equipment.desc", itemID);
                }
            }
        }

        #region ILocalizable Implementation

        public LocalizationKey[] GetLocalizationKeys()
        {
            return new[] { nameKey, descKey };
        }

        public string GetFallbackText(LocalizationKey key)
        {
            if (key == nameKey)
                return itemName;
            if (key == descKey)
                return description;
            return string.Empty;
        }

        public string GetLocalizedName()
        {
            if (nameKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(nameKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return itemName;
        }

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
        /// Get formatted tooltip for UI display.
        /// </summary>
        public string GetTooltip()
        {
            var tooltip = $"<b>{itemName}</b>\n";
            tooltip += $"<i>{slot}</i>\n\n";

            if (strengthBonus > 0) tooltip += $"+{strengthBonus} Strength\n";
            if (agilityBonus > 0) tooltip += $"+{agilityBonus} Agility\n";
            if (vitalityBonus > 0) tooltip += $"+{vitalityBonus} Vitality\n";
            if (resonanceBonus > 0) tooltip += $"+{resonanceBonus} Resonance\n";
            if (attunementBonus > 0) tooltip += $"+{attunementBonus} Attunement\n";
            if (armorValue > 0) tooltip += $"+{armorValue} Armor\n";

            if (specialEffects != null && specialEffects.Length > 0)
            {
                tooltip += "\n<color=#FFD700>Special Effects:</color>\n";
                foreach (var effect in specialEffects)
                {
                    if (!string.IsNullOrEmpty(effect))
                        tooltip += $"• {effect}\n";
                }
            }

            if (!string.IsNullOrEmpty(description))
            {
                tooltip += $"\n<color=#AAAAAA>{description}</color>";
            }

            return tooltip;
        }

        /// <summary>
        /// Comprehensive validation for equipment data integrity.
        /// </summary>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // ID validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateID(itemID, "itemID"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateIDFormat(itemID, "itemID"));

            // Name validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateDisplayName(itemName, "itemName"));

            // Icon validation
            if (icon == null)
            {
                results.Add(ValidationResult.Error(
                    "icon is null",
                    "Equipment must have icons for inventory display",
                    "Assign a Sprite to the icon field"
                ));
            }

            // Slot validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateEnum(slot, "slot"));

            // Stat validation (all should be non-negative)
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(strengthBonus, "strengthBonus"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(agilityBonus, "agilityBonus"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(vitalityBonus, "vitalityBonus"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(resonanceBonus, "resonanceBonus"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(attunementBonus, "attunementBonus"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(armorValue, "armorValue"));

            // Check if equipment has any stats
            bool hasAnyStats = strengthBonus > 0 || agilityBonus > 0 || vitalityBonus > 0 ||
                              resonanceBonus > 0 || attunementBonus > 0 || armorValue > 0;

            if (!hasAnyStats && (specialEffects == null || specialEffects.Length == 0))
            {
                results.Add(ValidationResult.Warning(
                    "Equipment has no stats or special effects",
                    "Equipment with no bonuses serves no gameplay purpose",
                    "Add stat bonuses or special effects"
                ));
            }

            // Mesh prefab validation (optional)
            if (meshPrefab == null)
            {
                results.Add(ValidationResult.Info(
                    "meshPrefab is not assigned",
                    "Equipment without mesh prefabs won't be visible when equipped",
                    "Assign a prefab to the meshPrefab field if equipment should be visible"
                ));
            }

            // Special effects validation
            if (specialEffects != null)
            {
                for (int i = 0; i < specialEffects.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(specialEffects[i]))
                    {
                        results.Add(ValidationResult.Warning(
                            $"specialEffects[{i}] is empty",
                            "Empty special effect entries clutter the inspector",
                            "Remove empty entries from specialEffects array"
                        ));
                    }
                }
            }

            return results;
        }
    }

    /// <summary>
    /// Equipment slot types.
    /// Moved from EquipmentSlotManager for shared access.
    /// </summary>
    public enum EquipSlot : byte
    {
        Weapon = 0,
        Armor = 1,
        Helmet = 2,
        Gloves = 3,
        Boots = 4,
        Accessory = 5
    }
}
