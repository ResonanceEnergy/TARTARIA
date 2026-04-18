using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// The Tartaria Old World Archive — an in-game educational wiki.
    ///
    /// Layout:
    ///   Left panel  — category tabs + scrollable entry list
    ///   Right panel — detail view: title / full text / anastasia quote / related entries
    ///   Top bar     — title, search field, entry count
    ///
    /// Controls:
    ///   I / Escape       — open / close
    ///   Category tabs    — filter by Architecture, Technology, etc.
    ///   Entry card       — select to read full text
    ///   Related entries  — click to jump directly to linked entry
    ///
    /// Unlock model:
    ///   ArchiveManager (Integration) calls SetUnlockedIds() / UnlockEntry() to notify this UI.
    ///   Locked entries appear as ??? in the list.
    ///   A golden badge (✦) marks newly unlocked entries.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArchiveUI : MonoBehaviour
    {
        public static ArchiveUI Instance { get; private set; }

        // ─── SerializeField refs (wired by RuntimeHUDBuilder) ───
        [SerializeField] public GameObject archivePanel;
        [SerializeField] public Button closeButton;
        [SerializeField] public TMP_InputField searchField;
        [SerializeField] public TextMeshProUGUI entryCountLabel;
        [SerializeField] public Transform entryListContainer;
        [SerializeField] public GameObject entryCardPrefab;    // null OK — fallback exists
        [SerializeField] public TextMeshProUGUI detailTitle;
        [SerializeField] public TextMeshProUGUI detailCategory;
        [SerializeField] public TextMeshProUGUI detailBody;
        [SerializeField] public TextMeshProUGUI anastasiaQuoteText;
        [SerializeField] public Transform relatedContainer;
        [SerializeField] public Button[] categoryTabButtons; // 8 tabs

        bool _open;
        ArchiveCategory? _activeCategory;
        ArchiveEntry _selectedEntry;
        string _searchQuery = "";
        ArchiveDatabase _db;

        // Unlock state — set by ArchiveManager (Integration) via public API
        readonly HashSet<string> _unlocked = new();
        readonly HashSet<string> _newBadge  = new();

        // Pool of spawned card GameObjects
        readonly List<GameObject> _cardPool = new();

        // ─── Lifecycle ────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Load the database from Resources (created by ArchiveDatabasePopulator)
            _db = Resources.Load<ArchiveDatabase>("ArchiveDatabase");
            if (archivePanel != null) archivePanel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (searchField != null) searchField.onValueChanged.AddListener(OnSearchChanged);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ───────────────────────────────

        public void Toggle() { if (_open) Close(); else Open(); }
        public bool IsOpen => _open;

        public void Open()
        {
            _open = true;
            if (archivePanel != null) archivePanel.SetActive(true);
            RefreshList();
        }

        public void Close()
        {
            _open = false;
            if (archivePanel != null) archivePanel.SetActive(false);
        }

        /// Called by ArchiveManager when a new entry is unlocked.
        public void OnEntryUnlocked(string entryId)
        {
            _unlocked.Add(entryId);
            _newBadge.Add(entryId);
            if (_open) RefreshList();
        }

        /// Bulk-restore unlocked state after loading a save.
        public void SetUnlockedIds(IEnumerable<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids) _unlocked.Add(id);
            if (_open) RefreshList();
        }

        public bool IsUnlocked(string id) => _unlocked.Contains(id);
        public bool IsNew(string id)      => _newBadge.Contains(id);
        public void MarkSeen(string id)   => _newBadge.Remove(id);
        public HashSet<string> GetUnlockedIds() => _unlocked;

        /// Jump to a specific entry and open the archive.
        public void OpenAtEntry(string entryId)
        {
            Open();
            if (_db == null) return;
            var entry = _db.GetById(entryId);
            if (entry != null) ShowDetail(entry);
        }

        // ─── Category Filtering ───────────────────────

        public void SelectCategory(int catIndex)
        {
            _activeCategory = catIndex < 0 ? (ArchiveCategory?)null : (ArchiveCategory)catIndex;
            UpdateTabVisuals(catIndex);
            RefreshList();
        }

        void UpdateTabVisuals(int selected)
        {
            if (categoryTabButtons == null) return;
            for (int i = 0; i < categoryTabButtons.Length; i++)
            {
                if (categoryTabButtons[i] == null) continue;
                var img = categoryTabButtons[i].GetComponent<Image>();
                if (img == null) continue;
                img.color = (i == selected)
                    ? new Color(0.95f, 0.82f, 0.35f, 0.9f)
                    : new Color(0.2f, 0.2f, 0.3f, 0.7f);
            }
        }

        void OnSearchChanged(string query)
        {
            _searchQuery = query;
            RefreshList();
        }

        // ─── Entry List ───────────────────────────────

        void RefreshList()
        {
            // Collect entries to display
            ArchiveEntry[] pool;
            if (!string.IsNullOrEmpty(_searchQuery))
                pool = _db?.Search(_searchQuery) ?? System.Array.Empty<ArchiveEntry>();
            else if (_activeCategory.HasValue)
                pool = _db?.GetByCategory(_activeCategory.Value) ?? System.Array.Empty<ArchiveEntry>();
            else
                pool = _db?.entries ?? System.Array.Empty<ArchiveEntry>();

            // Clear existing cards
            ClearCardPool();

            int visible = 0;
            foreach (var entry in pool)
            {
                if (entry == null) continue;
                SpawnCard(entry);
                visible++;
            }

            if (entryCountLabel != null)
                entryCountLabel.text = $"{visible} ENTRIES";
        }

        void ClearCardPool()
        {
            foreach (var go in _cardPool)
                if (go != null) Destroy(go);
            _cardPool.Clear();
        }

        void SpawnCard(ArchiveEntry entry)
        {
            bool unlocked = _unlocked.Contains(entry.entryId) || entry.unlockedByDefault;
            bool isNew     = _newBadge.Contains(entry.entryId);

            var cardGO = new GameObject($"Card_{entry.entryId}");
            cardGO.transform.SetParent(entryListContainer, false);
            _cardPool.Add(cardGO);

            var cardRT = cardGO.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0f, 56f);

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = unlocked
                ? new Color(0.08f, 0.08f, 0.20f, 0.85f)
                : new Color(0.06f, 0.06f, 0.10f, 0.65f);

            // Category colour strip (left edge)
            var stripGO = new GameObject("Strip");
            stripGO.transform.SetParent(cardGO.transform, false);
            var stripRT = stripGO.AddComponent<RectTransform>();
            stripRT.anchorMin = new Vector2(0f, 0f);
            stripRT.anchorMax = new Vector2(0.015f, 1f);
            stripRT.offsetMin = Vector2.zero;
            stripRT.offsetMax = Vector2.zero;
            var stripImg = stripGO.AddComponent<Image>();
            stripImg.color = CategoryColour(entry.category);

            // Title
            var titleGO = new GameObject("CardTitle");
            titleGO.transform.SetParent(cardGO.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.03f, 0.5f);
            titleRT.anchorMax = new Vector2(unlocked ? 0.85f : 0.95f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = unlocked ? entry.title : "??? LOCKED ENTRY ???";
            titleTMP.fontSize = 13;
            titleTMP.fontStyle = unlocked ? FontStyles.Normal : FontStyles.Italic;
            titleTMP.color = unlocked ? new Color(1f, 0.92f, 0.55f) : new Color(0.4f, 0.4f, 0.5f);
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Summary (unlocked only)
            if (unlocked)
            {
                var sumGO = new GameObject("CardSummary");
                sumGO.transform.SetParent(cardGO.transform, false);
                var sumRT = sumGO.AddComponent<RectTransform>();
                sumRT.anchorMin = new Vector2(0.03f, 0.0f);
                sumRT.anchorMax = new Vector2(0.95f, 0.5f);
                sumRT.offsetMin = Vector2.zero;
                sumRT.offsetMax = Vector2.zero;
                var sumTMP = sumGO.AddComponent<TextMeshProUGUI>();
                sumTMP.text = entry.summary;
                sumTMP.fontSize = 10;
                sumTMP.color = new Color(0.65f, 0.65f, 0.75f);
                sumTMP.overflowMode = TextOverflowModes.Ellipsis;
            }

            // "New" badge
            if (isNew && unlocked)
            {
                var badgeGO = new GameObject("NewBadge");
                badgeGO.transform.SetParent(cardGO.transform, false);
                var badgeRT = badgeGO.AddComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(0.87f, 0.2f);
                badgeRT.anchorMax = new Vector2(0.99f, 0.8f);
                badgeRT.offsetMin = Vector2.zero;
                badgeRT.offsetMax = Vector2.zero;
                var badgeTMP = badgeGO.AddComponent<TextMeshProUGUI>();
                badgeTMP.text = "✦ NEW";
                badgeTMP.fontSize = 11;
                badgeTMP.fontStyle = FontStyles.Bold;
                badgeTMP.color = new Color(0.95f, 0.82f, 0.35f);
                badgeTMP.alignment = TextAlignmentOptions.Right;
            }

            // Click handler (only if unlocked)
            if (unlocked)
            {
                var btn = cardGO.AddComponent<Button>();
                btn.targetGraphic = cardImg;
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.12f, 0.12f, 0.28f);
                btn.colors = colors;
                var captured = entry;
                btn.onClick.AddListener(() => { ShowDetail(captured); MarkSeen(captured.entryId); });
            }
        }

        // ─── Detail Panel ─────────────────────────────

        void ShowDetail(ArchiveEntry entry)
        {
            _selectedEntry = entry;

            if (detailTitle != null)
                detailTitle.text = entry.title;

            if (detailCategory != null)
                detailCategory.text = $"[ {entry.category.ToString().ToUpper()} ]";

            if (detailBody != null)
                detailBody.text = entry.fullText;

            if (anastasiaQuoteText != null)
            {
                bool hasQuote = !string.IsNullOrEmpty(entry.anastasiaQuote);
                anastasiaQuoteText.text = hasQuote
                    ? $"\u201c{entry.anastasiaQuote}\u201d\n\u2014 Anastasia"
                    : "";
                anastasiaQuoteText.gameObject.SetActive(hasQuote);
            }

            BuildRelatedLinks(entry);
        }

        void BuildRelatedLinks(ArchiveEntry entry)
        {
            if (relatedContainer == null) return;

            // Clear previous
            for (int i = relatedContainer.childCount - 1; i >= 0; i--)
                Destroy(relatedContainer.GetChild(i).gameObject);

            if (_db == null || entry.relatedEntryIds == null) return;

            foreach (var relId in entry.relatedEntryIds)
            {
                var rel = _db.GetById(relId);
                if (rel == null) continue;

                var btnGO = new GameObject($"Rel_{relId}");
                btnGO.transform.SetParent(relatedContainer, false);
                var btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(0f, 28f);
                var btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);
                var btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = btnImg;

                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(btnGO.transform, false);
                var lblRT = lblGO.AddComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = new Vector2(6f, 0f);
                lblRT.offsetMax = Vector2.zero;
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text = $"→ {rel.title}";
                lbl.fontSize = 11;
                lbl.color = new Color(0.7f, 0.82f, 1f);

                var captured = rel;
                btn.onClick.AddListener(() => ShowDetail(captured));
            }
        }

        // ─── Colour mapping ──────────────────────────

        static Color CategoryColour(ArchiveCategory cat) => cat switch
        {
            ArchiveCategory.Architecture => new Color(0.80f, 0.65f, 0.30f),
            ArchiveCategory.Technology   => new Color(0.30f, 0.70f, 0.95f),
            ArchiveCategory.Astronomy    => new Color(0.55f, 0.35f, 0.90f),
            ArchiveCategory.Culture      => new Color(0.80f, 0.40f, 0.60f),
            ArchiveCategory.Mystery      => new Color(0.40f, 0.80f, 0.55f),
            ArchiveCategory.Science      => new Color(0.95f, 0.75f, 0.20f),
            ArchiveCategory.People       => new Color(0.95f, 0.50f, 0.25f),
            ArchiveCategory.Evidence     => new Color(0.60f, 0.75f, 0.60f),
            _                            => Color.grey
        };
    }
}
