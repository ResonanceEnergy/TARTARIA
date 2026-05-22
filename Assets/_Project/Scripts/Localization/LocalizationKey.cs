using System;
using UnityEngine;

namespace Tartaria.Localization
{
    /// <summary>
    /// Localization key struct — immutable reference to a translated string.
    /// 
    /// Design:
    /// - Value type (struct) for zero-allocation lookups
    /// - Category-scoped keys (items.name.aether_shard, quests.title.moon1_main)
    /// - Auto-generated IDs from existing strings via editor tools
    /// - Fallback to legacy string fields for backward compatibility
    /// 
    /// Usage:
    ///     LocalizationKey key = new LocalizationKey("items.name", "aether_shard");
    ///     string text = LocalizationManager.Instance.GetText(key);
    /// 
    /// Key Format:
    ///     category.subcategory.identifier
    ///     Examples:
    ///       items.name.aether_shard
    ///       items.desc.aether_shard
    ///       quests.title.moon1_main
    ///       quests.objective.moon1_main_01
    ///       dialogue.node.anastasia_intro_01
    ///       dialogue.choice.player_accept
    ///       skills.name.tuning_master
    ///       skills.desc.tuning_master
    ///       ui.button.continue
    ///       ui.label.health
    /// </summary>
    [Serializable]
    public struct LocalizationKey : IEquatable<LocalizationKey>
    {
        [Tooltip("Category prefix (items, quests, dialogue, skills, ui)")]
        [SerializeField] private string category;
        
        [Tooltip("Unique identifier within category")]
        [SerializeField] private string id;

        /// <summary>
        /// Creates a new localization key from category and ID.
        /// </summary>
        public LocalizationKey(string category, string id)
        {
            this.category = category ?? string.Empty;
            this.id = id ?? string.Empty;
        }

        /// <summary>
        /// Creates a localization key from a full dotted path.
        /// Example: "items.name.aether_shard" → category="items.name", id="aether_shard"
        /// </summary>
        public static LocalizationKey FromPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return Empty;

            int lastDot = fullPath.LastIndexOf('.');
            if (lastDot < 0)
                return new LocalizationKey(string.Empty, fullPath);

            string category = fullPath.Substring(0, lastDot);
            string id = fullPath.Substring(lastDot + 1);
            return new LocalizationKey(category, id);
        }

        /// <summary>
        /// Full key path in dot notation (category.subcategory.id).
        /// </summary>
        public string FullPath => string.IsNullOrEmpty(category) ? id : $"{category}.{id}";

        /// <summary>
        /// Category prefix (items, quests, dialogue, etc.).
        /// </summary>
        public string Category => category;

        /// <summary>
        /// Identifier within category.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// True if this is a valid key with non-empty ID.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(id);

        /// <summary>
        /// Empty/default key representing no localization.
        /// </summary>
        public static LocalizationKey Empty => new LocalizationKey(string.Empty, string.Empty);

        // Equality implementation for dictionary lookups
        public bool Equals(LocalizationKey other)
        {
            return category == other.category && id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizationKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((category?.GetHashCode() ?? 0) * 397) ^ (id?.GetHashCode() ?? 0);
            }
        }

        public override string ToString() => FullPath;

        public static bool operator ==(LocalizationKey left, LocalizationKey right) => left.Equals(right);
        public static bool operator !=(LocalizationKey left, LocalizationKey right) => !left.Equals(right);
    }

    /// <summary>
    /// Interface for ScriptableObjects containing localizable text.
    /// Implemented by ItemData, QuestData, DialogueNodeData, etc.
    /// </summary>
    public interface ILocalizable
    {
        /// <summary>
        /// Returns all localization keys used by this object.
        /// Used by editor extraction tools to generate string tables.
        /// </summary>
        LocalizationKey[] GetLocalizationKeys();

        /// <summary>
        /// Returns fallback text for a given key (for missing translations).
        /// Used when localized text is not available in the current language.
        /// </summary>
        string GetFallbackText(LocalizationKey key);
    }

    /// <summary>
    /// Localization categories for organizing string tables.
    /// Each category gets its own CSV file.
    /// </summary>
    public static class LocalizationCategory
    {
        public const string Items = "items";
        public const string Quests = "quests";
        public const string Dialogue = "dialogue";
        public const string Skills = "skills";
        public const string UI = "ui";
        public const string Equipment = "equipment";
        public const string Crafting = "crafting";
        public const string Combat = "combat";
        public const string System = "system";

        /// <summary>
        /// All category names for editor tools.
        /// </summary>
        public static readonly string[] All = new[]
        {
            Items, Quests, Dialogue, Skills, UI,
            Equipment, Crafting, Combat, System
        };
    }
}
