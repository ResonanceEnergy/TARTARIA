// Moon1MudGolemRSSpawnTrigger.cs — 2026-06-03 §11 Combat ship-complete
//
// Per docs/15_MVP_BUILD_SPEC.md §11: Mud Golems spawn in waves on RS thresholds.
// 1 golem at RS=25, 2 at RS=50, 3 at RS=75. Per-threshold latch prevents re-fire
// on RS oscillation. Subscribes to GameEvents.OnRSChanged (delta-based per
// GameEvents.cs:322 — every caller passes a delta amount).
//
// Bootstrap: AfterSceneLoad guarded by Echohaven_VerticalSlice scene name —
// matches Moon1NarrativeBeats pattern so Play-from-anywhere still wires up.
//
// Per CLAUDE.md anti-circling mandate: this is the canonical RS-threshold combat
// trigger. NOT a Moon1*Safety / Fix / Override pattern. It implements docs/15 §11
// directly, so the file is permanent (not quarantined).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-60)]
    public class Moon1MudGolemRSSpawnTrigger : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] GameObject mudGolemPrefab; // optional; falls back to Resources.Load
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] int[] rsThresholds = { 25, 50, 75 };
        [SerializeField] int[] golemCountPerThreshold = { 1, 2, 3 };
        [SerializeField] float perGolemStaggerSec = 1.0f;
        [SerializeField] float perimeterRadius = 18f;

        float _accumulatedRS;
        bool[] _thresholdsCrossed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name != "Echohaven_VerticalSlice") return;

            // Find/create Moon1_Systems host
            var host = GameObject.Find("Moon1_Systems");
            if (host == null)
            {
                host = new GameObject("Moon1_Systems");
            }
            if (host.GetComponent<Moon1MudGolemRSSpawnTrigger>() == null)
            {
                host.AddComponent<Moon1MudGolemRSSpawnTrigger>();
                Debug.Log("[Moon1MudGolemRSSpawnTrigger] Attached to Moon1_Systems on Echohaven scene load.");
            }
        }

        void Awake()
        {
            if (rsThresholds == null || rsThresholds.Length == 0)
            {
                rsThresholds = new int[] { 25, 50, 75 };
                golemCountPerThreshold = new int[] { 1, 2, 3 };
            }
            _thresholdsCrossed = new bool[rsThresholds.Length];
            _accumulatedRS = 0f;

            // Lazy spawn-point generation: 4 perimeter ring points around origin
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                GeneratePerimeterRing();
            }
        }

        void OnEnable()
        {
            GameEvents.OnRSChanged += HandleRSChanged;
        }

        void OnDisable()
        {
            GameEvents.OnRSChanged -= HandleRSChanged;
        }

        void HandleRSChanged(float delta)
        {
            // Per CLAUDE.md verified — all OnRSChanged publishers pass DELTAS not absolutes.
            // Accumulate locally to detect threshold crossings.
            if (delta <= 0f) return; // ignore RS loss
            _accumulatedRS += delta;

            for (int i = 0; i < rsThresholds.Length; i++)
            {
                if (_thresholdsCrossed[i]) continue;
                if (_accumulatedRS < rsThresholds[i]) continue;

                _thresholdsCrossed[i] = true;
                int count = (i < golemCountPerThreshold.Length) ? golemCountPerThreshold[i] : 1;
                Debug.Log($"[MudGolemRSSpawnTrigger] RS={_accumulatedRS:F1} crossed threshold {rsThresholds[i]} — spawning {count} golem(s).");
                StartCoroutine(SpawnWaveStaggered(count));
            }
        }

        IEnumerator SpawnWaveStaggered(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnOne();
                if (i < count - 1) yield return new WaitForSeconds(perGolemStaggerSec);
            }
        }

        void SpawnOne()
        {
            // Pick random spawn point
            Vector3 pos = Vector3.zero;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (sp != null) pos = sp.position;
            }
            else
            {
                // Random angle on perimeter ring
                float angle = Random.value * Mathf.PI * 2f;
                pos = new Vector3(Mathf.Cos(angle) * perimeterRadius, 0f, Mathf.Sin(angle) * perimeterRadius);
            }

            // NavMesh sample for valid spawn
            if (NavMesh.SamplePosition(pos, out var hit, 8f, NavMesh.AllAreas))
            {
                pos = hit.position;
            }

            GameObject golem = null;
            if (mudGolemPrefab != null)
            {
                golem = Instantiate(mudGolemPrefab, pos, Quaternion.identity);
            }
            else
            {
                // Resources fallback chain — match Moon1CombatDirector's spawn pattern
                var prefab = Resources.Load<GameObject>("Enemies/MudGolem")
                          ?? Resources.Load<GameObject>("Prefabs/AI/MudGolem")
                          ?? Resources.Load<GameObject>("Prefabs/Characters/MudGolem");
                if (prefab != null)
                {
                    golem = Instantiate(prefab, pos, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("[MudGolemRSSpawnTrigger] No mudGolemPrefab assigned + no Resources fallback found. Spawn skipped.");
                    return;
                }
            }

            if (golem != null)
            {
                golem.name = "MudGolem_WaveSpawn_" + System.DateTime.Now.Ticks;
                Debug.Log($"[MudGolemRSSpawnTrigger] Spawned at {pos}.");
            }
        }

        void GeneratePerimeterRing()
        {
            var list = new List<Transform>(4);
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("MudGolemSpawn_Auto_" + i);
                float angle = (i / 4f) * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * perimeterRadius, 0f, Mathf.Sin(angle) * perimeterRadius);
                go.transform.SetParent(transform);
                list.Add(go.transform);
            }
            spawnPoints = list.ToArray();
        }
    }
}
