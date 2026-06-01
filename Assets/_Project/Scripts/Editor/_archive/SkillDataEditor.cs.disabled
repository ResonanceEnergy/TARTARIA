#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tartaria.Data;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Inspector for SkillNodeData — Designer-friendly skill editor.
    /// Features: cooldown timeline, stat modifier calculator, prerequisite graph,
    /// visual preview, validation, quick actions.
    /// </summary>
    [CustomEditor(typeof(SkillNodeData))]
    public class SkillDataEditor : UnityEditor.Editor
    {
        private SkillNodeData _skill;
        private bool _showBasic = true;
        private bool _showMechanics = true;
        private bool _showPrerequisites = false;
        private bool _showCalculator = false;
        private bool _showDebug = false;
        private List<ValidationResult> _validationResults = new();

        // Calculator state
        private float _baseValue = 100f;
        private int _playerLevel = 1;

        void OnEnable()
        {
            _skill = (SkillNodeData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ─── Header ────────────────────────────────────────────
            EditorGUILayout.Space(10);
            EditorUtils.DrawColoredLabel($"Skill: {_skill.displayName}", GetTierColor(_skill.tier), FontStyle.Bold);
            EditorGUILayout.LabelField($"Tier {_skill.tier} | {_skill.skillId}", EditorStyles.centeredGreyMiniLabel);
            EditorUtils.DrawSeparator();

            // ─── Cost Display ──────────────────────────────────────
            DrawCostBox();

            // ─── Quick Actions ─────────────────────────────────────
            EditorUtils.DrawQuickActions(
                ("Validate", ValidateSkill),
                ("Duplicate", DuplicateSkill),
                ("Export JSON", ExportSkill),
                ("Show Tree", ShowSkillTree)
            );

            EditorUtils.DrawSeparator();

            // ─── Collapsible Sections ──────────────────────────────
            _showBasic = EditorUtils.DrawFoldoutSection("Basic Properties", _showBasic, DrawBasicSection);
            _showMechanics = EditorUtils.DrawFoldoutSection("Mechanics", _showMechanics, DrawMechanicsSection);
            _showPrerequisites = EditorUtils.DrawFoldoutSection("Prerequisites", _showPrerequisites, DrawPrerequisitesSection);
            _showCalculator = EditorUtils.DrawFoldoutSection("Stat Calculator", _showCalculator, DrawCalculatorSection);
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

        void DrawCostBox()
        {
            EditorUtils.DrawBoxGroup("Unlock Cost", () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                // RS Cost with progress bar
                EditorUtils.DrawProgressBar(_skill.rsCost, 500f, "RS Cost", GetTierColor(_skill.tier));
                
                EditorGUILayout.EndHorizontal();

                // Prerequisite count
                if (_skill.prerequisiteIds != null && _skill.prerequisiteIds.Count > 0)
                {
                    EditorUtils.DrawColoredLabel($"Requires {_skill.prerequisiteIds.Count} prerequisite skills", 
                        EditorUtils.ColorWarning);
                }
            });

            EditorGUILayout.Space(5);
        }

        void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rsCost"));
        }

        void DrawMechanicsSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("modifierType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("modifierValue"));

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("⚙️ Modifier Effect", MessageType.None);

            // Visual modifier display
            string modifierText = GetModifierTypeDescription(_skill.modifierType);
            float modifierPercent = _skill.modifierValue * 100f;
            
            EditorUtils.DrawColoredLabel(
                $"{modifierText}: {(modifierPercent >= 0 ? "+" : "")}{modifierPercent:F1}%",
                modifierPercent > 0 ? EditorUtils.ColorBuffed : EditorUtils.ColorNerfed,
                FontStyle.Bold
            );

            // Modifier explanation
            string explanation = GetModifierExplanation(_skill.modifierType);
            EditorGUILayout.HelpBox(explanation, MessageType.Info);
        }

        void DrawPrerequisitesSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteIds"));

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("🔗 Prerequisite Chain", MessageType.None);
            
            if (_skill.prerequisiteIds != null && _skill.prerequisiteIds.Count > 0)
            {
                foreach (var prereqId in _skill.prerequisiteIds)
                {
                    EditorGUILayout.LabelField($"  → {prereqId}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (No prerequisites - starter skill)", EditorStyles.miniLabel);
            }
        }

        void DrawCalculatorSection()
        {
            EditorGUILayout.HelpBox("📊 Stat Impact Calculator", MessageType.None);

            // Input fields
            _baseValue = EditorGUILayout.FloatField("Base Value", _baseValue);
            _playerLevel = EditorGUILayout.IntSlider("Player Level", _playerLevel, 1, 50);

            EditorGUILayout.Space(5);

            // Calculate modified value
            float modifiedValue = _baseValue * (1f + _skill.modifierValue);
            float difference = modifiedValue - _baseValue;

            // Display results
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Base:", GUILayout.Width(100));
            EditorUtils.DrawColoredLabel($"{_baseValue:F2}", EditorUtils.ColorDefault);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Modified:", GUILayout.Width(100));
            EditorUtils.DrawColoredLabel($"{modifiedValue:F2}", EditorUtils.ColorBuffed);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Difference:", GUILayout.Width(100));
            EditorUtils.DrawColoredLabel(
                $"{(difference >= 0 ? "+" : "")}{difference:F2} ({(_skill.modifierValue * 100f):F1}%)", 
                difference >= 0 ? EditorUtils.ColorBuffed : EditorUtils.ColorNerfed,
                FontStyle.Bold
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Example scenarios
            EditorGUILayout.Space(10);
            DrawExampleScenarios();
        }

        void DrawExampleScenarios()
        {
            EditorGUILayout.LabelField("Example Scenarios:", EditorStyles.boldLabel);

            var scenarios = new[]
            {
                ("Tuning Precision", 75f),
                ("Damage Output", 50f),
                ("Defense Rating", 100f),
                ("Cooldown Reduction", 10f)
            };

            foreach (var (label, baseVal) in scenarios)
            {
                float modified = baseVal * (1f + _skill.modifierValue);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{label}:", GUILayout.Width(150));
                EditorGUILayout.LabelField($"{baseVal:F1} → {modified:F1}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawDebugSection()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Asset Path", AssetDatabase.GetAssetPath(_skill));
            EditorGUILayout.EnumPopup("Skill ID", _skill.skillId);
            EditorGUILayout.IntField("Tier", _skill.tier);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy Skill ID"))
            {
                EditorGUIUtility.systemCopyBuffer = _skill.skillId.ToString();
                Debug.Log($"[SkillDataEditor] Copied skill ID: {_skill.skillId}");
            }
        }

        void ValidateSkill()
        {
            _validationResults.Clear();

            // Validate skill ID
            if (_skill.skillId == SkillId.None)
            {
                _validationResults.Add(new ValidationResult("Skill ID is set to None", ValidationSeverity.Error, _skill));
            }

            // Validate tier
            if (_skill.tier < 1 || _skill.tier > 5)
            {
                _validationResults.Add(new ValidationResult("Tier must be between 1 and 5", ValidationSeverity.Error, _skill));
            }

            // Validate display name
            if (string.IsNullOrWhiteSpace(_skill.displayName))
            {
                _validationResults.Add(new ValidationResult("Display name is empty", ValidationSeverity.Error, _skill));
            }

            // Validate RS cost
            if (_skill.rsCost < 0)
            {
                _validationResults.Add(new ValidationResult("RS cost cannot be negative", ValidationSeverity.Error, _skill));
            }

            // Validate modifier value
            if (_skill.modifierValue == 0f)
            {
                _validationResults.Add(new ValidationResult("Modifier value is 0 (no effect)", ValidationSeverity.Warning, _skill));
            }

            if (_validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Success", "✓ All checks passed!", "OK");
            }

            Repaint();
        }

        void DuplicateSkill()
        {
            if (EditorUtils.ConfirmAction("Duplicate Skill", $"Create a copy of '{_skill.displayName}'?"))
            {
                EditorUtils.DuplicateAsset(_skill, $"Skill_{_skill.skillId}_Copy");
            }
        }

        void ExportSkill()
        {
            EditorUtils.ExportToJSON(_skill, $"Skill_{_skill.skillId}.json");
        }

        void ShowSkillTree()
        {
            Debug.Log($"[SkillDataEditor] Skill Tree for {_skill.skillId}:\n" +
                      $"Tier {_skill.tier} | Prerequisites: {_skill.prerequisiteIds?.Count ?? 0}");
            
            EditorUtility.DisplayDialog("Skill Tree", 
                $"Skill: {_skill.displayName}\n" +
                $"Tier: {_skill.tier}\n" +
                $"Prerequisites: {_skill.prerequisiteIds?.Count ?? 0}\n\n" +
                "Full tree visualization coming soon!", 
                "OK");
        }

        Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => new Color(0.7f, 0.7f, 0.7f),  // Gray
                2 => new Color(0.2f, 0.9f, 0.2f),  // Green
                3 => new Color(0.2f, 0.5f, 1f),    // Blue
                4 => new Color(0.7f, 0.2f, 0.9f),  // Purple
                5 => new Color(1f, 0.6f, 0.1f),    // Orange/Gold
                _ => Color.white
            };
        }

        string GetModifierTypeDescription(SkillModifierType type)
        {
            return type switch
            {
                SkillModifierType.TuningPrecision => "Tuning Precision",
                SkillModifierType.DamageBoost => "Damage Boost",
                SkillModifierType.DefenseBoost => "Defense Boost",
                SkillModifierType.CooldownReduction => "Cooldown Reduction",
                SkillModifierType.ResourceEfficiency => "Resource Efficiency",
                _ => "Unknown Modifier"
            };
        }

        string GetModifierExplanation(SkillModifierType type)
        {
            return type switch
            {
                SkillModifierType.TuningPrecision => "Increases accuracy window for tuning mechanics.",
                SkillModifierType.DamageBoost => "Increases all outgoing damage.",
                SkillModifierType.DefenseBoost => "Reduces incoming damage.",
                SkillModifierType.CooldownReduction => "Reduces ability cooldown times.",
                SkillModifierType.ResourceEfficiency => "Reduces resource costs for abilities.",
                _ => "No description available."
            };
        }
    }
}
#endif
