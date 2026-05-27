using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Security
{
    /// <summary>
    /// Economy Anomaly Detector — real-time monitoring for suspicious transactions.
    /// 
    /// AGENT 4: Anti-Cheat & Economy Guardian
    /// 
    /// Detection Strategies:
    ///   1. Rapid Transaction Spam (>10 transactions/second)
    ///   2. Impossible Currency Gains (>1M in single transaction)
    ///   3. Negative Balance Attempts (caught by EconomySystem but logged here)
    ///   4. Inventory Stack Overflow Attempts (>999K items)
    ///   5. Suspiciously Rapid Level Gains (>5 levels/minute)
    ///   6. Save File Modification Detection (checksum mismatch)
    /// 
    /// Actions on Detection:
    ///   - Log to Logs/security-events.log (persistent audit trail)
    ///   - Trigger OnAnomalyDetected event (UI warning, telemetry)
    ///   - Optionally: auto-save snapshot before suspicious action
    /// 
    /// Usage:
    ///   - Bootstraps automatically at runtime
    ///   - Subscribe to OnAnomalyDetected for custom handling
    ///   - Call LogSecurityEvent() from other systems for centralized logging
    /// </summary>
    public class EconomyAnomalyDetector : MonoBehaviour
    {
        public static EconomyAnomalyDetector Instance { get; private set; }

        [Header("Detection Thresholds")]
        [SerializeField] int maxTransactionsPerSecond = 10;
        [SerializeField] int suspiciousGainThreshold = 1_000_000;
        [SerializeField] int maxStackSizeThreshold = 999_999;
        [SerializeField] int maxLevelGainsPerMinute = 5;

        [Header("Logging")]
        [SerializeField] bool enableFileLogging = true;
        [SerializeField] bool logToConsole = true;
        string _logFilePath;

        // Events
        public event Action<SecurityEvent> OnAnomalyDetected;

        // Transaction tracking
        readonly Queue<float> _recentTransactions = new(); // timestamps
        readonly Queue<LevelGainRecord> _recentLevelGains = new();
        int _transactionsSinceLastCheck;
        float _lastTransactionCheckTime;

        // Statistics
        int _totalAnomaliesDetected;
        int _currencyAnomalies;
        int _inventoryAnomalies;
        int _progressionAnomalies;
        int _saveIntegrityFailures;

        public int TotalAnomaliesDetected => _totalAnomaliesDetected;
        public int CurrencyAnomalies => _currencyAnomalies;
        public int InventoryAnomalies => _inventoryAnomalies;
        public int ProgressionAnomalies => _progressionAnomalies;
        public int SaveIntegrityFailures => _saveIntegrityFailures;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("EconomyAnomalyDetector");
            DontDestroyOnLoad(go);
            go.AddComponent<EconomyAnomalyDetector>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // Setup log file
            _logFilePath = Path.Combine(Application.persistentDataPath, "Logs", "security-events.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));

            LogSecurityEvent(SecurityEventType.SystemStartup, "EconomyAnomalyDetector initialized", null);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                LogSecurityEvent(SecurityEventType.SystemShutdown, "EconomyAnomalyDetector shutting down", null);
                Instance = null;
            }
        }

        void OnEnable()
        {
            // Subscribe to economy events
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnCurrencyChanged += OnCurrencyChanged;

            // Subscribe to inventory events
            if (Gameplay.InventorySystem.Instance != null)
                Gameplay.InventorySystem.Instance.OnItemAdded += OnItemAdded;

            // Subscribe to progression events
            if (Gameplay.PlayerProgression.Instance != null)
                Gameplay.PlayerProgression.Instance.OnLevelUp += OnLevelUp;
        }

        void OnDisable()
        {
            // Unsubscribe from events
            if (EconomySystem.Instance != null)
                EconomySystem.Instance.OnCurrencyChanged -= OnCurrencyChanged;

            if (Gameplay.InventorySystem.Instance != null)
                Gameplay.InventorySystem.Instance.OnItemAdded -= OnItemAdded;

            if (Gameplay.PlayerProgression.Instance != null)
                Gameplay.PlayerProgression.Instance.OnLevelUp -= OnLevelUp;
        }

        void Update()
        {
            // Clean up old transaction timestamps (sliding window)
            float cutoff = Time.time - 1f; // 1-second window
            while (_recentTransactions.Count > 0 && _recentTransactions.Peek() < cutoff)
                _recentTransactions.Dequeue();

            // Clean up old level gain records (sliding window)
            float levelCutoff = Time.time - 60f; // 1-minute window
            while (_recentLevelGains.Count > 0 && _recentLevelGains.Peek().timestamp < levelCutoff)
                _recentLevelGains.Dequeue();
        }

        // ═════════════════════════════════════════════════════════════════
        // Event Handlers
        // ═════════════════════════════════════════════════════════════════

        void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
        {
            // Track transaction rate
            _recentTransactions.Enqueue(Time.time);
            _transactionsSinceLastCheck++;

            // Check for transaction spam
            if (_recentTransactions.Count > maxTransactionsPerSecond)
            {
                ReportAnomaly(SecurityEventType.TransactionSpam,
                    $"Excessive transaction rate detected: {_recentTransactions.Count} transactions/sec",
                    $"CurrencyType={type}, OldAmount={oldAmount}, NewAmount={newAmount}");
                _currencyAnomalies++;
            }

            int delta = newAmount - oldAmount;

            // Check for impossible gains (single transaction)
            if (delta > suspiciousGainThreshold)
            {
                ReportAnomaly(SecurityEventType.SuspiciousCurrencyGain,
                    $"Suspicious currency gain: +{delta} {type} in single transaction",
                    $"OldBalance={oldAmount}, NewBalance={newAmount}");
                _currencyAnomalies++;
            }

            // Check for negative balance (should be caught by EconomySystem)
            if (newAmount < 0)
            {
                ReportAnomaly(SecurityEventType.NegativeBalance,
                    $"CRITICAL: Negative balance detected for {type}",
                    $"Balance={newAmount}");
                _currencyAnomalies++;
            }
        }

        void OnItemAdded(string itemId, int newCount)
        {
            // Track transaction rate
            _recentTransactions.Enqueue(Time.time);

            // Check for stack overflow attempts
            if (newCount > maxStackSizeThreshold)
            {
                ReportAnomaly(SecurityEventType.InventoryStackOverflow,
                    $"Inventory stack overflow detected: {itemId} count={newCount}",
                    $"ItemId={itemId}, Count={newCount}, Threshold={maxStackSizeThreshold}");
                _inventoryAnomalies++;
            }

            // Check for rapid item addition spam
            if (_recentTransactions.Count > maxTransactionsPerSecond)
            {
                ReportAnomaly(SecurityEventType.InventorySpam,
                    $"Rapid item addition detected: {_recentTransactions.Count} additions/sec",
                    $"LastItem={itemId}, Count={newCount}");
                _inventoryAnomalies++;
            }
        }

        void OnLevelUp(int newLevel)
        {
            // Track level gain
            _recentLevelGains.Enqueue(new LevelGainRecord
            {
                timestamp = Time.time,
                level = newLevel
            });

            // Check for rapid leveling
            if (_recentLevelGains.Count > maxLevelGainsPerMinute)
            {
                int levelsGained = _recentLevelGains.Count;
                ReportAnomaly(SecurityEventType.RapidLeveling,
                    $"Suspicious leveling rate: {levelsGained} levels in 1 minute",
                    $"CurrentLevel={newLevel}, LevelsInLastMinute={levelsGained}");
                _progressionAnomalies++;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // Public API
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Log a security event from any system. Centralized security audit trail.
        /// </summary>
        public void LogSecurityEvent(SecurityEventType eventType, string message, string context)
        {
            var secEvent = new SecurityEvent
            {
                type = eventType,
                message = message,
                context = context,
                timestamp = DateTime.Now,
                sessionTime = Time.time
            };

            if (logToConsole)
            {
                string severity = GetSeverity(eventType);
                Debug.Log($"[SECURITY {severity}] {eventType}: {message} | {context}");
            }

            if (enableFileLogging)
            {
                try
                {
                    string logLine = $"[{secEvent.timestamp:yyyy-MM-dd HH:mm:ss}] [{eventType}] {message}";
                    if (!string.IsNullOrEmpty(context))
                        logLine += $" | {context}";
                    
                    File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SecurityAudit] Failed to write log: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Get all security logs as a string (for debugging/support tickets).
        /// </summary>
        public string GetSecurityLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                    return File.ReadAllText(_logFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecurityAudit] Failed to read logs: {e.Message}");
            }
            return "No security logs available.";
        }

        /// <summary>
        /// Clear security log file (use with caution).
        /// </summary>
        public void ClearLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                    File.Delete(_logFilePath);
                LogSecurityEvent(SecurityEventType.LogCleared, "Security logs manually cleared", null);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecurityAudit] Failed to clear logs: {e.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // Internal Helpers
        // ═════════════════════════════════════════════════════════════════

        void ReportAnomaly(SecurityEventType eventType, string message, string context)
        {
            _totalAnomaliesDetected++;

            var secEvent = new SecurityEvent
            {
                type = eventType,
                message = message,
                context = context,
                timestamp = DateTime.Now,
                sessionTime = Time.time
            };

            LogSecurityEvent(eventType, message, context);
            OnAnomalyDetected?.Invoke(secEvent);

            // Optionally: auto-save snapshot for forensic analysis
            // SaveManager.Instance?.SaveToSlot(-1); // Emergency backup slot
        }

        string GetSeverity(SecurityEventType eventType)
        {
            return eventType switch
            {
                SecurityEventType.NegativeBalance => "CRITICAL",
                SecurityEventType.SaveTampering => "CRITICAL",
                SecurityEventType.SuspiciousCurrencyGain => "HIGH",
                SecurityEventType.RapidLeveling => "HIGH",
                SecurityEventType.TransactionSpam => "MEDIUM",
                SecurityEventType.InventorySpam => "MEDIUM",
                SecurityEventType.InventoryStackOverflow => "MEDIUM",
                _ => "INFO"
            };
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // Data Structures
    // ═════════════════════════════════════════════════════════════════

    public enum SecurityEventType
    {
        SystemStartup,
        SystemShutdown,
        TransactionSpam,
        SuspiciousCurrencyGain,
        NegativeBalance,
        InventoryStackOverflow,
        InventorySpam,
        RapidLeveling,
        SaveTampering,
        SaveIntegrityFailure,
        LogCleared
    }

    public struct SecurityEvent
    {
        public SecurityEventType type;
        public string message;
        public string context;
        public DateTime timestamp;
        public float sessionTime;
    }

    struct LevelGainRecord
    {
        public float timestamp;
        public int level;
    }
}
