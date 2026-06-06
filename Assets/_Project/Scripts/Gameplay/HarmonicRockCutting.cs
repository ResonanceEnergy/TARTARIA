using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Harmonic Rock Cutting Mini-Game — Tartarian precision stone-shaping.
    ///
    /// Design per GDD §13 (Mini-Games):
    ///   Players trace cutting lines along glowing harmonic veins in stone blocks.
    ///   The stone resonates at specific frequencies; cutting accuracy depends on
    ///   matching the mouse trace to the golden-ratio vein pattern.
    ///
    /// Mechanics:
    ///   - Stone block appears with 3-6 glowing veins (golden curves)
    ///   - Player traces each vein with mouse/touch in sequence
    ///   - Accuracy per vein: proximity of trace to vein path
    ///   - Combo multiplier for consecutive high-accuracy cuts
    ///   - RS reward scales with overall accuracy
    ///   - Timer based on stone complexity (15-30s)
    ///   - Stone types: Granite (easy), Marble (medium), Obsidian (hard), Starstone (expert)
    ///
    /// Scoring:
    ///   Perfect (>95%) = ×φ (1.618) bonus, Great (80-95%) = ×1.3,
    ///   Good (60-80%) = ×1.0, Fail (<60%) = stone cracks (retry)
    ///
    /// Integration:
    ///   - Started via BuildingRestorationManager when restoring Tartarian structures
    ///   - RS reward fed into AetherFieldManager
    ///   - Haptic feedback for tool vibration
    ///   - VFX sparks along cutting line
    /// </summary>
    public class HarmonicRockCutting : MonoBehaviour
    {
        public static HarmonicRockCutting Instance { get; private set; }

        [Header("Configuration")]
[SerializeField] float baseTimeLimit = 20f;

        // ─── Events ───
        public event System.Action<float> OnCutComplete;     // total accuracy 0-1
        public event System.Action OnCutFailed;
        public event System.Action<int, float> OnVeinTraced;  // veinIndex, accuracy 0-1
        public event System.Action<int> OnComboChanged;       // combo count

        // ─── State ───
        bool _isActive;
        RockCuttingConfig _config;
        float _timeRemaining;
        int _currentVein;
        float _totalAccuracy;
        int _veinsCompleted;
        int _combo;
        int _maxCombo;

        // Vein tracing state
        bool _isTracing;
        float _veinAccumulator;
        int _veinSamples;
        float _veinBaseAngle;     // Direction of current vein curve

        // ─── Vein path data ───
        VeinPath[] _veins;

        // ─── Public Getters ───
        public bool IsActive => _isActive;
        public float TimeRemaining => _timeRemaining;
        public int CurrentVein => _currentVein;
        public int Combo => _combo;
        public float CurrentVeinAccuracy => _veinSamples > 0 ? _veinAccumulator / _veinSamples : 0f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Start / Stop ────────────────────────────

        public void StartCutting(RockCuttingConfig config)
        {
            _config = config;
            _timeRemaining = config.timeLimit > 0 ? config.timeLimit : baseTimeLimit;
            _currentVein = 0;
            _totalAccuracy = 0f;
            _veinsCompleted = 0;
            _combo = 0;
            _maxCombo = 0;
            _isTracing = false;
            _isActive = true;

            GenerateVeins(config.veinCount, config.stoneType);

            GameStateManager.Instance?.TransitionTo(GameState.Tuning);
        }

        public void AbortCutting()
        {
            _isActive = false;
            OnCutFailed?.Invoke();
            AudioManager.Instance?.PlaySFX2D("CuttingAborted");
            GameStateManager.Instance?.ReturnToPrevious();
        }

        void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                FinishCutting();
                return;
            }

            if (_currentVein >= _config.veinCount)
            {
                FinishCutting();
                return;
            }

            HandleTracing();
        }

        // ─── Tracing Logic ──────────────────────────

        void HandleTracing()
        {
            var vein = _veins[_currentVein];
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                // Start tracing if near vein start point
                Vector2 mousePos = GetNormalizedMousePosition();
                if (Vector2.Distance(mousePos, vein.startPoint) < 0.08f)
                {
                    _isTracing = true;
                    _veinAccumulator = 0f;
                    _veinSamples = 0;
                }
            }

            if (_isTracing && mouse.leftButton.isPressed)
            {
                Vector2 mousePos = GetNormalizedMousePosition();

                // Calculate distance from mouse to nearest point on vein curve
                float t = (float)_veinSamples / Mathf.Max(1f, vein.expectedSamples);
                Vector2 expected = EvaluateVein(vein, t);
                float dist = Vector2.Distance(mousePos, expected);

                // Convert distance to accuracy (0 = far, 1 = on vein)
                float sampleAcc = Mathf.Clamp01(1f - dist / 0.15f);
                _veinAccumulator += sampleAcc;
                _veinSamples++;

                // Haptic feedback
                if (HapticFeedbackManager.Instance != null)
                {
                    if (sampleAcc > 0.8f)
                        HapticFeedbackManager.Instance.PlayTuningOnFrequency();
                    else
                        HapticFeedbackManager.Instance.PlayTuningOffFrequency();
                }

                // VFX sparks along trace
                if (_veinSamples % 3 == 0)
                {
                    ServiceLocator.VFX?.PlayEffect(
                        VFXEffect.Spark,
                        new Vector3(mousePos.x * 10f - 5f, mousePos.y * 6f - 3f, 0f));
                }
            }

            if (_isTracing && mouse.leftButton.wasReleasedThisFrame)
            {
                CompleteVein();
            }
        }

        void CompleteVein()
        {
            _isTracing = false;
            float accuracy = _veinSamples > 0 ? _veinAccumulator / _veinSamples : 0f;

            // Combo tracking
            if (accuracy >= 0.8f)
            {
                _combo++;
                if (_combo > _maxCombo) _maxCombo = _combo;
            }
            else
            {
                _combo = 0;
            }

            _totalAccuracy += accuracy;
            _veinsCompleted++;

            OnVeinTraced?.Invoke(_currentVein, accuracy);
            OnComboChanged?.Invoke(_combo);

            // Audio: pitch rises with combo
            float pitch = 432f + _combo * 50f;
            AudioManager.Instance?.PlayTone(pitch, 0.3f);

            _currentVein++;
        }

        void FinishCutting()
        {
            _isActive = false;
            float overallAccuracy = _veinsCompleted > 0 ? _totalAccuracy / _veinsCompleted : 0f;

            if (overallAccuracy < 0.6f)
            {
                OnCutFailed?.Invoke();
            }
            else
            {
                // Apply scoring multiplier
                float multiplier;
                if (overallAccuracy >= 0.95f)
                    multiplier = GoldenRatioValidator.PHI; // 1.618×
                else if (overallAccuracy >= 0.8f)
                    multiplier = 1.3f;
                else
                    multiplier = 1.0f;

                // Combo bonus
                float comboBonus = 1f + _maxCombo * 0.1f;

                float finalAccuracy = Mathf.Clamp01(overallAccuracy * multiplier * comboBonus);
                OnCutComplete?.Invoke(finalAccuracy);
                HapticFeedbackManager.Instance?.PlayBuildingEmergence();

                // RS reward
                float rsReward = _config.baseRSReward * finalAccuracy;
                AetherFieldManager.Instance?.AddResonanceScore(rsReward);
                ServiceLocator.GameLoop?.OnMiniGameCompleted(rsReward, "HarmonicRockCutting");
            }

            GameStateManager.Instance?.ReturnToPrevious();
        }

        // ─── Vein Generation ─────────────────────────

        void GenerateVeins(int count, StoneType type)
        {
            _veins = new VeinPath[count];
            float complexity = type switch
            {
                StoneType.Granite => 0.3f,
                StoneType.Marble => 0.5f,
                StoneType.Obsidian => 0.7f,
                StoneType.Starstone => 0.9f,
                _ => 0.5f
            };

            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                float r = 0.25f + Random.Range(0f, 0.15f);

                var vein = new VeinPath
                {
                    startPoint = new Vector2(
                        0.5f + Mathf.Cos(angle) * r,
                        0.5f + Mathf.Sin(angle) * r),
                    endPoint = new Vector2(
                        0.5f + Mathf.Cos(angle + Mathf.PI) * r * 0.8f,
                        0.5f + Mathf.Sin(angle + Mathf.PI) * r * 0.8f),
                    curvature = complexity * Random.Range(0.5f, 1.5f),
                    controlOffset = new Vector2(
                        Random.Range(-0.1f, 0.1f) * complexity,
                        Random.Range(-0.1f, 0.1f) * complexity),
                    expectedSamples = Mathf.RoundToInt(30f + complexity * 40f)
                };

                _veins[i] = vein;
            }
        }

        Vector2 EvaluateVein(VeinPath vein, float t)
        {
            // Quadratic Bezier with golden-ratio–influenced control point
            t = Mathf.Clamp01(t);
            Vector2 mid = (vein.startPoint + vein.endPoint) * 0.5f + vein.controlOffset;
            Vector2 p0 = vein.startPoint;
            Vector2 p1 = mid;
            Vector2 p2 = vein.endPoint;

            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        Vector2 GetNormalizedMousePosition()
        {
            var mouse = Mouse.current;
            if (mouse == null) return Vector2.zero;
            var pos = mouse.position.ReadValue();
            return new Vector2(
                pos.x / Screen.width,
                pos.y / Screen.height);
        }

        // ─── Data Types ─────────────────────────────

        struct VeinPath
        {
            public Vector2 startPoint;
            public Vector2 endPoint;
            public float curvature;
            public Vector2 controlOffset;
            public int expectedSamples;
        }
    }

    // ─── Config / Enums ─────────────────────────

    [System.Serializable]
    public class RockCuttingConfig
    {
        public StoneType stoneType;
        public int veinCount;
        public float timeLimit;
        public float baseRSReward;

        /// <summary>Factory: easy Granite rock (tutorial).</summary>
        public static RockCuttingConfig Granite() => new()
        {
            stoneType = StoneType.Granite,
            veinCount = 3,
            timeLimit = 20f,
            baseRSReward = 5f
        };

        /// <summary>Factory: medium Marble rock.</summary>
        public static RockCuttingConfig Marble() => new()
        {
            stoneType = StoneType.Marble,
            veinCount = 4,
            timeLimit = 25f,
            baseRSReward = 10f
        };

        /// <summary>Factory: hard Obsidian rock.</summary>
        public static RockCuttingConfig Obsidian() => new()
        {
            stoneType = StoneType.Obsidian,
            veinCount = 5,
            timeLimit = 25f,
            baseRSReward = 18f
        };

        /// <summary>Factory: expert Starstone rock.</summary>
        public static RockCuttingConfig Starstone() => new()
        {
            stoneType = StoneType.Starstone,
            veinCount = 6,
            timeLimit = 30f,
            baseRSReward = 30f
        };
    }

    public enum StoneType : byte
    {
        Granite = 0,
        Marble = 1,
        Obsidian = 2,
        Starstone = 3
    }
}
