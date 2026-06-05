using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Data
{
    /// <summary>
    /// DialogueTreeAsset — ScriptableObject container for complete branching dialogue trees.
    /// Writers create dialogue trees as assets, which are then loaded and traversed by DialoguePlayer.
    ///
    /// Structure:
    /// - Tree contains a list of DialogueNodeData nodes
    /// - Tree specifies a root node ID (entry point)
    /// - Nodes reference each other by ID for branching
    ///
    /// Usage:
    /// 1. Create tree asset: Assets > Create > Tartaria > Dialogue > Tree
    /// 2. Add DialogueNodeData nodes to the tree
    /// 3. Set root node ID
    /// 4. Place in Resources/Dialogue/ for runtime loading
    ///
    /// Example trees:
    /// - Anastasia_Intro: First conversation with Anastasia in Moon 1
    /// - Cassian_Moon2: Cassian's trust-building arc in Moon 2
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Tartaria/Dialogue/Tree", order = 301)]
    public class DialogueTreeAsset : ScriptableObject
    {
        [Header("Tree Identity")]
        [Tooltip("Unique identifier for this dialogue tree")]
        public string treeId;

        [Tooltip("Human-readable description of this conversation")]
        [TextArea(2, 5)]
        public string description;

        [Header("Structure")]
        [Tooltip("ID of the root node (entry point for this conversation)")]
        public string rootNodeId;

        [Tooltip("All nodes in this dialogue tree")]
        public List<DialogueNodeData> nodes = new List<DialogueNodeData>();

        [Header("NPC Info")]
        [Tooltip("Primary NPC speaker for this tree (for relationship tracking)")]
        public string primarySpeaker;

        [Header("Metadata")]
        [Tooltip("Tags for organization (e.g., 'main_quest', 'companion', 'moon_2')")]
        public string[] tags;

        [Tooltip("If true, this tree can only be played once per save")]
        public bool oneTimeOnly;

        /// <summary>
        /// Gets a node by ID. Returns null if not found.
        /// </summary>
        public DialogueNodeData GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            foreach (var node in nodes)
            {
                if (node != null && node.nodeId == nodeId)
                    return node;
            }

            Debug.LogError($"[DialogueTree] Node not found: {nodeId} in tree {treeId}");
            return null;
        }

        /// <summary>
        /// Gets the root node (entry point of the conversation).
        /// </summary>
        public DialogueNodeData GetRootNode()
        {
            return GetNode(rootNodeId);
        }

        /// <summary>
        /// Validates the tree structure for broken references and missing nodes.
        /// Called in editor; logs warnings for authoring issues.
        /// </summary>
        public void ValidateTree()
        {
            if (string.IsNullOrEmpty(treeId))
            {
                Debug.LogError($"[DialogueTree] Tree {name} has no treeId assigned!", this);
            }

            if (string.IsNullOrEmpty(rootNodeId))
            {
                Debug.LogError($"[DialogueTree] Tree {treeId} has no rootNodeId assigned!", this);
            }
            else if (GetRootNode() == null)
            {
                Debug.LogError($"[DialogueTree] Tree {treeId} root node '{rootNodeId}' not found in nodes list!", this);
            }

            // Check for duplicate node IDs
            var nodeIds = new HashSet<string>();
            foreach (var node in nodes)
            {
                if (node == null) continue;

                if (string.IsNullOrEmpty(node.nodeId))
                {
                    Debug.LogWarning($"[DialogueTree] Tree {treeId} has a node with no nodeId!", this);
                    continue;
                }

                if (!nodeIds.Add(node.nodeId))
                {
                    Debug.LogError($"[DialogueTree] Tree {treeId} has duplicate node ID: {node.nodeId}", this);
                }
            }

            // Check for broken choice references
            foreach (var node in nodes)
            {
                if (node == null || node.choices == null) continue;

                foreach (var choice in node.choices)
                {
                    if (choice.endsConversation) continue;

                    if (string.IsNullOrEmpty(choice.nextNodeId))
                    {
                        Debug.LogWarning($"[DialogueTree] Tree {treeId}, node {node.nodeId}: choice '{choice.choiceText}' has no nextNodeId!", this);
                    }
                    else if (GetNode(choice.nextNodeId) == null)
                    {
                        Debug.LogError($"[DialogueTree] Tree {treeId}, node {node.nodeId}: choice points to missing node '{choice.nextNodeId}'!", this);
                    }
                }

                // Check auto-advance target
                if (!string.IsNullOrEmpty(node.autoAdvanceToNode) && GetNode(node.autoAdvanceToNode) == null)
                {
                    Debug.LogError($"[DialogueTree] Tree {treeId}, node {node.nodeId}: autoAdvanceToNode '{node.autoAdvanceToNode}' not found!", this);
                }
            }

            Debug.Log($"[DialogueTree] Validation complete for tree '{treeId}': {nodes.Count} nodes, root={rootNodeId}");
        }

        /// <summary>
        /// Returns a summary of the tree for debugging.
        /// </summary>
        public string GetTreeSummary()
        {
            int branchingNodes = 0;
            int endNodes = 0;
            int conditionalNodes = 0;

            foreach (var node in nodes)
            {
                if (node == null) continue;

                if (node.endsConversation || (node.choices == null || node.choices.Length == 0))
                    endNodes++;
                else if (node.choices.Length > 1)
                    branchingNodes++;

                if (node.displayCondition.type != DialogueConditionType.None)
                    conditionalNodes++;
            }

            return $"Tree: {treeId}\n" +
                   $"Nodes: {nodes.Count}\n" +
                   $"Branching: {branchingNodes}\n" +
                   $"End nodes: {endNodes}\n" +
                   $"Conditional: {conditionalNodes}\n" +
                   $"Root: {rootNodeId}";
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor utility: Validate tree on save.
        /// </summary>
        void OnValidate()
        {
            // Delay validation to avoid issues during asset import
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                ValidateTree();
            };
        }
#endif
    }
}
