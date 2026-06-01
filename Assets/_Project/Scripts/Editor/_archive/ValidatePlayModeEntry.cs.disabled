using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;

namespace Tartaria.Scripts.Editor
{
    /// <summary>
    /// Diagnostic tool to validate Play mode entry requirements.
    /// Checks for compilation errors, scene state, and other blockers.
    /// </summary>
    public class ValidatePlayModeEntry : EditorWindow
    {
        private string lastCheckResult = "";
        private bool hasCompilationErrors = false;
        private bool hasSceneLoaded = false;
        private bool hasGameObjects = false;
        private bool canEnterPlayMode = true;
        private string blockerDetails = "";

        [MenuItem("TARTARIA/Validate Play Mode Entry", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<ValidatePlayModeEntry>("Play Mode Validator");
            window.minSize = new Vector2(500, 400);
            window.RunValidation();
            window.Show();
        }

        [MenuItem("TARTARIA/Quick Play Mode Check", priority = 101)]
        public static void QuickCheck()
        {
            var result = PerformValidation();
            
            if (result.allChecksPassed)
            {
                EditorUtility.DisplayDialog(
                    "Play Mode Ready",
                    "✓ All checks passed!\n\nYou can enter Play mode.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Play Mode Blocked",
                    result.message,
                    "OK"
                );
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("TARTARIA Play Mode Validator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool checks for common blockers that prevent entering Play mode.",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Validation", GUILayout.Height(30)))
            {
                RunValidation();
            }
            
            GUILayout.Space(10);
            
            if (!string.IsNullOrEmpty(lastCheckResult))
            {
                EditorGUILayout.LabelField("Validation Results:", EditorStyles.boldLabel);
                
                // Compilation check
                DrawCheckResult("Compilation Status", !hasCompilationErrors);
                
                // Scene loaded check
                DrawCheckResult("Scene Loaded", hasSceneLoaded);
                
                // GameObjects check
                DrawCheckResult("Scene Has GameObjects", hasGameObjects);
                
                // Play mode capability check
                DrawCheckResult("Can Enter Play Mode", canEnterPlayMode);
                
                GUILayout.Space(10);
                
                if (!string.IsNullOrEmpty(blockerDetails))
                {
                    EditorGUILayout.HelpBox(blockerDetails, MessageType.Warning);
                }
                
                GUILayout.Space(10);
                
                // Summary
                if (hasCompilationErrors || !hasSceneLoaded || !hasGameObjects || !canEnterPlayMode)
                {
                    EditorGUILayout.HelpBox(
                        "❌ BLOCKED: Play mode entry is blocked. See details above.",
                        MessageType.Error
                    );
                    
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Instructions to Fix:", EditorStyles.boldLabel);
                    DrawFixInstructions();
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "✓ READY: All checks passed! You can enter Play mode.",
                        MessageType.Info
                    );
                    
                    GUILayout.Space(10);
                    if (GUILayout.Button("Enter Play Mode Now", GUILayout.Height(30)))
                    {
                        EditorApplication.isPlaying = true;
                    }
                }
            }
        }

        private void DrawCheckResult(string label, bool passed)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(passed ? "✓" : "✗", GUILayout.Width(20));
            EditorGUILayout.LabelField(label, passed ? EditorStyles.label : EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFixInstructions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (hasCompilationErrors)
            {
                GUILayout.Label("• Fix Compilation Errors:", EditorStyles.boldLabel);
                GUILayout.Label("  1. Open Console window (Ctrl+Shift+C)");
                GUILayout.Label("  2. Resolve all CS#### errors");
                GUILayout.Label("  3. Wait for recompilation to complete");
                GUILayout.Space(5);
            }
            
            if (!hasSceneLoaded)
            {
                GUILayout.Label("• Load a Scene:", EditorStyles.boldLabel);
                GUILayout.Label("  1. File > Open Scene");
                GUILayout.Label("  2. Or double-click a scene in Project window");
                GUILayout.Label("  3. Recommended: Boot_b1.unity");
                GUILayout.Space(5);
            }
            
            if (!hasGameObjects)
            {
                GUILayout.Label("• Add GameObjects to Scene:", EditorStyles.boldLabel);
                GUILayout.Label("  1. Scene appears empty or invalid");
                GUILayout.Label("  2. Load a different scene with content");
                GUILayout.Label("  3. Or create GameObject via Hierarchy panel");
                GUILayout.Space(5);
            }
            
            if (!canEnterPlayMode)
            {
                GUILayout.Label("• Unity Internal Issue:", EditorStyles.boldLabel);
                GUILayout.Label("  1. Check Editor.log for errors");
                GUILayout.Label("  2. Restart Unity Editor");
                GUILayout.Label("  3. Verify Unity 6 installation");
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void RunValidation()
        {
            var result = PerformValidation();
            
            hasCompilationErrors = result.hasCompilationErrors;
            hasSceneLoaded = result.hasSceneLoaded;
            hasGameObjects = result.hasGameObjects;
            canEnterPlayMode = result.canEnterPlayMode;
            blockerDetails = result.blockerDetails;
            lastCheckResult = result.message;
        }

        private static ValidationResult PerformValidation()
        {
            var result = new ValidationResult();
            var sb = new StringBuilder();
            
            // Check 1: Compilation errors
            result.hasCompilationErrors = EditorUtility.scriptCompilationFailed;
            
            // Check 2: Scene loaded
            var activeScene = EditorSceneManager.GetActiveScene();
            result.hasSceneLoaded = activeScene.IsValid() && activeScene.isLoaded;
            
            // Check 3: Scene has GameObjects
            if (result.hasSceneLoaded)
            {
                result.hasGameObjects = activeScene.rootCount > 0;
            }
            
            // Check 4: Can enter play mode (basic check)
            result.canEnterPlayMode = !EditorApplication.isCompiling && !EditorApplication.isUpdating;
            
            // Build detailed blocker message
            if (result.hasCompilationErrors)
            {
                sb.AppendLine("✗ Compilation Errors Detected");
                sb.AppendLine("  Scripts are failing to compile. Check Console (Ctrl+Shift+C) for errors.");
                sb.AppendLine();
            }
            
            if (!result.hasSceneLoaded)
            {
                sb.AppendLine("✗ No Scene Loaded");
                sb.AppendLine("  Scene: " + (activeScene.IsValid() ? activeScene.name : "None"));
                sb.AppendLine("  Load a scene from Assets/_Project/Scenes/ to continue.");
                sb.AppendLine();
            }
            else if (!result.hasGameObjects)
            {
                sb.AppendLine("✗ Scene is Empty");
                sb.AppendLine("  Scene: " + activeScene.name);
                sb.AppendLine("  The scene has no root GameObjects. This may be invalid.");
                sb.AppendLine();
            }
            
            if (!result.canEnterPlayMode)
            {
                if (EditorApplication.isCompiling)
                {
                    sb.AppendLine("✗ Scripts are Compiling");
                    sb.AppendLine("  Wait for compilation to finish before entering Play mode.");
                }
                else if (EditorApplication.isUpdating)
                {
                    sb.AppendLine("✗ Unity is Updating");
                    sb.AppendLine("  Wait for Unity to finish updating assets.");
                }
                sb.AppendLine();
            }
            
            result.blockerDetails = sb.ToString().TrimEnd();
            
            // Build summary message
            sb.Clear();
            sb.AppendLine(result.allChecksPassed ? "✓ Play Mode Ready" : "❌ Play Mode Blocked");
            sb.AppendLine();
            sb.AppendLine($"{(result.hasCompilationErrors ? "✗" : "✓")} Compilation Status: {(result.hasCompilationErrors ? "ERRORS" : "Clean")}");
            sb.AppendLine($"{(result.hasSceneLoaded ? "✓" : "✗")} Scene Loaded: {(result.hasSceneLoaded ? activeScene.name : "None")}");
            sb.AppendLine($"{(result.hasGameObjects ? "✓" : "✗")} GameObjects: {(result.hasGameObjects ? activeScene.rootCount + " root objects" : "Empty scene")}");
            sb.AppendLine($"{(result.canEnterPlayMode ? "✓" : "✗")} Play Mode Ready: {(result.canEnterPlayMode ? "Yes" : "No")}");
            
            if (!string.IsNullOrEmpty(result.blockerDetails))
            {
                sb.AppendLine();
                sb.AppendLine("BLOCKERS:");
                sb.AppendLine(result.blockerDetails);
            }
            
            result.message = sb.ToString();
            return result;
        }

        private struct ValidationResult
        {
            public bool hasCompilationErrors;
            public bool hasSceneLoaded;
            public bool hasGameObjects;
            public bool canEnterPlayMode;
            public string blockerDetails;
            public string message;
            
            public bool allChecksPassed => !hasCompilationErrors && hasSceneLoaded && hasGameObjects && canEnterPlayMode;
        }
    }
}
