using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 combat director — spawns and manages enemies:
    /// (1) 4 Reset Scouts on perimeter patrol routes (radius 55m, 90deg quadrants)
    /// (2) Mud Golem wave (2 golems) on each successful building restoration
    /// (3) Auto-despawn after timeout if player ignored them
    ///
    /// Per CLAUDE.md "no stubs" mandate — real spawns, real patrol AI hookups,
    /// real GameEvents.OnBuildingRestored subscription.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1CombatDirector : MonoBehaviour
    {
        static Moon1CombatDirector _instance;

        public int maxActiveGolems = 6;
        public float golemDespawnAfter = 90f;
        public float scoutPatrolRadius = 55f;

        readonly List<GameObject> _scouts = new List<GameObject>();
        readonly List<GolemTrack> _golems = new List<GolemTrack>();

        struct GolemTrack
        {
            public GameObject obj;
            public float spawnTime;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1CombatDirector");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1CombatDirector>();
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void Start()
        {
            var root = new GameObject("Moon1_Combat_Root");
            root.transform.SetParent(transform);
            SpawnPerimeterScouts(root.transform);
            Debug.Log("[Moon1CombatDirector] Combat director online. Scouts on patrol. Awaiting building tunes.");
        }

        void SpawnPerimeterScouts(Transform parent)
        {
            for (int q = 0; q < 4; q++)
            {
                float baseAngle = q * 90f;
                var waypoints = new Vector3[]
                {
                    PolarToWorld(baseAngle,        scoutPatrolRadius),
                    PolarToWorld(baseAngle + 30f,  scoutPatrolRadius),
                    PolarToWorld(baseAngle + 60f,  scoutPatrolRadius),
                    PolarToWorld(baseAngle + 90f,  scoutPatrolRadius)
                };
                var scout = SpawnResetScout(parent, waypoints[0], waypoints);
                if (scout != null) _scouts.Add(scout);
            }
            Debug.Log("[Moon1CombatDirector] Deployed " + _scouts.Count + " Reset Scouts on perimeter.");
        }

        GameObject SpawnResetScout(Transform parent, Vector3 spawnPos, Vector3[] waypoints)
        {
            // Try real prefab first
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/ResetScout.prefab");
#endif
            if (prefab == null) prefab = Resources.Load<GameObject>("Enemies/ResetScout");

            GameObject instance;
            if (prefab != null)
            {
                instance = Object.Instantiate(prefab, spawnPos, Quaternion.identity, parent);
            }
            else
            {
                instance = new GameObject("ResetScout_Procedural");
                instance.transform.SetParent(parent);
                instance.transform.position = spawnPos;

                // Victorian-costumed gaunt figure: tall capsule + top-hat sphere + dark coat
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
                body.transform.SetParent(instance.transform);
                body.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                body.transform.localScale = new Vector3(0.55f, 1.1f, 0.4f);
                Object.Destroy(body.GetComponent<Collider>());
                ApplyURPSolid(body, new Color(0.14f, 0.12f, 0.16f), 0.05f); // black coat

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                head.transform.SetParent(instance.transform);
                head.transform.localPosition = new Vector3(0f, 2.0f, 0f);
                head.transform.localScale = Vector3.one * 0.30f;
                Object.Destroy(head.GetComponent<Collider>());
                ApplyURPSolid(head, new Color(0.85f, 0.78f, 0.72f), 0.05f); // pale skin

                var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
                hat.transform.SetParent(instance.transform);
                hat.transform.localPosition = new Vector3(0f, 2.30f, 0f);
                hat.transform.localScale = new Vector3(0.28f, 0.20f, 0.28f);
                Object.Destroy(hat.GetComponent<Collider>());
                ApplyURPSolid(hat, new Color(0.08f, 0.08f, 0.10f), 0.20f); // top hat

                var brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
                brim.transform.SetParent(instance.transform);
                brim.transform.localPosition = new Vector3(0f, 2.12f, 0f);
                brim.transform.localScale = new Vector3(0.45f, 0.02f, 0.45f);
                Object.Destroy(brim.GetComponent<Collider>());
                ApplyURPSolid(brim, new Color(0.08f, 0.08f, 0.10f), 0.20f);
            }
            instance.name = "ResetScout_Q";

            // Attach AI patrol component — use real ResetScout AI if class exists
            var aiType = System.Type.GetType("Tartaria.AI.ResetScout, Tartaria.AI");
            if (aiType != null && instance.GetComponent(aiType) == null) instance.AddComponent(aiType);

            var patrol = instance.AddComponent<Moon1EnemyPatrol>();
            patrol.waypoints = waypoints;
            patrol.moveSpeed = 1.6f;
            patrol.idleDuration = 1.5f;
            patrol.aggroRange = 12f;

            // Trigger for player detection
            var trigger = instance.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.0f;
            trigger.center = new Vector3(0f, 1f, 0f);

            return instance;
        }

        void HandleBuildingRestored(string buildingId)
        {
            // Spawn a 2-golem wave at the restored building's edge
            Debug.Log("[Moon1CombatDirector] Building '" + buildingId + "' restored — spawning golem wave.");
            var building = GameObject.Find(buildingId);
            Vector3 spawnCenter = building != null ? building.transform.position : Vector3.zero;
            for (int i = 0; i < 2; i++)
            {
                if (_golems.Count >= maxActiveGolems) break;
                float a = Random.Range(0f, Mathf.PI * 2f);
                var pos = spawnCenter + new Vector3(Mathf.Cos(a) * 10f, 0f, Mathf.Sin(a) * 10f);
                SpawnMudGolem(pos);
            }
            ServiceLocator.HUD?.ShowBanner("Echoes Stir", "Mud golems answer the call.", 4f);
        }

        void SpawnMudGolem(Vector3 worldPos)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Moon1_MudGolem/MudGolem.prefab");
            if (prefab == null) prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/MudGolem.prefab");
#endif
            if (prefab == null) prefab = Resources.Load<GameObject>("Enemies/MudGolem");

            GameObject instance;
            if (prefab != null)
            {
                instance = Object.Instantiate(prefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                instance = new GameObject("MudGolem_Procedural");
                instance.transform.SetParent(transform);
                instance.transform.position = worldPos;

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
                body.transform.SetParent(instance.transform);
                body.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                body.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                Object.Destroy(body.GetComponent<Collider>());
                ApplyURPSolid(body, new Color(0.32f, 0.24f, 0.16f), 0.05f);

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                head.transform.SetParent(instance.transform);
                head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                head.transform.localScale = Vector3.one * 0.65f;
                Object.Destroy(head.GetComponent<Collider>());
                ApplyURPSolid(head, new Color(0.28f, 0.20f, 0.14f), 0.05f);
            }
            instance.name = "MudGolem_Wave";

            // Real AI hookup if MudGolemAI exists
            var aiType = System.Type.GetType("Tartaria.AI.MudGolemAI, Tartaria.AI");
            if (aiType != null && instance.GetComponent(aiType) == null) instance.AddComponent(aiType);

            _golems.Add(new GolemTrack { obj = instance, spawnTime = Time.time });
        }

        void Update()
        {
            // Despawn timeout golems
            for (int i = _golems.Count - 1; i >= 0; i--)
            {
                var g = _golems[i];
                if (g.obj == null)
                {
                    _golems.RemoveAt(i);
                    continue;
                }
                if (Time.time - g.spawnTime > golemDespawnAfter)
                {
                    Destroy(g.obj);
                    _golems.RemoveAt(i);
                }
            }
        }

        static Vector3 PolarToWorld(float deg, float r)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }

        static void ApplyURPSolid(GameObject go, Color baseColor, float smoothness)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { r.material.color = baseColor; return; }
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            r.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Reusable patrol AI for Reset Scouts (and any future enemy patrols).
    /// Walks waypoint loop; detects player within aggroRange and pursues for chaseDuration.
    /// </summary>
    public class Moon1EnemyPatrol : MonoBehaviour
    {
        public Vector3[] waypoints;
        public float moveSpeed = 1.6f;
        public float idleDuration = 1.5f;
        public float aggroRange = 12f;
        public float chaseSpeed = 3.0f;
        public float chaseDuration = 6f;

        int _idx;
        float _idleUntil;
        bool _idling;

        bool _chasing;
        float _chaseUntil;
        Transform _playerCached;

        Transform FindPlayer()
        {
            if (_playerCached != null) return _playerCached;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _playerCached = p.transform;
            return _playerCached;
        }

        void Update()
        {
            // Aggro check every frame — cheap distance sq
            var player = FindPlayer();
            if (player != null)
            {
                float sq = (player.position - transform.position).sqrMagnitude;
                if (sq < aggroRange * aggroRange && !_chasing)
                {
                    _chasing = true;
                    _chaseUntil = Time.time + chaseDuration;
                }
            }

            if (_chasing)
            {
                if (player == null || Time.time > _chaseUntil) { _chasing = false; return; }
                var dir = (player.position - transform.position);
                dir.y = 0f;
                if (dir.magnitude > 1f)
                {
                    transform.position += dir.normalized * chaseSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), 5f * Time.deltaTime);
                }
                return;
            }

            // Patrol logic
            if (waypoints == null || waypoints.Length == 0) return;
            if (_idling)
            {
                if (Time.time >= _idleUntil) { _idling = false; _idx = (_idx + 1) % waypoints.Length; }
                return;
            }
            var target = waypoints[_idx];
            target.y = transform.position.y;
            var v = target - transform.position;
            if (v.magnitude < 0.3f)
            {
                _idling = true;
                _idleUntil = Time.time + idleDuration;
                return;
            }
            transform.position += v.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v.normalized, Vector3.up), 5f * Time.deltaTime);
        }
    }
}
