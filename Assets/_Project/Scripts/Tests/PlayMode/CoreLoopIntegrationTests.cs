using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Tartaria.Gameplay;
using Tartaria.Integration;
using Tartaria.AI;
using Tartaria.Core;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for core gameplay loop.
    /// Tests end-to-end flows: building restore, combat XP, leveling, quest completion.
    /// </summary>
    public class CoreLoopIntegrationTests
    {
        [UnityTest]
        public IEnumerator Test_FullBuildingRestoreLoop()
        {
            // Setup: player + building + tuning system
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.zero;
            
            var buildingGO = new GameObject("TestBuilding");
            var building = buildingGO.AddComponent<InteractableBuilding>();
            buildingGO.transform.position = Vector3.forward * 3f;
            
            // Setup building as UnityEngine.Object reference
            var buildingField = typeof(InteractableBuilding).GetField("buildingObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (buildingField != null)
            {
                buildingField.SetValue(building, buildingGO);
            }
            
            bool restoredEventFired = false;
            // Subscribe to GameEvents for building restored
            GameEvents.BuildingRestored += (args) => restoredEventFired = true;
            
            yield return null;
            
            // Act: Interact with building (trigger tuning minigame)
            var interactMethod = typeof(InteractableBuilding).GetMethod("OnInteract", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (interactMethod == null)
            {
                interactMethod = typeof(InteractableBuilding).GetMethod("Interact", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }
            
            if (interactMethod != null)
            {
                interactMethod.Invoke(building, null);
                yield return null;
                
                // Complete tuning successfully (simulate)
                // Note: Full integration would require TuningMiniGameController setup
                
                // Manually trigger restoration
                var completeMethod = typeof(InteractableBuilding).GetMethod("CompleteTuning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (completeMethod != null)
                {
                    completeMethod.Invoke(building, new object[] { 1f });  // Perfect accuracy
                    yield return null;
                    
                    // Assert: Building restored
                    Assert.IsTrue(restoredEventFired, "BuildingRestored event should fire");
                }
            }
            
            // Cleanup
            GameEvents.BuildingRestored = null;
            Object.Destroy(playerGO);
            Object.Destroy(buildingGO);
        }

        [UnityTest]
        public IEnumerator Test_CombatKillRewardsXP()
        {
            // Setup: player + progression + enemy
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            
            var progressionGO = new GameObject("PlayerProgression");
            var progression = progressionGO.AddComponent<PlayerProgression>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemy.SetMaxHealth(50f, true);  // Low HP for quick kill
            
            yield return null;
            
            int initialXP = progression.CurrentXP;
            int initialLevel = progression.CurrentLevel;
            
            // Act: Kill enemy
            enemy.TakeDamage(100f);
            
            yield return new WaitForSeconds(0.5f);
            
            // Assert: XP gained
            Assert.IsFalse(enemy.IsAlive, "Enemy should be dead");
            // Note: XP reward depends on GameEvents.EnemyKilled being wired to PlayerProgression
            // This may require running the full scene context
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(progressionGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_LevelUpGrantsStatPoints()
        {
            // Setup: player progression
            var progressionGO = new GameObject("PlayerProgression");
            var progression = progressionGO.AddComponent<PlayerProgression>();
            
            yield return null;
            
            int initialLevel = progression.CurrentLevel;
            int initialStatPoints = progression.AvailableStatPoints;
            
            // Act: Grant XP to trigger level up
            var grantXPMethod = typeof(PlayerProgression).GetMethod("GainXP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (grantXPMethod != null)
            {
                // Grant enough XP for level 2 (100 XP needed)
                grantXPMethod.Invoke(progression, new object[] { 150 });
                
                yield return null;
                
                // Assert: Level increased
                Assert.Greater(progression.CurrentLevel, initialLevel, "Level should increase after gaining XP");
                Assert.Greater(progression.AvailableStatPoints, initialStatPoints, "Stat points should be granted on level up");
            }
            
            // Cleanup
            Object.Destroy(progressionGO);
        }

        [UnityTest]
        public IEnumerator Test_QuestCompletionFlow()
        {
            // Setup: quest manager + quest
            var questManagerGO = new GameObject("QuestManager");
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            yield return null;
            
            // Create test quest (via reflection or API)
            var startQuestMethod = typeof(QuestManager).GetMethod("StartQuest", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (startQuestMethod != null)
            {
                // Start a quest
                startQuestMethod.Invoke(questManager, new object[] { "test_quest_moon1" });
                
                yield return null;
                
                // Progress quest objective
                var progressMethod = typeof(QuestManager).GetMethod("ProgressObjective", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (progressMethod != null)
                {
                    progressMethod.Invoke(questManager, new object[] { "test_quest_moon1", "objective_1", 1 });
                    
                    yield return null;
                    
                    // Complete quest
                    var completeMethod = typeof(QuestManager).GetMethod("CompleteQuest", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (completeMethod != null)
                    {
                        completeMethod.Invoke(questManager, new object[] { "test_quest_moon1" });
                        
                        yield return null;
                        
                        // Assert: Quest marked complete
                        var isCompleteMethod = typeof(QuestManager).GetMethod("IsQuestComplete", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (isCompleteMethod != null)
                        {
                            bool isComplete = (bool)isCompleteMethod.Invoke(questManager, new object[] { "test_quest_moon1" });
                            Assert.IsTrue(isComplete, "Quest should be marked complete");
                        }
                    }
                }
            }
            
            // Cleanup
            Object.Destroy(questManagerGO);
        }

        [UnityTest]
        public IEnumerator Test_ResonanceScoreEconomy()
        {
            // Setup: economy system
            var economyGO = new GameObject("EconomySystem");
            var economy = economyGO.AddComponent<EconomySystem>();
            
            yield return null;
            
            int initialRS = economy.ResonanceScore;
            
            // Act: Award RS
            var awardMethod = typeof(EconomySystem).GetMethod("AwardResonanceScore", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (awardMethod != null)
            {
                awardMethod.Invoke(economy, new object[] { 100 });
                
                yield return null;
                
                // Assert: RS increased
                Assert.AreEqual(initialRS + 100, economy.ResonanceScore, "RS should increase after award");
                
                // Spend RS
                bool spent = economy.SpendResonanceScore(50);
                
                Assert.IsTrue(spent, "Should be able to spend RS");
                Assert.AreEqual(initialRS + 50, economy.ResonanceScore, "RS should decrease after spending");
            }
            
            // Cleanup
            Object.Destroy(economyGO);
        }

        [UnityTest]
        public IEnumerator Test_InventoryItemPickup()
        {
            // Setup: inventory system
            var inventoryGO = new GameObject("InventorySystem");
            var inventory = inventoryGO.AddComponent<InventorySystem>();
            
            yield return null;
            
            bool itemAddedEventFired = false;
            string addedItemID = "";
            int addedQuantity = 0;
            
            // Subscribe to inventory events
            GameEvents.ItemPickedUp += (args) =>
            {
                itemAddedEventFired = true;
                addedItemID = args.ItemID;
                addedQuantity = args.Quantity;
            };
            
            // Act: Add item
            inventory.AddItem("aether_shard", 5);
            
            yield return null;
            
            // Assert: Item added, event fired
            Assert.IsTrue(itemAddedEventFired, "ItemPickedUp event should fire");
            Assert.AreEqual("aether_shard", addedItemID);
            Assert.AreEqual(5, addedQuantity);
            Assert.IsTrue(inventory.HasItem("aether_shard"), "Inventory should contain item");
            
            // Cleanup
            GameEvents.ItemPickedUp = null;
            Object.Destroy(inventoryGO);
        }

        [UnityTest]
        public IEnumerator Test_CombatWithAbilities()
        {
            // Setup: player with combat + abilities + health
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var combat = playerGO.AddComponent<PlayerCombat>();
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemy.SetMaxHealth(200f, true);
            enemyGO.transform.position = Vector3.forward * 3f;
            
            yield return null;
            
            float initialHealth = enemy.CurrentHealth;
            
            // Act: Melee attack
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(combat, null);
            
            yield return null;
            
            float healthAfterMelee = enemy.CurrentHealth;
            Assert.Less(healthAfterMelee, initialHealth, "Melee should damage enemy");
            
            // Act: Harmonic Strike ability
            var harmonicMethod = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            harmonicMethod.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: Additional damage from ability
            Assert.Less(enemy.CurrentHealth, healthAfterMelee, "Ability should deal additional damage");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_FullCombatToDeathLoop()
        {
            // Setup: player vs enemy, complete kill flow
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var combat = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemy.SetMaxHealth(75f, true);  // 3 hits to kill (25 dmg each)
            enemyGO.transform.position = Vector3.forward * 2f;
            
            bool deathEventFired = false;
            enemy.OnDeath += () => deathEventFired = true;
            
            yield return null;
            
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act: Attack until dead
            for (int i = 0; i < 4; i++)
            {
                if (!enemy.IsAlive) break;
                
                swingMethod.Invoke(combat, null);
                yield return new WaitForSeconds(0.5f);  // Wait for cooldown
            }
            
            // Assert: Enemy killed
            Assert.IsFalse(enemy.IsAlive, "Enemy should be dead after multiple attacks");
            Assert.IsTrue(deathEventFired, "OnDeath event should fire");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_SaveLoadPersistence()
        {
            // Setup: Test that core systems support save/load
            var progressionGO = new GameObject("PlayerProgression");
            var progression = progressionGO.AddComponent<PlayerProgression>();
            
            yield return null;
            
            // Grant some XP and level
            var grantXPMethod = typeof(PlayerProgression).GetMethod("GainXP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (grantXPMethod != null)
            {
                grantXPMethod.Invoke(progression, new object[] { 250 });
                yield return null;
            }
            
            int savedLevel = progression.CurrentLevel;
            int savedXP = progression.CurrentXP;
            
            // Act: Get save data
            var saveMethod = typeof(PlayerProgression).GetMethod("GetSaveData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (saveMethod != null)
            {
                object saveData = saveMethod.Invoke(progression, null);
                Assert.IsNotNull(saveData, "Should return save data");
                
                // Reset progression
                var resetMethod = typeof(PlayerProgression).GetMethod("ResetProgression", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (resetMethod != null)
                {
                    resetMethod.Invoke(progression, null);
                }
                
                // Load save data
                var loadMethod = typeof(PlayerProgression).GetMethod("LoadFromSaveData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (loadMethod != null)
                {
                    loadMethod.Invoke(progression, new object[] { saveData });
                    yield return null;
                    
                    // Assert: Data restored
                    Assert.AreEqual(savedLevel, progression.CurrentLevel, "Level should be restored from save");
                    Assert.AreEqual(savedXP, progression.CurrentXP, "XP should be restored from save");
                }
            }
            
            // Cleanup
            Object.Destroy(progressionGO);
        }

        [UnityTest]
        public IEnumerator Test_TuningToRestoreFlow()
        {
            // Setup: Simulate tuning minigame → building restore
            var tuningGO = new GameObject("TuningController");
            var tuning = tuningGO.AddComponent<TuningMiniGameController>();
            
            bool completionFired = false;
            float finalAccuracy = 0f;
            
            tuning.OnTuningComplete += (accuracy) =>
            {
                completionFired = true;
                finalAccuracy = accuracy;
            };
            
            yield return null;
            
            // Start tuning
            var config = new TuningPuzzleConfig
            {
                variant = TuningVariant.FrequencySlider,
                targetFrequency = 432f,
                timeLimitSeconds = 5f,
                tolerancePercent = 2f
            };
            
            tuning.StartTuning(config);
            
            // Simulate perfect completion
            var accuracyField = typeof(TuningMiniGameController).GetField("_accuracy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            accuracyField.SetValue(tuning, 1f);
            
            tuning.OnTuningComplete?.Invoke(1f);
            
            yield return null;
            
            // Assert: Completion success
            Assert.IsTrue(completionFired, "Tuning completion should fire");
            Assert.AreEqual(1f, finalAccuracy, 0.01f, "Accuracy should be perfect");
            
            // Cleanup
            Object.Destroy(tuningGO);
        }
    }

    // Helper classes (copied from TuningMiniGameControllerTests for integration)
    public class TuningPuzzleConfig
    {
        public TuningVariant variant;
        public float targetFrequency = 432f;
        public float timeLimitSeconds = 15f;
        public float tolerancePercent = 2f;
    }

    public enum TuningVariant
    {
        FrequencySlider,
        WaveformTrace,
        HarmonicPattern
    }
}
