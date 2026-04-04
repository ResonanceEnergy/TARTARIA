using UnityEngine;
using UnityEditor;
using System.IO;
using Tartaria.Gameplay;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Asset Factory — generates prefabs, materials, audio placeholders,
    /// and the UI Canvas for the vertical slice.
    ///
    /// Menu: Tartaria > Build Assets
    /// Individual sub-menus for each category.
    /// </summary>
    public static class AssetFactoryWizard
    {
        const string MaterialsPath = "Assets/_Project/Materials";
        const string PrefabsPath = "Assets/_Project/Prefabs/Buildings";
        const string AudioMusicPath = "Assets/_Project/Audio/Music";
        const string AudioSFXPath = "Assets/_Project/Audio/SFX";
        const string ConfigPath = "Assets/_Project/Config";

        [MenuItem("Tartaria/Build Assets/All", false, 10)]
        public static void BuildAll()
        {
            EnsureDirectories();
            CreateMaterials();
            CreateBuildingPrefabs();
            CreateAudioManifest();
            CreateUICanvas();

            // Invoke sub-factory scripts
            AudioStubFactory.BuildAudioStubs();
            AnastasiaDialoguePopulator.BuildDialogueDatabase();
            ZoneDefinitionFactory.BuildZoneDefinitions();
            QuestDefinitionFactory.BuildAllQuests();
            AnastasiaPrefabFactory.BuildAnastasiaPrefab();
            GoldenMotePrefabFactory.BuildMotePrefab();
            CharacterPrefabFactory.BuildAllCharacters();
            Moon2ZoneScaffold.BuildBuildingDefinitions();
            MoonBuildingFactory.BuildAllMoonBuildings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] All assets built (Audio, Anastasia, Zones, Quests, Prefabs, Moon2).");
        }

        [MenuItem("Tartaria/Build Assets/Materials", false, 11)]
        public static void BuildMaterials()
        {
            EnsureDirectories();
            CreateMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tartaria/Build Assets/Building Prefabs", false, 12)]
        public static void BuildPrefabs()
        {
            EnsureDirectories();
            CreateBuildingPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Public entry point for pipeline — creates building prefabs using
        /// procedural meshes (must be called after BuildVisualAssets).
        /// </summary>
        public static void BuildBuildingPrefabs()
        {
            EnsureDirectories();
            CreateBuildingPrefabs();
        }

        [MenuItem("Tartaria/Build Assets/UI Canvas", false, 13)]
        public static void BuildUICanvas()
        {
            CreateUICanvas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void EnsureDirectories()
        {
            string[] dirs = {
                MaterialsPath,
                PrefabsPath,
                AudioMusicPath,
                AudioSFXPath,
                "Assets/_Project/Prefabs/UI",
            };
            foreach (var dir in dirs)
            {
                string fullPath = Path.Combine(Application.dataPath, "..", dir);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
            }
            AssetDatabase.Refresh();
        }

        // ─── Materials (Item 5) ──────────────────────

        static void CreateMaterials()
        {
            // Use custom Tartaria shaders when available, URP/Lit as fallback
            var stoneSh = Shader.Find("Tartaria/TartarianStone");
            var mudSh = Shader.Find("Tartaria/MudDissolution");
            var aetherSh = Shader.Find("Tartaria/AetherFlow");
            var urpLit = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard");

            // Building state materials — MudDissolution shader
            if (mudSh != null)
            {
                CreateMudMaterial("Mud_Buried", mudSh, new Color(0.35f, 0.25f, 0.15f), 0f);
                CreateMudMaterial("Mud_Revealed", mudSh, new Color(0.50f, 0.40f, 0.25f), 0.35f);
            }
            else
            {
                CreateMaterialIfMissing("Mud_Buried", urpLit,
                    new Color(0.35f, 0.25f, 0.15f), 0.6f, 0.0f);
                CreateMaterialIfMissing("Mud_Revealed", urpLit,
                    new Color(0.5f, 0.4f, 0.25f), 0.4f, 0.1f);
            }

            // TartarianStone for buildings
            if (stoneSh != null)
            {
                CreateStoneMaterial("Stone_Active", stoneSh,
                    new Color(0.72f, 0.68f, 0.58f), 0.2f, 0.3f, 0.2f);
                CreateStoneMaterial("Gold_Accent", stoneSh,
                    new Color(0.9f, 0.78f, 0.3f), 0f, 1f, 1.2f);
                CreateStoneMaterial("Ground_Plaza", stoneSh,
                    new Color(0.60f, 0.56f, 0.48f), 0.5f, 0.1f, 0.05f);
            }
            else
            {
                CreateMaterialIfMissing("Stone_Active", urpLit,
                    new Color(0.7f, 0.65f, 0.55f), 0.2f, 0.5f);
                CreateMaterialIfMissing("Gold_Accent", urpLit,
                    new Color(0.9f, 0.8f, 0.3f), 0.0f, 0.95f);
                CreateMaterialIfMissing("Ground_Plaza", urpLit,
                    new Color(0.45f, 0.4f, 0.35f), 0.7f, 0.0f);
            }

            // AetherFlow for glowing elements
            if (aetherSh != null)
            {
                CreateAetherMaterial("Crystal_Active", aetherSh,
                    new Color(0.7f, 0.8f, 1f, 0.6f), 2f);
                CreateAetherMaterial("Aether_Glow", aetherSh,
                    new Color(0.2f, 0.5f, 0.9f, 0.3f), 1.5f);
                CreateAetherMaterial("Player_Aether", aetherSh,
                    new Color(0.2f, 0.6f, 0.9f, 0.4f), 1.2f);
            }
            else
            {
                CreateMaterialIfMissing("Crystal_Active", urpLit,
                    new Color(0.85f, 0.82f, 0.95f), 0.0f, 0.9f);
                CreateMaterialIfMissing("Aether_Glow", urpLit,
                    new Color(0.2f, 0.6f, 0.9f), 0.0f, 0.8f, true);
                CreateMaterialIfMissing("Player_Aether", urpLit,
                    new Color(0.2f, 0.6f, 0.9f), 0.1f, 0.6f);
            }

            // Enemy — MudDissolution for golem
            if (mudSh != null)
                CreateMudMaterial("MudGolem_Body", mudSh, new Color(0.30f, 0.20f, 0.10f), 0f);
            else
                CreateMaterialIfMissing("MudGolem_Body", urpLit,
                    new Color(0.3f, 0.2f, 0.1f), 0.8f, 0.0f);

            Debug.Log("[Tartaria] Materials created (9 custom-shader materials).");
        }

        static void CreateStoneMaterial(string name, Shader shader,
            Color baseColor, float weathering, float goldenStr, float emissionStr)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_WeatheringAmount", weathering);
            mat.SetFloat("_GoldenStrength", goldenStr);
            mat.SetFloat("_EmissionStrength", emissionStr);
            mat.SetFloat("_AetherPulse", 1.618f);
            mat.SetFloat("_RestorationProgress", 1f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateMudMaterial(string name, Shader shader,
            Color baseColor, float dissolution)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_DissolutionProgress", dissolution);
            mat.SetFloat("_EdgeWidth", 0.03f);
            mat.SetColor("_EdgeColor", new Color(0.9f, 0.75f, 0.3f));
            mat.SetFloat("_RumbleIntensity", 0.05f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateAetherMaterial(string name, Shader shader,
            Color baseColor, float intensity)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Intensity", intensity);
            mat.SetFloat("_FlowSpeed", 1f);
            mat.SetFloat("_PulseSpeed", 1.618f);
            mat.SetFloat("_FresnelPower", 3f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateMaterialIfMissing(string name, Shader shader,
            Color baseColor, float roughness, float metallic, bool emission = false)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 1f - roughness);
            mat.SetFloat("_Metallic", metallic);

            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", baseColor * 2f);
            }

            AssetDatabase.CreateAsset(mat, path);
        }

        // ─── Building Prefabs (Item 4) ───────────────

        static void CreateBuildingPrefabs()
        {
            // Load building definitions
            string[] defNames = {
                "Echohaven_StarDome",
                "Echohaven_HarmonicFountain",
                "Echohaven_CrystalSpire"
            };

            // Load materials
            var mudMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mud_Buried.mat");
            var revealedMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Mud_Revealed.mat");
            var activeMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Stone_Active.mat");
            var crystalMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/Crystal_Active.mat");

            foreach (var defName in defNames)
            {
                var def = AssetDatabase.LoadAssetAtPath<BuildingDefinition>($"{ConfigPath}/{defName}.asset");
                if (def == null)
                {
                    Debug.LogWarning($"[Tartaria] BuildingDefinition not found: {defName}");
                    continue;
                }

                string prefabPath = $"{PrefabsPath}/{defName}.prefab";
                // Always recreate to pick up latest procedural meshes
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                    AssetDatabase.DeleteAsset(prefabPath);

                // Create building GameObject hierarchy
                var root = new GameObject(def.buildingName);

                // Add BoxCollider first to satisfy RequireComponent(typeof(Collider))
                root.AddComponent<BoxCollider>();

                // Add InteractableBuilding
                var interactable = root.AddComponent<InteractableBuilding>();
                if (interactable == null)
                {
                    Debug.LogWarning($"[Tartaria] AddComponent<InteractableBuilding> returned null for {def.buildingName} — saving basic prefab.");
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    Object.DestroyImmediate(root);
                    continue;
                }
                var so = new SerializedObject(interactable);
                so.FindProperty("definition").objectReferenceValue = def;
                so.FindProperty("buildingId").stringValue = defName;

                // Main body mesh — use procedural meshes when available
                const string MeshDir = "Assets/_Project/Models/Generated";
                GameObject body;
                switch (def.archetype)
                {
                    case BuildingArchetype.Dome:
                    {
                        var domeMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/Dome.asset");
                        if (domeMesh != null)
                        {
                            body = new GameObject("Body");
                            body.AddComponent<MeshFilter>().sharedMesh = domeMesh;
                            body.AddComponent<MeshRenderer>();
                        }
                        else
                        {
                            body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        }
                        body.transform.localScale = new Vector3(def.width, def.height * 0.5f, def.width);
                        break;
                    }
                    case BuildingArchetype.Fountain:
                    {
                        var fountainMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/Fountain.asset");
                        if (fountainMesh != null)
                        {
                            body = new GameObject("Body");
                            body.AddComponent<MeshFilter>().sharedMesh = fountainMesh;
                            body.AddComponent<MeshRenderer>();
                        }
                        else
                        {
                            body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        }
                        body.transform.localScale = new Vector3(def.width, def.height * 0.5f, def.width);
                        break;
                    }
                    case BuildingArchetype.Spire:
                    {
                        var spireMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/Spire.asset");
                        if (spireMesh != null)
                        {
                            body = new GameObject("Body");
                            body.AddComponent<MeshFilter>().sharedMesh = spireMesh;
                            body.AddComponent<MeshRenderer>();
                        }
                        else
                        {
                            body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        }
                        body.transform.localScale = new Vector3(def.width, def.height, def.width);
                        break;
                    }
                    default:
                        body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        body.transform.localScale = new Vector3(def.width, def.height, def.width);
                        break;
                }
                body.name = "Body";
                body.transform.SetParent(root.transform);
                body.transform.localPosition = new Vector3(0f, body.transform.localScale.y * 0.5f, 0f);

                // Assign material references via SerializedObject
                var renderer = body.GetComponent<MeshRenderer>();
                so.FindProperty("mainRenderer").objectReferenceValue = renderer;
                if (mudMat != null)
                    so.FindProperty("mudMaterial").objectReferenceValue = mudMat;
                if (revealedMat != null)
                    so.FindProperty("revealedMaterial").objectReferenceValue = revealedMat;
                // Crystal Spire gets crystal material for active state
                var finalMat = def.archetype == BuildingArchetype.Spire && crystalMat != null
                    ? crystalMat : activeMat;
                if (finalMat != null)
                    so.FindProperty("activeMaterial").objectReferenceValue = finalMat;

                // Tuning node positions (3 points around the building)
                var tuningArray = so.FindProperty("tuningNodePositions");
                if (tuningArray != null)
                {
                    tuningArray.arraySize = 3;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = i * 120f * Mathf.Deg2Rad;
                        float nodeRadius = def.width * 0.6f;
                        var nodeGO = new GameObject($"TuningNode_{i}");
                        nodeGO.transform.SetParent(root.transform);
                        nodeGO.transform.localPosition = new Vector3(
                            Mathf.Sin(angle) * nodeRadius,
                            1.5f,
                            Mathf.Cos(angle) * nodeRadius);

                        // Visual marker
                        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        marker.name = "NodeMarker";
                        marker.transform.SetParent(nodeGO.transform);
                        marker.transform.localPosition = Vector3.zero;
                        marker.transform.localScale = Vector3.one * 0.5f;
                        Object.DestroyImmediate(marker.GetComponent<SphereCollider>());

                        tuningArray.GetArrayElementAtIndex(i).objectReferenceValue = nodeGO.transform;
                    }
                }

                // Collider for interaction (reuse one added for RequireComponent)
                var boxCollider = root.GetComponent<BoxCollider>();
                if (boxCollider == null) boxCollider = root.AddComponent<BoxCollider>();
                boxCollider.center = new Vector3(0f, body.transform.localScale.y * 0.5f, 0f);
                boxCollider.size = new Vector3(def.width, body.transform.localScale.y, def.width);

                // Building layer
                int buildingLayer = LayerMask.NameToLayer("Building");
                if (buildingLayer >= 0) root.layer = buildingLayer;

                so.ApplyModifiedProperties();

                // Save as prefab
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);
                Debug.Log($"[Tartaria] Prefab created: {prefabPath}");
            }

            Debug.Log("[Tartaria] Building prefabs created (3 Echohaven buildings).");
        }

        // ─── Audio Manifest (Item 6) ─────────────────

        static void CreateAudioManifest()
        {
            // Create a manifest documenting needed audio assets
            // Actual audio clips require WAV/OGG files — we create the manifest
            // so the team knows exactly what's needed
            string manifestPath = Path.Combine(Application.dataPath, "..",
                "Assets/_Project/Audio/AUDIO_MANIFEST.md");

            if (File.Exists(manifestPath)) return;

            string content = @"# Tartaria Audio Manifest
## Required audio assets for vertical slice

### Music (Assets/_Project/Audio/Music/)
| File | Description | Duration | Loop |
|------|-------------|----------|------|
| music_exploration_low.ogg | Exploration RS 0-25, eerie ambient | 90s | Yes |
| music_exploration_mid.ogg | Exploration RS 25-50, hopeful hints | 90s | Yes |
| music_exploration_high.ogg | Exploration RS 50-75, building warmth | 90s | Yes |
| music_exploration_peak.ogg | Exploration RS 75-100, triumphant gold | 90s | Yes |
| music_combat_loop.ogg | Combat encounter loop | 60s | Yes |
| music_tuning_ambient.ogg | Tuning mini-game backdrop | 30s | Yes |
| stinger_threshold_25.ogg | RS 25 threshold crossed | 3s | No |
| stinger_threshold_50.ogg | RS 50 threshold crossed | 3s | No |
| stinger_zone_complete.ogg | Zone restoration complete | 8s | No |

### SFX (Assets/_Project/Audio/SFX/)
| File | Description | Duration |
|------|-------------|----------|
| sfx_resonance_pulse.ogg | Resonance Pulse attack | 0.8s |
| sfx_harmonic_strike.ogg | Harmonic Strike attack | 1.0s |
| sfx_frequency_shield.ogg | Shield activation | 0.6s |
| sfx_enemy_hit.ogg | Enemy takes damage | 0.3s |
| sfx_enemy_death.ogg | Mud Golem dissolves | 1.5s |
| sfx_golem_spawn.ogg | Golem emerges from mud | 2.0s |
| sfx_building_discover.ogg | Building proximity discovery | 1.5s |
| sfx_building_emerge.ogg | Building emergence sequence | 5.0s |
| sfx_tuning_success.ogg | Tuning node completed | 1.0s |
| sfx_tuning_fail.ogg | Tuning node failed | 0.8s |
| sfx_tuning_slider_tick.ogg | Frequency slider tick | 0.1s |
| sfx_tuning_tap.ogg | Harmonic pattern tap | 0.15s |
| sfx_footstep_mud.ogg | Walking on mud | 0.3s |
| sfx_footstep_stone.ogg | Walking on stone plaza | 0.3s |
| sfx_aether_vision_on.ogg | Aether vision toggle on | 0.5s |
| sfx_aether_vision_off.ogg | Aether vision toggle off | 0.4s |
| sfx_pause_open.ogg | Pause menu open | 0.3s |
| sfx_save_complete.ogg | Save indicator | 0.4s |
| sfx_milo_chirp.ogg | Milo companion sound | 0.5s |
| sfx_ui_hover.ogg | UI button hover | 0.1s |
| sfx_ui_click.ogg | UI button click | 0.15s |

### 432 Hz Reference Tones
| File | Description |
|------|-------------|
| tone_432hz_pure.ogg | Pure 432 Hz sine wave (tuning reference) |
| tone_864hz_octave.ogg | 864 Hz octave harmonic |
| tone_1296hz_celestial.ogg | 1296 Hz celestial harmonic |
";
            File.WriteAllText(manifestPath, content);
            Debug.Log("[Tartaria] Audio manifest created.");
        }

        // ─── UI Canvas (Item 7) ──────────────────────

        static void CreateUICanvas()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/GameCanvas.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("[Tartaria] UI Canvas prefab already exists.");
                return;
            }

            // Root Canvas
            var canvasGO = new GameObject("GameCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // ─── HUD Panel ───
            var hudPanel = CreatePanel(canvasGO.transform, "HUDPanel", true);

            // RS Gauge (bottom-center)
            var rsGaugeRT = CreateUIElement(hudPanel.transform, "RSGauge",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f),
                new Vector2(200f, 200f));
            var rsFill = CreateImage(rsGaugeRT.transform, "RSFill",
                UnityEngine.UI.Image.Type.Filled, new Color(0.9f, 0.85f, 0.3f, 0.8f));
            var rsText = CreateText(rsGaugeRT.transform, "RSValue", "0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(100f, 40f), 32);

            // Aether Charge Bar (top-right)
            var aetherBarRT = CreateUIElement(hudPanel.transform, "AetherChargeBar",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -40f),
                new Vector2(200f, 24f));
            var aetherBg = CreateImage(aetherBarRT.transform, "Background",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.1f, 0.1f, 0.15f, 0.7f));
            var aetherFill = CreateImage(aetherBarRT.transform, "Fill",
                UnityEngine.UI.Image.Type.Filled, new Color(0.2f, 0.6f, 0.9f, 0.9f));
            var aetherText = CreateText(aetherBarRT.transform, "AetherValue", "100",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(80f, 24f), 16);

            // Zone Name (top-center)
            var zoneNameRT = CreateText(hudPanel.transform, "ZoneName", "Echohaven",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f),
                new Vector2(400f, 40f), 24);

            // Interaction Prompt (bottom-center, above RS gauge)
            var promptRT = CreateText(hudPanel.transform, "InteractionPrompt", "[E] Interact",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 220f),
                new Vector2(400f, 30f), 18);

            // ─── Pause Menu Panel ───
            var pausePanel = CreatePanel(canvasGO.transform, "PauseMenuPanel", false);
            var pauseBg = CreateImage(pausePanel.transform, "DimOverlay",
                UnityEngine.UI.Image.Type.Sliced, new Color(0f, 0f, 0f, 0.6f));
            StretchToParent(pauseBg.GetComponent<RectTransform>());

            var pauseTitle = CreateText(pausePanel.transform, "PauseTitle", "PAUSED",
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero,
                new Vector2(300f, 60f), 48);

            CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
                new Vector2(0.5f, 0.5f), new Vector2(0f, 40f));
            CreateButton(pausePanel.transform, "SettingsButton", "SETTINGS",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -20f));
            CreateButton(pausePanel.transform, "QuitButton", "QUIT",
                new Vector2(0.5f, 0.5f), new Vector2(0f, -80f));

            // ─── Settings Panel ───
            var settingsPanel = CreatePanel(canvasGO.transform, "SettingsPanel", false);
            var settingsBg = CreateImage(settingsPanel.transform, "DimOverlay",
                UnityEngine.UI.Image.Type.Sliced, new Color(0f, 0f, 0.05f, 0.7f));
            StretchToParent(settingsBg.GetComponent<RectTransform>());
            CreateText(settingsPanel.transform, "SettingsTitle", "SETTINGS",
                new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero,
                new Vector2(300f, 50f), 36);
            CreateButton(settingsPanel.transform, "BackButton", "BACK",
                new Vector2(0.5f, 0.15f), Vector2.zero);

            // ─── Dialogue Panel ───
            var dialoguePanel = CreatePanel(canvasGO.transform, "DialoguePanel", false);
            var dialogueBg = CreateUIElement(dialoguePanel.transform, "DialogueBG",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f),
                new Vector2(800f, 200f));
            CreateImage(dialogueBg.transform, "Background",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.05f, 0.03f, 0.08f, 0.85f));

            var speakerText = CreateText(dialogueBg.transform, "SpeakerName", "Milo",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -10f),
                new Vector2(200f, 30f), 20);
            var bodyText = CreateText(dialogueBg.transform, "DialogueBody",
                "The resonance... it's reawakening.",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -45f),
                new Vector2(750f, 140f), 16);

            // ─── Loading Panel ───
            var loadingPanel = CreatePanel(canvasGO.transform, "LoadingPanel", false);
            var loadBg = CreateImage(loadingPanel.transform, "Background",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.02f, 0.01f, 0.04f, 1f));
            StretchToParent(loadBg.GetComponent<RectTransform>());
            var loadBar = CreateUIElement(loadingPanel.transform, "ProgressBar",
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero,
                new Vector2(600f, 16f));
            CreateImage(loadBar.transform, "BarFill",
                UnityEngine.UI.Image.Type.Filled, new Color(0.9f, 0.85f, 0.3f));
            var tipText = CreateText(loadingPanel.transform, "LoadingTip",
                "The ancients built with frequencies we've forgotten...",
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero,
                new Vector2(600f, 30f), 16);

            // ─── Aether Vision Overlay ───
            var aetherOverlay = CreatePanel(canvasGO.transform, "AetherVisionOverlay", false);
            var aetherOverlayImg = CreateImage(aetherOverlay.transform, "ScreenTint",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.1f, 0.3f, 0.6f, 0.15f));
            StretchToParent(aetherOverlayImg.GetComponent<RectTransform>());

            // ─── Save Indicator ───
            var saveIndicator = CreateUIElement(canvasGO.transform, "SaveIndicator",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-60f, 40f),
                new Vector2(80f, 80f));
            CreateText(saveIndicator.transform, "SaveText", "SAVING...",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -5f),
                new Vector2(100f, 20f), 12);
            saveIndicator.gameObject.SetActive(false);

            // ─── Boss Health Bar (top-center, below zone name) ───
            var bossPanel = CreateUIElement(hudPanel.transform, "BossHealthPanel",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f),
                new Vector2(400f, 40f));
            bossPanel.gameObject.SetActive(false);
            CreateImage(bossPanel.transform, "BossBarBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.1f, 0.05f, 0.05f, 0.7f));
            var bossFill = CreateImage(bossPanel.transform, "BossBarFill",
                UnityEngine.UI.Image.Type.Filled, new Color(0.8f, 0.15f, 0.1f, 0.9f));
            var bossName = CreateText(bossPanel.transform, "BossName", "Corruption Entity",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 5f),
                new Vector2(300f, 24f), 16);

            // ─── Wave Counter (top-left) ───
            var wavePanel = CreateUIElement(hudPanel.transform, "WaveCounterPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -40f),
                new Vector2(200f, 50f));
            wavePanel.gameObject.SetActive(false);
            var waveText = CreateText(wavePanel.transform, "WaveCounter", "Wave 1/5",
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero,
                new Vector2(180f, 24f), 18);
            var waveEnemies = CreateText(wavePanel.transform, "WaveEnemies", "8 remaining",
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero,
                new Vector2(180f, 20f), 14);

            // ─── Achievement Toast (top-right corner) ───
            var achievePanel = CreateUIElement(hudPanel.transform, "AchievementToastPanel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160f, -100f),
                new Vector2(280f, 60f));
            achievePanel.gameObject.SetActive(false);
            CreateImage(achievePanel.transform, "ToastBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.1f, 0.08f, 0.14f, 0.85f));
            var achieveText = CreateText(achievePanel.transform, "AchievementText", "Achievement Unlocked!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(260f, 40f), 16);

            // ─── Moon Trophy Banner (center screen) ───
            var trophyPanel = CreateUIElement(hudPanel.transform, "MoonTrophyPanel",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero,
                new Vector2(500f, 100f));
            trophyPanel.gameObject.SetActive(false);
            CreateImage(trophyPanel.transform, "TrophyBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.08f, 0.06f, 0.12f, 0.9f));
            var trophyText = CreateText(trophyPanel.transform, "MoonTrophyText", "MOON COMPLETE",
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero,
                new Vector2(450f, 40f), 32);
            var trophySub = CreateText(trophyPanel.transform, "MoonTrophySubtext", "Echohaven Restored",
                new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero,
                new Vector2(400f, 30f), 18);

            // ─── World Map Panel ───
            var worldMapPanel = CreatePanel(canvasGO.transform, "WorldMapPanel", false);
            CreateImage(worldMapPanel.transform, "MapBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.03f, 0.02f, 0.06f, 0.95f));
            CreateText(worldMapPanel.transform, "MapTitle", "WORLD MAP",
                new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero,
                new Vector2(300f, 40f), 28);
            var mapZoneName = CreateText(worldMapPanel.transform, "ZoneName", "",
                new Vector2(0.7f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 30f), 22);
            var mapZoneDesc = CreateText(worldMapPanel.transform, "ZoneDescription", "",
                new Vector2(0.7f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 60f), 14);
            var mapZoneStatus = CreateText(worldMapPanel.transform, "ZoneStatus", "",
                new Vector2(0.7f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            var mapZoneMoon = CreateText(worldMapPanel.transform, "ZoneMoonIndex", "",
                new Vector2(0.7f, 0.38f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            CreateButton(worldMapPanel.transform, "TravelButton", "TRAVEL",
                new Vector2(0.7f, 0.28f), Vector2.zero);
            CreateButton(worldMapPanel.transform, "MapTabButton", "MAP",
                new Vector2(0.2f, 0.95f), Vector2.zero);
            CreateButton(worldMapPanel.transform, "CodexTabButton", "CODEX",
                new Vector2(0.35f, 0.95f), Vector2.zero);
            CreateButton(worldMapPanel.transform, "CloseMapButton", "CLOSE",
                new Vector2(0.95f, 0.95f), Vector2.zero);

            // ─── Skill Tree Panel ───
            var skillTreePanel = CreatePanel(canvasGO.transform, "SkillTreePanel", false);
            CreateImage(skillTreePanel.transform, "SkillBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.04f, 0.03f, 0.07f, 0.95f));
            CreateText(skillTreePanel.transform, "TreeTitle", "SKILL TREE",
                new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero,
                new Vector2(300f, 40f), 28);
            CreateText(skillTreePanel.transform, "RSDisplay", "RS: 0",
                new Vector2(0.85f, 0.95f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(120f, 30f), 18);
            CreateButton(skillTreePanel.transform, "ResonatorTab", "RESONATOR",
                new Vector2(0.15f, 0.88f), Vector2.zero);
            CreateButton(skillTreePanel.transform, "ArchitectTab", "ARCHITECT",
                new Vector2(0.35f, 0.88f), Vector2.zero);
            CreateButton(skillTreePanel.transform, "GuardianTab", "GUARDIAN",
                new Vector2(0.55f, 0.88f), Vector2.zero);
            CreateButton(skillTreePanel.transform, "HistorianTab", "HISTORIAN",
                new Vector2(0.75f, 0.88f), Vector2.zero);
            var skillDetailName = CreateText(skillTreePanel.transform, "DetailName", "",
                new Vector2(0.75f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(250f, 30f), 20);
            var skillDetailDesc = CreateText(skillTreePanel.transform, "DetailDescription", "",
                new Vector2(0.75f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(250f, 60f), 14);
            var skillDetailCost = CreateText(skillTreePanel.transform, "DetailCost", "",
                new Vector2(0.75f, 0.34f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            var skillDetailMod = CreateText(skillTreePanel.transform, "DetailModifier", "",
                new Vector2(0.75f, 0.30f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            CreateButton(skillTreePanel.transform, "UnlockButton", "UNLOCK",
                new Vector2(0.75f, 0.22f), Vector2.zero);
            CreateButton(skillTreePanel.transform, "CloseSkillButton", "CLOSE",
                new Vector2(0.95f, 0.95f), Vector2.zero);

            // ─── Quest Log Panel ───
            var questLogPanel = CreatePanel(canvasGO.transform, "QuestLogPanel", false);
            CreateImage(questLogPanel.transform, "QuestBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.03f, 0.03f, 0.06f, 0.95f));
            CreateText(questLogPanel.transform, "QuestTitle", "QUEST LOG",
                new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero,
                new Vector2(300f, 40f), 28);
            CreateButton(questLogPanel.transform, "TabActive", "ACTIVE",
                new Vector2(0.2f, 0.88f), Vector2.zero);
            CreateButton(questLogPanel.transform, "TabCompleted", "COMPLETED",
                new Vector2(0.4f, 0.88f), Vector2.zero);
            CreateButton(questLogPanel.transform, "TabAll", "ALL",
                new Vector2(0.6f, 0.88f), Vector2.zero);
            var questDetailTitle = CreateText(questLogPanel.transform, "DetailTitle", "",
                new Vector2(0.65f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 30f), 20);
            var questDetailDesc = CreateText(questLogPanel.transform, "DetailDescription", "",
                new Vector2(0.65f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 80f), 14);
            var questReward = CreateText(questLogPanel.transform, "RewardText", "",
                new Vector2(0.65f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);

            // ─── Workshop Panel ───
            var workshopPanel = CreatePanel(canvasGO.transform, "WorkshopPanel", false);
            CreateImage(workshopPanel.transform, "WorkshopBg",
                UnityEngine.UI.Image.Type.Sliced, new Color(0.04f, 0.03f, 0.06f, 0.95f));
            CreateText(workshopPanel.transform, "WorkshopTitle", "WORKSHOP",
                new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.95f), Vector2.zero,
                new Vector2(300f, 40f), 28);
            var workshopList = CreateUIElement(workshopPanel.transform, "BuildingListContainer",
                new Vector2(0.2f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(250f, 500f));
            var workshopName = CreateText(workshopPanel.transform, "BuildingName", "",
                new Vector2(0.65f, 0.75f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 30f), 22);
            var workshopTier = CreateText(workshopPanel.transform, "CurrentTier", "Tier 0",
                new Vector2(0.65f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 16);
            var workshopNextTier = CreateText(workshopPanel.transform, "NextTier", "",
                new Vector2(0.65f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 16);
            var workshopReq = CreateText(workshopPanel.transform, "RSRequirement", "",
                new Vector2(0.65f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            var workshopOutput = CreateText(workshopPanel.transform, "OutputMultiplier", "",
                new Vector2(0.65f, 0.50f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(200f, 24f), 14);
            var workshopDesc = CreateText(workshopPanel.transform, "Description", "",
                new Vector2(0.65f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(300f, 60f), 14);
            var workshopProgress = CreateUIElement(workshopPanel.transform, "TierProgress",
                new Vector2(0.65f, 0.32f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(250f, 16f));
            CreateImage(workshopProgress.transform, "ProgressFill",
                UnityEngine.UI.Image.Type.Filled, new Color(0.9f, 0.85f, 0.3f, 0.8f));
            CreateButton(workshopPanel.transform, "UpgradeButton", "UPGRADE",
                new Vector2(0.65f, 0.22f), Vector2.zero);

            // ─── Wire UIManager references ───
            var uiManager = canvasGO.AddComponent<UI.UIManager>();
            var uiSO = new SerializedObject(uiManager);
            uiSO.FindProperty("hudPanel").objectReferenceValue = hudPanel;
            uiSO.FindProperty("pauseMenuPanel").objectReferenceValue = pausePanel;
            uiSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            uiSO.FindProperty("dialoguePanel").objectReferenceValue = dialoguePanel;
            uiSO.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;
            uiSO.FindProperty("aetherVisionOverlay").objectReferenceValue = aetherOverlay;
            uiSO.FindProperty("dialogueSpeakerText").objectReferenceValue =
                speakerText.GetComponent<TMPro.TextMeshProUGUI>();
            uiSO.FindProperty("dialogueBodyText").objectReferenceValue =
                bodyText.GetComponent<TMPro.TextMeshProUGUI>();
            uiSO.FindProperty("loadingBar").objectReferenceValue =
                loadBar.GetComponentInChildren<UnityEngine.UI.Image>();
            uiSO.FindProperty("loadingTipText").objectReferenceValue =
                tipText.GetComponent<TMPro.TextMeshProUGUI>();
            uiSO.FindProperty("saveIndicator").objectReferenceValue = saveIndicator.gameObject;
            uiSO.ApplyModifiedProperties();

            // ─── Wire HUDController references ───
            var hudController = canvasGO.AddComponent<UI.HUDController>();
            var hudSO = new SerializedObject(hudController);
            hudSO.FindProperty("rsGauge").objectReferenceValue = rsGaugeRT;
            hudSO.FindProperty("rsFillImage").objectReferenceValue =
                rsFill.GetComponent<UnityEngine.UI.Image>();
            hudSO.FindProperty("rsValueText").objectReferenceValue =
                rsText.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("aetherChargeBar").objectReferenceValue =
                aetherFill.GetComponent<UnityEngine.UI.Image>();
            hudSO.FindProperty("aetherValueText").objectReferenceValue =
                aetherText.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("interactionPrompt").objectReferenceValue =
                promptRT.GetComponent<RectTransform>();
            hudSO.FindProperty("interactionText").objectReferenceValue =
                promptRT.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("zoneNameText").objectReferenceValue =
                zoneNameRT.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("bossHealthPanel").objectReferenceValue =
                bossPanel.GetComponent<RectTransform>();
            hudSO.FindProperty("bossHealthFill").objectReferenceValue =
                bossFill.GetComponent<UnityEngine.UI.Image>();
            hudSO.FindProperty("bossNameText").objectReferenceValue =
                bossName.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("waveCounterPanel").objectReferenceValue =
                wavePanel.GetComponent<RectTransform>();
            hudSO.FindProperty("waveCounterText").objectReferenceValue =
                waveText.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("waveEnemiesText").objectReferenceValue =
                waveEnemies.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("achievementToastPanel").objectReferenceValue =
                achievePanel.GetComponent<RectTransform>();
            hudSO.FindProperty("achievementToastText").objectReferenceValue =
                achieveText.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("moonTrophyPanel").objectReferenceValue =
                trophyPanel.GetComponent<RectTransform>();
            hudSO.FindProperty("moonTrophyText").objectReferenceValue =
                trophyText.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.FindProperty("moonTrophySubtext").objectReferenceValue =
                trophySub.GetComponent<TMPro.TextMeshProUGUI>();
            hudSO.ApplyModifiedProperties();

            // ─── Wire WorldMapUI ───
            var worldMap = canvasGO.AddComponent<UI.WorldMapUI>();
            var wmSO = new SerializedObject(worldMap);
            wmSO.FindProperty("mapPanel").objectReferenceValue = worldMapPanel;
            wmSO.FindProperty("zoneName").objectReferenceValue =
                mapZoneName.GetComponent<TMPro.TextMeshProUGUI>();
            wmSO.FindProperty("zoneDescription").objectReferenceValue =
                mapZoneDesc.GetComponent<TMPro.TextMeshProUGUI>();
            wmSO.FindProperty("zoneStatus").objectReferenceValue =
                mapZoneStatus.GetComponent<TMPro.TextMeshProUGUI>();
            wmSO.FindProperty("zoneMoonIndex").objectReferenceValue =
                mapZoneMoon.GetComponent<TMPro.TextMeshProUGUI>();
            wmSO.ApplyModifiedProperties();

            // ─── Wire SkillTreeUI ───
            var skillTree = canvasGO.AddComponent<UI.SkillTreeUI>();
            var stSO = new SerializedObject(skillTree);
            stSO.FindProperty("skillTreePanel").objectReferenceValue = skillTreePanel;
            stSO.FindProperty("detailName").objectReferenceValue =
                skillDetailName.GetComponent<TMPro.TextMeshProUGUI>();
            stSO.FindProperty("detailDescription").objectReferenceValue =
                skillDetailDesc.GetComponent<TMPro.TextMeshProUGUI>();
            stSO.FindProperty("detailCost").objectReferenceValue =
                skillDetailCost.GetComponent<TMPro.TextMeshProUGUI>();
            stSO.FindProperty("detailModifier").objectReferenceValue =
                skillDetailMod.GetComponent<TMPro.TextMeshProUGUI>();
            stSO.ApplyModifiedProperties();

            // ─── Wire QuestLogUI ───
            var questLog = canvasGO.AddComponent<UI.QuestLogUI>();
            var qlSO = new SerializedObject(questLog);
            qlSO.FindProperty("questLogPanel").objectReferenceValue = questLogPanel;
            qlSO.FindProperty("detailTitle").objectReferenceValue =
                questDetailTitle.GetComponent<TMPro.TextMeshProUGUI>();
            qlSO.FindProperty("detailDescription").objectReferenceValue =
                questDetailDesc.GetComponent<TMPro.TextMeshProUGUI>();
            qlSO.FindProperty("rewardText").objectReferenceValue =
                questReward.GetComponent<TMPro.TextMeshProUGUI>();
            qlSO.ApplyModifiedProperties();

            // ─── Wire WorkshopUIPanel ───
            var workshop = canvasGO.AddComponent<UI.WorkshopUIPanel>();
            var wsSO = new SerializedObject(workshop);
            wsSO.FindProperty("panelRoot").objectReferenceValue = workshopPanel;
            wsSO.FindProperty("buildingListContainer").objectReferenceValue =
                workshopList.GetComponent<RectTransform>();
            wsSO.FindProperty("buildingNameText").objectReferenceValue =
                workshopName.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("currentTierText").objectReferenceValue =
                workshopTier.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("nextTierText").objectReferenceValue =
                workshopNextTier.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("rsRequirementText").objectReferenceValue =
                workshopReq.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("outputMultiplierText").objectReferenceValue =
                workshopOutput.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("descriptionText").objectReferenceValue =
                workshopDesc.GetComponent<TMPro.TextMeshProUGUI>();
            wsSO.FindProperty("tierProgressBar").objectReferenceValue =
                workshopProgress.GetComponentInChildren<UnityEngine.UI.Image>();
            wsSO.ApplyModifiedProperties();

            // Save as prefab
            string dir = Path.Combine(Application.dataPath, "..", "Assets/_Project/Prefabs/UI");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);
            Object.DestroyImmediate(canvasGO);
            Debug.Log("[Tartaria] UI Canvas prefab created with HUD, Pause, Settings, Dialogue, Loading, AetherVision, SaveIndicator, WorldMap, SkillTree, QuestLog, Workshop panels.");
        }

        // ─── UI Helpers ──────────────────────────────

        static RectTransform CreateUIElement(Transform parent, string name,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        static GameObject CreatePanel(Transform parent, string name, bool active)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            StretchToParent(go.GetComponent<RectTransform>());
            go.SetActive(active);
            return go;
        }

        static GameObject CreateImage(Transform parent, string name,
            UnityEngine.UI.Image.Type type, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            StretchToParent(go.GetComponent<RectTransform>());
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.type = type;
            img.color = color;
            return go;
        }

        static RectTransform CreateText(Transform parent, string name, string text,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return rt;
        }

        static void CreateButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform),
                typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(240f, 50f);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(0.15f, 0.12f, 0.2f, 0.8f);

            var textRT = CreateText(go.transform, "Label", label,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(220f, 40f), 20);
        }

        static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
