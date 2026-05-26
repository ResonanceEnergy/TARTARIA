using System.Collections.Generic;
using Tartaria.Core;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Cymatic Tuning Puzzle - harmonic frequency matching minigame.
    /// Used in Moon 12 bell towers to synchronize planetary resonance.
    /// Player adjusts frequency sliders to match target harmonic (visual feedback via cymatic patterns).
    /// </summary>
    public class CymaticTuningPuzzle : MonoBehaviour
    {
        [Header("Tuning Configuration")]
        [SerializeField] float targetFrequency = 432f; // Base Solfeggio frequency
        [SerializeField] float toleranceHz = 5f;       // Acceptable deviation
        [SerializeField] int harmonicBands = 7;        // 7-band spectrum (1-7 chakras/notes)

        [Header("Puzzle State")]
        [SerializeField] bool isPuzzleActive;
        [SerializeField] bool isPuzzleSolved;

        readonly float[] _currentBandFrequencies = new float[7];
        readonly float[] _targetBandFrequencies = new float[7];
        int _bandsMatched;

        public bool IsSolved => isPuzzleSolved;
        public float CompletionPercent => _bandsMatched / (float)harmonicBands;

        void Awake()
        {
            InitializeTargetFrequencies();
            RandomizeStartingFrequencies();
        }

        /// <summary>
        /// Initialize target harmonic frequencies (Solfeggio-based)
        /// </summary>
        void InitializeTargetFrequencies()
        {
            // 7 Solfeggio tones derived from 432 Hz base
            _targetBandFrequencies[0] = 174f;  // UT
            _targetBandFrequencies[1] = 285f;  // RE
            _targetBandFrequencies[2] = 396f;  // MI
            _targetBandFrequencies[3] = 417f;  // FA
            _targetBandFrequencies[4] = 528f;  // SOL (DNA repair)
            _targetBandFrequencies[5] = 639f;  // LA
            _targetBandFrequencies[6] = 741f;  // TI

            Debug.Log("[CymaticTuning] Target frequencies initialized (7-band Solfeggio spectrum)");
        }

        /// <summary>
        /// Randomize starting frequencies (offset from targets)
        /// </summary>
        void RandomizeStartingFrequencies()
        {
            for (int i = 0; i < harmonicBands; i++)
            {
                float offset = Random.Range(-50f, 50f);
                _currentBandFrequencies[i] = _targetBandFrequencies[i] + offset;
            }
        }

        /// <summary>
        /// Activate the cymatic tuning puzzle
        /// </summary>
        public void ActivatePuzzle()
        {
            if (isPuzzleActive) return;

            isPuzzleActive = true;
            Debug.Log("[CymaticTuning] Puzzle activated — tune all 7 harmonic bands");

            // Spawn UI overlay (would be a proper UI panel in production)
            SpawnTuningInterface();

            // Play ambient tuning tone
            Audio.AudioManager.Instance?.PlayTone(targetFrequency, 30f, 0.3f);
        }

        /// <summary>
        /// Spawn tuning interface (placeholder - would be proper UI in production)
        /// </summary>
        void SpawnTuningInterface()
        {
            Debug.Log("[CymaticTuning] Tuning interface spawned:");
            for (int i = 0; i < harmonicBands; i++)
            {
                Debug.Log($"  Band {i + 1}: Current={_currentBandFrequencies[i]:F1} Hz, Target={_targetBandFrequencies[i]:F1} Hz");
            }

            Debug.Log("[CymaticTuning] Use AdjustBand(bandIndex, delta) to tune frequencies");
        }

        /// <summary>
        /// Adjust a frequency band by delta Hz
        /// </summary>
        public void AdjustBand(int bandIndex, float deltaHz)
        {
            if (bandIndex < 0 || bandIndex >= harmonicBands)
            {
                Debug.LogWarning($"[CymaticTuning] Invalid band index {bandIndex}");
                return;
            }

            if (isPuzzleSolved) return;

            _currentBandFrequencies[bandIndex] += deltaHz;
            _currentBandFrequencies[bandIndex] = Mathf.Clamp(_currentBandFrequencies[bandIndex], 50f, 1000f);

            Debug.Log($"[CymaticTuning] Band {bandIndex + 1} adjusted to {_currentBandFrequencies[bandIndex]:F1} Hz");

            // Play feedback tone
            Audio.AudioManager.Instance?.PlayTone(_currentBandFrequencies[bandIndex], 0.3f, 0.5f);

            // Check if band is now matched
            CheckBandMatch(bandIndex);

            // Spawn cymatic pattern visual feedback
            UpdateCymaticPattern(bandIndex);

            // Check if all bands matched
            if (_bandsMatched >= harmonicBands)
            {
                CompletePuzzle();
            }
        }

        /// <summary>
        /// Check if a band is within tolerance of its target
        /// </summary>
        void CheckBandMatch(int bandIndex)
        {
            float diff = Mathf.Abs(_currentBandFrequencies[bandIndex] - _targetBandFrequencies[bandIndex]);
            bool isMatched = diff <= toleranceHz;

            if (isMatched)
            {
                Debug.Log($"[CymaticTuning] Band {bandIndex + 1} MATCHED! (Δ{diff:F2} Hz)");

                // Count unique matches
                int matchCount = 0;
                for (int i = 0; i < harmonicBands; i++)
                {
                    float d = Mathf.Abs(_currentBandFrequencies[i] - _targetBandFrequencies[i]);
                    if (d <= toleranceHz) matchCount++;
                }
                _bandsMatched = matchCount;

                // Play success chime
                Audio.AudioManager.Instance?.PlaySFX2D("FrequencyMatch");
            }
        }

        /// <summary>
        /// Update cymatic pattern visual (Chladni plate simulation)
        /// </summary>
        void UpdateCymaticPattern(int bandIndex)
        {
            // Spawn particle effect representing cymatic standing wave pattern
            var pattern = new GameObject($"CymaticPattern_Band{bandIndex}");
            pattern.transform.position = transform.position + Vector3.up * (bandIndex * 2f);

            var particles = pattern.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = Color.Lerp(Color.red, Color.cyan, CompletionPercent);
            main.startSize = 1f;
            main.startLifetime = 2f;
            main.maxParticles = 200;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 3f;

            // Destroy after a few seconds
            Destroy(pattern, 3f);
        }

        /// <summary>
        /// Complete the cymatic tuning puzzle
        /// </summary>
        void CompletePuzzle()
        {
            if (isPuzzleSolved) return;

            isPuzzleSolved = true;
            isPuzzleActive = false;

            Debug.Log("[CymaticTuning] PUZZLE SOLVED — All 7 harmonic bands synchronized!");

            // Play all tones in harmony
            for (int i = 0; i < harmonicBands; i++)
            {
                Audio.AudioManager.Instance?.PlayTone(_targetBandFrequencies[i], 10f, 0.5f);
            }

            // VFX: perfect resonance burst
            var burst = new GameObject("ResonanceBurst");
            burst.transform.position = transform.position;
            var particles = burst.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.4f, 1f); // Golden
            main.startSize = 10f;
            main.startLifetime = 5f;
            main.maxParticles = 5000;

            Destroy(burst, 6f);

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("korath_frequencies_aligned");

            // DISABLED (Phase 51) - QuestManager not yet available
            // QuestManager.Instance?.ProgressObjective("moon12_cymatic_tuning", 0, 1);

            // DISABLED (Phase 51) - AchievementSystem not yet available
            // AchievementSystem.Instance?.Unlock("harmonic_master");

            Debug.Log("[CymaticTuning] Planetary resonance frequency locked — bell tower synchronized");
        }

        /// <summary>
        /// Auto-solve for testing (simulates player tuning all bands)
        /// </summary>
        public void AutoSolve()
        {
            Debug.Log("[CymaticTuning] Auto-solving puzzle...");

            for (int i = 0; i < harmonicBands; i++)
            {
                _currentBandFrequencies[i] = _targetBandFrequencies[i];
            }

            _bandsMatched = harmonicBands;
            CompletePuzzle();
        }

        /// <summary>
        /// Get current tuning state for UI display
        /// </summary>
        public (float current, float target, bool matched)[] GetTuningState()
        {
            var state = new (float, float, bool)[harmonicBands];

            for (int i = 0; i < harmonicBands; i++)
            {
                float diff = Mathf.Abs(_currentBandFrequencies[i] - _targetBandFrequencies[i]);
                bool matched = diff <= toleranceHz;
                state[i] = (_currentBandFrequencies[i], _targetBandFrequencies[i], matched);
            }

            return state;
        }
    }
}
