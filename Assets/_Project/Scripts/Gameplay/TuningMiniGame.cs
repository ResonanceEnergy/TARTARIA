using UnityEngine;
using UnityEngine.InputSystem;
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
            _patternIndex = 0;
            _correctTaps = 0;
            _lastTapTime = Time.time;
            _traceAccumulatedAccuracy = 0f;
            _traceSamples = 0;

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
                case TuningVariant.BellTower:
                    UpdateBellTower();
                    break;
            }
        }

        // ─── Variant A: Frequency Slider ──────────────

        void UpdateFrequencySlider()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Mouse X position maps to frequency range
            float mouseX = mouse.position.ReadValue().x / Screen.width;
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
            if (mouse.leftButton.wasPressedThisFrame)
            {
                CompleteTuning();
            }
        }

        // ─── Variant B: Waveform Trace ────────────────

        float _traceAccumulatedAccuracy;
        int _traceSamples;

        void UpdateWaveformTrace()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                // Track how closely mouse follows the golden waveform
                float mouseY = mouse.position.ReadValue().y / Screen.height;
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
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && _patternIndex < _totalTaps)
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

        // ─── Variant D: Bell Tower Purification ──────

        int _bellSequenceIndex;
        int _correctBells;
        readonly int _bellSequenceLength = 7;
        float _bellShieldStrength;
        // Bell pattern: sequence of directions (0=North, 1=East, 2=South, 3=West)
        readonly int[] _bellPattern = { 0, 2, 1, 3, 0, 3, 1 };

        void UpdateBellTower()
        {
            if (_bellSequenceIndex >= _bellSequenceLength)
            {
                CompleteTuning();
                return;
            }

            // Check directional input (WASD mapped to bell directions)
            var keyboard = Keyboard.current;
            int inputDir = -1;
            if (keyboard != null)
            {
                if (keyboard.wKey.wasPressedThisFrame) inputDir = 0; // North
                if (keyboard.dKey.wasPressedThisFrame) inputDir = 1; // East
                if (keyboard.sKey.wasPressedThisFrame) inputDir = 2; // South
                if (keyboard.aKey.wasPressedThisFrame) inputDir = 3; // West
            }

            if (inputDir < 0) return;

            // Check timing window
            float expectedTime = GetExpectedBellTime(_bellSequenceIndex);
            float timing = Mathf.Abs(Time.time - expectedTime);

            bool correctDirection = inputDir == _bellPattern[
                _bellSequenceIndex % _bellPattern.Length];

            if (correctDirection)
            {
                if (timing <= 0.15f)       // ±150ms = perfect ring
                    _correctBells += 3;
                else if (timing <= 0.3f)   // ±300ms = good ring
                    _correctBells += 2;
                else                       // Late but correct direction
                    _correctBells += 1;

                // Build scalar wave shield with each correct bell
                _bellShieldStrength += 1f / _bellSequenceLength;
            }

            _bellSequenceIndex++;
            _accuracy = (float)_correctBells / (_bellSequenceLength * 3);

            // Haptic feedback for each bell ring
            if (HapticFeedbackManager.Instance != null)
            {
                if (correctDirection)
                    HapticFeedbackManager.Instance.PlayTuningOnFrequency();
                else
                    HapticFeedbackManager.Instance.PlayTuningOffFrequency();
            }

            OnFrequencyChanged?.Invoke(_currentConfig.targetFrequency
                * (0.5f + _accuracy * 0.5f));
        }

        float GetExpectedBellTime(int index)
        {
            // Bell rings spaced at golden-ratio rhythm intervals
            float interval = _currentConfig.timeLimitSeconds / (_bellSequenceLength + 1);
            return _lastTapTime + interval * (index + 1)
                * GoldenRatioValidator.PHI_INVERSE;
        }

        /// <summary>
        /// Shield strength generated by a successful Bell Tower sequence.
        /// 0 = no shield, 1 = full scalar wave barrier.
        /// </summary>
        public float BellShieldStrength => _bellShieldStrength;

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
