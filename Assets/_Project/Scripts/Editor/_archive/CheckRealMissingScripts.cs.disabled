using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor utility to accurately count and report missing script references
    /// across all scenes and prefabs in the project.
    /// </summary>
    public class CheckRealMissingScripts : EditorWindow
    {
        private Vector2 scrollPosition;
        private string reportText = "";
        private bool isScanning = false;
        private int totalMissing = 0;

        [MenuItem("TARTARIA/Count Real Missing Scripts")]
        private static void ShowWindow()
        {
            var window = GetWindow<CheckRealMissingScripts>("Missing Scripts Report");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        [MenuItem("TARTARIA/Count Real Missing Scripts (Console Log Only)")]
        private static void CountMissingScriptsConsole()
        {
            var report = ScanForMissingScripts();
            Debug.Log(report.ReportText);
            Debug.Log($"<color=cyan>═══ SCAN COMPLETE ═══</color>");
            Debug.Log($"<color=yellow>Total Missing References: {report.TotalMissing}</color>");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Missing Script Reference Scanner", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Scans all scenes in Build Settings and all prefabs in Assets/_Project/ for missing script references.",
                MessageType.Info
            );
            EditorGUILayout.Space(10);

            GUI.enabled = !isScanning;
            if (GUILayout.Button("Scan Project", GUILayout.Height(30)))
            {
                RunScan();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            if (!string.IsNullOrEmpty(reportText))
            {
                EditorGUILayout.LabelField($"Total Missing References: {totalMissing}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);
                if (GUILayout.Button("Copy Report to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = reportText;
                    Debug.Log("Report copied to clipboard");
                }
            }
        }

        private void RunScan()
        {
            isScanning = true;
            var report = ScanForMissingScripts();
            reportText = report.ReportText;
            totalMissing = report.TotalMissing;
            isScanning = false;
            Repaint();
        }

        private static MissingScriptReport ScanForMissingScripts()
        {
            var report = new MissingScriptReport();
            var sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine("   TARTARIA - MISSING SCRIPT REFERENCE REPORT");
            sb.AppendLine($"   Scan Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine();

            // Save current scene
            var currentScene = EditorSceneManager.GetActiveScene();
            var currentScenePath = currentScene.path;

            try
            {
                // Scan scenes in Build Settings
                sb.AppendLine("─── SCANNING SCENES IN BUILD SETTINGS ───");
                var buildScenes = EditorBuildSettings.scenes;
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    var scenePath = buildScenes[i].path;
                    if (string.IsNullOrEmpty(scenePath) || !buildScenes[i].enabled)
                        continue;

                    EditorUtility.DisplayProgressBar(
                        "Scanning Scenes",
                        $"Scene {i + 1}/{buildScenes.Length}: {System.IO.Path.GetFileNameWithoutExtension(scenePath)}",
                        (float)i / buildScenes.Length
                    );

                    var sceneReport = ScanScene(scenePath);
                    if (sceneReport.MissingCount > 0)
                    {
                        sb.AppendLine($"\n[SCENE] {scenePath}");
                        sb.AppendLine($"  Missing References: {sceneReport.MissingCount}");
                        foreach (var item in sceneReport.Items)
                        {
                            sb.AppendLine($"    • {item}");
                        }
                        report.TotalMissing += sceneReport.MissingCount;
                    }
                }

                EditorUtility.ClearProgressBar();

                // Scan prefabs in Assets/_Project/
                sb.AppendLine("\n─── SCANNING PREFABS IN Assets/_Project/ ───");
                var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" });
                
                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    
                    EditorUtility.DisplayProgressBar(
                        "Scanning Prefabs",
                        $"Prefab {i + 1}/{prefabGuids.Length}: {System.IO.Path.GetFileName(prefabPath)}",
                        (float)i / prefabGuids.Length
                    );

                    var prefabReport = ScanPrefab(prefabPath);
                    if (prefabReport.MissingCount > 0)
                    {
                        sb.AppendLine($"\n[PREFAB] {prefabPath}");
                        sb.AppendLine($"  Missing References: {prefabReport.MissingCount}");
                        foreach (var item in prefabReport.Items)
                        {
                            sb.AppendLine($"    • {item}");
                        }
                        report.TotalMissing += prefabReport.MissingCount;
                    }
                }

                EditorUtility.ClearProgressBar();
            }
            finally
            {
                // Restore original scene
                if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != EditorSceneManager.GetActiveScene().path)
                {
                    EditorSceneManager.OpenScene(currentScenePath);
                }
                EditorUtility.ClearProgressBar();
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine($"   TOTAL MISSING REFERENCES: {report.TotalMissing}");
            sb.AppendLine("═══════════════════════════════════════════════════════");

            report.ReportText = sb.ToString();
            return report;
        }

        private static AssetReport ScanScene(string scenePath)
        {
            var report = new AssetReport();
            
            try
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var rootObjects = scene.GetRootGameObjects();

                foreach (var root in rootObjects)
                {
                    ScanGameObjectHierarchy(root, report);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error scanning scene {scenePath}: {ex.Message}");
            }

            return report;
        }

        private static AssetReport ScanPrefab(string prefabPath)
        {
            var report = new AssetReport();

            try
            {
                // Load prefab contents (creates temporary scene)
                var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                
                if (prefabRoot != null)
                {
                    ScanGameObjectHierarchy(prefabRoot, report);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error scanning prefab {prefabPath}: {ex.Message}");
            }

            return report;
        }

        private static void ScanGameObjectHierarchy(GameObject root, AssetReport report)
        {
            // Use GetComponentsInChildren to get all components in hierarchy
            var allComponents = root.GetComponentsInChildren<Component>(true);

            foreach (var component in allComponents)
            {
                // Check for null components (destroyed/missing)
                if (component == null)
                {
                    var go = GetGameObjectForNullComponent(root, allComponents);
                    if (go != null)
                    {
                        report.MissingCount++;
                        report.Items.Add($"{GetGameObjectPath(go)} - Missing Component");
                    }
                    continue;
                }

                // Check SerializedObject for m_Script with fileID: 0
                var so = new SerializedObject(component);
                var scriptProperty = so.FindProperty("m_Script");
                
                if (scriptProperty != null && scriptProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (scriptProperty.objectReferenceValue == null && scriptProperty.objectReferenceInstanceIDValue != 0)
                    {
                        // This indicates a missing script (fileID exists but reference is null)
                        report.MissingCount++;
                        report.Items.Add($"{GetGameObjectPath(component.gameObject)} - {component.GetType().Name}");
                    }
                }
            }
        }

        private static GameObject GetGameObjectForNullComponent(GameObject root, Component[] allComponents)
        {
            // When we find a null component, we need to find which GameObject it belongs to
            // This is tricky - we'll scan all GameObjects and check their components
            var allGameObjects = root.GetComponentsInChildren<Transform>(true)
                .Select(t => t.gameObject)
                .ToArray();

            foreach (var go in allGameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null)
                    {
                        return go;
                    }
                }
            }

            return root; // Fallback to root if we can't determine
        }

        private static string GetGameObjectPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private class MissingScriptReport
        {
            public int TotalMissing { get; set; }
            public string ReportText { get; set; }
        }

        private class AssetReport
        {
            public int MissingCount { get; set; }
            public List<string> Items { get; } = new List<string>();
        }
    }
}
