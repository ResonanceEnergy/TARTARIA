using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Pipe Organ Mini-Game — Moon 2 (Crystalline Caverns) harmonic puzzle.
    ///
    /// Design per GDD §13 (Mini-Games):
    ///   Players reconstruct a Tartarian pipe organ by matching harmonic
    ///   chord progressions. Each pipe corresponds to a frequency, and
    ///   players must activate pipes in golden-ratio intervals to produce
    ///   the correct resonance pattern.
    ///
    /// Mechanics:
    ///   - 7 pipes (C D E F G A B mapped to keys 1-7)
    ///   - Target chord progression displayed as glowing pipe highlights
    ///   - Player must reproduce the 5-chord sequence in order
    ///   - Timing window per chord: ±400ms generous, ±200ms great, ±100ms perfect
    ///   - Wrong pipe breaks the combo and adds a penalty
    ///   - RS reward scales with accuracy
    ///   - Time limit: 30s
    ///
    /// Scoring:
    ///   Perfect (>95%) = ×φ (1.618), Great (80–95%) = ×1.3,
    ///   Good (60–80%) = ×1.0, Fail (<60%) = retry
    /// </summary>
    public class PipeOrganMiniGame : MonoBehaviour
    {
        public static PipeOrganMiniGame Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] float timeLimit = 30f;
        [SerializeField] float baseRSReward = 12f;

        // ─── Events ───
        public event System.Action<float> OnOrganComplete;  // accuracy 0-1
        public event System.Action OnOrganFailed;
        public event System.Action<int, bool> OnPipePlayed;  // pipeIndex, correct
        public event System.Action<int> OnChordAdvanced;     // chordIndex

        // ─── State ───
        bool _isActive;
        float _timeRemaining;
        int _currentChord;
        int _currentPipeInChord;
        int _correctNotes;
        int _totalNotes;
        int _combo;
        int _maxCombo;

        // Chord progression data
        int[][] _chordProgression;
        float[] _chordTiming;    // expected time for each chord
        float _startTime;

        // Pipe frequencies (Tartarian harmonic series based on 432Hz)
        static readonly float[] PipeFrequencies =
        {
            432.0f,  // C (Tartarian root)
            486.0f,  // D
            513.7f,  // E (golden interval)
            576.0f,  // F
            648.0f,  // G
            699.0f,  // A (φ × 432)
            769.5f   // B
        };

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Start / Stop ────────────────────────────

        public void StartOrgan(PipeOrganConfig config = null)
        {
            config ??= PipeOrganConfig.Default();
            _timeRemaining = config.timeLimit;
            _currentChord = 0;
            _currentPipeInChord = 0;
            _correctNotes = 0;
            _totalNotes = 0;
            _combo = 0;
            _maxCombo = 0;
            _isActive = true;
            _startTime = Time.time;

            GenerateProgression(config.difficulty);

            GameStateManager.Instance?.TransitionTo(GameState.Tuning);
        }

        public void AbortOrgan()
        {
            _isActive = false;
            OnOrganFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
        }

        void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                FinishOrgan();
                return;
            }

            HandlePipeInput();
        }

        // ─── Input ───────────────────────────────────

        void HandlePipeInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            int pipePressed = -1;
            if (keyboard.digit1Key.wasPressedThisFrame) pipePressed = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame) pipePressed = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame) pipePressed = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame) pipePressed = 3;
            else if (keyboard.digit5Key.wasPressedThisFrame) pipePressed = 4;
            else if (keyboard.digit6Key.wasPressedThisFrame) pipePressed = 5;
            else if (keyboard.digit7Key.wasPressedThisFrame) pipePressed = 6;

            if (pipePressed < 0) return;
            if (_currentChord >= _chordProgression.Length) return;

            var chord = _chordProgression[_currentChord];
            int expectedPipe = chord[_currentPipeInChord];
            bool correct = pipePressed == expectedPipe;

            // Play the pipe tone
            float freq = PipeFrequencies[pipePressed];
            Audio.AudioManager.Instance?.PlayTone(freq, 0.4f);

            _totalNotes++;

            if (correct)
            {
                _correctNotes++;
                _combo++;
                if (_combo > _maxCombo) _maxCombo = _combo;

                // Timing bonus
                float expectedTime = _chordTiming[_currentChord]
                    + _currentPipeInChord * 0.5f;
                float timing = Mathf.Abs(Time.time - _startTime - expectedTime);
                if (timing <= 0.1f)
                    _correctNotes += 2; // perfect timing bonus

                // Haptic feedback
                HapticFeedbackManager.Instance?.PlayTuningOnFrequency();

                OnPipePlayed?.Invoke(pipePressed, true);

                _currentPipeInChord++;
                if (_currentPipeInChord >= chord.Length)
                {
                    // Chord complete
                    _currentChord++;
                    _currentPipeInChord = 0;
                    OnChordAdvanced?.Invoke(_currentChord);

                    if (_currentChord >= _chordProgression.Length)
                    {
                        FinishOrgan();
                        return;
                    }
                }
            }
            else
            {
                // Wrong pipe
                _combo = 0;
                HapticFeedbackManager.Instance?.PlayTuningOffFrequency();
                OnPipePlayed?.Invoke(pipePressed, false);
            }
        }

        // ─── Completion ──────────────────────────────

        void FinishOrgan()
        {
            _isActive = false;
            int maxPossible = 0;
            foreach (var chord in _chordProgression)
                maxPossible += chord.Length * 3; // 3 points for perfect timing

            float accuracy = maxPossible > 0
                ? (float)_correctNotes / maxPossible
                : 0f;

            // Normalize to 0-1 range
            accuracy = Mathf.Clamp01(accuracy * 2f); // Scale up since perfect timing is hard

            if (accuracy < 0.6f)
            {
                OnOrganFailed?.Invoke();
            }
            else
            {
                float multiplier = accuracy >= 0.95f
                    ? GoldenRatioValidator.PHI
                    : accuracy >= 0.80f ? 1.3f : 1.0f;

                float comboBonus = 1f + _maxCombo * 0.05f;
                float rsReward = baseRSReward * accuracy * multiplier * comboBonus;

                AetherFieldManager.Instance?.AddResonanceScore(rsReward);
                OnOrganComplete?.Invoke(accuracy);
            }

            GameStateManager.Instance?.ReturnToPrevious();
        }

        // ─── Chord Generation ────────────────────────

        void GenerateProgression(int difficulty)
        {
            int chordCount = 3 + difficulty; // 3-6 chords
            int notesPerChord = 2 + difficulty / 2; // 2-4 notes

            _chordProgression = new int[chordCount][];
            _chordTiming = new float[chordCount];

            float timePerChord = (timeLimit * 0.8f) / chordCount;

            for (int c = 0; c < chordCount; c++)
            {
                _chordProgression[c] = new int[notesPerChord];
                _chordTiming[c] = timePerChord * c + 1f; // 1s lead-in

                for (int n = 0; n < notesPerChord; n++)
                {
                    // Generate pipes using golden-ratio intervals
                    int basePipe = (c * 3 + n) % 7;
                    int goldenStep = Mathf.RoundToInt(n * GoldenRatioValidator.PHI) % 7;
                    _chordProgression[c][n] = (basePipe + goldenStep) % 7;
                }
            }
        }

        // ─── Public Getters ──────────────────────────

        public bool IsActive => _isActive;
        public float TimeRemaining => _timeRemaining;
        public int CurrentChord => _currentChord;
        public int TotalChords => _chordProgression?.Length ?? 0;
        public int Combo => _combo;
        public int[][] ChordProgression => _chordProgression;
    }

    // ─── Config ──────────────────────────────────

    [System.Serializable]
    public class PipeOrganConfig
    {
        public int difficulty;   // 0=easy, 1=medium, 2=hard, 3=expert
        public float timeLimit;
        public float baseRSReward;

        public static PipeOrganConfig Default() => new()
        {
            difficulty = 1,
            timeLimit = 30f,
            baseRSReward = 12f
        };

        public static PipeOrganConfig Easy() => new()
        {
            difficulty = 0,
            timeLimit = 35f,
            baseRSReward = 8f
        };

        public static PipeOrganConfig Hard() => new()
        {
            difficulty = 2,
            timeLimit = 25f,
            baseRSReward = 20f
        };

        public static PipeOrganConfig Expert() => new()
        {
            difficulty = 3,
            timeLimit = 20f,
            baseRSReward = 35f
        };
    }
}
