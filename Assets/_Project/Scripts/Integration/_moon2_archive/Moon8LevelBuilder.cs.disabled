using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Level Builder — The Celestial Spires
    /// Floating islands with sky temples and wind currents
    /// Theme: Aerial navigation, gravity manipulation, windwalking
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon8LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 skyCenter = new Vector3(0f, 100f, 0f);
        [SerializeField] float islandSpread = 120f;

        void Start()
        {
            BuildSkyTemple();
        }

        void BuildSkyTemple()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 8: THE CELESTIAL SPIRES — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon8_CelestialSpires");
            parent.transform.position = skyCenter;

            // Central spire island
            CreateIsland(parent, Vector3.zero, 40f, "Central_Spire");
            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "Main_Spire";
            spire.transform.SetParent(parent.transform);
            spire.transform.localPosition = new Vector3(0f, 20f, 0f);
            spire.transform.localScale = new Vector3(8f, 40f, 8f);

            // 6 Floating islands in hexagon
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(islandSpread, Random.Range(-20f, 20f), 0f);
                CreateIsland(parent, offset, Random.Range(20f, 35f), $"Island_{i + 1}");

                // Small temple on each island
                var temple = GameObject.CreatePrimitive(PrimitiveType.Cube);
                temple.name = "Sky_Temple";
                temple.transform.SetParent(parent.transform);
                temple.transform.localPosition = offset + Vector3.up * 5f;
                temple.transform.localScale = new Vector3(12f, 10f, 12f);
            }

            // Wind bridges (visual only)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 start = Quaternion.Euler(0f, angle, 0f) * new Vector3(20f, 0f, 0f);
                Vector3 end = Quaternion.Euler(0f, angle, 0f) * new Vector3(islandSpread - 20f, 0f, 0f);
                CreateBridge(parent, start, end);
            }

            Debug.Log("[Moon8LevelBuilder] ✅ Celestial Spires complete!");
            Debug.Log("  • Central spire + 6 floating islands + 6 wind bridges");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateIsland(GameObject parent, Vector3 localPos, float size, string name)
        {
            var island = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            island.name = name;
            island.transform.SetParent(parent.transform);
            island.transform.localPosition = localPos;
            island.transform.localScale = new Vector3(size, size * 0.5f, size);
        }

        void CreateBridge(GameObject parent, Vector3 start, Vector3 end)
        {
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = "Wind_Bridge";
            bridge.transform.SetParent(parent.transform);
            bridge.transform.localPosition = (start + end) / 2f;
            float length = Vector3.Distance(start, end);
            bridge.transform.localScale = new Vector3(2f, 0.5f, length);
            bridge.transform.LookAt(parent.transform.TransformPoint(end));
        }
    }
}
