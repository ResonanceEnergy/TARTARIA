using UnityEngine;
using System.Collections.Generic;
using Tartaria.Data.Validation;
using Tartaria.Localization;

namespace Tartaria.Data
{
    /// <summary>
    /// Item Data — ScriptableObject definition for a single item.
    /// Stores all item metadata: ID, name, description, icon, stats, category.
    /// 
    /// Localization Support:
    /// - nameKey: LocalizationKey for translated display name
    /// - descKey: LocalizationKey for translated description
    /// - Legacy displayName/description fields maintained as fallback
    /// 
    /// Create assets via: Assets → Create → Tartaria → Item Data
    /// Place in: Assets/_Project/Resources/Items/
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Tartaria/Item Data", order = 100)]
    public class ItemData : ScriptableObject, IValidatable, ILocalizable, UnityEngine.ISerializationCallbackReceiver
    {
        [Header("Schema Version")]
        [SerializeField] int schemaVersion = Tartaria.Save.SchemaVersion.CURRENT_ITEM;

        [Header("Identity")]
        [Tooltip("Unique identifier (e.g., 'aether_shard', 'golem_core')")]
        public string itemID;
        
        [Header("Localization")]
        [Tooltip("Localization key for display name (items.name.{itemID})")]
        public LocalizationKey nameKey;
        
        [Tooltip("Localization key for description (items.desc.{itemID})")]
        public LocalizationKey descKey;
        
        [Header("Legacy Text (Fallback)")]
        [Tooltip("Display name shown in UI (used if nameKey is empty)")]
        public string displayName;
        
        [TextArea(3, 6)]
        [Tooltip("Description shown in tooltips (used if descKey is empty)")]
        public string description;

        [Header("Visuals")]
        [Tooltip("Icon sprite for UI display")]
        public Sprite icon;

        [Header("Properties")]
        [Tooltip("Maximum stack size (1 = non-stackable)")]
        [Range(1, 999)]
        public int stackSize = 1;
        
        [Tooltip("Item category for organization")]
        public ItemCategory category = ItemCategory.Material;
        
        [Tooltip("Rarity tier for color coding")]
        public ItemRarity rarity = ItemRarity.Common;
        
        [Tooltip("Item weight (kg) for encumbrance systems")]
        [Range(0f, 100f)]
        public float weight = 0.1f;
        
        [Tooltip("Base value (Resonance Shards) for vendor prices")]
        [Range(0, 10000)]
        public int value = 10;

        [Header("Optional")]
        [Tooltip("Prefab to spawn when item is dropped in world")]
        public GameObject worldPrefab;
        
        [Tooltip("Custom data for item-specific behavior (JSON, etc.)")]
        [TextArea(2, 4)]
        public string customData;

        /// <summary>
        /// Validates item data on asset creation/edit.
        /// Auto-generates localization keys from itemID.
        /// </summary>
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemID))
            {
                Debug.LogWarning($"[ItemData] {name}: itemID is empty!");
            }
            else
            {
                // Auto-generate localization keys from itemID if not set
                if (!nameKey.IsValid)
                {
                    nameKey = new LocalizationKey("items.name", itemID);
                }
                if (!descKey.IsValid)
                {
                    descKey = new LocalizationKey("items.desc", itemID);
                }
            }
            
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name; // Default to asset name
            }
            
            if (stackSize < 1)
            {
                stackSize = 1;
            }
        }

        /// <summary>
        /// Comprehensive validation for runtime data integrity.
        /// </summary>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // ID validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateID(itemID, "itemID"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateIDFormat(itemID, "itemID"));

            // Display name validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateDisplayName(displayName));

            // Icon validation (critical for UI)
            if (icon == null)
            {
                results.Add(ValidationResult.Error(
                    "icon is null",
                    "All items must have an icon for inventory display",
                    "Assign a Sprite to the icon field"
                ));
            }

            // Stack size validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateRange(stackSize, 1, 999, "stackSize"));

            // Weight validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(weight, "weight"));

            // Value validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateNonNegative(value, "value"));

            // Category validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateEnum(category, "category"));

            // Rarity validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateEnum(rarity, "rarity"));

            // World prefab validation (optional but recommended)
            if (worldPrefab == null)
            {
                results.Add(ValidationResult.Warning(
                    "worldPrefab is not assigned",
                    "Items without world prefabs cannot be dropped in the world",
                    "Assign a GameObject prefab for world representation"
                ));
            }

            // Description validation (informational)
            if (string.IsNullOrWhiteSpace(description))
            {
                results.Add(ValidationResult.Info(
                    "description is empty",
                    "Item descriptions improve player understanding"
                ));
            }

            return results;
        }

        #region ILocalizable Implementation

        /// <summary>
        /// Returns all localization keys used by this item.
        /// Used by editor extraction tools to generate string tables.
        /// </summary>
        public LocalizationKey[] GetLocalizationKeys()
        {
            return new[] { nameKey, descKey };
        }

        /// <summary>
        /// Returns fallback text for a given key (legacy displayName/description).
        /// Used when localized text is not available in the current language.
        /// </summary>
        public string GetFallbackText(LocalizationKey key)
        {
            if (key == nameKey)
                return displayName;
            if (key == descKey)
                return description;
            return string.Empty;
        }

        #endregion

        #region Localized Text Accessors

        /// <summary>
        /// Get localized display name with fallback to legacy displayName field.
        /// This is the preferred way to get item names in UI code.
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
        /// Get localized description with fallback to legacy description field.
        /// This is the preferred way to get item descriptions in UI code.
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
            int currentVersion = Tartaria.Save.SchemaVersion.CURRENT_ITEM;
            
            if (schemaVersion < currentVersion)
            {
                Debug.Log($"[ItemData] {name}: Auto-migrating from v{schemaVersion} to v{currentVersion}");
                // Future: Apply migration logic here when v2 is released
                schemaVersion = currentVersion;
            }
        }

        #endregion
    }

    /// <summary>
    /// Item category enum for filtering and organization.
    /// </summary>
    public enum ItemCategory
    {
        Consumable,  // Health potions, food, buffs
        Equipment,   // Weapons, armor, tools
        Material,    // Crafting materials, resources
        QuestItem,   // Quest-specific items
        Currency,    // Resonance Shards, special currencies
        Misc         // Everything else
    }

    /// <summary>
    /// Item rarity enum for color coding and value scaling.
    /// </summary>
    public enum ItemRarity
    {
        Common,      // White/Gray
        Uncommon,    // Green
        Rare,        // Blue
        Epic,        // Purple
        Legendary,   // Orange/Gold
        Mythic       // Red/Crimson
    }
}
