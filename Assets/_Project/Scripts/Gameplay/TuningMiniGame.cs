using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Audio;
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
        [SerializeField, Min(1f)] float controllerNudgeHzPerSecond = 120f;

        float _controllerFrequencyAxis;

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

            // Apply skill tree tuning speed bonus
            float speedMod = SkillTreeSystem.Instance?.GetModifier(SkillModifierType.TuningSpeed) ?? 0f;
            _timeRemaining *= (1f + speedMod);

            _currentFrequency = config.targetFrequency > 0 ? 0f : baseTargetFrequency * 0.5f;
            _accuracy = 0f;
            _isActive = true;
            _patternIndex = 0;
            _correctTaps = 0;
            _lastTapTime = Time.time;
            _traceAccumulatedAccuracy = 0f;
            _traceSamples = 0;

            GameStateManager.Instance?.TransitionTo(GameState.Tuning);
            AudioManager.Instance?.PlaySFX2D("TuningStart");
        }

        void OnEnable()
        {
            if (PlayerInputHandler.Instance != null)
                PlayerInputHandler.Instance.OnFrequencyAdjust += HandleFrequencyAdjust;
        }

        void OnDisable()
        {
            if (PlayerInputHandler.Instance != null)
                PlayerInputHandler.Instance.OnFrequencyAdjust -= HandleFrequencyAdjust;
        }

        void HandleFrequencyAdjust(float axis)
        {
            _controllerFrequencyAxis = Mathf.Clamp(axis, -1f, 1f);
        }

        void Update()
        {
            if (!_isActive) return;

            _controllerFrequencyAxis = Mathf.MoveTowards(_controllerFrequencyAxis, 0f, Time.deltaTime * 6f);
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
            float freqRange = _currentConfig.targetFrequency * 2f;
            bool inputReceived = false;

            // ─ Keyboard/Mouse input ─
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float mouseX = mouse.position.ReadValue().x / Screen.width;
                _currentFrequency = mouseX * freqRange;
                inputReceived = true;

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    CompleteTuning();
                    return;
                }
            }

            // ─ Gamepad input (analog sticks for precision tuning) ─
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                // Right stick X: direct frequency control (horizontal sweep)
                Vector2 rightStick = gamepad.rightStick.ReadValue();
                if (Mathf.Abs(rightStick.x) > 0.2f)
                {
                    _currentFrequency = (rightStick.x + 1f) / 2f * freqRange; // Normalize -1..1 to 0..1, then scale
                    inputReceived = true;
                }

                // Button confirm: A button (buttonSouth)
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    CompleteTuning();
                    return;
                }
            }

            // FrequencyAdjust axis (right stick Y from PlayerInputHandler) applies fine nudge.
            if (Mathf.Abs(_controllerFrequencyAxis) > 0.01f)
            {
                _currentFrequency = Mathf.Clamp(
                    _currentFrequency + (_controllerFrequencyAxis * controllerNudgeHzPerSecond * Time.deltaTime),
                    0f,
                    freqRange);
                inputReceived = true;
            }

            // Only proceed if we got input (keyboard or gamepad)
            if (!inputReceived) return;

            // Calculate accuracy based on proximity to target
            _accuracy = GoldenRatioValidator.GetFrequencyAccuracy(
                _currentFrequency, _currentConfig.targetFrequency);

            OnFrequencyChanged?.Invoke(_currentFrequency);

            // Real-time audio feedback: play tone at current frequency
            AudioManager.Instance?.PlayTone(_currentFrequency, 0.5f);

            // Haptic feedback: vibration matches accuracy
            if (HapticFeedbackManager.Instance != null)
            {
                if (_accuracy > 0.9f)
                    HapticFeedbackManager.Instance.PlayTuningOnFrequency();
                else
                    HapticFeedbackManager.Instance.PlayTuningOffFrequency();
            }
        }

        // ─── Variant B: Waveform Trace ────────────────

        float _traceAccumulatedAccuracy;
        int _traceSamples;

        void UpdateWaveformTrace()
        {
            bool tracing = false;
            float traceY = 0.5f;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                tracing = true;
                traceY = mouse.position.ReadValue().y / Screen.height;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.isPressed)
            {
                tracing = true;
                traceY = Mathf.Clamp01(0.5f + (_controllerFrequencyAxis * 0.5f));
            }

            if (tracing)
            {
                // Track how closely pointer/stick follows the golden waveform
                float targetY = GenerateGoldenWaveform(Time.time);
                float sampleAccuracy = 1.0f - Mathf.Abs(traceY - targetY);

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
            bool tap = false;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                tap = true;

            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
                tap = true;

            if (tap && _patternIndex < _totalTaps)
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

            // ─ Keyboard input (WASD mapped to bell directions) ─
            int inputDir = -1;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.wasPressedThisFrame) inputDir = 0; // North
                if (keyboard.dKey.wasPressedThisFrame) inputDir = 1; // East
                if (keyboard.sKey.wasPressedThisFrame) inputDir = 2; // South
                if (keyboard.aKey.wasPressedThisFrame) inputDir = 3; // West
            }

            // ─ Gamepad input (D-pad mapped to bell directions) ─
            var gamepad = Gamepad.current;
            if (gamepad != null && inputDir < 0)
            {
                if (gamepad.dpad.up.wasPressedThisFrame) inputDir = 0;     // North
                if (gamepad.dpad.right.wasPressedThisFrame) inputDir = 1;  // East
                if (gamepad.dpad.down.wasPressedThisFrame) inputDir = 2;   // South
                if (gamepad.dpad.left.wasPressedThisFrame) inputDir = 3;   // West
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

            // Apply skill tree tuning precision bonus
            float precisionMod = SkillTreeSystem.Instance?.GetModifier(SkillModifierType.TuningPrecision) ?? 0f;
            _accuracy = Mathf.Clamp01(_accuracy + precisionMod);

            try
            {
                if (_accuracy >= 0.6f)
                {
                    // Success! Return to exploration
                    OnTuningComplete?.Invoke(_accuracy);
                    AudioManager.Instance?.PlaySFX2D("TuningComplete");

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
                    AudioManager.Instance?.PlaySFX2D("TuningFailed");
                }
            }
            finally
            {
                HapticFeedbackManager.Instance?.StopAll();
                GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            }
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
