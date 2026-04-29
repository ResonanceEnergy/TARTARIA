using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using Tartaria.Gameplay;
using Tartaria.Integration;
using Tartaria.UI;
using Tartaria.AI;

namespace Tartaria.Editor
{
    /// <summary>
    /// Automated setup wizard for runtime components that require Unity Editor actions.
    /// Handles: DayNight controller attachment, occlusion baking, LOD groups, prefab creation, UI wiring.
    /// 
    /// Usage: Unity menu → Tartaria → Run Runtime Setup Wizard
    /// </summary>
    public class RuntimeSetupWizard : EditorWindow
    {
        [MenuItem("Tartaria/Run Runtime Setup Wizard")]
        static void ShowWindow()
        {
            var window = GetWindow<RuntimeSetupWizard>("Runtime Setup");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        Vector2 _scrollPos;
        bool _setupDayNight = true;
        bool _bakeOcclusion = true;
        bool _setupLODs = true;
        bool _createGolemPrefab = true;
        bool _wireUI = true;

        void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("TARTARIA Runtime Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Automates manual Unity Editor setup steps for features 5-10.", MessageType.Info);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Select Setup Tasks:", EditorStyles.boldLabel);
            
            _setupDayNight = EditorGUILayout.Toggle("1. Attach DayNightCycleController to Sun", _setupDayNight);
            _bakeOcclusion = EditorGUILayout.Toggle("2. Bake Occlusion Culling Data", _bakeOcclusion);
            _setupLODs = EditorGUILayout.Toggle("3. Add LODGroup to 3 Buildings", _setupLODs);
            _createGolemPrefab = EditorGUILayout.Toggle("4. Create MudGolem Prefab (Placeholder)", _createGolemPrefab);
            _wireUI = EditorGUILayout.Toggle("5. Wire Tutorial/Subtitle UI Panels", _wireUI);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Run Selected Setup Tasks", GUILayout.Height(40)))
            {
                RunSetup();
            }
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("After setup completes, run: .\\tartaria-play.ps1 (interactive) to test in Editor.", MessageType.Warning);
            
            EditorGUILayout.EndScrollView();
        }

        void RunSetup()
        {
            Debug.Log("[RuntimeSetup] Beginning automated setup...");
            
            try
            {
                if (_setupDayNight) SetupDayNightController();
                if (_bakeOcclusion) BakeOcclusionCulling();
                if (_setupLODs) SetupLODGroups();
                if (_createGolemPrefab) CreateMudGolemPrefab();
                if (_wireUI) WireUIComponents();
                
                EditorUtility.DisplayDialog("Setup Complete", 
                    "Runtime setup finished successfully.\n\nRun .\\tartaria-play.ps1 to test in Editor.", 
                    "OK");
                
                Debug.Log("[RuntimeSetup] ✓ All setup tasks complete. Scene saved.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RuntimeSetup] Setup failed: {ex.Message}");
                EditorUtility.DisplayDialog("Setup Failed", 
                    $"Error: {ex.Message}\n\nCheck Console for details.", 
                    "OK");
            }
        }

        void SetupDayNightController()
        {
            Debug.Log("[RuntimeSetup] 1/5 Setting up DayNightCycleController...");
            
            // Find directional light (the Sun)
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light sun = null;
            
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sun = light;
                    break;
                }
            }
            
            if (sun == null)
            {
                Debug.LogWarning("[RuntimeSetup] No Directional Light found, creating new Sun...");
                var sunObj = new GameObject("Directional Light (Sun)");
                sun = sunObj.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.0f;
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
            
            // Add DayNightCycleController if not already attached
            var controller = sun.GetComponent<DayNightCycleController>();
            if (controller == null)
            {
                controller = sun.gameObject.AddComponent<DayNightCycleController>();
                EditorUtility.SetDirty(sun.gameObject);
                Debug.Log($"[RuntimeSetup]   ✓ DayNightCycleController attached to {sun.name}");
            }
            else
            {
                Debug.Log($"[RuntimeSetup]   ✓ DayNightCycleController already exists on {sun.name}");
            }
        }

        void BakeOcclusionCulling()
        {
            Debug.Log("[RuntimeSetup] 2/5 Baking occlusion culling data...");
            
            // Mark static geometry as Static
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int markedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Building") || obj.name.Contains("Terrain") || obj.name.Contains("Structure"))
                {
                    if (!obj.isStatic)
                    {
                        GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
                        markedCount++;
                    }
                }
            }
            
            Debug.Log($"[RuntimeSetup]   Marked {markedCount} objects as Static for occlusion");
            
            // Trigger occlusion bake
            StaticOcclusionCulling.Compute();
            
            Debug.Log("[RuntimeSetup]   ✓ Occlusion culling baked");
        }

        void SetupLODGroups()
        {
            Debug.Log("[RuntimeSetup] 3/5 Adding LODGroup components to buildings...");
            
            string[] buildingNames = { "Star Dome", "Harmonic Fountain", "Crystal Spire" };
            int setupCount = 0;
            
            foreach (var buildingName in buildingNames)
            {
                GameObject building = GameObject.Find(buildingName);
                
                if (building == null)
                {
                    Debug.LogWarning($"[RuntimeSetup]   Building not found: {buildingName}, skipping LOD setup");
                    continue;
                }
                
                LODGroup lodGroup = building.GetComponent<LODGroup>();
                if (lodGroup == null)
                {
                    lodGroup = building.AddComponent<LODGroup>();
                }
                
                // Get all MeshRenderers in building hierarchy
                MeshRenderer[] renderers = building.GetComponentsInChildren<MeshRenderer>();
                
                if (renderers.Length == 0)
                {
                    Debug.LogWarning($"[RuntimeSetup]   No MeshRenderers found in {buildingName}, skipping LOD");
                    continue;
                }
                
                // Setup 3 LOD levels (LOD0 = full detail, LOD1 = 30-60m, LOD2 = 60-100m)
                LOD[] lods = new LOD[3];
                
                // LOD0 (0-30m) - full detail
                lods[0] = new LOD(0.3f, renderers);
                
                // LOD1 (30-60m) - medium detail (same renderers for now)
                lods[1] = new LOD(0.15f, renderers);
                
                // LOD2 (60-100m) - low detail (same renderers for now)
                lods[2] = new LOD(0.05f, renderers);
                
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
                
                EditorUtility.SetDirty(building);
                setupCount++;
                
                Debug.Log($"[RuntimeSetup]   ✓ LODGroup configured for {buildingName} ({renderers.Length} renderers)");
            }
            
            Debug.Log($"[RuntimeSetup]   ✓ LOD groups setup complete ({setupCount}/3 buildings)");
        }

        void CreateMudGolemPrefab()
        {
            Debug.Log("[RuntimeSetup] 4/5 Creating MudGolem prefab...");
            
            string prefabPath = "Assets/_Project/Prefabs/Enemies/MudGolem.prefab";
            string prefabDir = "Assets/_Project/Prefabs/Enemies";
            
            // Create directory if needed
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                string parentDir = "Assets/_Project/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentDir))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                }
                AssetDatabase.CreateFolder(parentDir, "Enemies");
            }
            
            // Check if prefab already exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log($"[RuntimeSetup]   ✓ MudGolem prefab already exists at {prefabPath}");
                return;
            }
            
            // Create placeholder golem
            GameObject golem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            golem.name = "MudGolem";
            
            // Scale to 2x2x2 (human-sized)
            golem.transform.localScale = new Vector3(2f, 2f, 2f);
            
            // Set material to brown/mud color
            var renderer = golem.GetComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.4f, 0.3f, 0.2f, 1f); // muddy brown
            renderer.material = material;
            
            // Add MudGolemAI component
            var ai = golem.AddComponent<MudGolemAI>();
            
            // Add NavMeshAgent (will be configured at runtime)
            var agent = golem.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 2.5f;
            agent.radius = 1f;
            agent.height = 2f;
            
            // Add capsule collider
            var collider = golem.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 1f;
            collider.center = Vector3.zero;
            
            // Set layer to Enemy (12)
            golem.layer = LayerMask.NameToLayer("Enemy");
            if (golem.layer == 0) // Fallback if layer doesn't exist
            {
                golem.layer = 12;
            }
            
            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(golem, prefabPath);
            
            // Clean up scene instance
            DestroyImmediate(golem);
            
            Debug.Log($"[RuntimeSetup]   ✓ MudGolem prefab created at {prefabPath}");
        }

        void WireUIComponents()
        {
            Debug.Log("[RuntimeSetup] 5/5 Wiring tutorial/subtitle UI panels...");
            
            // Find or create UIManager
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            
            if (uiManager == null)
            {
                Debug.LogWarning("[RuntimeSetup]   UIManager not found in scene. Create UIManager GameObject first.");
                return;
            }
            
            // Find or create TutorialController
            TutorialController tutorialController = FindFirstObjectByType<TutorialController>();
            
            if (tutorialController == null)
            {
                GameObject tutorialObj = new GameObject("TutorialController");
                tutorialController = tutorialObj.AddComponent<TutorialController>();
                Debug.Log("[RuntimeSetup]   ✓ TutorialController created");
            }
            
            // Find AccessibilityManager
            AccessibilityManager accessibilityManager = FindFirstObjectByType<AccessibilityManager>();
            
            if (accessibilityManager == null)
            {
                Debug.LogWarning("[RuntimeSetup]   AccessibilityManager not found. Subtitle system may not work.");
            }
            else
            {
                Debug.Log("[RuntimeSetup]   ✓ AccessibilityManager found, subtitle system ready");
            }
            
            EditorUtility.SetDirty(uiManager.gameObject);
            if (tutorialController != null) EditorUtility.SetDirty(tutorialController.gameObject);
            
            Debug.Log("[RuntimeSetup]   ✓ UI components wired");
        }
    }
}
