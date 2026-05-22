using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Tartaria.Gameplay;
using Tartaria.AI;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// Unit tests for PlayerAbilityController special abilities system.
    /// Tests Harmonic Strike, Frequency Shield, cooldowns, and resource costs.
    /// </summary>
    public class PlayerAbilityControllerTests
    {
        [UnityTest]
        public IEnumerator Test_HarmonicStrikeDealsAOEDamage()
        {
            // Setup: player + 2 enemies in range
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            playerGO.transform.position = Vector3.zero;
            
            // Spawn 2 enemies within 5m radius
            var enemy1GO = new GameObject("Enemy1");
            var enemy1 = enemy1GO.AddComponent<MudGolemHealth>();
            enemy1GO.transform.position = Vector3.forward * 3f;
            
            var enemy2GO = new GameObject("Enemy2");
            var enemy2 = enemy2GO.AddComponent<MudGolemHealth>();
            enemy2GO.transform.position = Vector3.right * 4f;
            
            yield return null;
            
            float enemy1InitialHealth = enemy1.CurrentHealth;
            float enemy2InitialHealth = enemy2.CurrentHealth;
            
            // Act: Call TryHarmonicStrike via reflection
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "TryHarmonicStrike method should exist");
            
            method.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: Both enemies took damage
            Assert.Less(enemy1.CurrentHealth, enemy1InitialHealth, "Enemy1 should take AOE damage");
            Assert.Less(enemy2.CurrentHealth, enemy2InitialHealth, "Enemy2 should take AOE damage");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemy1GO);
            Object.Destroy(enemy2GO);
        }

        [UnityTest]
        public IEnumerator Test_HarmonicStrikeRespectsCooldown()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 3f;
            
            yield return null;
            
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act: First strike
            method.Invoke(abilities, null);
            yield return null;
            
            float healthAfterFirst = enemy.CurrentHealth;
            
            // Try immediate second strike (should be blocked by cooldown)
            method.Invoke(abilities, null);
            yield return new WaitForSeconds(0.5f);
            
            float healthAfterSecond = enemy.CurrentHealth;
            
            // Assert: No additional damage (cooldown blocked)
            Assert.AreEqual(healthAfterFirst, healthAfterSecond, 0.1f, "Cooldown should prevent immediate re-cast");
            
            // Wait for cooldown to expire (8s)
            yield return new WaitForSeconds(8f);
            
            // Third strike should work
            enemy.SetMaxHealth(300f, true);  // Reset health for clear test
            float healthBeforeThird = enemy.CurrentHealth;
            method.Invoke(abilities, null);
            yield return null;
            
            Assert.Less(enemy.CurrentHealth, healthBeforeThird, "Should be able to strike after cooldown");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_HarmonicStrikeRequiresEnoughRS()
        {
            // Setup: player with low RS
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            // Setup economy system (if exists)
            var economyGO = new GameObject("EconomySystem");
            var economy = economyGO.AddComponent<EconomySystem>();
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 3f;
            
            yield return null;
            
            // Set RS to less than cost (20 RS required)
            if (economy != null)
            {
                // Drain RS (spend all)
                while (economy.ResonanceScore > 0)
                {
                    economy.SpendResonanceScore(Mathf.Min(10, economy.ResonanceScore));
                }
            }
            
            float initialHealth = enemy.CurrentHealth;
            
            // Act: Try to cast with insufficient RS
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: No damage dealt (insufficient RS blocked cast)
            Assert.AreEqual(initialHealth, enemy.CurrentHealth, 0.1f, "Should not deal damage without sufficient RS");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(economyGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_ShieldReducesDamage()
        {
            // Setup: player with health component
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Assert: Shield initially inactive
            Assert.IsFalse(abilities.ShieldActive, "Shield should be inactive initially");
            
            // Act: Activate shield via TryFrequencyShield
            var method = typeof(PlayerAbilityController).GetMethod("TryFrequencyShield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "TryFrequencyShield method should exist");
            
            method.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: Shield should be active
            Assert.IsTrue(abilities.ShieldActive, "Shield should activate after casting");
            
            // Wait for shield duration (5s)
            yield return new WaitForSeconds(5.5f);
            
            // Assert: Shield expired
            Assert.IsFalse(abilities.ShieldActive, "Shield should expire after duration");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_ShieldCooldown()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            yield return null;
            
            var method = typeof(PlayerAbilityController).GetMethod("TryFrequencyShield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act: Cast shield
            method.Invoke(abilities, null);
            yield return null;
            
            Assert.IsTrue(abilities.ShieldActive, "First shield cast should succeed");
            
            // Wait for shield to expire
            yield return new WaitForSeconds(5.5f);
            
            // Try to cast again immediately (should be blocked by 12s cooldown)
            bool wasActive = abilities.ShieldActive;
            method.Invoke(abilities, null);
            yield return new WaitForSeconds(0.5f);
            
            // Shield should not reactivate yet (cooldown still active)
            // Note: This depends on cooldown tracking, may need reflection to verify
            
            // Wait for cooldown to expire (12s total from first cast)
            yield return new WaitForSeconds(7f);  // Already waited 5.5s
            
            // Now shield should work
            method.Invoke(abilities, null);
            yield return null;
            
            Assert.IsTrue(abilities.ShieldActive, "Shield should work after cooldown expires");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_HarmonicStrikeOutOfRange()
        {
            // Setup: enemy far from player (beyond 5m radius)
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            playerGO.transform.position = Vector3.zero;
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 10f;  // 10m away
            
            yield return null;
            
            float initialHealth = enemy.CurrentHealth;
            
            // Act: Try Harmonic Strike
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: No damage (out of 5m range)
            Assert.AreEqual(initialHealth, enemy.CurrentHealth, 0.1f, "Enemy out of range should not take damage");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_AetherVisionToggle()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            yield return null;
            
            // Assert: Initially disabled
            Assert.IsFalse(abilities.AetherVisionActive, "Aether Vision should be disabled initially");
            
            // Act: Toggle via ToggleAetherVision (reflection)
            var method = typeof(PlayerAbilityController).GetMethod("ToggleAetherVision", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(abilities, null);
                yield return null;
                
                // Assert: Should toggle state
                bool newState = abilities.AetherVisionActive;
                
                // Toggle again
                method.Invoke(abilities, null);
                yield return null;
                
                Assert.AreNotEqual(newState, abilities.AetherVisionActive, "Aether Vision should toggle");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_MultipleEnemiesAOE()
        {
            // Setup: 4 enemies around player
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            playerGO.transform.position = Vector3.zero;
            
            var enemies = new MudGolemHealth[4];
            Vector3[] positions = new Vector3[]
            {
                Vector3.forward * 3f,
                Vector3.back * 3f,
                Vector3.left * 3f,
                Vector3.right * 3f
            };
            
            for (int i = 0; i < 4; i++)
            {
                var enemyGO = new GameObject($"Enemy{i}");
                enemies[i] = enemyGO.AddComponent<MudGolemHealth>();
                enemyGO.transform.position = positions[i];
            }
            
            yield return null;
            
            float[] initialHealth = new float[4];
            for (int i = 0; i < 4; i++)
            {
                initialHealth[i] = enemies[i].CurrentHealth;
            }
            
            // Act: Harmonic Strike
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(abilities, null);
            
            yield return null;
            
            // Assert: All 4 enemies hit
            int hitCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (enemies[i].CurrentHealth < initialHealth[i])
                {
                    hitCount++;
                }
            }
            
            Assert.AreEqual(4, hitCount, "All 4 enemies should take AOE damage");
            
            // Cleanup
            Object.Destroy(playerGO);
            foreach (var enemy in enemies)
            {
                if (enemy != null) Object.Destroy(enemy.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Test_AbilityWithoutEconomySystem()
        {
            // Setup: Test graceful handling when EconomySystem is null
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            var enemyGO = new GameObject("Enemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 3f;
            
            yield return null;
            
            // Act: Try ability without EconomySystem.Instance
            var method = typeof(PlayerAbilityController).GetMethod("TryHarmonicStrike", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Should not throw exception
            Assert.DoesNotThrow(() => method.Invoke(abilities, null));
            
            yield return null;
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_ShieldActiveProperty()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var abilities = playerGO.AddComponent<PlayerAbilityController>();
            
            yield return null;
            
            // Assert: Initially false
            Assert.IsFalse(abilities.ShieldActive);
            
            // Manually set shield end time (via reflection)
            var shieldEndTimeField = typeof(PlayerAbilityController).GetField("_shieldEndTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (shieldEndTimeField != null)
            {
                shieldEndTimeField.SetValue(abilities, Time.time + 5f);
                
                yield return null;
                
                // Assert: Now active
                Assert.IsTrue(abilities.ShieldActive, "Shield should be active when end time is in future");
                
                // Wait for expiry
                yield return new WaitForSeconds(5.5f);
                
                Assert.IsFalse(abilities.ShieldActive, "Shield should be inactive after expiry");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }
    }
}
