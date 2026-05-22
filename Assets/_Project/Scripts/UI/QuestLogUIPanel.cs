using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tartaria.UI
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
            // Integration.QuestManager.Instance.OnQuestStatusChanged += RefreshQuestLog;

            gameObject.SetActive(false);  // Hidden by default
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

            // TODO: Fetch quests from QuestManager
            // For now, display placeholder entries

            if (groupByMoon)
            {
                for (int moon = 2; moon <= 13; moon++)
                {
                    AddMoonSection(moon);
                }
            }
            else
            {
                AddPlaceholderEntries();
            }

            Debug.Log("[QuestLogUI] Refreshed quest log");
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

            // Add quests for this Moon (placeholder)
            // TODO: Query QuestManager for moon-specific quests
            AddPlaceholderQuest($"moon{moonNumber}_primary", $"Moon {moonNumber} Primary Quest", 0.75f);
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

            if (questNameText != null) questNameText.text = questName;

            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = Mathf.Clamp01(progress);
            }

            if (completeIcon != null)
            {
                completeIcon.SetActive(progress >= 1f);
            }
        }

        public void SetHeaderMode(string headerText)
        {
            _isHeader = true;

            if (questNameText != null)
            {
                questNameText.text = headerText;
                questNameText.fontStyle = FontStyle.Bold;
            }

            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (completeIcon != null) completeIcon.SetActive(false);
        }

        public void OnPointerClick()
        {
            if (!_isHeader && !string.IsNullOrEmpty(_questId))
            {
                Debug.Log($"[QuestLogUI] Clicked quest {_questId}");
                // TODO: Show detailed quest panel
            }
        }
    }
}
