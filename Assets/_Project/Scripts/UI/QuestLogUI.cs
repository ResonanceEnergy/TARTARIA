using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Quest Log UI -- full-screen panel showing active + completed quests.
    ///
    /// Layout:
    ///   Left panel:  Scrollable quest list with category tabs (Active/Completed/All)
    ///   Right panel: Selected quest detail (name, description, objectives, rewards)
    ///
    /// Wired to QuestManager events for live updates.
    /// Toggle via Tab key (InputSystem) or HUD button.
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        public static QuestLogUI Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] GameObject questLogPanel;

        [Header("Quest List (Left)")]
        [SerializeField] Transform questListContent;
        [SerializeField] GameObject questEntryPrefab;

        [Header("Detail Panel (Right)")]
        [SerializeField] TextMeshProUGUI detailTitle;
        [SerializeField] TextMeshProUGUI detailDescription;
        [SerializeField] Transform objectivesListContent;
        [SerializeField] TextMeshProUGUI rewardText;

        [Header("Tab Buttons")]
        [SerializeField] Button tabActive;
        [SerializeField] Button tabCompleted;
        [SerializeField] Button tabAll;

        // ─── State ───
        bool _isOpen;
        QuestLogTab _currentTab = QuestLogTab.Active;
        string _selectedQuestId;
        readonly List<QuestEntryData> _entries = new();

        // ─── Runtime-created UI (when no prefab) ───
        readonly List<GameObject> _listItems = new();
        readonly List<GameObject> _objectiveItems = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (questLogPanel != null) questLogPanel.SetActive(false);
            WireTabButtons();
        }

        void Start()
        {
            // Subscribe to QuestManager events via Core interface
            var qp = QuestProviderLocator.Current;
            if (qp != null)
            {
                qp.OnQuestStatusChanged += OnQuestStatusChanged;
                qp.OnObjectiveProgressed += OnObjectiveProgressed;
            }
        }

        void OnDestroy()
        {
            var qp = QuestProviderLocator.Current;
            if (qp != null)
            {
                qp.OnQuestStatusChanged -= OnQuestStatusChanged;
                qp.OnObjectiveProgressed -= OnObjectiveProgressed;
            }
        }

        // ─── Public API ──────────────────────────────

        public void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public void Open()
        {
            _isOpen = true;
            if (questLogPanel != null) questLogPanel.SetActive(true);
            RefreshList();
        }

        public void Close()
        {
            _isOpen = false;
            if (questLogPanel != null) questLogPanel.SetActive(false);
        }

        public bool IsOpen => _isOpen;

        // ─── Tab Management ──────────────────────────

        void WireTabButtons()
        {
            tabActive?.onClick.AddListener(() => SetTab(QuestLogTab.Active));
            tabCompleted?.onClick.AddListener(() => SetTab(QuestLogTab.Completed));
            tabAll?.onClick.AddListener(() => SetTab(QuestLogTab.All));
        }

        void SetTab(QuestLogTab tab)
        {
            _currentTab = tab;
            RefreshList();
        }

        // ─── List Build ──────────────────────────────

        void RefreshList()
        {
            _entries.Clear();

            var qp = QuestProviderLocator.Current;
            if (qp == null) return;

            // Gather quests matching current tab
            var activeIds = qp.GetActiveQuestIds();
            var completedIds = qp.GetCompletedQuestIds();

            switch (_currentTab)
            {
                case QuestLogTab.Active:
                    foreach (var id in activeIds)
                        AddEntryData(qp, id, false);
                    break;
                case QuestLogTab.Completed:
                    foreach (var id in completedIds)
                        AddEntryData(qp, id, true);
                    break;
                case QuestLogTab.All:
                    foreach (var id in activeIds)
                        AddEntryData(qp, id, false);
                    foreach (var id in completedIds)
                        AddEntryData(qp, id, true);
                    break;
            }

            // Pool: reuse existing GameObjects, create only if needed, hide excess
            for (int i = 0; i < _entries.Count; i++)
            {
                GameObject go;
                if (i < _listItems.Count)
                {
                    go = _listItems[i];
                    go.SetActive(true);
                }
                else
                {
                    go = CreateListEntry(_entries[i]);
                    _listItems.Add(go);
                }
                UpdateListEntry(go, _entries[i]);
            }
            for (int i = _entries.Count; i < _listItems.Count; i++)
            {
                if (_listItems[i] != null) _listItems[i].SetActive(false);
            }

            // Auto-select first entry
            if (_entries.Count > 0 && string.IsNullOrEmpty(_selectedQuestId))
                SelectQuest(_entries[0].questId);
            else if (!string.IsNullOrEmpty(_selectedQuestId))
                ShowDetail(_selectedQuestId);
        }

        void AddEntryData(IQuestProvider qp, string questId, bool completed)
        {
            var def = qp.GetQuestDefinition(questId);
            if (def == null) return;

            _entries.Add(new QuestEntryData
            {
                questId = questId,
                displayName = def.displayName,
                isMain = def.isMainQuest,
                completed = completed
            });
        }

        void UpdateListEntry(GameObject go, QuestEntryData data)
        {
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                string prefix = data.isMain ? "[MAIN] " : "";
                string suffix = data.completed ? " <color=#888>DONE</color>" : "";
                txt.text = $"{prefix}{data.displayName}{suffix}";
            }
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                string capturedId = data.questId;
                btn.onClick.AddListener(() => SelectQuest(capturedId));
            }
        }

        GameObject CreateListEntry(QuestEntryData data)
        {
            GameObject go;
            if (questEntryPrefab != null && questListContent != null)
            {
                go = Instantiate(questEntryPrefab, questListContent);
            }
            else
            {
                go = new GameObject($"QuestEntry_{data.questId}");
                if (questListContent != null) go.transform.SetParent(questListContent, false);
            }

            // Ensure label exists
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt == null)
            {
                txt = go.AddComponent<TextMeshProUGUI>();
                txt.fontSize = 16;
                txt.alignment = TextAlignmentOptions.Left;
            }

            // Ensure button exists
            if (go.GetComponent<Button>() == null) go.AddComponent<Button>();

            return go;
        }

        // ─── Detail Panel ────────────────────────────

        void SelectQuest(string questId)
        {
            _selectedQuestId = questId;
            ShowDetail(questId);
        }

        void ShowDetail(string questId)
        {
            var qp = QuestProviderLocator.Current;
            if (qp == null) return;

            var def = qp.GetQuestDefinition(questId);
            var state = qp.GetQuestState(questId);
            if (def == null) return;

            if (detailTitle != null)
                detailTitle.text = def.displayName;
            if (detailDescription != null)
                detailDescription.text = def.description ?? "";

            // Objectives
            ClearObjectiveItems();
            if (def.objectives != null)
            {
                for (int i = 0; i < def.objectives.Length; i++)
                {
                    var obj = def.objectives[i];
                    int progress = (state.objectiveProgress != null && i < state.objectiveProgress.Length)
                        ? state.objectiveProgress[i] : 0;
                    bool done = progress >= obj.targetCount;

                    string label = done
                        ? $"<color=#6f6>[{progress}/{obj.targetCount}]</color> <s>{obj.description}</s>"
                        : $"[{progress}/{obj.targetCount}] {obj.description}";

                    var go = new GameObject($"Obj_{i}");
                    if (objectivesListContent != null) go.transform.SetParent(objectivesListContent, false);
                    var txt = go.AddComponent<TextMeshProUGUI>();
                    txt.fontSize = 14;
                    txt.text = label;
                    _objectiveItems.Add(go);
                }
            }

            // Reward
            if (rewardText != null)
                rewardText.text = def.rsReward > 0 ? $"Reward: +{def.rsReward:F0} RS" : "";
        }

        // ─── Event Handlers ──────────────────────────

        void OnQuestStatusChanged(string questId, QuestStatus status)
        {
            if (!_isOpen) return;
            RefreshList();

            // Fire notification
            if (status == QuestStatus.Active)
                NotificationSystem.Instance?.Show($"New Quest: {GetQuestName(questId)}", NotificationType.Quest);
            else if (status == QuestStatus.Completed)
                NotificationSystem.Instance?.Show($"Quest Complete: {GetQuestName(questId)}", NotificationType.QuestComplete);
        }

        void OnObjectiveProgressed(string questId, int objIndex)
        {
            if (_isOpen && _selectedQuestId == questId)
                ShowDetail(questId);
        }

        string GetQuestName(string questId)
        {
            var qp = QuestProviderLocator.Current;
            var def = qp?.GetQuestDefinition(questId);
            return def != null ? def.displayName : questId;
        }

        // ─── Cleanup ─────────────────────────────────

        void ClearListItems()
        {
            foreach (var go in _listItems) if (go != null) Destroy(go);
            _listItems.Clear();
        }

        void ClearObjectiveItems()
        {
            foreach (var go in _objectiveItems) if (go != null) Destroy(go);
            _objectiveItems.Clear();
        }


        // ─── Data ────────────────────────────────────

        struct QuestEntryData
        {
            public string questId;
            public string displayName;
            public bool isMain;
            public bool completed;
        }

        enum QuestLogTab : byte { Active, Completed, All }
    }

    // ══════════════════════════════════════════════════
    //  NOTIFICATION SYSTEM — Toast popup queue
    // ══════════════════════════════════════════════════

    /// <summary>
    /// Toast notification system — displays popup banners for game events.
    ///
    /// Features:
    ///   - Queue-based: max 3 simultaneous toasts
    ///   - Auto-dismiss after configurable duration
    ///   - Color-coded by NotificationType
    ///   - Animated slide-in from top-right
    ///
    /// Usage: NotificationSystem.Instance.Show("text", NotificationType.Quest);
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }

        [Header("Config")]
        [SerializeField] float defaultDuration = 3f;
        [SerializeField] float slideSpeed = 400f;
        [SerializeField] int maxVisible = 3;

        [Header("Anchor")]
        [SerializeField] Transform toastAnchor; // Top-right corner

        // ─── State ───
        readonly Queue<NotificationData> _pending = new();
        readonly List<ActiveToast> _active = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Process pending queue
            while (_active.Count < maxVisible && _pending.Count > 0)
                SpawnToast(_pending.Dequeue());

            // Update active toasts
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var toast = _active[i];
                toast.elapsed += Time.unscaledDeltaTime;

                // Slide in
                if (toast.rt != null)
                {
                    var pos = toast.rt.anchoredPosition;
                    float targetX = 0f;
                    pos.x = Mathf.Lerp(pos.x, targetX, slideSpeed * Time.unscaledDeltaTime * 0.01f);
                    toast.rt.anchoredPosition = pos;
                }

                // Fade out
                if (toast.elapsed > toast.duration - 0.5f)
                {
                    float fade = Mathf.Clamp01((toast.duration - toast.elapsed) / 0.5f);
                    if (toast.canvasGroup != null) toast.canvasGroup.alpha = fade;
                }

                // Remove expired
                if (toast.elapsed >= toast.duration)
                {
                    if (toast.go != null) Destroy(toast.go);
                    _active.RemoveAt(i);
                }
                else
                {
                    _active[i] = toast;
                }
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Show a notification toast.</summary>
        public void Show(string message, NotificationType type = NotificationType.Info)
        {
            Show(message, type, defaultDuration);
        }

        /// <summary>Show a notification toast with custom duration.</summary>
        public void Show(string message, NotificationType type, float duration)
        {
            var data = new NotificationData
            {
                message = message,
                type = type,
                duration = duration
            };

            if (_active.Count < maxVisible)
                SpawnToast(data);
            else
                _pending.Enqueue(data);
        }

        /// <summary>Convenience: currency gain notification.</summary>
        public void ShowCurrency(string currencyName, int amount)
        {
            Show($"+{amount} {currencyName}", NotificationType.Currency);
        }

        /// <summary>Convenience: codex unlock notification.</summary>
        public void ShowCodexUnlock(string entryTitle)
        {
            Show($"Codex: {entryTitle}", NotificationType.Codex);
        }

        /// <summary>Convenience: trust change notification.</summary>
        public void ShowTrustChange(string npcName, string newLevel)
        {
            Show($"{npcName}: Trust -> {newLevel}", NotificationType.Trust);
        }

        // ─── Toast Spawning ──────────────────────────

        void SpawnToast(NotificationData data)
        {
            var go = new GameObject($"Toast_{data.type}");
            if (toastAnchor != null)
                go.transform.SetParent(toastAnchor, false);
            else
                go.transform.SetParent(transform, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 50f);
            float yOffset = -60f * _active.Count;
            rt.anchoredPosition = new Vector2(350f, yOffset); // Start offscreen right

            // Background
            var bg = go.AddComponent<Image>();
            bg.color = GetBackgroundColor(data.type);

            // CanvasGroup for fade
            var cg = go.AddComponent<CanvasGroup>();

            // Text child
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 5f);
            textRt.offsetMax = new Vector2(-10f, -5f);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = data.message;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;

            // Icon prefix by type
            string icon = data.type switch
            {
                NotificationType.Quest         => ">> ",
                NotificationType.QuestComplete  => "** ",
                NotificationType.Currency       => "$ ",
                NotificationType.Codex          => "# ",
                NotificationType.Trust          => "<> ",
                NotificationType.Combat         => "!! ",
                NotificationType.Warning        => "/!\\ ",
                NotificationType.Achievement    => "<*> ",
                _ => ""
            };
            tmp.text = icon + data.message;

            _active.Add(new ActiveToast
            {
                go = go,
                rt = rt,
                canvasGroup = cg,
                elapsed = 0f,
                duration = data.duration
            });
        }

        Color GetBackgroundColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Quest         => new Color(0.2f, 0.4f, 0.7f, 0.85f), // Blue
                NotificationType.QuestComplete  => new Color(0.2f, 0.7f, 0.3f, 0.85f), // Green
                NotificationType.Currency       => new Color(0.8f, 0.7f, 0.1f, 0.85f), // Gold
                NotificationType.Codex          => new Color(0.6f, 0.3f, 0.7f, 0.85f), // Purple
                NotificationType.Trust          => new Color(0.3f, 0.6f, 0.6f, 0.85f), // Teal
                NotificationType.Combat         => new Color(0.7f, 0.2f, 0.2f, 0.85f), // Red
                NotificationType.Warning        => new Color(0.8f, 0.5f, 0.1f, 0.85f), // Orange
                NotificationType.Achievement    => new Color(0.85f, 0.75f, 0.2f, 0.85f), // Bright Gold
                _                               => new Color(0.3f, 0.3f, 0.3f, 0.85f), // Gray
            };
        }

        // ─── Data ────────────────────────────────────

        struct NotificationData
        {
            public string message;
            public NotificationType type;
            public float duration;
        }

        struct ActiveToast
        {
            public GameObject go;
            public RectTransform rt;
            public CanvasGroup canvasGroup;
            public float elapsed;
            public float duration;
        }
    }

    public enum NotificationType : byte
    {
        Info = 0,
        Quest = 1,
        QuestComplete = 2,
        Currency = 3,
        Codex = 4,
        Trust = 5,
        Combat = 6,
        Warning = 7,
        Achievement = 8
    }
}
