using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Examples
{
    /// <summary>
    /// Example demonstrating GameEvents pub/sub pattern for decoupled communication.
    /// This pattern eliminates direct Instance?.Method() calls that create tight coupling.
    /// 
    /// BENEFITS:
    ///   - No assembly dependencies (Core assembly only)
    ///   - Type-safe event payloads (BuildingRestoredEventArgs, etc.)
    ///   - Thread-safe invocation with exception handling
    ///   - Memory-leak prevention via proper unsubscribe in OnDestroy
    /// </summary>
    public class GameEventsUsageExample : MonoBehaviour
    {
        void Start()
        {
            // ─── SUBSCRIBE TO EVENTS ───────────────────────────────────
            // Always use OnDestroy for cleanup to prevent memory leaks!
            
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnLevelUp += HandleLevelUp;
            GameEvents.OnItemPickup += HandleItemPickup;
            GameEvents.OnQuestStatusChanged += HandleQuestStatusChanged;
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
            
            Debug.Log("[GameEventsExample] Subscribed to 6 event types");
        }

        void OnDestroy()
        {
            // ─── CRITICAL: UNSUBSCRIBE IN OnDestroy ───────────────────
            // Failure to unsubscribe causes memory leaks when this object is destroyed.
            
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnLevelUp -= HandleLevelUp;
            GameEvents.OnItemPickup -= HandleItemPickup;
            GameEvents.OnQuestStatusChanged -= HandleQuestStatusChanged;
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
            
            Debug.Log("[GameEventsExample] Unsubscribed from all events");
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENT HANDLERS (Subscribers)
        // ═══════════════════════════════════════════════════════════════════

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            Debug.Log($"[GameEventsExample] Building '{args.buildingId}' restored!");
            Debug.Log($"  RS Reward: {args.rsReward}");
            Debug.Log($"  Tuning Accuracy: {args.tuningAccuracy:P0}");
            Debug.Log($"  Position: {args.position}");
            
            // Example: Update achievement system
            if (args.tuningAccuracy >= 0.95f)
            {
                Debug.Log("  → Perfect tuning achievement unlocked!");
            }
        }

        void HandleEnemyKilled(EnemyKilledEventArgs args)
        {
            Debug.Log($"[GameEventsExample] Enemy killed: {args.enemyType}");
            Debug.Log($"  XP Reward: {args.xpReward}");
            Debug.Log($"  Loot: {args.lootCount}x {args.lootItemId}");
            Debug.Log($"  Killed by: {(args.killedBy != null ? args.killedBy.name : "unknown")}");
            
            // Example: Update kill counter UI
            // KillCounterUI.Instance?.IncrementKills(args.enemyType);
        }

        void HandleLevelUp(LevelUpEventArgs args)
        {
            Debug.Log($"[GameEventsExample] LEVEL UP! {args.oldLevel} → {args.newLevel}");
            Debug.Log($"  Max Health Bonus: +{args.maxHealthBonus}");
            Debug.Log($"  Damage Bonus: +{args.damageBonus}");
            Debug.Log($"  Movement Speed Bonus: +{args.movementSpeedBonus:P0}");
            
            // Example: Show level-up UI notification
            // LevelUpNotificationUI.Instance?.Show(args.newLevel);
        }

        void HandleItemPickup(ItemPickupEventArgs args)
        {
            Debug.Log($"[GameEventsExample] Item picked up: {args.itemId}");
            Debug.Log($"  Count: {args.count}");
            Debug.Log($"  Total in inventory: {args.totalCount}");
            
            // Example: Update quest objectives
            // QuestManager.Instance?.ProgressByType(QuestObjectiveType.CollectItem, args.itemId, args.count);
        }

        void HandleQuestStatusChanged(QuestStatusChangedEventArgs args)
        {
            Debug.Log($"[GameEventsExample] Quest status changed: {args.questId}");
            Debug.Log($"  Old Status: {args.oldStatus}");
            Debug.Log($"  New Status: {args.newStatus}");
            
            // Example: Show quest completion UI
            if (args.newStatus == QuestStatus.Completed)
            {
                // QuestCompletionUI.Instance?.Show(args.questId);
            }
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            Debug.Log($"[GameEventsExample] Moon {args.moonIndex} completed!");
            Debug.Log($"  Moon Name: {args.moonName}");
            Debug.Log($"  RS Reward: {args.rsReward}");
            Debug.Log($"  Completion Time: {args.completionTime:F1}s");
            
            // Example: Unlock next Moon portal
            // MoonProgressionSystem.Instance?.UnlockMoon(args.moonIndex + 1);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RAISING EVENTS (Publishers)
        // ═══════════════════════════════════════════════════════════════════
        // Typically done inside core systems (InteractableBuilding, PlayerProgression, etc.)
        // but shown here for reference.

        void ExampleRaiseEvents()
        {
            // Example: Raise building restored event
            GameEvents.RaiseBuildingRestored(new BuildingRestoredEventArgs
            {
                buildingId = "dome",
                rsReward = 150,
                position = new Vector3(30f, 0f, 20f),
                tuningAccuracy = 0.98f
            });

            // Example: Raise enemy killed event
            GameEvents.RaiseEnemyKilled(new EnemyKilledEventArgs
            {
                enemyType = "mud_golem",
                xpReward = 25,
                lootItemId = "aether_shard",
                lootCount = 3,
                position = transform.position,
                killedBy = gameObject
            });

            // Example: Raise level up event
            GameEvents.RaiseLevelUp(new LevelUpEventArgs
            {
                newLevel = 5,
                oldLevel = 4,
                maxHealthBonus = 25f,
                damageBonus = 5f,
                movementSpeedBonus = 0.1f
            });

            // Example: Raise item pickup event
            GameEvents.RaiseItemPickup(new ItemPickupEventArgs
            {
                itemId = "resonance_crystal",
                count = 1,
                totalCount = 5
            });

            // Example: Raise quest status changed event
            GameEvents.RaiseQuestStatusChanged(new QuestStatusChangedEventArgs
            {
                questId = "echoes_of_buried_city",
                newStatus = QuestStatus.Completed,
                oldStatus = QuestStatus.Active
            });

            // Example: Raise Moon completed event
            GameEvents.RaiseMoonCompleted(new MoonCompletedEventArgs
            {
                moonIndex = 1,
                moonName = "Moon of Echohaven",
                rsReward = 500,
                completionTime = 3600f
            });
        }
    }
}
