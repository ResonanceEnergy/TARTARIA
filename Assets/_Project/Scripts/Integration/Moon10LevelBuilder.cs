using UnityEngine;

namespace Tartaria.Integration
    #pragma warning disable CS0414, CS0219 // Placeholder fields/vars for planned features
{
    /// <summary>
    /// Moon 10 Level Builder — The Temporal Rift
    /// Time-warped ruins with past/present/future overlays
    /// Theme: Time manipulation, paradox puzzles, causality
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon10LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 riftCenter = Vector3.zero;
        [SerializeField] float riftSize = 110f;

        void Start()
        {
            BuildRift();
        }

        void BuildRift()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 10: THE TEMPORAL RIFT — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon10_TemporalRift");

            // Central time vortex
            var vortex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vortex.name = "Time_Vortex";
            vortex.transform.SetParent(parent.transform);
            vortex.transform.position = new Vector3(0f, 15f, 0f);
            vortex.transform.localScale = new Vector3(30f, 30f, 30f);

            // 3 Time layers (past, present, future) - concentric rings
            CreateTimeLayer(parent, 25f, "Past", new Vector3(0f, 0f, 0f));
            CreateTimeLayer(parent, 50f, "Present", new Vector3(0f, 5f, 0f));
            CreateTimeLayer(parent, 75f, "Future", new Vector3(0f, 10f, 0f));

            // Temporal anchors (8)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(55f, 8f, 0f);
                var anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                anchor.name = "Temporal_Anchor";
                anchor.transform.SetParent(parent.transform);
                anchor.transform.position = pos;
                anchor.transform.localScale = new Vector3(5f, 16f, 5f);
                anchor.transform.Rotate(0f, angle, 0f);
            }

            // Floating time shards
            for (int i = 0; i < 25; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "Time_Shard";
                shard.transform.SetParent(parent.transform);
                shard.transform.position = new Vector3(Random.Range(-70f, 70f), Random.Range(5f, 25f), Random.Range(-70f, 70f));
                shard.transform.localScale = new Vector3(Random.Range(1f, 3f), Random.Range(1f, 3f), Random.Range(1f, 3f));
                shard.transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                Destroy(shard.GetComponent<Collider>());
            }

            Debug.Log("[Moon10LevelBuilder] ✅ Temporal Rift complete!");
            Debug.Log("  • Time vortex + 3 layers (past/present/future) + 8 anchors + 25 shards");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateTimeLayer(GameObject parent, float radius, string layerName, Vector3 offset)
        {
            var layer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            layer.name = $"Time_Layer_{layerName}";
            layer.transform.SetParent(parent.transform);
            layer.transform.position = offset;
            layer.transform.localScale = new Vector3(radius * 2f, 0.5f, radius * 2f);
        }
    }
}
