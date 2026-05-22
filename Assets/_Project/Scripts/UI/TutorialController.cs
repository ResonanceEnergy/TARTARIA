using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.UI
{
    /// <summary>
    /// Tutorial Controller — context-sensitive hints system for new players.
    /// Complements TutorialOverlay (FTUE story beats) with gameplay-driven micro-hints.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Show hints when they matter (just-in-time learning)
    /// - Track completion in PlayerPrefs (never repeat completed hints)
    /// - Auto-dismiss after duration OR when action completed
    /// - Routes through HUDController.ShowAccessibilityHint() for visual polish
    /// 
    /// HINT CATALOG (6 core triggers):
    /// 1. Movement: "WASD to Move, Mouse to Look" (5s, on spawn)
    /// 2. Interaction: "Press E to Interact" (persistent until E pressed)
    /// 3. Combat: "Left Click to Attack" (persistent until first hit)
    /// 4. Low RS: "Restore buildings to gain Resonance Score" (until RS > 50)
    /// 5. Level Up: "You have stat points to allocate! Open menu with Tab" (until menu opened)
    /// 6. Death: "You respawned at the last checkpoint. Be more careful!" (5s)
    /// 
    /// INTEGRATION:
    /// - Subscribes to GameEvents (OnLevelUp, OnEnemyKilled, OnBuildingRestored)
    /// - Subscribes to PlayerInputHandler (OnInteract)
    /// - Polls GameStateManager, PlayerProgression, IntegrationBridge for conditions
    /// 
    /// CS:0 COMPLIANCE: Uses reflection facades (IntegrationBridge) for cross-assembly calls.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController Instance { get; private set; }

        [Header("Hint Settings")]
        [SerializeField, Tooltip("Default hint display duration (seconds)")] 
        float defaultHintDuration = 5f;
        
        [SerializeField, Tooltip("Delay before first hint triggers (seconds)")]
        float initialDelay = 1f;
        
        [SerializeField, Tooltip("RS threshold for low-RS hint dismissal")]
        float lowRSThreshold = 50f;

        // PlayerPrefs keys for completed hints
        const string PP_PREFIX = "TARTARIA_Hint_";
        const string HINT_MOVEMENT = "Movement_v1";
        const string HINT_INTERACT = "Interact_v1";
        const string HINT_COMBAT = "Combat_v1";
        const string HINT_LOW_RS = "LowRS_v1";
        const string HINT_LEVEL_UP = "LevelUp_v1";
        const string HINT_DEATH = "Death_v1";

        // State tracking
        HashSet<string> _completedHints = new HashSet<string>();
        string _activeHintId;
        float _hintTimer;
        float _initTimer;
        bool _initialized;
        
        // Condition flags
        bool _playerSpawned;
        bool _nearBuilding;
        bool _hasAttacked;
        bool _hasInteracted;
        bool _menuOpened;
        bool _justDied;
        
        // Cached references
        Transform _playerTransform;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("TutorialController");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<TutorialController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            }
            Instance = this;
            
            LoadCompletedHints();
            _initTimer = initialDelay;
        }

        void Start()
        {
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this) Instance = null;
        }

        void SubscribeToEvents()
        {
            // GameEvents subscriptions
            GameEvents.OnLevelUp += HandleLevelUp;
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnBuildingRestoredTyped += HandleBuildingRestored;
            
            // PlayerInputHandler subscriptions
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnInteract += HandleInteract;
                PlayerInputHandler.Instance.OnHarmonicStrike += HandleAttack;
            }
            
            // Subscribe to menu open events (via GameStateManager)
            GameEvents.OnTogglePause += HandleMenuOpened;
        }

        void UnsubscribeFromEvents()
        {
            GameEvents.OnLevelUp -= HandleLevelUp;
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnBuildingRestoredTyped -= HandleBuildingRestored;
            
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnInteract -= HandleInteract;
                PlayerInputHandler.Instance.OnHarmonicStrike -= HandleAttack;
            }
            
            GameEvents.OnTogglePause -= HandleMenuOpened;
        }

        void Update()
        {
            // Wait for initial delay before checking conditions
            if (!_initialized)
            {
                _initTimer -= Time.deltaTime;
                if (_initTimer <= 0f)
                {
                    _initialized = true;
                    DetectPlayerSpawn();
                }
                return;
            }

            // Update active hint timer
            if (!string.IsNullOrEmpty(_activeHintId))
            {
                _hintTimer -= Time.deltaTime;
                if (_hintTimer <= 0f)
                {
                    DismissActiveHint();
                }
            }

            // Check hint conditions
            CheckMovementHint();
            CheckInteractHint();
            CheckCombatHint();
            CheckLowRSHint();
        }

        // ═══════════════════════════════════════════════════════════════════
        // HINT CONDITION CHECKS (Update() Polling)
        // ═══════════════════════════════════════════════════════════════════

        void CheckMovementHint()
        {
            if (_completedHints.Contains(HINT_MOVEMENT)) return;
            if (!_playerSpawned) DetectPlayerSpawn();
            if (!_playerSpawned) return;

            // Show movement hint on player spawn (auto-dismiss after 5s)
            ShowHint(
                HINT_MOVEMENT, 
                "WASD to Move, Mouse to Look", 
                defaultHintDuration
            );
        }

        void CheckInteractHint()
        {
            if (_completedHints.Contains(HINT_INTERACT)) return;
            if (_hasInteracted) return;
            
            // Check if player is near a building
            _nearBuilding = DetectNearbyBuilding();
            
            if (_nearBuilding && string.IsNullOrEmpty(_activeHintId))
            {
                // Show persistent hint until player presses E
                ShowHint(
                    HINT_INTERACT,
                    "Press E to Interact",
                    0f // Persistent until action completed
                );
            }
        }

        void CheckCombatHint()
        {
            if (_completedHints.Contains(HINT_COMBAT)) return;
            if (_hasAttacked) return;
            
            // Check if enemies are present
            bool enemiesPresent = DetectNearbyEnemies();
            
            if (enemiesPresent && string.IsNullOrEmpty(_activeHintId))
            {
                ShowHint(
                    HINT_COMBAT,
                    "Left Click to Attack",
                    0f // Persistent until first hit
                );
            }
        }

        void CheckLowRSHint()
        {
            if (_completedHints.Contains(HINT_LOW_RS)) return;
            
            // Get current RS via IntegrationBridge (reflection facade for cross-assembly)
            float currentRS = GetCurrentResonanceScore();
            
            if (currentRS < lowRSThreshold && string.IsNullOrEmpty(_activeHintId))
            {
                ShowHint(
                    HINT_LOW_RS,
                    "Restore buildings to gain Resonance Score",
                    0f // Persistent until RS > 50
                );
            }
            else if (currentRS >= lowRSThreshold && _activeHintId == HINT_LOW_RS)
            {
                // Auto-complete when condition met
                CompleteHint(HINT_LOW_RS);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENT HANDLERS (Action Completion Detection)
        // ═══════════════════════════════════════════════════════════════════

        void HandleInteract()
        {
            _hasInteracted = true;
            CompleteHint(HINT_INTERACT);
        }

        void HandleAttack()
        {
            _hasAttacked = true;
            CompleteHint(HINT_COMBAT);
        }

        void HandleLevelUp(LevelUpEventArgs args)
        {
            if (_completedHints.Contains(HINT_LEVEL_UP)) return;
            if (_menuOpened) return;
            
            ShowHint(
                HINT_LEVEL_UP,
                "You have stat points to allocate! Open menu with Tab",
                0f // Persistent until menu opened
            );
        }

        void HandleMenuOpened()
        {
            _menuOpened = true;
            CompleteHint(HINT_LEVEL_UP);
        }

        void HandleEnemyKilled(EnemyKilledEventArgs args)
        {
            // Mark attack as performed when enemy killed
            _hasAttacked = true;
            CompleteHint(HINT_COMBAT);
        }

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            // Building restored - check if low RS hint should be dismissed
            float currentRS = GetCurrentResonanceScore();
            if (currentRS >= lowRSThreshold)
            {
                CompleteHint(HINT_LOW_RS);
            }
        }

        /// <summary>
        /// Called by external systems (e.g., PlayerHealth) when player dies.
        /// </summary>
        public void NotifyPlayerDeath()
        {
            if (_completedHints.Contains(HINT_DEATH)) return;
            
            _justDied = true;
            ShowHint(
                HINT_DEATH,
                "You respawned at the last checkpoint. Be more careful!",
                defaultHintDuration
            );
            CompleteHint(HINT_DEATH); // Only show once per session
        }

        // ═══════════════════════════════════════════════════════════════════
        // HINT DISPLAY & MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════

        void ShowHint(string hintId, string message, float duration)
        {
            // Already completed?
            if (_completedHints.Contains(hintId)) return;
            
            // Already showing this hint?
            if (_activeHintId == hintId) return;
            
            // Different hint active? Dismiss it first
            if (!string.IsNullOrEmpty(_activeHintId))
            {
                DismissActiveHint();
            }

            // Display via HUDController
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowAccessibilityHint("Tutorial", message);
            }
            else
            {
                Debug.Log($"[Tutorial] {message}");
            }

            _activeHintId = hintId;
            _hintTimer = duration > 0f ? duration : float.MaxValue; // Persistent if 0
        }

        void DismissActiveHint()
        {
            _activeHintId = null;
            _hintTimer = 0f;
        }

        void CompleteHint(string hintId)
        {
            if (_completedHints.Contains(hintId)) return;
            
            _completedHints.Add(hintId);
            PlayerPrefs.SetInt(PP_PREFIX + hintId, 1);
            PlayerPrefs.Save();
            
            // Dismiss if this hint is active
            if (_activeHintId == hintId)
            {
                DismissActiveHint();
            }
        }

        void LoadCompletedHints()
        {
            _completedHints.Clear();
            
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_MOVEMENT, 0) == 1)
                _completedHints.Add(HINT_MOVEMENT);
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_INTERACT, 0) == 1)
                _completedHints.Add(HINT_INTERACT);
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_COMBAT, 0) == 1)
                _completedHints.Add(HINT_COMBAT);
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_LOW_RS, 0) == 1)
                _completedHints.Add(HINT_LOW_RS);
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_LEVEL_UP, 0) == 1)
                _completedHints.Add(HINT_LEVEL_UP);
            if (PlayerPrefs.GetInt(PP_PREFIX + HINT_DEATH, 0) == 1)
                _completedHints.Add(HINT_DEATH);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CONDITION DETECTION (Reflection & Scene Queries)
        // ═══════════════════════════════════════════════════════════════════

        void DetectPlayerSpawn()
        {
            // Try to find player via IntegrationBridge or GameObject.FindWithTag
            if (_playerTransform == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null)
                {
                    _playerTransform = playerGO.transform;
                    _playerSpawned = true;
                }
            }
        }

        bool DetectNearbyBuilding()
        {
            if (_playerTransform == null) return false;
            
            // Use OverlapSphere to detect nearby buildings (layer-based)
            Collider[] hits = Physics.OverlapSphere(_playerTransform.position, 5f);
            foreach (var hit in hits)
            {
                // Check for InteractableBuilding component or Building tag
                if (hit.GetComponent<Tartaria.Integration.InteractableBuilding>() != null)
                    return true;
                if (hit.CompareTag("Building") || hit.CompareTag("Interactable"))
                    return true;
            }
            
            return false;
        }

        bool DetectNearbyEnemies()
        {
            if (_playerTransform == null) return false;
            
            // Use OverlapSphere to detect nearby enemies (layer-based)
            Collider[] hits = Physics.OverlapSphere(_playerTransform.position, 20f);
            foreach (var hit in hits)
            {
                // Check for Enemy tag or MudGolemHealth component
                if (hit.CompareTag("Enemy"))
                    return true;
                if (hit.GetComponent<Tartaria.AI.MudGolemHealth>() != null)
                    return true;
            }
            
            return false;
        }

        float GetCurrentResonanceScore()
        {
            // Use IntegrationBridge reflection facade (cross-assembly safe)
            return IntegrationBridge.GetResonanceScore();
        }

        // ═══════════════════════════════════════════════════════════════════
        // PUBLIC API (External Systems)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Reset all tutorial hints (dev testing / new game+).
        /// </summary>
        public void ResetAllHints()
        {
            _completedHints.Clear();
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_MOVEMENT);
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_INTERACT);
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_COMBAT);
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_LOW_RS);
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_LEVEL_UP);
            PlayerPrefs.DeleteKey(PP_PREFIX + HINT_DEATH);
            PlayerPrefs.Save();
            
            DismissActiveHint();
        }

        /// <summary>
        /// Force-show a custom hint (for special events or quest-driven tutorials).
        /// </summary>
        public void ShowCustomHint(string message, float duration = 5f)
        {
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowAccessibilityHint("Tutorial", message);
            }
        }
    }
}
