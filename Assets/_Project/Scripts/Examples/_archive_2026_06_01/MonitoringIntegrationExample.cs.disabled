using UnityEngine;
using Tartaria.Monitoring;
using Tartaria.Core;
using System.Collections.Generic;

namespace Tartaria.Examples
{
    /// <summary>
    /// MonitoringIntegrationExample — sample integration code for MetricsCollector.
    /// 
    /// Shows how to hook monitoring systems into existing game code.
    /// Copy these patterns to QuestManager, PlayerHealth, SaveManager, etc.
    /// 
    /// AGENT 9: Monitoring & Alert System Builder
    /// </summary>
    public class MonitoringIntegrationExample : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 1: Quest System Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into QuestManager.OnQuestStarted event.
        /// </summary>
        void OnQuestStarted(Quest quest)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "quest_started",
                context: quest.Id,
                extraData: new Dictionary<string, object>
                {
                    { "questName", quest.Name },
                    { "moon", quest.Moon },
                    { "difficulty", quest.Difficulty }
                }
            );
        }

        /// <summary>
        /// Hook into QuestManager.OnQuestCompleted event.
        /// </summary>
        void OnQuestCompleted(Quest quest, float completionTimeSeconds)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "quest_completed",
                context: quest.Id,
                extraData: new Dictionary<string, object>
                {
                    { "questName", quest.Name },
                    { "moon", quest.Moon },
                    { "completionTimeSeconds", completionTimeSeconds },
                    { "rewardRS", quest.RewardRS },
                    { "rewardXP", quest.RewardXP }
                }
            );
        }

        /// <summary>
        /// Hook into QuestManager.OnQuestAbandoned event.
        /// </summary>
        void OnQuestAbandoned(Quest quest)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "quest_abandoned",
                context: quest.Id,
                extraData: new Dictionary<string, object>
                {
                    { "questName", quest.Name },
                    { "moon", quest.Moon },
                    { "progress", quest.Progress }
                }
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 2: Player Health Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into PlayerHealth.OnPlayerDeath event.
        /// </summary>
        void OnPlayerDeath(string damageSource, Vector3 deathPosition)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "player_death",
                context: damageSource,
                extraData: new Dictionary<string, object>
                {
                    { "position", deathPosition.ToString() },
                    { "level", PlayerStats.Level },
                    { "currentHP", PlayerHealth.CurrentHP },
                    { "maxHP", PlayerHealth.MaxHP }
                }
            );
        }

        /// <summary>
        /// Hook into PlayerHealth.OnPlayerRespawn event.
        /// </summary>
        void OnPlayerRespawn(Vector3 respawnPosition)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "player_respawn",
                context: respawnPosition.ToString()
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 3: Save System Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into SaveManager.OnSaveStarted event.
        /// </summary>
        void OnSaveStarted(int saveSlot)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "save_started",
                context: $"slot_{saveSlot}"
            );
        }

        /// <summary>
        /// Hook into SaveManager.OnSaveCompleted event.
        /// </summary>
        void OnSaveCompleted(int saveSlot, bool success, float durationSeconds)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: success ? "save_success" : "save_failed",
                context: $"slot_{saveSlot}",
                extraData: new Dictionary<string, object>
                {
                    { "durationSeconds", durationSeconds },
                    { "playtimeSeconds", GameStateManager.Instance.PlaytimeSeconds },
                    { "saveType", "auto" } // or "manual"
                }
            );
        }

        /// <summary>
        /// Hook into SaveManager.OnLoadCompleted event.
        /// </summary>
        void OnLoadCompleted(int saveSlot, bool success)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: success ? "load_success" : "load_failed",
                context: $"slot_{saveSlot}"
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 4: Economy System Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into EconomySystem.OnCurrencyChanged event.
        /// </summary>
        void OnCurrencyChanged(CurrencyType type, int amount, string source)
        {
            // Only log currency gains (not losses to avoid spam)
            if (amount > 0)
            {
                MetricsCollector.Instance?.RecordPlayerAction(
                    action: "currency_gained",
                    context: source,
                    extraData: new Dictionary<string, object>
                    {
                        { "type", type.ToString() },
                        { "amount", amount },
                        { "newTotal", EconomySystem.Instance.GetCurrency(type) }
                    }
                );
            }
        }

        /// <summary>
        /// Hook into InventorySystem.OnItemAdded event.
        /// </summary>
        void OnItemAdded(ItemData item, int quantity)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "item_looted",
                context: item.Id,
                extraData: new Dictionary<string, object>
                {
                    { "itemName", item.Name },
                    { "rarity", item.Rarity.ToString() },
                    { "quantity", quantity },
                    { "source", "loot" } // or "craft", "purchase", etc.
                }
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 5: Scene Transition Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into SceneManager.sceneLoaded event.
        /// </summary>
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "scene_loaded",
                context: scene.name,
                extraData: new Dictionary<string, object>
                {
                    { "buildIndex", scene.buildIndex },
                    { "loadMode", mode.ToString() }
                }
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 6: Combat System Integration
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hook into EnemyAI.OnEnemyKilled event.
        /// </summary>
        void OnEnemyKilled(EnemyAI enemy, string damageSource)
        {
            MetricsCollector.Instance?.RecordPlayerAction(
                action: "enemy_killed",
                context: enemy.EnemyType,
                extraData: new Dictionary<string, object>
                {
                    { "damageSource", damageSource }, // weapon/spell used
                    { "level", enemy.Level },
                    { "xpRewarded", enemy.XPReward }
                }
            );
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 7: Manual Health Check (Debug/Testing)
        // ──────────────────────────────────────────────────────────────────────────

        void Update()
        {
            // Press F9 to force a health check (for testing)
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("[MonitoringIntegration] Forcing health check...");
                HealthCheckSystem.Instance?.ForceHealthCheck();
            }

            // Press F10 to export metrics to file
            if (Input.GetKeyDown(KeyCode.F10))
            {
                string logFile = MetricsCollector.Instance?.ExportMetrics();
                Debug.Log($"[MonitoringIntegration] Metrics exported to: {logFile}");
            }

            // Press F11 to print session stats
            if (Input.GetKeyDown(KeyCode.F11))
            {
                var stats = MetricsCollector.Instance?.GetSessionStats();
                if (stats != null)
                {
                    Debug.Log($"[MonitoringIntegration] Session Stats:");
                    Debug.Log($"  Session ID: {stats["sessionId"]}");
                    Debug.Log($"  Playtime: {stats["playtimeSeconds"]}s");
                    Debug.Log($"  Alerts: {stats["alertsTriggered"]}");
                    Debug.Log($"  Errors: {stats["errorCount"]}");
                    Debug.Log($"  Avg FPS: {stats["averageFPS"]:F1}");
                    Debug.Log($"  Peak Memory: {stats["peakMemoryMB"]:F1}MB");
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 8: Custom Alert Testing
        // ──────────────────────────────────────────────────────────────────────────

        [ContextMenu("Simulate Low FPS Alert")]
        void SimulateLowFPS()
        {
            // Simulate low FPS by pausing in editor
            Debug.LogWarning("[MonitoringIntegration] Simulating low FPS (pause for 10s)...");
            System.Threading.Thread.Sleep(10000); // Don't do this in production!
        }

        [ContextMenu("Simulate Memory Alert")]
        void SimulateMemoryPressure()
        {
            // Allocate large array to trigger memory alert
            Debug.LogWarning("[MonitoringIntegration] Simulating memory pressure...");
            byte[] largeArray = new byte[600 * 1024 * 1024]; // 600MB
            System.Array.Fill<byte>(largeArray, 0xFF); // Force allocation
            Debug.Log($"[MonitoringIntegration] Allocated {largeArray.Length / 1048576}MB");
        }

        [ContextMenu("Simulate Error Alert")]
        void SimulateErrorSpike()
        {
            // Log 50 errors to trigger error rate alert
            Debug.LogWarning("[MonitoringIntegration] Simulating error spike...");
            for (int i = 0; i < 50; i++)
            {
                Debug.LogError($"[MonitoringIntegration] Test error #{i}");
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 9: Privacy-Compliant Player ID
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Example: How to hash player names for privacy compliance.
        /// NEVER log real player names — use this pattern instead.
        /// </summary>
        string GetAnonymousPlayerId(string playerName)
        {
            // Hash player name to create anonymous ID (GDPR-compliant)
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(playerName));
                return System.BitConverter.ToString(hash).Replace("-", "").Substring(0, 12);
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // EXAMPLE 10: Cloud Integration (Phase 2)
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Example: Upload metrics to cloud endpoint (Phase 2).
        /// This would replace local logging with REST API calls.
        /// </summary>
        void UploadMetricsToCloud(string jsonData)
        {
            // TODO: Implement in Phase 2
            // Use UnityWebRequest to POST JSON to cloud endpoint
            // Example: POST https://api.tartaria.com/metrics
            /*
            StartCoroutine(PostMetricsCoroutine(jsonData));
            */
        }

        /*
        IEnumerator PostMetricsCoroutine(string jsonData)
        {
            using (UnityEngine.Networking.UnityWebRequest www = 
                   UnityEngine.Networking.UnityWebRequest.Post("https://api.tartaria.com/metrics", jsonData, "application/json"))
            {
                yield return www.SendWebRequest();
                
                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[MonitoringIntegration] Failed to upload metrics: {www.error}");
                }
                else
                {
                    Debug.Log("[MonitoringIntegration] Metrics uploaded successfully");
                }
            }
        }
        */
    }
}
