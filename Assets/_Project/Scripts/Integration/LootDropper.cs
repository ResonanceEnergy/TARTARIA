using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-11: spawns runtime loot pickups when enemies die.
    /// No prefab required — builds a glowing cube + PickupInteractable on the fly.
    /// Items rotate through a small drop table by deterministic count.
    /// Optional VFX: Set ShardCollectVFX prefab for vacuum effect on spawn.
    /// </summary>
    public static class LootDropper
    {
        static int _dropCount;

        /// <summary>
        /// Optional ShardCollect VFX prefab for loot spawn effect.
        /// Assign via LootDropper.ShardCollectVFX = yourPrefab in scene setup.
        /// </summary>
        public static GameObject ShardCollectVFX { get; set; }

        struct Drop { public string id; public string display; public Color color; }
        static readonly Drop[] Table =
        {
            new() { id = "aether_shard",     display = "Aether Shard",     color = new Color(0.45f, 0.85f, 1.0f) },
            new() { id = "golem_core",       display = "Golem Core",       color = new Color(0.95f, 0.6f, 0.25f) },
            new() { id = "resonance_crystal",display = "Resonance Crystal",color = new Color(0.85f, 0.4f, 1.0f) },
        };

        public static void Spawn(Vector3 position)
        {
            var pick = Table[_dropCount++ % Table.Length];

            // Build loot cube from components (no CreatePrimitive per primitive elimination mandate).
            var go = new GameObject($"Loot_{pick.id}");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.35f;
            
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            go.AddComponent<MeshRenderer>();
            go.AddComponent<BoxCollider>();
            // Make collider a trigger so the player can walk through; PickupInteractable uses E.
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(sh) { color = pick.color };
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", pick.color * 2.5f);
                }
                rend.material = mat;
            }

            // Layer: try to put on Interactable layer (9) if it exists, otherwise leave Default.
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            // Wobble + slow spin so loot reads at a glance.
            go.AddComponent<LootHover>();

            var p = go.AddComponent<PickupInteractable>();
            // Configure via reflection (private serialized fields).
            var t = typeof(PickupInteractable);
            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            t.GetField("itemId",      bf)?.SetValue(p, pick.id);
            t.GetField("displayName", bf)?.SetValue(p, pick.display);
            t.GetField("quantity",    bf)?.SetValue(p, 1);

            // Spawn ShardCollect VFX if assigned (vacuum effect toward position)
            if (ShardCollectVFX != null)
            {
                GameObject vfx = Object.Instantiate(ShardCollectVFX, position, Quaternion.identity);
                
                // Match VFX color to loot rarity
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = pick.color;
                }
                
                Object.Destroy(vfx, 2f);
            }

            // Auto-cleanup if never picked up.
            Object.Destroy(go, 60f);
        }
    }

    /// <summary>Visual flair for loot cubes — float and spin until collected.</summary>
    public class LootHover : MonoBehaviour
    {
        Vector3 _origin;
        float _phase;

        void Start()
        {
            _origin = transform.position;
            _phase = Random.value * 6.28f;
        }

        void Update()
        {
            transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
            transform.position = _origin + Vector3.up * (0.25f + 0.1f * Mathf.Sin(Time.time * 2.5f + _phase));
        }
    }
}
