using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data
{
    /// <summary>
    /// Central quest database - master collection of all quest definitions.
    /// Provides validation, lookup, and prerequisite chain resolution.
    /// Assign this to QuestManager via Inspector for data-driven quest loading.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Data/Quest Database", order = 1)]
    public class QuestDatabase : ScriptableObject
    {
        [Header("Quest Collection")]
        [Tooltip("All quests in the game - organized by Moon or category")]
        [SerializeField] QuestData[] allQuests = System.Array.Empty<QuestData>();

        [Header("Validation")]
        [Tooltip("If true, validate quest chain integrity on load")]
        public bool validateOnLoad = true;

        // Cached lookup table
        Dictionary<string, QuestData> _questLookup;
        bool _isIndexed;

        /// <summary>
        /// Get quest by ID with lazy initialization.
        /// </summary>
        public QuestData GetQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return null;

            EnsureIndexed();
            return _questLookup.TryGetValue(questId, out var quest) ? quest : null;
        }

        /// <summary>
        /// Get all quests for a specific moon.
        /// </summary>
        public QuestData[] GetQuestsByMoon(int moonId)
        {
            EnsureIndexed();
            return allQuests.Where(q => q != null && q.moonId == moonId).ToArray();
        }

        /// <summary>
        /// Get all quests of a specific category.
        /// </summary>
        public QuestData[] GetQuestsByCategory(QuestCategory category)
        {
            EnsureIndexed();
            return allQuests.Where(q => q != null && q.category == category).ToArray();
        }

        /// <summary>
        /// Get all main story quests in order.
        /// </summary>
        public QuestData[] GetMainQuestChain()
        {
            EnsureIndexed();
            return allQuests
                .Where(q => q != null && q.isMainQuest)
                .OrderBy(q => q.moonId)
                .ThenBy(q => q.rsRequirement)
                .ToArray();
        }

        /// <summary>
        /// Get quests that become available after completing the given quest.
        /// </summary>
        public QuestData[] GetFollowUpQuests(string completedQuestId)
        {
            if (string.IsNullOrEmpty(completedQuestId))
                return System.Array.Empty<QuestData>();

            EnsureIndexed();

            // Direct follow-ups
            var quest = GetQuest(completedQuestId);
            var followUps = new List<QuestData>();

            if (quest?.followUpQuestIds != null)
            {
                foreach (var followUpId in quest.followUpQuestIds)
                {
                    var followUp = GetQuest(followUpId);
                    if (followUp != null)
                        followUps.Add(followUp);
                }
            }

            // Prerequisite unlocks
            foreach (var q in allQuests)
            {
                if (q == null || q.prerequisiteQuestIds == null) continue;
                if (System.Array.IndexOf(q.prerequisiteQuestIds, completedQuestId) >= 0)
                {
                    if (!followUps.Contains(q))
                        followUps.Add(q);
                }
            }

            return followUps.ToArray();
        }

        /// <summary>
        /// Validate quest chain integrity (no missing references, circular deps, etc.).
        /// </summary>
        public bool ValidateQuestChains(out List<string> errors)
        {
            errors = new List<string>();
            EnsureIndexed();

            foreach (var quest in allQuests)
            {
                if (quest == null)
                {
                    errors.Add("Null quest entry in database");
                    continue;
                }

                // Validate quest ID
                if (string.IsNullOrEmpty(quest.questId))
                {
                    errors.Add($"Quest '{quest.name}' has empty questId");
                }

                // Validate prerequisites
                if (quest.prerequisiteQuestIds != null)
                {
                    foreach (var prereqId in quest.prerequisiteQuestIds)
                    {
                        if (string.IsNullOrEmpty(prereqId)) continue;

                        if (!_questLookup.ContainsKey(prereqId))
                        {
                            errors.Add($"Quest '{quest.questId}' references missing prerequisite '{prereqId}'");
                        }

                        // Check for circular dependencies
                        if (HasCircularDependency(quest.questId, prereqId, new HashSet<string>()))
                        {
                            errors.Add($"Circular dependency detected: '{quest.questId}' <-> '{prereqId}'");
                        }
                    }
                }

                // Validate follow-ups
                if (quest.followUpQuestIds != null)
                {
                    foreach (var followUpId in quest.followUpQuestIds)
                    {
                        if (string.IsNullOrEmpty(followUpId)) continue;

                        if (!_questLookup.ContainsKey(followUpId))
                        {
                            errors.Add($"Quest '{quest.questId}' references missing follow-up '{followUpId}'");
                        }
                    }
                }

                // Validate objectives
                var objectives = quest.GetRuntimeObjectives();
                if (objectives == null || objectives.Length == 0)
                {
                    errors.Add($"Quest '{quest.questId}' has no objectives defined");
                }
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Get total quest count.
        /// </summary>
        public int GetQuestCount() => allQuests?.Length ?? 0;

        /// <summary>
        /// Get all quest IDs.
        /// </summary>
        public string[] GetAllQuestIds()
        {
            EnsureIndexed();
            return _questLookup.Keys.ToArray();
        }

        void EnsureIndexed()
        {
            if (_isIndexed && _questLookup != null)
                return;

            _questLookup = new Dictionary<string, QuestData>();
            _isIndexed = true;

            if (allQuests == null)
                return;

            foreach (var quest in allQuests)
            {
                if (quest == null || string.IsNullOrEmpty(quest.questId))
                    continue;

                if (_questLookup.ContainsKey(quest.questId))
                {
                    Debug.LogWarning($"[QuestDatabase] Duplicate quest ID: {quest.questId}");
                    continue;
                }

                _questLookup[quest.questId] = quest;
            }

            if (validateOnLoad)
            {
                if (ValidateQuestChains(out var errors))
                {
                    Debug.Log($"[QuestDatabase] Validated {_questLookup.Count} quests successfully.");
                }
                else
                {
                    Debug.LogWarning($"[QuestDatabase] Validation found {errors.Count} issues:\n{string.Join("\n", errors)}");
                }
            }
        }

        bool HasCircularDependency(string questId, string prereqId, HashSet<string> visited)
        {
            if (visited.Contains(prereqId))
                return prereqId == questId;

            visited.Add(prereqId);

            var prereqQuest = GetQuest(prereqId);
            if (prereqQuest?.prerequisiteQuestIds == null)
                return false;

            foreach (var subPrereq in prereqQuest.prerequisiteQuestIds)
            {
                if (string.IsNullOrEmpty(subPrereq)) continue;
                if (HasCircularDependency(questId, subPrereq, visited))
                    return true;
            }

            return false;
        }

        void OnValidate()
        {
            // Force re-index on editor changes
            _isIndexed = false;
        }
    }
}
