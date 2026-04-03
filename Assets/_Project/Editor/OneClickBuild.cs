using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Master build-everything script. Runs all factories and scene builders
    /// in the correct dependency order, then opens the Boot scene ready for Play.
    ///
    /// Menu: Tartaria > BUILD EVERYTHING
    /// Also available as batch mode entry point.
    /// </summary>
    public static class OneClickBuild
    {
        [MenuItem("Tartaria/BUILD EVERYTHING", false, -100)]
        public static void BuildEverything()
        {
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                if (!EditorUtility.DisplayDialog("Tartaria: Build Everything",
                    "This will create/update:\n\n" +
                    "  1. Directory structure\n" +
                    "  2. ScriptableObjects (Buildings, Zones, Quests, Constants)\n" +
                    "  3. Character prefabs (Player, Milo, MudGolem)\n" +
                    "  4. Scenes (Boot, UI_Overlay)\n" +
                    "  5. Echohaven scene population\n" +
                    "  6. All singleton managers scaffolded\n" +
                    "  7. Input actions assigned\n" +
                    "  8. Build settings configured\n\n" +
                    "Existing assets are preserved (idempotent).",
                    "Build It!", "Cancel"))
                    return;
            }

            var timer = System.Diagnostics.Stopwatch.StartNew();

            RunBuild();

            timer.Stop();
            float seconds = timer.ElapsedMilliseconds / 1000f;
            Debug.Log($"[Tartaria] BUILD EVERYTHING complete in {seconds:F1}s");

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                EditorUtility.DisplayDialog("Build Complete",
                    $"All assets built in {seconds:F1}s.\n\n" +
                    "Boot scene is ready. Press Play to test!",
                    "OK");
            }
        }

        /// <summary>
        /// Batch-mode safe entry point:
        /// Unity.exe -batchmode -projectPath ... -executeMethod Tartaria.Editor.OneClickBuild.RunBuild -quit
        /// </summary>
        public static void RunBuild()
        {
            // ── Phase 1: Directories ──
            Debug.Log("[Tartaria] Phase 1/8: Ensuring directories...");
            EnsureDirectories();

            // ── Phase 2: ScriptableObjects ──
            Debug.Log("[Tartaria] Phase 2/8: Building ScriptableObjects...");
            ProjectSetupWizard.RunSetup(); // Creates Echohaven scene + building defs + constants

            // ── Phase 3: Zone Definitions ──
            Debug.Log("[Tartaria] Phase 3/8: Building Zone Definitions...");
            ZoneDefinitionFactory.BuildZoneDefinitions();

            // ── Phase 4: Quest Definitions ──
            Debug.Log("[Tartaria] Phase 4/8: Building Quest Definitions...");
            QuestDefinitionFactory.BuildAllQuests();

            // ── Phase 5: Character Prefabs ──
            Debug.Log("[Tartaria] Phase 5/8: Building Character Prefabs...");
            CharacterPrefabFactory.BuildAllCharacters();

            // ── Phase 6: Scenes (Boot + UI_Overlay) ──
            Debug.Log("[Tartaria] Phase 6/8: Creating missing scenes...");
            SceneFactory.CreateAllMissingScenes();

            // ── Phase 7: Open Echohaven and scaffold all managers ──
            Debug.Log("[Tartaria] Phase 7/8: Scaffolding managers + populating Echohaven...");
            string echohavenPath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                MasterSceneScaffold.ScaffoldAll();
                EchohavenScenePopulator.Populate();
                EditorSceneManager.SaveOpenScenes();
            }

            // ── Phase 8: Input + Build Settings ──
            Debug.Log("[Tartaria] Phase 8/8: Assigning input actions + updating build settings...");
            InputActionsAssigner.AssignInputActions();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Open Boot scene (the entry point)
            string bootPath = "Assets/_Project/Scenes/Boot.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath) != null)
                EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
        }

        static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/_Project/Scenes",
                "Assets/_Project/Config",
                "Assets/_Project/Config/Zones",
                "Assets/_Project/Config/Quests",
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Characters",
                "Assets/_Project/Prefabs/Buildings",
                "Assets/_Project/Materials",
                "Assets/_Project/Input",
                "Assets/_Project/Audio/Music",
                "Assets/_Project/Audio/SFX",
            };

            foreach (var dir in dirs)
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, "..", dir);
                if (!System.IO.Directory.Exists(fullPath))
                    System.IO.Directory.CreateDirectory(fullPath);
            }

            AssetDatabase.Refresh();
        }

        static void ConfigureBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            TryAddScene(scenes, "Assets/_Project/Scenes/Boot.unity");
            TryAddScene(scenes, "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity");
            TryAddScene(scenes, "Assets/_Project/Scenes/UI_Overlay.unity");

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[Tartaria] Build settings: Boot(0) -> Echohaven(1) -> UI_Overlay(2)");
        }

        static void TryAddScene(System.Collections.Generic.List<EditorBuildSettingsScene> scenes, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                scenes.Add(new EditorBuildSettingsScene(path, true));
            else
                Debug.LogWarning($"[Tartaria] Scene not found: {path}");
        }
    }
}
