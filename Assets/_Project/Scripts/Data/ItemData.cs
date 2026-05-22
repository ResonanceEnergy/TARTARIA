using UnityEngine;

namespace Tartaria.Data
{
    /// <summary>
    /// Item Data — ScriptableObject definition for a single item.
    /// Stores all item metadata: ID, name, description, icon, stats, category.
    /// 
    /// Create assets via: Assets → Create → Tartaria → Item Data
    /// Place in: Assets/_Project/Resources/Items/
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Tartaria/Item Data", order = 100)]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier (e.g., 'aether_shard', 'golem_core')")]
        public string itemID;
        
        [Tooltip("Display name shown in UI")]
        public string displayName;
        
        [TextArea(3, 6)]
        [Tooltip("Description shown in tooltips")]
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
        /// </summary>
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemID))
            {
                Debug.LogWarning($"[ItemData] {name}: itemID is empty!");
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
