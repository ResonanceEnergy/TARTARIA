using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Quest Nodes — Echohaven quest system
    /// 3 starter quests that guide player through Moon 1 objectives
    /// Integrates with progression systems and provides structured goals
    /// </summary>
    [DefaultExecutionOrder(-80)]
    public class Moon1QuestNodes : MonoBehaviour
    {
        [Header("Quest Configuration")]
        [SerializeField] Quest[] moon1Quests;
        
        readonly Dictionary<string, Quest> _activeQuests = new();
        readonly HashSet<string> _completedQuests = new();
        
        void Start()
        {
            InitializeQuests();
            WireGameEvents();
            
            // Start first quest automatically
            StartQuest("restore_the_grid");
            
            Debug.Log("[Moon1QuestNodes] ✅ Initialized - 3 quests available");
        }
        
        void OnDestroy()
        {
            UnwireGameEvents();
        }
        
        void InitializeQuests()
        {
            moon1Quests = new Quest[]
            {
                new Quest
                {
                    questID = "restore_the_grid",
                    questName = "Restore the Grid",
                    description = "Tune all 8 Resonance Nodes in Echohaven to restore the harmony grid.",
                    objectives = new QuestObjective[]
                    {
                        new QuestObjective { description = "Tune 8 Resonance Nodes", targetCount = 8, objectiveType = "tune_nodes" }
                    },
                    rewards = new QuestReward
                    {
                        rsReward = 50f,
                        unlockNextQuest = "gather_the_shards"
                    }
                },
                
                new Quest
                {
                    questID = "gather_the_shards",
                    questName = "Gather the Shards",
                    description = "Collect all 15 Aether Shards scattered throughout the cathedral.",
                    objectives = new QuestObjective[]
                    {
                        new QuestObjective { description = "Collect 15 Aether Shards", targetCount = 15, objectiveType = "collect_shards" }
                    },
                    rewards = new QuestReward
                    {
                        rsReward = 40f,
                        unlockNextQuest = "defeat_the_dissonance"
                    }
                },
                
                new Quest
                {
                    questID = "defeat_the_dissonance",
                    questName = "Defeat the Dissonance",
                    description = "Clear Echohaven of Mud Golems to purge the dissonance.",
                    objectives = new QuestObjective[]
                    {
                        new QuestObjective { description = "Defeat 10 Mud Golems", targetCount = 10, objectiveType = "kill_enemies" }
                    },
                    rewards = new QuestReward
                    {
                        rsReward = 60f,
                        unlockNextQuest = null  // Last quest
                    }
                }
            };
        }
        
        void WireGameEvents()
        {
            GameEvents.OnTuningNodeActivated += OnNodeTuned;
            GameEvents.OnCollectibleGathered += OnShardCollected;
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }
        
        void UnwireGameEvents()
        {
            GameEvents.OnTuningNodeActivated -= OnNodeTuned;
            GameEvents.OnCollectibleGathered -= OnShardCollected;
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }
        
        void OnNodeTuned(int nodeID)
        {
            UpdateQuestProgress("restore_the_grid", "tune_nodes", 1);
        }
        
        void OnShardCollected(CollectibleEventArgs args)
        {
            if (args.collectibleType == "AetherShard")
            {
                UpdateQuestProgress("gather_the_shards", "collect_shards", 1);
            }
        }
        
        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            if (args.enemyType == "MudGolem")
            {
                UpdateQuestProgress("defeat_the_dissonance", "kill_enemies", 1);
            }
        }
        
        public void StartQuest(string questID)
        {
            Quest quest = System.Array.Find(moon1Quests, q => q.questID == questID);
            if (quest == null || _activeQuests.ContainsKey(questID) || _completedQuests.Contains(questID))
                return;
            
            quest.isActive = true;
            _activeQuests[questID] = quest;
            
            // Show quest notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowQuestNotification($"New Quest: {quest.questName}", quest.description);
            }
            
            Debug.Log($"[Moon1QuestNodes] Quest started: {quest.questName}");
        }
        
        void UpdateQuestProgress(string questID, string objectiveType, int amount)
        {
            if (!_activeQuests.ContainsKey(questID)) return;
            
            Quest quest = _activeQuests[questID];
            
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.objectiveType == objectiveType)
                {
                    objective.currentCount += amount;
                    
                    // Check completion
                    if (objective.currentCount >= objective.targetCount)
                    {
                        objective.isComplete = true;
                        CheckQuestCompletion(questID);
                    }
                    
                    // Update UI
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateQuestProgress(questID, objective.currentCount, objective.targetCount);
                    }
                }
            }
        }
        
        void CheckQuestCompletion(string questID)
        {
            Quest quest = _activeQuests[questID];
            bool allComplete = true;
            
            foreach (QuestObjective objective in quest.objectives)
            {
                if (!objective.isComplete)
                {
                    allComplete = false;
                    break;
                }
            }
            
            if (allComplete)
            {
                CompleteQuest(questID);
            }
        }
        
        void CompleteQuest(string questID)
        {
            if (!_activeQuests.ContainsKey(questID)) return;
            
            Quest quest = _activeQuests[questID];
            quest.isComplete = true;
            _completedQuests.Add(questID);
            _activeQuests.Remove(questID);
            
            // Grant rewards
            if (GameStateManager.Instance != null && quest.rewards != null)
            {
                GameStateManager.Instance.AddResonancePoints(quest.rewards.rsReward);
            }
            
            // Show completion notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowQuestComplete(quest.questName, quest.rewards.rsReward);
            }
            
            Debug.Log($"[Moon1QuestNodes] ✅ Quest complete: {quest.questName} (+{quest.rewards.rsReward} RS)");
            
            // Unlock next quest
            if (quest.rewards != null && !string.IsNullOrEmpty(quest.rewards.unlockNextQuest))
            {
                Invoke("StartNextQuest", 2f);  // 2s delay
                _nextQuestToStart = quest.rewards.unlockNextQuest;
            }
        }
        
        string _nextQuestToStart;
        void StartNextQuest()
        {
            if (!string.IsNullOrEmpty(_nextQuestToStart))
            {
                StartQuest(_nextQuestToStart);
                _nextQuestToStart = null;
            }
        }
        
        public Quest GetActiveQuest(string questID)
        {
            return _activeQuests.ContainsKey(questID) ? _activeQuests[questID] : null;
        }
        
        public bool IsQuestComplete(string questID)
        {
            return _completedQuests.Contains(questID);
        }
    }
    
    [System.Serializable]
    public class Quest
    {
        public string questID;
        public string questName;
        public string description;
        public QuestObjective[] objectives;
        public QuestReward rewards;
        public bool isActive;
        public bool isComplete;
    }
    
    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public string objectiveType;  // "tune_nodes", "collect_shards", "kill_enemies"
        public int targetCount;
        public int currentCount;
        public bool isComplete;
    }
    
    [System.Serializable]
    public class QuestReward
    {
        public float rsReward;
        public string unlockNextQuest;
    }
}
