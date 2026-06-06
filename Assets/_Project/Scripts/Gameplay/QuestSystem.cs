using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Quest System — Manages active quests, objectives, and progression
    /// Fires events for UI updates, persists to SaveData
    /// </summary>
    public class QuestSystem
    {
        static readonly Lazy<QuestSystem> _instance = new(() => new QuestSystem());
        public static QuestSystem Instance => _instance.Value;

        readonly Dictionary<string, Quest> _allQuests = new();
        readonly List<string> _activeQuestIds = new();
        readonly List<string> _completedQuestIds = new();

        public event Action<string> OnQuestActivated; // questId
        public event Action<string, int> OnObjectiveCompleted; // questId, objectiveIndex
        public event Action<string> OnQuestCompleted; // questId

        QuestSystem()
        {
            InitializeStarterQuests();
        }

        void InitializeStarterQuests()
        {
            // Quest 1: Restore First Building
            var q1 = new Quest
            {
                id = "moon1_restore_first",
                title = "The First Resonance",
                description = "Restore the buried cathedral to its former glory",
                objectives = new List<string>
                {
                    "Excavate the cathedral entrance",
                    "Tune the pipe organ (3 notes)",
                    "Place mercury spire on dome"
                }
            };
            _allQuests[q1.id] = q1;

            // Quest 2: Collect Aether Shards
            var q2 = new Quest
            {
                id = "moon1_collect_shards",
                title = "Scattered Light",
                description = "Gather Aether shards from around Echohaven",
                objectives = new List<string>
                {
                    "Collect 5 Aether shards (0/5)"
                }
            };
            _allQuests[q2.id] = q2;

            // Quest 3: Talk to Milo
            var q3 = new Quest
            {
                id = "moon1_meet_milo",
                title = "The Merchant of Memories",
                description = "Find Milo and learn about the old world",
                objectives = new List<string>
                {
                    "Talk to Milo near the cathedral",
                    "Listen to Milo's story about 1893"
                }
            };
            _allQuests[q3.id] = q3;

            Debug.Log($"[QuestSystem] Initialized {_allQuests.Count} starter quests");
        }

        public void ActivateQuest(string questId)
        {
            if (!_allQuests.ContainsKey(questId))
            {
                Debug.LogError($"[QuestSystem] Quest not found: {questId}");
                return;
            }

            if (_activeQuestIds.Contains(questId))
            {
                Debug.LogWarning($"[QuestSystem] Quest already active: {questId}");
                return;
            }

            _activeQuestIds.Add(questId);
            OnQuestActivated?.Invoke(questId);
            Debug.Log($"[QuestSystem] Activated quest: {_allQuests[questId].title}");
        }

        public void CompleteObjective(string questId, int objectiveIndex)
        {
            if (!_allQuests.TryGetValue(questId, out Quest quest))
            {
                Debug.LogError($"[QuestSystem] Quest not found: {questId}");
                return;
            }

            if (!_activeQuestIds.Contains(questId))
            {
                Debug.LogWarning($"[QuestSystem] Quest not active: {questId}");
                return;
            }

            if (objectiveIndex < 0 || objectiveIndex >= quest.objectives.Count)
            {
                Debug.LogError($"[QuestSystem] Invalid objective index: {objectiveIndex}");
                return;
            }

            if (quest.completedObjectives.Contains(objectiveIndex))
            {
                Debug.LogWarning($"[QuestSystem] Objective already completed: {questId}[{objectiveIndex}]");
                return;
            }

            quest.completedObjectives.Add(objectiveIndex);
            OnObjectiveCompleted?.Invoke(questId, objectiveIndex);
            Debug.Log($"[QuestSystem] Completed objective: {quest.objectives[objectiveIndex]}");

            // Check if quest is complete
            if (quest.completedObjectives.Count >= quest.objectives.Count)
            {
                CompleteQuest(questId);
            }
        }

        void CompleteQuest(string questId)
        {
            if (!_allQuests.TryGetValue(questId, out Quest quest))
                return;

            _activeQuestIds.Remove(questId);
            _completedQuestIds.Add(questId);
            OnQuestCompleted?.Invoke(questId);
            Debug.Log($"[QuestSystem] Quest completed: {quest.title}");
        }

        public Quest GetQuest(string questId)
        {
            return _allQuests.TryGetValue(questId, out Quest quest) ? quest : null;
        }

        public List<Quest> GetActiveQuests()
        {
            var result = new List<Quest>();
            foreach (var id in _activeQuestIds)
            {
                if (_allQuests.TryGetValue(id, out Quest quest))
                    result.Add(quest);
            }
            return result;
        }

        public bool IsQuestActive(string questId)
        {
            return _activeQuestIds.Contains(questId);
        }

        public bool IsQuestCompleted(string questId)
        {
            return _completedQuestIds.Contains(questId);
        }
    }

    [Serializable]
    public class Quest
    {
        public string id;
        public string title;
        public string description;
        public List<string> objectives;
        public List<int> completedObjectives = new();
    }
}
