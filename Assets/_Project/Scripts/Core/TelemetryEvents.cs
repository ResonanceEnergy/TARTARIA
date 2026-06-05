using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Telemetry event definitions and taxonomy for TARTARIA.
    /// 
    /// AGENT 2: TELEMETRY EVENT TAXONOMY
    /// 
    /// Categories:
    /// - SESSION: Session start/end, playtime tracking
    /// - PLAYER: Level-up, death, stat allocation
    /// - PROGRESSION: Quest completion, moon completion, RS milestones
    /// - ECONOMY: Item acquisition, gold spent, crafting
    /// - PERFORMANCE: FPS drops, hitches, memory spikes
    /// - ENGAGEMENT: Heatmaps (death locations, time spent per zone)
    /// 
    /// Privacy: NO PII (personally identifiable information)
    /// - All IDs are session-scoped (deviceID hash, not account ID)
    /// - No IP addresses, no email, no usernames
    /// - Optional: player can opt-out via TelemetryService.SetOptOut(true)
    /// </summary>
    public static class TelemetryEvents
    {
        // ═══════════════════════════════════════════════════════════════════
        // SESSION EVENTS
        // ═══════════════════════════════════════════════════════════════════

        public const string SESSION_START = "session_start";
        public const string SESSION_END = "session_end";
        public const string SESSION_CRASH = "session_crash";

        /// <summary>
        /// Track session start.
        /// Properties: platform, unity_version, build_version, device_type
        /// </summary>
        public static Dictionary<string, object> SessionStart()
        {
            return new Dictionary<string, object>
            {
                ["platform"] = Application.platform.ToString(),
                ["unity_version"] = Application.unityVersion,
                ["build_version"] = Application.version,
                ["device_type"] = SystemInfo.deviceType.ToString(),
                ["os"] = SystemInfo.operatingSystem,
                ["gpu"] = SystemInfo.graphicsDeviceName,
                ["cpu_cores"] = SystemInfo.processorCount,
                ["ram_mb"] = SystemInfo.systemMemorySize,
                ["screen_resolution"] = $"{Screen.width}x{Screen.height}",
                ["quality_level"] = QualitySettings.GetQualityLevel()
            };
        }

        /// <summary>
        /// Track session end.
        /// Properties: duration_seconds, rs_score, level, playtime_total
        /// </summary>
        public static Dictionary<string, object> SessionEnd(float durationSeconds, float rsScore, int level, float playtimeTotal)
        {
            return new Dictionary<string, object>
            {
                ["duration_seconds"] = durationSeconds,
                ["rs_score"] = rsScore,
                ["level"] = level,
                ["playtime_total_seconds"] = playtimeTotal
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // PLAYER EVENTS
        // ═══════════════════════════════════════════════════════════════════

        public const string PLAYER_LEVEL_UP = "player_level_up";
        public const string PLAYER_DEATH = "player_death";
        public const string PLAYER_STAT_ALLOCATED = "player_stat_allocated";
        public const string PLAYER_SKILL_UNLOCKED = "player_skill_unlocked";

        /// <summary>
        /// Track player level-up.
        /// Properties: new_level, xp, session_time, source
        /// </summary>
        public static Dictionary<string, object> PlayerLevelUp(int newLevel, int xp, float sessionTime, string source)
        {
            return new Dictionary<string, object>
            {
                ["new_level"] = newLevel,
                ["xp"] = xp,
                ["session_time_seconds"] = sessionTime,
                ["source"] = source
            };
        }

        /// <summary>
        /// Track player death (critical for heatmap analysis).
        /// Properties: location, zone, enemy_type, level, health_before, session_time
        /// </summary>
        public static Dictionary<string, object> PlayerDeath(Vector3 location, string zone, string enemyType, int level, float healthBefore, float sessionTime)
        {
            return new Dictionary<string, object>
            {
                ["location_x"] = Mathf.RoundToInt(location.x),
                ["location_y"] = Mathf.RoundToInt(location.y),
                ["location_z"] = Mathf.RoundToInt(location.z),
                ["zone"] = zone,
                ["enemy_type"] = enemyType,
                ["player_level"] = level,
                ["health_before"] = healthBefore,
                ["session_time_seconds"] = sessionTime
            };
        }

        /// <summary>
        /// Track stat point allocation.
        /// Properties: stat_name, new_value, level
        /// </summary>
        public static Dictionary<string, object> StatAllocated(string statName, int newValue, int level)
        {
            return new Dictionary<string, object>
            {
                ["stat_name"] = statName,
                ["new_value"] = newValue,
                ["player_level"] = level
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // PROGRESSION EVENTS
        // ═══════════════════════════════════════════════════════════════════

        public const string QUEST_STARTED = "quest_started";
        public const string QUEST_COMPLETED = "quest_completed";
        public const string QUEST_FAILED = "quest_failed";
        public const string MOON_COMPLETED = "moon_completed";
        public const string RS_MILESTONE = "rs_milestone";
        public const string BUILDING_RESTORED = "building_restored";

        /// <summary>
        /// Track quest completion.
        /// Properties: quest_id, duration_seconds, level, moon
        /// </summary>
        public static Dictionary<string, object> QuestCompleted(string questId, float durationSeconds, int level, int moon)
        {
            return new Dictionary<string, object>
            {
                ["quest_id"] = questId,
                ["duration_seconds"] = durationSeconds,
                ["player_level"] = level,
                ["moon"] = moon
            };
        }

        /// <summary>
        /// Track moon completion (major milestone).
        /// Properties: moon_number, duration_seconds, level, rs_score, deaths
        /// </summary>
        public static Dictionary<string, object> MoonCompleted(int moonNumber, float durationSeconds, int level, float rsScore, int deaths)
        {
            return new Dictionary<string, object>
            {
                ["moon_number"] = moonNumber,
                ["duration_seconds"] = durationSeconds,
                ["player_level"] = level,
                ["rs_score"] = rsScore,
                ["deaths"] = deaths
            };
        }

        /// <summary>
        /// Track RS milestones (every 10 points).
        /// Properties: rs_score, level, session_time
        /// </summary>
        public static Dictionary<string, object> RSMilestone(float rsScore, int level, float sessionTime)
        {
            return new Dictionary<string, object>
            {
                ["rs_score"] = rsScore,
                ["player_level"] = level,
                ["session_time_seconds"] = sessionTime
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // ECONOMY EVENTS
        // ═══════════════════════════════════════════════════════════════════

        public const string ITEM_ACQUIRED = "item_acquired";
        public const string ITEM_SPENT = "item_spent";
        public const string ITEM_CRAFTED = "item_crafted";
        public const string GOLD_EARNED = "gold_earned";
        public const string GOLD_SPENT = "gold_spent";

        /// <summary>
        /// Track item acquisition.
        /// Properties: item_id, count, source, level
        /// </summary>
        public static Dictionary<string, object> ItemAcquired(string itemId, int count, string source, int level)
        {
            return new Dictionary<string, object>
            {
                ["item_id"] = itemId,
                ["count"] = count,
                ["source"] = source,
                ["player_level"] = level
            };
        }

        /// <summary>
        /// Track gold spending.
        /// Properties: amount, category, item_id, level
        /// </summary>
        public static Dictionary<string, object> GoldSpent(int amount, string category, string itemId, int level)
        {
            return new Dictionary<string, object>
            {
                ["amount"] = amount,
                ["category"] = category,
                ["item_id"] = itemId,
                ["player_level"] = level
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // PERFORMANCE EVENTS
        // ═══════════════════════════════════════════════════════════════════

        public const string PERFORMANCE_FRAME_DROP = "performance_frame_drop";
        public const string PERFORMANCE_HITCH = "performance_hitch";
        public const string PERFORMANCE_MEMORY_SPIKE = "performance_memory_spike";
        public const string PERFORMANCE_LOW_FPS_PERIOD = "performance_low_fps_period";

        /// <summary>
        /// Track frame hitch (>100ms frame).
        /// Properties: frame_ms, scene, player_count, memory_mb
        /// </summary>
        public static Dictionary<string, object> PerformanceHitch(float frameMs, string scene, int playerCount, long memoryMB)
        {
            return new Dictionary<string, object>
            {
                ["frame_ms"] = frameMs,
                ["scene"] = scene,
                ["player_count"] = playerCount,
                ["memory_mb"] = memoryMB
            };
        }

        /// <summary>
        /// Track low FPS period (>10 seconds below 30 FPS).
        /// Properties: duration_seconds, avg_fps, scene
        /// </summary>
        public static Dictionary<string, object> LowFPSPeriod(float durationSeconds, float avgFPS, string scene)
        {
            return new Dictionary<string, object>
            {
                ["duration_seconds"] = durationSeconds,
                ["avg_fps"] = avgFPS,
                ["scene"] = scene
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // ENGAGEMENT EVENTS (for heatmaps)
        // ═══════════════════════════════════════════════════════════════════

        public const string ZONE_ENTERED = "zone_entered";
        public const string ZONE_EXITED = "zone_exited";
        public const string STUCK_DETECTED = "stuck_detected";

        /// <summary>
        /// Track zone entry (for time-spent heatmaps).
        /// Properties: zone, entry_time, level
        /// </summary>
        public static Dictionary<string, object> ZoneEntered(string zone, float entryTime, int level)
        {
            return new Dictionary<string, object>
            {
                ["zone"] = zone,
                ["entry_time"] = entryTime,
                ["player_level"] = level
            };
        }

        /// <summary>
        /// Track zone exit (calculate time spent).
        /// Properties: zone, duration_seconds, level
        /// </summary>
        public static Dictionary<string, object> ZoneExited(string zone, float durationSeconds, int level)
        {
            return new Dictionary<string, object>
            {
                ["zone"] = zone,
                ["duration_seconds"] = durationSeconds,
                ["player_level"] = level
            };
        }

        /// <summary>
        /// Track player stuck (same position >30 seconds, not AFK).
        /// Properties: location, zone, duration_seconds, level
        /// </summary>
        public static Dictionary<string, object> StuckDetected(Vector3 location, string zone, float durationSeconds, int level)
        {
            return new Dictionary<string, object>
            {
                ["location_x"] = Mathf.RoundToInt(location.x),
                ["location_y"] = Mathf.RoundToInt(location.y),
                ["location_z"] = Mathf.RoundToInt(location.z),
                ["zone"] = zone,
                ["duration_seconds"] = durationSeconds,
                ["player_level"] = level
            };
        }
    }
}
