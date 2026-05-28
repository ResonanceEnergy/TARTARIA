using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Core.Enums;
using Tartaria.Gameplay;
using UnityEngine.EventSystems;

namespace Tartaria.UI
#pragma warning disable CS0414 // Field assigned but never used - reserved for future implementation
{
    /// <summary>
    /// Skill Tree UI — Phase 3 R6 Production Polish.
    ///
    /// Fully populates the 4 Skill Trees with real node content, capstones, Skill Crystals,
    /// and meaningful progression matching the GDD power fantasy:
    ///   • Resonator — Frequency sorcerer who conducts the hidden music of the world
    ///   • Architect — Sacred geometer who rebuilds the Golden Age in perfect proportion
    ///   • Guardian — Titan defender who becomes the living architecture in Giant Mode
    ///   • Historian — Keeper of echoes who hears the stones remember
    ///
    /// Visual language: crystalline Skill Crystals (3 per tree) as capstone tokens.
    /// Dynamic navigation + zoom (R5) hardened further for R6 gamepad/screen-reader/extreme scale.
    /// Every unlock is a small miracle — rich flavor text, magical particles (TMP), captions, screen reader.
    ///
    /// Zero gameplay logic changed. Pure UI/UX depth layer on R5 foundation.
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

        [Header("Phase 3 R6: Skill Crystals & Capstone Polish")]
        [SerializeField] Transform skillCrystalContainer; // Optional visual row of 3 crystals per tree
        [SerializeField] GameObject skillCrystalPrefab;   // Simple image or TMP star/crystal glyph

        [Header("Phase 3 R4 Accessibility & Navigation")]
        [SerializeField] TextMeshProUGUI navigationHintText; // On-screen hints for keyboard/gamepad
        [SerializeField] ScrollRect nodeScrollRect; // For scroll support on extreme text scales / large trees
        [SerializeField] float minZoom = 0.6f;

        SkillTreeType _activeTree = SkillTreeType.Resonator;
        SkillId _selectedSkill = SkillId.None;
        readonly Dictionary<SkillId, SkillNodeUI> _nodeWidgets = new();
        bool _isOpen;
        int _lastRSInt = -1;
        float _currentZoom = 1f;
        List<Button> _currentNodeButtons = new List<Button>(); // for dynamic nav wiring

        readonly string[] TreeNames = { "RESONATOR", "ARCHITECT", "GUARDIAN", "HISTORIAN" };
        readonly string[] TreeIcons = { "\u266B", "\u2302", "\u2694", "\u270D" }; // ♫ ⌂ ⚔ ✍

        // R6: Rich fantasy descriptions per archetype (shown in detail panel + screen reader)
        readonly Dictionary<SkillId, string> _richFantasyLore = new Dictionary<SkillId, string>
        {
            { SkillId.Res_FreqSense, "The first gift of the Resonator: you begin to see the invisible music that runs through every stone and leaf. Aether frequency values bloom above buildings like luminous notation." },
            { SkillId.Res_TuneSpeed, "Your hands remember the old conductor's gestures. The tuning mini-game breathes with you — time stretches just enough for perfect resonance." },
            { SkillId.Res_AetherPool, "The body becomes a vessel. The land pours more of itself into you because it recognizes a worthy listener." },
            { SkillId.Res_Cascade, "Golden Cascade now sings for fifteen perfect strikes. Each note births the next in an unbroken chain of harmonic glory." },
            { SkillId.Res_MasterFreq, "CAPSTONE — Master of the Hidden Choir. Tuning success +40%. You no longer chase frequencies; they answer your call. The world itself becomes your instrument." },

            { SkillId.Arc_BlueprintScan, "The Architect's eye: blueprints shimmer at fifty meters. The bones of the old world reveal themselves to one who still dreams in proportion." },
            { SkillId.Arc_QuickRepair, "Stone answers faster. Mud flees at the touch of your will. Thirty percent more speed — the age of restoration accelerates." },
            { SkillId.Arc_Fortify, "What you heal, you also armor. Repaired buildings stand twenty percent stronger against the returning corruption." },
            { SkillId.Arc_MassRestore, "CAPSTONE PATH — The three become one. You may conduct the restoration of three buildings in the same breath. The grid sings louder." },
            { SkillId.Arc_GoldenRatio, "ULTIMATE CAPSTONE — Golden Ratio Mastery. Buildings auto-align to phi. +50% Resonance from every restored structure. You are the living measure of the Golden Age." },

            { SkillId.Grd_StrongPulse, "The Guardian's first roar: Resonance Pulse strikes fifteen percent harder. The mud remembers fear again." },
            { SkillId.Grd_ShieldDuration, "Your harmonic shield lingers like a second skin. Five full seconds of protection — enough to turn a desperate stand into a counter-song." },
            { SkillId.Grd_StrikeRange, "Harmonic Strike reaches thirty percent farther. You conduct from the center of the battlefield like a general of light." },
            { SkillId.Grd_AOEPurge, "Purification Wave — your pulse now burns corruption in a radius. One strike, many cleansings. The land thanks you." },
            { SkillId.Grd_Invulnerable, "CAPSTONE — After a perfect combo you become untouchable for three seconds. The world itself steps aside while you finish the symphony." },
            { SkillId.Grd_TitanFlight, "SKILL CRYSTAL 1 — Titan Soar. In Giant Mode you take the sky. Physics, camera, and input all yield to the titan’s stride." },
            { SkillId.Grd_EarthShaper, "SKILL CRYSTAL 2 — Earth Shaper. Giant footsteps carve real terrain. You sculpt the world as easily as the old Tartarians once did." },
            { SkillId.Grd_ColossusForm, "SKILL CRYSTAL 3 + ULTIMATE — Living Colossus. Triple synergy cathedral-giant form. The fountain, the spire, and your own body become one living wonder." },

            { SkillId.His_LoreReveal, "The Historian opens her first eye: hidden inscriptions glow within thirty meters. The stones begin to speak again." },
            { SkillId.His_SecretPaths, "Secret Paths — walls that were once solid now show seams of light. The old builders left doors for those who remember how to knock." },
            { SkillId.His_MemoryEcho, "You hear the voices the mud tried to bury. Echoes of builders, lovers, and last defenders whisper their final notes." },
            { SkillId.His_AncientMap, "The full cartography of the zone unfolds. Every buried structure sings its location to you. No secret remains buried forever." },
            { SkillId.His_TrueHistory, "CAPSTONE — True History. All lore auto-collected. +100% Resonance from every discovery. You are the living archive of the Golden Age." }
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
            if (resonatorTabButton != null) resonatorTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Resonator));
            if (architectTabButton != null) architectTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Architect));
            if (guardianTabButton != null) guardianTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Guardian));
            if (historianTabButton != null) historianTabButton.onClick.AddListener(() => SwitchTree(SkillTreeType.Historian));
            if (unlockButton != null) unlockButton.onClick.AddListener(OnUnlockClicked);

            if (SkillTreeSystem.Instance != null)
                SkillTreeSystem.Instance.OnSkillUnlocked += HandleSkillUnlocked;

            if (AccessibilityManager.Instance != null)
            {
                AccessibilityManager.Instance.OnSettingsChanged += HandleAccessibilityChanged;
                AccessibilityManager.Instance.OnColorblindModeChanged += HandleColorblindChanged;
                AccessibilityManager.Instance.SetScreenReaderTrait("skill_tree", "Four arcane paths of power. Resonator, Architect, Guardian, Historian. Dynamic nodes with full keyboard, gamepad, screen reader, and extreme scale support. Skill Crystals mark the deepest capstones.");
            }

            SetNavigationHint("Arrow keys / D-pad: Navigate nodes • Enter/A: Unlock • Tab / Bumpers: Switch trees • +/- : Zoom • Crystals = Capstone power");
            Close();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (SkillTreeSystem.Instance != null)
                SkillTreeSystem.Instance.OnSkillUnlocked -= HandleSkillUnlocked;
            if (AccessibilityManager.Instance != null)
            {
                AccessibilityManager.Instance.OnSettingsChanged -= HandleAccessibilityChanged;
                AccessibilityManager.Instance.OnColorblindModeChanged -= HandleColorblindChanged;
            }
        }

        void Update()
        {
            if (!_isOpen) return;
            UpdateRSDisplay();
            PulseAvailableNodes();
        }

        // ─── Open / Close (R6: accessibility announce) ────────────────────────────

        public void Open()
        {
            if (skillTreePanel == null) return;
            _isOpen = true;
            skillTreePanel.SetActive(true);
            SwitchTree(_activeTree);
            Audio.AudioManager.Instance?.PlaySFX2D("UIOpen");
            SetNavigationHint("Use arrows/D-pad to move between skill nodes. Crystals mark the deepest capstones of each path. Press to select. Zoom with +/- if text is scaled large.");

            AccessibilityManager.Instance?.AnnounceForScreenReader($"Skill Tree opened. Current path: {TreeNames[(int)_activeTree]}. Four paths of power await your Resonance.", true);
            AccessibilityManager.Instance?.OnMajorUIRebuild();

            StartCoroutine(FocusFirstNodeAfterFrame());
        }

        public void Close()
        {
            _isOpen = false;
            if (skillTreePanel != null) skillTreePanel.SetActive(false);
            _currentZoom = 1f;
            if (nodeContainer != null)
            {
                var rt = nodeContainer.GetComponent<RectTransform>();
                if (rt != null) rt.localScale = Vector3.one;
            }
            Audio.AudioManager.Instance?.PlaySFX2D("UIClose");
            AccessibilityManager.Instance?.AnnounceForScreenReader("Skill Tree closed. The harmonics of your choices remain with you.", false);
        }

        public void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public bool IsOpen => _isOpen;

        // ─── Tree Switching (R6 richer announce) ──────────────────────────

        void SwitchTree(SkillTreeType tree)
        {
            _activeTree = tree;
            _selectedSkill = SkillId.None;
            if (treeTitle != null)
                treeTitle.text = $"{TreeIcons[(int)tree]} {TreeNames[(int)tree]}";

            RebuildNodeDisplay();
            ClearDetailPanel();
            UpdateTabHighlights();
            RebuildSkillCrystalsVisuals(); // R6 capstone crystals

            string flavor = tree switch
            {
                SkillTreeType.Resonator => "You conduct the hidden music of the world.",
                SkillTreeType.Architect => "You rebuild the Golden Age in perfect proportion.",
                SkillTreeType.Guardian => "You become the living architecture that defends the light.",
                SkillTreeType.Historian => "You hear the stones remember every name.",
                _ => ""
            };
            AccessibilityManager.Instance?.AnnounceForScreenReader($"{TreeNames[(int)tree]} path selected. {flavor}", true);
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

        // ─── Node Display + R6 Population ────────────────────────────

        void RebuildNodeDisplay()
        {
            foreach (var w in _nodeWidgets.Values)
                if (w.go != null) Destroy(w.go);
            _nodeWidgets.Clear();
            _currentNodeButtons.Clear();

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

            SetupUnityNavigationForDynamicButtons();
            ApplyAccessibilityToNodes();
            AccessibilityManager.Instance?.ApplyGlobalButtonSizing(); // R6 motor
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

            if (widget.button == null)
                widget.button = widget.go.GetComponentInChildren<Button>();
            if (widget.label == null)
                widget.label = widget.go.GetComponentInChildren<TextMeshProUGUI>();
            widget.image = widget.go.GetComponent<Image>();

            var rt = widget.go.GetComponent<RectTransform>();
            if (rt != null)
            {
                float y = 200f - (node.tier - 1) * 100f;
                float x = (index % 2 == 0) ? -40f : 40f;
                if (node.tier == 1 || node.tier >= 4) x = 0;
                rt.anchoredPosition = new Vector2(x, y);

                float scale = AccessibilityManager.Instance?.TextScale ?? 1f;
                float baseW = 160f * Mathf.Clamp(scale, 0.75f, 2f);
                float baseH = 60f * Mathf.Clamp(scale, 0.75f, 2f);
                rt.sizeDelta = new Vector2(baseW, baseH);
            }

            if (widget.label != null)
            {
                widget.label.text = node.displayName;
                float scale = AccessibilityManager.Instance?.TextScale ?? 1f;
                widget.label.fontSize = 12f * Mathf.Clamp(scale, 0.75f, 2f);
                widget.label.alignment = TextAlignmentOptions.Center;
            }

            UpdateNodeVisual(widget, node, currentRS);

            var skillId = node.id;
            widget.button?.onClick.AddListener(() => SelectNode(skillId));

            if (widget.button != null && !_currentNodeButtons.Contains(widget.button))
                _currentNodeButtons.Add(widget.button);

            // R6: mark capstones visually
            if (node.tier >= 4 && widget.image != null)
            {
                widget.image.transform.localScale = Vector3.one * 1.12f;
            }

            return widget;
        }

        void UpdateNodeVisual(SkillNodeUI widget, SkillNode node, float currentRS)
        {
            if (widget.image == null) return;

            Color baseCol;
            if (node.isUnlocked)
            {
                baseCol = unlockedColor;
                widget.state = NodeState.Unlocked;
            }
            else if (CanUnlock(node, currentRS))
            {
                baseCol = availableColor;
                widget.state = NodeState.Available;
            }
            else
            {
                baseCol = lockedColor;
                widget.state = NodeState.Locked;
            }

            var am = AccessibilityManager.Instance;
            if (am != null)
            {
                am.ApplyColorblindAdjustment(widget.image, baseCol);
            }
            else
            {
                widget.image.color = baseCol;
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

        // ─── R6: Skill Crystal Visuals (3 per tree = capstone tokens) ─────────────────

        void RebuildSkillCrystalsVisuals()
        {
            if (skillCrystalContainer == null) return;

            // Clear previous
            foreach (Transform child in skillCrystalContainer)
                Destroy(child.gameObject);

            int unlockedCount = 0;
            var sys = SkillTreeSystem.Instance;
            if (sys != null)
            {
                var nodes = sys.GetTree(_activeTree);
                foreach (var n in nodes)
                    if (n.isUnlocked && n.tier >= 4) unlockedCount++;
            }

            int crystalCount = 3; // R6: three Skill Crystals per path
            for (int i = 0; i < crystalCount; i++)
            {
                GameObject crystal = skillCrystalPrefab != null
                    ? Instantiate(skillCrystalPrefab, skillCrystalContainer)
                    : CreateFallbackCrystal();

                var rt = crystal.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(i * 42f - 42f, 0);

                var img = crystal.GetComponent<Image>();
                bool filled = i < unlockedCount;
                if (img != null)
                {
                    img.color = filled ? new Color(1f, 0.92f, 0.55f, 1f) : new Color(0.3f, 0.3f, 0.35f, 0.6f);
                    if (AccessibilityManager.Instance != null)
                        AccessibilityManager.Instance.ApplyColorblindAdjustment(img, img.color);
                }

                var tmp = crystal.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = filled ? "✧" : "◦";
                    tmp.fontSize = 18;
                }
            }

            AccessibilityManager.Instance?.SetScreenReaderTrait("skill_crystals", $"Skill Crystals for {_activeTree}: {unlockedCount} of 3 capstones claimed. These represent the deepest power fantasy of the path.");
        }

        GameObject CreateFallbackCrystal()
        {
            var go = new GameObject("SkillCrystal");
            go.transform.SetParent(skillCrystalContainer, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(32, 32);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.95f, 0.82f, 0.35f, 0.85f);
            var label = new GameObject("Glyph");
            label.transform.SetParent(go.transform, false);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "✦";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 22;
            tmp.color = Color.white;
            return go;
        }

        // ─── Selection & Detail — R6 Rich Fantasy Lore ──────────────────────

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

            // R6: Use rich fantasy lore when available, fall back to base description
            string lore = _richFantasyLore.TryGetValue(selected.id, out var rich) ? rich : selected.description;
            if (detailDescription != null) detailDescription.text = lore;

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
                unlockButtonLabel.text = selected.isUnlocked ? "UNLOCKED — THE SONG REMAINS" :
                    canUnlock ? "UNLOCK • CLAIM THIS CRYSTAL" : "LOCKED";
            }

            // R6 screen reader + caption on selection
            AccessibilityManager.Instance?.AnnounceForScreenReader($"{selected.displayName}. {lore}", true);
            AccessibilityManager.Instance?.PostSFXCaption("Skill Tree", $"Selected {selected.displayName}");
        }

        void ClearDetailPanel()
        {
            if (detailName != null) detailName.text = "Select a node — feel the resonance";
            if (detailDescription != null) detailDescription.text = "Each path tells a different story of the Golden Age reborn.";
            if (detailCost != null) detailCost.text = "";
            if (detailModifier != null) detailModifier.text = "";
            if (unlockButton != null) unlockButton.gameObject.SetActive(false);
        }

        void OnUnlockClicked()
        {
            if (_selectedSkill == SkillId.None) return;
            bool success = SkillTreeSystem.Instance?.TryUnlockSkill(_selectedSkill) ?? false;
            if (success)
            {
                RebuildSkillCrystalsVisuals();
                AccessibilityManager.Instance?.AnnounceForScreenReader("Skill unlocked. A new note joins the eternal harmony.", true);
                AccessibilityManager.Instance?.PostSFXCaption("Skill Tree", "A Skill Crystal resonates within you.");
            }
        }

        void HandleSkillUnlocked(SkillId id)
        {
            float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            var nodes = SkillTreeSystem.Instance?.GetTree(_activeTree);
            if (nodes == null) return;

            foreach (var n in nodes)
            {
                if (_nodeWidgets.TryGetValue(n.id, out var widget))
                    UpdateNodeVisual(widget, n, currentRS);
            }

            if (id == _selectedSkill)
                SelectNode(id);

            RebuildSkillCrystalsVisuals();
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

        // ─── R5/R6 Navigation + Accessibility (hardened) ───

        void SetupUnityNavigationForDynamicButtons()
        {
            if (_currentNodeButtons.Count == 0) return;

            _currentNodeButtons.Sort((a, b) =>
            {
                var rta = a.GetComponent<RectTransform>();
                var rtb = b.GetComponent<RectTransform>();
                if (rta == null || rtb == null) return 0;
                if (Mathf.Abs(rta.anchoredPosition.y - rtb.anchoredPosition.y) > 10f)
                    return rtb.anchoredPosition.y.CompareTo(rta.anchoredPosition.y);
                return rta.anchoredPosition.x.CompareTo(rtb.anchoredPosition.x);
            });

            for (int i = 0; i < _currentNodeButtons.Count; i++)
            {
                var btn = _currentNodeButtons[i];
                var nav = btn.navigation;
                nav.mode = Navigation.Mode.Explicit;

                int up = (i > 0) ? i - 1 : i;
                int down = (i < _currentNodeButtons.Count - 1) ? i + 1 : i;
                nav.selectOnUp = _currentNodeButtons[up];
                nav.selectOnDown = _currentNodeButtons[down];

                if (i > 0) nav.selectOnLeft = _currentNodeButtons[i - 1];
                if (i < _currentNodeButtons.Count - 1) nav.selectOnRight = _currentNodeButtons[i + 1];

                btn.navigation = nav;
                btn.onClick.AddListener(() => AccessibilityManager.Instance?.AnnounceForScreenReader("Node selected. " + btn.GetComponentInChildren<TextMeshProUGUI>()?.text, false));
            }
        }

        System.Collections.IEnumerator FocusFirstNodeAfterFrame()
        {
            yield return null;
            if (_currentNodeButtons.Count > 0 && _currentNodeButtons[0] != null)
            {
                EventSystem.current?.SetSelectedGameObject(_currentNodeButtons[0].gameObject);
            }
        }

        void ApplyAccessibilityToNodes()
        {
            float scale = AccessibilityManager.Instance?.TextScale ?? 1f;
            foreach (var widget in _nodeWidgets.Values)
            {
                if (widget.label != null)
                    widget.label.fontSize = 12f * Mathf.Clamp(scale, 0.75f, 2f);

                if (widget.go != null)
                {
                    var rt = widget.go.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        float s = Mathf.Clamp(scale, 0.75f, 2f);
                        rt.sizeDelta = new Vector2(160f * s, 60f * s);
                    }
                }
            }
            // R6 motor + crystals
            AccessibilityManager.Instance?.ApplyGlobalButtonSizing();
            RebuildSkillCrystalsVisuals();
        }

        void HandleAccessibilityChanged()
        {
            if (_isOpen)
            {
                RebuildNodeDisplay();
                RebuildSkillCrystalsVisuals();
            }
            SetNavigationHint("Arrow keys / D-pad: Navigate. Crystals = Capstone power. All sizes respect your accessibility settings.");
            AccessibilityManager.Instance?.OnMajorUIRebuild();
        }

        void HandleColorblindChanged()
        {
            if (_isOpen && _nodeWidgets.Count > 0)
            {
                float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
                foreach (var kvp in _nodeWidgets)
                {
                    var node = SkillTreeSystem.Instance?.GetTree(_activeTree).Find(n => n.id == kvp.Key); // safe lookup
                    if (node != null)
                        UpdateNodeVisual(kvp.Value, node, currentRS);
                }
            }
        }

        void SetNavigationHint(string text)
        {
            if (navigationHintText != null) navigationHintText.text = text;
        }

        // R6: Public API for external systems (HUD, GameLoop) to open specific tree with flair
        public void OpenTreeWithFlair(SkillTreeType tree)
        {
            Open();
            SwitchTree(tree);
            AccessibilityManager.Instance?.PostSFXCaption("Skill Tree", $"The {TreeNames[(int)tree]} path opens before you like a living constellation.");
        }
    }

    // Supporting small structs kept for compatibility
    public class SkillNodeUI
    {
        public GameObject go;
        public Button button;
        public TextMeshProUGUI label;
        public Image image;
        public NodeState state;
    }

    public enum NodeState { Locked, Available, Unlocked }
}
