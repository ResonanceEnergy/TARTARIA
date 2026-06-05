using UnityEngine;
using Yarn.Unity;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tartaria Line View — bridges Yarn Spinner dialogue lines to DialogueManager.
    /// Presents each Yarn line via DialogueManager.ShowDialogue UI + VO system.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public class TartariaLineView : DialogueViewBase
    {
        [SerializeField, Tooltip("Duration multiplier per word (default: 0.3s per word)")]
        float secondsPerWord = 0.3f;

        [SerializeField, Tooltip("Minimum line duration (seconds)")]
        float minDuration = 1.5f;

        string _currentLineID;
        string _currentSpeaker;
        float _currentDuration;

        public override void RunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
        {
            // Extract line data
            _currentLineID = dialogueLine.TextID;
            var text = dialogueLine.RawText;

            // Parse speaker from Yarn speaker attribute (if present)
            _currentSpeaker = "???";
            if (dialogueLine.CharacterName != null && !string.IsNullOrEmpty(dialogueLine.CharacterName))
                _currentSpeaker = dialogueLine.CharacterName;

            // Estimate duration (word count × secondsPerWord)
            int wordCount = text.Split(new[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
            _currentDuration = Mathf.Max(minDuration, wordCount * secondsPerWord);

            // Show via DialogueManager
            UI.UIManager.Instance?.ShowDialogue(_currentSpeaker, text);

            // Play VO if available (line ID matches Yarn TextID)
            Audio.VOPlaceholderLibrary.PlayLineIfAvailable(_currentLineID);

            // Auto-advance after duration
            CancelInvoke(nameof(FinishLine));
            Invoke(nameof(FinishLine), _currentDuration);

            // Store the callback to notify Yarn when done
            _onLineFinished = onDialogueLineFinished;

            Debug.Log($"[YarnLine] {_currentSpeaker}: {text}");
        }

        public override void DismissLine(System.Action onDismissalComplete)
        {
            UI.UIManager.Instance?.HideDialogue();
            onDismissalComplete?.Invoke();
        }

        System.Action _onLineFinished;

        void FinishLine()
        {
            UI.UIManager.Instance?.HideDialogue();
            _onLineFinished?.Invoke();
        }

        public override void InterruptLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
        {
            // Player skipped line — immediately finish
            CancelInvoke(nameof(FinishLine));
            UI.UIManager.Instance?.HideDialogue();
            onDialogueLineFinished?.Invoke();
        }
    }
}
