using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// World Map UI — displays the 13-Moon campaign map and zone states.
    ///
    /// Design per GDD §26 (Level Design) + §03 (Campaign 13 Moons):
    ///   - Constellation-style map with 13 zones as stars
    ///   - Lines connect zones showing progression path
    ///   - Zone states: Locked, Available, Active, Completed
    ///   - Click zone → show detail panel (name, desc, RS requirement, modifiers)
    ///   - Active zone pulses golden
    ///   - Completed zones glow bright, locked zones dim
    ///
    /// Also includes the Codex viewer for collected lore entries.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldMapUI : MonoBehaviour
    {
        public static WorldMapUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] GameObject mapPanel;
        [SerializeField] Button closeButton;

        [Header("Zone Node Template")]
        [SerializeField] GameObject zoneNodePrefab;
        [SerializeField] Transform nodeContainer;

        [Header("Detail Panel")]
        [SerializeField] TextMeshProUGUI zoneName;
        [SerializeField] TextMeshProUGUI zoneDescription;
        [SerializeField] TextMeshProUGUI zoneStatus;
        [SerializeField] TextMeshProUGUI zoneMoonIndex;
        [SerializeField] Button travelButton;

        [Header("Codex Tab")]
        [SerializeField] Button codexTabButton;
        [SerializeField] Button mapTabButton;
        [SerializeField] GameObject codexPanel;

        [Header("Colors")]
        [SerializeField] Color lockedColor = new(0.25f, 0.25f, 0.3f, 0.5f);
        [SerializeField] Color availableColor = new(0.5f, 0.5f, 0.6f, 0.8f);
        [SerializeField] Color activeColor = new(0.95f, 0.82f, 0.35f, 1f);
        [SerializeField] Color completedColor = new(0.2f, 0.8f, 0.4f, 1f);

        bool _isOpen;
        int _selectedZone = -1;
        readonly Dictionary<int, ZoneNodeWidget> _zoneWidgets = new();

        // Zone layout: constellation positions (normalized 0-1 screen space)
        static readonly Vector2[] ZonePositions =
        {
            new(0.15f, 0.25f),  // Moon 1: Echohaven
            new(0.30f, 0.15f),  // Moon 2: Crystalline Caverns
            new(0.45f, 0.30f),  // Moon 3: Celestial Spire
            new(0.25f, 0.45f),  // Moon 4: Iron Colosseum
            new(0.55f, 0.50f),  // Moon 5: Aether Falls
            new(0.40f, 0.60f),  // Moon 6: Builder's Reach
            new(0.60f, 0.35f),  // Moon 7: Harmonic Gardens
            new(0.70f, 0.55f),  // Moon 8: Void Cradle
            new(0.55f, 0.70f),  // Moon 9: Frequency Peaks
            new(0.75f, 0.40f),  // Moon 10: Mirror Sanctum
            new(0.80f, 0.65f),  // Moon 11: Tesla's Workshop
            new(0.65f, 0.80f),  // Moon 12: The Grand Dome
            new(0.50f, 0.90f),  // Moon 13: Tartaria Prime
        };

        static readonly string[] ZoneNames =
        {
            "Echohaven", "Crystalline Caverns", "Celestial Spire",
            "Iron Colosseum", "Aether Falls", "Builder's Reach",
            "Harmonic Gardens", "Void Cradle", "Frequency Peaks",
            "Mirror Sanctum", "Tesla's Workshop", "The Grand Dome",
            "Tartaria Prime"
        };

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (travelButton != null) travelButton.onClick.AddListener(OnTravelClicked);
            if (codexTabButton != null) codexTabButton.onClick.AddListener(ShowCodex);
            if (mapTabButton != null) mapTabButton.onClick.AddListener(ShowMap);
            Close();
        }

        void Update()
        {
            if (!_isOpen) return;
            PulseActiveZone();
        }

        // ─── Open / Close ────────────────────────────

        public void Open()
        {
            _isOpen = true;
            if (mapPanel != null) mapPanel.SetActive(true);
            _selectedZone = -1;
            RebuildMap();
            ClearDetail();
            ShowMap();
            Audio.AudioManager.Instance?.PlaySFX2D("MapOpen");
        }

        public void Close()
        {
            _isOpen = false;
            if (mapPanel != null) mapPanel.SetActive(false);
            Audio.AudioManager.Instance?.PlaySFX2D("UIClose");
        }

        public void Toggle() { if (_isOpen) Close(); else Open(); }

        public bool IsOpen => _isOpen;

        // ─── Map Display ─────────────────────────────

        void RebuildMap()
        {
            var campaign = ServiceLocator.Campaign;
            int currentMoon = campaign?.CurrentMoonIndex ?? 0;

            // Create widgets once, then just refresh state on subsequent opens
            if (_zoneWidgets.Count == 13)
            {
                RefreshWidgetStates(currentMoon);
                return;
            }

            foreach (var w in _zoneWidgets.Values)
                if (w.go != null) Destroy(w.go);
            _zoneWidgets.Clear();

            for (int i = 0; i < 13; i++)
            {
                var widget = CreateZoneNode(i, currentMoon);
                _zoneWidgets[i] = widget;
            }
        }

        void RefreshWidgetStates(int currentMoon)
        {
            for (int i = 0; i < 13; i++)
            {
                if (!_zoneWidgets.TryGetValue(i, out var widget)) continue;
                ZoneState state;
                if (i < currentMoon) state = ZoneState.Completed;
                else if (i == currentMoon) state = ZoneState.Active;
                else if (i == currentMoon + 1) state = ZoneState.Available;
                else state = ZoneState.Locked;
                widget.state = state;
                ApplyNodeVisuals(widget, i, state);
            }
        }

        ZoneNodeWidget CreateZoneNode(int index, int currentMoon)
        {
            var widget = new ZoneNodeWidget();
            ZoneState state;
            if (index < currentMoon) state = ZoneState.Completed;
            else if (index == currentMoon) state = ZoneState.Active;
            else if (index == currentMoon + 1) state = ZoneState.Available;
            else state = ZoneState.Locked;

            widget.state = state;

            if (zoneNodePrefab != null && nodeContainer != null)
            {
                widget.go = Instantiate(zoneNodePrefab, nodeContainer);
            }
            else
            {
                widget.go = new GameObject($"Zone_{index}");
                widget.go.transform.SetParent(nodeContainer ?? transform, false);
                widget.go.AddComponent<RectTransform>();
                widget.image = widget.go.AddComponent<Image>();
                widget.go.AddComponent<Button>();
                var lbl = new GameObject("Label");
                lbl.transform.SetParent(widget.go.transform, false);
                lbl.AddComponent<RectTransform>();
                widget.label = lbl.AddComponent<TextMeshProUGUI>();
            }

            if (widget.image == null) widget.image = widget.go.GetComponent<Image>();
            if (widget.label == null) widget.label = widget.go.GetComponentInChildren<TextMeshProUGUI>();

            var rt = widget.go.GetComponent<RectTransform>();
            if (rt != null)
            {
                var parentRT = (nodeContainer ?? transform) as RectTransform;
                float w2 = parentRT != null ? parentRT.rect.width : 800f;
                float h = parentRT != null ? parentRT.rect.height : 600f;
                rt.anchoredPosition = new Vector2(
                    ZonePositions[index].x * w2 - w2 * 0.5f,
                    ZonePositions[index].y * h - h * 0.5f);
                rt.sizeDelta = new Vector2(80f, 40f);
            }

            if (widget.label != null)
            {
                widget.label.text = $"{index + 1}";
                widget.label.fontSize = 14f;
                widget.label.alignment = TextAlignmentOptions.Center;
            }

            ApplyNodeVisuals(widget, index, state);

            int zoneIndex = index;
            var btn = widget.go.GetComponent<Button>();
            btn?.onClick.AddListener(() => SelectZone(zoneIndex));

            return widget;
        }

        void ApplyNodeVisuals(ZoneNodeWidget widget, int index, ZoneState state)
        {
            if (widget.image != null)
            {
                widget.image.color = state switch
                {
                    ZoneState.Completed => completedColor,
                    ZoneState.Active => activeColor,
                    ZoneState.Available => availableColor,
                    _ => lockedColor
                };
            }
        }

        void PulseActiveZone()
        {
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 2.5f);
            foreach (var kvp in _zoneWidgets)
            {
                if (kvp.Value.state == ZoneState.Active && kvp.Value.image != null)
                {
                    var c = activeColor;
                    c.a = pulse;
                    kvp.Value.image.color = c;
                }
            }
        }

        // ─── Zone Selection ──────────────────────────

        void SelectZone(int index)
        {
            _selectedZone = index;
            Audio.AudioManager.Instance?.PlaySFX2D("ZoneSelected");
            var campaign = ServiceLocator.Campaign;
            int currentMoon = campaign?.CurrentMoonIndex ?? 0;

            if (zoneName != null) zoneName.text = ZoneNames[index];
            if (zoneMoonIndex != null) zoneMoonIndex.text = $"Moon {index + 1} of 13";

            string desc = index < currentMoon ? "Restored" :
                index == currentMoon ? "Current zone — in progress" :
                "Locked — complete the current Moon to advance";
            if (zoneDescription != null) zoneDescription.text = desc;

            string status = index < currentMoon ? "COMPLETED" :
                index == currentMoon ? "ACTIVE" :
                index == currentMoon + 1 ? "AVAILABLE" : "LOCKED";
            if (zoneStatus != null) zoneStatus.text = status;

            if (travelButton != null)
                travelButton.interactable = (index == currentMoon || index < currentMoon);
        }

        void OnTravelClicked()
        {
            if (_selectedZone < 0) return;
            ServiceLocator.ZoneTransition?.TransitionToZone(_selectedZone);
            Close();
        }

        void ClearDetail()
        {
            if (zoneName != null) zoneName.text = "Select a zone";
            if (zoneDescription != null) zoneDescription.text = "";
            if (zoneStatus != null) zoneStatus.text = "";
            if (zoneMoonIndex != null) zoneMoonIndex.text = "";
            if (travelButton != null) travelButton.interactable = false;
        }

        void ShowMap()
        {
            if (mapPanel != null) mapPanel.SetActive(true);
            if (codexPanel != null) codexPanel.SetActive(false);
        }

        void ShowCodex()
        {
            if (mapPanel != null) mapPanel.SetActive(false);
            if (codexPanel != null) codexPanel.SetActive(true);
            CodexSystem.Instance?.RefreshUI();
        }

        // ─── Types ───
        enum ZoneState { Locked, Available, Active, Completed }

        class ZoneNodeWidget
        {
            public GameObject go;
            public Image image;
            public TextMeshProUGUI label;
            public ZoneState state;
        }
    }

    /// <summary>
    /// Codex System — collectable lore encyclopedia.
    ///
    /// Categories: History, Technology, Characters, Frequencies, Locations, Artifacts
    /// Entries unlock through gameplay: discovering buildings, NPC dialogue, quest rewards.
    /// </summary>
    [DisallowMultipleComponent]
    public class CodexSystem : MonoBehaviour
    {
        public static CodexSystem Instance { get; private set; }

        public event System.Action<string> OnEntryUnlocked; // entryId

        readonly Dictionary<string, CodexEntry> _entries = new();
        readonly HashSet<string> _unlockedIds = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildCodex();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ──────────────────────────────

        public void UnlockEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return;
            if (_unlockedIds.Contains(entryId)) return;
            if (!_entries.ContainsKey(entryId)) return;
            _unlockedIds.Add(entryId);
            OnEntryUnlocked?.Invoke(entryId);
            Debug.Log($"[Codex] Unlocked: {_entries[entryId].title}");
        }

        public bool IsUnlocked(string entryId) => !string.IsNullOrEmpty(entryId) && _unlockedIds.Contains(entryId);

        public CodexEntry GetEntry(string entryId) =>
            !string.IsNullOrEmpty(entryId) && _entries.TryGetValue(entryId, out var e) ? e : null;

        public int TotalEntries => _entries.Count;
        public int UnlockedCount => _unlockedIds.Count;
        public float CompletionPercent => _entries.Count > 0 ? (float)_unlockedIds.Count / _entries.Count : 0f;

        public IReadOnlyCollection<string> GetUnlockedIds() => _unlockedIds;

        public List<CodexEntry> GetEntriesByCategory(CodexCategory cat)
        {
            var result = new List<CodexEntry>();
            foreach (var kvp in _entries)
                if (kvp.Value.category == cat &&_unlockedIds.Contains(kvp.Key))
                    result.Add(kvp.Value);
            return result;
        }

        /// <summary>Refresh the Codex UI panel (called by WorldMapUI).</summary>
        public void RefreshUI()
        {
            // UI refresh handled by WorldMapUI's codex tab
        }

        // ─── Save / Load ────────────────────────────

        public CodexSaveData GetSaveData()
        {
            return new CodexSaveData
            {
                unlockedEntryIds = new List<string>(_unlockedIds)
            };
        }

        public void LoadSaveData(CodexSaveData data)
        {
            _unlockedIds.Clear();
            if (data?.unlockedEntryIds != null)
                foreach (var id in data.unlockedEntryIds)
                    _unlockedIds.Add(id);
        }

        // ─── Codex Database ─────────────────────────

        void BuildCodex()
        {
            // ── History ──
            Add("hist_tartaria_overview", CodexCategory.History, "The Tartarian Civilization",
                "A global civilization of extraordinary architectural and technological achievement. Their buildings channeled Aether energy through precise geometric proportions based on the golden ratio (φ = 1.618). Systematically buried and erased from history.");
            Add("hist_world_fairs", CodexCategory.History, "The World Fairs",
                "Grand exhibitions held in the 19th-20th centuries that displayed 'temporary' buildings of impossible grandeur. Official history claims they were built in months for exhibitions, then demolished. In truth, they were Tartarian structures being 'showcased' before their scheduled destruction.");
            Add("hist_the_burial", CodexCategory.History, "The Great Burial",
                "The systematic covering of first floors worldwide with meters of sediment. Officially attributed to 'ground level changes.' In reality, a coordinated effort to bury the Aether conduits that powered Tartarian technology.");
            Add("hist_mud_flood", CodexCategory.History, "The Mud Flood Theory",
                "Buildings across Europe and the Americas show evidence of buried first floors, now basement levels. Door frames buried in earth, windows half-underground. The scale suggests a deliberate, worldwide event.");

            // ── Technology ──
            Add("tech_aether_energy", CodexCategory.Technology, "Aether Energy",
                "The medium through which Tartarian technology operated. Not energy itself, but the substrate upon which energy propagates — like water carrying waves. Channeled through geometric conduits and crystal resonators.");
            Add("tech_golden_ratio", CodexCategory.Technology, "The Golden Ratio in Architecture",
                "φ (1.618...) appears throughout Tartarian buildings. Door heights, window spacing, dome curvature — all follow golden proportions. This wasn't aesthetic preference; it was functional engineering for Aether conductivity.");
            Add("tech_frequency_healing", CodexCategory.Technology, "Frequency-Based Restoration",
                "Specific frequencies interact with Tartarian materials: 432 Hz (harmonic alignment), 528 Hz (transformation), 7.83 Hz (Earth resonance), 1296 Hz (celestial connection). Tuning to these frequencies activates dormant Aether circuits.");
            Add("tech_tesla_369", CodexCategory.Technology, "Tesla's 3-6-9 Pattern",
                "'If you only knew the magnificence of 3, 6, and 9...' - Nikola Tesla. The ley line network follows this geometry. Three primary nodes form triangles, six secondary nodes hexagons, nine tertiary nodes complete the grid.");
            Add("tech_ley_lines", CodexCategory.Technology, "Ley Line Network",
                "Underground energy pathways connecting Tartarian structures. When buildings are restored, their ley line connections reactivate, forming a network that amplifies Aether flow across the entire zone.");

            // ── Characters ──
            Add("char_milo", CodexCategory.Characters, "Milo — The Companion",
                "A spirited canine companion with an uncanny ability to sense Aether fluctuations. Milo's ears literally perk up near buried structures, and his bark resonates at 432 Hz — a living tuning fork.");
            Add("char_lirael", CodexCategory.Characters, "Lirael — Crystal Singer",
                "Born in the Crystalline Caverns, Lirael carries memories of Tartaria encoded in crystal lattices. She can project holographic blueprints of buildings as they once were, singing frequencies that resonate with ancient materials.");
            Add("char_cassian", CodexCategory.Characters, "Cassian — The Double Agent",
                "An intelligence operative sent to monitor the restoration effort. Initially working for those who buried Tartaria, Cassian's loyalty shifts as he witnesses the beauty of what was destroyed. His insider knowledge proves invaluable.");
            Add("char_thorne", CodexCategory.Characters, "Commander Thorne",
                "Veteran of the Resonance Wars, Thorne kept the Restoration militia alive through decades of hiding. Pragmatic, scarred by the betrayal at Chronopolis, but unshakeable in his commitment to the cause.");
            Add("char_korath", CodexCategory.Characters, "Korath — Harmonic Keeper",
                "Ancient. Perhaps immortal. Korath guards the Harmonic Archives and speaks in frequencies as much as words. Connected to the Day Out of Time — the galactic alignment that could reveal all of history's buried truths.");
            Add("char_anastasia", CodexCategory.Characters, "Princess Anastasia",
                "The spectral guide who manifests at moments of peak resonance. Her connection to the Tartarian royal line makes her an anchor between the buried past and the emerging future.");

            // ── Frequencies ──
            Add("freq_432", CodexCategory.Frequencies, "432 Hz — Universal Harmony",
                "The 'Verdi tuning.' Pre-20th century music used 432 Hz as standard A. The shift to 440 Hz was deliberate — disconnecting the population from natural resonance. Tartarian instruments and buildings all tuned to 432.");
            Add("freq_528", CodexCategory.Frequencies, "528 Hz — Transformation",
                "The 'miracle frequency.' Part of the Solfeggio scale, 528 Hz is associated with DNA repair and cellular regeneration in cymatics research. Korath awakens when this frequency is sustained.");
            Add("freq_783", CodexCategory.Frequencies, "7.83 Hz — Schumann Resonance",
                "Earth's electromagnetic heartbeat. The gap between Earth's surface and ionosphere resonates at 7.83 Hz. Tartarian buildings were designed to amplify this frequency, keeping occupants in harmony with the planet.");
            Add("freq_1296", CodexCategory.Frequencies, "1296 Hz — Celestial Connection",
                "The highest frequency in the Tartarian system. 1296 = 6^4 = 1296. At this frequency, the boundary between dimensions becomes permeable. The Day Out of Time occurs when this frequency peaks globally.");

            // ── Locations ──
            Add("loc_echohaven", CodexCategory.Locations, "Echohaven",
                "First zone. A buried riverside settlement whose name echoes its former glory. Grand halls, aqueducts, and a central bell tower that once broadcast protecting frequencies across the region.");
            Add("loc_crystalline", CodexCategory.Locations, "Crystalline Caverns",
                "Underground crystal formations that served as Tartaria's memory storage. Each crystal facet holds encoded memories — songs, blueprints, histories — readable only by those who can sing the right frequency.");
            Add("loc_tartaria_prime", CodexCategory.Locations, "Tartaria Prime",
                "The legendary capital. Said to contain the Master Resonator — a device capable of broadcasting Aether energy worldwide. Its location was the most closely guarded secret in history. Moon 13's final destination.");
        }

        void Add(string id, CodexCategory cat, string title, string body)
        {
            _entries[id] = new CodexEntry { id = id, category = cat, title = title, body = body };
        }
    }

    // ─── Codex Data Types ────────────────────────

    public enum CodexCategory : byte
    {
        History = 0,
        Technology = 1,
        Characters = 2,
        Frequencies = 3,
        Locations = 4,
        Artifacts = 5
    }

    [System.Serializable]
    public class CodexEntry
    {
        public string id;
        public CodexCategory category;
        public string title;
        public string body;
    }

    [System.Serializable]
    public class CodexSaveData
    {
        public List<string> unlockedEntryIds;
    }
}
