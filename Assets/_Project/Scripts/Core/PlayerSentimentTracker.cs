using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 7: Player sentiment tracker for beta testing.
    /// Detects behavioral patterns that indicate frustration or engagement:
    /// - Rage quits (quit within 60s of death/failure)
    /// - Session length trends (short sessions = possible frustration)
    /// - Restart frequency (retrying same quest repeatedly)
    /// - Idle time (stuck/confused players)
    /// - Input spam (mashing buttons in frustration)
    ///
    /// This is NOT invasive telemetry — it's aggregate behavioral analysis
    /// to help identify pain points early in beta testing.
    ///
    /// All data is LOCAL ONLY (no cloud upload without explicit consent).
    /// </summary>
    public class PlayerSentimentTracker : MonoBehaviour
    {
        static PlayerSentimentTracker _instance;
        string _sentimentDir;

        // Session tracking
        float _sessionStartTime;
        float _lastInputTime;
        float _totalIdleTime;
        int _sessionCount;

        // Behavioral flags
        bool _playerDiedRecently;
        float _deathTime;
        const float RAGE_QUIT_WINDOW_SEC = 60f; // Quit within 60s of death = possible rage quit

        int _consecutiveDeaths;
        string _lastFailedQuest;
        int _questRestartCount;

        float _lastFrameTime;
        int _inputSpamCount;
        const int INPUT_SPAM_THRESHOLD = 10; // 10+ inputs in 1 second = spam
        float _inputSpamWindow;

        // Metrics
        List<float> _sessionLengths = new List<float>();
        int _rageQuitCount;
        int _longIdleCount; // >5min idle
        int _inputSpamEvents;

        // Sentiment report
        SentimentReport _currentReport;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[PlayerSentimentTracker]");
            _instance = go.AddComponent<PlayerSentimentTracker>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            _sentimentDir = Path.Combine(Application.dataPath, "..", "Logs", "Sentiment");
            if (!Directory.Exists(_sentimentDir))
                Directory.CreateDirectory(_sentimentDir);

            _sessionStartTime = Time.realtimeSinceStartup;
            _lastInputTime = Time.realtimeSinceStartup;
            _lastFrameTime = Time.realtimeSinceStartup;
            _sessionCount++;

            _currentReport = new SentimentReport
            {
                sessionID = SystemInfo.deviceUniqueIdentifier,
                sessionStartTime = DateTime.Now
            };

            LoadHistoricalData();

            Debug.Log($"[PlayerSentimentTracker] Initialized. Session #{_sessionCount}");
        }

        void Update()
        {
            float deltaTime = Time.realtimeSinceStartup - _lastFrameTime;
            _lastFrameTime = Time.realtimeSinceStartup;

            // Input activity tracking
            bool hasInput = (Keyboard.current != null && Keyboard.current.anyKey.isPressed) ||
                            (Mouse.current != null && (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed));
            if (hasInput)
            {
                // Input spam detection
                if (Time.realtimeSinceStartup - _inputSpamWindow < 1f)
                {
                    _inputSpamCount++;
                    if (_inputSpamCount >= INPUT_SPAM_THRESHOLD)
                    {
                        RecordInputSpam();
                        _inputSpamCount = 0;
                        _inputSpamWindow = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    _inputSpamCount = 1;
                    _inputSpamWindow = Time.realtimeSinceStartup;
                }

                _lastInputTime = Time.realtimeSinceStartup;
            }
            else
            {
                // Idle time tracking
                float idleTime = Time.realtimeSinceStartup - _lastInputTime;
                if (idleTime > 300f && idleTime % 60f < deltaTime) // Every 60s after 5min idle
                {
                    _totalIdleTime += 60f;
                    RecordLongIdle(idleTime);
                }
            }
        }

        void OnApplicationQuit()
        {
            // Check for rage quit
            if (_playerDiedRecently)
            {
                float timeSinceDeath = Time.realtimeSinceStartup - _deathTime;
                if (timeSinceDeath < RAGE_QUIT_WINDOW_SEC)
                {
                    _rageQuitCount++;
                    _currentReport.rageQuitDetected = true;
                    _currentReport.timeSinceLastDeath = timeSinceDeath;
                    Debug.Log($"[PlayerSentimentTracker] Possible rage quit detected ({timeSinceDeath:F1}s after death)");
                }
            }

            // Record session length
            float sessionLength = Time.realtimeSinceStartup - _sessionStartTime;
            _sessionLengths.Add(sessionLength);
            _currentReport.sessionLengthSeconds = (int)sessionLength;

            // Save sentiment report
            SaveSentimentReport();
            SaveHistoricalData();

            Debug.Log($"[PlayerSentimentTracker] Session ended. Length: {sessionLength:F0}s, RageQuits: {_rageQuitCount}");
        }

        void LoadHistoricalData()
        {
            try
            {
                string filepath = Path.Combine(_sentimentDir, "sentiment-history.txt");
                if (File.Exists(filepath))
                {
                    string[] lines = File.ReadAllLines(filepath);
                    if (lines.Length >= 4)
                    {
                        _rageQuitCount = int.Parse(lines[0].Split(':')[1].Trim());
                        _longIdleCount = int.Parse(lines[1].Split(':')[1].Trim());
                        _inputSpamEvents = int.Parse(lines[2].Split(':')[1].Trim());
                        _sessionCount = int.Parse(lines[3].Split(':')[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerSentimentTracker] Failed to load historical data: {ex.Message}");
            }
        }

        void SaveHistoricalData()
        {
            try
            {
                string filepath = Path.Combine(_sentimentDir, "sentiment-history.txt");
                using (var writer = new StreamWriter(filepath, false))
                {
                    writer.WriteLine($"TotalRageQuits: {_rageQuitCount}");
                    writer.WriteLine($"TotalLongIdles: {_longIdleCount}");
                    writer.WriteLine($"TotalInputSpamEvents: {_inputSpamEvents}");
                    writer.WriteLine($"TotalSessions: {_sessionCount}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerSentimentTracker] Failed to save historical data: {ex.Message}");
            }
        }

        void SaveSentimentReport()
        {
            try
            {
                string filename = $"sentiment-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                string filepath = Path.Combine(_sentimentDir, filename);

                using (var writer = new StreamWriter(filepath, false))
                {
                    writer.WriteLine("=== PLAYER SENTIMENT REPORT ===");
                    writer.WriteLine($"Session Start: {_currentReport.sessionStartTime:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Session Length: {_currentReport.sessionLengthSeconds}s ({_currentReport.sessionLengthSeconds / 60f:F1}min)");
                    writer.WriteLine($"Session ID: {_currentReport.sessionID}");
                    writer.WriteLine();

                    writer.WriteLine("=== BEHAVIORAL FLAGS ===");
                    writer.WriteLine($"Rage Quit: {_currentReport.rageQuitDetected}");
                    if (_currentReport.rageQuitDetected)
                    {
                        writer.WriteLine($"  Time Since Death: {_currentReport.timeSinceLastDeath:F1}s");
                    }
                    writer.WriteLine($"Long Idle Events: {_longIdleCount}");
                    writer.WriteLine($"Input Spam Events: {_inputSpamEvents}");
                    writer.WriteLine($"Consecutive Deaths: {_consecutiveDeaths}");
                    writer.WriteLine($"Quest Restarts: {_questRestartCount}");
                    if (!string.IsNullOrEmpty(_lastFailedQuest))
                    {
                        writer.WriteLine($"  Last Failed Quest: {_lastFailedQuest}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("=== SESSION HISTORY ===");
                    writer.WriteLine($"Total Sessions: {_sessionCount}");
                    writer.WriteLine($"Total Rage Quits: {_rageQuitCount}");
                    writer.WriteLine($"Rage Quit Rate: {(_rageQuitCount / (float)_sessionCount * 100f):F1}%");

                    if (_sessionLengths.Count > 0)
                    {
                        float avgSession = 0f;
                        foreach (var length in _sessionLengths)
                            avgSession += length;
                        avgSession /= _sessionLengths.Count;

                        writer.WriteLine($"Average Session: {avgSession / 60f:F1}min");
                        writer.WriteLine($"Shortest Session: {GetMinSessionLength() / 60f:F1}min");
                        writer.WriteLine($"Longest Session: {GetMaxSessionLength() / 60f:F1}min");
                    }

                    writer.WriteLine("\n=== END SENTIMENT REPORT ===");
                }

                Debug.Log($"[PlayerSentimentTracker] Sentiment report saved: {filename}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerSentimentTracker] Failed to save sentiment report: {ex.Message}");
            }
        }

        void RecordInputSpam()
        {
            _inputSpamEvents++;
            Debug.Log("[PlayerSentimentTracker] Input spam detected (possible frustration)");
        }

        void RecordLongIdle(float idleTime)
        {
            _longIdleCount++;
            Debug.Log($"[PlayerSentimentTracker] Long idle detected: {idleTime:F0}s");
        }

        float GetMinSessionLength()
        {
            if (_sessionLengths.Count == 0) return 0f;
            float min = float.MaxValue;
            foreach (var length in _sessionLengths)
                if (length < min) min = length;
            return min;
        }

        float GetMaxSessionLength()
        {
            if (_sessionLengths.Count == 0) return 0f;
            float max = 0f;
            foreach (var length in _sessionLengths)
                if (length > max) max = length;
            return max;
        }

        // Public API for other systems to report events
        public static void RecordPlayerDeath(string questID = null)
        {
            if (_instance == null) return;

            _instance._playerDiedRecently = true;
            _instance._deathTime = Time.realtimeSinceStartup;
            _instance._consecutiveDeaths++;

            if (!string.IsNullOrEmpty(questID))
            {
                if (_instance._lastFailedQuest == questID)
                {
                    _instance._questRestartCount++;
                }
                else
                {
                    _instance._lastFailedQuest = questID;
                    _instance._questRestartCount = 1;
                }
            }

            Debug.Log($"[PlayerSentimentTracker] Player died in {questID ?? "unknown"} (deaths: {_instance._consecutiveDeaths})");
        }

        public static void RecordQuestSuccess(string questID)
        {
            if (_instance == null) return;

            // Reset death counters on success
            _instance._consecutiveDeaths = 0;
            _instance._lastFailedQuest = null;
            _instance._questRestartCount = 0;
            Debug.Log($"[PlayerSentimentTracker] Quest success: {questID}");
        }

        public static SentimentMetrics GetMetrics()
        {
            if (_instance == null) return new SentimentMetrics();

            return new SentimentMetrics
            {
                totalSessions = _instance._sessionCount,
                rageQuitCount = _instance._rageQuitCount,
                rageQuitRate = _instance._rageQuitCount / (float)_instance._sessionCount,
                averageSessionLength = _instance.GetAverageSessionLength(),
                longIdleCount = _instance._longIdleCount,
                inputSpamCount = _instance._inputSpamEvents,
                consecutiveDeaths = _instance._consecutiveDeaths,
                questRestartCount = _instance._questRestartCount
            };
        }

        float GetAverageSessionLength()
        {
            if (_sessionLengths.Count == 0) return 0f;
            float sum = 0f;
            foreach (var length in _sessionLengths)
                sum += length;
            return sum / _sessionLengths.Count;
        }
    }

    // Sentiment report data structure
    [Serializable]
    public class SentimentReport
    {
        public string sessionID;
        public DateTime sessionStartTime;
        public int sessionLengthSeconds;
        public bool rageQuitDetected;
        public float timeSinceLastDeath;
    }

    // Sentiment metrics (for dashboard)
    [Serializable]
    public struct SentimentMetrics
    {
        public int totalSessions;
        public int rageQuitCount;
        public float rageQuitRate; // 0.0 - 1.0
        public float averageSessionLength; // seconds
        public int longIdleCount;
        public int inputSpamCount;
        public int consecutiveDeaths;
        public int questRestartCount;
    }
}
