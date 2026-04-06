using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tartaria.Editor
{
    /// <summary>
    /// Populates the UI_Overlay scene with actual HUD content:
    /// TextMeshPro text fields, Image fills, panels for every HUD element.
    /// Then wires all serialized references on HUDController and UIManager.
    ///
    /// Idempotent — checks for existing children before creating.
    /// Called from OneClickBuild pipeline after scene creation.
    /// </summary>
    public static class UIOverlayPopulator
    {
        // Theme colors
        static readonly Color GoldBright = new Color(0.9f, 0.85f, 0.3f);
        static readonly Color GoldDim = new Color(0.6f, 0.5f, 0.1f);
        static readonly Color CyanBlue = new Color(0.2f, 0.6f, 0.9f);
        static readonly Color PanelBG = new Color(0.05f, 0.05f, 0.1f, 0.75f);
        static readonly Color DialogueBG = new Color(0.03f, 0.03f, 0.08f, 0.85f);
        static readonly Color BossRed = new Color(0.8f, 0.15f, 0.1f);
        static readonly Color TutorialAmber = new Color(0.95f, 0.8f, 0.2f);

        [MenuItem("Tartaria/Populate UI Overlay", false, 8)]
        public static void Populate()
        {
            int added = 0;

            // Find or validate scene objects
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIOverlayPopulator] No Canvas found in scene.");
                return;
            }

            var hud = FindChild(canvas.transform, "HUD");
            if (hud == null)
            {
                hud = CreatePanel(canvas.transform, "HUD", stretch: true);
                added++;
            }

            // ─── RS Gauge (top-left) ───
            var rsGauge = FindChild(hud.transform, "RSGauge");
            if (rsGauge == null)
            {
                rsGauge = CreatePanel(hud.transform, "RSGauge", stretch: false);
                SetAnchors(rsGauge, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(20f, -20f), new Vector2(200f, 40f));
                added++;
            }
            added += EnsureChild<Image>(rsGauge.transform, "RSBackground", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<Image>(rsGauge.transform, "RSFill", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = GoldBright;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillAmount = 0.5f;
                StretchFill(go);
                // Inset slightly so background shows as border
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
            });
            added += EnsureChild<TextMeshProUGUI>(rsGauge.transform, "RSValue", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "RS: 0";
                tmp.fontSize = 18f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Aether Bar (top-left, below RS) ───
            added += EnsurePanel(hud.transform, "AetherBar",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -70f), new Vector2(160f, 24f));
            var aetherBar = FindChild(hud.transform, "AetherBar");

            added += EnsureChild<Image>(aetherBar.transform, "AetherBG", go =>
            {
                go.GetComponent<Image>().color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<Image>(aetherBar.transform, "AetherFill", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = CyanBlue;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillAmount = 1f;
                StretchFill(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
            });
            added += EnsureChild<TextMeshProUGUI>(aetherBar.transform, "AetherValue", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Aether: 100%";
                tmp.fontSize = 14f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Zone Name (top-center) ───
            added += EnsurePanel(hud.transform, "ZoneNamePanel",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -15f), new Vector2(300f, 30f));
            var zonePanel = FindChild(hud.transform, "ZoneNamePanel");
            added += EnsureChild<TextMeshProUGUI>(zonePanel.transform, "ZoneName", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Echohaven";
                tmp.fontSize = 22f;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Interaction Prompt (center-bottom) ───
            added += EnsurePanel(hud.transform, "InteractionPrompt",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 80f), new Vector2(300f, 40f));
            var interactPrompt = FindChild(hud.transform, "InteractionPrompt");
            interactPrompt.SetActive(false);
            added += EnsureChild<Image>(interactPrompt.transform, "PromptBG", go =>
            {
                go.GetComponent<Image>().color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(interactPrompt.transform, "PromptText", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Press [E] to interact";
                tmp.fontSize = 18f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Dialogue Panel (bottom, wide) ───
            var dialoguePanel = FindChild(hud.transform, "DialoguePanel");
            if (dialoguePanel == null)
            {
                dialoguePanel = CreatePanel(hud.transform, "DialoguePanel", stretch: false);
                SetAnchors(dialoguePanel, new Vector2(0.1f, 0f), new Vector2(0.9f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(0f, 120f));
                added++;
            }
            dialoguePanel.SetActive(false);
            added += EnsureChild<Image>(dialoguePanel.transform, "DialogueBG", go =>
            {
                go.GetComponent<Image>().color = DialogueBG;
                StretchFill(go);
            });
            added += EnsureChild<Image>(dialoguePanel.transform, "DialoguePortrait", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = new Color(0.4f, 0.4f, 0.5f, 1f); // Placeholder grey
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(10f, 0f);
                rt.sizeDelta = new Vector2(90f, -20f);
            });
            added += EnsureChild<TextMeshProUGUI>(dialoguePanel.transform, "SpeakerName", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Milo";
                tmp.fontSize = 16f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(55f, -8f);
                rt.sizeDelta = new Vector2(-130f, 24f);
            });
            added += EnsureChild<TextMeshProUGUI>(dialoguePanel.transform, "DialogueBody", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "";
                tmp.fontSize = 14f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                tmp.enableWordWrapping = true;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(115f, 10f);
                rt.offsetMax = new Vector2(-15f, -35f);
            });

            // ─── Tutorial Prompt (upper-center) ───
            var tutorialPrompt = FindChild(hud.transform, "TutorialPrompt");
            if (tutorialPrompt == null)
            {
                tutorialPrompt = CreatePanel(hud.transform, "TutorialPrompt", stretch: false);
                SetAnchors(tutorialPrompt, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 60f));
                added++;
            }
            tutorialPrompt.SetActive(false);
            added += EnsureChild<Image>(tutorialPrompt.transform, "TutorialBG", go =>
            {
                go.GetComponent<Image>().color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(tutorialPrompt.transform, "TutorialStepText", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Step 1: Move with WASD";
                tmp.fontSize = 18f;
                tmp.color = TutorialAmber;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(10f, 20f);
                rt.offsetMax = new Vector2(-10f, -5f);
            });
            added += EnsureChild<TextMeshProUGUI>(tutorialPrompt.transform, "TutorialHintText", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Use W A S D to walk around";
                tmp.fontSize = 13f;
                tmp.color = new Color(0.7f, 0.7f, 0.7f);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 5f);
                rt.sizeDelta = new Vector2(-20f, 20f);
            });

            // ─── Boss Health (top-center, hidden) ───
            added += EnsurePanel(hud.transform, "BossHealthPanel",
                new Vector2(0.3f, 1f), new Vector2(0.7f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -50f), new Vector2(0f, 28f));
            var bossPanel = FindChild(hud.transform, "BossHealthPanel");
            bossPanel.SetActive(false);
            added += EnsureChild<Image>(bossPanel.transform, "BossHealthBG", go =>
            {
                go.GetComponent<Image>().color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<Image>(bossPanel.transform, "BossHealthFill", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = BossRed;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillAmount = 1f;
                StretchFill(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
            });
            added += EnsureChild<TextMeshProUGUI>(bossPanel.transform, "BossName", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Boss";
                tmp.fontSize = 14f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Wave Counter (top-left area, below aether) ───
            added += EnsurePanel(hud.transform, "WaveCounterPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -105f), new Vector2(180f, 40f));
            var wavePanel = FindChild(hud.transform, "WaveCounterPanel");
            wavePanel.SetActive(false);
            added += EnsureChild<Image>(wavePanel.transform, "WaveBG", go =>
            {
                go.GetComponent<Image>().color = PanelBG;
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(wavePanel.transform, "WaveText", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Wave 1/3";
                tmp.fontSize = 14f;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Top;
                tmp.raycastTarget = false;
                StretchFill(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMax = new Vector2(0f, -2f);
            });
            added += EnsureChild<TextMeshProUGUI>(wavePanel.transform, "WaveEnemies", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "4 remaining";
                tmp.fontSize = 12f;
                tmp.color = new Color(0.7f, 0.7f, 0.7f);
                tmp.alignment = TextAlignmentOptions.Bottom;
                tmp.raycastTarget = false;
                StretchFill(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(0f, 2f);
            });

            // ─── Achievement Toast (top-right, hidden) ───
            added += EnsurePanel(hud.transform, "AchievementToast",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -20f), new Vector2(280f, 50f));
            var achievePanel = FindChild(hud.transform, "AchievementToast");
            achievePanel.SetActive(false);
            added += EnsureChild<Image>(achievePanel.transform, "AchieveBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.02f, 0.9f);
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(achievePanel.transform, "AchieveText", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Achievement Unlocked!";
                tmp.fontSize = 16f;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                StretchFill(go);
            });

            // ─── Moon Trophy Banner (center, hidden) ───
            added += EnsurePanel(hud.transform, "MoonTrophyPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(500f, 120f));
            var trophyPanel = FindChild(hud.transform, "MoonTrophyPanel");
            trophyPanel.SetActive(false);
            added += EnsureChild<Image>(trophyPanel.transform, "TrophyBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.02f, 0.92f);
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(trophyPanel.transform, "TrophyTitle", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "ECHOHAVEN AWAKENED";
                tmp.fontSize = 28f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0.4f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(10f, 0f);
                rt.offsetMax = new Vector2(-10f, -10f);
            });
            added += EnsureChild<TextMeshProUGUI>(trophyPanel.transform, "TrophySubtext", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "The resonance flows again through ancient stone.";
                tmp.fontSize = 14f;
                tmp.color = new Color(0.8f, 0.75f, 0.5f);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0.4f);
                rt.offsetMin = new Vector2(10f, 10f);
                rt.offsetMax = new Vector2(-10f, 0f);
            });

            // ─── Loading Panel (fullscreen, hidden) ───
            added += EnsurePanel(canvas.transform, "LoadingPanel",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, stretch: true);
            var loadingPanel = FindChild(canvas.transform, "LoadingPanel");
            loadingPanel.SetActive(false);
            added += EnsureChild<Image>(loadingPanel.transform, "LoadingBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.95f);
                StretchFill(go);
            });
            added += EnsureChild<Image>(loadingPanel.transform, "LoadingBarBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.2f, 0.45f);
                rt.anchorMax = new Vector2(0.8f, 0.45f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(0f, 12f);
            });
            added += EnsureChild<Image>(loadingPanel.transform, "LoadingBarFill", go =>
            {
                var img = go.GetComponent<Image>();
                img.color = GoldBright;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillAmount = 0f;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.2f, 0.45f);
                rt.anchorMax = new Vector2(0.8f, 0.45f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(-4f, 8f);
            });
            added += EnsureChild<TextMeshProUGUI>(loadingPanel.transform, "LoadingTip", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "The resonance remembers...";
                tmp.fontSize = 16f;
                tmp.fontStyle = FontStyles.Italic;
                tmp.color = new Color(0.6f, 0.55f, 0.4f);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.2f, 0.35f);
                rt.anchorMax = new Vector2(0.8f, 0.35f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(0f, 30f);
            });

            // ─── Pause Menu Content ───
            var pauseMenu = FindChild(canvas.transform, "PauseMenu");
            if (pauseMenu == null)
            {
                pauseMenu = CreatePanel(canvas.transform, "PauseMenu", stretch: true);
                pauseMenu.SetActive(false);
                added++;
            }
            added += EnsureChild<Image>(pauseMenu.transform, "PauseBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(pauseMenu.transform, "PauseTitle", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "PAUSED";
                tmp.fontSize = 36f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.3f, 0.7f);
                rt.anchorMax = new Vector2(0.7f, 0.85f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            });
            // Pause buttons container
            added += EnsurePanel(pauseMenu.transform, "PauseButtons",
                new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.65f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, stretch: true);
            var btnContainer = FindChild(pauseMenu.transform, "PauseButtons");
            var vlg = btnContainer.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 12f;
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
            }
            string[] btnNames = { "ResumeBtn", "SettingsBtn", "SaveBtn", "QuitBtn" };
            string[] btnLabels = { "Resume", "Settings", "Save Game", "Quit" };
            for (int i = 0; i < btnNames.Length; i++)
            {
                int idx = i; // capture
                added += EnsureChild<Button>(btnContainer.transform, btnNames[idx], go =>
                {
                    var img = go.GetComponent<Image>();
                    if (img == null) img = go.AddComponent<Image>();
                    img.color = new Color(0.15f, 0.13f, 0.08f, 0.9f);
                    var le = go.AddComponent<LayoutElement>();
                    le.preferredHeight = 45f;

                    var labelGO = new GameObject("Label");
                    labelGO.transform.SetParent(go.transform, false);
                    var tmp = labelGO.AddComponent<TextMeshProUGUI>();
                    tmp.text = btnLabels[idx];
                    tmp.fontSize = 20f;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.raycastTarget = false;
                    StretchFill(labelGO);
                });
            }

            // ─── Settings Panel (hidden, placeholder) ───
            added += EnsurePanel(canvas.transform, "SettingsPanel",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, stretch: true);
            var settingsPanel = FindChild(canvas.transform, "SettingsPanel");
            settingsPanel.SetActive(false);
            added += EnsureChild<Image>(settingsPanel.transform, "SettingsBG", go =>
            {
                go.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
                StretchFill(go);
            });
            added += EnsureChild<TextMeshProUGUI>(settingsPanel.transform, "SettingsTitle", go =>
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = "Settings";
                tmp.fontSize = 28f;
                tmp.color = GoldBright;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.3f, 0.8f);
                rt.anchorMax = new Vector2(0.7f, 0.9f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            });

            // ─── Save Indicator (small top-right icon) ───
            added += EnsurePanel(canvas.transform, "SaveIndicator",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-50f, -50f), new Vector2(32f, 32f));
            var saveIndicator = FindChild(canvas.transform, "SaveIndicator");
            saveIndicator.SetActive(false);
            added += EnsureChild<Image>(saveIndicator.transform, "SaveIcon", go =>
            {
                go.GetComponent<Image>().color = GoldBright;
                StretchFill(go);
            });

            // ─── Aether Vision Overlay (fullscreen tint, hidden) ───
            added += EnsurePanel(canvas.transform, "AetherVisionOverlay",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, stretch: true);
            var aetherOverlay = FindChild(canvas.transform, "AetherVisionOverlay");
            aetherOverlay.SetActive(false);
            added += EnsureChild<Image>(aetherOverlay.transform, "AetherTint", go =>
            {
                go.GetComponent<Image>().color = new Color(0.1f, 0.3f, 0.5f, 0.25f);
                StretchFill(go);
            });

            // ─── Wire References ───
            WireHUDController(hud, rsGauge, aetherBar, interactPrompt, bossPanel,
                wavePanel, achievePanel, trophyPanel);
            WireUIManager(canvas.gameObject, hud, dialoguePanel, loadingPanel,
                pauseMenu, settingsPanel, aetherOverlay, saveIndicator);

            Debug.Log($"[UIOverlayPopulator] Populated — {added} UI elements added.");
        }

        // ═══════════════════════════════════════════════════════════════
        // Reference Wiring
        // ═══════════════════════════════════════════════════════════════

        static void WireHUDController(GameObject hud, GameObject rsGauge,
            GameObject aetherBar, GameObject interactPrompt, GameObject bossPanel,
            GameObject wavePanel, GameObject achievePanel, GameObject trophyPanel)
        {
            var ctrl = Object.FindAnyObjectByType<UI.HUDController>();
            if (ctrl == null) { Debug.LogWarning("[UIOverlayPopulator] HUDController not found."); return; }

            var so = new SerializedObject(ctrl);
            SetRef(so, "rsGauge", rsGauge?.GetComponent<RectTransform>());
            SetRef(so, "rsFillImage", FindChild(rsGauge?.transform, "RSFill")?.GetComponent<Image>());
            SetRef(so, "rsValueText", FindChild(rsGauge?.transform, "RSValue")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "aetherChargeBar", FindChild(aetherBar?.transform, "AetherFill")?.GetComponent<Image>());
            SetRef(so, "aetherValueText", FindChild(aetherBar?.transform, "AetherValue")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "interactionPrompt", interactPrompt?.GetComponent<RectTransform>());
            SetRef(so, "interactionText", FindChild(interactPrompt?.transform, "PromptText")?.GetComponent<TextMeshProUGUI>());
            // ZoneName is nested: HUD > ZoneNamePanel > ZoneName
            var znPanel = FindChild(hud?.transform, "ZoneNamePanel");
            SetRef(so, "zoneNameText", FindChild(znPanel?.transform, "ZoneName")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "bossHealthPanel", bossPanel?.GetComponent<RectTransform>());
            SetRef(so, "bossHealthFill", FindChild(bossPanel?.transform, "BossHealthFill")?.GetComponent<Image>());
            SetRef(so, "bossNameText", FindChild(bossPanel?.transform, "BossName")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "waveCounterPanel", wavePanel?.GetComponent<RectTransform>());
            SetRef(so, "waveCounterText", FindChild(wavePanel?.transform, "WaveText")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "waveEnemiesText", FindChild(wavePanel?.transform, "WaveEnemies")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "achievementToastPanel", achievePanel?.GetComponent<RectTransform>());
            SetRef(so, "achievementToastText", FindChild(achievePanel?.transform, "AchieveText")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "moonTrophyPanel", trophyPanel?.GetComponent<RectTransform>());
            SetRef(so, "moonTrophyText", FindChild(trophyPanel?.transform, "TrophyTitle")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "moonTrophySubtext", FindChild(trophyPanel?.transform, "TrophySubtext")?.GetComponent<TextMeshProUGUI>());
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            Debug.Log("[UIOverlayPopulator] HUDController references wired.");
        }

        static void WireUIManager(GameObject canvas, GameObject hud, GameObject dialoguePanel,
            GameObject loadingPanel, GameObject pauseMenu, GameObject settingsPanel,
            GameObject aetherOverlay, GameObject saveIndicator)
        {
            var mgr = Object.FindAnyObjectByType<UI.UIManager>();
            if (mgr == null) { Debug.LogWarning("[UIOverlayPopulator] UIManager not found."); return; }

            var so = new SerializedObject(mgr);
            SetRef(so, "hudPanel", hud);
            SetRef(so, "pauseMenuPanel", pauseMenu);
            SetRef(so, "settingsPanel", settingsPanel);
            SetRef(so, "dialoguePanel", dialoguePanel);
            SetRef(so, "loadingPanel", loadingPanel);
            SetRef(so, "aetherVisionOverlay", aetherOverlay);
            SetRef(so, "dialogueSpeakerText", FindChild(dialoguePanel?.transform, "SpeakerName")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "dialogueBodyText", FindChild(dialoguePanel?.transform, "DialogueBody")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "dialoguePortrait", FindChild(dialoguePanel?.transform, "DialoguePortrait")?.GetComponent<Image>());
            SetRef(so, "loadingBar", FindChild(loadingPanel?.transform, "LoadingBarFill")?.GetComponent<Image>());
            SetRef(so, "loadingTipText", FindChild(loadingPanel?.transform, "LoadingTip")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "saveIndicator", saveIndicator);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(mgr);
            Debug.Log("[UIOverlayPopulator] UIManager references wired.");
        }

        // ═══════════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════════

        static bool SetRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) return false;
            if (p.objectReferenceValue != null) return true; // Already wired
            p.objectReferenceValue = value;
            return value != null;
        }

        static GameObject FindChild(Transform parent, string name)
        {
            if (parent == null) return null;
            var t = parent.Find(name);
            if (t != null) return t.gameObject;
            // Flat search among direct children
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name)
                    return parent.GetChild(i).gameObject;
            }
            return null;
        }

        static GameObject CreatePanel(Transform parent, string name, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            if (stretch) StretchFill(go);
            return go;
        }

        static int EnsurePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, bool stretch = false)
        {
            if (FindChild(parent, name) != null) return 0;
            var go = CreatePanel(parent, name, stretch);
            if (!stretch)
                SetAnchors(go, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            return 1;
        }

        static int EnsureChild<T>(Transform parent, string name, System.Action<GameObject> setup) where T : Component
        {
            if (FindChild(parent, name) != null) return 0;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            if (go.GetComponent<T>() == null) go.AddComponent<T>();
            setup?.Invoke(go);
            return 1;
        }

        static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        static void StretchFill(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
