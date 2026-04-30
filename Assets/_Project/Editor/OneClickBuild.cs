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

            // ── Phase 1: Directories + URP + TMP Essentials ──
            BuildReport.RunPhase("Phase 1/10: Directories", () =>
            {
                EnsureDirectories();
                URPSetup.EnsureURPPipeline();
                ImportTMPEssentials();
            });

            // ── Phase 2: ScriptableObjects + Input Actions ──
            BuildReport.RunPhase("Phase 2/12: ScriptableObjects + Input", () =>
            {
                ProjectSetupWizard.RunSetup();
                InputActionsFactory.CreateInputActionsAsset();
            });

            // ── Phase 3: Visual Assets (meshes + materials + skybox) ──
            BuildReport.RunPhase("Phase 3/10: Visual Assets", () =>
            {
                VisualUpgradeBuilder.BuildVisualAssets();
                AssetFactoryWizard.BuildBuildingPrefabs(); // Must run before Phase 8 populate
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
                AnastasiaDialoguePopulator.BuildDialogueDatabase();
                ArchiveDatabasePopulator.BuildArchiveDatabase();
            });

            // ── Phase 6: Character Prefabs ──
            BuildReport.RunPhase("Phase 6/10: Character Prefabs", () =>
            {
                CharacterPrefabFactory.BuildAllCharacters();
                AnastasiaPrefabFactory.BuildAnastasiaPrefab();
            });

            // ── Phase 7: Scenes (Boot + UI_Overlay) ──
            BuildReport.RunPhase("Phase 7/12: Scenes (Boot + UI_Overlay)", () =>
            {
                SceneFactory.CreateAllMissingScenes();
            });

            // ── Phase 7b: Populate UI Overlay ──
            string uiOverlayPath = "Assets/_Project/Scenes/UI_Overlay.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(uiOverlayPath) != null)
            {
                BuildReport.RunPhase("Phase 7b/12: Populate UI Overlay", () =>
                {
                    EditorSceneManager.OpenScene(uiOverlayPath, OpenSceneMode.Single);
                    UIOverlayPopulator.Populate();
                    EditorSceneManager.SaveOpenScenes();
                });
            }

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

            // ── Phase 9b: URP Quality Upgrade (MSAA, shadows, post-processing) ──
            BuildReport.RunPhase("Phase 9b/15: URP Quality Upgrade", () =>
            {
                URPSetup.UpgradeURPQuality();
            });

            // ── Phase 9c: Build Post-FX Volume Profile + place in scene ──
            BuildReport.RunPhase("Phase 9c/15: Post-FX Volume", () =>
            {
                PostFXVolumeFactory.BuildVolumeProfile();
                URPSetup.UpgradeURPQuality(); // re-run to pick up the now-existing profile
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    PostFXVolumeFactory.EnsureSceneVolume();
                    EditorSceneManager.SaveOpenScenes();
                }
            });

            // ── Phase 9d: Procedural audio assets ──
            BuildReport.RunPhase("Phase 9d/15: Audio (procedural)", () =>
            {
                AudioFactory.BuildAudioAssets();
            });

            // ── Phase 9d2: VFX Prefabs (Feature 3) ──
            BuildReport.RunPhase("Phase 9d2/15: VFX Prefabs", () =>
            {
                VFXFactory.BuildAllVFX();
            });

            // ── Phase 9e: Decorate prefabs (Player FX + PlayerAnimator + Building auras + detail geo) ──
            BuildReport.RunPhase("Phase 9e/15: Decorate Prefabs (FX)", () =>
            {
                AmbientFXFactory.DecorateAllPrefabs();
                BuildingDetailFactory.DecorateAllBuildings();
            });

            // ── Phase 9f: Add ambient FX + audio + foliage to scene ──
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 9f/15: Scene Decoration (FX+Audio+Foliage+Skybox)", () =>
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    SkyboxFactory.BuildAndApply();
                    AmbientFXFactory.AddAmbientToScene();
                    AudioFactory.AddAmbienceToScene();
                    FoliageFactory.BuildAndScatter();
                    EditorSceneManager.SaveOpenScenes();
                });
            }

            // ── Phase 9g: Moon 1 APV scenarios + dome VFX Graph wiring ──
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 9g/17: Moon 1 APV + Dome VFX", () =>
                {
                    Moon1LightingAuthoring.SetupMoon1APV();
                    Moon1VFXGraphSetup.WireMoon1DomeVFXGraph();
                    EditorSceneManager.SaveOpenScenes();
                });
            }

            // ── Phase 9h: Custom Shaders (P1) — Create materials from 4 custom URP shaders + apply to scene ──
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 9h/17: Custom Shaders (P1)", () =>
                {
                    CustomShaderApplicator.CreateAllMaterialsStatic();
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    CustomShaderApplicator.ApplyMaterialsToSceneStatic();
                    EditorSceneManager.SaveOpenScenes();
                });
            }

            // ── Phase 9i: VFX Upgrade (P2) — Enhance particle systems to 500-2000 particles + create Aurora ──
            BuildReport.RunPhase("Phase 9i/17: VFX Upgrade (P2)", () =>
            {
                VFXUpgradeTool.UpgradeScanPulseStatic();
                VFXUpgradeTool.UpgradeRestoreSparkleStatic();
                VFXUpgradeTool.UpgradeShardCollectStatic();
                VFXUpgradeTool.CreateAuroraVFXStatic();
            });

            // ── Phase 9j: Asset Integration (P3) — Apply downloaded FREE assets: Capoeira animations + Player mesh ──
            BuildReport.RunPhase("Phase 9j/18: Asset Integration (Capoeira + Player Mesh)", () =>
            {
                // Check if assets exist before integration
                bool hasCapoeira = System.IO.Directory.Exists("Assets/_Project/Models/Animations/Capoeira");

                if (hasCapoeira)
                {
                    AssetIntegrationTool.IntegrateCapoeiraAnimations();
                }
                else
                {
                    Debug.LogWarning("[OneClickBuild] Capoeira animations not found - skipping animation integration");
                }

                // ALWAYS restore Player to capsule (remove any wrong mesh that was applied)
                // Player is ELARA VOSS (female) - Player_Mesh.fbx was male, keeping capsule until correct model
                RestorePlayerCapsule.RestoreCapsule();

                AssetIntegrationTool.ValidateCustomShaders();
            });

            // ── Phase 9k: Asset Framework Bootstrap (Mixer + Snapshots + Cue Library + default profiles) ──
            //    Idempotent — only creates assets that don't already exist.
            //    Snapshot transitions (Exploration/Combat) are wired in AudioManager
            //    and auto-trigger on GameState.Combat changes.
            BuildReport.RunPhase("Phase 9k/19: Asset Framework Bootstrap", () =>
            {
                AssetFrameworkFactory.BootstrapAll();
            });

            // ── Phase 9l: Slice Assets — Quest_AwakenStarDome + Dialogue_Anastasia_AwakenStarDome ──
            BuildReport.RunPhase("Phase 9l/19: Slice Assets (Quest + Dialogue)", () =>
            {
                SliceAssetsFactory.EnsureSliceAssets();
            });

            // ── Phase 10: Input assignment (scene must be open) + Build Settings ──
            BuildReport.RunPhase("Phase 10/14: Input + Build Settings", () =>
            {
                InputActionsAssigner.AssignInputActions();
                EditorSceneManager.SaveOpenScenes();
                ConfigureBuildSettings();
            });

            // ── Phase 11: Scene Wiring Pass — fill all serialized references ──
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
            {
                BuildReport.RunPhase("Phase 11/14: Scene Wiring Pass", () =>
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    SceneWiringPass.WireAll();
                    EditorSceneManager.SaveOpenScenes();
                });
            }

            // ── Phase 12: Scene Validation — forbidden component check ──
            BuildReport.RunPhase("Phase 12/14: Scene Validation", () =>
            {
                int violations = SceneValidator.ValidateAll();
                if (violations > 0)
                    throw new System.Exception($"SceneValidator found {violations} forbidden component(s). See errors above.");
            });

            // ── Phase 13: Bind external assets fetched by OpenClaw (HDRI + Mixamo + PBR + Decals) ──
            BuildReport.RunPhase("Phase 13/17: External Assets (HDRI + Mixamo + PBR + Decals)", () =>
            {
                HDRISkyboxBinder.BindLatestHDRI();
                MixamoAnimatorBinder.BuildController();
                PBRMaterialBinder.BindAll();
                DecalFeatureBinder.AddDecalFeature();
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    HDRISkyboxBinder.BindLatestHDRI(); // reapply with scene loaded
                    PBRSceneApplier.Apply();
                    PBRResourceCopier.MirrorAndAttach();
                    EditorSceneManager.SaveOpenScenes();
                }
            });

            // ── Finalize ──
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Open Boot scene (the entry point)
            string bootPath = "Assets/_Project/Scenes/Boot.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath) != null)
                EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
        }

        /// <summary>
        /// Import TMP Essential Resources if not already present.
        /// Must run before any UI phases that create TextMeshPro components.
        /// Also closes the TMP importer window if it's open.
        /// </summary>
        static void ImportTMPEssentials()
        {
            // Check if TMP settings asset exists (indicates essentials are imported)
            var settings = AssetDatabase.FindAssets("t:TMP_Settings");
            if (settings.Length > 0)
            {
                Debug.Log("[Tartaria] TMP Essential Resources already imported — skipping.");
                CloseTMPImporterWindow();
                return;
            }

            // Find the .unitypackage inside the ugui package cache
            string packagePath = null;
            var ugui = System.IO.Directory.GetDirectories(
                System.IO.Path.Combine(Application.dataPath, "..", "Library", "PackageCache"), "com.unity.ugui*");
            foreach (var dir in ugui)
            {
                string candidate = System.IO.Path.Combine(dir, "Package Resources", "TMP Essential Resources.unitypackage");
                if (System.IO.File.Exists(candidate))
                {
                    packagePath = candidate;
                    break;
                }
            }

            if (packagePath == null)
            {
                Debug.LogWarning("[Tartaria] TMP Essential Resources package not found in PackageCache — TMP fonts may be missing.");
                return;
            }

            Debug.Log($"[Tartaria] Importing TMP Essential Resources from {packagePath}");
            AssetDatabase.ImportPackage(packagePath, false); // false = don't show dialog
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] TMP Essential Resources imported.");

            CloseTMPImporterWindow();
        }

        /// <summary>
        /// Close any open TMP Package Resource Importer windows.
        /// These pop up automatically and block Play mode with "Cannot import in play mode".
        /// </summary>
        public static void CloseTMPImporterWindow()
        {
            try
            {
                var windowType = System.Type.GetType(
                    "TMPro.TMP_PackageResourceImporterWindow, Unity.ugui");
                if (windowType == null)
                    windowType = System.Type.GetType(
                        "TMPro.TMP_PackageResourceImporterWindow, Unity.TextMeshPro");
                if (windowType == null) return;

                var windows = Resources.FindObjectsOfTypeAll(windowType);
                foreach (var w in windows)
                {
                    if (w is EditorWindow ew)
                    {
                        Debug.Log("[Tartaria] Closing TMP Package Resource Importer window.");
                        ew.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Tartaria] Could not close TMP importer window: {ex.Message}");
            }
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
