using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// High-performance quest registry with indexed lookups.
    /// Replaces O(n) linear searches with O(1) dictionary lookups.
    /// 
    /// Usage:
    ///   QuestRegistry.Initialize(questDatabase);
    ///   QuestData quest = QuestRegistry.Get("moon1_discover_echohaven");
    ///   var moon1Quests = QuestRegistry.GetByMoon(1);
    /// </summary>
    public static class QuestRegistry
    {
        static DataRegistry<QuestData> _registry;
        static bool _isInitialized;

        // Index names
        const string INDEX_MOON = "moon";
        const string INDEX_CATEGORY = "category";
        const string INDEX_IS_MAIN = "isMain";
        const string INDEX_RS_RANGE = "rsRange";

        /// <summary>
        /// Initializes the registry from QuestDatabase.
        /// Call this once at game startup.
        /// </summary>
        public static void Initialize(QuestDatabase database)
        {
            if (database == null)
            {
                Debug.LogError("[QuestRegistry] Cannot initialize with null database");
                return;
            }

            // Create registry with ID extractor
            _registry = new DataRegistry<QuestData>(quest => quest.questId, cacheSize: 100);

            // Register secondary indexes
            _registry.RegisterSecondaryIndex(INDEX_MOON, quest => quest.moonId);
            _registry.RegisterSecondaryIndex(INDEX_CATEGORY, quest => quest.category);
            _registry.RegisterSecondaryIndex(INDEX_IS_MAIN, quest => quest.isMainQuest);
            _registry.RegisterSecondaryIndex(INDEX_RS_RANGE, quest => GetRSRange(quest.rsRequirement));

            // Build indexes from database
            var db = database;
            // QuestDatabase stores quests in allQuests field - we need to access it via reflection or add a public getter
            // For now, we'll use GetMainQuestChain and GetQuestsByMoon to build the registry
            var allQuests = new List<QuestData>();
            for (int moon = 1; moon <= 13; moon++)
            {
                var moonQuests = database.GetQuestsByMoon(moon);
                allQuests.AddRange(moonQuests);
            }
            
            // Add quests from all categories
            foreach (QuestCategory category in System.Enum.GetValues(typeof(QuestCategory)))
            {
                var categoryQuests = database.GetQuestsByCategory(category);
                foreach (var quest in categoryQuests)
                {
                    if (!allQuests.Contains(quest))
                        allQuests.Add(quest);
                }
            }

            _registry.AddRange(allQuests.Where(q => q != null));

            _isInitialized = true;
            Debug.Log($"[QuestRegistry] Initialized with {_registry.Count} quests");
        }

        /// <summary>
        /// Gets a quest by ID. O(1) lookup.
        /// </summary>
        public static QuestData Get(string questId)
        {
            EnsureInitialized();
            return _registry.Get(questId);
        }

        /// <summary>
        /// Checks if a quest exists.
        /// </summary>
        public static bool Contains(string questId)
        {
            EnsureInitialized();
            return _registry.Contains(questId);
        }

        /// <summary>
        /// Gets all quests for a specific moon. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<QuestData> GetByMoon(int moonId)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_MOON, moonId);
        }

        /// <summary>
        /// Gets all quests of a specific category. O(1) lookup.
        /// </summary>
        public static IReadOnlyList<QuestData> GetByCategory(QuestCategory category)
        {
            EnsureInitialized();
            return _registry.GetByIndex(INDEX_CATEGORY, category);
        }

        /// <summary>
        /// Gets all main story quests. O(1) lookup + sorting.
        /// </summary>
        public static List<QuestData> GetMainQuests()
        {
            EnsureInitialized();
            
            var mainQuests = _registry.GetByIndex(INDEX_IS_MAIN, true);
            return mainQuests
                .OrderBy(q => q.moonId)
                .ThenBy(q => q.rsRequirement)
                .ToList();
        }

        /// <summary>
        /// Gets side quests for a specific moon.
        /// </summary>
        public static List<QuestData> GetSideQuests(int moonId)
        {
            EnsureInitialized();
            
            return _registry.Query()
                .Where(q => q.moonId == moonId && !q.isMainQuest)
                .OrderBy(q => q.rsRequirement)
                .ToList();
        }

        /// <summary>
        /// Gets quests available at a specific RS (Resonance Score).
        /// </summary>
        public static List<QuestData> GetAvailableQuests(float currentRS)
        {
            EnsureInitialized();
            
            return _registry.Query()
                .Where(q => q.rsRequirement <= currentRS)
                .OrderBy(q => q.moonId)
                .ThenBy(q => q.rsRequirement)
                .ToList();
        }

        /// <summary>
        /// Gets quests that unlock a specific follow-up quest.
        /// </summary>
        public static List<QuestData> GetPrerequisitesFor(string questId)
        {
            EnsureInitialized();
            
            var quest = Get(questId);
            if (quest == null || quest.prerequisiteQuestIds == null)
                return new List<QuestData>();

            var prerequisites = new List<QuestData>();
            foreach (var prereqId in quest.prerequisiteQuestIds)
            {
                var prereq = Get(prereqId);
                if (prereq != null)
                    prerequisites.Add(prereq);
            }

            return prerequisites;
        }

        /// <summary>
        /// Gets quests that become available after completing the specified quest.
        /// </summary>
        public static List<QuestData> GetFollowUpQuests(string completedQuestId)
        {
            EnsureInitialized();
            
            var completed = Get(completedQuestId);
            if (completed == null)
                return new List<QuestData>();

            var followUps = new List<QuestData>();

            // Direct follow-ups
            if (completed.followUpQuestIds != null)
            {
                foreach (var followUpId in completed.followUpQuestIds)
                {
                    var followUp = Get(followUpId);
                    if (followUp != null)
                        followUps.Add(followUp);
                }
            }

            // Quests that have this as a prerequisite
            var allQuests = GetAll();
            foreach (var quest in allQuests)
            {
                if (quest.prerequisiteQuestIds != null &&
                    System.Array.IndexOf(quest.prerequisiteQuestIds, completedQuestId) >= 0)
                {
                    if (!followUps.Contains(quest))
                        followUps.Add(quest);
                }
            }

            return followUps;
        }

        /// <summary>
        /// Creates a fluent query builder for custom queries.
        /// </summary>
        public static QueryBuilder<QuestData> Query()
        {
            EnsureInitialized();
            return _registry.Query();
        }

        /// <summary>
        /// Gets all quests.
        /// </summary>
        public static IReadOnlyList<QuestData> GetAll()
        {
            EnsureInitialized();
            return _registry.GetAll();
        }

        /// <summary>
        /// Gets the total quest count.
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
                Debug.LogError("[QuestRegistry] Not initialized! Call QuestRegistry.Initialize(database) first.");
            }
        }

        // Helper to bucket quests by RS ranges for indexing
        static string GetRSRange(float rs)
        {
            if (rs < 100) return "0-100";
            if (rs < 500) return "100-500";
            if (rs < 1000) return "500-1000";
            if (rs < 2000) return "1000-2000";
            return "2000+";
        }
    }
}
