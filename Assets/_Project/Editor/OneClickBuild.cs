using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Master build-everything script. Runs all factories and scene builders
    /// in the correct dependency order, then opens the Boot scene ready for Play.
    ///
    /// Every phase is isolated: a failure in one phase logs the error and
    /// continues to the next. Results are tracked via BuildReport for
    /// post-mortem analysis by both the Unity console and PowerShell launcher.
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
                    "  1. Directory structure + URP\n" +
                    "  2. ScriptableObjects (Buildings, Constants)\n" +
                    "  3. Visual Assets (procedural meshes + shader materials)\n" +
                    "  4-5. Zone + Quest Definitions\n" +
                    "  6. Character prefabs (Player, Milo, MudGolem)\n" +
                    "  7. Scenes (Boot, UI_Overlay)\n" +
                    "  8. Echohaven scene population\n" +
                    "  9. Apply Visual Upgrade (materials + building prefabs)\n" +
                    "  10. Input actions + Build settings\n\n" +
                    "Existing assets are preserved (idempotent).",
                    "Build It!", "Cancel"))
                    return;
            }

            RunBuild();

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                string msg = BuildReport.HasFailures
                    ? $"{BuildReport.FailCount} phase(s) failed. Check Console for details."
                    : $"All {BuildReport.PassCount} phases completed. Boot scene is ready!";

                EditorUtility.DisplayDialog("Build Complete", msg, "OK");
            }
        }

        /// <summary>
        /// Batch-mode safe entry point:
        /// Unity.exe -batchmode -projectPath ... -executeMethod Tartaria.Editor.OneClickBuild.RunBuild -quit
        /// </summary>
        public static void RunBuild()
        {
            BuildReport.Begin("BUILD EVERYTHING");
            RunBuildPhases();
            BuildReport.Finish();
        }

        /// <summary>
        /// Raw build phases WITHOUT owning the BuildReport lifecycle.
        /// Call this from AutoPlayBoot (which owns Begin/Finish) to avoid
        /// resetting the outer report.
        /// </summary>
        public static void RunBuildPhases()
        {

            // ── Phase 1: Directories + URP ──
            BuildReport.RunPhase("Phase 1/10: Directories", () =>
            {
                EnsureDirectories();
                URPSetup.EnsureURPPipeline();
            });

            // ── Phase 2: ScriptableObjects + Scene skeleton ──
            BuildReport.RunPhase("Phase 2/10: ScriptableObjects", () =>
            {
                ProjectSetupWizard.RunSetup();
            });

            // ── Phase 3: Visual Assets (meshes + materials + skybox) ──
            BuildReport.RunPhase("Phase 3/10: Visual Assets", () =>
            {
                VisualUpgradeBuilder.BuildVisualAssets();
            });

            // ── Phase 4: Zone Definitions ──
            BuildReport.RunPhase("Phase 4/10: Zone Definitions", () =>
            {
                ZoneDefinitionFactory.BuildZoneDefinitions();
            });

            // ── Phase 5: Quest Definitions ──
            BuildReport.RunPhase("Phase 5/10: Quest Definitions", () =>
            {
                QuestDefinitionFactory.BuildAllQuests();
            });

            // ── Phase 6: Character Prefabs ──
            BuildReport.RunPhase("Phase 6/10: Character Prefabs", () =>
            {
                CharacterPrefabFactory.BuildAllCharacters();
            });

            // ── Phase 7: Scenes (Boot + UI_Overlay) ──
            BuildReport.RunPhase("Phase 7/10: Scenes (Boot + UI_Overlay)", () =>
            {
                SceneFactory.CreateAllMissingScenes();
            });

            // ── Phase 8: Scaffold managers + populate Echohaven ──
            string echohavenPath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 8/10: Scaffold + Populate", () =>
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    MasterSceneScaffold.ScaffoldAll();
                    EchohavenScenePopulator.Populate();
                    EditorSceneManager.SaveOpenScenes();
                });
            }
            else
            {
                BuildReport.Skip("Phase 8/10: Scaffold + Populate", "Echohaven scene not found");
            }

            // ── Phase 9: Apply Visual Upgrade to scene + build building prefabs ──
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 9/10: Apply Visual Upgrade", () =>
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    VisualUpgradeBuilder.ApplyVisualUpgrade();
                    EditorSceneManager.SaveOpenScenes();
                });
            }
            else
            {
                BuildReport.Skip("Phase 9/10: Apply Visual Upgrade", "Echohaven scene not found");
            }

            // ── Phase 10: Input + Build Settings ──
            BuildReport.RunPhase("Phase 10/10: Input + Build Settings", () =>
            {
                InputActionsAssigner.AssignInputActions();
                ConfigureBuildSettings();
            });

            // ── Finalize ──
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
