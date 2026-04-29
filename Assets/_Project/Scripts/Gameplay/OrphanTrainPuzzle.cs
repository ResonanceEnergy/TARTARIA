using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Orphan Train Rail Alignment Puzzle — Moon 3 (Windswept Highlands) mini-game.
    ///
    /// Mechanic: Crystallised Aether rail segments must be re-tuned to connect
    /// the orphan train network. Player aligns segments by matching resonance
    /// frequencies along a multi-segment track.
    ///
    /// Flow:
    ///   1. Display N rail segments (3-7 depending on difficulty)
    ///   2. Each segment has a current frequency and a target frequency
    ///   3. Player adjusts each segment's frequency via dial input
    ///   4. Connected segments create visible golden energy paths
    ///   5. All segments aligned → train materialises on the track
    ///
    /// Scoring:
    ///   - Per-segment accuracy (how close to target Hz)
    ///   - Time bonus (faster = more RS)
    ///   - Chain bonus: sequential perfect alignments (within 2 Hz)
    ///   - Orphan lullaby bonus: children singing boosts zone healing
    ///
    /// Design ref: GDD §03C Moon 3 — Resonance Rail Reactivation
    /// </summary>
    public class OrphanTrainPuzzle : MonoBehaviour
    {
        [Header("Puzzle Config")]
        [SerializeField] int segmentCount = 5;
        [SerializeField] float timeLimitSeconds = 45f;
        [SerializeField] float perfectThresholdHz = 2f;
        [SerializeField] float goodThresholdHz = 8f;

        [Header("Frequency Range")]
        [SerializeField] float minFrequency = 400f;
        [SerializeField] float maxFrequency = 460f;
        [SerializeField] float targetBaseFrequency = 432f;

        [Header("Scoring")]
        [SerializeField] float perfectSegmentRS = 3f;
        [SerializeField] float goodSegmentRS = 1.5f;
        [SerializeField] float chainBonusMultiplier = 0.25f;
        [SerializeField] float timeBonusMaxRS = 5f;

        [Header("Input")]
        [SerializeField, Min(1f)] float controllerAdjustHzPerSecond = 24f;

        // ─── State ───
        RailSegment[] _segments;
        int _activeSegment;
        float _timer;
        bool _isActive;
        bool _completed;
        int _perfectChain;
        int _bestChain;
        int _perfectCount;

        public bool IsActive => _isActive;
        public bool IsCompleted => _completed;
        public int ActiveSegment => _activeSegment;
        public float TimeRemaining => Mathf.Max(0f, timeLimitSeconds - _timer);
        public int PerfectChain => _perfectChain;

        public event System.Action<float> OnPuzzleCompleted;   // total RS reward
        public event System.Action OnPuzzleFailed;
        public event System.Action<int, float> OnSegmentAligned;  // index, accuracy
        public event System.Action<int> OnChainBroken;

        void Update()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;
            if (_timer >= timeLimitSeconds)
            {
                FailPuzzle();
                return;
            }

            // Keyboard fallback for desktop playtesting
            var kb = Keyboard.current;
            if (kb != null)
            {
                float axis = 0f;
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) axis -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) axis += 1f;
                if (Mathf.Abs(axis) > 0.01f)
                    AdjustFrequency(axis * controllerAdjustHzPerSecond * Time.deltaTime);

                if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                    ConfirmSegment();
            }

            // Gamepad controls: right-stick Y tunes, A confirms
            var pad = Gamepad.current;
            if (pad != null)
            {
                float axis = pad.rightStick.ReadValue().y;
                if (Mathf.Abs(axis) > 0.2f)
                    AdjustFrequency(axis * controllerAdjustHzPerSecond * Time.deltaTime);

                if (pad.buttonSouth.wasPressedThisFrame)
                    ConfirmSegment();
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Start the rail alignment puzzle.</summary>
        public void StartPuzzle()
        {
            StartPuzzle(OrphanTrainPuzzleConfig.Default(segmentCount, timeLimitSeconds));
        }

        /// <summary>Start with a specific config.</summary>
        public void StartPuzzle(OrphanTrainPuzzleConfig config)
        {
            if (_isActive) return;

            segmentCount = config.segmentCount;
            timeLimitSeconds = config.timeLimit;
            perfectThresholdHz = config.perfectThreshold;

            _segments = new RailSegment[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                // Each segment has a different target (harmonic series around 432 Hz)
                float harmonicOffset = (i - segmentCount / 2f) * 12f;
                float target = targetBaseFrequency + harmonicOffset;

                // Random starting frequency (detuned)
                float start = Random.Range(minFrequency, maxFrequency);

                _segments[i] = new RailSegment
                {
                    targetFrequency = target,
                    currentFrequency = start,
                    isAligned = false
                };
            }

            _activeSegment = 0;
            _timer = 0f;
            _perfectChain = 0;
            _bestChain = 0;
            _perfectCount = 0;
            _isActive = true;
            _completed = false;

            Debug.Log($"[OrphanTrain] Puzzle started: {segmentCount} segments, {timeLimitSeconds}s limit");
        }

        /// <summary>
        /// Adjust the active segment's frequency. Called by player input.
        /// Delta in Hz per frame (positive = tune up, negative = tune down).
        /// </summary>
        public void AdjustFrequency(float deltaHz)
        {
            if (!_isActive || _activeSegment >= segmentCount) return;

            ref var seg = ref _segments[_activeSegment];
            seg.currentFrequency = Mathf.Clamp(seg.currentFrequency + deltaHz,
                minFrequency, maxFrequency);
        }

        /// <summary>
        /// Lock in the current segment's frequency and evaluate accuracy.
        /// </summary>
        public void ConfirmSegment()
        {
            if (!_isActive || _activeSegment >= segmentCount) return;

            ref var seg = ref _segments[_activeSegment];
            float diff = Mathf.Abs(seg.currentFrequency - seg.targetFrequency);

            if (diff <= perfectThresholdHz)
            {
                seg.isAligned = true;
                seg.accuracy = 1f;
                _perfectChain++;
                _perfectCount++;
                if (_perfectChain > _bestChain) _bestChain = _perfectChain;
            }
            else if (diff <= goodThresholdHz)
            {
                seg.isAligned = true;
                seg.accuracy = 1f - (diff - perfectThresholdHz) / (goodThresholdHz - perfectThresholdHz);
                _perfectChain = 0;
                OnChainBroken?.Invoke(_activeSegment);
            }
            else
            {
                // Failed segment — still counts but no RS
                seg.isAligned = false;
                seg.accuracy = 0f;
                _perfectChain = 0;
                OnChainBroken?.Invoke(_activeSegment);
            }

            OnSegmentAligned?.Invoke(_activeSegment, seg.accuracy);
            _activeSegment++;

            // Check completion
            if (_activeSegment >= segmentCount)
                CompletePuzzle();
        }

        /// <summary>Get current segment data for UI display.</summary>
        public RailSegment GetSegment(int index)
        {
            if (index < 0 || index >= segmentCount) return default;
            return _segments[index];
        }

        /// <summary>Get all segment data.</summary>
        public RailSegment[] GetAllSegments()
        {
            return _segments ?? System.Array.Empty<RailSegment>();
        }

        // ─── Internal ────────────────────────────────

        void CompletePuzzle()
        {
            _isActive = false;
            _completed = true;

            float totalRS = 0f;
            int alignedCount = 0;

            for (int i = 0; i < segmentCount; i++)
            {
                if (_segments[i].accuracy >= 1f)
                {
                    totalRS += perfectSegmentRS;
                    alignedCount++;
                }
                else if (_segments[i].accuracy > 0f)
                {
                    totalRS += goodSegmentRS * _segments[i].accuracy;
                    alignedCount++;
                }
            }

            // Chain bonus
            totalRS += _bestChain * chainBonusMultiplier * perfectSegmentRS;

            // Time bonus (proportional to remaining time)
            float timeRatio = timeLimitSeconds > 0f ? 1f - (_timer / timeLimitSeconds) : 0f;
            totalRS += timeBonusMaxRS * timeRatio;

            // Golden ratio bonus: if all segments perfect
            if (_perfectCount == segmentCount)
                totalRS *= 1.618f;

            Debug.Log($"[OrphanTrain] Puzzle complete! {alignedCount}/{segmentCount} aligned, " +
                       $"chain {_bestChain}, RS reward {totalRS:F1}");

            // Notify companions
            ServiceLocator.Milo?.AddTrust(2f);
            ServiceLocator.Lirael?.AddTrust(3f);

            // Trigger story beat if first completion on this zone
            ServiceLocator.Milo?.WitnessOrphanTrain();
            ServiceLocator.Lirael?.RememberOrphanTrain();

            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            ServiceLocator.GameLoop?.OnMiniGameCompleted(totalRS, "orphan_train");
            OnPuzzleCompleted?.Invoke(totalRS);
            Audio.AudioManager.Instance?.PlaySFX2D("TrainRestored");
        }

        void FailPuzzle()
        {
            _isActive = false;
            Debug.Log("[OrphanTrain] Puzzle failed — time expired");
            OnPuzzleFailed?.Invoke();
            Audio.AudioManager.Instance?.PlaySFX2D("TrainFailed");
        }
    }

    // ─── Data Types ──────────────────────────────

    public struct RailSegment
    {
        public float targetFrequency;
        public float currentFrequency;
        public float accuracy;
        public bool isAligned;
    }

    [System.Serializable]
    public class OrphanTrainPuzzleConfig
    {
        public int segmentCount;
        public float timeLimit;
        public float perfectThreshold;

        public static OrphanTrainPuzzleConfig Default(int segments = 5, float time = 45f)
        {
            return new OrphanTrainPuzzleConfig
            {
                segmentCount = segments,
                timeLimit = time,
                perfectThreshold = 2f
            };
        }

        public static OrphanTrainPuzzleConfig Easy()
        {
            return new OrphanTrainPuzzleConfig
            {
                segmentCount = 3,
                timeLimit = 60f,
                perfectThreshold = 4f
            };
        }

        public static OrphanTrainPuzzleConfig Hard()
        {
            return new OrphanTrainPuzzleConfig
            {
                segmentCount = 7,
                timeLimit = 35f,
                perfectThreshold = 1.5f
            };
        }
    }
}
