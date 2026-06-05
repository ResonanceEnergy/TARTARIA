using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core.Enums;
using Tartaria.Core.Validation;
using Tartaria.Localization;

namespace Tartaria.Data
{
    /// <summary>
    /// DialogueChoice — represents a single choice option in a dialogue node.
    /// </summary>
    [Serializable]
    public struct DialogueChoice
    {
        [Tooltip("Text displayed to player for this choice")]
        public string choiceText;

        [Tooltip("Localization key for this choice")]
        public LocalizationKey choiceKey;

        [Tooltip("ID of the next node to transition to when this choice is selected")]
        public string nextNodeId;

        [Tooltip("Optional condition that must be met for this choice to be available")]
        public DialogueCondition condition;

        [Tooltip("If true, selecting this choice ends the conversation")]
        public bool endsConversation;
    }

    /// <summary>
    /// DialogueCondition — requirements that must be met for a choice/node to be available.
    /// Supports quest states, player level, and stat checks.
    ///
    /// Evaluation delegate pattern:
    ///   - Data assembly cannot reference Integration/Gameplay assemblies
    ///   - Static EvaluateDelegate is set by DialogueConditionEvaluator (Integration)
    ///   - Allows condition checks to access QuestManager/PlayerProgression without assembly violations
    /// </summary>
    [Serializable]
    public struct DialogueCondition
    {
        /// <summary>
        /// Static delegate for condition evaluation. Set by DialogueConditionEvaluator in Integration assembly.
        /// </summary>
        public static System.Func<DialogueCondition, bool> EvaluateDelegate;

        public DialogueConditionType type;

        [Tooltip("Quest ID (for QuestComplete/QuestActive checks)")]
        public string questId;

        [Tooltip("Minimum player level required")]
        public int minPlayerLevel;

        [Tooltip("Stat check type (strength/vitality/etc)")]
        public DialogueStatType statType;

        [Tooltip("Minimum stat value required")]
        public int minStatValue;

        [Tooltip("Custom condition key for special checks")]
        public string customConditionKey;

        /// <summary>
        /// Evaluates if this condition is currently met.
        /// Uses delegate if set (Integration assembly), otherwise returns true (fallback).
        /// </summary>
        public bool Evaluate()
        {
            // Use delegate if registered (normal case)
            if (EvaluateDelegate != null)
            {
                return EvaluateDelegate(this);
            }

            // Fallback: if delegate not set (editor mode or early initialization), return true to avoid blocking
            Debug.LogWarning($"[DialogueCondition] EvaluateDelegate not set, returning true for {type} (DialogueConditionEvaluator may not be initialized)");
            return true;
        }
    }

    public enum DialogueConditionType
    {
        None,
        QuestComplete,
        QuestActive,
        MinPlayerLevel,
        StatCheck,
        Custom
    }

    /// <summary>
    /// DialogueNodeData — ScriptableObject representing a single node in a dialogue tree.
    /// Contains speaker, text, choices, and conditions for branching conversations.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Tartaria/Dialogue/Node", order = 300)]
    public class DialogueNodeData : ScriptableObject, ILocalizable
    {
        [Header("Node Identity")]
        [Tooltip("Unique identifier for this node within the tree")]
        public string nodeId;

        [Header("Localization")]
        [Tooltip("Localization key for speaker name (dialogue.speaker.{speakerId})")]
        public LocalizationKey speakerKey;

        [Tooltip("Localization key for dialogue text (dialogue.node.{nodeId})")]
        public LocalizationKey textKey;

        [Header("Legacy Dialogue Content (Fallback)")]
        [Tooltip("Name of the speaker (used if speakerKey is empty)")]
        public string speakerName;

        [TextArea(3, 10)]
        [Tooltip("Dialogue text displayed (used if textKey is empty)")]
        public string dialogueText;

        [Header("Choices")]
        [Tooltip("Player response options (empty for non-branching nodes)")]
        public DialogueChoice[] choices;

        [Header("Conditions")]
        [Tooltip("Condition that must be met to display this node")]
        public DialogueCondition displayCondition;

        [Header("Flow Control")]
        [Tooltip("If true, conversation ends after this node (no choices needed)")]
        public bool endsConversation;

        [Tooltip("Optional auto-advance to next node ID after delay (for cutscenes)")]
        public string autoAdvanceToNode;

        [Tooltip("Delay in seconds before auto-advancing (0 = no auto-advance)")]
        public float autoAdvanceDelay;

        [Header("Audio")]
        [Tooltip("Optional voice line ID for this node")]
        public string voiceLineId;

        [Header("Events")]
        [Tooltip("Optional quest to activate when this node is displayed")]
        public string activateQuestId;

        private void OnValidate()
        {
            // Auto-generate localization keys from nodeId
            if (!string.IsNullOrWhiteSpace(nodeId))
            {
                if (!textKey.IsValid)
                {
                    textKey = new LocalizationKey("dialogue.node", nodeId);
                }
                if (!speakerKey.IsValid && !string.IsNullOrWhiteSpace(speakerName))
                {
                    string speakerId = speakerName.Replace(" ", "_").ToLower();
                    speakerKey = new LocalizationKey("dialogue.speaker", speakerId);
                }
            }
        }

        #region ILocalizable Implementation

        public LocalizationKey[] GetLocalizationKeys()
        {
            var keys = new List<LocalizationKey> { speakerKey, textKey };

            // Add choice keys
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    if (choice.choiceKey.IsValid)
                        keys.Add(choice.choiceKey);
                }
            }

            return keys.ToArray();
        }

        public string GetFallbackText(LocalizationKey key)
        {
            if (key == speakerKey)
                return speakerName;
            if (key == textKey)
                return dialogueText;
            return string.Empty;
        }

        public string GetLocalizedSpeaker()
        {
            if (speakerKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(speakerKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return speakerName;
        }

        public string GetLocalizedText()
        {
            if (textKey.IsValid && LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(textKey);
                if (!localized.StartsWith("[MISSING:"))
                    return localized;
            }
            return dialogueText;
        }

        #endregion

        [Tooltip("Optional quest to complete when this node is displayed")]
        public string completeQuestId;

        [Header("NPC State Tracking")]
        [Tooltip("Set NPC relationship value when node is reached")]
        public int setRelationshipValue = -1; // -1 = no change

        [Tooltip("Modify NPC relationship by delta when node is reached")]
        public int relationshipDelta;

        /// <summary>
        /// Checks if this node's display condition is met.
        /// </summary>
        public bool CanDisplay()
        {
            return displayCondition.Evaluate();
        }

        /// <summary>
        /// Gets available choices (filters out choices whose conditions are not met).
        /// </summary>
        public List<DialogueChoice> GetAvailableChoices()
        {
            var available = new List<DialogueChoice>();

            if (choices == null || choices.Length == 0)
                return available;

            foreach (var choice in choices)
            {
                if (choice.condition.Evaluate())
                    available.Add(choice);
            }

            return available;
        }

        /// <summary>
        /// Executes node events (quest activation/completion, relationship changes).
        /// Called by DialoguePlayer when node is displayed.
        ///
        /// Delegate pattern: Data assembly cannot reference Integration assembly.
        /// DialogueNodeExecutor (Integration) sets ExecuteDelegate to bridge this gap.
        /// </summary>
        public static System.Action<DialogueNodeData> ExecuteDelegate;

        public void ExecuteNodeEvents()
        {
            // Use delegate if registered (normal case)
            if (ExecuteDelegate != null)
            {
                ExecuteDelegate(this);
                return;
            }

            // Fallback: log warnings if delegate not set
            Debug.LogWarning($"[DialogueNode] ExecuteDelegate not set (DialogueNodeExecutor may not be initialized)");

            if (!string.IsNullOrEmpty(activateQuestId))
            {
                Debug.LogWarning($"[DialogueNode] Cannot activate quest: {activateQuestId} - ExecuteDelegate not registered");
            }

            if (!string.IsNullOrEmpty(completeQuestId))
            {
                Debug.LogWarning($"[DialogueNode] Cannot complete quest: {completeQuestId} - ExecuteDelegate not registered");
            }
        }

        /// <summary>
        /// Comprehensive validation for dialogue node data integrity.
        /// </summary>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // Node ID validation
            DataValidator.AddIfNotNull(results, DataValidator.ValidateID(nodeId, "nodeId"));
            DataValidator.AddIfNotNull(results, DataValidator.ValidateIDFormat(nodeId, "nodeId"));

            // Speaker name validation
            if (string.IsNullOrWhiteSpace(speakerName))
            {
                results.Add(ValidationResult.Warning(
                    "speakerName is empty",
                    "Speaker names help identify who is talking",
                    "Assign a speaker name (e.g., 'Anastasia', 'Player')"
                ));
            }

            // Dialogue text validation
            if (string.IsNullOrWhiteSpace(dialogueText))
            {
                results.Add(ValidationResult.Error(
                    "dialogueText is empty",
                    "Dialogue nodes must have text to display",
                    "Add dialogue text content"
                ));
            }

            // Flow control validation
            if (!endsConversation && (choices == null || choices.Length == 0) && string.IsNullOrEmpty(autoAdvanceToNode))
            {
                results.Add(ValidationResult.Error(
                    "Node has no exit path",
                    "Node must either end conversation, have choices, or auto-advance",
                    "Set endsConversation=true, add choices, or set autoAdvanceToNode"
                ));
            }

            // Choices validation
            if (choices != null && choices.Length > 0)
            {
                for (int i = 0; i < choices.Length; i++)
                {
                    var choice = choices[i];

                    if (string.IsNullOrWhiteSpace(choice.choiceText))
                    {
                        results.Add(ValidationResult.Error(
                            $"choices[{i}].choiceText is empty",
                            "Empty choice text creates blank buttons",
                            $"Add text for choice {i}"
                        ));
                    }

                    if (!choice.endsConversation && string.IsNullOrWhiteSpace(choice.nextNodeId))
                    {
                        results.Add(ValidationResult.Error(
                            $"choices[{i}] has no exit path",
                            "Choice must either end conversation or link to next node",
                            $"Set nextNodeId or endsConversation=true for choice {i}"
                        ));
                    }

                    // Check for self-reference (infinite loop)
                    if (choice.nextNodeId == nodeId)
                    {
                        results.Add(ValidationResult.Error(
                            $"choices[{i}].nextNodeId references self",
                            "Self-referencing choices cause infinite loops",
                            "Link to a different node or end conversation"
                        ));
                    }
                }
            }

            // Auto-advance validation
            if (!string.IsNullOrEmpty(autoAdvanceToNode))
            {
                if (autoAdvanceToNode == nodeId)
                {
                    results.Add(ValidationResult.Error(
                        "autoAdvanceToNode references self",
                        "Self-referencing auto-advance causes infinite loops",
                        "Set autoAdvanceToNode to a different node ID"
                    ));
                }

                if (autoAdvanceDelay < 0)
                {
                    results.Add(ValidationResult.Error(
                        $"autoAdvanceDelay is negative: {autoAdvanceDelay}",
                        "Negative delays are invalid",
                        "Set autoAdvanceDelay to 0 or positive value"
                    ));
                }

                if (autoAdvanceDelay == 0)
                {
                    results.Add(ValidationResult.Warning(
                        "autoAdvanceDelay is 0 with auto-advance enabled",
                        "Instant auto-advance may skip text display",
                        "Consider setting a small delay (e.g., 2 seconds)"
                    ));
                }
            }

            // Relationship value validation
            if (setRelationshipValue > 100)
            {
                results.Add(ValidationResult.Warning(
                    $"setRelationshipValue is very high: {setRelationshipValue}",
                    "Relationship values typically range 0-100",
                    "Verify this value is intentional"
                ));
            }

            return results;
        }
    }

    /// <summary>
    /// DialogueConditionHandler — static helper for custom dialogue condition evaluation.
    /// Extend this class to add game-specific condition checks.
    /// </summary>
    public static class DialogueConditionHandler
    {
        static readonly Dictionary<string, Func<bool>> _customConditions = new();

        /// <summary>
        /// Register a custom condition evaluator.
        /// </summary>
        public static void RegisterCondition(string key, Func<bool> evaluator)
        {
            _customConditions[key] = evaluator;
        }

        /// <summary>
        /// Evaluate a custom condition by key.
        /// </summary>
        public static bool EvaluateCustom(string key)
        {
            if (string.IsNullOrEmpty(key))
                return true;

            if (_customConditions.TryGetValue(key, out var evaluator))
                return evaluator.Invoke();

            Debug.LogWarning($"[DialogueCondition] Unknown custom condition: {key}");
            return false;
        }
    }
}
