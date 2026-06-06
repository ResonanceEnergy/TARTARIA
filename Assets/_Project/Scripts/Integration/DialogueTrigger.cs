using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Automatic dialogue trigger component for NPCs. Plays context dialogue when player interacts.
    /// Used by Moon spawners to wire NPC dialogue without manual interaction setup.
    /// Requires DialogueManager.Instance to be available in Integration assembly.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("Dialogue Configuration")]
        [SerializeField] private string _dialogueContext = "npc_greeting";
        [SerializeField] private bool _playOnce = false;
        [SerializeField] private bool _requiresLineOfSight = true;
        [SerializeField] private float _interactionRange = 3f;

        [Header("Visual Feedback")]
        [SerializeField] private string _promptText = "Talk";
        [SerializeField] private GameObject _interactionPromptPrefab; // Optional 3D prompt above NPC

        // State tracking
        private bool _hasPlayed = false;
        private GameObject _interactionPromptInstance;

        void Awake()
        {
            // Spawn interaction prompt if configured
            if (_interactionPromptPrefab != null)
            {
                _interactionPromptInstance = Instantiate(_interactionPromptPrefab, transform.position + Vector3.up * 2.5f, Quaternion.identity, transform);
                _interactionPromptInstance.SetActive(false); // Hide until player is in range
            }
        }

        void Update()
        {
            // Show/hide interaction prompt based on player proximity
            if (_interactionPromptInstance != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    bool inRange = distance <= _interactionRange;
                    bool lineOfSightClear = !_requiresLineOfSight || HasLineOfSight(player.transform.position);

                    _interactionPromptInstance.SetActive(inRange && lineOfSightClear && (!_playOnce || !_hasPlayed));
                }
            }
        }

        /// <summary>
        /// IInteractable implementation: return dynamic prompt text
        /// </summary>
        public string GetInteractPrompt()
        {
            if (_playOnce && _hasPlayed)
            {
                return ""; // No prompt if already played
            }

            return $"{_promptText} (Hold [E])";
        }

        /// <summary>
        /// IInteractable implementation: play dialogue when player interacts
        /// </summary>
        public void Interact(GameObject interactor)
        {
            // Check if already played (one-shot dialogue)
            if (_playOnce && _hasPlayed)
            {
                Debug.Log($"[DialogueTrigger] {gameObject.name} dialogue already played, ignoring interaction.");
                return;
            }

            // Play context dialogue via DialogueManager
            if (DialogueManager.Instance != null)
            {
                Debug.Log($"[DialogueTrigger] {gameObject.name} playing dialogue context: {_dialogueContext}");
                DialogueManager.Instance.PlayContextDialogue(_dialogueContext);
                _hasPlayed = true;

                // Hide interaction prompt after first play if configured
                if (_playOnce && _interactionPromptInstance != null)
                {
                    _interactionPromptInstance.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"[DialogueTrigger] {gameObject.name} cannot play dialogue - DialogueManager.Instance is null!");
            }
        }

        /// <summary>
        /// Check if NPC has clear line of sight to target position (simple raycast).
        /// </summary>
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f; // Eye level
            Vector3 direction = (targetPosition - origin).normalized;
            float distance = Vector3.Distance(origin, targetPosition);

            // Raycast to check for obstacles (ignore triggers)
            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                // If we hit something other than the player, line of sight is blocked
                return hit.collider.CompareTag("Player");
            }

            return true; // No obstacles
        }

        /// <summary>
        /// Public API: manually trigger dialogue from external script
        /// </summary>
        public void TriggerDialogue()
        {
            Interact(null);
        }

        /// <summary>
        /// Public API: set dialogue context dynamically (e.g., for quest progression)
        /// </summary>
        public void SetDialogueContext(string newContext)
        {
            _dialogueContext = newContext;
            _hasPlayed = false; // Reset one-shot state
            Debug.Log($"[DialogueTrigger] {gameObject.name} dialogue context set to: {newContext}");
        }

        /// <summary>
        /// Public API: reset one-shot state (allow dialogue to replay)
        /// </summary>
        public void ResetPlayState()
        {
            _hasPlayed = false;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Draw interaction range sphere
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _interactionRange);

            // Draw line of sight ray to player if configured
            if (_requiresLineOfSight)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 origin = transform.position + Vector3.up * 1.5f;
                    Vector3 targetPos = player.transform.position + Vector3.up * 1f;
                    bool los = HasLineOfSight(targetPos);

                    Gizmos.color = los ? Color.green : Color.red;
                    Gizmos.DrawLine(origin, targetPos);
                }
            }
        }
#endif
    }
}
