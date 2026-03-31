using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 (Cosmic Moon) meta-game — Cosmic Convergence.
    /// The final mini-game that orchestrates elements from ALL 6 mini-game types
    /// into a single grand challenge. Player must activate systems in sequence
    /// while the world's RS approaches 100%.
    ///
    /// Sequence:
    ///   1. Bell Tower Cascade — re-sync all 12 towers simultaneously
    ///   2. Prophecy Alignment — align 12 stones to the convergence point
    ///   3. Aquifer Harmony — maintain purge while towers ring
    ///   4. Fleet Formation — position 3 airships at convergence nodes
    ///   5. Rail Pulse — send resonance pulse through entire rail network
    ///   6. Final Tuning — golden-ratio tune the Planetary Nexus
    ///
    /// Cross-ref: docs/03C_MOON_MECHANICS_DETAILED.md §Moon 13, docs/13_MINI_GAMES.md
    /// </summary>
    [DisallowMultipleComponent]
    public class CosmicConvergenceMiniGame : MonoBehaviour
    {
        public static CosmicConvergenceMiniGame Instance { get; private set; }

        // ─── Constants ───────────────────────────────

        public const int TotalPhases = 6;
        public const float PhaseTimeLimit = 60f;        // seconds per phase
        public const float RSRewardPerPhase = 5f;
        public const float RSRewardCompletion = 20f;
        public const float GoldenRatio = 1.618034f;

        // ─── Phase Definitions ───────────────────────

        public enum ConvergencePhase : byte
        {
            NotStarted = 0,
            BellTowerCascade = 1,
            ProphecyAlignment = 2,
            AquiferHarmony = 3,
            FleetFormation = 4,
            RailPulse = 5,
            FinalTuning = 6,
            Complete = 7
        }

        // ─── State ───────────────────────────────────

        ConvergencePhase _currentPhase = ConvergencePhase.NotStarted;
        readonly bool[] _phasesComplete = new bool[TotalPhases];
        readonly float[] _phaseAccuracy = new float[TotalPhases];
        float _phaseTimer;
        bool _miniGameActive;
        float _convergenceScore;    // 0..1 overall score
        float _finalTuningAccumulator;

        // ─── Events ─────────────────────────────────

        public event Action<ConvergencePhase> OnPhaseStarted;
        public event Action<ConvergencePhase, float> OnPhaseCompleted; // phase, accuracy
        public event Action<float> OnConvergenceComplete;              // final score
        public event Action OnConvergenceFailed;

        // ─── Stored delegates for unsubscription ────
        Action _bellHandler;
        Action _leyHandler;
        Action _aquiferHandler;
        Action _fleetHandler;

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            UnsubscribePhaseHandlers();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (!_miniGameActive) return;
            if (_currentPhase == ConvergencePhase.NotStarted ||
                _currentPhase == ConvergencePhase.Complete) return;

            _phaseTimer += Time.deltaTime;
            if (_phaseTimer >= PhaseTimeLimit)
            {
                // Time ran out — phase fails with partial score
                CompleteCurrentPhase(0.3f);
            }

            // Check phase-specific completion conditions
            CheckPhaseConditions();
        }

        // ─── Public API ──────────────────────────────

        public void StartConvergence()
        {
            _miniGameActive = true;
            _convergenceScore = 0f;
            for (int i = 0; i < TotalPhases; i++)
            {
                _phasesComplete[i] = false;
                _phaseAccuracy[i] = 0f;
            }

            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
            Debug.Log("[CosmicConvergence] The Cosmic Convergence begins!");

            StartCoroutine(ConvergenceSequence());
        }

        public void StopConvergence()
        {
            UnsubscribePhaseHandlers();
            _miniGameActive = false;
            _currentPhase = ConvergencePhase.NotStarted;
            StopAllCoroutines();
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);
        }

        // ─── Convergence Sequence ────────────────────

        IEnumerator ConvergenceSequence()
        {
            try
            {
                // Phase 1: Bell Tower Cascade
                yield return StartCoroutine(RunPhase(ConvergencePhase.BellTowerCascade));

                // Phase 2: Prophecy Alignment
                yield return StartCoroutine(RunPhase(ConvergencePhase.ProphecyAlignment));

                // Phase 3: Aquifer Harmony
                yield return StartCoroutine(RunPhase(ConvergencePhase.AquiferHarmony));

                // Phase 4: Fleet Formation
                yield return StartCoroutine(RunPhase(ConvergencePhase.FleetFormation));

                // Phase 5: Rail Pulse
                yield return StartCoroutine(RunPhase(ConvergencePhase.RailPulse));

                // Phase 6: Final Tuning
                yield return StartCoroutine(RunPhase(ConvergencePhase.FinalTuning));

                // Calculate final score
                float totalAccuracy = 0f;
                for (int i = 0; i < TotalPhases; i++)
                    totalAccuracy += _phaseAccuracy[i];
                _convergenceScore = totalAccuracy / TotalPhases;

                _currentPhase = ConvergencePhase.Complete;
                _miniGameActive = false;

                float totalReward = RSRewardCompletion * _convergenceScore;
                GameLoopController.Instance?.OnMiniGameCompleted(totalReward, "cosmic_convergence");
                AchievementSystem.Instance?.Unlock("M06");

                HUDController.Instance?.ShowInteractionPrompt(
                    $"COSMIC CONVERGENCE COMPLETE\nScore: {_convergenceScore:P0}");

                OnConvergenceComplete?.Invoke(_convergenceScore);

                if (_convergenceScore < 0.5f)
                    OnConvergenceFailed?.Invoke();

                Debug.Log($"[CosmicConvergence] Complete! Score: {_convergenceScore:P0}");

                yield return new WaitForSeconds(5f);
                HUDController.Instance?.HideInteractionPrompt();
            }
            finally
            {
                UnsubscribePhaseHandlers();
                _miniGameActive = false;
                GameStateManager.Instance?.TransitionTo(GameState.Exploration);
            }
        }

        IEnumerator RunPhase(ConvergencePhase phase)
        {
            _currentPhase = phase;
            _phaseTimer = 0f;
            int idx = (int)phase - 1;

            OnPhaseStarted?.Invoke(phase);
            ActivatePhaseSystem(phase);

            HUDController.Instance?.ShowInteractionPrompt(
                $"CONVERGENCE PHASE {(int)phase}: {GetPhaseDisplayName(phase)}");

            Debug.Log($"[CosmicConvergence] Phase {(int)phase}: {phase}");

            // Wait until phase completes or times out
            yield return new WaitUntil(() =>
                _phasesComplete[idx] || _phaseTimer >= PhaseTimeLimit);

            if (!_phasesComplete[idx])
                CompleteCurrentPhase(0.3f); // Timeout penalty

            HUDController.Instance?.HideInteractionPrompt();
            yield return new WaitForSeconds(2f); // Brief pause between phases
        }

        // ─── Phase System Activation ─────────────────

        void UnsubscribePhaseHandlers()
        {
            if (_bellHandler != null)
            {
                if (BellTowerSyncMiniGame.Instance != null)
                    BellTowerSyncMiniGame.Instance.OnCascadeTriggered -= _bellHandler;
                _bellHandler = null;
            }
            if (_leyHandler != null)
            {
                if (LeyLineProphecyMiniGame.Instance != null)
                    LeyLineProphecyMiniGame.Instance.OnAllStonesComplete -= _leyHandler;
                _leyHandler = null;
            }
            if (_aquiferHandler != null)
            {
                if (AquiferPurgeMiniGame.Instance != null)
                    AquiferPurgeMiniGame.Instance.OnAllLayersPurged -= _aquiferHandler;
                _aquiferHandler = null;
            }
            if (_fleetHandler != null)
            {
                if (AirshipFleetManager.Instance != null)
                    AirshipFleetManager.Instance.OnFleetOperational -= _fleetHandler;
                _fleetHandler = null;
            }
        }

        void ActivatePhaseSystem(ConvergencePhase phase)
        {
            UnsubscribePhaseHandlers();

            switch (phase)
            {
                case ConvergencePhase.BellTowerCascade:
                    var bells = BellTowerSyncMiniGame.Instance;
                    if (bells != null)
                    {
                        _bellHandler = () => CompleteCurrentPhase(1f);
                        bells.OnCascadeTriggered += _bellHandler;
                        bells.StartMiniGame();
                    }
                    break;

                case ConvergencePhase.ProphecyAlignment:
                    var ley = LeyLineProphecyMiniGame.Instance;
                    if (ley != null)
                    {
                        _leyHandler = () => CompleteCurrentPhase(1f);
                        ley.OnAllStonesComplete += _leyHandler;
                        ley.StartMiniGame();
                    }
                    break;

                case ConvergencePhase.AquiferHarmony:
                    var aquifer = AquiferPurgeMiniGame.Instance;
                    if (aquifer != null)
                    {
                        _aquiferHandler = () => CompleteCurrentPhase(1f);
                        aquifer.OnAllLayersPurged += _aquiferHandler;
                        aquifer.StartMiniGame();
                    }
                    break;

                case ConvergencePhase.FleetFormation:
                    var fleet = AirshipFleetManager.Instance;
                    if (fleet != null)
                    {
                        _fleetHandler = () => CompleteCurrentPhase(0.9f);
                        fleet.OnFleetOperational += _fleetHandler;
                    }
                    break;

                case ConvergencePhase.RailPulse:
                    var rail = ContinentalRailSystem.Instance;
                    if (rail != null && rail.IsNetworkComplete)
                        CompleteCurrentPhase(1f);
                    break;

                case ConvergencePhase.FinalTuning:
                    // Player must tune the Planetary Nexus to golden-ratio 432 Hz
                    HUDController.Instance?.ShowInteractionPrompt(
                        "Tune the Planetary Nexus to 432 Hz.\nAlign all harmonic frequencies to the golden ratio.");
                    _finalTuningAccumulator = 0f;
                    break;
            }
        }

        // ─── Phase Completion ────────────────────────

        void CheckPhaseConditions()
        {
            // Additional per-frame checks for phases that need polling
            int idx = (int)_currentPhase - 1;
            if (idx < 0 || idx >= TotalPhases || _phasesComplete[idx]) return;

            switch (_currentPhase)
            {
                case ConvergencePhase.FinalTuning:
                    // Check if player tuning accumulates to completion (driven by TuningMiniGame)
                    if (_finalTuningAccumulator >= 1f)
                        CompleteCurrentPhase(Mathf.Clamp01(_finalTuningAccumulator));
                    break;
            }
        }

        void CompleteCurrentPhase(float accuracy)
        {
            int idx = (int)_currentPhase - 1;
            if (idx < 0 || idx >= TotalPhases) return;
            if (_phasesComplete[idx]) return;

            _phasesComplete[idx] = true;
            _phaseAccuracy[idx] = Mathf.Clamp01(accuracy);

            GameLoopController.Instance?.QueueRSReward(RSRewardPerPhase * accuracy, $"convergence_phase_{(int)_currentPhase}");
            OnPhaseCompleted?.Invoke(_currentPhase, accuracy);

            Debug.Log($"[CosmicConvergence] Phase {(int)_currentPhase} complete. Accuracy: {accuracy:P0}");
        }

        // ─── Helpers ─────────────────────────────────

        static string GetPhaseDisplayName(ConvergencePhase phase)
        {
            return phase switch
            {
                ConvergencePhase.BellTowerCascade => "Bell Tower Cascade",
                ConvergencePhase.ProphecyAlignment => "Prophecy Alignment",
                ConvergencePhase.AquiferHarmony => "Aquifer Harmony",
                ConvergencePhase.FleetFormation => "Fleet Formation",
                ConvergencePhase.RailPulse => "Rail Pulse",
                ConvergencePhase.FinalTuning => "Final Tuning",
                _ => "Unknown"
            };
        }

        // ─── Queries ─────────────────────────────────

        public bool IsActive => _miniGameActive;
        public ConvergencePhase CurrentPhase => _currentPhase;
        public float ConvergenceScore => _convergenceScore;
        public bool IsComplete => _currentPhase == ConvergencePhase.Complete;

        /// <summary>
        /// Called by TuningMiniGame when the player completes a tuning during Phase 6.
        /// Accuracy 0-1 contributes toward the final tuning threshold.
        /// </summary>
        public void ContributeFinalTuning(float accuracy)
        {
            if (_currentPhase != ConvergencePhase.FinalTuning) return;
            _finalTuningAccumulator += accuracy * 0.34f; // ~3 perfect tunes to complete
            Debug.Log($"[Convergence] Final tuning progress: {_finalTuningAccumulator:P0}");
        }

        // ─── Save / Load ─────────────────────────────

        public CosmicSavePayload GetSaveData()
        {
            return new CosmicSavePayload
            {
                currentPhase = (int)_currentPhase,
                phasesComplete = (bool[])_phasesComplete.Clone(),
                phaseAccuracy = (float[])_phaseAccuracy.Clone(),
                convergenceScore = _convergenceScore
            };
        }

        public void LoadSaveData(CosmicSavePayload data)
        {
            if (data == null) return;
            _currentPhase = (ConvergencePhase)data.currentPhase;
            int count = Mathf.Min(TotalPhases, data.phasesComplete?.Length ?? 0);
            for (int i = 0; i < count; i++)
            {
                _phasesComplete[i] = data.phasesComplete[i];
                _phaseAccuracy[i] = data.phaseAccuracy != null && i < data.phaseAccuracy.Length ? data.phaseAccuracy[i] : 0f;
            }
            _convergenceScore = data.convergenceScore;
        }

        public class CosmicSavePayload
        {
            public int currentPhase;
            public bool[] phasesComplete;
            public float[] phaseAccuracy;
            public float convergenceScore;
        }
    }
}
