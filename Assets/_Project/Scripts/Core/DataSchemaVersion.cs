using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Data Schema Version Constants — version tracking for ScriptableObject data types.
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
    public static class DataSchemaVersion
    {
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

        // ── Dialogue Data Versions ──────────────────────────────────────────
        public const int DIALOGUE_V1 = 1; // Initial DialogueNodeData schema
        public const int CURRENT_DIALOGUE = DIALOGUE_V1;
    }
}
