using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// High-performance skill registry with indexed lookups.
    /// Indexes skills by tier, RS cost, and modifier type for fast filtering.
    /// 
    /// Usage:
    ///   SkillRegistry.Initialize(skillTree);
    ///   SkillNodeData skill = SkillRegistry.Get(SkillId.TuningPrecision1);
    ///   var tier1Skills = SkillRegistry.GetByTier(1);
    /// </summary>
    public static class SkillRegistry
    {
        static DataRegistry<SkillNodeData> _registry;
        static bool _isInitialized;

        // Index names
        const string INDEX_TIER = "tier";
        const string INDEX_MODIFIER_TYPE = "modifierType";
        const string INDEX_RS_RANGE = "rsRange";
        const string INDEX_IS_BLESSING = "isBlessing";

        /// <summary>
        /// Initializes the registry from SkillTreeAsset.
        /// Call this once at game startup.
        /// </summary>
        public static void Initialize(SkillTreeAsset skillTree)
        {
            if (skillTree == null)
            {
                Debug.LogError("[SkillRegistry] Cannot initialize with null skill tree");
                return;
            }

            // Create registry with ID extractor
            _registry = new DataRegistry<SkillNodeData>(
                skill => skill.skillId.ToString(), 
                cacheSize: 50
            );

            // Register secondary indexes
            _registry.RegisterSecondaryIndex(INDEX_TIER, skill => skill.tier);
            _registry.RegisterSecondaryIndex(INDEX_MODIFIER_TYPE, skill => skill.modifierType);
            _registry.RegisterSecondaryIndex(INDEX_RS_RANGE, skill => GetRSRange(skill.rsCost));
            _registry.RegisterSecondaryIndex(INDEX_IS_BLESSING, skill => skill.rsCost == 0f);

            // Build indexes from skill tree
            var skills = skillTree.nodes.Where(n => n != null).ToList();
            _registry.AddRange(skills);

            _isInitialized = true;
            Debug.Log($"[SkillRegistry] Initialized with {_registry.Count} skills");
        }

        /// <summary>
        /// Gets a skill by ID. O(1) lookup.
        /// </summary>
        public static SkillNodeData Get(SkillId skillId)
        {
            EnsureInitialized();
            return _registry.Get(skillId.ToString());
        }

        /// <summary>
        /// Checks if a skill exists.
        /// </summary>
        public static bool Contains(SkillId skillId)
        {
            EnsureInitialized();
            return _registry.Contains(skillId.ToString());
        }

        /// <summary>
        /// Gets all skills in a specific tier. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<SkillNodeData> GetByTier(int tier)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_TIER, tier);
        }

        /// <summary>
        /// Gets all skills of a specific modifier type. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<SkillNodeData> GetByModifierType(SkillModifierType modifierType)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_MODIFIER_TYPE, modifierType);
        }

        /// <summary>
        /// Gets all progression blessings (0 RS cost). O(1) lookup.
        /// </summary>
        public static IReadOnlyList<SkillNodeData> GetBlessings()
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_IS_BLESSING, true);
        }

        /// <summary>
        /// Gets skills available at the specified RS budget.
        /// </summary>
        public static List<SkillNodeData> GetAffordableSkills(float currentRS)
        {
            EnsureInitialized();
            
            return _registry.Query()
                .Where(skill => skill.rsCost <= currentRS && skill.rsCost > 0)
                .OrderBy(skill => skill.tier)
                .ThenBy(skill => skill.rsCost)
                .ToList();
        }

        /// <summary>
        /// Gets prerequisite skills for the specified skill.
        /// </summary>
        public static List<SkillNodeData> GetPrerequisites(SkillId skillId)
        {
            EnsureInitialized();
            
            var skill = Get(skillId);
            if (skill == null || skill.prerequisiteIds == null || skill.prerequisiteIds.Count == 0)
                return new List<SkillNodeData>();

            var prerequisites = new List<SkillNodeData>();
            foreach (var prereqId in skill.prerequisiteIds)
            {
                var prereq = Get(prereqId);
                if (prereq != null)
                    prerequisites.Add(prereq);
            }

            return prerequisites;
        }

        /// <summary>
        /// Gets skills that unlock after completing the specified skill.
        /// </summary>
        public static List<SkillNodeData> GetDependents(SkillId skillId)
        {
            EnsureInitialized();
            
            var allSkills = GetAll();
            var dependents = new List<SkillNodeData>();

            foreach (var skill in allSkills)
            {
                if (skill.prerequisiteIds != null && skill.prerequisiteIds.Contains(skillId))
                {
                    dependents.Add(skill);
                }
            }

            return dependents;
        }

        /// <summary>
        /// Gets skills with high value (modifier > threshold).
        /// </summary>
        public static List<SkillNodeData> GetHighValueSkills(float minModifier)
        {
            EnsureInitialized();
            
            return _registry.Query()
                .Where(skill => skill.modifierValue >= minModifier)
                .OrderByDescending(skill => skill.modifierValue)
                .ToList();
        }

        /// <summary>
        /// Creates a fluent query builder for custom queries.
        /// </summary>
        public static QueryBuilder<SkillNodeData> Query()
        {
            EnsureInitialized();
            return _registry.Query();
        }

        /// <summary>
        /// Gets all skills.
        /// </summary>
        public static IReadOnlyList<SkillNodeData> GetAll()
        {
            EnsureInitialized();
            return _registry.GetAll();
        }

        /// <summary>
        /// Gets the total skill count.
        /// </summary>
        public static int Count
        {
            get
            {
                EnsureInitialized();
                return _registry.Count;
            }
        }

        /// <summary>
        /// Clears the registry (for hot-reload/testing).
        /// </summary>
        public static void Clear()
        {
            _registry?.Clear();
            _isInitialized = false;
        }

        static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[SkillRegistry] Not initialized! Call SkillRegistry.Initialize(skillTree) first.");
            }
        }

        // Helper to bucket skills by RS cost ranges
        static string GetRSRange(float rs)
        {
            if (rs == 0) return "blessing";
            if (rs < 50) return "0-50";
            if (rs < 100) return "50-100";
            if (rs < 200) return "100-200";
            if (rs < 500) return "200-500";
            return "500+";
        }
    }
}
