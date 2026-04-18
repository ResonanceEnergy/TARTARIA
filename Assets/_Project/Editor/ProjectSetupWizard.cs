using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// One-click project setup wizard.
    /// Menu: Tartaria > Setup Vertical Slice
    ///
    /// Creates:
    ///   1. Echohaven_VerticalSlice scene with all managers
    ///   2. ScriptableObject assets (3 buildings, constants, perf profile)
    ///   3. Test geometry for Echohaven plaza
    ///   4. Directional light + camera rig
    /// </summary>
    public static class ProjectSetupWizard
    {
        const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string ConfigPath = "Assets/_Project/Config";
        const string PrefabPath = "Assets/_Project/Prefabs";

        [MenuItem("Tartaria/Setup Vertical Slice", false, 0)]
        public static void SetupVerticalSlice()
        {
            if (!EditorUtility.DisplayDialog("Tartaria Setup",
                "This will create the Echohaven vertical slice scene with all managers, " +
                "ScriptableObject configs, and test geometry.\n\nProceed?",
                "Build It!", "Cancel"))
                return;

            RunSetup();
        }

        /// <summary>
        /// Batch-mode entry point — no dialogs, safe for -batchmode -executeMethod.
        /// Usage: Unity.exe -batchmode -projectPath ... -executeMethod Tartaria.Editor.ProjectSetupWizard.RunSetup -quit -logFile -
        /// </summary>
        public static void RunSetup()
        {
            EnsureDirectories();
            CreateScriptableObjects();
            CreateScene();

            bool autoPlay = BuildReport.IsRunning || File.Exists(Path.Combine(Application.dataPath,
                "..", "Library", "TARTARIA_AUTOPLAY"));
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode && !autoPlay)
            {
                EditorUtility.DisplayDialog("Tartaria Setup Complete",
                    "Echohaven vertical slice is ready!\n\n" +
                    "- Scene: _Project/Scenes/Echohaven_VerticalSlice\n" +
                    "- Configs: _Project/Config/\n" +
                    "- 3 buildings placed in the plaza\n\n" +
                    "Press Play to test.", "Let's Go!");
            }
            else
            {
                Debug.Log("[Tartaria] Batch setup complete.");
            }
        }

        [MenuItem("Tartaria/Rebuild Test Geometry", false, 1)]
        public static void RebuildGeometry()
        {
            CreateEchohavenGeometry();
            Debug.Log("[Tartaria] Test geometry rebuilt.");
        }

        // ─── Directory Setup ─────────────────────────

        static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/_Project/Scenes",
                "Assets/_Project/Config",
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Buildings",
                "Assets/_Project/Materials",
                "Assets/_Project/VFX",
                "Assets/_Project/Audio/Music",
                "Assets/_Project/Audio/SFX",
                "Assets/_Project/Textures",
                "Assets/_Project/Models",
            };

            foreach (var dir in dirs)
            {
                string fullPath = Path.Combine(Application.dataPath, "..", dir);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    Debug.Log($"[Tartaria] Created: {dir}");
                }
            }
            AssetDatabase.Refresh();
        }

        // ─── ScriptableObject Creation ───────────────

        static void CreateScriptableObjects()
        {
            // Game Constants
            CreateAssetIfMissing<Core.TartariaConstants>(
                $"{ConfigPath}/TartariaConstants.asset");

            // Performance Profiles
            CreateAssetIfMissing<Core.PerformanceProfile>(
                $"{ConfigPath}/PerformanceProfile_Recommended.asset",
                (profile) => {
                    profile.targetFrameRate = 60;
                    profile.renderScale = 1.0f;
                    profile.enableFSR = true;
                    profile.aetherGridX = 64;
                    profile.aetherGridY = 64;
                    profile.aetherGridZ = 32;
                });

            CreateAssetIfMissing<Core.PerformanceProfile>(
                $"{ConfigPath}/PerformanceProfile_Minimum.asset",
                (profile) => {
                    profile.targetFrameRate = 30;
                    profile.renderScale = 0.75f;
                    profile.enableFSR = true;
                    profile.enableDLSS = false;
                    profile.shadowCascades = 2;
                    profile.shadowDistance = 60f;
                    profile.aetherGridX = 32;
                    profile.aetherGridY = 32;
                    profile.aetherGridZ = 16;
                    profile.aetherCellSize = 4.0f;
                    profile.aetherGPUCompute = false;
                    profile.texturesBudget = 1024;
                    profile.meshesBudget = 300;
                    profile.maxActiveParticleSystems = 16;
                    profile.maxDrawDistance = 300f;
                    profile.lodBias = 0.7f;
                });

            // Building Definitions — 3 Echohaven buildings
            CreateBuildingDefinition("Echohaven_StarDome",
                "The Star Dome", Gameplay.BuildingArchetype.Dome,
                "A celestial observatory with a perfect golden-ratio dome. " +
                "Its acoustic chamber once amplified 432 Hz frequencies across the entire settlement.",
                20f, 32.36f,  // width, height (golden ratio: 20 × 1.618 = 32.36)
                15f, 12f, 0); // aether strength, radius, band 0=Telluric

            CreateBuildingDefinition("Echohaven_HarmonicFountain",
                "The Harmonic Fountain", Gameplay.BuildingArchetype.Fountain,
                "A resonance fountain that once channeled Aether from deep underground. " +
                "Its spiral channels follow the golden ratio in every curve.",
                8f, 5f,       // width, height
                20f, 8f, 1);  // aether strength, radius, band 1=Harmonic

            CreateBuildingDefinition("Echohaven_CrystalSpire",
                "The Crystal Spire", Gameplay.BuildingArchetype.Spire,
                "A towering frequency antenna that connected Echohaven to the celestial Aether band. " +
                "Built from resonance-forged crystal, it hummed at 1296 Hz.",
                6f, 40f,      // width, height
                25f, 20f, 2); // aether strength, radius, band 2=Celestial

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] ScriptableObject configs created.");
        }

        static void CreateAssetIfMissing<T>(string path, System.Action<T> configure = null)
            where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return;

            var asset = ScriptableObject.CreateInstance<T>();
            configure?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
        }

        static void CreateBuildingDefinition(string fileName, string displayName,
            Gameplay.BuildingArchetype archetype, string lore,
            float width, float height, float aetherStr, float aetherRad, int band)
        {
            string path = $"{ConfigPath}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<Gameplay.BuildingDefinition>(path) != null) return;

            var def = ScriptableObject.CreateInstance<Gameplay.BuildingDefinition>();
            def.buildingName = displayName;
            def.loreDescription = lore;
            def.archetype = archetype;
            def.width = width;
            def.height = height;
            def.goldenRatioTarget = 1.618f;
            def.aetherSourceStrength = aetherStr;
            def.aetherSourceRadius = aetherRad;
            def.outputBand = (Core.HarmonicBand)band;
            def.nodeCount = 3;
            def.dissolutionDuration = 5f;

            // Tuning puzzles — one of each variant
            def.nodePuzzles = new Gameplay.TuningPuzzleConfig[]
            {
                new Gameplay.TuningPuzzleConfig
                {
                    variant = Gameplay.TuningVariant.FrequencySlider,
                    targetFrequency = 432f,
                    timeLimitSeconds = 15f,
                    tolerancePercent = 5f,
                    difficultySpeed = 1f
                },
                new Gameplay.TuningPuzzleConfig
                {
                    variant = Gameplay.TuningVariant.WaveformTrace,
                    targetFrequency = 432f,
                    timeLimitSeconds = 20f,
                    tolerancePercent = 10f,
                    difficultySpeed = 0.8f
                },
                new Gameplay.TuningPuzzleConfig
                {
                    variant = Gameplay.TuningVariant.HarmonicPattern,
                    targetFrequency = 432f,
                    timeLimitSeconds = 12f,
                    tolerancePercent = 8f,
                    difficultySpeed = 1.2f
                }
            };

            AssetDatabase.CreateAsset(def, path);
        }

        // ─── Scene Creation ──────────────────────────

        static void CreateScene()
        {
            // Skip if scene already exists — geometry is managed by
            // EchohavenScenePopulator (Phase 8) and VisualUpgradeBuilder (Phase 9)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log($"[Tartaria] Scene already exists: {ScenePath} — skipping creation.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Directional Light (warm golden sunlight) ===
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.92f, 0.75f); // Warm golden
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // === GameManagers (root) ===
            var managersGO = new GameObject("--- GAME MANAGERS ---");

            // GameBootstrap lives in Boot.unity only — do NOT add to gameplay scene

            // GameLoopController (central orchestrator)
            var loopGO = new GameObject("GameLoopController");
            loopGO.transform.SetParent(managersGO.transform);
            loopGO.AddComponent<GameLoopController>();

            // AudioManager
            var audioGO = new GameObject("AudioManager");
            audioGO.transform.SetParent(managersGO.transform);
            audioGO.AddComponent<Audio.AudioManager>();

            // AdaptiveMusicController
            var musicGO = new GameObject("AdaptiveMusicController");
            musicGO.transform.SetParent(managersGO.transform);
            musicGO.AddComponent<Audio.AdaptiveMusicController>();

            // SaveManager
            var saveGO = new GameObject("SaveManager");
            saveGO.transform.SetParent(managersGO.transform);
            saveGO.AddComponent<Save.SaveManager>();

            // UIManager
            var uiRootGO = new GameObject("UIManager");
            uiRootGO.transform.SetParent(managersGO.transform);
            uiRootGO.AddComponent<UI.UIManager>();

            // HUDController
            var hudGO = new GameObject("HUDController");
            hudGO.transform.SetParent(uiRootGO.transform);
            hudGO.AddComponent<UI.HUDController>();

            // HapticFeedbackManager
            var hapticGO = new GameObject("HapticFeedbackManager");
            hapticGO.transform.SetParent(managersGO.transform);
            hapticGO.AddComponent<Input.HapticFeedbackManager>();

            // CombatBridge
            var combatGO = new GameObject("CombatBridge");
            combatGO.transform.SetParent(managersGO.transform);
            combatGO.AddComponent<CombatBridge>();

            // DialogueManager
            var dialogueGO = new GameObject("DialogueManager");
            dialogueGO.transform.SetParent(managersGO.transform);
            dialogueGO.AddComponent<DialogueManager>();

            // ZoneController
            var zoneGO = new GameObject("ZoneController");
            zoneGO.transform.SetParent(managersGO.transform);
            zoneGO.AddComponent<ZoneController>();

            // VFXController
            var vfxGO = new GameObject("VFXController");
            vfxGO.transform.SetParent(managersGO.transform);
            vfxGO.AddComponent<VFXController>();

            // DebugOverlay
            var debugGO = new GameObject("DebugOverlay");
            debugGO.transform.SetParent(managersGO.transform);
            debugGO.AddComponent<DebugOverlay>();

            // WorldInitializer (creates companion, spawn triggers, building ECS entities)
            var worldInitGO = new GameObject("WorldInitializer");
            worldInitGO.transform.SetParent(managersGO.transform);
            worldInitGO.AddComponent<WorldInitializer>();

            // PlayerSpawner (runtime player instantiation with greybox fallback)
            var playerSpawnerGO = new GameObject("PlayerSpawner");
            playerSpawnerGO.transform.SetParent(managersGO.transform);
            playerSpawnerGO.AddComponent<PlayerSpawner>();

            // BuildingSpawner (creates building GameObjects from ECS entities)
            var buildingSpawnerGO = new GameObject("BuildingSpawner");
            buildingSpawnerGO.transform.SetParent(managersGO.transform);
            buildingSpawnerGO.AddComponent<BuildingSpawner>();

            // SceneLoader lives in Boot.unity only — do NOT add to gameplay scene

            // === Player ===
            var playerGO = CreatePlayer();

            // === Camera Rig ===
            var cameraRigGO = CreateCameraRig(playerGO.transform);

            // === Wire GameLoopController references ===
            var loopCtrl = loopGO.GetComponent<GameLoopController>();
            if (loopCtrl != null)
            {
                var loopSO = new SerializedObject(loopCtrl);
                var inputProp = loopSO.FindProperty("playerInput");
                var camProp = loopSO.FindProperty("cameraController");
                if (inputProp != null)
                    inputProp.objectReferenceValue = playerGO.GetComponent<Input.PlayerInputHandler>();
                if (camProp != null)
                    camProp.objectReferenceValue = cameraRigGO.GetComponent<Camera.CameraController>();
                loopSO.ApplyModifiedProperties();
            }

            // Geometry is created by EchohavenScenePopulator (Phase 7)
            // and upgraded by VisualUpgradeBuilder (Phase 7b).

            // Save scene
            string dir = Path.GetDirectoryName(ScenePath);
            string fullDir = Path.Combine(Application.dataPath, "..", dir);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Tartaria] Scene saved: {ScenePath}");
        }

        static GameObject CreatePlayer()
        {
            var playerGO = new GameObject("Player");
            playerGO.tag = "Player";
            playerGO.layer = LayerMask.NameToLayer("Player") >= 0 ?
                LayerMask.NameToLayer("Player") : 0;
            playerGO.transform.position = new Vector3(0f, 1f, 0f);

            // Character controller
            var cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Input handler
            playerGO.AddComponent<Input.PlayerInputHandler>();

            // Placeholder mesh (capsule)
            var meshGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            meshGO.name = "PlayerMesh_Placeholder";
            meshGO.transform.SetParent(playerGO.transform);
            meshGO.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            Object.DestroyImmediate(meshGO.GetComponent<CapsuleCollider>());

            var renderer = meshGO.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.6f, 0.9f); // Aether blue
                mat.name = "Player_Placeholder_Mat";
                renderer.sharedMaterial = mat;
            }

            return playerGO;
        }

        static GameObject CreateCameraRig(Transform followTarget)
        {
            var cameraRigGO = new GameObject("CameraRig");
            cameraRigGO.transform.position = new Vector3(0f, 15f, -15f);
            cameraRigGO.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

            var cam = cameraRigGO.AddComponent<UnityEngine.Camera>();
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 500f;
            cam.backgroundColor = new Color(0.06f, 0.04f, 0.08f); // Deep purple-black

            var camCtrl = cameraRigGO.AddComponent<Camera.CameraController>();
            // We'll use SerializedObject to set the followTarget since it's a serialized field
            var so = new SerializedObject(camCtrl);
            var prop = so.FindProperty("followTarget");
            if (prop != null)
            {
                prop.objectReferenceValue = followTarget;
                so.ApplyModifiedProperties();
            }

            // Audio Listener — only if none exists yet
            if (Object.FindAnyObjectByType<AudioListener>() == null)
                cameraRigGO.AddComponent<AudioListener>();

            return cameraRigGO;
        }

        // ─── Echohaven Test Geometry ─────────────────

        static void CreateEchohavenGeometry()
        {
            // Clean up existing test geometry
            var existing = GameObject.Find("--- ECHOHAVEN GEOMETRY ---");
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject("--- ECHOHAVEN GEOMETRY ---");

            // Ground plane — large stone plaza
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Plaza_Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(20f, 1f, 20f); // 200m × 200m
            ground.transform.position = Vector3.zero;
            SetMaterial(ground, new Color(0.45f, 0.4f, 0.35f), "Stone_Plaza_Mat");

            // Surrounding terrain (raised edges for bowl shape)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float radius = 90f;
                var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edge.name = $"Terrain_Edge_{i}";
                edge.transform.SetParent(root.transform);
                edge.transform.position = new Vector3(
                    Mathf.Sin(angle) * radius,
                    2f,
                    Mathf.Cos(angle) * radius);
                edge.transform.localScale = new Vector3(30f, 8f, 30f);
                edge.transform.rotation = Quaternion.Euler(0f, i * 45f, 0f);
                SetMaterial(edge, new Color(0.3f, 0.35f, 0.25f), "Terrain_Edge_Mat");
            }

            // === BUILDING 1: Star Dome (center-north) ===
            CreateBuildingPlaceholder(root.transform,
                "StarDome_Buried", new Vector3(0f, 0f, 30f),
                new Vector3(20f, 5f, 20f), // Buried — mostly underground
                new Color(0.35f, 0.25f, 0.15f), // Mud brown
                "Star Dome — buried under 1000 years of mud");

            // Discovery trigger sphere (visual indicator)
            CreateTriggerSphere(root.transform,
                "StarDome_DiscoveryRadius", new Vector3(0f, 1f, 30f),
                30f, new Color(0.9f, 0.75f, 0.2f, 0.05f));

            // === BUILDING 2: Harmonic Fountain (center-east) ===
            CreateBuildingPlaceholder(root.transform,
                "HarmonicFountain_Buried", new Vector3(35f, 0f, -10f),
                new Vector3(8f, 3f, 8f),
                new Color(0.3f, 0.2f, 0.12f),
                "Harmonic Fountain — sealed beneath sediment");

            CreateTriggerSphere(root.transform,
                "Fountain_DiscoveryRadius", new Vector3(35f, 1f, -10f),
                30f, new Color(0.2f, 0.5f, 0.9f, 0.05f));

            // === BUILDING 3: Crystal Spire (center-west) ===
            CreateBuildingPlaceholder(root.transform,
                "CrystalSpire_Buried", new Vector3(-30f, 0f, -5f),
                new Vector3(6f, 8f, 6f),
                new Color(0.25f, 0.2f, 0.15f),
                "Crystal Spire — tip barely visible above the mud");

            CreateTriggerSphere(root.transform,
                "Spire_DiscoveryRadius", new Vector3(-30f, 1f, -5f),
                30f, new Color(0.8f, 0.85f, 1f, 0.05f));

            // === Environmental features ===

            // Aether vent (ground glow point — center of plaza)
            var vent = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vent.name = "AetherVent_Central";
            vent.transform.SetParent(root.transform);
            vent.transform.position = new Vector3(0f, 0.1f, 0f);
            vent.transform.localScale = new Vector3(3f, 0.2f, 3f);
            SetMaterial(vent, new Color(0.2f, 0.6f, 0.9f), "AetherVent_Mat");

            // Mud patches (scattered corruption zones)
            CreateMudPatch(root.transform, new Vector3(15f, 0.05f, 15f), 6f);
            CreateMudPatch(root.transform, new Vector3(-20f, 0.05f, 20f), 8f);
            CreateMudPatch(root.transform, new Vector3(25f, 0.05f, -25f), 10f);
            CreateMudPatch(root.transform, new Vector3(-15f, 0.05f, -30f), 5f);

            // Ruined pillars (environmental storytelling)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                float radius = 50f;
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"RuinedPillar_{i}";
                pillar.transform.SetParent(root.transform);
                pillar.transform.position = new Vector3(
                    Mathf.Sin(angle) * radius,
                    Random.Range(1f, 4f),
                    Mathf.Cos(angle) * radius);
                pillar.transform.localScale = new Vector3(1.5f, Random.Range(2f, 6f), 1.5f);
                pillar.transform.rotation = Quaternion.Euler(
                    Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
                SetMaterial(pillar, new Color(0.5f, 0.48f, 0.42f), "RuinedPillar_Mat");
            }

            // Milo spawn point
            var miloSpawn = new GameObject("Milo_SpawnPoint");
            miloSpawn.transform.SetParent(root.transform);
            miloSpawn.transform.position = new Vector3(3f, 0f, -18f); // Near player start

            // Companion placeholder
            var miloGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            miloGO.name = "Milo_Placeholder";
            miloGO.transform.SetParent(miloSpawn.transform);
            miloGO.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            miloGO.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            SetMaterial(miloGO, new Color(0.9f, 0.6f, 0.2f), "Milo_Placeholder_Mat");

            // Enemy spawn markers
            CreateEnemySpawnMarker(root.transform, "GolemSpawn_RS25",
                new Vector3(20f, 0.5f, 25f), 25);
            CreateEnemySpawnMarker(root.transform, "GolemSpawn_RS50",
                new Vector3(-25f, 0.5f, 15f), 50);
            CreateEnemySpawnMarker(root.transform, "GolemSpawn_RS75",
                new Vector3(0f, 0.5f, 60f), 75);

            Debug.Log("[Tartaria] Echohaven test geometry created — 3 buildings, " +
                       "6 pillars, 4 mud patches, 3 enemy spawns, 1 Aether vent.");
        }

        static void CreateBuildingPlaceholder(Transform parent, string name,
            Vector3 position, Vector3 scale, Color color, string label)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetMaterial(go, color, $"{name}_Mat");

            // Tag for building system
            if (TagExists("TartarianBuilding"))
                go.tag = "TartarianBuilding";

            // Interactable layer
            int interactLayer = LayerMask.NameToLayer("Interactable");
            if (interactLayer >= 0) go.layer = interactLayer;

            // Add InteractableBuilding component for gameplay integration
            var interactable = go.AddComponent<InteractableBuilding>();
            // Set building ID via SerializedObject
            var so = new SerializedObject(interactable);
            var idProp = so.FindProperty("buildingId");
            if (idProp != null)
            {
                idProp.stringValue = name;
                so.ApplyModifiedProperties();
            }
        }

        static void CreateTriggerSphere(Transform parent, string name,
            Vector3 position, float radius, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            // Visual indicator (subtle wireframe in editor)
            // The sphere collider gizmo handles this in Scene view
        }

        static void CreateMudPatch(Transform parent, Vector3 position, float size)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            patch.name = $"MudPatch_{position.x:F0}_{position.z:F0}";
            patch.transform.SetParent(parent);
            patch.transform.position = position;
            patch.transform.localScale = new Vector3(size, 0.1f, size);
            SetMaterial(patch, new Color(0.28f, 0.18f, 0.1f), "MudPatch_Mat");
        }

        static void CreateEnemySpawnMarker(Transform parent, string name,
            Vector3 position, int rsThreshold)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;

            // Visual marker (red diamond)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "SpawnMarker_Visual";
            marker.transform.SetParent(go.transform);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            marker.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            SetMaterial(marker, new Color(0.8f, 0.2f, 0.1f), "EnemySpawn_Mat");
        }

        static void SetMaterial(GameObject go, Color color, string matName)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) return;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.color = color;
            mat.name = matName;
            renderer.sharedMaterial = mat;
        }

        static bool TagExists(string tag)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(tag);
                return true;
            }
            catch { return false; }
        }
    }
}
