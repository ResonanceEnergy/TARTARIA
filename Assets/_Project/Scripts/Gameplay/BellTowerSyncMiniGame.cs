using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// BellTowerSyncMiniGame - Rhythm-based bell tuning minigame.
    /// Player must ring bells in correct sequence to restore resonance.
    /// </summary>
    public class BellTowerSyncMiniGame : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject miniGameUI;
        [SerializeField] private Text instructionsText;
        [SerializeField] private Text timerText;
        [SerializeField] private List<Button> bellButtons = new();

        [Header("Game Settings")]
        [SerializeField] private float timeLimit = 45f;
        [SerializeField] private int sequenceLength = 5;
        [SerializeField] private bool isPlaying = false;

        private List<int> _targetSequence = new();
        private List<int> _playerSequence = new();
        private System.Action _onComplete;
        private float _timer;

        void Start()
        {
            if (miniGameUI != null)
                miniGameUI.SetActive(false);
        }

        public void StartGame(System.Action onComplete)
        {
            _onComplete = onComplete;
            isPlaying = true;
            _timer = timeLimit;
            _playerSequence.Clear();

            // Generate random sequence
            _targetSequence.Clear();
            for (int i = 0; i < sequenceLength; i++)
            {
                _targetSequence.Add(Random.Range(0, bellButtons.Count));
            }

            // Show UI
            if (miniGameUI != null)
                miniGameUI.SetActive(true);

            if (instructionsText != null)
                instructionsText.text = $"Ring {sequenceLength} bells in the correct sequence!";

            Debug.Log($"[BellTowerSync] Started! Sequence: {string.Join(", ", _targetSequence)}");

            // Play sequence demonstration
            StartCoroutine(PlaySequenceDemonstration());
        }

        IEnumerator PlaySequenceDemonstration()
        {
            yield return new WaitForSeconds(1f);

            // Play each bell in sequence
            foreach (int bellIndex in _targetSequence)
            {
                if (bellIndex < bellButtons.Count)
                {
                    HighlightBell(bellIndex);
                    // TODO: Add audio service to ServiceLocator
                    yield return new WaitForSeconds(0.8f);
                }
            }

            if (instructionsText != null)
                instructionsText.text = "Now you try!";
        }

        void Update()
        {
            if (!isPlaying) return;

            _timer -= Time.unscaledDeltaTime;
            if (timerText != null)
                timerText.text = $"Time: {_timer:F1}s";

            if (_timer <= 0)
            {
                FailGame();
            }
        }

        public void OnBellPressed(int bellIndex)
        {
            if (!isPlaying) return;

            _playerSequence.Add(bellIndex);
            HighlightBell(bellIndex);
            // TODO: Add audio service to ServiceLocator

            // Check if correct so far
            if (_playerSequence.Count <= _targetSequence.Count)
            {
                int currentIndex = _playerSequence.Count - 1;
                if (_playerSequence[currentIndex] != _targetSequence[currentIndex])
                {
                    // Wrong bell!
                    FailGame();
                    return;
                }
            }

            // Check if sequence complete
            if (_playerSequence.Count >= _targetSequence.Count)
            {
                SucceedGame();
            }
        }

        void HighlightBell(int bellIndex)
        {
            if (bellIndex < bellButtons.Count)
            {
                var button = bellButtons[bellIndex];
                StartCoroutine(FlashButton(button));
            }
        }

        IEnumerator FlashButton(Button button)
        {
            var colors = button.colors;
            var originalColor = colors.normalColor;
            colors.normalColor = Color.yellow;
            button.colors = colors;

            yield return new WaitForSeconds(0.3f);

            colors.normalColor = originalColor;
            button.colors = colors;
        }

        void SucceedGame()
        {
            isPlaying = false;
            Debug.Log("[BellTowerSync] SUCCESS!");

            // TODO: PlaySFX("BellTowerSuccess")
            ServiceLocator.HUD?.ShowBanner("SYNCHRONIZED!", "Bells ring in harmony");

            CleanupGame();
            _onComplete?.Invoke();
        }

        void FailGame()
        {
            isPlaying = false;
            Debug.Log("[BellTowerSync] FAILED!");

            // TODO: PlaySFX("BellTowerFail")
            ServiceLocator.HUD?.ShowBanner("FAILED", "Wrong sequence");

            CleanupGame();
        }

        void CleanupGame()
        {
            if (miniGameUI != null)
                miniGameUI.SetActive(false);

            _playerSequence.Clear();
            _targetSequence.Clear();
        }
    }
}
