// BuildSettingsPanelPrefab.cs
// Sprint 7 Lane 5 - agent/ui/pause-settings-extract
// Owner: UI agent. Path: Assets/_Project/Scripts/Editor/BuildSettingsPanelPrefab.cs
//
// Editor-only authoring tool. Generates a Canvas-backed settings prefab at
// Assets/_Project/Resources/UI/SettingsPanel.prefab. Both the Main Menu and
// the Pause Menu spawn this single prefab via SettingsPanelController.Open().
//
// Menu: Tartaria/UI/Build Settings Panel Prefab
//
// Layout:
//   - Canvas (ScreenSpaceOverlay) + CanvasGroup
//     - Background (full-screen dim)
//     - PanelFrame (560x540 centered)
//       - Title (TMP)
//       - DISPLAY section: resolution dropdown + fullscreen toggle
//       - AUDIO section:   master/music/sfx sliders + percent labels
//       - INPUT section:   invert-Y toggle + rumble toggle
//       - LANGUAGE section: language dropdown + warning label
//       - Apply button + Cancel button
//
// API_CONTRACT compliance:
//   - Editor-only assembly (Tartaria.Scripts.Editor).
//   - No silent catches. AssetDatabase ops log on each step.
//   - Resource path constant matches SettingsPanelController.ResourcePath.

using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Tartaria.UI;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor tool that authors Resources/UI/SettingsPanel.prefab. Idempotent:
    /// re-running deletes the existing prefab and re-creates it from scratch.
    /// </summary>
    public static class BuildSettingsPanelPrefab
    {
        const string PrefabFolder = "Assets/_Project/Resources/UI";
        const string PrefabPath   = "Assets/_Project/Resources/UI/SettingsPanel.prefab";

        // Brand-ish colors (matches SettingsMenu IMGUI palette: title gold,
        // section header amber, dim near-black-purple background).
        static readonly Color ColorDim       = new Color(0.02f, 0.01f, 0.04f, 0.85f);
        static readonly Color ColorPanel     = new Color(0.08f, 0.06f, 0.12f, 0.96f);
        static readonly Color ColorTitle     = new Color(0.98f, 0.90f, 0.55f, 1f);
        static readonly Color ColorHeader    = new Color(0.95f, 0.85f, 0.50f, 1f);
        static readonly Color ColorText      = new Color(0.92f, 0.92f, 0.94f, 1f);
        static readonly Color ColorWarn      = new Color(1.00f, 0.78f, 0.40f, 1f);
        static readonly Color ColorApply     = new Color(0.30f, 0.55f, 0.30f, 1f);
        static readonly Color ColorCancel    = new Color(0.55f, 0.30f, 0.30f, 1f);
        static readonly Color ColorButtonTxt = new Color(0.98f, 0.98f, 0.98f, 1f);

        // Geometry constants (kept here so the Editor builder is a single
        // source of truth - if the panel needs resizing, change it here).
        const float PanelWidth  = 720f;
        const float PanelHeight = 760f;
        const float Margin      = 24f;
        const float SectionGap  = 14f;
        const float RowHeight   = 36f;
        const float HeaderHeight = 28f;

        [MenuItem("Tartaria/UI/Build Settings Panel Prefab")]
        public static void Build()
        {
            Debug.Log("[BuildSettingsPanelPrefab] Starting build.");

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
            {
                Debug.LogError("[BuildSettingsPanelPrefab] Missing folder Assets/_Project/Resources - aborting.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "UI");
                Debug.Log("[BuildSettingsPanelPrefab] Created folder " + PrefabFolder);
            }

            // Build the hierarchy in the scene (we'll save it to disk then destroy).
            var root = new GameObject("SettingsPanel");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9500;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<CanvasGroup>();

            var controller = root.AddComponent<SettingsPanelController>();

            // --- Background dim (full screen) ---
            var bg = CreateUIImage("Background", root.transform, ColorDim);
            StretchToParent((RectTransform)bg.transform);

            // --- Panel frame (centered) ---
            var panel = CreateUIImage("PanelFrame", root.transform, ColorPanel);
            var panelRT = (RectTransform)panel.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            panelRT.anchoredPosition = Vector2.zero;

            // --- Title ---
            var title = CreateTMP("Title", panel.transform, "SETTINGS",
                fontSize: 32, color: ColorTitle, alignment: TextAlignmentOptions.Center, bold: true);
            var titleRT = (RectTransform)title.transform;
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -Margin);
            titleRT.sizeDelta = new Vector2(-Margin * 2f, 44f);

            // Content cursor (anchored top-left inside the panel, marching downward).
            float y = -(Margin + 44f + SectionGap);

            // ===== DISPLAY =====
            CreateSectionHeader(panel.transform, "DISPLAY", ref y);
            var resolutionDropdown = CreateLabeledDropdown(panel.transform, "Resolution", ref y);
            var fullscreenToggle   = CreateLabeledToggle(panel.transform, "Fullscreen", ref y);
            y -= SectionGap;

            // ===== AUDIO =====
            CreateSectionHeader(panel.transform, "AUDIO", ref y);
            var masterSlider = CreateLabeledSliderWithValue(panel.transform, "Master Volume", ref y, out var masterValueLabel);
            var musicSlider  = CreateLabeledSliderWithValue(panel.transform, "Music Volume",  ref y, out var musicValueLabel);
            var sfxSlider    = CreateLabeledSliderWithValue(panel.transform, "SFX Volume",    ref y, out var sfxValueLabel);
            y -= SectionGap;

            // ===== INPUT =====
            CreateSectionHeader(panel.transform, "INPUT", ref y);
            var invertYToggle = CreateLabeledToggle(panel.transform, "Invert Camera Y", ref y);
            var rumbleToggle  = CreateLabeledToggle(panel.transform, "Controller Rumble", ref y);
            y -= SectionGap;

            // ===== LANGUAGE =====
            CreateSectionHeader(panel.transform, "LANGUAGE", ref y);
            var languageDropdown = CreateLabeledDropdown(panel.transform, "Language", ref y);
            var languageWarning = CreateTMP("LanguageWarning", panel.transform,
                "This locale is not localized yet - text will remain English on Apply.",
                fontSize: 14, color: ColorWarn, alignment: TextAlignmentOptions.Left, bold: false);
            var warnRT = (RectTransform)languageWarning.transform;
            warnRT.anchorMin = new Vector2(0f, 1f);
            warnRT.anchorMax = new Vector2(1f, 1f);
            warnRT.pivot = new Vector2(0.5f, 1f);
            warnRT.anchoredPosition = new Vector2(0f, y);
            warnRT.sizeDelta = new Vector2(-Margin * 2f, 22f);
            languageWarning.gameObject.SetActive(false);
            y -= 28f;

            // ===== BUTTONS (Apply / Cancel, anchored bottom-right) =====
            var applyButton  = CreateButton(panel.transform, "ApplyButton",  "APPLY",  ColorApply,  ColorButtonTxt);
            var cancelButton = CreateButton(panel.transform, "CancelButton", "CANCEL", ColorCancel, ColorButtonTxt);
            var applyRT = (RectTransform)applyButton.transform;
            applyRT.anchorMin = applyRT.anchorMax = new Vector2(1f, 0f);
            applyRT.pivot = new Vector2(1f, 0f);
            applyRT.sizeDelta = new Vector2(140f, 44f);
            applyRT.anchoredPosition = new Vector2(-Margin, Margin);
            var cancelRT = (RectTransform)cancelButton.transform;
            cancelRT.anchorMin = cancelRT.anchorMax = new Vector2(1f, 0f);
            cancelRT.pivot = new Vector2(1f, 0f);
            cancelRT.sizeDelta = new Vector2(140f, 44f);
            cancelRT.anchoredPosition = new Vector2(-(Margin + 140f + 12f), Margin);

            // --- Wire serialized references onto the controller via SerializedObject ---
            var so = new SerializedObject(controller);
            so.FindProperty("_resolutionDropdown").objectReferenceValue       = resolutionDropdown;
            so.FindProperty("_fullscreenToggle").objectReferenceValue         = fullscreenToggle;
            so.FindProperty("_masterVolumeSlider").objectReferenceValue      = masterSlider;
            so.FindProperty("_musicVolumeSlider").objectReferenceValue       = musicSlider;
            so.FindProperty("_sfxVolumeSlider").objectReferenceValue         = sfxSlider;
            so.FindProperty("_masterVolumeValueLabel").objectReferenceValue  = masterValueLabel;
            so.FindProperty("_musicVolumeValueLabel").objectReferenceValue   = musicValueLabel;
            so.FindProperty("_sfxVolumeValueLabel").objectReferenceValue     = sfxValueLabel;
            so.FindProperty("_invertYToggle").objectReferenceValue           = invertYToggle;
            so.FindProperty("_rumbleToggle").objectReferenceValue            = rumbleToggle;
            so.FindProperty("_languageDropdown").objectReferenceValue        = languageDropdown;
            so.FindProperty("_languageWarningLabel").objectReferenceValue    = languageWarning;
            so.FindProperty("_applyButton").objectReferenceValue             = applyButton;
            so.FindProperty("_cancelButton").objectReferenceValue            = cancelButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            // --- Save as prefab, then destroy the temp scene object ---
            if (File.Exists(PrefabPath))
            {
                AssetDatabase.DeleteAsset(PrefabPath);
                Debug.Log("[BuildSettingsPanelPrefab] Deleted existing prefab.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool success);
            Object.DestroyImmediate(root);

            if (!success)
            {
                Debug.LogError("[BuildSettingsPanelPrefab] SaveAsPrefabAsset reported failure for " + PrefabPath);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BuildSettingsPanelPrefab] Prefab authored at " + PrefabPath +
                      " - resource path '" + SettingsPanelController.ResourcePath + "'.");
        }

        // ===== Section header =========================================
        static void CreateSectionHeader(Transform parent, string text, ref float y)
        {
            var hdr = CreateTMP("Section_" + text, parent, "> " + text,
                fontSize: 18, color: ColorHeader, alignment: TextAlignmentOptions.Left, bold: true);
            var rt = (RectTransform)hdr.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-Margin * 2f, HeaderHeight);
            y -= HeaderHeight + 4f;
        }

        // ===== Row builders ===========================================
        static TMP_Dropdown CreateLabeledDropdown(Transform parent, string label, ref float y)
        {
            // Row container
            var row = new GameObject(label + "_Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRT = (RectTransform)row.transform;
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, y);
            rowRT.sizeDelta = new Vector2(-Margin * 2f, RowHeight);

            // Label
            var labelGO = CreateTMP("Label", row.transform, label,
                fontSize: 16, color: ColorText, alignment: TextAlignmentOptions.MidlineLeft, bold: false);
            var lblRT = (RectTransform)labelGO.transform;
            lblRT.anchorMin = new Vector2(0f, 0f);
            lblRT.anchorMax = new Vector2(0.45f, 1f);
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

            // Dropdown
            var dd = CreateDropdown(row.transform, label + "_Dropdown");
            var ddRT = (RectTransform)dd.transform;
            ddRT.anchorMin = new Vector2(0.45f, 0f);
            ddRT.anchorMax = new Vector2(1f, 1f);
            ddRT.offsetMin = ddRT.offsetMax = Vector2.zero;

            y -= RowHeight + 6f;
            return dd;
        }

        static Toggle CreateLabeledToggle(Transform parent, string label, ref float y)
        {
            var row = new GameObject(label + "_Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRT = (RectTransform)row.transform;
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, y);
            rowRT.sizeDelta = new Vector2(-Margin * 2f, RowHeight);

            var toggleGO = new GameObject(label + "_Toggle", typeof(RectTransform));
            toggleGO.transform.SetParent(row.transform, false);
            var toggleRT = (RectTransform)toggleGO.transform;
            toggleRT.anchorMin = Vector2.zero;
            toggleRT.anchorMax = Vector2.one;
            toggleRT.offsetMin = toggleRT.offsetMax = Vector2.zero;

            var toggle = toggleGO.AddComponent<Toggle>();

            // Checkbox background
            var box = CreateUIImage("Box", toggleGO.transform, new Color(0.18f, 0.16f, 0.22f, 1f));
            var boxRT = (RectTransform)box.transform;
            boxRT.anchorMin = new Vector2(0f, 0.5f);
            boxRT.anchorMax = new Vector2(0f, 0.5f);
            boxRT.pivot = new Vector2(0f, 0.5f);
            boxRT.sizeDelta = new Vector2(26f, 26f);
            boxRT.anchoredPosition = new Vector2(0f, 0f);

            // Checkmark (child of Box)
            var check = CreateUIImage("Checkmark", box.transform, ColorTitle);
            var checkRT = (RectTransform)check.transform;
            checkRT.anchorMin = new Vector2(0.15f, 0.15f);
            checkRT.anchorMax = new Vector2(0.85f, 0.85f);
            checkRT.offsetMin = checkRT.offsetMax = Vector2.zero;
            toggle.graphic = check.GetComponent<Image>();
            toggle.targetGraphic = box.GetComponent<Image>();

            // Label to the right
            var lbl = CreateTMP("Label", toggleGO.transform, label,
                fontSize: 16, color: ColorText, alignment: TextAlignmentOptions.MidlineLeft, bold: false);
            var lblRT = (RectTransform)lbl.transform;
            lblRT.anchorMin = new Vector2(0f, 0f);
            lblRT.anchorMax = new Vector2(1f, 1f);
            lblRT.offsetMin = new Vector2(36f, 0f);
            lblRT.offsetMax = Vector2.zero;

            y -= RowHeight + 4f;
            return toggle;
        }

        static Slider CreateLabeledSliderWithValue(Transform parent, string label, ref float y, out TMP_Text valueLabel)
        {
            var row = new GameObject(label + "_Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRT = (RectTransform)row.transform;
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, y);
            rowRT.sizeDelta = new Vector2(-Margin * 2f, RowHeight);

            var labelGO = CreateTMP("Label", row.transform, label,
                fontSize: 16, color: ColorText, alignment: TextAlignmentOptions.MidlineLeft, bold: false);
            var lblRT = (RectTransform)labelGO.transform;
            lblRT.anchorMin = new Vector2(0f, 0f);
            lblRT.anchorMax = new Vector2(0.40f, 1f);
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

            var sliderGO = new GameObject(label + "_Slider", typeof(RectTransform));
            sliderGO.transform.SetParent(row.transform, false);
            var sliderRT = (RectTransform)sliderGO.transform;
            sliderRT.anchorMin = new Vector2(0.40f, 0.25f);
            sliderRT.anchorMax = new Vector2(0.88f, 0.75f);
            sliderRT.offsetMin = sliderRT.offsetMax = Vector2.zero;
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 0.8f;

            // Background
            var bg = CreateUIImage("Background", sliderGO.transform, new Color(0.18f, 0.16f, 0.22f, 1f));
            var bgRT = (RectTransform)bg.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            // Fill Area / Fill
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
            var faRT = (RectTransform)fillArea.transform;
            faRT.anchorMin = new Vector2(0f, 0.25f);
            faRT.anchorMax = new Vector2(1f, 0.75f);
            faRT.offsetMin = new Vector2(6f, 0f);
            faRT.offsetMax = new Vector2(-6f, 0f);
            var fill = CreateUIImage("Fill", fillArea.transform, ColorTitle);
            var fillRT = (RectTransform)fill.transform;
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            slider.fillRect = fillRT;

            // Handle Slide Area / Handle
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGO.transform, false);
            var haRT = (RectTransform)handleArea.transform;
            haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(6f, 0f);
            haRT.offsetMax = new Vector2(-6f, 0f);
            var handle = CreateUIImage("Handle", handleArea.transform, ColorText);
            var hRT = (RectTransform)handle.transform;
            hRT.sizeDelta = new Vector2(14f, 0f);
            hRT.anchorMin = new Vector2(0f, 0f);
            hRT.anchorMax = new Vector2(0f, 1f);
            slider.handleRect = hRT;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;

            // Percent label on the far right
            var valGO = CreateTMP("Value", row.transform, "80%",
                fontSize: 16, color: ColorText, alignment: TextAlignmentOptions.MidlineRight, bold: false);
            var valRT = (RectTransform)valGO.transform;
            valRT.anchorMin = new Vector2(0.88f, 0f);
            valRT.anchorMax = new Vector2(1f, 1f);
            valRT.offsetMin = valRT.offsetMax = Vector2.zero;
            valueLabel = valGO;

            y -= RowHeight + 4f;
            return slider;
        }

        // ===== Atomic UI primitives ===================================
        static GameObject CreateUIImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return go;
        }

        static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static TMP_Text CreateTMP(string name, Transform parent, string text,
                                  int fontSize, Color color, TextAlignmentOptions alignment, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TMPro.TextWrappingModes.Normal; // 2026-06-05 CS0618 fix: enableWordWrapping obsolete
            if (bold) tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        static Button CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var tmp = CreateTMP("Label", go.transform, label,
                fontSize: 18, color: textColor, alignment: TextAlignmentOptions.Center, bold: true);
            var rt = (RectTransform)tmp.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            return btn;
        }

        static TMP_Dropdown CreateDropdown(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.16f, 0.22f, 1f);
            var dd = go.AddComponent<TMP_Dropdown>();
            dd.targetGraphic = bg;

            // Label (current value)
            var label = CreateTMP("Label", go.transform, "Option",
                fontSize: 14, color: ColorText, alignment: TextAlignmentOptions.MidlineLeft, bold: false);
            var lblRT = (RectTransform)label.transform;
            lblRT.anchorMin = new Vector2(0f, 0f);
            lblRT.anchorMax = new Vector2(1f, 1f);
            lblRT.offsetMin = new Vector2(10f, 2f);
            lblRT.offsetMax = new Vector2(-22f, -2f);
            dd.captionText = label;

            // Arrow
            var arrow = CreateTMP("Arrow", go.transform, "v",
                fontSize: 14, color: ColorTitle, alignment: TextAlignmentOptions.MidlineRight, bold: true);
            var arrRT = (RectTransform)arrow.transform;
            arrRT.anchorMin = new Vector2(1f, 0f);
            arrRT.anchorMax = new Vector2(1f, 1f);
            arrRT.pivot = new Vector2(1f, 0.5f);
            arrRT.sizeDelta = new Vector2(20f, 0f);
            arrRT.anchoredPosition = new Vector2(-6f, 0f);

            // Template (TMP_Dropdown requires one)
            var template = new GameObject("Template", typeof(RectTransform));
            template.transform.SetParent(go.transform, false);
            var tmplRT = (RectTransform)template.transform;
            tmplRT.anchorMin = new Vector2(0f, 0f);
            tmplRT.anchorMax = new Vector2(1f, 0f);
            tmplRT.pivot = new Vector2(0.5f, 1f);
            tmplRT.anchoredPosition = new Vector2(0f, 2f);
            tmplRT.sizeDelta = new Vector2(0f, 150f);
            var tmplBg = template.AddComponent<Image>();
            tmplBg.color = new Color(0.12f, 0.10f, 0.16f, 0.98f);
            template.AddComponent<ScrollRect>();
            template.SetActive(false);

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(template.transform, false);
            var vpRT = (RectTransform)viewport.transform;
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
            var vpMask = viewport.AddComponent<Mask>();
            vpMask.showMaskGraphic = false;
            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var cRT = (RectTransform)content.transform;
            cRT.anchorMin = new Vector2(0f, 1f);
            cRT.anchorMax = new Vector2(1f, 1f);
            cRT.pivot = new Vector2(0.5f, 1f);
            cRT.sizeDelta = new Vector2(0f, 28f);

            // Item template (single row in the dropdown list)
            var item = new GameObject("Item", typeof(RectTransform));
            item.transform.SetParent(content.transform, false);
            var iRT = (RectTransform)item.transform;
            iRT.anchorMin = new Vector2(0f, 0.5f);
            iRT.anchorMax = new Vector2(1f, 0.5f);
            iRT.sizeDelta = new Vector2(0f, 28f);
            var itemToggle = item.AddComponent<Toggle>();
            var itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.16f, 0.14f, 0.20f, 1f);
            itemToggle.targetGraphic = itemBg;

            // Item background graphic for selection highlight
            var itemCheck = CreateUIImage("Item Background", item.transform, new Color(0.30f, 0.25f, 0.40f, 1f));
            var ibRT = (RectTransform)itemCheck.transform;
            ibRT.anchorMin = Vector2.zero; ibRT.anchorMax = Vector2.one;
            ibRT.offsetMin = ibRT.offsetMax = Vector2.zero;
            itemToggle.graphic = itemCheck.GetComponent<Image>();

            // Item label
            var itemLabel = CreateTMP("Item Label", item.transform, "Option A",
                fontSize: 14, color: ColorText, alignment: TextAlignmentOptions.MidlineLeft, bold: false);
            var ilRT = (RectTransform)itemLabel.transform;
            ilRT.anchorMin = Vector2.zero; ilRT.anchorMax = Vector2.one;
            ilRT.offsetMin = new Vector2(10f, 0f);
            ilRT.offsetMax = new Vector2(-10f, 0f);

            // Wire template references on the dropdown
            dd.template = tmplRT;
            dd.itemText = itemLabel;

            // Seed with a placeholder option (cleared at runtime by ClearOptions).
            dd.options.Clear();
            dd.options.Add(new TMP_Dropdown.OptionData("Default"));

            return dd;
        }
    }
}
