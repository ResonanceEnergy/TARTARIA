#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tartaria.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tartaria.Editor
{
    /// <summary>
    /// Sprint 6 Lane 1: Builds Assets/_Project/Scenes/MainMenu.unity from scratch and wires
    /// a Canvas + 5 buttons to <see cref="MainMenuController"/>.
    ///
    /// Menu: Tartaria/UI/Build Main Menu Scene
    ///
    /// Idempotent — re-running it overwrites the existing scene.
    /// Output also adds MainMenu.unity to EditorBuildSettings.scenes (front of list) so that
    /// SceneManager.LoadScene("MainMenu") works at runtime.
    /// </summary>
    public static class BuildMainMenuScene
    {
        const string SCENE_PATH = "Assets/_Project/Scenes/MainMenu.unity";
        const string MENU_ITEM = "Tartaria/UI/Build Main Menu Scene";

        [MenuItem(MENU_ITEM, priority = 12)]
        public static void Build()
        {
            try
            {
                EnsureSceneFolder();

                // Save any current scene first so we don't lose work.
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Debug.LogWarning("[BuildMainMenuScene] User cancelled save of the current scene — aborting build.");
                    return;
                }

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "MainMenu";

                // ─── EventSystem ─────────────────────────────────────────────
                var eventSystemGO = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                SceneManager.MoveGameObjectToScene(eventSystemGO, scene);

                // ─── Camera (UI-only) ────────────────────────────────────────
                var camGO = new GameObject("UI Camera", typeof(UnityEngine.Camera));
                SceneManager.MoveGameObjectToScene(camGO, scene);
                var cam = camGO.GetComponent<UnityEngine.Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.03f, 0.06f, 1f); // deep aether-violet
                cam.orthographic = true;
                cam.tag = "MainCamera";

                // ─── Canvas root ─────────────────────────────────────────────
                var canvasGO = new GameObject("MainMenuCanvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster),
                    typeof(MainMenuController));
                SceneManager.MoveGameObjectToScene(canvasGO, scene);

                var canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                var scaler = canvasGO.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                // ─── Background panel ────────────────────────────────────────
                var bg = CreateUIChild(canvasGO, "Background", typeof(Image));
                StretchToParent(bg);
                bg.GetComponent<Image>().color = new Color(0.04f, 0.03f, 0.06f, 1f);

                // ─── Title ───────────────────────────────────────────────────
                var titleGO = CreateUIChild(canvasGO, "Title", typeof(TextMeshProUGUI));
                var titleRT = (RectTransform)titleGO.transform;
                titleRT.anchorMin = new Vector2(0.5f, 1f);
                titleRT.anchorMax = new Vector2(0.5f, 1f);
                titleRT.pivot = new Vector2(0.5f, 1f);
                titleRT.anchoredPosition = new Vector2(0, -80f);
                titleRT.sizeDelta = new Vector2(1400, 140);
                var titleText = titleGO.GetComponent<TextMeshProUGUI>();
                titleText.text = "TARTARIA WORLD OF WONDER";
                titleText.fontSize = 84;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = new Color(0.95f, 0.88f, 0.62f, 1f); // aether gold

                // ─── Subtitle ────────────────────────────────────────────────
                var subGO = CreateUIChild(canvasGO, "Subtitle", typeof(TextMeshProUGUI));
                var subRT = (RectTransform)subGO.transform;
                subRT.anchorMin = new Vector2(0.5f, 1f);
                subRT.anchorMax = new Vector2(0.5f, 1f);
                subRT.pivot = new Vector2(0.5f, 1f);
                subRT.anchoredPosition = new Vector2(0, -220f);
                subRT.sizeDelta = new Vector2(1200, 60);
                var subText = subGO.GetComponent<TextMeshProUGUI>();
                subText.text = "Aether Awakening";
                subText.fontSize = 38;
                subText.alignment = TextAlignmentOptions.Center;
                subText.fontStyle = FontStyles.Italic;
                subText.color = new Color(0.75f, 0.85f, 1f, 1f);

                // ─── Button column ───────────────────────────────────────────
                var col = CreateUIChild(canvasGO, "ButtonColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
                var colRT = (RectTransform)col.transform;
                colRT.anchorMin = new Vector2(0.5f, 0.5f);
                colRT.anchorMax = new Vector2(0.5f, 0.5f);
                colRT.pivot = new Vector2(0.5f, 0.5f);
                colRT.anchoredPosition = new Vector2(0, -40f);
                colRT.sizeDelta = new Vector2(420, 480);
                var vlg = col.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 18;
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;

                var newGameBtn = CreateMenuButton(col, "Btn_NewGame", "New Game");
                var continueBtn = CreateMenuButton(col, "Btn_Continue", "Continue");
                var settingsBtn = CreateMenuButton(col, "Btn_Settings", "Settings");
                var creditsBtn = CreateMenuButton(col, "Btn_Credits", "Credits");
                var quitBtn = CreateMenuButton(col, "Btn_Quit", "Quit");

                // ─── Version label (bottom-right) ────────────────────────────
                var verGO = CreateUIChild(canvasGO, "VersionLabel", typeof(TextMeshProUGUI));
                var verRT = (RectTransform)verGO.transform;
                verRT.anchorMin = new Vector2(1f, 0f);
                verRT.anchorMax = new Vector2(1f, 0f);
                verRT.pivot = new Vector2(1f, 0f);
                verRT.anchoredPosition = new Vector2(-24f, 18f);
                verRT.sizeDelta = new Vector2(300, 32);
                var verText = verGO.GetComponent<TextMeshProUGUI>();
                verText.text = $"v{Application.version}";
                verText.fontSize = 22;
                verText.alignment = TextAlignmentOptions.BottomRight;
                verText.color = new Color(1f, 1f, 1f, 0.55f);

                // ─── Wire controller fields ──────────────────────────────────
                var controller = canvasGO.GetComponent<MainMenuController>();
                var so = new SerializedObject(controller);

                AssignField(so, "_newGameButton", newGameBtn);
                AssignField(so, "_continueButton", continueBtn);
                AssignField(so, "_settingsButton", settingsBtn);
                AssignField(so, "_creditsButton", creditsBtn);
                AssignField(so, "_quitButton", quitBtn);

                AssignField(so, "_titleLabel", titleText);
                AssignField(so, "_subtitleLabel", subText);
                AssignField(so, "_versionLabel", verText);

                so.ApplyModifiedPropertiesWithoutUndo();

                // ─── Save scene ──────────────────────────────────────────────
                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = EditorSceneManager.SaveScene(scene, SCENE_PATH);
                if (!saved)
                {
                    Debug.LogError($"[BuildMainMenuScene] Failed to save scene at '{SCENE_PATH}'. Check folder permissions.");
                    return;
                }

                AddSceneToBuildSettings(SCENE_PATH);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[BuildMainMenuScene] OK — wrote '{SCENE_PATH}' with 5 wired buttons + title/subtitle/version labels.");
                EditorUtility.DisplayDialog(
                    "Main Menu Built",
                    $"Created {SCENE_PATH}\n\n" +
                    "Scene contains a Canvas with 5 buttons (New Game / Continue / Settings / Credits / Quit) " +
                    "wired to MainMenuController, plus title/subtitle/version labels.\n\n" +
                    "Scene was added to Build Settings (first entry).",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[BuildMainMenuScene] Build failed (BuildMainMenuScene.cs:Build) — ex={ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Main Menu Build FAILED",
                    $"{ex.GetType().Name}: {ex.Message}\n\nSee Console for full stack.", "OK");
            }
        }

        // ─── Builders ────────────────────────────────────────────────────────

        static GameObject CreateUIChild(GameObject parent, string name, params Type[] components)
        {
            var hasRect = components.Any(t => t == typeof(RectTransform));
            var all = hasRect ? components : new[] { typeof(RectTransform) }.Concat(components).ToArray();
            var go = new GameObject(name, all);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        static void StretchToParent(GameObject go)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Button CreateMenuButton(GameObject parent, string name, string labelText)
        {
            var btnGO = CreateUIChild(parent, name, typeof(Image), typeof(Button), typeof(LayoutElement));
            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.12f, 0.10f, 0.18f, 0.92f);

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(0.95f, 0.88f, 0.62f, 1f);
            colors.pressedColor = new Color(0.75f, 0.65f, 0.35f, 1f);
            colors.selectedColor = new Color(0.95f, 0.88f, 0.62f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            btn.colors = colors;
            btn.targetGraphic = img;

            var layout = btnGO.GetComponent<LayoutElement>();
            layout.preferredHeight = 70;
            layout.preferredWidth = 400;
            layout.minHeight = 60;

            var labelGO = CreateUIChild(btnGO, "Label", typeof(TextMeshProUGUI));
            StretchToParent(labelGO);
            var text = labelGO.GetComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(0.96f, 0.93f, 0.85f, 1f);
            text.raycastTarget = false;

            return btn;
        }

        static void AssignField(SerializedObject so, string fieldName, UnityEngine.Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError(
                    $"[BuildMainMenuScene] Field '{fieldName}' not found on MainMenuController (BuildMainMenuScene.cs:AssignField). " +
                    "Did the controller field name change? Update BuildMainMenuScene to match.");
                return;
            }
            prop.objectReferenceValue = value;
        }

        static void EnsureSceneFolder()
        {
            string abs = Path.Combine(Application.dataPath, "_Project", "Scenes");
            if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var existing = EditorBuildSettings.scenes.ToList();
            // De-dupe: if it's already there, leave the existing entry alone.
            if (existing.Any(s => s.path == scenePath))
            {
                Debug.Log($"[BuildMainMenuScene] '{scenePath}' already in EditorBuildSettings.scenes — no change.");
                return;
            }
            existing.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = existing.ToArray();
            Debug.Log($"[BuildMainMenuScene] Added '{scenePath}' to EditorBuildSettings.scenes (index 0).");
        }
    }
}
#endif
