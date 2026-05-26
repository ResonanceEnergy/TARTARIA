using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Data;

namespace Tartaria.Integration
{
    /// <summary>
    /// DialoguePlayer — traverses DialogueTreeAsset instances and displays nodes.
    /// Handles choice selection, auto-advance, and conversation state tracking.
    ///
    /// Usage:
    /// 1. DialogueManager.PlayTree("Anastasia_Intro") loads tree and creates DialoguePlayer
    /// 2. Player selects choices via UI
    /// 3. DialoguePlayer.SelectChoice(index) advances to next node
    /// 4. OnConversationEnded event fires when conversation completes
    ///
    /// Integrates with:
    /// - UIManager for choice display
    /// - DialogueManager for line playback
    /// - QuestManager for quest events
    /// </summary>
    public class DialoguePlayer : MonoBehaviour
    {
        DialogueTreeAsset _currentTree;
        DialogueNodeData _currentNode;
        List<DialogueChoice> _currentChoices = new List<DialogueChoice>();
        HashSet<string> _visitedNodes = new HashSet<string>();

        bool _isPlaying;
        bool _waitingForChoice;
        float _autoAdvanceTimer;

        public event Action<DialogueNodeData> OnNodeDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesAvailable;
        public event Action<string> OnConversationEnded; // Passes tree ID

        public bool IsPlaying => _isPlaying;
        public bool WaitingForChoice => _waitingForChoice;
        public DialogueTreeAsset CurrentTree => _currentTree;
        public DialogueNodeData CurrentNode => _currentNode;
        public List<DialogueChoice> CurrentChoices => _currentChoices;

        void Update()
        {
            if (!_isPlaying || _currentNode == null)
                return;

            // Auto-advance logic
            if (_autoAdvanceTimer > 0f)
            {
                _autoAdvanceTimer -= Time.deltaTime;
                if (_autoAdvanceTimer <= 0f && !string.IsNullOrEmpty(_currentNode.autoAdvanceToNode))
                {
                    AdvanceToNode(_currentNode.autoAdvanceToNode);
                }
            }
        }

        /// <summary>
        /// Starts playing a dialogue tree from its root node.
        /// </summary>
        public void PlayTree(DialogueTreeAsset tree)
        {
            if (tree == null)
            {
                Debug.LogError("[DialoguePlayer] Cannot play null tree!");
                return;
            }

            if (_isPlaying)
            {
                Debug.LogWarning($"[DialoguePlayer] Already playing tree {_currentTree?.treeId}. Stopping current conversation.");
                EndConversation();
            }

            _currentTree = tree;
            _visitedNodes.Clear();
            _isPlaying = true;

            Debug.Log($"[DialoguePlayer] Starting tree: {tree.treeId}");

            var rootNode = tree.GetRootNode();
            if (rootNode == null)
            {
                Debug.LogError($"[DialoguePlayer] Tree {tree.treeId} has no valid root node!");
                EndConversation();
                return;
            }

            DisplayNode(rootNode);
        }

        /// <summary>
        /// Advances to a specific node by ID.
        /// </summary>
        public void AdvanceToNode(string nodeId)
        {
            if (!_isPlaying)
            {
                Debug.LogWarning("[DialoguePlayer] Cannot advance - no conversation active!");
                return;
            }

            var nextNode = _currentTree.GetNode(nodeId);
            if (nextNode == null)
            {
                Debug.LogError($"[DialoguePlayer] Failed to advance to node '{nodeId}' - node not found!");
                EndConversation();
                return;
            }

            DisplayNode(nextNode);
        }

        /// <summary>
        /// Selects a choice by index. Advances conversation to the chosen branch.
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!_waitingForChoice)
            {
                Debug.LogWarning("[DialoguePlayer] Not waiting for choice input!");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= _currentChoices.Count)
            {
                Debug.LogError($"[DialoguePlayer] Invalid choice index: {choiceIndex} (available: {_currentChoices.Count})");
                return;
            }

            var choice = _currentChoices[choiceIndex];
            _waitingForChoice = false;

            Debug.Log($"[DialoguePlayer] Choice selected: {choice.choiceText}");

            if (choice.endsConversation)
            {
                EndConversation();
            }
            else if (!string.IsNullOrEmpty(choice.nextNodeId))
            {
                AdvanceToNode(choice.nextNodeId);
            }
            else
            {
                Debug.LogWarning($"[DialoguePlayer] Choice has no next node and doesn't end conversation!");
                EndConversation();
            }
        }

        /// <summary>
        /// Ends the current conversation and cleans up state.
        /// </summary>
        public void EndConversation()
        {
            if (!_isPlaying)
                return;

            string treeId = _currentTree?.treeId;
            _isPlaying = false;
            _waitingForChoice = false;
            _currentNode = null;
            _currentChoices.Clear();
            _autoAdvanceTimer = 0f;

            // Hide dialogue UI
            // UIManager.Instance?.HideDialogue(); // UI assembly disabled (Phase 7)
            // TODO: Re-enable when UI assembly restored

            Debug.Log($"[DialoguePlayer] Conversation ended: {treeId}");
            OnConversationEnded?.Invoke(treeId);

            _currentTree = null;
        }

        void DisplayNode(DialogueNodeData node)
        {
            if (node == null)
            {
                Debug.LogError("[DialoguePlayer] Attempted to display null node!");
                EndConversation();
                return;
            }

            // Check if node can be displayed (condition evaluation)
            if (!node.CanDisplay())
            {
                Debug.LogWarning($"[DialoguePlayer] Node {node.nodeId} condition not met. Ending conversation.");
                EndConversation();
                return;
            }

            _currentNode = node;
            _visitedNodes.Add(node.nodeId);

            Debug.Log($"[DialoguePlayer] Displaying node: {node.nodeId} ({node.speakerName})");

            // Execute node events (quest activation/completion, relationship changes)
            node.ExecuteNodeEvents();

            // Display dialogue text via UIManager
            string displayText = node.dialogueText;
            if (string.IsNullOrEmpty(displayText))
                displayText = "[MISSING DIALOGUE TEXT]";

            // UIManager.Instance?.ShowDialogue(node.speakerName, displayText); // UI assembly disabled (Phase 7)

            // Play voice line if available
            if (!string.IsNullOrEmpty(node.voiceLineId))
            {
                DialogueManager.Instance?.PlayLineById(node.voiceLineId);
            }

            // Fire event for listeners
            OnNodeDisplayed?.Invoke(node);

            // Determine next action
            if (node.endsConversation)
            {
                // Conversation ends after this node
                Invoke(nameof(EndConversation), 3f); // Short delay before closing
            }
            else if (node.autoAdvanceDelay > 0f && !string.IsNullOrEmpty(node.autoAdvanceToNode))
            {
                // Auto-advance to next node after delay
                _autoAdvanceTimer = node.autoAdvanceDelay;
                _waitingForChoice = false;
            }
            else
            {
                // Present choices to player
                ShowChoices(node);
            }
        }

        void ShowChoices(DialogueNodeData node)
        {
            _currentChoices = node.GetAvailableChoices();

            if (_currentChoices.Count == 0)
            {
                // No choices available - treat as conversation end
                Debug.LogWarning($"[DialoguePlayer] Node {node.nodeId} has no available choices. Ending conversation.");
                Invoke(nameof(EndConversation), 2f);
                return;
            }

            _waitingForChoice = true;

            Debug.Log($"[DialoguePlayer] Presenting {_currentChoices.Count} choices");

            // Notify UI to display choices
            OnChoicesAvailable?.Invoke(_currentChoices);

            // Fallback: if no UI integration, display in console
            // if (UIManager.Instance == null) // UI assembly disabled (Phase 7)
            // {
                Debug.Log("[DialoguePlayer] Available choices:");
                for (int i = 0; i < _currentChoices.Count; i++)
                {
                    Debug.Log($"  [{i}] {_currentChoices[i].choiceText}");
                }
            // }
        }

        /// <summary>
        /// Skips the current node if auto-advance is pending.
        /// </summary>
        public void SkipAutoAdvance()
        {
            if (_autoAdvanceTimer > 0f && !string.IsNullOrEmpty(_currentNode?.autoAdvanceToNode))
            {
                _autoAdvanceTimer = 0f;
                AdvanceToNode(_currentNode.autoAdvanceToNode);
            }
        }

        /// <summary>
        /// Returns true if the player has visited a specific node in the current tree.
        /// </summary>
        public bool HasVisitedNode(string nodeId)
        {
            return _visitedNodes.Contains(nodeId);
        }

        /// <summary>
        /// Debug helper: prints current conversation state.
        /// </summary>
        public void DebugPrintState()
        {
            if (!_isPlaying)
            {
                Debug.Log("[DialoguePlayer] No active conversation.");
                return;
            }

            Debug.Log($"[DialoguePlayer] State:\n" +
                      $"  Tree: {_currentTree?.treeId}\n" +
                      $"  Node: {_currentNode?.nodeId}\n" +
                      $"  Waiting for choice: {_waitingForChoice}\n" +
                      $"  Choices available: {_currentChoices.Count}\n" +
                      $"  Visited nodes: {_visitedNodes.Count}");
        }
    }
}
