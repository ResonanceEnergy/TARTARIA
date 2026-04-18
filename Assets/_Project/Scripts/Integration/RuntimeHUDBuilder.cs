using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// RuntimeHUDBuilder -- creates the entire UI Canvas hierarchy at runtime,
    /// wires all SerializeField references on HUDController and QuestLogUI,
    /// builds pause menu, tuning mini-game overlay, and damage number pool.
    ///
    /// Fixes gaps: 4 (tuning UI), 11 (HUD null), 12 (pause menu), 13 (quest log), 14 (damage numbers).
    /// Execution order -100: runs before all gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class RuntimeHUDBuilder : MonoBehaviour
    {
        public static RuntimeHUDBuilder Instance { get; private set; }

        // Cached references for damage number pool
        RectTransform _damageNumberContainer;
        TextMeshProUGUI[] _damagePool;
        int _damagePoolIndex;
        const int DamagePoolSize = 12;

        // Pause menu reference
        GameObject _pauseMenuPanel;
        bool _isPaused;

        // Canvas root — cached so Start() can retry HUD wiring
        RectTransform _canvasRT;
        bool _hudWired;

        // Discovery flash panel
        Image _discoveryFlashImage;
        bool _flashRunning;

        // World Map and Skill Tree panels (built at runtime, toggled by M/K)
        GameObject _worldMapPanel;
        GameObject _skillTreePanel;

        // AetherVision full-screen tint
        Image _aetherVisionOverlay;
        bool _aetherVisionActive;
        bool _aetherFadeRunning;

        // Archive (Old World wiki)
        GameObject _archivePanel;

        // M / K / I / T key state for UI toggle (raw keyboard — no InputActionAsset needed)
        bool _prevMapKey;
        bool _prevSkillKey;
        bool _prevArchiveKey;
        bool _prevTuneKey;
        GameObject _tuningOverlayGO;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            BuildCanvas();
        }

        void Start()
        {
            // HUDController.Awake() runs after RuntimeHUDBuilder.Awake() due to execution order.
            // Retry wiring here once all Awake() calls have completed.
            if (!_hudWired && _canvasRT != null)
            {
                var hud = HUDController.Instance;
                if (hud != null)
                {
                    WireHUD(hud, _canvasRT);
                    _hudWired = true;
                }
                else
                {
                    Debug.LogError("[RuntimeHUDBuilder] HUDController.Instance still null in Start() -- HUD panels will not be wired.");
                }
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            GameEvents.OnTogglePause -= HandleTogglePause;
            GameEvents.OnToggleAetherVision -= HandleAetherVisionToggle;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void Update()
        {
            // M = World Map toggle (works without InputActionAsset)
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                bool mapDown = kb.mKey.isPressed;
                if (mapDown && !_prevMapKey)
                {
                    var map = WorldMapUI.Instance;
                    if (map != null) map.Toggle();
                }
                _prevMapKey = mapDown;

                bool skillDown = kb.kKey.isPressed;
                if (skillDown && !_prevSkillKey)
                {
                    var skill = SkillTreeUI.Instance;
                    if (skill != null) skill.Toggle();
                }
                _prevSkillKey = skillDown;

                bool archiveDown = kb.iKey.isPressed;
                if (archiveDown && !_prevArchiveKey)
                {
                    ArchiveUI.Instance?.Toggle();
                }
                _prevArchiveKey = archiveDown;

                bool tuneDown = kb.tKey.isPressed;
                if (tuneDown && !_prevTuneKey && _tuningOverlayGO != null)
                {
                    bool nowActive = !_tuningOverlayGO.activeSelf;
                    _tuningOverlayGO.SetActive(nowActive);
                }
                _prevTuneKey = tuneDown;
            }
        }

        void BuildCanvas()
        {
            // ─── Root Canvas ─────────────────────────
            var canvasGO = new GameObject("HUD_Canvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.transform.SetParent(transform);
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var canvasRT = canvasGO.GetComponent<RectTransform>();
            _canvasRT = canvasRT;

            // ─── Wire HUDController ──────────────────
            var hud = HUDController.Instance;
            if (hud != null)
            {
                WireHUD(hud, canvasRT);
                _hudWired = true;
            }
            // else: Start() will retry once all Awake() calls have run

            // ─── Pause Menu ──────────────────────────
            BuildPauseMenu(canvasRT);

            // ─── Damage Number Pool ──────────────────
            BuildDamageNumberPool(canvasRT);

            // ─── Quest Log UI ────────────────────────
            WireQuestLogUI(canvasRT);

            // ─── Tuning Mini-Game Overlay ────────────
            BuildTuningOverlay(canvasRT);

            // ─── Discovery Flash ─────────────────────
            BuildDiscoveryFlash(canvasRT);

            // ─── World Map ───────────────────────────
            BuildWorldMapUI(canvasRT);

            // ─── Skill Tree ──────────────────────────
            BuildSkillTreeUI(canvasRT);

            // ─── AetherVision Overlay ─────────────────
            BuildAetherVisionOverlay(canvasRT);

            // ─── Old World Archive ───────────────────
            BuildArchiveUI(canvasRT);

            // ─── Controls Hint Strip ─────────────────
            BuildControlsHint(canvasRT);

            // ─── Companion Dialogue Panel (Fix 14) ───
            BuildDialoguePanel(canvasRT);

            // ─── Startup Mission Briefing ─────────────
            BuildMissionBriefing(canvasRT);

            // Subscribe to events
            GameEvents.OnTogglePause += HandleTogglePause;
            GameEvents.OnToggleAetherVision += HandleAetherVisionToggle;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void BuildControlsHint(RectTransform canvasRT)
        {
            var hintGO = new GameObject("ControlsHint");
            hintGO.transform.SetParent(canvasRT);
            var rt = hintGO.AddComponent<RectTransform>();
            // Bottom-left corner strip
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot     = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(12f, 12f);
            rt.sizeDelta = new Vector2(520f, 28f);

            var bg = hintGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.45f);
            bg.raycastTarget = false;

            var txt = CreateChild(rt, "HintText").AddComponent<TextMeshProUGUI>();
            txt.text = "[WASD] Move   [E] Dig/Interact   [T] Frequency Tuner   [TAB] Aether Vision   [J] Quest Log   [ESC] Pause";
            txt.fontSize = 14;
            txt.color = new Color(0.85f, 0.85f, 0.7f, 0.9f);
            txt.alignment = TextAlignmentOptions.Left;
            txt.raycastTarget = false;
            txt.enableWordWrapping = false;
            var txtRT = txt.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(8f, 2f);
            txtRT.offsetMax = new Vector2(-4f, -2f);
        }

        /// <summary>Fix 14: Build companion dialogue panel and wire UIManager fields.</summary>
        void BuildDialoguePanel(RectTransform canvasRT)
        {
            // Semi-transparent bar at bottom-center of screen
            var panelGO = new GameObject("DialoguePanel");
            panelGO.transform.SetParent(canvasRT, false);
            panelGO.SetActive(false); // hidden until triggered

            var rt = panelGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0f);
            rt.anchorMax = new Vector2(0.9f, 0.2f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = panelGO.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            // Speaker name (top strip)
            var speakerGO = new GameObject("DialogueSpeaker");
            speakerGO.transform.SetParent(panelGO.transform, false);
            var speakerRT = speakerGO.AddComponent<RectTransform>();
            speakerRT.anchorMin = new Vector2(0f, 0.7f);
            speakerRT.anchorMax = Vector2.one;
            speakerRT.offsetMin = new Vector2(12f, 0f);
            speakerRT.offsetMax = new Vector2(-12f, -4f);
            var speakerTMP = speakerGO.AddComponent<TMPro.TextMeshProUGUI>();
            speakerTMP.text = "Anastasia";
            speakerTMP.fontSize = 18f;
            speakerTMP.fontStyle = TMPro.FontStyles.Bold;
            speakerTMP.color = new Color(0.6f, 0.85f, 1f);
            speakerTMP.alignment = TMPro.TextAlignmentOptions.TopLeft;

            // Dialogue body (main area)
            var bodyGO = new GameObject("DialogueBody");
            bodyGO.transform.SetParent(panelGO.transform, false);
            var bodyRT = bodyGO.AddComponent<RectTransform>();
            bodyRT.anchorMin = Vector2.zero;
            bodyRT.anchorMax = new Vector2(1f, 0.72f);
            bodyRT.offsetMin = new Vector2(12f, 8f);
            bodyRT.offsetMax = new Vector2(-12f, -4f);
            var bodyTMP = bodyGO.AddComponent<TMPro.TextMeshProUGUI>();
            bodyTMP.text = "";
            bodyTMP.fontSize = 15f;
            bodyTMP.color = Color.white;
            bodyTMP.alignment = TMPro.TextAlignmentOptions.TopLeft;

            // Wire UIManager references via reflection
            var uiMgr = UI.UIManager.Instance;
            if (uiMgr != null)
            {
                SetField(uiMgr, "dialoguePanel", panelGO);
                SetField(uiMgr, "dialogueSpeakerText", speakerTMP);
                SetField(uiMgr, "dialogueBodyText", bodyTMP);
            }
        }

        void BuildMissionBriefing(RectTransform canvasRT)
        {
            // Centre-screen modal — auto-dismisses after 10 seconds or on any key
            var briefGO = new GameObject("MissionBriefing");
            briefGO.transform.SetParent(canvasRT);
            var rt = briefGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(600f, 340f);

            var bg = briefGO.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.03f, 0.08f, 0.92f);

            // Title
            var titleGO = CreateChild(rt, "Title");
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "ECHOHAVEN AWAKENING";
            title.fontSize = 32;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.88f, 0.2f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.78f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.offsetMin = new Vector2(16f, 4f);
            titleRT.offsetMax = new Vector2(-16f, -4f);

            // Body
            var bodyGO = CreateChild(rt, "Body");
            var body = bodyGO.AddComponent<TextMeshProUGUI>();
            body.text =
                "The ancient city sleeps beneath centuries of mud.\n\n" +
                "<color=#80FF90>•</color>  Walk toward the <color=#80FF90>green beacon lights</color> — those are buried structures.\n" +
                "<color=#80FF90>•</color>  Press <color=#FFE050>[E]</color> near a beacon to begin excavating.\n" +
                "<color=#80FF90>•</color>  Restore all 3 buildings to raise your <color=#FFE050>Resonance Score</color>.\n" +
                "<color=#FF8888>•</color>  Mud Golems will attack — fight or evade them.\n" +
                "<color=#88AAFF>•</color>  Press <color=#FFE050>[T]</color> to open the Frequency Tuner.\n" +
                "<color=#88AAFF>•</color>  A presence watches from the shadows...";
            body.fontSize = 18;
            body.alignment = TextAlignmentOptions.Left;
            body.color = new Color(0.88f, 0.88f, 0.88f);
            body.richText = true;
            var bodyRT = bodyGO.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0.15f);
            bodyRT.anchorMax = new Vector2(1f, 0.78f);
            bodyRT.offsetMin = new Vector2(24f, 8f);
            bodyRT.offsetMax = new Vector2(-24f, -8f);

            // Dismiss hint
            var dismissGO = CreateChild(rt, "Dismiss");
            var dismiss = dismissGO.AddComponent<TextMeshProUGUI>();
            dismiss.text = "[Press any key to continue]";
            dismiss.fontSize = 14;
            dismiss.alignment = TextAlignmentOptions.Center;
            dismiss.color = new Color(0.5f, 0.5f, 0.5f);
            var dismissRT = dismissGO.GetComponent<RectTransform>();
            dismissRT.anchorMin = new Vector2(0f, 0f);
            dismissRT.anchorMax = new Vector2(1f, 0.15f);
            dismissRT.offsetMin = new Vector2(8f, 4f);
            dismissRT.offsetMax = new Vector2(-8f, -4f);

            briefGO.AddComponent<MissionBriefingDismisser>();
        }

        // ─── HUD Wiring ─────────────────────────────

        void WireHUD(HUDController hud, RectTransform canvasRT)
        {
            // RS Gauge (bottom-center)
            var rsGaugeRT = CreatePanel(canvasRT, "RSGauge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(200f, 60f));
            var rsFillGO = CreateChild(rsGaugeRT, "RSFill");
            var rsFill = rsFillGO.AddComponent<Image>();
            rsFill.color = new Color(0.9f, 0.85f, 0.3f, 0.9f);
            rsFill.type = Image.Type.Filled;
            rsFill.fillMethod = Image.FillMethod.Horizontal;
            SetStretchAll(rsFillGO.GetComponent<RectTransform>());

            var rsTextGO = CreateChild(rsGaugeRT, "RSText");
            var rsText = rsTextGO.AddComponent<TextMeshProUGUI>();
            rsText.text = "RS: 0";
            rsText.fontSize = 20;
            rsText.alignment = TextAlignmentOptions.Center;
            rsText.color = Color.white;
            SetStretchAll(rsTextGO.GetComponent<RectTransform>());

            // Aether Charge Bar (top-right)
            var aetherRT = CreatePanel(canvasRT, "AetherBar", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-120f, -30f), new Vector2(200f, 24f));
            var aetherFill = aetherRT.gameObject.AddComponent<Image>();
            aetherFill.color = new Color(0.2f, 0.6f, 0.9f, 0.8f);
            aetherFill.type = Image.Type.Filled;
            aetherFill.fillMethod = Image.FillMethod.Horizontal;

            var aetherTextGO = CreateChild(aetherRT, "AetherText");
            var aetherText = aetherTextGO.AddComponent<TextMeshProUGUI>();
            aetherText.text = "Aether: 0";
            aetherText.fontSize = 20;
            aetherText.alignment = TextAlignmentOptions.Center;
            aetherText.color = Color.white;
            SetStretchAll(aetherTextGO.GetComponent<RectTransform>());

            // Interaction Prompt (center-bottom)
            var promptRT = CreatePanel(canvasRT, "InteractionPrompt", new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f),
                Vector2.zero, new Vector2(500f, 40f));
            promptRT.gameObject.SetActive(false);
            var promptBG = promptRT.gameObject.AddComponent<Image>();
            promptBG.color = new Color(0f, 0f, 0f, 0.6f);

            var promptTextGO = CreateChild(promptRT, "PromptText");
            var promptText = promptTextGO.AddComponent<TextMeshProUGUI>();
            promptText.text = "";
            promptText.fontSize = 18;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = new Color(1f, 0.95f, 0.7f);
            SetStretchAll(promptTextGO.GetComponent<RectTransform>());

            // Zone Name (top-center)
            var zoneRT = CreatePanel(canvasRT, "ZoneName", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(300f, 36f));
            var zoneText = zoneRT.gameObject.AddComponent<TextMeshProUGUI>();
            zoneText.text = "Echohaven";
            zoneText.fontSize = 22;
            zoneText.alignment = TextAlignmentOptions.Center;
            zoneText.color = new Color(0.9f, 0.85f, 0.6f);

            // Boss Health Panel (top-center, hidden)
            var bossRT = CreatePanel(canvasRT, "BossHealthPanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -100f), new Vector2(400f, 30f));
            bossRT.gameObject.SetActive(false);
            var bossBG = bossRT.gameObject.AddComponent<Image>();
            bossBG.color = new Color(0.15f, 0.05f, 0.05f, 0.7f);

            var bossFillGO = CreateChild(bossRT, "BossFill");
            var bossFill = bossFillGO.AddComponent<Image>();
            bossFill.color = new Color(0.8f, 0.15f, 0.1f);
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;
            SetStretchAll(bossFillGO.GetComponent<RectTransform>());

            var bossNameGO = CreateChild(bossRT, "BossName");
            var bossNameText = bossNameGO.AddComponent<TextMeshProUGUI>();
            bossNameText.text = "";
            bossNameText.fontSize = 20;
            bossNameText.alignment = TextAlignmentOptions.Center;
            bossNameText.color = Color.white;
            SetStretchAll(bossNameGO.GetComponent<RectTransform>());

            // Wave Counter Panel (top-left, hidden)
            var waveRT = CreatePanel(canvasRT, "WaveCounterPanel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(120f, -100f), new Vector2(200f, 50f));
            waveRT.gameObject.SetActive(false);

            var waveTextGO = CreateChild(waveRT, "WaveText");
            var waveText = waveTextGO.AddComponent<TextMeshProUGUI>();
            waveText.text = "Wave 1/3";
            waveText.fontSize = 18;
            waveText.alignment = TextAlignmentOptions.Center;
            waveText.color = new Color(1f, 0.8f, 0.3f);
            SetStretchAll(waveTextGO.GetComponent<RectTransform>());

            // Achievement Toast (top-right, hidden)
            var achieveRT = CreatePanel(canvasRT, "AchievementToast", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-160f, -70f), new Vector2(280f, 40f));
            achieveRT.gameObject.SetActive(false);
            var achieveBG = achieveRT.gameObject.AddComponent<Image>();
            achieveBG.color = new Color(0.1f, 0.1f, 0.2f, 0.85f);

            var achieveTextGO = CreateChild(achieveRT, "AchieveText");
            var achieveText = achieveTextGO.AddComponent<TextMeshProUGUI>();
            achieveText.text = "";
            achieveText.fontSize = 20;
            achieveText.alignment = TextAlignmentOptions.Center;
            achieveText.color = new Color(1f, 0.9f, 0.4f);
            SetStretchAll(achieveTextGO.GetComponent<RectTransform>());

            // Moon Trophy Banner (center, hidden)
            var trophyRT = CreatePanel(canvasRT, "MoonTrophyPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(500f, 100f));
            trophyRT.gameObject.SetActive(false);
            var trophyBG = trophyRT.gameObject.AddComponent<Image>();
            trophyBG.color = new Color(0.05f, 0.05f, 0.15f, 0.9f);

            var trophyTextGO = CreateChild(trophyRT, "TrophyText");
            var trophyText = trophyTextGO.AddComponent<TextMeshProUGUI>();
            trophyText.text = "";
            trophyText.fontSize = 28;
            trophyText.alignment = TextAlignmentOptions.Center;
            trophyText.color = new Color(1f, 0.95f, 0.5f);
            var trophyTextRT = trophyTextGO.GetComponent<RectTransform>();
            trophyTextRT.anchorMin = new Vector2(0f, 0.4f);
            trophyTextRT.anchorMax = new Vector2(1f, 1f);
            trophyTextRT.offsetMin = Vector2.zero;
            trophyTextRT.offsetMax = Vector2.zero;

            var trophySubGO = CreateChild(trophyRT, "TrophySubtext");
            var trophySubtext = trophySubGO.AddComponent<TextMeshProUGUI>();
            trophySubtext.text = "";
            trophySubtext.fontSize = 20;
            trophySubtext.alignment = TextAlignmentOptions.Center;
            trophySubtext.color = new Color(0.8f, 0.8f, 0.9f);
            var trophySubRT = trophySubGO.GetComponent<RectTransform>();
            trophySubRT.anchorMin = new Vector2(0f, 0f);
            trophySubRT.anchorMax = new Vector2(1f, 0.4f);
            trophySubRT.offsetMin = Vector2.zero;
            trophySubRT.offsetMax = Vector2.zero;

            // Objective Panel — top-right, larger so objectives are easy to read
            var objRT = CreatePanel(canvasRT, "ObjectivePanel", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -60f), new Vector2(340f, 100f));
            var objBG = objRT.gameObject.AddComponent<Image>();
            objBG.color = new Color(0f, 0f, 0f, 0.5f);

            var objHeaderGO = CreateChild(objRT, "ObjectiveHeader");
            var objHeader = objHeaderGO.AddComponent<TextMeshProUGUI>();
            objHeader.text = "MISSION";
            objHeader.fontSize = 14;
            objHeader.fontStyle = FontStyles.Bold;
            objHeader.alignment = TextAlignmentOptions.Right;
            objHeader.color = new Color(1f, 0.88f, 0.3f);
            var objHeaderRT = objHeaderGO.GetComponent<RectTransform>();
            objHeaderRT.anchorMin = new Vector2(0f, 0.65f);
            objHeaderRT.anchorMax = new Vector2(1f, 1f);
            objHeaderRT.offsetMin = new Vector2(8f, 2f);
            objHeaderRT.offsetMax = new Vector2(-8f, -2f);

            var objTextGO = CreateChild(objRT, "ObjectiveText");
            var objText = objTextGO.AddComponent<TextMeshProUGUI>();
            objText.text = "Explore Echohaven — dig up buried structures";
            objText.fontSize = 20;
            objText.alignment = TextAlignmentOptions.Right;
            objText.color = new Color(0.9f, 0.9f, 0.7f);
            var objTextRT = objTextGO.GetComponent<RectTransform>();
            objTextRT.anchorMin = new Vector2(0f, 0f);
            objTextRT.anchorMax = new Vector2(1f, 0.65f);
            objTextRT.offsetMin = new Vector2(8f, 4f);
            objTextRT.offsetMax = new Vector2(-8f, -2f);

            // ─── Inject references via reflection ────
            SetField(hud, "rsGauge", rsGaugeRT);
            SetField(hud, "rsFillImage", rsFill);
            SetField(hud, "rsValueText", rsText);
            SetField(hud, "aetherChargeBar", aetherFill);
            SetField(hud, "aetherValueText", aetherText);
            SetField(hud, "interactionPrompt", promptRT);
            SetField(hud, "interactionText", promptText);
            SetField(hud, "zoneNameText", zoneText);
            SetField(hud, "bossHealthPanel", bossRT);
            SetField(hud, "bossHealthFill", bossFill);
            SetField(hud, "bossNameText", bossNameText);
            SetField(hud, "waveCounterPanel", waveRT);
            SetField(hud, "waveCounterText", waveText);
            SetField(hud, "achievementToastPanel", achieveRT);
            SetField(hud, "achievementToastText", achieveText);
            SetField(hud, "moonTrophyPanel", trophyRT);
            SetField(hud, "moonTrophyText", trophyText);
            SetField(hud, "moonTrophySubtext", trophySubtext);
            SetField(hud, "objectivePanel", objRT);
            SetField(hud, "objectiveText", objText);

            Debug.Log("[RuntimeHUDBuilder] HUD Canvas + all panels wired to HUDController.");
        }

        // ─── Pause Menu ─────────────────────────────

        void BuildPauseMenu(RectTransform canvasRT)
        {
            _pauseMenuPanel = new GameObject("PauseMenu");
            _pauseMenuPanel.transform.SetParent(canvasRT);
            var rt = _pauseMenuPanel.AddComponent<RectTransform>();
            SetStretchAll(rt);

            var bg = _pauseMenuPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);
            bg.raycastTarget = true;

            // Title
            var titleGO = CreateChild(rt, "PauseTitle");
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "PAUSED";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.95f, 0.6f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.3f, 0.6f);
            titleRT.anchorMax = new Vector2(0.7f, 0.75f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Resume Button
            CreatePauseButton(rt, "ResumeBtn", "Resume", new Vector2(0.5f, 0.5f), () =>
            {
                GameEvents.FireTogglePause();
            });

            // Quit Button
            CreatePauseButton(rt, "QuitBtn", "Quit to Desktop", new Vector2(0.5f, 0.4f), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            _pauseMenuPanel.SetActive(false);
        }

        void CreatePauseButton(RectTransform parent, string name, string label, Vector2 anchorPos,
            UnityEngine.Events.UnityAction onClick)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = anchorPos - new Vector2(0.1f, 0.02f);
            rt.anchorMax = anchorPos + new Vector2(0.1f, 0.02f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var textGO = CreateChild(rt, "Text");
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            SetStretchAll(textGO.GetComponent<RectTransform>());
        }

        void HandleTogglePause()
        {
            _isPaused = !_isPaused;
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(_isPaused);
            Time.timeScale = _isPaused ? 0f : 1f;
        }

        // ─── Damage Number Pool ─────────────────────

        void BuildDamageNumberPool(RectTransform canvasRT)
        {
            var containerGO = new GameObject("DamageNumbers");
            containerGO.transform.SetParent(canvasRT);
            _damageNumberContainer = containerGO.AddComponent<RectTransform>();
            SetStretchAll(_damageNumberContainer);
            _damageNumberContainer.gameObject.AddComponent<CanvasGroup>().interactable = false;

            _damagePool = new TextMeshProUGUI[DamagePoolSize];
            for (int i = 0; i < DamagePoolSize; i++)
            {
                var go = new GameObject($"Dmg_{i}");
                go.transform.SetParent(_damageNumberContainer);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 30f);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 24;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.3f, 0.2f);
                tmp.fontStyle = FontStyles.Bold;
                go.SetActive(false);
                _damagePool[i] = tmp;
            }
        }

        /// <summary>
        /// Show a floating damage number at a world position.
        /// </summary>
        public void ShowDamageNumber(float damage, Vector3 worldPos)
        {
            if (_damagePool == null) return;
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            var screenPos = cam.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0f) return; // Behind camera

            var tmp = _damagePool[_damagePoolIndex];
            _damagePoolIndex = (_damagePoolIndex + 1) % DamagePoolSize;

            tmp.text = Mathf.RoundToInt(damage).ToString();
            tmp.gameObject.SetActive(true);
            tmp.GetComponent<RectTransform>().position = screenPos + new Vector3(
                Random.Range(-20f, 20f), Random.Range(10f, 30f), 0f);

            // Fade out via coroutine
            StartCoroutine(FadeDamageNumber(tmp));
        }

        System.Collections.IEnumerator FadeDamageNumber(TextMeshProUGUI tmp)
        {
            float t = 0f;
            Color baseColor = tmp.color;
            var rt = tmp.GetComponent<RectTransform>();
            Vector3 startPos = rt.position;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 1.5f;
                tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
                rt.position = startPos + new Vector3(0f, t * 40f, 0f);
                yield return null;
            }

            tmp.gameObject.SetActive(false);
            tmp.color = baseColor;
        }

        // ─── Quest Log UI ───────────────────────────

        void WireQuestLogUI(RectTransform canvasRT)
        {
            var questLog = QuestLogUI.Instance;
            if (questLog == null) return;

            // Create the panel structure that QuestLogUI expects
            var panelGO = new GameObject("QuestLogPanel");
            panelGO.transform.SetParent(canvasRT);
            var panelRT = panelGO.AddComponent<RectTransform>();
            SetStretchAll(panelRT);
            var panelBG = panelGO.AddComponent<Image>();
            panelBG.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);
            panelGO.SetActive(false);

            // Left side: quest list
            var listGO = new GameObject("QuestListContent");
            listGO.transform.SetParent(panelRT);
            var listRT = listGO.AddComponent<RectTransform>();
            listRT.anchorMin = new Vector2(0.02f, 0.05f);
            listRT.anchorMax = new Vector2(0.4f, 0.9f);
            listRT.offsetMin = Vector2.zero;
            listRT.offsetMax = Vector2.zero;
            listGO.AddComponent<VerticalLayoutGroup>();

            // Right side: detail
            var detailTitleGO = CreateChild(panelRT, "DetailTitle");
            var detailTitle = detailTitleGO.AddComponent<TextMeshProUGUI>();
            detailTitle.fontSize = 24;
            detailTitle.color = new Color(1f, 0.95f, 0.6f);
            var dtRT = detailTitleGO.GetComponent<RectTransform>();
            dtRT.anchorMin = new Vector2(0.45f, 0.8f);
            dtRT.anchorMax = new Vector2(0.95f, 0.9f);
            dtRT.offsetMin = Vector2.zero;
            dtRT.offsetMax = Vector2.zero;

            var detailDescGO = CreateChild(panelRT, "DetailDesc");
            var detailDesc = detailDescGO.AddComponent<TextMeshProUGUI>();
            detailDesc.fontSize = 18;
            detailDesc.color = new Color(0.8f, 0.8f, 0.85f);
            var ddRT = detailDescGO.GetComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0.45f, 0.4f);
            ddRT.anchorMax = new Vector2(0.95f, 0.78f);
            ddRT.offsetMin = Vector2.zero;
            ddRT.offsetMax = Vector2.zero;

            var objListGO = new GameObject("ObjectivesList");
            objListGO.transform.SetParent(panelRT);
            var olRT = objListGO.AddComponent<RectTransform>();
            olRT.anchorMin = new Vector2(0.45f, 0.1f);
            olRT.anchorMax = new Vector2(0.95f, 0.38f);
            olRT.offsetMin = Vector2.zero;
            olRT.offsetMax = Vector2.zero;

            var rewardGO = CreateChild(panelRT, "RewardText");
            var rewardText = rewardGO.AddComponent<TextMeshProUGUI>();
            rewardText.fontSize = 18;
            rewardText.color = new Color(0.9f, 0.85f, 0.4f);
            var rwRT = rewardGO.GetComponent<RectTransform>();
            rwRT.anchorMin = new Vector2(0.45f, 0.02f);
            rwRT.anchorMax = new Vector2(0.95f, 0.09f);
            rwRT.offsetMin = Vector2.zero;
            rwRT.offsetMax = Vector2.zero;

            // Inject via reflection
            SetField(questLog, "questLogPanel", panelGO);
            SetField(questLog, "questListContent", listGO.transform);
            SetField(questLog, "detailTitle", detailTitle);
            SetField(questLog, "detailDescription", detailDesc);
            SetField(questLog, "objectivesListContent", objListGO.transform);
            SetField(questLog, "rewardText", rewardText);

            Debug.Log("[RuntimeHUDBuilder] QuestLogUI panel wired.");
        }

        // ─── Tuning Mini-Game Overlay ────────────────

        void BuildTuningOverlay(RectTransform canvasRT)
        {
            // Full-screen darkened backdrop
            var overlayGO = new GameObject("TuningOverlay");
            overlayGO.transform.SetParent(canvasRT);
            var rt = overlayGO.AddComponent<RectTransform>();
            SetStretchAll(rt);
            overlayGO.SetActive(false);
            _tuningOverlayGO = overlayGO; // cache for T-key toggle

            var bg = overlayGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0.08f, 0.82f);

            // Title label
            var titleGO = CreateChild(rt, "TuningTitle");
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "RESONANCE TUNING";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.55f, 0.85f, 1f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.25f, 0.73f);
            titleRT.anchorMax = new Vector2(0.75f, 0.79f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // ─── Circular Dial Container ──────────────────────────────────
            // Fixed-size 300×300 centred on screen
            var dialGO = new GameObject("DialContainer");
            dialGO.transform.SetParent(rt);
            var dialRT = dialGO.AddComponent<RectTransform>();
            dialRT.anchorMin = new Vector2(0.5f, 0.5f);
            dialRT.anchorMax = new Vector2(0.5f, 0.5f);
            dialRT.pivot    = new Vector2(0.5f, 0.5f);
            dialRT.anchoredPosition = new Vector2(0f, 20f);
            dialRT.sizeDelta = new Vector2(300f, 300f);

            // Background ring (full 360° dark arc)
            var ringBGGO = CreateChild(dialRT, "DialRingBG");
            var ringBG = ringBGGO.AddComponent<Image>();
            ringBG.color = new Color(0.08f, 0.08f, 0.2f, 1f);
            ringBG.type = Image.Type.Filled;
            ringBG.fillMethod = Image.FillMethod.Radial360;
            ringBG.fillOrigin = (int)Image.Origin360.Top;
            ringBG.fillAmount = 1f;
            SetStretchAll(ringBGGO.GetComponent<RectTransform>());

            // Inner circle (dark centre so ring looks like a ring)
            var innerGO = CreateChild(dialRT, "DialInner");
            var inner = innerGO.AddComponent<Image>();
            inner.color = new Color(0f, 0f, 0.06f, 1f);
            var innerRT = innerGO.GetComponent<RectTransform>();
            innerRT.anchorMin = new Vector2(0.18f, 0.18f);
            innerRT.anchorMax = new Vector2(0.82f, 0.82f);
            innerRT.offsetMin = Vector2.zero;
            innerRT.offsetMax = Vector2.zero;

            // Cyan frequency needle (fillAmount driven by TuningDialUpdater)
            var needleGO = CreateChild(dialRT, "DialNeedle");
            var needleFill = needleGO.AddComponent<Image>();
            needleFill.color = new Color(0.2f, 0.75f, 1f, 0.92f);
            needleFill.type = Image.Type.Filled;
            needleFill.fillMethod = Image.FillMethod.Radial360;
            needleFill.fillOrigin = (int)Image.Origin360.Top;
            needleFill.fillAmount = 0f;
            SetStretchAll(needleGO.GetComponent<RectTransform>());

            // Golden 432 Hz target tick — thin rectangle rotated to 6 o'clock (= 0.5 of 864 Hz range)
            // Pivot at top-centre so rotation sweeps tip around the rim
            var tickGO = new GameObject("Target432Tick");
            tickGO.transform.SetParent(dialRT);
            var tickRT = tickGO.AddComponent<RectTransform>();
            tickRT.anchorMin = new Vector2(0.5f, 0.5f);
            tickRT.anchorMax = new Vector2(0.5f, 0.5f);
            tickRT.pivot = new Vector2(0.5f, 1f);        // pivot at top edge
            tickRT.anchoredPosition = Vector2.zero;
            tickRT.sizeDelta = new Vector2(5f, 150f);    // thin bar reaching to the rim
            tickRT.localEulerAngles = new Vector3(0f, 0f, 180f); // 180° = 6 o'clock = 432 Hz
            var tickImg = tickGO.AddComponent<Image>();
            tickImg.color = new Color(1f, 0.88f, 0.15f);

            // 432 label near the tick at 6 o'clock
            var tickLabelGO = CreateChild(dialRT, "Label432");
            var tickLabel = tickLabelGO.AddComponent<TextMeshProUGUI>();
            tickLabel.text = "432";
            tickLabel.fontSize = 16;
            tickLabel.alignment = TextAlignmentOptions.Center;
            tickLabel.color = new Color(1f, 0.88f, 0.3f);
            var tickLabelRT = tickLabelGO.GetComponent<RectTransform>();
            tickLabelRT.anchorMin = new Vector2(0.35f, 0.03f);
            tickLabelRT.anchorMax = new Vector2(0.65f, 0.17f);
            tickLabelRT.offsetMin = Vector2.zero;
            tickLabelRT.offsetMax = Vector2.zero;

            // Frequency readout at dial centre
            var freqGO = CreateChild(dialRT, "FrequencyDisplay");
            var freqText = freqGO.AddComponent<TextMeshProUGUI>();
            freqText.text = "0.0 Hz";
            freqText.fontSize = 38;
            freqText.fontStyle = FontStyles.Bold;
            freqText.alignment = TextAlignmentOptions.Center;
            freqText.color = new Color(0.4f, 0.9f, 1f);
            var freqRT = freqGO.GetComponent<RectTransform>();
            freqRT.anchorMin = new Vector2(0.2f, 0.38f);
            freqRT.anchorMax = new Vector2(0.8f, 0.62f);
            freqRT.offsetMin = Vector2.zero;
            freqRT.offsetMax = Vector2.zero;

            // Target Hz label (static, smaller)
            var targetLabelGO = CreateChild(dialRT, "TargetLabel");
            var targetLabel = targetLabelGO.AddComponent<TextMeshProUGUI>();
            targetLabel.text = "TARGET: 432 Hz";
            targetLabel.fontSize = 16;
            targetLabel.alignment = TextAlignmentOptions.Center;
            targetLabel.color = new Color(1f, 0.88f, 0.3f);
            var targetLabelRT = targetLabelGO.GetComponent<RectTransform>();
            targetLabelRT.anchorMin = new Vector2(0.2f, 0.28f);
            targetLabelRT.anchorMax = new Vector2(0.8f, 0.38f);
            targetLabelRT.offsetMin = Vector2.zero;
            targetLabelRT.offsetMax = Vector2.zero;

            // ─── Below-dial UI ────────────────────────────────────────────

            // Accuracy fill bar (thin strip below dial)
            var barBGGO = CreateChild(rt, "BarBackground");
            var barBG = barBGGO.AddComponent<Image>();
            barBG.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            var barBGRT = barBGGO.GetComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0.3f, 0.3f);
            barBGRT.anchorMax = new Vector2(0.7f, 0.335f);
            barBGRT.offsetMin = Vector2.zero;
            barBGRT.offsetMax = Vector2.zero;

            var fillGO = CreateChild(barBGRT, "BarFill");
            var fill = fillGO.AddComponent<Image>();
            fill.color = new Color(0.2f, 0.75f, 1f, 0.9f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            SetStretchAll(fillGO.GetComponent<RectTransform>());

            // Accuracy text
            var accGO = CreateChild(rt, "AccuracyText");
            var accText = accGO.AddComponent<TextMeshProUGUI>();
            accText.text = "Accuracy: 0%";
            accText.fontSize = 20;
            accText.alignment = TextAlignmentOptions.Center;
            accText.color = new Color(0.9f, 0.9f, 0.7f);
            var accRT = accGO.GetComponent<RectTransform>();
            accRT.anchorMin = new Vector2(0.35f, 0.23f);
            accRT.anchorMax = new Vector2(0.65f, 0.3f);
            accRT.offsetMin = Vector2.zero;
            accRT.offsetMax = Vector2.zero;

            // Instructions
            var instrGO = CreateChild(rt, "Instructions");
            var instrText = instrGO.AddComponent<TextMeshProUGUI>();
            instrText.text = "Move mouse left/right to tune the frequency\nClick to lock in";
            instrText.fontSize = 18;
            instrText.alignment = TextAlignmentOptions.Center;
            instrText.color = new Color(0.6f, 0.65f, 0.75f);
            var instrRT = instrGO.GetComponent<RectTransform>();
            instrRT.anchorMin = new Vector2(0.25f, 0.16f);
            instrRT.anchorMax = new Vector2(0.75f, 0.23f);
            instrRT.offsetMin = Vector2.zero;
            instrRT.offsetMax = Vector2.zero;

            // ─── Wire TuningMiniGameController ───────────────────────────
            var tuning = FindAnyObjectByType<Tartaria.Gameplay.TuningMiniGameController>();
            if (tuning != null)
            {
                SetField(tuning, "overlayPanel", overlayGO);
                SetField(tuning, "frequencyText", freqText);
                SetField(tuning, "accuracyFill", fill);
                SetField(tuning, "accuracyText", accText);
            }

            // Add dial updater component — drives the needle fill & frequency text
            var updater = overlayGO.AddComponent<TuningDialUpdater>();
            updater.Initialize(needleFill, freqText, accText, fill);

            Debug.Log("[RuntimeHUDBuilder] Tuning overlay built with 432 Hz radial dial.");
        }

        void BuildDiscoveryFlash(RectTransform canvasRT)
        {
            var flashGO = new GameObject("DiscoveryFlash");
            flashGO.transform.SetParent(canvasRT);
            var rt = flashGO.AddComponent<RectTransform>();
            SetStretchAll(rt);
            rt.SetAsLastSibling(); // Renders on top of everything

            _discoveryFlashImage = flashGO.AddComponent<Image>();
            _discoveryFlashImage.color = new Color(1f, 0.9f, 0.3f, 0f); // starts invisible
            _discoveryFlashImage.raycastTarget = false;
        }

        /// <summary>Triggers a golden screen flash to celebrate a building discovery.</summary>
        public void FlashDiscovery()
        {
            if (_flashRunning || _discoveryFlashImage == null) return;
            StartCoroutine(DoDiscoveryFlash());
        }

        System.Collections.IEnumerator DoDiscoveryFlash()
        {
            _flashRunning = true;
            float fadeIn  = 0.12f;
            float hold    = 0.15f;
            float fadeOut = 0.5f;
            float t = 0f;

            // Fade in to 65% opacity gold
            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / fadeIn) * 0.65f;
                _discoveryFlashImage.color = new Color(1f, 0.9f, 0.3f, a);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(hold);

            // Fade out
            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                float a = (1f - Mathf.Clamp01(t / fadeOut)) * 0.65f;
                _discoveryFlashImage.color = new Color(1f, 0.9f, 0.3f, a);
                yield return null;
            }

            _discoveryFlashImage.color = new Color(1f, 0.9f, 0.3f, 0f);
            _flashRunning = false;
        }

        // ─── World Map UI ────────────────────────────

        void BuildWorldMapUI(RectTransform canvasRT)
        {
            var map = WorldMapUI.Instance;
            if (map == null) return;

            // Root fullscreen panel
            var panelGO = new GameObject("WorldMapPanel");
            panelGO.transform.SetParent(canvasRT);
            var panelRT = panelGO.AddComponent<RectTransform>();
            SetStretchAll(panelRT);
            var panelBG = panelGO.AddComponent<Image>();
            panelBG.color = new Color(0.04f, 0.04f, 0.12f, 0.95f);
            panelGO.SetActive(false);
            _worldMapPanel = panelGO;

            // Title
            var titleGO = CreateChild(panelRT, "MapTitle");
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "✦  TARTARIA — 13 MOONS  ✦";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.92f, 0.4f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.9f);
            titleRT.anchorMax = new Vector2(0.9f, 0.98f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Close button (top-right)
            var closeBtnGO = new GameObject("CloseMapBtn");
            closeBtnGO.transform.SetParent(panelRT);
            var closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.93f, 0.92f);
            closeBtnRT.anchorMax = new Vector2(0.98f, 0.98f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;
            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImg;
            var closeLblGO = CreateChild(closeBtnRT, "X");
            var closeLbl = closeLblGO.AddComponent<TextMeshProUGUI>();
            closeLbl.text = "✕";
            closeLbl.fontSize = 20;
            closeLbl.alignment = TextAlignmentOptions.Center;
            closeLbl.color = Color.white;
            SetStretchAll(closeLblGO.GetComponent<RectTransform>());

            // Tab bar: Map | Codex
            var mapTabGO = new GameObject("MapTabBtn");
            mapTabGO.transform.SetParent(panelRT);
            var mapTabRT = mapTabGO.AddComponent<RectTransform>();
            mapTabRT.anchorMin = new Vector2(0.02f, 0.9f);
            mapTabRT.anchorMax = new Vector2(0.12f, 0.97f);
            mapTabRT.offsetMin = Vector2.zero;
            mapTabRT.offsetMax = Vector2.zero;
            var mapTabImg = mapTabGO.AddComponent<Image>();
            mapTabImg.color = new Color(0.95f, 0.82f, 0.35f, 0.9f);
            var mapTabBtn = mapTabGO.AddComponent<Button>();
            mapTabBtn.targetGraphic = mapTabImg;
            var mapTabLblGO = CreateChild(mapTabRT, "Label");
            var mapTabLbl = mapTabLblGO.AddComponent<TextMeshProUGUI>();
            mapTabLbl.text = "MAP";
            mapTabLbl.fontSize = 16;
            mapTabLbl.alignment = TextAlignmentOptions.Center;
            mapTabLbl.color = new Color(0.1f, 0.05f, 0f);
            SetStretchAll(mapTabLblGO.GetComponent<RectTransform>());

            var codexTabGO = new GameObject("CodexTabBtn");
            codexTabGO.transform.SetParent(panelRT);
            var codexTabRT = codexTabGO.AddComponent<RectTransform>();
            codexTabRT.anchorMin = new Vector2(0.13f, 0.9f);
            codexTabRT.anchorMax = new Vector2(0.23f, 0.97f);
            codexTabRT.offsetMin = Vector2.zero;
            codexTabRT.offsetMax = Vector2.zero;
            var codexTabImg = codexTabGO.AddComponent<Image>();
            codexTabImg.color = new Color(0.2f, 0.2f, 0.35f, 0.8f);
            var codexTabBtn = codexTabGO.AddComponent<Button>();
            codexTabBtn.targetGraphic = codexTabImg;
            var codexTabLblGO = CreateChild(codexTabRT, "Label");
            var codexTabLbl = codexTabLblGO.AddComponent<TextMeshProUGUI>();
            codexTabLbl.text = "CODEX";
            codexTabLbl.fontSize = 16;
            codexTabLbl.alignment = TextAlignmentOptions.Center;
            codexTabLbl.color = Color.white;
            SetStretchAll(codexTabLblGO.GetComponent<RectTransform>());

            // Node container (constellation field — left 70%)
            var nodeContainerGO = new GameObject("NodeContainer");
            nodeContainerGO.transform.SetParent(panelRT);
            var nodeContainerRT = nodeContainerGO.AddComponent<RectTransform>();
            nodeContainerRT.anchorMin = new Vector2(0f, 0.05f);
            nodeContainerRT.anchorMax = new Vector2(0.70f, 0.88f);
            nodeContainerRT.offsetMin = Vector2.zero;
            nodeContainerRT.offsetMax = Vector2.zero;
            nodeContainerGO.AddComponent<Image>().color = new Color(0f, 0f, 0.08f, 0.6f);

            // Detail panel (right 30%)
            var detailGO = new GameObject("ZoneDetailPanel");
            detailGO.transform.SetParent(panelRT);
            var detailRT = detailGO.AddComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.71f, 0.05f);
            detailRT.anchorMax = new Vector2(0.99f, 0.88f);
            detailRT.offsetMin = Vector2.zero;
            detailRT.offsetMax = Vector2.zero;
            detailGO.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.18f, 0.85f);

            var zoneNameGO = CreateChild(detailRT, "ZoneName");
            var zoneNameText = zoneNameGO.AddComponent<TextMeshProUGUI>();
            zoneNameText.text = "SELECT A MOON";
            zoneNameText.fontSize = 20;
            zoneNameText.alignment = TextAlignmentOptions.Center;
            zoneNameText.color = new Color(1f, 0.92f, 0.4f);
            var znRT = zoneNameGO.GetComponent<RectTransform>();
            znRT.anchorMin = new Vector2(0.02f, 0.82f);
            znRT.anchorMax = new Vector2(0.98f, 0.95f);
            znRT.offsetMin = Vector2.zero;
            znRT.offsetMax = Vector2.zero;

            var zoneDescGO = CreateChild(detailRT, "ZoneDesc");
            var zoneDescText = zoneDescGO.AddComponent<TextMeshProUGUI>();
            zoneDescText.text = "";
            zoneDescText.fontSize = 13;
            zoneDescText.color = new Color(0.8f, 0.8f, 0.88f);
            var zdRT = zoneDescGO.GetComponent<RectTransform>();
            zdRT.anchorMin = new Vector2(0.02f, 0.45f);
            zdRT.anchorMax = new Vector2(0.98f, 0.80f);
            zdRT.offsetMin = Vector2.zero;
            zdRT.offsetMax = Vector2.zero;

            var zoneStatusGO = CreateChild(detailRT, "ZoneStatus");
            var zoneStatusText = zoneStatusGO.AddComponent<TextMeshProUGUI>();
            zoneStatusText.text = "";
            zoneStatusText.fontSize = 14;
            zoneStatusText.alignment = TextAlignmentOptions.Center;
            zoneStatusText.color = new Color(0.5f, 0.85f, 0.6f);
            var zsRT = zoneStatusGO.GetComponent<RectTransform>();
            zsRT.anchorMin = new Vector2(0.02f, 0.35f);
            zsRT.anchorMax = new Vector2(0.98f, 0.44f);
            zsRT.offsetMin = Vector2.zero;
            zsRT.offsetMax = Vector2.zero;

            var zoneMoonGO = CreateChild(detailRT, "ZoneMoonIndex");
            var zoneMoonText = zoneMoonGO.AddComponent<TextMeshProUGUI>();
            zoneMoonText.text = "";
            zoneMoonText.fontSize = 13;
            zoneMoonText.alignment = TextAlignmentOptions.Center;
            zoneMoonText.color = new Color(0.7f, 0.7f, 0.8f);
            var zmRT = zoneMoonGO.GetComponent<RectTransform>();
            zmRT.anchorMin = new Vector2(0.02f, 0.26f);
            zmRT.anchorMax = new Vector2(0.98f, 0.34f);
            zmRT.offsetMin = Vector2.zero;
            zmRT.offsetMax = Vector2.zero;

            // Travel button
            var travelBtnGO = new GameObject("TravelBtn");
            travelBtnGO.transform.SetParent(detailRT);
            var travelBtnRT = travelBtnGO.AddComponent<RectTransform>();
            travelBtnRT.anchorMin = new Vector2(0.1f, 0.06f);
            travelBtnRT.anchorMax = new Vector2(0.9f, 0.18f);
            travelBtnRT.offsetMin = Vector2.zero;
            travelBtnRT.offsetMax = Vector2.zero;
            var travelBtnImg = travelBtnGO.AddComponent<Image>();
            travelBtnImg.color = new Color(0.95f, 0.82f, 0.35f, 0.85f);
            var travelBtn = travelBtnGO.AddComponent<Button>();
            travelBtn.targetGraphic = travelBtnImg;
            var travelLblGO = CreateChild(travelBtnRT, "Label");
            var travelLbl = travelLblGO.AddComponent<TextMeshProUGUI>();
            travelLbl.text = "TRAVEL";
            travelLbl.fontSize = 18;
            travelLbl.fontStyle = FontStyles.Bold;
            travelLbl.alignment = TextAlignmentOptions.Center;
            travelLbl.color = new Color(0.1f, 0.05f, 0f);
            SetStretchAll(travelLblGO.GetComponent<RectTransform>());

            // Codex panel (hidden by default; sits over the node container area)
            var codexPanelGO = new GameObject("CodexPanel");
            codexPanelGO.transform.SetParent(panelRT);
            var codexPanelRT = codexPanelGO.AddComponent<RectTransform>();
            codexPanelRT.anchorMin = new Vector2(0f, 0.05f);
            codexPanelRT.anchorMax = new Vector2(0.70f, 0.88f);
            codexPanelRT.offsetMin = Vector2.zero;
            codexPanelRT.offsetMax = Vector2.zero;
            codexPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0.06f, 0.9f);
            var codexTextGO = CreateChild(codexPanelRT, "CodexText");
            var codexText = codexTextGO.AddComponent<TextMeshProUGUI>();
            codexText.text = "Codex entries will appear here as you discover lore fragments.";
            codexText.fontSize = 14;
            codexText.color = new Color(0.75f, 0.75f, 0.85f);
            var codexTextRT = codexTextGO.GetComponent<RectTransform>();
            codexTextRT.anchorMin = new Vector2(0.02f, 0.02f);
            codexTextRT.anchorMax = new Vector2(0.98f, 0.98f);
            codexTextRT.offsetMin = Vector2.zero;
            codexTextRT.offsetMax = Vector2.zero;
            codexPanelGO.SetActive(false);

            // Wire via reflection
            SetField(map, "mapPanel", panelGO);
            SetField(map, "closeButton", closeBtn);
            SetField(map, "nodeContainer", nodeContainerRT as Transform);
            SetField(map, "zoneName", zoneNameText);
            SetField(map, "zoneDescription", zoneDescText);
            SetField(map, "zoneStatus", zoneStatusText);
            SetField(map, "zoneMoonIndex", zoneMoonText);
            SetField(map, "travelButton", travelBtn);
            SetField(map, "mapTabButton", mapTabBtn);
            SetField(map, "codexTabButton", codexTabBtn);
            SetField(map, "codexPanel", codexPanelGO);

            Debug.Log("[RuntimeHUDBuilder] WorldMapUI panel built and wired. Press M to open.");
        }

        // ─── Skill Tree UI ───────────────────────────

        void BuildSkillTreeUI(RectTransform canvasRT)
        {
            var skill = SkillTreeUI.Instance;
            if (skill == null) return;

            // Root fullscreen panel
            var panelGO = new GameObject("SkillTreePanel");
            panelGO.transform.SetParent(canvasRT);
            var panelRT = panelGO.AddComponent<RectTransform>();
            SetStretchAll(panelRT);
            panelGO.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.12f, 0.95f);
            panelGO.SetActive(false);
            _skillTreePanel = panelGO;

            // Header bar
            var headerGO = new GameObject("SkillTreeHeader");
            headerGO.transform.SetParent(panelRT);
            var headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 0.88f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;
            headerGO.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.18f, 0.9f);

            var treeTitleGO = CreateChild(headerRT, "TreeTitle");
            var treeTitleText = treeTitleGO.AddComponent<TextMeshProUGUI>();
            treeTitleText.text = "♫ RESONATOR";
            treeTitleText.fontSize = 24;
            treeTitleText.fontStyle = FontStyles.Bold;
            treeTitleText.alignment = TextAlignmentOptions.Center;
            treeTitleText.color = new Color(1f, 0.92f, 0.4f);
            var ttRT = treeTitleGO.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0.2f, 0.1f);
            ttRT.anchorMax = new Vector2(0.8f, 0.9f);
            ttRT.offsetMin = Vector2.zero;
            ttRT.offsetMax = Vector2.zero;

            var rsDisplayGO = CreateChild(headerRT, "RSDisplay");
            var rsDisplayText = rsDisplayGO.AddComponent<TextMeshProUGUI>();
            rsDisplayText.text = "RS: 0";
            rsDisplayText.fontSize = 16;
            rsDisplayText.alignment = TextAlignmentOptions.Right;
            rsDisplayText.color = new Color(0.9f, 0.85f, 0.3f);
            var rsDispRT = rsDisplayGO.GetComponent<RectTransform>();
            rsDispRT.anchorMin = new Vector2(0.82f, 0.1f);
            rsDispRT.anchorMax = new Vector2(0.98f, 0.9f);
            rsDispRT.offsetMin = Vector2.zero;
            rsDispRT.offsetMax = Vector2.zero;

            // Close button (top-right)
            var closeBtnGO = new GameObject("CloseSkillBtn");
            closeBtnGO.transform.SetParent(panelRT);
            var closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.95f, 0.93f);
            closeBtnRT.anchorMax = new Vector2(0.99f, 0.99f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;
            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImg;
            var closeLblGO = CreateChild(closeBtnRT, "X");
            var closeLbl = closeLblGO.AddComponent<TextMeshProUGUI>();
            closeLbl.text = "✕";
            closeLbl.fontSize = 18;
            closeLbl.alignment = TextAlignmentOptions.Center;
            closeLbl.color = Color.white;
            SetStretchAll(closeLblGO.GetComponent<RectTransform>());

            // Tab buttons (below header, 4 across the top of the content area)
            string[] tabNames = { "♫ RESONATOR", "⌂ ARCHITECT", "⚔ GUARDIAN", "✍ HISTORIAN" };
            string[] tabFields = { "resonatorTabButton", "architectTabButton", "guardianTabButton", "historianTabButton" };
            var tabButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float xMin = 0.02f + i * 0.245f;
                float xMax = xMin + 0.23f;
                var tabGO = new GameObject($"Tab_{tabFields[i]}");
                tabGO.transform.SetParent(panelRT);
                var tabRT = tabGO.AddComponent<RectTransform>();
                tabRT.anchorMin = new Vector2(xMin, 0.82f);
                tabRT.anchorMax = new Vector2(xMax, 0.89f);
                tabRT.offsetMin = Vector2.zero;
                tabRT.offsetMax = Vector2.zero;
                var tabImg = tabGO.AddComponent<Image>();
                tabImg.color = i == 0
                    ? new Color(0.95f, 0.82f, 0.35f, 0.9f)
                    : new Color(0.2f, 0.2f, 0.3f, 0.7f);
                var tabBtn = tabGO.AddComponent<Button>();
                tabBtn.targetGraphic = tabImg;
                var tabLblGO = CreateChild(tabRT, "Label");
                var tabLbl = tabLblGO.AddComponent<TextMeshProUGUI>();
                tabLbl.text = tabNames[i];
                tabLbl.fontSize = 13;
                tabLbl.alignment = TextAlignmentOptions.Center;
                tabLbl.color = i == 0 ? new Color(0.1f, 0.05f, 0f) : Color.white;
                SetStretchAll(tabLblGO.GetComponent<RectTransform>());
                tabButtons[i] = tabBtn;
                SetField(skill, tabFields[i], tabBtn);
            }

            // Node container (scrollable region — left 72%)
            var nodeContainerGO = new GameObject("SkillNodeContainer");
            nodeContainerGO.transform.SetParent(panelRT);
            var nodeContainerRT = nodeContainerGO.AddComponent<RectTransform>();
            nodeContainerRT.anchorMin = new Vector2(0f, 0.05f);
            nodeContainerRT.anchorMax = new Vector2(0.72f, 0.81f);
            nodeContainerRT.offsetMin = Vector2.zero;
            nodeContainerRT.offsetMax = Vector2.zero;
            nodeContainerGO.AddComponent<Image>().color = new Color(0f, 0f, 0.06f, 0.5f);

            // Detail panel (right 28%)
            var detailGO = new GameObject("SkillDetailPanel");
            detailGO.transform.SetParent(panelRT);
            var detailRT = detailGO.AddComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.73f, 0.05f);
            detailRT.anchorMax = new Vector2(0.99f, 0.81f);
            detailRT.offsetMin = Vector2.zero;
            detailRT.offsetMax = Vector2.zero;
            detailGO.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.18f, 0.85f);

            var detailNameGO = CreateChild(detailRT, "DetailName");
            var detailNameText = detailNameGO.AddComponent<TextMeshProUGUI>();
            detailNameText.text = "Select a skill";
            detailNameText.fontSize = 18;
            detailNameText.fontStyle = FontStyles.Bold;
            detailNameText.alignment = TextAlignmentOptions.Center;
            detailNameText.color = new Color(1f, 0.92f, 0.4f);
            var dnRT = detailNameGO.GetComponent<RectTransform>();
            dnRT.anchorMin = new Vector2(0.02f, 0.84f);
            dnRT.anchorMax = new Vector2(0.98f, 0.96f);
            dnRT.offsetMin = Vector2.zero;
            dnRT.offsetMax = Vector2.zero;

            var detailDescGO = CreateChild(detailRT, "DetailDesc");
            var detailDescText = detailDescGO.AddComponent<TextMeshProUGUI>();
            detailDescText.text = "";
            detailDescText.fontSize = 12;
            detailDescText.color = new Color(0.8f, 0.8f, 0.88f);
            var ddRT = detailDescGO.GetComponent<RectTransform>();
            ddRT.anchorMin = new Vector2(0.02f, 0.54f);
            ddRT.anchorMax = new Vector2(0.98f, 0.82f);
            ddRT.offsetMin = Vector2.zero;
            ddRT.offsetMax = Vector2.zero;

            var detailCostGO = CreateChild(detailRT, "DetailCost");
            var detailCostText = detailCostGO.AddComponent<TextMeshProUGUI>();
            detailCostText.text = "";
            detailCostText.fontSize = 14;
            detailCostText.alignment = TextAlignmentOptions.Center;
            detailCostText.color = new Color(0.9f, 0.85f, 0.3f);
            var dcRT = detailCostGO.GetComponent<RectTransform>();
            dcRT.anchorMin = new Vector2(0.02f, 0.44f);
            dcRT.anchorMax = new Vector2(0.98f, 0.53f);
            dcRT.offsetMin = Vector2.zero;
            dcRT.offsetMax = Vector2.zero;

            var detailModGO = CreateChild(detailRT, "DetailModifier");
            var detailModText = detailModGO.AddComponent<TextMeshProUGUI>();
            detailModText.text = "";
            detailModText.fontSize = 13;
            detailModText.alignment = TextAlignmentOptions.Center;
            detailModText.color = new Color(0.55f, 0.85f, 1f);
            var dmRT = detailModGO.GetComponent<RectTransform>();
            dmRT.anchorMin = new Vector2(0.02f, 0.34f);
            dmRT.anchorMax = new Vector2(0.98f, 0.43f);
            dmRT.offsetMin = Vector2.zero;
            dmRT.offsetMax = Vector2.zero;

            // Unlock button
            var unlockBtnGO = new GameObject("UnlockBtn");
            unlockBtnGO.transform.SetParent(detailRT);
            var unlockBtnRT = unlockBtnGO.AddComponent<RectTransform>();
            unlockBtnRT.anchorMin = new Vector2(0.1f, 0.06f);
            unlockBtnRT.anchorMax = new Vector2(0.9f, 0.18f);
            unlockBtnRT.offsetMin = Vector2.zero;
            unlockBtnRT.offsetMax = Vector2.zero;
            var unlockBtnImg = unlockBtnGO.AddComponent<Image>();
            unlockBtnImg.color = new Color(0.95f, 0.82f, 0.35f, 0.85f);
            var unlockBtn = unlockBtnGO.AddComponent<Button>();
            unlockBtn.targetGraphic = unlockBtnImg;
            var unlockLblGO = CreateChild(unlockBtnRT, "Label");
            var unlockLbl = unlockLblGO.AddComponent<TextMeshProUGUI>();
            unlockLbl.text = "UNLOCK";
            unlockLbl.fontSize = 16;
            unlockLbl.fontStyle = FontStyles.Bold;
            unlockLbl.alignment = TextAlignmentOptions.Center;
            unlockLbl.color = new Color(0.1f, 0.05f, 0f);
            SetStretchAll(unlockLblGO.GetComponent<RectTransform>());

            // Wire all refs via reflection
            SetField(skill, "skillTreePanel", panelGO);
            SetField(skill, "closeButton", closeBtn);
            SetField(skill, "nodeContainer", nodeContainerRT as Transform);
            SetField(skill, "treeTitle", treeTitleText);
            SetField(skill, "rsDisplay", rsDisplayText);
            SetField(skill, "detailName", detailNameText);
            SetField(skill, "detailDescription", detailDescText);
            SetField(skill, "detailCost", detailCostText);
            SetField(skill, "detailModifier", detailModText);
            SetField(skill, "unlockButton", unlockBtn);
            SetField(skill, "unlockButtonLabel", unlockLbl);

            Debug.Log("[RuntimeHUDBuilder] SkillTreeUI panel built and wired. Press K to open.");
        }

        // ─── AetherVision Overlay ─────────────────────

        void BuildAetherVisionOverlay(RectTransform canvasRT)
        {
            var overlayGO = new GameObject("AetherVisionOverlay");
            overlayGO.transform.SetParent(canvasRT);
            var rt = overlayGO.AddComponent<RectTransform>();
            SetStretchAll(rt);

            _aetherVisionOverlay = overlayGO.AddComponent<Image>();
            // Deep blue-cyan tint — unobtrusive at 0 alpha until activated
            _aetherVisionOverlay.color = new Color(0.05f, 0.35f, 0.85f, 0f);
            _aetherVisionOverlay.raycastTarget = false;

            // AetherVision scan-lines texture (1×1 white stands in; shader would replace this)
            overlayGO.SetActive(true); // stays active — alpha drives visibility

            Debug.Log("[RuntimeHUDBuilder] AetherVision overlay built.");
        }

        void HandleAetherVisionToggle()
        {
            _aetherVisionActive = !_aetherVisionActive;
            if (!_aetherFadeRunning && _aetherVisionOverlay != null)
                StartCoroutine(FadeAetherVision(_aetherVisionActive));
        }

        System.Collections.IEnumerator FadeAetherVision(bool fadeIn)
        {
            _aetherFadeRunning = true;
            float duration = 0.3f;
            float targetAlpha = fadeIn ? 0.18f : 0f;
            float startAlpha = _aetherVisionOverlay.color.a;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                _aetherVisionOverlay.color = new Color(0.05f, 0.35f, 0.85f, a);
                yield return null;
            }

            _aetherVisionOverlay.color = new Color(0.05f, 0.35f, 0.85f, targetAlpha);
            _aetherFadeRunning = false;
        }

        // ─── Helpers ─────────────────────────────────

        static RectTransform CreatePanel(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        static GameObject CreateChild(RectTransform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<RectTransform>();
            return go;
        }

        static void SetStretchAll(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void SetField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[RuntimeHUDBuilder] Field '{fieldName}' not found on {type.Name}");
            }
        }

        // ─── Old World Archive ────────────────────────

        void BuildArchiveUI(RectTransform canvasRT)
        {
            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("ArchivePanel");
            panelGO.transform.SetParent(canvasRT, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            SetStretchAll(panelRT);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.03f, 0.03f, 0.08f, 0.97f);
            panelGO.SetActive(false);
            _archivePanel = panelGO;

            var archiveUI = panelGO.AddComponent<ArchiveUI>();

            // ── Header bar ────────────────────────────────────────────────────
            var headerRT = CreatePanel(panelRT, "ArchiveHeader",
                new Vector2(0f, 0.92f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero);
            var headerImg = headerRT.gameObject.AddComponent<Image>();
            headerImg.color = new Color(0.06f, 0.06f, 0.14f, 1f);

            // Title
            var titleGO = new GameObject("ArchiveTitle");
            titleGO.transform.SetParent(headerRT, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.3f, 0f);
            titleRT.anchorMax = new Vector2(0.7f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "\u2736  TARTARIA — OLD WORLD ARCHIVE  \u2736";
            titleTMP.fontSize = 22;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.95f, 0.82f, 0.35f);

            // Entry count label
            var countGO = new GameObject("EntryCount");
            countGO.transform.SetParent(headerRT, false);
            var countRT = countGO.AddComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0.72f, 0.15f);
            countRT.anchorMax = new Vector2(0.88f, 0.85f);
            countRT.offsetMin = Vector2.zero;
            countRT.offsetMax = Vector2.zero;
            var countTMP = countGO.AddComponent<TextMeshProUGUI>();
            countTMP.text = "0 ENTRIES";
            countTMP.fontSize = 12;
            countTMP.color = new Color(0.5f, 0.5f, 0.65f);
            countTMP.alignment = TextAlignmentOptions.Right;

            // Search field
            var searchGO = new GameObject("SearchField");
            searchGO.transform.SetParent(headerRT, false);
            var searchRT = searchGO.AddComponent<RectTransform>();
            searchRT.anchorMin = new Vector2(0.01f, 0.2f);
            searchRT.anchorMax = new Vector2(0.26f, 0.8f);
            searchRT.offsetMin = Vector2.zero;
            searchRT.offsetMax = Vector2.zero;
            var searchBg = searchGO.AddComponent<Image>();
            searchBg.color = new Color(0.08f, 0.08f, 0.15f, 0.9f);
            var searchInput = searchGO.AddComponent<TMP_InputField>();
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(searchGO.transform, false);
            var placeholderRT = placeholderGO.AddComponent<RectTransform>();
            SetStretchAll(placeholderRT);
            placeholderRT.offsetMin = new Vector2(6f, 0f);
            var placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = "Search archive...";
            placeholderTMP.fontSize = 12;
            placeholderTMP.color = new Color(0.3f, 0.3f, 0.4f);
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(searchGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            SetStretchAll(textRT);
            textRT.offsetMin = new Vector2(6f, 0f);
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.fontSize = 12;
            textTMP.color = Color.white;
            searchInput.textComponent = textTMP;
            searchInput.placeholder = placeholderTMP;

            // Close button
            var closeBtnGO = new GameObject("CloseButton");
            closeBtnGO.transform.SetParent(headerRT, false);
            var closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.96f, 0.15f);
            closeBtnRT.anchorMax = new Vector2(1.00f, 0.85f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;
            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.color = new Color(0.6f, 0.1f, 0.1f, 0.8f);
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImg;
            var closeLblGO = new GameObject("Label");
            closeLblGO.transform.SetParent(closeBtnGO.transform, false);
            var closeLblRT = closeLblGO.AddComponent<RectTransform>();
            SetStretchAll(closeLblRT);
            var closeLblTMP = closeLblGO.AddComponent<TextMeshProUGUI>();
            closeLblTMP.text = "✕";
            closeLblTMP.fontSize = 16;
            closeLblTMP.alignment = TextAlignmentOptions.Center;
            closeLblTMP.color = Color.white;

            // ── Category tabs (left column header) ────────────────────────────
            string[] catNames = { "ALL", "ARCHITECTURE", "TECHNOLOGY", "ASTRONOMY",
                                  "CULTURE", "MYSTERY", "SCIENCE", "PEOPLE", "EVIDENCE" };
            var tabButtons = new Button[catNames.Length];
            for (int ci = 0; ci < catNames.Length; ci++)
            {
                float yMax = 0.92f - ci * (0.92f / catNames.Length);
                float yMin = yMax - (0.92f / catNames.Length) + 0.004f;
                var tabRT = CreatePanel(panelRT, $"Tab_{catNames[ci]}",
                    new Vector2(0f, yMin), new Vector2(0.18f, yMax),
                    Vector2.zero, Vector2.zero);
                var tabImg = tabRT.gameObject.AddComponent<Image>();
                tabImg.color = ci == 0
                    ? new Color(0.95f, 0.82f, 0.35f, 0.9f)
                    : new Color(0.10f, 0.10f, 0.20f, 0.75f);
                var tabBtn = tabRT.gameObject.AddComponent<Button>();
                tabBtn.targetGraphic = tabImg;
                var tabLblGO = new GameObject("Label");
                tabLblGO.transform.SetParent(tabRT, false);
                var tabLblRT = tabLblGO.AddComponent<RectTransform>();
                SetStretchAll(tabLblRT);
                tabLblRT.offsetMin = new Vector2(6f, 0f);
                var tabLblTMP = tabLblGO.AddComponent<TextMeshProUGUI>();
                tabLblTMP.text = catNames[ci];
                tabLblTMP.fontSize = 11;
                tabLblTMP.alignment = TextAlignmentOptions.MidlineLeft;
                tabLblTMP.color = ci == 0
                    ? new Color(0.06f, 0.06f, 0.10f)
                    : new Color(0.70f, 0.70f, 0.80f);
                int capturedIndex = ci - 1;  // -1 = ALL
                tabBtn.onClick.AddListener(() => archiveUI.SelectCategory(capturedIndex));
                tabButtons[ci] = tabBtn;
            }

            // ── Entry list (scroll view) ───────────────────────────────────────
            var listViewGO = new GameObject("EntryScrollView");
            listViewGO.transform.SetParent(panelGO.transform, false);
            var listViewRT = listViewGO.AddComponent<RectTransform>();
            listViewRT.anchorMin = new Vector2(0.18f, 0.00f);
            listViewRT.anchorMax = new Vector2(0.45f, 0.92f);
            listViewRT.offsetMin = new Vector2(4f, 4f);
            listViewRT.offsetMax = new Vector2(-4f, -4f);
            var listViewImg = listViewGO.AddComponent<Image>();
            listViewImg.color = new Color(0.05f, 0.05f, 0.10f, 0.7f);
            var scrollRect = listViewGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(listViewGO.transform, false);
            var viewportRT = viewportGO.AddComponent<RectTransform>();
            SetStretchAll(viewportRT);
            var mask = viewportGO.AddComponent<RectMask2D>();

            var contentGO = new GameObject("EntryListContent");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3f;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;

            // ── Detail panel (right side) ─────────────────────────────────────
            var detailRT = CreatePanel(panelRT, "ArchiveDetail",
                new Vector2(0.45f, 0.00f), new Vector2(1.00f, 0.92f),
                Vector2.zero, Vector2.zero);
            var detailImg = detailRT.gameObject.AddComponent<Image>();
            detailImg.color = new Color(0.04f, 0.04f, 0.10f, 0.8f);

            // Detail title
            var dTitleGO = new GameObject("DetailTitle");
            dTitleGO.transform.SetParent(detailRT, false);
            var dTitleRT = dTitleGO.AddComponent<RectTransform>();
            dTitleRT.anchorMin = new Vector2(0.02f, 0.88f);
            dTitleRT.anchorMax = new Vector2(0.98f, 0.98f);
            dTitleRT.offsetMin = Vector2.zero;
            dTitleRT.offsetMax = Vector2.zero;
            var dTitleTMP = dTitleGO.AddComponent<TextMeshProUGUI>();
            dTitleTMP.text = "SELECT AN ENTRY";
            dTitleTMP.fontSize = 20;
            dTitleTMP.fontStyle = FontStyles.Bold;
            dTitleTMP.color = new Color(0.95f, 0.82f, 0.35f);
            dTitleTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Detail category
            var dCatGO = new GameObject("DetailCategory");
            dCatGO.transform.SetParent(detailRT, false);
            var dCatRT = dCatGO.AddComponent<RectTransform>();
            dCatRT.anchorMin = new Vector2(0.02f, 0.83f);
            dCatRT.anchorMax = new Vector2(0.98f, 0.89f);
            dCatRT.offsetMin = Vector2.zero;
            dCatRT.offsetMax = Vector2.zero;
            var dCatTMP = dCatGO.AddComponent<TextMeshProUGUI>();
            dCatTMP.text = "";
            dCatTMP.fontSize = 12;
            dCatTMP.color = new Color(0.55f, 0.65f, 0.85f);

            // Detail body (scroll)
            var dBodyScrollGO = new GameObject("DetailBodyScroll");
            dBodyScrollGO.transform.SetParent(detailRT, false);
            var dBodyScrollRT = dBodyScrollGO.AddComponent<RectTransform>();
            dBodyScrollRT.anchorMin = new Vector2(0.02f, 0.32f);
            dBodyScrollRT.anchorMax = new Vector2(0.98f, 0.82f);
            dBodyScrollRT.offsetMin = Vector2.zero;
            dBodyScrollRT.offsetMax = Vector2.zero;
            var dBodyImg = dBodyScrollGO.AddComponent<Image>();
            dBodyImg.color = new Color(0f, 0f, 0f, 0f);
            var dBodyScroll = dBodyScrollGO.AddComponent<ScrollRect>();
            dBodyScroll.horizontal = false;

            var dBodyViewGO = new GameObject("Viewport");
            dBodyViewGO.transform.SetParent(dBodyScrollGO.transform, false);
            var dBodyViewRT = dBodyViewGO.AddComponent<RectTransform>();
            SetStretchAll(dBodyViewRT);
            dBodyViewGO.AddComponent<RectMask2D>();

            var dBodyContentGO = new GameObject("Content");
            dBodyContentGO.transform.SetParent(dBodyViewGO.transform, false);
            var dBodyContentRT = dBodyContentGO.AddComponent<RectTransform>();
            dBodyContentRT.anchorMin = new Vector2(0f, 1f);
            dBodyContentRT.anchorMax = new Vector2(1f, 1f);
            dBodyContentRT.pivot = new Vector2(0.5f, 1f);
            dBodyContentRT.offsetMin = Vector2.zero;
            dBodyContentRT.offsetMax = Vector2.zero;
            var dBodyTMP = dBodyContentGO.AddComponent<TextMeshProUGUI>();
            dBodyTMP.text = "";
            dBodyTMP.fontSize = 13;
            dBodyTMP.color = new Color(0.85f, 0.85f, 0.92f);
            dBodyTMP.textWrappingMode = TextWrappingModes.Normal;
            var dBodyCSF = dBodyContentGO.AddComponent<ContentSizeFitter>();
            dBodyCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            dBodyScroll.content = dBodyContentRT;
            dBodyScroll.viewport = dBodyViewRT;

            // Anastasia quote block (background + text separated to avoid dual-Graphic conflict)
            var quoteGO = new GameObject("AnastasiaQuote");
            quoteGO.transform.SetParent(detailRT, false);
            var quoteRT = quoteGO.AddComponent<RectTransform>();
            quoteRT.anchorMin = new Vector2(0.04f, 0.18f);
            quoteRT.anchorMax = new Vector2(0.96f, 0.31f);
            quoteRT.offsetMin = Vector2.zero;
            quoteRT.offsetMax = Vector2.zero;
            var quoteImg = quoteGO.AddComponent<Image>();
            quoteImg.color = new Color(0.30f, 0.10f, 0.40f, 0.35f);
            var quoteTxtGO = new GameObject("QuoteText");
            quoteTxtGO.transform.SetParent(quoteGO.transform, false);
            var quoteTxtRT = quoteTxtGO.AddComponent<RectTransform>();
            SetStretchAll(quoteTxtRT);
            var quoteTMP = quoteTxtGO.AddComponent<TextMeshProUGUI>();
            quoteTMP.text = "";
            quoteTMP.fontSize = 12;
            quoteTMP.fontStyle = FontStyles.Italic;
            quoteTMP.color = new Color(0.88f, 0.68f, 0.95f);
            quoteTMP.alignment = TextAlignmentOptions.Center;
            quoteTMP.textWrappingMode = TextWrappingModes.Normal;
            quoteGO.SetActive(false);

            // Related entries panel
            var relatedRT = CreatePanel(detailRT, "RelatedEntries",
                new Vector2(0.02f, 0.00f), new Vector2(0.98f, 0.17f),
                Vector2.zero, Vector2.zero);
            var relHeaderGO = new GameObject("RelatedHeader");
            relHeaderGO.transform.SetParent(relatedRT, false);
            var relHeaderRT = relHeaderGO.AddComponent<RectTransform>();
            relHeaderRT.anchorMin = new Vector2(0f, 0.75f);
            relHeaderRT.anchorMax = Vector2.one;
            relHeaderRT.offsetMin = Vector2.zero;
            relHeaderRT.offsetMax = Vector2.zero;
            var relHeaderTMP = relHeaderGO.AddComponent<TextMeshProUGUI>();
            relHeaderTMP.text = "RELATED ENTRIES";
            relHeaderTMP.fontSize = 11;
            relHeaderTMP.color = new Color(0.4f, 0.5f, 0.7f);
            relHeaderTMP.fontStyle = FontStyles.Bold;

            var relListGO = new GameObject("RelatedList");
            relListGO.transform.SetParent(relatedRT, false);
            var relListRT = relListGO.AddComponent<RectTransform>();
            relListRT.anchorMin = new Vector2(0f, 0f);
            relListRT.anchorMax = new Vector2(1f, 0.74f);
            relListRT.offsetMin = Vector2.zero;
            relListRT.offsetMax = Vector2.zero;
            var relVlg = relListGO.AddComponent<HorizontalLayoutGroup>();
            relVlg.spacing = 4f;
            relVlg.childControlHeight = true;
            relVlg.childControlWidth = false;

            // ── Wire ArchiveUI fields ──────────────────────────────────────────
            SetField(archiveUI, "archivePanel",        panelGO);
            SetField(archiveUI, "closeButton",         closeBtn);
            SetField(archiveUI, "searchField",         searchInput);
            SetField(archiveUI, "entryCountLabel",     countTMP);
            SetField(archiveUI, "entryListContainer",  contentRT);
            SetField(archiveUI, "detailTitle",         dTitleTMP);
            SetField(archiveUI, "detailCategory",      dCatTMP);
            SetField(archiveUI, "detailBody",          dBodyTMP);
            SetField(archiveUI, "anastasiaQuoteText",  quoteTMP);
            SetField(archiveUI, "relatedContainer",    relListRT.transform);
            // Wire the 9 tabs (ALL + 8 categories)
            SetField(archiveUI, "categoryTabButtons",  tabButtons);
        }

        void HandleBuildingRestored(string buildingId)
        {
            ArchiveManager.Instance?.UnlockByTrigger(buildingId);
        }
    }

    /// <summary>
    /// Subscribes to TuningMiniGameController.OnFrequencyChanged and keeps the
    /// radial dial needle + frequency text in sync at runtime.
    /// Added to the TuningOverlay GameObject by RuntimeHUDBuilder.
    /// </summary>
    [DisallowMultipleComponent]
    internal class TuningDialUpdater : MonoBehaviour
    {
        Image _needle;
        TextMeshProUGUI _freqText;
        TextMeshProUGUI _accText;
        Image _accFill;
        Tartaria.Gameplay.TuningMiniGameController _tuning;
        const float MaxFrequency = 864f; // targetFreq × 2

        public void Initialize(Image needle, TextMeshProUGUI freqText,
            TextMeshProUGUI accText, Image accFill)
        {
            _needle   = needle;
            _freqText = freqText;
            _accText  = accText;
            _accFill  = accFill;
        }

        void OnEnable()
        {
            _tuning = FindAnyObjectByType<Tartaria.Gameplay.TuningMiniGameController>();
            if (_tuning != null)
                _tuning.OnFrequencyChanged += OnFrequencyChanged;
        }

        void OnDisable()
        {
            if (_tuning != null)
                _tuning.OnFrequencyChanged -= OnFrequencyChanged;
        }

        // Lazy-bind: TuningMiniGameController is created by InteractableBuilding.Awake()
        // which may run long after this overlay is built. Retry every frame until found.
        void Update()
        {
            if (_tuning != null) return;
            _tuning = FindAnyObjectByType<Tartaria.Gameplay.TuningMiniGameController>();
            if (_tuning != null)
                _tuning.OnFrequencyChanged += OnFrequencyChanged;
        }

        void OnFrequencyChanged(float freq)
        {
            if (_needle != null)
                _needle.fillAmount = Mathf.Clamp01(freq / MaxFrequency);

            if (_freqText != null)
                _freqText.text = $"{freq:F1} Hz";

            float acc = _tuning != null ? _tuning.CurrentAccuracy : 0f;
            if (_accFill != null)
                _accFill.fillAmount = acc;
            if (_accText != null)
                _accText.text = $"Accuracy: {acc:P0}";
        }
    }

    /// <summary>
    /// Auto-dismisses the startup mission briefing panel on any key press or after 10 seconds.
    /// </summary>
    internal class MissionBriefingDismisser : MonoBehaviour
    {
        float _timer = 10f;

        void Update()
        {
            _timer -= Time.deltaTime;
            bool keyPressed = UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame ?? false;
            if (_timer <= 0f || keyPressed)
                Destroy(gameObject);
        }
    }
}
