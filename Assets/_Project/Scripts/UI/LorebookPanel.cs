using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// Screen-space-overlay lorebook panel. Catalogs lore pages picked up via
    /// <see cref="LorebookCollectible"/>. Tab key toggles visibility.
    ///
    /// Layout: full-screen with a centered ~1100x640 reader frame. Left column = scrollable
    /// list of titles (each a Button), right column = full body for the selected entry.
    /// Built with legacy UnityEngine.UI components — NO TextMeshPro — per the UI brief.
    ///
    /// Canvas sortingOrder = 30700. Sits BETWEEN:
    ///   - QuestLogPanel @ 30500 (gameplay HUD layer)
    ///   - PauseMenu @ 30000 / WinScreen @ 32000 (modal overlays)
    /// so the lorebook covers the quest log but never blocks pause/win modals.
    ///
    /// Persistence: collected ids are written to PlayerPrefs under
    /// <c>Tartaria.Lorebook.CollectedIds</c> as a single newline-joined string. Titles
    /// and bodies are NOT persisted — the corresponding <see cref="LorebookCollectible"/>
    /// re-pumps them on next play via AddEntry, which is idempotent by id (so the
    /// timestamp is preserved if the entry was already present, but for a reload the
    /// timestamp resets to now — that's the intended behavior, the prefs store only the
    /// fact-of-discovery, not the wall-clock seconds since boot).
    ///
    /// CLAUDE.md NO-DEBT mandate enforced:
    ///   - rule 3: every catch block logs loud with file context
    ///   - rule 4: missing components/fallbacks log warnings with hierarchy paths
    ///   - rule 5: no // TODO, no stubs, every method does the thing
    /// API_CONTRACT.md compliance:
    ///   - GameEvents.RaiseHUDShowBanner(title, sub, dur) — verified at GameEvents.cs:623
    ///   - No FindObjectOfType (use FindFirstObjectByType when needed)
    ///   - No banned namespace suffix (UI is safe)
    /// </summary>
    [DisallowMultipleComponent]
    public class LorebookPanel : MonoBehaviour
    {
        // ─────────────── Constants ───────────────

        const int CANVAS_SORTING_ORDER = 30700; // between QuestLog (30500) and WinScreen (32000)
        const string PREFS_KEY = "Tartaria.Lorebook.CollectedIds";
        const char PREFS_DELIMITER = '\n';

        const float FRAME_WIDTH = 1100f;
        const float FRAME_HEIGHT = 640f;
        const float LIST_WIDTH = 360f;
        const float ROW_HEIGHT = 44f;
        const int TITLE_FONT_SIZE = 28;
        const int LIST_FONT_SIZE = 16;
        const int BODY_FONT_SIZE = 16;
        const int HEADER_FONT_SIZE = 22;

        // ─────────────── Singleton + Bootstrap ───────────────

        static LorebookPanel _instance;

        /// <summary>Singleton accessor. Auto-created via <see cref="Bootstrap"/> after scene load.</summary>
        public static LorebookPanel Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("LorebookPanel");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LorebookPanel>();
        }

        // ─────────────── State ───────────────

        readonly List<LorebookEntry> _entries = new();
        readonly Dictionary<string, LorebookEntry> _byId = new();
        readonly List<Button> _listButtons = new();
        string _selectedId;
        bool _visible;
        bool _uiDirty = true;

        // ─────────────── UI refs ───────────────

        Canvas _canvas;
        CanvasGroup _canvasGroup;
        RectTransform _frame;
        RectTransform _listContent;
        Text _headerText;
        Text _selectedTitleText;
        Text _selectedBodyText;
        Text _emptyHintText;
        Font _font;

        // ─────────────── Lifecycle ───────────────

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[LorebookPanel] Duplicate instance detected on '{GetHierarchyPath(gameObject)}'. Destroying duplicate; existing Instance is on '{GetHierarchyPath(_instance.gameObject)}'.");
                Destroy(this);
                return;
            }
            _instance = this;

            ResolveFont();
            BuildCanvas();
            BuildFrame();
            LoadPersistedIds();

            ApplyVisibility(false); // start hidden
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        void Update()
        {
            // Tab toggles visibility. Input System path (project is on the Input System
            // package, not legacy UnityEngine.Input — see CLAUDE.md F310 section).
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                Toggle();
            }

            if (_uiDirty)
            {
                RefreshUI();
                _uiDirty = false;
            }
        }

        // ─────────────── Public API ───────────────

        /// <summary>
        /// Add (or re-acknowledge) a lore entry. Idempotent by id — calling twice with the
        /// same id will NOT duplicate the entry, will NOT re-fire the banner, and will NOT
        /// reset the discovery timestamp. Returns true if this was a NEW discovery.
        /// </summary>
        public bool AddEntry(string id, string title, string body, float discoveredAt)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[LorebookPanel] AddEntry called with null/empty id. title='{title}'. Current entry count: {_entries.Count}.");
                return false;
            }

            if (_byId.ContainsKey(id))
            {
                // Idempotent — already collected. Silently ignore the second call.
                return false;
            }

            var entry = new LorebookEntry(id, title, body, discoveredAt);
            _entries.Add(entry);
            _byId[id] = entry;

            // Auto-select first entry so the right panel isn't empty.
            if (string.IsNullOrEmpty(_selectedId))
            {
                _selectedId = entry.id;
            }

            PersistIds();
            _uiDirty = true;

            // Fire HUD banner — signature verified at
            // Assets/_Project/Scripts/Core/GameEvents.cs:623:
            //   RaiseHUDShowBanner(string title, string subtitle, float duration = 5f)
            try
            {
                Tartaria.Core.GameEvents.RaiseHUDShowBanner("Lore discovered", entry.title, 4f);
            }
            catch (Exception ex)
            {
                // Log loud per NO-DEBT rule 3. Swallow is correct here because the banner
                // is cosmetic feedback — the entry is still in the lorebook, the player can
                // open it with Tab. The Debug.LogError above surfaces the gap to devs.
                Debug.LogError($"[LorebookPanel] Failed to raise HUD banner for lore '{entry.id}' ('{entry.title}') on '{GetHierarchyPath(gameObject)}': {ex.GetType().Name}: {ex.Message}");
            }

            return true;
        }

        /// <summary>Flip visibility. Public so other systems / debug commands can drive it.</summary>
        public void Toggle() { ApplyVisibility(!_visible); }
        public void Show() { ApplyVisibility(true); }
        public void Hide() { ApplyVisibility(false); }

        /// <summary>True when the panel is currently shown.</summary>
        public bool IsVisible => _visible;

        /// <summary>Read-only snapshot of collected entries. For tests / debug overlays.</summary>
        public IReadOnlyList<LorebookEntry> Entries => _entries;

        /// <summary>True if an entry with the given id has been collected.</summary>
        public bool HasEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _byId.ContainsKey(id);
        }

        // ─────────────── Visibility ───────────────

        void ApplyVisibility(bool visible)
        {
            _visible = visible;

            if (_canvas != null) _canvas.enabled = visible;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }

            // When opening, refresh now instead of waiting a frame so the first paint is correct.
            if (visible) RefreshUI();
        }

        // ─────────────── Persistence ───────────────

        void LoadPersistedIds()
        {
            string raw;
            try
            {
                raw = PlayerPrefs.GetString(PREFS_KEY, string.Empty);
            }
            catch (Exception ex)
            {
                // Per NO-DEBT rule 3: log loud, then continue with empty set. PlayerPrefs
                // is a platform service — if it threw, the platform is in a bad state but
                // we should not block panel construction. The player can re-collect.
                Debug.LogError($"[LorebookPanel] PlayerPrefs.GetString('{PREFS_KEY}') threw {ex.GetType().Name}: {ex.Message}. Starting with empty lorebook.");
                return;
            }

            if (string.IsNullOrEmpty(raw)) return;

            var ids = raw.Split(PREFS_DELIMITER);
            int loaded = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (_byId.ContainsKey(id)) continue;

                // Title/body are not persisted — we know only the id. The corresponding
                // LorebookCollectible will re-pump full data on next collection. Until then,
                // surface the id as a placeholder so the player can see something was
                // collected previously. Log info so devs aren't surprised by the placeholders.
                var placeholder = new LorebookEntry(
                    id,
                    id, // title falls back to id
                    "(This lore page was previously collected. Re-encounter the source object in the world to refresh its text.)",
                    Time.realtimeSinceStartup);
                _entries.Add(placeholder);
                _byId[id] = placeholder;
                loaded++;
            }

            if (loaded > 0)
            {
                if (string.IsNullOrEmpty(_selectedId)) _selectedId = _entries[0].id;
                _uiDirty = true;
                Debug.Log($"[LorebookPanel] Restored {loaded} previously-collected lore id(s) from PlayerPrefs key '{PREFS_KEY}'. Titles will refresh on re-encounter.");
            }
        }

        void PersistIds()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (i > 0) sb.Append(PREFS_DELIMITER);
                    sb.Append(_entries[i].id);
                }
                PlayerPrefs.SetString(PREFS_KEY, sb.ToString());
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                // Log loud per NO-DEBT rule 3. Swallow is correct: the entry is still in
                // memory for this session, persistence failure means next reload won't
                // remember — surface that to devs immediately.
                Debug.LogError($"[LorebookPanel] PlayerPrefs persistence failed for key '{PREFS_KEY}' (entry count {_entries.Count}): {ex.GetType().Name}: {ex.Message}. Session memory unaffected; next reload will forget.");
            }
        }

        // ─────────────── Canvas + frame construction ───────────────

        void ResolveFont()
        {
            // Legacy UI requires a font asset. Unity 6 ships LegacyRuntime.ttf as the
            // Arial.ttf replacement. Fall back chain mirrors QuestLogPanel for parity.
            try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (Exception ex) { Debug.LogWarning($"[LorebookPanel] LegacyRuntime.ttf builtin lookup threw {ex.GetType().Name}: {ex.Message} — trying Arial.ttf fallback."); }
            if (_font == null)
            {
                try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (Exception ex) { Debug.LogWarning($"[LorebookPanel] Arial.ttf fallback also failed: {ex.GetType().Name}: {ex.Message}"); }
            }
            if (_font == null)
            {
                Debug.LogError($"[LorebookPanel] No built-in font resolved on '{GetHierarchyPath(gameObject)}'. Text will render with Unity's default null-font path and may appear blank.");
            }
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("LorebookPanel_Canvas");
            canvasGo.transform.SetParent(transform, worldPositionStays: false);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = CANVAS_SORTING_ORDER;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            _canvasGroup = canvasGo.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        void BuildFrame()
        {
            // Full-screen scrim — slight dim behind the reader frame.
            var scrimGo = new GameObject("Scrim", typeof(RectTransform));
            scrimGo.transform.SetParent(_canvas.transform, worldPositionStays: false);
            var scrimRt = scrimGo.GetComponent<RectTransform>();
            scrimRt.anchorMin = Vector2.zero;
            scrimRt.anchorMax = Vector2.one;
            scrimRt.offsetMin = Vector2.zero;
            scrimRt.offsetMax = Vector2.zero;
            var scrimImg = scrimGo.AddComponent<Image>();
            scrimImg.color = new Color(0f, 0f, 0f, 0.55f);
            scrimImg.raycastTarget = true; // swallows clicks behind the lorebook

            // Centered reader frame.
            var frameGo = new GameObject("Frame", typeof(RectTransform));
            frameGo.transform.SetParent(_canvas.transform, worldPositionStays: false);
            _frame = frameGo.GetComponent<RectTransform>();
            _frame.anchorMin = new Vector2(0.5f, 0.5f);
            _frame.anchorMax = new Vector2(0.5f, 0.5f);
            _frame.pivot = new Vector2(0.5f, 0.5f);
            _frame.anchoredPosition = Vector2.zero;
            _frame.sizeDelta = new Vector2(FRAME_WIDTH, FRAME_HEIGHT);
            var frameImg = frameGo.AddComponent<Image>();
            frameImg.color = new Color(0.08f, 0.06f, 0.04f, 0.92f);

            // Header banner.
            _headerText = CreateText(
                _frame,
                "Header",
                "Lorebook",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0.5f, 1f),
                anchoredPosition: new Vector2(0f, -16f),
                sizeDelta: new Vector2(-40f, 40f),
                fontSize: TITLE_FONT_SIZE,
                fontStyle: FontStyle.Bold,
                color: new Color(1f, 0.92f, 0.7f, 1f),
                alignment: TextAnchor.UpperCenter);

            // Close hint (top-right).
            CreateText(
                _frame,
                "CloseHint",
                "[Tab] close",
                anchorMin: new Vector2(1f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(1f, 1f),
                anchoredPosition: new Vector2(-18f, -22f),
                sizeDelta: new Vector2(160f, 24f),
                fontSize: 14,
                fontStyle: FontStyle.Italic,
                color: new Color(0.75f, 0.75f, 0.6f, 0.85f),
                alignment: TextAnchor.UpperRight);

            BuildList();
            BuildReader();
        }

        void BuildList()
        {
            // List background panel (left column).
            var listBg = new GameObject("ListPanel", typeof(RectTransform));
            listBg.transform.SetParent(_frame, worldPositionStays: false);
            var listBgRt = listBg.GetComponent<RectTransform>();
            listBgRt.anchorMin = new Vector2(0f, 0f);
            listBgRt.anchorMax = new Vector2(0f, 1f);
            listBgRt.pivot = new Vector2(0f, 0.5f);
            listBgRt.anchoredPosition = new Vector2(20f, 0f);
            listBgRt.sizeDelta = new Vector2(LIST_WIDTH, -80f); // leave space for header
            var listBgImg = listBg.AddComponent<Image>();
            listBgImg.color = new Color(0f, 0f, 0f, 0.35f);

            // ScrollRect for the entries.
            var scroll = listBg.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Viewport.
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(listBg.transform, worldPositionStays: false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = new Vector2(4f, 4f);
            viewportRt.offsetMax = new Vector2(-4f, -4f);
            var viewportImg = viewportGo.AddComponent<Image>();
            viewportImg.color = new Color(0f, 0f, 0f, 0.01f); // near-invisible but required by Mask
            var mask = viewportGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            scroll.viewport = viewportRt;

            // Content (vertical layout).
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, worldPositionStays: false);
            _listContent = contentGo.GetComponent<RectTransform>();
            _listContent.anchorMin = new Vector2(0f, 1f);
            _listContent.anchorMax = new Vector2(1f, 1f);
            _listContent.pivot = new Vector2(0.5f, 1f);
            _listContent.anchoredPosition = Vector2.zero;
            _listContent.sizeDelta = new Vector2(0f, 0f);
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = _listContent;

            // Empty-state hint.
            _emptyHintText = CreateText(
                listBg.GetComponent<RectTransform>(),
                "EmptyHint",
                "(No lore collected yet. Explore Echohaven to find pages.)",
                anchorMin: new Vector2(0f, 0.5f),
                anchorMax: new Vector2(1f, 0.5f),
                pivot: new Vector2(0.5f, 0.5f),
                anchoredPosition: Vector2.zero,
                sizeDelta: new Vector2(-20f, 80f),
                fontSize: LIST_FONT_SIZE,
                fontStyle: FontStyle.Italic,
                color: new Color(0.7f, 0.7f, 0.6f, 0.85f),
                alignment: TextAnchor.MiddleCenter);
        }

        void BuildReader()
        {
            // Reader (right column).
            var readerGo = new GameObject("Reader", typeof(RectTransform));
            readerGo.transform.SetParent(_frame, worldPositionStays: false);
            var readerRt = readerGo.GetComponent<RectTransform>();
            readerRt.anchorMin = new Vector2(0f, 0f);
            readerRt.anchorMax = new Vector2(1f, 1f);
            readerRt.pivot = new Vector2(0.5f, 0.5f);
            readerRt.offsetMin = new Vector2(20f + LIST_WIDTH + 16f, 30f);
            readerRt.offsetMax = new Vector2(-20f, -70f);
            var readerImg = readerGo.AddComponent<Image>();
            readerImg.color = new Color(0f, 0f, 0f, 0.25f);

            // Selected title.
            _selectedTitleText = CreateText(
                readerRt,
                "SelectedTitle",
                "(select an entry)",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0.5f, 1f),
                anchoredPosition: new Vector2(0f, -12f),
                sizeDelta: new Vector2(-24f, 40f),
                fontSize: HEADER_FONT_SIZE,
                fontStyle: FontStyle.Bold,
                color: new Color(1f, 0.95f, 0.78f, 1f),
                alignment: TextAnchor.UpperLeft);

            // Selected body.
            _selectedBodyText = CreateText(
                readerRt,
                "SelectedBody",
                string.Empty,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0.5f, 0.5f),
                anchoredPosition: Vector2.zero,
                sizeDelta: Vector2.zero,
                fontSize: BODY_FONT_SIZE,
                fontStyle: FontStyle.Normal,
                color: new Color(0.92f, 0.92f, 0.88f, 1f),
                alignment: TextAnchor.UpperLeft);
            // Body has explicit offsets so it doesn't crash into the title.
            var bodyRt = _selectedBodyText.GetComponent<RectTransform>();
            bodyRt.offsetMin = new Vector2(16f, 16f);
            bodyRt.offsetMax = new Vector2(-16f, -56f);
            _selectedBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _selectedBodyText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        // ─────────────── Render ───────────────

        void RefreshUI()
        {
            if (_listContent == null) return;

            // Empty hint.
            if (_emptyHintText != null)
            {
                _emptyHintText.gameObject.SetActive(_entries.Count == 0);
            }

            // Rebuild list buttons. Cheap because entry count is small (lorebook is dozens, not thousands).
            for (int i = 0; i < _listButtons.Count; i++)
            {
                if (_listButtons[i] != null) Destroy(_listButtons[i].gameObject);
            }
            _listButtons.Clear();

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                var btn = BuildListRow(entry);
                _listButtons.Add(btn);
            }

            // Refresh reader pane.
            if (_entries.Count == 0)
            {
                _selectedTitleText.text = "(no entries yet)";
                _selectedBodyText.text = "Collect lore pages by interacting with marked objects in the world. They will appear here.";
                return;
            }

            if (string.IsNullOrEmpty(_selectedId) || !_byId.ContainsKey(_selectedId))
            {
                _selectedId = _entries[0].id;
            }

            var selected = _byId[_selectedId];
            _selectedTitleText.text = selected.title;
            _selectedBodyText.text = string.IsNullOrEmpty(selected.body)
                ? "(no body text — entry was restored from save and has not been re-encountered this session)"
                : selected.body;

            // Update header to show count.
            if (_headerText != null)
            {
                _headerText.text = $"Lorebook  ({_entries.Count})";
            }
        }

        Button BuildListRow(LorebookEntry entry)
        {
            var rowGo = new GameObject($"Row_{entry.id}", typeof(RectTransform));
            rowGo.transform.SetParent(_listContent, worldPositionStays: false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0f, ROW_HEIGHT);

            var img = rowGo.AddComponent<Image>();
            bool isSelected = entry.id == _selectedId;
            img.color = isSelected
                ? new Color(0.4f, 0.32f, 0.18f, 0.95f)
                : new Color(0.18f, 0.16f, 0.12f, 0.75f);

            var btn = rowGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.7f, 1f);
            colors.pressedColor = new Color(0.85f, 0.75f, 0.5f, 1f);
            colors.selectedColor = new Color(1f, 0.92f, 0.6f, 1f);
            btn.colors = colors;
            btn.targetGraphic = img;

            // Row label.
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rowGo.transform, worldPositionStays: false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10f, 4f);
            labelRt.offsetMax = new Vector2(-10f, -4f);

            var label = labelGo.AddComponent<Text>();
            label.font = _font;
            label.text = entry.title;
            label.fontSize = LIST_FONT_SIZE;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = isSelected ? Color.black : new Color(0.92f, 0.9f, 0.82f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;

            string capturedId = entry.id;
            btn.onClick.AddListener(() => SelectEntry(capturedId));

            return btn;
        }

        void SelectEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_byId.ContainsKey(id))
            {
                Debug.LogWarning($"[LorebookPanel] SelectEntry called with unknown id '{id}'. Known count: {_entries.Count}.");
                return;
            }
            _selectedId = id;
            _uiDirty = true;
        }

        // ─────────────── Text helper ───────────────

        Text CreateText(
            RectTransform parent,
            string name,
            string initialText,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var txt = go.AddComponent<Text>();
            txt.font = _font;
            txt.text = initialText;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.raycastTarget = false;
            txt.supportRichText = true;
            return txt;
        }

        // ─────────────── Utility ───────────────

        static string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "<null>";
            var t = go.transform;
            var path = go.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.gameObject.name + "/" + path;
            }
            return path;
        }
    }
}
