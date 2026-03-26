using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Tuning Mini-Game Controller — manages the three tuning variants:
    ///   A: Frequency Slider (15s, match golden band)
    ///   B: Waveform Trace (20s, draw the sound)
    ///   C: Harmonic Pattern (10s, rhythm taps)
    ///
    /// Scoring:
    ///   Perfect (>95%) = ×φ (1.618), Great (80–95%) = ×1.3,
    ///   Good (60–80%) = ×1.0, Fail (<60%) = retry
    /// </summary>
    public class TuningMiniGameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float baseTargetFrequency = 432f;

        // Current game state
        TuningPuzzleConfig _currentConfig;
        TuningVariant _currentVariant;
        float _timeRemaining;
        float _currentFrequency;
        float _accuracy;
        bool _isActive;

        // Events
        public event System.Action<float> OnTuningComplete; // accuracy 0–1
        public event System.Action OnTuningFailed;
        public event System.Action<float> OnFrequencyChanged; // current freq

        public bool IsActive => _isActive;
        public float TimeRemaining => _timeRemaining;
        public float CurrentAccuracy => _accuracy;

        public void StartTuning(TuningPuzzleConfig config)
        {
            _currentConfig = config;
            _currentVariant = config.variant;
            _timeRemaining = config.timeLimitSeconds;
            _currentFrequency = 0f;
            _accuracy = 0f;
            _isActive = true;

            GameStateManager.Instance.TransitionTo(GameState.Tuning);
        }

        void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                CompleteTuning();
                return;
            }

            switch (_currentVariant)
            {
                case TuningVariant.FrequencySlider:
                    UpdateFrequencySlider();
                    break;
                case TuningVariant.WaveformTrace:
                    UpdateWaveformTrace();
                    break;
                case TuningVariant.HarmonicPattern:
                    UpdateHarmonicPattern();
                    break;
            }
        }

        // ─── Variant A: Frequency Slider ──────────────

        void UpdateFrequencySlider()
        {
            // Mouse X position maps to frequency range
            float mouseX = UnityEngine.Input.mousePosition.x / Screen.width;
            float freqRange = _currentConfig.targetFrequency * 2f;
            _currentFrequency = mouseX * freqRange;

            // Calculate accuracy based on proximity to target
            _accuracy = GoldenRatioValidator.GetFrequencyAccuracy(
                _currentFrequency, _currentConfig.targetFrequency);

            OnFrequencyChanged?.Invoke(_currentFrequency);

            // Haptic feedback: vibration matches accuracy
            if (HapticFeedbackManager.Instance != null)
            {
                if (_accuracy > 0.9f)
                    HapticFeedbackManager.Instance.PlayTuningOnFrequency();
                else
                    HapticFeedbackManager.Instance.PlayTuningOffFrequency();
            }

            // Player confirms with click
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                CompleteTuning();
            }
        }

        // ─── Variant B: Waveform Trace ────────────────

        float _traceAccumulatedAccuracy;
        int _traceSamples;

        void UpdateWaveformTrace()
        {
            if (UnityEngine.Input.GetMouseButton(0))
            {
                // Track how closely mouse follows the golden waveform
                float mouseY = UnityEngine.Input.mousePosition.y / Screen.height;
                float targetY = GenerateGoldenWaveform(Time.time);
                float sampleAccuracy = 1.0f - Mathf.Abs(mouseY - targetY);

                _traceAccumulatedAccuracy += Mathf.Max(0f, sampleAccuracy);
                _traceSamples++;

                _accuracy = _traceSamples > 0
                    ? _traceAccumulatedAccuracy / _traceSamples
                    : 0f;
            }
        }

        float GenerateGoldenWaveform(float time)
        {
            // Golden-ratio frequency modulated sine wave
            float freq = _currentConfig.targetFrequency / 100f; // Scale for visual
            return 0.5f + 0.4f * Mathf.Sin(
                time * freq * GoldenRatioValidator.PHI);
        }

        // ─── Variant C: Harmonic Pattern ──────────────

        int _patternIndex;
        int _correctTaps;
        readonly int _totalTaps = 5;
        float _lastTapTime;
        readonly float _tapWindow = 0.3f; // ±300ms for "ok"

        void UpdateHarmonicPattern()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0) && _patternIndex < _totalTaps)
            {
                float expectedBeat = GetExpectedBeatTime(_patternIndex);
                float timing = Mathf.Abs(Time.time - expectedBeat);

                if (timing <= 0.1f)       // ±100ms = perfect
                    _correctTaps += 3;
                else if (timing <= 0.2f)  // ±200ms = good
                    _correctTaps += 2;
                else if (timing <= _tapWindow) // ±300ms = ok
                    _correctTaps += 1;

                _patternIndex++;
                _accuracy = (float)_correctTaps / (_totalTaps * 3);

                if (_patternIndex >= _totalTaps)
                    CompleteTuning();
            }
        }

        float GetExpectedBeatTime(int index)
        {
            // Beats spaced at golden-ratio intervals
            float beatInterval = _currentConfig.timeLimitSeconds / (_totalTaps + 1);
            return _lastTapTime + beatInterval * (index + 1);
        }

        // ─── Completion ───────────────────────────────

        void CompleteTuning()
        {
            _isActive = false;

            if (_accuracy >= 0.6f)
            {
                // Success! Return to exploration
                OnTuningComplete?.Invoke(_accuracy);

                // Haptic feedback for success
                if (HapticFeedbackManager.Instance != null)
                {
                    if (_accuracy >= 0.95f)
                        HapticFeedbackManager.Instance.PlayPerfectTune();
                    else
                        HapticFeedbackManager.Instance.StopAll();
                }
            }
            else
            {
                // Fail — reset for retry
                OnTuningFailed?.Invoke();
            }

            HapticFeedbackManager.Instance?.StopAll();
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
        }

        /// <summary>
        /// Returns the RS multiplier tier for the achieved accuracy.
        /// </summary>
        public static string GetAccuracyTier(float accuracy)
        {
            if (accuracy >= 0.95f) return "Perfect";
            if (accuracy >= 0.80f) return "Great";
            if (accuracy >= 0.60f) return "Good";
            return "Fail";
        }
    }
}
