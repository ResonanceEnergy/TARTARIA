using UnityEngine;
using System;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Frequency matching mini-game for building restoration.
    /// Player adjusts frequency dial (200-800 Hz) to match target frequency (default 432 Hz).
    /// Requires 3 successful node matches within ±10 Hz tolerance to complete tuning.
    /// </summary>
    public class TuningMiniGameController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Tuning Parameters")]
        [SerializeField, Tooltip("Target frequency to match (Hz)")]
        private float targetFrequency = 432f;
        
        [SerializeField, Tooltip("Acceptable frequency deviation (Hz)")]
        private float toleranceHz = 10f;
        
        [SerializeField, Tooltip("Number of successful nodes required")]
        private int requiredNodes = 3;
        
        [SerializeField, Tooltip("Frequency adjustment speed (Hz per second)")]
        private float adjustSpeed = 5f;
        
        [Header("Frequency Range")]
        [SerializeField, Tooltip("Minimum frequency (Hz)")]
        private float minFrequency = 200f;
        
        [SerializeField, Tooltip("Maximum frequency (Hz)")]
        private float maxFrequency = 800f;
        
        [Header("Difficulty Settings")]
        [SerializeField, Tooltip("Max attempts before failure (0 = unlimited)")]
        private int maxAttempts = 0;
        
        [SerializeField, Tooltip("Time limit per node in seconds (0 = unlimited)")]
        private float timeLimitPerNode = 0f;
        
        [Header("Audio Feedback")]
        [SerializeField, Tooltip("Play tone when frequency changes")]
        private bool playFrequencyTone = true;
        
        [SerializeField, Tooltip("Audio volume for frequency tone")]
        private float toneVolume = 0.3f;
        #endregion

        #region Events
        /// <summary>Fired when all nodes are successfully tuned. Accuracy = avg precision (0-1).</summary>
        public event Action<float> OnTuningComplete;
        
        /// <summary>Fired when tuning fails (max attempts exceeded or time expired).</summary>
        public event Action OnTuningFailed;
        
        /// <summary>Fired when a single node is successfully matched. nodeIndex = 0-based.</summary>
        public event Action<int> OnNodeComplete;
        
        /// <summary>Fired when frequency changes. frequency = current Hz value.</summary>
        public event Action<float> OnFrequencyChanged;
        
        /// <summary>Fired when player submits frequency (before validation).</summary>
        public event Action<float, bool> OnFrequencySubmitted; // frequency, isSuccess
        #endregion

        #region Private State
        private float _currentFrequency = 432f;
        private int _nodesCompleted = 0;
        private int _attemptsMade = 0;
        private bool _isActive = false;
        private bool _isPaused = false;
        private float _nodeStartTime = 0f;
        private float _totalAccuracy = 0f;
        private AudioSource _audioSource;
        #endregion

        #region Public Properties
        /// <summary>Current frequency setting (Hz).</summary>
        public float CurrentFrequency => _currentFrequency;
        
        /// <summary>Target frequency to match (Hz).</summary>
        public float TargetFrequency => targetFrequency;
        
        /// <summary>Number of nodes successfully completed.</summary>
        public int NodesCompleted => _nodesCompleted;
        
        /// <summary>Total nodes required.</summary>
        public int RequiredNodes => requiredNodes;
        
        /// <summary>Is mini-game currently active?</summary>
        public bool IsActive => _isActive;
        
        /// <summary>Is mini-game paused?</summary>
        public bool IsPaused => _isPaused;
        
        /// <summary>Current accuracy (0-1) using golden ratio validator.</summary>
        public float CurrentAccuracy => GoldenRatioValidator.GetFrequencyAccuracy(_currentFrequency);
        
        /// <summary>Attempts made on current node.</summary>
        public int AttemptsMade => _attemptsMade;
        
        /// <summary>Time remaining for current node (0 if no limit).</summary>
        public float TimeRemaining => timeLimitPerNode > 0 
            ? Mathf.Max(0, timeLimitPerNode - (Time.time - _nodeStartTime)) 
            : 0f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.volume = toneVolume;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D sound
        }

        private void Update()
        {
            if (!_isActive || _isPaused)
                return;

            // Check time limit
            if (timeLimitPerNode > 0 && Time.time - _nodeStartTime > timeLimitPerNode)
            {
                HandleTimeLimitExpired();
                return;
            }

            // Input handling
            HandleFrequencyInput();
            HandleSubmitInput();
        }

        private void OnDestroy()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }
        #endregion

        #region Public API
        /// <summary>Start the tuning mini-game.</summary>
        public void BeginTuning()
        {
            _isActive = true;
            _isPaused = false;
            _nodesCompleted = 0;
            _attemptsMade = 0;
            _totalAccuracy = 0f;
            _currentFrequency = (minFrequency + maxFrequency) / 2f; // Start at midpoint
            _nodeStartTime = Time.time;
            
            Debug.Log($"[TuningMiniGame] BEGIN: Target={targetFrequency}Hz, Tolerance=±{toleranceHz}Hz, Nodes={requiredNodes}");
            OnFrequencyChanged?.Invoke(_currentFrequency);
        }

        /// <summary>End the tuning mini-game (cleanup).</summary>
        public void EndTuning()
        {
            _isActive = false;
            _isPaused = false;
            
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            
            Debug.Log($"[TuningMiniGame] END: NodesCompleted={_nodesCompleted}/{requiredNodes}");
        }

        /// <summary>Reset progress to initial state (maintains active status).</summary>
        public void ResetProgress()
        {
            _nodesCompleted = 0;
            _attemptsMade = 0;
            _totalAccuracy = 0f;
            _currentFrequency = (minFrequency + maxFrequency) / 2f;
            _nodeStartTime = Time.time;
            
            Debug.Log("[TuningMiniGame] RESET: Progress cleared");
            OnFrequencyChanged?.Invoke(_currentFrequency);
        }

        /// <summary>Pause/unpause the mini-game.</summary>
        public void SetPaused(bool paused)
        {
            if (!_isActive)
                return;
                
            _isPaused = paused;
            
            if (_audioSource != null && _audioSource.isPlaying && paused)
            {
                _audioSource.Pause();
            }
            else if (_audioSource != null && paused == false)
            {
                _audioSource.UnPause();
            }
        }

        /// <summary>Manually set current frequency (for UI sliders, etc.).</summary>
        public void SetFrequency(float frequency)
        {
            _currentFrequency = Mathf.Clamp(frequency, minFrequency, maxFrequency);
            OnFrequencyChanged?.Invoke(_currentFrequency);
            
            if (playFrequencyTone && _isActive && !_isPaused)
            {
                PlayFrequencyTone(_currentFrequency);
            }
        }

        /// <summary>Manually submit current frequency (for UI buttons, etc.).</summary>
        public void SubmitFrequency()
        {
            if (!_isActive || _isPaused)
                return;

            _attemptsMade++;
            float deviation = Mathf.Abs(_currentFrequency - targetFrequency);
            bool isSuccess = deviation <= toleranceHz;
            
            OnFrequencySubmitted?.Invoke(_currentFrequency, isSuccess);
            
            if (isSuccess)
            {
                HandleNodeSuccess(deviation);
            }
            else
            {
                HandleNodeFailure(deviation);
            }
        }

        /// <summary>Check if current frequency is within tolerance.</summary>
        public bool IsFrequencyMatched()
        {
            return Mathf.Abs(_currentFrequency - targetFrequency) <= toleranceHz;
        }

        /// <summary>Get current deviation from target (Hz).</summary>
        public float GetDeviation()
        {
            return _currentFrequency - targetFrequency;
        }

        /// <summary>Get normalized frequency (0-1 across min-max range).</summary>
        public float GetNormalizedFrequency()
        {
            return Mathf.InverseLerp(minFrequency, maxFrequency, _currentFrequency);
        }
        #endregion

        #region Input Handling
        private void HandleFrequencyInput()
        {
            float adjustment = 0f;

            // Q/E key input
            if (Input.GetKey(KeyCode.Q))
            {
                adjustment -= adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E))
            {
                adjustment += adjustSpeed * Time.deltaTime;
            }

            // Mouse wheel input
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                adjustment += scroll * adjustSpeed * 10f; // Scale scroll for better feel
            }

            // Apply adjustment
            if (Mathf.Abs(adjustment) > 0.001f)
            {
                SetFrequency(_currentFrequency + adjustment);
            }
        }

        private void HandleSubmitInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SubmitFrequency();
            }
        }
        #endregion

        #region Logic
        private void HandleNodeSuccess(float deviation)
        {
            _nodesCompleted++;
            
            // Calculate accuracy (0 deviation = 1.0, tolerance deviation = 0.0)
            float nodeAccuracy = 1f - (deviation / toleranceHz);
            _totalAccuracy += nodeAccuracy;
            
            Debug.Log($"[TuningMiniGame] NODE SUCCESS: {_nodesCompleted}/{requiredNodes} | Frequency={_currentFrequency:F1}Hz | Deviation={deviation:F1}Hz | Accuracy={nodeAccuracy:F2}");
            
            OnNodeComplete?.Invoke(_nodesCompleted - 1); // 0-based index
            
            if (_nodesCompleted >= requiredNodes)
            {
                float avgAccuracy = _totalAccuracy / requiredNodes;
                Debug.Log($"[TuningMiniGame] COMPLETE! Average Accuracy: {avgAccuracy:F2}");
                OnTuningComplete?.Invoke(avgAccuracy);
                EndTuning();
            }
            else
            {
                // Reset for next node
                _attemptsMade = 0;
                _nodeStartTime = Time.time;
                _currentFrequency = (minFrequency + maxFrequency) / 2f; // Reset to midpoint
                OnFrequencyChanged?.Invoke(_currentFrequency);
            }
        }

        private void HandleNodeFailure(float deviation)
        {
            Debug.Log($"[TuningMiniGame] NODE FAIL: Frequency={_currentFrequency:F1}Hz | Deviation={deviation:F1}Hz | Attempts={_attemptsMade}");
            
            // Check max attempts
            if (maxAttempts > 0 && _attemptsMade >= maxAttempts)
            {
                Debug.Log($"[TuningMiniGame] FAILED: Max attempts ({maxAttempts}) exceeded");
                OnTuningFailed?.Invoke();
                EndTuning();
            }
        }

        private void HandleTimeLimitExpired()
        {
            Debug.Log($"[TuningMiniGame] FAILED: Time limit ({timeLimitPerNode}s) expired");
            OnTuningFailed?.Invoke();
            EndTuning();
        }
        #endregion

        #region Audio
        private void PlayFrequencyTone(float frequency)
        {
            if (_audioSource == null || !playFrequencyTone)
                return;

            // Generate simple sine wave tone at current frequency
            // Note: This is a simplified version - production would use AudioClip generation
            // For now, just adjust pitch based on frequency
            float normalizedFreq = Mathf.InverseLerp(minFrequency, maxFrequency, frequency);
            _audioSource.pitch = Mathf.Lerp(0.5f, 2f, normalizedFreq);
            
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        #endregion

        #region Debug
        private void OnGUI()
        {
            if (!_isActive)
                return;

            // Simple debug overlay
            GUI.Box(new Rect(10, 10, 300, 150), "Tuning Mini-Game");
            
            GUILayout.BeginArea(new Rect(20, 35, 280, 120));
            GUILayout.Label($"Target: {targetFrequency:F1} Hz (±{toleranceHz:F1} Hz)");
            GUILayout.Label($"Current: {_currentFrequency:F1} Hz");
            GUILayout.Label($"Deviation: {GetDeviation():F1} Hz");
            GUILayout.Label($"Nodes: {_nodesCompleted}/{requiredNodes}");
            GUILayout.Label($"Attempts: {_attemptsMade}" + (maxAttempts > 0 ? $"/{maxAttempts}" : ""));
            if (timeLimitPerNode > 0)
            {
                GUILayout.Label($"Time: {TimeRemaining:F1}s");
            }
            GUILayout.Label($"Accuracy: {(CurrentAccuracy * 100):F1}%");
            GUILayout.Label("Controls: Q/E or Mouse Wheel = Adjust | Space = Submit");
            GUILayout.EndArea();
        }
        #endregion
    }
}
