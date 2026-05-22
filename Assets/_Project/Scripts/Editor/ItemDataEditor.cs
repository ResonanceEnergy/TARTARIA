#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Inspector for ItemData — Designer-friendly item editor.
    /// Features: sprite preview, rarity color coding, stat graphs, validation,
    /// quick actions (duplicate, export, find references, test in scene).
    /// </summary>
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : UnityEditor.Editor
    {
        private ItemData _item;
        private bool _showBasic = true;
        private bool _showAdvanced = false;
        private bool _showDebug = false;
        private List<ValidationResult> _validationResults = new();

        void OnEnable()
        {
            _item = (ItemData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ─── Header ────────────────────────────────────────────
            EditorGUILayout.Space(10);
            EditorUtils.DrawRarityLabel($"Item: {_item.displayName}", _item.rarity);
            EditorUtils.DrawSeparator();

            // ─── Preview Panel ─────────────────────────────────────
            if (_item.icon != null)
            {
                EditorUtils.DrawSpritePreview(_item.icon, 128f);
                EditorGUILayout.LabelField("Icon Preview", EditorStyles.centeredGreyMiniLabel);
            }

            EditorUtils.DrawSeparator();

            // ─── Quick Actions ─────────────────────────────────────
            EditorUtils.DrawQuickActions(
                ("Validate", ValidateItem),
                ("Duplicate", DuplicateItem),
                ("Export JSON", ExportItem),
                ("Find References", FindReferences)
            );

            EditorUtils.DrawSeparator();

            // ─── Collapsible Sections ──────────────────────────────
            _showBasic = EditorUtils.DrawFoldoutSection("Basic Properties", _showBasic, DrawBasicSection);
            _showAdvanced = EditorUtils.DrawFoldoutSection("Advanced Properties", _showAdvanced, DrawAdvancedSection);
            _showDebug = EditorUtils.DrawFoldoutSection("Debug Info", _showDebug, DrawDebugSection);

            // ─── Validation Results ────────────────────────────────
            if (_validationResults.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
                EditorUtils.DrawValidationResults(_validationResults);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemID"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rarity"));

            // Color-coded value display
            EditorGUILayout.Space(5);
            EditorUtils.DrawColoredStat("Base Value (RS)", _item.value, 10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
        }

        void DrawAdvancedSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stackSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("customData"));

            // Prefab preview
            if (_item.worldPrefab != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("World Prefab Preview", EditorStyles.boldLabel);
                EditorUtils.DrawPrefabPreview(_item.worldPrefab, 128f);
            }
        }

        void DrawDebugSection()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Asset Path", AssetDatabase.GetAssetPath(_item));
            EditorGUILayout.TextField("GUID", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_item)));
            EditorGUILayout.TextField("Instance ID", _item.GetInstanceID().ToString());
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy Asset Path"))
            {
                EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(_item);
                Debug.Log($"[ItemDataEditor] Copied path: {AssetDatabase.GetAssetPath(_item)}");
            }
        }

        void ValidateItem()
        {
            _validationResults.Clear();

            // Validate ID
            if (string.IsNullOrWhiteSpace(_item.itemID))
            {
                _validationResults.Add(new ValidationResult("Item ID is empty", ValidationSeverity.Error, _item));
            }
            else if (_item.itemID.Contains(" "))
            {
                _validationResults.Add(new ValidationResult("Item ID contains spaces (use underscores)", ValidationSeverity.Warning, _item));
            }

            // Validate display name
            if (string.IsNullOrWhiteSpace(_item.displayName))
            {
                _validationResults.Add(new ValidationResult("Display name is empty", ValidationSeverity.Error, _item));
            }

            // Validate icon
            if (_item.icon == null)
            {
                _validationResults.Add(new ValidationResult("No icon assigned", ValidationSeverity.Warning, _item));
            }

            // Validate stack size
            if (_item.stackSize < 1)
            {
                _validationResults.Add(new ValidationResult("Stack size must be at least 1", ValidationSeverity.Error, _item));
            }

            // Validate value
            if (_item.value < 0)
            {
                _validationResults.Add(new ValidationResult("Item value cannot be negative", ValidationSeverity.Error, _item));
            }

            if (_validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Success", "✓ All checks passed!", "OK");
            }

            Repaint();
        }

        void DuplicateItem()
        {
            if (EditorUtils.ConfirmAction("Duplicate Item", $"Create a copy of '{_item.displayName}'?"))
            {
                var clone = EditorUtils.DuplicateAsset(_item, $"{_item.itemID}_copy");
                if (clone != null)
                {
                    clone.itemID = $"{_item.itemID}_copy";
                }
            }
        }

        void ExportItem()
        {
            EditorUtils.ExportToJSON(_item, $"{_item.itemID}.json");
        }

        void FindReferences()
        {
            var references = EditorUtils.FindReferencesToAsset(_item);
            
            if (references.Count == 0)
            {
                EditorUtility.DisplayDialog("Find References", "No references found.", "OK");
            }
            else
            {
                string message = $"Found {references.Count} references:\n\n";
                foreach (var refPath in references)
                {
                    message += $"• {refPath}\n";
                }
                EditorUtility.DisplayDialog("Find References", message, "OK");
                Debug.Log($"[ItemDataEditor] References to {_item.itemID}:\n{string.Join("\n", references)}");
            }
        }
    }
}
#endif
