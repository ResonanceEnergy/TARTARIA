using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// MudGolemSpawner --- centralized wave spawner for Mud Golems with strict
    /// concurrency cap and hub-progress scaling.
    ///
    /// Contract (per agent/ai/wave-spawner-tuning):
    ///  - Hard cap: never more than MAX_ALIVE (=3) mud golems alive simultaneously.
    ///  - Wave trigger: subscribes to GameEvents.OnBuildingRestored. First restored
    ///    building fires wave 1.
    ///  - Wave size: min(MAX_ALIVE, 1 + restoredCount) clamped against the live cap.
    ///  - Cleanup: 10s after MudGolemHealth.OnDeath, Destroy(gameObject).
    ///  - Auto-bootstrap: [RuntimeInitializeOnLoadMethod(AfterSceneLoad)] singleton,
    ///    DontDestroyOnLoad.
    ///
    /// Notes:
    ///  - This file lives in Tartaria.AI which cannot reference Integration, so the
    ///    "EchohavenProgressionSystem.Instance" path is implemented via a local
    ///    restored-building counter incremented from the GameEvents.OnBuildingRestored
    ///    event (functionally equivalent and assembly-clean).
    ///  - Spawn point + prefab discovery is best-effort at runtime: prefab is loaded
    ///    from Resources/AI/MudGolem if present; spawn points are GameObjects tagged
    ///    "GolemSpawn" or, as fallback, scattered around the player.
    /// </summary>
    public class MudGolemSpawner : MonoBehaviour
    {
        public const int MAX_ALIVE = 3;
        public const float CORPSE_DESTROY_DELAY = 10f;

        private const string PrefabResourcePath = "AI/MudGolem";
        private const string SpawnPointTag = "GolemSpawn";

        public static MudGolemSpawner Instance { get; private set; }

        private readonly HashSet<MudGolemHealth> _alive = new HashSet<MudGolemHealth>();
        private GameObject _prefab;
        private int _restoredCount;
        private int _waveNumber;
        private bool _eventSubscribed;

        public int AliveCount => _alive.Count;
        public int WaveNumber => _waveNumber;
        public int RestoredCount => _restoredCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[MudGolemSpawner]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MudGolemSpawner>();
        }

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (!_eventSubscribed)
            {
                GameEvents.OnBuildingRestored += HandleBuildingRestored;
                _eventSubscribed = true;
            }

            _prefab = Resources.Load<GameObject>(PrefabResourcePath);
        }

        private void OnDisable()
        {
            if (_eventSubscribed)
            {
                GameEvents.OnBuildingRestored -= HandleBuildingRestored;
                _eventSubscribed = false;
            }
            if (Instance == this) Instance = null;
        }

        private void HandleBuildingRestored(string buildingId)
        {
            _restoredCount++;
            int waveSize = Mathf.Min(MAX_ALIVE, 1 + _restoredCount);
            SpawnWave(waveSize);
        }

        /// <summary>
        /// Spawn up to <paramref name="requested"/> golems, clamped by the live cap.
        /// Returns the number actually spawned.
        /// </summary>
        public int SpawnWave(int requested)
        {
            PruneDead();
            int budget = Mathf.Max(0, MAX_ALIVE - _alive.Count);
            int toSpawn = Mathf.Clamp(requested, 0, budget);
            if (toSpawn <= 0)
            {
                Debug.Log($"[MudGolemSpawner] Wave skipped --- alive={_alive.Count}/{MAX_ALIVE}, budget=0.");
                return 0;
            }

            _waveNumber++;
            int spawned = 0;
            for (int i = 0; i < toSpawn; i++)
            {
                if (SpawnOne()) spawned++;
            }
            Debug.Log($"[MudGolemSpawner] Wave {_waveNumber} --- spawned {spawned}/{toSpawn} (alive={_alive.Count}/{MAX_ALIVE}, restored={_restoredCount}).");
            return spawned;
        }

        private bool SpawnOne()
        {
            if (_alive.Count >= MAX_ALIVE) return false;

            Vector3 pos = PickSpawnPoint();
            GameObject go;
            if (_prefab != null)
            {
                go = Instantiate(_prefab, pos, Quaternion.identity);
            }
            else
            {
                go = new GameObject("MudGolem(Runtime)");
                go.transform.position = pos;
                go.AddComponent<MudGolemHealth>();
                go.AddComponent<MudGolemAI>();
            }

            var health = go.GetComponent<MudGolemHealth>() ?? go.AddComponent<MudGolemHealth>();
            Register(health);
            return true;
        }

        private Vector3 PickSpawnPoint()
        {
            var points = GameObject.FindGameObjectsWithTag(SpawnPointTag);
            if (points != null && points.Length > 0)
            {
                return points[Random.Range(0, points.Length)].transform.position;
            }
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 anchor = player != null ? player.transform.position : Vector3.zero;
            Vector2 ring = Random.insideUnitCircle.normalized * 12f;
            return anchor + new Vector3(ring.x, 0f, ring.y);
        }

        /// <summary>
        /// Register an externally-spawned golem so it counts toward the live cap and
        /// participates in the 10s cleanup contract.
        /// </summary>
        public void Register(MudGolemHealth health)
        {
            if (health == null) return;
            if (!_alive.Add(health)) return;
            health.OnDeath += () => HandleGolemDeath(health);
        }

        private void HandleGolemDeath(MudGolemHealth health)
        {
            if (health == null) return;
            _alive.Remove(health);
            if (health.gameObject != null)
            {
                StartCoroutine(DestroyAfterDelay(health.gameObject, CORPSE_DESTROY_DELAY));
            }
        }

        private static IEnumerator DestroyAfterDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) Destroy(go);
        }

        private void PruneDead()
        {
            _alive.RemoveWhere(h => h == null || !h.IsAlive);
        }
    }
}
