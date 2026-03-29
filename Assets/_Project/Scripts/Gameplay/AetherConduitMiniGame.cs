using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Aether Conduit Mini-Game — Moon 4 (Star Fort Bastion) precision geometry puzzle.
    ///
    /// Mechanic: Place bastion points on a ley-line grid. Each placement must
    /// conform to golden-ratio (φ = 1.618…) proportions relative to existing
    /// anchors. Precision determines resonance amplification bonus.
    ///
    /// Flow:
    ///   1. Grid of ley-line vectors displayed with anchor nodes
    ///   2. Player places bastion points — snapping guides show φ-ratio ranges
    ///   3. Precision Rock Cutting phase: hold-and-release timing for each wall
    ///   4. All bastions placed → star fort router activates
    ///
    /// Scoring:
    ///   - φ-alignment accuracy per bastion (how close to 1.618 ratio)
    ///   - Rock cut precision (timing window hit)
    ///   - Structural integrity composite score
    ///   - Korath echo bonus (+30% if all placements ≥ 90% accuracy)
    ///
    /// Design ref: GDD §03C Moon 4 — Golden-Ratio Integrity System,
    ///             Star Fort Bastion Construction, Bell Tower Resonance Anchor
    /// </summary>
    public class AetherConduitMiniGame : MonoBehaviour
    {
        [Header("Puzzle Config")]
        [SerializeField] int bastionCount = 5;
        [SerializeField] float timeLimitSeconds = 60f;
        [SerializeField] float phiPerfectThreshold = 0.02f;    // tolerance from 1.618
        [SerializeField] float phiGoodThreshold = 0.08f;

        [Header("Rock Cutting")]
        [SerializeField] float cutWindowStart = 0.45f;         // normalised peak start
        [SerializeField] float cutWindowEnd = 0.55f;           // normalised peak end
        [SerializeField] float resonanceBuildRate = 1.2f;      // per second

        [Header("Scoring")]
        [SerializeField] float perfectBastionRS = 4f;
        [SerializeField] float goodBastionRS = 2f;
        [SerializeField] float korathBonusMultiplier = 1.3f;
        [SerializeField] float timeBonusMaxRS = 6f;

        const float PHI = 1.6180339887f;

        // ─── State ───
        BastionNode[] _bastions;
        Vector2[] _anchors;                // pre-existing anchor positions
        int _currentBastion;
        float _timer;
        bool _isActive;
        bool _completed;

        // Rock-cutting sub-phase
        bool _isCutting;
        float _cutResonance;               // 0→1→0 oscillation
        int _cutDirection;                  // +1 building, -1 falling

        public bool IsActive => _isActive;
        public bool IsCompleted => _completed;
        public int CurrentBastion => _currentBastion;
        public float TimeRemaining => Mathf.Max(0f, timeLimitSeconds - _timer);
        public bool IsCutting => _isCutting;
        public float CutResonance => _cutResonance;

        public event System.Action<float> OnPuzzleCompleted;
        public event System.Action OnPuzzleFailed;
        public event System.Action<int, float> OnBastionPlaced;     // index, accuracy
        public event System.Action<int, float> OnRockCut;           // index, cut quality
        public event System.Action OnResonancePeak;                 // visual/haptic cue

        void Update()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;
            if (_timer >= timeLimitSeconds)
            {
                FailPuzzle();
                return;
            }

            if (_isCutting)
                UpdateCutResonance();
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Start the conduit placement puzzle.</summary>
        public void StartPuzzle()
        {
            StartPuzzle(bastionCount, timeLimitSeconds);
        }

        /// <summary>Start with specific parameters.</summary>
        public void StartPuzzle(int bastions, float timeLimit)
        {
            bastionCount = bastions;
            timeLimitSeconds = timeLimit;

            // Generate anchor positions (existing ley-line nodes)
            _anchors = new Vector2[bastionCount + 2];
            _anchors[0] = Vector2.zero;    // central tower
            _anchors[1] = new Vector2(PHI, 0f);
            for (int i = 2; i < _anchors.Length; i++)
            {
                float angle = (i - 1) * (2f * Mathf.PI / (bastionCount + 1));
                _anchors[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * PHI * (1f + i * 0.3f);
            }

            _bastions = new BastionNode[bastionCount];
            for (int i = 0; i < bastionCount; i++)
            {
                _bastions[i] = new BastionNode
                {
                    targetPosition = ComputeIdealBastionPosition(i),
                    placedPosition = Vector2.zero,
                    phiAccuracy = 0f,
                    cutQuality = 0f,
                    isPlaced = false,
                    isCut = false
                };
            }

            _currentBastion = 0;
            _timer = 0f;
            _isActive = true;
            _completed = false;
            _isCutting = false;

            Debug.Log($"[AetherConduit] Puzzle started: {bastionCount} bastions, {timeLimitSeconds}s");
        }

        /// <summary>
        /// Place the current bastion at given grid position.
        /// Evaluates φ-ratio accuracy relative to nearest anchors.
        /// </summary>
        public void PlaceBastion(Vector2 position)
        {
            if (!_isActive || _isCutting || _currentBastion >= bastionCount) return;

            ref var bastion = ref _bastions[_currentBastion];
            bastion.placedPosition = position;

            // Evaluate phi-ratio accuracy
            float ratio = ComputePhiRatio(position);
            float deviation = Mathf.Abs(ratio - PHI);
            bastion.phiAccuracy = deviation <= phiPerfectThreshold ? 1f
                : deviation <= phiGoodThreshold ? 1f - (deviation - phiPerfectThreshold) / (phiGoodThreshold - phiPerfectThreshold)
                : 0f;
            bastion.isPlaced = true;

            OnBastionPlaced?.Invoke(_currentBastion, bastion.phiAccuracy);

            // Enter rock cutting sub-phase
            _isCutting = true;
            _cutResonance = 0f;
            _cutDirection = 1;
        }

        /// <summary>
        /// Release cut during rock-cutting phase. Timing determines quality.
        /// </summary>
        public void ReleaseCut()
        {
            if (!_isActive || !_isCutting) return;

            ref var bastion = ref _bastions[_currentBastion];

            // Evaluate cut timing — peak is best
            if (_cutResonance >= cutWindowStart && _cutResonance <= cutWindowEnd)
            {
                bastion.cutQuality = 1f;    // perfect cut
            }
            else
            {
                // Partial quality based on distance from peak window
                float center = (cutWindowStart + cutWindowEnd) * 0.5f;
                float dist = Mathf.Abs(_cutResonance - center);
                bastion.cutQuality = Mathf.Clamp01(1f - dist * 2f);
            }

            bastion.isCut = true;
            _isCutting = false;

            OnRockCut?.Invoke(_currentBastion, bastion.cutQuality);

            _currentBastion++;
            if (_currentBastion >= bastionCount)
                CompletePuzzle();
        }

        /// <summary>Get bastion data for UI.</summary>
        public BastionNode GetBastion(int index)
        {
            if (index < 0 || index >= bastionCount) return default;
            return _bastions[index];
        }

        /// <summary>Get all anchor positions for grid display.</summary>
        public Vector2[] GetAnchors()
        {
            return _anchors ?? System.Array.Empty<Vector2>();
        }

        /// <summary>Get ideal position for current bastion (for guide overlay).</summary>
        public Vector2 GetCurrentIdealPosition()
        {
            if (_currentBastion >= bastionCount) return Vector2.zero;
            return _bastions[_currentBastion].targetPosition;
        }

        // ─── Internal ────────────────────────────────

        Vector2 ComputeIdealBastionPosition(int index)
        {
            // Star fort points at φ-ratio distances from centre, evenly distributed
            float angle = index * (2f * Mathf.PI / bastionCount);
            float radius = PHI * 2f;   // outer ring at 2φ from centre
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        float ComputePhiRatio(Vector2 position)
        {
            // Ratio of distance-to-nearest-anchor / distance-to-centre
            float distCentre = Vector2.Distance(position, _anchors[0]);
            if (distCentre < 0.001f) return 0f;

            float minAnchorDist = float.MaxValue;
            for (int i = 1; i < _anchors.Length; i++)
            {
                float d = Vector2.Distance(position, _anchors[i]);
                if (d < minAnchorDist) minAnchorDist = d;
            }

            return distCentre / Mathf.Max(minAnchorDist, 0.001f);
        }

        void UpdateCutResonance()
        {
            _cutResonance += _cutDirection * resonanceBuildRate * Time.deltaTime;

            if (_cutResonance >= 1f)
            {
                _cutResonance = 1f;
                _cutDirection = -1;
            }
            else if (_cutResonance <= 0f)
            {
                // Over-held — auto-fail this cut
                _cutResonance = 0f;
                _isCutting = false;

                ref var bastion = ref _bastions[_currentBastion];
                bastion.cutQuality = 0f;
                bastion.isCut = true;

                OnRockCut?.Invoke(_currentBastion, 0f);
                _currentBastion++;
                if (_currentBastion >= bastionCount)
                    CompletePuzzle();
            }

            // Fire peak event for haptic/visual cue
            if (_cutDirection == 1 && _cutResonance >= cutWindowStart && _cutResonance < cutWindowStart + 0.02f)
                OnResonancePeak?.Invoke();
        }

        void CompletePuzzle()
        {
            _isActive = false;
            _completed = true;

            float totalRS = 0f;
            int perfectCount = 0;

            for (int i = 0; i < bastionCount; i++)
            {
                float composite = (_bastions[i].phiAccuracy * 0.6f + _bastions[i].cutQuality * 0.4f);

                if (composite >= 0.9f)
                {
                    totalRS += perfectBastionRS;
                    perfectCount++;
                }
                else if (composite > 0f)
                {
                    totalRS += goodBastionRS * composite;
                }
            }

            // Time bonus
            float timeRatio = 1f - (_timer / timeLimitSeconds);
            totalRS += timeBonusMaxRS * timeRatio;

            // Korath echo bonus: all bastions ≥ 90%
            if (perfectCount == bastionCount)
            {
                totalRS *= korathBonusMultiplier;
                Debug.Log("[AetherConduit] Korath echo bonus applied — flawless geometry!");
            }

            // Golden ratio multiplier for perfect scores
            if (perfectCount == bastionCount)
                totalRS *= PHI;

            Debug.Log($"[AetherConduit] Complete! {perfectCount}/{bastionCount} perfect, RS {totalRS:F1}");

            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            OnPuzzleCompleted?.Invoke(totalRS);
            Integration.GameLoopController.Instance?.OnMiniGameCompleted(totalRS, "AetherConduit");
        }

        void FailPuzzle()
        {
            _isActive = false;
            Debug.Log("[AetherConduit] Failed — time expired");
            OnPuzzleFailed?.Invoke();
        }
    }

    // ─── Data Types ──────────────────────────────

    public struct BastionNode
    {
        public Vector2 targetPosition;
        public Vector2 placedPosition;
        public float phiAccuracy;
        public float cutQuality;
        public bool isPlaced;
        public bool isCut;
    }
}
