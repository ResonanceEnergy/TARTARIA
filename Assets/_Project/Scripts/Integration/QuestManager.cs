using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Data;
using Tartaria.Gameplay;
using Tartaria.Save;
using QuestStatus = Tartaria.Core.Enums.QuestStatus;
using Tartaria.Input;

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
    ///
    /// Now supports QuestDatabase integration with prerequisite validation.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestManager : MonoBehaviour, IQuestProvider, IQuestService, ISaveDataProvider
    {
        public static QuestManager Instance { get; private set; }

        [Header("Quest Database")]
        [Tooltip("Legacy: individual quest definitions (deprecated - use questDatabaseAsset instead)")]
        [SerializeField] QuestDefinition[] questDatabase;

        [Tooltip("New: centralized quest database with validation and prerequisite chains")]
        [SerializeField] QuestDatabase questDatabaseAsset;

        readonly Dictionary<string, QuestState> _questStates = new();
        readonly Dictionary<string, QuestDefinition> _questLookup = new();

        // Cached ID lists — rebuilt only when quest status changes
        readonly List<string> _cachedActiveIds = new();
        readonly List<string> _cachedCompletedIds = new();
        bool _questListsDirty = true;

        public event Action<string, QuestStatus> OnQuestStatusChanged;
        public event Action<string, int> OnObjectiveProgressed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            QuestProviderLocator.Current = this;
            ServiceLocator.Quest = this;

            // Wire save/load events
            if (Save.SaveManager.Instance != null)
            {
                Save.SaveManager.Instance.OnBeforeSave += OnSave;
                Save.SaveManager.Instance.OnAfterLoad += OnLoad;
                Save.SaveManager.Instance.RegisterProvider(this);  // ISaveDataProvider registration
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (ServiceLocator.Quest == (IQuestService)this) ServiceLocator.Quest = null;

            // Cleanup save/load event handlers
            if (Save.SaveManager.Instance != null)
            {
                Save.SaveManager.Instance.OnBeforeSave -= OnSave;
                Save.SaveManager.Instance.OnAfterLoad -= OnLoad;
                Save.SaveManager.Instance.UnregisterProvider(this);  // ISaveDataProvider cleanup
            }
        }

        void OnSave(Save.SaveData sd)
        {
            // Persist quest states to SaveData.quests
            if (sd.quests != null)
            {
                var entries = new List<Save.QuestSaveEntry>();

                foreach (var kvp in _questStates)
                {
                    entries.Add(new Save.QuestSaveEntry
                    {
                        questId = kvp.Key,
                        status = (int)kvp.Value.status,
                        objectiveProgress = kvp.Value.objectiveProgress ?? System.Array.Empty<int>()
                    });
                }

                sd.quests.entries = entries.ToArray();
                Debug.Log($"[QuestManager] Saved {entries.Count} quest states");
            }
        }

        void OnLoad(Save.SaveData sd)
        {
            // Restore quest states from SaveData.quests
            if (sd.quests?.entries != null)
            {
                foreach (var entry in sd.quests.entries)
                {
                    if (string.IsNullOrEmpty(entry.questId)) continue;

                    // Only restore if quest exists in database
                    if (_questStates.ContainsKey(entry.questId))
                    {
                        _questStates[entry.questId] = new QuestState
                        {
                            status = (QuestStatus)entry.status,
                            objectiveProgress = entry.objectiveProgress ?? System.Array.Empty<int>()
                        };
                    }
                }

                _questListsDirty = true;
                Debug.Log($"[QuestManager] Loaded {sd.quests.entries.Length} quest states");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ISaveDataProvider Implementation
        // ═══════════════════════════════════════════════════════════════

        public string GetProviderKey() => "QuestManager";

        public object GetSaveData()
        {
            var entries = new List<QuestManagerSaveData.QuestEntry>();

            foreach (var kvp in _questStates)
            {
                entries.Add(new QuestManagerSaveData.QuestEntry
                {
                    questId = kvp.Key,
                    status = (int)kvp.Value.status,
                    objectiveProgress = kvp.Value.objectiveProgress ?? System.Array.Empty<int>()
                });
            }

            return new QuestManagerSaveData
            {
                entries = entries.ToArray()
            };
        }

        public void RestoreSaveData(object data)
        {
            if (data is string json)
            {
                try
                {
                    var saveData = JsonUtility.FromJson<QuestManagerSaveData>(json);
                    if (saveData?.entries != null)
                    {
                        foreach (var entry in saveData.entries)
                        {
                            if (string.IsNullOrEmpty(entry.questId)) continue;

                            // Only restore if quest exists in database
                            if (_questStates.ContainsKey(entry.questId))
                            {
                                _questStates[entry.questId] = new QuestState
                                {
                                    status = (QuestStatus)entry.status,
                                    objectiveProgress = entry.objectiveProgress ?? System.Array.Empty<int>()
                                };
                            }
                        }

                        _questListsDirty = true;
                        Debug.Log($"[QuestManager] Restored {saveData.entries.Length} quest states via ISaveDataProvider");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[QuestManager] Failed to restore save data: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[QuestManager] RestoreSaveData received invalid data type");
            }
        }

        void Start()
        {
            // Load from new QuestDatabase asset (preferred)
            if (questDatabaseAsset != null)
            {
                LoadFromQuestDatabase();
            }
            // Fallback to legacy quest array (must have entries — Unity initializes SerializeField
            // arrays as Length=0 empty arrays, not null, so the original `!= null` check was always true
            // on a freshly-added QuestManager and starved the builder fallback. R38 bugfix.)
            else if (questDatabase != null && questDatabase.Length > 0)
            {
                LoadFromLegacyArray();
            }
            // Last resort: auto-populate from builder (Moons 1-13)
            else
            {
                LoadFromBuilder();
            }

            Debug.Log($"[QuestManager] Loaded {_questLookup.Count} quests.");
        }

        void LoadFromQuestDatabase()
        {
            var allQuestIds = questDatabaseAsset.GetAllQuestIds();
            foreach (var questId in allQuestIds)
            {
                var quest = questDatabaseAsset.GetQuest(questId);
                if (quest == null) continue;

                _questLookup[quest.questId] = quest;
                _questStates[quest.questId] = new QuestState
                {
                    status = quest.autoActivate ? QuestStatus.Active : QuestStatus.Locked,
                    objectiveProgress = new int[quest.GetRuntimeObjectives().Length]
                };
            }

            Debug.Log($"[QuestManager] Loaded from QuestDatabase asset: {_questLookup.Count} quests");
        }

        void LoadFromLegacyArray()
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

            Debug.Log($"[QuestManager] Loaded from legacy array: {_questLookup.Count} quests");
        }

        void LoadFromBuilder()
        {
            // QuestDatabaseBuilder is implemented — build Moons 1-13 quest database
            questDatabase = QuestDatabaseBuilder.BuildAll();
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
                Debug.Log($"[QuestManager] Auto-populated {_questLookup.Count} quests from QuestDatabaseBuilder.");
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Activate a locked quest (e.g., when RS threshold or trigger condition met).
        /// Now validates prerequisites if using QuestData.
        /// </summary>
        public void ActivateQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return;

            if (!_questStates.TryGetValue(questId, out var state)) return;
            if (state.status != QuestStatus.Locked) return;

            // Validate prerequisites if QuestData
            if (_questLookup.TryGetValue(questId, out var def) && def is QuestData questData)
            {
                if (!ValidatePrerequisites(questData))
                {
                    Debug.LogWarning($"[QuestManager] Cannot activate '{questId}' - prerequisites not met");
                    return;
                }
            }

            state.status = QuestStatus.Active;
            _questStates[questId] = state;
            _questListsDirty = true;

            // Fire both legacy event and GameEvents
            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Active);
            Core.GameEvents.RaiseQuestStatusChanged(new Core.QuestStatusChangedEventArgs
            {
                questId = questId,
                newStatus = QuestStatus.Active,
                oldStatus = QuestStatus.Locked
            });

            if (_questLookup.TryGetValue(questId, out var questDef))
            {
                DialogueManager.Instance?.PlayContextDialogue("quest_start");
                GameEvents.RaiseHUDShowInteractionPrompt($"New Quest: {questDef.displayName}");
                AudioManager.Instance?.PlaySFX2D("QuestAccept");
                Input.HapticFeedbackManager.Instance?.PlayDiscovery();
            }

            Debug.Log($"[QuestManager] Quest activated: {questId}");

            // TODO: Enable when TutorialSystem is active
            // TutorialSystem.Instance?.ForceComplete(TutorialStep.QuestAccept);
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
            if (string.IsNullOrEmpty(questId)) return;
            if (!_questStates.TryGetValue(questId, out var state)) return;
            if (state.status != QuestStatus.Active) return;
            if (!_questLookup.TryGetValue(questId, out var def)) return;
            if (def.objectives == null || objectiveIndex < 0 || objectiveIndex >= def.objectives.Length) return;

            state.objectiveProgress[objectiveIndex] = Mathf.Min(
                state.objectiveProgress[objectiveIndex] + amount,
                def.objectives[objectiveIndex].targetCount);

            _questStates[questId] = state;

            // Fire both legacy event and GameEvents
            OnObjectiveProgressed?.Invoke(questId, objectiveIndex);
            Core.GameEvents.RaiseQuestObjectiveProgressed(new Core.QuestObjectiveProgressedEventArgs
            {
                questId = questId,
                objectiveIndex = objectiveIndex,
                current = state.objectiveProgress[objectiveIndex],
                target = def.objectives[objectiveIndex].targetCount
            });

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
            // Snapshot keys — ProgressObjective/CompleteQuest/ActivateQuest modify _questStates
            var snapshot = new List<string>(_questStates.Keys);
            foreach (var questId in snapshot)
            {
                if (!_questStates.TryGetValue(questId, out var state)) continue;
                if (state.status != QuestStatus.Active) continue;
                if (!_questLookup.TryGetValue(questId, out var def)) continue;
                if (def.objectives == null) continue;

                for (int i = 0; i < def.objectives.Length; i++)
                {
                    if (def.objectives[i].type != type) continue;
                    if (!string.IsNullOrEmpty(def.objectives[i].targetId) &&
                        def.objectives[i].targetId != targetId)
                        continue;

                    ProgressObjective(questId, i, amount);
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
            RebuildCachedListsIfDirty();
            return _cachedActiveIds;
        }

        /// <summary>
        /// Get the first active quest's data (for simple single-quest-at-a-time UIs).
        /// Feature 4: returns current active quest.
        /// </summary>
        public QuestDisplayData GetActiveQuest()
        {
            RebuildCachedListsIfDirty();
            if (_cachedActiveIds.Count == 0) return null;

            string questId = _cachedActiveIds[0];
            if (!_questLookup.TryGetValue(questId, out var def)) return null;
            if (!_questStates.TryGetValue(questId, out var state)) return null;

            return new QuestDisplayData
            {
                id = questId,
                title = def.displayName,
                description = def.description,
                objectiveIds = def.objectives != null
                    ? System.Array.ConvertAll(def.objectives, o => o.description)
                    : new string[0]
            };
        }

        /// <summary>
        /// Get all completed quest IDs.
        /// </summary>
        public List<string> GetCompletedQuestIds()
        {
            RebuildCachedListsIfDirty();
            return _cachedCompletedIds;
        }

        void RebuildCachedListsIfDirty()
        {
            if (!_questListsDirty) return;
            _questListsDirty = false;
            _cachedActiveIds.Clear();
            _cachedCompletedIds.Clear();
            foreach (var kvp in _questStates)
            {
                if (kvp.Value.status == QuestStatus.Active)
                    _cachedActiveIds.Add(kvp.Key);
                else if (kvp.Value.status == QuestStatus.Completed)
                    _cachedCompletedIds.Add(kvp.Key);
            }
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
            _questListsDirty = true;
            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Failed);
            Debug.Log($"[QuestManager] Quest failed: {questId}");
        }

        // ─── Internal ────────────────────────────────

        /// <summary>
        /// Marks a quest as completed and grants rewards. Can be called by Moon spawners for quest completion.
        /// Now handles QuestData rewards (XP, items, unlocks) and auto-activates prerequisite-unlocked quests.
        /// </summary>
        public void CompleteQuest(string questId)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return;

            QuestStatus oldStatus = state.status;
            state.status = QuestStatus.Completed;
            _questStates[questId] = state;
            _questListsDirty = true;

            // Fire both legacy event and GameEvents
            OnQuestStatusChanged?.Invoke(questId, QuestStatus.Completed);
            Core.GameEvents.RaiseQuestStatusChanged(new Core.QuestStatusChangedEventArgs
            {
                questId = questId,
                newStatus = QuestStatus.Completed,
                oldStatus = oldStatus
            });

            if (_questLookup.TryGetValue(questId, out var def))
            {
                // Grant RS reward via GameLoopController
                if (def.rsReward > 0f)
                    GameLoopController.Instance?.QueueRSReward(def.rsReward, "quest_complete");

                // Grant enhanced rewards if QuestData
                if (def is QuestData questData)
                {
                    GrantQuestDataRewards(questData);
                }

                DialogueManager.Instance?.PlayContextDialogue("quest_complete");
                GameEvents.RaiseHUDShowInteractionPrompt($"Quest Complete: {def.displayName}");
                AudioManager.Instance?.PlaySFX2D("QuestComplete");
                Input.HapticFeedbackManager.Instance?.PlayBuildingEmergence();
                Save.SaveManager.Instance?.MarkDirty();
                Debug.Log($"[QuestManager] Quest completed: {questId} (+{def.rsReward} RS)");

                // Activate follow-up quests
                if (def.followUpQuestIds != null)
                {
                    foreach (var followUp in def.followUpQuestIds)
                        ActivateQuest(followUp);
                }

                // Check for quests unlocked by this completion (prerequisite chains)
                if (questDatabaseAsset != null)
                {
                    var unlockedQuests = questDatabaseAsset.GetFollowUpQuests(questId);
                    foreach (var unlockedQuest in unlockedQuests)
                    {
                        if (unlockedQuest.autoActivateOnPrerequisites)
                        {
                            TryAutoActivateQuest(unlockedQuest);
                        }
                    }
                }
            }
        }

        bool AreAllObjectivesComplete(string questId)
        {
            if (!_questStates.TryGetValue(questId, out var state)) return false;
            if (!_questLookup.TryGetValue(questId, out var def)) return false;

            // Use enhanced objectives if QuestData
            var objectives = (def is QuestData qd) ? qd.GetRuntimeObjectives() : def.objectives;
            if (objectives == null) return true;

            for (int i = 0; i < objectives.Length; i++)
            {
                if (state.objectiveProgress[i] < objectives[i].targetCount)
                    return false;
            }
            return true;
        }

        // ─── Prerequisite Validation ─────────────────

        /// <summary>
        /// Validate if prerequisites are met for a QuestData.
        /// </summary>
        bool ValidatePrerequisites(QuestData questData)
        {
            if (questData == null) return true;

            // Check prerequisites via GameLoopController
            float currentRS = GameLoopController.Instance?.GetCurrentRS() ?? 0f;
            int currentLevel = PlayerProgression.Instance?.CurrentLevel ?? 1;

            return questData.ArePrerequisitesMet(currentRS, currentLevel, IsQuestComplete);
        }

        /// <summary>
        /// Try to auto-activate a quest if all prerequisites are met.
        /// </summary>
        void TryAutoActivateQuest(QuestData questData)
        {
            if (questData == null) return;

            if (!_questStates.TryGetValue(questData.questId, out var state))
                return;

            if (state.status != QuestStatus.Locked)
                return;

            if (ValidatePrerequisites(questData))
            {
                ActivateQuest(questData.questId);
            }
        }

        /// <summary>
        /// Grant enhanced rewards from QuestData (XP, items, unlocks).
        /// </summary>
        void GrantQuestDataRewards(QuestData questData)
        {
            if (questData == null) return;

            // Grant XP
            if (questData.xpReward > 0)
            {
                PlayerProgression.Instance?.AddXP(questData.xpReward, "quest_complete");
                Debug.Log($"[QuestManager] Granted {questData.xpReward} XP");
            }

            // Grant items
            if (questData.itemRewards != null)
            {
                foreach (var itemId in questData.itemRewards)
                {
                    if (string.IsNullOrEmpty(itemId)) continue;
                    InventorySystem.Instance?.AddItem(itemId, 1);
                    Debug.Log($"[QuestManager] Granted item: {itemId}");
                }
            }

            // Trigger unlocks
            if (questData.unlockRewards != null)
            {
                foreach (var unlockId in questData.unlockRewards)
                {
                    if (string.IsNullOrEmpty(unlockId)) continue;
                    Gameplay.PlayerProgression.Instance?.UnlockFeature(unlockId);
                    Debug.Log($"[QuestManager] Feature unlocked: {unlockId}");
                }
            }
        }

        // ─── Save/Load ──────────────────────────────

        public Dictionary<string, QuestState> GetAllStatesForSave()
        {
            return new Dictionary<string, QuestState>(_questStates);
        }

        // ROUND 4: Wiring + 5-beat Moon3-6 handler + bond interplay + giant harmony + permanent payoff reactivity complete (only this + 5 other narrative files edited).

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

    /// <summary>
    /// Simple quest data container for UI/HUD display (Feature 4).
    /// Contains only the essential fields needed to show active quest info.
    /// </summary>
    public class QuestDisplayData
    {
        public string id;
        public string title;
        public string description;
        public string[] objectiveIds;
    }

    // Quest types (QuestStatus, QuestState, QuestDefinition, etc.) are defined in Tartaria.Core.QuestTypes

    /// <summary>
    /// Serializable save data for QuestManager ISaveDataProvider.
    /// Must be JSON-serializable (no MonoBehaviour, no Unity objects).
    /// </summary>
    [Serializable]
    public class QuestManagerSaveData
    {
        public QuestEntry[] entries;

        [Serializable]
        public class QuestEntry
        {
            public string questId;
            public int status;  // QuestStatus enum as int
            public int[] objectiveProgress;
        }
    }
}
