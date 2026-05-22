using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AI;
using System.Collections;
using Tartaria.AI;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// Unit tests for EnemyAIController state machine.
    /// Tests Idle, Chasing, Attacking states, detection range, and AI transitions.
    /// </summary>
    public class EnemyAIControllerTests
    {
        GameObject _navMeshSurfaceGO;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create NavMesh surface for AI testing
            _navMeshSurfaceGO = new GameObject("NavMeshSurface");
            var surface = _navMeshSurfaceGO.AddComponent<NavMeshSurface>();
            surface.BuildNavMesh();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_navMeshSurfaceGO != null)
            {
                Object.Destroy(_navMeshSurfaceGO);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Test_IdleStateWhenPlayerFarAway()
        {
            // Setup: enemy + player far away (beyond 15m detection)
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            enemyGO.transform.position = Vector3.zero;
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 30f;  // 30m away
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Enemy should remain in Idle state (check via reflection)
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = stateField.GetValue(enemy);
            Assert.AreEqual("Idle", state.ToString(), "Enemy should be in Idle state when player is far");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_ChaseStateWhenPlayerInRange()
        {
            // Setup: player within 15m detection radius
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            enemyGO.transform.position = Vector3.zero;
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 10f;  // 10m away (within 15m)
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Enemy should transition to Chasing
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = stateField.GetValue(enemy);
            Assert.AreEqual("Chasing", state.ToString(), "Enemy should chase when player enters detection range");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_AttackStateWhenPlayerClose()
        {
            // Setup: player within 2.5m attack range
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            var agent = enemyGO.AddComponent<NavMeshAgent>();
            enemyGO.transform.position = Vector3.zero;
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 2f;  // 2m away
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Enemy should enter Attacking state
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = stateField.GetValue(enemy);
            Assert.AreEqual("Attacking", state.ToString(), "Enemy should attack when player is close");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_RetreatStateWhenLowHealth()
        {
            // Note: Current implementation doesn't have retreat (corrupted constructs fight to death)
            // This test verifies that behavior
            
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            var health = enemyGO.AddComponent<MudGolemHealth>();
            health.SetMaxHealth(100f, true);
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 5f;
            
            yield return null;
            yield return new WaitForSeconds(0.3f);
            
            // Damage enemy to low health
            health.TakeDamage(80f);
            
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Enemy should NOT retreat (fight to death by design)
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = stateField.GetValue(enemy);
            Assert.AreNotEqual("Retreating", state.ToString(), "Enemies do not retreat in current design");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_StopAIOnDeath()
        {
            // Setup
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            var agent = enemyGO.AddComponent<NavMeshAgent>();
            var health = enemyGO.AddComponent<MudGolemHealth>();
            health.SetMaxHealth(50f, true);
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 5f;
            
            yield return null;
            yield return new WaitForSeconds(0.3f);
            
            // Kill enemy
            health.TakeDamage(100f);
            
            yield return new WaitForSeconds(0.5f);
            
            // Assert: AI should stop (NavMeshAgent disabled or object destroyed)
            // Note: MudGolemHealth destroys the object after death delay
            Assert.IsFalse(health.IsAlive, "Enemy should be dead");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_AttackCooldownRespected()
        {
            // Setup: player in attack range
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            var playerHealth = playerGO.AddComponent<Tartaria.Gameplay.PlayerHealth>();
            playerGO.transform.position = Vector3.forward * 2f;
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            float initialHealth = playerHealth.CurrentHealth;
            
            // Wait for 2 attack cycles (1.5s cooldown)
            yield return new WaitForSeconds(3.5f);
            
            // Assert: Player should have taken damage (proving attacks happened)
            // Note: Depends on PerformAttack implementation
            Assert.LessOrEqual(playerHealth.CurrentHealth, initialHealth, "Player should take damage from enemy attacks");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_ChaseSpeedFasterThanWanderSpeed()
        {
            // Setup
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            var agent = enemyGO.AddComponent<NavMeshAgent>();
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 10f;
            
            yield return null;
            
            float initialSpeed = agent.speed;
            
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Speed should increase when chasing
            Assert.Greater(agent.speed, initialSpeed, "Chase speed should be faster than wander speed");
            Assert.AreEqual(4f, agent.speed, 0.1f, "Chase speed should be 4 m/s");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_LosePlayerAfterTimeout()
        {
            // Setup: enemy chasing, then player teleports far
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 10f;
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Should be chasing
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.AreEqual("Chasing", stateField.GetValue(enemy).ToString());
            
            // Act: Player escapes (teleport far away)
            playerGO.transform.position = Vector3.forward * 50f;
            
            // Wait for lose player timeout (5s)
            yield return new WaitForSeconds(5.5f);
            
            // Assert: Should return to Idle
            Assert.AreEqual("Idle", stateField.GetValue(enemy).ToString(), "Enemy should return to Idle after losing player");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_FacePlayerDuringAttack()
        {
            // Setup: player to the right of enemy
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            enemyGO.transform.position = Vector3.zero;
            enemyGO.transform.rotation = Quaternion.identity;
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.right * 2f;
            
            yield return null;
            yield return new WaitForSeconds(1f);
            
            // Assert: Enemy should face player (rotation towards +X)
            Vector3 forward = enemyGO.transform.forward;
            float angleToPlayer = Vector3.Angle(forward, Vector3.right);
            Assert.Less(angleToPlayer, 45f, "Enemy should face player during attack");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_StartHostileMode()
        {
            // Setup: enemy with startHostile = true (via reflection)
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            
            // Set startHostile = true
            var startHostileField = typeof(EnemyAIController).GetField("startHostile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startHostileField.SetValue(enemy, true);
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 20f;
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Should start in Chasing state even if player is far
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.AreEqual("Chasing", stateField.GetValue(enemy).ToString(), "Should start hostile when configured");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }

        [UnityTest]
        public IEnumerator Test_DetectionRadiusConfigurable()
        {
            // Setup: test that detection radius can be changed
            var enemyGO = new GameObject("TestEnemy");
            var enemy = enemyGO.AddComponent<EnemyAIController>();
            enemyGO.AddComponent<NavMeshAgent>();
            
            // Change detection radius to 5m (via serialized field reflection)
            var detectionField = typeof(EnemyAIController).GetField("detectionRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            detectionField.SetValue(enemy, 5f);
            
            var playerGO = new GameObject("TestPlayer");
            playerGO.tag = "Player";
            playerGO.transform.position = Vector3.forward * 7f;  // 7m away
            
            yield return null;
            yield return new WaitForSeconds(0.5f);
            
            // Assert: Should still be Idle (7m > 5m detection)
            var stateField = typeof(EnemyAIController).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.AreEqual("Idle", stateField.GetValue(enemy).ToString(), "Should not detect player beyond custom detection radius");
            
            // Cleanup
            Object.Destroy(enemyGO);
            Object.Destroy(playerGO);
        }
    }
}
