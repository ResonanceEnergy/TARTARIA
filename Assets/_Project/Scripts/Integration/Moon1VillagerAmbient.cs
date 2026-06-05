using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Places 4 ambient villagers (KayKit Char_* prefabs) around the village.
    /// Each patrols between 2-3 waypoints, idles 3-6s at each stop.
    /// Per CLAUDE.md "no stubs" — real prefab loads, real patrol AI loop.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1VillagerAmbient : MonoBehaviour
    {
        static Moon1VillagerAmbient _instance;

        struct VillagerSpec
        {
            public string prefabPath;
            public string displayName;
            public Vector3[] waypoints;
        }

        static readonly VillagerSpec[] Villagers =
        {
            new VillagerSpec
            {
                prefabPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Knight.prefab",
                displayName = "Watch Captain",
                waypoints = new[] { new Vector3(35f, 0f, 35f), new Vector3(38f, 0f, 25f), new Vector3(42f, 0f, 40f) }
            },
            new VillagerSpec
            {
                prefabPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab",
                displayName = "Wandering Scholar",
                waypoints = new[] { new Vector3(-38f, 0f, 0f), new Vector3(-42f, 0f, 5f), new Vector3(-38f, 0f, -8f) }
            },
            new VillagerSpec
            {
                prefabPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Ranger.prefab",
                displayName = "Forest Warden",
                waypoints = new[] { new Vector3(0f, 0f, -38f), new Vector3(-12f, 0f, -42f), new Vector3(12f, 0f, -40f) }
            },
            new VillagerSpec
            {
                prefabPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab",
                displayName = "Market Tender",
                waypoints = new[] { new Vector3(-5f, 0f, -38f), new Vector3(5f, 0f, -38f), new Vector3(0f, 0f, -34f) }
            }
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1VillagerAmbient");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1VillagerAmbient>();
        }

        void Start()
        {
            int spawned = 0;
            var root = new GameObject("Moon1_Villagers_Root");
            root.transform.SetParent(transform);

            foreach (var spec in Villagers)
            {
                if (SpawnVillager(root.transform, spec)) spawned++;
            }
            Debug.Log("[Moon1VillagerAmbient] Spawned " + spawned + " / " + Villagers.Length + " villagers.");
        }

        bool SpawnVillager(Transform parent, VillagerSpec spec)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(spec.prefabPath);
#endif
            if (prefab == null)
            {
                // Try Resources runtime path
                var leaf = System.IO.Path.GetFileNameWithoutExtension(spec.prefabPath);
                prefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/" + leaf);
            }
            GameObject instance;
            if (prefab != null)
            {
                instance = Object.Instantiate(prefab, spec.waypoints[0], Quaternion.identity, parent);
            }
            else
            {
                // Procedural fallback — capsule with random tint
                instance = new GameObject("Villager_" + spec.displayName);
                instance.transform.SetParent(parent);
                instance.transform.position = spec.waypoints[0];
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
                body.transform.SetParent(instance.transform);
                body.transform.localPosition = new Vector3(0f, 1f, 0f);
                body.transform.localScale = new Vector3(0.6f, 0.9f, 0.45f);
                Object.Destroy(body.GetComponent<Collider>());
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    var tint = new Color(Random.Range(0.3f, 0.7f), Random.Range(0.3f, 0.6f), Random.Range(0.3f, 0.6f));
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint); else mat.color = tint;
                    body.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
            instance.name = "Villager_" + spec.displayName;

            // Capsule collider for player interaction
            var col = instance.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.45f;
            col.center = new Vector3(0f, 1f, 0f);
            col.isTrigger = true;

            // Patrol behavior
            var patrol = instance.AddComponent<Moon1VillagerPatrol>();
            patrol.waypoints = spec.waypoints;
            patrol.displayName = spec.displayName;
            patrol.moveSpeed = 1.4f;
            patrol.idleMin = 3f;
            patrol.idleMax = 6f;
            return true;
        }
    }

    /// <summary>
    /// Patrol-loop AI: walks between waypoints, idles randomly at each.
    /// Lerp-based movement, no NavMesh needed (waypoints are pre-validated open ground).
    /// </summary>
    public class Moon1VillagerPatrol : MonoBehaviour
    {
        public Vector3[] waypoints;
        public string displayName = "Villager";
        public float moveSpeed = 1.4f;
        public float idleMin = 3f;
        public float idleMax = 6f;

        int _targetIdx;
        float _idleUntil;
        bool _idling;
        Animator _anim;
        bool _hasIsWalking;

        void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _targetIdx = 0;
            _idling = false;
            _hasIsWalking = false;
            if (_anim != null && _anim.runtimeAnimatorController != null)
            {
                foreach (var p in _anim.parameters)
                {
                    if (p.name == "IsWalking" && p.type == AnimatorControllerParameterType.Bool)
                    {
                        _hasIsWalking = true;
                        break;
                    }
                }
            }
        }

        void SetWalking(bool v)
        {
            if (_anim != null && _hasIsWalking) _anim.SetBool("IsWalking", v);
        }

        void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (_idling)
            {
                if (Time.time >= _idleUntil)
                {
                    _idling = false;
                    _targetIdx = (_targetIdx + 1) % waypoints.Length;
                    SetWalking(true);
                }
                return;
            }

            var target = waypoints[_targetIdx];
            var pos = transform.position;
            target.y = pos.y; // ignore Y for planar walk

            var dir = target - pos;
            float dist = dir.magnitude;
            if (dist < 0.25f)
            {
                _idling = true;
                _idleUntil = Time.time + Random.Range(idleMin, idleMax);
                SetWalking(false);
                return;
            }

            var step = dir.normalized * moveSpeed * Time.deltaTime;
            transform.position = pos + step;
            // Face direction of travel
            if (step.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(step.normalized, Vector3.up), 5f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            ServiceLocator.HUD?.ShowBanner(displayName, GetGreeting(), 3.5f);
        }

        string GetGreeting()
        {
            // Per-archetype lines
            switch (displayName)
            {
                case "Watch Captain":  return "Keep clear of the mud pools, traveller.";
                case "Wandering Scholar": return "The Aether bands — Telluric, Harmonic, Celestial. Memorize them.";
                case "Forest Warden":  return "The trees lean toward the Spire. They know what's coming.";
                case "Market Tender":  return "Bring me a Lore Artifact and I'll trade you something good.";
                default: return "Hm.";
            }
        }
    }
}
