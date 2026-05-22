#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Inspector for DialogueNodeData — Designer-friendly dialogue editor.
    /// Features: node graph visualization, character portraits, choice tree,
    /// condition preview, export to DOT format, validation, quick actions.
    /// </summary>
    [CustomEditor(typeof(DialogueNodeData))]
    public class DialogueDataEditor : UnityEditor.Editor
    {
        private DialogueNodeData _dialogue;
        private bool _showBasic = true;
        private bool _showContent = true;
        private bool _showChoices = true;
        private bool _showConditions = false;
        private bool _showDebug = false;
        private List<ValidationResult> _validationResults = new();

        void OnEnable()
        {
            _dialogue = (DialogueNodeData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ─── Header ────────────────────────────────────────────
            EditorGUILayout.Space(10);
            EditorUtils.DrawColoredLabel($"Dialogue Node: {_dialogue.nodeId}", Color.cyan, FontStyle.Bold);
            EditorGUILayout.LabelField($"Speaker: {_dialogue.speakerName}", EditorStyles.centeredGreyMiniLabel);
            EditorUtils.DrawSeparator();

            // ─── Character Portrait ────────────────────────────────
            DrawCharacterPortrait();

            // ─── Quick Actions ─────────────────────────────────────
            EditorUtils.DrawQuickActions(
                ("Validate", ValidateDialogue),
                ("Duplicate", DuplicateDialogue),
                ("Export DOT", ExportToDOT),
                ("Preview", PreviewDialogue)
            );

            EditorUtils.DrawSeparator();

            // ─── Node Graph Preview ────────────────────────────────
            DrawNodeGraphPreview();

            // ─── Collapsible Sections ──────────────────────────────
            _showBasic = EditorUtils.DrawFoldoutSection("Basic Properties", _showBasic, DrawBasicSection);
            _showContent = EditorUtils.DrawFoldoutSection("Dialogue Content", _showContent, DrawContentSection);
            _showChoices = EditorUtils.DrawFoldoutSection("Player Choices", _showChoices, DrawChoicesSection);
            _showConditions = EditorUtils.DrawFoldoutSection("Conditions", _showConditions, DrawConditionsSection);
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

        void DrawCharacterPortrait()
        {
            // Placeholder for character portrait display
            // In a real implementation, this would load portraits from Resources
            EditorUtils.DrawBoxGroup("Character", () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                // Portrait placeholder (128x128 box)
                var rect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.3f));
                
                var portraitStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                EditorGUI.LabelField(rect, _dialogue.speakerName.Substring(0, System.Math.Min(3, _dialogue.speakerName.Length)).ToUpper(), portraitStyle);

                // Speaker info
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(_dialogue.speakerName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Node: {_dialogue.nodeId}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            });

            EditorGUILayout.Space(5);
        }

        void DrawNodeGraphPreview()
        {
            EditorUtils.DrawBoxGroup("Node Flow Preview", () =>
            {
                EditorGUILayout.LabelField($"📍 Current Node: {_dialogue.nodeId}", EditorStyles.boldLabel);
                
                // Show choices count
                var choicesProp = serializedObject.FindProperty("choices");
                int choicesCount = choicesProp?.arraySize ?? 0;
                
                if (choicesCount > 0)
                {
                    EditorGUILayout.LabelField($"→ {choicesCount} player choice(s)", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("→ No choices (linear)", EditorStyles.miniLabel);
                }
            });

            EditorGUILayout.Space(5);
        }

        void DrawBasicSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speakerName"));
        }

        void DrawContentSection()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueText"));

            EditorGUILayout.Space(10);
            
            // Character counter
            var dialogueTextProp = serializedObject.FindProperty("dialogueText");
            if (dialogueTextProp != null)
            {
                int charCount = dialogueTextProp.stringValue?.Length ?? 0;
                Color countColor = charCount > 200 ? EditorUtils.ColorWarning : EditorUtils.ColorDefault;
                EditorUtils.DrawColoredLabel($"Character count: {charCount}", countColor);
                
                if (charCount > 200)
                {
                    EditorGUILayout.HelpBox("⚠️ Dialogue is quite long. Consider splitting into multiple nodes.", MessageType.Warning);
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voiceClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("emotionTag"));
        }

        void DrawChoicesSection()
        {
            var choicesProp = serializedObject.FindProperty("choices");
            
            if (choicesProp == null)
            {
                EditorGUILayout.HelpBox("Choices array not found", MessageType.Error);
                return;
            }

            EditorGUILayout.HelpBox("🔀 Branching Choices", MessageType.None);

            if (choicesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No choices defined (linear dialogue)", MessageType.Info);
                if (GUILayout.Button("Add First Choice"))
                {
                    choicesProp.InsertArrayElementAtIndex(0);
                }
            }
            else
            {
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    var choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Choice header
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Choice {i + 1}", EditorStyles.boldLabel, GUILayout.Width(80));
                    
                    var endsConvProp = choiceProp.FindPropertyRelative("endsConversation");
                    if (endsConvProp != null && endsConvProp.boolValue)
                    {
                        EditorUtils.DrawColoredLabel("[ENDS]", EditorUtils.ColorWarning);
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("✖", GUILayout.Width(25)))
                    {
                        choicesProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Choice properties
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("choiceText"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("nextNodeId"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("endsConversation"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("condition"));
                    EditorGUI.indentLevel--;

                    // Visual flow indicator
                    var nextNodeProp = choiceProp.FindPropertyRelative("nextNodeId");
                    if (nextNodeProp != null && !string.IsNullOrWhiteSpace(nextNodeProp.stringValue))
                    {
                        EditorGUILayout.LabelField($"  → {nextNodeProp.stringValue}", EditorStyles.miniLabel);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }

                if (GUILayout.Button("+ Add Choice"))
                {
                    choicesProp.InsertArrayElementAtIndex(choicesProp.arraySize);
                }
            }
        }

        void DrawConditionsSection()
        {
            EditorGUILayout.HelpBox("Conditions are defined per-choice in the Choices section above.", MessageType.Info);
            EditorGUILayout.LabelField("Available Condition Types:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• None - Always available");
            EditorGUILayout.LabelField("• QuestComplete - Requires quest completion");
            EditorGUILayout.LabelField("• QuestActive - Requires active quest");
            EditorGUILayout.LabelField("• MinPlayerLevel - Level requirement");
            EditorGUILayout.LabelField("• StatCheck - Stat requirement");
            EditorGUILayout.LabelField("• Custom - Custom condition handler");
        }

        void DrawDebugSection()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Asset Path", AssetDatabase.GetAssetPath(_dialogue));
            EditorGUILayout.TextField("Node ID", _dialogue.nodeId);
            
            var choicesProp = serializedObject.FindProperty("choices");
            EditorGUILayout.IntField("Choice Count", choicesProp?.arraySize ?? 0);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy Node ID"))
            {
                EditorGUIUtility.systemCopyBuffer = _dialogue.nodeId;
                Debug.Log($"[DialogueDataEditor] Copied node ID: {_dialogue.nodeId}");
            }
        }

        void ValidateDialogue()
        {
            _validationResults.Clear();

            // Validate node ID
            if (string.IsNullOrWhiteSpace(_dialogue.nodeId))
            {
                _validationResults.Add(new ValidationResult("Node ID is empty", ValidationSeverity.Error, _dialogue));
            }

            // Validate speaker name
            if (string.IsNullOrWhiteSpace(_dialogue.speakerName))
            {
                _validationResults.Add(new ValidationResult("Speaker name is empty", ValidationSeverity.Error, _dialogue));
            }

            // Validate dialogue text
            var dialogueTextProp = serializedObject.FindProperty("dialogueText");
            if (dialogueTextProp != null)
            {
                if (string.IsNullOrWhiteSpace(dialogueTextProp.stringValue))
                {
                    _validationResults.Add(new ValidationResult("Dialogue text is empty", ValidationSeverity.Error, _dialogue));
                }
                else if (dialogueTextProp.stringValue.Length > 300)
                {
                    _validationResults.Add(new ValidationResult("Dialogue text is very long (>300 chars)", ValidationSeverity.Warning, _dialogue));
                }
            }

            // Validate choices
            var choicesProp = serializedObject.FindProperty("choices");
            if (choicesProp != null)
            {
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    var choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    var choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
                    var nextNodeProp = choiceProp.FindPropertyRelative("nextNodeId");
                    var endsConvProp = choiceProp.FindPropertyRelative("endsConversation");

                    if (choiceTextProp != null && string.IsNullOrWhiteSpace(choiceTextProp.stringValue))
                    {
                        _validationResults.Add(new ValidationResult($"Choice {i + 1} has empty text", ValidationSeverity.Warning, _dialogue));
                    }

                    bool endsConv = endsConvProp != null && endsConvProp.boolValue;
                    bool hasNextNode = nextNodeProp != null && !string.IsNullOrWhiteSpace(nextNodeProp.stringValue);

                    if (!endsConv && !hasNextNode)
                    {
                        _validationResults.Add(new ValidationResult($"Choice {i + 1} has no next node and doesn't end conversation", ValidationSeverity.Error, _dialogue));
                    }
                }
            }

            if (_validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Success", "✓ All checks passed!", "OK");
            }

            Repaint();
        }

        void DuplicateDialogue()
        {
            if (EditorUtils.ConfirmAction("Duplicate Dialogue Node", $"Create a copy of node '{_dialogue.nodeId}'?"))
            {
                var clone = EditorUtils.DuplicateAsset(_dialogue, $"DialogueNode_{_dialogue.nodeId}_Copy");
                if (clone != null)
                {
                    clone.nodeId = $"{_dialogue.nodeId}_copy";
                }
            }
        }

        void ExportToDOT()
        {
            // Generate DOT graph format for Graphviz visualization
            var dot = new StringBuilder();
            dot.AppendLine("digraph DialogueTree {");
            dot.AppendLine("  rankdir=LR;");
            dot.AppendLine($"  \"{_dialogue.nodeId}\" [shape=box, style=filled, fillcolor=lightcyan, label=\"{_dialogue.nodeId}\\n{_dialogue.speakerName}\"];");

            var choicesProp = serializedObject.FindProperty("choices");
            if (choicesProp != null)
            {
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    var choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    var nextNodeProp = choiceProp.FindPropertyRelative("nextNodeId");
                    var choiceTextProp = choiceProp.FindPropertyRelative("choiceText");

                    if (nextNodeProp != null && !string.IsNullOrWhiteSpace(nextNodeProp.stringValue))
                    {
                        string label = choiceTextProp != null ? choiceTextProp.stringValue.Replace("\"", "'") : $"Choice {i + 1}";
                        if (label.Length > 30) label = label.Substring(0, 27) + "...";
                        
                        dot.AppendLine($"  \"{_dialogue.nodeId}\" -> \"{nextNodeProp.stringValue}\" [label=\"{label}\"];");
                    }
                }
            }

            dot.AppendLine("}");

            string dotGraph = dot.ToString();
            Debug.Log($"[DialogueDataEditor] DOT Graph:\n{dotGraph}");
            EditorGUIUtility.systemCopyBuffer = dotGraph;
            
            EditorUtility.DisplayDialog("Export to DOT", 
                "DOT graph copied to clipboard!\n\n" +
                "Visualize at:\n" +
                "• https://dreampuf.github.io/GraphvizOnline/\n" +
                "• https://edotor.net/\n\n" +
                "Graph also printed to console.", 
                "OK");
        }

        void PreviewDialogue()
        {
            var dialogueTextProp = serializedObject.FindProperty("dialogueText");
            string text = dialogueTextProp != null ? dialogueTextProp.stringValue : "(No text)";

            var preview = new StringBuilder();
            preview.AppendLine($"Speaker: {_dialogue.speakerName}");
            preview.AppendLine($"Node: {_dialogue.nodeId}");
            preview.AppendLine();
            preview.AppendLine("Text:");
            preview.AppendLine($"\"{text}\"");
            preview.AppendLine();

            var choicesProp = serializedObject.FindProperty("choices");
            if (choicesProp != null && choicesProp.arraySize > 0)
            {
                preview.AppendLine("Choices:");
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    var choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    var choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
                    var nextNodeProp = choiceProp.FindPropertyRelative("nextNodeId");

                    string choiceText = choiceTextProp != null ? choiceTextProp.stringValue : "(No text)";
                    string nextNode = nextNodeProp != null ? nextNodeProp.stringValue : "(None)";

                    preview.AppendLine($"  {i + 1}. \"{choiceText}\" → {nextNode}");
                }
            }

            Debug.Log($"[DialogueDataEditor] Preview:\n{preview}");
            EditorUtility.DisplayDialog("Dialogue Preview", preview.ToString(), "OK");
        }
    }
}
#endif
