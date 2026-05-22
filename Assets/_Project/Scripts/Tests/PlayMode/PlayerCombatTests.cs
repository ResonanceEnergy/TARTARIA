using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Tartaria.Gameplay;
using Tartaria.AI;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// Unit tests for PlayerCombat melee system.
    /// Tests attack mechanics, cooldowns, damage calculation, and range detection.
    /// </summary>
    public class PlayerCombatTests
    {
        [UnityTest]
        public IEnumerator Test_AttackDealsCorrectDamage()
        {
            // Setup: spawn player + enemy
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            
            // Position enemy in front of player (within 2.6m reach)
            playerGO.transform.position = Vector3.zero;
            enemyGO.transform.position = Vector3.forward * 2f;
            
            yield return null;  // Wait 1 frame for setup
            
            float initialHealth = enemy.CurrentHealth;
            Assert.Greater(initialHealth, 0f, "Enemy should have health");
            
            // Act: Simulate attack by calling Swing via reflection (private method)
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(swingMethod, "Swing method should exist");
            swingMethod.Invoke(player, null);
            
            yield return null;  // Wait for damage to apply
            
            // Assert: health decreased
            Assert.Less(enemy.CurrentHealth, initialHealth, "Enemy health should decrease after attack");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_AttackRespectsCooldown()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            yield return null;
            
            // Act: First swing
            swingMethod.Invoke(player, null);
            bool firstSwingActive = player.IsSwinging;
            
            // Try second swing immediately
            swingMethod.Invoke(player, null);
            
            yield return new WaitForSeconds(0.1f);
            
            // Assert: First swing should be active, but cooldown should prevent instant re-fire
            Assert.IsTrue(firstSwingActive, "First swing should be active");
            
            // Wait for cooldown to expire
            yield return new WaitForSeconds(0.5f);
            
            // Now swing should work again
            swingMethod.Invoke(player, null);
            Assert.IsTrue(player.IsSwinging, "Should be able to swing after cooldown");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_AttackMissesOutOfRange()
        {
            // Setup: enemy far away (beyond 2.6m reach + 1.4m radius)
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            
            playerGO.transform.position = Vector3.zero;
            enemyGO.transform.position = Vector3.forward * 10f;  // Far away
            
            yield return null;
            
            float initialHealth = enemy.CurrentHealth;
            
            // Act: Attack
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(player, null);
            
            yield return null;
            
            // Assert: Health unchanged (miss)
            Assert.AreEqual(initialHealth, enemy.CurrentHealth, 0.01f, "Enemy should not take damage when out of range");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_MultipleEnemiesInRange()
        {
            // Setup: player + 3 enemies in range
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            playerGO.transform.position = Vector3.zero;
            
            var enemies = new MudGolemHealth[3];
            for (int i = 0; i < 3; i++)
            {
                var enemyGO = new GameObject($"Enemy{i}");
                enemies[i] = enemyGO.AddComponent<MudGolemHealth>();
                enemyGO.transform.position = Vector3.forward * 2f + Vector3.right * (i - 1) * 0.5f;
            }
            
            yield return null;
            
            float[] initialHealth = new float[3];
            for (int i = 0; i < 3; i++)
            {
                initialHealth[i] = enemies[i].CurrentHealth;
            }
            
            // Act: Swing
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(player, null);
            
            yield return null;
            
            // Assert: All enemies hit
            for (int i = 0; i < 3; i++)
            {
                Assert.Less(enemies[i].CurrentHealth, initialHealth[i], $"Enemy {i} should take damage");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
            foreach (var enemy in enemies)
            {
                if (enemy != null) Object.Destroy(enemy.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Test_IsSwingingFlag()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            yield return null;
            
            // Assert: Initially not swinging
            Assert.IsFalse(player.IsSwinging, "Should not be swinging initially");
            
            // Act: Swing
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(player, null);
            
            // Assert: Now swinging
            Assert.IsTrue(player.IsSwinging, "Should be swinging immediately after swing");
            
            // Wait for swing duration to expire (0.25s)
            yield return new WaitForSeconds(0.3f);
            
            // Assert: Swing complete
            Assert.IsFalse(player.IsSwinging, "Swing should be complete after duration");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_OnSwingEventFires()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            bool eventFired = false;
            PlayerCombat.OnSwing += () => eventFired = true;
            
            yield return null;
            
            // Act: Swing
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(player, null);
            
            yield return null;
            
            // Assert: Event fired
            Assert.IsTrue(eventFired, "OnSwing event should fire");
            
            // Cleanup
            PlayerCombat.OnSwing = null;
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_DamageAppliesWithSendMessage()
        {
            // Setup: Test SendMessage pattern for cross-assembly safety
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 2f;
            
            yield return null;
            
            float initialHealth = enemy.CurrentHealth;
            
            // Act: Swing (uses SendMessage internally)
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swingMethod.Invoke(player, null);
            
            yield return null;
            
            // Assert: SendMessage delivered damage
            Assert.Less(enemy.CurrentHealth, initialHealth, "SendMessage should deliver damage");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_SwingIgnoresSelfHit()
        {
            // Setup: Player with collider (shouldn't damage self)
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            var playerCollider = playerGO.AddComponent<CapsuleCollider>();
            
            yield return null;
            
            // Act: Swing (should ignore own collider)
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Should not throw or cause errors
            Assert.DoesNotThrow(() => swingMethod.Invoke(player, null));
            
            yield return null;
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_CooldownBlocksRapidAttacks()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemyGO.transform.position = Vector3.forward * 2f;
            
            yield return null;
            
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act: Rapid fire 5 attacks (should be limited by cooldown)
            int swingAttempts = 0;
            for (int i = 0; i < 5; i++)
            {
                bool wasSwinging = player.IsSwinging;
                swingMethod.Invoke(player, null);
                if (!wasSwinging && player.IsSwinging) swingAttempts++;
                yield return new WaitForSeconds(0.05f);  // 50ms between attempts
            }
            
            // Assert: Only 1-2 swings should succeed (cooldown = 0.45s)
            Assert.LessOrEqual(swingAttempts, 2, "Cooldown should prevent rapid attacks");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }

        [UnityTest]
        public IEnumerator Test_EnemyDeathAfterMultipleHits()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var player = playerGO.AddComponent<PlayerCombat>();
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<MudGolemHealth>();
            enemy.SetMaxHealth(50f, true);  // Low health for quick kill
            enemyGO.transform.position = Vector3.forward * 2f;
            
            bool deathEventFired = false;
            enemy.OnDeath += () => deathEventFired = true;
            
            yield return null;
            
            var swingMethod = typeof(PlayerCombat).GetMethod("Swing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act: Attack until dead (25 dmg per hit, 50 HP = 2 hits)
            for (int i = 0; i < 3; i++)
            {
                swingMethod.Invoke(player, null);
                yield return new WaitForSeconds(0.5f);  // Wait for cooldown
                if (!enemy.IsAlive) break;
            }
            
            // Assert: Enemy dead
            Assert.IsFalse(enemy.IsAlive, "Enemy should be dead after multiple hits");
            Assert.IsTrue(deathEventFired, "OnDeath event should fire");
            
            // Cleanup
            Object.Destroy(playerGO);
            Object.Destroy(enemyGO);
        }
    }
}
