using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
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
            HUDController.Instance?.ShowBanner("TUTORIAL COMPLETE!", "You are ready to explore!");
            Debug.Log("[Tutorial] ✅ Tutorial complete!");
        }

        public bool IsTutorialActive() => tutorialActive;
    }
}
