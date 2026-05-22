using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Schema Version Constants — centralized version tracking for all data types.
    /// 
    /// Design principles:
    ///   - Each data type has independent versioning (ITEM_V1, QUEST_V1, etc.)
    ///   - CURRENT_* constants point to latest version
    ///   - Legacy constants preserved for migration paths
    ///   - Version bump = breaking schema change
    /// 
    /// When to bump version:
    ///   ✓ Field added/removed/renamed
    ///   ✓ Field type changed
    ///   ✓ Enum value added/removed
    ///   ✗ Field value changed (data change, not schema change)
    ///   ✗ Documentation updated
    /// </summary>
    public static class SchemaVersion
    {
        // ── Save Data Versions ──────────────────────────────────────────────
        public const int SAVE_V1 = 1;   // Initial save system
        public const int SAVE_V2 = 2;   // Added achievement tracking
        public const int SAVE_V17 = 17; // Added ISaveDataProvider extensibility
        public const int SAVE_V18 = 18; // Added schema versioning (this system!)
        public const int CURRENT_SAVE = SAVE_V18;

        // ── Item Data Versions ──────────────────────────────────────────────
        public const int ITEM_V1 = 1;   // Initial ItemData schema
        public const int ITEM_V2 = 2;   // (Reserved for future stat refactor)
        public const int CURRENT_ITEM = ITEM_V1;

        // ── Quest Data Versions ─────────────────────────────────────────────
        public const int QUEST_V1 = 1;  // Initial QuestData schema
        public const int QUEST_V2 = 2;  // (Reserved for future objective refactor)
        public const int CURRENT_QUEST = QUEST_V1;

        // ── Enemy Data Versions ─────────────────────────────────────────────
        public const int ENEMY_V1 = 1;  // (Reserved for future enemy data)
        public const int CURRENT_ENEMY = ENEMY_V1;

        // ── Crafting Recipe Versions ────────────────────────────────────────
        public const int RECIPE_V1 = 1; // Initial CraftingRecipeData schema
        public const int CURRENT_RECIPE = RECIPE_V1;

        // ── Skill Tree Versions ─────────────────────────────────────────────
        public const int SKILL_V1 = 1;  // Initial SkillNodeData schema
        public const int CURRENT_SKILL = SKILL_V1;

        // ── Equipment Data Versions ─────────────────────────────────────────
        public const int EQUIPMENT_V1 = 1; // Initial EquipmentItemData schema
        public const int CURRENT_EQUIPMENT = EQUIPMENT_V1;

        // ── Dialogue Data Versions ──────────────────────────────────────────
        public const int DIALOGUE_V1 = 1; // Initial DialogueNodeData schema
        public const int CURRENT_DIALOGUE = DIALOGUE_V1;

        /// <summary>
        /// Check if a schema version is compatible (within supported migration range).
        /// </summary>
        /// <param name="currentVersion">Latest version supported</param>
        /// <param name="dataVersion">Version of the data being loaded</param>
        /// <param name="maxVersionsBack">How many versions back we support (default 10)</param>
        public static bool IsCompatible(int currentVersion, int dataVersion, int maxVersionsBack = 10)
        {
            if (dataVersion > currentVersion)
            {
                Debug.LogError($"[SchemaVersion] Data version {dataVersion} is newer than current {currentVersion}! Update the game.");
                return false;
            }

            if (dataVersion < (currentVersion - maxVersionsBack))
            {
                Debug.LogError($"[SchemaVersion] Data version {dataVersion} is too old (>{maxVersionsBack} versions behind {currentVersion})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get human-readable changelog between versions.
        /// </summary>
        public static string GetChangelog(string dataType, int fromVersion, int toVersion)
        {
            if (fromVersion == toVersion) return "No changes";

            string log = $"{dataType} schema updated v{fromVersion} → v{toVersion}:\n";

            // Save Data changelog
            if (dataType == "SaveData")
            {
                if (fromVersion < 2 && toVersion >= 2) log += "  • Added achievement tracking\n";
                if (fromVersion < 17 && toVersion >= 17) log += "  • Added ISaveDataProvider extensibility\n";
                if (fromVersion < 18 && toVersion >= 18) log += "  • Added schema versioning system\n";
            }

            // Item Data changelog
            else if (dataType == "ItemData")
            {
                if (fromVersion < 1 && toVersion >= 1) log += "  • Initial item schema\n";
                // Future: if (fromVersion < 2 && toVersion >= 2) log += "  • Refactored stats system\n";
            }

            // Quest Data changelog
            else if (dataType == "QuestData")
            {
                if (fromVersion < 1 && toVersion >= 1) log += "  • Initial quest schema\n";
                // Future: if (fromVersion < 2 && toVersion >= 2) log += "  • Refactored objectives\n";
            }

            return log;
        }

        /// <summary>
        /// Get the current version for a data type by name.
        /// </summary>
        public static int GetCurrentVersion(string dataType)
        {
            return dataType switch
            {
                "SaveData" => CURRENT_SAVE,
                "ItemData" => CURRENT_ITEM,
                "QuestData" => CURRENT_QUEST,
                "EnemyData" => CURRENT_ENEMY,
                "CraftingRecipeData" => CURRENT_RECIPE,
                "SkillNodeData" => CURRENT_SKILL,
                "EquipmentItemData" => CURRENT_EQUIPMENT,
                "DialogueNodeData" => CURRENT_DIALOGUE,
                _ => 1 // Default to v1 for unknown types
            };
        }
    }
}
