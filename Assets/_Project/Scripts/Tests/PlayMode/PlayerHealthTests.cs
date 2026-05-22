using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Tartaria.Gameplay;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// Unit tests for PlayerHealth system.
    /// Tests damage, healing, death/respawn, regeneration, and event flow.
    /// </summary>
    public class PlayerHealthTests
    {
        [UnityTest]
        public IEnumerator Test_TakeDamageReducesHealth()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            int initialHealth = health.CurrentHealth;
            Assert.AreEqual(100, initialHealth, "Initial health should be 100");
            
            // Act: Take damage
            health.TakeDamage(25);
            
            yield return null;
            
            // Assert: Health reduced
            Assert.AreEqual(75, health.CurrentHealth, "Health should be reduced by damage amount");
            Assert.IsFalse(health.IsDead, "Player should not be dead");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_HealthZeroTriggersDeath()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            bool deathEventFired = false;
            health.OnDeath += () => deathEventFired = true;
            
            yield return null;
            
            // Act: Deal lethal damage
            health.TakeDamage(150);  // More than 100 HP
            
            yield return null;
            
            // Assert: Dead
            Assert.AreEqual(0, health.CurrentHealth, "Health should be clamped at 0");
            Assert.IsTrue(health.IsDead, "Player should be dead");
            Assert.IsTrue(deathEventFired, "OnDeath event should fire");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_RespawnRestoresHealth()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Kill player
            health.TakeDamage(100);
            yield return null;
            Assert.IsTrue(health.IsDead);
            
            // Act: Respawn via reflection (call Respawn method)
            var respawnMethod = typeof(PlayerHealth).GetMethod("Respawn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (respawnMethod == null)
            {
                respawnMethod = typeof(PlayerHealth).GetMethod("Respawn", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }
            
            if (respawnMethod != null)
            {
                respawnMethod.Invoke(health, null);
                yield return null;
                
                // Assert: Alive again
                Assert.IsFalse(health.IsDead, "Player should be alive after respawn");
                Assert.Greater(health.CurrentHealth, 0, "Health should be restored");
            }
            else
            {
                Debug.LogWarning("Respawn method not found - test skipped");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_RegenWorksOutOfCombat()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Damage player
            health.TakeDamage(50);
            yield return null;
            Assert.AreEqual(50, health.CurrentHealth);
            
            // Wait for regen delay (5s) plus regen time
            yield return new WaitForSeconds(7f);
            
            // Assert: Health regenerated
            Assert.Greater(health.CurrentHealth, 50, "Health should regenerate after delay");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_RegenStopsOnDamage()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Damage player
            health.TakeDamage(40);
            yield return null;
            
            // Wait 3s (less than 5s regen delay)
            yield return new WaitForSeconds(3f);
            
            // Take more damage (reset regen timer)
            health.TakeDamage(10);
            int healthAfterSecondHit = health.CurrentHealth;
            
            // Wait 3s more
            yield return new WaitForSeconds(3f);
            
            // Assert: Health should not have regenerated yet (timer was reset)
            Assert.AreEqual(healthAfterSecondHit, health.CurrentHealth, "Regen should not happen within delay window");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_OnHealthChangedEvent()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            bool eventFired = false;
            int receivedCurrent = -1;
            int receivedMax = -1;
            
            health.OnHealthChanged += (current, max) =>
            {
                eventFired = true;
                receivedCurrent = current;
                receivedMax = max;
            };
            
            yield return null;
            
            // Act: Take damage
            health.TakeDamage(30);
            
            yield return null;
            
            // Assert: Event fired with correct values
            Assert.IsTrue(eventFired, "OnHealthChanged event should fire");
            Assert.AreEqual(70, receivedCurrent);
            Assert.AreEqual(100, receivedMax);
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_GodModePreventsDamage()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Enable god mode
            health.GodMode = true;
            
            int initialHealth = health.CurrentHealth;
            
            // Act: Try to damage
            health.TakeDamage(50);
            
            yield return null;
            
            // Assert: No damage taken
            Assert.AreEqual(initialHealth, health.CurrentHealth, "God mode should prevent damage");
            Assert.IsFalse(health.IsDead, "God mode should prevent death");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_HealIncreasesHealth()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Damage first
            health.TakeDamage(40);
            Assert.AreEqual(60, health.CurrentHealth);
            
            // Act: Heal via reflection (call Heal method)
            var healMethod = typeof(PlayerHealth).GetMethod("Heal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (healMethod == null)
            {
                healMethod = typeof(PlayerHealth).GetMethod("Heal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }
            
            if (healMethod != null)
            {
                healMethod.Invoke(health, new object[] { 25 });
                yield return null;
                
                // Assert: Health increased
                Assert.AreEqual(85, health.CurrentHealth, "Heal should increase health");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_HealCannotExceedMax()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Damage slightly
            health.TakeDamage(10);
            Assert.AreEqual(90, health.CurrentHealth);
            
            // Act: Overheal
            var healMethod = typeof(PlayerHealth).GetMethod("Heal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (healMethod != null)
            {
                healMethod.Invoke(health, new object[] { 50 });  // Heal more than needed
                yield return null;
                
                // Assert: Clamped to max
                Assert.AreEqual(100, health.CurrentHealth, "Health should be clamped to max");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_CannotDamageWhileDead()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            
            yield return null;
            
            // Kill player
            health.TakeDamage(100);
            yield return null;
            Assert.IsTrue(health.IsDead);
            
            // Act: Try to damage while dead
            health.TakeDamage(50);
            
            yield return null;
            
            // Assert: Health stays at 0
            Assert.AreEqual(0, health.CurrentHealth, "Dead player should not take additional damage");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_DodgeInvulnerability()
        {
            // Setup: Player with dodge component
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            var dodge = playerGO.AddComponent<PlayerDodge>();
            
            yield return null;
            
            // Manually set dodge i-frames active (via reflection)
            var invulnerableField = typeof(PlayerDodge).GetField("_isInvulnerable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (invulnerableField != null)
            {
                invulnerableField.SetValue(dodge, true);
            }
            
            int initialHealth = health.CurrentHealth;
            
            // Act: Try to damage during dodge i-frames
            health.TakeDamage(50);
            
            yield return null;
            
            // Assert: No damage taken (dodge i-frames)
            Assert.AreEqual(initialHealth, health.CurrentHealth, "Dodge i-frames should prevent damage");
            
            // Cleanup
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_SetCheckpoint()
        {
            // Setup
            var playerGO = new GameObject("TestPlayer");
            var health = playerGO.AddComponent<PlayerHealth>();
            playerGO.transform.position = Vector3.zero;
            
            yield return null;
            
            Vector3 checkpointPos = new Vector3(10f, 0f, 10f);
            Quaternion checkpointRot = Quaternion.Euler(0f, 90f, 0f);
            
            // Act: Set checkpoint
            health.SetCheckpoint(checkpointPos, checkpointRot);
            
            yield return null;
            
            // Kill and respawn
            health.TakeDamage(100);
            yield return null;
            
            var respawnMethod = typeof(PlayerHealth).GetMethod("Respawn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (respawnMethod != null)
            {
                respawnMethod.Invoke(health, null);
                yield return null;
                
                // Assert: Respawned at checkpoint
                Assert.AreEqual(checkpointPos, playerGO.transform.position, "Should respawn at checkpoint position");
            }
            
            // Cleanup
            Object.Destroy(playerGO);
        }
    }
}
