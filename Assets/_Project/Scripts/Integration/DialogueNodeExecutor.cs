using UnityEngine;
using Tartaria.Data;

namespace Tartaria.Integration
{
    /// <summary>
    /// DialogueNodeExecutor — executes dialogue node events (quest activation/completion, relationship changes).
    /// Bridges Data assembly (DialogueNodeData) with Integration assembly (QuestManager).
    ///
    /// Architecture:
    ///   - Data assembly: DialogueNodeData.ExecuteNodeEvents() calls static ExecuteDelegate
    ///   - Integration assembly: DialogueNodeExecutor sets delegate in Awake()
    ///   - Avoids circular dependency (Data → Integration)
    ///
    /// Supported events:
    ///   - activateQuestId: Calls QuestManager.ActivateQuest()
    ///   - completeQuestId: Calls QuestManager.CompleteQuest()
    ///   - setRelationshipValue/relationshipDelta: Logged (future NPC system integration)
    ///
    /// Execution order: -65 (before DialogueConditionEvaluator -60, after QuestManager -50)
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class DialogueNodeExecutor : MonoBehaviour
    {
        static DialogueNodeExecutor Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("DialogueNodeExecutor");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<DialogueNodeExecutor>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Wire execution delegate to bridge Data → Integration
            DialogueNodeData.ExecuteDelegate = ExecuteNodeEvents;
            Debug.Log("[DialogueNodeExecutor] Node event execution delegate registered");
        }

        void OnDestroy()
        {
            // Cleanup delegate
            if (DialogueNodeData.ExecuteDelegate == ExecuteNodeEvents)
            {
                DialogueNodeData.ExecuteDelegate = null;
            }
        }

        /// <summary>
        /// Executes node events with access to all game systems.
        /// </summary>
        static void ExecuteNodeEvents(DialogueNodeData node)
        {
            if (node == null)
            {
                Debug.LogWarning("[DialogueNodeExecutor] Attempted to execute events on null node");
                return;
            }

            // Quest activation
            if (!string.IsNullOrEmpty(node.activateQuestId))
            {
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.ActivateQuest(node.activateQuestId);
                    Debug.Log($"[DialogueNodeExecutor] Activated quest: {node.activateQuestId} (via node {node.nodeId})");
                }
                else
                {
                    Debug.LogWarning($"[DialogueNodeExecutor] Cannot activate quest {node.activateQuestId}: QuestManager not initialized");
                }
            }

            // Quest completion
            if (!string.IsNullOrEmpty(node.completeQuestId))
            {
                if (QuestManager.Instance != null)
                {
                    // Note: QuestManager.CompleteQuest may not exist - use ProgressObjective instead
                    var state = QuestManager.Instance.GetQuestState(node.completeQuestId);
                    if (state.status == Core.Enums.QuestStatus.Active)
                    {
                        // Mark all objectives complete
                        for (int i = 0; i < state.objectiveProgress.Length; i++)
                        {
                            QuestManager.Instance.ProgressObjective(node.completeQuestId, i, 99999); // Force complete
                        }
                        Debug.Log($"[DialogueNodeExecutor] Completed quest: {node.completeQuestId} (via node {node.nodeId})");
                    }
                    else
                    {
                        Debug.LogWarning($"[DialogueNodeExecutor] Cannot complete quest {node.completeQuestId}: not active (status={state.status})");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DialogueNodeExecutor] Cannot complete quest {node.completeQuestId}: QuestManager not initialized");
                }
            }

            // Relationship tracking (placeholder for future NPC system)
            if (node.setRelationshipValue >= 0)
            {
                Debug.Log($"[DialogueNodeExecutor] Set {node.speakerName} relationship to {node.setRelationshipValue} (node {node.nodeId}) — NPC system not implemented");
            }
            else if (node.relationshipDelta != 0)
            {
                Debug.Log($"[DialogueNodeExecutor] Modified {node.speakerName} relationship by {node.relationshipDelta:+0;-0} (node {node.nodeId}) — NPC system not implemented");
            }
        }
    }
}
