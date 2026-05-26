using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tartaria.Data.Validation;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom editor base class that adds validation UI to all IValidatable ScriptableObjects.
    /// Automatically displays a "Validate" button and shows validation results.
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ValidatableEditor : UnityEditor.Editor
    {
        private List<ValidationResult> _validationResults;
        private bool _showValidationResults = true;

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            // Check if target implements IValidatable
            var validatable = target as IValidatable;
            if (validatable == null)
                return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Data Validation", EditorStyles.boldLabel);

            // Validate button
            if (GUILayout.Button("Validate Data", GUILayout.Height(30)))
            {
                _validationResults = validatable.Validate();
                _showValidationResults = true;
                
                // Log results to console
                LogValidationResults(target.name, _validationResults);
            }

            // Show validation results if available
            if (_validationResults != null && _showValidationResults)
            {
                EditorGUILayout.Space(5);
                DrawValidationResults(_validationResults);
            }
        }

        private void DrawValidationResults(List<ValidationResult> results)
        {
            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ No validation issues found. Data is valid!", MessageType.Info);
                return;
            }

            // Summary
            int errors = DataValidator.GetErrorCount(results);
            int warnings = DataValidator.GetWarningCount(results);
            int infos = results.Count - errors - warnings;

            var summaryText = $"Validation Results: {errors} Error(s), {warnings} Warning(s), {infos} Info";
            var summaryType = errors > 0 ? MessageType.Error : (warnings > 0 ? MessageType.Warning : MessageType.Info);
            EditorGUILayout.HelpBox(summaryText, summaryType);

            EditorGUILayout.Space(5);

            // Scrollable results
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var result in results)
            {
                DrawValidationResult(result);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawValidationResult(ValidationResult result)
        {
            // Choose icon and color based on severity
            string icon = result.Level switch
            {
                ValidationLevel.Error => "⊗",
                ValidationLevel.Warning => "⚠",
                ValidationLevel.Info => "ℹ",
                _ => "•"
            };

            Color color = result.Level switch
            {
                ValidationLevel.Error => new Color(1f, 0.3f, 0.3f),
                ValidationLevel.Warning => new Color(1f, 0.8f, 0.2f),
                ValidationLevel.Info => new Color(0.4f, 0.7f, 1f),
                _ => Color.white
            };

            // Draw result box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var originalColor = GUI.color;
            GUI.color = color;

            EditorGUILayout.LabelField($"{icon} {result.Message}", EditorStyles.boldLabel);

            GUI.color = originalColor;

            if (!string.IsNullOrEmpty(result.Context))
            {
                EditorGUILayout.LabelField($"Context: {result.Context}", EditorStyles.wordWrappedMiniLabel);
            }

            if (!string.IsNullOrEmpty(result.FixSuggestion))
            {
                GUI.color = new Color(0.7f, 1f, 0.7f);
                EditorGUILayout.LabelField($"→ Fix: {result.FixSuggestion}", EditorStyles.wordWrappedMiniLabel);
                GUI.color = originalColor;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private void LogValidationResults(string assetName, List<ValidationResult> results)
        {
            if (results.Count == 0)
            {
                Debug.Log($"[Validation] <color=green>✓ {assetName}: No issues found</color>");
                return;
            }

            int errors = DataValidator.GetErrorCount(results);
            int warnings = DataValidator.GetWarningCount(results);

            string summary = $"[Validation] {assetName}: {errors} error(s), {warnings} warning(s)";

            foreach (var result in results)
            {
                switch (result.Level)
                {
                    case ValidationLevel.Error:
                        Debug.LogError($"{summary}\n{result}", target);
                        break;
                    case ValidationLevel.Warning:
                        Debug.LogWarning($"{summary}\n{result}", target);
                        break;
                    case ValidationLevel.Info:
                        Debug.Log($"{summary}\n{result}", target);
                        break;
                }
            }
        }
    }
}
