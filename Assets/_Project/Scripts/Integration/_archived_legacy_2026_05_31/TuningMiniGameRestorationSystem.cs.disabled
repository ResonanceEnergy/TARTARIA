using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.UI;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// TuningMiniGame - Frequency matching mini-game for building restoration.
    /// Player adjusts frequency slider to match target (432 Hz, 528 Hz, etc.).
    /// Per 15_MVP_BUILD_SPEC.md.
    /// </summary>
    public class TuningMiniGame : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject tuningUI;
        [SerializeField] private Slider frequencySlider;
        [SerializeField] private Text currentFrequencyText;
        [SerializeField] private Text targetFrequencyText;
        [SerializeField] private Image feedbackMeter; // Green = close, Red = far

        [Header("Game Settings")]
        [SerializeField] private float targetFrequency = 432f;
        [SerializeField] private float tolerance = 5f; // ±5 Hz
        [SerializeField] private float timeLimit = 30f;
        [SerializeField] private bool isPlaying = false;

        private System.Action _onComplete;
        private float _timer;

        void Start()
        {
            if (tuningUI != null)
                tuningUI.SetActive(false);
        }

        public void StartTuning(Vector3 nodePosition, System.Action onComplete)
        {
            _onComplete = onComplete;
            isPlaying = true;
            _timer = timeLimit;

            // Randomize target frequency (432, 528, 639, 741, 852 Hz - Solfeggio scale)
            float[] frequencies = { 432f, 528f, 639f, 741f, 852f };
            targetFrequency = frequencies[Random.Range(0, frequencies.Length)];

            // Show UI
            if (tuningUI != null)
                tuningUI.SetActive(true);

            // Set slider range (target ± 100 Hz)
            if (frequencySlider != null)
            {
                frequencySlider.minValue = targetFrequency - 100f;
                frequencySlider.maxValue = targetFrequency + 100f;
                frequencySlider.value = Random.Range(frequencySlider.minValue, frequencySlider.maxValue);
            }

            if (targetFrequencyText != null)
                targetFrequencyText.text = $"Target: {targetFrequency:F0} Hz";

            Debug.Log($"[TuningMiniGame] Started! Target: {targetFrequency} Hz");

            // Disable player movement
            Time.timeScale = 0.5f; // Slow-mo effect

            // Enable depth of field
            PostProcessingSetup.Instance?.EnableDepthOfField(true);
        }

        void Update()
        {
            if (!isPlaying) return;

            // Update timer
            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0)
            {
                FailTuning();
                return;
            }

            // Update frequency display
            if (frequencySlider != null && currentFrequencyText != null)
            {
                float current = frequencySlider.value;
                currentFrequencyText.text = $"{current:F1} Hz";

                // Calculate distance from target
                float distance = Mathf.Abs(current - targetFrequency);
                float accuracy = 1f - Mathf.Clamp01(distance / 100f);

                // Update feedback meter (green = close)
                if (feedbackMeter != null)
                {
                    feedbackMeter.color = Color.Lerp(Color.red, Color.green, accuracy);
                }

                // Check success
                if (distance <= tolerance)
                {
                    SucceedTuning();
                }
            }
        }

        void SucceedTuning()
        {
            isPlaying = false;
            Debug.Log("[TuningMiniGame] SUCCESS!");

            // VFX + Audio
            AudioFeedbackController.Instance?.PlaySFX("TuningSuccess", Vector3.zero);
            HUDController.Instance?.ShowBanner("TUNED!", $"Frequency matched: {targetFrequency:F0} Hz");

            // Cleanup
            CleanupTuning();

            // Callback
            _onComplete?.Invoke();
        }

        void FailTuning()
        {
            isPlaying = false;
            Debug.Log("[TuningMiniGame] FAILED (timeout)");

            AudioFeedbackController.Instance?.PlaySFX("TuningFail", Vector3.zero);
            HUDController.Instance?.ShowBanner("FAILED", "Tuning timeout");

            CleanupTuning();
        }

        void CleanupTuning()
        {
            // Hide UI
            if (tuningUI != null)
                tuningUI.SetActive(false);

            // Restore time
            Time.timeScale = 1f;

            // Disable depth of field
            PostProcessingSetup.Instance?.EnableDepthOfField(false);
        }

        public bool IsPlaying() => isPlaying;
    }
}
