using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Level Builder — The Aether Convergence
    /// Final level - epicenter of all aether energy, all previous mechanics converge
    /// Theme: Ultimate test, cosmic architecture, reality itself
    /// Largest and most complex level - combines elements from all 12 previous moons
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon13LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 convergenceCenter = Vector3.zero;
        [SerializeField] float convergenceSize = 200f; // Largest level

        const float PHI = 1.618033988749895f;

        void Start()
        {
            BuildConvergence();
        }

        void BuildConvergence()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 13: THE AETHER CONVERGENCE — BUILDING");
            Debug.Log("    FINAL LEVEL - ALL MECHANICS CONVERGE");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon13_AetherConvergence");

            // Central Aether Core - massive sphere
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Aether_Core";
            core.transform.SetParent(parent.transform);
            core.transform.position = new Vector3(0f, 50f, 0f);
            core.transform.localScale = new Vector3(60f, 60f, 60f);

            // 12 Tribute platforms (one for each previous moon) in dodecahedron arrangement
            Create12TributePlatforms(parent);

            // Golden spiral of ascending platforms
            CreateSpiralPath(parent, 50);

            // 3 Concentric rings of pillars (past, present, future)
            CreatePillarRing(parent, 60f, 24, "Inner Ring");
            CreatePillarRing(parent, 100f, 36, "Middle Ring");
            CreatePillarRing(parent, 140f, 48, "Outer Ring");

            // Final altar at the peak
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "Final_Altar";
            altar.transform.SetParent(parent.transform);
            altar.transform.position = new Vector3(0f, 100f, 0f);
            altar.transform.localScale = new Vector3(20f, 5f, 20f);

            // Energy conduits connecting all 12 platforms to core
            CreateEnergyConduits(parent);

            // Floating aether shards (100)
            for (int i = 0; i < 100; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "Aether_Shard";
                shard.transform.SetParent(parent.transform);
                float angle = i * 3.6f;
                float radius = Random.Range(30f, 150f);
                float height = Random.Range(10f, 90f);
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, height, 0f);
                shard.transform.position = pos;
                shard.transform.localScale = Vector3.one * Random.Range(1.5f, 4f);
                shard.transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                Destroy(shard.GetComponent<Collider>());
            }

            Debug.Log("[Moon13LevelBuilder] ✅ Aether Convergence complete!");
            Debug.Log("  • Aether Core (60m sphere) at 50m height");
            Debug.Log("  • 12 Tribute platforms (dodecahedron arrangement)");
            Debug.Log("  • Golden spiral path (50 steps)");
            Debug.Log("  • 3 Concentric pillar rings (24 + 36 + 48 = 108 pillars)");
            Debug.Log("  • Final Altar at 100m height");
            Debug.Log("  • 12 Energy conduits");
            Debug.Log("  • 100 Floating aether shards");
            Debug.Log("  • TOTAL SCALE: 200m radius, 100m height");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void Create12TributePlatforms(GameObject parent)
        {
            // Dodecahedron vertex positions (12 vertices)
            float phi_inv = 1f / PHI;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(1, 1, 1).normalized * 90f,
                new Vector3(1, 1, -1).normalized * 90f,
                new Vector3(1, -1, 1).normalized * 90f,
                new Vector3(1, -1, -1).normalized * 90f,
                new Vector3(-1, 1, 1).normalized * 90f,
                new Vector3(-1, 1, -1).normalized * 90f,
                new Vector3(-1, -1, 1).normalized * 90f,
                new Vector3(-1, -1, -1).normalized * 90f,
                new Vector3(0, phi_inv, PHI).normalized * 90f,
                new Vector3(0, phi_inv, -PHI).normalized * 90f,
                new Vector3(0, -phi_inv, PHI).normalized * 90f,
                new Vector3(0, -phi_inv, -PHI).normalized * 90f
            };

            string[] moonNames = {
                "Echohaven", "ResonantCaverns", "VerdantLabyrinth", "SunscorchedOasis",
                "FrostboundCitadel", "MoltenForge", "AbyssalDepths", "CelestialSpires",
                "BlightedWastes", "TemporalRift", "PrismaticNexus", "UmbralSanctum"
            };

            for (int i = 0; i < 12; i++)
            {
                var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                platform.name = $"Tribute_Platform_Moon{i + 1}_{moonNames[i]}";
                platform.transform.SetParent(parent.transform);
                platform.transform.position = positions[i] + new Vector3(0f, 30f, 0f);
                platform.transform.localScale = new Vector3(15f, 2f, 15f);

                // Memorial obelisk on each platform
                var obelisk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obelisk.name = "Memorial_Obelisk";
                obelisk.transform.SetParent(platform.transform);
                obelisk.transform.localPosition = new Vector3(0f, 6f, 0f);
                obelisk.transform.localScale = new Vector3(2f, 12f, 2f);
            }
        }

        void CreateSpiralPath(GameObject parent, int steps)
        {
            var spiralParent = new GameObject("Golden_Spiral_Path");
            spiralParent.transform.SetParent(parent.transform);

            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                float angle = t * Mathf.PI * 8f; // 4 full rotations
                float radius = 20f + (t * 70f); // Expanding spiral
                float height = t * 80f; // Rising from 0 to 80m

                Vector3 pos = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) * new Vector3(radius, height, 0f);

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Spiral_Step_{i:D2}";
                step.transform.SetParent(spiralParent.transform);
                step.transform.position = pos;
                step.transform.localScale = new Vector3(6f, 1f, 6f);
                step.transform.LookAt(parent.transform.position + Vector3.up * height);
            }
        }

        void CreatePillarRing(GameObject parent, float radius, int count, string ringName)
        {
            var ring = new GameObject(ringName);
            ring.transform.SetParent(parent.transform);

            for (int i = 0; i < count; i++)
            {
                float angle = i * (360f / count);
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, 0f, 0f);

                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Pillar";
                pillar.transform.SetParent(ring.transform);
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(4f, 30f, 4f);
            }
        }

        void CreateEnergyConduits(GameObject parent)
        {
            var conduitParent = new GameObject("Energy_Conduits");
            conduitParent.transform.SetParent(parent.transform);

            // Connect each tribute platform to the core
            var platforms = parent.GetComponentsInChildren<Transform>();
            foreach (var platform in platforms)
            {
                if (platform.name.StartsWith("Tribute_Platform"))
                {
                    var conduit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    conduit.name = "Energy_Conduit";
                    conduit.transform.SetParent(conduitParent.transform);

                    Vector3 start = platform.position;
                    Vector3 end = new Vector3(0f, 50f, 0f); // Aether Core position

                    conduit.transform.position = (start + end) / 2f;
                    float length = Vector3.Distance(start, end);
                    conduit.transform.localScale = new Vector3(0.5f, length / 2f, 0.5f);
                    conduit.transform.LookAt(end);
                    conduit.transform.Rotate(90f, 0f, 0f);

                    Destroy(conduit.GetComponent<Collider>());
                }
            }
        }
    }
}
