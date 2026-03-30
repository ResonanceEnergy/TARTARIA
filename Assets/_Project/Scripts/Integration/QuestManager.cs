using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Quest Manager -- tracks active, completed, and locked quests.
    /// Integrates with GameLoopController for RS rewards and DialogueManager
    /// for narrative beats.
    ///
    /// Phase 1 quests (Echohaven):
    ///   - Main: "Echoes of the Buried City" (discover + restore 3 buildings)
    ///   - Side: "Milo's Frequency" (optional companion quest)
    ///   - Side: "Golem Graveyard" (defeat all wave enemies)
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestManager : MonoBehaviour, IQuestProvider
    {
        public static QuestManager Instance { get; private set; }

        [Header("Quest Database")]
        [SerializeField] QuestDefinition[] questDatabase;

        readonly Dictionary<string, QuestState> _questStates = new();
        readonly Dictionary<string, QuestDefinition> _questLookup = new();

        public event Action<string, QuestStatus> OnQuestStatusChanged;
        public event Action<string, int> OnObjectiveProgressed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            QuestProviderLocator.Current = this;
        }

        void Start()
        {
            // Index quest database
            if (questDatabase != null)
            {
                foreach (var quest in questDatabase)
                {
                    if (quest == null) continue;
                    _questLookup[quest.questId] = quest;
                    _questStates[quest.questId] = new QuestState
                    {
                        status = quest.autoActivate ? QuestStatus.Active : QuestStatus.Locked,
                        objectiveProgress = new int[quest.objectives != null ? quest.objectives.Length : 0]
                    };
                }
            }

            Debug.Log($"[QuestManager] Loaded {_questLookup.Count} quests.");
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Activate a locked quest (e.g., when RS threshold or trigger condition met).
        /// </summary>
        public void ActivateQuest(string questId)
        {

            if (!_questStates.TryGetValue(questId, out var state)) return;
            if (state.status != QuestStatus.Locked) return;

            state.status = QuestStatus.Active;
            _questStates[questId] = state;
            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Active);

            if (_questLookup.TryGetValue(questId, out var def))
            {
                DialogueManager.Instance?.PlayContextDialogue("quest_start");
                HUDController.Instance?.ShowInteractionPrompt($"New Quest: {def.displayName}");
            }

            Debug.Log($"[QuestManager] Quest activated: {questId}");
        }

        /// <summary>
        /// Alias for ActivateQuest — used by tutorial/gameloop triggers.
        /// </summary>
        public void UnlockQuest(string questId) => ActivateQuest(questId);

        /// <summary>
        /// Progress an objective within an active quest.
        /// </summary>
        public void ProgressObjective(string questId, int objectiveIndex, int amount = 1)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return;
            if (state.status != QuestStatus.Active) return;
            if (!_questLookup.TryGetValue(questId, out var def)) return;
            if (def.objectives == null || objectiveIndex < 0 || objectiveIndex >= def.objectives.Length) return;

            state.objectiveProgress[objectiveIndex] = Mathf.Min(
                state.objectiveProgress[objectiveIndex] + amount,
                def.objectives[objectiveIndex].targetCount);

            _questStates[questId] = state;
            OnObjectiveProgressed?.Invoke(questId, objectiveIndex);

            // Check if all objectives complete
            if (AreAllObjectivesComplete(questId))
                CompleteQuest(questId);
        }

        /// <summary>
        /// Progress any objective matching the given type across all active quests.
        /// Called by GameLoopController when events occur.
        /// </summary>
        public void ProgressByType(QuestObjectiveType type, string targetId = null, int amount = 1)
        {
            foreach (var kvp in _questStates)
            {
                if (kvp.Value.status != QuestStatus.Active) continue;
                if (!_questLookup.TryGetValue(kvp.Key, out var def)) continue;
                if (def.objectives == null) continue;

                for (int i = 0; i < def.objectives.Length; i++)
                {
                    if (def.objectives[i].type != type) continue;
                    if (!string.IsNullOrEmpty(def.objectives[i].targetId) &&
                        def.objectives[i].targetId != targetId)
                        continue;

                    ProgressObjective(kvp.Key, i, amount);
                }
            }
        }

        /// <summary>
        /// Get current quest state for save/UI.
        /// </summary>
        public QuestState GetQuestState(string questId)
        {
            return _questStates.TryGetValue(questId, out var state) ? state : default;
        }

        /// <summary>
        /// Get all active quest IDs.
        /// </summary>
        public List<string> GetActiveQuestIds()
        {
            var result = new List<string>();
            foreach (var kvp in _questStates)
            {
                if (kvp.Value.status == QuestStatus.Active)
                    result.Add(kvp.Key);
            }
            return result;
        }

        /// <summary>
        /// Get all completed quest IDs.
        /// </summary>
        public List<string> GetCompletedQuestIds()
        {
            var result = new List<string>();
            foreach (var kvp in _questStates)
            {
                if (kvp.Value.status == QuestStatus.Completed)
                    result.Add(kvp.Key);
            }
            return result;
        }

        /// <summary>
        /// Get quest definition by ID (for UI display).
        /// </summary>
        public QuestDefinition GetQuestDefinition(string questId)
        {
            return _questLookup.TryGetValue(questId, out var def) ? def : null;
        }

        /// <summary>
        /// Check if a quest has been completed.
        /// </summary>
        public bool IsQuestComplete(string questId)
        {
            return _questStates.TryGetValue(questId, out var state)
                && state.status == QuestStatus.Completed;
        }

        public void FailQuest(string questId)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return;
            if (state.status != QuestStatus.Active) return;

            state.status = QuestStatus.Failed;
            _questStates[questId] = state;
            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Failed);
            Debug.Log($"[QuestManager] Quest failed: {questId}");
        }

        // ─── Internal ────────────────────────────────

        void CompleteQuest(string questId)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return;
            state.status = QuestStatus.Completed;
            _questStates[questId] = state;

            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Completed);

            if (_questLookup.TryGetValue(questId, out var def))
            {
                // Grant RS reward
                if (def.rsReward > 0f)
                    GameLoopController.Instance?.QueueRSReward(def.rsReward, "quest_complete");

                DialogueManager.Instance?.PlayContextDialogue("quest_complete");
                HUDController.Instance?.ShowInteractionPrompt($"Quest Complete: {def.displayName}");
                Debug.Log($"[QuestManager] Quest completed: {questId} (+{def.rsReward} RS)");

                // Activate follow-up quests
                if (def.followUpQuestIds != null)
                {
                    foreach (var followUp in def.followUpQuestIds)
                        ActivateQuest(followUp);
                }
            }
        }

        bool AreAllObjectivesComplete(string questId)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return false;
            if (!_questLookup.TryGetValue(questId, out var def)) return false;
            if (def.objectives == null) return true;

            for (int i = 0; i < def.objectives.Length; i++)
            {
                if (state.objectiveProgress[i] < def.objectives[i].targetCount)
                    return false;
            }
            return true;
        }

        // ─── Save/Load ──────────────────────────────

        public Dictionary<string, QuestState> GetAllStatesForSave()
        {
            return new Dictionary<string, QuestState>(_questStates);
        }

        public void RestoreFromSave(Dictionary<string, QuestState> saved)
        {
            if (saved == null) return;
            foreach (var kvp in saved)
            {
                if (_questStates.ContainsKey(kvp.Key))
                    _questStates[kvp.Key] = kvp.Value;
            }
        }
    }

    // Quest types (QuestStatus, QuestState, QuestDefinition, etc.) are defined in Tartaria.Core.QuestTypes
}
