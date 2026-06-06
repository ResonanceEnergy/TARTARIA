using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Build pre-processor that validates all data assets before building.
    /// Prevents builds with invalid data from being created.
    /// Can be disabled via Preferences if needed for rapid iteration.
    /// </summary>
    public class DataValidationBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string PrefKey = "Tartaria.ValidateOnBuild";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Default to enabled
            if (!EditorPrefs.HasKey(PrefKey))
            {
                EditorPrefs.SetBool(PrefKey, true);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Check if validation is enabled
            if (!EditorPrefs.GetBool(PrefKey, true))
            {
                Debug.LogWarning("[Build Validation] Skipped (disabled in preferences)");
                return;
            }

            Debug.Log("[Build Validation] Running pre-build data validation...");

            bool passed = DataValidationTools.ValidateForBuild();

            if (!passed)
            {
                // Show error dialog
                bool shouldContinue = EditorUtility.DisplayDialog(
                    "Build Validation Failed",
                    "Data validation errors found!\n\n" +
                    "Building with invalid data may cause runtime crashes.\n\n" +
                    "Do you want to continue building anyway?",
                    "Cancel Build",
                    "Build Anyway (Risky)"
                );

                if (shouldContinue) // User clicked "Cancel Build"
                {
                    throw new BuildFailedException("[Build Validation] Build cancelled due to validation errors");
                }
                else
                {
                    Debug.LogWarning("[Build Validation] User chose to build despite validation errors!");
                }
            }
            else
            {
                Debug.Log("[Build Validation] <color=green>✓ All data validated successfully</color>");
            }
        }

        // ─── Preferences UI ────────────────────────────────────────────

        [PreferenceItem("Tartaria Validation")]
        private static void PreferencesGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Build Validation Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            bool enabled = EditorPrefs.GetBool(PrefKey, true);
            bool newEnabled = EditorGUILayout.Toggle(
                new GUIContent(
                    "Validate Data Before Build",
                    "When enabled, all ScriptableObject data is validated before building.\n" +
                    "Disable for rapid iteration (not recommended for production builds)."
                ),
                enabled
            );

            if (newEnabled != enabled)
            {
                EditorPrefs.SetBool(PrefKey, newEnabled);
                Debug.Log($"[Build Validation] {(newEnabled ? "Enabled" : "Disabled")}");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Validation checks all ItemData, QuestDefinition, SkillNodeData, " +
                "EquipmentItemData, and DialogueNodeData assets for:\n\n" +
                "• Null references\n" +
                "• Invalid IDs and names\n" +
                "• Negative or zero values where required\n" +
                "• Circular dependencies\n" +
                "• Missing required fields",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Run Validation Now", GUILayout.Height(30)))
            {
                DataValidationTools.RunPreBuildValidation();
            }
        }
    }
}
