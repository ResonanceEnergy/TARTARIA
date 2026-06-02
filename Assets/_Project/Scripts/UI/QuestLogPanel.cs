using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// Screen-space-overlay quest log panel that replaces the old world-space floating
    /// objective text. Top-left corner, 360x280px, sortingOrder 30500 (above gameplay HUD
    /// at 30000, below pause / death overlays at 31000+).
    ///
    /// Subscribes to:
    ///   - <c>Tartaria.Core.GameEvents.OnQuestActivated</c> (static Action&lt;string&gt;)
    ///   - <c>Tartaria.Core.GameEvents.OnQuestCompleted</c> (static Action&lt;string&gt;)
    ///   - <c>Tartaria.Gameplay.QuestSystem.Instance.OnQuestActivated</c> (instance Action&lt;string&gt;)
    ///   - <c>Tartaria.Gameplay.QuestSystem.Instance.OnQuestCompleted</c> (instance Action&lt;string&gt;)
    ///
    /// Both event paths exist in the codebase (Core.GameEvents is the canonical fan-out,
    /// QuestSystem fires its own instance events for direct subscribers). We listen on both
    /// and de-dupe by questId so the same activation never produces two log entries.
    ///
    /// Built with legacy UnityEngine.UI.Text — no TextMeshPro dependency, per UI agent brief.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestLogPanel : MonoBehaviour
    {
        // ─────────────── Bootstrap ───────────────

        static QuestLogPanel _instance;
        const float PLAYER_WAIT_TIMEOUT_SEC = 5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            // Defer spawn until a Player-tagged GameObject exists, or we time out at 5s.
            // Echohaven scene init sometimes spawns the player asynchronously; spawning the
            // panel before then leaves it visible on the main menu / loading screen.
            var driver = new GameObject("QuestLogPanel_Bootstrap");
            DontDestroyOnLoad(driver);
            driver.AddComponent<QuestLogPanelBootstrapDriver>().StartCoroutine_Bootstrap();
        }

        sealed class QuestLogPanelBootstrapDriver : MonoBehaviour
        {
            public void StartCoroutine_Bootstrap() => StartCoroutine(WaitForPlayerThenSpawn());

            IEnumerator WaitForPlayerThenSpawn()
            {
                float t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < PLAYER_WAIT_TIMEOUT_SEC)
                {
                    GameObject player = null;
                    try { player = GameObject.FindGameObjectWithTag("Player"); }
                    catch (UnityException)
                    {
                        // "Player" tag not defined in this project's TagManager. Log loud
                        // (no silent fail per no-debt rule 3) and bail — we won't spawn.
                        Debug.LogWarning("[QuestLogPanel] 'Player' tag is not registered in TagManager — quest log will not spawn this session. Add the tag in Project Settings → Tags & Layers.");
                        Destroy(gameObject);
                        yield break;
                    }
                    if (player != null) break;
                    yield return null;
                }
                if (_instance == null)
                {
                    var go = new GameObject("QuestLogPanel");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<QuestLogPanel>();
                }
                Destroy(gameObject);
            }
        }

        // ─────────────── State ───────────────

        readonly List<QuestLogEntry> _active = new();
        readonly List<QuestLogEntry> _completed = new();
        readonly HashSet<string> _seenIds = new();
        bool _completedSectionExpanded = false;

        // ─────────────── Canvas / UI refs ───────────────

        Canvas _canvas;
        RectTransform _panelRoot;
        Text _activeHeader;
        Text _activeBody;
        Text _completedHeader;
        Text _completedBody;
        bool _uiDirty = true;

        const int CANVAS_SORTING_ORDER = 30500;
        const float PANEL_WIDTH = 360f;
        const float PANEL_HEIGHT = 280f;
        const int HEADER_FONT_SIZE = 18;
        const int ENTRY_FONT_SIZE = 14;

        // ─────────────── Lifecycle ───────────────

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            BuildCanvas();
            BuildPanel();
            SubscribeEvents();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
            UnsubscribeEvents();
        }

        void Update()
        {
            if (_uiDirty)
            {
                RefreshUI();
                _uiDirty = false;
            }
        }

        // ─────────────── Event subscription ───────────────

        void SubscribeEvents()
        {
            // GameEvents.OnQuestActivated / OnQuestCompleted are static Action<string>
            // declared in Tartaria.Core.GameEvents. We reach them by full name so this
            // file doesn't need to take a hard `using` on Core (keeps the asmdef dep
            // surface implicit through existing references).
            try
            {
                Tartaria.Core.GameEvents.OnQuestActivated += HandleQuestActivated;
                Tartaria.Core.GameEvents.OnQuestCompleted += HandleQuestCompleted;
            }
            catch (Exception ex)
            {
                // Per no-debt rule 3: log loud, then rethrow. Never swallow.
                Debug.LogError($"[QuestLogPanel] Failed to subscribe to GameEvents quest events: {ex.GetType().Name}: {ex.Message}");
                throw;
            }

            // QuestSystem also fires instance events. Subscribe so we catch quests
            // routed directly through QuestSystem.ActivateQuest / CompleteQuest without
            // going through GameEvents (some legacy systems still do this).
            try
            {
                var qs = Tartaria.Gameplay.QuestSystem.Instance;
                if (qs != null)
                {
                    qs.OnQuestActivated += HandleQuestActivated;
                    qs.OnQuestCompleted += HandleQuestCompleted;
                }
                else
                {
                    Debug.LogWarning("[QuestLogPanel] QuestSystem.Instance is null at subscribe time — relying on GameEvents fan-out only.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuestLogPanel] Failed to subscribe to QuestSystem instance events: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        void UnsubscribeEvents()
        {
            try
            {
                Tartaria.Core.GameEvents.OnQuestActivated -= HandleQuestActivated;
                Tartaria.Core.GameEvents.OnQuestCompleted -= HandleQuestCompleted;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QuestLogPanel] Failed to unsubscribe from GameEvents on teardown ({ex.GetType().Name}: {ex.Message}) — continuing teardown.");
            }

            try
            {
                var qs = Tartaria.Gameplay.QuestSystem.Instance;
                if (qs != null)
                {
                    qs.OnQuestActivated -= HandleQuestActivated;
                    qs.OnQuestCompleted -= HandleQuestCompleted;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QuestLogPanel] Failed to unsubscribe from QuestSystem instance events ({ex.GetType().Name}: {ex.Message}) — continuing teardown.");
            }
        }

        // ─────────────── Event handlers ───────────────

        void HandleQuestActivated(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning($"[QuestLogPanel] OnQuestActivated fired with null/empty questId. Active count: {_active.Count}");
                return;
            }
            if (_seenIds.Contains(questId))
            {
                // De-dupe: GameEvents and QuestSystem.Instance both fire for the same
                // activation. Silently ignore the second.
                return;
            }

            string title = questId;
            string description = string.Empty;
            try
            {
                var quest = Tartaria.Gameplay.QuestSystem.Instance?.GetQuest(questId);
                if (quest != null)
                {
                    title = string.IsNullOrEmpty(quest.title) ? questId : quest.title;
                    description = quest.description ?? string.Empty;
                }
                else
                {
                    // Per no-debt rule 4: surface the id + the count of currently active quests.
                    Debug.LogWarning($"[QuestLogPanel] OnQuestActivated for unknown questId '{questId}' (QuestSystem.GetQuest returned null). Active count: {_active.Count}. Adding placeholder entry.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QuestLogPanel] QuestSystem.GetQuest('{questId}') threw {ex.GetType().Name}: {ex.Message}. Active count: {_active.Count}. Adding placeholder entry.");
            }

            var entry = new QuestLogEntry(questId, title, description, Time.realtimeSinceStartup);
            _active.Add(entry);
            _seenIds.Add(questId);
            _uiDirty = true;
        }

        void HandleQuestCompleted(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning($"[QuestLogPanel] OnQuestCompleted fired with null/empty questId. Active count: {_active.Count}");
                return;
            }

            QuestLogEntry entry = null;
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].questId == questId) { entry = _active[i]; _active.RemoveAt(i); break; }
            }

            if (entry == null)
            {
                // Already completed? Or never activated through us? Either way, surface it.
                for (int i = 0; i < _completed.Count; i++)
                {
                    if (_completed[i].questId == questId)
                    {
                        // Idempotent — already in completed list, ignore.
                        return;
                    }
                }

                // Per no-debt rule 4: log warning with the id + active count.
                Debug.LogWarning($"[QuestLogPanel] OnQuestCompleted for unknown questId '{questId}' (no matching active entry). Active count: {_active.Count}. Synthesizing entry from QuestSystem lookup.");

                string title = questId;
                string description = string.Empty;
                try
                {
                    var quest = Tartaria.Gameplay.QuestSystem.Instance?.GetQuest(questId);
                    if (quest != null)
                    {
                        title = string.IsNullOrEmpty(quest.title) ? questId : quest.title;
                        description = quest.description ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[QuestLogPanel] QuestSystem.GetQuest('{questId}') threw on completion path: {ex.GetType().Name}: {ex.Message}.");
                }
                entry = new QuestLogEntry(questId, title, description, Time.realtimeSinceStartup);
                _seenIds.Add(questId);
            }

            entry.completed = true;
            entry.completionTime = Time.realtimeSinceStartup;
            _completed.Add(entry);
            _uiDirty = true;
        }

        // ─────────────── Public API (for debug / toggle) ───────────────

        /// <summary>Expand or collapse the "Completed" section. Default: collapsed.</summary>
        public void SetCompletedSectionExpanded(bool expanded)
        {
            if (_completedSectionExpanded == expanded) return;
            _completedSectionExpanded = expanded;
            _uiDirty = true;
        }

        /// <summary>Read-only snapshot of active entries. For tests / debug overlays.</summary>
        public IReadOnlyList<QuestLogEntry> ActiveEntries => _active;

        /// <summary>Read-only snapshot of completed entries.</summary>
        public IReadOnlyList<QuestLogEntry> CompletedEntries => _completed;

        // ─────────────── Canvas + panel construction ───────────────

        void BuildCanvas()
        {
            var canvasGo = new GameObject("QuestLogPanel_Canvas");
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
        }

        void BuildPanel()
        {
            // Top-left anchored panel.
            var panelGo = new GameObject("QuestLogPanel_Root", typeof(RectTransform));
            panelGo.transform.SetParent(_canvas.transform, worldPositionStays: false);
            _panelRoot = panelGo.GetComponent<RectTransform>();
            _panelRoot.anchorMin = new Vector2(0f, 1f);
            _panelRoot.anchorMax = new Vector2(0f, 1f);
            _panelRoot.pivot = new Vector2(0f, 1f);
            _panelRoot.anchoredPosition = new Vector2(16f, -16f); // 16px inset from top-left
            _panelRoot.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);

            // Semi-transparent black background.
            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            bg.raycastTarget = false;

            // ── Active section header ──
            _activeHeader = CreateTextChild(
                _panelRoot,
                "ActiveHeader",
                "Active Quests",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(10f, -8f),
                sizeDelta: new Vector2(-20f, 24f),
                fontSize: HEADER_FONT_SIZE,
                fontStyle: FontStyle.Bold,
                color: new Color(1f, 0.95f, 0.7f, 1f) // warm gold for the header
            );

            // ── Active body (list) ──
            _activeBody = CreateTextChild(
                _panelRoot,
                "ActiveBody",
                "(no active quests)",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(10f, -36f),
                sizeDelta: new Vector2(-20f, 160f),
                fontSize: ENTRY_FONT_SIZE,
                fontStyle: FontStyle.Normal,
                color: Color.white
            );

            // ── Completed section header ──
            _completedHeader = CreateTextChild(
                _panelRoot,
                "CompletedHeader",
                "Completed (0)",
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                pivot: new Vector2(0f, 0f),
                anchoredPosition: new Vector2(10f, 60f),
                sizeDelta: new Vector2(-20f, 22f),
                fontSize: HEADER_FONT_SIZE,
                fontStyle: FontStyle.Bold,
                color: new Color(0.75f, 0.75f, 0.75f, 1f)
            );

            // ── Completed body (collapsed by default) ──
            _completedBody = CreateTextChild(
                _panelRoot,
                "CompletedBody",
                string.Empty,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                pivot: new Vector2(0f, 0f),
                anchoredPosition: new Vector2(10f, 8f),
                sizeDelta: new Vector2(-20f, 50f),
                fontSize: ENTRY_FONT_SIZE,
                fontStyle: FontStyle.Normal,
                color: Color.gray // strike-through effect via gray + bracket markup in RefreshUI
            );
            _completedBody.gameObject.SetActive(false); // collapsed by default
        }

        static Text CreateTextChild(
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
            Color color)
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
            // Legacy UI requires a font asset; LegacyRuntime.ttf is the Unity 6 built-in
            // replacement for Arial.ttf (which was removed in 2022.2+). Falling back to
            // a manual resource load if the built-in resolver returns null — log loud if so.
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (Exception ex) { Debug.LogWarning($"[QuestLogPanel] LegacyRuntime.ttf builtin lookup threw {ex.GetType().Name}: {ex.Message} — trying Arial.ttf fallback."); }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (Exception ex) { Debug.LogWarning($"[QuestLogPanel] Arial.ttf fallback also failed: {ex.GetType().Name}: {ex.Message}"); }
            }
            if (font == null)
            {
                Debug.LogError($"[QuestLogPanel] No built-in font resolved for Text component '{name}'. Text will render with Unity's default null-font path and may appear blank.");
            }
            txt.font = font;
            txt.text = initialText;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = TextAnchor.UpperLeft;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.raycastTarget = false;
            return txt;
        }

        // ─────────────── Render ───────────────

        void RefreshUI()
        {
            // Active body
            if (_active.Count == 0)
            {
                _activeBody.text = "(no active quests)";
                _activeBody.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < _active.Count; i++)
                {
                    var e = _active[i];
                    sb.Append("- <b>").Append(e.title).Append("</b>");
                    if (!string.IsNullOrEmpty(e.description))
                    {
                        sb.Append("\n   ").Append(e.description);
                    }
                    if (i < _active.Count - 1) sb.Append('\n');
                }
                _activeBody.text = sb.ToString();
                _activeBody.color = Color.white;
                _activeBody.supportRichText = true;
            }

            // Completed header (always visible — shows count)
            _completedHeader.text = _completedSectionExpanded
                ? $"Completed ({_completed.Count}) [-]"
                : $"Completed ({_completed.Count}) [+]";

            // Completed body — visible only when expanded.
            _completedBody.gameObject.SetActive(_completedSectionExpanded);
            if (_completedSectionExpanded)
            {
                if (_completed.Count == 0)
                {
                    _completedBody.text = "(none yet)";
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < _completed.Count; i++)
                    {
                        var e = _completed[i];
                        // Strike-through via gray text + manual "~" bracket markup
                        // (UnityEngine.UI.Text has no native strike, but Color.gray is
                        // the spec'd treatment per the brief).
                        sb.Append("- ~").Append(e.title).Append("~");
                        if (i < _completed.Count - 1) sb.Append('\n');
                    }
                    _completedBody.text = sb.ToString();
                    _completedBody.color = Color.gray;
                    _completedBody.supportRichText = true;
                }
            }
        }
    }
}
