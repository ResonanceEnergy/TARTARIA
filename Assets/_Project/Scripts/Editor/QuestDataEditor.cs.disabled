#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Data;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Inspector for QuestData — Designer-friendly quest editor.
    /// Features: objective tree, reward summary, dependency graph, validation,
    /// quick actions (duplicate, export, find references, test quest flow).
    /// </summary>
    [CustomEditor(typeof(QuestData))]
    public class QuestDataEditor : UnityEditor.Editor
    {
        private QuestData _quest;
        private bool _showBasic = true;
        private bool _showObjectives = true;
        private bool _showRewards = true;
        private bool _showPrerequisites = false;
        private bool _showFlow = false;
        private bool _showDebug = false;
        private List<ValidationResult> _validationResults = new();

        void OnEnable()
        {
            _quest = (QuestData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ─── Header ────────────────────────────────────────────
            EditorGUILayout.Space(10);
            string questType = _quest.isMainQuest ? "[MAIN QUEST]" : "[SIDE QUEST]";
            EditorUtils.DrawColoredLabel($"{questType} {_quest.displayName}", 
                _quest.isMainQuest ? EditorUtils.ColorBuffed : EditorUtils.ColorDefault, 
                FontStyle.Bold);
            
            EditorGUILayout.LabelField($"Moon {_quest.moonId} | {_quest.category}", EditorStyles.centeredGreyMiniLabel);
            EditorUtils.DrawSeparator();

            // ─── Quick Actions ─────────────────────────────────────
            EditorUtils.DrawQuickActions(
                ("Validate", ValidateQuest),
                ("Duplicate", DuplicateQuest),
                ("Export JSON", ExportQuest),
                ("Show Graph", ShowDependencyGraph)
            );

            EditorUtils.DrawSeparator();

            // ─── Reward Summary (Quick View) ───────────────────────
            DrawRewardSummaryBox();

            // ─── Collapsible Sections ──────────────────────────────
            _showBasic = EditorUtils.DrawFoldoutSection("Basic Properties", _showBasic, DrawBasicSection);
            _showObjectives = EditorUtils.DrawFoldoutSection("Objectives", _showObjectives, DrawObjectivesSection);
            _showRewards = EditorUtils.DrawFoldoutSection("Rewards", _showRewards, DrawRewardsSection);
            _showPrerequisites = EditorUtils.DrawFoldoutSection("Prerequisites", _showPrerequisites, DrawPrerequisitesSection);
            _showFlow = EditorUtils.DrawFoldoutSection("Quest Flow", _showFlow, DrawFlowSection);
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

        void DrawRewardSummaryBox()
        {
            EditorUtils.DrawBoxGroup("Reward Summary", () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                // RS Reward
                if (_quest.rsReward > 0)
                {
                    EditorUtils.DrawColoredLabel($"💎 RS: +{_quest.rsReward:F0}", EditorUtils.ColorBuffed);
                }

                GUILayout.FlexibleSpace();

                // XP Reward
                if (_quest.xpReward > 0)
                {
                    EditorUtils.DrawColoredLabel($"⭐ XP: +{_quest.xpReward}", EditorUtils.ColorBuffed);
                }

                GUILayout.FlexibleSpace();

                // Item Rewards
                if (_quest.itemRewards != null && _quest.itemRewards.Length > 0)
                {
                    EditorUtils.DrawColoredLabel($"🎁 Items: {_quest.itemRewards.Length}", EditorUtils.ColorBuffed);
                }

                EditorGUILayout.EndHorizontal();
            });

            EditorGUILayout.Space(5);
        }

        void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isMainQuest"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoActivate"));
        }

        void DrawObjectivesSection()
        {
            EditorGUILayout.HelpBox("📋 Objective Tree", MessageType.None);

            var objectivesProp = serializedObject.FindProperty("objectives");
            
            if (objectivesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No objectives defined", MessageType.Warning);
                if (GUILayout.Button("Add First Objective"))
                {
                    objectivesProp.InsertArrayElementAtIndex(0);
                }
            }
            else
            {
                for (int i = 0; i < objectivesProp.arraySize; i++)
                {
                    var objProp = objectivesProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Objective header with type icon
                    EditorGUILayout.BeginHorizontal();
                    var typeProp = objProp.FindPropertyRelative("type");
                    string typeIcon = GetObjectiveTypeIcon((QuestObjectiveType)typeProp.enumValueIndex);
                    EditorGUILayout.LabelField($"{typeIcon} Objective {i + 1}", EditorStyles.boldLabel, GUILayout.Width(150));
                    
                    if (GUILayout.Button("✖", GUILayout.Width(25)))
                    {
                        objectivesProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Objective properties
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("description"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("type"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("targetId"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("targetCount"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }

                if (GUILayout.Button("+ Add Objective"))
                {
                    objectivesProp.InsertArrayElementAtIndex(objectivesProp.arraySize);
                }
            }
        }

        void DrawRewardsSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rsReward"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xpReward"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRewards"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlockRewards"));
        }

        void DrawPrerequisitesSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteQuestIds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteRS"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteLevel"));

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("🔗 Prerequisite Chain", MessageType.None);
            
            if (_quest.prerequisiteQuestIds != null && _quest.prerequisiteQuestIds.Length > 0)
            {
                foreach (var prereqId in _quest.prerequisiteQuestIds)
                {
                    EditorGUILayout.LabelField($"  → {prereqId}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (No prerequisites)", EditorStyles.miniLabel);
            }
        }

        void DrawFlowSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoActivateOnPrerequisites"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canAbandon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isRepeatable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("followUpQuestIds"));

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("➡️ Quest Chain Flow", MessageType.None);
            
            if (_quest.followUpQuestIds != null && _quest.followUpQuestIds.Length > 0)
            {
                foreach (var followUpId in _quest.followUpQuestIds)
                {
                    EditorGUILayout.LabelField($"  → {followUpId}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (No follow-up quests)", EditorStyles.miniLabel);
            }
        }

        void DrawDebugSection()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Asset Path", AssetDatabase.GetAssetPath(_quest));
            EditorGUILayout.TextField("Quest ID", _quest.questId);
            EditorGUILayout.IntField("Objective Count", _quest.objectives?.Length ?? 0);
            EditorGUI.EndDisabledGroup();
        }

        void ValidateQuest()
        {
            _validationResults.Clear();

            // Validate quest ID
            if (string.IsNullOrWhiteSpace(_quest.questId))
            {
                _validationResults.Add(new ValidationResult("Quest ID is empty", ValidationSeverity.Error, _quest));
            }

            // Validate display name
            if (string.IsNullOrWhiteSpace(_quest.displayName))
            {
                _validationResults.Add(new ValidationResult("Display name is empty", ValidationSeverity.Error, _quest));
            }

            // Validate objectives
            if (_quest.objectives == null || _quest.objectives.Length == 0)
            {
                _validationResults.Add(new ValidationResult("No objectives defined", ValidationSeverity.Error, _quest));
            }
            else
            {
                for (int i = 0; i < _quest.objectives.Length; i++)
                {
                    var obj = _quest.objectives[i];
                    if (string.IsNullOrWhiteSpace(obj.description))
                    {
                        _validationResults.Add(new ValidationResult($"Objective {i + 1} has empty description", ValidationSeverity.Warning, _quest));
                    }
                    if (obj.targetCount < 1)
                    {
                        _validationResults.Add(new ValidationResult($"Objective {i + 1} has invalid target count", ValidationSeverity.Error, _quest));
                    }
                }
            }

            // Validate rewards
            if (_quest.rsReward <= 0 && _quest.xpReward <= 0 && (_quest.itemRewards == null || _quest.itemRewards.Length == 0))
            {
                _validationResults.Add(new ValidationResult("Quest has no rewards defined", ValidationSeverity.Warning, _quest));
            }

            // Validate moon ID
            if (_quest.moonId < 0 || _quest.moonId > 13)
            {
                _validationResults.Add(new ValidationResult("Invalid moon ID (must be 0-13)", ValidationSeverity.Error, _quest));
            }

            if (_validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Success", "✓ All checks passed!", "OK");
            }

            Repaint();
        }

        void DuplicateQuest()
        {
            if (EditorUtils.ConfirmAction("Duplicate Quest", $"Create a copy of '{_quest.displayName}'?"))
            {
                var clone = EditorUtils.DuplicateAsset(_quest, $"{_quest.questId}_copy");
                if (clone != null)
                {
                    clone.questId = $"{_quest.questId}_copy";
                }
            }
        }

        void ExportQuest()
        {
            EditorUtils.ExportToJSON(_quest, $"{_quest.questId}.json");
        }

        void ShowDependencyGraph()
        {
            // Generate DOT graph format for visualization
            var graph = new System.Text.StringBuilder();
            graph.AppendLine("digraph QuestGraph {");
            graph.AppendLine($"  \"{_quest.questId}\" [shape=box, style=filled, fillcolor=lightblue];");

            if (_quest.prerequisiteQuestIds != null)
            {
                foreach (var prereq in _quest.prerequisiteQuestIds)
                {
                    graph.AppendLine($"  \"{prereq}\" -> \"{_quest.questId}\";");
                }
            }

            if (_quest.followUpQuestIds != null)
            {
                foreach (var followUp in _quest.followUpQuestIds)
                {
                    graph.AppendLine($"  \"{_quest.questId}\" -> \"{followUp}\";");
                }
            }

            graph.AppendLine("}");

            Debug.Log($"[QuestDataEditor] Dependency Graph (DOT format):\n{graph}");
            EditorUtility.DisplayDialog("Dependency Graph", 
                $"DOT graph copied to console.\n\nVisualize at: graphviz.org or webgraphviz.com\n\n{graph}", 
                "OK");
        }

        string GetObjectiveTypeIcon(QuestObjectiveType type)
        {
            return type switch
            {
                QuestObjectiveType.DiscoverBuilding => "🏛️",
                QuestObjectiveType.RestoreBuilding => "🔧",
                QuestObjectiveType.DefeatEnemies => "⚔️",
                QuestObjectiveType.TalkToNPC => "💬",
                QuestObjectiveType.CollectItem => "📦",
                QuestObjectiveType.DefeatBoss => "👹",
                QuestObjectiveType.ReachRS => "💎",
                _ => "📌"
            };
        }
    }
}
#endif
