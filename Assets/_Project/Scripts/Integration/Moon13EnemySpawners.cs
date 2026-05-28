using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-33)]
    public class Moon13EnemySpawners : MonoBehaviour
    {
        [Header("Moon 13: Convergence Enemy Spawners")]
        [SerializeField] int basicSpawnCount = 8;
        [SerializeField] int eliteSpawnCount = 4;
        [SerializeField] int bossArenaCount = 1;
        [SerializeField] int patrolRouteCount = 2;
        List<GameObject> spawners = new List<GameObject>();
        void Start() { CreateSpawnZones(); }
        void CreateSpawnZones()
        {
            for (int i = 0; i < basicSpawnCount; i++)
                CreateBasicSpawn($"BasicSpawn_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), Random.Range(2, 5), "MixedTypes");
            for (int i = 0; i < eliteSpawnCount; i++)
                CreateEliteSpawn($"EliteSpawn_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), Random.Range(1, 3), "AllTypes");
            CreateBossArena("BossArena_Convergence", new Vector3(0f, 0.5f, -80f), 20f, "AetherGolem");
            for (int i = 0; i < patrolRouteCount; i++)
                CreatePatrolRoute($"PatrolRoute_{i}", GeneratePatrolWaypoints(), "MixedTypes");
            Debug.Log($"👹 Moon13EnemySpawners: {spawners.Count} spawn zones created");
        }
        GameObject CreateBasicSpawn(string name, Vector3 pos, int enemyCount, string enemyType) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; obj.transform.localScale = Vector3.one * 1.5f; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(1f, 0.2f, 0.2f); mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.red); rend.material = mat; } var spawn = obj.AddComponent<Moon13EnemySpawnPoint>(); spawn.enemyType = enemyType; spawn.spawnCount = enemyCount; spawn.spawnRadius = 8f; spawners.Add(obj); return obj; }
        GameObject CreateEliteSpawn(string name, Vector3 pos, int enemyCount, string eliteType) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; obj.transform.localScale = Vector3.one * 2f; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(1f, 0.8f, 0.2f); mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.yellow); rend.material = mat; } var spawn = obj.AddComponent<Moon13EnemySpawnPoint>(); spawn.enemyType = eliteType; spawn.spawnCount = enemyCount; spawn.spawnRadius = 10f; spawners.Add(obj); return obj; }
        GameObject CreateBossArena(string name, Vector3 center, float radius, string bossType) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = center; obj.transform.localScale = Vector3.one * 4f; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(1f, 0f, 1f); mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", new Color(2f, 0f, 2f)); rend.material = mat; } var spawn = obj.AddComponent<Moon13EnemySpawnPoint>(); spawn.enemyType = bossType; spawn.spawnCount = 1; spawn.spawnRadius = radius; spawners.Add(obj); return obj; }
        GameObject CreatePatrolRoute(string name, Vector3[] waypoints, string enemyType) { GameObject obj = new GameObject(name); obj.transform.position = waypoints[0]; var spawn = obj.AddComponent<Moon13EnemySpawnPoint>(); spawn.enemyType = enemyType; spawn.spawnCount = 2; spawn.isPatrol = true; spawn.patrolWaypoints = waypoints; foreach (var wp in waypoints) { GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); marker.name = $"{name}_Waypoint"; marker.transform.position = wp; marker.transform.localScale = Vector3.one * 0.5f; marker.transform.SetParent(obj.transform); Renderer rend = marker.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(0.2f, 0.8f, 1f); rend.material = mat; } } spawners.Add(obj); return obj; }
        Vector3[] GeneratePatrolWaypoints() { int count = Random.Range(4, 7); Vector3[] waypoints = new Vector3[count]; Vector3 center = new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f)); for (int i = 0; i < count; i++) { float angle = (i / (float)count) * Mathf.PI * 2f; float radius = Random.Range(10f, 20f); waypoints[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius); } return waypoints; }
        void OnDestroy() { foreach (var obj in spawners) if (obj != null) Destroy(obj); spawners.Clear(); }
    }
    public class Moon13EnemySpawnPoint : MonoBehaviour
    {
        public string enemyType;
        public int spawnCount;
        public float spawnRadius = 5f;
        public bool isPatrol;
        public Vector3[] patrolWaypoints;
    }
}
