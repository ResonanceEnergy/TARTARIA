using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Scene Integration Patch — Programmatically wires TestOrchestrator into Echohaven scene.
    ///
    /// MISSION: Add TestOrchestrator GameObject to Echohaven_VerticalSlice.unity
    ///
    /// OPERATIONS:
    /// - Create "TestOrchestrator" GameObject in scene root
    /// - Attach TestOrchestrator component
    /// - Configure autoStartOnPlay = true, phaseDelay = 1.5f
    /// - Save scene with proper serialization
    ///
    /// CONSTRAINTS:
    /// - Idempotent (safe to run multiple times)
    /// - Preserves existing scene structure
    /// - NO manual Unity Editor actions required
    ///
    /// USAGE:
    /// - Menu: Tartaria/QA/Wire Test Orchestrator
    /// - Batch: Unity.exe -batchmode -projectPath ... -executeMethod Tartaria.Editor.SceneIntegrationPatch.WireTestOrchestrator -quit
    /// - PowerShell: .\apply-test-integration.ps1
    ///
    /// VALIDATION:
    /// - Verifies TestOrchestrator component exists after patching
    /// - Logs success/failure with [SceneIntegration] prefix
    /// - Exit code 0 (success) or 1 (failure) in batchmode
    /// </summary>
    public static class SceneIntegrationPatch
    {
        const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string GameObjectName = "TestOrchestrator";

        /// <summary>
        /// Menu item: Wire Test Orchestrator into Echohaven scene.
        /// </summary>
        [MenuItem("Tartaria/QA/Wire Test Orchestrator", false, 200)]
        public static void WireTestOrchestratorMenu()
        {
            bool success = WireTestOrchestratorInternal();

            if (success)
            {
                EditorUtility.DisplayDialog(
                    "Scene Integration Complete",
                    "TestOrchestrator successfully wired into Echohaven_VerticalSlice.unity\n\n" +
                    "Configuration:\n" +
                    "• autoStartOnPlay = true\n" +
                    "• phaseDelay = 1.5s\n\n" +
                    "Press Play to run automated tests.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Scene Integration Failed",
                    "Could not wire TestOrchestrator into scene.\n\n" +
                    "Check Console for error details.",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Batchmode entry point: Wire Test Orchestrator into Echohaven scene.
        /// </summary>
        public static void WireTestOrchestrator()
        {
            bool success = WireTestOrchestratorInternal();

            if (Application.isBatchMode)
            {
                int exitCode = success ? 0 : 1;
                Debug.Log($"[SceneIntegration] Exiting batchmode with code {exitCode}");
                EditorApplication.Exit(exitCode);
            }
        }

        /// <summary>
        /// Get the TestOrchestrator type using reflection (can't directly reference test assemblies from Editor).
        /// </summary>
        static System.Type GetTestOrchestratorType()
        {
            // Find the Tartaria.Tests assembly
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var testAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Tartaria.Tests");

            if (testAssembly == null)
            {
                LogError("Could not find Tartaria.Tests assembly");
                return null;
            }

            // Get the TestOrchestrator type
            var testOrchestratorType = testAssembly.GetType("Tartaria.Tests.TestOrchestrator");

            if (testOrchestratorType == null)
            {
                LogError("Could not find TestOrchestrator type in Tartaria.Tests assembly");
                return null;
            }

            return testOrchestratorType;
        }

        /// <summary>
        /// Internal implementation: Wire Test Orchestrator into Echohaven scene.
        /// Returns true on success, false on failure.
        /// </summary>
        static bool WireTestOrchestratorInternal()
        {
            try
            {
                LogHeader();

                // Step 1: Verify scene exists
                if (!VerifySceneExists())
                {
                    return false;
                }

                // Step 2: Open scene
                if (!OpenScene())
                {
                    return false;
                }

                // Step 3: Check if TestOrchestrator already exists
                bool alreadyExists = CheckIfExists();

                // Step 4: Get TestOrchestrator type
                var testOrchestratorType = GetTestOrchestratorType();

                if (testOrchestratorType == null)
                {
                    return false;
                }

                // Step 5: Create or update TestOrchestrator GameObject
                GameObject testOrchestratorGO = GetOrCreateGameObject();

                if (testOrchestratorGO == null)
                {
                    LogError("Failed to create or retrieve TestOrchestrator GameObject");
                    return false;
                }

                // Step 6: Add or get TestOrchestrator component
                Component orchestrator = GetOrAddComponent(testOrchestratorGO, testOrchestratorType);

                if (orchestrator == null)
                // Step 8: Mark scene dirty and save
                if (!SaveScene())
                {
                    return false;
                }

                // Step 9: Validate integration
                if (!ValidateIntegration(testOrchestratorType))
                {
                    return false;
                }

                // Success
                LogSuccess(alreadyExists);
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"Exception during scene integration: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        static bool VerifySceneExists()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            if (sceneAsset == null)
            {
                LogError($"Scene not found at path: {ScenePath}");
                return false;
            }

            LogInfo($"✓ Scene found: {ScenePath}");
            return true;
        }

        static bool OpenScene()
        {
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                if (!scene.IsValid())
                {
                    LogError($"Failed to open scene: {ScenePath}");
                    return false;
                }

                LogInfo($"✓ Scene opened: {scene.name}");
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"Exception opening scene: {ex.Message}");
                return false;
            }
        }

        static bool CheckIfExists()
        {
            var existing = GameObject.Find(GameObjectName);

            if (existing != null)
            {
                LogInfo($"⚠ TestOrchestrator already exists in scene (will update configuration)");
                return true;
            }

            LogInfo($"TestOrchestrator not found in scene (will create new)");
            return false;
        }

        static GameObject GetOrCreateGameObject()
        {
            var existing = GameObject.Find(GameObjectName);

            if (existing != null)
            {
                LogInfo($"Using existing GameObject: {GameObjectName}");
                return existing;
            }

            var newGO = new GameObject(GameObjectName);
            LogInfo($"✓ Created new GameObject: {GameObjectName}");
            return newGO;
        }

        static Component GetOrAddComponent(GameObject go, System.Type componentType)
        {
            var orchestrator = go.GetComponent(componentType);

            if (orchestrator != null)
            {
                LogInfo($"Using existing TestOrchestrator component");
                return orchestrator;
            }

            orchestrator = go.AddComponent(componentType);
            LogInfo($"✓ Added TestOrchestrator component");
            return orchestrator;
        }

        static void ConfigureComponent(Component orchestrator)
        {
            // Use SerializedObject for proper dirty tracking
            var so = new SerializedObject(orchestrator);

            var autoStartProp = so.FindProperty("autoStartOnPlay");
            var phaseDelayProp = so.FindProperty("phaseDelay");

            if (autoStartProp != null)
            {
                autoStartProp.boolValue = true;
                LogInfo($"✓ Set autoStartOnPlay = true");
            }
            else
            {
                LogWarn("Could not find autoStartOnPlay property");
            }

            if (phaseDelayProp != null)
            {
                phaseDelayProp.floatValue = 1.5f;
                LogInfo($"✓ Set phaseDelay = 1.5s");
            }
            else
            {
                LogWarn("Could not find phaseDelay property");
            }

            so.ApplyModifiedProperties();
        }

        static bool SaveScene()
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = EditorSceneManager.SaveScene(scene);

                if (!saved)
                {
                    LogError("Failed to save scene");
                    return false;
                }

                LogInfo($"✓ Scene saved: {scene.name}");
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"Exception saving scene: {ex.Message}");
                return false;
            }
        }

        static bool ValidateIntegration(System.Type testOrchestratorType)
        {
            var testGO = GameObject.Find(GameObjectName);

            if (testGO == null)
            {
                LogError("Validation failed: TestOrchestrator GameObject not found after save");
                return false;
            }

            var orchestrator = testGO.GetComponent(testOrchestratorType);

            if (orchestrator == null)
            {
                LogError("Validation failed: TestOrchestrator component not found after save");
                return false;
            }

            LogInfo($"✓ Validation passed: TestOrchestrator component exists in scene");
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // LOGGING
        // ═══════════════════════════════════════════════════════════════════════════

        static void LogHeader()
        {
            Debug.Log("[SceneIntegration] ═══════════════════════════════════════════════════");
            Debug.Log("[SceneIntegration] Scene Integration Patch — TestOrchestrator");
            Debug.Log("[SceneIntegration] ═══════════════════════════════════════════════════");
        }

        static void LogInfo(string message)
        {
            Debug.Log($"[SceneIntegration] {message}");
        }

        static void LogWarn(string message)
        {
            Debug.LogWarning($"[SceneIntegration] {message}");
        }

        static void LogError(string message)
        {
            Debug.LogError($"[SceneIntegration] {message}");
        }

        static void LogSuccess(bool wasUpdate)
        {
            Debug.Log("[SceneIntegration] ═══════════════════════════════════════════════════");

            if (wasUpdate)
            {
                Debug.Log("[SceneIntegration] ✓ TestOrchestrator configuration UPDATED");
            }
            else
            {
                Debug.Log("[SceneIntegration] ✓ TestOrchestrator successfully WIRED");
            }

            Debug.Log("[SceneIntegration] Scene: Echohaven_VerticalSlice.unity");
            Debug.Log("[SceneIntegration] GameObject: TestOrchestrator");
            Debug.Log("[SceneIntegration] Component: TestOrchestrator");
            Debug.Log("[SceneIntegration] Configuration:");
            Debug.Log("[SceneIntegration]   • autoStartOnPlay = true");
            Debug.Log("[SceneIntegration]   • phaseDelay = 1.5s");
            Debug.Log("[SceneIntegration] ═══════════════════════════════════════════════════");
        }
    }
}
