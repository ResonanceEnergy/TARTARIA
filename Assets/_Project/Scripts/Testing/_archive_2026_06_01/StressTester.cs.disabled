using UnityEngine;
using System.Collections;
using Tartaria.Core;

namespace Tartaria.Testing
{
    /// <summary>
    /// StressTester - Agent 2: Edge case and stress testing.
    /// </summary>
    public class StressTester : MonoBehaviour
    {
        [Header("Stress Test Settings")]
        [SerializeField] private bool isRunning = false;
        [SerializeField] private float sessionDuration = 600f; // 10 minutes
        [SerializeField] private int enemiesSpawned = 0;
        [SerializeField] private int maxEnemies = 100;

        public void StartStressTest()
        {
            if (isRunning) return;
            StartCoroutine(StressTestSequence());
        }

        IEnumerator StressTestSequence()
        {
            isRunning = true;
            float startTime = Time.time;

            Debug.Log("[StressTester] Starting 10-minute stress test...");

            while (Time.time - startTime < sessionDuration)
            {
                // Spawn enemies
                if (enemiesSpawned < maxEnemies)
                {
                    SpawnEnemy();
                    enemiesSpawned++;
                }

                // Add inventory items (inventory bloat test)
                if (InventorySystem.Instance != null && Random.value < 0.1f)
                {
                    InventorySystem.Instance.AddItem($"TestItem_{Random.Range(0, 1000)}");
                }

                yield return new WaitForSeconds(1f);
            }

            isRunning = false;
            Debug.Log($"[StressTester] ✅ Stress test complete! Spawned {enemiesSpawned} enemies");
        }

        void SpawnEnemy()
        {
            var player = PlayerSpawner.Instance?.GetPlayer();
            if (player != null)
            {
                Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 20f;
                spawnPos.y = 1f;
                
                GameObject enemy = new GameObject($"StressTest_Enemy_{enemiesSpawned}");
                enemy.transform.position = spawnPos;
                enemy.AddComponent<MudGolemEnemy>();
            }
        }
    }
}
