using UnityEngine;
using Yarn.Unity;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// Yarn Dialogue Adapter — bridges Yarn Spinner dialogue system to existing
    /// DialogueManager. Wires TartariaLineView for presentation and 
    /// TartariaVariableStorage for game state access.
    /// 
    /// Attach to the same GameObject as DialogueRunner + YarnProject.
    /// TartariaLineView and TartariaVariableStorage are added automatically.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public class YarnDialogueAdapter : MonoBehaviour
    {
        DialogueRunner _runner;
        TartariaLineView _lineView;
        TartariaVariableStorage _variableStorage;

        void Awake()
        {
            _runner = GetComponent<DialogueRunner>();

            // Add line presenter
            _lineView = gameObject.GetComponent<TartariaLineView>();
            if (_lineView == null)
                _lineView = gameObject.AddComponent<TartariaLineView>();

            // Add variable storage
            _variableStorage = gameObject.GetComponent<TartariaVariableStorage>();
            if (_variableStorage == null)
                _variableStorage = gameObject.AddComponent<TartariaVariableStorage>();

            // Wire variable storage to runner (Yarn auto-detects VariableStorageBehaviour on same GameObject)
            // _runner.variableStorage is read-only in Yarn 2.5.1, no manual assignment needed

            Debug.Log("[YarnDialogueAdapter] Initialized — TartariaLineView + TartariaVariableStorage wired.");
        }

        void Start()
        {
            if (_runner == null || _runner.CurrentNodeName != null)
                return;

            // Auto-start "Start" node if YarnProject is loaded
            if (_runner.NodeExists("Start"))
            {
                Debug.Log("[YarnDialogueAdapter] Auto-starting node 'Start'");
                _runner.StartDialogue("Start");
            }
        }

        /// <summary>
        /// Start a Yarn dialogue by node name.
        /// Call this from gameplay code to trigger a specific conversation.
        /// </summary>
        public void StartDialogue(string nodeName)
        {
            if (_runner == null)
            {
                Debug.LogError("[YarnDialogueAdapter] DialogueRunner not found!");
                return;
            }

            if (!_runner.NodeExists(nodeName))
            {
                Debug.LogWarning($"[YarnDialogueAdapter] Node '{nodeName}' does not exist in YarnProject.");
                return;
            }

            _runner.StartDialogue(nodeName);
        }

        /// <summary>
        /// Stop the current dialogue.
        /// </summary>
        public void StopDialogue()
        {
            if (_runner != null && _runner.IsDialogueRunning)
                _runner.Stop();
        }
    }
}
