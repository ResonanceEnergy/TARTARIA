using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tutorial Controller — presents 5 step-by-step tutorial prompts
    /// to teach core mechanics. Each step triggers contextually and
    /// dismisses after 5 seconds or when the player presses E.
    ///
    /// Steps:
    ///   1. "WASD to move" → triggered on player spawn
    ///   2. "E to interact" → triggered when within 3m of any IInteractable for 2s
    ///   3. "Hold TAB to scan" → triggered when scanner becomes active
    ///   4. "Restore buildings" → triggered on first building discovery
    ///   5. "Defeat Mud Golems" → triggered when first enemy spawns
    ///
    /// Completion is saved to PlayerPrefs (fallback if SaveData.tutorial unavailable).
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController Instance { get; private set; }

        [Header("Settings")]
        [SerializeField, Min(1f)] float autoCloseDelay = 5f;
        [SerializeField, Min(0f)] float proximityCheckInterval = 0.5f;
        [SerializeField] float interactableProximityDuration = 2f;

        readonly HashSet<int> _completedSteps = new();
        int? _activeStep;
        float _stepStartTime;
        float _proximityTimer;
        float _interactableProximityStart;
        bool _nearInteractable;

        public event System.Action OnTutorialComplete;

        readonly string[] _stepMessages = new[]
        {
            "WASD to move",
            "E to interact",
            "Hold TAB to scan",
            "Restore buildings to increase Resonance Score",
            "Defeat Mud Golems to protect Echohaven"
        };

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            LoadCompletedSteps();
        }

        void OnEnable()
        {
            // Wire player spawn event
            var spawner = FindFirstObjectByType<PlayerSpawner>();
            if (spawner != null)
                Invoke(nameof(TriggerStep1), 1f); // Delay 1s after spawn

            // Wire building discovery event
            if (GameLoopController.Instance != null)
                GameEvents.OnBuildingDiscovered += HandleBuildingDiscovered;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingDiscovered -= HandleBuildingDiscovered;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Auto-close active step after delay
            if (_activeStep.HasValue && Time.time - _stepStartTime >= autoCloseDelay)
                DismissActiveStep();

            // Check proximity to interactables for step 2
            if (!IsStepCompleted(1) && !_activeStep.HasValue)
            {
                _proximityTimer += Time.deltaTime;
                if (_proximityTimer >= proximityCheckInterval)
                {
                    _proximityTimer = 0f;
                    CheckInteractableProximity();
                }
            }
        }

        // ─── Step Triggers ───────────────────────────

        void TriggerStep1()
        {
            if (IsStepCompleted(0)) return;
            ShowStep(0);
        }

        void CheckInteractableProximity()
        {
            // Find player position
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Check if any IInteractable is within 3m
            var interactables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            bool foundNearby = false;
            foreach (var obj in interactables)
            {
                if (obj is Tartaria.Input.IInteractable)
                {
                    float dist = Vector3.Distance(player.transform.position, obj.transform.position);
                    if (dist <= 3f)
                    {
                        foundNearby = true;
                        break;
                    }
                }
            }

            if (foundNearby && !_nearInteractable)
            {
                _nearInteractable = true;
                _interactableProximityStart = Time.time;
            }
            else if (!foundNearby)
            {
                _nearInteractable = false;
            }

            if (_nearInteractable && Time.time - _interactableProximityStart >= interactableProximityDuration)
            {
                ShowStep(1);
                _nearInteractable = false;
            }
        }

        public void TriggerScannerStep()
        {
            if (IsStepCompleted(2)) return;
            ShowStep(2);
        }

        void HandleBuildingDiscovered(string buildingName, Vector3 position)
        {
            if (IsStepCompleted(3)) return;
            ShowStep(3);
        }

        public void TriggerEnemyStep()
        {
            if (IsStepCompleted(4)) return;
            ShowStep(4);
        }

        // ─── Display ─────────────────────────────────

        void ShowStep(int stepIndex)
        {
            if (_activeStep.HasValue || IsStepCompleted(stepIndex)) return;

            _activeStep = stepIndex;
            _stepStartTime = Time.time;

            string message = _stepMessages[stepIndex];
            UIManager.Instance?.ShowTutorial(message);

            Debug.Log($"[Tutorial] Step {stepIndex + 1} shown: {message}");
        }

        void DismissActiveStep()
        {
            if (!_activeStep.HasValue) return;

            int step = _activeStep.Value;
            _completedSteps.Add(step);
            _activeStep = null;

            SaveCompletedSteps();
            UIManager.Instance?.HideTutorial();

            Debug.Log($"[Tutorial] Step {step + 1} completed");

            // Check if all steps complete
            if (_completedSteps.Count >= _stepMessages.Length)
            {
                Debug.Log("[Tutorial] All steps complete!");
                OnTutorialComplete?.Invoke();
            }
        }



        // ─── Persistence ─────────────────────────────

        bool IsStepCompleted(int stepIndex) => _completedSteps.Contains(stepIndex);

        void LoadCompletedSteps()
        {
            // Try SaveData first (preferred)
            var saveManager = SaveManager.Instance;
            if (saveManager != null && saveManager.CurrentSave != null)
            {
                var tutorial = saveManager.CurrentSave.tutorial;
                if (tutorial.completedSteps != null)
                {
                    foreach (int step in tutorial.completedSteps)
                        _completedSteps.Add(step);
                    return;
                }
            }

            // Fallback to PlayerPrefs
            string key = "TutorialCompletedSteps";
            if (PlayerPrefs.HasKey(key))
            {
                string csv = PlayerPrefs.GetString(key);
                if (!string.IsNullOrEmpty(csv))
                {
                    foreach (string s in csv.Split(','))
                    {
                        if (int.TryParse(s, out int step))
                            _completedSteps.Add(step);
                    }
                }
            }
        }

        void SaveCompletedSteps()
        {
            // Try SaveData first
            var saveManager = SaveManager.Instance;
            if (saveManager != null && saveManager.CurrentSave != null)
            {
                var steps = new List<int>(_completedSteps);
                steps.Sort();
                saveManager.CurrentSave.tutorial.completedSteps = steps.ToArray();
                return;
            }

            // Fallback to PlayerPrefs
            var stepsList = new List<int>(_completedSteps);
            stepsList.Sort();
            string csv = string.Join(",", stepsList);
            PlayerPrefs.SetString("TutorialCompletedSteps", csv);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Resets all tutorial progress (for debugging).
        /// </summary>
        public void ResetTutorial()
        {
            _completedSteps.Clear();
            _activeStep = null;
            SaveCompletedSteps();
            UIManager.Instance?.HideTutorial();
            Debug.Log("[Tutorial] Reset");
        }
    }
}
