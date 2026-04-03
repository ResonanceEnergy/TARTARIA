using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tutorial & Onboarding System — guides the player through
    /// the first 15 minutes of gameplay with a progressive 4-step loop:
    ///   1. PROMPT  → show contextual instruction
    ///   2. OBSERVE → wait for player to perform action
    ///   3. REWARD  → celebrate success with VFX + dialogue
    ///   4. ADVANCE → unlock next tutorial step
    ///
    /// Steps auto-complete if the player performs the action organically.
    /// Never blocks gameplay — overlays are non-modal.
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }

        public event Action<TutorialStep> OnStepCompleted;
        public event Action OnTutorialComplete;

        [Header("Settings")]
        [SerializeField] float promptDelay = 2f;
        [SerializeField] float celebrationDuration = 1.5f;

        readonly List<TutorialStepDef> _steps = new();
        readonly HashSet<TutorialStep> _completed = new();
        int _currentIndex;
        float _promptTimer;
        float _celebrationTimer;
        TutorialState _state = TutorialState.Idle;
        bool _tutorialFinished;

        public int CurrentStepIndex => _currentIndex;
        public bool IsComplete => _tutorialFinished;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildSteps();
            GameEvents.OnToggleAetherVision += HandleAetherVisionToggle;
        }

        void OnDestroy()
        {
            GameEvents.OnToggleAetherVision -= HandleAetherVisionToggle;
            if (Instance == this) Instance = null;
        }

        void HandleAetherVisionToggle()
        {
            ForceComplete(TutorialStep.AetherVision);
        }

        void Start()
        {
            if (!_tutorialFinished)
                BeginStep(0);
        }

        void Update()
        {
            if (_tutorialFinished) return;

            switch (_state)
            {
                case TutorialState.Prompting:
                    _promptTimer -= Time.deltaTime;
                    if (_promptTimer <= 0f)
                    {
                        ShowPrompt();
                        _state = TutorialState.Observing;
                    }
                    break;

                case TutorialState.Observing:
                    if (CheckStepCondition())
                    {
                        CompleteCurrentStep();
                    }
                    break;

                case TutorialState.Celebrating:
                    _celebrationTimer -= Time.deltaTime;
                    if (_celebrationTimer <= 0f)
                    {
                        AdvanceToNext();
                    }
                    break;
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Force-complete a step (e.g. if player performed the action
        /// before the tutorial reached that step).
        /// </summary>
        public void ForceComplete(TutorialStep step)
        {
            if (_completed.Contains(step)) return;
            _completed.Add(step);
            OnStepCompleted?.Invoke(step);

            // If this is the current step, advance
            if (_currentIndex < _steps.Count && _steps[_currentIndex].step == step)
            {
                _state = TutorialState.Celebrating;
                _celebrationTimer = celebrationDuration;
            }
        }

        /// <summary>
        /// Skip the entire tutorial (settings toggle).
        /// </summary>
        public void SkipTutorial()
        {
            _tutorialFinished = true;
            HUDController.Instance?.HideInteractionPrompt();
            OnTutorialComplete?.Invoke();
        }

        /// <summary>
        /// Reset tutorial to the beginning (debug console).
        /// </summary>
        public void ResetTutorial()
        {
            _completed.Clear();
            _currentIndex = 0;
            _tutorialFinished = false;
            _state = TutorialState.Idle;
            BeginStep(0);
        }

        /// <summary>
        /// Get save-friendly data for persistence.
        /// </summary>
        public TutorialSaveData GetSaveData()
        {
            var ids = new List<int>();
            foreach (var s in _completed) ids.Add((int)s);
            return new TutorialSaveData
            {
                completedSteps = ids,
                currentIndex = _currentIndex,
                finished = _tutorialFinished
            };
        }

        /// <summary>
        /// Restore from save.
        /// </summary>
        public void RestoreFromSave(TutorialSaveData data)
        {
            if (data == null) return;
            _completed.Clear();
            if (data.completedSteps != null)
            {
                foreach (var id in data.completedSteps)
                    if (System.Enum.IsDefined(typeof(TutorialStep), id))
                        _completed.Add((TutorialStep)id);
            }
            _currentIndex = data.currentIndex;
            _tutorialFinished = data.finished;

            if (!_tutorialFinished && _currentIndex < _steps.Count)
                BeginStep(_currentIndex);
        }

        // ─── Step Definitions ────────────────────────

        void BuildSteps()
        {
            // Moon 1: Echohaven onboarding sequence
            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.Movement,
                prompt = "Use WASD to explore Echohaven. Follow the hum of lost frequencies.",
                speaker = "Milo",
                celebration = "That's it! Every step sends ripples through the Aether field."
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.Discovery,
                prompt = "Approach the glowing mound ahead. Press E to interact.",
                speaker = "Milo",
                celebration = "A Tartarian structure! Buried for centuries, but still resonating!"
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.Tuning,
                prompt = "Match the frequency to 432 Hz. Use the dial to align the harmonic.",
                speaker = "Milo",
                celebration = "The building remembers its song! Watch the mud dissolve!"
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.ResonancePulse,
                prompt = "A Mud Golem! Press LMB to fire a Resonance Pulse. Purify, don't destroy.",
                speaker = "Milo",
                celebration = "The Golem dissolves back into clean earth. Restoration, not destruction."
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.HarmonicStrike,
                prompt = "Press RMB for a Harmonic Strike — directed, high-damage frequency beam.",
                speaker = "Milo",
                celebration = "Direct hit! You're a natural frequency warrior."
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.FrequencyShield,
                prompt = "Hold SPACE to activate your Frequency Shield. Blocks incoming attacks for 3 seconds.",
                speaker = "Milo",
                celebration = "Shield held! The 432 Hz barrier repels dissonant frequencies."
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.AetherVision,
                prompt = "Press TAB to toggle Aether Vision. See hidden ley lines and corruption.",
                speaker = "Milo",
                celebration = "You can see the invisible web of energy connecting everything!"
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.QuestAccept,
                prompt = "Open the Quest Log with J. Accept the quest 'Echohaven Awakening'.",
                speaker = "Milo",
                celebration = "Quest accepted! Each step brings us closer to full restoration."
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.BuildingRestore,
                prompt = "Complete all 3 tuning nodes on the building to fully restore it.",
                speaker = "Milo",
                celebration = "ALIVE! The building is generating Aether again! This is what we're fighting for!"
            });

            _steps.Add(new TutorialStepDef
            {
                step = TutorialStep.WorkshopUpgrade,
                prompt = "Visit the Workshop to upgrade your restored building. Interact with the workbench.",
                speaker = "Milo",
                celebration = "Upgraded! Higher tiers generate more Aether and unlock new abilities."
            });
        }

        // ─── Internal Flow ───────────────────────────

        void BeginStep(int index)
        {
            if (index >= _steps.Count)
            {
                _tutorialFinished = true;
                OnTutorialComplete?.Invoke();
                Debug.Log("[Tutorial] All steps complete!");
                return;
            }

            // Skip already-completed steps
            while (index < _steps.Count && _completed.Contains(_steps[index].step))
                index++;

            if (index >= _steps.Count)
            {
                _tutorialFinished = true;
                OnTutorialComplete?.Invoke();
                return;
            }

            _currentIndex = index;
            _promptTimer = promptDelay;
            _state = TutorialState.Prompting;
        }

        void ShowPrompt()
        {
            var step = _steps[_currentIndex];
            HUDController.Instance?.ShowInteractionPrompt(step.prompt);
            Debug.Log($"[Tutorial] Step {_currentIndex}: {step.step} — {step.prompt}");
        }

        bool CheckStepCondition()
        {
            if (_currentIndex >= _steps.Count) return false;
            var step = _steps[_currentIndex].step;

            switch (step)
            {
                case TutorialStep.Movement:
                    return PlayerInputHandler.Instance != null && PlayerInputHandler.Instance.IsMoving;

                case TutorialStep.ResonancePulse:
                case TutorialStep.HarmonicStrike:
                case TutorialStep.FrequencyShield:
                case TutorialStep.AetherVision:
                    // These are force-completed by the input handlers / GameLoopController
                    return _completed.Contains(step);

                case TutorialStep.Discovery:
                case TutorialStep.Tuning:
                case TutorialStep.QuestAccept:
                case TutorialStep.BuildingRestore:
                case TutorialStep.WorkshopUpgrade:
                    // Force-completed by respective system callbacks
                    return _completed.Contains(step);

                default:
                    return false;
            }
        }

        void CompleteCurrentStep()
        {
            var stepDef = _steps[_currentIndex];
            _completed.Add(stepDef.step);

            // Celebration
            HUDController.Instance?.ShowInteractionPrompt(stepDef.celebration);
            DialogueManager.Instance?.PlayContextDialogue("tuning_success");
            VFXController.Instance?.PlayDiscoveryBurst(
                PlayerInputHandler.Instance != null
                    ? PlayerInputHandler.Instance.transform.position
                    : Vector3.zero);

            OnStepCompleted?.Invoke(stepDef.step);

            _state = TutorialState.Celebrating;
            _celebrationTimer = celebrationDuration;

            Debug.Log($"[Tutorial] Completed: {stepDef.step}");
        }

        void AdvanceToNext()
        {
            HUDController.Instance?.HideInteractionPrompt();
            BeginStep(_currentIndex + 1);
        }

        // ─── Data Types ──────────────────────────────

        enum TutorialState { Idle, Prompting, Observing, Celebrating }

        class TutorialStepDef
        {
            public TutorialStep step;
            public string prompt;
            public string speaker;
            public string celebration;
        }
    }

    public enum TutorialStep
    {
        Movement = 0,
        Discovery = 1,
        Tuning = 2,
        ResonancePulse = 3,
        HarmonicStrike = 4,
        FrequencyShield = 5,
        AetherVision = 6,
        QuestAccept = 7,
        BuildingRestore = 8,
        WorkshopUpgrade = 9
    }

    [Serializable]
    public class TutorialSaveData
    {
        public List<int> completedSteps = new();
        public int currentIndex;
        public bool finished;
    }
}
