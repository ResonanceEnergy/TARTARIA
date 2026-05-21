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
        /// <summary>
        /// True while the auto-play pipeline (or batch build) is running.
        /// Editor tools called as pipeline phases must NOT show modal dialogs
        /// when this flag is true — they would block the headless run.
        /// </summary>
        public static bool PipelineActive { get; set; }

        /// <summary>
        /// Returns true if a modal dialog is safe to show right now
        /// (i.e. user invoked the menu item directly, not via the pipeline).
        /// </summary>
        public static bool DialogsAllowed =>
            !UnityEditorInternal.InternalEditorUtility.inBatchMode
            && !Application.isBatchMode
            && !PipelineActive;

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
            // ── Phase 0: Version Generation ──
            BuildReport.RunPhase("Phase 0/12: Build Version", () =>
            {
                BuildVersionGenerator.Generate();
            });

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
                bool hasKayKit  = System.IO.File.Exists("Assets/_Project/Models/Characters/KayKit/Rogue_Hooded.fbx");
                bool hasCapoeira = System.IO.Directory.Exists("Assets/_Project/Models/Animations/Capoeira");

                // Skip Capoeira (Humanoid rig) when KayKit (Generic) is the avatar — it can't drive a Generic skeleton
                // and only litters the prefab with an unused humanoid AnimatorController.
                if (hasCapoeira && !hasKayKit)
                {
                    AssetIntegrationTool.IntegrateCapoeiraAnimations();
                }
                else if (hasCapoeira && hasKayKit)
                {
                    Debug.Log("[OneClickBuild] KayKit Generic rig active — skipping Capoeira humanoid controller (incompatible).");
                }
                else
                {
                    Debug.LogWarning("[OneClickBuild] Capoeira animations not found - skipping animation integration");
                }

                // Replace procedural capsule with KayKit Rogue_Hooded (gender-neutral hooded silhouette).
                // Falls back to capsule if KayKit FBX missing.
                if (hasKayKit)
                {
                    AssetIntegrationTool.ReplacePlayerMeshModel();
                }
                else
                {
                    RestorePlayerCapsule.RestoreCapsule();
                }

                AssetIntegrationTool.ValidateCustomShaders();
            });

            // ── Phase 9j2: Humanoid Mesh Auto-Bind (closes §8 items 5 + 8) ──
            //    Drop a Mixamo female humanoid FBX into Assets/_Project/Models/Characters/
            //    and Player.prefab gets a real SkinnedMeshRenderer. Silently skips when
            //    the drop-zone is empty so the build stays green pre-asset-acquisition.
            BuildReport.RunPhase("Phase 9j2/19: Humanoid Mesh Auto-Bind", () =>
            {
                HumanoidAutoBinder.BindIfAvailable();
            });

            // ── Phase 9j3: KayKit Adventurers Import (chibi character pack) ──
            //    Copies the FREE KayKit Adventurers pack into the project tree
            //    (models, textures, materials, prefabs). Uses Generic rig so it
            //    coexists with HumanoidAutoBinder. Silently no-ops when the pack
            //    folder is missing.
            BuildReport.RunPhase("Phase 9j3/19: KayKit Adventurers Import", () =>
            {
                KayKitImporter.ImportAll();
            });

            // ── Phase 9j4: KayKit Extra Packs (Tools, Forest, Skeletons, Anims) ──
            //    Imports the additional FREE KayKit packs. Each pack is
            //    independently optional and silently no-ops when its source
            //    folder is missing.
            BuildReport.RunPhase("Phase 9j4/19: KayKit Extra Packs Import", () =>
            {
                KayKitPacksImporter.ImportAll();
            });

            // ── Phase 9j5: KayKit Deep Integration ──
            //    Imports adventurer weapons/gear, mannequin chars, sets up anim
            //    FBX clip flags, builds shared AnimatorControllers (Medium +
            //    Large), assigns them to every char prefab and attaches a
            //    class-appropriate weapon under each character's right hand.
            BuildReport.RunPhase("Phase 9j5/19: KayKit Deep Integration", () =>
            {
                KayKitDeepIntegrator.Run();
            });

            // ── Phase 9j6: Generate stub scenes for Moons 2–13 ─
            //    Creates an Assets/_Project/Scenes/Moons/{ZoneName}.unity stub
            //    for every moon that doesn't yet have one. Each stub has a
            //    ground plane, sun, fog/ambient tuned from ZoneDefinitionFactory,
            //    a PlayerSpawn marker, and a MainCamera.
            BuildReport.RunPhase("Phase 9j6/19: Moon Scenes Factory (stubs for Moons 2–13)", () =>
            {
                MoonScenesFactory.CreateAll();
            });

            // ── Phase 9j7: Dress every moon scene with KayKit characters + props ──
            //    Places adventurers / skeletons / forest / tools props into
            //    Echohaven AND every Moon 2–13 stub so all 13 zones contain
            //    visible content. Idempotent — recreates a single
            //    "KayKit_Dressing" root per scene each pass.
            BuildReport.RunPhase("Phase 9j7/19: KayKit Dressing (all 13 moons)", () =>
            {
                EchohavenKayKitDressing.DressAllMoons();
            });

            // ── Phase 9j8: Per-Moon definitions, quest stubs, runtime bootstrappers ──
            //    Authors:
            //      • 13 MoonDefinition SOs in Config/Moons/
            //      • 12 Quest stubs (Moon 2-13) in Config/Quests/
            //      • Drops a MoonRuntimeBootstrapper into every Moon 2-13 stub scene
            //        wired to the matching MoonDefinition (auto-applies fog, spawns
            //        player, activates per-moon quest at Start).
            BuildReport.RunPhase("Phase 9j8/19: Moon Definitions + Quest Stubs + Bootstrappers", () =>
            {
                MoonDefinitionsFactory.Run();
            });

            // ── Phase 9j9: Echohaven Combat Arena ───────────────────────────────
            //    Drops a scripted 3-wave golem encounter into Echohaven so the
            //    player gets immediate, escalating combat on scene load.
            BuildReport.RunPhase("Phase 9j9/19: Echohaven Combat Arena", () =>
            {
                EchohavenCombatArenaAttacher.Attach();
            });

            // ── Phase 9j10: Player Animator Controller (DISABLED) ──
            //   PlayerAnimatorController.controller is no longer the active controller —
            //   KayKitPlayerController.controller is bound to the player at runtime.
            //   Re-creating an empty stub here was destructive and unnecessary.
            //   If you need a fresh stub, run the menu: Tartaria/Fix/Rebuild Player Animator Controller
            // BuildReport.RunPhase("Phase 9j10/19: Player Animator Controller", () =>
            // {
            //     Tartaria.EditorTools.PlayerAnimatorControllerFactory.Run();
            // });

            // ── Phase 9j11: Master Mixer expose Master/Music/SFX/UI/Ambience volumes ──
            BuildReport.RunPhase("Phase 9j11/19: Master Mixer Exposed Params", () =>
            {
                Tartaria.EditorTools.MasterMixerExposer.Run();
            });

            // ── Phase 9j12: Application icon (procedural 13-pointed glyph) ──
            BuildReport.RunPhase("Phase 9j12/19: App Icon", () =>
            {
                Tartaria.EditorTools.AppIconFactory.Run();
            });

            // ── Phase 9k: Asset Framework Bootstrap (Mixer + Snapshots + Cue Library + default profiles) ──
            //    Idempotent — only creates assets that don't already exist.
            //    Snapshot transitions (Exploration/Combat) are wired in AudioManager
            //    and auto-trigger on GameState.Combat changes.
            BuildReport.RunPhase("Phase 9k/19: Asset Framework Bootstrap", () =>
            {
                AssetFrameworkFactory.BootstrapAll();
            });

            // ── Phase 9k2: Bind any designer-dropped ambient music tracks ──
            //    Drop .wav/.ogg files into Assets/_Project/Audio/Ambience/ and
            //    they get auto-attached to the AudioAmbience scene root.
            BuildReport.RunPhase("Phase 9k2/19: Ambience Auto-Bind", () =>
            {
                AmbienceAutoBinder.BindAll();
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

            // ── Phase 9j13: Timeline for Moon-8 Armada Flyover ──
            BuildReport.RunPhase("Phase 9j13/19: Climax Timeline (Moon 8)", () =>
            {
                ClimaxTimelineFactory.Run();
            });

            // ── Phase 9j14: VO Placeholder Generation ──
            BuildReport.RunPhase("Phase 9j14/19: VO Placeholders (12 beep tones)", () =>
            {
                AudioFactory.BuildVOPlaceholders();
            });

            // ── Phase 9j15: Volumetric Fog ──
            BuildReport.RunPhase("Phase 9j15/19: Volumetric Fog (URP or fallback)", () =>
            {
                VolumetricFogFactory.Run();
            });

            // ── Phase 9j16: Foliage Wind Shader (rebuild grass with vertex colors) ──
            BuildReport.RunPhase("Phase 9j16/19: Foliage Wind Shader", () =>
            {
                // Re-run FoliageFactory to rebuild grass with wind shader + vertex colors
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    FoliageFactory.BuildAndScatter();
                    EditorSceneManager.SaveOpenScenes();
                }
            });

            // ── Phase 9j17: Lens Flares (sun) ──
            BuildReport.RunPhase("Phase 9j17/19: Lens Flares (Sun)", () =>
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    LensFlareFactory.Run();
                    EditorSceneManager.SaveOpenScenes();
                }
            });

            // ── Phase 9j18: URP Upscaling (STP + render scale) ──
            BuildReport.RunPhase("Phase 9j18/19: URP Upscaling (STP)", () =>
            {
                UpscalingTuner.Run();
            });

            // ── Phase 9j19: HDR Output Enable ──
            BuildReport.RunPhase("Phase 9j19/19: HDR Output Enable", () =>
            {
                HDROutputConfigurator.Run();
            });

            // ── Phase 9j20: NavMesh Baking — enables MudGolem navigation ──
            BuildReport.RunPhase("Phase 9j20/19: NavMesh Baking", () =>
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(echohavenPath) != null)
                {
                    EditorSceneManager.OpenScene(echohavenPath, OpenSceneMode.Single);
                    NavMeshBaker.BakeNavMesh();
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
            // Delegate to MoonScenesFactory so Build Settings always reflects
            // the canonical Boot → Echohaven → Moons 2–13 → UI_Overlay order.
            MoonScenesFactory.UpdateBuildSettings();
        /// <summary>
        /// Applies recommended Player Settings for Moon 1 Development Builds
        /// and general performance-friendly configuration.
        ///
        /// - Fast iteration (Mono backend, windowed 1280x720)
        /// - Performance baselines (Vulkan+DX11, run in bg, reasonable defaults)
        /// - Clear branding for the vertical slice dev player
        /// Call before building dev players. For release builds, switch backend to IL2CPP + bump res/quality.
        /// </summary>
        public static void ConfigureRecommendedPlayerSettings(bool forDevelopment = true)
        {
            PlayerSettings.productName = forDevelopment
                ? "TARTARIA - Moon 1 Vertical Slice (Dev)"
                : "TARTARIA";
            PlayerSettings.companyName = "Resonance Forge";
            PlayerSettings.applicationVersion = forDevelopment ? "0.9.0-Moon1-Dev" : "1.0.0";

            // Development / iteration friendly
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;

            // Performance-oriented defaults
            PlayerSettings.use32BitDisplayBuffer = false;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,
                new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11, UnityEngine.Rendering.GraphicsDeviceType.Vulkan });

            if (forDevelopment)
            {
                // Mono for rapid dev iteration & clean debugging (IL2CPP for final perf builds)
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_4_6);
            }
            else
            {
                // Production performance path
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_4_6);
            }

            // Misc quality/perf
            QualitySettings.vSyncCount = forDevelopment ? 0 : 1; // dev: uncapped for profiling

            Debug.Log($"[Tartaria] Recommended Player Settings applied (Development={forDevelopment}, Echohaven/Moon1 focused).");
        }
    }
}
