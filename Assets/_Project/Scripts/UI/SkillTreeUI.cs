using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Skill Tree UI — visual interface for the 4 skill trees.
    ///
    /// Layout: 4 vertical columns (Resonator/Architect/Guardian/Historian),
    /// each with 5 tiers of nodes connected by lines.
    ///
    /// Node states:
    ///   - Locked (gray, no prereq met)
    ///   - Available (pulsing gold, can afford + prereq met)
    ///   - Unlocked (bright, fully lit)
    ///
    /// Refs Tartaria.Core (AetherFieldManager for RS) and Tartaria.Gameplay (SkillTreeSystem).
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        public static SkillTreeUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] GameObject skillTreePanel;
        [SerializeField] Button closeButton;

        [Header("Tree Tab Buttons")]
        [SerializeField] Button resonatorTabButton;
        [SerializeField] Button architectTabButton;
        [SerializeField] Button guardianTabButton;
        [SerializeField] Button historianTabButton;

        [Header("Node Template")]
        [SerializeField] GameObject skillNodePrefab;
        [SerializeField] Transform nodeContainer;

        [Header("Detail Panel")]
        [SerializeField] TextMeshProUGUI detailName;
        [SerializeField] TextMeshProUGUI detailDescription;
        [SerializeField] TextMeshProUGUI detailCost;
        [SerializeField] TextMeshProUGUI detailModifier;
        [SerializeField] Button unlockButton;
        [SerializeField] TextMeshProUGUI unlockButtonLabel;

        [Header("Header")]
        [SerializeField] TextMeshProUGUI treeTitle;
        [SerializeField] TextMeshProUGUI rsDisplay;

        [Header("Colors")]
        [SerializeField] Color lockedColor = new(0.3f, 0.3f, 0.3f, 0.6f);
        [SerializeField] Color availableColor = new(0.95f, 0.82f, 0.35f, 1f); // Golden
        [SerializeField] Color unlockedColor = new(0.2f, 0.8f, 0.4f, 1f);     // Green
        [SerializeField] Color connectionActive = new(0.95f, 0.82f, 0.35f, 0.8f);
        [SerializeField] Color connectionInactive = new(0.3f, 0.3f, 0.3f, 0.3f);

        SkillTreeType _activeTree = SkillTreeType.Resonator;
        SkillId _selectedSkill = SkillId.None;
        readonly Dictionary<SkillId, SkillNodeUI> _nodeWidgets = new();
        bool _isOpen;
        int _lastRSInt = -1;

        readonly string[] TreeNames = { "RESONATOR", "ARCHITECT", "GUARDIAN", "HISTORIAN" };
        readonly string[] TreeIcons = { "\u266B", "\u2302", "\u2694", "\u270D" }; // ♫ ⌂ ⚔ ✍

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
            if (resonatorTabButton != null) resonatorTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Resonator));
            if (architectTabButton != null) architectTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Architect));
            if (guardianTabButton != null) guardianTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Guardian));
            if (historianTabButton != null) historianTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Historian));
            if (unlockButton != null) unlockButton.onClick.AddListener(OnUnlockClicked);

            if (SkillTreeSystem.Instance != null)
                SkillTreeSystem.Instance.OnSkillUnlocked += HandleSkillUnlocked;

            Close();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (SkillTreeSystem.Instance != null)
                SkillTreeSystem.Instance.OnSkillUnlocked -= HandleSkillUnlocked;
        }

        void Update()
        {
            if (!_isOpen) return;
            UpdateRSDisplay();
            PulseAvailableNodes();
        }

        // ─── Open / Close ────────────────────────────

        public void Open()
        {
            if (skillTreePanel == null) return;
            _isOpen = true;
            skillTreePanel.SetActive(true);
            SwitchTree(_activeTree);
            Audio.AudioManager.Instance?.PlaySFX2D("UIOpen");
        }

        public void Close()
        {
            _isOpen = false;
            if (skillTreePanel != null) skillTreePanel.SetActive(false);
            Audio.AudioManager.Instance?.PlaySFX2D("UIClose");
        }

        public void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public bool IsOpen => _isOpen;

        // ─── Tree Switching ──────────────────────────

        void SwitchTree(SkillTreeType tree)
        {
            _activeTree = tree;
            _selectedSkill = SkillId.None;
            if (treeTitle != null)
                treeTitle.text = $"{TreeIcons[(int)tree]} {TreeNames[(int)tree]}";

            RebuildNodeDisplay();
            ClearDetailPanel();
            UpdateTabHighlights();
        }

        void UpdateTabHighlights()
        {
            SetTabActive(resonatorTabButton, _activeTree == SkillTreeType.Resonator);
            SetTabActive(architectTabButton, _activeTree == SkillTreeType.Architect);
            SetTabActive(guardianTabButton, _activeTree == SkillTreeType.Guardian);
            SetTabActive(historianTabButton, _activeTree == SkillTreeType.Historian);
        }

        void SetTabActive(Button tab, bool active)
        {
            if (tab == null) return;
            var colors = tab.colors;
            colors.normalColor = active ? availableColor : lockedColor;
            tab.colors = colors;
        }

        // ─── Node Display ────────────────────────────

        void RebuildNodeDisplay()
        {
            // Clear existing
            foreach (var w in _nodeWidgets.Values)
                if (w.go != null) Destroy(w.go);
            _nodeWidgets.Clear();

            var sys = SkillTreeSystem.Instance;
            if (sys == null) return;

            var nodes = sys.GetTree(_activeTree);
            if (nodes == null) return;

            float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var widget = CreateNodeWidget(node, i, nodes.Count, currentRS);
                _nodeWidgets[node.id] = widget;
            }
        }

        SkillNodeUI CreateNodeWidget(SkillNode node, int index, int total, float currentRS)
        {
            var widget = new SkillNodeUI();

            if (skillNodePrefab != null && nodeContainer != null)
            {
                widget.go = Instantiate(skillNodePrefab, nodeContainer);
            }
            else
            {
                // Fallback: create simple UI element
                widget.go = new GameObject($"SkillNode_{node.id}");
                widget.go.transform.SetParent(nodeContainer ?? transform, false);
                widget.go.AddComponent<RectTransform>();
                widget.go.AddComponent<Image>();
                var btn = widget.go.AddComponent<Button>();
                var label = new GameObject("Label");
                label.transform.SetParent(widget.go.transform, false);
                label.AddComponent<RectTransform>();
                widget.label = label.AddComponent<TextMeshProUGUI>();
                widget.button = btn;
            }

            // Try get components
            if (widget.button == null)
                widget.button = widget.go.GetComponentInChildren<Button>();
            if (widget.label == null)
                widget.label = widget.go.GetComponentInChildren<TextMeshProUGUI>();
            widget.image = widget.go.GetComponent<Image>();

            // Position: tier-based vertical, centered horizontal
            var rt = widget.go.GetComponent<RectTransform>();
            if (rt != null)
            {
                float y = 200f - (node.tier - 1) * 100f; // Tier 1 at top, cascade down
                float x = (index % 2 == 0) ? -40f : 40f; // Slight offset for branching
                if (node.tier == 1 || node.tier >= 4) x = 0; // Single node at top/bottom
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(160f, 60f);
            }

            // Label
            if (widget.label != null)
            {
                widget.label.text = node.displayName;
                widget.label.fontSize = 12f;
                widget.label.alignment = TextAlignmentOptions.Center;
            }

            // State coloring
            UpdateNodeVisual(widget, node, currentRS);

            // Click handler
            var skillId = node.id;
            widget.button?.onClick.AddListener(() => SelectNode(skillId));

            return widget;
        }

        void UpdateNodeVisual(SkillNodeUI widget, SkillNode node, float currentRS)
        {
            if (widget.image == null) return;

            if (node.isUnlocked)
            {
                widget.image.color = unlockedColor;
                widget.state = NodeState.Unlocked;
            }
            else if (CanUnlock(node, currentRS))
            {
                widget.image.color = availableColor;
                widget.state = NodeState.Available;
            }
            else
            {
                widget.image.color = lockedColor;
                widget.state = NodeState.Locked;
            }
        }

        bool CanUnlock(SkillNode node, float currentRS)
        {
            if (node.isUnlocked) return false;
            if (currentRS < node.rsCost) return false;
            if (node.prerequisite == SkillId.None) return true;
            return SkillTreeSystem.Instance?.IsSkillUnlocked(node.prerequisite) ?? false;
        }

        void PulseAvailableNodes()
        {
            float pulse = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 3f);
            foreach (var kvp in _nodeWidgets)
            {
                if (kvp.Value.state == NodeState.Available && kvp.Value.image != null)
                {
                    var c = availableColor;
                    c.a = pulse;
                    kvp.Value.image.color = c;
                }
            }
        }

        // ─── Selection & Detail ──────────────────────

        void SelectNode(SkillId id)
        {
            _selectedSkill = id;
            var sys = SkillTreeSystem.Instance;
            if (sys == null) return;

            var nodes = sys.GetTree(_activeTree);
            if (nodes == null) return;

            SkillNode selected = null;
            foreach (var n in nodes)
                if (n.id == id) { selected = n; break; }

            if (selected == null) return;

            if (detailName != null) detailName.text = selected.displayName;
            if (detailDescription != null) detailDescription.text = selected.description;
            if (detailCost != null) detailCost.text = $"Cost: {selected.rsCost} RS";
            if (detailModifier != null)
                detailModifier.text = $"+{selected.modifierValue:P0} {FormatModType(selected.modifierType)}";

            float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            bool canUnlock = CanUnlock(selected, currentRS);

            if (unlockButton != null)
            {
                unlockButton.interactable = canUnlock;
                unlockButton.gameObject.SetActive(!selected.isUnlocked);
            }
            if (unlockButtonLabel != null)
            {
                unlockButtonLabel.text = selected.isUnlocked ? "UNLOCKED" :
                    canUnlock ? "UNLOCK" : "LOCKED";
            }
        }

        void ClearDetailPanel()
        {
            if (detailName != null) detailName.text = "Select a skill";
            if (detailDescription != null) detailDescription.text = "";
            if (detailCost != null) detailCost.text = "";
            if (detailModifier != null) detailModifier.text = "";
            if (unlockButton != null) unlockButton.gameObject.SetActive(false);
        }

        void OnUnlockClicked()
        {
            if (_selectedSkill == SkillId.None) return;
            SkillTreeSystem.Instance?.TryUnlockSkill(_selectedSkill);
        }

        void HandleSkillUnlocked(SkillId id)
        {
            // Refresh all node visuals
            float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            var nodes = SkillTreeSystem.Instance?.GetTree(_activeTree);
            if (nodes == null) return;

            foreach (var n in nodes)
            {
                if (_nodeWidgets.TryGetValue(n.id, out var widget))
                    UpdateNodeVisual(widget, n, currentRS);
            }

            // Re-select to update detail panel
            if (id == _selectedSkill)
                SelectNode(id);
        }

        void UpdateRSDisplay()
        {
            if (rsDisplay == null) return;
            float rs = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            int rsInt = Mathf.RoundToInt(rs);
            if (rsInt == _lastRSInt) return;
            _lastRSInt = rsInt;
            rsDisplay.text = $"RS: {rsInt}";
        }

        string FormatModType(SkillModifierType mod)
        {
            return mod switch
            {
                SkillModifierType.TuningPrecision => "Tuning Precision",
                SkillModifierType.TuningSpeed => "Tuning Speed",
                SkillModifierType.AetherCapacity => "Aether Capacity",
                SkillModifierType.ComboDuration => "Combo Duration",
                SkillModifierType.RepairSpeed => "Repair Speed",
                SkillModifierType.BuildingResistance => "Building Resistance",
                SkillModifierType.RSMultiplier => "RS Multiplier",
                SkillModifierType.PulseDamage => "Pulse Damage",
                SkillModifierType.ShieldDuration => "Shield Duration",
                SkillModifierType.StrikeRange => "Strike Range",
                _ => mod.ToString()
            };
        }

        // ─── Internal Types ─────────────────────────

        enum NodeState { Locked, Available, Unlocked }

        class SkillNodeUI
        {
            public GameObject go;
            public Button button;
            public TextMeshProUGUI label;
            public Image image;
            public NodeState state;
        }
    }
}
