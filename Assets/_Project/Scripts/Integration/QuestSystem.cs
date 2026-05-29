using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// QuestSystem — Quest activation and tracking.
    /// TODO from REALITY_CHECK Phase 2.
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        [Header("Active Quests")]
        [SerializeField] private List<Quest> activeQuests = new();
        [SerializeField] private List<Quest> completedQuests = new();

        private Dictionary<string, Quest> _questRegistry = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStarterQuests();
        }

        void InitializeStarterQuests()
        {
            // 3 starter quests from REALITY_CHECK Phase 2
            RegisterQuest(new Quest
            {
                questId = "moon1_discovery",
                title = "First Discovery",
                description = "Discover the buried cathedral in Echohaven",
                objectives = new List<QuestObjective>
                {
                    new() { description = "Use Resonance Scanner", completed = false },
                    new() { description = "Discover buried building", completed = false }
                }
            });

            RegisterQuest(new Quest
            {
                questId = "moon1_restoration",
                title = "First Restoration",
                description = "Restore the dome building",
                objectives = new List<QuestObjective>
                {
                    new() { description = "Tune Node 1", completed = false },
                    new() { description = "Tune Node 2", completed = false },
                    new() { description = "Tune Node 3", completed = false },
                    new() { description = "Complete restoration", completed = false }
                }
            });

            RegisterQuest(new Quest
            {
                questId = "moon1_combat",
                title = "First Combat",
                description = "Defeat the corrupted Mud Golem",
                objectives = new List<QuestObjective>
                {
                    new() { description = "Defeat Mud Golem", completed = false }
                }
            });

            Debug.Log("[QuestSystem] ✅ Initialized 3 starter quests");
        }

        void RegisterQuest(Quest quest)
        {
            _questRegistry[quest.questId] = quest;
        }

        public void ActivateQuest(string questId)
        {
            if (_questRegistry.TryGetValue(questId, out var quest))
            {
                if (!activeQuests.Contains(quest) && !completedQuests.Contains(quest))
                {
                    activeQuests.Add(quest);
                    quest.isActive = true;
                    Debug.Log($"[QuestSystem] ✅ Activated quest: {quest.title}");
                    GameEvents.FireQuestActivated(questId);
                }
            }
            else
            {
                Debug.LogWarning($"[QuestSystem] Quest not found: {questId}");
            }
        }

        public void CompleteObjective(string questId, int objectiveIndex)
        {
            if (_questRegistry.TryGetValue(questId, out var quest) && quest.isActive)
            {
                if (objectiveIndex < quest.objectives.Count)
                {
                    quest.objectives[objectiveIndex].completed = true;
                    Debug.Log($"[QuestSystem] Objective {objectiveIndex} completed for {quest.title}");
                    GameEvents.FireQuestObjectiveCompleted(questId, objectiveIndex);

                    // Check if all objectives complete
                    if (quest.objectives.TrueForAll(o => o.completed))
                    {
                        CompleteQuest(questId);
                    }
                }
            }
        }

        public void CompleteQuest(string questId)
        {
            if (_questRegistry.TryGetValue(questId, out var quest) && quest.isActive)
            {
                quest.isActive = false;
                quest.isComplete = true;
                activeQuests.Remove(quest);
                completedQuests.Add(quest);
                Debug.Log($"[QuestSystem] ✅ Quest COMPLETE: {quest.title}");
                GameEvents.FireQuestCompleted(questId);
            }
        }

        public Quest GetQuest(string questId) => _questRegistry.GetValueOrDefault(questId);
        public List<Quest> GetActiveQuests() => activeQuests;
    }

    [System.Serializable]
    public class Quest
    {
        public string questId;
        public string title;
        public string description;
        public List<QuestObjective> objectives;
        public bool isActive;
        public bool isComplete;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public bool completed;
        
        // Additional fields to match Core.QuestObjective for QuestDatabaseBuilder compatibility
        public QuestObjectiveType type;
        public string targetId;
        public int targetCount = 1;
    }
}
