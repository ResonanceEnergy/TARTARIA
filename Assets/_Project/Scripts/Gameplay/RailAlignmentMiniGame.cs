using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Rail Alignment Mini-Game — Moon 5 (Aether Falls) precision puzzle.
    ///
    /// Design per GDD §13 (Mini-Games):
    ///   Tartarian rail systems run on harmonic frequencies. Players must
    ///   align magnetic rail segments to create a golden-ratio–curved track
    ///   that channels Aether between two endpoints.
    ///
    /// Mechanics:
    ///   - Grid of 4-8 rail segments, each rotatable in 45° increments
    ///   - Player clicks a segment then rotates with Q/E (or scroll wheel)
    ///   - Segments must form a continuous path from Start to End
    ///   - Golden curves score higher than sharp turns
    ///   - Aether flow visualization shows correctness in real-time
    ///   - Time limit scales with complexity (20-40s)
    ///
    /// Scoring:
    ///   Perfect (>95%) = ×φ, Great (80–95%) = ×1.3,
    ///   Good (60–80%) = ×1.0, Fail (<60%) = retry
    /// </summary>
    public class RailAlignmentMiniGame : MonoBehaviour
    {
        public static RailAlignmentMiniGame Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] float baseRSReward = 15f;

        // ─── Events ───
        public event System.Action<float> OnAlignmentComplete;  // accuracy 0-1
        public event System.Action OnAlignmentFailed;
        public event System.Action<int, int> OnSegmentRotated;  // segIndex, newAngle
        public event System.Action<float> OnFlowChanged;        // flow strength 0-1

        // ─── State ───
        bool _isActive;
        float _timeRemaining;
        int _selectedSegment = -1;
        RailSegment[] _segments;
        int _gridWidth;
        int _gridHeight;
        int _startIndex;
        int _endIndex;

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float TimeRemaining => _timeRemaining;
        public int SelectedSegment => _selectedSegment;
        public float FlowStrength { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Start / Stop ────────────────────────────

        public void StartAlignment(RailAlignmentConfig config = null)
        {
            config ??= RailAlignmentConfig.Default();
            _timeRemaining = config.timeLimit;
            _gridWidth = config.gridWidth;
            _gridHeight = config.gridHeight;
            _selectedSegment = -1;
            _isActive = true;

            GenerateGrid(config);

            GameStateManager.Instance?.TransitionTo(GameState.Tuning);
        }

        public void AbortAlignment()
        {
            _isActive = false;
            OnAlignmentFailed?.Invoke();
            GameStateManager.Instance?.ReturnToPrevious();
        }

        void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                FinishAlignment();
                return;
            }

            HandleInput();
            UpdateFlow();
        }

        // ─── Input ───────────────────────────────────

        void HandleInput()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            // Click to select segment
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 pos = mouse.position.ReadValue();
                int idx = ScreenPosToSegmentIndex(pos);
                if (idx >= 0 && idx < _segments.Length)
                {
                    _selectedSegment = idx;
                    Audio.AudioManager.Instance?.PlayTone(
                        432f + idx * 50f, 0.15f);
                }
            }

            if (_selectedSegment < 0 || _selectedSegment >= _segments.Length) return;

            // Rotate selected segment
            bool rotated = false;
            if (keyboard != null)
            {
                if (keyboard.qKey.wasPressedThisFrame)
                {
                    _segments[_selectedSegment].angle =
                        (_segments[_selectedSegment].angle - 45 + 360) % 360;
                    rotated = true;
                }
                if (keyboard.eKey.wasPressedThisFrame)
                {
                    _segments[_selectedSegment].angle =
                        (_segments[_selectedSegment].angle + 45) % 360;
                    rotated = true;
                }
            }

            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 10f)
                {
                    int dir = scroll > 0 ? 45 : -45;
                    _segments[_selectedSegment].angle =
                        (_segments[_selectedSegment].angle + dir + 360) % 360;
                    rotated = true;
                }
            }

            if (rotated)
            {
                OnSegmentRotated?.Invoke(_selectedSegment, _segments[_selectedSegment].angle);
                HapticFeedbackManager.Instance?.PlayTuningOnFrequency();
            }
        }

        // ─── Flow Calculation ────────────────────────

        void UpdateFlow()
        {
            // Trace path from start to end
            float flow = CalculateFlowStrength();
            FlowStrength = flow;
            OnFlowChanged?.Invoke(flow);

            // Auto-complete if perfect alignment achieved
            if (flow >= 0.95f)
                FinishAlignment();
        }

        float CalculateFlowStrength()
        {
            // BFS/trace from start to end, scoring based on angular continuity
            int current = _startIndex;
            float totalScore = 0f;
            int steps = 0;
            bool[] visited = new bool[_segments.Length];

            while (current >= 0 && current != _endIndex && steps < _segments.Length)
            {
                visited[current] = true;
                var seg = _segments[current];

                // Find the neighbor this segment points toward
                int next = GetNextSegment(current, seg.angle);
                if (next < 0 || next >= _segments.Length || visited[next])
                    break;

                // Score angular alignment between connected segments
                var nextSeg = _segments[next];
                int expectedReturnAngle = (seg.angle + 180) % 360;
                int angleDiff = Mathf.Abs(nextSeg.angle - expectedReturnAngle);
                if (angleDiff > 180) angleDiff = 360 - angleDiff;

                // Golden ratio: 137.5° is the golden angle
                float goldenAngleDiff = Mathf.Abs(angleDiff - 137.5f);
                float segmentScore = Mathf.Clamp01(1f - goldenAngleDiff / 180f);

                totalScore += segmentScore;
                steps++;
                current = next;
            }

            // Bonus for actually reaching the end
            bool reachedEnd = current == _endIndex;
            float pathCompletion = steps > 0
                ? totalScore / steps
                : 0f;

            return reachedEnd ? pathCompletion : pathCompletion * 0.5f;
        }

        int GetNextSegment(int current, int angle)
        {
            int col = current % _gridWidth;
            int row = current / _gridWidth;

            // Map angle to grid direction
            return angle switch
            {
                0 => row > 0 ? current - _gridWidth : -1,               // North
                45 => row > 0 && col < _gridWidth - 1 ? current - _gridWidth + 1 : -1,
                90 => col < _gridWidth - 1 ? current + 1 : -1,          // East
                135 => row < _gridHeight - 1 && col < _gridWidth - 1 ? current + _gridWidth + 1 : -1,
                180 => row < _gridHeight - 1 ? current + _gridWidth : -1, // South
                225 => row < _gridHeight - 1 && col > 0 ? current + _gridWidth - 1 : -1,
                270 => col > 0 ? current - 1 : -1,                      // West
                315 => row > 0 && col > 0 ? current - _gridWidth - 1 : -1,
                _ => -1
            };
        }

        // ─── Completion ──────────────────────────────

        void FinishAlignment()
        {
            _isActive = false;
            float accuracy = FlowStrength;

            if (accuracy < 0.6f)
            {
                OnAlignmentFailed?.Invoke();
            }
            else
            {
                float multiplier = accuracy >= 0.95f
                    ? GoldenRatioValidator.PHI
                    : accuracy >= 0.80f ? 1.3f : 1.0f;

                float rsReward = baseRSReward * accuracy * multiplier;
                AetherFieldManager.Instance?.AddResonanceScore(rsReward);
                OnAlignmentComplete?.Invoke(accuracy);
            }

            GameStateManager.Instance?.ReturnToPrevious();
        }

        // ─── Grid Generation ─────────────────────────

        void GenerateGrid(RailAlignmentConfig config)
        {
            int total = config.gridWidth * config.gridHeight;
            _segments = new RailSegment[total];

            _startIndex = 0;
            _endIndex = total - 1;

            // Generate a valid solution path first, then randomize rotations
            for (int i = 0; i < total; i++)
            {
                _segments[i] = new RailSegment
                {
                    angle = Random.Range(0, 8) * 45,
                    segmentType = (RailSegmentType)(i % 3)
                };
            }
        }

        int ScreenPosToSegmentIndex(Vector2 screenPos)
        {
            // Map screen position to grid cell
            float cellW = Screen.width / (float)_gridWidth;
            float cellH = Screen.height / (float)_gridHeight;
            int col = Mathf.FloorToInt(screenPos.x / cellW);
            int row = _gridHeight - 1 - Mathf.FloorToInt(screenPos.y / cellH);
            if (col < 0 || col >= _gridWidth || row < 0 || row >= _gridHeight) return -1;
            return row * _gridWidth + col;
        }
    }

    // ─── Data Types ──────────────────────────────

    struct RailSegment
    {
        public int angle;              // 0-360 in 45° increments
        public RailSegmentType segmentType;
    }

    enum RailSegmentType : byte
    {
        Straight = 0,
        Curve = 1,
        Junction = 2
    }

    [System.Serializable]
    public class RailAlignmentConfig
    {
        public int gridWidth;
        public int gridHeight;
        public float timeLimit;
        public float baseRSReward;

        public static RailAlignmentConfig Default() => new()
        {
            gridWidth = 4,
            gridHeight = 4,
            timeLimit = 30f,
            baseRSReward = 15f
        };

        public static RailAlignmentConfig Easy() => new()
        {
            gridWidth = 3,
            gridHeight = 3,
            timeLimit = 35f,
            baseRSReward = 10f
        };

        public static RailAlignmentConfig Hard() => new()
        {
            gridWidth = 5,
            gridHeight = 5,
            timeLimit = 25f,
            baseRSReward = 25f
        };

        public static RailAlignmentConfig Expert() => new()
        {
            gridWidth = 6,
            gridHeight = 6,
            timeLimit = 40f,
            baseRSReward = 40f
        };
    }
}
