using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Scene setup fixer - creates missing PlayerSpawner and ensures scene is playable
    /// </summary>
    public static class SceneSetupFixer
    {
        [MenuItem("Tartaria/🚀 ONE-CLICK: Load & Setup Echohaven", false, 50)]
        public static void LoadAndSetupEchohaven()
        {
            // CANNOT run in Play Mode
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                EditorUtility.DisplayDialog(
                    "Cannot Run in Play Mode",
                    "⛔ Exit Play Mode (Ctrl+P) first, then run this tool again.",
                    "OK"
                );
                return;
            }

            // Load Echohaven scene
            string scenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Debug.Log($"[QuickSetup] Loaded scene: {scene.name}");

            // Setup the scene
            SetupCurrentScene();

            // Save
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Echohaven Ready!",
                "✓ Scene loaded\n" +
                "✓ PlayerSpawner created\n" +
                "✓ Scene saved\n\n" +
                "Press Ctrl+P to Play!",
                "OK"
            );
        }

        [MenuItem("Tartaria/FIX: Setup Current Scene for Play", false, 100)]
        public static void SetupCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            Debug.Log($"[SceneSetup] Fixing scene: {activeScene.name}");

            // 1. Find or create PlayerSpawner
            var spawner = Object.FindFirstObjectByType<Tartaria.Integration.PlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("[SceneSetup] No PlayerSpawner found - creating one...");

                var spawnerGO = new GameObject("PlayerSpawner");
                spawner = spawnerGO.AddComponent<Tartaria.Integration.PlayerSpawner>();

                // Try to find player prefab
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab Player");
                GameObject playerPrefab = null;

                foreach (var guid in prefabGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Player") && !path.Contains("Test"))
                    {
                        playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (playerPrefab != null)
                        {
                            Debug.Log($"[SceneSetup] Found player prefab: {path}");
                            break;
                        }
                    }
                }

                if (playerPrefab != null)
                {
                    // Use reflection to set private field
                    var field = typeof(Tartaria.Integration.PlayerSpawner).GetField("playerPrefab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(spawner, playerPrefab);
                        Debug.Log("[SceneSetup] Assigned player prefab to spawner");
                    }
                }

                // Try to find input actions
                string[] inputGuids = AssetDatabase.FindAssets("t:InputActionAsset");
                foreach (var guid in inputGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Tartaria") || path.Contains("Input"))
                    {
                        var inputActions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(path);
                        if (inputActions != null)
                        {
                            var inputField = typeof(Tartaria.Integration.PlayerSpawner).GetField("inputActions",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (inputField != null)
                            {
                                inputField.SetValue(spawner, inputActions);
                                Debug.Log($"[SceneSetup] Assigned input actions: {path}");
                            }
                            break;
                        }
                    }
                }

                EditorUtility.SetDirty(spawnerGO);
            }
            else
            {
                Debug.Log("[SceneSetup] PlayerSpawner already exists");
            }

            // 2. Find or create PlayerSpawn marker
            var spawnMarker = GameObject.Find("PlayerSpawn");
            if (spawnMarker == null)
            {
                spawnMarker = new GameObject("PlayerSpawn");
                spawnMarker.transform.position = new Vector3(0, 1, 0);
                Debug.Log("[SceneSetup] Created PlayerSpawn marker at (0,1,0)");
            }

            // 3. Check for Main Camera
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[SceneSetup] No Main Camera found!");
                var camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camGO.transform.position = new Vector3(0, 5, -10);
                Debug.Log("[SceneSetup] Created Main Camera");
            }

            // 4. GameStateManager is a Lazy singleton - no GameObject needed
            // It auto-creates itself on first access
            Debug.Log($"[SceneSetup] GameStateManager.Instance ready: {Tartaria.Core.GameStateManager.Instance != null}");

            // 5. Save scene
            EditorSceneManager.MarkSceneDirty(activeScene);

            EditorUtility.DisplayDialog(
                "Scene Setup Complete",
                $"Scene: {activeScene.name}\n\n" +
                $"✓ PlayerSpawner: {(spawner != null ? "Ready" : "FAILED")}\n" +
                $"✓ PlayerSpawn marker: {(spawnMarker != null ? "Ready" : "FAILED")}\n" +
                $"✓ Main Camera: {(cam != null ? "Ready" : "FAILED")}\n\n" +
                "Save scene and enter Play Mode to test.",
                "OK"
            );
        }

        [MenuItem("Tartaria/DEBUG: Show Scene Info", false, 101)]
        public static void ShowSceneInfo()
        {
            var activeScene = SceneManager.GetActiveScene();
            var spawner = Object.FindFirstObjectByType<Tartaria.Integration.PlayerSpawner>();
            var spawnMarker = GameObject.Find("PlayerSpawn");
            var player = GameObject.FindGameObjectWithTag("Player");
            var cam = Camera.main;

            string info = $"SCENE INFO\n\n";
            info += $"Scene: {activeScene.name} ({(activeScene.isLoaded ? "Loaded" : "NOT loaded")})\n";
            info += $"Root objects: {activeScene.rootCount}\n\n";
            info += $"PlayerSpawner: {(spawner != null ? "✓ Found" : "✗ Missing")}\n";
            info += $"PlayerSpawn marker: {(spawnMarker != null ? "✓ Found" : "✗ Missing")}\n";
            info += $"Player: {(player != null ? $"✓ Found at {player.transform.position}" : "✗ Missing")}\n";
            info += $"Main Camera: {(cam != null ? $"✓ Found at {cam.transform.position}" : "✗ Missing")}\n";

            Debug.Log(info);
            EditorUtility.DisplayDialog("Scene Info", info, "OK");
        }
    }
}
