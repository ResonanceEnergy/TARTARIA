using UnityEngine;

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
    public class EquipmentItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for save/load and inventory tracking")]
        public string itemID;

        [Tooltip("Display name shown in UI")]
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
        [Tooltip("Lore text or item description")]
        public string description;

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
