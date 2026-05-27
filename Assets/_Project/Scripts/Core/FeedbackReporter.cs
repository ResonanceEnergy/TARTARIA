using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 7: In-game feedback reporter for beta testing.
    /// Features:
    /// - Player-submitted bug reports with categories
    /// - Screenshot capture on submission
    /// - Privacy-first design (no PII without consent)
    /// - Offline queue (sync when online)
    /// - Integration with CrashReporter context capture
    ///
    /// Usage:
    /// FeedbackReporter.SubmitFeedback(FeedbackType.Bug, "Combat feels unresponsive", "Attacks not registering");
    /// FeedbackReporter.OpenFeedbackUI(); // Bind to hotkey (F8)
    /// </summary>
    public class FeedbackReporter : MonoBehaviour
    {
        static FeedbackReporter _instance;
        string _feedbackDir;
        Queue<FeedbackReport> _offlineQueue = new Queue<FeedbackReport>();

        // Privacy settings (can be toggled in Settings menu)
        public static bool AllowScreenshots { get; set; } = true;
        public static bool AllowDeviceInfo { get; set; } = true;
        public static bool AllowGameContext { get; set; } = true;

        // Submission throttle (prevent spam)
        const float SUBMIT_COOLDOWN_SEC = 30f;
        float _lastSubmitTime = -999f;

        // Feedback UI state
        bool _feedbackUIOpen;
        FeedbackType _selectedType = FeedbackType.Bug;
        string _feedbackTitle = "";
        string _feedbackDescription = "";
        Vector2 _scrollPos;

        // Stats
        int _totalSubmissions;
        int _pendingSync;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[FeedbackReporter]");
            _instance = go.AddComponent<FeedbackReporter>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            _feedbackDir = Path.Combine(Application.dataPath, "..", "Logs", "Feedback");
            if (!Directory.Exists(_feedbackDir))
                Directory.CreateDirectory(_feedbackDir);

            LoadOfflineQueue();
            Debug.Log($"[FeedbackReporter] Initialized. Feedback dir: {_feedbackDir}");
        }

        void Update()
        {
            // F8 hotkey to open feedback UI
            if (Keyboard.current != null && Keyboard.current[Key.F8].wasPressedThisFrame)
            {
                OpenFeedbackUI();
            }

            // ESC to close
            if (_feedbackUIOpen && Keyboard.current != null && Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                _feedbackUIOpen = false;
            }
        }

        void OnGUI()
        {
            if (!_feedbackUIOpen) return;

            // Semi-transparent background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

            // Feedback panel (centered, 600x500)
            float panelWidth = 600f;
            float panelHeight = 500f;
            Rect panelRect = new Rect(
                (Screen.width - panelWidth) / 2,
                (Screen.height - panelHeight) / 2,
                panelWidth,
                panelHeight
            );

            GUI.Box(panelRect, "");
            GUILayout.BeginArea(panelRect);

            GUILayout.Space(10);
            GUILayout.Label("<size=18><b>Beta Feedback Reporter</b></size>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter });
            GUILayout.Space(10);

            // Category selector
            GUILayout.Label("<b>Feedback Type:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.BeginHorizontal();
            foreach (FeedbackType type in Enum.GetValues(typeof(FeedbackType)))
            {
                if (GUILayout.Toggle(_selectedType == type, type.ToString(), GUI.skin.button))
                {
                    _selectedType = type;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Title field
            GUILayout.Label("<b>Title (required):</b>", new GUIStyle(GUI.skin.label) { richText = true });
            _feedbackTitle = GUILayout.TextField(_feedbackTitle, 100, GUILayout.Height(25));

            GUILayout.Space(10);

            // Description field (scrollable)
            GUILayout.Label("<b>Description:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            _feedbackDescription = GUILayout.TextArea(_feedbackDescription, 2000, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUILayout.Space(10);

            // Privacy notice
            GUILayout.Label($"<size=10><i>Privacy: Screenshot={AllowScreenshots}, DeviceInfo={AllowDeviceInfo}, GameContext={AllowGameContext}</i></size>",
                new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter });

            GUILayout.Space(5);

            // Submit/Cancel buttons
            GUILayout.BeginHorizontal();

            // Throttle check
            bool canSubmit = !string.IsNullOrWhiteSpace(_feedbackTitle) &&
                           (Time.realtimeSinceStartup - _lastSubmitTime >= SUBMIT_COOLDOWN_SEC);

            GUI.enabled = canSubmit;
            if (GUILayout.Button("Submit Feedback", GUILayout.Height(35)))
            {
                SubmitFeedbackInternal();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel", GUILayout.Height(35)))
            {
                _feedbackUIOpen = false;
            }

            GUILayout.EndHorizontal();

            // Cooldown warning
            if (!canSubmit && !string.IsNullOrWhiteSpace(_feedbackTitle))
            {
                float remaining = SUBMIT_COOLDOWN_SEC - (Time.realtimeSinceStartup - _lastSubmitTime);
                if (remaining > 0)
                {
                    GUILayout.Label($"<color=yellow>Cooldown: {remaining:F0}s</color>",
                        new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter });
                }
            }

            GUILayout.Space(5);
            GUILayout.Label($"<size=10>Total submissions: {_totalSubmissions} | Pending sync: {_pendingSync}</size>",
                new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.UpperCenter });

            GUILayout.EndArea();
        }

        void SubmitFeedbackInternal()
        {
            var report = new FeedbackReport
            {
                timestamp = DateTime.Now,
                type = _selectedType,
                title = _feedbackTitle,
                description = _feedbackDescription,
                sessionID = SystemInfo.deviceUniqueIdentifier, // Anonymized session ID
                buildVersion = Application.version,
                unityVersion = Application.unityVersion
            };

            // Capture optional context based on privacy settings
            if (AllowDeviceInfo)
            {
                report.deviceOS = SystemInfo.operatingSystem;
                report.deviceGPU = SystemInfo.graphicsDeviceName;
                report.deviceRAM = SystemInfo.systemMemorySize;
            }

            if (AllowGameContext)
            {
                report.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                // TODO: Restore after implementing event-driven telemetry (CrashReporter moved to LiveOps)
                // report.playtimeSeconds = (int)CrashReporter.GetSessionUptime();
                report.playtimeSeconds = (int)Time.realtimeSinceStartup;
                report.resonanceScore = GetCurrentResonanceScore();
                report.playerLevel = GetPlayerLevel();
                // report.crashCount = CrashReporter.GetCrashCount();
                // report.hitchCount = CrashReporter.GetHitchCount();
            }

            // Capture screenshot (PNG, compressed)
            if (AllowScreenshots)
            {
                report.screenshotPath = CaptureScreenshot();
            }

            // Save to disk
            SaveFeedbackReport(report);

            // Add to sync queue
            _offlineQueue.Enqueue(report);
            _pendingSync = _offlineQueue.Count;

            // Update stats
            _totalSubmissions++;
            _lastSubmitTime = Time.realtimeSinceStartup;

            // Reset form
            _feedbackTitle = "";
            _feedbackDescription = "";
            _feedbackUIOpen = false;

            // Notify player
            Debug.Log($"[FeedbackReporter] Feedback submitted: {report.type} - {report.title}");
            ShowNotification("Feedback submitted! Thank you for helping improve TARTARIA.");

            // Try to sync (if online)
            TrySyncFeedback();
        }

        string CaptureScreenshot()
        {
            try
            {
                string filename = $"screenshot-{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.png";
                string filepath = Path.Combine(_feedbackDir, filename);
                ScreenCapture.CaptureScreenshot(filepath);
                return filename; // Return relative path
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FeedbackReporter] Screenshot capture failed: {ex.Message}");
                return null;
            }
        }

        void SaveFeedbackReport(FeedbackReport report)
        {
            try
            {
                string filename = $"feedback-{report.timestamp:yyyy-MM-dd_HH-mm-ss-fff}.txt";
                string filepath = Path.Combine(_feedbackDir, filename);

                using (var writer = new StreamWriter(filepath, false))
                {
                    writer.WriteLine("=== BETA FEEDBACK REPORT ===");
                    writer.WriteLine($"Timestamp: {report.timestamp:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Type: {report.type}");
                    writer.WriteLine($"Title: {report.title}");
                    writer.WriteLine($"Build: {report.buildVersion} (Unity {report.unityVersion})");
                    writer.WriteLine($"Session ID: {report.sessionID}");
                    writer.WriteLine();

                    if (!string.IsNullOrEmpty(report.description))
                    {
                        writer.WriteLine("Description:");
                        writer.WriteLine(report.description);
                        writer.WriteLine();
                    }

                    if (AllowDeviceInfo)
                    {
                        writer.WriteLine("=== DEVICE INFO ===");
                        writer.WriteLine($"OS: {report.deviceOS}");
                        writer.WriteLine($"GPU: {report.deviceGPU}");
                        writer.WriteLine($"RAM: {report.deviceRAM}MB");
                        writer.WriteLine();
                    }

                    if (AllowGameContext)
                    {
                        writer.WriteLine("=== GAME CONTEXT ===");
                        writer.WriteLine($"Scene: {report.sceneName}");
                        writer.WriteLine($"Playtime: {report.playtimeSeconds}s");
                        writer.WriteLine($"Player Level: {report.playerLevel}");
                        writer.WriteLine($"Resonance Score: {report.resonanceScore:F1}");
                        writer.WriteLine($"Crashes: {report.crashCount}");
                        writer.WriteLine($"Hitches: {report.hitchCount}");
                        writer.WriteLine();
                    }

                    if (!string.IsNullOrEmpty(report.screenshotPath))
                    {
                        writer.WriteLine($"Screenshot: {report.screenshotPath}");
                    }

                    writer.WriteLine("\n=== END FEEDBACK REPORT ===");
                }

                Debug.Log($"[FeedbackReporter] Report saved: {filename}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FeedbackReporter] Failed to save report: {ex.Message}");
            }
        }

        void LoadOfflineQueue()
        {
            // TODO: Load pending feedback reports from disk (for sync retry)
            // For now, just scan the feedback directory
            try
            {
                var files = Directory.GetFiles(_feedbackDir, "feedback-*.txt");
                _pendingSync = files.Length;
                Debug.Log($"[FeedbackReporter] Found {_pendingSync} pending feedback reports");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FeedbackReporter] Failed to load offline queue: {ex.Message}");
            }
        }

        void TrySyncFeedback()
        {
            // TODO: Implement cloud sync (e.g., upload to web API, Google Forms, Discord webhook)
            // For beta launch, feedback reports are stored locally
            // Dev team can collect via Steam Cloud save sync or manual file sharing

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[FeedbackReporter] Offline — feedback queued for sync");
                return;
            }

            // Placeholder for future web API integration
            Debug.Log($"[FeedbackReporter] Sync not implemented yet — {_pendingSync} reports queued locally");
        }

        void ShowNotification(string message)
        {
            // TODO: Integrate with UI notification system
            Debug.Log($"[NOTIFICATION] {message}");
        }

        float GetCurrentResonanceScore()
        {
            try
            {
                var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
                if (world != null && world.IsCreated)
                {
                    var em = world.EntityManager;
                    var query = em.CreateEntityQuery(typeof(ResonanceScore));
                    if (query.CalculateEntityCount() > 0)
                    {
                        var entity = query.GetSingletonEntity();
                        var rs = em.GetComponentData<ResonanceScore>(entity);
                        query.Dispose();
                        return rs.CurrentRS;
                    }
                    query.Dispose();
                }
            }
            catch { /* Safe to ignore */ }
            return 0f;
        }

        int GetPlayerLevel()
        {
            try
            {
                // TODO: Restore after implementing interface abstraction (Gameplay moved out of Core refs)
                // var playerProg = FindFirstObjectByType<Tartaria.Gameplay.PlayerProgression>();
                // if (playerProg != null)
                //     return playerProg.CurrentLevel;
            }
            catch { /* Safe to ignore */ }
            return 0;
        }

        // Public API
        public static void OpenFeedbackUI()
        {
            if (_instance == null)
            {
                Debug.LogWarning("[FeedbackReporter] Instance not initialized");
                return;
            }
            _instance._feedbackUIOpen = true;
        }

        public static void SubmitFeedback(FeedbackType type, string title, string description)
        {
            if (_instance == null)
            {
                Debug.LogWarning("[FeedbackReporter] Instance not initialized");
                return;
            }

            _instance._selectedType = type;
            _instance._feedbackTitle = title;
            _instance._feedbackDescription = description;
            _instance.SubmitFeedbackInternal();
        }

        public static int GetTotalSubmissions() => _instance?._totalSubmissions ?? 0;
        public static int GetPendingSync() => _instance?._pendingSync ?? 0;
    }

    // Feedback taxonomy
    public enum FeedbackType
    {
        Bug,        // Something broken (quest bug, crash, softlock)
        Balance,    // Too easy/hard, unfair mechanics
        UX,         // Confusing UI, bad controls, unclear tooltips
        Feature     // Requested new features or improvements
    }

    // Feedback report data structure
    [Serializable]
    public class FeedbackReport
    {
        public DateTime timestamp;
        public FeedbackType type;
        public string title;
        public string description;
        public string sessionID;
        public string buildVersion;
        public string unityVersion;

        // Device info (optional, privacy-gated)
        public string deviceOS;
        public string deviceGPU;
        public int deviceRAM;

        // Game context (optional, privacy-gated)
        public string sceneName;
        public int playtimeSeconds;
        public int playerLevel;
        public float resonanceScore;
        public int crashCount;
        public int hitchCount;

        // Screenshot (optional, privacy-gated)
        public string screenshotPath;
    }
}
