using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Dialogue Tree — branching conversation system that supports:
    ///   - Multi-node dialogue with player choices
    ///   - Condition-gated branches (RS threshold, quest completion, trust level)
    ///   - Consequence callbacks (RS rewards, trust changes, quest activation)
    ///   - Choice persistence across saves
    ///
    /// Each NPC can have multiple DialogueTree assets. The DialogueManager
    /// selects the appropriate tree based on context and conditions.
    /// </summary>
    public class DialogueTreeRunner : MonoBehaviour
    {
        public static DialogueTreeRunner Instance { get; private set; }

        public event Action<string, string> OnDialogueStarted;  // treeId, speakerName
        public event Action<string> OnDialogueEnded;             // treeId
        public event Action<string, int> OnChoiceMade;           // nodeId, choiceIndex

        DialogueTree _activeTree;
        DialogueNode _currentNode;
        bool _isRunning;

        readonly HashSet<string> _visitedNodes = new();
        readonly Dictionary<string, int> _choiceHistory = new();

        public bool IsRunning => _isRunning;
        public DialogueNode CurrentNode => _currentNode;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Start a branching dialogue tree.
        /// </summary>
        public void StartTree(DialogueTree tree)
        {
            if (tree == null || tree.nodes.Count == 0) return;
            if (_isRunning) EndTree();

            _activeTree = tree;
            _isRunning = true;
            OnDialogueStarted?.Invoke(tree.treeId, tree.speakerName);

            // Find the root node (first node or explicit rootNodeId)
            var root = string.IsNullOrEmpty(tree.rootNodeId)
                ? tree.nodes[0]
                : tree.nodes.Find(n => n.nodeId == tree.rootNodeId);

            if (root != null)
                EnterNode(root);
            else
                EndTree();
        }

        /// <summary>
        /// Player selects a dialogue choice (0-indexed).
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!_isRunning || _currentNode == null) return;
            if (_currentNode.choices == null || choiceIndex >= _currentNode.choices.Count) return;

            var choice = _currentNode.choices[choiceIndex];

            // Record in history
            _choiceHistory[_currentNode.nodeId] = choiceIndex;
            OnChoiceMade?.Invoke(_currentNode.nodeId, choiceIndex);

            // Execute consequences
            ExecuteConsequences(choice.consequences);

            // Navigate to next node
            if (string.IsNullOrEmpty(choice.nextNodeId))
            {
                EndTree();
            }
            else
            {
                var nextNode = _activeTree.nodes.Find(n => n.nodeId == choice.nextNodeId);
                if (nextNode != null)
                    EnterNode(nextNode);
                else
                    EndTree();
            }
        }

        /// <summary>
        /// Advance past a non-choice node (just dialogue text, click to continue).
        /// </summary>
        public void Continue()
        {
            if (!_isRunning || _currentNode == null) return;

            // If node has choices, don't auto-advance
            if (_currentNode.choices != null && _currentNode.choices.Count > 0) return;

            // Follow default next
            if (!string.IsNullOrEmpty(_currentNode.defaultNextNodeId))
            {
                var next = _activeTree.nodes.Find(n => n.nodeId == _currentNode.defaultNextNodeId);
                if (next != null) { EnterNode(next); return; }
            }

            EndTree();
        }

        /// <summary>
        /// Force-end the current dialogue.
        /// </summary>
        public void EndTree()
        {
            if (!_isRunning) return;
            _isRunning = false;
            string treeId = _activeTree?.treeId ?? "";

            UI.UIManager.Instance?.HideDialogue();

            _activeTree = null;
            _currentNode = null;
            OnDialogueEnded?.Invoke(treeId);
        }

        /// <summary>
        /// Check if a node was visited in any previous dialogue run.
        /// </summary>
        public bool WasNodeVisited(string nodeId) => _visitedNodes.Contains(nodeId);

        /// <summary>
        /// Get the choice the player made at a node (-1 if not visited).
        /// </summary>
        public int GetPreviousChoice(string nodeId) =>
            _choiceHistory.TryGetValue(nodeId, out int c) ? c : -1;

        /// <summary>
        /// Get save data for persistence.
        /// </summary>
        public DialogueTreeSaveData GetSaveData()
        {
            var choices = new List<DialogueChoiceRecord>();
            foreach (var kvp in _choiceHistory)
                choices.Add(new DialogueChoiceRecord { nodeId = kvp.Key, choiceIndex = kvp.Value });

            var visited = new List<string>(_visitedNodes);

            return new DialogueTreeSaveData
            {
                choiceRecords = choices,
                visitedNodeIds = visited
            };
        }

        /// <summary>
        /// Restore from save.
        /// </summary>
        public void RestoreFromSave(DialogueTreeSaveData data)
        {
            if (data == null) return;
            _choiceHistory.Clear();
            _visitedNodes.Clear();

            if (data.choiceRecords != null)
                foreach (var r in data.choiceRecords)
                    _choiceHistory[r.nodeId] = r.choiceIndex;

            if (data.visitedNodeIds != null)
                foreach (var id in data.visitedNodeIds)
                    _visitedNodes.Add(id);
        }

        // ─── Internal ────────────────────────────────

        void EnterNode(DialogueNode node)
        {
            _currentNode = node;
            _visitedNodes.Add(node.nodeId);

            // Check conditions — skip to fallback if not met
            if (!EvaluateConditions(node.conditions))
            {
                if (!string.IsNullOrEmpty(node.fallbackNodeId))
                {
                    var fallback = _activeTree.nodes.Find(n => n.nodeId == node.fallbackNodeId);
                    if (fallback != null) { EnterNode(fallback); return; }
                }
                EndTree();
                return;
            }

            // Display line
            UI.UIManager.Instance?.ShowDialogue(node.speaker, node.text);

            // Execute enter consequences
            ExecuteConsequences(node.onEnterConsequences);

            // If no choices and auto-advance, schedule continue
            if ((node.choices == null || node.choices.Count == 0) && node.autoAdvanceDelay > 0)
            {
                CancelInvoke(nameof(AutoContinue));
                Invoke(nameof(AutoContinue), node.autoAdvanceDelay);
            }

            Debug.Log($"[DialogueTree] {node.speaker}: {node.text}");
        }

        void AutoContinue() => Continue();

        bool EvaluateConditions(List<DialogueCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var cond in conditions)
            {
                switch (cond.type)
                {
                    case ConditionType.MinRS:
                        if (AetherFieldManager.Instance == null) break;
                        if (AetherFieldManager.Instance.ResonanceScore < cond.floatValue)
                            return false;
                        break;

                    case ConditionType.QuestComplete:
                        if (QuestManager.Instance == null) break;
                        if (!QuestManager.Instance.IsQuestComplete(cond.stringValue))
                            return false;
                        break;

                    case ConditionType.QuestActive:
                        if (QuestManager.Instance == null) break;
                        var state = QuestManager.Instance.GetQuestState(cond.stringValue);
                        if (state.status != QuestStatus.Active)
                            return false;
                        break;

                    case ConditionType.MinTrust:
                        if (CassianNPCController.Instance == null) break;
                        if (CassianNPCController.Instance.TrustLevel < cond.floatValue)
                            return false;
                        break;

                    case ConditionType.NodeVisited:
                        if (!_visitedNodes.Contains(cond.stringValue))
                            return false;
                        break;

                    case ConditionType.NodeNotVisited:
                        if (_visitedNodes.Contains(cond.stringValue))
                            return false;
                        break;

                    case ConditionType.ChoiceWas:
                        if (!_choiceHistory.TryGetValue(cond.stringValue, out int prev) || prev != cond.intValue)
                            return false;
                        break;
                }
            }
            return true;
        }

        void ExecuteConsequences(List<DialogueConsequence> consequences)
        {
            if (consequences == null) return;

            foreach (var c in consequences)
            {
                switch (c.type)
                {
                    case ConsequenceType.AddRS:
                        GameLoopController.Instance?.QueueRSReward(c.floatValue, "dialogue");
                        break;

                    case ConsequenceType.AddTrust:
                        CassianNPCController.Instance?.ModifyTrust(c.floatValue);
                        break;

                    case ConsequenceType.ActivateQuest:
                        QuestManager.Instance?.ActivateQuest(c.stringValue);
                        break;

                    case ConsequenceType.PlayVFX:
                        VFXController.Instance?.PlayDiscoveryBurst(
                            PlayerInputHandler.Instance != null
                                ? PlayerInputHandler.Instance.transform.position
                                : Vector3.zero);
                        break;

                    case ConsequenceType.PlayDialogue:
                        DialogueManager.Instance?.PlayContextDialogue(c.stringValue);
                        break;

                    case ConsequenceType.SetFlag:
                        _visitedNodes.Add(c.stringValue); // Reuse visited set as flag store
                        break;
                }
            }
        }
    }

    // ─── Data Structures ─────────────────────────

    [Serializable]
    public class DialogueTree
    {
        public string treeId;
        public string speakerName;
        public string rootNodeId;
        public List<DialogueNode> nodes = new();
    }

    [Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public string speaker;
        public string text;
        public string defaultNextNodeId;
        public string fallbackNodeId;
        public float autoAdvanceDelay;
        public List<DialogueCondition> conditions;
        public List<DialogueConsequence> onEnterConsequences;
        public List<DialogueChoice> choices;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextNodeId;
        public List<DialogueCondition> conditions;
        public List<DialogueConsequence> consequences;
    }

    [Serializable]
    public class DialogueCondition
    {
        public ConditionType type;
        public float floatValue;
        public int intValue;
        public string stringValue;
    }

    [Serializable]
    public class DialogueConsequence
    {
        public ConsequenceType type;
        public float floatValue;
        public string stringValue;
    }

    public enum ConditionType
    {
        MinRS = 0,
        QuestComplete = 1,
        QuestActive = 2,
        MinTrust = 3,
        NodeVisited = 4,
        NodeNotVisited = 5,
        ChoiceWas = 6
    }

    public enum ConsequenceType
    {
        AddRS = 0,
        AddTrust = 1,
        ActivateQuest = 2,
        PlayVFX = 3,
        PlayDialogue = 4,
        SetFlag = 5
    }

    [Serializable]
    public class DialogueTreeSaveData
    {
        public List<DialogueChoiceRecord> choiceRecords = new();
        public List<string> visitedNodeIds = new();
    }

    [Serializable]
    public class DialogueChoiceRecord
    {
        public string nodeId;
        public int choiceIndex;
    }
}
