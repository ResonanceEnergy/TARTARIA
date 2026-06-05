using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tutorial step identifiers for programmatic tutorial completion.
    /// </summary>
    public enum TutorialStep
    {
        Welcome,
        Movement,
        Scanning,
        BuildingTuning,
        Combat,
        Inventory,
        CompanionDialogue,
        Complete,
        // Phase 2 tutorial steps
        Discovery,
        HarmonicStrike,
        ResonancePulse,
        FrequencyShield,
        WorkshopUpgrade,
        BuildingRestore,
        FirstCombat,
        CombatComplete,
        Tuning
    }

    /// <summary>
    /// TutorialSystem - Progressive tutorial system.
    /// Guides new players through core mechanics.
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }

        [Header("Tutorial State")]
        [SerializeField] private bool tutorialActive = false;
        [SerializeField] private int currentStep = 0;
        [SerializeField] private List<string> completedSteps = new();

        // PlayerPrefs keys — survive session restart for save/load round-trip.
        private const string PREF_KEY_COMPLETED = "Tartaria.Tutorial.Completed";       // 0/1
        private const string PREF_KEY_CURRENT_STEP = "Tartaria.Tutorial.CurrentStep";  // int
        private const string PREF_KEY_COMPLETED_LIST = "Tartaria.Tutorial.CompletedList"; // csv

        private readonly string[] TUTORIAL_STEPS = new[]
        {
            "Welcome to TARTARIA! Use WASD to move.",
            "Press Space to jump. Explore the ruins.",
            "Press E to scan for buried buildings.",
            "You found a building! Now tune it to restore it.",
            "Use Q to attack enemies with Harmonic Strike.",
            "Open inventory with I. Collect Aether Shards.",
            "Talk to Milo by pressing T when nearby.",
            "Tutorial complete! Explore the world freely."
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            StartTutorial();
        }

        public void StartTutorial()
        {
            tutorialActive = true;
            currentStep = 0;
            ShowCurrentStep();
        }

        void ShowCurrentStep()
        {
            if (currentStep < TUTORIAL_STEPS.Length)
            {
                string step = TUTORIAL_STEPS[currentStep];
                HUDController.Instance?.ShowBanner("TUTORIAL", step);
                Debug.Log($"[Tutorial] Step {currentStep + 1}: {step}");
            }
            else
            {
                CompleteTutorial();
            }
        }

        public void CompleteStep(string stepName)
        {
            if (!completedSteps.Contains(stepName))
            {
                completedSteps.Add(stepName);
                currentStep++;
                ShowCurrentStep();
            }
        }

        void CompleteTutorial()
        {
            tutorialActive = false;
            PlayerPrefs.SetInt(PREF_KEY_COMPLETED, 1);
            PersistState();
            HUDController.Instance?.ShowBanner("TUTORIAL COMPLETE!", "You are ready to explore!");
            Debug.Log("[Tutorial] Tutorial complete!");
            OnTutorialComplete?.Invoke();
        }

        /// <summary>
        /// Force-complete a single tutorial step (debug/cheat path).
        /// If the step matches TutorialStep.Complete, the entire tutorial is marked done
        /// and OnTutorialComplete fires — useful for skipping the intro on QA runs.
        /// </summary>
        public void ForceComplete(TutorialStep step)
        {
            string stepKey = step.ToString();
            if (!completedSteps.Contains(stepKey))
            {
                completedSteps.Add(stepKey);
            }

            // If this is the terminal step, treat as full tutorial completion.
            if (step == TutorialStep.Complete)
            {
                currentStep = TUTORIAL_STEPS.Length;
                CompleteTutorial();
                Debug.Log("[Tutorial] Force-completed entire tutorial via TutorialStep.Complete");
                return;
            }

            // Otherwise advance the active counter past this step if we were on it.
            if (tutorialActive && currentStep < TUTORIAL_STEPS.Length)
            {
                currentStep++;
                PersistState();
                ShowCurrentStep();
            }
            else
            {
                PersistState();
            }
            Debug.Log($"[Tutorial] Force-completed step: {step} (currentStep={currentStep})");
        }

        /// <summary>
        /// Reset tutorial to beginning. Clears PlayerPrefs persistence and
        /// re-fires the welcome banner. Used by the debug menu and "New Game".
        /// </summary>
        public void ResetTutorial()
        {
            currentStep = 0;
            completedSteps.Clear();
            tutorialActive = false;

            PlayerPrefs.DeleteKey(PREF_KEY_COMPLETED);
            PlayerPrefs.DeleteKey(PREF_KEY_CURRENT_STEP);
            PlayerPrefs.DeleteKey(PREF_KEY_COMPLETED_LIST);
            PlayerPrefs.Save();

            Debug.Log("[Tutorial] Tutorial reset (PlayerPrefs cleared)");
            StartTutorial();
        }

        /// <summary>
        /// Persist current tutorial progress to PlayerPrefs so it survives
        /// scene reloads and editor restarts.
        /// </summary>
        void PersistState()
        {
            PlayerPrefs.SetInt(PREF_KEY_CURRENT_STEP, currentStep);
            PlayerPrefs.SetString(PREF_KEY_COMPLETED_LIST, string.Join(",", completedSteps));
            PlayerPrefs.Save();
        }

        /// <summary>True if the tutorial has been fully completed at any point on this device.</summary>
        public bool HasEverCompleted() => PlayerPrefs.GetInt(PREF_KEY_COMPLETED, 0) == 1;

        /// <summary>
        /// Event raised when entire tutorial is completed.
        /// </summary>
        public event System.Action OnTutorialComplete;

        public bool IsTutorialActive() => tutorialActive;
    }
}
