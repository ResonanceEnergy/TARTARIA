#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor Utilities — Shared UI helpers for custom inspectors.
    /// Provides reusable components: collapsible sections, color-coded labels,
    /// progress bars, validation displays, quick actions, and more.
    /// </summary>
    public static class EditorUtils
    {
        // ─── Colors ────────────────────────────────────────────────
        public static readonly Color ColorBuffed = new Color(0.2f, 0.8f, 0.2f);     // Green
        public static readonly Color ColorNerfed = new Color(0.8f, 0.2f, 0.2f);     // Red
        public static readonly Color ColorDefault = Color.gray;
        public static readonly Color ColorWarning = new Color(1f, 0.7f, 0.2f);      // Orange
        public static readonly Color ColorError = new Color(0.9f, 0.1f, 0.1f);      // Red
        public static readonly Color ColorSuccess = new Color(0.1f, 0.9f, 0.1f);    // Green

        // Rarity colors
        private static readonly Dictionary<Data.ItemRarity, Color> RarityColors = new()
        {
            { Data.ItemRarity.Common, new Color(0.8f, 0.8f, 0.8f) },
            { Data.ItemRarity.Uncommon, new Color(0.2f, 0.9f, 0.2f) },
            { Data.ItemRarity.Rare, new Color(0.2f, 0.5f, 1f) },
            { Data.ItemRarity.Epic, new Color(0.7f, 0.2f, 0.9f) },
            { Data.ItemRarity.Legendary, new Color(1f, 0.6f, 0.1f) }
        };

        // ─── Foldout Section ───────────────────────────────────────
        /// <summary>
        /// Draw a collapsible section with a foldout header.
        /// </summary>
        public static bool DrawFoldoutSection(string label, bool isExpanded, Action drawContent)
        {
            EditorGUILayout.Space(5);
            var style = new GUIStyle(EditorStyles.foldoutHeader) { fontStyle = FontStyle.Bold };
            isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, label, style);

            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                drawContent?.Invoke();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return isExpanded;
        }

        // ─── Color-Coded Label ─────────────────────────────────────
        /// <summary>
        /// Draw a label with color coding.
        /// </summary>
        public static void DrawColoredLabel(string text, Color color, FontStyle fontStyle = FontStyle.Normal)
        {
            var oldColor = GUI.color;
            var style = new GUIStyle(EditorStyles.label)
            {
                fontStyle = fontStyle,
                normal = { textColor = color }
            };
            GUI.color = color;
            EditorGUILayout.LabelField(text, style);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Draw a label with rarity color coding.
        /// </summary>
        public static void DrawRarityLabel(string text, Data.ItemRarity rarity)
        {
            if (RarityColors.TryGetValue(rarity, out var color))
            {
                DrawColoredLabel(text, color, FontStyle.Bold);
            }
            else
            {
                EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
            }
        }

        // ─── Progress Bar ──────────────────────────────────────────
        /// <summary>
        /// Draw a horizontal progress bar (e.g., for HP, cooldowns).
        /// </summary>
        public static void DrawProgressBar(float current, float max, string label, Color barColor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
            EditorGUI.ProgressBar(rect, current / max, $"{current:F0} / {max:F0}");
            
            // Color overlay
            var oldColor = GUI.color;
            GUI.color = barColor;
            EditorGUI.ProgressBar(rect, current / max, "");
            GUI.color = oldColor;

            EditorGUILayout.EndHorizontal();
        }

        // ─── Stat Display ──────────────────────────────────────────
        /// <summary>
        /// Draw a stat value with color coding (green if > default, red if < default).
        /// </summary>
        public static void DrawColoredStat(string label, int value, int defaultValue = 0)
        {
            Color color = value > defaultValue ? ColorBuffed :
                          value < defaultValue ? ColorNerfed :
                          ColorDefault;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            DrawColoredLabel(value.ToString(), color, FontStyle.Bold);
            EditorGUILayout.EndHorizontal();
        }

        // ─── Validation Display ────────────────────────────────────
        /// <summary>
        /// Draw validation results with icons and colors.
        /// </summary>
        public static void DrawValidationResults(List<ValidationResult> results)
        {
            if (results == null || results.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ No issues found", MessageType.Info);
                return;
            }

            foreach (var result in results)
            {
                MessageType msgType = result.severity switch
                {
                    ValidationSeverity.Error => MessageType.Error,
                    ValidationSeverity.Warning => MessageType.Warning,
                    _ => MessageType.Info
                };

                EditorGUILayout.HelpBox(result.message, msgType);
            }
        }

        // ─── Preview Panel ─────────────────────────────────────────
        /// <summary>
        /// Draw a preview box for sprites/textures.
        /// </summary>
        public static void DrawSpritePreview(Sprite sprite, float size = 128f)
        {
            if (sprite == null)
            {
                EditorGUILayout.HelpBox("No sprite preview available", MessageType.None);
                return;
            }

            var rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
            rect.x += (EditorGUIUtility.currentViewWidth - size) * 0.5f; // Center

            EditorGUI.DrawPreviewTexture(rect, sprite.texture);
            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// Draw a preview box for 3D prefabs.
        /// </summary>
        public static void DrawPrefabPreview(GameObject prefab, float size = 128f)
        {
            if (prefab == null)
            {
                EditorGUILayout.HelpBox("No prefab preview available", MessageType.None);
                return;
            }

            var rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
            rect.x += (EditorGUIUtility.currentViewWidth - size) * 0.5f; // Center

            var editor = UnityEditor.Editor.CreateEditor(prefab);
            editor.OnPreviewGUI(rect, GUIStyle.none);
            UnityEngine.Object.DestroyImmediate(editor);
        }

        // ─── Quick Action Buttons ──────────────────────────────────
        /// <summary>
        /// Draw a row of quick action buttons.
        /// </summary>
        public static void DrawQuickActions(params (string label, Action action)[] actions)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            foreach (var (label, action) in actions)
            {
                if (GUILayout.Button(label, GUILayout.Height(28)))
                {
                    action?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        // ─── Icon Display ──────────────────────────────────────────
        private static readonly Dictionary<string, Texture2D> IconCache = new();

        /// <summary>
        /// Draw an inline icon next to a label.
        /// </summary>
        public static void DrawIconLabel(string iconName, string label)
        {
            var icon = EditorGUIUtility.IconContent(iconName);
            if (icon?.image != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                EditorGUILayout.LabelField(label);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField($"[{iconName}] {label}");
            }
        }

        // ─── Horizontal Line ───────────────────────────────────────
        /// <summary>
        /// Draw a horizontal separator line.
        /// </summary>
        public static void DrawSeparator(int thickness = 1, int padding = 10)
        {
            EditorGUILayout.Space(padding);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(padding);
        }

        // ─── Confirmation Dialog ───────────────────────────────────
        /// <summary>
        /// Show a confirmation dialog before executing an action.
        /// </summary>
        public static bool ConfirmAction(string title, string message, string okButton = "OK", string cancelButton = "Cancel")
        {
            return EditorUtility.DisplayDialog(title, message, okButton, cancelButton);
        }

        // ─── File Export ───────────────────────────────────────────
        /// <summary>
        /// Export data to JSON file with file picker dialog.
        /// </summary>
        public static void ExportToJSON<T>(T data, string defaultFileName)
        {
            string path = EditorUtility.SaveFilePanel("Export JSON", Application.dataPath, defaultFileName, "json");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"[EditorUtils] Exported to {path}");
                EditorUtility.DisplayDialog("Export Successful", $"Data exported to:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EditorUtils] Export failed: {ex.Message}");
                EditorUtility.DisplayDialog("Export Failed", $"Failed to export:\n{ex.Message}", "OK");
            }
        }

        // ─── Find References ───────────────────────────────────────
        /// <summary>
        /// Find all assets that reference a specific asset.
        /// </summary>
        public static List<string> FindReferencesToAsset(UnityEngine.Object target)
        {
            var references = new List<string>();
            var targetPath = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(targetPath)) return references;

            // Search all assets for references
            var allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.EndsWith(".asset") || p.EndsWith(".prefab") || p.EndsWith(".unity"))
                .ToArray();

            foreach (var assetPath in allAssets)
            {
                var dependencies = AssetDatabase.GetDependencies(assetPath, false);
                if (dependencies.Contains(targetPath))
                {
                    references.Add(assetPath);
                }
            }

            return references;
        }

        // ─── Duplicate Asset ───────────────────────────────────────
        /// <summary>
        /// Duplicate a ScriptableObject asset with a new name.
        /// </summary>
        public static T DuplicateAsset<T>(T source, string newName = null) where T : ScriptableObject
        {
            if (source == null) return null;

            var clone = UnityEngine.Object.Instantiate(source);
            clone.name = newName ?? $"{source.name}_Copy";

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string directory = System.IO.Path.GetDirectoryName(sourcePath);
            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{clone.name}.asset");

            AssetDatabase.CreateAsset(clone, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(clone);
            Selection.activeObject = clone;

            Debug.Log($"[EditorUtils] Duplicated {source.name} → {clone.name}");
            return clone;
        }

        // ─── Ping Asset ────────────────────────────────────────────
        /// <summary>
        /// Highlight an asset in the Project window.
        /// </summary>
        public static void PingAsset(UnityEngine.Object asset)
        {
            if (asset == null) return;
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        // ─── Box Group ─────────────────────────────────────────────
        /// <summary>
        /// Draw content inside a styled box.
        /// </summary>
        public static void DrawBoxGroup(string label, Action drawContent, MessageType boxType = MessageType.None)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                DrawSeparator(1, 3);
            }

            drawContent?.Invoke();
            EditorGUILayout.EndVertical();
        }
    }

    // ─── Validation Result ─────────────────────────────────────────
    public class ValidationResult
    {
        public string message;
        public ValidationSeverity severity;
        public UnityEngine.Object context;

        public ValidationResult(string message, ValidationSeverity severity, UnityEngine.Object context = null)
        {
            this.message = message;
            this.severity = severity;
            this.context = context;
        }
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
#endif
