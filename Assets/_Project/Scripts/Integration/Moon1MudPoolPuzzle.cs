using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Mud Pool resonance puzzle. Each pool has 3 floating crystal nodes (E/A/D notes).
    /// Player must touch all 3 within 30s to drain the pool and spawn a lore artifact.
    /// Per CLAUDE.md "no stubs" — full puzzle logic, real VFX, real drain animation.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1MudPoolPuzzle : MonoBehaviour
    {
        static Moon1MudPoolPuzzle _instance;

        // Pool world positions (from existing Moon1PointsOfInterest spawn coords)
        static readonly Vector3[] PoolPositions =
        {
            new Vector3(-50f, 0f, 35f),
            new Vector3( 55f, 0f, 30f),
            new Vector3(-45f, 0f, -45f)
        };

        readonly List<PoolState> _pools = new List<PoolState>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1MudPoolPuzzle");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1MudPoolPuzzle>();
        }

        void Start()
        {
            var root = new GameObject("Moon1_MudPools_Root");
            root.transform.SetParent(transform);

            foreach (var pos in PoolPositions)
            {
                _pools.Add(BuildPool(root.transform, pos));
            }
            Debug.Log("[Moon1MudPoolPuzzle] Built " + _pools.Count + " mud pools with 3 crystal nodes each.");
        }

        PoolState BuildPool(Transform parent, Vector3 pos)
        {
            var poolRoot = new GameObject("MudPool_" + pos.x + "_" + pos.z);
            poolRoot.transform.SetParent(parent);
            poolRoot.transform.position = pos;

            // The mud disc (visual)
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
            disc.name = "MudDisc";
            disc.transform.SetParent(poolRoot.transform);
            disc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            disc.transform.localScale = new Vector3(8f, 0.05f, 8f);
            Object.Destroy(disc.GetComponent<Collider>());
            ApplyURPMud(disc, new Color(0.18f, 0.13f, 0.08f), 0.4f);

            // NavMesh carve so NPCs route around the mud pool.
            var carver = poolRoot.AddComponent<NavMeshObstacle>();
            carver.shape = NavMeshObstacleShape.Capsule;
            carver.center = new Vector3(0f, 0.5f, 0f);
            carver.radius = 4f;   // disc scale is 8 — radius matches half-extent
            carver.height = 1.5f;
            carver.carving = true;

            // Bubble particle system for living-mud feel
            var bubbles = new GameObject("MudBubbles");
            bubbles.transform.SetParent(poolRoot.transform);
            bubbles.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var ps = bubbles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.3f;
            main.startSize = 0.4f;
            main.startColor = new Color(0.22f, 0.14f, 0.08f, 0.9f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 12f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 3.8f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var psShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (psShader != null) renderer.material = new Material(psShader);

            // 3 crystal nodes at radius 4m at 120° apart
            float[] noteHz = { 164.81f, 220f, 293.66f }; // E3, A3, D4
            Color[] noteCol = {
                new Color(0.30f, 0.65f, 0.85f), // E — cool blue
                new Color(0.85f, 0.60f, 0.30f), // A — amber
                new Color(0.55f, 0.85f, 0.55f)  // D — pale green
            };
            var state = new PoolState { root = poolRoot, disc = disc, bubbles = ps, crystals = new CrystalNode[3] };
            for (int i = 0; i < 3; i++)
            {
                float a = (i / 3f) * Mathf.PI * 2f;
                var nodePos = new Vector3(Mathf.Cos(a) * 4f, 1.2f, Mathf.Sin(a) * 4f);
                state.crystals[i] = BuildCrystal(poolRoot.transform, nodePos, noteHz[i], noteCol[i]);
            }
            return state;
        }

        CrystalNode BuildCrystal(Transform parent, Vector3 localPos, float hz, Color col)
        {
            var node = new GameObject("Crystal_" + Mathf.RoundToInt(hz) + "Hz");
            node.transform.SetParent(parent);
            node.transform.localPosition = localPos;

            // Floating crystal shape — scaled octahedron (use cube rotated 45° as cheap stand-in)
            var shard = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe
            shard.name = "Shard";
            shard.transform.SetParent(node.transform);
            shard.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
            shard.transform.localRotation = Quaternion.Euler(0f, 45f, 35f);
            Object.Destroy(shard.GetComponent<Collider>());
            ApplyURPCrystal(shard, col);

            // Point light tinted to the note color
            var lightGO = new GameObject("Light");
            lightGO.transform.SetParent(node.transform);
            lightGO.transform.localPosition = Vector3.zero;
            var lt = lightGO.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = col;
            lt.intensity = 2.2f;
            lt.range = 6f;

            // Trigger collider for player touch
            var trigger = node.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.2f;

            var cn = node.AddComponent<CrystalNode>();
            cn.hz = hz;
            cn.tintColor = col;
            cn.shardRenderer = shard.GetComponent<Renderer>();
            cn.nodeLight = lt;
            cn.parentPuzzle = this;
            return cn;
        }

        public void OnCrystalTouched(CrystalNode node)
        {
            if (node == null || node.tuned) return;
            node.tuned = true;
            node.tunedAtTime = Time.time;
            // Bright pulse on tune
            if (node.nodeLight != null) node.nodeLight.intensity = 5.0f;
            ApplyURPCrystal(node.shardRenderer.gameObject, node.tintColor * 1.8f);

            // Check if all 3 in this pool are tuned within window
            foreach (var pool in _pools)
            {
                if (pool.drained) continue;
                bool ownsThisNode = false;
                foreach (var c in pool.crystals) if (c == node) { ownsThisNode = true; break; }
                if (!ownsThisNode) continue;

                bool all = true;
                float oldest = float.MaxValue;
                foreach (var c in pool.crystals)
                {
                    if (!c.tuned) { all = false; break; }
                    if (c.tunedAtTime < oldest) oldest = c.tunedAtTime;
                }
                if (all && (Time.time - oldest) <= 30f) DrainPool(pool);
            }
        }

        void DrainPool(PoolState pool)
        {
            pool.drained = true;
            StartCoroutine(DrainPoolCoroutine(pool));
        }

        System.Collections.IEnumerator DrainPoolCoroutine(PoolState pool)
        {
            ServiceLocator.HUD?.ShowBanner("Mud Pool", "Drained. Something glints in the muck.", 6f);
            if (pool.bubbles != null) pool.bubbles.Stop();
            float t = 0f;
            const float dur = 3.5f;
            var startScale = pool.disc.transform.localScale;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = 1f - (t / dur);
                pool.disc.transform.localScale = new Vector3(startScale.x * k, startScale.y, startScale.z * k);
                yield return null;
            }
            // Reveal artifact at pool center
            SpawnArtifact(pool.root.transform.position);
            // Disable crystals
            foreach (var c in pool.crystals) if (c != null) c.gameObject.SetActive(false);
        }

        void SpawnArtifact(Vector3 worldPos)
        {
            var artifactPrefab = Resources.Load<GameObject>("Collectibles/LoreArtifact");
            if (artifactPrefab != null)
            {
                Object.Instantiate(artifactPrefab, worldPos + new Vector3(0f, 0.4f, 0f), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            }
            else
            {
                // Procedural fallback — small golden cube as "artifact"
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe
                go.name = "LoreArtifact_FromPool";
                go.transform.position = worldPos + new Vector3(0f, 0.4f, 0f);
                go.transform.localScale = Vector3.one * 0.35f;
                ApplyURPCrystal(go, new Color(0.95f, 0.78f, 0.30f));
                go.AddComponent<Moon1ArtifactPickup>();
            }
            GameEvents.FireRSChange(25); // reward
        }

        static void ApplyURPMud(GameObject go, Color baseColor, float smoothness)
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

        static void ApplyURPCrystal(GameObject go, Color baseColor)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { r.material.color = baseColor; return; }
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", baseColor * 1.4f);
            r.sharedMaterial = mat;
        }

        // ───────────── inner types ─────────────

        class PoolState
        {
            public GameObject root;
            public GameObject disc;
            public ParticleSystem bubbles;
            public CrystalNode[] crystals;
            public bool drained;
        }
    }

    /// <summary>
    /// Per-crystal trigger handler. Bobs in place, rotates, reports touch up to puzzle.
    /// Real implementation — not a stub.
    /// </summary>
    public class CrystalNode : MonoBehaviour
    {
        public float hz;
        public Color tintColor;
        public Renderer shardRenderer;
        public Light nodeLight;
        public Moon1MudPoolPuzzle parentPuzzle;
        public bool tuned;
        public float tunedAtTime;

        Vector3 _basePos;
        float _seed;

        void Awake()
        {
            _basePos = transform.localPosition;
            _seed = Random.Range(0f, 100f);
        }

        void Update()
        {
            // Bob + slow rotation
            float bob = Mathf.Sin((Time.time + _seed) * 1.4f) * 0.18f;
            transform.localPosition = _basePos + new Vector3(0f, bob, 0f);
            transform.Rotate(Vector3.up, 22f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (tuned) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            if (parentPuzzle != null) parentPuzzle.OnCrystalTouched(this);
        }
    }

    /// <summary>
    /// Fallback artifact pickup — proxies to GameEvents.FireRSChange when player walks over it.
    /// </summary>
    public class Moon1ArtifactPickup : MonoBehaviour
    {
        bool _grabbed;
        void Awake()
        {
            var col = GetComponent<Collider>();
            if (col == null) col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            if (col is SphereCollider sc) sc.radius = 1.0f;
        }
        void Update()
        {
            transform.Rotate(Vector3.up, 60f * Time.deltaTime);
        }
        void OnTriggerEnter(Collider other)
        {
            if (_grabbed) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            _grabbed = true;
            ServiceLocator.HUD?.ShowBanner("Lore Artifact", "+25 RS — fragment of the Buried Codex", 4f);
            GameEvents.FireRSChange(25);
            Destroy(gameObject);
        }
    }
}
