using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// </summary>
    [Serializable]
    public struct DialogueCondition
    {
        public DialogueConditionType type;

        [Tooltip("Quest ID (for QuestComplete/QuestActive checks)")]
        public string questId;

        [Tooltip("Minimum player level required")]
        public int minPlayerLevel;

        [Tooltip("Stat check type (strength/vitality/etc)")]
        public StatType statType;

        [Tooltip("Minimum stat value required")]
        public int minStatValue;

        [Tooltip("Custom condition key for special checks")]
        public string customConditionKey;

        /// <summary>
        /// Evaluates if this condition is currently met.
        /// </summary>
        public bool Evaluate()
        {
            switch (type)
            {
                case DialogueConditionType.None:
                    return true;

                case DialogueConditionType.QuestComplete:
                    return Integration.QuestManager.Instance != null &&
                           Integration.QuestManager.Instance.IsQuestComplete(questId);

                case DialogueConditionType.QuestActive:
                    return Integration.QuestManager.Instance != null &&
                           Integration.QuestManager.Instance.IsQuestActive(questId);

                case DialogueConditionType.MinPlayerLevel:
                    return Gameplay.PlayerProgression.Instance != null &&
                           Gameplay.PlayerProgression.Instance.CurrentLevel >= minPlayerLevel;

                case DialogueConditionType.StatCheck:
                    // Placeholder: integrate with player stats when implemented
                    Debug.LogWarning($"[DialogueCondition] StatCheck not yet implemented for {statType}");
                    return true;

                case DialogueConditionType.Custom:
                    // Delegate to custom condition handler
                    return DialogueConditionHandler.EvaluateCustom(customConditionKey);

                default:
                    return true;
            }
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

    public enum StatType
    {
        Strength,
        Agility,
        Vitality,
        Resonance,
        Intelligence,
        Charisma
    }

    /// <summary>
    /// DialogueNodeData — ScriptableObject representing a single node in a dialogue tree.
    /// Contains speaker, text, choices, and conditions for branching conversations.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Tartaria/Dialogue/Node", order = 300)]
    public class DialogueNodeData : ScriptableObject
    {
        [Header("Node Identity")]
        [Tooltip("Unique identifier for this node within the tree")]
        public string nodeId;

        [Header("Dialogue Content")]
        [Tooltip("Name of the speaker (e.g., 'Anastasia', 'Cassian', 'Player')")]
        public string speakerName;

        [TextArea(3, 10)]
        [Tooltip("Dialogue text displayed when this node is reached")]
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
        /// </summary>
        public void ExecuteNodeEvents()
        {
            // Quest activation
            if (!string.IsNullOrEmpty(activateQuestId))
            {
                Integration.QuestManager.Instance?.ActivateQuest(activateQuestId);
                Debug.Log($"[DialogueNode] Activated quest: {activateQuestId}");
            }

            // Quest completion
            if (!string.IsNullOrEmpty(completeQuestId))
            {
                Integration.QuestManager.Instance?.CompleteQuest(completeQuestId);
                Debug.Log($"[DialogueNode] Completed quest: {completeQuestId}");
            }

            // Relationship tracking (placeholder for future NPC system)
            if (setRelationshipValue >= 0)
            {
                Debug.Log($"[DialogueNode] Set {speakerName} relationship to {setRelationshipValue}");
                // TODO: Wire to NPC relationship system when implemented
            }
            else if (relationshipDelta != 0)
            {
                Debug.Log($"[DialogueNode] Modified {speakerName} relationship by {relationshipDelta:+0;-0}");
                // TODO: Wire to NPC relationship system when implemented
            }
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
