using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Core
{
    /// <summary>
    /// Central quest management system for TARTARIA 13 Moons campaign.
    /// Tracks active quests, objective progress, and completion state across all Moons.
    /// Integrates with SaveManager for persistence and GameEvents for quest callbacks.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        // Lazy singleton pattern (matches SaveManager, GameStateManager convention)
        private static readonly Lazy<QuestManager> _lazyInstance = new Lazy<QuestManager>(() =>
        {
            var go = new GameObject("[QuestManager]");
            DontDestroyOnLoad(go);
            return go.AddComponent<QuestManager>();
        });

        public static QuestManager Instance => _lazyInstance.Value;

        // Quest tracking
        private Dictionary<string, QuestData> _activeQuests = new Dictionary<string, QuestData>();
        private Dictionary<string, QuestData> _completedQuests = new Dictionary<string, QuestData>();
        private Dictionary<string, QuestData> _allQuests = new Dictionary<string, QuestData>(); // Quest database

        // Events
        public event Action<string> OnQuestStarted;
        public event Action<string> OnQuestCompleted;
        public event Action<string, string> OnObjectiveUpdated; // (questID, objectiveID)
        public event Action<string, string> OnObjectiveCompleted; // (questID, objectiveID)

        void Awake()
        {
            if (_lazyInstance.IsValueCreated && _lazyInstance.Value != this)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log("[QuestManager] Initialized");
        }

        /// <summary>
        /// Register a quest definition in the quest database. Called by Moon spawners or quest definition assets.
        /// </summary>
        public void RegisterQuest(QuestData questData)
        {
            if (questData == null || string.IsNullOrEmpty(questData.QuestID))
            {
                Debug.LogError("[QuestManager] Cannot register quest with null data or empty ID");
                return;
            }

            if (_allQuests.ContainsKey(questData.QuestID))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questData.QuestID}' already registered, overwriting");
            }

            _allQuests[questData.QuestID] = questData;
            Debug.Log($"[QuestManager] Registered quest: {questData.QuestID} ({questData.QuestName})");
        }

        /// <summary>
        /// Start tracking a quest as active. Returns false if quest not found in database.
        /// </summary>
        public bool StartQuest(string questID)
        {
            if (_activeQuests.ContainsKey(questID))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questID}' is already active");
                return true;
            }

            if (_completedQuests.ContainsKey(questID))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questID}' is already completed");
                return false;
            }

            if (!_allQuests.TryGetValue(questID, out QuestData questData))
            {
                Debug.LogError($"[QuestManager] Quest '{questID}' not found in database");
                return false;
            }

            _activeQuests[questID] = questData;
            OnQuestStarted?.Invoke(questID);
            Debug.Log($"[QuestManager] Started quest: {questID} ({questData.QuestName})");

            return true;
        }

        /// <summary>
        /// Mark a quest as completed. Public API for Moon spawners.
        /// </summary>
        public void CompleteQuest(string questID)
        {
            if (_completedQuests.ContainsKey(questID))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questID}' is already completed");
                return;
            }

            if (!_activeQuests.TryGetValue(questID, out QuestData questData))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questID}' is not active, marking completed anyway");
                
                // Try to find in all quests database
                if (!_allQuests.TryGetValue(questID, out questData))
                {
                    Debug.LogError($"[QuestManager] Quest '{questID}' not found in database");
                    return;
                }
            }

            // Mark all objectives complete
            if (questData.Objectives != null)
            {
                foreach (var objective in questData.Objectives)
                {
                    objective.State = QuestObjectiveState.Completed;
                    objective.CurrentProgress = objective.RequiredProgress;
                }
            }

            _activeQuests.Remove(questID);
            _completedQuests[questID] = questData;
            
            OnQuestCompleted?.Invoke(questID);
            Debug.Log($"[QuestManager] Completed quest: {questID} ({questData.QuestName})");
        }

        /// <summary>
        /// Update objective progress. Automatically completes objective if progress reaches requirement.
        /// </summary>
        public void UpdateObjective(string questID, string objectiveID, int progressIncrement)
        {
            if (!_activeQuests.TryGetValue(questID, out QuestData questData))
            {
                Debug.LogWarning($"[QuestManager] Cannot update objective - quest '{questID}' is not active");
                return;
            }

            var objective = questData.Objectives?.FirstOrDefault(obj => obj.ObjectiveID == objectiveID);
            if (objective == null)
            {
                Debug.LogError($"[QuestManager] Objective '{objectiveID}' not found in quest '{questID}'");
                return;
            }

            objective.CurrentProgress += progressIncrement;
            objective.CurrentProgress = Mathf.Clamp(objective.CurrentProgress, 0, objective.RequiredProgress);

            OnObjectiveUpdated?.Invoke(questID, objectiveID);

            // Check for objective completion
            if (objective.CurrentProgress >= objective.RequiredProgress && objective.State != QuestObjectiveState.Completed)
            {
                objective.State = QuestObjectiveState.Completed;
                OnObjectiveCompleted?.Invoke(questID, objectiveID);
                Debug.Log($"[QuestManager] Completed objective: {objectiveID} in quest {questID}");

                // Check if all objectives complete (auto-complete quest)
                if (questData.IsComplete)
                {
                    CompleteQuest(questID);
                }
            }
        }

        /// <summary>
        /// Check if a quest is currently active.
        /// </summary>
        public bool IsQuestActive(string questID)
        {
            return _activeQuests.ContainsKey(questID);
        }

        /// <summary>
        /// Check if a quest is completed.
        /// </summary>
        public bool IsQuestCompleted(string questID)
        {
            return _completedQuests.ContainsKey(questID);
        }

        /// <summary>
        /// Get quest data for a specific quest (active or completed).
        /// </summary>
        public QuestData GetQuest(string questID)
        {
            if (_activeQuests.TryGetValue(questID, out QuestData questData))
                return questData;

            if (_completedQuests.TryGetValue(questID, out questData))
                return questData;

            if (_allQuests.TryGetValue(questID, out questData))
                return questData;

            return null;
        }

        /// <summary>
        /// Get all currently active quests.
        /// </summary>
        public List<QuestData> GetActiveQuests()
        {
            return new List<QuestData>(_activeQuests.Values);
        }

        /// <summary>
        /// Get all completed quests.
        /// </summary>
        public List<QuestData> GetCompletedQuests()
        {
            return new List<QuestData>(_completedQuests.Values);
        }

        /// <summary>
        /// Get active quests for a specific Moon.
        /// </summary>
        public List<QuestData> GetActiveQuestsForMoon(int moonNumber)
        {
            return _activeQuests.Values.Where(q => q.MoonNumber == moonNumber).ToList();
        }

        /// <summary>
        /// Create a new quest dynamically (for Moon spawners that generate quests on the fly).
        /// </summary>
        public QuestData CreateQuest(string questID, string questName, string description, int moonNumber, QuestPriority priority = QuestPriority.Major)
        {
            var questData = new QuestData
            {
                QuestID = questID,
                QuestName = questName,
                Description = description,
                MoonNumber = moonNumber,
                Priority = priority,
                Objectives = new QuestObjective[0]
            };

            RegisterQuest(questData);
            return questData;
        }

        /// <summary>
        /// Add an objective to an existing quest.
        /// </summary>
        public void AddObjective(string questID, QuestObjective objective)
        {
            if (!_allQuests.TryGetValue(questID, out QuestData questData))
            {
                Debug.LogError($"[QuestManager] Cannot add objective - quest '{questID}' not found");
                return;
            }

            var objectivesList = questData.Objectives != null ? questData.Objectives.ToList() : new List<QuestObjective>();
            objectivesList.Add(objective);
            questData.Objectives = objectivesList.ToArray();

            Debug.Log($"[QuestManager] Added objective '{objective.ObjectiveID}' to quest '{questID}'");
        }

        /// <summary>
        /// Clear all quest data (for testing or new game).
        /// </summary>
        public void ClearAllQuests()
        {
            _activeQuests.Clear();
            _completedQuests.Clear();
            _allQuests.Clear();
            Debug.Log("[QuestManager] All quest data cleared");
        }
    }
}
