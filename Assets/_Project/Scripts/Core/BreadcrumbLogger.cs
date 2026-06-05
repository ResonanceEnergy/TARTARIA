using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 7: Enhanced breadcrumb logger for player actions.
    /// Structured player-action tracking written to rolling in-memory window + daily log file:
    /// - Quest milestones (start, objective complete, fail, success)
    /// - Combat events (hit, miss, death, kill)
    /// - Inventory changes (pickup, use, drop, craft)
    /// - UI interactions (menu open, settings change)
    /// - World events (level up, Moon phase change, discovery)
    /// 
    /// Breadcrumbs are captured in crash/hitch logs automatically.
    /// Also maintains a rolling window log (last 50 actions) for support requests.
    /// </summary>
    public static class BreadcrumbLogger
    {
        const int MAX_BREADCRUMBS = 50;
        static Queue<BreadcrumbEntry> _breadcrumbs = new Queue<BreadcrumbEntry>(MAX_BREADCRUMBS);
        static string _logFilePath;
        static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (_initialized) return;
            
            string logDir = Path.Combine(Application.dataPath, "..", "Logs", "Breadcrumbs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            
            _logFilePath = Path.Combine(logDir, $"breadcrumbs-{DateTime.Now:yyyy-MM-dd}.log");
            _initialized = true;
            
            Log(BreadcrumbCategory.System, "BreadcrumbLogger initialized");
            Debug.Log($"[BreadcrumbLogger] Initialized. Log: {_logFilePath}");
        }

        /// <summary>
        /// Log a player action breadcrumb to the rolling window and append to daily log file.
        /// </summary>
        public static void Log(BreadcrumbCategory category, string message, string context = null)
        {
            if (!_initialized) Initialize();
            
            var entry = new BreadcrumbEntry
            {
                timestamp = DateTime.Now,
                gameTime = Time.realtimeSinceStartup,
                category = category,
                message = message,
                context = context
            };
            
            // Add to rolling window
            _breadcrumbs.Enqueue(entry);
            if (_breadcrumbs.Count > MAX_BREADCRUMBS)
                _breadcrumbs.Dequeue();
            
            // Append to daily log file (for manual review)
            AppendToLogFile(entry);
        }

        static void AppendToLogFile(BreadcrumbEntry entry)
        {
            try
            {
                using (var writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine($"[{entry.timestamp:HH:mm:ss.fff}] [{entry.gameTime:F1}s] [{entry.category}] {entry.message}");
                    if (!string.IsNullOrEmpty(entry.context))
                        writer.WriteLine($"  Context: {entry.context}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BreadcrumbLogger] Failed to append to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the last N breadcrumbs (for support requests).
        /// </summary>
        public static List<BreadcrumbEntry> GetRecentBreadcrumbs(int count = 50)
        {
            var result = new List<BreadcrumbEntry>();
            foreach (var crumb in _breadcrumbs)
            {
                result.Add(crumb);
                if (result.Count >= count)
                    break;
            }
            return result;
        }

        /// <summary>
        /// Export breadcrumbs to a formatted string (for bug reports).
        /// </summary>
        public static string ExportBreadcrumbs(int count = 50)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RECENT PLAYER ACTIONS (Last 50) ===");
            
            var crumbs = GetRecentBreadcrumbs(count);
            foreach (var crumb in crumbs)
            {
                sb.AppendLine($"[{crumb.gameTime:F1}s] [{crumb.category}] {crumb.message}");
                if (!string.IsNullOrEmpty(crumb.context))
                    sb.AppendLine($"  └─ {crumb.context}");
            }
            
            sb.AppendLine("=== END BREADCRUMBS ===");
            return sb.ToString();
        }

        // Convenience methods for common actions

        public static void LogQuestStart(string questID, string questName)
        {
            Log(BreadcrumbCategory.Quest, $"Started: {questName}", $"ID={questID}");
        }

        public static void LogQuestComplete(string questID, string questName)
        {
            Log(BreadcrumbCategory.Quest, $"Completed: {questName}", $"ID={questID}");
        }

        public static void LogQuestFail(string questID, string questName, string reason)
        {
            Log(BreadcrumbCategory.Quest, $"Failed: {questName}", $"ID={questID}, Reason={reason}");
        }

        public static void LogCombatHit(string enemyName, int damage, bool isCritical)
        {
            string msg = isCritical ? $"CRIT HIT {enemyName} for {damage}" : $"Hit {enemyName} for {damage}";
            Log(BreadcrumbCategory.Combat, msg);
        }

        public static void LogCombatMiss(string enemyName)
        {
            Log(BreadcrumbCategory.Combat, $"Missed {enemyName}");
        }

        public static void LogPlayerDeath(string cause, string location)
        {
            Log(BreadcrumbCategory.Combat, $"Player died: {cause}", $"Location={location}");
        }

        public static void LogEnemyKill(string enemyName, int xpGain)
        {
            Log(BreadcrumbCategory.Combat, $"Killed {enemyName} (+{xpGain} XP)");
        }

        public static void LogItemPickup(string itemName, int count)
        {
            Log(BreadcrumbCategory.Inventory, $"Picked up: {itemName} x{count}");
        }

        public static void LogItemUse(string itemName, string effect)
        {
            Log(BreadcrumbCategory.Inventory, $"Used: {itemName}", $"Effect={effect}");
        }

        public static void LogItemDrop(string itemName, int count)
        {
            Log(BreadcrumbCategory.Inventory, $"Dropped: {itemName} x{count}");
        }

        public static void LogItemCraft(string itemName, string recipe)
        {
            Log(BreadcrumbCategory.Inventory, $"Crafted: {itemName}", $"Recipe={recipe}");
        }

        public static void LogLevelUp(int newLevel, int statPointsGained)
        {
            Log(BreadcrumbCategory.Progression, $"Level up! Now level {newLevel}", $"StatPoints=+{statPointsGained}");
        }

        public static void LogStatIncrease(string statName, int oldValue, int newValue)
        {
            Log(BreadcrumbCategory.Progression, $"Increased {statName}: {oldValue} → {newValue}");
        }

        public static void LogMoonChange(string oldMoon, string newMoon)
        {
            Log(BreadcrumbCategory.World, $"Moon changed: {oldMoon} → {newMoon}");
        }

        public static void LogDiscovery(string locationName)
        {
            Log(BreadcrumbCategory.World, $"Discovered: {locationName}");
        }

        public static void LogMenuOpen(string menuName)
        {
            Log(BreadcrumbCategory.UI, $"Opened menu: {menuName}");
        }

        public static void LogSettingsChange(string settingName, string oldValue, string newValue)
        {
            Log(BreadcrumbCategory.UI, $"Changed setting: {settingName}", $"{oldValue} → {newValue}");
        }

        public static void LogSaveGame(string saveName, bool success)
        {
            string msg = success ? $"Saved game: {saveName}" : $"FAILED to save: {saveName}";
            Log(BreadcrumbCategory.System, msg);
        }

        public static void LogLoadGame(string saveName, bool success)
        {
            string msg = success ? $"Loaded game: {saveName}" : $"FAILED to load: {saveName}";
            Log(BreadcrumbCategory.System, msg);
        }

        public static void LogSceneTransition(string fromScene, string toScene, float loadTime)
        {
            Log(BreadcrumbCategory.System, $"Scene transition: {fromScene} → {toScene}", $"LoadTime={loadTime:F2}s");
        }
    }

    // Breadcrumb category taxonomy
    public enum BreadcrumbCategory
    {
        Quest,          // Quest start/complete/fail
        Combat,         // Hits, deaths, kills
        Inventory,      // Item pickup/use/drop/craft
        Progression,    // Level up, stat changes
        World,          // Moon changes, discoveries
        UI,             // Menu navigation, settings
        System          // Save/load, scene transitions
    }

    // Breadcrumb entry data structure
    [Serializable]
    public struct BreadcrumbEntry
    {
        public DateTime timestamp;      // Real-world time
        public float gameTime;          // Time.realtimeSinceStartup
        public BreadcrumbCategory category;
        public string message;
        public string context;          // Optional additional details
    }
}
