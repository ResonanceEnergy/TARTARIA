using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Input
{
    /// <summary>
    /// Gamepad Haptic Feedback Manager — translates game events into
    /// XInput (Xbox) / DualSense (PlayStation) rumble patterns.
    /// Budget: 0.1ms per frame.
    /// </summary>
    public class HapticFeedbackManager : MonoBehaviour
    {
        public static HapticFeedbackManager Instance { get; private set; }

        Gamepad _activeGamepad;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            _activeGamepad = Gamepad.current;
            UpdateActivePatterns();
        }

        void OnDisable()
        {
            StopAll();
        }

        // ─── Public API ──────────────────────────────

        public void PlayFootstep()
        {
            PlayPulse(0.3f, 0.02f);
        }

        public void PlayDiscovery()
        {
            PlayRamp(0.4f, 0.8f, 0.5f);
        }

        public void PlayTuningOnFrequency()
        {
            SetContinuous(0.5f, 0.0f);
        }

        public void PlayTuningOffFrequency()
        {
            SetContinuous(0.0f, 0.3f);
        }

        public void PlayPerfectTune()
        {
            PlayCascade(0.6f, 1.0f, 0.0f, 1.0f);
        }

        public void PlayBuildingEmergence()
        {
            PlayEmergencePattern();
        }

        public void PlayGolemSpawn()
        {
            PlayPulse(0.7f, 2.0f, lowFrequency: true);
        }

        public void PlayCombatHit()
        {
            PlayPulse(0.9f, 0.05f);
        }

        public void PlayGolemDeath()
        {
            PlayRamp(0.8f, 0.0f, 1.5f);
        }

        public void StopAll()
        {
            if (_activeGamepad != null)
                _activeGamepad.SetMotorSpeeds(0f, 0f);
            _currentPattern = null;
        }

        // ─── Haptic Pattern Engine ──────────────────

        HapticPattern _currentPattern;
        float _patternTime;

        void PlayPulse(float intensity, float duration, bool lowFrequency = false)
        {
            if (_activeGamepad == null) return;

            _currentPattern = new HapticPattern
            {
                Duration = duration,
                LowMotorStart = lowFrequency ? intensity : 0f,
                LowMotorEnd = 0f,
                HighMotorStart = lowFrequency ? 0f : intensity,
                HighMotorEnd = 0f
            };
            _patternTime = 0f;
        }

        void PlayRamp(float startIntensity, float endIntensity, float duration)
        {
            if (_activeGamepad == null) return;

            _currentPattern = new HapticPattern
            {
                Duration = duration,
                LowMotorStart = startIntensity,
                LowMotorEnd = endIntensity,
                HighMotorStart = startIntensity * 0.5f,
                HighMotorEnd = endIntensity * 0.5f
            };
            _patternTime = 0f;
        }

        void PlayCascade(float start, float peak, float end, float duration)
        {
            if (_activeGamepad == null) return;

            _currentPattern = new HapticPattern
            {
                Duration = duration,
                LowMotorStart = start,
                LowMotorEnd = end,
                HighMotorStart = peak,
                HighMotorEnd = end,
                IsCascade = true
            };
            _patternTime = 0f;
        }

        void PlayEmergencePattern()
        {
            // 5-second building emergence — per MVP spec
            if (_activeGamepad == null) return;

            _currentPattern = new HapticPattern
            {
                Duration = 5.0f,
                LowMotorStart = 0.3f,
                LowMotorEnd = 0.0f,
                HighMotorStart = 0.1f,
                HighMotorEnd = 0.0f,
                IsCascade = true
            };
            _patternTime = 0f;
        }

        void SetContinuous(float lowMotor, float highMotor)
        {
            if (_activeGamepad == null) return;
            _currentPattern = null;
            _activeGamepad.SetMotorSpeeds(lowMotor, highMotor);
        }

        void UpdateActivePatterns()
        {
            if (_currentPattern == null || _activeGamepad == null) return;

            _patternTime += Time.deltaTime;
            float t = Mathf.Clamp01(_patternTime / _currentPattern.Duration);

            float low, high;

            if (_currentPattern.IsCascade)
            {
                // Cascade: rise to peak at 0.8, then fall
                float peakT = 0.8f;
                if (t < peakT)
                {
                    float riseT = t / peakT;
                    low = Mathf.Lerp(_currentPattern.LowMotorStart, 1.0f, riseT);
                    high = Mathf.Lerp(_currentPattern.HighMotorStart, 0.8f, riseT);
                }
                else
                {
                    float fallT = (t - peakT) / (1f - peakT);
                    low = Mathf.Lerp(1.0f, _currentPattern.LowMotorEnd, fallT);
                    high = Mathf.Lerp(0.8f, _currentPattern.HighMotorEnd, fallT);
                }
            }
            else
            {
                low = Mathf.Lerp(_currentPattern.LowMotorStart, _currentPattern.LowMotorEnd, t);
                high = Mathf.Lerp(_currentPattern.HighMotorStart, _currentPattern.HighMotorEnd, t);
            }

            _activeGamepad.SetMotorSpeeds(low, high);

            if (_patternTime >= _currentPattern.Duration)
            {
                _activeGamepad.SetMotorSpeeds(0f, 0f);
                _currentPattern = null;
            }
        }

        class HapticPattern
        {
            public float Duration;
            public float LowMotorStart;
            public float LowMotorEnd;
            public float HighMotorStart;
            public float HighMotorEnd;
            public bool IsCascade;
        }

        // ─── Moon-Specific Haptic Profiles ───────────

        /// <summary>
        /// Play a context-sensitive haptic for a specific Moon.
        /// Each Moon has unique rumble signatures for its boss, environment, and climax.
        /// </summary>
        public void PlayMoonHaptic(int moonIndex, HapticContext context)
        {
            if (_activeGamepad == null) return;

            var profile = GetMoonProfile(moonIndex, context);
            _currentPattern = profile;
            _patternTime = 0f;
        }

        static HapticPattern GetMoonProfile(int moonIndex, HapticContext context)
        {
            // Base intensity scales with Moon progression
            float baseIntensity = 0.3f + moonIndex * 0.05f;
            baseIntensity = Mathf.Clamp(baseIntensity, 0.3f, 0.95f);

            return context switch
            {
                HapticContext.BossEntrance => new HapticPattern
                {
                    Duration = 2.5f,
                    LowMotorStart = baseIntensity,
                    LowMotorEnd = baseIntensity * 0.5f,
                    HighMotorStart = 0.2f,
                    HighMotorEnd = baseIntensity,
                    IsCascade = true
                },
                HapticContext.BossPhaseShift => new HapticPattern
                {
                    Duration = 1.5f,
                    LowMotorStart = baseIntensity * 0.8f,
                    LowMotorEnd = 0f,
                    HighMotorStart = baseIntensity,
                    HighMotorEnd = 0f,
                    IsCascade = false
                },
                HapticContext.EnvironmentShake => new HapticPattern
                {
                    Duration = 1.0f + moonIndex * 0.1f,
                    LowMotorStart = baseIntensity * 0.6f,
                    LowMotorEnd = 0f,
                    HighMotorStart = 0.1f,
                    HighMotorEnd = 0f,
                    IsCascade = false
                },
                HapticContext.ClimaxCinematic => new HapticPattern
                {
                    Duration = 5.0f,
                    LowMotorStart = 0.1f,
                    LowMotorEnd = 0f,
                    HighMotorStart = baseIntensity * 0.3f,
                    HighMotorEnd = baseIntensity,
                    IsCascade = true
                },
                _ => new HapticPattern
                {
                    Duration = 0.5f,
                    LowMotorStart = baseIntensity * 0.5f,
                    LowMotorEnd = 0f,
                    HighMotorStart = baseIntensity * 0.3f,
                    HighMotorEnd = 0f,
                    IsCascade = false
                }
            };
        }
    }

    public enum HapticContext : byte
    {
        BossEntrance = 0,
        BossPhaseShift = 1,
        EnvironmentShake = 2,
        ClimaxCinematic = 3,
        ZoneTransition = 4
    }
}
