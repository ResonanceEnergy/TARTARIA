using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Core;  // QuestStatus enum

namespace Tartaria.Integration  // NOTE: Lives in UI folder but part of Integration assembly (wires QuestManager→UI)
{
    /// <summary>
    /// Quest Log UI Panel — displays active and completed quests grouped by Moon.
    /// Attach to Canvas panel, wire to QuestManager events.
    /// Shows Moon 2-13 quest chains with progress bars.
    /// </summary>
    public class QuestLogUIPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] Transform questListContainer;
        [SerializeField] GameObject questEntryPrefab;
        [SerializeField] Text titleText;

        [Header("Filters")]
        [SerializeField] bool showActiveOnly = true;
        [SerializeField] bool groupByMoon = true;

        readonly List<QuestEntryUI> _entries = new();

        void Start()
        {
            // Subscribe to QuestManager events
            var questMgr = QuestManager.Instance;
            if (questMgr != null)
            {
                questMgr.OnQuestStatusChanged += OnQuestStatusChangedHandler;
                questMgr.OnObjectiveProgressed += OnObjectiveProgressedHandler;
            }

            gameObject.SetActive(false);  // Hidden by default
        }

        void OnDestroy()
        {
            // Unsubscribe
            var questMgr = QuestManager.Instance;
            if (questMgr != null)
            {
                questMgr.OnQuestStatusChanged -= OnQuestStatusChangedHandler;
                questMgr.OnObjectiveProgressed -= OnObjectiveProgressedHandler;
            }
        }

        void OnQuestStatusChangedHandler(string questId, QuestStatus status)
        {
            Debug.Log($"[QuestLogUI] Quest {questId} → {status}");
            RefreshQuestLog();
        }

        void OnObjectiveProgressedHandler(string questId, int objectiveIndex)
        {
            Debug.Log($"[QuestLogUI] Quest {questId} objective {objectiveIndex} progressed");
            RefreshQuestLog();
        }

        public void ToggleQuestLog()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            if (gameObject.activeSelf)
            {
                RefreshQuestLog();
            }
        }

        public void RefreshQuestLog()
        {
            ClearEntries();

            // Fetch active and completed quests from QuestManager
            var questMgr = Integration.QuestManager.Instance;
            if (questMgr == null)
            {
                Debug.LogWarning("[QuestLogUI] QuestManager not available, showing placeholder");
                AddPlaceholderEntries();
                return;
            }

            var activeQuests = questMgr.GetActiveQuestIds();
            var completedQuests = questMgr.GetCompletedQuestIds();

            if (groupByMoon)
            {
                // Group quests by Moon number
                for (int moon = 1; moon <= 13; moon++)
                {
                    var moonQuests = activeQuests.Where(qid => qid.StartsWith($"moon{moon}")).ToList();
                    if (moonQuests.Count > 0)
                    {
                        AddMoonSectionWithQuests(moon, moonQuests);
                    }
                }
            }
            else
            {
                // Flat list of all quests
                foreach (var qid in activeQuests)
                {
                    AddQuestEntry(qid, QuestStatus.Active);
                }
                foreach (var qid in completedQuests)
                {
                    AddQuestEntry(qid, QuestStatus.Completed);
                }
            }

            Debug.Log($"[QuestLogUI] Refreshed with {activeQuests.Count} active, {completedQuests.Count} completed");
        }

        void AddMoonSection(int moonNumber)
        {
            // Add Moon header
            var headerGO = Instantiate(questEntryPrefab, questListContainer);
            var header = headerGO.GetComponent<QuestEntryUI>();
            if (header != null)
            {
                header.SetHeaderMode($"— MOON {moonNumber} —");
                _entries.Add(header);
            }

            // Add quests for this Moon
            var questMgr = Integration.QuestManager.Instance;
            if (questMgr != null)
            {
                var moonQuests = questMgr.GetActiveQuestIds()
                    .Where(qid => qid.StartsWith($"moon{moonNumber}"))
                    .ToList();

                foreach (var qid in moonQuests)
                {
                    AddQuestEntry(qid, QuestStatus.Active);
                }
            }
            else
            {
                // Fallback placeholder
                AddPlaceholderQuest($"moon{moonNumber}_primary", $"Moon {moonNumber} Primary Quest", 0.75f);
            }
        }

        void AddPlaceholderEntries()
        {
            AddPlaceholderQuest("test_quest_1", "Discover Star Dome", 1.0f);
            AddPlaceholderQuest("test_quest_2", "Restore Bell Tower", 0.5f);
            AddPlaceholderQuest("test_quest_3", "Tune Harmonic Fountain", 0.0f);
        }

        void AddPlaceholderQuest(string questId, string questName, float progress)
        {
            var entryGO = Instantiate(questEntryPrefab, questListContainer);
            var entry = entryGO.GetComponent<QuestEntryUI>();
            if (entry != null)
            {
                entry.SetQuestData(questId, questName, progress);
                _entries.Add(entry);
            }
        }

        void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                if (entry != null) Destroy(entry.gameObject);
            }
            _entries.Clear();
        }
    }

    /// <summary>
    /// Individual quest entry UI component.
    /// </summary>
    public class QuestEntryUI : MonoBehaviour
    {
        [SerializeField] Text questNameText;
        [SerializeField] Slider progressBar;
        [SerializeField] GameObject completeIcon;

        string _questId;
        bool _isHeader;

        public void SetQuestData(string questId, string questName, float progress)
        {
            _questId = questId;
            _isHeader = false;

            if (questNameText != null)
                questNameText.text = questName;

            if (progressBar != null)
            {
                progressBar.value = progress;
                progressBar.gameObject.SetActive(true);
            }

            if (completeIcon != null)
                completeIcon.SetActive(progress >= 0.99f);
        }

        public void SetHeaderMode(string headerText)
        {
            _isHeader = true;

            if (questNameText != null)
            {
                questNameText.text = headerText;
                questNameText.fontStyle = FontStyle.Bold;
            }

            if (progressBar != null)
                progressBar.gameObject.SetActive(false);

            if (completeIcon != null)
                completeIcon.SetActive(false);
        }

        public void OnPointerClick()
        {
            if (!_isHeader && !string.IsNullOrEmpty(_questId))
            {
                Debug.Log($"[QuestLogUI] Clicked quest {_questId}");
                // Show detailed quest panel (QuestDetailPanel UI integration pending)
                var detailPanel = FindFirstObjectByType<QuestDetailPanel>();
                if (detailPanel != null)
                {
                    detailPanel.ShowQuest(_questId);
                }
            }
        }
    }
}
