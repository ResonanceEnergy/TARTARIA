namespace Tartaria.Core.Enums
{
    /// <summary>
    /// Crafting station types — defines which station is required for crafting recipes.
    /// Used by CraftingStationManager and recipe definitions.
    /// </summary>
    public enum StationType : byte
    {
        Workbench = 0,      // Basic tools, building materials
        Forge = 1,          // Weapons, armor, metal refinement
        AlchemyTable = 2    // Potions, buffs, resonance consumables
    }

    /// <summary>
    /// Player stat types — primary attributes for character progression.
    /// Used by PlayerProgression for stat allocation and scaling.
    /// </summary>
    public enum StatType : byte
    {
        Vitality = 0,       // HP scaling, survivability
        Resonance = 1,      // RS pool, ability power
        Strength = 2,       // Melee damage, carry weight
        Agility = 3,        // Dodge, movement speed
        Attunement = 4      // Magic damage, RS regen
    }

    /// <summary>
    /// Item category enum for filtering and organization.
    /// Merged from ItemData and EnemyData with conflict resolution.
    /// </summary>
    public enum ItemCategory
    {
        Consumable,     // Health potions, food, buffs
        Equipment,      // Weapons, armor, tools (player equipment)
        Material,       // Crafting materials, resources
        QuestItem,      // Quest-specific items
        KeyItem,        // Special story items (from enemy loot)
        Currency,       // Resonance Shards, special currencies
        Misc            // Everything else
    }

    /// <summary>
    /// Item rarity enum for color coding and value scaling.
    /// Canonical version from ItemData with Mythic + Ascendant (Moon 13) tier.
    /// </summary>
    public enum ItemRarity
    {
        Common,         // White/Gray
        Uncommon,       // Green
        Rare,           // Blue
        Epic,           // Purple
        Legendary,      // Orange/Gold
        Mythic,         // Red/Crimson
        Ascendant       // Cyan/White (Moon 13 endgame)
    }

    /// <summary>
    /// Equipment slot types for player character.
    /// Canonical version from EquipmentItemData.
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

    /// <summary>
    /// Dialogue stat types — extended stat set for dialogue conditions.
    /// Includes social stats (Intelligence, Charisma) not in player progression.
    /// Renamed from StatType to avoid conflict with player stat enum.
    /// </summary>
    public enum DialogueStatType
    {
        Strength,       // Physical power (shared with StatType)
        Agility,        // Speed, reflexes (shared with StatType)
        Vitality,       // Health, endurance (shared with StatType)
        Resonance,      // Aether attunement (shared with StatType)
        Intelligence,   // Knowledge, perception (dialogue-only)
        Charisma        // Social influence (dialogue-only)
    }
}
