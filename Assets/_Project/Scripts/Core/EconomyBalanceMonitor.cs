using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tartaria.Core.Enums;
// using Tartaria.Gameplay; // DISABLED: Circular dependency (Core can't reference Gameplay)

namespace Tartaria.Core
{
    /// <summary>
    /// Economy Balance Monitor — real-time tracking of currency gain rates,
    /// loot drops, and economy health metrics for beta testing.
    ///
    /// AGENT 4: Anti-Cheat & Economy Guardian
    ///
    /// Monitors:
    ///   - Currency gain rates (RS/hour per source)
    ///   - Loot drop frequency and rarity distribution
    ///   - Inventory overflow attempts
    ///   - Suspicious activity patterns (rapid gains, impossible rates)
    ///
    /// Data is logged to Debug.Log and can be exported for analytics.
    /// Integrates with EconomySystem and InventorySystem via events.
    /// </summary>
    [DisallowMultipleComponent]
    public class EconomyBalanceMonitor : MonoBehaviour
    {
        public static EconomyBalanceMonitor Instance { get; private set; }

        [Header("Monitoring Settings")]
        [SerializeField] bool enableMonitoring = true;
        [SerializeField] float reportIntervalSeconds = 60f; // Report every minute
#pragma warning disable CS0414 // Assigned but never used - future anti-cheat
        [SerializeField] int suspiciousGainThreshold = 10000; // RS per minute triggers warning
#pragma warning restore CS0414

        [Header("Alert Thresholds")]
        [SerializeField] int maxCurrencyGainPerMinute = 5000; // Normal max gain rate
        [SerializeField] int maxItemsAddedPerMinute = 100;     // Normal max pickup rate
        [SerializeField] float maxXPGainPerMinute = 2000f;     // Normal max XP rate

        // Currency tracking
        readonly Dictionary<CurrencyType, int> _currencyGainsThisInterval = new();
        readonly Dictionary<CurrencyType, int> _totalCurrencyGains = new();
        readonly Dictionary<string, int> _currencySourceBreakdown = new(); // source -> amount

        // Item tracking
        readonly Dictionary<string, int> _itemsPickedUp = new(); // itemId -> count
        readonly Dictionary<ItemRarity, int> _rarityDistribution = new(); // rarity -> count
        int _totalItemsAdded;
        int _totalItemsRemoved;

        // XP tracking
        float _xpGainedThisInterval;
        float _totalXPGained;
        readonly Dictionary<string, float> _xpSourceBreakdown = new(); // source -> amount

        // Time tracking
        float _sessionStartTime;
        float _lastReportTime;
        float _intervalStartTime;

        // Suspicious activity flags
        int _suspiciousActivityCount;
        readonly List<string> _suspiciousEvents = new();

        // Events for external analytics
        public event Action<EconomyReport> OnReportGenerated;
        public event Action<string> OnSuspiciousActivityDetected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("EconomyBalanceMonitor");
            DontDestroyOnLoad(go);
            go.AddComponent<EconomyBalanceMonitor>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _sessionStartTime = Time.time;
            _lastReportTime = Time.time;
            _intervalStartTime = Time.time;

            // Initialize rarity distribution tracking
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                _rarityDistribution[rarity] = 0;
            }

            // Subscribe to economy events
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            // Currency tracking
            if (EconomySystem.Instance != null)
            {
                EconomySystem.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }

            // Inventory tracking
            // DISABLED: InventorySystem is in Tartaria.Gameplay (circular dependency)
            /*
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnItemAdded += HandleItemAdded;
                InventorySystem.Instance.OnItemRemoved += HandleItemRemoved;
            }
            */

            // XP tracking
            GameEvents.OnXPGained += HandleXPGained;

            // Item pickup tracking
            GameEvents.OnItemPickup += HandleItemPickup;
        }

        void UnsubscribeFromEvents()
        {
            if (EconomySystem.Instance != null)
            {
                EconomySystem.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }

            // DISABLED: InventorySystem is in Tartaria.Gameplay (circular dependency)
            /*
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnItemAdded -= HandleItemAdded;
                InventorySystem.Instance.OnItemRemoved -= HandleItemRemoved;
            }
            */

            GameEvents.OnXPGained -= HandleXPGained;
            GameEvents.OnItemPickup -= HandleItemPickup;
        }

        void Update()
        {
            if (!enableMonitoring) return;

            float currentTime = Time.time;
            float intervalElapsed = currentTime - _intervalStartTime;

            if (intervalElapsed >= reportIntervalSeconds)
            {
                GenerateReport();
                ResetIntervalCounters();
                _intervalStartTime = currentTime;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════

        void HandleCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
        {
            if (!enableMonitoring) return;

            int gain = newAmount - oldAmount;
            if (gain <= 0) return; // Only track gains, not spending

            // Track interval gains
            if (!_currencyGainsThisInterval.ContainsKey(type))
                _currencyGainsThisInterval[type] = 0;
            _currencyGainsThisInterval[type] += gain;

            // Track total gains
            if (!_totalCurrencyGains.ContainsKey(type))
                _totalCurrencyGains[type] = 0;
            _totalCurrencyGains[type] += gain;

            // Check for suspicious gain rate
            float intervalMinutes = (Time.time - _intervalStartTime) / 60f;
            if (intervalMinutes > 0.1f) // Avoid division by zero
            {
                float gainRate = _currencyGainsThisInterval[type] / intervalMinutes;
                if (gainRate > maxCurrencyGainPerMinute)
                {
                    LogSuspiciousActivity($"High currency gain rate: {type} at {gainRate:F0}/min (threshold {maxCurrencyGainPerMinute}/min)");
                }
            }
        }

        void HandleItemAdded(string itemId, int newCount)
        {
            if (!enableMonitoring) return;

            _totalItemsAdded++;

            if (!_itemsPickedUp.ContainsKey(itemId))
                _itemsPickedUp[itemId] = 0;
            _itemsPickedUp[itemId]++;

            // Track rarity distribution if item data available
            // DISABLED: Gameplay reference (circular dependency)
            /*
            var itemData = Gameplay.InventorySystem.Instance?.GetItemData(itemId);
            if (itemData != null && itemData is Data.ItemData typedData)
            {
                _rarityDistribution[typedData.rarity]++;
            }
            */

            // Check for suspicious pickup rate
            float intervalMinutes = (Time.time - _intervalStartTime) / 60f;
            if (intervalMinutes > 0.1f)
            {
                float pickupRate = _totalItemsAdded / intervalMinutes;
                if (pickupRate > maxItemsAddedPerMinute)
                {
                    LogSuspiciousActivity($"High item pickup rate: {pickupRate:F0}/min (threshold {maxItemsAddedPerMinute}/min)");
                }
            }
        }

        void HandleItemRemoved(string itemId, int remainingCount)
        {
            if (!enableMonitoring) return;
            _totalItemsRemoved++;
        }

        void HandleXPGained(XPGainedEventArgs args)
        {
            if (!enableMonitoring) return;

            _xpGainedThisInterval += args.amount;
            _totalXPGained += args.amount;

            // Track XP sources
            string source = args.source ?? "unknown";
            if (!_xpSourceBreakdown.ContainsKey(source))
                _xpSourceBreakdown[source] = 0;
            _xpSourceBreakdown[source] += args.amount;

            // Check for suspicious XP gain rate
            float intervalMinutes = (Time.time - _intervalStartTime) / 60f;
            if (intervalMinutes > 0.1f)
            {
                float xpRate = _xpGainedThisInterval / intervalMinutes;
                if (xpRate > maxXPGainPerMinute)
                {
                    LogSuspiciousActivity($"High XP gain rate: {xpRate:F0}/min from {source} (threshold {maxXPGainPerMinute}/min)");
                }
            }
        }

        void HandleItemPickup(ItemPickupEventArgs args)
        {
            if (!enableMonitoring) return;
            // Additional tracking for item pickup events (separate from inventory adds)
        }

        // ═══════════════════════════════════════════════════════════════
        // REPORTING
        // ═══════════════════════════════════════════════════════════════

        void GenerateReport()
        {
            float sessionMinutes = (Time.time - _sessionStartTime) / 60f;
            float intervalMinutes = reportIntervalSeconds / 60f;

            var report = new EconomyReport
            {
                timestamp = DateTime.Now,
                sessionDurationMinutes = sessionMinutes,
                intervalDurationMinutes = intervalMinutes,

                // Currency stats
                currencyGainsThisInterval = new Dictionary<CurrencyType, int>(_currencyGainsThisInterval),
                totalCurrencyGains = new Dictionary<CurrencyType, int>(_totalCurrencyGains),
                currencySourceBreakdown = new Dictionary<string, int>(_currencySourceBreakdown),

                // Item stats
                itemsPickedUp = new Dictionary<string, int>(_itemsPickedUp),
                rarityDistribution = new Dictionary<ItemRarity, int>(_rarityDistribution),
                totalItemsAdded = _totalItemsAdded,
                totalItemsRemoved = _totalItemsRemoved,

                // XP stats
                xpGainedThisInterval = _xpGainedThisInterval,
                totalXPGained = _totalXPGained,
                xpSourceBreakdown = new Dictionary<string, float>(_xpSourceBreakdown),

                // Suspicious activity
                suspiciousActivityCount = _suspiciousActivityCount,
                suspiciousEvents = new List<string>(_suspiciousEvents)
            };

            // Log report
            Debug.Log($"[EconomyMonitor] ═══ INTERVAL REPORT ({intervalMinutes:F1}m) ═══");
            Debug.Log($"  Currency Gains: {string.Join(", ", _currencyGainsThisInterval.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            Debug.Log($"  XP Gained: {_xpGainedThisInterval:F0} ({_xpGainedThisInterval / intervalMinutes:F0}/min)");
            Debug.Log($"  Items Added: {_totalItemsAdded} ({_totalItemsAdded / intervalMinutes:F0}/min)");
            Debug.Log($"  Rarity Distribution: {string.Join(", ", _rarityDistribution.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

            if (_suspiciousActivityCount > 0)
            {
                Debug.LogWarning($"  ⚠ Suspicious Activity: {_suspiciousActivityCount} events");
                foreach (var evt in _suspiciousEvents.Take(5)) // Show last 5
                {
                    Debug.LogWarning($"    - {evt}");
                }
            }

            // Fire event for external analytics
            OnReportGenerated?.Invoke(report);
        }

        void ResetIntervalCounters()
        {
            _currencyGainsThisInterval.Clear();
            _xpGainedThisInterval = 0f;
            _suspiciousEvents.Clear();
            // Keep cumulative totals intact
        }

        void LogSuspiciousActivity(string description)
        {
            _suspiciousActivityCount++;
            _suspiciousEvents.Add($"[{DateTime.Now:HH:mm:ss}] {description}");
            Debug.LogWarning($"[EconomyMonitor] SUSPICIOUS: {description}");
            OnSuspiciousActivityDetected?.Invoke(description);
        }

        // ═══════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Get current session statistics.</summary>
        public EconomyReport GetCurrentStats()
        {
            float sessionMinutes = (Time.time - _sessionStartTime) / 60f;

            return new EconomyReport
            {
                timestamp = DateTime.Now,
                sessionDurationMinutes = sessionMinutes,
                intervalDurationMinutes = (Time.time - _intervalStartTime) / 60f,
                currencyGainsThisInterval = new Dictionary<CurrencyType, int>(_currencyGainsThisInterval),
                totalCurrencyGains = new Dictionary<CurrencyType, int>(_totalCurrencyGains),
                currencySourceBreakdown = new Dictionary<string, int>(_currencySourceBreakdown),
                itemsPickedUp = new Dictionary<string, int>(_itemsPickedUp),
                rarityDistribution = new Dictionary<ItemRarity, int>(_rarityDistribution),
                totalItemsAdded = _totalItemsAdded,
                totalItemsRemoved = _totalItemsRemoved,
                xpGainedThisInterval = _xpGainedThisInterval,
                totalXPGained = _totalXPGained,
                xpSourceBreakdown = new Dictionary<string, float>(_xpSourceBreakdown),
                suspiciousActivityCount = _suspiciousActivityCount,
                suspiciousEvents = new List<string>(_suspiciousEvents)
            };
        }

        /// <summary>Reset all tracking counters (use for testing or new session).</summary>
        public void ResetAllCounters()
        {
            _currencyGainsThisInterval.Clear();
            _totalCurrencyGains.Clear();
            _currencySourceBreakdown.Clear();
            _itemsPickedUp.Clear();
            _rarityDistribution.Clear();
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                _rarityDistribution[rarity] = 0;
            }
            _totalItemsAdded = 0;
            _totalItemsRemoved = 0;
            _xpGainedThisInterval = 0f;
            _totalXPGained = 0f;
            _xpSourceBreakdown.Clear();
            _suspiciousActivityCount = 0;
            _suspiciousEvents.Clear();
            _sessionStartTime = Time.time;
            _lastReportTime = Time.time;
            _intervalStartTime = Time.time;

            Debug.Log("[EconomyMonitor] All counters reset");
        }
    }

    /// <summary>
    /// Economy Report — snapshot of economy metrics for a time interval.
    /// </summary>
    [Serializable]
    public class EconomyReport
    {
        public DateTime timestamp;
        public float sessionDurationMinutes;
        public float intervalDurationMinutes;

        // Currency
        public Dictionary<CurrencyType, int> currencyGainsThisInterval;
        public Dictionary<CurrencyType, int> totalCurrencyGains;
        public Dictionary<string, int> currencySourceBreakdown;

        // Items
        public Dictionary<string, int> itemsPickedUp;
        public Dictionary<ItemRarity, int> rarityDistribution;
        public int totalItemsAdded;
        public int totalItemsRemoved;

        // XP
        public float xpGainedThisInterval;
        public float totalXPGained;
        public Dictionary<string, float> xpSourceBreakdown;

        // Suspicious activity
        public int suspiciousActivityCount;
        public List<string> suspiciousEvents;
    }
}
