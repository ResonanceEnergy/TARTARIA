using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Tartaria.Editor
{
    public class SceneMissingScriptAnalyzer
    {
        [MenuItem("TARTARIA/Analyze Scene Missing Scripts")]
        public static void AnalyzeCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            AnalyzeScene(scene.path);
        }

        public static void AnalyzeEchohavenScene()
        {
            string scenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            AnalyzeScene(scenePath);
        }

        private static void AnalyzeScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"Scene not found: {scenePath}");
                return;
            }

            // Load scene additively to not close current
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var report = new System.Text.StringBuilder();
            report.AppendLine("ECHOHAVEN AUDIT REPORT");
            report.AppendLine("======================");
            
            FileInfo sceneFile = new FileInfo(scenePath);
            report.AppendLine($"Scene size: {sceneFile.Length / 1024} KB");
            report.AppendLine($"Scene path: {scenePath}");
            report.AppendLine($"GameObject count: {scene.rootCount}");
            report.AppendLine();

            var missingData = new List<MissingScriptData>();
            int totalComponents = 0;
            int missingCount = 0;

            // Analyze all GameObjects in scene
            var allObjects = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

            foreach (var go in allObjects)
            {
                var components = go.GetComponents<Component>();
                totalComponents += components.Length;

                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingCount++;
                        
                        // Get SerializedObject to extract GUID
                        SerializedObject so = new SerializedObject(go);
                        var component = so.FindProperty("m_Component");
                        
                        if (component != null && i < component.arraySize)
                        {
                            var element = component.GetArrayElementAtIndex(i);
                            var componentProp = element.FindPropertyRelative("component");
                            
                            string guid = "unknown";
                            long fileID = 0;
                            
                            // Try to get missing script info
                            var scriptProp = componentProp.FindPropertyRelative("m_Script");
                            if (scriptProp != null)
                            {
                                fileID = scriptProp.longValue;
                            }

                            missingData.Add(new MissingScriptData
                            {
                                gameObjectName = go.name,
                                gameObjectPath = GetGameObjectPath(go),
                                componentIndex = i,
                                guid = guid,
                                fileID = fileID
                            });
                        }
                    }
                }
            }

            report.AppendLine($"Total GameObjects analyzed: {allObjects.Count}");
            report.AppendLine($"Total components: {totalComponents}");
            report.AppendLine($"Total missing refs: {missingCount}");
            report.AppendLine();

            // Group by potential assembly (based on GameObject naming patterns)
            var groupedByPattern = missingData
                .GroupBy(m => GetAssemblyGuess(m.gameObjectName))
                .OrderByDescending(g => g.Count());

            report.AppendLine("Breakdown by likely assembly:");
            foreach (var group in groupedByPattern)
            {
                report.AppendLine($"  {group.Key}: {group.Count()} refs");
            }
            report.AppendLine();

            // Top 15 missing references
            report.AppendLine("Top 15 Missing Script References:");
            int count = 1;
            foreach (var missing in missingData.Take(15))
            {
                report.AppendLine($"  {count}. GameObject: \"{missing.gameObjectName}\"");
                report.AppendLine($"     Path: {missing.gameObjectPath}");
                report.AppendLine($"     Component Index: {missing.componentIndex}");
                report.AppendLine($"     FileID: {missing.fileID}");
                report.AppendLine();
                count++;
            }

            if (missingCount == 0)
            {
                report.AppendLine("✓ NO MISSING SCRIPTS FOUND!");
            }
            else
            {
                report.AppendLine("Critical blockers:");
                report.AppendLine($"  - {missingCount} missing script references prevent Play mode");
                report.AppendLine($"  - Scene must be cleaned before testing");
            }

            string reportText = report.ToString();
            Debug.Log(reportText);

            // Save to file
            string reportPath = "Assets/ECHOHAVEN_MISSING_SCRIPTS_REPORT.txt";
            File.WriteAllText(reportPath, reportText);
            AssetDatabase.Refresh();
            
            Debug.Log($"Report saved to: {reportPath}");
        }

        private static string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static string GetAssemblyGuess(string gameObjectName)
        {
            // Guess assembly based on naming patterns
            if (gameObjectName.Contains("Player") || gameObjectName.Contains("Camera") || gameObjectName.Contains("Controller"))
                return "Tartaria.Gameplay";
            if (gameObjectName.Contains("Enemy") || gameObjectName.Contains("AI") || gameObjectName.Contains("NPC"))
                return "Tartaria.AI";
            if (gameObjectName.Contains("UI") || gameObjectName.Contains("Canvas") || gameObjectName.Contains("HUD"))
                return "Tartaria.UI";
            if (gameObjectName.Contains("Audio") || gameObjectName.Contains("Sound") || gameObjectName.Contains("Music"))
                return "Tartaria.Audio";
            if (gameObjectName.Contains("VFX") || gameObjectName.Contains("Particle") || gameObjectName.Contains("Effect"))
                return "Tartaria.VFX";
            if (gameObjectName.Contains("Quest") || gameObjectName.Contains("Dialogue"))
                return "Tartaria.Narrative";
            if (gameObjectName.Contains("Input") || gameObjectName.Contains("Control"))
                return "Tartaria.Input";
            
            return "Unknown/Core";
        }

        private class MissingScriptData
        {
            public string gameObjectName;
            public string gameObjectPath;
            public int componentIndex;
            public string guid;
            public long fileID;
        }
    }
}
